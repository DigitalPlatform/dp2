using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    public class OperLogDatesLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        List<string> m_filterdates = null;

        /// <summary>
        /// 用于筛选的日期集合。用于筛选从服务器获得的日期
        /// 每个文件名最好是 8 字符的日期形态。如果多于 8 字符，在使用时候多余部分会被丢弃
        /// </summary>
        public List<string> FilterDates
        {
            get
            {
                return this.m_filterdates;
            }
            set
            {
                this.m_filterdates = value;
            }
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

        public IEnumerator GetEnumerator()
        {
            string strError = "";

            if ((this.LogType & LogType.AccessLog) != 0
&& (this.LogType & LogType.OperLog) != 0)
                throw new ArgumentException("OperLogLoader 的 LogType 只能使用一种类型");

            List<string> filter_dates = null;
            if (this.FilterDates != null)
            {
                // 丢弃文件扩展名部分
                filter_dates = new List<string>();
                foreach (string date in this.FilterDates)
                {
                    filter_dates.Add(date.Substring(0, 8));
                }
            }

            long lStart = 0;
            int nPerCount = -1;
            long lHitCount = 0;
            for (; ; )
            {

                if (this.Stop != null && this.Stop.State != 0)
                {
                    strError = "用户中断";
                    throw new InterruptException(strError);
                }

                REDO:
                OperLogInfo[] records = null;
                long lRet = this.Channel.GetOperLogs(this.Stop,
                    "",
                    lStart,
                    -1,
                    nPerCount,
                    "getfilenames",
                    "",
                    out records,
                    out strError);
                /*
                // testring
                lRet = -1;
                strError = "测试文字";
                */

                if (lRet == -1)
                {
                    if (this.Prompt != null)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = "获取日志文件名的操作发生错误： " + strError;
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
                            // 还没有得到文件名，通讯就失败，所以操作无法进行了，只能抛出异常
                            throw new ChannelException(this.Channel.ErrorCode, strError);
                            // continue;
                        }
                    }
                    else
                        throw new ChannelException(this.Channel.ErrorCode, strError);
                }

                lHitCount = lRet;

                if (records != null)
                {
                    foreach (OperLogInfo info in records)
                    {
                        // testing
                        // info.Xml = null;

                        string strDate = info.Xml.Substring(0, 8);
                        if (filter_dates != null
                            && filter_dates.IndexOf(strDate) == -1)
                            continue;

                        OperLogDateItem item = new OperLogDateItem();
                        item.Date = strDate;
                        item.Length = info.AttachmentLength;
                        yield return item;
                    }
                }

                int length = 0;
                if (records != null)
                    length = records.Length;

                if (lStart + length >= lHitCount)
                    yield break;
                lStart += length;
            }
        }
    }

    /// <summary>
    /// 日志信息事项
    /// </summary>
    public class OperLogDateItem
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 日志记录在文件中的序号
        /// </summary>
        public long Index = -1;

        public long Length { get; set; }

#if NO
        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;

        /// <summary>
        /// 错误信息字符串
        /// </summary>
        public string ErrorInfo = "";
#endif
    }
}
