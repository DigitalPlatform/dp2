using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    public class RebuildKeysTask : BatchTask
    {
        public RebuildKeysTask(KernelApplication app, 
            string strName)
            : base(app, strName)
        {
            this.Loop = false;
        }

        public override string DefaultName
        {
            get
            {
                return "重建检索点";
            }
        }

        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;
            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";
            int nRet = 0;

            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strTaskName = "rebuildKeysTask";
            {
                string strLastTime = "";

                nRet = ReadLastTime(
                    strTaskName,
                    out strLastTime,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从文件中获取 "+strTaskName+" 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                string strStartTimeDef = "";
                bool bRet = false;
                nRet = IsNowAfterPerDayStart(
                    strTaskName,
                    strLastTime,
                    out bRet,
                    out strStartTimeDef,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "获取 "+strTaskName+" 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                // 如果nRet == 0，表示没有配置相关参数，则兼容原来的习惯，每次都作
                if (nRet == 0)
                {

                }
                else if (nRet == 1)
                {
                    bPerDayStart = true;

                    if (bRet == false)
                    {
                        if (this.ManualStart == true)
                            this.AppendResultText("已试探启动任务 '" + this.Name + "'，但因没有到每日启动时间 " + strStartTimeDef + " 而未能启动。(上次任务处理结束时间为 " + DateTimeUtil.LocalTime(strLastTime) + ")\r\n");

                        return; // 还没有到每日时间
                    }
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(定时)" : "(不定时)") + strTaskName + " 启动。");
            }

            this.AppendResultText("开始新一轮循环\r\n");


            for (int i = 0; ; i++)
            {
                // 系统挂起的时候，不运行本线程
                // 2008/5/27
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;
                // 2012/2/4
                if (this.App.PauseBatchTask == true)
                    break;

                if (this.m_bClosed == true)
                    break;

                if (this.Stopped == true)
                    break;

                // AppendResultText("针对读者库 " + strReaderDbName + " 的循环结束。共处理 " + nOnePassRecCount.ToString() + " 条记录。\r\n");

            }
            // AppendResultText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。\r\n");
            // SetProgressText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。");

            {
                Debug.Assert(this.App != null, "");

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.UtcNow.ToLocalTime());
                WriteLastTime(strTaskName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strTaskName + "结束。共处理记录 ? 个。";
                this.App.WriteErrorLog(strErrorText);

            }

            return;
        ERROR1:
            AppendResultText("RebuildKeysTask thread error : " + strError + "\r\n");
        this.App.WriteErrorLog("RebuildKeysTask thread error : " + strError + "\r\n");
            return;
        }
    }
}
