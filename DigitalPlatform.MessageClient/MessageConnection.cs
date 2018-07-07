// #define TIMER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;

using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// 实现热点功能的一个连接，基础类
    /// 负责处理收发消息
    /// </summary>
    public class MessageConnection
    {
        public event ConnectionEventHandler ConnectionStateChange = null;

        private IHubProxy HubProxy
        {
            get;
            set;
        }

        private HubConnection Connection
        {
            get;
            set;
        }

#if TIMER
        System.Timers.Timer _timer = new System.Timers.Timer();
#endif

        bool _exiting = false;  // 是否处在正在退出过程

        public virtual string dp2MServerUrl
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string Parameters
        {
            get;
            set;
        }


        public virtual void Initial()
        {
#if TIMER
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;
#endif

            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false)
            {
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                ConnectAsync(
                    // this.dp2MServerUrl
                    );
            }
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Connection != null)
                AddInfoLine("tick connection state = " + this.Connection.State.ToString());

            if (this.Connection == null ||
                this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
            {
                AddInfoLine("自动重新连接 ...");
                this.Connect();
            }
        }

        public bool IsConnected
        {
            get
            {
                return this.Connection.State == ConnectionState.Connected;
            }
        }

        // 确保连接和登录
        public void Connect()
        {
            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false
                && (this.Connection == null || this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected))
            {
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                ConnectAsync(//this.dp2MServerUrl
                    );
            }
        }

        public virtual void Destroy()
        {
#if TIMER
            _timer.Stop();
#endif
            _exiting = true;
            CloseConnection();
        }

        #region 显示信息

        public virtual void AddErrorLine(string strContent)
        {
            OutputText(strContent, 2);
        }

        public virtual void AddInfoLine(string strContent)
        {
            OutputText(strContent, 0);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public virtual void OutputText(string strText, int nWarningLevel = 0)
        {
        }

        #endregion

        // StreamWriter _writer = null;

        // 连接 server
        // 要求调用前设置好 this.ServerUrl this.UserName this.Password this.Parameters
        private void ConnectAsync(
            // string strServerUrl
            )
        {
            AddInfoLine("正在连接服务器 " + this.dp2MServerUrl + " ...");

            Connection = new HubConnection(this.dp2MServerUrl);
            Connection.Closed += new Action(Connection_Closed);
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;

#if NO
            if (_writer == null)
            {
                _writer = new StreamWriter("c:\\log.txt", true, Encoding.UTF8);
                _writer.AutoFlush = true;
            }
            Connection.TraceLevel = TraceLevels.All;
            Connection.TraceWriter = _writer;
#endif

            Connection.Headers.Add("username", this.UserName);
            Connection.Headers.Add("password", this.Password);
            Connection.Headers.Add("parameters", this.Parameters);

            HubProxy = Connection.CreateHubProxy("MyHub");

            HubProxy.On<string, IList<MessageRecord>>("addMessage",
                (name, messages) =>
                OnAddMessageRecieved(name, messages)
                );

            HubProxy.On<SearchRequest>("search",
                (searchParam) => OnSearchBiblioRecieved(searchParam)
                );

            HubProxy.On<SearchResponse>("responseSearch", (responseParam) =>
 OnSearchResponseRecieved(responseParam)
);
            HubProxy.On<SetInfoRequest>("setInfo",
            (searchParam) => OnSetInfoRecieved(searchParam)
            );

#if NO
            Task task = Connection.Start();
#if NO
            CancellationTokenSource token = new CancellationTokenSource();
            if (!task.Wait(60 * 1000, token.Token))
            {
                token.Cancel();
                // labelStatusText.Text = "time out";
                AddMessageLine("error", "time out");
                return;
            }
#endif
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
#if NO
                if (task.Exception is HttpRequestException)
                    labelStatusText.Text = "Unable to connect to server: start server bofore connection client.";
#endif
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }


            AddInfoLine("停止 Timer");
            _timer.Stop();

            //EnableControls(true);
            //textBox_input.Focus();
            AddInfoLine("成功连接到 " + strServerUrl);

            this.MainForm.BeginInvoke(new Action(Login));
#endif
            try
            {
                Connection.Start()  // new ServerSentEventsTransport()
                    .ContinueWith((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            AddErrorLine(GetExceptionText(antecendent.Exception));
                            return;
                        }
#if TIMER
                        AddInfoLine("停止 Timer");
                        _timer.Stop();
#endif
                        AddInfoLine("成功连接到 " + this.dp2MServerUrl);
                        // Login();
                        TriggerConnectionStateChange("Connected");

                    });
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
                return;
            }
        }

