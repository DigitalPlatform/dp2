using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Core;
using DigitalPlatform.GUI;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

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

        ErrorTable _errorTable = null;

        public ScanDialog()
        {
            InitializeComponent();

            toolTip1.SetToolTip(this.textBox_barcode, "输入条码号");
            toolTip1.SetToolTip(this.textBox_processingBarcode, "待处理的条码号");
            toolTip1.SetToolTip(this.button_clearProcessingBarcode, "清除待处理的条码号");

            DataModel.TagChanged += DataModel_TagChanged;
            DataModel.SetError += DataModel_SetError;

            _errorTable = new ErrorTable((s) =>
            {
                try
                {
                    this.Invoke((Action)(() =>
                    {

                    }));
                }
                catch (ObjectDisposedException)
                {

                }
            });

            if (StringUtil.IsDevelopMode() == true)
                this.button_test.Visible = true;
        }

        private void textBox_barcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                // MessageBox.Show(this, $"输入 '{this.textBox_barcode.Text}'");
                string barcode = this.textBox_barcode.Text.Trim(new char[] { ' ', '\r', '\n' });

                var verifyBarcode = DataModel.VerifyPiiWhenWriteTag;
                // 校验条码号
                if (verifyBarcode == true)
                {
                    if (string.IsNullOrEmpty(barcode) == true)
                    {
                        string text = "条码号不应为空";
                        FormClientInfo.Speak(text);
                        ShowMessageBox("input", text);
                        this.textBox_barcode.SelectAll();
                        this.textBox_barcode.Focus();
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(DataModel.PiiVerifyRule))
                    {
                        string text = $"尚未设置条码号校验规则";
                        FormClientInfo.Speak(text);
                        ShowMessageBox("input", text);
                        this.textBox_barcode.SelectAll();
                        this.textBox_barcode.Focus();
                        return;

                    }
                    var verify_result = VerifyBarcode(ModifyDialog.GetVerifyType(TypeOfUsage), barcode);
                    if (verify_result.OK == false)
                    {
                        string text = $"条码号 {barcode} 不合法";
                        FormClientInfo.Speak(text);
                        ShowMessageBox("input", text);
                        this.textBox_barcode.SelectAll();
                        this.textBox_barcode.Focus();
                        return;
                    }
                }

                // 查询本地存储
                if (this.UseLocalStoreage())
                {
                    var items = EntityStoreage.FindByBarcode(barcode);
                    string text = "";
                    if (items == null || items.Count == 0)
                        text = $"条码号 {barcode} 没有找到册记录";
                    else if (items.Count > 1)
                    {
                        // TODO: 是否允许从多个中选择？
                        text = $"条码号 {barcode} 找到 {items.Count} 个册记录";
                    }

                    if (items.Count == 1)
                    {
                        this.ProcessingEntity = items[0];
                    }
                    else
                    {
                        this.ProcessingEntity = null;
                    }

                    ShowBookTitle(this.ProcessingEntity?.Title);

                    if (string.IsNullOrEmpty(text) == false)
                    {
                        FormClientInfo.Speak(text);
                        ShowMessageBox("input", text);
                        this.textBox_barcode.SelectAll();
                        this.textBox_barcode.Focus();
                        return;
                    }
                }
                else
                    this.ProcessingEntity = null;

                ShowMessageBox("input", null);

                this.ProcessingBarcode = barcode;
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

            BeginVerifyEnvironment();
        }

        private void ScanDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            DataModel.TagChanged -= DataModel_TagChanged;
            DataModel.SetError -= DataModel_SetError;
        }

        public void BeginVerifyEnvironment()
        {
            Task.Run(() =>
            {
                if (UseLocalStoreage() && EntityStoreage.GetCount() == 0)
                {
                    string text = "尚未导入脱机册信息";
                    ShowMessage(text);
                    ShowMessageBox("load", text);
                }
                else
                    ShowMessageBox("load", null);
            });
        }

        void SetTitle()
        {
            /*
            if (this.TypeOfUsage == "30")
                this.Text = "扫描并写入 层架标";
            else if (string.IsNullOrEmpty(this.TypeOfUsage) || this.TypeOfUsage == "10")
                this.Text = "扫描并写入 图书标签";
            else if (this.TypeOfUsage == "80")
                this.Text = "扫描并写入 读者证";
            else
                this.Text = $"扫描并写入 '{this.TypeOfUsage}'";
            */
            this.Text = $"扫描并写入 {GetCaption(this.TypeOfUsage)}";
        }

        public string GetCaption(string tu)
        {
            if (tu == "30")
                return "层架标";
            else if (string.IsNullOrEmpty(tu) || tu == "10")
                return "图书标签";
            else if (tu == "80")
                return "读者证";
            else
                return $"'{tu}'";
        }

        private void ScanDialog_VisibleChanged(object sender, EventArgs e)
        {
            /*
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
            */
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
                    lock (_syncRootFill)
                    {
                        UpdateTags(e.AddTags);
                    }
                }));
                hasAdded = true;
            }

            if (e.UpdateTags != null && e.UpdateTags.Count > 0)
            {
                this.Invoke((Action)(() =>
                {
                    lock (_syncRootFill)
                    {
                        UpdateTags(e.UpdateTags);
                    }
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
        const int COLUMN_TITLE = 3;     // 册记录中的书名
        const int COLUMN_ACCESSNO = 4;  // 册记录中的索取号
        const int COLUMN_EAS = 5;
        const int COLUMN_AFI = 6;
        const int COLUMN_OI = 7;
        const int COLUMN_AOI = 8;
        const int COLUMN_SHELFLOCATION = 9; // 标签中的 ShelfLocation
        const int COLUMN_ANTENNA = 10;
        const int COLUMN_READERNAME = 11;
        const int COLUMN_PROTOCOL = 12;

        object _syncRootFill = new object();

        // TODO: 注意和 DataModel_TagChanged() 处理互斥
        void FillAllTags()
        {
            lock (_syncRootFill)
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
        }

        // 更新 tags
        void UpdateTags(List<TagAndData> tags)
        {
            foreach (var tag in tags)
            {
                ListViewItem item = ListViewUtil.FindItem(this.listView_tags, tag.OneTag.UID, COLUMN_UID);
                if (item == null)
                {
                    // 2021/1/7
                    // tag.OneTag = DeepClone(tag.OneTag);

                    item = new ListViewItem();
                    item.Tag = new ItemInfo { TagData = tag };
                    ListViewUtil.ChangeItemText(item, COLUMN_PII, "(尚未填充)");
                    this.listView_tags.Items.Add(item);
                }
                RefreshItem(item, tag);
            }

            /*
            OneTag DeepClone(OneTag t)
            {
                t = t.Clone();
                if (t.TagInfo != null)
                    t.TagInfo = t.TagInfo.Clone();
                return t;
            }
            */
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
            // 2022/7/24
            SetItemColor(item, "normal");

            string pii = "(尚未填充)";
            string tou = "";
            string eas = "";
            string afi = "";
            string oi = "";
            string aoi = "";
            string shelfLocation = "";  // 标签中的 shelfLocation

            var iteminfo = item.Tag as ItemInfo;

            ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.OneTag.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.OneTag.AntennaID.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.OneTag.ReaderName);
            ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.OneTag.Protocol);

            ListViewUtil.ChangeItemText(item, COLUMN_PII, "(尚未填充)");
            ListViewUtil.ChangeItemText(item, COLUMN_TITLE, "");
            ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, "");
            ListViewUtil.ChangeItemText(item, COLUMN_SHELFLOCATION, "");

            try
            {
                var taginfo = tag.OneTag.TagInfo;
                if (taginfo != null)
                {
                    LogicChip chip = null;

                    if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        var epc_bank = Element.FromHexString(taginfo.UID);

                        if (UhfUtility.IsBlankTag(epc_bank, taginfo.Bytes) == true)
                        {
                            // 空白标签
                            pii = GetPIICaption(null);
                        }
                        else
                        {
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
                                iteminfo.UhfProtocol = "gb";
                                pii = GetPIICaption(GetPiiPart(parse_result.UII));
                                oi = GetOiPart(parse_result.UII, false);
                            }
                            else
                            {
                                // *** 高校联盟 UHF
                                var parse_result = GaoxiaoUtility.ParseTag(
                    epc_bank,
                    taginfo.Bytes);
                                if (parse_result.Value == -1)
                                    throw new Exception(parse_result.ErrorInfo);
                                chip = parse_result.LogicChip;
                                taginfo.EAS = !parse_result.EpcInfo.Lending;
                                iteminfo.UhfProtocol = "gxlm";
                                pii = GetPIICaption(GetPiiPart(parse_result.EpcInfo.PII));
                                oi = GetOiPart(parse_result.EpcInfo.PII, false);
                            }
                        }
                    }
                    else
                    {
                        // *** ISO15693 HF
                        if (taginfo.Bytes != null)
                        {
                            iteminfo.Exception = null;
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            chip = LogicChip.From(taginfo.Bytes,
                (int)taginfo.BlockSize,
                "");

                            pii = GetPIICaption(chip.FindElement(ElementOID.PII)?.Text);
                        }
                    }

                    tou = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;
                    eas = taginfo.EAS ? "On" : "Off";
                    afi = Element.GetHexString(taginfo.AFI);

                    if (string.IsNullOrEmpty(oi))
                    {
                        oi = chip?.FindElement(ElementOID.OI)?.Text;
                        aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                    }

                    if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        if (iteminfo.UhfProtocol == "gxlm")
                        {
                            // 数字平台针对高校联盟扩充的 AOI
                            if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                                aoi = chip?.FindElement((ElementOID)27)?.Text;
                        }
                    }

                    shelfLocation = chip?.FindElement(ElementOID.ShelfLocation)?.Text;
                }


                if (string.IsNullOrEmpty(tag.Error) == false)
                {
                    /*
                    // 2022/7/23
                    if (iteminfo.TagData != null && iteminfo.TagData.Error == null)
                        iteminfo.TagData.Error = tag.Error;
                    */

                    ListViewUtil.ChangeItemText(item, COLUMN_PII, pii + " error:" + tag.Error);
                    SetItemColor(item, "error");
                }
                else
                    ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);

                // string accessNo = "";   // 册记录中的索取号

                // 设置 Title
                if (this.UseLocalStoreage())
                {
                    var uii = BuildUii(pii, oi, aoi);
                    var entity = EntityStoreage.FindByUII(uii);
                    if (entity == null)
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_TITLE, "");
                        ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, "");
                    }
                    else
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_TITLE, entity.Title);
                        ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, GetAccessNo(entity));
                    }
                }

                // 方括号中为标签中的索取号
                ListViewUtil.ChangeItemText(item, COLUMN_SHELFLOCATION, shelfLocation);

                ListViewUtil.ChangeItemText(item, COLUMN_TOU, tou);
                ListViewUtil.ChangeItemText(item, COLUMN_EAS, eas);
                ListViewUtil.ChangeItemText(item, COLUMN_AFI, afi);
                ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
                ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);

                // 刷新协议栏
                if (tag.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    string name = iteminfo.UhfProtocol;
                    if (iteminfo.UhfProtocol == "gxlm")
                        name = "高校联盟";
                    else if (iteminfo.UhfProtocol == "gb")
                        name = "国标";
                    ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL,
                        string.IsNullOrEmpty(name) ? tag.OneTag.Protocol : tag.OneTag.Protocol + ":" + name);
                }
            }
            catch (Exception ex)
            {
                // 2022/7/23
                // iteminfo.TagData.Error = ex.Message;
                iteminfo.Exception = ex;
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + ex.Message);
                SetItemColor(item, "error");
            }
        }

        static string BuildUii(string pii, string oi, string aoi)
        {
            if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                return pii;
            if (string.IsNullOrEmpty(oi) == false)
                return oi + "." + pii;
            if (string.IsNullOrEmpty(aoi) == false)
                return aoi + "." + pii;
            return pii;
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

        public static string GetPIICaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";
            return text;
        }

        class FindTagResult : NormalResult
        {
            public ListViewItem Item { get; set; }
            public OneTag Tag { get; set; }
        }

        // 寻找一个可用于写入的空白标签，或者相同 PII 的标签
        FindTagResult FindBlankTag(string pii, string oi)
        {
            var overwrite_error_tag = DataModel.ErrorContentAsBlank;

            lock (_syncRootFill)
            {
                List<FindTagResult> blank_results = new List<FindTagResult>();
                List<FindTagResult> pii_results = new List<FindTagResult>();
                List<FindTagResult> error_results = new List<FindTagResult>();
                //FindTagResult blank_result = null;
                //FindTagResult pii_result = null;

                this.Invoke((Action)(() =>
                {
                    foreach (ListViewItem item in this.listView_tags.Items)
                    {
                        /*
                        if (item.Tag is not ItemInfo info)
                            continue;
                        */
                        ItemInfo info = item.Tag as ItemInfo;
                        if (info == null)
                            continue;

                        if (info.Exception != null && info.Exception is TagDataException)
                        {
                            error_results.Add(new FindTagResult
                            {
                                Value = 1,
                                Item = item,
                                Tag = info.TagData.OneTag,
                            });
                            continue;
                        }

                        if (string.IsNullOrEmpty(info.TagData.Error) == false)
                            continue;

                        string current_pii = ListViewUtil.GetItemText(item, COLUMN_PII);
                        string current_oi = ListViewUtil.GetItemText(item, COLUMN_OI);
                        string current_aoi = ListViewUtil.GetItemText(item, COLUMN_AOI);


                        if (current_pii == pii
                        // 2021/6/16
                        // 判断机构代码是否吻合
                        && (oi == current_oi || oi == current_aoi))
                        {
                            pii_results.Add(new FindTagResult
                            {
                                Value = 1,
                                Item = item,
                                Tag = info.TagData.OneTag,
                            });
                        }
                        else if ((string.IsNullOrEmpty(current_pii) == true || current_pii == "(空)")
                            && info.TagData.OneTag.TagInfo != null)
                        {
                            blank_results.Add(new FindTagResult
                            {
                                Value = 1,
                                Item = item,
                                Tag = info.TagData.OneTag,
                            });
                        }
                    }
                }));

                if (pii_results.Count + blank_results.Count == 1)
                {
                    // 优先返回 PII 匹配的行
                    if (pii_results.Count == 1)
                        return pii_results[0];
                    // 次优先返回 PII 为空的行
                    if (blank_results.Count == 1)
                        return blank_results[0];
                }

                // 2022/7/23
                // 如果有解析错误的标签，则返回
                if (DataModel.ErrorContentAsBlank
                    && error_results.Count == 1)
                {
                    return error_results[0];
                }

                // 返回无法满足条件的具体原因
                List<string> reasons = new List<string>();
                if (pii_results.Count > 1)
                    reasons.Add($"PII '{pii}' 匹配标签不唯一 ({pii_results.Count})");
                if (blank_results.Count > 1)
                    reasons.Add($"空白标签不唯一 ({blank_results.Count})");
                if (pii_results.Count > 0 && blank_results.Count > 0)
                    reasons.Add($"出现了 PII 匹配，同时还有空白标签的情况");

                // 没有找到
                return new FindTagResult
                {
                    Value = 0,
                    ErrorInfo = StringUtil.MakePathList(reasons, ";")
                };
            }
        }

        int _inProcessing = 0;

        // 寻找适当的 RFID 标签完成写入操作
        void ProcessBarcode(ListViewItem selectedItem)
        {
            _inProcessing++;
            try
            {
                // 防止重入
                if (_inProcessing > 1)
                {
                    Console.Beep();
                    return;
                }

                string barcode = "";
                EntityItem entity = ProcessingEntity;

                this.Invoke((Action)(() =>
                {
                    barcode = this.ProcessingBarcode;
                }));

                if (string.IsNullOrEmpty(barcode))
                    return;

                if (string.IsNullOrEmpty(barcode) == true)
                {
                    string text = "条码号不应为空";
                    FormClientInfo.Speak(text);
                    ShowMessageBox("processBarcode", text);
                    return;
                }

                // TODO: 本地存储情况下，直接使用册记录中的 OI
                // 还可以提供一种强制使用 OiSetting 中的 OI 的方法

                bool localStore = this.UseLocalStoreage();

                if (localStore == false)
                {
                    string error = VerifyOiSetting();
                    if (error != null)
                    {
                        FormClientInfo.Speak("O I (所属机构代码) 和 A O I (非标准所属机构代码) 尚未配置");
                        MessageBox.Show(this, error);
                        using (SettingDialog dlg = new SettingDialog())
                        {
                            GuiUtil.SetControlFont(dlg, this.Font);
                            ClientInfo.MemoryState(dlg, "settingDialog", "state");

                            dlg.ShowDialog(this);
                            if (dlg.DialogResult == DialogResult.OK)
                                DataModel.TagList.EnableTagCache = DataModel.EnableTagCache;
                        }
                        return;
                    }
                }

                string oi = DataModel.DefaultOiString;
                string aoi = DataModel.DefaultAoiString;

                if (localStore && entity != null)
                {
                    oi = GetOI(entity);
                    aoi = "";
                }
                else
                {
                    if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                    {
                        ShowMessage($"警告: 尚未设置机构代码或非标准机构代码");
                        // TODO: 弹出对话框警告一次。可以选择不再警告
                    }
                }

                OneTag tag = null;
                ItemInfo iteminfo = null;
                if (selectedItem == null)
                {
                    var find_result = FindBlankTag(barcode,
                        string.IsNullOrEmpty(oi) == false ? oi : aoi);
                    if (find_result.Value == 0)
                    {
                        FormClientInfo.Speak($"请在读写器上放好空白标签，或双击选择其他可用标签");
                        ShowMessage($"请在读写器上放好空白标签，或双击选择其他可用标签");
                        return;
                    }

                    tag = find_result.Tag;
                    iteminfo = find_result.Item.Tag as ItemInfo;
                    selectedItem = find_result.Item;
                }
                else
                {
                    iteminfo = (selectedItem.Tag as ItemInfo);
                    tag = iteminfo.TagData.OneTag;
                }

                // 2021/1/7
                // 克隆对象，避免后面因为标签快速拿走而被改变
                if (tag != null)
                {
                    tag = DeepClone(tag);
                }

                OneTag DeepClone(OneTag t)
                {
                    t = t.Clone();
                    if (t.TagInfo != null)
                        t.TagInfo = t.TagInfo.Clone();
                    return t;
                }

                if (tag.TagInfo == null)
                {
                    /*
                    string pii = ListViewUtil.GetItemText(selectedItem, COLUMN_PII);
                    throw new Exception("test");
                    */
                    string text = "标签信息尚未填充。请稍后重试写入";
                    ShowMessage(text);
                    ShowMessageBox("processBarcode", text);
                    return;
                }

                /*
                Debug.Assert(tag != null);
                Debug.Assert(tag.TagInfo != null);

                // testing
                // DataModel.TagList.ClearTagTable(tag.UID);

                Debug.Assert(tag.TagInfo != null);
                */



                // 判断序列号中的功能类型
                {
                    string function_type = "HF";
                    if (tag.TagInfo.Protocol == InventoryInfo.ISO18000P6C)
                        function_type = "UHF";

                    if (HasLicense(function_type) == false)
                        return;
                }

                var uid = tag.UID;
                var tou = this.TypeOfUsage;
                if (string.IsNullOrEmpty(tou))
                    tou = "10"; // 默认图书

                string accessNo = GetAccessNo(entity);

                var chip = new LogicChip();
                chip.SetElement(ElementOID.PII, barcode);
                if (string.IsNullOrEmpty(oi) == false)
                    chip.SetElement(ElementOID.OI, oi);
                if (string.IsNullOrEmpty(aoi) == false)
                    chip.SetElement(ElementOID.AOI, aoi);
                if (string.IsNullOrEmpty(accessNo) == false)
                    chip.SetElement(ElementOID.ShelfLocation, accessNo);

                bool eas = false;
                if (this.IsBook())
                    eas = true;

                TagInfo new_tag_info = GetTagInfo(tag.TagInfo, chip, eas);

                NormalResult write_result = null;
                for (int i = 0; i < 2; i++)
                {
                    // 重试前延时半秒
                    if (i > 0)
                        Thread.Sleep(500);

                    write_result = DataModel.WriteTagInfo(tag.ReaderName, tag.TagInfo,
                        new_tag_info);
                    if (write_result.Value != -1)
                        break;
                }
                if (write_result.Value == -1)
                {
                    ShowMessage(write_result.ErrorInfo);
                    ShowMessageBox("processBarcode", write_result.ErrorInfo);
                    return;
                }

                WriteComplete?.Invoke(this, new WriteCompleteventArgs
                {
                    Chip = chip,
                    TagInfo = new_tag_info
                });

                // 写入 UID-->PII 对照关系日志文件
                if (tou == "10")
                    DataModel.WriteToUidLogFile(uid,
                        ModifyDialog.MakeOiPii(barcode, oi, aoi));

                /*
                // 2022/7/24
                TagAndData data = new TagAndData();
                data.OneTag.TagInfo = new_tag_info;
                UpdateTags(new List<TagAndData> { data });
                */

                // 语音提示写入成功
                FormClientInfo.Speak($"{GetSpeakNumber(barcode)} 写入成功", false, true);
                ShowMessage($"{barcode} 写入成功");
                ShowMessageBox("processBarcode", null);
                ClearBarcode();
            }
            catch (Exception ex)
            {
                string error = $"写入失败: {ex.Message}";
                // FormClientInfo.Speak(error);
                ShowMessage(error, "red");
                ClientInfo.WriteErrorLog($"写入标签时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                ShowMessageBox("processBarcode", error);
            }
            finally
            {
                _inProcessing--;
            }
        }

        // 把号码转换为方便 text-to-speech 念出来的方式
        static string GetSpeakNumber(string number)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach(var ch in number)
            {
                if (i > 0)
                    text.Append(" ");
                text.Append(ch);
                i++;
            }

            return text.ToString();
        }

        bool IsBook()
        {
            if (string.IsNullOrEmpty(this.TypeOfUsage) || this.TypeOfUsage.StartsWith("1"))
                return true;
            return false;
        }

        bool UseLocalStoreage()
        {
            if (DataModel.UseLocalStoreage && this.IsBook())
                return true;
            return false;
        }

        static string GetOI(EntityItem entity)
        {
            if (entity == null)
                return "";
            if (string.IsNullOrEmpty(entity.Xml))
                return "";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(entity.Xml);
            string oi = DomUtil.GetElementText(dom.DocumentElement, "oi");
            return oi;
        }

        static string GetAccessNo(EntityItem entity)
        {
            if (entity == null)
                return "";
            if (string.IsNullOrEmpty(entity.Xml))
                return "";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(entity.Xml);
            return DomUtil.GetElementText(dom.DocumentElement, "accessNo");
        }

        BarcodeValidator _validator = null;

        public ValidateResult VerifyBarcode(string type,
            string barcode)
        {
            string rule = DataModel.PiiVerifyRule;
            if (string.IsNullOrWhiteSpace(rule))
                throw new ArgumentException("尚未设置 PiiVerifyRule");

            if (_validator == null)
                _validator = new BarcodeValidator(rule);

            return _validator.ValidateByType(
                type,
                barcode);
        }

        // 设置 TU 字段。注意 国标和高校联盟的取值表完全不同
        // parameters:
        //      data_format gb/gxlm
        void SetTypeOfUsage(LogicChip chip, string data_format)
        {
            string tou = this.TypeOfUsage;
            if (string.IsNullOrEmpty(tou))
                tou = "10"; // 默认图书

            // 高校联盟
            if (data_format == "gxlm")
            {
                switch (tou)
                {
                    case "10":  // 图书
                        tou = "0.0";
                        break;
                    case "80":  // 读者证
                        tou = "3.0";
                        break;
                    case "30":  // 层架标
                        tou = "2.0";
                        break;
                    default:
                        throw new Exception($"高校联盟不支持的国标 TU 值 '{tou}'");
                }
                chip.SetElement(ElementOID.TypeOfUsage, tou, false);
            }
            else
                chip.SetElement(ElementOID.TypeOfUsage, tou);
        }

        // 校验 OI 和 AOI 参数是否正确设置了
        public static string VerifyOiSetting()
        {
            string oi = DataModel.DefaultOiString;
            string aoi = DataModel.DefaultAoiString;
            if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                return "OI(所属机构代码) 和 AOI(非标准所属机构代码) 尚未配置";
            return null;
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
                if (string.IsNullOrEmpty(value))
                    this.label_title.Text = "";
            }
        }

        // 正在处理的册对象
        public EntityItem ProcessingEntity
        {
            get;
            set;
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

        public TagInfo GetTagInfo(TagInfo existing,
    LogicChip chip,
    bool eas)
        {
            if (existing.Protocol == InventoryInfo.ISO15693)
            {
                SetTypeOfUsage(chip, "gb");

                TagInfo new_tag_info = existing.Clone();
                new_tag_info.Bytes = chip.GetBytes(
                    (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                    (int)new_tag_info.BlockSize,
                    LogicChip.GetBytesStyle.None,
                    out string block_map);
                new_tag_info.LockStatus = block_map;

                new_tag_info.DSFID = LogicChip.DefaultDSFID;  // 图书

                // 上架状态
                /*
                if (eas == true)
                {
                    new_tag_info.AFI = 0x07;
                    new_tag_info.EAS = true;
                }
                else
                {
                    new_tag_info.AFI = 0xc2;
                    new_tag_info.EAS = false;
                }
                */
                new_tag_info.SetEas(eas);

                return new_tag_info;
            }

            if (existing.Protocol == InventoryInfo.ISO18000P6C)
            {
                var build_user_bank = DataModel.WriteUhfUserBank;

                // 读者卡和层架标必须有 User Bank，不然 TU 字段没有地方放
                if (build_user_bank == false
    && this.TypeOfUsage != "10")
                    throw new Exception($"{GetCaption(this.TypeOfUsage)}必须写入 User Bank");

                // TODO: 判断标签内容是空白/国标/高校联盟格式，采取不同的写入格式
                /*
高校联盟格式
国标格式
* */
                var isExistingGB = UhfUtility.IsISO285604Format(Element.FromHexString(existing.UID), existing.Bytes);

                TagInfo new_tag_info = existing.Clone();
                if (DataModel.UhfWriteFormat == "高校联盟格式")
                {
                    // 写入高校联盟数据格式
                    if (isExistingGB)
                    {
                        string warning = $"警告：即将用高校联盟格式覆盖原有国标格式";
                        DialogResult dialog_result = MessageBox.Show(this,
$"{warning}\r\n\r\n确实要覆盖？",
$"ScanDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (dialog_result == DialogResult.No)
                            throw new Exception("放弃写入");
                    }

                    // chip.SetElement(ElementOID.TypeOfUsage, tou);
                    SetTypeOfUsage(chip, "gxlm");

                    var result = GaoxiaoUtility.BuildTag(chip, build_user_bank, eas);
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                else
                {
                    // 写入国标数据格式
                    if (isExistingGB == false)
                    {
                        string warning = $"警告：即将用国标格式覆盖原有高校联盟格式";
                        DialogResult dialog_result = MessageBox.Show(this,
$"{warning}\r\n\r\n确实要覆盖？",
$"ScanDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (dialog_result == DialogResult.No)
                            throw new Exception("放弃写入");
                    }
                    SetTypeOfUsage(chip, "gb");

                    var result = UhfUtility.BuildTag(chip,
                        true,
                        eas ? "afi_eas_on" : "");
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                return new_tag_info;
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
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

            menuItem = new MenuItem("写入(&W)");
            menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.button_write_Click);
            menuItem.Enabled = isWriteEnabled();
            contextMenu.MenuItems.Add(menuItem);


            /*
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);
            */

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

            menuItem = new MenuItem("修改标签 EAS [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&E)");
            // menuItem.Click += new System.EventHandler(this.menu_changeSelectedTagEas_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            {
                MenuItem subMenuItem = new MenuItem("On");
                subMenuItem.Tag = "on";
                subMenuItem.Click += new System.EventHandler(this.menu_changeSelectedTagEas_Click);
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("Off");
                subMenuItem.Tag = "off";
                subMenuItem.Click += new System.EventHandler(this.menu_changeSelectedTagEas_Click);
                menuItem.MenuItems.Add(subMenuItem);

            }
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

            menuItem = new MenuItem("测试创建 PII 为空的标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedErrorTagContent_1_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
            */

            menuItem = new MenuItem("测试创建错误的标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&E)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedErrorTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除标签缓存 (&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearTagsCache_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_tags, new Point(e.X, e.Y));
        }

        void menu_clearTagsCache_Click(object sender, EventArgs e)
        {
            DataModel.TagList.ClearTagTable(null);
        }

        void menu_saveSelectedErrorTagContent_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
    $"确实要写入错误内容到选定的 {this.listView_tags.SelectedItems.Count} 个标签中?",
    "RfidTool",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                SaveErrorTagContent(item);
            }
        }

        // 2022/7/23
        // 写入错误标签内容
        void SaveErrorTagContent(ListViewItem item)
        {
            string strError = "";

            try
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                var old_tag_info = item_info.TagData.OneTag.TagInfo;
                var new_tag_info = old_tag_info.Clone();

                {
                    var bytes = ByteArray.GetTimeStampByteArray("E14018000300FE30303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030303030");

                    var count = old_tag_info.BlockSize * old_tag_info.MaxBlockCount;
                    // 修正
                    if (bytes.Length < count)
                    {
                        var temp = new List<byte>(bytes);
                        while (temp.Count < count)
                        {
                            temp.Add(temp[temp.Count - 1]);
                        }
                        bytes = temp.ToArray();
                        Debug.Assert(bytes.Length == count);
                    }
                    else if (bytes.Length > count)
                    {
                        var temp = new List<byte>(bytes);
                        temp.RemoveRange((int)count, temp.Count - (int)count);
                        bytes = temp.ToArray();
                        Debug.Assert(bytes.Length == count);
                    }

                    new_tag_info.Bytes = bytes;
                }

#if REMOVED
                // 制造一套空内容
                {
                    new_tag_info.AFI = 0;
                    new_tag_info.DSFID = 0;
                    new_tag_info.EAS = false;
                    if (old_tag_info.Protocol == InventoryInfo.ISO15693)
                    {
                        List<byte> bytes = new List<byte>();
                        for (int i = 0; i < new_tag_info.BlockSize * new_tag_info.MaxBlockCount; i++)
                        {
                            bytes.Add(0);
                        }
                        new_tag_info.Bytes = bytes.ToArray();
                        new_tag_info.LockStatus = "";
                    }
                    else if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // var pc = UhfUtility.ParsePC(Element.FromHexString(old_tag_info.UID), 2);
                        new_tag_info.UID = "0000" + Element.GetHexString(UhfUtility.BuildBlankEpcBank());
                        new_tag_info.Bytes = null;  // 这样可使得 User Bank 被清除
                    }
                }
#endif
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
                strError = "SaveErrorTagContent() 出现异常: " + ex.Message;
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
                MessageBox.Show(this, strError);
            }));
        }

#if REMOVED
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllItems(this.listView_tags);
            /*
            foreach(ListViewItem item in this.listView_tags.Items)
            {
                item.Selected = true;
            }
            */
        }
