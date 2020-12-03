using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.RFID;

namespace RfidTool
{
    public partial class ScanDialog : Form
    {
        // 当前正在寻求处理的册条码号
        string _currentBarcode = "";


        public ScanDialog()
        {
            InitializeComponent();
        }

        private void textBox_barcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                // MessageBox.Show(this, $"输入 '{this.textBox_barcode.Text}'");
                _currentBarcode = this.textBox_barcode.Text;
                this.textBox_barcode.SelectAll();
                e.Handled = true;

                // 触发处理
            }
        }

        private void ScanDialog_Load(object sender, EventArgs e)
        {
            // 首次填充标签
            FillAllTags();
        }

        private void ScanDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void ScanDialog_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                DataModel.TagChanged += DataModel_TagChanged;
            else
                DataModel.TagChanged -= DataModel_TagChanged;
        }

        // 读卡器上的标签发生变化
        private void DataModel_TagChanged(object sender, NewTagChangedEventArgs e)
        {
            if (e.AddTags != null && e.AddTags.Count > 0)
            {
                this.Invoke((Action)(() =>
                {
                    UpdateTags(e.AddTags);
                }));
            }

            if (e.UpdateTags != null && e.UpdateTags.Count > 0)
            {
                this.Invoke((Action)(() =>
                {
                    UpdateTags(e.UpdateTags);
                }));
            }

            if (e.RemoveTags != null && e.RemoveTags.Count > 0)
            {
                this.Invoke((Action)(() =>
                {
                    RemoveTags(e.RemoveTags);
                }));
            }
        }

        const int COLUMN_UID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_TOU = 2;
        const int COLUMN_OI = 3;
        const int COLUMN_ANTENNA = 4;
        const int COLUMN_READERNAME = 5;

        // TODO: 注意和 DataModel_TagChanged() 处理互斥
        void FillAllTags()
        {
            this.listView_tags.Items.Clear();
            foreach (var tag in DataModel.TagList.Tags)
            {
                if (tag.OneTag.Protocol == InventoryInfo.ISO14443A)
                    continue;

                ListViewItem item = new ListViewItem();
                item.Tag = new ItemInfo { TagData = tag };
                this.listView_tags.Items.Add(item);
                RefreshItem(item, tag);
            }
        }

        // 更新 tags
        void UpdateTags(List<TagAndData> tags)
        {
            foreach (var tag in tags)
            {
                ListViewItem item = ListViewUtil.FindItem(this.listView_tags, tag.OneTag.UID, COLUMN_UID);
                if (item == null)
                {
                    item = new ListViewItem();
                    item.Tag = new ItemInfo { TagData = tag };
                    this.listView_tags.Items.Add(item);
                }
                RefreshItem(item, tag);
            }
        }

        void RemoveTags(List<TagAndData> tags)
        {
            foreach (var tag in tags)
            {
                ListViewItem item = ListViewUtil.FindItem(this.listView_tags, tag.OneTag.UID, COLUMN_UID);
                if (item != null)
                {
                    this.listView_tags.Items.Remove(item);
                }
            }
        }

        // 刷新一个 ListViewItem 的所有列显示
        void RefreshItem(ListViewItem item, TagAndData tag)
        {
            string pii = "";
            string tou = "";
            string oi = "";

            ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.OneTag.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.OneTag.AntennaID.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.OneTag.ReaderName);

            var taginfo = tag.OneTag.TagInfo;
            if (taginfo != null)
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                var chip = LogicChip.From(taginfo.Bytes,
    (int)taginfo.BlockSize,
    "");
                pii = chip.FindElement(ElementOID.PII)?.Text;
                tou = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                oi = chip.FindElement(ElementOID.OI)?.Text;
            }


            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
            ListViewUtil.ChangeItemText(item, COLUMN_TOU, tou);
            ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
        }

        class FindTagResult : NormalResult
        {
            public ListViewItem Item { get; set; }
            public OneTag Tag { get; set; }
        }

        // 寻找一个可用于写入的空白标签，或者相同 PII 的标签
        FindTagResult FindBlankTag(string pii)
        {
            FindTagResult blank_result = null;
            FindTagResult pii_result = null;

            foreach (ListViewItem item in this.listView_tags.Items)
            {
                if (item.Tag is not ItemInfo info)
                    continue;

                if (string.IsNullOrEmpty(info.TagData.Error) == false)
                    continue;

                string current_pii = ListViewUtil.GetItemText(item, COLUMN_PII);
                if (current_pii == pii)
                {
                    pii_result = new FindTagResult
                    {
                        Value = 1,
                        Item = item,
                        Tag = info.TagData.OneTag,
                    };
                }
                else if (string.IsNullOrEmpty(current_pii) == true
                    && info.TagData.OneTag.TagInfo != null)
                {
                    blank_result = new FindTagResult
                    {
                        Value = 1,
                        Item = item,
                        Tag = info.TagData.OneTag,
                    };
                }
            }

            // 优先返回 PII 匹配的行
            if (pii_result != null)
                return pii_result;
            // 次优先返回 PII 为空的行
            if (blank_result != null)
                return blank_result;

            // 没有找到
            return new FindTagResult { Value = 0 };
        }

        // 寻找适当的 RFID 标签完成写入操作
        void ProcessBarcode()
        {
            var find_result = FindBlankTag(_currentBarcode);
            if (find_result.Value == 0)
                return;

            var uid = find_result.Tag.UID;
            var tag = find_result.Tag;

            var chip = new LogicChip();
            chip.SetElement(ElementOID.PII, _currentBarcode);
            chip.SetElement(ElementOID.OI, "");
            chip.SetElement(ElementOID.TypeOfUsage, "");

            TagInfo new_tag_info = GetTagInfo(tag.TagInfo, chip);

            var write_result = DataModel.WriteTagInfo(tag.ReaderName, tag.TagInfo,
                new_tag_info);

            if (write_result.Value == -1)
            {
                ShowMessageBox(write_result.ErrorInfo);
                return;
            }

            // TODO: 语音提示写入成功

            ClearBarcode();
        }

        void ShowMessageBox(string text)
        {
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, text);
            }));

            // TODO: 语音提示出错
        }

        void ClearBarcode()
        {
            this.Invoke((Action)(() =>
            {
                _currentBarcode = "";
                this.textBox_barcode.Text = "";
                this.textBox_barcode.Focus();
            }));
        }

        public static TagInfo GetTagInfo(TagInfo existing,
    LogicChip chip)
        {
            TagInfo new_tag_info = existing.Clone();
            new_tag_info.Bytes = chip.GetBytes(
                (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                (int)new_tag_info.BlockSize,
                LogicChip.GetBytesStyle.None,
                out string block_map);
            new_tag_info.LockStatus = block_map;

            // new_tag_info.DSFID = chip.DSFID;

            // 上架状态
            new_tag_info.AFI = 0x07;
            new_tag_info.EAS = true;

            return new_tag_info;
        }

    }

    public class ItemInfo
    {
        public TagAndData TagData { get; set; }
    }
}
