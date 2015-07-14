using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是登录缓存相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        public MemoryCache LoginCache = null;

        internal void InitialLoginCache()
        {
            this.LoginCache = new MemoryCache("login");
        }

        // 清除 LoginCache
        internal void ClearLoginCache(string strReaderBarcode)
        {
            if (this.LoginCache != null)
            {
                // 清除全部事项
                if (string.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    MemoryCache old = this.LoginCache;
                    this.LoginCache = new MemoryCache("login");
                    old.Dispose();
                    return;
                }
                this.LoginCache.Remove(strReaderBarcode);
            }
        }
    }
}