#if NO
        // 连接成功后被调用，执行登录功能。重载时要调用 Login(...) 向 server 发送 login 消息
        public virtual void Login()
        {

        }
#endif

        void Connection_Reconnecting()
        {
            // tryingToReconnect = true;
            TriggerConnectionStateChange("Reconnecting");
        }

        void TriggerConnectionStateChange(string strAction)
        {
            ConnectionEventHandler handler = this.ConnectionStateChange;
            if (handler != null)
            {
                ConnectionEventArgs e = new ConnectionEventArgs();
                e.Action = strAction;
                handler(this, e);
            }
        }

        void Connection_Reconnected()
        {
            // tryingToReconnect = false;

            AddInfoLine("Connection_Reconnected");

            TriggerConnectionStateChange("Reconnected");

            // this.Login();
        }

        void Connection_Closed()
        {

            if (_exiting == false)
            {
#if TIMER
                AddInfoLine("开启 Timer");
                _timer.Start();
#endif
            }

            TriggerConnectionStateChange("Closed");

#if NO
            this.Invoke((Action)(() => panelChat.Visible = false));
            this.Invoke((Action)(() => buttonSend.Enabled = false));
            this.Invoke((Action)(() => this.labelStatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => this.panelSignIn.Visible = true));
#endif

        }

        void Connection_Error(Exception obj)
        {
            AddErrorLine(obj.ToString());
        }

        public virtual void OnAddMessageRecieved(string action,
    IList<MessageRecord> messages)
        {
            AddMessageEventArgs e = new AddMessageEventArgs();
            e.Action = action;
            e.Records = new List<MessageRecord>();
            e.Records.AddRange(messages);
            this.TriggerAddMessage(this, e);
        }

        public event AddMessageEventHandler AddMessage = null;

        // 触发消息通知事件
        public virtual void TriggerAddMessage(MessageConnection connection,
            AddMessageEventArgs e)
        {
            AddMessageEventHandler handler = this.AddMessage;
            if (handler != null)
            {
                handler(connection, e);
            }
        }

        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public virtual void OnSearchBiblioRecieved(
#if NO
            string searchID,
            string operation,
            string dbNameList,
             string queryWord,
             string fromList,
             string matchStyle,
             string formatList,
             long maxResults
#endif
SearchRequest param
            )
        {
        }

        public virtual void OnSetInfoRecieved(SetInfoRequest request)
        {

        }

        // 当 server 发来检索响应的时候被调用。重载时可以显示收到的记录
        public virtual void OnSearchResponseRecieved(
#if NO
            string searchID,
    long resultCount,
    long start,
    IList<Record> records,
    string errorInfo,
            string errorCode
#endif
            SearchResponse responseParam)
        {
        }

        // 关闭连接，并且不会引起自动重连接
        public void CloseConnection()
        {
            if (this.Connection != null)
            {
                Connection.Closed -= new Action(Connection_Closed);
                /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxxxxxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 Microsoft.AspNet.SignalR.Client.Connection.Stop(TimeSpan timeout)
在 dp2Circulation.MessageHub.CloseConnection()
在 dp2Circulation.MessageHub.Close()
在 dp2Circulation.MainForm.MainForm_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.WmClose(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 dp2Circulation.MainForm.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.4.5697.17821, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/7 10:51:56 (Fri, 07 Aug 2015 10:51:56 +0800) 
前端地址 xxxxx 经由 http://dp2003.com/dp2library 

                 * 
                 * */
                try
                {
                    this.Connection.Stop(new TimeSpan(0, 0, 5));
                }
                catch (System.NullReferenceException)
                {
                }
                this.Connection = null;
            }
        }

#if NO
        // 发起一次书目检索
        // 发起检索成功后，调主应该用 SearchResponseEvent 事件接收检索结果
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索
        public int BeginSearchBiblio(
            string inputSearchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults,
            out string outputSearchID,
            out string strError)
        {
            strError = "";
            outputSearchID = "";

            try
            {

                Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearchBiblio",
                    inputSearchID,
                    dbNameList,
                    queryWord,
                    fromList,
                    matchStyle,
                    formatList,
                    maxResults);

                while (task.IsCompleted == false)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                }

                if (task.IsFaulted == true)
                {
                    // AddErrorLine(GetExceptionText(task.Exception));
                    strError = GetExceptionText(task.Exception);
                    return -1;
                }

                MessageResult result = task.Result;
                if (result.Value == -1)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    return -1;
                }
                if (result.Value == 0)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    return 0;
                }
                // AddMessageLine("search ID:", result.String);
                outputSearchID = result.String;
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }
#endif
        public static string GetExceptionText(AggregateException exception)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                text.Append(ex.Message + "\r\n");
                // text.Append(ex.ToString() + "\r\n");
            }

            return text.ToString();
        }

