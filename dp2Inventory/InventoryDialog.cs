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
using static DigitalPlatform.RFID.LogicChip;
using static dp2Inventory.InventoryData;

namespace dp2Inventory
{
    public partial class InventoryDialog : Form
    {
        // 加入操作历史
        public event WriteCompleteEventHandler WriteComplete = null;

        // 加入 ShelfDialog
        public event AddBookEventHandler AddBook = null;

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

#if AUTO_TEST
            toolStripButton_test_nextTags.Visible = true;
#endif
        }

        private void InventoryDialog_Load(object sender, EventArgs e)
        {

        }

        private void InventoryDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel?.Cancel();

        }

        private void InventoryDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
#if AUTO_TEST
            FinishAutoTest();
#endif
        }

        // ActionInfo _action = null;

        bool _slowMode = false;

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

                _actionMode = dlg.ActionMode;

                CurrentLocation = dlg.LocationString;
                CurrentBatchNo = dlg.BatchNo;
                _slowMode = dlg.SlowMode;

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

            // TODO: 开始前用一个概要对话框显示确认一下本次批处理要进行的修改操作
            _cancel?.Dispose();
            _cancel = new CancellationTokenSource();

            // 装入 UID --> UII 对照关系
            DataModel.ClearUidTable();
            if (_slowMode == false)
            {
                strError = "";

                await Task.Run(async () =>
                {
                    FileDownloadDialog progress = null;
                    this.Invoke(new Action(() =>
                    {
                        progress = new FileDownloadDialog();
                        progress.Font = this.Font;
                        progress.Text = "正在将 UID --> UII 对照关系装入内存";
                        progress.Show(this);
                    }));
                    try
                    {
                        /*
                        progress.SetMessage("正在获取 dp2library 服务器配置");
                        var initial_result = LibraryChannelUtil.Initial();
                        if (initial_result.Value == -1)
                        {
                            strError = $"获得 dp2library 服务器配置失败: {initial_result.ErrorInfo}";
                            return;
                        }
                        */

                        progress.SetMessage("正在将 UID --> UII 对照关系装入内存");

                        Hashtable uid_table = new Hashtable();
                        NormalResult result = null;
                        if (DataModel.Protocol == "sip")
                            result = await DataModel.LoadUidTableAsync(uid_table,
                                (text, bytes, total) =>
                                {
                                    progress.SetProgress(text, bytes, total);
                                },
                                _cancel.Token);
                        else
                            result = LibraryChannelUtil.DownloadUidTable(
                                null,
                                uid_table,
                                (text, bytes, total) =>
                                {
                                    progress.SetProgress(text, bytes, total);
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
            }

            BeginModify(_cancel.Token);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if AUTO_TEST
        List<SimuTagInfo> _simuTagInfos = new List<SimuTagInfo>();

        // 初始化自动测试
        NormalResult InitialAutoTest(CancellationToken token)
        {
            List<string> names = new List<string> { "RL8600" };
            {
                var result = RfidManager.SimuTagInfo(
                    "switchToSimuMode",
                    null,
                    $"readerNameList:{StringUtil.MakePathList(names, "|")}");
                if (result.Value == -1)
                {
                    this.ShowMessageBox("simuReader", result.ErrorInfo);
                    return result;
                }
            }

            {
                var result = LibraryChannelUtil.DownloadTagsInfo(null,
                    100,
                    null,
                    token);
                if (result.Value == -1)
                {
                    this.ShowMessageBox("simuReader", result.ErrorInfo);
                    return result;
                }
                _simuTagInfos = result.TagInfos;

                foreach (var info in _simuTagInfos)
                {
                    info.ReaderName = names[0];
                    info.AntennaID = 1;
                }
            }

            // 首次初始化标签
            _simuTagIndex = 0;
            PrepareSimuRfidTag();

            return new NormalResult();
        }
#endif 

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            {
                var button = sender as ToolStripButton;
                var cancel = button.Tag as CancellationTokenSource;
                cancel?.Cancel();
            }

            _cancel?.Cancel();
        }

#if REMOVED
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
#endif

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

        void SetPauseText()
        {
            this.Invoke((Action)(() =>
            {
                if (_pause)
                    this.toolStripButton_pause.Text = "继续";
                else
                    this.toolStripButton_pause.Text = "暂停";
            }));
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
            this.toolStripButton_pause.Enabled = true;
            SetPauseText();
            // ClearUidTable();
            ClearProcessedTable();
            ClearCacheTagTable(null);
            InitialRfidManager(DataModel.RfidCenterUrl);

            {
                // 清除当前层架标位置
                CurrentShelfNo = null;
                // 更新状态行显示
                SetCurrentShelfNoLabel(CurrentShelfNo);
            }

            this.Invoke((Action)(() =>
            {
                toolStripStatusLabel_rpanMode.Text = "";
            }));

            _ = Task.Factory.StartNew(
                async () =>
                {
                    // 暂停基本循环
                    //DataModel.PauseLoop();
                    try
                    {
#if AUTO_TEST
                        InitialAutoTest(token);
#endif

                        while (token.IsCancellationRequested == false)
                        {
                            if (_pause)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1), token);
                                continue;
                            }

                            /*
#if AUTO_TEST
                            {
                                var test_result = PrepareSimuRfidTag();
                                if (test_result.Value == -1)
                                    throw new Exception(test_result.ErrorInfo);
                            }
#endif
                            */

                            // 语音提示倒计时开始盘点
                            await SpeakCounter(token);

                            if (_pause)
                                continue;

                            string readerNameList = "*";
                            var result = RfidManager.CallListTags(readerNameList, "");
                            // var result = DataModel.ListTags(readerNameList, "");
                            if (result.Results == null)
                                result.Results = new List<OneTag>();
                            if (result.Value == -1)
                            {
                                await ShowMessageBox("inventory", $"列出标签失败: {result.ErrorInfo}", token);
                                break;
                            }
                            else
                            {
                                await ShowMessageBox("inventory", null, token);
                            }

                            if (_pause)
                                continue;

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
                            int filtered_count = 0;
                            int switch_count = 0;
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
                                        filtered_count = process_result.FilteredCount;
                                        switch_count = process_result.SwitchCount;
                                        process_count += process_result.ProcessCount;
                                        if (process_result.Value == 0 || process_result.Value == -1)
                                        {
                                            if (process_result.ErrorCode == "interrupt")
                                            {
                                                cancel?.Cancel();
                                                PauseLoop();    // 暂停循环
                                                SetPauseText();
                                                await ShowMessageBox("interrupt", process_result.ErrorInfo, token);
                                            }
                                            break;
                                        }

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

                                    FormClientInfo.CancelSpeaking();
                                    EnableSkipButton(false);

                                    if (result.Results.Count == 0)
                                        await FormClientInfo.Speaking($"没有发现",
                                            true,
                                            token);
                                    else
                                    {
                                        // int complete_count = result.Results.Count - cross_count;
                                        string text = $"完成 {process_count} 项";
                                        if (switch_count == 0)
                                        {
                                            if (process_count == 0)
                                                text = $"交叉 {cross_count} 项";
                                            else
                                                text += $"  交叉 {cross_count} 项";
                                        }
                                        else
                                            text = "";

                                        // 仅在没有处理(任何一项)、没有发生过切换的时候语音提示滤除数
                                        if (filtered_count != 0
                                        && process_count == 0
                                        && switch_count == 0)
                                        {
                                            text += $" 滤除 {filtered_count} 项";
                                        }

                                        if (string.IsNullOrEmpty(text) == false)
                                            await FormClientInfo.Speaking(text,
                                                false,  // true,
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
                        ClientInfo.WriteErrorLog($"盘点循环出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        await ShowMessageBox("inventory", $"盘点循环因为异常已终止: {ex.Message}", token);
                    }
                    finally
                    {
                        FormClientInfo.Speak("停止盘点", false, false);

                        this.Invoke((Action)(() =>
                        {
                            // 把按钮状态复原到未启动状态
                            this.toolStripButton_begin.Enabled = true;
                            this.toolStripButton_stop.Enabled = false;
                            this.toolStripButton_pause.Enabled = false;
                            ContinueLoop();
                            SetPauseText();
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

#if AUTO_TEST
        // 模拟标签的轮次
        static int _simuTagIndex = 0;

        // 准备模拟 RFID 标签
        NormalResult PrepareSimuRfidTag()
        {
            // 清除全部标签
            {
                var result = RfidManager.SimuTagInfo("removeTag", null, "");
                if (result.Value == -1)
                {
                    this.ShowMessageBox("simuReader", result.ErrorInfo);
                    return result;
                }
            }

            int index = _simuTagIndex;

            List<TagInfo> tags = new List<TagInfo>();

            // 插入一个层架标
            if (index == 1)
            {
                LogicChip chip = new LogicChip();
                chip.NewElement(ElementOID.PII, "0909");
                chip.NewElement(ElementOID.TU, "30");
                chip.NewElement(ElementOID.OwnerInstitution, "testoi").WillLock = true;

                var bytes = chip.GetBytes(4 * 20,
4,
GetBytesStyle.None,
out string block_map);

                var tag = new TagInfo
                {
                    ReaderName = "RL8600",
                    AntennaID = 1,

                    BlockSize = 4,
                    MaxBlockCount = 28,
                    Bytes = bytes
                };

                tags.Add(tag);
            }

            // 对当前每个柜门，都给填充一定数量的标签
            // int index = 0;
            int offset = _simuTagIndex * 10;
            for (int i = offset; i < offset + 10/*_simuTagInfos.Count*/; i++)
            {
                LogicChip chip = new LogicChip();
                SimuTagInfo info = null;
                if (i < _simuTagInfos.Count)
                    info = _simuTagInfos[i];
                else
                    info = new SimuTagInfo
                    {
                        PII = $"B{(i + 1).ToString().PadLeft(8, '0')}",
                        AccessNo = "?",
                        OI = "testoi"
                    };
                chip.NewElement(ElementOID.PII, $"{info.PII}");
                chip.NewElement(ElementOID.ShelfLocation, info.AccessNo);
                chip.NewElement(ElementOID.OwnerInstitution, info.OI).WillLock = true;

                var bytes = chip.GetBytes(4 * 20,
4,
GetBytesStyle.None,
out string block_map);

                var tag = new TagInfo
                {
                    ReaderName = info.ReaderName,
                    AntennaID = info.AntennaID,
                    BlockSize = 4,
                    MaxBlockCount = 28,
                    Bytes = bytes
                };

                tags.Add(tag);
            }

            {
                var result = RfidManager.SimuTagInfo("setTag", tags, "");
                if (result.Value == -1)
                {
                    this.ShowMessageBox("simuReader", result.ErrorInfo);
                    return result;
                }
            }

            _simuTagIndex++;
            return new NormalResult();
        }

        // 清除全部标签
        NormalResult FinishAutoTest()
        {
            return RfidManager.SimuTagInfo("removeTag", null, "");
        }
#endif

        void EnableSkipButton(bool enable, CancellationTokenSource cancel = null)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripButton_nextScan.Tag = cancel;
                this.toolStripButton_nextScan.Enabled = enable;

                // 2021/4/21
                this.toolStripButton_stop.Tag = cancel;
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

        public async Task ShowMessageBox(string type,
            string text,
            CancellationToken token)
        {
            // 语音提示出错
            Task task = null;
            if (text != null)
            {
                task = Task.Run(async () =>
                {
                    await FormClientInfo.Speaking(text, false, token);
                });
            }

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

            if (task != null)
                await task;
        }

        #endregion

        const int COLUMN_UID = 0;
        const int COLUMN_ERRORINFO = 1;
        public const int COLUMN_PII = 2;
        const int COLUMN_TITLE = 3;
        public const int COLUMN_CURRENTLOCATION = 4;
        const int COLUMN_LOCATION = 5;
        const int COLUMN_STATE = 6;
        const int COLUMN_TU = 7;
        const int COLUMN_OI = 8;
        const int COLUMN_AOI = 9;
        const int COLUMN_EAS = 10;
        const int COLUMN_AFI = 11;
        const int COLUMN_READERNAME = 12;
        const int COLUMN_ANTENNA = 13;
        const int COLUMN_PROTOCOL = 14;

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
                    // 可能会标记 "cross"
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
            // 处理时用过的层架位置。用于后来 cross 比较
            public string ProcessedShelfNo { get; set; }

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

            if (IsProcessed(tag.UID, CurrentShelfNo))
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
            // 发生层架标切换的次数
            public int SwitchCount { get; set; }

            // 校验出错的个数。包含在 ErrorCount 内
            public int VerifyErrorCount { get; set; }
        }

        // 之前处理过的事项的集合。用于计算本次和以前处理过的交叉部分
        // TODO: 这里面存储所有的已经处理过的，还是仅仅只保留前一轮的？
        // UID --> ItemInfo
        Hashtable _processedTable = new Hashtable();

        // 先前是否已经处理过？
        // parameters:
        //      current_shelfno 当前层架标位置。用于和先前处理时的层架标位置进行比较，如果不同，则不算做交叉(意思是需要重新处理)
        bool IsProcessed(string uid,
            string current_shelfno)
        {
            lock (_processedTable.SyncRoot)
            {
                var exist = _processedTable.ContainsKey(uid);
                if (exist == false)
                    return false;
                ItemInfo iteminfo = (ItemInfo)_processedTable[uid];
                if (iteminfo == null)
                    return false;
                if (iteminfo.ProcessedShelfNo == current_shelfno)
                    return true;
                return false;
            }
        }

        void ClearProcessedTable()
        {
            lock (_processedTable.SyncRoot)
            {
                _processedTable.Clear();
            }
        }

        void AddToProcessed(string uid,
            ItemInfo iteminfo)
        {
            lock (_processedTable.SyncRoot)
            {
                _processedTable[uid] = iteminfo;
            }
        }

        // 处理每一个标签的盘点动作
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
                bool noCurrentShelfNoSpeaked = false;

                int process_count = 0;
                int error_count = 0;
                int filtered_count = 0;
                int verify_error_count = 0;
                int switch_count = 0;   // 切换层架标次数
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
                                SetItemIcon(item, GetImageIndex(iteminfo));
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
                    if (iteminfo.TagInfo == null
                        && (iteminfo.UII == null || StringUtil.IsInList("verifyEAS", ActionMode))
                        )
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

                        tag.TagInfo = get_result.TagInfo;

                        // 第二步，刷新 PII 等栏目
                        this.Invoke((Action)(() =>
                        {
                            SetItemIcon(item, GetImageIndex(iteminfo));

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

                    // 第三步，进行处理
                    // bool create_from_uii = false;   // 是否从 UII 直接创建
                    Entity entity = null;
                    string tou = "";
                    if (tag.TagInfo != null)
                    {
                        // 根据 TagInfo 创建
                        entity = InventoryData.NewEntity(tag,
        null,
        out tou,
        true);
                    }
                    else
                    {
                        // 根据 UII 创建
                        entity = InventoryData.NewEntity(tag,
                            iteminfo.UII);
                        tou = "10"; // 图书
                        // create_from_uii = true;
                    }

                    // 2021/5/10
                    if (entity.Error != null)
                    {
                        SetErrorInfo(item, entity.Error);
                        error_count++;
                        continue;
                    }

                    // 语音播报标签(天线)类型切换
                    await SpeakingTagTypeChangeAsync(entity, token);

                    // 判断层架标
                    // 10 图书; 80 读者证; 30 层架标
                    if (tou != null && tou.StartsWith("3"))
                    {
                        // 按照天线状态滤除
                        if (FilterLocationTag(entity) == false)
                        {
                            this.Invoke((Action)(() =>
                            {
                                iteminfo.State = "disable";
                                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "滤除");
                                SetItemColor(item, "disable");
                            }));
                            filtered_count++;
                            continue;
                        }

                        // 校验号码合法性
                        var validate_result = InventoryData.ValidateBarcode("shelf", entity.PII);
                        if (validate_result.OK == false)
                        {
                            SetErrorInfo(item, validate_result.ErrorInfo);
                            error_count++;
                            continue;
                        }

                        {
                            iteminfo.State = "succeed";
                            this.Invoke((Action)(() =>
                            {
                                RefreshItem(item, iteminfo.UII);
                                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "切换层架标成功");
                                SetItemColor(item, "changed");
                            }));
                        }

                        var changed = await SwitchCurrentShelfNoAsync(entity, token);
                        if (changed)
                            switch_count++;
                    }
                    else if (tou != null && tou.StartsWith("8"))
                    {
                        this.Invoke((Action)(() =>
                        {
                            iteminfo.State = "disable";
                            ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "读者证被滤除");
                            SetItemColor(item, "disable");
                        }));
                        filtered_count++;
                        continue;
                    }
                    else
                    {
                        // 按照天线状态滤除
                        if (FilterBookTag(entity) == false)
                        {
                            this.Invoke((Action)(() =>
                            {
                                iteminfo.State = "disable";
                                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "滤除");
                                SetItemColor(item, "disable");
                            }));
                            filtered_count++;
                            continue;
                        }

                        // 校验图书 PII 号码合法性
                        var validate_result = InventoryData.ValidateBarcode("entity", entity.PII);
                        if (validate_result.OK == false)
                        {
                            SetErrorInfo(item, validate_result.ErrorInfo);
                            error_count++;
                            continue;
                        }

                        // 记忆处理时的层架位置
                        iteminfo.ProcessedShelfNo = CurrentShelfNo;

                        ProcessInfo process_info = new ProcessInfo { Entity = entity, FoundUii = iteminfo.UII != null };
                        var set_result = SetTargetCurrentLocation(process_info);
                        if (set_result.Value == -1
                            && set_result.ErrorCode == "noCurrentShelfNo")
                        {
                            SetErrorInfo(item, "请先扫层架标");
                            if (noCurrentShelfNoSpeaked == false)
                            {
                                await FormClientInfo.Speaking("请先扫层架标，再扫图书", false, token);
                                noCurrentShelfNoSpeaked = true;
                            }

                            /*
                            // 作为致命错误返回
                            return new ProcessResult
                            {
                                Value = -1,
                                ErrorInfo = "请先扫层架标。处理中断",
                                ErrorCode = "noCurrentShelfNo"
                            };
                            */
                            continue;
                        }
                        else if (set_result.Value == -1)
                        {
                            SetErrorInfo(item, set_result.ErrorInfo);
                            // 作为严重错误返回
                            return new ProcessResult
                            {
                                Value = -1,
                                ErrorInfo = set_result.ErrorInfo,
                                ErrorCode = "interrupt"
                            };
                        }

                        // string old_title = process_info.Entity?.Title;
                        process_info.ListViewItem = item;

                        await ProcessAsync(process_info,
                            (i, a) =>
                            {
                                // 加入操作历史
                                WriteComplete?.Invoke(this, new WriteCompleteventArgs { Info = i, Action = a });
                            });
                        if (process_info.State == "interrupt")
                        {
                            SetErrorInfo(item, entity.Error);
                            // 作为严重错误返回
                            return new ProcessResult
                            {
                                Value = -1,
                                ErrorInfo = entity.Error,
                                ErrorCode = "interrupt"
                            };
                        }

                        /*
                        // 更新题名显示
                        if (old_title != process_info.Entity?.Title)
                            this.Invoke((Action)(() =>
                            {
                                ListViewUtil.ChangeItemText(item, COLUMN_TITLE, process_info.Entity?.Title);
                            }));
                        */

                        if (process_info.IsAllTaskCompleted())
                        {
                            iteminfo.State = "succeed";
                            this.Invoke((Action)(() =>
                            {
                                RefreshItem(item, iteminfo.UII);
                                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "盘点成功");
                                SetItemColor(item, "changed");
                            }));

                            AddToProcessed(iteminfo.Tag.UID, iteminfo);
                            process_count++;

                            // 加入 ShelfDialog
                            this.AddBook?.Invoke(this, new AddBookEventArgs
                            {
                                BookInfo = new BookInfo
                                {
                                    UII = entity.GetOiPii(),
                                    Title = entity.Title,
                                    State = entity.State,
                                    Location = entity.Location + ":" + entity.ShelfNo,
                                    CurrentLocation = entity.CurrentLocation,
                                    AccessNo = entity.AccessNo,
                                },
                            });
                        }
                        else
                        {

                            SetErrorInfo(item, entity.Error);
                            if (NeedRetry(entity.ErrorItems))
                                error_count++;
                        }
                    }
#if REMOVED
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
#endif
                }

                // 返回 0 表示全部完成，没有遇到出错的情况
                return new ProcessResult
                {
                    Value = error_count,
                    ErrorCount = error_count,
                    ProcessCount = process_count,
                    VerifyErrorCount = verify_error_count,
                    FilteredCount = filtered_count,
                    SwitchCount = switch_count,
                };
            }
            finally
            {
                // DataModel.DecApiCount();
            }
        }

        // 过滤层架标(按照天线状态)
        // return:
        //      false   应被滤除
        //      true    应被保留
        static bool FilterLocationTag(Entity entity)
        {
            // 2021/4/22
            // 检查 RPAN 天线号，也就是标签类型
            if (DataModel.RfidRpanTypeSwitch == true
                && entity.ReaderName.StartsWith("R-PAN")
                && entity.Antenna != "2")
            {
                // TODO: 发出响声，表示有标签被忽略
                return false;
            }

            return true;
        }

        // 过滤图书(按照天线状态)
        // return:
        //      false   应被滤除
        //      true    应被保留
        static bool FilterBookTag(Entity entity)
        {
            // 2021/4/22
            // 检查 RPAN 天线号，也就是标签类型
            if (DataModel.RfidRpanTypeSwitch == true
                && entity.ReaderName.StartsWith("R-PAN")
                && entity.Antenna != "1")
            {
                // TODO: 发出响声，表示有标签被忽略
                return false;
            }

            return true;
        }

        static string _prevAntenna = "";

        // 当图书标签/层架标类型切换时发出语音提示
        async Task SpeakingTagTypeChangeAsync(
            Entity entity,
            CancellationToken token)
        {
            if (DataModel.RfidRpanTypeSwitch == false
                || entity.ReaderName.StartsWith("R-PAN") == false)
                return;

            if (entity.Antenna != _prevAntenna)
            {
                _prevAntenna = entity.Antenna;

                string mode = (entity.Antenna == "1" ? "B 模式" : "L 模式");
                this.Invoke((Action)(() =>
                {
                    toolStripStatusLabel_rpanMode.Text = mode;
                }));

                await FormClientInfo.Speaking($"{mode}", false, token);
            }
        }


        // 根据当前的错误事项判断，是否需要重试操作
        // 一般来说请求 dp2library 服务器的失败，不需要重试；
        // 读写标签过程中的出错需要重试(重试前操作者会调整天线位置)
        static bool NeedRetry(List<ErrorItem> error_items)
        {
            return true;
        }

        /*
        static bool IsAllComplete(ProcessInfo info)
        {
            if (info.IsTaskCompleted("getItemXml") == false)
                return false;
            if (info.IsTaskCompleted("setUID") == false)
                return false;
            if (info.IsTaskCompleted("setLocation") == false)
                return false;
            return true;
        }
        */

        static async Task ProcessAsync(ProcessInfo info,
            delegate_writeHistory writeHistory)
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
                        // bool isLocal = StringUtil.IsInList("inventory", DataModel.SipLocalStore);
                        bool isLocal = DataModel.sipLocalStore;

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
                    if (result.Value == -1
                        || result.Value == 0
                        || result.ErrorCode == "NotFound")
                    {
                        result.Value = -1;  // 注意 TaskCompleted 是按照 .Value != -1 来计算的，所以 == 0 要变为 -1 才行
                        info.SetTaskInfo("getItemXml", result);

                        // 2021/1/19
                        FormClientInfo.Speak($"{entity.PII} 无法获得册信息", false, false);

                        entity.BuildError("getItemXml", result.ErrorInfo, result.ErrorCode);
                        if (result.ErrorCode == "getChannelError")
                        {
                            // 严重错误，后面不要再重试
                            // throw new InterruptException(result.ErrorInfo);
                            info.State = "interrupt";
                            return;
                        }
                    }
                    else
                    {
                        info.SetTaskInfo("getItemXml", result);

                        entity.BuildError("getItemXml", null, null);

                        if (string.IsNullOrEmpty(result.Title) == false)
                        {
                            entity.Title = GetCaption(result.Title);
                        }

                        // 重新把 ItemXml 中的信息更新到 Entity
                        if (string.IsNullOrEmpty(result.ItemXml) == false)
                        {
                            if (info != null)
                                info.ItemXml = result.ItemXml;
                            entity.SetData(result.ItemRecPath, result.ItemXml);
                        }

                        // 立即刷新 ListViewItem 显示
                        info.ListViewItem.ListView.Invoke((Action)(() =>
                        {
                            ListViewUtil.ChangeItemText(info.ListViewItem, COLUMN_TITLE, entity.Title);
                            ListViewUtil.ChangeItemText(info.ListViewItem, COLUMN_STATE, entity.State);

                            /*
                            ListViewUtil.ChangeItemText(info.ListViewItem, COLUMN_CURRENTLOCATION, entity.CurrentLocation);
                            ListViewUtil.ChangeItemText(info.ListViewItem, COLUMN_LOCATION, entity.Location + ":" + entity.ShelfNo);
                            */
                        }));

                        // 立即刷新当前架位和永久架位的显示
                        InventoryDialog.RefreshLocations(info);


                    }
                }

                // 请求 dp2library Inventory()
                if (string.IsNullOrEmpty(entity.PII) == false
                    && info != null && info.IsLocation == false)
                {
                    await InventoryData.BeginInventoryAsync(
                        info,
                        ActionMode,
                        writeHistory);
                }

                // App.SetError("processing", null);
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"ProcessingAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                // App.SetError("processing", $"ProcessingAsync() 出现异常: {ex.Message}");
            }
            finally
            {
                if (info.State != "interrupt")
                    info.State = "";
            }
        }

        public static void RefreshLocations(ProcessInfo info)
        {
            var entity = info.Entity;

            // 立即刷新 ListViewItem 显示
            info.ListViewItem.ListView.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(info.ListViewItem, COLUMN_CURRENTLOCATION, entity.CurrentLocation);
                ListViewUtil.ChangeItemText(info.ListViewItem, COLUMN_LOCATION, entity.Location + ":" + entity.ShelfNo);
            }));
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

#if REMOVED
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

#endif

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

#if REMOVED
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
#endif

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

                    Debug.Assert(iteminfo != null);
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
                ClientInfo.WriteErrorLog($"RefreshItemByUII() exception: {ExceptionUtil.GetDebugText(ex)}");
                SetErrorInfo(item, "exception1:" + ex.Message);
            }
        }


        // 刷新一个 ListViewItem 的所有列显示
        // parameters:
        //      display_uii 用于显示的 PII。如果 TagInfo == null 的时候会采用它来显示
        void RefreshItem(ListViewItem item,
            string display_uii = null)
        {
            ParseOiPii(display_uii,
                out string display_pii, out string display_oi);
            string pii = "(尚未填充)";
            if (string.IsNullOrEmpty(display_pii) == false)
                pii = display_pii;

            string tou = "";
            string eas = "";
            string afi = "";
            string oi = "";
            string aoi = "";

            var iteminfo = item.Tag as ItemInfo;

            try
            {
                Debug.Assert(iteminfo != null);
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

                if (string.IsNullOrEmpty(oi)
                    && string.IsNullOrEmpty(aoi)
                    && string.IsNullOrEmpty(display_oi) == false)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_OI, display_oi);
                    ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);
                }
                else
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_OI, oi);
                    ListViewUtil.ChangeItemText(item, COLUMN_AOI, aoi);
                }

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

        static int GetImageIndex(ItemInfo info)
        {
            if (info.TagInfo != null)
                return 2;

            if (string.IsNullOrEmpty(info.UII) == false)
                return 3;

            return 1;
        }

        static void SetItemIcon(ListViewItem item, int imageIndex)
        {
            item.ImageIndex = imageIndex;
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
            if (DataModel.EnableTagCache)
            {
                var taginfo = GetCachedTagInfo(info.UID);
                if (taginfo != null)
                    return new GetTagInfoResult { TagInfo = taginfo };
            }

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
        static Hashtable _tagTable = new Hashtable();

        public static TagInfo GetCachedTagInfo(string uid)
        {
            lock (_tagTable.SyncRoot)
            {
                return (TagInfo)_tagTable[uid];
            }
        }

        public static void SetCacheTagInfo(string uid, TagInfo taginfo)
        {
            lock (_tagTable.SyncRoot)
            {
                // 防止缓存规模失控
                if (_tagTable.Count > 1000)
                    _tagTable.Clear();
                _tagTable[uid] = taginfo;
            }
        }

        public static void ClearCacheTagTable(string uid)
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

        // 动作模式
        /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
         * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
         * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
         * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
         * forceLog             transfer 请求时，即便没有对册记录发生实质性修改，也会被 dp2library 记入操作日志
         * */
        static string _actionMode = "setUID";    // 空/setUID/setCurrentLocation/setLocation/verifyEAS 中之一或者组合

        public static string ActionMode
        {
            get
            {
                return _actionMode;
            }
        }

        // 当前层架标
        public static string CurrentShelfNo { get; set; }

        // 状态行显示
        void SetCurrentShelfNoLabel(string text)
        {
            this.Invoke(new Action(() =>
            {
                toolStripStatusLabel_currentShelfNo.Text = "当前层架 " + text;
            }));
        }

        // 当前馆藏地。例如 “海淀分馆/阅览室”
        public static string CurrentLocation { get; set; }

        // 当前批次号
        public static string CurrentBatchNo { get; set; }

        // 切换当前层架标
        async Task<bool> SwitchCurrentShelfNoAsync(Entity entity,
            CancellationToken token)
        {
            if (string.IsNullOrEmpty(entity.PII) == false)
            {
                CurrentShelfNo = entity.PII;
                // 更新状态行显示
                SetCurrentShelfNoLabel(CurrentShelfNo);
                await FormClientInfo.Speaking($"切换层架标 {entity.PII}", false, token);
                return true;
            }

            return false;
        }

        // 给册事项里面添加 目标当前架位 参数。
        // 注意，如果 actionMode 里面不包含 setCurrentLocation，则没有必要做这一步
        NormalResult SetTargetCurrentLocation(ProcessInfo info)
        {
            // 2021/4/26
            // 检查 CurrentLocation 是否为空
            if (StringUtil.IsInList("setCurrentLocation,setLocation", ActionMode))
            {
                if (string.IsNullOrEmpty(CurrentLocation))
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "当前馆藏地点尚未设置。无法切换",
                        ErrorCode = "noCurrentLocation"
                    };
                }
            }

            info.TargetLocation = CurrentLocation;
            info.TargetShelfNo = CurrentShelfNo;
            info.BatchNo = CurrentBatchNo;

            if (string.IsNullOrEmpty(CurrentShelfNo) == true)
            {
                if (StringUtil.IsInList("setCurrentLocation,setLocation", ActionMode) == false)
                    return new NormalResult();

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "当前尚未设置层架位置，无法切换。请先扫层架标",
                    ErrorCode = "noCurrentShelfNo"
                };
            }
            info.TargetCurrentLocation = CurrentLocation + ":" + CurrentShelfNo;
            return new NormalResult();
        }

        private void toolStripButton_test_nextTags_Click(object sender, EventArgs e)
        {
#if AUTO_TEST
            PrepareSimuRfidTag();
#endif
        }

        private void toolStripButton_cancelSpeaking_Click(object sender, EventArgs e)
        {
            FormClientInfo.CancelSpeaking();
        }

        private void toolStripButton_pause_Click(object sender, EventArgs e)
        {
            if (_pause == false)
            {
                PauseLoop();
            }
            else
            {
                ContinueLoop();
            }

            SetPauseText();
        }

        // 清除标签缓存信息
        private void toolStripButton_clearTagCache_Click(object sender, EventArgs e)
        {
            // 按住 Ctrl 键清除，则清除缓存的全部标签信息。否则只清除本窗口涉及的标签缓存信息
            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            List<string> uids = new List<string>();
            foreach (ListViewItem item in this.listView_tags.Items)
            {
                var uid = ListViewUtil.GetItemText(item, COLUMN_UID);
                uids.Add(uid);
            }

            if (control)
                ClearCacheTagTable(null);
            else
            {
                foreach (var uid in uids)
                {
                    ClearCacheTagTable(uid);
                }
            }
        }
    }

#if REMOVED
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

#endif
}
