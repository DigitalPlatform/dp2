using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Collections;
using System.Windows;
using System.Deployment.Application;
using System.IO;
using System.Reflection;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.Threading;
using Z.Expressions;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.WPF;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using DigitalPlatform.SimpleMessageQueue;
using DigitalPlatform.RFID;
using dp2SSL.Models;

namespace dp2SSL
{
    public static class TinyServer
    {
        #region 消息队列

        static MessageQueue _queue = null;

        public static async Task InitialMessageQueueAsync(string databaseFileName,
            CancellationToken token)
        {
            _queue = new MessageQueue(databaseFileName, false);
            await _queue.EnsureCreatedAsync();
        }

        static Task _sendTask = null;

        // 同步重试间隔时间
        static TimeSpan _idleLength = TimeSpan.FromMinutes(5);   // 5 // TimeSpan.FromSeconds(10);

        static AutoResetEvent _eventSend = new AutoResetEvent(false);

        public static void ActivateSend()
        {
            _eventSend.Set();
        }

        // 启动发送消息任务。此任务长期在后台运行
        public static void StartSendTask(CancellationToken token = default)
        {
            if (_sendTask != null)
                return;

            token.Register(() =>
            {
                _eventSend.Set();
            });

            _sendTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("消息发送专用线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        _eventSend.WaitOne(_idleLength);
                        token.ThrowIfCancellationRequested();

                        // 如果暂时没有配置消息服务器
                        if (string.IsNullOrEmpty(App.messageServerUrl))
                            continue;

                        // 检查 dp2mserver 相关参数的合法性
                        var check_result = App.CurrentApp.CheckMessageServerParameters();
                        if (check_result.Value == -1)
                        {
                            App.SetError("messageServer", $"消息服务器参数配置错误: {check_result.ErrorInfo}");
                            continue;
                        }
                        else
                            App.SetError("messageServer", null);

                        // 检查和确保连接到消息服务器
                        bool connected = await App.EnsureConnectMessageServerAsync();

                        if (connected)
                        {
                            while (token.IsCancellationRequested == false)
                            {
                                try
                                {
                                    var message = await _queue.PeekAsync(token);
                                    if (message == null)
                                        break;
                                    var request = JsonConvert.DeserializeObject<SetMessageRequest>(message.GetString());

                                    // 对 request 中的 groups 进行必要的变换
                                    foreach (var record in request.Records)
                                    {
                                        if (record.groups != null
                                            && record.groups.Length == 1
                                            && record.groups[0] == null)
                                        {
                                            record.groups = null;
                                        }
                                    }

                                    /*
                                    // 2020/4/29
                                    // 对 request 中的 groups 进行必要的变换
                                    foreach(var record in request.Records)
                                    {
                                        if (record.groups == null)
                                        {

                                        }
                                    }
                                    */

                                    var result = await SetMessageAsync(request);
                                    if (result.Value == -1)
                                    {
                                        // 为了让用户引起警觉，最好显示到界面报错
                                        App.SetError("sendMessage", $"同步发送消息出错: {result.ErrorInfo}");

                                        // TODO: 错误日志中要写入消息内容
                                        WpfClientInfo.WriteLogInternal("error", $"SetMessageAsync() 出错(本条消息已被跳过，不会再重试发送): {result.ErrorInfo}");
                                        break;
                                    }
                                    else
                                        App.SetError("sendMessage", null);

                                    await _queue.PullAsync(token);
                                }
                                catch (Exception ex)
                                {
                                    // WpfClientInfo.WriteErrorLog($"发送消息过程中出现异常(不会终止循环): {ExceptionUtil.GetDebugText(ex)}");
                                    // 避免错误日志太多把错误日志文件塞满
                                    _ = GlobalMonitor.CompactLog.Add("发送消息过程中出现异常(不会终止循环): {0}", new object[] { ExceptionUtil.GetDebugText(ex) });
                                    break;
                                }
                            }
                        }
                    }
                    _sendTask = null;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"消息发送专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("send", $"消息发送专用线程出现异常: {ex.Message}");
                }
                finally
                {
                    WpfClientInfo.WriteInfoLog("消息发送专用线程结束");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        public static async Task SendMessageAsync(string group, string content)
        {
            string[] groups = null;
            if (group != null)
                groups = new string[] { group };
            await SendMessageAsync(groups, content);
        }

        // 注意，groups 可能为空。表示当前 dp2mserver 用户所参与的所有群
        public static async Task SendMessageAsync(string[] groups,
            string content)
        {
            // 2020/8/20
            if (_queue == null)
                return;

            // 2021/9/6
            if (groups != null && groups.Length == 1 && groups[0] == null)
                throw new ArgumentException("groups 参数不允许使用 [0] == null 方式调用", "groups");

            SetMessageRequest request = new SetMessageRequest("create", "dontNotifyMe",
                new List<MessageRecord> {
                        new MessageRecord {
                            groups = groups,    // new string[] { groupName},
                            data = content}
                });

            // 2020/4/27 增加 chunk 能力
            {
                int chunk_size = 4096;
                int length = GetLength(request);
                if (length < chunk_size)
                {
                    await _queue.PushAsync(new List<string> { JsonConvert.SerializeObject(request) }).ConfigureAwait(false);
                    ActivateSend();
                    return;
                }

                // SetMessageResult result = null;
                foreach (MessageRecord record in request.Records)
                {
                    string taskID = Guid.NewGuid().ToString();
                    string data = record.data;
                    int send = 0;
                    for (; ; )
                    {
                        SetMessageRequest current_request = new SetMessageRequest();
                        current_request.TaskID = taskID;
                        current_request.Style = request.Style;
                        current_request.Action = request.Action;
                        current_request.Records = new List<MessageRecord>();
                        MessageRecord current_record = new MessageRecord();
                        // TODO: 除了第一次请求外，其它的都只要 .data 成员具备即可
                        current_record.CopyFrom(record);
                        current_record.data = data.Substring(send, Math.Min(chunk_size, data.Length - send));
                        current_request.Records.Add(current_record);
                        // 这一次就是最后一次
                        if (send + current_record.data.Length >= data.Length)
                        {
                            MessageRecord tail_record = new MessageRecord();
                            tail_record.data = null;    // 表示结束
                            current_request.Records.Add(tail_record);
                        }

                        await _queue.PushAsync(new List<string> { JsonConvert.SerializeObject(current_request) }).ConfigureAwait(false);
                        /*
                        result = await TrySetMessageAsync(current_request).ConfigureAwait(false);
                        if (result.Value == -1)
                            return result;  // 中途出错了
                            */

                        send += current_record.data.Length;
                        if (send >= data.Length)
                            break;
                    }
                }
            }
            ActivateSend();
            // return SetMessageAsync(request);
        }

        static int GetLength(SetMessageRequest request)
        {
            if (request.Records == null)
                return 0;
            int length = 0;
            foreach (MessageRecord record in request.Records)
            {
                if (record.data != null)
                    length += record.data.Length;
            }

            return length;
        }


        #endregion

        static IHubProxy HubProxy
        {
            get;
            set;
        }

        static HubConnection Connection
        {
            get;
            set;
        }

        public static void CloseConnection()
        {
            if (Connection == null)
                return;

            DisposeHandlers();

            if (Connection != null)
            {
                Connection.Reconnected -= Connection_Reconnected;

                Connection.Stop();
                Connection.Dispose();
                Connection = null;
            }

            _userName = "";
        }

        static List<IDisposable> _handlers = new List<IDisposable>();

        static void DisposeHandlers()
        {
            foreach (IDisposable handler in _handlers)
            {
                if (handler != null)
                    handler.Dispose();
            }

            _handlers.Clear();
        }

        public static bool IsDisconnected
        {
            get
            {
                if (Connection == null)
                    return true;
                return Connection.State == ConnectionState.Disconnected;
            }
        }

        private static readonly Object _syncRoot = new Object();

        static string _userName = "";

        public static async Task<NormalResult> ConnectAsync(string url,
            string userName,
            string password,
            string parameters)
        {
            lock (_syncRoot)
            {
                CloseConnection();

                Connection = new HubConnection(url);

                Connection.Reconnected += Connection_Reconnected;

                // 一直到真正连接前才触发登录事件
                //if (this.Container != null)
                //    this.Container.TriggerLogin(this);

                //if (this.Container != null && this.Container.TraceWriter != null)
                //    Connection.TraceWriter = this.Container.TraceWriter;

                // Connection.Credentials = new NetworkCredential("testusername", "testpassword");
                Connection.Headers.Add("username", userName);
                Connection.Headers.Add("password", password);
                Connection.Headers.Add("parameters", parameters);

                HubProxy = Connection.CreateHubProxy("MyHub");

                {
                    var handler = HubProxy.On<string, IList<MessageRecord>>("addMessage",
                        (name, messages) =>
                        OnAddMessageRecieved(name, messages)
                        );
                    _handlers.Add(handler);
                }

                // *** search
                {
                    var handler = HubProxy.On<SearchRequest>("search",
                    (param) => OnSearchRecieved(param)
                    );
                    _handlers.Add(handler);
                }

                // *** setInfo
                {
                    var handler = HubProxy.On<SetInfoRequest>("setInfo",
                    (param) => OnSetInfoRecieved(param)
                    );
                    _handlers.Add(handler);
                }

                // *** getRes
                {
                    var handler = HubProxy.On<GetResRequest>("getRes",
                    (param) => OnGetResRecieved(param)
                    );
                    _handlers.Add(handler);
                }

                /*
                // 2021/8/27
                // *** circulation
                {
                    var handler = HubProxy.On<CirculationRequest>("circulation",
                    (param) => OnCirculationRecieved(param)
                    );
                    _handlers.Add(handler);
                }
                */

                // 2020/9/29
                // *** close
                {
                    var handler = HubProxy.On<CloseRequest>("close",
                    (param) =>
                    {
                        if (param.Action == "reconnect")
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await ConnectAsync(url,
                                        userName,
                                        password,
                                        parameters);
                                }
                                catch (Exception ex)
                                {
                                    WpfClientInfo.WriteErrorLog($"(close handler) ConnectAsync() 出现异常:{ExceptionUtil.GetDebugText(ex)}");
                                }
                            });
                            // _ = App.ConnectMessageServerAsync(); // 不用等待完成
                        }
                        else
                            CloseConnection();
                    });
                    _handlers.Add(handler);
                }

                /*
                // *** webCall
                {
                    var handler = HubProxy.On<WebCallRequest>("webCall",
                    (param) => OnWebCallRecieved(param)
                    );
                    _handlers.Add(handler);
                }

                */
            }

            try
            {
                await Connection.Start().ConfigureAwait(false);

                _userName = userName;

                {
                    /*
                    _exiting = false;
                    AddInfoLine("成功连接到 " + this.ServerUrl);
                    TriggerConnectionStateChange("Connected");
                    */
                    return new NormalResult();
                }
            }
            catch (HttpRequestException ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = "HttpRequestException"
                };
            }
            catch (Microsoft.AspNet.SignalR.Client.HttpClientException ex)
            {
                Microsoft.AspNet.SignalR.Client.HttpClientException ex0 = ex as Microsoft.AspNet.SignalR.Client.HttpClientException;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = ex0.Response.StatusCode.ToString()
                };
            }
            catch (AggregateException ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = GetExceptionText(ex)
                };
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ExceptionUtil.GetExceptionText(ex)
                };
            }
        }

        private static void Connection_Reconnected()
        {
            _ = LoadGapMessageAsync();
        }

#if REMOVED
        // 准备群名列表
        public static async Task<NormalResult> PrepareGroupNames()
        {
            if (string.IsNullOrEmpty(_userName))
                throw new ArgumentException("_userName 为空");

            var result = await GetUsersAsync(_userName, 0, -1);
            if (result.Value == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo
                };
            if (result.Users == null || result.Users.Count == 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"当前 dp2mserver 服务器中不存在名为 '{_userName}' 的用户"
                };

            _groups = new List<string>(result.Users[0].groups);

            return new NormalResult();
        }

        static List<string> _groups = null;

        public static string[] GroupNames
        {
            get
            {
                if (_groups == null)
                    return null;
                return _groups.ToArray();
            }
        }
