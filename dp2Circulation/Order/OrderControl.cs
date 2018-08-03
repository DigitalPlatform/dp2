#define NEW_DUP_API

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;

using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    // public partial class OrderControl : UserControl
    /// <summary>
    /// 订购记录列表控件
    /// </summary>
    public partial class OrderControl : OrderControlBase
    {
#if NO
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();
#endif

        /// <summary>
        /// 是否为期刊订购模式？
        /// </summary>
        public bool SeriesMode = false; // 是否为期刊订购模式？

        /*
        // (一次验收操作中)先前已经创建的BookItem
        public List<BookItem> AcceptedBookItems = new List<BookItem>();
         * */

        /// <summary>
        /// 目标记录路径
        /// </summary>
        public string TargetRecPath = "";   // 4种状态：1)这里的路径和当前记录路径一致，表明实体记录就创建在当前记录下；2)这里的路径和当前记录路径不一致，种记录已经存在，需要在它下面创建实体记录；3) 这里的路径仅有库名部分，表示种记录不存在，需要根据当前记录的MARC来创建；4) 这里的路径为空，表示需要通过菜单选择目标库，然后处理方法同3)
        /// <summary>
        /// 验收批次号
        /// </summary>
        public string AcceptBatchNo = "";   // 验收批次号
        /// <summary>
        /// 是否要在验收操作末段自动出现允许输入册条码号的界面?
        /// </summary>
        public bool InputItemsBarcode = true;   // 是否要在验收操作末段自动出现允许输入册条码号的界面?
        /// <summary>
        /// 是否为新创建的册记录设置“加工中”状态
        /// </summary>
        public bool SetProcessingState = true;   // 是否为新创建的册记录设置“加工中”状态 2009/10/19
        /// <summary>
        /// 是否为新创建的册记录创建索取号
        /// </summary>
        public bool CreateCallNumber = true;   // 是否为新创建的册记录创建索取号 2012/5/7

        // 2010/12/5
        /// <summary>
        /// 为册记录中的价格字段设置何种价格值。值为 书目价/订购价/验收价/空白 之一
        /// </summary>
        public string PriceDefault = "验收价";  // 为册记录中的价格字段设置何种价格值。书目价/订购价/验收价/空白

        // 
        /// <summary>
        /// 打开验收目标记录(以便输入条码等)
        /// </summary>
        public event OpenTargetRecordEventHandler OpenTargetRecord = null;

        /// <summary>
        /// 加亮指定验收批次号的实体行
        /// </summary>
        public event HilightTargetItemsEventHandler HilightTargetItem = null;

        /// <summary>
        /// 准备验收
        /// </summary>
        public event PrepareAcceptEventHandler PrepareAccept = null;

        /// <summary>
        /// 设置目标记录路径
        /// </summary>
        public event SetTargetRecPathEventHandler SetTargetRecPath = null;

        // 2012/10/4
        /// <summary>
        /// 检查馆代码是否在管辖范围内
        /// </summary>
        public event VerifyLibraryCodeEventHandler VerifyLibraryCode = null;

        /// <summary>
        /// 创建实体数据
        /// </summary>
        public event GenerateEntityEventHandler GenerateEntity = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderControl()
        {
            InitializeComponent();

            this.m_listView = this.listView;
            this.ItemType = "order";
            this.ItemTypeName = "订购";
        }

#if NO
        public int OrderCount
        {
            get
            {
                if (this.Items != null)
                    return this.Items.Count;

                return 0;
            }
        }

        // 将listview中的订购事项修改为new状态
        public void ChangeAllItemToNewState()
        {
            foreach (OrderItem orderitem in this.Items)
            {
                // OrderItem orderitem = this.OrderItems[i];

                if (orderitem.ItemDisplayState == ItemDisplayState.Normal
                    || orderitem.ItemDisplayState == ItemDisplayState.Changed
                    || orderitem.ItemDisplayState == ItemDisplayState.Deleted)   // 注意未提交的deleted也变为new了
                {
                    orderitem.ItemDisplayState = ItemDisplayState.New;
                    orderitem.RefreshListView();
                    orderitem.Changed = true;    // 这一句决定了使能后如果立即关闭窗口，是否会警告(实体修改)内容丢失
                }
            }
        }

        public string BiblioRecPath
        {
            get
            {
                return this.m_strBiblioRecPath;
            }
            set
            {
                this.m_strBiblioRecPath = value;

                if (this.Items != null)
                {
                    string strID = Global.GetRecordID(value);
                    this.Items.SetParentID(strID);
                }

            }
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                if (this.Items == null)
                    return false;

                return this.Items.Changed;
            }
            set
            {
                if (this.Items != null)
                    this.Items.Changed = value;
            }
        }

        // 清除listview中的全部事项
        public void Clear()
        {
            this.ListView.Items.Clear();

            // 2009/2/10
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.ListView.Columns);

            // 2012/7/24
            this.TargetRecPath = "";
        }

        // 清除订购有关信息
        public void ClearOrders()
        {
            this.Clear();
            this.Items = new OrderItemCollection();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        public int CountOfVisibleOrderItems()
        {
            return this.ListView.Items.Count;
        }

        public int IndexOfVisibleOrderItems(OrderItem orderitem)
        {
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                OrderItem cur = (OrderItem)this.ListView.Items[i].Tag;

                if (cur == orderitem)
                    return i;
            }

            return -1;
        }

        public OrderItem GetAtVisibleOrderItems(int nIndex)
        {
            return (OrderItem)this.ListView.Items[nIndex].Tag;
        }

#endif

        // 
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 获得一个书目记录下属的全部订购记录路径
        /// </summary>
        /// <param name="stop">Stop对象</param>
        /// <param name="channel">通讯通道</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="recpaths">返回记录路径字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1 出错</para>
        /// <para>0 没有装载</para>
        /// <para>1 已经装载</para>
        /// </returns>
        public static int GetOrderRecPaths(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            out List<string> recpaths,
            out string strError)
        {
            strError = "";
            recpaths = new List<string>();

            long lPerCount = 100; // 每批获得多少个
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;
            for (; ; )
            {
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }
                EntityInfo[] entities = null;

                /*
                if (lCount > 0)
                    stop.SetMessage("正在装入册信息 " + lStart.ToString() + "-" + (lStart + lCount - 1).ToString() + " ...");
                 * */

                long lRet = channel.GetOrders(
                    stop,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    "onlygetpath",
                    "zh",
                    out entities,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lResultCount = lRet;

                if (lRet == 0)
                    return 0;

                Debug.Assert(entities != null, "");

                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i].ErrorCode != ErrorCodeValue.NoError)
                    {
                        strError = "路径为 '" + entities[i].OldRecPath + "' 的订购记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
                        return -1;
                    }

                    recpaths.Add(entities[i].OldRecPath);
                }

                lStart += entities.Length;
                if (lStart >= lResultCount)
                    break;

                if (lCount == -1)
                    lCount = lPerCount;

                if (lStart + lCount > lResultCount)
                    lCount = lResultCount - lStart;
            }

            return 1;
            ERROR1:
            return -1;
        }

