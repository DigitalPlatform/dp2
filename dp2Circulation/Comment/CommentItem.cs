using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;  // EntityInfo

namespace dp2Circulation
{
    /// <summary>
    /// 评注信息
    /// 主要用于 CommentControl 中，表示一个评注记录
    /// </summary>
    [Serializable()]
    public class CommentItem : BookItemBase
    {
#if NO
        /// <summary>
        /// 事项的显示状态
        /// </summary>
        public ItemDisplayState ItemDisplayState = ItemDisplayState.Normal;
#endif

        // 列index。注意要保持和CommentControl中的列号一致
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
        /// ListView 栏目下标：评注类型
        /// </summary>
        public const int COLUMN_TYPE = 3;

        /// <summary>
        /// ListView 栏目下标：订购建议
        /// </summary>
        public const int COLUMN_ORDERSUGGESTION = 4;

        /// <summary>
        /// ListView 栏目下标：标题
        /// </summary>
        public const int COLUMN_TITLE = 5;
        /// <summary>
        /// ListView 栏目下标：作者
        /// </summary>
        public const int COLUMN_CREATOR = 6;
        /// <summary>
        /// ListView 栏目下标：图书馆代码
        /// </summary>
        public const int COLUMN_LIBRARYCODE = 7;
        /// <summary>
        /// ListView 栏目下标：主题词
        /// </summary>
        public const int COLUMN_SUBJECT = 8;
        /// <summary>
        /// ListView 栏目下标：内容摘要
        /// </summary>
        public const int COLUMN_SUMMARY = 9;
        /// <summary>
        /// ListView 栏目下标：正文
        /// </summary>
        public const int COLUMN_CONTENT = 10;
        /// <summary>
        /// ListView 栏目下标：创建时间
        /// </summary>
        public const int COLUMN_CREATETIME = 11;
        /// <summary>
        /// ListView 栏目下标：最后修改时间
        /// </summary>
        public const int COLUMN_LASTMODIFIED = 12;
        /// <summary>
        /// ListView 栏目下标：参考 ID
        /// </summary>
        public const int COLUMN_REFID = 13;
        /// <summary>
        /// ListView 栏目下标：操作历史信息
        /// </summary>
        public const int COLUMN_OPERATIONS = 14;
        /// <summary>
        /// ListView 栏目下标：评注记录路径
        /// </summary>
        public const int COLUMN_RECPATH = 15;

        #region 数据成员

#if NO
        /// <summary>
        /// 获取或设置 参考 ID
        /// 对应于评注记录 XML 结构中的 refID 元素内容
        /// </summary>
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
        /// 获取或者设置 当前记录从属的书目记录 ID
        /// 对应于评注记录 XML 结构中的 parent 元素内容
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
                this.Changed = true;
            }
        }
#endif

        /// <summary>
        /// 获取或设置 操作历史 XML 片断信息
        /// 对应于评注记录 XML 结构中的 operations 元素的 InnerXml 内容
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

        // 暂未使用
        /// <summary>
        /// 获取或设置 编号
        /// 对应于评注记录 XML 结构中的 index 元素内容
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
                this.Changed = true; 
            }
        }

        /// <summary>
        /// 获取或设置 记录状态
        /// 对应于评注记录 XML 结构中的 state 元素内容
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
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 类型
        /// 对应于评注记录 XML 结构中的 type 元素内容
        /// </summary>
        public string TypeString
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "type");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "type",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 订购建议
        /// 对应于评注记录 XML 结构中的 orderSuggestion 元素内容
        /// </summary>
        public string OrderSuggestion
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "orderSuggestion");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "orderSuggestion",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 标题
        /// 对应于评注记录 XML 结构中的 title 元素内容
        /// </summary>
        public string Title
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "title");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "title",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 作者
        /// 对应于评注记录 XML 结构中的 creator 元素内容
        /// </summary>
        public string Creator
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "creator");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "creator",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 馆代码
        /// 对应于评注记录 XML 结构中的 libraryCode 元素内容
        /// </summary>
        public string LibraryCode
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "libraryCode");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "libraryCode",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 主题
        /// 对应于评注记录 XML 结构中的 subject 元素内容
        /// </summary>
        public string Subject
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "subject");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "subject",
                    value);
                this.Changed = true; 
            }
        }

        /// <summary>
        /// 获取或设置 摘要
        /// 对应于评注记录 XML 结构中的 summary 元素内容
        /// </summary>
        public string Summary
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "summary");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "summary",
                    value);
                this.Changed = true;
            }
        }

        /// <summary>
        /// 获取或设置 正文
        /// 对应于评注记录 XML 结构中的 content 元素内容
        /// </summary>
        public string Content
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "content");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "content",
                    value);
                this.Changed = true; 
            }
        }

        // 暂未使用
        /// <summary>
        /// 获取或者设置 创建时间。RFC1123 格式
        /// 对应于评注记录 XML 结构中的 createTime 元素内容
        /// </summary>
        public string CreateTime
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "createTime");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement,
                    "createTime",
                    value);
                this.Changed = true; 
            }
        }

        // 暂未使用
        /// <summary>
        /// 获取或设置 最后修改时间。RFC1123格式
        /// 对应于评注记录 XML 结构中的 lastModified 元素内容
        /// </summary>
        public string LastModified
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement,
                    "lastModified");
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, 
                    "lastModified", 
                    value);
                this.Changed = true; 
            }
        }

        #endregion

