using System;
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

using Microsoft.VisualStudio.Threading;
using AsyncFriendlyStackTrace;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.RFID.UI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
// using DocumentFormat.OpenXml.Drawing;

namespace dp2Circulation
{
    public partial class RfidToolForm : MyForm
    {
        // ErrorTable _errorTable = null;

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

        string _mode = "";  // auto_fix_eas 或 auto_fix_eas_and_close 或它们的组合
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

        // RFID 协议过滤
        // 如果为 null，表示不过滤，任意协议都显示出来
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
        const int COLUMN_ANTENNA = 4;

        public RfidToolForm()
        {
            this.UseLooping = true; // 2022/11/4

            InitializeComponent();

            this.chipEditor1.TitleVisible = false;

            /*
            this._errorTable = new ErrorTable((s) =>
            {
                // TODO: 如果这以前残余的是同样 type，才能清为空。否则要保留
                this.ShowMessage(s, "red", true);
            });
            */
        }

        private void RfidToolForm_Load(object sender, EventArgs e)
        {
            this.FloatingMessageForm.AutoHide = false;

            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            _ = Task.Run(() =>
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
                _ = Task.Run(async () => { await UpdateChipListAsync(_cancel.Token, true); });

            this.toolStripButton_autoFixEas.Checked = Program.MainForm.AppInfo.GetBoolean("rfidtoolform",
    "auto_fix_eas",
    true);

            _errorTable = new ErrorTable((s) =>
            {
                this.Invoke((Action)(() =>
                {
                    if (this.label_message.Text != s)
                    {
                        this.label_message.Text = s;

                        this.label_message.Visible = !string.IsNullOrEmpty(s);
                    }
                }));
            });

            RfidManager.SetError += RfidManager_SetError;
            Program.MainForm.TagChanged += MainForm_TagChanged;

            this.BeginInvoke(new Action(() =>
            {
                this.listView_tags.Focus();
            }));
        }

        private void RfidManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("rfid", e.Error);
        }

        private void RfidToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        // CancellationTokenSource _cancel = new CancellationTokenSource();

        private void RfidToolForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel.Cancel();

            Program.MainForm.TagChanged -= MainForm_TagChanged;
            RfidManager.SetError -= RfidManager_SetError;

            if (_timerRefresh != null)
            {
                _timerRefresh.Dispose();
            }

#if OLD_CODE
            _rfidChannels?.Close((channel) =>
            {
                EndRfidChannel(channel);
            });
#endif

            Program.MainForm.AppInfo.SetBoolean("rfidtoolform",
    "auto_refresh",
    this.toolStripButton_autoRefresh.Checked);

            Program.MainForm.AppInfo.SetBoolean("rfidtoolform",
"auto_fix_eas",
this.toolStripButton_autoFixEas.Checked);
        }

