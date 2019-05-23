﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.RFID.UI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    public partial class RfidToolForm : MyForm
    {
        public event AskTagEventHandler AskTag = null;

        public bool LayoutVertical
        {
            get
            {
                return this.splitContainer1.Orientation == Orientation.Vertical;
            }
            set
            {
                if (value == true)
                    this.splitContainer1.Orientation = Orientation.Vertical;
                else
                {
                    this.splitContainer1.Orientation = Orientation.Horizontal;
                    this.splitContainer1.SplitterDistance = this.splitContainer1.Height / 3;
                }
            }
        }

        string _mode = "";  // auto_fix_eas
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        public string ProtocolFilter = null;    //  InventoryInfo.ISO15693 + "," + InventoryInfo.ISO14443A;

        public string MessageText { get; set; }

        // EAS 是否修复成功
        public bool EasFixed { get; set; }

        // [in][out] 当前选中的事项的 PII 或者 UID。形态为 uid:xxxx 或者 pii:xxxx
        public string SelectedID { get; set; }

        // [out] 选中的事项的 PII
        public string SelectedPII { get; set; }

        // 是否自动关闭对话框。条件是 SelectedID 事项被自动选定了
        public bool AutoCloseDialog { get; set; }

        // 自动选择的条件。
        // 空等同于 "auto"。
        //      "auto"  表示会自动使用 selectedID 和 selectedPII 中非 null 的哪个
        //      "auto_or_blankPII" 表示匹配上 SelectedID 和 SelectedPII 之一，或者当前列表中有一个空标签(PII 为空)就算匹配
        public string AutoSelectCondition { get; set; }

        const int COLUMN_PII = 0;
        const int COLUMN_UID = 1;
        const int COLUMN_READERNAME = 2;
        const int COLUMN_PROTOCOL = 3;

        public RfidToolForm()
        {
            InitializeComponent();

            this.chipEditor1.TitleVisible = false;
        }

        private void RfidToolForm_Load(object sender, EventArgs e)
        {
            this.FloatingMessageForm.AutoHide = false;

            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            Task.Run(() =>
            {
                //InitialRfidChannel();
                OpenRfidCapture(false);
            });


            if (string.IsNullOrEmpty(this.MessageText) == false)
                this.ShowMessage(this.MessageText, "yellow", true);

            this.toolStripButton_autoRefresh.Checked = Program.MainForm.AppInfo.GetBoolean("rfidtoolform",
                "auto_refresh",
                true);
            if (this.toolStripButton_autoRefresh.Checked == false)
                Task.Run(() => { UpdateChipList(true); });

            this.BeginInvoke(new Action(() =>
            {
                this.listView_tags.Focus();
            }));
        }

        private void RfidToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void RfidToolForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_timerRefresh != null)
                _timerRefresh.Dispose();

            _rfidChannels?.Close((channel) =>
            {
                EndRfidChannel(channel);
            });
            //ReleaseRfidChannel();

            Program.MainForm.AppInfo.SetBoolean("rfidtoolform",
    "auto_refresh",
    this.toolStripButton_autoRefresh.Checked);
        }

        // private static readonly Object _syncRoot_update = new Object();
        int _inUpdate = 0;

        // 更新标签列表
        bool UpdateChipList(bool show_messageBox)
        {
            int nRet = Interlocked.Increment(ref _inUpdate);
            try
            {
                if (nRet != 1)
                    return false;
                // this.ClearMessage();
                string strError = "";
                if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl))
                {
                    strError = "尚未配置 RFID 中心 URL";
                    goto ERROR1;
                }

                RfidChannel channel = GetRfidChannel(
                    out strError);
                if (channel == null)
                {
                    strError = $"GetRfidChannel() error: {strError}";
                    goto ERROR1;
                }

                try
                {
                    ListTagsResult result = channel.Object.ListTags("*", null);
                    if (result.Value == -1)
                    {
                        strError = $"ListTags() error. ErrorInfo={result.ErrorInfo}, ErrorCode={result.ErrorCode}";
                        goto ERROR1;
                    }

                    List<Task> tasks = new List<Task>();
                    bool is_empty = false;
                    bool changed = false;
                    this.Invoke((Action)(() =>
                    {
                        is_empty = this.listView_tags.Items.Count == 0;

                        List<ListViewItem> items = new List<ListViewItem>();
                        foreach (OneTag tag in result.Results)
                        {
                            if (this.ProtocolFilter != null
                                && StringUtil.IsInList(tag.Protocol, this.ProtocolFilter) == false)
                                continue;

                            ListViewItem item = FindItem(this.listView_tags,
                                tag.ReaderName,
                                tag.UID);
                            if (item == null)
                            {
                                item = new ListViewItem();
                                ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.UID);
                                ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);
                                ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.Protocol);
                                item.Tag = new ItemInfo { OneTag = tag };
                                this.listView_tags.Items.Add(item);

                                if (tag.TagInfo == null)
                                {
                                    // 启动单独的线程去填充 .TagInfo
                                    tasks.Add(Task.Run(() => { GetTagInfo(item); }));
                                }
                            }
                            else
                            {
                                // 刷新 readerName
                                string old_readername = ListViewUtil.GetItemText(item, COLUMN_READERNAME);
                                if (old_readername != tag.ReaderName)
                                {
                                    // ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);
                                    item.Tag = new ItemInfo { OneTag = tag };
                                    if (tag.TagInfo == null)
                                    {
                                        // 启动单独的线程去填充 .TagInfo
                                        tasks.Add(Task.Run(() => { GetTagInfo(item); }));
                                    }
                                }
                            }

                            if (item != null)
                                items.Add(item);
                            changed = true;
                        }

                        // 交叉运算得到比 items 中多出来的 ListViewItem，删除它们
                        List<ListViewItem> delete_items = new List<ListViewItem>();
                        foreach (ListViewItem item in this.listView_tags.Items)
                        {
                            if (items.IndexOf(item) == -1)
                                delete_items.Add(item);
                        }

                        foreach (ListViewItem item in delete_items)
                        {
                            this.listView_tags.Items.Remove(item);
                            changed = true;
                        }
                    }));

                    // 再建立一个 task，等待 tasks 执行完以后，自动选定一个 item
                    if (tasks.Count > 0 || changed)
                    {
                        Task.Run(() =>
                        {
                            Task.WaitAll(tasks.ToArray());
                            bool closed = false;
                            this.Invoke((Action)(() =>
                            {
                                // 首次填充，自动设好选定状态
                                // if (is_empty)
                                {
                                    // TODO: 只有当列表发生了实质性刷新的时候，才有必要调用一次 SelectItem。也就是说，不要每秒都无条件调用一次
                                    var ret = SelectItem(this.SelectedID != null ? this.SelectedID : this.SelectedPII);

                                    if (// string.IsNullOrEmpty(this.SelectedPII) == false
                                        ret == true
                                        && this.AutoCloseDialog)
                                    {
                                        if (this.DoOK(show_messageBox) == true)
                                        {
                                            this.DialogResult = DialogResult.OK;
                                            this.Close();
                                            closed = true;
                                        }
                                    }
                                }
                            }));

                            if (closed == false)
                            {
                                //this.Invoke((Action)(() =>
                                //{
                                FillEntityInfo();
                                //}));

                                if (this._mode == "auto_fix_eas")
                                {
                                    this.Invoke((Action)(() =>
                                    {
                                        AutoFixEas(channel);
                                    }));
                                }
                            }
                        });
                    }
                    else
                    {
                        if (this.AskTag != null)
                        {
                            AskTagEventArgs e = new AskTagEventArgs();
                            this.AskTag(this, e);
                            this.ShowMessage(e.Text, "green", true);
                        }
                    }
                    return true;
                }
                catch (RemotingException ex)
                {
                    strError = "UpdateChipList() 出现异常: " + ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "UpdateChipList() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
                finally
                {
                    ReturnRfidChannel(channel);
                }

                ERROR1:
                if (show_messageBox)
                    this.ShowMessageBox(strError);
                else
                    this.ShowMessage(strError, "red", true);
                return false;
            }
            finally
            {
                Interlocked.Decrement(ref _inUpdate);
            }
        }

        static void SetItemColor(ListViewItem item, string state)
        {
            if (state == "normal")
            {
                //item.BackColor = SystemColors.Window;
                //item.ForeColor = SystemColors.WindowText;

                item.BackColor = item.ListView.BackColor;
                item.ForeColor = item.ListView.ForeColor;

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


        private void toolStripButton_loadRfid_Click(object sender, EventArgs e)
        {
            Task.Run(() => { UpdateChipList(true); });
        }

        class IdInfo
        {
            public string Prefix { get; set; }
            public string Text { get; set; }

            public static IdInfo Parse(string text)
            {
                IdInfo info = new IdInfo();
                if (text.IndexOf(":") == -1)
                {
                    info.Prefix = "pii";
                    info.Text = text;
                    return info;
                }
                List<string> parts = StringUtil.ParseTwoPart(text, ":");
                info.Prefix = parts[0];
                info.Text = parts[1];
                return info;
            }
        }

#if NO
        // 事项和权重
        class ItemAndWeight
        {
            public ListViewItem Item { get; set; }
            public int Weight { get; set; }
        }
#endif

        bool SelectItem(string id)
        {
            if (id == null)
                return false;

            IdInfo info = IdInfo.Parse(id);
            List<ListViewItem> level1_items = new List<ListViewItem>();
            List<ListViewItem> level2_items = new List<ListViewItem>();
            // List<ItemAndWeight> results = new List<ItemAndWeight>();
            foreach (ListViewItem item in this.listView_tags.Items)
            {
                if (info.Prefix == "pii")
                {
                    // string current_pii = ListViewUtil.GetItemText(item, COLUMN_PII);
                    string current_pii = GetItemPII(item);
                    if (current_pii == info.Text)
                    {
                        level1_items.Add(item);
                        //ListViewUtil.SelectLine(item, true);
                        //return true;
                    }
                    else if (StringUtil.IsInList("auto_or_blankPII", this.AutoSelectCondition) && string.IsNullOrEmpty(current_pii) == true)
                    {
                        level2_items.Add(item);
                    }
                }

                if (info.Prefix == "uid")
                {
                    string current_uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                    if (current_uid == info.Text)
                    {
                        level1_items.Add(item);
                        // ListViewUtil.SelectLine(item, true);
                        // return true;
                    }
                }
            }

            if (level1_items.Count > 0)
            {
                ListViewUtil.SelectLine(level1_items[0], true);
                return true;
            }

            // 只有当 level2 命中精确为一个时才选中。命中多了则无法选择
            if (level2_items.Count == 1)
            {
                ListViewUtil.SelectLine(level2_items[0], true);
                return true;
            }

            // 触发提示
            if (this.AskTag != null)
            {
                AskTagEventArgs e = new AskTagEventArgs();
                this.AskTag(this, e);
                this.ShowMessage(e.Text, "green", true);
            }

            return false;
        }

        // 根据读卡器名字和标签 UID 找到已有的 ListViewItem 对象
        static ListViewItem FindItem(ListView list,
            string reader_name,
            string uid)
        {
            foreach (ListViewItem item in list.Items)
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                OneTag tag = item_info.OneTag;
                if (// tag.ReaderName == reader_name && 
                    tag.UID == uid)
                    return item;
            }

            return null;
        }

        void GetTagInfo(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            OneTag tag = item_info.OneTag;
            if (tag.Protocol == InventoryInfo.ISO14443A)
                return; // 暂时还不支持对 14443A 的卡进行 GetTagInfo() 操作

            RfidChannel channel = GetRfidChannel(
    out string strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }

            try
            {
                GetTagInfoResult result = channel.Object.GetTagInfo("*", tag.UID);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                tag.TagInfo = result.TagInfo;

                string hex_string = Element.GetHexString(result.TagInfo.Bytes, "4");

                item_info.LogicChipItem = LogicChipItem.FromTagInfo(result.TagInfo);
                item_info.LogicChipItem.PropertyChanged += LogicChipItem_PropertyChanged;
                this.Invoke((Action)(() =>
                {
                    // 2019/2/27
                    // 刷新 ReaderName 列
                    {
                        string new_readername = tag.TagInfo.ReaderName;
                        if (string.IsNullOrEmpty(new_readername))
                            new_readername = tag.ReaderName;

                        string old_readername = ListViewUtil.GetItemText(item, COLUMN_READERNAME);
                        if (old_readername != new_readername)
                            ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, new_readername);
                    }

                    string pii = item_info.LogicChipItem.PrimaryItemIdentifier;
                    // ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
                    SetItemPIIColumn(item, pii, true);
                    if (this.SelectedPII != null
                        && pii == this.SelectedPII)
                        item.Font = new Font(item.Font, FontStyle.Bold);
                }));
                return;
            }
            catch (Exception ex)
            {
                strError = "GetTagInfo() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                ReturnRfidChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
        }

        private void LogicChipItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateChanged(sender as LogicChipItem);

            UpdateSaveButton();
        }

        void UpdateSaveButton()
        {
            this.Invoke((Action)(() =>
            {
                int count = 0;
                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    ItemInfo tag_info = (ItemInfo)item.Tag;
                    if (tag_info.LogicChipItem != null
                    && tag_info.LogicChipItem.Changed == true)
                        count++;
                }
                if (count > 0)
                    this.toolStripButton_saveRfid.Enabled = true;
                else
                    this.toolStripButton_saveRfid.Enabled = false;
            }));
        }

        void UpdateChanged(LogicChipItem chip)
        {
            this.Invoke((Action)(() =>
            {
                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    ItemInfo tag_info = (ItemInfo)item.Tag;
                    if (tag_info.LogicChipItem == chip)
                    {
#if NO
                        // 更新 column 0
                        string uid = ListViewUtil.GetItemText(item, 0);
                        if (uid.StartsWith("*"))
                            uid = uid.Substring(1);
                        if (tag_info.LogicChip.Changed)
                            uid = "*" + uid;
                        ListViewUtil.ChangeItemText(item, 0, uid);
#endif

                        if (tag_info.LogicChipItem.Changed)
                        {
                            SetItemColor(item, "changed");

                            // item.BackColor = Color.DarkGreen;
                            // item.ForeColor = Color.White;
                        }
                        else
                        {
                            SetItemColor(item, "normal");

                            //item.BackColor = this.listView_tags.BackColor;
                            //item.ForeColor = this.listView_tags.ForeColor;
                        }

                        // 更新 PII
                        string pii = tag_info.LogicChipItem.FindElement(ElementOID.PII)?.Text;
                        // ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
                        SetItemPIIColumn(item, pii, false);
                        return;
                    }
                }
            }));
        }

        private void listView_tags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_tags.SelectedItems.Count == 1)
            {
                ItemInfo item_info = (ItemInfo)this.listView_tags.SelectedItems[0].Tag;
                OneTag tag = item_info.OneTag;
                // var tag_info = tag.TagInfo;

                this.SelectedTag = tag;
                this.button_OK.Enabled = true;

                this.chipEditor1.LogicChipItem = item_info.LogicChipItem;

                if (string.IsNullOrEmpty(item_info.Xml) == false)
                {
                    BookItem book_item = new BookItem();
                    int nRet = book_item.SetData("",
                        item_info.Xml,
                        null,
                        out string strError);
                    if (nRet == -1)
                    {
                        // 如何报错?
                    }
                    else
                        this.propertyGrid_record.SelectedObject = book_item;
                }
                else
                    this.propertyGrid_record.SelectedObject = null;
            }
            else
            {
                this.chipEditor1.LogicChipItem = null;
                this.propertyGrid_record.SelectedObject = null;

                this.SelectedTag = null;
                this.button_OK.Enabled = false;
            }
        }

        // 填充所有的册记录信息
        void FillEntityInfo()
        {
            LibraryChannel channel = this.GetChannel();
            try
            {
                var items = (List<ListViewItem>)this.Invoke(new Func<List<ListViewItem>>(() =>
                {
                    List<ListViewItem> results = new List<ListViewItem>();
                    results.AddRange(this.listView_tags.Items.Cast<ListViewItem>());
                    return results;
                }));


                foreach (ListViewItem item in items)
                {
                    ItemInfo item_info = (ItemInfo)item.Tag;
                    var tag_info = item_info.OneTag.TagInfo;
                    if (tag_info == null)
                        continue;
                    LogicChip chip = LogicChip.From(tag_info.Bytes,
                        (int)tag_info.BlockSize);
                    string pii = chip.FindElement(ElementOID.PII)?.Text;
                    if (string.IsNullOrEmpty(pii))
                        continue;

                    long lRet = channel.GetItemInfo(null,
                        pii,
                        "xml",
                        out string xml,
                        "",
                        out string biblio,
                        out string strError);

                    if (lRet == -1)
                    {
                        // TODO: 给 item 设置出错状态
                        continue;
                    }

                    item_info.Xml = xml;
                }
            }
            catch (Exception ex)
            {
                // TODO: 如何报错？让操作者从册信息界面上可以看出报错
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 自动修复 EAS
        void AutoFixEas(RfidChannel channel)
        {
            string strError = "";

#if NO
            RfidChannel channel = GetRfidChannel(
    out strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }
#endif
            try
            {
                IdInfo info = IdInfo.Parse(this.SelectedID);

                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    string uid = ListViewUtil.GetItemText(item, COLUMN_UID);

                    ItemInfo item_info = (ItemInfo)item.Tag;
                    var tag_info = item_info.OneTag.TagInfo;
                    if (tag_info == null)
                        continue;
                    LogicChip chip = LogicChip.From(tag_info.Bytes,
                        (int)tag_info.BlockSize);
                    string pii = chip.FindElement(ElementOID.PII)?.Text;
                    if ((info.Prefix == "pii" && pii == info.Text)
                        || (info.Prefix == "uid" && uid == info.Text))
                    {
                        // 获得册记录的外借状态。
                        // return:
                        //      -1  出错
                        //      0   没有被外借
                        //      1   在外借状态
                        int nRet = GetCirculationState(item_info.Xml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 便于观察
                        // Application.DoEvents();
                        // Thread.Sleep(2000);

                        // 检测 EAS 是否正确
                        NormalResult result = null;
                        // TODO: 这里发现不一致的时候，是否要出现明确提示，让操作者知晓？
                        if (nRet == 1 && tag_info.EAS == true)
                            result = SetEAS(channel, "*", "uid:" + tag_info.UID, false, out strError);
                        else if (nRet == 0 && tag_info.EAS == false)
                            result = SetEAS(channel, "*", "uid:" + tag_info.UID, true, out strError);
                        else
                            continue;

                        if (result.Value == -1)
                        {
                            strError = $"{result.ErrorInfo}, error_code={result.ErrorCode}";
                            goto ERROR1;
                        }

                        this.EasFixed = true;
                    }
                }
            }
            finally
            {
                // ReturnRfidChannel(channel);
            }

            if (this._mode == "auto_fix_eas" && this.EasFixed)
                this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得册记录的外借状态。
        // return:
        //      -1  出错
        //      0   没有被外借
        //      1   在外借状态
        static int GetCirculationState(string strItemXml, out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strItemXml))
            {
                strError = "册记录 XML 为空，无法判断外借状态";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "册记录 XML 装入 XMLDOM 时出错:" + ex.Message;
                return -1;
            }

            string borrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
            if (string.IsNullOrEmpty(borrower) == false)
                return 1;
            return 0;
        }

        #region RFID Channel

        ChannelPool<RfidChannel> _rfidChannels = new ChannelPool<RfidChannel>();

        RfidChannel GetRfidChannel()
        {
            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl))
                throw new Exception("尚未配置 RFID 中心 URL");

            return _rfidChannels.GetChannel(() =>
            {
                var channel = StartRfidChannel(
        Program.MainForm.RfidCenterUrl,
        out string strError);
                if (channel == null)
                    throw new Exception(strError);
                return channel;
            });
        }

        RfidChannel GetRfidChannel(out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl))
            {
                strError = "尚未配置 RFID 中心 URL";
                return null;
            }

            try
            {
                return _rfidChannels.GetChannel(() =>
                {
                    var channel = StartRfidChannel(
            Program.MainForm.RfidCenterUrl,
            out string strError1);
                    if (channel == null)
                        throw new Exception(strError1);
                    return channel;
                });
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }
        }

        void ReturnRfidChannel(RfidChannel channel)
        {
            _rfidChannels.ReturnChannel(channel);
        }

