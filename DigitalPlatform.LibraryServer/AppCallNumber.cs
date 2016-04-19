using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;

using DigitalPlatform.rms.Client.rmsws_localhost;
using System.Text.RegularExpressions;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和索取号功能相关的代码
    /// </summary>
    public partial class LibraryApplication
    {

        /*
    <callNumber>
        <group name="中文" zhongcihaodb="种次号">
            <location name="基藏库" />
            <location name="流通库" />
        </group>
        <group name="英文" zhongcihaodb="新种次号库">
            <location name="英文基藏库" />
            <location name="英文流通库" />
        </group>
    </callNumber>         * */

        // 通过馆藏地点名得到排架group名
        string GetArrangeGroupName(string strLocation)
        {
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//callNumber/group[./location[@name='" + strLocation + "']]");
            if (node == null)
            {
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//callNumber/group/location");
                if (nodes.Count == 0)
                    return null;
                foreach (XmlNode current in nodes)
                {
                    string strPattern = DomUtil.GetAttr(current, "name");
                    if (LibraryServerUtil.MatchLocationName(strLocation, strPattern) == true)
                        return DomUtil.GetAttr(current.ParentNode, "name");
                }

                return null;
            }

            return DomUtil.GetAttr(node, "name");
        }

        // 通过种排架体系名获得种次号库名
        string GetTailDbName(string strArrangeGroupName)
        {
            if (String.IsNullOrEmpty(strArrangeGroupName) == true)
                return null;

            if (strArrangeGroupName[0] == '!')
            {
                string strTemp = GetArrangeGroupName(strArrangeGroupName.Substring(1));

                if (strTemp == null)
                {
                    return null;
                }
                strArrangeGroupName = strTemp;
            }

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//callNumber/group[@name='" + strArrangeGroupName + "']");
            if (node == null)
                return null;

            return DomUtil.GetAttr(node, "zhongcihaodb");
        }

        // (根据一定排架体系)检索出某一类的同类书的索取号
        // parameters:
        //      strArrangeGroupName 排架体系名。如果为"!xxx"形式，表示通过馆藏地点名来暗示排架体系名
        public LibraryServerResult SearchOneClassCallNumber(
            SessionInfo sessioninfo,
            string strArrangeGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml)
        {
            strQueryXml = "";

            string strError = "";

            LibraryServerResult result = new LibraryServerResult();

            if (String.IsNullOrEmpty(strArrangeGroupName) == true)
            {
                strError = "strArrangeGroupName参数值不能为空";
                goto ERROR1;
            }

            if (strArrangeGroupName[0] == '!')
            {
                string strTemp = GetArrangeGroupName(strArrangeGroupName.Substring(1));

                if (strTemp == null)
                {
                    strError = "馆藏地点名 " + strArrangeGroupName.Substring(1) + " 没有找到对应的排架体系名";
                    goto ERROR1;
                }
                strArrangeGroupName = strTemp;
            }

            // <location>元素数组
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//callNumber/group[@name='" + strArrangeGroupName + "']/location");
            if (nodes.Count == 0)
            {
                strError = "library.xml中尚未配置有关 '" + strArrangeGroupName + "' 的<callNumber>/<group>/<location>相关参数";
                goto ERROR1;
            }

            string strTargetList = "";

            // 遍历所有实体库
            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                string strItemDbName = this.ItemDbs[i].DbName;

                if (String.IsNullOrEmpty(strItemDbName) == true)
                    continue;

                if (String.IsNullOrEmpty(strTargetList) == false)
                    strTargetList += ";";
                strTargetList += strItemDbName + ":索取类号";
            }

            int nCount = 0;
            // 构造检索式
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strLocationName = DomUtil.GetAttr(node, "name");

                if (String.IsNullOrEmpty(strLocationName) == true)
                    continue;

                if (nCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strLocationName = strLocationName.Replace("*", "%");

                /*
                strQueryXml += "<item><word>"
                    + StringUtil.GetXmlStringSimple(strLocationName + "|" + strClass)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang>";
                 * */
                strQueryXml += "<item><word>"
    + StringUtil.GetXmlStringSimple(strLocationName + "|" + strClass + "/")
    + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang>";

                nCount++;
            }

            strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strTargetList)       // 2007/9/14
                    + "'>" + strQueryXml + "</target>";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,   // "default",
                "keyid", // "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "not found";
                result.ErrorCode = ErrorCode.NotFound;
                return result;
            }


            result.Value = lRet;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // parameters:
        //      strBrowseInfoStyle  返回的特性。cols 返回实体记录的浏览列
        public LibraryServerResult GetCallNumberSearchResult(
            SessionInfo sessioninfo,
            string strArrangeGroupName,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out CallNumberSearchResult[] searchresults)
        {
            string strError = "";
            searchresults = null;

            LibraryServerResult result = new LibraryServerResult();
            // int nRet = 0;
            long lRet = 0;

            if (String.IsNullOrEmpty(strArrangeGroupName) == true)
            {
                strError = "strArrangeGroupName参数值不能为空";
                goto ERROR1;
            }

            if (strArrangeGroupName[0] == '!')
            {
                string strTemp = GetArrangeGroupName(strArrangeGroupName.Substring(1));

                if (strTemp == null)
                {
                    strError = "馆藏地点名 " + strArrangeGroupName.Substring(1) + " 没有找到对应的排架体系名";
                    goto ERROR1;
                }
                strArrangeGroupName = strTemp;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                result.Value = -1;
                result.ErrorInfo = "get channel error";
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }


            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

            bool bCols = StringUtil.IsInList("cols", strBrowseInfoStyle);

            string strBrowseStyle = "keyid,id,key";
            if (bCols == true)
                strBrowseStyle += ",cols,format:cfgs/browse_callnumber";

            Record[] origin_searchresults = null; //

            lRet = channel.DoGetSearchResult(
                strResultSetName,
                lStart,
                lCount,
                strBrowseStyle, // "id",
                strLang,
                null,
                out origin_searchresults,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            long lResultCount = lRet;

            searchresults = new CallNumberSearchResult[origin_searchresults.Length];

            for (int i = 0; i < origin_searchresults.Length; i++)
            {
                CallNumberSearchResult item = new CallNumberSearchResult();

                Record record = origin_searchresults[i];
                item.ItemRecPath = record.Path;
                searchresults[i] = item;

                string strLocation = "";
                item.CallNumber = BuildAccessNoKeyString(record.Keys, out strLocation);

                if (bCols == true && record.Cols != null)
                {
                    if (record.Cols.Length > 0)
                        item.ParentID = record.Cols[0];
                    if (record.Cols.Length > 1)
                        item.Location = record.Cols[1];
                    if (record.Cols.Length > 2)
                        item.Barcode = record.Cols[2];
                }

                if (string.IsNullOrEmpty(item.Location) == true)
                    item.Location = strLocation;    // 用从keys中得来的代替。可能有大小写的差异 --- keys中都是大写
#if NO
                if (bCols == true)
                {
                    // 继续填充其余成员
                    string strXml = "";
                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(item.ItemRecPath,
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        item.ErrorInfo = "获取记录 '" + item.ItemRecPath + "' 出错: " + strError;
                        continue;
                    }

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        item.ErrorInfo = "记录 '" + item.ItemRecPath + "' XML装载到DOM时出错: " + ex.Message;
                        continue;
                    }

                    /*
                    item.CallNumber = DomUtil.GetElementText(dom.DocumentElement,
                        "accessNo");
                     * */
                    item.ParentID = DomUtil.GetElementText(dom.DocumentElement,
                        "parent");
                    item.Location = DomUtil.GetElementText(dom.DocumentElement,
                        "location");
                    item.Barcode = DomUtil.GetElementText(dom.DocumentElement,
                        "barcode");
                }
#endif
            }

            result.Value = lResultCount;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        public static string BuildAccessNoKeyString(KeyFrom[] keys,
            out string strLocation)
        {
            strLocation = "";

            if (keys == null || keys.Length == 0)
                return "";
            string strValue = keys[0].Key;
            int nRet = strValue.IndexOf("|");
            if (nRet == -1)
                return strValue;
            strLocation = strValue.Substring(0, nRet);
            return strValue.Substring(nRet + 1);
        }

        // 获得种次号尾号
        public LibraryServerResult GetOneClassTailNumber(
            SessionInfo sessioninfo,
            string strArrangeGroupName,
            string strClass,
            out string strTailNumber)
        {
            strTailNumber = "";

            string strError = "";

            LibraryServerResult result = new LibraryServerResult();

            if (String.IsNullOrEmpty(strArrangeGroupName) == true)
            {
                strError = "strArrangeGroupName参数值不能为空";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strPath = "";
            string strXml = "";
            byte[] timestamp = null;
            // 检索尾号记录的路径和记录体
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = SearchOneClassTailNumberPathAndRecord(
                // sessioninfo.Channels,
                channel,
                strArrangeGroupName,
                strClass,
                out strPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                result.ErrorCode = ErrorCode.NotFound;
                result.ErrorInfo = strError;
                result.Value = 0;
                return result;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "尾号记录 '" + strPath + "' XML装入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            strTailNumber = DomUtil.GetAttr(dom.DocumentElement, "v");

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 设置种次号尾号
        public LibraryServerResult SetOneClassTailNumber(
            SessionInfo sessioninfo,
            string strAction,
            string strArrangeGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber)
        {
            strOutputNumber = "";

            string strError = "";

            LibraryServerResult result = new LibraryServerResult();

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strPath = "";
            string strXml = "";
            byte[] timestamp = null;
            // 检索尾号记录的路径和记录体
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = SearchOneClassTailNumberPathAndRecord(
                // sessioninfo.Channels,
                channel,
                strArrangeGroupName,
                strClass,
                out strPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strZhongcihaoDbName = GetTailDbName(strArrangeGroupName);
            if (String.IsNullOrEmpty(strZhongcihaoDbName) == true)
            {
                // TODO: 这里报错还需要精确一些，对于带有'!'的馆藏地点名
                strError = "无法通过排架体系名 '" + strArrangeGroupName + "' 获得种次号库名";
                goto ERROR1;
            }

            // byte[] baOutputTimestamp = null;
            bool bNewRecord = false;
            long lRet = 0;

#if NO
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
#endif

            byte[] output_timestamp = null;
            string strOutputPath = "";

            if (strAction == "conditionalpush")
            {

                if (nRet == 0)
                {
                    // 新创建记录
                    strPath = strZhongcihaoDbName + "/?";
                    strXml = "<r c='" + strClass + "' v='" + strTestNumber + "'/>";

                    bNewRecord = true;
                }
                else
                {
                    string strPartXml = "/xpath/<locate>@v</locate><action>Push</action>";
                    strPath += strPartXml;
                    strXml = strTestNumber;

                    bNewRecord = false;
                }

                lRet = channel.DoSaveTextRes(strPath,
                    strXml,
                    false,
                    "content",
                    timestamp,   // timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存尾号记录时出错: " + strError;
                    goto ERROR1;
                }

                if (bNewRecord == true)
                {
                    strOutputNumber = strTestNumber;
                }
                else
                {
                    strOutputNumber = strError;
                }

                goto END1;
            }
            else if (strAction == "increase")
            {
                string strDefaultNumber = strTestNumber;

                if (nRet == 0)
                {
                    // 新创建记录
                    strPath = strZhongcihaoDbName + "/?";
                    strXml = "<r c='" + strClass + "' v='" + strDefaultNumber + "'/>";

                    bNewRecord = true;
                }
                else
                {
                    string strPartXml = "/xpath/<locate>@v</locate><action>+AddInteger</action>";
                    strPath += strPartXml;
                    strXml = "1";

                    bNewRecord = false;
                }

                // 
                lRet = channel.DoSaveTextRes(strPath,
                    strXml,
                    false,
                    "content",
                    timestamp,   // timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存尾号记录时出错: " + strError;
                    goto ERROR1;
                }

                if (bNewRecord == true)
                {
                    strOutputNumber = strDefaultNumber;
                }
                else
                {
                    strOutputNumber = strError;
                }

                goto END1;
            }
            else if (strAction == "save")
            {
                string strTailNumber = strTestNumber;

                if (nRet == 0)
                {
                    strPath = strZhongcihaoDbName + "/?";
                }
                else
                {
                    // 覆盖记录
                    if (String.IsNullOrEmpty(strPath) == true)
                    {
                        strError = "记录存在时strPath居然为空";
                        goto ERROR1;
                    }
                }

                strXml = "<r c='" + strClass + "' v='" + strTailNumber + "'/>";

                lRet = channel.DoSaveTextRes(strPath,
    strXml,
    false,
    "content",
    timestamp,   // timestamp,
    out output_timestamp,
    out strOutputPath,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        strError = "尾号记录时间戳不匹配，说明可能被他人修改过。详细原因: " + strError;
                        goto ERROR1;
                    }

                    strError = "保存尾号记录时出错: " + strError;
                    goto ERROR1;
                }

            }
            else
            {
                strError = "无法识别的strAction参数值 '" + strAction + "'";
                goto ERROR1;
            }

        END1:
            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 检索尾号记录的路径和记录体
        // return:
        //      -1  error(注：检索命中多条情况被当作错误返回)
        //      0   not found
        //      1   found
        public int SearchOneClassTailNumberPathAndRecord(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strArrangeGroupName,
            string strClass,
            out string strPath,
            out string strXml,
            out byte[] timestamp,
            out string strError)
        {
            strError = "";
            strPath = "";
            strXml = "";
            timestamp = null;

            if (strClass == "")
            {
                strError = "尚未指定分类号";
                return -1;
            }

            if (strArrangeGroupName == "")
            {
                strError = "尚未指定排架体系名";
                return -1;
            }


            string strZhongcihaoDbName = GetTailDbName(strArrangeGroupName);
            if (String.IsNullOrEmpty(strZhongcihaoDbName) == true)
            {
                strError = "无法通过排架体系名 '" + strArrangeGroupName + "' 获得种次号库名";
                return -1;
            }

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strZhongcihaoDbName + ":" + "分类号")       // 2007/9/14 
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strClass)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            List<string> aPath = null;
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
                out aPath,
                out timestamp,
                out strError);
            if (nRet == -1)
            {
                strError = "检索库 " + strZhongcihaoDbName + " 时出错: " + strError;
                return -1;
            }
            if (nRet == 0)
            {
                return 0;	// 没有找到
            }

            if (nRet > 1)
            {
                strError = "以分类号'" + strClass + "'检索库 " + strZhongcihaoDbName + " 时命中 " + Convert.ToString(nRet) + " 条，无法取得尾号。请修改库 '" + strZhongcihaoDbName + "' 中相应记录，确保同一类目只有一条对应的记录。";
                return -1;
            }

            Debug.Assert(aPath.Count >= 1, "");
            strPath = aPath[0];

            return 1;
        }
    }

    // 索取号检索命中结果的一行
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class CallNumberSearchResult
    {
        [DataMember]
        public string ItemRecPath = "";    // 实体记录路径
        [DataMember]
        public string CallNumber = "";  // 索取号全部
        [DataMember]
        public string Location = "";    // 馆藏地点
        [DataMember]
        public string Barcode = ""; // 册条码号
        // public string[] Cols = null;    // 其余的列。一般为题名、作者，或者书目摘要
        [DataMember]
        public string ParentID = "";    // 父(书目)记录ID
        [DataMember]
        public string ErrorInfo = "";   // 出错信息
    }

}
