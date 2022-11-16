using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.Script;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Interfaces;
using DigitalPlatform.RFID;
using DigitalPlatform.Core;

using dp2Circulation.Charging;

namespace dp2Circulation
{
    /// <summary>
    /// 快捷出纳窗
    /// </summary>
    public partial class QuickChargingForm : MyForm, IProtectFocus, IChargingForm
    {
        /// <summary>
        /// 借书、还书完成的事件
        /// </summary>
        public event BorrowCompleteEventHandler BorrowComplete = null;

        /// <summary>
        /// IProtectFocus 接口要求的函数
        /// </summary>
        /// <param name="pfAllow">是否允许</param>
        public void AllowFocusChange(ref bool pfAllow)
        {
            pfAllow = false;
        }

        Commander commander = null;

        const int WM_LOAD_READER = API.WM_USER + 300;
        const int WM_LOAD_ITEM = API.WM_USER + 301;

        WebExternalHost m_webExternalHost_readerInfo = new WebExternalHost();

        // 借书、还书等主要业务的任务队列
        TaskList _taskList = new TaskList();

        // 刷新书目摘要的任务队列
        SummaryList _summaryList = new SummaryList();

        internal ExternalChannel _summaryChannel = new ExternalChannel();
        internal ExternalChannel _barcodeChannel = new ExternalChannel();

        // FloatingMessageForm _floatingMessage = null;

        PatronCardStyle _cardStyle = new PatronCardStyle();

        /// <summary>
        /// 构造函数
        /// </summary>
        public QuickChargingForm()
        {
            this.UseLooping = true; // 2022/11/1

            InitializeComponent();

            this.dpTable_tasks.ImageList = this.imageList_progress;

#if NO
            // 黑色调
            this.dpTable_tasks.BackColor = Color.Black;
            this.dpTable_tasks.ForeColor = Color.LightGray;
            this.dpTable_tasks.Font = new Font(this.dpTable_tasks.Font, FontStyle.Bold);

            // 深蓝
            this.dpTable_tasks.BackColor = Color.DarkBlue;
            this.dpTable_tasks.ForeColor = Color.LightGray;
            this.dpTable_tasks.Font = new Font(this.dpTable_tasks.Font, FontStyle.Bold);
#endif

            _cardStyle.PhtoMaxWidth = 50;

            this.webBrowser_reader.ScriptErrorsSuppressed = true;
        }

        PatronCardControl _cardControl = null;

        private void QuickChargingForm_Load(object sender, EventArgs e)
        {
            // this.Channel.Idle += new IdleEventHandler(Channel_Idle);
            // 被专门的线程使用，因而不需要出让控制权
#if SUPPORT_OLD_STOP
            this.ChannelDoEvents = false;
#endif
            if (this.DisplayFormat == "卡片")
            {
                this.splitContainer_main.Panel1.Controls.Remove(this.webBrowser_reader);
                this.AddFreeControl(this.webBrowser_reader);    // 2015/11/7

                _cardControl = new PatronCardControl();
                _cardControl.Dock = DockStyle.Fill;
                this.splitContainer_main.Panel1.Controls.Add(_cardControl);
            }

            // webbrowser
            this.m_webExternalHost_readerInfo.Initial(// Program.MainForm,
                this.webBrowser_reader);
            this.m_webExternalHost_readerInfo.OutputDebugInfo += new OutputDebugInfoEventHandler(m_webExternalHost_readerInfo_OutputDebugInfo);
            // this.m_webExternalHost_readerInfo.WebBrowser = this.webBrowser_reader;  //
            this.webBrowser_reader.ObjectForScripting = this.m_webExternalHost_readerInfo;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            this._summaryChannel.Initial(/*Program.MainForm*/);
            this._barcodeChannel.Initial(/*Program.MainForm*/);

            this.FuncState = this.FuncState;

#if OLD_CHARGING_CHANNEL
            this._taskList.Channel = this.Channel;
            this._taskList.stop = this.stop;
#endif
            this._taskList.Container = this;
            this._taskList.BeginThread();

            // this._summaryList.Channel = this._summaryChannel;
            // this._summaryList.stop = this.stop;
            this._summaryList.Container = this;
            this._summaryList.BeginThread();
            this._floatingMessage.RectColor = Color.Purple;

            this.toolStripButton_enableHanzi.Checked = Program.MainForm.AppInfo.GetBoolean(
                "quickchargingform",
                "enable_hanzi",
                false);
            this.toolStripButton_upperInput.Checked = Program.MainForm.UpperInputBarcode;

            {   // 恢复列宽度
                string strWidths = Program.MainForm.AppInfo.GetString(
                               "quickchargingform",
                                "tasklist_column_width",
                               "");
                if (String.IsNullOrEmpty(strWidths) == false)
                {
                    DpTable.SetColumnHeaderWidth(this.dpTable_tasks,
                        strWidths,
                        false);
                }
            }

            this.SetControlsColor(this.DisplayStyle);
            if (this.DisplayFormat == "HTML")
            {
                SetReaderHtmlString("(空)");
            }

            _errorTable = new ErrorTable((s) =>
            {
                this.Invoke((Action)(() =>
                {
                    if (this.label_rfidMessage.Text != s)
                    {
                        if (this.label_rfidMessage.Visible == false)
                            this.label_rfidMessage.Visible = true;

                        if (string.IsNullOrEmpty(s))
                        {
                            this.label_rfidMessage.Text = _rfidNumber;

                            this.label_rfidMessage.BackColor = Color.White;
                            this.label_rfidMessage.ForeColor = Color.Black;
                        }
                        else
                        {
                            this.label_rfidMessage.Text = s;

                            this.label_rfidMessage.BackColor = Color.DarkRed;
                            this.label_rfidMessage.ForeColor = Color.White;
                        }
                    }
                }));
            });

            {
                // 清除以前残留的未读出的消息
                FingerprintManager.ClearMessage();

                FingerprintManager.SetError += PalmprintManager_SetError;
#if NEWFINGER
                // 2022/6/7
                // FingerprintManager.EnableSendkey(false);
#endif
            }

            RfidManager.SetError += RfidManager_SetError;
            Program.MainForm.TagChanged += MainForm_TagChanged;
            InitialSendKey();
            RfidManager.ClearCache();
            if (string.IsNullOrEmpty(RfidManager.Url) == false)
            {
                this.label_rfidMessage.Visible = true;
                /*
                var result = RfidManager.GetState("");
                if (result.Value == -1)
                    this.ShowMessage($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})", "red", true);
                    */
                var result = RfidManager.EnableSendkey(false);
            }
            else
            {
                this.label_rfidMessage.Visible = false;
            }
            InitialEasForm();
            ShowEasForm(false);

            // Task.Run(() => { InitialRfidChannel(); });

            // 2019/9/4 增加
            // 首次设置输入焦点
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                this.Invoke((Action)(() =>
                {
                    this.textBox_input.Focus();
                }));
            });
        }

        public void FocusInput()
        {
            this.Invoke((Action)(() =>
            {
                this.textBox_input.Focus();
            }));
        }

        // 新 Tag 到来
        private void MainForm_TagChanged(object sender, TagChangedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                {
                    if (e.AddPatrons != null)
                        foreach (var tag in e.AddPatrons)
                        {
                            SendKey(tag, now);
                        }
                    if (e.UpdatePatrons != null)
                        foreach (var tag in e.UpdatePatrons)
                        {
                            SendKey(tag, now);
                        }
                    if (e.RemovePatrons != null)
                        foreach (var tag in e.RemovePatrons)
                        {
                            if (tag.OneTag != null)
                                SetLastTime(tag.OneTag.UID, now);
                        }
                }

                {
                    if (e.AddBooks != null)
                        foreach (var tag in e.AddBooks)
                        {
                            SendKey(tag, now);
                        }
                    if (e.RemoveBooks != null)
                        foreach (var tag in e.RemoveBooks)
                        {
                            if (tag.OneTag != null)
                                SetLastTime(tag.OneTag.UID, now);
                        }
                    if (e.UpdateBooks != null)
                        foreach (var tag in e.UpdateBooks)
                        {
                            SendKey(tag, now);
                        }
                }

                RefreshRfidTagNumber();
                CheckMultiPatronCard();
            }
            catch (Exception ex)
            {
                WriteErrorLog($"MainForm_TagChanged exception: {ExceptionUtil.GetDebugText(ex)}");
                throw new Exception(ex.Message, ex);
            }
        }

        // 检查当前是否有多张读者卡持续放在读卡器上
        void CheckMultiPatronCard()
        {
            var count = RfidTagList.Patrons.Count;
            if (count > 1)
                SetError("multi", $"请拿走多余的读者卡(当前一共放了 {count} 张)");
            else
                SetError("multi", null);
        }

        string _rfidNumber = "";

        void RefreshRfidTagNumber()
        {
            _rfidNumber = $"{RfidTagList.Books.Count}:{RfidTagList.Patrons.Count}";
            this.Invoke((Action)(() =>
            {
                if (this.label_rfidMessage.Visible == false)
                    this.label_rfidMessage.Visible = true;

                if (this.label_rfidMessage.BackColor == Color.White)
                {
                    // 标签总数显示 图书+读者卡
                    this.label_rfidMessage.Text = _rfidNumber;
                }
            }));
        }

        // 把存量的 PII 发送出去
        void InitialSendKey()
        {
            DateTime now = DateTime.Now;

            var books = RfidTagList.Books;
            if (books.Count > 0)
            {
                foreach (var tag in books)
                {
                    SendKey(tag, now);
                }
            }

            var patrons = RfidTagList.Patrons;
            if (patrons.Count > 0)
            {
                foreach (var tag in patrons)
                {
                    SendKey(tag, now);
                }
            }

            RefreshRfidTagNumber();
            CheckMultiPatronCard();
        }


        public static string GetPII(TagInfo tagInfo)
        {
            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            LogicChip chip = LogicChip.From(tagInfo.Bytes,
(int)tagInfo.BlockSize,
"" // tagInfo.LockStatus
);
            return chip.FindElement(ElementOID.PII)?.Text;
        }

        public static string GetTOU(TagInfo tagInfo)
        {
            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            LogicChip chip = LogicChip.From(tagInfo.Bytes,
(int)tagInfo.BlockSize,
"" // tagInfo.LockStatus
);
            return chip.FindElement(ElementOID.TypeOfUsage)?.Text;
        }

        // UID --> 最近出现时间 的对照表
        // 用于平滑标签拿放的事件。原理是，如果一个标签最后离开和后来一次到来之间的时间差太小，则放弃这一次到来事件
        Hashtable _uidTable = new Hashtable();
        // private readonly Object _syncRoot_uidTable = new object();

        static TimeSpan _minDelay = TimeSpan.FromMilliseconds(500);

        DateTime GetLastTime(string uid)
        {
            lock (_uidTable.SyncRoot)
            {
                if (_uidTable.ContainsKey(uid) == false)
                    return DateTime.MinValue;
                DateTime time = (DateTime)_uidTable[uid];
                return time;
            }
        }

        void SetLastTime(string uid, DateTime now)
        {
            if (string.IsNullOrEmpty(uid))
                return;

            lock (_uidTable.SyncRoot)
            {
                if (_uidTable.Count > 1000)
                    _uidTable.Clear();  // TODO: 可以优化为每隔一段时间自动清除太旧的事项
                _uidTable[uid] = now;
            }
        }

        public bool PauseRfid = true;

        void SendKey(TagAndData data, DateTime now)
        {
            if (this.PauseRfid)
                return;

            SetError("sendKey", null);

            if (data.OneTag.Protocol == InventoryInfo.ISO14443A)
            {
                // 检查时间差额
                {
                    DateTime last_time = GetLastTime(data.OneTag.UID);
                    if (now - last_time < _minDelay)
                    {
                        Debug.WriteLine("smooth ISO14443A");
                        return;
                    }
                }

                SetLastTime(data.OneTag.UID, DateTime.Now);

                TaskList.Sound(0);

                string text = $"uid:{data.OneTag.UID},tou:80";
                this.Invoke((Action)(() =>
                {
                    this.textBox_input.Text = text;
                }));
                AsyncDoAction(this.FuncState, text);
                return;
            }

            if (data.OneTag.TagInfo == null)
            {
                //Debug.WriteLine("TagInfo == null");
                return;
            }

            string pii = GetPII(data.OneTag.TagInfo);

            if (string.IsNullOrEmpty(pii))
            {
                // TODO: 改进显示方式
                SetError("sendKey", $"此标签(UID={data.OneTag.UID})无法解析出 PII 元素。已写入错误日志");
                MainForm.WriteErrorLog($"此标签(UID={data.OneTag.UID})无法解析出 PII 元素。bytes='{Element.GetHexString(data.OneTag.TagInfo.Bytes)}'");
                return;
            }

            // 缓存起来
            if (_easForm != null)
                _easForm.SetUID(pii, data.OneTag.UID);

            Debug.WriteLine($"pii={pii}");

            // 检查时间差额
            {
                DateTime last_time = GetLastTime(data.OneTag.UID);
                if (now - last_time < _minDelay)
                {
                    Debug.WriteLine("smooth ISO15693");
                    return;
                }
            }

            SetLastTime(data.OneTag.UID, now);

            string strTypeOfUsage = GetTOU(data.OneTag.TagInfo);
            if (string.IsNullOrEmpty(strTypeOfUsage))
                strTypeOfUsage = "10";
            // 2019/6/13
            // 注意：特殊处理!
            else if (strTypeOfUsage == "32")
                strTypeOfUsage = "10";

            if (strTypeOfUsage[0] == '8')
                TaskList.Sound(0);
            else
                TaskList.Sound(1);

            if (strTypeOfUsage[0] == '1'
                // && _easForm.ErrorCount > 0
                )
            {
                // 尝试自动修正 EAS
                // result.Value
                //      -1  出错
                //      0   ListsView 中没有找到事项
                //      1   发生了修改
                var eas_result = _easForm.TryCorrectEas(data.OneTag.UID,
                    data.OneTag.AntennaID,
                    pii);
                if (eas_result.Value == -1)
                {
                    // TODO SetError()
                    // this.ShowMessage($"尝试自动修正 EAS 时出错 '{eas_result.ErrorInfo}'", "red", true);
                    TaskList.Sound(-1);
                    return;
                }

                if (eas_result.Value == 1)
                {
                    TaskList.Sound(2);

                    // 如果所有错误均被消除，则 EasForm 要隐藏
                    if (_easForm.ErrorCount == 0)
                    {
                        _easForm.ClearMessage();
                        this.Invoke((Action)(() =>
                        {
                            ShowEasForm(false);
                        }));
                    }

                    if (this.StateSpeak != "[不朗读]")
                        Program.MainForm.Speak("自动修正 EAS 成功");
                    this.ShowMessageAutoClear("自动修正 EAS 成功", "green", 2000, true);
                    // 本次标签触发了自动修正动作，并操作成功，后面就不再继续进行借书或者还书操作了
                    return;
                }

                // TODO: 如果 errorCount > 0，则搜索 tasklist，如果 PII 找到匹配则放弃继续操作
                {
                    var task = this._taskList.FindTaskByItemBarcode(pii);
                    if (task != null
                        && (task.Color != "red"))
                    {
                        // TODO: 发出尖锐声音提示操作者注意被吞掉的号码
                        TaskList.Sound(-1);

                        // 延时 ShowMessage
                        this.ShowMessageAutoClear($"任务 {pii} 被忽略(和当前任务列表(count={_taskList.Count})重复)",
                            "yellow",
                            5000,
                            true);
                        // 让 task 闪烁几次，让操作者容易看到
                        FlashTask(task, 5);
                        return;
                    }
                }
            }

            {
                string text = $"pii:{pii},tou:{strTypeOfUsage}";

                this.Invoke((Action)(() =>
                {
                    this.textBox_input.Text = text;
                }));
                AsyncDoAction(this.FuncState, text);
            }
        }

        public DigitalPlatform.Core.RecordLockCollection _tasklocks = new DigitalPlatform.Core.RecordLockCollection();

        // TODO: 针对同一个 task 对象的线程同一时间只能允许一个运行
        void FlashTask(ChargingTask task, int count)
        {
            Task.Run(() =>
            {
                try
                {
                    var hashcode = task.GetHashCode().ToString();
                    _tasklocks.LockForWrite(hashcode);
                    try
                    {
                        string save_color = task.Color;
                        for (int i = 0; i < count; i++)
                        {
                            Thread.Sleep(500);
                            task.Color = "";
                            this.DisplayTask("refresh", task);
                            Thread.Sleep(500);
                            task.Color = save_color;
                            this.DisplayTask("refresh", task);
                        }
                        SetColorList(); // 2019/9/4 最后刷新一次 colorlist
                    }
                    finally
                    {
                        _tasklocks.UnlockForWrite(hashcode);
                    }
                }
                catch
                {

                }
            });
        }

        private void PalmprintManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("palm", e.Error);
        }

        private void RfidManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("rfid", e.Error);
        }

        // result.Value:
        //      -1  出错
        //      0   Off
        //      1   On
        internal GetEasStateResult GetEAS(string reader_name,
            string tag_name)
        {
            return _easForm.GetEAS(reader_name, tag_name);
        }

        internal NormalResult SetEAS(
            ChargingTask task,
            string reader_name,
            string tag_name,
            bool enable)
        {
            var result = _easForm.SetEAS(task, reader_name, tag_name, enable);
            if (result.Value != 1)
            {
                _easForm.ShowMessage($"请把图书放回读卡器以修正 EAS\r\n拿放动作不要太快，给读卡器一点时间", "yellow", true);
                this.Invoke((Action)(() =>
                {
                    // 显示 EasForm
                    ShowEasForm(true);
                }));
            }
            return result;
        }

        /*
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
                // 马上检测一下通道是否可用
                try
                {
                    _rfidChannel.Object.ListReaders();
                }
                catch (Exception ex)
                {
                    this.ShowMessageBox("启动 RFID 设备时出错: " + ex.Message);
                }
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

        public void OpenRfidCapture(bool open)
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

            */