#if NO
        // 装入订购记录
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        public int LoadOrderRecords(string strBiblioRecPath,
            out string strError)
        {
            this.BiblioRecPath = strBiblioRecPath;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在装入订购信息 ...");
            Stop.BeginLoop();

            this.Update();
            // Program.MainForm.Update();

            try
            {
                // string strHtml = "";
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                this.ClearOrders();

                // 2012/5/9 改写为循环方式
                for (; ; )
                {
                    EntityInfo[] orders = null;

                    long lRet = Channel.GetOrders(
                        Stop,
                        strBiblioRecPath,
                        lStart,
                        lCount,
                        "",
                        "zh",
                        out orders,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    if (lRet == 0)
                        return 0;

                    lResultCount = lRet;

                    Debug.Assert(orders != null, "");

                    this.ListView.BeginUpdate();
                    try
                    {
                        for (int i = 0; i < orders.Length; i++)
                        {
                            if (orders[i].ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + orders[i].OldRecPath + "' 的订购记录装载中发生错误: " + orders[i].ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            // 剖析一个订购xml记录，取出有关信息放入listview中
                            OrderItem orderitem = new OrderItem();

                            int nRet = orderitem.SetData(orders[i].OldRecPath, // NewRecPath
                                     orders[i].OldRecord,
                                     orders[i].OldTimestamp,
                                     out strError);
                            if (nRet == -1)
                                return -1;

                            if (orders[i].ErrorCode == ErrorCodeValue.NoError)
                                orderitem.Error = null;
                            else
                                orderitem.Error = orders[i];

                            this.Items.Add(orderitem);

                            orderitem.AddToListView(this.ListView);
                        }
                    }
                    finally
                    {
                        this.ListView.EndUpdate();
                    }

                    lStart += orders.Length;
                    if (lStart >= lResultCount)
                        break;
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

#endif

        void designOrder_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = Program.MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(ForegroundWindow.Instance, strError);
            e.values = values;
        }

        // 规划多个订购事项
        void DoDesignOrder()
        {
            string strError = "";
            int nRet = 0;

            // 
            if (this.Items == null)
                this.Items = new OrderItemCollection();

            Debug.Assert(this.Items != null, "");

            OrderDesignForm dlg = new OrderDesignForm();

            this.ParentShowMessage("正在准备数据 ...", "green", false);
            try
            {
                dlg.SeriesMode = this.SeriesMode;   // 2008/12/24
                dlg.BiblioDbName = Global.GetDbName(this.BiblioRecPath);    // 2009/2/15
                dlg.CheckDupItem = true;

                // TODO: 从缺省工作单中获得批次号? 只能直接在缺省工作单中修改?
                // dlg.Text = "订购 -- 批次号:" + this.OrderBatchNo;
                dlg.ClearAllItems();

                // 将已有的订购信息反映到对话框中。
                // 已经发出的订单事项，不能修改。而其他事项都可以修改
                foreach (OrderItem item in this.Items)
                {
                    if (item.ItemDisplayState == ItemDisplayState.Deleted)
                    {
                        strError = "当前存在标记删除的订购事项，必须先提交保存后，才能使用订购规划功能";
                        goto ERROR1;
                    }

                    nRet = item.BuildRecord(
                        true,   // 要检查 Parent 成员
                        out string strOrderXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    DigitalPlatform.CommonControl.Item design_item =
                        dlg.AppendNewItem(strOrderXml, out strError);
                    if (design_item == null)
                        goto ERROR1;

                    design_item.Tag = (object)item; // 建立连接关系
                }

                dlg.Changed = false;

                dlg.GetValueTable -= new GetValueTableEventHandler(designOrder_GetValueTable);
                dlg.GetValueTable += new GetValueTableEventHandler(designOrder_GetValueTable);
                dlg.GetDefaultRecord -= new DigitalPlatform.CommonControl.GetDefaultRecordEventHandler(dlg_GetDefaultRecord);
                dlg.GetDefaultRecord += new DigitalPlatform.CommonControl.GetDefaultRecordEventHandler(dlg_GetDefaultRecord);
                dlg.VerifyLibraryCode += new VerifyLibraryCodeEventHandler(dlg_VerifyLibraryCode);
                dlg.VerifyLibraryCode += new VerifyLibraryCodeEventHandler(dlg_VerifyLibraryCode);
            }
            finally
            {
                this.ParentShowMessage("", "", false);
            }

            Program.MainForm.AppInfo.LinkFormState(dlg,
                "order_design_form_state");

            dlg.FocusedTime = DateTime.Now;

            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool bOldChanged = this.Items.Changed;

            // TODO: 没有被复原的对象，要理解为标记删除
            List<OrderItem> save_orderitems = new List<OrderItem>();
            foreach (OrderItem item in this.Items)
            {
                save_orderitems.Add(item);
            }

            // 先清除集合内的所有元素
            this.Items.Clear();

            for (int i = 0; i < dlg.Items.Count; i++)
            {
                DigitalPlatform.CommonControl.Item design_item = dlg.Items[i];

                if ((design_item.State & ItemState.ReadOnly) != 0)
                {
                    // 复原
                    OrderItem order_item = (OrderItem)design_item.Tag;
                    Debug.Assert(order_item != null, "");

                    this.Items.Add(order_item);
                    order_item.AddToListView(this.listView);

                    save_orderitems.Remove(order_item);
                    continue;
                }

                OrderItem orderitem = new OrderItem();

                // 复原某些字段
                nRet = RestoreOtherFields(design_item.OtherXml,
                    orderitem,
                    out strError);
                if (nRet == -1)
                {
                    strError = "RestoreOtherFields()发生错误: " + strError;
                    goto ERROR1;
                }

                bool bNew = false;
                // 对于全新创建的行
                if (design_item.Tag == null)
                {
                    // 促使将来以追加保存
                    orderitem.RecPath = "";
                    orderitem.ItemDisplayState = ItemDisplayState.New;
                    bNew = true;
                }
                else
                {
                    // 复原recpath
                    OrderItem order_item = (OrderItem)design_item.Tag;

                    /*
                    // 复原一些必要的值
                    orderitem.RecPath = order_item.RecPath;
                    orderitem.RefID = order_item.RefID;
                    orderitem.Timestamp = order_item.Timestamp;
                    orderitem.OldRecord = order_item.OldRecord;

                    // 2009/1/6 changed
                    orderitem.ItemDisplayState = order_item.ItemDisplayState;
                    */
                    orderitem = order_item;

                    save_orderitems.Remove(order_item);
                }

                bool bChanged = false;

                if (orderitem.Parent != Global.GetRecordID(this.BiblioRecPath))
                {
                    orderitem.Parent = Global.GetRecordID(this.BiblioRecPath);
                    bChanged = true;
                }

                if (orderitem.CatalogNo != design_item.CatalogNo)
                {
                    orderitem.CatalogNo = design_item.CatalogNo;
                    bChanged = true;
                }

                if (orderitem.Seller != design_item.Seller)
                {
                    orderitem.Seller = design_item.Seller;
                    bChanged = true;
                }

                if (orderitem.Source != design_item.Source)
                {
                    orderitem.Source = design_item.Source;  // 只取最新值
                    bChanged = true;
                }
                if (orderitem.Range != design_item.RangeString)
                {
                    orderitem.Range = design_item.RangeString;
                    bChanged = true;
                }

                if (orderitem.IssueCount != design_item.IssueCountString)
                {
                    orderitem.IssueCount = design_item.IssueCountString;
                    bChanged = true;
                }
                if (orderitem.Copy != design_item.CopyString)
                {
                    orderitem.Copy = design_item.CopyString;    // 只取最新值
                    bChanged = true;
                }

                if (orderitem.FixedPrice != design_item.FixedPrice)
                {
                    orderitem.FixedPrice = design_item.FixedPrice;   // 只取最新值
                    bChanged = true;
                }

                if (orderitem.Discount != design_item.Discount)
                {
                    orderitem.Discount = design_item.Discount;   // 只取最新值
                    bChanged = true;
                }

                if (orderitem.Price != design_item.Price)
                {
                    orderitem.Price = design_item.Price;   // 只取最新值
                    bChanged = true;
                }

                if (orderitem.Distribute != design_item.Distribute)
                {
                    orderitem.Distribute = design_item.Distribute;
                    bChanged = true;
                }
                if (orderitem.Class != design_item.Class)
                {
                    orderitem.Class = design_item.Class;
                    bChanged = true;
                }
                // 2009/2/13
                string strAddressXml = design_item.SellerAddressXml;
                if (String.IsNullOrEmpty(strAddressXml) == false)
                {
                    try
                    {
                        XmlDocument address_dom = new XmlDocument();
                        address_dom.LoadXml(strAddressXml);

                        if (orderitem.SellerAddress != address_dom.DocumentElement.InnerXml)
                        {
                            orderitem.SellerAddress = address_dom.DocumentElement.InnerXml;
                            bChanged = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "设置SellerAddress时发生错误: " + ex.Message;
                        goto ERROR1;
                    }
                }

                // 2009/11/9
                try
                {
                    if (orderitem.TotalPrice != design_item.TotalPrice)
                    {
                        orderitem.TotalPrice = design_item.TotalPrice;
                        bChanged = true;
                    }
                }
                catch (Exception ex)
                {
                    strError = "设置TotalPrice时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (bNew == false && bChanged == true)
                {
                    if (orderitem.ItemDisplayState != ItemDisplayState.New)
                    {
                        // 注: 状态为New的不能修改为Changed，这是一个例外
                        orderitem.ItemDisplayState = ItemDisplayState.Changed;
                    }
                }

                // 2017/3/2
                if (string.IsNullOrEmpty(orderitem.RefID))
                {
                    orderitem.RefID = Guid.NewGuid().ToString();
                }
                // 先加入列表
                this.Items.Add(orderitem);

                orderitem.AddToListView(this.listView);
                orderitem.HilightListViewItem(true);

                if (bChanged == true)
                    orderitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对
            }

            // 标记删除某些元素
            // 2008/12/24
            for (int i = 0; i < save_orderitems.Count; i++)
            {
                OrderItem order_item = save_orderitems[i];

                // 先加入列表
                this.Items.Add(order_item);
                order_item.AddToListView(this.listView);

                nRet = MaskDeleteItem(order_item,
                        m_bRemoveDeletedItem);
            }

#if NO
            // 改变保存按钮状态
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = true;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, true);
            return;
            ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

        void dlg_VerifyLibraryCode(object sender, VerifyLibraryCodeEventArgs e)
        {
            if (this.VerifyLibraryCode != null)
                this.VerifyLibraryCode(sender, e);
        }

        // 获得缺省记录
        void dlg_GetDefaultRecord(object sender, DigitalPlatform.CommonControl.GetDefaultRecordEventArgs e)
        {
            string strError = "";

            string strNewDefault = Program.MainForm.AppInfo.GetString(
                "entityform_optiondlg",
                "order_normalRegister_default",
                "<root />");

            // 字符串strNewDefault包含了一个XML记录，里面相当于一个记录的原貌。
            // 但是部分字段的值可能为"@"引导，表示这是一个宏命令。
            // 需要把这些宏兑现后，再正式给控件
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewDefault);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                goto ERROR1;
            }

            // 遍历所有一级元素的内容
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strText = nodes[i].InnerText;
                if (strText.Length > 0
                    && (strText[0] == '@' || strText.IndexOf("%") != -1))
                {
                    // 兑现宏
                    nodes[i].InnerText = DoGetMacroValue(strText);
                }
            }

            DomUtil.SetElementText(dom.DocumentElement,
                "parent", "");

            // 清除一些字段，不让设置缺省值
            DomUtil.SetElementText(dom.DocumentElement,
                "orderID", "");
            DomUtil.SetElementText(dom.DocumentElement,
                "state", "");

            strNewDefault = dom.OuterXml;

            e.Xml = strNewDefault;

            return;
            ERROR1:
            throw new Exception(strError);
        }

        // 根据XML记录恢复一些不重要的其他字段值
        int RestoreOtherFields(string strXml,
            OrderItem item,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

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
            item.Index = DomUtil.GetElementText(dom.DocumentElement,
                "index");
            item.State = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            item.Range = DomUtil.GetElementText(dom.DocumentElement,
                "range");
            item.IssueCount = DomUtil.GetElementText(dom.DocumentElement,
                "issueCount");
            item.OrderTime = DomUtil.GetElementText(dom.DocumentElement,
                "orderTime");
            item.OrderID = DomUtil.GetElementText(dom.DocumentElement,
                "orderID");
            item.Comment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            item.BatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            // 2014/2/24
            item.RefID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");
            return 0;
        }

        // 根据出版时间，匹配“时间范围”符合的订购记录
        // 2008/12/24
        // parameters:
        //      strPublishTime  出版时间，8字符。如果为"*"，表示统配任意出版时间均可
        internal int GetOrderInfoByPublishTime(string strPublishTime,
            string strLibraryCodeList,
            out List<string> XmlRecords,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlRecords = new List<string>();

            if (this.Items == null)
                return 0;

            int i = 0;
            foreach (OrderItem item in this.Items)
            {
                // OrderItem item = this.OrderItems[i];

                if (item.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    i++;
                    continue;
                }

                // 2017/2/28
                if (string.IsNullOrEmpty(item.RefID))
                {
                    strError = "第 " + (i + 1) + " 个订购记录缺乏 参考 ID 字段(XML 元素 refID)，获取订购记录失败";
                    return -1;
                }

                // 星号表示通配
                if (strPublishTime != "*")
                {
                    try
                    {
                        if (Global.InRange(strPublishTime, item.Range) == false)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        strError = "OrderControl GetOrderInfoByPublishTime() exception: " + ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }
                }

                // 2012/9/19
                // 观察一个馆藏分配字符串，看看是否至少部分在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   没有任何部分在管辖范围
                //      1   至少部分在管辖范围内
                nRet = Global.DistributeCross(item.Distribute,
                    strLibraryCodeList,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    continue;

                string strOrderXml = "";
                nRet = item.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strOrderXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                XmlRecords.Add(strOrderXml);
                i++;
            }

            return 1;
        }

        // 进行验收
        // TODO: 中途不让关闭 EntityForm
        void DoAccept()
        {
            string strError = "";
            int nRet = 0;

            // this.AcceptedBookItems.Clear();
            string strBiblioSourceRecord = "";
            string strBiblioSourceSyntax = "";
            if (this.PrepareAccept != null)
            {
                PrepareAcceptEventArgs e = new PrepareAcceptEventArgs();
                e.SourceRecPath = this.BiblioRecPath;
                this.PrepareAccept(this, e);
                if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = e.ErrorInfo;
                    goto ERROR1;
                }

                if (e.Cancel == true)
                    return;

                this.TargetRecPath = e.TargetRecPath;
                this.AcceptBatchNo = e.AcceptBatchNo;
                this.InputItemsBarcode = e.InputItemsBarcode;
                this.SetProcessingState = e.SetProcessingState;
                this.CreateCallNumber = e.CreateCallNumber;

                this.PriceDefault = e.PriceDefault;

                strBiblioSourceRecord = e.BiblioSourceRecord;
                strBiblioSourceSyntax = e.BiblioSourceSyntax;

                if (String.IsNullOrEmpty(e.WarningInfo) == false)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                        "警告: \r\n" + e.WarningInfo + "\r\n\r\n继续进行验收?",
                            "OrderControl",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
            }
            else
                this.TargetRecPath = "";    // 2017/7/6

            // 
            if (this.Items == null)
                this.Items = new OrderItemCollection();

            Debug.Assert(this.Items != null, "");

            OrderArriveForm dlg = new OrderArriveForm();
            // dlg.MainForm = Program.MainForm;
            dlg.BiblioDbName = Global.GetDbName(this.BiblioRecPath);    // 2009/2/15
            dlg.Text = "验收 -- 批次号:" + this.AcceptBatchNo + " -- 源:" + this.BiblioRecPath + ", 目标:" + this.TargetRecPath;
            dlg.TargetRecPath = this.TargetRecPath;
            dlg.ClearAllItems();

            // bool bCleared = false;  // 是否清除过对话框里面的参与事项?

            // 将已有的订购信息反映到对话框中。
            foreach (OrderItem item in this.Items)
            {
                // OrderItem item = this.OrderItems[i];

                if (item.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    strError = "当前存在标记删除的订购事项，必须先提交保存后，才能使用订购规划功能";
                    goto ERROR1;
                }

                string strOrderXml = "";
                nRet = item.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strOrderXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                /*
                if (bCleared == false)
                {
                    dlg.ClearAllItems();
                    bCleared = true;
                }*/

                DigitalPlatform.CommonControl.Item design_item =
                    dlg.AppendNewItem(strOrderXml, out strError);
                if (design_item == null)
                    goto ERROR1;

                design_item.Tag = (object)item; // 建立连接关系
            }

            dlg.Changed = false;

            dlg.GetValueTable -= new GetValueTableEventHandler(designOrder_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(designOrder_GetValueTable);

            Program.MainForm.AppInfo.LinkFormState(dlg,
                "order_accept_design_form_state");

            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool bOldChanged = this.Items.Changed;

            // 保存集合内的所有元素
            OrderItemCollection save_items = new OrderItemCollection();
            save_items.AddRange(this.Items);

            // 清除集合内的所有元素
            this.Items.Clear();

            List<OrderItem> changed_orderitems = new List<OrderItem>();

            for (int i = 0; i < dlg.Items.Count; i++)
            {
                DigitalPlatform.CommonControl.Item design_item = dlg.Items[i];

                if ((design_item.State & ItemState.ReadOnly) != 0)
                {
                    // 复原
                    OrderItem order_item = (OrderItem)design_item.Tag;
                    Debug.Assert(order_item != null, "");
                    this.Items.Add(order_item);
                    order_item.AddToListView(this.listView);
                    continue;
                }

                OrderItem orderitem = new OrderItem();

                // 复原某些字段
                nRet = RestoreOtherFields(design_item.OtherXml,
                    orderitem,
                    out strError);
                if (nRet == -1)
                {
                    strError = "RestoreOtherFields()发生错误: " + strError;
                    goto ERROR1;
                }

                // 对于全新创建的行
                if (design_item.Tag == null)
                {
                    // 促使将来以追加保存
                    orderitem.RecPath = "";

                    orderitem.ItemDisplayState = ItemDisplayState.New;
                }
                else
                {
                    // 复原recpath
                    OrderItem order_item = (OrderItem)design_item.Tag;

                    // 复原一些必要的值
                    orderitem.RecPath = order_item.RecPath;
                    orderitem.Timestamp = order_item.Timestamp;
                    orderitem.OldRecord = order_item.OldRecord;

                    // 2009/1/6 changed
                    orderitem.ItemDisplayState = order_item.ItemDisplayState;

                    if (orderitem.ItemDisplayState != ItemDisplayState.New)
                    {
                        // 注: 状态为New的不能修改为Changed，这是一个例外
                        orderitem.ItemDisplayState = ItemDisplayState.Changed;
                    }
                }

                orderitem.Parent = Global.GetRecordID(this.BiblioRecPath);

                orderitem.CatalogNo = design_item.CatalogNo;    // 2008/8/31
                orderitem.Seller = design_item.Seller;

                orderitem.Source = OrderDesignControl.LinkOldNewValue(design_item.OldSource, design_item.Source);

                orderitem.Range = design_item.RangeString;  // 2008/12/17
                orderitem.IssueCount = design_item.IssueCountString;    // 2008/12/17

                orderitem.Copy = OrderDesignControl.LinkOldNewValue(design_item.OldCopyString, design_item.CopyString);

                orderitem.FixedPrice = OrderDesignControl.LinkOldNewValue(design_item.OldFixedPrice, design_item.FixedPrice);
                orderitem.Discount = OrderDesignControl.LinkOldNewValue(design_item.OldDiscount, design_item.Discount);

                orderitem.Price = OrderDesignControl.LinkOldNewValue(design_item.OldPrice, design_item.Price);

                // 2018/8/2
                orderitem.TotalPrice = design_item.TotalPrice;

                orderitem.Distribute = design_item.Distribute;
                orderitem.Class = design_item.Class;    // 2008/8/31

                // 2009/2/13
                string strAddressXml = design_item.SellerAddressXml;
                if (String.IsNullOrEmpty(strAddressXml) == false)
                {
                    try
                    {
                        XmlDocument address_dom = new XmlDocument();
                        address_dom.LoadXml(strAddressXml);
                        orderitem.SellerAddress = address_dom.DocumentElement.InnerXml;
                    }
                    catch (Exception ex)
                    {
                        strError = "设置SellerAddress时发生错误: " + ex.Message;
                        goto ERROR1;
                    }
                }

                // orderitem.State 需要被修改为 “已验收”
                // 2008/10/22
                if (design_item.NewlyAcceptedCount > 0)
                    orderitem.State = "已验收";

                changed_orderitems.Add(orderitem);

                /*
                if (this.GenerateEntity != null)
                {
                    // 根据验收数据，自动创建实体数据
                    // TODO: 能否放到循环外面去，一次性做若干个orderitem?
                    nRet = GenerateEntities(ref orderitem,
                        out strError);
                    if (nRet == -1)
                    {
                        // TODO: 放弃创建实体记录，或者实体记录创建失败后，应还原订购定记录的修改前状态?
                        this.orderitems.Clear();
                        this.orderitems.AddRange(save_items);
                        // 刷新显示
                        this.orderitems.AddToListView(this.ListView);
                        goto ERROR1;
                    }
                }
                 * */

                // 2017/3/2
                if (string.IsNullOrEmpty(orderitem.RefID))
                {
                    orderitem.RefID = Guid.NewGuid().ToString();
                }
                // 先加入列表
                this.Items.Add(orderitem);

                orderitem.AddToListView(this.listView);
                orderitem.HilightListViewItem(true);

                orderitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对
            }

            // TODO: 还要注意删除listview中某些元素，学DoDesignOrder

            if (this.GenerateEntity != null)
            {
                string strTargetRecPath = "";
                // 根据验收数据，自动创建实体数据
                nRet = GenerateEntities(
                    strBiblioSourceRecord,
                    strBiblioSourceSyntax,
                    changed_orderitems,
                    out strTargetRecPath,
                    out strError);
                if (nRet == -1)
                {
                    // TODO: 放弃创建实体记录，或者实体记录创建失败后，应还原订购定记录的修改前状态?
                    this.Items.Clear();
                    this.Items.AddRange(save_items);
                    // 刷新显示
                    this.Items.AddToListView(this.listView);
                    goto ERROR1;
                }

                // 2012/7/24
                this.TargetRecPath = strTargetRecPath;

                // 源记录不属于采购工库时，源记录需要写入998$t
                if (String.IsNullOrEmpty(strTargetRecPath) == false)
                {
                    string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);
                    if (Program.MainForm.IsOrderWorkDb(strBiblioDbName) == false)
                    {
                        if (this.SetTargetRecPath != null)
                        {
                            SetTargetRecPathEventArgs e = new SetTargetRecPathEventArgs();
                            e.TargetRecPath = strTargetRecPath;
                            this.SetTargetRecPath(this, e);
                            if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                                goto ERROR1;
                        }
                    }
                }

                if (nRet == 0)
                    MessageBox.Show(this, "警告：本次验收没有创建任何新的册(实体)");
            }

#if NO
            // 改变保存按钮状态
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = true;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, true);
            return;

            ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

        static string BuildOtherPrices(string strOrderPrice,
            string strAcceptPrice,
            string strBiblioPrice,
            int nRightCopy)
        {
            string strResult = "";

            if (String.IsNullOrEmpty(strOrderPrice) == false)
            {
                strResult += "订购价:" + strOrderPrice;
                if (nRightCopy > 1)
                    strResult += "/" + nRightCopy.ToString();
            }

            if (String.IsNullOrEmpty(strAcceptPrice) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ";";
                strResult += "验收价:" + strAcceptPrice;
                if (nRightCopy > 1)
                    strResult += "/" + nRightCopy.ToString();
            }

            if (String.IsNullOrEmpty(strBiblioPrice) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ";";
                strResult += "书目价:" + strBiblioPrice;
                if (nRightCopy > 1)
                    strResult += "/" + nRightCopy.ToString();
            }

            return strResult;
        }

        // 根据验收数据，自动创建实体数据
        // parameters:
        //      strTargetRecPath    创建了实体记录的目标书目记录。可能是新创建的目标记录
        // return:
        //      -1  error
        //      0   没有创建任何新的实体
        //      1   成功创建了实体
        int GenerateEntities(
            string strNewBiblioRecord,
            string strNewBiblioRecordSyntax,
            List<OrderItem> orderitems,
            out string strTargetRecPath,
            out string strError)
        {
            strError = "";
            strTargetRecPath = "";

            if (this.GenerateEntity == null)
            {
                strError = "GenerateEntity事件尚未挂接";
                return -1;
            }

            GenerateEntityEventArgs data_container = new GenerateEntityEventArgs();
            data_container.InputItemBarcode = this.InputItemsBarcode;
            data_container.SetProcessingState = this.SetProcessingState;
            data_container.CreateCallNumber = this.CreateCallNumber;

            data_container.BiblioRecord = strNewBiblioRecord;
            data_container.BiblioSyntax = strNewBiblioRecordSyntax;

            string strBiblioPrice = DoGetMacroValue("@price");

            for (int j = 0; j < orderitems.Count; j++)
            {
                OrderItem order_item = orderitems[j];

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(order_item.Distribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                bool bChanged = false;

                // 2010/12/1 add
                string strOldCopyValue = "";
                string strNewCopyValue = "";
                OrderDesignControl.ParseOldNewValue(order_item.Copy,
                    out strOldCopyValue,
                    out strNewCopyValue);
                string strCopyString = strNewCopyValue;
                if (String.IsNullOrEmpty(strCopyString) == true)
                    strCopyString = strOldCopyValue;

                // 2010/12/1 add
                int nRightCopy = 1;  // 套内册数
                string strRightCopy = OrderDesignControl.GetRightFromCopyString(strCopyString);
                if (String.IsNullOrEmpty(strRightCopy) == false)
                {
                    try
                    {
                        nRightCopy = Convert.ToInt32(strRightCopy);
                    }
                    catch
                    {
                        strError = "套内册数字符串 '" + strRightCopy + "' 格式错误";
                        return -1;
                    }
                }

                // 为每个馆藏地点创建一个实体记录
                for (int i = 0; i < locations.Count; i++)
                {
                    Location location = locations[i];

                    // TODO: 要注意两点：1) 已经验收过的行，里面出现*的refid，是否要再次创建册？这样效果结识，反复用的时候有好处
                    // 2) 没有验收足的时候，是不是要按照验收足来循环了？检查一下

                    // 已经创建过的事项，跳过
                    if (location.RefID != "*")
                        continue;

                    location.RefID = "";


                    // 2010/12/1 add
                    for (int k = 0; k < nRightCopy; k++)
                    {
                        GenerateEntityData e = new GenerateEntityData();

                        if (nRightCopy > 1)
                            e.Sequence = (k + 1).ToString() + "/" + nRightCopy.ToString();

                        e.Action = "new";
                        e.RefID = Guid.NewGuid().ToString();

                        if (String.IsNullOrEmpty(location.RefID) == false)
                            location.RefID += "|";  // 表示套内区分

                        location.RefID += e.RefID;   // 修改到馆藏地点字符串中

                        bChanged = true;

                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml("<root />");

                        // 2009/10/19
                        // 状态
                        if (this.SetProcessingState == true)
                        {
                            // 增补“加工中”值
                            string strOldState = DomUtil.GetElementText(dom.DocumentElement,
                                "state");
                            DomUtil.SetElementText(dom.DocumentElement,
                                "state", Global.AddStateProcessing(strOldState));

                        }

                        // seller内是单纯值
                        DomUtil.SetElementText(dom.DocumentElement,
                            "seller", order_item.Seller);

                        {
                            string strOldValue = "";
                            string strNewValue = "";


                            // source内采用新值
                            // 分离 "old[new]" 内的两个值
                            OrderDesignControl.ParseOldNewValue(order_item.Source,
                                out strOldValue,
                                out strNewValue);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "source", strNewValue);
                        }

                        // 分离两个价格
                        OrderDesignControl.ParseOldNewValue(order_item.Price,
                            out string strOrderPrice,
                            out string strArrivePrice);
                        string strPriceValue = "";
                        if (this.PriceDefault == "订购价")
                            strPriceValue = strOrderPrice;
                        else if (this.PriceDefault == "验收价")
                            strPriceValue = strArrivePrice;
                        else if (this.PriceDefault == "书目价")
                            strPriceValue = strBiblioPrice;


                        if (nRightCopy == 1)
                        {
                            DomUtil.SetElementText(dom.DocumentElement,
                                "price", strPriceValue);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(strArrivePrice) == false)
                            {
                                DomUtil.SetElementText(dom.DocumentElement,
                                    "price", strPriceValue + "/" + nRightCopy.ToString());
                            }
                        }

                        e.OtherPrices = BuildOtherPrices(
    strOrderPrice,
    strArrivePrice,
    strBiblioPrice,
    nRightCopy);

                        // location
                        DomUtil.SetElementText(dom.DocumentElement,
                            "location", location.Name);

                        // 批次号
                        DomUtil.SetElementText(dom.DocumentElement,
                            "batchNo", this.AcceptBatchNo);

                        e.Xml = dom.OuterXml;

                        data_container.DataList.Add(e);
                    } // end of j loop
                }

                // 馆藏地点字符串有变化，需要反映给调主
                if (bChanged == true)
                {
                    order_item.Distribute = locations.ToString();
                    order_item.RefreshListView();
                }
            }

            if (data_container.DataList != null
                && (data_container.DataList.Count > 0 || String.IsNullOrEmpty(data_container.BiblioRecord) == false)
                )
            {
                // 调用外部挂接的事件
                this.GenerateEntity(this, data_container);
                string strErrorText = "";

                if (String.IsNullOrEmpty(data_container.ErrorInfo) == false)
                {
                    strError = data_container.ErrorInfo;
                    return -1;
                }

                // 2009/11/8
                strTargetRecPath = data_container.TargetRecPath;

                for (int i = 0; i < data_container.DataList.Count; i++)
                {
                    GenerateEntityData data = data_container.DataList[i];
                    if (String.IsNullOrEmpty(data.ErrorInfo) == false)
                    {
                        strErrorText += data.ErrorInfo;
                    }
                }

                if (String.IsNullOrEmpty(strErrorText) == false)
                {
                    strError = strErrorText;
                    return -1;
                }

                return 1;
            }

            return 0;
        }

#if NO
        // 外部调用接口
        // 追加一个新的订购记录
        public int AppendOrder(OrderItem orderitem,
            out string strError)
        {
            strError = "";

            orderitem.Parent = Global.GetID(this.BiblioRecPath);

            this.Items.Add(orderitem);

            orderitem.ItemDisplayState = ItemDisplayState.New;
            orderitem.AddToListView(this.ListView);
            orderitem.HilightListViewItem(true);

            orderitem.Changed = true;
            return 0;
        }
#endif

        // 外部调用接口
        // 追加一个新的订购记录
        /// <summary>
        /// 追加一个新的订购记录
        /// </summary>
        /// <param name="orderitem">要追加的事项。OrderItem 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int AppendOrder(OrderItem orderitem,
            out string strError)
        {
            return this.AppendItem(orderitem, out strError);
        }

        // 新增一个订购事项，要打开对话框让输入详细信息
        void DoNewOrder(/*string strIndex*/)
        {
            string strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未载入书目记录";
                goto ERROR1;
            }

            // 
            if (this.Items == null)
                this.Items = new OrderItemCollection();

            Debug.Assert(this.Items != null, "");

            bool bOldChanged = this.Items.Changed;

#if NO
            if (String.IsNullOrEmpty(strIndex) == false)
            {

                // 对当前窗口内进行编号查重
                OrderItem dupitem = this.OrderItems.GetItemByIndex(
                    strIndex,
                    null);
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的编号 '" + strIndex + "' 和本种中未提交之一删除编号相重。请先行提交已有之修改，再进行新订购操作。";
                    else
                        strText = "拟新增的编号 '" + strIndex + "' 在本种中已经存在。";

                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
        strText + "\r\n\r\n要立即对已存在编号进行修改吗？",
        "OrderControl",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);

                    // 转为修改
                    if (result == DialogResult.Yes)
                    {
                        ModifyOrder(dupitem);
                        return;
                    }

                    // 突出显示，以便操作人员观察这条已经存在的记录
                    dupitem.HilightListViewItem(true);
                    return;
                }

                // 对(本种)所有订购记录进行编号查重
                if (true)
                {
                    string strOrderText = "";
                    string strBiblioText = "";
                    nRet = SearchOrderIndex(strIndex,
                        this.BiblioRecPath,
                        out strOrderText,
                        out strBiblioText,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(ForegroundWindow.Instance, "对编号 '" + strIndex + "' 进行查重的过程中发生错误: " + strError);
                    else if (nRet == 1) // 发生重复
                    {
                        OrderIndexFoundDupDlg dlg = new OrderIndexFoundDupDlg();
                        MainForm.SetControlFont(dlg, this.Font, false);
                        dlg.MainForm = Program.MainForm;
                        dlg.BiblioText = strBiblioText;
                        dlg.OrderText = strOrderText;
                        dlg.MessageText = "拟新增的编号 '" + strIndex + "' 在数据库中发现已经存在。因此无法新增。";
                        dlg.ShowDialog(this);
                        return;
                    }
                }

            } // end of ' if (String.IsNullOrEmpty(strIndex) == false)
#endif

            OrderItem orderitem = new OrderItem();

            // 设置缺省值
            nRet = SetItemDefaultValues(
                "order_normalRegister_default",
                true,
                orderitem,
                out strError);
            if (nRet == -1)
            {
                strError = "设置缺省值的时候发生错误: " + strError;
                goto ERROR1;
            }

#if NO
            orderitem.Index = strIndex;
#endif
            orderitem.Parent = Global.GetRecordID(this.BiblioRecPath);

            // 先加入列表
            this.Items.Add(orderitem);
            orderitem.ItemDisplayState = ItemDisplayState.New;
            orderitem.AddToListView(this.listView);
            orderitem.HilightListViewItem(true);

            orderitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对

            OrderEditForm edit = new OrderEditForm();

            edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15
            edit.Text = "新增订购事项";
            // edit.MainForm = Program.MainForm;
            edit.ItemControl = this;    // 2016/1/8
            nRet = edit.InitialForEdit(orderitem,
                this.Items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            //REDO:
            Program.MainForm.AppInfo.LinkFormState(edit, "OrderEditForm_state");
            edit.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(edit);

            if (edit.DialogResult != DialogResult.OK
                && edit.Item == orderitem    // 表明尚未前后移动，或者移动回到起点，然后Cancel
                )
            {
                this.Items.PhysicalDeleteItem(orderitem);
                TriggerContentChanged(bOldChanged, this.Items.Changed);
                return;
            }

            TriggerContentChanged(bOldChanged, true);

            // 要对本种和所有相关订购库进行编号查重。
            // 如果重了，要保持窗口，以便修改。不过从这个角度，查重最好在对话框关闭前作？
            // 或者重新打开对话框
            string strRefID = orderitem.RefID;
            if (String.IsNullOrEmpty(strRefID) == false)
            {
                // 需要排除掉刚加入的自己: orderitem。
                List<BookItemBase> excludeItems = new List<BookItemBase>();
                excludeItems.Add(orderitem);

                // 对当前窗口内进行参考ID查重
                OrderItem dupitem = this.Items.GetItemByRefID(
                    strRefID,
                    excludeItems) as OrderItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的参考ID '" + strRefID + "' 和本种中未提交之一删除参考ID相重。请先行提交已有之修改，再进行新增订购操作。";
                    else
                        strText = "拟新增的参考ID '" + strRefID + "' 在本种中已经存在。";

                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
        strText + "\r\n\r\n要立即对新记录的参考ID进行修改吗？\r\n(Yes 进行修改; No 不修改，让发生重复的新记录进入列表; Cancel 放弃刚刚创建的新记录)",
        "OrderControl",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);

                    // 转为修改
                    if (result == DialogResult.Yes)
                    {
                        ModifyOrder(orderitem);
                        return;
                    }

                    // 放弃刚刚创建的记录
                    if (result == DialogResult.Cancel)
                    {
                        this.Items.PhysicalDeleteItem(orderitem);

#if NO
                        // 改变保存按钮状态
                        // SetSaveAllButtonState(true);
                        if (this.ContentChanged != null)
                        {
                            ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                            e1.OldChanged = bOldChanged;
                            e1.CurrentChanged = this.Items.Changed;
                            this.ContentChanged(this, e1);
                        }
#endif
                        TriggerContentChanged(bOldChanged, this.Items.Changed);

                        return;
                    }

                    // 突出显示，以便操作人员观察这条已经存在的记录
                    dupitem.HilightListViewItem(true);
                    return;
                }
            }
            else
            {
                orderitem.RefID = Guid.NewGuid().ToString();    // 2017/3/2
            }

            return;
            ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

