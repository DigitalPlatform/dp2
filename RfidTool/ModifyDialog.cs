// #define SIMU_ERROR  // 模拟第一次写入 UHF 标签发生错误，然后自动重试写入

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
using DigitalPlatform.RFID;
using DigitalPlatform.LibraryServer.Common;

namespace RfidTool
{
    public partial class ModifyDialog : Form
    {
        public event WriteCompleteEventHandler WriteComplete = null;

        public ModifyDialog()
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
        private void toolStripButton_begin_Click(object sender, EventArgs e)
        {
            using (BeginModifyDialog dlg = new BeginModifyDialog())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "beginModifyDialog", "state");

                dlg.UiState = ClientInfo.Config.Get("BeginModifyDialog", "uiState", "");
                dlg.ShowDialog(this);
                ClientInfo.Config.Set("BeginModifyDialog", "uiState", dlg.UiState);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

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
                    SwitchMethod = dlg.SwitchMethod,
                };
            }

            // TODO: 开始前用一个概要对话框显示确认一下本次批处理要进行的修改操作
            _cancel?.Dispose();
            _cancel = new CancellationTokenSource();
            BeginModify(_cancel.Token);
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
            _ = Task.Factory.StartNew(
                async () =>
                {
                    // 暂停基本循环
                    DataModel.PauseLoop();
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
                            var result = DataModel.ListTags(readerNameList, "");  // getTagInfo 是为了可以获得 UHF 标签的 TID
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
                                        var process_result = await ProcessTagsAsync(result.Results, current_token);
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
                        DataModel.ContinueLoop();
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
        const int COLUMN_TID = 11;

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
                    if (tag.Protocol == InventoryInfo.ISO14443A)
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

            // 是否被过滤掉
            // public bool Disabled { get; set; }
            public string State { get; set; }   // disable/cross/succeed/error

            public string ErrorInfo { get; set; }

            // UID --> PII 日志是否成功写入过了
            public bool UidPiiLogWrited { get; set; }

            // 2025/9/23
            // 标签所用的 UHF 标准。空/gb/gxlm 其中空表示未知
            public string UhfProtocol { get; set; }

            // 2025/9/23
            public Exception Exception { get; set; }

        }

        // 用 OneTag 刷新一个 ListViewItem 的相关列显示
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

            Debug.Assert(tag.Protocol != "ISO14443A");
            ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL, tag.Protocol);

            if (tag.Protocol == InventoryInfo.ISO18000P6C)
            {
                byte[] tid_bank = tag.TagInfo?.Tag as byte[];
                ListViewUtil.ChangeItemText(item, COLUMN_TID, GetTidHex(tid_bank));
            }

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

        /*
2025-09-30 12:03:37.217 +08:00 [ERR] 修改循环出现异常: Type: System.ArgumentNullException
Message: 值不能为 null。
参数名: first
Stack:
   在 System.Windows.Forms.Control.MarshaledInvoke(Control caller, Delegate method, Object[] args, Boolean synchronous)
   在 System.Windows.Forms.Control.Invoke(Delegate method, Object[] args)
   在 RfidTool.ModifyDialog.<ProcessTagsAsync>d__52.MoveNext()
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   在 RfidTool.ModifyDialog.<>c__DisplayClass21_0.<<BeginModify>b__0>d.MoveNext()
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   在 RfidTool.ModifyDialog.<>c__DisplayClass21_0.<<BeginModify>b__0>d.MoveNext()         * 
         * */
        // 处理每一个标签的修改动作
        // result.Value:
        //      -1  表示遇到了严重出错，要停止循环调用本函数
        //      0   表示全部完成，没有遇到出错的情况
        //      >0  表示处理过程中有事项出错。后继需要调主循环调用本函数
        async Task<ProcessResult> ProcessTagsAsync(List<OneTag> tags,
            CancellationToken token)
        {
            if (tags == null)
                return new ProcessResult();

            DataModel.IncApiCount();
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

                    if (tag.Protocol == InventoryInfo.ISO14443A)
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

                    if (iteminfo.TagInfo == null)
                    {
                        // 第一步，获得标签详细信息
                        InventoryInfo info = new InventoryInfo
                        {
                            Protocol = tag.Protocol,
                            UID = tag.UID,
                            AntennaID = tag.AntennaID
                        };
                        var get_result = GetTagInfo(tag.ReaderName,
                            info,
                            "tid"); // 如果需要写入 tid --> barcode 日志
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

                    // 尝试将先前出错的标签重试写入
                    {
                        NormalResult retry_ret = new NormalResult();
                        this.Invoke((Action)(() =>
                        {
                            retry_ret = RetryWriteTag(item);
                            if (retry_ret.Value == 1)
                            {
                                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "自动重试写入修改成功");
                                SetItemColor(item, "changed");
                                process_count++;
                            }
                        }));
                        if (retry_ret.Value != 0)
                            continue;   // EPC 可能已经被改变，不要继续后面的批处理动作
                    }

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

                            if (action_result.ErrorCode == "cancel")
                                return new ProcessResult
                                {
                                    Value = -1,
                                    ErrorInfo = action_result.ErrorInfo
                                };
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
                DataModel.DecApiCount();
            }
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
                    // 注1: taginfo.EAS 在调用后可能被修改
                    // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                    var uhf_info = RfidTagList.GetUhfChipInfo(taginfo/*, "convertValueToGB,ensureChip"*/); // "dontCheckUMI"

                    if (string.IsNullOrEmpty(uhf_info.ErrorInfo) == false)
                    {
                        /*
                        var ex = new Exception(uhf_info.ErrorInfo);
                        iteminfo.Exception = ex;
                        ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + ex.Message);
                        SetItemColor(item, "error");
                        return;
                        */
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = uhf_info.ErrorInfo
                        };
                    }

                    // TODO: 对于 .Bytes 缺失的畸形 UHF 标签，最好是尽量解析内容，然后给出警告信息解释问题所在
                    // 单独严格解析一次标签内容

                    chip = uhf_info.Chip;
                    Debug.Assert(chip != null);

                    // taginfo.EAS 可能会被修改
                    iteminfo.UhfProtocol = uhf_info.UhfProtocol;
                    pii = uhf_info.PII;
                    if (uhf_info.ContainOiElement)
                        aoi = uhf_info.OI;
                    else
                        oi = uhf_info.OI;
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

                if (string.IsNullOrEmpty(oi)
                    && taginfo.Protocol == InventoryInfo.ISO15693)
                {
                    oi = chip?.FindElement(ElementOID.OI)?.Text;
                    aoi = chip?.FindElement(ElementOID.AOI)?.Text;
                }

                bool changed = false;

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

                // 2023/11/26
                RfidTagList.SetTagInfoEAS(taginfo);

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

                // 写入 UID --> PII 对照日志
                if (/*_action.WriteUidPiiLog &&*/ iteminfo.UidPiiLogWrited == false)
                {
                    if (string.IsNullOrEmpty(pii) == false)
                    {
                        string new_oi = chip?.FindElement(ElementOID.OI)?.Text;
                        string new_aoi = chip?.FindElement(ElementOID.AOI)?.Text;

                        string uid = iteminfo.Tag.UID;
                        if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                        {
                            var bytes = taginfo.Tag as byte[];
                            uid = ByteArray.GetHexTimeStampString(bytes)?.ToUpper();
                        }

                        DataModel.WriteToUidLogFile(uid,
                            MakeOiPii(pii, new_oi, new_aoi));

                        iteminfo.UidPiiLogWrited = true;
                    }
                }

                // 判断内容格式是否需要切换
                if (changed == false && taginfo.Protocol == InventoryInfo.ISO18000P6C)
                {
                    if (iteminfo.UhfProtocol == "gb"
                        && _action.SwitchMethod == "UHF国标-->高校联盟")
                        changed = true;
                    else if (iteminfo.UhfProtocol == "gxlm"
    && _action.SwitchMethod == "高校联盟-->UHF国标")
                        changed = true;
                }

                // 为判断 UHF 标签内容是否有写入 User Bank 的，需要先创建好标签内容
                var tag = iteminfo.Tag;
                TagInfo new_tag_info = null;
                try
                {
                    new_tag_info = PrepareTagInfo(taginfo, chip, new_eas);
                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"DoAction() PrepareTagInfo() exception: {ExceptionUtil.GetDebugText(ex)}");
                    string error = "exception1:" + ex.Message;
                    SetErrorInfo(item, error);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = error,
                        ErrorCode = "cancel",
                    };
                }

                // 判断 User Bank 的存在与否是否发生了变化
                if (changed == false && taginfo.Protocol == InventoryInfo.ISO18000P6C)
                {
                    // 进行一些检查

                    if (DataModel.WriteUhfUserBank
                        && new_tag_info.Bytes != taginfo.Bytes)
                        changed = true;
                    else if (DataModel.WriteUhfUserBank == false
    && taginfo.Bytes != null
    && new_tag_info.Bytes == null)
                        changed = true;
                }

                // 写回标签
                if (changed)
                {
                    /*
                    var tag = iteminfo.Tag;
                    var new_tag_info = PrepareTagInfo(taginfo, chip, new_eas);
                    */

#if SIMU_ERROR
                    // testing
                    // 
                    string save_uid = new_tag_info.UID;
                    if (_simuErrorCount == 0)
                        new_tag_info.UID = "A8203400000300050730303030303033";
#endif

                    // 写入标签
                    var write_result = WriteTagInfo(tag.ReaderName,
                        taginfo,
                        new_tag_info);
                    if (write_result.Value == -1
#if SIMU_ERROR
                        || _simuErrorCount == 0
#endif
                        )
                    {
#if SIMU_ERROR
                        if (_simuErrorCount == 0
                            && string.IsNullOrEmpty(write_result.ErrorInfo))
                            write_result.ErrorInfo = "模拟写入错误";
                        _simuErrorCount++;
                        new_tag_info.UID = save_uid;
#endif

                        // 把未成功写入的 EPC 和 TID 记载到内存，便于后面重试写入
                        Debug.Assert(taginfo.Tag != null);
                        var tid = taginfo.Tag as byte[];
                        var error_info = new WriteErrorInfo
                        {
                            TID = tid,
                            OldTagInfo = taginfo,
                            NewTagInfo = new_tag_info,
                        };
                        SetWriteErrorInfo(tid, error_info);

                        this.TryInvoke(() =>
                        {
                            var dlg = Program.MainForm.OpenWriteErrorDialog();
                            dlg.Add(error_info, $"写标签出错: {write_result.ErrorInfo}");
                        });

                        /*
                        // 及时刷新 ListViewItem 内容，为后继重试写入做准备
                        this.Invoke((Action)(() =>
                        {
                            RefreshItem(item);
                        }));
                        */

                        string error = $"写入标签出错: {write_result.ErrorInfo}";
                        SetErrorInfo(item, error);
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = error,
                            ErrorCode = tag.Protocol == InventoryInfo.ISO18000P6C ? "cancel" : "",  // 超高频写入错误时，可能已经损坏 EPC 内容，所以要中断整个处理循环，重新开始了
                        };
                    }
                    else
                    {
                        /*
                        // testing

                        // 把未成功写入的 EPC 和 TID 记载到内存，便于后面重试写入
                        Debug.Assert(taginfo.Tag != null);
                        var tid = taginfo.Tag as byte[];
                        var error_info = new WriteErrorInfo
                        {
                            TID = tid,
                            OldTagInfo = taginfo,
                            NewTagInfo = new_tag_info,
                        };
                        _writeErrorTable[tid] = error_info;

                        this.TryInvoke(() =>
                        {
                            var dlg = Program.MainForm.OpenWriteErrorDialog();
                            dlg.Add(error_info);
                        });

                        string error = $"写入标签出错: {write_result.ErrorInfo}";
                        SetErrorInfo(item, error);
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                        */
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

                    WriteComplete?.Invoke(this, new WriteCompleteventArgs
                    {
                        Chip = chip,
                        TagInfo = new_tag_info
                    });
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
                ClientInfo.WriteErrorLog($"DoAction() exception: {ExceptionUtil.GetDebugText(ex)}");
                string error = "exception1:" + ex.Message;
                SetErrorInfo(item, error);
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = error
                };
            }
        }

