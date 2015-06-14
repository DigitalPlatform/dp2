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

using DigitalPlatform.CirculationClient.localhost;  // IssueInfo
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 期信息
    /// 主要用于 IssueControl 中，表示一个期记录
    /// </summary>
    [Serializable()]
    public class IssueItem : BookItemBase
    {
#if NO
        public ItemDisplayState ItemDisplayState = ItemDisplayState.Normal;

#endif

        // 列index。注意要保持和IssueControl中的列号一致

        /// <summary>
        /// ListView 栏目下标：出版日期
        /// </summary>
        public const int COLUMN_PUBLISHTIME = 0;
        /// <summary>
        /// ListView 栏目下标：错误信息
        /// </summary>
        public const int COLUMN_ERRORINFO = 1;
        /// <summary>
        /// ListView 栏目下标：记录状态
        /// </summary>
        public const int COLUMN_STATE = 2;
        /// <summary>
        /// ListView 栏目下标：期号
        /// </summary>
        public const int COLUMN_ISSUE = 3;
        /// <summary>
        /// ListView 栏目下标：总期号
        /// </summary>
        public const int COLUMN_ZONG = 4;
        /// <summary>
        /// ListView 栏目下标：卷号
        /// </summary>
        public const int COLUMN_VOLUME = 5;
        /// <summary>
        /// ListView 栏目下标：订购信息
        /// </summary>
        public const int COLUMN_ORDERINFO = 6;
        /// <summary>
        /// ListView 栏目下标：注释
        /// </summary>
        public const int COLUMN_COMMENT = 7;
        /// <summary>
        /// ListView 栏目下标：批次号
        /// </summary>
        public const int COLUMN_BATCHNO = 8;
        /// <summary>
        /// ListView 栏目下标：参考 ID
        /// </summary>
        public const int COLUMN_REFID = 9;
        /// <summary>
        /// ListView 栏目下标：操作历史信息
        /// </summary>
        public const int COLUMN_OPERATIONS = 10;
        /// <summary>
        /// ListView 栏目下标：期记录路径
        /// </summary>
        public const int COLUMN_RECPATH = 11;

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
                    "parent", value);
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
        /// 出版时间
        /// </summary>
        public string PublishTime
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "publishTime");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, 
                    "publishTime", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 期状态
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
                    "state", value);
                this.Changed = true; // 2009/3/5
            }
        }



        /// <summary>
        /// 订购信息
        /// </summary>
        public string OrderInfo
        {
            get
            {
                /*
                XmlNode node = this.RecordDom.DocumentElement.SelectSingleNode("orderInfo");
                if (node == null)
                    return "";

                return node.InnerXml;
                 * */
                return DomUtil.GetElementInnerXml(this.RecordDom.DocumentElement,
                    "orderInfo");
            }
            set
            {
                /*
                XmlNode node = this.RecordDom.DocumentElement.SelectSingleNode("orderInfo");
                if (node == null)
                {
                    node = this.RecordDom.CreateElement("orderInfo");
                    this.RecordDom.DocumentElement.AppendChild(node);
                }

                // 注意，可能抛出异常
                node.InnerXml = value;
                 * */

                // 注意，可能抛出异常
                DomUtil.SetElementInnerXml(this.RecordDom.DocumentElement, 
                    "orderInfo",
                    value);
                this.Changed = true; // 2009/11/24
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
                    "comment", value);
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
                    "batchNo", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 卷号
        /// </summary>
        public string Volume
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "volume");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, 
                    "volume", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 总期号
        /// </summary>
        public string Zong
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "zong");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, 
                    "zong", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 期号
        /// </summary>
        public string Issue
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, 
                    "issue");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "issue", value);
                this.Changed = true; // 2009/3/5
            }
        }

        #endregion

