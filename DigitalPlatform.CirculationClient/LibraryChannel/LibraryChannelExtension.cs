using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace DigitalPlatform.CirculationClient
{
    public static class LibraryChannelExtension
    {
        // 获得资源。包装版本 -- 返回字符串版本、Cache版本。
        // parameters:
        //      remote_timestamp    远端时间戳。如果为 null，表示要从服务器实际获取时间戳
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public static long GetRes(this LibraryChannel channel,
            DigitalPlatform.Stop stop,
            CfgCache cache,
            string strPath,
            string strStyle,
            byte[] remote_timestamp,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            strError = "";
            strResult = "";
            strMetaData = "";
            baOutputTimeStamp = null;
            strOutputResPath = "";

            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            long lRet = 0;

            string strFullPath = channel.Url + "?" + strPath;

            if (StringUtil.IsInList("forceget", strStyle) == true)
            {
                // 强制获取
                StringUtil.RemoveFromInList("forceget",
                    true,
                    ref strStyle);
                goto GETDATA;
            }

            // 从cache中得到timestamp
            // return:
            //      -1  error
            //		0	not found
            //		1	found
            int nRet = cache.FindLocalFile(strFullPath,
                out strLocalName,
                out strTimeStamp);
            if (nRet == -1)
            {
                strError = "CfgCache 尚未初始化";
                return -1;
            }
            if (nRet == 1)
            {
                Debug.Assert(strLocalName != "", "FindLocalFile()返回的strLocalName为空");

                if (strTimeStamp == "")
                    goto GETDATA;	// 时间戳不对, 那就只好重新获取服务器端内容

                Debug.Assert(strTimeStamp != "", "FindLocalFile()获得的strTimeStamp为空");
                cached_timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
                // bExistInCache = true;
            }
            else
                goto GETDATA;

            if (remote_timestamp == null)
            {
                // 探测时间戳关系
                string strNewStyle = strStyle;

                StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
                    true,
                    ref strNewStyle);	// 不要数据体和metadata

                lRet = channel.GetRes(stop,
                    strPath,
                    strNewStyle,
                    out strResult,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputResPath,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
            else
                baOutputTimeStamp = remote_timestamp;

            // 如果证明timestamp没有变化, 但是本次并未返回内容,则从cache中取原来的内容

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)	// 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                try
                {
                    using (StreamReader sr = new StreamReader(strLocalName, Encoding.UTF8))
                    {
                        strResult = sr.ReadToEnd();
                        return 0;	// 以无错误姿态返回
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

        GETDATA:

            // 重新正式获取内容
            lRet = channel.GetRes(
                stop,
                strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 因为时间戳不匹配而新获得了内容
            // 保存到cache
            cache.PrepareLocalFile(strFullPath, out strLocalName);
            Debug.Assert(strLocalName != "", "PrepareLocalFile()返回的strLocalName为空");

            // 写入文件,以便以后从cache获取
            using (StreamWriter sw = new StreamWriter(strLocalName,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strResult);
            }

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;

            return lRet;
        }

        // 获得资源。包装版本 -- 返回本地映射文件、Cache版本。
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public static long GetResLocalFile(this LibraryChannel channel,
            DigitalPlatform.Stop stop,
            CfgCache cache,
            string strPath,
            string strStyle,
            out string strOutputFilename,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            strOutputFilename = "";

            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            string strResult = "";

            string strFullPath = channel.Url + "?" + strPath;

            if (StringUtil.IsInList("forceget", strStyle) == true)
            {
                // 强制获取
                StringUtil.RemoveFromInList("forceget",
                    true,
                    ref strStyle);
                goto GETDATA;
            }

            // 从cache中得到timestamp
            // return:
            //      -1  error
            //		0	not found
            //		1	found
            int nRet = cache.FindLocalFile(strFullPath,
                out strLocalName,
                out strTimeStamp);
            if (nRet == -1)
            {
                strOutputResPath = "";
                strMetaData = "";
                baOutputTimeStamp = null;
                strError = "CfgCache 尚未初始化";
                return -1;
            }
            if (nRet == 1)
            {
                Debug.Assert(strLocalName != "", "FindLocalFile()返回的strLocalName为空");

                if (strTimeStamp == "")
                    goto GETDATA;	// 时间戳不对, 那就只好重新获取服务器端内容

                Debug.Assert(strTimeStamp != "", "FindLocalFile()获得的strTimeStamp为空");
                cached_timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
                // bExistInCache = true;
            }
            else
                goto GETDATA;

            // 探测时间戳关系
            string strNewStyle = strStyle;

            /*
            StringUtil.RemoveFromInList("data",
                true,
                ref strNewStyle);	// 不要数据体
             * */
            StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
    true,
    ref strNewStyle);	// 不要数据体和metadata

            long lRet = channel.GetRes(stop,
                strPath,
                strNewStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 如果证明timestamp没有变化, 但是本次并未返回内容,则从cache中取原来的内容

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)	// 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                strOutputFilename = strLocalName;
                return 0;	// 以无错误姿态返回
            }

        GETDATA:
            // 重新正式获取内容
            lRet = channel.GetRes(
                stop,
                strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 因为时间戳不匹配而新获得了内容
            // 保存到cache
            cache.PrepareLocalFile(strFullPath, out strLocalName);
            Debug.Assert(strLocalName != "", "PrepareLocalFile()返回的strLocalName为空");

            // 写入文件,以便以后从cache获取
            using (StreamWriter sw = new StreamWriter(strLocalName,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strResult);
            }

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;

            strOutputFilename = strLocalName;

            return lRet;
        }

    }
}
