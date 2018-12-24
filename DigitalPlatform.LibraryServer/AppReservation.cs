using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Messaging;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和流通预约(保留)功能相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 预约
        // 权限：需要有reservation权限
        public LibraryServerResult Reservation(
            SessionInfo sessioninfo,
            string strFunction,
            string strReaderBarcode,
            string strItemBarcodeList)
        {
            LibraryServerResult result = new LibraryServerResult();

            // 权限字符串
            if (StringUtil.IsInList("reservation", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                // text-level: 用户提示
                result.ErrorInfo = this.GetString("预约操作被拒绝。不具备 reservation 权限。");
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            int nRet = 0;
            string strError = "";

            // 2010/12/31
            if (String.IsNullOrEmpty(this.ArrivedDbName) == true)
            {
                strError = "预约到书库尚未定义, 预约操作失败";
                goto ERROR1;
            }

            if (String.Compare(strFunction, "new", true) != 0
                && String.Compare(strFunction, "delete", true) != 0
                && String.Compare(strFunction, "merge", true) != 0
                && String.Compare(strFunction, "split", true) != 0
                )
            {
                result.Value = -1;

                // text-level: 内部错误
                result.ErrorInfo = string.Format(this.GetString("未知的strFunction参数值s"),    // "未知的strFunction参数值 '{0}'"
                    strFunction);
                // "未知的strFunction参数值 '" + strFunction + "'";
                result.ErrorCode = ErrorCode.InvalidParameter;
                return result;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 在架册集合
            List<string> OnShelfItemBarcodes = new List<string>();

            // 被删除的已到书状态册集合
            List<string> ArriveItemBarcodes = new List<string>();

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("Reservation 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif

            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try
            {
                // 读入读者记录
                string strReaderXml = "";
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
                    result.Value = -1;
                    // text-level: 用户提示
                    result.ErrorInfo = string.Format(this.GetString("读者证条码号s不存在"),   // 读者证条码号 {0} 不存在
                        strReaderBarcode);
                    // "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = string.Format(this.GetString("读入读者记录时发生错误s"), // "读入读者记录时发生错误: {0}"
                        strError);
                    // "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // 看看读者记录所从属的数据库，是否在参与流通的读者库之列
                // 2012/9/8
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);
                    bool bReaderDbInCirculation = true;
                    if (this.IsReaderDbName(strReaderDbName,
                        out bReaderDbInCirculation,
                        out strLibraryCode) == false)
                    {
                        // text-level: 内部错误
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 中的数据库名 '" + strReaderDbName + "' 居然不在定义的读者库之列。";
                        goto ERROR1;
                    }

                    if (bReaderDbInCirculation == false)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("预约操作被拒绝。读者证条码号s所在的读者记录s因其数据库s属于未参与流通的读者库"),  // "预约操作被拒绝。读者证条码号 '{0}' 所在的读者记录 '{1}' 因其数据库 '{2}' 属于未参与流通的读者库"
                            strReaderBarcode,
                            strOutputReaderRecPath,
                            strReaderDbName);

                        goto ERROR1;
                    }

                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = string.Format(this.GetString("装载读者记录进入XMLDOM时发生错误s"),   // "装载读者记录进入XML DOM时发生错误: {0}"
                        strError);
                    // "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                CachedRecordCollection records = new CachedRecordCollection();
                records.Add(strOutputReaderRecPath, readerdom, reader_timestamp);

                if (strFunction == "delete"
                    && sessioninfo.UserType != "reader")
                {
                    // 当工作人员代为操作时, 对于delete操作网开一面，不做基本检查(读者证状态和检查和未取次数的检查)
                }
                else
                {
                    // return:
                    //      -1  检测过程发生了错误。应当作不能借阅来处理
                    //      0   可以借阅
                    //      1   证已经过了失效期，不能借阅
                    //      2   证有不让借阅的状态
                    nRet = CheckReaderExpireAndState(readerdom,
                        out strError);
                    if (nRet != 0)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("预约操作被拒绝，原因s"),   // "预约操作被拒绝，原因: {0}"
                            strError);

                        // "预约操作被拒绝，原因: " + strError;
                        goto ERROR1;
                    }

                    // 检查到书未取次数是否超标
                    XmlNode nodeOutof = readerdom.DocumentElement.SelectSingleNode("outofReservations");
                    if (nodeOutof != null)
                    {
                        string strCount = DomUtil.GetAttr(nodeOutof, "count");
                        int nCount = 0;
                        try
                        {
                            nCount = Convert.ToInt32(strCount);
                        }
                        catch
                        {
                        }
                        if (nCount >= this.OutofReservationThreshold)
                        {
                            strError = string.Format(this.GetString("预约操作被拒绝，因为次数超过"),    // "预约操作被拒绝，因为当前读者以前预约到书后未取的次数超过了 {0} 次，被取消预约能力。如果要恢复预约能力，请读者到图书馆柜台办理解除手续。"
                                this.OutofReservationThreshold.ToString());

                            // "预约操作被拒绝，因为当前读者以前预约到书后未取的次数超过了 " + this.OutofReservationThreshold.ToString() + " 次，被取消预约能力。如果要恢复预约能力，请读者到图书馆柜台办理解除手续。";
                            goto ERROR1;
                        }
                    }
                }

                // 准备日志DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "reservation");

                if (String.Compare(strFunction, "new", true) == 0)
                {
                    // 对即将预约的册条码号进行查重
                    // 要求本读者先前未曾用这些条码号预约过
                    // return:
                    //      -1  出错
                    //      0   没有重
                    //      1   有重 提示信息在strError中
                    nRet = this.ReservationCheckDup(
                        strItemBarcodeList,
                        strLibraryCode,
                        ref readerdom,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        result.Value = -1;
                        // text-level: 用户提示
                        result.ErrorInfo = string.Format(this.GetString("预约操作被拒绝，原因s"),
                            strError);
                        // result.ErrorInfo = "预约请求被拒绝: " + strError;

                        result.ErrorCode = ErrorCode.DupItemBarcode;
                        return result;
                    }
                } // end of "new"

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

                // 切割出单个的册条码号
                string[] itembarcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < itembarcodes.Length; i++)
                {
                    string strItemBarcode = itembarcodes[i].Trim();

                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                        continue;

                    string strItemXml = "";
                    string strOutputItemRecPath = "";

                    // 册记录加锁
                    this.EntityLocks.LockForWrite(strItemBarcode);

                    try
                    {
                        int nRedoCount = 0;

                        REDO_LOAD:
                        byte[] item_timestamp = null;

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
                            out strOutputItemRecPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            // text-level: 用户提示
                            result.ErrorInfo = string.Format(this.GetString("册条码号s不存在"),   // "册条码号 {0} 不存在"
                                strItemBarcode);
                            // "册条码号 '" + strItemBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = string.Format(this.GetString("读入册记录时发生错误s"),   // "读入册记录时发生错误: {0}"
                                strError);
                            // "读入册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (nRet > 1)
                        {
                            // text-level: 内部错误
                            strError = string.Format(this.GetString("册条码号s有重复"),   // "册条码号 '{0}' 有重复({1}条)，无法进行预约操作。"
                                strItemBarcode,
                                nRet.ToString());
                            // "册条码号 '" + strItemBarcode + "' 有重复(" + nRet.ToString() + "条)，无法进行预约操作。";
                        }

                        nRet = LibraryApplication.LoadToDom(strItemXml,
                            out XmlDocument itemdom,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = string.Format(this.GetString("装载册记录进入XMLDOM时发生错误s"),   // "装载册记录进入XML DOM时发生错误: {0}"
                                strError);
                            // "装载册记录进入XML DOM时发生错误: " + strError;
                            goto ERROR1;
                        }

                        records.Add(strOutputItemRecPath, itemdom, item_timestamp);

                        // TODO: 若册属于个人藏书，则不仅要求预约者是同一分馆的读者，另外还要求预约者是主人的好友。即，主人的读者记录中 firends 列表中有预约者

                        if (strFunction != "delete")
                        {
                            // 2012/9/13
                            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
                            // return:
                            //      -1  检查过程出错
                            //      0   符合要求
                            //      1   不符合要求
                            nRet = CheckItemLibraryCode(itemdom,
                                strLibraryCode,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 1)
                            {
                                strError = "册记录 '" + strItemBarcode + "' 因馆藏地而不能进行预约: " + strError;
                                goto ERROR1;
                            }
                        }

                        // 检查册是否允许借出?
                        if (strFunction == "new")
                        {
                            // 2011/12/7
                            // 检查册记录状态
                            string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                                "state");
                            if (string.IsNullOrEmpty(strState) == false)
                            {
                                // text-level: 用户提示
                                strError = string.Format(this.GetString("册状态为s无法预约"),   // "册 {0} 状态为 '{1}' ，无法进行预约操作。"
                                    strItemBarcode,
                                    strState,
                                    nRet.ToString());
                                goto ERROR1;
                            }

                            StringBuilder debugInfo = null;
                            // 检查册是否允许被借出
                            // return:
                            //      -1  出错
                            //      0   借阅操作应该被拒绝
                            //      1   借阅操作应该被允许
                            nRet = CheckCanBorrow(
                                strLibraryCode,
                                false,
                                sessioninfo.Account,
                                readerdom,
                                itemdom,
                                ref debugInfo,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 0)
                            {
                                strError = string.Format("因册 {0} 不允许借出，所以也不允许预约: {1}", strItemBarcode, strError);
                                goto ERROR1;
                                // TODO: 这里是否可以跳过不允许借书的一册，继续向后处理?
                            }
                        }

                        // 在册记录中添加或者删除预约信息
                        nRet = this.DoReservationItemXml(
                            records,
                            channel,
                            strFunction,
                            strReaderBarcode,
                            sessioninfo.UserID,
                            ref itemdom,
                            out bool bOnShelf,
                            out bool bArrived,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (strFunction == "delete")
                        {
                            nRet = NotifyCancelArrive(
    channel,
    strItemBarcode,
    "", // strRefID 暂时不使用此参数
    itemdom,
    strLibraryCode,
    strReaderBarcode,
    strReaderXml,
    out strError);
                            if (nRet == -1)
                            {
                                this.WriteErrorLog("发出放弃取书通知(册条码号=" + strItemBarcode + ")时出错: " + strError);
                            }

                        }

                        if (bOnShelf == true)
                            OnShelfItemBarcodes.Add(strItemBarcode);

                        if (bArrived == true)
                            ArriveItemBarcodes.Add(strItemBarcode);

                        // 写回册记录
                        lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                            itemdom.OuterXml,
                            false,
                            "content", // ,ignorechecktimestamp
                            item_timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount > 10)
                                {
                                    // text-level: 内部错误
                                    strError = "写回册记录 '" + strOutputItemRecPath + "' 时反复遇到时间戳冲突, 超过10次重试仍然失败";
                                    goto ERROR1;
                                }
                                nRedoCount++;
                                goto REDO_LOAD;
                            }
                            // 当写操作发生错误时，将来用更科学的措施来undo，
                            // 即把刚才增加的<request>元素找到后删除
                            goto ERROR1;
                        }

                        records.Remove(strOutputItemRecPath);
                    }
                    finally
                    {
                        this.EntityLocks.UnlockForWrite(strItemBarcode);
                    }
                } // end of for

                // 在读者记录中加入或删除预约信息
                // parameters:
                //      strFunction "new"新增预约信息；"delete"删除预约信息; "merge"合并; "split"拆散
                // return:
                //      -1  error
                //      0   unchanged
                //      1   changed
                nRet = this.DoReservationReaderXml(
                    strFunction,
                    strItemBarcodeList,
                    sessioninfo.UserID,
                    ref readerdom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                CachedRecord reader_record = records.Find(strOutputReaderRecPath);
                if (nRet == 1 || (reader_record != null && reader_record.Changed))
                {
                    // 野蛮写入
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    records.Remove(strOutputReaderRecPath);

                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "action", strFunction);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode", strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "itemBarcodeList", strItemBarcodeList);

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
                        // text-level: 内部错误
                        strError = "Reservation() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    // 写入统计指标
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "出纳",
                        "预约次",
                        1);
                }

                // 对当前在普通架的图书，立即发出到书通知
                if (this.CanReserveOnshelf == true
                    && OnShelfItemBarcodes.Count > 0)
                {
                    // 只通知第一个在架的册
                    string strItemBarcode = OnShelfItemBarcodes[0];

                    if (string.IsNullOrEmpty(strItemBarcode) == true)
                    {
                        strError = "内部错误：OnShelfItemBarcodes 的第一个元素为空。数组情况 '" + StringUtil.MakePathList(OnShelfItemBarcodes) + "'";
                        goto ERROR1;
                    }

                    // 通知预约到书的操作
                    // 出于对读者库加锁方面的便利考虑, 单独做了此函数
                    // return:
                    //      -1  error
                    //      0   没有找到<request>元素
                    nRet = DoReservationNotify(
                        null,
                        channel,
                        strReaderBarcode,
                        false,  // 不需要函数内加读者锁，因为这里已经加了
                        strItemBarcode,
                        true,   // 在普通架
                        true,   // 需要修改当前册记录的<request>元素state属性
                        "",
                        out List<string> DeletedNotifyRecPaths, // 被删除的通知记录。不用。
                        out strError);
                    if (nRet == -1)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("预约操作已经成功, 但是在架立即通知功能失败, 原因s"),  // "预约操作已经成功, 但是在架立即通知功能失败, 原因: {0}"
                            strError);
                        // "预约操作已经成功, 但是在架立即通知功能失败, 原因: " + strError;
                        goto ERROR1;
                    }

                    /*
                            if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "出纳",
                        "预约到书册",
                        1);
                     * */

                    // 给与成功提示
                    // text-level: 用户提示
                    string strMessage = string.Format(this.GetString("请注意，您刚提交的预约请求立即就得到了兑现"), // "请注意，您刚提交的预约请求立即就得到了兑现(预约到书通知消息也向您发出了，请注意查收)。所预约的册 {0} 为在架状态，已为您保留，您从现在起就可来图书馆办理借阅手续。"
                        strItemBarcode);
                    // "请注意，您刚提交的预约请求立即就得到了兑现(预约到书通知消息也向您发出了，请注意查收)。所预约的册 " + strItemBarcode + " 为在架状态，已为您保留，您从现在起就可来图书馆办理借阅手续。";
                    if (OnShelfItemBarcodes.Count > 1)
                    {
                        OnShelfItemBarcodes.Remove(strItemBarcode);
                        string[] barcodelist = new string[OnShelfItemBarcodes.Count];
                        OnShelfItemBarcodes.CopyTo(barcodelist);
                        // text-level: 用户提示
                        strMessage += string.Format(this.GetString("您在同一预约请求中也同时提交了其他在架状态的册"),   // "您在同一预约请求中也同时提交了其他在架状态的册: {0}。因同一集合中的前述册 {1} 的生效，这些册同时被忽略。(如确要预约多个在架的册让它们都独立生效，请每次勾选一个后单独提交，而不要把多个册一次性提交。)"
                            String.Join(",", barcodelist),
                            strItemBarcode);
                        // "您在同一预约请求中也同时提交了其他在架状态的册: " + String.Join(",", barcodelist) + "。因同一集合中的前述册 " + strItemBarcode + " 的生效，这些册同时被忽略。(如确要预约多个在架的册让它们都独立生效，请每次勾选一个后单独提交，而不要把多个册一次性提交。)";
                    }

                    result.ErrorInfo = strMessage;
                }

                if (ArriveItemBarcodes.Count > 0)
                {
                    string[] barcodelist = new string[ArriveItemBarcodes.Count];
                    ArriveItemBarcodes.CopyTo(barcodelist);

                    // text-level: 用户提示
                    result.ErrorInfo += string.Format(this.GetString("册s在删除前已经处在到书状态"),    // "册 {0} 在删除前已经处在“到书”状态。您刚刚删除了这(些)请求，这意味着您已经放弃取书。图书馆将顺次满足后面排队等待的预约者的请求，或允许其他读者借阅此书。(若您意图要去图书馆正常取书，请一定不要去删除这样的状态为“已到书”的请求，软件会在您取书后自动删除)"
                        String.Join(",", barcodelist));
                    // "册 " + String.Join(",", barcodelist) + " 在删除前已经处在“到书”状态。您刚刚删除了这(些)请求，这意味着您已经放弃取书。图书馆将顺次满足后面排队等待的预约者的请求，或允许其他读者借阅此书。(若您意图要去图书馆正常取书，请一定不要去删除这样的状态为“已到书”的请求，软件会在您取书后自动删除)";
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Reservation 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            return result;
            ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 对即将预约的册条码号进行查重
        // 要求本读者先前未曾用这些条码号预约过
        // parameters:
        //      strLibraryCode  读者记录所在读者库的馆代码
        // return:
        //      -1  出错
        //      0   没有重
        //      1   有重 提示信息在strError中
        public int ReservationCheckDup(
            string strItemBarcodeList,
            string strLibraryCode,
            ref XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strItemBarcodeList) == true)
            {
                strError = this.GetString("册条码号列表不能为空");    // 册条码号列表不能为空
                return -1;
            }

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            // 得到该读者类型针对所有类型图书的"可预约册数"
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "可预约册数",
                out string strParamValue,
                out MatchResult matchresult,
                out strError);
            if (nRet == -1 || nRet < 3)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("读者类型s尚未定义可预约册数参数"),  // "读者类型 '{0}' 尚未定义 可预约册数 参数, 预约操作被拒绝"
                    strReaderType);
                // "读者类型 '" + strReaderType + "' 尚未定义 可预约册数 参数, 预约操作被拒绝";
                return -1;
            }

            int nMaxReserveItems = 0;
            try
            {
                nMaxReserveItems = Convert.ToInt32(strParamValue);
            }
            catch
            {
                // text-level: 内部错误
                strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 定义的 可预约册数 参数值 '" + strParamValue + "' 不合法，应当为纯数字";
                return -1;
            }

            string[] newbarcodes = strItemBarcodeList.Split(new char[] { ',' });

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("reservations/request");

            // 检查是否超过每个读者的配额
            if (nodes.Count >= nMaxReserveItems)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("预约册数超过最大值"),  // "本次预约前已经预约的事项数已经达到 {0}，已经超过 读者类型 '{1}' 允许的可预约册数 {2}，预约操作被拒绝"
                    nodes.Count,
                    strReaderType,
                    nMaxReserveItems.ToString());
                // "本次预约前已经预约的事项数已经达到 " + nodes.Count + "，已经超过 读者类型 '" + strReaderType + "' 允许的可预约册数 " + nMaxReserveItems.ToString() + "，预约操作被拒绝";
                return -1;
            }

            // 检查是否曾经预约过
            foreach (XmlElement node in nodes)
            {
                // XmlNode node = nodes[i];
                string strItems = DomUtil.GetAttr(node, "items");

                string[] barcodes = strItems.Split(new char[] { ',' });
                foreach (string barcode in barcodes)
                {
                    string strBarcode = barcode.Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    foreach (string newbarcode in newbarcodes)
                    {
                        string strNewBarcode = newbarcode.Trim();
                        if (String.IsNullOrEmpty(strNewBarcode) == true)
                            continue;

                        if (strNewBarcode == strBarcode)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("册条码号s已经被预约过"), // "册条码号 '{0}' 已经被预约过..."
                                strNewBarcode);
                            // "册条码号 '" + strNewBarcode + "' 已经被预约过...";
                            return 1;
                        }

                    } // end of newbarcodes

                } // end for barcode

            } // end for nodes

            // 检查是否正被当前读者借阅
            nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            foreach (XmlElement node in nodes)
            {
                // XmlNode node = nodes[i];

                string strItemBarcode = DomUtil.GetAttr(node, "barcode");
                if (String.IsNullOrEmpty(strItemBarcode) == true)
                    continue;

                foreach (string newbarcode in newbarcodes)
                {
                    string strNewBarcode = newbarcode.Trim();
                    if (String.IsNullOrEmpty(strNewBarcode) == true)
                        continue;

                    if (strNewBarcode == strItemBarcode)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("册s已经被当前读者借阅"),   // "册 '{0}' 已被当前读者借阅，因此不能被预约..."
                            strNewBarcode);
                        // "册 '" + strNewBarcode + "' 已被当前读者借阅，因此不能被预约...";
                        return 1;
                    }
                } // end of newbarcodes
            } // end for nodes

