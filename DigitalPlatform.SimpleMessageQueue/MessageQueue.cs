using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

namespace DigitalPlatform.SimpleMessageQueue
{
    public class MessageQueue
    {
        string _databaseFileName = null;

        AsyncSemaphore _databaseLimit = new AsyncSemaphore(1);

        int _chunkSize = 4096;
        public int ChunkSize
        {
            get
            {
                return _chunkSize;
            }
            set
            {
                _chunkSize = value;
            }
        }

        public MessageQueue(string databaseFileName,
            bool ensureCreated = true)
        {
            _databaseFileName = databaseFileName;
            if (ensureCreated)
                using (var context = new QueueContext(_databaseFileName))
                {
                    context.Database.EnsureCreated();
                }
        }

        public async Task EnsureCreatedAsync()
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new QueueContext(_databaseFileName))
                {
                    context.Database.EnsureCreated();
                }
            }
        }

        public async Task PushAsync(List<string> texts,
            CancellationToken token = default)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new QueueContext(_databaseFileName))
                {
                    foreach (string text in texts)
                    {
                        context.Items.AddRange(BuildItem(text));
                    }
                    await context.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        List<QueueItem> BuildItem(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            return BuildItem(buffer);
        }

        List<QueueItem> BuildItem(byte[] buffer)
        {
            DateTime now = DateTime.Now;
            List<QueueItem> results = new List<QueueItem>();
            int start = 0;
            while (start < buffer.Length)
            {
                int length = Math.Min(buffer.Length - start, this.ChunkSize);
                byte[] fragment = new byte[length];
                Array.Copy(buffer, start, fragment, 0, length);
                QueueItem item = new QueueItem
                {
                    Content = fragment,
                    CreateTime = now
                };
                results.Add(item);

                start += length;
            }

            if (results.Count > 0)
            {
                string group_id = Guid.NewGuid().ToString();
                foreach (var item in results)
                {
                    item.GroupID = group_id;
                }
            }

            return results;
        }

        public async Task PushAsync(List<byte[]> contents,
            CancellationToken token = default)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new QueueContext(_databaseFileName))
                {
                    foreach (var content in contents)
                    {
                        // 注意，这里每次 Add() 以后都要及时 SaveChanges()。否则 ID 的顺序会发生混乱
                        var items = BuildItem(content);
                        foreach (var item in items)
                        {
                            context.Items.Add(item);
                            await context.SaveChangesAsync(token);
                        }
                    }
                }
            }
        }

        public async Task<Message> PullAsync(CancellationToken token = default)
        {
            return await GetAsync(true, token);
        }

        public async Task<Message> GetAsync(bool remove_items,
            CancellationToken token = default)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new QueueContext(_databaseFileName))
                {
                    List<QueueItem> items = new List<QueueItem>();

                    var first = context.Items.OrderBy(o => o.ID).FirstOrDefault();
                    if (first == null)
                        return null;

                    items.Add(first);
                    if (string.IsNullOrEmpty(first.GroupID))
                        return new Message { Content = first.Content };
                    // 取出所有 GroupID 相同的事项，然后拼接起来
                    var group_id = first.GroupID;
                    List<byte> bytes = new List<byte>(first.Content);

                    int id = first.ID;
                    while (true)
                    {
                        var current = context.Items.Where(o => o.ID > id).OrderBy(o => o.ID).FirstOrDefault();
                        if (current == null)
                            break;
                        if (current.GroupID != group_id)
                            break;
                        bytes.AddRange(current.Content);
                        id = current.ID;

                        items.Add(current);
                    }

                    // 删除涉及到的事项
                    if (remove_items)
                    {
                        context.Items.RemoveRange(items);
                        await context.SaveChangesAsync(token);
                    }

                    return new Message { Content = bytes.ToArray() };
                }
            }
        }

        public async Task<Message> PeekAsync(CancellationToken token = default)
        {
            return await GetAsync(false, token);
        }
    }

    public class Message
    {
        public byte[] Content { get; set; }
        public DateTime CreateTime { get; set; }

        public string GetString()
        {
            if (Content == null)
                return null;
            return Encoding.UTF8.GetString(Content);
        }
    }
}