#endif

        public static string GetExceptionText(AggregateException exception)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                if (ex is AggregateException)
                    text.Append(GetExceptionText(ex as AggregateException));
                else
                    text.Append(ex.Message + "\r\n");
                // text.Append(ex.ToString() + "\r\n");
            }

            return text.ToString();
        }

        #region 接收和处理聊天消息

        // 收到消息。被当作命令解释。执行后发回命令执行结果
        static void OnAddMessageRecieved(string action,
IList<MessageRecord> messages)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (action == "create")
                        await ProcessMessages(messages);
                }
                catch(Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"ProcessMessages() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        static async Task ProcessMessages(IList<MessageRecord> messages)
        {
            try
            {
                foreach (var message in messages)
                {
                    // TODO: 忽略自己发出的消息?
                    if (message.data.StartsWith($"@{_userName} ")
                        && _instantMessageIDs.ContainsKey(message.id) == false)
                    {
                        string command = message.data.Substring($"@{_userName} ".Length).Trim();

                        if (message.groups != null && message.groups.Length > 0)
                            await ProcessCommandAsync(command, message.groups[0]);

                        _instantMessageIDs[message.id] = message.publishTime;
                        ClearOldIDs();
                    }

                    _lastMessage = message;
                }
            }
            catch (Exception ex)
            {
                // 写入错误日志
                WpfClientInfo.WriteErrorLog($"ProcessMessages() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        public delegate void Delegate_outputMessage(
StringBuilder cache,
long totalCount,
long start,
IList<MessageRecord> records,
string errorInfo,
string errorCode);

        public static async Task<MessageResult> GetMessageAsyncLite(
GetMessageRequest request,
Delegate_outputMessage proc,
TimeSpan timeout,
CancellationToken token)
        {
            MessageResult result = new MessageResult();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            long recieved = 0;

            StringBuilder cache = new StringBuilder();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<
                    string, long, long, IList<MessageRecord>, string, string>(
                    "responseGetMessage",
                    (taskID, resultCount, start, records, errorInfo, errorCode) =>
                    {
                        if (taskID != request.TaskID)
                            return;

                        if (resultCount == -1 || start == -1)
                        {
                            if (start == -1)
                            {
                                // 表示发送响应过程已经结束。只是起到通知的作用，不携带任何信息
                                // result.Finished = true;
                            }
                            else
                            {
                                result.Value = resultCount;
                                result.ErrorInfo = errorInfo;
                                result.String = errorCode;
                            }
                            wait_events.finish_event.Set();
                            return;
                        }

                        proc(
                            cache,
                            resultCount,
                            start,
                            records,
                            errorInfo,
                            errorCode);

                        if (records != null)
                            recieved += GetCount(records);  // records.Count;

                        if (errorCode == "_complete")
                        {
                            result.Value = resultCount;
                            wait_events.finish_event.Set();
                            return;
                        }

                        if (resultCount >= 0 &&
                            IsComplete(request.Start, request.Count, resultCount, recieved) == true)
                            wait_events.finish_event.Set();
                        else
                            wait_events.active_event.Set();
                    }))
                {
                    MessageResult temp = await HubProxy.Invoke<MessageResult>(
"RequestGetMessage",
request).ConfigureAwait(false);
                    if (temp.Value == -1 || temp.Value == 0 || temp.Value == 2)
                        return temp;

                    // result.String 里面是返回的 taskID

                    await WaitAsync(
    request.TaskID,
    wait_events,
    timeout,
    token).ConfigureAwait(false);
                    return result;
                }
            }
        }

        static bool IsComplete(long requestStart,
long requestCount,
long totalCount,
long recordsCount)
        {
            long tail = 0;
            if (requestCount != -1)
                tail = Math.Min(requestStart + requestCount, totalCount);
            else
                tail = totalCount;

            if (requestStart + recordsCount >= totalCount)
                return true;
            return false;
        }

        // 计算列表中完满元素的个数。所谓完满元素就是 id 不是空的元素
        static int GetCount(IList<MessageRecord> records)
        {
            int count = 0;
            foreach (MessageRecord record in records)
            {
                if (string.IsNullOrEmpty(record.id) == false)
                    count++;
            }

            return count;
        }

        internal class WaitEvents : IDisposable
        {
            public ManualResetEvent finish_event = new ManualResetEvent(false);    // 表示数据全部到来
            public AutoResetEvent active_event = new AutoResetEvent(false);    // 表示中途数据到来

            public virtual void Dispose()
            {
                finish_event.Dispose();
                active_event.Dispose();
            }
        }


        static Task WaitAsync(string taskID,
    WaitEvents wait_events,
    TimeSpan timeout,
    CancellationToken cancellation_token)
        {
            return TaskRunAction(
    () =>
    {
        Wait(taskID, wait_events, timeout, cancellation_token);
    },
cancellation_token);
        }

        static void Wait(string taskID,
            WaitEvents wait_events,
            TimeSpan timeout,
            CancellationToken cancellation_token)
        {
            DateTime start_time = DateTime.Now; // 其实可以不用

            WaitHandle[] events = null;

            if (cancellation_token != null)
            {
                events = new WaitHandle[3];
                events[0] = wait_events.finish_event;
                events[1] = wait_events.active_event;
                events[2] = cancellation_token.WaitHandle;
            }
            else
            {
                events = new WaitHandle[2];
                events[0] = wait_events.finish_event;
                events[1] = wait_events.active_event;
            }

            while (true)
            {
                int index = WaitHandle.WaitAny(events,
                    timeout,
                    true); // false

                if (index == 0) // 正常完成
                    return; //  result;
                else if (index == 1)
                {
                    start_time = DateTime.Now;  // 重新计算超时开始时刻
                    Debug.WriteLine("重新计算超时开始时间 " + start_time.ToString());
                }
                else if (index == 2)
                {
                    if (cancellation_token != null)
                    {
                        // 向服务器发送 CancelSearch 请求
                        CancelSearchAsync(taskID);
                        cancellation_token.ThrowIfCancellationRequested();
                    }
                }
                else if (index == WaitHandle.WaitTimeout)
                {
                    // if (DateTime.Now - start_time >= timeout)
                    {
                        Debug.WriteLine("超时。delta=" + (DateTime.Now - start_time).TotalSeconds.ToString());
                        // 向服务器发送 CancelSearch 请求
                        CancelSearchAsync(taskID);
                        throw new TimeoutException("已超时 " + timeout.ToString());
                    }
                }
            }
        }

        // 请求服务器中断一个 task
        public static Task<MessageResult> CancelSearchAsync(string taskID)
        {
            return HubProxy.Invoke<MessageResult>(
                "CancelSearch",
                taskID);
        }

        public static Task TaskRunAction(Action function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(
                function,
                cancellationToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        // 当前获得的最后一条消息
        static MessageRecord _lastMessage = null;

        // 消息 ID --> 消息创建时间
        static Hashtable _instantMessageIDs = new Hashtable();

        static int _inGetMessage = 0;  // 防止因为 ConnectionStateChange 事件导致重入

        // 主动装载间隙期间的消息
        public static async Task LoadGapMessageAsync()
        {
            if (_lastMessage != null)
            {
                var strStartDate = _lastMessage.publishTime.ToString("G");
                await LoadMessageAsync(_lastMessage.groups[0],
        strStartDate,
        "");
            }
        }

        // 清除 _instantMessageIDs 中较旧的条目。避免空间膨胀失控
        static void ClearOldIDs()
        {
            DateTime now = DateTime.Now;
            TimeSpan delta = TimeSpan.FromMinutes(5);

            List<string> delete_keys = new List<string>();
            foreach (string key in _instantMessageIDs.Keys)
            {
                object value = _instantMessageIDs[key];
                if (value == null)
                    continue;
                var time = (DateTime)value;
                if (now - time > delta)
                {
                    delete_keys.Add(key);
                    // _instantMessageIDs.Remove(key);
                }
            }

            // 2021/9/8
            if (delete_keys.Count > 0)
            {
                foreach(var key in delete_keys)
                {
                    _instantMessageIDs.Remove(key);
                }
            }

            // 实在太多了干脆全部清除
            if (_instantMessageIDs.Count > 1024 * 10)
                _instantMessageIDs.Clear();
        }

        // 获得指定时间范围的消息
        // 装载已经存在的消息记录
        static async Task LoadMessageAsync(string strGroupName,
            string strStartDate,
            string strEndDate
            // string strStyle
            )
        {
            if (_inGetMessage > 0)
                return;

            _inGetMessage++;
            try
            {
                string strError = "";

#if NO
                // TODO: 如果当前 Connection 尚未连接，则要促使它连接，然后重试 load
                if (Program.MainForm.MessageHub.IsConnected == false)
                {
                    if (_redoLoadMesssageCount < 5)
                    {
                        AddErrorLine("当前点对点连接尚未建立。重试操作中 ...");
                        Program.MainForm.MessageHub.Connect();
                        Thread.Sleep(5000);
                        _redoLoadMesssageCount++;
                        // await Task.Factory.StartNew(() => DoLoadMessage(strGroupName, strTimeRange, bClearAll));
                        await DoLoadMessage(strGroupName, strTimeRange, bClearAll);
                        return;
                    }
                    else
                    {
                        AddErrorLine("当前点对点连接尚未建立。停止重试。消息装载失败。");
                        _redoLoadMesssageCount = 0; // 以后再调用本函数，就重新计算重试次数
                        return;
                    }
                }
#endif

                {
                    CancellationToken cancel_token = new CancellationToken();

                    string id = Guid.NewGuid().ToString();
                    GetMessageRequest request = new GetMessageRequest(id,
                        strGroupName, // "<default>" 表示默认群组
                        "",
                        strStartDate + "~" + strEndDate,
                        0,
                        -1);
                    try
                    {
                        MessageResult result = await GetMessageAsyncLite(
                            request,
                            FillMessage,
                            new TimeSpan(0, 1, 0),
                            cancel_token);
                        if (result.Value == -1)
                        {
                            //strError = result.ErrorInfo;
                            //goto ERROR1;
                            return;
                        }

                        // 成功
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
                    return;
                }

            ERROR1:
                //this.Invoke((Action)(() => MessageBox.Show(this, strError)));
                return;
            }
            finally
            {
                _inGetMessage--;
            }
        }

        // 拼接后 data 的最大长度
        const int MAX_MESSAGE_DATA_LENGTH = 1024 * 1024;

        static void FillMessage(
    StringBuilder cache,
    long totalCount,
    long start,
    IList<MessageRecord> records,
    string errorInfo,
    string errorCode)
        {
            if (totalCount == -1)
            {
                StringBuilder text = new StringBuilder();
                text.Append("***\r\n");
                text.Append("totalCount=" + totalCount + "\r\n");
                text.Append("errorInfo=" + errorInfo + "\r\n");
                text.Append("errorCode=" + errorCode + "\r\n");

                return;
            }

            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    string data = "";   // 拼接完成的 data
                    if (string.IsNullOrEmpty(record.id)
                        && cache.Length < MAX_MESSAGE_DATA_LENGTH)
                    {
                        cache.Append(record.data);
                        continue;
                    }
                    else
                    {
                        if (cache.Length > 0)
                        {
                            cache.Append(record.data);
                            data = cache.ToString();
                            cache.Clear();

                            record.data = data;
                        }
                    }

                    _ = ProcessMessages(new List<MessageRecord> { record });

                    /*
                    StringBuilder text = new StringBuilder();
                    text.Append("***\r\n");
                    text.Append("id=" + HttpUtility.HtmlEncode(record.id) + "\r\n");
                    text.Append("data=" + HttpUtility.HtmlEncode(record.data) + "\r\n");
                    if (record.data != null)
                        text.Append("data.Length=" + record.data.Length + "\r\n");

                    if (string.IsNullOrEmpty(data) == false)
                        text.Append("concated data=" + HttpUtility.HtmlEncode(data) + "\r\n");

                    if (record.groups != null)
                        text.Append("groups=" + HttpUtility.HtmlEncode(string.Join(",", record.groups)) + "\r\n");
                    text.Append("creator=" + HttpUtility.HtmlEncode(record.creator) + "\r\n");
                    text.Append("userName=" + HttpUtility.HtmlEncode(record.userName) + "\r\n");

                    text.Append("format=" + HttpUtility.HtmlEncode(record.format) + "\r\n");
                    text.Append("type=" + HttpUtility.HtmlEncode(record.type) + "\r\n");
                    text.Append("thread=" + HttpUtility.HtmlEncode(record.thread) + "\r\n");

                    if (record.subjects != null)
                        text.Append("subjects=" + HttpUtility.HtmlEncode(string.Join(SUBJECT_DELIM, record.subjects)) + "\r\n");

                    text.Append("publishTime=" + HttpUtility.HtmlEncode(record.publishTime.ToString("G")) + "\r\n");
                    text.Append("expireTime=" + HttpUtility.HtmlEncode(record.expireTime) + "\r\n");
                    AppendHtml(this.webBrowser_message, text.ToString());
                    */
                }
            }
        }


        #endregion

        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        static void OnSearchBiblioRecieved(SearchRequest param)
        {
        }

        /*
        static string GroupName
        {
            get
            {
                return App.messageGroupName;
            }
        }
        */

        static async Task ProcessCommandAsync(string command, string groupName)
        {
            /*
            // 测试 让 dp2mserver 清除当前通道的 ConnectionInfo
            if (command.StartsWith("test"))
            {
                GetConnectionInfoRequest request = new GetConnectionInfoRequest
                {
                    QueryWord = "!myself",
                    Operation = "clear"
                };

                var result = await GetConnectionInfoAsync(request,
                    TimeSpan.FromSeconds(30),
                    new CancellationToken());
                string text = $"result.ResultCount={result.ResultCount},result.ErrorInfo={result.ErrorInfo}";

                await SendMessageAsync(new string[] { groupName }, $"{text}");
                return;
            }
            */

            if (command.StartsWith("hello"))
            {
                await SendMessageAsync(groupName, $"hello! 硬件时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
                return;
            }

            if (command.StartsWith("version"))
            {
                await SendMessageAsync(groupName, $"dp2SSL 前端版本: {WpfClientInfo.ClientVersion}");
                return;
            }

            if (command.StartsWith("error"))
            {
                await SendMessageAsync(groupName, $"dp2SSL 当前界面报错: [{App.CurrentApp.Error}]; 书柜初始化是否完成: {ShelfData.FirstInitialized}");
                return;
            }

            // 2021/7/1
            if (command.StartsWith("checktime"))
            {
                var result = LibraryChannelUtil.CheckServerClock(TimeSpan.FromSeconds(5));
                await SendMessageAsync(groupName, $"{result.ErrorInfo}");
                return;
            }

            if (command.StartsWith("help") || command.StartsWith("?"))
            {
                await SendMessageAsync(groupName,
                    @"可用命令如下:
hello
version
error
checktime
dialog
press 按钮文字
led 命令参数
rebuild patron cache
list history
change history
check book
check patron
check tag
write tag
set lamp time
sterilamp
exit
restart
update
"
);
                return;
            }

            // 触发按钮
            if (command.StartsWith("press"))
            {
                string param = command.Substring("press".Length).Trim();
                NormalResult press_result = null;
                App.Invoke(new Action(() =>
                {
                    press_result = App.PressButton(param);
                }));
                if (press_result.Value == -1)
                    await SendMessageAsync(groupName, $"触发按钮发生错误: {press_result.ErrorInfo}");
                else
                    await SendMessageAsync(groupName, $"触发按钮成功");
                return;
            }

            // 显示当前对话框的内容
            if (command.StartsWith("dialog"))
            {
                string param = command.Substring("dialog".Length).Trim();
                string text = "";
                App.Invoke(new Action(() =>
                {
                    var window = App.GetActiveWindow();
                    if (window == null)
                        text = "";
                    else
                        text = App.FindTextChildren(window);
                }));
                await SendMessageAsync(groupName, $"==== 对话框文字 ====\r\n{text}");
                return;
            }

            // 让书柜说话
            if (command.StartsWith("speak"))
            {
                string param = command.Substring("speak".Length).Trim();

                App.CurrentApp.SpeakSequence(param);

                await SendMessageAsync(groupName, $"正在朗读: {param}");
                return;
            }

            // 在 LED 屏上显示文字
            if (command.StartsWith("led"))
            {
                string param = command.Substring("led".Length).Trim();
                App.LedText = param;    // 保存起来
                await LedDisplayAsync(param, groupName);
                return;
            }

            // 2020/11/7
            // 设置每日亮灯时间段
            if (command.StartsWith("set lamp time"))
            {
                string param = command.Substring("set lamp time".Length).Trim();
                string old_param = LampPerdayTask.GetPerdayTask();
                var result = LampPerdayTask.ChangePerdayTask(param);
                if (result.Value == -1)
                    await SendMessageAsync(groupName, $"设置每日亮灯时间范围时出错: {result.ErrorInfo}");
                else
                    await SendMessageAsync(groupName, $"已设置每日亮灯时间范围 {param}。(上次的时间范围是 {old_param})");
                return;
            }

            // 开灯、关灯
            // lamp 获得灯状态
            // lamp on 开灯
            // lanp off 关灯
            // 注意，这是控制背景灯。开关门引起的开灯关灯会和背景灯变量叠加，最后决定灯的亮灭
            if (command.StartsWith("lamp"))
            {
                string param = command.Substring("lamp".Length).Trim();
                if (string.IsNullOrEmpty(param))
                {
                    var state = LampPerdayTask.GetBackLampState();
                    await SendMessageAsync(groupName, $"当前灯状态为 {(state ? "亮" : "灭")}");
                    return;
                }

                param = param.ToLower();
                if (param == "on")
                    LampPerdayTask.TurnBackLampOn();
                else
                    LampPerdayTask.TurnBackLampOff();
                await SendMessageAsync(groupName, param == "on" ? "已开灯" : "已关灯");
                return;
            }

            // 2020/9/25
            // 重建读者本地缓存
            if (command.StartsWith("rebuild patron cache"))
            {
                ShelfData.RedoReplicatePatron();
                await SendMessageAsync(groupName, $"已启动重建读者本地缓存任务");
                return;
            }

            // 2020/9/25
            // 立即更新版本
            if (command.StartsWith("update"))
            {
                GlobalMonitor.ReturnUpateResult = true; // 2021/9/1
                GlobalMonitor.ActivateUpdate();
                return;
            }

            // 列出操作历史
            if (command.StartsWith("list history"))
            {
                await ListHistoryAsync(command, groupName);
                return;
            }

            // 列出图书
            if (command.StartsWith("list book"))
            {
                await ListBookAsync(command, groupName);
                return;
            }

            // 修改操作历史
            if (command.StartsWith("change history"))
            {
                await ChangeHistoryAsync(command, groupName);
                return;
            }

            // 检测 RFID 标签(以柜门为单位)
            if (command.StartsWith("check tag"))
            {
                await CheckTagAsync(command, groupName);
                return;
            }

            // 写入 RFID 标签
            if (command.StartsWith("write tag"))
            {
                await WriteTagAsync(command, null/*groupName*/);
                return;
            }

            // 检查册状态
            if (command.StartsWith("check book"))
            {
                await CheckBookAsync(command, groupName);
                return;
            }

            // 检查读者状态
            if (command.StartsWith("check patron"))
            {
                await CheckPatronAsync(command, groupName);
                return;
            }

            // 2021/1/20
            // 设置每日紫外灯时间点
            if (command.StartsWith("set sterilamp time"))
            {
                string param = command.Substring("set sterilamp time".Length).Trim();
                string old_param = SterilampTask.GetPerdayTask();
                var result = SterilampTask.ChangePerdayTask(param);
                if (result.Value == -1)
                    await SendMessageAsync(groupName, $"设置紫外灯时间时出错: {result.ErrorInfo}");
                else
                    await SendMessageAsync(groupName, $"已设置紫外灯时间 {param}。(上次的时间是 {old_param})");
                return;
            }

            // 开启紫外线杀菌
            if (command.StartsWith("sterilamp"))
            {
                string param = command.Substring("sterilamp".Length).Trim();

                // 若当前为书柜界面，则需要检查是否有打开的门
                if (IsInShelfPage() && ShelfData.OpeningDoorCount > 0)
                {
                    await SendMessageAsync(groupName, $"当前有 {ShelfData.OpeningDoorCount} 个柜门处于打开状态，因此拒绝执行 sterilamp 命令");
                    return;
                }

                if (param != null)
                    param = param.ToLower();

                if (string.IsNullOrEmpty(param)
                    || param == "on" || param == "begin" || param == "turnon")
                {
                    App.CurrentApp.BeginSterilamp();
                    return;
                }

                if (param == "off" || param == "end" || param == "stop" || param == "turnoff")
                {
                    App.CurrentApp.CancelSterilamp();
                    return;
                }

                await SendMessageAsync(groupName, $"sterilamp 命令无法执行：未知的参数 '{param}'");
                return;
            }

            // 退出 dp2ssl
            if (command.StartsWith("exit"))
            {
                await SendMessageAsync(groupName, $"即将退出 dp2ssl");
                App.Invoke(new Action(() =>
                {
                    WpfClientInfo.WriteInfoLog($"远程命令退出 dp2ssl");
                    Application.Current.Shutdown();
                }));
                return;
            }

            // 重新启动 dp2ssl
            if (command.StartsWith("restart"))
            {
                // 若当前为书柜界面，则需要检查是否有打开的门
                if (IsInShelfPage() && ShelfData.OpeningDoorCount > 0)
                {
                    await SendMessageAsync(groupName, $"当前有 {ShelfData.OpeningDoorCount} 个柜门处于打开状态，因此拒绝执行 restart 命令");
                    return;
                }

                // 子参数 默认 silently。若为 "interact" 则表示初始化时候要进行交互
                string param = command.Substring("restart".Length).Trim();

                WpfClientInfo.WriteInfoLog($"restart 命令参数：'{param}'");

                // 重启计算机
                if (ContainsParam(param,
                    (s) =>
                    {
                        return s.StartsWith("computer");
                    }))
                {
                    // 重启电脑
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 为 dp2ssl 开机后自动重启预先设定好 cmdlineparam.txt 文件
                            WriteParameterFile();

                            await Task.Delay(1000);
                            ShutdownUtil.DoExitWindows(ShutdownUtil.ExitWindows.Reboot);
                        }
                        catch (Exception ex)
                        {
                            WpfClientInfo.WriteErrorLog($"接受远程命令重启电脑过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        }
                    });
                    await SendMessageAsync(groupName, $"Windows 将在一秒后重新启动");
                    return;
                }

                bool silently = true;
                if (ContainsParam(param,
                    (s) =>
                    {
                        return s.StartsWith("interact");
                    }))
                    silently = false;

                // string ApplicationEntryPoint = null;
                if (ApplicationDeployment.IsNetworkDeployed == true)
                {
                    /*
                    ApplicationEntryPoint = ApplicationDeployment.CurrentDeployment?.UpdatedApplicationFullName;
                    WpfClientInfo.WriteInfoLog($"ApplicationDeployment.CurrentDeployment?.UpdatedApplicationFullName='{ApplicationEntryPoint}'");
                    // Process.Start(ApplicationEntryPoint);
                    */
                    await SendMessageAsync(groupName, $"ClickOnce 版本无法远程重启");
                    return;
                }

                string greensetup_path = "c:\\dp2ssl\\greensetup.exe";
                if (File.Exists(greensetup_path) == false)
                {
                    await SendMessageAsync(groupName, $"尚未安装绿色启动器 {greensetup_path}，无法对 dp2ssl.exe 进行远程重启");
                    return;
                }

                await SendMessageAsync(groupName, "开始重新启动。请等待至少 30 秒");
                App.Invoke(new Action(() =>
                {
                    Application.Current.Shutdown();
                    App.CurrentApp.CloseMutex();
                    // TODO: 测试一下是否可以起到升级的作用
                    // System.Diagnostics.Process.Start(Assembly.GetEntryAssembly().Location);
                    /*
                    if (string.IsNullOrEmpty(ApplicationEntryPoint) == false)
                        Process.Start(ApplicationEntryPoint);
                    else
                        System.Windows.Forms.Application.Restart();
                    */
                    // StartModule(ShortcutPath, "");

                    string args = silently ? "silently" : "interact";
                    WpfClientInfo.WriteInfoLog($"启动 {greensetup_path}，参数={args}");
                    Process.Start(greensetup_path,
                        args + " delay");  // 延迟 30 秒启动 dp2ssl，以便前一个 dp2ssl 进程能完全退出，避免错误日志文件出现 _001
                }));
                return;
            }

            await SendMessageAsync(groupName, $"我无法理解这个命令 '{command}'");
        }

        static bool IsInSettingPage()
        {
            bool result = false;
            App.Invoke(new Action(() =>
            {
                var nav = (NavigationWindow)App.Current.MainWindow;
                result = nav.Content.GetType() == typeof(PageSetting);
            }));
            return result;
        }


        // 当前是否正处在书柜页面
        static bool IsInShelfPage()
        {
            bool result = false;
            App.Invoke(new Action(() =>
            {
                var nav = (NavigationWindow)App.Current.MainWindow;
                result = nav.Content.GetType() == typeof(PageShelf);
            }));
            return result;
        }

        // 准备命令行参数文件
        static bool WriteParameterFile()
        {
            try
            {
                string binDir = "c:\\dp2ssl";
                if (Directory.Exists(binDir))
                {
                    string fileName = System.IO.Path.Combine(binDir, "cmdlineparam.txt");
                    File.WriteAllText(fileName, "silently");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"准备命令行参数文件时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return false;
            }
        }

        // -x:0
        // -y:0
        // -ledName:*
        // -text:显示文字
        // -fontSize:24
        // -effect:moveLeft
        // -moveSpeed:fast
        // -duration:1
        // -horzAlign:left
        // -vertAlign:top
        // -style:xxx
        public static async Task LedDisplayAsync(string param, string groupName)
        {
            string ledName = "*";
            DisplayStyle property = new DisplayStyle();
            int x = 0;
            int y = 0;
            string style = "";
            string text = "";

            List<string> parameters = StringUtil.SplitList(param, " ");
            foreach (string parameter in parameters)
            {
                string name = "";
                string value = "";
                if (parameter.StartsWith("-"))
                {
                    var parts = StringUtil.ParseTwoPart(parameter.Substring(1), ":");
                    name = parts[0].ToLower();
                    value = parts[1];
                }
                else
                {
                    name = "text";
                    value = parameter;
                }

                if (name == "x")
                    Int32.TryParse(value, out x);
                else if (name == "y")
                    Int32.TryParse(value, out y);
                else if (name == "ledname")
                    ledName = value;
                else if (name == "fontsize" || name == "size")
                    property.FontSize = value;
                else if (name == "effect")
                    property.Effect = value;
                else if (name == "movespeed" || name == "speed")
                    property.MoveSpeed = value;
                else if (name == "duration")
                    property.Duration = value;
                else if (name == "horzalign")
                    property.HorzAlign = value;
                else if (name == "vertalign")
                    property.VertAlign = value;
                else if (name == "style" || name == "extendstyle")
                    style = value;
                else if (name == "text")
                {
                    text = Unescape(value);
                }
                else
                {
                    await SendMessageAsync(groupName, $"无法识别子参数名 '{name}'");
                    return;
                }
            }

            // 2021/8/13
            // 最多重试 5 次，耗费 10 秒钟
            NormalResult result = null;
            for (int i = 0; i < 5; i++)
            {
                if (i > 0)
                    await Task.Delay(TimeSpan.FromSeconds(2));

                result = RfidManager.LedDisplay(ledName,
                    text,
                    x,
                    y,
                    property,
                    style);
                if (result.Value != -1)
                    break;

                // if (result.Value == -1 && result.ErrorCode == "uninitialized")
            }

            await SendMessageAsync(groupName, result.Value == -1 ? $"{result.ErrorInfo} errorCode='{result.ErrorCode}'" : $"'{text}' 已成功显示在 LED 屏上");
        }

        static string Unescape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return System.Text.RegularExpressions.Regex.Unescape(text.Replace("\\w", " "));
        }

        public static bool ContainsParam(string args, string param)
        {
            var list = StringUtil.SplitList(args, ' ');
            return list.IndexOf(param) != -1;
        }
        public delegate bool Delegate_contains(string arg);

        public static bool ContainsParam(string args, Delegate_contains contains)
        {
            if (args == null)
                return false;

            var list = StringUtil.SplitList(args, ' ');
            foreach (var s in list)
            {
                if (contains(s))
                    return true;
            }

            return false;
        }

        static string ShortcutPath = "DigitalPlatform/dp2 V3/dp2SSL-自助借还";

        public static bool StartModule(
    string shortcut_path,
    string arguments)
        {
            string strShortcutFilePath = PathUtil.GetShortcutFilePath(
                    shortcut_path
                    // "DigitalPlatform/dp2 V3/dp2Library XE V3"
                    );

            if (File.Exists(strShortcutFilePath) == false)
                return false;

            // https://stackoverflow.com/questions/558344/clickonce-appref-ms-argument
            // Process.Start(strShortcutFilePath, arguments);

            // https://stackoverflow.com/questions/46808315/net-core-2-0-process-start-throws-the-specified-executable-is-not-a-valid-appl
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(strShortcutFilePath)
            {
                UseShellExecute = true,
                Arguments = arguments,
            };
            p.Start();
            return true;
        }

        // https://stackoverflow.com/questions/30299671/matching-strings-with-wildcard
        // If you want to implement both "*" and "?"
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        // 列出书柜中的现有图书
        static async Task ListBookAsync(string command, string groupName)
        {
            try
            {
                StringBuilder text = new StringBuilder();

                List<string> names = new List<string>();
                ShelfData.Doors.ForEach(o => names.Add(o.Name));
                text.AppendLine($"=== 书柜共有 {ShelfData.Doors.Count} 门，名字分别为 {StringUtil.MakePathList(names)} ===");

                // 子参数
                string param = command.Substring("list book".Length).Trim();
                if (string.IsNullOrEmpty(param))
                    text.AppendLine($"下面各门内图书信息");
                else
                    text.AppendLine($"下面列出名字 '{param}' 的各门内图书信息");

                // 柜门名字，前方一致。如果为空表示全部柜门
                int total_count = 0;
                int door_count = 0;
                foreach (var door in ShelfData.Doors)
                {
                    if (string.IsNullOrEmpty(param) == false
                        && Regex.IsMatch(door.Name, WildCardToRegular(param)) == false /*door.Name.StartsWith(param) == false*/)
                        continue;

                    text.AppendLine($"门 {door.Name} ({door.AllEntities.Count}):");
                    int i = 1;
                    foreach (var entity in door.AllEntities)
                    {
                        text.AppendLine($"    {i++}) {GetString(entity)}");
                    }
                    door_count++;
                    total_count += door.AllEntities.Count;
                }

                text.AppendLine($"=== {door_count} 门，共 {total_count} 册 ===");
                await SendMessageAsync(groupName,
text.ToString());

            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"ListBookAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                await SendMessageAsync(groupName,
                    $"命令 {command} 执行过程出现异常:\r\n{ExceptionUtil.GetDebugText(ex)}");
            }

            string GetString(Entity e)
            {
                return e.GetOiPii() + " " + e.Title;
            }
        }


        // 列出操作历史
        // 子参数:
        //      not sync/!sync/new 没有同步的那些事项
        //      sync 已经同步的那些shixiang
        //      error 同步出错的事项
        //      空 所有事项
        static async Task ListHistoryAsync(string command, string groupName)
        {
            try
            {
                // 子参数
                string param = command.Substring("list history".Length).Trim();
                // "not sync" 表示只列出那些没有成功同步的操作

                // TODO: 实现 id=xxx~xxx 操作。可以根据 ID 查看记录
                // id=xxx(n) 表示从指定 ID 位置开始向后看至多 n 条记录
                // id=xxx(-n) 表示从指定 ID 位置开始向前(时间靠前)看至多 n 条记录
                // TODO: 实现 state=xxx,xxx 操作。可以根据记录状态查看记录

                using (var context = new RequestContext())
                {
                    context.Database.EnsureCreated();   // 2020/4/1

                    List<RequestItem> items = null;
                    if (param == "not sync" || param == "!sync" || param == "new")
                        items = context.Requests.Where(o => o.State != "sync")
                            .OrderBy(o => o.ID).ToList();
                    else if (param == "sync")
                        items = context.Requests.Where(o => o.State == "sync")
                            .OrderBy(o => o.ID).ToList();
                    else if (param == "error")
                        items = context.Requests.Where(o => o.State == "commerror" || o.State == "normalerror")
                            .OrderBy(o => o.ID).ToList();
                    else
                        items = context.Requests.Where(o => true)
                            .OrderBy(o => o.ID).ToList();

                    await SendMessageAsync(groupName,
                        $"> {command}\r\n当前共有 {items.Count} 个历史事项");
                    int i = 1;
                    foreach (var item in items)
                    {
                        await SendMessageAsync(groupName,
                            $"{i++}\r\n{DisplayRequestItem.GetDisplayString(item)}");
                    }
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"ListHistoryAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                await SendMessageAsync(groupName,
                    $"命令 {command} 执行过程出现异常:\r\n{ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // 检查读者状态
        static async Task CheckPatronAsync(string command, string groupName)
        {
            // 子参数
            string param = command.Substring("check patron".Length).Trim();

            // 目前子参数为 PII
            param = param.ToUpper();

            StringBuilder text = new StringBuilder();
            var result = await ShelfData.VerifyBookAsync(null, param);
            if (result.Value == -1)
                text.Append("检查过程出错:" + result.ErrorInfo);
            else if (result.Infos.Count == 0)
                text.Append("没有实际处理任何记录");
            else
            {
                text.AppendLine($"检查共返回 {result.Infos.Count} 条信息(其中 error 开头的代表错误):");
                int i = 0;
                foreach (var s in result.Infos)
                {
                    text.AppendLine($"{++i}) {s}");
                }
            }

            await SendMessageAsync(groupName, text.ToString());


            /*
            // TODO: 用一段文字描述这一册的总体状态。特别是是否同步成功，本地库最新状态和 dp2library 一端是否吻合

            using (var context = new RequestContext())
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine($"> {command}\r\n");

                // 显示该读者的在借册情况
                var borrows = context.Requests
                    .Where(o => o.OperatorID == param && o.Action == "borrow" && o.LinkID == null)
                    .OrderBy(o => o.ID).ToList();
                if (borrows.Count == 0)
                    text.AppendLine($"操作历史中，该读者目前没有在借册");
                else
                {
                    text.AppendLine($"操作历史中，该读者目前有 {borrows.Count} 个在借册");
                    int j = 0;
                    foreach (var item in borrows)
                    {
                        var title = GetEntityTitle(item.EntityString);
                        text.AppendLine($"{(j++) + 1}) [{item.PII}] {title} 借阅日期:{item.OperTime.ToString()}");
                    }
                }

                text.AppendLine();

                // dp2library 一端的读者记录
                var result = LibraryChannelUtil.GetReaderInfo(param);
                if (result.Value == -1)
                    text.AppendLine($"获得读者记录时出错: {result.ErrorInfo}");
                else
                {
                    string xml = DomUtil.GetIndentXml(result.ReaderXml);
                    text.AppendLine($"读者记录\r\n{xml}");
                }

                text.AppendLine();

                // 关于这个 PII 的最新 10 操作
                var items = context.Requests.Where(o => o.OperatorID == param)
                    .OrderByDescending(o => o.ID).Take(10).ToList();

                text.AppendLine($"证条码号 为 {param} 的读者最新操作历史 {items.Count} 个");
                int i = 1;
                foreach (var item in items)
                {
                    text.AppendLine($"{i++}\r\n{SimpleRequestItem.GetDisplayString(item)}");
                }

                await SendMessageAsync(new string[] { groupName }, text.ToString());
            }
            */
        }

        class EntityTemplate
        {
            public string Title { get; set; }
        }

        static string GetEntityTitle(string entityString)
        {
            var template = JsonConvert.DeserializeObject<EntityTemplate>(entityString);
            if (template == null)
                return "";
            return template.Title;
        }

        static List<string> _commandQueue = new List<string>();
        static object _syncRoot_commandQueue = new object();

        static WriteTagWindow _writeTagDialog = null;
        static AsyncSemaphore _writeTagLimit = new AsyncSemaphore(1);

        // 写入 RFID 标签
        public static async Task<NormalResult> WriteTagAsync(string command, string groupName)
        {
            // 检查当前是否正在 SettingPage
            if (IsInSettingPage() == false)
            {
                await SendMessageAsync(groupName, "当前书柜不在设置页面，无法启动写入标签的操作。请先派人到书柜屏幕上，手动进入设置页面，然后再使用本命令");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "当前书柜不在设置页面，无法启动写入标签的操作",
                    ErrorCode = "notInSettingPage"
                };
            }

            using (var releaser = await _writeTagLimit.EnterAsync().ConfigureAwait(false))
            {
                if (_writeTagDialog != null)
                {
                    lock (_syncRoot_commandQueue)
                    {
                        _commandQueue.Add(command);
                    }
                    await SendMessageAsync(groupName, $"前一次写入 RFID 标签任务尚未完成，本次任务 '{command}' 已加入队列");
                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "本次任务已经加入队列",
                        ErrorCode = "queue"
                    };
                }

            // REDO:
                // 子参数
                string param = command.Substring("write tag".Length).Trim();

                // B0000001
                // 子参数为图书 PII。为 xxxx.xxxx 或者 xxxx 形态

                // -once 表示对话框只执行一次任务就自动关闭

                bool loop = false;
                string pii = "";
                List<string> parameters = StringUtil.SplitList(param, " ");
                foreach (string parameter in parameters)
                {
                    string name = "";
                    string value = "";
                    if (parameter.StartsWith("-"))
                    {
                        var parts = StringUtil.ParseTwoPart(parameter.Substring(1), ":");
                        name = parts[0].ToLower();
                        value = parts[1];
                    }
                    else
                    {
                        name = "barcode";
                        value = parameter;
                    }

                    if (name == "loop")
                        loop = true;
                    else if (name == "barcode" || name == "pii")
                    {
                        pii = Unescape(value);
                    }
                    else
                    {
                        return new NormalResult
                        {
                            Value = 0,
                            ErrorInfo = $"无法识别子参数名 '{name}'",
                            ErrorCode = "parameterError"
                        };
                    }
                }


                /*
                if (string.IsNullOrEmpty(pii))
                {
                    await SendMessageAsync(new string[] { groupName }, $"无法执行命令 '{command}'，因命令中缺乏图书 PII 部分。注: 命令格式为 write tag PII");
                    goto END;
                }
                */

                {
                    WriteTagWindow dlg = null;
                    App.Invoke(new Action(() =>
                    {
                        dlg = new WriteTagWindow();
                        App.SetSize(dlg, "wide");
                    }));

                    dlg.LoopWriting = loop;
                    dlg.Closed += (o1, e1) =>
                    {
                        PageMenu.PageSetting?.RemoveLayer();

                        _writeTagDialog = null;

                        lock (_syncRoot_commandQueue)
                        {
                            if (_commandQueue.Count > 0)
                            {
                                var c = _commandQueue[0];
                                _commandQueue.RemoveAt(0);
                                _ = WriteTagAsync(c, groupName);
                            }
                        }
                    };
                    _writeTagDialog = dlg;

                    if (string.IsNullOrEmpty(pii) == false)
                    {
                        // 根据 PII 准备好 TaskInfo
                        var result = await dlg.PrepareTaskAsync(pii);
                        if (result.Value == -1)
                        {
                            await SendMessageAsync(groupName, $"命令 '{command}': 准备 TaskInfo 时出错: {result.ErrorInfo}");
                            App.Invoke(new Action(() =>
                            {
                                dlg.Close();
                            }));
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = string.IsNullOrEmpty(result.ErrorCode) ? "prepareTaskInfoError" : result.ErrorCode
                            };
                        }
                    }
                    else
                    {
                        // 等待扫入条码模式
                    }

                    App.Invoke(new Action(() =>
                    {
                        dlg.Owner = Application.Current.MainWindow;
                        dlg.Show();
                        PageMenu.PageSetting?.AddLayer();
                    }));
                }

                return new NormalResult
                {
                    Value = 1,
                    ErrorInfo = "写入标签对话框已打开"
                };
            }
        }

        /*
        private static void Dlg_Closed(object sender, EventArgs e)
        {
            if (_writeTagDialog != null)
                _writeTagDialog.Closed -= Dlg_Closed;

            _writeTagDialog = null;

            lock (_syncRoot_commandQueue)
            {
                if (_commandQueue.Count > 0)
                {
                    var command = _commandQueue[0];
                    _commandQueue.RemoveAt(0);
                    _ = WriteTagAsync(command, null);
                }
            }
        }
        */

#if REMOVED
        static int _inWriteTag = 0;

        // 写入 RFID 标签
        static async Task<NormalResult> WriteTagAsync(string command, string groupName)
        {
            // 检查当前是否正在 SettingPage
            if (IsInSettingPage() == false)
            {
                await SendMessageAsync(new string[] { groupName }, "当前书柜不在设置页面，无法启动写入标签的操作。请先派人到书柜屏幕上，手动进入设置页面，然后再使用本命令");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "当前书柜不在设置页面，无法启动写入标签的操作",
                    ErrorCode = "notInSettingPage"
                };
            }

            _inWriteTag++;
            try
            {
                if (_inWriteTag > 1)
                {
                    lock (_syncRoot_commandQueue)
                    {
                        _commandQueue.Add(command);
                    }
                    await SendMessageAsync(new string[] { groupName }, $"前一次写入 RFID 标签任务尚未完成，本次任务 '{command}' 已加入队列");
                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "本次任务已经加入队列",
                        ErrorCode = "queue"
                    };
                }

            REDO:
                // 子参数
                string param = command.Substring("write tag".Length).Trim();

                // 子参数为图书 PII。为 xxxx.xxxx 或者 xxxx 形态
                var pii = param;

                /*
                if (string.IsNullOrEmpty(pii))
                {
                    await SendMessageAsync(new string[] { groupName }, $"无法执行命令 '{command}'，因命令中缺乏图书 PII 部分。注: 命令格式为 write tag PII");
                    goto END;
                }
                */

                {
                    WriteTagWindow dlg = null;
                    App.Invoke(new Action(() =>
                    {
                        dlg = new WriteTagWindow();
                        App.SetSize(dlg, "wide");
                    }));

                    if (string.IsNullOrEmpty(pii) == false)
                    {
                        // 根据 PII 准备好 TaskInfo
                        var result = await dlg.PrepareTaskAsync(pii);
                        if (result.Value == -1)
                        {
                            await SendMessageAsync(new string[] { groupName }, $"命令 '{command}': 准备 TaskInfo 时出错: {result.ErrorInfo}");
                            goto END;
                        }
                    }
                    else
                    {
                        // 等待扫入条码模式
                    }

                    // await SendMessageAsync(new string[] { groupName }, $"开始命令 '{command}': 对话框打开");

                    App.Invoke(new Action(() =>
                    {
                        dlg.Owner = Application.Current.MainWindow;
                        dlg.ShowDialog();
                    }));
                }

            // await SendMessageAsync(new string[] { groupName }, $"结束命令 '{command}': 对话框关闭");

            END:
                lock (_syncRoot_commandQueue)
                {
                    if (_commandQueue.Count > 0)
                    {
                        command = _commandQueue[0];
                        _commandQueue.RemoveAt(0);
                        goto REDO;
                    }
                }

                return new NormalResult { Value = 1 };
            }
            finally
            {
                _inWriteTag--;
            }
        }

