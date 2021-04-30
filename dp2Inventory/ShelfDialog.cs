using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace dp2Inventory
{
    public partial class ShelfDialog : Form
    {
        // shelfNo --> List<BookInfo>
        static Hashtable _shelfTable = new Hashtable();

        // PII --> BookInfo
        static Hashtable _bookTable = new Hashtable();

        public static void ClearTable()
        {
            _shelfTable.Clear();
            _bookTable.Clear();
        }

        public ShelfDialog()
        {
            InitializeComponent();

            // 要确保有序
            this.listView_shelfList.ListViewItemSorter = new ListViewItemComparer();
        }

        // parameters:
        //      action  "newShelf" 新增了书架
        //              "updateCount"   需要刷新左侧书架名对应的图书数
        //              "updateList"    需要更新指定书架的图书列表
        public delegate void delegate_refreshUI(string action,
            string shelfNo,
            List<BookInfo> list);

        public static void AddBookEntry(BookInfo info,
            delegate_refreshUI refreshUI)
        {
            string currentShelfNo = info.CurrentLocation;
            string uii = info.UII;

            /*
            string currentLocation = null;
            string currentShelfNo = null;

            if (currentLocationString != null)
            {
                // 分解 currentLocation 字符串
                var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                currentLocation = parts[0];
                currentShelfNo = parts[1];
            }
            */

            // 定位 list
            List<BookInfo> list = null;
            if (_shelfTable.ContainsKey(currentShelfNo))
            {
                list = (List<BookInfo>)_shelfTable[currentShelfNo];
            }
            else
            {
                list = new List<BookInfo>();
                _shelfTable[currentShelfNo] = list;

                // TODO: 触发添加书架视觉事项
                refreshUI?.Invoke("newShelf", currentShelfNo, list);
                /*
                // 在左侧添加一行
                ListViewItem shelf_item = new ListViewItem();
                ListViewUtil.ChangeItemText(shelf_item, 0, currentShelfNo);
                ListViewUtil.ChangeItemText(shelf_item, 1, "0");

                this.listView_shelfList.Items.Add(shelf_item);
                */
            }

            // 加入 entry
            BookInfo exist_info = list.Find(o => o.UII == uii);
            if (exist_info == null)
            {
                list.Add(info);

                // TODO: 触发 更新左侧的 count 列数字
                // UpdateCount(currentShelfNo, list.Count);
                refreshUI?.Invoke("updateCount", currentShelfNo, list);

                // 检测此前是否在其它 shelfNo 名下有同 PII 的事项
                BookInfo same_name_info = (BookInfo)_bookTable[uii];
                if (same_name_info != null && same_name_info.CurrentLocation != currentShelfNo)
                {
                    // 删除此前的 BookInfo
                    // RemoveBookInfo(old_info.CurrentLocation);
                    List<BookInfo> old_list = (List<BookInfo>)_shelfTable[same_name_info.CurrentLocation];
                    if (old_list != null)
                    {
                        old_list.Remove(same_name_info);
                        refreshUI?.Invoke("updateCount", same_name_info.CurrentLocation, old_list);
                        refreshUI?.Invoke("updateList", same_name_info.CurrentLocation, old_list);
                    }
                }

                // 查找表中替换为新的值
                _bookTable[uii] = info;
            }
            else
            {
                // 替换掉
                list.Remove(exist_info);
                list.Add(info);
            }

            // TODO: 如果需要，触发 更新右侧
            // UpdateBookList();
            refreshUI?.Invoke("updateList", currentShelfNo, list);
        }

#if REMOVED
        public static void AddBook(AddBookEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (this.listView_books.Columns.Count == 0)
                {
                    for (int i = 0; i < e.Columns.Count; i++)
                    {
                        this.listView_books.Columns.Add(e.Columns[i].Clone() as ColumnHeader);
                    }
                }

                var new_item = e.Item.Clone() as ListViewItem;
                AddBookEntry(new_item);

                // this.listView_books.Items.Add(new_item);
            }));
        }

        void AddBookEntry(ListViewItem new_item)
        {
            string currentLocationString = ListViewUtil.GetItemText(new_item, InventoryDialog.COLUMN_CURRENTLOCATION);
            string pii = ListViewUtil.GetItemText(new_item, InventoryDialog.COLUMN_PII);

            string currentLocation = null;
            string currentShelfNo = null;

            if (currentLocationString != null)
            {
                // 分解 currentLocation 字符串
                var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                currentLocation = parts[0];
                currentShelfNo = parts[1];
            }

            // 定位 list
            List<BookEntry> list = null;
            if (_shelfTable.ContainsKey(currentShelfNo))
            {
                list = (List<BookEntry>)_shelfTable[currentShelfNo];
            }
            else
            {
                list = new List<BookEntry>();
                _shelfTable[currentShelfNo] = list;

                // 在左侧添加一行
                ListViewItem shelf_item = new ListViewItem();
                ListViewUtil.ChangeItemText(shelf_item, 0, currentShelfNo);
                ListViewUtil.ChangeItemText(shelf_item, 1, "0");

                this.listView_shelfList.Items.Add(shelf_item);
            }

            // 加入 entry
            BookEntry entry = list.Find(o => o.PII == pii);
            if (entry == null)
            {
                entry = new BookEntry
                {
                    Item = new_item,
                    PII = pii
                };
                list.Add(entry);

                // 更新左侧的 count 列数字
                UpdateCount(currentShelfNo, list.Count);
            }
            else
            {
                entry.Item = new_item;  // 替换掉 ListViewItem
            }

            // 如果需要，更新右侧
            UpdateBookList();
        }
