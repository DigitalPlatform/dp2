using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// LibraryClient 函数库全局参数
    /// </summary>
    public static class LibraryChannelManager
    {
        public static ILog Log { get; set; }
    }
}
