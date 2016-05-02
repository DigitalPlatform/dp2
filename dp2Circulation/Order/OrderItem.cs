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
#if NO
        public ItemDisplayState ItemDisplayState = ItemDisplayState.Normal;
#endif

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
        /// ListView 栏目下标：订购时间
        /// </summary>
        public const int COLUMN_ORDERTIME = 11;
        /// <summary>
        /// ListView 栏目下标：订单 ID
        /// </summary>
        public const int COLUMN_ORDERID = 12;
        /// <summary>
        /// ListView 栏目下标：馆藏分配去向
        /// </summary>
        public const int COLUMN_DISTRIBUTE = 13;
        /// <summary>
        /// ListView 栏目下标：类目
        /// </summary>
        public const int COLUMN_CLASS = 14;
        /// <summary>
        /// ListView 栏目下标：注释
        /// </summary>
        public const int COLUMN_COMMENT = 15;
        /// <summary>
        /// ListView 栏目下标：批次号
        /// </summary>
        public const int COLUMN_BATCHNO = 16;
        /// <summary>
        /// ListView 栏目下标：渠道地址
        /// </summary>
        public const int COLUMN_SELLERADDRESS = 17;
        /// <summary>
        /// ListView 栏目下标：参考 ID
        /// </summary>
        public const int COLUMN_REFID = 18;
        /// <summary>
        /// ListView 栏目下标：操作历史信息
        /// </summary>
        public const int COLUMN_OPERATIONS = 19;
        /// <summary>
        /// ListView 栏目下标：订购记录路径
        /// </summary>
        public const int COLUMN_RECPATH = 20;

        #region 数据成员

#if NO
        public string RefID
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "refID");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "refID", value);
                this.Changed = true;
            }
        }

                /// <summary>
        /// 从属的书目记录id
        /// </summary>
        public string Parent
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "parent");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "parent",
                    value);
                this.Changed = true; // 2009/3/5
            }
        }
#endif

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

