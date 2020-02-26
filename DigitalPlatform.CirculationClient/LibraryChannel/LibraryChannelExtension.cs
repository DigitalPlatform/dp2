using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Core;

namespace DigitalPlatform.CirculationClient
{
    public static class LibraryChannelExtension
    {
        // 上载对象。以对象文件方式。自动探测 MIME
        public static int UploadObject(
            this LibraryChannel channel,
            Stop stop,
            string strClientFilePath,
            string strServerFilePath,
            string strStyle,
            byte[] timestamp,
            bool bRetryOverwiteExisting,
            out byte[] output_timestamp,
            out string strError)
        {
            string strMime = PathUtil.MimeTypeFrom(strClientFilePath);
            string strMetadata = LibraryChannel.BuildMetadata(strMime, Path.GetFileName(strClientFilePath));
            return UploadFile(
            channel,
            stop,
            strClientFilePath,
            strServerFilePath,
            strMetadata,
            strStyle,
            timestamp,
            bRetryOverwiteExisting,
            false,
            null,
            out output_timestamp,
            out strError);
        }

        // 
        public static int UploadObject(
    this LibraryChannel channel,
    Stop stop,
    string strClientFilePath,
    string strServerFilePath,
    string strStyle,
    byte[] timestamp,
    bool bRetryOverwiteExisting,
    bool bProgressChange,
    delegate_prompt prompt_func,
    out byte[] output_timestamp,
    out string strError)
        {
            string strMime = PathUtil.MimeTypeFrom(strClientFilePath);
            string strMetadata = LibraryChannel.BuildMetadata(strMime, Path.GetFileName(strClientFilePath));
            return UploadFile(
            channel,
            stop,
            strClientFilePath,
            strServerFilePath,
            strMetadata,
            strStyle,
            timestamp,
            bRetryOverwiteExisting,
            bProgressChange,
            prompt_func,
            out output_timestamp,
            out strError);
        }

        public static int UploadFile(
    this LibraryChannel channel,
    Stop stop,
    string strClientFilePath,
    string strServerFilePath,
    string strMetadata,
    string strStyle,
    byte[] timestamp,
    bool bRetryOverwiteExisting,
    out byte[] output_timestamp,
    out string strError)
        {
            return UploadFile(
            channel,
            stop,
            strClientFilePath,
            strServerFilePath,
            strMetadata,
            strStyle,
            timestamp,
            bRetryOverwiteExisting,
            false,
            null,
            out output_timestamp,
            out strError);
        }

#if NO
        // 兼容以前的版本
        public static int UploadFile(
    this LibraryChannel channel,
    Stop stop,
    string strClientFilePath,
    string strServerFilePath,
    string strMetadata,
    string strStyle,
    byte[] timestamp,
    bool bRetryOverwiteExisting,
    bool bProgressChange,
    out byte[] output_timestamp,
    out string strError)
        {
            return UploadFile(
            channel,
            stop,
            strClientFilePath,
            strServerFilePath,
            strMetadata,
            strStyle,
            timestamp,
            bRetryOverwiteExisting,
            bProgressChange,
            null,
            out output_timestamp,
            out strError);
        }
#endif

        // return:
        //      "retry" "skip" 或 "cancel"
        public delegate string delegate_prompt(LibraryChannel channel,
            string message,
            string[] buttons,
            int seconds);

