using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// Summary description for Util.
    /// </summary>
    public class Util
    {


        /// <summary>
        /// 写入实用库
        /// </summary>
        /// <param name="Channels"></param>
        /// <param name="strServerUrl"></param>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strRootElementName"></param>
        /// <param name="strKeyAttrName"></param>
        /// <param name="strValueAttrName"></param>
        /// <param name="strKey"></param>
        /// <param name="strValue"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public static int WriteUtilDb(
            RmsChannelCollection Channels,
            string strServerUrl,
            string strDbName,
            string strFrom,
            string strRootElementName,
            string strKeyAttrName,
            string strValueAttrName,
            string strKey,
            string strValue,
            out string strError)
        {
            strError = "";

            string strPath = "";

            RmsChannel channel = Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }


            int nRet = SearchOnePath(channel,
                strDbName,
                strFrom,
                strKey,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            string strXml = "";

            if (nRet == 0)
            {
                strPath = strDbName + "/?";
                strXml = "<" + strRootElementName + " " + strKeyAttrName + "='" + strKey + "' " + strValueAttrName + "='" + strValue + "'/>";

                //bNewRecord = true;

            }
            else
            {
                string strPartXml = "/xpath/<locate>@" + strValueAttrName + "</locate><create>@" + strValueAttrName + "</create>";
                strPath += strPartXml;
                strXml = strValue;

                //bNewRecord = false;
            }

            byte[] baTimestamp = null;

            byte[] baOutputTimeStamp = null;
            string strOutputPath = "";

        REDO:


            channel = Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }



            long lRet = channel.DoSaveTextRes(strPath,
                strXml,
                false,	// bInlucdePreamble
                "ignorechecktimestamp",	// style
                baTimestamp,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {

                    baTimestamp = baOutputTimeStamp;
                    goto REDO;
                }

                return -1;
            }


            return 1;
        }

        /// <summary>
        /// 检索获得一个路径
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strKey"></param>
        /// <param name="strPath"></param>
        /// <param name="strError"></param>
        /// <returns>-1	error;0	not found;1	found</returns>
        static int SearchOnePath(RmsChannel channel,
            string strDbName,
            string strFrom,
            string strKey,
            out string strPath,
            out string strError)
        {
            strPath = "";
            strError = "";

            // 2007/4/5 改造 加上了 GetXmlStringSimple()
            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)        // 2007/9/14
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strKey)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
                    out strError);
            if (lRet == -1)
            {
                strError = "检索库 '" + strDbName + "/" + strFrom + "' 时出错: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                return 0;	// 没有找到
            }

            if (lRet > 1)
            {
                strError = "以Key '" + strKey + "' 检索库 '" + strDbName + "' 时命中 " + Convert.ToString(lRet) + " 条，属于不正常情况。请修改库 '" + strDbName + "' 中相应记录，确保同一Key只有一条对应的记录。";
                return -1;
            }

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                    "default",
                0,
                1,
                "zh",
                null,	// this.stop,
                out aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索库 '" + strDbName + "' 获取检索结果时出错: " + strError;
                return -1;
            }

            strPath = (string)aPath[0];
            return 1;
        }


        /// <summary>
        /// 检索获得若干路径
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strKey"></param>
        /// <param name="nMax"></param>
        /// <param name="paths"></param>
        /// <param name="strError"></param>
        /// <returns>-1	error;0	not found;>=1	found</returns>
        static int SearchPath(RmsChannel channel,
            string strDbName,
            string strFrom,
            string strKey,
            long nMax,
            out string[] paths,
            out string strError)
        {
            paths = null;
            strError = "";

            // 2007/4/5 改造 加上了 GetXmlStringSimple()
            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strKey)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + Convert.ToString(nMax) + "</maxCount></item><lang>zh</lang></target>";


            long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
                    out strError);
            if (lRet == -1)
            {
                strError = "检索库 '" + strDbName + "/" + strFrom + "' 时出错: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                return 0;	// 没有找到
            }

            if (lRet > 1)
            {
                strError = "以Key '" + strKey + "' 检索库 '" + strDbName + "' 时命中 " + Convert.ToString(lRet) + " 条，属于不正常情况。请修改库 '" + strDbName + "' 中相应记录，确保同一Key只有一条对应的记录。";
                return -1;
            }

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                    "default",
                0,
                -1,
                "zh",
                null,	// this.stop,
                out aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索库 '" + strDbName + "' 获取检索结果时出错: " + strError;
                return -1;
            }


            paths = new string[aPath.Count];

            for (int i = 0; i < aPath.Count; i++)
            {
                paths[i] = (string)aPath[i];
            }

            return paths.Length;
        }


        /// <summary>
        /// 检索实用库
        /// </summary>
        /// <param name="Channels"></param>
        /// <param name="strServerUrl"></param>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strKey"></param>
        /// <param name="strValueAttrName"></param>
        /// <param name="strValue"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public static int SearchUtilDb(
            RmsChannelCollection Channels,
            string strServerUrl,
            string strDbName,
            string strFrom,
            string strKey,
            string strValueAttrName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            string strPath = "";

            RmsChannel channel = Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            int nRet = SearchOnePath(channel,
                strDbName,
                strFrom,
                strKey,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;


            // 取记录
            string strStyle = "content,data,timestamp";

            string strMetaData;
            string strOutputPath;
            string strXml = "";
            byte[] baTimeStamp = null;

            long lRet = channel.GetRes(strPath,
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索 '" + strPath + "' 记录体时出错: " + strError;
                return -1;
            }


            XmlDocument domRecord = new XmlDocument();
            try
            {
                domRecord.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载路径为'" + strPath + "'的xml记录时出错: " + ex.Message;
                return -1;
            }

            strValue = DomUtil.GetAttr(domRecord.DocumentElement, strValueAttrName);

            return 1;
        }

        /// <summary>
        /// 检索实用库
        /// </summary>
        /// <param name="Channels"></param>
        /// <param name="strServerUrl"></param>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strKey"></param>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public static int SearchUtilDb(
            RmsChannelCollection Channels,
            string strServerUrl,
            string strDbName,
            string strFrom,
            string strKey,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            RmsChannel channel = Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }


            string[] paths = null;

            int nRet = SearchPath(channel,
                strDbName,
                strFrom,
                strKey,
                1,
                out paths,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            string strPath = paths[0];

            // 取记录
            string strStyle = "content,data,timestamp";

            string strMetaData;
            string strOutputPath;
            byte[] baTimeStamp = null;

            long lRet = channel.GetRes(strPath,
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索 '" + strPath + "' 记录体时出错: " + strError;
                return -1;
            }


            return 1;
        }


    }
}
