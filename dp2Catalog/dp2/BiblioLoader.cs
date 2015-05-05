using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DigitalPlatform.CirculationClient;
using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient.localhost;

namespace dp2Catalog
{
    /// <summary>
    /// 快速获得书目记录信息
    /// </summary>
    public class BiblioLoader : IEnumerable
    {
        List<string> m_recpaths = new List<string>();

        // 每个记录路径都是带有服务器名部分的
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

        public GetBiblioInfoStyle GetBiblioInfoStyle
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            List<string> format_list = new List<string>();
            int nContentIndex = format_list.Count;
            int nTimestampIndex = -1;
            int nMetadataIndex = -1;
            format_list.Add(this.Format);
            if ((this.GetBiblioInfoStyle & dp2Catalog.GetBiblioInfoStyle.Timestamp) != 0)
            {
                nTimestampIndex = format_list.Count;
                format_list.Add("timestamp");
            }
            if ((this.GetBiblioInfoStyle & dp2Catalog.GetBiblioInfoStyle.Metadata) != 0)
            {
                nMetadataIndex = format_list.Count;
                format_list.Add("metadata");
            }

            string[] formats = new string[format_list.Count];
            format_list.CopyTo(formats);

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
                        string strCommand = "@path-list:" + StringUtil.MakePathList(batch);

                        string[] results = null;
                        byte[] timestamp = null;
                        string strError = "";
                        long lRet = channel.GetBiblioInfos(
                            this.Stop,
                            strCommand,
                            "",
                            formats,
                            out results,
                            out timestamp,
                            out strError);
                        if (lRet == -1)
                            throw new Exception(strError);
                        if (lRet == 0)
                        {
                            if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                            {
                                foreach (string path in batch)
                                {
                                    BiblioItem item = new BiblioItem();
                                    item.RecPath = path + "@" + temp.ServerName;
                                    item.ErrorCode = ErrorCode.NotFound;
                                    item.ErrorInfo = "书目记录 '" + path + "' 不存在";
                                    yield return item;
                                }
                                goto CONTINUE;
                            }


                            // 如果results.Length表现正常，其实还可以继续处理
                            if (results != null && results.Length > 0)
                            {
                            }
                            else
                            {
                                strError = "获得书目记录 '" + StringUtil.MakePathList(batch) + "' 时发生错误: " + strError;
                                throw new Exception(strError);
                            }
                        }

                        if (results == null)
                        {
                            strError = "results == null";
                            throw new Exception(strError);
                        }

                        for (int i = 0; i < results.Length / formats.Length; i++)
                        {
                            BiblioItem item = new BiblioItem();
                            item.RecPath = batch[i] + "@" + temp.ServerName;
                            if (nContentIndex != -1)
                                item.Content = results[i * formats.Length + nContentIndex];
                            if (nTimestampIndex != -1)
                                item.Timestamp = ByteArray.GetTimeStampByteArray(results[i * formats.Length + nTimestampIndex]);
                            if (nMetadataIndex != -1)
                                item.Metadata = results[i * formats.Length + nMetadataIndex];
                            if (string.IsNullOrEmpty(item.Content) == true)
                            {
                                item.ErrorCode = ErrorCode.NotFound;
                                item.ErrorInfo = "书目记录 '" + item.RecPath + "' 不存在";
                            }
                            yield return item;

                        }

                    CONTINUE:
                        if (batch.Count > results.Length / formats.Length)
                        {
                            // 有本次没有获取到的记录
                            batch.RemoveRange(0, results.Length / formats.Length);
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

    // 属于同一个服务器的一批次路径
    class OneBatch : List<string>
    {
        public string ServerName = "";
        // public List<string> PureRecPaths = new List<string>();

        public string TailRecPath
        {
            get
            {
                if (this.Count == 0)
                    return null;
                return this[this.Count - 1];
            }
        }

    }

    public class BiblioItem
    {
        public string RecPath = "";
        public string Content = "";
        public byte[] Timestamp = null;
        public string Metadata = "";

        public ErrorCode ErrorCode = ErrorCode.NoError;
        public string ErrorInfo = "";
    }

    // 要获取哪些附加信息?
    public enum GetBiblioInfoStyle
    {
        None = 0,   // 不获取 Timestamp 和 Metadata 部分
        Timestamp = 0x01,   // 要获取 Timestamp 部分
        Metadata = 0x02,    // 要获取 Metadata 部分
    }

}
