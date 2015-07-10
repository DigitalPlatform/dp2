using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Drawing;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// EntityControl 等类的基础类
    /// </summary>
    /// <typeparam name="T">事项类型。例如 BookItem IssueItem OrderItem CommentItem</typeparam>
    /// <typeparam name="TC">事项集合类型。例如 BookItemCollection IssueItemCollection OrderItemCollection CommentItemCollection</typeparam>
    public class ItemControlBase<T, TC> : UserControl
        where T : BookItemBase, new ()
        where TC : BookItemCollectionBase, new ()
    {
        /// <summary>
        /// 界面许可 / 禁止状态发生改变
        /// </summary>
        public event EnableControlsHandler EnableControlsEvent = null;

        // Ctrl+A自动创建数据
        /// <summary>
        /// 自动创建数据
        /// </summary>
        public event GenerateDataEventHandler GenerateData = null;

        /// <summary>
        /// 装载记录
        /// </summary>
        public event LoadRecordHandler LoadRecord = null;

        internal bool m_bRemoveDeletedItem = false;   // 在删除事项时, 是否从视觉上抹除这些事项(实际上内存里面还保留有即将提交的事项)?

        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = null;

        /// <summary>
        /// 当前通道的(已登录使用)用户的权限字符串
        /// </summary>
        public string Rights
        {
            get
            {
                if (this.Channel == null)
                    return null;
                return this.Channel.Rights;
            }
        }

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 获得宏的值
        /// </summary>
        public event GetMacroValueHandler GetMacroValue = null;

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        string m_strBiblioRecPath = "";

        // 参与排序的列号数组
        internal SortColumns SortColumns = new SortColumns();

        /// <summary>
        /// 事项集合
        /// </summary>
        public TC Items = null;

        internal void EnableControls(bool bEnable)
        {
            if (this.EnableControlsEvent == null)
                return;

            EnableControlsEventArgs e = new EnableControlsEventArgs();
            e.bEnable = bEnable;
            this.EnableControlsEvent(this, e);
        }

        // 原名 EntityCount
        /// <summary>
        /// 事项数
        /// </summary>
        public int ItemCount
        {
            get
            {
                int nEntityCount = 0;
                if (this.Items != null)
                    nEntityCount = this.Items.Count;

                return nEntityCount;
            }
        }

        /// <summary>
        /// 从属的书目记录路径
        /// </summary>
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
                bool bOldChanged = false;

                if (this.Items != null)
                {
                    bOldChanged = this.Items.Changed;
                    this.Items.Changed = value;
                }

                // 2011/11/10
                if (this.ContentChanged != null)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = value;
                    this.ContentChanged(this, e1);
                }
            }
        }

        internal ListView m_listView = null;

        /// <summary>
        /// ListView 控件
        /// </summary>
        public ListView ListView
        {
            get
            {
                return this.m_listView;
            }
        }

        // 清除listview中的全部事项
        /// <summary>
        /// 清楚全部内容
        /// </summary>
        public void Clear()
        {
            this.m_listView.Items.Clear();

            // 2008/11/22
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.m_listView.Columns);
        }

        // 清除期有关信息
        // 原名 ClearEntities
        /// <summary>
        /// 清楚事项内容
        /// </summary>
        public void ClearItems()
        {
            this.Clear();
            this.Items = new TC();
        }

        internal void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
        /// <summary>
        /// 获得全部事项的数目
        /// </summary>
        /// <returns>数目</returns>
        public int CountOfVisibleItems()
        {
            return this.m_listView.Items.Count;
        }

        // 
        /// <summary>
        /// 在 ListView 中定位一个 Item 对象的 index
        /// </summary>
        /// <param name="item">事项</param>
        /// <returns>index</returns>
        public int IndexOfVisibleItems(T item)
        {
            for (int i = 0; i < this.m_listView.Items.Count; i++)
            {
                T cur = (T)this.m_listView.Items[i].Tag;

                if (cur == item)
                    return i;
            }

            return -1;
        }

        // 根据ListView行index找到一个BookItem对象
        // 那些没有显示在ListView中的BookItem对象则无法通过本函数找到了
        /// <summary>
        /// 根据 ListView 行 index 找到一个 Item 对象
        /// </summary>
        /// <param name="nIndex">index</param>
        /// <returns>Item对象</returns>
        public T GetVisibleItemAt(int nIndex)
        {
            return (T)this.m_listView.Items[nIndex].Tag;
        }
#if NO
        // TODO: 最好删除其中一个函数
        public T GetAtVisibleItems(int nIndex)
        {
            return (T)this.listView.Items[nIndex].Tag;
        }