#if NO
        /// <summary>
        ///  期记录路径
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
        public IssueItem()
        {
            this.RecordDom.LoadXml("<root />");
        }


        public IssueItem Clone()
        {
            IssueItem newObject = new IssueItem();

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

#endif

        // 2013/6/18
        // 
        // return:
        //      -1  出错
        //      0   没有发生替换修改
        //      >0  共修改了多少个<refID>元素内容
        /// <summary>
        /// 更换 orderInfo 元素里的 refID 元素中的 参考 ID 字符串
        /// </summary>
        /// <param name="order_refid_change_table">参考 ID 对照表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1  出错; 0   没有发生替换修改; >0  共修改了多少个 refID 元素内容</returns>
        public int ReplaceOrderInfoRefID(Hashtable order_refid_change_table,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            string strOrderInfo = this.OrderInfo;
            if (String.IsNullOrEmpty(strOrderInfo) == true)
                return 0;


            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<orderInfo/>");
            try
            {
                dom.DocumentElement.InnerXml = strOrderInfo;
            }
            catch (Exception ex)
            {
                strError = "load inner xml error: " + ex.Message;
                return -1;
            }

            int nChangedCount = 0;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*/refID");
            foreach (XmlNode node in nodes)
            {
                string strOldValue = node.InnerText;
                if (String.IsNullOrEmpty(strOldValue) == true)
                    continue;

                string strNewValue = (string)order_refid_change_table[strOldValue];
                if (string.IsNullOrEmpty(strNewValue) == false)
                {
                    node.InnerText = strNewValue;
                    nChangedCount++;
                }
            }

            if (nChangedCount > 0)
                this.OrderInfo = dom.DocumentElement.InnerXml;

            return nChangedCount;
        }

        // 
        // return:
        //      -1  出错
        //      0   没有发生替换修改
        //      >0  共修改了多少个<distribute>元素内容
        /// <summary>
        /// 更换 orderInfo 元素里的 distribute 元素中的 refid 字符串
        /// </summary>
        /// <param name="item_refid_change_table">参考 ID 对照表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1  出错; 0   没有发生替换修改; >0  共修改了多少个 distribute 元素内容</returns>
        public int ReplaceOrderInfoItemRefID(Hashtable item_refid_change_table,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strOrderInfo = this.OrderInfo;
            if (String.IsNullOrEmpty(strOrderInfo) == true)
                return 0;


            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<orderInfo/>");
            try
            {
                dom.DocumentElement.InnerXml = strOrderInfo;
            }
            catch (Exception ex)
            {
                strError = "load inner xml error: " + ex.Message;
                return -1;
            }

            int nChangedCount = 0;
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*/distribute");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strDistribute = nodes[i].InnerText;
                if (String.IsNullOrEmpty(strDistribute) == true)
                    continue;

                LocationCollection locations = new LocationCollection();
                nRet = locations.Build(strDistribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                bool bChanged = false;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location location = locations[j];
                    if (item_refid_change_table.Contains(location.RefID) == true)
                    {
                        location.RefID = (string)item_refid_change_table[location.RefID];
                        bChanged = true;
                    }
                }

                if (bChanged == true)
                {
                    nodes[i].InnerText = locations.ToString(true);
                    nChangedCount++;
                }

            }

            if (nChangedCount > 0)
                this.OrderInfo = dom.DocumentElement.InnerXml;

            return nChangedCount;
        }

#if NO
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
            ListViewItem item = new ListViewItem(this.PublishTime, 0);

            /*
            item.SubItems.Add(this.ErrorInfo);
            item.SubItems.Add(this.State);
            item.SubItems.Add(this.Issue);
            item.SubItems.Add(this.Zong);
            item.SubItems.Add(this.Volume);
            item.SubItems.Add(this.OrderInfo);

            item.SubItems.Add(this.Comment);
            item.SubItems.Add(this.BatchNo);

            item.SubItems.Add(this.RefID);  // 2010/2/27

            item.SubItems.Add(this.RecPath);
             * */
            ListViewUtil.ChangeItemText(item,
                COLUMN_ERRORINFO,
                this.ErrorInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ISSUE,
                this.Issue);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ZONG,
                this.Zong);
            ListViewUtil.ChangeItemText(item,
                COLUMN_VOLUME,
                this.Volume);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ORDERINFO,
                this.OrderInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_COMMENT,
                this.Comment);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BATCHNO,
                this.BatchNo);
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

            this.ListViewItem = item;

            this.ListViewItem.Tag = this;   // 将IssueItem对象引用保存在ListViewItem事项中

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
                COLUMN_PUBLISHTIME,
                this.PublishTime);  // 2014/6/5
            ListViewUtil.ChangeItemText(item,
                COLUMN_ERRORINFO,
                this.ErrorInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ISSUE,
                this.Issue);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ZONG,
                this.Zong);
            ListViewUtil.ChangeItemText(item,
                COLUMN_VOLUME,
                this.Volume);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ORDERINFO,
                this.OrderInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_COMMENT,
                this.Comment);
            ListViewUtil.ChangeItemText(item,
                COLUMN_BATCHNO,
                this.BatchNo);
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
                COLUMN_PUBLISHTIME,
                this.PublishTime);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ERRORINFO,
                this.ErrorInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ISSUE,
                this.Issue);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ZONG,
                this.Zong);
            ListViewUtil.ChangeItemText(item,
                COLUMN_VOLUME,
                this.Volume);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ORDERINFO,
                this.OrderInfo);


            ListViewUtil.ChangeItemText(item,
                COLUMN_COMMENT,
                this.Comment);
            ListViewUtil.ChangeItemText(item, 
                COLUMN_BATCHNO,
                this.BatchNo);

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
    /// 册信息的集合容器
    /// </summary>
    [Serializable()]
    public class IssueItemCollection : BookItemCollectionBase
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
                    strError = "册事项中出现了空的ParentID值";
                    return -1;
                }

                if (strID == "?")
                {
                    strError = "册事项中出现了'?'式的ParentID值";
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
                IssueItem item = this[i];
                string strParentID = item.Parent;
                if (results.IndexOf(strParentID) == -1)
                    results.Add(strParentID);
            }

            return results;
        }


        // 设置全部isueitem事项的Parent域
        public void SetParentID(string strParentID)
        {
            for (int i = 0; i < this.Count; i++)
            {
                IssueItem item = this[i];
                if (item.Parent != strParentID) // 避免连带无谓地修改item.Changed 2009/3/6
                    item.Parent = strParentID;
            }
        }

