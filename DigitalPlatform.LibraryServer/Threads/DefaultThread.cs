using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 作管理的基本线程
    /// 都是一些需要较短时间就可以处理的小任务
    /// </summary>
    public class DefaultThread : BatchTask
    {
        public DefaultThread(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
            this.PerTime = 5 * 60 * 1000;	// 5分钟
        }

        public override string DefaultName
        {
            get
            {
                return "管理线程";
            }
        }

        DateTime m_lastRetryTime = DateTime.Now;
        int m_nRetryAfterMinutes = 5;   // 每间隔多少分钟以后重试一次

        int _createLocationResultsetCount = 0;
        DateTime _locationResultsetLastTime = new DateTime(0);  // 最近一次执行结果集更新的时间

        // 一次操作循环
        public override void Worker()
        {
            // 首次自动启动创建馆藏地结果集的任务。以后就靠固定时间启动，或者 _request 触发
            if (_createLocationResultsetCount == 0)
            {
                this.App.StartCreateLocationResultset("");
                this.App.StartCreateBiblioResultset("");
                _locationResultsetLastTime = DateTime.Now;
                _createLocationResultsetCount++;
            }
            else
            {
                DateTime now = DateTime.Now;
                // 每隔 24 小时自动启动执行一次
                // TODO: 有可能每次都在每天的同一小时开始执行，如果这正好是每日繁忙时段就不理想了。一个办法是可以定义每日定时时间；另外一个做法是增加一点随机性，不是正好 24 小时间隔
                if (now - _locationResultsetLastTime > new TimeSpan(25, 0, 0)
                    || now.Day != _locationResultsetLastTime.Day    // 跨越了一天。2018/5/10
                    || this.App.NeedRebuildResultset() == true)
                {
                    this.App.StartCreateLocationResultset("");
                    this.App.StartCreateBiblioResultset("");
                    _locationResultsetLastTime = DateTime.Now;
                }

                // 促使累积的请求执行
                this.App.StartCreateLocationResultset(null);
                this.App.StartCreateBiblioResultset(null);
                _createLocationResultsetCount++;
            }

            // 清理 Garden
            try
            {
                // TODO: 当 hashtable 已经满了的时候，需要缩短存活时间
                if (this.App.Garden.IsFull == true)
                    this.App.Garden.CleanPersons(new TimeSpan(0, 5, 0), this.App.Statis);    // 5分钟 紧急情况下 REST Session的最长存活时间
                else
                    this.App.Garden.CleanPersons(new TimeSpan(0, 20, 0), this.App.Statis);    // 20分钟 REST Session的最长存活时间
            }
            catch (Exception ex)
            {
                string strErrorText = "DefaultTread中 CleanPersons() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.App.WriteErrorLog(strErrorText);
            }

            // 代为刷新Statis
            if (this.App.Statis != null)
            {
                try
                {
                    this.App.Statis.Flush();
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 this.App.Statis.Flush() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 2020/10/19
            try
            {
                OnlineStatis.ClearIdle(TimeSpan.FromMinutes(60));
            }
            catch (Exception ex)
            {
                string strErrorText = "OnlineStatis.ClearIdle() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.App.WriteErrorLog(strErrorText);
            }

            // 清除文件 Stream
            if (this.App._physicalFileCache != null)
            {
                try
                {
                    this.App._physicalFileCache.ClearIdle(TimeSpan.FromMinutes(5));
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 this.App._physicalFileCache.ClearIdle() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 及时保存library.xml的变化
            if (this.App.Changed == true)
            {
                this.App.Flush();
            }

            // 清理Sessions
            try
            {
                // TODO: 当 hashtable 已经满了的时候，需要缩短存活时间
                if (this.App.SessionTable.IsFull == true)
                    this.App.SessionTable.CleanSessions(new TimeSpan(0, 5, 0));    // 5分钟 紧急情况下 REST Session的最长存活时间
                else
                    this.App.SessionTable.CleanSessions(new TimeSpan(0, 20, 0));    // 20分钟 REST Session的最长存活时间
            }
            catch (Exception ex)
            {
                string strErrorText = "DefaultTread中 CleanSessions() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.App.WriteErrorLog(strErrorText);
            }

            int nRet = 0;
            string strError = "";

            if (this.App.kdbs == null
                && (DateTime.Now - this.m_lastRetryTime).TotalMinutes >= m_nRetryAfterMinutes)
            {
                try
                {
                    nRet = this.App.InitialKdbs(this.RmsChannels,
            out strError);
                    if (nRet == -1)
                    {
                        this.App.WriteErrorLog("ERR003 初始化kdbs失败: " + strError);
                    }
                    else
                    {
                        // 检查 dpKernel 版本号
                        nRet = this.App.CheckKernelVersion(this.RmsChannels,
                            out strError);
                        if (nRet == -1)
                            this.App.WriteErrorLog(strError);
                    }
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 InitialKdbs() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 
            if (this.App.vdbs == null
                && (DateTime.Now - this.m_lastRetryTime).TotalMinutes >= m_nRetryAfterMinutes)
            {
                try
                {
                    nRet = this.App.InitialVdbs(this.RmsChannels,
                        out strError);
                    if (nRet == -1)
                    {
                        this.App.WriteErrorLog("ERR004 初始化vdbs失败: " + strError);
                    }
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 InitialVdbs() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            if (this.App._mongoClient == null && string.IsNullOrEmpty(this.App.MongoDbConnStr) == false
            && (DateTime.Now - this.m_lastRetryTime).TotalMinutes >= m_nRetryAfterMinutes)
            {
                try
                {
                    nRet = this.App.InitialMongoDatabases(out strError);
                    if (nRet == -1)
                    {
                        this.App.WriteErrorLog("ERR006 初始化 mongodb database 失败: " + strError);
                    }
                    else
                    {
                        // 清除 Hangup 状态
                        if (this.App.ContainsHangup("ERR002"))
                            this.App.ClearHangup("ERR002");
                    }
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 InitialMongoDatabases() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            if (this.App.kdbs == null || this.App.vdbs == null
                || (this.App._mongoClient == null && string.IsNullOrEmpty(this.App.MongoDbConnStr) == false))
            {
                m_nRetryAfterMinutes++;
            }

            // 2012/9/23
            if (this.App.OperLog != null && this.App.OperLog.Cache != null)
            {
                try
                {
                    this.App.OperLog.Cache.Shrink(new TimeSpan(0, 1, 0));    // 一分钟
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 压缩 OperLog.Cache 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 2016/11/6
            try
            {
                this.App.InitialMsmq();
            }
            catch (Exception ex)
            {
                string strErrorText = "DefaultTread中 InitialMsmq() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.App.WriteErrorLog(strErrorText);
            }

            // 2016/11/13
            if (this.App.TempCodeTable != null)
            {
                try
                {
                    this.App.TempCodeTable.CleanExpireItems();
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 清除验证码集合 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 2021/9/12
            // 清理本地全局结果集
            {
                try
                {
                    this.App.CleanIdleGlobalMemorySets(TimeSpan.FromHours(24));
                }
                catch(Exception ex)
                {
                    string strErrorText = "DefaultTread中 清除本地全局结果集时 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 2023/1/20
            // 清理 MemoryTable(MemoryChunk机制)
            {
                try
                {
                    this.App.MemoryTable?.CleanIdle(TimeSpan.FromMinutes(5));
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 清除 MemoryTable 时 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.App.WriteErrorLog(strErrorText);
                }
            }

            // 2021/11/21
            // 确保连接到 dp2mserver
            {
                var result = this.App.EnsureConnectMessageServerAsync().Result;
                if (result.Value == -1 && result.ErrorCode != "notEnabled")
                {
                    this.App.WriteErrorLog($"尝试连接到 dp2mserver 服务器时出错: {result.ErrorInfo}");
                }
            }
        }

        public void ClearRetryDelay()
        {
            this.m_nRetryAfterMinutes = 0;
        }
    }
}
