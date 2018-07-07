using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using System.Threading.Tasks;

namespace dp2Circulation
{
    /// <summary>
    /// 一个显示属性的任务
    /// </summary>
    public class PropertyTask
    {
        public PropertyTaskList Container = null;

        public string GetWaitingHtml(string strText)
        {
            string strGifFileName = Path.Combine(Program.MainForm.DataDir, "ajax-loader3.gif");

            // 显示 正在处理
            return "<html>" +
    Program.MainForm.GetMarcHtmlHeadString(true) +
    "<body style='background-color: #aaaaaa;'>" +
    "<h2 align='center'><img src='" + strGifFileName + "' /></h2>"
            + "<h2 align='center'>" + HttpUtility.HtmlEncode(strText) + "</h2>"
            + "</body></html>";
        }

        // 打开对话框
        // return:
        //      false   无法打开对话框
        //      true    成功打开
        public virtual bool OpenWindow()
        {
            return true;
        }

        // 装载数据
        public virtual bool LoadData()
        {
            return true;
        }

        public virtual bool ShowData()
        {
            // TODO: 从外部装载数据成功后，要回馈给调用者
            return true;
        }

        // 中断正在进行的长操作
        public virtual bool Cancel()
        {
            return true;
        }

    }

    /// <summary>
    /// 显示属性的任务列表
    /// </summary>
    public class PropertyTaskList : ThreadBase
    {
        List<PropertyTask> _tasks = new List<PropertyTask>();

#if NO
        public MainForm MainForm
        {
            get;
            set;
        }
#endif

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        PropertyTask _currentTask = null;
        private static readonly Object syncRoot = new Object();

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            try
            {
                PropertyTask task = null;
                int nRestCount = 0;

                // 取出第一个元素
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
                try
                {
                    if (this._tasks.Count == 0)
                        return;
                    task = this._tasks[0];
                    this._tasks.RemoveAt(0);
                    nRestCount = this._tasks.Count;
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }

                try
                {
                    lock (syncRoot)
                    {
                        this._currentTask = task;
                    }

                    if (task.LoadData() == false)
                        return;

                    task.ShowData();

                    lock (syncRoot)
                    {
                        this._currentTask = null;
                    }
                }
                finally
                {
                    if (nRestCount > 0)
                        this.Activate();
                }
            }
            catch (Exception ex)
            {
                MainForm.TryWriteErrorLog("PropertyTaskList Worker() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                Program.MainForm.ReportError("dp2circulation 调试信息", "PropertyTaskList Worker() 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
        }

        void CancelTask(PropertyTask task)
        {
            if (task != null)
                task.Cancel();
        }

        // 加入一个任务到列表中
        // parameters:
        //      bClearBefore    清除以前积累的任务。也要中断插入前正在做的任务
        public void AddTask(PropertyTask task, bool bClearBefore)
        {
            if (this.m_lock.TryEnterWriteLock(500) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                if (bClearBefore)
                {
                    this._tasks.Clear();

                    lock (syncRoot)
                    {
                        if (_currentTask != null)
                            Task.Factory.StartNew(() => CancelTask(_currentTask));
                    }
                }

                task.Container = this;
                this._tasks.Add(task);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 触发任务开始执行
            this.Activate();
        }

        public void Close(bool bForce = true)
        {
            this.Clear();

            this.StopThread(bForce);
        }

        public void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                this._tasks.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }
    }
}