#endif

        // 检测标签状态
        static async Task CheckTagAsync(string command, string groupName)
        {
            // 子参数
            string param = command.Substring("check tag".Length).Trim();

            // 子参数为门名字列表，或者空。空表示所有的门
            var doors = DoorItem.FindDoors(ShelfData.Doors, param);
            if (doors.Count == 0)
            {
                await SendMessageAsync(groupName, "没有找到符合条件的柜门，无法进行检查");
                return;
            }

            StringBuilder text = new StringBuilder();
            foreach (var door in doors)
            {
                var test_result = await ShelfData.TestInventoryAsync(
    door,
    "getTagInfo");
                text.AppendLine($"=== {door.Name} ({door.ReaderName}:{door.Antenna})===");
                if (test_result.Value == -1)
                    text.AppendLine($"　检测时出错: {test_result.ErrorInfo}");
                else if (test_result.Datas == null
                    || test_result.Datas.Count == 0)
                {
                    text.AppendLine($"　(没有探测到任何标签)");
                }
                else if (test_result.Datas != null)
                {
                    text.AppendLine($"　共探测到 {test_result.Datas.Count} 个标签:");
                    int index = 0;
                    foreach (var data in test_result.Datas)
                    {
                        text.AppendLine($"　{index + 1}) {data.OneTag?.UID}");
                        if (string.IsNullOrEmpty(data.Error) == false)
                            text.AppendLine($"　　错误信息: {data.Error}");
                        var taginfo = data.OneTag?.TagInfo;
                        if (taginfo != null)
                        {
                            try
                            {
                                var chip = LogicChip.From(taginfo.Bytes,
                                    (int)taginfo.BlockSize);
                                var pii = chip.FindElement(ElementOID.PII)?.Text;
                                var oi = chip.FindElement(ElementOID.OI)?.Text;
                                var tu = chip.FindElement(ElementOID.TU)?.Text;
                                text.AppendLine($"　　PII='{pii}' TU='{tu}' OI='{oi}'");
                            }
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"CheckTagAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                                text.AppendLine($"　　解析 TagInfo 时出错: {ex.Message}");
                            }
                        }

                        index++;
                    }
                }
            }

            text.AppendLine("(结束)");
            await SendMessageAsync(groupName, text.ToString());
        }

        // 检查册状态
        static async Task CheckBookAsync(string command, string groupName)
        {
            // 子参数
            string param = command.Substring("check book".Length).Trim();

            // 目前子参数为 PII
            param = param.ToUpper();

            StringBuilder text = new StringBuilder();
            var result = await ShelfData.VerifyBookAsync(param, null);
            if (result.Value == -1)
                text.Append("检查过程出错:" + result.ErrorInfo);
            else if (result.Infos.Count == 0)
                text.Append("没有实际处理任何记录");
            else
            {
                text.AppendLine($"检查共返回 {result.Infos.Count} 条信息(其中 error 开头的代表错误):");
                int i = 0;
                foreach (var s in result.Infos)
                {
                    text.AppendLine($"{++i}) {s}");
                }
            }

            await SendMessageAsync(groupName, text.ToString());

            /*
            // TODO: 用一段文字描述这一册的总体状态。特别是是否同步成功，本地库最新状态和 dp2library 一端是否吻合

            using (var context = new RequestContext())
            {
                // 关于这个 PII 的最新 10 操作
                var items = context.Requests.Where(o => o.PII == param)
                    .OrderByDescending(o => o.ID).Take(10).ToList();

                StringBuilder text = new StringBuilder();
                text.AppendLine($"> {command}\r\nPII 为 {param} 的册最新操作历史 {items.Count} 个");
                int i = 1;
                foreach (var item in items)
                {
                    text.AppendLine($"{i++}\r\n{SimpleRequestItem.GetDisplayString(item)}");
                }
                // 获得 dp2library 中的册记录
                var result = await LibraryChannelUtil.GetEntityDataAsync(param, "");
                if (result.Value == -1 || result.Value == 0)
                    text.AppendLine($"尝试获得册记录时出错: {result.ErrorInfo}");
                else
                    text.AppendLine($"册记录:\r\n{DomUtil.GetIndentXml(result.ItemXml)}");

                await SendMessageAsync(new string[] { groupName }, text.ToString());
            }
            */
        }

        // 修改操作历史
        // 子参数:
        //      id=xxxx state=xxxx
        static async Task ChangeHistoryAsync(string command, string groupName)
        {
            try
            {
                // 子参数
                string param = command.Substring("change history".Length).Trim();

                var table = StringUtil.ParseParameters(param, ' ', '=', "");
                string id_string = table["id"] as string;
                if (string.IsNullOrEmpty(id_string))
                {
                    await SendMessageAsync(groupName,
                        $"> {command}\r\n命令中缺乏 id=xxxx 部分，无法定位要修改的记录");
                    return;
                }

                int id = Convert.ToInt32(id_string);

                using (var context = new RequestContext())
                {
                    context.Database.EnsureCreated();

                    var item = context.Requests.Where(o => o.ID == id).FirstOrDefault();
                    if (item == null)
                    {
                        await SendMessageAsync(groupName,
                            $"> {command}\r\n没有找到 ID 为 '{id}' 的操作历史记录");
                        return;
                    }

                    bool changed = false;
                    if (table.ContainsKey("state"))
                    {
                        string new_value = table["state"] as string;
                        if (isStateValid(new_value) == false)
                        {
                            await SendMessageAsync(groupName,
                                $"> {command}\r\n要修改为新的 State 值 '{new_value}' 不合法。修改操作被拒绝");
                            return;
                        }
                        // TODO: 是否自动写入一个附注字段内容，记载修改前的内容，和修改的原因(comment=xxx)？
                        item.State = new_value;
                        changed = true;
                    }

                    if (changed == true)
                    {
                        await context.SaveChangesAsync();
                        await SendMessageAsync(groupName,
                            $"> {command}\r\n记录被修改。修改后内容如下:\r\n{DisplayRequestItem.GetDisplayString(item)}");
                    }
                    else
                        await SendMessageAsync(groupName,
                            $"> {command}\r\n记录没有发生修改。记录内容如下:\r\n{DisplayRequestItem.GetDisplayString(item)}");
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"ChangeHistoryAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                await SendMessageAsync(groupName,
                    $"命令 {command} 执行过程出现异常:\r\n{ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // 检查要修改成的 State 字段值是否合法。目前只支持修改为 "dontsync" 值
        static bool isStateValid(string value)
        {
            if (value == "dontsync")
                return true;
            return false;
        }


        #region 显示格式

        // 简略一点的格式
        class SimpleRequestItem
        {
            public int ID { get; set; }

            public string PII { get; set; } // PII 单独从 EntityString 中抽取出来，便于进行搜索

            public string Action { get; set; }  // borrow/return/transfer

            public DateTime OperTime { get; set; }  // 操作时间
            public string State { get; set; }   // 状态。sync/commerror/normalerror/空
                                                // 表示是否完成同步，还是正在出错重试同步阶段，还是从未同步过
            public string SyncErrorInfo { get; set; }   // 最近一次同步操作的报错信息
            public int SyncCount { get; set; }

            public string Operator { get; set; }
            public string Title { get; set; }
            /*
            public Operator Operator { get; set; }  // 提起请求的读者

            public Entity Entity { get; set; }
            */

            public string TransferDirection { get; set; } // in/out 典藏移交的方向
            public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
            public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
            public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成

            public static string GetDisplayString(RequestItem item)
            {
                SimpleRequestItem result = new SimpleRequestItem
                {
                    ID = item.ID,
                    PII = item.PII,
                    Action = item.Action,
                    OperTime = item.OperTime,
                    State = item.State,
                    SyncErrorInfo = item.SyncErrorInfo,
                    SyncCount = item.SyncCount,
                    Operator = JsonConvert.DeserializeObject<Operator>(item.OperatorString)?.ToString(),
                    Title = JsonConvert.DeserializeObject<Entity>(item.EntityString)?.Title,
                    TransferDirection = item.TransferDirection,
                    Location = item.Location,
                    CurrentShelfNo = item.CurrentShelfNo,
                    BatchNo = item.BatchNo,
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
        }

        // 详细的格式
        public class DisplayRequestItem
        {
            public int ID { get; set; }

            public string PII { get; set; } // PII 单独从 EntityString 中抽取出来，便于进行搜索

            public string Action { get; set; }  // borrow/return/transfer

            public DateTime OperTime { get; set; }  // 操作时间
            public string State { get; set; }   // 状态。sync/commerror/normalerror/空
                                                // 表示是否完成同步，还是正在出错重试同步阶段，还是从未同步过

            // 操作者 ID。为读者证条码号，或者 ~工作人员账户名
            public string OperatorID { get; set; }  // 从 Operator 而来
            public string LinkID { get; set; }

            public string SyncErrorCode { get; set; }
            public string SyncErrorInfo { get; set; }   // 最近一次同步操作的报错信息
            public int SyncCount { get; set; }

            // 同步操作时间。最后一次同步操作的时间
            public DateTime SyncOperTime { get; set; }

            public Operator Operator { get; set; }  // 提起请求的读者

            public Entity Entity { get; set; }

            public string TransferDirection { get; set; } // in/out 典藏移交的方向
            public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
            public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
            public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成

            // 2020/4/27
            public string ActionString { get; set; }

            public static string GetDisplayString(RequestItem item)
            {
                // 马赛克处理
                var operator1 = JsonConvert.DeserializeObject<Operator>(item.OperatorString);
                operator1.PatronName = operator1.PatronNameMasked;
                if (Operator.IsPatronBarcodeWorker(operator1.PatronBarcode) == false)
                    operator1.PatronBarcode = operator1.PatronBarcodeMasked;

                DisplayRequestItem result = new DisplayRequestItem
                {
                    ID = item.ID,
                    OperatorID = item.GetMaskedOperatorID(),
                    LinkID = item.LinkID,
                    SyncOperTime = item.SyncOperTime,
                    PII = item.PII,
                    Action = item.Action,
                    OperTime = item.OperTime,
                    State = item.State,
                    SyncErrorCode = item.SyncErrorCode,
                    SyncErrorInfo = item.SyncErrorInfo,
                    SyncCount = item.SyncCount,
                    Operator = operator1,   // JsonConvert.DeserializeObject<Operator>(item.OperatorString),
                    Entity = JsonConvert.DeserializeObject<Entity>(item.EntityString),
                    TransferDirection = item.TransferDirection,
                    Location = item.Location,
                    CurrentShelfNo = item.CurrentShelfNo,
                    BatchNo = item.BatchNo,
                    ActionString = item.ActionString,
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }

            public static DisplayRequestItem FromDisplayString(string value)
            {
                return JsonConvert.DeserializeObject<DisplayRequestItem>(value);
            }
        }

        #endregion

        #region SetMessage() API

        // 不可靠的直接发送消息
        public static Task<SetMessageResult> InnerSetMessageAsync(string[] groups, string content)
        {
            if (HubProxy == null)
                return Task.FromResult<SetMessageResult>(new SetMessageResult());

            // TODO: 如果 groups 为 null 代表所有加入的群名列表

            /*
            if (groups == null)
            {
                groups = GroupNames;
                if (groups == null || groups.Length == 0)
                {
                    string error = $"InnerSetMessageAsync() 出错: GroupName 不正确";
                    WpfClientInfo.WriteErrorLog(error);
                    return Task.FromResult<SetMessageResult>(new SetMessageResult
                    {
                        Value = -1,
                        ErrorInfo = error
                    });
                }
            }
            */


            SetMessageRequest request = new SetMessageRequest("create", "dontNotifyMe",
                new List<MessageRecord> {
                        new MessageRecord {
                            groups= groups, // new string[] { groupName},
                            data = content}
                });
            return SetMessageAsync(request);
        }

        // 不可靠的直接发送消息
        public static Task<SetMessageResult> SetMessageAsync(
            SetMessageRequest param)
        {
            return HubProxy.Invoke<SetMessageResult>(
 "SetMessage",
 param);
        }

        #endregion

        #region Search() API

        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        static void OnSearchRecieved(SearchRequest param)
        {
            // 单独给一个线程来执行
            // Task.Factory.StartNew(() => SearchAndResponse(param));
            _ = Task.Run(() =>
            {
                try
                {
                    SearchAndResponse(param);
                }
                catch(Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"SearchAndResponse() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        static void SearchAndResponse(SearchRequest searchParam)
        {
            /*
            if (searchParam.Operation == "getPatronInfo")
            {
                GetPatronInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getSystemParameter")
            {
                GetSystemParameter(searchParam);
                return;
            }

            if (searchParam.Operation == "getUserInfo")
            {
                GetUserInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBiblioInfo")
            {
                GetBiblioInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBiblioSummary")
            {
                GetBiblioSummary(searchParam);
                return;
            }

            if (searchParam.Operation == "getItemInfo")
            {
                GetItemInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBrowseRecords")
            {
                GetBrowseRecords(searchParam);
                return;
            }
            */

            _ = SearchHistoryAsync(searchParam);
            return;
        }

        public static int ParseTimeRangeString(string strText,
    bool bAdjustEnd,
    out DateTime start,
    out DateTime end,
    out string strError)
        {
            strError = "";
            start = new DateTime((long)0);
            end = new DateTime((long)0);

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                //strError = "'" + strText + "' 中缺乏破折号 '-'";
                //return -1;
                if (strText.Length != 8)
                {
                    strError = "时间字符串 '" + strText + "' 不是8字符 (注：用法为 20200101-20211231 或 20200101)";
                    return -1;
                }

                // 变为 xxxxxxxx-xxxxxxxx 形态继续处理
                strText = strText + "-" + strText;
                nRet = strText.IndexOf("-");
                Debug.Assert(nRet != -1);
            }

            string strStart = strText.Substring(0, nRet).Trim();
            string strEnd = strText.Substring(nRet + 1).Trim();

            if (String.IsNullOrEmpty(strStart) == true)
                start = new DateTime(0);
            else
            {
                if (strStart.Length != 8)
                {
                    strError = "破折号左边的部分 '" + strStart + "' 不是8字符";
                    return -1;
                }
                start = DateTimeUtil.Long8ToDateTime(strStart);
            }

            if (String.IsNullOrEmpty(strEnd) == true)
                end = new DateTime(0);
            else
            {
                if (strEnd.Length != 8)
                {
                    strError = "破折号右边的部分 '" + strEnd + "' 不是8字符";
                    return -1;
                }
                end = DateTimeUtil.Long8ToDateTime(strEnd);

                if (bAdjustEnd == true)
                {
                    // 修正一天
                    end += new TimeSpan(24, 0, 0);
                }
            }

            return 0;
        }

        // https://eval-expression.net/linq-dynamic
        // parameters:
        //      request.UseList 为一个逗号间隔的检索途径列表。检索途径可以用 State/...
        //      request.MatchStyle 为 left/right/middle/exact 之一。缺省为 middle
        static string BuildQuery(SearchRequest request)
        {
            if (string.IsNullOrEmpty(request.QueryWord))
                return "true";

            string queryWordString = request.QueryWord.Replace("\"", "\\\"");

            StringBuilder text = new StringBuilder();
            List<string> uses = StringUtil.SplitList(request.UseList);
            List<string> where_list = new List<string>();
            foreach (var use in uses)
            {
                //string s = "";
                //s.EndsWith()

                if (use == "ID" || use == "SyncCount")
                {
                    // TODO: 允许范围检索
                    if (queryWordString.Contains("-"))
                    {
                        var parts = StringUtil.ParseTwoPart(queryWordString, "-");
                        where_list.Add($" x.{use} >= Convert.ToInt32(\"{parts[0]}\") && x.{use} <= Convert.ToInt32(\"{parts[1]}\")");
                    }
                    else
                    {
                        where_list.Add($" x.{use} == Convert.ToInt32(\"{queryWordString}\") ");
                    }
                    continue;
                }

                if (use == "OperTime")
                {
                    int nRet = ParseTimeRangeString(queryWordString,
true,
out DateTime start,
out DateTime end,
out string strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                    where_list.Add($" x.{use} >= new DateTime({start.Ticks}) && x.{use} <= new DateTime({end.Ticks})");
                    continue;
                }

                if (request.MatchStyle == "left")
                    where_list.Add($" x.{use}?.StartsWith(\"{queryWordString}\") == true ");
                else if (request.MatchStyle == "right")
                    where_list.Add($" x.{use}?.EndsWith(\"{queryWordString}\") == true ");
                else if (request.MatchStyle == "exact")
                    where_list.Add($" x.{use} == \"{queryWordString}\" ");
                else // if (request.MatchStyle == "middle")
                    where_list.Add($" x.{use}?.IndexOf(\"{queryWordString}\") != -1 ");
            }

            text.Append(StringUtil.MakePathList(where_list, "||"));
            return text.ToString();
        }

        delegate void delegate_process(RequestItem item);

        // 遍历结果集内指定范围的记录
        // 按照 ID 从小到大遍历
        // parameters:
        //      length  本次要获得的最大记录数。-1 表示尽可能多
        static void ForEach(string resultsetName,
            int start,
            int length,
            delegate_process func_process)
        {
            using (var context = new ResultsetContext())
            using (var mycontext = new RequestContext())
            {
                int count = 0;
                foreach (var item in context.Items
                    .Where(o => o.ResultsetName == resultsetName)
                    .OrderBy(o => o.ID)
                    .Skip(start))
                {
                    if (length != -1 && count >= length)
                        break;
                    var request = mycontext.Requests.Where(o => o.ID == item.ID).FirstOrDefault();
                    func_process(request);
                    count++;
                }
            }
        }

        // 创建一个结果集
        // IOrderedQueryable
        static async Task<int> CreateResultsetAsync(IEnumerable<RequestItem> result,
            string resultsetName)
        {
            int count = 0;
            using (var context = new ResultsetContext())
            {
                context.Database.EnsureCreated();
                context.Items.RemoveRange(context.Items.Where(o => o.ResultsetName == resultsetName));

                foreach (var item in result)
                {
                    context.Items.Add(new ResultsetItem
                    {
                        ResultsetName = resultsetName,
                        ID = item.ID
                    });
                    count++;
                }
                await context.SaveChangesAsync();
            }

            return count;
        }

        // 删除一个结果集
        static async Task DeleteResultsetAsync(string resultsetName)
        {
            using (var context = new ResultsetContext())
            {
                context.Database.EnsureCreated();
                context.Items.RemoveRange(context.Items.Where(o => o.ResultsetName == resultsetName));
                await context.SaveChangesAsync();
            }
        }

        public static async Task DeleteAllResultsetAsync()
        {
            try
            {
                using (var context = new ResultsetContext())
                {
                    context.Database.EnsureDeleted();
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"启动时删除全部结果集出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        static int GetResultsetLength(string resultsetName)
        {
            using (var context = new ResultsetContext())
            {
                return context.Items.Where(o => o.ResultsetName == resultsetName).Count();
            }
        }

        class Scanner : IEnumerable<RequestItem>
        {
            public RequestContext Context { get; set; }
            public SearchRequest SearchParam { get; set; }

            public IEnumerator<RequestItem> GetEnumerator()
            {
                List<RequestItem> results = new List<RequestItem>();
                if (SearchParam.UseList == "Operator")
                {
                    foreach (var item in Context.Requests)
                    {
                        var oper = item.OperatorString == null ? null :
            JsonConvert.DeserializeObject<Operator>(item.OperatorString);
                        if (Match(SearchParam.MatchStyle, oper.PatronBarcode, SearchParam.QueryWord))
                            yield return item;
                    }
                }
                else if (SearchParam.UseList == "ItemRecPath")
                {
                    foreach (var item in Context.Requests)
                    {
                        var entity = item.EntityString == null ? null :
            JsonConvert.DeserializeObject<Entity>(item.EntityString);
                        if (Match(SearchParam.MatchStyle, entity.ItemRecPath, SearchParam.QueryWord))
                            yield return item;
                    }
                }
                else
                    throw new Exception($"未知的 UseList '{SearchParam.UseList}'");
            }

            static bool Match(string matchStyle, string text, string query)
            {
                if (text == null)
                    text = "";

                if (matchStyle == "left")
                    return text.StartsWith(query);
                else if (matchStyle == "right")
                    return text.EndsWith(query);
                else if (matchStyle == "exact")
                    return text == query;
                else // if (request.MatchStyle == "middle")
                    return text.IndexOf(query) != -1;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                List<RequestItem> results = new List<RequestItem>();
                if (SearchParam.UseList == "Operator")
                {
                    foreach (var item in Context.Requests)
                    {
                        var oper = item.OperatorString == null ? null :
            JsonConvert.DeserializeObject<Operator>(item.OperatorString);
                        if (Match(SearchParam.MatchStyle, oper.PatronBarcode, SearchParam.QueryWord))
                            yield return item;
                    }
                }
                else if (SearchParam.UseList == "ItemRecPath")
                {
                    foreach (var item in Context.Requests)
                    {
                        var entity = item.EntityString == null ? null :
            JsonConvert.DeserializeObject<Entity>(item.EntityString);
                        if (Match(SearchParam.MatchStyle, entity.ItemRecPath, SearchParam.QueryWord))
                            yield return item;
                    }
                }
                else
                    throw new Exception($"未知的 UseList '{SearchParam.UseList}'");
            }
        }

#if REMOVED
        static IEnumerable<RequestItem> Scan(RequestContext context,
            SearchRequest searchParam)
        {
            List<RequestItem> results = new List<RequestItem>();
            if (searchParam.UseList == "Operator")
            {
                foreach (var item in context.Requests)
                {
                    var oper = item.OperatorString == null ? null :
        JsonConvert.DeserializeObject<Operator>(item.OperatorString);
                    if (Match(searchParam.MatchStyle, oper.PatronBarcode, searchParam.QueryWord))
                        results.Add(item);
                }

                return results;
            }
            else if (searchParam.UseList == "ItemRecPath")
            {
                foreach (var item in context.Requests)
                {
                    var entity = item.EntityString == null ? null :
        JsonConvert.DeserializeObject<Entity>(item.EntityString);
                    if (Match(searchParam.MatchStyle, entity.ItemRecPath, searchParam.QueryWord))
                        results.Add(item);
                }

                return results;
            }
            else
                throw new Exception($"未知的 UseList '{searchParam.UseList}'");
        }
#endif

        static string _libraryUID = Guid.NewGuid().ToString();

        // TODO: 结果集用 ID 数组表示? 早期可只支持 default 这一个结果集
        static async Task SearchHistoryAsync(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.MessageClient.Record> records = new List<DigitalPlatform.MessageClient.Record>();

            string strResultSetName = searchParam.ResultSetName;
            if (string.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";  // "#" + searchParam.TaskID;    // "default";
            else
                strResultSetName = "#" + strResultSetName;  // 如果请求方指定了结果集名，则在 dp2library 中处理为全局结果集名

            try
            {
                int count = (int)searchParam.Count;
                if (count == -1)
                    count = Int32.MaxValue;

                if (searchParam.Operation != "searchHistory")
                {
                    strError = $"暂不支持操作 '{searchParam.Operation}'";
                    goto ERROR1;
                }

                if (searchParam.QueryWord == "!getResult")
                {
                    // lRet = -1;
                    ForEach(strResultSetName,
                        (int)searchParam.Start,
                        count,
                        item =>
                        {
                            records.Add(new Record
                            {
                                RecPath = item.ID.ToString(),
                                Format = "JSON",
                                Data = DisplayRequestItem.GetDisplayString(item),
                                Timestamp = null
                            });
                        });
                    var result = await TryResponseSearchAsync(
searchParam.TaskID,
GetResultsetLength(strResultSetName),
searchParam.Start,
_libraryUID, // libraryUID,
records,
"", // errorInfo,
"", // errorCode,
100);
                }
                else
                {
                    // TODO: .Operation == "searchBiblio"
                    using (var context = new RequestContext())
                    {
                        int result_count = 0;

                        // 2020/8/27
                        if (searchParam.UseList == "Operator"
                            || searchParam.UseList == "ItemRecPath")
                        {
                            Scanner scanner = new Scanner
                            {
                                SearchParam = searchParam,
                                Context = context
                            };

                            /*
                            var results = Scan(context, searchParam);
                            if (results == null)
                                result_count = 0;
                            else
                                result_count = results.Count();
                            */

                            // 创建一个结果集
                            result_count = await CreateResultsetAsync(scanner, strResultSetName);
                        }
                        else
                        {
                            // https://stackoverflow.com/questions/37078256/entity-framework-building-where-clause-on-the-fly-using-expression
                            string query = BuildQuery(searchParam);

                            var query_result = context.Requests.WhereDynamic(x => query)
        .OrderBy(o => o.ID);
                            result_count = query_result.Count();

                            // 创建一个结果集
                            if (result_count > 0)
                            {
                                await CreateResultsetAsync(query_result, strResultSetName);
                            }
                        }

                        if (result_count == 0)
                        {
                            // 没有命中
                            await TryResponseSearchAsync(
                                new SearchResponse(
    searchParam.TaskID,
    0,
    0,
    _libraryUID, // this.dp2library.LibraryUID,
    records,
    "没有命中",  // 出错信息大概为 not found。
    "NotFound"));
                            return;
                        }

                        if (searchParam.Count == 0)
                        {
                            // 返回命中数
                            await TryResponseSearchAsync(
                                new SearchResponse(
                                searchParam.TaskID,
                                result_count,
    0,
    _libraryUID, // this.dp2library.LibraryUID,
    records,
    "本次没有返回任何记录",
    strErrorCode));
                            return;
                        }

                        /*
                        var items = query_result.Skip((int)searchParam.Start)
    .Take(count)
    .ToList();
                        foreach (var item in items)
                        {
                            records.Add(new Record
                            {
                                RecPath = item.ID.ToString(),
                                Format = "JSON",
                                Data = DisplayRequestItem.GetDisplayString(item),
                                Timestamp = null
                            });
                        }
                        */
                        ForEach(strResultSetName,
                            (int)searchParam.Start,
                            count,
                            item =>
                            {
                                records.Add(new Record
                                {
                                    RecPath = item.ID.ToString(),
                                    Format = "JSON",
                                    Data = DisplayRequestItem.GetDisplayString(item),
                                    Timestamp = null
                                });
                            });

                        var result = await TryResponseSearchAsync(
    searchParam.TaskID,
    result_count,
    searchParam.Start,
    _libraryUID, // libraryUID,
    records,
    "", // errorInfo,
    "", // errorCode,
    100);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SearchHistoryAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

        ERROR1:
            // 报错
            await TryResponseSearchAsync(
                new SearchResponse(
searchParam.TaskID,
-1,
0,
_libraryUID, // this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        // 调用 server 端 ResponseSearchBiblio
        static async Task TryResponseSearchAsync(
SearchResponse responseParam)
        {
            // TODO: 等待执行完成。如果有异常要当时处理。比如减小尺寸重发。
            int nRedoCount = 0;
        REDO:
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
 responseParam).ConfigureAwait(false);
                if (result.Value == -1)
                {
                    //AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                //AddErrorLine(ex.Message);
                if (ex.InnerException is InvalidOperationException
                    && nRedoCount < 2)
                {
                    nRedoCount++;
                    Thread.Sleep(1000);
                    goto REDO;
                }
            }
        }

        class ResponseSearchResult : NormalResult
        {
            // [out]
            public long BatchSize { get; set; }
        }

        // TODO: 注意测试，一次只能发送一个元素，或者连一个元素都发送不成功的情况
        // 具有重试机制的 ReponseSearch
        // 运行策略是，当遇到 InvalidOperationException 异常时，减少一半数量重试发送，用多次小批发送解决问题
        // 如果最终无法完成发送，则尝试发送一条报错信息，然后返回 false
        // parameters:
        //      batch_size  建议的最佳一次发送数目。-1 表示不限制
        // return Value:
        //      1    成功
        //      0   失败
        static async Task<ResponseSearchResult> TryResponseSearchAsync(
            string taskID,
            long resultCount,
            long start,
            string libraryUID,
            IList<Record> records,
            string errorInfo,
            string errorCode,
            long batch_size)
        {
            string strError = "";

            List<Record> rest = new List<Record>(); // 等待发送的
            List<Record> current = new List<Record>();  // 当前正在发送的
            if (batch_size == -1)
                current.AddRange(records);
            else
            {
                rest.AddRange(records);

                // 将最多 batch_size 个元素从 rest 中移动到 current 中
                for (int i = 0; i < batch_size && rest.Count > 0; i++)
                {
                    current.Add(rest[0]);
                    rest.RemoveAt(0);
                }
            }

            long send = 0;  // 已经发送过的元素数
            while (current.Count > 0)
            {
                try
                {
                    // Wait(new TimeSpan(0, 0, 0, 0, 50)); // 50

                    var result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
new SearchResponse(
                        taskID,
                        resultCount,
                        start + send,
                        libraryUID,
                        current,
                        errorInfo,
                        errorCode));

                    // _lastTime = DateTime.Now;
                    if (result.Value == -1)
                        return new ResponseSearchResult
                        {
                            Value = 0,
                            BatchSize = batch_size
                        };   // 可能因为服务器端已经中断此 taskID，或者执行 ReponseSearch() 时出错
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is InvalidOperationException)
                    {
                        if (current.Count == 1)
                        {
                            strError = "向中心发送 ResponseSearch 消息时出现异常(连一个元素也发送不出去): " + ex.InnerException.Message;
                            goto ERROR1;
                        }
                        // 减少一半元素发送
                        int half = Math.Max(1, current.Count / 2);
                        int offs = current.Count - half;
                        for (int i = 0; current.Count > offs; i++)
                        {
                            Record record = current[offs];
                            rest.Insert(i, record);
                            current.RemoveAt(offs);
                        }
                        batch_size = half;
                        continue;
                    }

                    strError = "向中心发送 ResponseSearch 消息时出现异常: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }

                // Console.WriteLine("成功发送 offset=" + (start + send) + " " + current.Count.ToString());

                send += current.Count;
                current.Clear();
                if (batch_size == -1)
                    current.AddRange(rest);
                else
                {
                    // 将最多 batch_size 个元素从 rest 中移动到 current 中
                    for (int i = 0; i < batch_size && rest.Count > 0; i++)
                    {
                        current.Add(rest[0]);
                        rest.RemoveAt(0);
                    }
                }
            }

            Debug.Assert(rest.Count == 0, "");
            Debug.Assert(current.Count == 0, "");
            return new ResponseSearchResult
            {
                Value = 1,
                BatchSize = batch_size
            };
        ERROR1:
            // 报错
            await TryResponseSearchAsync(
                new SearchResponse(
taskID,
-1,
0,
libraryUID,
new List<Record>(),
strError,
"_sendResponseSearchError"));    // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
            return new ResponseSearchResult
            {
                Value = 0,
                BatchSize = batch_size
            };
        }

        #endregion

        #region GetRes() API

        static void OnGetResRecieved(GetResRequest param)
        {
            _ = Task.Run(async () => await GetResAndResponse(param));
        }

        // TODO: 比对每次获得的时间戳，如果不一致则要报错。
        static async Task GetResAndResponse(GetResRequest param)
        {
            string strError = "";
            IList<string> results = new List<string>();
            long batch_size = 4 * 1024;    // 4K

            string strStyle = param.Style;
            if (StringUtil.IsInList("timestamp", strStyle) == false)
                StringUtil.SetInList(ref strStyle, "timestamp", true);  // 为了每次 GetRes() 以后比对

            // para.Path 为 "data/" 或者 "program/" 开头
            string strFilePath = "";
            if (param.Path.StartsWith("data/"))
            {
                string path = param.Path.Substring("data/".Length);
                strFilePath = Path.Combine(WpfClientInfo.UserDir, path);
            }
            else if (param.Path.StartsWith("program/"))
            {
                string path = param.Path.Substring("program/".Length);
                string binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                strFilePath = Path.Combine(binDir, path);
            }
            else
            {
                strError = $"路径 {param.Path} 无法识别(应当以 data/ 或 program/ 开头)";
                goto ERROR1;
            }

            try
            {
                long lTotalLength = 0;  // 对象的总长度
                long send = 0;  // 累计发送了多少 byte
                long chunk_size = -1;   // 每次从 dp2library 获取的分片大小
                long length = -1;    // 本次点对点 API 希望获得的长度

                if (param.Length != -1)
                {
                    chunk_size = Math.Min(100 * 1024, (int)param.Length);
                    length = param.Length;
                }

                byte[] timestamp = null;
                for (; ; )
                {
                    long lRet = 0;
                    byte[] baContent = null;
                    string strMetadata = "";
                    string strOutputResPath = "";
                    byte[] baOutputTimestamp = null;
                    string error_code = "";

                    if (param.Operation == "getRes")
                    {
                        Console.WriteLine("getRes() start=" + (param.Start + send)
                            + " length=" + chunk_size);
                        // Thread.Sleep(500);

                        /*
                        lRet = channel.GetRes(param.Path,
                            param.Start + send,
                            (int)chunk_size, // (int)param.Length,
                            strStyle,   // param.Style,
                            out baContent,
                            out strMetadata,
                            out strOutputResPath,
                            out baOutputTimestamp,
                            out strError);
                        */

                        // 下载本地文件
                        // TODO: 限制 nMaxLength 最大值
                        // parameters:
                        //      strStyle    "uploadedPartial" 表示操作都是针对已上载临时部分的。比如希望获得这个局部的长度，时间戳，等等
                        // return:
                        //      -2      文件不存在
                        //		-1      出错
                        //		>= 0	成功，返回最大长度
                        lRet = GetFile(
                            strFilePath,
                            param.Start + send,
                            (int)chunk_size,
                            100 * 1024,
                            strStyle,
                            out baContent,
                            out baOutputTimestamp,
                            out strError);
                        if (lRet == -2)
                        {
                            error_code = "NotFound";
                            lRet = -1;
                        }
                        else if (lRet == -1)
                        {
                            error_code = "SystemError";
                            lRet = -1;
                        }
                    }
                    else
                    {
                        strError = "无法识别的 Operation 值 '" + param.Operation + "'";
                        goto ERROR1;
                    }

                    if (timestamp != null)
                    {
                        if (ByteArray.Compare(timestamp, baOutputTimestamp) != 0)
                        {
                            strError = "获取对象过程中发现时间戳发生了变化，本次获取操作无效";
                            goto ERROR1;
                        }
                    }

                    // 记忆下来供下一轮比对之用
                    timestamp = baOutputTimestamp;

                    lTotalLength = lRet;
                    if (length == -1)
                        length = lRet;

                    GetResResponse result = new GetResResponse();
                    result.TaskID = param.TaskID;
                    result.TotalLength = lRet;
                    result.Start = param.Start + send;
                    result.Path = strOutputResPath;
                    result.Data = baContent;
                    if (send == 0)
                    {
                        result.Metadata = strMetadata;
                        if (StringUtil.IsInList("timestamp,md5", param.Style) == true)
                            result.Timestamp = ByteArray.GetHexTimeStampString(baOutputTimestamp);
                    }
                    result.ErrorInfo = strError;
                    result.ErrorCode = error_code;

                    /*
                    bool bRet = TryResponseGetRes(result,
        ref batch_size);
                    if (bRet == false
                        || result.Data == null || result.Data.Length == 0
                        || length == -1)
                        return;
                    */

                    var response_result = await TryResponseGetRes(result,
                    batch_size);
                    batch_size = response_result.BatchSize;
                    if (response_result.Value == -1
                        || result.Data == null || result.Data.Length == 0
                        || length == -1)
                        return;

                    if (param.Start + send >= length)
                        return;

                    send += result.Data.Length;

                    {
                        chunk_size = length - param.Start - send;
                        if (chunk_size <= 0)
                            return;
                        if (chunk_size >= Int32.MaxValue)
                            chunk_size = 100 * 1024;
                    }
                }
            }
            catch (Exception ex)
            {
                // AddErrorLine("GetResAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

        ERROR1:
            {
                // 报错
                GetResResponse result = new GetResResponse();
                result.TaskID = param.TaskID;
                result.TotalLength = -1;
                result.ErrorInfo = strError;
                // result.ErrorCode = error_code;

                await HubProxy.Invoke<MessageResult>("ResponseGetRes",
result);
                // ResponseGetRes(result);
            }
        }

        /*
        class ResponseResult : NormalResult
        {
            public long batch_size { get; set; }
        }
        */

        // TODO: 如果第一次 metadata 和 timestamp 发送成功了，后面的几次就不要发送了，这样可以节省流量
        // TODO: 如果 dp2mserver 返回值表示需要中断，就不要继续处理了
        // parameters:
        //      batch_size  建议的最佳一次发送数目。-1 表示不限制
        // return:
        //      true    成功
        //      false   失败
        static async Task<ResponseSearchResult> TryResponseGetRes(
            GetResResponse param,
            long batch_size/*,
            ref long batch_size*/)
        {
            string strError = "";

            // 修正 2018/10/2
            if (param.Data != null && param.Data.Length == 0)
                param.Data = null;

            List<byte> rest = new List<byte>(); // 等待发送的
            List<byte> current = new List<byte>();  // 当前正在发送的
            if (param.Data != null)
            {
                if (batch_size == -1)
                    current.AddRange(param.Data);
                else
                {
                    rest.AddRange(param.Data);

                    // 将最多 batch_size 个元素从 rest 中移动到 current 中
                    for (int i = 0; i < batch_size && rest.Count > 0; i++)
                    {
                        current.Add(rest[0]);
                        rest.RemoveAt(0);
                    }
                }
            }

            long send = 0;  // 已经发送过的元素数
            while (current.Count > 0 || param.Data == null)
            {
                try
                {
                    // await Task.Delay(TimeSpan.FromMilliseconds(50));

                    MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseGetRes",
                        new GetResResponse(
                        param.TaskID,
                        param.TotalLength,
                        param.Start + send,
                        param.Path,
                        current.ToArray(),
                        send == 0 ? param.Metadata : "",
                        send == 0 ? param.Timestamp : "",
                        param.ErrorInfo,
                        param.ErrorCode));
                    // _lastTime = DateTime.Now;
                    if (result.Value == -1)
                    {
                        // return false;   // 可能因为服务器端已经中断此 taskID，或者执行 ReponseSearch() 时出错
                        return new ResponseSearchResult
                        {
                            Value = -1,
                            BatchSize = batch_size,
                            ErrorInfo = result.ErrorInfo
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine("(retry)ResponseGetRes() exception=" + ex.Message);

                    if (ex.InnerException is InvalidOperationException)
                    {
                        if (current.Count == 1)
                        {
                            strError = "向中心发送 ResponseGetRes 消息时出现异常(连一个元素也发送不出去): " + ex.InnerException.Message;
                            goto ERROR1;
                        }
                        // 减少一半元素发送
                        int half = Math.Max(1, current.Count / 2);
                        int offs = current.Count - half;
                        for (int i = 0; current.Count > offs; i++)
                        {
                            byte record = current[offs];
                            rest.Insert(i, record);
                            current.RemoveAt(offs);
                        }
                        batch_size = half;
                        continue;
                    }

                    strError = "向中心发送 ResponseGetRes 消息时出现异常: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }

                // Console.WriteLine("成功发送 offset=" + (param.Start + send) + " " + current.Count.ToString());

                send += current.Count;
                current.Clear();
                if (batch_size == -1)
                    current.AddRange(rest);
                else
                {
                    // 将最多 batch_size 个元素从 rest 中移动到 current 中
                    for (int i = 0; i < batch_size && rest.Count > 0; i++)
                    {
                        current.Add(rest[0]);
                        rest.RemoveAt(0);
                    }
                }

                if (param.Data == null)
                    break;
            }

            Debug.Assert(rest.Count == 0, "");
            Debug.Assert(current.Count == 0, "");
            // return true;
            return new ResponseSearchResult
            {
                Value = 0,
                BatchSize = batch_size
            };
        ERROR1:
            // 报错
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseGetRes",
        new GetResResponse(
        param.TaskID,
        -1, // param.TotalLength,
        param.Start + send,
        param.Path,
        current.ToArray(),
        param.Metadata,
        param.Timestamp,
        strError,
        "_sendResponseGetResError"));
                // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
            }
            // return false;
            return new ResponseSearchResult
            {
                Value = -1,
                BatchSize = batch_size,
                ErrorInfo = strError
            };
        }

        /*
        static void ResponseGetRes(GetResResponse param)
        {
            try
            {
                MessageResult result = HubProxy.Invoke<MessageResult>("ResponseGetRes",
 param).Result;
            }
            catch
            {
            }
        }
        */

        // 下载本地文件
        // TODO: 限制 nMaxLength 最大值
        // parameters:
        //      strStyle    "uploadedPartial" 表示操作都是针对已上载临时部分的。比如希望获得这个局部的长度，时间戳，等等
        //                  "taskID" 在进行 taskResult 和 taskStop 操作时用 taskID 来指定任务 ID
        //                  "beginTask" 表示本次启动了任务但并不等待任务完成。outputTimestamp 参数会返回 taskID(byte [] 用 UTF-8 Encoding 解释)
        //                  "getTaskResult" 获取任务是否结束的信息和两个返回参数值
        //                  "stopTask" 停止一个任务
        // return:
        //      -2      文件不存在
        //		-1      出错
        //		>= 0	成功，返回最大长度
        static long GetFile(
            string strFilePath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out byte[] outputTimestamp,
            out string strError)
        {
            destBuffer = null;
            outputTimestamp = null;
            strError = "";

            bool isPartial = StringUtil.IsInList("uploadedPartial", strStyle);

            long lTotalLength = 0;
            strFilePath = strFilePath.Replace("/", "\\");

            FileInfo file = null;
            if (isPartial)
            {
                string strNewFileName = GetNewFileName(strFilePath);
                file = new FileInfo(strNewFileName);
                if (file.Exists == false)
                {
                    strError = " dp2Library 服务器不存在属于 '" + strFilePath + "' 的已上载局部文件";
                    return -2;
                }
            }
            else
            {
                file = new FileInfo(strFilePath);
                if (file.Exists == false)
                {
                    strError = " dp2Library 服务器不存在物理路径为 '" + strFilePath + "' 的文件";
                    return -2;
                }
            }
            file.Refresh();

            // 1.取时间戳
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                string strNewFileName = GetNewFileName(strFilePath);
                if (File.Exists(strNewFileName) == true)
                {
                    outputTimestamp = FileUtil.GetFileTimestamp(strNewFileName);
                }
                else
                {
                    outputTimestamp = FileUtil.GetFileTimestamp(strFilePath);
                }
            }

#if NO
            // 2.取元数据
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
                if (File.Exists(strMetadataFileName) == true)
                {
                    strMetadata = FileUtil.File2StringE(strMetadataFileName);
                }
            }
#endif

            // 3.取range
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                string strRangeFileName = GetRangeFileName(strFilePath);
                if (File.Exists(strRangeFileName) == true)
                {
                    string strText = FileUtil.File2StringE(strRangeFileName);
                    string strTotalLength = "";
                    string strRange = "";
                    StringUtil.ParseTwoPart(strText, "|", out strRange, out strTotalLength);
                }
            }

            // 4.长度
            lTotalLength = file.Length;
            // 这个长度有时候会有迟滞
            // https://stackoverflow.com/questions/7828132/getting-current-file-length-fileinfo-length-caching-and-stale-information

            // 2020/3/1
            // lTotalLength = GetFileLength(strFilePath);
            /*
            // 2020/2/29
            // 如果是正在获取当日的操作日志文件
            if (PathUtil.IsEqual(strFilePath, this.OperLog.CurrentFileName))
            {
                // 如果刚才通过 FileInfo.Length 获得的文件长度不准确
                if (lTotalLength < this.OperLog.GetCurrentStreamLength())
                {
                    // this.OperLog.ReOpen();
                    lTotalLength = this.OperLog.GetCurrentStreamLength();
                }
            }
            */

            // 5.有data风格时,才会取数据
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (nLength == 0)  // 取0长度
                {
                    destBuffer = new byte[0];
                    return lTotalLength;
                }

                // 检查范围是否合法
                // return:
                //		-1  出错
                //		0   成功
                int nRet = ConvertUtil.GetRealLengthNew(lStart,
                    nLength,
                    lTotalLength,
                    nMaxLength,
                    out long lOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (lOutputLength == 0)
                {
                    destBuffer = new byte[lOutputLength];
                }
                else
                {
                    using (FileStream s = new FileStream(strFilePath,
                        FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        destBuffer = new byte[lOutputLength];

                        Debug.Assert(lStart >= 0, "");

                        s.Seek(lStart, SeekOrigin.Begin);
                        int readed = s.Read(destBuffer,
                            0,
                            (int)lOutputLength);
                        if (readed < lOutputLength)
                        {
                            // 2017/9/4
                            strError = "希望从文件偏移 " + lStart + " 开始读入 " + lOutputLength + " 字节，但只成功读入了 " + readed + " 字节";
                            return -1;
                        }
                    }

                    /*
                    StreamItem s = this._physicalFileCache.GetStream(strFilePath,
    FileMode.Open,
    FileAccess.Read,
    lStart > 10 * 1024);
                    try
                    {
                        destBuffer = new byte[lOutputLength];

                        Debug.Assert(lStart >= 0, "");
                        Debug.Assert(s.FileStream != null, "");

                        s.FileStream.FastSeek(lStart);
                        int readed = s.FileStream.Read(destBuffer,
                            0,
                            (int)lOutputLength);
                        if (readed < lOutputLength)
                        {
                            strError = "希望从文件偏移 " + lStart + " 开始读入 " + lOutputLength + " 字节，但只成功读入了 " + readed + " 字节";
                            return -1;
                        }
                    }
                    finally
                    {
                        _physicalFileCache.ReturnStream(s);
                    }
                    */

                    /*
                    // 2020/2/29
                    // 顺序获取到最后一次，则清除缓存事项。这样可以确保后面再次获取 FileInfo 的时候能准确一些
                    if (lStart + lOutputLength >= lTotalLength)
                    {
                        _physicalFileCache.ClearItems(strFilePath);
                    }
                    */
                }
            }

            // TODO: 测试一下获取 30G 尺寸的文件的 MD5 需要多少时间
            // 取 MD5
            if (StringUtil.IsInList("md5", strStyle) == true)
            {
#if NO
                if (StringUtil.IsInList("beginTask", strStyle))
                {
                    var taskID = _md5Tasks.StartMd5Task(strFilePath);
                    outputTimestamp = Encoding.UTF8.GetBytes(taskID);
                }
                else if (StringUtil.IsInList("getTaskResult", strStyle)
                    || StringUtil.IsInList("stopTask", strStyle))
                {
                    var taskID = StringUtil.GetParameterByPrefix(strStyle, "taskID");
                    if (string.IsNullOrEmpty(taskID))
                    {
                        strError = "没有提供 taskID";
                        return -1;
                    }
                    var task = _md5Tasks.FindMd5Task(taskID);
                    if (task == null)
                    {
                        strError = $"没有找到 taskID 为 '{taskID}' 的 MD5 任务";
                        return -1;
                    }
                    if (StringUtil.IsInList("getTaskResult", strStyle))
                    {
                        if (task.Result == null)
                        {
                            outputTimestamp = null;
                            return 0;   // 表示任务尚未完成
                        }
                        outputTimestamp = ByteArray.GetTimeStampByteArray(task.Result.ErrorCode);
                        _md5Tasks.RemoveMd5Task(taskID);
                        return 1;   // 表示任务已经完成
                    }

                    _md5Tasks.StopMd5Task(taskID);
                    return 0;
                }
                else
#endif
                outputTimestamp = GetFileMd5(strFilePath);
            }

            return lTotalLength;
        }

        public static byte[] GetFileMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(
                        filename,
                        FileMode.Open,
                        FileAccess.Read, // 改过
                        FileShare.ReadWrite))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        // 得到 newdata 字段对应的文件名
        public static string GetNewFileName(string strFilePath)
        {
            return strFilePath + ".new~";
        }

        // 得到 range 字段对应的文件名
        public static string GetRangeFileName(string strFilePath)
        {
            return strFilePath + ".range~";
        }

        // 得到 timestamp 字段对应的文件名
        public static string GetTimestampFileName(string strFilePath)
        {
            return strFilePath + ".timestamp~";
        }


        #endregion


        #region SetInfo() API

        static void OnSetInfoRecieved(SetInfoRequest param)
        {
            // 单独给一个线程来执行
            _ = Task.Run(async () =>
            {
                try
                {
                    await SetInfoAndResponse(param);
                }
                catch (Exception ex)
                {
                    // 写入错误日志
                    WpfClientInfo.WriteErrorLog($"SetInfoAndResponse() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        public class ChangeHistoryResult : NormalResult
        {
            // 返回的实体
            public List<DigitalPlatform.MessageClient.Entity> ResultEntities { get; set; }
        }

        // 写入 RFID 标签
        async static Task<ChangeHistoryResult> CommandAsync(List<DigitalPlatform.MessageClient.Entity> actions)
        {
            List<DigitalPlatform.MessageClient.Entity> results = new List<DigitalPlatform.MessageClient.Entity>();

            foreach (var action in actions)
            {
                string action_string = action.Action;
                /*
                // 分离冒号左右部分
                var parts = StringUtil.ParseTwoPart(action_string, ":");
                string command = parts[0];
                string parameters = parts[1];
                */

                if (action_string.StartsWith("write tag"))
                {
                    var result = await WriteTagAsync(action_string, null);
                    var result_entity = new DigitalPlatform.MessageClient.Entity();
                    result_entity.Action = action.Action;
                    result_entity.OldRecord = null;
                    result_entity.ErrorCode = result.ErrorCode;
                    result_entity.ErrorInfo = result.ErrorInfo;

                    /*
                    if (result.Value == 1)
                    {
                        result_entity.NewRecord = new Record();
                        // result_entity.NewRecord.Data = result.ChangedRecord;
                    }
                    */

                    results.Add(result_entity);
                }
                /*
                if (command == "write")
                {
                    var result = await WriteRfidTagAsync(action.NewRecord.Data, parameters);
                    var result_entity = new DigitalPlatform.MessageClient.Entity();
                    result_entity.Action = action.Action;
                    result_entity.OldRecord = null;
                    result_entity.ErrorCode = result.ErrorCode;
                    result_entity.ErrorInfo = result.ErrorInfo;

                    if (result.Value == 1)
                    {
                        result_entity.NewRecord = new Record();
                        result_entity.NewRecord.Data = result.ChangedRecord;
                    }

                    results.Add(result_entity);
                }
                */
            }

            return new ChangeHistoryResult { ResultEntities = results };
        }


        // 写入一个 RFID 标签
        // parameters:
        //      text        一条 JSON 记录
        //      parameters  参数列表
        static async Task<ChangeResult> WriteRfidTagAsync(string text,
            string parameters)
        {
            return new ChangeResult();
        }

        // 对操作历史记录进行修改
        async static Task<ChangeHistoryResult> ChangeHistoryAsync(List<DigitalPlatform.MessageClient.Entity> actions)
        {
            List<DigitalPlatform.MessageClient.Entity> results = new List<DigitalPlatform.MessageClient.Entity>();

            foreach (var action in actions)
            {
                string action_string = action.Action;
                // 分离冒号左右部分
                var parts = StringUtil.ParseTwoPart(action_string, ":");
                string command = parts[0];
                string parameters = parts[1];

                if (command == "change")
                {
                    var result = await ChangeHistoryRecordAsync(action.NewRecord.Data, parameters);
                    var result_entity = new DigitalPlatform.MessageClient.Entity();
                    result_entity.Action = action.Action;
                    result_entity.OldRecord = null;
                    result_entity.ErrorCode = result.ErrorCode;
                    result_entity.ErrorInfo = result.ErrorInfo;

                    if (result.Value == 1)
                    {
                        result_entity.NewRecord = new Record();
                        result_entity.NewRecord.Data = result.ChangedRecord;
                    }

                    results.Add(result_entity);
                }
            }

            return new ChangeHistoryResult { ResultEntities = results };
        }

        public class ChangeResult : NormalResult
        {
            // 修改后的记录
            public string ChangedRecord { get; set; }
        }

        // 修改一条操作历史记录
        // parameters:
        //      text    一条 JSON 记录。里面的 ID 字段用于定位要修改的记录；State 字段内容是要修改成的该字段内容
        static async Task<ChangeResult> ChangeHistoryRecordAsync(string text,
            string parameters)
        {
            // 解析 parameters
            var fields = StringUtil.SplitList(parameters, '|');

            if (fields.Count == 0)
                return new ChangeResult
                {
                    Value = -1,
                    ErrorInfo = "action 中缺乏冒号以后的参数"
                };

            // 从 record 中取出 ID 字符串
            var record = DisplayRequestItem.FromDisplayString(text);
            if (record.ID == 0)
            {
                return new ChangeResult
                {
                    Value = -1,
                    ErrorInfo = "新记录内容中没有包含 ID 字段"
                };
            }
            // 检索出以前的记录

            // 修改以后保存

            using (var context = new RequestContext())
            {
                context.Database.EnsureCreated();

                var item = context.Requests.Where(o => o.ID == record.ID).FirstOrDefault();
                if (item == null)
                {
                    return new ChangeResult
                    {
                        Value = -1,
                        ErrorInfo = $"没有找到 ID 为 '{record.ID}' 的操作历史记录"
                    };
                }

                int count = 0;
                bool changed = false;
                if (fields.IndexOf("state") != -1)
                {
                    count++;
                    string new_value = record.State;
                    if (isStateValid(new_value) == false)
                    {
                        return new ChangeResult
                        {
                            Value = -1,
                            ErrorInfo = $"要修改为新的 State 值 '{new_value}' 不合法。修改操作被拒绝"
                        };
                    }
                    // TODO: 是否自动写入一个附注字段内容，记载修改前的内容，和修改的原因(comment=xxx)？
                    item.State = new_value;
                    changed = true;
                }
                else if (fields.IndexOf("linkID") != -1) // 2021/8/17
                {
                    count++;
                    string new_value = record.LinkID;
                    if (new_value == "[null]")
                        new_value = null;
                    /*
                    if (isLinkIDValid(new_value) == false)
                    {
                        return new ChangeResult
                        {
                            Value = -1,
                            ErrorInfo = $"要修改为新的 State 值 '{new_value}' 不合法。修改操作被拒绝"
                        };
                    }
                    */

                    // TODO: 是否自动写入一个附注字段内容，记载修改前的内容，和修改的原因(comment=xxx)？
                    item.LinkID = new_value;
                    changed = true;
                }

                if (count == 0)
                {
                    return new ChangeResult
                    {
                        Value = -1,
                        ErrorInfo = $"修改字段名列表 '{parameters}' 中没有可支持的部分"
                    };
                }

                if (changed == true)
                {
                    await context.SaveChangesAsync();

                    return new ChangeResult
                    {
                        Value = 1,  // 表示修改了
                        ChangedRecord = DisplayRequestItem.GetDisplayString(item)
                    };
                }
                else
                    return new ChangeResult
                    {
                        Value = 0,  // 表示没有发生修改
                        ChangedRecord = DisplayRequestItem.GetDisplayString(item)
                    };
            }

        }

        static async Task SetInfoAndResponse(SetInfoRequest param)
        {
            string strError = "";

            try
            {
                ChangeHistoryResult result = null;
                if (param.Operation == "setHistory")
                    result = await ChangeHistoryAsync(param.Entities);
                else if (param.Operation == "command")
                    result = await CommandAsync(param.Entities);
                else
                {
                    strError = "无法识别的 param.Operation 值 '" + param.Operation + "'";
                    goto ERROR1;
                }

                await ResponseSetInfo(param.TaskID,
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
            await TryResponseSetInfo(
param.TaskID,
-1,
new List<DigitalPlatform.MessageClient.Entity>(),   // results
strError);

        }

        // 调用 server 端 ResponseSetInfo
        // TODO: 要考虑发送失败的问题
        public static async Task<MessageResult> ResponseSetInfo(
            string taskID,
            long resultValue,
            IList<DigitalPlatform.MessageClient.Entity> results,
            string errorInfo)
        {
            return await HubProxy.Invoke<MessageResult>("ResponseSetInfo",
        taskID,
        resultValue,
        results,
        errorInfo).ConfigureAwait(false);
        }

        public static async Task TryResponseSetInfo(
        string taskID,
        long resultValue,
        IList<DigitalPlatform.MessageClient.Entity> results,
        string errorInfo)
        {
            try
            {
                await HubProxy.Invoke<MessageResult>("ResponseSetInfo",
        taskID,
        resultValue,
        results,
        errorInfo).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"TryResponseSetInfo() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        #endregion

        #region GetUsers()

        public static Task<GetUserResult> GetUsersAsync(string userName,
            int start,
            int count)
        {
            return HubProxy.Invoke<GetUserResult>("GetUsers",
                userName,
                start,
                count);
        }

        #endregion

        #region GetConnectionInfo() API

        public static async Task<GetConnectionInfoResult> GetConnectionInfoAsync(
GetConnectionInfoRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            GetConnectionInfoResult result = new GetConnectionInfoResult();
            if (result.Records == null)
                result.Records = new List<ConnectionRecord>();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<
                    string, long, long, IList<ConnectionRecord>, string, string>(
                    "responseGetConnectionInfo",
                    (taskID, resultCount, start, records, errorInfo, errorCode) =>
                    {
                        if (taskID != request.TaskID)
                            return;

                        // 装载命中结果
                        if (resultCount == -1 || start == -1)
                        {
                            if (start == -1)
                            {
                                // 表示发送响应过程已经结束。只是起到通知的作用，不携带任何信息
                                // result.Finished = true;
                            }
                            else
                            {
                                result.ResultCount = resultCount;
                                result.ErrorInfo = errorInfo;
                                result.ErrorCode = errorCode;
                            }
                            wait_events.finish_event.Set();
                            return;
                        }

                        result.ResultCount = resultCount;
                        // TODO: 似乎应该关注 start 位置
                        result.Records.AddRange(records);
                        result.ErrorInfo = errorInfo;
                        result.ErrorCode = errorCode;

                        if (IsComplete(request.Start, request.Count, resultCount, result.Records.Count) == true)
                            wait_events.finish_event.Set();
                        else
                            wait_events.active_event.Set();
                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestGetConnectionInfo",
        request).ConfigureAwait(false);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.ResultCount = -1;
                        result.ErrorCode = message.String;
                        return result;
                    }

                    // result.String 里面是返回的 taskID

                    // start_time = DateTime.Now;

                    await WaitAsync(
    request.TaskID,
    wait_events,
    timeout,
    token).ConfigureAwait(false);
                    return result;
                }
            }
        }


        #endregion

    }
}
