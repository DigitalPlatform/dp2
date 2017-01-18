using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 枚举一个书目记录下属的全部下级记录
    /// </summary>
    public class SubItemLoader : IEnumerable
    {
        public string DbType { get; set; }  // item/order/issue/comment 之一

        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string BiblioRecPath
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

        public long BatchSize { get; set; }

        public static long DefaultBatchSize = 100;

        public IEnumerator GetEnumerator()
        {
            string strError = "";

            long lPerCount = this.BatchSize; // 每批获得多少个
            if (lPerCount == 0)
                lPerCount = DefaultBatchSize;
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;

            TimeSpan old_timeout = this.Channel.Timeout;
            this.Channel.Timeout = new TimeSpan(0, 5, 0);

            try
            {
                for (; ; )
                {
                    if (this.Stop != null && this.Stop.State != 0)
                        throw new InterruptException("用户中断");

                    EntityInfo[] entities = null;

                    long lRet = 0;

                    if (this.DbType == "item")
                    {
                        lRet = this.Channel.GetEntities(
                             this.Stop,
                             this.BiblioRecPath,
                             lStart,
                             lCount,
                             this.Format == null ? "" : this.Format,    // 为了回避 dp2library 2.102 以前版本的一个 bug
                             "zh",
                             out entities,
                             out strError);
                    }
                    else if (this.DbType == "order")
                    {
                        lRet = this.Channel.GetOrders(
                             this.Stop,
                             this.BiblioRecPath,
                             lStart,
                             lCount,
                             this.Format == null ? "" : this.Format,
                             "zh",
                             out entities,
                             out strError);
                    }
                    else if (this.DbType == "issue")
                    {
                        lRet = this.Channel.GetIssues(
                             this.Stop,
                             this.BiblioRecPath,
                             lStart,
                             lCount,
                             this.Format == null ? "" : this.Format,
                             "zh",
                             out entities,
                             out strError);
                    }
                    else if (this.DbType == "comment")
                    {
                        lRet = this.Channel.GetComments(
                             this.Stop,
                             this.BiblioRecPath,
                             lStart,
                             lCount,
                             this.Format == null ? "" : this.Format,
                             "zh",
                             out entities,
                             out strError);
                    }
                    else
                        throw new Exception("未知的 this.DbType '"+this.DbType+"'");

                    if (lRet == -1)
                        throw new ChannelException(Channel.ErrorCode, strError);

                    lResultCount = lRet;

                    if (lRet == 0)
                        yield break;

                    Debug.Assert(entities != null, "");

                    foreach (EntityInfo info in entities)
                    {
                        if (info.ErrorCode != ErrorCodeValue.NoError)
                        {
                            strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;
                            throw new Exception(strError);
                        }

                        yield return info;
                    }

                    lStart += entities.Length;
                    if (lStart >= lResultCount)
                        yield break;

                    if (lCount == -1)
                        lCount = lPerCount;

                    if (lStart + lCount > lResultCount)
                        lCount = lResultCount - lStart;
                }
            }
            finally
            {
                this.Channel.Timeout = old_timeout;
            }
        }
    }
}