#endif

        /// <summary>
        /// 以出版时间定位一个事项
        /// </summary>
        /// <param name="strPublishTime">出版时间</param>
        /// <param name="excludeItems">判断中需要排除的事项</param>
        /// <returns>找到的事项。null 表示没有找到</returns>
        public IssueItem GetItemByPublishTime(string strPublishTime,
            List<IssueItem> excludeItems)
        {
            foreach (IssueItem item in this)
            {
                // 需要排除的事项
                if (excludeItems != null)
                {
                    if (excludeItems.IndexOf(item) != -1)
                        continue;
                }

                if (item.PublishTime == strPublishTime)
                    return item;
            }

            return null;
        }

#if NO
        /// <summary>
        /// 以记录路径定位一个事项
        /// </summary>
        /// <param name="strRegisterNo"></param>
        /// <returns></returns>
        public IssueItem GetItemByRecPath(string strRecPath)
        {
            for (int i = 0; i < this.Count; i++)
            {
                IssueItem item = this[i];
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
        public IssueItem GetItemByRefID(string strRefID,
            List<IssueItem> excludeItems)
        {
            for (int i = 0; i < this.Count; i++)
            {
                IssueItem item = this[i];

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
                    IssueItem item = this[i];
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
                        IssueItem item = this[i];
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
            IssueItem issueitem)
        {
            if (issueitem.ItemDisplayState == ItemDisplayState.New)
            {
                PhysicalDeleteItem(issueitem);
                return;
            }


            issueitem.ItemDisplayState = ItemDisplayState.Deleted;
            issueitem.Changed = true;

            // 从listview中消失?
            if (bRemoveFromList == true)
                issueitem.DeleteFromListView();
            else
            {
                issueitem.RefreshListView();
            }
        }

        // Undo标记删除
        // return:
        //      false   没有必要Undo
        //      true    已经Undo
        public bool UndoMaskDeleteItem(IssueItem issueitem)
        {
            if (issueitem.ItemDisplayState != ItemDisplayState.Deleted)
                return false;   // 要Undo的事项根本就不是Deleted状态，所以谈不上Undo

            // 因为不知道上次标记删除前数据是否改过，因此全当改过
            issueitem.ItemDisplayState = ItemDisplayState.Changed;
            issueitem.Changed = true;

            // 刷新
            issueitem.RefreshListView();
            return true;
        }

        // 从集合中和视觉上同时删除
        public void PhysicalDeleteItem(
            IssueItem issueitem)
        {
            // 从listview中消失
            issueitem.DeleteFromListView();

            this.Remove(issueitem);
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
                this[i].DeleteFromListView();
            }

            base.Clear();
        }

        // 把事项重新全部加入listview
        public void AddToListView(ListView list)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].AddToListView(list);
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
            dom.LoadXml("<issues />");

            foreach (IssueItem item in this)
            {
                XmlNode node = dom.CreateElement("dprms", "issue", DpNs.dprms);
                dom.DocumentElement.AppendChild(node);

                node.InnerXml = item.RecordDom.DocumentElement.InnerXml;
            }

            strXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        // parameters:
        //      order_refid_change_table   订购记录的 refid 变迁情况
        //      item_refid_change_table 发生过修改的册refid对照表。key为旧值，value为新值
        /// <summary>
        /// 根据一个 XML 字符串内容，构建出集合内的若干事项
        /// </summary>
        /// <param name="nodeIssueCollection">XmlNode对象，本方法将使用其下属的 dprms:issue 元素来构造事项</param>
        /// <param name="list">ListView 对象。构造好的事项会显示到其中</param>
        /// <param name="order_refid_change_table">订购记录的 refid 变迁情况</param>
        /// <param name="bRefreshRefID">构造事项的过程中，是否要刷新每个事项的 RefID 成员值</param>
        /// <param name="item_refid_change_table">发生过修改的册refid对照表。key为旧值，value为新值</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
        public int ImportFromXml(XmlNode nodeIssueCollection,
            ListView list,
            Hashtable order_refid_change_table,
            bool bRefreshRefID,
            Hashtable item_refid_change_table,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nodeIssueCollection == null)
                return 0;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = nodeIssueCollection.SelectNodes("dprms:issue", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                IssueItem issue_item = new IssueItem();
                nRet = issue_item.SetData("",
                    node.OuterXml,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bRefreshRefID == true)
                    issue_item.RefID = Guid.NewGuid().ToString();

                if (item_refid_change_table != null
                    && item_refid_change_table.Count > 0)
                {
                    // 更换<orderInfo>里的<distribute>中的refid字符串
                    nRet = issue_item.ReplaceOrderInfoItemRefID(item_refid_change_table,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                if (order_refid_change_table != null
                    && order_refid_change_table.Count > 0)
                {
                    // 更换<orderInfo>里的<refID>中的refid字符串
                    nRet = issue_item.ReplaceOrderInfoRefID(order_refid_change_table,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                this.Add(issue_item);
                issue_item.ItemDisplayState = ItemDisplayState.New;
                issue_item.AddToListView(list);

                issue_item.Changed = true;
            }

            return 0;
        }
    }

}