#if NO
        // 检索订购编号。用于新编号查重。
        // 注：仅用strIndex无法获得订购记录，必须加上书目记录路径才行
        int SearchOrderRefID(string strRefID,
            string strBiblioRecPath,
            out string strOrderText,
            out string strBiblioText,
            out string strError)
        {
            strError = "";
            strOrderText = "";
            strBiblioText = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                byte[] order_timestamp = null;
                string strOrderRecPath = "";
                string strOutputBiblioRecPath = "";

                long lRet = Channel.GetOrderInfo(
                    stop,
                    strRefID,
                    strBiblioRecPath,
                    "html",
                    out strOrderText,
                    out strOrderRecPath,
                    out order_timestamp,
                    "html",
                    out strBiblioText,
                    out strOutputBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;   // found
        }
#endif

#if !NEW_DUP_API
        // 编号查重。用于(可能是)旧编号查重。
        // 本函数可以自动排除和当前路径strOriginRecPath重复之情形
        // parameters:
        //      strOriginRecPath    出发记录的路径。
        //      paths   所有命中的路径
        // return:
        //      -1  error
        //      0   not dup
        //      1   dup
        int SearchOrderIndexDup(string strIndex,
            string strBiblioRecPath,
            string strOriginRecPath,
            out string[] paths,
            out string strError)
        {
            strError = "";
            paths = null;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对编号 '" + strIndex + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SearchOrderDup(
                    stop,
                    strIndex,
                    strBiblioRecPath,
                    100,
                    out paths,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found

                if (lRet == 1)
                {
                    // 检索命中一条。看看路径是否和出发记录一样
                    if (paths.Length != 1)
                    {
                        strError = "系统错误: SearchOrderDup() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
                        return -1;
                    }

                    if (paths[0] != strOriginRecPath)
                        return 1;   // 发现重复的了

                    return 0;   // 不重复
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;   // found
        }
#endif

#if NEW_DUP_API

#if NO
        // 参考ID查重。用于(可能是)旧参考ID查重。
        // 本函数可以自动排除和当前路径strOriginRecPath重复之情形
        // parameters:
        //      strOriginRecPath    出发记录的路径。
        //      paths   所有命中的路径
        // return:
        //      -1  error
        //      0   not dup
        //      1   dup
        int SearchOrderRefIdDup(string strRefID,
            string strBiblioRecPath,
            string strOriginRecPath,
            out string[] paths,
            out string strError)
        {
            strError = "";
            paths = null;

#if NO
            if (string.IsNullOrEmpty(strRefID) == true)
                return 0;   // 对于空的参考ID不必查重
#endif
            if (string.IsNullOrEmpty(strRefID) == true)
            {
                strError = "不应用参考ID为空来查重";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SearchOrderDup(
                    stop,
                    strRefID,
                    strBiblioRecPath,
                    100,
                    out paths,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found

                if (lRet == 1)
                {
                    // 检索命中一条。看看路径是否和出发记录一样
                    if (paths.Length != 1)
                    {
                        strError = "系统错误: SearchOrderDup() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
                        return -1;
                    }

                    if (paths[0] != strOriginRecPath)
                        return 1;   // 发现重复的了

                    return 0;   // 不重复
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;   // found
        }
#endif
        int SearchOrderRefIdDup(
            LibraryChannel channel,
            string strRefID,
    string strOriginRecPath,
    out string[] paths,
    out string strError)
        {
            strError = "";
            paths = null;

            if (string.IsNullOrEmpty(strRefID) == true)
            {
                strError = "不应用参考ID为空来查重";
                return -1;
            }

#if NO
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            Stop.BeginLoop();
#endif
            Stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");

            try
            {
                long lRet = channel.SearchOrder(
    Stop,
    "<全部>",
    strRefID,
    100,
    "参考ID",
    "exact",
    "zh",
    "dup",
    "", // strSearchStyle
    "", // strOutputStyle
    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found

                long lHitCount = lRet;

                lRet = channel.GetSearchResult(Stop,
                    "dup",
                    0,
                    Math.Min(lHitCount, 100),
                    "zh",
                    out List<string> aPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                paths = new string[aPath.Count];
                aPath.CopyTo(paths);

                if (lHitCount == 1)
                {
                    // 检索命中一条。看看路径是否和出发记录一样
                    if (paths.Length != 1)
                    {
                        strError = "系统错误: SearchOrder() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
                        return -1;
                    }

                    if (paths[0] != strOriginRecPath)
                        return 1;   // 发现重复的了

                    return 0;   // 不重复
                }
            }
            finally
            {
#if NO
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
#endif
                Stop.Initial("");
            }

            return 1;   // found
        }


#endif

#if NO
        string DoGetMacroValue(string strMacroName)
        {
            if (this.GetMacroValue != null)
            {
                GetMacroValueEventArgs e = new GetMacroValueEventArgs();
                e.MacroName = strMacroName;
                this.GetMacroValue(this, e);

                return e.MacroValue;
            }

            return null;
        }
#endif

#if NO
        // 为OrderItem对象设置缺省值
        // parameters:
        //      strCfgEntry 为"order_normalRegister_default"或"order_quickRegister_default"
        public int SetOrderItemDefaultValues(
            string strCfgEntry,
            OrderItem orderitem,
            out string strError)
        {
            strError = "";

            string strNewDefault = Program.MainForm.AppInfo.GetString(
    "entityform_optiondlg",
    strCfgEntry,
    "<root />");

            // 字符串strNewDefault包含了一个XML记录，里面相当于一个记录的原貌。
            // 但是部分字段的值可能为"@"引导，表示这是一个宏命令。
            // 需要把这些宏兑现后，再正式给控件
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewDefault);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            // 遍历所有一级元素的内容
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strText = nodes[i].InnerText;
                if (strText.Length > 0 && strText[0] == '@')
                {
                    // 兑现宏
                    nodes[i].InnerText = DoGetMacroValue(strText);
                }
            }

            strNewDefault = dom.OuterXml;

            int nRet = orderitem.SetData("",
                strNewDefault,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            orderitem.Parent = "";
            orderitem.RecPath = "";

            return 0;
        }
#endif

        void ModifyOrder(OrderItem orderitem)
        {
            int nRet = 0;
            string strError = "";
            Debug.Assert(orderitem != null, "");

            bool bOldChanged = this.Items.Changed;

            string strOldIndex = orderitem.Index;

            using (OrderEditForm edit = new OrderEditForm())
            {
                this.ParentShowMessage("正在准备数据 ...", "green", false);
                try
                {
                    edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15
                                                                                // edit.MainForm = Program.MainForm;
                    edit.ItemControl = this;
                    nRet = edit.InitialForEdit(orderitem,
                        this.Items,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(ForegroundWindow.Instance, strError);
                        return;
                    }
                    edit.StartItem = null;  // 清除原始对象标记
                }
                finally
                {
                    this.ParentShowMessage("", "", false);
                }

                REDO:
                Program.MainForm.AppInfo.LinkFormState(edit, "OrderEditForm_state");
                edit.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(edit);

                if (edit.DialogResult != DialogResult.OK)
                    return;

                TriggerContentChanged(bOldChanged, true);

                LibraryChannel channel = Program.MainForm.GetChannel();
                this.EnableControls(false);
                try
                {
                    if (strOldIndex != orderitem.Index) // 编号改变了的情况下才查重
                    {
                        // 需要排除掉自己: orderitem。
                        List<OrderItem> excludeItems = new List<OrderItem>();
                        excludeItems.Add(orderitem);


                        // 对当前窗口内进行编号查重
                        OrderItem dupitem = this.Items.GetItemByIndex(
                            orderitem.Index,
                            excludeItems);
                        if (dupitem != null)
                        {
                            string strText = "";
                            if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                                strText = "编号 '" + orderitem.Index + "' 和本种中未提交之一删除编号相重。按“确定”按钮重新输入，或退出对话框后先行提交已有之修改。";
                            else
                                strText = "编号 '" + orderitem.Index + "' 在本种中已经存在。按“确定”按钮重新输入。";

                            MessageBox.Show(ForegroundWindow.Instance, strText);
                            goto REDO;
                        }

                        // 对(本种)所有订购记录进行编号查重
                        if (edit.AutoSearchDup == true
#if NEW_DUP_API
 && string.IsNullOrEmpty(orderitem.RefID) == false
#endif
)
                        {
                            // Debug.Assert(false, "");

                            string[] paths = null;

#if !NEW_DUP_API
                        // 编号查重。
                        // parameters:
                        //      strOriginRecPath    出发记录的路径。
                        //      paths   所有命中的路径
                        // return:
                        //      -1  error
                        //      0   not dup
                        //      1   dup
                        nRet = SearchOrderIndexDup(orderitem.Index,
                            this.BiblioRecPath,
                            orderitem.RecPath,
                            out paths,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, "对编号 '" + orderitem.Index + "' 进行查重的过程中发生错误: " + strError);
                        else if (nRet == 1) // 发生重复
                        {
                            string pathlist = String.Join(",", paths);

                            string strText = "编号 '" + orderitem.Index + "' 在数据库中发现已经被(属于其他种的)下列订购记录所使用。\r\n" + pathlist + "\r\n\r\n按“确定”按钮重新编辑订购信息，或者根据提示的订购记录路径，去修改其他订购记录信息。";
                            MessageBox.Show(ForegroundWindow.Instance, strText);

                            goto REDO;
                        }
#else
                            // 参考ID查重。
                            // parameters:
                            //      strOriginRecPath    出发记录的路径。
                            //      paths   所有命中的路径
                            // return:
                            //      -1  error
                            //      0   not dup
                            //      1   dup
                            nRet = SearchOrderRefIdDup(
                                channel,
                                orderitem.RefID,
                                // this.BiblioRecPath,
                                orderitem.RecPath,
                                out paths,
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(ForegroundWindow.Instance, "对参考ID '" + orderitem.RefID + "' 进行查重的过程中发生错误: " + strError);
                            else if (nRet == 1) // 发生重复
                            {
                                string pathlist = String.Join(",", paths);

                                string strText = "参考ID '" + orderitem.RefID + "' 在数据库中发现已经被(属于其他种的)下列订购记录所使用。\r\n" + pathlist + "\r\n\r\n按“确定”按钮重新编辑订购信息，或者根据提示的订购记录路径，去修改其他订购记录信息。";
                                MessageBox.Show(ForegroundWindow.Instance, strText);

                                goto REDO;
                            }
#endif
                        }
                    }

                    // 2017/3/2
                    if (string.IsNullOrEmpty(orderitem.RefID))
                    {
                        orderitem.RefID = Guid.NewGuid().ToString();
                    }
                }
                finally
                {
                    this.EnableControls(true);
                    Program.MainForm.ReturnChannel(channel);
                }
            }
        }

#if NO
        // 分批进行保存
        // return:
        //      -2  已经警告(部分成功，部分失败)
        //      -1  出错
        //      0   保存成功，没有错误和警告
        int SaveOrders(EntityInfo[] orders,
            out string strError)
        {
            strError = "";

            bool bWarning = false;
            EntityInfo[] errorinfos = null;

            int nBatch = 100;
            for (int i = 0; i < (orders.Length / nBatch) + ((orders.Length % nBatch) != 0 ? 1 : 0); i++)
            {
                int nCurrentCount = Math.Min(nBatch, orders.Length - i * nBatch);
                EntityInfo[] current = EntityControl.GetPart(orders, i * nBatch, nCurrentCount);

                int nRet = SaveOrderRecords(this.BiblioRecPath,
                    current,
                    out errorinfos,
                    out strError);

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                if (RefreshOperResult(errorinfos) == true)
                    bWarning = true;

                if (nRet == -1)
                    return -1;
            }

            if (bWarning == true)
                return -2;
            return 0;
        }

        // 提交订购保存请求
        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        public int DoSaveOrders()
        {
            // 2008/9/17
            if (this.Items == null)
                return 0;

            EnableControls(false);

            try
            {
                string strError = "";
                int nRet = 0;

                if (this.Items == null)
                {
                    /*
                    strError = "没有订购信息需要保存";
                    goto ERROR1;
                     * */
                    return 0;
                }

                // 检查全部事项的Parent值是否适合保存
                // return:
                //      -1  有错误，不适合保存
                //      0   没有错误
                nRet = this.Items.CheckParentIDForSave(out strError);
                if (nRet == -1)
                {
                    strError = "保存订购信息失败，原因：" + strError;
                    goto ERROR1;
                }

                EntityInfo[] orders = null;

                // 构造需要提交的订购信息数组
                nRet = BuildSaveOrders(
                    out orders,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (orders == null || orders.Length == 0)
                    return 0; // 没有必要保存

#if NO
                EntityInfo[] errorinfos = null;
                nRet = SaveOrderRecords(this.BiblioRecPath,
                    orders,
                    out errorinfos,
                    out strError);

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                RefreshOperResult(errorinfos);

                if (nRet == -1)
                {
                    goto ERROR1;
                }
#endif
                // return:
                //      -2  已经警告(部分成功，部分失败)
                //      -1  出错
                //      0   保存成功，没有错误和警告
                nRet = SaveOrders(orders, out strError);
                if (nRet == -2)
                    return -1;  // SaveOrders()已经MessageBox()显示过了
                if (nRet == -1)
                    goto ERROR1;

                this.Changed = false;
                Program.MainForm.StatusBarMessage = "订购信息 提交 / 保存 成功";
                return 1;
            ERROR1:
                MessageBox.Show(ForegroundWindow.Instance, strError);
                return -1;
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 根据订购记录路径加亮事项
        public OrderItem HilightLineByItemRecPath(string strItemRecPath,
                bool bClearOtherSelection)
        {
            OrderItem orderitem = null;

            if (bClearOtherSelection == true)
            {
                this.ListView.SelectedItems.Clear();
            }

            if (this.Items != null)
            {
                orderitem = this.Items.GetItemByRecPath(strItemRecPath) as OrderItem;
                if (orderitem != null)
                    orderitem.HilightListViewItem(true);
            }

            return orderitem;
        }

        // 构造用于保存的订购信息数组
        int BuildSaveOrders(
            out EntityInfo[] orders,
            out string strError)
        {
            strError = "";
            orders = null;
            int nRet = 0;

            // TODO: 对集合内的全部事项的refid进行查重

            Debug.Assert(this.Items != null, "");

            List<EntityInfo> orderArray = new List<EntityInfo>();

            foreach (OrderItem orderitem in this.Items)
            {
                // OrderItem orderitem = this.OrderItems[i];

                if (orderitem.ItemDisplayState == ItemDisplayState.Normal)
                    continue;

                EntityInfo info = new EntityInfo();

                // 2010/3/15 add
                if (String.IsNullOrEmpty(orderitem.RefID) == true)
                {
                    orderitem.RefID = Guid.NewGuid().ToString();
                    orderitem.RefreshListView();
                }

                info.RefID = orderitem.RefID;  // 2008/2/17

                string strXml = "";
                nRet = orderitem.BuildRecord(out strXml,
                        out strError);
                if (nRet == -1)
                    return -1;

                if (orderitem.ItemDisplayState == ItemDisplayState.New)
                {
                    info.Action = "new";
                    info.NewRecPath = "";
                    info.NewRecord = strXml;
                    info.NewTimestamp = null;
                }

                if (orderitem.ItemDisplayState == ItemDisplayState.Changed)
                {
                    info.Action = "change";
                    info.OldRecPath = orderitem.RecPath;
                    info.NewRecPath = orderitem.RecPath;

                    info.NewRecord = strXml;
                    info.NewTimestamp = null;

                    info.OldRecord = orderitem.OldRecord;
                    info.OldTimestamp = orderitem.Timestamp;
                }

                if (orderitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    info.Action = "delete";
                    info.OldRecPath = orderitem.RecPath; // NewRecPath

                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    info.OldRecord = orderitem.OldRecord;
                    info.OldTimestamp = orderitem.Timestamp;
                }

                orderArray.Add(info);
            }

            // 复制到目标
            orders = new EntityInfo[orderArray.Count];
            for (int i = 0; i < orderArray.Count; i++)
            {
                orders[i] = orderArray[i];
            }

            return 0;
        }

        // 构造用于修改归属的信息数组
        // 如果strNewBiblioPath中的书目库名发生变化，那订购记录都要在订购库之间移动，因为订购库和书目库有一定的捆绑关系。
        int BuildChangeParentRequestOrders(
            List<OrderItem> orderitems,
            string strNewBiblioRecPath,
            out EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            entities = null;
            int nRet = 0;

            string strSourceBiblioDbName = Global.GetDbName(this.BiblioRecPath);
            string strTargetBiblioDbName = Global.GetDbName(strNewBiblioRecPath);

            // 检查一下目标书目库名是不是合法的书目库名
            if (MainForm.IsValidBiblioDbName(strTargetBiblioDbName) == false)
            {
                strError = "目标库名 '" + strTargetBiblioDbName + "' 不在系统定义的书目库名之列";
                return -1;
            }

            // 获得目标书目记录id
            string strTargetBiblioRecID = Global.GetRecordID(strNewBiblioRecPath);   // !!!
            if (String.IsNullOrEmpty(strTargetBiblioRecID) == true)
            {
                strError = "因目标书目记录路径 '" + strNewBiblioRecPath + "' 中没有包含ID部分，无法进行操作";
                return -1;
            }
            if (strTargetBiblioRecID == "?")
            {
                strError = "目标书目记录路径 '" + strNewBiblioRecPath + "' 中记录ID不应为问号";
                return -1;
            }
            if (Global.IsPureNumber(strTargetBiblioRecID) == false)
            {
                strError = "目标书目记录路径 '" + strNewBiblioRecPath + "' 中记录ID应为纯数字";
                return -1;
            }

            bool bMove = false; // 是否需要移动订购记录
            string strTargetOrderDbName = "";  // 目标订购库名

            if (strSourceBiblioDbName != strTargetBiblioDbName)
            {
                // 书目库发生了改变，才有必要移动。否则仅仅修改订购记录的<parent>即可
                bMove = true;
                strTargetOrderDbName = MainForm.GetOrderDbName(strTargetBiblioDbName);

                if (String.IsNullOrEmpty(strTargetOrderDbName) == true)
                {
                    strError = "书目库 '" + strTargetBiblioDbName + "' 并没有从属的订购库定义。操作失败";
                    return -1;
                }
            }

            Debug.Assert(orderitems != null, "");

            List<EntityInfo> entityArray = new List<EntityInfo>();

            for (int i = 0; i < orderitems.Count; i++)
            {
                OrderItem orderitem = orderitems[i];

                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(orderitem.RefID) == true)
                {
                    Debug.Assert(false,"orderitem.RefID应当为只读，并且不可能为空");
                    /*
                    orderitem.RefID = Guid.NewGuid().ToString();
                    orderitem.RefreshListView();
                     * */
                }

                info.RefID = orderitem.RefID;
                orderitem.Parent = strTargetBiblioRecID;

                string strXml = "";
                nRet = orderitem.BuildRecord(out strXml,
                        out strError);
                if (nRet == -1)
                    return -1;

                info.OldRecPath = orderitem.RecPath;
                if (bMove == false)
                {
                    info.Action = "change";
                    info.NewRecPath = orderitem.RecPath;
                }
                else
                {
                    info.Action = "move";
                    Debug.Assert(String.IsNullOrEmpty(strTargetOrderDbName) == false, "");
                    info.NewRecPath = strTargetOrderDbName + "/?";  // 把订购记录移动到另一个订购库中，追加成一条新记录，而旧记录自动被删除
                }

                info.NewRecord = strXml;
                info.NewTimestamp = null;

                info.OldRecord = orderitem.OldRecord;
                info.OldTimestamp = orderitem.Timestamp;

                entityArray.Add(info);
            }

            // 复制到目标
            entities = new EntityInfo[entityArray.Count];
            for (int i = 0; i < entityArray.Count; i++)
            {
                entities[i] = entityArray[i];
            }

            return 0;
        }

        // 保存订购记录
        // 不负责刷新界面和报错
        int SaveOrderRecords(string strBiblioRecPath,
            EntityInfo[] orders,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在保存订购信息 ...");
            Stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetOrders(
                    Stop,
                    strBiblioRecPath,
                    orders,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 把报错信息中的成功事项的状态修改兑现
        // 并且彻底去除没有报错的“删除”OrderItem事项（内存和视觉上）
        // return:
        //      false   没有警告
        //      true    出现警告
        bool RefreshOperResult(EntityInfo[] errorinfos)
        {
            int nRet = 0;

            string strWarning = ""; // 警告信息

            if (errorinfos == null)
                return false;

            bool bOldChanged = this.Items.Changed;

            for (int i = 0; i < errorinfos.Length; i++)
            {
                /*
                XmlDocument dom = new XmlDocument();

                string strNewXml = errorinfos[i].NewRecord;
                string strOldXml = errorinfos[i].OldRecord;

                if (String.IsNullOrEmpty(strNewXml) == false)
                {
                    dom.LoadXml(strNewXml);
                }
                else if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    dom.LoadXml(strOldXml);
                }
                else
                {
                    // 找不到编号来定位
                    Debug.Assert(false, "找不到定位的编号");
                    // 是否单独显示出来?
                    continue;
                }
                 * */

                OrderItem orderitem = null;

                string strError = "";

                if (String.IsNullOrEmpty(errorinfos[i].RefID) == true)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "服务器返回的EntityInfo结构中RefID为空");
                    return true;
                }

                /*
                string strIndex = "";
                // 在listview中定位和dom关联的事项
                // 顺次根据 记录路径 -- 编号 来定位
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = LocateOrderItem(
                    errorinfos[i].OldRecPath,   // 原来是NewRecPath
                    dom,
                    out orderitem,
                    out strIndex,
                    out strError);
                 * */
                nRet = LocateOrderItem(
                    errorinfos[i].RefID,
                    GetOneRecPath(errorinfos[i].NewRecPath, errorinfos[i].OldRecPath),
                    out orderitem,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "定位错误信息 '" + errorinfos[i].ErrorInfo + "' 所在行的过程中发生错误:" + strError);
                    continue;
                }

                if (nRet == 0)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "无法定位索引值为 " + i.ToString() + " 的错误信息 '" + errorinfos[i].ErrorInfo + "'");
                    continue;
                }

                string strLocationSummary = GetLocationSummary(
                    orderitem.Index,    // strIndex,
                    errorinfos[i].NewRecPath,
                    errorinfos[i].RefID);

                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                {
                    if (errorinfos[i].Action == "new")
                    {
                        orderitem.OldRecord = errorinfos[i].NewRecord;
                        nRet = orderitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, strError);
                    }
                    else if (errorinfos[i].Action == "change"
                        || errorinfos[i].Action == "move")
                    {
                        orderitem.OldRecord = errorinfos[i].NewRecord;

                        nRet = orderitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, strError);

                        orderitem.ItemDisplayState = ItemDisplayState.Normal;
                    }

                    // 对于保存后变得不再属于本种的，要在listview中消除
                    if (String.IsNullOrEmpty(orderitem.RecPath) == false)
                    {
                        string strTempOrderDbName = Global.GetDbName(orderitem.RecPath);
                        string strTempBiblioDbName = Program.MainForm.GetBiblioDbNameFromOrderDbName(strTempOrderDbName);

                        Debug.Assert(String.IsNullOrEmpty(strTempBiblioDbName) == false, "");
                        // TODO: 这里要正规报错

                        string strTempBiblioRecPath = strTempBiblioDbName + "/" + orderitem.Parent;

                        if (strTempBiblioRecPath != this.BiblioRecPath)
                        {
                            this.Items.PhysicalDeleteItem(orderitem);
                            continue;
                        }
                    }

                    orderitem.Error = null;   // 还是显示 空?

                    orderitem.Changed = false;
                    orderitem.RefreshListView();
                    continue;
                }

                // 报错处理
                orderitem.Error = errorinfos[i];
                orderitem.RefreshListView();

                strWarning += strLocationSummary + "在提交订购保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }


            // 最后把没有报错的，那些成功删除事项，都从内存和视觉上抹除
            for (int i = 0; i < this.Items.Count; i++)
            {
                OrderItem orderitem = this.Items[i] as OrderItem;
                if (orderitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    if (string.IsNullOrEmpty(orderitem.ErrorInfo) == true)
                    {
                        this.Items.PhysicalDeleteItem(orderitem);
                        i--;
                    }
                }
            }

            // 修改Changed状态
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Items.Changed;
                this.ContentChanged(this, e1);
            }

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改订购信息后重新提交保存";
                MessageBox.Show(ForegroundWindow.Instance, strWarning);
                return true;
            }

            return false;
        }
        // 从两个记录路径中选择一个不是追加方式的实在路径
        public static string GetOneRecPath(string strRecPath1, string strRecPath2)
        {
            if (string.IsNullOrEmpty(strRecPath1) == true)
                return strRecPath2;

            if (Global.IsAppendRecPath(strRecPath1) == false)
                return strRecPath1;

            return strRecPath2;
        }
#endif



#if NO
        // 构造事项称呼
        static string GetLocationSummary(
            string strIndex,
            string strRecPath,
            string strRefID)
        {
            if (String.IsNullOrEmpty(strIndex) == false)
                return "编号为 '" + strIndex + "' 的事项";
            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";
            // 2009/10/27
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";


            return "无任何定位信息的事项";
        }
#endif

        // 构造事项称呼
        internal override string GetLocationSummary(OrderItem bookitem)
        {
            string strIndex = bookitem.Index;

            if (String.IsNullOrEmpty(strIndex) == false)
                return "编号为 '" + strIndex + "' 的事项";

            string strRecPath = bookitem.RecPath;

            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            string strRefID = bookitem.RefID;
            // 2008/6/24
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";

            return "无任何定位信息的事项";
        }



#if NOOOOOOOOOOOOOOOO
        // 在this.orderitems中定位和dom关联的事项
        // 顺次根据 记录路径 -- 编号 来定位
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateOrderItem(
            string strRecPath,
            XmlDocument dom,
            out OrderItem orderitem,
            out string strIndex,
            out string strError)
        {
            strError = "";
            orderitem = null;
            strIndex = "";

            // 提前获取, 以便任何返回路径时, 都可以得到这些值
            strIndex = DomUtil.GetElementText(dom.DocumentElement,
                "index");

            if (String.IsNullOrEmpty(strRecPath) == false)
            {
                orderitem = this.orderitems.GetItemByRecPath(strRecPath);

                if (orderitem != null)
                    return 1;   // found

            }

            if (String.IsNullOrEmpty(strIndex) == false)
            {
                orderitem = this.orderitems.GetItemByIndex(
                    strIndex,
                    null);
                if (orderitem != null)
                    return 1;   // found

            }

            return 0;
        }
#endif

        // 在this.orderitems中定位和strRecPath/strRefID关联的事项
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateOrderItem(
            string strRefID,
            string strRecPath,
            out OrderItem orderitem,
            out string strError)
        {
            strError = "";

            // 优先用记录路径来定位
            if (string.IsNullOrEmpty(strRecPath) == false
                && Global.IsAppendRecPath(strRecPath) == false)
            {
                orderitem = this.Items.GetItemByRecPath(strRecPath) as OrderItem;
                if (orderitem != null)
                    return 1;   // found
            }

            // 然后用参考ID来定位
            orderitem = this.Items.GetItemByRefID(strRefID, null) as OrderItem;

            if (orderitem != null)
                return 1;   // found

            strError = "没有找到 记录路径为 '" + strRecPath + "'，并且 参考ID 为 '" + strRefID + "' 的OrderItem事项";
            return 0;
        }

        private void ListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool bHasBillioLoaded = false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
                bHasBillioLoaded = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("订购(&O)");
            menuItem.Click += new System.EventHandler(this.menu_design_Click);
            if (bHasBillioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("验收(&A)");
            menuItem.Click += new System.EventHandler(this.menu_arrive_Click);
            if (bHasBillioLoaded == false || this.SeriesMode == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            bool bEnableOpenTarget = true;
            string strTargetRecID = "";
            if (String.IsNullOrEmpty(this.TargetRecPath) == false)
            {
                strTargetRecID = Global.GetRecordID(this.TargetRecPath);
            }

            if (this.OpenTargetRecord == null)
                bEnableOpenTarget = false;
            else if (this.TargetRecPath == this.BiblioRecPath)
                bEnableOpenTarget = false;
            else if (String.IsNullOrEmpty(strTargetRecID) == true || strTargetRecID == "?")
                bEnableOpenTarget = false;

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            bool bAllowModify = StringUtil.IsInList("client_uimodifyorderrecord",
                Program.MainForm._currentUserRights// this.Rights
                ) == true;

            {
                menuItem = new MenuItem("修改(&M)");
                menuItem.Click += new System.EventHandler(this.menu_modifyOrder_Click);
                if (this.listView.SelectedItems.Count == 0 || bAllowModify == false)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);

                // -----
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);


                menuItem = new MenuItem("新增(&N)");
                menuItem.Click += new System.EventHandler(this.menu_newOrder_Click);
                if (bHasBillioLoaded == false || bAllowModify == false)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);

                // -----
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);
            }

            // 全选
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("打开已验收的目标记录 '" + this.TargetRecPath + "' (&T)");
            menuItem.Click += new System.EventHandler(this.menu_openTargetRecord_Click);
            if (bEnableOpenTarget == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("加亮显示属于本种的、验收批次号为 '" + this.AcceptBatchNo + "' 的册记录(&H)");
            menuItem.Click += new System.EventHandler(this.menu_hilightTargetItemLines_Click);
            if (String.IsNullOrEmpty(this.AcceptBatchNo) == true
                || this.HilightTargetItem == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            /*

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // cut 剪切
            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // copy 复制
            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(ClipboardBookItemCollection)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            // paste 粘贴
            menuItem = new MenuItem("粘贴(&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteEntity_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


             * */

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            // 改变归属
            menuItem = new MenuItem("改变归属(&B)");
            menuItem.Click += new System.EventHandler(this.menu_changeParent_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入新开的订购窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToNewItemForm_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入已经打开的订购窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToExistItemForm_Click);
            if (this.listView.SelectedItems.Count == 0
                || Program.MainForm.GetTopChildWindow<ItemInfoForm>() == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("察看订购记录的检索点 (&K)");
            menuItem.Click += new System.EventHandler(this.menu_getKeys_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("标记删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteOrder_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("撤销删除(&U)");
            menuItem.Click += new System.EventHandler(this.menu_undoDeleteOrder_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView, new Point(e.X, e.Y));
        }

        // 全选
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView);
        }

        void menu_loadToNewItemForm_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未选定要操作的事项";
                goto ERROR1;
            }

            OrderItem cur = (OrderItem)this.ListView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "OrderItem == null";
                goto ERROR1;
            }

            string strRecPath = cur.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "所选定的事项记录路径为空，尚未在数据库中建立";
                goto ERROR1;
            }

            ItemInfoForm form = null;

            form = new ItemInfoForm();
            form.MdiParent = Program.MainForm;
            form.MainForm = Program.MainForm;
            form.Show();

            form.DbType = "order";

            form.LoadRecordByRecPath(strRecPath, "");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            LoadToItemInfoForm(true);

        }

        void menu_loadToExistItemForm_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未选定要操作的事项";
                goto ERROR1;
            }

            OrderItem cur = (OrderItem)this.ListView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "OrderItem == null";
                goto ERROR1;
            }

            string strRecPath = cur.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "所选定的事项记录路径为空，尚未在数据库中建立";
                goto ERROR1;
            }

            ItemInfoForm form = Program.MainForm.GetTopChildWindow<ItemInfoForm>();
            if (form == null)
            {
                strError = "当前并没有已经打开的订购窗";
                goto ERROR1;
            }
            form.DbType = "order";
            form.Activate();
            if (form.WindowState == FormWindowState.Minimized)
                form.WindowState = FormWindowState.Normal;

            form.LoadRecordByRecPath(strRecPath, "");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            LoadToItemInfoForm(false);

        }