#if SIMU_ERROR
        int _simuErrorCount = 0; // 用于测试
#endif

        public static string MakeOiPii(string pii, string oi, string aoi)
        {
            // 2021/4/29
            if (pii != null && pii.Contains(" "))
            {
                string error = $"MakeOiPii() pii='{pii}' 出现了意外的空格字符";
                ClientInfo.WriteErrorLog(error);
                throw new Exception(error);
            }

            if (oi != null && oi.Contains(" "))
            {
                string error = $"MakeOiPii() oi='{oi}' 出现了意外的空格字符";
                ClientInfo.WriteErrorLog(error);
                throw new Exception(error);
            }

            if (aoi != null && aoi.Contains(" "))
            {
                string error = $"MakeOiPii() aoi='{aoi}' 出现了意外的空格字符";
                ClientInfo.WriteErrorLog(error);
                throw new Exception(error);
            }

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
            // 2021/4/30
            if (tu == null)
                return null;

            // 10 图书; 80 读者证; 30 层架标
            if (tu.StartsWith("1"))
                return "entity";
            if (tu.StartsWith("8"))
                return "patron";
            if (tu.StartsWith("3"))
                return "shelf";
            return null;    // 无法识别的 tou
        }

#if OLD
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
#endif

        string _typeOfUsage = "10"; // 10 图书; 80 读者证; 30 层架标

        // 准备即将写入的内容
        public TagInfo PrepareTagInfo(TagInfo existing,
LogicChip chip,
bool eas)
        {
            bool dontWarningInvalidGaoxiaoOI = true;

            if (existing.Protocol == InventoryInfo.ISO15693)
            {
                ScanDialog.SetTypeOfUsage(chip, this._typeOfUsage);

                TagInfo new_tag_info = existing.Clone();
                new_tag_info.Bytes = chip.GetBytes(
                    (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                    (int)new_tag_info.BlockSize,
                    LogicChip.GetBytesStyle.None,
                    out string block_map);
                new_tag_info.LockStatus = block_map;

                // 上架状态
                new_tag_info.SetEas(eas);

                return new_tag_info;
            }

            if (existing.Protocol == InventoryInfo.ISO18000P6C)
            {
                var build_user_bank = DataModel.WriteUhfUserBank;

                // 读者卡和层架标必须有 User Bank，不然 TU 字段没有地方放
                if (build_user_bank == false
    && this._typeOfUsage != "10")
                    throw new Exception($"{ScanDialog.GetCaption(this._typeOfUsage)}必须写入 User Bank");

                // TODO: 判断标签内容是空白/国标/高校联盟格式，采取不同的写入格式
                /*
高校联盟格式
国标格式
* */
                var isExistingGB = UhfUtility.IsISO285604Format(Element.FromHexString(existing.UID), existing.Bytes);

                bool dontSwitch = string.IsNullOrEmpty(_action.SwitchMethod) || _action.SwitchMethod == "[不切换]";

                TagInfo new_tag_info = existing.Clone();
                /*
高校联盟-->UHF国标
UHF国标-->高校联盟
[不切换]
                * */
                if (_action.SwitchMethod == "UHF国标-->高校联盟"
                    || (dontSwitch && isExistingGB == false))
                {
                    // 写入高校联盟数据格式
                    if (isExistingGB)
                    {
                        string warning = $"警告：即将用高校联盟格式覆盖原有国标格式";
                        DialogResult dialog_result = DialogResult.Yes;
                        if (_dontWarningChangeToDifferentFormat == false)
                        {
                            dialog_result = this.TryGet(() =>
                            {
                                return MessageDlg.Show(this,
                                    $"{warning}\r\n\r\n确实要覆盖？",
                                    $"ModifyDialog",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxDefaultButton.Button2,
                                    ref _dontWarningChangeToDifferentFormat,
                                    new string[] { "继续", "放弃" },
                                    "以后不再警告");
                            });
                        }
                        if (dialog_result == DialogResult.No)
                            throw new Exception("放弃写入");
                    }

                    // chip.SetElement(ElementOID.TypeOfUsage, tou);
                    if (chip != null)
                        ScanDialog.SetTypeOfUsage(chip, this._typeOfUsage);

                    /*
                    // 2023/10/24
                    // chip 中的 AOI 改到 OI 中。这是由“设置”对话框的局限造成的麻烦。(设置对话框中不允许非规范的机构代码填入 OI 文字框，只能填入 AOI 文字框)
                    {
                        var element_aoi = chip.FindElement(ElementOID.AOI);
                        if (element_aoi != null && string.IsNullOrEmpty(element_aoi.Text) == false)
                        {
                            chip.SetElement(ElementOID.OI, element_aoi.Text, false);
                            chip.RemoveElement(ElementOID.AOI);
                        }
                    }
                    */

                    // 2025/9/21
                    // 检查机构代码是否符合高校联盟 OI 的格式
                    var oi = chip?.FindElement(ElementOID.OI)?.Text;
                    if (string.IsNullOrEmpty(oi) == false
                        && GaoxiaoUtility.VerifyOI(oi) == false
                        && dontWarningInvalidGaoxiaoOI == false)
                    {
                        var dialog_result =
                            this.TryGet(() =>
                            {
                                return MessageDlg.Show(this,
                                $"警告: 机构代码 '{oi}' 不符合高校联盟格式的规定。\r\n\r\n若坚持写入，将被写入到 User Bank 中的 27(备用)元素。\r\n\r\n请问是否坚持写入？",
                                "写入高校联盟格式",
                                MessageBoxButtons.YesNo,
                                MessageBoxDefaultButton.Button2,
                                ref dontWarningInvalidGaoxiaoOI,
                                new string[] { "继续", "放弃" },
                                "以后不再警告");
                            });
                        if (dialog_result != DialogResult.Yes)
                            throw new Exception("放弃写入标签");
                    }

                    // 警告丢失 TypeOfUsage 和 OI(AOI)
                    if (chip != null)
                        VerifyElementsCondition(chip,
                            build_user_bank);

                    // 2025/9/21
                    /*
                    var epc_info = new GaoxiaoEpcInfo
                    {
                        Version = 4,
                        Lending = false,
                        Picking = 0,
                        Reserve = 0,
                    };
                    */
                    var epc_info = DataModel.BuildGaoxiaoEpcInfo();

                    // 可能抛出 ArgumentException
                    var result = GaoxiaoUtility.BuildTag(chip != null ? chip : new LogicChip(),
                        build_user_bank,
                        eas,
                        epc_info);
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = UhfUtility.EpcBankHex(result.EpcBank);  // existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                else if (_action.SwitchMethod == "高校联盟-->UHF国标"
                    || (dontSwitch && isExistingGB == true))
                {
                    // 写入国标数据格式
                    if (isExistingGB == false)
                    {
                        string warning = $"警告：即将用国标格式覆盖原有高校联盟格式";
                        DialogResult dialog_result = DialogResult.Yes;
                        if (_dontWarningChangeToDifferentFormat == false)
                        {
                            dialog_result = this.TryGet(() =>
                            {
                                return MessageDlg.Show(this,
                                    $"{warning}\r\n\r\n确实要覆盖？",
                                    $"ScanDialog",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxDefaultButton.Button2,
                                    ref _dontWarningChangeToDifferentFormat,
                                    new string[] { "继续", "放弃" },
                                    "以后不再警告");
                            });
                        }
                        if (dialog_result == DialogResult.No)
                            throw new Exception("放弃写入");
                    }
                    if (chip != null)
                        ScanDialog.SetTypeOfUsage(chip, this._typeOfUsage);

                    var result = UhfUtility.BuildTag(chip != null ? chip : new LogicChip(),
                        build_user_bank,
                        true,
                        eas ? "afi_eas_on" : "");
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    new_tag_info.Bytes = build_user_bank ? result.UserBank : null;
                    new_tag_info.UID = UhfUtility.EpcBankHex(result.EpcBank);  // existing.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                }
                else
                {
                    throw new ArgumentException($"出现了超出预料的组合 dontSwitch={dontSwitch} _action.SwitchMethod={_action.SwitchMethod} isExistingGB={isExistingGB}");
                }

                return new_tag_info;
            }

            throw new ArgumentException($"目前暂不支持 {existing.Protocol} 协议标签的写入操作");
        }

        bool _dontWarningLostTU = false;    // 不警告丢失 TypeOfUsage
        bool _dontWarningLostOI = false;    // 不警告丢失 OI
        bool _dontWarningChangeToDifferentFormat = false;   // 在转换为不同格式的时候不警告

        // 检查当不写入 UserBank 时可能和 chip 中哪些元素相矛盾。警告并尝试删除矛盾的元素
        bool VerifyElementsCondition(LogicChip chip,
            bool build_user_bank)
        {
            bool changed = false;
            if (build_user_bank == false)
            {
                var tou = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                if (IsNormalBookTou(tou) == false)
                {
                    // 如果具有非 '1?' 的 TypeOfUsage，这表明不应缺乏 User Bank。
                    // 因为如果缺了 User Bank，则会被默认为图书类型，这样就令读出标签的人产生误会了
                    // throw new ArgumentException($"又要写入 TypeOfUsage 元素(内容为 '{tou}')，又不让写入 User Bank，这种组合不被支持。因为这样会被读出时误当作图书类型标签");

                    DialogResult dialog_result = DialogResult.Yes;
                    if (_dontWarningLostTU == false)
                    {
                        dialog_result = this.TryGet(() =>
                        {
                            return MessageDlg.Show(this,
                            $"您选择了不写入 User Bank 内容，这样会丢失标签中原有的 TypeOfUsage 元素。\r\n\r\n请问是否继续写入？",
                            "丢失元素警告",
                            MessageBoxButtons.YesNo,
                            MessageBoxDefaultButton.Button2,
                            ref _dontWarningLostTU,
                            new string[] { "继续", "放弃" },
                            "以后不再警告");
                        });
                    }
                    if (dialog_result != DialogResult.Yes)
                        throw new Exception("放弃写入标签");

                    // 删除 chip 中 TypeOfUsage 元素
                    if (chip.RemoveElement(ElementOID.TypeOfUsage) != null)
                        changed = true;
                }

                var element_aoi = chip.FindElement(ElementOID.AOI);
                var element_oi = chip.FindElement(ElementOID.OI);

                string oi = element_oi?.Text;
                if (string.IsNullOrEmpty(oi))
                    oi = element_aoi?.Text;

                // 如果不让写入 user_bank，但又想要写入机构代码，那就只能包含在 PII 之内了
                if (build_user_bank == false && string.IsNullOrEmpty(oi) == false)
                {
                    // throw new ArgumentException("又要写入 OI 字段，又不让写入 User Bank，这种组合不被支持。因为高校联盟格式的 EPC 册号码中不允许出现字符 '.'，所以机构代码无法以 UII 部件方式进入 EPC Bank");

                    DialogResult dialog_result = DialogResult.Yes;
                    if (_dontWarningLostOI == false)
                    {
                        dialog_result = this.TryGet(() =>
                        {
                            return MessageDlg.Show(this,
                            $"您选择了不写入 User Bank 内容，这样会丢失标签中原有的机构代码 '{oi}'。\r\n\r\n请问是否继续写入？",
                            "丢失机构代码警告",
                            MessageBoxButtons.YesNo,
                            MessageBoxDefaultButton.Button2,
                            ref _dontWarningLostOI,
                            new string[] { "继续", "放弃" },
                            "以后不再警告");
                        });
                    }
                    if (dialog_result != DialogResult.Yes)
                        throw new Exception("放弃写入标签");

                    // TODO: 删除 chip 中 OI 或 AOI 元素
                    if (chip.RemoveElement(ElementOID.OI) != null)
                        changed = true;
                    if (chip.RemoveElement(ElementOID.AOI) != null)
                        changed = true;
                }

            }
            return changed;

            // 是否普通图书类型?
            bool IsNormalBookTou(string value)
            {
                /*
                if (string.IsNullOrEmpty(value)
    || value.StartsWith("1")
    || value.StartsWith("2")
    || value.StartsWith("7"))
                    return true;
                */
                // 注: 20 和 70 是特殊的图书，不被当作普通图书
                if (string.IsNullOrEmpty(value)
|| value.StartsWith("1"))
                    return true;
                return false;
            }
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
                        // throw new NotImplementedException("暂不支持");

                        // 注1: taginfo.EAS 在调用后可能被修改
                        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                        var uhf_info = RfidTagList.GetUhfChipInfo(taginfo/*, "convertValueToGB,ensureChip"*/); // "dontCheckUMI"

                        if (string.IsNullOrEmpty(uhf_info.ErrorInfo) == false)
                        {
                            var ex = new Exception(uhf_info.ErrorInfo);
                            iteminfo.Exception = ex;
                            ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + ex.Message);
                            SetItemColor(item, "error");
                            return;
                        }

                        // TODO: 对于 .Bytes 缺失的畸形 UHF 标签，最好是尽量解析内容，然后给出警告信息解释问题所在
                        // 单独严格解析一次标签内容

                        chip = uhf_info.Chip;
                        Debug.Assert(chip != null);

                        // taginfo.EAS 可能会被修改
                        iteminfo.UhfProtocol = uhf_info.UhfProtocol;
                        pii = uhf_info.PII;
                        if (uhf_info.ContainOiElement)
                            aoi = uhf_info.OI;
                        else
                            oi = uhf_info.OI;

                        tou = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;
                        eas = taginfo.EAS ? "On" : "Off";
                        afi = Element.GetHexString(taginfo.AFI);
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
                            pii = ScanDialog.GetPIICaption(chip.FindElement(ElementOID.PII)?.Text);
                        }
                    }

                    tou = chip?.FindElement(ElementOID.TypeOfUsage)?.Text;

                    // 2023/11/26
                    RfidTagList.SetTagInfoEAS(taginfo);

                    eas = taginfo.EAS ? "On" : "Off";
                    afi = Element.GetHexString(taginfo.AFI);

                    if (string.IsNullOrEmpty(oi)
                        && taginfo.Protocol == InventoryInfo.ISO15693)
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

                // 刷新协议栏
                if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
                {
                    string name = iteminfo.UhfProtocol;
                    if (iteminfo.UhfProtocol == "gxlm")
                        name = "高校联盟";
                    else if (iteminfo.UhfProtocol == "gb")
                        name = "国标";
                    ListViewUtil.ChangeItemText(item, COLUMN_PROTOCOL,
                        string.IsNullOrEmpty(name) ? taginfo.Protocol : taginfo.Protocol + ":" + name);

                    byte[] tid_bank = taginfo.Tag as byte[];
                    ListViewUtil.ChangeItemText(item, COLUMN_TID, GetTidHex(tid_bank));
                }

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
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"RefreshItem() exception: {ExceptionUtil.GetDebugText(ex)}");
                SetErrorInfo(item, "exception2:" + ex.Message);
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

            // 2021/4/30
            if (tu == null)
                return false;

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

        // 重试写入早先发生写入错误的标签内容
        void RetryWriteTags()
        {
            foreach (ListViewItem item in this.listView_tags.Items)
            {
                RetryWriteTag(item);
            }
        }

        // return:
        //      -1  重写时出错
        //      0   没有必要重写
        //      1   成功重写
        NormalResult RetryWriteTag(ListViewItem item)
        {
            var iteminfo = item.Tag as ItemInfo;
            if (iteminfo == null || iteminfo.TagInfo == null)
                return new NormalResult();

            if (iteminfo.TagInfo.Protocol == InventoryInfo.ISO18000P6C)
            {
                byte[] tid = iteminfo.TagInfo.Tag as byte[];
                // 用 TID 从 _writeErrorTable 中找到对应的 WriteErrorInfo
                var error_info = GetWriteErrorInfo(tid);
                if (error_info == null)
                    return new NormalResult();
                var uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                // 如果当前标签的 EPC 和 UserBank 原来的一样，说明标签是完整的，没有被损坏。就不用重写了。如果用户需要改写，用正常改写的方法就可以了
                if (uid == error_info.OldTagInfo.UID
                    && iteminfo.TagInfo.Bytes.SequenceEqual(error_info.OldTagInfo.Bytes))
                {
                    RemoveWriteErrorInfo(tid);
                    return new NormalResult();
                }

                var write_result = WriteTagInfo(iteminfo.TagInfo.ReaderName,
    iteminfo.TagInfo,
    error_info.NewTagInfo);
                if (write_result.Value == -1)
                {
                    string error = $"自动重试写入标签时出错: {write_result.ErrorInfo}";
                    SetErrorInfo(item, error);
                    error_info.RetryCount++;
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = error,
                        ErrorCode = write_result.ErrorCode
                    };
                }
                else
                {
                    // 写入成功，刷新显示
                    // TODO: 重新读入标签?
                    ListViewUtil.ChangeItemText(item, COLUMN_UID, error_info.NewTagInfo.UID);
                    RefreshItem(item);
                    RemoveWriteErrorInfo(tid);
                    return new NormalResult { Value = 1 };
                }
            }
            return new NormalResult();
        }

        public class WriteErrorInfo
        {
            public byte[] TID { get; set; }

            public string ReaderName { get; set; }

            public TagInfo OldTagInfo { get; set; }

            public TagInfo NewTagInfo { get; set; }

            public NormalResult ErrorResult { get; set; }

            public int RetryCount { get; set; }
        }

        // 存储写入标签时发生错误的详细信息。供后来重试写入
        // TID byte[] --> WriteErrorInfo
        Hashtable _writeErrorTable = new Hashtable();

        public WriteErrorInfo GetWriteErrorInfo(byte[] tid)
        {
            var hex = GetTidHex(tid);
            return _writeErrorTable[hex] as WriteErrorInfo;
        }

        public void SetWriteErrorInfo(byte[] tid, WriteErrorInfo info)
        {
            var hex = GetTidHex(tid);
            if (info == null)
                _writeErrorTable.Remove(hex);
            else
                _writeErrorTable[hex] = info;
        }

        public void RemoveWriteErrorInfo(byte[] tid)
        {
            var hex = GetTidHex(tid);
            _writeErrorTable.Remove(hex);

            //this.TryInvoke(() =>
            //{
                Program.MainForm.RemoveWriteErrorItem(hex);
            //});
        }

        public static string GetTidHex(byte[] tid)
        {
            if (tid == null)
                return "";
            if (tid.Length < 12)
                throw new ArgumentException("tid 长度不应小于 12");

            var head = tid.AsQueryable().Take(12).ToArray();
            var hex = ByteArray.GetHexTimeStampString(head).ToUpper();
            return hex;
        }

        public NormalResult WriteTagInfo(string one_reader_name,
    TagInfo old_tag_info,
    TagInfo new_tag_info)
        {
            /* // 第一种方法，清除对应的缓存事项
            ClearCacheTagTable(new_tag_info.UID);
            if (new_tag_info.UID != old_tag_info.UID)
                ClearCacheTagTable(old_tag_info.UID);
            */
            var result = DataModel.WriteTagInfo(one_reader_name,
    old_tag_info,
    new_tag_info);
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
            var result = DataModel.GetTagInfo(one_reader_name,
                info,
                style);
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
    }

    // 修改动作
    class ActionInfo
    {
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

        // 切换内容格式 的 方法
        /*
高校联盟-->UHF国标
UHF国标-->高校联盟
[不切换]
        * */
        public string SwitchMethod { get; set; }

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
