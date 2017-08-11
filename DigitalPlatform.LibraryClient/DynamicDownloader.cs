using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 用于下载动态增长的文件的下载器
    /// </summary>
    public class DynamicDownloader
    {
        CancellationToken _token = new CancellationToken();

        public string State { get; set; }

        // 服务器端的文件路径
        public string ServerFilePath { get; set; }

        // 本地文件路径
        public string LocalFilePath { get; set; }

        public LibraryChannel Channel { get; set; }

        public Stop Stop { get; set; }

        FileStream _stream = null;

        public DynamicDownloader(
            LibraryChannel channel,
            string strServerFilePath,
            string strOutputFileName)
        {
            this.Channel = channel;
            this.ServerFilePath = strServerFilePath;
            this.LocalFilePath = strOutputFileName;
        }

        void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }

        public void StartDownload(bool bContinue)
        {
            // 创建输出文件
            this.Close();

            if (bContinue == false)
                _stream = File.Create(this.LocalFilePath);
            else
            {
                _stream = File.Open(this.LocalFilePath, FileMode.OpenOrCreate);
                _stream.Seek(0, SeekOrigin.End);
            }

            Task.Factory.StartNew(() => Download(),
    CancellationToken.None,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

        void Download()
        {
            string strError = "";

            string strPath = this.ServerFilePath;
            string strStyle = "content,data,metadata,timestamp,outputpath";

            byte[] baContent = null;

            long lStart = _stream.Length;
            int nPerLength = -1;

            byte[] old_timestamp = null;
            byte[] timestamp = null;

            long lTotalLength = -1;

            for (; ; )
            {
                if (_token.IsCancellationRequested)
                {
                    strError = "中断";
                    goto ERROR1;
                }
                // Application.DoEvents();	// 出让界面控制权

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
                    goto ERROR1;

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
                    Debug.Assert(StringUtil.IsInList("content", strStyle) == true,
                        "不是attachment风格，就应是content风格");

                    Debug.Assert(baContent != null, "返回的baContent不能为null");
                    Debug.Assert(baContent.Length <= lRet, "每次返回的包尺寸[" + Convert.ToString(baContent.Length) + "]应当小于result.Value[" + Convert.ToString(lRet) + "]");

                    _stream.Write(baContent, 0, baContent.Length);
                    _stream.Flush(); // 2013/5/17
                    lStart += baContent.Length;

                    if (lRet > 0)
                    {
                        Debug.Assert(baContent.Length > 0, "");
                    }
                }

                if (lStart >= lRet)
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
                    if (strState == "finish")
                    {
                        this.State = "finish";
                        return;
                    }

                    // 休眠一段时间后重试下载
                    Thread.Sleep(1000);
                }
            } // end of for

            this.State = "end";
            return;
        ERROR1:
            this.State = "error";
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

            string strPath = strServerPath + ".state";

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
            }

            return 0;
        }

    }
}
