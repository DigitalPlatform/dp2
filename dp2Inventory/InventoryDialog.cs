using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Core;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using static dp2Inventory.LibraryChannelUtil;

namespace dp2Inventory
{
    public partial class InventoryDialog : Form
    {
        // public event WriteCompleteEventHandler WriteComplete = null;

        public InventoryDialog()
        {
            InitializeComponent();

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
        }

        private void ModifyDialog_Load(object sender, EventArgs e)
        {

        }

        private void ModifyDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel?.Cancel();

        }

        private void ModifyDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        ActionInfo _action = null;

        // 开始修改
        private async void toolStripButton_begin_Click(object sender, EventArgs e)
        {
            string strError = "";

            using (BeginInventoryDialog dlg = new BeginInventoryDialog())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "beginModifyDialog", "state");

                dlg.UiState = ClientInfo.Config.Get("BeginInventoryDialog", "uiState", "");
                dlg.ShowDialog(this);
                ClientInfo.Config.Set("BeginInventoryDialog", "uiState", dlg.UiState);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                /*
                _action = new ActionInfo
                {
                    FilterTU = dlg.FilterTU,
                    OI = dlg.OiString,
                    AOI = dlg.AoiString,
                    // LinkUID = dlg.LinkUID,
                    ModifyEas = dlg.ModifyEas,
                    WriteUidPiiLog = dlg.WriteUidPiiLog,
                    VerifyPii = dlg.VerifyPii,
                    PiiVerifyRule = dlg.PiiVerifyRule,
                };
                */
            }

            // 装入 UID --> UII 对照关系
            await Task.Run(async () =>
            {
                FileDownloadDialog progress = null;
                this.Invoke(new Action(() =>
                {
                    progress = new FileDownloadDialog();
                    progress.Show(this);
                }));
                try
                {
                    progress.SetMessage("正在获取 dp2library 服务器配置");
                    var initial_result = LibraryChannelUtil.Initial();
                    if (initial_result.Value == -1)
                    {
                        strError = $"获得 dp2library 服务器配置失败: {initial_result.ErrorInfo}";
                        return;
                    }

                    progress.SetMessage("正在获取 UID --> UII 对照关系");

                    Hashtable uid_table = new Hashtable();
                    NormalResult result = null;
                    if (DataModel.Protocol == "sip")
                        result = await DataModel.LoadUidTableAsync(uid_table,
                            (text) =>
                            {
                                progress.SetMessage(text);
                            },
                            _cancel.Token);
                    else
                        result = LibraryChannelUtil.DownloadUidTable(
                            null,
                            uid_table,
                            (text) =>
                            {
                                progress.SetMessage(text);
                            },
                            _cancel.Token);
                    DataModel.SetUidTable(uid_table);
                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"准备册记录过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    strError = ex.Message;
                    return;
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                }
            });

            if (string.IsNullOrEmpty(strError) == false)
                goto ERROR1;

            // TODO: 开始前用一个概要对话框显示确认一下本次批处理要进行的修改操作
            _cancel?.Dispose();
            _cancel = new CancellationTokenSource();
            BeginModify(_cancel.Token);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            _cancel?.Cancel();
        }

        // 导出 UID --> PII 对照关系
        private async void toolStripButton_exportUidPiiMap_Click(object sender, EventArgs e)
        {
            using (MessageBar bar = MessageBar.Create(this, "正在导出 UID --> PII 对照关系文件 ..."))
            {
                await ExportUidPiiMapFile();
            }
        }

        async Task ExportUidPiiMapFile()
        {
            string filename = "";
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "请指定要保存的对照文件名";
                dlg.CreatePrompt = false;
                // dlg.FileName = "";
                dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "对照文件 (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                filename = dlg.FileName;
            }

            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
                {
                    lock (_uidTable.SyncRoot)
                    {
                        foreach (string key in _uidTable.Keys)
                        {
                            writer.WriteLine($"{key}\t{(string)_uidTable[key]}");
                        }
                    }
                }
            });
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

        volatile bool _pause = false;

        // 暂停循环
        public void PauseLoop()
        {
            _pause = true;
        }

        // 继续循环
        public void ContinueLoop()
        {
            _pause = false;
        }

        void InitialRfidManager(string url)
        {
            RfidManager.Base.Name = "RFID 中心";
            RfidManager.Url = url;
            // RfidManager.AntennaList = "1|2|3|4";    // testing
            // RfidManager.SetError += RfidManager_SetError;
            // RfidManager.ListTags += RfidManager_ListTags;
            // RfidManager.Start(_cancelRfidManager.Token);
        }

        static volatile int _error_count = 0;
        static volatile int _verify_error_count = 0;

        CancellationTokenSource _cancel = new CancellationTokenSource();

        void BeginModify(CancellationToken token)
        {
            this.listView_tags.Items.Clear();
            this.toolStripButton_begin.Enabled = false;
            this.toolStripButton_stop.Enabled = true;
            ClearUidTable();
            ClearProcessedTable();
            ClearCacheTagTable(null);
            InitialRfidManager(DataModel.RfidCenterUrl);
            _ = Task.Factory.StartNew(
                async () =>
                {
                    // 暂停基本循环
                    //DataModel.PauseLoop();
                    try
                    {
                        while (token.IsCancellationRequested == false)
                        {
                            if (_pause)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1), token);
                                continue;
                            }

                            // 语音提示倒计时开始盘点
                            await SpeakCounter(token);

                            string readerNameList = "*";
                            var result = RfidManager.CallListTags(readerNameList, "");
                            // var result = DataModel.ListTags(readerNameList, "");
                            if (result.Results == null)
                                result.Results = new List<OneTag>();
                            if (result.Value == -1)
                                ShowMessageBox("inventory", result.ErrorInfo);
                            else
                            {
                                ShowMessageBox("inventory", null);
                            }

                            /*
                            if (result.Results.Count == 0)
                            {
                                await FormClientInfo.Speaking($"没有发现",
    false,
    token);
                                await Task.Delay(TimeSpan.FromSeconds(1), token);
                                continue;
                            }
                            */

                            int cross_count = 0;
                            int process_count = 0;
                            this.Invoke((Action)(() =>
                            {
                                var fill_result = FillTags(result.Results);
                                cross_count = fill_result.CrossCount;
                            }));

                            // TODO: 语音念出交叉的事项个数

                            // testing
                            // await SpeakPauseCounter(token);

                            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(token))
                            {
                                var current_token = cancel.Token;
                                EnableSkipButton(true, cancel);
                                try
                                {
                                    // int test_count = 0;
                                    Task task = null;
                                    while (current_token.IsCancellationRequested == false)
                                    {
                                        if (_pause)
                                        {
                                            await Task.Delay(TimeSpan.FromSeconds(1), current_token);
                                            continue;
                                        }

                                        // result.Value:
                                        //      -1  表示遇到了严重出错，要停止循环调用本函数
                                        //      0   表示全部完成，没有遇到出错的情况
                                        //      >0  表示处理过程中有事项出错。后继需要调主循环调用本函数
                                        var process_result = await ProcessTags(result.Results, current_token);
                                        _error_count = process_result.Value;
                                        _verify_error_count = process_result.VerifyErrorCount;
                                        process_count += process_result.ProcessCount;
                                        if (process_result.Value == 0 || process_result.Value == -1)
                                            break;

                                        if (current_token.IsCancellationRequested)
                                            break;

                                        // await SpeakAdjust($"有 {process_result.Value} 项出错。请调整天线位置", token/*注意这里不能用 current_token(用了会在“跳过”时停止全部循环)*/);
                                        // test_count++;

                                        if (task == null)
                                            task = Task.Run(async () =>
                                            {
                                                while (current_token.IsCancellationRequested == false)
                                                {
                                                    // 暂停循环，此时语音也停止播报
                                                    if (_pause)
                                                    {
                                                        await Task.Delay(TimeSpan.FromSeconds(1), current_token);
                                                        continue;
                                                    }

                                                    if (_verify_error_count > 0)
                                                        await FormClientInfo.Speaking($"有 {_verify_error_count} 项 PII 校验问题。请注意记载和解决",
    false,
    current_token);

                                                    if (_error_count - _verify_error_count > 0)
                                                        await FormClientInfo.Speaking($"有 {(_error_count - _verify_error_count)} 项出错。请调整天线位置",
                                                            false,
                                                            current_token);

                                                    await Task.Delay(TimeSpan.FromSeconds(2), current_token);
                                                }

                                            });
                                        await Task.Delay(TimeSpan.FromMilliseconds(100), current_token);
                                    }
                                }
                                catch (TaskCanceledException)
                                {
                                    if (token.IsCancellationRequested)
                                        throw;
                                    // “跳过”被触发
                                    await FormClientInfo.Speaking("跳过",
    true,
    token);
                                    // 清除相关标签的缓存，以便后续循环的时候从标签重新获取内容信息
                                    ClearCache(result.Results);
                                }
                                finally
                                {
                                    cancel?.Cancel();

                                    EnableSkipButton(false);

                                    if (result.Results.Count == 0)
                                        await FormClientInfo.Speaking($"没有发现",
                                            true,
                                            token);
                                    else
                                    {
                                        // int complete_count = result.Results.Count - cross_count;
                                        string text = $"完成 {process_count} 项  交叉 {cross_count} 项";
                                        if (process_count == 0)
                                            text = $"交叉 {cross_count} 项";

                                        await FormClientInfo.Speaking(text,
                                            true,
                                            token);
                                    }

                                }
                            }

                            // 语音或音乐提示正在处理，不要移动天线
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        ClientInfo.WriteErrorLog($"修改循环出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        ShowMessageBox("inventory", $"修改循环因为异常已终止: {ex.Message}");
                    }
                    finally
                    {
                        FormClientInfo.Speak("停止修改", false, false);

                        this.Invoke((Action)(() =>
                        {
                            // 把按钮状态复原到未启动状态
                            this.toolStripButton_begin.Enabled = true;
                            this.toolStripButton_stop.Enabled = false;
                        }));

                        // 恢复基本循环
                        //DataModel.ContinueLoop();
                    }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            void ClearCache(List<OneTag> tags)
            {
                foreach (var tag in tags)
                {
                    ClearCacheTagTable(tag.UID);
                }
            }
        }

        void EnableSkipButton(bool enable, CancellationTokenSource cancel = null)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripButton_nextScan.Tag = cancel;
                this.toolStripButton_nextScan.Enabled = enable;
            }));
        }

        // 语音提示倒计时
        async Task SpeakCounter(CancellationToken token)
        {
            for (int i = DataModel.BeforeScanSeconds; i > 0; i--)
            {
                FormClientInfo.Speak($"{i}", false, true);
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            Console.Beep();
            // FormClientInfo.Speak($"开始扫描", false, true);
        }

        // 语音提示间隙时间，方便拿走从读写器上标签
        async Task SpeakPauseCounter(CancellationToken token)
        {
            // 让上一句话说完
            await Task.Delay(TimeSpan.FromSeconds(3), token);

            FormClientInfo.Speak($"暂停开始", false, true);
            await Task.Delay(TimeSpan.FromSeconds(3), token);
            for (int i = 5; i > 0; i--)
            {
                FormClientInfo.Speak($"{i}", false, true);
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            FormClientInfo.Speak($"暂停结束", false, true);
            await Task.Delay(TimeSpan.FromSeconds(3), token);
        }

        async Task SpeakAdjust(string text, CancellationToken token)
        {
            FormClientInfo.Speak(text, false, true);
            await Task.Delay(TimeSpan.FromSeconds(4), token);
        }

        #region Error Dialog

        ErrorTable _errorTable = null;

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

        public void ShowMessageBox(string type, string text)
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
                    }
                }

                _errorTable.SetError(type, text);
                _errorDialog.Message = _errorTable.GetError(false);
            }));
        }

        #endregion

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

        class FillResult : NormalResult
        {
            public int CrossCount { get; set; }
        }

        // 初始填充 tags 列表
        FillResult FillTags(List<OneTag> tags)
        {
            this.listView_tags.Items.Clear();
            int cross_count = 0;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (tag.Protocol != InventoryInfo.ISO15693)
                        continue;

                    ListViewItem item = ListViewUtil.FindItem(this.listView_tags, tag.UID, COLUMN_UID);
                    if (item == null)
                    {
                        item = new ListViewItem();
                        item.Tag = new ItemInfo { Tag = tag };
                        ListViewUtil.ChangeItemText(item, COLUMN_PII, "(尚未填充)");
                        this.listView_tags.Items.Add(item);
                    }
                    RefreshItem(item, tag);

                    var iteminfo = item.Tag as ItemInfo;
                    if (iteminfo.State == "cross")
                        cross_count++;
                }
            }

            return new FillResult { CrossCount = cross_count };
        }

        public class ItemInfo
        {
            public OneTag Tag { get; set; }
            public TagInfo TagInfo { get; set; }

            // 从对照关系中得到的 UII 字符串
            public string UII { get; set; }

            // 是否被过滤掉
            // public bool Disabled { get; set; }
            public string State { get; set; }   // disable/cross/succeed/error

            public string ErrorInfo { get; set; }

            // UID --> PII 日志是否成功写入过了
            public bool UidPiiLogWrited { get; set; }

            ProcessInfo ProcessInfo { get; set; }
        }

        // 刷新一个 ListViewItem 的所有列显示
        void RefreshItem(ListViewItem item, OneTag tag)
        {
            var iteminfo = item.Tag as ItemInfo;
            if (iteminfo == null)
                item.Tag = new ItemInfo { Tag = tag };

            if (IsProcessed(tag.UID))
            {
                iteminfo.State = "cross";   // 表示以前处理过这个标签
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "交叉");
                SetItemColor(item, "cross");
            }

            ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_ANTENNA, tag.AntennaID.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_READERNAME, tag.ReaderName);
            ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.Protocol);

            ListViewUtil.ChangeItemText(item, COLUMN_PII, "(尚未填充)");
        }

        class ProcessResult : NormalResult
        {
            // 出错的个数
            public int ErrorCount { get; set; }
            // 处理个数(包含出错的个数)
            public int ProcessCount { get; set; }
            // 被过滤掉(不符合 TU)的个数
            public int FilteredCount { get; set; }

            // 校验出错的个数。包含在 ErrorCount 内
            public int VerifyErrorCount { get; set; }
        }

        // 之前处理过的事项的集合。用于计算本次和以前处理过的交叉部分
        // TODO: 这里面存储所有的已经处理过的，还是仅仅只保留前一轮的？
        // UID --> ItemInfo
        Hashtable _processedTable = new Hashtable();

        // 先前是否已经处理过？
        bool IsProcessed(string uid)
        {
            lock (_processedTable.SyncRoot)
            {
                return _processedTable.ContainsKey(uid);
            }
        }

        void ClearProcessedTable()
        {
            lock (_processedTable.SyncRoot)
            {
                _processedTable.Clear();
            }
        }

        void AddToProcessed(string uid, ItemInfo iteminfo)
        {
            lock (_processedTable.SyncRoot)
            {
                _processedTable[uid] = iteminfo;
            }
        }

        // 处理每一个标签的修改动作
        // result.Value:
        //      -1  表示遇到了严重出错，要停止循环调用本函数
        //      0   表示全部完成，没有遇到出错的情况
        //      >0  表示处理过程中有事项出错。后继需要调主循环调用本函数
        async Task<ProcessResult> ProcessTags(List<OneTag> tags,
            CancellationToken token)
        {
            if (tags == null)
                return new ProcessResult();

            // DataModel.IncApiCount();
            try
            {
                int process_count = 0;
                int error_count = 0;
                int filtered_count = 0;
                int verify_error_count = 0;
                foreach (var tag in tags)
                {
                    if (token.IsCancellationRequested)
                        return new ProcessResult
                        {
                            Value = -1,
                            ErrorInfo = "中断",
                            ErrorCode = "cancel"
                        };

                    if (tag.Protocol != InventoryInfo.ISO15693)
                        continue;

                    ListViewItem item = (ListViewItem)this.Invoke((Func<ListViewItem>)(() =>
                    {
                        return ListViewUtil.FindItem(this.listView_tags, tag.UID, COLUMN_UID);
                    }));

                    if (item == null)
                    {
                        // 可能是因为用户在界面上把列表清空了
                        return new ProcessResult
                        {
                            Value = -1,
                            ErrorInfo = "item == null"
                        };
                    }
                    Debug.Assert(item != null);

                    var iteminfo = item.Tag as ItemInfo;

                    if (iteminfo.State == "disable")
                    {
                        filtered_count++;
                        continue;
                    }

                    // 跳过已经成功的项目
                    if (iteminfo.State == "succeed")
                        continue;

                    // 从 UID --> UII 对照表中获得 PII 和 OI
                    if (iteminfo.UII == null)
                    {
                        if (DataModel.UidExsits(tag.UID, out string uii) == true)
                        {
                            iteminfo.UII = uii;

                            this.Invoke((Action)(() =>
                            {
                                // 清除残留的 error 状态
                                if (iteminfo.State == "error")
                                {
                                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "");
                                    SetItemColor(item, "normal");
                                    iteminfo.State = null;
                                    iteminfo.ErrorInfo = null;
                                }

                                RefreshItemByUII(item);
                            }));
                        }
                    }

                    // TODO: 根据动作判断哪些情况必须获得 tagInfo
                    if (iteminfo.TagInfo == null)
                    {
                        // 第一步，获得标签详细信息
                        InventoryInfo info = new InventoryInfo
                        {
                            Protocol = tag.Protocol,
                            UID = tag.UID,
                            AntennaID = tag.AntennaID
                        };
                        var get_result = GetTagInfo(tag.ReaderName, info, "");
                        if (get_result.Value == -1)
                        {
                            SetErrorInfo(item, get_result.ErrorInfo);
                            error_count++;
                            // 间隔一定时间才鸣叫
                            ErrorSound();
                            continue;
                        }

                        iteminfo.TagInfo = get_result.TagInfo;

                        // 第二步，刷新 PII 等栏目
                        this.Invoke((Action)(() =>
                        {
                            // 2021/1/14
                            // 清除残留的 error 状态
                            if (iteminfo.State == "error")
                            {
                                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "");
                                SetItemColor(item, "normal");
                                iteminfo.State = null;
                                iteminfo.ErrorInfo = null;
                            }

                            RefreshItem(item);
                        }));
                    }

                    if (iteminfo.State == "disable")
                    {
                        filtered_count++;
                        continue;
                    }

                    if (iteminfo.State == "succeed")
                        continue;

                    // 跳过以前已经处理过的项目
                    if (iteminfo.State == "cross")
                        continue;

                    // 第三步，执行修改动作
                    var action_result = DoAction(item);
                    if (action_result.Value == -1)
                    {
                        error_count++;
                        if (action_result.ErrorCode == "piiVerifyError")
                        {
                            verify_error_count++;

                            // TODO: 发出特定的声音，表示这是校验问题
                        }
                        else
                        {
                            // 间隔一定时间才鸣叫
                            ErrorSound();
                        }
                    }
                    else
                    {
                        process_count++;
                        SoundMaker.SucceedSound();
                        /*
                        // 2021/1/14
                        if (iteminfo.State == "error")
                        {
                            error_count++;
                            continue;
                        }
                        */
                    }
                }

                // 返回 0 表示全部完成，没有遇到出错的情况
                return new ProcessResult
                {
                    Value = error_count,
                    ErrorCount = error_count,
                    ProcessCount = process_count,
                    VerifyErrorCount = verify_error_count,
                };
            }
            finally
            {
                // DataModel.DecApiCount();
            }
        }

        static async Task ProcessAsync(ProcessInfo info)
        {
            var entity = info.Entity;
            info.State = "processing";
            try
            {
                // throw new Exception("testing processing");

                if (info.IsTaskCompleted("getItemXml") == false)
                {
                    // 获得册记录和书目摘要
                    // .Value
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    GetEntityDataResult result = null;
                    if (DataModel.Protocol == "sip")
                    {
                        bool isLocal = StringUtil.IsInList("inventory", DataModel.SipLocalStore);

                        result = await SipChannelUtil.GetEntityDataAsync(entity.PII,
                            entity.GetOiOrAoi(),
                            isLocal ? "network,localInventory" : "network");
                        if (result.Value != -1)
                        {
                            // 顺便保存到本地数据库
                        }
                    }
                    else
                    {
                        // 这里预先检查，不让 OI 为空的请求发给 dp2library 服务器(实际上发出请求是有可能成功响应的)
                        if (string.IsNullOrEmpty(entity.GetOiOrAoi()))
                        {
                            result = new GetEntityDataResult
                            {
                                Value = -1,
                                ErrorInfo = "RFID 标签中机构代码不允许为空",
                                ErrorCode = "NotFound"
                            };
                        }
                        else
                            result = await LibraryChannelUtil.GetEntityDataAsync(entity.GetOiPii(true)/*entity.PII*/, "network");
                    }

                    /*
                    // testing
                    result.Value = -1;
                    result.ErrorInfo = "获得册信息出错";
                    */
                    info.SetTaskInfo("getItemXml", result);
                    if (result.Value == -1
                        || result.Value == 0
                        || result.ErrorCode == "NotFound")
                    {
                        // 2021/1/19
                        FormClientInfo.Speak($"{entity.PII} 无法获得册信息", false, false);

                        entity.BuildError("getItemXml", result.ErrorInfo, result.ErrorCode);
                    }
                    else
                    {
                        entity.BuildError("getItemXml", null, null);

                        if (string.IsNullOrEmpty(result.Title) == false)
                            entity.Title = GetCaption(result.Title);
                        if (string.IsNullOrEmpty(result.ItemXml) == false)
                        {
                            if (info != null)
                                info.ItemXml = result.ItemXml;
                            entity.SetData(result.ItemRecPath, result.ItemXml);
                        }
                    }
                }

                /*
                // 请求 dp2library Inventory()
                if (string.IsNullOrEmpty(entity.PII) == false
                    && info != null && info.IsLocation == false)
                {
                    await BeginInventoryAsync(entity, PageInventory.ActionMode);
                }
                */

                // App.SetError("processing", null);
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"ProcessingAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                // App.SetError("processing", $"ProcessingAsync() 出现异常: {ex.Message}");
            }
            finally
            {
                info.State = "";
            }

        }

        public static string GetCaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";

            return text;
        }

        DateTime _lastErrorSound;

        void ErrorSound()
        {
            var now = DateTime.Now;
            if (now - _lastErrorSound > TimeSpan.FromMilliseconds(2000))
            {
                // 间隔一定时间才鸣叫
                SoundMaker.ErrorSound();
                _lastErrorSound = now;
            }
        }

        // 执行修改动作
        NormalResult DoAction(ListViewItem item)
        {
            var iteminfo = item.Tag as ItemInfo;

            string pii = "";
            string tou = "";
            string oi = "";
            string aoi = "";
            try
            {
                var taginfo = iteminfo.TagInfo;
                if (taginfo == null)
                {
                    // TODO: 显示出错信息
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "taginfo == null"
                    };
                }

                LogicChip chip = null;

                if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                {
                    throw new NotImplementedException("暂不支持");
                }
                else
                {
                    // *** ISO15693 HF
                    if (taginfo.Bytes != null)
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        chip = LogicChip.From(taginfo.Bytes,
            (int)taginfo.BlockSize,
            "");
                        pii = chip.FindElement(ElementOID.PII)?.Text;
                    }
                    else
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"标签 {taginfo.UID} 没有数据体"
                        };
                    }
                }

                tou = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;

                if (string.IsNullOrEmpty(oi))
                {
                    oi = chip?.FindElement(ElementOID.OI)?.Text;
                    aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                }

                bool changed = false;

                if (_action == null)
                    return new NormalResult();

                // 校验 PII
                if (_action.VerifyPii)
                {
                    if (string.IsNullOrEmpty(pii))
                    {
                        string error = $"校验 PII 发现问题: PII 为空";
                        SetErrorInfo(item, error);
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = error,
                            ErrorCode = "piiVerifyError"
                        };
                    }

                    if (string.IsNullOrEmpty(_action.PiiVerifyRule) == false)
                    {
                        var type = GetVerifyType(tou);
                        if (type == null)
                        {
                            // 不可知的类型，因而无法进行号码校验
                            // TODO: 此情况写入操作日志
                        }
                        else
                        {
                            var verify_result = _action.VerifyBarcode(type, pii);
                            if (verify_result.OK == false)
                            {
                                string error = $"校验 PII 发现问题: {verify_result.ErrorInfo}";
                                SetErrorInfo(item, error);
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = error,
                                    ErrorCode = "piiVerifyError"
                                };
                            }
                        }
                    }
                }

                if (_action.OI != null && oi != _action.OI)
                {
                    chip?.RemoveElement(ElementOID.AOI);
                    chip?.SetElement(ElementOID.OI, _action.OI);
                    changed = true;
                }
                if (_action.AOI != null && aoi != _action.AOI)
                {
                    chip?.RemoveElement(ElementOID.OI);
                    chip?.SetElement(ElementOID.AOI, _action.AOI);
                    changed = true;
                }

                bool new_eas = taginfo.EAS;
                if (string.IsNullOrEmpty(_action.ModifyEas) == true
                    || _action.ModifyEas == "不修改")
                {
                    // 不修改
                }
                else
                {
                    new_eas = _action.ModifyEas.ToLower() == "on" ? true : false;
                    if (taginfo.EAS != new_eas)
                    {
                        changed = true;
                    }
                }

                /*
                if (_action.LinkUID)
                {
                    AddUidEntry(iteminfo.Tag.UID, pii);
                }
                */

