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
using System.Drawing.Imaging;

using System.Runtime.InteropServices;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 期刊图像界面对象：第一层次，期对象
    /// </summary>
    internal class IssueBindingItem : CellBase
    {
        /// <summary>
        /// 容器，期刊图形界面控件
        /// </summary>
        public BindingControl Container = null;

        // 显示单元的数组
        /// <summary>
        /// 单元格集合
        /// </summary>
        public List<Cell> Cells = new List<Cell>();

        /// <summary>
        /// 布局模式
        /// </summary>
        public IssueLayoutState IssueLayoutState = IssueLayoutState.Binding;  // Binding

        /// <summary>
        /// 用于存放需要连接的任意类型对象
        /// </summary>
        public object Tag = null;   // 

        // public string Xml = ""; // 
        // string m_strXml = "";

        /// <summary>
        /// 获得期记录的 XML 字符串
        /// </summary>
        public string Xml
        {
            get
            {
                if (dom != null)
                    return dom.OuterXml;

                return "";

                // return m_strXml;
            }
            /*
            set
            {
                m_strXml = value;
            }
             * */
        }
        internal XmlDocument dom = null;

        internal bool Virtual = false;  // 是否为虚拟的期？== true 是虚拟的。所谓虚拟的，就是根据实有的册的publishtime临时创建的期对象，而不是真实存在于数据库中的期对象

        // 册信息的数组
        // 注意: 当Cells安放好以后，这里就要清空?
        internal List<ItemBindingItem> Items = new List<ItemBindingItem>();   // 下属的册

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;

        /// <summary>
        /// 是否为新创建的对象
        /// </summary>
        public bool NewCreated = false;

        /// <summary>
        /// 订购的份数。从订购XML中获得的。-1表示未知
        /// </summary>
        public int OrderedCount = -1;    // 订购的份数。从订购XML中获得的。-1表示未知

        bool OrderInfoLoaded = false;   // 采购信息是否从外部装载过。避免重复装载

        // 下属的采购信息对象的数组
        internal List<OrderBindingItem> OrderItems = new List<OrderBindingItem>();

        /// <summary>
        /// 获得本对象显示的像素宽度
        /// </summary>
        public override int Width
        {
            get
            {
                return this.Container.m_nLeftTextWidth + (Container.m_nCellWidth * this.Cells.Count);
            }
        }

        /// <summary>
        /// 获得本对象显示像素高度
        /// </summary>
        public override int Height
        {
            get
            {
                return this.Container.m_nCellHeight;
            }
        }

        /// <summary>
        /// 左侧文字部分的像素宽度
        /// </summary>
        public int LeftTextWidth
        {
            get
            {
                return this.Container.m_nLeftTextWidth;
            }
        }

        // 用于显示的期名。例如：“2008年第1期 (总.100 v.10)”
        /// <summary>
        /// 用于显示的期名。例如：“2008年第1期 (总.100 v.10)”
        /// </summary>
        public string Caption
        {
            get
            {
                if (String.IsNullOrEmpty(this.PublishTime) == true)
                    return "自由";

                string strZongAndVolume = "";

                if (String.IsNullOrEmpty(this.Volume) == false)
                    strZongAndVolume += "v." + this.Volume;
                if (String.IsNullOrEmpty(this.Zong) == false)
                {
                    if (String.IsNullOrEmpty(strZongAndVolume) == false)
                        strZongAndVolume += " ";
                    strZongAndVolume += "总." + this.Zong;
                }

                string strYear = IssueUtil.GetYearPart(this.PublishTime);
                return strYear + "年第" + this.Issue + "期"
                    + (String.IsNullOrEmpty(strZongAndVolume) == false ?
                        "(" + strZongAndVolume + ")" : "");
            }

        }

        /// <summary>
        /// 清除下属的 OrderItems 集合内容
        /// </summary>
        public void ClearOrderItems()
        {
            this.OrderItems.Clear();
        }

        // 当前期是否可以删除?
        internal bool CanDelete(out string strMessage)
        {
            strMessage = "";
            this.RemoveTailNullCell();
            if (this.Cells.Count == 0)
                return true;

            int nLockedCellCount = 0;
            int nCellCount = 0;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    continue;
                if (cell.item == null)
                    continue;

                if (cell.item.Deleted == true)
                    continue;

                if (cell.item.Calculated == true && cell.item.Locked == true)
                {
                    nLockedCellCount++;
                }

                if (cell.item.Calculated == true)
                    continue;

                // 如果有合订册，是否允许删除？如果删除以后，重新装载时候能重建，可以允许删除
                nCellCount ++;
            }

            if (nLockedCellCount > 0)
                strMessage += "含有 " + nLockedCellCount.ToString() + " 个锁定状态的册格子";
            if (nCellCount > 0)
            {
                if (string.IsNullOrEmpty(strMessage) == false)
                    strMessage += ",";
                strMessage += "含有 " + nCellCount.ToString() + " 个已到册格子";
            }

            if (string.IsNullOrEmpty(strMessage) == false)
                return false;

            return true;
        }

        /// <summary>
        /// 通过 index 获得一个单元对象
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>单元对象</returns>
        public Cell GetCell(int index)
        {
            if (this.Cells.Count <= index)
                return null;

            return this.Cells[index];
        }

        /// <summary>
        /// 设置一个单元对象
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="cell">单元对象</param>
        public void SetCell(int index, Cell cell)
        {
            // 确保数组规则足够
            while (this.Cells.Count <= index)
                this.Cells.Add(null);

            Cell old_cell = this.Cells[index];
            if (old_cell != null && old_cell != cell)
            {
                if (this.Container.FocusObject == old_cell)
                    this.Container.FocusObject = null;
                if (this.Container.HoverObject == old_cell)
                    this.Container.HoverObject = null;
            }

            this.Cells[index] = cell;
            if (cell != null)
            {
                cell.Container = this;
                /*
                // 2010/3/3
                if (cell.item != null)
                    cell.item.Container = this;
                 * */
            }

            // TODO: 被丢弃的Cell的Container是否要设置为null? 不过这样会影响到刷新
        }

        /// <summary>
        /// 追加一个单元对象
        /// </summary>
        /// <param name="cell">单元对象</param>
        public void AddCell(Cell cell)
        {
            this.Cells.Add(cell);
            cell.Container = this;
        }

        /// <summary>
        /// 插入一个单元对象
        /// </summary>
        /// <param name="nPos">要插入的位置下标</param>
        /// <param name="cell">要插入的单元对象</param>
        public void InsertCell(int nPos, Cell cell)
        {
            this.Cells.Insert(nPos, cell);
            cell.Container = this;
        }

        // 删除一个Cell。
        // 注意，如果要设置为空白Cell，不能使用本函数
        /// <summary>
        /// 通过指定 ItemBindingItem 对象来删除一个单元格
        /// </summary>
        /// <param name="item">ItemBindingItem 对象</param>
        public void RemoveCell(ItemBindingItem item)
        {
            int index = IndexOfItem(item);
            if (index == -1)
                return;
            this.Cells.RemoveAt(index);
        }

        /*
        // 设置为空白Cell
        // parameters:
        //      parent_item 新设置的Cell的从属合订本。如果为null表示不再是合订格子
        public void SetCellBlank(ItemBindingItem item,
            ItemBindingItem parent_item)
        {
            int index = IndexOfItem(item);
            if (index == -1)
                return;
            Cell cell = this.Cells[index];

            if (cell.item != null)
            {
                cell.ParentItem = parent_item;
                cell.item = null;
            }
        }
         * */

        /// <summary>
        /// 获得单元对象的下标
        /// </summary>
        /// <param name="cell">单元对象</param>
        /// <returns>下标</returns>
        public int IndexOfCell(Cell cell)
        {
            return this.Cells.IndexOf(cell);
        }

        // 定位在Cells里面的下标
        /// <summary>
        /// 通过 ItemBindingItem 对象定位单元的下标
        /// </summary>
        /// <param name="item">ItemBindingItem对象</param>
        /// <returns>下标</returns>
        public int IndexOfItem(ItemBindingItem item)
        {
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    continue;
                if (cell.item == item)
                    return i;
            }

            return -1;
        }

        // 删除末尾的null单元
        /// <summary>
        /// 删除末尾的null单元
        /// </summary>
        /// <returns>是否发生了删除</returns>
        public bool RemoveTailNullCell()
        {
            bool bChanged = false;
            for (int i = this.Cells.Count - 1; i >= 0; i--)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                {
                    this.Cells.RemoveAt(i);
                    bChanged = true;
                }
                else
                    break;
            }

            return bChanged;
        }

        // 是否包含成员册或合订本格子?
        /// <summary>
        /// 是否包含成员册或合订本格子?
        /// </summary>
        /// <returns>是或者否</returns>
        public bool HasMemberOrParentCell()
        {
            if (this.IssueLayoutState == dp2Circulation.IssueLayoutState.Binding)
            {
                for (int i = 0; i < this.Cells.Count; i++)
                {
                    int nRet = this.IsBoundIndex(i);
                    if (nRet != 0)
                        return true;
                }
            }
            else
            {
                // 2012/9/29
                for (int i = 0; i < this.Cells.Count; i++)
                {
                    Cell cell = this.Cells[i];
                    if (cell == null)
                        continue;
                    if (cell.IsMember == true)
                        return true;
                    if (cell.item != null &&  cell.item.IsParent == true)
                        return true;
                }
            }

            return false;
        }

        public void AfterMembersChanged()
        {
            bool bChanged = false;
            if (RefreshOrderInfoXml() == true)
                bChanged = true;

            if (bChanged == true)
            {
                this.Changed = true;

                // 如果期格子中所显示的数量文字发生变化的话
                /*
                try
                {
                    this.Container.UpdateObject(this);
                }
                catch
                {
                }
                 * */
            }
        }

        // MemberCells修改后，要刷新binding XML片断
        // 可能会抛出异常
        // return:
        //      false   binding XML片断没有发生修改
        //      true    binding XML片断发生了修改
        public bool RefreshOrderInfoXml()
        {
            if (this.OrderItems.Count == 0)
            {
                if (this.OrderInfo == "")
                    return false;
                this.OrderInfo = "";
                return true;
            }

            // 创建<orderInfo>元素内片断
            string strInnerXml = "";
            string strError = "";
            int nRet = BuildOrderInfoXmlString(this.OrderItems,
                out strInnerXml,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            if (this.OrderInfo == strInnerXml)
                return false;

            this.OrderInfo = strInnerXml;
            return true;
        }

        // 创建<orderInfo>元素内片断
        // 要创建若干<root>元素
        public static int BuildOrderInfoXmlString(List<OrderBindingItem> orders,
            out string strInnerXml,
            out string strError)
        {
            strInnerXml = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<orderInfo />");

            for (int i = 0; i < orders.Count; i++)
            {
                OrderBindingItem order = orders[i];
                if (order == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                XmlNode node = dom.CreateElement("root");   // item?
                dom.DocumentElement.AppendChild(node);

                node.InnerXml = order.dom.DocumentElement.InnerXml;
            }

            strInnerXml = dom.DocumentElement.InnerXml;
            return 0;
        }

        // 2012/5/16
        // 在订购信息数组中定位符合四元组信息的Cell对象
        static List<OrderBindingItem> LocateInOrders(Cell cell,
            List<OrderBindingItem> orders)
        {
            List<OrderBindingItem> results = new List<OrderBindingItem>();
            if (cell.item == null)
                return results;

            for (int i = 0; i < orders.Count; i++)
            {
                OrderBindingItem order = orders[i];

                // 检查四元组
                if (cell.item.Seller != order.Seller)
                    continue;
                if (cell.item.Source != order.Source)
                    continue;
                if (cell.item.Price != order.Price)
                    continue;
                // 检查一下这个出版时间是否超过订购时间范围?
                if (Global.InRange(cell.item.PublishTime,
                    order.Range) == false)
                    continue;

                results.Add(order);
            }

            return results;
        }

        // 2012/5/16
        // 从数组中找到符合订购四元组信息的Cell对象
        // 如果有多个匹配，则返回null
        static Cell FindGroupMemberCell(List<Cell> cells,
            OrderBindingItem order,
            string strLocationName)
        {
            List<Cell> results = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;
                if (cell.item == null)
                    continue;

                if (cell.item.LocationString != strLocationName)
                    continue;

                // 检查四元组
                if (cell.item.Seller != order.Seller)
                    continue;
                if (cell.item.Source != order.Source)
                    continue;
                if (cell.item.Price != order.Price)
                    continue;
                // 检查一下这个出版时间是否超过订购时间范围?
                if (Global.InRange(cell.item.PublishTime,
                    order.Range) == false)
                    continue;

                results.Add(cell);
            }

            if (results.Count == 1)
                return results[0];

            return null;
        }

        // 从数组中找到具有指定 x, y订购连接的Cell对象
        static Cell FindGroupMemberCell(List<Cell> cells,
            int x, int y)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;
                if (cell.item == null)
                    continue;
                if (cell.item.OrderInfoPosition.X == x
                    && cell.item.OrderInfoPosition.Y == y)
                    return cell;
            }

            return null;
        }

        public static int GetNumberValue(string strNumber)
        {
            try
            {
                return Convert.ToInt32(strNumber);
            }
            catch
            {
                return 0;
            }
        }

        // 修改新旧值字符串中的新字符串部分
        public static string ChangeNewValue(string strExistString,
            string strNewValue)
        {
            string strTempNewValue = "";
            string strOldValue = "";
            OrderDesignControl.ParseOldNewValue(strExistString,
                out strOldValue,
                out strTempNewValue);
            return OrderDesignControl.LinkOldNewValue(strOldValue,
                strNewValue);
        }

        // 从新旧值字符串中获得新值部分
        public static string GetNewValue(string strValue)
        {
            string strNewValue = "";
            string strOldValue = "";
            OrderDesignControl.ParseOldNewValue(strValue,
                out strOldValue,
                out strNewValue);
            return strNewValue;
        }


        // 从新旧值字符串中顺次获得新值部分，优先获得新值
        public static string GetNewOrOldValue(string strValue)
        {
            string strNewValue = "";
            string strOldValue = "";
            OrderDesignControl.ParseOldNewValue(strValue,
                out strOldValue,
                out strNewValue);
            if (String.IsNullOrEmpty(strNewValue) == false)
                return strNewValue;
            return strOldValue;
        }

        // 从新旧值字符串中顺次获得新值部分，优先获得旧值
        public static string GetOldOrNewValue(string strValue)
        {
            string strNewValue = "";
            string strOldValue = "";
            OrderDesignControl.ParseOldNewValue(strValue,
                out strOldValue,
                out strNewValue);
            if (String.IsNullOrEmpty(strOldValue) == false)
                return strOldValue;
            return strNewValue;
        }

        // 从新旧值字符串中获得旧值部分
        public static string GetOldValue(string strValue)
        {
            string strNewValue = "";
            string strOldValue = "";
            OrderDesignControl.ParseOldNewValue(strValue,
                out strOldValue,
                out strNewValue);
            return strOldValue;
        }

        internal static void SetFieldValueFromOrderInfo(
            bool bForce,    // 是否强行设置
            ItemBindingItem item,
            OrderBindingItem order)
        {
            if (bForce == true || String.IsNullOrEmpty(item.Source) == true)
                item.Source = GetNewOrOldValue(order.Source);

            if (bForce == true || String.IsNullOrEmpty(item.Seller) == true)
                item.Seller = GetNewOrOldValue(order.Seller);

            if (bForce == true || String.IsNullOrEmpty(item.Price) == true)
            {
                // TODO: 如果订购记录中的 price 为空，则需要从 totalPrice 中计算出来
                string strPrice = GetNewOrOldValue(order.Price);
                if (string.IsNullOrEmpty(strPrice) == false)
                    item.Price = strPrice;
                else
                {
                    // 2015/4/1
                    item.Price = CalcuPrice(order.TotalPrice, order.IssueCount, GetOldOrNewValue(order.Copy));
                }

                // item.Price = GetNewOrOldValue(order.Price);
            }
        }

        // 根据总价和期数、复本数计算出单价
        internal static string CalcuPrice(string strTotalPrice,
            string strIssueCount,
            string strCopy)
        {
            long count = 0;
            if (long.TryParse(strIssueCount, out count) == false)
            {
                return "订购信息中期数 '" + strIssueCount + "' 格式错误";
            }

            long copy = 0;
            if (long.TryParse(strCopy, out copy) == false)
            {
                return "订购信息中复本数 '" + strCopy + "' 格式错误";
            }

            return strTotalPrice + "/" + (count * copy).ToString();
        }

        // 根据格子index获得组(头部)对象
        // 无论是组格子还是其他格子位置，都可以使用本函数
        // 本函数对index位置的格子本身的状态并无要求，只是按照位置关系来判断
        internal GroupCell BelongToGroup(int index)
        {
            GroupCell group = null;
            // int n = -1;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    goto CONTINUE;
                if (cell is GroupCell)
                {
                    GroupCell current_group = (GroupCell)cell;
                    if (current_group.EndBracket == false)
                    {
                        // n++;
                        group = current_group;
                    }
                    else if (current_group.EndBracket == true)
                    {
                        if (index == i)
                            return group;
                        group = null;
                    }
                }
            CONTINUE:
                if (index == i)
                    return group;
            }

            return null;
        }

        // 根据组序号获得组(头部)对象
        internal GroupCell GetGroupCellHead(int group_index)
        {
            int n = 0;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    continue;
                if (cell is GroupCell)
                {
                    GroupCell group = (GroupCell)cell;
                    if (group.EndBracket == false)
                    {
                        if (n == group_index)
                            return group;
                        n++;
                    }
                }
            }

            return null;
        }

        // 根据组序号获得组(尾部)对象
        internal GroupCell GetGroupCellTail(int group_index)
        {
            int n = 0;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    continue;
                if (cell is GroupCell)
                {
                    GroupCell group = (GroupCell)cell;
                    if (group.EndBracket == true)
                    {
                        if (n == group_index)
                            return group;
                        n++;
                    }
                }
            }

            return null;
        }

        // 获得可见的订购组中的refid
        public int GetVisibleRefIDs(
            string strLibraryCodeList,
            out List<string> refids,
            out string strError)
        {
            strError = "";
            refids = new List<string>();

            // TODO: 可以为orderitem打上是否可见的标记，就可以加速判断
            // 另外是否可以提前删除不可见的orderitem对象?
            foreach (OrderBindingItem order in this.OrderItems)
            {
                // 观察一个馆藏分配字符串，看看是否部分在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   没有任何部分在管辖范围
                //      1   至少部分在管辖范围内
                int nRet = Global.DistributeCross(order.Distribute,
                    strLibraryCodeList,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    continue;

                List<string> temp = null;
                nRet = Global.GetRefIDs(order.Distribute,
                    out temp,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (temp != null && temp.Count > 0)
                    refids.AddRange(temp);
            }

            return 0;
        }

        // 2012/9/21
        public int InitialOrderItems(out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count == 0
                && this.OrderInfoLoaded == false)
            {
                // 需要从外部获得采购信息
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = this.PublishTime;
                if (this.Container.m_bHideLockedOrderGroup == true)
                    e1.LibraryCodeList = this.Container.LibraryCodeList;
                this.Container.DoGetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + this.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    return -1;
                }

                if (e1.OrderXmls.Count == 0)
                {
                    this.OrderInfoLoaded = true;
                }
                else
                {
                    XmlNode root = this.dom.DocumentElement.SelectSingleNode("orderInfo");
                    if (root == null)
                    {
                        root = this.dom.CreateElement("orderInfo");
                        this.dom.DocumentElement.AppendChild(root);
                    }
                    for (int i = 0; i < e1.OrderXmls.Count; i++)
                    {
                        XmlDocument whole_dom = new XmlDocument();
                        try
                        {
                            whole_dom.LoadXml(e1.OrderXmls[i]);
                        }
                        catch (Exception ex)
                        {
                            strError = "订购记录第 " + i.ToString() + " 个XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }
                        XmlNode node = this.dom.CreateElement("root");
                        root.AppendChild(node);
                        node.InnerXml = whole_dom.DocumentElement.InnerXml;
                    }
                    nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                OrderBindingItem order = new OrderBindingItem();
                nRet = order.Initial(node.OuterXml, out strError);
                if (nRet == -1)
                    return -1;

#if NO
                if (this.Container.HideLockedOrderGroup == true)
                {
                    // 观察一个馆藏分配字符串，看看是否部分在当前用户管辖范围内
                    // return:
                    //      -1  出错
                    //      0   没有任何部分在管辖范围
                    //      1   至少部分在管辖范围内
                    nRet = Global.DistributeCross(order.Distribute,
                        this.Container.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        List<string> refids = null;
                        // 获得一个馆藏分配字符串里面的所有refid
                        nRet = Global.GetRefIDs(order.Distribute,
            out refids,
            out strError);
                        if (nRet == -1)
                            return -1;
                        foreach (string refid in refids)
                        {
                            Cell cell = FindCellByRefID(refid, already_placed_cells);
                            if (cell != null && cell.IsMember == false)
                            {
                                already_placed_cells.Remove(cell);
                                if (bAddGroupCells == false)
                                    index = already_placed_cells.Count; // 使得排列紧密
                            }
                        }
                        foreach (string refid in refids)
                        {
                            Cell cell = FindCellByRefID(refid, exist_cells);
                            if (cell != null && cell.IsMember == false)
                                exist_cells.Remove(cell);
                        }
                        continue;
                    }
                }
#endif

                this.OrderItems.Add(order);
            }

            this.OrderInfoLoaded = true;
            return 0;
        }


        // 根据订购信息安放格子
        // TODO: 如果在已经安放的数组中发现，则仅仅添加采购信息连接而已，不再重新安放
        // parameters:
        int PlaceCellsByOrderInfo(
            bool bAddGroupCells,
            List<Cell> already_placed_cells,
            ref List<Cell> exist_cells,
            ref int index,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 然后安放单册

            // 根据Xml中<orderInfo>元素内容，计算出还没有到达的预测册，追加在右边
            // TODO: 将来是否从一个恒定的位置开始追加? 
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count == 0
                && this.OrderInfoLoaded == false)
            {
                // 需要从外部获得采购信息
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = this.PublishTime;
                if (this.Container.m_bHideLockedOrderGroup == true)
                    e1.LibraryCodeList = this.Container.LibraryCodeList;
                this.Container.DoGetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + this.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    return -1;
                }

                if (e1.OrderXmls.Count == 0)
                {
                    this.OrderInfoLoaded = true;
                }
                else
                {
                    XmlNode root = this.dom.DocumentElement.SelectSingleNode("orderInfo");
                    if (root == null)
                    {
                        root = this.dom.CreateElement("orderInfo");
                        this.dom.DocumentElement.AppendChild(root);
                    }
                    for (int i = 0; i < e1.OrderXmls.Count; i++)
                    {
                        XmlDocument whole_dom = new XmlDocument();
                        try
                        {
                            whole_dom.LoadXml(e1.OrderXmls[i]);
                        }
                        catch (Exception ex)
                        {
                            strError = "订购记录第 " + i.ToString() + " 个XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }
                        XmlNode node = this.dom.CreateElement("root");
                        root.AppendChild(node);
                        node.InnerXml = whole_dom.DocumentElement.InnerXml;
                    }
                    nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");
                }
            }

            string strVolumeString =
    VolumeInfo.BuildItemVolumeString(
    IssueUtil.GetYearPart(this.PublishTime),
    this.Issue,
            this.Zong,
            this.Volume);

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                OrderBindingItem order = new OrderBindingItem();
                nRet = order.Initial(node.OuterXml, out strError);
                if (nRet == -1)
                    return -1;

                if (this.Container.m_bHideLockedOrderGroup == true)
                {
                    // 观察一个馆藏分配字符串，看看是否部分在当前用户管辖范围内
                    // return:
                    //      -1  出错
                    //      0   没有任何部分在管辖范围
                    //      1   至少部分在管辖范围内
                    nRet = Global.DistributeCross(order.Distribute,
                        this.Container.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        List<string> refids = null;
                        // 获得一个馆藏分配字符串里面的所有refid
                        nRet = Global.GetRefIDs(order.Distribute,
            out refids,
            out strError);
                        if (nRet == -1)
                            return -1;
                        foreach (string refid in refids)
                        {
                            Cell cell = FindCellByRefID(refid, already_placed_cells);
                            if (cell != null && cell.IsMember == false)
                            {
                                already_placed_cells.Remove(cell);
                                if (cell.item != null)
                                    this.Container.m_hideitems.Add(cell.item);

                                if (bAddGroupCells == false)
                                    index = already_placed_cells.Count; // 使得排列紧密
                            }
                        }
                        foreach (string refid in refids)
                        {
                            Cell cell = FindCellByRefID(refid, exist_cells);
                            if (cell != null && cell.IsMember == false)
                            {
                                exist_cells.Remove(cell);
                                if (cell.item != null)
                                    this.Container.m_hideitems.Add(cell.item);
                            }
                        }
                        continue;
                    }
                }

                this.OrderItems.Add(order);
            }

            for (int i = 0; i < this.OrderItems.Count; i++)
            {
                /*
                XmlNode node = nodes[i];

                OrderBindingItem order = new OrderBindingItem();
                nRet = order.Initial(node.OuterXml, out strError);
                if (nRet == -1)
                    return -1;

                this.OrderItems.Add(order);
                 * */
                OrderBindingItem order = this.OrderItems[i];

                GroupCell group_head = null;
                if (bAddGroupCells == true)
                {
                    // 首先创建一个GroupCell格子
                    group_head = new GroupCell();
                    group_head.order = order;

                    this.SetCell(index++, group_head);
                }

                // 
                string strOldCopy = GetOldValue(order.Copy);
                int nOldCopy = GetNumberValue(strOldCopy);

                string strNewCopy = GetNewValue(order.Copy);
                int nNewCopy = GetNumberValue(strNewCopy);



                LocationCollection locations = new LocationCollection();
                nRet = locations.Build(order.Distribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                int nArriveCount = 0;

                for (int j = 0; j < Math.Max(nOldCopy, locations.Count); j++)
                {
                    if (nArriveCount >= nNewCopy
                        && nNewCopy > nOldCopy)
                        break;  // 注：如果location中的事项最终也无法达到满足nAriiveCount这么多个已到事项，那么未来得及建立的那些事项就只好被算作计划外的事项了

                    // 2010/4/28
                    if (nArriveCount >= nNewCopy
    && j >= nOldCopy)
                        break;


                    string strLocationName = "";
                    string strLocationRefID = "";

                    if (j < locations.Count)
                    {
                        Location location = locations[j];
                        strLocationName = location.Name;
                        strLocationRefID = location.RefID;
                    }

                    bool bOutOfControl = false;
                    if (Global.IsGlobalUser(this.Container.LibraryCodeList) == false)
                    {
                        string strLibraryCode = "";
                        string strPureName = "";

                        // 解析
                        Global.ParseCalendarName(strLocationName,
                    out strLibraryCode,
                    out strPureName);
                        if (StringUtil.IsInList(strLibraryCode, this.Container.LibraryCodeList) == false)
                            bOutOfControl = true;
                    }

                    // 没有refid
                    if (String.IsNullOrEmpty(strLocationRefID) == true
                        && this.Virtual == false)
                    {
                        // 预测格子

                        // 还是尽量从exist_cells中查找。因为这样可以避免对象被放弃后带来外部引用失效的问题
                        Cell cell = FindGroupMemberCell(exist_cells, i, j);

                        /*
                        // 对于修复的期行，则按照订购信息四元组进行寻找
                        if (cell == null && this.Virtual == true)
                        {
                            cell = FindGroupMemberCell(exist_cells, order);
                            if (cell != null)
                            {
                                cell.item.OrderInfoPosition = new Point(i, j);
                            }
                        }
                         * */

                        if (cell == null)
                        {
                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = strLocationRefID;
                            cell.item.LocationString = strLocationName;
                            cell.item.Locked = bOutOfControl;
                            cell.item.Calculated = true;
                            SetFieldValueFromOrderInfo(
                                false,
                                cell.item,
                                order);
                            cell.item.OrderInfoPosition = new Point(i, j);
                            // 如果必要，填充出版时间
                            if (String.IsNullOrEmpty(cell.item.PublishTime) == true)
                                cell.item.PublishTime = this.PublishTime;
                            if (String.IsNullOrEmpty(cell.item.Volume) == true)
                                cell.item.Volume = strVolumeString;
                        }
                        else
                        {
                            Debug.Assert(cell.item.OrderInfoPosition.X == i
                                && cell.item.OrderInfoPosition.Y == j, "");

                            // 2010/4/28
                            cell.item.LocationString = strLocationName;
                            cell.item.Locked = bOutOfControl;

                            if (cell.item != null)
                                cell.item.Container = this;
                            exist_cells.Remove(cell);   // 安放后就从临时数组中移走
                        }

                        this.SetCell(index++, cell);

                        if (group_head != null)
                            group_head.MemberCells.Add(cell);
                    }
                    else if (String.IsNullOrEmpty(strLocationRefID) == true
    && this.Virtual == true)
                    {
                        // 2012/5/16
                        // 没有refid
                        // 估计是已经到达的格子

                        // 从调用前已经安放的列表中查找
                        Cell cell = FindGroupMemberCell(already_placed_cells, order, strLocationName);
                        if (cell != null)
                        {
                            // 已经安放，仅仅添加采购信息
                            cell.item.OrderInfoPosition = new Point(i, j);
                            exist_cells.Remove(cell);   // 安放后就从临时数组中移走

                            if (group_head != null)
                                group_head.MemberCells.Add(cell);

                            {
                                string strTempRefID = cell.item.RefID;
                                string strTempLocationName = "";
                                nRet = order.DoAccept(cell.item.OrderInfoPosition.Y,
    ref strTempRefID,
    out strTempLocationName,
    out strError);
                                /*
                                if (nRet == -1)
                                    return -1;
                                 * */
                            }
                            nArriveCount++;
                            continue;
                        }

                        // 根据refid从exist_cells中查找
                        cell = FindGroupMemberCell(exist_cells, order, strLocationName);
                        if (cell == null)
                        {
                            /*
                            // 在订购信息中表明达到过，但是目前发现已经Deleted
                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = strLocationRefID;
                            cell.item.LocationString = strLocationName;
                            cell.item.Deleted = true;
                            SetFieldValueFromOrderInfo(
                                false,
                                cell.item,
                                order);
                            cell.item.OrderInfoPosition = new Point(i, j);
                            // 如果必要，填充出版时间
                            if (String.IsNullOrEmpty(cell.item.PublishTime) == true)
                                cell.item.PublishTime = this.PublishTime;
                            if (String.IsNullOrEmpty(cell.item.Volume) == true)
                                cell.item.Volume = strVolumeString;
                            this.SetCell(index++, cell);
                             * */
                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = strLocationRefID;
                            cell.item.LocationString = strLocationName;
                            cell.item.Locked = bOutOfControl;
                            cell.item.Calculated = true;
                            SetFieldValueFromOrderInfo(
                                false,
                                cell.item,
                                order);
                            cell.item.OrderInfoPosition = new Point(i, j);
                            // 如果必要，填充出版时间
                            if (String.IsNullOrEmpty(cell.item.PublishTime) == true)
                                cell.item.PublishTime = this.PublishTime;
                            if (String.IsNullOrEmpty(cell.item.Volume) == true)
                                cell.item.Volume = strVolumeString;
                            this.SetCell(index++, cell);

                        }
                        else
                        {
                            // 已经存在，直接安放
                            this.SetCell(index++, cell);
                            if (cell.item != null)
                                cell.item.Container = this;

                            cell.item.OrderInfoPosition = new Point(i, j);
                            exist_cells.Remove(cell);   // 安放后就从临时数组中移走

                            {
                                string strTempRefID = cell.item.RefID;
                                string strTempLocationName = "";
                                nRet = order.DoAccept(cell.item.OrderInfoPosition.Y,
    ref strTempRefID,
    out strTempLocationName,
    out strError);
                                /*
                                if (nRet == -1)
                                    return -1;
                                 * */
                            } 
                            nArriveCount++;
                        }

                        if (group_head != null)
                            group_head.MemberCells.Add(cell);
                    }
                    else
                    {
                        // 有refid
                        // 已经到达的格子
                        Debug.Assert(String.IsNullOrEmpty(strLocationRefID) == false, "");

                        nArriveCount++;

                        // 从调用前已经安放的列表中查找
                        Cell cell = FindCellByRefID(strLocationRefID, already_placed_cells);
                        if (cell != null)
                        {
                            // 已经安放，仅仅添加采购信息
                            cell.item.OrderInfoPosition = new Point(i, j);

                            cell.item.Locked = bOutOfControl;

                            exist_cells.Remove(cell);   // 安放后就从临时数组中移走

                            if (group_head != null)
                                group_head.MemberCells.Add(cell);
                            continue;
                        }

                        // 根据refid从exist_cells中查找
                        cell = FindCellByRefID(strLocationRefID, exist_cells);

                        // 2012/9/25
                        // 从隐藏的集合里面找
                        if (cell == null && this.Container.m_bHideLockedOrderGroup == false)
                        {
                            ItemBindingItem item = BindingControl.FindItemByRefID(strLocationRefID, this.Container.m_hideitems);
                            if (item != null)
                            {
                                this.Container.m_hideitems.Remove(item);
                                cell = new Cell();
                                cell.item = item;
                                cell.item.RefID = strLocationRefID;
                                cell.item.LocationString = strLocationName;
                                cell.item.Deleted = false;
                                SetFieldValueFromOrderInfo(
                                    false,
                                    cell.item,
                                    order);
                                cell.item.OrderInfoPosition = new Point(i, j);
                                // 如果必要，填充出版时间
                                if (String.IsNullOrEmpty(cell.item.PublishTime) == true)
                                    cell.item.PublishTime = this.PublishTime;
                                if (String.IsNullOrEmpty(cell.item.Volume) == true)
                                    cell.item.Volume = strVolumeString;

                                cell.ParentItem = cell.item.ParentItem;

                                // TODO: 如果需要，把parentitem带进来
                            }
                        }

                        if (cell == null)
                        {
                            // 在订购信息中表明达到过，但是目前发现已经Deleted
                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = strLocationRefID;
                            cell.item.LocationString = strLocationName;
                            cell.item.Locked = bOutOfControl;
                            cell.item.Deleted = true;
                            SetFieldValueFromOrderInfo(
                                false,
                                cell.item,
                                order);
                            cell.item.OrderInfoPosition = new Point(i, j);
                            // 如果必要，填充出版时间
                            if (String.IsNullOrEmpty(cell.item.PublishTime) == true)
                                cell.item.PublishTime = this.PublishTime;
                            if (String.IsNullOrEmpty(cell.item.Volume) == true)
                                cell.item.Volume = strVolumeString;
                            this.SetCell(index++, cell);

                            // 从其他期中去找
                            // "*"是一个特殊的refID，代表那些不存在的记录。因此没有必要去找refid为“*”的记录，因为它并不代表什么，如果真要去找，也会发现很多重复的
                            if (strLocationRefID != "*")
                            {
                                Cell cellTemp = this.Container.FindCellByRefID(strLocationRefID, this);
                                if (cellTemp != null)
                                {
                                    Debug.Assert(cellTemp.item != null, "");
                                    // 可以填充足够的信息了。但是是否可以移动那个对象过来?
                                    nRet = cell.item.Initial(cellTemp.item.Xml, out strError);
                                    if (nRet == -1)
                                    {
                                        string strTemp = "";
                                        cell.item.Initial("<root />", out strTemp);
                                        cell.item.State = strError;
                                    }
                                    else
                                    {
                                        cell.item.Comment += "\r\n警告: 在本种的其他期内发现了本册";
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 已经存在，直接安放
                            this.SetCell(index++, cell);
                            if (cell.item != null)
                                cell.item.Container = this;

                            cell.item.Locked = bOutOfControl;

                            cell.item.OrderInfoPosition = new Point(i, j);
                            exist_cells.Remove(cell);   // 安放后就从临时数组中移走
                        }

                        if (group_head != null)
                            group_head.MemberCells.Add(cell);
                    }
                }

                if (bAddGroupCells == true)
                {
                    // 最后创建一个代表右括号的GroupCell格子
                    GroupCell group = new GroupCell();
                    group.order = null;
                    group.EndBracket = true;

                    this.SetCell(index++, group);
                }
            }

            return 0;
        }

        static int IndexOf(List<ItemAndCol> infos,
            ItemBindingItem item)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].item == item)
                    return i;
            }

            return -1;
        }

        static void Remove(ref List<ItemAndCol> infos,
    ItemBindingItem item)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].item == item)
                {
                    infos.RemoveAt(i);
                    return;
                }
            }
        }

        static Cell FindCell(List<Cell> cells,
            ItemBindingItem item)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;
                if (cell.item == item)
                    return cell;
            }

            return null;
        }

        // 获得被用过位置区间右边的第一个空档位置
        static int GetRightUseableCol(List<ItemAndCol> infos)
        {
            int nCol = 0;
            for (int j = 0; j < infos.Count; j++)
            {
                ItemAndCol info = infos[j];
                if (info.Index == -1)
                    continue;

                if (nCol <= info.Index + 1)
                {
                    nCol = info.Index + 1 + 1;
                }
            }

            return nCol;
        }

        // 获得一个没有被用过的空档位置
        static int GetUseableCol(List<ItemAndCol> infos,
            int nCellCount,
            int nStartIndex)
        {
            Debug.Assert(nCellCount == 1 || nCellCount == 2, "");
            for (int index = nStartIndex; ; index++)
            {
                bool bExist = false;
                for (int j = 0; j < infos.Count; j++)
                {
                    ItemAndCol info = infos[j];
                    if (info.Index == -1)
                        continue;
                    if (nCellCount == 2)
                    {
                        if (info.Index == index
                            || info.Index == index + 1)
                        {
                            bExist = true;
                            break;
                        }
                        if (info.Index+1 == index
                            || info.Index+1 == index + 1)
                        {
                            bExist = true;
                            break;
                        }
                        continue;
                    }
                    Debug.Assert(nCellCount == 1, "");
                    if (info.Index == index
                        || info.Index + 1== index)
                    {
                        bExist = true;
                        break;
                    }
                }

                if (bExist == false)
                    return index;
            }

        }

        // parameters:
        //      index   在infos中找不到的对象，用index来确定安放位置
        void PlaceCells(
            List<ItemAndCol> infos,
            ref List<Cell> exist_cells,
            ref int index)
        {
            int nStartIdex = index;
            List<int> used_indexs = new List<int>();

            for (int i = 0; i < exist_cells.Count; i++)
            {
                Cell cell = exist_cells[i];
                if (cell == null)
                    continue;
                // 检查一个格子是否为合订成员?
                if (cell.ParentItem != null)
                {
                    int nCol = -1;
                    int nInfoIndex = IndexOf(infos, cell.ParentItem);
                    if (nInfoIndex != -1)
                    {
                        ItemAndCol info = infos[nInfoIndex];
                        if (info.Index == -1)
                        {
                            // 找到一个可用的位置
                            nCol = GetUseableCol(infos,
                                2,
                                0);
                            info.Index = nCol;  // 加入到里面，以便后面会自动避开
                        }
                        else
                            nCol = info.Index;
                    }
                    else
                    {
                        nCol = index++;
                        index++;
                    }

                    this.SetCell(nCol + 1, cell);
                    if (cell.item != null)
                        cell.item.Container = this;
                    exist_cells.Remove(cell);   // 安放后就从临时数组中移走
                    i--;

                    // 看看合订册是否正好在本期位置?
                    if (cell.ParentItem.MemberCells.Count > 0
                        && cell.ParentItem.MemberCells[0] == cell)
                    {
                        Cell parent_cell = FindCell(exist_cells, cell.ParentItem);
                        Debug.Assert(parent_cell != null, "第一个成员居然和合订册对象不在同一期");
                        if (parent_cell != null)
                        {
                            this.SetCell(nCol, parent_cell);
                            if (parent_cell.item != null)
                                parent_cell.item.Container = this;
                            // 已经把合订本对象也处理了，所以从数组中移走
                            exist_cells.Remove(parent_cell);
                        }
                    }
                }
                else if (cell.item != null && cell.item.IsParent == true)
                {
                    int nCol = -1;
                    int nInfoIndex = IndexOf(infos, cell.item);
                    if (nInfoIndex != -1)
                    {
                        ItemAndCol info = infos[nInfoIndex];
                        if (info.Index == -1)
                        {
                            // 找到一个可用的位置
                            nCol = GetUseableCol(infos,
                                2,
                                0);
                            info.Index = nCol;  // 加入到里面，以便后面会自动避开
                        }
                        else
                            nCol = info.Index;
                    }
                    else
                    {
                        nCol = index++;
                        index++;
                    }

                    this.SetCell(nCol, cell);
                    if (cell.item != null)
                        cell.item.Container = this;
                    exist_cells.Remove(cell);   // 安放后就从临时数组中移走
                    i--;

                    // 紧接着安放第一个成员册
                    if (cell.item.MemberCells.Count > 0)
                    {
                        this.SetCell(nCol + 1, cell.item.MemberCells[0]);
                        // 已经把合订本对象也处理了，所以从数组中移走
                        exist_cells.Remove(cell.item.MemberCells[0]);
                    }

                }
            }
            
            int temp = GetRightUseableCol(infos);
            if (temp > index)
                index = temp;
        }

        // parameters:
        //      strDistribute1  主要的
        //      strDistribute2  次要的。在strDistribute1中没有refid的位置，用这里的对应位置填充
        static int MergeDistribute(string strDistribute1,
            string strDistribute2,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            LocationCollection locations1 = new LocationCollection();
            int nRet = locations1.Build(strDistribute1,
                out strError);
            if (nRet == -1)
                return -1;

            LocationCollection locations2 = new LocationCollection();
            nRet = locations2.Build(strDistribute2,
                out strError);
            if (nRet == -1)
                return -1;

            LocationCollection locations3 = new LocationCollection();

            for(int i=0;i<locations2.Count;i++)
            {
                Location location2 = locations2[i];
                Location location1 = null;
                
                if (locations1.Count > i)
                    location1 = locations1[i];

                if (location1 == null || String.IsNullOrEmpty(location1.RefID) == true)
                    locations3.Add(location2);
                else
                    locations3.Add(location1);
            }

            for (int i = locations2.Count; i < locations1.Count; i++)
            {
                Location location1 = locations1[i];
                locations3.Add(location1);
            }

            strResult = locations3.ToString(true);
            return 0;
        }

        // 刷新订购信息
        public int RefreshOrderInfo(out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlNodeList exist_nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");


            List<string> exist_xmls = new List<string>();   // 刷新前已经存在的XML片断
            List<string> exist_refids = new List<string>(); // 这些XML片断的refid字符串
            foreach (XmlNode node in exist_nodes)
            {
                exist_xmls.Add(node.InnerXml);
                string strRefID = DomUtil.GetElementText(node, "refID");
                exist_refids.Add(strRefID);
            }

            {
                // 需要从外部获得采购信息
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = this.PublishTime;
                if (this.Container.m_bHideLockedOrderGroup == true)
                    e1.LibraryCodeList = this.Container.LibraryCodeList;
                this.Container.DoGetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + this.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    return -1;
                }

                {
                    XmlNode root = this.dom.DocumentElement.SelectSingleNode("orderInfo");
                    if (root == null)
                    {
                        root = this.dom.CreateElement("orderInfo");
                        this.dom.DocumentElement.AppendChild(root);
                    }

                    root.InnerXml = ""; // 删除原有的下级元素

                    this.Changed = true;

                    for (int i = 0; i < e1.OrderXmls.Count; i++)
                    {
                        XmlDocument whole_dom = new XmlDocument();
                        try
                        {
                            whole_dom.LoadXml(e1.OrderXmls[i]);
                        }
                        catch (Exception ex)
                        {
                            strError = "订购记录第 " + i.ToString() + " 个XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }

                        XmlNode node = null;
                        node = this.dom.CreateElement("root");
                        root.AppendChild(node);

                        string strRefID = DomUtil.GetElementText(whole_dom.DocumentElement, "refID");
                        int index = exist_refids.IndexOf(strRefID);

                        // 以前就有
                        if (index != -1)
                        {
                            node.InnerXml = exist_xmls[index];
                            // 仅仅修改<copy>里面的oldvalue部分；增补<distribute>
                            // 
                            string strCopy = DomUtil.GetElementText(node, "copy");
                            string strNewValue = "";
                            string strOldValue = "";
                            OrderDesignControl.ParseOldNewValue(strCopy,
                                out strOldValue,
                                out strNewValue);

                            string strDistribute = DomUtil.GetElementText(node, "distribute");

                            // 变为刷新的内容
                            node.InnerXml = whole_dom.DocumentElement.InnerXml;

                            string strNewDistribute = DomUtil.GetElementText(node, "distribute");

                            string strMerged = "";
                            nRet = MergeDistribute(strDistribute,
                                strNewDistribute,
                                out strMerged,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            DomUtil.SetElementText(node, "distribute", strMerged);
                            strCopy = DomUtil.GetElementText(node, "copy");
                            string strNewValue1 = "";
                            string strOldValue1 = "";
                            OrderDesignControl.ParseOldNewValue(strCopy,
                                out strOldValue1,
                                out strNewValue1);
                            DomUtil.SetElementText(node, "copy",
                                OrderDesignControl.LinkOldNewValue(strOldValue1,
                                strNewValue));

                            /*
                            // 用掉一个，清除它的位置
                            exist_refids[index] = "";
                            exist_xmls[index] = "";
                             * */
                        }
                        else
                        {
                            // 刷新后新增的
                            node.InnerXml = whole_dom.DocumentElement.InnerXml;
                        }
                    }

                    // TODL: 刷新后，比原来少的?


                    /*
                    XmlNodeList nodes = null;

                    nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");
                     * */
                }
                this.OrderInfoLoaded = true;
            }

            if (this.IssueLayoutState == IssueLayoutState.Accepting)
            {
                return LayoutAccepting(out strError);
            }
            else
            {
                return ReLayoutBinding(out strError);
            }
        }

        // 按照装订模式(重新)布局显示
        public int ReLayoutBinding(out string strError)
        {
            strError = "";
            int nRet = 0;

            // this.OrderItems.Clear();
            this.ClearOrderItems();

            // 将本期已有的Cell对象汇集在一起，待用
            List<Cell> exist_cells = new List<Cell>();
            exist_cells.AddRange(this.Cells);

            // 清除Cells数组
            this.Cells.Clear();

            int index = 0;  // 创建对象的最后下标

            // TODO: 先准备好被穿越的位置和合订对象信息，然后从本行中找适当的单元
            // 去安放这些位置，找不到的单元，则安放空的成员格子

            // 找到被本行割裂的合订册
            List<ItemAndCol> crossed_infos = null;
            nRet = this.Container.GetCrossBoundRange(this,
                false,  // 不但检测，还要返回具体的parent items
                out crossed_infos);

            List<Cell> crossed_cells = new List<Cell>();

            // 从这里删除，剩下的就是被跨越的空白位置
            List<ItemAndCol> crossed_blank_infos = new List<ItemAndCol>();
            crossed_blank_infos.AddRange(crossed_infos);

            // 从exist_cell中分选出跨越位置的对象
            if (crossed_blank_infos.Count > 0)
            {
                // 先安放在本行中能找到的成员
                for (int i = 0; i < exist_cells.Count; i++)
                {
                    Cell cell = exist_cells[i];
                    if (cell == null)
                        continue;
                    // 检查一个格子是否为合订成员?
                    if (cell.ParentItem != null
                        && IndexOf(crossed_blank_infos, cell.ParentItem) != -1)
                    {
                        Remove(ref crossed_blank_infos, cell.ParentItem);

                        crossed_cells.Add(cell);

                        exist_cells.Remove(cell);
                        i--;

                        // 看看合订册是否正好在本期位置?
                        if (cell.ParentItem.MemberCells.Count > 0
                            && cell.ParentItem.MemberCells[0] == cell)
                        {
                            Cell parent_cell = FindCell(exist_cells, cell.ParentItem);
                            Debug.Assert(parent_cell != null, "第一个成员居然和合订册对象不在同一期");
                            if (parent_cell != null)
                            {
                                crossed_cells.Add(parent_cell);
                                // 已经把合订本对象也处理了，所以从数组中移走
                                exist_cells.Remove(parent_cell);
                            }
                        }
                    }
                    else if (cell.item != null && cell.item.IsParent == true
                        && IndexOf(crossed_blank_infos, cell.item) != -1)
                    {
                        Remove(ref crossed_blank_infos, cell.item);

                        crossed_cells.Add(cell);

                        exist_cells.Remove(cell);
                        i--;

                        // 紧接着安放第一个成员册
                        if (cell.item.MemberCells.Count > 0)
                        {
                            Debug.Assert(cell.item.MemberCells[0] != null, "");

                            crossed_cells.Add(cell.item.MemberCells[0]);
                            // 已经把合订本对象也处理了，所以从数组中移走
                            exist_cells.Remove(cell.item.MemberCells[0]);
                        }
                    }
                }
            }

            // 安放跨越位置的对象
            if (crossed_cells.Count > 0)
            {
                PlaceCells(
                    crossed_infos,
                    ref crossed_cells,
                    ref index);
            }

            // 然后将剩下的被跨越的部位安放空白格子
            if (crossed_blank_infos.Count > 0)
            {
                for (int i = 0; i < crossed_blank_infos.Count; i++)
                {
                    ItemAndCol info = crossed_blank_infos[i];

                    // 找到现有的合适的nCol位置，安放它。
                    // 如果暂时没有合适的位置，就顺次安放
                    int nCol = info.Index;
                    if (nCol == -1)
                    {
                        // 找到一个可用的位置
                        nCol = GetUseableCol(crossed_infos,
                            2,
                            0);
                        info.Index = nCol;  // 加入到里面，以便后面会自动避开
                    }

                    // 创建空白格子
                    Cell cell = new Cell();
                    cell.item = null;
                    cell.ParentItem = info.item;
                    this.SetCell(nCol + 1, cell);
                }

                int temp = GetRightUseableCol(crossed_infos);
                if (temp > index)
                    index = temp;

            }

            // 然后安放剩下的成员或合订册对象
            if (exist_cells.Count > 0)
            {
                PlaceCells(
                    crossed_infos,
                    ref exist_cells,
                    ref index);
            }

            Debug.Assert(index >= this.Cells.Count, "");
            if (index < this.Cells.Count)
                index = this.Cells.Count;

            // 根据订购信息安放单册格子
            nRet = PlaceCellsByOrderInfo(
                false,
                this.Cells,
                ref exist_cells,
                ref index,
                out strError);
            if (nRet == -1)
                return -1;

            // 将最后剩下的单元放在末尾
            if (exist_cells.Count > 0)
            {
                if (this.Cells.Count > 0)
                {
                    // 先插入一个分割的null，表示和有订购信息的隔开
                    this.SetCell(index++, null);
                }

                for (int i = 0; i < exist_cells.Count; i++)
                {
                    Cell cell = exist_cells[i];

                    if (cell == null)
                        continue;
                    if (cell is GroupCell)
                        continue;
                    if (cell.item == null)
                        continue;
                    if (cell.item != null && cell.item.Calculated == true)
                        continue;

                    this.SetCell(index++, cell);
                    if (cell.item != null)
                        cell.item.Container = this;

                    exist_cells.RemoveAt(i);
                    i--;
                }

                // 丢弃剩下的对象
                for (int i = 0; i < exist_cells.Count; i++)
                {
                    Cell cell = exist_cells[i];

                    if (cell == null)
                        continue;
                    cell.Container = null;
                    if (cell.item != null)
                        cell.item.Container = null;
                }
            }

            this.RefreshAllOutofIssueValue();

#if DEBUG
            this.Container.VerifyAll();
#endif
            return 0;
        }

        // 按照装订模式(首次)布局显示
        public int InitialLayoutBinding(out string strError)
        {
            strError = "";
            int nRet = 0;

            // this.OrderItems.Clear();
            this.ClearOrderItems();

            List<Cell> exist_cells = new List<Cell>();
            int index = this.Cells.Count;  // 创建对象的最后下标

            // 根据订购信息安放格子
            nRet = PlaceCellsByOrderInfo(
                false,
                this.Cells,
                ref exist_cells,
                ref index,
                out strError);
            if (nRet == -1)
                return -1;

            /*
            // 根据Xml中<orderInfo>元素内容，计算出还没有到达的预测册，追加在右边
            // TODO: 将来是否从一个恒定的位置开始追加? 
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count == 0
                && this.OrderInfoLoaded == false)
            {
                // 需要从外部获得采购信息
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = this.PublishTime;
                if (this.Container.m_bHideLockedOrderGroup == true)
                    e1.LibraryCodeList = this.Container.LibraryCodeList;
                this.Container.DoGetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + this.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    return -1;
                }

                if (e1.OrderXmls.Count == 0)
                {
                    this.OrderInfoLoaded = true;
                }
                else
                {
                    XmlNode root = this.dom.DocumentElement.SelectSingleNode("orderInfo");
                    if (root == null)
                    {
                        root = this.dom.CreateElement("orderInfo");
                        this.dom.DocumentElement.AppendChild(root);
                    }
                    for (int i = 0; i < e1.OrderXmls.Count; i++)
                    {
                        XmlDocument whole_dom = new XmlDocument();
                        try
                        {
                            whole_dom.LoadXml(e1.OrderXmls[i]);
                        }
                        catch (Exception ex)
                        {
                            strError = "订购记录第 " + i.ToString() + " 个XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }
                        XmlNode node = this.dom.CreateElement("root");
                        root.AppendChild(node);
                        node.InnerXml = whole_dom.DocumentElement.InnerXml;
                    }
                    nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                OrderBindingItem order = new OrderBindingItem();
                nRet = order.Initial(node.OuterXml, out strError);
                if (nRet == -1)
                    return -1;

                this.OrderItems.Add(order);

                LocationColletion locations = new LocationColletion();
                nRet = locations.Build(order.Distribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location location = locations[j];

                    if (String.IsNullOrEmpty(location.RefID) == true)
                    {
                        // 预测格子
                        Cell cell = new Cell();
                        cell.item = new ItemBindingItem();
                        cell.item.Container = this;
                        cell.item.Initial("<root />", out strError);
                        cell.item.RefID = location.RefID;
                        cell.item.LocationString = location.Name;
                        cell.item.Calculated = true;
                        cell.item.OrderInfoPosition = new Point(i, j);
                        this.SetCell(index++, cell);
                    }
                    else
                    {
                        // 已经到达的格子

                        // 根据refid从this.Cells中查找
                        Cell cell = FindCellByRefID(location.RefID, this.Cells);
                        if (cell == null)
                        {
                            // 在订购信息中表明达到过，但是目前发现已经Deleted
                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = location.RefID;
                            cell.item.LocationString = location.Name;
                            cell.item.Deleted = true;
                            cell.item.OrderInfoPosition = new Point(i, j);
                            this.SetCell(index++, cell);
                        }
                        else
                        {
                            // 已经存在，跳过
                            cell.item.OrderInfoPosition = new Point(i, j);
                        }
                    }
                }
            }
             * */

            this.RefreshAllOutofIssueValue();
            return 0;
        }

        // 按照记到模式(重新)布局显示
        public int LayoutAccepting(out string strError)
        {
            strError = "";
            int nRet = 0;

            // this.OrderItems.Clear();
            this.ClearOrderItems();

            // 将现有的Cell集中起来
            List<Cell> exist_cells = new List<Cell>();
            exist_cells.AddRange(this.Cells);

            // 清除Cells数组
            this.Cells.Clear();

            int index = 0;  // 创建对象的最后下标

            // 根据订购信息安放格子
            nRet = PlaceCellsByOrderInfo(
                true,
                this.Cells, // 或者 new List<Cells>();
                ref exist_cells,
                ref index,
                out strError);
            if (nRet == -1)
                return -1;

            /*
            // 根据Xml中<orderInfo>元素内容，初始化出全部应有的位置
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count == 0
                && this.OrderInfoLoaded == false)
            {
                // 需要从外部获得采购信息
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = this.PublishTime;
                if (this.Container.m_bHideLockedOrderGroup == true)
                    e1.LibraryCodeList = this.Container.LibraryCodeList;
                this.Container.DoGetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + this.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    return -1;
                }

                if (e1.OrderXmls.Count == 0)
                {
                    this.OrderInfoLoaded = true;
                }
                else
                {
                    XmlNode root = this.dom.DocumentElement.SelectSingleNode("orderInfo");
                    if (root == null)
                    {
                        root = this.dom.CreateElement("orderInfo");
                        this.dom.DocumentElement.AppendChild(root);
                    }
                    for (int i = 0; i < e1.OrderXmls.Count; i++)
                    {
                        XmlDocument whole_dom = new XmlDocument();
                        try
                        {
                            whole_dom.LoadXml(e1.OrderXmls[i]);
                        }
                        catch (Exception ex)
                        {
                            strError = "订购记录第 " + i.ToString() + " 个XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }
                        XmlNode node = this.dom.CreateElement("root");
                        root.AppendChild(node);
                        node.InnerXml = whole_dom.DocumentElement.InnerXml;
                    }
                    nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*");
                }
            }


            for(int i=0;i<nodes.Count;i++)
            {
                XmlNode node = nodes[i];


                OrderBindingItem order = new OrderBindingItem();
                nRet = order.Initial(node.OuterXml, out strError);
                if (nRet == -1)
                    return -1;

                this.OrderItems.Add(order);

                // 首先创建一个GroupCell格子
                GroupCell group = new GroupCell();
                group.order = order;

                this.SetCell(index++, group);

                // 逐个创建下属的册(预测)格子
                LocationColletion locations = new LocationColletion();
                nRet = locations.Build(order.Distribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location location = locations[j];

                    if (String.IsNullOrEmpty(location.RefID) == true)
                    {
                        // 预测格子

                        // 还是尽量从exist_cells中查找。因为这样可以避免对象被放弃后带来外部引用失效的问题
                        Cell cell = FindOrderedCell(exist_cells, i, j);
                        if (cell == null)
                        {

                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = location.RefID;
                            cell.item.LocationString = location.Name;
                            cell.item.Calculated = true;
                            cell.item.OrderInfoPosition = new Point(i, j);
                        }
                        else
                        {
                            Debug.Assert(cell.item.OrderInfoPosition.X == i
    && cell.item.OrderInfoPosition.Y == j, "");
                            exist_cells.Remove(cell);   // 安放后就从临时数组中移走
                        }

                        this.SetCell(index++, cell);
                    }
                    else
                    {
                        // 已经到达的格子

                        // 根据refid从exist_cells中查找
                        Cell cell = FindCellByRefID(location.RefID, exist_cells);
                        if (cell == null)
                        {
                            // Deleted
                            cell = new Cell();
                            cell.item = new ItemBindingItem();
                            cell.item.Container = this;
                            cell.item.Initial("<root />", out strError);
                            cell.item.RefID = location.RefID;
                            cell.item.LocationString = location.Name;
                            cell.item.Deleted = true;
                            cell.item.OrderInfoPosition = new Point(i, j);
                            this.SetCell(index++, cell);
                        }
                        else
                        {
                            // 直接采纳
                            cell.item.OrderInfoPosition = new Point(i, j);
                            this.SetCell(index++, cell);
                            exist_cells.Remove(cell);   // 用过的就移走
                        }
                    }
                }

                // 最后创建一个代表右括号的GroupCell格子
                group = new GroupCell();
                group.order = null;
                group.EndBracket = true;

                this.SetCell(index++, group);
            }
            */

            // 将剩下的单元放在末尾
            if (exist_cells.Count > 0)
            {
                /*
                if (nodes.Count > 0)
                {
                    // 先插入一个分割的null
                    this.SetCell(index++, null);
                }
                 * */

                for (int i = 0; i < exist_cells.Count; i++)
                {
                    Cell cell = exist_cells[i];
                    if (cell == null)
                        continue;
                    if (cell is GroupCell)
                        continue;
                    if (cell.item == null
                        && cell.ParentItem == null) // 合订成员的cell.item == null依然要加入到本行 2010/6/5
                    {
                        continue;
                    }

                    if (cell.item != null && cell.item.Calculated == true)
                        continue;

                    this.SetCell(index++, cell);
                    if (cell.item != null)
                        cell.item.Container = this;

                    exist_cells.RemoveAt(i);
                    i--;
                }

                // 丢弃剩下的对象
                for (int i = 0; i < exist_cells.Count; i++)
                {
                    Cell cell = exist_cells[i];

                    if (cell == null)
                        continue;
                    cell.Container = null;
                    if (cell.item != null)
                        cell.item.Container = null;
                }

                // exist_cells.Clear();    // 
            }
            this.RefreshAllOutofIssueValue();

#if DEBUG
            this.Container.VerifyAll();
#endif
            return 0;
        }



        static Cell FindCellByRefID(string strRefID,
            List<Cell> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;
                if (cell.item == null)
                    continue;
                if (cell.item.RefID == strRefID)
                    return cell;
            }
            return null;
        }

        // 一个出版时间是否在出版时间范围内
        // 可能会抛出异常
        static bool IsInPublishTimeRange(string strPublishTime,
            string strRange)
        {
            string strError = "";
            int nRet = 0;

            if (strPublishTime.IndexOf("-") != -1)
            {
                strError = "strPublishTime时间字符串 '"+strPublishTime+"' 应当为单点形式，而不能用范围形式";
                throw new Exception(strError);
            }

            if (strRange.IndexOf("-") == -1)
            {
                strError = "strRange时间字符串 '" + strRange + "' 应当为范围形式(具有破折号)";
                throw new Exception(strError);
            }

            DateTime startTime = new DateTime(0);
            DateTime endTime = new DateTime(0);
            nRet = Global.ParseTimeRangeString(strRange,
                false,
                out startTime,
                out endTime,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            DateTime testTime = DateTimeUtil.Long8ToDateTime(strPublishTime);

            if (testTime >= startTime && testTime <= endTime)
                return true;
            return false;
        }

        // 设置下属的所有成员册和单册格子的OutofIssue值
        internal void RefreshAllOutofIssueValue()
        {
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.GetCell(i);
                if (cell == null)
                    continue;
                if (cell.item == null)
                {
                    cell.OutofIssue = false;
                    continue;
                }

                // 跳过合订册对象
                // TODO: 是否可以比较一下合订册所在的期，其否在合订册的publishtimerange以内
                if (cell.item.IsParent == true)
                {
                    try
                    {
                        if (IsInPublishTimeRange(this.PublishTime, cell.item.PublishTime) == false)
                        {
                            cell.OutofIssue = true;
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        cell.item.Comment += "\r\n警告: " + ex.Message;
                        cell.OutofIssue = true;
                    }
                    continue;
                }

                if (cell.item.PublishTime != this.PublishTime)
                {
                    cell.OutofIssue = true;
                    continue;
                }

                string strIssue = "";
                string strZong = "";
                string strVolume = "";

                // 解析当年期号、总期号、卷号的字符串
                VolumeInfo.ParseItemVolumeString(cell.item.Volume,
                    out strIssue,
                    out strZong,
                    out strVolume);

                if (strIssue != this.Issue
                    || strZong != this.Zong
                    || strVolume != this.Volume)
                    cell.OutofIssue = true;
                else
                    cell.OutofIssue = false;
            }
        }

        // 设置下属的一个成员册和单册格子的OutofIssue值
        // parameters:
        //      index 单格index
        internal void RefreshOutofIssueValue(int index)
        {
            if (index >= this.Cells.Count)
                return;
            Cell cell = this.GetCell(index);
            if (cell == null)
                return;

            if (cell.item == null)
            {
                cell.OutofIssue = false;
                return;
            }

            // TODO: 是否可以比较一下合订册所在的期，其否在合订册的publishtimerange以内
            if (cell.item.IsParent == true)
            {
                try
                {
                    if (IsInPublishTimeRange(this.PublishTime, cell.item.PublishTime) == false)
                        cell.OutofIssue = true;
                    else
                        cell.OutofIssue = false;
                    return;
                }
                catch (Exception ex)
                {
                    cell.item.Comment += "\r\n警告: " + ex.Message;
                }
                return;
            }

            if (cell.item.PublishTime != this.PublishTime)
            {
                cell.OutofIssue = true;
                return;
            }

            string strIssue = "";
            string strZong = "";
            string strVolume = "";

            // 解析当年期号、总期号、卷号的字符串
            VolumeInfo.ParseItemVolumeString(cell.item.Volume,
                out strIssue,
                out strZong,
                out strVolume);

            if (strIssue != this.Issue
                || strZong != this.Zong
                || strVolume != this.Volume)
                cell.OutofIssue = true;
            else
                cell.OutofIssue = false;
        }

#if OLD_VERSION

        // 测试一个双格位置是否为空白
        public bool IsBlankPosition(int nNo,
            ItemBindingItem exclude_parent_item)
        {
            if (this.Cells.Count <= nNo*2)
                return true;
            Cell cell = this.Cells[nNo*2];
            if (cell != null)
            {
                Debug.Assert(cell.ParentItem == null, "合订册所在格子ParentItem必须为null");

                // 属于要排除的合订册的。即被当作空位置
                if (exclude_parent_item != null
                    && exclude_parent_item == cell.ParentItem)
                    return true;

                if (cell.IsMember == true)
                    return false;

                if (cell.item != null)
                {
                    Debug.Assert(cell.item.ParentItem == null, "合订册Item的ParentItem必须为null");

                    // 属于要排除的合订册的。即被当作空位置
                    if (exclude_parent_item != null
                        && cell.item == exclude_parent_item)
                        return true;

                    return false;
                }
            }
            if (this.Cells.Count <= nNo*2 + 1)
                return true;

            cell = this.Cells[nNo*2+1];
            if (cell == null)
                return true;

            //
            if (cell.ParentItem == exclude_parent_item
                && exclude_parent_item != null)
                return true;

            if (cell.IsMember == true)
                return false;

            if (cell.item != null)
            {
                //
                if (cell.item.ParentItem == exclude_parent_item
                    && exclude_parent_item != null)
                    return true;

                return false;
            } 
            
            return true;
        }
#endif

#if OLD_VERSION

        // 测试一个双格位置是否为合订位置
        public bool IsBindedPosition(int nNo)
        {
            if (this.Cells.Count <= nNo * 2)
                return false;
            /* 首次安放的时候，这里会误会
            Cell cell = this.Cells[nNo * 2];
            if (cell.Binded == true)
                return true;
            if (cell.item != null)
                return true;    // 奇数位置出现item
             * */
            if (this.Cells.Count <= nNo * 2 + 1)
                return false;
            Cell cell = this.Cells[nNo * 2 + 1];
            if (cell == null)
                return false;
            if (cell.IsMember == true)
                return true;
            if (cell.item != null && cell.item.IsMember == true)
                return true;
            return false;
        }

#endif

        // 探测是否为适合覆盖单格的空白位置
        public bool IsBlankSingleIndex(int index)
        {
            Cell cell = this.GetCell(index);
            bool bBlank = IsBlankOrNullCell(cell);
            if (bBlank == true)
            {
                // 2010/3/6
                // 还要判断当前位置是否为合订成员格子
                if (cell != null && cell.ParentItem != null)
                    return false;

                // 还要判断右侧是否为合订成员格子
                Cell cellRight = this.GetCell(index + 1);
                if (cellRight == null || cellRight.IsMember == false)
                    return true;
            }

            return false;
        }

        // 探测是否为适合覆盖双格的空白位置
        // 简单版本。TODO: 也可以通过包装IsBlankDoubleIndex(int, ItemBindingItem)版本来实现，那样更容易维护一些
        public bool IsBlankDoubleIndex(int index)
        {
            bool bLeftBlank = IsBlankSingleIndex(index);
            if (bLeftBlank == false)
                return false;   // 优化
            bool bRightBlank = IsBlankSingleIndex(index + 1);
            if (bLeftBlank == true && bRightBlank == true)
                return true;
            return false;
        }

        // 包装版本
        public bool IsBlankDoubleIndex(int index,
            ItemBindingItem exclude_parent_item)
        {
            return this.IsBlankDoubleIndex(index, exclude_parent_item, null);
        }


        // 探测是否为适合覆盖双格的空白位置
        // 复杂一点的版本。可以排除特定的合订本对象
        public bool IsBlankDoubleIndex(int index,
            ItemBindingItem exclude_parent_item,
            ItemBindingItem exclude_member_item)
        {
            bool bLeftBlank = IsBlankSingleIndex(index);
            bool bRightBlank = IsBlankSingleIndex(index + 1);
            if (bLeftBlank == true && bRightBlank == true)
                return true;

            // 需要继续判断成员册所属的合订册，是否为要排除的对象
            if (exclude_parent_item != null
                || exclude_member_item != null)
            {
                // 探测是否为合订成员占据的位置
                // return:
                //      -1  是。并且是双格的左侧位置
                //      0   不是
                //      1   是。并且是双格的右侧位置
                int nRet = IsBoundIndex(index);
                if (nRet == 0)
                {
                    // 还要继续判断index右边一个，是否为成员的左侧，并属于exclude_parent_item
                    if (exclude_parent_item != null)
                    {
                        if (this.GetIndexMemberParent(index + 1) == exclude_parent_item
                             && bLeftBlank == true)
                            return true;
                    }

                    if (exclude_member_item != null)
                    {
                        Debug.Assert(exclude_member_item != null, "");
                        if (bLeftBlank == true)
                        {
                            Cell cellTemp = this.GetCell(index + 1);
                            if (cellTemp != null && cellTemp.item == exclude_member_item)
                                return true;
                        }
                        else if (bRightBlank == true)
                        {
                            Debug.Assert(bRightBlank == true, "");
                            Cell cellTemp = this.GetCell(index);
                            if (cellTemp != null && cellTemp.item == exclude_member_item)
                                return true;
                        }
                    }

                    return false;
                }

                int nLeftIndex = -1;
                int nRightIndex = -1;
                if (nRet == -1)
                {
                    //      -1  是。并且是双格的左侧位置

                    nLeftIndex = index;
                    nRightIndex = index + 1;
                }
                else if (nRet == 1)
                {
                    //      1   是。并且是双格的右侧位置


                    // 需要补充判断一下，index右边一个格子是否为空白位置
                    if (this.IsBlankSingleIndex(index + 1) == false)
                        return false;

                    nLeftIndex = index - 1;
                    nRightIndex = index;
                }
                else
                {
                    Debug.Assert(false, "");
                }

                Debug.Assert(nLeftIndex >= 0, "");
                Debug.Assert(nRightIndex >= 0, "");
                Debug.Assert(nLeftIndex + 1 == nRightIndex, "");

                Cell cellLeft = this.GetCell(nLeftIndex);
                if (cellLeft != null)
                {
                    Debug.Assert(cellLeft != null, "");
                    if (cellLeft.item != null)
                    {
                        if (exclude_parent_item != null)
                        {
                            if (cellLeft.item == exclude_parent_item)
                                return true;
                        }

                        if (exclude_member_item != null)
                        {
                            if (cellLeft.item == exclude_member_item)
                                return true;
                        }
                    }
                }

                Cell cellRight = this.GetCell(nRightIndex);
                if (cellRight != null)
                {
                    Debug.Assert(cellRight != null, "");

                    if (exclude_parent_item != null)
                    {
                        if (cellRight.ParentItem == exclude_parent_item)
                            return true;
                    }
                    if (exclude_member_item != null)
                    {
                        if (cellRight.ParentItem == exclude_member_item)
                            return true;
                    }
                }
                else
                {
                    Debug.Assert(false, "右侧的格子不应该是null");
                }
            }

            return false;
        }

        // 获得某index所属合订册对象
        public ItemBindingItem GetIndexMemberParent(int index)
        {
            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nRet = IsBoundIndex(index);
            if (nRet == 0)
                return null;
            int nLeftIndex = -1;
            int nRightIndex = -1;
            if (nRet == -1)
            {
                nLeftIndex = index;
                nRightIndex = index + 1;
            }
            else if (nRet == 1)
            {
                nLeftIndex = index - 1;
                nRightIndex = index;
            }
            else
            {
                Debug.Assert(false, "");
            }

            Debug.Assert(nLeftIndex >= 0, "");
            Debug.Assert(nRightIndex >= 0, "");
            Debug.Assert(nLeftIndex + 1 == nRightIndex, "");

            Cell cellLeft = this.GetCell(nLeftIndex);
            if (cellLeft != null)
            {
                Debug.Assert(cellLeft != null, "");
                if (cellLeft.item != null)
                {
                    return cellLeft.item;
                }
            }

            Cell cellRight = this.GetCell(nRightIndex);
            if (cellRight != null)
            {
                Debug.Assert(cellRight != null, "");
                return cellRight.ParentItem;
            }
            else
            {
                Debug.Assert(false, "右侧的格子不应该是null");
            }

            return null;
        }

        // 探测是否为合订成员占据的位置
        // return:
        //      -1  是。并且是双格的左侧位置
        //      0   不是
        //      1   是。并且是双格的右侧位置
        public int IsBoundIndex(int index)
        {
            // 如果是自由期
            if (String.IsNullOrEmpty(this.PublishTime) == true)
                return 0;

            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Binding, "只能在Binding布局使用IsBoundIndex()函数");
            Debug.Assert(index >= 0, "");

            Cell cellCurrent = this.GetCell(index);

            // 如果是非Null格子, 要看看cell.IsMember是否为true
            if (cellCurrent != null)
            {
                if (cellCurrent.IsMember == true)
                    return 1;
            }

            Cell cellRight = null;
            bool bLeftBlank = IsBlankOrNullCell(cellCurrent);
            if (bLeftBlank == true)
            {
                // 还要判断右侧是否为合订成员格子
                cellRight = this.GetCell(index + 1);
                if (cellRight != null && cellRight.IsMember == true)
                    return -1;
                return 0;
            }

            // 当前位置为非空白
            Debug.Assert(cellCurrent != null && cellCurrent.item != null, "");

            /*
            if (cellCurrent.IsMember == true)
                return 1;
             * */

            // 如果左侧是合订本
            if (cellCurrent.item != null && cellCurrent.item.IsParent == true)
                return -1;


            // 还要判断右侧是否为合订成员格子
            cellRight = this.GetCell(index + 1);
            if (cellRight != null && cellRight.IsMember == true)
                return -1;
            return 0;
        }

        // 2010/2/28
        // 腾出一个单格的空位。如果已经是空白格子，就直接使用
        // parameters:
        //      nNo 单格index
        public void GetBlankSingleIndex(int nIndex)
        {
            if (this.IsBlankSingleIndex(nIndex) == true)
                return; // 已经是空位

            // 打算在右边扩展出空位
            // 要分为两种情况：
            // 1) 当前位置是一个普通的单格内容。例如非合订册
            // 2) 当前位置是一个合订成员占据。
            // 分别需要扩展出一个和两个格子来

            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nIsBoundIndex = IsBoundIndex(nIndex);

            // 是双格子之左
            if (nIsBoundIndex == -1)
            {
                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(nIndex + 1);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离一格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 1);
            }
            else if (nIsBoundIndex == 1)
            {
                // 是双格之右

                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(nIndex);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离二格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 2);
            }
            else
            {
                // 单册

                // 在右边扩展出一个空格子
                bool bRet = this.IsBlankSingleIndex(nIndex + 1);
                if (bRet == false)
                    this.GetBlankSingleIndex(nIndex + 1);

                Cell cell = this.GetCell(nIndex);
                this.SetCell(nIndex + 1, cell);
                this.SetCell(nIndex, null);
            }
        }

        // 2010/2/28
        // 腾出一个双格空位。如果已经是空白格子，就直接使用
        // parameters:
        //      nIndex 单格index
        //      exclude_parent_item 要排除的合订本对象
        //      exclude_member_item 要排除的成员对象
        public void GetBlankDoubleIndex(int nIndex,
            ItemBindingItem exclude_parent_item,
            ItemBindingItem exclude_member_item)
        {
            if (this.IsBlankDoubleIndex(nIndex,
                exclude_parent_item,
                exclude_member_item) == true)
                return; // 已经是空位

            // 打算在右边扩展出空位
            // 要分为两种情况：
            // 1) 当前位置是一个普通的单格内容。例如非合订册
            // 2) 当前位置是一个合订成员占据。
            // 分别需要扩展出一个和两个格子来

            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nIsBoundIndex = IsBoundIndex(nIndex);

            // 是双格子之左
            if (nIsBoundIndex == -1)
            {
                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(nIndex + 1);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离二格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 2);
            }
            else if (nIsBoundIndex == 1)
            {
                // 是双格之右

                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(nIndex);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离三格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 3);
            }
            else
            {
                // 单册
                Cell cell = this.GetCell(nIndex);

                /*
                // index位置是否为要排除的成员格子
                if (cell.item == exclude_member_item
                    && exclude_member_item != null)
                {
                    // 在右边扩展出一个空格子，就够了
                    bool bRet = this.IsBlankSingleIndex(nIndex + 1);
                    if (bRet == false)
                        this.GetBlankSingleIndex(nIndex + 1);

                    return;
                }*/

                {
                    // 在右边扩展出两个空格子
                    bool bRet = this.IsBlankDoubleIndex(nIndex + 1,
                        exclude_parent_item,
                        exclude_member_item);
                    if (bRet == false)
                        this.GetBlankDoubleIndex(nIndex + 1,
                            exclude_parent_item,
                            exclude_member_item);

                    // 把在当前位置的格子挪动过去
                    this.SetCell(nIndex + 2, cell);
                    this.SetCell(nIndex, null);
                }
            }
        }

        // 新版本，替代GetNewPosition()
        // 腾出一个单格新位。无论如何都要创建新位
        // parameters:
        //      nNo 双格序号
        public void GetNewSingleIndex(int index)
        {
            {
                Cell cell_1 = this.GetCell(index);
                if (cell_1 == null)
                    return;
            }

            // 打算在右边扩展出空位
            // 要分为两种情况：
            // 1) 当前位置是一个普通的单格内容。例如非合订册
            // 2) 当前位置是一个合订成员占据。
            // 分别需要扩展出一个和两个格子来

            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nIsBoundIndex = IsBoundIndex(index);

            // 是双格子之左
            if (nIsBoundIndex == -1)
            {
                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(index + 1);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离1格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 1);
            }
            else if (nIsBoundIndex == 1)
            {
                // 是双格之右

                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(index);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离2格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 2);
            }
            else
            {
                // 单册

                /*
                Cell cell = this.GetCell(index);
                IssueBindingItem issue = cell.Container;
                 * */

                // 在当前位置扩展出一个空双格
                bool bRet = this.IsBlankSingleIndex(index + 1);
                if (bRet == false)
                    this.GetBlankSingleIndex(index + 1);

                Cell cell = this.GetCell(index);
                this.SetCell(index + 1, cell);
                this.SetCell(index, null);
            }

            // 清除当前位置
            this.SetCell(index, null);
        }

        // 新版本，替代GetNewPosition()
        // 腾出一个双格新位。无论如何都要创建新位
        // parameters:
        //      nNo 双格序号
        public void GetNewDoubleIndex(int index)
        {
            {
                Cell cell_1 = this.GetCell(index);
                Cell cell_2 = this.GetCell(index + 1);

                if (cell_1 == null && cell_2 == null)
                    return;
            }

            // 打算在右边扩展出空位
            // 要分为两种情况：
            // 1) 当前位置是一个普通的单格内容。例如非合订册
            // 2) 当前位置是一个合订成员占据。
            // 分别需要扩展出一个和两个格子来

            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nIsBoundIndex = IsBoundIndex(index);

            // 是双格子之左
            if (nIsBoundIndex == -1)
            {
                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(index + 1);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离二格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 2);
            }
            else if (nIsBoundIndex == 1)
            {
                // 是双格之右

                // 把nIndex这里的内容移动到右边。
                // 因为这里已经是合订内容，所以要同时搬动整个一个合订册的所有成员格子
                Cell cell_2 = this.GetCell(index);
                Debug.Assert(cell_2.ParentItem != null, "");

                // 移动距离三格
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem, 3);
            }
            else
            {
                // 单册

                /*
                Cell cell = this.GetCell(index);
                IssueBindingItem issue = cell.Container;
                 * */

                // 在当前位置右边扩展出一个空双格
                bool bRet = this.IsBlankDoubleIndex(index + 1, null, null);
                if (bRet == false)
                    this.GetBlankDoubleIndex(index + 1, null, null);

                // 把在当前位置的格子挪动过去
                Cell cell = this.GetCell(index);
                this.SetCell(index + 2, cell);
                this.SetCell(index, null);
            }

            // 清除当前位置
            this.SetCell(index, null);
            this.SetCell(index + 1, null);
        }