#if NO
        string _focusLibraryCode = "";

        // 当前操作所针对的分馆 代码
        // 注: 全局用户可以操作任何分管，和总馆，通过此成员，可以明确它当前正在操作哪个分馆，这样可以明确 VerifyBarcode() 的 strLibraryCodeList 参数值
        public string FocusLibraryCode
        {
            get
            {
                return _focusLibraryCode;
            }
            set
            {
                this._focusLibraryCode = value;
                this.Text = "快捷出纳 - " + (string.IsNullOrEmpty(value) == true ? "[总馆]" : value);
            }
        }

        void FillLibraryCodeListMenu()
        {
            string strError = "";
            List<string> all_library_codes = null;
            int nRet = this.GetAllLibraryCodes(out all_library_codes, out strError);

            List<string> library_codes = null;
            if (Global.IsGlobalUser(this.Channel.LibraryCodeList) == true)
            {
                library_codes = all_library_codes;
                library_codes.Insert(0, "");
            }
            else
                library_codes = StringUtil.SplitList(this.Channel.LibraryCodeList);

            this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Clear();
            foreach (string library_code in library_codes)
            {
                string strName = library_code;
                if (string.IsNullOrEmpty(strName) == true)
                    strName = "[总馆]";
                ToolStripItem item = new ToolStripMenuItem(strName);
                item.Tag = library_code;
                item.Click += item_Click;
                this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Add(item);
            }

            // 默认选定第一项
            if (this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Count > 0)
                item_Click(this.toolStripDropDownButton_selectLibraryCode.DropDownItems[0], new EventArgs());
        }

        void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem current in this.toolStripDropDownButton_selectLibraryCode.DropDownItems)
            {
                if (current != item && current.Checked == true)
                    current.Checked = false;
            }
            item.Checked = true;
            FocusLibraryCode = item.Tag as string;
        }

#endif

        void m_webExternalHost_readerInfo_OutputDebugInfo(object sender, OutputDebugInfoEventArgs e)
        {
            if (_floatingMessage != null)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string>(AppendFloatingMessage), "\r\n" + e.Text);
            }
        }

#if NO
        void MainForm_Move(object sender, EventArgs e)
        {
            this._floatingMessage.OnResizeOrMove();
        }
#endif

#if NO
        void Channel_Idle(object sender, IdleEventArgs e)
        {
            // 被专门的线程使用，因而不需要出让控制权
            // e.bDoEvents = false;
        }
#endif

        private void QuickChargingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            this.ShowMessage("正在关闭窗口 ...", "green", false);
            Application.DoEvents();
#endif
        }

        private void QuickChargingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MainForm.TagChanged -= MainForm_TagChanged;
            RfidManager.SetError -= RfidManager_SetError;
            FingerprintManager.SetError -= PalmprintManager_SetError;

            //OpenRfidCapture(false);
            //ReleaseRfidChannel();

#if NO
            if (Program.MainForm != null)
                Program.MainForm.Move -= new EventHandler(MainForm_Move);
#endif

            this.commander.Destroy();

            if (this.m_webExternalHost_readerInfo != null)
            {
                this.m_webExternalHost_readerInfo.OutputDebugInfo -= new OutputDebugInfoEventHandler(m_webExternalHost_readerInfo_OutputDebugInfo);
                this.m_webExternalHost_readerInfo.Destroy();
            }

            this.ShowMessage("正在停止任务线程 ...", "green", false);
            this._taskList.Close();

            this.ShowMessage("正在停止刷新摘要线程 ...", "green", false);
            this._summaryList.Close();

            this.ClearMessage();

            this._summaryChannel.Close();
            this._barcodeChannel.Close();

#if NO
            if (_floatingMessage != null)
                _floatingMessage.Close();
#endif

            if (_patronSummaryForm != null)
                _patronSummaryForm.Close();

            DestroyEasForm();

            // this.Channel.Idle -= new IdleEventHandler(Channel_Idle);

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "quickchargingform",
                    "enable_hanzi",
                    this.toolStripButton_enableHanzi.Checked);

#if NO
                Program.MainForm.AppInfo.SetBoolean(
                    "quickchargingform",
                    "upper_input",
                    this.toolStripButton_upperInput.Checked);
