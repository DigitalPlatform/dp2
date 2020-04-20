using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR.Client;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;

namespace dp2ManageCenter.Message
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

                /*
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
                */

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
