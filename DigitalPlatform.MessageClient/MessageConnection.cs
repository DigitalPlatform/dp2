using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR.Client;

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// 实现热点功能的一个连接，基础类
    /// 负责处理收发消息
    /// </summary>
    public class MessageConnection
    {
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

        System.Timers.Timer _timer = new System.Timers.Timer();

        bool _exiting = false;  // 是否处在正在退出过程

        public virtual string dp2MServerUrl
        {
            get;
            set;
        }

        public virtual void Initial()
        {
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;

            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false)
            {
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                ConnectAsync(this.dp2MServerUrl);
            }
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            AddInfoLine("tick connection state = " + this.Connection.State.ToString());

            if (this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
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
                ConnectAsync(this.dp2MServerUrl);
            }
        }

        public virtual void Destroy()
        {
            _timer.Stop();
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

        // 连接 server
        private void ConnectAsync(string strServerUrl)
        {
            AddInfoLine("正在连接服务器 " + strServerUrl + " ...");

            Connection = new HubConnection(strServerUrl);
            Connection.Closed += new Action(Connection_Closed);
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;

            HubProxy = Connection.CreateHubProxy("MyHub");

            HubProxy.On<string, string>("AddMessage",
                (name, message) =>
                OnAddMessageRecieved(name, message)
                );

            HubProxy.On<SearchRequest>("search",
                (searchParam) => OnSearchBiblioRecieved(searchParam)
                );

            HubProxy.On<string,
    long,
    long,
    IList<Record>,
        string>("responseSearch", (searchID,
    resultCount,
    start,
    records,
    errorInfo) =>
 OnSearchResponseRecieved(searchID,
    resultCount,
    start,
    records,
    errorInfo)

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
                Connection.Start()
                    .ContinueWith((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            AddErrorLine(GetExceptionText(antecendent.Exception));
                            return;
                        }
                        AddInfoLine("停止 Timer");
                        _timer.Stop();
                        AddInfoLine("成功连接到 " + strServerUrl);
                        Login();
                    });
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
                return;
            }
        }

        // 连接成功后被调用，执行登录功能。重载时要调用 Login(...) 向 server 发送 login 消息
        public virtual void Login()
        {

        }

        void Connection_Reconnecting()
        {
            // tryingToReconnect = true;
        }

        void Connection_Reconnected()
        {
            // tryingToReconnect = false;

            AddInfoLine("Connection_Reconnected");

            this.Login();
        }

        void Connection_Closed()
        {
            if (_exiting == false)
            {
                AddInfoLine("开启 Timer");
                _timer.Start();
            }
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

        public virtual void OnAddMessageRecieved(string strName, string strContent)
        {

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

        // 当 server 发来检索响应的时候被调用。重载时可以显示收到的记录
        public virtual void OnSearchResponseRecieved(string searchID,
    long resultCount,
    long start,
    IList<Record> records,
    string errorInfo)
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

        #region 调用 Server 端函数

        // 发起一次书目检索
        // 发起检索成功后，调主应该用 SearchResponseEvent 事件接收检索结果
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索
        public int BeginSearchBiblio(
            string userNameList,
#if NO
            string operation,
            string inputSearchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults,
#endif
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
                Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearch",
#if NO
                    inputSearchID,
                    dbNameList,
                    queryWord,
                    fromList,
                    matchStyle,
                    formatList,
                    maxResults
#endif
                    searchParam
                    );

#if NO
                while (task.IsCompleted == false)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                }
#endif
                WaitTaskComplete(task);

                if (task.IsFaulted == true)
                {
                    // AddErrorLine(GetExceptionText(task.Exception));
                    strError = GetExceptionText(task.Exception);
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
    + "; return error=" + strError + " value="
    + -1);
                    return -1;
                }

                MessageResult result = task.Result;
                if (result.Value == -1)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
    + "; return error=" + strError + " value="
    + -1);
                    return -1;
                }
                if (result.Value == 0)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
   + "; return error=" + strError + " value="
   + 0);
                    return 0;
                }
                // AddMessageLine("search ID:", result.String);
                outputSearchID = result.String;
                AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
+ "; return value="
+ 1);
                return 1;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
+ "; return error=" + strError + " value="
+ -1);
                return -1;
            }
        }

        // 调用 server 端 ResponseSearchBiblio
        public async void Response(
            string searchID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
    searchID,
    resultCount,
    start,
    records,
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

        public GetUserResult GetUsers(string userName, int start, int count)
        {
            var task = HubProxy.Invoke<GetUserResult>("GetUsers",
                userName,
                start,
                count);
            task.Wait();
            return task.Result;
        }

        public MessageResult SetUsers(string action, List<UserItem> users)
        {
            var task = HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users);
            task.Wait();
            return task.Result;
        }

        // 调用 server 端 Login
        public async void Login(
            string userName,
            string password,
            string libraryUID,
            string libraryName,
            string propertyList)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("Login",
                    userName,
                    password,
                    libraryUID,
                    libraryName,
                    propertyList);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
                AddInfoLine("成功登录。属性为 " + propertyList);
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }
        }

        #endregion
    }

    public class MessageResult
    {
        public string String { get; set; }  // 字符串类型的返回值
        public long Value { get; set; }      // 整数类型的返回值
        public string ErrorInfo { get; set; }   // 出错信息
    }

    public class Record
    {
        // 记录路径。这是本地路径，例如 “图书总库/1”
        public string RecPath { get; set; }
        // 图书馆 UID
        public string LibraryUID { get; set; }
        // 图书馆名
        public string LibraryName { get; set; }

        public string Format { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }
    }

    public class GetUserResult : MessageResult
    {
        public List<UserItem> Users { get; set; }
    }

    public class UserItem
    {
        public string id { get; set; }

        public string userName { get; set; } // 用户名
        public string password { get; set; }  // 密码
        public string rights { get; set; } // 权限
        public string duty { get; set; } // 义务
        public string department { get; set; } // 部门名称
        public string tel { get; set; }  // 电话号码
        public string comment { get; set; }  // 注释
    }

    public class SearchRequest
    {
        public string SearchID { get; set; }    // 本次检索的 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public string Operation { get; set; }   // 操作名。若为 getResult 表示本次不需要进行检索，而是从已有的结果集中获取数据。结果集名在 ResultSetName 中
        public string DbNameList { get; set; }  // 数据库名列表。一般为 "<全部>"
        public string QueryWord { get; set; }   // 检索词。
        public string UseList { get; set; }     // 检索途径列表
        public string MatchStyle { get; set; }  // 匹配方式。为 exact/left/right/middle 之一
        public string ResultSetName { get; set; }   // 检索创建的结果集名。空表示为默认结果集
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多

        public SearchRequest(string searchID,
            string operation,
            string dbNameList,
            string queryWord,
            string useList,
            string matchStyle,
            string resultSetName,
            string formatList,
            long maxResults,
            long start,
            long count)
        {
            this.SearchID = searchID;
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
        }
    }
}
