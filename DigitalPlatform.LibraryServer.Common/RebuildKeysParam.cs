using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer.Common
{
    // 重建检索点后台任务的参数处理
    public static class RebuildKeysParam
    {
        #region 参数字符串处理
        // 这些函数也被 dp2Library 前端使用

        // 解析 开始 参数
        // 参数原始存储的时候，为了避免在参数字符串中发生混淆，数据库名之间用 | 间隔
        public static int ParseStart(string strStart,
            out string strDbNameList,
            out string strError)
        {
            strError = "";
            strDbNameList = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strStart);
            strDbNameList = (string)table["dbnamelist"];
            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace("|", ",");
            return 0;
        }

        // 构造开始参数，也是断点字符串
        // 参数原始存储的时候，为了避免在参数字符串中发生混淆，数据库名之间用 | 间隔
        public static string BuildStart(
            string strDbNameList)
        {
            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace(",", "|");

            Hashtable table = new Hashtable();
            table["dbnamelist"] = strDbNameList;

            return StringUtil.BuildParameterString(table);
        }

        // 解析通用启动参数
        public static int ParseTaskParam(string strParam,
            out string strFunction,
            out bool bClearFirst,
            out bool quick_mode,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            strFunction = "";
            quick_mode = false;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            strFunction = (string)table["function"];
            quick_mode = DomUtil.IsBooleanTrue((string)table["quick"], false);
            string strClearFirst = (string)table["clear_first"];
            if (strClearFirst == null)
                strClearFirst = "";
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            return 0;
        }

        public static string BuildTaskParam(
            string strFunction,
            bool bClearFirst,
            bool quick_mode)
        {
            Hashtable table = new Hashtable();
            table["function"] = strFunction;
            table["clear_first"] = bClearFirst ? "yes" : "no";
            if (quick_mode == true)
                table["quick"] = quick_mode ? "true" : "false";
            return StringUtil.BuildParameterString(table);
        }

        #endregion

    }
}
