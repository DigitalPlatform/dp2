using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.IO;  // EntityInfo

/*
 * TODO:
 * 1) 需要增加Source成员 -- 表示经费来源的字段。服务器端也要增加相应定义。
 * 2) 增加IssueCount成员
 * 
 * */


namespace dp2Circulation
{
    /// <summary>
    /// 订购信息
    /// </summary>
    [Serializable()]
    public class OrderItem : BookItemBase
    {
        // 列index。注意要保持和OrderControl中的列号一致
        /// <summary>
        /// ListView 栏目下标：编号
        /// </summary>
        public const int COLUMN_INDEX = 0;
        /// <summary>
        /// ListView 栏目下标：错误信息
        /// </summary>
        public const int COLUMN_ERRORINFO = 1;
        /// <summary>
        /// ListView 栏目下标：记录状态
        /// </summary>
        public const int COLUMN_STATE = 2;
        /// <summary>
        /// ListView 栏目下标：书目号
        /// </summary>
        public const int COLUMN_CATALOGNO = 3;
        /// <summary>
        /// ListView 栏目下标：渠道
        /// </summary>
        public const int COLUMN_SELLER = 4;
        /// <summary>
        /// ListView 栏目下标：经费来源
        /// </summary>
        public const int COLUMN_SOURCE = 5;
        /// <summary>
        /// ListView 栏目下标：订购时间范围
        /// </summary>
        public const int COLUMN_RANGE = 6;
        /// <summary>
        /// ListView 栏目下标：期数
        /// </summary>
        public const int COLUMN_ISSUECOUNT = 7;
        /// <summary>
        /// ListView 栏目下标：复本数
        /// </summary>
        public const int COLUMN_COPY = 8;
        /// <summary>
        /// ListView 栏目下标：价格
        /// </summary>
        public const int COLUMN_PRICE = 9;
        /// <summary>
        /// ListView 栏目下标：总价格
        /// </summary>
        public const int COLUMN_TOTALPRICE = 10;

        /// <summary>
        /// ListView 栏目下标：码洋
        /// </summary>
        public const int COLUMN_FIXEDPRICE = 11;

        /// <summary>
        /// ListView 栏目下标：折扣
        /// </summary>
        public const int COLUMN_DISCOUNT = 12;

        /// <summary>
        /// ListView 栏目下标：订购时间
        /// </summary>
        public const int COLUMN_ORDERTIME = 13;
        /// <summary>
        /// ListView 栏目下标：订单 ID
        /// </summary>
        public const int COLUMN_ORDERID = 14;
        /// <summary>
        /// ListView 栏目下标：馆藏分配去向
        /// </summary>
        public const int COLUMN_DISTRIBUTE = 15;
        /// <summary>
        /// ListView 栏目下标：类目
        /// </summary>
        public const int COLUMN_CLASS = 16;
        /// <summary>
        /// ListView 栏目下标：注释
        /// </summary>
        public const int COLUMN_COMMENT = 17;
        /// <summary>
        /// ListView 栏目下标：批次号
        /// </summary>
        public const int COLUMN_BATCHNO = 18;
        /// <summary>
        /// ListView 栏目下标：渠道地址
        /// </summary>
        public const int COLUMN_SELLERADDRESS = 19;
        /// <summary>
        /// ListView 栏目下标：参考 ID
        /// </summary>
        public const int COLUMN_REFID = 20;
        /// <summary>
        /// ListView 栏目下标：操作历史信息
        /// </summary>
        public const int COLUMN_OPERATIONS = 21;
        /// <summary>
        /// ListView 栏目下标：订购记录路径
        /// </summary>
        public const int COLUMN_RECPATH = 22;

        #region 数据成员

