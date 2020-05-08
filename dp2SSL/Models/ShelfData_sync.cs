﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Collections;

using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;

using dp2SSL.Models;

using DigitalPlatform;
using DigitalPlatform.WPF;
using DigitalPlatform.Text;

namespace dp2SSL
{
    public static partial class ShelfData
    {
        static Task _syncTask = null;

        /*
        static List<ActionInfo> _retryActions = new List<ActionInfo>();
        static object _syncRoot_retryActions = new object();
        */

        static AutoResetEvent _eventSync = new AutoResetEvent(false);

        public static void ActivateRetry()
        {
            _eventSync.Set();
        }

        /*
        // 从 _retryActions 中找到匹配的元素加以删除
        public static void RemoveFromRetryActions(List<Entity> entities)
        {
            lock (_syncRoot_actions)
            {
                List<ActionInfo> matched = new List<ActionInfo>();
                foreach (var action in _retryActions)
                {
                    string pii = action.Entity.PII;
                    var found = entities.Find((a) =>
                    {
                        if (a.PII == pii)
                            return true;
                        return false;
                    });
                    if (found != null)
                        matched.Add(action);
                }

                foreach (var action in matched)
                {
                    _retryActions.Remove(action);
                }
                RefreshRetryInfo();
            }
        }
        */

        public static void RefreshRetryInfo(List<ActionInfo> actions)
        {
            PageMenu.PageShelf?.SetRetryInfo(actions.Count == 0 ? "" : $"滞留:{actions.Count}");
        }

        public static bool PauseSubmit { get; set; }

        // 同步重试间隔时间
        static TimeSpan _syncIdleLength = TimeSpan.FromSeconds(10);

        // 启动同步任务。此任务长期在后台运行
        public static void StartSyncTask()
        {
            if (_syncTask != null)
                return;

            CancellationToken token = _cancel.Token;

            token.Register(() =>
            {
                _eventSync.Set();
            });

            // 启动重试专用线程
            _syncTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("重试专用线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // TODO: 无论是整体退出，还是需要激活，都需要能中断 Delay
                        // Task.Delay(TimeSpan.FromSeconds(10)).Wait(token);
                        _eventSync.WaitOne(_syncIdleLength);
                        token.ThrowIfCancellationRequested();

#if REMOVED
                        // 顺便检查和确保连接到消息服务器
                        App.CurrentApp.EnsureConnectMessageServer().Wait(token);
#endif

#if REMOVED
                        // 顺便关闭天线射频
                        if (_tagAdded)
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await SelectAntennaAsync();
                                }
                                catch
                                {
                                    // TODO: 写入错误日志
                                }
                            });
                            _tagAdded = false;
                        }
