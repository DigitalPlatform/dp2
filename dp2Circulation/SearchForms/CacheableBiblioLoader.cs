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
    /// </summary>
    internal class CacheableBiblioLoader : IEnumerable
    {
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

            if (this.CacheTable == null)
                this.CacheTable = new Hashtable();  // 如果调主没有给出 CacheTable， 则临时分配一个

            List<string> new_recpaths = new List<string>(); // 缓存中没有包含的那些记录
            foreach (string strRecPath in this.RecPaths)
            {
                Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                BiblioItem info = (BiblioItem)this.CacheTable[strRecPath];
                if (info == null)
                {
                    new_recpaths.Add(strRecPath);

                    // 需要放入缓存，便于后面的发现
                    // 但放入缓存的是 .Content 为空的对象
                    if (info == null)
                    {
                        info = new BiblioItem();
                        info.RecPath = strRecPath;
                    }
                    this.CacheTable[strRecPath] = info;
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
                
                if (this.CacheTable != null)
                    info = (BiblioItem)this.CacheTable[strRecPath];

                if (info == null || string.IsNullOrEmpty(info.Content) == true)
                {
#if NO
                    if (m_loader.Stop != null)
                    {
                        m_loader.Stop.SetMessage("正在获取书目记录 " + strRecPath);
                    }
#endif
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
                        info.Content = "{null}";    // 2013/11/18
                    else
                        info.Content = biblio.Content;
                    info.Metadata = biblio.Metadata;
                    info.Timestamp = biblio.Timestamp;
                    this.CacheTable[strRecPath] = info;
                    yield return info;
                }
                else
                    yield return info;
            }
        }
    }
}