#if NO
            // 检查册是否允许借出?
            foreach (string newbarcode in newbarcodes)
            {
                string strNewBarcode = newbarcode.Trim();
                if (String.IsNullOrEmpty(strNewBarcode) == true)
                    continue;


            }
#endif

            return 0;
        }

        // 在册记录中加入预约信息
        // TODO: 要关注本函数到底保存了 itemdom 代表的册记录没有？因为这样会引起时间戳发生变化
        // parameters:
        //      strFunction "new"新增预约信息；"delete"删除预约信息
        //      bOnShelf    strFunction为"new"的情况下，如果册本来就没有人借阅，并且当前读者为预约该册的第一人，则bOnShelf返回true
        //      bArrived    strFunciont为"delete"的情况下，删除了状态为"arrived"的预约请求
        public int DoReservationItemXml(
            CachedRecordCollection records,
            RmsChannel channel,
            string strFunction,
            string strReaderBarcode,
            string strOperator,
            ref XmlDocument itemdom,
            out bool bOnShelf,
            out bool bArrived,
            out string strError)
        {
            strError = "";
            bOnShelf = false;
            bArrived = false;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: 用户提示
                strError = this.GetString("读者证条码号不能为空");    // 读者证条码号不能为空
                return -1;
            }

            XmlNode root = null;

            root = itemdom.DocumentElement.SelectSingleNode("reservations");
            if (root == null)
            {
                root = itemdom.CreateElement("reservations");
                root = itemdom.DocumentElement.AppendChild(root);
            }
            // 看看是否已经存在元素
            XmlNode nodeRequest = root.SelectSingleNode("request[@reader='" + strReaderBarcode + "']");

            if (String.Compare(strFunction, "new", true) == 0)
            {
                // 检查是否没有被人借阅
                string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                    "borrower");
                string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                    "state");
                string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                    "barcode");

                XmlNodeList nodesRequest = itemdom.DocumentElement.SelectNodes("reservations/request");
                if (String.IsNullOrEmpty(strBorrower) == true
                    && nodesRequest.Count == 0)
                {
                    // 2009/10/19 
                    // 状态为“加工中”
                    if (IncludeStateProcessing(strState) == true
                        && this.CanReserveOnshelf == false)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("不能预约加工中的册s"), // "不能预约状态为加工中的册 {0}"
                            strItemBarcode);
                        return -1;
                    }

                    if (this.CanReserveOnshelf == false)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("不能预约在架的册s"), // "不能预约在架(未被借出的)册 {0}"
                            strItemBarcode);
                        // "不能预约在架(未被借出的)册 " + strItemBarcode;
                        return -1;
                    }

                    if (IncludeStateProcessing(strState) == false)   // 只有不包含“加工中”的，才马上通知。否则只能等以后状态改变时通知
                    {
                        // 如果本来就没有人借阅，而且当前读者是第一个预约该册的
                        bOnShelf = true;
                    }
                }

                if (nodeRequest == null)
                {
                    nodeRequest = itemdom.CreateElement("request");
                    nodeRequest = root.AppendChild(nodeRequest);
                    DomUtil.SetAttr(nodeRequest, "reader", strReaderBarcode);
                }

                // 请求时间
                DomUtil.SetAttr(nodeRequest, "requestDate", this.Clock.GetClock());
                // 操作者
                DomUtil.SetAttr(nodeRequest, "operator", strOperator);
            }

            if (String.Compare(strFunction, "delete", true) == 0)
            {
                if (nodeRequest != null)
                {
                    // 删除前要检查状态，是不是arrived
                    string strState = DomUtil.GetAttr(nodeRequest, "state");
                    if (strState == "arrived")
                    {
                        // TODO: 是否需要短信通知当前读者，这样意味着已经到的书放弃取。

                        string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                            "barcode");

                        string strQueueRecXml = "";
                        byte[] baQueueRecTimestamp = null;
                        string strQueueRecPath = "";

                        // 获得预约到书队列记录
                        // parameters:
                        //      strItemBarcodeParam  册条码号。可以使用 @itemRefID: 前缀
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        int nRet = GetArrivedQueueRecXml(
                            // channels,
                            channel,
                            strItemBarcode.Replace("@refID:", "@itemRefID:"),
                            out strQueueRecXml,
                            out baQueueRecTimestamp,
                            out strQueueRecPath,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet >= 1)
                        {
                            XmlDocument queue_rec_dom = new XmlDocument();
                            try
                            {
                                queue_rec_dom.LoadXml(strQueueRecXml);
                            }
                            catch (Exception ex)
                            {
                                // text-level: 内部错误
                                strError = "预约队列记录XML装入DOM时失败: " + ex.Message;
                                return -1;
                            }
                            nRet = DoNotifyNext(
                                records,
                                channel,
                                strQueueRecPath,
                                queue_rec_dom,
                                baQueueRecTimestamp,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 1)
                            {
                                // 需要归架
                                // 册记录中<location>需要去掉#reservation，相关<request>元素也需要删除
                                string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                                    "location");
                                // StringUtil.RemoveFromInList("#reservation", true, ref strLocation);
                                strLocation = StringUtil.GetPureLocationString(strLocation);
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "location", strLocation);

                            }
                        }

                        // 给出返回状态
                        bArrived = true;

                    } // end of -- if (strState == "arrived")

                    // 经过 DoNotifyNext() 以后 itemdom 内容可能会发生变化
                    nodeRequest = root.SelectSingleNode("request[@reader='" + strReaderBarcode + "']");
                    if (nodeRequest != null && nodeRequest.ParentNode != null)
                        nodeRequest.ParentNode.RemoveChild(nodeRequest);
                } // end of -- if (nodeRequest != null)
            }

            return 0;
        }

        // 在读者记录中加入或删除预约信息
        // parameters:
        //      strFunction "new"新增预约信息；"delete"删除预约信息; "merge"合并; "split"拆散
        //      strItemBarcodeList  册条码号的列表。每个部分可以使用 @refID: 前缀
        // return:
        //      -1  error
        //      0   unchanged
        //      1   changed
        public int DoReservationReaderXml(
            string strFunction,
            string strItemBarcodeList,
            string strOperator,
            ref XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strItemBarcodeList) == true)
            {
                // text-level: 用户提示
                strError = this.GetString("册条码号列表不能为空");    // 册条码号列表不能为空
                return -1;
            }

            XmlNode root = null;

            root = readerdom.DocumentElement.SelectSingleNode("reservations");
            if (root == null)
            {
                // 2016/6/9
                if (String.Compare(strFunction, "delete", true) == 0)
                    return 0;

                root = readerdom.CreateElement("reservations");
                root = readerdom.DocumentElement.AppendChild(root);
            }

            if (String.Compare(strFunction, "new", true) == 0)
            {
                XmlNode node = readerdom.CreateElement("request");
                node = root.AppendChild(node);
                DomUtil.SetAttr(node, "items", strItemBarcodeList);

                // 请求时间
                DomUtil.SetAttr(node, "requestDate", this.Clock.GetClock());
                // 操作者
                DomUtil.SetAttr(node, "operator", strOperator);

                return 1;
            }

            // 删除
            // 注意已经在通知状态的事项, 不能简单编辑删除. 如果表现为撤销, 可以
            if (String.Compare(strFunction, "delete", true) == 0)
            {
                bool bChanged = false;
                string[] barcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    // 在所有请求行中，只要匹配上此条码的行，就删除
                    XmlNodeList nodes = root.SelectNodes("request");

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        XmlNode node = nodes[j];
                        string strItems = DomUtil.GetAttr(node, "items");
                        if (IsInBarcodeList(strBarcode, strItems) == true)
                        {
                            node.ParentNode.RemoveChild(node);
                            bChanged = true;
                        }
                    }

                    // 若删除了状态为arrived的事项，items处理那里已经产生了提示信息
                }

                if (bChanged == true)
                    return 1;

                return 0;
            }

            // 合并
            // 注意已经处在通知状态的事项, 不能和其他事项合并
            if (String.Compare(strFunction, "merge", true) == 0)
            {
                string strMerged = "";
                bool bChanged = false;
                XmlNode node = null;
                // 找到符合条件的行, 然后删除, 重新形成一个新行
                string[] barcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    XmlNodeList nodes = root.SelectNodes("request");

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        node = nodes[j];
                        string strItems = DomUtil.GetAttr(node, "items");
                        if (IsInBarcodeList(strBarcode, strItems) == true)
                        {
                            string strState = DomUtil.GetAttr(node, "state");
                            if (strState == "arrived")
                            {
                                // text-level: 用户提示
                                strError = this.GetString("合并操作被拒绝。状态为已到书的行不能参与合并操作。");    // "合并操作被拒绝。状态为已到书的行不能参与合并操作。"
                                return -1;
                            }

                            if (strMerged != "")
                                strMerged += ",";
                            strMerged += strItems;
                            node.ParentNode.RemoveChild(node);
                            bChanged = true;
                        }
                    } // end of for j

                } // end of for i

                if (bChanged == true)
                {
                    node = readerdom.CreateElement("request");
                    node = root.InsertBefore(node, root.FirstChild);    // 插入到第一个
                    DomUtil.SetAttr(node, "items", strMerged);

                    // 请求时间
                    DomUtil.SetAttr(node, "requestDate", this.Clock.GetClock());
                    // 操作者
                    DomUtil.SetAttr(node, "operator", strOperator);
                }

                if (bChanged == true)
                    return 1;

                return 0;
            }

            // 拆散
            // 注意已经处在通知状态的事项, 不能被拆散
            if (String.Compare(strFunction, "split", true) == 0)
            {
                string strSplited = "";
                bool bChanged = false;
                XmlNode node = null;
                // 找到符合条件的行, 然后删除, 重新加入许多新行
                string[] barcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    XmlNodeList nodes = root.SelectNodes("request");

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        node = nodes[j];
                        string strItems = DomUtil.GetAttr(node, "items");
                        if (IsInBarcodeList(strBarcode, strItems) == true)
                        {
                            string strState = DomUtil.GetAttr(node, "state");
                            if (strState == "arrived")
                            {
                                // text-level: 用户提示
                                strError = this.GetString("拆散操作被拒绝。状态为已到书的行不能参与拆散操作。");    // "拆散操作被拒绝。状态为已到书的行不能参与拆散操作。"
                                return -1;
                            }


                            if (strSplited != "")
                                strSplited += ",";
                            strSplited += strItems;
                            node.ParentNode.RemoveChild(node);
                            bChanged = true;
                        }
                    } // end of for j

                } // end of for i

                if (bChanged == true)
                {
                    bool bFirst = true;
                    XmlNode prev = null;
                    barcodes = strSplited.Split(new char[] { ',' });
                    for (int i = 0; i < barcodes.Length; i++)
                    {
                        string strBarcode = barcodes[i].Trim();
                        if (String.IsNullOrEmpty(strBarcode) == true)
                            continue;

                        node = readerdom.CreateElement("request");
                        if (bFirst == true)
                        {
                            node = root.InsertBefore(node, root.FirstChild);    // 插入到第一个
                            bFirst = false;
                        }
                        else
                        {
                            node = root.InsertAfter(node, prev);
                        }
                        DomUtil.SetAttr(node, "items", strBarcode);

                        // 请求时间
                        DomUtil.SetAttr(node, "requestDate", this.Clock.GetClock());
                        // 操作者
                        DomUtil.SetAttr(node, "operator", strOperator);
                        prev = node;
                    }
                }

                if (bChanged == true)
                    return 1;

                return 0;
            }

            return 0;
        }

        // text-level: 内部处理
        // 通知预约到书的操作
        // 出于对读者库加锁方面的便利考虑, 单独做了此函数
        // 注：本函数可能要删除部分通知记录
        // parameters:
        //      strItemBarcodeParam  册条码号。可以使用 "@refID:" 前缀
        //      bOnShelf    是否为在架预约情况。在架预约就是书已经立即可以来取了
        // return:
        //      -1  error
        //      0   没有找到<request>元素
        //      1   已成功处理
        public int DoReservationNotify(
            CachedRecordCollection records,
            RmsChannel channel,
            string strReservationReaderBarcode,
            bool bNeedLockReader,
            string strItemBarcodeParam,
            bool bOnShelf,
            bool bModifyItemRecord,
            string strNotifyID,
            out List<string> DeletedNotifyRecPaths,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            long lRet = 0;
            DeletedNotifyRecPaths = null;

            // 获得相关读者记录
            string strReaderXml = "";
            string strOutputReaderRecPath = "";

            string strItemXml = "";

            // string strNotifyID = Guid.NewGuid().ToString();

            // 加读者记录锁
            if (bNeedLockReader == true)
            {
#if DEBUG_LOCK_READER
                this.WriteErrorLog("DoReservationNotify 开始为读者加写锁 '" + strReservationReaderBarcode + "'");
#endif
                this.ReaderLocks.LockForWrite(strReservationReaderBarcode);
            }
            try
            {
                // 读入读者记录
                nRet = this.GetReaderRecXml(
                    //sessioninfo,
                    // channels,
                    channel,
                    strReservationReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReservationReaderBarcode + "' 不存在";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    return -1;
                }

                XmlDocument readerdom = null;

                CachedRecord reader_record = null;
                if (records != null)
                    reader_record = records.Find(strOutputReaderRecPath);

                if (reader_record == null)
                {
                    nRet = LibraryApplication.LoadToDom(strReaderXml,
                        out readerdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                        return -1;
                    }
                }
                else
                    readerdom = reader_record.Dom;

                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

#if NO
                RmsChannel channel = channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }
#endif

                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("reservations/request");
                XmlElement readerRequestNode = null;
                string strItems = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    readerRequestNode = nodes[i] as XmlElement;
                    strItems = DomUtil.GetAttr(readerRequestNode, "items");
                    if (IsInBarcodeList(strItemBarcodeParam, strItems) == true)
                        goto FOUND;
                }

                return 0;   // not found request
                FOUND:
                Debug.Assert(readerRequestNode != null, "");

                // 将相关册中的<request>元素清除
                // 因为预约的时候可能列举了很多册，但只要其中一个到书，其它册的预约信息就需要解除了
                string[] barcodes = strItems.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;
                    if (strBarcode == strItemBarcodeParam
                        && bModifyItemRecord == false)
                    {
                        continue;   // 这条册记录已经处理过了
                    }

                    // 读入册记录
                    string strCurrentItemXml = "";
                    string strOutputItemRecPath = "";
                    // 获得册记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetItemRecXml(
                        //sessioninfo,
                        channel,
                        strBarcode,
                        out strCurrentItemXml,
                        out strOutputItemRecPath,
                        out strError);
                    if (nRet == 0)
                    {
                        strError = "册条码号 '" + strBarcode + "' 不存在";
                        continue;
                    }
                    if (nRet == -1)
                    {
                        strError = "读入册记录 '" + strBarcode + "' 时发生错误: " + strError;
                        return -1;
                    }

                    XmlDocument itemdom = null;

                    CachedRecord item_record = null;
                    if (records != null)
                        item_record = records.Find(strOutputItemRecPath);

                    if (item_record == null)
                    {
                        nRet = LibraryApplication.LoadToDom(strCurrentItemXml,
                            out itemdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载册记录进入XML DOM时发生错误: " + strError;
                            return -1;
                        }
                    }
                    else
                        itemdom = item_record.Dom;

                    if (strBarcode == strItemBarcodeParam)
                        strItemXml = strCurrentItemXml;

                    if (strBarcode == strItemBarcodeParam)
                    {
                        string strTempReservationReaderBarcode = "";

                        // TODO: 确认一下这个函数内部不会保存册记录
                        // 如果正好是当前册记录，需要设置arrived状态
                        // 如果为丢失处理，本函数的调用者需要通知等待者：书已经丢失了，不用再等待
                        // parameters:
                        //      bMaskLocationReservation    不要给<location>打上#reservation标记
                        // return:
                        //      -1  error
                        //      0   没有修改
                        //      1   进行过修改
                        nRet = DoItemReturnReservationCheck(
                            bOnShelf,   // 当册现在还在架上的时候，册记录的 <location> 就没有 #reservation。这样在借书环节检查册是否被预约的时候，就不能只看 <location> 了
                            ref itemdom,
                            out strTempReservationReaderBarcode,
                            out strNotifyID,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "修改册记录'" + strBarcode + "' 预约到书状态时(DoItemReturnReservationCheck)发生错误: " + strError;
                            return -1;
                        }
                        // 对册记录如果没有改动
                        if (nRet == 0)
                            continue;
                    }
                    else
                    {
                        // 如果是同一请求中的其他册记录

                        // 删除对应的<request>元素
                        XmlNode itemrequestnode = itemdom.DocumentElement.SelectSingleNode("reservations/request[@reader='" + strReservationReaderBarcode + "']");
                        if (itemrequestnode == null)
                            continue;

                        itemrequestnode.ParentNode.RemoveChild(itemrequestnode);
                    }

                    if (item_record == null)
                    {
                        // 写回册记录
                        lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                            itemdom.OuterXml,
                            false,
                            "content,ignorechecktimestamp",
                            timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "写回册记录'" + strBarcode + "' (记录路径'" + strOutputItemRecPath + "')时发生错误: " + strError;
                            return -1;
                        }
                    }
                    else
                        item_record.Changed = true;

                } // end of for

                // 读者记录中为对应的<request>元素打上状态记号
                DomUtil.SetAttr(readerRequestNode, "state", "arrived");
                // 到达时间
                DomUtil.SetAttr(readerRequestNode, "arrivedDate", this.Clock.GetClock());
                // 实际到达的一个册条码号 2007/1/18 
                DomUtil.SetAttr(readerRequestNode, "arrivedItemBarcode", strItemBarcodeParam);

                // 2016/12/4
                readerRequestNode.SetAttribute("notifyID", strNotifyID);

                // 2016/12/4
                // 确保 strItemXml 有值
                nRet = EnsureGetItemXml(
            records,
            channel,
            strItemBarcodeParam,
            ref strItemXml,
            out strError);
                if (nRet == -1)
                    return -1;

                // 增补两个属性值
                {
                    XmlDocument itemdom = new XmlDocument();
                    itemdom.LoadXml(strItemXml);

                    readerRequestNode.SetAttribute("accessNo", DomUtil.GetElementText(itemdom.DocumentElement, "accessNo"));
                    readerRequestNode.SetAttribute("location", DomUtil.GetElementText(itemdom.DocumentElement, "location"));
                }

                if (reader_record == null)
                {
                    // 写回读者记录
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        return -1;
                }
                else
                    reader_record.Changed = true;

                strReaderXml = readerdom.DocumentElement.OuterXml;
            }
            finally
            {
                if (bNeedLockReader == true)
                {
                    this.ReaderLocks.UnlockForWrite(strReservationReaderBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("DoReservationNotify 结束为读者加写锁 '" + strReservationReaderBarcode + "'");
#endif
                }
            }

            string strLibraryCode = "";
            nRet = this.GetLibraryCode(strOutputReaderRecPath,
                out strLibraryCode,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            if (string.IsNullOrEmpty(strItemXml) == true)
            {
                // 读入册记录
                string strOutputItemRecPath = "";
                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    channel,
                    strItemBarcodeParam,
                    out strItemXml,
                    out strOutputItemRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strItemBarcodeParam + "' 不存在";
                }
                if (nRet == -1)
                {
                    strError = "读入册记录 '" + strItemBarcodeParam + "' 时发生错误: " + strError;
                    return -1;
                }

                CachedRecord temp_record = null;
                if (records != null)
                    temp_record = records.Find(strOutputItemRecPath);

                if (temp_record != null
                    && temp_record.Dom != null
                    && temp_record.Dom.DocumentElement != null)
                    strItemXml = temp_record.Dom.DocumentElement.OuterXml;
            }

#endif

            // 构造一个XML记录, 加入"预约到书"库
            // 加入记录预约到书库的目的，是为了让工作线程可以监控读者是否来取书，如果超过保留期限，要转而通知下一个预约了此册的读者。
            // 兼有email通知功能
            nRet = AddNotifyRecordToQueueDatabase(
                // channels,
                channel,
                strItemBarcodeParam,
                "", // 暂时不使用此参数
                strItemXml,
                bOnShelf,
                strLibraryCode,
                strReservationReaderBarcode,
                strReaderXml,
                strNotifyID,
                out DeletedNotifyRecPaths,
                out strError);
            if (nRet == -1)
                return -1;

            if (this.Statis != null)
                this.Statis.IncreaseEntryValue(strLibraryCode,
    "出纳",
    "预约到书册",
    1);

            return 1;
        }

        int EnsureGetItemXml(
            CachedRecordCollection records,
            RmsChannel channel,
            string strItemBarcodeParam,
            ref string strItemXml,
            out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(strItemXml) == true)
            {
                // 读入册记录
                string strOutputItemRecPath = "";
                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = this.GetItemRecXml(
                    channel,
                    strItemBarcodeParam,
                    out strItemXml,
                    out strOutputItemRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strItemBarcodeParam + "' 不存在";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "读入册记录 '" + strItemBarcodeParam + "' 时发生错误: " + strError;
                    return -1;
                }

                CachedRecord temp_record = null;
                if (records != null)
                    temp_record = records.Find(strOutputItemRecPath);

                if (temp_record != null
                    && temp_record.Dom != null
                    && temp_record.Dom.DocumentElement != null)
                    strItemXml = temp_record.Dom.DocumentElement.OuterXml;
            }

            return 0;
        }

        // 探测当前的到书队列库是否具备 册参考ID 这个检索点
        bool ArrivedDbKeysContainsRefIDKey()
        {
            if (this.ArrivedDbFroms == null || this.ArrivedDbFroms.Length == 0)
                return false;
            foreach (BiblioDbFromInfo info in this.ArrivedDbFroms)
            {
                if (StringUtil.IsInList("item_refid", info.Style) == true)
                    return true;
            }

            return false;
        }

        // 发出放弃取书通知
        // parameters:
        //      strItemBarcode  册条码号。必须是册条码号。如果册条码号为空，参考ID需要使用 strRefID 参数
        //      strRefID        参考ID
        //      strLibraryCode  读者所在的馆代码
        //      strReaderXml    预约了图书的读者的XML记录。用于消息通知接口
        int NotifyCancelArrive(
            RmsChannel channel,
            string strItemBarcode,
            string strRefID,
            // string strItemXml,
            XmlDocument itemdom,
            string strLibraryCode,
            string strReaderBarcode,
            string strReaderXml,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                // 如果检索用的册条码号为空，加上对命中结果数量不设限，那就会造成系统严重繁忙。
                strError = "参数strItemBarcode中的册条码号不能为空。";
                return -1;
            }

            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 兼容 strItemBarcode 中含有前缀的用法
            string strHead = "@refID:";
            if (StringUtil.HasHead(strItemBarcode, strHead, true) == true)
            {
                strRefID = strItemBarcode.Substring(strHead.Length);
                strItemBarcode = "";
            }

            string strReaderEmailAddress = "";
            string strName = "";
            nRet = GetReaderNotifyInfo(
                strReaderXml,
                out strName,
                out strReaderEmailAddress,
                out strError);
            if (nRet == -1)
                return -1;

            // 获得图书摘要信息
            string strSummary = "";
            string strBiblioRecPath = "";

            nRet = this.GetBiblioSummary(strItemBarcode,
                "", //  strConfirmItemRecPath,
                null,   //  strBiblioRecPathExclude,
                25,
                out strBiblioRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
            {
                strSummary = "ERROR: " + strError;
            }

            // 发送短消息通知
            string strTotalError = "";

            // *** dpmail
            if (this.MessageCenter != null
                && StringUtil.IsInList("dpmail", this.ArrivedNotifyTypes))
            {
                string strTemplate = "";
                // 获得邮件模板
                nRet = GetMailTemplate(
                    "dpmail",
                    "放弃取书通知",
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
#if NO
                    strError = "放弃取书通知<mailTemplate/template>尚未配置。";
                    return -1;
#endif
                    return 0;   // 没有配置模板就不通知了
                }

                /*
                %item%  册信息
                %reservetime%   保留期限
                %today% 发出email的当天
                %summary% 书目摘要
                %itembarcode% 册条码号 
                %name% 读者姓名
                 * */
                Hashtable table = new Hashtable();
                table["%item%"] = "(册条码号为: " + strItemBarcode + " URL为: " + this.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + " )";
                table["%today%"] = DateTime.Now.ToString();
                table["%summary%"] = strSummary;
                table["%itembarcode%"] = strItemBarcode;
                table["%name%"] = strName;
                string strBody = "";

                nRet = GetMailText(strTemplate,
                    table,
                    out strBody,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(this.MessageCenter.MessageDbName) == false)
                {
                    Debug.Assert(channel.Container != null, "");
                    // 发送消息
                    nRet = this.MessageCenter.SendMessage(
                        channel.Container,  // channels,
                        strReaderBarcode,
                        "图书馆",
                        "放弃取书通知",
                        "text",
                        strBody,
                        false,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送dpmail消息时出错: " + strError + "\r\n";
                    }
                }
            }

            // 2016/4/26
            // *** mq
            if (string.IsNullOrEmpty(this.OutgoingQueue) == false
                && StringUtil.IsInList("mq", this.ArrivedNotifyTypes))
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                /* 元素名
                 * type 消息类型。预约到书通知
                 * itemBarcode 册条码号
                 * location 馆藏地 2016/9/5
                 * refID    参考 ID
                 * opacURL  图书在 OPAC 中的 URL。相对路径
                 * today 今天的日期
                 * summary 书目摘要
                 * patronName   读者姓名
                 * patronRecord 读者 XML 记录
 * */
                DomUtil.SetElementText(dom.DocumentElement, "type", "放弃取书通知");

                DomUtil.SetElementText(dom.DocumentElement,
                    "itemBarcode", strItemBarcode);
                DomUtil.SetElementText(dom.DocumentElement,
                    "location", strLocation);
                DomUtil.SetElementText(dom.DocumentElement,
                    "refID", strRefID);

                DomUtil.SetElementText(dom.DocumentElement,
                    "opacURL", this.OpacServerUrl + "/book.aspx?barcode="
                    + (string.IsNullOrEmpty(strItemBarcode) ? "@refID:" + strRefID : strItemBarcode));
                DomUtil.SetElementText(dom.DocumentElement,
                    "today", DateTime.Now.ToString());
                DomUtil.SetElementText(dom.DocumentElement,
                    "summary", strSummary);
                DomUtil.SetElementText(dom.DocumentElement,
                    "patronName", strName);

                {
                    XmlDocument readerdom = new XmlDocument();
                    readerdom.LoadXml(strReaderXml);

                    XmlElement record = dom.CreateElement("patronRecord");
                    dom.DocumentElement.AppendChild(record);
                    record.InnerXml = readerdom.DocumentElement.InnerXml;

                    DomUtil.DeleteElement(record, "borrowHistory");
                    DomUtil.DeleteElement(record, "password");
                    DomUtil.DeleteElement(record, "fingerprint");
                    DomUtil.DeleteElement(record, "face");
                    DomUtil.SetElementText(record, "libraryCode", strLibraryCode);
                }

                try
                {
                    MessageQueue queue = new MessageQueue(this.OutgoingQueue);

                    // 向 MSMQ 消息队列发送消息
                    nRet = ReadersMonitor.SendToQueue(queue,
                        (string.IsNullOrEmpty(strRefID) ? strReaderBarcode : "!refID:" + strRefID)
                        + "@LUID:" + this.UID,
                        "xml",
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送 MQ 消息时出错: " + strError + "\r\n";
                    }
                }
                catch (Exception ex)
                {
                    strTotalError += "创建路径为 '" + this.OutgoingQueue + "' 的 MessageQueue 对象失败: " + ExceptionUtil.GetDebugText(ex);
                }
            }

            // ** email
            if (String.IsNullOrEmpty(strReaderEmailAddress) == false
                && StringUtil.IsInList("email", this.ArrivedNotifyTypes))
            {
                string strTemplate = "";
                // 获得邮件模板
                nRet = GetMailTemplate(
                    "email",
                    "放弃取书通知",
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
#if NO
                    strError = "预约到书通知<mailTemplate/template>尚未配置。";
                    return -1;
#endif
                    return 0;   // 没有配置模板就不通知
                }

                /*
                %item%  册信息
                %reservetime%   保留期限
                %today% 发出email的当天
                %summary% 书目摘要
                %itembarcode% 册条码号 
                %name% 读者姓名
                 * */
                Hashtable table = new Hashtable();
                table["%item%"] = "(册条码号为: " + strItemBarcode + " URL为: " + this.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + " )";
                table["%today%"] = DateTime.Now.ToString();
                table["%summary%"] = strSummary;
                table["%itembarcode%"] = strItemBarcode;
                table["%name%"] = strName;

                string strBody = "";

                nRet = GetMailText(strTemplate,
                    table,
                    out strBody,
                    out strError);
                if (nRet == -1)
                    return -1;

                {
                    // 发送email
                    // return:
                    //      -1  error
                    //      0   not found smtp server cfg
                    //      1   succeed
                    nRet = SendEmail(strReaderEmailAddress,
                        "放弃取书通知",
                        strBody,
                        "text",
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送email消息时出错: " + strError + "\r\n";
                    }
                }
            }

            // *** external messageinterfaces
            if (this.m_externalMessageInterfaces != null)
            {
                foreach (MessageInterface message_interface in this.m_externalMessageInterfaces)
                {
                    // types
                    if (StringUtil.IsInList(message_interface.Type, this.ArrivedNotifyTypes) == false)
                        continue;

                    string strTemplate = "";
                    // 获得邮件模板
                    nRet = GetMailTemplate(
                        message_interface.Type,
                        "放弃取书通知",
                        out strTemplate,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
#if NO
                        strError = "预约到书通知<mailTemplate/template>尚未配置。";
                        return -1;
#endif
                        return 0;   // 没有配置模板就不通知
                    }

                    /*
                    %item%  册信息
                    %reservetime%   保留期限
                    %today% 发出email的当天
                %summary% 书目摘要
                %itembarcode% 册条码号 
                %name% 读者姓名
                     * */
                    Hashtable table = new Hashtable();
                    table["%item%"] = "(册条码号为: " + strItemBarcode + " URL为: " + this.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + " )";
                    table["%today%"] = DateTime.Now.ToString();
                    table["%summary%"] = strSummary;
                    table["%itembarcode%"] = strItemBarcode;
                    table["%name%"] = strName;

                    string strBody = "";

                    nRet = GetMailText(strTemplate,
                        table,
                        out strBody,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 发送消息
                    nRet = message_interface.HostObj.SendMessage(
                        strReaderBarcode,
                        strReaderXml,
                        strBody,
                        strLibraryCode,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送" + message_interface.Type + "消息时出错: " + strError + "\r\n";
                    }
                }
            }

            if (String.IsNullOrEmpty(strTotalError) == false)
            {
                strError = strTotalError;
                return -1;
            }

            return 0;
        }

        // TODO: 各个环节要改为尽量使用 refID。要做大量测试
        // text-level: 内部处理
        // 在 预约到书 库中，追加一条新的记录，并作 email / dpmail / mq 通知
        // 注：本函数可能要删除部分通知记录
        // parameters:
        //      strItemBarcode  册条码号。必须是册条码号。如果册条码号为空，参考ID需要使用 strRefID 参数
        //      strRefID        参考ID
        //      bOnShelf    要通知的册是否在架。在架指并没有人借阅过，本来就在书架上。
        //      strLibraryCode  读者所在的馆代码
        //      strReaderXml    预约了图书的读者的XML记录。用于消息通知接口
        int AddNotifyRecordToQueueDatabase(
            RmsChannel channel,
            string strItemBarcodeParam,
            string strRefIDParam,
            string strItemXml,
            bool bOnShelf,
            string strLibraryCode,
            string strReaderBarcode,
            string strReaderXml,
            string strNotifyID,
            out List<string> DeletedNotifyRecPaths,
            out string strError)
        {
            strError = "";
            DeletedNotifyRecPaths = new List<string>();

            // 2010/12/31
            if (String.IsNullOrEmpty(this.ArrivedDbName) == true)
            {
                strError = "预约到书库尚未定义, AddNotifyRecordToQueue()调用失败";
                return -1;
            }

            // 准备写记录
            byte[] timestamp = null;
            byte[] output_timestamp = null;
            string strOutputPath = "";
            int nRet = 0;
            long lRet = 0;

            if (String.IsNullOrEmpty(strItemBarcodeParam) == true)
            {
                // 如果检索用的册条码号为空，加上对命中结果数量不设限，那就会造成系统严重繁忙。
                strError = "参数strItemBarcode中的册条码号不能为空。";
                return -1;
            }

#if NO
            RmsChannel channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            REDODELETE:
            // 如果队列中已经存在同册条码号的记录, 要先删除
            string strNotifyXml = "";
            // 获得预约到书队列记录
            // parameters:
            //      strItemBarcodeParam  册条码号。可以使用 @itemRefID: 前缀
            // return:
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            nRet = GetArrivedQueueRecXml(
                // channels,
                channel,
                strItemBarcodeParam.Replace("@refID:", "@itemRefID:"),
                out strNotifyXml,
                out timestamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                // 写入错误日志?
                this.WriteErrorLog("在还书操作中，检索册条码号为 " + strItemBarcodeParam + " 的预约到书库记录时出错: " + strError);
            }
            if (nRet >= 1)
            {
                int nRedoDeleteCount = 0;
                // TODO: 这一段删除代码可以专门编制在一个函数中，不必这么费力循环。可以优化处理
                REDO_DELETE:
                lRet = channel.DoDeleteRes(strOutputPath,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    // 时间戳不匹配
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoDeleteCount < 10)
                    {
                        nRedoDeleteCount++;
                        timestamp = output_timestamp;
                        goto REDO_DELETE;
                    }

                    // 写入错误日志?
                    this.WriteErrorLog("在还书操作中，加入新预约到书记录前, 删除已存在的预约到书库记录 '" + strOutputPath + "' 出错: " + strError);
                }

                DeletedNotifyRecPaths.Add(strOutputPath);    // 记忆已经被删除的记录路径 2007/7/5 

                goto REDODELETE;    // 如果有多条，循环删除
            }

            XmlDocument itemdom = null;
            nRet = LibraryApplication.LoadToDom(strItemXml,
                out itemdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载册记录 '" + strItemBarcodeParam + "' 的 XML 进入 DOM 时发生错误: " + strError;
                return -1;
            }

            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);

            string strAccessNo = DomUtil.GetElementText(itemdom.DocumentElement, "accessNo");

            /*
  <reservations>
        <request reader="R0000001" requestDate="Mon, 05 Sep 2016 16:57:47 +0800" operator="R0000001" /> 
  </reservations>
             * */
            // 从册记录 reservations 元素下找第一个 request 元素，其 requestDate 属性
            string strRequestDate = "";
            XmlElement request = itemdom.DocumentElement.SelectSingleNode("reservations/request") as XmlElement;
            if (request != null)
                strRequestDate = request.GetAttribute("requestDate");

            // 创建预约到书记录
            XmlDocument new_queue_dom = new XmlDocument();
            new_queue_dom.LoadXml("<root />");

            DomUtil.SetElementText(new_queue_dom.DocumentElement, "state", "arrived");

            // TODO: 以后增加 <refID> 元素，存储册记录的参考ID