#if NO
        // 等待 Task 结束。重载时可以在其中加入出让界面控制权，或者显示进度的功能
        public virtual void WaitTaskComplete(Task task)
        {
#if NO
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }
#endif
            task.Wait();
        }
#endif

        #region 调用 Server 端函数

        // 发起一次书目检索
        // 发起检索成功后，调主应该用 SearchResponseEvent 事件接收检索结果
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索
        public int BeginSearchBiblio(
            string userNameList,
            SearchRequest searchParam,
            out string outputSearchID,
            out string strError)
        {
            strError = "";
            outputSearchID = "";

            AddInfoLine("BeginSearchBiblio "
                + "; userNameList=" + userNameList
#if NO
                + "; operation=" + operation
                + "; searchID=" + inputSearchID
                + "; dbNameList=" + dbNameList
                + "; queryWord=" + queryWord
                + "; fromList=" + fromList
                + "; matchStyle=" + matchStyle
                + "; formatList=" + formatList
                + "; maxResults=" + maxResults
#endif
);
            try
            {
                MessageResult result = HubProxy.Invoke<MessageResult>("RequestSearch",
                    userNameList,
                    searchParam).Result;

#if NO
                if (task.IsFaulted == true)
                {
                    // AddErrorLine(GetExceptionText(task.Exception));
                    strError = GetExceptionText(task.Exception);
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.TaskID
    + "; return error=" + strError + " value="
    + -1);
                    return -1;
                }


                MessageResult result = task.Result;
#endif
                if (result.Value == -1)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.TaskID
    + "; return error=" + strError + " value="
    + -1);
                    return -1;
                }
                if (result.Value == 0)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.TaskID
   + "; return error=" + strError + " value="
   + 0);
                    return 0;
                }
                // AddMessageLine("search ID:", result.String);
                outputSearchID = result.String;
                AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.TaskID
+ "; return value="
+ 1);
                return (int)result.Value;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.TaskID
+ "; return error=" + strError + " value="
+ -1);
                return -1;
            }
        }

        DateTime _lastTime = DateTime.Now;

        // 和上次操作的时刻之间，等待至少这么多时间。
        void Wait(TimeSpan length)
        {
            DateTime now = DateTime.Now;
            TimeSpan delta = now - _lastTime;
            if (delta < length)
            {
                // Console.WriteLine("Sleep " + (length - delta).ToString());
                Thread.Sleep(length - delta);
            }
            _lastTime = DateTime.Now;
        }

        // TODO: 注意测试，一次只能发送一个元素，或者连一个元素都发送不成功的情况
        // 具有重试机制的 ReponseSearch
        // 运行策略是，当遇到 InvalidOperationException 异常时，减少一半数量重试发送，用多次小批发送解决问题
        // 如果最终无法完成发送，则尝试发送一条报错信息，然后返回 false
        // parameters:
        //      batch_size  建议的最佳一次发送数目。-1 表示不限制
        // return:
        //      true    成功
        //      false   失败
        public bool TryResponseSearch(string taskID,
            long resultCount,
            long start,
            string libraryUID,
            IList<Record> records,
            string errorInfo,
            string errorCode,
            ref long batch_size)
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
                    Wait(new TimeSpan(0, 0, 0, 0, 50));

                    MessageResult result = ResponseSearchAsync(
                        new SearchResponse(
                        taskID,
                        resultCount,
                        start + send,
                        libraryUID,
                        current,
                        errorInfo,
                        errorCode)).Result;
                    _lastTime = DateTime.Now;
                    if (result.Value == -1)
                        return false;   // 可能因为服务器端已经中断此 taskID，或者执行 ReponseSearch() 时出错
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

                Console.WriteLine("成功发送 " + current.Count.ToString());

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
            return true;
        ERROR1:
            // 报错
            ResponseSearch(
                new SearchResponse(
taskID,
-1,
0,
libraryUID,
new List<Record>(),
strError,
"_sendResponseSearchError"));    // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
            return false;
        }

        // 调用 server 端 ResponseSearchBiblio
        public Task<MessageResult> ResponseSearchAsync(
#if NO
            string taskID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode
#endif
            SearchResponse responseParam)
        {
            return HubProxy.Invoke<MessageResult>("ResponseSearch",
#if NO
taskID,
resultCount,
start,
records,
errorInfo,
errorCode
#endif
                responseParam);
        }

        // 调用 server 端 ResponseSearchBiblio
        public async void ResponseSearch(
#if NO
            string taskID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode
#endif
            SearchResponse responseParam)
        {
            // TODO: 等待执行完成。如果有异常要当时处理。比如减小尺寸重发。
            int nRedoCount = 0;
        REDO:
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
#if NO
                    taskID,
    resultCount,
    start,
    records,
    errorInfo,
    errorCode
#endif
                    responseParam);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
                if (ex.InnerException is InvalidOperationException
                    && nRedoCount < 2)
                {
                    nRedoCount++;
                    Thread.Sleep(1000);
                    goto REDO;
                }
            }
        }