#if NO
        // 改变归属
        // 即修改订购信息的<parent>元素内容，使指向另外一条书目记录
        void menu_changeParent_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未指定要修改归属的事项";
                goto ERROR1;
            }

            // TODO: 如果有尚未保存的,是否要提醒先保存?

            string strNewBiblioRecPath = InputDlg.GetInput(
                this,
                "请指定新的书目记录路径",
                "书目记录路径(格式'库名/ID'): ",
                "",
            Program.MainForm.DefaultFont);

            if (strNewBiblioRecPath == null)
                return;

            // TODO: 最好检查一下这个路径的格式。合法的书目库名可以在MainForm中找到

            if (String.IsNullOrEmpty(strNewBiblioRecPath) == true)
            {
                strError = "尚未指定新的书目记录路径，放弃操作";
                goto ERROR1;
            }

            if (strNewBiblioRecPath == this.BiblioRecPath)
            {
                strError = "指定的新书目记录路径和当前书目记录路径相同，放弃操作";
                goto ERROR1;
            }

            List<OrderItem> selectedorderitems = new List<OrderItem>();
            foreach (ListViewItem item in this.ListView.SelectedItems)
            {
                // ListViewItem item = this.ListView.SelectedItems[i];

                OrderItem orderitem = (OrderItem)item.Tag;

                selectedorderitems.Add(orderitem);
            }

            EntityInfo[] orders = null;

            nRet = BuildChangeParentRequestEntities(
                selectedorderitems,
                strNewBiblioRecPath,
                out orders,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (orders == null || orders.Length == 0)
                return; // 没有必要保存

#if NO
            EntityInfo[] errorinfos = null;
            nRet = SaveOrderRecords(strNewBiblioRecPath,
                entities,
                out errorinfos,
                out strError);

            // 把出错的事项和需要更新状态的事项兑现到显示、内存
            // 是否有能力把归属已经改变的事项排除出listview?
            RefreshOperResult(errorinfos);


            if (nRet == -1)
            {
                goto ERROR1;
            }
#endif
            nRet = SaveEntities(orders, out strError);
            if (nRet == -1)
                goto ERROR1;

            Program.MainForm.StatusBarMessage = "订购信息 修改归属 成功";
            return;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
        }
