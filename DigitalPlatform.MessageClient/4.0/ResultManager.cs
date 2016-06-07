using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// 从 Chord 项目复制代码而来

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// 广播式检索结果的管理器
    /// 能判断出响应的数据是否已经完全到来
    /// </summary>
    public class ResultManager
    {
        public long _targetCount = -1;  // -1 表示目标对象数量未知

        Hashtable _targetTable = new Hashtable();   // target id --> HitInfo

        // return:
        //      false   响应还没有完全到来
        //      true    响应已经完全到来
        public bool SetTargetCount(long count)
        {
            _targetCount = count;
            return IsAllCompleted();
        }

        public long GetTotalCount()
        {
            // TODO: 如果 -targetCount 为 -1，此时就无法探测准确的总数。不准确的可以作为负数返回?
            long count = 0;
            foreach (string targetid in _targetTable.Keys)
            {
                HitInfo info = _targetTable[targetid] as HitInfo;
                if (info.TotalResults >= 1)
                    count += Math.Max(info.Recieved, info.TotalResults);    // 有时候弄错了 libraryUID 的情况下，可能都归到一个对象，导致 recieved 数字大于 totalResults 数字
            }

            return count;
        }

        public bool IsAllCompleted()
        {
            if (_targetCount == -1)
                return false;   // 暂时无法判断

            int complete_count = 0; // 已经完成的对象个数
#if NO
            bool bOverflow = false; // 是否有 recieved > totalResults 的情况
            long total_count = 0;   // 总命中的记录数累计
            long recieve_count = 0; // 收到的记录数累计
#endif
            foreach (string targetid in _targetTable.Keys)
            {
                HitInfo info = _targetTable[targetid] as HitInfo;

#if NO
                if (info.TotalResults > 0)
                    total_count += info.TotalResults;
                recieve_count += info.Recieved;

                if (info.TotalResults >= 1 && info.Recieved > info.TotalResults)
                    bOverflow = true;
#endif

                if (info.Recieved >= info.TotalResults)
                    complete_count++;


            }

            // 第一种判断法
            if (complete_count >= _targetCount)
                return true;

#if NO
            // 第二种判断法
            if (bOverflow == true && recieve_count >= total_count)
                return true;
#endif

            return false;
        }

        // 标记结束一个检索目标
        // return:
        //      0   尚未结束
        //      1   结束
        //      2   全部结束
        public int CompleteTarget(string strLibraryUID, long total_count, long this_count)
        {
            HitInfo info = _targetTable[strLibraryUID] as HitInfo;
            if (info == null)
            {
                info = new HitInfo();
                info.TotalResults = total_count;
                _targetTable[strLibraryUID] = info;
            }
            info.Recieved += this_count;
            Debug.WriteLine(strLibraryUID + "\r\n" + info.Dump());
            if (info.Recieved >= info.TotalResults)
            {
                if (IsAllCompleted())
                    return 2;
                return 1;
            }
            return 0;
        }

    }

    // 一次检索命中的信息
    public class HitInfo
    {
        public long TotalResults = 0;   // 总的命中数量。-1 表示已出错
        public long Recieved = 0;   // 已经收到的数量

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TotalResults=" + this.TotalResults + "\r\n");
            text.Append("Recieved=" + this.Recieved + "\r\n");
            return text.ToString();
        }
    }

}