#if NO
            XmlNode nodeItemBarcode = DomUtil.SetElementText(dom.DocumentElement, "itemBarcode", strItemBarcode);

            // 在<itemBarcode>元素中增加一个onShelf属性，表示属于在架情况
            Debug.Assert(nodeItemBarcode != null, "");
            if (bOnShelf == true)
                DomUtil.SetAttr(nodeItemBarcode, "onShelf", "true");
#endif
            string strItemRefID = "";   // 计划存储纯粹的 refid
            string strItemBarcode = strItemBarcodeParam;    // 计划存储纯粹的册条码号

            // 兼容 strItemBarcode 中含有前缀的用法
            string strHead = "@refID:";
            if (StringUtil.HasHead(strItemBarcodeParam, strHead, true) == true)
            {
                strItemRefID = strItemBarcodeParam.Substring(strHead.Length);
                strItemBarcode = "";
            }

            string strUnionItemBarcode = GetUnionBarcode(strItemBarcode, strItemRefID);

            if (this.ArrivedDbKeysContainsRefIDKey() == true)
            {
                DomUtil.SetElementText(new_queue_dom.DocumentElement, "itemBarcode", strItemBarcode);
                DomUtil.SetElementText(new_queue_dom.DocumentElement, "itemRefID", strItemRefID);  // 原来元素名是 "refID"，2016/12/4 修改为 itemRefID
            }
            else
            {
                if (string.IsNullOrEmpty(strItemBarcode) == true)
                {
                    if (string.IsNullOrEmpty(strItemRefID) == true)
                    {
                        strError = "AddNotifyRecordToQueue() 函数当 strItemBarcode 参数为空的时候，必须让 strItemRefID 参数不为空";
                        return -1;
                    }

                    Debug.Assert(string.IsNullOrEmpty(strItemRefID) == false, "");
                    // 旧的用法。避免检索时候查不到
                    DomUtil.SetElementText(new_queue_dom.DocumentElement, "itemBarcode", "@refID:" + strItemRefID);
                }
                else
                    DomUtil.SetElementText(new_queue_dom.DocumentElement, "itemBarcode", strItemBarcode); // 2015/5/20 添加，修正 BUG
            }

            // 改为存储在元素中 2015/5/7
            if (bOnShelf == true)
                DomUtil.SetElementText(new_queue_dom.DocumentElement, "onShelf", "true");
            else
                DomUtil.SetElementText(new_queue_dom.DocumentElement, "box", "#reservation");   // 表示在保留大架，混合存放

            // 2012/10/26
            DomUtil.SetElementText(new_queue_dom.DocumentElement, "libraryCode", strLibraryCode);

            DomUtil.SetElementText(new_queue_dom.DocumentElement, "readerBarcode", strReaderBarcode);
            DomUtil.SetElementText(new_queue_dom.DocumentElement, "notifyDate", this.Clock.GetClock());

            // 2016/12/4
            // 预约到书记录的参考 ID
            DomUtil.SetElementText(new_queue_dom.DocumentElement, "refID", strNotifyID);

            // 2015/6/13
            DomUtil.SetElementText(new_queue_dom.DocumentElement, "location", strLocation);
            // 2016/12/4
            DomUtil.SetElementText(new_queue_dom.DocumentElement, "accessNo", strAccessNo);

            string strPath = this.ArrivedDbName + "/?";

            // 写新记录
            lRet = channel.DoSaveTextRes(
                strPath,
                new_queue_dom.OuterXml,
                false,
                "content,ignorechecktimestamp",
                timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // 写入错误日志 2007/1/3 
                this.WriteErrorLog("创建新的预约到书队列记录时出错: " + strError);
                return -1;
            }

            string strReaderEmailAddress = "";
            string strName = "";
            nRet = GetReaderNotifyInfo(
                strReaderXml,
                out strName,
                out strReaderEmailAddress,
                out strError);
            if (nRet == -1)
                return -1;

            // 获得图书摘要信息
            string strSummary = "";     // 没有被截断的摘要字符串
            string strShortSummary = "";    // 截断后的摘要字符串
            string strBiblioRecPath = "";

            nRet = this.GetBiblioSummary(strUnionItemBarcode,
                "", //  strConfirmItemRecPath,
                null,   //  strBiblioRecPathExclude,
                -1, // 25,
                out strBiblioRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
            {
                strSummary = "ERROR: " + strError;
            }
            else
            {
                strShortSummary = LibraryApplication.CutSummary(strSummary, 25);
            }

#if NO
            // 临时的SessionInfo对象
            SessionInfo sessioninfo = new SessionInfo(this);
            // 模拟一个账户
            Account account = new Account();
            account.LoginName = "CacheBuilder";
            account.Password = "";
            account.Rights = "getbibliosummary";

            account.Type = "";
            account.Barcode = "";
            account.Name = "AddNotifyRecordToQueue";
            account.UserID = "AddNotifyRecordToQueue";
            account.RmsUserName = this.ManagerUserName;
            account.RmsPassword = this.ManagerPassword;

            sessioninfo.Account = account;
            try
            {

                string strBiblioRecPath = "";
                LibraryServerResult result = this.GetBiblioSummary(
                    sessioninfo,
                    strItemBarcode,
                    "", // strConfirmItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1)
                {
                    strSummary = "ERROR: " + result.ErrorInfo;
                }
                else
                {
                    // 截断
                    if (strSummary.Length > 25)
                        strSummary = strSummary.Substring(0, 25) + "...";
                }
            }
            finally
            {
                sessioninfo.Close();
                sessioninfo = null;
            }
#endif

            // 发送短消息通知
            string strTotalError = "";

            // *** dpmail
            if (this.MessageCenter != null
                && StringUtil.IsInList("dpmail", this.ArrivedNotifyTypes))
            {
                string strTemplate = "";
                // 获得邮件模板
                nRet = GetMailTemplate(
                    "dpmail",
                    bOnShelf == false ? "预约到书通知" : "预约到书通知(在架)",
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "预约到书通知<mailTemplate/template>尚未配置。";
                    return -1;
                }

                /*
                %item%  册信息
                %reservetime%   保留期限
                %today% 发出email的当天
                %summary% 书目摘要
                %itembarcode% 册条码号 
                %name% 读者姓名
                 * */
                Hashtable table = new Hashtable();
                table["%item%"] = "(册条码号为: " + strUnionItemBarcode + " URL为: " + this.OpacServerUrl + "/book.aspx?barcode=" + strUnionItemBarcode + " )";
                table["%reservetime%"] = this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan);
                table["%today%"] = DateTime.Now.ToString();
                table["%summary%"] = strShortSummary;
                table["%itembarcode%"] = strUnionItemBarcode;
                table["%name%"] = strName;
                string strBody = "";

                nRet = GetMailText(strTemplate,
                    table,
                    out strBody,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(this.MessageCenter.MessageDbName) == false)
                {
                    Debug.Assert(channel.Container != null, "");
                    // 发送消息
                    nRet = this.MessageCenter.SendMessage(
                        channel.Container,  // channels,
                        strReaderBarcode,
                        "图书馆",
                        "预约到书通知",
                        "text",
                        strBody,
                        false,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送dpmail消息时出错: " + strError + "\r\n";
                    }
                }
            }

            // 2016/4/26
            // *** mq
            if (string.IsNullOrEmpty(this.OutgoingQueue) == false
                && StringUtil.IsInList("mq", this.ArrivedNotifyTypes))
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                /* 元素名
                 * type 消息类型。预约到书通知
                 * itemBarcode 册条码号
                 * location 馆藏地 2016/9/5
                 * itemRefID    册的参考 ID
                 * notifyID     预约到书通知记录的 ID
                 * onShelf 是否在架。true/false
                 * opacURL  图书在 OPAC 中的 URL。相对路径
                 * requestDate 预约请求创建时间 2016/9/5
                 * reserveTime 保留的时间
                 * today 今天的日期
                 * summary 书目摘要
                 * patronName   读者姓名
                 * patronRecord 读者 XML 记录
 * */
                DomUtil.SetElementText(dom.DocumentElement, "type", "预约到书通知");

                DomUtil.SetElementText(dom.DocumentElement,
                    "itemBarcode", strItemBarcode);
                DomUtil.SetElementText(dom.DocumentElement,
                    "location", strLocation);
                // 2016/11/15
                DomUtil.SetElementText(dom.DocumentElement,
                    "accessNo", strAccessNo);

                // 2016/9/5
                if (string.IsNullOrEmpty(strItemRefID))
                    strItemRefID = DomUtil.GetElementText(itemdom.DocumentElement, "refID");

                DomUtil.SetElementText(dom.DocumentElement,
                    "itemRefID", strItemRefID); // 以前为 refID。2016/12/4 修改为 itemRefID
                // 2016/12/4
                if (string.IsNullOrEmpty(strNotifyID) == false)
                    DomUtil.SetElementText(dom.DocumentElement,
                        "notifyID", strNotifyID);

                DomUtil.SetElementText(dom.DocumentElement,
                    "onShelf", bOnShelf ? "true" : "false");

                DomUtil.SetElementText(dom.DocumentElement,
                    "opacURL", this.OpacServerUrl + "/book.aspx?barcode="
                    + strUnionItemBarcode);
                DomUtil.SetElementText(dom.DocumentElement,
                    "requestDate", strRequestDate);
                DomUtil.SetElementText(dom.DocumentElement,
                    "reserveTime", this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan));
                DomUtil.SetElementText(dom.DocumentElement,
                    "today", DateTime.Now.ToString());
                DomUtil.SetElementText(dom.DocumentElement,
                    "summary", strSummary);
                DomUtil.SetElementText(dom.DocumentElement,
                    "patronName", strName);

                string strReaderRefID = "";
                {
                    XmlDocument readerdom = new XmlDocument();
                    readerdom.LoadXml(strReaderXml);

                    strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");

                    XmlElement record = dom.CreateElement("patronRecord");
                    dom.DocumentElement.AppendChild(record);
                    record.InnerXml = readerdom.DocumentElement.InnerXml;

                    DomUtil.DeleteElement(record, "borrowHistory");
                    DomUtil.DeleteElement(record, "password");
                    DomUtil.DeleteElement(record, "fingerprint");
                    DomUtil.DeleteElement(record, "face");
                    DomUtil.SetElementText(record, "libraryCode", strLibraryCode);
                }

                try
                {
                    MessageQueue queue = new MessageQueue(this.OutgoingQueue);

                    // 向 MSMQ 消息队列发送消息
                    nRet = ReadersMonitor.SendToQueue(queue,
                        (string.IsNullOrEmpty(strReaderRefID) ? strReaderBarcode : "!refID:" + strReaderRefID)
                        + "@LUID:" + this.UID,
                        "xml",
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送 MQ 消息时出错: " + strError + "\r\n";
                    }
                }
                catch (Exception ex)
                {
                    strTotalError += "创建路径为 '" + this.OutgoingQueue + "' 的 MessageQueue 对象失败: " + ExceptionUtil.GetDebugText(ex);
                }
            }

            // ** email
            if (String.IsNullOrEmpty(strReaderEmailAddress) == false
                && StringUtil.IsInList("email", this.ArrivedNotifyTypes))
            {
                string strTemplate = "";
                // 获得邮件模板
                nRet = GetMailTemplate(
                    "email",
                    bOnShelf == false ? "预约到书通知" : "预约到书通知(在架)",
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "预约到书通知<mailTemplate/template>尚未配置。";
                    return -1;
                }

                /*
                %item%  册信息
                %reservetime%   保留期限
                %today% 发出email的当天
                %summary% 书目摘要
                %itembarcode% 册条码号 
                %name% 读者姓名
                 * */
                Hashtable table = new Hashtable();
                table["%item%"] = "(册条码号为: " + strUnionItemBarcode + " URL为: " + this.OpacServerUrl + "/book.aspx?barcode=" + strUnionItemBarcode + " )";
                table["%reservetime%"] = this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan);
                table["%today%"] = DateTime.Now.ToString();
                table["%summary%"] = strShortSummary;
                table["%itembarcode%"] = strUnionItemBarcode;
                table["%name%"] = strName;

                string strBody = "";

                nRet = GetMailText(strTemplate,
                    table,
                    out strBody,
                    out strError);
                if (nRet == -1)
                    return -1;

                {
                    // 发送email
                    // return:
                    //      -1  error
                    //      0   not found smtp server cfg
                    //      1   succeed
                    nRet = SendEmail(strReaderEmailAddress,
                        "预约到书通知",
                        strBody,
                        "text",
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送email消息时出错: " + strError + "\r\n";
                    }
                }
            }

            // *** external messageinterfaces
            if (this.m_externalMessageInterfaces != null)
            {
                foreach (MessageInterface message_interface in this.m_externalMessageInterfaces)
                {
                    // types
                    if (StringUtil.IsInList(message_interface.Type, this.ArrivedNotifyTypes) == false)
                        continue;

                    string strTemplate = "";
                    // 获得邮件模板
                    nRet = GetMailTemplate(
                        message_interface.Type,
                        bOnShelf == false ? "预约到书通知" : "预约到书通知(在架)",
                        out strTemplate,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "预约到书通知<mailTemplate/template>尚未配置。";
                        return -1;
                    }

                    /*
                    %item%  册信息
                    %reservetime%   保留期限
                    %today% 发出email的当天
                %summary% 书目摘要
                %itembarcode% 册条码号 
                %name% 读者姓名
                     * */
                    Hashtable table = new Hashtable();
                    table["%item%"] = "(册条码号为: " + strUnionItemBarcode + " URL为: " + this.OpacServerUrl + "/book.aspx?barcode=" + strUnionItemBarcode + " )";
                    table["%reservetime%"] = this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan);
                    table["%today%"] = DateTime.Now.ToString();
                    table["%summary%"] = strShortSummary;
                    table["%itembarcode%"] = strUnionItemBarcode;
                    table["%name%"] = strName;

                    string strBody = "";

                    nRet = GetMailText(strTemplate,
                        table,
                        out strBody,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 发送消息
                    nRet = message_interface.HostObj.SendMessage(
                        strReaderBarcode,
                        strReaderXml,
                        strBody,
                        strLibraryCode,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "发送" + message_interface.Type + "消息时出错: " + strError + "\r\n";
                    }
                }
            }

            if (String.IsNullOrEmpty(strTotalError) == false)
            {
                strError = strTotalError;
                return -1;
            }

            return 0;
        }

        // text-level: 内部处理
        // 获得和读者通知有关的信息
        int GetReaderNotifyInfo(
            string strReaderXml,
            out string strName,
            out string strReaderEmailAddress,
            out string strError)
        {
            strError = "";
            strReaderEmailAddress = "";
            strName = "";

            XmlDocument readerdom = null;
            int nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            strReaderEmailAddress =
                LibraryServerUtil.GetEmailAddress(
                DomUtil.GetElementText(readerdom.DocumentElement, "email")
                );

            strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");

            return 0;
        }