#endif

        internal bool HasGenerateData()
        {
            if (this.GenerateData == null)
                return false;
            return true;
        }

        internal GenerateDataEventArgs DoGenerateData(string strEntry,
            bool bShowErrorBox = true)
        {
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.ShowErrorBox = bShowErrorBox;
                e1.FocusedControl = this.m_listView;
                e1.ScriptEntry = strEntry;
                this.GenerateData(this, e1);
                return e1;
            }
            else
            {
                MessageBox.Show(this, this.GetType().ToString() + "控件没有挂接 GenerateData 事件");
                return null;
            }
        }

        internal void DoGenerateData(object sender, GenerateDataEventArgs e1)
        {
            if (this.GenerateData != null)
            {
                this.GenerateData(sender, e1);
            }
            else
            {
                MessageBox.Show(this, this.GetType().ToString() + "控件没有挂接 GenerateData 事件");
            }
        }

        internal void TriggerContentChanged(bool bOldChanged, bool bNewChanged)
        {
            // 改变保存按钮状态
            // SetSaveAllButtonState(true);
            if (this.ContentChanged != null && bOldChanged != bNewChanged)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = bNewChanged;
                this.ContentChanged(this, e1);
            }
        }

        // 为CommentItem对象设置缺省值
        // parameters:
        //      strCfgEntry 为"comment_normalRegister_default"或"comment_quickRegister_default"
        /// <summary>
        /// 为事项对象设置缺省值
        /// </summary>
        /// <param name="strCfgEntry">事项配置字符串。
        /// <para>册记录快速登记时为 "quickRegister_default"</para>
        /// <para>册记录一般登记时为 "normalRegister_default"</para>
        /// <para>期记录一般登记时为 "issue_normalRegister_default"</para>
        /// <para>订购记录一般登记时为 "order_normalRegister_default"</para>
        /// <para>评注记录一般登记时为 "comment_normalRegister_default"</para>
        /// </param>
        /// <param name="bGetMacroValue">是否兑现宏。例如在价格字段中可以使用 "@price"</param>
        /// <param name="item">事项</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int SetItemDefaultValues(
            string strCfgEntry,
            bool bGetMacroValue,
            T item,
            out string strError)
        {
            strError = "";

            string strNewDefault = this.MainForm.AppInfo.GetString(
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

            if (bGetMacroValue == true)
            {
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
            }

            strNewDefault = dom.OuterXml;

            int nRet = item.SetData("",
                strNewDefault,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            item.Parent = "";
            item.RecPath = "";
            return 0;
        }

        /// <summary>
        /// 对事项字段内容中的宏进行兑现
        /// </summary>
        /// <param name="item">事项</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int ReplaceMacroValues(
            T item,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "调用 ReplaceMacroValues() 时 this.BiblioRecPath 应当不为空";
                return -1;
            }

            string strXml = "";
            int nRet = item.BuildRecord(false,
                 out strXml,
                 out strError);
            if (nRet == -1)
                return -1;

            // 字符串strNewDefault包含了一个XML记录，里面相当于一个记录的原貌。
            // 但是部分字段的值可能为"@"引导，表示这是一个宏命令。
            // 需要把这些宏兑现后，再正式给控件
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

            {
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
            }

            strXml = dom.OuterXml;

            nRet = item.SetData("",
                strXml,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        internal string DoGetMacroValue(string strMacroName)
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

        // return:
        //      -1  出错。已经用MessageBox报错
        //      0   没有装载
        //      1   成功装载
        /// <summary>
        /// 触发 LoadRecord 事件
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <returns>-1: 出错。已经用 MessageBox 报错了; 0: 没有状态; 1: 成功装载</returns>
        public int TriggerLoadRecord(string strBiblioRecPath)
        {
            if (this.LoadRecord == null)
                return 0;

            LoadRecordEventArgs e = new LoadRecordEventArgs();
            e.BiblioRecPath = strBiblioRecPath;
            this.LoadRecord(this, e);
            return e.Result;
        }

        /// <summary>
        /// Item 类型。item/order/issue/comment
        /// </summary>
        public string ItemType = "";    // item/order/issue/comment

        /// <summary>
        /// Item 类型的显示名称。册/订购/期/评注
        /// </summary>
        public string ItemTypeName = "";    // 册/订购/期/评注

        //
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 装载 Item 记录
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strStyle">装载风格</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有装载; 1: 已经装载</returns>
        public virtual int LoadItemRecords(
            string strBiblioRecPath,
            // bool bDisplayOtherLibraryItem,
            string strStyle,
            out string strError)
        {
            strError = "";
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在装入"+this.ItemTypeName+"信息 ...");
            Stop.BeginLoop();

            // this.Update();   // 优化
            // this.MainForm.Update();

            try
            {
                // string strHtml = "";
                this.ClearItems();
                this.ErrorInfo = "";

                long lPerCount = 100; // 每批获得多少个
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                for (; ; )
                {

                    EntityInfo[] entities = null;

                    // ? Thread.Sleep(500);

                    if (lCount > 0)
                        Stop.SetMessage("正在装入册信息 " + lStart.ToString() + "-" + (lStart + lCount - 1).ToString() + " ...");

                    long lRet = 0;

                    if (this.ItemType == "item")
                    {
                        lRet = Channel.GetEntities(
                             Stop,
                             strBiblioRecPath,
                             lStart,
                             lCount,
                             strStyle,  // bDisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
                             "zh",
                             out entities,
                             out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (this.ItemType == "order")
                    {
                        lRet = Channel.GetOrders(
    Stop,
    strBiblioRecPath,
    lStart,
    lCount,
    "",
    "zh",
    out entities,
    out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (this.ItemType == "issue")
                    {
                        lRet = Channel.GetIssues(
            Stop,
    strBiblioRecPath,
        lStart,
        lCount,
        "",
        "zh",
    out entities,
    out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (this.ItemType == "comment")
                    {
                        lRet = Channel.GetComments(
    Stop,
    strBiblioRecPath,
    lStart,
    lCount,
    "",
    "zh",
    out entities,
    out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        strError = "未知的事项类型 '"+this.ItemType+"'";
                        goto ERROR1;
                    }

                    lResultCount = lRet;

                    if (lRet == 0)
                        return 0;

                    Debug.Assert(entities != null, "");

                    this.m_listView.BeginUpdate();
                    try
                    {
                        for (int i = 0; i < entities.Length; i++)
                        {
                            if (entities[i].ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + entities[i].OldRecPath + "' 的" + this.ItemTypeName + "记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            // 所返回的记录有可能是被过滤掉的
                            if (string.IsNullOrEmpty(entities[i].OldRecord) == true)
                                continue;

                            // 剖析一个册的xml记录，取出有关信息放入listview中
                            T bookitem = new T();

                            int nRet = bookitem.SetData(entities[i].OldRecPath, // NewRecPath
                                     entities[i].OldRecord,
                                     entities[i].OldTimestamp,
                                     out strError);
                            if (nRet == -1)
                                return -1;

                            if (entities[i].ErrorCode == ErrorCodeValue.NoError)
                                bookitem.Error = null;
                            else
                                bookitem.Error = entities[i];

                            this.Items.Add(bookitem);


                            bookitem.AddToListView(this.m_listView);
                        }
                    }
                    finally
                    {
                        this.m_listView.EndUpdate();
                    }

                    lStart += entities.Length;
                    if (lStart >= lResultCount)
                        break;

                    if (lCount == -1)
                        lCount = lPerCount;

                    if (lStart + lCount > lResultCount)
                        lCount = lResultCount - lStart;
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

        // 
        /// <summary>
        /// 将 ListView 中的 Item 事项修改为 new 状态
        /// </summary>
        public void ChangeAllItemToNewState()
        {
            foreach (T bookitem in this.Items)
            {
                // BookItem bookitem = this.BookItems[i];

                if (bookitem.ItemDisplayState == ItemDisplayState.Normal
                    || bookitem.ItemDisplayState == ItemDisplayState.Changed
                    || bookitem.ItemDisplayState == ItemDisplayState.Deleted)   // 注意未提交的deleted也变为new了
                {
                    bookitem.ItemDisplayState = ItemDisplayState.New;
                    bookitem.RefreshListView();
                    bookitem.Changed = true;    // 这一句决定了使能后如果立即关闭EntityForm窗口，是否会警告(实体修改)内容丢失
                }
            }
        }

        // 构造用于修改实体归属的实体信息数组
        // 如果strNewBiblioPath中的书目库名发生变化，那实体记录都要在实体库之间移动，因为实体库和编目库有一定的捆绑关系。
        internal int BuildChangeParentRequestEntities(
            List<T> bookitems,
            string strNewBiblioRecPath,
            out EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            entities = null;
            int nRet = 0;

            string strSourceBiblioDbName = Global.GetDbName(this.BiblioRecPath);
            string strTargetBiblioDbName = Global.GetDbName(strNewBiblioRecPath);

            // 检查一下目标编目库名是不是合法的编目库名
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
            // 2009/10/27
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

            bool bMove = false; // 是否需要移动实体记录
            string strTargetEntityDbName = "";  // 目标实体库名

            if (strSourceBiblioDbName != strTargetBiblioDbName)
            {
                // 书目库发生了改变，才有必要移动。否则仅仅修改实体记录的<parent>即可
                bMove = true;
                strTargetEntityDbName = MainForm.GetItemDbName(strTargetBiblioDbName, this.ItemType);

                // 2008/11/28
                if (String.IsNullOrEmpty(strTargetEntityDbName) == true)
                {
                    strError = "书目库 '" + strTargetBiblioDbName + "' 并没有从属的实体库定义。操作失败";
                    return -1;
                }
            }

            Debug.Assert(bookitems != null, "");

            List<EntityInfo> entityArray = new List<EntityInfo>();

            for (int i = 0; i < bookitems.Count; i++)
            {
                T bookitem = bookitems[i];

                EntityInfo info = new EntityInfo();

                // 2008/4/16
                if (String.IsNullOrEmpty(bookitem.RefID) == true)
                {
                    bookitem.RefID = BookItem.GenRefID();
                    bookitem.RefreshListView();
                }

                info.RefID = bookitem.RefID; // 2008/2/17

                bookitem.Parent = strTargetBiblioRecID;   // !!!

                string strXml = "";
                nRet = bookitem.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                info.OldRecPath = bookitem.RecPath; // 2007/6/2
                if (bMove == false)
                {
                    info.Action = "change";
                    info.NewRecPath = bookitem.RecPath;
                }
                else
                {
                    info.Action = "move";

                    Debug.Assert(String.IsNullOrEmpty(strTargetEntityDbName) == false, "");

                    info.NewRecPath = strTargetEntityDbName + "/?";  // 把实体记录移动到另一个实体库中，追加成一条新记录，而旧记录自动被删除
                }

                info.NewRecord = strXml;
                info.NewTimestamp = null;

                info.OldRecord = bookitem.OldRecord;
                info.OldTimestamp = bookitem.Timestamp;

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

        // 构造用于保存的实体信息数组
        int BuildSaveEntities(
            out EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            entities = null;
            int nRet = 0;

            Debug.Assert(this.Items != null, "");

            List<EntityInfo> entityArray = new List<EntityInfo>();

            foreach (T bookitem in this.Items)
            {
                // BookItem bookitem = this.BookItems[i];

                if (bookitem.ItemDisplayState == ItemDisplayState.Normal)
                    continue;

                EntityInfo info = new EntityInfo();

                // 2008/4/16
                if (String.IsNullOrEmpty(bookitem.RefID) == true)
                {
                    bookitem.RefID = BookItem.GenRefID();
                    bookitem.RefreshListView();
                }

                info.RefID = bookitem.RefID; // 2008/2/17


                string strXml = "";
                nRet = bookitem.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;


                if (bookitem.ItemDisplayState == ItemDisplayState.New)
                {
                    info.Action = "new";
                    info.NewRecPath = "";
                    info.NewRecord = strXml;
                    info.NewTimestamp = null;
                }

                if (bookitem.ItemDisplayState == ItemDisplayState.Changed)
                {
                    info.Action = "change";
                    info.OldRecPath = bookitem.RecPath; // 2007/6/2
                    info.NewRecPath = bookitem.RecPath;

                    info.NewRecord = strXml;
                    info.NewTimestamp = null;

                    info.OldRecord = bookitem.OldRecord;
                    info.OldTimestamp = bookitem.Timestamp;

                }

                if (bookitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    info.Action = "delete";
                    info.OldRecPath = bookitem.RecPath; // NewRecPath

                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    info.OldRecord = bookitem.OldRecord;
                    info.OldTimestamp = bookitem.Timestamp;

                    // 2013/6/18
                    // 删除操作要放在前面执行。否则容易出现条码号重复的情况
                    entityArray.Insert(0, info);
                    continue;
                }

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

        internal static EntityInfo[] GetPart(EntityInfo[] source,
    int nStart,
    int nCount)
        {
            EntityInfo[] result = new EntityInfo[nCount];
            for (int i = 0; i < nCount; i++)
            {
                result[i] = source[i + nStart];
            }
            return result;
        }

        // 分批进行保存
        // return:
        //      -2  部分成功，部分失败
        //      -1  出错
        //      0   保存成功，没有错误和警告
        internal int SaveEntities(EntityInfo[] entities,
            out string strError)
        {
            strError = "";

            bool bWarning = false;
            EntityInfo[] errorinfos = null;
            string strWarning = "";

            int nBatch = 100;
            for (int i = 0; i < (entities.Length / nBatch) + ((entities.Length % nBatch) != 0 ? 1 : 0); i++)
            {
                int nCurrentCount = Math.Min(nBatch, entities.Length - i * nBatch);
                EntityInfo[] current = GetPart(entities, i * nBatch, nCurrentCount);

                int nRet = SaveEntityRecords(this.BiblioRecPath,
                    current,
                    out errorinfos,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                string strError1 = "";
                if (RefreshOperResult(errorinfos, out strError1) == true)
                {
                    bWarning = true;
                    strWarning += " " + strError1;
                }

                if (nRet == -1)
                    return -1;
            }

            if (string.IsNullOrEmpty(strWarning) == false)
                strError += " " + strWarning;

            if (bWarning == true)
                return -2;
            return 0;
        }

        // 把报错信息中的成功事项的状态修改兑现
        // 并且彻底去除没有报错的“删除”BookItem事项（内存和视觉上）
        // return:
        //      false   没有警告
        //      true    出现警告
        bool RefreshOperResult(EntityInfo[] errorinfos,
            out string strWarning)
        {
            int nRet = 0;

            strWarning = ""; // 警告信息

            if (errorinfos == null)
                return false;

            for (int i = 0; i < errorinfos.Length; i++)
            {
                T bookitem = null;

                string strError = "";

                if (String.IsNullOrEmpty(errorinfos[i].RefID) == true)
                {
                    strWarning += " 服务器返回的EntityInfo结构中RefID为空";
                    // MessageBox.Show(ForegroundWindow.Instance, "服务器返回的EntityInfo结构中RefID为空");
                    return true;
                }

                /*
                string strBarcode = "";
                string strRegisterNo = "";
                // 在listview中定位和dom关联的事项
                // 顺次根据 记录路径 -- 条码 -- 登录号 来定位
                nRet = LocateBookItem(
                    errorinfos[i].OldRecPath,   // 原来是NewRecPath
                    dom,
                    out bookitem,
                    out strBarcode,
                    out strRegisterNo,
                    out strError);
                 * */
                nRet = LocateItem(
                    errorinfos[i].RefID,
                    GetOneRecPath(errorinfos[i].NewRecPath, errorinfos[i].OldRecPath),
                    out bookitem,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    // MessageBox.Show(ForegroundWindow.Instance, "定位错误信息 '" + errorinfos[i].ErrorInfo + "' 所在行的过程中发生错误:" + strError);
                    strWarning += " 定位错误信息 '" + errorinfos[i].ErrorInfo + "' 所在行的过程中发生错误:" + strError;
                    continue;
                }

                // string strLocationSummary = GetLocationSummary(strBarcode, strRegisterNo, errorinfos[i].NewRecPath);
#if NO
                string strLocationSummary = GetLocationSummary(bookitem.Barcode,
                    bookitem.RegisterNo,
                    errorinfos[i].NewRecPath,
                    errorinfos[i].RefID);
#endif                
                string strLocationSummary = GetLocationSummary(bookitem);

                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                {
                    if (errorinfos[i].Action == "new")
                    {
                        bookitem.OldRecord = errorinfos[i].NewRecord;
                        /*
                        bookitem.Timestamp = errorinfos[i].NewTimestamp;
                        bookitem.RecPath = errorinfos[i].RecPath;   // 检查一下这里是否还是问号? 如果还是问号，就不对了
                        bookitem.ItemDisplayState = ItemDisplayState.Normal;
                         */
                        nRet = bookitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            // MessageBox.Show(ForegroundWindow.Instance, strError);
                            strWarning += " " + strError;
                        }
                    }
                    else if (errorinfos[i].Action == "change"
                        || errorinfos[i].Action == "move")
                    {
                        bookitem.OldRecord = errorinfos[i].NewRecord;

                        nRet = bookitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            // MessageBox.Show(ForegroundWindow.Instance, strError);
                            strWarning += " " + strError;

                        }

                        bookitem.ItemDisplayState = ItemDisplayState.Normal;
                    }

                    // 对于保存后变得不再属于本种的，要在listview中消除
                    if (String.IsNullOrEmpty(bookitem.RecPath) == false)
                    {
                        string strTempItemDbName = Global.GetDbName(bookitem.RecPath);
                        string strTempBiblioDbName = "";

                        strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(this.ItemType, strTempItemDbName);
                        if (string.IsNullOrEmpty(strTempBiblioDbName) == true)
                        {
                            strWarning += " " + this.ItemType + "类型的数据库名 '" + strTempItemDbName + "' 没有找到对应的书目库名";
                            //// MessageBox.Show(ForegroundWindow.Instance, this.ItemType + "类型的数据库名 '" + strTempItemDbName + "' 没有找到对应的书目库名");
                            return true;
                        }
#if NO
                        if (this.ItemType == "item")
                            strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(strTempItemDbName);
                        else if (this.ItemType == "order")
                            strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromOrderDbName(strTempItemDbName);
                        else if (this.ItemType == "issue")
                            strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromIssueDbName(strTempItemDbName);
                        else if (this.ItemType == "comment")
                            strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromCommentDbName(strTempItemDbName);
                        else
                        {
                            MessageBox.Show(ForegroundWindow.Instance, "未知的 ItemType 类型 '"+this.ItemType+"'");
                            return true;
                        }

                        Debug.Assert(String.IsNullOrEmpty(strTempBiblioDbName) == false, "");
                        // TODO: 这里要正规报错
#endif



                        string strTempBiblioRecPath = strTempBiblioDbName + "/" + bookitem.Parent;

                        if (strTempBiblioRecPath != this.BiblioRecPath)
                        {
                            this.Items.PhysicalDeleteItem(bookitem);
                            continue;
                        }
                    }

                    bookitem.Error = null;   // 还是显示 空?

                    bookitem.Changed = false;   // bookitem的changed变化，间接会引起booitems的changed变化或发送消息？
                    bookitem.RefreshListView();

                    continue;
                }

                // 报错处理
                bookitem.Error = errorinfos[i];
                bookitem.RefreshListView();

                strWarning += strLocationSummary + "在提交保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }


            // 最后把没有报错的，那些成功删除事项，都从内存和视觉上抹除
            for (int i = 0; i < this.Items.Count; i++)
            {
                BookItemBase bookitem = this.Items[i];
                if (bookitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    if (bookitem.ErrorInfo == "")
                    {
                        this.Items.PhysicalDeleteItem(bookitem);
                        i--;    // 2007/4/12
                    }
                }
            }

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改后重新提交保存";
                //// MessageBox.Show(ForegroundWindow.Instance, strWarning);
                return true;
            }

            return false;
        }

        // 构造事项称呼
        // 必须重载
        internal virtual string GetLocationSummary(T bookitem)
        {
            throw new Exception("尚未实现 GetLocationSummary()");
        }

        // 原名 LocateBookItem
        // 在this.bookitems中定位和strRefID关联的事项
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateItem(
            string strRefID,
            out T item,
            out string strError)
        {
            strError = "";
            item = null;

            item = this.Items.GetItemByRefID(strRefID) as T;

            if (item != null)
                return 1;   // found

            strError = "没有找到 id 为 '" + strRefID + "' 的 Item 事项";
            return 0;
        }

        // 原名 LocateBookItem
        // 在this.issueitems中定位和strRefID关联的事项
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateItem(
            string strRefID,
            string strRecPath,
            out T item,
            out string strError)
        {
            strError = "";

            // 优先用记录路径来定位
            if (string.IsNullOrEmpty(strRecPath) == false
                && Global.IsAppendRecPath(strRecPath) == false)
            {
                item = this.Items.GetItemByRecPath(strRecPath) as T;
                if (item != null)
                    return 1;   // found
            }

            // 然后用参考ID来定位
            item = this.Items.GetItemByRefID(strRefID, null) as T;

            if (item != null)
                return 1;   // found

            strError = "没有找到 记录路径为 '" + strRecPath + "'，并且 参考 ID 为 '" + strRefID + "' 的 Item 事项";
            return 0;
        }

        // 
        /// <summary>
        /// 从两个记录路径中选择一个不是追加方式的实在路径
        /// </summary>
        /// <param name="strRecPath1">记录路径</param>
        /// <param name="strRecPath2">记录路径</param>
        /// <returns>返回实在路径</returns>
        public static string GetOneRecPath(string strRecPath1, string strRecPath2)
        {
            if (string.IsNullOrEmpty(strRecPath1) == true)
                return strRecPath2;

            if (Global.IsAppendRecPath(strRecPath1) == false)
                return strRecPath1;

            return strRecPath2;
        }

        // 保存实体记录
        // 不负责刷新界面和报错
        int SaveEntityRecords(string strBiblioRecPath,
            EntityInfo[] entities,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            errorinfos = null;
            strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在保存"+this.ItemTypeName+"信息 ...");
            Stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = 0;

                if (this.ItemType == "item")
                {
                    lRet = Channel.SetEntities(
                         Stop,
                         strBiblioRecPath,
                         entities,
                         out errorinfos,
                         out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (this.ItemType == "order")
                {
                    lRet = Channel.SetOrders(
    Stop,
    strBiblioRecPath,
    entities,
    out errorinfos,
    out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (this.ItemType == "issue")
                {
                    lRet = Channel.SetIssues(
    Stop,
    strBiblioRecPath,
    entities,
    out errorinfos,
    out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (this.ItemType == "comment")
                {
                    lRet = Channel.SetComments(
    Stop,
    strBiblioRecPath,
    entities,
    out errorinfos,
    out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strError = "未知的事项类型 '" + this.ItemType + "'";
                    goto ERROR1;
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

        int m_nInSaveItems = 0;

        // 提交实体保存请求
        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        /// <summary>
        /// 提交 Items 保存请求
        /// </summary>
        /// <returns>-1: 出错; 0: 没有必要保存; 1: 保存成功</returns>
        public virtual int SaveItems(out string strError)
        {
            strError = "";
            if (this.Items == null)
                return 0;

            m_nInSaveItems++;
            try
            {
                if (m_nInSaveItems > 1)
                {
                    strError = "SaveItems() 不允许重入";
                    return -1;
                }

                EnableControls(false);

                try
                {
                    int nRet = 0;

                    if (this.Items == null)
                    {
                        /*
                        strError = "没有册信息需要保存";
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
                        strError = "无法保存册信息，原因：" + strError;
                        goto ERROR1;
                    }

                    nRet = this.Items.CheckRefIDForSave(out strError);
                    if (nRet == -1)
                    {
                        strError = "无法保存册信息，原因：" + strError;
                        goto ERROR1;
                    }

                    EntityInfo[] entities = null;

                    // 构造需要提交的实体信息数组
                    nRet = BuildSaveEntities(
                        out entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (entities == null || entities.Length == 0)
                        return 0; // 没有必要保存

                    // return:
                    //      -2  部分成功，部分失败
                    //      -1  出错
                    //      0   保存成功，没有错误和警告
                    nRet = SaveEntities(entities, out strError);
                    if (nRet != 0)
                        goto ERROR1;

                    this.Changed = false;
                    this.MainForm.StatusBarMessage = this.ItemTypeName + "信息 提交 / 保存 成功";
                    return 1;
                ERROR1:
                    // MessageBox.Show(ForegroundWindow.Instance, strError);
                    return -1;
                }
                finally
                {
                    EnableControls(true);
                }
            }
            finally
            {
                m_nInSaveItems--;
            }
        }


        // 原名 DoSaveEntities
        // 提交实体保存请求
        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        /// <summary>
        /// 提交 Items 保存请求
        /// </summary>
        /// <returns>-1: 出错; 0: 没有必要保存; 1: 保存成功</returns>
        public virtual int DoSaveItems()
        {
            string strError = "";
            int nRet = SaveItems(out strError);

            if (nRet == -1)
            {
                MessageBox.Show(this, strError);    // ForegroundWindow.Instance
                return -1;
            }

            return nRet;
        }

        #region 菜单命令

        internal void menu_getKeys_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.m_listView.SelectedItems.Count == 0)
            {
                strError = "尚未指定要查看检索点的事项";
                goto ERROR1;
            }

            T bookitem = (T)this.ListView.SelectedItems[0].Tag;

            string strXml = "";
            int nRet = bookitem.BuildRecord(false,
                 out strXml,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            string strRecPath = bookitem.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                string strItemDbName = MainForm.GetItemDbName(Global.GetDbName(this.BiblioRecPath), this.ItemType);
                if (string.IsNullOrEmpty(strItemDbName) == true)
                {
                    strError = "无法获得当前 " + this.ItemTypeName + "库的库名";
                    goto ERROR1;
                }
                strRecPath = strItemDbName + "/?";
            }

            DisplayKeys(strRecPath, strXml);
            return;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
        }


        // 改变归属
        // 即修改实体信息的<parent>元素内容，使指向另外一条书目记录
        internal void menu_changeParent_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.m_listView.SelectedItems.Count == 0)
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
            this.MainForm.DefaultFont);

            if (strNewBiblioRecPath == null)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.ListView.SelectedItems)
            {
                items.Add(item);
            }


            // parameters:
            //      items   要改变归属的事项集合。如果为 null，表示全部改变归属
            nRet = ChangeParent(items,
                strNewBiblioRecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.MainForm.StatusBarMessage = this.ItemTypeName + "信息 修改归属 成功";
            return;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
        }

        // 改变归属
        // 即修改实体信息的<parent>元素内容，使指向另外一条书目记录
        // parameters:
        //      items   要改变归属的事项集合。如果为 null，表示全部改变归属
        public int ChangeParent(List<ListViewItem> items,
            string strNewBiblioRecPath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (items == null)
            {
                items = new List<ListViewItem>();
                foreach (ListViewItem item in this.ListView.Items)
                {
                    items.Add(item);
                }
                if (items.Count == 0)
                    return 0;
            }
            else
            {
                if (items.Count == 0)
                {
                    strError = "尚未指定要修改归属的事项";
                    goto ERROR1;
                }
            }

            // TODO: 如果有尚未保存的,是否要提醒先保存?

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

            List<T> selectedbookitems = new List<T>();
            foreach (ListViewItem item in items)
            {
                T bookitem = (T)item.Tag;

                selectedbookitems.Add(bookitem);
            }

            EntityInfo[] entities = null;

            nRet = BuildChangeParentRequestEntities(
                selectedbookitems,
                strNewBiblioRecPath,
                out entities,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (entities == null || entities.Length == 0)
                return 0; // 没有必要保存

#if NO
            EntityInfo[] errorinfos = null;

            nRet = SaveEntityRecords(strNewBiblioRecPath,
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
            // 分批进行保存
            // return:
            //      -2  部分成功，部分失败
            //      -1  出错
            //      0   保存成功，没有错误和警告
            nRet = SaveEntities(entities, out strError);
            if (nRet != 0)
                goto ERROR1;

            return 0;
        ERROR1:
            return -1;
        }

        // parameters:
        //      bNew    是否新开窗口。==false 表示利用已经打开的同类窗口
        internal void LoadToItemInfoForm(bool bNew)
        {
            string strError = "";

            if (this.m_listView.SelectedItems.Count == 0)
            {
                strError = "尚未选定要操作的事项";
                goto ERROR1;
            }

            T cur = (T)this.m_listView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "item == null";
                goto ERROR1;
            }

            string strRecPath = cur.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "所选定的"+this.ItemTypeName+"记录路径为空，尚未在数据库中建立";
                goto ERROR1;
            }

            if (bNew == true)
            {
                ItemInfoForm form = new ItemInfoForm();
                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;
                form.Show();
                form.DbType = this.ItemType;
                form.LoadRecordByRecPath(strRecPath, "");
            }
            else
            {
                ItemInfoForm form = this.MainForm.GetTopChildWindow<ItemInfoForm>();
                if (form == null)
                {
                    strError = "当前并没有已经打开的"+this.ItemTypeName+"窗";
                    goto ERROR1;
                }
                form.DbType = this.ItemType;
                Global.Activate(form);
                form.LoadRecordByRecPath(strRecPath, "");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #endregion

        // 外部调用接口
        /// <summary>
        /// 追加一个新的 Item 记录
        /// </summary>
        /// <param name="item">要追加的事项</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int AppendItem(T item,
            out string strError)
        {
            strError = "";

            item.Parent = Global.GetRecordID(this.BiblioRecPath);

            this.Items.Add(item);

            item.ItemDisplayState = ItemDisplayState.New;
            item.AddToListView(this.m_listView);
            item.HilightListViewItem(true);

            item.Changed = true;
            return 0;
        }

        // 
        // return:
        //      0   因为有流通信息，未能标记删除
        //      1   成功删除
        /// <summary>
        /// 标记删除事项
        /// 重载本方法可以实现特定的功能
        /// </summary>
        /// <param name="item">事项</param>
        /// <param name="bRemoveDeletedItem">是否从 ListView 中移走事项显示</param>
        /// <returns>0: 因为某些原因，未能标记删除; 1: 成功删除</returns>
        public virtual int MaskDeleteItem(T item,
            bool bRemoveDeletedItem = false)
        {
#if NO
            if (String.IsNullOrEmpty(bookitem.Borrower) == false)
                return 0;
#endif

            this.Items.MaskDeleteItem(bRemoveDeletedItem,
                item);
            return 1;
        }

        // 
        /// <summary>
        /// 根据记录路径加亮事项
        /// </summary>
        /// <param name="strItemRecPath">记录路径</param>
        /// <param name="bClearOtherSelection">是否清除其它事项的选择状态</param>
        /// <returns>事项</returns>
        public T HilightLineByItemRecPath(string strItemRecPath,
                bool bClearOtherSelection)
        {
            T bookitem = null;

            if (bClearOtherSelection == true)
            {
                this.m_listView.SelectedItems.Clear();
            }

            if (this.Items != null)
            {
                bookitem = this.Items.GetItemByRecPath(strItemRecPath) as T;
                if (bookitem != null)
                    bookitem.HilightListViewItem(true);
            }

            return bookitem;
        }

        // 
        /// <summary>
        /// 根据册记录参考ID来加亮事项
        /// </summary>
        /// <param name="strItemRefID">事项的参考 ID</param>
        /// <param name="bClearOtherSelection">是否清除其它事项的选择状态</param>
        /// <returns>事项</returns>
        public T HilightLineByItemRefID(string strItemRefID,
                bool bClearOtherSelection)
        {
            T bookitem = null;

            if (bClearOtherSelection == true)
            {
                this.m_listView.SelectedItems.Clear();
            }

            if (this.Items != null)
            {
                bookitem = this.Items.GetItemByRefID(strItemRefID) as T;
                if (bookitem != null)
                    bookitem.HilightListViewItem(true);
            }

            return bookitem;
        }

        /// <summary>
        /// 根据事项记录的某个检索途径 检索出 书目记录 和全部下属事项记录，装入窗口
        /// </summary>
        /// <param name="strSearchPrefix">为 空 / @path: / @refID: 等之一</param>
        /// <param name="strSearchText">检索词</param>
        /// <param name="result_item">返回匹配记录路径的那个事项</param>
        /// <param name="bDisplayWarning">是否显示警告信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchItem(
            string strSearchPrefix,
            string strSearchText,
            out T result_item,
            bool bDisplayWarning = true)
        {
            int nRet = 0;
            string strError = "";
            result_item = null;

            // 先检查是否已在本窗口中?
            // 对当前窗口内进行册记录路径查重
            if (this.Items != null)
            {
                T dupitem = this.Items.GetItemByRecPath(strSearchText) as T;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = this.ItemTypeName + "记录 '" + strSearchText + "' 正好为本种中未提交之一删除" + this.ItemTypeName + "请求。";
                    else
                        strText = this.ItemTypeName + "记录 '" + strSearchText + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    if (bDisplayWarning == true)
                        MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";
            string strOutputItemRecPath = "";
            string strIndex = strSearchPrefix + strSearchText;

            string strFromCaption = "记录路径";
            if (this.ItemType == "item" && string.IsNullOrEmpty(strSearchPrefix) == true)
                strFromCaption = "册条码号";


            // 根据事项记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchBiblioRecPath(strIndex,
                out strOutputItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对"+this.ItemTypeName+"-"+strFromCaption+" '" + strSearchText + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {

                MessageBox.Show(ForegroundWindow.Instance, "没有找到"+strFromCaption+"为 '" + strSearchText + "' 的" + this.ItemTypeName + "记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上事项
                result_item = HilightLineByItemRecPath(strOutputItemRecPath, true); // BUG strSearchText 2013/10/22 修改
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                // Debug.Assert(false, "用" + this.ItemTypeName + "记录路径检索绝对不会发生重复现象");
                Debug.Assert(false, "用" + this.ItemTypeName + "记录 "+strSearchPrefix+" 检索应当不会发生重复现象");
                MessageBox.Show(ForegroundWindow.Instance, "用 '" + strIndex + "' 检索" + this.ItemTypeName + "记录命中多于一条，为 " + nRet.ToString() + " 条");
                return -1;
            }

            return 0;
        }

        // 
        // parameters:
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据事项记录路径 检索出 书目记录 和全部下属事项记录，装入窗口
        /// </summary>
        /// <param name="strItemRecPath">事项记录路径</param>
        /// <param name="result_item">返回匹配记录路径的那个事项</param>
        /// <param name="bDisplayWarning">是否显示警告信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchItemByRecPath(string strItemRecPath,
            out T result_item,
            bool bDisplayWarning = true)
        {
            return DoSearchItem("@path:",
                strItemRecPath,
                out result_item,
                bDisplayWarning);
#if NO
            int nRet = 0;
            string strError = "";
            result_item = null;

            // 先检查是否已在本窗口中?
            // 对当前窗口内进行册记录路径查重
            if (this.Items != null)
            {
                T dupitem = this.Items.GetItemByRecPath(strItemRecPath) as T;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = this.ItemTypeName + "记录 '" + strItemRecPath + "' 正好为本种中未提交之一删除"+this.ItemTypeName+"请求。";
                    else
                        strText = this.ItemTypeName + "记录 '" + strItemRecPath + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    if (bDisplayWarning == true)
                        MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";
            // string strIndex = "@path:" + strItemRecPath;

            // 根据期记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchBiblioRecPath("@path:" + strItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对期记录路径 '" + strItemRecPath + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到路径为 '" + strItemRecPath + "' 的期记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上期事项
                result_item = HilightLineByItemRecPath(strItemRecPath, true);
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用"+this.ItemTypeName+"记录路径检索绝对不会发生重复现象");
            }

            return 0;
#endif
        }

        void DisplayKeys(string strRecPath, string strXml)
        {
            string strError = "";

            string strResultXml = "";
            int nRet = GetKeys(strRecPath,
                strXml,
                out strResultXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = this.ItemTypeName + "记录 "+strRecPath+" 的检索点";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strResultXml;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int GetKeys(string strRecPath,
    string strXml,
    out string strResultXml,
    out string strError)
        {
            strError = "";
            strResultXml = "";

            if (this.MainForm.ServerVersion < 2.43)
            {
                strError = "获得子记录检索点要求 dp2Library 版本在 2.43 及以上";
                return -1;
            }

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得记录 " + strRecPath + " 的检索点 ...");
            Stop.BeginLoop();

            try
            {
                string strBiblio = "";
                string strBiblioRecPath = "";
                string strItemRecPath = "";
                byte[] timestamp = null;
                long lRet = 0;

                lRet = Channel.GetItemInfo(
 Stop,
 this.ItemType,
  "@path:" + strRecPath,
 strXml,
 "keys",
 out strResultXml,
 out strItemRecPath,
 out timestamp,
 "",
 out strBiblio,
 out strBiblioRecPath,
 out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }

            return 1;
        }

        // 根据事项记录路径，检索出其从属的书目记录路径。
        // parameters:
        //      strIndex    注意使用的时候用 @path @refID 引导
        int SearchBiblioRecPath(string strIndex,
            out string strOutputItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            strOutputItemRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索" + this.ItemTypeName + "记录 '" + strIndex + "' 所从属的书目记录路径 ...");
            Stop.BeginLoop();

            try
            {
                // string strIndex = "@path:" + strItemRecPath;
                // string strOutputItemRecPath = "";

                long lRet = 0;

                if (this.ItemType == "item")
                {
                    lRet = Channel.GetItemInfo(
    Stop,
    strIndex,
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
                }
                else if (this.ItemType == "order")
                {
                    lRet = Channel.GetOrderInfo(
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
                }
                else if (this.ItemType == "issue")
                {
                    lRet = Channel.GetIssueInfo(
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
                }
                else if (this.ItemType == "comment")
                {
                    lRet = Channel.GetCommentInfo(
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
                }
                else
                {
                    strError = "未知的事项类型 '" + this.ItemType + "'";
                    return -1;
                } 
                
                return (int)lRet;   // not found
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }
        }

        /// <summary>
        /// 根据事项记录参考ID 检索出 书目记录 和全部下属事项记录，装入窗口
        /// </summary>
        /// <param name="strItemRefID">事项记录的参考 ID</param>
        /// <param name="result_item">返回匹配记录路径的那个事项</param>
        /// <param name="bDisplayWarning">是否显示警告信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchItemByRefID(string strItemRefID,
            out T result_item,
            bool bDisplayWarning = true)
        {
            return DoSearchItem("@refID:",
    strItemRefID,
    out result_item,
    bDisplayWarning);

#if NO
            result_item = null;

            int nRet = 0;
            string strError = "";

            // 先检查是否已在本窗口中?
            // 对当前窗口内进行册记录参考 ID 查重
            if (this.Items != null)
            {
                T dupitem = this.Items.GetItemByRefID(strItemRefID) as T;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = this.ItemTypeName + "记录 '" + strItemRefID + "' 正好为本种中未提交之一删除"+this.ItemTypeName+"请求。";
                    else
                        strText = this.ItemTypeName + "记录 '" + strItemRefID + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";

            string strSearchText = "@refID:" + strItemRefID;

            nRet = SearchBiblioRecPath(strSearchText,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对"+this.ItemTypeName+"记录的参考ID '" + strItemRefID + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到参考 ID 为 '" + strItemRefID + "' 的"+this.ItemTypeName+"记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                result_item = HilightLineByItemRefID(strItemRefID, true);
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用"+this.ItemTypeName+"记录参考 ID 检索应当不会发生重复现象");
                MessageBox.Show(ForegroundWindow.Instance, "用参考ID '" + strItemRefID + "' 检索"+this.ItemTypeName+"记录命中多于一条，为 " + nRet.ToString() + " 条");
                return -1;
            }

            return 0;
            /*
        ERROR1:
            return -1;
             * */
#endif
        }

        public virtual string ErrorInfo
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
                if (this.m_listView != null)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        this.m_listView.Visible = true;
                    else
                        this.m_listView.Visible = false;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制错误信息字符串
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
#if NO
            Brush brush = new SolidBrush(Color.FromArgb(100, 0,0,255));
            e.Graphics.FillEllipse(brush, 30, 30, 100, 100);
#endif
            if (string.IsNullOrEmpty(this.Text) == true)
                return;

            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            format.Alignment = StringAlignment.Center;
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = e.Graphics.MeasureString(this.Text,
                this.Font,
                this.Size.Width,
                format);

            RectangleF textRect = new RectangleF(
(this.Size.Width - size.Width) / 2,
(this.Size.Height - size.Height) / 2,
size.Width,
size.Height);
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.DrawString(
                    this.Text,
                    this.Font,
                    brush,
                    textRect,
                    format);
            }
        }
    }

    /// <summary>
    /// 使能/禁止界面控件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void EnableControlsHandler(object sender,
    EnableControlsEventArgs e);

    /// <summary>
    /// 使能/禁止界面控件的参数
    /// </summary>
    public class EnableControlsEventArgs : EventArgs
    {
        /// <summary>
        /// 使能/禁止
        /// </summary>
        public bool bEnable = false;
    }


    /// <summary>
    /// 装载书目记录和下属的期记录、册记录
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void LoadRecordHandler(object sender,
    LoadRecordEventArgs e);

    /// <summary>
    /// LoadRecordHandler的参数
    /// </summary>
    public class LoadRecordEventArgs : EventArgs
    {
        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string BiblioRecPath = "";

        // return:
        //      -1  出错。已经用MessageBox报错
        //      0   没有装载
        //      1   成功装载
        /// <summary>
        /// 返回值
        ///      -1  出错。已经用MessageBox报错
        ///      0   没有装载
        ///      1   成功装载
        /// </summary>
        public int Result = 0;
    }

}
