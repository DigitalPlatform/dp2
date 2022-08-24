using System;
using System.Collections;
using System.Diagnostics;

using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 枚举一个书目记录下属的全部下级记录
    /// </summary>
    public class SubItemLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

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

        public long TotalCount { get; set; }    // TODO: 可改进为，如果在没有枚举以前访问此成员，则触发一次不返回记录的请求

        public static long DefaultBatchSize = 100;

        // 是否把“书目库下没有定义实体库”错误当作错误处理
        bool _itemDbNotDefAsError = true;
        public bool ItemDbNotDefAsError
        {
            get
            {
                return _itemDbNotDefAsError;
            }
            set
            {
                _itemDbNotDefAsError = value;
            }
        }

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

                    int nRedoCount = 0;
                REDO:
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
                        throw new Exception("未知的 this.DbType '" + this.DbType + "'");

                    if (lRet == -1)
                    {
                        if (this.Stop != null && this.Stop.State != 0)
                            throw new InterruptException("用户中断");

                        if (this.Channel.ErrorCode == ErrorCode.ItemDbNotDef)
                        {
                            yield break;
                        }

                        if (this.Prompt != null)
                        {
                            MessagePromptEventArgs e = new MessagePromptEventArgs();
                            e.MessageText = "获得书目记录 '" + this.BiblioRecPath + "' 的下级 " + this.DbType + " 记录时发生错误： " + strError;
                            e.Actions = "yes,no,cancel";
                            this.Prompt(this, e);
                            if (e.ResultAction == "cancel")
                                throw new ChannelException(Channel.ErrorCode, strError);
                            else if (e.ResultAction == "yes")
                                goto REDO;
                            else
                            {
                                /*
                                // no 也是抛出异常。因为继续下一批代价太大
                                throw new ChannelException(Channel.ErrorCode, strError);
                                */
                                yield break;    // 2022/8/24 跳过本批
                            }
                        }
                        else
                            throw new ChannelException(Channel.ErrorCode, strError);
                    }

                    // 发现 dp2library GetEntities() API 非常偶然地会在 entities 数组中返回一个 info.OldRecPath 为空的元素。这里试图重试获取
                    // 2017/6/5
                    // return:
                    //      true    出现了反常的情况
                    //      false   没有发现反常的情况
                    if (VerifyItems(entities) == true
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    lResultCount = lRet;
                    this.TotalCount = lRet; // 记载下级记录总数

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

        // 2017/6/5
        // return:
        //      true    出现了反常的情况
        //      false   没有发现反常的情况
        static bool VerifyItems(EntityInfo[] entities)
        {
            if (entities == null)
                return false;
            foreach (EntityInfo info in entities)
            {
                if (string.IsNullOrEmpty(info.OldRecPath))
                    return true;
            }

            return false;
        }
    }
}