        // 新 Tag 到来、变化、消失
        private async void MainForm_TagChanged(object sender, TagChangedEventArgs e)
        {
            if (this.PauseRfid)
                return;

            try
            {
                bool auto_refresh = (bool)this.Invoke((Func<bool>)(() =>
                {
                    return this.toolStripButton_autoRefresh.Checked;
                }));

                if (auto_refresh)
                    await UpdateChipListAsync(_cancel.Token, false);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                if (_cancel.IsCancellationRequested)
                    return;
                this.ShowMessage($"MainForm_TagChanged() 出现异常: {ex.Message}");
                MainForm.WriteErrorLog($"MainForm_TagChanged() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }


        /*
        void SetError(string type, string error)
        {
            _errorTable.SetError(type, error);
        }*/

        // private static readonly Object _syncRoot_update = new Object();
        int _inUpdate = 0;

        // 更新标签列表
        // 注意这个函数是在非界面线程中执行
        async Task<bool> UpdateChipListAsync(CancellationToken token,
            bool show_messageBox)
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

#if OLD_CODE
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
#if NO
                    ListTagsResult result = channel.Object.ListTags("*", null);
                    if (result.Value == -1)
                    {
                        strError = $"ListTags() error. ErrorInfo={result.ErrorInfo}, ErrorCode={result.ErrorCode}";
                        goto ERROR1;
                    }
#else
                    List<TagAndData> tags = new List<TagAndData>();
                    foreach (var book in RfidTagList.Books)
                    {
                        if (book.OneTag.TagInfo == null)
                            continue;
                        tags.Add(book);
                    }

                    tags.AddRange(RfidTagList.Patrons);

#endif

                    // List<Task> tasks = new List<Task>();
                    bool is_empty = false;
                    bool changed = false;
                    this.Invoke((Action)(() =>
                    {
                        is_empty = this.listView_tags.Items.Count == 0;

                        List<ListViewItem> items = new List<ListViewItem>();
#if OLD_CODE
                        foreach (OneTag tag in result.Results)
#else
                        foreach (TagAndData data in tags)
#endif
                        {
                            token.ThrowIfCancellationRequested();
#if !OLD_CODE
                            var tag = data.OneTag;
#endif

                            Debug.Assert(tag != null, "tag 不应该为 null");
                            if (tag == null)
                                continue;

                            if (this.ProtocolFilter != null
                                && StringUtil.IsInList(tag.Protocol, this.ProtocolFilter) == false)
                                continue;

                            ListViewItem item = FindItem(this.listView_tags,
                                tag.ReaderName,
                                tag.UID);
                            if (item == null)
                            {
                                // 2023/10/25
                                string protocol = tag.Protocol;
                                string uhfProtocol = RfidTagList.GetUhfProtocol(tag.TagInfo);
                                if (uhfProtocol == "gb")
                                    protocol += ":国标";
                                else if (uhfProtocol == "gxlm")
                                    protocol += ":高校联盟";

                                item = new ListViewItem();
                                ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.UID);
                                ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);
                                ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, /*tag.Protocol*/protocol);
                                ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.AntennaID.ToString());
                                item.Tag = new ItemInfo { OneTag = tag };
                                this.listView_tags.Items.Add(item);

                                if (tag.TagInfo == null)
                                {
                                    /*
                                    // 启动单独的线程去填充 .TagInfo
                                    tasks.Add(Task.Run(() => { GetTagInfo(item); }));
                                    */
                                    // 既然是在非界面线程中运行，就安心等待运行完
                                    GetTagInfo(item);
                                }
                                else
                                {
                                    /*
                                    if (TagInfoFilled(item) == false)
                                        tasks.Add(Task.Run(() => { FillTagInfo(item); }));
                                    */
                                    // 既然是在非界面线程中运行，就安心等待运行完
                                    FillTagInfo(item);
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
                                        /*
                                        // 启动单独的线程去填充 .TagInfo
                                        tasks.Add(Task.Run(() => { GetTagInfo(item); }));
                                        */
                                        // 既然是在非界面线程中运行，就安心等待运行完
                                        GetTagInfo(item);
                                    }
                                    else
                                    {
                                        /*
                                        if (TagInfoFilled(item) == false)
                                            tasks.Add(Task.Run(() => { FillTagInfo(item); }));
                                        */
                                        // 既然是在非界面线程中运行，就安心等待运行完
                                        FillTagInfo(item);
                                    }
                                    changed = true;
                                }

                                string old_antenna = ListViewUtil.GetItemText(item, COLUMN_ANTENNA);
                                if (old_antenna != tag.AntennaID.ToString())
                                {
                                    ItemInfo info = (ItemInfo)item.Tag;
                                    if (info != null && info.OneTag != null)
                                        info.OneTag.AntennaID = tag.AntennaID;
                                    ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.AntennaID.ToString());
                                    changed = true;
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
                    if (/*tasks.Count > 0 || */changed)
                    {
                        // _ = Task.Run(async () =>
                        {
                            /*
                            Task.WaitAll(tasks.ToArray());
                            */
                            bool closed = false;

                            try
                            {
                                // 注意：Invoke() 里面抛出的异常，在外部 catch 以后，里面的 StackTrace 会丢失，也就是说看不到 this.Invoke() 里面的调用栈
                                this.Invoke((Action)(() =>
                                {
                                    // 首次填充，自动设好选定状态
                                    // if (is_empty)
                                    {
                                        // MainForm.WriteErrorLog($"=== UpdateChipListAsync() this={this}, this.SelectedID={this.SelectedID}, this.SelectedPII={this.SelectedPII}");

                                        // TODO: 只有当列表发生了实质性刷新的时候，才有必要调用一次 SelectItem。也就是说，不要每秒都无条件调用一次
                                        string id = this.SelectedID != null ? this.SelectedID : this.SelectedPII;
                                        // MainForm.WriteErrorLog($"=== UpdateChipListAsync() id='{id}'");

                                        var ret = SelectItem(id);
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
                            }
                            catch (ObjectDisposedException)
                            {
                                return false;
                            }

                            if (closed == false)
                            {
                                //this.Invoke((Action)(() =>
                                //{
                                await FillEntityInfoAsync(token);
                                //}));

                                if (this._mode.StartsWith("auto_fix_eas"))
                                {
                                    try
                                    {
                                        this.Invoke((Action)(() =>
                                        {
                                            AutoFixEas(
#if OLD_CODE
                                                channel
#endif
                                                );
                                        }));
                                    }
                                    catch (ObjectDisposedException)
                                    {

                                    }
                                }
                            }
                        }
                        // );
                    }
                    else
                    {
                        if (_asked == false && this.AskTag != null)
                        {
                            AskTagEventArgs e = new AskTagEventArgs();
                            this.AskTag(this, e);
                            this.ShowMessage(e.Text, "green", true);
                            _asked = true;
                        }
                    }
                    return true;
                }
                catch (RemotingException ex)
                {
                    strError = "UpdateChipList() 内出现异常: " + ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "UpdateChipList() 内出现异常: " + ExceptionUtil.GetDebugText(ex);    // ex.ToAsyncString(); 
                    MainForm.WriteErrorLog(strError);
                    goto ERROR1;
                }
                finally
                {
#if OLD_CODE
                    ReturnRfidChannel(channel);
#endif
                }

            ERROR1:
                if (show_messageBox)
                    this.ShowMessageBox(strError);
                else
                {
                    this.ShowMessage(strError, "red", true);
                    // this.SetError("updateChipList", strError);
                }
                return false;
            }
            finally
            {
                Interlocked.Decrement(ref _inUpdate);
            }
        }

        bool _asked = false;

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


        private void toolStripButton_loadRfid_Click(object sender, EventArgs e)
        {
            _ = Task.Run(async () => { await UpdateChipListAsync(_cancel.Token, true); });
        }

        class IdInfo
        {
            public string Prefix { get; set; }
            public string Text { get; set; }

            public static IdInfo Parse(string text)
            {
                // 2019/6/18
                if (string.IsNullOrEmpty(text))
                    return null;

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
            // 2020/10/26
            if (info == null)
                return false;

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
            if (_asked == false && this.AskTag != null)
            {
                AskTagEventArgs e = new AskTagEventArgs();
                this.AskTag(this, e);
                this.ShowMessage(e.Text, "green", true);
                _asked = true;
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

        // return:
        //      null    没有错误
        //      其他      错误信息
        string FillTagInfo(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            OneTag tag = item_info.OneTag;
            if (tag.Protocol == InventoryInfo.ISO14443A)
                return null; // 暂时还不支持对 14443A 的卡进行 GetTagInfo() 操作

            if (tag.TagInfo == null)
                return null;

            string strError = "";
            try
            {
                string hex_string = Element.GetHexString(tag.TagInfo.Bytes, "4");

                string chip_parse_error = "";
                try
                {
                    item_info.LogicChipItem = LogicChipItem.FromTagInfo(tag.TagInfo);
                    item_info.LogicChipItem.PropertyChanged += LogicChipItem_PropertyChanged;
                }
                catch (Exception ex)
                {
                    if (item_info != null)
                        item_info.LogicChipItem = null;
                    chip_parse_error = ex.Message;
                }

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

                    if (item_info.LogicChipItem != null)    // 2019/7/6
                    {
                        string pii = item_info.LogicChipItem.PrimaryItemIdentifier;
                        // ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
                        SetItemPIIColumn(item, pii, true);
                        if (this.SelectedPII != null
                            && pii == this.SelectedPII)
                            item.Font = new Font(item.Font, FontStyle.Bold);
                    }
                }));

                if (string.IsNullOrEmpty(chip_parse_error) == false)
                {
                    strError = chip_parse_error;
                    goto ERROR1;
                }
                return null;
            }
            catch (Exception ex)
            {
                strError = "FillTagInfo() 出现异常: " + ex.Message;
                goto ERROR1;
            }

        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
        }

        // TagInfo 是否已经填充过了？ 
        static bool TagInfoFilled(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            OneTag tag = item_info.OneTag;
            if (tag.Protocol == InventoryInfo.ISO14443A)
                return true; // 暂时还不支持对 14443A 的卡进行 GetTagInfo() 操作

            return item_info.LogicChipItem != null;
        }

        // return:
        //      null    正常
        //      其他      报错信息
        string GetTagInfo(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            OneTag tag = item_info.OneTag;
            if (tag.Protocol == InventoryInfo.ISO14443A)
                return null; // 暂时还不支持对 14443A 的卡进行 GetTagInfo() 操作

            /*
            if (tag.UID == "00000000")
                throw new Exception($"UID 错误！");
            */
#if OLD_CODE
            RfidChannel channel = GetRfidChannel(
    out string strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }
#else
            string strError = "";
#endif
            try
            {
                // TODO: 判断重新获取前后的 Protocol 是否一致
#if OLD_CODE
                GetTagInfoResult result = channel.Object.GetTagInfo("*", tag.UID);
#else
                GetTagInfoResult result = RfidManager.GetTagInfo("*", tag.UID, tag.AntennaID);
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                tag.TagInfo = result.TagInfo;

                // string hex_string = Element.GetHexString(result.TagInfo.Bytes, "4");

                string chip_parse_error = "";
                try
                {
                    item_info.LogicChipItem = LogicChipItem.FromTagInfo(result.TagInfo);
                    item_info.LogicChipItem.PropertyChanged += LogicChipItem_PropertyChanged;
                }
                catch (Exception ex)
                {
                    if (item_info != null)
                        item_info.LogicChipItem = null;
                    chip_parse_error = ex.Message;
                }

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

                    if (item_info.LogicChipItem != null)    // 2019/7/6
                    {
                        string pii = item_info.LogicChipItem.PrimaryItemIdentifier;
                        // ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
                        SetItemPIIColumn(item, pii, true);
                        if (this.SelectedPII != null
                            && pii == this.SelectedPII)
                            item.Font = new Font(item.Font, FontStyle.Bold);
                    }
                }));

                if (string.IsNullOrEmpty(chip_parse_error) == false)
                {
                    strError = chip_parse_error;
                    goto ERROR1;
                }
                return null;
            }
            catch (Exception ex)
            {
                strError = "GetTagInfo() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
#if OLD_CODE
                ReturnRfidChannel(channel);
#endif
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
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
            // 2022/5/10
            if (chip == null)
                return;
            this.Invoke((Action)(() =>
            {
                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    ItemInfo tag_info = (ItemInfo)item.Tag;
                    if (tag_info.LogicChipItem == chip
                        && tag_info.LogicChipItem != null)
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
            try
            {
                if (this.listView_tags.SelectedItems.Count == 1)
                {
                    ItemInfo item_info = (ItemInfo)this.listView_tags.SelectedItems[0].Tag;
                    OneTag tag = item_info.OneTag;
                    // var tag_info = tag.TagInfo;

                    this.SelectedTag = tag.Clone(); // 最好是深度拷贝

                    // 2019/9/30
                    // 修正 AntennaID
                    if (this.SelectedTag.TagInfo != null)
                        this.SelectedTag.TagInfo.AntennaID = this.SelectedTag.AntennaID;

                    this.button_OK.Enabled = true;

                    this.chipEditor1.LogicChipItem = item_info.LogicChipItem;

                    if (string.IsNullOrEmpty(item_info.XmlErrorInfo) == false)
                    {
                        BookItem book_item = new BookItem();
                        book_item.Barcode = "error:" + item_info.XmlErrorInfo;
                        this.propertyGrid_record.SelectedObject = book_item;
                    }
                    else if (string.IsNullOrEmpty(item_info.Xml) == false)
                    {
                        BookItem book_item = new BookItem();
                        int nRet = book_item.SetData("",
                            item_info.Xml,
                            null,
                            out string strError);
                        if (nRet == -1)
                        {
                            // 如何报错?
                            book_item.Barcode = "error:" + strError;
                        }

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
            catch (Exception ex)
            {
                this.ShowMessage(ex.Message, "red", true);
            }
        }

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(1);

        // 填充所有的册记录信息
        // TODO: 返回值最好能体现实际是否发生过刷新
        async Task FillEntityInfoAsync(CancellationToken token)
        {
            using (var releaser = await _channelLimit.EnterAsync())
            {
                // TODO: 还要通过 typeOfUsage 判断是否读者证卡，如果是，要用 GetReaderInfo() 获得读者记录
                var items = (List<ListViewItem>)this.Invoke(new Func<List<ListViewItem>>(() =>
                {
                    List<ListViewItem> results = new List<ListViewItem>();
                    results.AddRange(this.listView_tags.Items.Cast<ListViewItem>());
                    return results;
                }));

                foreach (ListViewItem item in items)
                {
                    token.ThrowIfCancellationRequested();

                    ItemInfo item_info = (ItemInfo)item.Tag;

                    // 2020/10/16
                    // 前面已经装载过了
                    if (string.IsNullOrEmpty(item_info.Xml) == false)
                        continue;

                    // 2023/10/25
                    // 先前已经报错了。避免重复请求 dp2library
                    // TODO: 可以为 ListViewItem 实现一个上下文菜单命令，刷新和重新装载 XML
                    if (string.IsNullOrEmpty(item_info.XmlErrorInfo) == false)
                        continue;

                    var tag_info = item_info.OneTag.TagInfo;
                    if (tag_info == null)
                        continue;

                    string pii = "";
                    string oi = "";
                    if (tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // 注1: taginfo.EAS 在调用后可能被修改
                        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                        var chip_info = RfidTagList.GetUhfChipInfo(tag_info);
                        // 2023/11/3
                        if (string.IsNullOrEmpty(chip_info.ErrorInfo) == false)
                            continue;

                        pii = chip_info.PII;
                        oi = chip_info.OI;
                        // TODO: OI?
                    }
                    else
                    {
                        // *** ISO15693 HF

                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        var chip = LogicChip.From(tag_info.Bytes,
                            (int)tag_info.BlockSize);
                        // string pii = chip.FindElement(ElementOID.PII)?.Text;
                        pii = chip.FindElement(ElementOID.PII)?.Text;
                        oi = chip.FindElement(ElementOID.OI)?.Text;
                        if (string.IsNullOrEmpty(oi))
                            oi = chip.FindElement(ElementOID.AOI)?.Text;
                    }

                    if (string.IsNullOrEmpty(pii))
                        continue;

                    /*
                    LibraryChannel channel = this.GetChannel();
                    */
                    var looping = Looping(out LibraryChannel channel);
                    try
                    {
                        token.ThrowIfCancellationRequested();

                        long lRet = channel.GetItemInfo(looping.Progress,
                            BuildUii(pii, oi, null),
                            "xml",
                            out string xml,
                            "",
                            out string biblio,
                            out string strError);

                        if (lRet == -1)
                        {
                            item_info.Xml = xml;
                            // TODO: 给 item 设置出错状态
                            // 注意要防止 “馆外机构”等报错，导致这里被反复执行 API 请求，让 server 非常繁忙
                            item_info.XmlErrorInfo = strError;
                            continue;
                        }
                        else if (lRet == 0)
                        {
                            item_info.Xml = xml;
                            item_info.XmlErrorInfo = string.IsNullOrEmpty(strError) ? "not found" : strError;
                        }
                        else
                        {
                            item_info.Xml = xml;
                            item_info.XmlErrorInfo = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO: 如何报错？让操作者从册信息界面上可以看出报错
                    }
                    finally
                    {
                        looping.Dispose();
                        /*
                        this.ReturnChannel(channel);
                        */
                    }
                }
            }
        }

#if REMOVED

        public class ChipInfo
        {
            public LogicChip Chip { get; set; }

            public string OI { get; set; }

            public string PII { get; set; }

            public string UhfProtocol { get; set; }
        }

        static ChipInfo GetHfChipInfo(TagInfo tag_info)
        {
            ChipInfo result = new ChipInfo();

            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            result.Chip = LogicChip.From(tag_info.Bytes,
                (int)tag_info.BlockSize);
            // string pii = chip.FindElement(ElementOID.PII)?.Text;
            result.PII = GetPIICaption(result.Chip.FindElement(ElementOID.PII)?.Text);

            return result;
        }

        static ChipInfo GetUhfChipInfo(TagInfo taginfo)
        {
            ChipInfo result = new ChipInfo();

            var epc_bank = Element.FromHexString(taginfo.UID);

            if (UhfUtility.IsBlankTag(epc_bank, taginfo.Bytes) == true)
            {
                // 空白标签
                result.PII = GetPIICaption(null);
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
                    result.Chip = parse_result.LogicChip;
                    taginfo.EAS = parse_result.PC.AFI == 0x07;
                    result.UhfProtocol = "gb";
                    result.PII = GetPIICaption(GetPiiPart(parse_result.UII));
                    result.OI = GetOiPart(parse_result.UII, false);
                }
                else
                {
                    // *** 高校联盟 UHF
                    var parse_result = GaoxiaoUtility.ParseTag(
        epc_bank,
        taginfo.Bytes);
                    if (parse_result.Value == -1)
                        throw new Exception(parse_result.ErrorInfo);
                    result.Chip = parse_result.LogicChip;
                    taginfo.EAS = !parse_result.EpcInfo.Lending;
                    result.UhfProtocol = "gxlm";
                    result.PII = GetPIICaption(GetPiiPart(parse_result.EpcInfo.PII));
                    result.OI = GetOiPart(parse_result.EpcInfo.PII, false);
                }
            }

            return result;
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
        
#endif

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

        // 自动修复 EAS
        void AutoFixEas(
#if OLD_CODE
            RfidChannel channel
#endif
            )
        {
            string strError = "";


            try
            {
                // 注：如果 info == null，表示对每一个 List View Item 都尝试去修复一下
                // TODO: 注意修复后刷新显示
                IdInfo info = IdInfo.Parse(this.SelectedID);
                List<string> uids = new List<string>();
                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    string uid = ListViewUtil.GetItemText(item, COLUMN_UID);

                    ItemInfo item_info = (ItemInfo)item.Tag;

                    if (item_info.EasChecked)
                        continue;

                    var tag_info = item_info.OneTag.TagInfo;
                    if (tag_info == null)
                        goto CONTINUE;

                    string pii = "";
                    if (tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // 注1: taginfo.EAS 在调用后可能被修改
                        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                        var chip_info = RfidTagList.GetUhfChipInfo(tag_info);
                        if (string.IsNullOrEmpty(chip_info.ErrorInfo) == false)
                            goto ERROR1;

                        pii = chip_info.PII;
                        // oi = chip_info.OI;
                        // TODO: OI?
                    }
                    else
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        LogicChip chip = LogicChip.From(tag_info.Bytes,
                        (int)tag_info.BlockSize);
                        pii = chip.FindElement(ElementOID.PII)?.Text;
                    }

                    if (info == null
                        || (info.Prefix == "pii" && pii == info.Text)
                        || (info.Prefix == "uid" && uid == info.Text))
                    {
                        // 获得册记录的外借状态。
                        // return:
                        //      -2  册记录为空，无法判断状态
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
                        SetEasResult result = null;
                        // TODO: 这里发现不一致的时候，是否要出现明确提示，让操作者知晓？
                        // TODO: 要迫使界面刷新，因为 EAS 值可能发生了变化
                        if (nRet == 1 && tag_info.EAS == true)
#if OLD_CODE
                            result = SetEAS(channel, "*", "uid:" + tag_info.UID, false, out strError);
#else
                        {
                            result = RfidManager.SetEAS("*",
                                "uid:" + tag_info.UID,
                                tag_info.AntennaID,
                                false);
                            if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                tag_info.UID = result.ChangedUID;
                            RfidTagList.SetEasData(
                                tag_info.UID,
                                false);
                        }
#endif
                        else if (nRet == 0 && tag_info.EAS == false)
#if OLD_CODE
                            result = SetEAS(channel, "*", "uid:" + tag_info.UID, true, out strError);
#else
                        {
                            result = RfidManager.SetEAS("*",
                                "uid:" + tag_info.UID,
                                tag_info.AntennaID,
                                true);
                            if (string.IsNullOrEmpty(result.ChangedUID) == false)
                            {
                                tag_info.UID = result.ChangedUID;
                                item_info.OneTag.UID = result.ChangedUID;
                            }
                            RfidTagList.SetEasData(
                                tag_info.UID,
                                true);
                        }
#endif
                        else
                        {
                            this.EasFixed = true;
                            goto CONTINUE;
                        }

                        uids.Add(tag_info.UID);


                        // if (tag.TagInfo == null)
                        {
                            // 启动单独的线程去填充 .TagInfo
                            Task.Run(() => { GetTagInfo(item); }).ContinueWith((o) =>
                            {
                                this.Invoke((Action)(() =>
                                {
                                    // 如果当前右侧显示了标签内容，也需要刷新
                                    listView_tags_SelectedIndexChanged(this, new EventArgs());
                                }));
                            });
                        }

                        if (result.Value == -1)
                        {
                            strError = $"{result.ErrorInfo}, error_code={result.ErrorCode}";
                            goto ERROR1;
                        }

                        this.EasFixed = true;
                    }

                CONTINUE:
                    item_info.EasChecked = true;    // 避免以后重复检查
                }

                if (uids.Count > 0)
                    this.ShowMessage($"UID 为 {StringUtil.MakePathList(uids, ",")} 的标签 EAS 状态不正确，已经被自动纠正", "yellow", true);
            }
            finally
            {
            }

            if (StringUtil.IsInList("auto_fix_eas_and_close", this._mode)
                && this.EasFixed)
                this.Close();
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        // 获得册记录的外借状态。
        // return:
        //      -2  册记录为空，无法判断状态
        //      -1  出错
        //      0   没有被外借
        //      1   在外借状态
        public static int GetCirculationState(string strItemXml, out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strItemXml))
            {
                strError = "册记录 XML 为空，无法判断外借状态";
                return -2;
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

#if OLD_CODE

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

#endif



        void OpenRfidCapture(bool open)
        {
#if OLD_CODE
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
#else
            RfidManager.EnableSendkey(open);
#endif
        }

        #endregion

        class ItemInfo
        {
            public OneTag OneTag { get; set; }
            public string Xml { get; set; }

            public string XmlErrorInfo { get; set; }

            public LogicChipItem LogicChipItem { get; set; }

            // EAS 是否被检查过。检查过就不要重复检查了
            public bool EasChecked { get; set; }
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
            if (this.PauseRfid)
                return;
            _ = UpdateChipListAsync(_cancel.Token, false);
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
                strError = "您选择的行尚未获得 TagInfo。请稍后重试";
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

            menuItem = new MenuItem("清除标签缓存 (&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearTagsCache_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_tags, new Point(e.X, e.Y));
        }

        void menu_clearTagsCache_Click(object sender, EventArgs e)
        {
            RfidTagList.ClearTagTable(null);
        }

        void menu_test_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                ItemInfo item_info = (ItemInfo)item.Tag;

                // item_info.LogicChipItem = new LogicChipItem();
                SetContent(item_info.LogicChipItem);
                item_info.LogicChipItem?.SetChanged(true);
            }
        }

        // 测试写入一些内容
        static void SetContent(LogicChip chip)
        {
            chip.SetElement(ElementOID.PII, "1234567890");
            chip.SetElement(ElementOID.SetInformation, "1203");
            chip.SetElement(ElementOID.ShelfLocation, "QA268.L55");
            chip.SetElement(ElementOID.OwnerInstitution, "US-InU-Mu");
            chip.SetElement(ElementOID.LocalDataA, "1234567890");
            chip.SetElement(ElementOID.LocalDataB, "1234567890");
            chip.SetElement(ElementOID.LocalDataC, "1234567890");
            chip.SetElement(ElementOID.Title, "1234567890 1234567890 1234567890");
            chip.SetElement(ElementOID.AOI, "1234567890");
            chip.SetElement(ElementOID.SOI, "1234567890");
            chip.SetElement(ElementOID.AIBI, "1234567890");
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
                {
                    text.Append("\r\n[LogicChipItem 为空]\r\n");
                    text.Append(item_info.OneTag.GetDescription());
                }
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
            var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

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
                var clear_result = await ClearTagContentAsync(item, !control);
                if (clear_result.Value == -1)
                {
                    ShowMessageBox("清除标签时出错: " + clear_result.ErrorInfo);
                    break;
                }
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());
        }

        async Task<NormalResult> ClearTagContentAsync(
    ListViewItem item,
    bool lock_as_error)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                return await ClearUhfTagContentAsync(item, lock_as_error);
            else
                return await ClearHfTagContentAsync(item, lock_as_error);
        }

        // parameters:
        //      lock_as_error   == true 如果有锁定块，则不清除，报错返回; == false 有锁定块依然会执行清除
        async Task<NormalResult> ClearHfTagContentAsync(
            ListViewItem item,
            bool lock_as_error)
        {
#if OLD_CODE
            RfidChannel channel = GetRfidChannel(
    out string strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }
#else
            string strError = "";
#endif

            try
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                var old_tag_info = item_info.OneTag.TagInfo;

                // 检查标签是否有 block 被锁定
                if (lock_as_error
                    && old_tag_info.LockStatus != null
                    && old_tag_info.LockStatus.Contains("l"))
                {
                    strError = $"标签 {old_tag_info.UID} 有被锁定的块({old_tag_info.LockStatus})，放弃进行清除";
                    goto ERROR1;
                }

                var new_tag_info = old_tag_info.Clone();
                // 制造一套空内容
                {
                    new_tag_info.AFI = 0;
                    new_tag_info.DSFID = 0;
                    new_tag_info.EAS = false;
                    /*
                    List<byte> bytes = new List<byte>();
                    for (int i = 0; i < new_tag_info.BlockSize * new_tag_info.MaxBlockCount; i++)
                    {
                        bytes.Add(0);
                    }
                    new_tag_info.Bytes = bytes.ToArray();
                    new_tag_info.LockStatus = "";
                    */
                    // 对 byte[] 内容执行清除。锁定的块不会被清除
                    new_tag_info.Bytes = LogicChip.ClearBytes(new_tag_info.Bytes,
                        new_tag_info.BlockSize,
                        new_tag_info.MaxBlockCount,
                        new_tag_info.LockStatus);
                }
#if OLD_CODE
                var result = channel.Object.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
#else
                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
    old_tag_info,
    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);
                }
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError != null)
                    goto ERROR1;
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "ClearHfTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
#if OLD_CODE
                ReturnRfidChannel(channel);
#endif
            }
        ERROR1:
            /*
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            */
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // parameters:
        //      lock_as_error   == true 如果有锁定块，则不清除，报错返回; == false 有锁定块依然会执行清除
        async Task<NormalResult> ClearUhfTagContentAsync(
            ListViewItem item,
            bool lock_as_error)
        {
            string strError = "";

            try
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                var old_tag_info = item_info.OneTag.TagInfo;

                /*
                // 检查标签是否有 block 被锁定
                if (lock_as_error
                    && old_tag_info.LockStatus != null
                    && old_tag_info.LockStatus.Contains("l"))
                {
                    strError = $"标签 {old_tag_info.UID} 有被锁定的块({old_tag_info.LockStatus})，放弃进行清除";
                    goto ERROR1;
                }
                */

                var new_tag_info = old_tag_info.Clone();
                // 制造一套空内容
                {
                    new_tag_info.AFI = 0;
                    new_tag_info.DSFID = 0;
                    new_tag_info.EAS = false;
                    /*
                    List<byte> bytes = new List<byte>();
                    for (int i = 0; i < new_tag_info.BlockSize * new_tag_info.MaxBlockCount; i++)
                    {
                        bytes.Add(0);
                    }
                    new_tag_info.Bytes = bytes.ToArray();
                    new_tag_info.LockStatus = "";
                    */
                    /*
                    // 对 byte[] 内容执行清除。锁定的块不会被清除
                    new_tag_info.Bytes = new byte[old_tag_info.Bytes.Length];
                    for(int i =0;i<new_tag_info.Bytes.Length;i++)
                    {
                        new_tag_info.Bytes[i] = 0;
                    }
                    */

                    new_tag_info.UID = UhfUtility.EpcBankHex(UhfUtility.BuildBlankEpcBank()); // "0000" + Element.GetHexString(UhfUtility.BuildBlankEpcBank());
                    new_tag_info.Bytes = null;  // 这样可使得 User Bank 被清除
                }
                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
    old_tag_info,
    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);
                }

                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }


                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError != null)
                    goto ERROR1;

                /*
                // 注: 这里不用专门去刷新。自然会被刷新。
                // 专门刷新很困难，因为 UHF 标签 EPC 内容修改后，暂时无法获知其 UID，最好是等待自动探测刷新
                */
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "ClearUhfTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


        async void menu_saveSelectedTagContent_Click(object sender, EventArgs e)
        {
            await SaveItemChangeAsync(async (item) => await SaveTagContentAsync(item));
#if OLD
            int count = 0;
            List<string> errors = new List<string>();
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                var error = await SaveTagContent(item);
                if (error != null)
                    errors.Add(error);
                else
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            if (count > 0)
                this.ShowMessage($"保存成功({count}) 错误({errors.Count})", errors.Count == 0 ? "green" : "yellow", true);
            //else
            //    this.ShowMessage("没有需要保存的事项", "yellow", true);

            if (errors.Count > 0)
                MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "保存出错");
