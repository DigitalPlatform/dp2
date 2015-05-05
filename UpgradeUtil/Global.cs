using System;
using System.Collections.Generic;
using System.Text;

namespace UpgradeUtil
{
    public class Global
    {
        // 解析时间范围字符串
        // parameters:
        //      strText 日期范围字符串。形态为 “19980101-19991231”
        public static int ParseTimeRangeString(string strText,
            out string strStart,
            out string strEnd,
            out string strError)
        {
            strError = "";
            strStart = "";
            strEnd = "";

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                strError = "缺乏破折号 '-'";
                return -1;
            }

            strStart = strText.Substring(0, nRet).Trim();
            strEnd = strText.Substring(nRet + 1).Trim();

            if (strStart.Length != strEnd.Length)
            {
                strError = "起始 '"+strStart+"' 和结束时间字符串 '"+strEnd+"' 长度不同";
                return -1;
            }

            return 0;
        }

    }
}