#if OLD_VERSION
        // 腾出一个空位。如果已经是空白格子，就直接使用
        // parameters:
        //      nNo 双格序号
        public void GetBlankPosition(int nNo,
            ItemBindingItem exclude_parent_item)
        {
            if (this.IsBlankPosition(nNo, exclude_parent_item) == true)
                return; // 已经是空位

            // 看看右边是否有空位
            bool bRet = this.IsBlankPosition(nNo + 1, exclude_parent_item);
            if (bRet == false)
                this.GetBlankPosition(nNo + 1, exclude_parent_item);

            bRet = IsBindedPosition(nNo);
            if (bRet == false)
            {
                Cell cell_1 = this.GetCell(nNo * 2);
                Cell cell_2 = this.GetCell(nNo * 2 + 1);
                if (cell_1 != null)
                {
                    this.SetCell(nNo * 2 + 2, cell_1);
                    this.SetCell(nNo * 2,  null);

                }
                if (cell_2 != null)
                {
                    this.SetCell(nNo * 2 + 2 + 1, cell_2);
                    this.SetCell(nNo * 2 + 1, null);
                }
            }
            else
            {
                Cell cell_2 = this.GetCell(nNo * 2 + 1);
                /*
                Debug.Assert(cell_2.item != null, "");
                Debug.Assert(cell_2.item.ParentItem != null, "");
                // 为合订格子
                this.Container.MoveMemberCellsToRight(cell_2.item.ParentItem);
                 * */
                Debug.Assert(cell_2.ParentItem != null, "");

                if (exclude_parent_item != null
                    && cell_2.ParentItem == exclude_parent_item)
                    return; //

                // 为合订格子
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem);
            }
        }

        // 腾出一个新位。无论如何都要创建新位
        // parameters:
        //      nNo 双格序号
        public void GetNewPosition(int nNo)
        {
            {
                Cell cell_1 = this.GetCell(nNo * 2);
                Cell cell_2 = this.GetCell(nNo * 2 + 1);

                if (cell_1 == null && cell_2 == null)
                    return;
            }

            // 看看右边是否有空位
            bool bRet = this.IsBlankPosition(nNo + 1, null);
            if (bRet == false)
                this.GetBlankPosition(nNo + 1, null);

            bRet = IsBindedPosition(nNo);
            if (bRet == false)
            {
                Cell cell_1 = this.GetCell(nNo * 2);
                Cell cell_2 = this.GetCell(nNo * 2 + 1);
                if (cell_1 != null)
                {
                    this.SetCell(nNo * 2 + 2, cell_1);
                    this.SetCell(nNo * 2, null);

                }
                if (cell_2 != null)
                {
                    this.SetCell(nNo * 2 + 2 + 1, cell_2);
                    this.SetCell(nNo * 2 + 1, null);
                }
            }
            else
            {
                Cell cell_2 = this.GetCell(nNo * 2 + 1);
                Debug.Assert(cell_2.ParentItem != null, "");
                // 为合订格子
                this.Container.MoveMemberCellsToRight(cell_2.ParentItem);
            }
        }
