using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace dp2ManageCenter.Message
{
    public partial class CommandDialog : Form
    {
        public CommandDialog()
        {
            InitializeComponent();
        }

        public string MyAccount
        {
            get { return this.comboBox_myAccount.Text; }
            set { this.comboBox_myAccount.Text = value; }
        }

        public string TargetAccount
        {
            get { return this.comboBox_targetAccount.Text; }
            set { this.comboBox_targetAccount.Text = value; }
        }

        // 使用过的 TargetAccount 值列表
        public List<string> UsedTargetAccounts
        {
            get
            {
                return new List<string> (this.comboBox_targetAccount.Items.Cast<string>());
            }
            set
            {
                this.comboBox_targetAccount.Items.Clear();
                if (value != null)
                {
                    foreach (var s in value)
                    {
                        this.comboBox_targetAccount.Items.Add(s);
                    }
                }
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_myAccount),
                    new ComboBoxText(this.comboBox_targetAccount),
                    this.textBox_command,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_myAccount),
                    new ComboBoxText(this.comboBox_targetAccount),
                    this.textBox_command,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        void EnableControls(bool enable)
        {
            this.comboBox_myAccount.Enabled = enable;
            this.comboBox_targetAccount.Enabled = enable;
            this.textBox_command.Enabled = enable;
            this.button_send.Enabled = enable;
            this.button_stop.Enabled = !enable;
        }

        CancellationTokenSource _cancelSearch = null;

        private async void button_send_Click(object sender, EventArgs e)
        {
            string strError = "";

            _cancelSearch?.Dispose();
            _cancelSearch = new CancellationTokenSource();
            CancellationToken token = _cancelSearch.Token;

            EnableControls(false);
            try
            {
                var get_result = await ConnectionPool.OpenConnectionAsync(this.MyAccount);
                if (get_result.Value == -1)
                {
                    strError = get_result.ErrorInfo;
                    goto ERROR1;
                }

                SetStatusText($"正在发送命令 {this.textBox_command.Text} ...");

                var connection = get_result.Connection;

                string remoteUserName = this.TargetAccount;
                AddTargetAccountToUsedList(remoteUserName);

                string command = this.textBox_command.Text;

                SetInfoRequest request = new SetInfoRequest();
                request.TaskID = Guid.NewGuid().ToString();
                request.Operation = "command";
                request.BiblioRecPath = null;
                request.Entities = new List<Entity>() { new Entity { Action = command } };

                var result = await connection.SetInfoAsyncLite(remoteUserName,
                    request,
                    TimeSpan.FromSeconds(60),
                    token);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
                if (result.Entities == null || result.Entities.Count == 0)
                {
                    strError = "没有返回 Entities 结果";
                    goto ERROR1;
                }
                var result_entity = result.Entities[0];
                MessageBox.Show(this, $"ErrorInfo={result_entity.ErrorInfo}, ErrorCode={result_entity.ErrorCode}");
                SetStatusText("命令发送完成");
                return;
            }
            catch (OperationCanceledException)
            {
                strError = "用户中断";
                goto ERROR1;
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
                EnableControls(true);
            }

        ERROR1:
            SetStatusText(strError.Replace("\r\n", "; "));
            MessageBox.Show(this, strError);
        }

        void AddTargetAccountToUsedList(string text)
        {
            if (this.comboBox_targetAccount.Items.Cast<string>().Contains(text))
                return;
            this.comboBox_targetAccount.Items.Add(text);
        }

        void SetStatusText(string text)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripStatusLabel_message.Text = text;
            }));
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            _cancelSearch?.Cancel();
        }

        private void GetFileDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            {
                var list = StringUtil.MakePathList(this.UsedTargetAccounts, ",");

                ClientInfo.Config.Set("CommandDialog",
        "usedTargetAccounts",
        list);
            }
        }

        private void GetFileDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancelSearch?.Cancel();
            _cancelSearch?.Dispose();
        }

        void FillMyAccountList()
        {
            if (this.comboBox_myAccount.Items.Count == 0)
            {
                var accounts = MessageAccountForm.GetAccounts();
                foreach (var account in accounts)
                {
                    this.comboBox_myAccount.Items.Add(account.UserName + "@" + account.ServerUrl);
                }
            }
        }

        private void comboBox_query_myAccount_DropDown(object sender, EventArgs e)
        {
            FillMyAccountList();
        }

        private void GetFileDialog_Load(object sender, EventArgs e)
        {
            var list = ClientInfo.Config.Get("CommandDialog",
                "usedTargetAccounts",
                "");
            this.UsedTargetAccounts = StringUtil.SplitList(list);
        }
    }
}
