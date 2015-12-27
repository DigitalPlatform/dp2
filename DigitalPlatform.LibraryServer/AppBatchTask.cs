using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和批处理任务相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 从断点记忆文件中读出信息
        // return:
        //      -1  error
        //      0   file not found
        //      1   found
        public int ReadBatchTaskBreakPointFile(string strTaskName,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            string strFileName = this.LogDir + "\\" + strTaskName.Replace(" ", "_") + ".breakpoint";

            try
            {
                using (StreamReader sr = new StreamReader(strFileName, Encoding.UTF8))
                {
                    sr.ReadLine();  // 读入时间行
                    strText = sr.ReadToEnd();// 读入其余
                }
            }
            catch (FileNotFoundException /*ex*/)
            {
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "open file '" +strFileName+ "' error : " + ex.Message;
                return -1;
            }

            return 1;
        }

        // 写入断点记忆文件
        // parameters:
        //      strTaskName 任务名。允许出现空格字符
        public void WriteBatchTaskBreakPointFile(string strTaskName,
            string strText)
        {
            string strFileName = this.LogDir + "\\" + strTaskName.Replace(" ", "_") + ".breakpoint";
            string strTime = DateTime.Now.ToString();

            // 删除原来的文件
            try
            {
                File.Delete(strFileName);
            }
            catch
            {
            }

            // 写入新内容
            StreamUtil.WriteText(strFileName,
                strTime + "\r\n");
            StreamUtil.WriteText(strFileName,
                strText);
        }

        // 删除断点文件
        public void RemoveBatchTaskBreakPointFile(string strTaskName)
        {
            string strFileName = this.LogDir + "\\" + strTaskName.Replace(" ", "_") + ".breakpoint";
            try
            {
                File.Delete(strFileName);
            }
            catch
            {
            }
        }

        public void StopAllBatchTasks()
        {
            for (int i = 0; i < this.BatchTasks.Count; i++)
            {
                BatchTask task = this.BatchTasks[i];
                task.Stop();
            }
        }

        // 获得任务当前信息
        // 多线程：安全
        public BatchTaskInfo GetTaskInfo(string strText)
        {
            BatchTaskInfo info = new BatchTaskInfo();
            info.Name = "";
            info.State = "";

            info.ProgressText = strText;
            info.ResultText = null;
            info.ResultOffset = 0;
            info.ResultTotalLength = 0;
            info.ResultVersion = 0;

            return info;
        }

        // 按照命令启动一个批处理任务(不是自动启动)
        // return:
        //      -1  出错
        //      0   启动成功
        //      1   调用前任务已经处于执行状态，本次调用激活了这个任务
        public int StartBatchTask(string strName,
            BatchTaskInfo param,
            out BatchTaskInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            if (strName == "!continue")
            {
                this.PauseBatchTask = false;

                // 2013/11/23
                foreach (BatchTask current_task in this.BatchTasks)
                {
                    current_task.Activate();
                }

                info = GetTaskInfo("全部批处理任务已经解除暂停");
                return 1;
            }

            // 2007/12/18
            if (this.HangupReason == HangupReason.LogRecover)
            {
                strError = "当前系统正处在LogRecover挂起状态，无法启动新的批处理任务";
                return -1;
            }

            // 2012/2/4
            if (this.PauseBatchTask == true)
            {
                strError = "当前所有批处理任务均处在暂停状态，无法启动新的批处理任务";
                return -1;
            }

            if (this.BatchTasks == null)
            {
                strError = "this.AppTasks == null";
                return -1;
            }

            BatchTask task = this.BatchTasks.GetBatchTask(strName);

            // 创建新的任务
            if (task == null)
            {
                if (strName == "预约到书管理")
                    task = new ArriveMonitor(this, strName);
                else if (strName == "日志恢复")
                    task = new OperLogRecover(this, strName);
                else if (strName == "dp2Library 同步")
                    task = new LibraryReplication(this, strName);
                else if (strName == "重建检索点")
                    task = new RebuildKeys(this, strName);
                /*
            else if (strName == "跟踪DTLP数据库")
                task = new TraceDTLP(this, strName);
                 * */
                else if (strName == "正元一卡通读者信息同步")
                    task = new ZhengyuanReplication(this, strName);
                else if (strName == "迪科远望一卡通读者信息同步")
                    task = new DkywReplication(this, strName);
                else if (strName == "读者信息同步")
                    task = new PatronReplication(this, strName);
                else if (strName == "超期通知")
                    task = new ReadersMonitor(this, strName);
                else if (strName == "消息监控")
                    task = new MessageMonitor(this, strName);
                else if (strName == "创建 MongoDB 日志库")
                    task = new BuildMongoOperDatabase(this, strName);
                else
                {
                    strError = "系统不能识别任务名 '" + strName + "'";
                    return -1;
                }

                try
                {
                    this.BatchTasks.Add(task);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }
            else
            {
                bool bOldStoppedValue = task.Stopped;


                if (bOldStoppedValue == false)
                {
                    if (strName == "重建检索点")
                    {
                        task.StartInfos.Add(param.StartInfo);

                        task.AppendResultText("新任务已加入等待队列：\r\n---\r\n" + RebuildKeys.GetSummary(param.StartInfo) + "\r\n---\r\n\r\n");
                    }

                    else
                    {
                        // 尽量采用前端发来的参数进行运行
                        task.StartInfo = param.StartInfo;
                    }

                    // 激活 2007/10/10
                    task.eventActive.Set();
                    task.ManualStart = true;    // 表示为命令启动

                    strError = "任务 " + task.Name + " 已经在运行中，不能重复启动。本次操作激活了这个任务。";
                    return 1;
                }
            }

            // 执行日志恢复任务前，需要先中断正在执行的其他任何任务
            // TODO: 日志恢复 任务结束后，原先中断的那些任务并不会自动去启动。需要系统管理员手动重新启动一次Application
            if (strName == "日志恢复")
            {
                StopAllBatchTasks();
            }

            task.ManualStart = true;    // 表示为命令启动
            task.StartInfo = param.StartInfo;
            task.ClearProgressFile();   // 清除进度文件内容
            task.StartWorkerThread();

            /*
            // 激活 2007/10/10
            task.eventActive.Set();
             * */

            info = task.GetCurrentInfo(param.ResultOffset,
                param.MaxResultBytes);

            return 0;
        }

        public int StopBatchTask(string strName,
            BatchTaskInfo param,
            out BatchTaskInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            if (strName == "!pause")
            {
                this.PauseBatchTask = true;
                info = GetTaskInfo("全部批处理任务已经被暂停");
                return 1;
            }

            BatchTask task = this.BatchTasks.GetBatchTask(strName);

            // 任务本来就不存在
            if (task == null)
            {
                strError = "任务 '" + strName + "' 不存在";
                return -1;
            }

            task.Stop();

            info = task.GetCurrentInfo(param.ResultOffset,
                param.MaxResultBytes);

            return 1;
        }

        public int GetBatchTaskInfo(string strName,
            BatchTaskInfo param,
            out BatchTaskInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            BatchTask task = this.BatchTasks.GetBatchTask(strName);

            // 任务本来就不存在
            if (task == null)
            {
                strError = "任务 '" + strName + "' 不存在";
                return -1;
            }

            info = task.GetCurrentInfo(param.ResultOffset,
                param.MaxResultBytes);

            return 1;
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class BatchTaskStartInfo
    {
        // 启动、停止一般参数
        [DataMember]
        public string Param = "";   // 格式一般为XML

        // 专门参数
        [DataMember]
        public string BreakPoint = ""; // 断点  格式为 序号@文件名
        [DataMember]
        public string Start = ""; // 起点  格式为 序号@文件名
        [DataMember]
        public string Count = ""; // 个数 纯数字

        public override string ToString()
        {
            Hashtable table = new Hashtable();
            if (string.IsNullOrEmpty(this.Param) == false)
                table["Param"] = this.Param;
            if (string.IsNullOrEmpty(this.BreakPoint) == false)
                table["BreakPoint"] = this.BreakPoint;
            if (string.IsNullOrEmpty(this.Start) == false)
                table["Start"] = this.Start;
            if (string.IsNullOrEmpty(this.Count) == false)
                table["Count"] = this.Count;

            return StringUtil.BuildParameterString(table, ',', ':');
        }

        public static BatchTaskStartInfo FromString(string strText)
        {
            BatchTaskStartInfo info = new BatchTaskStartInfo();
            Hashtable table = StringUtil.ParseParameters(strText, ',', ':');
            info.Param = (string)table["Param"] ;
            info.BreakPoint = (string)table["BreakPoint"];
            info.Start = (string)table["Start"];
            info.Count = (string)table["Count"];
            return info;
        }
    }

    // 批处理任务信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class BatchTaskInfo
    {
        // 名字
        [DataMember]
        public string Name = "";

        // 状态
        [DataMember]
        public string State = "";

        // 当前进度
        [DataMember]
        public string ProgressText = "";

        // 输出结果
        [DataMember]
        public int MaxResultBytes = 0;
        [DataMember]
        public byte[] ResultText = null;
        [DataMember]
        public long ResultOffset = 0;   // 本次获得到ResultText达的末尾点
        [DataMember]
        public long ResultTotalLength = 0;  // 整个结果文件的长度

        [DataMember]
        public BatchTaskStartInfo StartInfo = null;

        [DataMember]
        public long ResultVersion = 0;  // 信息文件版本
    }
}
