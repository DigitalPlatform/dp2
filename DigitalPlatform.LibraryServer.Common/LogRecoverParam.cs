using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer.Common
{
    public class LogRecoverParam
    {
        public static string Build(
            string strDirectory,
            string strRecoverLevel,
            bool bClearFirst,
            bool bContinueWhenError,
            string strStyle)
        {
            // 通用启动参数
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            if (string.IsNullOrEmpty(strRecoverLevel) == false)
            {
                DomUtil.SetAttr(dom.DocumentElement,
                    "recoverLevel",
                    strRecoverLevel);
            }
            DomUtil.SetAttr(dom.DocumentElement,
                "clearFirst",
                bClearFirst ? "yes" : "no");

            DomUtil.SetAttr(dom.DocumentElement,
    "continueWhenError",
    bContinueWhenError ? "yes" : "no");

            if (string.IsNullOrEmpty(strDirectory) == false)
                dom.DocumentElement.SetAttribute("directory", strDirectory);

            if (string.IsNullOrEmpty(strStyle))
                dom.DocumentElement.SetAttribute("style", strStyle);
            return dom.OuterXml;
        }

#if REMOVED
        // TODO: 增加 style 参数
        /// <summary>
        /// 解析日志恢复参数
        /// </summary>
        /// <param name="strParam">待解析的参数字符串</param>
        /// <param name="strDirectory"></param>
        /// <param name="strRecoverLevel">日志恢复级别</param>
        /// <param name="bClearFirst">在恢复前是否清除现有的数据库记录</param>
        /// <param name="bContinueWhenError">出错后是否继续批处理</param>
        /// <param name="strError">错误信息。当本方法发生错误时</param>
        /// <returns>-1: 出错。错误信息在 strError 参数中返回；0: 成功</returns>
        public static int ParseLogRecoverParam(string strParam,
            out string strDirectory,    // 2024/2/15
            out string strRecoverLevel,
            out bool bClearFirst,
            out bool bContinueWhenError,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bContinueWhenError = false;
            strRecoverLevel = "";
            strDirectory = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam参数装入XML DOM时出错: " + ex.Message;
                return -1;
            }

            /*
            Logic = 0,  // 逻辑操作
            LogicAndSnapshot = 1,   // 逻辑操作，若失败则转用快照恢复
            Snapshot = 3,   // （完全的）快照
            Robust = 4, // 最强壮的容错恢复方式
             * */

            strRecoverLevel = DomUtil.GetAttr(dom.DocumentElement,
                "recoverLevel");
            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            // 2016/3/8
            bContinueWhenError = DomUtil.GetBooleanParam(dom.DocumentElement,
                "continueWhenError",
                false);

            dom.DocumentElement.GetAttribute("directory");
            return 0;
        }
#endif

        // 解析通用启动参数
        // 格式
        /*
         * <root recoverLevel='...' clearFirst='...' continueWhenError='...'/>
         * recoverLevel 缺省为 Snapshot
         * clearFirst 缺省为 false
         * continueWhenError 缺省值为 false
         * */
        public static int ParseLogRecoverParam(string strParam,
            out string strDirectory,    // 2024/2/15
            out string strRecoverLevel,
            out bool bClearFirst,
            out bool bContinueWhenError,
            out string strStyle,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            strRecoverLevel = "";
            bContinueWhenError = false;
            strStyle = "";
            strDirectory = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam参数装入XML DOM时出错: " + ex.Message;
                return -1;
            }

            /*
            Logic = 0,  // 逻辑操作
            LogicAndSnapshot = 1,   // 逻辑操作，若失败则转用快照恢复
            Snapshot = 3,   // （完全的）快照
            Robust = 4,
             * */

            strRecoverLevel = DomUtil.GetAttr(dom.DocumentElement,
                "recoverLevel");
            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            // 2016/3/8
            bContinueWhenError = DomUtil.GetBooleanParam(dom.DocumentElement,
                "continueWhenError",
                false);
            strStyle = dom.DocumentElement.GetAttribute("style");
            strDirectory = dom.DocumentElement.GetAttribute("directory");
            return 0;
        }
    }

    public class LogRecoverStart
    {
        // 合成参数
        public static string Build(string strFileName,
            string strIndex)
        {
            if (string.IsNullOrEmpty(strFileName))
                return "";
            else
            {
                long index = 0;
                if (string.IsNullOrEmpty(strIndex) == false)
                {
                    try
                    {
                        index = Convert.ToInt64(strIndex);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("记录 ID '" + strIndex + "' 必须为纯数字");
                    }
                }
                return index.ToString() + "@" + strFileName;
            }

        }

        // 解析 开始 参数
        public static int ParseLogRecoverStart(string strStart,
            out long index,
            out string strFileName,
            out string strError)
        {
            strError = "";
            index = 0;
            strFileName = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            int nRet = strStart.IndexOf('@');
            if (nRet == -1)
            {
                try
                {
                    index = Convert.ToInt64(strStart);
                }
                catch (Exception)
                {
                    strError = "启动参数 '" + strStart + "' 格式错误：" + "如果没有@，则应为纯数字。";
                    return -1;
                }
                return 0;
            }

            try
            {
                index = Convert.ToInt64(strStart.Substring(0, nRet).Trim());
            }
            catch (Exception)
            {
                strError = "启动参数 '" + strStart + "' 格式错误：'" + strStart.Substring(0, nRet).Trim() + "' 部分应当为纯数字。";
                return -1;
            }


            strFileName = strStart.Substring(nRet + 1).Trim();

            // 如果文件名没有扩展名，自动加上
            if (String.IsNullOrEmpty(strFileName) == false)
            {
                nRet = strFileName.ToLower().LastIndexOf(".log");
                if (nRet == -1)
                    strFileName = strFileName + ".log";
            }

            return 0;
        }

#if REMOVED
        // 解析 开始 参数
        static int ParseLogRecoverStart(string strStart,
            out long index,
            out string strFileName,
            out string strError)
        {
            strError = "";
            index = 0;
            strFileName = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            int nRet = strStart.IndexOf('@');
            if (nRet == -1)
            {
                try
                {
                    index = Convert.ToInt64(strStart);
                }
                catch (Exception)
                {
                    strError = "启动参数 '" + strStart + "' 格式错误：" + "如果没有@，则应为纯数字。";
                    return -1;
                }
                return 0;
            }

            try
            {
                index = Convert.ToInt64(strStart.Substring(0, nRet).Trim());
            }
            catch (Exception)
            {
                strError = "启动参数 '" + strStart + "' 格式错误：'" + strStart.Substring(0, nRet).Trim() + "' 部分应当为纯数字。";
                return -1;
            }

            strFileName = strStart.Substring(nRet + 1).Trim();
            return 0;
        }
#endif
    }
}
