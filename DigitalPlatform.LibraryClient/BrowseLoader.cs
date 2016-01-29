using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 快速获得浏览行信息
    /// </summary>
    public class BrowseLoader : IEnumerable
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
#if NO
                    string[] paths = new string[batch.Count];
                    batch.CopyTo(paths);
#endif
                    string [] paths = batch.ToArray();

                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;
                    string strError = "";

                    long lRet = Channel.GetBrowseRecords(
                        this.Stop,
                        paths,
                        this.Format,    // "id,cols",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        throw new Exception(strError);
#if NO
                    if (lRet == -1)
                    {
                        if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                        {
                            foreach (string path in batch)
                            {
                                DigitalPlatform.LibraryClient.localhost.Record record = new DigitalPlatform.LibraryClient.localhost.Record();
                                record.Path = path;
                                // TODO: 是否需要设置 ErrorCode ?
                                yield return record;
                            }
                            goto CONTINUE;
                        }

                        // 如果results.Length表现正常，其实还可以继续处理?
                        if (searchresults != null && searchresults.Length > 0)
                        {
                        }
                        else
                        {
                            strError = "获得浏览记录 '" + StringUtil.MakePathList(batch) + "' 时发生错误: " + strError;
                            throw new Exception(strError);
                        }
                    }
#endif

                    if (searchresults == null)
                    {
                        strError = "searchresults == null";
                        throw new Exception(strError);
                    }

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record record = searchresults[i];
                        if (batch[i] != record.Path)
                        {
                            throw new Exception("下标 "+i+" 的 batch 元素 '"+batch[i]+"' 和返回的该下标位置 GetBrowseRecords() 结果路径 '"+record.Path+"' 不匹配");
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