#endif

        void menu_hilightTargetItemLines_Click(object sender, EventArgs e)
        {
            if (this.HilightTargetItem == null)
            {
                MessageBox.Show(this, "尚未挂接HilightTargetItem事件");
                return;
            }

            if (String.IsNullOrEmpty(this.AcceptBatchNo) == false)
            {
                HilightTargetItemsEventArgs e1 = new HilightTargetItemsEventArgs();
                e1.BatchNo = this.AcceptBatchNo;
                this.HilightTargetItem(this, e1);
            }
        }

        void menu_openTargetRecord_Click(object sender, EventArgs e)
        {
            if (this.OpenTargetRecord == null)
                return;

            OpenTargetRecordEventArgs e1 = new OpenTargetRecordEventArgs();
            e1.SourceRecPath = this.BiblioRecPath;
            e1.TargetRecPath = this.TargetRecPath;
            e1.BatchNo = this.AcceptBatchNo;
            this.OpenTargetRecord(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                MessageBox.Show(this, e1.ErrorInfo);
        }

        void menu_modifyOrder_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要编辑的事项");
                return;
            }

            bool bAllowModify = StringUtil.IsInList("client_uimodifyorderrecord",
    Program.MainForm._currentUserRights// this.Rights
    ) == true;
            if (bAllowModify == false)
            {
                MessageBox.Show(ForegroundWindow.Instance, "当前用户不具备 client_uimodifyorderrecord 权限");
                return;
            }

            OrderItem orderitem = (OrderItem)this.listView.SelectedItems[0].Tag;

            ModifyOrder(orderitem);
        }

        void menu_newOrder_Click(object sender, EventArgs e)
        {
            DoNewOrder();
        }

        // 订购(规划)
        void menu_design_Click(object sender, EventArgs e)
        {
            DoDesignOrder();
        }

        // 验收
        void menu_arrive_Click(object sender, EventArgs e)
        {
            DoAccept();
        }

        // 撤销删除一个或多个订购事项
        void menu_undoDeleteOrder_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要撤销删除的事项");
                return;
            }

            this.EnableControls(false);

            try
            {
                bool bOldChanged = this.Items.Changed;

                // 实行Undo
                List<ListViewItem> selectedItems = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView.SelectedItems)
                {
                    selectedItems.Add(item);
                }

                string strNotUndoList = "";
                int nUndoCount = 0;
                foreach (ListViewItem item in selectedItems)
                {
                    OrderItem orderitem = (OrderItem)item.Tag;

                    bool bRet = this.Items.UndoMaskDeleteItem(orderitem);

                    if (bRet == false)
                    {
                        if (strNotUndoList != "")
                            strNotUndoList += ",";
                        strNotUndoList += orderitem.Index;
                        continue;
                    }

                    nUndoCount++;
                }

                string strText = "";

                if (strNotUndoList != "")
                    strText += "编号为 '" + strNotUndoList + "' 的事项先前并未被标记删除过, 所以现在谈不上撤销删除。\r\n\r\n";

                strText += "共撤销删除 " + nUndoCount.ToString() + " 项。";
                MessageBox.Show(ForegroundWindow.Instance, strText);

