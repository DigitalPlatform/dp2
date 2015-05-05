using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 扩展的SSO接口
    /// 2012/11/7 创建
    /// </summary>
    public class ExternalSsoHost
    {
        // parameters:
        //      strGotoUrl  如果函数返回-1，并且本参数不为空，则需要redirect到这个URL
        //      strError    [out]返回错误字符串
        // return:
        //      -1  出错
        //      0   不具备SSO条件。宿主会继续向后寻找其他接口
        //      1   成功
        public virtual int GetUserInfo(
            System.Web.UI.Page page,
            out string strLoginName,
            out string strGotoUrl,
            out string strError)
        {
            strError = "接口尚未实现";
            strLoginName = "";
            strGotoUrl = "";

            return -1;
        }

    }
}