#endif

        // 删除一个单格位置。如果右边是单册，则向左移动，填补空位；如果右边是合订成员位置，如果可以向左整体移动，则也移动
        // parameters:
        //      nNo 双格序号
        public void RemoveSingleIndex(int index)
        {
            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Binding, "");

            if (this.Cells.Count <= index)
                return;

            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nRet = IsBoundIndex(index);
            if (nRet == -1 || nRet == 1)
            {
                Debug.Assert(false, "不能用本函数RemoveSingleIndex()删除合订位置");
            }

            this.SetCell(index, null);

            // 向左移动同期所有单格，直到第一个合订成员双格
            for (int i = index + 1; ; i++)
            {
                if (i >= this.Cells.Count)
                    break;

                // 探测是否为合订成员占据的位置
                // return:
                //      -1  是。并且是双格的左侧位置
                //      0   不是
                //      1   是。并且是双格的右侧位置
                nRet = IsBoundIndex(i);
                if (nRet == -1)
                {
                    //      -1  是。并且是双格的左侧位置
                    Cell member = this.GetCell(i + 1);
                    Debug.Assert(member != null, "");
                    Debug.Assert(member.ParentItem != null, "");

                    Cell parent_cell = member.ParentItem.ContainerCell;
                    if (this.Container.CanMoveToLeft(parent_cell) == true)
                    {
                        this.Container.MoveCellsToLeft(parent_cell);
                    }
                    break;
                }
                else if (nRet == 1)
                {
                    //      1   是。并且是双格的右侧位置
                    Debug.Assert(false, "");
                    break;
                }


                Cell cell_1 = this.GetCell(i);
                this.SetCell(i - 1, cell_1);
                this.SetCell(i, null);
            }
        }

