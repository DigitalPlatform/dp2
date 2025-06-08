using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 作管理的基本线程
    /// </summary>
    public class DefaultThread : BatchTask
    {
        public DefaultThread(OpacApplication app,
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

        // 一次操作循环
        public override void Worker()
        {
            /*
            // 代为刷新
            if (this.App.Statis != null)
            {
                this.App.Statis.Flush();
            }
             * */

            if (this.App.Changed == true)
            {
                this.App.Flush();
            }

            if (this.App.ChatRooms != null)
            {
                try
                {
                    this.App.ChatRooms.Flush();
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 ChatRooms.Flush() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    OpacApplication.WriteErrorLog(strErrorText);
                }
            }

            int nRet = 0;
            string strError = "";

#if OPAC_SEARCH_LOG
            // 2012/12/16
            // 将检索日志写入数据库
            if (this.App.SearchLog != null)
            {
                nRet = this.App.SearchLog.Flush(out strError);
                if (nRet == -1)
                {
                    this.App.WriteErrorLog(strError);
                }
            }
#endif

            // 清除过期的 FilterTask 对象
            try
            {
                this.App.CleanFilterTask(new TimeSpan(24, 0, 0));   // TimeSpan(24, 0, 0)
            }
            catch (Exception ex)
            {
                string strErrorText = "DefaultTread中 CleanFilterTask() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                OpacApplication.WriteErrorLog(strErrorText);
            }

            if (this.App.XmlLoaded == false
                && (DateTime.Now - this.m_lastRetryTime).TotalMinutes >= m_nRetryAfterMinutes)
            {
                try
                {
                    string strDebugInfo = "";
                    // return:
                    //      -2  dp2Library版本不匹配
                    //      -1  出错
                    //      0   成功
                    nRet = this.App.GetXmlDefs(
                        false,
                        out strDebugInfo,
                        out strError);
                    if (nRet != 0)
                    {
                        OpacApplication.WriteErrorLog("ERR003 初始化XmlDefs失败: " + strError);
                        m_nRetryAfterMinutes++;
                        return; // 如果必要可修改为继续后面的任务
                    }
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 GetXmlDefs() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    OpacApplication.WriteErrorLog(strErrorText);
                    m_nRetryAfterMinutes++;
                    return; // 如果必要可修改为继续后面的任务
                }

                // 
                this.App.vdbs = null;
                try
                {
                    nRet = this.App.InitialVdbs(out strError);
                    if (nRet == -1)
                    {
                        OpacApplication.WriteErrorLog("ERR004 初始化vdbs失败: " + strError);
                    }
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 InitialVdbs() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    OpacApplication.WriteErrorLog(strErrorText);
                }

                try
                {
                    // <biblioDbGroup> 
                    nRet = this.App.LoadBiblioDbGroupParam(
                        out strError);
                    if (nRet == -1)
                    {
                        OpacApplication.WriteErrorLog("ERR006 初始化BiblioDbGroup失败: " + strError);
                    }
                }
                catch (Exception ex)
                {
                    string strErrorText = "DefaultTread中 LoadBiblioDbGroupParam() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                    OpacApplication.WriteErrorLog(strErrorText);
                }
            }
        }
    }
}

