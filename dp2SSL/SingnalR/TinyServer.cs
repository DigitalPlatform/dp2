using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq.Expressions;

using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client;
using Z.Expressions;

using DigitalPlatform;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Threading;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    public static class TinyServer
    {
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

            Connection.Stop();
            Connection.Dispose();
            Connection = null;

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

                /*
                // *** webCall
                {
                    var handler = HubProxy.On<WebCallRequest>("webCall",
                    (param) => OnWebCallRecieved(param)
                    );
                    _handlers.Add(handler);
                }

                // *** close
                {
                    var handler = HubProxy.On<CloseRequest>("close",
                    (param) => OnCloseRecieved(param)
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

        // 收到消息。被当作命令解释。执行后发回命令执行结果
        static void OnAddMessageRecieved(string action,
IList<MessageRecord> messages)
        {
            foreach (var message in messages)
            {
                // TODO: 忽略自己发出的消息?
                if (message.data.StartsWith($"@{_userName}"))
                {
                    string command = message.data.Substring($"@{_userName}".Length).Trim();
                    ProcessCommand(command);
                }
            }
        }

        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        static void OnSearchBiblioRecieved(SearchRequest param)
        {
        }

        static string GroupName
        {
            get
            {
                return App.messageGroupName;
            }
        }

        static void ProcessCommand(string command)
        {
            if (command.StartsWith("hello"))
            {
                SetMessageAsync("hello!");
                return;
            }

            if (command.StartsWith("version"))
            {
                SetMessageAsync($"dp2SSL 前端版本: {WpfClientInfo.ClientVersion}");
                return;
            }

            if (command.StartsWith("error"))
            {
                SetMessageAsync($"dp2SSL 当前界面报错: [{App.CurrentApp.Error}]; 书柜初始化是否完成: {ShelfData.FirstInitialized}");
                return;
            }

            // 列出操作历史
            if (command.StartsWith("list history"))
            {
                ListHistory(command);
                return;
            }

            // 检查册状态
            if (command.StartsWith("check"))
            {
                CheckBook(command);
                return;
            }

            SetMessageAsync($"我无法理解这个命令 '{command}'");
        }

        // 列出操作历史
        // 子参数:
        //      not sync/!sync/new 没有同步的那些事项
        //      sync 已经同步的那些shixiang
        //      error 同步出错的事项
        //      空 所有事项
        static void ListHistory(string command)
        {
            try
            {
                // 子参数
                string param = command.Substring("list history".Length).Trim();
                // "not sync" 表示只列出那些没有成功同步的操作

                using (var context = new MyContext())
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

                    SetMessageAsync($"> {command}\r\n当前共有 {items.Count} 个历史事项");
                    int i = 1;
                    foreach (var item in items)
                    {
                        SetMessageAsync($"{i++}\r\n{DisplayRequestItem.GetDisplayString(item)}");
                    }
                }
            }
            catch(Exception ex)
            {
                SetMessageAsync($"命令 {command} 执行过程出现异常:\r\n{ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // 检查册状态
        static void CheckBook(string command)
        {
            // 子参数
            string param = command.Substring("check".Length).Trim();

            // 目前子参数为 PII
            param = param.ToUpper();

            // TODO: 用一段文字描述这一册的总体状态。特别是是否同步成功，本地库最新状态和 dp2library 一端是否吻合

            using (var context = new MyContext())
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
                var result = LibraryChannelUtil.GetEntityData(param);
                if (result.Value == -1 || result.Value == 0)
                    text.AppendLine($"尝试获得册记录时出错: {result.ErrorInfo}");
                else
                    text.AppendLine($"册记录:\r\n{DomUtil.GetIndentXml(result.ItemXml)}");

                SetMessageAsync(text.ToString());
            }
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
        class DisplayRequestItem
        {
            public int ID { get; set; }

            public string PII { get; set; } // PII 单独从 EntityString 中抽取出来，便于进行搜索

            public string Action { get; set; }  // borrow/return/transfer

            public DateTime OperTime { get; set; }  // 操作时间
            public string State { get; set; }   // 状态。sync/commerror/normalerror/空
                                                // 表示是否完成同步，还是正在出错重试同步阶段，还是从未同步过
            public string SyncErrorInfo { get; set; }   // 最近一次同步操作的报错信息
            public int SyncCount { get; set; }

            public Operator Operator { get; set; }  // 提起请求的读者

            public Entity Entity { get; set; }

            public string TransferDirection { get; set; } // in/out 典藏移交的方向
            public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
            public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
            public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成

            public static string GetDisplayString(RequestItem item)
            {
                DisplayRequestItem result = new DisplayRequestItem
                {
                    ID = item.ID,
                    PII = item.PII,
                    Action = item.Action,
                    OperTime = item.OperTime,
                    State = item.State,
                    SyncErrorInfo = item.SyncErrorInfo,
                    SyncCount = item.SyncCount,
                    Operator = JsonConvert.DeserializeObject<Operator>(item.OperatorString),
                    Entity = JsonConvert.DeserializeObject<Entity>(item.EntityString),
                    TransferDirection = item.TransferDirection,
                    Location = item.Location,
                    CurrentShelfNo = item.CurrentShelfNo,
                    BatchNo = item.BatchNo,
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
        }

        #endregion

        #region SetMessage() API

        public static Task<SetMessageResult> SetMessageAsync(string content)
        {
            SetMessageRequest request = new SetMessageRequest("create", "",
                new List<MessageRecord> {
                        new MessageRecord {
                            groups= new string[] { GroupName},
                            data = content}
                });
            return SetMessageAsync(request);
        }

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
            Task.Run(() => SearchAndResponse(param));
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

            SearchHistory(searchParam);
            return;
        }

        // https://eval-expression.net/linq-dynamic
        static string BuildQuery(SearchRequest request)
        {
            if (string.IsNullOrEmpty(request.QueryWord))
                return "true";

            StringBuilder text = new StringBuilder();
            List<string> uses = StringUtil.SplitList(request.UseList);
            List<string> where_list = new List<string>();
            foreach (var use in uses)
            {
                //string s = "";
                //s.EndsWith()
                if (request.MatchStyle == "left")
                    where_list.Add($" x.{use}.StartsWith(\"{request.QueryWord}\") ");
                else if (request.MatchStyle == "right")
                    where_list.Add($" x.{use}.EndsWith(\"{request.QueryWord}\") ");
                else if (request.MatchStyle == "exact")
                    where_list.Add($" x.{use} == \"{request.QueryWord}\" ");
                else // if (request.MatchStyle == "middle")
                    where_list.Add($" x.{use}.IndexOf(\"{request.QueryWord}\") != -1 ");
            }

            text.Append(StringUtil.MakePathList(where_list, "||"));
            return text.ToString();
        }

        // TODO: 结果集用 ID 数组表示? 早期可只支持 default 这一个结果集
        static async Task SearchHistory(SearchRequest searchParam)
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

                if (searchParam.QueryWord == "!getResult")
                {
                    // lRet = -1;
                }
                else
                {
                    // TODO: .Operation == "searchBiblio"
                    using (var context = new MyContext())
                    {
                        // https://stackoverflow.com/questions/37078256/entity-framework-building-where-clause-on-the-fly-using-expression
                        string query = BuildQuery(searchParam);

                        var query_result = context.Requests.WhereDynamic(x => query)
    .OrderBy(o => o.ID);
                        int result_count = query_result.Count();

                        if (result_count == 0)
                        {
                            // 没有命中
                            TryResponseSearch(
                                new SearchResponse(
    searchParam.TaskID,
    0,
    0,
    "", // this.dp2library.LibraryUID,
    records,
    "没有命中",  // 出错信息大概为 not found。
    "NotFound"));
                            return;
                        }

                        if (searchParam.Count == 0)
                        {
                            // 返回命中数
                            TryResponseSearch(
                                new SearchResponse(
                                searchParam.TaskID,
                                result_count,
    0,
    "", // this.dp2library.LibraryUID,
    records,
    "本次没有返回任何记录",
    strErrorCode));
                            return;
                        }

                        var items = query_result.Skip((int)searchParam.Start)
    .Take(count)
    .ToList();
                        /*
                        var items = context.Requests.WhereDynamic(x => query)
                            .OrderBy(o => o.ID)
                            .Skip((int)searchParam.Start)
                            .Take(count)
                            .ToList();
                            */

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
                        var result = await TryResponseSearch(
    searchParam.TaskID,
    result_count,
    searchParam.Start,
    "", // libraryUID,
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
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

        ERROR1:
            // 报错
            TryResponseSearch(
                new SearchResponse(
searchParam.TaskID,
-1,
0,
"", // this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        // 调用 server 端 ResponseSearchBiblio
        static async void TryResponseSearch(
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
        static async Task<ResponseSearchResult> TryResponseSearch(
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
            TryResponseSearch(
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
    }
}
