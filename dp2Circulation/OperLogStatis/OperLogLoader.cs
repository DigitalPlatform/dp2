using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.CirculationClient;
using DigitalPlatform;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    public class OperLogLoader : IEnumerable 
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;


        List<string> m_filenames = new List<string>();

        /// <summary>
        /// 文件名集合。
        /// 每个文件名可能为特殊形态 20130101:0-100 表示从 0 开始到 100
        /// </summary>
        public List<string> FileNames
        {
            get
            {
                return this.m_filenames;
            }
            set
            {
                this.m_filenames = value;
            }
        }

        /// <summary>
        /// 缓存文件的目录
        /// </summary>
        public string CacheDir
        {
            get;
            set;
        }

        /// <summary>
        /// 是否自动进行缓存
        /// </summary>
        public bool AutoCache
        {
            get;
            set;
        }

        public int nLevel
        {
            get;
            set;
        }

#if NO
        public IWin32Window owner
        {
            get;
            set;
        }
#endif

        /// <summary>
        /// 进度条开始位置。-1 表示从一半开始，0 表示从头开始。缺省为 0
        /// </summary>
        public long ProgressStart
        {
            get;
            set;
        }

        

        public ProgressEstimate estimate
        {
            get;
            set;
        }

        /// <summary>
        /// 要选取的 operation 列表。
        /// 例如 "SetBiblioInfo,setReaderInfo"
        /// </summary>
        public string Filter
        {
            get;
            set;
        }

        public string Format
        {
            get;
            set;
        }

        public LibraryChannel Channel
        {
            get;
            set;
        }

        public Stop Stop
        {
            get;
            set;
        }

        // 获得一个日志文件的尺寸
        // return:
        //      -1  error
        //      0   file not found
        //      1   found
        static int GetFileSize(
            Stop stop,
            LibraryChannel channel,
            string strCacheDir,
            string strLogFileName,
            out long lServerFileSize,
            out long lCacheFileSize,
            out string strError)
        {
            strError = "";
            lServerFileSize = 0;
            lCacheFileSize = 0;

            string strCacheFilename = PathUtil.MergePath(strCacheDir, strLogFileName);

            FileInfo fi = new FileInfo(strCacheFilename);
            if (fi.Exists == true)
                lCacheFileSize = fi.Length;

            stop.SetMessage("正获得日志文件 " + strLogFileName + " 的尺寸...");

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            // 获得日志文件尺寸
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围
            long lRet = channel.GetOperLog(
                stop,
                strLogFileName,
                -1,    // lIndex,
                -1, // lHint,
                "level-0",
                        "", // strFilter
                out strXml,
                out lServerFileSize,
                0,  // lAttachmentFragmentStart,
                0,  // nAttachmentFramengLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
            if (lRet == 0)
            {
                lServerFileSize = 0;
                Debug.Assert(lServerFileSize == 0, "");
                return 0;
            }
            if (lRet != 1)
                return -1;
            Debug.Assert(lServerFileSize >= 0, "");

            return 1;
        }

        // 检查日志文件缓存目录的版本是否和当前用户的信息一致
        // return:
        //      -1  出错
        //      0   一致
        //      1   不一致
        static int DetectCacheVersionFile(
    string strCacheDir,
    string strVersionFileName,
    string strLibraryCodeList,
    string strDp2LibraryServerUrl,
    out string strError)
        {
            strError = "";

            string strVersionFilePath = PathUtil.MergePath(strCacheDir, strVersionFileName);
            if (File.Exists(strVersionFilePath) == false)
                return 1;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strVersionFilePath);
            }
            catch (Exception ex)
            {
                strError = "创建日志缓存版本文件 '" + strVersionFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            string strCurrentLibraryCode = DomUtil.GetAttr(dom.DocumentElement, "libraryCodeList");
            string strCurrentServerUrl = DomUtil.GetAttr(dom.DocumentElement, "libraryServerUrl");

            if (strLibraryCodeList != strCurrentLibraryCode
                || strCurrentServerUrl != strDp2LibraryServerUrl)
                return 1;

            return 0;
        }

        // 创建表示缓存版本的文件
        // 记载了当前用户管辖的馆代码，dp2Library服务器的地址
        static int CreateCacheVersionFile(
            string strCacheDir,
            string strVersionFileName,
            string strLibraryCodeList,
            string strDp2LibraryServerUrl,
            out string strError)
        {
            strError = "";

            string strVersionFilePath = PathUtil.MergePath(strCacheDir, strVersionFileName);
            try
            {
                File.Delete(strVersionFilePath);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                DomUtil.SetAttr(dom.DocumentElement, "libraryCodeList", strLibraryCodeList);
                DomUtil.SetAttr(dom.DocumentElement, "libraryServerUrl", strDp2LibraryServerUrl);
                dom.Save(strVersionFilePath);
            }
            catch (Exception ex)
            {
                strError = "创建日志缓存版本文件 '" + strVersionFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(this.CacheDir) == false)
                PathUtil.CreateDirIfNeed(this.CacheDir);

            // ProgressEstimate estimate = new ProgressEstimate();
            bool bAutoCache = this.AutoCache;

            if (bAutoCache == true)
            {
                long lServerFileSize = 0;
                long lCacheFileSize = 0;
                // 象征性获得一个日志文件的尺寸，主要目的是为了触发一次通道登录
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = GetFileSize(
                    this.Stop,
                    this.Channel,
                    this.CacheDir,
                    "20121001.log",
                    out lServerFileSize,
                    out lCacheFileSize,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                // 检查日志文件缓存目录的版本是否和当前用户的信息一致
                // return:
                //      -1  出错
                //      0   一致
                //      1   不一致
                nRet = DetectCacheVersionFile(
                    this.CacheDir,
                    "version.xml",
                    this.Channel.LibraryCodeList,
                    this.Channel.Url,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
                if (nRet == 1)
                {
                REDO:
                    // 清空当前缓存目录
                    nRet = Global.DeleteDataDir(
                        null, // owner,
                        this.CacheDir,
                        out strError);
                    if (nRet == -1)
                    {
                        if (this.Prompt != null)
                        {
                            MessagePromptEventArgs e = new MessagePromptEventArgs();
                            e.MessageText = "清空当前缓存目录时发生错误： " + strError;
                            e.Actions = "yes,no,cancel";
                            this.Prompt(this, e);
                            if (e.ResultAction == "cancel")
                                throw new Exception(strError);
                            else if (e.ResultAction == "yes")
                                goto REDO;
                            else
                            {
                            }
                        }
                        else
                            throw new Exception(strError);
                    }
#if NO
                    if (nRet == -1)
                        throw new Exception(strError);
#endif

                    PathUtil.CreateDirIfNeed(this.CacheDir);  // 重新创建目录

                    // 创建版本文件
                    nRet = CreateCacheVersionFile(
                        this.CacheDir,
                        "version.xml",
                        this.Channel.LibraryCodeList,
                        this.Channel.Url,
                        out strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                }

            }

            long lTotalSize = 0;
            List<string> lines = new List<string>();    // 经过处理后排除了不存在的文件名
            List<string> ranges = new List<string>();
            List<long> sizes = new List<long>();
            this.Stop.SetMessage("正在准备获得日志文件尺寸 ...");
            foreach (string strLine in this.FileNames)
            {
                Application.DoEvents();

                if (this.Stop != null && this.Stop.State != 0)
                {
                    strError = "用户中断";
                    throw new Exception(strError);
                    // yield break; ?
                }

                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                string strFilename = strLine.Trim();
                // 去掉注释
                nRet = strFilename.IndexOf("#");
                if (nRet != -1)
                    strFilename = strFilename.Substring(0, nRet).Trim();

                if (String.IsNullOrEmpty(strFilename) == true)
                    continue;

                string strLogFilename = "";
                string strRange = "";

                nRet = strFilename.IndexOf(":");
                if (nRet != -1)
                {
                    strLogFilename = strFilename.Substring(0, nRet).Trim();
                    strRange = strFilename.Substring(nRet + 1).Trim();
                }
                else
                {
                    strLogFilename = strFilename.Trim();
                    strRange = "";
                }

                if (strLogFilename.Length == 8)
                    strLogFilename += ".log";
            REDO_GETSIZE:
                long lServerFileSize = 0;
                long lCacheFileSize = 0;
                // 获得一个日志文件的尺寸
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = GetFileSize(
                    this.Stop,
                    this.Channel,
                    this.CacheDir,
                    strLogFilename,
                    out lServerFileSize,
                    out lCacheFileSize,
                    out strError);
                if (nRet == -1)
                {
                    // throw new Exception(strError);

                    if (this.Prompt != null)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = "获取日志文件 " + strLogFilename + " 尺寸的操作发生错误： " + strError;
                        e.Actions = "yes,no,cancel";
                        this.Prompt(this, e);
                        if (e.ResultAction == "cancel")
                            throw new Exception(strError);
                        else if (e.ResultAction == "yes")
                        {
                            if (this.Stop != null)
                                this.Stop.Continue();
                            goto REDO_GETSIZE;
                        }
                        else
                        {
                            // ??
                            continue;
                        }
                    }
                    else
                        throw new Exception(strError);
                }

                if (nRet == 0)
                    continue;

                if (lServerFileSize == 0)
                    continue;   // 0字节的文件当作不存在处理

                Debug.Assert(lServerFileSize >= 0, "");

                if (bAutoCache == true)
                {
                    if (lCacheFileSize > 0)
                        lTotalSize += lCacheFileSize;
                    else
                        lTotalSize += lServerFileSize;
                }
                else
                {
                    lTotalSize += lServerFileSize;
                }

                // lines.Add(strFilename);
                lines.Add(strLogFilename);
                ranges.Add(strRange);

                // 记忆每个文件的尺寸，后面就不用获取了?
                sizes.Add(lServerFileSize);
            }

            long lDoneSize = 0;

            if (this.ProgressStart == -1)
                lDoneSize = lTotalSize;
            else
                lDoneSize = this.ProgressStart;

            lTotalSize += lDoneSize;

            if (this.Stop != null)
            {
                this.Stop.SetProgressRange(0, lTotalSize);
            }

            estimate.SetRange(lDoneSize, lTotalSize);
            estimate.StartEstimate();

            for (int i = 0; i < lines.Count; i++)
            {
                Application.DoEvents();

                if (this.Stop != null && this.Stop.State != 0)
                {
                    strError = "用户中断";
                    throw new Exception(strError);
                    // yield break; ?
                }

                string strLine = lines[i];
                string strRange = ranges[i];

                string strLogFilename = strLine;
#if NO
                string strLogFilename = "";
                string strRange = "";

                nRet = strLine.IndexOf(":");
                if (nRet != -1)
                {
                    strLogFilename = strLine.Substring(0, nRet).Trim();
                    strRange = strLine.Substring(nRet + 1).Trim();
                }
                else
                {
                    strLogFilename = strLine.Trim();
                    strRange = "";
                }
#endif

                // begin
                {
                    OperLogItemLoader loader = new OperLogItemLoader();
                    loader.Stop = this.Stop;
                    loader.Channel = this.Channel;
                    // loader.owner = this.owner;
                    loader.estimate = this.estimate;
                    loader.FileName = strLogFilename;
                    loader.nLevel = this.nLevel;
                    loader.lServerFileSize = sizes[i];
                    loader.Range = strRange;
                    loader.AutoCache = this.AutoCache;
                    loader.lProgressValue = lDoneSize;
                    loader.lSize = lTotalSize;
                    loader.Filter = this.Filter;

                    loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                    loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                    foreach (OperLogItem item in loader)
                    {
                        yield return item;
                    }

                    lDoneSize = loader.lProgressValue;
                    lTotalSize = loader.lSize;
                }
                // end

            }



        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (this.Prompt != null)
                this.Prompt(sender, e);
        }



    }

    /// <summary>
    /// 日志信息事项
    /// </summary>
    public class OperLogItem
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Date = "";
        /// <summary>
        /// 日志记录内容
        /// </summary>
        public string Xml = "";
        /// <summary>
        /// 日志记录在文件中的序号
        /// </summary>
        public long Index = -1;
        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;
        /// <summary>
        /// 错误信息字符串
        /// </summary>
        public string ErrorInfo = "";
    }

}
