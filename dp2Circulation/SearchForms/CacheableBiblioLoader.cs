using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 带有缓存的书目记录枚举器
    /// 注意每次使用时事项不能太多，避免 Hashtable 体积太大
    /// 在向 dp2library 请求 API 的时候，能自动划分为适当的批，多次进行
    /// </summary>
    internal class CacheableBiblioLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        public List<string> RecPaths
        {
            get;
            set;
        }

        public string Format
        {
            get
            {
                return this.m_loader.Format;
            }
            set
            {
                this.m_loader.Format = value;
            }
        }

        public LibraryChannel Channel
        {
            get
            {
                return this.m_loader.Channel;
            }
            set
            {
                this.m_loader.Channel = value;
            }
        }

        public Stop Stop
        {
            get
            {
                return this.m_loader.Stop;
            }
            set
            {
                this.m_loader.Stop = value;
            }
        }

        public GetBiblioInfoStyle GetBiblioInfoStyle
        {
            get
            {
                return this.m_loader.GetBiblioInfoStyle;
            }
            set
            {
                this.m_loader.GetBiblioInfoStyle = value;
            }
        }

        /// <summary>
        /// 用于缓存的 Hashtable 对象
        /// </summary>
        public Hashtable CacheTable
        {
            get;
            set;
        }

        BiblioLoader m_loader = new BiblioLoader();

        /// <summary>
        /// 获得枚举接口
        /// </summary>
        /// <returns>枚举接口</returns>
        public IEnumerator GetEnumerator()
        {
            Debug.Assert(m_loader != null, "");

            if (this.Prompt != null)
                m_loader.Prompt += m_loader_Prompt;
            try
            {
#if NO
            if (this.CacheTable == null)
                this.CacheTable = new Hashtable();  // 如果调主没有给出 CacheTable， 则临时分配一个
#endif
                Hashtable tempTable = new Hashtable();

                List<string> new_recpaths = new List<string>(); // 缓存中没有包含的那些记录
                foreach (string strRecPath in this.RecPaths)
                {
                    if (string.IsNullOrEmpty(strRecPath))
                        throw new ArgumentException("RecPaths 中不应包含空元素");

                    Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                    BiblioItem info = null;
                    if (this.CacheTable != null)
                    {
                        info = (BiblioItem)this.CacheTable[strRecPath];
                        if (info != null)
                        {
                            tempTable[strRecPath] = info;
                            continue;
                        }
                    }
                    if (info == null)
                        info = (BiblioItem)tempTable[strRecPath];   // 注： tempTable 自带去重效果

                    if (info == null)
                    {
                        new_recpaths.Add(strRecPath);

#if NO
                    // 需要放入缓存，便于后面的发现
                    // 但放入缓存的是 .Content 为空的对象
                    if (info == null)
                    {
                        info = new BiblioItem();
                        info.RecPath = strRecPath;
                    }
                    this.CacheTable[strRecPath] = info;
#endif
                        info = new BiblioItem();
                        info.RecPath = strRecPath;
                        tempTable[strRecPath] = info;
                    }
                }

                // 注： Hashtable 在这一段时间内不应该被修改。否则会破坏 m_loader 和 items 之间的锁定对应关系

                m_loader.RecPaths = new_recpaths;

                var enumerator = m_loader.GetEnumerator();

                // 开始循环
                foreach (string strRecPath in this.RecPaths)
                {
                    Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                    BiblioItem info = null;

#if NO
                if (this.CacheTable != null)
                    info = (BiblioItem)this.CacheTable[strRecPath];
#endif
                    info = (BiblioItem)tempTable[strRecPath];   // 注： tempTable 自带去重效果
                    if (info != null && string.IsNullOrEmpty(info.Content) == false)
                    {
                        yield return info;
                        continue;
                    }

                    if (new_recpaths.IndexOf(strRecPath) != -1)
                    // if (info == null || string.IsNullOrEmpty(info.Content) == true)
                    {
                        bool bRet = enumerator.MoveNext();
                        if (bRet == false)
                        {
                            Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                            // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                            yield break;
                        }

                        BiblioItem biblio = (BiblioItem)enumerator.Current;
                        Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                        // 需要放入缓存
                        if (info == null)
                        {
                            info = new BiblioItem();
                            info.RecPath = biblio.RecPath;
                        }
                        if (string.IsNullOrEmpty(biblio.Content) == true)
                        {
                            // info.Content = "{null}";    // 2013/11/18
                            info.Contents = new List<string> { "{null}" };
                        }
                        else
                        {
                            // info.Content = biblio.Content;
                            info.Contents = new List<string> { biblio.Content };
                        }
                        info.Metadata = biblio.Metadata;
                        info.Timestamp = biblio.Timestamp;
                        if (tempTable.ContainsKey(strRecPath) == false)
                            tempTable[strRecPath] = info;
                        if (this.CacheTable != null)
                        {
                            if (this.CacheTable.ContainsKey(strRecPath) == false)
                                this.CacheTable[strRecPath] = info;
                        }
                        yield return info;
                    }
                    else
                    {
                        info = (BiblioItem)tempTable[strRecPath];   // 注： tempTable 自带去重效果
                        if (info == null)
                            throw new Exception("tempTable 里面没有找到 '" + strRecPath + "'");
                        yield return info;
                    }
                }
            }
            finally
            {
                if (this.Prompt != null)
                    m_loader.Prompt -= m_loader_Prompt;
            }
        }

        // 2017/6/16
        void m_loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            MessagePromptEventHandler handler = this.Prompt;
            if (handler != null)
                handler(sender, e);
        }
    }
}