#endif

        // Implements the manual sorting of items by columns.
        class ListViewItemComparer : IComparer
        {
            public ListViewItemComparer()
            {
            }

            public int Compare(object x, object y)
            {
                return String.Compare(((ListViewItem)x).SubItems[0].Text, ((ListViewItem)y).SubItems[0].Text);
            }
        }

        // 首次显示，把 _shelfTable 中积累的内容全部显示到界面上
        public void DisplayContent()
        {
            this.Invoke((Action)(() =>
            {
                this.listView_shelfList.Items.Clear();
                this.listView_books.Clear();

                this.listView_shelfList.BeginUpdate();
                foreach (string shelfNo in _shelfTable.Keys)
                {
                    List<BookInfo> list = (List<BookInfo>)_shelfTable[shelfNo];

                    // 在左侧添加一行
                    ListViewItem shelf_item = new ListViewItem();
                    ListViewUtil.ChangeItemText(shelf_item, 0, shelfNo);
                    ListViewUtil.ChangeItemText(shelf_item, 1, list.Count.ToString());

                    this.listView_shelfList.Items.Add(shelf_item);
                }
                this.listView_shelfList.EndUpdate();
            }));
        }

        public void RefreshUI(string action,
            string shelfNo,
            List<BookInfo> list)
        {
            this.Invoke((Action)(() =>
            {
                if (action == "newShelf")
                {
                    // 在左侧添加一行
                    ListViewItem shelf_item = new ListViewItem();
                    ListViewUtil.ChangeItemText(shelf_item, 0, shelfNo);
                    ListViewUtil.ChangeItemText(shelf_item, 1, "0");

                    this.listView_shelfList.Items.Add(shelf_item);
                    return;
                }

                if (action == "updateCount")
                {
                    // 更新左侧的 count 列数字
                    UpdateCount(shelfNo, list.Count);
                    return;
                }

                if (action == "updateList")
                {
                    // TODO: 如果当前选择的书架是 shelfNo 的话才需要进行刷新
                    if (this.listView_shelfList.SelectedItems.Count == 1)
                    {
                        var shelfItem = this.listView_shelfList.SelectedItems[0];
                        string selectedShelfNo = ListViewUtil.GetItemText(shelfItem, 0);

                        if (selectedShelfNo == shelfNo)
                            UpdateBookList();
                    }
                }
            }));
        }

        // 更新左侧的 count 列数字
        void UpdateCount(string shelfNo, int count)
        {
            // 找到左侧 ListViewItem
            foreach (ListViewItem item in this.listView_shelfList.Items)
            {
                string currentShelfNo = ListViewUtil.GetItemText(item, 0);
                if (currentShelfNo == shelfNo)
                {
                    ListViewUtil.ChangeItemText(item, 1, count.ToString());
                    return;
                }
            }
        }

        void UpdateBookList()
        {
            if (this.listView_shelfList.SelectedItems.Count == 1)
            {
                var shelfItem = this.listView_shelfList.SelectedItems[0];
                string shelfNo = ListViewUtil.GetItemText(shelfItem, 0);

                List<BookInfo> list = (List<BookInfo>)_shelfTable[shelfNo];
                if (list == null)
                    return;

                this.listView_books.BeginUpdate();
                try
                {
                    this.listView_books.Items.Clear();
                    foreach (var entry in list)
                    {
                        this.listView_books.Items.Add(NewItem(entry));
                    }
                }
                finally
                {
                    this.listView_books.EndUpdate();
                }
            }
            else
                this.listView_books.Items.Clear();
        }

        ListViewItem NewItem(BookInfo info)
        {
            ListViewItem item = new ListViewItem();
            UpdateItem(item, info);
            return item;
        }

        const int COLUMN_PII = 0;
        const int COLUMN_OI = 1;
        const int COLUMN_TITLE = 2;
        const int COLUMN_STATE = 3;
        const int COLUMN_CURRENTLOCATION = 4;
        const int COLUMN_LOCATION = 5;
        const int COLUMN_ACCESSNO = 6;

        void UpdateItem(ListViewItem item, BookInfo info)
        {
            DataModel.ParseOiPii(info.UII, out string pii, out string oi);

            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
            ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
            ListViewUtil.ChangeItemText(item, COLUMN_TITLE, info.Title);
            ListViewUtil.ChangeItemText(item, COLUMN_STATE, info.State);
            ListViewUtil.ChangeItemText(item, COLUMN_CURRENTLOCATION, info.CurrentLocation);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, info.Location);
            ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, info.AccessNo);
        }

        private void listView_shelfList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBookList();
        }

        // 清除全部显示内容
        private void toolStripButton_clearAll_Click(object sender, EventArgs e)
        {
            ClearTable();
            DisplayContent();
        }
    }

    /*
    public class BookEntry
    {
        public string PII { get; set; }
        public ListViewItem Item { get; set; }
    }
    */

    public class BookInfo
    {
        public string UII { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string Location { get; set; }
        public string CurrentLocation { get; set; }
        public string AccessNo { get; set; }

    }

}
