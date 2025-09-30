using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.RFID;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static RfidTool.ModifyDialog;

namespace RfidTool
{
    public partial class WriteErrorDialog : Form
    {
        const int COLUMN_UID = 0;
        const int COLUMN_ERRORINFO = 1;
        const int COLUMN_PII = 2;
        const int COLUMN_TU = 3;
        const int COLUMN_OI = 4;
        const int COLUMN_AOI = 5;
        const int COLUMN_EAS = 6;
        const int COLUMN_AFI = 7;
        const int COLUMN_READERNAME = 8;
        const int COLUMN_ANTENNA = 9;
        const int COLUMN_PROTOCOL = 10;
        const int COLUMN_TID = 11;

        public WriteErrorDialog()
        {
            InitializeComponent();
        }

        public void Remove(WriteErrorInfo info)
        {
            var hex = ModifyDialog.GetTidHex(info.TID);

            ListViewItem item = ListViewUtil.FindItem(this.listView_tags, hex, COLUMN_TID);
            if (item != null)
            {
                this.listView_tags.Items.Remove(item);
            }
        }

        public void Remove(string tid_hex)
        {
            ListViewItem item = ListViewUtil.FindItem(this.listView_tags, tid_hex, COLUMN_TID);
            if (item != null)
            {
                this.listView_tags.Items.Remove(item);
            }
        }

        public void Add(WriteErrorInfo info,
            string error_info)
        {
            // old uid/new uid/tid/protocol/antenna/reader name/error info

            string tid = GetTidHex(info.TID);

            ListViewItem item = ListViewUtil.FindItem(this.listView_tags, tid, COLUMN_TID);
            if (item == null)
            {
                item = new ListViewItem();
                // item.Tag = new ItemInfo { TagData = tag };
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "(尚未填充)");
                this.listView_tags.Items.Add(item);
            }
            RefreshItem(item, info.OldTagInfo, error_info);
        }

        void RefreshItem(ListViewItem item,
            TagInfo tag,
            string error_info)
        {
            ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.AntennaID.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);

            Debug.Assert(tag.Protocol != "ISO14443A");
            ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.Protocol);

            if (tag.Protocol == InventoryInfo.ISO18000P6C)
            {
                byte[] tid_bank = tag.Tag as byte[];
                ListViewUtil.ChangeItemText(item, COLUMN_TID, GetTidHex(tid_bank));
            }

            ListViewUtil.ChangeItemText(item, COLUMN_PII, "(尚未填充)");

            {
                LogicChip chip = null;
                string pii = "";
                string tou = "";
                string oi = "";
                string aoi = "";
                string uhf_protocol = "";

                if (tag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    // 注1: taginfo.EAS 在调用后可能被修改
                    // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                    var uhf_info = RfidTagList.GetUhfChipInfo(tag/*, "convertValueToGB,ensureChip"*/); // "dontCheckUMI"

                    if (string.IsNullOrEmpty(uhf_info.ErrorInfo) == false)
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_PII, $"error: {uhf_info.ErrorInfo}");
                        SetItemColor(item, "error");
                        return;
                    }

                    // TODO: 对于 .Bytes 缺失的畸形 UHF 标签，最好是尽量解析内容，然后给出警告信息解释问题所在
                    // 单独严格解析一次标签内容

                    chip = uhf_info.Chip;
                    Debug.Assert(chip != null);
                    // taginfo.EAS 可能会被修改
                    uhf_protocol = uhf_info.UhfProtocol;
                    pii = uhf_info.PII;
                    if (uhf_info.ContainOiElement)
                        aoi = uhf_info.OI;
                    else
                        oi = uhf_info.OI;
                }
                else
                {
                    // *** ISO15693 HF
                    if (tag.Bytes != null)
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        chip = LogicChip.From(tag.Bytes,
            (int)tag.BlockSize,
            "");
                        pii = chip.FindElement(ElementOID.PII)?.Text;
                    }
                    else
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_PII, $"error: 标签 {tag.UID} 没有数据体");
                        SetItemColor(item, "error");
                        return;
                    }
                }

                tou = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;

                // 2023/11/26
                RfidTagList.SetTagInfoEAS(tag);

                var eas = tag.EAS ? "On" : "Off";
                var afi = Element.GetHexString(tag.AFI);

                if (string.IsNullOrEmpty(oi)
                    && tag.Protocol == InventoryInfo.ISO15693)
                {
                    oi = chip?.FindElement(ElementOID.OI)?.Text;
                    aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                }

                ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);

                if (string.IsNullOrEmpty(error_info) == false)
                {
                    // TODO: 可显示拟修改成的 PII 内容
                    // ListViewUtil.ChangeItemText(item, COLUMN_PII, $"?{pii}");
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, $"{error_info}");
                    SetItemColor(item, "error");
                }

                ListViewUtil.ChangeItemText(item, COLUMN_TU, tou);
                ListViewUtil.ChangeItemText(item, COLUMN_EAS, eas);
                ListViewUtil.ChangeItemText(item, COLUMN_AFI, afi);
                ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
                ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);

                // 刷新协议栏
                if (tag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    string name = uhf_protocol;
                    if (uhf_protocol == "gxlm")
                        name = "高校联盟";
                    else if (uhf_protocol == "gb")
                        name = "国标";
                    ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL,
                        string.IsNullOrEmpty(name) ? tag.Protocol : tag.Protocol + ":" + name);

                    byte[] tid_bank = tag.Tag as byte[];
                    ListViewUtil.ChangeItemText(item, COLUMN_TID, GetTidHex(tid_bank));
                }
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

            // 处理过了，并且发生了实质性修改
            if (state == "changed")
            {
                item.BackColor = Color.DarkGreen;
                item.ForeColor = Color.White;
                return;
            }

            // 处理过了但没有发生实质性修改
            if (state == "notchanged")
            {
                item.BackColor = Color.Black;
                item.ForeColor = Color.White;
                return;
            }

            if (state == "error")
            {
                // if 是为了避免(在重复刷新时)出现闪动
                if (item.BackColor != Color.DarkRed)
                    item.BackColor = Color.DarkRed;
                if (item.ForeColor != Color.White)
                    item.ForeColor = Color.White;
                return;
            }

            if (state == "disable")
            {
                item.BackColor = Color.DarkGray;
                item.ForeColor = Color.White;
                return;
            }

            if (state == "cross")
            {
                item.BackColor = Color.White;
                item.ForeColor = Color.DarkGray;
                return;
            }

            throw new ArgumentException($"未知的 state '{state}'");
        }


        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.listView_tags,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.listView_tags,
                };
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