#if NO
        /// <summary>
        /// 订购记录路径
        /// </summary>
        public string RecPath = "";

        /// <summary>
        /// 是否被修改
        /// </summary>
        bool m_bChanged = false;

        public string OldRecord = "";

        public string CurrentRecord = "";   // 在Serialize过程中用来储存RecordDom内容

        /// <summary>
        /// 记录的dom
        /// </summary>
        [NonSerialized()]
        public XmlDocument RecordDom = new XmlDocument();

        // 恢复那些不能序列化的成员值
        public void RestoreNonSerialized()
        {
            this.RecordDom = new XmlDocument();

            if (String.IsNullOrEmpty(this.CurrentRecord) == false)
            {
                this.RecordDom.LoadXml(this.CurrentRecord);
                this.CurrentRecord = "";    // 完成了任务
            }
            else
                this.RecordDom.LoadXml("<root />");

        }

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        [NonSerialized()]
        internal ListViewItem ListViewItem = null;

        public string ErrorInfo
        {
            get
            {
                if (this.Error == null)
                    return "";
                return this.Error.ErrorInfo;
            }
        }

        public EntityInfo Error = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderItem()
        {
            this.RecordDom.LoadXml("<root />");
        }

        public OrderItem Clone()
        {
            OrderItem newObject = new OrderItem();

            newObject.ItemDisplayState = this.ItemDisplayState;

            newObject.RecPath = this.RecPath;
            newObject.m_bChanged = this.m_bChanged;
            newObject.OldRecord = this.OldRecord;


            // 放入最新鲜的内容
            newObject.CurrentRecord = this.RecordDom.OuterXml;


            newObject.RecordDom = new XmlDocument();
            newObject.RecordDom.LoadXml(this.RecordDom.OuterXml);

            newObject.Timestamp = ByteArray.GetCopy(this.Timestamp);
            newObject.ListViewItem = null;  // this.ListViewItem;
            newObject.Error = null; // this.Error;

            return newObject;
        }

        // 设置数据
        public int SetData(string strRecPath,
            string strXml,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            Debug.Assert(this.RecordDom != null);
            // 可能抛出异常
            try
            {
                this.RecordDom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装载到DOM时出错: " + ex.Message;
                return -1;
            }

            this.OldRecord = strXml;

            this.RecPath = strRecPath;
            this.Timestamp = baTimeStamp;

            return 0;
        }

        // 重新设置数据
        public int ResetData(
            string strRecPath,
            string strNewXml,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            this.RecPath = strRecPath;
            this.Timestamp = baTimeStamp;

            Debug.Assert(this.RecordDom != null);
            try
            {
                this.RecordDom.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "xml装载到DOM时出错: " + ex.Message;
                return -1;
            }

            // this.Initial();

            this.Changed = false;   // 2009/3/5
            this.ItemDisplayState = ItemDisplayState.Normal;

            // this.RefreshListView();
            return 0;
        }


        /// <summary>
        /// 创建好适合于保存的记录信息
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int BuildRecord(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";


            if (this.Parent == "")
            {
                strError = "Parent成员尚未定义";
                return -1;
            }

            strXml = this.RecordDom.OuterXml;

            return 0;
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return m_bChanged;
            }
            set
            {
                m_bChanged = value;

                // 2009/3/5
                if ((this.ItemDisplayState == ItemDisplayState.Normal)
                    && this.m_bChanged == true)
                    this.ItemDisplayState = ItemDisplayState.Changed;
                else if ((this.ItemDisplayState == ItemDisplayState.Changed)
                    && this.m_bChanged == false)
                    this.ItemDisplayState = ItemDisplayState.Normal;
            }

        }


        /// <summary>
        /// 将本事项加入到listview中
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ListViewItem AddToListView(ListView list)
        {
            ListViewItem item = new ListViewItem(this.Index, 0);

            /*
            item.SubItems.Add(this.ErrorInfo);
            item.SubItems.Add(this.State);
            item.SubItems.Add(this.CatalogNo);  // 2008/8/31
            item.SubItems.Add(this.Seller);

            item.SubItems.Add(this.Source);

            item.SubItems.Add(this.Range);
            item.SubItems.Add(this.IssueCount);
            item.SubItems.Add(this.Copy);
            item.SubItems.Add(this.Price);

            item.SubItems.Add(this.TotalPrice);
            item.SubItems.Add(this.OrderTime);
            item.SubItems.Add(this.OrderID);
            item.SubItems.Add(this.Distribute);
            item.SubItems.Add(this.Class);


            item.SubItems.Add(this.Comment);
            item.SubItems.Add(this.BatchNo);

            item.SubItems.Add(this.SellerAddress);  // 2009/2/13

            item.SubItems.Add(this.RefID);  // 2010/3/15
            item.SubItems.Add(this.RecPath);
             * */
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
    COLUMN_ORDERTIME,
    this.OrderTime);
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

            this.SetItemBackColor(item);

            list.Items.Add(item);

            Debug.Assert(item.ListView != null, "");

            this.ListViewItem = item;

            this.ListViewItem.Tag = this;   // 将OrderItem对象引用保存在ListViewItem事项中

            return item;
        }

#endif


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
#if NO
            ListViewUtil.ChangeItemText(item,
    COLUMN_ORDERTIME,
    this.OrderTime);
#endif
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

