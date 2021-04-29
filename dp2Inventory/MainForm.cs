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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Core;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using static dp2Inventory.InventoryData;

namespace dp2Inventory
{
    public partial class MainForm : Form
    {
        ErrorTable _errorTable = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

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
            ClientInfo.ProgramName = "dp2inventory";
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

            // DataModel.SetError += DataModel_SetError;

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
                catch (ObjectDisposedException)
                {

                }
            });
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // 2021/4/25
            ClientInfo.BeginUpdate(
    TimeSpan.FromMinutes(2),
    TimeSpan.FromMinutes(60),
    _cancel.Token,
    (text, level) =>
    {
        //      warning_level   警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        ShowStatusText(text);
    });

            // FormClientInfo.SerialNumberMode = "must";
            var ret = FormClientInfo.Initial("dp2inventory",
                () => StringUtil.IsDevelopMode());
            if (ret == false)
            {
                Application.Exit();
                return;
            }

            LoadSettings();

            RefreshMenuState();

            Storeage.Initialize();

            _ = Task.Run(() =>
            {
                StartProcessManager();
            });

            List<string> errors = new List<string>();
            this._showMessage("正在初始化窗口内容，请稍候 ...");
            this.EnableControls(false);
            try
            {
                var task1 = Task.Run(() =>
                {
                    LoadHistory();
                    // await Task.Delay(TimeSpan.FromSeconds(5));
                });

                var task2 = Task.Run(() =>
                {
                    var initial_result = LibraryChannelUtil.Initial();
                    if (initial_result.Value == -1)
                    {
                        errors.Add($"获得 dp2library 服务器配置失败: {initial_result.ErrorInfo}");
                        return;
                    }
                });

                await Task.WhenAll(new Task[] { task1, task2 });
            }
            finally
            {
                this.EnableControls(true);
                if (errors.Count > 0)
                    this.ShowMessage(StringUtil.MakePathList(errors, "\r\n"));
                else
                    this._clearMessage();
            }

        }

        void ShowStatusText(string text)
        {
            this.Invoke((Action)(() =>
            {
                toolStripStatusLabel_message.Text = text;
            }));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel?.Cancel();

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

        private void MenuItem_settings_Click(object sender, EventArgs e)
        {
            OpenSettingDialog(this);
        }

        public void OpenSettingDialog(Form parent,
    string style = "")
        {
            using (SettingDialog dlg = new SettingDialog())
            {
                var oldUrl = DataModel.RfidCenterUrl;

                GuiUtil.SetControlFont(dlg, parent.Font);
                ClientInfo.MemoryState(dlg, "settingDialog", "state");
                //dlg.OpenStyle = style;
                dlg.ShowDialog(parent);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    // 更新菜单状态
                    RefreshMenuState();

                    if (oldUrl != DataModel.RfidCenterUrl)
                        StartProcessManager();

                    InventoryData.ClearVarcodeValidator();

                    //    DataModel.TagList.EnableTagCache = DataModel.EnableTagCache;
                }
            }
        }

        // 刷新菜单状态
        void RefreshMenuState()
        {
            if (DataModel.Protocol == "sip")
            {
                MenuItem_clearUidUiiTable.Enabled = true;
                MenuItem_exportLocalItemsToExcel.Enabled = DataModel.sipLocalStore;
            }
            else
            {
                MenuItem_clearUidUiiTable.Enabled = false;
                MenuItem_exportLocalItemsToExcel.Enabled = false;
            }
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 盘点
        private void MenuItem_inventory_Click(object sender, EventArgs e)
        {
            // 把盘点对话框打开
            CreateInventoryDialog();

            if (_inventoryDialog.Visible == false)
                _inventoryDialog.Show(this);
            else if (_inventoryDialog.WindowState == FormWindowState.Minimized)
                _inventoryDialog.WindowState = FormWindowState.Normal;
        }

        InventoryDialog _inventoryDialog = null;

        void CreateInventoryDialog()
        {
            if (_inventoryDialog == null)
            {
                _inventoryDialog = new InventoryDialog();

                _inventoryDialog.FormClosing += _inventoryDialog_FormClosing;
                _inventoryDialog.WriteComplete += _inventoryDialog_WriteComplete;
                _inventoryDialog.AddBook += _inventoryDialog_AddBook;
                
                GuiUtil.SetControlFont(_inventoryDialog, this.Font);
                ClientInfo.MemoryState(_inventoryDialog, "inventoryDialog", "state");
                _inventoryDialog.UiState = ClientInfo.Config.Get("inventoryDialog", "uiState", null);
            }
        }

        private void _inventoryDialog_AddBook(object sender, AddBookEventArgs e)
        {
            // TODO: 如果窗口尚未创建，则可以先加入一个临时 list，等窗口创建后再集中初始化
            _shelfDialog?.AddBook(e);
        }

        bool _historyChanged = false;

        private void _inventoryDialog_WriteComplete(object sender, WriteCompleteventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                AppendItem(e.Info, e.Action);
                _historyChanged = true;
            }));
        }

        private void _inventoryDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dialog = sender as Form;

            // 将关闭改为隐藏
            dialog.Visible = false;
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        #region 操作历史

        const int COLUMN_UID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_TITLE = 2;
        const int COLUMN_CURRENTLOCATION = 3;
        const int COLUMN_LOCATION = 4;
        const int COLUMN_STATE = 5;
        const int COLUMN_TOU = 6;
        const int COLUMN_OI = 7;
        const int COLUMN_WRITETIME = 8;
        const int COLUMN_ACTION = 9;
        const int COLUMN_BATCHNO = 10;

        /*
        const int COLUMN_UID = 0;
        const int COLUMN_ERRORINFO = 1;
        const int COLUMN_PII = 2;
        const int COLUMN_TITLE = 3;
        const int COLUMN_CURRENTLOCATION = 4;
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
         * */

        public void AppendItem(ProcessInfo info,
            string action)
        {
            ListViewItem item = new ListViewItem();
            this.listView_writeHistory.Items.Insert(0, item);
            item.EnsureVisible();

            var entity = info.Entity;
            ListViewUtil.ChangeItemText(item, COLUMN_UID, entity.UID);
            ListViewUtil.ChangeItemText(item, COLUMN_PII, entity.PII);
            ListViewUtil.ChangeItemText(item, COLUMN_TITLE, entity.Title);
            ListViewUtil.ChangeItemText(item, COLUMN_CURRENTLOCATION, entity.CurrentLocation);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, entity.Location + ":" + entity.ShelfNo);
            ListViewUtil.ChangeItemText(item, COLUMN_STATE, entity.State);
            ListViewUtil.ChangeItemText(item, COLUMN_TOU, info.IsLocation ? "层架标" : "图书");
            ListViewUtil.ChangeItemText(item, COLUMN_OI, entity.GetOiOrAoi());
            ListViewUtil.ChangeItemText(item, COLUMN_WRITETIME, DateTime.Now.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_ACTION, action);
            ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, info.BatchNo);
        }

        #endregion

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

        private void MenuItem_userManual_Click(object sender, EventArgs e)
        {
            /*
            string url = "https://github.com/DigitalPlatform/dp2/issues/764";

            Process.Start(url);
            */
        }

        private void MenuItem_resetSerialCode_Click(object sender, EventArgs e)
        {
            /*
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
            */
        }

        private void MenuItem_about_Click(object sender, EventArgs e)
        {
            var text = $"dp2Inventory 盘点 (版本号: {ClientInfo.ClientVersion})\r\n数字平台(北京)软件有限责任公司\r\nhttp://dp2003.com\r\n\r\n\r\n";
            MessageDlg.Show(this, text, "关于");
        }

        private void MenuItem_saveHistoryToExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_writeHistory.Items)
            {
                items.Add(item);
            }

            if (items.Count == 0)
            {
                strError = "没有可供保存的盘点历史事项";
                goto ERROR1;
            }

            this.ShowMessage("正在导出全部盘点历史事项到 Excel 文件 ...");

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
            this.menuStrip1.Enabled = enable;
            this.toolStrip1.Enabled = enable;
        }

        private void MenuItem_clearHistory_all_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
