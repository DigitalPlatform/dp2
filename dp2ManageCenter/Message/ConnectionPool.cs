using DigitalPlatform;
using DigitalPlatform.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2ManageCenter.Message
{
    /// <summary>
    /// 管理点对点通讯通道的池
    /// 为每一个独立的账户维持一个唯一的通道
    /// </summary>
    public static class ConnectionPool
    {
        static Hashtable _table = new Hashtable();

        static AsyncSemaphore _limit = new AsyncSemaphore(1);

        public class GetConnectionResult : NormalResult
        {
            public P2PConnection Connection { get; set; }
        }

        // 获得一个连接
        public static async Task<GetConnectionResult> GetConnectiontAsync(string userNameAndUrl)
        {
            using (var releaser = await _limit.EnterAsync())
            {
                P2PConnection connection = _table[userNameAndUrl] as P2PConnection;
                if (connection != null)
                    return new GetConnectionResult
                    {
                        Value = 0,
                        Connection = connection
                    };

                var accounts = MessageAccountForm.GetAccounts();
                var account = FindAccount(accounts, userNameAndUrl);
                if (account == null)
                {
                    return new GetConnectionResult
                    {
                        Value = -1,
                        ErrorInfo = $"用户名 '{userNameAndUrl}' 没有找到"
                    };
                }

                connection = new P2PConnection();
                _table[userNameAndUrl] = connection;

                var result = await connection.ConnectAsync(account.ServerUrl,
                    account.UserName,
                    account.Password,
                    "");
                if (result.Value == -1)
                    return new GetConnectionResult
                    {
                        Value = -1,
                        ErrorInfo = result.ErrorInfo,
                        ErrorCode = result.ErrorCode
                    };
                return new GetConnectionResult
                {
                    Value = 0,
                    Connection = connection
                };
            }
        }

        // 关闭全部通道
        public static async Task CloseAllAsync()
        {
            using (var releaser = await _limit.EnterAsync())
            {
                foreach (string key in _table.Keys)
                {
                    var connection = _table[key] as P2PConnection;
                    connection.CloseConnection();
                }
            }
        }

        static Account FindAccount(List<Account> accounts,
    string userNameAndUrl)
        {
            var parts = StringUtil.ParseTwoPart(userNameAndUrl, "@");
            string userName = parts[0];
            string serverUrl = parts[1];
            var found = accounts.FindAll(o => o.UserName == userName && o.ServerUrl == serverUrl);
            if (found.Count == 0)
                return null;
            return found[0];
        }
    }
}
