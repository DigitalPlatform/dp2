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

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.RFID.UI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using static DigitalPlatform.RFID.RfidTagList;
using DigitalPlatform.CirculationClient;
using Accord.IO;
using static DigitalPlatform.RFID.UhfUtility;

namespace dp2Circulation
{
    public partial class RfidToolForm : MyForm
    {
        // 是否作为选择 RFID 标签的对话框打开。此种打开方式下，AutoRefresh 和 AutoFixEas 都不会沿用以前的状态
        public bool DialogMode
        {
            get
            {
                return StringUtil.IsInList("dialog", _mode);
            }
            set
            {
                StringUtil.SetInList(ref this._mode, "dialog", value);
            }
        }

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
        const int COLUMN_RSSI = 5;

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

            if (this.DialogMode == false)
            {
                this.toolStripButton_autoRefresh.Checked = Program.MainForm.AppInfo.GetBoolean("rfidtoolform",
                    "auto_refresh",
                    true);
            }

            if (this.DialogMode)
                RfidManager.CallInventory("");

            if (this.AutoRefresh == false)
            {
#if REMOVED
                _ = Task.Factory.StartNew(async () =>
                {
                    await UpdateChipListAsync(_cancel.Token, true);
                },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
#endif
                _ = Task.Run(async () =>
                {
                    await UpdateChipListAsync(_cancel.Token, true);
                });
            }

            if (this.DialogMode == false)
            {
                this.toolStripButton_autoFixEas.Checked = Program.MainForm.AppInfo.GetBoolean("rfidtoolform",
        "auto_fix_eas",
        true);
            }

            _errorTable = new ErrorTable((s) =>
            {
                // 2023/11/26 从 Invoke() 改为 BeginInvoke()
                this.BeginInvoke((Action)(() =>
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
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var count = this.ChangedItemCount;
                if (count > 0)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
        $"当前有 {count} 个标签的信息被修改后尚未保存。\r\n\r\n确实要关闭窗口? ",
        "RfidToolForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        e.Cancel = true;
                }
            }
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

            if (this.DialogMode == false)
            {
                Program.MainForm.AppInfo.SetBoolean("rfidtoolform",
                    "auto_refresh",
                    this.toolStripButton_autoRefresh.Checked);

                Program.MainForm.AppInfo.SetBoolean("rfidtoolform",
                    "auto_fix_eas",
                    this.toolStripButton_autoFixEas.Checked);
            }
        }

        // 新 Tag 到来、变化、消失
        private async void MainForm_TagChanged(object sender, TagChangedEventArgs e)
        {
            if (this.PauseRfid)
                return;

            try
            {
                /*
                bool auto_refresh = (bool)this.Invoke((Func<bool>)(() =>
                {
                    return this.toolStripButton_autoRefresh.Checked;
                }));
                */
                bool auto_refresh = this.AutoRefresh;

                if (auto_refresh)
                    await UpdateChipListAsync(_cancel.Token, false);

                if (auto_refresh && e.UpdateRssiTags != null && e.UpdateRssiTags.Count > 0)
                    await UpdateRssiAsync(e.UpdateRssiTags, _cancel.Token);

#if REMOVED
                if (AutoRefresh)
                    BeginUpdateChipList(e);
#endif
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

#if REMOVED
        void BeginUpdateChipList(TagChangedEventArgs e)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                await UpdateChipListAsync(_cancel.Token, false);
                if (e.UpdateRssiTags != null && e.UpdateRssiTags.Count > 0)
                    await UpdateRssiAsync(e.UpdateRssiTags, _cancel.Token);
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }
#endif

        // 2023/11/13
        // 更新 RSSI 显示
        async Task<bool> UpdateRssiAsync(
            List<OneTag> tags,
            CancellationToken token)
        {
            /*
            if (this.AutoRefresh == false)
                return false;
            */
            int nRet = Interlocked.Increment(ref _inUpdate);
            try
            {
                if (nRet != 1)
                    return false;

                string strError = "";
                if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl))
                {
                    strError = "尚未配置 RFID 中心 URL";
                    goto ERROR1;
                }

                try
                {
                    bool is_empty = false;
                    bool changed = false;
                    this.Invoke((Action)(() =>
                    {
                        is_empty = this.listView_tags.Items.Count == 0;
                        this.listView_tags.BeginUpdate();
                        foreach (var tag in tags)
                        {
                            token.ThrowIfCancellationRequested();

                            Debug.Assert(tag != null, "tag 不应该为 null");
                            if (tag == null)
                                continue;

                            if (this.ProtocolFilter != null
                                && StringUtil.IsInList(tag.Protocol, this.ProtocolFilter) == false)
                                continue;

                            ListViewItem item = FindItem(this.listView_tags,
                                tag.ReaderName,
                                tag.UID);
                            if (item != null)
                            {
                                // 2023/11/12
                                // 刷新 RSSI
                                string old_rssi = ListViewUtil.GetItemText(item, COLUMN_RSSI);
                                string new_rssi = tag.RSSI.ToString();
                                if (old_rssi != new_rssi)
                                    ListViewUtil.ChangeItemText(item, COLUMN_RSSI, new_rssi);

#if REMOVED
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

#endif
                            }

                            changed = true;
                        }
                        this.listView_tags.EndUpdate();
                    }));

                    return true;
                }
                catch (RemotingException ex)
                {
                    strError = "UpdateRssiAsync() 内出现异常: " + ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "UpdateRssiAsync() 内出现异常: " + ExceptionUtil.GetDebugText(ex);    // ex.ToAsyncString(); 
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
                bool show_messageBox = false;
                if (show_messageBox)
                    this.ShowMessageBox(strError);
                else
                {
                    this.ShowMessage(strError, "red", true);
                }
                return false;
            }
            finally
            {
                Interlocked.Decrement(ref _inUpdate);
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
                                {
                                    if (GaoxiaoUtility.IsWhdt(ByteArray.GetTimeStampByteArray(tag.UID)))
                                        protocol += ":望湖洞庭";
                                    else
                                        protocol += ":高校联盟";
                                }

                                item = new ListViewItem();
                                ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.UID);
                                ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);
                                ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, /*tag.Protocol*/protocol);
                                ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.AntennaID.ToString());
                                ListViewUtil.ChangeItemText(item, COLUMN_RSSI, tag.RSSI.ToString());

                                /*
                                // 2023/11/24
                                // 让 TagInfo 不可变
                                if (tag.TagInfo != null)
                                    tag.TagInfo = tag.TagInfo.Clone();
                                */

                                item.Tag = new ItemInfo(tag);   // { OneTag = tag };
                                this.listView_tags.Items.Add(item);

                                if (tag.TagInfo == null
                                    || UhfNeedGetUserBank(tag.TagInfo))
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
                                // 2023/11/12
                                // 刷新 RSSI
                                string old_rssi = ListViewUtil.GetItemText(item, COLUMN_RSSI);
                                string new_rssi = tag.RSSI.ToString();
                                if (old_rssi != new_rssi)
                                    ListViewUtil.ChangeItemText(item, COLUMN_RSSI, new_rssi);