#endif

                        if (PauseSubmit)
                            continue;

                        // TODO: 从本地数据库中装载需要同步的那些 Actions
                        List<ActionInfo> actions = await LoadRetryActionsFromDatabaseAsync();
                        if (actions.Count == 0)
                            continue;

                        // RefreshRetryInfo() ???

                        // 一般来说，只要 SubmitWindow 开着，就要显示请求情况结果。
                        // 特殊地，如果 SubmitWindow 没有开着，但本次至少有一个成功的请求结果了，那就专门打开 SubmitWindow 显示信息

                        int succeedCount = 0;   // 同步成功的事项数量
                        int newCount = 0;   // 首次进行同步的事项数量

                        // 排序和分组。按照分组提交给 dp2library 服务器
                        // TODO: 但进度显示不应该太细碎？应该按照总的进度来显示
                        var groups = GroupActions(actions);

                        // List<MessageItem> messages = new List<MessageItem>();

                        // 准备对话框
                        // SubmitWindow progress = PageMenu.PageShelf?.OpenProgressWindow();
                        SubmitWindow progress = PageMenu.PageShelf?.ProgressWindow;

                        int start = 0;  // 当前 group 开始的偏移
                        int total = actions.Count;
                        foreach (var group in groups)
                        {
                            int current_count = group.Count;    // 当前 group 包含的动作数量

                            var result = await SubmitCheckInOutAsync(
                            (min, max, value, text) =>
                            {
                                // 2020/4/2
                                // 修正三个值
                                if (max != -1)
                                    max = total;
                                //if (min != -1)
                                //    min += start;
                                if (value != -1)
                                    value += start;

                                if (progress != null)
                                {
                                    App.Invoke(new Action(() =>
                                    {
                                        if (min == -1 && max == -1 && value == -1
                                        && groups.IndexOf(group) == groups.Count - 1)   // 只有最后一次才隐藏进度条
                                            progress.ProgressBar.Visibility = Visibility.Collapsed;
                                        else
                                            progress.ProgressBar.Visibility = Visibility.Visible;

                                        if (text != null)
                                            progress.TitleText = text;  // + " " + (progress.Tag as string);

                                        if (min != -1)
                                            progress.ProgressBar.Minimum = min;
                                        if (max != -1)
                                            progress.ProgressBar.Maximum = max;
                                        if (value != -1)
                                            progress.ProgressBar.Value = value;
                                    }));
                                }
                            },
                            group,
                            "auto_stop");

                            // TODO: 把 group 中报错的信息写入本地数据库的对应事项中

                            /*
                            // 把已经处理成功的 Action 对应在本地数据库中的事项的状态修改
                            List<ActionInfo> processed = new List<ActionInfo>();
                            if (result.RetryActions != null)
                            {
                                foreach (var action in group)
                                {
                                    if (result.RetryActions.IndexOf(action) == -1)
                                    {
                                        ChangeDatabaseActionState(action.ID, "sync");
                                        processed.Add(action);
                                    }
                                }
                            }
                            */
                            if (result.ProcessedActions != null)
                            {
                                // result.ProcessedActions.ForEach(o => { if (o.SyncCount == 0) newCount++; });

                                foreach (var action in result.ProcessedActions)
                                {
                                    if (action.State == "sync")
                                        succeedCount++;
                                    if (action.SyncCount == 1)
                                        newCount++;
                                    // sync/commerror/normalerror/空
                                    // 同步成功/通讯出错/一般出错/从未同步过
                                    await ChangeDatabaseActionStateAsync(action.ID, action);
                                }

                                // TODO: 通知消息正文是否也告知一下同一个 PII 后面有多少个动作被跳过处理？
                                MessageNotifyOverflow(result.ProcessedActions);
                            }

                            if (progress != null && progress.IsVisible)
                            {
                                // 根据全部和已处理集合得到未处理(被跳过的)集合
                                var skipped = GetSkippedActions(group, result.ProcessedActions);
                                foreach (var action in skipped)
                                {
                                    action.State = "_";
                                    action.SyncErrorCode = "skipped";
                                    // action.SyncErrorInfo = "暂时跳过同步";
                                }

                                List<ActionInfo> display = new List<ActionInfo>(result.ProcessedActions);
                                display.AddRange(skipped);
                                // Thread.Sleep(3000);
                                // 刷新显示
                                App.Invoke(new Action(() =>
                                {
                                    progress?.Refresh(display);
                                }));
                            }

                            /*
                            if (result.MessageDocument != null)
                                messages.AddRange(result.MessageDocument.Items);
                                */
                            start += current_count;
                        }

                        // TODO: 更新每个事项的 RetryCount。如果超过 10 次，要把 State 更新为 fail

                        // 将 submit 情况写入日志备查
                        // WpfClientInfo.WriteInfoLog($"重试提交请求:\r\n{ActionInfo.ToString(actions)}\r\n返回结果:{result.ToString()}");



#if REMOVED

                        // 如果本轮有成功的请求，并且进度窗口没有打开，则补充打开进度窗口
                        if ((progress == null || progress.IsVisible == false)
                            && succeedCount > 0)
                            progress = PageMenu.PageShelf?.OpenProgressWindow();

                        // 把执行结果显示到对话框内
                        // 全部事项都重试失败的时候不需要显示
                        if (progress != null && progress.IsVisible
                            && (succeedCount > 0 || newCount > 0))
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                MessageDocument doc = new MessageDocument();
                                doc.AddRange(messages);
                                progress?.PushContent(doc);
                            }));

                            // 显示出来
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    progress?.ShowContent();
                                }));
                        }
