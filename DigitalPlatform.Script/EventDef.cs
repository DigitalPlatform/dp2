using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Script
{
    public class BeforeSaveRecordEventArgs : EventArgs
    {
        // 2025/10/20
        // [in]保存动作名称。
        // copy / move / onlycopybiblio / onlymovebiblio 之一
        // 空默认为 save
        public string SaveAction { get; set; }

        // 2025/10/20
        // [in]源路径。即移动或者复制操作的来源记录的路径。
        // 如果是覆盖保存操作，本属性应该为空
        public string SourceRecPath { get; set; }

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

        public override string ToString()
        {
            return $"SaveAction={SaveAction},SourceRecPath={SourceRecPath},TargetRecPath={TargetRecPath},CurrentUserName={CurrentUserName},Changed={Changed},ErrorInfo={ErrorInfo},Cancel={Cancel}";
        }

    }

    public class AfterCreateItemsArgs : EventArgs
    {
        public string CurrentUserName = ""; // [in](备选的)当前用户名
        public string Case = ""; // [in] 什么缘由进行的保存
        public string ErrorInfo = "";   // [out]
    }

}