#if NO
        /// <summary>
        /// 评注记录路径
        /// </summary>
        public string RecPath = "";

        /// <summary>
        /// 是否被修改
        /// </summary>
        bool m_bChanged = false;

        /// <summary>
        /// 旧记录内容
        /// </summary>
        public string OldRecord = "";

        /// <summary>
        /// 当前记录内容
        /// </summary>
        public string CurrentRecord = "";   // 在Serialize过程中用来储存RecordDom内容

        /// <summary>
        /// 记录内容的 XmlDocument 形态
        /// </summary>
        [NonSerialized()]
        public XmlDocument RecordDom = new XmlDocument();

        // 
        /// <summary>
        /// 恢复那些不能序列化的成员值
        /// </summary>
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

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorInfo
        {
            get
            {
                if (this.Error == null)
                    return "";
                return this.Error.ErrorInfo;
            }
        }

        /// <summary>
        /// 存储返回的错误信息
        /// </summary>
        public EntityInfo Error = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommentItem()
        {
            this.RecordDom.LoadXml("<root />");
        }

        /// <summary>
        /// 复制出一个新的 CommentItem 对象
        /// </summary>
        /// <returns>新的 CommentItem 对象</returns>
        public CommentItem Clone()
        {
            CommentItem newObject = new CommentItem();

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

        // 
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="strRecPath">评注记录路径</param>
        /// <param name="strXml">评注记录 XML 内容</param>
        /// <param name="baTimeStamp">评注记录时间戳</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
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

        // 
        /// <summary>
        /// 重新设置数据
        /// </summary>
        /// <param name="strRecPath">评注记录路径</param>
        /// <param name="strNewXml">评注记录 XML 内容</param>
        /// <param name="baTimeStamp">评注记录时间戳</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
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

            this.Changed = false;
            this.ItemDisplayState = ItemDisplayState.Normal;
            return 0;
        }


        /// <summary>
        /// 获得适合于保存的记录信息
        /// </summary>
        /// <param name="strXml">记录 XML 内容</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
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
        /// 当前内容是否被修改过
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

                if ((this.ItemDisplayState == ItemDisplayState.Normal)
                    && this.m_bChanged == true)
                    this.ItemDisplayState = ItemDisplayState.Changed;
                else if ((this.ItemDisplayState == ItemDisplayState.Changed)
                    && this.m_bChanged == false)
                    this.ItemDisplayState = ItemDisplayState.Normal;
            }
        }

        /// <summary>
        /// 将本事项加入到 ListView 中
        /// </summary>
        /// <param name="list">ListView对象</param>
        /// <returns>本次加入的 ListViewItem 对象</returns>
        public ListViewItem AddToListView(ListView list)
        {
            ListViewItem item = new ListViewItem(this.Index, 0);

            ListViewUtil.ChangeItemText(item,
                COLUMN_ERRORINFO,
                this.ErrorInfo);
            ListViewUtil.ChangeItemText(item,
                COLUMN_STATE,
                this.State);
            ListViewUtil.ChangeItemText(item,
                COLUMN_TYPE,
                this.TypeString);
            ListViewUtil.ChangeItemText(item,
    COLUMN_ORDERSUGGESTION,
    this.OrderSuggestion);
            ListViewUtil.ChangeItemText(item,
    COLUMN_TITLE,
    this.Title);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CREATOR,
    this.Creator);
            ListViewUtil.ChangeItemText(item,
COLUMN_LIBRARYCODE,
this.LibraryCode);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SUBJECT,
    this.Subject);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SUMMARY,
    this.Summary);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CONTENT,
    this.Content);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CREATETIME,
    this.CreateTime);
            ListViewUtil.ChangeItemText(item,
    COLUMN_LASTMODIFIED,
    this.LastModified);
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

            this.ListViewItem.Tag = this;   // 将CommentItem对象引用保存在ListViewItem事项中

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
                COLUMN_TYPE,
                this.TypeString);
            ListViewUtil.ChangeItemText(item,
    COLUMN_ORDERSUGGESTION,
    this.OrderSuggestion);
            ListViewUtil.ChangeItemText(item,
    COLUMN_TITLE,
    this.Title);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CREATOR,
    this.Creator);
            ListViewUtil.ChangeItemText(item,
COLUMN_LIBRARYCODE,
this.LibraryCode);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SUBJECT,
    this.Subject);
            ListViewUtil.ChangeItemText(item,
    COLUMN_SUMMARY,
    this.Summary);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CONTENT,
    this.Content);
            ListViewUtil.ChangeItemText(item,
    COLUMN_CREATETIME,
    this.CreateTime);
            ListViewUtil.ChangeItemText(item,
    COLUMN_LASTMODIFIED,
    this.LastModified);
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
        /// <summary>
        /// 将本事项从 ListView 中删除
        /// </summary>
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

        // 
        /// <summary>
        /// 刷新本事项在 ListView 中的各列内容和图标、背景颜色
        /// </summary>
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
                COLUMN_TYPE,
                this.TypeString);
            ListViewUtil.ChangeItemText(item,
                COLUMN_ORDERSUGGESTION,
                this.OrderSuggestion);
            ListViewUtil.ChangeItemText(item,
                COLUMN_TITLE,
                this.Title);
            ListViewUtil.ChangeItemText(item,
                COLUMN_CREATOR,
                this.Creator);
            ListViewUtil.ChangeItemText(item,
    COLUMN_LIBRARYCODE,
    this.LibraryCode);

            ListViewUtil.ChangeItemText(item,
                COLUMN_SUBJECT,
                this.Subject);

            ListViewUtil.ChangeItemText(item,
                COLUMN_SUMMARY,
                this.Summary);
            ListViewUtil.ChangeItemText(item,
                COLUMN_CONTENT,
                this.Content);
            ListViewUtil.ChangeItemText(item,
                COLUMN_CREATETIME,
                this.CreateTime);
            ListViewUtil.ChangeItemText(item,
                COLUMN_LASTMODIFIED,
                this.LastModified);

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
        /// <summary>
        /// 将本事项在 ListView 中的显示刷新为高亮状态
        /// </summary>
        /// <param name="bClearOtherHilight">是否清除 ListView 中其余事项的高亮显示状态？</param>
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
    /// 评注信息事项的集合容器
    /// </summary>
    [Serializable()]
    public class CommentItemCollection : BookItemCollectionBase
    {
#if NO
        // 检查全部事项的 Parent 成员值是否适合保存
        // return:
        //      -1  有错误，不适合保存
        //      0   没有错误
        /// <summary>
        /// 检查全部事项的 Parent 成员值是否适合保存
        /// 如果发现有空的 Parent 成员值，或者 '?' 值的 Parent 成员值，则会返回错误
        /// </summary>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 没有错误</returns>
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
                    strError = "评注事项中出现了空的 ParentID 值";
                    return -1;
                }

                if (strID == "?")
                {
                    strError = "评注事项中出现了 '?' 式的 ParentID 值";
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 获得 Panrent ID 列表
        /// </summary>
        /// <returns>Parent ID 列表</returns>
        public List<string> GetParentIDs()
        {
            List<string> results = new List<string>();

            for (int i = 0; i < this.Count; i++)
            {
                CommentItem item = this[i];
                string strParentID = item.Parent;
                if (results.IndexOf(strParentID) == -1)
                    results.Add(strParentID);
            }

            return results;
        }

        // 设置全部commentitem事项的Parent域
        /// <summary>
        /// 为全部事项设置一致的 Parent ID 值
        /// </summary>
        /// <param name="strParentID">要设置的 Parent ID 值</param>
        public void SetParentID(string strParentID)
        {
            for (int i = 0; i < this.Count; i++)
            {
                CommentItem item = this[i];
                if (item.Parent != strParentID) // 避免连带无谓地修改item.Changed
                    item.Parent = strParentID;
            }
        }

#endif

        /// <summary>
        /// 以下标编号定位一个事项
        /// </summary>
        /// <param name="strIndex">下标编号。从0开始计数</param>
        /// <param name="excludeItems">判断中需要排除的事项</param>
        /// <returns>找到的事项。null 表示没有找到</returns>
        public CommentItem GetItemByIndex(string strIndex,
            List<CommentItem> excludeItems)
        {
            foreach (CommentItem item in this)
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
        /// <param name="strRecPath">评注记录路径</param>
        /// <returns>找到的事项。null 表示没有找到</returns>
        public CommentItem GetItemByRecPath(string strRecPath)
        {
            for (int i = 0; i < this.Count; i++)
            {
                CommentItem item = this[i];
                if (item.RecPath == strRecPath)
                    return item;
            }

            return null;
        }
#endif

        // parameters:
        //      strLibraryCodeList  馆代码列表，用于过滤。仅统计这个列表中的。如果为null表示全部统计
        //      nYes    建议订购的数量
        //      nNo     建议不订购的数量
        //      nNull   没有表态，但也是“订购征询”的数量
        //      nOther  "订购征询"以外的数量
        /// <summary>
        /// 获得建议订购的统计信息。也就是那些类型为“订购征询”的事项的个数
        /// </summary>
        /// <param name="strLibraryCodeList">馆代码列表，用于过滤参与统计的事项。仅统计符合馆代码范围的事项。如果本参数为 null， 表示全部事项都参与统计</param>
        /// <param name="nYes">选择了 Yes 的个数</param>
        /// <param name="nNo">选择了 No 的个数</param>
        /// <param name="nNull">既没有选择 Yes 也没有选择 No 的个数</param>
        /// <param name="nOther">类型不是“订购征询”的事项个数</param>
        public void GetOrderSuggestion(
            string strLibraryCodeList,
            out int nYes,
            out int nNo,
            out int nNull,
            out int nOther)
        {
            nYes = 0;
            nNo = 0;
            nNull = 0;
            nOther = 0;

            foreach (CommentItem item in this)
            {
                if (Global.IsGlobalUser(strLibraryCodeList) == false)
                {
                    // 注意：item.LibraryCode可能是一个逗号列表
                    if (StringUtil.IsInList(item.LibraryCode, strLibraryCodeList) == false)
                        continue;
                }

                if (item.TypeString != "订购征询")
                {
                    nOther++;
                    continue;
                }

                if (item.OrderSuggestion == "yes")
                    nYes++;
                else if (string.IsNullOrEmpty(item.OrderSuggestion) == true)
                    nNull++;
                else
                    nNo++;
            }
        }

#if NO
        /// <summary>
        /// 以参考 ID 定位一个事项
        /// </summary>
        /// <param name="strRefID">参考 ID</param>
        /// <param name="excludeItems">要加以排除的事项列表</param>
        /// <returns>找到的事项。null 表示没有找到</returns>
        public CommentItem GetItemByRefID(string strRefID,
            List<CommentItem> excludeItems)
        {
            for (int i = 0; i < this.Count; i++)
            {
                CommentItem item = this[i];

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
        /// 获取或者设置：集合是否修改过
        /// 只要有一个元素被修改过，就当作集合被修改过
        /// </summary>
        public bool Changed
        {
            get
            {
                if (this.m_bChanged == true)
                    return true;

                for (int i = 0; i < this.Count; i++)
                {
                    CommentItem item = this[i];
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
                        CommentItem item = this[i];
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

        // 
        /// <summary>
        /// 标记删除指定的事项
        /// </summary>
        /// <param name="bRemoveFromList">是否要从 ListView 中移除这个事项?</param>
        /// <param name="comentitem">要标记删除的事项</param>
        public void MaskDeleteItem(
            bool bRemoveFromList,
            CommentItem comentitem)
        {
            if (comentitem.ItemDisplayState == ItemDisplayState.New)
            {
                PhysicalDeleteItem(comentitem);
                return;
            }


            comentitem.ItemDisplayState = ItemDisplayState.Deleted;
            comentitem.Changed = true;

            // 从listview中消失?
            if (bRemoveFromList == true)
                comentitem.DeleteFromListView();
            else
            {
                comentitem.RefreshListView();
            }
        }

        // Undo标记删除
        // return:
        //      false   没有必要Undo
        //      true    成功Undo
        /// <summary>
        /// 撤销对一个事项的标记删除
        /// </summary>
        /// <param name="commentitem">要撤销标记删除的事项</param>
        /// <returns>false: 没有必要撤销(因为指定的事项不在标记删除状态); true: 成功撤销标记删除</returns>
        public bool UndoMaskDeleteItem(CommentItem commentitem)
        {
            if (commentitem.ItemDisplayState != ItemDisplayState.Deleted)
                return false;   // 要Undo的事项根本就不是Deleted状态，所以谈不上Undo

            // 因为不知道上次标记删除前数据是否改过，因此全当改过
            commentitem.ItemDisplayState = ItemDisplayState.Changed;
            commentitem.Changed = true;

            // 刷新
            commentitem.RefreshListView();
            return true;
        }

        // 从集合中和视觉上同时删除
        /// <summary>
        /// 从集合中和 ListView 中同时清除指定的事项。
        /// 注意，不是指从数据库删除记录
        /// </summary>
        /// <param name="commentitem"></param>
        public void PhysicalDeleteItem(
            CommentItem commentitem)
        {
            // 从listview中消失
            commentitem.DeleteFromListView();

            this.Remove(commentitem);
        }

        /// <summary>
        /// 清除 ListView 中全部高亮状态的行
        /// </summary>
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

        /// <summary>
        /// 清除集合内元素
        /// 从集合中和 ListView 中同时清除全部事项
        /// 注意，不是指从数据库删除记录
        /// </summary>
        public new void Clear()
        {
            for (int i = 0; i < this.Count; i++)
            {
                CommentItem item = this[i];
                item.DeleteFromListView();
            }

            base.Clear();
        }

        // 把事项重新全部加入listview
        /// <summary>
        /// 把当前集合中的事项全部加入 ListView
        /// </summary>
        /// <param name="list"></param>
        public void AddToListView(ListView list)
        {
            for (int i = 0; i < this.Count; i++)
            {
                CommentItem item = this[i];
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
            dom.LoadXml("<comments />");

            foreach (CommentItem item in this)
            {
                XmlNode node = dom.CreateElement("dprms", "comment", DpNs.dprms);
                dom.DocumentElement.AppendChild(node);

                node.InnerXml = item.RecordDom.DocumentElement.InnerXml;
            }

            strXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        /// <summary>
        /// 根据一个 XML 字符串内容，构建出集合内的若干事项
        /// </summary>
        /// <param name="nodeCommentCollection">XmlNode对象，本方法将使用其下属的 dprms:comment 元素来构造事项</param>
        /// <param name="list">ListView 对象。构造好的事项会显示到其中</param>
        /// <param name="bRefreshRefID">构造事项的过程中，是否要刷新每个事项的 RefID 成员值</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 成功</returns>
        public int ImportFromXml(XmlNode nodeCommentCollection,
            ListView list,
            bool bRefreshRefID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nodeCommentCollection == null)
                return 0;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = nodeCommentCollection.SelectNodes("dprms:comment", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                CommentItem comment_item = new CommentItem();
                nRet = comment_item.SetData("",
                    node.OuterXml,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bRefreshRefID == true)
                    comment_item.RefID = Guid.NewGuid().ToString();

                this.Add(comment_item);
                comment_item.ItemDisplayState = ItemDisplayState.New;
                comment_item.AddToListView(list);

                comment_item.Changed = true;
            }

            return 0;
        }
    }
}

