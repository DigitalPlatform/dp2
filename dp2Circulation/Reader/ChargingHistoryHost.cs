using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace dp2Circulation
{

    /// <summary>
    /// 用于出纳历史显示 Web 控件的 Script 宿主类
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class ChargingHistoryHost : IDisposable
    {
        public event EventHandler ButtonClick = null;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
        }



    }
}