#if OLD_VERSION

        // 删除一个双格位置。如果右边是单册，则向左移动，填补空位；如果右边是合订成员位置，如果可以向左整体移动，则也移动
        // parameters:
        //      nNo 双格序号
        public void RemovePosition(int nNo)
        {
            if (this.Cells.Count <= nNo * 2)
                return;

            this.SetCell(nNo * 2, null);
            this.SetCell((nNo * 2)+1, null);

            // 向左移动所有单册双格，直到第一个合订成员双格
            for (int i = nNo + 1; ; i++)
            {
                if (i * 2 >= this.Cells.Count)
                    break;

                if (IsBindedPosition(i) == true)
                {
                    Cell member = this.GetCell((i * 2) + 1);
                    Debug.Assert(member != null, "");
                    Debug.Assert(member.ParentItem != null, "");

                    Cell parent_cell = member.ParentItem.ContainerCell;
                    if (this.Container.CanMoveToLeft(parent_cell) == true)
                    {
                        this.Container.MoveCellsToLeft(parent_cell);
                    }
                    break;
                }

                Cell cell_1 = this.GetCell(i * 2);
                Cell cell_2 = this.GetCell((i * 2)+ 1);

                this.SetCell((i * 2) - 2, cell_1);
                this.SetCell((i * 2)+1 - 2, cell_2);

                this.SetCell((i * 2), null);
                this.SetCell((i * 2) + 1, null);
            }
        }

#endif

        // 复制一个单格到别处
        // 本功能比较原始，不负责挤压空位
        // parameters:
        //      nSourceIndex    源index位置。注意，必须是双格的左侧
        //      nTargetIndex    目标index位置。注意，必须是双格的左侧
        public void CopySingleIndexTo(
            int nSourceIndex,
            int nTargetIndex,
            bool bClearSource)
        {
            string strError = "";
            if (nSourceIndex == nTargetIndex)
            {
                strError = "源和目标不能是同一个";
                throw new Exception(strError);
            }

            Cell source_cell_1 = this.GetCell(nSourceIndex);

            this.SetCell(nTargetIndex,
                source_cell_1);

            if (bClearSource == true)
            {
                if (nSourceIndex != nTargetIndex)
                {
                    this.SetCell(nSourceIndex,
                        null);
                }
            }
        }

        // 复制一个双格到别处
        // 本功能比较原始，不负责挤压空位
        // parameters:
        //      nSourceIndex    源index位置。注意，必须是双格的左侧
        //      nTargetIndex    目标index位置。注意，必须是双格的左侧
        public void CopyDoubleIndexTo(
            int nSourceIndex,
            int nTargetIndex,
            bool bClearSource)
        {
            string strError = "";

            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Binding, "");

            if (nSourceIndex == nTargetIndex)
            {
                strError = "源和目标不能是同一个";
                throw new Exception(strError);
            }

            Cell source_cell_1 = this.GetCell(nSourceIndex);
            Cell source_cell_2 = this.GetCell(nSourceIndex + 1);

            this.SetCell(nTargetIndex,
                source_cell_1);
            this.SetCell(nTargetIndex + 1,
                source_cell_2);

            if (bClearSource == true)
            {
                if (nSourceIndex != nTargetIndex
                    && nSourceIndex != nTargetIndex + 1)
                {
                    this.SetCell(nSourceIndex,
                        null);
                }
                if (nSourceIndex + 1 != nTargetIndex
                    && nSourceIndex + 1 != nTargetIndex + 1)
                {
                    this.SetCell(nSourceIndex + 1,
                        null);
                }
            }
        }

#if OLD_VERSION
        // 复制一个双格到别处
        // 本功能比较原始，不负责挤压空位
        public void CopyPositionTo(
            int nSourceNo,
            int nTargetNo,
            bool bClearSource)
        {
            string strError = "";
            if (nSourceNo == nTargetNo)
            {
                strError = "源和目标不能是同一个";
                throw new Exception(strError);
            }

            Cell source_cell_1 = this.GetCell(nSourceNo * 2);
            Cell source_cell_2 = this.GetCell((nSourceNo * 2) + 1);

            this.SetCell(nTargetNo * 2,
                source_cell_1);
            this.SetCell((nTargetNo * 2) + 1,
                source_cell_2);

            if (bClearSource == true)
            {
                this.SetCell(nSourceNo * 2,
                    null);
                this.SetCell((nSourceNo * 2) + 1,
                    null);
            }
        }
#endif

        // 将一个单格移动(挤入)到另一个位置。然后，如果必要，腾出来的源位置被挤压
        // 操作过程不删除任何有内容的格子
        // return:
        //      -1  出错
        //      0   最大单元数没有改变
        //      1   最大单元数发生改变
        public int MoveSingleIndexTo(
    int nSourceIndex,
    int nTargetIndex,
    out string strError)
        {
            strError = "";

            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Binding, "只能在Binding布局下使用MoveSingleIndexTo()函数");

            if (nSourceIndex == nTargetIndex)
            {
                strError = "源和目标不能是同一个";
                return -1;
            }

#if DEBUG
            // 探测是否为合订成员占据的位置
            // return:
            //      -1  是。并且是双格的左侧位置
            //      0   不是
            //      1   是。并且是双格的右侧位置
            int nRet = IsBoundIndex(nSourceIndex);
            if (nRet == -1 || nRet == 1)
            {
                Debug.Assert(false, "不能用来移动来自合订范围的格子");
            }