        // 上传文件到到 dp2lbrary 服务器
        // 注：已通过 prompt_func 实现通讯出错时候重试
        // parameters:
        //      timestamp   时间戳。如果为 null，函数会自动根据文件信息得到一个时间戳
        //      bRetryOverwiteExisting   是否自动在时间戳不一致的情况下覆盖已经存在的服务器文件。== true，表示当发现时间戳不一致的时候，自动用返回的时间戳重试覆盖
        // return:
        //		-1	出错
        //		0   上传文件成功
        public static int UploadFile(
            this LibraryChannel channel,
            Stop stop,
            string strClientFilePath,
            string strServerFilePath,
            string strMetadata,
            string strStyle,
            byte[] timestamp,
            bool bRetryOverwiteExisting,
            bool bProgressChange,
            delegate_prompt prompt_func,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;

            string strResPath = strServerFilePath;

#if NO
            if (string.IsNullOrEmpty(strMime))
                strMime = PathUtil.MimeTypeFrom(strClientFilePath);
#endif

            // 只修改 metadata
            if (string.IsNullOrEmpty(strClientFilePath) == true)
            {
                long lRet = channel.SaveResObject(
stop,
strResPath,
null, // strClientFilePath,
-1, // 0, // 0 是 bug，会导致清除原有对象内容 2018/8/13
strMetadata,
"",
timestamp,
strStyle,
out output_timestamp,
out strError);
                timestamp = output_timestamp;
                if (lRet == -1)
                    return -1;
                return 0;
            }

            long skip_offset = 0;

            {
            REDO_DETECT:
                // 先检查以前是否有已经上传的局部
                long lRet = channel.GetRes(stop,
                    strServerFilePath,
                    0,
                    0,
                    "uploadedPartial",
                    out byte[] temp_content,
                    out string temp_metadata,
                    out string temp_outputPath,
                    out byte[] temp_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.NotFound)
                    {
                        // 以前上传的局部不存在，说明只能从头上传
                        skip_offset = 0;
                    }
                    else
                    {
                        // 探测过程通讯或其他出错
                        if (prompt_func != null)
                        {
                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            string action = prompt_func(channel,
                                strError + "\r\n\r\n(重试) 重试操作; (中断) 中断处理",
                                new string[] { "重试", "中断" },
                                10);
                            if (action == "重试")
                                goto REDO_DETECT;
                            if (action == "中断")
                                return -1;
                        }
                        return -1;
                    }
                }
                else if (lRet > 0)
                {
                    // *** 发现以前存在 lRet 这么长的已经上传部分
                    long local_file_length = FileUtil.GetFileLength(strClientFilePath);
                    // 本地文件尺寸居然小于已经上传的临时部分
                    if (local_file_length < lRet)
                    {
                        // 只能从头上传
                        skip_offset = 0;
                    }
                    else
                    {
                        // 询问是否断点续传
                        if (prompt_func != null)
                        {
                            string percent = StringUtil.GetPercentText(lRet, local_file_length);
                            string action = prompt_func(null,
                                $"本地文件 {strClientFilePath} 以前曾经上传过长度为 {lRet} 的部分内容(占整个文件 {percent})，请问现在是否继续上传余下部分? \r\n[是]从断点位置开始继续上传; [否]从头开始上传; [中断]取消这个文件的上传",
                                new string[] { "是", "否", "中断" },
                                0);
                            if (action == "是")
                                skip_offset = lRet;
                            else if (action == "否")
                                skip_offset = 0;
                            else
                            {
                                strError = "取消处理";
                                return -1;
                            }
                        }
                        // 如果 prompt_func 为 null 那就不做断点续传，效果是从头开始上传
                    }
                }
            }

            // 检测文件尺寸
            FileInfo fi = new FileInfo(strClientFilePath);
            if (fi.Exists == false)
            {
                strError = "文件 '" + strClientFilePath + "' 不存在...";
                return -1;
            }

            string[] ranges = null;

            if (fi.Length == 0)
            {
                // 空文件
                ranges = new string[1];
                ranges[0] = "";
            }
            else
            {
                string strRange = "";
                strRange = $"{skip_offset}-{(fi.Length - 1)}";

                // 按照100K作为一个chunk
                // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                ranges = RangeList.ChunkRange(strRange,
                    channel.UploadResChunkSize // 500 * 1024
                    );
                if (bProgressChange && stop != null)
                    stop.SetProgressRange(0, fi.Length);
            }

            if (timestamp == null)
                timestamp = FileUtil.GetFileTimestamp(strClientFilePath);

            // byte[] output_timestamp = null;

            string strWarning = "";