#if NO
                if (this.ContentChanged != null
    && bOldChanged != this.Items.Changed)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = this.Items.Changed;
                    this.ContentChanged(this, e1);
                }
#endif
                TriggerContentChanged(bOldChanged, this.Items.Changed);

            }
            finally
            {
                this.EnableControls(true);
            }
        }

        // 删除一个或多个订购事项
        void menu_deleteOrder_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要标记删除的事项");
                return;
            }

            string strIndexList = "";
            for (int i = 0; i < this.listView.SelectedItems.Count; i++)
            {
                if (i > 20)
                {
                    strIndexList += "...(共 " + this.listView.SelectedItems.Count.ToString() + " 项)";
                    break;
                }
                string strIndex = this.listView.SelectedItems[i].Text;
                strIndexList += strIndex + "\r\n";
            }

            string strWarningText = "以下(编号)订购事项将被标记删除: \r\n" + strIndexList + "\r\n\r\n确实要标记删除它们?";

            // 警告
            DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                strWarningText,
                "OrderControl",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Cancel)
                return;

            List<string> deleted_recpaths = new List<string>();

            this.EnableControls(false);

            try
            {
                bool bOldChanged = this.Items.Changed;

                // 实行删除
                List<ListViewItem> selectedItems = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView.SelectedItems)
                {
                    selectedItems.Add(item);
                }

                string strNotDeleteList = "";
                int nDeleteCount = 0;
                foreach (ListViewItem item in selectedItems)
                {
                    OrderItem orderitem = (OrderItem)item.Tag;

                    int nRet = MaskDeleteItem(orderitem,
                        m_bRemoveDeletedItem);

                    if (nRet == 0)
                    {
                        if (strNotDeleteList != "")
                            strNotDeleteList += ",";
                        strNotDeleteList += orderitem.Index;
                        continue;
                    }

                    if (string.IsNullOrEmpty(orderitem.RecPath) == false)
                        deleted_recpaths.Add(orderitem.RecPath);

                    nDeleteCount++;
                }

                string strText = "";

                if (strNotDeleteList != "")
                    strText += "编号为 '" + strNotDeleteList + "' 的订购事项未能加以标记删除。\r\n\r\n";

                if (deleted_recpaths.Count == 0)
                    strText += "共直接删除 " + nDeleteCount.ToString() + " 项。";
                else if (nDeleteCount - deleted_recpaths.Count == 0)
                    strText += "共标记删除 "
                        + deleted_recpaths.Count.ToString()
                        + " 项。\r\n\r\n(注：所标记删除的事项，要到“提交”后才会真正从服务器删除)";
                else
                    strText += "共标记删除 "
    + deleted_recpaths.Count.ToString()
    + " 项；直接删除 "
    + (nDeleteCount - deleted_recpaths.Count).ToString()
    + " 项。\r\n\r\n(注：所标记删除的事项，要到“提交”后才会真正从服务器删除)";

                MessageBox.Show(ForegroundWindow.Instance, strText);

