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
                foreach (var tag in Form1.TagList.Tags)
                {
                    UpdateItem(tag);
                }
            }));
        }

        private void Form1_TagChanged(object sender, TagChangedEventArgs e)
        {
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
            }));

            /*
            // 方法2：每次都遍历 TagList.Books Patrons 集合
            this.Invoke((Action)(() =>
            {
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (var tag in Form1.TagList.Tags)
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
            */
        }

        private void TagListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.TagChanged -= Form1_TagChanged;
        }

        const int COLUMN_UID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_TU = 2;
        const int COLUMN_OI = 3;
        const int COLUMN_PROTOCOL = 4;
        const int COLUMN_ANTENNA = 5;
        const int COLUMN_READERNAME = 6;

        // 添加一个新行
        ListViewItem AddItem(TagAndData tag)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = tag;
            this.listView_tags.Items.Add(item);
            RefreshItem(item, tag);
            return item;
        }

        void RefreshItem(ListViewItem item, TagAndData tag)
        {
            item.Tag = tag;

            ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.OneTag.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.OneTag.Protocol);
            ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.OneTag.AntennaID.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.OneTag.ReaderName);

            string pii = "";
            string tu = "";
            string oi = "";
            var taginfo = tag.OneTag.TagInfo;
            if (taginfo != null)
            {
                LogicChip chip = null;

                if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                {
                    var parse_result = GaoxiaoUtility.ParseTag(
        Element.FromHexString(taginfo.UID),
        taginfo.Bytes);
                    if (parse_result.Value == -1)
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + parse_result.ErrorInfo);
                        return;
                    }
                    chip = parse_result.LogicChip;
                    taginfo.EAS = !parse_result.EpcInfo.Lending;
                }
                else if (taginfo.Protocol == InventoryInfo.ISO15693)
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    chip = LogicChip.From(taginfo.Bytes,
        (int)taginfo.BlockSize,
        "");
                }

                if (chip != null)
                {
                    pii = chip.FindElement(ElementOID.PII)?.Text;
                    tu = chip.FindElement(ElementOID.TU)?.Text;
                    oi = chip.FindElement(ElementOID.OI)?.Text;
                }
            }

            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
            ListViewUtil.ChangeItemText(item, COLUMN_TU, tu);
            ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
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

        // 测试写入高校联盟格式
        private void button_writeGaoxiao_Click(object sender, EventArgs e)
        {
            string strError;
            if (this.listView_tags.SelectedItems.Count != 1)
            {
                strError = "请先从列表中选择一个要写入的标签行";
                goto ERROR1;
            }
            var item = this.listView_tags.SelectedItems[0];
            TagAndData data = item.Tag as TagAndData;

            var chip = new LogicChip();
            // 这里是要写入的字段
            chip.SetElement(ElementOID.PII, "0000002");
            chip.SetElement(ElementOID.OI, "50000");
            chip.SetElement(ElementOID.TypeOfUsage, "1.0", false/* 表示不校验这个值 */);

            bool eas = true;    // 防盗标志位
            var tag = data.OneTag;
            TagInfo new_tag_info = GetTagInfo(tag.TagInfo, chip, eas);

            var write_result = RfidManager.WriteTagInfo(tag.ReaderName,
                tag.TagInfo,
                new_tag_info);

            if (write_result.Value == -1)
            {
                strError = write_result.ErrorInfo;
                goto ERROR1;
            }

            // 清掉标签缓冲，迫使刷新列表显示
            Form1.TagList.ClearTagTable(tag.UID);
            MessageBox.Show(this, "写入成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public static TagInfo GetTagInfo(TagInfo existing,
LogicChip chip,
bool eas)
        {
            if (existing.Protocol == InventoryInfo.ISO15693)
            {
                TagInfo new_tag_info = existing.Clone();
                new_tag_info.Bytes = chip.GetBytes(
                    (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                    (int)new_tag_info.BlockSize,
                    LogicChip.GetBytesStyle.None,
                    out string block_map);
                new_tag_info.LockStatus = block_map;

                new_tag_info.DSFID = LogicChip.DefaultDSFID;  // 图书

                // 上架状态
                new_tag_info.SetEas(eas);
                return new_tag_info;
            }

            if (existing.Protocol == InventoryInfo.ISO18000P6C)
            {
                var result = GaoxiaoUtility.BuildTag(chip, eas);
                TagInfo new_tag_info = existing.Clone();
                new_tag_info.Bytes = result.UserBank;
                new_tag_info.UID = existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                return new_tag_info;
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
        }


        private void listView_tags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_tags.SelectedItems.Count == 0)
            {
                this.button_writeGaoxiao.Enabled = false;
            }
            else
            {
                this.button_writeGaoxiao.Enabled = true;
            }
        }
    }
}