#if NO
        // text-level: 内部处理
        // 获得和读者通知有关的信息
        int GetReaderNotifyInfo(
            RmsChannelCollection channels,
            string strReaderBarcode,
            out string strName,
            out string strReaderEmailAddress,
            out string strError)
        {
            strError = "";
            strReaderEmailAddress = "";
            strName = "";

            // 读入读者记录
            string strReaderXml = "";
            string strOutputReaderRecPath = "";
            byte[] reader_timestamp = null;
            int nRet = this.GetReaderRecXml(
                channels,
                strReaderBarcode,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == 0)
            {
                strError = "读者记录 '" + strReaderBarcode + "' 没有找到。";
                return -1;
            }
            if (nRet == -1)
            {
                strError = "读入读者记录时发生错误: " + strError;
                return -1;
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            strReaderEmailAddress = DomUtil.GetElementText(readerdom.DocumentElement,
                "email");

            strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");

            return 0;
        }

#endif

        // text-level: 内部处理
        // 通知下一个预约者，或者(因没有下一个预约者了)而归架
        // 调用前，需要先获得预约队列记录
        // return:
        //      -1  error
        //      0   正常
        //      1   后面已经没有预约者，已通知管理员归架
        public int DoNotifyNext(
            CachedRecordCollection records,
            RmsChannel channel,
            string strQueueRecPath,
            XmlDocument queue_rec_dom,
            byte[] baQueueRecTimeStamp,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            // RmsChannel channel = null;
            byte[] output_timestamp = null;

            //string strState = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
            //    "state");

            string strNotifyDate = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "notifyDate");
            // XmlNode nodeItemBarcode = null;
            string strItemBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "itemBarcode"/*, out nodeItemBarcode*/);

            // 2015/5/7
            string strRefID = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
    "refID");

            string strReaderBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "readerBarcode");

            bool bOnShelf = false;
