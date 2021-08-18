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

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MessageClient;
using DigitalPlatform.CirculationClient;

namespace dp2ManageCenter.Message
{
    public partial class ShelfSearchForm : Form
    {
        // P2PConnection _connection = new P2PConnection();

        CancellationTokenSource _cancel = new CancellationTokenSource();

        const int COLUMN_ID = 0;
        const int COLUMN_ACTION = 1;
        const int COLUMN_OPERATOR = 2;
        const int COLUMN_NAME = 3;
        const int COLUMN_PII = 4;
        const int COLUMN_BOOKTITLE = 5;
        const int COLUMN_OPERTIME = 6;
        const int COLUMN_STATE = 7;
        const int COLUMN_ERRORCODE = 8;
        const int COLUMN_ERRORINFO = 9;
        const int COLUMN_SYNCCOUNT = 10;
        const int COLUMN_SYNCOPERTIME = 11;
        const int COLUMN_ACTIONINFO = 12;
        const int COLUMN_LINKID = 13;
        const int COLUMN_BATCHNO = 14;
        const int COLUMN_TOSHELFNO = 15;
        const int COLUMN_TOLOCATION = 16;
        const int COLUMN_TRANSFERDIRECTION = 17;

        public ShelfSearchForm()
        {
            InitializeComponent();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_query_myAccount),
                    new ComboBoxText(this.comboBox_query_shelfAccount),
                    this.textBox_query_word,
                    new ComboBoxText(this.comboBox_query_from),
                    new ComboBoxText(this.comboBox_query_matchStyle),
                    this.listView_records,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_query_myAccount),
                    new ComboBoxText(this.comboBox_query_shelfAccount),
                    this.textBox_query_word,
                    new ComboBoxText(this.comboBox_query_from),
                    new ComboBoxText(this.comboBox_query_matchStyle),
                    this.listView_records,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void ShelfSearchForm_Load(object sender, EventArgs e)
        {
            FillMyAccountList();

            this.UiState = ClientInfo.Config.Get("shelfSearchForm", "ui_state", "");

            /*
            var result = await ConnectAsync();
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);
                */
        }

        private void ShelfSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();

            ClientInfo.Config?.Set("shelfSearchForm", "ui_state", this.UiState);

