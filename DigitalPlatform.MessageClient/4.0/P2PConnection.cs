using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNet.SignalR.Client;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// 点对点消息通道
    /// </summary>
    public class P2PConnection
    {
        IHubProxy HubProxy
        {
            get;
            set;
        }

        HubConnection Connection
        {
            get;
            set;
        }

        public void CloseConnection()
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

        public bool IsDisconnected
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

        public async Task<NormalResult> ConnectAsync(string url,
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
                                    catch
                                    {

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
                // *** search
                {
                    var handler = HubProxy.On<SearchRequest>("search",
                    (param) => OnSearchRecieved(param)
                    );
                    _handlers.Add(handler);
                }
                */

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

        public delegate void Delegate_outputMessage(
    StringBuilder cache,
    long totalCount,
    long start,
    IList<MessageRecord> records,
    string errorInfo,
    string errorCode);

        public async Task<MessageResult> GetMessageAsyncLite(
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

        // 对于 .data 超过 chunk_size 的情况可以自动切割为多次发送请求
        public async Task<SetMessageResult> SetMessageAsyncLite(
SetMessageRequest request)
        {
            // 请求结构中如果具备了 TaskID 值，说明调主想自己控制拼接过程，那这里就直接发送出去
            if (string.IsNullOrEmpty(request.TaskID) == false)
                return await TrySetMessageAsync(request).ConfigureAwait(false);

            int chunk_size = 4096;
            int length = GetLength(request);
            if (length < chunk_size)
                return await TrySetMessageAsync(request);

            SetMessageResult result = null;
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

                    // TODO: 
#if NO
                    result = await HubProxy.Invoke<SetMessageResult>(
"SetMessage",
current_request);
#endif
                    result = await TrySetMessageAsync(current_request).ConfigureAwait(false);
                    if (result.Value == -1)
                        return result;  // 中途出错了

                    send += current_record.data.Length;
                    if (send >= data.Length)
                        break;
                }
            }

            return result;  // 返回最后一次请求的 result 值
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

        public async Task<SetMessageResult> TrySetMessageAsync(SetMessageRequest request)
        {
            int max = 5;
            for (int i = 0; ; i++)
            {
                try
                {
                    return await HubProxy.Invoke<SetMessageResult>(
"SetMessage",
request).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    if (i < max && ex.InnerException is InvalidOperationException)
                    {
                        Console.WriteLine("*** TrySetMessageAsync InvalidOperationException, retryCount=" + i.ToString());
                        Thread.Sleep(1000);
                        continue;
                    }
                    else
                        throw;
                }
            }
        }


        // 新版 API，测试中
        public async Task<SearchResult> SearchAsyncLite(
    string strRemoteUserName,
    SearchRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            ResultManager manager = new ResultManager();
            List<string> errors = new List<string>();
            List<string> codes = new List<string>();

            SearchResult result = new SearchResult();
            if (result.Records == null)
                result.Records = new List<Record>();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<SearchResponse>(
                    "responseSearch",
                    (responseParam) =>
                    {
                        try
                        {
                            if (responseParam.TaskID != request.TaskID)
                                return;

                            // Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                            // 装载命中结果
                            if (responseParam.ResultCount == -1 && responseParam.Start == -1)
                            {
                                if (result.ResultCount != -1)
                                    result.ResultCount = manager.GetTotalCount();
                                //result.ErrorInfo = responseParam.ErrorInfo;
                                //result.ErrorCode = responseParam.ErrorCode;
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                Debug.WriteLine("finish_event.Set() 1");
                                wait_events.finish_event.Set();
                                return;
                            }

                            // TODO: 似乎应该关注 start 位置
                            if (responseParam.Records != null)
                                AddLibraryUID(responseParam.Records, responseParam.LibraryUID);

                            result.Records.AddRange(responseParam.Records);
                            if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                && errors.IndexOf(responseParam.ErrorInfo) == -1)
                            {
                                errors.Add(responseParam.ErrorInfo);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            }
                            if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                && codes.IndexOf(responseParam.ErrorCode) == -1)
                            {
                                codes.Add(responseParam.ErrorCode);
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");
                            }

                            // 标记结束一个检索目标
                            // return:
                            //      0   尚未结束
                            //      1   结束
                            //      2   全部结束
                            int nRet = manager.CompleteTarget(responseParam.LibraryUID,
                                responseParam.ResultCount,
                                responseParam.Records == null ? 0 : responseParam.Records.Count);

                            if (responseParam.ResultCount == -1)
                                result.ResultCount = -1;
                            else
                                result.ResultCount = manager.GetTotalCount();

#if NO
                                            if (nRet == 2)
                                            {
                                                Debug.WriteLine("finish_event.Set() 2");
                                                wait_events.finish_event.Set();
                                            }
                                            else
                                                wait_events.active_event.Set();
#endif
                            wait_events.active_event.Set();

                        }
                        catch (Exception ex)
                        {
                            errors.Add("SearchAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                            result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            if (!(ex is ObjectDisposedException))
                                wait_events.finish_event.Set();
                        }
                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestSearch",
        strRemoteUserName,
        request).ConfigureAwait(false);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.ResultCount = -1;
                        result.ErrorCode = message.String;
                        Debug.WriteLine("return pos 1");
                        return result;
                    }

                    if (manager.SetTargetCount(message.Value) == true)
                    {
                        Debug.WriteLine("return pos 2");
                        return result;
                    }

                    try
                    {
                        // TODO: 当检索一个目标的时候，不应该长时间等待其他目标
                        await WaitAsync(
        request.TaskID,
        wait_events,
        timeout,
        token).ConfigureAwait(false);
                    }
                    catch (TimeoutException)
                    {
                        // 超时的时候实际上有结果了
                        if (result.Records != null
                            && result.Records.Count > 0)
                        {
                            result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                            Debug.WriteLine("return pos 3");
                            return result;
                        }
                        throw;
                    }

                    Debug.WriteLine("return pos 4");
                    return result;
                }
            }
        }

        static void AddLibraryUID(IList<Record> records, string libraryUID)
        {
            if (records == null)
                return;
            if (string.IsNullOrEmpty(libraryUID) || libraryUID == "|")
                return;

            foreach (Record record in records)
            {
                record.RecPath += "@" + libraryUID;
            }
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


        Task WaitAsync(string taskID,
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

        public static Task TaskRunAction(Action function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(
                function,
                cancellationToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        // 发起一次书目检索
        // 这是比较原始的 API，并不负责接收对方传来的消息
        // result.Value:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索。此时 Result.String 里面返回了 taskID
        public Task<MessageResult> SearchAsync(
            string userNameList,
            SearchRequest searchParam)
        {
            return HubProxy.Invoke<MessageResult>(
                "RequestSearch",
                userNameList,
                searchParam);
        }

        // 请求服务器中断一个 task
        public Task<MessageResult> CancelSearchAsync(string taskID)
        {
            return HubProxy.Invoke<MessageResult>(
                "CancelSearch",
                taskID);
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
        public virtual void TriggerAddMessage(P2PConnection connection,
            AddMessageEventArgs e)
        {
            AddMessageEventHandler handler = this.AddMessage;
            if (handler != null)
            {
                handler(connection, e);
            }
        }

        #region SetInfo() API

        public class SetInfoResult : MessageResult
        {
            public List<Entity> Entities { get; set; }
        }

        public static Task<TResult> TaskRun<TResult>(
    Func<TResult> function,
    CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew<TResult>(
                function,
                cancellationToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public Task<SetInfoResult> SetInfoTaskAsync(
string strRemoteUserName,
SetInfoRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<SetInfoResult>(() =>
            {
                return SetInfoAsyncLite(strRemoteUserName, request, timeout, token).Result;
            }, token);
        }

        public async Task<SetInfoResult> SetInfoAsyncLite(
    string strRemoteUserName,
    SetInfoRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            SetInfoResult result = new SetInfoResult();
            if (result.Entities == null)
                result.Entities = new List<Entity>();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())
            {
                using (var handler = HubProxy.On<
                    string, long, IList<Entity>, string>(
                    "responseSetInfo",
                    (taskID, resultValue, entities, errorInfo) =>
                    {
                        if (taskID != request.TaskID)
                            return;

                        // 装载命中结果
                        if (entities != null)
                            result.Entities.AddRange(entities);
                        result.Value = resultValue;
                        result.ErrorInfo = errorInfo;
                        wait_events.finish_event.Set();
                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestSetInfo",
        strRemoteUserName,
        request).ConfigureAwait(false);
                    if (message.Value == -1
                        || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.Value = -1;
                        result.String = message.String;
                        return result;
                    }

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


        #region GetConnectionInfo() API

#if REMOVED
        public class GetConnectionInfoResult
        {
            public long ResultCount = 0;
            public List<ConnectionRecord> Records = null;
            public string ErrorInfo = "";
            public string ErrorCode = "";
        }
#endif
        public async Task<GetConnectionInfoResult> GetConnectionInfoAsync(
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


        #region GetRes() API

        public delegate void Delegate_setProgress(long totalLength, long current);

        // 2021/8/21
        // 写入流版本
        // 返回结果中 result.Data 不会使用，为 null
        public async Task<GetResResponse> GetResAsyncLite(
            string strRemoteUserName,
            GetResRequest request,
            Stream stream,
            Delegate_setProgress func_setProgress,
            TimeSpan timeout,
            CancellationToken token)
        {
            long lTail = -1;    // -1 表示尚未使用
            long count = 0;
            List<string> errors = new List<string>();
            List<string> codes = new List<string>();

            GetResResponse result = new GetResResponse();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<GetResResponse>(
                    "responseGetRes",
                    (responseParam) =>
                    {
                        try
                        {
                            if (responseParam.TaskID != request.TaskID)
                                return;

                            Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                            // 装载命中结果
                            if (responseParam.TotalLength == -1 && responseParam.Start == -1)
                            {
                                if (func_setProgress != null && result.TotalLength >= 0)
                                    func_setProgress(result.TotalLength, lTail);

                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                Debug.WriteLine("finish_event.Set() 1");
                                wait_events.finish_event.Set();
                                return;
                            }

                            result.TotalLength = responseParam.TotalLength;
                            if (string.IsNullOrEmpty(responseParam.Metadata) == false)
                                result.Metadata = responseParam.Metadata;
                            if (string.IsNullOrEmpty(responseParam.Timestamp) == false)
                                result.Timestamp = responseParam.Timestamp;
                            if (string.IsNullOrEmpty(responseParam.Path) == false)
                                result.Path = responseParam.Path;

                            // TODO: 检查一下和上次的最后位置是否连续
                            if (lTail != -1 && responseParam.Start != lTail)
                            {
                                errors.Add("GetResAsync 接收数据过程出现不连续的批次 lTail=" + lTail + " param.Start=" + responseParam.Start);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.TotalLength = -1;
                                // 向服务器发送 CancelSearch 请求
                                _ = CancelSearchAsync(responseParam.TaskID);
                                wait_events.finish_event.Set();
                                return;
                            }

                            if (responseParam.Data != null
                                && responseParam.Data.Length > 0
                                && stream != null)
                            {
                                stream.Write(responseParam.Data,
                                    0,
                                    responseParam.Data.Length);
                                lTail = responseParam.Start + responseParam.Data.Length;
                            }

                            if (func_setProgress != null && result.TotalLength >= 0 && (count++ % 10) == 0)
                                func_setProgress(result.TotalLength, lTail);

                            if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                && errors.IndexOf(responseParam.ErrorInfo) == -1)
                            {
                                errors.Add(responseParam.ErrorInfo);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            }
                            if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                && codes.IndexOf(responseParam.ErrorCode) == -1)
                            {
                                codes.Add(responseParam.ErrorCode);
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");
                            }

                            Debug.WriteLine("active_event activate");
                            wait_events.active_event.Set();
                        }
                        catch (Exception ex)
                        {
                            errors.Add("GetResAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                            result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            if (!(ex is ObjectDisposedException))
                                wait_events.finish_event.Set();
                            // 向服务器发送 CancelSearch 请求
                            _ = CancelSearchAsync(responseParam.TaskID);
                        }

                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestGetRes",
        strRemoteUserName,
        request).ConfigureAwait(false);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.TotalLength = -1;
                        result.ErrorCode = message.String;
                        Debug.WriteLine("return pos 1");
                        return result;
                    }

                    try
                    {
                        await WaitAsync(
        request.TaskID,
        wait_events,
        timeout,
        token).ConfigureAwait(false);
                    }
                    catch (TimeoutException)
                    {
                        // 超时的时候实际上有结果了
                        if (lTail != -1)
                        {
                            result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                            return result;
                        }
                        throw;
                    }

                    return result;
                }
            }
        }

        #endregion
    }

    public class SearchResult
    {
        public long ResultCount = 0;
        public List<Record> Records = null;
        public string ErrorInfo = "";
        public string ErrorCode = "";   // 2016/4/15 增加
        // public bool Finished = false;
    }

}
