using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;
using DigitalPlatform.Text;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 在一个日志文件中，枚举每个日志记录事项的枚举器
    /// </summary>
    public class OperLogItemLoader
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

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

        // dp2library 服务器版本号。决定了是否使用获取时等待功能
        public string ServerVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 日志日期。
        /// 这是纯粹的日期，没有冒号后面的范围部分。范围部分要放入 Range 中
        /// </summary>
        public string Date
        {
            get;
            set;
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

#if NO
        public IWin32Window owner
        {
            get;
            set;
        }
#endif

        public ProgressEstimate Estimate
        {
            get;
            set;
        }

        public long lProgressValue
        {
            get;
            set;
        }

        public long lSize
        {
            get;
            set;
        }

        public long lServerFileSize
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        // 是否为同步级别？同步级别可以获得日志记录中的密码字段内容
        // 注：当前账户中还应该包含 replicatoin 权限才能真正获得日志记录中的密码字段
        public bool ReplicationLevel
        {
            get;
            set;
        }

        public string Range
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

        static int PrepareCacheFile(
    string strCacheDir,
    string strDate,
    long lServerFileSize,
    out bool bCacheFileExist,
    out Stream stream,
    out string strError)
        {
            strError = "";
            stream = null;
            // 观察本地缓存文件是否存在
            bCacheFileExist = false;
            XmlDocument metadata_dom = new XmlDocument();

            if (strDate.Length != 8)
            {
                strError = "strDate 参数值的长度应该是 8 字符";
                return -1;
            }

            string strLogFileName = strDate + ".log";

            string strCacheFilename = Path.Combine(strCacheDir, strLogFileName);
            string strCacheMetaDataFilename = Path.Combine(strCacheDir, strLogFileName + ".meta");

            if (File.Exists(strCacheFilename) == true
                && File.Exists(strCacheMetaDataFilename) == true)
            {
                bCacheFileExist = true;

                // 观察metadata
                try
                {
                    metadata_dom.Load(strCacheMetaDataFilename);
                }
                catch (FileNotFoundException)
                {
                    bCacheFileExist = false;    // 虽然数据文件存在，也需要重新获取
                }
                catch (Exception ex)
                {
                    strError = "装载 metadata 文件 '" + strCacheMetaDataFilename + "' 时出错: " + ex.Message;
                    return -1;
                }

                // 对比文件尺寸
                string strFileSize = metadata_dom.DocumentElement.GetAttribute("serverFileSize");
                if (string.IsNullOrEmpty(strFileSize) == true)
                {
                    strError = "metadata中缺乏fileSize属性";
                    return -1;
                }
                long lTempFileSize = 0;
                if (Int64.TryParse(strFileSize, out lTempFileSize) == false)
                {
                    strError = "metadata中缺乏fileSize属性值 '" + strFileSize + "' 格式错误";
                    return -1;
                }

                if (lTempFileSize != lServerFileSize)
                    bCacheFileExist = false;

            }
            // 如果文件存在，就打开，如果文件不存在，就创建一个新的
            stream = File.Open(
strCacheFilename,
FileMode.OpenOrCreate,
FileAccess.ReadWrite,
FileShare.ReadWrite);

            if (bCacheFileExist == false)
                stream.SetLength(0);

            return 0;
        }

        // 根据记录编号，定位到记录起始位置
        // parameters:
        // return:
        //      -1  error
        //      0   成功
        //      1   到达文件末尾或者超出
        static int LocationRecord(Stream stream,
            long lIndex,
            out string strError)
        {
            strError = "";

            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            for (long i = 0; i < lIndex; i++)
            {
                byte[] length = new byte[8];

                int nRet = stream.Read(length, 0, 8);
                if (nRet < 8)
                {
                    strError = "起始位置不正确";
                    return -1;
                }

                Int64 lLength = BitConverter.ToInt64(length, 0);

                stream.Seek(lLength, SeekOrigin.Current);
            }

            if (stream.Position >= stream.Length)
                return 1;
            return 0;
        }

        // 将日志写入文件
        // 不处理异常
        static void WriteCachedEnventLog(
            Stream stream,
            string strXmlBody,
            long lAttachmentLength)
        {
            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            // 清空
            for (int i = 0; i < length.Length; i++)
            {
                length[i] = 0;
            }

            stream.Write(length, 0, 8);	// 临时写点数据,占据记录总长度位置

            if (string.IsNullOrEmpty(strXmlBody) == false)
            {
                // 写入xml事项
                WriteCachedEntry(
                    stream,
                    strXmlBody,
                    lAttachmentLength);
            }

            long lRecordLength = stream.Position - lStart - 8;

            // 写入记录总长度
            if (stream.Position != lStart)
                stream.Seek(lStart, SeekOrigin.Begin);

            length = BitConverter.GetBytes((long)lRecordLength);

            stream.Write(length, 0, 8);

            // 迫使写入物理文件
            stream.Flush();

            // 文件指针回到末尾位置
            stream.Seek(lRecordLength, SeekOrigin.Current);
        }

        // 写入一个事项(string类型)
        static void WriteCachedEntry(
            Stream stream,
            string strBody,
            long lAttachmentLength)
        {
            byte[] length = new byte[8];

            // 记忆起始位置
            long lEntryStart = stream.Position;

            // strBody长度
            byte[] xmlbody = Encoding.UTF8.GetBytes(strBody);

            length = BitConverter.GetBytes((long)xmlbody.Length);

            stream.Write(length, 0, 8);  // body长度

            if (xmlbody.Length > 0)
            {
                // xml body本身
                stream.Write(xmlbody, 0, xmlbody.Length);
            }

            byte[] lengthbody = null;

            lengthbody = BitConverter.GetBytes(lAttachmentLength);
            length = BitConverter.GetBytes((long)lengthbody.Length);

            stream.Write(length, 0, 8);	// metadata长度

            Debug.Assert(lengthbody.Length == 8, "");
            stream.Write(lengthbody, 0, lengthbody.Length);
        }

        // 读出一个事项(string类型)
        // | body length (8bytes)| ... bodydata | attachment length | ... attachment |
        static int ReadCachedEntry(
            Stream stream,
            out string strBody,
            out long lTotalAttachmentLength,
            out string strError)
        {
            strBody = "";
            strError = "";
            lTotalAttachmentLength = 0;

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            // strBody长度
            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "strBody长度位置不足8bytes";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength > 1000 * 1024)
            {
                strError = "记录格式不正确，body长度超过1000K";
                return -1;
            }

            if (lBodyLength > 0)
            {
                byte[] xmlbody = new byte[(int)lBodyLength];

                nRet = stream.Read(xmlbody, 0, (int)lBodyLength);
                if (nRet < (int)lBodyLength)
                {
                    strError = "body不足其长度定义";
                    return -1;
                }

                strBody = Encoding.UTF8.GetString(xmlbody);
            }

            // attachment长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "attachment长度位置不足8bytes";
                return -1;
            }

            Int64 lAttachmentLength = BitConverter.ToInt64(length, 0);

            if (lAttachmentLength != 8)
            {
                strError = "记录格式不正确，lAttachmentLength != 8";
                return -1;
            }

            // attahment data
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "attachment data位置不足8bytes";
                return -1;
            }

            lTotalAttachmentLength = BitConverter.ToInt64(length, 0);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lBodyLength + 8 + lAttachmentLength + 8)
            {
                Debug.Assert(false, "");
                strError = "entry长度经检验不正确";
                return -1;
            }

            return 0;
        }

        // 创建日志文件的metadata文件，记载服务器端文件尺寸
        static int CreateCacheMetadataFile(
            string strCacheDir,
            string strDate,
            long lServerFileSize,
            out string strError)
        {
            strError = "";

            if (strDate.Length != 8)
            {
                strError = "strDate 参数值的长度应该是 8 字符";
                return -1;
            }

            string strLogFileName = strDate + ".log";


            string strCacheMetaDataFilename = Path.Combine(strCacheDir, strLogFileName + ".meta");
            try
            {
                File.Delete(strCacheMetaDataFilename);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                dom.DocumentElement.SetAttribute("serverFileSize", lServerFileSize.ToString());
                dom.Save(strCacheMetaDataFilename);
            }
            catch (Exception ex)
            {
                strError = "创建metadata文件 '" + strCacheMetaDataFilename + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 删除一个日志文件的本地缓存文件
        static int DeleteCacheFile(
    string strCacheDir,
    string strDate,
    out string strError)
        {
            strError = "";

            if (strDate.Length != 8)
            {
                strError = "strDate 参数值的长度应该是 8 字符";
                return -1;
            }

            string strLogFileName = strDate + ".log";

            string strCacheFilename = Path.Combine(strCacheDir, strLogFileName);
            string strCacheMetaDataFilename = Path.Combine(strCacheDir, strLogFileName + ".meta");
            try
            {
                File.Delete(strCacheMetaDataFilename);
                File.Delete(strCacheFilename);
            }
            catch (Exception ex)
            {
                strError = "删除日志缓存文件 '" + strCacheFilename + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 从本地缓存的日志文件当前位置读出一条日志记录
        /// </summary>
        /// <param name="stream">Stream 对象</param>
        /// <param name="strXmlBody">返回日志记录 XML</param>
        /// <param name="lTotalAttachmentLength">返回日志附件尺寸</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        static int ReadCachedEnventLog(
            Stream stream,
            out string strXmlBody,
            out long lTotalAttachmentLength,
            out string strError)
        {
            strError = "";
            strXmlBody = "";
            lTotalAttachmentLength = 0;

            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lRecordLength = BitConverter.ToInt64(length, 0);

            if (lRecordLength == 0)
                return 0;   // 表示这是一个空记录

            Debug.Assert(lRecordLength != 0, "");

            // 读出xml事项
            nRet = ReadCachedEntry(stream,
                out strXmlBody,
                out lTotalAttachmentLength,
                out strError);
            if (nRet == -1)
                return -1;

            // 文件指针自然指向末尾位置
            // this.m_stream.Seek(lRecordLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lRecordLength + 8)
            {
                Debug.Assert(false, "");
                strError = "Record长度经检验不正确: stream.Position - lStart ["
                    + (stream.Position - lStart).ToString()
                    + "] 不等于 lRecordLength + 8 ["
                    + (lRecordLength + 8).ToString()
                    + "]";
                return -1;
            }

            return 0;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";
            int nRet = 0;

            long lRet = 0;

            if (this.Date.Length != 8)
                throw new ArgumentException("FileName 成员值的长度应该是 8 字符");

            if ((this.LogType & LogType.AccessLog) != 0
                && (this.LogType & LogType.OperLog) != 0)
                throw new ArgumentException("OperLogItemLoader 的 LogType 只能使用一种类型");

            if (this.Stop != null && this.Estimate != null)
                this.Stop.SetMessage("正在装入日志文件 " + this.Date + " 中的记录。"
                    + "剩余时间 " + ProgressEstimate.Format(Estimate.Estimate(lProgressValue)) + " 已经过时间 " + ProgressEstimate.Format(Estimate.delta_passed));

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            long lFileSize = 0;
            // 2021/3/17
            // 如果是当天的日志文件，尺寸易变，要每次都探测一下
            if (lServerFileSize == -1
                || IsToday(this.Date))
            {
                long _lServerFileSize = 0;

                string strStyle = "level-" + Level.ToString();
                if ((this.LogType & LogType.AccessLog) != 0)
                    strStyle += ",accessLog";

                // 获得服务器端日志文件尺寸
                lRet = this.Channel.GetOperLog(
                    this.Stop,
                    this.Date + ".log", // 2021/3/18 增加 ".log"
                    -1,    // lIndex,
                    -1, // lHint,
                    strStyle,
                    "", // strFilter
                    out strXml,
                    out _lServerFileSize,
                    0,  // lAttachmentFragmentStart,
                    0,  // nAttachmentFramengLength,
                    out attachment_data,
                    out lAttachmentTotalLength,
                    out strError);
                // 2015/11/25
                if (lRet == -1)
                    throw new ChannelException(this.Channel.ErrorCode, strError);

                this.lServerFileSize = _lServerFileSize;

                if (lRet == 0)
                    yield break;
                // 2015/11/25
                if (_lServerFileSize == -1)
                    yield break;    // 此类型的日志尚未启用
            }

            Stream stream = null;
            bool bCacheFileExist = false;
            bool bRemoveCacheFile = false;  // 是否要自动删除未全部完成的本地缓存文件

            bool bAutoCache = this.AutoCache;

            if (bAutoCache == true)
            {
                string strFileName = this.Date;
                if ((this.LogType & LogType.AccessLog) != 0)
                    strFileName = this.Date + ".a";

                nRet = PrepareCacheFile(
                    this.CacheDir,
                    strFileName,    // this.FileName,
                    lServerFileSize,
                    out bCacheFileExist,
                    out stream,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                if (bCacheFileExist == false && stream != null)
                    bRemoveCacheFile = true;
            }

            try
            {
                if (bCacheFileExist == true)
                    lFileSize = stream.Length;
                else
                    lFileSize = lServerFileSize;

                // stop.SetProgressRange(0, lTotalSize);

                if (String.IsNullOrEmpty(Range) == true)
                    Range = "0-" + (long.MaxValue - 1).ToString();    // "0-9999999999";

                RangeList rl = new RangeList(Range);

                for (int i = 0; i < rl.Count; i++)
                {
                    RangeItem ri = (RangeItem)rl[i];

                    // 让 100- 这样的 range 可以使用
                    if (ri.lLength == -1)
                        ri.lLength = long.MaxValue - ri.lStart;

                    OperLogInfo[] records = null;
                    long lStartRecords = 0;

                    long lHint = -1;
                    long lHintNext = -1;
                    for (long lIndex = ri.lStart; lIndex < ri.lStart + ri.lLength; lIndex++)
                    {
                        // Application.DoEvents();

                        if (this.Stop != null && this.Stop.State != 0)
                        {
                            strError = "用户中断";
                            throw new InterruptException(strError);
                            // yield break; ?
                        }

                        if (lIndex == ri.lStart)
                            lHint = -1;
                        else
                            lHint = lHintNext;

                        if (bCacheFileExist == true)
                        {
                            if (lHint == -1)
                            {
                                // return:
                                //      -1  error
                                //      0   成功
                                //      1   到达文件末尾或者超出
                                nRet = LocationRecord(stream,
                    lIndex,
                    out strError);
                                if (nRet == -1)
                                    throw new Exception(strError);
                            }
                            else
                            {
                                // 根据暗示找到
                                if (lHint == stream.Length)
                                    break;

                                if (lHint > stream.Length)
                                {
                                    strError = "lHint参数值不正确";
                                    throw new Exception(strError);
                                }
                                if (stream.Position != lHint)
                                    stream.Seek(lHint, SeekOrigin.Begin);
                            }

                            nRet = ReadCachedEnventLog(
                                stream,
                                out strXml,
                                out lAttachmentTotalLength,
                                out strError);
                            if (nRet == -1)
                                throw new Exception(strError);
                            lHintNext = stream.Position;
                        }
                        else
                        {
                            if (records == null || lIndex /*- ri.lStart*/ >= lStartRecords + records.Length)
                            {
                                int nCount = -1;
                                if (ri.lLength >= Int32.MaxValue)
                                    nCount = -1;    // 500;   // -1;
                                else
                                    nCount = (int)ri.lLength;   // Math.Min(500, (int)ri.lLength);

                                string strStyle = "level-" + Level.ToString();
                                if ((this.LogType & LogType.AccessLog) != 0)
                                    strStyle += ",accessLog";

                                // 2017/10/9
                                if (this.ReplicationLevel == true)
                                    strStyle += ",supervisor";  // 注：当前账户中还应该包含 replication 权限才能真正获得日志记录中的密码字段

                                if (string.IsNullOrEmpty(this.ServerVersion) == false
                                    && StringUtil.CompareVersion(this.ServerVersion, "3.17") >= 0)
                                    strStyle += ",wait";

                                REDO:
                                // 获得日志
                                // return:
                                //      -1  error
                                //      0   file not found
                                //      1   succeed
                                //      2   超过范围，本次调用无效
                                lRet = this.Channel.GetOperLogs(
                                    this.Stop,
                                    this.Date + ".log",
                                    lIndex,
                                    lHint,
                                    nCount,
                                    strStyle,
                                    this.Filter, // strFilter
                                    out records,
                                    out strError);
                                if (lRet == -1)
                                {
#if NO
                                    DialogResult result = MessageBox.Show(owner,
    "获取日志信息 ("+this.FileName + " " +lIndex.ToString() + ") 的操作发生错误： " + strError + "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试;  否: 跳过本次操作，继续后面的操作; 放弃: 停止全部操作)",
    "OperLogItemLoader",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                                    if (result == DialogResult.Yes)
                                        goto REDO;
                                    if (result == DialogResult.Cancel)
                                        throw new Exception(strError);
                                    else
                                    {
                                        // TODO: 是否要在listview中装入一条表示出错的行?
                                        lHintNext = -1;
                                        continue;
                                    }
#endif
                                    bool isStopped = (this.Stop != null && this.Stop.State != 0);
                                    if (isStopped)
                                        throw new InterruptException(strError);

                                    if (this.Prompt != null)
                                    {
                                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                                        e.MessageText = "获取 " + this._logType.ToString() + " 日志信息 (" + this.Date + " " + lIndex.ToString() + ") 的操作发生错误： " + strError;
                                        e.Actions = "yes,no,cancel";
                                        this.Prompt(this, e);
                                        if (e.ResultAction == "cancel")
                                            throw new InterruptException(strError);
                                        else if (e.ResultAction == "yes")
                                        {
                                            if (this.Stop != null)
                                                this.Stop.Continue();
                                            goto REDO;
                                        }
                                        else
                                        {
                                            lHintNext = -1;
                                            continue;
                                        }
                                    }
                                    else
                                        throw new ChannelException(this.Channel.ErrorCode, strError);
                                }
                                if (lRet == 0)
                                    yield break;

                                if (lRet == 2)
                                    break;

                                // records数组表示的起点位置
                                lStartRecords = lIndex /* - ri.lStart*/;
                            }

                            OperLogInfo info = records[lIndex - lStartRecords];

                            strXml = info.Xml;
                            lHintNext = info.HintNext;
                            lAttachmentTotalLength = info.AttachmentLength;

                            // 写入本地缓存的日志文件
                            if (stream != null)
                            {
                                try
                                {
                                    WriteCachedEnventLog(
                                        stream,
                                        strXml,
                                        lAttachmentTotalLength);
                                }
                                catch (Exception ex)
                                {
                                    strError = "写入本地缓存文件的时候出错: " + ex.Message;
                                    throw new Exception(strError);
                                }
                            }
                        }

#if NO
                            // 2011/12/30
                            // 日志记录可能动态地增加了，超过了原先为ProgressBar设置的范围
                            if (lFizeTotalSize < (int)lHintNext)
                            {
                                lFizeTotalSize = lHintNext;

                                stop.SetProgressRange(0, lFizeTotalSize);
                            }
#endif
                        if (lHintNext >= 0)
                        {
                            // 校正
                            if (lProgressValue + lHintNext >= lSize)    // > 2017/12/4 修改为 >=
                            {
                                lSize = lProgressValue + lHintNext;

                                if (this.Stop != null)
                                    this.Stop.SetProgressRange(0, lSize);
                                if (this.Estimate != null)
                                    Estimate.SetRange(0, lSize);
                            }

                            this.Stop?.SetProgressValue(lProgressValue + lHintNext);
                        }

                        if (lIndex % 100 == 0)
                        {
                            if (this.Stop != null && this.Estimate != null)
                            {
                                Estimate.Text = "剩余时间 " + ProgressEstimate.Format(Estimate.Estimate(lProgressValue + lHintNext)) + " 已经过时间 " + ProgressEstimate.Format(Estimate.delta_passed);
                                this.Stop.SetMessage("正在装入日志文件 " + this.Date + " 中的记录 " + lIndex.ToString() + " 。"
                                    + Estimate.Text);
                            }
                        }

                        {
                            OperLogItem item = new OperLogItem
                            {
                                Xml = strXml,
                                Index = lIndex,
                                Date = this.Date.Substring(0, 8),
                                AttachmentLength = lAttachmentTotalLength
                            };
                            yield return item;
                        }

                    }
                }

                // 创建本地缓存的日志文件的 metadata 文件
                if (bCacheFileExist == false && stream != null)
                {
                    string strFileName = this.Date;
                    if ((this.LogType & LogType.AccessLog) != 0)
                        strFileName = this.Date + ".a";

                    nRet = CreateCacheMetadataFile(
                        this.CacheDir,
                        strFileName,    // this.FileName,
                        lServerFileSize,
                        out strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                }

                bRemoveCacheFile = false;   // 不删除
            }
            finally
            {
                if (stream != null)
                    stream.Close();

                if (bRemoveCacheFile == true)
                {
                    string strFileName = this.Date;
                    if ((this.LogType & LogType.AccessLog) != 0)
                        strFileName = this.Date + ".a";

                    string strError1 = "";
                    nRet = DeleteCacheFile(
                        this.CacheDir,
                        strFileName,    // this.FileName,
                        out strError1);
                    if (nRet == -1)
                    {
                        // MessageBox.Show(owner, strError1);
                        if (this.Prompt != null)
                        {
                            MessagePromptEventArgs e = new MessagePromptEventArgs();
                            e.MessageText = strError1;
                            e.Actions = "ok";
                            this.Prompt(this, e);
                        }
                    }
                }
            }

            lProgressValue += lFileSize;
        }

        static bool IsToday(string date)
        {
            return date == DateTimeUtil.DateTimeToString8(DateTime.Now);
        }
    }
}
