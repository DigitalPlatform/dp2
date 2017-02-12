using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// MEMO: 订单打印后，可以保留“电子订单” XML 格式，供以后参考。数据库内各种数据可能会后来变化，但电子订单固化记忆了打印瞬间的数据状态

namespace dp2Circulation
{
    /// <summary>
    /// 一张订单的内存结构
    /// </summary>
    public class OrderList
    {
        public string Seller { get; set; }

        List<OrderListItem> _items = new List<OrderListItem>();

        // 种、册、金额累计
    }

    /// <summary>
    /// 一行订单事项
    /// </summary>
    public class OrderListItem
    {
        public string Seller { get; set; }

        public string CatalogNo { get; set; }
        public string Copy { get; set; }
        public string Price { get; set; }
        public string TotalPrice { get; set; }

        public BiblioStore BiblioStore { get; set; }

        /// <summary>
        /// 构成本行的订购记录列表
        /// </summary>
        public List<OrderStore> Orders { get; set; }

    }
}
