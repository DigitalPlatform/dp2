using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    public static class LibraryClientExtension
    {
        #region 配置文件相关

        // 包装版本
        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public static int GetCfgFileContent(
            this IChannelLooping host,
            string strBiblioDbName,
            string strCfgFileName,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            return GetCfgFileContent(
                host,
                strBiblioDbName + "/cfgs/" + strCfgFileName,
                out strContent,
                out baOutputTimestamp,
                out strError);
        }

        // static int m_nInGetCfgFile = 0;    // 防止GetCfgFile()函数重入 2008/3/6

        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public static int GetCfgFileContent(
            this IChannelLooping host,
            string strCfgFilePath,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strContent = "";

            /*
            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }
            */

            /*
            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial("正在下载配置文件 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在下载配置文件 ...");
            Progress.BeginLoop();
#endif
            */
            var looping = host.Looping(out LibraryChannel channel,
                "正在下载配置文件 ...",
                "timeout:0:1:0");

            /*
            m_nInGetCfgFile++;
            */
            try
            {
                looping.Progress.SetMessage("正在下载配置文件 " + strCfgFilePath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath,gzip";

                long lRet = channel.GetRes(looping.Progress,
                    Program.MainForm?.cfgCache,
                    strCfgFilePath,
                    strStyle,
                    null,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    return -1;
                }
                return 1;
            }
            finally
            {
                /*
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
                looping.Dispose();

                /*
                m_nInGetCfgFile--;
                */
            }
        }

        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public static int GetCfgFile(
            this IChannelLooping host,
            string strBiblioDbName,
            string strCfgFileName,
            out string strOutputFilename,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strOutputFilename = "";

            /*
            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }
            */

            /*
            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial("正在下载配置文件 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在下载配置文件 ...");
            Progress.BeginLoop();
#endif
            */
            var looping = host.Looping(out LibraryChannel channel,
                "正在下载配置文件 ...",
                "timeout:0:1:0");

            /*
            m_nInGetCfgFile++;
            */
            try
            {
                string strPath = strBiblioDbName + "/cfgs/" + strCfgFileName;

                looping.Progress.SetMessage("正在下载配置文件 " + strPath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = channel.GetResLocalFile(looping.Progress,
                    Program.MainForm?.cfgCache,
                    strPath,
                    strStyle,
                    out strOutputFilename,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    return -1;
                }

                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
                /*
                m_nInGetCfgFile--;
                */
            }
        }

        // 保存配置文件
        public static int SaveCfgFile(
            this IChannelLooping host,
            string strBiblioDbName,
            string strCfgFileName,
            string strContent,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";

            /*
            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial("正在保存配置文件 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在保存配置文件 ...");
            Progress.BeginLoop();
#endif
            */
            var looping = host.Looping(out LibraryChannel channel,
                "正在保存配置文件 ...",
                "timeout:0:1:0");

            try
            {
                string strPath = strBiblioDbName + "/cfgs/" + strCfgFileName;

                looping.Progress.SetMessage("正在保存配置文件 " + strPath + " ...");

                byte[] output_timestamp = null;
                string strOutputPath = "";

                long lRet = channel.WriteRes(
                    looping.Progress,
                    strPath,
                    strContent,
                    true,
                    "",	// style
                    baTimestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
            }
        }

        #endregion

        #region 摘要

        // (从 OperLogStatisForm 中移动过来)
        // 2012/10/6
        // 获得册记录的书目摘要
        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="host"></param>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="nMaxLength">书目摘要的最大字符数。-1 表示不截断。超过这个字符数的书目摘要被截断，末尾添加"..."</param>
        /// <returns>书目摘要字符串</returns>
        public static string GetItemSummary(
            this IChannelLooping host,
            string strItemBarcode,
            int nMaxLength = -1)
        {
            /*
            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);
            */
            var looping = host.Looping(out LibraryChannel channel,
    null,
    "timeout:0:0:10");
            try
            {
                var ret = channel.GetBiblioSummary(
                    looping.Progress,
                    // channel,
                    strItemBarcode,
                    "",
                    "",
                    out string strBiblioRecPath,
                    out string strSummary,
                    out string strError);
                if (ret == -1)
                    return strError;

                if (nMaxLength == -1 || strSummary.Length <= nMaxLength)
                    return strSummary;

                return strSummary.Substring(0, nMaxLength) + "...";
            }
            finally
            {
                looping.Dispose();
                /*
                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
            }
        }

        #endregion

        // 要设法将是否 BeginLoop()、是否自动重试 等特性用一种统一的参数表达
#if REMOVED
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static int GetTable(
            this IChannelLooping host,
            string strRecPath,
            string strStyleList,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strFormat = "table";
            if (string.IsNullOrEmpty(strStyleList) == false)
                strFormat += ":" + strStyleList.Replace(",", "|");

            LibraryChannel channel = host.GetChannel();
            /*
            var looping = Looping(out LibraryChannel channel,
                null);
            */
            try
            {
                long lRet = channel.GetBiblioInfos(
                    null,   // looping.Progress,
                    strRecPath,
                    "",
                    new string[] { strFormat },   // formats
                    out string[] results,
                    out byte[] baNewTimestamp,
                    out strError);
                if (lRet == 0)
                    return 0;
                if (lRet == -1)
                    return -1;
                if (results == null || results.Length == 0)
                {
                    strError = "results error";
                    return -1;
                }
                strXml = results[0];
                return 1;
            }
            finally
            {
                // looping.Dispose();
                host.ReturnChannel(channel);
            }
        }
#endif
    }
}