#endif

                {   // 保存列宽度
                    string strWidths = DpTable.GetColumnWidthListString(this.dpTable_tasks);
                    Program.MainForm.AppInfo.SetString(
                        "quickchargingform",
                        "tasklist_column_width",
                        strWidths);
                }
            }
        }

        // 任务单元的背景色
        Color TaskBackColor
        {
            get;
            set;
        }

        // 任务单元的前景色
        Color TaskForeColor
        {
            get;
            set;
        }

        void SetControlsColor(string strStyle)
        {
            if (strStyle == "dark")
            {
                this.BackColor = Color.FromArgb(40, 40, 40);    //  Color.DimGray;
                this.ForeColor = Color.White;

                this.toolStrip_main.BackColor = Color.FromArgb(70, 70, 70); // 50

                this.textBox_input.BackColor = Color.Black;
                this.textBox_input.ForeColor = Color.White;

                this.ActionTextColor = Color.LightGray;

                this.dpTable_tasks.BackColor = this.BackColor;
                this.dpTable_tasks.ColumnsBackColor = this.BackColor;
                this.dpTable_tasks.ColumnsForeColor = this.ForeColor;

                this.TaskBackColor = Color.FromArgb(255, 10, 10, 10);
                this.TaskForeColor = Color.FromArgb(255, 230, 230, 230);

                this._cardStyle.BarcodeTextColor = Color.FromArgb(255, 10, 200, 10);
                this._cardStyle.NameTextColor = this.TaskForeColor;
                this._cardStyle.DepartmentTextColor = Color.FromArgb(255, 150, 150, 150);

            }
            else if (strStyle == "light")
            {
                this.BackColor = SystemColors.Window;
                this.ForeColor = SystemColors.WindowText;

                this.toolStrip_main.BackColor = this.BackColor;

                this.textBox_input.BackColor = this.BackColor;
                this.textBox_input.ForeColor = this.ForeColor;

                this.ActionTextColor = this.ForeColor;

                this.dpTable_tasks.BackColor = Color.FromArgb(255, 230, 230, 230); // this.BackColor;  // SystemColors.ControlDarkDark;
                this.dpTable_tasks.ColumnsBackColor = SystemColors.Control;
                this.dpTable_tasks.ColumnsForeColor = this.ForeColor;

                this.TaskBackColor = this.BackColor;
                this.TaskForeColor = this.ForeColor;

                this._cardStyle.BarcodeTextColor = Color.DarkGreen;
                this._cardStyle.NameTextColor = this.TaskForeColor;
                this._cardStyle.DepartmentTextColor = SystemColors.ControlDark;
            }

            this.m_webExternalHost_readerInfo.BackColor = this.BackColor;

            this.panel_input.BackColor = this.BackColor;
            this.panel_input.ForeColor = this.ForeColor;
            this.pictureBox_action.BackColor = this.BackColor;
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost_readerInfo.ChannelInUse;
        }

        public void DoEnter()
        {
            AsyncDoAction(this.FuncState,
                GetUpperCase(this.textBox_input.Text));
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            if (keyData == Keys.Enter
                || keyData == Keys.LineFeed)
            {
                // MessageBox.Show(this, "test");
                DoEnter();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            // return false;
            return base.ProcessDialogKey(keyData);
        }

        delegate int Delegate_SelectOnePatron(long lRet,
            string strRecPath,
            out string strBarcode,
            out string strResult,
            out string strError);

        // return:
        //      -1  error
        //      0   放弃
        //      1   成功
        internal int SelectOnePatron(long lRet,
            string strRecPath,
            out string strBarcode,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";
            strBarcode = "";

            if (lRet <= 1)
            {
                strError = "没有必要调用 SelectMultiPatron()";
                return -1;
            }

            if (this.IsDisposed)
                return 0;

            if (this.InvokeRequired)
            {
                Delegate_SelectOnePatron d = new Delegate_SelectOnePatron(SelectOnePatron);
                object[] args = new object[5];
                args[0] = lRet;
                args[1] = strRecPath;
                args[2] = strBarcode;
                args[3] = strResult;
                args[4] = strError;
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strBarcode = (string)args[2];
                strResult = (string)args[3];
                strError = (string)args[4];
                return result;
            }

            /*
            strError = "读者证条码号 '" + strBarcode + "' 命中 " + lRet.ToString() + " 条读者记录。这是一个严重错误，请系统管理员尽快排除。\r\n\r\n(当前窗口中显示的是其中的第一个记录)";
            goto ERROR1;
             * */
            SelectPatronDialog dlg = new SelectPatronDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.NoBorrowHistory = this.NoBorrowHistory;
            dlg.ColorBarVisible = false;
            dlg.MessageVisible = false;
            dlg.Overflow = StringUtil.SplitList(strRecPath).Count < lRet;
            int nRet = dlg.Initial(
                // Program.MainForm,
                StringUtil.SplitList(strRecPath),
                "请选择一个读者记录",
                out strError);
            if (nRet == -1)
                return -1;
            // TODO: 保存窗口内的尺寸状态
            Program.MainForm.AppInfo.LinkFormState(dlg, "QuickChargingForm_SelectPatronDialog_state");
            dlg.ShowDialog(this.SafeWindow);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return 0;

            strBarcode = dlg.SelectedBarcode;
            strResult = dlg.SelectedHtml;

            return 1;
        }


        //
        delegate int Delegate_SelectOneItem(
            FuncState func,
            string strText,
    out string strItemBarcode,
    out string strError);

        // return:
        //      -1  error
        //      0   放弃
        //      1   成功
        internal int SelectOneItem(
            FuncState func,
            string strText,
            out string strItemBarcode,
            out string strError)
        {
            strError = "";
            strItemBarcode = "";

            if (this.IsDisposed)
                return 0;

            if (this.InvokeRequired)
            {
                Delegate_SelectOneItem d = new Delegate_SelectOneItem(SelectOneItem);
                object[] args = new object[4];
                args[0] = func;
                args[1] = strText;
                args[2] = strItemBarcode;
                args[3] = strError;
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strItemBarcode = (string)args[2];
                strError = (string)args[3];
                return result;
            }

            SelectItemDialog dlg = new SelectItemDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            if (func == dp2Circulation.FuncState.Borrow
                || func == dp2Circulation.FuncState.ContinueBorrow
                || func == FuncState.SpecialBorrow)
            {
                dlg.FunctionType = "borrow";
                dlg.Text = "请选择要借阅的册";
            }
            else if (func == dp2Circulation.FuncState.Renew
                || func == FuncState.SpecialRenew)
            {
                dlg.FunctionType = "renew";
                dlg.Text = "请选择要续借的册";
            }
            else if (func == dp2Circulation.FuncState.VerifyRenew)
            {
                dlg.FunctionType = "renew";
                dlg.VerifyBorrower = this._taskList.CurrentReaderBarcode;
                dlg.Text = "请选择要(验证)续借的册";
            }
            else if (func == dp2Circulation.FuncState.Return || func == dp2Circulation.FuncState.Lost)
            {
                dlg.FunctionType = "return";
                dlg.Text = "请选择要还回的册";
            }
            else if (func == dp2Circulation.FuncState.VerifyReturn || func == dp2Circulation.FuncState.VerifyLost)
            {
                dlg.FunctionType = "return";
                dlg.VerifyBorrower = this._taskList.CurrentReaderBarcode;
                dlg.Text = "请选择要(验证)还回的册";
            }
            else if (func == dp2Circulation.FuncState.InventoryBook)
            {
                dlg.FunctionType = "inventory";
                dlg.Text = "请选择要盘点的册";
            }
            else if (func == dp2Circulation.FuncState.Read)
            {
                dlg.FunctionType = "read";
                dlg.VerifyBorrower = this._taskList.CurrentReaderBarcode;
                dlg.Text = "请选择要读过的册";
            }
            else if (func == dp2Circulation.FuncState.Boxing)
            {
                dlg.FunctionType = "boxing";
                dlg.VerifyBorrower = this._taskList.CurrentReaderBarcode;
                dlg.Text = "请选择要配书的册";
            }
            else if (func == dp2Circulation.FuncState.Transfer)
            {
                dlg.FunctionType = "transfer";
                dlg.Text = "请选择要调拨的册";
            }

            dlg.AutoOperSingleItem = this.AutoOperSingleItem;
            dlg.AutoSearch = true;
            dlg.MainForm = Program.MainForm;
            dlg.From = "ISBN";
            dlg.QueryWord = strText;

            string strUiState = Program.MainForm.AppInfo.GetString(
        "QuickChargingForm",
        "SelectItemDialog_uiState",
        "");
            dlg.UiState = strUiState;

            if (string.IsNullOrEmpty(strUiState) == false
                || Program.MainForm.PanelFixedVisible == true)
                Program.MainForm.AppInfo.LinkFormState(dlg, "QuickChargingForm_SelectItemDialog_state");
            else
            {
                dlg.Size = Program.MainForm.panel_fixed.Size;
                dlg.StartPosition = FormStartPosition.Manual;
                dlg.Location = Program.MainForm.PointToScreen(Program.MainForm.panel_fixed.Location);
            }

            dlg.ShowDialog(this.SafeWindow);

            if (string.IsNullOrEmpty(strUiState) == false
                || Program.MainForm.PanelFixedVisible == true)
                Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"QuickChargingForm",
"SelectItemDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return 0;

            Debug.Assert(string.IsNullOrEmpty(dlg.SelectedItemBarcode) == false, "");
            strItemBarcode = dlg.SelectedItemBarcode;
            return 1;
        }

        internal void AddItemSummaryTask(string strItemBarcode,
            string strConfirmItemRecPath,
            ChargingTask charging_task,
            bool bClearBofore = true)
        {
            if (bClearBofore)
            {
                // 如果以前有摘要，要先清除。这样操作者在等待过程中能清楚当前处在什么状态
                charging_task.ItemSummary = "正在获取书目摘要 ...";
                DisplayTask("refresh", charging_task);
            }

            SummaryTask task = new SummaryTask();
            task.Action = "get_item_summary";
            task.ItemBarcode = strItemBarcode;
            task.ConfirmItemRecPath = strConfirmItemRecPath;
            task.ChargingTask = charging_task;

            this._summaryList.AddTask(task);
        }
#if NO
        delegate void Delegate_FillItemSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            ChargingTask task);
        internal void AsynFillItemSummary(string strItemBarcode, 
            string strConfirmItemRecPath,
            ChargingTask task)
        {
            if (this.IsDisposed)
                return;

            // 这里被做事的线程调用，希望启动任务后尽快返回。但不应把长时任务交给界面线程
            if (this.InvokeRequired)
            {
                Delegate_FillItemSummary d = new Delegate_FillItemSummary(AsynFillItemSummary);
                this.BeginInvoke(d, 
                    new object[] { 
                        strItemBarcode,
                        strConfirmItemRecPath,
                        task }
                    );
                return;
            }

            DpRow row = FindTaskLine(task);
            if (row == null)
                return;

            string strError = "";
            string strSummary = "";
            int nRet = GetBiblioSummary(strItemBarcode,
                strConfirmItemRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
                strSummary = strError;

            task.ItemSummary = strSummary;
            DisplayTask("refresh", task);

            if (this.SpeakBookTitle == true && nRet != -1
                && string.IsNullOrEmpty(strSummary) == false)
            {
                string strTitle = "";
                nRet = strSummary.IndexOf("/");
                if (nRet != -1)
                    strTitle = strSummary.Substring(0, nRet).Trim();
                else
                    strTitle = strSummary.Trim();

                Program.MainForm.Speak(strTitle);
            }
        }
#endif
        // 把摘要显示到任务列表中，并朗读出来
        // parameters:
        //      bSpeak  是否要把 strSummary 朗读出来？注意，当此参数为 true 时，依然要看当前系统参数配置，允许朗读才真正朗读
        internal void AsyncFillItemSummary(ChargingTask task,
            string strSummary,
            bool bSpeak)
        {
            if (this.IsDisposed)
                return;

            // 这里被做事的线程调用，希望启动任务后尽快返回。但不应把长时任务交给界面线程
            if (this.InvokeRequired)
            {
                // 原来是 BeginInvoke。可能和清除文本的动作发生顺序交错
                this.Invoke(new Action<ChargingTask, string, bool>(AsyncFillItemSummary), task, strSummary, bSpeak);
                return;
            }

            DpRow row = FindTaskLine(task);
            if (row == null)
                return;

            task.ItemSummary = strSummary;
            DisplayTask("refresh", task);

            // 把摘要的书名部分朗读出来
            if (bSpeak
                && this.SpeakBookTitle == true
                && string.IsNullOrEmpty(strSummary) == false)
            {
                string strTitle = "";
                int nRet = strSummary.IndexOf("/");
                if (nRet != -1)
                    strTitle = strSummary.Substring(0, nRet).Trim();
                else
                    strTitle = strSummary.Trim();

                Program.MainForm.Speak(strTitle);
            }
        }

        internal void WriteErrorLog(string strText)
        {
            if (this.LogOperTime)
            {
                try
                {
                    MainForm.WriteErrorLog(strText);
                }
                catch (Exception ex)
                {
                    // 这样在 dp2003.com 的异常汇报里面就能看到 strText 内容了
                    throw new Exception("在 QuickChargingForm::WriteErrorLog() 中尝试写入错误日志时出错。"
                        + "拟写入错误日志的内容为 '" + strText + "'",
                        ex);
                }
            }
        }

        public override void UpdateEnable(bool bEnable)
        {
            // this.textBox_input.Enabled = bEnable;
        }

        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="strBarcode">要校验的条码号</param>
        /// <param name="strLibraryCodeList">馆代码列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-2  服务器没有配置校验方法，无法校验</para>
        /// <para>-1  出错</para>
        /// <para>0   不是合法的条码号</para>
        /// <para>1   是合法的读者证条码号</para>
        /// <para>2   是合法的册条码号</para>
        /// </returns>
        public override int VerifyBarcode(
            string strLibraryCodeList,
            string strBarcode,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "条码号不应为空";
                return -1;
            }

            // 2014/5/4
            if (StringUtil.HasHead(strBarcode, "PQR:") == true)
            {
                strError = "这是读者证号二维码";
                return 1;
            }

            // 2022/1/14
            if (StringUtil.HasHead(strBarcode, "@refID:", true) == true)
            {
                strError = "这是册参考ID";
                return 2;
            }

            // 2019/1/9
            string prefix = ""; //  "pii:";
            string type_of_usage = "";  // "10";    // 10 流通馆藏; 80 读者证
            if (strBarcode.StartsWith("pii:") == true
                || strBarcode.StartsWith("PII:") == true
                || strBarcode.StartsWith("uid:") == true
                || strBarcode.StartsWith("UID:") == true)
            {
                // 这是册条码号(RFID 读卡器发来的)。但内容依然需要进行校验
                Hashtable table = StringUtil.ParseParameters(strBarcode, ',', ':');
                strBarcode = GetValue(table, "pii");
                if (string.IsNullOrEmpty(strBarcode))
                {
                    strBarcode = GetValue(table, "uid");
                    prefix = "uid:";
                }
                type_of_usage = GetValue(table, "tou");
                if (string.IsNullOrEmpty(type_of_usage))
                    type_of_usage = "10";
            }

            // 2015/12/9
            if (strBarcode == "_testreader")
            {
                strError = "这是一个测试用的读者证号";
                return 1;
            }

            // 2016/1/13
            if (strBarcode.ToLower().StartsWith("@bibliorecpath:") == true)
            {
                strError = "无法确定类型(为兼容“读过”功能)";
                return -2;
            }

            // 2019/3/18
            // 14443A 的读者卡，不校验 UID 字符串
            if (prefix == "uid:" && type_of_usage.StartsWith("8"))
                return 1;

            this._barcodeChannel.PrepareSearch("正在验证条码号 " + strBarcode + "...");
            try
            {
                // TODO: 使用回调函数，以决定是否 disable textbox
                int nRet = Program.MainForm.VerifyBarcode(
                    this._barcodeChannel.stop,
                    this._barcodeChannel.Channel,
                    strLibraryCodeList,
                    strBarcode,
                    EnableControls,
                    out strError);
                if (type_of_usage == "10" && nRet == 1)
                {
                    // pii: 或者 uid: 引导的内容居然符合读者证条码号规则了?
                    strError = $"{prefix}引导的号码 {strBarcode} 不符合册条码号校验规则: " + strError;
                    return -1;
                }
                if (type_of_usage == "80" && nRet == 2)
                {
                    // pii: 或者 uid: 引导的内容居然符合册条码号规则了?
                    strError = $"{prefix}引导的号码 {strBarcode} 不符合读者证条码号校验规则: " + strError;
                    return -1;
                }
                return nRet;
            }
            finally
            {
                this._barcodeChannel.EndSearch();
            }
        }

        /// <summary>
        /// 获得书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认的册记录路径。可以为空</param>
        /// <param name="strSummary">书目摘要</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError中；0: 没有找到; 1: 找到了</returns>
        public int GetBiblioSummary(string strItemBarcode,
    string strConfirmItemRecPath,
    out string strSummary,
    out string strError)
        {
            string strBiblioRecPath = "";
            int nRet = Program.MainForm.GetCachedBiblioSummary(strItemBarcode,
strConfirmItemRecPath,
out strSummary,
out strError);
            if (nRet == -1 || nRet == 1)
                return nRet;

            Debug.Assert(nRet == 0, "");

            this._summaryChannel.PrepareSearch("正在获取书目摘要 ...");
            try
            {
                this._summaryChannel.Channel.Timeout = new TimeSpan(0, 0, 5);
                long lRet = this._summaryChannel.Channel.GetBiblioSummary(
                    this._summaryChannel.stop,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                if (lRet == -1)
                {
                    return -1;
                }
                else
                {
                    Program.MainForm.SetBiblioSummaryCache(strItemBarcode,
                         strConfirmItemRecPath,
                         strSummary);
                }

                return (int)lRet;
            }
            finally
            {
                this._summaryChannel.EndSearch();
            }
        }

        public bool IsCardMode
        {
            get
            {
                if (_cardControl != null)
                    return true;
                return false;
            }
        }

        delegate void Delegate_SetReaderCardString(string strText);
        public void SetReaderCardString(string strText)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_SetReaderCardString d = new Delegate_SetReaderCardString(SetReaderCardString);
                this.BeginInvoke(d, new object[] { strText });
                return;
            }

            if (string.IsNullOrEmpty(strText) == false
                && strText[0] != '<')
                _cardControl.Text = strText;
            else
            {
                try
                {
                    _cardControl.Xml = strText;
                }
                catch (Exception ex)
                {
                    _cardControl.Text = ex.Message;
                }
            }
        }

        delegate void Delegate_SetReaderHtmlString(string strHtml);
        /// <summary>
        /// 显示读者 HTML 字符串
        /// </summary>
        /// <param name="strHtml">HTML 字符串</param>
        public void SetReaderHtmlString(string strHtml)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_SetReaderHtmlString d = new Delegate_SetReaderHtmlString(_setReaderHtmlString);
                this.BeginInvoke(d, new object[] { strHtml });
            }
            else
            {
                _setReaderHtmlString(strHtml);
            }
        }

        void _setReaderHtmlString(string strHtml)
        {
#if NO
            m_webExternalHost_readerInfo.StopPrevious();

            if (strText == "(空)")
            {
                Global.ClearHtmlPage(this.webBrowser_reader,
                    Program.MainForm.DataDir);
                return;
            }

            Global.StopWebBrowser(this.webBrowser_reader);

            string strTempFilename = Program.MainForm.DataDir + "\\~charging_temp_reader.html";
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strText);
            }

            int nRedoCount = 0;
        REDO:
            try
            {
                this.webBrowser_reader.Navigate(strTempFilename);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }
            }
