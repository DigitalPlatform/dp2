using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;

// using DigitalPlatform.rms.Client.rmsws_localhost;   // Record

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和 实用库 功能相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
#if TEMP
        public LibraryServerResult GetItemInfo(
            SessionInfo sessioninfo,
            string strResPath,
            string strStyle,
            out string strMetadata,
            out byte[] baOutputTimestamp,
            out string strOutputResPath)
        {
            LibraryServerResult result = new LibraryServerResult();

            // TODO: 建议这里抽取出一个函数 GetAmerceInfo()，里面包含检查分馆权限的功能。FilterResultSet() 那里也可以用上这个抽取出来的函数
            lRet = channel.GetRes(strResPath,
                strStyle + ",data", // 确保可以获取到记录 XML
                out string amerce_xml,
                out strMetadata,
                out baOutputTimestamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
            {
                result.Value = lRet;
                result.ErrorInfo = strError;
                ConvertKernelErrorCode(channel.ErrorCode,
                    ref result);
                return result;
            }

            XmlDocument amerce_dom = new XmlDocument();
            try
            {
                amerce_dom.LoadXml(amerce_xml);
            }
            catch (Exception ex)
            {
                strError = "违约金记录 '" + strOutputResPath + "' 装入XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            // 检查当前账户是否有查看一条违约金记录的权限
            // return:
            //      -1  出错
            //      0   不具备权限
            //      1   具备权限
            int nRet = HasAmerceReadRight(
                sessioninfo,
                strOutputResPath,
                amerce_dom,
                out strError);
            if (nRet != 1)
            {
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // TODO: 根据当前账户是否具备 getamerceobject 权限，决定是否过滤掉 XML 记录中的 dprms:file 元素

            if (StringUtil.IsInList("data", strStyle))
            {
                formats.Add("xml");
                results = new string[] { amerce_xml };
            }

        }

#endif

        // TODO: 这里要检查一下 strDbName，是否为合法的实用库名
        // 设置实用库信息
        //      strRootElementName  根元素名。如果为空，系统自会用<r>作为根元素
        //      strKeyAttrName  key属性名。如果为空，系统自动会用k
        //      strValueAttrName    value属性名。如果为空，系统自动会用v

        public LibraryServerResult SetUtilInfo(
            SessionInfo sessioninfo,
            string strAction,
            string strDbName,
            string strFrom,
            string strRootElementName,
            string strKeyAttrName,
            string strValueAttrName,
            string strKey,
            string strValue)
        {
            string strError = "";
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            string strPath = "";
            string strXml = "";
            byte[] timestamp = null;

            bool bRedo = false;

            if (String.IsNullOrEmpty(strRootElementName) == true)
                strRootElementName = "r";   // 最简单的缺省模式

            if (String.IsNullOrEmpty(strKeyAttrName) == true)
                strKeyAttrName = "k";

            if (String.IsNullOrEmpty(strValueAttrName) == true)
                strValueAttrName = "v";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 检索实用库记录的路径和记录体
            // return:
            //      -1  error(注：检索命中多条情况被当作错误返回)
            //      0   not found
            //      1   found
            nRet = SearchUtilPathAndRecord(
                // sessioninfo.Channels,
                channel,
                strDbName,
                strKey,
                strFrom,
                out strPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 如果动作为直接设置整个记录
            if (strAction == "setrecord")
            {
                if (nRet == 0)
                {
                    strPath = strDbName + "/?";
                }

                strXml = strValue;
            }
            else
            {
                // 根据若干信息构造出记录
                if (nRet == 0)
                {
                    strPath = strDbName + "/?";

                    // strXml = "<" + strRootElementName + " " + strKeyAttrName + "='" + strKey + "' " + strValueAttrName + "='" + strValue + "'/>";

                    // 2011/12/11
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<" + strRootElementName + "/>");
                    DomUtil.SetAttr(dom.DocumentElement, strKeyAttrName, strKey);
                    DomUtil.SetAttr(dom.DocumentElement, strValueAttrName, strValue);
                    strXml = dom.DocumentElement.OuterXml;
                }
                else
                {
                    string strPartXml = "/xpath/<locate>@" + strValueAttrName + "</locate><create>@" + strValueAttrName + "</create>";
                    strPath += strPartXml;
                    strXml = strValue;
                }
            }

#if NO
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
#endif

            byte[] baOutputTimeStamp = null;
            string strOutputPath = "";
            int nRedoCount = 0;
        REDO:
            long lRet = channel.DoSaveTextRes(strPath,
                strXml,
                false,	// bInlucdePreamble
                "ignorechecktimestamp",	// style
                timestamp,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (bRedo == true)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoCount < 10)
                    {
                        timestamp = baOutputTimeStamp;
                        nRedoCount++;
                        goto REDO;
                    }
                }

                goto ERROR1;
            }

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 获得实用库信息
        public LibraryServerResult GetUtilInfo(
            SessionInfo sessioninfo,
            string strAction,
            string strDbName,
            string strFrom,
            string strKey,
            string strValueAttrName,
            out string strValue)
        {
            string strError = "";
            strValue = "";
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            /*
            if (String.IsNullOrEmpty(strKeyAttrName) == true)
                strKeyAttrName = "k";
             * */

            if (String.IsNullOrEmpty(strValueAttrName) == true)
                strValueAttrName = "v";


            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strPath = "";
            string strXml = "";
            byte[] timestamp = null;

            // 检索实用库记录的路径和记录体
            // return:
            //      -1  error(注：检索命中多条情况被当作错误返回)
            //      0   not found
            //      1   found
            nRet = SearchUtilPathAndRecord(
                // sessioninfo.Channels,
                channel,
                strDbName,
                strKey,
                strFrom,
                out strPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                result.ErrorCode = ErrorCode.NotFound;
                result.ErrorInfo = "库名为 '"+strDbName+"' 途径为 '"+strFrom+"' 键值为 '" + strKey + "' 的记录没有找到";
                result.Value = 0;
                return result;
            }

            // 如果动作为获得整个记录
            if (strAction == "getrecord")
            {
                strValue = strXml;

                result.Value = 1;
                return result;
            }

            XmlDocument domRecord = new XmlDocument();
            try
            {
                domRecord.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载路径为'" + strPath + "'的xml记录时出错: " + ex.Message;
                goto ERROR1;
            }

            strValue = DomUtil.GetAttr(domRecord.DocumentElement, strValueAttrName);

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 检索实用库记录的路径和记录体
        // return:
        //      -1  error(注：检索命中多条情况被当作错误返回)
        //      0   not found
        //      1   found
        public int SearchUtilPathAndRecord(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strDbName,
            string strKey,
            string strFrom,
            out string strPath,
            out string strXml,
            out byte[] timestamp,
            out string strError)
        {
            strError = "";
            strPath = "";
            strXml = "";
            timestamp = null;

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未指定库名";
                return -1;
            }

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strKey)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            // 获得通用记录
            // 本函数可获得超过1条以上的路径
            // return:
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            int nRet = GetRecXml(
                // Channels,
                channel,
                strQueryXml,
                out strXml,
                2,
                out List<string> aPath,
                out timestamp,
                out strError);
            if (nRet == -1)
            {
                strError = "检索库 " + strDbName + " 时出错: " + strError;
                return -1;
            }
            if (nRet == 0)
            {
                return 0;	// 没有找到
            }

            /*
            if (nRet > 1)
            {
                strError = "以检索键 '" + strKey + "' 检索库 " + strDbName + " 时命中 " + Convert.ToString(nRet) + " 条，属于不正常情况。请修改库 '" + strDbName + "' 中相应记录，确保同一键值只有一条对应的记录。";
                return -1;
            }
             * */

            Debug.Assert(aPath.Count >= 1, "");
            strPath = aPath[0];

            return 1;
        }
    }

}