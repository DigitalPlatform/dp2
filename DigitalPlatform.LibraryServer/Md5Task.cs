using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    public class Md5TaskCollection : IDisposable
    {
        List<Md5Task> _md5Tasks = new List<Md5Task>();
        readonly object _syncRoot_md5Tasks = new object();

        public Md5Task FindMd5Task(string taskID)
        {
            lock (_syncRoot_md5Tasks)
            {
                foreach (var task in _md5Tasks)
                {
                    if (task.TaskID == taskID)
                        return task;
                }
                return null;
            }
        }

        public bool RemoveMd5Task(string taskID)
        {
            var task = FindMd5Task(taskID);
            if (task == null)
                return false;
            lock (_syncRoot_md5Tasks)
            {
                _md5Tasks.Remove(task);
                task.Cancel();
                task.Dispose();
            }
            return true;
        }

        // parameters:
        //      taskID  任务 ID。如果为空，表示服务器自动发生 task id。否则会采用这个 task id
        public string StartMd5Task(string strFilePath,
            string taskID = null)
        {
            // 做一下清理
            RemoveIdleMd5Task();

            if (string.IsNullOrEmpty(taskID))
                taskID = Guid.NewGuid().ToString();
            else
            {
                // 对 taskID 进行查重
                var dup_task = FindMd5Task(taskID);
                // 发现已经存在的 task, 先移走
                if (dup_task != null)
                    RemoveMd5Task(dup_task.TaskID);
            }

            Md5Task task = new Md5Task
            {
                FilePath = strFilePath,
                TaskID = taskID,
                StartTime = DateTime.Now,
                FinishTime = DateTime.MinValue  // 表示尚未结束
            };

            lock (_syncRoot_md5Tasks)
            {
                if (_md5Tasks.Count > 100)
                    throw new Exception($"当前正在运行的 MD5 任务数超过极限，创建新任务失败。请稍后重试");
                _md5Tasks.Add(task);
            }

            task.Task = Task.Run( /*async*/ () =>
            {
                try
                {
                    task.Begin();
                    var outputTimestamp = Md5Task.GetFileMd5(strFilePath, task.Token);

                    // testing
                    // await Task.Delay(TimeSpan.FromSeconds(10));

                    task.FinishTime = DateTime.Now; // 2022/1/7
                    task.Result = new NormalResult
                    {
                        Value = 0,
                        ErrorCode = ByteArray.GetHexTimeStampString(outputTimestamp)
                    };
                }
                catch (Exception ex)
                {
                    task.Result = new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ex.Message,
                        ErrorCode = "exception:" + ex.GetType().ToString()
                    };
                }
            });

            return task.TaskID;
        }

        // 清除长期没有 remove 的已经完成的任务?
        void RemoveIdleMd5Task()
        {
            TimeSpan period = TimeSpan.FromHours(1);
            lock (_syncRoot_md5Tasks)
            {
                List<Md5Task> tasks = new List<Md5Task>();
                foreach (var task in _md5Tasks)
                {
                    if (task.Result != null
                        && task.FinishTime > DateTime.MinValue // 2022/1/7
                        && task.FinishTime >= task.StartTime
                        && DateTime.Now - task.FinishTime > period)
                        tasks.Add(task);
                    // 或者开始运行超过一天的
                    else if (DateTime.Now - task.StartTime > TimeSpan.FromDays(1))
                        tasks.Add(task);
                }

                foreach (var task in tasks)
                {
                    _md5Tasks.Remove(task);
                }
            }
        }

        public bool StopMd5Task(string taskID)
        {
            var task = FindMd5Task(taskID);
            if (task == null)
                return false;
            task.Cancel();
            return true;
        }

        public void Clear()
        {
            lock (_syncRoot_md5Tasks)
            {
                foreach (var task in _md5Tasks)
                {
                    task.Dispose();
                }

                _md5Tasks.Clear();
            }
        }

        public void Dispose()
        {
            this.Clear();
        }
    }

    // 一个获得 MD5 的任务
    public class Md5Task : IDisposable
    {
        public string FilePath { get; set; }

        public Task Task { get; set; }
        public string TaskID { get; set; }

        CancellationTokenSource _cancel = null;

        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }

        public NormalResult Result { get; set; }    // 任务结束返回的结果。其中 ErrorInfo 为任务结束时返回的 MD5 字符串(当 Value == 0)

        public void Dispose()
        {
            Cancel();
            if (Task != null)
                Task.Dispose();
        }

        public void Cancel()
        {
            if (_cancel != null)
                _cancel.Cancel();
        }

        public CancellationToken Token
        {
            get
            {
                if (_cancel == null)
                    return new CancellationToken();
                return _cancel.Token;
            }
        }

        public void Begin()
        {
            Cancel();   // 停止前一个任务
            _cancel = new CancellationTokenSource();
        }

        public static byte[] GetFileMd5(string filename,
            CancellationToken token)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(
                        filename,
                        FileMode.Open,
                        FileAccess.ReadWrite, // Read会造成无法打开
                        FileShare.ReadWrite))
                {
                    using (CancellationTokenRegistration ctr = token.Register(() =>
                    {
                        stream.Close();
                    }))
                    {
                        return md5.ComputeHash(stream);
                    }
                }
            }
        }
    }
}
