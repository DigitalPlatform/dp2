using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    // 全局信息
    public partial class KernelApplication
    {
        public bool PauseBatchTask = false;

        public BatchTaskCollection BatchTasks = new BatchTaskCollection();
        public HangupReason HangupReason = HangupReason.None;


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
                strError = "open file '" + strFileName + "' error : " + ex.Message;
                return -1;
            }
            return 1;
        }

        // 写入断点记忆文件
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

        // 停止所有批处理任务
        public void StopAllBatchTasks()
        {
            for (int i = 0; i < this.BatchTasks.Count; i++)
            {
                BatchTask task = this.BatchTasks[i];
                task.Stop();
            }
        }

#if NO
        // 获得任务当前信息
        // 多线程：安全
        public TaskInfo GetTaskInfo(string strText)
        {
            TaskInfo info = new TaskInfo();
            info.Name = "";
            info.State = "";

            info.ProgressText = strText;
            info.ResultText = null;
            info.ResultOffset = 0;
            info.ResultTotalLength = 0;
            info.ResultVersion = 0;

            return info;
        }
#endif

        // 按照命令启动一个批处理任务(不是自动启动)
        public int StartBatchTask(string strName,
            TaskInfo param,
            out TaskInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            if (strName == "!continue")
            {
                this.PauseBatchTask = false;
                return 0;
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

            List<BatchTask> tasks = this.BatchTasks.GetBatchTask(strName);
            if (tasks == null || tasks.Count == 0)
            {
                // 创建新的任务
                BatchTask task = null;

                if (string.IsNullOrEmpty(strName) == false && strName[0] == '#')
                {
                    strError = "不允许用 '#' 方式启动任务";
                    return -1;
                }

                if (strName == "预约到书管理")
                    task = new RebuildKeysTask(this, strName);
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

                tasks.Add(task);
            }

            // BatchTask task = this.BatchTasks.GetBatchTask(strName);
            else
            {
                string strNameString = "";
                foreach (BatchTask task in tasks)
                {
                    bool bOldStoppedValue = task.Stopped;

                    if (bOldStoppedValue == false)
                    {
                        // 激活 2007/10/10
                        task.eventActive.Set();
                        task.ManualStart = true;    // 表示为命令启动

                        if (string.IsNullOrEmpty(strNameString) == false)
                            strNameString += ",";
                        strNameString += task.Name + "(ID=" + task.ID + ")";
                    }
                }

                if (string.IsNullOrEmpty(strNameString) == false)
                {
                    strError = "任务 " + strNameString + " 已经在运行中，不能重复启动。本次操作激活了这些(个)任务。";
                    return -1;
                }
            }

            {
                BatchTask task = tasks[0];

                task.ManualStart = true;    // 表示为命令启动
                task.StartInfo = param.StartInfo;
                task.ClearProgressFile();   // 清除进度文件内容
                task.StartWorkerThread();

                info = task.GetCurrentInfo(param.ResultOffset,
                    param.MaxResultBytes);
            }

            return 0;
        }

        public int StopBatchTask(string strName,
            TaskInfo param,
            out TaskInfo[] results,
            out string strError)
        {
            strError = "";
            results = null;

            if (strName == "!pause")
            {
                this.PauseBatchTask = true;
                return 0;
            }

            List<BatchTask> tasks = this.BatchTasks.GetBatchTask(strName);

            // 任务本来就不存在
            if (tasks == null || tasks.Count == 0)
            {
                strError = "任务 '" + strName + "' 不存在";
                return -1;
            }

            List<TaskInfo> infos = new List<TaskInfo>();
            foreach (BatchTask task in tasks)
            {
                task.Stop();

                infos.Add(task.GetCurrentInfo(param.ResultOffset,
                    param.MaxResultBytes));
            }

            results = new TaskInfo[infos.Count];
            infos.CopyTo(results);

            return results.Length;
        }

        // 2013/3/13
        // 列出当前存在的任务信息
        public int ListBatchTasks(
            out TaskInfo[] results,
            out string strError)
        {
            strError = "";
            results = null;

            List<TaskInfo> list = new List<TaskInfo>();
            foreach (BatchTask task in this.BatchTasks)
            {
                TaskInfo info = new TaskInfo();
                info.Name = task.Name;
                info.ID = task.ID;
                info.State = task.Stopped == true ? "stop" : "run";
                list.Add(info);
            }

            results = new TaskInfo[list.Count];
            list.CopyTo(results);
            return 0;
        }

        // parameters:
        //      strName 任务名。如果用 '#' 开头，表示用 ID 获取任务
        public int GetBatchTaskInfo(string strName,
            TaskInfo param,
            out TaskInfo[] results,
            out string strError)
        {
            strError = "";
            results = null;

            List<BatchTask> tasks = this.BatchTasks.GetBatchTask(strName);

            // 任务本来就不存在
            if (tasks == null || tasks.Count == 0)
            {
                strError = "任务 '" + strName + "' 不存在";
                return -1;
            }

            if (tasks.Count == 0)
            {
                strError = "任务 '" + strName + "' 不存在";
                results = new TaskInfo[0];
                return 0;
            }

            List<TaskInfo> infos = new List<TaskInfo>();
            foreach (BatchTask task in tasks)
            {
                TaskInfo info = task.GetCurrentInfo(param.ResultOffset,
                    param.MaxResultBytes);
                infos.Add(info);
            }

            results = new TaskInfo[infos.Count];
            infos.CopyTo(results);

            return results.Length;
        }
    }

    // 系统挂起的理由
    public enum HangupReason
    {
        None = 0,   // 没有挂起
        LogRecover = 1, // 日志恢复
        Backup = 2, // 大备份
        Normal = 3, // 普通维护
        OperLogError = 4,   // 操作日志错误（例如日志空间满）
    }
}