#if NO
        // 调用 server 端 ResponseSearch
        public async void ResponseSearch(
            string searchID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
    searchID,
    resultCount,
    start,
    records,
    errorInfo,
    errorCode);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }


#if NO
            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("ResponseSearchBiblio",
searchID,
resultCount,
start,
records,
errorInfo);
            task.Wait();
            if (task.IsFaulted == true)
            {
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }
            if (task.Result.Value == -1)
            {
                AddErrorLine(task.Result.ErrorInfo);
            }
#endif
        }
#endif

        // 调用 server 端 ResponseSetInfo
        public async void ResponseSetInfo(
            string taskID,
            long resultValue,
            IList<Entity> entities,
            string errorInfo)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSetInfo",
    taskID,
    resultValue,
    entities,
    errorInfo);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }
        }

        public Task<GetUserResult> GetUsers(string userName, int start, int count)
        {
#if NO
            var task = HubProxy.Invoke<GetUserResult>("GetUsers",
                userName,
                start,
                count);
            task.Wait();
            return task.Result;
#endif
            return HubProxy.Invoke<GetUserResult>("GetUsers",
                userName,
                start,
                count);
        }

        public Task<MessageResult> SetUsers(string action, List<User> users)
        {
#if NO
            var task = HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users);
            task.Wait();
            return task.Result;
#endif
            return HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users);
        }

#if NO
        // 调用 server 端 Login
        public async void Login(
            LoginRequest param)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("Login",
                    param);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
                AddInfoLine("成功登录。属性为 " + param.PropertyList);
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }
        }
