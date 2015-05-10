using System;
using System.Collections.Generic;
using System.Text;

namespace dp2ZServer
{
    /// <summary>
    /// 全局函数
    /// </summary>
    public class Global
    {
        // 从路径中取出库名部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetDbName(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }

        // 从路径中取出记录号部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

    }
}
