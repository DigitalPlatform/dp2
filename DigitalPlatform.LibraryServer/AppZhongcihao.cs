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


namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和种次号功能相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 通过种次号组名获得种次号库名
        // parameters:
        //      strZhongcihaoGroupName  @引导种次号库名 !引导线索书目库名 否则就是 种次号组名
        string GetZhongcihaoDbName(string strZhongcihaoGroupName)
        {
            if (String.IsNullOrEmpty(strZhongcihaoGroupName) == true)
                return null;

            // 2012/11/8
            // @引导种次号库名
            if (strZhongcihaoGroupName[0] == '@')
            {
                return strZhongcihaoGroupName.Substring(1);
            }

            // !引导线索书目库名
            if (strZhongcihaoGroupName[0] == '!')
            {
                string strTemp = GetZhongcihaoGroupName(strZhongcihaoGroupName.Substring(1));

                if (strTemp == null)
                {
                    /*
                    strError = "书目库名 " + strZhongcihaoGroupName.Substring(1) + " 没有找到对应的种次号组名";
                    goto ERROR1;
                     * */
                    return null;
                }
                strZhongcihaoGroupName = strTemp;
            }

            // 否则就是 种次号组名
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhongcihao/group[@name='"+strZhongcihaoGroupName+"']");
            if (node == null)
                return null;

            return DomUtil.GetAttr(node, "zhongcihaodb");
        }

        // 检索尾号记录的路径和记录体
        // return:
        //      -1  error(注：检索命中多条情况被当作错误返回)
        //      0   not found
        //      1   found
        public int SearchTailNumberPathAndRecord(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strZhongcihaoGroupName,
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

            if (strZhongcihaoGroupName == "")
            {
                strError = "尚未指定种次号组名";
                return -1;
            }


            string strZhongcihaoDbName = GetZhongcihaoDbName(strZhongcihaoGroupName);
            if (String.IsNullOrEmpty(strZhongcihaoDbName) == true)
            {
                strError = "无法通过种次号组名 '" + strZhongcihaoGroupName + "' 获得种次号库名";
                return -1;
            }

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strZhongcihaoDbName + ":" + "分类号")       // 2007/9/14 
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strClass)
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

        // 获得种次号尾号
        public LibraryServerResult GetZhongcihaoTailNumber(
            SessionInfo sessioninfo,
            string strZhongcihaoGroupName,
            string strClass,
            out string strTailNumber)
        {
            strTailNumber = "";

            string strError = "";

            LibraryServerResult result = new LibraryServerResult();

            if (String.IsNullOrEmpty(strZhongcihaoGroupName) == true)
            {
                strError = "种次号组名参数值不能为空";
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
            int nRet = SearchTailNumberPathAndRecord(
                // sessioninfo.Channels,
                channel,
                strZhongcihaoGroupName,
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

        // TODO: 读取和修改都要放到一个锁定范围内，避免并发修改同一条种次号库记录
        // TODO: 尾号变化要写入操作日志。日志恢复功能可以暂时忽略这些新动作类型。
        // 设置种次号尾号
        // parameters:
        //      strAction   动作。conditionalpush, increase, +increase, increase+, save
        //      strZhongcihaoGroupName  种次号组名。如果是 '@' 打头，则表示种次号库名
        public LibraryServerResult SetZhongcihaoTailNumber(
            SessionInfo sessioninfo,
            string strAction,
            string strZhongcihaoGroupName,
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
            int nRet = SearchTailNumberPathAndRecord(
                // sessioninfo.Channels,
                channel,
                strZhongcihaoGroupName,
                strClass,
                out strPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strZhongcihaoDbName = GetZhongcihaoDbName(strZhongcihaoGroupName);
            if (String.IsNullOrEmpty(strZhongcihaoDbName) == true)
            {
                strError = "无法通过种次号组名 '" + strZhongcihaoGroupName + "' 获得种次号库名";
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
            else if (strAction == "increase" || strAction == "+increase" || strAction == "increase+")
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

                    // 2012/11/8
                    if (strAction == "increase+")
                          strPartXml = "/xpath/<locate>@v</locate><action>AddInteger+</action>";

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

                // 2025/10/23
                strOutputNumber = strTailNumber;
            }
            else if (strAction == "delete")
            {
                // 2025/10/25 新增 delete 动作

                string strTailNumber = strTestNumber;

                if (nRet == 0)
                {
                    strError = $"组名为 '{strZhongcihaoGroupName}' 类名为 '{strClass}' 的尾号没有找到";
                    goto ERROR1;
                }
                else
                {
                    // 删除记录
                    if (String.IsNullOrEmpty(strPath) == true)
                    {
                        strError = "记录存在时strPath居然为空";
                        goto ERROR1;
                    }
                }

                // TODO: 要求 strTailNumber 参数给出的尾号，要和即将删除的 XML 记录中的尾号一致，才允许删除

                lRet = channel.DoDeleteRes(strPath,
    timestamp,   // timestamp,
    out output_timestamp,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        strError = $"删除尾号记录 {strPath} 失败: 尾号记录时间戳不匹配，说明可能被他人修改过。详细原因: {strError}";
                        goto ERROR1;
                    }

                    strError = $"删除尾号记录 {strPath} 时出错: " + strError;
                    goto ERROR1;
                }

                strOutputNumber = "";
            }
            else
            {
                strError = "无法识别的 strAction 参数值 '" + strAction + "'";
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

        // 通过书目库名得到种次号group名
        string GetZhongcihaoGroupName(string strBiblioDbName)
        {
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhongcihao/group[./database[@name='"+strBiblioDbName+"']]");
            if (node == null)
                return null;

            return DomUtil.GetAttr(node, "name");
        }

        static string GetZhongcihaoPart(string strText)
        {
            int nRet = strText.IndexOf("/");
            if (nRet == -1)
                return strText;

            return strText.Substring(nRet + 1).Trim();
        }

        public static string BuildZhongcihaoString(DigitalPlatform.rms.Client.rmsws_localhost.KeyFrom[] keys)
        {
            if (keys == null || keys.Length == 0)
                return "";
            /*
            foreach (KeyFrom entry in keys)
            {
                return GetZhongcihaoPart(entry.Key);
            }

            return "";
             * */
            return GetZhongcihaoPart(keys[0].Key);
        }

        public LibraryServerResult GetZhongcihaoSearchResult(
    SessionInfo sessioninfo,
    string strZhongcihaoGroupName,
    string strResultSetName,
    long lStart,
    long lCount,
    string strBrowseInfoStyle,
    string strLang,
    out ZhongcihaoSearchResult[] searchresults)
        {
            string strError = "";
            searchresults = null;

            LibraryServerResult result = new LibraryServerResult();
            // int nRet = 0;
            long lRet = 0;

            if (String.IsNullOrEmpty(strZhongcihaoGroupName) == true)
            {
                strError = "strZhongcihaoGroupName参数值不能为空";
                goto ERROR1;
            }

            if (strZhongcihaoGroupName[0] == '!')
            {
                string strTemp = GetZhongcihaoGroupName(strZhongcihaoGroupName.Substring(1));

                if (strTemp == null)
                {
                    strError = "书目库名 " + strZhongcihaoGroupName.Substring(1) + " 没有找到对应的种次号组名";
                    goto ERROR1;
                }
                strZhongcihaoGroupName = strTemp;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                result.Value = -1;
                result.ErrorInfo = "get channel error";
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }

            /*

// 
XmlNode nodeNsTable = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhongcihao/nstable");

XmlNamespaceManager mngr = null;

if (nodeNsTable != null)
{
    // 准备名字空间环境
    nRet = PrepareNs(
        nodeNsTable,
        out mngr,
        out strError);
    if (nRet == -1)
        goto ERROR1;
}

// 构造数据库定义和库名的对照表
XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//zhongcihao/group[@name='" + strZhongcihaoGroupName + "']/database");
if (nodes.Count == 0)
{
    strError = "library.xml中尚未配置有关 '" + strZhongcihaoGroupName + "'的<zhongcihao>/<group>/<database>相关参数";
    goto ERROR1;
}

Hashtable db_prop_table = new Hashtable();

for (int i = 0; i < nodes.Count; i++)
{
    XmlNode node = nodes[i];

    DbZhongcihaoProperty prop = new DbZhongcihaoProperty();
    prop.DbNames = DomUtil.GetAttr(node, "name");
    prop.NumberXPath = DomUtil.GetAttr(node, "rightxpath");
    prop.TitleXPath = DomUtil.GetAttr(node, "titlexpath");
    prop.AuthorXPath = DomUtil.GetAttr(node, "authorxpath");

    db_prop_table[prop.DbNames] = prop;
}
 * */
            bool bCols = (StringUtil.IsInList("cols", strBrowseInfoStyle) == true);
            string strBrowseStyle = "keyid,key,id";
            if (bCols == true)
                strBrowseStyle += ",cols";

            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

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

            searchresults = new ZhongcihaoSearchResult[origin_searchresults.Length];

            for (int i = 0; i < origin_searchresults.Length; i++)
            {
                ZhongcihaoSearchResult item = new ZhongcihaoSearchResult();

                Record origin_item =  origin_searchresults[i];

                item.Path = origin_item.Path;
                searchresults[i] = item;
                item.Zhongcihao = BuildZhongcihaoString(origin_item.Keys);
                item.Cols = origin_item.Cols;
            }


#if NO
            List<string> pathlist = new List<string>();

            searchresults = new ZhongcihaoSearchResult[origin_searchresults.Length];

            for (int i = 0; i < origin_searchresults.Length; i++)
            {
                ZhongcihaoSearchResult item = new ZhongcihaoSearchResult();

                item.Path = origin_searchresults[i].Path;
                searchresults[i] = item;
                item.Zhongcihao = BuildZhongcihaoString(origin_searchresults[i].Keys);

                if (bCols == true)
                    pathlist.Add(item.Path);
            }

            if (pathlist.Count > 0)
            {
                // string[] paths = new string[pathlist.Count];
                string[] paths = StringUtil.FromListString(pathlist);

                ArrayList aRecord = null;

                nRet = channel.GetBrowseRecords(paths,
                    "cols",
                    out aRecord,
                    out strError);
                if (nRet == -1)
                {
                    strError = "GetBrowseRecords() error: " + strError;
                    goto ERROR1;
                }

                int j = 0;
                for (int i = 0; i < searchresults.Length; i++)
                {
                    ZhongcihaoSearchResult result_item = searchresults[i];
                    if (result_item.Path != pathlist[j])
                        continue;

                    string[] cols = (string[])aRecord[j];

                    result_item.Cols = cols;   // style中不包含id
                    j++;
                    if (j >= pathlist.Count)
                        break;
                }
            }

#endif

            result.Value = lResultCount;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

#if OLD
        public LibraryServerResult GetZhongcihaoSearchResult(
            SessionInfo sessioninfo,
            string strZhongcihaoGroupName,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out ZhongcihaoSearchResult[] searchresults)
        {
            string strError = "";
            searchresults = null;

            LibraryServerResult result = new LibraryServerResult();
            int nRet = 0;
            long lRet = 0;

            if (String.IsNullOrEmpty(strZhongcihaoGroupName) == true)
            {
                strError = "strZhongcihaoGroupName参数值不能为空";
                goto ERROR1;
            }

            if (strZhongcihaoGroupName[0] == '!')
            {
                string strTemp = GetZhongcihaoGroupName(strZhongcihaoGroupName.Substring(1));

                if (strTemp == null)
                {
                    strError = "书目库名 " + strZhongcihaoGroupName.Substring(1) + " 没有找到对应的种次号组名";
                    goto ERROR1;
                }
                strZhongcihaoGroupName = strTemp;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                result.Value = -1;
                result.ErrorInfo = "get channel error";
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }

            // 
            XmlNode nodeNsTable = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhongcihao/nstable");

            XmlNamespaceManager mngr = null;

            if (nodeNsTable != null)
            {
                // 准备名字空间环境
                nRet = PrepareNs(
                    nodeNsTable,
                    out mngr,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 构造数据库定义和库名的对照表
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//zhongcihao/group[@name='" + strZhongcihaoGroupName + "']/database");
            if (nodes.Count == 0)
            {
                strError = "library.xml中尚未配置有关 '" + strZhongcihaoGroupName + "'的<zhongcihao>/<group>/<database>相关参数";
                goto ERROR1;
            }

            Hashtable db_prop_table = new Hashtable();

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                DbZhongcihaoProperty prop = new DbZhongcihaoProperty();
                prop.DbName = DomUtil.GetAttr(node, "name");
                prop.NumberXPath = DomUtil.GetAttr(node, "rightxpath");
                prop.TitleXPath = DomUtil.GetAttr(node, "titlexpath");
                prop.AuthorXPath = DomUtil.GetAttr(node, "authorxpath");

                db_prop_table[prop.DbName] = prop;
            }

            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

            Record[] origin_searchresults = null; // 

            lRet = channel.DoGetSearchResult(
                strResultSetName,
                lStart,
                lCount,
                "id",
                strLang,
                null,
                out origin_searchresults,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            long lResultCount = lRet;

            searchresults = new ZhongcihaoSearchResult[origin_searchresults.Length];

            for (int i = 0; i < origin_searchresults.Length; i++)
            {
                ZhongcihaoSearchResult item = new ZhongcihaoSearchResult();

                item.Path = origin_searchresults[i].Path;
                searchresults[i] = item;

                // 继续填充其余成员
                string strXml = "";
                string strMetaData = "";
                byte[] timestamp = null;
                string strOutputPath = "";

                lRet = channel.GetRes(item.Path,
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    item.Zhongcihao = "获取记录 '" + item.Path + "' 出错: " + strError;
                    continue;
                }

                string strDbName = ResPath.GetDbName(item.Path);

                DbZhongcihaoProperty prop = (DbZhongcihaoProperty)db_prop_table[strDbName];
                if (prop == null)
                {
                    item.Zhongcihao = "数据库名 '" + strDbName + "' 不在定义的种次号特性(<zhongcihao>/<group>/<database>)中";
                    continue;
                }

                string strNumber = "";
                string strTitle = "";
                string strAuthor = "";

                nRet = GetRecordProperties(
                    strXml,
                    prop,
                    mngr,
                    out strNumber,
                    out strTitle,
                    out strAuthor,
                    out strError);
                if (nRet == -1)
                {
                    item.Zhongcihao = strError;
                    continue;
                }

                item.Zhongcihao = strNumber;
                item.Cols = new string[2];
                item.Cols[0] = strTitle;
                item.Cols[1] = strAuthor;
            }


            result.Value = lResultCount;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        /// <summary>
        /// 准备名字空间环境
        /// </summary>
        /// <param name="nodeNsTable">nstable节点</param>
        /// <param name="mngr">返回名字空间管理器对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>0</returns>
        static int PrepareNs(
            XmlNode nodeNsTable,
            out XmlNamespaceManager mngr,
            out string strError)
        {
            strError = "";
            mngr = new XmlNamespaceManager(new NameTable());
            XmlNodeList nodes = nodeNsTable.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strPrefix = DomUtil.GetAttr(node, "prefix");
                string strUri = DomUtil.GetAttr(node, "uri");

                mngr.AddNamespace(strPrefix, strUri);
            }

            return 0;
        }

        int GetRecordProperties(
            string strXml,
            DbZhongcihaoProperty prop,
            XmlNamespaceManager mngr,
            out string strNumber,
            out string strTitle,
            out string strAuthor,
            out string strError)
        {
            strNumber = "";
            strTitle = "";
            strAuthor = "";
            strError = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            try
            {

                if (prop.NumberXPath != "")
                {
                    XmlNode node = dom.DocumentElement.SelectSingleNode(prop.NumberXPath, mngr);
                    if (node != null)
                        strNumber = node.Value;
                }

                if (prop.TitleXPath != "")
                {
                    XmlNode node = dom.DocumentElement.SelectSingleNode(prop.TitleXPath, mngr);
                    if (node != null)
                        strTitle = node.Value;
                }

                if (prop.AuthorXPath != "")
                {
                    XmlNode node = dom.DocumentElement.SelectSingleNode(prop.AuthorXPath, mngr);
                    if (node != null)
                        strAuthor = node.Value;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return 0;
        }
#endif

        // 检索同类记录
        public LibraryServerResult SearchUsedZhongcihao(
            SessionInfo sessioninfo,
            string strZhongcihaoGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml)
        {
            strQueryXml = "";

            string strError = "";

            LibraryServerResult result = new LibraryServerResult();

            if (String.IsNullOrEmpty(strZhongcihaoGroupName) == true)
            {
                strError = "strZhongcihaoGroupName参数值不能为空";
                goto ERROR1;
            }

            if (strZhongcihaoGroupName[0] == '!')
            {
                string strTemp = GetZhongcihaoGroupName(strZhongcihaoGroupName.Substring(1));

                if (strTemp == null)
                {
                    strError = "书目库名 " + strZhongcihaoGroupName.Substring(1) + " 没有找到对应的种次号组名";
                    goto ERROR1;
                }
                strZhongcihaoGroupName = strTemp;
            }


            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//zhongcihao/group[@name='" + strZhongcihaoGroupName + "']/database");
            if (nodes.Count == 0)
            {
                strError = "library.xml中尚未配置有关 '" + strZhongcihaoGroupName + "'的<zhongcihao>/<group>/<database>相关参数";
                goto ERROR1;
            }

            // 构造检索式
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strDbName = DomUtil.GetAttr(node, "name");

                if (string.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "<database>元素必须有非空的name属性值";
                    goto ERROR1;
                }

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                if (i > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "索取号")       // 2007/9/14
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strClass) + "/"
                    + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            }

            if (nodes.Count > 0)
                strQueryXml = "<group>" + strQueryXml + "</group>";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,   // "default",
                "keyid", // strOuputStyle
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


#if OLD
        // 检索同类记录
        public LibraryServerResult SearchUsedZhongcihao(
            SessionInfo sessioninfo,
            string strZhongcihaoGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml)
        {
            strQueryXml = "";

            string strError = "";

            LibraryServerResult result = new LibraryServerResult();

            if (String.IsNullOrEmpty(strZhongcihaoGroupName) == true)
            {
                strError = "strZhongcihaoGroupName参数值不能为空";
                goto ERROR1;
            }

            if (strZhongcihaoGroupName[0] == '!')
            {
                string strTemp = GetZhongcihaoGroupName(strZhongcihaoGroupName.Substring(1));

                if (strTemp == null)
                {
                    strError = "书目库名 " + strZhongcihaoGroupName.Substring(1) + " 没有找到对应的种次号组名";
                    goto ERROR1;
                }
                strZhongcihaoGroupName = strTemp;
            }


            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//zhongcihao/group[@name='" + strZhongcihaoGroupName + "']/database");
            if (nodes.Count == 0)
            {
                strError = "library.xml中尚未配置有关 '"+strZhongcihaoGroupName+"'的<zhongcihao>/<group>/<database>相关参数";
                goto ERROR1;
            }

            // 构造检索式
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strDbName = DomUtil.GetAttr(node, "name");
                string strLeftFrom = DomUtil.GetAttr(node, "leftfrom");

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                if (i > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strLeftFrom)       // 2007/9/14
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strClass)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            }

            if (nodes.Count > 0)
                strQueryXml = "<group>" + strQueryXml + "</group>";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,   // "default",
                "", // strOuputStyle
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
#endif


    }

    // 种次号检索命中结果的一行
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class ZhongcihaoSearchResult
    {
        [DataMember]
        public string Path = "";    // 记录路径
        [DataMember]
        public string Zhongcihao = "";  // 同类书区分号
        [DataMember]
        public string[] Cols = null;    // 其余的列。一般为题名、作者，或者书目摘要
    }

    // 数据库的有关种次号的特性
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class DbZhongcihaoProperty
    {
        [DataMember]
        public string DbName = "";
        [DataMember]
        public string NumberXPath = ""; // "rightxpath"
        [DataMember]
        public string TitleXPath = "";  // "titlexpath"
        [DataMember]
        public string AuthorXPath = ""; // "authorxpath"

    }
}
