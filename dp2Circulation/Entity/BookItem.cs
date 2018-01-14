using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

using DigitalPlatform.LibraryClient.localhost;  // EntityInfo

namespace dp2Circulation
{
    /// <summary>
    /// 册信息
    /// </summary>
    [Serializable()]
    public class BookItem : BookItemBase
    {
        // 列index。注意要保持和EntityControl中的列号一致

        /// <summary>
        /// ListView 栏目下标：册条码号
        /// </summary>
        public const int COLUMN_BARCODE = 0;
        /// <summary>
        /// ListView 栏目下标：错误信息
        /// </summary>
        public const int COLUMN_ERRORINFO = 1;
        /// <summary>
        /// ListView 栏目下标：记录状态
        /// </summary>
        public const int COLUMN_STATE = 2;
        /// <summary>
        /// ListView 栏目下标：出版时间
        /// </summary>
        public const int COLUMN_PUBLISHTIME = 3;
        /// <summary>
        /// ListView 栏目下标：馆藏地点
        /// </summary>
        public const int COLUMN_LOCATION = 4;
        /// <summary>
        /// ListView 栏目下标：渠道
        /// </summary>
        public const int COLUMN_SELLER = 5;
        /// <summary>
        /// ListView 栏目下标：经费来源
        /// </summary>
        public const int COLUMN_SOURCE = 6;
        /// <summary>
        /// ListView 栏目下标：价格
        /// </summary>
        public const int COLUMN_PRICE = 7;
        /// <summary>
        /// ListView 栏目下标：卷册信息
        /// </summary>
        public const int COLUMN_VOLUME = 8;
        /// <summary>
        /// ListView 栏目下标：索取号
        /// </summary>
        public const int COLUMN_ACCESSNO = 9;
        /// <summary>
        /// ListView 栏目下标：架号
        /// </summary>
        public const int COLUMN_SHELFNO = 10;
        /// <summary>
        /// ListView 栏目下标：图书类型
        /// </summary>
        public const int COLUMN_BOOKTYPE = 11;
        /// <summary>
        /// ListView 栏目下标：登录号
        /// </summary>
        public const int COLUMN_REGISTERNO = 12;
        /// <summary>
        /// ListView 栏目下标：注释
        /// </summary>
        public const int COLUMN_COMMENT = 13;
        /// <summary>
        /// ListView 栏目下标：合并注释
        /// </summary>
        public const int COLUMN_MERGECOMMENT = 14;
        /// <summary>
        /// ListView 栏目下标：批次号
        /// </summary>
        public const int COLUMN_BATCHNO = 15;
        /// <summary>
        /// ListView 栏目下标：借阅者
        /// </summary>
        public const int COLUMN_BORROWER = 16;
        /// <summary>
        /// ListView 栏目下标：借阅日期
        /// </summary>
        public const int COLUMN_BORROWDATE = 17;
        /// <summary>
        /// ListView 栏目下标：借阅期限
        /// </summary>
        public const int COLUMN_BORROWPERIOD = 18;

        /// <summary>
        /// ListView 栏目下标：完好率
        /// </summary>
        public const int COLUMN_INTACT = 19;
        /// <summary>
        /// ListView 栏目下标：装订费用
        /// </summary>
        public const int COLUMN_BINDINGCOST = 20;
        /// <summary>
        /// ListView 栏目下标：装订信息
        /// </summary>
        public const int COLUMN_BINDING = 21;
        /// <summary>
        /// ListView 栏目下标：操作历史信息
        /// </summary>
        public const int COLUMN_OPERATIONS = 22;

        /// <summary>
        /// ListView 栏目下标：册记录路径
        /// </summary>
        public const int COLUMN_RECPATH = 23;
        /// <summary>
        /// ListView 栏目下标：参考 ID
        /// </summary>
        public const int COLUMN_REFID = 24;

        /// <summary>
        /// 根据当前对象克隆出一个新对象
        /// </summary>
        /// <returns>新对象</returns>
        public BookItem Clone()
        {
            BookItem item = new BookItem();
            this.CopyTo(item);
            return item;
        }

