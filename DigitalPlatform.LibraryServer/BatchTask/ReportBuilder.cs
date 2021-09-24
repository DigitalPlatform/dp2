using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Threading;

using DigitalPlatform.IO;
using DigitalPlatform.LibraryServer.Reporting;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 负责同步数据和创建报表的后台任务
    /// </summary>
    public class ReportBuilder : BatchTask
    {
        public ReportBuilder(LibraryApplication app,
    string strName)
    : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "报表创建";
            }
        }

        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";
            int nRet = 0;

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strMonitorName = "reportBuilder";
            {
                string strLastTime = "";

                nRet = ReadLastTime(
                    strMonitorName,
                    out strLastTime,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从文件中获取 " + strMonitorName + " 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                string strStartTimeDef = "";
                //      bRet    是否到了每日启动时间
                bool bRet = false;
                string strOldLastTime = strLastTime;

                // return:
                //      -2  strLastTime 格式错误
                //      -1  一般错误
                //      0   没有找到startTime配置参数
                //      1   找到了startTime配置参数
                nRet = IsNowAfterPerDayStart(
                    strMonitorName,
                    ref strLastTime,
                    out bRet,
                    out strStartTimeDef,
                    out strError);
                if (nRet == -1 || nRet == -2)
                {
                    string strErrorText = "获取 " + strMonitorName + " 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    if (nRet == -2)
                    {
                        WriteLastTime(strMonitorName, "");
                    }
                    return;
                }

                // 如果nRet == 0，表示没有配置相关参数，则兼容原来的习惯，每次都作
                if (nRet == 0)
                {

                }
                else if (nRet == 1 && startinfo.Start == "activate")
                {
                    // 虽然 library.xml 中定义了每日定时启动，但被前端要求立即启动
                    this.AppendResultText("任务 '" + this.Name + "' 被立即启动\r\n");
                }
                else if (nRet == 1)
                {
                    bPerDayStart = true;

                    if (bRet == false)
                    {
                        if (this.ManualStart == true)
                            this.AppendResultText("已试探启动任务 '" + this.Name + "'，但因没有到每日启动时间 " + strStartTimeDef + " 而未能启动。(上次任务处理结束时间为 " + DateTimeUtil.LocalTime(strLastTime) + ")\r\n");

                        if (string.IsNullOrEmpty(strOldLastTime) == true
                            && string.IsNullOrEmpty(strLastTime) == false)
                        {
                            this.AppendResultText("史上首次启动此任务。已把当前时间当作上次任务处理结束时间 " + DateTimeUtil.LocalTime(strLastTime) + " 写入了断点记忆文件\r\n");
                            WriteLastTime(strMonitorName, strLastTime);
                        }

                        return; // 还没有到每日时间
                    }
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + " 启动。");
            }

            this.AppendResultText("开始新一轮循环\r\n");

            if (string.IsNullOrEmpty(this.App._reportStorageServerName))
            {
                this.AppendResultText("library.xml 中没有配置 reportStorage/@serverName\r\n");
                return;
            }

            var get_result = GetDatabaseConfig();
            if (get_result.Value == -1)
                if (get_result.Value == -1)
                {
                    this.AppendResultText($"GetDatabaseConfig() 出错: {get_result.ErrorInfo}\r\n");
                    return;
                }

            // 装载执行计划 XML 文件。如果不存在此文件，则为首次创建
            var load_result = LoadPlan(true);
            if (load_result.Value == -1)
            {
                this.AppendResultText($"LoadPlan() 出错: {load_result.ErrorInfo}\r\n");
                return;
            }

            var task_dom = load_result.Dom;
            var replication_result = Replication(
    get_result.Config,
    ref task_dom,
    default);
            if (replication_result.Value == -1)
            {
                this.AppendResultText($"Replication() 出错: {replication_result.ErrorInfo}\r\n");
                return;
            }

#if NO
            List<string> bodytypes = new List<string>();

            string strBodyTypesDef = GetBodyTypesDef(); // 已经处理了默认值情况

            if (strBodyTypesDef == "[all]")
            {
                // 全部类型。包括 mq 和外部接口
                bodytypes.Add("dpmail");
                bodytypes.Add("email");
                if (string.IsNullOrEmpty(this.App.OutgoingQueue) == false)
                    bodytypes.Add("mq");    // MSMQ 消息队列
                if (this.App.m_externalMessageInterfaces != null)
                {
                    foreach (MessageInterface message_interface in this.App.m_externalMessageInterfaces)
                    {
                        bodytypes.Add(message_interface.Type);
                    }
                }
            }
            else
                bodytypes = StringUtil.SplitList(strBodyTypesDef);

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            int nTotalRecCount = 0;
            foreach (DigitalPlatform.LibraryServer.LibraryApplication.ReaderDbCfg cfg in this.App.ReaderDbs)
            {
#if NO
                // 系统挂起的时候，不运行本线程
                // 2008/5/27
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;
                // 2012/2/4
                if (this.App.PauseBatchTask == true)
                    break;
#endif
                if (this.Stopped == true)
                    break;

                string strReaderDbName = cfg.DbName;
                string strLibraryCode = cfg.LibraryCode;

                AppendResultText("开始处理读者库 " + strReaderDbName + " 的循环\r\n");

                bool bFirst = true; // 2008/5/27 moved
                string strID = "1";
                int nOnePassRecCount = 0;
                for (; ; nOnePassRecCount++, nTotalRecCount++)
                {
#if NO
                    // 系统挂起的时候，不运行本线程
                    // 2008/2/4
                    if (this.App.HangupReason == HangupReason.LogRecover)
                        break;
                    // 2012/2/4
                    if (this.App.PauseBatchTask == true)
                        break;
#endif
                    if (this.Stopped == true)
                        break;

                    string strStyle = "";
                    strStyle = "data,content,timestamp,outputpath";

                    if (bFirst == true)
                        strStyle += "";
                    else
                    {
                        strStyle += ",next";
                    }

                    string strPath = strReaderDbName + "/" + strID;

                    string strXmlBody = "";
                    string strMetaData = "";
                    string strOutputPath = "";
                    byte[] baOutputTimeStamp = null;

                    // 
                    SetProgressText((nOnePassRecCount + 1).ToString() + " " + strPath);

                    // 获得资源
                    // return:
                    //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                    //		0	成功
                    long lRet = channel.GetRes(strPath,
                        strStyle,
                        out strXmlBody,
                        out strMetaData,
                        out baOutputTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                        {
                            if (bFirst == true)
                            {
                                // 第一条没有找到, 但是要强制循环进行
                                bFirst = false;
                                goto CONTINUE;
                            }
                            else
                            {
                                if (bFirst == true)
                                {
                                    strError = "数据库 " + strReaderDbName + " 记录 " + strID + " 不存在。处理结束。";
                                }
                                else
                                {
                                    strError = "数据库 " + strReaderDbName + " 记录 " + strID + " 是最末一条记录。处理结束。";
                                }
                                break;
                            }

                        }
                        else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                        {
                            bFirst = false;
                            // 把id解析出来
                            strID = ResPath.GetRecordId(strOutputPath);
                            goto CONTINUE;

                        }

                        goto ERROR1;
                    }

#if NO
                    string strLibraryCode = "";
                    nRet = this.App.GetLibraryCode(strOutputPath,
                        out strLibraryCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#endif

                    bFirst = false;

                    this.AppendResultText("正在处理" + (nOnePassRecCount + 1).ToString() + " " + strOutputPath + "\r\n");

                    // 把id解析出来
                    strID = ResPath.GetRecordId(strOutputPath);

                    int nRedoCount = 0;
                REDO:
                    // 处理
                    // parameters:
                    //      bChanged    [out] strReaderXml 是否发生过改变
                    // return:
                    //      -1  出错
                    //      0   正常
                    nRet = DoOneRecord(
                        bodytypes,
                        strOutputPath,
                        strLibraryCode,
                        ref strXmlBody,
                        nRedoCount,
                        baOutputTimeStamp,
                        out bool bChanged,
                        out strError);
                    if (nRet == -1)
                    {
                        AppendResultText("DoOneRecord() error : " + strError + "。\r\n");
                        // 循环并不停止
                    }
                    if (bChanged == true)
                    {
                        // return:
                        //      -2  保存时遇到时间戳冲突，已经重新装载读者记录返回于 strReaderXml 中，时间戳于 output_timestamp 中
                        //      -1  出错
                        //      0   保存成功
                        nRet = SavePatronRecord(channel,
    strOutputPath,
    ref strXmlBody,
    baOutputTimeStamp,
    out byte[] output_timestamp,
    out strError);
                        if (nRet == -1)
                        {
                            AppendResultText("SavePatronRecord() error : " + strError + "。\r\n");
                            // 循环并不停止
                        }
                        else if (nRet == -2)
                        {
                            if (nRedoCount > 10)
                            {
                                AppendResultText("SavePatronRecord() (遇到时间戳不匹配)重试十次以后依然出错，放弃重试。error : " + strError + "。\r\n");
                                // 循环并不停止
                            }
                            baOutputTimeStamp = output_timestamp;
                            goto REDO;
                        }
                    }

                CONTINUE:
                    continue;
                } // end of for

                AppendResultText("针对读者库 " + strReaderDbName + " 的循环结束。共处理 " + nOnePassRecCount.ToString() + " 条记录。\r\n");
            }

            recpath_table.Clear();
#endif

            /*
            AppendResultText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。\r\n");
            SetProgressText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。");
            */

            AppendResultText("循环结束\r\n");
            SetProgressText("循环结束");

            // 2015/10/3
            // 让前端激活的任务，只执行一次。如果配置了每日激活时间，后面要再执行，除非是每日激活时间已到
            if (startinfo.Start == "activate")
                startinfo.Start = "";

            {
                Debug.Assert(this.App != null, "");

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime());  // 2007/12/17 changed // DateTime.UtcNow
                WriteLastTime(strMonitorName,
                    strLastTime);
                // string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束。共处理记录 " + nTotalRecCount.ToString() + " 个。";
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束";
                this.App.WriteErrorLog(strErrorText);
            }

            return;
        ERROR1:
            AppendResultText("ReadersMonitor thread error : " + strError + "\r\n");
            this.App.WriteErrorLog("ReadersMonitor thread error : " + strError + "\r\n");
            return;
        }

        class GetDatabaseConfigResult : NormalResult
        {
            public DatabaseConfig Config { get; set; }
        }

        GetDatabaseConfigResult GetDatabaseConfig()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(this.App._reportStorageServerName))
                errors.Add("library.xml 中尚未配置 reportStorage/@serverName 属性");
            if (string.IsNullOrEmpty(this.App._reportStorageDatabaseName))
                errors.Add("library.xml 中尚未配置 reportStorage/@databaseName 属性");
            if (string.IsNullOrEmpty(this.App._reportStorageUserId))
                errors.Add("library.xml 中尚未配置 reportStorage/@userId 属性");
            if (string.IsNullOrEmpty(this.App._reportStoragePassword))
                errors.Add("library.xml 中尚未配置 reportStorage/@password 属性");
            if (errors.Count > 0)
                return new GetDatabaseConfigResult
                {
                    Value = -1,
                    ErrorInfo = StringUtil.MakePathList(errors, "; ")
                };

            DatabaseConfig config = new DatabaseConfig
            {
                ServerName = this.App._reportStorageServerName,
                DatabaseName = this.App._reportStorageDatabaseName,
                UserName = this.App._reportStorageUserId,
                Password = this.App._reportStoragePassword,
            };
            return new GetDatabaseConfigResult { Config = config };
        }

        class LoadPlanResult : NormalResult
        {
            public XmlDocument Dom { get; set; }
        }

        LoadPlanResult LoadPlan(bool new_plan)
        {
            string filePath = Path.Combine(this.App.LogDir, "plan.xml");
            if (new_plan == false && File.Exists(filePath))
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(filePath);
                return new LoadPlanResult
                {
                    Value = 1,
                    Dom = dom
                };
            }

            Replication replication = new Replication();
            LibraryChannel channel = this.GetChannel();
            try
            {
                int nRet = replication.Initialize(channel,
                    out string strError);
                if (nRet == -1)
                    return new LoadPlanResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                nRet = replication.BuildFirstPlan("*",
                    channel,
                    (message) =>
                    {
                        AppendResultText(message + "\r\n");
                    },
                    out XmlDocument task_dom,
                    out strError);
                if (nRet == -1)
                    return new LoadPlanResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                return new LoadPlanResult
                {
                    Value = 1,
                    Dom = task_dom
                };
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        LibraryChannel GetChannel()
        {
            LibraryChannel channel = new LibraryChannel();
            channel.Url = this.App._reportReplicationServer;

            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);
            return channel;
        }

        void ReturnChannel(LibraryChannel channel)
        {
            channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            channel.Close();
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == false)
            {
                e.Cancel = true;
                return;
            }

            if (string.IsNullOrEmpty(this.App._reportReplicationServer))
                throw new Exception("library.xml 中尚未配置 reportReplication/@serverUrl 属性");

            e.UserName = this.App._reportReplicationUserName;
            e.Password = this.App._reportReplicationPassword;

            e.Parameters = "";
            e.Parameters += ",client=dp2library|" + LibraryApplication.Version;

            e.LibraryServerUrl = this.App._reportReplicationServer;
        }

        NormalResult Replication(
            DatabaseConfig config,
            ref XmlDocument task_dom,
            CancellationToken token)
        {
            Replication replication = new Replication();
            LibraryChannel channel = this.GetChannel();
            try
            {
                int nRet = replication.Initialize(channel,
    out string strError);
                if (nRet == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                nRet = replication.RunFirstPlan(
                    config,
                    channel,
                    ref task_dom,
                    (message) =>
                    {
                        AppendResultText(message + "\r\n");
                    },
                    token,
                    out strError);
                if (nRet == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                return new NormalResult();
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

    }
}