#endif



            // source小于target，拷贝完再删除源。因为源的位置不会变化
            // source大于target，删除完源再插入复制到target位置。因为晚了去删除源，源的位置已经发生了变化

            int nOldMaxCells = this.Cells.Count;

            Cell source_cell = this.GetCell(nSourceIndex);

            // 优化
            if (nSourceIndex + 1 == nTargetIndex
                || nSourceIndex == nTargetIndex + 1)
            {
                // 特殊情况。这是意图要将两者交换

                // source already in temp

                // target --> source
                this.SetCell(nSourceIndex,
                    this.GetCell(nTargetIndex));

                // temp --> target
                this.SetCell(nTargetIndex,
                    source_cell);

                // this.Container.Invalidate();
                if (nOldMaxCells != this.Cells.Count)
                    return 1;
                return 0;
            }

            // source大于target，删除完源再插入复制到target位置。因为晚了去删除源，源的位置已经发生了变化
            if (nSourceIndex > nTargetIndex)
            {
                // 清除旧位置
                this.RemoveSingleIndex(nSourceIndex);
            }


            // 可能会改变格局，nSourceNo会变得无效
            // this.GetNewDoubleIndex(nTargetIndex);
            this.GetNewSingleIndex(nTargetIndex);   // 2010/3/12

            /*
            {
                // 重新确定nSourceNo
                int nTemp = -1;

                Debug.Assert(source_cell_1 != null || source_cell_2 != null, "源的两个格子，不可能都为null");

                if (source_cell_1 != null)
                {
                    nTemp = this.IndexOfCell(source_cell_1);
                }
                if (nTemp == -1 && source_cell_2 != null)
                {
                    nTemp = this.IndexOfCell(source_cell_2);
                    if (nTemp != -1)
                        nTemp--;
                }

                if (nTemp != -1)
                    nSourceIndex = nTemp;
                else
                    nSourceIndex = -1; // 找不到了?
            }
             * */

            // 设置到新位置
            this.SetCell(nTargetIndex,
                source_cell);

            // source小于target，拷贝完再删除源。因为源的位置不会变化
            if (nSourceIndex < nTargetIndex)
            {
                if (nSourceIndex != -1)
                {
                    // 清除旧位置
                    this.RemoveSingleIndex(nSourceIndex);
                }
            }

            // TODO: 最好是最大个数真正有变化时才调用。一般情况仅仅刷新当前issue范围
            // this.Container.AfterWidthChanged(true);
            if (nOldMaxCells != this.Cells.Count)
                return 1;
            return 0;
        }

        // 将一个单格移动(挤入)到另一个位置。
        // 如果目标位置是订购组中，则新增一个格子；如果在计划外区域，则尽可能占用空白格子
        // 腾出来的源位置，如果在订购组中，变为预测状态；如果在计划外区间，则被挤压
        // 操作过程不删除任何有内容的格子
        // return:
        //      -1  出错
        //      0   最大单元数没有改变
        //      1   最大单元数发生改变
        public int MoveCellTo(
    int nSourceIndex,
    int nTargetIndex,
    out string strError)
        {
            strError = "";

            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Accepting, "只能在Accepting布局下使用MoveCellTo()函数");

            if (nSourceIndex == nTargetIndex)
            {
                strError = "源和目标不能是同一个";
                return -1;
            }
            int nOldMaxCells = this.Cells.Count;

            // 看看源位置是否在订购组中
            Cell source_cell = this.GetCell(nSourceIndex);
            if (source_cell is GroupCell)
            {
                strError = "订购组格子不能被移动";
                return -1;
            }

            GroupCell source_group = null;
            if (source_cell.item != null)
            {
                if (source_cell.item.Calculated == true
                    || source_cell.item.OrderInfoPosition.X != -1)
                {
                    source_group = source_cell.item.GroupCell;
                }
            }

            // source小于target，拷贝完再删除源。因为源的位置不会变化
            // source大于target，删除完源再插入复制到target位置。因为晚了去删除源，源的位置已经发生了变化

            // 优化
            if (nSourceIndex + 1 == nTargetIndex
                || nSourceIndex == nTargetIndex + 1)
            {
                // 特殊情况。这是意图要将两者交换

                // source already in temp

                // target --> source
                this.SetCell(nSourceIndex,
                    this.GetCell(nTargetIndex));

                // temp --> target
                this.SetCell(nTargetIndex,
                    source_cell);

                goto END1;
            }

            // source大于target，删除完源再插入复制到target位置。因为晚了去删除源，源的位置已经发生了变化
            if (nSourceIndex > nTargetIndex)
            {
                // 清除旧位置
                if (this.Cells.Count > nSourceIndex)
                    this.Cells.RemoveAt(nSourceIndex);
            }


            // 可能会改变格局，nSourceNo会变得无效
            // 在target位置增加一个格子
            if (this.Cells.Count > nTargetIndex)
                this.Cells.Insert(nTargetIndex, null);


            // 设置到新位置
            this.SetCell(nTargetIndex,
                source_cell);

            // source小于target，拷贝完再删除源。因为源的位置不会变化
            if (nSourceIndex < nTargetIndex)
            {
                if (nSourceIndex != -1)
                {
                    // 清除旧位置
                    if (this.Cells.Count > nSourceIndex)
                        this.Cells.RemoveAt(nSourceIndex);
                }
            }

        END1:
            // 刷新订购信息
            /*
            if (source_group != null)
            {
                // source group必须先刷新一次，否则无法通过移动位置以后的source_cell得到target_group
                source_group.RefreshGroupMembersOrderInfo(0, 0);
            }
             * */

            int nNewTargetIndex = this.IndexOfCell(source_cell);
            Debug.Assert(nNewTargetIndex != -1, "");
            GroupCell target_group = this.BelongToGroup(nNewTargetIndex);


            // 如果源被从组区域拖动到计划外区域
            if (source_group != null && target_group == null)
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
            if (source_group == null && target_group != null)
            {
                if (source_cell.item == null)
                {
                    // 空白格子要变为预测格式
                    source_cell.item = new ItemBindingItem();
                    source_cell.item.Container = this;
                    source_cell.item.Initial("<root />", out strError);
                    source_cell.item.RefID = "";
                    source_cell.item.LocationString = "";
                    source_cell.item.Calculated = true;
                    SetFieldValueFromOrderInfo(
                        false,
                        source_cell.item,
                        target_group.order);
                    // 如果必要，填充出版时间
                    if (String.IsNullOrEmpty(source_cell.item.PublishTime) == true)
                        source_cell.item.PublishTime = this.PublishTime;
                    if (String.IsNullOrEmpty(source_cell.item.Volume) == true)
                    {
                        string strVolumeString =
VolumeInfo.BuildItemVolumeString(
IssueUtil.GetYearPart(this.PublishTime),
this.Issue,
this.Zong,
this.Volume);
                        source_cell.item.Volume = strVolumeString;
                    }
                }

                // order xy 后面自然会设置
            }

            int nSourceOrderCountDelta = 0;
            int nSourceArrivedCountDelta = 0;
            if (source_group != null && source_group != target_group)
            {
                nSourceOrderCountDelta--;
                if (source_cell.item != null
                    && source_cell.item.Calculated == false)
                    nSourceArrivedCountDelta--;

                source_group.RefreshGroupMembersOrderInfo(nSourceOrderCountDelta,
    nSourceArrivedCountDelta);
            }
            int nTargetOrderCountDelta = 0;
            int nTargetArrivedCountDelta = 0;
            if (target_group != null && source_group != target_group)
            {
                nTargetOrderCountDelta++;
                if (source_cell.item != null
                    && source_cell.item.Calculated == false)
                    nTargetArrivedCountDelta++;
                target_group.RefreshGroupMembersOrderInfo(nTargetOrderCountDelta,
    nTargetArrivedCountDelta);

            }

            if (source_group == target_group
                && source_group != null)
            {
                // 在一个组内拖动
                source_group.RefreshGroupMembersOrderInfo(0,
                    0);
            }


            // TODO: 最好是最大个数真正有变化时才调用。一般情况仅仅刷新当前issue范围
            // this.Container.AfterWidthChanged(true);
            if (nOldMaxCells != this.Cells.Count)
                return 1;
            return 0;
        }

#if OLD_VERSION
        // 将一个双格移动(挤入)到另一个双格位置。然后，如果必要，腾出来的源位置被挤压
        // 操作过程不删除任何有内容的双格
        // return:
        //      -1  出错
        //      0   最大单元数没有改变
        //      1   最大单元数发生改变
        public int MovePositionTo(
    int nSourceNo,
    int nTargetNo,
    out string strError)
        {
            strError = "";

            if (nSourceNo == nTargetNo)
            {
                strError = "源和目标不能是同一个";
                return -1;
            }
            /*
            if (this.IsBindedPosition(nSourceNo) == true)
            {
                strError = "源不能是成员册";
                return -1;
            }
            if (this.IsBindedPosition(nTargetNo) == true)
            {
                strError = "目标不能是成员册";
                return -1;
            }
             * */
            int nOldMaxCells = this.Cells.Count;

            Cell source_cell_1 = this.GetCell(nSourceNo * 2);
            Cell source_cell_2 = this.GetCell((nSourceNo * 2) + 1);

            // 优化
            if (nSourceNo + 1 == nTargetNo
                || nSourceNo == nTargetNo + 1)
            {
                // 特殊情况。这是意图要将两者交换

                // source already in temp

                // target --> source
                this.SetCell(nSourceNo * 2,
                    this.GetCell(nTargetNo * 2));
                this.SetCell((nSourceNo * 2) + 1,
                    this.GetCell((nTargetNo * 2) + 1));

                // temp --> target
                this.SetCell(nTargetNo * 2,
                    source_cell_1);
                this.SetCell((nTargetNo * 2) + 1,
                    source_cell_2);

                // this.Container.Invalidate();
                if (nOldMaxCells != this.Cells.Count)
                    return 1;
            }


            // 可能会改变格局，nSourceNo会变得无效
            this.GetNewPosition(nTargetNo);

            {
                // 重新确定nSourceNo
                int nTemp = -1;

                Debug.Assert(source_cell_1 != null || source_cell_2 != null, "源的两个格子，不可能都为null");

                if (source_cell_1 != null)
                    nTemp = this.IndexOfCell(source_cell_1);
                if (nTemp == -1 && source_cell_2 != null)
                    nTemp = this.IndexOfCell(source_cell_2);

                if (nTemp != -1)
                    nSourceNo = nTemp / 2;
                else
                    nSourceNo = -1; // 找不到了?
            }


            // 设置到新位置
            this.SetCell(nTargetNo * 2,
                source_cell_1);
            this.SetCell((nTargetNo * 2) + 1,
                source_cell_2);


            if (nSourceNo != -1)
            {
                // 清除旧位置
                this.RemovePosition(nSourceNo);
            }

            // TODO: 最好是最大个数真正有变化时才调用。一般情况仅仅刷新当前issue范围
            // this.Container.AfterWidthChanged(true);
            if (nOldMaxCells != this.Cells.Count)
                return 1;
            return 0;
        }
#endif

        public CellBase GetFirstCell()
        {
            if (this.Cells.Count == 0
                || this.Cells[0] == null)
                return new NullCell(0, this.Container.Issues.IndexOf(this));

            return this.Cells[0];
        }

        // 刷新<copy>元素里面的订购/已到值
        public int RefreshOrderCopy(int x,
            out string strError)
        {
            strError = "";

            if (x >= this.OrderItems.Count)
            {
                Debug.Assert(false, "x >= this.OrderItems.Count["+this.OrderItems.Count.ToString()+"]");
                return -1;
            }
            OrderBindingItem order = this.OrderItems[x];
            // return:
            //      -1  出错
            //      0   没有发生修改
            //      1   发生了修改
            int nRet = order.RefreshOrderCopy(
                    false,
                    out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // Binding布局?下刷新OrderInfoPosition
        // 在属于x的那些格子中，y为start_Y以上的增量nDelta
        public void RefreshOrderInfoPositionXY(int x,
            int start_y,
            int nDeltaY)
        {
            // Debug.Assert(this.IssueLayoutState == IssueLayoutState.Binding, "");

            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null || cell.item == null)
                    continue;
                if (cell.item.OrderInfoPosition.X == -1)
                    continue;
                if (cell.item.OrderInfoPosition.X == x)
                {
                    if (cell.item.OrderInfoPosition.Y >= start_y)
                        cell.item.OrderInfoPosition.Y += nDeltaY;
                }
            }
        }

        // 清除当前对象本身以及全部下级的选择标志, 并返回需要刷新的对象
        public void ClearAllSubSelected(ref List<CellBase> objects,
            int nMaxCount)
        {
            if (this.Selected == true)
            {
                this.Selected = false;
                // 状态修改过的才加入数组
                if (objects.Count < nMaxCount)
                    objects.Add(this);
            }

            for (int i = 0; i < this.Cells.Count; i++)
            {
                CellBase cell = this.Cells[i];
                if (cell != null)
                {
                    if (cell.Selected == true)
                    {
                        cell.Selected = false;
                        // 状态修改过的才加入数组
                        if (objects.Count < nMaxCount)
                            objects.Add(cell);
                    }
                }
            }
        }

        // 确定当前已装订册占据的列(区域)之右，第一个可用于插入单格子的列。从右靠左。
        // 注意返回的是单格index数
        internal int GetFirstAvailableSingleInsertIndex()
        {
            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Binding, "");
            int nMax = this.Cells.Count - 1;

            int index = -1;
            for (int i = nMax; i >= 0; i--)
            {
                // 探测是否为合订成员占据的位置
                // return:
                //      -1  是。并且是双格的左侧位置
                //      0   不是
                //      1   是。并且是双格的右侧位置
                int nRet = IsBoundIndex(i);
                if (nRet != 0)
                    break;

                index = i;
            }

            if (index == -1)
            {
                Debug.Assert(nMax + 1 >= 0, "");
                return (nMax + 1);
            }

            return index;
        }

        // 得到所有采购组以外(右)的第一个可用的index
        internal int GetFirstFreeIndex()
        {
            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Accepting, "");
            int nMax = this.Cells.Count - 1;

            int index = -1;
            for (int i = nMax; i >= 0; i--)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                {
                    index = i;
                    continue;
                }

                if (cell is GroupCell)
                    break;

                index = i;
            }

            if (index == -1)
            {
                Debug.Assert(nMax + 1 >= 0, "");
                return (nMax + 1);
            }

            return index;
        }

        // 得到所有采购组以外(右)的第一个空白或者null的index
        internal int GetFirstFreeBlankIndex()
        {
            Debug.Assert(this.IssueLayoutState == IssueLayoutState.Accepting, "");

            int nMax = this.Cells.Count - 1;
            int nFreeIndex = GetFirstFreeIndex();

            for (int i = nFreeIndex; i <=nMax; i++)
            {
                Cell cell = this.Cells[i];
                if (IsBlankOrNullCell(cell) == true)
                {
                    // 2010/3/29
                    if (cell != null && cell.IsMember == true)
                        continue;   // 避免占用缺期成员册
                    return i;
                }
            }

            return this.Cells.Count;
        }

        // 是否为空白或者(无)格子
        internal static bool IsBlankOrNullCell(Cell cell)
        {
            if (cell == null)
                return true;
            if (cell.item == null)
                return true;
            return false;
        }

        // 2010/2/28
        // ????? 有问题
        // 探测当前已装订册占据的列之外，第一个可用的列(单格编号)。靠左。
        // 注意，所返回的index可能超过现有Cells数组的规格
        internal int GetFirstAvailableBoundColumn()
        {
            for (int i = 0; i < this.Cells.Count; i++)
            {
                if (IsBlankDoubleIndex(i) == true)
                    return i;
                /*
                Cell cell = this.Cells[i];
                bool bBlank = IsBlankOrNullCell(cell);

                if (bBlank == true)
                {
                    // 还要判断右侧是否为合订成员格子
                    Cell cellRight = this.GetCell(i + 1);
                    if (cell == null || cell.IsMember == false)
                        return i;
                }
                 * */
            }

            return this.Cells.Count;
        }

#if OLD_VERSION

        // 确定当前已装订册占据的列之外，第一个可用的列。靠左。
        // 注意，所返回的index可能超过现有Cells数组的规格
        // 注：要求纵向即便是空白的位置，也应该有Cell，表明被合订范围占据? 否则判断起来就麻烦了
        internal int GetFirstAvailableBindingColumn()
        {
            // 奇数位置如果有了，就表明是合订本
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if ((i % 2) == 0)
                {
                    // 奇数位置
                    bool bBlank = false;
                    if (cell == null)
                        bBlank = true;

                    // 2010/2/16 changed
                    if (cell != null && cell.item == null)
                    {
                        if (cell.ParentItem == null)
                            bBlank = true;
                    }


                    if (bBlank == false)
                        continue;

                    // 追加判断偶数位置
                    Cell right_cell = null;
                    if (i+1<this.Cells.Count)
                        right_cell = this.Cells[i+1];

                    if (right_cell != null)
                    {
                        if (right_cell.Binded == false)
                            return i;
                    }
                }
                else
                {
                    // 偶数位置
                }
            }
            if ((this.Cells.Count % 2) == 0)
                return this.Cells.Count;
            return this.Cells.Count + 1;
        }
