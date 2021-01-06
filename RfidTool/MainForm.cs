﻿using System;
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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Core;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace RfidTool
{
    public partial class MainForm : Form
    {
        ScanDialog _scanDialog = null;

        ErrorTable _errorTable = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

        bool _historyChanged = false;

        #region floating message

        internal FloatingMessageForm _floatingMessage = null;

        public FloatingMessageForm FloatingMessageForm
        {
            get
            {
                return this._floatingMessage;
            }
            set
            {
                this._floatingMessage = value;
            }
        }

        public void ShowMessageAutoClear(string strMessage,
string strColor = "",
int delay = 2000,
bool bClickClose = false)
        {
            _ = Task.Run(() =>
            {
                _showMessage(strMessage,
    strColor,
    bClickClose);
                System.Threading.Thread.Sleep(delay);
                // 中间一直没有变化才去消除它
                if (_floatingMessage.Text == strMessage)
                    _clearMessage();
            });
        }

        public void ShowMessage(string message)
        {
            _errorTable.SetError("message", message, false);
        }

        public void ShowErrorMessage(string type, string message)
        {
            _errorTable.SetError(type, message, false);
        }

        public void _showMessage(string strMessage,
    string strColor = "",
    bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        // 线程安全
        public void _clearMessage()
        {
            if (this._floatingMessage == null)
                return;

            this._floatingMessage.Text = "";
        }

        #endregion

        public MainForm()
        {
            ClientInfo.ProgramName = "rfidtool";
            FormClientInfo.MainForm = this;

            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.AutoHide = false;
                _floatingMessage.Show(this);

                this.Move += (s1, o1) =>
                {
                    if (this._floatingMessage != null)
                        this._floatingMessage.OnResizeOrMove();
                };
            }

            DataModel.SetError += DataModel_SetError;

            _errorTable = new ErrorTable((s) =>
            {
                try
                {
                    this.Invoke((Action)(() =>
                    {
                        bool error = _errorTable.GetError("error") != null || _errorTable.GetError("error_initial") != null;
                        if (string.IsNullOrEmpty(s) == false)
                        {
                            string text = s.Replace(";", "\r\n");
                            if (text != this._floatingMessage.Text)
                            {
                                if (error)
                                    this._showMessage(text, "red", false);
                                else
                                    this._showMessage(text);

                            // ClientInfo.WriteErrorLog(text);
                        }
                        }
                        else
                            this._clearMessage();
                    }));
                }
                catch(ObjectDisposedException)
                {

                }
            });

            UsbInfo.StartWatch((add_count, remove_count) =>
            {
                // this.OutputHistory($"add_count:{add_count}, remove_count:{remove_count}", 1);
                string type = "disconnected";
                if (add_count > 0)
                    type = "connected";

                BeginRefreshReaders(type, new CancellationToken());
            },
_cancel.Token);
        }

        int _refreshCount = 0;
        const int _delaySeconds = 5;
        Task _refreshTask = null;

        public void BeginRefreshReaders(string action,
            CancellationToken token)
        {
            if (_refreshTask != null)
            {
                if (action == "disconnected")
                {
                    if (_refreshCount < 1)
                        _refreshCount++;
                }
                else
                    _refreshCount++;
                return;
            }

            // _refreshCount = 2;
            _refreshTask = Task.Run(() =>
            {
                try
                {
                    while (_refreshCount-- >= 0)
                    {
                        Task.Delay(TimeSpan.FromSeconds(_delaySeconds)).Wait(token);
                        if (token.IsCancellationRequested)
                            break;
                        // 迫使重新启动
                        BeginConnectReader("正在重新连接读卡器 ...", false);

                        if (token.IsCancellationRequested)
                            break;

                        break;
                        /*
                        // 如果初始化没有成功，则要追加初始化
                        if (this.ErrorState == "normal")
                            break;
                        */
                    }
                    _refreshTask = null;
                    _refreshCount = 0;
                }
                catch
                {
                    _refreshTask = null;
                    _refreshCount = 0;
                }
            });
        }

        private void DataModel_SetError(object sender, SetErrorEventArgs e)
        {
            _errorTable.SetError("error", e.Error, false);
        }

        void CreateScanDialog()
        {
            if (_scanDialog == null)
            {
                _scanDialog = new ScanDialog();

                _scanDialog.FormClosing += _scanDialog_FormClosing;
                _scanDialog.WriteComplete += _scanDialog_WriteComplete;

                GuiUtil.SetControlFont(_scanDialog, this.Font);
                ClientInfo.MemoryState(_scanDialog, "scanDialog", "state");
                _scanDialog.UiState = ClientInfo.Config.Get("scanDialog", "uiState", null);
            }
        }

        private void _scanDialog_WriteComplete(object sender, WriteCompleteventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                AppendItem(e.Chip, e.TagInfo);
                _historyChanged = true;
            }));
        }

        private void _scanDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dialog = sender as Form;

            // 将关闭改为隐藏
            dialog.Visible = false;
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FormClientInfo.SerialNumberMode = "must";
            var ret = FormClientInfo.Initial("rfidtool",
                () => StringUtil.IsDevelopMode());
            if (ret == false)
            {
                Application.Exit();
                return;
            }

            Storeage.Initialize();

            LoadSettings();

            BeginConnectReader("正在连接 RFID 读写器 ...");

            _ = Task.Run(() =>
            {
                LoadHistory();
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel?.Cancel();

            {
                if (_scanDialog != null)
                    ClientInfo.Config.Set("scanDialog", "uiState", _scanDialog.UiState);
                _scanDialog?.Close();
                _scanDialog?.Dispose();
                _scanDialog = null;
            }

            this.ShowMessage("正在退出 ...");

            DataModel.SetError -= DataModel_SetError;
            DataModel.ReleaseDriver();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Dispose();

            SaveSettings();

            if (_historyChanged)
                SaveHistory();
            Storeage.Finish();
        }

        void LoadSettings()
        {
            this.UiState = ClientInfo.Config.Get("global", "ui_state", "");

            // 恢复 MainForm 的显示状态
            {
                var state = ClientInfo.Config.Get("mainForm", "state", "");
                if (string.IsNullOrEmpty(state) == false)
                {
                    FormProperty.SetProperty(state, this, ClientInfo.IsMinimizeMode());
                }
            }

        }

        void SaveSettings()
        {
            // 保存 MainForm 的显示状态
            {
                var state = FormProperty.GetProperty(this);
                ClientInfo.Config.Set("mainForm", "state", state);
            }

            ClientInfo.Config?.Set("global", "ui_state", this.UiState);
            ClientInfo.Finish();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.tabControl1,
                    this.listView_writeHistory,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl1,
                    this.listView_writeHistory,
                };
                //_inSetUiState++;
                try
                {
                    GuiState.SetUiState(controls, value);
                }
                finally
                {
                    //_inSetUiState--;
                }
            }
        }

        const int COLUMN_UID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_TOU = 2;
        const int COLUMN_OI = 3;
        const int COLUMN_AOI = 4;
        const int COLUMN_WRITETIME = 5;

        public void AppendItem(LogicChip chip,
            TagInfo tagInfo)
        {
            ListViewItem item = new ListViewItem();
            this.listView_writeHistory.Items.Add(item);
            item.EnsureVisible();
            ListViewUtil.ChangeItemText(item, COLUMN_UID, tagInfo.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_PII, chip.FindElement(ElementOID.PII)?.Text);
            ListViewUtil.ChangeItemText(item, COLUMN_TOU, chip.FindElement(ElementOID.TypeOfUsage)?.Text);
            ListViewUtil.ChangeItemText(item, COLUMN_OI, chip.FindElement(ElementOID.OI)?.Text);
            ListViewUtil.ChangeItemText(item, COLUMN_AOI, chip.FindElement(ElementOID.AOI)?.Text);
            ListViewUtil.ChangeItemText(item, COLUMN_WRITETIME, DateTime.Now.ToString());
        }

        // 导出选择的行到 Excel 文件
        private void MenuItem_saveToExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_writeHistory.Items)
            {
                items.Add(item);
            }

            this.ShowMessage("正在导出选定的事项到 Excel 文件 ...");

            this.EnableControls(false);
            try
            {
                int nRet = ClosedXmlUtil.ExportToExcel(
                    null,
                    items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
                this.ShowMessage(null);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void EnableControls(bool enable)
        {
            this.listView_writeHistory.Enabled = enable;
        }

        // 写入层架标
        private void MenuItem_writeShelfTags_Click(object sender, EventArgs e)
        {
            // 把扫描对话框打开
            CreateScanDialog();

            _scanDialog.TypeOfUsage = "30"; // 层架标
            if (_scanDialog.Visible == false)
                _scanDialog.Show(this);
        }

        // 开始(扫描并)写入图书标签
        private void MenuItem_writeBookTags_Click(object sender, EventArgs e)
        {
            // 把扫描对话框打开
            CreateScanDialog();

            _scanDialog.TypeOfUsage = "10"; // 图书
            if (_scanDialog.Visible == false)
                _scanDialog.Show(this);
        }

        // 设置
        private void MenuItem_settings_Click(object sender, EventArgs e)
        {
            using (SettingDialog dlg = new SettingDialog())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "settingDialog", "state");

                dlg.ShowDialog(this);
            }
        }

        // 退出
        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 写入读者证件
        private void MenuItem_writePatronTags_Click(object sender, EventArgs e)
        {
            // 把扫描对话框打开
            CreateScanDialog();

            _scanDialog.TypeOfUsage = "80"; // 读者
            if (_scanDialog.Visible == false)
                _scanDialog.Show(this);
        }

        // 关于
        private void MenuItem_about_Click(object sender, EventArgs e)
        {
            var text = $"RFID 工具 (版本号: {ClientInfo.ClientVersion})\r\n数字平台(北京)软件有限责任公司\r\nhttp://dp2003.com\r\n\r\n\r\n当前可用读写器:\r\n{StringUtil.MakePathList(DataModel.GetReadNameList(), "\r\n")}";
            MessageDlg.Show(this, text, "关于");
        }

        // 重新连接读写器
        private void MenuItem_reconnectReader_Click(object sender, EventArgs e)
        {
            BeginConnectReader("正在重新连接 RFID 读写器 ...");
        }

        Task _taskConnect = null;

        // 连接读写器
        void BeginConnectReader(string message,
            bool reset_hint_table = false)
        {
            // 避免重入
            if (_taskConnect != null)
                return;

            _taskConnect = Task.Run(() =>
            {
                try
                {
                REDO:
                    this.ShowErrorMessage("error_initial", null);
                    this.ShowMessage(message);
                    DataModel.ReleaseDriver();
                    var result = DataModel.InitialDriver(reset_hint_table);

                    /*
                    // testing
                    result.Value = -1;
                    result.ErrorInfo = "test";
                    */

                    if (result.Value == -1)
                    {
                        bool check = false;
                        var dlg_result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
                        {
                            return MessageDlg.Show(this,
                                $"连接读写器失败: {result.ErrorInfo}。\r\n\r\n是否重新探测?",
                                "连接读写器",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxDefaultButton.Button1,
                                ref check,
                                new string[] { "重新探测", "重试连接", "取消" });
                        }));

                        if (dlg_result == DialogResult.Yes)
                        {
                            reset_hint_table = true;
                            goto REDO;
                        }
                        if (dlg_result == DialogResult.No)
                        {
                            reset_hint_table = false;
                            goto REDO;
                        }
                        this.ShowErrorMessage("error_initial", $"连接读写器失败: {result.ErrorInfo}");
                    }
                    else
                    {
                        this.ShowMessage(null);

                        var count = DataModel.GetReadNameList().Count;
                        this.Invoke((Action)(() =>
                        {
                            this.StatusReaderCount = $"读写器: {count}";
                        }));

                        if (count == 0)
                        {
                            this.ShowErrorMessage("error_initial", $"当前没有连接任何读写器");
                        }
                        else
                        {
                            this.ShowErrorMessage("error_initial", null);
                        }
                    }
                }
                finally
                {
                    _taskConnect = null;
                }
            });
        }

        // 重新探测读写器
        private void MenuItem_resetConnectReader_Click(object sender, EventArgs e)
        {
            /*
            this.ShowMessage("正在重新探测 RFID 读写器");
            _ = Task.Run(() =>
            {
                DataModel.InitialDriver(true);
                this.ClearMessage();
            });
            */
            BeginConnectReader("正在重新探测 RFID 读写器\r\n\r\n时间可能稍长，请耐心等待 ...", true);
        }

        public string StatusMessage
        {
            get
            {
                return this.toolStripStatusLabel_message.Text;
            }
            set
            {
                this.toolStripStatusLabel_message.Text = value;
            }
        }

        // 状态行中读写器数量显示
        public string StatusReaderCount
        {
            get
            {
                return this.toolStripStatusLabel_readerCount.Text;
            }
            set
            {
                this.toolStripStatusLabel_readerCount.Text = value;
            }
        }

        void LoadHistory()
        {
            var items = Storeage.LoadItems();
            this.Invoke((Action)(() =>
            {
                foreach (var history in items)
                {
                    ListViewItem item = new ListViewItem();
                    this.listView_writeHistory.Items.Add(item);
                    item.EnsureVisible();
                    ListViewUtil.ChangeItemText(item, COLUMN_UID, history.UID);
                    ListViewUtil.ChangeItemText(item, COLUMN_PII, history.PII);
                    ListViewUtil.ChangeItemText(item, COLUMN_TOU, history.TOU);
                    ListViewUtil.ChangeItemText(item, COLUMN_OI, history.OI);
                    ListViewUtil.ChangeItemText(item, COLUMN_AOI, history.AOI);
                    ListViewUtil.ChangeItemText(item, COLUMN_WRITETIME, history.WriteTime);
                }
            }));
        }

        void SaveHistory()
        {
            List<HistoryItem> items = new List<HistoryItem>();
            int i = 0;
            foreach (ListViewItem item in this.listView_writeHistory.Items)
            {
                HistoryItem history = new HistoryItem();
                history.Id = i + 1;
                history.UID = ListViewUtil.GetItemText(item, COLUMN_UID);
                history.PII = ListViewUtil.GetItemText(item, COLUMN_PII);
                history.TOU = ListViewUtil.GetItemText(item, COLUMN_TOU);
                history.OI = ListViewUtil.GetItemText(item, COLUMN_OI);
                history.AOI = ListViewUtil.GetItemText(item, COLUMN_AOI);
                history.WriteTime = ListViewUtil.GetItemText(item, COLUMN_WRITETIME);
                items.Add(history);
            }

            Storeage.SaveItems(items);
        }

        private void MenuItem_userManual_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/DigitalPlatform/dp2/issues/764";

            Process.Start(url);
        }

        private void MenuItem_resetSerialCode_Click(object sender, EventArgs e)
        {
            // return:
            //      -1  出错
            //      0   正确
            int nRet = FormClientInfo.VerifySerialCode(
                "", // strTitle,
                "", // strRequirFuncList,
                "reset",
                out string strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_openUserFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(ClientInfo.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(ClientInfo.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_openProgramFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_clearHistory_all_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
$"确实要清除全部 {this.listView_writeHistory.Items.Count} 个历史事项?",
"RfidTool",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;
            this.listView_writeHistory.Items.Clear();
            _historyChanged = true;
        }

        private void MenuItem_clearHistory_selected_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
$"确实要清除所选的 {this.listView_writeHistory.SelectedItems.Count} 个历史事项?",
"RfidTool",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;
            foreach(ListViewItem item in this.listView_writeHistory.SelectedItems)
            {
                this.listView_writeHistory.Items.Remove(item);
            }
            _historyChanged = true;
        }

        private void MenuItem_clearHistory_DropDownOpening(object sender, EventArgs e)
        {
            this.MenuItem_clearHistory_all.Text = $"清除全部 {this.listView_writeHistory.Items.Count} 个事项(&A)";
            this.MenuItem_clearHistory_all.Enabled = this.listView_writeHistory.Items.Count > 0;

            this.MenuItem_clearHistory_selected.Text = $"清除所选 {this.listView_writeHistory.SelectedItems.Count} 个事项(&S)";
            this.MenuItem_clearHistory_selected.Enabled = this.listView_writeHistory.SelectedItems.Count > 0;
        }
    }

    public delegate void WriteCompleteEventHandler(object sender,
WriteCompleteventArgs e);

    /// <summary>
    /// 写入成功事件的参数
    /// </summary>
    public class WriteCompleteventArgs : EventArgs
    {
        public LogicChip Chip { get; set; }
        public TagInfo TagInfo { get; set; }
    }
}
