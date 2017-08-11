using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.rms.Client
{
    public class RmsBrowseLoader : IEnumerable
    {
        List<string> m_recpaths = new List<string>();

        public List<string> RecPaths
        {
            get
            {
                return this.m_recpaths;
            }
            set
            {
                this.m_recpaths = value;
            }
        }

        public string Format
        {
            get;
            set;
        }

        public RmsChannel Channel
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            List<string> batch = new List<string>();
            for (int index = 0; index < m_recpaths.Count; index++)
            {
                string s = m_recpaths[index];
                batch.Add(s);

                // 每100个一批，或者最后一次
                if (batch.Count >= 100 ||
                    (index == m_recpaths.Count - 1 && batch.Count > 0))
                {
                REDO:
                    string[] paths = batch.ToArray();

                    Record[] searchresults = null;
                    string strError = "";

                    long lRet = Channel.GetBrowseRecords(
                        paths,
                        this.Format,    // "id,cols",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        throw new Exception(strError);
                        //  throw new ChannelException(Channel.ErrorCode, strError);
                    }

                    if (searchresults == null)
                    {
                        strError = "searchresults == null";
                        throw new Exception(strError);
                    }

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        Record record = searchresults[i];
                        if (batch[i] != record.Path)
                        {
                            throw new Exception("下标 " + i + " 的 batch 元素 '" + batch[i] + "' 和返回的该下标位置 GetBrowseRecords() 结果路径 '" + record.Path + "' 不匹配");
                        }
                        Debug.Assert(batch[i] == record.Path, "");
                        yield return record;
                    }

                    // CONTINUE:
                    if (batch.Count > searchresults.Length)
                    {
                        // 有本次没有获取到的记录
                        batch.RemoveRange(0, searchresults.Length);
                        if (index == m_recpaths.Count - 1)
                            goto REDO;  // 当前已经是最后一轮了，需要继续做完

                        // 否则可以留给下一轮处理
                    }
                    else
                        batch.Clear();
                }
            }
        }
    }

}
