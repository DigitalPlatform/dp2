using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 本部分是和 FilterTask 管理相关的代码
    /// </summary>
    public partial class OpacApplication
    {
        ReaderWriterLockSlim _filterTaskLock = new ReaderWriterLockSlim();
        Hashtable FilterTasks = new Hashtable();

        // 清除最近没有使用过的 FilterTask 对象
        // parameters:
        //      delta   最近一次用过的时刻距离现在的时间长度。长于这个的对象才会被清除
        public void CleanFilterTask(TimeSpan delta)
        {
            List<string> remove_keys = new List<string>();

            // 第一步，找出过期的 key
            // 读锁定并不阻碍一般性访问
            _filterTaskLock.EnterReadLock();
            try
            {
                foreach (string key in this.FilterTasks.Keys)
                {
                    FilterTask task = (FilterTask)this.FilterTasks[key];
                    if (task == null
                        || (DateTime.Now - task.LastUsedTime) >= delta)
                    {
                        remove_keys.Add(key);   // 这里暂时无法删除，因为 foreach 还要用枚举器
                    }
                }
            }
            finally
            {
                _filterTaskLock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return;

            // 第二步，清除 hashtable 中的对象
            // 因为要删除某些元素，所以用写锁定
            List<FilterTask> delete_items = new List<FilterTask>();
            _filterTaskLock.EnterWriteLock();
            try
            {
                foreach (string key in remove_keys)
                {
                    FilterTask task = (FilterTask)this.FilterTasks[key];
                    if (task == null)
                        continue;

                    // 和 hashtable 脱离关系
                    this.FilterTasks.Remove(key);

                    delete_items.Add(task);
                }
            }
            finally
            {
                _filterTaskLock.ExitWriteLock();
            }

            // 第三步，删除临时文件
            string strTempDir = this.TempDir;
            foreach (FilterTask task in delete_items)
            {
                task.DeleteTempFiles(strTempDir);
            }
        }

        void ClearFilterTask()
        {
            // string strTempDir = this.GetTempDir();
            string strTempDir = this.TempDir;

            List<FilterTask> delete_items = new List<FilterTask>();
            _filterTaskLock.EnterWriteLock();
            try
            {
                foreach (string key in this.FilterTasks.Keys)
                {
                    FilterTask task = (FilterTask)this.FilterTasks[key];
                    if (task != null)
                        delete_items.Add(task);
                }
                this.FilterTasks.Clear();
            }
            finally
            {
                _filterTaskLock.ExitWriteLock();
            }

            foreach (FilterTask task in delete_items)
            {
                task.DeleteTempFiles(strTempDir);
            }
        }

        public FilterTask FindFilterTask(string strName)
        {
            _filterTaskLock.EnterReadLock();
            try
            {
                FilterTask task = (FilterTask)this.FilterTasks[strName];
                if (task != null)
                    task.Touch();
                return task;
            }
            finally
            {
                _filterTaskLock.ExitReadLock();
            }
        }

        // parameters:
        //      task    要设置的 FilterTask 对象。如果为 null，表示要删除名字为 strName 的对象
        public void SetFilterTask(string strName, FilterTask task)
        {
            _filterTaskLock.EnterWriteLock();
            try
            {
                FilterTask old_task = (FilterTask)this.FilterTasks[strName];
                if (old_task == task)
                {
                    if (old_task != null)
                        old_task.Touch();
                    return;
                }

                // 删除任务所创建的结果集文件
                if (old_task != null)
                    old_task.DeleteTempFiles(
                        // this.GetTempDir()
                        this.TempDir
                        );

                // TODO: 是否要定义一个极限值，不让元素数超过这个数目
                if (task == null)
                    this.FilterTasks.Remove(strName);
                else
                {
                    task.Touch();
                    this.FilterTasks[strName] = task;
                }
            }
            finally
            {
                _filterTaskLock.ExitWriteLock();
            }
        }
    }
}
