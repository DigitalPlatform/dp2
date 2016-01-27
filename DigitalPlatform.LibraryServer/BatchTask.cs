using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Web;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    // 批处理任务
    public class BatchTask : IDisposable
    {
        public bool ManualStart = false;    // 本轮是否为手动启动？

        public bool Loop = false;
        int m_nPrevLoop = -1;   // 3态 -1:尚未初始化 0:false 1:true

        // 启动参数
        public BatchTaskStartInfo StartInfo = null;

        // 其他启动参数
        // 当任务正在执行的时候，追加其他参数，可以让任务继续向后执行
        public List<BatchTaskStartInfo> StartInfos = new List<BatchTaskStartInfo>();

        // 任务名
        public string Name = "";

        // 进度文件
        Stream m_stream = null;
        public string ProgressFileName = "";
        public long ProgressFileVersion = 0;

        public string ProgressText = "";

        // //

        internal bool m_bClosed = true;

        internal LibraryApplication App = null;
        internal RmsChannelCollection RmsChannels = new RmsChannelCollection();

        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        internal Thread threadWorker = null;
        // TODO: 似乎 eventClose 用 ManualResetEvent 更好
        internal ManualResetEvent eventClose = new ManualResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号
        internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        public int PerTime = 60 * 60 * 1000;	// 1小时

        public virtual void Dispose()
        {
            this.Close();

            RmsChannels.Dispose();

            eventClose.Dispose();
            eventActive.Dispose();
            eventFinished.Dispose();
        }

        public void Activate()
        {
            eventActive.Set();
        }

        // 是否该停止处理，用于日志恢复以外的其它任务
        public virtual bool Stopped
        {
            get
            {
                //if (this.App.HangupReason == HangupReason.LogRecover)
                //    return true;

                if (this.App.ContainsHangup("LogRecover") == true)
                    return true;

                if (this.App.PauseBatchTask == true)
                    return true;
                return this.m_bClosed;
            }
        }

        // 读取上次最后处理的时间
        public int ReadLastTime(
            string strMonitorName,
            out string strLastTime,
            out string strError)
        {
            strError = "";
            strLastTime = "";

            string strFileName = PathUtil.MergePath(this.App.LogDir, strMonitorName + "_lasttime.txt");

            try
            {
                lock (this.m_lock)
                {
                    using (StreamReader sr = new StreamReader(strFileName, Encoding.UTF8))
                    {
                        strLastTime = sr.ReadLine();  // 读入时间行
                    }
                }

                return 1;
            }
            catch (FileNotFoundException /*ex*/)
            {
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "open file '" + strFileName + "' error : " + ex.Message;
                return -1;
            }
        }

        // 写入断点记忆文件
        public void WriteLastTime(string strMonitorName,
            string strLastTime)
        {
            lock (this.m_lock)
            {
                string strFileName = PathUtil.MergePath(this.App.LogDir, strMonitorName + "_lasttime.txt");

                // 删除原来的文件
                File.Delete(strFileName);

                if (string.IsNullOrEmpty(strLastTime) == false)
                {
                    // 写入新内容
                    StreamUtil.WriteText(strFileName,
                        strLastTime);
                }
            }
        }

        // 本轮是不是逢上了每日启动时间(以后)?
        // TODO: 如果上次记载的时间，大大超过当前日期，则一直会屏蔽启动。是否可以在这种情况下强制启动，以便达到促使覆盖上次操作时间的目的？
        // parameters:
        //      strLastTime 最后一次执行过的时间 RFC1123格式
        //      strStartTimeDef 返回定义的每日启动时间
        //      bRet    是否到了每日启动时间
        // return:
        //      -2  strLastTime 格式错误
        //      -1  一般错误
        //      0   没有找到startTime配置参数
        //      1   找到了startTime配置参数
        public int IsNowAfterPerDayStart(
            string strMonitorName,
            ref string strLastTime,
            out bool bRet,
            out string strStartTimeDef,
            out string strError)
        {
            strError = "";
            strStartTimeDef = "";

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//monitors/" + strMonitorName);
            if (node == null)
            {
                bRet = false;
                return 0;
            }

            string strStartTime = DomUtil.GetAttr(node, "startTime");
            if (String.IsNullOrEmpty(strStartTime) == true)
            {
                bRet = false;
                return 0;
            }

            strStartTimeDef = strStartTime;

            string strHour = "";
            string strMinute = "";

            int nRet = strStartTime.IndexOf(":");
            if (nRet == -1)
            {
                strHour = strStartTime.Trim();
                strMinute = "00";
            }
            else
            {
                strHour = strStartTime.Substring(0, nRet).Trim();
                strMinute = strStartTime.Substring(nRet + 1).Trim();
            }

            int nHour = 0;
            int nMinute = 0;
            try
            {
                nHour = Convert.ToInt32(strHour);
                nMinute = Convert.ToInt32(strMinute);
            }
            catch
            {
                bRet = false;
                strError = "时间值 " + strStartTime + " 格式不正确。应为 hh:mm";
                return -1;   // 格式不正确
            }

            // 当前时间
            DateTime now1 = DateTime.Now;

            // 观察本日是否已经做过了
            if (String.IsNullOrEmpty(strLastTime) == false)
            {
                DateTime lasttime;

                try
                {
                    lasttime = DateTimeUtil.FromRfc1123DateTimeString(strLastTime);

                    if (lasttime.Year == now1.Year
                        && lasttime.Month == now1.Month
                        && lasttime.Day == now1.Day)
                    {
                        bRet = false;   // 今天已经做过了
                        return 1;
                    }

                }
                catch
                {
                    bRet = false;
                    strError = "strLastTime '" + strLastTime + "' 格式错误";
                    return -2;
                }

                // 2014/3/22
                TimeSpan delta = new DateTime(now1.Year, now1.Month, now1.Day)
                    - new DateTime(lasttime.Year, lasttime.Month, lasttime.Day);
                // 上次做过的已经是昨天以前
                if (delta.TotalDays > 1)
                {
                    bRet = true;
                    return 1;
                }
            }
            else
            {
                // strLastTime 为空
                // 把当前时间作为上次处理的时间。这样可以避免以后永远轮不到处理时间
                strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime());
            }

            // 今天的定点时刻
            DateTime now2 = new DateTime(now1.Year,
                now1.Month,
                now1.Day,
                nHour,
                nMinute,
                0);

            if (now1 >= now2)
                bRet = true;
            else
                bRet = false;
            return 1;
        }

        public BatchTask(LibraryApplication app,
            string strName)
        {
            if (String.IsNullOrEmpty(strName) == true)
                this.Name = this.DefaultName;
            else
                this.Name = strName;

            this.App = app;
            this.RmsChannels.GUI = false;

            this.RmsChannels.AskAccountInfo -= new AskAccountInfoEventHandle(RmsChannels_AskAccountInfo);
            this.RmsChannels.AskAccountInfo += new AskAccountInfoEventHandle(RmsChannels_AskAccountInfo);

            Debug.Assert(this.App != null, "");
            this.ProgressFileName = this.App.GetTempFileName("batch_progress"); //  Path.GetTempFileName();
            try
            {
                // 如果文件存在，就打开，如果文件不存在，就创建一个新的
                m_stream = File.Open(
    this.ProgressFileName,
    FileMode.OpenOrCreate,
    FileAccess.ReadWrite,
    FileShare.ReadWrite);
                this.ProgressFileVersion = DateTime.Now.Ticks;
            }
            catch (Exception ex)
            {
                string strError = "打开或创建文件 '" + this.ProgressFileName + "' 发生错误: " + ex.Message;
                throw new Exception(strError);
            }

            m_stream.Seek(0, SeekOrigin.End);
        }

        // 清除进度文件内容
        public void ClearProgressFile()
        {
            if (String.IsNullOrEmpty(this.ProgressFileName) == false)
            {
                if (this.m_stream != null)
                {
                    this.m_stream.SetLength(0);
                }
            }

            this.ProgressFileVersion = DateTime.Now.Ticks;  // 2009/7/16
        }

        public virtual string DefaultName
        {
            get
            {
                throw new Exception("DefaltName尚未实现");
            }
        }

        public string Dp2UserName
        {
            get
            {
                return App.ManagerUserName;
            }
        }

        public string Dp2Password
        {
            get
            {
                return App.ManagerPassword;
            }
        }

        void RmsChannels_AskAccountInfo(object sender, AskAccountInfoEventArgs e)
        {
            e.Owner = null;

            ///
            e.UserName = this.Dp2UserName;
            e.Password = this.Dp2Password;
            e.Result = 1;
        }

        // 启动工作线程
        public void StartWorkerThread()
        {
            if (this.threadWorker != null
                && this.threadWorker.IsAlive == true)
            {
                this.eventActive.Set();
                this.eventClose.Reset();    // 2006/11/24
                return;
            }

            this.m_bClosed = false;

            this.eventActive.Set();
            this.eventClose.Reset();    // 2006/11/24

            this.threadWorker =
                new Thread(new ThreadStart(this.ThreadMain));

            // Thread.Sleep(1);

            if (this.m_nPrevLoop != -1)
                this.Loop = this.m_nPrevLoop == 1 ? true : false;   // 恢复上一次的Loop值
            try
            {
                this.threadWorker.Start();
            }
            catch (Exception ex)
            {
                string strErrorText = "StartWorkerThread()出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.App.WriteErrorLog(strErrorText);

                try
                {
                    this.threadWorker.Abort();
                }
                catch
                {
                }

                try
                {
                    // 丢弃原来的线程。重新创建一个线程
                    this.threadWorker =
                        new Thread(new ThreadStart(this.ThreadMain));
                    this.threadWorker.Start();
                }
                catch
                {
                }
            }
        }

        // 启动并激活工作线程
        public void ActivateWorkerThread()
        {
            if (this.threadWorker != null
    && this.threadWorker.IsAlive == true)
            {
                this.eventActive.Set();
                return;
            }

            StartWorkerThread();

            /*
            this.eventActive.Set();
            if (this.threadWorker == null)
            {
                this.threadWorker =
                    new Thread(new ThreadStart(this.ThreadMain));
            }
            if (this.threadWorker.IsAlive == false)
            {
                try
                {
                    this.threadWorker.Start();
                }
                catch (Exception ex)
                {
                    string strErrorText = "ActivateWorkerThread()出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }
             * */
        }

        public void Close()
        {
#if NO
            this.eventClose.Set();
            this.m_bClosed = true;
#endif
            this.Stop();

            if (this.m_stream != null)
            {
                this.m_stream.Close();
                this.m_stream = null;
            }

            if (String.IsNullOrEmpty(this.ProgressFileName) == false)
            {
                File.Delete(this.ProgressFileName);
                this.ProgressFileVersion++;
            }
        }

        public void Stop()
        {
            this.eventClose.Set();
            this.m_bClosed = true;

            this.m_nPrevLoop = this.Loop == true ? 1: 0;   // 记忆前一次的Loop值
            this.Loop = false;  // 防止过一会继续循环
        }

        // 设置进度文本
        // 多线程：安全
        internal void SetProgressText(string strText)
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            try
            {
                this.ProgressText = strText;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 追加结果文本
        // 包装版本
        internal void AppendResultText(string strText)
        {
            AppendResultText(true, strText);
        }

        // 追加结果文本
        // 包装版本
        internal void AppendResultTextNoTime(string strText)
        {
            AppendResultText(false, strText);
        }

        // 追加结果文本
        // 多线程：安全
        internal void AppendResultText(bool bDisplayTime,
            string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return;
            if (m_stream == null)
                return;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            try
            {
                strText = (bDisplayTime == true ? DateTime.Now.ToString() + " " : "")
                    + HttpUtility.HtmlEncode(strText);  // 2007/10/10 htmlencode()
                byte[] buffer = Encoding.UTF8.GetBytes(strText);

                m_stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 获得任务当前信息
        // 多线程：安全
        public BatchTaskInfo GetCurrentInfo(long lResultStart,
            int nMaxResultBytes)
        {
            this.m_lock.AcquireReaderLock(m_nLockTimeout);

            try
            {
                BatchTaskInfo info = new BatchTaskInfo();
                info.Name = this.Name;
                if (this.m_bClosed == false)
                    info.State = "运行中";
                else
                    info.State = "停止";

                if (this.App.PauseBatchTask == true)
                    info.ProgressText = "[注意：全部批处理任务已经被暂停] " + this.ProgressText;
                else
                    info.ProgressText = this.ProgressText;

                byte[] baResultText = null;
                long lOffset = 0;
                long lTotalLength = 0;
                this.GetResultText(lResultStart,
                    nMaxResultBytes,
                    out baResultText,
                    out lOffset,
                    out lTotalLength);
                info.ResultText = baResultText;
                info.ResultOffset = lOffset;
                info.ResultTotalLength = lTotalLength;
                info.ResultVersion = this.ProgressFileVersion;

                return info;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

        }

        // 获得输出结果文本
        // parameters:
        //      lEndOffset  本次获取的末尾偏移
        //      lTotalLength    返回流的最大长度
        public void GetResultText(long lStart,
            int nMaxBytes,
            out byte[] baResult,
            out long lEndOffset,
            out long lTotalLength)
        {
            baResult = null;
            lEndOffset = 0;

            lTotalLength = this.m_stream.Length;

            long lLength = this.m_stream.Length - lStart;

            if (lLength <= 0)
            {
                lEndOffset = this.m_stream.Length;
                return;
            }

            baResult = new byte[Math.Min(nMaxBytes, (int)lLength)];

            this.m_stream.Seek(lStart, SeekOrigin.Begin);
            try
            {
                int nByteReaded = this.m_stream.Read(baResult, 0, baResult.Length);

                Debug.Assert(nByteReaded == baResult.Length);

                lEndOffset = lStart + nByteReaded;
            }
            finally
            {
                // 指针回到文件末尾
                this.m_stream.Seek(0, SeekOrigin.End);
            }

            return;
        }

        // 工作线程
        public virtual void ThreadMain()
        {
            try
            {
                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventActive;

                while (true)
                {
                    int index = 0;
                    try
                    {
                        index = WaitHandle.WaitAny(events, PerTime, false);
                    }
                    catch (System.Threading.ThreadAbortException /*ex*/)
                    {
                        /*
                        // 调试用
                        LibraryApplication.WriteWindowsLog("BatchTask俘获了ThreadAbortException异常", EventLogEntryType.Information);
                         * */
                        this.App.Save(null, false);    // 触发保存
                        this.App.WriteErrorLog("刚才是ThreadAbortException触发了配置文件保存");
                        break;
                    }

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // 超时
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();    // 2013/11/23 只让堵住的时候发挥作用

                    }
                    else if (index == 0)
                    {
                        break;
                    }
                    else
                    {
                        // 得到激活信号
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();    // 2013/11/23 只让堵住的时候发挥作用
                    }

                    // 是否循环?
                    if (this.Loop == false)
                        break;
                }
                this.ManualStart = false;   // 这个变量只在一轮处理中管用
            }
            catch (Exception ex)
            {
                string strErrorText = "BatchTask工作线程出现异常: " + ExceptionUtil.GetDebugText(ex);
                try
                {
                    this.App.WriteErrorLog(strErrorText);
                }
                catch
                {
                    LibraryApplication.WriteWindowsLog(strErrorText);
                }
            }
            finally
            {
                // 2009/7/16 移动到这里
                eventFinished.Set();

                // 2009/7/16 新增
                this.m_bClosed = true;
            }

        }

        // 工作线程每一轮循环的实质性工作
        public virtual void Worker()
        {

        }

    }
}
