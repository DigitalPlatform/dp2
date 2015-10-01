using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是书目缓存相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        public MemoryCache BiblioSummaryCache = null;

        internal void InitialBiblioSummaryCache()
        {
            this.BiblioSummaryCache = new MemoryCache("bibliosummary");
        }

        // 清除 BiblioSummaryCache
        internal void ClearBiblioSummaryCache(string strItemRecPath)
        {
            if (this.BiblioSummaryCache != null)
            {
                // 清除全部事项
                if (string.IsNullOrEmpty(strItemRecPath) == true)
                {
                    MemoryCache old = this.LoginCache;
                    this.BiblioSummaryCache = new MemoryCache("bibliosummary");
                    old.Dispose();
                    return;
                }
                this.BiblioSummaryCache.Remove(strItemRecPath);
            }
        }
    }
}