#if NO
                if (this.ContentChanged != null
                    && bOldChanged != this.Items.Changed)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = this.Items.Changed;
                    this.ContentChanged(this, e1);
                }
#endif
                TriggerContentChanged(bOldChanged, this.Items.Changed);


            }
            finally
            {
                this.EnableControls(true);
            }
        }

#if NO
        // 标记删除事项
        // return:
        //      0   因为有册信息，未能标记删除
        //      1   成功删除
        int MaskDeleteItem(OrderItem orderitem,
            bool bRemoveDeletedItem)
        {
            this.Items.MaskDeleteItem(bRemoveDeletedItem,
                orderitem);
            return 1;
        }
#endif


        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            // menu_modifyOrder_Click(this, null);
            menu_design_Click(this, new EventArgs());
        }

#if NO
        void EnableControls(bool bEnable)
        {
            if (this.EnableControlsEvent == null)
                return;

            EnableControlsEventArgs e = new EnableControlsEventArgs();
            e.bEnable = bEnable;
            this.EnableControlsEvent(this, e);
        }
#endif

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns.SetFirstColumn(nClickColumn,
                this.listView.Columns);

            // 排序
            this.listView.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView.ListViewItemSorter = null;
        }

        // 条码查重
        // return:
        //      -1  出错
        //      0   不重复
        //      1   重复
        /// <summary>
        /// 对distribute 中的 refid 查重
        /// </summary>
        /// <param name="strDistribute">发起查重的 distribute 字符串</param>
        /// <param name="myself">发起查重的对象</param>
        /// <param name="bCheckCurrentList">是否要检查当前列表中的(尚未保存的)事项</param>
        /// <param name="bCheckDb">是否对数据库进行查重</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 不重复; 1: 有重复</returns>
        public int CheckDistributeDup(
            string strDistribute,
            OrderItem myself,
            bool bCheckCurrentList,
            bool bCheckDb,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strDistribute) == true)
                return 0;

            if (bCheckCurrentList == true)
            {
                LocationCollection locations = new LocationCollection();
                nRet = locations.Build(strDistribute, out strError);
                if (nRet == -1)
                {
                    strError = "待查重的馆藏分配字符串 '" + strDistribute + "' 格式错误: " + strError;
                    return -1;
                }

                List<string> refids = locations.GetRefIDs();
                if (refids.Count == 0)
                    return 0;

                foreach (OrderItem item in this.Items)
                {
                    if (item == myself)
                        continue;
                    string strCurrent = item.Distribute;
                    if (string.IsNullOrEmpty(strCurrent) == true)
                        continue;

                    LocationCollection current_locations = new LocationCollection();
                    nRet = current_locations.Build(strCurrent, out strError);
                    if (nRet == -1)
                    {
                        strError = "列表中某订购记录的馆藏分配字符串 '" + strCurrent + "' 格式错误: " + strError;
                        return -1;
                    }
                    if (current_locations.Count == 0)
                        continue;
                    List<string> current_refids = current_locations.GetRefIDs();
                    if (current_refids.Count == 0)
                        continue;
                    foreach (string s in refids)
                    {
                        if (current_refids.IndexOf(s) != -1)
                        {
                            strError = "馆藏分配字符串中的参考ID '" + s + "' 和其它订购记录的馆藏分配字符串发生了重复";
                            return 1;
                        }
                    }
                }
            }

            // 对所有订购记录进行馆藏分配字符串(refid)查重
            if (bCheckDb == true)
            {
            }

            return 0;
        }