#if NO
            // <itemBarcode>元素是否有onShelf属性。
            if (nodeItemBarcode != null)
            {
                string strOnShelf = DomUtil.GetAttr(nodeItemBarcode, "onShelf");
                if (strOnShelf.ToLower() == "true"
                    || strOnShelf.ToLower() == "yes"
                    || strOnShelf.ToLower() == "on")
                    bOnShelf = true;
            }
#endif
            // 2015/5/7
            // <onShelf> 元素
            {
                string strOnShelf = DomUtil.GetElementText(queue_rec_dom.DocumentElement, "onShelf");
                if (DomUtil.IsBooleanTrue(strOnShelf, false) == true)
                    bOnShelf = true;
            }

            // 2015/5/7
            if (string.IsNullOrEmpty(strItemBarcode) == true && string.IsNullOrEmpty(strRefID) == false)
            {
                strItemBarcode = "@refID:" + strRefID;
            }

            // 要通知下一位预约者

            string strReservationReaderBarcode = "";
            string strNotifyID = "";

            // 清除读者和册记录中的已到预约事项，并提取下一个预约读者证条码号
            // 本函数还负责清除册记录中以前残留的state=arrived的<request>元素
            nRet = this.ClearArrivedInfo(
                records,
                channel,
                strReaderBarcode,
                strItemBarcode,
                bOnShelf,
                out strReservationReaderBarcode,
                out strNotifyID,
                out strError);
            if (nRet == -1)
                return -1;

            // 3) 通知预约到书的操作
            List<string> DeletedNotifyRecPaths = null;  // 被删除的通知记录。

            if (String.IsNullOrEmpty(strReservationReaderBarcode) == false)
            {
                // 通知下一读者

                // 出于对读者库加锁方面的便利考虑, 单独做了此函数
                // return:
                //      -1  error
                //      0   没有找到<request>元素
                //      1   已成功处理
                nRet = this.DoReservationNotify(
                    records,
                    channel,
                    strReservationReaderBarcode,
                    true,
                    strItemBarcode,
                    bOnShelf,
                    false,   // 不要修改当前册记录的<request> state属性，因为前面ClearArrivedInfo()中已经调用了DoItemReturnReservationCheck(), 修改了当前册的<request> state属性。
                    strNotifyID,
                    out DeletedNotifyRecPaths,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                // outof 的记录何时删除？
                // 册记录中的馆藏地点 #reservation何时消除？一个是现在就消除，一个是盘点模块扫描条码时消除。

                // 把记录状态修改为 outofreservation
                DomUtil.SetElementText(queue_rec_dom.DocumentElement,
                    "state",
                    "outof");

                // channel = channels.GetChannel(this.WsUrl);

                string strOutputPath = "";
                lRet = channel.DoSaveTextRes(strQueueRecPath,
                    queue_rec_dom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    baQueueRecTimeStamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "写回预约到书记录 '" + strQueueRecPath + "' 时发生错误: " + strError;
                    return -1;
                }

                // TODO: 通知馆员进行上架操作
                // 可以在系统某目录不断追加到一个文本文件，工作人员可以查对。
                // 格式：每行 册条码号 最后一个预约的读者证条码号
                if (String.IsNullOrEmpty(this.StatisDir) == false)
                {
                    string strLogFileName = this.StatisDir + "\\outof_reservation_" + Statis.GetCurrentDate() + ".txt";
                    StreamUtil.WriteText(strLogFileName, strItemBarcode + " " + strReaderBarcode + "\r\n");
                }

                return 1;
            }

            // 4) 删除当前通知记录

            // 2007/7/5 
            bool bAlreadeDeleted = false;
            if (DeletedNotifyRecPaths != null)
            {
                if (DeletedNotifyRecPaths.IndexOf(strQueueRecPath) != -1)
                    bAlreadeDeleted = true;
            }

            if (bAlreadeDeleted == false)
            {
                lRet = channel.DoDeleteRes(
                    strQueueRecPath,
                    baQueueRecTimeStamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "DoNotifyNext()删除通知记录 '" + strQueueRecPath + "' 时失败: " + strError;
                    return -1;
                }
            }

            return 0;
        }

        public static string GetUnionBarcode(string strBarcode, string strRefID)
        {
            if (string.IsNullOrEmpty(strBarcode) == false)
                return strBarcode;
            return "@refID:" + strRefID;
        }
    }
}
