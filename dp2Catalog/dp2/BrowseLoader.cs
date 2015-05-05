using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DigitalPlatform.CirculationClient;
using DigitalPlatform;
using System.Diagnostics;

namespace dp2Catalog
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

        public LibraryChannelCollection Channels
        {
            get;
            set;
        }

        public dp2ServerCollection Servers
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
            // 首先按照服务器名的不同，划分为若干个区段
            List<OneBatch> batchs = new List<OneBatch>();

            {
                OneBatch batch = new OneBatch();
                for (int index = 0; index < m_recpaths.Count; index++)
                {
                    string s = m_recpaths[index];

                    // 解析记录路径
                    string strServerName = "";
                    string strPurePath = "";
                    dp2SearchForm.ParseRecPath(s,
                        out strServerName,
                        out strPurePath);

                    // 服务器名发生变化了
                    if (batch.Count > 0 && strServerName != batch.ServerName)
                    {
                        batchs.Add(batch);
                        batch = new OneBatch();
                        batch.ServerName = strServerName;
                        batch.Add(strPurePath);
                        continue;
                    }

                    if (string.IsNullOrEmpty(batch.ServerName) == true)
                        batch.ServerName = strServerName;

                    batch.Add(strPurePath);
                }

                if (batch.Count > 0)
                    batchs.Add(batch);
            }

            // 进行循环获取
            foreach (OneBatch temp in batchs)
            {
                // 获得server url
                dp2Server server = this.Servers.GetServerByName(temp.ServerName);
                if (server == null)
                {
                    string strError = "名为 '" + temp.ServerName + "' 的服务器在检索窗中尚未定义...";
                    throw new Exception(strError);
                }
                string strServerUrl = server.Url;

                LibraryChannel channel = this.Channels.GetChannel(strServerUrl);

                List<string> batch = new List<string>();
                for (; batch.Count > 0 || temp.Count > 0; )
                {

                    if (batch.Count == 0)
                    {
                        for (int i = 0; i < Math.Min(temp.Count, 100); i++)
                        {
                            batch.Add(temp[i]);
                        }
                        temp.RemoveRange(0, batch.Count);
                    }

                    // 每100个一批
                    if (batch.Count > 0)
                    {
                    REDO:
                        string[] paths = new string[batch.Count];
                        batch.CopyTo(paths);

                        DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;
                        string strError = "";

                        long lRet = channel.GetBrowseRecords(
                            this.Stop,
                            paths,
                            "id,cols",
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            throw new Exception(strError);


                        if (searchresults == null)
                        {
                            strError = "searchresults == null";
                            throw new Exception(strError);
                        }

                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            DigitalPlatform.CirculationClient.localhost.Record record = searchresults[i];
                            Debug.Assert(batch[i] == record.Path, "");

                            // 包含服务器名
                            record.Path = record.Path + "@" + temp.ServerName;
                            yield return record;

                        }

                    CONTINUE:
                        if (batch.Count > searchresults.Length)
                        {
                            // 有本次没有获取到的记录
                            batch.RemoveRange(0, searchresults.Length);
                            /*
                            if (index == m_recpaths.Count - 1)
                                goto REDO;  // 当前已经是最后一轮了，需要继续做完
                             * */

                            // 否则可以留给下一轮处理
                        }
                        else
                            batch.Clear();
                    }
                }
            }
        }
    }
}