#endif
                    }
                    _syncTask = null;

                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"重试专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.CurrentApp?.SetError("sync", $"重试专用线程出现异常: {ex.Message}");
                }
                finally
                {
                    WpfClientInfo.WriteInfoLog("重试专用线程结束");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // 根据全部和已处理集合得到未处理(被跳过的)集合
        static List<ActionInfo> GetSkippedActions(List<ActionInfo> all,
            List<ActionInfo> proccessed)
        {
            List<ActionInfo> results = new List<ActionInfo>();
            foreach (var action in all)
            {
                if (proccessed.IndexOf(action) == -1)
                    results.Add(action);
            }

            return results;
        }


        // static object _syncRoot_database = new object();

        // 限制数据库操作，同一时刻只能一个函数进入
        static AsyncSemaphore _databaseLimit = new AsyncSemaphore(1);

        // sync/commerror/normalerror/空
        // 同步成功/通讯出错/一般出错/从未同步过

        // 从外部存储中装载尚未同步的 Actions
        // 注意：这些 Actions 应该先按照 PII 排序分组以后，一组一组进行处理
        public static async Task<List<ActionInfo>> LoadRetryActionsFromDatabaseAsync()
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new RequestContext())
                {
                    context.Database.EnsureCreated();
                    var items = context.Requests.Where(o => o.State != "sync" && o.State != "dontsync")
                        .OrderBy(o => o.ID).ToList();
                    var actions = FromRequests(items);
                    WpfClientInfo.WriteInfoLog($"从本地数据库装载 Actions 成功。内容如下：\r\n{ActionInfo.ToString(actions)}");
                    return actions;
                }
            }
        }

        static async Task ChangeDatabaseActionStateAsync(int id, ActionInfo action)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new RequestContext())
                {
                    var item = context.Requests.FirstOrDefault(o => o.ID == id);
                    item.State = action.State;
                    item.SyncErrorInfo = action.SyncErrorInfo;
                    item.SyncErrorCode = action.SyncErrorCode;
                    item.SyncCount = action.SyncCount;
                    item.SyncOperTime = action.SyncOperTime;
                    item.ActionString = action.ActionString;
                    context.SaveChanges();
                }
            }
        }

        // 把 Actions 追加保存到本地数据库
        // 当本函数执行完以后，ActionInfo 对象的 ID 有了值，和数据库记录的 ID 对应
        public static async Task SaveActionsToDatabaseAsync(List<ActionInfo> actions)
        {
            try
            {
                using (var releaser = await _databaseLimit.EnterAsync())
                {
                    using (var context = new RequestContext())
                    {
                        context.Database.EnsureCreated();

                        var requests = FromActions(actions);
                        foreach (var request in requests)
                        {
                            // 注：这样一个一个保存可以保持 ID 的严格从小到大。因为这些事项之间是有严格顺序关系的(借和还顺序不能颠倒)
                            await context.Requests.AddRangeAsync(request);
                            int nCount = await context.SaveChangesAsync();

                            // 2020/4/27
                            // 如果是还书动作，需要更新它之前的全部同 PII 的借书动作的 LinkID 字段
                            if (request.Action == "return" || request.Action == "inventory")
                            {
                                var borrowRequests = context.Requests.
                                    Where(o => o.PII == request.PII && o.Action == "borrow" && o.LinkID == null)
                                    .ToList();
                                foreach (var item in borrowRequests)
                                {
                                    item.LinkID = request.ID.ToString();
                                    context.Requests.Update(item);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }

                        Debug.Assert(requests.Count == actions.Count, "");
                        // 刷新 ActionInfo 对象的 ID
                        for (int i = 0; i < requests.Count; i++)
                        {
                            actions[i].ID = requests[i].ID;
                        }

                        WpfClientInfo.WriteInfoLog($"Actions 保存到本地数据库成功。内容如下：\r\n{ActionInfo.ToString(actions)}");
                    }
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SaveActionsToDatabase() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                throw ex;
            }
        }

        // 从操作日志数据库中把一些需要重试的事项移走
        // 原理：当首次初始化以后，已经初始化确认在书架内的图书，已经进行了还书操作，那么此前累积的需要重试借书或者还书的同步请求，都可以不执行了。这样不会造成图书丢失。但可能会丢掉一些中间操作信息
        // 改进：可以不删除，但把这些事项的状态标记为 “放弃重试”
        public static async Task RemoveRetryActionsFromDatabaseAsync(IEnumerable<string> piis)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new RequestContext())
                {
                    // context.Database.Migrate();
                    context.Database.EnsureCreated();
                    foreach (var pii in piis)
                    {
                        var items = context.Requests.Where(o => o.PII == pii && o.State != "sync").ToList();
                        // context.Requests.RemoveRange(items);
                        items.ForEach(o => o.State = "dontsync");
                        context.SaveChanges();
                    }
                }
            }
        }

        static List<ActionInfo> FromRequests(List<RequestItem> requests)
        {
            List<ActionInfo> actions = new List<ActionInfo>();
            foreach (var request in requests)
            {
                ActionInfo action = new ActionInfo();
                action.Operator = request.OperatorString == null ? null :
                    JsonConvert.DeserializeObject<Operator>(request.OperatorString);
                // action.Operator = request.Operator;
                action.Entity = JsonConvert.DeserializeObject<Entity>(request.EntityString);
                // action.Entity = request.Entity;
                action.Action = request.Action;
                action.TransferDirection = request.TransferDirection;
                action.Location = request.Location;
                action.CurrentShelfNo = request.CurrentShelfNo;
                action.BatchNo = request.BatchNo;
                action.ID = request.ID;
                action.SyncCount = request.SyncCount;
                action.State = request.State;
                action.SyncErrorInfo = request.SyncErrorInfo;
                action.SyncErrorCode = request.SyncErrorCode;
                action.OperTime = request.OperTime;
                action.SyncOperTime = request.SyncOperTime;
                action.ActionString = request.ActionString;
                actions.Add(action);
            }

            return actions;
        }

        static List<RequestItem> FromActions(List<ActionInfo> actions)
        {
            List<RequestItem> requests = new List<RequestItem>();
            foreach (var action in actions)
            {
                RequestItem request = new RequestItem();

                // 2020/4/27
                request.OperatorID = action.Operator?.PatronBarcode;

                request.PII = action.Entity?.PII;
                // TODO: 若 PII 为空，写入 UID?
                request.OperatorString = action.Operator == null ? null : JsonConvert.SerializeObject(action.Operator);
                request.EntityString = JsonConvert.SerializeObject(action.Entity);
                /*
                request.Operator = action.Operator.Clone();
                request.Entity = action.Entity.Clone();
                */
                request.Action = action.Action;
                request.TransferDirection = action.TransferDirection;
                request.Location = action.Location;
                request.CurrentShelfNo = action.CurrentShelfNo;
                request.BatchNo = action.BatchNo;
                request.SyncCount = action.SyncCount;
                request.State = action.State;
                request.SyncErrorInfo = action.SyncErrorInfo;
                request.SyncErrorCode = action.SyncErrorCode;
                if (action.OperTime == DateTime.MinValue)
                    request.OperTime = DateTime.Now;
                else
                    request.OperTime = action.OperTime;

                request.SyncOperTime = action.SyncOperTime;
                request.ActionString = action.ActionString;
                requests.Add(request);
            }

            return requests;
        }


        static void MessageNotifyOverflow(List<ActionInfo> actions)
        {
            // 检查超额图书
            List<string> overflow_titles = new List<string>();
            int i = 1;
            actions.ForEach(item =>
            {
                if (item.Action == "borrow" && item.SyncErrorCode == "overflow")
                {
                    var pii = GetPiiString(item.Entity);
                    overflow_titles.Add($"{i++}) {SubmitDocument.ShortTitle(item.Entity.Title)} [{pii}]");
                }
            });
            if (overflow_titles.Count > 0)
            {
                PageShelf.TrySetMessage(null, $"下列图书发生超额借阅：\r\n{StringUtil.MakePathList(overflow_titles, "\r\n")}");
            }
        }

        // 对 Actions 按照 PII 进行分组
        static List<List<ActionInfo>> GroupActions(List<ActionInfo> actions)
        {
            // 按照 PII 分装
            // PII --> List<ActionInfo>
            Hashtable table = new Hashtable();
            foreach (var action in actions)
            {
                string pii = GetPiiString(action.Entity);
                List<ActionInfo> list = table[pii] as List<ActionInfo>;
                if (list == null)
                {
                    list = new List<ActionInfo>();
                    table[pii] = list;
                }
                list.Add(action);
            }

            return new List<List<ActionInfo>>(table.Values.Cast<List<ActionInfo>>());
            /*
            List<List<ActionInfo>> results = new List<List<ActionInfo>>();
            foreach(var key in table.Keys)
            {
                results.Add(table[key] as List<ActionInfo>);
            }

            return results;
            */
        }

        // 获得 PII 字符串。如果 PII 为空，会改取 UID 返回
        public static string GetPiiString(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.PII))
                return $"UID:{entity.UID}";
            return entity.PII;
        }
    }
}
