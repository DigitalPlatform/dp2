using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.VisualStudio.Threading.AsyncSemaphore;
using Microsoft.VisualStudio.Threading;

namespace dp2ManageCenter
{
    // 限制同一台服务器内同时进行的任务数量的 Semaphore 类
    public class ServerTaskLimit : AsyncSemaphore
    {
        public string ServerName { get; set; }

        public ServerTaskLimit(string server_name, int initialCount) : base(initialCount)
        {
            ServerName = server_name;
        }
    }

    public class ServerTaskManager : IDisposable
    {
        List<ServerTaskLimit> _limits = new List<ServerTaskLimit>();

        public int InitialCount { get; set; }

        object _sync_Root = new object();

        public ServerTaskManager(int initialCount)
        {
            InitialCount = initialCount;
        }

        public async Task<Releaser> EnterAsync(string server_name, CancellationToken token)
        {
            ServerTaskLimit limit = null;
            lock (_sync_Root)
            {
                limit = _find(server_name);
                if (limit == null)
                {
                    limit = new ServerTaskLimit(server_name, InitialCount);
                    _limits.Add(limit);
                }
            }

            return await limit.EnterAsync(token);
        }

        ServerTaskLimit _find(string server_name)
        {
            foreach (var limit in _limits)
            {
                if (limit.ServerName == server_name)
                    return limit;
            }

            return null;
        }

        public void Dispose()
        {
            lock (_sync_Root)
            {
                foreach (var limit in _limits)
                {
                    limit.Dispose();
                }

                _limits.Clear();
            }
        }
    }
}