                                // 刷新 readerName
                                string old_readername = ListViewUtil.GetItemText(item, COLUMN_READERNAME);
                                if (old_readername != tag.ReaderName)
                                {
                                    // ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);

                                    /*
                                    // 2023/11/24
                                    // 让 TagInfo 不可变
                                    if (tag.TagInfo != null)
                                        tag.TagInfo = tag.TagInfo.Clone();
                                    */

                                    item.Tag = new ItemInfo(tag);  // { OneTag = tag };
                                    if (tag.TagInfo == null
                                    || UhfNeedGetUserBank(tag.TagInfo))
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
                                    {
                                        // info.OneTag.AntennaID = tag.AntennaID;
                                        info.AntennaID = tag.AntennaID; // ???
                                    }
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

                                var list = this._mode?.Split(',');
                                if (/*this._mode.StartsWith("auto_fix_eas")*/
                                    list.Where(o => o.StartsWith("auto_fix_eas")).Any())
                                {
                                    try
                                    {
                                        DoAutoFixEas();
#if REMOVED
                                        this.Invoke((Action)(() =>
                                        {
                                            AutoFixEas(
#if OLD_CODE
                                                channel
#endif
                                                );
                                        }));
#endif
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

        // 是否有必要获取 UHF 标签的 User Bank?
        static bool UhfNeedGetUserBank(TagInfo taginfo)
        {
            if (taginfo == null)
                return true;
            if (taginfo.Protocol == InventoryInfo.ISO18000P6C
                && taginfo.Bytes == null)
            {
                var epc_bytes = ByteArray.GetTimeStampByteArray(taginfo.UID);
                var parse_result = UhfUtility.ParsePC(epc_bytes, 2);
                if (parse_result.UMI == true)
                    return true;
            }

            return false;
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
#if REMOVED
            _ = Task.Factory.StartNew(async () =>
            {
                await UpdateChipListAsync(_cancel.Token, true);
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
#endif
            _ = Task.Run(async () =>
            {
                await UpdateChipListAsync(_cancel.Token, true);
            });
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
                /*
                OneTag tag = item_info.OneTag;
                if (// tag.ReaderName == reader_name && 
                    tag.UID == uid)
                    return item;
                */
                if (item_info.UID == uid)
                    return item;    // ???
            }

            return null;
        }

        // return:
        //      null    没有错误
        //      其他      错误信息
        string FillTagInfo(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            var tag = item_info.OneTag;
            if (tag.Protocol == InventoryInfo.ISO14443A)
                return null; // 暂时还不支持对 14443A 的卡进行 GetTagInfo() 操作

            if (tag.TagInfo == null)
                return null;

            if (tag.TagInfo != null)
            {
                Debug.Assert(tag.UID == tag.TagInfo.UID, $"FillTagInfo(ListViewItem) tag.UID({tag.UID}) != tag.TagInfo.UID({tag.TagInfo.UID})");
            }

            string strError = "";
            try
            {
                // string hex_string = Element.GetHexString(tag.TagInfo.Bytes, "4");

                string chip_parse_error = "";
                try
                {
                    item_info.SetLogicChipItem(LogicChipItem.FromTagInfo(tag.TagInfo.CloneTagInfo()));  // ???
                    item_info.LogicChipItem.PropertyChanged += LogicChipItem_PropertyChanged;
                }
                catch (TagDataException ex)
                {
                    if (item_info != null)
                        item_info.SetLogicChipItem(null);

                    if (SetBlankPii(item,
                        tag.UID,
                        tag.TagInfo?.Bytes) == false)
                        chip_parse_error = ex.Message;
                }
                catch (ArgumentException ex)
                {
                    if (item_info != null)
                        item_info.SetLogicChipItem(null);

                    if (SetBlankPii(item,
                        tag.UID,
                        tag.TagInfo?.Bytes) == false)
                        chip_parse_error = ex.Message;
                }
                catch (Exception ex)
                {
                    if (item_info != null)
                        item_info.SetLogicChipItem(null);
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
                        // string pii = item_info.LogicChipItem.PrimaryItemIdentifier;
                        string pii = item_info.LogicChipItem.GetUII();
                        SetItemPIIColumn(item, pii, true);
                        if (this.SelectedPII != null
                            && pii == this.SelectedPII)
                            item.Font = new Font(item.Font, FontStyle.Bold);

                        item_info.LogicChipItem.UID = tag.TagInfo.UID;
                    }

                    // UID 栏
                    {
                        string new_uid = tag.TagInfo.UID;
                        string old_uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                        if (old_uid != new_uid)
                            ListViewUtil.ChangeItemText(item, COLUMN_UID, new_uid);
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

        bool SetBlankPii(ListViewItem item,
            // OneTag tag
            string uid,
            byte[] user_bank)
        {
            /*
            if (tag.TagInfo == null)
                return false;
            */
            if (uid == null)
                return false;
            byte[] epc_bytes = ByteArray.GetTimeStampByteArray(/*tag.UID*/uid);

            if (epc_bytes == null
                || epc_bytes.Length < 4)
                return false;
            if (UhfUtility.IsBlankEpcBank(epc_bytes))
            {
                // 跳过 4 个 byte
                List<byte> bytes = new List<byte>(epc_bytes);
                bytes.RemoveRange(0, 4);
                this.Invoke((Action)(() =>
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_PII, $"(空白){ByteArray.GetHexTimeStampString(bytes.ToArray()).ToUpper()}");
                    // 把 item 修改为红色背景，表示出错的状态
                    SetItemColor(item, "normal");
                }));
                return true;
            }
            return false;
        }

        // TagInfo 是否已经填充过了？ 
        static bool TagInfoFilled(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            var tag = item_info.OneTag;
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
            var tag = item_info.OneTag;
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
                GetTagInfoResult result = RfidManager.GetTagInfo("*",
                    tag.UID,
                    tag.AntennaID,
                    null,
                    "");
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                // tag.TagInfo = result.TagInfo;
                item_info.SetTagInfo(result.TagInfo, false); // ???

                if (tag.TagInfo != null)
                {
                    Debug.Assert(tag.UID == tag.TagInfo.UID, $"GetTagInfo(ListViewItem) tag.UID({tag.UID}) != tag.TagInfo.UID({tag.TagInfo.UID})");
                }

                // string hex_string = Element.GetHexString(result.TagInfo.Bytes, "4");

                string chip_parse_error = "";
                try
                {
                    item_info.SetLogicChipItem(LogicChipItem.FromTagInfo(tag.TagInfo.CloneTagInfo()));  // ???
                    item_info.LogicChipItem.PropertyChanged += LogicChipItem_PropertyChanged;
                }
                catch (Exception ex)
                {
                    if (item_info != null)
                        item_info.SetLogicChipItem(null);
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
                        // string pii = item_info.LogicChipItem.PrimaryItemIdentifier;
                        string pii = item_info.LogicChipItem.GetUII();
                        SetItemPIIColumn(item, pii, true);
                        if (this.SelectedPII != null
                            && pii == this.SelectedPII)
                            item.Font = new Font(item.Font, FontStyle.Bold);

                        item_info.LogicChipItem.UID = tag.TagInfo.UID;
                    }

                    // UID 栏
                    {
                        string new_uid = tag.TagInfo.UID;
                        string old_uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                        if (old_uid != new_uid)
                            ListViewUtil.ChangeItemText(item, COLUMN_UID, new_uid);
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
#if REMOVED
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
#endif
            var count = ChangedItemCount;
            this.Invoke((Action)(() =>
            {
                if (count > 0)
                    this.toolStripButton_saveRfid.Enabled = true;
                else
                    this.toolStripButton_saveRfid.Enabled = false;
            }));
        }

        public int ChangedItemCount
        {
            get
            {
                return this.TryGet(() =>
                {
                    int count = 0;
                    foreach (ListViewItem item in this.listView_tags.Items)
                    {
                        ItemInfo tag_info = (ItemInfo)item.Tag;
                        if (tag_info.LogicChipItem != null
                        && tag_info.LogicChipItem.Changed == true)
                            count++;
                    }
                    return count;
                });
            }
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
                    ItemInfo item_info = (ItemInfo)item.Tag;
                    if (item_info.LogicChipItem == chip
                        && item_info.LogicChipItem != null)
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

                        if (item_info.LogicChipItem.Changed)
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
                        // string pii = item_info.LogicChipItem.FindElement(ElementOID.PII)?.Text;
                        string pii = item_info.LogicChipItem.GetUII();
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
                    var tag = item_info.OneTag;
                    // var tag_info = tag.TagInfo;

                    if (tag.TagInfo != null)
                    {
                        // 比较两个 UID
                        Debug.Assert(tag.UID == tag.TagInfo.UID, $"tag.UID({tag.UID}) != tag.TagInfo.UID({tag.TagInfo.UID})");
                    }

                    if (item_info.LogicChipItem != null)
                    {
                        // 比较两个 UID
                        Debug.Assert(tag.UID == item_info.LogicChipItem.UID, $"tag.UID({tag.UID}) != item_info.LogicChipItem.UID({item_info.LogicChipItem.UID})");
                    }

                    // this.SelectedTag = tag.Clone(); // 最好是深度拷贝
                    this.SelectedTag = tag.CloneOneTag();   // ???

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
                        var chip_info = RfidTagList.GetUhfChipInfo(tag_info.CloneTagInfo());    // ???
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

                        try
                        {
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            LogicChip chip = LogicChip.From(tag_info.Bytes,
                                (int)tag_info.BlockSize);
                            // string pii = chip.FindElement(ElementOID.PII)?.Text;
                            pii = chip.FindElement(ElementOID.PII)?.Text;
                            oi = chip.FindElement(ElementOID.OI)?.Text;
                            if (string.IsNullOrEmpty(oi))
                                oi = chip.FindElement(ElementOID.AOI)?.Text;
                        }
                        catch(TagDataException ex)
                        {
                            // 2025/9/25
                            this.Invoke((Action)(() =>
                            {
                                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + ex.Message);
                                // 把 item 修改为红色背景，表示出错的状态
                                SetItemColor(item, "error");
                            }));
                        }
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

                        var uii = BuildUii(pii, oi, null);
                        long lRet = channel.GetItemInfo(looping.Progress,
                            uii,
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
                            item_info.XmlErrorInfo = $"册记录 '{uii}' {strError}";
                            continue;
                        }
                        else if (lRet == 0)
                        {
                            item_info.Xml = xml;
                            item_info.XmlErrorInfo = string.IsNullOrEmpty(strError) ? $"册记录 '{uii}' 没有找到" : $"册记录 '{uii}' {strError}";
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

        public static string BuildUii(string pii, string oi, string aoi)
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
        void DoAutoFixEas(
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
                List<string> errors = new List<string>();

                var items = ListViewUtil.GetItems(this.listView_tags);

                foreach (ListViewItem item in /*this.listView_tags.Items*/items)
                {
                    string uid = ListViewUtil.GetItemText(item, COLUMN_UID);

                    ItemInfo item_info = (ItemInfo)item.Tag;

                    if (item_info.EasChecked)
                        continue;

                    var tag_info = item_info.OneTag.TagInfo;
                    if (tag_info == null)
                        goto CONTINUE;

                    // 2025/2/28
                    // 如果是读者卡，不处理
                    {
                        if (tag_info.Protocol == InventoryInfo.ISO14443A)
                            goto CONTINUE;
                        var typeOfUsage = item_info.LogicChipItem?.TypeOfUsage;
                        // var typeOfUsage = item_info.LogicChipItem?.FindElement(ElementOID.TypeOfUsage)?.Text;
                        if (string.IsNullOrEmpty(typeOfUsage) == false
                            && typeOfUsage.StartsWith("8"))
                            goto CONTINUE;
                    }


                    // 比较两个 UID
                    Debug.Assert(tag_info.UID == item_info.OneTag.UID, $"DoAutoFixEas() tag_info.UID({tag_info.UID}) != item_info.OneTag.UID({item_info.OneTag.UID})");

                    string pii = "";
                    if (tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // 注1: taginfo.EAS 在调用后可能被修改
                        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                        var chip_info = RfidTagList.GetUhfChipInfo(tag_info.CloneTagInfo());    // ???
                        if (string.IsNullOrEmpty(chip_info.ErrorInfo) == false)
                            goto ERROR1;

                        pii = chip_info.PII;
                        // oi = chip_info.OI;
                        // TODO: OI?
                    }
                    else
                    {
                        try
                        {
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            LogicChip chip = LogicChip.From(tag_info.Bytes,
                            (int)tag_info.BlockSize);
                            pii = chip.FindElement(ElementOID.PII)?.Text;
                        }
                        catch(TagDataException ex)
                        {
                            // 2025/9/25
                            item_info.EasChecked = true;
                            goto ERROR1;
                        }
                    }

                    // 把 tag_info.EAS 设置到位
                    var tag_info_eas = RfidTagList.SetTagInfoEAS(tag_info.CloneTagInfo());

                    if (info == null
                        || (info.Prefix == "pii" && pii == info.Text)
                        || (info.Prefix == "uid" && uid == info.Text))
                    {
                        // 2025/9/25
                        if (string.IsNullOrEmpty(item_info.XmlErrorInfo) == false)
                        {
                            errors.Add($"{item_info.XmlErrorInfo}。无法完成 EAS 修正");
                            goto CONTINUE;
                        }
                        // 获得册记录的外借状态。
                        // return:
                        //      -2  册记录为空，无法判断状态
                        //      -1  出错
                        //      0   没有被外借
                        //      1   在外借状态
                        int nRet = GetCirculationState(item_info.Xml,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                        {
                            errors.Add(strError);
                            goto CONTINUE;
                        }

                        // 便于观察
                        // Application.DoEvents();
                        // Thread.Sleep(2000);

                        // 检测 EAS 是否正确
                        SetEasResult result = null;
                        var enable = false;
                        // TODO: 这里发现不一致的时候，是否要出现明确提示，让操作者知晓？
                        // TODO: 要迫使界面刷新，因为 EAS 值可能发生了变化
                        if (nRet == 1 && tag_info_eas == true)
                        {
                            enable = false;
#if REMOVED
                            result = RfidManager.SetEAS("*",
                                "uid:" + tag_info.UID,
                                tag_info.AntennaID,
                                false);
                            if (result.Value == 1)
                            {
                                if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                {
                                    // tag_info.UID = result.ChangedUID;
                                    // item_info.OneTag.UID = result.ChangedUID;
                                    item_info.UID = result.ChangedUID;  // ???
                                }

                                if (string.IsNullOrEmpty(result.OldUID) == false
                                    && result.Value == 1)
                                {
                                    if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                        TaskList.ChangeUID(result.OldUID, result.ChangedUID);
                                    else
                                        TaskList.ChangeUID(result.OldUID, "off");
                                }
                                /*
                                RfidTagList.SetEasData(
                                    tag_info.UID,
                                    false);
                                */
                            }
#endif
                        }
                        else if (nRet == 0 && tag_info_eas == false)
                        {
                            enable = true;
#if REMOVED
                            // return result.Value:
                            //      -1  出错
                            //      0   没有找到指定的标签
                            //      1   找到，并成功修改 EAS
                            result = RfidManager.SetEAS("*",
                                "uid:" + tag_info.UID,
                                tag_info.AntennaID,
                                true);
                            if (result.Value == 1)
                            {
                                if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                {
                                    // tag_info.UID = result.ChangedUID;
                                    // item_info.OneTag.UID = result.ChangedUID;
                                    item_info.UID = result.ChangedUID;  // ???
                                }

                                if (string.IsNullOrEmpty(result.OldUID) == false
                                    && result.Value == 1)
                                {
                                    if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                        TaskList.ChangeUID(result.OldUID, result.ChangedUID);
                                    else
                                        TaskList.ChangeUID(result.OldUID, "on");
                                }
                                /*
                                RfidTagList.SetEasData(
                                    tag_info.UID,
                                    true);
                                */
                            }
#endif
                        }
                        else
                        {
                            this.EasFixed = true;
                            goto CONTINUE;
                        }

                        {
                            result = RfidManager.SetEAS("*",
                                "uid:" + tag_info.UID,
                                tag_info.AntennaID,
                                enable);
                            if (result.Value == 1)
                            {
                                if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                {
                                    // tag_info.UID = result.ChangedUID;
                                    // item_info.OneTag.UID = result.ChangedUID;
                                    item_info.UID = result.ChangedUID;  // ???
                                }

                                if (string.IsNullOrEmpty(result.OldUID) == false
                                    && result.Value == 1)
                                {
                                    if (string.IsNullOrEmpty(result.ChangedUID) == false)
                                        TaskList.ChangeUID(result.OldUID, result.ChangedUID);
                                    else
                                        TaskList.ChangeUID(result.OldUID, enable ? "on" : "off");
                                }
                                /*
                                RfidTagList.SetEasData(
                                    tag_info.UID,
                                    enable);
                                */
                            }
                        }

                        uids.Add(tag_info.UID);


                        // if (tag.TagInfo == null)
                        {
                            // 2023/11/25 改进写法
                            // 启动单独的线程去填充 .TagInfo
                            _ = Task.Run(() =>
                            {
                                GetTagInfo(item);
                                this.Invoke((Action)(() =>
                                {
                                    // 如果当前右侧显示了标签内容，也需要刷新
                                    listView_tags_SelectedIndexChanged(this, new EventArgs());
                                }));
                            });

                            /*
                            // 启动单独的线程去填充 .TagInfo
                            Task.Run(() => { GetTagInfo(item); }).ContinueWith((o) =>
                            {
                                this.Invoke((Action)(() =>
                                {
                                    // 如果当前右侧显示了标签内容，也需要刷新
                                    listView_tags_SelectedIndexChanged(this, new EventArgs());
                                }));
                            });
                            */
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
                    this.ShowMessage($"下列 UID 的标签 EAS 状态不正确，已经被自动纠正\r\n{StringUtil.MakePathList(uids, "\r\n")}", "yellow", true);
                // 2023/11/25
                if (errors.Count > 0)
                    this.ShowMessageBox($"自动修正中遇到如下报错: \r\n{StringUtil.MakePathList(errors, "\r\n")}");
            }
            finally
            {
            }

            if (StringUtil.IsInList("auto_fix_eas_and_close", this._mode)
                && this.EasFixed)
            {
                this.TryInvoke(() =>
                {
                    this.Close();
                });
            }
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

        // TODO: SetTagInfo() 或修改 PII 的时候，根据需要较正 .EAS 成员值
        class ReadonlyTagInfo
        {
            public string Protocol
            {
                get
                {
                    return _tagInfo?.Protocol;
                }
            }
            public string UID
            {
                get
                {
                    return _tagInfo?.UID;
                }
            }

            public string ReaderName
            {
                get
                {
                    return _tagInfo?.ReaderName;
                }
            }

            public byte[] Bytes
            {
                get
                {
                    return _tagInfo?.Bytes;
                }
            }

            public uint BlockSize
            {
                get
                {
                    if (_tagInfo == null)
                        return 0;
                    return _tagInfo.BlockSize;
                }
            }

            public uint MaxBlockCount
            {
                get
                {
                    if (_tagInfo == null)
                        return 0;
                    return _tagInfo.MaxBlockCount;
                }
            }

            public uint AntennaID
            {
                get
                {
                    if (_tagInfo == null)
                        return 0;
                    return _tagInfo.AntennaID;
                }
            }

            public byte AFI
            {
                get
                {
                    if (_tagInfo == null)
                        return 0;
                    return _tagInfo.AFI;
                }
            }

            public byte DSFID
            {
                get
                {
                    if (_tagInfo == null)
                        return 0;
                    return _tagInfo.DSFID;
                }
            }

            public bool EAS
            {
                get
                {
                    if (_tagInfo == null)
                        return false;
                    return _tagInfo.EAS;
                }
            }

            public string LockStatus
            {
                get
                {
                    return _tagInfo?.LockStatus;
                }
            }

            TagInfo _tagInfo = null;

            public ReadonlyTagInfo(TagInfo taginfo)
            {
                _tagInfo = taginfo;
            }

            public TagInfo CloneTagInfo()
            {
                return _tagInfo?.Clone();
            }
        }

        class ReadonlyOneTag
        {
            public string UID
            {
                get
                {
                    return _oneTag?.UID;
                }
            }

            public string Protocol
            {
                get
                {
                    return _oneTag?.Protocol;
                }
            }

            public string ReaderName
            {
                get
                {
                    return _oneTag?.ReaderName;
                }
            }

            public uint AntennaID
            {
                get
                {
                    if (_oneTag == null)
                        return 0;
                    return _oneTag.AntennaID;
                }
            }

            public ReadonlyTagInfo TagInfo
            {
                get
                {
                    if (_oneTag == null || _oneTag.TagInfo == null)
                        return null;
                    return new ReadonlyTagInfo(_oneTag.TagInfo);
                }
            }

            OneTag _oneTag = null;

            public ReadonlyOneTag(OneTag onetag)
            {
                _oneTag = onetag;
            }

            void SetOneTag(OneTag onetag)
            {
                _oneTag = onetag;
            }

            public OneTag CloneOneTag()
            {
                return _oneTag?.Clone();
            }
        }

        class ItemInfo
        {
            // public OneTag OneTag { get; set; }

            public uint AntennaID
            {
                get
                {
                    if (_oneTag == null)
                        return 0;
                    return _oneTag.AntennaID;
                }
                set
                {
                    _oneTag.AntennaID = value;
                    if (_oneTag.TagInfo != null)
                        _oneTag.TagInfo.AntennaID = value;
                }
            }

            public string UID
            {
                get
                {
                    return _oneTag.UID;
                }
                set
                {
                    _oneTag.UID = value;
                    if (_oneTag.TagInfo != null)
                        _oneTag.TagInfo.UID = value;
                    if (this.LogicChipItem != null)
                        this.LogicChipItem.UID = value;
                }
            }

            OneTag _oneTag = null;
            public ReadonlyOneTag OneTag
            {
                get
                {
                    return new ReadonlyOneTag(_oneTag);
                }
            }

            public ItemInfo(OneTag onetag)
            {
                _oneTag = onetag?.Clone();
            }

            public void SetOneTag(OneTag onetag)
            {
                _oneTag = onetag.Clone();
            }

            public void SetTagInfo(TagInfo taginfo, bool clone)
            {
                if (clone)
                    _oneTag.TagInfo = taginfo.Clone();
                else
                    _oneTag.TagInfo = taginfo;

                _oneTag.UID = _oneTag.TagInfo.UID;
                _oneTag.ReaderName = _oneTag.TagInfo.ReaderName;
                _oneTag.AntennaID = _oneTag.TagInfo.AntennaID;
                _oneTag.Protocol = _oneTag.TagInfo.Protocol;

                if (this.LogicChipItem != null)
                    this.LogicChipItem.UID = _oneTag.UID;
            }

            public string Xml { get; set; }

            public string XmlErrorInfo { get; set; }

            LogicChipItem _logicChipItem = null;
            public LogicChipItem LogicChipItem
            {
                get
                {
                    return _logicChipItem;
                }
            }
            public void SetLogicChipItem(LogicChipItem chip)
            {
                this._logicChipItem = chip;
            }

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
            // 注: _inUpdate 可以防止叠加调用
#if REMOVED
            _ = Task.Factory.StartNew(async () =>
            {
                await UpdateChipListAsync(_cancel.Token, false);
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
#endif

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
                && (this.SelectedTag.TagInfo == null || UhfNeedGetUserBank(this.SelectedTag.TagInfo))
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
        void SetItemPIIColumn(ListViewItem item, string pii, bool clearColor)
        {
            if (string.IsNullOrEmpty(pii))
            {
                var item_info = item.Tag as ItemInfo;
                var ret = SetBlankPii(item,
                    item_info.UID/*item_info.OneTag.UID*/,
                    item_info.OneTag?.TagInfo?.Bytes);
                if (ret == true)
                    return;
                pii = "(空白)";
            }
            ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);

            if (clearColor)
                SetItemColor(item, "normal");
        }

        // 获得一个 ListViewItem 的 PII 列文字
        static string GetItemPII(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info != null && item_info.LogicChipItem != null)
            {
                // return item_info.LogicChipItem.PrimaryItemIdentifier;
                return item_info.LogicChipItem.GetUII();
            }
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

            menuItem = new MenuItem("解释标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_describe_Click);
            if (this.listView_tags.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清空标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearSelectedTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0
                || CountingSaveableItems(this.listView_tags.SelectedItems.Cast<ListViewItem>(), "ISO15693,ISO18000P6C") == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("写入原始 HEX 内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_createSelectedTagHexContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0
                || CountingSaveableItems(this.listView_tags.SelectedItems.Cast<ListViewItem>(), "ISO18000P6C") == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("保存标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0
                || CountingSaveableItems(this.listView_tags.SelectedItems.Cast<ListViewItem>()) == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            if (StringUtil.IsDevelopMode())
            {
                menuItem = new MenuItem("测试");
                menuItem.Click += new System.EventHandler(this.menu_test_Click);
                if (this.listView_tags.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            /*
            {
                menuItem = new MenuItem("解释“高校联盟”标签 ...");
                menuItem.Click += new System.EventHandler(this.menu_describeGaoxiaoTag_Click);
                if (this.listView_tags.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }
            */

            menuItem = new MenuItem("测试创建错误的标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedErrorTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0
                || CountingSaveableItems(this.listView_tags.SelectedItems.Cast<ListViewItem>(), "ISO15693") == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("测试创建 PII 为空的标签内容 [" + this.listView_tags.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveBlankPiiTagContent_Click);
            if (this.listView_tags.SelectedItems.Count == 0
                || CountingSaveableItems(this.listView_tags.SelectedItems.Cast<ListViewItem>(), "ISO15693") == 0)
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

        // 解释高校联盟标签
        void menu_describeGaoxiaoTag_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_tags.SelectedItems.Count == 0)
            {
                strError = "请选择要解释的标签";
                goto ERROR1;
            }

            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                strError = GetUhfTagRowInfo(this.listView_tags.SelectedItems[0],
        out byte[] user_bank,
        out string epc_bank_hex);
                if (strError != null)
                    goto ERROR1;

                text.AppendLine($"{(i + 1)})");

                var epc_bank = ByteArray.GetTimeStampByteArray(epc_bank_hex);

                var parse_result = GaoxiaoUtility.ParseTag(epc_bank,
                    user_bank,
                    "");
                if (parse_result.Value == -1)
                    text.AppendLine($"解析过程出错: {parse_result.ErrorInfo}");
                else
                    text.AppendLine(parse_result.GetDescription(epc_bank, user_bank));

                i++;
            }
            MessageDlg.Show(this, text.ToString(), "解释文字");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

        // 针对选定的标签，创建描述文字
        void menu_describe_Click(object sender, EventArgs e)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (ListViewItem item in this.listView_tags.SelectedItems)
            {
                /*
                if (text.Length > 0)
                    text.Append("\r\n***\r\n");
                */
                text.AppendLine($"{(i + 1)})");

                ItemInfo item_info = (ItemInfo)item.Tag;

                // 超高频
                if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    text.Append(GetUhfTagDescription(item_info.OneTag));    // ???
                }
                else
                {
                    if (item_info.LogicChipItem == null)
                    {
                        text.Append("\r\n[LogicChipItem 为空]\r\n");
                        text.Append(item_info.OneTag.CloneOneTag().GetDescription());   // ???
                    }
                    else
                        text.Append(item_info.LogicChipItem.GetDescription());
                }

                i++;
            }

            MessageDlg.Show(this, text.ToString(), "解释文字");
            // Clipboard.SetDataObject(text.ToString(), true);
        }

        static string GetUhfTagDescription(ReadonlyOneTag tag)
        {
            StringBuilder text = new StringBuilder();

            var taginfo = tag.TagInfo;
            var user_bank = taginfo.Bytes;
            var epc_bank = ByteArray.GetTimeStampByteArray(tag.UID);

            {
                text.AppendLine("=== PC ===");
                text.AppendLine($"Hex(十六进制内容):\t{ByteArray.GetHexTimeStampString(epc_bank.ToList().GetRange(2, 2).ToArray())?.ToUpper()}");

                var pc = UhfUtility.ParsePC(epc_bank, 2);
                text.AppendLine(pc.ToString());
            }

            {
                text.AppendLine("=== 尝试按照高校联盟格式解析 ===");

                var parse_result = GaoxiaoUtility.ParseTag(epc_bank,
        user_bank,
        "");
                if (parse_result.Value == -1)
                    text.AppendLine($"解析过程出错: {parse_result.ErrorInfo}");
                else
                    text.Append(parse_result.GetDescription(epc_bank, user_bank));
            }

            {
                text.AppendLine("=== 尝试按照国标格式解析 ===");

                var parse_result = UhfUtility.ParseTag(epc_bank,
        user_bank,
        4);
                if (parse_result.Value == -1)
                    text.AppendLine($"解析过程出错: {parse_result.ErrorInfo}");
                else
                    text.Append(parse_result.GetDescription(epc_bank, user_bank));
            }

            // 尝试获得 TID Bank
            {
                text.AppendLine("=== TID Bank ===");
                GetTagInfoResult result = RfidManager.GetTagInfo("*",
    tag.UID,
    tag.AntennaID,
    null,
    "tid");
                if (result.Value == -1)
                    text.AppendLine($"error: {result.ErrorInfo}");
                else
                {
                    // TODO: 核对两个 bytes 是否完全一致，不一致则报错
                    // tag.TagInfo = result.TagInfo;   // 使用重新获得的数据

                    if (result.TagInfo.Tag != null)
                    {
                        var bytes = result.TagInfo.Tag as byte[];
                        text.AppendLine($"Hex(十六进制内容):\t{ByteArray.GetHexTimeStampString(bytes)?.ToUpper()} ({bytes?.Length}bytes)");
                    }
                }
            }

            return text.ToString();
        }


#if REMOVED
        static string GetUhfTagDescription(ReadonlyOneTag tag)
        {
            StringBuilder text = new StringBuilder();

            // 尝试获得 TID Bank
            {
                GetTagInfoResult result = RfidManager.GetTagInfo("*",
    tag.UID,
    tag.AntennaID,
    null,
    "tid");
                if (result.Value == -1)
                    text.Append($"TID:\terror:{result.ErrorInfo}\r\n");
                else
                {
                    // TODO: 核对两个 bytes 是否完全一致，不一致则报错
                    // tag.TagInfo = result.TagInfo;   // 使用重新获得的数据
                }

                if (result.TagInfo.Tag != null)
                {
                    var bytes = result.TagInfo.Tag as byte[];
                    text.Append($"TID:\t{ByteArray.GetHexTimeStampString(bytes).ToUpper()} ({bytes?.Length}bytes)\r\n");
                }
            }

            var taginfo = tag.TagInfo;
            text.Append($"UID:\t{tag.UID}\r\n");
            text.Append($"AFI:\t{Element.GetHexString(taginfo.AFI)}\r\n");
            text.Append($"DSFID:\t{Element.GetHexString(taginfo.DSFID)}\r\n");
            text.Append($"EAS:\t{taginfo.EAS}\r\n");

            if (taginfo.Bytes != null)
            {
                text.Append($"\r\nUser Bank 字节内容:\r\n{ByteArray.GetHexTimeStampString(taginfo.Bytes)}\r\n");
            }
            else
                text.Append($"\r\nUser Bank 字节内容:\r\n(null)\r\n");

            var chip_info = RfidTagList.GetUhfChipInfo(taginfo.CloneTagInfo(), "");    // 注: 不对高校联盟的元素内容映射到 GB
            if (string.IsNullOrEmpty(chip_info.ErrorInfo) == false)
                text.AppendLine($"解析标签内容时出错: {chip_info.ErrorInfo}");
            // text.Append($"\r\n锁定位置:\r\n{this.OriginLockStatus}\r\n\r\n");

            /*
            if (taginfo.Bytes != null)
            {
                text.Append($"初始 User Bank 元素:(共 {chip_info.Chip.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in chip_info.Chip.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }
            */

            if (chip_info.Chip != null)
            {
                text.Append($"当前 Chip 元素:(共 {chip_info.Chip.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in chip_info.Chip.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }

            return text.ToString();
        }
#endif

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllItems(this.listView_tags);
        }

        // TODO: 增加对 ISO15693 协议的支持
        // 写入原始 HEX 内容
        void menu_createSelectedTagHexContent_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_tags.SelectedItems.Count == 0)
            {
                strError = "请选择要创建覆盖的标签";
                goto ERROR1;
            }

            // 从第一个 ListViewItem 对象获得数据
            strError = GetUhfTagRowInfo(this.listView_tags.SelectedItems[0],
    out byte[] user_bank,
    out string epc_bank_hex);
            if (strError != null)
                goto ERROR1;

            DuplicateUhfTagDialog dlg = new DuplicateUhfTagDialog();
            dlg.EpcBankHex = epc_bank_hex;
            dlg.UserBankHex = ByteArray.GetHexTimeStampString(user_bank)?.ToUpper(); ;
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var result = MessageBox.Show(this,
$"创建标签内容，会覆盖当前标签的原有内容。\r\n\r\n确实要覆盖? ",
"RfidToolForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            epc_bank_hex = dlg.EpcBankHex;
            string user_bank_hex = dlg.UserBankHex;

            _ = Task.Factory.StartNew(async () =>
            {
                var items = ListViewUtil.GetSelectedItems(this.listView_tags);
                foreach (ListViewItem item in /*this.listView_tags.SelectedItems*/items)
                {
                    NormalResult clear_result = null;
                    ItemInfo item_info = (ItemInfo)item.Tag;
                    if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                        clear_result = await CreateTagContentAsync(item, epc_bank_hex, user_bank_hex);
                    else
                        continue;
                    if (clear_result.Value == -1)
                    {
                        ShowMessageBox("清除标签时出错: " + clear_result.ErrorInfo);
                        break;
                    }
                }

                this.TryInvoke(() =>
                {
                    listView_tags_SelectedIndexChanged(this, new EventArgs());
                });
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
            ShowMessage("创建完成", "green", true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);

#if REMOVED
            // var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            DialogResult result = MessageBox.Show(this,
    $"确实要清除选定的 {this.listView_tags.SelectedItems.Count} 个标签的内容?",
    "RfidToolForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            _ = Task.Factory.StartNew(async () =>
            {
                var items = ListViewUtil.GetSelectedItems(this.listView_tags);
                foreach (ListViewItem item in /*this.listView_tags.SelectedItems*/items)
                {
                    NormalResult clear_result = null;
                    ItemInfo item_info = (ItemInfo)item.Tag;
                    if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                        clear_result = await TestClearUhfTagContentAsync(item);
                    else
                        continue;
                    if (clear_result.Value == -1)
                    {
                        ShowMessageBox("清除标签时出错: " + clear_result.ErrorInfo);
                        break;
                    }
                }

                this.TryInvoke(() =>
                {
                    listView_tags_SelectedIndexChanged(this, new EventArgs());
                });
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
#endif
        }

        static string GetUhfTagRowInfo(ListViewItem item,
    out byte[] user_bank,
    out string epc_bank_hex)
        {
            user_bank = null;
            epc_bank_hex = null;

            ItemInfo item_info = (ItemInfo)item.Tag;

            if (item_info.OneTag.Protocol != InventoryInfo.ISO18000P6C)
            {
                return "GetUhfTagRowInfo() 仅支持 UHF 标签";
            }

            user_bank = item_info.OneTag?.TagInfo?.Bytes;
            epc_bank_hex = item_info.OneTag?.UID;
            return null;
        }



        void menu_clearSelectedTagContent_Click(object sender, EventArgs e)
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

            _ = Task.Factory.StartNew(async () =>
            {
                var items = ListViewUtil.GetSelectedItems(this.listView_tags);
                foreach (ListViewItem item in /*this.listView_tags.SelectedItems*/items)
                {
                    // string uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                    var clear_result = await ClearTagContentAsync(item, !control);
                    if (clear_result.Value == -1)
                    {
                        ShowMessageBox("清除标签时出错: " + clear_result.ErrorInfo);
                        break;
                    }
                }

                this.TryInvoke(() =>
                {
                    listView_tags_SelectedIndexChanged(this, new EventArgs());
                });
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        async Task<NormalResult> ClearTagContentAsync(
    ListViewItem item,
    bool lock_as_error)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                return await ClearUhfTagContentAsync(item, lock_as_error);
            else if (item_info.OneTag.Protocol == InventoryInfo.ISO15693)
                return await ClearHfTagContentAsync(item, lock_as_error);
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"暂不支持 {item_info.OneTag.Protocol} 协议的标签"
                };
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

                var new_tag_info = old_tag_info.CloneTagInfo(); // ???
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

#if REMOVED
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
#endif
                strError = await WriteTagInfo(item,
old_tag_info,
new_tag_info);
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

        async Task<NormalResult> CreateTagContentAsync(
            ListViewItem item,
            string epc_bank_hex,
            string user_bank_hex)
        {
            string strError = "";

            try
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                var old_tag_info = item_info.OneTag.TagInfo;

                var user_bank = ByteArray.GetTimeStampByteArray(user_bank_hex);

                var new_tag_info = old_tag_info.CloneTagInfo();

                new_tag_info.Bytes = user_bank;
                new_tag_info.UID = epc_bank_hex;    // UhfUtility.EpcBankHex(result.EpcBank);
                strError = await WriteTagInfo(item,
                    old_tag_info,
                    new_tag_info);
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
                strError = "TestClearUhfTagContent() 出现异常: " + ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static byte[] BuildTestBlankEpcBank()
        {
            List<byte> bytes = new List<byte>();
            {
                ProtocolControlWord pc = new ProtocolControlWord();
                pc.UMI = false;
                pc.XPC = false;
                pc.ISO = false;
                pc.AFI = 0;
                pc.LengthIndicator = 0; // 载荷为 0
                bytes.AddRange(UhfUtility.EncodePC(pc));
            }

            return bytes.ToArray();
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

                var new_tag_info = old_tag_info.CloneTagInfo();     // ???
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
#if REMOVED
                if (old_tag_info.Protocol != InventoryInfo.ISO18000P6C)
                    RfidTagList.ClearTagTable(item_info.OneTag.UID);
                var result = RfidManager.WriteTagInfo(item_info.OneTag.ReaderName,
    old_tag_info,
    new_tag_info);
                /*
                // 2023/10/31
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    RfidTagList.ClearTagTable(new_tag_info.UID);
                    UpdateUID(item_info, new_tag_info.UID);
                }
                */

                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    goto ERROR1;
                }

                if (old_tag_info.Protocol != InventoryInfo.ISO18000P6C)
                {
                    strError = await Task.Run(() => { return GetTagInfo(item); });
                    if (strError != null)
                        goto ERROR1;
                }
#endif
                strError = await WriteTagInfo(item,
                    old_tag_info,
                    new_tag_info);
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

        async Task<string> WriteTagInfo(
            ListViewItem item,
            ReadonlyTagInfo old_tag_info,
            TagInfo new_tag_info)
        {
            // 验证显示的 UID 和 TagInfo 中的 UID 是否一致
            {
                var display_uid = ListViewUtil.GetItemText(item, COLUMN_UID); ;
                Debug.Assert(display_uid == old_tag_info.UID, $"WriteTagInfo() display_uid({display_uid}) != old_tag_info.UID({old_tag_info.UID})");
            }

            ItemInfo item_info = (ItemInfo)item.Tag;

            // 2023/11/25
            // 为 new_tag_info.Bytes 补足长度，和 old_tag_info.Bytes 一样长
            if (new_tag_info.Bytes != null
                && old_tag_info.Protocol == InventoryInfo.ISO18000P6C
                && this.AutoRefresh == false)
            {
                var old_bytes_count = old_tag_info.Bytes?.Length;
                if (old_bytes_count > new_tag_info.Bytes.Length)
                {
                    var temp = new List<Byte>(new_tag_info.Bytes);
                    while (temp.Count < old_bytes_count)
                    {
                        temp.Add(0);
                    }
                    new_tag_info.Bytes = temp.ToArray();
                }
            }

#if OLD
            string readerName = item_info.OneTag.ReaderName;
            if (old_tag_info.Protocol != InventoryInfo.ISO18000P6C)
                RfidTagList.ClearTagTable(old_tag_info.UID);
            var result = RfidManager.WriteTagInfo(readerName,
old_tag_info.CloneTagInfo(),    // ???
new_tag_info);
            /*
            // 2023/10/31
            if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
            {
                RfidTagList.ClearTagTable(new_tag_info.UID);
                UpdateUID(item_info, new_tag_info.UID);
            }
            */

            if (result.Value == -1)
            {
                return result.ErrorInfo + $" errorCode={result.ErrorCode}";
            }
#endif
            string readerName = item_info.OneTag.ReaderName;
            Debug.Assert(readerName == old_tag_info.ReaderName);
            var result = WriteTagInfo(old_tag_info.CloneTagInfo(),
                new_tag_info);
            if (result.Value == -1)
                return result.ErrorInfo;


            if (old_tag_info.Protocol != InventoryInfo.ISO18000P6C
                || (old_tag_info.Protocol == InventoryInfo.ISO18000P6C
                && old_tag_info.UID == new_tag_info.UID)
                )
            {
                var error = await Task.Run(() => { return GetTagInfo(item); });
                if (error != null)
                    return error;

                if (item_info.LogicChipItem != null)
                {
                    item_info.LogicChipItem.SetChanged(false);
                    UpdateChanged(item_info.LogicChipItem);
                }
                UpdateSaveButton();

                // 让自动修正 EAS 能够重新进行
                item_info.EasChecked = false;
            }
            else if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C
                && old_tag_info.UID != new_tag_info.UID
                && this.AutoRefresh == false)
            {
                // 注: 不清掉 Books 里面的 .TagInfo，因为清掉后可能会导致立刻自动进行一次无效的 GetTagInfo()
                RfidTagList.ClearTagTable(new_tag_info.UID, false);
                UpdateItemInfo(item_info, new_tag_info);

                // 修改视觉显示
                // TODO: 其实整个 ListViewItem 每一列都可能需要刷新
                // ListViewUtil.ChangeItemText(item, COLUMN_UID, new_tag_info.UID);
                FillTagInfo(item);

                if (item_info.LogicChipItem != null)
                {
                    item_info.LogicChipItem.SetChanged(false);
                    UpdateChanged(item_info.LogicChipItem);
                }
                UpdateSaveButton();

                // 让自动修正 EAS 能够重新进行
                item_info.EasChecked = false;
            }
            return null;
        }

        public static NormalResult WriteTagInfo(TagInfo old_tag_info,
            TagInfo new_tag_info)
        {
            lock (RfidManager.SyncRoot)
            {
                NormalResult result = RfidManager.WriteTagInfo(
    old_tag_info.ReaderName,
    old_tag_info,
    new_tag_info);
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    RfidTagList.ClearTagTable(old_tag_info.UID, false);
                else
                    RfidTagList.ClearTagTable(old_tag_info.UID);

                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                    RfidTagList.ClearTagTable(new_tag_info.UID);

                if (result.Value == -1)
                    return new NormalResult
                    {
                        Value = result.Value,
                        ErrorInfo = result.ErrorInfo + $" errorCode={result.ErrorCode}",
                        ErrorCode = result.ErrorCode,
                    };
                return result;
            }
        }

        async void menu_saveSelectedTagContent_Click(object sender, EventArgs e)
        {
            var control = (Control.ModifierKeys & Keys.Control) != 0;

            _ = BeginSaveItemChangeAsync(
                ListViewUtil.GetSelectedItems(this.listView_tags),
                async (item) => await SaveTagContentAsync(item, control ? "select_format" : ""));
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

        Task BeginSaveItemChangeAsync(
    IEnumerable<ListViewItem> items,
    Delegate_save proc)
        {
            return Task.Factory.StartNew(async () =>
            {
                await SaveItemChangeAsync(
                    items,
                    proc);
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        delegate Task<string> Delegate_save(ListViewItem item);

        async Task SaveItemChangeAsync(
            IEnumerable<ListViewItem> items,
            Delegate_save proc)
        {
            int count = 0;
            List<string> errors = new List<string>();
            List<string> succeeds = new List<string>();
            foreach (ListViewItem item in /*this.listView_tags.SelectedItems*/items)
            {
                var error = await proc(item);
                if (error != null && error.StartsWith("!succeed:") == false)
                    errors.Add(error);
                else
                {
                    if (error != null && error.StartsWith("!succeed:"))
                        succeeds.Add(error.Substring("!succeed:".Length));
                    count++;
                }
            }

            this.TryInvoke(() =>
            {
                listView_tags_SelectedIndexChanged(this, new EventArgs());
            });

            UpdateSaveButton();

            // if (count > 0)
            this.ShowMessage($"保存完全成功:{count} {(errors.Count > 0 ? ("警告或错误:" + errors.Count) : "")}",
                errors.Count == 0 ? "green" : "yellow", true);
            //else
            //    this.ShowMessage("没有需要保存的事项", "yellow", true);

            if (succeeds.Count > 0)
            {
                this.TryInvoke(() =>
                {
                    MessageDlg.Show(this, StringUtil.MakePathList(succeeds, "\r\n"), "保存成功");
                });
            }

            if (errors.Count > 0)
            {
                this.TryInvoke(() =>
                {
                    MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "警告或错误");
                });
            }
        }

        // 故意写入 PII 为空的标签内容
        async void menu_saveBlankPiiTagContent_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this,
$"写入 PII 为空的标签内容，会覆盖当前标签的原有内容。\r\n\r\n确实要覆盖? ",
"RfidToolForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            _ = BeginSaveItemChangeAsync(
                ListViewUtil.GetSelectedItems(this.listView_tags),
                async (item) => await SaveBlankPiiTagContentAsync(item));

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

        int CountingSaveableItems(IEnumerable<ListViewItem> items,
            string style = "changed,ISO15693,ISO18000P6C")
        {
            int count = 0;
            foreach(var item in items)
            {
                ItemInfo item_info = (ItemInfo)item.Tag;
                if (item_info == null)
                    continue;
                if (StringUtil.IsInList("changed", style)
                    && item_info.LogicChipItem?.Changed == false)
                    continue;

                // 对 ISO14443A 协议的标签暂不支持保存操作
                if (item_info.OneTag.Protocol == InventoryInfo.ISO14443A)
                    continue;

                if (StringUtil.IsInList("ISO14443A", style)
    && item_info.OneTag.Protocol == InventoryInfo.ISO14443A)
                {
                    count++;
                    continue;
                }

                if (StringUtil.IsInList("ISO15693", style)
&& item_info.OneTag.Protocol == InventoryInfo.ISO15693)
                {
                    count++;
                    continue;
                }

                if (StringUtil.IsInList("ISO18000P6C", style)
&& item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    count++;
                    continue;
                }
            }
            return count;
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
            if (item_info.OneTag.Protocol == InventoryInfo.ISO14443A)
                return "暂不支持 ISO14443A 标签写入";
            if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                return "暂不支持 ISO18000P6C 标签写入";

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
                    new_tag_info = old_tag_info.CloneTagInfo(); // ???
                    // 对 byte[] 内容执行清除。锁定的块不会被清除
                    new_tag_info.Bytes = LogicChip.ClearBytes(new_tag_info.Bytes,
                        new_tag_info.BlockSize,
                        new_tag_info.MaxBlockCount,
                        new_tag_info.LockStatus);
                }

#if REMOVED
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
#endif
                strError = await WriteTagInfo(item,
old_tag_info,
new_tag_info);
                if (strError == null)
                {
                    //UpdateChanged(item_info.LogicChipItem);
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
            var result = MessageBox.Show(this,
$"测试创建错误的标签内容，会覆盖当前标签的原有内容。\r\n\r\n确实要覆盖? ",
"RfidToolForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            _ = BeginSaveItemChangeAsync(
                ListViewUtil.GetSelectedItems(this.listView_tags),
                async (item) => await SaveErrorTagContent1Async(item));
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
            if (item_info.OneTag.Protocol == InventoryInfo.ISO14443A)
                return "暂不支持 ISO14443A 标签写入";
            if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                return "暂不支持 ISO18000P6C 标签写入";

            string strError = "";

            try
            {
                var old_tag_info = item_info.OneTag.TagInfo;
                var new_tag_info = old_tag_info.CloneTagInfo(); // ???
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

#if REMOVED
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
#endif
                strError = await WriteTagInfo(item,
old_tag_info,
new_tag_info);
                if (strError == null)
                {
                    //UpdateChanged(item_info.LogicChipItem);
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
                var new_tag_info = old_tag_info.CloneTagInfo(); // ???
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

#if REMOVED
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
#endif
                strError = await WriteTagInfo(item,
old_tag_info,
new_tag_info);

                if (strError == null)
                {
                    //UpdateChanged(item_info.LogicChipItem);
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

        async Task<string> SaveTagContentAsync(ListViewItem item, string style)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                return await SaveUhfTagContentAsync(item, style);
            else
                return await SaveHfTagContentAsync(item);
        }

        // 保存超高频标签内容
        async Task<string> SaveUhfTagContentAsync(ListViewItem item,
            string style)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            if (item_info.LogicChipItem == null)
                return "item_info.LogicChipItem == null";
            if (item_info.LogicChipItem.Changed == false)
                return $"{item_info.LogicChipItem.UID} 没有发生修改";

            string strError = "";

            try
            {
                // int i = 0;
                string build_style = "";  // testing "noUserBank"
            REDO:
                var old_tag_info = item_info.OneTag.TagInfo;

                // 询问是否写入 User Bank
                bool build_user_bank = true;
                if (old_tag_info.Protocol == InventoryInfo.ISO18000P6C)
                {
                    string ret = "";
                    if (StringUtil.IsInList("noUserBank", build_style))
                        ret = "不写入";
                    else
                    {
                        ret = this.TryGet(() =>
                        {
                            return ListDialog.GetInput(
                                this,
                                "请选择写入超高频标签时",
                                "是否要写入 User Bank",
                                new string[] { "要写入", "不写入" },
                                0,
                                this.Font);
                        });
                    }
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

                var select_format = StringUtil.IsInList("select_format", style);

                var build_result = BuildWritingTagInfo(old_tag_info.CloneTagInfo(), // ???
                    item_info.LogicChipItem,
                    item_info.LogicChipItem.EAS,
                    select_format ? "select" : "auto", // gb/gxlm/auto
                    build_style,
                    (initial_format) =>
                    {
                        // 如果是空白标签，需要弹出对话框提醒选择格式
                        var ret = this.TryGet(() =>
                        {
                            return ListDialog.GetInput(
                                this,
                                "请选择写入超高频标签的内容格式",
                                "请选择一个内容格式",
                                new string[] { "国标", "高校联盟", "望湖洞庭" },
                                initial_format == "gb" ? 0 : (initial_format == "gxlm" ? 1 : (initial_format == "gxlm(whdt)" ? 2 : 0)),
                                this.Font);
                        });
                        if (ret == "国标")
                            return "gb";
                        if (ret == "高校联盟")
                            return "gxlm";
                        if (ret == "望湖洞庭")
                            return "gxlm(whdt)";
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
                            var dlg_result = this.TryGet(() =>
                            {
                                return MessageBox.Show(this,
    "刚才您选择了不写入超高频标签的 User Bank，然而即将写入的内容又有 OI(机构代码) 元素。\r\n\r\n请问是否继续写入? (警告: 继续写入会导致 OI 元素发生丢失)",
    "RfidToolForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                            });
                            if (dlg_result == DialogResult.No)
                                return false;
                        }
                        return true;
                    },
                    // build_user_bank
                    (chip, uhfProtocol) =>
                    {
                        return build_user_bank;
                    },
                    () =>
                    {
                        // TODO: 可以弹出一个对话框询问一下，是否要把 OI 移动到 User Bank
                        return false;
                    }
                    );
                var new_tag_info = build_result.TagInfo;

#if REMOVED
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

#endif
                strError = await WriteTagInfo(item,
old_tag_info,
new_tag_info);
                /*
                // testing
                {
                    if (i == 0)
                        strError = "测试出错";
                    i++;
                }
                */
                if (strError != null
                    && StringUtil.IsInList("noUserBank", build_style) == false
                    && build_result.NewUhfFormat == "gxlm(whdt)")
                {
                    // 2023/12/4
                    // 询问是否用“不写入 User Bank 重试一次”
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
    $"写入标签时出错:\r\n{strError}\r\n\r\n是否要用“不写入 User Bank”方式重试一次?",
    "RfidToolForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    });
                    if (result == DialogResult.Yes)
                    {
                        StringUtil.SetInList(ref build_style, "noUserBank", true);
                        goto REDO;
                    }
                }

                string format_string = "";
                if (string.IsNullOrEmpty(build_result.NewUhfFormat) == false)
                {
                    if (build_result.OldUhfFormat != build_result.NewUhfFormat)
                        format_string = $"(格式:{GetUhfFormatCaption(build_result.OldUhfFormat)}-->{GetUhfFormatCaption(build_result.NewUhfFormat)})";
                    else
                        format_string = $"(格式:{GetUhfFormatCaption(build_result.NewUhfFormat)})";
                }

                string format_warning = "";
                if (build_result.NewUhfFormat == "gxlm(whdt)")
                    format_warning = " 注意: 创建“望湖洞庭”格式时，除了 PII 以外的元素会被预置的内容重置";

                if (strError == null)
                {
                    //UpdateChanged(item_info.LogicChipItem);
                    return $"!succeed:保存成功{format_string} {format_warning}";
                }
                else
                {
                    strError = $"保存成功{format_string}。重新读入时出错: {strError}";
                }
                return strError;
            }
            catch (Exception ex)
            {
                strError = "SaveUhfTagContentAsync() 出现异常: " + ex.Message;
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

        // 更新 item_info 中的 TagInfo 和相关数据成员
        static void UpdateItemInfo(ItemInfo item_info,
            TagInfo tag_info/*
            string uid*/)
        {
            if (item_info == null)
                throw new ArgumentException("item_info 不应为 null");

            if (tag_info == null)
                throw new ArgumentException("tag_info 不应为 null");

            item_info.SetTagInfo(tag_info, false);
            /*
            string uid = tag_info.UID;

            if (item_info.OneTag != null)
                item_info.OneTag.UID = uid;

            if (item_info.OneTag != null && item_info.OneTag.TagInfo != null)
            {
                item_info.OneTag.TagInfo = tag_info.Clone();
            }

            // 2023/11/24
            if (item_info.LogicChipItem != null)
                item_info.LogicChipItem.UID = uid;
            */
        }

        public static string GetUhfFormatCaption(string format)
        {
            if (format == "gb")
                return "国标";
            if (format == "gxlm")
                return "高校联盟";
            if (format == "gxlm(whdt)")
                return "望湖洞庭";
            if (format == "blank")
                return "空白";
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
                return $"{item_info.LogicChipItem.UID} 没有发生修改";

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

#if REMOVED
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
#endif

                strError = await WriteTagInfo(item,
    old_tag_info,
    new_tag_info);
                if (strError == null)
                {
                    //UpdateChanged(item_info.LogicChipItem);
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


        static TagInfo BuildNewHfTagInfo(ReadonlyTagInfo old_tag_info,
    LogicChipItem chip)
        {
            TagInfo new_tag_info = old_tag_info.CloneTagInfo(); // ???
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
        //      initial_format  选择对话框初始显示的格式。为 blank/gb/gxlm/gxlm(whdt) 之一
        // return:
        //      null    放弃选择
        //      其它      返回选择的格式。为 gb/gxlm/gxlm(whdt) 之一
        public delegate string delegate_askUhfDataFormat(string initial_format);

        // 过滤 User Bank 中的不需要的元素
        // return:
        //      true    要创建 User Bank
        //      false   不创建 User Bank
        public delegate bool filterUserBankElements(LogicChip chip, string uhfProtocol);

        // 询问是否覆盖不同内容格式的标签
        // parameters:
        //      new_format  新格式。为 gb/gxlm 之一
        //      old_format  老格式。为 gb/gxlm 之一
        // return:
        //      true    要覆盖
        //      false   放弃覆盖
        public delegate bool delegate_askOverwriteDifference(string new_format, string old_format);

        public class BuildWritingResult : NormalResult
        {
            public TagInfo TagInfo { get; set; }

            public string OldUhfFormat { get; set; }
            public string NewUhfFormat { get; set; }
        }

        // return:
        //      true    OI 要移动到 User Bank 中(EPC 中 UII 里面就没有了 OI)
        //      false   OI 写入 EPC 中的 UII 中(User Bank 中就没有了 OI 元素)
        public delegate bool delegate_askOiMoveToUserBank();

        // 构造写入用的 TagInfo
        // TODO: 要留意 高校联盟 格式的 TOU 写入前翻译是否正确
        // parameters:
        //          style   包含 "noUserBank" 表示对“望湖洞庭”格式不创建 User Bank
        //          chip    包含即将写入标签信息的 LogicChip 对象。注意本函数执行过程中对 chip 对象进行了保护，确保不会修改它的内容
        public static BuildWritingResult BuildWritingTagInfo(TagInfo existing,
    LogicChip chip,
    bool eas,
    string uhfProtocol = "auto", // gb/gxlm/gxlm(whdt)/auto/select auto 表示自动探测格式，select 表示强制弹出对话框选择格式
    string style = "",
    delegate_askUhfDataFormat func_askFormat = null,
    delegate_askOverwriteDifference func_askOverwrite = null,
    delegate_warningRemoveOI func_askRemoveOI = null,
    // bool build_user_bank = true
    filterUserBankElements func_filterUserBankElements = null,
    delegate_askOiMoveToUserBank func_oiMoveToUserBank = null
            )
        {
            var working_chip = chip.Clone();

            var noUserBank = StringUtil.IsInList("noUserBank", style);

            if (existing.Protocol == InventoryInfo.ISO15693)
            {
                // SetTypeOfUsage("", chip, "gb");

                TagInfo new_tag_info = existing.Clone();
                new_tag_info.Bytes = working_chip.GetBytes(
                    (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                    (int)new_tag_info.BlockSize,
                    LogicChip.GetBytesStyle.None,
                    out string block_map);
                new_tag_info.LockStatus = block_map;
                new_tag_info.DSFID = LogicChip.DefaultDSFID;  // 图书
                new_tag_info.SetEas(eas);
                return new BuildWritingResult
                {
                    TagInfo = new_tag_info
                };
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


                var epc_bank = Element.FromHexString(existing.UID);
                var isExistingGB = UhfUtility.IsISO285604Format(epc_bank, existing.Bytes);
                string existingUhfProtocol = "";
                if (UhfUtility.IsBlankTag(epc_bank, existing.Bytes) == true)
                    existingUhfProtocol = "blank";
                else
                {
                    if (isExistingGB)
                        existingUhfProtocol = "gb";
                    else
                    {
                        if (GaoxiaoUtility.IsWhdt(epc_bank))
                            existingUhfProtocol = "gxlm(whdt)";
                        else
                            existingUhfProtocol = "gxlm";
                    }
                }

                if (uhfProtocol == "auto")
                {
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
                        /*
                        if (result == "gb")
                            uhfProtocol = "gb";
                        else
                            uhfProtocol = "gxlm";
                        */
                        uhfProtocol = result;
                    }
                    else
                    {
                        if (isExistingGB)
                            uhfProtocol = "gb";
                        else
                        {
                            if (GaoxiaoUtility.IsWhdt(epc_bank))
                                uhfProtocol = "gxlm(whdt)";
                            else
                                uhfProtocol = "gxlm";
                        }
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

                    var result = func_askFormat.Invoke(/*isExistingGB ? "gb" : "gxlm"*/
                        existingUhfProtocol);

                    if (result == null)
                        throw new InterruptException("放弃写入标签");
                    /*
                    if (result == "gb")
                        uhfProtocol = "gb";
                    else
                        uhfProtocol = "gxlm";
                    */
                    uhfProtocol = result;
                }

                bool build_user_bank = true;
                if (func_filterUserBankElements != null)
                {
                    // 注: 如果强制不让创建 User Bank 了，这里就不询问了
                    if (noUserBank == false)
                        build_user_bank = func_filterUserBankElements(working_chip, uhfProtocol);
                    else
                        build_user_bank = false;    // 强制不让创建 User Bank
                }

                // 2023/11/6
                // 过滤不需要的元素
                if (build_user_bank)
                {
                    // FilterUserBankElements(chip, uhfProtocol);
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
                    if (uhfProtocol == "gxlm"
                        || uhfProtocol == "gxlm(whdt)")
                    {
                        if (RemoveOI(working_chip, func_askRemoveOI) == false)
                            throw new Exception("放弃写入");
                    }
                }

                TagInfo new_tag_info = existing.Clone();

                // 当写入不同格式的时候，警告格式覆盖
                if (uhfProtocol != existingUhfProtocol
                    && existingUhfProtocol != "blank")
                {
                    var ret = func_askOverwrite.Invoke(uhfProtocol, existingUhfProtocol);
                    if (ret == false)
                        throw new Exception("放弃写入");
                }

                if (uhfProtocol == "gxlm"
                    || uhfProtocol == "gxlm(whdt)")
                {
                    // 写入高校联盟数据格式
#if REMVOED
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
                        var ret = func_askOverwrite.Invoke(uhfProtocol, "gb");
                        if (ret == false)
                            throw new Exception("放弃写入");
                    }
#endif

                    // SetTypeOfUsage(chip, "gxlm");
                    var epc_info = new GaoxiaoEpcInfo
                    {
                        Version = 5,
                        Picking = 1,
                        Reserve = 0
                    };
                    BuildTagResult result = null;
                    if (uhfProtocol == "gxlm(whdt)")
                    {
                        epc_info = new GaoxiaoEpcInfo
                        {
                            Version = 5,
                            Picking = 1,
                            Reserve = 0,
                            ContentParameters = new int[] { 16, 24, 28, 30 },   // 强行规定 Content Parameters 值
                        };
                        // chip 中只需要一个 PII 元素。因为其它的都会被样本内容强行设置进入
                        var pii = working_chip.FindElement(ElementOID.PII)?.Text;
                        var temp_chip = new LogicChip();
                        temp_chip.SetElement(ElementOID.PII, pii);
                        result = GaoxiaoUtility.BuildTag(temp_chip, build_user_bank, eas,
    epc_info);
                    }
                    else
                    {
                        // 删除空内容元素
                        // 2023/11/23
                        working_chip.RemoveEmptyElements();

                        result = GaoxiaoUtility.BuildTag(working_chip, build_user_bank, eas,
                            epc_info);
                    }

                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = UhfUtility.EpcBankHex(result.EpcBank);    //  existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);

                    // 强行修改 User Bank 为样本值
                    if (uhfProtocol == "gxlm(whdt)")
                    {
                        if (build_user_bank)
                            new_tag_info.Bytes = ByteArray.GetTimeStampByteArray("0C02D9941004000100012C0038000000");
                        else
                            new_tag_info.Bytes = null;
                    }
                }
                else
                {
                    // 写入国标数据格式
#if REMOVED
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
#endif
                    // SetTypeOfUsage(chip, "gb");


                    string build_style = eas ? "afi_eas_on" : "";


                    // 是否要特意把 OI 写入 User Bank
                    if (func_oiMoveToUserBank != null
                        && build_user_bank)
                    {
                        // TODO: 可以进一步判断当 working_chip 中包含了非空的 OI 或者 AOI 元素时，才 invoke() func_oiMoveToUserBank
                        if (func_oiMoveToUserBank.Invoke() == true)
                            build_style += ",oi_in_userbank";
                        /*
                        if (StringUtil.IsInList("OwnerInstitution", Program.MainForm.UhfUserBankElements)
                            && build_user_bank == true)
                            build_style += ",oi_in_userbank";
                        */
                    }

                    var result = UhfUtility.BuildTag(working_chip,
                        build_user_bank,
                        true,
                        build_style);
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = UhfUtility.EpcBankHex(result.EpcBank);  // existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                return new BuildWritingResult
                {
                    TagInfo = new_tag_info,
                    OldUhfFormat = existingUhfProtocol,
                    NewUhfFormat = uhfProtocol,
                };
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
        }

#if REMOVED
        // 根据 EPC Bank 判断是不是“望湖洞庭”格式
        public static bool IsWhdt(byte[] epc_bank)
        {
            var parse_result = GaoxiaoUtility.ParseTag(epc_bank, null, ""); // 注意，没有包含 checkUMI 表示不要检查 UMI 和 ContentParameters 是否具备之间的关系
            if (parse_result.PC == null
                || parse_result.EpcInfo == null)
                return false;
            var pc = parse_result.PC;
            // 注: 望湖洞庭有一批标签没有 User Bank 内容，但 EPC 内容和先前的无异
            if (/*pc.UMI == true
                && */pc.AFI == 0
                && pc.XPC == false
                && pc.ISO == false)
            {

            }
            else
                return false;

            var epc_info = parse_result.EpcInfo;

            // content parameter 16 24 28 30
            var cp = new int[] { 16, 24, 28, 30 };
            if (cp.SequenceEqual(epc_info.ContentParameters) == false)
                return false;

            if (epc_info.Reserve != 0)
                return false;
            if (epc_info.Picking != 1)
                return false;
            if (epc_info.Version != 5)
                return false;
            if (epc_info.EncodingType != 0)
                return false;
            return true;
        }
#endif

        // 过滤掉 User Bank 中的不需要的元素。
        // Program.MainForm.UhfUserBankElements 中定义了不该被过滤的元素名列表。其余的元素名就应该被过滤掉
        public static void FilterUserBankElements(LogicChip chip,
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


        private void toolStripButton_saveRfid_Click(object sender, EventArgs e)
        {
            var control = (Control.ModifierKeys & Keys.Control) != 0;
            _ = BeginSaveItemChangeAsync(
                ListViewUtil.GetItems(this.listView_tags),
                async (item) => await SaveTagContentAsync(item, control ? "select_format" : ""));

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

        public bool AutoRefresh
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.toolStripButton_autoRefresh.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.toolStripButton_autoRefresh.Checked = value;
                });
            }
        }

        public bool AutoFixEas
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.toolStripButton_autoFixEas.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.toolStripButton_autoFixEas.Checked = value;
                });
            }
        }

        // 2023/11/29
        private void listView_tags_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(sender, e);
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

    // 选择 RFID 标签的对话框。
    // 和 RFID 工具窗的区别是，AutoFixEas 和 AutoRefresh 都是预先设定好的，不会理会上次保存的状态
    public class SelectRfidTagDialog : RfidToolForm
    {
        public SelectRfidTagDialog()
        {
            this.DialogMode = true;
        }
    }
}
