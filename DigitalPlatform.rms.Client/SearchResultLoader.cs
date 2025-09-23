using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.rms.Client
{
    // 2016/5/6
    /// <summary>
    /// 检索命中结果的枚举器
    /// </summary>
    public class SearchResultLoader : IEnumerable
    {
        public string ElementType { get; set; } // KernelRecord / Record。默认 KernelRecord

        public string ResultSetName { get; set; }

        public RmsChannel Channel { get; set; }

        public DigitalPlatform.Stop Stop { get; set; }

        public string FormatList
        {
            get;
            set;
        }

        public string Lang { get; set; }

        // 每批获取最多多少个记录
        public long BatchSize { get; set; }

        // 2021/9/12
        // 最多获取的记录数。若 HitCount 超过 MaxSize，则只获取 MaxSize 个记录
        // 0 或 -1 都表示不限制
        public long MaxResultCount { get; set; }

        // 2021/9/12
        // 在 GetSearchResult() 第一次调用以后获得 dp2kernel 一端结果集中实际存在的记录数
        public long ResultCount { get; set; }

        // 2025/9/12
        // 获取时的起始偏移量
        public long Start { get; set; }

        public SearchResultLoader(RmsChannel channel,
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

            this.ResultCount = 0;
            long lHitCount = -1;
            long lStart = this.Start;
            long nPerCount = this.BatchSize == 0 ? -1 : this.BatchSize;
            // nPerCount = 1;  // test
            for (; ; )
            {
                Record[] searchresults = null;

                long length = nPerCount;

                // 需要明确限制每批获得的最多记录数，避免超过 MaxSize
                if (this.MaxResultCount > 0)
                {
                    if (length == -1)
                        length = this.MaxResultCount/* - lStart*/;
                    else if (/*lStart + */length > this.MaxResultCount)
                        length = this.MaxResultCount/* - lStart*/;
                }

                long lRet = this.Channel.DoGetSearchResult(
                    this.ResultSetName, // "default",
                    lStart,
                    length, // nPerCount,
                    this.FormatList,  // "id,xml,timestamp,metadata",
                    string.IsNullOrEmpty(this.Lang) ? "zh" : this.Lang,
                    this.Stop,
                    out searchresults,
                    out strError);

                this.ResultCount = lRet;

                if (lRet == -1)
                    goto ERROR1;
                // 2017/5/3
                if (lRet == 0)
                    yield break;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    goto ERROR1;
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    goto ERROR1;
                }
                lHitCount = lRet;

                // 2021/9/12
                if (MaxResultCount > 0 && lHitCount > MaxResultCount)
                    lHitCount = MaxResultCount;

                foreach (Record record in searchresults)
                {
                    if (string.IsNullOrEmpty(this.ElementType) == true
                        || this.ElementType == "KernelRecord")
                    {
                        KernelRecord k = KernelRecord.From(record);
                        k.Url = this.Channel.Url;   // 2017/5/3
                        yield return k;
                    }
                    else
                        yield return record;
                }

                lStart += searchresults.Length;
                if (lStart >= lHitCount)
                    yield break;
            }
            // ???
        ERROR1:
            throw new Exception(strError);
        }

    }
}
