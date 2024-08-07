using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Script
{
    public class BeforeSaveRecordEventArgs : EventArgs
    {
        // 2024/7/31
        // [in]打算保存到的目标路径
        public string TargetRecPath { get; set; }

        // [out] 当出错时(也就是 ErrorInfo 中有内容)是否中断后续处理
        public bool Cancel { get; set; }


        public string CurrentUserName = ""; // [in](备选的)当前用户名
        
        public string ErrorInfo = "";   // [out]
        
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;    // [out]数据是否发生改变
    }

    public class AfterCreateItemsArgs : EventArgs
    {
        public string CurrentUserName = ""; // [in](备选的)当前用户名
        public string Case = ""; // [in] 什么缘由进行的保存
        public string ErrorInfo = "";   // [out]
    }

}