#endif
            int nRedoCount = 0;
        REDO:
            try
            {
                if (strHtml == "(空)")
                    this.m_webExternalHost_readerInfo.ClearHtmlPage();
                else
                    this.m_webExternalHost_readerInfo.SetHtmlString(strHtml, "reader_html");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }
            }
        }

        delegate void Delegate_SetReaderTextString(string strText);
        /// <summary>
        /// 显示读者文本字符串
        /// </summary>
        /// <param name="strText">文本字符串</param>
        public void SetReaderTextString(string strText)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_SetReaderTextString d = new Delegate_SetReaderTextString(SetReaderTextString);
                this.BeginInvoke(d, new object[] { strText });
                return;
            }

            int nRedoCount = 0;
        REDO:
            try
            {
                this.m_webExternalHost_readerInfo.SetTextString(strText, "reader_text");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }
            }
        }

        #region IChargingForm 接口相关

        // 2008/10/31 
        ChargingInfoHost m_chargingInfoHost = null;

        /// <summary>
        /// 获得 ChargingInfoHost 对象
        /// </summary>
        internal ChargingInfoHost CharingInfoHost
        {
            get
            {
                if (this.m_chargingInfoHost == null)
                {
                    m_chargingInfoHost = new ChargingInfoHost();
                    m_chargingInfoHost.ap = MainForm.AppInfo;
                    m_chargingInfoHost.window = this;
                    if (this.StopFillingWhenCloseInfoDlg == true)
                    {
                        m_chargingInfoHost.StopGettingSummary -= new EventHandler(m_chargingInfoHost_StopGettingSummary);
                        m_chargingInfoHost.StopGettingSummary += new EventHandler(m_chargingInfoHost_StopGettingSummary);
                    }
                }
                else
                {
                    if (this.StopFillingWhenCloseInfoDlg == false)
                    {
                        m_chargingInfoHost.StopGettingSummary -= new EventHandler(m_chargingInfoHost_StopGettingSummary);
                    }
                    else
                    {
                        m_chargingInfoHost.StopGettingSummary -= new EventHandler(m_chargingInfoHost_StopGettingSummary);
                        m_chargingInfoHost.StopGettingSummary += new EventHandler(m_chargingInfoHost_StopGettingSummary);
                    }
                }

                return m_chargingInfoHost;
            }
        }

        void m_chargingInfoHost_StopGettingSummary(object sender, EventArgs e)
        {
            if (this.m_webExternalHost_readerInfo != null)
                this.m_webExternalHost_readerInfo.StopPrevious();
            this.webBrowser_reader.Stop();
        }
        // 信息对话框的不透明度
        public double InfoDlgOpacity
        {
            get
            {
                return (double)Program.MainForm.AppInfo.GetInt(
                    "charging_form",
                    "info_dlg_opacity",
                    100) / (double)100;
            }
        }

        /// <summary>
        /// 是否要在关闭信息对话框的时候自动停止填充
        /// </summary>
        public bool StopFillingWhenCloseInfoDlg
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
    "charging_form",
    "stop_filling_when_close_infodlg",
    true);
            }
        }

        /// <summary>
        /// 自动操作唯一事项
        /// </summary>
        public bool AutoOperSingleItem
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "auto_oper_single_item",
                    false);
            }
        }

        /// <summary>
        /// 是否启用 ISBN 借书还书功能
        /// </summary>
        public bool UseIsbnBorrow
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "isbn_borrow",
                    true);
            }
        }

        /// <summary>
        /// 显示快速操作对话框
        /// </summary>
        /// <param name="color">信息颜色</param>
        /// <param name="strCaption">对话框标题文字</param>
        /// <param name="strMessage">消息内容文字</param>
        /// <param name="nTarget">对话框关闭后要切换去的位置。为 READER_BARCODE READER_PASSWORD ITEM_BARCODE 之一</param>
        public void FastMessageBox(InfoColor color,
            string strCaption,
            string strMessage,
            int nTarget)
        {
            string strFastInputText = ChargingInfoDlg.Show(
                this.CharingInfoHost,
                strMessage,
                color,
                strCaption,
                this.InfoDlgOpacity,
                Program.MainForm.DefaultFont);

            // this.SwitchFocus(nTarget, strFastInputText);
            if (string.IsNullOrEmpty(strFastInputText) == false)
            {
                this.textBox_input.Text = strFastInputText;
                AsyncDoAction(this._funcstate, strFastInputText);
            }
        }

        #endregion

        delegate void Delegate_DisplayTask(string strAction,
            ChargingTask task);
        //
        // 在显示列表中操作一个 Task 行
        // parameters:
        //      strAction   add remove refresh refresh_and_visible
        internal void DisplayTask(string strAction,
            ChargingTask task)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_DisplayTask d = new Delegate_DisplayTask(_displayTask);
                this.Invoke(d, new object[] { strAction, task });
            }
            else
            {
                _displayTask(strAction, task);
            }
        }

        void _displayTask(string strAction, ChargingTask task)
        {
            if (strAction == "add")
            {
                DpRow line = new DpRow();
                line.Style = DpRowStyle.HorzGrid;
                line.BackColor = this.TaskBackColor;    // SystemColors.Window;
                line.ForeColor = this.TaskForeColor;
                task.RefreshDisplay(line);

                line.Tag = task;
                this.dpTable_tasks.Rows.Add(line);
                if (this._bScrollBarTouched == false)
                {
                    // TODO: 应该分为两种情况  希望看到最末事项的，和看中间的。信号是触动卷滚条到底部；拖动卷滚条到中部
                    this.dpTable_tasks.FocusedItem = line;
                    line.EnsureVisible();
                }
            }
            else if (strAction == "remove")
            {
                DpRow line = FindTaskLine(task);
                if (line != null)
                    this.dpTable_tasks.Rows.Remove(line);
                else
                {
                    // Debug.Assert(false, "");
                }
            }
            else if (strAction == "refresh"
                || strAction == "refresh_and_visible")
            {
                DpRow line = FindTaskLine(task);
                if (line != null)
                {
                    // 刷新显示
                    task.RefreshDisplay(line);

                    if (this.StateSpeak != "[不朗读]")
                    {
                        string strContent = task.GetSpeakContent(line, this.StateSpeak);
                        if (string.IsNullOrEmpty(strContent) == false)
                            Program.MainForm.Speak(strContent);
                    }

                    if (task.Action == "load_reader_info" && string.IsNullOrEmpty(task.ReaderXml) == false)
                        task.RefreshPatronCardDisplay(line);

                    if (this._bScrollBarTouched == false)
                    {
                        // 如果刷新的对象是 Focus 对象，则确保显示在视野范围内
                        // TODO: 当发现中途人触动了控件时，这个功能要禁用，以免对人的操作发生干扰
                        if (this.dpTable_tasks.FocusedItem == line)
                            line.EnsureVisible();
                        else
                        {
                            if (strAction == "refresh_and_visible")
                                line.EnsureVisible();
                        }
                    }
                }
            }

            // 刷新读者摘要窗口
            if (this._patronSummaryForm != null
                && this._patronSummaryForm.Visible
                && strAction == "add")
                this._patronSummaryForm.OnTaskStateChanged(strAction, task);
        }

        // 创建读者摘要
        internal List<PatronSummary> BuildPatronSummary(ChargingTask exclude_task)
        {
            List<PatronSummary> results = new List<PatronSummary>();
            PatronSummary current_summary = null;
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task == exclude_task)
                    continue;

                if (task.Action == "load_reader_info")
                {
                    // 为前一个读者
                    if (current_summary != null)
                        current_summary.RefreshColorList();

                    current_summary = new PatronSummary();
                    current_summary.Name = task.ReaderName;
                    current_summary.Barcode = task.ReaderBarcode;
                    current_summary.Tasks.Add(task);
                    results.Add(current_summary);

                    continue;
                }

                if (current_summary != null)
                    current_summary.Tasks.Add(task);
            }

            if (current_summary != null)
                current_summary.RefreshColorList();
            return results;
        }

        // 是否为姓名
        // 包含一个以上汉字，或者 ~ 开头的任意文字
        static bool IsName(string strText)
        {
            if (string.IsNullOrEmpty(strText) == false && strText[0] == '@')
                return false;
            if (StringUtil.ContainHanzi(strText) == true)
                return true;
            if (StringUtil.HasHead(strText, "~") == true)
                return true;
            return false;
        }

        bool __bLoadReaderInfo = false;   // true
        /// <summary>
        /// 后面是否需要输入 证条码号。 false 表示需要输入册条码号
        /// </summary>
        public bool WillLoadReaderInfo
        {
            get
            {
                return __bLoadReaderInfo;
            }
            set
            {
                bool bChanged = false;
                if (__bLoadReaderInfo != value)
                    bChanged = true;

                __bLoadReaderInfo = value;

                if (bChanged == true)
                {
                    /*
                    if (value == true)
                        Program.MainForm.EnterPatronIdEdit(InputType.PQR);
                    else
                        Program.MainForm.LeavePatronIdEdit();
                     * */
                    SetInputMessage(value);

                    // EnterOrLeavePQR(value);
                    EnterOrLeavePQR(true, InputType.ALL);
                }

            }
        }

        delegate void Delegate_EnterOrLeavePQR(bool bEnter, InputType input_type);
        // 进入或离开 PQR 状态
        void EnterOrLeavePQR(bool bEnter, InputType input_type = InputType.ALL)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired == true)
            {
                Delegate_EnterOrLeavePQR d = new Delegate_EnterOrLeavePQR(EnterOrLeavePQR);
                this.BeginInvoke(d, new object[] { bEnter, input_type });
                return;
            }

            if (Program.MainForm != null)
            {
                if (bEnter == true)
                    Program.MainForm.EnterPatronIdEdit(input_type);
                else
                    Program.MainForm.LeavePatronIdEdit();
            }
        }

        delegate void Delegate_DoAction(FuncState func,
            string strText,
            string strTaskID,
            string strParameters);
        // //
        /// <summary>
        /// 执行一个出纳动作。
        /// 由于这是异步执行，不能立即返回操作结果，需要后面主动去查询
        /// </summary>
        /// <param name="func">出纳功能</param>
        /// <param name="strText">字符串。可能是证条码号，也可能是册条码号</param>
        /// <param name="strTaskID">任务 ID，用于管理和查询任务状态</param>
        /// <param name="strParameters"></param>
        public void AsyncDoAction(FuncState func,
            string strText,
            string strTaskID = "",
            string strParameters = "")
        {
            Delegate_DoAction d = new Delegate_DoAction(_doAction);
            this.BeginInvoke(d, new object[] { func, strText, strTaskID, strParameters });
        }

        // 盘算是否为 ISBN 字符串
        // 如果用 ISBN 作为前缀，返回的时候 strTextParam 中会去掉前缀部分。这样便于用于对话框检索
        public static bool IsISBN(ref string strTextParam)
        {
            string strText = strTextParam;

            if (string.IsNullOrEmpty(strText) == true)
                return false;
            strText = strText.Replace("-", "").ToUpper();
            if (string.IsNullOrEmpty(strText) == true)
                return false;

            if (StringUtil.HasHead(strText, "ISBN") == true)
            {
                strText = strText.Substring("ISBN".Length).Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    return false;
                strTextParam = strText;
                return true;
            }

            // 2015/5/8
            if (strText.ToUpper().EndsWith("ISBN") == true)
            {
                strText = strText.Substring(0, strText.Length - "ISBN".Length).Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    return false;
                strTextParam = strText;
                return true;
            }

            // string strError = "";
            // return:
            //      -1  出错
            //      0   校验正确
            //      1   校验不正确。提示信息在strError中
            int nRet = IsbnSplitter.VerifyISBN(strText,
                out string strError);
            if (nRet == 0)
            {
                // 2016/12/15
                if (strText.Length == 10 && strText[0] != '7')
                {
                    // 10 位 ISBN，不是中国的出版物，则当作不是 ISBN 字符串。
                    // 如果确实需要输入这样的 ISBN，请这样输入“ISBN2010120035”
                    return false;
                }
                return true;
            }

#if NO
            if (strText.Length == 13)
            {
                if (IsbnSplitter.IsIsbn13(strText) == true)
                    return true;
            }
#endif

            return false;
        }

        /// <summary>
        /// 盘点操作中要用到的批次号
        /// </summary>
        public string BatchNo
        {
            get;
            set;
        }

        /// <summary>
        /// 盘点操作中用于筛选的馆藏地列表
        /// </summary>
        public List<string> FilterLocations
        {
            get;
            set;
        }

        // parameters:
        //      strTaskID   任务 ID，用于管理和查询任务状态
        //      strParameters   附加的参数。可用于描述测试要求
        void _doAction(FuncState func,
            string strText,
            string strTaskID,
            string strParameters)
        {
            if (string.IsNullOrEmpty(strText) == true)
            {
                MessageBox.Show(this, "请输入适当的条码号");
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }

            // 如果中间(温和)停止过，则需要重新启动线程
            if (this._taskList.Stopped == true)
                this._taskList.BeginThread();

#if NO
            if (this._summaryList.Stopped == true)
                this._summaryList.BeginThread();
#endif
            // m_webExternalHost_readerInfo.StopPrevious();
            int nRet = 0;
            string strError = "";

            if ((this.UseIsbnBorrow == true && IsISBN(ref strText) == true)
                || strText.ToLower() == "?b")
            {
                // return:
                //      -1  error
                //      0   放弃
                //      1   成功
                nRet = SelectOneItem(func,
                    strText.ToLower() == "?b" ? "" : strText,
                    out string strItemBarcode,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "选择册记录的过程中出错: " + strError);
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "已取消选择册记录。注意操作并未执行");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                    return;
                }

                strText = strItemBarcode;
            }

            // 变换条码号
            // return:
            //      -1  出错
            //      0   不需要进行变换
            //      1   需要进行变换
            nRet = Program.MainForm.NeedTransformBarcode(
                Program.MainForm.FocusLibraryCode,
                out strError);
            if (nRet == -1)
            {
                // TODO: 语音提示
                // TODO: 红色对话框
                MessageBox.Show(this, strError);
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }
            if (nRet == 1)
            {

                // 2017/1/4
                nRet = Program.MainForm.TransformBarcode(
                    Program.MainForm.FocusLibraryCode,
                    ref strText,
                    out strError);
                if (nRet == -1)
                {
                    // TODO: 语音提示
                    // TODO: 红色对话框
                    MessageBox.Show(this, strError);
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                    return;
                }

                // TODO: 如何让操作者能看到变换后的字符串?
            }

            // 检查条码号，如果是读者证条码号，则 func = LoadPatronInfo
            if (this.NeedVerifyBarcode == true)
            {
                if (StringUtil.IsIdcardNumber(strText) == true
                    || IsName(strText) == true)
                {
                    WillLoadReaderInfo = true;
                }
                else if (func == dp2Circulation.FuncState.Read
                    && string.IsNullOrEmpty(strText) == false
                    && strText.ToLower().StartsWith("@bibliorecpath:") == true)
                {
                    if (this.WillLoadReaderInfo == true)
                    {
                        // TODO: 语音提示
                        MessageBox.Show(this, "这里需要输入 证 条码号，而您输入的 '" + strText + "' 是一个 册 条码号。\r\n\r\n请重新输入");
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus(); // 2020/6/2
                        return;
                    }
                }
                else
                {

                    // 形式校验条码号
                    // return:
                    //      -2  服务器没有配置校验方法，无法校验
                    //      -1  error
                    //      0   不是合法的条码号
                    //      1   是合法的读者证条码号
                    //      2   是合法的册条码号
                    nRet = VerifyBarcode(
                        Program.MainForm.FocusLibraryCode,  // this.Channel.LibraryCodeList,
                        strText,
                        out strError);
                    if (nRet == -2)
                    {
                        MessageBox.Show(this, "服务器没有配置条码号验证脚本，无法使用验证条码号功能。请在前端参数配置对话框的“快捷出纳”属性页中清除“校验输入的条码号”事项");
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus();
                        return;
                    }
                    if (nRet == -1)
                    {
                        // TODO: 语音提示
                        // TODO: 红色对话框
                        var result = MessageBox.Show(this,
                            strError + "\r\n\r\n是否停止操作?",
                            "QuickChargingForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        {
                            this.textBox_input.SelectAll();
                            this.textBox_input.Focus();
                            return;
                        }
                        else
                            goto FREE;
                    }
                    if (nRet == 0)
                    {
                        // TODO: 语音提示
                        // TODO: 红色对话框
                        var result = MessageBox.Show(this,
                            $"'{strText}' (当前操作员位于 '{Program.MainForm.FocusLibraryCode}')不是合法的条码号: {strError}\r\n\r\n是否停止操作?",
                            "QuickChargingForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        {
                            this.textBox_input.SelectAll();
                            this.textBox_input.Focus();
                            return;
                        }
                        else
                            goto FREE;
                    }
                    // 有可能 验证条码号的时候因为 EnableControls 丢失了焦点
                    this.textBox_input.Focus();

                    if (nRet == 1)
                        WillLoadReaderInfo = true;
                    else
                    {
                        Debug.Assert(nRet == 2, "");
                        if (this.WillLoadReaderInfo == true)
                        {
                            // TODO: 语音提示
                            MessageBox.Show(this, "这里需要输入 证 条码号，而您输入的 '" + strText + "' 是一个 册 条码号。\r\n\r\n请重新输入");
                            this.textBox_input.SelectAll();
                            this.textBox_input.Focus(); // 2020/6/2
                            return;
                        }
                    }
                }
            }

        FREE:

            if (WillLoadReaderInfo == true)
            {
                func = FuncState.LoadPatronInfo;
                // _bLoadReaderInfo = false;
            }

            ChargingTask task = new ChargingTask();
            task.ID = strTaskID;
            if (func == FuncState.LoadPatronInfo)
            {
                // 此处限定只能是读者证条码号
                if (IsReaderType(strText) == -1)
                {
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus(); // 2020/6/2
                    return;
                }
                task.ReaderBarcode = GetContent(strText);   // strText
                task.Action = "load_reader_info";
            }
            else if (func == dp2Circulation.FuncState.Borrow
                || func == FuncState.SpecialBorrow)
            {
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "borrow";
                if (func == FuncState.SpecialBorrow)
                {
                    // 检查 dp2library 版本号
                    if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.85") < 0)
                    {
                        MessageBox.Show(this, "本功能需要 dp2library 版本在 3.85 以上");
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus();
                        return;
                    }

                    task.Parameters += ",special:dontCheckOverdue";
                }
            }
            else if (func == dp2Circulation.FuncState.ContinueBorrow)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    // TODO: 这里直接出现对话框搜集读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus(); // 2020/6/2
                    return;
                }
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "borrow";
            }
            else if (func == dp2Circulation.FuncState.Renew
                || func == FuncState.SpecialRenew)
            {
                // task.ReaderBarcode = "";
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "renew";
                task.Parameters = strParameters;
                if (func == FuncState.SpecialRenew)
                {
                    // 检查 dp2library 版本号
                    if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.85") < 0)
                    {
                        MessageBox.Show(this, "本功能需要 dp2library 版本在 3.85 以上");
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus();
                        return;
                    }

                    task.Parameters += ",special:dontCheckOverdue";
                }
            }
            else if (func == dp2Circulation.FuncState.VerifyRenew)
            {
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "verify_renew";
            }
            else if (func == dp2Circulation.FuncState.Return)
            {
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "return";
                task.Parameters = strParameters;
            }
            else if (func == dp2Circulation.FuncState.InventoryBook)
            {
                task.ReaderBarcode = this.BatchNo;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "inventory";
            }
            else if (func == dp2Circulation.FuncState.VerifyReturn)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    // TODO: 这里直接出现对话框搜集读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus(); // 2020/6/2
                    return;
                }
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "verify_return";
                task.Parameters = strParameters;
            }
            else if (func == dp2Circulation.FuncState.Lost)
            {
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "lost";
                task.Parameters = strParameters;
            }
            else if (func == dp2Circulation.FuncState.VerifyLost)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus(); // 2020/6/2
                    return;
                }
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "verify_lost";
                task.Parameters = strParameters;
            }
            else if (func == dp2Circulation.FuncState.Read)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    // TODO: 这里直接出现对话框搜集读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus(); // 2020/6/2
                    return;
                }
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "read";
            }
            else if (func == dp2Circulation.FuncState.Boxing)
            {
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "boxing";
                task.Parameters = strParameters;
            }
            else if (func == dp2Circulation.FuncState.Transfer)
            {
                // 检查 dp2library 版本
                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.16") < 0)
                {
                    // TODO: 语音提示
                    // TODO: 红色对话框
                    MessageBox.Show(this, $"调拨功能要求 dp2library 版本为 3.16 以上(而现在是 {Program.MainForm.ServerVersion})");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                    return;
                }
                task.ItemBarcode = GetContent(strText);
                task.ItemBarcodeEasType = GetEasType(strText);
                task.Action = "transfer";

                List<string> parameters = new List<string>();
                if (string.IsNullOrEmpty(strParameters) == false)
                    parameters.Add(strParameters);

                parameters.Add($"location:{this._targetLocation}");
                parameters.Add($"batchNo:{this.BatchNo}");

                task.Parameters = StringUtil.MakePathList(parameters);
            }

            this.textBox_input.SelectAll();
            this.textBox_input.Focus(); // 2020/6/2

            try
            {
                this._taskList.AddTask(task);
            }
            catch (LockException)
            {
                Delegate_DoAction d = new Delegate_DoAction(_doAction);
                this.BeginInvoke(d, new object[] { func, strText, strTaskID });
            }
        }

        // 获得一个字符串的 pii: 或者 uid: 内容部分
        // 例: pii:13412341324,tou:10
        static string GetContent(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return "";
            if (strText.IndexOf(":") == -1
                || strText.StartsWith("PQR:")  // 2021/11/22
                || StringUtil.HasHead(strText, "@refID:", true)    // 2022/1/14
                )
                return ItemBarcode(strText);
            Hashtable table = StringUtil.ParseParameters(strText, ',', ':');
            string strBarcode = GetValue(table, "pii");
            if (string.IsNullOrEmpty(strBarcode) == false)
                return strBarcode;
            return GetValue(table, "uid");
        }

        // 把内容中的 @refID: 复原。避免全大写影响发送到 dp2library API 的效果
        static string ItemBarcode(string text)
        {
            if (text == null)
                return text;
            if (StringUtil.HasHead(text, "@refID:", true) == true)
            {
                return "@refID:" + text.Substring("@refID:".Length);
            }
            return text;
        }


        // 注意本函数可能会返回 null
        static string GetValue(Hashtable table, string name)
        {
            string value = (string)table[name];
            if (string.IsNullOrEmpty(value) == false)
                return value;
            value = (string)table[name.ToUpper()];
            return value;
        }

        // return:
        //      1   是读者类型
        //      0   不清楚
        //      -1  不是读者类型
        static int IsReaderType(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return -1;
            if (strText.IndexOf(":") == -1)
                return 0;
            Hashtable table = StringUtil.ParseParameters(strText, ',', ':');
            string strTypeOfUsage = GetValue(table, "tou");

            // 注意：特殊处理!
            if (strTypeOfUsage == "32")
                strTypeOfUsage = "10";

            if (string.IsNullOrEmpty(strTypeOfUsage) == false && strTypeOfUsage[0] == '8')
                return 1;
            string strBarcode = GetValue(table, "pii");
            if (string.IsNullOrEmpty(strBarcode) == false)
                return -1;
            strBarcode = GetValue(table, "uid");
            if (string.IsNullOrEmpty(strBarcode) == false)
                return -1;
            return 0;
        }

        // 获得一个字符串的 RFID 前缀类型
        // 如果是 pii: 或者 uid: 引导，并且 tou: 内容的第一位为 ‘1’，表示这是 rfid 标签
        // return:
        //      "pii" 或 "uid" 表示这是需要修改 EAS 的 RFID 标签
        //      空   表示这不是需要修改 EAS 的 RFID 标签
        static string GetEasType(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return "";
            if (strText.IndexOf(":") == -1)
                return "";
            Hashtable table = StringUtil.ParseParameters(strText, ',', ':');
            string strTypeOfUsage = GetValue(table, "tou");

            // 2019/6/13
            // 注意：特殊处理!
            if (strTypeOfUsage == "32")
                strTypeOfUsage = "10";

            if (string.IsNullOrEmpty(strTypeOfUsage) == false && strTypeOfUsage[0] != '1')
                return "";
            string strBarcode = GetValue(table, "pii");
            if (string.IsNullOrEmpty(strBarcode) == false)
                return "pii";
            strBarcode = GetValue(table, "uid");
            if (string.IsNullOrEmpty(strBarcode) == false)
                return "uid";
            return "";
        }

        DpRow FindTaskLine(ChargingTask task)
        {
            foreach (DpRow line in this.dpTable_tasks.Rows)
            {
                if (line.Tag == task)
                    return line;
            }
            return null;
        }

        private void webBrowser_reader_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (Program.MainForm.SuppressScriptErrors == true)
                e.Handled = true;
        }


        private void toolStripMenuItem_loadPatronInfo_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.LoadPatronInfo;
        }

        private void toolStripMenuItem_borrow_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Borrow,
    true,
    false);
        }

        // 统一读者继续借书。不清除现有窗口内容
        private void toolStripMenuItem_continueBorrow_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.ContinueBorrow,
