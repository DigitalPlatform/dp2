using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
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
            }

            if (command.StartsWith("list history"))
            {
                ListHistory(command);
            }
        }

        static void ListHistory(string command)
        {
            using (var context = new MyContext())
            {
                var items = context.Requests.Where(o => true).ToList();
                SetMessageAsync($"> {command}\r\n当前共有 {items.Count} 个历史事项");
                int i = 1;
                foreach (var item in items)
                {
                    SetMessageAsync($"{i++}\r\n{GetDisplayString(item)}");
                }
            }
        }

        public class DisplayRequestItem
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
        }

        static string GetDisplayString(RequestItem item)
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

    }
}
