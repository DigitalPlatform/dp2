using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.RFID;

namespace CallRfidCenterSample
{
    public partial class TagListForm : Form
    {
        public TagListForm()
        {
            InitializeComponent();
        }

        private void TagListForm_Load(object sender, EventArgs e)
        {
            Form1.TagChanged += Form1_TagChanged;

            this.Invoke((Action)(() =>
            {
                foreach (var tag in TagList.Books)
                {
                    UpdateItem(tag);
                }

                foreach (var tag in TagList.Patrons)
                {
                    UpdateItem(tag);
                }
            }));
        }

        private void Form1_TagChanged(object sender, TagChangedEventArgs e)
        {
            /*
            // 方法1：用 e 中的增删改集合动态修改 ListView
            this.Invoke((Action)(() =>
            {
                if (e.AddBooks != null)
                    foreach (var tag in e.AddBooks)
                    {
                        AddItem(tag);
                    }

                if (e.UpdateBooks != null)
                    foreach (var tag in e.UpdateBooks)
                    {
                        UpdateItem(tag);
                    }

                if (e.RemoveBooks != null)
                    foreach (var tag in e.RemoveBooks)
                    {
                        RemoveItem(tag);
                    }

                if (e.AddPatrons != null)
                    foreach (var tag in e.AddPatrons)
                    {
                        AddItem(tag);
                    }

                if (e.UpdatePatrons != null)
                    foreach (var tag in e.UpdatePatrons)
                    {
                        UpdateItem(tag);
                    }

                if (e.RemovePatrons != null)
                    foreach (var tag in e.RemovePatrons)
                    {
                        RemoveItem(tag);
                    }
            }));
            */

            // 方法2：每次都遍历 TagList.Books Patrons 集合
            this.Invoke((Action)(() =>
            {
                List<TagAndData> tags = new List<TagAndData>();
                tags.AddRange(TagList.Books);
                tags.AddRange(TagList.Patrons);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (var tag in tags)
                {
                    var item = UpdateItem(tag);
                    items.Add(item);
                }

                // 删除掉 tags 中没有包含的 ListViewItem
                List<ListViewItem> delete_items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    if (items.IndexOf(item) == -1)
                        delete_items.Add(item);
                }

                foreach(ListViewItem item in delete_items)
                {
                    this.listView_tags.Items.Remove(item);
                }
            }));
        }

        private void TagListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.TagChanged -= Form1_TagChanged;
        }

        const int COLUMN_UID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_PROTOCOL = 2;
        const int COLUMN_ANTENNA = 3;

        // 添加一个新行
        ListViewItem AddItem(TagAndData tag)
        {
            ListViewItem item = new ListViewItem();
            this.listView_tags.Items.Add(item);
            RefreshItem(item, tag);
            return item;
        }

        void RefreshItem(ListViewItem item, TagAndData tag)
        {
            string pii = "";

            var taginfo = tag.OneTag.TagInfo;
            if (taginfo != null)
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                var chip = LogicChip.From(taginfo.Bytes,
    (int)taginfo.BlockSize,
    "");
                pii = chip.FindElement(ElementOID.PII)?.Text;
            }

            ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.OneTag.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
            ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.OneTag.Protocol);
            ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.OneTag.AntennaID.ToString());
        }

        // 移走一行
        void RemoveItem(TagAndData tag)
        {
            var item = ListViewUtil.FindItem(this.listView_tags, tag.OneTag.UID, COLUMN_UID);
            if (item == null)
                return;
            this.listView_tags.Items.Remove(item);
        }

        // 更新一行内容
        ListViewItem UpdateItem(TagAndData tag)
        {
            var item = ListViewUtil.FindItem(this.listView_tags, tag.OneTag.UID, COLUMN_UID);
            if (item == null)
            {
                return AddItem(tag);
            }
            RefreshItem(item, tag);
            return item;
        }
    }
}