false,
false);
        }

        private void toolStripMenuItem_return_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Return,
    true,
    false);
        }

        private void toolStripMenuItem_renew_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Renew,
true,
false);
        }

        private void toolStripMenuItem_verifyRenew_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.VerifyRenew,
true,
false);
        }

        private void toolStripMenuItem_lost_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Lost,
true,
false);
        }

        private void toolStripMenuItem_verifyReturn_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.VerifyReturn,
true,
false);
        }

        private void toolStripMenuItem_verifyLost_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.VerifyLost,
true,
false);
        }

        private void ToolStripMenuItem_inventoryBook_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.InventoryBook;
        }

        private void ToolStripMenuItem_read_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Read;
        }

        #region 各种配置参数

        // 加快响应的记忆变量
        int _nLogOperTime = 0;  // 0 尚未初始化; -1 false; 1 true
        // 日志记载操作耗时
        public bool LogOperTime
        {
            get
            {
                if (_nLogOperTime == 0)
                {
                    bool bRet = Program.MainForm.AppInfo.GetBoolean(
                        "quickcharging_form",
                        "log_opertime",
                        true);
                    if (bRet == true)
                        _nLogOperTime = 1;
                    else
                        _nLogOperTime = -1;
                }

                return _nLogOperTime == 1;
            }
        }

        public string DisplayFormat
        {
            get
            {
                return Program.MainForm.AppInfo.GetString("quickcharging_form",
                    "display_format",
                    "HTML");
            }
        }

        public string DisplayStyle
        {
            get
            {
                return Program.MainForm.AppInfo.GetString("quickcharging_form",
                    "display_style",
                    "light");
            }
        }

        // 朗读状态
        public string StateSpeak
        {
            get
            {
                return Program.MainForm.AppInfo.GetString("quickcharging_form",
        "state_speak",
        "[不朗读]");
            }
        }
        /// <summary>
        /// 显示读者信息的格式。为 text html 之一
        /// </summary>
        public string PatronRenderFormat
        {
            get
            {
                List<string> styles = new List<string>();
                string strDisplayStyle = this.DisplayStyle;
                if (strDisplayStyle != "light")
                    styles.Add("style_" + strDisplayStyle);

                string strFormat = "";
                if (_cardControl != null)
                {
                    if (this.NoBorrowHistory == true
                        && StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.25") >= 0)
                    {
                        styles.Add("noborrowhistory");
                        // return "xml:noborrowhistory";
                    }
                    strFormat = "xml";
                }
                else
                {
                    if (this.NoBorrowHistory == true
                        && StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.21") >= 0)
                    {
                        styles.Add("noborrowhistory");
                        // return "html:noborrowhistory";
                    }
                    strFormat = "html";
                }

                if (styles.Count == 0)
                    return strFormat;
                return strFormat + ":" + StringUtil.MakePathList(styles, "|");
            }
        }

        // 读者信息中不显示借阅历史
        public bool NoBorrowHistory
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "no_borrow_history",
                    true);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "quickcharging_form",
                    "no_borrow_history",
                    value);
            }
        }

        /// <summary>
        /// 是否自动清除输入框中内容
        /// </summary>
        public bool AutoClearTextbox
        {
            get
            {
                return true;
#if NO
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "autoClearTextbox",
                    true);
#endif
            }
        }

        /// <summary>
        /// 是否自动校验输入的条码号
        /// </summary>
        public bool NeedVerifyBarcode
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "verify_barcode",
                    false);
            }
        }

        /// <summary>
        /// 显示书目、册信息的格式。为 text html 之一
        /// </summary>
        public string RenderFormat
        {
            get
            {
                return "html";
            }
        }


        /// <summary>
        /// 是否要朗读读者姓名
        /// </summary>
        public bool SpeakPatronName
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "speak_reader_name",
                    false);
            }
        }

        /// <summary>
        /// 是否要朗读书名
        /// </summary>
        public bool SpeakBookTitle
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "speak_book_title",
                    false);
            }
        }

        #endregion


        FuncState _funcstate = FuncState.Borrow;

        FuncState FuncState
        {
            get
            {
                return _funcstate;
            }
            set
            {

                this._funcstate = value;
                this.pictureBox_action.Invalidate();
                WillLoadReaderInfo = true;
                this._bScrollBarTouched = false;

                Program.MainForm.ClearQrLastText();

                this.toolStripMenuItem_borrow.Checked = false;
                this.toolStripMenuItem_return.Checked = false;
                this.toolStripMenuItem_verifyReturn.Checked = false;
                this.toolStripMenuItem_renew.Checked = false;
                this.toolStripMenuItem_verifyRenew.Checked = false;
                this.toolStripMenuItem_lost.Checked = false;
                this.toolStripMenuItem_verifyLost.Checked = false;
                this.toolStripMenuItem_loadPatronInfo.Checked = false;
                this.toolStripMenuItem_continueBorrow.Checked = false;
                this.toolStripMenuItem_inventoryBook.Checked = false;
                this.toolStripMenuItem_read.Checked = false;
                this.toolStripMenuItem_boxing.Checked = false;
                this.toolStripMenuItem_transfer.Checked = false;
                this.toolStripMenuItem_specialBorrow.Checked = false;
                this.toolStripMenuItem_specialRenew.Checked = false;

                if (this.AutoClearTextbox == true)
                {
                    this.textBox_input.Text = "";
                }

                if (_funcstate == FuncState.Borrow)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[0];
                    this.toolStripMenuItem_borrow.Checked = true;
                }
                else if (_funcstate == FuncState.ContinueBorrow)
                {
                    //this.pictureBox_action.Image = this.imageList_func_large.Images[0];
                    this.toolStripMenuItem_continueBorrow.Checked = true;
                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.SpecialBorrow)
                {
                    this.toolStripMenuItem_specialBorrow.Checked = true;
                }
                else if (_funcstate == FuncState.Return)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[1];
                    this.toolStripMenuItem_return.Checked = true;

                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.VerifyReturn)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[1];
                    this.toolStripMenuItem_verifyReturn.Checked = true;
                }
                else if (_funcstate == FuncState.Renew)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[2];
                    this.toolStripMenuItem_renew.Checked = true;

                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.VerifyRenew)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[2];
                    this.toolStripMenuItem_verifyRenew.Checked = true;
                }
                else if (_funcstate == FuncState.SpecialRenew)
                {
                    this.toolStripMenuItem_specialRenew.Checked = true;

                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.Lost)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[3];
                    this.toolStripMenuItem_lost.Checked = true;

                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.VerifyLost)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[3];
                    this.toolStripMenuItem_verifyLost.Checked = true;
                }
                else if (_funcstate == FuncState.LoadPatronInfo)
                {
                    // this.pictureBox_action.Image = this.imageList_func_large.Images[4];
                    this.toolStripMenuItem_loadPatronInfo.Checked = true;
                }
                else if (_funcstate == FuncState.InventoryBook)
                {
                    this.toolStripMenuItem_inventoryBook.Checked = true;
                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.Read)
                {
                    this.toolStripMenuItem_read.Checked = true;
                }
                else if (_funcstate == FuncState.Boxing)
                {
                    this.toolStripMenuItem_boxing.Checked = true;

                    WillLoadReaderInfo = false;
                }
                else if (_funcstate == FuncState.Transfer)
                {
                    this.toolStripMenuItem_transfer.Checked = true;
                    WillLoadReaderInfo = false;
                    _ = Task.Run(() =>
                    {
                        this.Invoke((Action)(() =>
                        {
                            toolStripButton_selectTransferTargetLocation_Click(this, new EventArgs());
                        }));
                    });
                }
                // SetInputMessage();
            }
        }

        // 清除任务列表
        // 本函数只能被界面线程调用
        // parameters:
        //      rows    要清除的显示行。如果为 null，表示希望全部清除
        bool ClearTaskByRows(List<DpRow> rows, bool bWarning = true)
        {
            if (rows == null)
            {
                rows = new List<DpRow>();
                foreach (DpRow row in this.dpTable_tasks.Rows)
                {
                    rows.Add(row);
                }
            }

            List<ChargingTask> tasks = new List<ChargingTask>();    // 希望清除的任务
            List<ChargingTask> warning_tasks = new List<ChargingTask>();    // 需要警告的任务
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                tasks.Add(task);
                if (task.Compeleted == false)
                {
                    warning_tasks.Add(task);
                }
            }

            if (warning_tasks.Count == 0 && tasks.Count == 0)
                return true;

            if (bWarning == true)
            {
                if (warning_tasks.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
            "当前有 " + warning_tasks.Count.ToString() + " 个任务尚未完成，如果要继续，必须取消这些任务。\r\n\r\n是否要继续?\r\n\r\n(是) 继续，全部任务被清除; (否) 放弃操作",
            "QuickChargingForm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
#if OLD_CHARGING_CHANNEL
                        this._taskList.stop.DoStop();
#else
                        this._taskList.DoStop(this, new StopEventArgs());
#endif
                    }
                    else
                        return false;   // 放弃清除
                }
            }

            this._taskList.ClearTasks(tasks);
            foreach (DpRow row in rows)
            {
                this.dpTable_tasks.Rows.Remove(row);
            }
            this._summaryList.ClearRelativeTasks(tasks);

            // this._taskList.Clear();
            // this.dpTable_tasks.Rows.Clear();

            this._taskList.CurrentReaderBarcode = "";
            SetColorList();
            return true;    // 清除并继续
        }

        delegate DialogResult Delegate_AskContinue(string strText);
        internal DialogResult AskContinue(string strText)
        {
            Delegate_AskContinue d = new Delegate_AskContinue(_askContinue);
            return (DialogResult)this.Invoke(d, new object[] { strText });
        }

        internal DialogResult _askContinue(string strText)
        {
            return MessageBox.Show(this,
                strText,
                "QuickChargingForm",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
        }

        delegate void Delegate_ClearTaskList(List<ChargingTask> tasks);
        // 清除任务列表显示
        // 注意，并不负责删除 _taskList 中的元素
        internal void ClearTaskList(List<ChargingTask> tasks)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_ClearTaskList d = new Delegate_ClearTaskList(ClearTaskList);
                this.Invoke(d, new object[] { tasks });
                return;
            }

            if (tasks == null)
                this.dpTable_tasks.Rows.Clear();
            else
            {
                for (int i = 0; i < this.dpTable_tasks.Rows.Count; i++)
                {
                    DpRow row = this.dpTable_tasks.Rows[i];
                    if (tasks.IndexOf((ChargingTask)row.Tag) != -1)
                    {
                        // this.dpTable_tasks.Rows.RemoveAt(i);
                        this.dpTable_tasks.Rows.Remove(row);
                        i--;
                    }
                }
            }

            this._bScrollBarTouched = false;
        }

        delegate void Delegate_DisplayCurrentReaderBarcode(string strReaderBarcode);
        /// <summary>
        /// 显示当前证条码号到工具条上
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        void DisplayCurrentReaderBarcode(string strReaderBarcode)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_DisplayCurrentReaderBarcode d = new Delegate_DisplayCurrentReaderBarcode(DisplayCurrentReaderBarcode);
                this.BeginInvoke(d, new object[] { strReaderBarcode });
                return;
            }

            this.toolStripLabel_currentPatron.Text = strReaderBarcode;
        }

        delegate void Delegate_SetInputMessage(bool bReaderBarcode);
        // 刷新 输入号码类型的标签显示
        void SetInputMessage(bool bReaderBarcode)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_SetInputMessage d = new Delegate_SetInputMessage(SetInputMessage);
                this.BeginInvoke(d, new object[] { bReaderBarcode });
                return;
            }

            if (bReaderBarcode == true)
            {
                this.label_barcode_type.ImageIndex = 1;
                this.label_input_message.Text = "证 条码号";
            }
            else
            {
                this.label_barcode_type.ImageIndex = 0;
                this.label_input_message.Text = "册 条码号";
            }
        }

        delegate void Delegate_SetColorList();
        // 显示色条
        internal void SetColorList()
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_SetColorList d = new Delegate_SetColorList(SetColorList);
                this.BeginInvoke(d);
                return;
            }

            int nWaitingCount = 0;
            StringBuilder text = new StringBuilder(256);
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if ((task.State == "begin" || string.IsNullOrEmpty(task.State) == true)
                    && task.Action != "load_reader_info")
                    nWaitingCount++;

                if (task.Action == "load_reader_info")
                    continue;   // 装载读者的动作不计算在颜色显示内

                char color = 'W';   // 缺省为白色
                if (string.IsNullOrEmpty(task.Color) == false)
                    color = Char.ToUpper(task.Color[0]);
                text.Append(color);
            }

            this.colorSummaryControl1.ColorList = text.ToString();

            // TODO: 是否延迟显示，避免反复出现和隐藏
            if (nWaitingCount > 0)
            {
                string strState = "";
                if (this._taskList.Stopped == true)
                    strState = "已暂停任务处理。\r\n";
                this.FloatingMessage = strState + "有 " + nWaitingCount.ToString() + " 个任务尚未完成 ...";
            }
            else
            {
                if (this._taskList.Stopped == true)
                    this.FloatingMessage = "已暂停任务处理。";
                else
                    this.FloatingMessage = "";
            }

            // 刷新读者摘要窗口
            if (this._patronSummaryForm != null
                && this._patronSummaryForm.Visible)
            {
                List<PatronSummary> summaries = BuildPatronSummary(null);
                _patronSummaryForm.PatronSummaries = summaries;
                _patronSummaryForm.FillList();
                if (summaries.Count == 0)
                {
                    _patronSummaryForm.Close();
                    _patronSummaryForm = null;
                }
            }

        }

        // 当前读者证条码号已经成功设置
        internal void CurrentReaderBarcodeChanged(string strReaderBarcode)
        {
            // 在装载读者记录的时候，不改变应输入条码号类型的提示
            if (this.FuncState != dp2Circulation.FuncState.LoadPatronInfo)
            {
                if (string.IsNullOrEmpty(strReaderBarcode) == false)
                    this.WillLoadReaderInfo = false;
                else
                    this.WillLoadReaderInfo = true;

                // SetInputMessage();
            }

            // 显示到 ToolStrip 上
            DisplayCurrentReaderBarcode(strReaderBarcode);
        }

        /// <summary>
        /// 功能类型。设置时带有焦点切换功能
        /// </summary>
        public FuncState SmartFuncState
        {
            get
            {
                return _funcstate;
            }
            set
            {

                SmartSetFuncState(value,
                    true,
                    true);
            }
        }

        // 智能设置功能名。
        // parameters:
        //      bClearInfoWindow    切换中是否清除信息窗内容
        //      bDupAsClear 是否把重复的设置动作当作清除输入域内容来理解
        void SmartSetFuncState(FuncState value,
            bool bClearInfoWindow,
            bool bDupAsClear)
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();

            this.webBrowser_reader.Stop();

            //watch.Stop();
            //Debug.WriteLine("this.webBrowser_reader.Stop() elapsed " + watch.Elapsed.TotalSeconds);
            //watch.Restart();

            this.m_webExternalHost_readerInfo.StopPrevious();

            //watch.Stop();
            //Debug.WriteLine("this.m_webExternalHost_readerInfo.StopPrevious() elapsed " + watch.Elapsed.TotalSeconds);
            //watch.Restart();

            // 清除 webbrowser 和任务列表
            if (bClearInfoWindow == true)
            {
                //watch.Stop();
                //Debug.WriteLine("---1  elapsed " + watch.Elapsed.TotalSeconds);
                //watch.Restart();

                if (ClearTaskByRows(null, true) == false)
                    return;

                //watch.Stop();
                //Debug.WriteLine("---2  elapsed " + watch.Elapsed.TotalSeconds);
                //watch.Restart();

                if (this.IsCardMode == true)
                    SetReaderCardString("");
                else
                    SetReaderHtmlString("(空)");
            }


            FuncState old_funcstate = this._funcstate;

            this.FuncState = value;

            // 同一读者借的附加判断
            if (value == dp2Circulation.FuncState.ContinueBorrow)
            {
                // TODO: 警告一下，让操作者知道需要先输入读者证条码号
                // 或者将功能菜单 disabled

                // 如果当前证条码号为空，则只好让操作者先输入读者证条码号，本功能就退化为普通借书功能了
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                    this.WillLoadReaderInfo = true;
            }

            // 切换为不同的功能的时候，定位焦点
            if (old_funcstate != this._funcstate)
            {
                if (this.AutoClearTextbox == true)
                {
                    this.textBox_input.Text = "";
                }

                if (this._funcstate != FuncState.Return)
                {
                    this.textBox_input.SelectAll();
                }
                else
                {
                    this.textBox_input.SelectAll();
                }
            }
            else // 重复设置为同样功能，当作清除功能
            {
                if (this.AutoClearTextbox == true)
                {
                    this.textBox_input.Text = "";
                }
                else
                {
                    if (bDupAsClear == true)
                    {
                        this.textBox_input.Text = "";
                    }
                }

                // focus input 
                this.textBox_input.Focus();
            }

            //watch.Stop();
            //Debug.WriteLine("SmartSetFuncState elapsed " + watch.Elapsed.TotalSeconds);
            //watch.Restart();
        }

        // 查询任务状态
        // 如果列表中同样 ID 的任务超过一个，则只给出第一个的状态
        // return:
        //      null    指定的任务没有找到
        //      其他      任务状态字符串
        public string GetTaskState(string strTaskID)
        {
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                row.Selected = false;
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task.ID == strTaskID)
                    return task.State;
            }

            return null;
        }

        // 将指定的任务行滚入可见范围，并设为焦点状态
        // parameters:
        //      index   事项的 index，是排出了颜色为 "" 的 Task
        bool EnsureVisibleLine(int index)
        {
            int i = 0;
            bool bFound = false;
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                row.Selected = false;
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (string.IsNullOrEmpty(task.Color) == true)
                    continue;
                if (i == index)
                {
                    this.dpTable_tasks.FocusedItem = row;
                    row.Selected = true;
                    row.EnsureVisible();
                    bFound = true;
                }
                i++;
            }

            return bFound;
        }

        private void colorSummaryControl1_Click(object sender, EventArgs e)
        {
            Point pt = Control.MousePosition;
            pt = this.colorSummaryControl1.PointToClient(pt);
            int index = this.colorSummaryControl1.HitTest(pt.X, pt.Y);
            // MessageBox.Show(this, index.ToString());
            EnsureVisibleLine(index);
        }

        private void dpTable_tasks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            //ToolStripMenuItem subMenuItem = null;
            ToolStripSeparator menuSepItem = null;

            DpRow selected_row = null;
            ChargingTask selected_task = null;
            if (this.dpTable_tasks.SelectedRows.Count > 0)
            {
                selected_row = this.dpTable_tasks.SelectedRows[0];
                selected_task = (ChargingTask)selected_row.Tag;
            }

            // 
            menuItem = new ToolStripMenuItem("打开到 读者窗(&R)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToReaderInfoForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 交费窗(&A)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToAmerceForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 激活窗[源] (&S)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToActivateForm_old_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 停借窗(&M)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToReaderManageForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 激活窗[目标] (&T)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToActivateForm_old_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 册窗(&I)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ItemBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToItemInfoForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 种册窗(&E)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ItemBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToEntityForm_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("复制 [" + this.dpTable_tasks.SelectedRows.Count.ToString() + "] (&D)");
            if (this.dpTable_tasks.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_copy_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("刷新摘要(&R)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ItemBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_refreshSummary_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("删除任务 [" + this.dpTable_tasks.SelectedRows.Count.ToString() + "] (&D)");
            if (this.dpTable_tasks.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_deleteTask_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("任务数 (&C)");
            menuItem.Click += new EventHandler(menuItem_countTask_Click);
            contextMenu.Items.Add(menuItem);

            // 
            if (StringUtil.IsDevelopMode() == true)
            {
#if NO
            // 
            menuItem = new ToolStripMenuItem("test");
            menuItem.Click += new EventHandler(menuItem_test_Click);
            contextMenu.Items.Add(menuItem);

#endif

                menuItem = new ToolStripMenuItem("test change state");
                menuItem.Click += new EventHandler(menuItem_test_change_state_Click);
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.Show(this.dpTable_tasks, e.Location);
        }

        // 获得一个 DpRow 行的用于 Copy 的文本
        static string GetRowText(DpRow row)
        {
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (DpCell cell in row)
            {
                // 跳过第一列
                if (i > 0)
                {
                    if (text.Length > 0)
                        text.Append("\t");
                    text.Append(cell.Text);
                }

                i++;
            }

            return text.ToString();
        }

        // TODO: 增加双格式功能，可以 paste 到任务列表中。任务还可以重新提交执行
        void menuItem_copy_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
#if NO
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                strTotal.Append(task.ErrorInfo + "\r\n");
#endif
                strTotal.Append(GetRowText(row) + "\r\n");

            }

            Clipboard.SetDataObject(strTotal.ToString(), true);

            this.Cursor = oldCursor;
        }

        void menuItem_test_change_state_Click(object sender, EventArgs e)
        {
            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                task.State = "begin";
                this.DisplayTask("refresh", task);
            }
        }

        // TODO: 可以成批获得摘要
        void menuItem_test_Click(object sender, EventArgs e)
        {
            HtmlDocument doc = this.webBrowser_reader.Document;
            HtmlElementCollection col = doc.GetElementsByTagName("td");
            string strResult = "";
            List<HtmlElement> nodes = new List<HtmlElement>();
            foreach (HtmlElement ele in col)
            {
                string strClass = ele.GetAttribute("className");
                strResult += ele.OuterHtml;
                if (strClass.IndexOf("pending") != -1)
                    nodes.Add(ele);
            }

            m_webExternalHost_readerInfo.IsInLoop = true;
            foreach (HtmlElement ele in nodes)
            {

                string strText = ele.InnerText.Trim();
                string strLeft = "";
                string strRight = "";
                if (strText.IndexOf(":") != -1)
                {
                    StringUtil.ParseTwoPart(strText,
                        ":",
                        out strLeft,
                        out strRight);
                }
                else
                    strRight = strText;

                ele.InnerHtml = "<img src='./servermapped/images/ajax-loader.gif'></img>";

                if (strLeft == "P")
                    ele.InnerHtml = m_webExternalHost_readerInfo.GetPatronSummary(strRight);
                else
                    ele.InnerHtml = "<div class='wide'><div>" + m_webExternalHost_readerInfo.GetSummary(strRight, false);

                string strClass = ele.GetAttribute("className");
                if (string.IsNullOrEmpty(strClass) == false)
                {
                    strClass = strClass.Replace("pending", "");
                    ele.SetAttribute("className", strClass);
                }
            }

            // MessageBox.Show(this, strResult);
        }

        // 统计当前所有成功的任务数。也就是绿色和黄色的任务数
        void menuItem_countTask_Click(object sender, EventArgs e)
        {
            int count = 0;
            int nErrorCount = 0;

            // List<DpRow> rows = new List<DpRow>();
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task.State == "error")
                    nErrorCount++;
                if (task.Color == "green" || task.Color == "yellow")
                    count++;
            }

            string text = $"{count} 个成功任务";
            Program.MainForm.Speak(text);
            this.ShowMessageAutoClear(text, "green", 5000, true);
        }


        // 删除选定的任务
        // 如果有没有完成的任务，则需要统一中断(等待完成)，然后再删除任务
        void menuItem_deleteTask_Click(object sender, EventArgs e)
        {
            int nErrorCount = 0;
            int nNotCompleteCount = 0;
            int nYellowCount = 0;

            List<DpRow> rows = new List<DpRow>();
            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task.State == "error")
                    nErrorCount++;
                if ((string.IsNullOrEmpty(task.State) == true && task.Action != "load_reader_info")
                    || task.State == "begin")
                    nNotCompleteCount++;
                if (task.Color == "yellow")
                    nYellowCount++;

                rows.Add(row);
            }

            if (rows.Count == 0)
            {
                MessageBox.Show(this, "当前没有任何任务可以清除");
                return;
            }

            if (nErrorCount + nNotCompleteCount + nYellowCount != 0)
            {
                string strText = "";
                if (nNotCompleteCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "; ";
                    strText += nNotCompleteCount.ToString() + " 个未完成的事项";
                }
                if (nErrorCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "; ";
                    strText += nErrorCount.ToString() + " 个发生错误的(红色)事项";
                }
                if (nYellowCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "; ";
                    strText += nYellowCount.ToString() + " 个需要进一步处理的(黄色)事项";
                }

                DialogResult result = MessageBox.Show(this,
"当前有 " + strText + "。\r\n\r\n确实要清除选定的 " + rows.Count.ToString() + " 个事项?",
"QuickChargingForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            ClearTaskByRows(rows, false);
        }

        // 打开到 读者窗
        void menuItem_loadToReaderInfoForm_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("readerinfo_form");
        }

        // 打开到 交费窗
        void menuItem_loadToAmerceForm_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("amerce_form");

        }

        // 打开到 激活窗 (源)
        void menuItem_loadToActivateForm_old_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("activate_form_old");
        }

        // 打开到 激活窗 (目标)
        void menuItem_loadToActivateForm_new_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("activate_form_new");
        }

        // 打开到 停借窗
        void menuItem_loadToReaderManageForm_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("readermanage_form");
        }

        // 装入读者相关类型的窗口
        void LoadToPatronTypeForm(string strType)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            DpRow selected_row = null;
            ChargingTask selected_task = null;

            selected_row = this.dpTable_tasks.SelectedRows[0];
            selected_task = (ChargingTask)selected_row.Tag;

            if (string.IsNullOrEmpty(selected_task.ReaderBarcode) == true)
            {
                strError = "所选定的任务事项不具备证条码号信息";
                goto ERROR1;
            }

            if (strType == "readerinfo_form")
            {
                ReaderInfoForm form = Program.MainForm.EnsureReaderInfoForm();
                Global.Activate(form);

                form.LoadRecord(selected_task.ReaderBarcode,
                    false);
            }
            if (strType == "amerce_form")
            {
                AmerceForm form = Program.MainForm.EnsureAmerceForm();
                Global.Activate(form);

                form.LoadReader(selected_task.ReaderBarcode, true);
            }
            if (strType == "activate_form_old")
            {
                ActivateForm form = Program.MainForm.EnsureActivateForm();
                Global.Activate(form);

                form.LoadOldRecord(selected_task.ReaderBarcode);
            }
            if (strType == "activate_form_new")
            {
                ActivateForm form = Program.MainForm.EnsureActivateForm();
                Global.Activate(form);

                form.LoadNewRecord(selected_task.ReaderBarcode);
            }
            if (strType == "readermanage_form")
            {
                ReaderManageForm form = Program.MainForm.EnsureReaderManageForm();
                Global.Activate(form);

                form.LoadRecord(selected_task.ReaderBarcode);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打开到 册窗
        void menuItem_loadToItemInfoForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            DpRow selected_row = null;
            ChargingTask selected_task = null;

            selected_row = this.dpTable_tasks.SelectedRows[0];
            selected_task = (ChargingTask)selected_row.Tag;

            if (string.IsNullOrEmpty(selected_task.ItemBarcode) == true)
            {
                strError = "所选定的任务事项不具备册条码号信息";
                goto ERROR1;
            }

            ItemInfoForm form = Program.MainForm.EnsureItemInfoForm();
            Global.Activate(form);

            form.LoadRecord(selected_task.ItemBarcode);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 刷新摘要
        void menuItem_refreshSummary_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
                ChargingTask charging_task = (ChargingTask)row.Tag;
                if (string.IsNullOrEmpty(charging_task.ItemBarcode) == true)
                    continue;
                this.AddItemSummaryTask(charging_task.ItemBarcode,
                    null,
                    charging_task);
            }

            return;
        ERROR1:
            this.ShowMessage(strError, "error", true);
        }

        // 打开到 种册窗
        void menuItem_loadToEntityForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            DpRow selected_row = null;
            ChargingTask selected_task = null;

            selected_row = this.dpTable_tasks.SelectedRows[0];
            selected_task = (ChargingTask)selected_row.Tag;

            if (string.IsNullOrEmpty(selected_task.ItemBarcode) == true)
            {
                strError = "所选定的任务事项不具备册条码号信息";
                goto ERROR1;
            }

            EntityForm form = Program.MainForm.EnsureEntityForm();
            Global.Activate(form);

            form.LoadItemByBarcode(selected_task.ItemBarcode, false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void QuickChargingForm_Activated(object sender, EventArgs e)
        {
            this.textBox_input.Focus();
            //OpenRfidCapture(true);
            //Debug.WriteLine("activated");
            this.PauseRfid = false;
        }

        private void QuickChargingForm_Deactivate(object sender, EventArgs e)
        {
            //OpenRfidCapture(false);
            //Debug.WriteLine("deactivate");
            this.PauseRfid = true;

            SetInputFocusState(false);
        }

        private void textBox_input_Enter(object sender, EventArgs e)
        {
#if NO
            if (__bLoadReaderInfo == true)
            {
                EnterOrLeavePQR(__bLoadReaderInfo);
            }
#endif
            // 扫入 3 种条码均可
            EnterOrLeavePQR(true, InputType.ALL);
            //OpenRfidCapture(true);

            // 外观变化
            SetInputFocusState(true);
        }

        private void textBox_input_Leave(object sender, EventArgs e)
        {
            EnterOrLeavePQR(false);
            //OpenRfidCapture(false);

            // 外观变化
            SetInputFocusState(false);
        }

        public void SetInputFocusState(bool focus)
        {
            /*
            if (focus)
            {
                this.textBox_input.BackColor = SystemColors.Window;
                this.textBox_input.ForeColor = SystemColors.WindowText;
            }
            else
            {
                this.textBox_input.BackColor = Color.DarkRed;
                this.textBox_input.ForeColor = Color.White;
            }
            */
        }

        private void QuickChargingForm_Enter(object sender, EventArgs e)
        {

        }

        bool _bScrollBarTouched = false;    // 当前一轮操作中，任务列表的卷滚条是否被触动过。如果被触动过，则刷新显示和加入新对象的过程，均不要自动卷动内容

        private void dpTable_tasks_ScrollBarTouched(object sender, ScrollBarTouchedArgs e)
        {
            this._bScrollBarTouched = true;
        }

        private void dpTable_tasks_Click(object sender, EventArgs e)
        {
            this._bScrollBarTouched = true;
        }

        private void dpTable_tasks_PaintRegion(object sender, PaintRegionArgs e)
        {
            if (e.Action == "query")
            {
                e.Height = 100;
                DpCell cell = e.Item as DpCell;
                DpRow row = cell.Container;
                ChargingTask task = (ChargingTask)row.Tag;

                PatronCardInfo info = new PatronCardInfo();

                string strError = "";

                int nRet = info.SetData(task.ReaderXml, out strError);
                if (nRet == -1)
                {
                    e.Height = 0;
                    return;
                }
                using (Graphics g = Graphics.FromHwnd(this.dpTable_tasks.Handle))
                {
                    info.Layout(g,
    _cardStyle,
    e.Width,
    e.Height);
                }
                cell.Tag = info;
                return;
            }

            {
                Debug.Assert(e.Action == "paint", "");
                PatronCardInfo info = ((DpCell)e.Item).Tag as PatronCardInfo;
                if (info != null)
                {
                    Debug.Assert(info != null, "");

                    info.Paint(e.pe.Graphics,
                        e.X,
                        e.Y,
                        this._cardStyle);
                }
            }
        }

        private void contextMenuStrip_selectFunc_Opening(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                this.toolStripMenuItem_continueBorrow.Enabled = false;
            else
                this.toolStripMenuItem_continueBorrow.Enabled = true;
        }

        PatronSummaryForm _patronSummaryForm = null;

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ObjectDisposedException
Message: 无法访问已释放的对象。
对象名:“PatronSummaryForm”。
Stack:
在 System.Windows.Forms.Control.CreateHandle()
在 System.Windows.Forms.Form.CreateHandle()
在 System.Windows.Forms.Control.get_Handle()
在 System.Windows.Forms.Form.Show(IWin32Window owner)
在 dp2Circulation.QuickChargingForm.DisplayReaderSummary(ChargingTask exclude_task, String strText)


dp2Circulation 版本: dp2Circulation, Version=2.4.5735.664, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1 
操作时间 2015/9/14 16:37:15 (Mon, 14 Sep 2015 16:37:15 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 

         * */
        delegate void Delegate_DisplayReaderSummary(ChargingTask exclude_task, string strText);
        internal void DisplayReaderSummary(ChargingTask exclude_task,
            string strText)
        {
            if (this.IsDisposed)
                return;

            if (this.InvokeRequired)
            {
                Delegate_DisplayReaderSummary d = new Delegate_DisplayReaderSummary(DisplayReaderSummary);
                this.BeginInvoke(d,
                    new object[] { exclude_task, strText }
                    );
                return;
            }

            List<PatronSummary> summaries = BuildPatronSummary(exclude_task);

            if (_patronSummaryForm == null)
            {
                _patronSummaryForm = new PatronSummaryForm();
                _patronSummaryForm.FormClosed -= new FormClosedEventHandler(_patronSummaryForm_FormClosed);
                _patronSummaryForm.FormClosed += new FormClosedEventHandler(_patronSummaryForm_FormClosed);
                // _patronSummaryForm.Show(this);
            }

            _patronSummaryForm.PatronSummaries = summaries;
            _patronSummaryForm.Font = this.Font;


            if (_patronSummaryForm.Visible == false)
            {
                Program.MainForm.AppInfo.LinkFormState(this._patronSummaryForm, "_patronSummaryForm_state");
                if (_patronSummaryForm.IsDisposed)
                    return;
                _patronSummaryForm.Show(this);
            }
            else
                _patronSummaryForm.FillList();

            _patronSummaryForm.Comment = strText;
            _patronSummaryForm.ShowComment();
        }

        void _patronSummaryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_patronSummaryForm != null
                && Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.UnlinkFormState(_patronSummaryForm);
                this._patronSummaryForm = null;
            }
        }

        private void toolStripButton_openPatronSummaryWindow_Click(object sender, EventArgs e)
        {
            List<PatronSummary> summaries = BuildPatronSummary(null);

            _patronSummaryForm = new PatronSummaryForm();
            _patronSummaryForm.PatronSummaries = summaries;
            _patronSummaryForm.Font = this.Font;
            _patronSummaryForm.Show(this);
        }

        /// <summary>
        /// 打印借还凭条
        /// </summary>
        public void Print()
        {
            // 触发历史动作
            Program.MainForm.OperHistory.Print();
        }

