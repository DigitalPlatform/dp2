using DigitalPlatform.rms.Client.rmsws_localhost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            long lHitCount = -1;
            long lStart = 0;
            long nPerCount = this.BatchSize == 0 ? -1 : this.BatchSize;
            nPerCount = 1;  // test
            for (; ; )
            {
                Record[] searchresults = null;

                long lRet = this.Channel.DoGetSearchResult(
                    this.ResultSetName, // "default",
                    lStart,
                    nPerCount,
                    this.FormatList,  // "id,xml,timestamp,metadata",
                    string.IsNullOrEmpty(this.Lang) ? "zh" : this.Lang,
                    this.Stop,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
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

                foreach (Record record in searchresults)
                {
                    if (string.IsNullOrEmpty(this.ElementType) == true
                        || this.ElementType == "KernelRecord")
                    {
                        KernelRecord k = KernelRecord.From(record);
                        yield return k;
                    }
                    else
                        yield return record;
                }

                lStart += searchresults.Length;
                if (lStart >= lHitCount)
                    yield break;
            }
        ERROR1:
            throw new Exception(strError);
        }

    }
}
