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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;

namespace dp2ManageCenter.Message
{
    public partial class ShelfSearchForm : Form
    {
        P2PConnection _connection = new P2PConnection();

        CancellationTokenSource _cancel = new CancellationTokenSource();

        const int COLUMN_ID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_ACTION = 2;
        const int COLUMN_OPERTIME = 3;
        const int COLUMN_STATE = 4;
        const int COLUMN_ERRORCODE = 5;
        const int COLUMN_ERRORINFO = 6;

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
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private async void ShelfSearchForm_Load(object sender, EventArgs e)
        {
            FillMyAccountList();

            this.UiState = ClientInfo.Config.Get("shelfSearchForm", "ui_state", "");

            var result = await ConnectAsync();
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);

        }

        private void ShelfSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();

            ClientInfo.Config?.Set("shelfSearchForm", "ui_state", this.UiState);

            _connection.CloseConnection();
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

        static Account FindAccount(List<Account> accounts,
            string userNameAndUrl)
        {
            var parts = StringUtil.ParseTwoPart(userNameAndUrl, "@");
            string userName = parts[0];
            string serverUrl = parts[1];
            var found = accounts.FindAll(o=>o.UserName == userName && o.ServerUrl == serverUrl);
            if (found.Count == 0)
                return null;
            return found[0];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "<挂起>")]
        private async void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            try
            {

                string remoteUserName = this.comboBox_query_shelfAccount.Text;

                SearchRequest request = new SearchRequest(Guid.NewGuid().ToString(),
                    null,   // loginInfo,
                    "searchBiblio",
                    "dbNameList",
                    this.textBox_query_word.Text,
                    this.comboBox_query_from.Text,
                    this.comboBox_query_matchStyle.Text,
                    "resultsetName",
                    "json",
                    -1,
                    0,
                    10);
                var result = await _connection.SearchAsyncLite(remoteUserName,
                    request,
                    TimeSpan.FromSeconds(10),
                    _cancel.Token);
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
                FillRecords(result.Records);
                return;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        void FillRecords(List<Record> records)
        {
            this.listView_records.Items.Clear();

            foreach(var record in records)
            {
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_ID, record.RecPath);
                ListViewUtil.ChangeItemText(item, COLUMN_ACTION, "");

                this.listView_records.Items.Add(item);
            }
        }
    }
}
