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
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.RFID;

namespace RfidTool
{
    public partial class ScanDialog : Form
    {
        string _typeOfUsage = null; // 10 图书; 80 读者证; 30 层架标

        public string TypeOfUsage
        {
            get
            {
                return _typeOfUsage;
            }
            set
            {
                _typeOfUsage = value;
                SetTitle();
            }
        }

        // 当前正在寻求处理的册条码号
        // string _currentBarcode = "";
        public event WriteCompleteEventHandler WriteComplete = null;

        public ScanDialog()
        {
            InitializeComponent();

            toolTip1.SetToolTip(this.textBox_barcode, "输入条码号");
            toolTip1.SetToolTip(this.textBox_processingBarcode, "待处理的条码号");
            toolTip1.SetToolTip(this.button_clearProcessingBarcode, "清除待处理的条码号");
        }

        private void textBox_barcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                // MessageBox.Show(this, $"输入 '{this.textBox_barcode.Text}'");
                this.ProcessingBarcode = this.textBox_barcode.Text.Trim(new char[] { ' ', '\r', '\n' });
                // this.textBox_barcode.SelectAll();
                this.textBox_barcode.Clear();
                e.Handled = true;

                // 触发处理
                ProcessBarcode(null);
            }
        }

        private void ScanDialog_Load(object sender, EventArgs e)
        {
            SetTitle();

            // 首次填充标签
            FillAllTags();
        }

        private void ScanDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        void SetTitle()
        {
            if (this.TypeOfUsage == "30")
                this.Text = "扫描并写入 层架标";
            else if (string.IsNullOrEmpty(this.TypeOfUsage) || this.TypeOfUsage == "10")
                this.Text = "扫描并写入 图书标签";
            else if (this.TypeOfUsage == "80")
                this.Text = "扫描并写入 读者证";
            else
                this.Text = $"扫描并写入 '{this.TypeOfUsage}'";
        }

        private void ScanDialog_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                DataModel.TagChanged += DataModel_TagChanged;
                DataModel.SetError += DataModel_SetError;
            }
            else
            {
                DataModel.TagChanged -= DataModel_TagChanged;
                DataModel.SetError -= DataModel_SetError;
            }
        }

        private void DataModel_SetError(object sender, SetErrorEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Error))
                this.ShowMessage("", "");
            else
                this.ShowMessage(e.Error, "red");
        }

        // 读卡器上的标签发生变化
        private void DataModel_TagChanged(object sender, NewTagChangedEventArgs e)
        {
            bool hasAdded = false;
            if (e.AddTags != null && e.AddTags.Count > 0)
            {
                this.Invoke((Action)(() =>
                {
                    UpdateTags(e.AddTags);
                }));
                hasAdded = true;
            }

            if (e.UpdateTags != null && e.UpdateTags.Count > 0)
            {
                this.Invoke((Action)(() =>
                {
                    UpdateTags(e.UpdateTags);
                }));
                hasAdded = true;
            }

            if (hasAdded)
                ProcessBarcode(null);

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
        const int COLUMN_AOI = 4;
        const int COLUMN_ANTENNA = 5;
        const int COLUMN_READERNAME = 6;

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
            string aoi = "";

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
                aoi = chip.FindElement(ElementOID.AOI)?.Text;
            }


            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
            ListViewUtil.ChangeItemText(item, COLUMN_TOU, tou);
            ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
            ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);
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
                /*
                if (item.Tag is not ItemInfo info)
                    continue;
                */
                ItemInfo info = item.Tag as ItemInfo;
                if (info == null)
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
        void ProcessBarcode(ListViewItem selectedItem)
        {
            string barcode = "";
            this.Invoke((Action)(() =>
            {
                barcode = this.ProcessingBarcode;
            }));

            if (string.IsNullOrEmpty(barcode))
                return;

            OneTag tag = null;
            if (selectedItem == null)
            {
                var find_result = FindBlankTag(barcode);
                if (find_result.Value == 0)
                {
                    ClientInfo.Speak($"请在读写器上放好空白标签，或双击选择其他可用标签");
                    ShowMessage($"请在读写器上放好空白标签，或双击选择其他可用标签");
                    return;
                }

                tag = find_result.Tag;
            }
            else
            {
                tag = (selectedItem.Tag as ItemInfo).TagData.OneTag;
            }

            string oi = DataModel.DefaultOiString;
            string aoi = DataModel.DefaultAoiString;

            if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
            {
                ShowMessage($"警告: 尚未设置机构代码或非标准机构代码");
                // TODO: 弹出对话框警告一次。可以选择不再警告
            }

            var uid = tag.UID;
            var tou = this.TypeOfUsage;
            if (string.IsNullOrEmpty(tou))
                tou = "10"; // 默认图书

            var chip = new LogicChip();
            chip.SetElement(ElementOID.PII, barcode);
            if (string.IsNullOrEmpty(oi) == false)
                chip.SetElement(ElementOID.OI, oi);
            if (string.IsNullOrEmpty(aoi) == false)
                chip.SetElement(ElementOID.AOI, aoi);
            chip.SetElement(ElementOID.TypeOfUsage, tou);

            TagInfo new_tag_info = GetTagInfo(tag.TagInfo, chip);

            var write_result = DataModel.WriteTagInfo(tag.ReaderName, tag.TagInfo,
                new_tag_info);

            if (write_result.Value == -1)
            {
                ShowMessage(write_result.ErrorInfo);
                ShowMessageBox(write_result.ErrorInfo);
                return;
            }

            WriteComplete?.Invoke(this, new WriteCompleteventArgs
            {
                Chip = chip,
                TagInfo = new_tag_info
            });

            // 语音提示写入成功
            ClientInfo.Speak($"{barcode} 写入成功");
            ShowMessage($"{barcode} 写入成功");
            ClearBarcode();
        }

        // 正在处理的条码号
        public string ProcessingBarcode
        {
            get
            {
                return this.textBox_processingBarcode.Text;
            }
            set
            {
                this.textBox_processingBarcode.Text = value;
            }
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
                /*
                _currentBarcode = "";
                this.textBox_barcode.Text = "";
                this.textBox_barcode.Focus();
                */
                this.ProcessingBarcode = "";
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

            new_tag_info.DSFID = 0x06;  // 图书

            // 上架状态
            new_tag_info.AFI = 0x07;
            new_tag_info.EAS = true;

            return new_tag_info;
        }

        private void button_write_Click(object sender, EventArgs e)
        {
            ListViewItem item = null;
            if (this.listView_tags.SelectedItems.Count == 1)
                item = this.listView_tags.SelectedItems[0];

            ProcessBarcode(item);

            this.textBox_barcode.Focus();
        }

        // 上下文菜单
        private void listView_tags_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            /*
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制解释信息到剪贴板 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_copyDescriptionToClipbard_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
            */

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearSelectedTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*
            menuItem = new MenuItem("保存标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("测试");
            menuItem.Click += new System.EventHandler(this.menu_test_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("测试创建错误的标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedErrorTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("测试创建 PII 为空的标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedErrorTagContent_1_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
            */

            menuItem = new MenuItem("清除标签缓存 (&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearTagsCache_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_tags, new Point(e.X, e.Y));
        }

        void menu_clearTagsCache_Click(object sender, EventArgs e)
        {
            TagList.ClearTagTable(null);
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllItems(this.listView_tags);
        }

        void menu_clearSelectedTagContent_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
    $"确实要清除选定的 {this.listView_tags.SelectedItems.Count} 个标签的内容?",
    "RfidToolForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                // string uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                ClearTagContent(item);
            }

            // listView_tags_SelectedIndexChanged(this, new EventArgs());
        }

        void ClearTagContent(ListViewItem item)
        {
            string strError = "";

            try
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                var old_tag_info = item_info.TagData.OneTag.TagInfo;
                var new_tag_info = old_tag_info.Clone();
                // 制造一套空内容
                {
                    new_tag_info.AFI = 0;
                    new_tag_info.DSFID = 0;
                    new_tag_info.EAS = false;
                    List<byte> bytes = new List<byte>();
                    for (int i = 0; i < new_tag_info.BlockSize * new_tag_info.MaxBlockCount; i++)
                    {
                        bytes.Add(0);
                    }
                    new_tag_info.Bytes = bytes.ToArray();
                    new_tag_info.LockStatus = "";
                }

                var result = DataModel.WriteTagInfo(item_info.TagData.OneTag.ReaderName,
    old_tag_info,
    new_tag_info);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                // await Task.Run(() => { GetTagInfo(item); });
                return;
            }
            catch (Exception ex)
            {
                strError = "ClearTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
        }

        static void SetItemColor(ListViewItem item, string state)
        {
            if (state == "normal")
            {
                //item.BackColor = SystemColors.Window;
                //item.ForeColor = SystemColors.WindowText;

                if (item.ListView != null)
                {
                    item.BackColor = item.ListView.BackColor;
                    item.ForeColor = item.ListView.ForeColor;
                }
                return;
            }

            if (state == "changed")
            {
                //item.BackColor = SystemColors.Info;
                //item.ForeColor = SystemColors.InfoText;

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

        private void listView_tags_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_tags.SelectedItems.Count == 1)
            {
                ProcessBarcode(this.listView_tags.SelectedItems[0]);
            }

            this.textBox_barcode.Focus();
        }

        private void listView_tags_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.button_write.Enabled = (this.listView_tags.SelectedItems.Count == 1);
        }

        private void button_clearProcessingBarcode_Click(object sender, EventArgs e)
        {
            this.textBox_processingBarcode.Clear();
        }

        private void textBox_processingBarcode_TextChanged(object sender, EventArgs e)
        {
            this.button_clearProcessingBarcode.Enabled = (this.textBox_processingBarcode.Text.Length > 0);
        }

        void ShowMessage(string text, string color = "")
        {
            if (this.InvokeRequired)
                this.Invoke((Action)(() =>
                {
                    _showMessage(text, color);
                }));
            else
                _showMessage(text, color);
        }

        void _showMessage(string text, string color = "")
        {
            this.label_message.Text = text;
            if (color == "red")
            {
                this.label_message.BackColor = Color.DarkRed;
                this.label_message.ForeColor = Color.White;
            }
            else
            {
                this.label_message.BackColor = SystemColors.Control;
                this.label_message.ForeColor = SystemColors.ControlText;
            }
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

    public class ItemInfo
    {
        public TagAndData TagData { get; set; }
    }
}
