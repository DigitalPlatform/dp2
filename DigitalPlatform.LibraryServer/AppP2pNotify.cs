using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.MessageClient;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 和点对点通知有关的功能
    /// 点对点通讯用到了 dp2mserver 服务器
    /// </summary>
    public partial class LibraryApplication
    {
        internal P2PConnection _connection = null;

        internal string _mserverUrl = "";
        internal string _mserverUserName = "";
        internal string _mserverPassword = "";

#if NO
        public class OpenConnectionResult : NormalResult
        {
            public P2PConnection Connection { get; set; }
        }

        // 获得一个连接
        public async Task<OpenConnectionResult> OpenConnectionAsync(
            string url,
            string userName,
            string password)
        {
            // P2PConnection connection = null;
            if (connection != null)
            {
                // 尝试连接一次
                if (connection.IsDisconnected == true)
                {
                    await EnsureConnectMessageServerAsync(
connection,
userNameAndUrl);
                }
                return new OpenConnectionResult
                {
                    Value = 0,
                    Connection = connection
                };
            }

            if (_connection == null)
                _connection = new P2PConnection();
            else
            {
                _connection.CloseConnection();
                _connection = new P2PConnection();
            }

            var result = await _connection.ConnectAsync(url,
                userName,
                password,
                "");
            if (result.Value == -1)
                return new OpenConnectionResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            return new OpenConnectionResult
            {
                Value = 0,
                Connection = _connection
            };
        }
#endif

        // 确保连接到消息服务器
        public async Task<NormalResult> EnsureConnectMessageServerAsync()
        {
            if (string.IsNullOrEmpty(_mserverUrl)
                || string.IsNullOrEmpty(_mserverUserName))
            {
                if (_connection != null)
                {
                    _connection.CloseConnection();
                    _connection.AddMessage -= _connection_AddMessage;
                    _connection = null;
                }

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "点对点消息功能尚未启用",
                    ErrorCode = "notEnabled"
                };
            }

            if (_connection == null)
            {
                _connection = new P2PConnection();
                _connection.AddMessage += _connection_AddMessage;
            }

            if (_connection.IsDisconnected)
            {
                return await _connection.ConnectAsync(_mserverUrl,
    _mserverUserName,
    _mserverPassword,
    "");
            }

            return new NormalResult();
        }

        private void _connection_AddMessage(object sender, AddMessageEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (e.Action == "create")
                    {
                        foreach (var message in e.Records)
                        {
                            // 响应 dp2ssl 发来的 hello 消息
                            // "gn:_62637a12-1965-4876-af3a-fc1d3009af8a"
                            if (message.groups[0] == $"gn:_dp2library_{this.UID}"
                                && message.data == "hello, dp2library")
                            {
                                var message_sender = message.creator;
                                var send_result = await _connection.SetMessageAsyncLite(new SetMessageRequest {
                                    Action = "create",
                                    Records = new List<MessageRecord> {
                                        new MessageRecord{ 
                                            groups = new string [] { message.groups[0] },
                                            subjects = new string [] { "hello" },
                                            data = $"hello, {message_sender}",
                                            expireTime = DateTime.Now + TimeSpan.FromMinutes(5) // 5 分钟以后消息自动失效
                                        }
                                    },
                                    Style = "dontNotifyMe",
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog($"_connection_AddMessage() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        public bool MessageServerConnected
        {
            get
            {
                return _connection != null;
            }
        }

        public Task<SetMessageResult> SendMessageAsync(string[] groups,
            string subject,
            string content)
        {
            if (_connection == null)
                return Task.FromResult<SetMessageResult>(new SetMessageResult());

            SetMessageRequest request = new SetMessageRequest("create",
                "dontNotifyMe",
                new List<MessageRecord> {
                        new MessageRecord {
                            groups= groups,
                            subjects = new string [] { subject },
                            data = content,
                            expireTime = DateTime.Now + TimeSpan.FromMinutes(5) // 5 分钟以后消息自动失效
                        }
                });
            return _connection.SetMessageAsyncLite(request);
        }
    }
}
