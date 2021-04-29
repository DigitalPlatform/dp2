using DigitalPlatform.GUI;
using DigitalPlatform.Text;
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

namespace dp2Inventory
{
    public partial class ShelfDialog : Form
    {
        // shelfNo --> List<BookEntry>
        Hashtable _shelfTable = new Hashtable();

        public ShelfDialog()
        {
            InitializeComponent();

            // 要确保有序
            this.listView_shelfList.ListViewItemSorter = new ListViewItemComparer();
        }

        public void AddBook(AddBookEventArgs e)
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

        // 更新左侧的 count 列数字
        void UpdateCount(string shelfNo, int count)
        {
            // 找到左侧 ListViewItem
            foreach(ListViewItem item in this.listView_shelfList.Items)
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

                List<BookEntry> list = (List<BookEntry>)_shelfTable[shelfNo];
                if (list == null)
                    return;

                this.listView_books.BeginUpdate();
                try
                {
                    this.listView_books.Items.Clear();
                    foreach (var entry in list)
                    {
                        this.listView_books.Items.Add(entry.Item);
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

        private void listView_shelfList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBookList();
        }
    }

    public class BookEntry
    {
        public string PII { get; set; }
        public ListViewItem Item { get; set; }
    }


}