#if NO
        // 2009/11/23
        // 根据订购记录路径 检索出 书目记录 和全部下属订购记录，装入窗口
        // parameters:
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int DoSearchOrderByRecPath(string strOrderRecPath)
        {
            int nRet = 0;
            string strError = "";
            // 先检查是否已在本窗口中?

            // 对当前窗口内进行册记录路径查重
            if (this.Items != null)
            {
                OrderItem dupitem = this.Items.GetItemByRecPath(strOrderRecPath) as OrderItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "订购记录 '" + strOrderRecPath + "' 正好为本种中未提交之一删除订购请求。";
                    else
                        strText = "订购记录 '" + strOrderRecPath + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";


            // 根据订购记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchBiblioRecPath(strOrderRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对订购记录路径 '" + strOrderRecPath + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到路径为 '" + strOrderRecPath + "' 的订购记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上订购事项
                OrderItem result_item = HilightLineByItemRecPath(strOrderRecPath, true);
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用订购记录路径检索绝对不会发生重复现象");
            }

            return 0;
        }
#endif

#if NO
        // 根据订购记录路径，检索出其从属的书目记录路径。
        int SearchBiblioRecPath(string strOrderRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索订购记录 '" + strOrderRecPath + "' 所从属的书目记录路径 ...");
            Stop.BeginLoop();

            try
            {
                string strIndex = "@path:" + strOrderRecPath;
                string strOutputItemRecPath = "";

                long lRet = Channel.GetOrderInfo(
                    Stop,
                    strIndex,
                    // "", // strBiblioRecPath,
                    null,
                    out strItemText,
                    out strOutputItemRecPath,
                    out item_timestamp,
                    "recpath",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                return (int)lRet;   // not found
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }
        }

#endif

#if NO
        // return:
        //      -1  出错。已经用MessageBox报错
        //      0   没有装载
        //      1   成功装载
        public int DoLoadRecord(string strBiblioRecPath)
        {
            if (this.LoadRecord == null)
                return 0;

            LoadRecordEventArgs e = new LoadRecordEventArgs();
            e.BiblioRecPath = strBiblioRecPath;
            this.LoadRecord(this, e);
            return e.Result;
        }
#endif
    }

    // 设置998$t目标记录路径
    /// <summary>
    /// 设置目标记录路径事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void SetTargetRecPathEventHandler(object sender,
        SetTargetRecPathEventArgs e);

    /// <summary>
    /// 设置目标记录路径事件的参数
    /// </summary>
    public class SetTargetRecPathEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 目标记录路径
        /// </summary>
        public string TargetRecPath = "";    // [in] 目标记录路径

        /// <summary>
        /// [out] 返回出错信息。如果为非空，表示执行过程出错
        /// </summary>
        public string ErrorInfo = "";   // [out] 如果为非空，表示执行过程出错，这里是出错信息
    }

    // 创建实体(册)事项
    /// <summary>
    /// 创建实体数据事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void GenerateEntityEventHandler(object sender,
        GenerateEntityEventArgs e);

    /// <summary>
    /// 创建实体数据事件的参数
    /// </summary>
    public class GenerateEntityEventArgs : EventArgs
    {
        // 2009/11/5
        /// <summary>
        /// [in] 书目记录。一般用来传递外源书目数据。如果为空，表示直接利用源或者目标的书目记录
        /// </summary>
        public string BiblioRecord = "";    // [in] 书目记录。一般用来传递外源书目数据。如果为空，表示直接利用源或者目标的书目记录

        /// <summary>
        /// [in] 书目记录的格式。为 unimarc usmarc xml 之一
        /// </summary>
        public string BiblioSyntax = "";    // [in] 书目记录的格式 unimarc usmarc xml

        /// <summary>
        /// [in] 是否为期刊模式
        /// </summary>
        public bool SeriesMode = false; // [in] 是否为期刊模式

        /// <summary>
        /// [in] 是否需要立即输入册条码号
        /// </summary>
        public bool InputItemBarcode = true;

        /// <summary>
        /// [in] 是否为新创建的册记录设置“加工中”状态
        /// </summary>
        public bool SetProcessingState = true;

        /// <summary>
        /// [in] 是否为新创建的册记录创建索取号
        /// </summary>
        public bool CreateCallNumber = false;   // [in] 是否创建索取号 2012/5/7

        /// <summary>
        /// [in] 册数据集合
        /// </summary>
        public List<GenerateEntityData> DataList = new List<GenerateEntityData>();

        /// <summary>
        /// [out] 返回出错信息
        /// </summary>
        public string ErrorInfo = "";   // [out] 如果为非空，表示执行过程出错，这里是出错信息

        // 2009/11/8
        /// <summary>
        /// [out] 返回新创建的、或者直接利用的目标记录路径
        /// </summary>
        public string TargetRecPath = "";   // [out] 新创建的、或者直接利用的目标记录路径
    }

    // 一个数据存储单元
    /// <summary>
    /// 创建册时候用到的册信息存储结构
    /// </summary>
    public class GenerateEntityData
    {
        /// <summary>
        /// 动作。为 new/delete/change 之一
        /// </summary>
        public string Action = "";  // new/delete/change
        /// <summary>
        /// 参考ID。保持信息联系的一个唯一性ID值
        /// </summary>
        public string RefID = "";   // 参考ID。保持信息联系的一个唯一性ID值
        /// <summary>
        /// 册记录 XML
        /// </summary>
        public string Xml = ""; // 实体记录XML
        /// <summary>
        /// [out] 返回出错信息
        /// </summary>
        public string ErrorInfo = "";   // [out]如果为非空，表示执行过程出错，这里是出错信息

        // 2010/12/1
        /// <summary>
        /// 套序。例如“1/7”
        /// </summary>
        public string Sequence = "";    // 套序。例如“1/7”
        /// <summary>
        /// 候选的其他价格。格式为: "订购价:CNY12.00;验收价:CNY15.00"
        /// </summary>
        public string OtherPrices = ""; // 候选的其他价格。格式为: "订购价:CNY12.00;验收价:CNY15.00"
    }


    // 询问是否可以验收？
    /// <summary>
    /// 准备验收事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void PrepareAcceptEventHandler(object sender,
        PrepareAcceptEventArgs e);

    /// <summary>
    /// 准备验收事件的参数
    /// </summary>
    public class PrepareAcceptEventArgs : EventArgs
    {
        // 2009/11/8
        /// <summary>
        /// [in] 外源记录的路径
        /// </summary>
        public string BiblioSourceRecPath = ""; // 外源记录的路径
        /// <summary>
        /// [in] 外源记录的书目记录
        /// </summary>
        public string BiblioSourceRecord = "";  // 外源记录的书目记录
        /// <summary>
        /// [in] 外源记录的书目格式。为 unimarc usmarc xml 之一
        /// </summary>
        public string BiblioSourceSyntax = "";  // marc unimarc usmarc xml

        // 
        /// <summary>
        /// [in] 源记录路径
        /// </summary>
        public string SourceRecPath = "";   // 源记录路径

        // 
        /// <summary>
        /// [out] 目标记录路径
        /// </summary>
        public string TargetRecPath = "";   // 目标记录路径

        // 
        /// <summary>
        /// [out] 本次验收的批次号
        /// </summary>
        public string AcceptBatchNo = "";   // 本次验收的批次号

        // 
        /// <summary>
        /// [out] 是否在验收末段，自动出现允许输入各册条码号的界面?
        /// </summary>
        public bool InputItemsBarcode = true;   // 是否在验收末段，自动出现允许输入各册条码号的界面?

        // 
        /// <summary>
        /// [out] 是否为新创建的册记录设置“加工中”状态
        /// </summary>
        public bool SetProcessingState = true;    // 是否为新创建的册记录设置“加工中”状态 2009/10/19

        // 
        /// <summary>
        /// [out] 是否为新创建的册记录创建索取号
        /// </summary>
        public bool CreateCallNumber = true;    // 是否为新创建的册记录创建索取号 2012/5/7

        // 
        /// <summary>
        /// [out] 为册记录中的价格字段设置何种价格值。值为 书目价/订购价/验收价/空白 之一
        /// </summary>
        public string PriceDefault = "验收价";  // 为册记录中的价格字段设置何种价格值。书目价/订购价/验收价/空白

        // 
        /// <summary>
        /// [out] 警告信息。可以对操作者提出警告，如果操作者执意要继续执行，也可以。这里主要警告源和目标title不符合的情况
        /// </summary>
        public string WarningInfo = ""; // 警告信息。可以对操作者提出警告，如果操作者执意要继续执行，也可以。这里主要警告源和目标title不符合的情况

        // 
        /// <summary>
        /// [out] 返回错误信息。如果为非空，表示执行过程出错，或者验收的条件不满足
        /// </summary>
        public string ErrorInfo = "";   // [out]如果为非空，表示执行过程出错，或者验收的条件不满足，这里是出错信息

        // 
        /// <summary>
        /// [out] 是否要放弃操作。如果为true，表示执行过程需放弃。原因在 ErrorInfo 中，放弃前可以显示出来
        /// </summary>
        public bool Cancel = false;     // [out]如果为true，表示执行过程需放弃。原因在ErrorInfo中，放弃前可以显示出来
    }

    /// <summary>
    /// 打开验收目标记录事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void OpenTargetRecordEventHandler(object sender,
        OpenTargetRecordEventArgs e);

    /// <summary>
    /// 打开验收目标记录事件的参数
    /// </summary>
    public class OpenTargetRecordEventArgs : EventArgs
    {
        // 暂不使用
        // 
        /// <summary>
        /// [in] 源记录路径
        /// </summary>
        public string SourceRecPath = "";   // 源记录路径

        // 
        /// <summary>
        /// [in] 目标记录路径
        /// </summary>
        public string TargetRecPath = "";   // 目标记录路径

        // 
        /// <summary>
        /// [in] 验收批次号
        /// </summary>
        public string BatchNo = "";   // 验收批次号


        // 
        /// <summary>
        /// [out] 返回错误信息。如果为非空，表示执行过程出错，或者条件不满足
        /// </summary>
        public string ErrorInfo = "";   // [out]如果为非空，表示执行过程出错，或者条件不满足，这里是出错信息

        // 
        /// <summary>
        /// [out] 是否需要放弃操作。如果为 true，表示执行过程需放弃。原因在 ErrorInfo 中，放弃前可以显示出来
        /// </summary>
        public bool Cancel = false;     // [out]如果为true，表示执行过程需放弃。原因在ErrorInfo中，放弃前可以显示出来
    }

    // 
    /// <summary>
    /// 加亮指定验收批次号的实体行事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void HilightTargetItemsEventHandler(object sender,
        HilightTargetItemsEventArgs e);

    /// <summary>
    /// 加亮指定验收批次号的实体行事件的参数
    /// </summary>
    public class HilightTargetItemsEventArgs : EventArgs
    {
        // 
        /// <summary>
        /// [in] 验收批次号
        /// </summary>
        public string BatchNo = "";   // 验收批次号


        // 
        /// <summary>
        /// [out] 返回错误信息。如果为非空，表示执行过程出错，或者条件不满足
        /// </summary>
        public string ErrorInfo = "";   // [out]如果为非空，表示执行过程出错，或者条件不满足，这里是出错信息

        // 
        /// <summary>
        /// [out] 是否需要放弃操作。如果为 true，表示执行过程需放弃。原因在 ErrorInfo 中
        /// </summary>
        public bool Cancel = false;     // [out]如果为true，表示执行过程需放弃。原因在ErrorInfo中，放弃前可以显示出来
    }

    // 如果不这样书写，视图设计器会出现故障
    /// <summary>
    /// OrderControl 类的基础类
    /// </summary>
    public class OrderControlBase : ItemControlBase<OrderItem, OrderItemCollection>
    {
    }
}
