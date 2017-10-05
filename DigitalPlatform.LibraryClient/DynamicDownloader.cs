using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;

using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 用于下载动态增长的文件的下载器
    /// </summary>
    public class DynamicDownloader
    {
        // 附加的数据
        public object Tag { get; set; }

        public event EventHandler Closed = null;

        public event DownloadProgressChangedEventHandler ProgressChanged = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

#if NO
        CancellationToken _token = new CancellationToken();

        public CancellationToken CancelToken
        {
            get
            {
                return _token;
            }
            set
            {
                _token = value;
            }
        }
#endif

        public string State { get; set; }

        public string ErrorInfo { get; set; }

        // 服务器端的文件路径
        public string ServerFilePath { get; set; }

        // 本地文件路径
        public string LocalFilePath { get; set; }

        public LibraryChannel Channel { get; set; }

        public Stop Stop { get; set; }

        FileStream _stream = null;

        byte[] _timestamp = null;   // 下载完成后的 timestamp

        public DynamicDownloader(
            LibraryChannel channel,
            string strServerFilePath,
            string strOutputFileName)
        {
            this.Channel = channel;
            this.ServerFilePath = strServerFilePath;
            this.LocalFilePath = strOutputFileName;
        }

        public void Cancel()
        {
            this._cancel.Cancel();
        }

        public bool IsCancellationRequested
        {
            get
            {
                return (_cancel.IsCancellationRequested);
            }
        }

        public void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;

                TriggerClosedEvent();
            }
        }

        void TriggerClosedEvent()
        {
            if (this.Closed != null)
                this.Closed(this, new EventArgs());
        }

        string GetTempFileName()
        {
            return GetTempFileName(this.LocalFilePath);
        }

        public static string GetTempFileName(string strFileName)
        {
            return strFileName + ".tmp";
        }

        void RenameTempFile()
        {
            string strTempFileName = this.GetTempFileName();

            // 2017/9/22
            if (File.Exists(this.LocalFilePath))
                File.Delete(this.LocalFilePath);

            if (File.Exists(strTempFileName))
                File.Move(strTempFileName, this.LocalFilePath);

            if (this._timestamp != null
                && this.ServerFilePath.StartsWith("!"))
            {
                // 根据返回的时间戳设置文件最后修改时间
                SetFileLastWriteTimeByTimestamp(this.LocalFilePath, this._timestamp);
            }
        }

        public Task StartDownload(bool bContinue)
        {
            // 创建输出文件
            this.Close();

            string strTempFileName = this.GetTempFileName();

            if (bContinue == false)
                _stream = File.Create(strTempFileName);
            else
            {
                _stream = File.Open(strTempFileName, FileMode.OpenOrCreate);
                _stream.Seek(0, SeekOrigin.End);
            }

            return Task.Factory.StartNew(() => Download(),
    CancellationToken.None,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

        // TODO: 遇到出错，要删除已经下载的临时文件; 如果是中断，临时文件要保留
        void Download()
        {
            string strError = "";

            TimeSpan old_timeout = this.Channel.Timeout;
            this.Channel.Timeout = TimeSpan.FromSeconds(60);
            try
            {
                bool bNotFound = false;
                string strPath = this.ServerFilePath;
                string strStyle = "content,data,metadata,timestamp,outputpath";

                byte[] baContent = null;

                long lStart = _stream.Length;
                int nPerLength = -1;

                // byte[] old_timestamp = null;
                byte[] timestamp = null;

                long lTotalLength = -1;

                for (; ; )
                {
#if NO
                    if (_token.IsCancellationRequested)
                    {
                        strError = "中断";
                        goto ERROR1;
                    }
#endif

                    if (_cancel.IsCancellationRequested)
                    {
                        strError = "中断";
                        goto ERROR1;
                    }

                    if (this.Stop != null && this.Stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    // REDO:

                    string strMessage = "";

                    string strPercent = "";
                    if (lTotalLength != -1)
                    {
                        double ratio = (double)lStart / (double)lTotalLength;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    if (this.Stop != null)
                    {
                        strMessage = "正在下载 " + Convert.ToString(lStart) + "-"
                            + (lTotalLength == -1 ? "?" : Convert.ToString(lTotalLength))
                            + " " + strPercent + " "
                            + strPath;
                        this.Stop.SetMessage(strMessage);
                    }

                    string strMetadata = "";
                    string strOutputPath = "";
                    long lRet = this.Channel.GetRes(
                        this.Stop,
                        this.ServerFilePath,
                        lStart,
                        nPerLength,
                        strStyle,
                        out baContent,
                        out strMetadata,
                        out strOutputPath,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (this.Channel.ErrorCode == localhost.ErrorCode.NotFound)
                        {
                            bNotFound = true;
                            goto DETECT_STATE;
                        }
                        goto ERROR1;
                    }

                    bNotFound = false;

#if NO
                if (bHasMetadataStyle == true)
                {
                    StringUtil.RemoveFromInList("metadata",
                        true,
                        ref strStyle);
                    bHasMetadataStyle = false;
                }
#endif
                    if (lTotalLength != -1 && lTotalLength > lRet)
                    {
                        strError = "下载被前端放弃。因下载中途文件尺寸变小(曾经的尺寸=" + lTotalLength + ",当前尺寸=" + lRet + ")";
                        goto ERROR1;
                    }

                    lTotalLength = lRet;

#if NO
                if (StringUtil.IsInList("timestamp", strStyle) == true)
                {
                    if (input_timestamp != null)
                    {
                        if (ByteArray.Compare(input_timestamp, timestamp) != 0)
                        {
                            strError = "下载过程中发现时间戳和input_timestamp参数中的时间戳不一致，下载失败 ...";
                            return -1;
                        }
                    }
                    if (old_timestamp != null)
                    {
                        if (ByteArray.Compare(old_timestamp, timestamp) != 0)
                        {
                            strError = "下载过程中发现时间戳变化，下载失败 ...";
                            return -1;
                        }
                    }
                }

                old_timestamp = timestamp;

                if (fileTarget == null)
                    break;
#endif

                    // 写入文件
                    if (StringUtil.IsInList("attachment", strStyle) == true)
                    {
                        Debug.Assert(false, "attachment style暂时不能使用");
                    }
                    else
                    {
                        if (_cancel.IsCancellationRequested)
                        {
                            strError = "中断";
                            goto ERROR1;
                        }

                        Debug.Assert(StringUtil.IsInList("content", strStyle) == true,
                            "不是attachment风格，就应是content风格");

                        Debug.Assert(baContent != null, "返回的baContent不能为null");
                        Debug.Assert(baContent.Length <= lRet, "每次返回的包尺寸[" + Convert.ToString(baContent.Length) + "]应当小于result.Value[" + Convert.ToString(lRet) + "]");

                        if (baContent.Length > 0)
                        {
                            _stream.Write(baContent, 0, baContent.Length);
                            _stream.Flush(); // 2013/5/17
                            lStart += baContent.Length;

                            var func = this.ProgressChanged;
                            if (func != null)
                            {
                                try
                                {
                                    DownloadProgressChangedEventArgs e = new DownloadProgressChangedEventArgs(lStart, lTotalLength);
                                    func(this, e);
                                }
                                catch(ObjectDisposedException)
                                {

                                }
                            }
                        }
                    }

                DETECT_STATE:
                    if (lStart >= lRet || bNotFound == true)
                    {
                        // 探测文件状态。
                        string strState = "";
                        // 探测下载状态
                        // return:
                        //      -1  出错
                        //      0   文件没有找到
                        //      1   文件找到
                        int nRet = DetectDownloadState(this.ServerFilePath,
                out strState,
                out strError);
                        if (nRet == -1)
                        {
                            strError = "探测状态文件过程出错: " + strError;
                            goto ERROR1;
                        }

                        if (nRet == 0 && bNotFound)
                        {
                            strError = "文件 '" + this.ServerFilePath + "' 没有找到";
                            goto ERROR1;
                        }

                        if (nRet == 0 // 状态文件没有找到，说明不是动态下载情形，要结束下载
                            || strState != "creating")
                        {
                            // 需要再次确认一下文件最大尺寸
                            long lTempLength = DetectFileLength(lTotalLength,
            out strError);
                            if (lTempLength == -1)
                            {
                                strError = "文件 '" + this.ServerFilePath + "' finish 之前探测文件尺寸发生错误: " + strError;
                                goto ERROR1;
                            }

                            // 如果发现文件尺寸又变大了，则继续循环
                            if (lTempLength > lTotalLength)
                                continue;

                            if (strState != "finish" && nRet != 0)
                                this.ErrorInfo = "下载文件 '" + this.ServerFilePath + "' 时遭遇状态出错: " + strState;
                            else if (this.ServerFilePath.StartsWith("!"))
                            {
                                DisplayMessage("正在获得服务器文件 " + this.ServerFilePath + " 的 MD5 ...");
                                // 检查 MD5
                                byte[] server_md5 = null;
                                // return:
                                //      -1  出错
                                //      0   文件没有找到
                                //      1   文件找到
                                nRet = GetServerFileMD5(
                                    this.Channel,
                                    this.Stop,
                                    this.ServerFilePath,
            out server_md5,
            out strError);
                                if (nRet != 1)
                                {
                                    strError = "探测服务器端文件 '" + this.ServerFilePath + "' MD5 时出错: " + strError;
                                    goto ERROR1;
                                }

                                DisplayMessage("正在获得本地文件 " + this.LocalFilePath + " 的 MD5 ...");

                                _stream.Seek(0, SeekOrigin.Begin);
                                byte[] local_md5 = GetFileMd5(_stream);
                                if (ByteArray.Compare(server_md5, local_md5) != 0)
                                {
                                    strError = "服务器端文件 '" + this.ServerFilePath + "' 和刚下载的本地文件 MD5 不匹配";
                                    goto ERROR1;
                                }
                            }
                            this.State = "finish:" + strState;
                            TriggerClosedEvent();
                            this._timestamp = timestamp;
                            return;
                        }

                        // 休眠一段时间后重试下载
                        Thread.Sleep(1000);
                    }
                } // end of for

                this.State = "end";
                TriggerClosedEvent();
                return;
            ERROR1:
                this.State = "error";
                this.ErrorInfo = strError;
                TriggerClosedEvent();
            }
            catch (Exception ex)
            {
                this.State = "error";
                this.ErrorInfo = ExceptionUtil.GetDebugText(ex);
                TriggerClosedEvent();
            }
            finally
            {
                this.Channel.Timeout = old_timeout;

                if (_stream != null)
                {
                    _stream.Close();
                    _stream = null;
                }

                if (string.IsNullOrEmpty(this.ErrorInfo))
                    RenameTempFile();
            }
        }

        public static void SetFileLastWriteTimeByTimestamp(string strFilePath,
    byte[] baTimeStamp)
        {
            if (baTimeStamp == null || baTimeStamp.Length < 8)
                return;
            long lTicks = BitConverter.ToInt64(baTimeStamp, 0);

            FileInfo fileInfo = new FileInfo(strFilePath);
            if (fileInfo.Exists == false)
                return;
            fileInfo.LastWriteTimeUtc = new DateTime(lTicks);
        }


        public static byte[] GetFileMd5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(stream);
            }
        }

        // 探测下载状态
        // return:
        //      -1  出错
        //      0   文件没有找到
        //      1   文件找到
        int DetectDownloadState(string strServerPath,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            string strPath = strServerPath + ".~state"; // LibraryServerUtil.STATE_EXTENSION

            string strStyle = "content,data";
            string strMetadata = "";
            byte[] baOutputTimestamp = null;
            string strOutputPath = "";

            // 获得资源。包装版本 -- 返回字符串版本。
            // return:
            //		strStyle	一般设置为"content,data,metadata,timestamp,outputpath";
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            long lRet = this.Channel.GetRes(
                this.Stop,
                strPath,
                strStyle,
                out strResult,
                out strMetadata,
                out baOutputTimestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (this.Channel.ErrorCode == localhost.ErrorCode.NotFound)
                    return 0;
                return -1;
            }

            return 1;
        }

        long DetectFileLength(long lStart,
            out string strError)
        {
            byte[] baContent = null;
            string strMetadata = "";
            string strOutputPath = "";
            string strStyle = "content,data";
            byte[] timestamp = null;

            long lRet = this.Channel.GetRes(
                this.Stop,
                this.ServerFilePath,
                lStart,
                0,  // nPerLength,
                strStyle,
                out baContent,
                out strMetadata,
                out strOutputPath,
                out timestamp,
                out strError);
            if (lRet == -1)
                return -1;

            return lRet;
        }

        void DisplayMessage(string strText)
        {
            var func = this.ProgressChanged;
            if (func != null)
            {
                try
                {
                    DownloadProgressChangedEventArgs e = new DownloadProgressChangedEventArgs(strText);
                    func(this, e);
                }
                catch (ObjectDisposedException)
                {

                }
            }
        }

        // 探测 MD5
        // return:
        //      -1  出错
        //      0   文件没有找到
        //      1   文件找到
        public static int GetServerFileMD5(
            LibraryChannel channel,
            Stop stop,
            string strServerPath,
            out byte[] md5,
            out string strError)
        {
            strError = "";
            md5 = null;

            string strStyle = "md5";
            string strMetadata = "";
            byte[] baContent = null;
            string strOutputPath = "";

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);
            try
            {
                // return:
                //		strStyle	一般设置为"content,data,metadata,timestamp,outputpath";
                //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                //		0	成功
                long lRet = channel.GetRes(
                    stop,
                    strServerPath,
                    0,
                    0,
                    strStyle,
                    out baContent,
                    out strMetadata,
                    out strOutputPath,
                    out md5,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == localhost.ErrorCode.NotFound)
                        return 0;
                    return -1;
                }

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

#if NO
        // 探测 MD5
        // return:
        //      -1  出错
        //      0   文件没有找到
        //      1   文件找到
        int DetectMD5(string strServerPath,
            out byte[] md5,
            out string strError)
        {
            strError = "";
            md5 = null;

            string strStyle = "md5";
            string strMetadata = "";
            byte[] baContent = null;
            string strOutputPath = "";

            TimeSpan old_timeout = this.Channel.Timeout;
            this.Channel.Timeout = TimeSpan.FromMinutes(5);
            try
            {
                // return:
                //		strStyle	一般设置为"content,data,metadata,timestamp,outputpath";
                //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                //		0	成功
                long lRet = this.Channel.GetRes(
                    this.Stop,
                    strServerPath,
                    0,
                    0,
                    strStyle,
                    out baContent,
                    out strMetadata,
                    out strOutputPath,
                    out md5,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.ErrorCode == localhost.ErrorCode.NotFound)
                        return 0;
                    return -1;
                }

                return 1;
            }
            finally
            {
                this.Channel.Timeout = old_timeout;
            }
        }
#endif
    }

    // 摘要: 
    //     表示将要处理 System.Net.WebClient 的 System.Net.WebClient.DownloadProgressChanged
    //     事件的方法。
    //
    // 参数: 
    //   sender:
    //     事件源。
    //
    //   e:
    //     包含事件数据的 System.Net.DownloadProgressChangedEventArgs。
    public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);

    // 摘要: 
    //     为 System.Net.WebClient 的 System.Net.WebClient.DownloadProgressChanged 事件提供数据。
    public class DownloadProgressChangedEventArgs
    {
        public string Text { get; set; }

        public DownloadProgressChangedEventArgs(string strText)
        {
            this.Text = strText;
        }

        public DownloadProgressChangedEventArgs(long recieved, long total)
        {
            this.BytesReceived = recieved;
            this.TotalBytesToReceive = total;
        }

        // 摘要: 
        //     获取收到的字节数。
        //
        // 返回结果: 
        //     一个指示收到的字节数的 System.Int64 值。
        public long BytesReceived { get; set; }
        //
        // 摘要: 
        //     获取 System.Net.WebClient 数据下载操作中的字节总数。
        //
        // 返回结果: 
        //     一个指示将要接收的字节数的 System.Int64 值。
        public long TotalBytesToReceive { get; set; }
    }
}
