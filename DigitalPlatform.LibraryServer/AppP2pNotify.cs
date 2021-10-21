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
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "点对点消息功能尚未启用",
                    ErrorCode = "notEnabled"
                };

            if (_connection == null)
                _connection = new P2PConnection();

            if (_connection.IsDisconnected)
            {
                return await _connection.ConnectAsync(_mserverPassword,
    _mserverUserName,
    _mserverPassword,
    "");
            }

            return new NormalResult();
        }
    }
}
