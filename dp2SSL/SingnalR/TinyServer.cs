using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR.Client;

using DigitalPlatform;
using DigitalPlatform.MessageClient;

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

        static void CloseConnection()
        {
            if (Connection == null)
                return;

            DisposeHandlers();

            Connection.Stop();
            Connection.Dispose();
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

        private static readonly Object _syncRoot = new Object();

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
            
        }

        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        static void OnSearchBiblioRecieved(SearchRequest param)
        {
        }

        #region SetMessage() API

        public static Task<SetMessageResult> SetMessageAsync(
            SetMessageRequest param)
        {
            return HubProxy.Invoke<SetMessageResult>(
 "SetMessage",
 param);
        }

        #endregion

    }
}