        /// <summary>
        /// 根据指定的栏目号设置字段内容
        /// </summary>
        /// <param name="nCol">栏目号</param>
        /// <param name="strText">要设置的文字</param>
        public void SetColumnText(int nCol, string strText)
        {
            if (nCol == COLUMN_BARCODE)
                this.Barcode = strText;
            else if (nCol == COLUMN_ERRORINFO)
                this.ErrorInfo = strText;

            else if (nCol == COLUMN_STATE)
                this.State = strText;
            else if (nCol == COLUMN_PUBLISHTIME)
                this.PublishTime = strText;
            else if (nCol == COLUMN_LOCATION)
                this.Location = strText;
            else if (nCol == COLUMN_SELLER)
                this.Seller = strText;
            else if (nCol == COLUMN_SOURCE)
                this.Source = strText;
            else if (nCol == COLUMN_PRICE)
                this.Price = strText;
            else if (nCol == COLUMN_VOLUME)
                this.Volume = strText;
            else if (nCol == COLUMN_ACCESSNO)
                this.AccessNo = strText;
            else if (nCol == COLUMN_SHELFNO)
                this.ShelfNo = strText;
            else if (nCol == COLUMN_BOOKTYPE)
                this.BookType = strText;
            else if (nCol == COLUMN_REGISTERNO)
                this.RegisterNo = strText;
            else if (nCol == COLUMN_COMMENT)
                this.Comment = strText;
            else if (nCol == COLUMN_MERGECOMMENT)
                this.MergeComment = strText;
            else if (nCol == COLUMN_BATCHNO)
                this.BatchNo = strText;
            else if (nCol == COLUMN_BORROWER)
                this.Borrower = strText;
            else if (nCol == COLUMN_BORROWDATE)
                this.BorrowDate = strText;
            else if (nCol == COLUMN_BORROWPERIOD)
                this.BorrowPeriod = strText;

            else if (nCol == COLUMN_INTACT)
                this.Intact = strText;
            else if (nCol == COLUMN_BINDINGCOST)
                this.BindingCost = strText;
            else if (nCol == COLUMN_BINDING)
                this.Binding = strText;
            else if (nCol == COLUMN_OPERATIONS)
                this.Operations = strText;

            else if (nCol == COLUMN_RECPATH)
                this.RecPath = strText;
            else if (nCol == COLUMN_REFID)
                this.RefID = strText;
            else
                throw new Exception("未知的列号 " + nCol.ToString());

        }

        #region 数据成员

        /*
        string m_strTempRefID = "";

        public string TempRefID
        {

            get
            {
                // TODO: 如何刷新显示?
                if (String.IsNullOrEmpty(this.m_strTempRefID) == true)
                    this.m_strTempRefID = Guid.NewGuid().ToString();

                return this.m_strTempRefID;
            }
        }*/