#if NO
        public void DeleteFromListView()
        {
            Debug.Assert(this.ListViewItem.ListView != null, "");
            ListView list = this.ListViewItem.ListView;

            list.Items.Remove(this.ListViewItem);
        }

        // 刷新背景颜色和图标
        void SetItemBackColor(ListViewItem item)
        {
            if ((this.ItemDisplayState == ItemDisplayState.Normal)
                && this.Changed == true)
            {
                Debug.Assert(false, "ItemDisplayState.Normal状态和Changed == true矛盾了");
            }
            else if ((this.ItemDisplayState == ItemDisplayState.Changed)
                && this.Changed == false) // 2009/3/5
            {
                Debug.Assert(false, "ItemDisplayState.Changed状态和Changed == false矛盾了");
            }

            if (String.IsNullOrEmpty(this.ErrorInfo) == false)
            {
                // 出错的事项
                item.BackColor = Color.FromArgb(255, 0, 0); // 纯红色
                item.ForeColor = Color.White;
            }
            else if (this.ItemDisplayState == ItemDisplayState.Normal)
            {
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
            }
            else if (this.ItemDisplayState == ItemDisplayState.Changed)
            {
                // 修改过的旧事项
                item.BackColor = Color.FromArgb(100, 255, 100); // 浅绿色
                item.ForeColor = SystemColors.WindowText;
            }
            else if (this.ItemDisplayState == ItemDisplayState.New)
            {
                // 新事项
                item.BackColor = Color.FromArgb(255, 255, 100); // 浅黄色
                item.ForeColor = SystemColors.WindowText;
            }
            else if (this.ItemDisplayState == ItemDisplayState.Deleted)
            {
                // 删除的事项
                item.BackColor = Color.FromArgb(255, 150, 150); // 浅红色
                item.ForeColor = SystemColors.WindowText;
            }
            else // 其他事项
            {
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
            }

            item.ImageIndex = Convert.ToInt32(this.ItemDisplayState);

        }

        /// <summary>
        /// 刷新事项颜色
        /// </summary>
        public void RefreshItemColor()
        {
            if (this.ListViewItem != null)
            {
                this.SetItemBackColor(this.ListViewItem);
            }
        }

        // 刷新各列内容和图标、背景颜色
        public void RefreshListView()
        {
            if (this.ListViewItem == null)
                return;

            ListViewItem item = this.ListViewItem;

            ListViewUtil.ChangeItemText(item, 
                COLUMN_INDEX,
                this.Index);
            ListViewUtil.ChangeItemText(item, 
                COLUMN_ERRORINFO,
                this.ErrorInfo);
            ListViewUtil.ChangeItemText(item, 
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item, 
                COLUMN_CATALOGNO,
                this.CatalogNo);   // 2008/8/31
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
                COLUMN_ORDERTIME,
                this.OrderTime);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ORDERID,
                this.OrderID);
            ListViewUtil.ChangeItemText(item, 
                COLUMN_DISTRIBUTE,
                this.Distribute);
            ListViewUtil.ChangeItemText(item,
                COLUMN_CLASS,
                this.Class);  // 2008/8/31

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

            this.SetItemBackColor(item);
        }

        // parameters:
        //      bClearOtherHilight  是否清除其余存在的高亮标记？
        public void HilightListViewItem(bool bClearOtherHilight)
        {
            if (this.ListViewItem == null)
                return;

            int nIndex = -1;
            ListView list = this.ListViewItem.ListView;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item == this.ListViewItem)
                {
                    item.Selected = true;
                    nIndex = i;
                }
                else
                {
                    if (bClearOtherHilight == true)
                    {
                        if (item.Selected == true)
                            item.Selected = false;
                    }
                }
            }

            if (nIndex != -1)
                list.EnsureVisible(nIndex);
        }

#endif
    }

    /// <summary>
    /// 订购信息的集合容器
    /// </summary>
    [Serializable()]
    public class OrderItemCollection : BookItemCollectionBase
    {

#if NO
        // 检查全部事项的Parent值是否适合保存
        // return:
        //      -1  有错误，不适合保存
        //      0   没有错误
        public int CheckParentIDForSave(out string strError)
        {
            strError = "";
            // 检查每个事项的ParentID
            List<string> ids = this.GetParentIDs();
            for (int i = 0; i < ids.Count; i++)
            {
                string strID = ids[i];
                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "订购事项中出现了空的ParentID值";
                    return -1;
                }

                if (strID == "?")
                {
                    strError = "订购事项中出现了'?'式的ParentID值";
                    return -1;
                }
            }

            return 0;
        }

        // 2008/11/28
        public List<string> GetParentIDs()
        {
            List<string> results = new List<string>();

            for (int i = 0; i < this.Count; i++)
            {
                OrderItem item = this[i];
                string strParentID = item.Parent;
                if (results.IndexOf(strParentID) == -1)
                    results.Add(strParentID);
            }

            return results;
        }

        // 设置全部orderitem事项的Parent域
        public void SetParentID(string strParentID)
        {
            for (int i = 0; i < this.Count; i++)
            {
                OrderItem item = this[i];
                if (item.Parent != strParentID) // 避免连带无谓地修改item.Changed 2009/3/6
                    item.Parent = strParentID;
            }
        }

