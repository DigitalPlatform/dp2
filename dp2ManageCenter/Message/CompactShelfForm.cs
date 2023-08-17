using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace dp2ManageCenter.Message
{
    public partial class CompactShelfForm : Form
    {
        ErrorTable _errorTable = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

        string _userNameAndUrl = "";

        // 当前用户名和 dp2mserver 服务器 URL
        public string UserNameAndUrl
        {
            get
            {
                return _userNameAndUrl;
            }
            set
            {
                _userNameAndUrl = value;
                this.Text = $"密集书架 {_userNameAndUrl}";

                // 2020/9/20
                RemoveEvents();

                _ = AddEventAsync();
            }
        }

        public CompactShelfForm()
        {
            InitializeComponent();
            this.toolStripStatusLabel_alive.Alignment = ToolStripItemAlignment.Right;

            _errorTable = new ErrorTable((s) =>
            {
                try
                {
                    this.TryInvoke(() =>
                    {
                        var error1 = _errorTable.GetError("nearCode");
                        var error2 = _errorTable.GetError("connect");
                        bool error = (error1 != null && error1.StartsWith(" "))
                        || (error2 != null && error2.StartsWith(" "));
                        if (string.IsNullOrEmpty(s) == false)
                        {
                            string text = s.Replace(";", "\r\n");
                            // string text = s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "。");   // .Replace(";", "\r\n");
                            if (text != GetStatusMessage())
                            {
                                if (error)
                                    this.SetStatusMessage(text, "red");
                                else
                                    this.SetStatusMessage(text);

                                // ClientInfo.WriteErrorLog(text);
                            }
                        }
                        else
                            this.SetStatusMessage("");
                    });
                }
                catch (ObjectDisposedException)
                {

                }
            });
        }

        private void CompactShelfForm_Load(object sender, EventArgs e)
        {
            LoadSettings();

        }

        private void CompactShelfForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall
|| e.CloseReason == CloseReason.FormOwnerClosing)
            {
            }
            else
            {
                var result = MessageBox.Show(this,
                    "关闭此对话框，意味着停止密集书架服务。\r\n\r\n确实要关闭?",
                    "CompactShelfForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            SaveSettings();
        }

        private void CompactShelfForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Cancel();
            RemoveEvents();
        }

        // 当前窗口用过的 P2PConnection 对象
        List<P2PConnection> _usedConnections = new List<P2PConnection>();

        // 为当前 this.UserNameAndUrl 挂接 AddMessage 事件
        async Task AddEventAsync()
        {
            bool succeed = false;
            StopUpdateNearCode();
            _errorTable.SetError("nearCode", null);
            _errorTable.SetError("connect", $"正在连接 '{this.UserNameAndUrl}' ...");
            try
            {
                // 挂接事件
                var get_result = await ConnectionPool.OpenConnectionAsync(this.UserNameAndUrl);
                if (get_result.Value == -1)
                {
                    _errorTable.SetError("connect", $" 连接 '{this.UserNameAndUrl}' 时出错: {get_result.ErrorInfo}");
                    MessageBox.Show(this, $"连接 '{this.UserNameAndUrl}' 时出错: {get_result.ErrorInfo}");
                    return;
                }

                P2PConnection connection = get_result.Connection;

                // 记忆
                if (_usedConnections.IndexOf(connection) == -1)
                    _usedConnections.Add(connection);

                connection.AddMessage -= Connection_AddMessage;
                connection.AddMessage += Connection_AddMessage;

                connection.SetInfo -= Connection_SetInfo;
                connection.SetInfo += Connection_SetInfo;

                // TODO: 界面显示连接成功
                succeed = true;
                StartUpdateNearCode();
            }
            finally
            {
                if (succeed)
                    _errorTable.SetError("connect", null);
            }
        }

        // 用于拼接 data 的缓冲区
        StringBuilder _messageData = new StringBuilder();

        private void Connection_AddMessage(object sender, AddMessageEventArgs e)
        {

        }

        private void Connection_SetInfo(object sender, SetInfoEventArgs e)
        {
            P2PConnection connection = sender as P2PConnection;

            // 单独给一个线程来执行
            _ = Task.Run(async () =>
            {
                try
                {
                    await SetInfoAndResponse(connection, e.Request);
                }
                catch (Exception ex)
                {
                    // 写入错误日志
                    ClientInfo.WriteErrorLog($"SetInfoAndResponse() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        async Task SetInfoAndResponse(
            P2PConnection connection,
            SetInfoRequest param)
        {
            string strError = "";

            try
            {
                ChangeHistoryResult result = null;
                if (param.Operation == "command")
                    result = await CommandAsync(param.Entities);
                else
                {
                    strError = "无法识别的 param.Operation 值 '" + param.Operation + "'";
                    goto ERROR1;
                }

                await connection.ResponseSetInfo(
                    param.TaskID,
    result.ResultEntities.Count,
    result.ResultEntities,
    strError);
                return;
            }
            catch (Exception ex)
            {
                // AddErrorLine("SetInfoAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

        ERROR1:
            // 报错
            await connection.TryResponseSetInfo(
param.TaskID,
-1,
new List<DigitalPlatform.MessageClient.Entity>(),   // results
strError);

        }

        public class ChangeHistoryResult : NormalResult
        {
            // 返回的实体
            public List<DigitalPlatform.MessageClient.Entity> ResultEntities { get; set; }
        }

        // 执行命令
        async Task<ChangeHistoryResult> CommandAsync(List<DigitalPlatform.MessageClient.Entity> actions)
        {
            List<DigitalPlatform.MessageClient.Entity> results = new List<DigitalPlatform.MessageClient.Entity>();

            foreach (var action in actions)
            {
                string action_string = action.Action;

                if (action_string.StartsWith("compactShelf"))
                {
                    ClientInfo.WriteErrorLog($"执行命令 '{action_string}'");
                    // result.Value:
                    //      -1  出错
                    //      0   设备没有成功
                    //      1   设备成功
                    var result = await CompactShelfAsync(action_string);
                    ClientInfo.WriteErrorLog($"执行命令 '{action_string}' 返回结果 {result.ToString()}");

                    if (result.Value == -1
                        && string.IsNullOrEmpty(result.ErrorCode))
                    {
                        result.ErrorCode = "error";
                    }

                    // 确保: 当 ErrorCode 为空的时候表示操作成功
                    if (result.Value == 1)
                        result.ErrorCode = null;

                    var result_entity = new DigitalPlatform.MessageClient.Entity();
                    result_entity.Action = action.Action;
                    result_entity.OldRecord = null;
                    result_entity.ErrorCode = result.ErrorCode;
                    result_entity.ErrorInfo = result.ErrorInfo;

                    results.Add(result_entity);
                }
                else
                {
                    var result_entity = new DigitalPlatform.MessageClient.Entity();
                    result_entity.Action = action.Action;
                    result_entity.OldRecord = null;
                    result_entity.ErrorCode = "unknownCommand";
                    result_entity.ErrorInfo = $"无法识别命令 '{action_string}'";

                    results.Add(result_entity);
                }
            }

            return new ChangeHistoryResult { ResultEntities = results };
        }

        static AsyncSemaphore _writeTagLimit = new AsyncSemaphore(1);

        // result.Value:
        //      -1  出错
        //      0   设备没有成功
        //      1   设备成功
        public async Task<NormalResult> CompactShelfAsync(string command)
        {
            using (var releaser = await _writeTagLimit.EnterAsync().ConfigureAwait(false))
            {
                // REDO:
                // 子参数
                string param = command.Substring("compactShelf".Length).Trim();

                // open xxx
                // close xxx
                List<string> parameters = StringUtil.SplitList(param, " ");
                if (parameters.Count < 3)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "compactShelf 后面的参数应为三个(分别是 open|close 地址 现场码)"
                    };
                }
                string action = parameters[0];
                string shelf = parameters[1];
                string nearCode = parameters[2];

                // 先核对现场码
                if (nearCode != NearCode)
                {
                    // TODO: 现场码不匹配的次数超过限额，需要把对方账户名打入黑名单。
                    // 同时记入错误日志，也可让系统进入慢速状态，避免现场码被破解
                    // 或者保护性主动变化一次现场码
                    NewNearCode();

                    ClientInfo.WriteErrorLog($"现场码 '{nearCode}' 不正确。已触发一次变化");

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"现场码 '{nearCode}' 不正确",
                        ErrorCode = "nearCodeMismatch"
                    };
                }

                // MessageBox.Show($"action='{action}' shelf='{shelf}'");
                NormalResult result = null;
                if (action == "open")
                    result = await CompactShelfDriver.First.Driver.OpenColumn(DriverUrl, shelf);
                else if (action == "close")
                    result = await CompactShelfDriver.First.Driver.CloseArea(DriverUrl, shelf);

                return result;
            }
        }


        // 2020/9/20
        // 解挂所有 AddMessage 事件
        void RemoveEvents()
        {
            if (_usedConnections == null)
                return;

            foreach (var connection in _usedConnections)
            {
                connection.AddMessage -= Connection_AddMessage;
                connection.SetInfo -= Connection_SetInfo;
            }

            _usedConnections.Clear();
        }


        private void toolStripButton_selectAccount_Click(object sender, EventArgs e)
        {
            using (MessageAccountForm dlg = new MessageAccountForm())
            {
                dlg.Mode = "select";
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;
                var account = dlg.SelectedAccount;

                this.SaveSettings();

                this.UserNameAndUrl = account.UserName + "@" + account.ServerUrl;
            }
        }

        void LoadSettings()
        {
            //this.UiState = ClientInfo.Config.Get("chat", "uiState", "");
            this.UserNameAndUrl = ClientInfo.Config.Get("compactShelf", "userNameAndUrl", "");
            this.TryInvoke(() =>
            {
                this.toolStripTextBox_codeExpireLength.Text = ClientInfo.Config.Get("compactShelf", "codeExpireLength", "24:00:00");
            });
        }

        void SaveSettings()
        {
            //ClientInfo.Config.Set("chat", "uiState", this.UiState);
            ClientInfo.Config.Set("compactShelf", "userNameAndUrl", this.UserNameAndUrl);
            var value = this.TryGet(() =>
            {
                return this.toolStripTextBox_codeExpireLength.Text;
            });
            ClientInfo.Config.Set("compactShelf", "codeExpireLength", value);
        }

        CancellationTokenSource _cancelSearch = new CancellationTokenSource();

        void EnableSearchButtons(bool bEnable)
        {
            try
            {
                this.Invoke((Action)(() =>
                {
                    this.toolStrip1.Enabled = bEnable;
                }));
            }
            catch (InvalidOperationException)
            {
                // 可能是因为窗口被摧毁后调用的原因
            }
        }

        string GetShelfAccount()
        {
            var parts = StringUtil.ParseTwoPart(this.UserNameAndUrl, "@");
            return parts[0];
        }

#if REMOVED
        private async void toolStripButton_command_Click(object sender, EventArgs e)
        {
            var command = InputDlg.GetInput(this,
                "命令",
                "命令行:",
                "compactShelf open 101A", null);
            if (command == null)
                return;

            string strError = "";

            _cancelSearch = new CancellationTokenSource();
            CancellationToken token = _cancelSearch.Token;

            EnableSearchButtons(false);
            try
            {
                var get_result = await ConnectionPool.OpenConnectionAsync(this.UserNameAndUrl);
                if (get_result.Value == -1)
                {
                    strError = get_result.ErrorInfo;
                    goto ERROR1;
                }

                var connection = get_result.Connection;

                string remoteUserName = GetShelfAccount();

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
#endif

        public static string DriverUrl
        {
            get
            {
                return ClientInfo.Config.Get("compactShelf",
                    "driverUrl",
                    "http://localhost:8733/DFServer/RestFullWcf");
            }
            set
            {
                ClientInfo.Config.Set("compactShelf",
                    "driverUrl",
                    value);
            }
        }

        private void toolStripButton_setControlParameters_Click(object sender, EventArgs e)
        {
            var result = InputDlg.GetInput(this,
    "控制参数",
    "驱动 URL:",
    DriverUrl,
    null);
            if (result == null)
                return;
            DriverUrl = result;
        }

        // 上一次现场码所对应的时间
        // string _lastTime = null;
        DateTime _lastTime = DateTime.MinValue;
        // 现场码
        static string _nearCode = null;

        void StopUpdateNearCode()
        {
            _cancel?.Cancel();
            _cancel = null;
            ClearNearCode();
            ClearAnimation();
        }

        void StartUpdateNearCode()
        {
            _cancel?.Cancel();
            _cancel = new CancellationTokenSource();
            _ = Task.Factory.StartNew(() =>
            {
                UpdateNearCode(_cancel.Token);
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        TimeSpan CodeExpireLength
        {
            get
            {
                string value = this.TryGet(() =>
                {
                    return this.toolStripTextBox_codeExpireLength.Text;
                });

                if (string.IsNullOrEmpty(value))
                    return TimeSpan.Zero;
                // https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-timespan-format-strings
                if (TimeSpan.TryParseExact(value,
                    new string[] {
                    "%d\\.%h\\:%m\\:%s",
                    "%h\\:%m\\:%s",
                    "%m\\:%s",
                    "%s",
                    "%d\\.%h\\:%m",
                    "%d\\.%h",
                    "%d\\.",
                    },
                    CultureInfo.InvariantCulture,
                    out TimeSpan length) == false)
                    throw new ArgumentException($"时间长度值 '{value}' 不合法");
                return length;
            }
            set
            {
                this.TryInvoke(() =>
                {
                    if (value == TimeSpan.Zero)
                        this.toolStripTextBox_codeExpireLength.Text = "";
                    else
                        this.toolStripTextBox_codeExpireLength.Text = value.ToString();
                });
            }
        }

        public string AnimationText
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.toolStripStatusLabel_alive.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.toolStripStatusLabel_alive.Text = value;
                });
            }
        }

        void ClearAnimation()
        {
            AnimationText = " ";
        }

        CharAnimation _animation = new CharAnimation("▁▂▃▅▆▇▉");

        void UpdateNearCode(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                // 动画
                AnimationText = _animation.Rotate();
                var delta = DateTime.Now - _lastTime;
                try
                {
                    var length = CodeExpireLength;

                    if (delta > length)
                    {
                        _lastTime = DateTime.Now;
                        NewNearCode();
                    }
                    _errorTable.SetError("nearCode", null);
                }
                catch (ArgumentException ex)
                {
                    _errorTable.SetError("nearCode", " " + ex.Message);
                    /*
                    this.TryInvoke(() =>
                    {
                        MessageBox.Show(this, ex.Message);
                    });
                    */
                }
#if REMOVED
                var time_string = DateTime.Now.ToString("yyyyMMdd_HH"); // 每小时变化一次
                // DateTime.Now.ToString("yyyyMMdd_HHmm");   // DateTimeUtil.DateTimeToString8(DateTime.Now);
                if (time_string != _lastTime)
                {
                    _lastTime = time_string;

                    NewNearCode();
                }
#endif

                Thread.Sleep(1000);
            }
        }

        void NewNearCode()
        {
            var random = new Random();
            _nearCode = random.Next(1, 9999).ToString().PadLeft(4, '0');

            this.TryInvoke(() =>
            {
                this.label_code.Text = _nearCode;
            });
        }

        void ClearNearCode()
        {
            _lastTime = DateTime.MinValue;
            _nearCode = null;
            this.TryInvoke(() =>
            {
                this.label_code.Text = _nearCode;
            });
        }

        // 现场码
        static string NearCode
        {
            get
            {
                return _nearCode;
            }
        }

        string GetStatusMessage()
        {
            return this.TryGet(() =>
            {
                return this.toolStripStatusLabel1.Text;
            });
        }

        void SetStatusMessage(string text, string color = "")
        {
            this.TryInvoke(() =>
            {
                this.toolStripStatusLabel1.Text = text;
                if (color == "red")
                {
                    this.toolStripStatusLabel1.BackColor = Color.DarkRed;
                    this.toolStripStatusLabel1.ForeColor = Color.White;
                }
                else
                {
                    this.toolStripStatusLabel1.BackColor = SystemColors.Control;
                    this.toolStripStatusLabel1.ForeColor = SystemColors.ControlText;
                }
            });
        }

        private void toolStripTextBox_codeExpireLength_Validating(object sender, CancelEventArgs e)
        {
            // 2023/8/16
            // 这里的验证其实要到对话框关闭时候才被触发，意义不大了，因此注释掉了
            /*
            var value = this.toolStripTextBox_codeExpireLength.Text;
            if (string.IsNullOrEmpty(value) == false
                && TimeSpan.TryParse(value, out TimeSpan length) == false)
            {
                MessageBox.Show(this, $"时间长度值 '{value}' 格式错误。请重新输入");
                e.Cancel = true;
            }
            */
        }

        /*
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.splitContainer_main,
                    this.dpTable_messages,
                    this.dpTable_groups,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.splitContainer_main,
                    this.dpTable_messages,
                    this.dpTable_groups,
                };
                GuiState.SetUiState(controls, value);
            }
        }
        */
    }

    public class CharAnimation
    {
        int _index = 0; // -1 表示不进行字符动画
        char[] movingChars = new char[] { '/', '-', '\\', '|' };

        public CharAnimation()
        {
            _index = 0;
        }

        public CharAnimation(string chars)
        {
            _index = 0;
            SetChars(chars);
        }


        public void Enable()
        {
            _index = 0;
        }

        public void Disable()
        {
            _index = -1;
        }

        public void SetChars(string chars)
        {
            movingChars = chars.ToArray();
        }

        public string Rotate()
        {
            if (_index != -1)
            {
                var text = new string(movingChars[_index], 1);
                _index++;
                if (_index > movingChars.Length - 1)
                    _index = 0;
                return text;
            }

            return null;
        }
    }
}