#if NO
                if (/*_action.WriteUidPiiLog &&*/ iteminfo.UidPiiLogWrited == false)
                {
                    if (string.IsNullOrEmpty(pii) == false)
                    {
                        string new_oi = chip?.FindElement(ElementOID.OI)?.Text;
                        string new_aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                        
                        DataModel.WriteToUidLogFile(iteminfo.Tag.UID, 
                            MakeOiPii(pii, new_oi, new_aoi));
                        
                        iteminfo.UidPiiLogWrited = true;
                    }
                }
#endif

                // 写回标签
                if (changed)
                {
                    var tag = iteminfo.Tag;
                    var new_tag_info = GetTagInfo(taginfo, chip, new_eas);
                    // 写入标签
                    var write_result = WriteTagInfo(tag.ReaderName,
                        taginfo,
                        new_tag_info);
                    if (write_result.Value == -1)
                    {
                        string error = $"写入标签出错: {write_result.ErrorInfo}";
                        SetErrorInfo(item, error);
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                    }

                    iteminfo.TagInfo = new_tag_info;
                    iteminfo.State = "succeed";
                    this.Invoke((Action)(() =>
                    {
                        RefreshItem(item);
                        ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "修改成功");
                        SetItemColor(item, "changed");
                    }));

                    AddToProcessed(iteminfo.Tag.UID, iteminfo);

                    /*
                    WriteComplete?.Invoke(this, new WriteCompleteventArgs
                    {
                        Chip = chip,
                        TagInfo = new_tag_info
                    });
                    */
                }
                else
                {
                    // 没有发生实质性修改，显示为特定状态

                    iteminfo.State = "succeed";
                    this.Invoke((Action)(() =>
                    {
                        SetItemColor(item, "notchanged");
                        ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "没有发生修改");
                    }));

                    AddToProcessed(iteminfo.Tag.UID, iteminfo);
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                string error = "exception:" + ex.Message;
                SetErrorInfo(item, error);
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = error
                };
            }
        }

        public static string MakeOiPii(string pii, string oi, string aoi)
        {
            if (string.IsNullOrEmpty(pii))
                return ".";
            if (string.IsNullOrEmpty(oi) == false)
                return oi + "." + pii;
            if (string.IsNullOrEmpty(aoi) == false)
                return aoi + "." + pii;
            return "." + pii;
        }


        public static string GetVerifyType(string tu)
        {
            // 10 图书; 80 读者证; 30 层架标
            if (tu == "10")
                return "entity";
            if (tu == "80")
                return "patron";
            if (tu.StartsWith("3"))
                return "shelf";
            return null;    // 无法识别的 tou
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

                // new_tag_info.DSFID = LogicChip.DefaultDSFID;  // 图书

                new_tag_info.SetEas(eas);
                return new_tag_info;
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
        }


        Hashtable _uidTable = new Hashtable();

        void AddUidEntry(string uid, string pii)
        {
            lock (_uidTable.SyncRoot)
            {
                _uidTable[uid] = pii;
            }
        }

        void ClearUidTable()
        {
            lock (_uidTable.SyncRoot)
            {
                _uidTable.Clear();
            }
        }

        void RefreshItemByUII(ListViewItem item)
        {
            string pii = "(尚未填充)";
            string tou = "";
            string eas = "";
            string afi = "";
            string oi = "";
            string aoi = "";

            var iteminfo = item.Tag as ItemInfo;

            try
            {
                {
                    tou = "10"; // 假定为图书
                    eas = "?";  // 表示 EAS 状态不确定
                    afi = "?";

                    DataModel.ParseOiPii(iteminfo.UII, out pii, out oi);
                }

                ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);

                ListViewUtil.ChangeItemText(item, COLUMN_TU, tou);
                ListViewUtil.ChangeItemText(item, COLUMN_EAS, eas);
                ListViewUtil.ChangeItemText(item, COLUMN_AFI, afi);
                ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
                ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);
            }
            catch (Exception ex)
            {
                SetErrorInfo(item, "exception:" + ex.Message);
            }
        }


        // 刷新一个 ListViewItem 的所有列显示
        void RefreshItem(ListViewItem item)
        {
            string pii = "(尚未填充)";
            string tou = "";
            string eas = "";
            string afi = "";
            string oi = "";
            string aoi = "";

            var iteminfo = item.Tag as ItemInfo;

            try
            {
                var taginfo = iteminfo.TagInfo;
                if (taginfo != null)
                {
                    LogicChip chip = null;

                    if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        throw new NotImplementedException("暂不支持");
                    }
                    else
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

                    tou = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;
                    eas = taginfo.EAS ? "On" : "Off";
                    afi = Element.GetHexString(taginfo.AFI);

                    if (string.IsNullOrEmpty(oi))
                    {
                        oi = chip?.FindElement(ElementOID.OI)?.Text;
                        aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                    }
                }

                ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);

                ListViewUtil.ChangeItemText(item, COLUMN_TU, tou);
                ListViewUtil.ChangeItemText(item, COLUMN_EAS, eas);
                ListViewUtil.ChangeItemText(item, COLUMN_AFI, afi);
                ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
                ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);


                /*
                // 过滤 TU
                if (FilterTU(_action.FilterTU, tou))
                {
                    // iteminfo.Disabled = false;
                }
                else
                {
                    // 被过滤掉的
                    iteminfo.State = "disable";
                    iteminfo.ErrorInfo = "被过滤掉";
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, iteminfo.ErrorInfo);
                    SetItemColor(item, "disable");
                }
                */
            }
            catch (Exception ex)
            {
                SetErrorInfo(item, "exception:" + ex.Message);
            }
        }

        /*
图书
读者证
层架标
所有类别
* */
        static bool FilterTU(string filter, string tu)
        {
            if (string.IsNullOrEmpty(filter) || filter == "所有类别")
                return true;

            if (tu == filter)
                return true;
            // 10 图书; 80 读者证; 30 层架标
            if (tu.StartsWith("1") && filter == "图书")
                return true;
            if (tu.StartsWith("8") && filter == "读者证")
                return true;
            if (tu == "30" && filter == "层架标")
                return true;

            return false;
        }

        // 设置指定行的错误信息栏，并改变背景色
        void SetErrorInfo(ListViewItem item, string error)
        {
            var iteminfo = item.Tag as ItemInfo;
            if (iteminfo != null)
            {
                iteminfo.ErrorInfo = error;
                iteminfo.State = "error";
            }

            this.Invoke((Action)(() =>
            {
                string old_value = ListViewUtil.GetItemText(item, COLUMN_ERRORINFO);
                if (old_value != error)
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, error);
                SetItemColor(item, "error");
            }));
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

        // 清空列表
        private void toolStripButton_clearList_Click(object sender, EventArgs e)
        {
            this.listView_tags.Items.Clear();
        }

        // 跳过本次扫描内循环(即语音不断提示“调整天线位置”的循环)
        private void toolStripButton_nextScan_Click(object sender, EventArgs e)
        {
            var button = sender as ToolStripButton;
            var cancel = button.Tag as CancellationTokenSource;
            cancel?.Cancel();
        }

        public void EnableControls(bool enable)
        {
            this.Invoke((Action)(() =>
            {
                this.listView_tags.Enabled = enable;
                this.toolStrip1.Enabled = enable;
            }));
        }

        public string MessageText
        {
            get
            {
                return this.toolStripStatusLabel1.Text;
            }
            set
            {
                this.toolStripStatusLabel1.Text = value;
            }
        }

        #region 标签缓存

        public NormalResult WriteTagInfo(string one_reader_name,
    TagInfo old_tag_info,
    TagInfo new_tag_info)
        {
            /* // 第一种方法，清除对应的缓存事项
            ClearCacheTagTable(new_tag_info.UID);
            if (new_tag_info.UID != old_tag_info.UID)
                ClearCacheTagTable(old_tag_info.UID);
            */
            var result = RfidManager.WriteTagInfo(one_reader_name,
                old_tag_info,
                new_tag_info);
            /*
            var result = DataModel.WriteTagInfo(one_reader_name,
    old_tag_info,
    new_tag_info);
            */
            // 第二种方法，直接修改缓存中的内容为新内容
            SetCacheTagInfo(new_tag_info.UID, new_tag_info);
            return result;
        }

        public GetTagInfoResult GetTagInfo(
    string one_reader_name,
    InventoryInfo info,
    string style)
        {
            // 先从 cache 里面找
            var taginfo = GetCachedTagInfo(info.UID);
            if (taginfo != null)
                return new GetTagInfoResult { TagInfo = taginfo };

            // 再真正从读写器读
            var result = RfidManager.GetTagInfo(one_reader_name,
                info.UID,
                info.AntennaID);
            /*
            var result = DataModel.GetTagInfo(one_reader_name,
                info,
                style);
            */
            if (result.Value != -1 && result.TagInfo != null)
            {
                // 存入 cache
                SetCacheTagInfo(info.UID, result.TagInfo);
            }

            return result;
        }

        // 标签信息缓存
        // uid --> TagInfo
        Hashtable _tagTable = new Hashtable();

        public TagInfo GetCachedTagInfo(string uid)
        {
            lock (_tagTable.SyncRoot)
            {
                return (TagInfo)_tagTable[uid];
            }
        }

        public void SetCacheTagInfo(string uid, TagInfo taginfo)
        {
            lock (_tagTable.SyncRoot)
            {
                // 防止缓存规模失控
                if (_tagTable.Count > 1000)
                    _tagTable.Clear();
                _tagTable[uid] = taginfo;
            }
        }

        public void ClearCacheTagTable(string uid)
        {
            lock (_tagTable.SyncRoot)
            {
                if (string.IsNullOrEmpty(uid))
                {
                    _tagTable.Clear();
                }
                else
                {
                    _tagTable.Remove(uid);
                }
            }
        }

        #endregion

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

    }

    // 修改动作
    class ActionInfo
    {
        public bool SetUID { get; set; }
        public bool SetCurrentLocation { get; set; }
        public bool SetLocation { get; set; }
        public bool VerifyEAS { get; set; }
        public bool SlowMode { get; set; }

        public string BatchNo { get; set; }
        public string Location { get; set; }


        // Type of usage 过滤
        public string FilterTU { get; set; }    // 图书/读者证/层架标/所有类别 (空=所有类型)
        public string OI { get; set; }
        public string AOI { get; set; }

        // public bool LinkUID { get; set; }

        public string ModifyEas { get; set; }   // 不修改/On/Off (空=不修改)

        public bool WriteUidPiiLog { get; set; }

        public bool VerifyPii { get; set; }

        string _piiVerifyRule = null;
        public string PiiVerifyRule
        {
            get
            {
                return _piiVerifyRule;
            }
            set
            {
                _piiVerifyRule = value;
                _validator = null;
            }
        }

        BarcodeValidator _validator = null;

        public ValidateResult VerifyBarcode(string type,
            string barcode)
        {
            if (string.IsNullOrEmpty(_piiVerifyRule))
                throw new ArgumentException("尚未设置 PiiVerifyRule");

            if (_validator == null)
                _validator = new BarcodeValidator(_piiVerifyRule);

            return _validator.ValidateByType(
                type,
                barcode);
        }
    }
}
