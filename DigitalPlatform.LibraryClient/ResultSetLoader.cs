using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    // 2017/5/6
    /// <summary>
    /// 检索命中结果的枚举器
    /// </summary>
    public class ResultSetLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        public string ResultSetName { get; set; }

        public LibraryChannel Channel { get; set; }

        public DigitalPlatform.Stop Stop { get; set; }

        public string FormatList
        {
            get;
            set;
        }

        public string Lang { get; set; }

        // 每批获取最多多少个记录
        public long BatchSize { get; set; }

        // 从什么偏移位置开始取
        public long Start { get; set; }

        public ResultSetLoader(LibraryChannel channel,
            DigitalPlatform.Stop stop,
            string resultsetName,
            string formatList,
            string lang = "zh")
        {
            this.Channel = channel;
            this.Stop = stop;
            this.ResultSetName = resultsetName;
            this.FormatList = formatList;
            this.Lang = lang;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";

            long lHitCount = -1;
            long lStart = this.Start;
            long nPerCount = this.BatchSize == 0 ? -1 : this.BatchSize;
            // nPerCount = 1;  // test
            for (; ; )
            {
                Record[] searchresults = null;

            REDO:
                long lRet = this.Channel.GetSearchResult(
                    this.Stop,
                    this.ResultSetName, // "default",
                    lStart,
                    nPerCount,
                    this.FormatList,  // "id,xml,timestamp,metadata",
                    string.IsNullOrEmpty(this.Lang) ? "zh" : this.Lang,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Prompt != null)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = "获得数据库记录时发生错误： " + strError;
                        e.Actions = "yes,no,cancel";
                        this.Prompt(this, e);
                        if (e.ResultAction == "cancel")
                            throw new ChannelException(Channel.ErrorCode, strError);
                        else if (e.ResultAction == "yes")
                            goto REDO;
                        else
                        {
                            // no 也是抛出异常。因为继续下一批代价太大
                            throw new ChannelException(Channel.ErrorCode, strError);
                        }
                    }
                    else
                        throw new ChannelException(Channel.ErrorCode, strError);
                }

                if (lRet == 0)
                    yield break;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    throw new Exception(strError);
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    throw new Exception(strError);
                }
                lHitCount = lRet;

                foreach (Record record in searchresults)
                {
                    yield return record;
                }

                lStart += searchresults.Length;
                if (lStart >= lHitCount)
                    yield break;
            }
        }

    }

}
