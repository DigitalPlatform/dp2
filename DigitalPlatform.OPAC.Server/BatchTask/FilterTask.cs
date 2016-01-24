using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Server
{
    // 一个后台执行的剖析任务
    public class FilterTask
    {
        public List<NodeInfo> ResultItems = null;
        public TaskState TaskState = TaskState.Processing;
        public long ProgressRange = 0;
        public long ProgressValue = 0;
        public long HitCount = 0;   // 原始结果集中的记录总数
        public string ErrorInfo = "";

        // 对象创建时间
        public DateTime CreateTime = DateTime.Now;
        // 最近使用过的时间
        public DateTime LastUsedTime = DateTime.Now;

        public void Touch()
        {
            this.LastUsedTime = DateTime.Now;
        }

        // 删除所有结果集文件
        public void DeleteTempFiles(string strTempDir)
        {
            if (this.ResultItems != null)
            {
                foreach (NodeInfo info in this.ResultItems)
                {
                    if (string.IsNullOrEmpty(info.ResultSetPureName) == false)
                    {
                        try
                        {
                            string strFileName = Path.Combine(strTempDir, info.ResultSetPureName);
                            File.Delete(strFileName);
                            File.Delete(strFileName + ".index");
                        }
                        catch
                        {
                        }
                    }

                    if (string.IsNullOrEmpty(info.SubNodePureName) == false)
                    {
                        try
                        {
                            string strFileName = Path.Combine(strTempDir, info.SubNodePureName);
                            File.Delete(strFileName);
                            File.Delete(strFileName + ".index");
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public void _SetProgress(long lProgressRange, long lProgressValue)
        {
            this.ProgressRange = lProgressRange;
            this.ProgressValue = lProgressValue;
        }

        public void ThreadPoolCallBack(object context)
        {
            FilterTaskInput input = (FilterTaskInput)context;

            Hashtable result_table = null;
            string strError = "";

#if NO
            // 临时的SessionInfo对象
            SessionInfo session = new SessionInfo(input.App);
            session.UserID = input.App.ManagerUserName;
            session.Password = input.App.ManagerPassword;
            session.IsReader = false;
#endif
            LibraryChannel channel = input.App.GetChannel();

            try
            {
                long lHitCount = 0;
                int nRet = ResultsetFilter.DoFilter(
                    input.App,
                    channel,    // session.Channel,
                    input.ResultSetName,
                    input.FilterFileName,
                    input.MaxCount,
                    _SetProgress,
                    ref result_table,
                    out lHitCount,
                    out strError);
                if (nRet == -1)
                {
                    this.ErrorInfo = strError;
                    this.TaskState = TaskState.Done;
                    return;
                }

                this.HitCount = lHitCount;

                // 继续加工
                List<NodeInfo> output_items = null;
                nRet = ResultsetFilter.BuildResultsetFile(result_table,
                    input.DefDom,
                    // input.aggregation_names,
                    input.TempDir,  // input.SessionInfo.GetTempDir(),
                    out output_items,
                    out strError);
                if (nRet == -1)
                {
                    this.ErrorInfo = strError;
                    this.TaskState = TaskState.Done;
                    return;
                }

                {
                    this.ResultItems = output_items;
                    this.TaskState = TaskState.Done;
                }

                if (input.ShareResultSet == true)
                {
                    // 删除全局结果集对象
                    // 管理结果集
                    // parameters:
                    //      strAction   share/remove 分别表示共享为全局结果集对象/删除全局结果集对象
                    long lRet = // session.Channel.
                        channel.ManageSearchResult(
                        null,
                        "remove",
                        "",
                        input.ResultSetName,
                        out strError);
                    if (lRet == -1)
                        this.ErrorInfo = strError;

                    input.ShareResultSet = false;
                    input.ResultSetName = "";
                }
            }
            finally
            {
#if NO
                session.CloseSession();
#endif
                input.App.ReturnChannel(channel);
            }

#if NO
            if (input.SessionInfo != null && string.IsNullOrEmpty(input.TaskName) == false)
                input.SessionInfo.SetFilterTask(input.TaskName, this);
#endif
            input.App.SetFilterTask(input.TaskName, this);
        }
    }

    public class FilterTaskInput
    {
        public OpacApplication App = null;

        public string ResultSetName = "";   // 全局结果集的名字。第一字符 '#' 被省略了
        public bool ShareResultSet = false;   // 结果集是否被 Share 过。如果是，最后不要忘记 Remove

        public string FilterFileName = "";
        // public SessionInfo SessionInfo = null;  // 何时释放? 可能引起 leak
        public string TaskName = "";
        public XmlDocument DefDom = null;   // facetdef.xml
        public int MaxCount = 1000;   // 剖析的最多记录数

        public string TempDir = "";
    }

    public enum TaskState
    {
        Processing = 0,
        Done = 1,
    }

}