#if REMOVED
        public RfidChannel _rfidChannel = null;

        void InitialRfidChannel()
        {
            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl) == false)
            {
                _rfidChannel = StartRfidChannel(
        Program.MainForm.RfidCenterUrl,
        out string strError);
                if (_rfidChannel == null)
                    this.ShowMessageBox(strError);
#if NO
                // 马上检测一下通道是否可用
                try
                {
                    _rfidChannel.Object.ListReaders();
                }
                catch (Exception ex)
                {
                    this.ShowMessageBox("启动 RFID 设备时出错: " + ex.Message);
                }
#endif
            }
        }

        void ReleaseRfidChannel()
        {
            if (_rfidChannel != null)
            {
                EndRfidChannel(_rfidChannel);
                _rfidChannel = null;
            }
        }
#endif

#if REMOVED
        void OpenRfidCapture(bool open)
        {
            if (_rfidChannel != null)
            {
                try
                {
                    _rfidChannel.Object.EnableSendKey(open);
                }
                catch
                {

                }
            }
        }
#endif

        void OpenRfidCapture(bool open)
        {
            try
            {
                var channel = GetRfidChannel();
                try
                {
                    channel.Object.EnableSendKey(open);
                }
                catch
                {

                }
                finally
                {
                    ReturnRfidChannel(channel);
                }
            }
            catch
            {

            }
        }

        #endregion

        class ItemInfo
        {
            public OneTag OneTag { get; set; }
            public string Xml { get; set; }
            public LogicChipItem LogicChipItem { get; set; }
        }

        private void toolStripButton_autoRefresh_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_autoRefresh.Checked)
                _timerRefresh = new System.Threading.Timer(
                    new System.Threading.TimerCallback(timerCallback),
                    null,
                    TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            else
            {
                _timerRefresh.Dispose();
                _timerRefresh = null;
            }
        }

        System.Threading.Timer _timerRefresh = null;

        void timerCallback(object o)
        {
            UpdateChipList(false);
        }

        private bool DoOK(bool show_messageBox)
        {
            string strError = "";

            if (this.listView_tags.SelectedItems.Count == 0)
            {
                strError = "请选择一个标签";
                goto ERROR1;
            }

            if (this.listView_tags.SelectedItems.Count > 0)
            {
                this.SelectedID = "uid:" + ListViewUtil.GetItemText(this.listView_tags.SelectedItems[0], COLUMN_UID);
                this.SelectedPII = GetItemPII(this.listView_tags.SelectedItems[0]);
                // this.SelectedPII = ListViewUtil.GetItemText(this.listView_tags.SelectedItems[0], COLUMN_PII);
            }
            else
            {
                this.SelectedID = null;
                this.SelectedPII = null;
            }

            Debug.Assert(this.SelectedTag != null);
            if (this.SelectedTag != null
                && this.SelectedTag.TagInfo == null
                && this.SelectedTag.Protocol == InventoryInfo.ISO15693
                && this.listView_tags.SelectedItems.Count > 0)
            {
                Debug.Assert(this.listView_tags.SelectedItems.Count > 0);
                ListViewItem selected_item = this.listView_tags.SelectedItems[0];
                GetTagInfo(selected_item);
                strError = "您选择的行尚未获得 TagInfo。请稍候重试";
                goto ERROR1;
            }

            return true;
            ERROR1:
            if (show_messageBox)
                MessageBox.Show(this, strError);
            return false;
        }


        public OneTag SelectedTag = null;

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (DoOK(true) == true)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
#if NO
            string strError = "";

            if (this.listView_tags.SelectedItems.Count == 0)
            {
                strError = "请选择一个标签";
                goto ERROR1;
            }

            if (this.listView_tags.SelectedItems.Count > 0)
            {
                this.SelectedID = "uid:" + ListViewUtil.GetItemText(this.listView_tags.SelectedItems[0], COLUMN_UID);
                this.SelectedPII = GetItemPII(this.listView_tags.SelectedItems[0]);
                // this.SelectedPII = ListViewUtil.GetItemText(this.listView_tags.SelectedItems[0], COLUMN_PII);
            }
            else
            {
                this.SelectedID = null;
                this.SelectedPII = null;
            }


            Debug.Assert(this.SelectedTag != null);
            if (this.SelectedTag != null
                && this.SelectedTag.TagInfo == null
                && this.SelectedTag.Protocol == InventoryInfo.ISO15693
                && this.listView_tags.SelectedItems.Count > 0)
            {
                Debug.Assert(this.listView_tags.SelectedItems.Count > 0);
                ListViewItem selected_item = this.listView_tags.SelectedItems[0];
                GetTagInfo(selected_item);
                strError = "您选择的行尚未获得 TagInfo。请稍候重试";
                goto ERROR1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        // 修改 ListViewItem 的 PII 列文字
        static void SetItemPIIColumn(ListViewItem item, string pii, bool clearColor)
        {
            if (string.IsNullOrEmpty(pii))
                pii = "(空白)";
            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);

            if (clearColor)
                SetItemColor(item, "normal");
        }

        // 获得一个 ListViewItem 的 PII 列文字
        static string GetItemPII(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info != null && item_info.LogicChipItem != null)
                return item_info.LogicChipItem.PrimaryItemIdentifier;
            string text = ListViewUtil.GetItemText(item, COLUMN_PII);
            if (text == "(空白)")
                text = "";
            return text;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public bool OkCancelVisible
        {
            get
            {
                return this.panel_okCancel.Visible;
            }
            set
            {
                this.panel_okCancel.Visible = value;
            }
        }

        private void listView_tags_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制解释信息到剪贴板 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_copyDescriptionToClipbard_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

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

            menuItem = new MenuItem("保存标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_tags, new Point(e.X, e.Y));
        }

        // 针对选定的标签，创建描述文字并复制到 Windows 剪贴板
        void menu_copyDescriptionToClipbard_Click(object sender, EventArgs e)
        {
            StringBuilder text = new StringBuilder();
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                if (text.Length > 0)
                    text.Append("\r\n***\r\n");
                ItemInfo item_info = (ItemInfo)item.Tag;
                if (item_info.LogicChipItem == null)
                    text.Append("\r\n[LogicChipItem 为空]\r\n");
                else
                    text.Append(item_info.LogicChipItem.GetDescription());
            }
            Clipboard.SetDataObject(text.ToString(), true);
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllItems(this.listView_tags);
        }

        async void menu_clearSelectedTagContent_Click(object sender, EventArgs e)
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
                await ClearTagContent(item);
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());
        }

        async Task ClearTagContent(ListViewItem item)
        {
            RfidChannel channel = GetRfidChannel(
    out string strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }

            try
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                var old_tag_info = item_info.OneTag.TagInfo;
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
                var result = channel.Object.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                await Task.Run(() => { GetTagInfo(item); });
                return;
            }
            catch (Exception ex)
            {
                strError = "ClearTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                ReturnRfidChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
        }

        async void menu_saveSelectedTagContent_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                if (await SaveTagContent(item) == true)
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            if (count > 0)
                this.ShowMessage($"保存成功({count})", "green", true);
            else
                this.ShowMessage("没有需要保存的事项", "yellow", true);
        }

        async Task<bool> SaveTagContent(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.LogicChipItem.Changed == false)
                return false;

            RfidChannel channel = GetRfidChannel(
    out string strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;
                var new_tag_info = BuildNewTagInfo(
    old_tag_info,
    item_info.LogicChipItem);

                var result = channel.Object.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                await Task.Run(() => { GetTagInfo(item); });

                UpdateChanged(item_info.LogicChipItem);
                return true;
            }
            catch (Exception ex)
            {
                strError = "SaveTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                ReturnRfidChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return false;
        }

        static TagInfo BuildNewTagInfo(TagInfo old_tag_info,
    LogicChipItem chip)
        {
            TagInfo new_tag_info = old_tag_info.Clone();
            new_tag_info.Bytes = chip.GetBytes(
                (int)new_tag_info.MaxBlockCount * (int)new_tag_info.BlockSize,
                (int)new_tag_info.BlockSize,
                LogicChip.GetBytesStyle.None,
                out string block_map);
            new_tag_info.LockStatus = block_map;

            new_tag_info.DSFID = chip.DSFID;
            new_tag_info.AFI = chip.AFI;
            new_tag_info.EAS = chip.EAS;
            return new_tag_info;
        }

        private async void toolStripButton_saveRfid_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (ListViewItem item in this.listView_tags.Items)
            {
                if (await SaveTagContent(item) == true)
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            if (count > 0)
                this.ShowMessage($"保存成功({count})", "green", true);
            else
                this.ShowMessage("没有需要保存的事项", "yellow", true);
        }

#if NO
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    new ControlWrapper(this.toolStripButton_autoRefresh, true),
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    new ControlWrapper(this.toolStripButton_autoRefresh, true),
                };
                GuiState.SetUiState(controls, value);
            }
        }
#endif
    }

    /// <summary>
    /// 提示放置标签事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void AskTagEventHandler(object sender,
    AskTagEventArgs e);

    /// <summary>
    /// 提示放置标签事件的参数
    /// </summary>
    public class AskTagEventArgs : EventArgs
    {
        // [out] 提示文字
        public string Text { get; set; }
    }
}