$"确实要清除全部 {this.listView_writeHistory.Items.Count} 个历史事项?",
"dp2Inventory",
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
"dp2Inventory",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;
            foreach (ListViewItem item in this.listView_writeHistory.SelectedItems)
            {
                this.listView_writeHistory.Items.Remove(item);
            }
            _historyChanged = true;
        }

        void LoadHistory()
        {
            var items = Storeage.LoadItems();
            this.Invoke((Action)(() =>
            {
                this.listView_writeHistory.BeginUpdate();
                foreach (var history in items)
                {
                    ListViewItem item = new ListViewItem();
                    this.listView_writeHistory.Items.Add(item);
                    item.EnsureVisible();
                    ListViewUtil.ChangeItemText(item, COLUMN_UID, history.UID);
                    ListViewUtil.ChangeItemText(item, COLUMN_PII, history.PII);
                    ListViewUtil.ChangeItemText(item, COLUMN_TITLE, history.Title);
                    ListViewUtil.ChangeItemText(item, COLUMN_CURRENTLOCATION, history.CurrentLocation);
                    ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, history.Location);
                    ListViewUtil.ChangeItemText(item, COLUMN_STATE, history.State);

                    ListViewUtil.ChangeItemText(item, COLUMN_TOU, history.TOU);
                    ListViewUtil.ChangeItemText(item, COLUMN_OI, history.OI);
                    ListViewUtil.ChangeItemText(item, COLUMN_WRITETIME, history.WriteTime);
                    ListViewUtil.ChangeItemText(item, COLUMN_ACTION, history.Action);
                    ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, history.BatchNo);
                }
                this.listView_writeHistory.EndUpdate();
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
                history.Title = ListViewUtil.GetItemText(item, COLUMN_TITLE);
                history.CurrentLocation = ListViewUtil.GetItemText(item, COLUMN_CURRENTLOCATION);
                history.Location = ListViewUtil.GetItemText(item, COLUMN_LOCATION);
                history.State = ListViewUtil.GetItemText(item, COLUMN_STATE);
                history.TOU = ListViewUtil.GetItemText(item, COLUMN_TOU);
                history.OI = ListViewUtil.GetItemText(item, COLUMN_OI);
                history.WriteTime = ListViewUtil.GetItemText(item, COLUMN_WRITETIME);
                history.Action = ListViewUtil.GetItemText(item, COLUMN_ACTION);
                history.BatchNo = ListViewUtil.GetItemText(item, COLUMN_BATCHNO);
                items.Add(history);
            }

            Storeage.SaveItems(items);
        }

        private async void MenuItem_exportLocalItemsToExcel_Click(object sender, EventArgs e)
        {
            List<InventoryColumn> columns = new List<InventoryColumn>()
            {
                // new InventoryColumn{ Caption = "UID", Property = "UID"},
                new InventoryColumn{ Caption = "PII", Property = "Barcode"},
                new InventoryColumn{ Caption = "状态", Property = "State"},
                new InventoryColumn{ Caption = "书名", Property = "Title"},
                new InventoryColumn{ Caption = "当前位置", Property = "CurrentLocation"},
                new InventoryColumn{ Caption = "当前架号", Property = "CurrentShelfNo"},
                new InventoryColumn{ Caption = "永久馆藏地", Property = "Location"},
                new InventoryColumn{ Caption = "永久架号", Property = "ShelfNo"},
                new InventoryColumn{ Caption = "盘点日期", Property = "InventoryTime"},
            };

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(this._cancel.Token))
            {
                FileDownloadDialog progress = null;
                this.Invoke(new Action(() =>
                {
                    progress = new FileDownloadDialog();
                    progress.Font = this.Font;
                    progress.Text = "正在导出本地册记录到 Excel 文件";
                    // progress.Show(this);
                }));

                try
                {
                    // 导出所有的本地册记录到 Excel 文件
                    var result = await ExportAllItemToExcelAsync(
                        columns,
                        (text, bytes, total) =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                // 延迟显示进度对话框
                                if (progress.Visible == false)
                                    progress.Show(this);

                                progress.SetProgress(text, bytes, total);
                                // progress.SetMessage(text);
                            }));
                        },
                        cancel.Token);

                    // 隐藏对话框
                    this.Invoke(new Action(() =>
                    {
                        progress.Hide();
                    }));

                    if (result.Value == -1)
                        MessageDlg.Show(this,
                            $"导出册记录到 Excel 文件过程出错: {result.ErrorInfo}",
                            "导出册记录到 Excel 文件");
                    else
                        MessageDlg.Show(this,
                            $"导出册记录到 Excel 文件完成，共导出 {result.Value} 行",
                            "导出册记录到 Excel 文件");
                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"导出册记录到 Excel 文件过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    MessageDlg.Show(this,
                        $"导出册记录到 Excel 文件过程出现异常: {ex.Message}",
                        "导出册记录到 Excel 文件");
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                }
            }
        }

        // 导入 UID-->UII 对照关系
        private async void MenuItem_importUidUiiTable_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "导入 UID PII 对照表 - 请指定文件名";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);    // WpfClientInfo.UserDir;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "对照表文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(this._cancel.Token))
            {
                FileDownloadDialog progress = null;
                this.Invoke(new Action(() =>
                {
                    progress = new FileDownloadDialog();
                    progress.Font = this.Font;
                    progress.Text = "正在导入 UID PII 对照表";
                    // progress.Show(this);
                }));
                progress.FormClosed += (o1, e1) =>
                {
                    cancel.Cancel();
                };
                try
                {

                    await Task.Run(async () =>
                    {
                        bool sip = DataModel.Protocol == "sip";
                        // 导入 UID PII 对照表文件
                        var result = await InventoryData.ImportUidPiiTableAsync(
                            openFileDialog.FileName,
                            (text, bytes, total) =>
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (progress.IsDisposed)
                                        return;

                                    // 延迟显示进度对话框
                                    if (progress.Visible == false)
                                        progress.Show(this);

                                    progress.SetProgress(text, bytes, total);
                                }));
                            },
                            this._cancel.Token);

                        // 隐藏对话框
                        this.Invoke(new Action(() =>
                        {
                            progress.Hide();
                        }));

                        if (result.Value == -1)
                            this.ErrorBox("导入 UID PII 对照表文件", $"导入过程出错: {result.ErrorInfo}");
                        else
                        {
                            if (sip)
                                this.ErrorBox("导入 UID PII 对照表文件", $"导入完成。\r\n\r\n共处理条目 {result.LineCount} 个; 新创建本地库记录 {result.NewCount} 个; 修改本地库记录 {result.ChangeCount} 个; 删除本地库记录 {result.DeleteCount}", "green");
                            else
                            {
                                if (result.ErrorCount > 0)
                                    this.ErrorBox("导入 UID PII 对照表文件", $"导入完成。\r\n\r\n共处理条目 {result.LineCount} 个; 修改册记录 {result.ChangeCount} 个; 出错 {result.ErrorCount} 次(已写入错误日志文件)", "green");
                                else
                                    this.ErrorBox("导入 UID PII 对照表文件", $"导入完成。\r\n\r\n共处理条目 {result.LineCount} 个; 修改册记录 {result.ChangeCount} 个", "green");
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"导入 UID 对照表过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    this.ErrorBox("导入 UID PII 对照表文件", $"导入 UID 对照表过程出现异常: {ex.Message}");
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                }
            }
        }

        void ErrorBox(string title,
            string text,
            string color = "red")
        {
            this.Invoke((Action)(() =>
            {
                MessageDlg.Show(this,
    text,
    title);
            }));
        }

        private async void MenuItem_clearUidUiiTable_Click(object sender, EventArgs e)
        {
            {
                var result = MessageBox.Show("若清除了本地 UID --> PII 缓存，后继第一次盘点时会速度较慢(但缓存会自动重新建立，速度会恢复)。\r\n\r\n确实要清除缓存？",
    "清除 UID-->UII 对照关系",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(this._cancel.Token))
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        var result = await InventoryData.ClearUidPiiLocalCacheAsync(
                            this._cancel.Token);
                        if (result.Value == -1)
                            this.ErrorBox("清除 UID-->UII 对照关系", $"清除过程出错: {result.ErrorInfo}");
                        else
                            this.ErrorBox("清除 UID-->UII 对照关系", $"清除完成。\r\n\r\n共清除条目 {result.Value} 个", "green");
                    });
                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"清除 UID-->UII 对照关系过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    this.ErrorBox("清除 UID-->UII 对照关系", $"清除 UID-->UII 对照关系过程出现异常: {ex.Message}");
                }
                finally
                {
                }
            }
        }


        #region ProcessManager

        CancellationTokenSource _cancelProcessMonitor = new CancellationTokenSource();

        public void StartProcessManager()
        {
            // 停止前一次的 monitor
            if (_cancelProcessMonitor != null)
            {
                _cancelProcessMonitor.Cancel();
                _cancelProcessMonitor.Dispose();

                _cancelProcessMonitor = new CancellationTokenSource();
            }

            // if (ProcessMonitor == true)
            {
                List<DigitalPlatform.IO.ProcessInfo> infos = new List<DigitalPlatform.IO.ProcessInfo>();

                if (string.IsNullOrEmpty(DataModel.RfidCenterUrl) == false
                    && ProcessManager.IsIpcUrl(DataModel.RfidCenterUrl))
                    infos.Add(new DigitalPlatform.IO.ProcessInfo
                    {
                        Name = "RFID中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/dp2-RFID中心",
                        MutexName = "{CF1B7B4A-C7ED-4DB8-B5CC-59A067880F92}"
                    });

                ProcessManager.Start(infos,
                    (info, text) =>
                    {
                        ClientInfo.WriteErrorLog($"{info.Name} {text}");
                    },
                    _cancelProcessMonitor.Token);
            }
        }

        #endregion

        ShelfDialog _shelfDialog = null;

        void CreateShelfDialog()
        {
            if (_shelfDialog == null)
            {
                _shelfDialog = new ShelfDialog();

                _shelfDialog.FormClosing += (sender, e) =>
                {
                    var dialog = sender as Form;

                    // 将关闭改为隐藏
                    dialog.Visible = false;
                    if (e.CloseReason == CloseReason.UserClosing)
                        e.Cancel = true;
                };

                GuiUtil.SetControlFont(_shelfDialog, this.Font);
                ClientInfo.MemoryState(_shelfDialog, "shelfDialog", "state");
                // _shelfDialog.UiState = ClientInfo.Config.Get("shelfDialog", "uiState", null);
            }
        }

        private void MenuItem_shelfWindow_Click(object sender, EventArgs e)
        {
            CreateShelfDialog();

            if (_shelfDialog.Visible == false)
                _shelfDialog.Show(this);
            else if (_shelfDialog.WindowState == FormWindowState.Minimized)
                _shelfDialog.WindowState = FormWindowState.Normal;
        }
    }

    public delegate void WriteCompleteEventHandler(object sender,
WriteCompleteventArgs e);

    /// <summary>
    /// 写入成功事件的参数
    /// </summary>
    public class WriteCompleteventArgs : EventArgs
    {
        public ProcessInfo Info { get; set; }
        // 盘点动作
        public string Action { get; set; }
    }

    public delegate void AddBookEventHandler(object sender,
AddBookEventArgs e);

    /// <summary>
    /// 加入图书(到 ShelfDialog)事件的参数
    /// </summary>
    public class AddBookEventArgs : EventArgs
    {
        public ListViewItem Item { get; set; }

        public ListView.ColumnHeaderCollection Columns { get; set; }
    }
}