            TimeSpan old_timeout = channel.Timeout;
            try
            {
                using (FileStream stream = File.OpenRead(strClientFilePath))
                {
                    for (int j = 0; j < ranges.Length; j++)
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string range = ranges[j];

                        string strWaiting = "";
                        if (j == ranges.Length - 1)
                        {
                            strWaiting = " 请耐心等待...";
                            channel.Timeout = new TimeSpan(0, 40, 0);   // 40 分钟
                        }

                        string strPercent = "";
                        RangeList rl = new RangeList(range);
                        if (rl.Count != 1)
                        {
                            strError = $"{range} 中只应包含一个连续范围";
                            return -1;
                        }

                        {
                            double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                            strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                        }

                        if (stop != null)
                        {
                            stop.SetMessage( // strMessagePrefix + 
                                "正在上载 " + range + "/"
                                + StringUtil.GetLengthText(fi.Length)
                                + " " + strPercent + " " + strClientFilePath + strWarning + strWaiting);
                            if (bProgressChange && rl.Count > 0)
                                stop.SetProgressValue(rl[0].lStart);
                        }

                        // 2019/6/23
                        StreamUtil.FastSeek(stream, rl[0].lStart);

                        int nRedoCount = 0;
                        long save_pos = stream.Position;
                    REDO:
                        // 2019/6/21
                        // 如果是重做，文件指针要回到合适位置
                        if (stream.Position != save_pos)
                            StreamUtil.FastSeek(stream, save_pos);

                        long lRet = channel.SaveResObject(
        stop,
        strResPath,
        stream, // strClientFilePath,
        -1,
        j == ranges.Length - 1 ? strMetadata : null,    // 最尾一次操作才写入 metadata
        range,
        // j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
        timestamp,
        strStyle,
        out output_timestamp,
        out strError);
                        if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch)
                            strError = $"{strError}。timestamp={ByteArray.GetHexTimeStampString(timestamp)},output_timestamp={ByteArray.GetHexTimeStampString(output_timestamp)}。parsed:{ParseTimestamp(timestamp)};{ParseTimestamp(output_timestamp)}";

                        // Debug.WriteLine($"parsed:{ParseTimestamp(timestamp)};{ParseTimestamp(output_timestamp)}");

                        timestamp = output_timestamp;

                        strWarning = "";

                        if (lRet == -1)
                        {
                            // 如果是第一个 chunk，自动用返回的时间戳重试一次覆盖
                            if (bRetryOverwiteExisting == true
                                && j == 0
                                && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch
                                && nRedoCount == 0)
                            {
                                nRedoCount++;
                                goto REDO;
                            }

                            if (prompt_func != null)
                            {
                                if (stop != null && stop.State != 0)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }

                                string action = prompt_func(channel,
                                    strError + "\r\n\r\n(重试) 重试操作; (中断) 中断处理",
                                    new string[] { "重试", "中断" },
                                    10);
                                if (action == "重试")
                                    goto REDO;
                                if (action == "中断")
                                    return -1;
                            }
                            return -1;
                        }
                    }
                }

                if (StringUtil.IsInList("_checkMD5", strStyle))
                {
                    stop?.SetMessage($"正在校验本地文件 {strClientFilePath} 和刚上传的服务器文件 {strServerFilePath} ...");
                    // result.Value:
                    //      -1  出错
                    //      0   不匹配
                    //      1   匹配
                    // exception:
                    //      可能会抛出异常
                    var result = CheckMD5(
                        channel,
                        stop,
                        strServerFilePath,
                        strClientFilePath,
                        prompt_func);
                    stop?.SetMessage("MD5 校验完成");

                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        return -1;
                    }
                    if (result.Value == 0)
                    {
                        strError = $"UploadFile() 出错：本地文件 {strClientFilePath} 和刚上传的服务器文件 {strServerFilePath} MD5 校验不一致。请重新上传";
                        return -1;
                    }
                    Debug.Assert(result.Value == 1, "");
                }

            }
            catch (Exception ex)
            {
                strError = "UploadFile() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }

            return 0;
        }

        // result.Value:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        // exception:
        //      可能会抛出异常
        public static NormalResult CheckMD5(
            this LibraryChannel channel,
            Stop stop,
            string strServerFilePath,
            string strLocalFilePath,
            delegate_prompt prompt_func)
        {
            // stop 对中断 MD5 会起作用
            Debug.Assert(stop != null, "");

            // 2020/2/26 改为 ...ByTask()
            // 检查 MD5
            // return:
            //      -1  出错
            //      0   文件没有找到
            //      1   文件找到
            int nRet = DynamicDownloader.GetServerFileMD5ByTask(
                channel,
                stop,   // this.Stop,
                strServerFilePath,
                /*
                new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
                {
                    //转换为 prompt_func 发生作用
                }),
                */
                (o1, e1) =>
                {
                    if (prompt_func == null)
                    {
                        e1.ResultAction = "cancel";
                        return;
                    }

                    string[] buttons = e1.Actions.Split(new char[] { ',' });
                    //转换为 prompt_func 发生作用
                    e1.ResultAction = prompt_func(channel, e1.MessageText, buttons, 20);
                },
                new System.Threading.CancellationToken(),
                out byte[] server_md5,
                out string strError); ;
            if (nRet != 1)
            {
                strError = "探测服务器端文件 '" + strServerFilePath + "' MD5 时出错: " + strError;
                return new NormalResult(-1, strError);
            }

            using (FileStream stream = File.OpenRead(strLocalFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] local_md5 = DynamicDownloader.GetFileMd5(stream);
                if (ByteArray.Compare(server_md5, local_md5) != 0)
                {
                    strError = "服务器端文件 '" + strServerFilePath + "' 和本地文件 '" + strLocalFilePath + "' MD5 不匹配";
                    return new NormalResult(0, strError);
                }
            }

            return new NormalResult(1, null);
        }

        static string ParseTimestamp(byte[] timestamp)
        {
            if (timestamp == null)
                return "";
            if (timestamp.Length < 16)
                return "(length<16)";
            var ticks = BitConverter.ToInt64(timestamp, 0);
            DateTime time = new DateTime(ticks).ToLocalTime();
            var length = BitConverter.ToInt64(timestamp, 8);
            return $"time={time.ToString()},length={length}";
        }

        // 获得资源。包装版本 -- 返回字符串版本、Cache版本。
        // parameters:
        //      remote_timestamp    远端时间戳。如果为 null，表示要从服务器实际获取时间戳
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public static long GetRes(this LibraryChannel channel,
            DigitalPlatform.Stop stop,
            CfgCache cache,
            string strPath,
            string strStyle,
            byte[] remote_timestamp,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            strError = "";
            strResult = "";
            strMetaData = "";
            baOutputTimeStamp = null;
            strOutputResPath = "";

            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            long lRet = 0;

            string strFullPath = channel.Url + "?" + strPath;

            if (StringUtil.IsInList("forceget", strStyle) == true)
            {
                // 强制获取
                StringUtil.RemoveFromInList("forceget",
                    true,
                    ref strStyle);
                goto GETDATA;
            }

            // 从cache中得到timestamp
            // return:
            //      -1  error
            //		0	not found
            //		1	found
            int nRet = cache.FindLocalFile(strFullPath,
                out strLocalName,
                out strTimeStamp);
            if (nRet == -1)
            {
                strError = "CfgCache 尚未初始化";
                return -1;
            }
            if (nRet == 1)
            {
                Debug.Assert(strLocalName != "", "FindLocalFile()返回的strLocalName为空");

                if (strTimeStamp == "")
                    goto GETDATA;   // 时间戳不对, 那就只好重新获取服务器端内容

                Debug.Assert(strTimeStamp != "", "FindLocalFile()获得的strTimeStamp为空");
                cached_timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
                // bExistInCache = true;
            }
            else
                goto GETDATA;

            if (remote_timestamp == null)
            {
                // 探测时间戳关系
                string strNewStyle = strStyle;

                StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
                    true,
                    ref strNewStyle);   // 不要数据体和metadata

                lRet = channel.GetRes(stop,
                    strPath,
                    strNewStyle,
                    out strResult,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputResPath,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
            else
                baOutputTimeStamp = remote_timestamp;

            // 如果证明timestamp没有变化, 但是本次并未返回内容,则从cache中取原来的内容

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)    // 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");
                try
                {
                    using (StreamReader sr = new StreamReader(strLocalName, Encoding.UTF8))
                    {
                        strResult = sr.ReadToEnd();
                        // TODO: 这里返回 0 似乎不严谨。但是否应该返回 byte[] 的 Length 呢？
                        return 0;   // 以无错误姿态返回
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

        GETDATA:

            // 重新正式获取内容
            lRet = channel.GetRes(
                stop,
                strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 因为时间戳不匹配而新获得了内容
            // 保存到cache
            cache.PrepareLocalFile(strFullPath, out strLocalName);
            Debug.Assert(strLocalName != "", "PrepareLocalFile()返回的strLocalName为空");

            // 写入文件,以便以后从cache获取
            using (StreamWriter sw = new StreamWriter(strLocalName,
                false,  // append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strResult);
            }

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;

            return lRet;
        }

        // 获得资源。包装版本 -- 返回本地映射文件、Cache版本。
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public static long GetResLocalFile(this LibraryChannel channel,
            DigitalPlatform.Stop stop,
            CfgCache cache,
            string strPath,
            string strStyle,
            out string strOutputFilename,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            strOutputFilename = "";

            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            string strResult = "";

            string strFullPath = channel.Url + "?" + strPath;

            if (StringUtil.IsInList("forceget", strStyle) == true)
            {
                // 强制获取
                StringUtil.RemoveFromInList("forceget",
                    true,
                    ref strStyle);
                goto GETDATA;
            }

            // 从cache中得到timestamp
            // return:
            //      -1  error
            //		0	not found
            //		1	found
            int nRet = cache.FindLocalFile(strFullPath,
                out strLocalName,
                out strTimeStamp);
            if (nRet == -1)
            {
                strOutputResPath = "";
                strMetaData = "";
                baOutputTimeStamp = null;
                strError = "CfgCache 尚未初始化";
                return -1;
            }
            if (nRet == 1)
            {
                Debug.Assert(strLocalName != "", "FindLocalFile()返回的strLocalName为空");

                if (strTimeStamp == "")
                    goto GETDATA;   // 时间戳不对, 那就只好重新获取服务器端内容

                Debug.Assert(strTimeStamp != "", "FindLocalFile()获得的strTimeStamp为空");
                cached_timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
                // bExistInCache = true;
            }
            else
                goto GETDATA;

            // 探测时间戳关系
            string strNewStyle = strStyle;

            /*
            StringUtil.RemoveFromInList("data",
                true,
                ref strNewStyle);	// 不要数据体
             * */
            StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
    true,
    ref strNewStyle);   // 不要数据体和metadata

            long lRet = channel.GetRes(stop,
                strPath,
                strNewStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 如果证明timestamp没有变化, 但是本次并未返回内容,则从cache中取原来的内容

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)    // 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                strOutputFilename = strLocalName;
                // TODO: 这里返回 0 似乎不严谨。但是否应该返回 byte[] 的 Length 呢？
                return 0;   // 以无错误姿态返回
            }

        GETDATA:
            // 重新正式获取内容
            lRet = channel.GetRes(
                stop,
                strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 因为时间戳不匹配而新获得了内容
            // 保存到cache
            cache.PrepareLocalFile(strFullPath, out strLocalName);
            Debug.Assert(strLocalName != "", "PrepareLocalFile()返回的strLocalName为空");

            // 写入文件,以便以后从cache获取
            using (StreamWriter sw = new StreamWriter(strLocalName,
                false,  // append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strResult);
            }

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;

            strOutputFilename = strLocalName;
            return lRet;
        }
    }
}
