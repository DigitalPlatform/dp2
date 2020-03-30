using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 根据日期列表获得日志记录的枚举器
    /// </summary>
    public class OperLogLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;


        List<string> m_dates = new List<string>();

        /// <summary>
        /// 日期集合。
        /// 每个日期为 8 字符数字形态
        /// 每个日期可能为特殊形态 20130101:0-100 表示从 0 开始到 100
        /// </summary>
        public List<string> Dates
        {
            get
            {
                return this.m_dates;
            }
            set
            {
#if NO
                // 检查值
                if (value != null)
                {
                    foreach (string s in value)
                    {
                        if (s.IndexOf(".") != -1)
                            throw new ArgumentException("日期字符串 '" + s + "' 中不应该出现 '.' 字符。应为 8 字符数字");
                    }
                }
#endif
                if (value == null)
                    this.m_dates = null;
                else
                    this.m_dates = CannonicalizeDateString(value);
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

        public int Level
        {
            get;
            set;
        }

        public bool ReplicationLevel
        {
            get;
            set;
        }

        /// <summary>
        /// 进度条开始位置。-1 表示从一半开始，0 表示从头开始。缺省为 0
        /// </summary>
        public long ProgressStart
        {
            get;
            set;
        }

        public ProgressEstimate Estimate
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

        // dp2library 服务器版本号。决定了是否使用获取时等待功能
        public string ServerVersion
        {
            get;
            set;
        }

        LogType _logType = LogType.OperLog;
        /// <summary>
        /// 要获取的日志的类型。注意，只能用一种类型
        /// </summary>
        public LogType LogType
        {
            get
            {
                return _logType;
            }
            set
            {
                _logType = value;
            }
        }

        // 获得日志文件中记录的总数
        // parameters:
        //      strDate 日志文件的日期，8 字符
        // return:
        //      -2  此类型的日志在 dp2library 端尚未启用
        //      -1  出错
        //      0   日志文件不存在，或者记录数为 0
        //      >0  记录数
        public static long GetOperLogCount(
            Stop stop,
            LibraryChannel channel,
            string strDate,
            LogType logType,
            out string strError)
        {
            strError = "";

            if (strDate.Length != 8)
            {
                strError = "strDate 参数值应该是 8 字符 (当前为 '" + strDate + "')";
                return -1;
            }

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            long lRecCount = 0;

            string strStyle = "getcount";
            if ((logType & LogType.AccessLog) != 0)
                strStyle += ",accessLog";

            // 获得日志文件尺寸
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围
            long lRet = channel.GetOperLog(
                stop,
                strDate + ".log",
                -1,    // lIndex,
                -1, // lHint,
                strStyle,
                "", // strFilter
                out strXml,
                out lRecCount,
                0,  // lAttachmentFragmentStart,
                0,  // nAttachmentFramengLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
            if (lRet == 0)
            {
                lRecCount = 0;
                return 0;
            }
            if (lRet != 1)
                return -1;
            if (lRecCount == -1)
            {
                strError = logType.ToString() + " 型的日志在 dp2library 中尚未启用";
                return -2;
            }
            Debug.Assert(lRecCount >= 0, "");
            return lRecCount;
        }

        // 获得一个日志文件的尺寸
        // return:
        //      -2  此类型的日志尚未启用
        //      -1  error
        //      0   file not found
        //      1   found
        static int GetFileSize(
            Stop stop,
            LibraryChannel channel,
            string strCacheDir,
            string strLogFileName,
            LogType logType,
            out long lServerFileSize,
            out long lCacheFileSize,
            out string strError)
        {
            strError = "";
            lServerFileSize = 0;
            lCacheFileSize = 0;

            string strCacheFilename = Path.Combine(strCacheDir, strLogFileName);

            FileInfo fi = new FileInfo(strCacheFilename);
            if (fi.Exists == true)
                lCacheFileSize = fi.Length;

            stop.SetMessage("正获得日志文件 " + strLogFileName + " 的尺寸...");

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            string strStyle = "level-0";
            if ((logType & LogType.AccessLog) != 0)
                strStyle += ",accessLog";

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
                strStyle,
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
            if (lServerFileSize == -1)
            {
                strError = "日志尚未启用";
                return -2;
            }
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

            string strVersionFilePath = Path.Combine(strCacheDir, strVersionFileName);
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

            string strCurrentLibraryCode = dom.DocumentElement.GetAttribute("libraryCodeList");
            string strCurrentServerUrl = dom.DocumentElement.GetAttribute("libraryServerUrl");

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

            string strVersionFilePath = Path.Combine(strCacheDir, strVersionFileName);
            try
            {
                File.Delete(strVersionFilePath);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                dom.DocumentElement.SetAttribute("libraryCodeList", strLibraryCodeList);
                dom.DocumentElement.SetAttribute("libraryServerUrl", strDp2LibraryServerUrl);
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

            if ((this.LogType & LogType.AccessLog) != 0
    && (this.LogType & LogType.OperLog) != 0)
                throw new ArgumentException("OperLogLoader 的 LogType 只能使用一种类型");

            if (string.IsNullOrEmpty(this.CacheDir) == false)
                TryCreateDir(this.CacheDir);

            // ProgressEstimate estimate = new ProgressEstimate();
            bool bAutoCache = this.AutoCache;

            if (bAutoCache == true)
            {
                // 象征性获得一个日志文件的尺寸，主要目的是为了触发一次通道登录
                // return:
                //      -2  此类型的日志尚未启用
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = GetFileSize(
                    this.Stop,
                    this.Channel,
                    this.CacheDir,
                    "20121001.log",
                    this.LogType,
                    out long lServerFileSize,
                    out long lCacheFileSize,
                    out strError);
                if (nRet == -1)
                    throw new ChannelException(this.Channel.ErrorCode, strError);
                // 2015/11/25
                if (nRet == -2)
                    yield break;    // 此类型的日志尚未启用

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
                    nRet = DeleteDataDir(
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
                                throw new InterruptException(strError);
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

                    TryCreateDir(this.CacheDir);  // 重新创建目录

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
            List<string> ranges = new List<string>();   // 日期后面的偏移部分
            List<long> sizes = new List<long>();    // 服务器日志文件的尺寸

#if NO
            this.Stop.SetMessage("正在准备获得日志文件尺寸 ...");
            foreach (string strLine in this.FileNames)
            {
                // Application.DoEvents();

                if (this.Stop != null && this.Stop.State != 0)
                {
                    strError = "用户中断";
                    throw new InterruptException(strError);
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
                //      -2  此类型的日志尚未启用
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = GetFileSize(
                    this.Stop,
                    this.Channel,
                    this.CacheDir,
                    strLogFilename,
                    this.LogType,
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
                            throw new InterruptException(strError);
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
                        throw new ChannelException(this.Channel.ErrorCode, strError);
                }

                if (nRet == 0)
                    continue;

                if (lServerFileSize == 0)
                    continue;   // 0字节的文件当作不存在处理
                // 2015/11/25
                if (lServerFileSize == -1)
                    yield break;    // 此类型的日志尚未启用

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
#endif
            // 预先处理一下文件名
            List<string> filenames = new List<string>();
            foreach (string strLine in this.Dates)
            {
                if (this.Stop != null && this.Stop.State != 0)
                {
                    strError = "用户中断";
                    throw new InterruptException(strError);
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

#if NO
                List<string> parts = StringUtil.ParseTwoPart(strFilename, ":");
                string strRange = parts[1];

                strFilename = parts[0].Substring(0, 8);

                if (string.IsNullOrEmpty(strRange) == false)
                    range_table[strFilename] = strRange;    // TODO: 如何防范文件名重复?
#endif
                filenames.Add(strFilename);
                Debug.Assert(strFilename.Length >= 8, "");
            }

            // 准备查找表
            Hashtable length_table = new Hashtable();
            {
                List<FileSize> size_pairs = GetFilesLength(
                    // filenames
                    );
                foreach (FileSize size_pair in size_pairs)
                {
                    length_table[size_pair.Date] = size_pair;
                }
            }

            foreach (string filename in filenames)
            {
                FileSize size_pair = length_table[filename.Substring(0, 8)] as FileSize;
                if (size_pair == null)
                    continue;
                Debug.Assert(size_pair != null, "");

                lines.Add(filename);
                sizes.Add(size_pair.ServerFileSize);

                if (size_pair.ServerFileSize == -1)
                    yield break;    // 此类型的日志尚未启用

                Debug.Assert(size_pair.ServerFileSize >= 0, "");

                if (bAutoCache == true)
                {
                    if (size_pair.CacheFileSize > 0)
                        lTotalSize += size_pair.CacheFileSize;
                    else
                        lTotalSize += size_pair.ServerFileSize;
                }
                else
                {
                    lTotalSize += size_pair.ServerFileSize;
                }
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

            if (Estimate != null)
            {
                Estimate.SetRange(lDoneSize, lTotalSize);
                Estimate.StartEstimate();
            }

            for (int i = 0; i < lines.Count; i++)
            {
                // Application.DoEvents();

                if (this.Stop != null && this.Stop.State != 0)
                {
                    strError = "用户中断";
                    throw new InterruptException(strError);
                    // yield break; ?
                }

                string strLine = lines[i];
                Debug.Assert(strLine.Length >= 8, "");

                // string strRange = ranges[i];

                // 2017/10/9
                List<string> parts = StringUtil.ParseTwoPart(strLine, ":");
                string strRange = parts[1];

                strLine = parts[0];

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

                {
                    OperLogItemLoader loader = new OperLogItemLoader
                    {
                        Stop = this.Stop,
                        Channel = this.Channel,
                        // loader.owner = this.owner;
                        Estimate = this.Estimate,
                        Date = strLogFilename,
                        Level = this.Level,
                        ReplicationLevel = this.ReplicationLevel,
                        lServerFileSize = sizes[i],
                        Range = strRange,
                        AutoCache = this.AutoCache,
                        lProgressValue = lDoneSize,
                        lSize = lTotalSize,
                        Filter = this.Filter,
                        LogType = this.LogType,
                        ServerVersion = this.ServerVersion,
                        CacheDir = this.CacheDir,   // 2020/3/30
                    };

                    if (this.Prompt != null)
                        loader.Prompt += new MessagePromptEventHandler(loader_Prompt);
                    try
                    {
                        foreach (OperLogItem item in loader)
                        {
                            yield return item;
                        }
                    }
                    finally
                    {
                        if (this.Prompt != null)
                            loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                    }

                    lDoneSize = loader.lProgressValue;
                    lTotalSize = loader.lSize;
                }
            }
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (this.Prompt != null)
                this.Prompt(sender, e);
        }

        // 重新用不同详细级别获取事项内容
        // parameters:
        //      item    要被重新获取内容的对象
        //      nLevel  内容详细级别。0/1/2。0 为最详细级
        public OperLogItem LoadOperLogItem(OperLogItem item, int nLevel)
        {
            OperLogItemLoader loader = new OperLogItemLoader();
            loader.Stop = this.Stop;
            loader.Channel = this.Channel;
            loader.Estimate = null;//
            loader.Date = item.Date + ".log";
            loader.Level = nLevel;
            loader.lServerFileSize = -1;
            loader.Range = item.Index.ToString() + "-" + item.Index.ToString();
            loader.AutoCache = this.AutoCache;
            loader.lProgressValue = 0;
            loader.lSize = 0;
            loader.Filter = this.Filter;
            loader.LogType = this.LogType;

            foreach (OperLogItem new_item in loader)
            {
                return new_item;
            }

            return null;
        }

        // 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
        public static bool TryCreateDir(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        public static int DeleteDataDir(
    string strDataDir,
    out string strError)
        {
            strError = "";
            try
            {
                Directory.Delete(strDataDir, true);
                return 0;
            }
            catch (Exception ex)
            {
                strError = "删除目录 '" + strDataDir + "' 时出错: " + ex.Message;
                return -1;
            }
        }

        // 根据日期范围，发生日志文件名
        // parameters:
        //      strStartDate    起始日期。8字符
        //      strEndDate  结束日期。8字符
        // return:
        //      -1  错误
        //      0   成功
        /// <summary>
        /// 根据日期范围，发生日志文件名
        /// </summary>
        /// <param name="strStartDate">起始日期。8字符</param>
        /// <param name="strEndDate">结束日期。8字符</param>
        /// <param name="bOutputExt">输出的 LogFileNames 元素中是否包含扩展名 ".log"</param>
        /// <param name="LogFileNames">返回创建的文件名</param>
        /// <param name="strWarning">返回警告信息</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int MakeLogFileNames(string strStartDate,
            string strEndDate,
            bool bOutputExt,  // 是否包含扩展名 ".log"
            out List<string> LogFileNames,
            out string strWarning,
            out string strError)
        {
            LogFileNames = new List<string>();
            strError = "";
            strWarning = "";
            int nRet = 0;

            // 2017/10/5
            if (strStartDate.IndexOf(".") != -1)
            {
                strError = "strStartDate 参数值中不能包含 . 或 .log";
                return -1;
            }
            if (strEndDate.IndexOf(".") != -1)
            {
                strError = "strEndDate 参数值中不能包含 . 或 .log";
                return -1;
            }

            if (String.Compare(strStartDate, strEndDate) > 0)
            {
                strError = "起始日期 '" + strStartDate + "' 不应大于结束日期 '" + strEndDate + "'。";
                return -1;
            }

            string strLogFileName = strStartDate;

            for (; ; )
            {
                LogFileNames.Add(strLogFileName + (bOutputExt == true ? ".log" : ""));

                string strNextLogFileName = "";
                // 获得（理论上）下一个日志文件名
                // return:
                //      -1  error
                //      0   正确
                //      1   正确，并且strLogFileName已经是今天的日子了
                nRet = NextLogFileName(strLogFileName,
                    out strNextLogFileName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    if (String.Compare(strLogFileName, strEndDate) < 0)
                    {
                        strWarning = "因日期范围的尾部 " + strEndDate + " 超过今天(" + DateTime.Now.ToLongDateString() + ")，部分日期被略去...";
                        break;
                    }
                }

                strLogFileName = strNextLogFileName;
                if (String.Compare(strLogFileName, strEndDate) > 0)
                    break;
            }

            return 0;
        }

        // 获得（理论上）下一个日志文件名
        // return:
        //      -1  error
        //      0   正确
        //      1   正确，并且strLogFileName已经是今天的日子了
        static int NextLogFileName(string strLogFileName,
            out string strNextLogFileName,
            out string strError)
        {
            strError = "";
            strNextLogFileName = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strLogFileName)
                || strLogFileName.Length < 8)
            {
                strError = "日期 '" + strLogFileName + "' 字符串格式错误，应为 8 个数字字符";
                return -1;
            }

            string strYear = strLogFileName.Substring(0, 4);
            string strMonth = strLogFileName.Substring(4, 2);
            string strDay = strLogFileName.Substring(6, 2);

            int nYear = 0;
            int nMonth = 0;
            int nDay = 0;

            try
            {
                nYear = Convert.ToInt32(strYear);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strYear + "' 部分格式错误";
                return -1;
            }

            try
            {
                nMonth = Convert.ToInt32(strMonth);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strMonth + "' 部分格式错误";
                return -1;
            }

            try
            {
                nDay = Convert.ToInt32(strDay);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strDay + "' 部分格式错误";
                return -1;
            }

            DateTime time = DateTime.Now;
            try
            {
                time = new DateTime(nYear, nMonth, nDay);
            }
            catch (Exception ex)
            {
                strError = "日期 " + strLogFileName + " 格式错误: " + ex.Message;
                return -1;
            }

            DateTime now = DateTime.Now;

            // 正规化时间
            nRet = RoundTime("day",
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = RoundTime("day",
                ref time,
                out strError);
            if (nRet == -1)
                return -1;

            bool bNow = false;
            if (time >= now)
                bNow = true;

            time = time + new TimeSpan(1, 0, 0, 0); // 后面一天

            strNextLogFileName = time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0');

            if (bNow == true)
                return 1;

            return 0;
        }

        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        /*public*/
        static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            time = time.ToLocalTime();
            if (strUnit == "day")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            time = time.ToUniversalTime();

            return 0;
        }

        // 根据 OperLogItem 对象获得相关的附件文件
        // return:
        //      -1  出错
        //      0   没有找到日志记录
        //      >0  附件总长度
        public long DownloadAttachment(OperLogItem item,
            string strCurrentFileName,
            out string strError)
        {
            long lHintNext = 0;
            // return:
            //      -1  出错
            //      0   没有找到日志记录
            //      >0  附件总长度
            long lRet = this.Channel.DownloadOperlogAttachment(
                this.Stop,   // stop,
                item.Date + ".log",
                item.Index,
                -1, // lHint,
                strCurrentFileName,
                out lHintNext,
                out strError);
            if (lRet == -1)
                return -1;

            return lRet;
        }

        class FileSize
        {
            public string Date { get; set; }
            public long ServerFileSize { get; set; }
            public long CacheFileSize { get; set; }
        }

        // this.Dates 里面有需要获得文件长度的文件名
        List<FileSize> GetFilesLength(
            // List<string> filenames
            )
        {
            List<FileSize> results = new List<FileSize>();

            OperLogDatesLoader loader = new OperLogDatesLoader();
            loader.Channel = this.Channel;
            loader.Stop = this.Stop;
            loader.LogType = this.LogType;
            if (this.Dates != null && this.Dates.Count > 0)
                loader.FilterDates = this.Dates;

            if (this.Prompt != null)
                loader.Prompt += loader_Prompt;
            try
            {
                foreach (OperLogDateItem item in loader)
                {
                    FileSize size = new FileSize();
                    size.Date = item.Date;
                    size.ServerFileSize = item.Length;
                    results.Add(size);

                    if (this.AutoCache == true && string.IsNullOrEmpty(this.CacheDir) == false)
                    {
                        string strCacheFilename = Path.Combine(this.CacheDir, item.Date + ".log");

                        FileInfo fi = new FileInfo(strCacheFilename);
                        if (fi.Exists == true)
                            size.CacheFileSize = fi.Length;
                    }
                }

                return results;
            }
            finally
            {
                if (this.Prompt != null)
                    loader.Prompt -= loader_Prompt;
            }
        }

        List<string> CannonicalizeDateString(List<string> lines)
        {
            List<string> results = new List<string>();
            foreach (string line in lines)
            {
                if (line.IndexOf(".") == -1)
                {
                    results.Add(line);
                    continue;
                }
                List<string> parts = StringUtil.ParseTwoPart(line, ":");
                string left = parts[0];
                string right = parts[1];

                List<string> temp = StringUtil.ParseTwoPart(left, ".");
                left = temp[0]; // 丢弃 '.log' 部分
                if (string.IsNullOrEmpty(right) == false)
                    results.Add(left + ":" + right);
                else
                    results.Add(left);
            }

            return results;
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

        // 2017/10/6
        /// <summary>
        /// 附件内容长度
        /// </summary>
        public long AttachmentLength = 0;

        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;

        /// <summary>
        /// 错误信息字符串
        /// </summary>
        public string ErrorInfo = "";
    }

    [Flags]
    public enum LogType
    {
        OperLog = 0x01,     // 操作日志
        AccessLog = 0x02,   // 只读日志
    }
}