#endif

        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<CellBase> update_objects,
            int nMaxCount)
        {
            float x0 = this.Container.m_nLeftTextWidth;

            // 先看看整体上有没有交会
            RectangleF rectAll = new RectangleF(0,
                0,
                x0 + (this.Container.m_nCellWidth * this.Cells.Count),
                this.Container.m_nCellHeight);
            if (rectAll.IntersectsWith(rect) == false)
                return;

            // 左边标题部分。代表Issue对象
            RectangleF rectLeftText = new RectangleF(0,
                0,
                x0,
                this.Container.m_nCellHeight);
            if (rectLeftText.IntersectsWith(rect) == true)
            {
                bool bRet = this.Select(action);
                if (bRet == true && update_objects.Count < nMaxCount)
                {
                    update_objects.Add(this);
                }
                return;
            }

            // 先看看整体上有没有交会
            rectAll = new RectangleF(x0,
                0,
                this.Container.m_nCellWidth * this.Cells.Count,
                this.Container.m_nCellHeight);
            if (rectAll.IntersectsWith(rect) == false)
                return;


            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    goto CONTINUE;

                RectangleF rectCell = new RectangleF(x0,
                    0,
                    this.Container.m_nCellWidth,
                    this.Container.m_nCellHeight);


                if (rectCell.IntersectsWith(rect) == true)
                {
                    bool bRet = cell.Select(action);
                    if (bRet == true && update_objects.Count < nMaxCount)
                    {
                        update_objects.Add(cell);
                    }
                }
            CONTINUE:
                x0 += this.Container.m_nCellWidth;
            }
        }

        // 是否有单元被选择?
        public bool HasCellSelected()
        {
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell != null)
                {
                    if (cell.Selected == true)
                        return true;
                }
            }

            return false;
        }

        public List<Cell> SelectedCells
        {
            get
            {
                List<Cell> results = new List<Cell>();
                for (int i = 0; i < this.Cells.Count; i++)
                {
                    Cell cell = this.Cells[i];
                    if (cell != null)
                    {
                        if (cell.Selected == true)
                            results.Add(cell);
                    }
                }

                return results;
            }
        }

        // 初始化期信息，然后初始化下方的册信息
        // parameters:
        //      strXml  期记录XML
        //      bLoadItems  是否利用外部接口装载下属的册对象到Items数组中
        public int Initial(string strXml,
            bool bLoadItems,
            out string strError)
        {
            strError = "";

            // this.Xml = strXml;
            this.dom = new XmlDocument();
            try
            {
                this.dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "期记录 XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            Debug.Assert(this.Container != null, "");

            if (bLoadItems == true)
            {
                // TODO: this.PublishTime为空怎么办？
                // TODO: 不同的期有交叉拥有的册怎么办？是否要限定出版日期的位数和格式，还有检查不同的期行是否有相同的出版日期

                // 装载期下属的册对象
                int nRet = LoadItems(this.PublishTime,
                    out strError);
                if (nRet == -1)
                    return -1;
            }



            return 0;
        }



        #region 数据成员

        public string PublishTime
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "publishTime");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "publishTime", value);
            }
        }

        public string Issue
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "issue");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "issue", value);
            }
        }

        public string Volume
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "volume");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "volume", value);
            }
        }

        public string Zong
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "zong");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "zong", value);
            }
        }

        // 2010/3/28
        public string Comment
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "comment");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "comment", value);
            }
        }

        public string OrderInfo
        {
            get
            {
                if (this.dom == null)
                    return "";

                // 如果有多个<orderInfo>元素，要删除后面的
                {
                    XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo");
                    if (nodes.Count > 1)
                    {
                        for (int i = 1; i < nodes.Count; i++)
                        {
                            XmlNode node = nodes[i];
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }

                return DomUtil.GetElementInnerXml(this.dom.DocumentElement,
                    "orderInfo");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("dom尚未初始化");

                // 如果有多个<orderInfo>元素，要删除后面的
                {
                    XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo");
                    if (nodes.Count > 1)
                    {
                        for (int i = 1; i < nodes.Count; i++)
                        {
                            XmlNode node = nodes[i];
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }

                DomUtil.SetElementInnerXml(this.dom.DocumentElement,
                    "orderInfo",
                    value);
            }
        }

        public string RefID
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "refID");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "refID", value);
            }
        }

        // 2010/4/7
        public string Operations
        {
            get
            {
                if (this.dom == null)
                    return "";

                // 如果有多个<operations>元素，要删除后面的
                {
                    XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("operations");
                    if (nodes.Count > 1)
                    {
                        for (int i = 1; i < nodes.Count; i++)
                        {
                            XmlNode node = nodes[i];
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }

                return DomUtil.GetElementInnerXml(this.dom.DocumentElement,
                    "operations");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("dom尚未初始化");

                // 如果有多个<operations>元素，要删除后面的
                {
                    XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("operations");
                    if (nodes.Count > 1)
                    {
                        for (int i = 1; i < nodes.Count; i++)
                        {
                            XmlNode node = nodes[i];
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }

                DomUtil.SetElementInnerXml(this.dom.DocumentElement,
                    "operations",
                    value);
            }
        }

        #endregion

        // 设置或者刷新一个操作记载
        // 可能会抛出异常
        public void SetOperation(
            string strAction,
            string strOperator,
            string strComment)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<operations />");

            string strInnerXml = this.Operations;
            if (String.IsNullOrEmpty(strInnerXml) == false)
            {
                dom.DocumentElement.InnerXml = this.Operations;
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("operation[@name='" + strAction + "']");
            if (node == null)
            {
                node = dom.CreateElement("operation");
                dom.DocumentElement.AppendChild(node);
                DomUtil.SetAttr(node, "name", strAction);
            }

            DomUtil.SetAttr(node, "time", DateTimeUtil.Rfc1123DateTimeString(DateTime.Now.ToUniversalTime()));
            DomUtil.SetAttr(node, "operator", strOperator);
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(node, "comment", strComment);

            this.Operations = dom.DocumentElement.InnerXml;
        }

        // 获得册参考ID列表
        public int GetItemRefIDs(out List<string> ids,
            out string strError)
        {
            strError = "";
            ids = new List<string>();

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*/distribute");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strDistribute = node.InnerText.Trim();
                if (String.IsNullOrEmpty(strDistribute) == true)
                    continue;

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(strDistribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location location = locations[j];

                    // 尚未创建过的事项，跳过
                    if (location.RefID == "*"
                        || String.IsNullOrEmpty(location.RefID) == true)
                        continue;

                    ids.Add(location.RefID);
                }
            }

            return 0;
        }



        // IssueBindingItem 点击测试
        // parameters:
        //      p_x   已经是文档坐标。即文档左上角为(0,0)
        //      type    要测试的最下级（叶级）对象的类型。如果为null，表示一直到末端
        public void HitTest(long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = new HitTestResult();

            if (dest_type == typeof(IssueBindingItem))
            {
                result.AreaPortion = AreaPortion.Content;
                result.X = p_x;
                result.Y = p_y;
                result.Object = this;
                return;
            }

            if (p_x < this.Container.m_nLeftTextWidth)
            {
                result.AreaPortion = AreaPortion.LeftText;
                result.X = p_x;
                result.Y = p_y;
                result.Object = this;
                return;
            }

            p_x -= this.Container.m_nLeftTextWidth;
            long x0 = 0;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (p_x >= x0 && p_x < x0 + this.Container.m_nCellWidth)
                {
                    if (cell == null)
                    {
                        if (dest_type == typeof(NullCell))
                        {
                            result.AreaPortion = AreaPortion.Content;
                            //result.X = i;   // cell在数组中的index位置
                            //result.Y = this.Container.Issues.IndexOf(this); // issue的index位置
                            result.Object = new NullCell(i, this.Container.Issues.IndexOf(this));
                            return;
                        }

                        result.AreaPortion = AreaPortion.Blank; // 空白部分
                        result.X = p_x;
                        result.Y = p_y;
                        result.Object = null;
                        return;
                    }

                    /*
                    result.AreaPortion = AreaPortion.Content;
                    result.X = p_x;
                    result.Y = p_y;
                    result.Object = cell;
                     * */
                    cell.HitTest(p_x - x0,
                        p_y,
                        dest_type,
                        out result);
                    return;
                }

                x0 += this.Container.m_nCellWidth;
            }

            if (dest_type == typeof(NullCell))
            {
                result.AreaPortion = AreaPortion.Content;
                result.Object = new NullCell((int)(p_x / this.Container.m_nCellWidth),
                    this.Container.Issues.IndexOf(this));
                return;
            }

            result.AreaPortion = AreaPortion.Blank; // 空白部分
            result.X = p_x;
            result.Y = p_y;
            result.Object = null;
        }

        // 获得订购/到达数量字符串
        // return:
        //      -2  全缺
        //      -1  缺
        //      0   到齐
        //      1   超出。比到齐还要多
        int GetOrderAndArrivedCountString(out string strResult)
        {
            bool bMissing = false;
            bool bOverflow = false;
            int nTotalOrderCopy = 0;
            int nTotalArrivedCopy = 0;
            for (int i = 0; i < this.OrderItems.Count; i++)
            {
                OrderBindingItem order = this.OrderItems[i];
                string strOrderCopy = IssueBindingItem.GetOldValue(order.Copy);
                string strArrivedCopy = IssueBindingItem.GetNewValue(order.Copy);

                int nOrderCopy = IssueBindingItem.GetNumberValue(strOrderCopy);
                int nArrivedCopy = IssueBindingItem.GetNumberValue(strArrivedCopy);

                if (nArrivedCopy < nOrderCopy)
                    bMissing = true;
                if (nArrivedCopy > nOrderCopy)
                    bOverflow = true;

                nTotalOrderCopy += nOrderCopy;
                nTotalArrivedCopy += nArrivedCopy;
            }

            int nState = 0;
            if (nTotalArrivedCopy == 0)
                nState = -2;
            else if (bMissing == false && bOverflow == true)
            {
                Debug.Assert(nTotalArrivedCopy > nTotalOrderCopy, "");
                nState = 1;
            }
            else if (bMissing == true)
            {
                // 注意，总数可能超过总订购数。但是只要某一个部分不足，全部就算不足
                nState = -1;
            }
            else
            {
                Debug.Assert(nTotalArrivedCopy >= nTotalOrderCopy, "");
                nState = 0;
            }

            strResult = nTotalArrivedCopy.ToString() + "/" + nTotalOrderCopy.ToString();

            return nState;
        }

        // 获得计划外区域到达的数字。
        // 不包含空白格子、合订册格子
        int GetFreeArrivedCount()
        {
            int nCount = 0;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                Cell cell = this.Cells[i];
                if (cell == null)
                    continue;
                if (cell is GroupCell)
                    continue;
                if (cell.item == null)
                    continue;
                if (cell.item.OrderInfoPosition.X != -1)
                    continue;
                if (cell.item.Calculated == true)
                    continue;
                if (cell.item.IsParent == true)
                    continue;
                nCount++;
            }

            return nCount;
        }

        // 绘制一个期行
        internal void Paint(
            int nLineNo,
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            if (BindingControl.TooLarge(start_x) == true
    || BindingControl.TooLarge(start_y) == true)
                return;

            int x0 = (int)start_x;
            int y0 = (int)start_y;

            RectangleF rect;

            // 绘制第一栏，年期卷文字
            rect = new RectangleF(
                x0,
                y0,
                this.LeftTextWidth,
                this.Height);

            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true)
            {
                /*
                if (this.Selected == true)
                {
                    // 选择后的背景
                    RectangleF rect1 = new RectangleF(
                       x0,
                       y0,
                       this.LeftTextWidth,
                        this.Height);
                    Brush brush = new SolidBrush(this.Container.SelectedBackColor);
                    e.Graphics.FillRectangle(brush,
                        rect1);
                }
                else
                {
                    // 右边的竖线
                    e.Graphics.DrawLine(new Pen(Color.LightGray, (float)1),
                        new PointF(x0 + this.LeftTextWidth, y0),
                        new PointF(x0 + this.LeftTextWidth, y0 + this.Height));
                }
                 * */

                PaintLeftTextArea(
                    nLineNo,
                    x0,
                    y0,
                    this.LeftTextWidth,
                    (int)this.Height,
                    e);

            }

            int x = x0;
            int y = y0;

            // 绘制月份
            x = x0 + this.LeftTextWidth;
            y = y0;

            NullCell null_cell = null;
            if (this.Container.FocusObject is NullCell)
            {
                null_cell = (NullCell)this.Container.FocusObject;
                if (null_cell != null)
                {
                    if (null_cell.Y != nLineNo)
                        null_cell = null;
                    else if (null_cell.X >= this.Container.m_nMaxItemCountOfOneIssue)
                        null_cell = null;   // 超过右边极限范围的不要显示
                }
            }

            // 优化
            int nStartIndex = (e.ClipRectangle.Left - x) / this.Container.m_nCellWidth;
            nStartIndex = Math.Max(0, nStartIndex);
            x += this.Container.m_nCellWidth * nStartIndex;

            // 对各个册进行循环，画出格子
            for (int i = nStartIndex; i < this.Cells.Count; i++)
            {
                // 优化
                if (x > e.ClipRectangle.Right)
                    break;

                Cell cell = this.Cells[i];
                if (cell != null)
                {
                    cell.Paint(x, y, e);

                    if (null_cell != null)
                    {
                        if (null_cell.X == i)
                            null_cell = null;   // 已经被正常绘制过了
                    }
                }
                x += this.Container.m_nCellWidth;
            }

            if (null_cell != null)
            {
                null_cell.Paint(
                    this.Container,
                    x0 + this.LeftTextWidth + this.Container.m_nCellWidth * null_cell.X,
                    y0,
                    e);
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
                if (rect.IntersectsWith(e.ClipRectangle) == true)
                {
                    ControlPaint.DrawFocusRectangle(e.Graphics,
                    Rectangle.Round(rect));
                }
            }
        }

        // 绘制一个期的左侧文字部分。包括年期卷等信息
        void PaintLeftTextArea(
            int nLineNo,
            int start_x,
            int start_y,
            int nWidth11,
            int nHeight11,
            PaintEventArgs e)
        {
            Pen penBorder = new Pen(Color.FromArgb(255, Color.LightGray), (float)1);

            Rectangle rectFrame = new Rectangle(start_x, start_y, nWidth11, nHeight11);

            Brush brushBack = null;
            if (this.Selected == true)
            {
                // 选择后的背景
                /*
                RectangleF rect1 = new RectangleF(
                   x0,
                   y0,
                   this.LeftTextWidth,
                    this.Height);
                 * */
                brushBack = new SolidBrush(this.Container.SelectedBackColor);
            }
            else
            {
                brushBack = new SolidBrush(this.Container.IssueBoxBackColor);
            }

            /*
            e.Graphics.FillRectangle(brushBack,
                    rectFrame);
             * */

            /*
            // 左方竖线
            e.Graphics.DrawLine(penBorder,
                new PointF(rectFrame.X, rectFrame.Y),
                new PointF(rectFrame.X, rectFrame.Y + rectFrame.Height)
                );

            // 上方横线
            e.Graphics.DrawLine(penBorder,
                new PointF(rectFrame.X, rectFrame.Y),
                new PointF(rectFrame.X + rectFrame.Width, rectFrame.Y)
                );

            // 右边的竖线
            e.Graphics.DrawLine(penBorder,
                new PointF(rectFrame.X+rectFrame.Width, rectFrame.Y),
                new PointF(rectFrame.X + rectFrame.Width, rectFrame.Y + rectFrame.Height)
                );

             * */
            bool bFirstLine = false;
            bool bTailLine = false;
            if (nLineNo == 0)
                bFirstLine = true;
            if (nLineNo == this.Container.Issues.Count - 1)
                bTailLine = true;
            string strMask = "++++";
            if (bFirstLine && bTailLine)
                strMask = "+rr+";
            else if (bFirstLine == true)
                strMask = "+r++";
            else if (bTailLine == true)
                strMask = "++r+";

            BindingControl.PartRoundRectangle(
                e.Graphics,
penBorder,
brushBack,
rectFrame,
10,
strMask); // 左上 右上 右下 左下

            rectFrame = GuiUtil.PaddingRect(this.Container.LeftTextMargin,
rectFrame);

            Rectangle rectContent = GuiUtil.PaddingRect(this.Container.LeftTextPadding,
    rectFrame);

            /*
            // 绘制立体背景
            BindingControl.PaintButton(e.Graphics,
            Color.Red,
            rectContent);
             * */


            int x0 = rectContent.X;
            int y0 = rectContent.Y;
            int nWidth = rectContent.Width;
            int nHeight = rectContent.Height;

            Color colorDark = this.Container.IssueBoxForeColor;
            Color colorGray = this.Container.IssueBoxGrayColor;
            Brush brushText = null;

            int nMaxWidth = nWidth;
            int nMaxHeight = nHeight;
            int nUsedHeight = 0;
            SizeF size;

            string strPublishTime = this.PublishTime;

            bool bFree = false; // 是否为自由期
            if (String.IsNullOrEmpty(strPublishTime) == true)
                bFree = true;

            bool bFirstIssue = false;
            if (this.Container.IsYearFirstIssue(this) == true)
                bFirstIssue = true;

            // 预先获得期号，以便绘制条形背景
            string strNo = "";
            string strYear = "";
            if (bFree == true)
            {
                strNo = "(自由)";
            }
            else
            {
                strYear = IssueUtil.GetYearPart(strPublishTime);
                strNo = this.Issue;
            }

            size = e.Graphics.MeasureString(strNo,
    this.Container.m_fontTitleLarge);
            if (size.Width > nMaxWidth)
                size.Width = nMaxWidth;
            if (size.Height > nMaxHeight)
                size.Height = nMaxHeight;


            // 标题背景
            if (bFirstIssue == true || bFree == true)
            {
                RectangleF rect1 = new RectangleF(
                    x0,
                    y0,
                    nMaxWidth,
                    size.Height);

                // 左下 -- 右上
                LinearGradientBrush brushGradient = new LinearGradientBrush(
new PointF(rect1.X, rect1.Y + rect1.Height),
new PointF(rect1.X + rect1.Width, rect1.Y),
this.Container.IssueBoxBackColor,
ControlPaint.Light(this.Container.IssueBoxForeColor, 0.99F)
);

                e.Graphics.FillRectangle(brushGradient,
                    rect1);
            }

            Color colorSideBar = Color.FromArgb(0, 255, 255, 255);
            //Padding margin = this.Container.LeftTextMargin;
            //Padding padding = this.Container.LeftTextPadding;

            // 新建的和发生过修改的，侧边条颜色需要设定
            if (this.NewCreated == true)
            {
                // 新创建的单册
                colorSideBar = this.Container.NewBarColor;
            }
            else if (this.Changed == true)
            {
                // 修改过的的单册
                colorSideBar = this.Container.ChangedBarColor;
            }

            {
                // 边条。左侧
                Brush brushSideBar = new SolidBrush(colorSideBar);
                RectangleF rectSideBar = new RectangleF(
    start_x,
    y0,
    Math.Max(4, this.Container.LeftTextMargin.Left),
    nMaxHeight);
                e.Graphics.FillRectangle(brushSideBar, rectSideBar);
            }

            if (this.Virtual == true)
            {

                float nLittleWidth = Math.Min(nMaxWidth,
                    nMaxHeight);

                RectangleF rectMask = new RectangleF(
                    x0 + nMaxWidth/2 - nLittleWidth/2,
                    y0 + nMaxHeight/2 - nLittleWidth/2,
                    nLittleWidth,
                    nLittleWidth);
                Cell.PaintDeletedMask(rectMask,
                    Color.LightGray,
                    e,
                    false);
            }

            // 第一行文字 年份+期号
            // 如果不是某年的第一期，则年份显示为淡色
            {


                // 1) 期号
                brushText = new SolidBrush(colorDark);
                // size里面已经有strNo的尺寸
                RectangleF rect = new RectangleF(
                    x0 + nMaxWidth - size.Width,   // 靠右
                    y0,
                    size.Width,
                    size.Height);
                float fNoLeft = x0 + nMaxWidth - size.Width;  // 记忆下这个点

                e.Graphics.FillRectangle(brushText, rect);


                Brush brushBackground = new SolidBrush(this.Container.IssueBoxBackColor);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Near;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                e.Graphics.DrawString(strNo,
                    this.Container.m_fontTitleLarge,
                    brushBackground,
                    rect,
                    stringFormat);

                // 2) 年份
                if (String.IsNullOrEmpty(strYear) == false)
                {
                    if (bFirstIssue == true)
                        brushText = new SolidBrush(colorDark);
                    else
                        brushText = new SolidBrush(colorGray);

                    size = e.Graphics.MeasureString(strYear,
                        this.Container.m_fontTitleLarge);
                    if (size.Width > nMaxWidth)
                        size.Width = nMaxWidth;
                    if (size.Height > nMaxHeight)
                        size.Height = nMaxHeight;
                    rect = new RectangleF(
                        x0,  // + 4 // 靠左
                        y0,
                        size.Width,
                        size.Height);
                    /*
                    // 如果为当年第一个期，还需要绘制淡色文字背景
                    if (bFirstIssue == true)
                    {
                        RectangleF rect1 = new RectangleF(
                            x0,   // 靠左
                            y0,
                            fNoLeft - x0,   // 把宽度修正为贯通紧靠右边的no区域
                            size.Height);
                        Brush brushRect = new SolidBrush(ControlPaint.Light(this.Container.ForeColor, 0.99F));
                        // 左上 -- 右下
                        LinearGradientBrush brushGradient = new LinearGradientBrush(
    new PointF(rect1.X, rect1.Y),
    new PointF(rect1.X + rect1.Width, rect1.Y + rect1.Height),
    Color.White,
    ControlPaint.Light(this.Container.ForeColor, 0.99F)
    );

                        e.Graphics.FillRectangle(brushGradient,
                            rect1);
                    }
                     * */

                    stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Near;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                    e.Graphics.DrawString(strYear,
                        this.Container.m_fontTitleLarge,
                        brushText,
                        rect,
                        stringFormat);
                }

                nUsedHeight += (int)size.Height;
            }

            if (nUsedHeight >= nMaxHeight)
                return;

            // 第二行文字 出版日期
            // 淡色。字体稍小
            if (bFree == false)
            {
                y0 += (int)size.Height;
                string strText = BindingControl.GetDisplayPublishTime(this.PublishTime);

                brushText = new SolidBrush(colorGray);
                size = e.Graphics.MeasureString(strText,
                    this.Container.m_fontTitleSmall);
                if (size.Width > nMaxWidth)
                    size.Width = nMaxWidth;
                if (size.Height > nMaxHeight - nUsedHeight)
                    size.Height = nMaxHeight - nUsedHeight;
                RectangleF rect = new RectangleF(
                    x0 + nMaxWidth - size.Width,   // 靠右
                    y0,
                    size.Width,
                    size.Height);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Near;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                e.Graphics.DrawString(strText,
        this.Container.m_fontTitleSmall,
        brushText,
        rect,
        stringFormat);
                nUsedHeight += (int)size.Height;
            }

            if (nUsedHeight >= nMaxHeight)
                return;

            // 第三行文字 卷号+总期号
            // 淡色。字体稍小
            if (bFree == false)
            {
                string strText = this.Comment;

                int nTrimLength = 6;
                if (strText.Length > nTrimLength)
                    strText = strText.Substring(0, nTrimLength) + "...";

                if (String.IsNullOrEmpty(this.Volume) == false)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += " ";
                    strText += "v." + this.Volume;
                }

                if (String.IsNullOrEmpty(this.Zong) == false)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += " ";
                    strText += "总." + this.Zong;
                }

                if (String.IsNullOrEmpty(strText) == false)
                {
                    y0 += (int)size.Height;
                    brushText = new SolidBrush(colorGray);
                    size = e.Graphics.MeasureString(strText,
                        this.Container.m_fontTitleSmall);
                    if (size.Width > nMaxWidth)
                        size.Width = nMaxWidth;
                    if (size.Height > nMaxHeight - nUsedHeight)
                        size.Height = nMaxHeight - nUsedHeight;
                    RectangleF rect = new RectangleF(
                        x0 + nMaxWidth - size.Width,   // 靠右
                        y0,
                        size.Width,
                        size.Height);

                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Far;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                    e.Graphics.DrawString(strText,
            this.Container.m_fontTitleSmall,
            brushText,
            rect,
            stringFormat);
                    nUsedHeight += (int)size.Height;
                }
            }

            if (nUsedHeight >= nMaxHeight)
                return;
            //
            // 第四行文字 到达/订购数字
            // 淡色。字体稍小
            if (bFree == false)
            {
                string strText = "";
                // 获得订购/到达数量字符串
                // return:
                //      -2  全缺
                //      -1  缺
                //      0   到齐
                //      1   超出。比到齐还要多
                int nState = GetOrderAndArrivedCountString(out strText);

                int nFreeCount = GetFreeArrivedCount();
                if (nFreeCount > 0)
                    strText += " + " + nFreeCount.ToString();

                if (String.IsNullOrEmpty(strText) == false)
                {
                    y0 += (int)size.Height;
                    // brushText = new SolidBrush(colorGray);
                    if (nState == -2)
                        brushText = new SolidBrush(colorGray);
                    else if (nState == -1)
                        brushText = new SolidBrush(Color.DarkRed);
                    else if (nState == 0)
                        brushText = new SolidBrush(Color.DarkGreen);
                    else
                    {
                        Debug.Assert(nState == 1, "");
                        brushText = new SolidBrush(Color.DarkOrange);
                    }

                    size = e.Graphics.MeasureString(strText,
                        this.Container.m_fontTitleSmall);
                    if (size.Width > nMaxWidth)
                        size.Width = nMaxWidth;
                    if (size.Height > nMaxHeight - nUsedHeight)
                        size.Height = nMaxHeight - nUsedHeight;
                    RectangleF rect = new RectangleF(
                        x0 + nMaxWidth - size.Width,   // 靠右
                        y0,
                        size.Width,
                        size.Height);

                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Far;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                    e.Graphics.DrawString(strText,
            this.Container.m_fontTitleSmall,
            brushText,
            rect,
            stringFormat);
                    int nImageIndex = 0;
                    if (nState == -1)
                        nImageIndex = 1;
                    else if (nState == 0 || nState == 1)
                        nImageIndex = 2;

                    ImageAttributes attr = new ImageAttributes();
                    attr.SetColorKey(this.Container.imageList_treeIcon.TransparentColor,
                        this.Container.imageList_treeIcon.TransparentColor);

                    Image image = this.Container.imageList_treeIcon.Images[nImageIndex];
                    /*
                    e.Graphics.DrawImage(image,
                        rect.X + rect.Width - size.Width - image.Width - 4,
                        rect.Y);
                     * */
                    e.Graphics.DrawImage(
                        image,
                        new Rectangle(
                        (int)(rect.X + rect.Width - size.Width - image.Width - 4),
                        (int)rect.Y,
                        image.Width,
                        image.Height),
                        0, 0, image.Width, image.Height,
                        GraphicsUnit.Pixel,
                        attr);
                }



            }


            // 布局模式
            if (bFree == false)
            {
                ImageAttributes attr = new ImageAttributes();
                attr.SetColorKey(this.Container.imageList_layout.TransparentColor,
                    this.Container.imageList_layout.TransparentColor);
                int nImageIndex = 0;
                if (this.IssueLayoutState == IssueLayoutState.Accepting)
                    nImageIndex = 1;

                Image image = this.Container.imageList_layout.Images[nImageIndex];

                // 重新设置
                x0 = rectContent.X;
                y0 = rectContent.Y;

                Rectangle rect = new Rectangle(
                    x0, // + 8   // 靠左
                    y0 + nHeight - image.Height,    // 靠下
                    image.Width,
                    image.Height);

                if (rect.IntersectsWith(e.ClipRectangle) == true)
                {
                    e.Graphics.DrawImage(
                        image,
                        rect,
                        0, 0, image.Width, image.Height,
                        GraphicsUnit.Pixel,
                        attr);
                }
            }

        }

#if NOOOOOOOOOOOOOOOOOOOO
        // 设置树节点的文字和图像Icon
        public void SetNodeCaption(TreeNode tree_node)
        {
            Debug.Assert(this.dom != null, "");

            string strPublishTime = DomUtil.GetElementText(this.dom.DocumentElement,
                "publishTime");
            string strIssue = DomUtil.GetElementText(this.dom.DocumentElement,
                "issue");
            string strVolume = DomUtil.GetElementText(this.dom.DocumentElement,
                "volume");
            string strZong = DomUtil.GetElementText(this.dom.DocumentElement,
                "zong");

            int nOrderdCount = 0;
            int nRecievedCount = 0;
            // 已验收的册数
            // string strOrderInfoXml = "";

            if (this.dom == null)
                goto SKIP_COUNT;

            {

                XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*/copy");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    string strCopy = node.InnerText.Trim();
                    if (String.IsNullOrEmpty(strCopy) == true)
                        continue;

                    string strNewCopy = "";
                    string strOldCopy = "";
                    OrderDesignControl.ParseOldNewValue(strCopy,
                        out strOldCopy,
                        out strNewCopy);

                    int nNewCopy = 0;
                    int nOldCopy = 0;

                    try
                    {
                        if (String.IsNullOrEmpty(strNewCopy) == false)
                        {
                            nNewCopy = Convert.ToInt32(strNewCopy);
                        }
                        if (String.IsNullOrEmpty(strOldCopy) == false)
                        {
                            nOldCopy = Convert.ToInt32(strOldCopy);
                        }
                    }
                    catch
                    {
                    }

                    nOrderdCount += nOldCopy;
                    nRecievedCount += nNewCopy;
                }
            }

        SKIP_COUNT:

            if (this.OrderedCount == -1 && nOrderdCount > 0)
                this.OrderedCount = nOrderdCount;

            tree_node.Text = strPublishTime + " no." + strIssue + " 总." + strZong + " v." + strVolume + " (" + nRecievedCount.ToString() + ")";

            if (this.OrderedCount == -1)
            {
                if (nRecievedCount == 0)
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_ZERO;
                else
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_NOT_COMPLETE;
            }
            else
            {
                if (nRecievedCount >= this.OrderedCount)
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_COMPLETED;
                else if (nRecievedCount > 0)
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_NOT_COMPLETE;
                else
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_ZERO;
            }

            tree_node.SelectedImageIndex = tree_node.ImageIndex;
        }

