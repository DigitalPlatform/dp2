using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;

namespace SampleMessageClient
{
    public static class DataModel
    {
        static P2PConnection _connection = null;

        public static string messageServerUrl
        {
            get
            {
                return ClientInfo.Config.Get("messageServer", "url", null);
            }
            set
            {
                ClientInfo.Config.Set("messageServer", "url", value);
            }
        }

        public static string messageServerUserName
        {
            get
            {
                return ClientInfo.Config.Get("messageServer", "userName", null);
            }
            set
            {
                ClientInfo.Config.Set("messageServer", "userName", value);
            }
        }

        public static string messageServerPassword
        {
            get
            {
                string password = ClientInfo.Config.Get("messageServer", "password", null);
                return DecryptPasssword(password);
            }
            set
            {
                string password = EncryptPassword(value);
                ClientInfo.Config.Set("messageServer", "password", password);
            }
        }

        static string EncryptKey = "samplemessageclient_key";

        internal static string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        internal static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }

        // 确保连接到消息服务器
        public static async Task<NormalResult> EnsureConnectMessageServerAsync()
        {
            if (string.IsNullOrEmpty(messageServerUrl)
                || string.IsNullOrEmpty(messageServerUserName))
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
                return await _connection.ConnectAsync(messageServerUrl,
    messageServerUserName,
    messageServerPassword,
    "");
            }

            return new NormalResult();
        }

        private static void _connection_AddMessage(object sender, AddMessageEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (e.Action == "create")
                    {
                        foreach (var message in e.Records)
                        {
#if REMOVED
                            // 响应 dp2ssl 发来的 hello 消息
                            // "gn:_62637a12-1965-4876-af3a-fc1d3009af8a"
                            if (message.groups[0] == $"gn:_dp2library_{this.UID}"
                                && message.data == "hello, dp2library")
                            {
                                var message_sender = message.creator;
                                var send_result = await _connection.SetMessageAsyncLite(new SetMessageRequest
                                {
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
#endif
                        }
                    }
                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"_connection_AddMessage() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        public static bool MessageServerConnected
        {
            get
            {
                return _connection != null;
            }
        }

        public static void StartMessageThread(CancellationToken token)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // 确保连接到 dp2mserver
                        {
                            var result = await EnsureConnectMessageServerAsync();
                            if (result.Value == -1 && result.ErrorCode != "notEnabled")
                            {
                                ClientInfo.WriteErrorLog($"尝试连接到 dp2mserver 服务器时出错: {result.ErrorInfo}");
                            }
                        }

                        // 以后的每次延迟
                        await Task.Delay(TimeSpan.FromMinutes(1), token);
                    }
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    ClientInfo.WriteErrorLog($"后台线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
                finally
                {
                    ClientInfo.WriteInfoLog($"后台线程停止");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);

        }
    }
}