        /// <summary>
        ///  册条码号
        /// </summary>
        public string Barcode
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "barcode");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "barcode", value);
                this.Changed = true; // 2009/3/5 
            }
        }

        /// <summary>
        /// 登录号 (2006/9/25 增加)
        /// </summary>
        public string RegisterNo
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "registerNo");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "registerNo", value);
                this.Changed = true; // 2009/3/5 
            }
        }

        /// <summary>
        /// 册状态
        /// </summary>
        public string State
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "state");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "state", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 出版时间 2007/10/24 
        /// </summary>
        public string PublishTime
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "publishTime");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "publishTime", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 渠道(书商) 2007/10/24 
        /// </summary>
        public string Seller
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "seller");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "seller", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 采购经费来源 2008/2/15 
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
                    "source", value);
                this.Changed = true; // 2009/3/5
            }
        }



        /// <summary>
        /// 馆藏地点
        /// </summary>
        public string Location
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "location");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "location", value);
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
        /// 册类型
        /// </summary>
        public string BookType
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "bookType");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "bookType", value);
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
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "comment");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "comment", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 合并注释 (2006/9/25 增加)
        /// </summary>
        public string MergeComment
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "mergeComment");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "mergeComment", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 批次号 (2006/9/29 增加)
        /// </summary>
        public string BatchNo
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "batchNo");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "batchNo", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 卷期 (2007/10/19 增加)
        /// </summary>
        public string Volume
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "volume");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "volume", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 索取号 (2008/12/12 增加)
        /// </summary>
        public string AccessNo
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "accessNo");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "accessNo", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 架号 (2017/6/15 增加)
        /// </summary>
        public string ShelfNo
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "shelfNo");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "shelfNo", value);
                this.Changed = true; 
            }
        }

        /// <summary>
        /// 借书人条码
        /// </summary>
        public string Borrower
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "borrower");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "borrower", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 借书的日期
        /// </summary>
        public string BorrowDate
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "borrowDate");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "borrowDate", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 借阅期限
        /// </summary>
        public string BorrowPeriod
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "borrowPeriod");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "borrowPeriod", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 完好率
        /// </summary>
        public string Intact
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "intact");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "intact", value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 装订费
        /// </summary>
        public string BindingCost
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "bindingCost");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "bindingCost", value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 装订信息
        /// </summary>
        public string Binding
        {
            get
            {
                return DomUtil.GetElementInnerXml(this.RecordDom.DocumentElement,
                    "binding");
            }
            set
            {
                // 注意，可能抛出异常
                DomUtil.SetElementInnerXml(this.RecordDom.DocumentElement,
                    "binding",
                    value);
                this.Changed = true;
            }
        }

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

        #endregion


        // parameters:
        // return:
        //      -1  出错
        //      0   没有发生修改
        //      1   发生了修改
        /// <summary>
        /// 更换 binding 元素里的 item 元素中的 refID 属性值字符串
        /// </summary>
        /// <param name="item_refid_change_table">参考 ID 对照表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有发生修改; 1: 发生了修改</returns>
        public int ReplaceBindingItemRefID(Hashtable item_refid_change_table,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            string strBinding = this.Binding;
            if (String.IsNullOrEmpty(strBinding) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<binding/>");
            try
            {
                dom.DocumentElement.InnerXml = strBinding;
            }
            catch (Exception ex)
            {
                strError = "load inner xml error: " + ex.Message;
                return -1;
            }

            int nChangeCount = ReplaceBindingItemRefID(item_refid_change_table,
            dom.DocumentElement,
            out strError);

            if (nChangeCount > 0)
            {
                this.Binding = dom.DocumentElement.InnerXml;
                return 1;
            }

            return 0;
        }

        // parameters:
        //      root    指 binding 元素
        public static int ReplaceBindingItemRefID(Hashtable item_refid_change_table,
            XmlElement binding,
            out string strError)
        {
            strError = "";

            // bool bChanged = false;
            int nChangeCount = 0;
            XmlNodeList nodes = binding.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strRefID = DomUtil.GetAttr(node, "refID");
                if (String.IsNullOrEmpty(strRefID) == false)
                {
                    if (item_refid_change_table.Contains(strRefID) == true)
                    {
                        DomUtil.SetAttr(node, "refID", (string)item_refid_change_table[strRefID]);
                        // bChanged = true;
                        nChangeCount++;
                    }
                }
            }

            return nChangeCount;
        }


        /// <summary>
        /// 将内存值更新到显示的栏目
        /// </summary>
        /// <param name="item">ListViewItem事项，ListView中的一行</param>
        public override void SetItemColumns(ListViewItem item)
        {
            ListViewUtil.ChangeItemText(item,
    COLUMN_BARCODE,
    this.Barcode);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ERRORINFO,
                this.ErrorInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item,
                COLUMN_PUBLISHTIME,
                this.PublishTime);
            ListViewUtil.ChangeItemText(item,
                COLUMN_LOCATION,
                this.Location);

            ListViewUtil.ChangeItemText(item,
                COLUMN_SELLER,
                this.Seller);
            ListViewUtil.ChangeItemText(item,
                COLUMN_SOURCE,
                this.Source);

            ListViewUtil.ChangeItemText(item,
                COLUMN_PRICE,
                this.Price);

            ListViewUtil.ChangeItemText(item,
                COLUMN_VOLUME,
                this.Volume);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ACCESSNO,
                this.AccessNo);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SHELFNO,
    this.ShelfNo);


            ListViewUtil.ChangeItemText(item,
                COLUMN_BOOKTYPE,
                this.BookType);
            ListViewUtil.ChangeItemText(item,
                COLUMN_REGISTERNO,
                this.RegisterNo);

            ListViewUtil.ChangeItemText(item,
                COLUMN_COMMENT,
                this.Comment);
            ListViewUtil.ChangeItemText(item,
                COLUMN_MERGECOMMENT,
                this.MergeComment);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BATCHNO,
                this.BatchNo);

            ListViewUtil.ChangeItemText(item,
                COLUMN_BORROWER,
                this.Borrower);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BORROWDATE,
                this.BorrowDate);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BORROWPERIOD,
                this.BorrowPeriod);

            ListViewUtil.ChangeItemText(item,
                COLUMN_INTACT,
                this.Intact);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BINDINGCOST,
                this.BindingCost);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BINDING,
                this.Binding);
            ListViewUtil.ChangeItemText(item,
                COLUMN_OPERATIONS,
                this.Operations);

            ListViewUtil.ChangeItemText(item,
                COLUMN_RECPATH,
                this.RecPath);
            ListViewUtil.ChangeItemText(item,
                COLUMN_REFID,
                this.RefID);
        }


    }

    /// <summary>
    /// 册信息的集合容器
    /// </summary>
    [Serializable()]
    public class BookItemCollection : BookItemCollectionBase
    {
        /// <summary>
        /// 构造索取号信息集合
        /// </summary>
        /// <returns>CallNumberItem事项集合</returns>
        public List<CallNumberItem> GetCallNumberItems()
        {
            List<CallNumberItem> results = new List<CallNumberItem>();
            foreach (BookItem book_item in this)
            {
                CallNumberItem item = new CallNumberItem();
                item.RecPath = book_item.RecPath;

#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(item.RecPath))
                {
                    if (string.IsNullOrEmpty(book_item.RefID) == true)
                        throw new Exception("BookItem 的 RefID 成员不应为空"); // TODO: 可以考虑增加健壮性，当时发生 RefID 字符串

                    item.RecPath = "@refID:" + book_item.RefID;
                }
#endif

                item.CallNumber = DomUtil.GetElementText(book_item.RecordDom.DocumentElement, "accessNo");
                item.Location = DomUtil.GetElementText(book_item.RecordDom.DocumentElement, "location");
                item.Barcode = DomUtil.GetElementText(book_item.RecordDom.DocumentElement, "barcode");

                results.Add(item);
            }

            return results;
        }

        /// <summary>
        /// 以册条码号定位一个事项
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <returns>事项</returns>
        public BookItem GetItemByBarcode(string strBarcode)
        {
            foreach (BookItem item in this)
            {
                if (item.Barcode == strBarcode)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 以登录号定位一个事项
        /// </summary>
        /// <param name="strRegisterNo">登录号</param>
        /// <returns>事项</returns>
        public BookItem GetItemByRegisterNo(string strRegisterNo)
        {
            foreach (BookItem item in this)
            {
                if (item.RegisterNo == strRegisterNo)
                    return item;
            }

            return null;
        }

        // 2008/11/4
        /// <summary>
        /// 选定(加亮)匹配指定批次号的那些行
        /// </summary>
        /// <param name="strBatchNo">批次号</param>
        /// <param name="bClearOthersHilight">同时清除其它事项的加亮状态</param>
        public void SelectItemsByBatchNo(string strBatchNo,
            bool bClearOthersHilight)
        {
            if (this.Count == 0)
                return;

            ListView list = this[0].ListViewItem.ListView;
            int first_hilight_item_index = -1;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem listview_item = list.Items[i];

                BookItem book_item = (BookItem)listview_item.Tag;

                Debug.Assert(book_item != null, "");

                if (book_item.BatchNo == strBatchNo)
                {
                    listview_item.Selected = true;
                    if (first_hilight_item_index == -1)
                        first_hilight_item_index = i;
                }
                else
                {
                    if (bClearOthersHilight == true)
                        listview_item.Selected = false;
                }
            }

            // 滚入视野范围
            if (first_hilight_item_index != -1)
                list.EnsureVisible(first_hilight_item_index);
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
            dom.LoadXml("<items />");

            foreach (BookItem item in this)
            {
                XmlNode node = dom.CreateElement("dprms", "item", DpNs.dprms);
                dom.DocumentElement.AppendChild(node);

                node.InnerXml = item.RecordDom.DocumentElement.InnerXml;
            }

            strXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        // parameters:
        //      refid_change_table  修改过的refid。key为旧值，value为新值
        /// <summary>
        /// 根据一个 XML 字符串内容，构建出集合内的若干事项
        /// </summary>
        /// <param name="nodeItemCollection">XmlNode对象，本方法将使用其下属的 dprms:item 元素来构造事项</param>
        /// <param name="list">ListView 对象。构造好的事项会显示到其中</param>
        /// <param name="bRefreshRefID">构造事项的过程中，是否要刷新每个事项的 RefID 成员值</param>
        /// <param name="refid_change_table">返回修改过的refid。key为旧值，value为新值</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
        public int ImportFromXml(XmlNode nodeItemCollection,
            ListView list,
            bool bRefreshRefID,
            out Hashtable refid_change_table,
            out string strError)
        {
            strError = "";
            refid_change_table = new Hashtable();
            int nRet = 0;

            if (nodeItemCollection == null)
                return 0;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = nodeItemCollection.SelectNodes("dprms:item", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                BookItem book_item = new BookItem();
                nRet = book_item.SetData("",
                    node.OuterXml,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bRefreshRefID == true)
                {
                    string strOldRefID = book_item.RefID;
                    book_item.RefID = Guid.NewGuid().ToString();
                    if (String.IsNullOrEmpty(strOldRefID) == false)
                    {
                        refid_change_table[strOldRefID] = book_item.RefID;
                    }
                }

                this.Add(book_item);
                book_item.ItemDisplayState = ItemDisplayState.New;
                book_item.AddToListView(list);

                book_item.Changed = true;
            }

            // 更换<binding>元素内<item>元素的refID属性值
            if (bRefreshRefID == true
                && refid_change_table.Count > 0)
            {
                foreach (BookItem item in this)
                {
                    nRet = item.ReplaceBindingItemRefID(refid_change_table,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// 事项的显示状态
    /// </summary>
    public enum ItemDisplayState
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 新增
        /// </summary>
        New = 1,
        /// <summary>
        /// 发生过修改
        /// </summary>
        Changed = 2,
        /// <summary>
        /// 被删除
        /// </summary>
        Deleted = 3,
    }
}
