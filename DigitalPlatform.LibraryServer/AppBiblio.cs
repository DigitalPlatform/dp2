using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Core;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是编目业务相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        public const int QUOTA_SIZE = (int)((double)(1024 * 1024) * (double)0.8);   // 经过试验 0.5 基本可行 因为字符数换算为 byte 数，中文的缘故

        // 是否允许删除带有下级记录的书目记录
        public bool DeleteBiblioSubRecords = true;

        public Int64 BiblioSearchMaxCount = -1;

        // 获得目标记录路径。998$t
        // return:
        //      -1  error
        //      0   OK
        public static int GetTargetRecPath(string strBiblioXml,
            out string strTargetRecPath,
            out string strError)
        {
            strError = "";
            strTargetRecPath = "";

            XmlDocument bibliodom = new XmlDocument();
            try
            {
                bibliodom.LoadXml(strBiblioXml);
            }
            catch (Exception ex)
            {
                strError = "书目记录XML装载到DOM出错:" + ex.Message;
                return -1;
            }

            XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
            mngr.AddNamespace("dprms", DpNs.dprms);

            XmlNode node = null;
            string strXPath = "";
            if (bibliodom.DocumentElement.NamespaceURI == Ns.usmarcxml)
            {
                mngr.AddNamespace("usmarc", Ns.usmarcxml);	// "http://www.loc.gov/MARC21/slim"


                strXPath = "//usmarc:record/usmarc:datafield[@tag='998']/usmarc:subfield[@code='t']";

                // string d = "";

                node = bibliodom.SelectSingleNode(strXPath, mngr);
                if (node != null)
                    strTargetRecPath = node.InnerText;

                return 1;
            }
            else if (bibliodom.DocumentElement.NamespaceURI == DpNs.unimarcxml)
            {
                mngr.AddNamespace("unimarc", DpNs.unimarcxml);	// "http://dp2003.com/UNIMARC"
                strXPath = "//unimarc:record/unimarc:datafield[@tag='998']/unimarc:subfield[@code='t']";

                node = bibliodom.SelectSingleNode(strXPath, mngr);
                if (node != null)
                    strTargetRecPath = node.InnerText;

                return 1;
            }
            else
            {
                strError = "无法识别的MARC格式";
                return -1;
            }

            // return 1;
        }

        // 兼容以前的 library.xml 内脚本
        public LibraryServerResult GetBiblioInfos(
    SessionInfo sessioninfo,
    string strBiblioRecPath,
    string[] formats,
    out string[] results,
    out byte[] timestamp)
        {
            return GetBiblioInfos(sessioninfo,
                strBiblioRecPath,
                "",
                formats,
                out results,
                out timestamp);
        }

        int BatchGetBiblioSummary(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            out List<String> result_strings,
            out string strError)
        {
            strError = "";
            result_strings = new List<string>();

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                return -1;
            }

            List<string> commands = new List<string>();
            string strText = strBiblioRecPath.Substring("@path-list:".Length);

            commands = StringUtil.SplitList(strText);
            // string strErrorText = "";
            foreach (string command in commands)
            {
                if (string.IsNullOrEmpty(command) == true)
                    continue;

                string strItemBarcode = command;

                if (strItemBarcode.StartsWith("@itemBarcode:") == true)
                    strItemBarcode = strItemBarcode.Substring("@itemBarcode:".Length);
                else
                    strItemBarcode = "@bibliorecpath:" + strItemBarcode;

                string strOutputBiblioRecPath = "";
                string strSummary = "";
                LibraryServerResult result = GetBiblioSummary(
            sessioninfo,
            channel,
            strItemBarcode,
            "", // strConfirmItemRecPath,
            "", // strBiblioRecPathExclude,
            out strOutputBiblioRecPath,
            out strSummary);
                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCode.NotFound)
                        result_strings.Add("");
                    else
                        result_strings.Add("!" + result.ErrorInfo);
                    /*
                    strError = result.ErrorInfo;
                    return -1;
                     * */
                }
                else
                    result_strings.Add(strSummary);
            }
            return 0;
        }

        static string GetBiblioInfoAction(string strDbType)
        {
            return (strDbType == "biblio" ? "getbiblioinfo" : "getauthorityinfo");
        }

        static string GetBiblioSummaryAction(string strDbType)
        {
            return (strDbType == "biblio" ? "getbibliosummary" : "getauthoritysummary");
        }

        // 获得书目信息
        // 
        // TODO: 将来可以增加在strBiblioRecPath中允许多种检索入口的能力，比方说允许使用itembarcode和itemconfirmpath(甚至和excludebibliopath)结合起来定位种。这样就完全可以取代原有GetBiblioSummary API的功能
        // parameters:
        //      strBiblioRecPath    种记录路径。如果在最后接续"$prev" "$next"，表示前一条或后一条。
        //      formats     希望获得信息的若干格式。如果 == null，表示希望只返回timestamp (results返回空)
        //                  可以用多种格式：xml html text @??? summary outputpath
        // Result.Value -1出错 0没有找到 1找到
        // 附注:
        //      1)
        //      如果 strBiblioRecPath 为 @path-list: 引导的成批检索式，并且 formats 为唯一一个 "summary" 元素，则表示希望批获取摘要
        //      批检索式，为逗号间隔的元素列表，每个元素为 @itemBarcode: 或 @bibliorecpath 引导的内容
        //      results 中每个元素为一个 summary 内容
        //      2)
        //      如果 strBiblioRecPath 为 @path-list: 引导的成批检索式，format 为多种格式，则 results 中会返回 格式个数 X 检索词数量 这么多的元素
        public LibraryServerResult GetBiblioInfos(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            string strBiblioXmlParam,    // 2013/3/6
            string[] formats,
            out string[] results,
            out byte[] timestamp)
        {
            results = null;
            timestamp = null;

            LibraryServerResult result = new LibraryServerResult();

            int nRet = 0;
            long lRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                strError = "strBiblioRecPath参数不能为空";
                goto ERROR1;
            }

            if (formats != null && formats.Length == 1 && formats[0] == "summary"
                && strBiblioRecPath.StartsWith("@path-list:")   // 2016/4/15 增加
                && string.IsNullOrEmpty(strBiblioXmlParam) == true)
            {
                List<String> temp_results = null;

                nRet = BatchGetBiblioSummary(
                    sessioninfo,
                    strBiblioRecPath,
                    out temp_results,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                results = temp_results.ToArray();
                result.Value = 1;
                return result;
            }

            // 检查特定格式的权限
            if (formats != null)
            {
                foreach (string format in formats)
                {
                    if (String.Compare(format, "summary", true) == 0)
                    {
                        // 权限字符串
                        if (StringUtil.IsInList("getbibliosummary,order", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "获取种摘要信息被拒绝。不具备order、getbibliosummary权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }
            }

            List<string> commands = new List<string>();
            List<string> biblio_records = new List<string>();

            if (StringUtil.HasHead(strBiblioRecPath, "@path-list:") == true
                || strBiblioRecPath.IndexOf(",") != -1) // 2016/4/15 增加
            {
                string strText = strBiblioRecPath;
                if (StringUtil.HasHead(strBiblioRecPath, "@path-list:") == true)
                    strText = strBiblioRecPath.Substring("@path-list:".Length);

                commands = StringUtil.SplitList(strText);

                // 如果前端发来记录，需要切割为独立的字符串
                if (string.IsNullOrEmpty(strBiblioXmlParam) == false)
                {
                    biblio_records = StringUtil.SplitList(strBiblioXmlParam.Replace("<!-->", new string((char)0x01, 1)), (char)0x01);
                    if (commands.Count != biblio_records.Count)
                    {
                        strError = "strBiblioXml 参数中包含的子串个数 " + biblio_records.Count.ToString() + " 和 strBiblioRecPath 中包含记录路径子串个数 " + commands.Count.ToString() + " 应该相等才对";
                        goto ERROR1;
                    }
                }
            }
            else
            {
                commands.Add(strBiblioRecPath);
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }

            int nPackageSize = 0;   // 估算通讯包的尺寸

            List<String> result_strings = new List<string>();
            string strErrorText = "";
            foreach (string command in commands)
            {
                if (string.IsNullOrEmpty(command) == true)
                    continue;

                string strOutputPath = "";
                string strCurrentBiblioRecPath = "";

                // 检查数据库路径，看看是不是已经正规定义的书目库？

                // 分离出命令部分
                string strCommand = "";
                nRet = command.IndexOf("$");
                if (nRet != -1)
                {
                    strCommand = command.Substring(nRet + 1);
                    strCurrentBiblioRecPath = command.Substring(0, nRet);
                }
                else
                    strCurrentBiblioRecPath = command;

                // 2016/1/2
                if (strCurrentBiblioRecPath.StartsWith("@itemBarcode:") == true)
                {
                    string strTemp = "";
                    string strItemBarcode = strCurrentBiblioRecPath.Substring("@itemBarcode:".Length);
                    nRet = GetBiblioRecPathByItemBarcode(
                        sessioninfo,
                        channel,
                        strItemBarcode,
                        out strTemp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "根据册条码号 '" + strItemBarcode + "' 获取书目记录路径时出错: " + strError;
                        goto ERROR1;
                    }
                    if (nRet == 0)
                    {
                        result_strings.AddRange(new string[formats.Length]);
                        continue;
                    }
                    strCurrentBiblioRecPath = strTemp;
                }

                string strDbType = "";
                string strDbTypeCaption = "";
                string strBiblioDbName = ResPath.GetDbName(strCurrentBiblioRecPath);

                // TODO: 册条码号和册记录路径也应该允许
                if (IsBiblioDbName(strBiblioDbName) == true)
                {
                    strDbType = "biblio";
                    strDbTypeCaption = "书目";
                }
                else if (IsAuthorityDbName(strBiblioDbName) == true)
                {
                    strDbType = "authority";
                    strDbTypeCaption = "规范";
                }
                else
                {
                    strError = "书目记录路径 '" + strCurrentBiblioRecPath + "' 中包含的数据库名 '" + strBiblioDbName + "' 不是合法的书目或规范库名";
                    goto ERROR1;
                }

                // 2023/1/28
                if (IsDatabaseMetadataPath(sessioninfo, strCurrentBiblioRecPath) == false)
                {
                    strError = $"不允许使用 GetBiblioInfos() 获取路径为 '{strCurrentBiblioRecPath}' 的资源。请改用 GetRes() API";
                    goto ERROR1;
                }

                string strAccessParameters = "";
                bool bRightVerified = false;
                // 检查存取权限
                if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                {
#if OLDCODE
                    string strAction = "*";

                    // return:
                    //      null    指定的操作类型的权限没有定义
                    //      ""      定义了指定类型的操作权限，但是否定的定义
                    //      其它      权限列表。* 表示通配的权限列表
                    string strActionList = LibraryApplication.GetDbOperRights(sessioninfo.Access,
                        strBiblioDbName,
                        GetBiblioInfoAction(strDbType));
                    if (strActionList == null)
                    {
                        if (LibraryApplication.GetDbOperRights(sessioninfo.Access,
                            "",
                            GetBiblioInfoAction(strDbType)) != null)
                        {
                            strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 " + GetBiblioInfoAction(strDbType) + " 操作的存取权限";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                        else
                        {
                            // 没有定义任何 getbiblioinfo 的存取定义(虽然定义了其他操作的存取定义)
                            // 这时应该转过去看普通的权限
                            // TODO: 这种算法，速度较慢
                            goto VERIFY_NORMAL_RIGHTS;
                        }
                    }
                    if (strActionList == "*")
                    {
                        // 通配
                    }
                    else
                    {
                        if (IsInAccessList(strAction, strActionList, out strAccessParameters) == false)
                        {
                            strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 " + GetBiblioInfoAction(strDbType) + " 操作的存取权限";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    bRightVerified = true;
#endif
                    // 检查当前用户是否具备 GetBiblioInfo() API 的存取定义权限
                    // parameters:
                    //      check_normal_right 是否要连带一起检查普通权限？如果不连带，则本函数可能返回 "normal"，意思是需要追加检查一下普通权限
                    // return:
                    //      "normal"    (存取定义已经满足要求了，但)还需要进一步检查普通权限
                    //      null    具备权限
                    //      其它      不具备权限。文字是报错信息
                    var error = CheckGetBiblioInfoAccess(
                        sessioninfo,
                        strDbType,
                        strBiblioDbName,
                        false,
                        out strAccessParameters);
                    if (error == "normal")
                        goto VERIFY_NORMAL_RIGHTS;
                    if (error != null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = error;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    bRightVerified = true;
                }

            VERIFY_NORMAL_RIGHTS:
                if (bRightVerified == false)
                {
                    if (strDbType == "biblio")
                    {
                        // 权限字符串
                        if (StringUtil.IsInList("getbiblioinfo,order", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "获取书目信息被拒绝。不具备 getbiblioinfo 或 order 权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    if (strDbType == "authority")
                    {
                        // 权限字符串
                        if (StringUtil.IsInList("getauthorityinfo", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "获取规范信息被拒绝。不具备 getauthorityinfo 权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                // 检查特定格式的权限
                if (formats != null)
                {
                    foreach (string format in formats)
                    {
                        if (String.Compare(format, "summary", true) == 0)
                        {
                            // 检查存取权限
                            if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                            {
                                string strAction = "*";

                                // return:
                                //      null    指定的操作类型的权限没有定义
                                //      ""      定义了指定类型的操作权限，但是否定的定义
                                //      其它      权限列表。* 表示通配的权限列表
                                string strActionList = LibraryApplication.GetDbOperRights(sessioninfo.Access,
                                    strBiblioDbName,
                                    GetBiblioSummaryAction(strDbType));
                                if (strActionList == null)
                                {
                                    if (LibraryApplication.GetDbOperRights(sessioninfo.Access,
                                        "",
                                        GetBiblioSummaryAction(strDbType)) != null)
                                    {
                                        strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 " + GetBiblioSummaryAction(strDbType) + " 操作的存取权限";
                                        result.Value = -1;
                                        result.ErrorInfo = strError;
                                        result.ErrorCode = ErrorCode.AccessDenied;
                                        return result;
                                    }
                                    else
                                    {
                                        // 没有定义任何 getbibliosummary 的存取定义(虽然定义了其他操作的存取定义)
                                        // 这时应该转过去看普通的权限
                                        // TODO: 这种算法，速度较慢
                                        strActionList = "*";    // 为了能够通过验证
                                    }
                                }
                                if (strActionList == "*")
                                {
                                    // 通配
                                }
                                else
                                {
                                    if (IsInAccessList(strAction, strActionList, out strAccessParameters) == false)
                                    {
                                        strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 " + GetBiblioSummaryAction(strDbType) + " 操作的存取权限";
                                        result.Value = -1;
                                        result.ErrorInfo = strError;
                                        result.ErrorCode = ErrorCode.AccessDenied;
                                        return result;
                                    }
                                }
                            }
                        }
                    }
                }

                string strBiblioXml = "";
                string strMetaData = "";
                if (String.IsNullOrEmpty(strBiblioXmlParam) == false)
                {
                    // 前端已经发送过来一条记录

                    if (commands.Count > 1)
                    {
                        // 前端发来的书目记录是多个记录的形态
                        int index = commands.IndexOf(command);
                        strBiblioXml = biblio_records[index];
                    }
                    else
                        strBiblioXml = strBiblioXmlParam;
                }
                else
                {
                    string strStyle = "timestamp,outputpath";  // "metadata,timestamp,outputpath";

                    if (formats != null
                        && formats.Length > 0
                        // TODO: 仅当必要时候才获得书目 XML
                        )
                        strStyle += ",content,data";

                    // 2023/1/19
                    if (formats != null && Array.IndexOf(formats, "metadata") != -1)
                        strStyle += ",metadata";

                    string strSearchRecPath = strCurrentBiblioRecPath;  // 用于实际检索的路径，用于显示
                    /*
                    if (String.IsNullOrEmpty(strCommand) == false
                        && (strCommand == "prev" || strCommand == "next"))
                    {
                        strStyle += "," + strCommand;
                        strSearchRecPath += "," + strCommand;
                    }
                    */
                    // 2023/1/19
                    // 附加的一些 style。包括 prev next withresmetadata
                    // TODO: 可以把合并后 strStyle 内容中重复出现的一些值归并一下
                    if (string.IsNullOrEmpty(strCommand) == false)
                        strStyle += "," + strCommand.Replace("|", ",");

                    lRet = channel.GetRes(strCurrentBiblioRecPath,
                        strStyle,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                        {
                            if (commands.Count == 1)
                            {
                                result.Value = 0;
                                result.ErrorCode = ErrorCode.NotFound;  // 2009/8/8 
                                result.ErrorInfo = strDbTypeCaption + "记录 '" + strSearchRecPath + "' 不存在";  // 2009/8/8 
                                return result;
                            }
                            // 填入空字符串
                            if (formats != null)
                            {
                                for (int i = 0; i < formats.Length; i++)
                                {
                                    result_strings.Add("");
                                }
                                // strErrorText += "书目记录 '" + strCurrentBiblioRecPath + "' 不存在;\r\n";
                            }
                            continue;
                        }
                        strError = "获得" + strDbTypeCaption + "记录 '" + strSearchRecPath + "' 时出错: " + strError;
                        // 2019/5/28
                        result.Value = -1;
                        result.ErrorCode = ErrorCode.NotFoundObjectFile;
                        result.ErrorInfo = strError;
                        return result;
                        // goto ERROR1;
                    }

                    // 2014/12/16
                    strCurrentBiblioRecPath = strOutputPath;
                }

                // 2023/1/28 把这一段放到外面，让前端提交的 XML 记录也经过字段过滤步骤
                // 2013/3/6
                // 过滤字段内容
                // 没有 writeres 和 setobject 权限，也可以进入处理?
                if (string.IsNullOrEmpty(strAccessParameters) == false
                    // || !(StringUtil.IsInList("writeres", sessioninfo.Rights) == true || StringUtil.IsInList("setobject", sessioninfo.Rights) == true)
                    || StringUtil.IsInList("setobject,setbiblioobject", sessioninfo.Rights) == false
                    )
                {
                    // 根据字段权限定义过滤出允许的内容
                    // parameters:
                    //      strUserRights   用户权限。如果为 null，表示不启用过滤 856 字段功能
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   有部分字段被修改或滤除
                    nRet = FilterBiblioByFieldNameList(
#if USE_OBJECTRIGHTS
                            StringUtil.IsInList("objectRights", this.Function) == true ? sessioninfo.Rights : null,
#else
                        sessioninfo.Rights,
#endif
                        strAccessParameters,
                        ref strBiblioXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 根据账户权限中是否具备 getbiblioobject 或 getobject，决定是否滤除书目 XML 记录中的 dprms:file 元素
                // 注意判断针对 object 的写权限字段范围是否大于读。
                // 如果出现这种情况，可以有两种方法处理:
                // 1) 立即报错返回。因为如果不报错，前端提交的没有 file 元素的记录直接覆盖进去，原有记录的 file 元素就丢失了。
                // 2) SetBiblioInfo() API 内处理的时候，削弱写权限，等于用读和写的交集来决定那些字段可以写入。不过这样的缺点是系统给管理员发现系统表现不符合预期会感到困惑，不容易想到这是写权限字段范围大于读引起的保护性降格行为
                if (StringUtil.IsInList("getbiblioobject,getobject", sessioninfo.RightsOrigin) == false
                    && string.IsNullOrEmpty(strBiblioXml) == false)
                {
                    XmlDocument temp = new XmlDocument();
                    temp.LoadXml(strBiblioXml);
                    if (RemoveDprmsFile(temp))
                        strBiblioXml = temp.DocumentElement.OuterXml;
                }

                if (formats != null)
                {
                    // TODO: getbibliopart 应能返回函数的值
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   有警告信息返回在 strError 中
                    nRet = BuildFormats(
                        sessioninfo,
                        strCurrentBiblioRecPath,
                        strBiblioXml,
                        strOutputPath,
                        strMetaData,
                        timestamp,
                        formats,
                        out List<string> temp_results,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    strBiblioXml = "";  // 避免后面的循环以为是前端发过来的记录

                    int nSize = GetSize(temp_results);
                    if (nPackageSize > 0 && nPackageSize + nSize >= QUOTA_SIZE)
                        break;  // 没有返回全部事项就中断了

                    nPackageSize += nSize;

                    if (string.IsNullOrEmpty(strError) == false)
                        strErrorText += strError;

                    if (temp_results != null)
                        result_strings.AddRange(temp_results);
                }
            } // end of each command

            // 复制到结果中
            if (result_strings.Count > 0)
            {
                /*
                results = new string[result_strings.Count];
                result_strings.CopyTo(results);
                */
                results = result_strings.ToArray();

                if (String.IsNullOrEmpty(strErrorText) == false)
                {
                    // 统一报错
                    // strError = strErrorText;
                    // goto ERROR1;
                    result.ErrorInfo = strError;    // 2014/1/8
                }
            }

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        static int GetSize(List<string> list)
        {
            int nSize = 0;
            foreach (string s in list)
            {
                nSize += 100;   // 包装材料估算
                if (s != null)
                    nSize += s.Length;
            }

            return nSize;
        }

        // 构造各种书目格式
        // return:
        //      -1  出错
        //      0   成功
        //      1   有警告信息返回在 strError 中
        public int BuildFormats(
            SessionInfo sessioninfo,
            string strCurrentBiblioRecPath,
            string strBiblioXml,
            string strOutputPath,   // 记录的路径
            string strMetadata,     // 记录的metadata
            byte[] timestamp,
            string[] formats,
            out List<String> result_strings,
            out string strError)
        {
            strError = "";
            string strErrorText = "";
            result_strings = new List<string>();
            int nRet = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strBiblioDbName = ResPath.GetDbName(strCurrentBiblioRecPath);

            for (int i = 0; i < formats.Length; i++)
            {
                string strBiblioType = formats[i];
                string strBiblio = "";

                // 表明只需获取局部数据
                if (string.IsNullOrEmpty(strBiblioType) == false
                    && strBiblioType[0] == '@')
                {
                    if (String.IsNullOrEmpty(strBiblioXml) == true)
                    {
                        strBiblio = ""; //  "XML记录为空";
                        goto CONTINUE;
                    }

                    string strPartName = strBiblioType.Substring(1);

                    XmlDocument bibliodom = new XmlDocument();

                    try
                    {
                        bibliodom.LoadXml(strBiblioXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "将XML装入DOM时失败: " + ex.Message;
                        // goto ERROR1;
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }
                    int nResultValue = 0;

                    // 执行脚本函数GetBiblioPart
                    // parameters:
                    // return:
                    //      -2  not found script
                    //      -1  出错
                    //      0   成功
                    nRet = this.DoGetBiblioPartScriptFunction(
                        bibliodom,
                        strPartName,
                        out nResultValue,
                        out strBiblio,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                    {
                        strError = "获得书目记录 '" + strCurrentBiblioRecPath + "' 的局部 " + strBiblioType + " 时出错: " + strError;
                        // goto ERROR1;
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }
                }
                // 书目摘要
                else if (String.Compare(strBiblioType, "summary", true) == 0)
                {
                    if (String.IsNullOrEmpty(strBiblioXml) == true)
                    {
                        strBiblio = ""; // "XML记录为空";
                        goto CONTINUE;
                    }

                    // TODO: 是否要延迟获得书目记录?
                    SummaryItem summary = GetBiblioSummary(strCurrentBiblioRecPath);
                    if (summary != null && string.IsNullOrEmpty(summary.Summary) == false)
                    {
                        strBiblio = summary.Summary;
                        {
                            // 从存储中命中
                            if (this.Statis != null)
                                this.Statis.IncreaseEntryValue(
                                sessioninfo.LibraryCodeList,
                                "获取书目摘要",
                                "存储命中次",
                                1);
                        }
                    }
                    else
                    {
                        // 获得本地配置文件
                        string strLocalPath = "";

                        string strRemotePath = BrowseFormat.CanonicalizeScriptFileName(
                            ResPath.GetDbName(strCurrentBiblioRecPath),
                            "./cfgs/summary.fltx");

                        nRet = this.CfgsMap.MapFileToLocal(
                            // sessioninfo.Channels,
                            channel,
                            strRemotePath,
                            out strLocalPath,
                            out strError);
                        if (nRet == -1)
                        {
                            // goto ERROR1;
                            /*
                            if (String.IsNullOrEmpty(strErrorText) == false)
                                strErrorText += ";\r\n";
                            strErrorText += strError;
                            */
                            AppendErrorText(strError);
                            goto CONTINUE;
                        }
                        if (nRet == 0)
                        {
                            // 配置.fltx文件不存在, 再试探.cs文件
                            strRemotePath = BrowseFormat.CanonicalizeScriptFileName(
                            ResPath.GetDbName(strCurrentBiblioRecPath),
                            "./cfgs/summary.cs");

                            nRet = this.CfgsMap.MapFileToLocal(
                                // sessioninfo.Channels,
                                channel,
                                strRemotePath,
                                out strLocalPath,
                                out strError);
                            if (nRet == -1)
                            {
                                // goto ERROR1;
                                /*
                                if (String.IsNullOrEmpty(strErrorText) == false)
                                    strErrorText += ";\r\n";
                                strErrorText += strError;
                                */
                                AppendErrorText(strError);
                                goto CONTINUE;
                            }
                            if (nRet == 0)
                            {
                                strError = strRemotePath + "不存在...";
                                // goto ERROR1;
                                /*
                                if (String.IsNullOrEmpty(strErrorText) == false)
                                    strErrorText += ";\r\n";
                                strErrorText += strError;
                                */
                                AppendErrorText(strError);
                                goto CONTINUE;
                            }
                        }

                        bool bFltx = false;
                        // 如果是一般.cs文件, 还需要获得.cs.ref配置文件
                        if (IsCsFileName(strRemotePath) == true)
                        {
                            string strTempPath = "";
                            nRet = this.CfgsMap.MapFileToLocal(
                                // sessioninfo.Channels,
                                channel,
                                strRemotePath + ".ref",
                                out strTempPath,
                                out strError);
                            if (nRet == -1)
                            {
                                // goto ERROR1;
                                /*
                                if (String.IsNullOrEmpty(strErrorText) == false)
                                    strErrorText += ";\r\n";
                                strErrorText += strError;
                                */
                                AppendErrorText(strError);
                                goto CONTINUE;
                            }
                            bFltx = false;
                        }
                        else
                        {
                            bFltx = true;
                        }
                        string strSummary = "";

                        // 将种记录数据从XML格式转换为HTML格式
                        if (string.IsNullOrEmpty(strBiblioXml) == false)
                        {
                            if (bFltx == true)
                            {
                                string strFilterFileName = strLocalPath;
                                nRet = this.ConvertBiblioXmlToHtml(
                                        strFilterFileName,
                                        strBiblioXml,
                                        null,
                                        strCurrentBiblioRecPath,
                                        out strSummary,
                                        out strError);
                            }
                            else
                            {
                                nRet = this.ConvertRecordXmlToHtml(
                                    strLocalPath,
                                    strLocalPath + ".ref",
                                    strBiblioXml,
                                    strCurrentBiblioRecPath,   // 2009/10/18 
                                    out strSummary,
                                    out strError);
                            }
                            if (nRet == -1)
                            {
                                // goto ERROR1;
                                /*
                                if (String.IsNullOrEmpty(strErrorText) == false)
                                    strErrorText += ";\r\n";
                                strErrorText += strError;
                                */
                                AppendErrorText(strError);
                                goto CONTINUE;
                            }
                        }
                        else
                            strSummary = "";

                        // 注：这里并没有把产生好的摘要字符串写入 mongodb 缓存数据库
                        strBiblio = strSummary;
                    }
                }
                // 目标记录路径
                else if (String.Compare(strBiblioType, "targetrecpath", true) == 0)
                {
                    // 获得目标记录路径。998$t
                    // return:
                    //      -1  error
                    //      0   OK
                    nRet = GetTargetRecPath(strBiblioXml,
                        out strBiblio,
                        out strError);
                    if (nRet == -1)
                    {
                        // goto ERROR1;
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }
                }
                // 如果只需要种记录的XML格式
                else if (String.Compare(strBiblioType, "xml", true) == 0)
                {
                    strBiblio = strBiblioXml;
                }
                else if (IsResultType(strBiblioType, "iso2709") == true)
                {
                    List<string> parts = StringUtil.ParseTwoPart(strBiblioType, ":");
                    string strEncoding = parts[1];
                    if (string.IsNullOrEmpty(strEncoding))
                        strEncoding = "utf-8";

                    byte[] result = null;
                    Encoding targetEncoding = Encoding.GetEncoding(strEncoding);
                    nRet = GetIso2709(strBiblioXml,
                        targetEncoding,
                        out result,
                        out strError);
                    if (nRet == -1)
                    {
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }
                    strBiblio = Convert.ToBase64String(result);
                }
                // 2017/6/16
                else if (IsResultType(strBiblioType, "marc") == true
                    || IsResultType(strBiblioType, "marcquery") == true)
                {
                    // marc:syntax (或者 marcquery:syntax)表示希望返回 syntax|marc机内格式 这样的字符串；marc 表示只返回 marc机内格式
                    List<string> parts = StringUtil.ParseTwoPart(strBiblioType, ":");
                    string strParam = parts[1];

                    nRet = GetMarc(strBiblioXml,
                        strParam,
                        out strBiblio,
                        out strError);
                    if (nRet == -1)
                    {
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }
                }
                // 模拟创建检索点
                else if (String.Compare(strBiblioType, "keys", true) == 0)
                {
                    nRet = GetKeys(sessioninfo,
                        strCurrentBiblioRecPath,
                        strBiblioXml,
                        out string strResultXml,
                        out strError);
                    if (nRet == -1)
                    {
                        // goto ERROR1;
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }
                    strBiblio = strResultXml;
                }
                // 2014/3/17
                else if (IsResultType(strBiblioType, "subcount") == true
                    || IsResultType(strBiblioType, "subrecords") == true)
                {
                    bool bSubCount = IsResultType(strBiblioType, "subcount");

                    string strType = "";
                    string strSubType = "";
                    StringUtil.ParseTwoPart(strBiblioType,
                        ":",
                        out strType,
                        out strSubType);

                    {
                        if (strSubType == null)
                            strSubType = "";

                        strSubType = strSubType.Replace("|", ",");
                    }

#if NO
                    RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        goto CONTINUE;
                    }
#endif
                    XmlDocument collection_dom = new XmlDocument();
                    collection_dom.LoadXml("<collection />");

                    long lTotalCount = 0;
                    if (StringUtil.IsInList("item", strSubType)
                        || string.IsNullOrEmpty(strSubType) == true
                        || StringUtil.IsInList("all", strSubType))
                    {
                        List<DeleteEntityInfo> entityinfos = null;
                        long lTemp = 0;

                        // 权限字符串
                        if (StringUtil.IsInList("getiteminfo,order", sessioninfo.RightsOrigin) == false)
                        {
                            lTemp = -1;
                        }
                        else
                        {
                            // 探测书目记录有没有下属的实体记录(也顺便看看实体记录里面是否有流通信息)?

                            string strLibraryCodeParam = "";
                            if (sessioninfo.GlobalUser == false)
                                strLibraryCodeParam = sessioninfo.LibraryCodeList;

                            if (StringUtil.IsInList("getotherlibraryitem", strSubType))
                                strLibraryCodeParam = "";

                            string strStyle = bSubCount ? "only_getcount" : "return_record_xml,limit:10";
                            if (string.IsNullOrEmpty(strLibraryCodeParam) == false)
                                strStyle += ",libraryCodes:" + strLibraryCodeParam.Replace(",", "|");

                            //                  当包含 libraryCodes: 时，表示仅获得所列分馆代码的册记录。注意多个馆代码之间用竖线分隔
                            // return:
                            //      -2  not exist entity dbname
                            //      -1  error
                            //      >=0 含有流通信息的实体记录个数, 当strStyle包含count_borrow_info时。
                            nRet = SearchChildEntities(
                                null,
                                channel,
                                strCurrentBiblioRecPath,
                                strStyle,
                                // bSubCount ? (Delegate_checkRecord)null : CountItemRecord,
                                (Delegate_checkRecord)null,
                                null,
                                out lTemp,
                                out entityinfos,
                                out strError);
                            if (nRet == -1)
                            {
                                // 如果 subrecords xml 有错，则会以 "error:" 开头，前端可以据此判断
                                strBiblio = "error:" + strError;
                                goto CONTINUE;
                            }

                            if (nRet == -2)
                            {
                                Debug.Assert(entityinfos.Count == 0, "");
                            }

                            lTotalCount += lTemp;
                        }

                        if (bSubCount == false)
                        {
                            AddSubXml(
                collection_dom,
                entityinfos,
                "item",
                lTemp);
                        }
                    }
                    if (StringUtil.IsInList("order", strSubType)
                        || string.IsNullOrEmpty(strSubType) == true
                        || StringUtil.IsInList("all", strSubType))
                    {
                        // 探测书目记录有没有下属的订购记录
                        List<DeleteEntityInfo> orderinfos = null;
                        long lTemp = 0;

                        // 权限字符串
                        if (StringUtil.IsInList("getorderinfo,order", sessioninfo.RightsOrigin) == false)
                        {
                            lTemp = -1;
                        }
                        else
                        {
                            // return:
                            //      -1  error
                            //      0   not exist entity dbname
                            //      1   exist entity dbname
                            nRet = this.OrderItemDatabase.SearchChildItems(
                                null,
                                channel,
                                strCurrentBiblioRecPath,
                                bSubCount ? "only_getcount" : "return_record_xml,limit:10",
                                (Delegate_checkRecord)null,
                                null,
                                out lTemp,
                                out orderinfos,
                                out strError);
                            if (nRet == -1)
                            {
                                strBiblio = strError;
                                goto CONTINUE;
                            }

                            if (nRet == 0)
                            {
                                Debug.Assert(orderinfos.Count == 0, "");
                            }
                            lTotalCount += lTemp;
                        }

                        if (bSubCount == false)
                        {
                            AddSubXml(
                collection_dom,
                orderinfos,
                "order",
                lTemp);
                        }
                    }
                    if (StringUtil.IsInList("issue", strSubType)
    || string.IsNullOrEmpty(strSubType) == true
    || StringUtil.IsInList("all", strSubType))
                    {
                        // 探测书目记录有没有下属的期记录
                        List<DeleteEntityInfo> issueinfos = null;
                        long lTemp = 0;

                        // 权限字符串
                        if (StringUtil.IsInList("getissueinfo,order", sessioninfo.RightsOrigin) == false)
                        {
                            lTemp = -1;
                        }
                        else
                        {
                            // return:
                            //      -1  error
                            //      0   not exist entity dbname
                            //      1   exist entity dbname
                            nRet = this.IssueItemDatabase.SearchChildItems(
                                null,
                                channel,
                                strCurrentBiblioRecPath,
                                bSubCount ? "only_getcount" : "return_record_xml,limit:10",
                                (Delegate_checkRecord)null,
                                null,
                                out lTemp,
                                out issueinfos,
                                out strError);
                            if (nRet == -1)
                            {
                                strBiblio = strError;
                                goto CONTINUE;
                            }

                            if (nRet == 0)
                            {
                                Debug.Assert(issueinfos.Count == 0, "");
                            }
                            lTotalCount += lTemp;
                        }

                        if (bSubCount == false)
                        {
                            AddSubXml(
                collection_dom,
                issueinfos,
                "issue",
                lTemp);
                        }
                    }
                    if (StringUtil.IsInList("comment", strSubType)
    || string.IsNullOrEmpty(strSubType) == true
    || StringUtil.IsInList("all", strSubType))
                    {
                        // 探测书目记录有没有下属的评注记录
                        List<DeleteEntityInfo> commentinfos = null;
                        long lTemp = 0;

                        // 权限字符串
                        if (StringUtil.IsInList("getcommentinfo,order", sessioninfo.RightsOrigin) == false)
                        {
                            lTemp = -1;
                        }
                        else
                        {
                            // return:
                            //      -1  error
                            //      0   not exist entity dbname
                            //      1   exist entity dbname
                            nRet = this.CommentItemDatabase.SearchChildItems(
                                null,
                                channel,
                                strCurrentBiblioRecPath,
                                bSubCount ? "only_getcount" : "return_record_xml,limit:10",
                                (Delegate_checkRecord)null,
                                null,
                                out lTemp,
                                out commentinfos,
                                out strError);
                            if (nRet == -1)
                            {
                                strBiblio = strError;
                                goto CONTINUE;
                            }

                            if (nRet == 0)
                            {
                                Debug.Assert(commentinfos.Count == 0, "");
                            }

                            lTotalCount += lTemp;
                        }

                        if (bSubCount == false)
                        {
                            AddSubXml(
                collection_dom,
                commentinfos,
                "comment",
                lTemp);
                        }
                    }

                    if (bSubCount)
                        strBiblio = lTotalCount.ToString();
                    else
                        strBiblio = collection_dom.DocumentElement.OuterXml;
                }
                else if (String.Compare(strBiblioType, "outputpath", true) == 0
                    || String.Compare(strBiblioType, "recpath", true) == 0)
                {
                    strBiblio = strOutputPath;  // 2008/3/18 
                }
                else if (String.Compare(strBiblioType, "timestamp", true) == 0)
                {
                    strBiblio = ByteArray.GetHexTimeStampString(timestamp);  // 2013/3/8
                }
                else if (String.Compare(strBiblioType, "metadata", true) == 0)
                {
                    strBiblio = strMetadata;  // 2010/10/27 
                }
                else if (IsResultType(strBiblioType, "table") == true
                    // String.Compare(strBiblioType, "table", true) == 0
                    )
                {
                    if (String.IsNullOrEmpty(strBiblioXml) == true)
                    {
                        strBiblio = "XML记录为空";
                        goto CONTINUE;
                    }

                    List<string> parts = StringUtil.ParseTwoPart(strBiblioType, ":");
                    string style = parts[1].Replace("|", ",");

                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        XmlElement maps_container = this.LibraryCfgDom.DocumentElement.SelectSingleNode("maps_856u") as XmlElement;
                        nRet = this.ConvertBiblioXmlToTable(
                            strBiblioXml,
                            null,
                            strCurrentBiblioRecPath,
                            style,
                            maps_container,
                            out strBiblio,
                            out strError);
                        if (nRet == -1)
                        {
                            /*
                            if (String.IsNullOrEmpty(strErrorText) == false)
                                strErrorText += ";\r\n";
                            strErrorText += strError;
                            */
                            AppendErrorText(strError);
                            goto CONTINUE;
                        }
                    }
                    else
                        strBiblio = "";
                }
                else if (String.Compare(strBiblioType, "html", true) == 0)
                {
                    if (String.IsNullOrEmpty(strBiblioXml) == true)
                    {
                        strBiblio = "XML记录为空";
                        goto CONTINUE;
                    }

                    // string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                    // 是否需要检查这个数据库名确实为书目库名？

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    nRet = this.MapKernelScriptFile(
                        sessioninfo,
                        strBiblioDbName,
                        "./cfgs/loan_biblio.fltx",
                        out strLocalPath,
                        out strError);
                    if (nRet == -1)
                    {
                        // goto ERROR1;
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }

                    // 将种记录数据从XML格式转换为HTML格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = this.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                            strBiblioXml,
                            null,
                            strCurrentBiblioRecPath,
                            out strBiblio,
                            out strError);
                        if (nRet == -1)
                        {
                            // goto ERROR1;
                            /*
                            if (String.IsNullOrEmpty(strErrorText) == false)
                                strErrorText += ";\r\n";
                            strErrorText += strError;
                            */
                            AppendErrorText(strError);
                            goto CONTINUE;
                        }
                    }
                    else
                        strBiblio = "";
                }
                else if (String.Compare(strBiblioType, "text", true) == 0)
                {
                    if (String.IsNullOrEmpty(strBiblioXml) == true)
                    {
                        strBiblio = "XML记录为空";
                        goto CONTINUE;
                    }

                    // string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                    // 是否需要检查这个数据库名确实为书目库名？

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    nRet = this.MapKernelScriptFile(
                        sessioninfo,
                        strBiblioDbName,
                        "./cfgs/loan_biblio_text.fltx",
                        out strLocalPath,
                        out strError);
                    if (nRet == -1)
                    {
                        //goto ERROR1;
                        /*
                        if (String.IsNullOrEmpty(strErrorText) == false)
                            strErrorText += ";\r\n";
                        strErrorText += strError;
                        */
                        AppendErrorText(strError);
                        goto CONTINUE;
                    }

                    // 将种记录数据从XML格式转换为text格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";

                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = this.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                            strBiblioXml,
                            null,
                            strCurrentBiblioRecPath,
                            out strBiblio,
                            out strError);
                        if (nRet == -1)
                        {
                            //goto ERROR1;
                            /*
                            if (String.IsNullOrEmpty(strErrorText) == false)
                                strErrorText += ";\r\n";
                            strErrorText += strError;
                            */
                            AppendErrorText(strError);
                            goto CONTINUE;
                        }
                    }
                    else
                        strBiblio = "";
                }
                else
                {
                    //strErrorText = "未知的书目格式 '" + strBiblioType + "'";
                    //return -1;
                    // 2023/3/24
                    // 尽量继续处理，不影响其它 format 的处理
                    strError = $"未知的书目格式 '{strBiblioType}'";
                    result_strings.Add($"!error:{strError}");
                    AppendErrorText(strError);
                    continue;
                }

            CONTINUE:
                result_strings.Add(strBiblio);
            } // end of for

            strError = strErrorText;
            if (string.IsNullOrEmpty(strErrorText) == false && formats.Length <= 1)
                return -1;

            if (string.IsNullOrEmpty(strError) == false)
                return 1;
            return 0;

            void AppendErrorText(string error)
            {
                if (String.IsNullOrEmpty(strErrorText) == false)
                    strErrorText += ";\r\n";
                strErrorText += error;
            }
        }

        // 2017/6/16
        static int GetMarc(string strXml,
            string strParam,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";
            int nRet = 0;

            string strMARC = "";
            string strMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML 转换到 MARC 记录时出错: " + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(strMarcSyntax))
                strMarcSyntax = "unimarc";

            if (strParam == "syntax")
                strResult = strMarcSyntax + "|" + strMARC;
            else
                strResult = strMARC;
            return 0;
        }


        static int GetIso2709(string strXml,
            Encoding targetEncoding,
            out byte[] result,
            out string strError)
        {
            strError = "";
            result = null;
            int nRet = 0;

            string strMARC = "";
            string strMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 将MARC机内格式转换为ISO2709格式
            // parameters:
            //      strSourceMARC   [in]机内格式MARC记录。
            //      strMarcSyntax   [in]为"unimarc"或"usmarc"
            //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
            //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
            // return:
            //      -1  出错
            //      0   成功
            nRet = MarcUtil.CvtJineiToISO2709(
                strMARC,
                strMarcSyntax,
                targetEncoding,
                "", // 2019/6/13
                out result,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        static void AddSubXml(
            XmlDocument dom,
            List<DeleteEntityInfo> entityinfos,
            string strItemElementName,
            long lHitCount)
        {
            if (lHitCount != -1)
            {
                foreach (DeleteEntityInfo entity in entityinfos)
                {
                    XmlElement item = dom.CreateElement(strItemElementName);
                    dom.DocumentElement.AppendChild(item);
                    item.SetAttribute("recPath", entity.RecPath);
                    item.SetAttribute("timestamp", ByteArray.GetHexTimeStampString(entity.OldTimestamp));

                    if (string.IsNullOrEmpty(entity.OldRecord) == false)
                    {
                        // TODO: 这里如果抛出异常怎么办?
                        try
                        {
                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(entity.OldRecord);
                            item.InnerXml = item_dom.DocumentElement.InnerXml;
                        }
                        catch (Exception ex)
                        {
                            // itemTotalCount=-1 表示 AccessDenied
                            dom.DocumentElement.SetAttribute(strItemElementName + "TotalCount", "-1");
                            // 加入错误原因属性，以便前端判断和处理
                            dom.DocumentElement.SetAttribute(strItemElementName + "ErrorInfo", $"册记录 {entity.RecPath} XML 解析出错: {ex.Message}");
                            dom.DocumentElement.SetAttribute(strItemElementName + "ErrorCode", "");
                            return;
                        }
                    }
                }
            }

            // itemTotalCount=-1 表示 AccessDenied
            dom.DocumentElement.SetAttribute(strItemElementName + "TotalCount", lHitCount.ToString());
            if (lHitCount == -1)
            {
                dom.DocumentElement.SetAttribute(strItemElementName + "ErrorInfo", $"权限不足，无法获取 {strItemElementName}记录");
                dom.DocumentElement.SetAttribute(strItemElementName + "ErrorCode", "AccessDenied");
            }
        }

#if NO
        // 不让超过 10 条
        int CountItemRecord(
    int index,
    string strRecPath,
    XmlDocument dom,
    byte[] baTimestamp,
    object param,
    out string strError)
        {
            strError = "";
            if (index >= 10)
                return 1;
            return 0;
        }
#endif

        public int GetKeys(SessionInfo sessioninfo,
            string strRecPath,
            string strXml,
            out string strResultXml,
            out string strError)
        {
            strError = "";
            strResultXml = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
            List<AccessKeyInfo> keys = null;
            long lRet = channel.DoGetKeys(strRecPath,
                strXml,
                string.IsNullOrEmpty(sessioninfo.Lang) == true ? "zh" : sessioninfo.Lang,
                null,
                out keys,
                out strError);
            if (lRet == -1)
                return -1;

            // 产生 XML 内容
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            foreach (AccessKeyInfo key in keys)
            {
                XmlNode node = dom.CreateElement("k");
                dom.DocumentElement.AppendChild(node);
                DomUtil.SetAttr(node, "k", key.Key);
                DomUtil.SetAttr(node, "f", key.FromName);
            }

            strResultXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        // 获得图书摘要信息
        // 调用时不需要SessionInfo
        public int GetBiblioSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            int nMaxLength,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError)
        {
            strError = "";
            strSummary = "";
            strBiblioRecPath = "";

            // 临时的SessionInfo对象
            SessionInfo sessioninfo = new SessionInfo(this);
            // 模拟一个账户
            Account account = new Account();
            account.LoginName = "内部调用";
            account.Password = "";
            account.Rights = "getbibliosummary";

            account.Type = "";
            account.Barcode = "";
            account.Name = "内部调用";
            account.UserID = "内部调用";
            account.RmsUserName = this.ManagerUserName;
            account.RmsPassword = this.ManagerPassword;

            sessioninfo.Account = account;
            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                LibraryServerResult result = this.GetBiblioSummary(
                    sessioninfo,
                    channel,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    strBiblioRecPathExclude,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }
                else
                {
                    if (nMaxLength != -1)
                    {
                        // 截断
                        if (strSummary.Length > nMaxLength)
                            strSummary = strSummary.Substring(0, nMaxLength) + "...";
                    }
                }
            }
            finally
            {
                sessioninfo.CloseSession();
                sessioninfo = null;
            }

            return 0;
        }

        // 2016/7/24
        public static string CutSummary(string strSummary, int nMaxLength)
        {
            if (string.IsNullOrEmpty(strSummary))
                return strSummary;

            if (nMaxLength != -1)
            {
                // 截断
                if (strSummary.Length > nMaxLength)
                    return strSummary.Substring(0, nMaxLength) + "...";
            }

            return strSummary;
        }

        // 根据册条码号获得它所从属的书目记录路径
        int GetBiblioRecPathByItemBarcode(SessionInfo sessioninfo,
            RmsChannel channel,
            string strItemBarcode,
            out string strBiblioRecPath,
            out string strError)
        {
            strBiblioRecPath = "";
            strError = "";

            string strItemXml = "";
            string strOutputItemPath = "";
            // 获得册记录
            // return:
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            int nRet = this.GetItemRecXml(
                channel,
                strItemBarcode,
                out strItemXml,
                out strOutputItemPath,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            // 从册记录中获得从属的种id
            string strBiblioRecID = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "册记录XML装载到DOM出错:" + ex.Message;
                return -1;
            }

            strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
            if (String.IsNullOrEmpty(strBiblioRecID) == true)
            {
                strError = "种下属记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                return -1;
            }

            string strItemDbName = ResPath.GetDbName(strOutputItemPath);
            string strBiblioDbName = "";

            // 根据书目下属库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = this.GetBiblioDbNameByChildDbName(strItemDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "下属库名 '" + strItemDbName + "' 没有找到所从属的书目库名";
                return -1;
            }

            strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
            return 1;
        }

        // 从册条码号(+册记录路径)获得种记录摘要，或者从订购记录路径、期记录路径、评注记录路径获得种记录摘要
        // 权限:   需要具有getbibliosummary权限
        // parameters:
        //      strConfirmItemRecPath       册、订购、期、评注记录路径
        //                                  如果 strConfirmItemRecPath 形态为 xxx|xxx，右边部分就是书目记录路径
        //      strBiblioRecPathExclude   除开列表中的这些种路径, 才返回摘要内容, 否则仅仅返回种路径即可
        //                                  如果包含 "coverimage"，表示要在 strSummary 头部包含封面图像的 <img ... /> 片段
        public LibraryServerResult GetBiblioSummary(
            SessionInfo sessioninfo,
            RmsChannel channel,
            string strItemBarcodeParam,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary)
        {
            strBiblioRecPath = "";
            strSummary = "";
            string strError = "";
            LibraryServerResult result = new LibraryServerResult();

            ParseOI(strItemBarcodeParam, out string strItemBarcode, out string strOwnerInstitution);

#if NO
            string strCacheKey = strItemBarcode + "|" + strConfirmItemRecPath + "|" + strBiblioRecPathExclude;
            if (this.BiblioSummaryCache != null)
            {
                strSummary = this.BiblioSummaryCache.Get(strCacheKey) as string;
                if (string.IsNullOrEmpty(strSummary) == false)
                {
                    // 从 cache 中命中
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        sessioninfo.LibraryCodeList,
                        "获取书目摘要",
                        "缓存命中次",
                        1);

                    result.Value = 1;
                    return result;
                }
            }
#endif

            if (sessioninfo != null)
            {
                // 权限判断
                // 权限字符串
                if (StringUtil.IsInList("getbibliosummary,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取种摘要信息被拒绝。不具备 order 或 getbibliosummary 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            int nRet = 0;
            long lRet = 0;

            if (string.IsNullOrEmpty(strItemBarcode) == true
                && string.IsNullOrEmpty(strConfirmItemRecPath) == true)
            {
                strError = "strItemBarcode 和 strConfirmItemRecPath 参数值不应同时为空";
                goto ERROR1;
            }

            // string strItemXml = "";
            string strOutputItemPath = "";
            string strMetaData = "";

            // 特殊情况，通过种路径
            string strHead = "@bibliorecpath:";
            if (strItemBarcode != null
                && strItemBarcode.Length > strHead.Length
                && strItemBarcode.Substring(0, strHead.Length).ToLower() == strHead)
            {
                strBiblioRecPath = strItemBarcode.Substring(strHead.Length);

                // 检查书目库名是否合法
                string strTempBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                if (this.IsBiblioDbName(strTempBiblioDbName) == false)
                {
                    strError = "strItemBarcode参数中析出的书目库路径 '" + strBiblioRecPath + "' 中，书目库名 '" + strTempBiblioDbName + "' 不是系统定义的书目库名";
                    goto ERROR1;
                }
                goto LOADBIBLIO;
            }

            // 如果 strConfirmItemRecPath 形态为 xxx|xxx，右边部分就是书目记录路径
            {
                StringUtil.ParseTwoPart(strConfirmItemRecPath, "|", out string strLeft, out string strRight);
                if (string.IsNullOrEmpty(strRight) == false)
                {
                    strBiblioRecPath = strRight;
                    goto LOADBIBLIO;
                }
                strConfirmItemRecPath = strLeft;
            }

            // bool bByRecPath = false;    // 是否经过记录路径来获取的？

            // 从册记录中获得从属的种id
            string strBiblioRecID = "";

            if (string.IsNullOrEmpty(strItemBarcode) == false)
            {
#if NO
                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    channel,
                    strItemBarcode,
                    out strItemXml,
                    out strOutputItemPath,
                    out strError);
#endif
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecParent(
                    channel,
                    strItemBarcode,
                    strOwnerInstitution,
                    out strBiblioRecID,
                    out strOutputItemPath,
                    out strError);

                // 2018/10/21
                // 如果需要，从登录号等辅助途径进行检索
                if (nRet == 0 && strItemBarcode.StartsWith("@") == false)
                {
                    foreach (string strFrom in this.ItemAdditionalFroms)
                    {
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetOneItemRec(
                            channel,
                            "item",
                            strItemBarcode,
                            strFrom,
                            "parent",
                            out strBiblioRecID,
                            100,
                            out List<string> PathList,
                            out byte[] item_timestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                            continue;
                        if (PathList != null && PathList.Count > 0)
                            strOutputItemPath = PathList[0];
                        if (nRet > 1)
                            break;

                        // strItemFrom = strFrom;
                        break;
                    }
                }

                if (nRet == 0)
                {

                    result.Value = 0;
                    result.ErrorInfo = "册记录没有找到";
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }

                if (nRet == -1)
                    goto ERROR1;
            }

            // 如果命中多于一条(或者没有条码号)，并且已经有确定的册记录路径辅助判断
            if (string.IsNullOrEmpty(strItemBarcode) == true
                ||
                (nRet > 1 && String.IsNullOrEmpty(strConfirmItemRecPath) == false))
            {
                // 检查路径中的库名，是不是实体库、订购库、期库、评注库名
                nRet = CheckRecPath(strConfirmItemRecPath,
                    "item,order,issue,comment",
                    out strError);
                if (nRet != 1)
                    goto ERROR1;

#if NO
                lRet = channel.GetRes(strConfirmItemRecPath,
                    out strItemXml,
                    out strMetaData,
                    out item_timestamp,
                    out strOutputItemPath,
                    out strError);
#endif
                lRet = channel.GetRes(strConfirmItemRecPath + "/xpath/@//parent",
    out string strValue,
    out strMetaData,
    out byte[] item_timestamp,
    out strOutputItemPath,
    out strError);
                if (lRet == -1)
                {
                    strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得记录失败: " + strError;
                    goto ERROR1;
                }
                if (string.IsNullOrEmpty(strValue) == true)
                {
                    strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得记录失败: " + "记录中没有返回有效的 parent 元素";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strValue);
                }
                catch (Exception ex)
                {
                    strError = "strValue 装载到DOM出错:" + ex.Message;
                    goto ERROR1;
                }
                strBiblioRecID = dom.DocumentElement.InnerText.Trim();
#if NO
                bByRecPath = true;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strItemXml);
                }
                catch (Exception ex)
                {
                    strError = "册记录XML装载到DOM出错:" + ex.Message;
                    goto ERROR1;
                }

                if (bByRecPath == true
                    && string.IsNullOrEmpty(strItemBarcode) == false)   // 2011/9/6
                {
                    // 这种情况需要核实册条码号
                    string strTempItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                        "//barcode");
                    if (strTempItemBarcode != strItemBarcode)
                    {
                        strError = "通过册条码号 '" + strItemBarcode + "' 获取实体记录发现命中多条，然后自动用记录路径 '" + strConfirmItemRecPath + "' 来获取实体记录，虽然获取成功，但是发现所获取的记录中<barcode>元素中的册条码号 '" + strTempItemBarcode + "' 不符合要求的册条码号 '" + strItemBarcode + "。(后面)这种情况可能是由于实体记录发生过移动造成的。";
                        goto ERROR1;
                    }
                }

                strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
                if (String.IsNullOrEmpty(strBiblioRecID) == true)
                {
                    strError = "种下属记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                    goto ERROR1;
                }
#endif
            }

            // 从配置文件中获得和实体库对应的书目库名

            /*
            // 准备工作: 映射数据库名
            nRet = this.GetGlobalCfg(sessioninfo.Channels,
                out strError);
            if (nRet == -1)
                goto ERROR1;
             * */

            string strItemDbName = ResPath.GetDbName(strOutputItemPath);
            string strBiblioDbName = "";

            // 根据书目下属库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = this.GetBiblioDbNameByChildDbName(strItemDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "下属库名 '" + strItemDbName + "' 没有找到所从属的书目库名";
                goto ERROR1;
            }

            string strBiblioXml = "";
            strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;

        LOADBIBLIO:

            // 看看是否在排除列表中
            if (String.IsNullOrEmpty(strBiblioRecPathExclude) == false
                && IsInBarcodeList(strBiblioRecPath,
                strBiblioRecPathExclude) == true)
            {
                result.Value = 1;
                return result;
            }

            SummaryItem summary = GetBiblioSummary(strBiblioRecPath);
            if (summary != null)
            {
                if (StringUtil.IsInList("coverimage", strBiblioRecPathExclude) == true
                    && string.IsNullOrEmpty(summary.ImageFragment) == false)
                    strSummary = summary.ImageFragment + summary.Summary;
                else
                    strSummary = summary.Summary;
                if (string.IsNullOrEmpty(strSummary) == false
                    && sessioninfo != null)
                {
                    // 从存储中命中
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        sessioninfo.LibraryCodeList,
                        "获取书目摘要",
                        "存储命中次",
                        1);

                    result.Value = 1;
                    return result;
                }
            }

            /*
strSummary = "";
result.Value = 1;
return result;
 * */

            // 获得本地配置文件

            string strRemotePath = BrowseFormat.CanonicalizeScriptFileName(
                ResPath.GetDbName(strBiblioRecPath),
                "./cfgs/summary.fltx");

            nRet = this.CfgsMap.MapFileToLocal(
                // sessioninfo.Channels,
                channel,
                strRemotePath,
                out string strLocalPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                // 配置.fltx文件不存在, 再试探.cs文件
                strRemotePath = BrowseFormat.CanonicalizeScriptFileName(
                ResPath.GetDbName(strBiblioRecPath),
                "./cfgs/summary.cs");

                nRet = this.CfgsMap.MapFileToLocal(
                    // sessioninfo.Channels,
                    channel,
                    strRemotePath,
                    out strLocalPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = strRemotePath + "不存在...";
                    goto ERROR1;
                }
            }

            bool bFltx = false;
            // 如果是一般.cs文件, 还需要获得.cs.ref配置文件
            if (IsCsFileName(strRemotePath) == true)
            {
                nRet = this.CfgsMap.MapFileToLocal(
                    // sessioninfo.Channels,
                    channel,
                    strRemotePath + ".ref",
                    out string strTempPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                bFltx = false;
            }
            else
            {
                bFltx = true;
            }

            // 取得种记录
            lRet = channel.GetRes(strBiblioRecPath,
                out strBiblioXml,
                out strMetaData,
                out byte[] timestamp,
                out strOutputItemPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                if (channel.IsNotFound())
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }
                goto ERROR1;
            }

            string strMarc = "";
            string strMarcSyntax = "";
            {
                // 转换为MARC格式

                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // fragment 总是要产生，只是最后是否返回给前端需要判断一下
            string strFragment = "";
            {
                // 获得封面图像 URL
                string strImageUrl = ScriptUtil.GetCoverImageUrl(strMarc, "SmallImage");
                if (string.IsNullOrEmpty(strImageUrl) == false)
                {
                    if (StringUtil.HasHead(strImageUrl, "uri:") == true)
                    {
                        strImageUrl = "object-path:" + strBiblioRecPath + "/object/" + strImageUrl.Substring(4);
                        strFragment = "<img class='biblio pending' name='" + strImageUrl + "'/>";
                    }
                    else
                    {
                        strFragment = "<img class='biblio' src='" + strImageUrl + "'/>";
                    }
                }
            }

            // 将种记录数据从XML格式转换为HTML格式
            if (string.IsNullOrEmpty(strBiblioXml) == false)
            {
                if (bFltx == true)
                {
                    string strFilterFileName = strLocalPath;
                    nRet = this.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                            strMarc,    // strBiblioXml,
                            strMarcSyntax,
                            strBiblioRecPath,
                            out strSummary,
                            out strError);
                }
                else
                {
                    nRet = this.ConvertRecordXmlToHtml(
                        strLocalPath,
                        strLocalPath + ".ref",
                        strBiblioXml,
                        strBiblioRecPath,   // 2009/10/18 
                        out strSummary,
                        out strError);
                }
                if (nRet == -1)
                    goto ERROR1;

            }
            else
                strSummary = "";

            this.SetBiblioSummary(strBiblioRecPath, strSummary, strFragment);

            if (StringUtil.IsInList("coverimage", strBiblioRecPathExclude) == true)
                strSummary = strFragment + strSummary;

#if NO
            if (this.BiblioSummaryCache != null)
            {
                DateTimeOffset offset = DateTimeOffset.Now.AddDays(1);
                this.BiblioSummaryCache.Set(strCacheKey, strSummary, offset);
            }
#endif

            if (this.Statis != null
                && sessioninfo != null)
                this.Statis.IncreaseEntryValue(
                sessioninfo.LibraryCodeList,
                "获取书目摘要",
                "构造次",
                1);

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 探测MARC格式
        // return:
        //      -1  error
        //      0   无法探测
        //      1   探测到了
        static int DetectMarcSyntax(XmlDocument dom,
            out string strMarcSyntax,
            out string strError)
        {
            strMarcSyntax = "";
            strError = "";

            // 取MARC根 和 取得marc syntax
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNode root_new = null;
            // '//'保证了无论MARC的根在何处，都可以正常取出。
            root_new = dom.DocumentElement.SelectSingleNode("//unimarc:record",
                nsmgr);
            if (root_new == null)
            {
                root_new = dom.DocumentElement.SelectSingleNode("//usmarc:record",
                    nsmgr);

                if (root_new == null)
                {
                    return 0;   // 无法探测到
                }

                strMarcSyntax = "usmarc";
            }
            else
            {
                strMarcSyntax = "unimarc";
            }

            Debug.Assert(strMarcSyntax != "", "");
            return 1;
        }

        // 获得书目记录的创建者
        // return:
        //      -1  出错
        //      0   没有找到 998$z子字段
        //      1   找到
        static int GetBiblioOwner(string strXml,
            out string strOwner,
            out string strError)
        {
            strError = "";
            strOwner = "";

            XmlDocument domNew = new XmlDocument();
            try
            {
                domNew.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入DOM时出错: " + ex.Message;
                return -1;
            }

            int nRet = DetectMarcSyntax(domNew,
    out string strMarcSyntax,
    out strError);
            if (nRet == -1)
                return -1;

            // 取MARC根 和 取得marc syntax
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            string strXPath = "";

            if (strMarcSyntax == "unimarc")
                strXPath = "//unimarc:record/unimarc:datafield[@tag='998']/unimarc:subfield[@code='z']";
            else
                strXPath = "//usmarc:record/usmarc:datafield[@tag='998']/usmarc:subfield[@code='z']";

            XmlNode node = domNew.DocumentElement.SelectSingleNode(strXPath, nsmgr);

            if (node == null)
                return 0;   // 没有找到

            strOwner = node.InnerText.Trim();
            return 1;
        }

        // 合并联合编目的新旧书目库XML记录
        // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
        // parameters:
        //      strChangeableFieldNameList  用于限定修改范围的，可修改字段名列表。例如 "###,100-200"
        //                          如果为 null，表示不使用此参数，意思是不限制任何字段的修改
        //                          如果为 ""，则表示空集合，意思是限制所有字段的修改。那实际上此时调用本函数不会对 strNewMarc 发生任何改动
        //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
        // return:
        //      -1  error
        //      0   new record not changed
        //      1   new record changed
        int MergeOldNewBiblioRec(
            string strRights,
            string strUnionCatalogStyle,
            string strLibraryCode,
            string strDefaultOperation,
            string strFieldNameList,
            string strChangeableFieldNameList, // 2023/2/11
            string strOldBiblioXml,
            ref string strNewBiblioXml,
            ref bool bChangePartDeniedParam,
            out string strError)
        {
            strError = "";
            string strNewSave = strNewBiblioXml;

            string strComment = "";
            bool bChangePartDenied = false;

            try
            {
                if (string.IsNullOrEmpty(strFieldNameList) == false
                    // || !(StringUtil.IsInList("writeres", strRights) == true || StringUtil.IsInList("setobject", strRights) == true)
                    || StringUtil.IsInList("setobject,setbiblioobject", strRights) == false
                    )
                {
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   有部分修改要求被拒绝。strError 中返回了注释信息
                    int nRet = MergeOldNewBiblioByFieldNameList(
                        strRights,
                        strDefaultOperation,
                        strFieldNameList,
                        strChangeableFieldNameList,
                        strOldBiblioXml,
                        ref strNewBiblioXml,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        bChangePartDenied = true;
                        strComment = strError;
                    }

                    strError = "";
                }

                // 2016/12/14
                if (/*strDefaultOperation != "delete" // 2017/5/5
                    &&*/ String.IsNullOrEmpty(strNewBiblioXml) == false)
                {
                    int nRet = CreateUniformKey(
                        strDefaultOperation == "delete",
ref strNewBiblioXml,
out strError);
                    if (nRet == -1)
                        return -1;
                }

                XmlDocument domNew = new XmlDocument();
                if (String.IsNullOrEmpty(strNewBiblioXml) == true)
                    strNewBiblioXml = "<root />";
                try
                {
                    domNew.LoadXml(strNewBiblioXml);
                }
                catch (Exception ex)
                {
                    strError = "strNewBiblioXml装入XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                // string strNewSave = domNew.OuterXml;

                XmlDocument domOld = new XmlDocument();
                if (String.IsNullOrEmpty(strOldBiblioXml) == true
                    || (string.IsNullOrEmpty(strOldBiblioXml) == false && strOldBiblioXml.Length == 1))
                    strOldBiblioXml = "<root />";
                try
                {
                    domOld.LoadXml(strOldBiblioXml);
                }
                catch (Exception ex)
                {
                    strError = "strOldBiblioXml装入XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                // 确保<operations>元素被服务器彻底控制
                {
                    // 删除new中的全部<operations>元素，然后将old记录中的全部<operations>元素插入到new记录中

                    // 删除new中的全部<operations>元素
                    XmlNodeList nodes = domNew.DocumentElement.SelectNodes("operations");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        if (node.ParentNode != null)
                            node.ParentNode.RemoveChild(node);
                    }

                    // 然后将old记录中的全部<operations>元素插入到new记录中
                    nodes = domOld.DocumentElement.SelectNodes("operations");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];

                        XmlDocumentFragment fragment = domNew.CreateDocumentFragment();
                        fragment.InnerXml = node.OuterXml;

                        domNew.DocumentElement.AppendChild(fragment);
                    }
                }

                // 如果不具备 setbiblioobject 和 setobject 权限，则要屏蔽前端发来的 XML 记录中的 dprms:file 元素
                if (StringUtil.IsInList("setbiblioobject,setobject", strRights) == false)
                {
                    string strRequstFragments = GetAllFileElements(domNew);

                    // 2023/2/1
                    // TODO: 用 MergeDprmsFile() 函数替代下面段落 
                    MergeDprmsFile(ref domNew, domOld);
#if OLDCODE
                    // 删除new中的全部<dprms:file>元素，然后将old记录中的全部<dprms:file>元素插入到new记录中
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                    nsmgr.AddNamespace("dprms", DpNs.dprms);

                    // 删除new中的全部<dprms:file>元素
                    XmlNodeList nodes = domNew.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        if (node.ParentNode != null)
                            node.ParentNode.RemoveChild(node);
                    }

                    // 然后将old记录中的全部<dprms:file>元素插入到new记录中
                    nodes = domOld.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];

                        XmlDocumentFragment fragment = domNew.CreateDocumentFragment();
                        fragment.InnerXml = node.OuterXml;

                        domNew.DocumentElement.AppendChild(fragment);
                    }
#endif

                    // 2017/6/2
                    string strAcceptedFragments = GetAllFileElements(domNew);
                    if (strRequstFragments != strAcceptedFragments)
                    {
                        // 如果前端提交的关于 dprms:file 元素的修改被拒绝，则要通过设置 bChangePartDenied = true 来反映这种情况
                        bChangePartDenied = true;
                        if (string.IsNullOrEmpty(strComment) == false)
                            strComment += "; ";
                        if (string.IsNullOrEmpty(strAcceptedFragments) && string.IsNullOrEmpty(strRequstFragments) == false)
                            strComment += "因不具备 setobject 权限, 创建 dprms:file (数字对象)元素被拒绝";
                        else
                            strComment += "因不具备 setobject 权限, 修改 dprms:file (数字对象)元素被拒绝";
                    }
                }
                else
                {
                    // 此时 StringUtil.IsInList("setbiblioobject,setobject", strRights) == true
                    // 意味着直接采纳前端发来的 XML 记录中的 dprms:file 元素，写入记录
                    // 但需要注意检查账户权限，读的字段范围是否小于写的字段范围？如果小了，则读和写往返一轮会丢失记录中原有的 dprms:file 元素。这种情况需要直接报错
                    if (StringUtil.IsInList("getbiblioobject,getobject", strRights) == false)
                    {
                        strError = "操作被放弃。当前用户的权限定义不正确：具有 setbiblioobject(或 setobject) 但不具有 getbiblioobject(或getobject) 权限(即写范围大于读范围)，这样会造成数据库内书目记录中原有的 dprms:file 元素丢失。请修改当前账户权限再重新操作";
                        return -1;
                    }

                    // TODO: 是否仅当 domOld 里面确实存在 dprms:file 元素的时候才这样报错？
                    // 不过这样有可能会让账户权限账户定义不正确的情况长期隐藏(并在比较尴尬的时候暴露出来)，不利于系统维护
                }

                if (StringUtil.IsInList("905", strUnionCatalogStyle) == true)
                {
                    // *号表示有权力处理全部馆代码的905
                    if (strLibraryCode == "*")
                    {
                        strNewBiblioXml = domNew.OuterXml;

                        if (strNewSave == strNewBiblioXml)
                            return 0;

                        return 1;
                    }

                    string strMarcSyntax = "";

                    // 取MARC根 和 取得marc syntax
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                    nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
                    nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

                    XmlNode root_new = null;
                    if (strMarcSyntax == "")
                    {
                        // '//'保证了无论MARC的根在何处，都可以正常取出。
                        root_new = domNew.DocumentElement.SelectSingleNode("//unimarc:record",
                            nsmgr);
                        if (root_new == null)
                        {
                            root_new = domNew.DocumentElement.SelectSingleNode("//usmarc:record",
                                nsmgr);

                            if (root_new == null)
                            {
                                root_new = domNew.DocumentElement;

                                int nRet = DetectMarcSyntax(domOld,
                                    out strMarcSyntax,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (nRet == 0)
                                {
                                    strError = "新旧MARC记录的syntax均无法探测到，因此无法进行处理";
                                    return -1;
                                }
                            }
                            else
                                strMarcSyntax = "usmarc";
                        }
                        else
                        {
                            strMarcSyntax = "unimarc";
                        }
                    }
                    else
                    {
                        Debug.Assert(false, "暂时走不到这里");
                        root_new = domNew.DocumentElement.SelectSingleNode("//" + strMarcSyntax + ":record",
                            nsmgr);
                        if (root_new == null)
                        {
                            return 0;
                        }
                    }

                    // 在新记录中删除指定馆代码以外的全部905字段；而符合指定馆代码的905字段保留
                    XmlNodeList nodes_new = domNew.DocumentElement.SelectNodes("//" + strMarcSyntax + ":datafield[@tag='905']",
                        nsmgr);

                    List<XmlNode> deleting = new List<XmlNode>();

                    for (int i = 0; i < nodes_new.Count; i++)
                    {
                        XmlNode field = nodes_new[i];

                        XmlNode subfield_a = field.SelectSingleNode(strMarcSyntax + ":subfield[@code='a']",
                            nsmgr);
                        string strValue = "";
                        if (subfield_a != null)
                            strValue = subfield_a.InnerText;

                        // 找出那些905$a不符合馆代码的
                        if (strValue != strLibraryCode)
                            deleting.Add(field);
                    }
                    for (int i = 0; i < deleting.Count; i++)
                    {
                        XmlNode temp = deleting[i];
                        if (temp.ParentNode != null)
                        {
                            temp.ParentNode.RemoveChild(temp);
                        }
                    }

                    // 然后在新记录中插入旧记录中，指定馆代码以外的全部905字段
                    XmlNodeList nodes_old = null;
                    nodes_old = domOld.DocumentElement.SelectNodes("//" + strMarcSyntax + ":datafield[@tag='905']",
                        nsmgr);

                    if (nodes_old.Count > 0)
                    {
                        // 找到插入点 -- 第一个905字段
                        XmlNode insert_pos = domNew.SelectSingleNode("//" + strMarcSyntax + ":datafield[@tag='905']",
                            nsmgr);

                        for (int i = 0; i < nodes_old.Count; i++)
                        {
                            XmlNode field = nodes_old[i];

                            XmlNode subfield_a = field.SelectSingleNode(strMarcSyntax + ":subfield[@code='a']",
                                nsmgr);
                            string strValue = "";
                            if (subfield_a != null)
                                strValue = subfield_a.InnerText;

                            // 符合指定馆代码的，跳过，不插入
                            if (strValue == strLibraryCode)
                                continue;

                            // 插入到旧记录末尾
                            XmlDocumentFragment fragment = domNew.CreateDocumentFragment();
                            fragment.InnerXml = field.OuterXml;

                            if (insert_pos != null)
                                root_new.InsertBefore(fragment, insert_pos);
                            else
                                root_new.AppendChild(fragment);
                        }
                    }
                }

                strNewBiblioXml = domNew.OuterXml;

                if (strNewSave == strNewBiblioXml)
                    return 0;

                return 1;
            }
            finally
            {
                if (bChangePartDenied == true && string.IsNullOrEmpty(strComment) == false)
                    strError += strComment;

                if (bChangePartDenied == true)
                    bChangePartDeniedParam = true;
            }
        }

        // 获得全部 dprms:file 元素的字符串拼接。返回前先排序。这样不同元素顺序的表示法被当作等同的
        public static string GetAllFileElements(XmlDocument domNew)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = domNew.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            List<string> results = new List<string>();
            foreach (XmlElement node in nodes)
            {
                results.Add(node.OuterXml.Trim());
            }

            results.Sort();
            return StringUtil.MakePathList(results, "\r\n");
        }

        // 获得交叉的馆代码的第一个
        static string Cross(string strLibraryCodeList1, string strLibraryCodeList2)
        {
            if (string.IsNullOrEmpty(strLibraryCodeList1) == true
                && string.IsNullOrEmpty(strLibraryCodeList2) == true)
                return "";

            if (string.IsNullOrEmpty(strLibraryCodeList1) == true
                && string.IsNullOrEmpty(strLibraryCodeList2) == false)
                return null;

            if (string.IsNullOrEmpty(strLibraryCodeList1) == false
                && string.IsNullOrEmpty(strLibraryCodeList2) == true)
                return null;

            string[] parts1 = strLibraryCodeList1.Split(new char[] { ',' });
            string[] parts2 = strLibraryCodeList2.Split(new char[] { ',' });

            foreach (string s1 in parts1)
            {
                string code1 = s1.Trim();
                if (string.IsNullOrEmpty(code1) == true)
                    continue;
                foreach (string s2 in parts2)
                {
                    string code2 = s2.Trim();
                    if (string.IsNullOrEmpty(code2) == true)
                        continue;
                    if (code1 == code2)
                        return code1;
                }
            }

            return null;
        }

        // 检测strSubList里面的馆代码是否完全包含于strList中
        static bool FullyContainIn(string strSubList, string strList)
        {
            if (string.IsNullOrEmpty(strSubList) == true
                && string.IsNullOrEmpty(strList) == true)
                return true;

            if (string.IsNullOrEmpty(strSubList) == true
    && string.IsNullOrEmpty(strList) == false)
                return false;

            string[] subs = strSubList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] parts = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string sub in subs)
            {
                foreach (string part in parts)
                {
                    if (sub == part)
                        goto FOUND;
                }
                return false;
            FOUND:
                continue;
            }

            return true;    // return false BUG
        }

        // 根据字段权限定义过滤出允许的内容
        // parameters:
        //      strUserRights   用户权限。如果为 null，表示不启用过滤 856 字段功能
        //      strFieldNameList    字段过滤列表。如果为空，则表示不(利用它)对字段进行过滤
        // return:
        //      -1  出错
        //      0   成功
        //      1   有部分字段被修改或滤除
        public static int FilterBiblioByFieldNameList(
            string strUserRights,
            string strFieldNameList,
            ref string strBiblioXml,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strBiblioXml) == true)
                return 0;

            string strMarcSyntax = "";
            string strMarc = "";

            if (string.IsNullOrEmpty(strBiblioXml) == false)
            {
                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	== true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (string.IsNullOrEmpty(strMarcSyntax) == true)
                return 0;   // 不是 MARC 格式

            bool bChanged = false;

            if (strUserRights != null)
            {
                // 对 MARC 记录进行过滤，将那些当前用户无法读取的 856 字段删除
                // return:
                //      -1  出错
                //      其他  滤除的 856 字段个数
                nRet = RemoveCantGet856(
                strUserRights,
                ref strMarc,
                out strError);
                if (nRet == -1)
                    return -1;
                if (nRet > 0)
                    bChanged = true;
            }

            if (string.IsNullOrEmpty(strFieldNameList) == false)
            {
                // 根据字段权限定义过滤出允许的内容
                // return:
                //      -1  出错
                //      0   成功
                //      1   有部分字段被修改或滤除
                nRet = MarcDiff.FilterFields(strFieldNameList,
                    ref strMarc,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    bChanged = true;
            }

            if (bChanged == true)
            {
                nRet = MarcUtil.Marc2XmlEx(strMarc,
                    strMarcSyntax,
                    ref strBiblioXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 1;
            }

            return 0;
        }

        // 删除记录里面的 997 字段
        public static int RemoveUniformKey(
ref string strBiblioXml,
out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strBiblioXml) == true)
                return 0;

            string strMarcSyntax = "";
            string strMarc = "";

            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	== true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                "", // this.CurMarcSyntax,
                out strMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strMarcSyntax) == true)
                return 0;   // 不是 MARC 格式

            MarcRecord record = new MarcRecord(strMarc);
            record.select("field[@name='997']").detach();
            strMarc = record.Text;

            nRet = MarcUtil.Marc2XmlEx(strMarc,
                strMarcSyntax,
                ref strBiblioXml,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // parameters:
        //      strAction   如果为 true，表示要删除 MARC 记录中的 997 字段
        //                  如果为 false，表示正常创建 997 字段
        // return:
        //      -1  出错
        //      0   strBiblioXml 没有发生修改
        //      1   strBiblioXml 发生了修改
        public static int CreateUniformKey(
            bool delete,
    ref string strBiblioXml,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strBiblioXml) == true)
                return 0;

            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	== true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                "", // this.CurMarcSyntax,
                out string strMarcSyntax,
                out string strMarc,
                out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strMarcSyntax) == true)
                return 0;   // 不是 MARC 格式

            bool changed = false;
            // 2023/2/21
            if (delete)
            {
                MarcRecord record = new MarcRecord(strMarc);
                record.select("field[@name='997']").detach();
                if (strMarc != record.Text)
                {
                    strMarc = record.Text;
                    changed = true;
                }
            }
            else
            {
                nRet = CreateUniformKey(ref strMarc,
                    strMarcSyntax,
                    out _,
                    out _,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    changed = true;
            }

            nRet = MarcUtil.Marc2XmlEx(strMarc,
                strMarcSyntax,
                ref strBiblioXml,
                out strError);
            if (nRet == -1)
                return -1;

            if (changed)
                return 1;
            return 0;
        }

        // 997 内查重键的构造算法版本
        // 0.04 (2020/8/19) 增加了版本项和 998$k 子字段
        static string key_version = "0.04";

        // TODO: 根据多个 ISBN 创建多个 997 字段。查重算法也要改造，变成根据多个 key 分别检索
        // 创建查重键字段
        // 要创建的字段名和 MARC 格式无关，都是 997 字段。但要提取的书名等信息在什么字段，和具体的 MARC 格式有关
        // return:
        //      -1  出错
        //      0   strMARC 没有发生修改
        //      1   strMARC 发生了修改
        public static int CreateUniformKey(ref string strMARC,
            string strMarcSyntax,
            out string strKey,
            out string strCode,
            out string strError)
        {
            strError = "";
            strKey = "";
            strCode = "";

            bool changed = false;
            MarcRecord record = new MarcRecord(strMARC);
            List<string> segments = new List<string>();
            if (strMarcSyntax == "unimarc")
            {
                // isbn
                {
                    List<string> isbns = record.select("field[@name='010']/subfield[@name='a']").Contents;

                    // 统一变换为 13 位形态
                    for (int i = 0; i < isbns.Count; i++)
                    {
                        isbns[i] = IsbnSplitter.GetISBnBarcode(isbns[i]);
                    }
                    Sort(isbns);
                    segments.Add(StringUtil.MakePathList(isbns));
                }

                // title
                {
                    List<string> titles = record.select("field[@name='200']/subfield[@name='a' or @name='e']").Contents;

                    StringUtil.CanonializeWideChars(titles);
                    Sort(titles);

                    List<string> his = record.select("field[@name='200']/subfield[@name='h' or @name='i']").Contents;

                    if (his.Count > 0)
                    {
                        // $a 里面的数字和标点符号要归一化
                        // h 和 i 里面的数字等要归一化
                        // h 和 i 要根据内容排序
                        StringUtil.CanonializeWideChars(his);
                        Sort(his);
                        titles.AddRange(his);
                    }

                    segments.Add(StringUtil.MakePathList(titles));
                }

                // author
                {
                    List<string> authors = record.select("field[@name='701' or @name='711']/subfield[@name='a']").Contents;

                    StringUtil.CanonializeWideChars(authors);
                    // 要按照内容排序
                    Sort(authors);
                    segments.Add(StringUtil.MakePathList(authors));
                }

                // publisher
                {
                    // 210 $c $d
                    List<string> publishers = record.select("field[@name='210']/subfield[@name='c']").Contents;
                    StringUtil.CanonializeWideChars(publishers);
                    Sort(publishers);

                    List<string> dates = record.select("field[@name='210']/subfield[@name='d']").Contents;
                    // 日期需要归一化为 4 chars 形态
                    StringUtil.CanonializeWideChars(dates);
                    CanonializeDate(dates);
                    Sort(dates);
                    segments.Add(StringUtil.MakePathList(publishers) + "," + StringUtil.MakePathList(dates));

                }

                // 2020/8/19
                // 版本
                {
                    List<string> temp_keys = record.select("field[@name='205']/subfield[@name='a']").Contents;
                    StringUtil.CanonializeWideChars(temp_keys);
                    // CanonializeVersion(temp_keys);
                    Sort(temp_keys);
                    segments.Add(StringUtil.MakePathList(temp_keys));
                }

                // 2020/8/19
                // 临时区分
                {
                    List<string> temp_keys = record.select("field[@name='998']/subfield[@name='k']").Contents;
                    if (temp_keys.Count > 0)
                    {
                        Sort(temp_keys);
                        segments.Add(StringUtil.MakePathList(temp_keys));
                    }
                }
#if NO
                // pages
                // size
                {
                    List<string> pages = record.select("field[@name='215']/subfield[@name='a']").Contents;
                    // 归一化为纯数字
                    CanonializeWideChars(pages);
                    CanonializeNumber(pages);
                    Sort(pages);

                    List<string> sizes = record.select("field[@name='215']/subfield[@name='d']").Contents;

                    // 归一化为纯粹厘米数字
                    CanonializeWideChars(sizes);
                    CanonializeNumber(sizes);
                    Sort(sizes);
                    segments.Add(StringUtil.MakePathList(pages) + "," + StringUtil.MakePathList(sizes));
                }
#endif

                strKey = StringUtil.MakePathList(segments, "|");
                strCode = StringUtil.GetMd5(strKey);

                record.setFirstField("997", "  ", MarcQuery.SUBFLD + "a" + strKey + MarcQuery.SUBFLD + "h" + strCode + MarcQuery.SUBFLD + "v" + key_version);

                if (strMARC != record.Text)
                {
                    strMARC = record.Text;
                    changed = true;
                }
            }

            if (strMarcSyntax == "usmarc")
            {
                // isbn
                {
                    List<string> isbns = record.select("field[@name='020']/subfield[@name='a']").Contents;

                    // 统一变换为 13 位形态
                    for (int i = 0; i < isbns.Count; i++)
                    {
                        // 去掉空格以后的部分
                        string text = isbns[i];
                        int nRet = text.IndexOf(" ");
                        if (nRet != -1)
                            text = text.Substring(0, nRet).Trim();
                        isbns[i] = IsbnSplitter.GetISBnBarcode(text);
                    }
                    // TODO: 去掉重复?
                    Sort(isbns);
                    segments.Add(StringUtil.MakePathList(isbns));
                }

                // title
                {
                    List<string> titles = record.select("field[@name='245']/subfield[@name='a'  or @name='b']").Contents;

                    // TODO: 是否要忽略大小写?
                    TrimEndChar(titles);
                    Sort(titles);

                    List<string> his = record.select("field[@name='245']/subfield[@name='n']").Contents;

                    if (his.Count > 0)
                    {
                        // $a 里面的数字和标点符号要归一化
                        // h 和 i 里面的数字等要归一化
                        // h 和 i 要根据内容排序
                        TrimEndChar(his);
                        Sort(his);
                        titles.AddRange(his);
                    }

                    segments.Add(StringUtil.MakePathList(titles));
                }

                // author
                {
                    List<string> authors = record.select("field[@name='100' or @name='700']/subfield[@name='a']").Contents;

                    TrimEndChar(authors);
                    // 要按照内容排序
                    Sort(authors);
                    segments.Add(StringUtil.MakePathList(authors));
                }

                // publisher
                {
                    // 260 $b
                    List<string> publishers = record.select("field[@name='260']/subfield[@name='b']").Contents;
                    TrimEndChar(publishers);
                    Sort(publishers);

                    List<string> dates = record.select("field[@name='260']/subfield[@name='c']").Contents;
                    // 日期需要归一化为 4 chars 形态
                    TrimEndChar(dates);
                    CanonializeDate(dates);
                    Sort(dates);
                    segments.Add(StringUtil.MakePathList(publishers) + "," + StringUtil.MakePathList(dates));
                }

                // 2020/8/19
                // 版本
                {
                    List<string> temp_keys = record.select("field[@name='250']/subfield[@name='a']").Contents;
                    TrimEndChar(temp_keys);
                    // CanonializeVersion(temp_keys);
                    Sort(temp_keys);
                    segments.Add(StringUtil.MakePathList(temp_keys));
                }

                // 2020/8/19
                // 临时区分
                {
                    List<string> temp_keys = record.select("field[@name='998']/subfield[@name='k']").Contents;
                    if (temp_keys.Count > 0)
                    {
                        Sort(temp_keys);
                        segments.Add(StringUtil.MakePathList(temp_keys));
                    }
                }
#if NO
                // pages
                // size
                {
                    List<string> pages = record.select("field[@name='300']/subfield[@name='a']").Contents;
                    // 归一化为纯数字
                    TrimEndChar(pages);
                    CanonializeNumber(pages);
                    Sort(pages);

                    List<string> sizes = record.select("field[@name='300']/subfield[@name='c']").Contents;

                    // 归一化为纯粹厘米数字
                    TrimEndChar(sizes);
                    CanonializeNumber(sizes);
                    Sort(sizes);
                    segments.Add(StringUtil.MakePathList(pages) + "," + StringUtil.MakePathList(sizes));
                }
#endif

                strKey = StringUtil.MakePathList(segments, "|");
                strCode = StringUtil.GetMd5(strKey);

                record.setFirstField("997", "  ", MarcQuery.SUBFLD + "a" + strKey + MarcQuery.SUBFLD + "h" + strCode + MarcQuery.SUBFLD + "v" + key_version);

                if (strMARC != record.Text)
                {
                    strMARC = record.Text;
                    changed = true;
                }
            }

            if (changed)
                return 1;
            return 0;
        }

        public static string TrimEndChar(string strText, string strDelimeters = "./,;:")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            char tail = strText[strText.Length - 1];
            if (strDelimeters.IndexOf(tail) != -1)
                return strText.Substring(0, strText.Length - 1);
            return strText;
        }

        static void CanonializeDate(List<string> dates)
        {
            // 2016/1/1
            // 因为有 [2008] 这样的情况，所以要先处理为纯数字
            CanonializeNumber(dates);

            for (int i = 0; i < dates.Count; i++)
            {
                string date = dates[i];
                if (date.Length > 4)
                {
                    date = date.Substring(0, 4);
                    dates[i] = date;
                }
            }
        }

        static void CanonializeNumber(List<string> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                string new_value = GetNumber(value);
                if (value != new_value)
                {
                    values[i] = new_value;
                }
            }
        }

        static void TrimEndChar(List<string> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                string new_value = TrimEndChar(value);
                if (value != new_value)
                {
                    values[i] = new_value;
                }
            }
        }

        // 获得一个字符串里面的纯数字部分
        static string GetNumber(string strText)
        {
            string strHead = "";
            string strNumber = "";
            string strEnd = "";
            // 把一个被字符引导的字符串分成三部分
            StringUtil.SplitLedNumber(strText,
            out strHead,
            out strNumber,
            out strEnd);
            return strNumber;
        }

        static void Sort(List<string> titles)
        {
            StringUtil.RemoveBlank(ref titles);
            titles.Sort();
        }

        // 根据允许的字段名列表，合并新旧两条书目记录
        // 列表中不允许的字段，沿用旧记录中的原始字段内容
        // 算法等于在 new 中复原了那些不让修改的 MARC 字段
        // 2015/7/17 本函数还能保护 856 字段，避免用户修改自己不能获取的 856 rights 的字段。UNIMARC 和 USMARC 都是用 856 表示数字资源
        // TODO: 特定的数据库应该规定了 MARC 格式，可以作为本函数输入参数。这样可以避免复杂的判断
        // parameters:
        //      strChangeableFieldNameList  用于限定修改范围的，可修改字段名列表。例如 "###,100-200"
        //                          如果为 null，表示不使用此参数，意思是不限制任何字段的修改
        //                          如果为 ""，则表示空集合，意思是限制所有字段的修改。那实际上此时调用本函数不会对 strNewMarc 发生任何改动
        //      strFieldNameList    字段过滤列表。如果为空，则表示不(利用它)对字段进行过滤，即全用新记录的字段
        // return:
        //      -1  出错
        //      0   成功
        //      1   有部分修改要求被拒绝。strError 中返回了注释信息
        static int MergeOldNewBiblioByFieldNameList(
            string strUserRights,
            string strDefaultOperation,
            string strFieldNameList,
            string strChangeableFieldNameList, // 2023/2/11
            string strOldBiblioXml,
            ref string strNewBiblioXml,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strOldBiblioXml) == true
                && string.IsNullOrEmpty(strNewBiblioXml) == true)
                return 0;

            string strOldMarcSyntax = "";
            string strOldMarc = "";

            if (string.IsNullOrEmpty(strOldBiblioXml) == false)
            {
                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strOldBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strOldMarcSyntax,
                    out strOldMarc,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strNewMarcSyntax = "";
            string strNewMarc = "";

            if (string.IsNullOrEmpty(strNewBiblioXml) == false)
            {
                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strNewBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strNewMarcSyntax,
                    out strNewMarc,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (string.IsNullOrEmpty(strOldMarcSyntax) == true
                && string.IsNullOrEmpty(strNewMarcSyntax) == true)
                return 0;   // 不是 MARC 格式

            string strMarcSyntax = "";
            if (string.IsNullOrEmpty(strOldMarcSyntax) == false)
                strMarcSyntax = strOldMarcSyntax;
            else if (string.IsNullOrEmpty(strNewMarcSyntax) == false)
                strMarcSyntax = strNewMarcSyntax;
            else
            {
                strError = "MergeOldNewBiblioByFieldNameList() 出错： 新旧两个XML中均无 MARC 格式信息";
                return -1;
            }

            // 检查两个 MARC 格式是否一致
            if (string.IsNullOrEmpty(strOldMarcSyntax) == false && string.IsNullOrEmpty(strNewMarcSyntax) == false)
            {
                if (strOldMarcSyntax != strNewMarcSyntax)
                {
                    strError = "旧记录的 MARC 格式 '" + strOldMarcSyntax + "' 不等于新记录的 MARC 格式 '" + strNewMarcSyntax + "'";
                    return -1;
                }
            }

#if NO
            // strNewMarc 中，要把那些当前账户无法修改的字段先加回去，然后再和 strOldMarc 进行合并
            {
                // 按照字段修改权限定义，合并新旧两个 MARC 记录
                // return:
                //      -1  出错
                //      0   成功
                //      1   有部分修改要求被拒绝
                nRet = MarcDiff.MergeOldNew(
                    strDefaultOperation,
                    strProtectFieldNameList,
                    strNewMarc,
                    ref strOldMarc,
                    out _,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif

            bool b856Masked = false;

            int field_856_count = Get856Count(strNewMarc);

            // 对 strNewMarc 进行过滤，将那些当前用户无法读取的 856 字段删除
            // 对 MARC 记录进行过滤，将那些当前用户无法读取的 856 字段删除
            // return:
            //      -1  出错
            //      其他  滤除的 856 字段个数
            nRet = MaskCantGet856(
                strUserRights,
                true,
                ref strNewMarc,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet > 0)
                b856Masked = true;

            nRet = MaskCantGet856(
    strUserRights,
    true,
    ref strOldMarc,
    out strError);
            if (nRet == -1)
                return -1;
            if (nRet > 0)
                b856Masked = true;

            // 2023/2/11
            // 给 997 字段打上保护标记，避免 MergeOldNew() 因为 997 变化返回 PartialDenied
            {
                Protect997(
false,
ref strOldMarc);

                Protect997(
false,
ref strNewMarc);
            }

            string strComment = "";
            bool bNotAccepted = false;

            if (string.IsNullOrEmpty(strFieldNameList) == true)
            {
                strFieldNameList = "*:***-***"; // 所有字段都允许操作
            }

            // 按照字段修改权限定义，合并新旧两个 MARC 记录
            // return:
            //      -1  出错
            //      0   成功
            //      1   有部分修改要求被拒绝
            nRet = MarcDiff.MergeOldNew(
                strDefaultOperation,
                strFieldNameList,
                strChangeableFieldNameList,
                strOldMarc,
                ref strNewMarc,
                out strComment,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                bNotAccepted = true;


            // 要将 strNewMarc 处理前的 856 字段个数和处理后的 856 字段个数进行比较，如果有变化，就表示有的字段没有被接纳，...
            if (b856Masked == true
                && field_856_count > Get856Count(strNewMarc))
            {
                // TODO: 这里还可以精细处理一下，当 strFieldNameList 中规定不能保存 856 时，strComment 就不要增加内容了
                bNotAccepted = true;
                if (string.IsNullOrEmpty(strComment) == false)
                    strComment += "; ";
                strComment += "部分 856 字段的修改被拒绝(因为其权限导致当前用户获取被限制)";
            }

            // 2017/5/5
            if (strDefaultOperation == "delete"
                && IsBlankHeader(strNewMarc))
                strNewBiblioXml = "";
            else
            {
                nRet = MarcUtil.Marc2XmlEx(strNewMarc,
                    strMarcSyntax,
                    ref strNewBiblioXml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (bNotAccepted == true)
            {
                strError = strComment;
                return 1;
            }
            return 0;
        }

        static bool IsBlankHeader(string strMARC)
        {
            if (strMARC == "????????????????????????")
                return true;
            return false;
        }

#if NO
        // 根据允许的字段名列表，合并新旧两条书目记录
        // 列表中不允许的字段，沿用旧记录中的原始字段内容
        // 算法等于在 new 中复原了那些不让修改的 MARC 字段
        static int MergeOldNewBiblioByFieldNameList(
            string strFieldNameList,
            string strOldBiblioXml,
            ref string strNewBiblioXml,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlDocument domNew = new XmlDocument();
            if (String.IsNullOrEmpty(strNewBiblioXml) == true)
                strNewBiblioXml = "<root />";
            try
            {
                domNew.LoadXml(strNewBiblioXml);
            }
            catch (Exception ex)
            {
                strError = "strNewBiblioXml装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            string strNewSave = domNew.OuterXml;

            XmlDocument domOld = new XmlDocument();
            if (String.IsNullOrEmpty(strOldBiblioXml) == true
                || (string.IsNullOrEmpty(strOldBiblioXml) == false && strOldBiblioXml.Length == 1))
                strOldBiblioXml = "<root />";
            try
            {
                domOld.LoadXml(strOldBiblioXml);
            }
            catch (Exception ex)
            {
                strError = "strOldBiblioXml装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            string strMarcSyntax = "";

            // 探测MARC格式
            // return:
            //      -1  error
            //      0   无法探测
            //      1   探测到了
            nRet = DetectMarcSyntax(domNew,
            out strMarcSyntax,
            out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                nRet = DetectMarcSyntax(domOld,
out strMarcSyntax,
out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "无法探测到 MARC 格式";
                    return -1;
                }
            }

            FieldNameList list = new FieldNameList();
            nRet = list.Build(strFieldNameList, out strError);
            if (nRet == -1)
                return -1;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNode old_root = domOld.DocumentElement.SelectSingleNode("//" + strMarcSyntax + ":record", nsmgr);
            if (old_root == null)
            {
                // 对 new 过滤出全部可以修改的字段即可
                XmlNodeList new_nodes = domNew.DocumentElement.SelectNodes("//" + strMarcSyntax + ":leader | //" + strMarcSyntax + ":controlfield | //" + strMarcSyntax + ":datafield",
nsmgr);
                foreach (XmlNode node in new_nodes)
                {
                    XmlElement element = (XmlElement)node;
                    string strTag = GetTag(element);

                    // TODO：如果连头标区也要滤除，要保留一个缺省值的头标区
                    if (list.Contains(strTag) == false && strTag != "###")
                        node.ParentNode.RemoveChild(node);

                }
                strNewBiblioXml = domNew.DocumentElement.OuterXml;
                return 0;
            }

            XmlNode new_root = domNew.DocumentElement.SelectSingleNode("//" + strMarcSyntax + ":record", nsmgr);
            if (new_root == null)
            {
                // new 里面没有任何 MARC 字段，因此不必作了
                return 0;
            }

            // 1) 把 new 中的 不允许修改的字段标记出来
            List<XmlNode> reserve_nodes = new List<XmlNode>();
            {
                XmlNodeList new_nodes = domNew.DocumentElement.SelectNodes("//" + strMarcSyntax + ":leader | //" + strMarcSyntax + ":controlfield | //" + strMarcSyntax + ":datafield",
                    nsmgr);
                foreach (XmlNode node in new_nodes)
                {
                    XmlElement element = (XmlElement)node;
                    string strTag = GetTag(element);

                    if (list.Contains(strTag) == false)
                        reserve_nodes.Add(element);
                }
            }

            // 2) 从 old 中把全部不允许修改的字段对应位置一个一个覆盖 new 中的对应字段。如果在 new 中没有找到对应字段，在插入在最后一个同名字段后面，如果没有同名字段，则插入到适当的顺序位置
            XmlNodeList old_nodes = domOld.DocumentElement.SelectNodes("//" + strMarcSyntax + ":leader | //" + strMarcSyntax + ":controlfield | //" + strMarcSyntax + ":datafield",
    nsmgr);
            foreach (XmlNode node in old_nodes)
            {
                XmlElement element = (XmlElement)node;
                string strTag = GetTag(element);

                if (list.Contains(strTag) == true)
                    continue;

                XmlElement last_same_name_node = null;
                // 在另一边(nodes内)寻找对应的字段元素
                // parameters:
                //      last_same_name_node 最后一个同tag名的元素
                XmlElement target = FindElement(element,
                    new_root,
                    out last_same_name_node);
                if (target != null)
                {
                    target.InnerXml = element.InnerXml;
                    if (target.LocalName == "datafield")
                    {
                        // 还要修改 ind1 ind2 属性
                        target.SetAttribute("ind1", element.GetAttribute("ind1"));
                        target.SetAttribute("ind2", element.GetAttribute("ind2"));
                    }
                    reserve_nodes.Remove(target);
                    continue;
                }

                if (last_same_name_node != null)
                {
                    last_same_name_node.InsertAfter(domNew.ImportNode(element, true), last_same_name_node);
                    continue;
                }

                // 找到一个合适的位置插入
                insertSequence(element, new_root);
            }

            // 3) 在 new 中把没有被覆盖的标记了的字段全部删除
            foreach (XmlNode node in reserve_nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            strNewBiblioXml = domNew.DocumentElement.OuterXml;
            return 0;
        }

        static string GetTag(XmlElement element)
        {
            string strTag = element.GetAttribute("tag");
            if (element.LocalName == "leader")
                strTag = "###";
            return strTag;
        }

        public static void insertSequence(XmlElement element,
            XmlNode root)
        {
            string strTag = GetTag(element);
            if (strTag == "###")
            {
                // TODO: 注意插入前是否已经有了一个头标区，避免插入后变成两个
                root.InsertBefore(root.OwnerDocument.ImportNode(element, true), null);
                return;
            }

            // 寻找插入位置
            List<int> values = new List<int>(); // 累积每个比较结果数字
            int nInsertPos = -1;
            int i = 0;
            foreach (XmlNode current in root.ChildNodes)
            {
                if (current.NodeType != XmlNodeType.Element)
                {
                    i++;
                    continue;
                }
                XmlElement current_element = (XmlElement)current;
                string strCurrentTag = GetTag(current_element);

                int nBigThanCurrent = 0;   // 相当于node和当前对象相减

                nBigThanCurrent = string.Compare(strTag, strCurrentTag);
                if (nBigThanCurrent < 0)
                {
                    nInsertPos = i;
                    break;
                }
                if (nBigThanCurrent == 0)
                {
                    /*
                    if ((style & InsertSequenceStyle.PreferHead) != 0)
                    {
                        nInsertPos = i;
                        break;
                    }
                     * */
                }

                // 刚刚遇到过相等的一段，但在当前位置结束了相等 (或者开始变大，或者开始变小)
                if (nBigThanCurrent != 0 && values.Count > 0 && values[values.Count - 1] == 0)
                {
                        nInsertPos = i - 1;
                        break;
                }

                values.Add(nBigThanCurrent);
                i++;
            }

            if (nInsertPos == -1)
            {
                root.AppendChild(root.OwnerDocument.ImportNode(element, true));
                return;
            }

            root.InsertBefore(root.OwnerDocument.ImportNode(element, true), root.ChildNodes[nInsertPos]);
        }

        // 获得一个元素在兄弟元素中同名的位置
        static int GetDupCount(XmlElement start)
        {
            string strTag = GetTag(start);
            int nCount = 0;
            XmlNode current = start.PreviousSibling;
            while (current != null)
            {
                if (current.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = (XmlElement)current;
                    string strCurrentTag = GetTag(element);

                    if (strCurrentTag == strTag)
                        nCount ++;
                }
                current = current.PreviousSibling;
            }

            return nCount;
        }

        // 在另一边(root之下内)寻找对应的字段元素
        // parameters:
        //      root    要寻找的元素的容器元素
        //      last_same_name_node 最后一个同tag名的元素
        static XmlElement FindElement(XmlElement start,
            XmlNode root,
            out XmlElement last_same_name_node)
        {
            last_same_name_node = null;

            string strTag = GetTag(start);

            int dup = GetDupCount(start);

            int nCount = 0;
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                XmlElement element = (XmlElement)node;
                string strCurrenTag = GetTag(element);

                if (strTag == strCurrenTag)
                {
                    if (nCount == dup)
                        return element;
                    nCount++;
                    last_same_name_node = element;
                }
            }

            return null;    // 没有找到
        }

#endif
        // 给 997 字段打上保护标记
        public static void Protect997(
bool clearMaskBefore,
ref string strMARC)
        {
            if (string.IsNullOrEmpty(strMARC) == true)
                return;

            string strMaskChar = new string((char)1, 1);

            if (clearMaskBefore)
            {
                // 在处理前替换记录中可能出现的 (char)1
                // 建议在调用本函数前，探测 strMARC 中是否有这个符号，如果有，可能是相关环节检查不严创建了这样的记录，需要进一步检查处理
                strMARC = strMARC.Replace(strMaskChar, "*");
            }

            MarcRecord record = new MarcRecord(strMARC);
            var fields = record.select("field[@name='997']");
            int nCount = 0;
            foreach (MarcField field in fields)
            {
                if (field.Content.EndsWith(strMaskChar) == false)
                {
                    field.Content += strMaskChar;
                    nCount++;
                }
            }

            if (nCount > 0)
                strMARC = record.Text;
        }

        public static int Get856Count(string strMARC)
        {
            MarcRecord record = new MarcRecord(strMARC);
            return record.select("field[@name='856']").count;
        }

        // 对 MARC 记录进行标记，将那些当前用户无法读取的 856 字段打上特殊标记(内码为 1 的字符)
        // parameters:
        //      clearMaskBefore 是否要在处理之前清理 MARC 记录中以前的标记字符?
        // return:
        //      -1  出错
        //      其他  标记的 856 字段个数
        public static int MaskCantGet856(
            string strUserRights,
            bool clearMaskBefore,
            ref string strMARC,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strMARC) == true)
                return 0;

            // 只要当前账户具备 setobject 或 writeres 权限，等于他可以获取任何对象，为了编辑加工的需要
            if (StringUtil.IsInList("setobject,setbiblioobject", strUserRights) == true
                /*|| StringUtil.IsInList("writeres", strUserRights) == true*/)
                return 0;

            string strMaskChar = new string((char)1, 1);

            if (clearMaskBefore)
            {
                // 在处理前替换记录中可能出现的 (char)1
                // 建议在调用本函数前，探测 strMARC 中是否有这个符号，如果有，可能是相关环节检查不严创建了这样的记录，需要进一步检查处理
                strMARC = strMARC.Replace(strMaskChar, "*");
            }

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");

            if (fields.count == 0)
                return 0;

            int nCount = 0;
            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;

                if (string.IsNullOrEmpty(x) == true)
                    continue;

                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strObjectRights = (string)table["rights"];

                if (string.IsNullOrEmpty(strObjectRights) == true)
                    continue;

                // 对象是否允许被获取?
                if (CanGet("download", strUserRights, strObjectRights) == false
                    && CanGet("preview", strUserRights, strObjectRights) == false /*2022/10/13*/)
                {
                    field.Content += strMaskChar;
                    nCount++;
                }
            }

            if (nCount > 0)
                strMARC = record.Text;
            return nCount;
        }

        // 对 MARC 记录进行过滤，将那些当前用户无法读取的 856 字段删除
        // return:
        //      -1  出错
        //      其他  滤除的 856 字段个数
        public static int RemoveCantGet856(
            string strUserRights,
            ref string strMARC,
            out string strError)
        {
            strError = "";

            // 只要当前账户具备 setobject 或 writeres 权限，等于他可以获取任何对象，为了编辑加工的需要
            if (StringUtil.IsInList("setobject,setbiblioobject", strUserRights) == true
                /*|| StringUtil.IsInList("writeres", strUserRights) == true*/)
                return 0;

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");

            if (fields.count == 0)
                return 0;

            List<MarcField> delete_fields = new List<MarcField>();
            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;

                if (string.IsNullOrEmpty(x) == true)
                    continue;

                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strObjectRights = (string)table["rights"];

                if (string.IsNullOrEmpty(strObjectRights) == true)
                    continue;

                // 对象是否允许被获取?
                if (CanGet("download", strUserRights, strObjectRights) == false
                    && CanGet("preview", strUserRights, strObjectRights) == false/*2022/10/13*/)
                    delete_fields.Add(field);
            }

            foreach (MarcField field in delete_fields)
            {
                field.detach();
            }

            if (delete_fields.Count > 0)
                strMARC = record.Text;
            return delete_fields.Count;
        }

        // 通知读者推荐的新书到书
        // parameters:
        //      strLibraryCodeList  要通知的读者所从属的馆代码列表。空表示只通知全局读者
        public LibraryServerResult NotifyNewBook(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            string strLibraryCodeList)
        {
            string strError = "";
            LibraryServerResult result = new LibraryServerResult();
            int nRet = 0;

            // 检查 strLibraryCodeList 中的馆代码是否全在当前用户管辖之下
            if (sessioninfo.GlobalUser == false)
            {
                if (FullyContainIn(strLibraryCodeList, sessioninfo.LibraryCodeList) == false)
                {
                    strError = "所请求的馆代码 '" + strLibraryCodeList + "' 不是完全包含于当前用户的管辖范围馆代码 '" + sessioninfo.LibraryCodeList + "' 中";
                    goto ERROR1;
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 探测书目记录有没有下属的评注记录
            List<DeleteEntityInfo> commentinfos = null;
            long lHitCount = 0;
            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.CommentItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml", // 在DeleteEntityInfo结构中返回OldRecord内容， 并且不要检查流通信息
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out commentinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(commentinfos.Count == 0, "");
            }

            // 如果没有评注记录，则不必通知
            if (commentinfos == null || commentinfos.Count == 0)
            {
                result.Value = 0;   // 表示没有可通知的
                return result;
            }

            // List<string> suggestors = new List<string>();
            Hashtable suggestor_table = new Hashtable();    // 操作者 --> 馆代码
            foreach (DeleteEntityInfo info in commentinfos)
            {
                if (string.IsNullOrEmpty(info.OldRecord) == true)
                    continue;

                XmlDocument domExist = new XmlDocument();
                try
                {
                    domExist.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "评注记录 '" + info.RecPath + "' 装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                // 是否为推荐者
                string strType = DomUtil.GetElementText(domExist.DocumentElement, "type");
                if (strType != "订购征询")
                    continue;
                string strOrderSuggestion = DomUtil.GetElementText(domExist.DocumentElement, "orderSuggestion");
                if (strOrderSuggestion != "yes")
                    continue;

                string strLibraryCode = DomUtil.GetElementText(domExist.DocumentElement, "libraryCode");
                // 检查读者所从属的馆代码是否在列表中
                if (string.IsNullOrEmpty(strLibraryCodeList) == true
                    && string.IsNullOrEmpty(strLibraryCode) == true)
                {
                    // 全局的读者，全局的馆代码要求
                }
                else
                {
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                        continue;   // 不是列表中的分馆的读者不要通知。因为一旦通知了，会发生这部分读者到自己的分馆借不到书的窘况
                }

                // 获得创建者用户名
                XmlNode node = domExist.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
                if (node == null)
                    continue;
                string strOperator = DomUtil.GetAttr(node, "operator");
                if (string.IsNullOrEmpty(strOperator) == true)
                    continue;

                // suggestors.Add(strOperator);

                // 从评注记录中获得<libraryCode>元素，这是创建评注记录时刻的操作者的馆代码
                // 这样就可以不必根据读者记录路径来推导读者的馆代码
                suggestor_table[strOperator] = strLibraryCode;  // 自然就去重了
            }

            if (suggestor_table.Count == 0)
            {
                result.Value = 0;   // 表示没有可通知的
                return result;
            }

            // 获得书目记录
#if NO
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }
#endif
            string strMetaData = "";
            string strOutputPath = "";
            byte[] exist_timestamp = null;
            string strBiblioXml = "";

            // 先读出数据库中此位置的已有记录
            long lRet = channel.GetRes(strBiblioRecPath,
                out strBiblioXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (strBiblioRecPath != strOutputPath)
            {
                strError = "根据路径 '" + strBiblioRecPath + "' 读入原有记录时，发现返回的路径 '" + strOutputPath + "' 和前者不一致";
                goto ERROR1;
            }

            // 创建书目摘要
            string[] formats = new string[1];
            formats[0] = "summary";
            List<string> temp_results = null;
            // return:
            //      -1  出错
            //      0   成功
            //      1   有警告信息返回在 strError 中
            nRet = BuildFormats(
                sessioninfo,
                strBiblioRecPath,
                strBiblioXml,
                "", // strOutputPath,   // 册记录的路径
                "", // strMetaData,     // 册记录的metadata
                null,
                formats,
                out temp_results,
                out strError);
            if (nRet == -1 || nRet == 1)
                goto ERROR1;
            if (temp_results == null || temp_results.Count == 0)
            {
                strError = "temp_results error";
                goto ERROR1;
            }
            string strSummary = temp_results[0];

            // 去重
            // StringUtil.RemoveDupNoSort(ref suggestors);

            foreach (string id in suggestor_table.Keys)
            {
                if (string.IsNullOrEmpty(id) == true)
                    continue;

                string strLibraryCode = (string)suggestor_table[id];

#if NO
                string strReaderXml = "";
                byte[] reader_timestamp = null;
                string strOutputReaderRecPath = "";

                int nIsReader = -1; // -1 不清楚  0 不是读者 1 是读者
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetReaderRecXml(
                    sessioninfo.Channels,
                    id,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "读入读者记录 '" + id + "' 时发生错误: " + strError;
                    goto ERROR1;
                }

                if (nRet == 0 || string.IsNullOrEmpty(strReaderXml) == true)
                    nIsReader = 0; // 不是读者
                else
                    nIsReader = 1;

                // 获得读者从属的馆代码
                string strLibraryCode = "";

                if (nIsReader == 1)
                {
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
                        sessioninfo.LibraryCodeList,
                        out strLibraryCode) == false)
                    {
                        continue;   // 读者的馆代码不在当前用户管辖范围内
                    }

                    // 检查读者所从属的馆代码是否在列表中
                    if (string.IsNullOrEmpty(strLibraryCodeList) == true
                        && string.IsNullOrEmpty(strLibraryCode) == true)
                    {
                        // 全局的读者，全局的馆代码要求
                    }
                    else
                    {
                        if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                            continue;   // 不是列表中的分馆的读者不要通知。因为一旦通知了，会发生这部分读者到自己的分馆借不到书的窘况
                    }
                }
                else
                {
                    // 工作人员帐户，获得工作人员的馆代码

                    UserInfo userinfo = null;
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetUserInfo(id,
                        out userinfo,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取帐户 '" + id + "' 的信息时出错: " + strError;
                        goto ERROR1;
                    }

                    if (nRet == 0)
                        continue;   // 没有这个用户

                    // 检查工作人员管辖的图书馆和 strLibraryCodeList 之间的交叉情况
                    Debug.Assert(userinfo != null, "");
                    strLibraryCode = Cross(userinfo.LibraryCode, strLibraryCodeList);
                    if (strLibraryCode == null)
                        continue;   // 和 strLibraryCodeList 没有交集
                }
#endif

                string strBody = "尊敬的读者：\r\n\r\n您推荐订购的图书\r\n\r\n------\r\n" + strSummary + "\r\n------\r\n\r\n已经到达图书馆，欢迎您到图书馆借阅或阅览。感谢您对图书馆工作的大力支持。";
                nRet = MessageNotify(
                    sessioninfo,
                    strLibraryCode,
                    id,
                    "", // strReaderXml,
                    "新书到书通知", // strTitle,
                    strBody,
                    "text", // strMime,    // "text",
                    "图书馆",
                    "新书到书通知",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            result.Value = 0;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 向读者发出通知消息
        // parameters:
        //      strLibraryCode  读者所从属的馆代码。如果为null(而""是表示全局用户)，表示希望函数内部自行获取馆代码，并且判断是否在当前用户的管辖范围内，如果不在管辖范围内则不发送消息。而如果本参数不为空调用的话，则假定调用前已经检查过了，函数内不再检查
        //      strReaderXml    读者记录XML。如果为空，表示函数中需要自动获得读者记录XML
        // return
        //      -1  出错
        //      0   成功
        //      1   因为读者馆代码不在当前用户管辖范围内，而放弃发送
        public int MessageNotify(
            SessionInfo sessioninfo,
            string strLibraryCode,
            string strReaderBarcode,
            string strReaderXml,
            string strTitle,
            string strBody,
            string strMime,    // "text",
            string strSender,
            string strErrorType,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<string> bodytypes = new List<string>();
            bodytypes.Add("dpmail");
            bodytypes.Add("email");
            if (this.m_externalMessageInterfaces != null)
            {
                foreach (MessageInterface message_interface in this.m_externalMessageInterfaces)
                {
                    bodytypes.Add(message_interface.Type);
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strReaderEmailAddress = "";

            // 读入读者记录
            byte[] reader_timestamp = null;
            string strOutputReaderRecPath = "";
            XmlDocument readerdom = null;
            int nIsReader = -1; // -1 不清楚  0 不是读者 1 是读者

            for (int i = 0; i < bodytypes.Count; i++)
            {
                string strBodyType = bodytypes[i];

                if (strBodyType == "email")
                {
                    if (readerdom == null && (nIsReader == -1 || nIsReader == 1))
                    {
                        if (string.IsNullOrEmpty(strReaderXml) == true)
                        {
                            // return:
                            //      -1  error
                            //      0   not found
                            //      1   命中1条
                            //      >1  命中多于1条
                            nRet = this.GetReaderRecXml(
                                // sessioninfo.Channels,
                                channel,
                                strReaderBarcode,
                                out strReaderXml,
                                out strOutputReaderRecPath,
                                out reader_timestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                // text-level: 内部错误
                                strError = "读入读者记录 '" + strReaderBarcode + "' 时发生错误: " + strError;
                                return -1;
                            }

                            if (nRet == 0 || string.IsNullOrEmpty(strReaderXml) == true)
                            {
                                nIsReader = 0; // 不是读者
                                continue;
                            }

                            if (strLibraryCode == null)
                            {
                                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                                if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
                                    sessioninfo.LibraryCodeList,
                                    out strLibraryCode) == false)
                                {
                                    strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                                    return 1;
                                }
                            }
                        }

                        nIsReader = 1;
                        nRet = LibraryApplication.LoadToDom(strReaderXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                            return -1;
                        }
                    }
                    string strValue = DomUtil.GetElementText(readerdom.DocumentElement,
                        "email");
                    strReaderEmailAddress = LibraryServerUtil.GetEmailAddress(strValue);
                    if (String.IsNullOrEmpty(strReaderEmailAddress) == true)
                        continue;
                }

                if (strBodyType == "dpmail")
                {
                    if (this.MessageCenter == null)
                    {
                        continue;
                    }
                }

#if NO
                List<string> notifiedBarcodes = new List<string>();


                // 获得特定类型的已通知过的册条码号列表
                // return:
                //      -1  error
                //      其他    notifiedBarcodes中条码号个数
                nRet = GetNotifiedBarcodes(readerdom,
                    strBodyType,
                    out notifiedBarcodes,
                    out strError);
                if (nRet == -1)
                    return -1;
#endif


                bool bSendMessageError = false;


                // dpmail
                if (strBodyType == "dpmail")
                {
                    // 发送消息
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = this.MessageCenter.SendMessage(
                        sessioninfo.Channels,
                        strReaderBarcode,
                        strSender, // "图书馆",
                        strTitle,
                        strMime,    // "text",
                        strBody,
                        false,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "发送dpmail出错: " + strError;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            strErrorType,
                            "dpmail message " + strErrorType + "消息发送错误数",
                            1);
                        bSendMessageError = true;
                        // return -1;
                    }
                    else
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            strErrorType,
                            "dpmail" + strErrorType + "人数",
                            1);
                    }
                }

                // 扩展消息
                MessageInterface external_interface = this.GetMessageInterface(strBodyType);

                if (external_interface != null && nIsReader != 0)
                {
                    if (readerdom == null)
                    {
                        if (string.IsNullOrEmpty(strReaderXml) == true)
                        {
                            // return:
                            //      -1  error
                            //      0   not found
                            //      1   命中1条
                            //      >1  命中多于1条
                            nRet = this.GetReaderRecXml(
                                // sessioninfo.Channels,
                                channel,
                                strReaderBarcode,
                                out strReaderXml,
                                out strOutputReaderRecPath,
                                out reader_timestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                // text-level: 内部错误
                                strError = "读入读者记录 '" + strReaderBarcode + "' 时发生错误: " + strError;
                                return -1;
                            }

                            if (nRet == 0 || string.IsNullOrEmpty(strReaderXml) == true)
                            {
                                nIsReader = 0; // 不是读者
                                continue;
                            }

                            if (strLibraryCode == null)
                            {
                                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                                if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
                                    sessioninfo.LibraryCodeList,
                                    out strLibraryCode) == false)
                                {
                                    strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                                    return 1;
                                }
                            }
                        }

                        nIsReader = 1;
                        nRet = LibraryApplication.LoadToDom(strReaderXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                            return -1;
                        }
                    }


                    // 发送消息
                    try
                    {
                        // 发送一条消息
                        // parameters:
                        //      strPatronBarcode    读者证条码号
                        //      strPatronXml    读者记录XML字符串。如果需要除证条码号以外的某些字段来确定消息发送地址，可以从XML记录中取
                        //      strMessageText  消息文字
                        //      strError    [out]返回错误字符串
                        // return:
                        //      -1  发送失败
                        //      0   没有必要发送
                        //      1   发送成功
                        nRet = external_interface.HostObj.SendMessage(
                            strReaderBarcode,
                            readerdom.DocumentElement.OuterXml,
                            strBody,
                            strLibraryCode,
                            out strError);
                    }
                    catch (Exception ex)
                    {
                        strError = external_interface.Type + " 类型的外部消息接口Assembly中SendMessage()函数抛出异常: " + ex.Message;
                        nRet = -1;
                    }
                    if (nRet == -1)
                    {
                        strError = "向读者 '" + strReaderBarcode + "' 发送" + external_interface.Type + " message时出错: " + strError;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            strErrorType,
                            external_interface.Type + " message " + strErrorType + "消息发送错误数",
                            1);
                        bSendMessageError = true;
                        // return -1;
                    }
                    else if (nRet == 1)
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            strErrorType,
                            external_interface.Type + " message " + strErrorType + "人数",
                            1);
                    }
                }

                // email
                if (strBodyType == "email")
                {
                    // 发送email
                    // return:
                    //      -1  error
                    //      0   not found smtp server cfg
                    //      1   succeed
                    nRet = this.SendEmail(strReaderEmailAddress,
                        strTitle,
                        strBody,
                        strMime,
                        (text) =>
                        {
                            ReadersMonitor.WriteEmailLogConditional(this, text);
                        },
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "发送 email 到 '" + strReaderEmailAddress + "' 出错: " + strError;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            strErrorType,
                            "email message " + strErrorType + "消息发送错误数",
                            1);
                        bSendMessageError = true;
                        // return -1;
                    }
                    else if (nRet == 1)
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            strErrorType,
                            "email" + strErrorType + "人数",
                            1);
                    }
                }
            } // end of for

            return 0;
        }

        // parameters:
        //      strParameters   ()中的部分
        public static bool IsInAccessList(string strSub,
            string strList,
            out string strParameters)
        {
            strParameters = "";

            List<string> segments = StringUtil.SplitString(strList,
    ",",
    new string[] { "()" },
    StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in segments)
            {
                string strLeft = "";
                string strRight = "";
                int nRet = s.IndexOf("(");
                if (nRet != -1)
                {
                    strLeft = s.Substring(0, nRet).Trim();
                    strRight = s.Substring(nRet + 1).Trim();
                    if (string.IsNullOrEmpty(strRight) == false && strRight[strRight.Length - 1] == ')')
                        strRight = strRight.Substring(0, strRight.Length - 1);
                }
                else
                    strLeft = s;

                if (strLeft == strSub)
                {
                    strParameters = strRight;
                    return true;
                }
            }

            return false;
        }

        // 修改书目或规范记录
        // parameters:
        //      strAction   动作。为"new" "change" "delete" "onlydeletebiblio" "onlydeletesubrecord" "checkunique" 之一。"delete"在删除书目记录的同时，会自动删除下属的实体记录。不过要求实体均未被借出才能删除。
        //      strBiblioRecPath    书目(规范)记录路径。TODO: 这个参数的值是否允许为空？如果不允许，要在函数中检查和尽早报错
        //      strBiblioType   xml 或 iso2709。iso2709 格式可以包含编码方式，例如 iso2709:utf-8
        //      baTimestamp 时间戳。如果为新创建记录，可以为null 
        //      strOutputBiblioRecPath 输出的书目记录路径。当strBiblioRecPath中末级为问号，表示追加保存书目记录的时候，本参数返回实际保存的书目记录路径
        //      baOutputTimestamp   操作完成后，新的时间戳
        // 日志:
        //      要产生操作日志
        public LibraryServerResult SetBiblioInfo(
            SessionInfo sessioninfo,
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            string strStyle,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp)
        {
            string strError = "";
            long lRet = 0;
            int nRet = 0;

            strOutputBiblioRecPath = "";
            baOutputTimestamp = null;

            LibraryServerResult result = new LibraryServerResult();

            bool bNoCheckDup = false;   // 是否为不查重?
            bool bNoEventLog = false;   // 是否为不记入事件日志?
            bool bNoOperations = false; // 是否为不要覆盖<operations>内容
            bool bSimulate = StringUtil.IsInList("simulate", strStyle);     // 是否为模拟操作? 2015/6/9
            bool bForce = false;

            if (StringUtil.IsInList("force", strStyle) == true)
            {
                bForce = true;

                if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "带有风格 'force' 的修改书目信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (sessioninfo.GlobalUser == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改书目信息的" + strAction + "操作被拒绝。只有全局用户并具备 restore 权限才能进行这样的操作。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (StringUtil.IsInList("nocheckdup", strStyle) == true)
            {
                bNoCheckDup = true;
            }

            if (StringUtil.IsInList("noeventlog", strStyle) == true)
            {
                bNoEventLog = true;

                // 2017/3/16
                if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "带有风格 'noeventlog' 的修改书目信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (StringUtil.IsInList("nooperations", strStyle) == true)
            {
                bNoOperations = true;
            }

            if (bNoCheckDup == true)
            {
                if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "带有风格 'nocheckdup' 的修改书目信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (sessioninfo.GlobalUser == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "带有风格 'nocheckdup' 的修改书目信息的" + strAction + "操作被拒绝。只有全局用户并具备 restore 权限才能进行这样的操作。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (bNoEventLog == true)
            {
                if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "带有风格 'noeventlog' 的修改书目信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (sessioninfo.GlobalUser == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "带有风格 'noeventlog' 的修改书目信息的" + strAction + "操作被拒绝。只有全局用户并具备 restore 权限才能进行这样的操作。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            bool bChangePartDenied = false; // 修改操作部分被拒绝
            string strDeniedComment = "";   // 关于部分字段被拒绝的注释

            string strLibraryCode = ""; // 图书馆代码
            if (sessioninfo.Account != null)
                strLibraryCode = sessioninfo.Account.AccountLibraryCode;

            // 检查参数
            strAction = strAction.ToLower();

            // 2015/6/8
            if (StringUtil.HasHead(strAction, "simulate_") == true)
            {
                strAction = strAction.Substring("simulate_".Length);
                bSimulate = true;
            }

            if (strAction != "new"
                && strAction != "change"
                && strAction != "delete"
                && strAction != "onlydeletebiblio"
                && strAction != "onlydeletesubrecord"
                && strAction != "checkunique")
            {
                strError = "strAction参数值应当为new change delete onlydeletebiblio onlydeletesubrecord checkunique之一  (然而当前为 '" + strAction + "')";
                goto ERROR1;
            }

            if (strAction == "new"
                && string.IsNullOrEmpty(strBiblioRecPath) == false
                && ResPath.IsAppendRecPath(strBiblioRecPath) == false)
            {
                // 如果 style 中包含 new，则 strAction 不会被改变
                if (StringUtil.IsInList("new", strStyle) == false)
                    strAction = "change";

#if NO
                strError = "当(new)创建书目记录的时候，只能使用“书目库名/?”形式的路径(而不能使用 '" + strBiblioRecPath + "' 形式)。如果要在指定位置保存，可使用修改(change)子功能";
                goto ERROR1;
#endif
            }

            strBiblioType = strBiblioType.ToLower();

            string strFormat = "";
            Encoding encoding = Encoding.UTF8;

            if (strBiblioType == "xml"
                || (strBiblioType == "marcquery" || strBiblioType == "marc"))
                strFormat = strBiblioType;
            else if (IsResultType(strBiblioType, "iso2709") == true)
            {
                List<string> parts = StringUtil.ParseTwoPart(strBiblioType, ":");
                string strEncoding = parts[1];
                if (string.IsNullOrEmpty(strEncoding))
                    strEncoding = "utf-8";

                try
                {
                    encoding = Encoding.GetEncoding(strEncoding);
                }
                catch (Exception ex)
                {
                    strError = "strBiblioType 参数值中的编码方式 '" + strEncoding + "' 不合法: " + ex.Message;
                    goto ERROR1;
                }

                strFormat = parts[0];
            }
            else
            {
                if (string.IsNullOrEmpty(strBiblio) == false
                    && strAction != "delete")
                {
                    strError = "strBiblioType参数值必须为\"xml\" \"iso2709\" \"marcquery\"之一";
                    goto ERROR1;
                }
            }

            {
                if (this.TestMode == true || sessioninfo.TestMode == true)
                {
                    // 检查评估模式
                    // return:
                    //      -1  检查过程出错
                    //      0   可以通过
                    //      1   不允许通过
                    nRet = CheckTestModePath(strBiblioRecPath,
                        out strError);
                    if (nRet != 0)
                    {
                        strError = "修改书目记录的操作被拒绝: " + strError;
                        goto ERROR1;
                    }
                }
            }

            string strUnionCatalogStyle = "";
            string strBiblioDbName = "";
            bool bRightVerified = false;
            bool bOwnerOnly = false;

            string strChangeableFieldNameList = null;
            string strAccessParameters = "";
            ItemDbCfg cfg = null;
            string strDbType = "";
            string strDbTypeCaption = "";

            // 检查数据库路径，看看是不是已经正规定义的书目库或规范库？
            if (String.IsNullOrEmpty(strBiblioRecPath) == false)
            {
                strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

                if (this.IsBiblioDbName(strBiblioDbName) == true)
                {
                    strDbType = "biblio";
                    strDbTypeCaption = "书目库";
                }
                else if (this.IsAuthorityDbName(strBiblioDbName) == true)
                {
                    strDbType = "authority";
                    strDbTypeCaption = "规范库";
                }
                else
                {
                    strError = "书目记录路径 '" + strBiblioRecPath + "' 中包含的数据库名 '" + strBiblioDbName + "' 不是合法的书目库名或规范库名";
                    goto ERROR1;
                }

#if NO
                if (this.TestMode == true || sessioninfo.TestMode == true)
                {
                    string strID = ResPath.GetRecordId(strBiblioRecPath);
                    if (StringUtil.IsPureNumber(strID) == true)
                    {
                        long v = 0;
                        long.TryParse(strID, out v);
                        if (v > 1000)
                        {
                            strError = "评估模式下只能修改 ID 小于等于 1000 的书目记录";
                            goto ERROR1;
                        }
                    }
                }
#endif

                if (strDbType == "biblio")
                    cfg = GetBiblioDbCfg(strBiblioDbName);
                else if (strDbType == "authority")
                    cfg = GetAuthorityDbCfg(strBiblioDbName);

                if (cfg == null)
                {
                    strError = "获得" + strDbTypeCaption + " '" + strBiblioDbName + "' 的配置信息时出错";
                    goto ERROR1;
                }
                Debug.Assert(cfg != null, "");
                strUnionCatalogStyle = cfg.UnionCatalogStyle;

#if OLDVERSION
                // 检查存取权限
                if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                {
                    string strAccessActionList = "";
                    // return:
                    //      null    指定的操作类型的权限没有定义
                    //      ""      定义了指定类型的操作权限，但是否定的定义
                    //      其它      权限列表。* 表示通配的权限列表
                    strAccessActionList = GetDbOperRights(sessioninfo.Access,
                        strBiblioDbName,
                        strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo");
                    if (strAccessActionList == null)
                    {
                        // 看看是不是关于 setbiblioinfo 的任何权限都没有定义?
                        strAccessActionList = GetDbOperRights(sessioninfo.Access,
                            "",
                            strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo");
                        if (strAccessActionList == null)
                        {
                            // 2013/4/18
                            // TODO: 可以提示"既没有... 也没有 ..."
                            goto CHECK_RIGHTS_2;
                        }
                        else
                        {
                            strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 " +
                                (strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo") +
                                " " + strAction + " 操作的存取权限";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    if (strAccessActionList == "*")
                    {
                        // 通配
                    }
                    else
                    {
                        if (strAction == "delete"
                            && IsInAccessList("ownerdelete", strAccessActionList, out strAccessParameters) == true)
                        {
                            bOwnerOnly = true;
                        }
                        else if (strAction == "change"
                            && IsInAccessList("ownerchange", strAccessActionList, out strAccessParameters) == true)
                        {
                            bOwnerOnly = true;
                        }
                        else if (strAction == "onlydeletebiblio"
                            && IsInAccessList("owneronlydeletebiblio", strAccessActionList, out strAccessParameters) == true)
                        {
                            bOwnerOnly = true;
                        }
                        else if (strAction == "onlydeletesubrecord"
                            && IsInAccessList("owneronlydeletesubrecord", strAccessActionList, out strAccessParameters) == true)
                        {
                            bOwnerOnly = true;
                        }
                        else if (IsInAccessList(strAction, strAccessActionList, out strAccessParameters) == false)
                        {
                            strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 " +
                                (strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo") +
                                " " + strAction + " 操作的存取权限";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }

                    bRightVerified = true;
                }

#endif

                // 检查当前用户是否具备 SetBiblioInfo() API 的存取定义权限
                // parameters:
                //      check_normal_right 是否要连带一起检查普通权限？如果不连带，则本函数可能返回 "normal"，意思是需要追加检查一下普通权限
                // return:
                //      "normal"    (存取定义已经满足要求了，但)还需要进一步检查普通权限
                //      null    具备权限
                //      其它      不具备权限。文字是报错信息
                var error = CheckSetBiblioInfoAccess(
                    sessioninfo,
                    strDbType,
                    strBiblioDbName,
                    strAction,
                    false,
                    out strAccessParameters,
                    out bOwnerOnly);
                if (error == "normal")
                    goto CHECK_RIGHTS_2;
                if (error != null)
                {
                    result.Value = -1;
                    result.ErrorInfo = error;
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                bRightVerified = true;
            }

        CHECK_RIGHTS_2:
            if (bRightVerified == false)
            {
                if (strDbType == "biblio")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("setbiblioinfo,writerecord,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息被拒绝。不具备 setbiblioinfo 或 writerecord 或 order 权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                if (strDbType == "authority")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("setauthorityinfo,writerecord", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置规范信息被拒绝。不具备 setauthorityinfo 或 writerecord 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }


            // 2023/2/11 获得 getbiblioinfo 可读的字段范围，在合并 MARC 记录的时候要用到
            {
                var error = CheckGetBiblioInfoAccess(
                    sessioninfo,
                    strDbType,
                    strBiblioDbName,
                    false,
                    out strChangeableFieldNameList);
                if (error == "normal")
                {
                    if (StringUtil.IsInList("getbiblioinfo", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息被拒绝。虽然当前账户具备 setbiblioinfo 权限(或对应存取定义)，但不具备 getbiblioinfo 权限(或对应存取定义)，这违反了权限安全性规则。请修改账户权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    strChangeableFieldNameList = null;  // 表示全部字段都可以修改
                }
                else if (error != null)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"设置书目信息被拒绝。虽然当前账户具备 setbiblioinfo 权限(或对应存取定义)，但不具备 getbiblioinfo 权限(或对应存取定义)，这违反了权限安全性规则。请修改账户权限。\r\n注: 检查 getbiblioinfo 权限时的详情如下: {error}";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }


            // 2016/7/4
            // 看看所保存的数据MARC格式是不是这个数据库要求的格式
            if (string.IsNullOrEmpty(strBiblio) == false
                && (strAction == "new" || strAction == "change" || strAction == "checkunique"))
            {
                // 2016/11/23
                if (strFormat == "iso2709")
                {
                    try
                    {
                        byte[] baRecord = Convert.FromBase64String(strBiblio);
                        // return:
                        //		-2	MARC格式错
                        //		-1	一般错误
                        //		0	正常
                        nRet = MarcUtil.ConvertByteArrayToMarcRecord(baRecord,
                            encoding,
                            true,
                            out string strMARC,
                            out strError);
                        if (nRet != 0)
                        {
                            strError = "strBiblio 参数中的 ISO2709 格式记录不合法: " + strError;
                            goto ERROR1;
                        }

                        if (cfg == null)
                        {
                            strError = "cfg == null。因参数 strBiblioRecPath 为空 (1)";
                            goto ERROR1;
                        }

                        string strBiblioXml = "";
                        nRet = MarcUtil.Marc2XmlEx(strMARC,
    cfg.BiblioDbSyntax,
    ref strBiblioXml,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        strBiblio = strBiblioXml;
                    }
                    catch (Exception ex)
                    {
                        strError = "将 strBiblio 参数值转换为 MARC 或 MARCXML 格式过程中出现异常: " + ex.Message;
                        goto ERROR1;
                    }
                }
                else if (strFormat == "marcquery" || strFormat == "marc")
                {
                    string strBiblioXml = "";
                    nRet = MarcUtil.Marc2XmlEx(strBiblio,
cfg.BiblioDbSyntax,
ref strBiblioXml,
out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    strBiblio = strBiblioXml;
                }
                else
                {
                    // 获得 MARCXML 字符串的 MARC 格式类型
                    // return:
                    //      -1  出错
                    //      0   无法探测
                    //      1   成功探测
                    nRet = MarcUtil.GetMarcSyntax(strBiblio,
        out string strMarcSyntax,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        if (IsEmptyXml(strBiblio) == true)
                        {
                            strMarcSyntax = cfg.BiblioDbSyntax;  // 权且当作数据库一致的 MARC 格式来处理
                        }
                        else
                        {
                            strError = "无法获得 strBiblio 参数值中 MARC 记录的 MARC 格式";
                            goto ERROR1;
                        }
                    }

                    if (cfg == null)
                    {
                        strError = "cfg == null。因参数 strBiblioRecPath 为空 (2)";
                        goto ERROR1;
                    }

                    if (cfg.BiblioDbSyntax != strMarcSyntax)
                    {
                        strError = $"所提交保存的 MARC 格式为 '{strMarcSyntax}'，和{strDbTypeCaption} '{strBiblioDbName}' 的 MARC 格式 '{cfg.BiblioDbSyntax}' 不符合";
                        goto ERROR1;
                    }
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }

            // 准备日志DOM
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            // 操作不涉及到读者库，所以没有<libraryCode>元素
            DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                strDbType == "biblio" ? "setBiblioInfo" : "setAuthorityInfo");
            DomUtil.SetElementText(domOperLog.DocumentElement, "action",
                strAction);
            if (string.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement, "comment",
        strComment);
            }

            string strOperTime = this.Clock.GetClock();

            string strExistingXml = "";
            byte[] exist_timestamp = null;

            if (strAction == "change"
                || strAction == "delete"
                || strAction == "onlydeletebiblio"
                || strAction == "onlydeletesubrecord"
                || strAction == "checkunique")
            {

                // TODO: strBiblioRecPath 为 中文图书/? 时如何表现

                // 先读出数据库中此位置的已有记录
                lRet = channel.GetRes(strBiblioRecPath,
                    out strExistingXml,
                    out string strMetaData,
                    out exist_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.IsNotFoundOrDamaged())    // 2019/5/28
                    {
                        if (strAction == "checkunique")
                            goto SKIP_MEMO_OLDRECORD;

                        // 2013/3/12
                        if (strAction == "change"
                            && bSimulate == false)    // 模拟操作情况下，不在乎以前这个位置的记录是否存在
                        {
#if NO
                            strError = "原有记录 '" + strBiblioRecPath + "' 不存在, 因此 setbiblioinfo " + strAction + " 操作被拒绝 (此时如果要保存新记录，请使用 new 子功能)";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.NotFound;
                            return result;
#endif
                            // 2017/5/5
                            strExistingXml = "";
                            strOutputPath = strBiblioRecPath;
                            exist_timestamp = null;
                        }
                        goto SKIP_MEMO_OLDRECORD;
                    }
                    else
                    {
                        strError = "设置" + strDbTypeCaption + "信息发生错误, 在读入原有记录阶段:" + strError;
                        goto ERROR1;
                    }
                }

                if (strBiblioRecPath != strOutputPath)
                {
                    strError = "根据路径 '" + strBiblioRecPath + "' 读入原有记录时，发现返回的路径 '" + strOutputPath + "' 和前者不一致";
                    goto ERROR1;
                }

                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "oldRecord", strExistingXml);
                DomUtil.SetAttr(node, "recPath", strBiblioRecPath);

                // 检查书目记录原来的创建者 998$z
                if (bOwnerOnly)
                {
                    string strOwner = "";

                    // 获得书目记录的创建者
                    // return:
                    //      -1  出错
                    //      0   没有找到 998$z子字段
                    //      1   找到
                    nRet = GetBiblioOwner(strExistingXml,
                        out strOwner,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (strOwner != sessioninfo.UserID)
                    {
                        strError = $"当前用户 '{sessioninfo.UserID}' 不是{strDbTypeCaption}记录 '{strBiblioRecPath}' 的创建者(998$z) '{strOwner}'，因此 setbiblio(authority)info {strAction} 操作被拒绝";
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // TODO: 如果已存在的XML记录中，MARC根不是文档根，那么表明书目记录
                // 还存储有其他信息，这时就需要把前端送来的XML记录和已存在的记录进行合并处理，
                // 防止贸然覆盖了文档根下的有用信息。
            }

        SKIP_MEMO_OLDRECORD:

            bool bBiblioNotFound = false;

            string strRights = "";

            if (sessioninfo.Account != null)
                strRights = sessioninfo.Account.Rights;

            // 2017/1/2
            if (strAction == "checkunique")
            {
                if (string.IsNullOrEmpty(strExistingXml) == false)
                {
                    if (string.IsNullOrEmpty(strBiblio) == true)
                        strBiblio = strExistingXml;
                    else
                    {
                        // 合并联合编目的新旧书目库XML记录
                        // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                        // parameters:
                        //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                        // return:
                        //      -1  error
                        //      0   not delete any fields
                        //      1   deleted some fields
                        nRet = MergeOldNewBiblioRec(
                            strRights,
                            strUnionCatalogStyle,
                            strLibraryCode,
                            "insert,replace,delete",
                            strAccessParameters,
                            strChangeableFieldNameList,
                            strExistingXml,
                            ref strBiblio,
                            ref bChangePartDenied,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                            strDeniedComment += " " + strError;
                    }
                }
                else
                {
                    strExistingXml = "";
                    // 合并联合编目的新旧书目库XML记录
                    // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                    // parameters:
                    //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                    // return:
                    //      -1  error
                    //      0   not delete any fields
                    //      1   deleted some fields
                    nRet = MergeOldNewBiblioRec(
                        strRights,
                        strUnionCatalogStyle,
                        strLibraryCode,
                        "insert",
                        strAccessParameters,
                        strChangeableFieldNameList,
                        strExistingXml,
                        ref strBiblio,
                        ref bChangePartDenied,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                        strDeniedComment += " " + strError;

                }

                // return:
                //      -1  出错
                //      0   没有命中
                //      >0  命中条数。此时 strError 中返回发生重复的路径列表
                nRet = SearchBiblioDup(
                    sessioninfo,
                    strBiblioRecPath,
                    strBiblio,
                    "setbiblio", // strResultSetName,
                    null,
                    out string error_code,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet > 0)
                {
                    strOutputBiblioRecPath = strError;
                    result.Value = -1;
                    result.ErrorInfo = "经查重发现书目库中已有 " + nRet.ToString() + " 条重复记录(" + strOutputBiblioRecPath + ")。";
                    result.ErrorCode = ErrorCode.BiblioDup;
                    return result;
                }

                if (strAction == "checkunique")
                {
                    if (nRet == 0 && string.IsNullOrEmpty(error_code) == false)
                    {
                        result.Value = -1;
                        if (error_code == "notInUniqueSpace")
                            result.ErrorInfo = $"发起记录 {strBiblioRecPath} 没有处在查重空间内，无法进行唯一性检查";
                        else if (error_code == "undefined")
                            result.ErrorInfo = "library.xml 中尚未定义查重空间参数。因此无法进行唯一性检查";
                        else
                            result.ErrorInfo = error_code;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
                    result.Value = 0;   // 没有发现重复
                    return result;
                }
            }

            if (strAction == "new")
            {
                // 对order权限的判断。order权限允许对任何库进行new操作

                // TODO: 不只是联合编目模块要进行记录预处理。
                // 也要结合当前用户是不是具有 setobject 权限，进行判断和处理。
                // 如果但前用户不具备 setobject 权限，则也不应在XML中包含任何<dprms:file>元素(如果包含了，则处理为出错或者警告(这会增加前端的负担)？还是忽略后写入？)

                {
                    /*
                    // 对strBiblio中内容进行加工，确保905字段符合联合编目要求

                    // 准备联合编目的新书目库XML记录
                    // 功能：排除strLibraryCode定义以外的905字段
                    // return:
                    //      -1  error
                    //      0   not delete any fields
                    //      1   deleted some fields
                    nRet = PrepareNewBiblioRec(
                        strLibraryCode,
                        ref strBiblio,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                     * */

                    strExistingXml = "";
                    // 合并联合编目的新旧书目库XML记录
                    // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                    // parameters:
                    //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                    // return:
                    //      -1  error
                    //      0   not delete any fields
                    //      1   deleted some fields
                    nRet = MergeOldNewBiblioRec(
                        strRights,
                        strUnionCatalogStyle,
                        strLibraryCode,
                        "insert",
                        strAccessParameters,
                        strChangeableFieldNameList,
                        strExistingXml,
                        ref strBiblio,
                        ref bChangePartDenied,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                        strDeniedComment += " " + strError;
                }

                if (bSimulate == false)
                {
                    // return:
                    //      -1  出错
                    //      0   没有命中
                    //      >0  命中条数。此时 strError 中返回发生重复的路径列表
                    nRet = SearchBiblioDup(
                        sessioninfo,
                        strBiblioRecPath,
                        strBiblio,
                        "setbiblio", // strResultSetName,
                        null,
                        out string error_code,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet > 0)
                    {
                        strOutputBiblioRecPath = strError;
                        result.Value = -1;
                        result.ErrorInfo = "经查重发现" + strDbTypeCaption + "中已有 " + nRet.ToString() + " 条重复记录(" + strOutputBiblioRecPath + ")。";
                        if (strAction != "checkunique")
                            result.ErrorInfo += "本次保存操作被拒绝";
                        result.ErrorCode = ErrorCode.BiblioDup;
                        return result;
                    }

                    if (strAction == "checkunique")
                    {
                        // 2022/1/28
                        if (nRet == 0
                            && string.IsNullOrEmpty(error_code) == false)
                        {
                            result.Value = -1;
                            if (error_code == "notInUniqueSpace")
                                result.ErrorInfo = $"发起记录 {strBiblioRecPath} 没有处在查重空间内，无法进行唯一性检查";
                            else if (error_code == "undefined")
                                result.ErrorInfo = "library.xml 中尚未定义查重空间参数。因此无法进行唯一性检查";
                            else
                                result.ErrorInfo = error_code;
                            result.ErrorCode = ErrorCode.SystemError;
                            return result;
                        }
                        result.Value = 0;   // 没有发现重复
                        return result;
                    }
                }

                // ?

                // 2009/11/2 
                // 需要判断路径最后一级是否为问号？
                string strTargetRecId = ResPath.GetRecordId(strBiblioRecPath);
                if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                {
                    if (String.IsNullOrEmpty(strTargetRecId) == true)
                        strBiblioRecPath = ResPath.GetDbName(strBiblioRecPath) + "/?";  // 这样做主要是防范输入 "中文图书/" 这样的字符串造成 "中文图书//?" 的结果 2016/11/24
                }
                else
                {
                    /*
                    strError = "当创建书目记录的时候，只能使用“书目库名/?”形式的路径(而不能使用 '"+strBiblioRecPath+"' 形式)。如果要在指定位置保存，可使用修改(change)子功能";
                    goto ERROR1;
                     * */
                }

                // 2011/11/30
                nRet = this.ClearOperation(
                    ref strBiblio,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                nRet = this.SetOperation(
ref strBiblio,
"create",
sessioninfo.UserID,
"",
true,
10,
out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                if (bSimulate)
                {
                    // 模拟创建新记录的操作
                    baOutputTimestamp = null;
                    strOutputBiblioRecPath = strBiblioRecPath;  // 路径中 ID 依然为问号，没有被处理
                }
                else
#endif

                {
                    lRet = channel.DoSaveTextRes(strBiblioRecPath,
                        strBiblio,
                        false,
                        "content" + (bSimulate ? ",simulate" : ""),
                        baTimestamp,
                        out baOutputTimestamp,
                        out strOutputBiblioRecPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (this.TestMode == true || sessioninfo.TestMode)
                    {
                        string strID = ResPath.GetRecordId(strOutputBiblioRecPath);
                        if (StringUtil.IsPureNumber(strID) == true)
                        {
                            long v = 0;
                            long.TryParse(strID, out v);
                            if (v > 1000)
                            {
                                strError = "评估模式下只能修改 ID 小于等于 1000 的书目记录。本记录 " + strOutputBiblioRecPath + " 虽然创建成功，但以后无法对其进行修改 ";
                                goto ERROR1;
                            }
                        }
                    }
                }
            }
            else if (strAction == "change")
            {
                if (strDbType == "biblio")
                {
                    // 2020/6/3
                    // 判断一下是否有存取定义的 setbiblioinfo 权限
                    bool has_rights = false;
                    if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                    {
                        // return:
                        //      null    指定的操作类型的权限没有定义
                        //      ""      定义了指定类型的操作权限，但是否定的定义
                        //      其它      权限列表。* 表示通配的权限列表
                        string strAccessActionList = GetDbOperRights(sessioninfo.Access,
                            strBiblioDbName,
                            strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo");

                        if (strAccessActionList == "*" || IsInAccessList(strAction, strAccessActionList, out string _) == true)
                            has_rights = true;
                    }

                    // 只有order权限的情况
                    if ((has_rights == false && StringUtil.IsInList("setbiblioinfo", sessioninfo.RightsOrigin) == false)
                    && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
                    {
                        // 工作库允许全部操作，非工作库只能追加记录
                        if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                        {
                            // 非工作库。要求原来记录不存在
                            if (String.IsNullOrEmpty(strExistingXml) == false)
                            {
                                strError = "当前帐户只有 order 权限而没有 setbiblioinfo 权限，不能用 change 功能修改已经存在的(位于非工作库中的)书目记录 '" + strBiblioRecPath + "'";
                                goto ERROR1;
                            }
                        }
                    }
                }

                {
                    // 合并联合编目的新旧书目库XML记录
                    // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                    // parameters:
                    //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                    // return:
                    //      -1  error
                    //      0   not delete any fields
                    //      1   deleted some fields
                    nRet = MergeOldNewBiblioRec(
                        strRights,
                        strUnionCatalogStyle,
                        strLibraryCode,
                        "insert,replace,delete",
                        strAccessParameters,
                        strChangeableFieldNameList,
                        strExistingXml,
                        ref strBiblio,
                        ref bChangePartDenied,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                        strDeniedComment += " " + strError;
                }

                if (bSimulate == false)
                {
                    // return:
                    //      -1  出错
                    //      0   没有命中
                    //      >0  命中条数。此时 strError 中返回发生重复的路径列表
                    nRet = SearchBiblioDup(
                        sessioninfo,
                        strBiblioRecPath,
                        strBiblio,
                        "setbiblio", // strResultSetName,
                        null,
                        out _,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = $"针对书目记录进行自动查重时出错: {strError}";
                        goto ERROR1;
                    }
                    if (nRet > 0)
                    {
                        strOutputBiblioRecPath = strError;
                        result.Value = -1;
                        result.ErrorInfo = "经查重发现书目库中已有 " + nRet.ToString() + " 条重复记录(" + strOutputBiblioRecPath + ")。本次保存操作被拒绝";
                        result.ErrorCode = ErrorCode.BiblioDup;
                        return result;
                    }
                }

                // 2011/11/30
                nRet = this.SetOperation(
ref strBiblio,
"change",
sessioninfo.UserID,
"",
true,
10,
out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                if (bSimulate)
                {
                    // 模拟修改记录的操作
                    baOutputTimestamp = null;
                    strOutputBiblioRecPath = strBiblioRecPath;
                }
                else
#endif
                // 保存修改前的书目记录
                if (StringUtil.IsInList("bibliotoitem", strStyle))
                {
                    // 为册记录添加书目信息
                    // return:
                    //      -1  失败
                    //      0   成功
                    //      1   需要结束运行，result 结果已经设置好了
                    nRet = AddBiblioToSubRecords(
                        sessioninfo,
                        strBiblioRecPath,
                        strExistingXml,
                        ref domOperLog,
                        ref result,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        return result;
                }

                {
                    // 需要判断路径是否为具备最末一级索引号的形式？

                    this.BiblioLocks.LockForWrite(strBiblioRecPath);

                    try
                    {
                        lRet = channel.DoSaveTextRes(strBiblioRecPath,
                            strBiblio,
                            false,
                            "content" + (bSimulate ? ",simulate" : ""),
                            baTimestamp,
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.TimestampMismatch;
                                return result;
                            }
                            goto ERROR1;
                        }
                    }
                    finally
                    {
                        this.BiblioLocks.UnlockForWrite(strBiblioRecPath);
                    }

                    this.DeleteBiblioSummary(strBiblioRecPath);
                }
            }
            else if (strAction == "delete"
                || strAction == "onlydeletesubrecord"
                || strAction == "onlydeletebiblio") // 2023/2/21 移动到这里
            {
                if (strDbType == "biblio")
                {
                    // 2020/6/3
                    // 判断一下是否有存取定义的 setbiblioinfo 权限
                    bool has_rights = false;
                    if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                    {
                        // return:
                        //      null    指定的操作类型的权限没有定义
                        //      ""      定义了指定类型的操作权限，但是否定的定义
                        //      其它      权限列表。* 表示通配的权限列表
                        string strAccessActionList = GetDbOperRights(sessioninfo.Access,
                            strBiblioDbName,
                            strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo");

                        if (strAccessActionList == "*" || IsInAccessList(strAction, strAccessActionList, out string _) == true)
                            has_rights = true;
                    }

                    // 只有order权限的情况
                    if ((has_rights == false && StringUtil.IsInList("setbiblioinfo", sessioninfo.RightsOrigin) == false)
                        && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
                    {
                        // 工作库允许全部操作，非工作库不能删除记录
                        if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                        {
                            // 非工作库。要求原来记录不存在
                            strError = "当前帐户只有 order 权限而没有 setbiblioinfo 权限，不能用 delete 功能删除(位于非工作库中的)书目记录 '" + strBiblioRecPath + "'";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                if (strAction == "delete"
                    || strAction == "onlydeletebiblio")
                {
                    strBiblio = "";

                    // 合并联合编目的新旧书目库XML记录
                    // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                    // parameters:
                    //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                    // return:
                    //      -1  error
                    //      0   not delete any fields
                    //      1   deleted some fields
                    nRet = MergeOldNewBiblioRec(
                        strRights,
                        strUnionCatalogStyle,
                        strLibraryCode,
                        "delete",
                        strAccessParameters,
                        strChangeableFieldNameList,
                        strExistingXml,
                        ref strBiblio,
                        ref bChangePartDenied,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                        strDeniedComment += " " + strError;

                    // 检查根下面是不是没有任何元素了。如果还有，说明当前权限不足以删除它们。
                    // 如果已经为空，就表示不必检查了
                    if (String.IsNullOrEmpty(strBiblio) == false)
                    {
#if REMOVED
                        XmlDocument tempdom = new XmlDocument();
                        try
                        {
                            tempdom.LoadXml(strBiblio);
                        }
                        catch (Exception ex)
                        {
                            strError = "经过 MergeOldNewBiblioRec() 处理后的 strBiblio 装入 XmlDocument 失败: " + ex.Message;
                            goto ERROR1;
                        }

                        // 2011/11/30
                        // 删除全部<operations>元素
                        XmlNodeList nodes = tempdom.DocumentElement.SelectNodes("operations");
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            XmlNode node = nodes[i];
                            if (node.ParentNode != null)
                                node.ParentNode.RemoveChild(node);
                        }

                        // 删除空的 header 元素
                        {
                            XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
                            mngr.AddNamespace("dprms", DpNs.dprms);

                            mngr.AddNamespace("usmarc", Ns.usmarcxml);  // "http://www.loc.gov/MARC21/slim"
                            mngr.AddNamespace("unimarc", DpNs.unimarcxml);	// "http://dp2003.com/UNIMARC"

                            var headers = tempdom.DocumentElement.SelectNodes("//unimarc:header | //usmarc:header");
                            foreach (XmlElement header in headers)
                            {
                                if (header.InnerText.Trim() == "????????????????????????")
                                    header.ParentNode.RemoveChild(header);
                            }
                        }
#endif
                        // 移除 MARCXML 中的工作用元素和字段
                        // return:
                        //      -1  出错
                        //      0   没有余下的字段
                        //      1   有余下的字段，余下的字段返回在 strOutputBiblio 中
                        nRet = RemoveWorkingFields(
                            strBiblio,
                            out List<string> rest_names,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (rest_names.Count > 0)
                        {
                            // 2017/5/5
                            this.WriteErrorLog("用户 '" + sessioninfo.UserID + "' 删除书目记录 '" + strBiblioRecPath + "' (已被拒绝) 最后一步剩下的字段: " + StringUtil.MakePathList(rest_names));

                            result.Value = -1;
                            result.ErrorInfo = "当前用户的权限不足以删除所有MARC字段，因此删除操作被拒绝。可改用修改操作。\r\n\r\n权限不足以删除的部分字段如下: \r\n" + StringUtil.MakePathList(rest_names);  // TODO: 可以考虑用更友好的显示 MARC 工作单格式的方式来报错
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                // 注：这里的模拟删除不是太容易模拟。
                // 因为真正删除的时候，是根据实际存在的下属记录的类型来检查权限的。也许模拟删除可以根据假定每种下属记录都存在的情况来检查权限。但这样往往是比实际情况要偏严的。如果有其他参数，能指出调用者关注哪些下属记录的类型就好了
                if (bSimulate == true)
                {
                    //strError = "尚未实现对 '"+strAction+"' 的模拟操作";
                    //goto ERROR1;
                    // 模拟删除记录的操作
                    baOutputTimestamp = null;
                    strOutputBiblioRecPath = strBiblioRecPath;
                    goto END1;
                }

                if (strDbType == "biblio")
                {
                    if (strAction == "delete"
                        || strAction == "onlydeletesubrecord")
                    {
                        // 删除书目记录的下级记录
                        // return:
                        //      -1  失败
                        //      0   成功
                        //      1   需要结束运行，result 结果已经设置好了
                        nRet = DeleteBiblioAndSubRecords(
                    sessioninfo,
                    strAction,
                    strBiblioRecPath,
                    strExistingXml,
                    strStyle,
                    baTimestamp,
                    ref bBiblioNotFound,
                    ref strBiblio,
                    ref baOutputTimestamp,
                    ref domOperLog,
                    ref result,
                    out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                            return result;
                    }
                    if (strAction == "onlydeletebiblio")
                    {
                        // 不需要同时删除下属的实体记录
                        this.BiblioLocks.LockForWrite(strBiblioRecPath);
                        try
                        {
                            baOutputTimestamp = null;

                            // 删除书目记录
                            lRet = channel.DoDeleteRes(strBiblioRecPath,
                                baTimestamp,
                                out baOutputTimestamp,
                                out strError);
                            if (lRet == -1)
                            {
                                // 只删除书目记录，但是如果书目记录却不存在，要报错
                                goto ERROR1;
                            }
                        }
                        finally
                        {
                            this.BiblioLocks.UnlockForWrite(strBiblioRecPath);
                        }

                        this.DeleteBiblioSummary(strBiblioRecPath);
                    }
                }
            }
#if REMOVED
            else if (strAction == "onlydeletebiblio")
            {
                if (strDbType == "biblio")
                {
                    // 2020/6/3
                    // 判断一下是否有存取定义的 setbiblioinfo 权限
                    bool has_rights = false;
                    if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                    {
                        // return:
                        //      null    指定的操作类型的权限没有定义
                        //      ""      定义了指定类型的操作权限，但是否定的定义
                        //      其它      权限列表。* 表示通配的权限列表
                        string strAccessActionList = GetDbOperRights(sessioninfo.Access,
                            strBiblioDbName,
                            strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo");

                        if (strAccessActionList == "*" || IsInAccessList(strAction, strAccessActionList, out string _) == true)
                            has_rights = true;
                    }

                    // 只有order权限的情况
                    if ((has_rights == false && StringUtil.IsInList("setbiblioinfo", sessioninfo.RightsOrigin) == false)
                        && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
                    {
                        // 工作库允许全部操作，非工作库不能删除记录
                        if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                        {
                            // 非工作库。要求原来记录不存在
                            strError = "当前帐户只有 order 权限而没有 setbiblioinfo 权限，不能用 onlydeletebiblio 功能删除(位于非工作库中的)书目记录 '" + strBiblioRecPath + "'";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                {
                    strBiblio = "";

                    // 合并联合编目的新旧书目库XML记录
                    // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                    // parameters:
                    //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                    // return:
                    //      -1  error
                    //      0   not delete any fields
                    //      1   deleted some fields
                    nRet = MergeOldNewBiblioRec(
                        strRights,
                        strUnionCatalogStyle,
                        strLibraryCode,
                        "delete",
                        strAccessParameters,
                        strChangeableFieldNameList,
                        strExistingXml,
                        ref strBiblio,
                        ref bChangePartDenied,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                    {
                        strDeniedComment += " " + strError;
                    }

                    // 检查根下面是不是没有任何元素了。如果还有，说明当前权限不足以删除它们。
                    // 如果已经为空，就表示不必检查了
                    if (String.IsNullOrEmpty(strBiblio) == false)
                    {
                        XmlDocument tempdom = new XmlDocument();
                        try
                        {
                            tempdom.LoadXml(strBiblio);
                        }
                        catch (Exception ex)
                        {
                            strError = "经过 MergeOldNewBiblioRec() 处理后的 strBiblio 装入 XmlDocument 失败: " + ex.Message;
                            goto ERROR1;
                        }

                        // 2011/12/9
                        // 删除全部<operations>元素
                        XmlNodeList nodes = tempdom.DocumentElement.SelectNodes("operations");
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            XmlNode node = nodes[i];
                            if (node.ParentNode != null)
                                node.ParentNode.RemoveChild(node);
                        }

                        if (tempdom.DocumentElement.ChildNodes.Count != 0)
                        {
                            // 2017/5/5
                            this.WriteErrorLog("用户 '" + sessioninfo.UserID + "' 删除书目记录 '" + strBiblioRecPath + "' (已被拒绝) 最后一步剩下的记录 XML 内容 '" + tempdom.OuterXml + "'");

                            result.Value = -1;
                            result.ErrorInfo = "当前用户的权限不足以删除所有MARC字段，因此删除操作被拒绝。可改用修改操作。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                if (bSimulate)
                {
                    baOutputTimestamp = null;
                }
                else
                {
                    // 不需要同时删除下属的实体记录
                    this.BiblioLocks.LockForWrite(strBiblioRecPath);
                    try
                    {
                        baOutputTimestamp = null;

                        // 删除书目记录
                        lRet = channel.DoDeleteRes(strBiblioRecPath,
                            baTimestamp,
                            out baOutputTimestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            // 只删除书目记录，但是如果书目记录却不存在，要报错
                            goto ERROR1;
                        }
                    }
                    finally
                    {
                        this.BiblioLocks.UnlockForWrite(strBiblioRecPath);
                    }

                    this.DeleteBiblioSummary(strBiblioRecPath);
                }
            }
#endif
            else
            {
                strError = "未知的strAction参数值 '" + strAction + "'";
                goto ERROR1;
            }

        END1:
            if (bSimulate == false && bNoEventLog == false)
            {
                if (string.IsNullOrEmpty(strOutputBiblioRecPath) == false)
                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "record", strBiblio);
                    DomUtil.SetAttr(node, "recPath", strOutputBiblioRecPath);
                }

                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    sessioninfo.UserID);
                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTime);

                // 写入日志
                nRet = this.OperLog.WriteOperLog(domOperLog,
                    sessioninfo.ClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "SetBiblioInfo() API 写入日志时发生错误: " + strError;
                    goto ERROR1;
                }
            }

            result.Value = 0;
            if (bBiblioNotFound == true)
                result.ErrorInfo = "虽然书目记录 '" + strBiblioRecPath + "' 不存在，但是删除下属的实体记录成功。";  // 虽然...但是...
            // 2013/3/5
            if (bChangePartDenied == true)
            {
                result.ErrorCode = ErrorCode.PartialDenied;
                if (string.IsNullOrEmpty(strDeniedComment) == false)
                {
                    if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                        result.ErrorInfo += " ; ";
                    result.ErrorInfo += strDeniedComment;
                }
            }
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            if (result.ErrorCode == ErrorCode.NoError)
                result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 移除 MARCXML 中的工作用元素和字段
        // return:
        //      -1  出错
        //      0   没有余下的字段
        //      1   有余下的字段，余下的字段返回在 strOutputBiblio 中
        int RemoveWorkingFields(
            string strBiblio,
            out List<string> output_field_names,
            out string strError)
        {
            strError = "";
            output_field_names = new List<string>();

            if (String.IsNullOrEmpty(strBiblio) == true)
                return 0;

            XmlDocument tempdom = new XmlDocument();
            try
            {
                tempdom.LoadXml(strBiblio);
            }
            catch (Exception ex)
            {
                strError = "经过 MergeOldNewBiblioRec() 处理后的 strBiblio 装入 XmlDocument 失败: " + ex.Message;
                return -1;
            }

            // 2011/11/30
            // 删除全部<operations>元素
            XmlNodeList nodes = tempdom.DocumentElement.SelectNodes("operations");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node.ParentNode != null)
                    node.ParentNode.RemoveChild(node);
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);  // "http://www.loc.gov/MARC21/slim"
            nsmgr.AddNamespace("unimarc", DpNs.unimarcxml);  // "http://dp2003.com/UNIMARC"

            // 删除空的 header 元素
            {

                var headers = tempdom.DocumentElement.SelectNodes("//unimarc:leader | //usmarc:leader", nsmgr);
                foreach (XmlElement header in headers)
                {
                    if (header.InnerText.Trim() == "????????????????????????")
                        header.ParentNode.RemoveChild(header);
                }
            }

            if (tempdom.DocumentElement.ChildNodes.Count != 0)
            {
                // strOutputBiblio = tempdom.OuterXml;

                // 头标区
                var headers = tempdom.DocumentElement.SelectNodes("//unimarc:leader | //usmarc:leader", nsmgr);
                if (headers.Count > 0)
                    output_field_names.Add("###");

                // 列出 MARC 字段名
                var names = tempdom.DocumentElement.SelectNodes("//*/@tag");
                foreach (XmlAttribute attr in names)
                {
                    output_field_names.Add(attr.Value);
                }

                // dprms:file 元素
                var files = tempdom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                if (files.Count > 0)
                    output_field_names.Add(GetMyName(files[0] as XmlElement));

                return 1;
            }

            return 0;
        }

        // 删除书目记录的下级记录
        // return:
        //      -1  失败
        //      0   成功
        //      1   需要结束运行，result 结果已经设置好了
        int DeleteBiblioAndSubRecords(
            SessionInfo sessioninfo,
            string strAction,
            string strBiblioRecPath,
            string strExistingXml,
            string strStyleParam,
            byte[] baTimestamp,
            ref bool bBiblioNotFound,
            ref string strBiblio,
            ref byte[] baOutputTimestamp,
            ref XmlDocument domOperLog,
            ref LibraryServerResult result,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            bool bWhenChildEmpty = StringUtil.IsInList("whenChildEmpty", strStyleParam);     // 是否仅当没有子记录时才删除书目记录？2020/10/27

            // 这个删除不是那么简单，需要同时删除下属的实体记录
            // 要对种和实体都进行锁定
            this.BiblioLocks.LockForWrite(strBiblioRecPath);
            try
            {
                // 探测书目记录有没有下属的实体记录(也顺便看看实体记录里面是否有流通信息，并检查 dprms:file 元素是否存在)?
                string strDetectStyle = "check_borrow_info";
                long lHitCount = 0;

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "channel == null";
                    goto ERROR1;
                }

                // return:
                //      -2  not exist entity dbname
                //      -1  error
                //      >=0 含有流通信息的实体记录个数
                nRet = SearchChildEntities(
                    sessioninfo,
                    channel,
                    strBiblioRecPath,
                    strDetectStyle,
                    // sessioninfo.GlobalUser == false ? CheckItemRecord : (Delegate_checkRecord)null,
                    CheckItemRecord,
                    sessioninfo.GlobalUser == false ? sessioninfo.LibraryCodeList : null,
                    out lHitCount,
                    out List<DeleteEntityInfo> entityinfos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == -2)
                {
                    Debug.Assert(entityinfos.Count == 0, "");
                }

                // 如果有实体记录，则要求 setiteminfo 权限，才能一同删除实体们
                if (entityinfos != null && entityinfos.Count > 0)
                {
                    // 2020/10/27
                    if (bWhenChildEmpty)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作(带有 whenChildEmpty 条件)被拒绝。因拟删除的书目记录带有下属的实体记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    // 权限字符串
                    if (StringUtil.IsInList("setiteminfo,writerecord", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的实体记录，但当前用户不具备 setiteminfo 或 writerecord 权限，不能删除它们。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    if (this.DeleteBiblioSubRecords == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的实体记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    // bFoundEntities = true;
                }

                // 探测书目记录有没有下属的订购记录
                // return:
                //      -1  error
                //      0   not exist entity dbname
                //      1   exist entity dbname
                nRet = this.OrderItemDatabase.SearchChildItems(
                    sessioninfo,
                    channel,
                    strBiblioRecPath,
                    "check_circulation_info", // 在DeleteEntityInfo结构中*不*返回OldRecord内容
                    CheckItemRecordDprmsFile,
                    "order",
                    out lHitCount,
                    out List<DeleteEntityInfo> orderinfos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    Debug.Assert(orderinfos.Count == 0, "");
                }

                // 如果有订购记录，则要求 setorderinfo 权限，才能一同删除它们
                if (orderinfos != null && orderinfos.Count > 0)
                {
                    // 2020/10/27
                    if (bWhenChildEmpty)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作(带有 whenChildEmpty 条件)被拒绝。因拟删除的书目记录带有下属的订购记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return 1;
                    }

                    // 权限字符串
                    if (StringUtil.IsInList("setorderinfo,writerecord,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的订购记录，但当前用户不具备 setorderinfo 或 writerecord 或 order 权限，不能删除它们。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    if (this.DeleteBiblioSubRecords == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的订购记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    // bFoundOrders = true;
                }

                //
                // 探测书目记录有没有下属的期记录
                // return:
                //      -1  error
                //      0   not exist entity dbname
                //      1   exist entity dbname
                nRet = this.IssueItemDatabase.SearchChildItems(
                    sessioninfo,
                    channel,
                    strBiblioRecPath,
                    "check_circulation_info", // 在DeleteEntityInfo结构中*不*返回OldRecord内容
                    CheckItemRecordDprmsFile,
                    "issue",
                    out lHitCount,
                    out List<DeleteEntityInfo> issueinfos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    Debug.Assert(issueinfos.Count == 0, "");
                }

                // 如果有期记录，则要求 setissueinfo 权限，才能一同删除它们
                if (issueinfos != null && issueinfos.Count > 0)
                {
                    // 2020/10/27
                    if (bWhenChildEmpty)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作(带有 whenChildEmpty 条件)被拒绝。因拟删除的书目记录带有下属的期记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return 1;
                    }

                    // 权限字符串
                    if (StringUtil.IsInList("setissueinfo,writerecord", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的期记录，但当前用户不具备 setissueinfo 或 writerecord 权限，不能删除它们。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    if (this.DeleteBiblioSubRecords == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的期记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }
                    // bFoundIssues = true;
                }

                // 探测书目记录有没有下属的评注记录
                // return:
                //      -1  error
                //      0   not exist entity dbname
                //      1   exist entity dbname
                nRet = this.CommentItemDatabase.SearchChildItems(
                    sessioninfo,
                    channel,
                    strBiblioRecPath,
                    "check_circulation_info", // 在DeleteEntityInfo结构中*不*返回OldRecord内容
                    CheckItemRecordDprmsFile,
                    "comment",
                    out lHitCount,
                    out List<DeleteEntityInfo> commentinfos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    Debug.Assert(commentinfos.Count == 0, "");
                }

                // 如果有评注记录，则要求setcommentinfo权限，才能一同删除它们
                if (commentinfos != null && commentinfos.Count > 0)
                {
                    // 2020/10/27
                    if (bWhenChildEmpty)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作(带有 whenChildEmpty 条件)被拒绝。因拟删除的书目记录带有下属的评注记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return 1;
                    }

                    // 权限字符串
                    if (StringUtil.IsInList("setcommentinfo,writerecord", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的评注记录，但当前用户不具备setcommentinfo 或 writerecord 权限，不能删除它们。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }

                    if (this.DeleteBiblioSubRecords == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "设置书目信息的删除(delete)操作被拒绝。因拟删除的书目记录带有下属的评注记录，不允许删除书目记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }
                }

                baOutputTimestamp = null;

                if (strAction == "delete")
                {
                    // 删除书目记录
                    lRet = channel.DoDeleteRes(strBiblioRecPath,
                        baTimestamp,
                        out baOutputTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        // 2023/2/21
                        ConvertKernelErrorCode(channel.ErrorCode, ref result);

                        if (channel.IsNotFound()
                            && (entityinfos.Count > 0 || orderinfos.Count > 0 || issueinfos.Count > 0)
                            )
                        {
                            bBiblioNotFound = true;
                            // strWarning = "书目记录 '" + strBiblioRecPath + "' 不存在";
                        }
                        else
                            goto ERROR1;
                    }

                    this.DeleteBiblioSummary(strBiblioRecPath);
                }

                strBiblio = ""; // 以免后面把残余信息写入操作日志的 <record>元素 2013/3/11
                baOutputTimestamp = null;

                // 删除属于同一书目记录的全部实体记录
                // 这是需要提供EntityInfo数组的版本
                // return:
                //      -1  error
                //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                //      >0  实际删除的实体记录数
                nRet = DeleteBiblioChildEntities(channel,
                    entityinfos,
                    domOperLog,
                    out strError);
                if (nRet == -1 && bBiblioNotFound == false)
                {
                    // TODO: 当书目记录中有对象资源时，DoSaveTextRes就无法恢复了

                    // 重新保存回去书目记录, 以便还有下次重试删除的机会
                    // 因此需要注意，前端在删除失败后，不要忘记了更新timestamp
                    if (strAction == "delete")
                    {
                        string strError_1 = "";
                        string strOutputBiblioRecPath = "";
                        lRet = channel.DoSaveTextRes(strBiblioRecPath,
                            strExistingXml,
                            false,
                            "content", // ,ignorechecktimestamp
                            null,   // timestamp
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError_1);
                        if (lRet == -1)
                        {
                            strError = "删除下级实体记录失败: " + strError + "；\r\n并且试图重新写回刚刚已删除的书目记录 '" + strBiblioRecPath + "' 的操作也发生了错误: " + strError_1;
                            goto ERROR1;
                        }
                    }

                    goto ERROR1;
                }

                // return:
                //      -1  error
                //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                //      >0  实际删除的实体记录数
                nRet = this.OrderItemDatabase.DeleteBiblioChildItems(
                    // sessioninfo.Channels,
                    channel,
                    orderinfos,
                    domOperLog,
                    out strError);
                if (nRet == -1 && bBiblioNotFound == false)
                {
                    // 重新保存回去书目记录, 以便还有下次重试删除的机会
                    // 因此需要注意，前端在删除失败后，不要忘记了更新timestamp
                    try
                    {
                        string strError_1 = "";
                        string strOutputBiblioRecPath = "";
                        lRet = channel.DoSaveTextRes(strBiblioRecPath,
                            strExistingXml,
                            false,
                            "content", // ,ignorechecktimestamp
                            null,   // timestamp
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError_1);
                        if (lRet == -1)
                        {
                            strError = "删除下级订购记录失败: " + strError + "；\r\n并且试图重新写回刚刚已删除的书目记录 '" + strBiblioRecPath + "' 的操作也发生了错误: " + strError_1;
                            goto ERROR1;
                        }
                        goto ERROR1;
                    }
                    finally
                    {
                        if (entityinfos.Count > 0)
                            strError += "；\r\n刚删除的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    }
                }

                // return:
                //      -1  error
                //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                //      >0  实际删除的实体记录数
                nRet = this.IssueItemDatabase.DeleteBiblioChildItems(
                    // sessioninfo.Channels,
                    channel,
                    issueinfos,
                    domOperLog,
                    out strError);
                if (nRet == -1 && bBiblioNotFound == false)
                {
                    // 重新保存回去书目记录, 以便还有下次重试删除的机会
                    // 因此需要注意，前端在删除失败后，不要忘记了更新timestamp
                    try
                    {
                        string strError_1 = "";
                        string strOutputBiblioRecPath = "";
                        lRet = channel.DoSaveTextRes(strBiblioRecPath,
                            strExistingXml,
                            false,
                            "content", // ,ignorechecktimestamp
                            null,   // timestamp
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError_1);
                        if (lRet == -1)
                        {
                            strError = "删除下级期记录失败: " + strError + "；\r\n并且试图重新写回刚刚已删除的书目记录 '" + strBiblioRecPath + "' 的操作也发生了错误: " + strError_1;
                            goto ERROR1;
                        }
                        goto ERROR1;
                    }
                    finally
                    {
                        if (entityinfos.Count > 0)
                            strError += "；\r\n刚删除的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                        if (orderinfos.Count > 0)
                            strError += "；\r\n刚删除的 " + orderinfos.Count.ToString() + " 个订购记录已经无法恢复";
                    }
                }

                // return:
                //      -1  error
                //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                //      >0  实际删除的实体记录数
                nRet = this.CommentItemDatabase.DeleteBiblioChildItems(
                    // sessioninfo.Channels,
                    channel,
                    commentinfos,
                    domOperLog,
                    out strError);
                if (nRet == -1 && bBiblioNotFound == false)
                {
                    // 重新保存回去书目记录, 以便还有下次重试删除的机会
                    // 因此需要注意，前端在删除失败后，不要忘记了更新timestamp
                    try
                    {
                        string strError_1 = "";
                        string strOutputBiblioRecPath = "";
                        lRet = channel.DoSaveTextRes(strBiblioRecPath,
                            strExistingXml,
                            false,
                            "content", // ,ignorechecktimestamp
                            null,   // timestamp
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError_1);
                        if (lRet == -1)
                        {
                            strError = "删除下级评注记录失败: " + strError + "；\r\n并且试图重新写回刚刚已删除的书目记录 '" + strBiblioRecPath + "' 的操作也发生了错误: " + strError_1;
                            goto ERROR1;
                        }
                        goto ERROR1;
                    }
                    finally
                    {
                        if (entityinfos.Count > 0)
                            strError += "；\r\n刚删除的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                        if (orderinfos.Count > 0)
                            strError += "；\r\n刚删除的 " + orderinfos.Count.ToString() + " 个订购记录已经无法恢复";
                        if (issueinfos.Count > 0)
                            strError += "；\r\n刚删除的 " + issueinfos.Count.ToString() + " 个期记录已经无法恢复";
                    }
                }
            }
            finally
            {
                this.BiblioLocks.UnlockForWrite(strBiblioRecPath);
            }
            return 0;
        ERROR1:
            return -1;
        }

        // 检查订购、期、评注记录是否适合进行删除和移动
        // 算法: 如果记录包含 dprms:file 元素，当前账户是否具备 setxxxobject 权限
        // 如果返回值不是0，就中断循环并返回
        int CheckItemRecordDprmsFile(
            SessionInfo sessioninfo,
            int index,
            string strRecPath,
            XmlDocument dom,
            byte[] baTimestamp,
            object param,
            out string strError)
        {
            strError = "";

            // 2023/3/9
            // 检查 dprms:file 元素是否存在
            if (sessioninfo != null)
            {
                /*
                var strDbName = ResPath.GetDbName(strRecPath);
                if (string.IsNullOrEmpty(strDbName))
                {
                    strError = $"无法从记录路径 '{strRecPath}' 中提取数据库名";
                    return -1;
                }
                var db_type = GetAllDbType(strDbName);
                if (string.IsNullOrEmpty(db_type))
                {
                    strError = $"没有找到数据库 '{strDbName}' 的类型";
                    return -1;
                }
                */
                string db_type = (string)param;
                if (string.IsNullOrEmpty(db_type))
                {
                    strError = $"param 不允许为空";
                    return -1;
                }

                XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
                mngr.AddNamespace("dprms", DpNs.dprms);

                var files = dom.DocumentElement.SelectNodes("//dprms:file", mngr);
                if (files.Count > 0
                    && StringUtil.IsInList($"set{db_type}object,setobject", sessioninfo.RightsOrigin) == false)
                {
                    strError = $"当前账户不具备 set{db_type}object 或 setobject 权限，然而记录 '{strRecPath}' 中包含 dprms:file 元素，操作被拒绝";
                    return -1;
                }
            }

            return 0;
        }


        // 检查册记录是否适合进行删除和移动
        // 算法: 要检查馆藏地点是否在当前用户的管辖范围内；另外如果记录包含 dprms:file 元素，当前账户是否具备 setxxxobject 权限
        // 如果返回值不是0，就中断循环并返回
        int CheckItemRecord(
            SessionInfo sessioninfo,
            int index,
            string strRecPath,
            XmlDocument dom,
            byte[] baTimestamp,
            object param,
            out string strError)
        {
            strError = "";

            string strLibraryCodeList = (string)param;
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                strLocation = StringUtil.GetPureLocationString(strLocation);

                // 解析
                ParseCalendarName(strLocation,
            out string strLibraryCode,
            out string strPureName);

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                {
                    strError = "册记录的 '" + strRecPath + "' 的馆藏地点 '" + strLocation + "' 不在当前用户管辖范围 '" + strLibraryCodeList + "' 内，操作被拒绝";
                    return -1;
                }
            }

            // 2023/3/7
            // 检查 dprms:file 元素是否存在
            if (sessioninfo != null)
            {
                XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
                mngr.AddNamespace("dprms", DpNs.dprms);

                var files = dom.DocumentElement.SelectNodes("//dprms:file", mngr);
                if (files.Count > 0 
                    && StringUtil.IsInList("setitemobject,setobject", sessioninfo.RightsOrigin) == false)
                {
                    strError = $"当前账户不具备 setitemobject 或 setobject 权限，然而册记录 '{strRecPath}' 中包含 dprms:file 元素，操作被拒绝";
                    return -1;
                }
            }

            return 0;
        }

        // 复制或者移动编目记录
        // parameters:
        //      strAction   动作。为"onlycopybiblio" "onlymovebiblio" "copy" "move" 之一 
        //      strBiblioType   目前只允许xml一种
        //      strBiblio   源书目记录。目前需要用null调用
        //      baTimestamp 源记录的时间戳
        //      strNewBiblio    需要在目标记录中更新的内容。如果 == null，表示不特意更新
        //      strMergeStyle   如何合并两条书目记录的元数据部分? reserve_source / reserve_target / missing_source_subrecord / overwrite_target_subrecord。 空表示 reserve_source + combine_subrecord
        //                      reserve_source 表示采用源书目记录; reserve_target 表示采用目标书目记录
        //                      missing_source_subrecord 表示丢失来自源的下级记录(保留目标原本的下级记录); overwrite_target_subrecord 表示采纳来自源的下级记录，删除目标记录原本的下级记录(注：此功能暂时没有实现); combine_subrecord 表示组合来源和目标的下级记录
        //      strOutputBiblioRecPath 输出的书目记录路径。当strBiblioRecPath中末级为问号，表示追加保存书目记录的时候，本参数返回实际保存的书目记录路径
        //      baOutputTimestamp   操作完成后，新的时间戳
        // result.Value:
        //      -1  出错
        //      0   成功，没有警告信息。
        //      1   成功，有警告信息。警告信息在 result.ErrorInfo 中
        public LibraryServerResult CopyBiblioInfo(
            SessionInfo sessioninfo,
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strNewBiblioRecPath,
            string strNewBiblio,
            string strMergeStyle,
            out string strOutputBiblio,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp)
        {
            string strError = "";
            long lRet = 0;
            int nRet = 0;

            strOutputBiblioRecPath = "";
            baOutputTimestamp = null;

            strOutputBiblio = "";

            LibraryServerResult result = new LibraryServerResult();

            if (StringUtil.IsInList("overwrite_target_subrecord", strMergeStyle) == true)
            {
                strError = "strMergeStyle 中的 overwrite_target_subrecord 尚未实现";
                goto ERROR1;
            }

            if (StringUtil.IsInList("reserve_target", strMergeStyle) == true
                && StringUtil.IsInList("reserve_source", strMergeStyle) == true)
            {
                strError = "strMergeStyle 中的 reserve_source 和 reserve_target 不应同时具备";
                goto ERROR1;
            }

            // 2017/4/18
            if (StringUtil.IsInList("reserve_target", strMergeStyle) == true
                && string.IsNullOrEmpty(strNewBiblio) == false)
            {
                strError = "strMergeStyle 中包含 reserve_target 时，strNewBiblio 参数必须为空";
                goto ERROR1;
            }

            bool bChangePartDenied = false; // 修改操作部分被拒绝
            string strDeniedComment = "";   // 关于部分字段被拒绝的注释

            string strLibraryCodeList = sessioninfo.LibraryCodeList;

            // 检查参数
            if (strAction != null)
                strAction = strAction.ToLower();

            if (strAction != "onlymovebiblio"
                && strAction != "onlycopybiblio"
                && strAction != "copy"
                && strAction != "move")
            {
                strError = "strAction参数值应当为onlymovebiblio/onlycopybiblio/move/copy之一";
                goto ERROR1;
            }

            strBiblioType = strBiblioType.ToLower();
            if (strBiblioType != "xml")
            {
                strError = "strBiblioType必须为\"xml\"";
                goto ERROR1;
            }

            {
                if (this.TestMode == true || sessioninfo.TestMode == true)
                {
                    // 检查评估模式
                    // return:
                    //      -1  检查过程出错
                    //      0   可以通过
                    //      1   不允许通过
                    nRet = CheckTestModePath(strBiblioRecPath,
                        out strError);
                    if (nRet != 0)
                    {
                        strError = "复制/移动书目记录的操作被拒绝: " + strError;
                        goto ERROR1;
                    }
                }
            }

            string strUnionCatalogStyle = "";
            string strReadAccessParameters = "";
            string strWriteAccessParameters = "";
            // 针对源的读权限是否已经校验过
            bool bReadRightVerified = false;
            // 针对目标的写权限是否已经校验过
            bool bWriteRightVerified = false;
            ItemDbCfg cfg_source = null;
            ItemDbCfg cfg_target = null;

            // 目标书目库名
            string strTargetBiblioDbName = "";

            // 2016/7/4
            // 检查 strNewBiblioRecPath
            if (String.IsNullOrEmpty(strNewBiblioRecPath) == false)
            {
                string strBiblioDbName = ResPath.GetDbName(strNewBiblioRecPath);

                if (this.IsBiblioDbName(strBiblioDbName) == false)
                {
                    strError = "书目记录路径 '" + strNewBiblioRecPath + "' 中包含的数据库名 '" + strBiblioDbName + "' 不是合法的书目库名";
                    goto ERROR1;
                }

                cfg_target = GetBiblioDbCfg(strBiblioDbName);
                if (cfg_target == null)
                {
                    strError = "GetBiblioDbCfg(" + strBiblioDbName + ") return null";
                    goto ERROR1;
                }
                Debug.Assert(cfg_target != null, "");

                strTargetBiblioDbName = strBiblioDbName;
            }

            // 检查数据库路径，看看是不是已经正规定义的编目库？
            if (String.IsNullOrEmpty(strBiblioRecPath) == false)
            {
                string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

                if (this.IsBiblioDbName(strBiblioDbName) == false)
                {
                    strError = "书目记录路径 '" + strBiblioRecPath + "' 中包含的数据库名 '" + strBiblioDbName + "' 不是合法的书目库名";
                    goto ERROR1;
                }

#if NO
                if (this.TestMode == true)
                {
                    string strID = ResPath.GetRecordId(strBiblioRecPath);
                    if (StringUtil.IsPureNumber(strID) == true)
                    {
                        long v = 0;
                        long.TryParse(strID, out v);
                        if (v > 1000)
                        {
                            strError = "dp2Library XE 评估模式下只能修改 ID 小于等于 1000 的书目记录";
                            goto ERROR1;
                        }
                    }
                }
#endif

                cfg_source = GetBiblioDbCfg(strBiblioDbName);
                if (cfg_source == null)
                {
                    strError = "GetBiblioDbCfg(" + strBiblioDbName + ") return null";
                    goto ERROR1;
                }
                Debug.Assert(cfg_source != null, "");
                strUnionCatalogStyle = cfg_source.UnionCatalogStyle;

                if (cfg_target != null && cfg_source.BiblioDbSyntax != cfg_target.BiblioDbSyntax)
                {
                    strError = "源书目库的 MARC 格式(" + cfg_source.BiblioDbSyntax + ") 和目标书目库的 MARC 格式(" + cfg_target.BiblioDbSyntax + ") 不一致，无法进行复制或移动操作";
                    goto ERROR1;
                }

                // 检查存取权限
                if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                {
                    // *** 第一步，检查源数据库相关权限
                    {
                        string strReadAction = "copy";

                        // return:
                        //      null    指定的操作类型的权限没有定义
                        //      ""      定义了指定类型的操作权限，但是否定的定义
                        //      其它      权限列表。* 表示通配的权限列表
                        string strActionList = GetDbOperRights(sessioninfo.Access,
                            strBiblioDbName,
                            "getbiblioinfo");
                        if (strActionList == null)
                        {
                            // 看看是不是关于 getbiblioinfo 的任何权限都没有定义?
                            strActionList = GetDbOperRights(sessioninfo.Access,
                                "",
                                "getbiblioinfo");
                            if (strActionList == null)
                            {
                                // 继续检查写权限
                                goto SKIP_1;
                            }
                            else
                            {
                                strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 getbiblioinfo " + strReadAction + " 操作的存取权限(注:复制操作由一个读和一个写操作构成)";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
                        }

                        if (strActionList == "*")
                        {
                            // 通配
                        }
                        else
                        {
                            if (IsInAccessList(strReadAction, strActionList, out strReadAccessParameters) == false)
                            {
                                strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 getbiblioinfo " + strReadAction + " 操作的存取权限(注:复制操作由一个读和一个写操作构成)";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
                        }

                        bReadRightVerified = true;
                    }

                SKIP_1:

                    // *** 第二步，检查目标数据库相关权限
                    {
                        string strWriteAction = "new";
                        if (ResPath.IsAppendRecPath(strNewBiblioRecPath) == false)
                            strWriteAction = "change";

                        // return:
                        //      null    指定的操作类型的权限没有定义
                        //      ""      定义了指定类型的操作权限，但是否定的定义
                        //      其它      权限列表。* 表示通配的权限列表
                        string strActionList = GetDbOperRights(sessioninfo.Access,
                            strTargetBiblioDbName,
                            "setbiblioinfo");
                        if (strActionList == null)
                        {
                            // 看看是不是关于 setbiblioinfo 的任何权限都没有定义?
                            strActionList = GetDbOperRights(sessioninfo.Access,
                                "",
                                "setbiblioinfo");
                            if (strActionList == null)
                            {
                                // 2014/3/12
                                // TODO: 可以提示"既没有... 也没有 ..."
                                goto CHECK_RIGHTS_2;
                            }
                            else
                            {
                                strError = $"用户 '{sessioninfo.UserID}' 不具备 针对数据库 '{strTargetBiblioDbName}' 执行 setbiblioinfo {strWriteAction} 操作的存取权限";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
                        }

                        if (strActionList == "*")
                        {
                            // 通配
                        }
                        else
                        {
                            if (IsInAccessList(strWriteAction, strActionList, out strWriteAccessParameters) == false)
                            {
                                strError = "用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 setbiblioinfo " + strWriteAction + " 操作的存取权限";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
                        }

                        bWriteRightVerified = true;
                    }
                }
            }

        CHECK_RIGHTS_2:
            if (bReadRightVerified == false)
            {
                // 权限字符串
                if (StringUtil.IsInList("getbiblioinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置书目信息被拒绝。不具备针对源数据库的 order 或 getbiblioinfo 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            if (bWriteRightVerified == false)
            {
                // 权限字符串
                if (StringUtil.IsInList("setbiblioinfo,writerecord,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置书目信息被拒绝。不具备 setbiblioinfo 或 writerecord 或 order 权限";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            // TODO: 需要额外的检查，看看所保存的数据MARC格式是不是这个数据库要求的格式？


            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }

            // 准备日志DOM
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            // 操作不涉及到读者库，所以没有<libraryCode>元素
            DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                "setBiblioInfo");
            DomUtil.SetElementText(domOperLog.DocumentElement, "action",
                strAction);

            string strOperTime = this.Clock.GetClock();

            string strExistingSourceXml = "";
            byte[] exist_source_timestamp = null;

            string strExistTargetXml = "";  // 被覆盖位置，覆盖前的记录

            if (strAction == "onlymovebiblio"
                || strAction == "onlycopybiblio"
                || strAction == "copy"
                || strAction == "move")
            {
                string strMetaData = "";
                string strOutputPath = "";

                // 先读出数据库中此位置的已有记录
                lRet = channel.GetRes(strBiblioRecPath,
                    out strExistingSourceXml,
                    out strMetaData,
                    out exist_source_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.IsNotFoundOrDamaged())
                    {
                        // 2017/5/20 即便源记录不存在，也要在日志记录里面记载 oldRecord 元素
                        XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldRecord", "");
                        DomUtil.SetAttr(node, "recPath", strBiblioRecPath);
                        DomUtil.SetAttr(node, "exist", "false");    // 2017/5/30 增加这个属性，表示源书目记录不存在

                        goto SKIP_MEMO_OLDRECORD;
                    }
                    else
                    {
                        strError = "设置书目信息发生错误, 在读入原有记录阶段:" + strError;
                        goto ERROR1;
                    }
                }

                if (strBiblioRecPath != strOutputPath)
                {
                    strError = "根据路径 '" + strBiblioRecPath + "' 读入原有记录时，发现返回的路径 '" + strOutputPath + "' 和前者不一致";
                    goto ERROR1;
                }

                {
                    // oldRecord 元素实际上是源记录的意思，不是旧记录的意思
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                            "oldRecord", strExistingSourceXml);
                    DomUtil.SetAttr(node, "recPath", strBiblioRecPath);
                }

                // TODO: 如果已存在的XML记录中，MARC根不是文档根，那么表明书目记录
                // 还存储有其他信息，这时就需要把前端送来的XML记录和已存在的记录进行合并处理，
                // 防止贸然覆盖了文档根下的有用信息。
            }

        SKIP_MEMO_OLDRECORD:

            // bool bBiblioNotFound = false;

            string strRights = "";

            if (sessioninfo.Account != null)
                strRights = sessioninfo.Account.Rights;

            if (strAction == "onlycopybiblio"
                || strAction == "onlymovebiblio"
                || strAction == "copy"
                || strAction == "move")
            {
                if (string.IsNullOrEmpty(strNewBiblio) == false)
                {
                    // 观察时间戳是否发生变化
                    nRet = ByteArray.Compare(baTimestamp, exist_source_timestamp);
                    if (nRet != 0)
                    {
                        strError = "移动或复制操作发生错误，源记录已经发生了修改(时间戳不匹配。当前提交的时间戳: '" + ByteArray.GetHexTimeStampString(baTimestamp) + "', 数库库中原记录的时间戳: '" + ByteArray.GetHexTimeStampString(exist_source_timestamp) + "')";
                        goto ERROR1;
                    }
                }

                // TODO: 如果目标书目记录路径已知，则需要对两个路径都加锁。注意从小号到大号顺次加锁，避免死锁

                this.BiblioLocks.LockForWrite(strBiblioRecPath);
                try
                {
                    if (String.IsNullOrEmpty(strNewBiblio) == false)
                    {
                        // 合并联合编目的新旧书目库XML记录
                        // 功能：排除新记录中对strLibraryCode定义以外的905字段的修改
                        // parameters:
                        //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
                        // return:
                        //      -1  error
                        //      0   not delete any fields
                        //      1   deleted some fields
                        nRet = MergeOldNewBiblioRec(
                            strRights,
                            strUnionCatalogStyle,
                            strLibraryCodeList,
                            "insert,replace,delete",
                            strWriteAccessParameters,
                            strReadAccessParameters,    // 2023/2/11
                            strExistingSourceXml,
                            ref strNewBiblio,
                            ref bChangePartDenied,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (bChangePartDenied == true && string.IsNullOrEmpty(strError) == false)
                            strDeniedComment += " " + strError;

                        /*
                        // 2011/11/30
                        nRet = this.SetOperation(
        ref strNewBiblio,
        strAction,
        sessioninfo.UserID,
        "source: " + strBiblioRecPath,
        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                         * */
                    }
                    else
                    {
                        // strNewBiblio 为空
                    }

                    // 查重
                    if (string.IsNullOrEmpty(strNewBiblio) == false)
                    {
                        // return:
                        //      -1  出错
                        //      0   没有命中
                        //      >0  命中条数。此时 strError 中返回发生重复的路径列表
                        nRet = SearchBiblioDup(
                            sessioninfo,
                            strNewBiblioRecPath,
                            strNewBiblio,
                            "copybiblio", // strResultSetName,
                            strAction == "move" || strAction == "onlymovebiblio" ? new List<string> { strBiblioRecPath } : null,
                            out _,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet > 0)
                        {
                            // move 操作，要排除 source 记录路径，因为它即将被删除
                            strOutputBiblioRecPath = strError;
                            result.Value = -1;
                            result.ErrorInfo = "经查重发现书目库中已有 " + nRet.ToString() + " 条重复记录(" + strOutputBiblioRecPath + ")。本次保存操作(" + strAction + ")被拒绝";
                            result.ErrorCode = ErrorCode.BiblioDup;
                            return result;
                        }
                    }
                    else if (StringUtil.IsInList("reserve_target", strMergeStyle) == false)
                    {
                        // 如果当前配置了要查重，则复制行为要看源和目标是否都在同一个 space 内，如果是，则必然会造成重复，那就要拒绝执行
                        // 如果不在同一个 space 内，则要用 strSourceBiblio 对 strTargetBiblioRecPath 所在空间进行查重
                        if ((strAction == "onlycopybiblio" || strAction == "copy")
                            && IsInSameUniqueSpace(ResPath.GetDbName(strBiblioRecPath), ResPath.GetDbName(strNewBiblioRecPath)) == true)
                        {
                            strError = "因源书目记录 '" + strBiblioRecPath + "' 和目标书目记录 '" + strNewBiblioRecPath + "' 处在同一查重空间内，不允许进行直接复制(若允许复制会导致书目记录出现重复)";
                            goto ERROR1;
                        }

                        // TODO: 确保 strExistingSourceXml 中有 997
                        {
                            /* // 测试代码
                            nRet = RemoveUniformKey(
ref strExistingSourceXml,
out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            */

                            // return:
                            //      -1  出错
                            //      0   strBiblioXml 没有发生修改
                            //      1   strBiblioXml 发生了修改
                            nRet = CreateUniformKey(
                                false,
        ref strExistingSourceXml,
        out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            // 拟复制的记录内容被添加了 997 字段
                            if (nRet == 1 && string.IsNullOrEmpty(strNewBiblio))
                                strNewBiblio = strExistingSourceXml;
                        }

                        // return:
                        //      -1  出错
                        //      0   没有命中
                        //      >0  命中条数。此时 strError 中返回发生重复的路径列表
                        nRet = SearchBiblioDup(
                            sessioninfo,
                            strNewBiblioRecPath,
                            strExistingSourceXml,
                            "copybiblio", // strResultSetName,
                            strAction == "move" || strAction == "onlymovebiblio" ? new List<string> { strBiblioRecPath } : null,
                            out _,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet > 0)
                        {
                            strOutputBiblioRecPath = strError;
                            result.Value = -1;
                            result.ErrorInfo = "经查重发现书目库中已有 " + nRet.ToString() + " 条重复记录(" + strOutputBiblioRecPath + ")。本次保存操作(" + strAction + ")被拒绝";
                            result.ErrorCode = ErrorCode.BiblioDup;
                            return result;
                        }
                    }

                    bool bBiblioMoved = false;
                    if (string.IsNullOrEmpty(strExistingSourceXml) == false)
                    {
                        nRet = DoBiblioOperMove(
                            strAction,
                            sessioninfo,
                            channel,
                            strBiblioRecPath,
                            // strExistingSourceXml,
                            strNewBiblioRecPath,
                            strNewBiblio,    // 已经经过Merge预处理的新记录XML
                            strMergeStyle,
                            out strExistTargetXml,
                            out strOutputBiblio,
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        bBiblioMoved = true;
                    }
                    else
                    {
                        // 如果要求追加到一个不确定的目标 ID 位置，但此时源记录又不存在，暂时不支持这个功能
                        if (ResPath.IsAppendRecPath(strNewBiblioRecPath) == true)
                        {
                            strError = "在源记录 '" + strBiblioRecPath + "' 不存在的情况下，不支持追加方式的目标记录路径 '" + strNewBiblioRecPath + "'";
                            goto ERROR1;
                        }
                        strOutputBiblioRecPath = strNewBiblioRecPath;
                    }

                    if ((strAction == "copy" || strAction == "move")
                        && StringUtil.IsInList("missing_source_subrecord", strMergeStyle) == false)
                    {
                        string strWarning = "";
                        // 
                        // 调用前，假定书目记录已经被锁定
                        // parameters:
                        //      strAction   copy / move
                        // return:
                        //      -2  权限不够
                        //      -1  出错
                        //      >=0   成功。返回实际拷贝或者移动的下级记录个数
                        nRet = DoCopySubRecord(
                            sessioninfo,
                            strAction,
                            strBiblioRecPath,
                            strOutputBiblioRecPath,
                            strMergeStyle,
                            domOperLog,
                            out strWarning,
                            out strError);
                        if (nRet == -1 || nRet == -2)   // 2017/5/28 增加 nRet == -2
                        {
                            // Undo Copy biblio record
                            {
                                string strText = "对书目记录 '" + strBiblioRecPath + "' 进行 '" + strAction + "' 操作的 复制或者移动下级记录阶段 时候发生错误：" + strError;
                                if (string.IsNullOrEmpty(strWarning) == false)
                                    strText += "(" + strWarning + ")";
                                if (bBiblioMoved)
                                    strText += "。后面将进行 undo 书目记录 '" + strAction + "' 的操作，这是一条提示消息";
                                this.WriteErrorLog(strText);
                            }

                            if (bBiblioMoved == true)
                            {
                                // 移动回去
                                if (strAction == "onlymovebiblio" || strAction == "move")
                                {
                                    byte[] output_timestamp = null;
                                    string strTempOutputRecPath = "";
                                    string strError_1 = "";

                                    lRet = channel.DoCopyRecord(strOutputBiblioRecPath,
                                         strBiblioRecPath,
                                         true,   // bDeleteSourceRecord
                                         out output_timestamp,
                                         out strTempOutputRecPath,
                                         out strError_1);
                                    if (lRet == -1)
                                    {
                                        this.WriteErrorLog("复制 '" + strBiblioRecPath + "' 下属的册记录时出错: " + strError + "，并且Undo的时候(从 '" + strOutputBiblioRecPath + "' 复制回 '" + strBiblioRecPath + "')失败: " + strError_1);
                                    }
                                }
                                else if (strAction == "onlycopybiblio" || strAction == "copy")
                                {
                                    // 删除刚刚复制的目标记录
                                    string strError_1 = "";
                                    int nRedoCount = 0;
                                REDO_DELETE:
                                    lRet = channel.DoDeleteRes(strOutputBiblioRecPath,
                                        baTimestamp,
                                        out baOutputTimestamp,
                                        out strError_1);
                                    if (lRet == -1 && !channel.IsNotFound())
                                    {
                                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                                            && nRedoCount < 10)
                                        {
                                            baTimestamp = baOutputTimestamp;
                                            nRedoCount++;
                                            goto REDO_DELETE;
                                        }
                                        this.WriteErrorLog("复制 '" + strBiblioRecPath + "' 下属的册记录时出错: " + strError + "，并且Undo的时候(删除记录 '" + strOutputBiblioRecPath + "')失败: " + strError_1);
                                    }
                                }
                            }
                            goto ERROR1;
                        }
                        result.ErrorInfo = strWarning;

                        if (nRet == 0 && bBiblioMoved == false)
                            domOperLog = null;  // 没有必要写入操作日志记录
                    }

                }
                finally
                {
                    this.BiblioLocks.UnlockForWrite(strBiblioRecPath);
                }
            }
            else
            {
                strError = "未知的strAction参数值 '" + strAction + "'";
                goto ERROR1;
            }

            if (domOperLog != null)
            {
                {
                    // 注：如果strNewBiblio为空，则表明仅仅进行了复制，并没有在目标记录写什么新内容
                    // 如果在日志记录中要查到到底复制了什么内容，可以看<oldRecord>元素的文本内容
                    // 注: 如果 strMergeStyle 为 reserve_target， 需要记载一下这个位置已经存在的记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                            "record", string.IsNullOrEmpty(strOutputBiblio) == false ? strOutputBiblio : strNewBiblio);
                    DomUtil.SetAttr(node, "recPath", strOutputBiblioRecPath);
                }

                // 2017/1/16
                if (string.IsNullOrEmpty(strExistTargetXml) == false)
                {
                    // 注：当 strMergeStyle 为 reserve_target 的时候，目标记录并未被覆盖，这里只是记载一下原目标记录(并未被覆盖或修改)，也有好处
                    // 若此时这里不记载，record 元素里面也能找到完全一样的内容
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
            "overwritedRecord", strExistTargetXml);
                    DomUtil.SetAttr(node, "recPath", strOutputBiblioRecPath);
                }

                // 2015/1/21
                // 注；如果 reserve_source 和 reserve_target 都没有，则默认 reserve_source
                DomUtil.SetElementText(domOperLog.DocumentElement, "mergeStyle",
        strMergeStyle);

                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    sessioninfo.UserID);
                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTime);

                // 写入日志
                nRet = this.OperLog.WriteOperLog(domOperLog,
                    sessioninfo.ClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "CopyBiblioInfo() API 写入日志时发生错误: " + strError;
                    goto ERROR1;
                }
            }

            if (string.IsNullOrEmpty(result.ErrorInfo) == true)
                result.Value = 0;   // 没有警告
            else
                result.Value = 1;   // 有警告

            // 2013/3/5
            if (bChangePartDenied == true)
            {
                result.ErrorCode = ErrorCode.PartialDenied;
                if (string.IsNullOrEmpty(strDeniedComment) == false)
                {
                    if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                        result.ErrorInfo += " ; ";
                    result.ErrorInfo += strDeniedComment;
                }
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // TODO: 想办法实现完美的出错 Undo 功能，即具备完整事务回滚能力
        // 2011/4/24
        // 调用前，假定书目记录已经被锁定
        // parameters:
        //      strAction   copy / move
        //      strStyle    strict 或者 loose。默认 strict。是否为严格模式。严格表示如果目标书目库少了必要的下级库，会当作出错返回
        //                  注意不要对上级函数的 strMergeStyle 值敏感，这里只关注 strict 和 loose 即可
        // return:
        //      -2  权限不够
        //      -1  出错
        //      >=0   成功。返回实际拷贝或者移动的下级记录个数
        int DoCopySubRecord(
            SessionInfo sessioninfo,
            string strAction,
            string strBiblioRecPath,
            string strNewBiblioRecPath,
            string strStyle,
            XmlDocument domOperLog,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nCopyCount = 0;

            int nRet = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strTargetBiblioDbName = ResPath.GetDbName(strNewBiblioRecPath);

            bool bStrict = true;
            if (StringUtil.IsInList("loose", strStyle) == true)
                bStrict = false;

            // 1)
            // 探测书目记录有没有下属的实体记录(也顺便看看实体记录里面是否有流通信息)?
            List<DeleteEntityInfo> entityinfos = null;
            long lHitCount = 0;

            // TODO: 只要获得记录路径即可，因为后面利用了CopyRecord复制
            // return:
            //      -2  not exist entity dbname
            //      -1  error
            //      >=0 含有流通信息的实体记录个数
            nRet = SearchChildEntities(
                sessioninfo,
                channel,
                strBiblioRecPath,
                "count_borrow_info,return_record_xml",
                // sessioninfo.GlobalUser == false ? CheckItemRecord : (Delegate_checkRecord)null,
                CheckItemRecord,
                sessioninfo.GlobalUser == false ? sessioninfo.LibraryCodeList : null,
                out lHitCount,
                out entityinfos,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == -2)
            {
                Debug.Assert(entityinfos.Count == 0, "");
            }

            int nBorrowInfoCount = nRet;

            // 如果有实体记录，则要求 setiteminfo 权限，才能创建或者移动实体们
            if (entityinfos != null && entityinfos.Count > 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("setiteminfo,setobject", sessioninfo.RightsOrigin) == false)
                {
                    strError = "复制(移动)书目信息的操作被拒绝。因拟操作的书目记录带有下属的实体记录，但当前用户不具备 setiteminfo 或 setobject 权限，不能复制或者移动它们。";
                    return -2;
                }

                if (strAction == "move")
                {
                    // return:
                    //      -2  目标实体库不存在
                    //      -1  出错
                    //      0   存在
                    nRet = DetectTargetChildDbExistence(
                "item",
                strTargetBiblioDbName,
                out strError);
                    if (nRet == -2)
                    {
                        if (bStrict == true)
                        {
                            strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，(若强行移动)将丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录。因此移动操作被拒绝";
                            goto ERROR1;
                        }

                        if (bStrict == false && nBorrowInfoCount > 0 && strAction == "move")
                        {
                            strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，(若强行移动)将丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录。但这些实体记录中已经存在有 " + nBorrowInfoCount.ToString() + " 个流通信息，这意味着这些实体记录不应消失。因此移动操作被迫放弃";
                            goto ERROR1;
                        }
                    }
                }
            }

            // 2)
            // 探测书目记录有没有下属的订购记录
            List<DeleteEntityInfo> orderinfos = null;
            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.OrderItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml,check_circulation_info",
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out orderinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(orderinfos.Count == 0, "");
            }

            // 如果有订购记录，则要求 setorderinfo 权限，才能创建或者移动它们
            if (orderinfos != null && orderinfos.Count > 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("setorderinfo,setobject,order", sessioninfo.RightsOrigin) == false)
                {
                    strError = "复制(移动)书目信息的操作被拒绝。因拟操作的书目记录带有下属的订购记录，但当前用户不具备 setorderinfo、setobject 或 order 权限，不能复制或移动它们。";
                    return -2;
                }

                if (bStrict && strAction == "move")
                {
                    // return:
                    //      -2  目标实体库不存在
                    //      -1  出错
                    //      0   存在
                    nRet = DetectTargetChildDbExistence(
                "order",
                strTargetBiblioDbName,
                out strError);
                    if (nRet == -2)
                    {
                        strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的订购库，(若强行移动)将丢失来自源书目库下属的 " + orderinfos.Count + " 条订购记录。因此移动操作被拒绝";
                        goto ERROR1;
                    }
                }
            }

            // 3)
            // 探测书目记录有没有下属的期记录
            List<DeleteEntityInfo> issueinfos = null;

            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.IssueItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml,check_circulation_info",
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out issueinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(issueinfos.Count == 0, "");
            }

            // 如果有期记录，则要求 setissueinfo 权限，才能创建或者移动它们
            if (issueinfos != null && issueinfos.Count > 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("setissueinfo,setobject", sessioninfo.RightsOrigin) == false)
                {
                    strError = "复制(移动)书目信息的操作被拒绝。因拟操作的书目记录带有下属的期记录，但当前用户不具备 setissueinfo 或 setobject 权限，不能复制或移动它们。";
                    return -2;
                }

                if (bStrict && strAction == "move")
                {
                    // return:
                    //      -2  目标实体库不存在
                    //      -1  出错
                    //      0   存在
                    nRet = DetectTargetChildDbExistence(
                "issue",
                strTargetBiblioDbName,
                out strError);
                    if (nRet == -2)
                    {
                        strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的期库，(若强行移动)将丢失来自源书目库下属的 " + issueinfos.Count + " 条期记录。因此移动操作被拒绝";
                        goto ERROR1;
                    }
                }

            }

            // 4)
            // 探测书目记录有没有下属的评注记录
            List<DeleteEntityInfo> commentinfos = null;
            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.CommentItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml,check_circulation_info",
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out commentinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(commentinfos.Count == 0, "");
            }

            // 如果有评注记录，则要求setcommentinfo权限，才能创建或者移动它们
            if (commentinfos != null && commentinfos.Count > 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("setcommentinfo,writerecord", sessioninfo.RightsOrigin) == false)
                {
                    strError = "复制(移动)书目信息的操作被拒绝。因拟操作的书目记录带有下属的评注记录，但当前用户不具备 setcommentinfo 或 writerecord 权限，不能复制或移动它们。";
                    return -2;
                }

                if (bStrict && strAction == "move")
                {
                    // return:
                    //      -2  目标实体库不存在
                    //      -1  出错
                    //      0   存在
                    nRet = DetectTargetChildDbExistence(
                "comment",
                strTargetBiblioDbName,
                out strError);
                    if (nRet == -2)
                    {
                        strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的评注库，(若强行移动)将丢失来自源书目库下属的 " + commentinfos.Count + " 条评注记录。因此移动操作被拒绝";
                        goto ERROR1;
                    }
                }
            }

            // ** 第二阶段
            // 真正进行移动或者复制
            if (entityinfos != null && entityinfos.Count > 0)
            {
                // TODO: 如果是复制, 则要为目标实体记录的测条码号增加一个前缀。或者受到strStyle控制，能决定在source或者target中加入前缀

                // 复制属于同一书目记录的全部实体记录
                // parameters:
                //      strAction   copy / move
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = CopyBiblioChildEntities(channel,
                    strAction,
                    entityinfos,
                    strNewBiblioRecPath,
                    domOperLog,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == -2)
                {
                    // TODO: 需要检查源实体记录中是否至少有一个包含流通信息。如果有，则这样丢失它们意味着流通信息的丢失，这是不能允许的
                    if (nBorrowInfoCount > 0
                        && strAction == "move")
                    {
                        strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，(若强行移动)将丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录。但这些实体记录中已经存在有 " + nBorrowInfoCount.ToString() + " 个流通信息，这意味着这些实体记录不能消失。因此移动操作被迫放弃";
                        goto ERROR1;
                    }

                    if (strAction == "move")
                    {
                        //strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，(若强行移动)将丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录。因此移动操作被拒绝";
                        //goto ERROR1;

                        // 删除属于同一书目记录的全部实体记录
                        // 这是需要提供EntityInfo数组的版本
                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = DeleteBiblioChildEntities(channel,
                            entityinfos,
                            domOperLog,
                            out strError);
                        if (nRet == -1)
                            this.WriteErrorLog("在删除书目记录 '" + strBiblioRecPath + "' 的下属册记录阶段出错: " + strError);
                    }

                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，已丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录; ";
                }

                if (nRet > 0)
                    nCopyCount += nRet;
            }

            if (orderinfos != null && orderinfos.Count > 0)
            {
                // 复制订购记录
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.OrderItemDatabase.CopyBiblioChildItems(channel,
                strAction,
                orderinfos,
                strNewBiblioRecPath,
                domOperLog,
                out strError);
                if (nRet == -1)
                {
                    if (entityinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    goto ERROR1;
                }
                if (nRet == -2)
                {
                    if (strAction == "move")
                    {
                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.OrderItemDatabase.DeleteBiblioChildItems(
                            channel,
                            orderinfos,
                            domOperLog,
                            out strError);
                        if (nRet == -1)
                            this.WriteErrorLog("在删除书目记录 '" + strBiblioRecPath + "' 的下属订购记录阶段出错: " + strError);
                    }

                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的订购库，已丢失来自源书目库下属的 " + orderinfos.Count + " 条订购记录; ";
                }
                if (nRet > 0)
                    nCopyCount += nRet;
            }

            if (issueinfos != null && issueinfos.Count > 0)
            {
                // 复制期记录
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.IssueItemDatabase.CopyBiblioChildItems(channel,
            strAction,
            issueinfos,
            strNewBiblioRecPath,
            domOperLog,
            out strError);
                if (nRet == -1)
                {
                    if (entityinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    if (orderinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + orderinfos.Count.ToString() + " 个订购记录已经无法恢复";
                    goto ERROR1;
                }
                if (nRet == -2)
                {
                    if (strAction == "move")
                    {
                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.IssueItemDatabase.DeleteBiblioChildItems(
                            channel,
                            issueinfos,
                            domOperLog,
                            out strError);
                        if (nRet == -1)
                            this.WriteErrorLog("在删除书目记录 '" + strBiblioRecPath + "' 的下属期记录阶段出错: " + strError);
                    }

                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的期库，已丢失来自源书目库下属的 " + issueinfos.Count + " 条期记录; ";
                }
                if (nRet > 0)
                    nCopyCount += nRet;
            }

            if (commentinfos != null && commentinfos.Count > 0)
            {
                // 复制评注记录
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.CommentItemDatabase.CopyBiblioChildItems(channel,
            strAction,
            commentinfos,
            strNewBiblioRecPath,
            domOperLog,
            out strError);
                if (nRet == -1)
                {
                    if (entityinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    if (orderinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + orderinfos.Count.ToString() + " 个订购记录已经无法恢复";
                    if (issueinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + issueinfos.Count.ToString() + " 个期记录已经无法恢复";
                    goto ERROR1;
                }
                if (nRet == -2)
                {
                    if (strAction == "move")
                    {
                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.CommentItemDatabase.DeleteBiblioChildItems(
                            channel,
                            commentinfos,
                            domOperLog,
                            out strError);
                        if (nRet == -1)
                            this.WriteErrorLog("在删除书目记录 '" + strBiblioRecPath + "' 的下属评注记录阶段出错: " + strError);
                    }

                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的评注库，已丢失来自源书目库下属的 " + commentinfos.Count + " 条评注记录; ";
                }
                if (nRet > 0)
                    nCopyCount += nRet;
            }

            return nCopyCount;
        ERROR1:
            return -1;
        }

        // 移动或者复制书目记录
        // strExistingXml和请求中传来的old xml的时间戳比较，在本函数外、调用前进行
        // parameters:
        //      strAction   动作。为"onlycopybiblio" "onlymovebiblio"之一。增加 copy / move
        //      strNewBiblio    需要在目标记录中更新的内容。如果 == null，表示不特意更新
        //      strMergeStyle   如何合并两条记录的(书目)元数据部分? reserve_source / reserve_target。 空表示 reserve_source
        //      strExistingTargetXml    被覆盖的目标位置的原有记录
        //      strOutputRecPath    目标记录路径
        int DoBiblioOperMove(
            string strAction,
            SessionInfo sessioninfo,
            RmsChannel channel,
            string strOldRecPath,
            // string strExistingSourceXml,
            string strNewRecPath,
            string strNewBiblio,    // 已经经过Merge预处理的新记录XML
            string strMergeStyle,
            out string strExistTargetXml,
            out string strOutputTargetXml,
            out byte[] baOutputTimestamp,
            out string strOutputRecPath,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            baOutputTimestamp = null;
            strOutputRecPath = "";
            strExistTargetXml = "";

            strOutputTargetXml = ""; // 最后保存成功的记录

            // 检查路径
            if (strOldRecPath == strNewRecPath)
            {
                strError = "当action为\"" + strAction + "\"时，strNewRecordPath路径 '" + strNewRecPath + "' 和strOldRecPath '" + strOldRecPath + "' 必须不相同";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strNewRecPath) == true)
            {
                strError = "DoBiblioOperMove() strNewRecPath参数值不能为空";
                goto ERROR1;
            }

            // 检查即将覆盖的目标位置是不是有记录，如果有，则不允许进行move操作。
            bool bAppendStyle = false;  // 目标路径是否为追加形态？
            string strTargetRecId = ResPath.GetRecordId(strNewRecPath);

            if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
            {
                // 2009/11/1 
                if (String.IsNullOrEmpty(strTargetRecId) == true)
                    strNewRecPath += "/?";

                bAppendStyle = true;
            }

            string strOutputPath = "";
            string strMetaData = "";

            byte[] exist_target_timestamp = null;
            if (bAppendStyle == false)
            {
                // 获取覆盖目标位置的现有记录
                lRet = channel.GetRes(strNewRecPath,
                    out strExistTargetXml,
                    out strMetaData,
                    out exist_target_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.IsNotFoundOrDamaged())
                    {
                        // 如果记录不存在, 说明不会造成覆盖态势
                        /*
                        strExistSourceXml = "<root />";
                        exist_source_timestamp = null;
                        strOutputPath = info.NewRecPath;
                         * */
                    }
                    else
                    {
                        strError = "移动操作发生错误, 在读入即将覆盖的目标位置 '" + strNewRecPath + "' 原有记录阶段:" + strError;
                        goto ERROR1;
                    }
                }
                else
                {
#if NO
                    // 如果记录存在，则目前不允许这样的操作
                    strError = "移动(move)操作被拒绝。因为在即将覆盖的目标位置 '" + strNewRecPath + "' 已经存在书目记录。请先删除(delete)这条记录，再进行移动(move)操作";
                    goto ERROR1;
#endif
                }

                strOutputRecPath = strOutputPath;   // 2017/4/17
            }

            /*
            // 把两个记录装入DOM

            XmlDocument domSourceExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domSourceExist.LoadXml(strExistingSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(strNewBiblio);
            }
            catch (Exception ex)
            {
                strError = "strNewBiblio装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }
             * */

            // 2020/6/3
            // 判断一下是否有存取定义的 setbiblioinfo 权限
            bool has_rights = false;
            if (String.IsNullOrEmpty(sessioninfo.Access) == false)
            {
                string strSourceDbName = ResPath.GetDbName(strOldRecPath);
                string strDbType = "biblio";

                // return:
                //      null    指定的操作类型的权限没有定义
                //      ""      定义了指定类型的操作权限，但是否定的定义
                //      其它      权限列表。* 表示通配的权限列表
                string strAccessActionList = GetDbOperRights(sessioninfo.Access,
                    strSourceDbName,
                    strDbType == "biblio" ? "setbiblioinfo" : "setauthorityinfo");

                if (strAccessActionList == "*" || IsInAccessList(strAction, strAccessActionList, out string _) == true)
                    has_rights = true;
            }

            // 只有order权限的情况
            if ((has_rights == false && StringUtil.IsInList("setbiblioinfo", sessioninfo.RightsOrigin) == false)
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
            {
                if (strAction == "onlymovebiblio"
                    || strAction == "move")
                {
                    string strSourceDbName = ResPath.GetDbName(strOldRecPath);

                    // 源头书目库为 非工作库 情况
                    if (IsOrderWorkBiblioDb(strSourceDbName) == false)
                    {
                        // 非工作库不能删除记录
                        if (IsOrderWorkBiblioDb(strSourceDbName) == false)
                        {
                            // 非工作库。要求原来记录不存在
                            strError = "当前帐户只有 order 权限而没有setbiblioinfo权限，不能用" + strAction + "功能删除(位于非工作库中的)源书目记录 '" + strOldRecPath + "'";
                            goto ERROR1;
                        }
                    }
                }
            }

            // 移动记录
#if NO
            if (StringUtil.IsInList("reserve_target", strMergeStyle) == true)
            {
            // 这个方法的问题是，源记录中的 files 不会被移动到目标记录中
                byte[] output_timestamp = null;

                lRet = channel.DoDeleteRes(strOldRecPath, 
                    null,
                    "ignorechecktimestamp",
                    out output_timestamp, 
                    out strError);
                if (lRet == -1)
                {
                    baOutputTimestamp = output_timestamp;   // 即便出错了也可能会返回

                    strError = "删除源书目记录时出错 DoDeleteRes() error :" + strError;
                    goto ERROR1;
                }
                baOutputTimestamp = exist_target_timestamp;
                strOutputTargetXml = strExistTargetXml;
            }
            else
#endif
            {
                byte[] output_timestamp = null;
                string strIdChangeList = "";

                // TODO: Copy后还要写一次？因为Copy并不写入新记录。
                // 其实Copy的意义在于带走资源。否则还不如用Save+Delete
                lRet = channel.DoCopyRecord(strOldRecPath,
                     strNewRecPath,
                     strAction == "onlymovebiblio" || strAction == "move" ? true : false,   // bDeleteSourceRecord
                     strMergeStyle,
                     out strIdChangeList,
                     out output_timestamp,
                     out strOutputRecPath,
                     out strError);
                if (lRet == -1)
                {
                    baOutputTimestamp = output_timestamp;   // 即便出错了也可能会返回

                    strError = "DoCopyRecord() error :" + strError;
                    goto ERROR1;
                }

                baOutputTimestamp = output_timestamp;
            }

            // TODO: 兑现对 856 字段的合并，和来自源的 856 字段的 $u 修改

            if (String.IsNullOrEmpty(strNewBiblio) == false)
            {
                this.BiblioLocks.LockForWrite(strOutputRecPath);

                try
                {
                    // TODO: 如果新的、已存在的xml没有不同，或者新的xml为空，则这步保存可以省略
                    byte[] output_timestamp = baOutputTimestamp;

                    string strOutputBiblioRecPath = "";
                    lRet = channel.DoSaveTextRes(strOutputRecPath,
                        strNewBiblio,
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputBiblioRecPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    this.BiblioLocks.UnlockForWrite(strOutputRecPath);
                }
            }

            // if (string.IsNullOrEmpty(strOutputTargetXml))
            {
                // TODO: 是否和前面一起锁定?

                // 获取最后的记录
                lRet = channel.GetRes(strOutputRecPath,
                    out strOutputTargetXml,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
            }

            return 0;
        ERROR1:
            return -1;
        }

        bool UniqueSpaceDefined()
        {
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("unique/space");
            if (nodes.Count > 0)
                return true;
            return false;
        }

        /*
    <unique>
        <space dbnames="中文图书,中文编目" />
    </unique>
         * * */
        // 获得一个书目库所从属的查重空间的所有书目库名字
        List<string> GetUniqueSpaceDbNames(string strBiblioDbName)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("unique/space");
            foreach (XmlElement space in nodes)
            {
                string list = space.GetAttribute("dbnames");

                string[] names = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (Array.IndexOf(names, strBiblioDbName) != -1)
                    results.AddRange(names);
            }

            StringUtil.RemoveDupNoSort(ref results);
            return results;
        }

        // 观察两个书目库是否处在同一个查重空间内
        bool IsInSameUniqueSpace(string strBiblioDbName1, string strBiblioDbName2)
        {
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("unique/space");
            foreach (XmlElement space in nodes)
            {
                string list = space.GetAttribute("dbnames");

                string[] names = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (Array.IndexOf(names, strBiblioDbName1) != -1
                    && Array.IndexOf(names, strBiblioDbName2) != -1)
                    return true;
            }

            return false;
        }

        static string Get997a(string strBiblioXml, out string strError)
        {
            strError = "";

            string strMarcSyntax = "";
            string strMarc = "";
            int nRet = MarcUtil.Xml2Marc(strBiblioXml,
    true,
    "", // this.CurMarcSyntax,
    out strMarcSyntax,
    out strMarc,
    out strError);
            if (nRet == -1)
                return null;

            MarcRecord record = new MarcRecord(strMarc);
            string strKey = record.select("field[@name='997']/subfield[@name='a']").FirstContent;
            if (string.IsNullOrEmpty(strKey))
            {
                strError = "MARC 记录中不存在 997$a";
                return null;
            }

            return strKey;
        }

        // 对书目或者规范库做强制查重
        // 注：书目库和规范库的名字即便混合起来配置在一个空间内也不怕
        // parameters:
        //      error_code [out] 返回出错码。undefined/notInUniqueSpace
        // return:
        //      -3  查重空间尚未定义
        //      -2  发起记录并不处在查重空间中
        //      -1  出错
        //      0   没有命中
        //      >0  命中条数。此时 strError 中返回发生重复的路径列表
        int SearchBiblioDup(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            string strBiblioXml,
            string strResultSetName,
            List<string> exclude_recpaths,
            out string error_code,
            out string strError)
        {
            strError = "";
            error_code = "";

            if (UniqueSpaceDefined() == false)
            {
                error_code = "undefined";   // library.xml 中尚未定义查重空间
                return 0;
            }

            // 2023/1/16
            // 对于空记录不进行查重
            /*
            if (string.IsNullOrEmpty(strBiblioXml)
                || strBiblioXml == "<root />")
                return 0;
            */
            if (IsEmptyXml(strBiblioXml))
                return 0;

            string strKey = Get997a(strBiblioXml, out strError);
            if (strKey == null)
                return -1;
            //if (strKey == null)
            //    return 0;   // 因为西文图书还没有提供 997，所以暂时这样返回

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

            // 一个书目库同时处在多个 space 中怎么办？
            List<string> dbnames = GetUniqueSpaceDbNames(strBiblioDbName);
            if (dbnames.Count == 0)
            {
                error_code = "notInUniqueSpace";    // 发起记录并不处在查重空间中
                return 0;
            }

            // 构造检索书目库的 XML 检索式
            // return:
            //      -2  没有找到指定风格的检索途径
            //      -1  出错
            //      0   没有发现任何书目库定义
            //      1   成功
            int nRet = this.BuildSearchBiblioQuery(
        StringUtil.MakePathList(dbnames),
        strKey,
        100,
        "ukey",
        "exact",
        "zh",
        "", // strSearchStyle,
        out List<string> dbTypes,
        out string strQueryXml,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;
            if (nRet == -2)
            {
#if NO
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.FromNotFound;
                    return result;
#endif
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "GetChannel() return null";
                return -1;
            }

#if NO
            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,   // "default",
                "", // strOutputStyle,
                out strError);
            if (lRet == -1)
                return -1;
            if (lRet == 0)
                return 0;   // not found
#endif
            long lRet = channel.DoSearchEx(strQueryXml,
    strResultSetName,   // "default",
    "", // strOutputStyle,
    1000,
            "zh",
            "id",
            out Record[] records,
            out strError);
            if (lRet == -1)
                return -1;
            if (lRet == 0)
                return 0;   // not found

            // 如果命中 1 条，则需要提取出来看看是否为 strBiblioRecPath 自己
            if (lRet == 1)
            {
                if (records == null || records.Length < 1)
                {
                    strError = "records == null || records.Length < 1";
                    return -1;
                }
                if (records[0].Path == strBiblioRecPath)
                    return 0;
            }

            List<string> recpaths = new List<string>();
            foreach (Record record in records)
            {
                recpaths.Add(record.Path);
            }

            // 2018/10/25
            // 对命中的记录路径进行去重
            recpaths.Sort();
            StringUtil.RemoveDup(ref recpaths, true);

            // 去掉查重发起记录的路径
            recpaths.Remove(strBiblioRecPath);

            // 去掉额外需要排除的记录路径
            if (exclude_recpaths != null)
            {
                foreach (string recpath in exclude_recpaths)
                {
                    recpaths.Remove(recpath);
                }
            }

            strError = StringUtil.MakePathList(recpaths);
            return recpaths.Count;
        }

        static bool IsEmptyXml(string strBiblioXml)
        {
            if (string.IsNullOrEmpty(strBiblioXml)
                || strBiblioXml == "<root />")
                return true;
            return false;
        }

        /*
        // 合并新旧两条记录。保存MARC根以外的其他信息。
        // 有下列几种模式：
        // 1) 新记录中只有MARC数据有效，忽略其他数据
        // 2) 新记录中全部数据均有效
        // 3) 新记录中仅MARC根外的其他数据有效
        // 4) 删除MARC根
        // 5) 删除MARC根外的其他数据
        int MergeOldNewRecord(string strMarcSyntax)
        {

            return 0;
        }
         * */

        // 为册记录添加书目信息
        // return:
        //      -1  失败
        //      0   成功
        //      1   需要结束运行，result 结果已经设置好了
        int AddBiblioToSubRecords(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            string strBiblioXml,
            ref XmlDocument domOperLog,
            ref LibraryServerResult result,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            // 要对种和实体都进行锁定
            this.BiblioLocks.LockForWrite(strBiblioRecPath);
            try
            {
                // 探测书目记录有没有下属的实体记录(也顺便看看实体记录里面是否有流通信息)?
                List<DeleteEntityInfo> entityinfos = null;
                string strStyle = "return_record_xml";
                long lHitCount = 0;

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "channel == null";
                    return -1;
                }

                // TODO: 是否必须全局用户才能使用本功能？这样避免只能修改部分册记录

                // return:
                //      -2  not exist entity dbname
                //      -1  error
                //      >=0 含有流通信息的实体记录个数
                nRet = SearchChildEntities(
                    null,
                    channel,
                    strBiblioRecPath,
                    strStyle,
                    (Delegate_checkRecord)null, // sessioninfo.GlobalUser == false ? CheckItemRecord : (Delegate_checkRecord)null,
                    sessioninfo.GlobalUser == false ? sessioninfo.LibraryCodeList : null,
                    out lHitCount,
                    out entityinfos,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == -2)
                {
                    Debug.Assert(entityinfos.Count == 0, "");
                }

                // 如果有实体记录，则要求 setiteminfo 权限，才能修改册记录
                if (entityinfos != null && entityinfos.Count > 0)
                {
                    // 权限字符串
                    if (StringUtil.IsInList("setiteminfo,writerecord", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "为册记录设置书目信息的操作被拒绝。前用户不具备 setiteminfo 或 writerecord 权限，不能修改它们。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        // return result;
                        return 1;
                    }
                }

                // 为实体记录添加 biblio 元素
                // return:
                //      -1  error
                //      0   没有找到属于书目记录的任何实体记录，因此也就无从修改
                //      >0  实际修改的实体记录数
                nRet = AddBiblioToChildEntities(channel,
                    entityinfos,
                    strBiblioXml,
                    false,
                    domOperLog,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                this.BiblioLocks.UnlockForWrite(strBiblioRecPath);
            }
        }
    }

    /*
    public enum MergeType
    {
        MARC = 0x01,
        OTHER = 0x02,
    }*/

}
