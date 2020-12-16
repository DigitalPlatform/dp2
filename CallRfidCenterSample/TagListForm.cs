using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

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

            try
            {
                string pii = "";
                string tu = "";
                string oi = "";
                string aoi = "";
                string eas = "";
                string uhfProtocol = "";

                var taginfo = tag.OneTag.TagInfo;
                if (taginfo != null)
                {
                    LogicChip chip = null;

                    if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        var epc_bank = Element.FromHexString(taginfo.UID);
                        var isGB = UhfUtility.IsISO285604Format(epc_bank, taginfo.Bytes);
                        if (isGB)
                        {
                            // *** 国标 UHF
                            var parse_result = UhfUtility.ParseTag(epc_bank,
                taginfo.Bytes,
                4);
                            if (parse_result.Value == -1)
                                throw new Exception(parse_result.ErrorInfo);
                            chip = parse_result.LogicChip;
                            taginfo.EAS = false;    // TODO
                            uhfProtocol = "gb";
                            pii = GetPIICaption(GetPiiPart(parse_result.UII));
                            oi = GetOiPart(parse_result.UII, false);
                        }
                        else
                        {
                            /*
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
                            */
                            // *** 高校联盟 UHF
                            var parse_result = GaoxiaoUtility.ParseTag(
                epc_bank,
                taginfo.Bytes);
                            if (parse_result.Value == -1)
                            {
                                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + parse_result.ErrorInfo);
                                return;
                            }
                            chip = parse_result.LogicChip;
                            taginfo.EAS = !parse_result.EpcInfo.Lending;
                            uhfProtocol = "gxlm";
                            pii = GetPIICaption(GetPiiPart(parse_result.EpcInfo.PII));
                            oi = GetOiPart(parse_result.EpcInfo.PII, false);
                        }
                    }
                    else if (taginfo.Protocol == InventoryInfo.ISO15693)
                    {
                        // *** ISO15693 HF
                        if (taginfo.Bytes != null)
                        {
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            chip = LogicChip.From(taginfo.Bytes,
                (int)taginfo.BlockSize,
                "");
                            pii = GetPIICaption(chip.FindElement(ElementOID.PII)?.Text);
                        }
                    }

                    /*
                    if (chip != null)
                    {
                        pii = chip.FindElement(ElementOID.PII)?.Text;
                        tu = chip.FindElement(ElementOID.TU)?.Text;
                        oi = chip.FindElement(ElementOID.OI)?.Text;
                    }
                    */


                    tu = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;
                    eas = taginfo.EAS ? "On" : "Off";

                    if (string.IsNullOrEmpty(oi))
                    {
                        oi = chip?.FindElement(ElementOID.OI)?.Text;
                        aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                    }

                    if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        if (uhfProtocol == "gxlm")
                        {
                            // 数字平台针对高校联盟扩充的 AOI
                            if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                                aoi = chip?.FindElement((ElementOID)27)?.Text;
                        }
                    }
                }

                ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
                ListViewUtil.ChangeItemText(item, COLUMN_TU, tu);
                //ListViewUtil.ChangeItemText(item, COLUMN_EAS, eas);
                ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
                //ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);

                // 刷新协议栏
                if (tag.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    string name = uhfProtocol;
                    if (uhfProtocol == "gxlm")
                        name = "高校联盟";
                    else if (uhfProtocol == "gb")
                        name = "国标";
                    ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL,
                        string.IsNullOrEmpty(name) ? tag.OneTag.Protocol : tag.OneTag.Protocol + ":" + name);
                }
            }
            catch (Exception ex)
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + ex.Message);
                SetItemColor(item, "error");
            }
        }

        static void SetItemColor(ListViewItem item, string state)
        {
            if (state == "normal")
            {
                if (item.ListView != null)
                {
                    item.BackColor = item.ListView.BackColor;
                    item.ForeColor = item.ListView.ForeColor;
                }
                return;
            }

            if (state == "changed")
            {
                item.BackColor = Color.DarkGreen;
                item.ForeColor = Color.White;
                return;
            }

            if (state == "error")
            {
                item.BackColor = Color.DarkRed;
                item.ForeColor = Color.White;
                return;
            }
        }

        // 获得 oi.pii 的 oi 部分
        public static string GetOiPart(string oi_pii, bool return_null)
        {
            if (oi_pii.IndexOf(".") == -1)
            {
                if (return_null)
                    return null;
                return "";
            }
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[0];
        }

        // 获得 oi.pii 的 pii 部分
        public static string GetPiiPart(string oi_pii)
        {
            if (oi_pii.IndexOf(".") == -1)
                return oi_pii;
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[1];
        }

        static string GetPIICaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";
            return text;
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

        public static TagInfo GetTagInfo(TagInfo existing,
LogicChip chip,
string data_format,
bool build_user_bank,
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
                if (data_format == "gxlm")
                {
                    var result = GaoxiaoUtility.BuildTag(chip, build_user_bank, eas);
#if DEBUG
                    if (build_user_bank)
                        Debug.Assert(result.UserBank != null);
                    else
                        Debug.Assert(result.UserBank == null);
#endif
                    TagInfo new_tag_info = existing.Clone();
                    new_tag_info.Bytes = result.UserBank;
                    new_tag_info.UID = existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                    return new_tag_info;
                }
                else if (data_format == "gb")
                {
                    var result = UhfUtility.BuildTag(chip,
                        true,
                        eas ? "afi_eas_on" : "");
                    TagInfo new_tag_info = existing.Clone();
                    new_tag_info.Bytes = result.UserBank;
                    new_tag_info.UID = existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                    return new_tag_info;
                }
                else
                    throw new ArgumentException($"无法识别的 data_format 值 '{data_format}'");
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
        }


        private void listView_tags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_tags.SelectedItems.Count == 0)
            {
                this.toolStripDropDownButton_writeUhf.Enabled = false;
                /*
                this.ToolStripMenuItem_writeUhf_gaoxiao.Enabled = false;
                this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank.Enabled = false;
                this.button_writeGaoxiao.Enabled = false;
                */
            }
            else
            {
                this.toolStripDropDownButton_writeUhf.Enabled = true;
            }
        }

        // 写入 高校联盟 格式，没有 User Bank
        private void ToolStripMenuItem_writeUhf_gaoxiao_noUserBank_Click(object sender, EventArgs e)
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

            bool eas = true;    // 防盗标志位
            var tag = data.OneTag;
            TagInfo new_tag_info = GetTagInfo(tag.TagInfo,
                chip,
                "gxlm",
                false,
                eas);

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
            MessageBox.Show(this, "写入成功 (高校联盟，无 User Bank)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 写入高校联盟格式，有 User Bank
        private void ToolStripMenuItem_writeUhf_gaoxiao_Click(object sender, EventArgs e)
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
            chip.SetElement(ElementOID.TypeOfUsage, "0.0", false/* 表示不校验这个值 */);    // 类型：图书

            bool eas = true;    // 防盗标志位
            var tag = data.OneTag;
            TagInfo new_tag_info = GetTagInfo(tag.TagInfo,
                chip,
                "gxlm",
                true,
                eas);

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
            MessageBox.Show(this, "写入成功 (高校联盟，有 User Bank)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 写入国标格式，没有 User Bank
        private void ToolStripMenuItem_writeUhf_gb_noUserBank_Click(object sender, EventArgs e)
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
            chip.SetElement(ElementOID.OI, "CN-110108-1-NLC");

            bool eas = true;    // 防盗标志位
            var tag = data.OneTag;
            TagInfo new_tag_info = GetTagInfo(tag.TagInfo,
                chip,
                "gb",
                false,
                eas);

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
            MessageBox.Show(this, "写入成功 (国标，无 User Bank)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 写入国标格式，有 User Bank
        private void ToolStripMenuItem_writeUhf_gb_Click(object sender, EventArgs e)
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
            chip.SetElement(ElementOID.OI, "CN-110108-1-NLC");
            chip.SetElement(ElementOID.TypeOfUsage, "10");  // 类型为 图书标签

            bool eas = true;    // 防盗标志位
            var tag = data.OneTag;
            TagInfo new_tag_info = GetTagInfo(tag.TagInfo,
                chip,
                "gb",
                true,
                eas);

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
            MessageBox.Show(this, "写入成功(国标，有 User Bank)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