#endif

        #endregion

        #region GetMessage() API

        // 请求服务器中断一个 task
        public Task<MessageResult> CancelSearchAsync(string taskID)
        {
            return HubProxy.Invoke<MessageResult>(
                "CancelSearch",
                taskID);
        }

        class WaitEvents : IDisposable
        {
            public ManualResetEvent finish_event = new ManualResetEvent(false);    // 表示数据全部到来
            public AutoResetEvent active_event = new AutoResetEvent(false);    // 表示中途数据到来

            public virtual void Dispose()
            {
                finish_event.Dispose();
                active_event.Dispose();
            }
        }

        void Wait(string taskID,
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
                    false);

                if (index == 0) // 正常完成
                    return; //  result;
                else if (index == 1)
                {
                    start_time = DateTime.Now;  // 重新计算超时开始时刻
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
                        // 向服务器发送 CancelSearch 请求
                        CancelSearchAsync(taskID);
                        throw new TimeoutException("已超时 " + timeout.ToString());
                    }
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

        public delegate void Delegate_outputMessage(long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode);

        public Task<MessageResult> GetMessageAsync(
            GetMessageRequest request,
            Delegate_outputMessage proc,
            TimeSpan timeout,
            CancellationToken token)
        {
            return Task.Factory.StartNew<MessageResult>(
                () =>
                {
                    MessageResult result = new MessageResult();

                    if (string.IsNullOrEmpty(request.TaskID) == true)
                        request.TaskID = Guid.NewGuid().ToString();

                    long recieved = 0;

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

                                proc(resultCount,
                                    start,
                                    records,
                                    errorInfo,
                                    errorCode);

                                if (records != null)
                                    recieved += records.Count;

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
                            MessageResult temp = HubProxy.Invoke<MessageResult>(
"RequestGetMessage",
request).Result;
                            if (temp.Value == -1 || temp.Value == 0)
                                return temp;

                            // result.String 里面是返回的 taskID

                            Wait(
            request.TaskID,
            wait_events,
            timeout,
            token);
                            return result;
                        }
                    }
                },
            token);
        }

        #endregion

        #region SetMessage() API

        public Task<SetMessageResult> SetMessageAsync(
            SetMessageRequest param)
        {
            return HubProxy.Invoke<SetMessageResult>(
 "SetMessage",
 param);
        }

        #endregion
    }

    public class MessageResult
    {
        public string String { get; set; }  // 字符串类型的返回值
        public long Value { get; set; }      // 整数类型的返回值
        public string ErrorInfo { get; set; }   // 出错信息

        public void SetError(string errorInfo, string errorCode)
        {
            this.ErrorInfo = errorInfo;
            this.String = errorCode;
            this.Value = -1;
        }
    }

    public class Record
    {
        // 记录路径。可能是本地路径，例如 “图书总库/1”；也可能是全局路径，例如“图书总库@xxxxxxx”
        public string RecPath { get; set; }
        public string Format { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }

        public string MD5 { get; set; } // Data 的 MD5 hash
    }

    public class GetUserResult : MessageResult
    {
        public List<User> Users { get; set; }
    }

    public class User
    {
        public string id { get; set; }

        public string userName { get; set; } // 用户名
        public string password { get; set; }  // 密码
        public string rights { get; set; } // 权限
        public string duty { get; set; } // 义务
        public string department { get; set; } // 部门名称
        public string tel { get; set; }  // 电话号码
        public string comment { get; set; }  // 注释
        public string [] groups { get; set; }
        public string binding { get; set; } // 绑定信息
    }

    // 2016/10/23
    public class LoginInfo
    {
        public string UserName { get; set; }    // 用户名。指 dp2library 的用户名。如果 Type 为 "Patron"，表示这是一个读者。2016/10/21
        public string UserType { get; set; }    // 用户类型。patron 表示读者，其他表示工作人员
        public string Password { get; set; }    // 密码。如果为 null，表示用代理方式登录
        public string Style { get; set; }       // 登录方式

        public LoginInfo()
        {

        }

        public LoginInfo(string userName, bool isPatron)
        {
            this.UserName = userName;
            if (isPatron)
                this.UserType = "patron";
        }

        public LoginInfo(string userName,
            bool isPatron,
            string password,
            string style)
        {
            this.UserName = userName;
            if (isPatron)
                this.UserType = "patron";
            this.Password = password;
            this.Style = style;
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (string.IsNullOrEmpty(this.UserName) == false)
                text.Append("UserName=" + this.UserName + ";");
            if (string.IsNullOrEmpty(this.UserType) == false)
                text.Append("UserType=" + this.UserType + ";");
            if (string.IsNullOrEmpty(this.Password) == false)
                text.Append("Password=" + this.Password + ";");
            if (string.IsNullOrEmpty(this.Style) == false)
                text.Append("Style=" + this.Style + ";");
            return text.ToString();
        }
    }


    public class SearchRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作

        public LoginInfo LoginInfo { get; set; }    // 登录信息 2016/10/22

        public string Operation { get; set; }   // 操作名。
        public string DbNameList { get; set; }  // 数据库名列表。一般为 "<全部>"
        public string QueryWord { get; set; }   // 检索词。若为 !getResult 表示不检索、从已有结果集中获取记录
        public string UseList { get; set; }     // 检索途径列表
        public string MatchStyle { get; set; }  // 匹配方式。为 exact/left/right/middle 之一
        public string ResultSetName { get; set; }   // 检索创建的结果集名。空表示为默认结果集
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多
        public string ServerPushEncoding { get; set; }

        public SearchRequest(string taskID,
            LoginInfo loginInfo,
            string operation,
            string dbNameList,
            string queryWord,
            string useList,
            string matchStyle,
            string resultSetName,
            string formatList,
            long maxResults,
            long start,
            long count,
            string serverPushEncoding = "")
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
            this.Operation = operation;
            this.DbNameList = dbNameList;
            this.QueryWord = queryWord;
            this.UseList = useList;
            this.MatchStyle = matchStyle;
            this.ResultSetName = resultSetName;
            this.FormatList = formatList;
            this.MaxResults = maxResults;
            this.Start = start;
            this.Count = count;
            this.ServerPushEncoding = serverPushEncoding;
        }
    }

    public class SearchResponse
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public long ResultCount { get; set; }
        public long Start { get; set; }    // 本次响应的偏移
        public string LibraryUID { get; set; }  // 响应者的 UID。这样 Record.RecPath 中就记载短路径即可
        public IList<Record> Records { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }

        public SearchResponse(string taskID,
            long resultCount,
            long start,
            string libraryUID,
            IList<Record> records,
            string errorInfo,
            string errorCode)
        {
            this.TaskID = taskID;
            this.ResultCount = resultCount;
            this.Start = start;
            this.LibraryUID = libraryUID;
            this.Records = records;
            this.ErrorInfo = errorInfo;
            this.ErrorCode = errorCode;
        }
    }


    public class SetInfoRequest
    {
        public string TaskID { get; set; }    // 任务 ID。由于一个 Connection 可以用于同时执行多个任务，本参数用于区分不同的任务
        public string Operation { get; set; }   // 操作名。

        public string BiblioRecPath { get; set; }
        public List<Entity> Entities { get; set; }
    }

    public class Entity
    {
        public string Action { get; set; }   // 要执行的操作(get时此项无用)

        public string RefID { get; set; }   // 参考 ID

        public Record OldRecord { get; set; }

        public Record NewRecord { get; set; }

        public string Style { get; set; }   // 风格。常用作附加的特性参数。例如: nocheckdup,noeventlog,force

        public string ErrorInfo { get; set; }  // 出错信息

        public string ErrorCode { get; set; }   // 出错码（表示属于何种类型的错误）
    }