#endif
        }

        delegate Task<string> Delegate_save(ListViewItem item);

        async Task SaveItemChangeAsync(Delegate_save proc)
        {
            int count = 0;
            List<string> errors = new List<string>();
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                var error = await proc(item);
                if (error != null)
                    errors.Add(error);
                else
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            // if (count > 0)
            this.ShowMessage($"保存完全成功:{count} {(errors.Count > 0 ? ("警告或错误:" + errors.Count) : "")}",
                errors.Count == 0 ? "green" : "yellow", true);
            //else
            //    this.ShowMessage("没有需要保存的事项", "yellow", true);

            if (errors.Count > 0)
                MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "警告或错误");
        }

        async void menu_saveSelectedErrorTagContent_1_Click(object sender, EventArgs e)
        {
            await SaveItemChangeAsync(async (item) => await SaveBlankPiiTagContentAsync(item));

#if OLD
            int count = 0;
            List<string> errors = new List<string>();

            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                var error = await SaveBlankPiiTagContent(item);
                if (error != null)
                    errors.Add(error);
                else
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            this.ShowMessage($"保存成功({count}) 错误({errors.Count})", errors.Count == 0 ? "green" : "yellow", true);
            if (errors.Count > 0)
                MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "保存出错");
#endif
        }

        // 故意写入 PII 为空的标签内容
        async Task<string> SaveBlankPiiTagContentAsync(ListViewItem item)
        {
            string strError = "";
            ItemInfo item_info = (ItemInfo)item.Tag;
            /*
            if (item_info.LogicChipItem == null)
            {
                // return false;
                strError = "item_info.LogicChipItem == null";
                goto ERROR1;
            }
            */

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;

                TagInfo new_tag_info = null;
                if (item_info.LogicChipItem != null)
                {
                    // 删除 PII
                    item_info.LogicChipItem.RemoveElement(ElementOID.PII);

                    new_tag_info = BuildNewHfTagInfo(
        old_tag_info,
        item_info.LogicChipItem);
                }
                else
                {
                    new_tag_info = old_tag_info.Clone();
                    // 对 byte[] 内容执行清除。锁定的块不会被清除
                    new_tag_info.Bytes = LogicChip.ClearBytes(new_tag_info.Bytes,
                        new_tag_info.BlockSize,
                        new_tag_info.MaxBlockCount,
                        new_tag_info.LockStatus);
                }

                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);
                }
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError == null)
                {
                    UpdateChanged(item_info.LogicChipItem);
                    return null;
                }
                else
                    strError = $"保存成功。重新读入时出错: {strError}";
                return strError;
            }
            catch (Exception ex)
            {
                strError = "SaveBlankPiiTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
        }


        async void menu_saveSelectedErrorTagContent_Click(object sender, EventArgs e)
        {
            await SaveItemChangeAsync(async (item) => await SaveErrorTagContent1Async(item));
#if OLD
            int count = 0;
            List<string> errors = new List<string>();
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                var error = await SaveErrorTagContent1(item);
                if (error != null)
                    errors.Add(error);
                else
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            this.ShowMessage($"保存成功({count}) 错误({errors.Count})", errors.Count == 0 ? "green" : "yellow", true);
            if (errors.Count > 0)
                MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "保存出错");