#endif

        // TODO: 即将废止
        // 装载期下属的册对象
        int LoadItems(string strPublishTime,
            out string strError)
        {
            strError = "";

            this.Items.Clear();

            List<string> XmlRecords = null;

            Debug.Assert(this.Container != null, "");

            // 利用事件接口this.GetItemInfo，获得所需的册信息
            // return:
            //      -1  error
            //      >-0 所获得记录个数。(XmlRecords.Count)
            int nRet = this.Container.DoGetItemInfo(strPublishTime,
                out XmlRecords,
                out strError);
            if (nRet == -1)
                return -1;

            for (int i = 0; i < XmlRecords.Count; i++)
            {
                string strXml = XmlRecords[i];
                ItemBindingItem item = new ItemBindingItem();
                nRet = item.Initial(strXml, out strError);
                if (nRet == -1)
                    return -1;

                item.Container = this;
                this.Items.Add(item);
            }

            return 0;
        }

        // 装载期下属的册对象
        internal int InitialLoadItems(
            string strPublishTime,
            out string strError)
        {
            strError = "";

            this.Items.Clear();

            Debug.Assert(this.Container != null, "");

            Debug.Assert(String.IsNullOrEmpty(strPublishTime) == false, "");

            // 2010/9/21 add
            if (strPublishTime.IndexOf("-") != -1)
            {
                strError = "出版日期 '"+strPublishTime+"' 应当为单册形态";
                return -1;
            }

            Debug.Assert(strPublishTime.IndexOf("-") == -1, "出版日期应当为单册形态");

            for (int i = 0; i < this.Container.InitialItems.Count; i++)
            {
                ItemBindingItem item = this.Container.InitialItems[i];
                if (strPublishTime == item.PublishTime)
                {
                    item.Container = this;
                    this.Items.Add(item);

                    // 使用后就立即移走
                    this.Container.InitialItems.RemoveAt(i);
                    i--;
                }
            }

            return 0;
        }
    }

    // 第二层次，册对象
    internal class ItemBindingItem
    {
        public IssueBindingItem Container = null;   // 如果Container为空，则属于合订本情况，直接隶属控件对象

        public object Tag = null;   // 用于存放需要连接的任意类型对象

        internal bool Missing = false;  // 当本对象为代用对象的时候，本成员用来表示空白的格子

        internal bool IsParent = false; // 是否为合订本?

        public bool Deleted = false;    // 记录是否已经被删除?

        public string RecPath = "";

        public bool NewCreated = false; // 是否为新创建的对象

        public bool Calculated = false; // 是否为预测的、尚未到达的册

        public bool Locked = false;   // 是否超出当前用户管辖范围?

        // 采购信息关联
            // 一个是<orderInfo>下的<root>偏移；一个是<root>中<distribute>里面的馆藏地点偏移
        public Point OrderInfoPosition = new Point(-1,-1);  // -1 表示尚未初始化

        // 是否为位于订购组中的单元
        public bool InGroup
        {
            get
            {
                if (this.OrderInfoPosition.X == -1)
                {
                    Debug.Assert(this.OrderInfoPosition.Y == -1, "");
                    return false;
                }
                return true;
            }
        }

        // 本格子所从属的GroupCell对象
        internal GroupCell GroupCell
        {
            get
            {
                IssueBindingItem issue = this.Container;
                Debug.Assert(issue != null, "");

                if (issue.IssueLayoutState != IssueLayoutState.Accepting)
                    return null;
                if (this.InGroup == false)
                    return null;
                int nOrderInfoIndex = this.OrderInfoPosition.X;

                return issue.GetGroupCellHead(nOrderInfoIndex);
            }
        }

        // string m_strXml = "";

        public string Xml
        {
            get
            {
                if (dom != null)
                    return dom.OuterXml;

                return "";
                // return m_strXml;
            }
            /*
            set
            {
                m_strXml = value;
            }
             * */
        }

        internal XmlDocument dom = null;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;

        // 自己是否为合订本成员册？== true，表示已经被装订
        // == false, 表示为单册，或者合订本
        public bool IsMember
        {
            get
            {
                if (this.ParentItem != null)
                    return true;
                return false;
            }
        }

        // 如果是合订本，返回下属的册信息对象
        // public List<ItemBindingItem> MemberItems = new List<ItemBindingItem>();
        public List<ItemBindingItem> MemberItems
        {
            get
            {
                List<ItemBindingItem> results = new List<ItemBindingItem>();
                for (int i = 0; i < this.MemberCells.Count; i++)
                {
                    Cell cell = this.MemberCells[i];
                    if (cell == null)
                        continue;
                    if (cell.item == null)
                        continue;
                    results.Add(cell.item);
                }

                if (results.Count > 0)
                {
                    Debug.Assert(this.IsMember == false, "");   // == true BUG?
                }

                return results;
            }
        }

        // 如果是合订本，这里记载下属的格子对象
        // 这里是引用关系，不是拥有关系
        // 不过，有可能下属格子对象个数为0
        internal List<Cell> MemberCells = new List<Cell>();

        // 检查MemberCells数组的正确性
        internal int VerifyMemberCells(out string strError)
        {
            strError = "";
            if (this.MemberCells.Count > 0)
            {
                Debug.Assert(this.IsParent == true, "");

                // 排序临时数组
                List<Cell> members = new List<Cell>();
                members.AddRange(this.MemberCells);

                members.Sort(new CellPublishTimeComparer());

                // 检查有无出版时间重复
                string strPrevPublishTime = "";
                for (int i = 0; i < members.Count; i++)
                {
                    Cell cell = members[i];
                    string strPublishTime = cell.Container.PublishTime;

                    if (strPublishTime == strPrevPublishTime)
                    {
                        strError = "出现了多个格子具有相同的出版时间 '" + strPublishTime + "'";
                        return -1;
                    }

                    strPrevPublishTime = strPublishTime;
                }

                // 检查原始数据是否排序
                for (int i = 0; i < members.Count; i++)
                {
                    Cell cell = members[i];
                    string strPublishTime = cell.Container.PublishTime;

                    if (strPublishTime != this.MemberCells[i].Container.PublishTime)
                    {
                        strError = "MemberCells内的格子未按照出版时间排序";
                        return -1;
                    }
                }

                // 检查ParentItem是否正确
                for (int i = 0; i < members.Count; i++)
                {
                    Cell cell = members[i];

                    if (cell.ParentItem != this)
                    {
                        strError = "cell.ParentItem值不正确";
                        return -1;
                    }

                    if (cell.item != null)
                    {
                        if (cell.item.ParentItem != this)
                        {
                            strError = "cell.item.ParentItem值不正确";
                            return -1;
                        }
                    }
                }

                IssueBindingItem issue = this.Container;
                int index = issue.IndexOfItem(this);    // 合订册所在的列号
                if (index == -1)
                {
                    strError = "居然不在Container所声明的期中";
                    return -1;
                }

                if (issue.IssueLayoutState != IssueLayoutState.Binding)
                    index = -1; // 不作检查了

                /*
                // 检查奇数偶数位置
                if ((index % 2) != 0)
                {
                    strError = "合订本格子应该在双格的左侧位置";
                    return -1;
                }
                 * */

                if (index != -1)
                {
                    // 检查成员格子的列号
                    for (int i = 0; i < members.Count; i++)
                    {
                        Cell cell = members[i];

                        int nCol = cell.Container.IndexOfCell(cell);
                        Debug.Assert(nCol != -1, "");

                        if (cell.Container.IssueLayoutState != IssueLayoutState.Binding)
                            continue;   // 不是binding layout的也不作检查了

                        if (nCol != index + 1)
                        {
                            strError = "成员格子 '" + cell.Container.PublishTime
                                + "' 的列号为"
                                + nCol.ToString() + "，和由合订册格子推算的成员列号 "
                                + (index + 1).ToString() + " 不符合";
                            return -1;
                        }
                    }
                }

                // 核对MemberCells.Count和实际显示的行数

                IssueBindingItem first_issue = members[0].Container;
                Debug.Assert(first_issue != null, "");
                // 找到行号
                int nLineNo = this.Container.Container.Issues.IndexOf(first_issue);
                Debug.Assert(nLineNo != -1, "");
                // 看看垂直方向包含多少个期
                int nIssueCount = 0;

                IssueBindingItem tail_issue = members[members.Count - 1].Container;// item.MemberItems[item.MemberItems.Count - 1].Container;
                Debug.Assert(tail_issue != null, "");
                // 找到行号
                int nTailLineNo = this.Container.Container.Issues.IndexOf(tail_issue);
                Debug.Assert(nTailLineNo != -1, "");

                nIssueCount = nTailLineNo - nLineNo + 1;

                if (nIssueCount != members.Count)
                {
                    strError = "显示的行数为 " + nIssueCount.ToString() + "，而成员数为 " + members.Count.ToString() + "，不一致";
                    return -1;
                }
            }
            else
            {
                // 不是合订本

                IssueBindingItem issue = this.Container;
                int index = issue.IndexOfItem(this);
                if (index == -1)
                {
                    strError = "居然不在Container所声明的期中";
                    return -1;
                }

                /*
                // 检查奇数偶数位置
                if ((index % 2) != 0)
                {
                    strError = "单册格子应该在双格的右侧位置";
                    return -1;
                }
                 * */
            }

            return 0;
        }

        // 从MemberCells数组中的移走属于特定期的格子
        internal void RemoveMemberCell(IssueBindingItem issue)
        {
            for (int i = 0; i < this.MemberCells.Count; i++)
            {
                Cell current = this.MemberCells[i];

                IssueBindingItem current_issue = current.Container;
                Debug.Assert(current_issue != null, "");

                if (current_issue == issue)
                {
                    this.MemberCells.RemoveAt(i);
                    i--;
                }
            }
        }

        // 把Cell对象插入到MemberCells数组中的适当位置
        internal void InsertMemberCell(Cell cell)
        {
            Debug.Assert(this.IsParent == true, "");

            this.MemberCells.Remove(cell);

            Debug.Assert(this.MemberCells.IndexOf(cell) == -1, "插入前已经在里面了");

            string strPublishTime = cell.Container.PublishTime;

            int nInsertIndex = -1;
            string strLastPublishTime = "";
            for (int i = 0; i < this.MemberCells.Count; i++)
            {
                Cell current = this.MemberCells[i];

                IssueBindingItem issue = current.Container;
                Debug.Assert(issue != null, "");

                if (String.Compare(strPublishTime, strLastPublishTime) >= 0
                    && String.Compare(strPublishTime, issue.PublishTime) < 0)
                    nInsertIndex = i;

                strLastPublishTime = issue.PublishTime;
            }

            if (nInsertIndex == -1)
                this.MemberCells.Add(cell);
            else
                this.MemberCells.Insert(nInsertIndex, cell);
        }

        public void SelectAllMemberCells()
        {
            for (int i = 0; i < this.MemberCells.Count; i++)
            {
                Cell cell = this.MemberCells[i];
                if (cell.Selected == false)
                    cell.Select(SelectAction.On);
            }
        }

        // 是否为“加工中”状态
        public bool IsProcessingState()
        {
            if (Global.IncludeStateProcessing(this.State) == true)
                return true;

            return false;
        }


        // 如果是合订本，这里记载本对象所从属的合订册
        // 这里是引用关系，不是拥有关系
        public ItemBindingItem ParentItem = null;

        // 初始化册信息
        public int Initial(string strXml,
            out string strError)
        {
            strError = "";

            // this.Xml = strXml;
            this.dom = new XmlDocument();
            try
            {
                this.dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "册记录 XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 修改册信息
        public int ChangeItemXml(string strXml,
            out string strError)
        {
            strError = "";

            this.dom = new XmlDocument();
            try
            {
                this.dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "册记录 XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            // 修改从属的合订册信息
            if (this.ParentItem != null)
            {
                /*
                this.ParentItem.RefreshBindingXml();
                this.ParentItem.RefreshIntact();
                try
                {
                    this.Container.Container.UpdateObject(this.ParentItem.ContainerCell);
                }
                catch
                {
                }
                 * */
                this.ParentItem.AfterMembersChanged();
            }

            // 刷新自己的格子
            try
            {
                this.Container.Container.UpdateObject(this.ContainerCell);
            }
            catch
            {
            }
            return 0;
        }

        // 检查一个格子，看看是否适合删除
        // return:
        //      -1  出错
        //      0   不适合删除
        //      1   适合删除
        public int CanDelete(out string strError)
        {
            strError = "";

            if (this.Container.Container.CheckProcessingState(this) == false
                && this.Calculated == false
                && this.Deleted == false)   // 2010/4/13
            {
                strError = "不具备“加工中”状态";
                return 0;
            }

            if (this.Locked == true
    && this.Calculated == false
    && this.Deleted == false)
            {
                strError = "处于“锁定”状态";
                return 0;
            }


            if (String.IsNullOrEmpty(this.Borrower) == false)
            {
                strError = "有借阅信息";
                return 0;
            }

            return 1;
        }

        // 删除格子前的准备工作。
        // 对具有订购信息绑定的格子才有必要使用本函数
        public int DoDelete(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.OrderInfoPosition.X == -1)
            {
                strError = "只有对被订购信息绑定的册才能使用本函数DoDelete()";
                return -1;
            }

            nRet = this.CanDelete(out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "册 " + this.RefID + " 不能被删除: " + strError;
                return -1;
            }

            // 找到相关的OrderBindingItem对象，刷新<distribute>元素内容
            IssueBindingItem issue = this.Container;
            Debug.Assert(issue != null, "");

            Debug.Assert(this.OrderInfoPosition.X >= 0, "");
            OrderBindingItem order_item = issue.OrderItems[this.OrderInfoPosition.X];
            // TODO: 预测或者已到的册对象中，应当记载和OrderBindingItem的关联信息
            // 已到的册对象，可以通过refid关联。而预测的册对象，只能通过两个位置偏移来记载：
            // 一个是<orderInfo>下的<root>偏移；一个是<root>中<distribute>里面的馆藏地点偏移

            Debug.Assert(this.OrderInfoPosition.Y >= 0, "");
            // 对特定偏移的预测册进行记到
            // 本操作的结果是对dom里面的XML字符串进行了改动
            nRet = order_item.DoDelete(this.OrderInfoPosition.Y,
            out strError);
            if (nRet == -1)
                return -1;

            bool bRefreshError = false;
            if (nRet == -2)
            {
                // 全部刷新
                nRet = issue.RefreshOrderCopy(this.OrderInfoPosition.X,
                    out strError);
                if (nRet == -1)
                {
                    bRefreshError = true;   // 延迟报错
                    strError = "issue.RefreshOrderCopy() error: " + strError;
                }
            }

            issue.RefreshOrderInfoPositionXY(this.OrderInfoPosition.X,
                this.OrderInfoPosition.Y,
                -1);

            issue.Changed = true;
            issue.AfterMembersChanged();    // 刷新Issue对象内的XML

            if (bRefreshError == true)
                return -1;

            return 0;
        }

        // 将已经记到的格子撤销到未记到状态
        public int DoUnaccept(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.OrderInfoPosition.X == -1)
            {
                strError = "只有对被订购信息绑定的册才能进行撤销记到操作";
                return -1;
            }
            if (this.Calculated == true)
            {
                strError = "只有对已经记到的册才能进行撤销记到操作";
                return -1;
            }
            nRet = this.CanDelete(out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "册 " + this.RefID + " 不能被删除: " + strError;
                return -1;
            }

            // 找到相关的OrderBindingItem对象，刷新<distribute>元素内容
            IssueBindingItem issue = this.Container;
            Debug.Assert(issue != null, "");

            Debug.Assert(this.OrderInfoPosition.X >= 0, "");
            OrderBindingItem order_item = issue.OrderItems[this.OrderInfoPosition.X];
            // TODO: 预测或者已到的册对象中，应当记载和OrderBindingItem的关联信息
            // 已到的册对象，可以通过refid关联。而预测的册对象，只能通过两个位置偏移来记载：
            // 一个是<orderInfo>下的<root>偏移；一个是<root>中<distribute>里面的馆藏地点偏移

            Debug.Assert(this.OrderInfoPosition.Y >= 0, "");
            // 对特定偏移的预测册进行记到
            // 本操作的结果是对dom里面的XML字符串进行了改动
            nRet = order_item.DoUnaccept(this.OrderInfoPosition.Y,
            out strError);
            if (nRet == -1)
                return -1;

            // 刷新当前对象(ItemBindingItem)的显示
            this.RefID = "";
            // 批次号
            this.BatchNo = "";
            this.State = "";

            IssueBindingItem.SetFieldValueFromOrderInfo(
                true,    // 是否强行设置
                this,
                order_item);

            this.Changed = true;
            this.NewCreated = false;
            this.Calculated = true;
            this.Deleted = false;

            issue.Changed = true;
            issue.AfterMembersChanged();    // 刷新Issue对象内的XML

            // TODO: 如果是Acception Layout, 还要刷新从属的GroupCell的显示

            return 0;
        }

        // 将预测对象进行收登(记到)
        public int DoAccept(out string strError)
        {
            strError = "";
            int nRet = 0;

            bool bSetProcessingState = this.Container.Container.SetProcessingState;

            if (this.Calculated == false)
            {
                strError = "只有对预测状态的册才能进行记到操作";
                return -1;
            }

#if NO
            if (this.Locked == true)
            {
                strError = "格子状态为锁定时 不允许进行记到操作";
                return -1;
            }
#endif

            // 找到相关的OrderBindingItem对象，刷新<distribute>元素内容
            IssueBindingItem issue = this.Container;
            Debug.Assert(issue != null, "");

            Debug.Assert(this.OrderInfoPosition.X >= 0, "");
            OrderBindingItem order_item = issue.OrderItems[this.OrderInfoPosition.X];
            // TODO: 预测或者已到的册对象中，应当记载和OrderBindingItem的关联信息
            // 已到的册对象，可以通过refid关联。而预测的册对象，只能通过两个位置偏移来记载：
            // 一个是<orderInfo>下的<root>偏移；一个是<root>中<distribute>里面的馆藏地点偏移

            Debug.Assert(this.OrderInfoPosition.Y >= 0, "");
            string strRefID = "";
            string strLocation = "";
            // 对特定偏移的预测册进行记到
            // 本操作的结果是对dom里面的XML字符串进行了改动
            nRet = order_item.DoAccept(this.OrderInfoPosition.Y,
            ref strRefID,
            out strLocation,
            out strError);
            if (nRet == -1)
                return -1;

            string strBatchNo = this.Container.Container.GetAcceptingBatchNo();

            /*
            XmlNode order_node = order_item.dom.DocumentElement;
            Debug.Assert(order_item.dom != null, "");
            Debug.Assert(order_node != null, "");
             * */

            // 刷新当前对象(ItemBindingItem)的显示
            this.RefID = strRefID;
            // location
            this.LocationString = strLocation;
            // 批次号
            this.BatchNo = strBatchNo;

            // 2009/10/19
            // 状态
            if (bSetProcessingState == true)
            {
                // 增补“加工中”值
                this.State = Global.AddStateProcessing(this.State);
            }

            /*
            // seller
            // seller内是单纯值
            if (String.IsNullOrEmpty(this.Seller) == true)
                this.Seller = order_item.Seller;

            // source
            // source内顺次采用新值/旧值
            if (String.IsNullOrEmpty(this.Source) == true)
                this.Source = IssueBindingItem.GetNewOrOldValue(order_item.Source);

            // price
            // price内顺次采用新值/旧值
            if (String.IsNullOrEmpty(this.Price) == true)
                this.Price = IssueBindingItem.GetNewOrOldValue(order_item.Price);
             * */
            IssueBindingItem.SetFieldValueFromOrderInfo(
                false,    // 是否强行设置
                this,
                order_item);

            // publishTime
            this.PublishTime = issue.PublishTime;

            // volume 其实是当年期号、总期号、卷号在一起的一个字符串
            string strVolume = VolumeInfo.BuildItemVolumeString(
                IssueUtil.GetYearPart(issue.PublishTime),
                issue.Issue,
                issue.Zong,
                issue.Volume);
            this.Volume = strVolume;

            // this.ContainerCell.Select(SelectAction.On);
            this.Changed = true;
            this.NewCreated = true;
            this.Calculated = false;
            this.Deleted = false;

            /*
            // 设置或者刷新一个操作记载
            // 可能会抛出异常
            this.SetOperation(
                "create",
                this.Container.Container.Operator,
                "");
             * */

            issue.Changed = true;
            issue.AfterMembersChanged();    // 刷新Issue对象内的XML

            // TODO: 如果是Acception Layout, 还要刷新从属的GroupCell的显示

            return 0;
        }

        // 不抛出异常了
        public void AfterMembersChanged()
        {
            bool bChanged = false;
            if (RefreshPublishTime() == true)
                bChanged = true;
            try
            {
                if (RefreshIntact() == true)
                    bChanged = true;
            }
            catch (Exception ex)
            {
                this.Intact = "警告: " + ex.Message;
            }

            if (RefreshBindingXml() == true)
                bChanged = true;
            if (RefreshVolumeString() == true)
                bChanged = true;
            if (RefreshPriceString() == true)
                bChanged = true;

            if (bChanged == true)
            {
                this.Changed = true;
                try
                {
                    this.Container.Container.UpdateObject(this.ContainerCell);
                }
                catch
                {
                }
            }
        }

        // MemberCells修改后，要刷新合订册的Intact值
        // return:
        //      false   Intact没有发生修改
        //      true    Intact发生了修改
        public bool RefreshIntact()
        {
            if (this.MemberCells.Count == 0)
            {
                if (this.Intact == "0")
                    return false;
                this.Intact = "0";
                return true;
            }
            // 获得Intact
            string strIntact = "";
            string strError = "";
            int nRet = BuildIntactString(this.MemberCells,
                out strIntact,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            if (this.Intact == "" && strIntact == "100")
                return false;

            if (this.Intact == strIntact)
                return false;
            this.Intact = strIntact;
            return true;
        }

        public static int BuildIntactString(List<Cell> cells,
    out string strIntact,
    out string strError)
        {
            strIntact = "";
            strError = "";

            if (cells.Count == 0)
            {
                strIntact = "0";
                return 0;
            }

            float fTotal = 0;

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;

                if (cell.item == null)
                    continue;

                string strValue = cell.item.Intact;
                if (String.IsNullOrEmpty(strValue) == true)
                {
                    fTotal += 100;
                    continue;
                }

                strValue = strValue.Replace("%", "");

                float v = 0;
                try
                {
                    v = (float)Convert.ToDecimal(strValue);
                }
                catch
                {
                    strError = "完好率值 '" + strValue + "' 格式错误(册'" + cell.item.RefID + "')";
                    strIntact = "0";
                    return -1;
                }

                if (v > 100)
                {
                    strError = "完好率值 '" + strValue + "' 格式错误(册'" + cell.item.RefID + "')。不能大于100";
                    strIntact = "0";
                    return -1;
                }

                fTotal += v;
            }

            Debug.Assert(cells.Count != 0, "");
            strIntact = (fTotal / (float)cells.Count).ToString("0.#");
            return 0;
        }

        // 对于合订册，MemberCells修改后，要刷新出版时间范围；对于成员册或者单册，刷新出版时间值
        // return:
        //      false   出版时间范围没有发生修改
        //      true    出版时间范围发生了修改
        public bool RefreshPublishTime()
        {
            if (this.IsParent == false)
            {
                IssueBindingItem issue = this.Container;
                if (issue != null 
                    && String.IsNullOrEmpty(issue.PublishTime) == false)
                {
                    if (this.PublishTime != issue.PublishTime)
                    {
                        this.PublishTime = issue.PublishTime;
                        return true;
                    }
                }

                return false;
            }

            try
            {
                string strNewPublishTime = "";

                IssueBindingItem first_issue = this.Container;
                Debug.Assert(first_issue != null, "");

                int nFirstLineNo = this.Container.Container.Issues.IndexOf(first_issue);
                Debug.Assert(nFirstLineNo != -1, "");

                string strFirstPublishTime = first_issue.PublishTime;
                Debug.Assert(String.IsNullOrEmpty(strFirstPublishTime) == false, "");

                if (this.MemberCells.Count == 0)
                {
                    strNewPublishTime = strFirstPublishTime + "-" + strFirstPublishTime;
                    if (this.PublishTime == strNewPublishTime)
                        return false;
                    this.PublishTime = strNewPublishTime;
                    return true;
                }

                IssueBindingItem last_issue = this.MemberCells[this.MemberCells.Count - 1].Container;
                Debug.Assert(last_issue != null, "");

                int nLastLineNo = this.Container.Container.Issues.IndexOf(last_issue);
                Debug.Assert(nLastLineNo != -1, "");

                string strLastPublishTime = last_issue.PublishTime;
                Debug.Assert(String.IsNullOrEmpty(strLastPublishTime) == false, "");

                strNewPublishTime = strFirstPublishTime + "-" + strLastPublishTime;
                if (this.PublishTime == strNewPublishTime)
                    return false;
                this.PublishTime = strNewPublishTime;
                return true;
            }
            finally
            {
                // 刷新outofissue状态
                int nCol = this.Container.IndexOfItem(this);
                Debug.Assert(nCol != -1, "");
                if (nCol != -1)
                    this.Container.RefreshOutofIssueValue(nCol);
            }
        }

        // 对于合订册，MemberCells修改后，要刷新合订册的组合volume字符串；对于成员册或者单册，刷新valume string
        public bool RefreshVolumeString()
        {
            if (this.IsParent == false)
            {
                IssueBindingItem issue = this.Container;
                if (issue != null
                    && String.IsNullOrEmpty(issue.PublishTime) == false)
                {
                    string strVolumeString = VolumeInfo.BuildItemVolumeString(
                        IssueUtil.GetYearPart(issue.PublishTime),
                        issue.Issue,
                        issue.Zong,
                        issue.Volume);
                    if (this.Volume != strVolumeString)
                    {
                        this.Volume = strVolumeString;
                        return true;
                    }
                }

                return false;
            }

            if (this.MemberCells.Count == 0)
            {
                if (this.Volume == "")
                    return false;
                this.Volume = "";
                return true;
            }

            Hashtable no_list_table = new Hashtable();
            // List<string> no_list = new List<string>();
            List<string> volumn_list = new List<string>();
            List<string> zong_list = new List<string>();

            for (int i = 0; i < this.MemberCells.Count; i++)
            {
                Cell cell = this.MemberCells[i];
                if (cell == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (cell.item == null)
                    continue;   // 跳过缺期

                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");

                string strNo = "";
                string strVolume = "";
                string strZong = "";

                if (cell.item != null
                    && String.IsNullOrEmpty(cell.item.Volume) == false)
                {
                    // 解析当年期号、总期号、卷号的字符串
                    VolumeInfo.ParseItemVolumeString(cell.item.Volume,
                        out strNo,
                        out strZong,
                        out strVolume);
                }

                // 实在不行，还是用期行的?
                if (String.IsNullOrEmpty(strNo) == true)
                {
                    strNo = issue.Issue;
                    Debug.Assert(String.IsNullOrEmpty(strNo) == false, "");

                    strVolume = issue.Volume;
                    strZong = issue.Zong;
                }

                Debug.Assert(String.IsNullOrEmpty(issue.PublishTime) == false, "");
                string strYear = IssueUtil.GetYearPart(issue.PublishTime);

                List<string> no_list = (List<string>)no_list_table[strYear];
                if (no_list == null)
                {
                    no_list = new List<string>();
                    no_list_table[strYear] = no_list;
                }

                no_list.Add(strNo);
                volumn_list.Add(strVolume);
                zong_list.Add(strZong);
            }

            List<string> keys = new List<string>();
            foreach (string key in no_list_table.Keys)
            {
                keys.Add(key);
            }
            keys.Sort();

            string strNoString = "";
            for (int i = 0; i < keys.Count; i++)
            {
                string strYear = keys[i];
                List<string> no_list = (List<string>)no_list_table[strYear];
                Debug.Assert(no_list != null);

                if (String.IsNullOrEmpty(strNoString) == false)
                    strNoString += ","; // ;
                strNoString += strYear + ",no." + Global.BuildNumberRangeString(no_list);   // :no
            }

            string strVolumnString = Global.BuildNumberRangeString(volumn_list);
            string strZongString = Global.BuildNumberRangeString(zong_list);

            string strValue = strNoString;


            if (String.IsNullOrEmpty(strZongString) == false)
            {
                if (String.IsNullOrEmpty(strValue) == false)
                    strValue += "=";
                strValue += "总." + strZongString;
            }

            if (String.IsNullOrEmpty(strVolumnString) == false)
            {
                if (String.IsNullOrEmpty(strValue) == false)
                    strValue += "=";
                strValue += "v." + strVolumnString;
            }

            if (this.Volume == strValue)
                return false;

            this.Volume = strValue;
            return true;
        }

        // MemberCells修改后，要刷新合订册的价格字符串
        public bool RefreshPriceString()
        {
            if (this.MemberCells.Count == 0)
            {
                if (this.Price == "")
                    return false;
                this.Price = "";
                return true;
            }

            List<string> prices = new List<string>();
            for (int i = 0; i < this.MemberCells.Count; i++)
            {
                Cell cell = this.MemberCells[i];
                if (cell == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (cell.item == null)
                    continue;   // 跳过缺期

                prices.Add(cell.item.Price);
            }

            string strTotalPrice = PriceUtil.TotalPrice(prices);

            if (this.Price == strTotalPrice)
                return false;
            this.Price = strTotalPrice;
            return true;
        }

        // MemberCells修改后，要刷新binding XML片断
        // 可能会抛出异常
        // return:
        //      false   binding XML片断没有发生修改
        //      true    binding XML片断发生了修改
        public bool RefreshBindingXml()
        {
            if (this.MemberCells.Count == 0)
            {
                if (this.Binding == "")
                    return false;
                this.Binding = "";
                return true;
            }

            // 创建<binding>元素内片断
            string strInnerXml = "";
            string strError = "";
            int nRet = BuildBindingXmlString(this.MemberCells,
                out strInnerXml,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            if (this.Binding == strInnerXml)
                return false;

            this.Binding = strInnerXml;
            return true;
        }

        /*
        // 创建<binding>元素内片断
        public static int BuildBindingXmlString(List<ItemBindingItem> items,
            out string strInnerXml,
            out string strError)
        {
            strInnerXml = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<binding />");

            for (int i = 0; i < items.Count; i++)
            {
                ItemBindingItem item = items[i];

                XmlNode node = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(node);

                DomUtil.SetAttr(node, "publishTime", item.PublishTime);
                DomUtil.SetAttr(node, "volume", item.Volume);
                DomUtil.SetAttr(node, "refID", item.RefID);
            }

            strInnerXml = dom.DocumentElement.InnerXml;

            return 0;
        }
         * */

        // 创建作为合订本的<binding>元素内片断
        // 要创建若干<item>元素
        public static int BuildBindingXmlString(List<Cell> cells,
            out string strInnerXml,
            out string strError)
        {
            strInnerXml = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<binding />");

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;

                XmlNode node = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(node);

                ItemBindingItem item = cell.item;

                if (item != null)
                {
                    // 存储的原则是，主要存储标识定位信息
                    DomUtil.SetAttr(node, "publishTime", item.PublishTime);
                    DomUtil.SetAttr(node, "volume", item.Volume);
                    DomUtil.SetAttr(node, "refID", item.RefID);
                    if (String.IsNullOrEmpty(item.Barcode) == false)
                        DomUtil.SetAttr(node, "barcode", item.Barcode);
                    if (String.IsNullOrEmpty(item.RegisterNo) == false)
                        DomUtil.SetAttr(node, "registerNo", item.RegisterNo);

                    // 2011/9/8
                    if (String.IsNullOrEmpty(item.Price) == false)
                        DomUtil.SetAttr(node, "price", item.Price);
                }
                else
                {
                    DomUtil.SetAttr(node, "publishTime", cell.Container.PublishTime);

                    string strVolume = VolumeInfo.BuildItemVolumeString(
                        IssueUtil.GetYearPart(cell.Container.PublishTime),
                        cell.Container.Issue,
                        cell.Container.Zong,
                        cell.Container.Volume);
                    DomUtil.SetAttr(node, "volume", strVolume);
                    DomUtil.SetAttr(node, "refID", "");
                    DomUtil.SetAttr(node, "missing", "true");

                    // TODO: 除了头尾连续的missing格子外，中间部分是否可以省略?
                }
            }

            strInnerXml = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 创建作为成员册的<binding>元素内片断
        // 仅仅创建一个<bindingParent>元素
        public int BuildMyselfBindingXmlString(
            out string strInnerXml,
            out string strError)
        {
            strInnerXml = "";
            strError = "";

            Debug.Assert(this.ParentItem != null, "成员册其.ParentItem必须为非空");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<binding />");

            {
                XmlNode node = dom.CreateElement("bindingParent");
                dom.DocumentElement.AppendChild(node);

                DomUtil.SetAttr(node, "refID", this.ParentItem.RefID);
            }

            strInnerXml = dom.DocumentElement.InnerXml;
            return 0;
        }

        // 定位在MemberCells里面的下标
        public int GetCellIndexOfMemberItem(ItemBindingItem item)
        {
            for (int i = 0; i < this.MemberCells.Count; i++)
            {
                Cell cell = this.MemberCells[i];
                if (cell == null)
                    continue;
                if (cell.item == item)
                    return i;
            }

            return -1;
        }

        // 包容本对象的Cell对象
        public Cell ContainerCell
        {
            get
            {
                IssueBindingItem issue = this.Container;
                if (issue == null)
                    return null;

                Debug.Assert(issue != null, "");
                int index = issue.IndexOfItem(this);

                if (index == -1)
                    return null;

                Debug.Assert(index != -1, "");
                return issue.Cells[index];
            }
        }


        #region 记录内的数据字段

        public string GetText(string strName)
        {
            switch (strName)
            {
                case "location":
                    return this.LocationString;
                case "intact":
                    return this.Intact;
                case "state":
                    return this.State;
                case "refID":
                    return this.RefID;

                case "publishTime":
                    return this.RefID;
                case "barcode":
                    return this.Barcode;
                case "regitserNo":
                    return this.RegisterNo;
                case "source":
                    return this.Source;
                case "seller":
                    return this.Seller;
                case "accessNo":
                    return this.AccessNo;
                case "bookType":
                    return this.BookType;
                case "price":
                    return this.Price;
                case "volumn":
                    return this.Volume;
                case "comment":
                    return this.Comment;
                case "batchNo":
                    return this.BatchNo;
                case "binding":
                    return this.Binding;
                case "recpath":
                    return this.RecPath;
                case "mergeComment":
                    return this.MergeComment;
                case "borrower":
                    return this.Borrower;
                case "borrowDate":
                    return this.BorrowDate;
                case "borrowPeriod":
                    return this.BorrowPeriod;

            }

            return "不支持的strName '" + strName + "'";
        }

        public string PublishTime
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "publishTime");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "publishTime", value);
            }
        }

        public string LocationString
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "location");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "location", value);
            }
        }

        public string Intact
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "intact");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "intact", value);
            }
        }

        public string Barcode
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "barcode");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "barcode", value);
            }
        }

        public string RegisterNo
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "registerNo");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "registerNo", value);
            }
        }

        public string RefID
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "refID");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "refID", value);
            }
        }

        public string Source
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "source");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "source", value);
            }
        }

        public string Seller
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "seller");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "seller", value);
            }
        }

        public string AccessNo
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "accessNo");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "accessNo", value);
            }
        }

        public string State
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "state");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "state", value);
            }
        }

        public string BookType
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "booktype");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "booktype", value);
            }
        }

        public string Price
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "price");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "price", value);
            }
        }

        public string Volume
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "volume");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "volume", value);
            }
        }

        public string Comment
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "comment");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "comment", value);
            }
        }

        public string BatchNo
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "batchNo");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "batchNo", value);
            }
        }

        public string Binding
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementInnerXml(this.dom.DocumentElement,
                    "binding");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementInnerXml(this.dom.DocumentElement,
                    "binding", value);
            }
        }

        public string Borrower
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "borrower");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "borrower", value);
            }
        }

        public string BorrowDate
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "borrowDate");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "borrowDate", value);
            }
        }

        public string BorrowPeriod
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "borrowPeriod");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "borrowPeriod", value);
            }
        }

        public string MergeComment
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "mergeComment");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "mergeComment", value);
            }
        }


        public string Operations
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementInnerXml(this.dom.DocumentElement,
                    "operations");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementInnerXml(this.dom.DocumentElement,
                    "operations", value);
            }
        }

        #endregion

        // 设置或者刷新一个操作记载
        // 可能会抛出异常
        public void SetOperation(
            string strAction,
            string strOperator,
            string strComment)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<operations />");

            string strInnerXml = this.Operations;
            if (String.IsNullOrEmpty(strInnerXml) == false)
            {
                dom.DocumentElement.InnerXml = this.Operations;
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("operation[@name='" + strAction + "']");
            if (node == null)
            {
                node = dom.CreateElement("operation");
                dom.DocumentElement.AppendChild(node);
                DomUtil.SetAttr(node, "name", strAction);
            }

            DomUtil.SetAttr(node, "time", DateTimeUtil.Rfc1123DateTimeString(DateTime.Now.ToUniversalTime()));
            DomUtil.SetAttr(node, "operator", strOperator);
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(node, "comment", strComment);

            this.Operations = dom.DocumentElement.InnerXml;
        }

    }

    // 第二层次，订购对象
    internal class OrderBindingItem
    {
        public IssueBindingItem Container = null;

        public object Tag = null;   // 用于存放需要连接的任意类型对象

        internal XmlDocument dom = null;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;

        string m_strXml = "";

        public string Xml
        {
            get
            {
                if (dom != null)
                    return dom.OuterXml;

                return m_strXml;
            }
        }

        // 初始化
        public int Initial(string strXml,
            out string strError)
        {
            strError = "";

            this.dom = new XmlDocument();
            try
            {
                this.dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "订购 XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 刷新<distribute>字符串
        // return:
        //      false   没有发生新的改变
        //      true    发生了改变
        internal bool UpdateDistributeString(GroupCell group_head)
        {
            List<Cell> members = group_head.MemberCells;
            LocationCollection locations = new LocationCollection();
            for (int i = 0; i < members.Count; i++)
            {
                Cell cell = members[i];
                Debug.Assert(cell.item != null, "");
                Location location = new Location();
                location.Name = cell.item.LocationString;
                location.RefID = cell.item.RefID;
                locations.Add(location);
            }

            string strNewValue = locations.ToString(true);
            if (this.Distribute != strNewValue)
            {
                this.Distribute = strNewValue;
                this.Changed = true;
                return true;
            }

            return false;
        }

        // 根据<distribute>中的实际情况刷新<copy>值
        // parameters:
        //      bRefreshOrderCount  是否也要刷新订购份数。无论如何，已到份数都是要刷新的
        // return:
        //      -1  出错
        //      0   没有发生修改
        //      1   发生了修改
        public int RefreshOrderCopy(
            bool bRefreshOrderCount,
            out string strError)
        {
            strError = "";

                string strNewValue = "";
                string strOldValue = "";
                OrderDesignControl.ParseOldNewValue(this.Copy,
                    out strOldValue,
                    out strNewValue);
                int nOldCopy = IssueBindingItem.GetNumberValue(strOldValue);
                int nNewCopy = IssueBindingItem.GetNumberValue(strNewValue);

            string strDistribute = this.Distribute;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute,
                out strError);
            if (nRet == -1)
                return -1;

            bool bChanged = false;

            // 订购数是一个谜，因为没有任何证据可以复原。最多可以判断，至少要等于等于已到值
            int nArrivedCount = locations.GetArrivedCopy();
            if (bRefreshOrderCount == true)
            {
                if (nOldCopy < nArrivedCount)
                {
                    bChanged = true;
                    nOldCopy = nArrivedCount;
                }
            }

            if (nNewCopy != nArrivedCount)
            {
                bChanged = true;
                nNewCopy = nArrivedCount;
            }

            if (bChanged == true)
            {
                this.Copy = OrderDesignControl.LinkOldNewValue(nOldCopy.ToString(),
         nNewCopy.ToString());
                return 1;
            }
            return 0;
        }

        // 对特定偏移的location位置进行删除，并修改订购册数
        // return:
        //      -2  订购或已到值显然不正确，需要刷新
        //      -1  出错
        //      0   正确
        public int DoDelete(int nLocationIndex,
            out string strError)
        {
            strError = "";

            string strDistribute = this.Distribute;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute,
                out strError);
            if (nRet == -1)
                return -1;

            if (nLocationIndex >= locations.Count)
                return 0;   // 正好不用修改了

            Location location = locations[nLocationIndex];
            bool bArrived = String.IsNullOrEmpty(location.RefID) == false;

            locations.RemoveAt(nLocationIndex);
            this.Distribute = locations.ToString(true);

            int nArrivedCountDelta = 0;
            int nOrderCountDelta = -1;
            if (bArrived == true)
                nArrivedCountDelta = -1;

            bool bCopyValueError = false;

            // 刷新<copy>元素中的订购和已到册数值
            if (nOrderCountDelta != 0 || nArrivedCountDelta != 0)
            {
                string strNewValue = "";
                string strOldValue = "";
                OrderDesignControl.ParseOldNewValue(this.Copy,
                    out strOldValue,
                    out strNewValue);
                int nOldCopy = IssueBindingItem.GetNumberValue(strOldValue);
                int nNewCopy = IssueBindingItem.GetNumberValue(strNewValue);
                nOldCopy += nOrderCountDelta;
                if (nOldCopy < 0)
                {
                    bCopyValueError = true;
                    nOldCopy = 0;
                }

                Debug.Assert(nOldCopy >= 0, "");

                nNewCopy += nArrivedCountDelta;
                if (nNewCopy < 0)
                {
                    bCopyValueError = true;
                    nNewCopy = 0;
                }

                Debug.Assert(nNewCopy >= 0, "");
                this.Copy = OrderDesignControl.LinkOldNewValue(nOldCopy.ToString(),
                     nNewCopy.ToString());
            }

            this.Changed = true;
            if (bCopyValueError == true)
            {
                strError = "copy值有错误，请及时刷新";
                return -2;
            }
            return 0;
        }

        // 对特定偏移的已经记到的册进行撤销记到
        // 本操作的结果是对dom里面的XML字符串进行了改动
        // 注意，本函数并不将修改汇总到IssueBindingItem对象中，需要调用者注意汇总
        // parameters:
        public int DoUnaccept(int nLocationIndex,
            out string strError)
        {
            strError = "";

            string strDistribute = this.Distribute;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute,
                out strError);
            if (nRet == -1)
                return -1;

            if (nLocationIndex >= locations.Count)
                return 0;   // 正好不用修改了

            Location location = locations[nLocationIndex];
            location.RefID = "";
            this.Distribute = locations.ToString(true);

            // 刷新<copy>元素中的已到册数值
            int nArrivedCount = locations.GetArrivedCopy();
            this.Copy = IssueBindingItem.ChangeNewValue(this.Copy, nArrivedCount.ToString());

            this.Changed = true;
            return 0;
        }

        // 对特定偏移的预测册进行记到
        // 本操作的结果是对dom里面的XML字符串进行了改动
        // 注意，本函数并不将修改汇总到IssueBindingItem对象中，需要调用者注意汇总
        // parameters:
        //      strRefID    [in]如果想使用以前的refid [out]返回这个位置的refid
        //      strLocation [out]返回这个位置的馆藏地点名
        public int DoAccept(int nLocationIndex,
            ref string strRefID,
            out string strLocation,
            out string strError)
        {
            strError = "";
            // strRefID = "";
            strLocation = "";

            string strDistribute = this.Distribute;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute,
                out strError);
            if (nRet == -1)
                return -1;

            if (nLocationIndex >= locations.Count)
            {
                /*
                strError = "nLocationIndex值 "+nLocationIndex.ToString()+" (从0开始计数)超过实际具有的馆藏地点条目个数 " + locations.Count.ToString();
                return -1;
                 * */
                // 在后面添加足够的空地点事项
                while (locations.Count <= nLocationIndex)
                {
                    locations.Add(new Location());
                }
                Debug.Assert(nLocationIndex < locations.Count, "");
            }

            Location location = locations[nLocationIndex];

            if (String.IsNullOrEmpty(location.RefID) == false)
            {
                strError = "记到操作前，发现位置 "+nLocationIndex.ToString()+" 已经存在 refid ["+location.RefID+"]";
                return -1;
            }

            strLocation = location.Name;
            if (string.IsNullOrEmpty(strRefID) == true)
                strRefID = Guid.NewGuid().ToString();
            location.RefID = strRefID;

            strDistribute = locations.ToString(true);
            this.Distribute = strDistribute;

            // 刷新<copy>元素中的已到册数值
            int nArrivedCount = locations.GetArrivedCopy();
            this.Copy = IssueBindingItem.ChangeNewValue(this.Copy, nArrivedCount.ToString());

            this.Changed = true;
            return 0;
        }

        #region 数据成员

        public string GetText(string strName)
        {
            switch (strName)
            {
                case "state":
                    return this.State;
                case "range":
                    return this.Range;
                case "issueCount":
                    return this.IssueCount;
                case "orderTime":
                    return this.OrderTime;

                case "orderID":
                    return this.OrderID;
                case "comment":
                    return this.Comment;
                case "batchNo":
                    return this.BatchNo;
                case "source":
                    return IssueBindingItem.GetNewOrOldValue(this.Source);
                case "seller":
                    return this.Seller;
                case "catalogNo":
                    return this.CatalogNo;
                case "copy":
                    return IssueBindingItem.GetNewOrOldValue(this.Copy);
                case "price":
                    {
                        string strPrice = IssueBindingItem.GetNewOrOldValue(this.Price);
                        if (string.IsNullOrEmpty(strPrice) == false)
                            return strPrice;
                        else
                        {
                            // 2015/4/1
                            return IssueBindingItem.CalcuPrice(this.TotalPrice, this.IssueCount, IssueBindingItem.GetOldOrNewValue(this.Copy));
                        }
                        // return IssueBindingItem.GetNewOrOldValue(this.Price);
                    }
                case "distribute":
                    return this.Distribute;
                case "class":
                    return this.Class;
                case "totalPrice":
                    return this.TotalPrice;
                case "sellerAddress":
                    return this.SellerAddress;
            }

            return "不支持的strName '" + strName + "'";
        }

        public string State
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "state");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "state", value);
            }
        }

        public string Range
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "range");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "range", value);
            }
        }

        public string IssueCount
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "issueCount");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "issueCount", value);
            }
        }

        public string OrderTime
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "orderTime");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "orderTime", value);
            }
        }

        public string OrderID
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "orderID");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "orderID", value);
            }
        }

        public string Comment
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "comment");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "comment", value);
            }
        }

        public string BatchNo
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "batchNo");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "batchNo", value);
            }
        }

        public string CatalogNo
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "catalogNo");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "catalogNo", value);
            }
        }

        public string Seller
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "seller");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "seller", value);
            }
        }

        public string Source
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "source");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "source", value);
            }
        }

        public string Copy
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "copy");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "copy", value);
            }
        }




        public string Price
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "price");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "price", value);
            }
        }

        public string Distribute
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "distribute");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "distribute", value);
            }
        }

        public string Class
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "class");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "class", value);
            }
        }

        public string TotalPrice
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "totalPrice");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "totalPrice", value);
            }
        }

        public string SellerAddress
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementInnerXml(this.dom.DocumentElement,
                    "sellerAddress");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementInnerXml(this.dom.DocumentElement,
                    "sellerAddress", value);
            }
        }



        #endregion


    }

    // 比较出版日期。小的在前
    internal class IssuePublishTimeComparer : IComparer<IssueBindingItem>
    {

        int IComparer<IssueBindingItem>.Compare(IssueBindingItem x, IssueBindingItem y)
        {
            string s1 = x.PublishTime;
            string s2 = y.PublishTime;

            int nRet = String.Compare(s1, s2);
            if (nRet == 0)
            {
                // 如果出版日期相同，则把虚的排在前面
                if (x.Virtual == false && y.Virtual == false)
                    return 0;
                if (x.Virtual == true)
                    return -1;
                return 1;
            }

            return nRet;
        }
    }

    // 期行的布局模式
    internal enum IssueLayoutState
    {
        Binding = 1,    // 装订
        Accepting = 2,  // 记到
    }

    // 比较完好率。大的在前
    internal class ItemIntactComparer : IComparer<ItemBindingItem>
    {

        int IComparer<ItemBindingItem>.Compare(ItemBindingItem x, ItemBindingItem y)
        {
            string s1 = x.Intact;
            string s2 = y.Intact;

            float v1 = 0;
            if (String.IsNullOrEmpty(s1) == true)
                v1 = 100;
            else
            {
                try
                {
                    v1 = (float)Convert.ToDecimal(s1);
                }
                catch
                {
                    v1 = 0;
                }
            }

            float v2 = 0;
            if (String.IsNullOrEmpty(s2) == true)
                v2 = 100;
            else
            {
                try
                {
                    v2 = (float)Convert.ToDecimal(s2);
                }
                catch
                {
                    v2 = 0;
                }
            }

            if (v1 - v2 > 0)
                return -1;
            if (v1 - v2 < 0)
                return 1;
            return 0;
        }
    }

}
