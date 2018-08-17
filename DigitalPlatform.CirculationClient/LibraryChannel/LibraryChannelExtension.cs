using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Range;

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
            out output_timestamp,
            out strError);
        }

        public static int UploadObject(
    this LibraryChannel channel,
    Stop stop,
    string strClientFilePath,
    string strServerFilePath,
    string strStyle,
    byte[] timestamp,
    bool bRetryOverwiteExisting,
    bool bProgressChange,
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
            out output_timestamp,
            out strError);
        }


        // 上传文件到到 dp2lbrary 服务器
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
                strRange = "0-" + Convert.ToString(fi.Length - 1);

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
                        //if (Program.MainForm.InvokeRequired == false)
                        //    Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strWaiting = "";
                        if (j == ranges.Length - 1)
                        {
                            strWaiting = " 请耐心等待...";
                            channel.Timeout = new TimeSpan(0, 40, 0);   // 40 分钟
                        }

                        string strPercent = "";
                        RangeList rl = new RangeList(ranges[j]);
                        if (rl.Count >= 1)
                        {
                            double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                            strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                        }

                        if (stop != null)
                        {
                            stop.SetMessage( // strMessagePrefix + 
                                "正在上载 " + ranges[j] + "/"
                                + StringUtil.GetLengthText(fi.Length)
                                + " " + strPercent + " " + strClientFilePath + strWarning + strWaiting);
                            if (bProgressChange && rl.Count > 0)
                                stop.SetProgressValue(rl[0].lStart);
                        }

                        int nRedoCount = 0;
                    REDO:
                        long lRet = channel.SaveResObject(
        stop,
        strResPath,
        stream, // strClientFilePath,
        -1,
        j == ranges.Length - 1 ? strMetadata : null,	// 最尾一次操作才写入 metadata
        ranges[j],
        // j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
        timestamp,
        strStyle,
        out output_timestamp,
        out strError);
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
                            return -1;
                        }
                    }
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
                    goto GETDATA;	// 时间戳不对, 那就只好重新获取服务器端内容

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
                    ref strNewStyle);	// 不要数据体和metadata

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

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)	// 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                try
                {
                    using (StreamReader sr = new StreamReader(strLocalName, Encoding.UTF8))
                    {
                        strResult = sr.ReadToEnd();
                        return 0;	// 以无错误姿态返回
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
                false,	// append
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
                    goto GETDATA;	// 时间戳不对, 那就只好重新获取服务器端内容

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
    ref strNewStyle);	// 不要数据体和metadata

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

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)	// 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                strOutputFilename = strLocalName;
                return 0;	// 以无错误姿态返回
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
                false,	// append
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