#endif
        }

        // 故意写入可导致解析错误的标签内容(fudan 空白标签)
        // return:
        //      null    没有出错
        //      其他      出错信息
        async Task<string> SaveErrorTagContent1Async(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            /*
            if (item_info.LogicChipItem == null)
                return false;
            */

            string strError = "";

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;
                var new_tag_info = old_tag_info.Clone();
                /*
                var new_tag_info = BuildNewTagInfo(
    old_tag_info,
    item_info.LogicChipItem);
                */
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

                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);

                }
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError == null)
                {
                    UpdateChanged(item_info.LogicChipItem);
                    return null;
                }
                else
                    strError = $"保存成功。重新读入时出错: {strError}";
                return strError;
            }
            catch (Exception ex)
            {
                strError = "SaveErrorTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
        }

        // 故意写入可导致解析错误的标签内容
        // return:
        //      null    没有出错
        //      其他      出错信息
        async Task<string> SaveErrorTagContentAsync(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            /*
            if (item_info.LogicChipItem == null)
                return false;
            */
            string strError = "";

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;
                var new_tag_info = old_tag_info.Clone();
                /*
                var new_tag_info = BuildNewTagInfo(
    old_tag_info,
    item_info.LogicChipItem);
                */
                {
                    List<byte> temp = new List<byte>();
                    for (int i = 0; i < 50; i++)
                    {
                        temp.Add((byte)200);
                    }
                    new_tag_info.Bytes = temp.ToArray();
                }

                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);

                }
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError == null)
                {
                    UpdateChanged(item_info.LogicChipItem);
                    return null;
                }
                else
                    strError = $"保存成功。重新读入时出错: {strError}";
                return strError;
            }
            catch (Exception ex)
            {
                strError = "SaveErrorTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
        }

        async Task<string> SaveTagContentAsync(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                return await SaveUhfTagContentAsync(item);
            else
                return await SaveHfTagContentAsync(item);
        }

        // 保存超高频标签内容
        async Task<string> SaveUhfTagContentAsync(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.LogicChipItem == null)
                return "item_info.LogicChipItem == null";
            if (item_info.LogicChipItem.Changed == false)
                return "没有发生修改";

            string strError = "";

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;

                // 询问是否写入 User Bank
                bool build_user_bank = true;
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    var ret = this.TryGet(() =>
                    {
                        return ListDialog.GetInput(
                            this,
                            "请选择写入超高频标签时",
                            "是否要写入 User Bank",
                            new string[] { "要写入", "不写入" },
                            0,
                            this.Font);
                    });
                    if (ret == "要写入")
                        build_user_bank = true;
                    else if (ret == "不写入")
                        build_user_bank = false;
                    else
                    {
                        strError = "放弃保存标签内容";
                        goto ERROR1;
                    }
                }

                var new_tag_info = BuildWritingTagInfo(old_tag_info,
                    item_info.LogicChipItem,
                    item_info.LogicChipItem.EAS,
                    "auto", // gb/gxlm/auto
                    (initial_format) =>
                    {
                        // 如果是空白标签，需要弹出对话框提醒选择格式
                        var ret = this.TryGet(() =>
                        {
                            return ListDialog.GetInput(
                                this,
                                "请选择写入超高频标签的内容格式",
                                "请选择一个内容格式",
                                new string[] { "国标", "高校联盟" },
                                initial_format == "gb" ? 0 : 1,
                                this.Font);
                        });
                        if (ret == "国标")
                            return "gb";
                        if (ret == "高校联盟")
                            return "gxlm";
                        return null;
                    },
                    (new_format, old_format) =>
                    {
                        string warning = $"警告：即将用{GetUhfFormatCaption(new_format)}格式覆盖原有{GetUhfFormatCaption(old_format)}格式";
                        DialogResult dialog_result = this.TryGet(() =>
                        {
                            return MessageBox.Show(this,
    $"{warning}\r\n\r\n确实要覆盖？",
    $"RfidToolForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                        });
                        if (dialog_result == DialogResult.Yes)
                            return true;
                        return false;
                    },
                    (chip, element) =>
                    {
                        if (build_user_bank == false)
                        {
                            var dlg_result = MessageBox.Show(this,
    "刚才您选择了不写入超高频标签的 User Bank，然而即将写入的内容又有 OI(机构代码) 元素。\r\n\r\n请问是否继续写入? (警告: 继续写入会导致 OI 元素发生丢失)",
    "RfidToolForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                            if (dlg_result == DialogResult.No)
                                return false;
                        }
                        return true;
                    },
                    build_user_bank);

                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);
                }
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError == null)
                {
                    UpdateChanged(item_info.LogicChipItem);
                    return null;
                }
                else
                    strError = $"保存成功。重新读入时出错: {strError}";
                return strError;
            }
            catch (Exception ex)
            {
                strError = "SaveUhfTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
        }

        static void UpdateUID(ItemInfo item_info,
            string uid)
        {
            if (item_info.OneTag != null)
                item_info.OneTag.UID = uid;

            if (item_info.OneTag != null && item_info.OneTag.TagInfo != null)
                item_info.OneTag.TagInfo.UID = uid;
        }

        public static string GetUhfFormatCaption(string format)
        {
            if (format == "gb")
                return "国标";
            if (format == "gxlm")
                return "高校联盟";
            throw new ArgumentException($"无法识别的格式名 '{format}'");
        }

        // return:
        //      null    没有出错
        //      其他      出错信息
        async Task<string> SaveHfTagContentAsync(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.LogicChipItem == null)
                return "item_info.LogicChipItem == null";
            if (item_info.LogicChipItem.Changed == false)
                return "没有发生修改";