#if NO
        private void toolStripButton_enableHanzi_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_enableHanzi.Checked == false)
                this.toolStripButton_enableHanzi.Checked = true;
            else
                this.toolStripButton_enableHanzi.Checked = false;
        }
#endif

        private void toolStripButton_enableHanzi_CheckedChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_enableHanzi.Checked == true)
            {
                this.toolStripButton_enableHanzi.Text = "中";
                this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            }
            else
            {
                this.toolStripButton_enableHanzi.Text = "英";
                this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.Disable;
            }
        }

        internal void TriggerBorrowComplete(BorrowCompleteEventArgs e)
        {
            if (this.BorrowComplete != null)
                this.BorrowComplete(this, e);
        }

        private void toolStripButton_selectItem_Click(object sender, EventArgs e)
        {
            string strItemBarcode = "";
            string strError = "";
            // return:
            //      -1  error
            //      0   放弃
            //      1   成功
            int nRet = SelectOneItem(this._funcstate,
                "",
                out strItemBarcode,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "选择册记录的过程中出错: " + strError);
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }
            if (nRet == 0)
            {
                MessageBox.Show(this, "已取消选择册记录。注意操作并未执行");
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }

            this.textBox_input.Text = strItemBarcode;
            AsyncDoAction(this.FuncState, GetUpperCase(this.textBox_input.Text));
        }

        string GetUpperCase(string strText)
        {
#if NO
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            // 除去首尾连续的空额
            // 2016/12/15
            strText = strText.Trim();

            if (this.toolStripButton_upperInput.Checked == true)
            {
                if (strText.ToLower().StartsWith("@bibliorecpath:") == true)
                    return strText; // 特殊地，不要转为大写
                return strText.ToUpper();
            }
            return strText;
#endif
            return Program.MainForm.GetUpperCase(strText);
        }

        private void toolStripButton_upperInput_CheckedChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_upperInput.Checked == true)
                this.toolStripButton_upperInput.Text = "A";
            else
                this.toolStripButton_upperInput.Text = "a";

            Program.MainForm.UpperInputBarcode = this.toolStripButton_upperInput.Checked;
        }

        void RefreshActionPicture()
        {


        }

        public Color ActionTextColor
        {
            get;
            set;
        }

        private void pictureBox_action_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            string strText = "还";

            if (_funcstate == FuncState.Borrow)
                strText = "借";
            else if (_funcstate == FuncState.ContinueBorrow)
                strText = "同借";
            else if (_funcstate == FuncState.SpecialBorrow)
                strText = "特借";
            else if (_funcstate == FuncState.Return)
                strText = "还";
            else if (_funcstate == FuncState.VerifyReturn)
                strText = "验还";
            else if (_funcstate == FuncState.Renew)
                strText = "续";
            else if (_funcstate == FuncState.VerifyRenew)
                strText = "验续";
            else if (_funcstate == FuncState.SpecialRenew)
                strText = "特续";
            else if (_funcstate == FuncState.Lost)
                strText = "丢";
            else if (_funcstate == FuncState.VerifyLost)
                strText = "验丢";
            else if (_funcstate == FuncState.LoadPatronInfo)
                strText = "人";
            else if (_funcstate == FuncState.Auto)
                strText = "自";
            else if (_funcstate == FuncState.InventoryBook)
                strText = "盘";
            else if (_funcstate == FuncState.Read)
                strText = "读";
            else if (_funcstate == FuncState.Boxing)
                strText = "配";
            else if (_funcstate == FuncState.Transfer)
                strText = "调";
            else
                strText = "?";

            int char_count = strText.Length;
            using (Font font = new System.Drawing.Font(this.Font.FontFamily, (float)this.pictureBox_action.Size.Height * (float)0.8 / (float)char_count, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                format.Alignment = StringAlignment.Center;
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                SizeF size = e.Graphics.MeasureString(strText,
                    font,
                    this.pictureBox_action.Size.Width,
                    format);

                RectangleF textRect = new RectangleF(
    (this.pictureBox_action.Size.Width - size.Width) / 2,
    (this.pictureBox_action.Size.Height - size.Height) / 2,
    size.Width,
    size.Height);
                using (Brush brush = new SolidBrush(this.ActionTextColor))
                {
                    e.Graphics.DrawString(
                        strText,
                        font,
                        brush,
                        textRect,
                        format);
                }
            }
        }

        public Control MainPanel
        {
            get
            {
                return this.splitContainer_main;
            }
        }

        // 从文件导入盘点
        private void ToolStripMenuItem_inventoryFromFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.ClearTaskList(null);

            InventoryFromFileDialog dlg = new InventoryFromFileDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.LibraryCodeList = GetOwnerLibraryCodes();
            dlg.BatchNo = this.BatchNo;
            Program.MainForm.AppInfo.LinkFormState(dlg, "InventoryFromFileDialog_state");
            dlg.ShowDialog(this.SafeWindow);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.BatchNo = dlg.BatchNo;

            int nRet = DoInventory(dlg.BarcodeFileName, out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据册条码号文件进行盘点操作
        // 调用前，要求设置好 BatchNo 和 FilterLocations。其中 FilterLocations 不是必须，如果为空则不对馆藏地进行检查
        public int DoInventory(string strBarcodeFileName,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.BatchNo) == true)
            {
                strError = "尚未给 QuickChargingForm 设置好 BatchNo 成员";
                return -1;
            }

            /*
            this.EnableControls(false);
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在进行盘点操作 ...");
            _stop.BeginLoop();
            */
            var looping = Looping("正在进行盘点操作 ...", "disableControl,halfstop");

            this.SmartFuncState = dp2Circulation.FuncState.InventoryBook;

            Encoding encoding = FileUtil.DetectTextFileEncoding(strBarcodeFileName);

            try
            {
                using (StreamReader sr = new StreamReader(strBarcodeFileName, Encoding.UTF8))
                {
                    while (true)
                    {
                        string strLine = sr.ReadLine();
                        if (strLine == null)
                            break;
                        if (string.IsNullOrEmpty(strLine) == true)
                            continue;

                        this.AsyncDoAction(this.SmartFuncState, strLine);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "读取文件 " + strBarcodeFileName + " 失败: " + ex.Message;
                return -1;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;

                this.EnableControls(true);
                */
            }
        }

        // 根据册条码号列表进行还书操作
        public int DoReturn(List<string> barcode_list,
            out string strError)
        {
            strError = "";

            /*
            this.EnableControls(false);
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在进行还书操作 ...");
            _stop.BeginLoop();
            */
            var looping = Looping("正在进行还书操作 ...", "disableControl,halfstop");

            this.SmartFuncState = dp2Circulation.FuncState.Return;

            try
            {
                foreach (string barcode in barcode_list)
                {
                    if (string.IsNullOrEmpty(barcode) == true)
                        continue;

                    this.AsyncDoAction(this.SmartFuncState, barcode);
                }
            }
            catch (Exception ex)
            {
                strError = "还书操作中出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;

                this.EnableControls(true);
                */
            }

            return 0;
        }

        // 模拟预约到书
        private void toolStripMenuItem_test_simulateReservationArrive_Click(object sender, EventArgs e)
        {
            if (this.FuncState == dp2Circulation.FuncState.Return
                || this.FuncState == dp2Circulation.FuncState.VerifyReturn
                || this.FuncState == dp2Circulation.FuncState.Lost
                || this.FuncState == dp2Circulation.FuncState.VerifyLost)
                AsyncDoAction(this.FuncState,
                    GetUpperCase(this.textBox_input.Text),
                    "",
                    "simulate_reservation_arrive");
            else
                MessageBox.Show(this, "此功能必须和还书、丢失功能配套使用");
        }

        private void ToolStripMenuItem_boxing_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Boxing;
        }

        private void ToolStripMenuItem_move_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Transfer;
        }

        string _targetLocation = "";

        // 选择调拨去向
        private void toolStripButton_selectTransferTargetLocation_Click(object sender, EventArgs e)
        {
            // 选择目标馆藏地的对话框
            // 须是当前操作者能管辖的分馆内的馆藏地
            /*
            REDO:
            var result = InputDlg.GetInput(this, "title",
                "目标馆藏地", "", this.Font);
            if (result == null)
                return;

            if (string.IsNullOrEmpty(result))
                goto REDO;

            this._targetLocation = result; 
            */
            using (SelectLocationDialog dlg = new SelectLocationDialog())
            {
                dlg.Text = "请选择调拨目标馆藏地";
                dlg.SelectedLocation = this._targetLocation;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BatchNo = this.BatchNo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                this._targetLocation = dlg.SelectedLocation;
                this.BatchNo = dlg.BatchNo;

                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
            }
        }

        void EnableControlsForFace(bool enable)
        {
            this.Invoke((Action)(() =>
            {
                this.textBox_input.Enabled = enable;
                this.toolStrip_main.Enabled = enable;
            }));
        }

        // 人脸识别
        private async void toolStripButton_faceInput_Click(object sender, EventArgs e)
        {
            RecognitionFaceResult result = null;
            EnableControlsForFace(false);
            try
            {
                NormalResult getstate_result = await FaceGetStateAsync("getLibraryServerUID");
                if (getstate_result.Value == -1)
                    result = new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorInfo = getstate_result.ErrorInfo
                    };
                else if (getstate_result.ErrorCode != Program.MainForm.ServerUID)
                    result = new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorInfo = $"人脸中心所连接的 dp2library 服务器 UID {getstate_result.ErrorCode} 和内务当前所连接的 UID {Program.MainForm.ServerUID} 不同。无法进行人脸识别"
                    };
                else
                    result = await RecognitionFace("ui");
            }
            finally
            {
                EnableControlsForFace(true);
            }
            this.Invoke((Action)(() =>
            {
                // 2019/6/13
                this.Activate();
                API.SetForegroundWindow(this.Handle);

                if (result.Value == 1)
                {
                    this.textBox_input.Text = result.Patron;
                    this.textBox_input.Focus();
                    // 触发回车
                    DoEnter();
                }
                else
                {
                    MessageBox.Show(this, result.ErrorInfo);
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                }
            }));
        }

        EasForm _easForm = null;

        private void ToolStripMenuItem_openEasForm_Click(object sender, EventArgs e)
        {
            InitialEasForm();
            ShowEasForm(true);
        }

        void InitialEasForm()
        {
            if (_easForm == null)
            {
                _easForm = new EasForm();
                _easForm.Font = this.Font;
                _easForm.FormClosed += (sender, e) =>
                {
                    _easForm.Dispose();
                    _easForm = null;
                };
                _easForm.EasChanged += (sender, e) =>
                {
                    ChargingTask task = e.Param as ChargingTask;
                    if (task != null)
                    {
                        task.Color = "green";
                        {
                            DpRow line = FindTaskLine(task);
                            if (line != null)
                                line.BackColor = this.TaskBackColor;
                        }
                        task.State = "finish";
                        task.ErrorInfo = "";
                        // task.ErrorInfo = "\r\nEAS 修正成功";
                        this.DisplayTask("refresh_and_visible", task);
                        this.SetColorList();
                    }
                };

                _easForm.Show(this);
            }
        }

        void ShowEasForm(bool show)
        {
            /*
            if (show)
            {
                if (_easForm.IsHandleCreated)
                    _easForm.Visible = true;
                else
                    _easForm.Show(this);
            }
            else
                _easForm.Visible = false;
                */

            _easForm.Visible = show;
            // Program.MainForm.Activate();
        }

        void DestroyEasForm()
        {
            if (_easForm != null)
            {
                _easForm.CloseFloatingMessage();
                _easForm.Close();
                //_easForm.Dispose();
                //_easForm = null;
            }
        }

        private void ToolStripMenuItem_rfid_restartRfidCenter_Click(object sender, EventArgs e)
        {
            var result = RfidManager.GetState("restart");
            if (result.Value == -1)
                this.ShowMessage($"重启 RFID 中心时出错: {result.ErrorInfo}", "red", true);
            else
                this.ShowMessageAutoClear("RFID 中心已经重启", "green", 5000, true);
        }

        // 是否处于“测试同步”状态。在此状态下，Borrow() 和 Return() 请求前，会弹出对话框要求操作者输入 operTime 子参数
        public bool TestSync
        {
            get
            {
                return (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.ToolStripMenuItem_testSync.Checked;
                }));
            }
        }

        public string GetDefaultOperTimeParamString()
        {
            return DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now);
        }

        DateTime _lastInputTime = DateTime.Now;

        public string GetOperTimeParamString()
        {
            if (this.TestSync == false)
                return "";
            REDO:
            string value = (string)this.Invoke(new Func<string>(() =>
            {
                return InputDlg.GetInput(this,
                "测试同步",
                "请输入实际操作时间(格式：'2020-1-1 08:01:55')",
                _lastInputTime.ToString("yyyy-MM-dd HH:mm:ss"),
                this.Font);
            }));
            if (value == null)
                return "";
            if (DateTime.TryParse(value, out DateTime time) == false)
            {
                MessageBox.Show(this, $"时间字符串 '{value}' 不合法。请重新输入");
                goto REDO;
            }
            _lastInputTime = time;
            return DateTimeUtil.Rfc1123DateTimeStringEx(time);
        }

        // 多册 还书
        private void ToolStripMenuItem_multiReturn_Click(object sender, EventArgs e)
        {
            // 检查 dp2library 版本
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.84") < 0)
            {
                MessageBox.Show(this, "本功能需要 dp2library 版本在 3.84 以上");
                return;
            }

            // 检查前端权限
            if (StringUtil.IsInList("client_multiplecharging", this.CurrentRights) == false)
            {
                MessageBox.Show(this, "当前用户不具备 client_multiplecharging 权限，无法进行复选还书的操作");
                return;
            }

            string list = this.webBrowser_reader.Document.InvokeScript("getSelectedBarcodes") as string;

            DialogResult result = MessageBox.Show(this,
$"确实要对下列册进行还书操作?\r\n\r\n{list}\r\n请仔细核对上述册条码号",
"QuickChargingForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            // MessageBox.Show(this, $"result='{result}'");
            var barcodes = StringUtil.SplitList(list);
            MultipleOperate(barcodes, FuncState.Return);
        }

        // 多册 续借
        private void ToolStripMenuItem_multiRenew_Click(object sender, EventArgs e)
        {
            // 检查 dp2library 版本
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.84") < 0)
            {
                MessageBox.Show(this, "本功能需要 dp2library 版本在 3.84 以上");
                return;
            }

            // 检查前端权限
            if (StringUtil.IsInList("client_multiplecharging", this.CurrentRights) == false)
            {
                MessageBox.Show(this, "当前用户不具备 client_multiplecharging 权限，无法进行复选续借的操作");
                return;
            }

            string list = this.webBrowser_reader.Document.InvokeScript("getSelectedBarcodes") as string;

            DialogResult result = MessageBox.Show(this,
$"确实要对下列册进行 续借 操作?\r\n\r\n{list}\r\n请仔细核对上述册条码号",
"QuickChargingForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            var barcodes = StringUtil.SplitList(list);
            MultipleOperate(barcodes, FuncState.Renew);
        }

        void MultipleOperate(List<string> barcodes, FuncState func)
        {
            // 先切换到这个功能状态，避免用户困惑
            if (this.FuncState != func)
                this.FuncState = func;
            foreach (var text in barcodes)
            {
                this.Invoke((Action)(() =>
                {
                    this.textBox_input.Text = text;
                }));

                AsyncDoAction(func, text, "", "comment:当前册是从内务快捷出纳窗界面勾选的，图书实物未经扫入册条码验证");
            }
        }

        // 2021/10/9
        // 执行一个功能
        public void Operate(FuncState func,
            string text,
            string comment)
        {
            // 先切换到这个功能状态，避免用户困惑
            if (this.FuncState != func)
                this.FuncState = func;

            this.Invoke((Action)(() =>
            {
                this.textBox_input.Text = text;
            }));

            AsyncDoAction(func, text, "", string.IsNullOrEmpty(comment) ? "" : $"comment:{comment}");
        }

        private void toolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            string list = this.webBrowser_reader.Document.InvokeScript("getSelectedBarcodes") as string;
            this.ToolStripMenuItem_multipleItem.Enabled = !string.IsNullOrEmpty(list);
        }

        private void toolStripMenuItem_specialBorrow_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.SpecialBorrow,
true,
false);
        }

        // 多册 特殊续借
        private void ToolStripMenuItem_special_multiRenew_Click(object sender, EventArgs e)
        {
            // 检查 dp2library 版本
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.84") < 0)
            {
                MessageBox.Show(this, "本功能需要 dp2library 版本在 3.84 以上");
                return;
            }

            // 检查前端权限
            if (StringUtil.IsInList("client_multiplecharging", this.CurrentRights) == false)
            {
                MessageBox.Show(this, "当前用户不具备 client_multiplecharging 权限，无法进行复选特殊续借的操作");
                return;
            }

            string list = this.webBrowser_reader.Document.InvokeScript("getSelectedBarcodes") as string;

            // TODO: 用 MessageDlg 改写，允许 list 内容很多的时候卷滚显示
            DialogResult result = MessageBox.Show(this,
$"确实要对下列册进行 特殊续借 操作?\r\n\r\n{list}\r\n请仔细核对上述册条码号",
"QuickChargingForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            var barcodes = StringUtil.SplitList(list);
            MultipleOperate(barcodes, FuncState.SpecialRenew);

        }

        private void toolStripMenuItem_specialRenew_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.SpecialRenew,
true,
false);
        }
    }

    /// <summary>
    /// 借书还书操作完成事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void BorrowCompleteEventHandler(object sender,
        BorrowCompleteEventArgs e);

    /// <summary>
    /// 借书还书操作完成事件的参数
    /// </summary>
    public class BorrowCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action = "";
        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = "";
        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode = "";
    }





}