#if NO
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public string LibraryUserName { get; set; }
        public string LibraryUID { get; set; }
        public string LibraryName { get; set; }
        public string PropertyList { get; set; }
    }
#endif

    public class MessageRecord
    {
        public string id { get; set; }  // 消息的 id

        public string[] groups { get; set; }   // 组名 或 组id。消息所从属的组
        public string creator { get; set; } // 创建消息的人。也就是发送消息的用户名或 id
        public string userName { get; set; } // 创建消息的人的用户名
        public string data { get; set; }  // 消息数据体
        public string format { get; set; } // 消息格式。格式是从存储格式角度来说的
        public string type { get; set; }    // 消息类型。类型是从用途角度来说的
        public string thread { get; set; }    // 消息所从属的话题线索

        public DateTime publishTime { get; set; } // 消息发布时间
        public DateTime expireTime { get; set; } // 消息失效时间
    }

    public class SetMessageRequest
    {
        public string Action { get; set; }
        public string Style { get; set; }
        public List<MessageRecord> Records { get; set; }

        public SetMessageRequest(string action,
            string style,
            List<MessageRecord> records)
        {
            this.Action = action;
            this.Style = style;
            this.Records = records;
        }
    }

    public class SetMessageResult : MessageResult
    {
        public List<MessageRecord> Results { get; set; }    // 返回的实际被创建或者修改的消息
    }

    public class GetMessageRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作

        public string GroupCondition { get; set; }
        public string UserCondition { get; set; }
        public string TimeCondition { get; set; }

        public long Start { get; set; }
        public long Count { get; set; }

        public GetMessageRequest(string taskID,
            string groupCondition,
            string userCondition,
            string timeCondition,
            long start,
            long count)
        {
            this.TaskID = taskID;
            this.GroupCondition = groupCondition;
            this.UserCondition = userCondition;
            this.TimeCondition = timeCondition;
            this.Start = start;
            this.Count = count;
        }
    }

    public class GetMessageResult : MessageResult
    {
        public List<MessageRecord> Results { get; set; }
    }

    /// <summary>
    /// 消息通知事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void AddMessageEventHandler(object sender,
        AddMessageEventArgs e);

    /// <summary>
    /// 消息通知事件的参数
    /// </summary>
    public class AddMessageEventArgs : EventArgs
    {
        public string Action = "";
        public List<MessageRecord> Records = null;
    }

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ConnectionEventHandler(object sender,
        ConnectionEventArgs e);

    /// <summary>
    /// 连接状态变化事件的参数
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        public string Action = "";
    }
}