#endif

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

#if NO
        /// <summary>
        /// 以记录路径定位一个事项
        /// </summary>
        /// <param name="strRecPath"></param>
        /// <returns></returns>
        public OrderItem GetItemByRecPath(string strRecPath)
        {
            for (int i = 0; i < this.Count; i++)
            {
                OrderItem item = this[i];
                if (item.RecPath == strRecPath)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 以RefID定位一个事项
        /// </summary>
        /// <param name="strRefID"></param>
        /// <returns></returns>
        public OrderItem GetItemByRefID(string strRefID,
            List<OrderItem> excludeItems)
        {
            for (int i = 0; i < this.Count; i++)
            {
                OrderItem item = this[i];

                // 需要排除的事项
                if (excludeItems != null)
                {
                    if (excludeItems.IndexOf(item) != -1)
                        continue;
                }

                if (item.RefID == strRefID)
                    return item;
            }

            return null;
        }

        bool m_bChanged = false;
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                if (this.m_bChanged == true)
                    return true;

                for (int i = 0; i < this.Count; i++)
                {
                    OrderItem item = this[i];
                    if (item.Changed == true)
                        return true;
                }

                return false;
            }

            set
            {
                // 2012/3/20
                // true和false不对称
                if (value == false)
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        OrderItem item = this[i];
                        if (item.Changed != value)
                            item.Changed = value;
                    }
                    this.m_bChanged = value;
                }
                else
                {
                    this.m_bChanged = value;
                }
            }
        }

        // 标记删除
        public void MaskDeleteItem(
            bool bRemoveFromList,
            OrderItem orderitem)
        {
            if (orderitem.ItemDisplayState == ItemDisplayState.New)
            {
                PhysicalDeleteItem(orderitem);
                return;
            }


            orderitem.ItemDisplayState = ItemDisplayState.Deleted;
            orderitem.Changed = true;

            // 从listview中消失?
            if (bRemoveFromList == true)
                orderitem.DeleteFromListView();
            else
            {
                orderitem.RefreshListView();
            }
        }

        // Undo标记删除
        // return:
        //      false   没有必要Undo
        //      true    已经Undo
        public bool UndoMaskDeleteItem(OrderItem orderitem)
        {
            if (orderitem.ItemDisplayState != ItemDisplayState.Deleted)
                return false;   // 要Undo的事项根本就不是Deleted状态，所以谈不上Undo

            // 因为不知道上次标记删除前数据是否改过，因此全当改过
            orderitem.ItemDisplayState = ItemDisplayState.Changed;
            orderitem.Changed = true;

            // 刷新
            orderitem.RefreshListView();
            return true;
        }

        // 从集合中和视觉上同时删除
        public void PhysicalDeleteItem(
            OrderItem orderitem)
        {
            // 从listview中消失
            orderitem.DeleteFromListView();

            this.Remove(orderitem);
        }

        public void ClearListViewHilight()
        {
            if (this.Count == 0)
                return;

            ListView list = this[0].ListViewItem.ListView;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item.Selected == true)
                    item.Selected = false;
            }
        }

        public new void Clear()
        {
            for (int i = 0; i < this.Count; i++)
            {
                OrderItem item = this[i];
                item.DeleteFromListView();
            }

            base.Clear();
        }

        // 把事项重新全部加入listview
        public void AddToListView(ListView list)
        {
            for (int i = 0; i < this.Count; i++)
            {
                OrderItem item = this[i];
                item.AddToListView(list);
            }
        }

#endif

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