#endif

        void menu_changeSelectedTagEas_Click(object sender, EventArgs e)
        {
            if (ScanDialog.HasLicense("modify_eas", false) == false)
            {
                MessageBox.Show(this, "尚未许可 modify_eas 功能");
                return;
            }

            string style = (sender as MenuItem).Tag as string;
            int nRet = SetEAS(style == "on",
                out string strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int SetEAS(bool enable,
    out string strError)
        {
            strError = "";

            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                var info = item.Tag as ItemInfo;
                var tag = info.TagData?.OneTag?.TagInfo;
                if (tag == null)
                {
                    strError = "info.TagData?.OneTag?.TagInfo == null";
                    return -1;
                }
                var write_result = DataModel.SetEAS(tag.ReaderName,
        tag.UID,
        tag.AntennaID,
        enable,
        "");
                if (write_result.Value == -1)
                {
                    strError = write_result.ErrorInfo;
                    return -1;
                }

                // 清掉标签缓冲，迫使刷新列表显示
                DataModel.TagList.ClearTagTable(tag.UID);
            }

            return 0;
        }

        void menu_clearSelectedTagContent_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
    $"确实要清除选定的 {this.listView_tags.SelectedItems.Count} 个标签的内容?",
    "RfidTool",
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

        // TODO: 针对 UHF 实现
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
                    if (old_tag_info.Protocol == InventoryInfo.ISO15693)
                    {
                        List<byte> bytes = new List<byte>();
                        for (int i = 0; i < new_tag_info.BlockSize * new_tag_info.MaxBlockCount; i++)
                        {
                            bytes.Add(0);
                        }
                        new_tag_info.Bytes = bytes.ToArray();
                        new_tag_info.LockStatus = "";
                    }
                    else if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // var pc = UhfUtility.ParsePC(Element.FromHexString(old_tag_info.UID), 2);
                        new_tag_info.UID = "0000" + Element.GetHexString(UhfUtility.BuildBlankEpcBank());
                        new_tag_info.Bytes = null;  // 这样可使得 User Bank 被清除
                    }
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
                MessageBox.Show(this, strError);
            }));
        }

        // 判断序列号中的功能类型是否匹配
        public static bool HasLicense(string function_type,
            bool reinput = true)
        {
            string style = reinput ? "reinput" : "";
            if (StringUtil.IsDevelopMode())
                style += ",skipVerify";
            // return:
            //      -1  出错
            //      0   放弃
            //      1   成功
            int nRet = FormClientInfo.VerifySerialCode(
    $"设置序列号({function_type})",
    function_type,
    style,
    out string strError);
            if (nRet == 1)
                return true;
            return false;
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
            if (isWriteEnabled() == false)
            {
                FormClientInfo.Speak($"警告：无法写入");
                MessageBox.Show(this, "当前没有待写入的条码号，无法写入");
                return;
            }

            if (this.listView_tags.SelectedItems.Count == 1)
            {
                ProcessBarcode(this.listView_tags.SelectedItems[0]);
            }

            this.textBox_barcode.Focus();
        }

        private void listView_tags_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetWriteButtonState();
        }

        bool isWriteEnabled()
        {
            return (this.listView_tags.SelectedItems.Count == 1
                && string.IsNullOrEmpty(this.textBox_processingBarcode.Text) == false);
        }

        void SetWriteButtonState()
        {
            this.button_write.Enabled = isWriteEnabled();


        }

        private void button_clearProcessingBarcode_Click(object sender, EventArgs e)
        {
            this.textBox_processingBarcode.Clear();
            ShowBookTitle("");
        }

        private void textBox_processingBarcode_TextChanged(object sender, EventArgs e)
        {
            this.button_clearProcessingBarcode.Enabled = (this.textBox_processingBarcode.Text.Length > 0);
            SetWriteButtonState();
        }

        void ShowBookTitle(string text)
        {
            if (this.InvokeRequired)
                this.Invoke((Action)(() =>
                {
                    this.label_title.Text = text;
                }));
            else
                this.label_title.Text = text;
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

        private void button_test_Click(object sender, EventArgs e)
        {
            this.textBox_barcode.Focus();
            this.textBox_barcode.SelectAll();
            SendKeys.Send("0000001\r");
        }

        #region Error Dialog

        FloatingErrorDialog _errorDialog = null;

        void CreateErrorDialog()
        {
            if (_errorDialog == null)
            {
                _errorDialog = new FloatingErrorDialog();

                _errorDialog.FormClosing += _errorDialog_FormClosing;

                /*
                GuiUtil.SetControlFont(_errorDialog, this.Font);
                ClientInfo.MemoryState(_errorDialog, "scanDialog", "state");
                _errorDialog.UiState = ClientInfo.Config.Get("scanDialog", "uiState", null);
                */
            }
        }
        private void _errorDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dialog = sender as Form;

            // 将关闭改为隐藏
            dialog.Visible = false;
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        void ShowMessageBox(string type, string text)
        {
            // 语音提示出错
            if (text != null)
                FormClientInfo.Speak(text, false, false);

            this.Invoke((Action)(() =>
            {
                CreateErrorDialog();
                if (text == null)
                    _errorDialog.Hide();
                else
                {
                    if (_errorDialog.Visible == false)
                    {
                        _errorDialog.Show(this);

                        this.textBox_barcode.SelectAll();
                        this.textBox_barcode.Focus();
                    }
                }

                _errorTable.SetError(type, text);
                _errorDialog.Message = _errorTable.GetError(false);
            }));
        }

        #endregion

        /*
        void ShowMessageBox(string text)
        {
            // 语音提示出错
            FormClientInfo.Speak(text);
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, text);
            }));
        }
        */

        public void EnableControls(bool enable)
        {
            this.Invoke((Action)(() =>
            {
                this.listView_tags.Enabled = enable;
                this.textBox_processingBarcode.Enabled = enable;
                this.textBox_barcode.Enabled = enable;
                if (enable == true)
                {
                    this.textBox_barcode.SelectAll();
                    this.textBox_barcode.Focus();
                }
            }));
        }
    }

    public class ItemInfo
    {
        public TagAndData TagData { get; set; }
        // 标签所用的 UHF 标准。空/gb/gxlm 其中空表示未知
        public string UhfProtocol { get; set; }

        // 2022/7/23
        public Exception Exception { get; set; }
    }
}