        /// <summary>
        /// 操作
        /// </summary>
        public string Operations
        {
            get
            {
                return DomUtil.GetElementInnerXml(this.RecordDom.DocumentElement,
                    "operations");
            }
            set
            {
                // 注意，可能抛出异常
                DomUtil.SetElementInnerXml(this.RecordDom.DocumentElement,
                    "operations",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 编号
        /// </summary>
        public string Index
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "index");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "index",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public string State
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "state");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "state",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 书目号
        /// </summary>
        public string CatalogNo
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "catalogNo");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "catalogNo",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 渠道(书商)
        /// </summary>
        public string Seller
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "seller");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "seller",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 经费来源
        /// </summary>
        public string Source
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "source");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "source",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 时间范围
        /// </summary>
        public string Range
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "range");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "range",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 包含期数
        /// </summary>
        public string IssueCount
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "issueCount");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "issueCount",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 复本数
        /// </summary>
        public string Copy
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "copy");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "copy",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 册价格
        /// </summary>
        public string Price
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "price");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "price", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 总价格
        /// </summary>
        public string TotalPrice
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "totalPrice");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "totalPrice",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 码洋
        /// </summary>
        public string FixedPrice
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "fixedPrice");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "fixedPrice",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 折扣
        /// </summary>
        public string Discount
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "discount");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "discount",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 订购时间 RFC1123格式
        /// </summary>
        public string OrderTime
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "orderTime");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "orderTime",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderID
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "orderID");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "orderID",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }


        /// <summary>
        /// 馆藏分配
        /// </summary>
        public string Distribute
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "distribute");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "distribute",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 类别
        /// </summary>
        public string Class
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "class");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "class",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "comment");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "comment",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 批次号
        /// </summary>
        public string BatchNo
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "batchNo");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "batchNo",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 渠道地址
        /// </summary>
        public string SellerAddress
        {
            get
            {
                return DomUtil.GetElementInnerXml(this.RecordDom.DocumentElement,
                    "sellerAddress");
            }
            set
            {
                // 注意，可能抛出异常
                DomUtil.SetElementInnerXml(this.RecordDom.DocumentElement,
                    "sellerAddress",
                    value);
            }
        }

        #endregion

        // 2013/6/20
        /// <summary>
        /// 将内存值更新到显示的栏目
        /// </summary>
        /// <param name="item">ListViewItem事项，ListView中的一行</param>
        public override void SetItemColumns(ListViewItem item)
        {
            ListViewUtil.ChangeItemText(item,
    COLUMN_ERRORINFO,
    this.ErrorInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CATALOGNO,
    this.CatalogNo);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SELLER,
    this.Seller);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SOURCE,
    this.Source);
            ListViewUtil.ChangeItemText(item,
    COLUMN_RANGE,
    this.Range);
            ListViewUtil.ChangeItemText(item,
    COLUMN_ISSUECOUNT,
    this.IssueCount);
            ListViewUtil.ChangeItemText(item,
    COLUMN_COPY,
    this.Copy);

            ListViewUtil.ChangeItemText(item,
    COLUMN_PRICE,
    this.Price);
            ListViewUtil.ChangeItemText(item,
                COLUMN_TOTALPRICE,
                this.TotalPrice);

            ListViewUtil.ChangeItemText(item,
    COLUMN_FIXEDPRICE,
    this.FixedPrice);

            ListViewUtil.ChangeItemText(item,
    COLUMN_DISCOUNT,
    this.Discount);

            // 2015/1/28
            string strOrderTime = "";
            try
            {
                strOrderTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(this.OrderTime, "s");
            }
            catch (Exception)
            {
                strOrderTime = "订购时间字符串 '"+this.OrderTime+"' 格式不合法";
            }
            ListViewUtil.ChangeItemText(item,
COLUMN_ORDERTIME,
strOrderTime);

            ListViewUtil.ChangeItemText(item,
    COLUMN_ORDERID,
    this.OrderID);
            ListViewUtil.ChangeItemText(item,
    COLUMN_DISTRIBUTE,
    this.Distribute);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CLASS,
    this.Class);
            ListViewUtil.ChangeItemText(item,
    COLUMN_COMMENT,
    this.Comment);
            ListViewUtil.ChangeItemText(item,
    COLUMN_BATCHNO,
    this.BatchNo);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SELLERADDRESS,
    this.SellerAddress);
            ListViewUtil.ChangeItemText(item,
    COLUMN_REFID,
    this.RefID);
            ListViewUtil.ChangeItemText(item,
    COLUMN_OPERATIONS,
    this.Operations);
            ListViewUtil.ChangeItemText(item,
                COLUMN_RECPATH,
                this.RecPath);
        }
    }

    /// <summary>
    /// 订购信息的集合容器
    /// </summary>
    [Serializable()]
    public class OrderItemCollection : BookItemCollectionBase
    {
        /// <summary>
        /// 以编号定位一个事项
        /// </summary>
        /// <param name="strIndex">编号</param>
        /// <param name="excludeItems">判断中需要排除的事项</param>
        /// <returns>找到的事项。null 表示没有找到</returns>
        public OrderItem GetItemByIndex(string strIndex,
            List<OrderItem> excludeItems)
        {
            foreach (OrderItem item in this)
            {
                // 需要排除的事项
                if (excludeItems != null)
                {
                    if (excludeItems.IndexOf(item) != -1)
                        continue;
                }

                if (item.Index == strIndex)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 将集合中的全部事项信息输出为一个完整的 XML 格式字符串
        /// </summary>
        /// <param name="strXml">XML 字符串</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
        public int BuildXml(
out string strXml,
out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<orders />");

            foreach (OrderItem item in this)
            {
                XmlNode node = dom.CreateElement("dprms", "order", DpNs.dprms);
                dom.DocumentElement.AppendChild(node);

                node.InnerXml = item.RecordDom.DocumentElement.InnerXml;
            }

            strXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        // parameters:
        //       changed_refids  累加修改过的 refid 对照表。 原来的 --> 新的
        /// <summary>
        /// 根据一个 XML 字符串内容，构建出集合内的若干事项
        /// </summary>
        /// <param name="nodeOrderCollection">XmlNode对象，本方法将使用其下属的 dprms:order 元素来构造事项</param>
        /// <param name="list">ListView 对象。构造好的事项会显示到其中</param>
        /// <param name="bRefreshRefID">构造事项的过程中，是否要刷新每个事项的 RefID 成员值</param>
        /// <param name="changed_refids">累加修改过的 refid 对照表。 原来的 --> 新的</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
        public int ImportFromXml(XmlNode nodeOrderCollection,
            ListView list,
            bool bRefreshRefID,
            ref Hashtable changed_refids,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nodeOrderCollection == null)
                return 0;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = nodeOrderCollection.SelectNodes("dprms:order", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                OrderItem order_item = new OrderItem();
                nRet = order_item.SetData("",
                    node.OuterXml,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bRefreshRefID == true)
                {
                    string strOldRefID = order_item.RefID;
                    order_item.RefID = Guid.NewGuid().ToString();

                    changed_refids[strOldRefID] = order_item.RefID;
                }

                // TODO: distribute 里面的册参考 ID 需要替换

                this.Add(order_item);
                order_item.ItemDisplayState = ItemDisplayState.New;
                order_item.AddToListView(list);

                order_item.Changed = true;
            }

            return 0;
        }
    }
}