            // _connection.CloseConnection();
        }

        private void ShelfSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void comboBox_query_myAccount_DropDown(object sender, EventArgs e)
        {
            FillMyAccountList();
        }

        void FillMyAccountList()
        {
            if (this.comboBox_query_myAccount.Items.Count == 0)
            {
                var accounts = MessageAccountForm.GetAccounts();
                foreach (var account in accounts)
                {
                    this.comboBox_query_myAccount.Items.Add(account.UserName + "@" + account.ServerUrl);
                }
            }
        }

        /*
        async Task<NormalResult> ConnectAsync()
        {
            string userNameAndUrl = this.comboBox_query_myAccount.Text;
            var accounts = MessageAccountForm.GetAccounts();
            var account = FindAccount(accounts, userNameAndUrl);
            if (account == null)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"用户名 '{userNameAndUrl}' 没有找到"
                };
            }
            return await _connection.ConnectAsync(account.ServerUrl,
                account.UserName,
                account.Password,
                "");
        }
        */

        CancellationTokenSource _cancelSearch = new CancellationTokenSource();

        void EnableSearchButtons(bool bEnable)
        {
            try
            {
                this.Invoke((Action)(() =>
                {
                    this.comboBox_query_from.Enabled = bEnable;
                    this.comboBox_query_matchStyle.Enabled = bEnable;
                    this.comboBox_query_myAccount.Enabled = bEnable;
                    this.comboBox_query_shelfAccount.Enabled = bEnable;
                    this.textBox_query_word.Enabled = bEnable;

                    this.button_search.Enabled = bEnable;
                    this.button_stop.Enabled = !bEnable;
                }));
            }
            catch (InvalidOperationException)
            {
                // 可能是因为窗口被摧毁后调用的原因
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "<挂起>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        private async void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            _cancelSearch = new CancellationTokenSource();
            CancellationToken token = _cancelSearch.Token;

            EnableSearchButtons(false);
            try
            {
                this.listView_records.Items.Clear();

                var get_result = await ConnectionPool.OpenConnectionAsync(this.comboBox_query_myAccount.Text);
                if (get_result.Value == -1)
                {
                    strError = get_result.ErrorInfo;
                    goto ERROR1;
                }

                var connection = get_result.Connection;

                string resultsetName = "default";
                string remoteUserName = this.comboBox_query_shelfAccount.Text;

                SearchRequest request = new SearchRequest(Guid.NewGuid().ToString(),
                    null,   // loginInfo,
                    "searchHistory",    // "searchBiblio",
                    "dbNameList",
                    this.textBox_query_word.Text,
                    this.comboBox_query_from.Text,
                    this.comboBox_query_matchStyle.Text,
                    resultsetName,
                    "json",
                    -1,
                    0,
                    10);
                var result = await connection.SearchAsyncLite(remoteUserName,
                    request,
                    TimeSpan.FromSeconds(60),
                    token);
                if (result.ResultCount == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "没有命中";
                    goto ERROR1;
                }
                FillRecords(result.Records, true, token);
                if (result.Records.Count < result.ResultCount)
                {
                    var fill_result = await FillRestRecordsAsync(
                        connection,
                        remoteUserName,
                        resultsetName,
                        result.Records.Count,
                        token);
                }
                return;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetDebugText(ex);  // ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableSearchButtons(true);
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        void FillRecords(List<Record> records, bool clear_before,
            CancellationToken token)
        {
            if (clear_before)
                this.listView_records.Items.Clear();

            // this.listView_records.BeginUpdate();
            foreach (var record in records)
            {
                if (token.IsCancellationRequested)
                    return;

                var request = JsonConvert.DeserializeObject<RequestItem>(record.Data);

                ListViewItem item = new ListViewItem();
                /*
                ListViewUtil.ChangeItemText(item, COLUMN_ID, record.RecPath);
                ListViewUtil.ChangeItemText(item, COLUMN_PII, request.PII);
                ListViewUtil.ChangeItemText(item, COLUMN_ACTION, request.Action);
                ListViewUtil.ChangeItemText(item, COLUMN_OPERTIME, request.OperTime.ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_STATE, request.State);
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORCODE, request.SyncErrorCode);
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, request.SyncErrorInfo);
                ListViewUtil.ChangeItemText(item, COLUMN_SYNCCOUNT, request.SyncCount.ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_SYNCOPERTIME, "");
                ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, request.BatchNo);
                ListViewUtil.ChangeItemText(item, COLUMN_TOSHELFNO, request.CurrentShelfNo);
                ListViewUtil.ChangeItemText(item, COLUMN_TOLOCATION, request.Location);
                ListViewUtil.ChangeItemText(item, COLUMN_TRANSFERDIRECTION, request.TransferDirection);
                item.Tag = request;
                */
                SetLine(item, request);
                this.listView_records.Items.Add(item);
            }
            // this.listView_records.EndUpdate();
        }

        static void SetLine(ListViewItem item, RequestItem request)
        {
            // testing
            // request.State = null;

            string action_info = "";
            if (request.Action == "borrow")
            {
                var borrow_info = request.GetBorrowInfo();
                if (borrow_info != null)
                {
                    string returning_date = "";
                    if (string.IsNullOrEmpty(borrow_info.LatestReturnTime) == false)
                    {
                        if (DateTimeUtil.TryParseRfc1123DateTimeString(borrow_info.LatestReturnTime, out DateTime time) == true)
                            returning_date = time.ToString();
                        else
                            returning_date = $"时间字符串 '{borrow_info.LatestReturnTime}' 格式错误";
                    }
                    action_info = $"应还时间: {returning_date} borrowID={borrow_info.BorrowID}";
                }
            }
            else
            {
                var return_info = request.GetReturnInfo();
                if (return_info != null
                    && string.IsNullOrEmpty(return_info.OverdueString) == false)
                {
                    action_info = $"超期情况: {return_info.OverdueString}";
                }
            }

            ListViewUtil.ChangeItemText(item, COLUMN_ID, request.ID.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_PII, request.PII);
            ListViewUtil.ChangeItemText(item, COLUMN_BOOKTITLE, request.Entity?.Title);
            ListViewUtil.ChangeItemText(item, COLUMN_ACTION, request.Action);
            ListViewUtil.ChangeItemText(item, COLUMN_OPERATOR, request.OperatorID);
            ListViewUtil.ChangeItemText(item, COLUMN_NAME, request.Operator?.PatronName);

            ListViewUtil.ChangeItemText(item, COLUMN_OPERTIME, request.OperTime.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_STATE, request.State);
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORCODE, request.SyncErrorCode);
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, request.SyncErrorInfo);
            ListViewUtil.ChangeItemText(item, COLUMN_SYNCCOUNT, request.SyncCount.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_SYNCOPERTIME, request.SyncOperTime.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_ACTIONINFO, action_info);

            ListViewUtil.ChangeItemText(item, COLUMN_LINKID, request.LinkID);
            ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, request.BatchNo);
            ListViewUtil.ChangeItemText(item, COLUMN_TOSHELFNO, request.CurrentShelfNo);
            ListViewUtil.ChangeItemText(item, COLUMN_TOLOCATION, request.Location);
            ListViewUtil.ChangeItemText(item, COLUMN_TRANSFERDIRECTION, request.TransferDirection);
            item.Tag = request;
            SetLineColor(item, request);
        }

        static void SetLineColor(ListViewItem item, RequestItem request)
        {
            Color backColor = Color.White;
            Color foreColor = Color.Black;
            if (request.State == "sync")
            {
                backColor = Color.DarkGreen;
                foreColor = GetForeColor();
            }
            else if (request.State == "dontsync")
            {
                backColor = Color.DarkBlue;
                foreColor = GetForeColor();
            }
            else if (request.State != null && request.State.Contains("error"))
            {
                backColor = Color.DarkRed;
                foreColor = GetForeColor();
            }

            // TODO: Action为 "borrow" 的，LinkID 为空的，显示为黄色？

            item.BackColor = backColor;
            item.ForeColor = foreColor;

            Color GetForeColor()
            {
                if (request.Action == "borrow" && string.IsNullOrEmpty(request.LinkID) == true)
                    return Color.Yellow;
                return Color.White;
            }
        }

        // 继续填充余下的命中记录
        async Task<NormalResult> FillRestRecordsAsync(
            P2PConnection connection,
            string remoteUserName,
            string resultsetName,
            int start,
            CancellationToken token)
        {
            try
            {
                // string remoteUserName = this.comboBox_query_shelfAccount.Text;
                while (token.IsCancellationRequested == false)
                {
                    SearchRequest request = new SearchRequest(Guid.NewGuid().ToString(),
                        null,   // loginInfo,
                        "searchHistory", // "searchBiblio",
                        "", // "dbNameList",
                        "!getResult",
                        "", // this.comboBox_query_from.Text,
                        "", // this.comboBox_query_matchStyle.Text,
                        resultsetName,
                        "json",
                        -1,
                        start,
                        10);
                    var result = await connection.SearchAsyncLite(remoteUserName,
                        request,
                        TimeSpan.FromSeconds(60),
                        token);
                    if (result.ResultCount == -1)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = result.ErrorInfo
                        };
                    }
                    if (result.ResultCount == 0)
                    {
                        // strError = "没有命中";
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "检索没有命中结果。无法获得结果"
                        };
                    }
                    if (start >= result.ResultCount)
                        break;

                    FillRecords(result.Records, false, token);
                    start += result.Records.Count;
                    if (start >= result.ResultCount)
                        break;
                }

                return new NormalResult();
            }
            catch (AggregateException ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = MessageConnection.GetExceptionText(ex)
                };
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message
                };
            }
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyJSONtoClipboard);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            /*
            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);
            */

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改状态 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&M)");
            menuItem.Tag = "state";
            menuItem.Click += new System.EventHandler(this.MenuItem_modifyState_Click);
            if (this.listView_records.SelectedItems.Count != 1)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改 “关联 ID” [" + this.listView_records.SelectedItems.Count.ToString() + "] (&L)");
            menuItem.Tag = "linkID";
            menuItem.Click += new System.EventHandler(this.MenuItem_modifyState_Click);
            if (this.listView_records.SelectedItems.Count != 1)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.MenuItem_deleteRecords_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_copyJSONtoClipboard(object sender, System.EventArgs e)
        {
            string strError;
            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要复制的事项 ...";
                goto ERROR1;
            }

            StringBuilder text = new StringBuilder();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                RequestItem tag = item.Tag as RequestItem;
                string value = JsonConvert.SerializeObject(tag, Formatting.Indented);
                text.AppendLine(value);
            }

            Clipboard.SetText(text.ToString());
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 修改状态
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        async void MenuItem_modifyState_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的事项";
                goto ERROR1;
            }

            string field = (sender as MenuItem).Tag as string;

            string new_state = null;
            if (field == "state")
                new_state = InputDlg.GetInput(this, "修改状态", "新状态值", "dontsync", this.Font);
            else if (field == "linkID")
            {
                var item = this.listView_records.SelectedItems[0];
                var action_info = ListViewUtil.GetItemText(item, COLUMN_ACTIONINFO);
                int index = action_info.IndexOf("borrowID");
                string default_value = "borrowID=xxx";
                if (index != -1)
                    default_value = action_info.Substring(index).Trim();

                new_state = InputDlg.GetInput(this, "修改 关联 ID", "新“关联 ID”值", default_value, this.Font);
            }
            else
            {
                strError = $"未知的 field 值 '{field}'";
                goto ERROR1;
            }

            if (new_state == null)
                return;
            try
            {
                SetInfoRequest request = new SetInfoRequest();
                request.Operation = "setHistory";
                request.Entities = new List<Entity>();

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    RequestItem tag = item.Tag as RequestItem;
                    if (field == "state")
                        tag.State = new_state;
                    else if (field == "linkID")
                    {
                        if (tag.Action != "borrow")
                        {
                            strError = "只允许针对 Action 为 'borrow' 的行修改 linkID";
                            goto ERROR1;
                        }
                        tag.LinkID = new_state;
                    }
                    Record record = new Record { Data = JsonConvert.SerializeObject(tag) };
                    request.Entities.Add(new Entity
                    {
                        Action = $"change:{field}",
                        NewRecord = record
                    });
                }

                var connection = await ConnectionPool.GetConnectionAsync(this.comboBox_query_myAccount.Text);
                var result = await connection.SetInfoAsyncLite(this.comboBox_query_shelfAccount.Text,
                    request,
                    TimeSpan.FromSeconds(10),
                    default);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                // 报错
                List<string> errors = new List<string>();
                List<string> succeed_records = new List<string>();
                if (result.Entities != null)
                {
                    foreach (var entity in result.Entities)
                    {
                        if (string.IsNullOrEmpty(entity.ErrorInfo) == false)
                            errors.Add(entity.ErrorInfo);
                        else
                            succeed_records.Add(entity.NewRecord?.Data);
                    }
                }

                // 刷新成功修改了的行
                foreach (var record in succeed_records)
                {
                    if (string.IsNullOrEmpty(record))
                        continue;
                    var item = JsonConvert.DeserializeObject<RequestItem>(record);
                    RefreshLine(item.ID, item);
                }

                if (errors.Count > 0)
                    MessageDialog.Show(this, $"出错:\r\n{StringUtil.MakePathList(errors, "\r\n")}");
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 刷新一行的显示
        void RefreshLine(int id, RequestItem request_item)
        {
            // 先定位到这一行
            ListViewItem item = ListViewUtil.FindItem(this.listView_records, id.ToString(), COLUMN_ID);
            if (item == null)
                return;
            SetLine(item, request_item);
        }

        // 删除记录
        void MenuItem_deleteRecords_Click(object sender, EventArgs e)
        {

        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            _cancelSearch?.Cancel();
        }
    }

    public class RequestItem
    {
        public int ID { get; set; }

        public string PII { get; set; } // PII 单独从 EntityString 中抽取出来，便于进行搜索

        public string Action { get; set; }  // borrow/return/transfer

        public DateTime OperTime { get; set; }  // 操作时间。这是首次操作时间，然后重试同步的时候并不改变这个时间
        public string State { get; set; }   // 状态。sync/commerror/normalerror/空
                                            // 表示是否完成同步，还是正在出错重试同步阶段，还是从未同步过
        public string SyncErrorInfo { get; set; }   // 最近一次同步操作的报错信息
        public string SyncErrorCode { get; set; }   // 最近一次同步操作的错误码
        public int SyncCount { get; set; }

        public RequestOperator Operator { get; set; }  // 提起请求的读者

        // Operator 对象 JSON 化以后的字符串
        // public string OperatorString { get; set; }

        public RequestEntity Entity { get; set; }
        // Entity 对象 JSON 化以后的字符串
        // public string EntityString { get; set; }

        public string TransferDirection { get; set; } // in/out 典藏移交的方向
        public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
        public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
        public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成

        // 操作者 ID。为读者证条码号，或者 ~工作人员账户名
        public string OperatorID { get; set; }  // 从 Operator 而来

        // 同步操作时间。最后一次同步操作的时间
        public DateTime SyncOperTime { get; set; }

        public string LinkID { get; set; }

        public string ActionString { get; set; }

        public DigitalPlatform.LibraryClient.localhost.ReturnInfo GetReturnInfo()
        {
            if (this.ActionString == null)
                return null;
            return JsonConvert.DeserializeObject<DigitalPlatform.LibraryClient.localhost.ReturnInfo>(this.ActionString);
        }

        public DigitalPlatform.LibraryClient.localhost.BorrowInfo GetBorrowInfo()
        {
            if (this.ActionString == null)
                return null;
            return JsonConvert.DeserializeObject<DigitalPlatform.LibraryClient.localhost.BorrowInfo>(this.ActionString);
        }

#if REMOVED
        public string GetBookTitle()
        {
            /*
            if (this.EntityString == null)
                return null;
            var entity = JsonConvert.DeserializeObject<RequestEntity>(this.EntityString);
            return entity?.Title;
            */
            return this.Entity?.Title;
        }
#endif
    }

    public class RequestOperator
    {
        public string PatronName { get; set; }
        public string PatronBarcode { get; set; }
    }

    public class RequestEntity
    {
        // TagInfo
        public string ItemRecPath { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        //dup.BorrowInfo = this.BorrowInfo;
        //dup.ShelfState = this.ShelfState;
        //dup.OnShelf = this.OnShelf;
        //dup.BelongToCurrentShelf = this.BelongToCurrentShelf;
        //dup.FillFinished = this.FillFinished;
        //dup.State = this.State;
        public string CurrentLocation { get; set; }
        public string ShelfNo { get; set; }

        public string PII { get; set; }
        public string UID { get; set; }
        //dup.Error = this.Error;
        //dup.ErrorColor = this.ErrorColor;
        //dup.Waiting = this.Waiting;
        public string ReaderName { get; set; }
        public string Antenna { get; set; }
    }

    public class DoubleBufferdListView : ListView
    {
        protected override bool DoubleBuffered
        {
            get => true;
            set => base.DoubleBuffered = true;
        }
    }
}