#if OLD_CODE
            RfidChannel channel = GetRfidChannel(
    out string strError);
            if (channel == null)
            {
                strError = $"GetRfidChannel() error: {strError}";
                goto ERROR1;
            }
#else
            string strError = "";
#endif

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;
                var new_tag_info = BuildNewHfTagInfo(
    old_tag_info,
    item_info.LogicChipItem);
#if OLD_CODE
                var result = channel.Object.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
#else
                RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
                    old_tag_info,
                    new_tag_info);
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);

                }
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                strError = await Task.Run(() => { return GetTagInfo(item); });
                if (strError == null)
                {
                    UpdateChanged(item_info.LogicChipItem);
                    return null;
                }
                else
                    strError = $"保存成功。重新读入时出错: {strError}";
                return strError;
            }
            catch (Exception ex)
            {
                strError = "SaveHfTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
#if OLD_CODE
                ReturnRfidChannel(channel);
#endif
            }
        ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // 把 item 修改为红色背景，表示出错的状态
                SetItemColor(item, "error");
            }));
            return strError;
        }


        static TagInfo BuildNewHfTagInfo(TagInfo old_tag_info,
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

#if REMOVED
        static TagInfo BuildNewUhfTagInfo(TagInfo old_tag_info,
LogicChipItem chip)
        {
            if (chip.Protocol.Contains(":") == false)
            {
                // 需要弹出对话框询问，是要写入 gb 还是 gxlm 格式
            }


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
#endif

        // 询问超高频内容格式
        // parameters:
        //      initial_format  选择对话框初始显示的格式。为 null/gb/html 之一
        // return:
        //      null    放弃选择
        //      其它      返回选择的格式。为 gb/gxlm 之一
        public delegate string delegate_askUhfDataFormat(string initial_format);

        // 询问是否覆盖不同内容格式的标签
        // parameters:
        //      new_format  新格式。为 gb/gxlm 之一
        //      old_format  老格式。为 gb/gxlm 之一
        // return:
        //      true    要覆盖
        //      false   放弃覆盖
        public delegate bool delegate_askOverwriteDifference(string new_format, string old_format);

        // 构造写入用的 TagInfo
        // TODO: 要留意 高校联盟 格式的 TOU 写入前翻译是否正确
        public static TagInfo BuildWritingTagInfo(TagInfo existing,
LogicChip chip,
bool eas,
string uhfProtocol = "auto", // gb/gxlm/auto/select auto 表示自动探测格式，select 表示强制弹出对话框选择格式
delegate_askUhfDataFormat func_askFormat = null,
delegate_askOverwriteDifference func_askOverwrite = null,
delegate_warningRemoveOI func_askRemoveOI = null,
bool build_user_bank = true)
        {
            if (existing.Protocol == InventoryInfo.ISO15693)
            {
                // SetTypeOfUsage("", chip, "gb");

                TagInfo new_tag_info = existing.Clone();
                new_tag_info.Bytes = chip.GetBytes(
                    (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                    (int)new_tag_info.BlockSize,
                    LogicChip.GetBytesStyle.None,
                    out string block_map);
                new_tag_info.LockStatus = block_map;
                new_tag_info.DSFID = LogicChip.DefaultDSFID;  // 图书
                new_tag_info.SetEas(eas);
                return new_tag_info;
            }

            if (existing.Protocol == InventoryInfo.ISO18000P6C)
            {
                /*
                // 读者卡和层架标必须有 User Bank，不然 TU 字段没有地方放
                if (build_user_bank == false
    && this.TypeOfUsage != "10")
                    throw new Exception($"{GetCaption(this.TypeOfUsage)}必须写入 User Bank");
                */

                // TODO: 判断标签内容是空白/国标/高校联盟格式，采取不同的写入格式
                /*
高校联盟格式
国标格式
* */


                var isExistingGB = UhfUtility.IsISO285604Format(Element.FromHexString(existing.UID), existing.Bytes);

                if (uhfProtocol == "auto")
                {
                    var epc_bank = Element.FromHexString(existing.UID);
                    if (UhfUtility.IsBlankTag(epc_bank, existing.Bytes) == true)
                    {
                        /*
                        // 如果是空白标签，需要弹出对话框提醒选择格式
                        var result = this.TryGet(() =>
                        {
                            return ListDialog.GetInput(
this,
$"请选择写入超高频标签的内容格式",
"请选择一个内容格式",
new string[] { "国标", "高校联盟" },
isExistingGB ? 0 : 1,
this.Font);
                        });
                        */
                        if (func_askFormat == null)
                            throw new ArgumentException($"当需要选择格式时，func_askFormat 为 null");

                        var result = func_askFormat.Invoke(isExistingGB ? "gb" : "gxlm");

                        if (result == null)
                            throw new InterruptException("放弃写入标签");
                        if (result == "gb")
                            uhfProtocol = "gb";
                        else
                            uhfProtocol = "gxlm";
                    }
                    else
                    {
                        if (isExistingGB)
                            uhfProtocol = "gb";
                        else
                            uhfProtocol = "gxlm";
                    }
                }
                else if (uhfProtocol == "select")
                {
                    /*
                    var result = this.TryGet(() =>
                    {
                        return ListDialog.GetInput(
this,
$"请选择写入超高频标签的内容格式",
"请选择一个内容格式",
new string[] { "国标", "高校联盟" },
isExistingGB ? 0 : 1,
this.Font);
                    });
                    */
                    if (func_askFormat == null)
                        throw new ArgumentException($"当需要选择格式时，func_askFormat 为 null");

                    var result = func_askFormat.Invoke(isExistingGB ? "gb" : "gxlm");

                    if (result == null)
                        throw new InterruptException("放弃写入标签");
                    if (result == "gb")
                        uhfProtocol = "gb";
                    else
                        uhfProtocol = "gxlm";
                }

                // 2023/11/6
                // 过滤不需要的元素
                if (build_user_bank)
                {
                    FilterUserBankElements(chip, uhfProtocol);
#if REMOVED
                    var elements = Program.MainForm.UhfUserBankElements;
                    List<Element> all = new List<Element>(chip.Elements);
                    foreach (var element in all)
                    {
                        // "SetInformation,OwnerInstitution,TypeOfUsage,ShelfLocation"
                        if (element.OID == ElementOID.SetInformation
                            && StringUtil.IsInList("SetInformation", elements) == false)
                            chip.RemoveElement(element.OID);

                        else if (element.OID == ElementOID.OwnerInstitution
    && StringUtil.IsInList("OwnerInstitution", elements) == false
    && uhfProtocol == "gxlm"/*国标不让去除 OI*/)
                            chip.RemoveElement(element.OID);

                        else if (element.OID == ElementOID.TypeOfUsage
    && StringUtil.IsInList("TypeOfUsage", elements) == false)
                            chip.RemoveElement(element.OID);

                        else if (element.OID == ElementOID.ShelfLocation
    && StringUtil.IsInList("ShelfLocation", elements) == false)
                            chip.RemoveElement(element.OID);
                    }
#endif
                }
                else
                {
                    if (uhfProtocol == "gxlm")
                    {
                        if (RemoveOI(chip, func_askRemoveOI) == false)
                            throw new Exception("放弃写入");
                    }
                }

                TagInfo new_tag_info = existing.Clone();
                if (uhfProtocol == "gxlm")
                {
                    // 写入高校联盟数据格式
                    if (isExistingGB)
                    {
                        /*
                        string warning = $"警告：即将用高校联盟格式覆盖原有国标格式";
                        DialogResult dialog_result = MessageBox.Show(this,
$"{warning}\r\n\r\n确实要覆盖？",
$"RfidToolForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (dialog_result == DialogResult.No)
                            throw new Exception("放弃写入");
                        */
                        var ret = func_askOverwrite.Invoke("gxlm", "gb");
                        if (ret == false)
                            throw new Exception("放弃写入");
                    }

                    // SetTypeOfUsage(chip, "gxlm");

                    var result = GaoxiaoUtility.BuildTag(chip, build_user_bank, eas);
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = UhfUtility.EpcBankHex(result.EpcBank);    //  existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                else
                {
                    // 写入国标数据格式
                    if (isExistingGB == false)
                    {
                        /*
                        string warning = $"警告：即将用国标格式覆盖原有高校联盟格式";
                        DialogResult dialog_result = MessageBox.Show(this,
$"{warning}\r\n\r\n确实要覆盖？",
$"ScanDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (dialog_result == DialogResult.No)
                            throw new Exception("放弃写入");
                        */
                        var ret = func_askOverwrite.Invoke("gb", "gxlm");
                        if (ret == false)
                            throw new Exception("放弃写入");
                    }
                    // SetTypeOfUsage(chip, "gb");

                    var result = UhfUtility.BuildTag(chip,
                        true,
                        eas ? "afi_eas_on" : "");
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = UhfUtility.EpcBankHex(result.EpcBank);  // existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                return new_tag_info;
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
        }

        // 过滤掉 User Bank 中的不需要的元素。
        // Program.MainForm.UhfUserBankElements 中定义了不该被过滤的元素名列表。其余的元素名就应该被过滤掉
        static void FilterUserBankElements(LogicChip chip,
            string uhfProtocol)
        {
            var elements = Program.MainForm.UhfUserBankElements;
            List<Element> all = new List<Element>(chip.Elements);
            foreach (var element in all)
            {
                // 2023/11/9
                // 顺便把空内容的元素删除
                if (string.IsNullOrEmpty(element.Text))
                    chip.RemoveElement(element.OID);

                // "SetInformation,OwnerInstitution,TypeOfUsage,ShelfLocation"
                else if (element.OID == ElementOID.SetInformation
                    && StringUtil.IsInList("SetInformation", elements) == false)
                    chip.RemoveElement(element.OID);

                else if (element.OID == ElementOID.OwnerInstitution
&& StringUtil.IsInList("OwnerInstitution", elements) == false
&& uhfProtocol == "gxlm"/*国标不让去除 OI*/)
                    chip.RemoveElement(element.OID);

                else if (element.OID == ElementOID.TypeOfUsage
&& StringUtil.IsInList("TypeOfUsage", elements) == false)
                    chip.RemoveElement(element.OID);

                else if (element.OID == ElementOID.ShelfLocation
&& StringUtil.IsInList("ShelfLocation", elements) == false)
                    chip.RemoveElement(element.OID);

            }
        }

        // return:
        //      true    同意
        //      false   不同意，并终止处理
        public delegate bool delegate_warningRemoveOI(LogicChip chip, Element element);

        // 过滤掉 OI 元素。只针对 高校联盟格式。
        // 这是为了避免在“不写入 User Bank”情况下后面 BuildTag 时发生矛盾
        static bool RemoveOI(LogicChip chip,
            delegate_warningRemoveOI func_askRemoveOI)
        {
            List<Element> all = new List<Element>(chip.Elements);
            foreach (var element in all)
            {
                if (element.OID == ElementOID.OwnerInstitution
                    || element.OID == ElementOID.AOI
                    || (int)element.OID == 27)
                {
                    if (func_askRemoveOI != null)
                    {
                        if (func_askRemoveOI(chip, element) == false)
                            return false;
                    }
                    chip.RemoveElement(element.OID);
                }
            }

            return true;
        }


        // 设置 TU 字段。注意 国标和高校联盟的取值表完全不同
        // parameters:
        //      data_format gb/gxlm
        void SetTypeOfUsage(
                    string tou,
                    LogicChip chip,
                    string data_format)
        {
            // string tou = this.TypeOfUsage;
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


        private async void toolStripButton_saveRfid_Click(object sender, EventArgs e)
        {
            await SaveItemChangeAsync(async (item) => await SaveTagContentAsync(item));

#if OLD
            int count = 0;
            List<string> errors = new List<string>();
            foreach (ListViewItem item in this.listView_tags.Items)
            {
                var error = await SaveTagContent(item);
                if (error != null)
                    errors.Add(error);
                else
                    count++;
            }

            listView_tags_SelectedIndexChanged(this, new EventArgs());

            UpdateSaveButton();

            this.ShowMessage($"保存成功({count}) 错误({errors.Count})", errors.Count == 0 ? "green" : "yellow", true);
            if (errors.Count > 0)
                MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "保存出错");
#endif
        }

        private void toolStripButton_autoFixEas_CheckedChanged(object sender, EventArgs e)
        {
            StringUtil.SetInList(ref this._mode, "auto_fix_eas", this.toolStripButton_autoFixEas.Checked);
        }

        public bool PauseRfid = true;

        private void RfidToolForm_Activated(object sender, EventArgs e)
        {
            // RfidManager.Pause = false;
            this.PauseRfid = false;
        }

        private void RfidToolForm_Deactivate(object sender, EventArgs e)
        {
            this.PauseRfid = true;
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
