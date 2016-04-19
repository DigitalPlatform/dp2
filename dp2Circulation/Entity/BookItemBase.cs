using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 种册窗属性页内列表事项的基类
    /// </summary>
    [Serializable()]
    public class BookItemBase
    {
        /// <summary>
        /// 事项的显示状态
        /// </summary>
        public ItemDisplayState ItemDisplayState = ItemDisplayState.Normal;

        /// <summary>
        ///  册记录路径
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
        /// 当前记录
        /// </summary>
        public string CurrentRecord = "";   // 在Serialize过程中用来储存RecordDom内容

        /// <summary>
        /// 表示当前对象内容的 XmlDocument
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
        /// 错误信息字符串
        /// </summary>
        public string ErrorInfo
        {
            get
            {
                if (this.Error == null)
                    return "";
                return this.Error.ErrorInfo;
            }
            set
            {
                // 2011/6/18 
                if (this.Error != null)
                    this.Error.ErrorInfo = value;
            }
        }

        /// <summary>
        /// 错误信息对象
        /// </summary>
        public EntityInfo Error = null;


        /// <summary>
        /// 构造函数
        /// </summary>
        public BookItemBase()
        {
            this.RecordDom.LoadXml("<root />");
        }

        /// <summary>
        /// 将本对象的成员复制给 target 对象
        /// </summary>
        public void CopyTo(BookItemBase target)
        {
            // BookItemBase newObject = new BookItemBase();

            target.ItemDisplayState = this.ItemDisplayState;

            target.RecPath = this.RecPath;
            target.m_bChanged = this.m_bChanged;
            target.OldRecord = this.OldRecord;

            // 放入最新鲜的内容
            target.CurrentRecord = this.RecordDom.OuterXml;


            target.RecordDom = new XmlDocument();
            target.RecordDom.LoadXml(this.RecordDom.OuterXml);

            target.Timestamp = ByteArray.GetCopy(this.Timestamp);
            target.ListViewItem = null;  // this.ListViewItem;
            target.Error = null; // this.Error;

            // return newObject;
        }

        // 
        /// <summary>
        /// 设置 XML 数据
        /// </summary>
        /// <param name="strRecPath">记录路径</param>
        /// <param name="strXml">XML 字符串</param>
        /// <param name="baTimeStamp">时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
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
        /// 重新设置 XML 数据
        /// </summary>
        /// <param name="strRecPath">记录路径</param>
        /// <param name="strNewXml">要设置的 XML 字符串</param>
        /// <param name="baTimeStamp">时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
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
        /// 创建好适合于保存的记录 XML 字符串
        /// </summary>
        /// <param name="bVerifyParent">是否要验证 parent 成员</param>
        /// <param name="strXml">返回 XML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int BuildRecord(
            bool bVerifyParent,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (bVerifyParent == true)
            {
                if (string.IsNullOrEmpty(this.Parent) == true)
                {
                    strError = "Parent 成员尚未定义";
                    return -1;
                }
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
        /// 将本事项加入到 ListView 中
        /// </summary>
        /// <param name="list">ListView</param>
        /// <returns>刚加入的 ListViewItem</returns>
        public ListViewItem AddToListView(ListView list)
        {
            ListViewItem item = new ListViewItem();
            item.ImageIndex = 0;

            // 2013/1/18
            SetItemColumns(item);

            this.SetItemBackColor(item);

            list.Items.Add(item);

            this.ListViewItem = item;

            this.ListViewItem.Tag = this;   // 将BookItem对象引用保存在ListViewItem事项中

            return item;
        }

        /// <summary>
        /// 从 ListView 中删除当前事项
        /// </summary>
        public void DeleteFromListView()
        {
            if (this.ListViewItem != null)
            {
                ListView list = this.ListViewItem.ListView;
                if (list != null)
                    list.Items.Remove(this.ListViewItem);
            }
        }

        /// <summary>
        /// 刷新背景颜色和图标
        /// </summary>
        /// <param name="item">ListViewItem</param>
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

        /// <summary>
        /// 将内存值更新到显示的栏目
        /// 需要重载
        /// </summary>
        /// <param name="item">ListViewItem事项，ListView中的一行</param>
        public virtual void SetItemColumns(ListViewItem item)
        {
            throw new Exception("尚未重载 SetItemColumns()");
        }


        // 
        /// <summary>
        /// 刷新各列内容和图标、背景颜色
        /// </summary>
        public void RefreshListView()
        {
            if (this.ListViewItem == null)
                return;

            ListViewItem item = this.ListViewItem;

            SetItemColumns(item);

            this.SetItemBackColor(item);
        }

        // parameters:
        //      bClearOtherHilight  是否清除其余存在的高亮标记？
        /// <summary>
        /// 在 ListView 中加亮显示本事项
        /// </summary>
        /// <param name="bClearOtherHilight">是否清除其余事项的高亮状态？</param>
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

        /// <summary>
        /// 获得一个新的参考 ID 字符串
        /// </summary>
        /// <returns>参考 ID 字符串</returns>
        public static string GenRefID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 参考 ID
        /// </summary>
        public string RefID
        {
            /*
            get
            {
                // TODO: 如何刷新显示?
                if (String.IsNullOrEmpty(this.m_strRefID) == true)
                    this.m_strRefID = Guid.NewGuid().ToString();

                return this.m_strRefID;
            }*/
            get
            {
                string strRefID = DomUtil.GetElementText(this.RecordDom.DocumentElement, "refID");

                /*
                if (String.IsNullOrEmpty(m_strRefID) == true)
                {
                    strRefID = Guid.NewGuid().ToString();
                    DomUtil.SetElementText(this.RecordDom.DocumentElement, "refID", strRefID);
                }
                 * */

                return strRefID;
            }
            set
            {
                DomUtil.SetElementText(this.RecordDom.DocumentElement, "refID", value);
                this.Changed = true; // 2009/3/5
            }
        }

        /// <summary>
        /// 当前事项所从属的书目记录 ID
        /// </summary>
        public string Parent
        {
            get
            {
                return DomUtil.GetElementText(this.RecordDom.DocumentElement, "parent");
            }
            set
            {
                string oldvalue = DomUtil.GetElementText(this.RecordDom.DocumentElement, "parent");
                if (oldvalue != value)
                {
                    DomUtil.SetElementText(this.RecordDom.DocumentElement, "parent", value);
                    this.Changed = true; // 2009/3/5
                }
            }
        }
    }

    /// <summary>
    /// BookItemBase 的集合容器
    /// </summary>
    [Serializable()]
    public class BookItemCollectionBase : List<BookItemBase>
    {
        // 
        /// <summary>
        /// 把事项重新全部加入 ListView
        /// </summary>
        /// <param name="list">ListView 对象</param>
        public void AddToListView(ListView list)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].AddToListView(list);
            }
        }

        /// <summary>
        /// 清除全部事项。也要从 ListView 中清除
        /// </summary>
        public new void Clear()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].DeleteFromListView();
            }

            base.Clear();
        }

        // 设置全部bookitem事项的Parent域
        /// <summary>
        /// 设置全部事项的 Parent 值
        /// </summary>
        /// <param name="strParentID">ParentID 值</param>
        public void SetParentID(string strParentID)
        {
            foreach (BookItemBase item in this)
            {
                if (item.Parent != strParentID) // 避免连带无谓地修改item.Changed 2009/3/6
                    item.Parent = strParentID;
            }
        }

        /// <summary>
        /// 设置全部事项的参考 ID 值
        /// </summary>
        public void SetRefID()
        {
            foreach (BookItemBase item in this)
            {
                if (String.IsNullOrEmpty(item.RefID) == true)
                    item.RefID = Guid.NewGuid().ToString();
            }
        }

        // return:
        //      -1  有错误，不适合保存
        //      0   没有错误
        /// <summary>
        /// 检查全部事项的 Parent 值是否适合保存
        /// </summary>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 有错误，不适合保存; 0: 没有错误</returns>
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
                    strError = "册事项中出现了空的 ParentID 值";
                    return -1;
                }

                if (strID == "?")
                {
                    strError = "册事项中出现了 '?' 式的 ParentID 值";
                    return -1;
                }
            }

            return 0;
        }

        // return:
        //      -1  有错误，不适合保存
        //      0   没有错误
        /// <summary>
        /// 检查全部事项的参考 ID 值是否适合保存
        /// </summary>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 有错误，不适合保存; 0: 没有错误</returns>
        public int CheckRefIDForSave(out string strError)
        {
            strError = "";
            // 检查每个事项的refID
            Hashtable table = new Hashtable();
            foreach (BookItemBase item in this)
            {
                if (String.IsNullOrEmpty(item.RefID) == true)
                {
                    /*
                    strError = "册事项中出现了空的RefID值";
                    return -1;
                     * */
                    continue;
                }

                if (item.ItemDisplayState != ItemDisplayState.Deleted)  // 删除的可以例外
                {
                    if (table.Contains(item.RefID) == true)
                    {
                        strError = "册事项中出现了重复的参考ID值 '" + item.RefID + "'";
                        return -1;
                    }
                }
                else
                    continue;

                if (table.Contains(item.RefID) == false)
                    table.Add(item.RefID, null);
            }

            return 0;
        }

        // 2008/11/28 
        /// <summary>
        /// 获得父记录 ID 的字符串集合
        /// </summary>
        /// <returns>字符串集合</returns>
        public List<string> GetParentIDs()
        {
            List<string> results = new List<string>();

            foreach (BookItemBase item in this)
            {
                string strParentID = item.Parent;
                if (results.IndexOf(strParentID) == -1)
                    results.Add(strParentID);
            }

            return results;
        }


        /// <summary>
        /// 以参考 ID 定位一个事项
        /// </summary>
        /// <param name="strRefID">参考 ID</param>
        /// <param name="excludeItems">要排除的事项的集合</param>
        /// <returns>事项</returns>
        public BookItemBase GetItemByRefID(string strRefID,
            List<BookItemBase> excludeItems = null)
        {
            foreach (BookItemBase item in this)
            {
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

        /// <summary>
        /// 以记录路径定位一个事项
        /// </summary>
        /// <param name="strRecPath">记录路径</param>
        /// <returns>事项</returns>
        public BookItemBase GetItemByRecPath(string strRecPath)
        {
            foreach (BookItemBase item in this)
            {
                if (item.RecPath == strRecPath)
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

                foreach (BookItemBase item in this)
                {
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
                    foreach (BookItemBase item in this)
                    {
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
        /// 标记删除
        /// </summary>
        /// <param name="bRemoveFromList">是否同时从列表中清除?</param>
        /// <param name="bookitem">要操作的事项</param>
        public void MaskDeleteItem(
            bool bRemoveFromList,
            BookItemBase bookitem)
        {
            if (bookitem.ItemDisplayState == ItemDisplayState.New)
            {
                PhysicalDeleteItem(bookitem);
                return;
            }

            bookitem.ItemDisplayState = ItemDisplayState.Deleted;
            bookitem.Changed = true;

            bookitem.Error = null;
            bookitem.ErrorInfo = "";    // 2015/2/8

            // 从listview中消失?
            if (bRemoveFromList == true)
                bookitem.DeleteFromListView();
            else
            {
                bookitem.RefreshListView();
            }
        }

        // 
        // return:
        //      false   没有必要Undo
        //      true    已经Undo
        /// <summary>
        /// Undo标记删除
        /// </summary>
        /// <param name="bookitem">要操作的事项</param>
        /// <returns>false: 没有必要 Undo; true: 成功 Undo</returns>
        public bool UndoMaskDeleteItem(BookItemBase bookitem)
        {
            if (bookitem.ItemDisplayState != ItemDisplayState.Deleted)
                return false;   // 要Undo的事项根本就不是Deleted状态，所以谈不上Undo

            // 因为不知道上次标记删除前数据是否改过，因此权当改过
            bookitem.ItemDisplayState = ItemDisplayState.Changed;
            bookitem.Changed = true;

            // 刷新
            bookitem.RefreshListView();
            return true;
        }

        // 
        /// <summary>
        /// 从集合中和视觉上同时删除
        /// </summary>
        /// <param name="bookitem">要删除的事项</param>
        public void PhysicalDeleteItem(
            BookItemBase bookitem)
        {
            // 从listview中消失
            bookitem.DeleteFromListView();

            this.Remove(bookitem);
        }

        /// <summary>
        /// 清除全部事项的加亮状态
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

        // 2008/11/4
        // parameters:
        //      bClearOthersHilight 是否要清除其他事项的加亮?
        /// <summary>
        /// 加亮指定的事项
        /// </summary>
        /// <param name="items">要加亮的事项的集合</param>
        /// <param name="bClearOthersHilight">是否要清除其余事项的加亮状态?</param>
        public void HilightItems(List<BookItemBase> items,
            bool bClearOthersHilight)
        {
            if (this.Count == 0)
                return;

            ListView list = this[0].ListViewItem.ListView;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem listview_item = list.Items[i];

                if (items.IndexOf((BookItemBase)listview_item.Tag) != -1)
                    listview_item.Selected = true;
                else
                {
                    if (bClearOthersHilight == true)
                        listview_item.Selected = false;
                }
            }
        }
    }
}
