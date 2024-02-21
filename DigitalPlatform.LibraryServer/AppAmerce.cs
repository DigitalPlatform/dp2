using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using static DigitalPlatform.LibraryServer.ReadersMonitor;

namespace DigitalPlatform.LibraryServer
{
    // 和违约金管理有关的功能
    public partial class LibraryApplication
    {
        // 结算
        public LibraryServerResult Settlement(
            SessionInfo sessioninfo,
            string strAction,
            string[] ids)
        {
            string strError = "";
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            strAction = strAction.ToLower();

            if (strAction == "settlement")
            {
                // 权限判断
                if (StringUtil.IsInList("settlement", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"结算操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 settlement 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (strAction == "undosettlement")
            {
                // 权限判断
                if (StringUtil.IsInList("undosettlement", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"撤销结算的操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 undosettlement 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (strAction == "delete")
            {
                // 权限判断
                if (StringUtil.IsInList("deletesettlement", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"删除结算记录的操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 deletesettlement 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else
            {
                strError = "无法识别的 strAction 参数值 '" + strAction + "'";
                goto ERROR1;
            }

            string strOperTime = this.Clock.GetClock();
            string strOperator = sessioninfo.UserID;

            //
            string strText = "";
            string strCount = "";

            strCount = "<maxCount>100</maxCount>";

            for (int i = 0; i < ids.Length; i++)
            {
                string strID = ids[i];

                if (i != 0)
                {
                    strText += "<operator value='OR' />";
                }

                strText += "<item><word>"
                    + StringUtil.GetXmlStringSimple(strID)
                    + "</word>"
                    + strCount
                    + "<match>exact</match><relation>=</relation><dataType>string</dataType>"
                    + "</item>";
            }

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(this.AmerceDbName + ":" + "ID")       // 2007/9/14
                + "'>" + strText
                + "<lang>zh</lang></target>";

            string strIds = String.Join(",", ids);

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "amerced",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "检索ID为 '" + strIds + "' 的违约金记录出错: " + strError;
                goto ERROR1;
            }

            if (lRet == 0)
            {
                strError = "没有找到id为 '" + strIds + "' 的违约金记录";
                goto ERROR1;
            }

            long lHitCount = lRet;

            long lStart = 0;
            long lPerCount = Math.Min(50, lHitCount);
            List<string> aPath = null;

            // 获得结果集，对逐个记录进行处理
            for (; ; )
            {
                lRet = channel.DoGetSearchResult(
                    "amerced",   // strResultSetName
                    lStart,
                    lPerCount,
                    "zh",
                    null,   // stop
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "未命中";
                    break;  // ??
                }

                // TODO: 要判断 aPath.Count == 0 跳出循环。否则容易进入死循环

                // 处理浏览结果
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strPath = aPath[i];

                    string strCurrentError = "";

                    // 结算一个交费记录
                    nRet = SettlementOneRecord(
                        sessioninfo.LibraryCodeList,
                        true,   // 要创建日志
                        channel,
                        strAction,
                        strPath,
                        strOperTime,
                        strOperator,
                        sessioninfo.ClientAddress,
                        out strCurrentError);
                    // 遇到一般出错应当继续处理
                    if (nRet == -1)
                    {
                        strError += strAction + "违约金记录 '" + strPath + "' 时发生错误: " + strCurrentError + "\r\n";
                    }
                    // 但是遇到日志空间满这样的错误就不能继续处理了
                    if (nRet == -2)
                    {
                        strError = strCurrentError;
                        goto ERROR1;
                    }
                }

                lStart += aPath.Count;
                if (lStart >= lHitCount || lPerCount <= 0)
                    break;
            }

            if (strError != "")
                goto ERROR1;

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 结算一个交费记录
        // parameters:
        //      strLibraryCodeList  当前操作者管辖的图书馆代码
        //      bCreateOperLog  是否创建日志
        //      strOperTime 结算的操作时间
        //      strOperator 结算的操作者
        // return:
        //      -2  致命出错，不宜再继续循环调用本函数
        //      -1  一般出错，可以继续循环调用本函数
        //      0   正常
        int SettlementOneRecord(
            string strLibraryCodeList,
            bool bCreateOperLog,
            RmsChannel channel,
            string strAction,
            string strAmercedRecPath,
            string strOperTime,
            string strOperator,
            string strClientAddress,
            out string strError)
        {
            strError = "";

            string strMetaData = "";
            byte[] amerced_timestamp = null;
            string strOutputPath = "";
            string strAmercedXml = "";

            // 准备日志DOM
            XmlDocument domOperLog = null;

            if (bCreateOperLog == true)
            {

            }

            int nRedoCount = 0;
        REDO:

            long lRet = channel.GetRes(strAmercedRecPath,
                out strAmercedXml,
                out strMetaData,
                out amerced_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获取违约金记录 '" + strAmercedRecPath + "' 时出错: " + strError;
                return -1;
            }

            XmlDocument amerced_dom = null;
            int nRet = LibraryApplication.LoadToDom(strAmercedXml,
                out amerced_dom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载违约金记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            string strLibraryCode = DomUtil.GetElementText(amerced_dom.DocumentElement, "libraryCode");
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                {
                    strError = $"{GetCurrentUserName(null)}未能管辖违约金记录 '{strAmercedRecPath}' 所在的馆代码 '{strLibraryCode}'";
                    return -1;
                }
            }

            if (bCreateOperLog == true)
            {
                domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");

                // 2012/10/2
                // 相关读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "libraryCode", strLibraryCode);

                DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                    "settlement");
                DomUtil.SetElementText(domOperLog.DocumentElement, "action",
                    strAction);


                // 在日志中记忆 id
                string strID = DomUtil.GetElementText(amerced_dom.DocumentElement,
                    "id");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "id", strID);
            }

            string strOldState = DomUtil.GetElementText(amerced_dom.DocumentElement,
                "state");

            if (strAction == "settlement")
            {
                if (strOldState != "amerced")
                {
                    strError = "结算操作前，记录状态必须为amerced。(但发现为'" + strOldState + "')";
                    return -1;
                }
                if (strOldState == "settlemented")
                {
                    strError = "结算操作前，记录状态已经为settlemented";
                    return -1;
                }
            }
            else if (strAction == "undosettlement")
            {
                if (strOldState != "settlemented")
                {
                    strError = "撤销结算操作前，记录状态必须为settlemented。(但发现为'" + strOldState + "')";
                    return -1;
                }
                if (strOldState == "amerced")
                {
                    strError = "撤销结算操作前，记录状态已经为settlemented";
                    return -1;
                }
            }
            else if (strAction == "delete")
            {
                if (strOldState != "settlemented")
                {
                    strError = "删除结算操作前，记录状态必须为settlemented。(但发现为'" + strOldState + "')";
                    return -1;
                }
            }
            else
            {
                strError = "无法识别的strAction参数值 '" + strAction + "'";
                return -1;
            }

            byte[] output_timestamp = null;

            if (bCreateOperLog == true)
            {
                // oldAmerceRecord
                XmlNode nodeOldAmerceRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
    "oldAmerceRecord", strAmercedXml);
                DomUtil.SetAttr(nodeOldAmerceRecord, "recPath", strAmercedRecPath);
            }

            if (strAction == "delete")
            {
                // 删除已结算违约金记录
                lRet = channel.DoDeleteRes(strAmercedRecPath,
                    amerced_timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoCount < 10)
                    {
                        nRedoCount++;
                        amerced_timestamp = output_timestamp;
                        goto REDO;
                    }
                    strError = "删除已结算违约金记录 '" + strAmercedRecPath + "' 失败: " + strError;
                    this.WriteErrorLog(strError);
                    return -1;
                }

                goto END1;  // 写日志
            }

            // 修改状态
            if (strAction == "settlement")
            {
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "state", "settlemented");


                // 清除两个信息
                DomUtil.DeleteElement(amerced_dom.DocumentElement,
                    "undoSettlementOperTime");
                DomUtil.DeleteElement(amerced_dom.DocumentElement,
                    "undoSettlementOperator");


                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperTime", strOperTime);
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperator", strOperator);
            }
            else
            {
                Debug.Assert(strAction == "undosettlement", "");

                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "state", "amerced");


                // 清除两个信息
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperTime", "");
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperator", "");


                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "undoSettlementOperTime", strOperTime);
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "undoSettlementOperator", strOperator);

            }

            if (bCreateOperLog == true)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    strOperator);   // 操作者
                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTime);   // 操作时间
            }


            // 保存回数据库
            lRet = channel.DoSaveTextRes(strAmercedRecPath,
                amerced_dom.OuterXml,
                false,
                "content", // ?????,ignorechecktimestamp
                amerced_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "写回违约金记录 '" + strAmercedRecPath + "' 时出错: " + strError;
                return -1;
            }

            if (bCreateOperLog == true)
            {
                // amerceRecord
                XmlNode nodeAmerceRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "amerceRecord", amerced_dom.OuterXml);
                DomUtil.SetAttr(nodeAmerceRecord, "recPath", strAmercedRecPath);
            }


        END1:
            if (bCreateOperLog == true)
            {
                if (this.Statis != null)
                {
                    if (strAction == "settlement")
                        this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "费用结算", "结算记录数", 1);
                    else if (strAction == "undosettlement")
                        this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "费用结算", "撤销结算记录数", 1);
                    else if (strAction == "delete")
                        this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "费用结算", "删除结算记录数", 1);
                }


                nRet = this.OperLog.WriteOperLog(domOperLog,
                    strClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "settlement() API 写入日志时发生错误: " + strError;
                    return -2;
                }
            }

            return 0;
        }

        // 交违约金/撤销交违约金
        // parameters:
        //      strReaderBarcode    如果功能是"undo"，可以将此参数设置为null。如果此参数不为null，则软件要进行核对，如果不是这个读者的已付违约金记录，则要报错
        //      strAmerceItemIdList id列表, 以逗号分割
        // 权限：需要有amerce/amercemodifyprice/amerceundo/amercemodifycomment等权限
        // 日志：
        //      要产生日志
        // return:
        //      result.Value    0 成功；1 部分成功(result.ErrorInfo中有信息)
        public LibraryServerResult Amerce(
            SessionInfo sessioninfo,
            string strFunction,
            string strReaderBarcodeParam,
            AmerceItem[] amerce_items,
            out AmerceItem[] failed_items,
            out string strReaderXml)
        {
            strReaderXml = "";
            failed_items = null;

            ParseOI(strReaderBarcodeParam, out string strPureReaderBarcodeParam, out string strOwnerInstitution);

            LibraryServerResult result = new LibraryServerResult();

            if (String.Compare(strFunction, "amerce", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amerce", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"交违约金操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 amerce 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (String.Compare(strFunction, "modifyprice", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amercemodifyprice", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"修改违约金额的操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 amercemodifyprice 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (String.Compare(strFunction, "modifycomment", true) == 0)
            {
                /*
                // 权限字符串
                if (StringUtil.IsInList("amercemodifycomment", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改违约金之注释的操作被拒绝。不具备 amercemodifycomment 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                 * */
            }

            if (String.Compare(strFunction, "undo", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amerceundo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"撤销交违约金操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 amerceundo 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (String.Compare(strFunction, "rollback", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amerce", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = $"撤回交违约金事务的操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 amerce 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (strFunction != "rollback")
            {
                // 看看amerce_items中是否有价格变更或注释变更的情况
                bool bHasNewPrice = false;
                bool bHasOverwriteComment = false;    // NewComment具有、并且为覆盖。也就是说包括NewPrice和NewComment同时具有的情况
                for (int i = 0; i < amerce_items.Length; i++)
                {
                    AmerceItem item = amerce_items[i];

                    // NewPrice域中有值
                    // TODO: 这里可以改进一下，如果 NewPrice 和原有金额没有变化，则不要求 amercemodifyprice 权限。或者滞后到 ModifyPrice() 函数内判断权限
                    if (String.IsNullOrEmpty(item.NewPrice) == false)
                    {
                        bHasNewPrice = true;
                    }

                    // NewComment域中有值
                    if (String.IsNullOrEmpty(item.NewComment) == false)
                    {
                        string strNewComment = item.NewComment;

                        bool bAppend = true;
                        if (string.IsNullOrEmpty(strNewComment) == false
                            && strNewComment[0] == '<')
                        {
                            bAppend = false;
                            strNewComment = strNewComment.Substring(1);
                        }
                        else if (string.IsNullOrEmpty(strNewComment) == false
                            && strNewComment[0] == '>')
                        {
                            bAppend = true;
                            strNewComment = strNewComment.Substring(1);
                        }

                        if (bAppend == false)
                            bHasOverwriteComment = true;
                    }
                }

                // 如果要变更价格，则需要额外的amercemodifyprice权限。
                // amercemodifyprice在功能amerce和modifyprice中都可能用到，关键是看是否提交了有新价格的参数
                if (bHasNewPrice == true)
                {
                    if (StringUtil.IsInList("amercemodifyprice", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = $"含有价格变更要求的交违约金操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 amercemodifyprice 权限。(仅仅具备 amerce 权限还不够的)";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (bHasOverwriteComment == true)
                {
                    // 如果有了amerce权限，则暗含有了amerceappendcomment的权限

                    if (StringUtil.IsInList("amercemodifycomment", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = $"含有违约金注释(覆盖型)变更要求的操作被拒绝。{SessionInfo.GetCurrentUserName(sessioninfo)}不具备 amercemodifycomment 权限。(仅仅具备 amerce 权限还不够的)";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            int nRet = 0;
            string strError = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            if (String.Compare(strFunction, "amerce", true) != 0
                && String.Compare(strFunction, "undo", true) != 0
                && String.Compare(strFunction, "modifyprice", true) != 0
                && String.Compare(strFunction, "modifycomment", true) != 0
                && String.Compare(strFunction, "rollback", true) != 0)
            {
                result.Value = -1;
                result.ErrorInfo = "未知的strFunction参数值 '" + strFunction + "'";
                result.ErrorCode = ErrorCode.InvalidParameter;
                return result;
            }

            // 如果是undo, 需要先检索出指定id的违约金库记录，然后从记录中得到<readerBarcode>，和参数核对
            if (String.Compare(strFunction, "undo", true) == 0)
            {
                // UNDO违约金交纳
                // parameters:
                //      strReaderBarcode    证条码号。可能包含机构代码
                // return:
                //      -1  error
                //      0   succeed
                //      1   部分成功。strError中有报错信息
                nRet = UndoAmerces(
                    sessioninfo,
                    strReaderBarcodeParam,
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2009/10/10 changed
                result.Value = nRet;
                if (nRet == 1)
                    result.ErrorInfo = strError;
                return result;
            }

            // 回滚
            // 2009/7/14
            if (String.Compare(strFunction, "rollback", true) == 0)
            {
                if (amerce_items != null)
                {
                    strError = "调用rollback功能时amerce_item参数必须为空";
                    goto ERROR1;
                }

                if (sessioninfo.AmerceIds == null
                    || sessioninfo.AmerceIds.Count == 0)
                {
                    strError = "当前没有可以rollback的违约金事项";
                    goto ERROR1;
                }

                // strReaderBarcode参数值一般为空即可。如果有值，则要求和SessionInfo对象中储存的最近一次的Amerce操作读者证条码号一致
                if (String.IsNullOrEmpty(strReaderBarcodeParam) == false)
                {
                    string refID = "";
                    nRet = this.ConvertReaderBarcodeListToRefIdList(
                        channel,
                        strPureReaderBarcodeParam,
                        out string strReaderRefIdString,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    refID = dp2StringUtil.GetRefIdValue(strReaderRefIdString);

                    if (sessioninfo.AmerceReaderRefID != refID)
                    {
                        strError = "调用rollback功能时strReaderBarcode参数和最近一次Amerce操作的读者证条码号不一致";
                        goto ERROR1;
                    }
                }

                amerce_items = new AmerceItem[sessioninfo.AmerceIds.Count];

                for (int i = 0; i < sessioninfo.AmerceIds.Count; i++)
                {
                    AmerceItem item = new AmerceItem();
                    item.ID = sessioninfo.AmerceIds[i];

                    amerce_items[i] = item;
                }

                // UNDO违约金交纳
                // parameters:
                //      strReaderBarcode    证条码号。可能包含机构代码
                // return:
                //      -1  error
                //      0   succeed
                //      1   部分成功。strError中有报错信息
                nRet = UndoAmerces(
                    sessioninfo,
                    $"@refID:{sessioninfo.AmerceReaderRefID}",
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;

                // 清空ids
                sessioninfo.AmerceIds = new List<string>();
                sessioninfo.AmerceReaderRefID = "";

                result.Value = 0;
                return result;
            }

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("Amerce 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strPureReaderBarcodeParam);

            try
            {
                // 读入读者记录
                strReaderXml = "";
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strPureReaderBarcodeParam,
                    out strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strPureReaderBarcodeParam + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                // 所操作的读者库的馆代码
                string strLibraryCode = "";

                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 获得读者库的馆代码
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = GetLibraryCode(
            strOutputReaderRecPath,
            out strLibraryCode,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 检查当前操作者是否管辖这个读者库，或者因为馆际互借关系可以连带管理这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.ExpandLibraryCodeList) == false)
                    {
                        strError = $"读者记录路径 '{strOutputReaderRecPath}' 从属的读者库不在{GetCurrentUserName(sessioninfo)}管辖范围内";
                        goto ERROR1;
                    }
                }

                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                var strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                    "barcode");
                // 2024/2/11
                var strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                    "refID");

                // 2021/3/3
                // 补充判断机构代码
                if (strOwnerInstitution != null)
                {
                    // return:
                    //      -1  出错
                    //      0   没有通过较验
                    //      1   通过了较验
                    nRet = VerifyPatronOI(
                        strOutputReaderRecPath,
                        strLibraryCode,
                        readerdom,
                        strOwnerInstitution,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                        return result;
                    }
                }

                // 准备日志DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "operation",
                    "amerce");

                // 具体动作
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "action", strFunction.ToLower());

                // 读者证条码号
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerBarcode", strReaderBarcode);
                // 2024/2/11
                if (string.IsNullOrEmpty(strReaderRefID) == false)
                    DomUtil.SetElementText(domOperLog.DocumentElement,
    "readerRefID", strReaderRefID);

                //
                List<string> AmerceRecordXmls = null;
                List<string> CreatedNewPaths = null;

                List<string> Ids = null;

                string strOperTimeString = this.Clock.GetClock();   // RFC1123格式


                bool bReaderDomChanged = false; // 读者dom是否发生了变化，需要回存

                {
                    // 在日志中保留旧的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldReaderRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);
                }

                if (String.Compare(strFunction, "modifyprice", true) == 0)
                {
                    /*
                    // 在日志中保留旧的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldReaderRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);
                     * */

                    nRet = ModifyPrice(ref readerdom,
                        amerce_items,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet != 0)
                    {
                        bReaderDomChanged = true;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "违约金",
                            "修改次",
                            nRet);
                    }
                    else
                    {
                        // 如果一个事项也没有发生修改，则需要返回错误信息，以引起前端的警觉
                        strError = "警告：没有任何事项的价格(和注释)被修改。";
                        goto ERROR1;
                    }

                    goto SAVERECORD;
                }

                if (String.Compare(strFunction, "modifycomment", true) == 0)
                {
                    /*
                    // 在日志中保留旧的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldReaderRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);
                     * */

                    nRet = ModifyComment(
                        ref readerdom,
                        amerce_items,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet != 0)
                    {
                        bReaderDomChanged = true;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "违约金之注释",
                            "修改次",
                            nRet);
                    }
                    else
                    {
                        // 如果一个事项也没有发生修改，则需要返回错误信息，以引起前端的警觉
                        strError = "警告：没有任何事项的注释被修改。";
                        goto ERROR1;
                    }

                    goto SAVERECORD;
                }

                List<string> NotFoundIds = null;
                Ids = null;

                // 交违约金：在读者记录中去除所选的<overdue>元素，并且构造一批新记录准备加入违约金库
                // return:
                //      -1  error
                //      0   读者dom没有变化
                //      1   读者dom发生了变化
                nRet = DoAmerceReaderXml(
                    sessioninfo,
                    channel,
                    strLibraryCode,
                    ref readerdom,
                    strOutputReaderRecPath,
                    amerce_items,
                    sessioninfo.UserID,
                    strOperTimeString,
                    out AmerceRecordXmls,
                    out NotFoundIds,
                    out Ids,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                    bReaderDomChanged = true;

                // 在违约金数据库中创建若干新的违约金记录
                // parameters:
                //      AmerceRecordXmls    需要写入的新记录的数组
                //      CreatedNewPaths 已经创建的新记录的路径数组。可以用于Undo(删除刚刚创建的新记录)
                nRet = CreateAmerceRecords(
                    channel,
                    AmerceRecordXmls,
                    out CreatedNewPaths,
                    out strError);
                if (nRet == -1)
                {
                    // undo已经写入的部分记录
                    if (CreatedNewPaths != null
                        && CreatedNewPaths.Count != 0)
                    {
                        string strNewError = "";
                        nRet = DeleteAmerceRecords(
                            sessioninfo.Channels,
                            CreatedNewPaths,
                            out strNewError);
                        if (nRet == -1)
                        {
                            string strList = "";
                            for (int i = 0; i < CreatedNewPaths.Count; i++)
                            {
                                if (strList != "")
                                    strList += ",";
                                strList += CreatedNewPaths[i];
                            }
                            strError = "在创建新的违约金记录的过程中发生错误: " + strError + "。在Undo新创建的违约金记录的过程中，又发生错误: " + strNewError + ", 请系统管理员手工删除新创建的罚款记录: " + strList;
                            goto ERROR1;
                        }
                    }

                    goto ERROR1;
                }

            SAVERECORD:

                // 为写回读者、册记录做准备
                // byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

#if NO
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif
                long lRet = 0;

                if (bReaderDomChanged == true)
                {
                    strReaderXml = readerdom.OuterXml;

                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        strReaderXml,
                        false,
                        "content,ignorechecktimestamp",
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    // id list
                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "idList", strAmerceItemIdList);
                     * */
                    WriteAmerceItemList(domOperLog, amerce_items);


                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode", strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "itemBarcodeList", strItemBarcodeList);
                     */

                    // 仅当功能为amerce时，才把被修改的实体记录写入日志。
                    if (String.Compare(strFunction, "amerce", true) == 0)
                    {
                        Debug.Assert(AmerceRecordXmls.Count == CreatedNewPaths.Count, "");

                        // 写入多个重复的<amerceRecord>元素
                        for (int i = 0; i < AmerceRecordXmls.Count; i++)
                        {
                            XmlNode nodeAmerceRecord = domOperLog.CreateElement("amerceRecord");
                            domOperLog.DocumentElement.AppendChild(nodeAmerceRecord);
                            nodeAmerceRecord.InnerText = AmerceRecordXmls[i];

                            DomUtil.SetAttr(nodeAmerceRecord, "recPath", CreatedNewPaths[i]);
                            /*
                            DomUtil.SetElementText(domOperLog.DocumentElement,
                                "record", AmerceRecordXmls[i]);
                             **/

                            if (this.Statis != null)
                                this.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "违约金",
                                "给付次",
                                1);

                            {
                                string strPrice = "";
                                // 取出违约金记录中的金额数字
                                nRet = GetAmerceRecordField(AmerceRecordXmls[i],
                                    "price",
                                    out strPrice,
                                    out strError);
                                if (nRet != -1)
                                {
                                    string strPrefix = "";
                                    string strPostfix = "";
                                    double fValue = 0.0;
                                    // 分析价格参数
                                    nRet = ParsePriceUnit(strPrice,
                                        out strPrefix,
                                        out fValue,
                                        out strPostfix,
                                        out strError);
                                    if (nRet != -1)
                                    {
                                        if (this.Statis != null)
                                            this.Statis.IncreaseEntryValue(
                                            strLibraryCode,
                                            "违约金",
                                            "给付元",
                                            fValue);
                                    }
                                    else
                                    {
                                        // 2012/11/15
                                        this.WriteErrorLog("累计 违约金 给付元 [" + strPrice + "] 时出错: " + strError);
                                    }
                                }
                            }
                        } // end of for
                    }

                    // 最新的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "readerRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);

                    string strOperTime = this.Clock.GetClock();
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);   // 操作者
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);   // 操作时间

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Amerce() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }
                }

                // 记忆下最近一次Amerce操作的ID和读者证条码号
                if (strFunction != "rollback"
                    && Ids != null
                    && Ids.Count != 0)
                {
                    sessioninfo.AmerceReaderRefID = strReaderRefID;
                    sessioninfo.AmerceIds = Ids;
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strPureReaderBarcodeParam);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Amerce 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 根据AmerceItem数组，修改readerdom中的<amerce>元素中的价格price属性。
        // 为功能"modifyprice"服务。
        int ModifyPrice(ref XmlDocument readerdom,
            AmerceItem[] amerce_items,
            out string strError)
        {
            strError = "";
            int nChangedCount = 0;

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                // 遇到NewPrice域值为空的，直接跳过。
                // 这说明，不接受修改价格为完全空的字符串。
                if (String.IsNullOrEmpty(item.NewPrice) == true)
                {
                    if (String.IsNullOrEmpty(item.NewComment) == false)
                    {
                        strError = "不能用modifyprice子功能来单独修改注释(而不修改价格)，请改用appendcomment和modifycomment子功能";
                        return -1;
                    }

                    continue;
                }

                // 通过id值在读者记录中找到对应的<overdue>元素
                XmlNode nodeOverdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + item.ID + "']");
                if (nodeOverdue == null)
                {
                    strError = "ID为 '" + item.ID + "' 的<overdues/overdue>元素没有找到...";
                    return -1;
                }

                string strOldPrice = DomUtil.GetAttr(nodeOverdue, "price");

                if (strOldPrice != item.NewPrice)
                {
                    // 修改price属性
                    DomUtil.SetAttr(nodeOverdue, "price", item.NewPrice);
                    nChangedCount++;

                    // 增补注释
                    string strNewComment = item.NewComment;
                    string strExistComment = DomUtil.GetAttr(nodeOverdue, "comment");

                    // 处理追加标志
                    bool bAppend = true;
                    if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '<')
                    {
                        bAppend = false;
                        strNewComment = strNewComment.Substring(1);
                    }
                    else if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '>')
                    {
                        bAppend = true;
                        strNewComment = strNewComment.Substring(1);
                    }

                    if (String.IsNullOrEmpty(strNewComment) == false
                        && bAppend == true)
                    {
                        string strText = "";
                        if (String.IsNullOrEmpty(strExistComment) == false)
                            strText += strExistComment;
                        if (String.IsNullOrEmpty(strNewComment) == false)
                        {
                            if (String.IsNullOrEmpty(strText) == false)
                                strText += "；";
                            strText += strNewComment;
                        }

                        DomUtil.SetAttr(nodeOverdue, "comment", strText);
                    }
                    else if (bAppend == false)
                    {
                        DomUtil.SetAttr(nodeOverdue, "comment", strNewComment);
                    }
                }
            }

            return nChangedCount;
        }

        // 2008/6/19
        // 根据AmerceItem数组，修改readerdom中的<amerce>元素中的comment属性。
        // 为功能"modifycomment"服务。
        int ModifyComment(
            ref XmlDocument readerdom,
            AmerceItem[] amerce_items,
            out string strError)
        {
            strError = "";
            int nChangedCount = 0;

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                // 不能同时修改价格。
                if (String.IsNullOrEmpty(item.NewPrice) == false)
                {
                    strError = "不能用modifycomment子功能来修改价格，请改用modifyprice子功能";
                    return -1;
                }

                /*
                // 遇到NewComment域值为空、并且为追加的，直接跳过
                if (String.IsNullOrEmpty(item.NewComment) == true
                    && strFunction == "appendcomment")
                {
                    continue;
                }*/

                // 通过id值在读者记录中找到对应的<overdue>元素
                XmlNode nodeOverdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + item.ID + "']");
                if (nodeOverdue == null)
                {
                    strError = "ID为 '" + item.ID + "' 的<overdues/overdue>元素没有找到...";
                    return -1;
                }


                {
                    string strExistComment = DomUtil.GetAttr(nodeOverdue, "comment");

                    // 增补或修改注释
                    string strNewComment = item.NewComment;

                    // 处理追加标志
                    bool bAppend = true;
                    if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '<')
                    {
                        bAppend = false;
                        strNewComment = strNewComment.Substring(1);
                    }
                    else if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '>')
                    {
                        bAppend = true;
                        strNewComment = strNewComment.Substring(1);
                    }

                    if (String.IsNullOrEmpty(strNewComment) == false
                        && bAppend == true)
                    {
                        string strText = "";
                        if (String.IsNullOrEmpty(strExistComment) == false)
                            strText += strExistComment;
                        if (String.IsNullOrEmpty(strNewComment) == false)
                        {
                            if (String.IsNullOrEmpty(strText) == false)
                                strText += "；";
                            strText += strNewComment;
                        }

                        DomUtil.SetAttr(nodeOverdue, "comment", strText);
                        nChangedCount++;
                    }
                    else if (bAppend == false)
                    {
                        DomUtil.SetAttr(nodeOverdue, "comment", strNewComment);
                        nChangedCount++;    // BUG!!! 2011/12/1前少了这句话
                    }
                }
            }

            return nChangedCount;
        }

        // 从日志DOM中读出违约金事项信息
        public static AmerceItem[] ReadAmerceItemList(XmlDocument domOperLog)
        {
            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceItems/amerceItem");
            AmerceItem[] results = new AmerceItem[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");
                string strNewPrice = DomUtil.GetAttr(node, "newPrice");
                string strComment = DomUtil.GetAttr(node, "newComment");

                results[i] = new AmerceItem();
                results[i].ID = strID;
                results[i].NewPrice = strNewPrice;
                results[i].NewComment = strComment;    // 2007/4/17
            }

            return results;
        }

        // 在日志DOM中写入违约金事项信息
        static void WriteAmerceItemList(XmlDocument domOperLog,
            AmerceItem[] amerce_items)
        {
            XmlNode root = domOperLog.CreateElement("amerceItems");
            domOperLog.DocumentElement.AppendChild(root);

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                XmlNode node = domOperLog.CreateElement("amerceItem");
                root.AppendChild(node);

                DomUtil.SetAttr(node, "id", item.ID);

                if (String.IsNullOrEmpty(item.NewPrice) == false)
                    DomUtil.SetAttr(node, "newPrice", item.NewPrice);

                // 2007/4/17
                if (String.IsNullOrEmpty(item.NewComment) == false)
                    DomUtil.SetAttr(node, "newComment", item.NewComment);

            }

            /*

            // id list
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "idList", strAmerceItemIdList);
            */
        }

        // UNDO违约金交纳
        // parameters:
        //      strReaderBarcode    证条码号。可能包含机构代码
        // return:
        //      -1  error
        //      0   succeed
        //      1   部分成功。strError中有报错信息，failed_item中有那些没有被处理的item的列表
        int UndoAmerces(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            AmerceItem[] amerce_items,
            out AmerceItem[] failed_items,
            out string strReaderXml,
            out string strError)
        {
            strError = "";
            strReaderXml = "";
            failed_items = null;
            int nErrorCount = 0;

            List<string> OverdueStrings = new List<string>();
            List<string> AmercedRecPaths = new List<string>();

            // string[] ids = strAmercedItemIdList.Split(new char[] { ',' });
            List<AmerceItem> failed_list = new List<AmerceItem>();
            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                /*
                string strID = ids[i].Trim();
                 * */
                if (String.IsNullOrEmpty(item.ID) == true)
                    continue;

                // parameters:
                //      strReaderBarcodeParam 证条码号。可能包含机构代码
                int nRet = UndoOneAmerce(sessioninfo,
                    strReaderBarcode,
                    item.ID,
                    out strReaderXml,
                    out string strTempError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ";\r\n";
                    strError += strTempError;
                    nErrorCount++;
                    // return -1;
                    failed_list.Add(item);
                }
            }

            // 每个ID都发生了错误
            if (nErrorCount >= amerce_items.Length)
                return -1;

            // 部分发生错误
            if (nErrorCount > 0)
            {
                failed_items = new AmerceItem[failed_list.Count];
                failed_list.CopyTo(failed_items);

                strError = "操作部分成功。(共提交了 " + amerce_items.Length + " 个事项，发生错误的有 " + nErrorCount + " 个) \r\n" + strError;
                return 1;
            }

            return 0;
        }

        // Undo一个已交费记录
        // parameters:
        //      strReaderBarcodeParam 证条码号。可能包含机构代码
        int UndoOneAmerce(SessionInfo sessioninfo,
            string strReaderBarcodeParam,
            string strAmercedItemId,
            out string strReaderXml,
            out string strError)
        {
            strError = "";
            strReaderXml = "";

            ParseOI(strReaderBarcodeParam, out string strReaderBarcode, out string strOwnerInstitution);

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strReaderRefID = "";
            int nRet = this.ConvertReaderBarcodeListToRefIdList(channel,
                strReaderBarcode,
                out string strReaderRefIdString,
                out strError);
            if (nRet != -1)
                strReaderRefID = dp2StringUtil.GetRefIdValue(strReaderRefIdString);

            long lRet = 0;

            string strFrom = "ID";
            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(this.AmerceDbName + ":" + strFrom)       // 2007/9/14
                + "'><item><word>"
                + strAmercedItemId + "</word><match>" + "exact" + "</match><relation>=</relation><dataType>string</dataType><maxCount>100</maxCount></item><lang>" + "zh" + "</lang></target>";

            lRet = channel.DoSearch(strQueryXml,
                "amerced",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录出错: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "没有找到ID为 '" + strAmercedItemId + "' 的已付违约金记录";
                return -1;
            }

            lRet = channel.DoGetSearchResult("amerced",
                100,
                "zh",
                null,
                out List<string> aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录，获取浏览格式阶段出错: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录，已检索命中，但是获取浏览格式没有找到";
                return -1;
            }

            if (aPath.Count == 0)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录，已检索命中，但是获取浏览格式没有找到";
                return -1;
            }

            if (aPath.Count > 1)
            {
                strError = "ID为 '" + strAmercedItemId + "' 的已付违约金记录检索出多条。请系统管理员及时更正此错误。";
                return -1;
            }

            string strAmercedRecPath = aPath[0];

            lRet = channel.GetRes(strAmercedRecPath,
                out string strAmercedXml,
                out string strMetaData,
                out byte[] amerced_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获取已付违约金记录 '" + strAmercedRecPath + "' 时出错: " + strError;
                return -1;
            }

            // 将违约金记录格式转换为读者记录中的<overdue>元素格式
            // parameters:
            //      strReaderKey    [out] 返回读者证条码号或者 @refID:xxx 形态
            // return:
            //      -1  error
            //      0   strAmercedXml中<state>元素的值为*非*"settlemented"
            //      1   strAmercedXml中<state>元素的值为"settlemented"
            nRet = ConvertAmerceRecordToOverdueString(strAmercedXml,
                out string strOutputReaderKey,
                out string strOverdueString,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                strError = "ID为 " + strAmercedItemId + " (路径为 '" + strOutputPath + "' ) 的违约金库记录其状态为 已结算(settlemented)，不能撤回交费操作";
                return -1;
            }

            // 如果 strOutputReaderKey 值非空，则要检查一下检索出来的已付违约金记录是否真的属于这个读者
            if (string.IsNullOrEmpty(strOutputReaderKey) == false)
            {
                if (MatchReaderKey(strOutputReaderKey,
                    strReaderBarcode,
                    strReaderRefID) == false)
                {
                    strError = "ID为 '" + strAmercedItemId + "' 的已付违约金记录，并不是属于所指定的读者 '" + dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID) + "'，而是属于另一读者 '" + strOutputReaderKey + "'";
                    return -1;
                }
                /*
                if (string.IsNullOrEmpty(strReaderBarcode) == false
                    && strOutputReaderKey.StartsWith("@") == false
                    && strReaderBarcode != strOutputReaderKey)
                {
                    strError = "ID为 '" + strAmercedItemId + "' 的已付违约金记录，并不是属于所指定的读者 '" + strReaderBarcode + "'，而是属于另一读者 '" + strOutputReaderKey + "'";
                    return -1;
                }
                else if (string.IsNullOrEmpty(strReaderRefIdString) == false
    && strOutputReaderKey.StartsWith("@refID:") == false
    && strReaderRefIdString != strOutputReaderKey)
                {
                    strError = "ID为 '" + strAmercedItemId + "' 的已付违约金记录，并不是属于所指定的读者 '" + strReaderRefIdString + "'，而是属于另一读者 '" + strOutputReaderKey + "'";
                    return -1;
                }
                */
            }

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("UndoOneAmerce 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try
            {
                // 读入读者记录
                strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;

                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList,
            out strLibraryCode) == false)
                    {
                        // TOOD: 进一步检查册条码号代表的册记录是否被当前用户管辖
                        // 取出违约金记录中的金额数字
                        nRet = GetAmerceRecordField(strAmercedXml,
                            "itemBarcode",
                            out string strItemBarcode,
                            out strError);
                        // 进一步判断册记录是否在当前用户管辖范围内
                        if (string.IsNullOrEmpty(strItemBarcode) == false)
                        {
                            // return:
                            //      -1  errpr
                            //      0   不在控制范围
                            //      1   在控制范围
                            nRet = IsItemInControl(
                                sessioninfo,
                                // channel,
                                strItemBarcode,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = $"读者记录路径 '{strOutputReaderRecPath}' 从属的读者库超出{GetCurrentUserName(sessioninfo)}管辖范围，并且在尝试检索册记录 '{strItemBarcode}' 时遇到问题: {strError}";
                                goto ERROR1;
                            }
                            if (nRet == 0)
                            {
                                strError = $"读者记录路径 '{strOutputReaderRecPath}' 从属的读者库不在{GetCurrentUserName(sessioninfo)}管辖范围内";
                                goto ERROR1;
                            }
                        }
                        else
                        {
                            strError = $"读者记录路径 '{strOutputReaderRecPath}' 从属的读者库不在{GetCurrentUserName(sessioninfo)}管辖范围内";
                            goto ERROR1;
                        }
                    }
                }

                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 2021/3/3
                if (strOwnerInstitution != null)
                {
                    // return:
                    //      -1  出错
                    //      0   没有通过较验
                    //      1   通过了较验
                    nRet = VerifyPatronOI(
                        strOutputReaderRecPath,
                        strLibraryCode,
                        readerdom,
                        strOwnerInstitution,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;
                    /*
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                        return result;
                    }
                    */
                }

                // 准备日志DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "libraryCode",
                    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                    "amerce");

                bool bReaderDomChanged = false;

                // 修改读者记录
                // 增添超期信息
                if (String.IsNullOrEmpty(strOverdueString) != true)
                {
                    XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                    fragment.InnerXml = strOverdueString;

                    // 看看根下面是否有overdues元素
                    XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
                    if (root == null)
                    {
                        root = readerdom.CreateElement("overdues");
                        readerdom.DocumentElement.AppendChild(root);
                    }

                    // 2008/11/11
                    // undo交押金
                    XmlNode node_added = root.AppendChild(fragment);
                    bReaderDomChanged = true;

                    Debug.Assert(node_added != null, "");
                    string strReason = DomUtil.GetAttr(node_added, "reason");
                    if (strReason == "押金。")
                    {
                        string strPrice = "";

                        strPrice = DomUtil.GetAttr(node_added, "newPrice");
                        if (String.IsNullOrEmpty(strPrice) == true)
                            strPrice = DomUtil.GetAttr(node_added, "price");
                        else
                        {
                            Debug.Assert(strPrice.IndexOf('%') == -1, "从newPrice属性中取出来的价格字符串，岂能包含%符号");
                        }

                        if (String.IsNullOrEmpty(strPrice) == false)
                        {
                            // 需要从<foregift>元素中减去这个价格
                            string strContent = DomUtil.GetElementText(readerdom.DocumentElement,
                                "foregift");

                            string strNegativePrice = "";
                            // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                            // parameters:
                            //      bSum    是否要顺便汇总? true表示要汇总
                            nRet = PriceUtil.NegativePrices(strPrice,
                                false,
                                out strNegativePrice,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "反转价格字符串 '" + strPrice + "时发生错误: " + strError;
                                goto ERROR1;
                            }

                            strContent = PriceUtil.JoinPriceString(strContent, strNegativePrice);

                            DomUtil.SetElementText(readerdom.DocumentElement,
                                "foregift",
                                strContent);
                            bReaderDomChanged = true;
                        }
                    }
                }

                if (bReaderDomChanged == true)
                {
                    byte[] output_timestamp = null;

                    strReaderXml = readerdom.OuterXml;
                    // 野蛮写入
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        strReaderXml,
                        false,
                        "content,ignorechecktimestamp", // ?????
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    int nRedoDeleteCount = 0;
                REDO_DELETE:
                    // 删除已付违约金记录
                    lRet = channel.DoDeleteRes(strAmercedRecPath,
                        amerced_timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                            && nRedoDeleteCount < 10)
                        {
                            nRedoDeleteCount++;
                            amerced_timestamp = output_timestamp;
                            goto REDO_DELETE;
                        }
                        strError = "删除已付违约金记录 '" + strAmercedRecPath + "' 失败: " + strError;
                        this.WriteErrorLog(strError);
                        goto ERROR1;
                    }

                    // 具体动作
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "action", "undo");

                    // id list
                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "idList", strAmercedItemId);
                     * */
                    AmerceItem[] amerce_items = new AmerceItem[1];
                    amerce_items[0] = new AmerceItem();
                    amerce_items[0].ID = strAmercedItemId;
                    WriteAmerceItemList(domOperLog,
                        amerce_items);


                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode", strReaderBarcode);
                    // 2024/2/11
                    if (string.IsNullOrEmpty(strReaderRefID) == false)
                        DomUtil.SetElementText(domOperLog.DocumentElement,
        "readerRefID", strReaderRefID);

                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "amerceItemID", strAmercedItemId);
                     */

                    // 删除掉的违约金记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "amerceRecord", strAmercedXml);
                    DomUtil.SetAttr(node, "recPath", strAmercedRecPath);

                    // 最新的读者记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);


                    string strOperTime = this.Clock.GetClock();
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);   // 操作者
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);   // 操作时间

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Amerce() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(strLibraryCode,
                        "违约金",
                        "取消次",
                        1);

                    {
                        string strPrice = "";
                        // 取出违约金记录中的金额数字
                        nRet = GetAmerceRecordField(strAmercedXml,
                            "price",
                            out strPrice,
                            out strError);
                        if (nRet != -1)
                        {
                            string strPrefix = "";
                            string strPostfix = "";
                            double fValue = 0.0;
                            // 分析价格参数
                            nRet = ParsePriceUnit(strPrice,
                                out strPrefix,
                                out fValue,
                                out strPostfix,
                                out strError);
                            if (nRet != -1)
                            {
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "违约金",
                                    "取消元",
                                    fValue);
                            }
                        }
                    }
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("UndoOneAmerce 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif

            }

            return 0;
        ERROR1:
            return -1;
        }

        // 删除刚刚创建的新违约金记录
        int DeleteAmerceRecords(
            RmsChannelCollection channels,
            List<string> CreatedNewPaths,
            out string strError)
        {
            strError = "";

            RmsChannel channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            for (int i = 0; i < CreatedNewPaths.Count; i++)
            {
                string strPath = CreatedNewPaths[i];

                byte[] timestamp = null;
                byte[] output_timestamp = null;
                int nRedoCount = 0;
            REDO:

                long lRet = channel.DoDeleteRes(strPath,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoCount < 5) // 重试次数小于5次
                    {
                        timestamp = output_timestamp;
                        nRedoCount++;
                        goto REDO;
                    }

                    return -1;
                }

            }


            return 0;
        }

        // 在违约金数据库中创建若干新的违约金记录
        // parameters:
        //      AmerceRecordXmls    需要写入的新记录的数组
        //      CreatedNewPaths 已经创建的新记录的路径数组。可以用于Undo(删除刚刚创建的新记录)
        int CreateAmerceRecords(
            RmsChannel channel,
            List<string> AmerceRecordXmls,
            out List<string> CreatedNewPaths,
            out string strError)
        {
            strError = "";
            CreatedNewPaths = new List<string>();
            long lRet = 0;

            if (string.IsNullOrEmpty(this.AmerceDbName) == true)
            {
                strError = "尚未配置违约金库名";
                return -1;
            }

            for (int i = 0; i < AmerceRecordXmls.Count; i++)
            {
                string strXml = AmerceRecordXmls[i];

                string strPath = this.AmerceDbName + "/?";

                string strOutputPath = "";
                byte[] timestamp = null;
                byte[] output_timestamp = null;

                // 写新记录
                lRet = channel.DoSaveTextRes(
                    strPath,
                    strXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                CreatedNewPaths.Add(strOutputPath);
            }

            return 0;
        }

        // 取出违约金记录中的金额数字
        static int GetAmerceRecordField(string strAmercedXml,
            string strElementName,
            out string strPrice,
            out string strError)
        {
            strPrice = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strAmercedXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            strPrice = DomUtil.GetElementText(dom.DocumentElement,
                strElementName);  // "price"
            return 0;
        }

        // 将违约金记录格式转换为读者记录中的<overdue>元素格式
        // parameters:
        //      strReaderKey    [out] 返回读者证条码号或者 @refID:xxx 形态
        // return:
        //      -1  error
        //      0   strAmercedXml中<state>元素的值为*非*"settlemented"
        //      1   strAmercedXml中<state>元素的值为"settlemented"
        public static int ConvertAmerceRecordToOverdueString(string strAmercedXml,
            out string strReaderKey,
            out string strOverdueString,
            out string strError)
        {
            strReaderKey = "";
            strOverdueString = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strAmercedXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "itemBarcode");
            // 2024/2/11
            var strItemRefID = DomUtil.GetElementText(dom.DocumentElement,
                "itemRefID");
            // 注: 早期版本里面违约金记录里面缺乏 itemRefID 元素

            string strItemRecPath = DomUtil.GetElementText(dom.DocumentElement,
                "itemRecPath");
            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "readerBarcode");
            string strReaderRefID = DomUtil.GetElementText(dom.DocumentElement,
                "readerRefID");
            strReaderKey = strReaderBarcode;
            if (string.IsNullOrEmpty(strReaderRefID) == false)
                strReaderKey = $"@refID:{strReaderRefID}";

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            string strID = DomUtil.GetElementText(dom.DocumentElement,
                "id");
            string strReason = DomUtil.GetElementText(dom.DocumentElement,
                "reason");

            // 2007/12/17
            string strOverduePeriod = DomUtil.GetElementText(dom.DocumentElement,
                "overduePeriod");

            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strOriginPrice = DomUtil.GetElementText(dom.DocumentElement,
                "originPrice");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");
            string strBorrowOperator = DomUtil.GetElementText(dom.DocumentElement,
                "borrowOperator");  // 2006/3/27

            string strReturnDate = DomUtil.GetElementText(dom.DocumentElement,
                "returnDate");
            string strReturnOperator = DomUtil.GetElementText(dom.DocumentElement,
                "returnOperator");

            // 2008/6/23
            string strPauseStart = DomUtil.GetElementText(dom.DocumentElement,
                "pauseStart");

            // 写入DOM
            XmlDocument domOutput = new XmlDocument();
            domOutput.LoadXml("<overdue />");
            XmlElement nodeOverdue = domOutput.DocumentElement;

            DomUtil.SetAttr(nodeOverdue, "barcode", strItemBarcode);
            if (String.IsNullOrEmpty(strItemRecPath) == false)
                DomUtil.SetAttr(nodeOverdue, "recPath", strItemRecPath);
            // 2024/2/11
            if (string.IsNullOrEmpty(strItemRefID) == false)
                nodeOverdue.SetAttribute("refID", strItemRefID);
            // 2016/9/5
            if (String.IsNullOrEmpty(strLocation) == false)
                DomUtil.SetAttr(nodeOverdue, "location", strLocation);

            DomUtil.SetAttr(nodeOverdue, "reason", strReason);

            // 2007/12/17
            if (String.IsNullOrEmpty(strOverduePeriod) == false)
                DomUtil.SetAttr(nodeOverdue, "overduePeriod", strOverduePeriod);

            if (String.IsNullOrEmpty(strOriginPrice) == false)
            {
                DomUtil.SetAttr(nodeOverdue, "price", strOriginPrice);
                DomUtil.SetAttr(nodeOverdue, "newPrice", strPrice);
            }
            else
                DomUtil.SetAttr(nodeOverdue, "price", strPrice);

            // 撤回的时候不丢失注释。因为已经无法分辨哪次追加的注释，所以原样保留。
            // 2007/4/19
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(nodeOverdue, "comment", strComment);

            // TODO: 这里值得研究一下。如果AmerceItem.Comment能覆盖数据中的comment信息，
            // 那么撤回的时候就不要丢失注释。

            DomUtil.SetAttr(nodeOverdue, "borrowDate", strBorrowDate);
            DomUtil.SetAttr(nodeOverdue, "borrowPeriod", strBorrowPeriod);
            DomUtil.SetAttr(nodeOverdue, "returnDate", strReturnDate);
            DomUtil.SetAttr(nodeOverdue, "borrowOperator", strBorrowOperator);
            DomUtil.SetAttr(nodeOverdue, "operator", strReturnOperator);
            DomUtil.SetAttr(nodeOverdue, "id", strID);

            // 2008/6/23
            if (String.IsNullOrEmpty(strPauseStart) == false)
                DomUtil.SetAttr(nodeOverdue, "pauseStart", strPauseStart);

            strOverdueString = nodeOverdue.OuterXml;

            if (strState == "settlemented")
                return 1;

            return 0;
        }

        // 将读者记录中的<overdue>元素和属性转换为违约金库的记录格式
        // parameters:
        //      strLibraryCode  读者记录从属的馆代码
        //      strReaderBarcode    读者证条码号
        //      strReaderRefID      读者参考 ID
        //      strState    一般为"amerced"，表示尚未结算
        //      strNewPrice 例外的价格。如果为空，则表示沿用原来的价格。
        //      strComment  前端给出的注释。
        int ConvertOverdueStringToAmerceRecord(
            RmsChannel channel,
            XmlElement nodeOverdue,
            string strLibraryCode,
            string strReaderBarcode,
            string strReaderRefID,  // 2024/2/11
            string strState,
            string strNewPrice,
            string strNewComment,
            string strOperator,
            string strOperTime,
            string strForegiftPrice,    // 来自读者记录<foregift>元素内的价格字符串
            out string strFinalPrice,   // 最终使用的价格字符串
            out string strAmerceRecord,
            out string strError)
        {
            strAmerceRecord = "";
            strError = "";
            strFinalPrice = "";
            int nRet = 0;

            string strItemBarcode = DomUtil.GetAttr(nodeOverdue, "barcode");

            // 2024/2/11
            var strItemRefID = nodeOverdue.GetAttribute("refID");
            // 注: 较早版本的 overdue 元素缺乏 refID 属性
            if (string.IsNullOrEmpty(strItemRefID))
            {
                nRet = this.ConvertItemBarcodeListToRefIdList(
                    channel,
                    strItemBarcode,
                    out string strItemRefIdString,
                    out strError);
                if (nRet != -1)
                    strItemRefID = dp2StringUtil.GetRefIdValue(strItemRefIdString);
            }

            string strItemRecPath = DomUtil.GetAttr(nodeOverdue, "recPath");
            string strLocation = DomUtil.GetAttr(nodeOverdue, "location");

            string strReason = DomUtil.GetAttr(nodeOverdue, "reason");

            // 2007/12/17
            string strOverduePeriod = DomUtil.GetAttr(nodeOverdue, "overduePeriod");

            string strPrice = "";
            string strOriginPrice = "";

            if (String.IsNullOrEmpty(strNewPrice) == true)
                strPrice = DomUtil.GetAttr(nodeOverdue, "price");
            else
            {
                strPrice = strNewPrice;
                strOriginPrice = DomUtil.GetAttr(nodeOverdue, "price");
            }

            // 2008/11/15
            // 看看价格字符串是否为宏?
            if (strPrice == "%return_foregift_price%")
            {
                // 记忆下取宏的变化
                if (String.IsNullOrEmpty(strOriginPrice) == true)
                    strOriginPrice = strPrice;

                // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                // parameters:
                //      bSum    是否要顺便汇总? true表示要汇总
                nRet = PriceUtil.NegativePrices(strForegiftPrice,
                    true,
                    out strPrice,
                    out strError);
                if (nRet == -1)
                {
                    strError = "反转(来自读者记录中的<foregift>元素的)价格字符串 '" + strForegiftPrice + "' 时出错: " + strError;
                    return -1;
                }

                // 如果经过反转后的价格字符串为空，则需要特别替换为“0”，以免后面环节被当作没有值的空字符串。负号是有意义的，表示退款(而不是交款)哟
                if (String.IsNullOrEmpty(strPrice) == true)
                    strPrice = "-0";

            }

            if (strPrice.IndexOf('%') != -1)
            {
                strError = "价格字符串 '" + strPrice + "' 格式错误：除了使用宏%return_foregift_price%以外，价格字符串中不允许出现%符号";
                return -1;
            }

            strFinalPrice = strPrice;

            string strBorrowDate = DomUtil.GetAttr(nodeOverdue, "borrowDate");
            string strBorrowPeriod = DomUtil.GetAttr(nodeOverdue, "borrowPeriod");
            string strReturnDate = DomUtil.GetAttr(nodeOverdue, "returnDate");
            string strBorrowOperator = DomUtil.GetAttr(nodeOverdue, "borrowOperator");
            string strReturnOperator = DomUtil.GetAttr(nodeOverdue, "operator");
            string strID = DomUtil.GetAttr(nodeOverdue, "id");
            string strExistComment = DomUtil.GetAttr(nodeOverdue, "comment");

            // 2008/6/23
            string strPauseStart = DomUtil.GetAttr(nodeOverdue, "pauseStart");

            // 写入DOM
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            DomUtil.SetElementText(dom.DocumentElement,
                "itemBarcode", strItemBarcode);

            if (String.IsNullOrEmpty(strItemRecPath) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "itemRecPath", strItemRecPath);
            }

            // 2024/2/11
            if (string.IsNullOrEmpty(strItemRefID) == false)
                DomUtil.SetElementText(dom.DocumentElement,
        "itemRefID", strItemRefID);

            // 2016/9/5
            if (String.IsNullOrEmpty(strLocation) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "location", strLocation);
            }

            DomUtil.SetElementText(dom.DocumentElement,
                "readerBarcode", strReaderBarcode);
            DomUtil.SetElementText(dom.DocumentElement,
                "readerRefID", strReaderRefID);

            // 2012/9/15
            DomUtil.SetElementText(dom.DocumentElement,
    "libraryCode", strLibraryCode);

            DomUtil.SetElementText(dom.DocumentElement,
                "state", strState);
            DomUtil.SetElementText(dom.DocumentElement,
                "id", strID);
            DomUtil.SetElementText(dom.DocumentElement,
                "reason", strReason);

            // 2007/12/17
            if (String.IsNullOrEmpty(strOverduePeriod) == false)
                DomUtil.SetElementText(dom.DocumentElement,
                    "overduePeriod", strOverduePeriod);

            DomUtil.SetElementText(dom.DocumentElement,
                "price", strPrice);
            if (String.IsNullOrEmpty(strOriginPrice) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "originPrice", strOriginPrice);
            }

            // 2008/6/25
            {
                bool bAppend = true;
                if (string.IsNullOrEmpty(strNewComment) == false
                    && strNewComment[0] == '<')
                {
                    bAppend = false;
                    strNewComment = strNewComment.Substring(1);
                }
                else if (string.IsNullOrEmpty(strNewComment) == false
                    && strNewComment[0] == '>')
                {
                    bAppend = true;
                    strNewComment = strNewComment.Substring(1);
                }

                if (bAppend == true)
                {
                    string strText = "";
                    if (String.IsNullOrEmpty(strExistComment) == false)
                        strText += strExistComment;
                    if (String.IsNullOrEmpty(strNewComment) == false)
                    {
                        if (String.IsNullOrEmpty(strText) == false)
                            strText += "；";
                        strText += strNewComment;
                    }

                    DomUtil.SetElementText(dom.DocumentElement,
                        "comment",
                        strText);
                }
                else
                {
                    Debug.Assert(bAppend == false, "");

                    DomUtil.SetElementText(dom.DocumentElement,
                        "comment",
                        strNewComment);
                }
            }

            /*
            if (String.IsNullOrEmpty(strNewComment) == false
                || String.IsNullOrEmpty(strExistComment) == false)
            {
                string strText = "";
                if (String.IsNullOrEmpty(strExistComment) == false)
                    strText += strExistComment;
                if (String.IsNullOrEmpty(strNewComment) == false)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "；";
                    strText += strNewComment;
                }

                // 2008/6/25 从SetElementInnerXml()修改而来
                DomUtil.SetElementText(dom.DocumentElement,
                    "comment",
                    strText);
            }
             * */

            DomUtil.SetElementText(dom.DocumentElement,
                "borrowDate", strBorrowDate);
            DomUtil.SetElementText(dom.DocumentElement,
                "borrowPeriod", strBorrowPeriod);
            DomUtil.SetElementText(dom.DocumentElement,
                "borrowOperator", strBorrowOperator);   // 2006/3/27

            DomUtil.SetElementText(dom.DocumentElement,
                "returnDate", strReturnDate);
            DomUtil.SetElementText(dom.DocumentElement,
                "returnOperator", strReturnOperator);

            DomUtil.SetElementText(dom.DocumentElement,
                "operator", strOperator);   // 罚金操作者
            DomUtil.SetElementText(dom.DocumentElement,
                "operTime", strOperTime);

            // 2008/6/23
            if (String.IsNullOrEmpty(strPauseStart) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "pauseStart", strPauseStart);
            }

            strAmerceRecord = dom.OuterXml;
            return 0;
        }

        // 交违约金：在读者记录中去除所选的<overdue>元素，并且构造一批新记录准备加入违约金库
        // parameters:
        //      sessioninfo     注意可能为 null
        //      strLibraryCode  读者记录从属的馆代码
        // return:
        //      -1  error
        //      0   读者dom没有变化
        //      1   读者dom发生了变化
        int DoAmerceReaderXml(
            SessionInfo sessioninfo,
            RmsChannel channel,
            string strLibraryCode,
            ref XmlDocument readerdom,
            string strReaderRecPath,
            AmerceItem[] amerce_items,
            string strOperator,
            string strOperTimeString,
            out List<string> AmerceRecordXmls,
            out List<string> NotFoundIds,
            out List<string> Ids,
            out string strError)
        {
            strError = "";
            AmerceRecordXmls = new List<string>();
            NotFoundIds = new List<string>();
            Ids = new List<string>();
            int nRet = 0;

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "读者记录中竟然没有<barcode>元素值";
                return -1;
            }

            // 2024/2/11
            string strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                "refID");

            // 2022/7/21
            // 当前账户是否完全控制这条读者记录？
            bool completely_control = true;
            if (sessioninfo != null)
                completely_control = this.IsCurrentChangeableReaderPath(strReaderRecPath,
    sessioninfo.LibraryCodeList);

            // 当前账户的扩展管辖范围是否控制这条读者记录？
            bool expand_control = false;
            if (sessioninfo != null)
                expand_control = this.IsCurrentChangeableReaderPath(strReaderRecPath,
sessioninfo.ExpandLibraryCodeList);

            if (completely_control == false && expand_control == false)
            {
                strError = $"{SessionInfo.GetCurrentUserName(sessioninfo)}对读者 '{strReaderBarcode}' 没有管辖权";
                return -1;
            }

            bool bChanged = false;  // 读者dom是否发生了改变

            // string strNotFoundIds = "";

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                // string strID = ids[i].Trim();
                if (String.IsNullOrEmpty(item.ID) == true)
                    continue;

                XmlElement node = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + item.ID + "']") as XmlElement;
                if (node == null)
                {
                    NotFoundIds.Add(item.ID);

                    /*
                    if (strNotFoundIds != "")
                        strNotFoundIds += ",";
                    strNotFoundIds += item.ID;
                     * */
                    continue;
                }

                string strForegiftPrice = DomUtil.GetElementText(readerdom.DocumentElement,
                    "foregift");

                /*
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }
                */

                // 将读者记录中的<overdue>元素和属性转换为违约金库的记录格式
                nRet = ConvertOverdueStringToAmerceRecord(
                    channel,
                    node,
                    strLibraryCode,
                    strReaderBarcode,
                    strReaderRefID,
                    "amerced",
                    item.NewPrice,
                    item.NewComment,
                    strOperator,
                    strOperTimeString,
                    strForegiftPrice,
                    out string strFinalPrice,   // 最终使用的价格字符串。这是从item.NewPrice和node节点的price属性中选择出来，并且经过去除宏操作的一个最后价格字符串
                    out string strAmerceRecord,
                    out strError);
                if (nRet == -1)
                    return -1;

                AmerceRecordXmls.Add(strAmerceRecord);

                Ids.Add(item.ID);

                if (completely_control == false && expand_control == true)
                {
                    // 2022/7/21
                    string strItemBarcode = node.GetAttribute("barcode");
                    string strItemLocation = node.GetAttribute("location");

                    // 如果不涉及到任何册记录，就不允许交费
                    if (string.IsNullOrEmpty(strItemBarcode))
                    {
                        strError = $"ID 为 '{item.ID}' 的违约金事项无法进行交费，因为{SessionInfo.GetCurrentUserName(sessioninfo)}不具备管辖读者 '{strReaderBarcode}' 的权限";
                        return -1;
                    }

                    // TODO: 进一步观察当前账户是否管辖这条册记录
                    // 检查一个册记录的馆藏地点是否符合当前用户管辖的馆代码列表要求
                    // return:
                    //      -1  检查过程出错
                    //      0   符合要求
                    //      1   不符合要求
                    nRet = this.CheckItemLibraryCodeByLocation(strItemLocation,
                        sessioninfo.LibraryCodeList,
                        out string strItemLibraryCode,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        strError = $"ID 为 '{item.ID}' 的违约金事项无法进行交费，因为{SessionInfo.GetCurrentUserName(sessioninfo)}既不具备管辖读者 '{strReaderBarcode}' 的权限，也不具备管辖册 '{strItemBarcode}' 的权限";
                        return -1;
                    }
                }

                // 如果是押金，需要增/减<foregift>元素内的价格值。交费为增，退费为减。不过正负号已经含在价格字符串中，可以都理解为交费
                string strReason = "";
                strReason = DomUtil.GetAttr(node, "reason");

                // 2008/11/11
                if (strReason == "押金。")
                {
                    string strNewPrice = "";

                    /*
                    string strOldPrice = DomUtil.GetElementText(readerdom.DocumentElement,
                        "foregift");

                    if (strOldPrice.IndexOf('%') != -1)
                    {
                        strError = "来自读者记录<foregift>元素的价格字符串 '" + strOldPrice + "' 格式错误：价格字符串中不允许出现%符号";
                        return -1;
                    }

                    string strPrice = "";

                    if (String.IsNullOrEmpty(item.NewPrice) == true)
                        strPrice = DomUtil.GetAttr(node, "price");
                    else
                        strPrice = item.NewPrice;

                    // 看看价格字符串是否为宏?
                    if (strPrice == "%return_foregift_price%")
                    {
                        // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                        // parameters:
                        //      bSum    是否要顺便汇总? true表示要汇总
                        nRet = PriceUtil.NegativePrices(strOldPrice,
                            true,
                            out strPrice,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "反转(来自读者记录中的<foregift>元素的)价格字符串 '" + strOldPrice + "' 时出错: " + strError;
                            return -1;
                        }
                    }

                    if (strPrice.IndexOf('%') != -1)
                    {
                        strError = "价格字符串 '" + strPrice + "' 格式错误：除了使用宏%return_foregift_price%以外，价格字符串中不允许出现%符号";
                        return -1;
                    }

                    if (String.IsNullOrEmpty(strOldPrice) == false)
                    {
                        strNewPrice = PriceUtil.JoinPriceString(strOldPrice, strPrice);
                    }
                    else
                    {
                        strNewPrice = strPrice;
                    }
                     * */
                    if (String.IsNullOrEmpty(strForegiftPrice) == false)
                    {
                        strNewPrice = PriceUtil.JoinPriceString(strForegiftPrice, strFinalPrice);
                    }
                    else
                    {
                        strNewPrice = strFinalPrice;
                    }


                    DomUtil.SetElementText(readerdom.DocumentElement,
                        "foregift",
                        strNewPrice);

                    // 是否顺便写入最近一次的交费时间?
                    bChanged = true;
                }

                // 在读者记录中删除这个节点
                node.ParentNode.RemoveChild(node);
                bChanged = true;
            }

            /*
            if (strNotFoundIds != "")
            {
                strError = "下列id没有相匹配的<overdue>元素" + strNotFoundIds;
                return -1;
            }*/
            if (NotFoundIds.Count > 0)
            {
                strError = "下列id没有相匹配的<overdue>元素: " + StringUtil.MakePathList(NotFoundIds);
                return -1;
            }

            if (bChanged == true)
                return 1;
            return 0;
        }
        /*
        // 是否存在以停代金事项？
        static bool InPauseBorrowing(XmlDocument readerdom,
            out string strMessage)
        {
            strMessage = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count == 0)
                return false;

            XmlNode node = null;
            int nTotalCount = 0;

            string strPauseStart = "";

            // 计算以停代金事项总数目
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                if (String.IsNullOrEmpty(strPauseStart) == false)
                    nTotalCount++;
            }

            // 找到第一个已启动事项
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                if (String.IsNullOrEmpty(strPauseStart) == false)
                    goto FOUND;
            }

            if (nTotalCount > 0)
            {
                strMessage = "有未启动的 " + nTotalCount.ToString() + " 项以停代金事项";
                return true;
            }


            return false;   // 没有找到已启动的事项
        FOUND:
            string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
            strMessage = "有一项于 " + DateTimeUtil.LocalDate(strPauseStart) + " 开始的，为期 " + strOverduePeriod + " 的以停代金过程";

            if (nTotalCount > 1)
                strMessage += "(此外还有未启动的 "+(nTotalCount-1).ToString()+" 项)";

            return true;
        }
         * */

        // 为了兼容以前的版本。除了在校本中使用外，尽量不要使用了
        // 计算以停代金的停借周期值
        // parameter:
        //      strReaderType 读者类型。可为“海淀分馆/普通读者”这样的形态。注意“海淀分馆”部分表示读者的馆代码
        public int ComputePausePeriodValue(string strReaderType,
            long lValue,
            out long lResultValue,
            out string strPauseCfgString,
            out string strError)
        {
            // parameter:
            //      strReaderType 读者类型。可为“海淀分馆/普通读者”这样的形态。注意“海淀分馆”部分表示读者的馆代码
            //      strLibraryCode  册所在馆代码
            return ComputePausePeriodValue(strReaderType,
                "",
                lValue,
                out lResultValue,
                out strPauseCfgString,
                out strError);
        }

        // 计算以停代金的停借周期值
        // parameter:
        //      strReaderType 读者类型。建议为“海淀分馆/普通读者”这样的形态。注意“海淀分馆”部分表示读者的馆代码
        //      strLibraryCode  册所在馆代码
        public int ComputePausePeriodValue(string strQualifiedReaderType,
            string strItemLibraryCode,
            long lValue,
            out long lResultValue,
            out string strPauseCfgString,
            out string strError)
        {
            strError = "";
            strPauseCfgString = "1.0";
            lResultValue = lValue;

            // 获得 '以停代金因子' 配置参数
            MatchResult matchresult;
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = this.GetLoanParam(
                strItemLibraryCode, // 册所在馆代码
                strQualifiedReaderType,
                "",
                "以停代金因子",
                out strPauseCfgString,
                out matchresult,
                out strError);
            if (nRet == -1)
            {
                strError = "获得 馆代码 '" + strItemLibraryCode + "' 中 读者类型 '" + strQualifiedReaderType + "' 的 以停代金因子 参数时发生错误: " + strError;
                return -1;
            }

            if (nRet < 3 || string.IsNullOrEmpty(strPauseCfgString) == true)
            {
                // 没有找到匹配读者类型的定义，则按照 1.0 计算
                strPauseCfgString = "1.0";
                return 0;
            }

            double ratio = 1.0;

            try
            {
                ratio = Convert.ToDouble(strPauseCfgString);
            }
            catch
            {
                strError = "以停代金因子 配置字符串 '" + strPauseCfgString + "' 格式错误。应该为一个小数。";
                return -1;
            }

            lResultValue = (long)((double)lValue * ratio);
            return 1;
        }

        // 包装版本，为了兼容以前脚本。一次代码中不要使用这个函数
        public int HasPauseBorrowing(
    Calendar calendar,
    XmlDocument readerdom,
    out string strMessage,
    out string strError)
        {
            return HasPauseBorrowing(
                calendar,
                "",
                readerdom,
                out strMessage,
                out strError);
        }

        // 是否存在以停代金事项？
        // text-level: 用户提示
        // return:
        //      -1  error
        //      0   不存在
        //      1   存在
        public int HasPauseBorrowing(
            Calendar calendar,
            string strReaderLibraryCode,
            XmlDocument readerdom,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count == 0)
                return 0;

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            int nRet = 0;
            XmlNode node = null;
            int nTotalCount = 0;

            string strFirstPauseStart = "";


            // 找到第一个已启动事项
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string strPauseStart = "";
                strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                if (String.IsNullOrEmpty(strPauseStart) == false)
                {
                    // 2008/1/16 修正：
                    // 如果有pauseStart属性，但是没有overduePeriod属性，属于格式错误，
                    // 需要接着向后寻找格式正确的第一项
                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                    if (String.IsNullOrEmpty(strOverduePeriod) == true)
                    {
                        strPauseStart = "";
                        continue;
                    }

                    strFirstPauseStart = strPauseStart;
                    break;
                }
            }

            long lTotalOverduePeriod = 0;
            string strTotalUnit = "";

            // 遍历以停代金事项，计算时程总长度和最后结束日期
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                if (String.IsNullOrEmpty(strOverduePeriod) == true)
                    continue;

                string strUnit = "";
                long lOverduePeriod = 0;

                // 分析期限参数
                nRet = ParsePeriodUnit(strOverduePeriod,
                    out lOverduePeriod,
                    out strUnit,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (strTotalUnit == "")
                    strTotalUnit = strUnit;
                else
                {
                    if (strTotalUnit != strUnit)
                    {
                        // 出现了时间单位的不一致
                        if (strTotalUnit == "day" && strUnit == "hour")
                            lOverduePeriod = lOverduePeriod / 24;
                        else if (strTotalUnit == "hour" && strUnit == "day")
                            lOverduePeriod = lOverduePeriod * 24;
                        else
                        {
                            // text-level: 内部错误
                            strError = "时间单位 '" + strUnit + "' 和前面曾用过的时间单位 '" + strTotalUnit + "' 不一致，无法进行加法运算";
                            return -1;
                        }
                    }
                }

                long lResultValue = 0;
                string strPauseCfgString = "";
                // 计算以停代金的停借周期值
                // parameter:
                //      strReaderType 读者类型。可为“海淀分馆/普通读者”这样的形态。注意“海淀分馆”部分表示读者的馆代码
                //      strLibraryCode  册所在馆代码
                nRet = ComputePausePeriodValue(strReaderType,
                    strReaderLibraryCode,
                    lOverduePeriod,
                    out lResultValue,
                    out strPauseCfgString,
                    out strError);
                if (nRet == -1)
                    return -1;


                lTotalOverduePeriod += lResultValue;    //  lOverduePeriod;

                nTotalCount++;
            }

            // 2008/1/16 changed strPauseStart -->strFirstPauseStart
            if (String.IsNullOrEmpty(strFirstPauseStart) == true)
            {
                if (nTotalCount > 0)
                {
                    // text-level: 用户提示
                    strMessage = string.Format(this.GetString("有s项未启动的以停代金事项"), // "有 {0} 项未启动的以停代金事项"
                        nTotalCount.ToString());
                    // "有 " + nTotalCount.ToString() + " 项未启动的以停代金事项";
                    return 1;
                }

                return 0;
            }

            DateTime pause_start;
            try
            {
                pause_start = DateTimeUtil.FromRfc1123DateTimeString(strFirstPauseStart);
            }
            catch
            {
                // text-level: 内部错误
                strError = "停借开始日期 '" + strFirstPauseStart + "' 格式错误";
                return -1;
            }

            DateTime timeEnd;   // 以停代金整个的结束日期
            DateTime nextWorkingDay;

            // 测算还书日期
            // parameters:
            //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
            // return:
            //      -1  出错
            //      0   成功。timeEnd在工作日范围内。
            //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
            nRet = GetReturnDay(
                calendar,
                pause_start,
                lTotalOverduePeriod,
                strTotalUnit,
                out timeEnd,
                out nextWorkingDay,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "测算以停代金结束日期过程发生错误: " + strError;
                return -1;
            }

            bool bEndInNonWorkingDay = false;
            if (nRet == 1)
            {
                // end在非工作日
                bEndInNonWorkingDay = true;
            }

            DateTime now_rounded = this.Clock.UtcNow;  //  今天

            // 正规化时间
            nRet = DateTimeUtil.RoundTime(strTotalUnit,
                ref now_rounded,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now_rounded - timeEnd;

            long lDelta = 0;
            nRet = ParseTimeSpan(
                delta,
                strTotalUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            if (strTotalUnit == "hour")
            {
                // text-level: 用户提示
                strMessage = string.Format(this.GetString("共有s项以停代金事项，从s开始，总计应暂停借阅s, 于s结束"),
                    // "共有 {0} 项以停代金事项，从 {1} 开始，总计应暂停借阅 {2}, 于 {3} 结束。"
                    nTotalCount.ToString(),
                    pause_start.ToString("s"),
                    lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit),
                    timeEnd.ToString("s"));
                // "共有 " + nTotalCount.ToString() + " 项以停代金事项，从 " + pause_start.ToString("s") + " 开始，总计应暂停借阅 " + lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit) + ", 于 " + timeEnd.ToString("s") + " 结束。";
            }
            else
            {
                // text-level: 用户提示

                strMessage = string.Format(this.GetString("共有s项以停代金事项，从s开始，总计应暂停借阅s, 于s结束"),
                    // "共有 {0} 项以停代金事项，从 {1} 开始，总计应暂停借阅 {2}, 于 {3} 结束。"
                    nTotalCount.ToString(),
                    pause_start.ToString("d"),  // "yyyy-MM-dd"
                    lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit),
                    timeEnd.ToString("d")); // "yyyy-MM-dd"
                                            // "共有 " + nTotalCount.ToString() + " 项以停代金事项，从 " + pause_start.ToString("yyyy-MM-dd") + " 开始，总计应暂停借阅 " + lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit) + ", 于 " + timeEnd.ToString("yyyy-MM-dd") + " 结束。";
            }

            if (lDelta > 0)
            {
                // text-level: 用户提示
                strMessage += this.GetString("到当前时刻，上述整个以停代金周期已经结束。"); // "到当前时刻，上述整个以停代金周期已经结束。"
            }

            return 1;
        }

        // 处理以停代金功能
        // TODO: 如果本函数被日志恢复程序调用，则其内部采用UtcNow作为当前时间就是不正确的。应当是日志中记载的借阅当时时间
        // TODO: 写入日志的同时，也需要写入<overdues>元素内一个说明性的位置，便于随时查对
        // parameters:
        //      strReaderRecPath    当strAction为"refresh"时，需要给这个参数内容。以便写入日志。
        // return:
        //      -1  error
        //      0   readerdom没有修改
        //      1   readerdom发生了修改
        public int ProcessPauseBorrowing(
            string strLibraryCode,
            ref XmlDocument readerdom,
            string strReaderRecPath,
            string strUserID,
            string strAction,
            string strClientAddress,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 启动
            if (strAction == "start")
            {
                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                if (nodes.Count == 0)
                    return 0;

                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                    if (String.IsNullOrEmpty(strPauseStart) == false)
                        return 0;   // 已经有启动了的事项，不必再启动
                }

                // 2008/1/16 changed
                // 寻找第一个具有overduePeriod属性值的事项，设置为启动
                bool bFound = false;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                    if (String.IsNullOrEmpty(strOverduePeriod) == false)
                    {
                        // 把第一个具有overduePeriod属性值的事项设置为启动
                        DomUtil.SetAttr(node, "pauseStart", this.Clock.GetClock());
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    return 0;   // 没有找到具有overduePeriod属性值的事项

                // 写入统计指标
                // 启动事项数，而不是读者个数
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                    strLibraryCode,
                    "出纳",
                    "以停代金事项启动",
                    1);

                // TODO: 创建事件日志，记录启动事项的动作
                return 1;
            }

            // 刷新
            if (strAction == "refresh")
            {
                if (String.IsNullOrEmpty(strReaderRecPath) == true)
                {
                    strError = "refresh时必须提供strReaderRecPath参数值，否则无法创建日志记录";
                    return -1;
                }

                int nExpiredCount = 0;

                string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                    "barcode");
                // 2024/2/11
                string strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                    "refID");
                string strOldReaderXml = readerdom.OuterXml;

                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "operation",
                    "amerce");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "action",
                    "expire");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerBarcode",
                    strReaderBarcode);
                // 2024/2/11
                if (string.IsNullOrEmpty(strReaderRefID) == false)
                    DomUtil.SetElementText(domOperLog.DocumentElement,
    "readerRefID", strReaderRefID);

                XmlNode node_expiredOverdues = domOperLog.CreateElement("expiredOverdues");
                domOperLog.DocumentElement.AppendChild(node_expiredOverdues);

                string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
    "readerType");

                bool bChanged = false;

                for (; ; )
                {
                    XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                    if (nodes.Count == 0)
                        break;

                    // 找到第一个已启动事项
                    XmlNode node = null;
                    string strPauseStart = "";
                    XmlNode node_firstOverdueItem = null;   // 第一项符合超期条件(但不一定启动了的)的<overdue>元素
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        node = nodes[i];
                        string strTempOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                        if (String.IsNullOrEmpty(strTempOverduePeriod) == true)
                            continue;   // 忽略那些没有overduePeriod的元素

                        if (node_firstOverdueItem == null)
                            node_firstOverdueItem = node;
                        strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                        if (String.IsNullOrEmpty(strPauseStart) == false)
                            goto FOUND;
                    }

                    // 没有找到已启动的事项，则需要把第一个符合条件的事项启动
                    if (node_firstOverdueItem != null)
                    {
                        DomUtil.SetAttr(node_firstOverdueItem,
                            "pauseStart",
                            this.Clock.GetClock());
                        bChanged = true;
                        continue;   // 重新执行刷新操作似乎没有必要，因为没有刚开始就立即结束的？
                    }
                    break;
                FOUND:
                    string strUnit = "";
                    long lOverduePeriod = 0;

                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");

                    // 分析期限参数
                    nRet = ParsePeriodUnit(strOverduePeriod,
                        out lOverduePeriod,
                        out strUnit,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    long lResultValue = 0;
                    string strPauseCfgString = "";

                    // 计算以停代金的停借周期值
                    // parameter:
                    //      strReaderType 读者类型。可为“海淀分馆/普通读者”这样的形态。注意“海淀分馆”部分表示读者的馆代码
                    //      strLibraryCode  册所在馆代码
                    nRet = ComputePausePeriodValue(strReaderType,
                        strLibraryCode,
                        lOverduePeriod,
                        out lResultValue,
                        out strPauseCfgString,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    lOverduePeriod = lResultValue;

                    DateTime timeStart = DateTimeUtil.FromRfc1123DateTimeString(strPauseStart);

                    nRet = DateTimeUtil.RoundTime(strUnit,
                        ref timeStart,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    DateTime timeNow = this.Clock.UtcNow;
                    nRet = DateTimeUtil.RoundTime(strUnit,
                        ref timeNow,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    DateTime nextWorkingDay = new DateTime(0);
                    long lDistance = 0;
                    // 计算时间之间的距离
                    // parameters:
                    //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
                    // return:
                    //      -1  出错
                    //      0   成功。timeEnd在工作日范围内。
                    //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
                    nRet = GetTimeDistance(
                        null,   // Calendar calendar,
                        strUnit,
                        timeStart,
                        timeNow,
                        out lDistance,
                        out nextWorkingDay,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    long lDelta = lDistance - lOverduePeriod;

                    if (lDelta < 0)
                        break;  // 已经起作用的事项尚未到期

                    // 消除已经惩罚到期的<overdue>元素
                    DomUtil.SetAttr(node, "pauseStart", "");
                    Debug.Assert(node.ParentNode != null);
                    if (node.ParentNode != null)
                    {
                        // 推入事件日志
                        XmlDocumentFragment fragment = domOperLog.CreateDocumentFragment();
                        fragment.InnerXml = node.OuterXml;
                        node_expiredOverdues.AppendChild(fragment);

                        nExpiredCount++;

                        // 将到期的<overdue>元素从读者记录中删除
                        node.ParentNode.RemoveChild(node);
                        bChanged = true;

                        // 写入统计指标
                        // 到期事项数，而不是读者个数
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "出纳",
                            "以停代金事项到期",
                            1);
                    }

                    // TODO: 创建事件日志，记录到期消除事项的动作

                    // 启动下一个具有overduePeriod属性的<overdue>元素
                    nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        node = nodes[i];
                        strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                        if (String.IsNullOrEmpty(strPauseStart) == true)
                            goto FOUND_1;
                    }

                    break;// 没有找到下一个可启动的事项了
                FOUND_1:

                    TimeSpan delta;

                    // 构造TimeSpan
                    nRet = BuildTimeSpan(
                        lDelta,
                        strUnit,
                        out delta,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    DateTime timeLastEnd = timeNow - delta;

                    // 把第一个事项设置为启动
                    // 启动的日期是上一个事项到期的日子，而不是今日
                    DomUtil.SetAttr(nodes[0],
                        "pauseStart",
                        DateTimeUtil.Rfc1123DateTimeStringEx(timeLastEnd.ToLocalTime()));
                    bChanged = true;

                    // 写入统计指标
                    // 启动事项数，而不是读者个数
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "出纳",
                        "以停代金事项启动",
                        1);

                    // TODO: 创建事件日志，记录启动事项的动作

                    // 需要重新刷新，因为刚启动的事项可能马上就到期
                } // end of for

                if (nExpiredCount > 0)
                {
                    string strOperTime = this.Clock.GetClock();

                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        strUserID);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);

                    // 2012/5/7
                    // 修改前的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
    "oldReaderRecord", strOldReaderXml);   // 2014/3/8 以前 oldReeaderRecord
                    DomUtil.SetAttr(node, "recPath", strReaderRecPath);

                    // 日志中包含修改后的读者记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerRecord", readerdom.OuterXml);
                    DomUtil.SetAttr(node, "recPath", strReaderRecPath);

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        strClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Refresh Pause Borrowing 操作 写入日志时发生错误: " + strError;
                        return -1;
                    }
                }

                return bChanged == true ? 1 : 0;
            } // end of if 

            return 0;
        }


        // 升级违约金库记录。主要是添加 readerRefID 和 itemRefID 元素
        // return:
        //      -1  出错
        //      其它  总共处理的条数
        public int UpgradeAmerceRecords(
            SessionInfo sessioninfo,
            delegate_appendResultText AppendResultText,
            delegate_setProgressText SetProgressText,
            CancellationToken token,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(this.AmerceDbName))
                return 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            var db_path = this.WsUrl + "?" + this.AmerceDbName;

            RecordLoader loader = new RecordLoader(
                sessioninfo.Channels,
null,
new List<string> { db_path },
"default",
"id,xml,timestamp");
            int count = 0;
            foreach (KernelRecord record in loader)
            {
                if (token.IsCancellationRequested)
                {
                    strError = "处理被中断";
                    return -1;
                }

                SetProgressText?.Invoke($"正在处理违约金记录 {record.RecPath}");

                string strOutputPath = record.RecPath;
                string strXmlBody = record.Xml;
                byte[] baOutputTimeStamp = record.Timestamp;

                int nRedoCount = 0;
            REDO:
                string old_xml = strXmlBody;

                // 处理
                // parameters:
                //      changed    [out] xml 是否发生过改变
                // return:
                //      -1  出错
                //      0   正常
                nRet = ProcessAmerceRecord(
                    channel,
                    strOutputPath,
                    ref strXmlBody,
                    out bool bChanged,
                    out strError);
                if (nRet == -1)
                {
                    AppendResultText?.Invoke("ProcessItemRecord() error : " + strError + "。", true);
                    // 循环并不停止
                }

                if (bChanged == true)
                {
                    // return:
                    //      -2  保存时遇到时间戳冲突，已经重新装载读者记录返回于 strReaderXml 中，时间戳于 output_timestamp 中
                    //      -1  出错
                    //      0   保存成功
                    nRet = SaveRecord(channel,
strOutputPath,
ref strXmlBody,
baOutputTimeStamp,
out byte[] output_timestamp,
out strError);
                    if (nRet == 0)
                    {
                        nRet = WriteWriteResLog(sessioninfo,
                            strOutputPath,
                            old_xml,
                            strXmlBody,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"修改后的册记录 {strOutputPath} 写 setEntity 日志动作记录时出错: {strError}";
                            return -1;
                        }
                    }
                    else if (nRet == -1)
                    {
                        AppendResultText?.Invoke($"SaveRecord({strOutputPath}) error : " + strError + "。", true);
                        // 循环并不停止
                    }
                    else if (nRet == -2)
                    {
                        if (nRedoCount > 10)
                        {
                            AppendResultText?.Invoke($"SavePatronRecord({strOutputPath}) (遇到时间戳不匹配)重试十次以后依然出错，放弃重试。error : {strError}。", true);
                            // 循环并不停止
                        }
                        else
                        {
                            baOutputTimeStamp = output_timestamp;
                            nRedoCount++;
                            goto REDO;
                        }
                    }
                }

                count++;
            }

            return count;
        }

        // 处理升级违约金记录
        // parameters:
        //      changed    [out] xml 是否发生过改变
        // return:
        //      -1  出错
        //      0   正常
        int ProcessAmerceRecord(
            RmsChannel channel,
            string recpath,
            ref string xml,
            out bool changed,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            changed = false;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                strError = $"路径为 {recpath} 的 XML 记录装入 XMLDOM 时出错: {ex.Message}";
                return -1;
            }

            var version = DomUtil.GetElementText(dom.DocumentElement,
                "version");
            if (string.IsNullOrEmpty(version) == false
                && StringUtil.CompareVersion(version, "0.02") >= 0)
                return 0;

            DateTime now = DateTime.Now;

            {
                string readerBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "readerBarcode");
                string readerRefID = DomUtil.GetElementText(dom.DocumentElement,
                    "readerRefID");
                if (string.IsNullOrEmpty(readerBarcode) == false
                    && string.IsNullOrEmpty(readerRefID) == true)
                {
                    nRet = this.ConvertReaderBarcodeListToRefIdList(
                        channel,
                        readerBarcode,
                        out string readerRefIdString,
                        out strError);
                    if (nRet == -1)
                    {
                        WriteAmerceLog(this, $"读者证条码号 '{readerBarcode}' 没有找到对应的读者记录(升级违约金记录 '{recpath}' 时)");
                    }
                    else
                    {
                        var refID = dp2StringUtil.GetRefIdValue(readerRefIdString);
                        var element = DomUtil.SetElementText(dom.DocumentElement,
                            "readerRefID", refID);
                        element.SetAttribute("comment", $"{now.ToLongTimeString()} 自动升级");
                        changed = true;
                    }
                }
            }

            {
                string itemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "itemBarcode");
                string itemRefID = DomUtil.GetElementText(dom.DocumentElement,
                    "itemRefID");
                if (string.IsNullOrEmpty(itemBarcode) == false
                    && string.IsNullOrEmpty(itemRefID) == true)
                {
                    nRet = this.ConvertItemBarcodeListToRefIdList(
                        channel,
                        itemBarcode,
                        out string itemRefIdString,
                        out strError);
                    if (nRet == -1)
                    {
                        WriteAmerceLog(this, $"册条码号 '{itemBarcode}' 没有找到对应的读者记录(升级违约金记录 '{recpath}' 时)");
                    }
                    else
                    {
                        var refID = dp2StringUtil.GetRefIdValue(itemRefIdString);
                        var element = DomUtil.SetElementText(dom.DocumentElement,
                            "itemRefID", refID);
                        element.SetAttribute("comment", $"{now.ToLongTimeString()} 自动升级");
                        changed = true;
                    }
                }
            }


            if (changed == true)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "version", "0.02");
                // 违约金记录发生了变化
                xml = dom.DocumentElement.OuterXml;
            }

            return 0;
        }

        // Exception: 可能会抛出 XML 异常
        static string GetAmerceLibraryCode(string old_xml)
        {
            XmlDocument itemdom = new XmlDocument();
            itemdom.LoadXml(old_xml);
            return DomUtil.GetElementText(itemdom.DocumentElement,
    "libraryCode");
        }

        int WriteWriteResLog(SessionInfo sessioninfo,
            string recpath,
            string old_xml,
            string new_xml,
            out string strError)
        {
            strError = "";

            string strLibraryCode = GetAmerceLibraryCode(new_xml);

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");

            DomUtil.SetElementText(domOperLog.DocumentElement,
                "libraryCode",
                strLibraryCode);

            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation", "writeRes");



            DomUtil.SetElementText(domOperLog.DocumentElement, "requestResPath",
                recpath);

            Debug.Assert(string.IsNullOrEmpty(recpath) == false, "");
            DomUtil.SetElementText(domOperLog.DocumentElement, "resPath",
                recpath);

            /*
            DomUtil.SetElementText(domOperLog.DocumentElement, "ranges",
                strRanges);
            DomUtil.SetElementText(domOperLog.DocumentElement, "totalLength",
                lTotalLength.ToString());
            DomUtil.SetElementText(domOperLog.DocumentElement, "metadata",
                strMetadata);
            */
            {
                var element = DomUtil.SetElementText(domOperLog.DocumentElement,
    "oldRecord", old_xml);
                element.SetAttribute("recPath", recpath);
            }

            {
                var element = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", new_xml);
                element.SetAttribute("recPath", recpath);
            }

            {
                string strOperTimeString = this.Clock.GetClock();   // RFC1123格式

                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    sessioninfo.UserID);
                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTimeString);
            }

            /*
            DomUtil.SetElementText(domOperLog.DocumentElement, "style",
strStyle);
            */

            /*
            Stream attachment = null;
            if (baContent != null && baContent.Length > 0)
                attachment = new MemoryStream(baContent);
            try
            {
                int nRet = app.OperLog.WriteOperLog(domOperLog,
                    sessioninfo.ClientAddress,
                    attachment,
                    out strError);
                if (nRet == -1)
                {
                    strError = "WriteRes() API 写入日志时发生错误: " + strError;
                    result.Value = -1;
                    result.ErrorCode = ErrorCode.SystemError;
                    result.ErrorInfo = strError;
                    return result;
                }
            }
            finally
            {
                if (attachment != null)
                    attachment.Close();
            }
            */

            int nRet = this.OperLog.WriteOperLog(domOperLog,
    sessioninfo.ClientAddress,
    out strError);
            if (nRet == -1)
            {
                strError = "WriteWriteResLog() 写入操作日志时发生错误: " + strError;
                return -1;
            }
            return 0;
        }

        // 探测当前的到书队列库是否具备 册参考ID 这个检索点
        bool AmerceDbKeysContainsReaderRefIdKey()
        {
            if (this.AmerceDbFroms == null || this.AmerceDbFroms.Length == 0)
                return false;
            foreach (var info in this.AmerceDbFroms)
            {
                if (StringUtil.IsInList("patron_refid", info.Style) == true)
                    return true;
            }

            return false;
        }

        public static WriteTypeLogResult WriteAmerceLog(
    LibraryApplication app,
    string text)
        {
            return WriteTypeLog(app, "amerce", text);
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class AmerceItem
    {
        [DataMember]
        public string ID = "";  // 识别id
        [DataMember]
        public string NewPrice = "";    // 变更的价格
        [DataMember]
        public string NewComment = ""; // 注释
    }

}
