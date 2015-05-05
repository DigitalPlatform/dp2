using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 扩展的消息接口
    /// 2011/9/27 创建
    /// </summary>
    public class ExternalMessageHost
    {
        // LibraryApplication对象。为了让接口尽量通用，类型设计为object，确保以后可以用于存储其他类型的对象。另外使用类型object也避免了DigitalPlatform.Interfaces库引用DigitalPlatform.LibraryServer库，因为这将导致循环引用
        public object App = null;

        // 发送一条消息
        // parameters:
        //      strPatronBarcode    读者证条码号
        //      strPatronXml    读者记录XML字符串。如果需要除证条码号以外的某些字段来确定消息发送地址，可以从XML记录中取
        //      strMessageText  消息文字
        //      strError    [out]返回错误字符串
        // return:
        //      -1  发送失败
        //      0   没有必要发送
        //      1   发送成功
        public virtual int SendMessage(
            string strPatronBarcode,
            string strPatronXml,
            string strMessageText,
            string strLibraryCode,
            out string strError)
        {
            strError = "";
            return 0;
        }
    }
}
