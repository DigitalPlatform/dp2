using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Linq;

using Jint.Parser.Ast;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryServer.Common;
using static DigitalPlatform.LibraryServer.BatchTask;



namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和日志恢复相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        static bool IsSnapshot(RecoverLevel level,
            out bool robust)
        {
            robust = ((level & RecoverLevel.RobustMask) == RecoverLevel.RobustMask);
            if ((level & RecoverLevel.Snapshot) == RecoverLevel.Snapshot)
                return true;
            return false;
        }

        static bool IsSnapshot(RecoverLevel level)
        {
            return IsSnapshot(level, out _);
        }

        static bool IsLogic(RecoverLevel level,
    out bool robust)
        {
            robust = ((level & RecoverLevel.RobustMask) == RecoverLevel.RobustMask);
            if ((level & RecoverLevel.Logic) == RecoverLevel.Logic)
                return true;
            return false;
        }

        static bool IsLogic(RecoverLevel level)
        {
            return IsLogic(level, out _);
        }

        // Borrow() API 恢复动作
        /* 日志记录格式如下
<root>
  <operation>borrow</operation> 操作类型
  <action>borrow</action>  动作 borrow/renew
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <itemBarcode>0000001</itemBarcode>  册条码号
  <borrowDate>Fri, 08 Dec 2006 04:17:31 GMT</borrowDate> 借阅日期
  <borrowPeriod>30day</borrowPeriod> 借阅期限
  <no>0</no> 续借次数。0为首次普通借阅，1开始为续借
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:31 GMT</operTime> 操作时间
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
</root>
         * */
        // parameters:
        //      level   恢复级别。
        //      bForce  是否为容错状态。在容错状态下，如果遇到重复的册条码号，就算做第一条。
        public int RecoverBorrow(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            bool bForce,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            // bool bMissing = false;  // 是否缺失快照信息?

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 重试时改用的策略
            RecoverLevel retry_level = level;

            string reader_xml = ""; // 所取得的读者记录 XML
            string reader_recpath = ""; // 用于指示保存回到读者记录时的记录路径

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                string strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺 <readerRecord> 元素";
                    return -1;
                }

                string strReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 2017/1/12
                bool bClipping = DomUtil.GetBooleanParam(node, "clipping", false);
                if (bClipping == true)
                {
                    // ***
                    // 尝试从数据库中获得这一条读者记录(并把它与 clipping 状态的这一条合并)。
                    // 如果数据库中找不到，则只好直接用这条读者记录(去掉全部 clipping 属性)
                    // 注: 因为 clipping 现象并不算一种“错误”，所以也谈不上“容错”，处理这种情况就被看作 Snapshot 模式的基本能力
                    var queries = new List<string>();
                    queries.Add($"@path:{strReaderRecPath}");
                    var refID = GetXmlRefID(strReaderXml);
                    if (string.IsNullOrEmpty(refID) == false)
                        queries.Add($"@refID:{refID}");
                    nRet = TryLoadReaderRecord(channel,
                        queries,
                        out reader_xml,
                        out _,
                        out reader_recpath,
                        out strError);
                    // nRet = 0;   // testing
                    if (nRet == 0)
                    {
                        // 从数据库中没有找到读者记录，只好勉强直接用 strReaderXml
                        RemoveClipping(ref strReaderXml);
                        reader_xml = strReaderXml;
                        reader_recpath = strReaderRecPath;
                        retry_level = RecoverLevel.LogicRobust;
                        goto DO_LOGIC;
                    }
                    if (nRet == 1)
                    {
                        // 从数据库中读出记录，然后用 readerRecord 中的可信局部去替代，形成结果记录内容
                        MergeXml(ref reader_xml, strReaderXml);
                        retry_level = RecoverLevel.LogicRobust;
                        goto DO_LOGIC;
                    }
                    strError = "日志记录中 <readerRecord> 元素为 clipping 状态(并且尝试从数据库中获取记录失败)，无法进行快照恢复";
                    return -1;
                }


                string strItemXml = DomUtil.GetElementText(domLog.DocumentElement,
    "itemRecord",
    out node);
                if (node == null)
                {
                    strError = "日志记录格式错误: 缺 <itemRecord> 元素";
                    return -1;
                }
                string strItemRecPath = DomUtil.GetAttr(node, "recPath");

                // 2024/3/27
                RemoveReservations(ref strReaderXml);

                // 写读者记录
                nRet = TryWriteReaderRecord(
                    channel,
                    strReaderRecPath,
                    strReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写读者记录
                    lRet = channel.DoSaveTextRes(strReaderRecPath,
        strReaderXml,
        false,
        "content,ignorechecktimestamp",
        null,   // timestamp,
        out byte[] output_timestamp,
        out string strOutputPath,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                // 写册记录
                nRet = TryWriteItemRecord(channel,
    strItemRecPath,
    strItemXml,
    "entity",
    robust,
    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写册记录
                    lRet = channel.DoSaveTextRes(strItemRecPath,
    strItemXml,
    false,
    "content,ignorechecktimestamp",
    null,   // timestamp,
    out _,
    out string strOutputPath,
    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateBiblioDatabase(
    channel,
    strItemRecPath,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strItemRecPath}' 临时决定创建书目库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入册记录 '" + strItemRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                return 0;
            }

        DO_LOGIC:
            // 逻辑恢复或者混合恢复
            if (IsLogic(level, out robust)
                || IsLogic(retry_level))
            {
                string strRecoverComment = "";

                if (string.IsNullOrEmpty(reader_xml))
                {
                    string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerBarcode");
                    string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerRefID");
                    // TODO: 如果日志记录中没有 readerRefID 元素，可以尝试从 readerRecord 元素内嵌的记录 XML 中寻找 refID 元素
                    var ret = TryGetUnionRefID(strReaderBarcode,
                        strReaderRefID,
                        out string strReaderKey);
                    if (ret == false)
                    {
                        strError = "日志记录中 readerBarcode 元素值为空，并且 readerRefID 元素值也为空";
                        goto ERROR1;
                    }

                    /*
                    // 读入读者记录
                    nRet = this.GetReaderRecXml(
                        // Channels,
                        channel,
                        strReaderKey,
                        out string strReaderXml,
                        out string strOutputReaderRecPath,
                        out byte[] reader_timestamp,
                        out strError);
                    */
                    nRet = LoadReaderRecord(
                        channel,
                        strReaderKey,
                        null,
                        domLog,
                        robust,
                        null,
                        out reader_xml,
                        out byte[] reader_timestamp,
                        out reader_recpath,
                        out strError);
                    if (nRet == 0)
                    {
                        strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                        goto ERROR1;
                    }
                    if (nRet == -1)
                    {
                        strError = "读入证条码号为 '" + strReaderKey + "' 的读者记录时发生错误: " + strError;
                        goto ERROR1;
                    }
                }

                // 获得读者库的馆代码
                // return:
                //      -1  出错
                //      0   成功
                nRet = GetLibraryCode(
                        reader_recpath,
                        out string strLibraryCode,
                        out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LibraryApplication.LoadToDom(reader_xml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 读入册记录
                //string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                //    "confirmItemRecPath");
                string strConfirmItemRecPath = GetConfirmRecPath(domLog);

                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                string strItemRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemRefID");

                // TODO: 如果日志记录中没有 itemRefID 元素，可以尝试从 itemRecord 元素内嵌的记录 XML 中寻找 refID 元素
                var ret1 = TryGetUnionRefID(strItemBarcode,
                    strItemRefID,
                    out string strItemKey);
                if (ret1 == false)
                {
                    strError = "itemBarcode 元素值为空，并且 itemRefID 元素值也为空";
                    goto ERROR1;
                }

#if REMOVED
                string strItemXml = "";
                string strOutputItemRecPath = "";
                byte[] item_timestamp = null;

                // 如果已经有确定的册记录路径
                if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                {
                    lRet = channel.GetRes(strConfirmItemRecPath,
                        out strItemXml,
                        out string strMetaData,
                        out item_timestamp,
                        out strOutputItemRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                        goto ERROR1;
                    }

                    // 需要检查记录中的<barcode>元素值是否匹配册条码号


                    // TODO: 如果记录路径所表达的记录不存在，或者其<barcode>元素值和要求的册条码号不匹配，那么都要改用逻辑方法，也就是利用册条码号来获得记录。
                    // 当然，这种情况下，非常要紧的是确保数据库的素质很好，本身没有重条码号的情况出现。
                }
                else
                {
                    // 从册条码号获得册记录

                    int nRedoCount = 0;
                REDO_GETITEM:
                    // 获得册记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetItemRecXml(
                        // Channels,
                        channel,
                        strItemKey,
                        out strItemXml,
                        100,
                        out List<string> aPath,
                        out item_timestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        // 尝试用 itemRecord 中的 XML 内容恢复这条册记录
                        // 根据日志记录中的特定元素，尝试恢复册记录
                        // return:
                        //      -2  nRedoCount 超过范围
                        //      -1  出错
                        //      0   日志记录中没有找到需要的元素
                        //      1   成功。nRedoCount 自动增量 1
                        nRet = RecoverItemRecord(
    channel,
    domLog,
    "itemRecord",
    ref nRedoCount,
    out string error);
                        if (nRet != 1)
                        {
                            strError = "册条码号 '" + strItemKey + "' 不存在";
                            goto ERROR1;
                        }
                        goto REDO_GETITEM;
                    }
                    if (nRet == -1)
                    {
                        strError = "读入册条码号为 '" + strItemKey + "' 的册记录时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (aPath.Count > 1)
                    {
                        if (bForce == true)
                        {
                            // 容错！
                            strOutputItemRecPath = aPath[0];

                            strRecoverComment += "册条码号 " + strItemKey + " 有 "
                                + aPath.Count.ToString() + " 条重复记录，因受容错要求所迫，权且采用其中第一个记录 "
                                + strOutputItemRecPath + " 来进行借阅操作。";
                        }
                        else
                        {
                            strError = "册条码号为 '" + strItemKey + "' 的册记录有 " + aPath.Count.ToString() + " 条，但此时confirmItemRecPath却为空";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        Debug.Assert(nRet == 1, "");
                        Debug.Assert(aPath.Count == 1, "");

                        if (nRet == 1)
                        {
                            strOutputItemRecPath = aPath[0];
                        }
                    }
                }

#endif
                nRet = LoadItemRecord(channel,
    strItemKey,
    strConfirmItemRecPath,
    domLog,
    bForce,
    (List<string> paths,
    ref string item_xml,
    ref byte[] timestamp,
    ref string item_recpath,
    out string error) =>
    {
        error = "";

        if (bForce == true)
        {
            // 容错！
            item_recpath = paths[0];

            strRecoverComment += "册条码号 " + strItemKey + " 有 "
                + paths.Count.ToString() + " 条重复记录，因受容错要求所迫，权且采用其中第一个记录 "
                + item_recpath + " 来进行借阅操作。";
        }
        else
        {
            error = "册条码号为 '" + strItemKey + "' 的册记录有 " + paths.Count.ToString() + " 条，但此时confirmItemRecPath却为空";
            return -1;
        }
        return 0;
    },
    out string strItemXml,
    out byte[] item_timestamp,
    out string strOutputItemRecPath,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out XmlDocument itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 修改读者记录
                // 修改册记录

                // TODO: 容错情况下如果遇到册条码号是重复的，要写入额外的日志。
                nRet = BorrowChangeReaderAndItemRecord(
                    channel,
                    //strItemBarcode,
                    //strReaderBarcode,
                    domLog,
                    strRecoverComment,
                    strLibraryCode,
                    strOutputItemRecPath,
                    ref readerdom,
                    ref itemdom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 写回读者、册记录

                // 写回读者记录
                lRet = channel.DoSaveTextRes(reader_recpath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    null,   // reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 写回册记录
                lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return 0;
            }

#if REMOVED
            // 容错恢复
            if (level == RecoverLevel.Robust)
            {
                string strRecoverComment = "";

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    strError = "<readerBarcode>元素值为空";
                    return -1;
                }

                // 读入读者记录

                nRet = this.GetReaderRecXml(
                    // Channels,
                    channel,
                    strReaderBarcode,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    // TODO: 记入信息文件

                    // 从日志记录中获得读者记录
                    strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<readerRecord>元素";
                        return -1;
                    }
                    string strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                    if (String.IsNullOrEmpty(strReaderRecPath) == true)
                    {
                        strError = "日志记录中<readerRecord>元素缺recPath属性";
                        return -1;
                    }

                    // TODO: 注意判断读者 XML 是否为 clipping 状态

                    // 新增一条读者记录
                    strOutputReaderRecPath = ResPath.GetDbName(strReaderRecPath) + "/?";
                    reader_timestamp = null;
                }
                else
                {
                    if (nRet == -1)
                    {
                        strError = "读入证条码号为 '" + strReaderBarcode + "' 的读者记录时发生错误: " + strError;
                        return -1;
                    }
                }

                string strLibraryCode = "";
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

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                // 读入册记录
                //string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                //    "confirmItemRecPath");
                string strConfirmItemRecPath = GetConfirmRecPath(domLog);

                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                if (String.IsNullOrEmpty(strItemBarcode) == true)
                {
                    strError = "<strItemBarcode>元素值为空";
                    return -1;
                }

                string strOutputItemRecPath = "";

                // 从册条码号获得册记录

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    // Channels,
                    channel,
                    strItemBarcode,
                    out string strItemXml,
                    100,
                    out List<string> aPath,
                    out byte[] item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strItemBarcode + "' 不存在";
                    // TODO: 记入信息文件

                    XmlNode node = null;
                    strItemXml = DomUtil.GetElementText(domLog.DocumentElement,
                        "itemRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<itemRecord>元素";
                        return -1;
                    }
                    string strItemRecPath = DomUtil.GetAttr(node, "recPath");
                    if (String.IsNullOrEmpty(strItemRecPath) == true)
                    {
                        strError = "日志记录中<itemRecord>元素缺recPath属性";
                        return -1;
                    }

                    // 新增一条册记录
                    strOutputItemRecPath = ResPath.GetDbName(strItemRecPath) + "/?";
                    item_timestamp = null;
                }
                else
                {
                    if (nRet == -1)
                    {
                        strError = "读入册条码号为 '" + strItemBarcode + "' 的册记录时发生错误: " + strError;
                        return -1;
                    }

                    Debug.Assert(aPath != null, "");

                    bool bNeedReload = false;

                    if (aPath.Count > 1)
                    {

                        // 建议根据strConfirmItemRecPath来进行挑选
                        if (String.IsNullOrEmpty(strConfirmItemRecPath) == true)
                        {
                            // 容错！
                            strOutputItemRecPath = aPath[0];

                            strRecoverComment += "册条码号 " + strItemBarcode + " 有 "
                                + aPath.Count.ToString() + " 条重复记录，因受容错要求所迫，权且采用其中第一个记录 "
                                + strOutputItemRecPath + " 来进行借阅操作。";

                            // 是否需要重新装载？
                            bNeedReload = false;    // 所取得的第一个路径，其记录已经装载
                        }
                        else
                        {

                            ///// 
                            nRet = aPath.IndexOf(strConfirmItemRecPath);
                            if (nRet != -1)
                            {
                                strOutputItemRecPath = aPath[nRet];
                                strRecoverComment += "册条码号 " + strItemBarcode + " 有 "
                                    + aPath.Count.ToString() + " 条重复记录，经过找到strConfirmItemRecPath=[" + strConfirmItemRecPath + "]"
                                    + "来进行借阅操作。";

                                // 是否需要重新装载？
                                if (nRet != 0)
                                    bNeedReload = true; // 第一个以外的路径才需要装载

                            }
                            else
                            {
                                // 容错
                                strOutputItemRecPath = aPath[0];

                                strRecoverComment += "册条码号 " + strItemBarcode + " 有 "
                                    + aPath.Count.ToString() + " 条重复记录，在其中无法找到strConfirmItemRecPath=[" + strConfirmItemRecPath + "]的记录"
                                    + "因受容错要求所迫，权且采用其中第一个记录 "
                                    + strOutputItemRecPath + " 来进行借阅操作。";

                                // 是否需要重新装载？
                                bNeedReload = false;    // 所取得的第一个路径，其记录已经装载

                                /* 
                                                                    strError = "册条码号 " + strItemBarcode + " 有 "
                                                                        + aPath.Count.ToString() + " 条重复记录，在其中无法找到strConfirmItemRecPath=[" + strConfirmItemRecPath + "]的记录";
                                                                    return -1;
                                 * */

                            }
                        }


                    } // if (aPath.Count > 1)
                    else
                    {

                        Debug.Assert(nRet == 1, "");
                        Debug.Assert(aPath.Count == 1, "");

                        if (nRet == 1)
                        {
                            strOutputItemRecPath = aPath[0];

                            // 是否需要重新装载？
                            bNeedReload = false;    // 所取得的第一个路径，其记录已经装载
                        }
                    }


                    // 重新装载
                    if (bNeedReload == true)
                    {
                        lRet = channel.GetRes(strOutputItemRecPath,
                            out strItemXml,
                            out string strMetaData,
                            out item_timestamp,
                            out strOutputItemRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "根据strOutputItemRecPath '" + strOutputItemRecPath + "' 重新获得册记录失败: " + strError;
                            return -1;
                        }

                        // 需要检查记录中的<barcode>元素值是否匹配册条码号

                    }
                }

                ////

                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out XmlDocument itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 修改读者记录
                // 修改册记录

                nRet = BorrowChangeReaderAndItemRecord(
                    channel,
                    //strItemBarcode,
                    //strReaderBarcode,
                    domLog,
                    strRecoverComment,
                    strLibraryCode,
                    ref readerdom,
                    ref itemdom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 写回读者、册记录

                // 写回读者记录
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 写回册记录
                lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
#endif

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverBorrow() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        #region RecoverBorrow()下级函数

        int TryWriteItemRecord(RmsChannel channel,
            string strItemRecPath,
            string strItemXml,
            string strItemType,
            bool robust,
            out string strError)
        {
            int nRedoCount = 0;
        REDO_WRITE:
            // 写册记录
            long lRet = channel.DoSaveTextRes(strItemRecPath,
strItemXml,
false,
"content,ignorechecktimestamp",
null,   // timestamp,
out _,
out string strOutputPath,
out strError);
            if (lRet == -1)
            {
                if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                    && robust == true)
                {
                    int nRet = TryCreateBiblioDatabase(
channel,
strItemRecPath,
ref nRedoCount,
out string error);
                    if (nRet == 0)
                        goto REDO_WRITE;
                    else if (nRet == -1)
                    {
                        strError = $"根据记录路径 '{strItemRecPath}' 临时决定创建书目库的过程出错: {error}";
                        return -1;
                    }
                }

                strError = $"写入{strItemType}记录 '{strItemRecPath}' 时发生错误: {strError}";
                return -1;
            }

            return 0;
        }

        // 尝试写入读者记录。如果读者库不存在，则会自动尝试重建读者库，然后再写入记录
        // return:
        //      -1  出错
        //      0   成功
        int TryWriteReaderRecord(RmsChannel channel,
            string strReaderRecPath,
            string strReaderXml,
            bool robust,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            int nRedoCount = 0;
        REDO_WRITE:
            // 写读者记录
            long lRet = channel.DoSaveTextRes(strReaderRecPath,
strReaderXml,
false,
"content,ignorechecktimestamp",
null,   // timestamp,
out byte[] output_timestamp,
out string strOutputPath,
out strError);
            if (lRet == -1)
            {
                if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                    && robust == true)
                {
                    int nRet = TryCreateReaderDatabase(
channel,
strReaderRecPath,
func_warning,
ref nRedoCount,
out string error);
                    if (nRet == 0)
                        goto REDO_WRITE;
                    else if (nRet == -1)
                    {
                        strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                        return -1;
                    }
                }

                strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                return -1;
            }

            return 0;
        }


        // 去除 XML 记录中包含 clipping 属性的元素
        static bool RemoveReservations(ref string xml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return false;
            }

            var nodes = dom.DocumentElement.SelectNodes("reservations");
            foreach (XmlElement e in nodes)
            {
                e.ParentNode.RemoveChild(e);
            }

            xml = dom.OuterXml;
            return true;
        }


        // 去除 XML 记录中包含 clipping 属性的元素
        static bool RemoveClipping(ref string xml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return false;
            }

            var nodes = dom.DocumentElement.SelectNodes("*[@clipping='true']");
            foreach (XmlElement e in nodes)
            {
                e.ParentNode.RemoveChild(e);
            }

            xml = dom.OuterXml;
            return true;
        }

        // 将 xml2 中不包含 clipping 属性的所有元素，覆盖到 xml1 中
        static bool MergeXml(ref string xml1, string xml2)
        {
            XmlDocument dom1 = new XmlDocument();
            try
            {
                dom1.LoadXml(xml1);
            }
            catch
            {
                return false;
            }

            XmlDocument dom2 = new XmlDocument();
            try
            {
                dom2.LoadXml(xml2);
            }
            catch
            {
                return false;
            }

            foreach (XmlElement e2 in dom2.DocumentElement.SelectNodes("*"))
            {
                if (e2.HasAttribute("clipping"))
                    continue;
                DomUtil.SetElementOuterXml(dom1.DocumentElement, e2.Name, e2.OuterXml);
            }

            xml1 = dom1.OuterXml;
            return true;
        }

        static string GetXmlRefID(string xml)
        {
            var dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return null;
            }
            return DomUtil.GetElementText(dom.DocumentElement,
                "refID");
        }

        static string GetXmlBarcode(string xml)
        {
            var dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return null;
            }
            return DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
        }

        static string GetXmlReaderKey(string xml)
        {
            var dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return null;
            }
            var barcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            var refID = DomUtil.GetElementText(dom.DocumentElement,
    "refID");
            var ret = TryGetUnionRefID(barcode,
    refID,
    out string strReaderKey);
            if (ret == false)
                return null;
            return strReaderKey;
        }

        // 根据 reader key 删除读者记录。
        // 注意，可以删除多条记录
        // parameters:
        //      strStyle    如果包含 notExistingAsError，表示遇到记录不存在的情况要报错
        int DeleteReaderRecord(RmsChannel channel,
            string strReaderKey,
            string strStyle,
            out string strError)
        {
            strError = "";

            int count = 0;
            while (true)
            {
                // 读入读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = this.GetReaderRecXml(
                    channel,
                    strReaderKey,
                    out string xml,
                    out string recpath,
                    out byte[] timestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    if (count == 0 && StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;
                    return count;
                }
                // 考虑了 timestamp mismatch 的重试问题
                long ret = TryDeleteRes(
                    channel,
                    recpath,
                    timestamp,
                    out strError);
                if (ret == -1)
                    return -1;
                count++;
                if (nRet == 1)  // 如果刚才检索得到的是一条，则这里删除后就可以返回了
                    return count;
            }
        }

        static long TryDeleteRes(RmsChannel channel,
    string recpath,
    byte[] timestamp,
    out string strError)
        {
            return TryDeleteRes(channel,
            recpath,
            timestamp,
            "",
            out strError);
        }

        // 删除一条记录。相比直接调用 DoDeleteRes()，本函数处理了时间戳不匹配重试的情况
        // parameters:
        //      strStyle    如果包含 notExistingAsError，表示如果记录不存在则要报错。否则是不报错
        // return:
        //      -1  出错
        //      0   成功
        static long TryDeleteRes(RmsChannel channel,
            string recpath,
            byte[] timestamp,
            string strStyle,
            out string strError)
        {
            strError = "";
            int nRedoCount = 0;
            byte[] output_timestamp = null;
        REDO_DELETE:
            long lRet = channel.DoDeleteRes(recpath,
                timestamp,
                "ignorechecktimestamp", //2024/5/16
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                if (channel.IsNotFound())
                {
                    if (StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;
                    return 0;   // 记录本来就不存在
                }

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount < 10)
                    {
                        timestamp = output_timestamp;
                        nRedoCount++;
                        goto REDO_DELETE;
                    }
                }
                strError = "删除记录 '" + recpath + "' 时发生错误: " + strError;
                return -1;
            }

            return 0;
        }

        // 尝试从数据库中获取读者记录。
        // 会按照指定的若干方式，顺次从数据库中尝试获取读者记录
        // parameters:
        //      queryies    检索式集合。每一个字符串代表一种检索方式。
        //                  可能是 @path:xxx 或 @refID:xxx 或者 xxx 形态
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   命中 1 条
        //      >1  命中多于 1 条
        int TryLoadReaderRecord(RmsChannel channel,
            List<string> queries,
            out string strReaderXml,
            out byte[] reader_timestamp,
            out string strOutputReaderRecPath,
            out string strError)
        {
            strError = "";
            strOutputReaderRecPath = "";
            reader_timestamp = null;
            strReaderXml = "";
            int nRet = 0;

            XmlDocument domLog = null;

            foreach (var query in queries)
            {
                if (query.StartsWith("@path:"))
                {
                    string strConfirmItemRecPath = query.Substring("@path:".Length);

                    long lRet = channel.GetRes(strConfirmItemRecPath,
                        out strReaderXml,
                        out _,  // strMetaData,
                        out reader_timestamp,
                        out strOutputReaderRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        strError = "根据记录路径 '" + strConfirmItemRecPath + "' 获得读者记录时出错: " + strError;
                        return -1;
                    }

                    return 1;
                }

                // 参考 ID 或者证条码号
                {
                    nRet = this.GetReaderRecXml(
    channel,
    query,
    out strReaderXml,
    out strOutputReaderRecPath,
    out reader_timestamp,
    out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        continue;
                    }

                    if (nRet == -1)
                    {
                        strError = "读入证条码号为 '" + query + "' 的读者记录时发生错误: " + strError;
                        return -1;
                    }

                    return nRet;
                }
            }

            return 0;
        }

        // 2024/3/16
        int LoadReaderRecord(RmsChannel channel,
            string strReaderKey,
            string strConfirmReaderRecPath,
            XmlDocument domLog,
            bool robust,
            delegate_selectItems func_selectItems,
            out string strReaderXml,
            out byte[] reader_timestamp,
            out string strOutputReaderRecPath,
            out string strError)
        {
            strError = "";
            strOutputReaderRecPath = "";
            reader_timestamp = null;
            strReaderXml = "";

            // 读入读者记录
            int nRet = this.GetReaderRecXml(
                // Channels,
                channel,
                strReaderKey,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                if (robust == false)
                    return 0;

                int nRedoCount = 0;
            REDO_GETITEM:
                // 尝试从日志记录中获取 readerRecord 元素
                // return:
                //      -2  nRedoCount 超过范围
                //      -1  出错
                //      0   日志记录中没有找到需要的元素
                //      1   成功。nRedoCount 自动增量 1
                nRet = RecoverReaderRecord(
channel,
domLog,
"readerRecord",
ref nRedoCount,
out bool clipping,
out string error);
                if (nRet != 1)
                {
                    strError = "证条码号 '" + strReaderKey + "' 不存在";
                    return -1;
                }
                if (clipping)
                {
                    strError = $"证条码号为 '{strReaderKey}' 的读者记录在日志记录中为 clipping 状态，并且数据库中没有找到对应的记录";
                    return -1;
                }
                goto REDO_GETITEM;
            }

            if (nRet == -1)
            {
                strError = "读入证条码号为 '" + strReaderKey + "' 的读者记录时发生错误: " + strError;
                return -1;
            }
            return 1;
        }

        delegate int delegate_selectItems(List<string> paths,
            ref string item_xml,
            ref byte[] item_timestamp,
            ref string item_recpath,
            out string error);

        int LoadItemRecord(RmsChannel channel,
            string strItemKey,
            string strConfirmItemRecPath,
            XmlDocument domLog,
            bool bForce,
            delegate_selectItems func_selectItems,
            out string strItemXml,
            out byte[] item_timestamp,
            out string strOutputItemRecPath,
            out string strError)
        {
            strError = "";

            strItemXml = "";
            strOutputItemRecPath = "";
            item_timestamp = null;

            // 如果已经有确定的册记录路径
            if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
            {
                int nRedoCount = 0;
            REDO_GETITEM:

                long lRet = channel.GetRes(strConfirmItemRecPath,
                    out strItemXml,
                    out _,  // strMetaData,
                    out item_timestamp,
                    out strOutputItemRecPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        // 根据日志记录中的特定元素，尝试恢复册记录
                        // return:
                        //      -2  nRedoCount 超过范围
                        //      -1  出错
                        //      0   日志记录中没有找到需要的元素
                        //      1   成功。nRedoCount 自动增量 1
                        int nRet = RecoverItemRecord(
    channel,
    domLog,
    "itemRecord",
    ref nRedoCount,
    out string error);
                        if (nRet != 1)
                        {
                            strError = $"根据strConfirmItemRecPath '{strConfirmItemRecPath}' 获得册记录失败: {strError}。并且尝试重建记录也失败: {error}";
                            return -1;
                        }
                        goto REDO_GETITEM;
                    }

                    strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                    return -1;
                }

                // 需要检查记录中的<barcode>元素值是否匹配册条码号


                // TODO: 如果记录路径所表达的记录不存在，或者其<barcode>元素值和要求的册条码号不匹配，那么都要改用逻辑方法，也就是利用册条码号来获得记录。
                // 当然，这种情况下，非常要紧的是确保数据库的素质很好，本身没有重条码号的情况出现。
            }
            else
            {
                // 从册条码号获得册记录

                int nRedoCount = 0;
            REDO_GETITEM:
                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = this.GetItemRecXml(
                    // Channels,
                    channel,
                    strItemKey,
                    out strItemXml,
                    100,
                    out List<string> aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    // 尝试用 itemRecord 中的 XML 内容恢复这条册记录
                    // 根据日志记录中的特定元素，尝试恢复册记录
                    // return:
                    //      -2  nRedoCount 超过范围
                    //      -1  出错
                    //      0   日志记录中没有找到需要的元素
                    //      1   成功。nRedoCount 自动增量 1
                    nRet = RecoverItemRecord(
channel,
domLog,
"itemRecord",
ref nRedoCount,
out string error);
                    if (nRet != 1)
                    {
                        strError = "册条码号 '" + strItemKey + "' 不存在";
                        return -1;
                    }
                    goto REDO_GETITEM;
                }
                if (nRet == -1)
                {
                    strError = "读入册条码号为 '" + strItemKey + "' 的册记录时发生错误: " + strError;
                    return -1;
                }

                if (aPath.Count > 1)
                {
#if REMOVED
                    if (bForce == true)
                    {
                        // 容错！
                        strOutputItemRecPath = aPath[0];

                        strRecoverComment += "册条码号 " + strItemKey + " 有 "
                            + aPath.Count.ToString() + " 条重复记录，因受容错要求所迫，权且采用其中第一个记录 "
                            + strOutputItemRecPath + " 来进行借阅操作。";
                    }
                    else
                    {
                        strError = "册条码号为 '" + strItemKey + "' 的册记录有 " + aPath.Count.ToString() + " 条，但此时confirmItemRecPath却为空";
                        return -1;
                    }
#endif
                    strOutputItemRecPath = aPath[0];
                    nRet = func_selectItems(aPath,
                        ref strItemXml,
                        ref item_timestamp,
                        ref strOutputItemRecPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");

                    if (nRet == 1)
                    {
                        strOutputItemRecPath = aPath[0];
                    }
                }
            }

            return 1;
        }

        // 获得借阅期限
        // parameters:
        //      strLibraryCode  读者记录所从属的恶读者库的馆代码
        // return:
        //      -1  出错
        //      0   没有获得参数
        //      1   获得了参数
        int GetBorrowPeriod(
            string strLibraryCode,
            XmlDocument readerdom,
            XmlDocument itemdom,
            int nNo,
            out string strPeriod,
            out string strError
            )
        {
            strPeriod = "";
            strError = "";
            int nRet = 0;

            // 从想要借阅的册信息中，找到图书类型
            string strBookType = DomUtil.GetElementText(itemdom.DocumentElement, "bookType");

            // 从读者信息中, 找到读者类型
            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");

            string strBorrowPeriodList = "";
            MatchResult matchresult;
            nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                strBookType,
                "借期",
                out strBorrowPeriodList,
                out matchresult,
                out strError);
            if (nRet == -1)
            {
                strError = "获得借期失败。获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数时发生错误: " + strError;
                return 0;
            }
            if (nRet < 4)  // nRet == 0
            {
                strError = "获得借期失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数无法获得: " + strError;
                return 0;
            }

            // 按照逗号分列值，需要根据序号取出某个参数

            string[] aPeriod = strBorrowPeriodList.Split(new char[] { ',' });

            if (aPeriod.Length == 0)
            {
                strError = "获得借期失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "'格式错误";
                return 0;
            }

            string strThisBorrowPeriod = "";
            string strLastBorrowPeriod = "";

            if (nNo > 0)
            {
                if (nNo >= aPeriod.Length)
                {
                    if (aPeriod.Length == 1)
                        strError = "续借失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数值 '" + strBorrowPeriodList + "' 规定，不能续借。(所定义的一个期限，是指第一次借阅的期限)";
                    else
                        strError = "续借失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数值 '" + strBorrowPeriodList + "' 规定，只能续借 " + Convert.ToString(aPeriod.Length - 1) + " 次。";
                    return -1;
                }
                strThisBorrowPeriod = aPeriod[nNo].Trim();

                strLastBorrowPeriod = aPeriod[nNo - 1].Trim();

                if (String.IsNullOrEmpty(strThisBorrowPeriod) == true)
                {
                    strError = "续借失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "' 格式错误：第 " + Convert.ToString(nNo) + "个部分为空。";
                    return -1;
                }
            }
            else
            {
                strThisBorrowPeriod = aPeriod[0].Trim();

                if (String.IsNullOrEmpty(strThisBorrowPeriod) == true)
                {
                    strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "' 格式错误：第一部分为空。";
                    return -1;
                }
            }

            // 检查strBorrowPeriod是否合法
            {
                nRet = LibraryApplication.ParsePeriodUnit(
                    strThisBorrowPeriod,
                    out long lPeriodValue,
                    out string strPeriodUnit,
                    out strError);
                if (nRet == -1)
                {
                    strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "' 格式错误：'" +
                         strThisBorrowPeriod + "' 格式错误: " + strError;
                    return -1;
                }
            }

            strPeriod = strThisBorrowPeriod;
            return 1;
        }

        // 去除读者记录侧的借阅信息链条
        // parameters:
        //      strReaderKey    读者检索键。可以是读者证条码号，也可以是 @refID:xxx 形态
        //      strItemBarcode  册条码号。注意，不允许 @refID:xxx 形态
        //      strItemRefID    册参考 ID
        // return:
        //      -1  出错
        //      0   没有必要修复
        //      1   修复成功
        int RemoveReaderSideLink(
            RmsChannel channel,
            string strReaderKey,
            string strItemBarcode,
            string strItemRefID,
            out string strRemovedInfo,
            out string strError)
        {
            strError = "";
            strRemovedInfo = "";

            string strReaderRefID = "";
            int nRet = this.ConvertReaderBarcodeListToRefIdList(
                channel,
                strReaderKey,
                out string strReaderRefIdString,
                out strError);
            if (nRet != -1)
                strReaderRefID = dp2StringUtil.GetRefIdValue(strReaderRefIdString);

            int nRedoCount = 0; // 因为时间戳冲突, 重试的次数

        REDO_REPAIR:

            // 读入读者记录
            string strReaderXml = "";
            string strOutputReaderRecPath = "";
            byte[] reader_timestamp = null;
            nRet = this.GetReaderRecXml(
                // Channels,
                channel,
                strReaderKey,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == 0)
            {
                strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                return 0;
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

            // 校验读者证条码号参数是否和XML记录中完全一致
            string strTempBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            string strTempRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                "refID");
            if (strReaderKey != strTempBarcode
                && strTempRefID != strReaderRefID)
            {
                strError = $"修复操作被拒绝。因读者证条码号参数 '{strReaderKey}' 和读者记录中<barcode>元素内的读者证条码号值 '{strTempBarcode}' (或参考 ID '{strTempRefID}')不一致。";
                return -1;
            }

            XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
            if (nodeBorrow == null)
                nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@refID='" + strItemRefID + "']");
            if (nodeBorrow == null)
            {
                strError = $"在读者记录 '{strReaderKey}' 中没有找到关于册条码号 '{strItemBarcode}' (或参考 ID '{strItemRefID}')的链";
                return 0;
            }

            strRemovedInfo = nodeBorrow.OuterXml;

            // 移除读者记录侧的链
            nodeBorrow.ParentNode.RemoveChild(nodeBorrow);

#if NO
            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            // 写回读者记录
            long lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                readerdom.OuterXml,
                false,
                "content",  // ,ignorechecktimestamp
                reader_timestamp,
                out byte[] output_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    nRedoCount++;
                    if (nRedoCount > 10)
                    {
                        strError = "写回读者记录 '" + strOutputReaderRecPath + "' 的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                        return -1;
                    }
                    goto REDO_REPAIR;
                }
                return -1;
            }

            return 1;
        }

        public delegate void delegate_writeLog(string text);

        // 借阅操作，修改读者和册记录
        // parameters:
        //      strItemBarcodeParam 册条码号。可以使用 @refID: 前缀
        //      strLibraryCode  读者记录所从属的读者库的馆代码
        int BorrowChangeReaderAndItemRecord(
            RmsChannel channel,
            //string strItemBarcodeParam,
            //string strReaderBarcode,
            XmlDocument domLog,
            string strRecoverComment,
            string strLibraryCode,
            string strItemRecPath,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
    "operator");
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
"location");
            strLocation = StringUtil.GetPureLocation(strLocation);


            // *** 修改读者记录


            string strNo = DomUtil.GetElementText(domLog.DocumentElement,
                "no");
            if (String.IsNullOrEmpty(strNo) == true)
            {
                strError = "日志记录中缺<no>元素";
                return -1;
            }

            int nNo = 0;

            try
            {
                nNo = Convert.ToInt32(strNo);
            }
            catch (Exception /*ex*/)
            {
                strError = "<no>元素值 '" + strNo + "' 应该为纯数字";
                return -1;
            }

            // 2024/2/13
            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "barcode");
            string strItemRefID = DomUtil.GetElementText(itemdom.DocumentElement,
                "refID");
            string strItemKey = dp2StringUtil.BuildReaderKey(strItemBarcode, strItemRefID);

            // 注意，读者证条码号可能为空
            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            string strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                "refID");
            string strReaderKey = dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID);

            XmlElement nodeBorrow = null;

            // 既然日志记录中记载的是 @refID: 的形态，那读者记录中 borrows 里面势必记载的也是这个形态
            nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='@refID:" + strItemRefID + "']") as XmlElement;
            if (nodeBorrow == null)
            {
                // 2024/2/13
                // 再尝试用册条码号匹配一次
                nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']") as XmlElement;
            }

            // 2008/2/1 changed 为了提高容错能力，续借操作时不去追究以前是否借阅过

            if (nodeBorrow != null)
            {
                // 2008/1/30 changed ，容错
                // strError = "该读者已经借阅了册 '" + strItemBarcode + "'，不能重复借。";
                // return -1;
                // 
            }
            else
            {
                // 检查<borrows>元素是否存在
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrows");
                if (root == null)
                {
                    root = readerdom.CreateElement("borrows");
                    root = readerdom.DocumentElement.AppendChild(root);
                }

                // 加入借阅册信息
                nodeBorrow = readerdom.CreateElement("borrow");
                nodeBorrow = root.AppendChild(nodeBorrow) as XmlElement;
            }

            // 
            // barcode
            if (string.IsNullOrEmpty(strItemBarcode))
                DomUtil.SetAttr(nodeBorrow, "barcode", $"@refID:{strItemRefID}");
            else
                DomUtil.SetAttr(nodeBorrow, "barcode", strItemBarcode);
            // 2024/2/13
            if (string.IsNullOrEmpty(strItemRefID) == false)
                nodeBorrow.SetAttribute("refID", strItemRefID);

            // 2024/3/18
            // 记载册记录路径 和 书目记录路径
            if (String.IsNullOrEmpty(strItemRecPath) == false)
            {
                nodeBorrow.SetAttribute("recPath", strItemRecPath); // 2006/12/24
                string strParentID = DomUtil.GetElementText(itemdom.DocumentElement, "parent");

                // 通过册记录路径和parentid得知从属的种记录路径
                // parameters:
                // return:
                //      -1  error
                //      1   找到
                nRet = GetBiblioRecPathByItemRecPath(
                    strItemRecPath,
                    strParentID,
                    out string strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    //strError = "根据册记录路径 '" + strItemRecPath + "' 和 parent_id '" + strParentID + "' 获得书目库路径时出错: " + strError;
                    //return -1;
                }
                else
                {
                    nodeBorrow.SetAttribute("biblioRecPath", strBiblioRecPath);
                }
            }

            nodeBorrow.SetAttribute("location", strLocation);

            string strRenewComment = "";

            string strBorrowDate = DomUtil.GetElementText(domLog.DocumentElement,
                "borrowDate");

            if (nNo >= 1)
            {
                // 保存前一次借阅的信息
                strRenewComment = DomUtil.GetAttr(nodeBorrow, "renewComment");

                if (strRenewComment != "")
                    strRenewComment += "; ";

                strRenewComment += "no=" + Convert.ToString(nNo - 1) + ", ";
                strRenewComment += "borrowDate=" + DomUtil.GetAttr(nodeBorrow, "borrowDate") + ", ";
                strRenewComment += "borrowPeriod=" + DomUtil.GetAttr(nodeBorrow, "borrowPeriod") + ", ";
                strRenewComment += "returnDate=" + strBorrowDate + ", ";
                strRenewComment += "operator=" + DomUtil.GetAttr(nodeBorrow, "operator");
            }

            // borrowDate
            DomUtil.SetAttr(nodeBorrow, "borrowDate",
                strBorrowDate);

            // no
            DomUtil.SetAttr(nodeBorrow, "no", Convert.ToString(nNo));

            // borrowPeriod
            string strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
                "borrowPeriod");

            if (String.IsNullOrEmpty(strBorrowPeriod) == true)
            {
                // 获得借阅期限
                // return:
                //      -1  出错
                //      0   没有获得参数
                //      1   获得了参数
                nRet = GetBorrowPeriod(
                    strLibraryCode,
                    readerdom,
                    itemdom,
                    nNo,
                    out strBorrowPeriod,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
    "defaultBorrowPeriod");
                    if (String.IsNullOrEmpty(strBorrowPeriod) == true)
                        strBorrowPeriod = "60day";
                }
            }

            DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);

            // 2024/3/18
            // borrowID
            LibraryServerUtil.SetAttribute(domLog,
                "borrowID",
                nodeBorrow);

            // returningDate
            LibraryServerUtil.SetAttribute(domLog,
                "returningDate",
                nodeBorrow);

            // renewComment
            {
                if (string.IsNullOrEmpty(strRenewComment) == false)
                    DomUtil.SetAttr(nodeBorrow, "renewComment", strRenewComment);
            }

            // operator
#if NO
            DomUtil.SetAttr(nodeBorrow, "operator", strOperator);
#endif
            LibraryServerUtil.SetAttribute(domLog,
    "operator",
    nodeBorrow);

            // recoverComment
            if (String.IsNullOrEmpty(strRecoverComment) == false)
                DomUtil.SetAttr(nodeBorrow, "recoverComment", strItemKey);

            // type
            LibraryServerUtil.SetAttribute(domLog,
                "type",
                nodeBorrow);

            // price
            LibraryServerUtil.SetAttribute(domLog,
                "price",
                nodeBorrow);

            // *** 检查册记录以前是否存在在借的痕迹，如果存在的话，(如果指向当前读者倒是无妨了反正后面即将要覆盖) 需要事先消除相关的另一个读者记录的痕迹，也就是说相当于把相关的册给进行还书操作

            string strBorrower0 = DomUtil.GetElementInnerText(itemdom.DocumentElement,
                "borrower");
            if (string.IsNullOrEmpty(strBorrower0) == false
                && strBorrower0 != strReaderBarcode
                && strBorrower0 != $"@refID:{strReaderRefID}")
            {
                // 去除读者记录侧的借阅信息链条
                // return:
                //      -1  出错
                //      0   没有必要修复
                //      1   修复成功
                nRet = RemoveReaderSideLink(
                    channel,
                    strBorrower0,
                    strItemBarcode,
                    strItemRefID,
                    out string strRemovedInfo,
                    out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("册条码号为 '" + strItemKey + "' 的册记录，在进行借书操作(拟被读者 '" + strReaderKey + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件尝试自动修正(删除)此读者记录的半侧借阅信息链。不过，在去除读者记录册借阅链时发生错误: " + strError);
                    // writeLog?.Invoke($"册条码号为 '{strItemBarcodeParam}' 的册记录，在进行借书操作(拟被读者 '{strReaderBarcode}' 借阅)以前，发现它被另一读者 '{strBorrower0}' 持有，软件尝试自动修正(删除)此读者记录的半侧借阅信息链。不过，在去除读者记录册借阅链时发生错误: {strError}");
                }
                else
                {
                    this.WriteErrorLog("册条码号为 '" + strItemKey + "' 的册记录，在进行借书操作(拟被读者 '" + strReaderKey + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件已经自动修正(删除)了此读者记录的半侧借阅信息链。被移走的片断 XML 信息为 '" + strRemovedInfo + "'");
                    // writeLog?.Invoke($"册条码号为 '{strItemBarcodeParam}' 的册记录，在进行借书操作(拟被读者 '{strReaderBarcode}' 借阅)以前，发现它被另一读者 '{strBorrower0}' 持有，软件已经自动修正(删除)了此读者记录的半侧借阅信息链。被移走的片断 XML 信息为 '{strRemovedInfo}'");
                }
            }

            // *** 修改册记录
            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrower", GetUnionRefID(strReaderBarcode, strReaderRefID));

            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrowDate",
                strBorrowDate);

            DomUtil.SetElementText(itemdom.DocumentElement,
                "no",
                Convert.ToString(nNo));

            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrowPeriod",
                strBorrowPeriod);

            DomUtil.SetElementText(itemdom.DocumentElement,
                "renewComment",
                strRenewComment);

            DomUtil.SetElementText(itemdom.DocumentElement,
    "operator",
    strOperator);

            // recoverComment
            if (String.IsNullOrEmpty(strRecoverComment) == false)
            {
                DomUtil.SetElementText(itemdom.DocumentElement,
        "recoverComment",
        strRecoverComment);
            }

            return 0;
        }

        #endregion

        // Return() API 恢复动作
        /* 日志记录格式
<root>
  <operation>return</operation> 操作类型
  <action>return</action> 动作。有 return/lost/inventory/read/boxing 几种。恢复动作目前仅恢复 return 和 lost 两种，其余会忽略
  <itemBarcode>0000001</itemBarcode> 册条码号
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
  <overdues>...</overdues> 超期信息 通常内容为一个字符串，为一个<overdue>元素XML文本片断
  
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
  
         * 2016/7/22 新增加
         * borrowDate
         * borrowPeriod
         * denyPeriod
         * returningDate
         * borrowOperator
</root>
         * * */
        // parameters:
        //      bForce  是否为容错状态。在容错状态下，如果遇到重复的册条码号，就算做第一条。
        public int RecoverReturn(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            bool bForce,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 重试时改用的策略
            RecoverLevel retry_level = level;

            string reader_xml = ""; // 所取得的读者记录 XML
            string reader_recpath = ""; // 用于指示保存回到读者记录时的记录路径

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
    "action");

                if (strAction != "return" && strAction != "lost")
                {
                    // 忽略其余动作
                    return 0;
                }

                string strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<readerRecord>元素";
                    return -1;
                }

                string strReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 2017/1/12
                bool bClipping = DomUtil.GetBooleanParam(node, "clipping", false);
                if (bClipping == true)
                {
                    // ***
                    // 尝试从数据库中获得这一条读者记录(并把它与 clipping 状态的这一条合并)。
                    // 如果数据库中找不到，则只好直接用这条读者记录(去掉全部 clipping 属性)
                    // 注: 因为 clipping 现象并不算一种“错误”，所以也谈不上“容错”，处理这种情况就被看作 Snapshot 模式的基本能力
                    var queries = new List<string>();
                    queries.Add($"@path:{strReaderRecPath}");
                    var refID = GetXmlRefID(strReaderXml);
                    if (string.IsNullOrEmpty(refID) == false)
                        queries.Add($"@refID:{refID}");
                    nRet = TryLoadReaderRecord(channel,
                        queries,
                        out reader_xml,
                        out _,
                        out reader_recpath,
                        out strError);
                    // nRet = 0;   // testing
                    if (nRet == 0)
                    {
                        // 从数据库中没有找到读者记录，只好勉强直接用 strReaderXml
                        RemoveClipping(ref strReaderXml);
                        reader_xml = strReaderXml;
                        reader_recpath = strReaderRecPath;
                        retry_level = RecoverLevel.LogicRobust;
                        goto DO_LOGIC;
                    }
                    if (nRet == 1)
                    {
                        // 从数据库中读出记录，然后用 readerRecord 中的可信局部去替代，形成结果记录内容
                        MergeXml(ref reader_xml, strReaderXml);
                        retry_level = RecoverLevel.LogicRobust;
                        goto DO_LOGIC;
                    }
                    strError = "日志记录中 <readerRecord> 元素为 clipping 状态(并且尝试从数据库中获取记录失败)，无法进行快照恢复";
                    return -1;
                }

                string strItemXml = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemRecord",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<itemRecord>元素";
                    return -1;
                }
                string strItemRecPath = DomUtil.GetAttr(node, "recPath");

                byte[] timestamp = null;

                // 2024/3/27
                RemoveReservations(ref strReaderXml);

                // 写读者记录
                nRet = TryWriteReaderRecord(
                    channel,
                    strReaderRecPath,
                    strReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写读者记录
                    lRet = channel.DoSaveTextRes(strReaderRecPath,
                        strReaderXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out byte[] output_timestamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                // 写册记录
                nRet = TryWriteItemRecord(channel,
                    strItemRecPath,
                    strItemXml,
    "entity",
    robust,
    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写册记录
                    lRet = channel.DoSaveTextRes(strItemRecPath,
                    strItemXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out _, // output_timestamp,
                    out string strOutputPath,
                    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateBiblioDatabase(
    channel,
    strItemRecPath,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strItemRecPath}' 临时决定创建书目库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入册记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                return 0;
            }

        DO_LOGIC:
            // 逻辑恢复或者混合恢复
            if (IsLogic(level, out robust)
                || IsLogic(retry_level))
            {
                string strRecoverComment = "";

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                if (strAction != "return" && strAction != "lost")
                {
                    // 忽略其余动作
                    return 0;
                }

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                "readerBarcode");
                // 2024/1/29
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
    "readerRefID");
                var ret = TryGetUnionRefID(strReaderBarcode,
                    strReaderRefID,
                    out string strReaderKey);
                if (ret == false)
                {
                    strError = "readerBarcode 元素值为空，并且 readerRefID 元素值也为空";
                    goto ERROR1;
                }

                // 读入册记录
                //string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                //    "confirmItemRecPath");
                string strConfirmItemRecPath = GetConfirmRecPath(domLog);

                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                string strItemRefID = DomUtil.GetElementText(domLog.DocumentElement,
    "itemRefID");
                var ret1 = TryGetUnionRefID(strItemBarcode,
                    strItemRefID,
                    out string strItemKey);
                if (ret1 == false)
                {
                    strError = "itemBarcode 元素值为空，并且 itemRefID 元素值也为空";
                    goto ERROR1;
                }

#if REMOVED
                string strItemXml = "";
                string strOutputItemRecPath = "";
                byte[] item_timestamp = null;

                // 如果已经有确定的册记录路径
                if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                {
                    lRet = channel.GetRes(strConfirmItemRecPath,
                        out strItemXml,
                        out string strMetaData,
                        out item_timestamp,
                        out strOutputItemRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                        goto ERROR1;
                    }

                    // 需要检查记录中的<barcode>元素值是否匹配册条码号
                }
                else
                {
                    // 从册条码号获得册记录

                    int nRedoCount = 0;
                REDO_GETITEM:
                    // 获得册记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetItemRecXml(
                        // Channels,
                        channel,
                        strItemKey,
                        out strItemXml,
                        100,
                        out List<string> aPath,
                        out item_timestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        // 尝试用 itemRecord 中的 XML 内容恢复这条册记录
                        // 根据日志记录中的特定元素，尝试恢复册记录
                        // return:
                        //      -2  nRedoCount 超过范围
                        //      -1  出错
                        //      0   日志记录中没有找到需要的元素
                        //      1   成功。nRedoCount 自动增量 1
                        nRet = RecoverItemRecord(
    channel,
    domLog,
    "itemRecord",
    ref nRedoCount,
    out string error);
                        if (nRet != 1)
                        {
                            strError = "册条码号 '" + strItemKey + "' 不存在";
                            goto ERROR1;
                        }
                        goto REDO_GETITEM;
                    }
                    if (nRet == -1)
                    {
                        strError = "读入册条码号为 '" + strItemKey + "' 的册记录时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (aPath.Count > 1)
                    {
                        if (string.IsNullOrEmpty(strReaderBarcode) == true)
                        {
                            // 发生重条码号的时候，又没有读者证条码号辅助判断
                            if (bForce == false)
                            {
                                strError = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，但此时日志记录中没有提供读者证条码号辅助判断，无法进行还书操作。";
                                goto ERROR1;
                            }
                            // TODO: 那就至少看看这些册中，哪些表明被人借阅着？如果正巧只有一个人借过，那就...。
                            strRecoverComment += "册条码号 " + strItemBarcode + "有 " + aPath.Count.ToString() + " 条重复记录，而且没有读者证条码号进行辅助选择。";
                        }

                        /*
                        strError = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，但此时confirmItemRecPath却为空";
                        goto ERROR1;
                         * */
                        // bItemBarcodeDup = true; // 此时已经需要设置状态。虽然后面可以进一步识别出真正的册记录

                        /*
                        // 构造strDupBarcodeList
                        string[] pathlist = new string[aPath.Count];
                        aPath.CopyTo(pathlist);
                        strDupBarcodeList = String.Join(",", pathlist);
                         * */


                        // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
                        // return:
                        //      -1  出错
                        //      其他    选出的数量
                        nRet = FindItem(
                            channel,
                            strReaderBarcode,
                            strReaderRefID,
                            aPath,
                            true,   // 优化
                            out List<string> aFoundPath,
                            out List<string> aItemXml,
                            out List<byte[]> aTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "选择重复条码号的册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (nRet == 0)
                        {
                            strError = "册条码号 '" + strItemBarcode + "' 检索出的 " + aPath.Count + " 条记录中，没有任何一条其<borrower>元素表明了被读者 '" + strReaderBarcode + "' 借阅。";
                            goto ERROR1;
                        }

                        if (nRet > 1)
                        {
                            if (bForce == true)
                            {
                                // 容错情况下，选择第一个册条码号
                                strOutputItemRecPath = aFoundPath[0];
                                item_timestamp = aTimestamp[0];
                                strItemXml = aItemXml[0];

                                // TODO: 不过，应当在记录中记载注释，表示这是容错处理方式
                                if (string.IsNullOrEmpty(strReaderBarcode) == true)
                                {
                                    strRecoverComment += "经过筛选，仍然有 " + aFoundPath.Count.ToString() + " 条册记录含有借阅者信息(无论什么读者证条码号)，那么就只好选择其中第一个册记录 " + strOutputItemRecPath + " 进行还书操作。";
                                }
                                else
                                {
                                    strRecoverComment += "经过筛选，仍然有 " + aFoundPath.Count.ToString() + " 条册记录含有借阅者 '" + strReaderBarcode + "' 信息，那么就只好选择其中第一个册记录 " + strOutputItemRecPath + " 进行还书操作。";
                                }
                            }
                            else
                            {
                                strError = "册条码号为 '" + strItemBarcode + "' 并且<borrower>元素表明为读者 '" + strReaderBarcode + "' 借阅的册记录有 " + aFoundPath.Count.ToString() + " 条，无法进行还书操作。";
                                /*
                                aDupPath = new string[aFoundPath.Count];
                                aFoundPath.CopyTo(aDupPath);
                                 * */
                                goto ERROR1;
                            }
                        }

                        Debug.Assert(nRet == 1, "");

                        strOutputItemRecPath = aFoundPath[0];
                        item_timestamp = aTimestamp[0];
                        strItemXml = aItemXml[0];
                    }
                    else
                    {

                        Debug.Assert(nRet == 1, "");
                        Debug.Assert(aPath.Count == 1, "");

                        if (nRet == 1)
                        {
                            strOutputItemRecPath = aPath[0];
                        }
                    }

                }
#endif
                nRet = LoadItemRecord(channel,
strItemKey,
strConfirmItemRecPath,
domLog,
bForce,
    (List<string> paths,
    ref string item_xml,
    ref byte[] timestamp,
    ref string item_recpath,
    out string error) =>
    {
        error = "";
        if (string.IsNullOrEmpty(strReaderBarcode) == true)
        {
            // 发生重条码号的时候，又没有读者证条码号辅助判断
            if (bForce == false)
            {
                error = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + paths.Count.ToString() + " 条，但此时日志记录中没有提供读者证条码号辅助判断，无法进行还书操作。";
                return -1;
            }
            // TODO: 那就至少看看这些册中，哪些表明被人借阅着？如果正巧只有一个人借过，那就...。
            strRecoverComment += "册条码号 " + strItemBarcode + "有 " + paths.Count.ToString() + " 条重复记录，而且没有读者证条码号进行辅助选择。";
        }

        // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
        // return:
        //      -1  出错
        //      其他    选出的数量
        nRet = FindItem(
            channel,
            strReaderBarcode,
            strReaderRefID,
            paths,
            true,   // 优化
            out List<string> aFoundPath,
            out List<string> aItemXml,
            out List<byte[]> aTimestamp,
            out error);
        if (nRet == -1)
        {
            error = "选择重复条码号的册记录时发生错误: " + error;
            return -1;
        }

        if (nRet == 0)
        {
            error = "册条码号 '" + strItemBarcode + "' 检索出的 " + paths.Count + " 条记录中，没有任何一条其<borrower>元素表明了被读者 '" + strReaderBarcode + "' 借阅。";
            return -1;
        }

        if (nRet > 1)
        {
            if (bForce == true)
            {
                // 容错情况下，选择第一个册条码号
                item_recpath = aFoundPath[0];
                timestamp = aTimestamp[0];
                item_xml = aItemXml[0];

                // TODO: 不过，应当在记录中记载注释，表示这是容错处理方式
                if (string.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    strRecoverComment += "经过筛选，仍然有 " + aFoundPath.Count.ToString() + " 条册记录含有借阅者信息(无论什么读者证条码号)，那么就只好选择其中第一个册记录 " + item_recpath + " 进行还书操作。";
                }
                else
                {
                    strRecoverComment += "经过筛选，仍然有 " + aFoundPath.Count.ToString() + " 条册记录含有借阅者 '" + strReaderBarcode + "' 信息，那么就只好选择其中第一个册记录 " + item_recpath + " 进行还书操作。";
                }
            }
            else
            {
                error = "册条码号为 '" + strItemBarcode + "' 并且<borrower>元素表明为读者 '" + strReaderBarcode + "' 借阅的册记录有 " + aFoundPath.Count.ToString() + " 条，无法进行还书操作。";
                return -1;
            }
        }

        Debug.Assert(nRet == 1, "");

        item_recpath = aFoundPath[0];
        timestamp = aTimestamp[0];
        item_xml = aItemXml[0];
        return 0;
    },
out string strItemXml,
out byte[] item_timestamp,
out string strOutputItemRecPath,
out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out XmlDocument itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                ///
                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    if (bForce == true)
                    {
                        // 容错的情况下，从册记录中获得借者证条码号
                        strReaderBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                            "borrower");
                        if (String.IsNullOrEmpty(strReaderBarcode) == true)
                        {
                            strError = "在不知道读者证条码号的情况下，册记录中的<borrower>元素值为空。无法进行还书操作。";
                            goto ERROR1;
                        }

                    }
                    else
                    {
                        strError = "日志记录中<readerBarcode>元素值为空";
                        goto ERROR1;
                    }

                    if (string.IsNullOrEmpty(strReaderKey))
                        strReaderKey = strReaderBarcode;
                }

#if REMOVED
                /*
                // 读入读者记录

                nRet = this.GetReaderRecXml(
                    // Channels,
                    channel,
                    strReaderKey,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                */
                nRet = LoadReaderRecord(
                    channel,
                    strReaderKey,
                    null,
                    domLog,
                    robust,
                    null,
                    out string strReaderXml,
                    out byte[] reader_timestamp,
                    out string strOutputReaderRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入证条码号为 '" + strReaderKey + "' 的读者记录时发生错误: " + strError;
                    goto ERROR1;
                }
#endif
                if (string.IsNullOrEmpty(reader_xml))
                {
                    nRet = LoadReaderRecord(
    channel,
    strReaderKey,
    null,
    domLog,
    robust,
    null,
    out reader_xml,
    out byte[] reader_timestamp,
    out reader_recpath,
    out strError);
                    if (nRet == 0)
                    {
                        strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                        goto ERROR1;
                    }
                    if (nRet == -1)
                    {
                        strError = "读入证条码号为 '" + strReaderKey + "' 的读者记录时发生错误: " + strError;
                        goto ERROR1;
                    }
                }

                nRet = LibraryApplication.LoadToDom(reader_xml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 修改读者记录
                // 修改册记录
                nRet = ReturnChangeReaderAndItemRecord(
                    channel,
                    strAction,
                    //strItemBarcode,
                    //strReaderBarcode,
                    domLog,
                    strRecoverComment,
                    ref readerdom,
                    ref itemdom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 写回读者记录
                lRet = channel.DoSaveTextRes(reader_recpath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    null,   // reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 写回册记录
                lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }

#if REMOVED
            // 容错恢复
            if (level == RecoverLevel.Robust)
            {
                string strRecoverComment = "";

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                if (strAction != "return" && strAction != "lost")
                {
                    // 忽略其余动作
                    return 0;
                }

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                // 2024/1/29
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRefID");
                var ret = TryGetUnionRefID(strReaderBarcode,
    strReaderRefID,
    out string strReaderKey);
                if (ret == false)
                {
                    strError = "readerBarcode 元素值为空，并且 readerRefID 元素值也为空";
                    goto ERROR1;
                }

                // 读入册记录
                //string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                //    "confirmItemRecPath");
                string strConfirmItemRecPath = GetConfirmRecPath(domLog);

                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                string strItemRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemRefID");
                ret = TryGetUnionRefID(strItemBarcode,
                    strItemRefID,
                    out string strItemKey);
                if (ret == false)
                {
                    strError = "itemBarcode 元素值为空，并且 itemRefID 元素值也为空";
                    goto ERROR1;
                }

                string strOutputItemRecPath = "";

                // 从册条码号获得册记录

                bool bDupItemBarcode = false;   // 册条码号是否发生了重复

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    channel,
                    strItemKey,
                    out string strItemXml,
                    100,
                    out List<string> aPath,
                    out byte[] item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strItemKey + "' 不存在";
                    // TODO: 记入信息文件

                    strItemXml = DomUtil.GetElementText(domLog.DocumentElement,
                        "itemRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<itemRecord>元素";
                        return -1;
                    }
                    string strItemRecPath = DomUtil.GetAttr(node, "recPath");
                    if (String.IsNullOrEmpty(strItemRecPath) == true)
                    {
                        strError = "日志记录中<itemRecord>元素缺recPath属性";
                        return -1;
                    }

                    // 新增一条册记录
                    strOutputItemRecPath = ResPath.GetDbName(strItemRecPath) + "/?";
                    item_timestamp = null;
                }
                else
                {
                    if (nRet == -1)
                    {
                        strError = "读入册条码号为 '" + strItemKey + "' 的册记录时发生错误: " + strError;
                        return -1;
                    }

                    if (aPath.Count > 1)
                    {
                        bDupItemBarcode = true;

                        if (string.IsNullOrEmpty(strReaderBarcode) == true)
                        {
                            // 发生重条码号的时候，又没有读者证条码号辅助判断
                            if (bForce == false)
                            {
                                strError = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，但此时日志记录中没有提供读者证条码号辅助判断，无法进行还书操作。";
                                return -1;
                            }
                            // TODO: 那就至少看看这些册中，哪些表明被人借阅着？如果正巧只有一个人借过，那就...。
                            strRecoverComment += "册条码号 " + strItemBarcode + " 有 " + aPath.Count.ToString() + " 条重复记录，而且没有读者证条码号进行辅助选择。";
                        }


                        // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
                        // return:
                        //      -1  出错
                        //      其他    选出的数量
                        nRet = FindItem(
                            channel,
                            strReaderBarcode,
                            strReaderRefID,
                            aPath,
                            true,   // 优化
                            out List<string> aFoundPath,
                            out List<string> aItemXml,
                            out List<byte[]> aTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "选择重复条码号的册记录时发生错误: " + strError;
                            return -1;
                        }

                        if (nRet == 0)
                        {
                            if (bDupItemBarcode == false)
                            {
                                // 没有重复册条码号的情况下才作
                                // 需要把根据“所借册条码号”清除读者记录中借阅信息的动作提前进行? 这样遇到特殊情况范围时，至少读者记录中的信息是被清除了的，这是容错的需要
                                string strError_1 = "";
                                nRet = ReturnAllReader(
                                        // Channels,
                                        channel,
                                        strItemBarcode,
                                        "",
                                        out strError_1);
                                if (nRet == -1)
                                {
                                    // 故意不报，继续处理
                                }
                            }

                            strError = "册条码号 '" + strItemBarcode + "' 检索出的 " + aPath.Count + " 条记录中，没有任何一条其<borrower>元素表明了被读者 '" + strReaderBarcode + "' 借阅。";
                            return -1;
                        }

                        if (nRet > 1)
                        {
                            if (bForce == true)
                            {
                                // 容错情况下，选择第一个册条码号
                                strOutputItemRecPath = aFoundPath[0];
                                item_timestamp = aTimestamp[0];
                                strItemXml = aItemXml[0];

                                // TODO: 不过，应当在记录中记载注释，表示这是容错处理方式
                                if (string.IsNullOrEmpty(strReaderBarcode) == true)
                                {
                                    strRecoverComment += "经过筛选，仍然有 " + aFoundPath.Count.ToString() + " 条册记录含有借阅者信息(无论什么读者证条码号)，那么就只好选择其中第一个册记录 " + strOutputItemRecPath + " 进行还书操作。";
                                }
                                else
                                {
                                    strRecoverComment += "经过筛选，仍然有 " + aFoundPath.Count.ToString() + " 条册记录含有借阅者 '" + strReaderBarcode + "' 信息，那么就只好选择其中第一个册记录 " + strOutputItemRecPath + " 进行还书操作。";
                                }
                            }
                            else
                            {
                                strError = "册条码号为 '" + strItemBarcode + "' 并且<borrower>元素表明为读者 '" + strReaderBarcode + "' 借阅的册记录有 " + aFoundPath.Count.ToString() + " 条，无法进行还书操作。";
                                return -1;
                            }
                        }

                        Debug.Assert(nRet == 1, "");

                        strOutputItemRecPath = aFoundPath[0];
                        item_timestamp = aTimestamp[0];
                        strItemXml = aItemXml[0];
                    }
                    else
                    {
                        Debug.Assert(nRet == 1, "");
                        Debug.Assert(aPath.Count == 1, "");

                        if (nRet == 1)
                        {
                            strOutputItemRecPath = aPath[0];
                        }
                    }
                }

                ////
                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out XmlDocument itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                ///
                if (String.IsNullOrEmpty(strReaderKey) == true)
                {
                    if (bForce == true)
                    {
                        // 容错的情况下，从册记录中获得借者证条码号
                        strReaderKey = DomUtil.GetElementText(itemdom.DocumentElement,
                            "borrower");
                        if (String.IsNullOrEmpty(strReaderKey) == true)
                        {
                            strError = "在不知道读者证条码号的情况下，册记录中的<borrower>元素值为空。无法进行还书操作。";
                            return -1;
                        }
                    }
                    else
                    {
                        strError = "日志记录中<readerBarcode>元素值为空";
                        return -1;
                    }
                }

                // 读入读者记录
                nRet = this.GetReaderRecXml(
                    channel,
                    strReaderKey,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                    // TODO: 记入信息文件

                    // 从日志记录中获得读者记录
                    strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<readerRecord>元素";
                        return -1;
                    }
                    string strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                    if (String.IsNullOrEmpty(strReaderRecPath) == true)
                    {
                        strError = "日志记录中<readerRecord>元素缺recPath属性";
                        return -1;
                    }

                    // 新增一条读者记录
                    strOutputReaderRecPath = ResPath.GetDbName(strReaderRecPath) + "/?";
                    reader_timestamp = null;
                }
                else
                {
                    if (nRet == -1)
                    {
                        strError = "读入证条码号为 '" + strReaderKey + "' 的读者记录时发生错误: " + strError;
                        return -1;
                    }
                }

                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                // 修改读者记录
                // 修改册记录
                nRet = ReturnChangeReaderAndItemRecord(
                    channel,
                    strAction,
                    //strItemBarcode,
                    //strReaderBarcode,
                    domLog,
                    strRecoverComment,
                    ref readerdom,
                    ref itemdom,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 在容错(并且没有重复册条码号的情况下)的情况下，需要利用读者库的“所借册条码号”检索途径，把除了当前关注的读者记录以外的潜在相关读者记录调出，
                // 把它们中的相关<borrows/borrow>抹除，以免造成多头的借阅信息。
                if (bDupItemBarcode == false)
                {
                    nRet = ReturnAllReader(
                            // Channels,
                            channel,
                            strItemBarcode,
                            strOutputReaderRecPath,
                            out strError);
                    if (nRet == -1)
                    {
                        // 故意不报，继续处理
                    }
                }

                // 写回读者、册记录

                // 写回读者记录
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                // 写回册记录
                lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
#endif
            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverReturn() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        #region RecoverReturn()下级函数

        static string GetConfirmRecPath(XmlDocument domOperLog)
        {
            string recpath = "";
            var node = domOperLog.DocumentElement
                .SelectSingleNode($"itemRecord/@recPath");
            if (node != null)
                recpath = node.Value;
            if (string.IsNullOrEmpty(recpath))
                recpath = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "confirmItemRecPath");
            return recpath;
        }

        // 根据日志记录中的特定元素，尝试恢复册记录
        // 注: 覆盖前是否需要检查原有位置已经存在一条记录，以及存在后如何处理，需要研究一下。
        // 作追加处理不一定合适。因为不知道当前日志动作后面的其它日志动作是不是又占用了追加使用的记录位置
        // return:
        //      -2  nRedoCount 超过范围
        //      -1  出错
        //      0   日志记录中没有找到需要的元素
        //      1   成功。nRedoCount 自动增量 1
        int RecoverItemRecord(
        RmsChannel channel,
        XmlDocument domOperLog,
        string elementName,
        ref int nRedoCount,
        out string strError)
        {
            strError = "";

            if (nRedoCount >= 2)
            {
                strError = "nRedoCount 超过范围";
                return -2;
            }

            var record = DomUtil.GetElementText(domOperLog.DocumentElement,
                elementName,
                out XmlNode node);
            var recPath = DomUtil.GetAttr(node, "recPath");
            if (string.IsNullOrEmpty(record)
                || string.IsNullOrEmpty(recPath))
            {
                strError = $"元素 {elementName} 中没有记录内容或者没有 recPath 属性";
                return 0;
            }

            /*
            // 先检查这条记录是否已经存在
            long lRet = channel.GetRes(recPath,
                out string result,
                out _,
                out byte[] timestamp,
                out string output_path,
                out strError);
            if (lRet == 0)
            {
                // 已经存在。改为追加
                recPath = ResPath.GetDbName(recPath) + "/?";
            }
            */

            // 写回册记录
            long lRet = channel.DoSaveTextRes(recPath,
                record,
                false,
                "content,ignorechecktimestamp",
                null,
                out _,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
                return -1;

            nRedoCount++;
            return 1;
        }

        // return:
        //      -2  nRedoCount 超过范围
        //      -1  出错
        //      0   日志记录中没有找到需要的元素
        //      1   成功。nRedoCount 自动增量 1
        int RecoverReaderRecord(
        RmsChannel channel,
        XmlDocument domOperLog,
        string elementName,
        ref int nRedoCount,
        out bool clipping,
        out string strError)
        {
            strError = "";
            clipping = false;

            if (nRedoCount >= 2)
            {
                strError = "nRedoCount 超过范围";
                return -2;
            }

            var record = DomUtil.GetElementText(domOperLog.DocumentElement,
                elementName,
                out XmlNode node);
            var recPath = DomUtil.GetAttr(node, "recPath");
            if (string.IsNullOrEmpty(record)
                || string.IsNullOrEmpty(recPath))
            {
                strError = $"元素 {elementName} 中没有记录内容或者没有 recPath 属性";
                return 0;
            }

            clipping = ((XmlElement)node).HasAttribute("clipping");

            /*
            // 先检查这条记录是否已经存在
            long lRet = channel.GetRes(recPath,
                out string result,
                out _,
                out byte[] timestamp,
                out string output_path,
                out strError);
            if (lRet == 0)
            {
                // 已经存在。改为追加
                recPath = ResPath.GetDbName(recPath) + "/?";
            }
            */

            if (clipping == false)
            {
                // 写回册记录
                long lRet = channel.DoSaveTextRes(recPath,
                    record,
                    false,
                    "content,ignorechecktimestamp",
                    null,
                    out _,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;
            }

            nRedoCount++;
            return 1;
        }

        // 根据 所借册条码号，清除若干读者记录中的借阅信息
        // 注意，要求读者库检索点有“所借册条码号”
        int ReturnAllReader(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strItemBarcode,
            string strExcludeReaderRecPath,
            out string strError)
        {
            strError = "";

            // TODO: 关注读者库 “所借册条码号” 检索途径是否存在。必须存在才行

            // 获得读者记录
            // return:
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            int nRet = this.GetReaderRecXml(
                channel,
                strItemBarcode,
                out string strTempReaderXml,
                100,
                out List<string> aReaderPath,
                out byte[] temp_timestamp,
                out strError);
            if (nRet == -1)
            {
                strError = "检索册条码号为 '" + strItemBarcode + "' 的一条或者多条册记录时发生错误: " + strError;
                return -1;
            }

            if (aReaderPath != null)
            {
                // 去除已经被作为当前记录的那条
                while (aReaderPath.Count > 0)
                {
                    nRet = aReaderPath.IndexOf(strExcludeReaderRecPath);
                    if (nRet != -1)
                        aReaderPath.Remove(strExcludeReaderRecPath);
                    else
                        break;
                }

                if (aReaderPath.Count >= 1)
                {
#if NO
                    RmsChannel channel = Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        return -1;
                    }
#endif

                    // 从若干读者记录中，清除特定的所借事项。相当于针对读者记录执行了还书操作
                    // parameters:
                    //      strBorrowItemBarcode    要清除的册记录条码号
                    //      aPath   读者记录路径集合
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = ClearBorrowItem(
                        channel,
                        strItemBarcode,
                        aReaderPath,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ClearBorrowItem() error: " + strError;
                        return -1;
                    }

                }
            }

            return 0;
        }

        // 从若干读者记录中，清除特定的所借事项。相当于针对读者记录执行了还书操作
        // parameters:
        //      strBorrowItemBarcode    要清除的册记录条码号
        //      aPath   读者记录路径集合
        // return:
        //      -1  出错
        //      0   成功
        static int ClearBorrowItem(
            RmsChannel channel,
            string strBorrowItemBarcode,
            List<string> reader_paths,
            out string strError)
        {
            strError = "";

            for (int i = 0; i < reader_paths.Count; i++)
            {
                string strXml = "";
                string strMetaData = "";
                string strOutputPath = "";
                byte[] timestamp = null;

                string strPath = reader_paths[i];

                long lRet = channel.GetRes(strPath,
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 装入DOM
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "记录 '" + strPath + "' XML装入DOM出错: " + ex.Message;
                    goto ERROR1;
                }

                bool bChanged = false;
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//borrows/borrow[@barcode='" + strBorrowItemBarcode + "']");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];
                    if (node.ParentNode != null)
                    {
                        node.ParentNode.RemoveChild(node);
                        bChanged = true;
                    }
                }

                if (bChanged == true)
                {
                    // 写读者记录
                    byte[] output_timestamp = null;
                    lRet = channel.DoSaveTextRes(strPath,
                        dom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "写入读者记录 '" + strPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 还书操作，修改读者和册记录
        // parameters:
        //      strItemBarcodeParam 册条码号。可以使用 @refID: 前缀
        int ReturnChangeReaderAndItemRecord(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strAction,
            //string strItemBarcodeParam,
            //string strReaderBarcode,
            XmlDocument domLog,
            string strRecoverComment,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 2017/10/24
            if (strAction != "return" && strAction != "lost")
            {
                strError = "ReturnChangeReaderAndItemRecord() 只能处理 strAction 为 'return' 和 'lost' 的情况，不能处理 '" + strAction + "'";
                return -1;
            }

            // 2024/2/13
            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "barcode");
            string strItemRefID = DomUtil.GetElementText(itemdom.DocumentElement,
                "refID");
            string strItemKey = dp2StringUtil.BuildReaderKey(strItemBarcode, strItemRefID);


            // 注意，读者证条码号可能为空
            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            string strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                "refID");
            string strReaderKey = dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID);

            string strReturnOperator = DomUtil.GetElementText(domLog.DocumentElement,
    "operator");

            string strOperTime = DomUtil.GetElementText(domLog.DocumentElement,
    "operTime");

            // *** 修改读者记录
            string strDeletedBorrowFrag = "";
            XmlNode dup_reader_history = null;

            // 既然日志记录中记载的是 @refID: 的形态，那读者记录中 borrows 里面势必记载的也是这个形态
            XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode(
                "borrows/borrow[@barcode='" + strItemBarcode + "']");
            if (nodeBorrow == null)
                readerdom.DocumentElement.SelectSingleNode(
                "borrows/borrow[@refID='" + strItemRefID + "']");
            if (nodeBorrow != null)
            {
                if (String.IsNullOrEmpty(strRecoverComment) == false)
                {
                    string strText = strRecoverComment;
                    string strOldRecoverComment = DomUtil.GetAttr(nodeBorrow, "recoverComment");
                    if (String.IsNullOrEmpty(strOldRecoverComment) == false)
                        strText = "(借阅时原注: " + strOldRecoverComment + ") " + strRecoverComment;
                    DomUtil.SetAttr(nodeBorrow, "recoverComment", strText);
                }
                strDeletedBorrowFrag = nodeBorrow.OuterXml;
                nodeBorrow.ParentNode.RemoveChild(nodeBorrow);

                // 获得几个查重需要的参数
                XmlDocument temp = new XmlDocument();
                temp.LoadXml(strDeletedBorrowFrag);
                string strItemBarcode0 = temp.DocumentElement.GetAttribute("barcode");
                string strBorrowDate = temp.DocumentElement.GetAttribute("borrowDate");
                string strBorrowPeriod = temp.DocumentElement.GetAttribute("borrowPeriod");

                dup_reader_history = readerdom.DocumentElement.SelectSingleNode("borrowHistory/borrow[@barcode='" + strItemBarcode0 + "' and @borrowDate='" + strBorrowDate + "' and @borrowPeriod='" + strBorrowPeriod + "']");
            }

            // 加入到读者记录借阅历史字段中

            if (string.IsNullOrEmpty(strDeletedBorrowFrag) == false
                && dup_reader_history == null)
            {
                // 看看根下面是否有 borrowHistory 元素
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrowHistory");
                if (root == null)
                {
                    root = readerdom.CreateElement("borrowHistory");
                    readerdom.DocumentElement.AppendChild(root);
                }

                XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                fragment.InnerXml = strDeletedBorrowFrag;

                // 插入到最前面
                XmlNode temp = DomUtil.InsertFirstChild(root, fragment);
                // 2007/6/19
                if (temp != null)
                {
                    // returnDate 加入还书时间
                    DomUtil.SetAttr(temp, "returnDate", strOperTime);

                    // borrowOperator
                    string strBorrowOperator = DomUtil.GetAttr(temp, "operator");
                    // 把原来的operator属性值复制到borrowOperator属性中
                    DomUtil.SetAttr(temp, "borrowOperator", strBorrowOperator);


                    // operator 此时需要表示还书操作者了
                    DomUtil.SetAttr(temp, "operator", strReturnOperator);

                }
                // 如果超过100个，则删除多余的
                while (root.ChildNodes.Count > this.MaxPatronHistoryItems)
                    root.RemoveChild(root.ChildNodes[root.ChildNodes.Count - 1]);

                // 2007/6/19
                // 增量借阅量属性值
                string strBorrowCount = DomUtil.GetAttr(root, "count");
                if (String.IsNullOrEmpty(strBorrowCount) == true)
                    strBorrowCount = "1";
                else
                {
                    long lCount = 1;
                    try
                    {
                        lCount = Convert.ToInt64(strBorrowCount);
                    }
                    catch { }
                    lCount++;
                    strBorrowCount = lCount.ToString();
                }
                DomUtil.SetAttr(root, "count", strBorrowCount);
            }

            // 增添超期信息
            string strOverdueString = DomUtil.GetElementText(domLog.DocumentElement,
                "overdues");
            if (String.IsNullOrEmpty(strOverdueString) == false)
            {
                XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                fragment.InnerXml = strOverdueString;

                List<string> existing_ids = new List<string>();

                // 看看根下面是否有overdues元素
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
                if (root == null)
                {
                    root = readerdom.CreateElement("overdues");
                    readerdom.DocumentElement.AppendChild(root);
                }
                else
                {
                    // 记载以前已经存在的 id
                    XmlNodeList nodes = root.SelectNodes("overdue");
                    foreach (XmlElement node in nodes)
                    {
                        string strID = node.GetAttribute("id");
                        if (string.IsNullOrEmpty(strID) == false)
                            existing_ids.Add(strID);
                    }
                }

                // root.AppendChild(fragment);
                {
                    // 一个一个加入，丢掉重复 id 属性值得 overdue 元素
                    XmlNodeList nodes = fragment.SelectNodes("overdue");
                    foreach (XmlElement node in nodes)
                    {
                        string strID = node.GetAttribute("id");
                        if (existing_ids.IndexOf(strID) != -1)
                            continue;
                        root.AppendChild(node);
                    }
                }
            }

            // *** 检查册记录操作前在借的读者，是否指向另外一个读者。如果是这样，则需要事先消除相关的另一个读者记录的痕迹，也就是说相当于把相关的册给进行还书操作
            string strBorrower0 = DomUtil.GetElementInnerText(itemdom.DocumentElement,
    "borrower");
            if (string.IsNullOrEmpty(strBorrower0) == false
                && strBorrower0 != strReaderBarcode
                && strBorrower0 != $"@refID:{strReaderRefID}")
            {
                string strRemovedInfo = "";

                // 去除读者记录侧的借阅信息链条
                // return:
                //      -1  出错
                //      0   没有必要修复
                //      1   修复成功
                nRet = RemoveReaderSideLink(
                    channel,
                    strBorrower0,
                    strItemBarcode, // strItemBarcodeParam,
                    strItemRefID,
                    out strRemovedInfo,
                    out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("册条码号为 '" + strItemKey + "' 的册记录，在进行还书操作(拟被读者 '" + strReaderKey + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件尝试自动修正(删除)此读者记录的半侧借阅信息链。不过，在去除读者记录册借阅链时发生错误: " + strError);
                }
                else
                {
                    this.WriteErrorLog("册条码号为 '" + strItemKey + "' 的册记录，在进行还书操作(拟被读者 '" + strReaderKey + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件已经自动修正(删除)了此读者记录的半侧借阅信息链。被移走的片断 XML 信息为 '" + strRemovedInfo + "'");
                }
            }


            // *** 修改册记录
            XmlElement nodeHistoryBorrower = null;

            string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement, "borrower");

            XmlNode dup_item_history = null;
            // 看看相同借者、借阅日期、换回日期的 BorrowHistory/borrower 元素是否已经存在
            {
                string strBorrowDate = DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate");
                dup_item_history = itemdom.DocumentElement.SelectSingleNode("borrowHistory/borrower[@barcode='" + strBorrower + "' and @borrowDate='" + strBorrowDate + "' and @returnDate='" + strOperTime + "']");
            }

            if (dup_item_history != null)
            {
                // 历史信息节点已经存在，就不必加入了

                // 清空相关元素
                DomUtil.DeleteElement(itemdom.DocumentElement,
    "borrower");
                DomUtil.DeleteElement(itemdom.DocumentElement,
"borrowDate");
                DomUtil.DeleteElement(itemdom.DocumentElement,
"returningDate");
                DomUtil.DeleteElement(itemdom.DocumentElement,
"borrowPeriod");
                DomUtil.DeleteElement(itemdom.DocumentElement,
"operator");
                DomUtil.DeleteElement(itemdom.DocumentElement,
"no");
                DomUtil.DeleteElement(itemdom.DocumentElement,
"renewComment");
            }
            else
            {
                // 加入历史信息节点

                // TODO: 也可从 domLog 中取得信息，创建 borrowHistory 下级事项。但要防范重复加入的情况
                // 这里判断册记录中 borrower 元素是否为空的做法，具有可以避免重复加入 borrowHistory 下级事项的优点
                if (string.IsNullOrEmpty(strBorrower) == false)
                {
                    // 加入到借阅历史字段中
                    {
                        // 看看根下面是否有borrowHistory元素
                        XmlNode root = itemdom.DocumentElement.SelectSingleNode("borrowHistory");
                        if (root == null)
                        {
                            root = itemdom.CreateElement("borrowHistory");
                            itemdom.DocumentElement.AppendChild(root);
                        }

                        nodeHistoryBorrower = itemdom.CreateElement("borrower");

                        // 插入到最前面
                        nodeHistoryBorrower = DomUtil.InsertFirstChild(root, nodeHistoryBorrower) as XmlElement;  // 2015/1/12 增加等号左边的部分

                        // 如果超过100个，则删除多余的
                        while (root.ChildNodes.Count > this.MaxItemHistoryItems)
                            root.RemoveChild(root.ChildNodes[root.ChildNodes.Count - 1]);
                    }

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
                    "barcode",
                    DomUtil.GetElementText(itemdom.DocumentElement, "borrower"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrower", "");
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
        "borrower",
        nodeHistoryBorrower,
        "barcode",
        true);

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
                  "borrowDate",
                  DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrowDate", "");
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
    "borrowDate",
    nodeHistoryBorrower,
    "borrowDate",
    true);

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
      "returningDate",
      DomUtil.GetElementText(itemdom.DocumentElement, "returningDate"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "returningDate", "");
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
    "returningDate",
    nodeHistoryBorrower,
    "returningDate",
    true);

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
                   "borrowPeriod",
                   DomUtil.GetElementText(itemdom.DocumentElement, "borrowPeriod"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrowPeriod", "");
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
                        "borrowPeriod",
                        nodeHistoryBorrower,
                        "borrowPeriod",
                        true);

                    // borrowOperator
#if NO
                DomUtil.SetAttr(nodeOldBorrower,
      "borrowOperator",
      DomUtil.GetElementText(itemdom.DocumentElement, "operator"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "operator", "");
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
    "operator",
    nodeHistoryBorrower,
    "borrowOperator",
    true);

                    // operator 本次还书的操作者
                    DomUtil.SetAttr(nodeHistoryBorrower,
                      "operator",
                      strReturnOperator);

                    DomUtil.SetAttr(nodeHistoryBorrower,
          "returnDate",
          strOperTime);

                    // TODO: 0 需要省略
#if NO
                DomUtil.SetAttr(nodeOldBorrower,
                    "no",
                    DomUtil.GetElementText(itemdom.DocumentElement, "no"));
                DomUtil.DeleteElement(itemdom.DocumentElement,
                    "no");
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
    "no",
    nodeHistoryBorrower,
    "no",
    true);

                    // renewComment
#if NO
                {
                    string strTemp = DomUtil.GetElementText(itemdom.DocumentElement, "renewComment");
                    if (string.IsNullOrEmpty(strTemp) == true)
                        strTemp = null;

                    DomUtil.SetAttr(nodeOldBorrower,
                       "renewComment",
                       strTemp);

                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "renewComment");
                }
#endif
                    LibraryServerUtil.SetAttribute(ref itemdom,
    "renewComment",
    nodeHistoryBorrower,
    "renewComment",
    true);

                    {
                        string strText = strRecoverComment;
                        string strOldRecoverComment = DomUtil.GetElementText(itemdom.DocumentElement, "recoverComment");
                        if (String.IsNullOrEmpty(strOldRecoverComment) == false)
                            strText = "(借阅时原注: " + strOldRecoverComment + ") " + strRecoverComment;

                        if (String.IsNullOrEmpty(strText) == false)
                        {
                            DomUtil.SetAttr(nodeHistoryBorrower,
                                "recoverComment",
                                strText);
                        }
                    }
                }

                if (strAction == "lost")
                {
                    // 修改册记录的<state>
                    string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                        "state");
                    if (nodeHistoryBorrower != null)
                    {
                        DomUtil.SetAttr(nodeHistoryBorrower,
        "state",
        strState);
                    }

                    if (String.IsNullOrEmpty(strState) == false)
                        strState += ",";
                    strState += "丢失";
                    DomUtil.SetElementText(itemdom.DocumentElement,
                        "state", strState);

                    // 将日志记录中的<lostComment>内容追加写入册记录的<comment>中
                    string strLostComment = DomUtil.GetElementText(domLog.DocumentElement,
                        "lostComment");

                    if (strLostComment != "")
                    {
                        string strComment = DomUtil.GetElementText(itemdom.DocumentElement,
                            "comment");

                        if (nodeHistoryBorrower != null)
                        {
                            DomUtil.SetAttr(nodeHistoryBorrower,
                                "comment",
                                strComment);
                        }

                        if (String.IsNullOrEmpty(strComment) == false)
                            strComment += "\r\n";
                        strComment += strLostComment;
                        DomUtil.SetElementText(itemdom.DocumentElement,
                            "comment", strComment);
                    }
                }
            }

            return 0;
        }

        #endregion

        // SetEntities() API 恢复动作
        /* 日志记录格式
<root>
  <operation>setEntity</operation> 操作类型
  <action>new</action> 具体动作。有new change delete setuid transfer move。2019/7/30 增加 transfer，transfer 行为和 change 相似
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书实体/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文图书实体/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) <record>中的内容, 涉及到流通的<borrower><borrowDate><borrowPeriod>等, 在日志恢复阶段, 都应当无效, 这几个内容应当从当前位置库中记录获取, 和<record>中其他内容合并后, 再写入数据库
	3) 一次SetEntities()API调用, 可能创建多条日志记录。
         
         * */
        // TODO: 要兑现style中force nocheckdup功能
        public int RecoverSetEntity(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            string db_type,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            if (db_type != "entity" && db_type != "order"
                && db_type != "issue" && db_type != "comment")
            {
                throw new ArgumentException($"参数 db_type 值 '{db_type}' 不合法。应为 entity order issue comment 之一");
            }

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            bool bReuse = false;    // 是否能够不顾 RecoverLevel 状态而重用部分代码

        DO_SNAPSHOT:

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                //byte[] timestamp = null;
                //byte[] output_timestamp = null;
                //string strOutputPath = "";

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "transfer"
                    || strAction == "setuid"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    if (strAction == "move")
                    {
                        // 2024/4/3
                        int nRedoCount = 0;
                    REDO_COPY:
                        // 复制源记录到目标位置，然后自动删除源记录
                        // 但是尚未在目标位置写入最新内容
                        // parameters:
                        //      strMergeStyle           如何合并两条记录的 XML 部分和下属对象?
                        //                              关于 XML 部分: reserve_source / reserve_target 之一。 缺省两者，则表示 reserve_source
                        //                              关于下属对象部分：file_reserve_source 和 file_reserve_target 组合使用。如果两者都没有出现，表示最后的目标记录中会被去掉所有 file 元素。这是 2017/4/19 新增的参数值。以前版本都是自动合并源和目标的全部 files 元素
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strNewRecPath,
                            true,   // bDeleteSourceRecord
                            "file_reserve_source",
                            out _,
                            out _,
                            out _,
                            out strError);
                        if (lRet == -1)
                        {
                            // 如果源数据库不存在或者源记录不存在
                            if ((channel.OriginErrorCode == ErrorCodeValue.NotFoundDb || channel.OriginErrorCode == ErrorCodeValue.NotFound)
                                && robust == true)
                            {
                                // 直接用日志记录中的新记录内容写册记录
                                nRet = TryWriteItemRecord(channel,
                                    strNewRecPath,
                                    strRecord,
                                    db_type,    // "entity",
                                    robust,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                return 0;
                            }
                            // 目标数据库不存在
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                                && robust)
                            {
                                nRet = TryCreateBiblioDatabase(
            channel,
            strNewRecPath,
            ref nRedoCount,
            out string error);
                                if (nRet == 0)
                                    goto REDO_COPY;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库和下级{db_type}库的过程出错: {error}";
                                    return -1;
                                }
                            }
                            return -1;
                        }
                    }
                    else
                    {
                        // 写册记录
                        nRet = TryWriteItemRecord(channel,
                            strNewRecPath,
                            strRecord,
                            db_type,    // "entity",
                            robust,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

#if REMOVED
                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写册记录
                        lRet = channel.DoSaveTextRes(strNewRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateBiblioDatabase(
        channel,
        strNewRecPath,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入册记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }
#endif

                    if (strAction == "move")
                    {
                        // 注: 前面 DoCopyRecord() 已经删除源记录
                        /*
                        lRet = TryDeleteRes(
                            channel,
                            strOldRecPath,
                            null,   // timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除册记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                        */
#if REMOVED
                        // 删除册记录
                        int nRedoCount = 0;

                    REDO_DELETE:
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                return 0;   // 记录本来就不存在

                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            strError = "删除册记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#endif
                    }

                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除册记录
                    lRet = TryDeleteRes(
                        channel,
                        strRecPath,
                        null,   // timestamp,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"删除{db_type}记录 '{strRecPath}' 时发生错误: {strError}";
                        return -1;
                    }
#if REMOVED
                    int nRedoCount = 0;
                REDO:
                    // 删除册记录
                    lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO;
                            }
                        }
                        strError = "删除册记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#endif
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }

                return 0;
            }

            bool bForce = false;
            // bool bNoCheckDup = false;

            string strStyle = DomUtil.GetElementText(domLog.DocumentElement,
                "style");

            if (StringUtil.IsInList("force", strStyle) == true)
                bForce = true;

            //if (StringUtil.IsInList("nocheckdup", strStyle) == true)
            //    bNoCheckDup = true;

            // 逻辑恢复或者混合恢复
            if (IsLogic(level, out robust))
            {
                // 和数据库中已有记录合并，然后保存
                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "transfer"
                    || strAction == "setuid"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    // 读出数据库中原有的记录
                    string strExistXml = "";
                    string strMetaData = "";
                    byte[] exist_timestamp = null;
                    string strOutputPath = "";

                    if ((strAction == "change"
                        || strAction == "transfer"
                        || strAction == "setuid"
                        || strAction == "move")
                        && bForce == false) // 2008/10/6
                    {
                        string strSourceRecPath = "";

                        if (strAction == "change" || strAction == "transfer" || strAction == "setuid")
                            strSourceRecPath = strNewRecPath;
                        if (strAction == "move")
                            strSourceRecPath = strOldRecPath;

                        lRet = channel.GetRes(strSourceRecPath,
                            out strExistXml,
                            out strMetaData,
                            out exist_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 容错
                            if (channel.IsNotFoundOrDamaged()
                                && /*level == RecoverLevel.LogicAndSnapshot*/robust)
                            {
                                // 如果记录不存在, 则构造一条空的记录
                                // bExist = false;
                                strExistXml = "<root />";
                                exist_timestamp = null;
                            }
                            else
                            {
                                strError = "在读入原有记录 '" + strNewRecPath + "' 时失败: " + strError;
                                goto ERROR1;
                            }
                        }
                    }

                    //
                    // 把两个记录装入DOM

                    XmlDocument domExist = new XmlDocument();
                    XmlDocument domNew = new XmlDocument();

                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strExistXml) == true)
                            strExistXml = "<root />";

                        domExist.LoadXml(strExistXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    try
                    {
                        domNew.LoadXml(strRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strRecord装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 合并新旧记录
                    string strNewXml = "";

                    if (false/*bForce == false*/)   // 注: 日志记录中记载的是实际写入的记录内容，为简化处理，这里不重新进行合并了
                    {
                        if (db_type == "entity")
                        {
                            // 2020/10/12
                            string[] elements = null;
                            if (strAction == "transfer")
                                elements = transfer_entity_element_names;
                            else if (strAction == "setuid")
                                elements = setuid_entity_element_names;

                            nRet = MergeTwoEntityXml(
                                null,
                                domExist,
                                domNew,
                                elements,   // strAction == "transfer" ? transfer_entity_element_names : null,
                                false,
                                out strNewXml,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        if (db_type == "order" || db_type == "issue" || db_type == "comment")
                        {
                            // 模拟一个 SessionInfo
                            string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
            "libraryCode");
                            string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
            "operator");
                            SessionInfo temp_sessioninfo = new SessionInfo(this);
                            temp_sessioninfo.Account = new Account();
                            temp_sessioninfo.Account.AccountLibraryCode = strLibraryCode;
                            temp_sessioninfo.Account.UserID = strOperator;

                            try
                            {
                                ItemDatabase itemDatabase = null;
                                if (db_type == "order")
                                    itemDatabase = this.OrderItemDatabase;
                                else if (db_type == "issue")
                                    itemDatabase = this.IssueItemDatabase;
                                else if (db_type == "comment")
                                    itemDatabase = this.CommentItemDatabase;
                                // TODO: 按理说日志中应该是当时操作时最终写入的内容，恢复的时候就不用经过合并这一步了
                                nRet = itemDatabase.MergeTwoItemXml(
                                    temp_sessioninfo,
                                    strAction,
                                    domExist,
                                    domNew,
                                    out strNewXml,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                            }
                            finally
                            {
                                temp_sessioninfo.CloseSession();
                            }
                        }
                    }
                    else
                    {
                        strNewXml = domNew.OuterXml;
                    }

                    // 保存新记录
                    byte[] output_timestamp = null;

                    if (strAction == "move")
                    {
                        int nRedoCount = 0;
                    REDO_COPY:
                        // 复制源记录到目标位置，然后自动删除源记录
                        // 但是尚未在目标位置写入最新内容
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strNewRecPath,
                            true,   // bDeleteSourceRecord
                            "file_reserve_source",  // 2024/4/28
                            out _,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // TODO: 源数据库不存在

                            // 目标数据库不存在
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                                && robust)
                            {
                                nRet = TryCreateBiblioDatabase(
            channel,
            strNewRecPath,
            ref nRedoCount,
            out string error);
                                if (nRet == 0)
                                    goto REDO_COPY;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库和下级实体库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            goto ERROR1;
                        }

                        exist_timestamp = output_timestamp; // 及时更新时间戳
                    }

                    lRet = channel.DoSaveTextRes(strNewRecPath,
                        strNewXml,
                        false,   // include preamble?
                        "content,ignorechecktimestamp",
                        exist_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    /*
                    if (strAction == "move")
                    {
                        // 删除册记录
                        int nRedoCount = 0;

                        byte[] timestamp = null;

                    REDO_DELETE:
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsEqualNotFound())
                                return 0;   // 记录本来就不存在

                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            strError = "删除册记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;

                        }
                    }
                     * */
                }
                else if (strAction == "delete")
                {
                    // 和SnapShot方式相同
                    bReuse = true;
                    goto DO_SNAPSHOT;
                }
                else
                {
                    strError = "无法识别的 <action> 内容 '" + strAction + "'";
                    return -1;
                }
            }

#if REMOVED
            // 容错恢复
            if (level == RecoverLevel.Robust)
            {
                if (strAction == "move")
                {
                    strError = "暂不支持SetEntity的move恢复操作";
                    return -1;
                }

                // 和数据库中已有记录合并，然后保存
                if (strAction == "change"
                    || strAction == "transfer"
                    || strAction == "setuid"
                    || strAction == "new")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    // 取得日志记录中声称的新记录路径。不能轻易相信这个路径。
                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";

                    string strOldItemBarcode = "";
                    string strNewItemBarcode = "";

                    string strExistXml = "";
                    byte[] exist_timestamp = null;

                    // 日志记录中记载的旧记录体
                    strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        if (strAction == "change" || strAction == "transfer" || strAction == "setuid")
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }
                    }

                    // 日志记录中声称的旧记录路径。不能轻易相信这个路径。
                    if (node != null)
                        strOldRecPath = DomUtil.GetAttr(node, "recPath");

                    // 从日志记录中记载的旧记录体中，获得旧记录册条码号
                    if (String.IsNullOrEmpty(strOldRecord) == false)
                    {
                        nRet = GetItemBarcode(strOldRecord,
                            out strOldItemBarcode,
                            out strError);
                    }

                    nRet = GetItemBarcode(strRecord,
                        out strNewItemBarcode,
                        out strError);

                    // TODO: 需要检查新旧记录中，<barcode>是否一致？如果不一致，则需要对新条码号进行查重？
                    if (strAction == "new" && strOldItemBarcode == "")
                    {
                        if (String.IsNullOrEmpty(strNewItemBarcode) == true)
                        {
                            strError = "因为拟新创建的记录内容中没有包含册条码号，所以new操作被放弃";
                            return -1;
                        }

                        strOldItemBarcode = strNewItemBarcode;
                    }

                    // 如果有旧记录的册条码号，则需要从数据库中提取最新鲜的旧记录
                    // (如果没有旧记录的册条码号，则依日志记录中的旧记录)
                    if (String.IsNullOrEmpty(strOldItemBarcode) == false)
                    {
                        string strOutputItemRecPath = "";

                        // 从册条码号获得册记录

                        // 获得册记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetItemRecXml(
                            // Channels,
                            channel,
                            strOldItemBarcode,
                            out strExistXml,
                            100,
                            out List<string> aPath,
                            out exist_timestamp,
                            out strError);
                        if (nRet == 0 || nRet == -1)
                        {
                            if (strAction == "change" || strAction == "transfer" || strAction == "setuid")
                            {
                                /*
                                // 从库中没有找到，只好依日志记录中记载的旧记录
                                strExistXml = strOldRecord;
                                 * */
                                strExistXml = "";

                                // 需要创建一条新记录。strOldRecPath中的路径似乎也可以用，但是要严格检查这个路径是否已经存在记录 -- 只能在这里位置不存在记录时才能用。既然如此麻烦，那就不如纯粹用一个新位置
                                strOutputItemRecPath = ResPath.GetDbName(strOldRecPath) + "/?";
                            }
                            else
                            {
                                Debug.Assert(strAction == "new", "");
                                strExistXml = "";
                                strOutputItemRecPath = ResPath.GetDbName(strNewRecPath) + "/?";
                            }
                        }
                        else
                        {
                            // 找到一条或者多条旧记录
                            Debug.Assert(aPath != null && aPath.Count >= 1, "");

                            bool bNeedReload = false;

                            if (aPath.Count == 1)
                            {
                                Debug.Assert(nRet == 1, "");

                                strOutputItemRecPath = aPath[0];

                                // 是否需要重新装载？
                                bNeedReload = false;    // 所取得的第一个路径，其记录已经装载
                            }
                            else
                            {
                                // 多条
                                Debug.Assert(aPath.Count > 1, "");

                                ///
                                // 建议根据strOldRecPath来进行挑选
                                if (String.IsNullOrEmpty(strOldRecPath) == true)
                                {
                                    // 空，无法挑选

                                    // 容错！
                                    strOutputItemRecPath = aPath[0];

                                    // 是否需要重新装载？
                                    bNeedReload = false;    // 所取得的第一个路径，其记录已经装载
                                }
                                else
                                {

                                    ///// 
                                    nRet = aPath.IndexOf(strOldRecPath);
                                    if (nRet != -1)
                                    {
                                        // 选中
                                        strOutputItemRecPath = aPath[nRet];

                                        // 是否需要重新装载？
                                        if (nRet != 0)
                                            bNeedReload = true; // 第一个以外的路径才需要装载

                                    }
                                    else
                                    {
                                        // 没有选中，只好依第一个

                                        // 容错
                                        strOutputItemRecPath = aPath[0];

                                        // 是否需要重新装载？
                                        bNeedReload = false;    // 所取得的第一个路径，其记录已经装载
                                    }
                                }

                                ///

                            }

                            // 重新装载
                            if (bNeedReload == true)
                            {
                                lRet = channel.GetRes(strOutputItemRecPath,
                                    out strExistXml,
                                    out string strMetaData,
                                    out exist_timestamp,
                                    out strOutputItemRecPath,
                                    out strError);
                                if (lRet == -1)
                                {
                                    strError = "根据strOutputItemRecPath '" + strOutputItemRecPath + "' 重新获得册记录失败: " + strError;
                                    return -1;
                                }

                                // 需要检查记录中的<barcode>元素值是否匹配册条码号
                            }
                        }

                        // 修正strOldRecPath
                        if (strOutputItemRecPath != "")
                            strOldRecPath = strOutputItemRecPath;
                        else
                            strOldRecPath = ""; // 破坏掉，以免后面被用

                        strNewRecPath = strOutputItemRecPath;

                    } // end if 如果有旧记录的册条码号
                    else
                    {
                        // (如果没有旧记录的册条码号，则依日志记录中的旧记录)
                        // 但无法确定旧记录的路径。也就无法确定覆盖位置。因此建议放弃这种特定的“修改操作”。
                        strError = "因为日志记录中没有记载旧记录条码号，因此无法确定记录位置，因此change操作被放弃";
                        return -1;
                    }

                    if (strAction == "change" || strAction == "transfer" || strAction == "setuid")
                    {
                        if (strNewItemBarcode != ""
                            && strNewItemBarcode != strOldItemBarcode)
                        {
                            // 新旧记录的条码号不一致，需要对新条码号进行查重

                            // 获得册记录
                            // return:
                            //      -1  error
                            //      0   not found
                            //      1   命中1条
                            //      >1  命中多于1条
                            nRet = this.GetItemRecXml(
                                // Channels,
                                channel,
                                strNewItemBarcode,
                                out string strTempXml,
                                100,
                                out List<string> aPath,
                                out byte[] temp_timestamp,
                                out strError);
                            if (nRet > 0)
                            {
                                // 有重复，取其第一条，作为老记录进行合并，并保存回这条的位置
                                strNewRecPath = aPath[0];
                                exist_timestamp = temp_timestamp;
                                strExistXml = strTempXml;
                            }
                        }
                    }

                    // 把两个记录装入DOM
                    XmlDocument domExist = new XmlDocument();
                    XmlDocument domNew = new XmlDocument();

                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strExistXml) == true)
                            strExistXml = "<root />";

                        domExist.LoadXml(strExistXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    try
                    {
                        domNew.LoadXml(strRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strRecord装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 合并新旧记录
                    string strNewXml = "";

                    if (bForce == false)
                    {
                        // 2020/10/12
                        string[] elements = null;
                        if (strAction == "transfer")
                            elements = transfer_entity_element_names;
                        else if (strAction == "setuid")
                            elements = setuid_entity_element_names;

                        nRet = MergeTwoEntityXml(
                            null,
                            domExist,
                            domNew,
                            elements,   // strAction == "transfer" ? transfer_entity_element_names : null,
                            false,
                            out strNewXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        strNewXml = domNew.OuterXml;
                    }

                    // 保存新记录
                    byte[] output_timestamp = null;

                    string strOutputPath = "";

                    if (strAction == "move")
                    {
                        // 复制源记录到目标位置，然后自动删除源记录
                        // 但是尚未在目标位置写入最新内容
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strNewRecPath,
                            true,   // bDeleteSourceRecord
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        exist_timestamp = output_timestamp; // 及时更新时间戳
                    }

                    /*
                    // 测试
                    {
                        string strRecID = ResPath.GetRecordId(strNewRecPath);

                        if (strRecID != "?")
                        {
                            try
                            {
                                long id = Convert.ToInt64(strRecID);
                                if (id > 150848)
                                {
                                    Debug.Assert(false, "id超过尾部");
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                     * */

                    lRet = channel.DoSaveTextRes(strNewRecPath,
                        strNewXml,
                        false,   // include preamble?
                        "content,ignorechecktimestamp",
                        exist_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    nRet = GetItemBarcode(strOldRecord,
                        out string strOldItemBarcode,
                        out strError);
                    if (String.IsNullOrEmpty(strOldItemBarcode) == true)
                    {
                        strError = "因为日志记录中的旧记录中缺乏非空的<barcode>内容，所以无法进行依据条码号定位的删除，delete操作被放弃";
                        return -1;
                    }

                    string strOutputItemRecPath = "";

                    // 从册条码号获得册记录

                    // 获得册记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetItemRecXml(
                        // Channels,
                        channel,
                        strOldItemBarcode,
                        out string strExistXml,
                        100,
                        out List<string> aPath,
                        out byte[] exist_timestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        // 本来就不存在
                        return 0;
                    }
                    if (nRet >= 1)
                    {
                        ///
                        // 找到一条或者多条旧记录
                        Debug.Assert(aPath != null && aPath.Count >= 1, "");

                        bool bNeedReload = false;

                        if (aPath.Count == 1)
                        {
                            Debug.Assert(nRet == 1, "");

                            /*
                            strOutputItemRecPath = aPath[0];

                            // 是否需要重新装载？
                            bNeedReload = false;    // 所取得的第一个路径，其记录已经装载
                             * */
                            strError = "册条码号 " + strOldItemBarcode + " 目前仅有唯一一条记录，放弃删除";
                            return -1;
                        }
                        else
                        {
                            // 多条
                            Debug.Assert(aPath.Count > 1, "");

                            ///
                            // 建议根据strRecPath来进行挑选
                            if (String.IsNullOrEmpty(strRecPath) == true)
                            {
                                strError = "册条码号 '" + strOldItemBarcode + "' 命中 " + aPath.Count.ToString() + " 条记录，而<oldRecord>的recPath参数缺乏，因此无法进行精确删除，delete操作被放弃";
                                return -1;
                            }
                            else
                            {

                                ///// 
                                nRet = aPath.IndexOf(strRecPath);
                                if (nRet != -1)
                                {
                                    // 选中
                                    strOutputItemRecPath = aPath[nRet];

                                    // 是否需要重新装载？
                                    if (nRet != 0)
                                        bNeedReload = true; // 第一个以外的路径才需要装载

                                }
                                else
                                {
                                    strError = "册条码号 '" + strOldItemBarcode + "' 命中 " + aPath.Count.ToString() + " 条记录，虽用了(<oldRecord>元素中属性recPath的)确认路径 '" + strRecPath + "' 也无法确认出其中一条，无法精确删除，因此delete操作被放弃";
                                    return -1;
                                }
                            }
                        }

                        ///

                        // 重新装载
                        if (bNeedReload == true)
                        {
                            lRet = channel.GetRes(strOutputItemRecPath,
                                out strExistXml,
                                out string strMetaData,
                                out exist_timestamp,
                                out strOutputItemRecPath,
                                out strError);
                            if (lRet == -1)
                            {
                                strError = "根据strOutputItemRecPath '" + strOutputItemRecPath + "' 重新获得册记录失败: " + strError;
                                return -1;
                            }

                            // 需要检查记录中的<barcode>元素值是否匹配册条码号
                        }
                    }

                    // 把两个记录装入DOM
                    XmlDocument domExist = new XmlDocument();
                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strExistXml) == true)
                            strExistXml = "<root />";

                        domExist.LoadXml(strExistXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                        return -1;
                    }

                    bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                        out string strDetail);

                    // 观察已经存在的记录是否有流通信息
                    if (bHasCirculationInfo == true
                        && bForce == false)
                    {
                        strError = "拟删除的册记录 '" + strOutputItemRecPath + "' 中包含有流通信息(" + strDetail + ")，不能删除。";
                        goto ERROR1;
                    }

                    int nRedoCount = 0;
                    byte[] timestamp = exist_timestamp;
                    byte[] output_timestamp = null;

                REDO:
                    // 删除册记录
                    lRet = channel.DoDeleteRes(strOutputItemRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO;
                            }
                        }
                        strError = "删除册记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }
#endif
            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetEntity() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

#if REMOVED
        // SetOrders() API 恢复动作
        /* 日志记录格式
<root>
  <operation>setOrder</operation> 操作类型
  <action>new</action> 具体动作。有new change delete 3种
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书订购/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文图书订购/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) 一次SetOrders()API调用, 可能创建多条日志记录。
         
         * */
        // TODO: 要兑现style中force nocheckdup功能
        public int RecoverSetOrder(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                byte[] timestamp = null;
                //byte[] output_timestamp = null;
                //string strOutputPath = "";

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    // 写订购记录
                    nRet = TryWriteItemRecord(channel,
                        strNewRecPath,
                        strRecord,
                        "order",
                        robust,
                        out strError);
                    if (nRet == -1)
                        return -1;
#if REMOVED
                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写订购记录
                        lRet = channel.DoSaveTextRes(strNewRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateBiblioDatabase(
        channel,
        strNewRecPath,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入订购记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }
#endif

                    if (strAction == "move")
                    {
                        lRet = TryDeleteRes(
                            channel,
                            strOldRecPath,
                            timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除订购记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        // 删除订购记录
                        int nRedoCount = 0;

                    REDO_DELETE:
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                return 0;   // 记录本来就不存在

                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            strError = "删除订购记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;

                        }
#endif
                    }

                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除订购记录
                    lRet = TryDeleteRes(
                        channel,
                        strRecPath,
                        timestamp,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "删除订购记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#if REMOVED
                    int nRedoCount = 0;
                REDO:
                    // 删除订购记录
                    lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO;
                            }
                        }
                        strError = "删除订购记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;

                    }
#endif
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }

                return 0;
            }

            bool bForce = false;
            // bool bNoCheckDup = false;

            string strStyle = DomUtil.GetElementText(domLog.DocumentElement,
                "style");

            if (StringUtil.IsInList("force", strStyle) == true)
                bForce = true;

            //if (StringUtil.IsInList("nocheckdup", strStyle) == true)
            //    bNoCheckDup = true;

            // 逻辑恢复或者混合恢复或者容错恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot
                || level == RecoverLevel.Robust*/
                IsLogic(level, out robust))    // 容错恢复没有单独实现
            {
                // 和数据库中已有记录合并，然后保存
                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    // 读出数据库中原有的记录
                    string strExistXml = "";
                    string strMetaData = "";
                    byte[] exist_timestamp = null;
                    string strOutputPath = "";

                    if ((strAction == "change"
                        || strAction == "move")
                        && bForce == false)
                    {
                        string strSourceRecPath = "";

                        if (strAction == "change")
                            strSourceRecPath = strNewRecPath;
                        if (strAction == "move")
                            strSourceRecPath = strOldRecPath;

                        lRet = channel.GetRes(strSourceRecPath,
                            out strExistXml,
                            out strMetaData,
                            out exist_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 容错
                            if (channel.IsNotFoundOrDamaged()
                                && /*level == RecoverLevel.LogicAndSnapshot*/robust)
                            {
                                // 如果记录不存在, 则构造一条空的记录
                                // bExist = false;
                                strExistXml = "<root />";
                                exist_timestamp = null;
                            }
                            else
                            {
                                strError = "在读入原有记录 '" + strNewRecPath + "' 时失败: " + strError;
                                goto ERROR1;
                            }
                        }
                    }

                    //
                    // 把两个记录装入DOM

                    XmlDocument domExist = new XmlDocument();
                    XmlDocument domNew = new XmlDocument();

                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strExistXml) == true)
                            strExistXml = "<root />";

                        domExist.LoadXml(strExistXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    try
                    {
                        domNew.LoadXml(strRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strRecord 装载进入 DOM 时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 合并新旧记录
                    string strNewXml = "";

                    if (false/*bForce == false*/)      // 注: 日志记录中记载的是实际写入的记录内容，为简化处理，这里不重新进行合并了
                    {
                        // 模拟一个 SessionInfo
                        string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
        "libraryCode");
                        string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
        "operator");
                        SessionInfo temp_sessioninfo = new SessionInfo(this);
                        temp_sessioninfo.Account = new Account();
                        temp_sessioninfo.Account.AccountLibraryCode = strLibraryCode;
                        temp_sessioninfo.Account.UserID = strOperator;

                        try
                        {
                            // TODO: 按理说日志中应该是当时操作时最终写入的内容，恢复的时候就不用经过合并这一步了
                            nRet = this.OrderItemDatabase.MergeTwoItemXml(
                                temp_sessioninfo,
                                strAction,
                                domExist,
                                domNew,
                                out strNewXml,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        finally
                        {
                            temp_sessioninfo.CloseSession();
                        }
                    }
                    else
                    {
                        strNewXml = domNew.OuterXml;
                    }

                    // 保存新记录
                    byte[] output_timestamp = null;

                    if (strAction == "move")
                    {
                        int nRedoCount = 0;
                    REDO_COPY:
                        // 复制源记录到目标位置，然后自动删除源记录
                        // 但是尚未在目标位置写入最新内容
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strNewRecPath,
                            true,   // bDeleteSourceRecord
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 目标数据库不存在
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                                && robust)
                            {
                                nRet = TryCreateBiblioDatabase(
            channel,
            strNewRecPath,
            ref nRedoCount,
            out string error);
                                if (nRet == 0)
                                    goto REDO_COPY;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库和下级订购库的过程出错: {error}";
                                    return -1;
                                }
                            }
                            goto ERROR1;
                        }

                        exist_timestamp = output_timestamp; // 及时更新时间戳
                    }


                    lRet = channel.DoSaveTextRes(strNewRecPath,
                        strNewXml,
                        false,   // include preamble?
                        "content,ignorechecktimestamp",
                        exist_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (strAction == "delete")
                {
                    // 和SnapShot方式相同
                    bReuse = true;
                    goto DO_SNAPSHOT;
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetOrder() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }
#endif

#if REMOVED
        // SetIssues() API 恢复动作
        /* 日志记录格式
<root>
  <operation>setIssue</operation> 操作类型
  <action>new</action> 具体动作。有new change delete 3种
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文期刊期/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文期刊期/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) 一次SetIssues()API调用, 可能创建多条日志记录。
         
         * */
        // TODO: 要兑现style中force nocheckdup功能
        public int RecoverSetIssue(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {

                byte[] timestamp = null;
                //byte[] output_timestamp = null;
                //string strOutputPath = "";

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    // 写期记录
                    nRet = TryWriteItemRecord(channel,
                        strNewRecPath,
                        strRecord,
                        "issue",
                        robust,
                        out strError);
                    if (nRet == -1)
                        return -1;
#if REMOVED
                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写期记录
                        lRet = channel.DoSaveTextRes(strNewRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateBiblioDatabase(
        channel,
        strNewRecPath,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入期记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }
#endif

                    if (strAction == "move")
                    {
                        // 删除期记录
                        lRet = TryDeleteRes(
                            channel,
                            strOldRecPath,
                            timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除期记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        // 删除期记录
                        int nRedoCount = 0;

                    REDO_DELETE:
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                return 0;   // 记录本来就不存在

                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            strError = "删除期记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;

                        }
#endif
                    }

                }
                else if (strAction == "delete")
                {
                    XmlNode node = null;
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除期记录
                    lRet = TryDeleteRes(
                        channel,
                        strRecPath,
                        timestamp,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "删除期记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

#if REMOVED
                    int nRedoCount = 0;
                REDO:
                    // 删除期记录
                    lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO;
                            }
                        }
                        strError = "删除期记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#endif
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }


                return 0;
            }

            bool bForce = false;
            // bool bNoCheckDup = false;

            string strStyle = DomUtil.GetElementText(domLog.DocumentElement,
                "style");

            if (StringUtil.IsInList("force", strStyle) == true)
                bForce = true;

            //if (StringUtil.IsInList("nocheckdup", strStyle) == true)
            //    bNoCheckDup = true;

            // 逻辑恢复或者混合恢复或者容错恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot
                || level == RecoverLevel.Robust*/
                IsLogic(level, out robust))    // 容错恢复没有单独实现
            {
                // 和数据库中已有记录合并，然后保存
                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }


                    // 读出数据库中原有的记录
                    string strExistXml = "";
                    string strMetaData = "";
                    byte[] exist_timestamp = null;
                    string strOutputPath = "";

                    if ((strAction == "change"
                        || strAction == "move")
                        && bForce == false)
                    {
                        string strSourceRecPath = "";

                        if (strAction == "change")
                            strSourceRecPath = strNewRecPath;
                        if (strAction == "move")
                            strSourceRecPath = strOldRecPath;

                        lRet = channel.GetRes(strSourceRecPath,
                            out strExistXml,
                            out strMetaData,
                            out exist_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 容错
                            if (channel.IsNotFoundOrDamaged()
                                && /*level == RecoverLevel.LogicAndSnapshot*/robust)
                            {
                                // 如果记录不存在, 则构造一条空的记录
                                // bExist = false;
                                strExistXml = "<root />";
                                exist_timestamp = null;
                            }
                            else
                            {
                                strError = "在读入原有记录 '" + strNewRecPath + "' 时失败: " + strError;
                                goto ERROR1;
                            }
                        }
                    }

                    //
                    // 把两个记录装入DOM

                    XmlDocument domExist = new XmlDocument();
                    XmlDocument domNew = new XmlDocument();

                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strExistXml) == true)
                            strExistXml = "<root />";

                        domExist.LoadXml(strExistXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    try
                    {
                        domNew.LoadXml(strRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strRecord装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 合并新旧记录
                    string strNewXml = "";

                    if (false/*bForce == false*/)      // 注: 日志记录中记载的是实际写入的记录内容，为简化处理，这里不重新进行合并了
                    {
                        // 模拟一个 SessionInfo
                        string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
        "libraryCode");
                        string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
        "operator");
                        SessionInfo temp_sessioninfo = new SessionInfo(this);
                        temp_sessioninfo.Account = new Account();
                        temp_sessioninfo.Account.AccountLibraryCode = strLibraryCode;
                        temp_sessioninfo.Account.UserID = strOperator;

                        try
                        {
                            nRet = this.IssueItemDatabase.MergeTwoItemXml(
                                temp_sessioninfo,
                                strAction,
                                domExist,
                                domNew,
                                out strNewXml,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        finally
                        {
                            temp_sessioninfo.CloseSession();
                        }
                    }
                    else
                    {
                        strNewXml = domNew.OuterXml;
                    }

                    // 保存新记录
                    byte[] output_timestamp = null;

                    if (strAction == "move")
                    {
                        int nRedoCount = 0;
                    REDO_COPY:
                        // 复制源记录到目标位置，然后自动删除源记录
                        // 但是尚未在目标位置写入最新内容
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strNewRecPath,
                            true,   // bDeleteSourceRecord
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 目标数据库不存在
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                                && robust)
                            {
                                nRet = TryCreateBiblioDatabase(
            channel,
            strNewRecPath,
            ref nRedoCount,
            out string error);
                                if (nRet == 0)
                                    goto REDO_COPY;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库和下级期库的过程出错: {error}";
                                    return -1;
                                }
                            }
                            goto ERROR1;
                        }

                        exist_timestamp = output_timestamp; // 及时更新时间戳
                    }


                    lRet = channel.DoSaveTextRes(strNewRecPath,
                        strNewXml,
                        false,   // include preamble?
                        "content,ignorechecktimestamp",
                        exist_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (strAction == "delete")
                {
                    // 和SnapShot方式相同
                    bReuse = true;
                    goto DO_SNAPSHOT;
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetIssue() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }
#endif

#if REMOVED
        // SetComments() API 恢复动作
        /* 日志记录格式
<root>
  <operation>setComment</operation> 操作类型
  <action>new</action> 具体动作。有new change delete 3种
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书评注/3'>...</record> 记录体
  <oldRecord recPath='中文图书评注/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) 一次SetComments()API调用, 可能创建多条日志记录。
         
         * */
        // TODO: 要兑现style中force nocheckdup功能
        public int RecoverSetComment(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                byte[] timestamp = null;
                //byte[] output_timestamp = null;
                //string strOutputPath = "";

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    // 写评注记录
                    nRet = TryWriteItemRecord(channel,
                        strNewRecPath,
                        strRecord,
                        "comment",
                        robust,
                        out strError);
                    if (nRet == -1)
                        return -1;
#if REMOVED
                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写评注记录
                        lRet = channel.DoSaveTextRes(strNewRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateBiblioDatabase(
        channel,
        strNewRecPath,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入评注记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }
#endif

                    if (strAction == "move")
                    {
                        // 删除评注记录
                        lRet = TryDeleteRes(
                            channel,
                            strOldRecPath,
                            timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除评注记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }

#if REMOVED
                        // 删除评注记录
                        int nRedoCount = 0;

                    REDO_DELETE:
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                return 0;   // 记录本来就不存在

                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            strError = "删除评注记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;

                        }
#endif
                    }

                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除评注记录
                    lRet = TryDeleteRes(
                        channel,
                        strRecPath,
                        timestamp,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "删除评注记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#if REMOVED
                    int nRedoCount = 0;
                REDO:
                    // 删除评注记录
                    lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO;
                            }
                        }
                        strError = "删除评注记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#endif
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }


                return 0;
            }

            bool bForce = false;
            // bool bNoCheckDup = false;

            string strStyle = DomUtil.GetElementText(domLog.DocumentElement,
                "style");

            if (StringUtil.IsInList("force", strStyle) == true)
                bForce = true;

            //if (StringUtil.IsInList("nocheckdup", strStyle) == true)
            //    bNoCheckDup = true;

            // 逻辑恢复或者混合恢复或者容错恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot
                || level == RecoverLevel.Robust*/
                IsLogic(level, out robust))    // 容错恢复没有单独实现
            {
                // 和数据库中已有记录合并，然后保存
                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }


                    // 读出数据库中原有的记录
                    string strExistXml = "";
                    string strMetaData = "";
                    byte[] exist_timestamp = null;
                    string strOutputPath = "";

                    if ((strAction == "change"
                        || strAction == "move")
                        && bForce == false)
                    {
                        string strSourceRecPath = "";

                        if (strAction == "change")
                            strSourceRecPath = strNewRecPath;
                        if (strAction == "move")
                            strSourceRecPath = strOldRecPath;

                        lRet = channel.GetRes(strSourceRecPath,
                            out strExistXml,
                            out strMetaData,
                            out exist_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 容错
                            if (channel.IsNotFoundOrDamaged()
                                && /*level == RecoverLevel.LogicAndSnapshot*/robust)
                            {
                                // 如果记录不存在, 则构造一条空的记录
                                // bExist = false;
                                strExistXml = "<root />";
                                exist_timestamp = null;
                            }
                            else
                            {
                                strError = "在读入原有记录 '" + strNewRecPath + "' 时失败: " + strError;
                                goto ERROR1;
                            }
                        }
                    }

                    //
                    // 把两个记录装入DOM

                    XmlDocument domExist = new XmlDocument();
                    XmlDocument domNew = new XmlDocument();

                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strExistXml) == true)
                            strExistXml = "<root />";

                        domExist.LoadXml(strExistXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    try
                    {
                        domNew.LoadXml(strRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strRecord装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 合并新旧记录
                    string strNewXml = "";

                    if (false/*bForce == false*/)   // 注: 因为模拟读者身份 sessioninfo 和合并处理有一定麻烦，所以这里都当作 force 处理
                    {
                        // 模拟一个 SessionInfo
                        string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
        "libraryCode");
                        string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
        "operator");
                        SessionInfo temp_sessioninfo = new SessionInfo(this);
                        temp_sessioninfo.Account = new Account();
                        temp_sessioninfo.Account.AccountLibraryCode = strLibraryCode;
                        temp_sessioninfo.Account.UserID = strOperator;

                        try
                        {
                            // TODO: 按理说日志中应该是当时操作时最终写入的内容，恢复的时候就不用经过合并这一步了
                            nRet = this.CommentItemDatabase.MergeTwoItemXml(
                                temp_sessioninfo,
                                strAction,
                                domExist,
                                domNew,
                                out strNewXml,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        finally
                        {
                            temp_sessioninfo.CloseSession();
                        }
                    }
                    else
                    {
                        strNewXml = domNew.OuterXml;
                    }

                    // 保存新记录
                    byte[] output_timestamp = null;

                    if (strAction == "move")
                    {
                        int nRedoCount = 0;
                    REDO_COPY:
                        // 复制源记录到目标位置，然后自动删除源记录
                        // 但是尚未在目标位置写入最新内容
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strNewRecPath,
                            true,   // bDeleteSourceRecord
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 目标数据库不存在
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                                && robust)
                            {
                                nRet = TryCreateBiblioDatabase(
            channel,
            strNewRecPath,
            ref nRedoCount,
            out string error);
                                if (nRet == 0)
                                    goto REDO_COPY;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strNewRecPath}' 临时决定创建书目库和下级评注库的过程出错: {error}";
                                    return -1;
                                }
                            }
                            goto ERROR1;
                        }

                        exist_timestamp = output_timestamp; // 及时更新时间戳
                    }


                    lRet = channel.DoSaveTextRes(strNewRecPath,
                        strNewXml,
                        false,   // include preamble?
                        "content,ignorechecktimestamp",
                        exist_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else if (strAction == "delete")
                {
                    // 和SnapShot方式相同
                    bReuse = true;
                    goto DO_SNAPSHOT;
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetComment() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }
#endif

        // 读出实体记录中的<barcode>元素值
        static int GetItemBarcode(string strXml,
            out string strItemBarcode,
            out string strError)
        {
            strItemBarcode = "";
            strError = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

            return 1;
        }

        // ChangeReaderPassword() API 恢复动作
        /*
<root>
  <operation>changeReaderPassword</operation> 
  <readerBarcode>...</readerBarcode>	读者证条码号
  <newPassword>5npAUJ67/y3aOvdC0r+Dj7SeXGE=</newPassword> 
  <type>...</type>  密码 Hash 算法类型。2024/5/21 增加。此前的日志记录中没有这个元素，恢复的时候 password 元素缺 type 属性，可能会出现故障
  <expire>...</expire>  失效时间
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>
注: 2019/4/25 以前的代码存在 bug，少写入了 readerBarcode 和 newPassword 元素。但此二元素可以从 readerRecord 里面找出来
         * */
        public int RecoverChangeReaderPassword(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                string strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<readerRecord>元素";
                    return -1;
                }
                string strReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 写读者记录
                nRet = TryWriteReaderRecord(
                    channel,
                    strReaderRecPath,
                    strReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                byte[] timestamp = null;

                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写读者记录
                    lRet = channel.DoSaveTextRes(strReaderRecPath,
    strReaderXml,
    false,
    "content,ignorechecktimestamp",
    timestamp,
    out byte[] output_timestamp,
    out string strOutputPath,
    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {

                // 读出原有读者记录，修改密码后存回
                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
    "readerRefID");
                var ret = TryGetUnionRefID(strReaderBarcode,
    strReaderRefID,
    out string strReaderKey);
                if (ret == false)
                {
                    strError = "日志记录中 readerBarcode 元素值为空，并且 readerRefID 元素值也为空";
                    goto ERROR1;
                }
                /*
                if (String.IsNullOrEmpty(strReaderBarcode) == true
                    && string.IsNullOrEmpty(strReaderRefID) == true)
                {
                    strError = "日志记录中同时缺乏 readerBarcode 和 readerRefID 元素";
                    goto ERROR1;
                }
                */

                string strNewPassword = DomUtil.GetElementText(domLog.DocumentElement,
                    "newPassword");
                if (String.IsNullOrEmpty(strNewPassword) == true)
                {
                    strError = "日志记录中缺乏 <newPassword> 元素";
                    goto ERROR1;
                }

                string type = DomUtil.GetElementText(domLog.DocumentElement,
                    "type");
                string expire = DomUtil.GetElementText(domLog.DocumentElement,
                    "expire");

                // 读入读者记录
                nRet = this.GetReaderRecXml(
                    channel,
                    strReaderKey,   // strReaderBarcode,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入证条码号为 '" + strReaderKey + "' 的读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // strNewPassword中本来就是 Hash 形态
                var element = DomUtil.SetElementText(
                    readerdom.DocumentElement,
                    "password",
                    strNewPassword);

                // 2024/5/21
                if (string.IsNullOrEmpty(type) == false)
                    element.SetAttribute("type", type);
                if (string.IsNullOrEmpty(expire) == false)
                    element.SetAttribute("expire", type);

                // 写回读者记录
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }


            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverChangeReaderPassword() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        // SetReaderInfo() API 恢复动作
        /*
<root>
	<operation>setReaderInfo</operation> 操作类型
	<action>...</action> 具体动作。有new change delete move 4种
	<record recPath='...'>...</record> 新记录
    <oldRecord recPath='...'>...</oldRecord> 被覆盖或者删除的记录 动作为 change 和 delete 时具备此元素(2024/5/25 动作为 new 时也可能会具备此元素)
    <changedEntityRecord itemBarcode='...' recPath='...' oldBorrower='...' newBorrower='...' /> 若干个元素。表示连带发生修改的册记录
	<operator>test</operator> 操作者
	<operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 操作时间
</root>

注: new 的时候只有<record>元素，delete的时候只有<oldRecord>元素，change的时候两者都有

         * */
        public int RecoverSetReaderInfo(
            RmsChannelCollection Channels,
            RecoverLevel level_param,
            XmlDocument domLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            string[] element_names = GetReaderFullElementNames();
            // string[] element_names = _reader_element_names;

            RecoverLevel level = level_param;

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                /*|| bReuse == true*/)
            {
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                if (strAction == "new"
                    || strAction == "change")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 写读者记录
                    nRet = TryWriteReaderRecord(
                        channel,
                        strRecPath,
                        strRecord,
                        robust,
                        func_warning,
                        out strError);
                    if (nRet == -1)
                        return -1;
#if REMOVED
                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写读者记录
                        lRet = channel.DoSaveTextRes(strRecPath,
        strRecord,
        false,
        "content,ignorechecktimestamp",
        timestamp,
        out output_timestamp,
        out strOutputPath,
        out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateReaderDatabase(
        channel,
        strRecPath,
        func_warning,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strRecPath}' 临时决定创建读者库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }

#endif

                    // 2015/9/11
                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("changedEntityRecord");
                    foreach (XmlElement item in nodes)
                    {
                        string strItemBarcode = item.GetAttribute("itemBarcode");
                        string strItemRecPath = item.GetAttribute("recPath");
                        string strOldReaderBarcode = item.GetAttribute("oldBorrower");
                        string strNewReaderBarcode = item.GetAttribute("newBorrower");

                        List<string> oldBorrowers = new List<string>();
                        if (string.IsNullOrEmpty(strOldReaderBarcode) == false)
                            oldBorrowers.Add(strOldReaderBarcode);
                        /*
                        if (string.IsNullOrEmpty(strOldReaderRefID) == false)
                            oldBorrowers.Add($"@refID:{strOldReaderRefID}");
                        */

                        // 修改一条册记录，的 borrower 元素内容
                        // return:
                        //      -2  保存记录时出错
                        //      -1  一般性错误
                        //      0   没有必要修改
                        //      1   成功
                        nRet = ChangeBorrower(
                            channel,
                            strItemBarcode,
                            strItemRecPath,
                            oldBorrowers,    // strOldReaderBarcode,
                            strNewReaderBarcode,
                            true,
                            out bool notfound,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                        {
                            if (notfound && robust)
                            {
                                func_warning?.Invoke($"尝试修改相关册记录 '{strItemRecPath}' 时发现记录不存在");
                            }
                            else
                            {
                                strError = "修改读者记录所关联的在借册记录时出错：" + strError;
                                return -1;
                            }
                        }
                    }
                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除读者记录
                    lRet = TryDeleteRes(
                        channel,
                        strRecPath,
                        timestamp,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "删除读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#if REMOVED
                    int nRedoCount = 0;
                REDO:
                    // 删除读者记录
                    lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO;
                            }
                        }
                        strError = "删除读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;

                    }
#endif
                }
                else if (strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
    "record",
    out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");
                    if (string.IsNullOrEmpty(strRecPath) == true)
                    {
                        strError = "日志记录中<record>元素内缺recPath属性值";
                        return -1;
                    }

                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    if (string.IsNullOrEmpty(strOldRecPath) == true)
                    {
                        strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                        return -1;
                    }
                    int nRedoCount = 0;
                REDO_COPY:
                    // 移动读者记录
                    lRet = channel.DoCopyRecord(
                        strOldRecPath,
                        strRecPath,
                        true,
                        "file_reserve_source",  // 2024/4/28
                        out _,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // 源记录本来就不存在。进行容错处理
                        if (channel.IsNotFoundOrDamaged()
                            && /*level_param == RecoverLevel.Robust*/robust)
                        {
                            // 优先用最新的记录内容复原。实在没有才用旧的记录内容
                            if (string.IsNullOrEmpty(strRecord) == true)
                                strRecord = strOldRecord;

                            if (string.IsNullOrEmpty(strRecord) == false)
                            {
                                // 写读者记录
                                // 注: 这里具有容错功能，比如目标库不存在会尝试自动创建
                                nRet = TryWriteReaderRecord(
    channel,
    strRecPath,
    strRecord,
    robust,
    func_warning,
    out strError);
                                if (nRet == -1)
                                {
                                    strError = "为容错，写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                                    return -1;
                                }
#if REMOVED
                                lRet = channel.DoSaveTextRes(strRecPath,
                    strRecord,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                                if (lRet == -1)
                                {
                                    strError = "为容错，写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                                    return -1;
                                }
#endif


                                return 0;
                            }
                        }
                        // 目标数据库不存在
                        else if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                            && robust)
                        {
                            nRet = TryCreateReaderDatabase(
        channel,
        strRecPath,
        func_warning,
        ref nRedoCount,
        out string error);
                            if (nRet == 0)
                                goto REDO_COPY;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "移动读者记录 '" + strOldRecPath + "' 到 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

                    // 注: 只用 DoCopyRecord()，如果数据库中源记录和 oldRecord 中的不一致，执行后效果
                    // 和快照模式的要求不一致，似还应补一次将 oldRecord 写入目标位置的动作。
                    // 原本使用 DoCopyRecord() API 的目的就是带着对象移动，那么后面补一次将 oldRecord 写入目标位置的时候，就可能破坏这些对象，因为两个不同元数据内容可能差异很大

                    // <record>中如果有记录体，则还需要写入一次
                    // 所以这里需要注意，在创建日志记录的时候，如果没有在CopyRecord()后追加修改过记录，则不要创建<record>记录正文部分，以免引起多余的日志恢复时写入动作
                    if (string.IsNullOrEmpty(strRecord) == false)
                    {
                        // 注: 这里具有容错功能，比如目标库不存在会尝试自动创建
                        nRet = TryWriteReaderRecord(
channel,
strRecPath,
strRecord,
robust,
func_warning,
out strError);
                        if (nRet == -1)
                        {
                            strError = "写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        lRet = channel.DoSaveTextRes(strRecPath,
            strRecord,
            false,
            "content,ignorechecktimestamp",
            timestamp,
            out output_timestamp,
            out strOutputPath,
            out strError);
                        if (lRet == -1)
                        {
                            strError = "写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#endif

                    }
                }

                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // 和数据库中已有记录合并，然后保存
                if (strAction == "new" || strAction == "change")
                {
                    string strOldRecord = "";
                    if (strAction == "change")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
    "oldRecord",
    out XmlNode oldrecord_node);
                        if (oldrecord_node == null)
                        {
                            strError = "日志记录中缺 <oldRecord> 元素，无法完成 change 动作";
                            return -1;
                        }
                        // 2024/3/21
                        // 从 strOldRecord 中取出 ReaderKey。可能是参考ID，也有可能是证条码号
                        var strOldReaderKey = GetXmlReaderKey(strOldRecord);

                        string strOldRecPath = DomUtil.GetAttr(oldrecord_node, "recPath");
                        if (string.IsNullOrEmpty(strOldReaderKey))
                            strOldReaderKey = $"@path:{strOldRecPath}";

                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetReaderRecXml(
                            channel,
                            strOldReaderKey,
                            out string old_xml,
                            out string old_recpath,
                            out byte[] old_timestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"探测原记录 '{strOldReaderKey}' 的过程出错: {strError}";
                            return -1;
                        }
                        if (nRet == 0)
                        {
                            if (robust == false)
                            {
                                strError = $"原记录 '{strOldReaderKey}' 不存在";
                                return -1;
                            }

                            // 继续向后处理
                        }
                        else
                            strOldRecPath = old_recpath;
                    }

                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }
                    // 2024/3/20
                    // 从 strRecord 中取出 ReaderKey。可能是参考ID，也有可能是证条码号
                    var strReaderKey = GetXmlReaderKey(strRecord);

                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 读出数据库中原有的记录
                    string strExistXml = "";
                    // string strMetaData = "";
                    byte[] exist_timestamp = null;
                    string strOutputPath = "";

                    // if (strAction == "change")
                    {
                        {
                            List<string> queries = new List<string>();
                            if (string.IsNullOrEmpty(strReaderKey) == false)
                                queries.Add(strReaderKey);
                            queries.Add(strRecPath);
                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   命中 1 条
                            //      >1  命中多于 1 条
                            nRet = TryLoadReaderRecord(
    channel,
    queries,
    out strExistXml,
    out exist_timestamp,
    out strOutputPath,
    out strError);
                            if (nRet == 1)
                            {

                            }
                            else
                            {
                                // 容错
                                if (nRet == 0/*
                                    && robust*/)
                                {
                                    // 如果记录不存在, 则构造一条空的记录
                                    // bExist = false;
                                    strExistXml = "<root />";
                                    exist_timestamp = null;
                                }
                                else
                                {
                                    strError = $"在读入原有记录 '{strReaderKey}','{strRecPath}' 时失败: " + strError;
                                    goto ERROR1;
                                }
                            }
                        }

#if REMOVED
                        {
                            lRet = channel.GetRes(strRecPath,
                                out strExistXml,
                                out strMetaData,
                                out exist_timestamp,
                                out strOutputPath,
                                out strError);
                            if (lRet == -1)
                            {
                                // 容错
                                if (channel.IsNotFoundOrDamaged()
                                    && /*level == RecoverLevel.LogicAndSnapshot*/robust)
                                {
                                    // 如果记录不存在, 则构造一条空的记录
                                    // bExist = false;
                                    strExistXml = "<root />";
                                    exist_timestamp = null;
                                }
                                else
                                {
                                    strError = "在读入原有记录 '" + strRecPath + "' 时失败: " + strError;
                                    goto ERROR1;
                                }
                            }
                        }
#endif
                    }

                    if (string.IsNullOrEmpty(strOutputPath))
                        strOutputPath = GetLogicRecPath(channel, strRecPath);

                    //
                    // 把两个记录装入DOM

                    XmlDocument domExist = new XmlDocument();
                    XmlDocument domNew = new XmlDocument();

                    try
                    {
                        // 防范空记录
                        if (String.IsNullOrEmpty(strOldRecord) == true)
                            strOldRecord = "<root />";

                        domExist.LoadXml(strOldRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strOldRecord 装载进入 DOM 时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    try
                    {
                        domNew.LoadXml(strRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "strRecord 装载进入 DOM 时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 观察 domNew 中是否有 refID 元素
                    var refID1 = DomUtil.GetElementText(domNew.DocumentElement,
                        "refID");

                    // 合并新旧记录
                    // string strNewXml = "";
                    // parameters:
                    //      important_fields    重要的字段名列表。要检查这些字段是否没有被采纳，如果没有被采纳要报错
                    //                          注: dprms:file 元素，在 important_fields 里面应当表达为 "http://dp2003.com/dprms:file"
                    //      strRights           当前账户权限。
                    //                          用于检查账户权限问题。如果不希望检查，可以用 null
                    //      denied_element_names    [out] 返回哪些被阻止修改的元素名集合。注意，如果本函数因为 importantFields 原因返回 -1 了，则 denied_element_names 不会返回任何内容
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   成功，并且 refID 元素是利用上了 domNew 里面的 refID 元素
                    nRet = MergeTwoReaderXml(
                        element_names,
                        "change",
                        domExist,
                        domNew,
                        null,
                        null,
                        // out strNewXml,
                        out XmlDocument domMerged,
                        out _,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 观察 domNew 中是否有 refID 元素
                    var refID2 = DomUtil.GetElementText(domMerged.DocumentElement,
                        "refID");
                    // 2024/2/26
                    // 如果合并后增加了随机的参考 ID，会影响到和后继日志动作的准确对应，所以这里决定删除掉参考 ID 元素
                    // 注1: 1.10 以前的操作日志记录 setReaderInfo new 类型存在 bug，所记载的 record 中，没有记载下来实际已经创建和写入到读者记录中的参考 ID 元素
                    // 注2: 后续如果遇到版本升级，升级过程会给读者记录统一添加参考 ID
                    if (string.IsNullOrEmpty(refID2) == false
                        && string.IsNullOrEmpty(refID1) == true)
                        DomUtil.DeleteElement(domMerged.DocumentElement,
                            "refID");

                    // 保存新记录
                    lRet = channel.DoSaveTextRes(strOutputPath, // strRecPath,
                        domMerged.OuterXml, // strNewXml,
                        false,   // include preamble?
                        "content,ignorechecktimestamp",
                        exist_timestamp,
                        out byte[] output_timestamp,
                        out _,  // strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }
                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
    "oldRecord",
    out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺 <oldRecord> 元素，无法完成 delete 动作";
                        return -1;
                    }
                    // 2024/3/21
                    // 从 strOldRecord 中取出 ReaderKey。可能是参考ID，也有可能是证条码号
                    var strOldReaderKey = GetXmlReaderKey(strOldRecord);

                    string strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    if (string.IsNullOrEmpty(strOldReaderKey))
                        strOldReaderKey = $"@path:{strOldRecPath}";

                    nRet = DeleteReaderRecord(channel,
                        strOldReaderKey,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (nRet == -1)
                    {
                        strError = $"删除读者记录 '{strOldReaderKey}' 时出错: {strError}";
                        return -1;
                    }

                    /*
                    // 和SnapShot方式相同
                    bReuse = true;
                    goto DO_SNAPSHOT;
                    */
                }
                else if (strAction == "move")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
"oldRecord",
out XmlNode oldrecord_node);
                    if (oldrecord_node == null)
                    {
                        strError = "日志记录中缺 <oldRecord> 元素，无法完成 move 动作";
                        return -1;
                    }
                    // 2024/3/21
                    // 从 strOldRecord 中取出 ReaderKey。可能是参考ID，也有可能是证条码号
                    var strOldReaderKey = GetXmlReaderKey(strOldRecord);

                    string strOldRecPath = DomUtil.GetAttr(oldrecord_node, "recPath");
                    if (string.IsNullOrEmpty(strOldReaderKey))
                        strOldReaderKey = $"@path:{strOldRecPath}";

                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetReaderRecXml(
                        channel,
                        strOldReaderKey,
                        out string old_xml,
                        out string old_recpath,
                        out byte[] old_timestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = $"探测源记录 '{strOldReaderKey}' 的过程出错: {strError}";
                        return -1;
                    }
                    if (nRet == 0)
                    {
                        if (robust == false)
                        {
                            strError = $"源记录 '{strOldReaderKey}' 不存在";
                            return -1;
                        }

                        // 继续向后处理
                    }
                    else
                        strOldRecPath = old_recpath;

                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
"record",
out XmlNode record_node);
                    if (record_node == null)
                    {
                        strError = "日志记录中缺 <record> 元素，无法完成 move 动作";
                        return -1;
                    }
                    // 2024/3/21
                    // 从 strRecord 中取出 ReaderKey。可能是参考ID，也有可能是证条码号
                    var strReaderKey = GetXmlReaderKey(strRecord);

                    string strRecPath = DomUtil.GetAttr(record_node, "recPath");
                    if (string.IsNullOrEmpty(strReaderKey))
                        strReaderKey = $"@path:{strRecPath}";

                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetReaderRecXml(
                        channel,
                        strReaderKey,
                        out string xml,
                        out string recpath,
                        out byte[] timestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = $"探测目标记录 '{strReaderKey}' 的过程出错: {strError}";
                        return -1;
                    }
                    if (nRet == 0)
                    {
                        /*
                        strError = $"目标记录 '{strReaderKey}' 不存在";
                        return -1;
                        */
                    }
                    else
                        strRecPath = recpath;

                    int nRedoCount = 0;
                REDO_COPY:
                    // 移动读者记录
                    lRet = channel.DoCopyRecord(
                        strOldRecPath,
                        strRecPath,
                        true,
                        "file_reserve_source",  // 2024/4/28
                        out _,
                        out byte[] output_timestamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // 源记录本来就不存在。进行容错处理
                        if (channel.IsNotFoundOrDamaged()
                            && robust)
                        {
                            // 优先用最新的记录内容复原。实在没有才用旧的记录内容
                            if (string.IsNullOrEmpty(strRecord) == true)
                                strRecord = strOldRecord;

                            if (string.IsNullOrEmpty(strRecord) == false)
                            {
                                // 写读者记录
                                nRet = TryWriteReaderRecord(
                                    channel,
                                    strRecPath,
                                    strRecord,
                                    robust,
                                    func_warning,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "为容错，写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                                    return -1;
                                }
#if REMOVED
                                lRet = channel.DoSaveTextRes(strRecPath,
                    strRecord,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                                if (lRet == -1)
                                {
                                    strError = "为容错，写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                                    return -1;
                                }
#endif

                                return 0;
                            }
                        }
                        // 目标数据库不存在
                        else if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                            && robust)
                        {
                            nRet = TryCreateReaderDatabase(
        channel,
        strRecPath,
        func_warning,
        ref nRedoCount,
        out string error);
                            if (nRet == 0)
                                goto REDO_COPY;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }


                        strError = "移动读者记录 '" + strOldRecPath + "' 到 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

                    // <record>中如果有记录体，则还需要写入一次
                    // 所以这里需要注意，在创建日志记录的时候，如果没有在CopyRecord()后追加修改过记录，则不要创建<record>记录正文部分，以免引起多余的日志恢复时写入动作
                    if (string.IsNullOrEmpty(strRecord) == false)
                    {
                        nRet = TryWriteReaderRecord(
    channel,
    strRecPath,
    strRecord,
    robust,
    func_warning,
    out strError);
                        if (nRet == -1)
                        {
                            strError = "写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        lRet = channel.DoSaveTextRes(strRecPath,
            strRecord,
            false,
            "content,ignorechecktimestamp",
            timestamp,
            out output_timestamp,
            out strOutputPath,
            out strError);
                        if (lRet == -1)
                        {
                            strError = "写入读者记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#endif
                    }


                    /*
                    // 和SnapShot方式相同
                    bReuse = true;
                    goto DO_SNAPSHOT;
                    */
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetReaderInfo() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        static string GetLogicRecPath(
            RmsChannel channel,
            string ref_recpath)
        {
            // 先探测 ref_recpath 位置的记录是否已经存在
            long lRet = channel.GetRes(ref_recpath,
                0,
                1,
                "content,data",
                out _,
                out _,
                out string output_recpath,
                out _,
                out string strError);
            if (lRet == -1 && channel.IsNotFound())
                return ref_recpath;

            return ResPath.GetDbName(ref_recpath) + "/?";
        }

        // Amerce() API 恢复动作
        /*
<root>
  <operation>amerce</operation> 操作类型
  <action>amerce</action> 具体动作。有amerce undo modifyprice
  <readerBarcode>...</readerBarcode> 读者证条码号
  <!-- <idList>...<idList> ID列表，逗号间隔 已废止 -->
  <amerceItems>
	<amerceItem id="..." newPrice="..." newComment="..." /> newComment中内容追加或替换原来的注释内容。到底是追加还是覆盖，取决于第一个字符是否为'>'还是'<'，前者为追加(这时第一个字符不被当作内容)。如果第一个字符不是这两者之一，则默认为追加
	...
  </amerceItems>
  <amerceRecord recPath='...'><root><itemBarcode>0000001</itemBarcode><readerBarcode>R0000002</readerBarcode><state>amerced</state><id>632958375041543888-1</id><over>31day</over><borrowDate>Sat, 07 Oct 2006 09:04:28 GMT</borrowDate><borrowPeriod>30day</borrowPeriod><returnDate>Thu, 07 Dec 2006 09:04:27 GMT</returnDate><returnOperator>test</returnOperator></root></amerceRecord> 在罚款库中创建的新记录。注意<amerceRecord>元素可以重复。<amerceRecord>元素内容里面的<itemBarcode><readerBarcode><id>等具备了足够的信息。
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 10:09:36 GMT</operTime> 操作时间
  
         * 早期缺乏 oldReaderRecord 元素，无法计算 修改了金额情况下 新旧金额之间的差值
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>

<root>
  <operation>amerce</operation> 
  <action>undo</action> 
  <readerBarcode>...</readerBarcode> 读者证条码号
  <!-- <idList>...<idList> ID列表，逗号间隔 已废止 -->
  <amerceItems>
	<amerceItem id="..." newPrice="..."/>
	...
  </amerceItems>
  <amerceRecord recPath='...'><root><itemBarcode>0000001</itemBarcode><readerBarcode>R0000002</readerBarcode><state>amerced</state><id>632958375041543888-1</id><over>31day</over><borrowDate>Sat, 07 Oct 2006 09:04:28 GMT</borrowDate><borrowPeriod>30day</borrowPeriod><returnDate>Thu, 07 Dec 2006 09:04:27 GMT</returnDate><returnOperator>test</returnOperator></root></amerceRecord> Undo所去掉的罚款库记录
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录

</root>

<root>
  <operation>amerce</operation> 
  <action>modifyprice</action> 
  <readerBarcode>...</readerBarcode> 读者证条码号
  <amerceItems>
	<amerceItem id="..." newPrice="..." newComment="..."/> newComment中内容追加或替换原来的注释内容。到底是追加还是覆盖，取决于第一个字符是否为'>'还是'<'，前者为追加(这时第一个字符不被当作内容)。如果第一个字符不是这两者之一，则默认为追加
	...
  </amerceItems>
  <!-- modifyprice操作时不产生<amerceRecord>元素 -->
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
  
  <oldReaderRecord recPath='...'>...</oldReaderRecord>	操作前旧的读者记录。<oldReaderRecord>元素是modifyprice操作时特有的元素
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>

2007/12/18
<root>
  <operation>amerce</operation> 操作类型
  <action>expire</action> 以停代金到期
  <readerBarcode>...</readerBarcode> 读者证条码号
  <expiredOverdues> 已经到期的若干<overdue>元素
	<overdue ... />
	...
  </expiredOverdues>
  <operator>test</operator> 操作者 如果为#readersMonitor，表示为后台线程
  <operTime>Fri, 08 Dec 2006 10:09:36 GMT</operTime> 操作时间
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>
         * 
2008/6/20
<root>
  <operation>amerce</operation> 
  <action>modifycomment</action> 
  <readerBarcode>...</readerBarcode> 读者证条码号
  <amerceItems>
	<amerceItem id="..." newComment="..."/> newComment中内容追加或替换原来的注释内容。到底是追加还是覆盖，取决于第一个字符是否为'>'还是'<'，前者为追加(这时第一个字符不被当作内容)。如果第一个字符不是这两者之一，则默认为追加
	...
  </amerceItems>
  <!-- modifycomment操作时不产生<amerceRecord>元素 -->
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
  
  <oldReaderRecord recPath='...'>...</oldReaderRecord>	操作前旧的读者记录。<oldReaderRecord>元素是modifycomment操作时特有的元素
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>

         * * 
         * */
        public int RecoverAmerce(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                if (strAction == "amerce")
                {
                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("amerceRecord");

                    int nErrorCount = 0;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        string strRecord = node.InnerText;
                        string strRecPath = DomUtil.GetAttr(node, "recPath");

                        int nRedoCount = 0;
                    // 写违约金记录
                    REDO_WRITE:
                        lRet = channel.DoSaveTextRes(strRecPath,
            strRecord,
            false,
            "content,ignorechecktimestamp",
            timestamp,
            out output_timestamp,
            out strOutputPath,
            out string strError0);
                        if (lRet == -1)
                        {
                            // 2024/3/20
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
&& robust == true)
                            {
                                // return:
                                //      -2  nRedoCount 达到或者超过 2
                                //      -1  出错
                                //      0   成功。nRedoCount 被增量
                                nRet = TryCreateAmerceDatabase(
                                    channel,
                                    strRecPath,
                                    ref nRedoCount,
                                    out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else
                                {
                                    strError = $"根据记录路径 '{strRecPath}' 临时决定创建违约金库的过程出错: {error}";
                                    return -1;
                                }
#if REMOVED
                                var dbName = ResPath.GetDbName(strRecPath);
                                // return:
                                //      -1  出错
                                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                                //      1   成功创建
                                nRet = CreateAmerceDatabase(channel,
                                    dbName,
                                    out _,
                                    out string error);
                                if (nRet == 1)
                                    goto REDO_WRITE;
                                else
                                {
                                    strError = $"根据记录路径 '{strRecPath}' 临时决定创建违约金库的过程出错: {error}";
                                    return -1;
                                }
#endif
                            }

                            // 继续循环
                            if (strError != "")
                                strError += "\r\n";
                            strError += "写入违约金记录 '" + strRecPath + "' 时发生错误: " + strError0;
                            nErrorCount++;
                        }
                    }

                    if (nErrorCount > 0)
                        return -1;
                }
                else if (strAction == "undo")
                {
                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("amerceRecord");

                    int nErrorCount = 0;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        string strRecPath = DomUtil.GetAttr(node, "recPath");

                        // 删除违约金记录
                        lRet = TryDeleteRes(
                            channel,
                            strRecPath,
                            timestamp,
                            robust ? "" : "notExistingAsError",
                            out string strError0);
                        if (lRet == -1)
                        {
                            // 继续循环
                            if (strError != "")
                                strError += "\r\n";
                            strError += "删除违约金记录 '" + strRecPath + "' 时发生错误: " + strError0;
                            nErrorCount++;
                        }
#if REMOVED
                        int nRedoCount = 0;
                        string strError0 = "";
                    REDO:
                        // 删除违约金记录
                        lRet = channel.DoDeleteRes(strRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError0);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                continue;   // 记录本来就不存在
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO;
                                }
                            }

                            // 继续循环
                            if (strError != "")
                                strError += "\r\n";
                            strError += "删除违约金记录 '" + strRecPath + "' 时发生错误: " + strError0;
                            nErrorCount++;
                        }
#endif
                    } // end of for

                    if (nErrorCount > 0)
                        return -1;
                }
                else if (strAction == "modifyprice")
                {
                    // 这里什么都不作，只等后面用快照的读者记录来恢复
                }
                else if (strAction == "expire")
                {
                    // 这里什么都不作，只等后面用快照的读者记录来恢复

                }
                else if (strAction == "modifycomment")
                {
                    // 这里什么都不作，只等后面用快照的读者记录来恢复
                }
                else if (strAction == "appendcomment")
                {
                    // 这里什么都不作，只等后面用快照的读者记录来恢复
                }
                else
                {
                    strError = "未知的<action>类型: " + strAction;
                    return -1;
                }

                {
                    // 准备读者记录
                    string strReaderRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerRecord",
                        out XmlNode node);
                    string strReaderRecPath = DomUtil.GetAttr(node, "recPath");

                    // 写读者记录
                    nRet = TryWriteReaderRecord(
                        channel,
                        strReaderRecPath,
                        strReaderRecord,
                        robust,
                        func_warning,
                        out strError);
                    if (nRet == -1)
                        return -1;
#if REMOVED
                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写读者记录
                        lRet = channel.DoSaveTextRes(strReaderRecPath,
        strReaderRecord,
        false,
        "content,ignorechecktimestamp",
        timestamp,
        out output_timestamp,
        out strOutputPath,
        out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateReaderDatabase(
        channel,
        strReaderRecPath,
        func_warning,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }

#endif
                }

                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    strError = "日志记录中缺乏<readerBarcode>元素";
                    return -1;
                }
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
    "readerRefID");
                var ret = TryGetUnionRefID(strReaderBarcode,
strReaderRefID,
out string strReaderKey);
                if (ret == false)
                {
                    strError = "readerBarcode 元素值为空，并且 readerRefID 元素值也为空";
                    goto ERROR1;
                }

                string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
                    "libraryCode");

                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
                    "operator");
                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement,
                    "operTime");

                /*
                string strAmerceItemIdList = DomUtil.GetElementText(domLog.DocumentElement,
                    "idList");
                if (String.IsNullOrEmpty(strAmerceItemIdList) == true)
                {
                    strError = "日志记录中缺乏<idList>元素";
                    return -1;
                }
                 * */

                AmerceItem[] amerce_items = ReadAmerceItemList(domLog);


                // 读入读者记录
                nRet = this.GetReaderRecXml(
                    // Channels,
                    channel,
                    strReaderKey,   // strReaderBarcode,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                /*
                // 2024/2/11
                var strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                    "refID");
                */

                byte[] output_timestamp = null;
                string strOutputPath = "";

                if (strAction == "amerce")
                {
                    List<string> NotFoundIds = null;
                    List<string> Ids = null;
                    List<string> AmerceRecordXmls = null;
                    // 交违约金：在读者记录中去除所选的<overdue>元素，并且构造一批新记录准备加入违约金库
                    // return:
                    //      -1  error
                    //      0   读者dom没有变化
                    //      1   读者dom发生了变化
                    nRet = DoAmerceReaderXml(
                        null,
                        channel,
                        strLibraryCode,
                        ref readerdom,
                        strOutputReaderRecPath,
                        amerce_items,
                        strOperator,
                        strOperTime,
                        out AmerceRecordXmls,
                        out NotFoundIds,
                        out Ids,
                        out strError);
                    if (nRet == -1)
                    {
                        // 在错误信息后面增补每个id对应的amerce record
                        if (NotFoundIds != null && NotFoundIds.Count > 0)
                        {
                            strError += "。读者证条码号为 " + strReaderKey + "，日志记录中相关的AmerceRecord如下：\r\n" + GetAmerceRecordStringByID(domLog, NotFoundIds);
                        }

                        goto ERROR1;
                    }

                    // 如果有精力，可以把AmerceRecordXmls和日志记录中的<amerceRecord>逐个进行核对


                    // 写入违约金记录
                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("amerceRecord");

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        string strRecord = node.InnerText;
                        string strRecPath = DomUtil.GetAttr(node, "recPath");

                        int nRedoCount = 0;
                    REDO_WRITE:
                        // 写违约金记录
                        lRet = channel.DoSaveTextRes(strRecPath,
                            strRecord,
                            false,
                            "content,ignorechecktimestamp",
                            null,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 2024/3/20
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
&& robust == true)
                            {
                                // return:
                                //      -2  nRedoCount 达到或者超过 2
                                //      -1  出错
                                //      0   成功。nRedoCount 被增量
                                nRet = TryCreateAmerceDatabase(
                                    channel,
                                    strRecPath,
                                    ref nRedoCount,
                                    out string error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else
                                {
                                    strError = $"根据记录路径 '{strRecPath}' 临时决定创建违约金库的过程出错: {error}";
                                    return -1;
                                }
#if REMOVED
                                var dbName = ResPath.GetDbName(strRecPath);
                                // return:
                                //      -1  出错
                                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                                //      1   成功创建
                                nRet = CreateAmerceDatabase(channel,
                                    dbName,
                                    out _,
                                    out string error);
                                if (nRet == 1)
                                    goto REDO_WRITE;
                                else
                                {
                                    strError = $"根据记录路径 '{strRecPath}' 临时决定创建违约金库的过程出错: {error}";
                                    return -1;
                                }
#endif
                            }

                            strError = "写入违约金记录 '" + strRecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }
                }

                if (strAction == "undo")
                {
                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("amerceRecord");

                    // 看看根下面是否有overdues元素
                    XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
                    if (root == null)
                    {
                        root = readerdom.CreateElement("overdues");
                        readerdom.DocumentElement.AppendChild(root);
                    }

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        string strRecord = node.InnerText;
                        string strRecPath = DomUtil.GetAttr(node, "recPath");


                        // 如果有精力，可以把违约金记录中的id和日志记录<amerceItems>中的id对比检查

                        // 违约金信息加回读者记录

                        // 将违约金记录格式转换为读者记录中的<overdue>元素格式
                        // parameters:
                        //      strReaderKey    [out] 返回读者证条码号或者 @refID:xxx 形态
                        nRet = ConvertAmerceRecordToOverdueString(strRecord,
                            out string strTempReaderKey,
                            out string strOverdueString,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (string.IsNullOrEmpty(strTempReaderKey) == false)
                        {
                            if (dp2StringUtil.MatchReaderKey(strTempReaderKey,
                                strReaderBarcode,
                                strReaderRefID) == false)
                            {
                                strError = "<amerceRecord>中的读者证条码号和日志记录中的<readerBarcode>读者证条码号不一致";
                                goto ERROR1;
                            }                            /*
                            if (strTempReaderKey.StartsWith("@") == false
                                && string.IsNullOrEmpty(strReaderBarcode) == false
                                && strTempReaderKey != strReaderBarcode)
                            {
                                strError = "<amerceRecord>中的读者证条码号和日志记录中的<readerBarcode>读者证条码号不一致";
                                goto ERROR1;
                            }
                            else if (strTempReaderKey.StartsWith("@refID:") == true
    && string.IsNullOrEmpty(strReaderRefID) == false
    && strTempReaderKey != $"@refID:{strReaderRefID}")
                            {
                                strError = "<amerceRecord>中的读者参考 ID 和日志记录中的<readerRefID>读者参考 ID 不一致";
                                goto ERROR1;
                            }
                            */
                        }

                        XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                        fragment.InnerXml = strOverdueString;

                        // 2008/11/13 changed
                        XmlNode node_added = root.AppendChild(fragment);
                        Debug.Assert(node_added != null, "");
                        string strReason = DomUtil.GetAttr(node_added, "reason");
                        if (strReason == "押金。")
                        {
                            string strPrice = DomUtil.GetAttr(node_added, "price");

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
                                // bReaderDomChanged = true;
                            }
                        }

                        // 删除违约金记录
                        lRet = TryDeleteRes(
                            channel,
                            strRecPath,
                            null,    // timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            // 是否需要继续循环？
                            strError = "删除违约金记录 '" + strRecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }
#if REMOVED
                        // 删除违约金记录
                        int nRedoCount = 0;
                        byte[] timestamp = null;
                    REDO:
                        // 删除违约金记录
                        lRet = channel.DoDeleteRes(strRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                continue;   // 记录本来就不存在
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO;
                                }
                            }

                            // 是否需要继续循环？
                            strError = "删除违约金记录 '" + strRecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }
#endif
                    }

                }

                if (strAction == "modifyprice")
                {
                    nRet = ModifyPrice(ref readerdom,
                        amerce_items,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ModifyPrice()时发生错误: " + strError;
                        goto ERROR1;
                    }
                }

                // 2008/6/20
                if (strAction == "modifycomment")
                {
                    nRet = ModifyComment(
                        ref readerdom,
                        amerce_items,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ModifyComment()时发生错误: " + strError;
                        goto ERROR1;
                    }
                }

                if (strAction == "expire")
                {
                    // 寻找<expiredOverdues/overdue>元素
                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("//expiredOverdues/overdue");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        string strID = DomUtil.GetAttr(node, "id");

                        if (String.IsNullOrEmpty(strID) == true)
                            continue;

                        // 从读者记录中去掉这个id的<overdue>元素
                        XmlNode nodeOverdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + strID + "']");
                        if (nodeOverdue != null)
                        {
                            if (nodeOverdue.ParentNode != null)
                                nodeOverdue.ParentNode.RemoveChild(nodeOverdue);
                        }
                    }
                }

                // 写回读者记录
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
                {
                    goto ERROR1;
                }
            }

            return 0;

        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverAmerce() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        // 获得和指定id相关的AmerceRecord
        static string GetAmerceRecordStringByID(XmlDocument domLog,
            List<string> NotFoundIds)
        {
            string strResult = "";

            List<string> records = new List<string>();
            List<string> ids = new List<string>();
            XmlNodeList nodes = domLog.DocumentElement.SelectNodes("amerceRecord");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strRecord = nodes[i].InnerText;
                if (String.IsNullOrEmpty(strRecord) == true)
                    continue;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strRecord);
                }
                catch (Exception ex)
                {
                    strResult += "XML字符串装入DOM时发生错误: " + ex.Message + "\r\n";
                    continue;
                }

                records.Add(strRecord);

                string strID = DomUtil.GetElementText(dom.DocumentElement, "id");

                ids.Add(strID);
            }

            for (int i = 0; i < NotFoundIds.Count; i++)
            {
                string strID = NotFoundIds[i];
                int index = ids.IndexOf(strID);
                if (index == -1)
                {
                    strResult += "id [" + strID + "] 在日志记录中没有找到对应的<amerceRecord>元素\r\n";
                    continue;
                }

                strResult += "id: " + strID + " -- " + records[index] + "\r\n";
            }

            return strResult;
        }

        // 获得附件记录
        static int GetAttachmentRecord(
            Stream attachment,
            int nAttachmentIndex,
            out byte[] baRecord,
            out string strError)
        {
            baRecord = null;
            strError = "";

            if (attachment == null)
            {
                strError = "attachment为空";
                return -1;
            }

            if (nAttachmentIndex < 0)
            {
                strError = "nAttachmentIndex参数值必须>=0";
                return -1;
            }

            attachment.Seek(0, SeekOrigin.Begin);

            long lLength = 0;

            // 找到记录开头
            for (int i = 0; i <= nAttachmentIndex; i++)
            {
                byte[] length = new byte[8];
                int nRet = attachment.Read(length, 0, 8);
                if (nRet != 8)
                {
                    strError = "附件格式错误1";
                    return -1;
                }
                lLength = BitConverter.ToInt64(length, 0);


                if (attachment.Length - attachment.Position < lLength)
                {
                    strError = "附件格式错误2";
                    return -1;
                }

                if (i == nAttachmentIndex)
                    break;

                attachment.Seek(lLength, SeekOrigin.Current);
            }

            if (lLength >= 1000 * 1024)
            {
                strError = "附件记录长度太大，超过1000*1024，无法处理";
                return -1;
            }

            // 读入记录内容
            baRecord = new byte[(int)lLength];
            attachment.Read(baRecord, 0, (int)lLength);

            return 0;
        }


        /*
<root>
  <operation>devolveReaderInfo</operation> 
  <sourceReaderBarcode>...</sourceReaderBarcode> 源读者证条码号
  <targetReaderBarcode>...</targetReaderBarcode> 目标读者证条码号
  <borrows>...</borrows> 移动过去的<borrows>内容，下级为<borrow>元素
  <overdues>...</overdues> 移动过去的<overdue>内容，下级为<overdue>元素
  <sourceReaderRecord recPath='...'>...</sourceReaderRecord>	最新源读者记录
  <targetReaderRecord recPath='...'>...</targetReaderRecord>	最新目标读者记录
  <changedEntityRecord recPath='...' attahchmentIndex='.'>...</changedEntityRecord> 所牵连到的发生了修改的实体记录。此元素的文本即是记录体，但注意为不透明的字符串（HtmlEncoding后的记录字符串）。如果存在attachmentIndex属性，则表明实体记录不在此元素文本中，而在日志记录的附件中
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
</root>
         * * */
        public int RecoverDevolveReaderInfo(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Stream attachmentLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                /*
                // 观察是否有<warning>元素
                XmlNode nodeWarning = domLog.SelectSingleNode("warning");
                if (nodeWarning != null)
                {
                    // 如果<warning元素存在，表明只能采用逻辑恢复>
                    strError = nodeWarning.InnerText;
                    return -1;
                }
                */

                // 获源读者记录
                string strSourceReaderXml = DomUtil.GetElementText(
                    domLog.DocumentElement,
                    "sourceReaderRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<sourceReaderRecord>元素";
                    return -1;
                }
                string strSourceReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 写源读者记录
                nRet = TryWriteReaderRecord(
                    channel,
                    strSourceReaderRecPath,
                    strSourceReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                byte[] timestamp = null;
                string strOutputPath = "";
                byte[] output_timestamp = null;

                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写源读者记录
                    lRet = channel.DoSaveTextRes(strSourceReaderRecPath,
    strSourceReaderXml,
    false,
    "content,ignorechecktimestamp",
    timestamp,
    out output_timestamp,
    out strOutputPath,
    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strSourceReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strSourceReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入源读者记录 '" + strSourceReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                // 获目标读者记录
                node = null;
                string strTargetReaderXml = DomUtil.GetElementText(
                    domLog.DocumentElement,
                    "targetReaderRecord",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<targetReaderRecord>元素";
                    return -1;
                }
                string strTargetReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 写目标读者记录
                nRet = TryWriteReaderRecord(
                    channel,
                    strTargetReaderRecPath,
                    strTargetReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写目标读者记录
                    lRet = channel.DoSaveTextRes(strTargetReaderRecPath,
                    strTargetReaderXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strTargetReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strTargetReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入目标读者记录 '" + strSourceReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                // 循环，写入相关的若干实体记录
                XmlNodeList nodeEntities = domLog.DocumentElement.SelectNodes("changedEntityRecord");
                for (int i = 0; i < nodeEntities.Count; i++)
                {
                    XmlNode nodeEntity = nodeEntities[i];

                    string strItemRecPath = DomUtil.GetAttr(nodeEntity,
                        "recPath");
                    string strAttachmentIndex = DomUtil.GetAttr(nodeEntity,
                        "attachmentIndex");

                    string strItemXml = "";

                    if (String.IsNullOrEmpty(strAttachmentIndex) == true)
                    {
                        strItemXml = nodeEntity.InnerText;
                        if (String.IsNullOrEmpty(strItemXml) == true)
                        {
                            strError = "<changedEntityRecord>元素缺乏文本内容。";
                            return -1;
                        }
                    }
                    else
                    {
                        // 实体记录在附件中
                        int nAttachmentIndex = 0;
                        try
                        {
                            nAttachmentIndex = Convert.ToInt32(strAttachmentIndex);
                        }
                        catch
                        {
                            strError = "<changedEntityRecord>元素的attachmentIndex属性值'" + strAttachmentIndex + "'格式不正确，应当为>=0的纯数字";
                            return -1;
                        }

                        byte[] baItem = null;
                        nRet = GetAttachmentRecord(
                            attachmentLog,
                            nAttachmentIndex,
                            out baItem,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "获得 index 为 " + nAttachmentIndex.ToString() + " 的日志附件记录时出错：" + strError;
                            return -1;
                        }
                        strItemXml = Encoding.UTF8.GetString(baItem);
                    }

                    nRet = TryWriteItemRecord(channel,
                        strItemRecPath,
                        strItemXml,
                        "entity",
                        robust,
                        out strError);
                    if (nRet == -1)
                        return -1;
#if REMOVED
                    // 写册记录
                    lRet = channel.DoSaveTextRes(strItemRecPath,
                        strItemXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "写入册记录 '" + strItemRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#endif
                }

                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                string strOperTimeString = DomUtil.GetElementText(domLog.DocumentElement,
                    "operTime");

                string strSourceReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "sourceReaderBarcode");
                string strSourceReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "sourceReaderRefID");
                if (String.IsNullOrEmpty(strSourceReaderBarcode) == true
                    && string.IsNullOrEmpty(strSourceReaderRefID) == true)
                {
                    strError = "<sourceReaderBarcode> 元素和 <sourceReaderRefID> 值均为空";
                    goto ERROR1;
                }
                string strSourceReaderKey = dp2StringUtil.BuildReaderKey(strSourceReaderBarcode, strSourceReaderRefID);

                string strTargetReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "targetReaderBarcode");
                string strTargetReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
    "targetReaderRefID");
                if (String.IsNullOrEmpty(strTargetReaderBarcode) == true
                    && string.IsNullOrEmpty(strTargetReaderRefID) == true)
                {
                    strError = "<targetReaderBarcode> 元素和 <targetReaderRefID> 值均为空";
                    goto ERROR1;
                }
                string strTargetReaderKey = dp2StringUtil.BuildReaderKey(strTargetReaderBarcode, strTargetReaderRefID);

                // 读入源读者记录
                string strSourceReaderXml = "";
                string strSourceOutputReaderRecPath = "";
                byte[] source_reader_timestamp = null;

                nRet = this.GetReaderRecXml(
                    channel,
                    strSourceReaderKey, // strSourceReaderBarcode,
                    out strSourceReaderXml,
                    out strSourceOutputReaderRecPath,
                    out source_reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "源读者证条码号 '" + strSourceReaderKey + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入证条码号为 '" + strSourceReaderKey + "' 的源读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                nRet = LibraryApplication.LoadToDom(strSourceReaderXml,
                    out XmlDocument source_readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载源读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                //
                // 读入目标读者记录
                string strTargetReaderXml = "";
                string strTargetOutputReaderRecPath = "";
                byte[] target_reader_timestamp = null;

                nRet = this.GetReaderRecXml(
                    channel,
                    strTargetReaderKey, // strTargetReaderBarcode,
                    out strTargetReaderXml,
                    out strTargetOutputReaderRecPath,
                    out target_reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "目标读者证条码号 '" + strTargetReaderKey + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入证条码号为 '" + strTargetReaderKey + "' 的目标读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                nRet = LibraryApplication.LoadToDom(strTargetReaderXml,
                    out XmlDocument target_readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载目标读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 移动信息
                XmlDocument domTemp = null;

                {
                    Stream tempstream = null;
                    try
                    {
                        // 移动借阅信息 -- <borrows>元素内容
                        // return:
                        //      -1  error
                        //      0   not found brrowinfo
                        //      1   found and moved
                        nRet = DevolveBorrowInfo(
                            channel,
                            strSourceReaderKey, // strSourceReaderBarcode,
                            strTargetReaderKey, // strTargetReaderBarcode,
                            strOperTimeString,
                            ref source_readerdom,
                            ref target_readerdom,
                            ref domTemp,
                            "",
                            out tempstream,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        if (tempstream != null)
                            tempstream.Close();
                    }
                }

                // 移动超期违约金信息 -- <overdues>元素内容
                // return:
                //      -1  error
                //      0   not found overdueinfo
                //      1   found and moved
                nRet = DevolveOverdueInfo(
                    strSourceReaderKey, // strSourceReaderBarcode,
                    strTargetReaderKey, // strTargetReaderBarcode,
                    strOperTimeString,
                    ref source_readerdom,
                    ref target_readerdom,
                    ref domTemp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 写回源读者记录
                nRet = TryWriteReaderRecord(channel,
                    strSourceOutputReaderRecPath,
                    source_readerdom.OuterXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 写回目标读者记录
                nRet = TryWriteReaderRecord(channel,
                    strTargetOutputReaderRecPath,
                    target_readerdom.OuterXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                // 写回读者记录
                byte[] output_timestamp = null;
                string strOutputPath = "";

                // 写回源读者记录
                lRet = channel.DoSaveTextRes(strSourceOutputReaderRecPath,
                    source_readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    source_reader_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 写回目标读者记录
                lRet = channel.DoSaveTextRes(strTargetOutputReaderRecPath,
                    target_readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    source_reader_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
#endif
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverDevolveReaderInfo() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }


        // SetBiblioInfo() API 或CopyBiblioInfo() API 的恢复动作
        // 函数内，使用return -1;还是goto ERROR1; 要看错误发生的时候，是否还有价值继续探索SnapShot重试。如果是，就用后者。
        /*
<root>
  <operation>setBiblioInfo</operation> 
  <action>...</action> 具体动作 有 new/change/delete/onlydeletebiblio/onlydeletesubrecord 和 onlycopybiblio/onlymovebiblio/copy/move
  <record recPath='中文图书/3'>...</record> 记录体 动作为new/change/ *move* / *copy* 时具有此元素(即delete时没有此元素)
  <oldRecord recPath='中文图书/3'>...</oldRecord> 被覆盖、删除或者移动的记录 动作为change/ *delete* / *move* / *copy* 时具备此元素
  <deletedEntityRecords> 被删除的实体记录(容器)。只有当<action>为delete时才有这个元素。
	  <record recPath='中文图书实体/100'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。
	  ...
  </deletedEntityRecords>
  <copyEntityRecords> 被复制的实体记录(容器)。只有当<action>为*copy*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110' newBarcode='...'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径。特殊地，如果 targetRecPath 属性值为空，表示删除了 recPath 属性值这条记录。newBarcode 属性为复制到目标后应该把其 barcode 元素修改成的值。
	  ...
  </copyEntityRecords>
  <moveEntityRecords> 被移动的实体记录(容器)。只有当<action>为*move*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径。特殊地，如果 targetRecPath 属性值为空，表示删除了 recPath 属性值这条记录。
	  ...
  </moveEntityRecords>
  <copyOrderRecords /> <moveOrderRecords />
  <copyIssueRecords /> <moveIssueRecords />
  <copyCommentRecords /> <moveCommentRecords />
  <mergeStyle>...</mergeStyle> reserve_source 或者 reserve_target。缺省为 reserve_source
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
</root>

逻辑恢复delete操作的时候，检索出全部下属的实体记录删除。
快照恢复的时候，可以根据operlogdom直接删除记录了path的那些实体记录
         * */
        public int RecoverSetBiblioInfo(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            var origin_level = level;
            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                if (strAction == "new" || strAction == "change")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        goto ERROR1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    {
                        int nRedoCount = 0;
                    REDO_WRITERES:
                        // 写书目记录
                        lRet = channel.DoSaveTextRes(strRecPath,
                            strRecord,
                            false,
                            "content,ignorechecktimestamp",
                            timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
#if REMOVED
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && nRedoCount < 2)
                        {
                            // 创建一个书目库。并记载下来这是新创建的
                            string strBiblioDbName = ResPath.GetDbName(strRecPath);
                            string usage = "book";
                            if (strBiblioDbName.Contains("期刊")
                                || strBiblioDbName.ToLower().Contains("series"))
                                usage = "series";

                            // return:
                            //      -1  出错
                            //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                            //      1   成功创建
                            nRet = CreateBiblioDatabase(
                                Channels,
                                strBiblioDbName,
                                usage, // strUsage,
                                "", // strRole,
                                "unimarc",
                                "*",    // strSubTypeList,
                                "", // strStyle,
                                out string strRequestXml,
                                out strError);
                            if (nRet != 1)
                            {
                                strError = $"临时决定创建书目库 '{strBiblioDbName}' 的过程出错: {strError}";
                                return -1;
                            }
                            nRedoCount++;
                            goto REDO_WRITERES;
                        }
#endif
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                nRet = TryCreateBiblioDatabase(
        channel,
        strRecPath,
        ref nRedoCount,
        out string error);
                                if (nRet == 0)
                                    goto REDO_WRITERES;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strRecPath}' 临时决定创建书目库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "写入书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }
                }
                else if (strAction == "onlymovebiblio"
                    || strAction == "onlycopybiblio"
                    || strAction == "move"
                    || strAction == "copy")
                {
                    // 2024/3/29 注: 日志记录 1.10 版本以后，record 元素多了一个 newBiblioParam 属性，如果其值为 "(null)"，表示当初 API 请求的时候 strNewBiblioParam 实际上为空
                    // 所以 record 元素中记载的 XML 记录，实际上可能是 API 并没有请求最后覆盖目标位置，但操作日志主动记载下来的实际写入目标位置的记录 XML 内容
                    string strTargetRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        goto ERROR1;
                    }
                    string strTargetRecPath = DomUtil.GetAttr(node, "recPath");

                    /*
                    // 2024/3/29
                    // 逻辑恢复时，要使用 API 提交时候的 strTargetRecord
                    if (IsLogic(origin_level) && DomUtil.GetAttr(container_node, "newBiblioParam") == "(null)")
                        strTargetRecord = "";
                    */

                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strOldRecPath = DomUtil.GetAttr(node, "recPath");

                    string strMergeStyle = DomUtil.GetElementText(domLog.DocumentElement,
                        "mergeStyle");

                    bool bSourceExist = true;
                    // 观察源记录是否存在
                    {
                        lRet = channel.GetRes(strOldRecPath,
                            out string strXml,
                            out string strMetaData,
                            out byte[] temp_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFoundOrDamaged())
                            {
                                bSourceExist = false;
                                if (robust == false)
                                {
                                    strError = $"移动或者拷贝({strAction})记录前，发现源记录 '{strOldRecPath}' 不存在";
                                    return -1;
                                }
                            }
                        }
                    }

                    // 源记录是否要被(CopyRecord())删除掉
                    bool delete_source_record = false;

                    if (bSourceExist == true)
                    {
                        delete_source_record = (strAction == "onlymovebiblio" || strAction == "move" ? true : false);
                        // 2024/4/28
                        string copy_style = GetCopyRecordMergeStyle(strMergeStyle);

                        int nRedoCount = 0;
                    REDO_COPY:
                        // 复制书目记录
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strTargetRecPath,
                            delete_source_record,   // bDeleteSourceRecord
                            copy_style,
                            out string strIdChangeList,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // 源记录或者源数据库不存在的情况已经被 bSourceExist == true 避开了

                            // 目标数据库不存在
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundTargetDb
                                && robust)
                            {
                                nRet = TryCreateBiblioDatabase(
            channel,
            strTargetRecPath,
            ref nRedoCount,
            out string error);
                                if (nRet == 0)
                                    goto REDO_COPY;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strTargetRecPath}' 临时决定创建书目库的过程出错: {error}";
                                    return -1;
                                }
                            }
                            strError = "DoCopyRecord() error :" + strError;
                            goto ERROR1;
                        }
                    }

                    /*
                    // 写书目记录
                    lRet = channel.DoSaveTextRes(strRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "复制书目记录 '" + strOldRecPath + "' 到 '" + strTargetRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }                     * */


                    if (bSourceExist == false)
                    {
                        if (String.IsNullOrEmpty(strTargetRecord) == true)
                        {
                            if (String.IsNullOrEmpty(strOldRecord) == true)
                            {
                                strError = "源记录 '" + strOldRecPath + "' 不存在，并且<record>元素无文本内容，这时<oldRecord>元素也无文本内容，无法获得要写入的记录内容";
                                return -1;
                            }

                            strTargetRecord = strOldRecord;
                        }
                    }

                    // 如果有“新记录”内容
                    if (String.IsNullOrEmpty(strTargetRecord) == false)
                    {
                        nRet = TryWriteItemRecord(channel,
                            strTargetRecPath,
                            strTargetRecord,
                            "biblio",
                            robust,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "写书目记录 '" + strTargetRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        // 写书目记录
                        lRet = channel.DoSaveTextRes(strTargetRecPath,
                            strTargetRecord,
                            false,
                            "content,ignorechecktimestamp",
                            timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "写书目记录 '" + strTargetRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#endif
                    }

                    // 复制或者移动下级子记录
                    if (strAction == "move"
                    || strAction == "copy")
                    {
                        string[] element_names = new string[] {
                            "copyEntityRecords",
                            "moveEntityRecords",
                            "copyOrderRecords",
                            "moveOrderRecords",
                            "copyIssueRecords",
                            "moveIssueRecords",
                            "copyCommentRecords",
                            "moveCommentRecords"
                        };

                        for (int i = 0; i < element_names.Length; i++)
                        {
                            var node_subrecords = domLog.DocumentElement.SelectSingleNode(
                                element_names[i]) as XmlElement;
                            if (node_subrecords != null)
                            {
                                nRet = CopySubRecords(
                                    channel,
                                    node_subrecords,
                                    strTargetRecPath,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }
                        }
                    }

                    // 2011/12/12
                    if (bSourceExist == true
                        && delete_source_record == false
                        && (strAction == "move" || strAction == "onlymovebiblio")
                        )
                    {
                        // 删除源书目记录
                        lRet = TryDeleteRes(
                            channel,
                            strOldRecPath,
                            timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除书目记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        int nRedoCount = 0;
                    REDO_DELETE:
                        // 删除源书目记录
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                            {
                                // 记录本来就不存在
                            }
                            else if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            else
                            {
                                strError = "删除书目记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                                return -1;
                            }
                        }
#endif
                    }
                }
                else if (strAction == "delete"
                    || strAction == "onlydeletebiblio"
                    || strAction == "onlydeletesubrecord")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    if (strAction != "onlydeletesubrecord")
                    {
                        // 删除书目记录
                        lRet = TryDeleteRes(
                            channel,
                            strRecPath,
                            timestamp,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#if REMOVED
                        int nRedoCount = 0;
                    REDO:
                        // 删除书目记录
                        lRet = channel.DoDeleteRes(strRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                goto DO_DELETE_CHILD_ENTITYRECORDS;   // 记录本来就不存在
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO;
                                }
                            }
                            strError = "删除书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
#endif
                    }

                DO_DELETE_CHILD_ENTITYRECORDS:
                    if (strAction == "delete" || strAction == "onlydeletesubrecord")
                    {
                        if (robust)
                        {
                            // 在 robust 状态下改用即时检索和删除下级记录的方式
                            nRet = LogicDeleteSubRecords(
channel,
strRecPath,
out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else
                        {
                            nRet = DeleteSubRecords(
                                channel,
                                domLog,
                                robust,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                    } // end if
                }

                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // 和数据库中已有记录合并，然后保存
                if (strAction == "new" || strAction == "change")
                {
                    // 和SnapShot方式相同
                    bReuse = true;
                    origin_level = level;
                    goto DO_SNAPSHOT;
                }
                else if (strAction == "onlymovebiblio"
                    || strAction == "onlycopybiblio"
                    || strAction == "move"
                    || strAction == "copy")
                {
                    // TODO: 最好重新实现一次。关键点是自行用原始 XML 记录利用 operator 账户权限进行字段过滤，最终决定要写入数据库的 XML 记录内容

                    // 和SnapShot方式相同
                    bReuse = true;
                    origin_level = level;
                    goto DO_SNAPSHOT;
                }
                else if (strAction == "delete"
                    || strAction == "onlydeletebiblio"
                    || strAction == "onlydeletesubrecord")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    if (strAction != "onlydeletesubrecord")
                    {
                        // 删除书目记录
                        lRet = TryDeleteRes(
                            channel,
                            strRecPath,
                            null,
                            robust ? "" : "notExistingAsError",
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }
#if REMOVED
                        int nRedoCount = 0;
                        byte[] timestamp = null;
                        byte[] output_timestamp = null;
                    REDO:
                        // 删除书目记录
                        lRet = channel.DoDeleteRes(strRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                goto DO_DELETE_CHILD_ENTITYRECORDS;   // 记录本来就不存在
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO;
                                }
                            }
                            strError = "删除书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }
#endif
                    }

                DO_DELETE_CHILD_ENTITYRECORDS:

                    if (strAction == "delete" || strAction == "onlydeletesubrecord")
                    {
                        nRet = LogicDeleteSubRecords(
    channel,
    strRecPath,
    out strError);
                        if (nRet == -1)
                            return -1;
#if REMOVED
                        // 删除属于同一书目记录的全部实体记录
                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = DeleteBiblioChildEntities(channel,
                            strRecPath,
                            null,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 下属的实体记录时出错: " + strError;
                            goto ERROR1;
                        }

                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.OrderItemDatabase.DeleteBiblioChildItems(
                            // Channels,
                            channel,
                            strRecPath,
                            null,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 下属的订购记录时出错: " + strError;
                            goto ERROR1;
                        }

                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.IssueItemDatabase.DeleteBiblioChildItems(
                            // Channels,
                            channel,
                            strRecPath,
                            null,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 下属的期记录时出错: " + strError;
                            goto ERROR1;
                        }
#endif
                    }
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }
            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetBiblioInfo() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        int LogicDeleteSubRecords(
            RmsChannel channel,
            string strRecPath,
            out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();
            // 删除属于同一书目记录的全部实体记录
            // return:
            //      -1  error
            //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
            //      >0  实际删除的实体记录数
            int nRet = DeleteBiblioChildEntities(channel,
                strRecPath,
                null,
                out strError);
            if (nRet == -1)
            {
                errors.Add("删除书目记录 '" + strRecPath + "' 下属的实体记录时出错: " + strError);
            }

            // return:
            //      -1  error
            //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
            //      >0  实际删除的实体记录数
            nRet = this.OrderItemDatabase.DeleteBiblioChildItems(
                channel,
                strRecPath,
                null,
                out strError);
            if (nRet == -1)
            {
                errors.Add("删除书目记录 '" + strRecPath + "' 下属的订购记录时出错: " + strError);
            }

            // return:
            //      -1  error
            //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
            //      >0  实际删除的实体记录数
            nRet = this.IssueItemDatabase.DeleteBiblioChildItems(
                channel,
                strRecPath,
                null,
                out strError);
            if (nRet == -1)
            {
                errors.Add("删除书目记录 '" + strRecPath + "' 下属的期记录时出错: " + strError);
            }

            nRet = this.CommentItemDatabase.DeleteBiblioChildItems(
    channel,
    strRecPath,
    null,
    out strError);
            if (nRet == -1)
            {
                errors.Add("删除书目记录 '" + strRecPath + "' 下属的评注记录时出错: " + strError);
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, ";");
                return -1;
            }

            return 0;
        }

        // 删除所有类型的下级记录
        int DeleteSubRecords(
    RmsChannel channel,
    XmlDocument domLog,
    bool robust,
    out string strError)
        {
            strError = "";

            List<string> all_types = new List<string> {
            "entity",
            "order",
            "issue",
            "comment"
            };

            List<string> errors = new List<string>();
            foreach (var type in all_types)
            {
                int nRet = DeleteSubRecords(
        channel,
        domLog,
        type,
        robust,
        out strError);
                if (nRet == -1)
                    errors.Add(strError);
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, ";");
                return -1;
            }

            return 0;
        }

        // 删除单一类型的下级记录
        int DeleteSubRecords(
            RmsChannel channel,
            XmlDocument domLog,
            string child_type,
            bool robust,
            out string strError)
        {
            strError = "";

            string UpperFirst(string text)
            {
                return char.ToUpper(text[0]) + text.Substring(1);
            }

            List<string> errors = new List<string>();

            XmlNodeList nodes = domLog.DocumentElement.SelectNodes($"deleted{UpperFirst(child_type)}Records/record");
            foreach (XmlElement node in nodes)
            {
                string strEntityRecPath = node.GetAttribute("recPath");

                /*
                if (String.IsNullOrEmpty(strEntityRecPath) == true)
                    continue;
                 * */

                // 删除实体记录
                long lRet = TryDeleteRes(
                    channel,
                    strEntityRecPath,
                    null,
                    robust ? "" : "notExistingAsError",
                    out strError);
                if (lRet == -1)
                {
                    errors.Add($"删除{child_type}记录 '{strEntityRecPath}' 时发生错误: {strError}");
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, ";");
                return -1;
            }

            return 0;
        }

        #region 尝试临时重建数据库

        // 猜测一个数据库名是书目库(系列)名字还是读者库名字
        // return:
        //      "biblio"
        //      "reader"
        //      "amerce"
        static string GuessBiblioOrReaderOrAmerce(string strDbName)
        {
            if (strDbName == "违约金")
                return "amerce";
            if (strDbName.Contains("读者"))
                return "reader";
            if (strDbName.Contains("图书")
                || strDbName.ToLower().Contains("books")
                || strDbName.Contains("期刊")
                || strDbName.ToLower().Contains("series"))
                return "biblio";
            if (strDbName.EndsWith("实体")
    || strDbName.EndsWith("订购")
    || strDbName.EndsWith("期")
    || strDbName.EndsWith("评注"))
                return "biblio";
            if (strDbName.Contains("书目")
                || strDbName.Contains("源"))
                return "biblio";
            return "reader";
        }

        static string GuessReaderDbLibraryCode(string strOriginDbName)
        {
            if (strOriginDbName.EndsWith("读者"))
                return strOriginDbName.Substring(0, strOriginDbName.Length - 2);
            return strOriginDbName;
        }

        // return:
        //      -2  nRedoCount 达到或者超过 2
        //      -1  出错
        //      0   成功。nRedoCount 被增量
        int TryCreateReaderDatabase(
            RmsChannel channel,
            string strRecPath,
            Delegate_warning func_warning,
            ref int nRedoCount,
            out string strError)
        {
            strError = "";
            if (nRedoCount >= 2)
            {
                strError = "重试次数达到或者超过 2";
                return -1;
            }

            // 创建一个书目库。并记载下来这是新创建的
            string strOriginDbName = ResPath.GetDbName(strRecPath);
            string strLibraryCode = GuessReaderDbLibraryCode(strOriginDbName);

            // 警告这里的馆代码是根据数据库名猜测出来的
            func_warning?.Invoke($"在临时创建读者库 '{strOriginDbName}' 时，根据读者库名猜测出馆代码为 '{strLibraryCode}', 这可能不正确，请注意在日志恢复完成后检查修订");

            // return:
            //      -1  出错
            //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
            //      1   成功创建
            int nRet = CreateReaderDatabase(
                channel,
                strOriginDbName,
                strLibraryCode,
                true,
                "", // strStyle,
                out string strRequestXml,
                out strError);
            if (nRet != 1)
            {
                strError = $"临时决定创建读者库 '{strOriginDbName}' 的过程出错: {strError}";
                return -1;
            }
            nRedoCount++;
            return 0;
        }



        // 根据原始数据库名字，猜测书目库名字
        // parameters:
        //      strDbName   下级库名字，或者书目库名字
        //      subtype [out] 返回 strDbName 本身对应的下级库类型。如果为空，表示 strDbName 为小书目库本体
        // return:
        //      所猜测的书目库名字
        static string GuessBiblioDbName(string strDbName,
            out string subtype)
        {
            subtype = "";

            if (strDbName.EndsWith("实体"))
            {
                subtype = "entity";
                return strDbName.Substring(0, strDbName.Length - 2);
            }
            if (strDbName.EndsWith("订购"))
            {
                subtype = "order";
                return strDbName.Substring(0, strDbName.Length - 2);
            }
            if (strDbName.EndsWith("期"))
            {
                subtype = "issue";
                return strDbName.Substring(0, strDbName.Length - 1);
            }
            if (strDbName.EndsWith("评注"))
            {
                subtype = "comment";
                return strDbName.Substring(0, strDbName.Length - 2);
            }

            return strDbName;
        }

        // 根据书目库名字猜测 MARC 格式
        static string GuessMarcSyntax(string strBiblioDbName)
        {
            if (strBiblioDbName.ToLower().Contains("english")
                || strBiblioDbName.Contains("西文")
                || strBiblioDbName.Contains("英文")
                || strBiblioDbName.Contains("外文"))
                return "usmarc";
            return "unimarc";
        }

        // return:
        //      -2  nRedoCount 达到或者超过 2
        //      -1  出错
        //      0   成功。nRedoCount 被增量
        int TryCreateBiblioDatabase(
            RmsChannel channel,
            string strRecPath,
            ref int nRedoCount,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nRedoCount >= 2)
            {
                strError = "重试次数达到或者超过 2";
                return -1;
            }

            // 创建一个书目库。并记载下来这是新创建的
            string strOriginDbName = ResPath.GetDbName(strRecPath);
            string strBiblioDbName = GuessBiblioDbName(strOriginDbName, out string subtype);
            string usage = "book";
            if (strBiblioDbName.Contains("期刊")
                || strBiblioDbName.ToLower().Contains("series"))
                usage = "series";

            // 探测书目(小)库是否存在
            bool bibliodb_exists = false;
            // 数据库是否已经存在？
            // return:
            //      -1  error
            //      0   not exist
            //      1   exist
            //      2   其他类型的同名对象已经存在
            nRet = DatabaseUtility.IsDatabaseExist(
                channel,
                strBiblioDbName,
                out strError);
            if (nRet == -1 || nRet == 2)
            {
                strError = $"在探测书目库 '{strBiblioDbName}' 是否存在的过程中出错: {strError}";
                return -1;
            }
            if (nRet == 1)
                bibliodb_exists = true;

            string strStyle = "";
            string strSubTypeList = "*";
            if (string.IsNullOrEmpty(subtype) == false
                && bibliodb_exists == true)
            {
                //strSubTypeList = subtype;
                //strStyle = "suppressBiblioExistingError";

                nRet = CreateBiblioChildDatabase(
    channel,
    strOriginDbName,
    strBiblioDbName,
    subtype,
    strStyle,
    out _,
    out strError);
                if (nRet != 1)
                {
                    strError = $"临时决定创建书目库 '{strBiblioDbName}' 的下级库 '{strOriginDbName}' 过程出错: {strError}";
                    return -1;
                }
            }
            else
            {
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                nRet = CreateBiblioDatabase(
                    channel,
                    strBiblioDbName,
                    usage, // strUsage,
                    "", // strRole,
                    GuessMarcSyntax(strBiblioDbName),
                    strSubTypeList,
                    strStyle,
                    out string strRequestXml,
                    out strError);
                if (nRet != 1)
                {
                    strError = $"临时决定创建书目库 '{strBiblioDbName}' 的过程出错: {strError}";
                    return -1;
                }
            }

            nRedoCount++;
            return 0;
        }


        // 创建一个书目库。用于日志恢复时临时决定创建一个书目库
        // parameters:
        //      strRequestXml   返回对 dp2library 发出的请求 XML
        //      strStyle        如果包含 suppressBiblioExistingError，表示如果小书目库本身在创建前已经存在，不要报错
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public int CreateBiblioDatabase(
            RmsChannel channel,
            // RmsChannelCollection Channels,
            string strBiblioDbName,
            string strUsage,
            string strRole,
            string strSyntax,
            string strSubTypeList,
            string strStyle,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建书目库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            List<string> biblio_dbnames = new List<string>();
            //List<string> biblio_aliases = new List<string>();

            // 创建书目库
            {
                // parameters:
                //      strUsage    book/series
                //      strSyntax   unimarc/usmarc
                ServerDatabaseUtility.CreateBiblioDatabaseNode(database_dom,
                    strBiblioDbName,
                    strUsage,   // "book",
                    strRole, // "orderRecommendStore,catalogTarget",    // 2015/7/6 增加 catalogTarget
                    strSyntax,  // "unimarc",
                    strSubTypeList,
                    true);
                biblio_dbnames.Add(strBiblioDbName);
                // biblio_aliases.Add("cbook");
            }

#if REMOVED
            // 创建 OPAC 数据库的定义
            XmlDocument opac_dom = new XmlDocument();
            opac_dom.LoadXml("<virtualDatabases />");

            // Debug.Assert(biblio_aliases.Count == biblio_dbnames.Count, "");

            int i = 0;
            foreach (string dbname in biblio_dbnames)
            {
                //string alias = biblio_aliases[i];

                XmlElement node = opac_dom.CreateElement("database");
                opac_dom.DocumentElement.AppendChild(node);
                node.SetAttribute("name", dbname);
                //node.SetAttribute("alias", alias);
                i++;
            }
#endif
            strRequestXml = database_dom.OuterXml;

            return this.CreateDatabase(null,
                channel,    // Channels,
                "", // strLibraryCodeList,
                strRequestXml,
                false,
                "skipOperLog" + "," + strStyle,
                out string output_info,
                out strError);

#if REMOVED
            {
                strRequestXml = database_dom.OuterXml;
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    strRequestXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
#endif
        }

        // 创建一个下级库
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public int CreateBiblioChildDatabase(
            RmsChannel channel,
            string strChildDbName,
            string strBiblioDbName,
            string strSubType,
            string strStyle,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建下级库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            // 创建下级库
            {
                ServerDatabaseUtility.CreateBiblioChildDatabaseNode(database_dom,
                    strChildDbName,
                    strBiblioDbName,
                    strSubType);
            }

            strRequestXml = database_dom.OuterXml;

            return this.CreateDatabase(null,
                channel,    // Channels,
                "", // strLibraryCodeList,
                strRequestXml,
                false,
                "skipOperLog" + "," + strStyle,
                out string output_info,
                out strError);
        }


        // 创建一个读者库
        // parameters:
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public int CreateReaderDatabase(
            RmsChannel channel,
            string strDbName,
            string strLibraryCode,
            bool bInCirculation,
            string strStyle,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            {
                // 创建读者库
                ServerDatabaseUtility.CreateReaderDatabaseNode(database_dom,
                    strDbName,
                    strLibraryCode,
                    bInCirculation);
            }

            strRequestXml = database_dom.OuterXml;

            return this.CreateDatabase(null,
                channel,    // Channels,
                "", // strLibraryCodeList,
                strRequestXml,
                false,
                "skipOperLog",
                out string output_info,
                out strError);
#if REMOVED
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                strRequestXml = database_dom.OuterXml;
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    strRequestXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
#endif
        }

        // 创建一个违约金库
        // parameters:
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public int CreateAmerceDatabase(
            RmsChannel channel,
            string strDbName,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            {
                // 违约金
                ServerDatabaseUtility.CreateSimpleDatabaseNode(database_dom,
                    strDbName,  // "违约金",
                    "amerce");
            }

            strRequestXml = database_dom.OuterXml;

            return this.CreateDatabase(null,
                channel,    // Channels,
                "", // strLibraryCodeList,
                strRequestXml,
                false,
                "skipOperLog",
                out string output_info,
                out strError);
        }

        // 2024/4/15
        // return:
        //      -2  nRedoCount 达到或者超过 2
        //      -1  出错
        //      0   成功。nRedoCount 被增量
        int TryCreateAmerceDatabase(
            RmsChannel channel,
            string strRecPath,
            ref int nRedoCount,
            out string strError)
        {
            strError = "";
            if (nRedoCount >= 2)
            {
                strError = "重试次数达到或者超过 2";
                return -1;
            }

            // 创建一个书目库。并记载下来这是新创建的
            string strDbName = ResPath.GetDbName(strRecPath);

            // return:
            //      -1  出错
            //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
            //      1   成功创建
            int nRet = CreateAmerceDatabase(
                channel,
                strDbName,
                out string strRequestXml,
                out strError);
            if (nRet != 1)
            {
                strError = $"临时决定创建违约金库 '{strDbName}' 的过程出错: {strError}";
                return -1;
            }
            nRedoCount++;
            return 0;
        }

        #endregion

        // 源记录不存在，应该忽略；目标库不存在，也应该忽略
        /*
    <copyEntityRecords> 被复制的实体记录(容器)。只有当<action>为*copy*时才有这个元素。
      <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110' newBarcode='...'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
      ...
    </copyEntityRecords>
    <moveEntityRecords> 被移动的实体记录(容器)。只有当<action>为*move*时才有这个元素。
      <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
      ...
    </moveEntityRecords>
        注意 targetRecPath 属性值可能为空，这时候表明删除 recPath 属性值指向的记录
         * */
        public int CopySubRecords(
            RmsChannel channel,
            XmlElement container_node,
            string strTargetBiblioRecPath,
            out string strError)
        {
            strError = "";

            string strAction = "";
            if (StringUtil.HasHead(container_node.Name, "copy") == true)
                strAction = "copy";
            else if (StringUtil.HasHead(container_node.Name, "move") == true) // 2011/12/5 原先有BUG "copy"
                strAction = "move";
            else
            {
                strError = "不能识别的元素名 '" + container_node.Name + "'";
                return -1;
            }

            XmlNodeList nodes = container_node.SelectNodes("record");
            foreach (XmlElement record_node in nodes)
            {
                string strSourceRecPath = DomUtil.GetAttr(record_node, "recPath");
                string strTargetRecPath = DomUtil.GetAttr(record_node, "targetRecPath");

                string strNewBarcode = DomUtil.GetAttr(record_node, "newBarcode");
                string strNewRefID = DomUtil.GetAttr(record_node, "newRefID");
                string strOldRefID = record_node.GetAttribute("oldRefID");  // 2025/2/13

                string strMetaData = "";
                string strXml = "";
                byte[] timestamp = null;
                string strOutputPath = "";

                long lRet = channel.GetRes(strSourceRecPath,
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.IsNotFound())
                        continue;   // 是否报错?

                    strError = "获取下级记录 '" + strSourceRecPath + "' 时发生错误: " + strError;
                    return -1;
                    // goto CONTINUE;
                }

                DeleteEntityInfo entityinfo = new DeleteEntityInfo();

                entityinfo.RecPath = strOutputPath;
                entityinfo.OldTimestamp = timestamp;
                entityinfo.OldRecord = strXml;

                List<DeleteEntityInfo> entityinfos = new List<DeleteEntityInfo>();
                entityinfos.Add(entityinfo);

                // TODO: 如果目标数据库已经不存在，要跳过

                List<string> target_recpaths = new List<string>();
                target_recpaths.Add(strTargetRecPath);  // 注意 strTargetRecPath 可能为空
                List<string> newbarcodes = new List<string>();
                newbarcodes.Add(strNewBarcode);
                List<string> newrefids = new List<string>();
                newrefids.Add(strNewRefID);
                // 2025/2/13
                List<string> oldrefids = new List<string>();
                oldrefids.Add(strOldRefID);


                // 复制属于同一书目记录的全部实体记录
                // parameters:
                //      strAction   copy / move
                // return:
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                int nRet = CopyBiblioChildRecords(channel,
                    strAction,
                    entityinfos,
                    target_recpaths,
                    strTargetBiblioRecPath,
                    newbarcodes,
                    newrefids,
                    oldrefids,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        /*
    hire 创建租金记录

    API: Hire()

    <root>
        <operation>hire</operation> 操作类型
        <action>...</action> 具体动作 有hire hirelate两种
        <readerBarcode>R0000002</readerBarcode> 读者证条码号
        <operator>test</operator> 操作者
        <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
        <overdues>...</overdues> 租金信息 通常内容为一个字符串，为一个或多个<overdue>元素XML文本片断
        <readerRecord recPath='...'>...</readerRecord>	最新读者记录
        <readerRefID>...</readerRefID> 读者记录参考 ID。2024/4/8 增加
    </root>
         * */
        public int RecoverHire(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            // long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                string strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺 readerRecord 元素";
                    return -1;
                }
                string strReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 写读者记录
                nRet = TryWriteReaderRecord(
                    channel,
                    strReaderRecPath,
                    strReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写读者记录
                    lRet = channel.DoSaveTextRes(strReaderRecPath,
                    strReaderXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                return 0;
            }


            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // string strRecoverComment = "";

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRefID");
                ///
                if (String.IsNullOrEmpty(strReaderBarcode) == true
                    && string.IsNullOrEmpty(strReaderRefID) == true)
                {
                    strError = "日志记录中 readerBarcode 和 readerRefID 元素值均为空";
                    goto ERROR1;
                }

                string strReaderKey = dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID);

                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
                    "operator");

                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement,
                    "operTime");

                string strOverdues = DomUtil.GetElementText(domLog.DocumentElement,
                    "overdues");
                if (String.IsNullOrEmpty(strOverdues) == true)
                {
                    strError = "日志记录中<overdues>元素值为空";
                    goto ERROR1;
                }

                // 从overdues字符串中分析出id
                XmlDocument tempdom = new XmlDocument();
                tempdom.LoadXml("<root />");
                XmlDocumentFragment fragment = tempdom.CreateDocumentFragment();
                fragment.InnerXml = strOverdues;
                tempdom.DocumentElement.AppendChild(fragment);

                XmlNode tempnode = tempdom.DocumentElement.SelectSingleNode("overdue");
                if (tempnode == null)
                {
                    strError = "<overdues>元素内容有误，缺乏<overdue>元素";
                    goto ERROR1;
                }

                string strID = DomUtil.GetAttr(tempnode, "id");
                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "日志记录中<overdues>内容中<overdue>元素中id属性值为空";
                    goto ERROR1;
                }

                // 读入读者记录
                nRet = this.GetReaderRecXml(
                    channel,
                    strReaderKey,   // strReaderBarcode,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    // 2024/4/15
                    if (robust)
                    {
                        // 用 record 元素中的内容代替
                        strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                            "readerRecord",
                            out XmlNode node);
                        if (node != null)
                        {
                            strOutputReaderRecPath = DomUtil.GetAttr(node, "recPath");
                            if (string.IsNullOrEmpty(strReaderXml) == false
                                && string.IsNullOrEmpty(strOutputReaderRecPath) == false)
                                goto CONTINUE_LOAD;
                        }
                    }

                    strError = "读者证条码号 '" + strReaderKey + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入证条码号为 '" + strReaderBarcode + "' 的读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

            CONTINUE_LOAD:
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 根据Hire() API要求，修改readerdom
                nRet = DoHire(strAction,
                    readerdom,
                    ref strID,
                    strOperator,
                    strOperTime,
                    out string strOverdueString,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2024/4/15
                nRet = TryWriteReaderRecord(
                    channel,
                    strOutputReaderRecPath,
                    readerdom.OuterXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#if REMOVED
                // 写回读者记录
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
#endif
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverHire() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }


        /*
    foregift 创建押金记录

    API: Foregift()

    <root>
        <operation>foregift</operation> 操作类型
        <action>...</action> 具体动作 目前有foregift return (注: return操作时，overdue元素里面的price属性，可以使用宏 %return_foregift_price% 表示当前剩余的押金额)
        <readerBarcode>R0000002</readerBarcode> 读者证条码号
        <operator>test</operator> 操作者
        <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
        <overdues>...</overdues> 押金信息 通常内容为一个字符串，为一个或多个<overdue>元素XML文本片断
        <readerRecord recPath='...'>...</readerRecord>	最新读者记录
        <readerRefID>...</readerRefID> 读者记录参考 ID。2024/4/8 增加
    </root>
         * * */
        public int RecoverForegift(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            // long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

        DO_SNAPSHOT:

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                string strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺 readerRecord 元素";
                    return -1;
                }
                string strReaderRecPath = DomUtil.GetAttr(node, "recPath");

                // 写读者记录
                // 注: foregift 和 return 动作，都只会修改读者记录。因此恢复时，覆盖读者记录即可
                nRet = TryWriteReaderRecord(
                    channel,
                    strReaderRecPath,
                    strReaderXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                {
                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写读者记录
                    lRet = channel.DoSaveTextRes(strReaderRecPath,
                    strReaderXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            nRet = TryCreateReaderDatabase(
    channel,
    strReaderRecPath,
    func_warning,
    ref nRedoCount,
    out string error);
                            if (nRet == 0)
                                goto REDO_WRITE;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strReaderRecPath}' 临时决定创建读者库的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "写入读者记录 '" + strReaderRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
#endif

                return 0;
            }


            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // string strRecoverComment = "";

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRefID");
                ///
                if (String.IsNullOrEmpty(strReaderBarcode) == true
                    && string.IsNullOrEmpty(strReaderRefID) == true)
                {
                    strError = "日志记录中 readerBarcode 和 readerRefID 元素值均为空";
                    goto ERROR1;
                }

                string strReaderKey = dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID);

                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
                    "operator");

                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement,
                    "operTime");

                string strOverdues = DomUtil.GetElementText(domLog.DocumentElement,
                    "overdues");
                if (String.IsNullOrEmpty(strOverdues) == true)
                {
                    strError = "日志记录中<overdues>元素值为空";
                    goto ERROR1;
                }

                // 从overdues字符串中分析出id
                XmlDocument tempdom = new XmlDocument();
                tempdom.LoadXml("<root />");
                XmlDocumentFragment fragment = tempdom.CreateDocumentFragment();
                fragment.InnerXml = strOverdues;
                tempdom.DocumentElement.AppendChild(fragment);

                XmlNode tempnode = tempdom.DocumentElement.SelectSingleNode("overdue");
                if (tempnode == null)
                {
                    strError = "<overdues>元素内容有误，缺乏<overdue>元素";
                    goto ERROR1;
                }

                string strID = DomUtil.GetAttr(tempnode, "id");
                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "日志记录中 overdues/overdue 元素中 id 属性值为空，无法完成日志恢复动作";
                    goto ERROR1;
                }

                // 读入读者记录
                nRet = this.GetReaderRecXml(
                    channel,
                    strReaderKey,   // strReaderBarcode,
                    out string strReaderXml,
                    out string strOutputReaderRecPath,
                    out byte[] reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    // 2024/4/15
                    if (robust)
                    {
                        // 用 record 元素中的内容代替
                        strReaderXml = DomUtil.GetElementText(domLog.DocumentElement,
                            "readerRecord",
                            out XmlNode node);
                        if (node != null)
                        {
                            strOutputReaderRecPath = DomUtil.GetAttr(node, "recPath");
                            if (string.IsNullOrEmpty(strReaderXml) == false
                                && string.IsNullOrEmpty(strOutputReaderRecPath) == false)
                                goto CONTINUE_LOAD;
                        }
                    }

                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入证条码号为 '" + strReaderBarcode + "' 的读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

            CONTINUE_LOAD:
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 根据Foregift() API要求，修改readerdom
                nRet = DoForegift(strAction,
                    readerdom,
                    ref strID,
                    strOperator,
                    strOperTime,
                    out string strOverdueString,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2024/4/15
                nRet = TryWriteReaderRecord(
                    channel,
                    strOutputReaderRecPath,
                    readerdom.OuterXml,
                    robust,
                    func_warning,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#if REMOVED
                // 写回读者记录
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    reader_timestamp,
                    out byte[] output_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
#endif
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverForegift() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        /*
    settlement 结算违约金

    API: Settlement()

    <root>
    <operation>settlement</operation> 操作类型
    <action>...</action> 具体动作 有settlement undosettlement delete 3种
    <id>1234567-1</id> ID
    <operator>test</operator> 操作者
    <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间

    <oldAmerceRecord recPath='...'>...</oldAmerceRecord>	旧违约金记录
    <amerceRecord recPath='...'>...</amerceRecord>	新违约金记录 delete操作无此元素
    </root>
         * */
        public int RecoverSettlement(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

        DO_SNAPSHOT:

            // 快照恢复
            if (IsSnapshot(level, out bool robust))
            {
                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                if (strAction == "settlement"
                    || strAction == "undosettlement")
                {
                    string strAmerceXml = DomUtil.GetElementText(domLog.DocumentElement,
                        "amerceRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺 amerceRecord 元素";
                        return -1;
                    }
                    string strAmerceRecPath = DomUtil.GetAttr(node, "recPath");

                    int nRedoCount = 0;
                REDO_WRITE:
                    // 写违约金记录
                    lRet = channel.DoSaveTextRes(strAmerceRecPath,
                        strAmerceXml,
                        false,
                        "content,ignorechecktimestamp",
                        null,   // timestamp,
                        out byte[] output_timestamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            // return:
                            //      -2  nRedoCount 达到或者超过 2
                            //      -1  出错
                            //      0   成功。nRedoCount 被增量
                            nRet = TryCreateAmerceDatabase(channel,
                                strAmerceRecPath,
                                ref nRedoCount,
                                out strError);
                            if (nRet == 0)
                                goto REDO_WRITE;
                        }
                        strError = "写入违约金记录 '" + strAmerceRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

                }
                else if (strAction == "delete")
                {
                    string strOldAmerceXml = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldAmerceRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺 oldAmerceRecord 元素";
                        return -1;
                    }
                    string strOldAmerceRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除违约金记录
                    lRet = TryDeleteRes(
                        channel,
                        strOldAmerceRecPath,
                        null,
                        robust ? "" : "notExistingAsError",
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "删除违约金记录 '" + strOldAmerceRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#if REMOVED
                    // 删除违约金记录
                    int nRedoCount = 0;
                    byte[] timestamp = null;
                    byte[] output_timestamp = null;

                REDO_DELETE:
                    lRet = channel.DoDeleteRes(strOldAmerceRecPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound())
                            return 0;   // 记录本来就不存在

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount < 10)
                            {
                                timestamp = output_timestamp;
                                nRedoCount++;
                                goto REDO_DELETE;
                            }
                        }
                        strError = "删除违约金记录 '" + strOldAmerceRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
#endif
                }
                else
                {
                    strError = "未能识别的action值 '" + strAction + "'";
                }

                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                string strID = DomUtil.GetElementText(domLog.DocumentElement,
                    "id");

                ///
                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "日志记录中 id 元素值为空";
                    goto ERROR1;
                }

                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
                    "operator");

                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement,
                    "operTime");

                // 通过id获得违约金记录的路径
                // TODO: 这一段可以重构到一个函数中
                string strText = "";
                string strCount = "";

                strCount = "<maxCount>100</maxCount>";

                strText = "<item><word>"
    + StringUtil.GetXmlStringSimple(strID)
    + "</word>"
    + strCount
    + "<match>exact</match><relation>=</relation><dataType>string</dataType>"
    + "</item>";
                string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.AmerceDbName + ":" + "ID")       // 2007/9/14
                    + "'>" + strText
    + "<lang>zh</lang></target>";

                lRet = channel.DoSearch(strQueryXml,
                    "amerced",
                    "", // strOuputStyle
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索ID为 '" + strID + "' 的违约金记录出错: " + strError;
                    goto ERROR1;
                }

                if (lRet == 0)
                {
                    strError = "没有找到id为 '" + strID + "' 的违约金记录";
                    goto ERROR1;
                }

                lRet = channel.DoGetSearchResult(
                    "amerced",   // strResultSetName
                    0,
                    1,
                    "zh",
                    null,   // stop
                    out List<string> aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "获取结果集未命中";
                    goto ERROR1;
                }

                if (aPath.Count != 1)
                {
                    strError = "aPath.Count != 1";
                    goto ERROR1;
                }

                string strAmerceRecPath = aPath[0];

                // 结算一个交费记录
                // parameters:
                //      bCreateOperLog  是否创建日志
                //      strOperTime 结算的操作时间
                //      strOperator 结算的操作者
                // return:
                //      -2  致命出错，不宜再继续循环调用本函数
                //      -1  一般出错，可以继续循环调用本函数
                //      0   正常
                nRet = SettlementOneRecord(
                    "", // 确保可以执行
                    false,  // 不创建日志
                    channel,
                    strAction,
                    strAmerceRecPath,
                    strOperTime,
                    strOperator,
                    "", // 表示本机触发
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;

            }
            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSettlement() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        /*
    <root>
    <operation>writeRes</operation> 
    <requestResPath>...</requestResPath> 资源路径参数。也就是请求API是的strResPath参数值。可能在路径中的记录ID部分包含问号，表示要追加创建新的记录
    <resPath>...</resPath> 资源路径。资源的确定路径。
    <ranges>...</ranges> 字节范围
    <totalLength>...</totalLength> 总长度。如果为 -1，表示仅修改 metadata
    <metadata>...</metadata> 此元素的文本即是记录体，但注意为不透明的字符串（HtmlEncoding后的记录字符串）。
    <style>...</style> 当 style 中包含 delete 子串时表示要删除这个资源 

    <record recPath='违约金/3'>...</record> 记录体
    <oldRecord recPath='违约金/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素

    <operator>test</operator> 
    <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
    </root>
         * 可能会有一个attachment
         * 注: 当记录为文本类型时，才会有 record 和 oldRecord 元素，此时不使用 ranges totalLength 元素和 attachment
    * * */
        public int RecoverWriteRes(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Stream attachmentLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            long lRet = 0;
            // int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                string strStyle = DomUtil.GetElementText(
domLog.DocumentElement,
"style");

                string strResPath = DomUtil.GetElementText(
                    domLog.DocumentElement,
                    "resPath");
                if (string.IsNullOrEmpty(strResPath) == true)
                {
                    // 注: delete 情况下不会有 resPath 元素
                    if (StringUtil.IsInList("delete", strStyle) == true)
                    {
                        strResPath = DomUtil.GetElementText(
                            domLog.DocumentElement,
                            "requestResPath");
                        if (string.IsNullOrEmpty(strResPath) == true)
                        {
                            strError = "日志记录中缺 requestResPath 元素";
                            return -1;
                        }
                    }
                    else
                    {
                        strError = "日志记录中缺 resPath 元素";
                        return -1;
                    }
                }

                // 2024/2/12
                // 文本类型的记录内容
                if (domLog.DocumentElement.SelectSingleNode("record") != null
                    && StringUtil.IsInList("delete", strStyle) == false)
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺 record 元素";
                        return -1;
                    }

                    if (StringUtil.IsInList("autocreatedir", strStyle))
                    {
                        strError = "当日志记录中存在 record 元素时(表明记录中是文本内容)，style 元素中就不应该具有 autocreatedir 子参数。日志恢复时会自动忽略此 autocreatedir 子参数";
                        func_warning?.Invoke(strError);
                    }


                    {
                        int nRedoCount = 0;
                    REDO_WRITE:
                        string strNewRecPath = DomUtil.GetAttr(node, "recPath");
                        lRet = channel.DoSaveTextRes(strNewRecPath,
                            strRecord,
                            false,
                            "content,ignorechecktimestamp",
                            null,
                            out _,
                            out _,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                            {
                                // 注意这里的路径还可能是读者库或违约金库路径
                                string dbName = ResPath.GetDbName(strResPath);
                                string type = GuessBiblioOrReaderOrAmerce(dbName);

                                int nRet = 0;
                                string error = "";
                                if (type == "biblio")
                                    nRet = TryCreateBiblioDatabase(
        channel,
        strResPath,
        ref nRedoCount,
        out error);
                                else if (type == "amerce")
                                    nRet = TryCreateAmerceDatabase(
channel,
strResPath,
ref nRedoCount,
out error);
                                else
                                    nRet = TryCreateReaderDatabase(
            channel,
            strResPath,
            func_warning,
            ref nRedoCount,
            out error);
                                if (nRet == 0)
                                    goto REDO_WRITE;
                                else if (nRet == -1)
                                {
                                    strError = $"根据记录路径 '{strResPath}' 临时决定创建{type}库的过程出错: {error}";
                                    return -1;
                                }
                            }

                            strError = "DoSaveTextRes() '" + strNewRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }
                    return 0;
                }


#if REMOVED
                long lTotalLength = 0;
                try
                {
                    lTotalLength = Convert.ToInt64(strTotalLength);
                }
                catch
                {
                    strError = "lTotalLength值 '" + strTotalLength + "' 格式不正确";
                    return -1;
                }
#endif


                string strMetadata = DomUtil.GetElementText(
    domLog.DocumentElement,
    "metadata");


                // 读入记录内容
                byte[] baRecord = null;

                if (attachmentLog != null && attachmentLog.Length > 0)
                {
                    baRecord = new byte[(int)attachmentLog.Length];
                    attachmentLog.Seek(0, SeekOrigin.Begin);
                    attachmentLog.Read(baRecord, 0, (int)attachmentLog.Length);
                }

                strStyle += ",ignorechecktimestamp";

                byte[] timestamp = null;
                string strOutputResPath = "";
                byte[] output_timestamp = null;

                {
                    int nRedoCount = 0;
                REDO_WRITERES:
                    if (StringUtil.IsInList("delete", strStyle) == true)
                    {
                        /*
                        // 2015/9/3 增加
                        lRet = channel.DoDeleteRes(strResPath,
                            timestamp,
                            strStyle,
                            out output_timestamp,
                            out strError);
                        */
                        // 2024/5/8
                        lRet = TryDeleteRes(
    channel,
    strResPath,
    timestamp,
    robust ? "" : "notExistingAsError",
    out strError);
                    }
                    else
                    {
                        string strTotalLength = DomUtil.GetElementText(
            domLog.DocumentElement,
            "totalLength");
                        if (string.IsNullOrEmpty(strTotalLength) == true)
                        {
                            strError = "日志记录中缺 totalLength 元素";
                            return -1;
                        }

                        if (long.TryParse(strTotalLength, out long lTotalLength) == false)
                        {
                            strError = $"totalLength 元素值 '{strTotalLength}' 格式不正确";
                            return -1;
                        }

                        string strRanges = DomUtil.GetElementText(
        domLog.DocumentElement,
        "ranges");
                        if (lTotalLength != -1 && string.IsNullOrEmpty(strRanges) == true)
                        {
                            // 2017/10/26 注: 当 totalLength 为 -1 时，表示仅修改 metadata。此时 ranges 为空
                            // 而当 totalLength 为非 -1 值时，ranges 就不允许为空
                            strError = "日志记录中缺 ranges 元素(当 totalLength 元素内容为非 -1 时)";
                            return -1;
                        }

                        lRet = 0;
                        // 如果 strStyle 中包含 autocreatedir，要先在这里检测路径第一级的数据库名是否存在对应的 dp2kernel 数据库
                        // 如果不存在，则要先自动创建好这个数据库再往后执行
                        if (StringUtil.IsInList("autocreatedir", strStyle))
                        {
                            var dbname = ResPath.GetDbName(strResPath);
                            // return:
                            //      -1  error
                            //      0   not exist
                            //      1   exist
                            //      2   其他类型的同名对象已经存在
                            var ret = DatabaseUtility.IsDatabaseExist(
                                channel,
                                dbname,
                                out _);
                            if (ret == 0)
                            {
                                lRet = -1;
                                channel.OriginErrorCode = ErrorCodeValue.NotFoundDb;
                                strError = $"记录路径 '{strResPath}' 中的数据库 '{dbname}' 不存在，无法完成写入记录";
                                // 注: 后面就会跳过 .WriteRes()
                            }
                            else if (ret == -1 || ret == 2)
                            {
                                strError = $"在探测 dp2kernel 数据库 '' 是否存在的过程中遇到错误: {strError}";
                                return -1;
                            }
                        }

                        if (lRet != -1)
                        {
                            lRet = channel.WriteRes(strResPath,
                strRanges,
                lTotalLength,
                baRecord,
                strMetadata,
                strStyle,
                timestamp,
                out strOutputResPath,
                out output_timestamp,
                out strError);
                        }
                    }
                    if (lRet == -1)
                    {
                        if (channel.OriginErrorCode == ErrorCodeValue.NotFoundDb
                            && robust == true)
                        {
                            // 注意这里的路径还可能是读者库或违约金库路径
                            string dbName = ResPath.GetDbName(strResPath);
                            string type = GuessBiblioOrReaderOrAmerce(dbName);

                            int nRet = 0;
                            string error = "";
                            if (type == "biblio")
                                nRet = TryCreateBiblioDatabase(
        channel,
        strResPath,
        ref nRedoCount,
        out error);
                            else if (type == "amerce")
                                nRet = TryCreateAmerceDatabase(
channel,
strResPath,
ref nRedoCount,
out error);
                            else
                                nRet = TryCreateReaderDatabase(
    channel,
    strResPath,
    func_warning,
    ref nRedoCount,
    out error);
                            if (nRet == 0)
                                goto REDO_WRITERES;
                            else if (nRet == -1)
                            {
                                strError = $"根据记录路径 '{strResPath}' 临时决定创建{type}库的过程出错: {error}";
                                return -1;
                            }
                        }
                        else if (channel.OriginErrorCode == ErrorCodeValue.NotFound)    // 注: NotFound 是指未找到路径指向的对象，而不是未找到上级元数据记录
                        {
                            // *** 缺乏下级记录
                            // return:
                            //      -1  出错
                            //      0   发现路径 strResPath 内容不是对象路径，拒绝处理
                            int nRet = AddSubRecord(channel,
                                strResPath,
                                out string error);
                            if (nRet == 1)
                                goto REDO_WRITERES;
                            else
                            {
                                strError = $"根据对象记录路径 '{strResPath}' 临时决定为元数据记录中添加 dprms:file 元素的过程出错: {error}";
                                return -1;
                            }
                        }

                        strError = "WriteRes() '" + strResPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
                return 0;
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // 和SnapShot方式相同
                bReuse = true;
                goto DO_SNAPSHOT;
            }
            return 0;
#if NO
            ERROR1:
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
            return -1;
#endif
        }

        // return:
        //      -1  出错
        //      0   发现路径 strResPath 内容不是对象路径，拒绝处理
        static int AddSubRecord(RmsChannel channel,
            string strResPath,
            out string strError)
        {
            strError = "";

            string strPath = strResPath;
            string strDbName = StringUtil.GetFirstPartPath(ref strPath);
            bool is_object_path = IsRestObjectPath(strPath, out string strObjectID);
            if (is_object_path == false)
            {
                strError = $"路径 '{strResPath}' 不是对象路径";
                return 0;
            }

            string strID = StringUtil.GetFirstPartPath(ref strPath);
            if (strID != null)
                strID = strID.TrimStart('0');
            if (IsId(strID) == false)
            {
                strError = $"路径 '{strResPath}' 不是对象路径: 元数据记录 ID '{strID}' 不符合格式要求";
                return 0;
            }
            if (IsId(strObjectID) == false)
            {
                strError = $"路径 '{strResPath}' 不是对象路径: 对象记录 ID '{strObjectID}' 不符合格式要求";
                return 0;
            }

            // 获得元数据记录
            string strMetadataRecPath = strDbName + "/" + strID;
            long lRet = channel.GetRes(strMetadataRecPath,
    out string xml,
    out _,
    out byte[] timestamp,
    out string output_recpath,
    out strError);
            if (lRet == -1)
            {
                strError = $"AddSubRecord() 在获取元数据记录 '{strMetadataRecPath}' 时出错: {strError}";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                strError = $"元数据记录 '{strMetadataRecPath}' 装入 XMLDOM 时出现异常: {ex.Message}";
                return -1;
            }

            XmlElement file_node = dom.CreateElement("dprms",
    "file",
    DpNs.dprms);
            dom.DocumentElement.AppendChild(file_node);
            file_node.SetAttribute("id", strObjectID);

            lRet = channel.DoSaveTextRes(output_recpath,
    dom.OuterXml,
    false,
    "content,ignorechecktimestamp",
    timestamp,
    out byte[] output_timestamp,
    out _,
    out strError);
            if (lRet == -1)
            {
                strError = $"AddSubRecord() 在保存回元数据记录 '{output_recpath}' 时出错: {strError}";
                return -1;
            }

            return 1;
        }

        /*
    <root>
        <operation>repairBorrowInfo</operation> 
        <action>...</action> 具体动作 有 repairreaderside repairitemside
        <readerBarcode>...</readerBarcode>
        <readerRefID>...</readerRefID> 新增加
        <itemBarcode>...</itemBarcode>
        <itemRefID>...</itemRefID> 新增加
        <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
        <operator>test</operator> 
        <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 

        <!-- 2024/2/22 增加 -->
        <readerRecord recPath='...' changed='false' existing='false'>...</readerRecord> 如果读者记录发生了改变，则这里记载改变后的读者记录内容
        <itemRecord recPath='...' changed='false'>...</itemRecord> 如果册记录发生了改变，则这里记载改变后的册记录内容
    </root>
         * * 
         * */
        public int RecoverRepairBorrowInfo(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Stream attachmentLog,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string version = DomUtil.GetElementText(domLog.DocumentElement,
"version");

            // 逻辑方式，或者日志动作版本 1.10 以下
            if (IsLogic(level, out bool robust)
                || StringUtil.CompareVersion(version, "1.10") < 0)  // 只能用逻辑方式恢复
            {
                /*
                string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
    "libraryCode");
                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
                    "operator");
                */
                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
"action");

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerBarcode");
                string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "readerRefID");
                if (String.IsNullOrEmpty(strReaderBarcode) == true
                    && string.IsNullOrEmpty(strReaderRefID) == true)
                {
                    strError = "readerBarcode 和 readerRefID 元素值均为空";
                    goto ERROR1;
                }

                string strReaderKey = dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID);
                // 2024/4/16
                // 如果日志记录中 readerBarcode 和 readerRefID 均为空，则需要找 readerRecord/@itemKey 属性值
                if (string.IsNullOrEmpty(strReaderBarcode) && string.IsNullOrEmpty(strReaderRefID))
                    strReaderKey = domLog.DocumentElement.SelectSingleNode("readerRecord/@readerKey")?.Value;

                string strConfirmItemRecPath = GetConfirmRecPath(domLog);

                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                string strItemRefID = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemRefID");
                if (String.IsNullOrEmpty(strItemBarcode) == true
                    && string.IsNullOrEmpty(strItemRefID) == true)
                {
                    strError = "itemBarcode 和 itemRefID 元素值均为空";
                    goto ERROR1;
                }
                string strItemKey = dp2StringUtil.BuildReaderKey(strItemBarcode, strItemRefID);
                // 2024/4/16
                // 如果日志记录中 itemBarcode 和 itemRefID 均为空，则需要找 itemRecord/@itemKey 属性值
                if (string.IsNullOrEmpty(strItemBarcode) && string.IsNullOrEmpty(strItemRefID))
                    strItemKey = domLog.DocumentElement.SelectSingleNode("itemRecord/@readerKey")?.Value;

                SessionInfo temp_sessioninfo = new SessionInfo(this);
                temp_sessioninfo.Account = new Account();
                temp_sessioninfo.Account.AccountLibraryCode = "";   // strLibraryCode;
                temp_sessioninfo.Account.UserID = "~recover";   // strOperator;
                try
                {
                    LibraryServerResult result = null;
                    if (strAction == "repairreaderside")
                    {
                        // result.Value
                        //      -1  出错
                        //      0   没有必要修复
                        //      1   成功修复
                        result = this.RepairReaderSideError(
        temp_sessioninfo,
        strReaderKey,
        strItemKey,
        strConfirmItemRecPath,
        $"skipOperLog,version:{version}",
        out _);
                    }
                    else if (strAction == "repairitemside")
                    {
                        // result.Value
                        //      -1  出错
                        //      0   没有必要修复
                        //      1   成功修复
                        result = this.RepairItemSideError(
                            temp_sessioninfo,
                            strReaderKey,
                            strItemKey,
                            strConfirmItemRecPath,
                            "skipOperLog",
                            out _);
                    }
                    else
                    {
                        strError = $"无法识别的 action 值 '{strAction}'";
                        return -1;
                    }
                    if (result.Value == -1 || result.Value == 0)
                    {
                        if (robust == false)
                        {
                            strError = result.ErrorInfo;
                            return -1;
                        }
                        else
                        {
                            func_warning?.Invoke(result.ErrorInfo);
                        }
                    }

                    return 0;
                }
                finally
                {
                    temp_sessioninfo.CloseSession();
                }

#if REMOVED
                    string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                    string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerBarcode");
                    string strReaderRefID = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerRefID");
                    if (String.IsNullOrEmpty(strReaderBarcode) == true
                        && string.IsNullOrEmpty(strReaderRefID) == true)
                    {
                        strError = "readerBarcode 和 readerRefID 元素值均为空";
                        goto ERROR1;
                    }

                    string strReaderKey = dp2StringUtil.BuildReaderKey(strReaderBarcode, strReaderRefID);

                    // 读入读者记录
                    nRet = this.GetReaderRecXml(
                        channel,
                        strReaderBarcode,
                        out string strReaderXml,
                        out string strOutputReaderRecPath,
                        out byte[] reader_timestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        if (strAction == "repairreaderside")
                        {
                            strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                            goto ERROR1;
                        }

                        // 从实体侧恢复的时候，是允许读者记录不存在的
                    }
                    if (nRet == -1)
                    {
                        strError = "读入证条码号为 '" + strReaderBarcode + "' 的读者记录时发生错误: " + strError;
                        goto ERROR1;
                    }

                    XmlDocument readerdom = null;
                    if (string.IsNullOrEmpty(strReaderXml) == false)
                    {
                        nRet = LibraryApplication.LoadToDom(strReaderXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }

                    // 校验读者证条码号参数是否和XML记录中完全一致
                    if (readerdom != null)
                    {
                        string strTempBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                            "barcode");
                        if (strReaderBarcode != strTempBarcode)
                        {
                            strError = "修复操作被拒绝。因读者证条码号参数 '" + strReaderBarcode + "' 和读者记录中<barcode>元素内的读者证条码号值 '" + strTempBarcode + "' 不一致。";
                            goto ERROR1;
                        }
                    }

                    // 读入册记录
                    //string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                    //    "confirmItemRecPath");
                    string strConfirmItemRecPath = GetConfirmRecPath(domLog);

                    string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                        "itemBarcode");
                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                    {
                        strError = "<strItemBarcode>元素值为空";
                        goto ERROR1;
                    }

                    string strItemXml = "";
                    string strOutputItemRecPath = "";
                    byte[] item_timestamp = null;

                    // 如果已经有确定的册记录路径
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        lRet = channel.GetRes(strConfirmItemRecPath,
                            out strItemXml,
                            out string strMetaData,
                            out item_timestamp,
                            out strOutputItemRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                            goto ERROR1;
                        }

                        // 需要检查记录中的<barcode>元素值是否匹配册条码号


                        // TODO: 如果记录路径所表达的记录不存在，或者其<barcode>元素值和要求的册条码号不匹配，那么都要改用逻辑方法，也就是利用册条码号来获得记录。
                        // 当然，这种情况下，非常要紧的是确保数据库的素质很好，本身没有重条码号的情况出现。
                    }
                    else
                    {
                        // 从册条码号获得册记录

                        // 获得册记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetItemRecXml(
                            // Channels,
                            channel,
                            strItemBarcode,
                            out strItemXml,
                            100,
                            out List<string> aPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            if (strAction == "repairitemside")
                            {
                                strError = "册条码号 '" + strItemBarcode + "' 不存在";
                                goto ERROR1;
                            }

                            // 从读者侧恢复的时候，册条码号不存在是允许的
                            goto CONTINUE_REPAIR;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入册条码号为 '" + strItemBarcode + "' 的册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (aPath.Count > 1)
                        {

                            strError = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，但此时confirmItemRecPath却为空";
                            goto ERROR1;
                        }
                        else
                        {
                            Debug.Assert(nRet == 1, "");
                            Debug.Assert(aPath.Count == 1, "");

                            if (nRet == 1)
                            {
                                strOutputItemRecPath = aPath[0];
                            }
                        }
                    }

                CONTINUE_REPAIR:

                    XmlDocument itemdom = null;
                    if (string.IsNullOrEmpty(strItemXml) == false)
                    {
                        nRet = LibraryApplication.LoadToDom(strItemXml,
                            out itemdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载册记录进入XML DOM时发生错误: " + strError;
                            goto ERROR1;
                        }

                        // 校验册条码号参数是否和XML记录中完全一致
                        string strTempItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                            "barcode");
                        if (strItemBarcode != strTempItemBarcode)
                        {
                            strError = "修复操作被拒绝。因册条码号参数 '" + strItemBarcode + "' 和册记录中<barcode>元素内的册条码号值 '" + strTempItemBarcode + "' 不一致。";
                            goto ERROR1;
                        }
                    }

                    if (strAction == "repairreaderside")
                    {
                        XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                        if (nodeBorrow == null)
                        {
                            strError = "修复操作被拒绝。读者记录 " + strReaderBarcode + " 中并不存在有关册 " + strItemBarcode + " 的借阅信息。";
                            goto ERROR1;
                        }

                        if (itemdom != null)
                        {
                            // 看看册记录中是否有指回读者记录的链
                            string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                                "borrower");
                            if (strBorrower == strReaderBarcode)
                            {
                                strError = "修复操作被拒绝。您所请求要修复的链，本是一条完整正确的链。可直接进行普通还书操作。";
                                goto ERROR1;
                            }
                        }

                        // 移除读者记录侧的链
                        nodeBorrow.ParentNode.RemoveChild(nodeBorrow);

                        // 写回读者记录
                        lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                            readerdom.OuterXml,
                            false,
                            "content,ignorechecktimestamp",
                            reader_timestamp,
                            out byte[] output_timestamp,
                            out string strOutputPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (strAction == "repairitemside")
                    {
                        // 看看册记录中是否有指向读者记录的链
                        string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                            "borrower");
                        if (String.IsNullOrEmpty(strBorrower) == true)
                        {
                            strError = "修复操作被拒绝。您所请求要修复的册记录中，本来就没有借阅信息，因此谈不上修复。";
                            goto ERROR1;
                        }

                        if (strBorrower != strReaderBarcode)
                        {
                            strError = "修复操作被拒绝。您所请求要修复的册记录中，并没有指明借阅者是读者 " + strReaderBarcode + "。";
                            goto ERROR1;
                        }

                        // 看看读者记录中是否有指回链条。
                        if (readerdom != null)
                        {
                            XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                            if (nodeBorrow != null)
                            {
                                strError = "修复操作被拒绝。您所请求要修复的链，本是一条完整正确的链。可直接进行普通还书操作。";
                                goto ERROR1;
                            }
                        }

                        // 移除册记录侧的链
                        DomUtil.SetElementText(itemdom.DocumentElement,
                            "borrower", "");
                        DomUtil.SetElementText(itemdom.DocumentElement,
                            "borrowDate", "");
                        DomUtil.SetElementText(itemdom.DocumentElement,
                            "borrowPeriod", "");

                        // 写回册记录
                        lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                            itemdom.OuterXml,
                            false,
                            "content,ignorechecktimestamp",
                            item_timestamp,
                            out byte[] output_timestamp,
                            out string strOutputPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        strError = "不可识别的strAction值 '" + strAction + "'";
                        goto ERROR1;
                    }

                    return 0;
#endif
            }


            else
            {
                // *** 日志动作 version 1.10 和以上的快照方式

                {
                    // 准备读者记录
                    string strReaderRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "readerRecord",
                        out XmlNode node);
                    string strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                    var existing = DomUtil.GetBooleanParam(node, "existing", true);
                    // var changed = DomUtil.GetBooleanParam(container_node, "changed", true);
                    // 注: 不管当初执行 API 时读者记录有没有修改，现在恢复的时候都覆盖保存一下读者记录
                    if (string.IsNullOrEmpty(strReaderRecPath) == false
                        && existing == true
                        // && changed == true
                        && string.IsNullOrEmpty(strReaderRecord) == false)
                    {
                        // 写读者记录
                        nRet = TryWriteReaderRecord(
                            channel,
                            strReaderRecPath,
                            strReaderRecord,
                            robust,
                            func_warning,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

                {
                    // 准备册记录
                    string strItemRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "itemRecord",
                        out XmlNode node);
                    string strItemRecPath = DomUtil.GetAttr(node, "recPath");
                    var existing = DomUtil.GetBooleanParam(node, "existing", true);
                    // var changed = DomUtil.GetBooleanParam(container_node, "changed", true);
                    // 注: 不管当初执行 API 时册记录有没有修改，现在恢复的时候都覆盖保存一下册记录
                    if (string.IsNullOrEmpty(strItemRecPath) == false
                        && existing == true
                        // && changed == true
                        && string.IsNullOrEmpty(strItemRecord) == false)
                    {
                        // 写册记录
                        nRet = TryWriteItemRecord(channel,
                        strItemRecPath,
                        strItemRecord,
                        "entity",
                        robust,
                        out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }

            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverRepairBorrowInfo() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        /*
    <root>
    <operation>manageDatabase</operation>
    <action>createDatabase</action> createDatabase/initializeDatabase/refreshDatabase/deleteDatabase/changeDatabase
    <databases>
    <database type="biblio" syntax="unimarc" usage="book" role="" inCirculation="true" name="_测试用中文图书" entityDbName="_测试用中文图书实体" orderDbName="_测试用中文图书订购" commentDbName="_测试用中文图书评注" />
    </databases>
    <operator>supervisor</operator>
    <operTime>Sat, 18 Nov 2017 20:00:05 +0800</operTime>
    <clientAddress via="net.pipe://localhost/dp2library/xe">localhost</clientAddress>
    <version>1.06</version>
    </root>
         * */
        // 2017/10/15
        //      attachment  附件流对象。注意调用本函数时其文件指针在流的尾部
        public int RecoverManageDatabase(
            RmsChannelCollection Channels,
            RecoverLevel level,
            XmlDocument domLog,
            Stream attachmentLog,
            string strStyle,
            Delegate_warning func_warning,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            // long lRet = 0;

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 2024/2/22
            if (this.Changed)
                this.Flush();

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                string strTempFileName = "";
                if (attachmentLog != null)
                {
                    strTempFileName = this.GetTempFileName("db");
                    using (Stream target = File.Create(strTempFileName))
                    {
                        attachmentLog.Seek(0, SeekOrigin.Begin);
                        attachmentLog.CopyTo(target);
                    }
                }


                string strTempDir = Path.Combine(this.TempDir, "~rcvdb");
                PathUtil.CreateDirIfNeed(strTempDir);

                this.LockForWrite();
                try
                {
                    bool bDbNameChanged = false;

                    string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                        "action");
                    string strLibraryCode = DomUtil.GetElementText(domLog.DocumentElement,
                        "libraryCode");
                    string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
                        "operator");
                    string style = DomUtil.GetElementText(domLog.DocumentElement,
    "style");
                    string version = DomUtil.GetElementText(domLog.DocumentElement,
    "version");

                    SessionInfo temp_sessioninfo = new SessionInfo(this);
                    temp_sessioninfo.Account = new Account();
                    temp_sessioninfo.Account.AccountLibraryCode = strLibraryCode;
                    temp_sessioninfo.Account.UserID = strOperator;
                    try
                    {
                        if (strAction == "createDatabase")
                        {
                            List<string> skip_warnings = new List<string>();
                            List<string> succeed_dbNames = new List<string>();
                            // *** 创建数据库
                            nRet = DatabaseUtility.CreateDatabases(
                                null,   // stop
                                channel,
                                strTempFileName,
                                strTempDir,
                                "continueLoop",
                                null,
                                (database_name) =>
                                {
                                    succeed_dbNames.Add(database_name);
                                    bDbNameChanged = true;
                                },
                                out strError);
                            if (nRet == -1)
                            {
                                // 一个也没有成功
                                if (succeed_dbNames.Count == 0)
                                    return -1;
                                skip_warnings.Add(strError);
                            }

                            if (succeed_dbNames.Count > 0)
                            {
                                // 更新 library.xml 内容
                                var nodes = succeed_dbNames.Select(o => domLog.DocumentElement.SelectSingleNode($"databases/database[@name='{o}']")).ToList().Cast<XmlElement>().Where(o => o != null);
                                // XmlNodeList nodes = domLog.DocumentElement.SelectNodes("databases/database");
                                nRet = AppendDatabaseElement(this.LibraryCfgDom,
                    nodes,
                    out strError);
                                if (nRet == -1)
                                    return -1;
                                this.Changed = true;
                            }
                            if (skip_warnings.Count > 0)
                            {
                                this.Flush();
                                strError = StringUtil.MakePathList(skip_warnings, "; ");
                                return -1;
                            }
                        }
                        else if (strAction == "changeDatabase")
                        {
                            // *** 修改数据库
                            /*
    - <root>
      <operation>manageDatabase</operation> 
      <action>changeDatabase</action> 
    - <databases>
        <database type="biblio" syntax="unimarc" usage="book" name="中文图书改名" oldName="中文图书" /> 
      </databases>
      <operator>supervisor</operator> 
      <operTime>Fri, 23 Feb 2024 18:44:24 +0800</operTime> 
      <clientAddress via="net.pipe://localhost/dp2library/xe">localhost</clientAddress> 
      <version>1.10</version> 
      </root>
                            注：1.10 之前版本的 changeDatabase action 的日志记录缺乏信息。
                            databases/database 元素中缺乏 oldName 属性，因此看不出改名以前的旧名字是什么，前滚日志动作没法进行。
                            1.10 (以及)更高版本的日志记录才改正了这个 bug
                            * 
                             * */
                            if (StringUtil.CompareVersion(version, "1.10") < 0)
                            {
                                strError = $"action 为 changeDatabase 的日志动作因为版本低于 1.10(为 {version})，缺乏 databases/database/@oldName 属性，无法执行日志前滚，特此警告";
                                return -1;
                            }
                            // 注意处理 attach 和 detach 风格。或者明确报错不予处理
                            // TODO: 操作日志中没有记载改名以前的数据库名

                            XmlNodeList oldNames = domLog.DocumentElement.SelectNodes("databases/database/@oldName");
                            string strDatabaseNames = StringUtil.MakePathList(oldNames.Cast<XmlNode>().Select(o => o.Value).ToList(), ",");
                            string strDatabaseInfo = domLog.DocumentElement.SelectSingleNode("databases")?.OuterXml;

                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   成功
                            nRet = this.ChangeDatabase(
                                temp_sessioninfo,
                                channel,
                                strLibraryCode,
                                strDatabaseNames,
                                strDatabaseInfo,
                                MergeStyle(strStyle, style, "skipOperLog,continueLoop"),
                                out string strOutputInfo,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            // 注: ChangeDatabase() 中已经使 this.Changed = true 了

                        }
                        else if (strAction == "initializeDatabase")
                        {
                            // *** 初始化数据库

                            XmlNodeList names = domLog.DocumentElement.SelectNodes("databases/database/@name");
                            string strDatabaseNames = StringUtil.MakePathList(names.Cast<XmlNode>().Select(o => o.Value).ToList(), ",");

                            List<string> notexist_dbnames = new List<string>();
                            // 初始化数据库
                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   成功
                            nRet = this.InitializeDatabase(
                                temp_sessioninfo,
                                channel,
                                strLibraryCode,
                                strDatabaseNames,
                                MergeStyle(strStyle, style, "skipOperLog"),
                                (name) => {
                                    notexist_dbnames.Add(name);
                                    return true;
                                },
                                out string strOutputInfo,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            // 注: InitializeDatabase() 中似乎不会改变 this.Changed

                            // 2024/5/12
                            // 对于当前不存在的数据库，执行重新创建
                            if (notexist_dbnames.Count > 0)
                            {
                                if (robust == false)
                                {
                                    strError = $"初始化数据库时发现下列数据库的定义不存在({StringUtil.MakePathList(notexist_dbnames, ",")})，无法完成初始化";
                                    return -1;
                                }

                                func_warning?.Invoke($"初始化数据库时发现下列数据库的定义不存在({StringUtil.MakePathList(notexist_dbnames, ",")})，正在尝试重新创建这些数据库");

                                List<string> skip_warnings = new List<string>();
                                List<string> succeed_dbNames = new List<string>();
                                // *** 创建数据库
                                nRet = DatabaseUtility.CreateDatabases(
                                    null,   // stop
                                    channel,
                                    strTempFileName,
                                    strTempDir,
                                    "continueLoop",
                                    notexist_dbnames,
                                    (database_name) =>
                                    {
                                        succeed_dbNames.Add(database_name);
                                        bDbNameChanged = true;
                                    },
                                    out strError);
                                if (nRet == -1)
                                {
                                    // 一个也没有成功
                                    if (succeed_dbNames.Count == 0)
                                    {
                                        strError = $"初始化数据库时发现下列数据库的定义不存在({StringUtil.MakePathList(notexist_dbnames, ",")})，然后尝试重新创建这些数据库时，一个也没有创建成功";
                                        return -1;
                                    }
                                    skip_warnings.Add(strError);
                                }

                                if (succeed_dbNames.Count > 0)
                                {
                                    // 更新 library.xml 内容
                                    var nodes = succeed_dbNames.Select(o => domLog.DocumentElement.SelectSingleNode($"databases/database[@name='{o}']")).ToList().Cast<XmlElement>().Where(o => o != null);
                                    // XmlNodeList nodes = domLog.DocumentElement.SelectNodes("databases/database");
                                    nRet = AppendDatabaseElement(this.LibraryCfgDom,
                        nodes,
                        out strError);
                                    if (nRet == -1)
                                    {
                                        strError = $"初始化数据库时发现下列数据库的定义不存在({StringUtil.MakePathList(notexist_dbnames, ",")})，然后尝试重新创建这些数据库时遇到报错: {strError}";
                                        return -1;
                                    }
                                    this.Changed = true;
                                }
                                if (skip_warnings.Count > 0)
                                {
                                    this.Flush();
                                    strError = $"初始化数据库时发现下列数据库的定义不存在({StringUtil.MakePathList(notexist_dbnames, ",")})，然后尝试重新创建这些数据库时遇到报错: " + StringUtil.MakePathList(skip_warnings, "; ");
                                    return -1;
                                }
                            }
                        }
                        else if (strAction == "refreshDatabase")
                        {
                            // *** 刷新数据库定义
                            // 指从 templates 目录中用最新的配置文件刷新数据库原有的配置文件

                            XmlNodeList names = domLog.DocumentElement.SelectNodes("databases/database/@name");
                            string strDatabaseNames = StringUtil.MakePathList(names.Cast<XmlNode>().Select(o => o.Value).ToList(), ",");

                            {
                                // 日志前滚的时候，要压制 autoRebuildKeys 和 recoverModeKeys 这两个属性
                                var refreshStyle_element = domLog.DocumentElement.SelectSingleNode("refreshStyle") as XmlElement;
                                if (refreshStyle_element != null)
                                {
                                    var auto_rebuild_keys = refreshStyle_element.GetAttribute("autoRebuildKeys");
                                    // TODO: 可以把 auto_rebuild_keys 所针对的数据库名字记载积累下来，在日志恢复任务结束时，再安排一次重建检索点后台任务

                                    refreshStyle_element.SetAttribute("autoRebuildKeys", "false");
                                    refreshStyle_element.SetAttribute("recoverModeKeys", "false");
                                }
                            }

                            string strDatabaseInfo = domLog.DocumentElement.SelectSingleNode("refreshStyle")?.OuterXml;

                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   成功
                            nRet = this.RefreshDatabaseDefs(
                                temp_sessioninfo,
                                channel,
                                strLibraryCode,
                                strDatabaseNames,
                                strDatabaseInfo,
                                MergeStyle(strStyle, style, "skipOperLog,continueLoop"),
                                out string strOutputInfo,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            // 注: RefreshDatabaseDefs() 中似乎不会改变 this.Changed
                        }
                        else if (strAction == "deleteDatabase")
                        {
                            // *** 删除数据库
                            List<string> dbnames = new List<string>();

                            XmlNodeList databases = domLog.DocumentElement.SelectNodes("databases/database");
                            foreach (XmlElement database in databases)
                            {
                                var name = database.GetAttribute("name");
                                var biblioDbName = database.GetAttribute("biblioDbName");
                                if (string.IsNullOrEmpty(biblioDbName) == false)
                                {
                                    var parent_node = domLog.DocumentElement.SelectSingleNode($"databases/database[@name='{biblioDbName}']") as XmlElement;
                                    if (parent_node != null)
                                        continue;   // 如果是书目库的子库，则跳过。因为删除书目库本身就会连带着删除这个子库
                                }
                                dbnames.Add(name);
                            }

                            nRet = DeleteDatabases(
                                null,
                                channel,
                                dbnames,
                                MergeStyle(strStyle, style, "skipOperLog,continueLoop") + (robust ? "" : ",notExistingAsError"),
                                ref bDbNameChanged,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else if (strAction == "compressTailNo")
                        {
                            // *** 压缩数据库尾号
                            var strDatabaseNames = DomUtil.GetElementText(domLog.DocumentElement,
                                "databaseNames");

                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   成功
                            nRet = this.CompressTailNo(
                                temp_sessioninfo,
                                channel,
                                strLibraryCode,
                                strDatabaseNames,
                                MergeStyle(strStyle, style, "skipOperLog"),
                                out string strOutputInfo,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            // 注: CompressTailNo() 中似乎不会改变 this.Changed
                        }
                        else
                        {
                            strError = "不可识别的strAction值 '" + strAction + "'";
                            goto ERROR1;
                        }
                    }
                    finally
                    {
                        temp_sessioninfo.CloseSession();
                    }

                    if (this.Changed == true)
                    {
                        this.Flush();   // 2024/2/22
                                        // this.ActivateManagerThread();
                    }

                    if (bDbNameChanged == true)
                    {
                        nRet = InitialKdbs(
                            channel,    // Channels,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        // 重新初始化虚拟库定义
                        this.vdbs = null;
                        nRet = this.InitialVdbs(channel,    // Channels,
                            out strError);
                        if (nRet == -1)
                        {
                            this.WriteErrorLog($"*** LogRecover 过程中出现致命错误，请在日志恢复完成后，手动修正故障: {strError}");
                            return -1;
                        }
                    }
                    return 0;
                }
                finally
                {
                    this.UnlockForWrite();

                    if (string.IsNullOrEmpty(strTempDir) == false)
                    {
                        PathUtil.RemoveReadOnlyAttr(strTempDir);    // 避免 .zip 文件中有有只读文件妨碍删除
                        PathUtil.DeleteDirectory(strTempDir);
                    }
                    if (string.IsNullOrEmpty(strTempFileName) == false)
                        File.Delete(strTempFileName);
                }
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // 和SnapShot方式相同
                bReuse = true;
                goto DO_SNAPSHOT;
            }
            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverManageDatabase() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        // 合并 style
        static string MergeStyle(params string[] styles)
        {
            if (styles == null)
                return "";
            List<string> results = new List<string>();
            foreach (var style in styles)
            {
                var list = StringUtil.SplitList(style, ",");
                if (list.Count > 0)
                    results.AddRange(list);
            }

            return StringUtil.MakePathList(results, ",");
        }

        // 用于日志前滚的删除数据库功能
        // parameters:
        //      strStyle    如果包含 continueLoop，表示中途遇到出错后尽量继续循环处理后续任务
        //                  如果包含 notExistingAsError，遇到数据库不存在时要报错
        int DeleteDatabases(
            Stop stop,
            RmsChannel channel,
            List<string> dbnames,
            string strStyle,
            ref bool bDbNameChanged,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 遇到报错是否尽量维持循环，以继续完成后面的处理
            var continueLoop = StringUtil.IsInList("continueLoop", strStyle);
            List<string> skip_warnings = new List<string>();

            foreach (string dbname in dbnames)
            {
                string strDbType = GetDbTypeByDbName(dbname);
                if (string.IsNullOrEmpty(dbname))
                {
                    // TODO: 遇到此种情况，写入错误日志
                    strError = "数据库 '" + dbname + "' 没有找到类型";
                    // return -1;
                    skip_warnings.Add(strError);
                    continue;
                }

                if (strDbType == "biblio")
                {
                    // 删除一个书目库。
                    // 根据书目库的库名，在 library.xml 的 itemdbgroup 中找出所有下属库的库名，然后删除它们
                    // return:
                    //      -1  出错
                    //      0   指定的数据库不存在
                    //      1   成功删除
                    nRet = this.DeleteBiblioDatabase(
                        channel,
                        "",
                        dbname,
                        "",
                        null,
                        ref bDbNameChanged,
                        out strError);
                    if (nRet == -1)
                    {
                        if (continueLoop)
                        {
                            skip_warnings.Add(strError);
                            continue;
                        }
                        return -1;
                    }
                    // 2024/3/25
                    if (nRet == 0 && StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;

                    if (StringUtil.IsInList("verify", strStyle))
                    {
                        if (this.VerifyDatabaseDelete(
                            channel,
                            strDbType,
                            dbname,
                            out strError) == -1)
                            return -1;
                    }
                    continue;
                }

                if (/*strDbType == "entity"
                    || strDbType == "order"
                    || strDbType == "issue"
                    || strDbType == "comment"*/
                    ServerDatabaseUtility.IsBiblioSubType(strDbType))
                {
                    // return:
                    //      -1  出错
                    //      0   指定的数据库不存在
                    //      1   成功删除
                    nRet = DeleteBiblioChildDatabase(channel,
    "", // strLibraryCodeList,
    dbname,
    "", // strLogFileName,
    ref bDbNameChanged,
    out strError);
                    if (nRet == -1)
                    {
                        if (continueLoop)
                        {
                            skip_warnings.Add(strError);
                            continue;
                        }
                        return -1;
                    }
                    // 2024/3/25
                    if (nRet == 0 && StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;

                    if (StringUtil.IsInList("verify", strStyle))
                    {
                        if (this.VerifyDatabaseDelete(
                            channel,
                            strDbType,
                            dbname,
                            out strError) == -1)
                            return -1;
                    }
                    continue;
                }

                // 2024/2/19
                // 删除读者库
                if (strDbType == "reader")
                {
                    // 删除读者库。
                    // 也会自动修改 library.xml 的 readerdbgroup 中相关元素
                    // parameters:
                    //      bDbNameChanged  如果数据库发生了删除或者修改名字的情况，此参数会被设置为 true。否则其值不会发生改变
                    // return:
                    //      -1  出错
                    //      0   指定的数据库不存在
                    //      1   成功删除
                    nRet = DeleteReaderDatabase(channel,
                        "",
                        dbname,
                        null,   // strLogFileName,
                        ref bDbNameChanged,
                        out strError);
                    if (nRet == -1)
                    {
                        if (continueLoop)
                        {
                            skip_warnings.Add(strError);
                            continue;
                        }
                        return -1;
                    }
                    // 2024/3/25
                    if (nRet == 0 && StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;

                    if (StringUtil.IsInList("verify", strStyle))
                    {
                        // test
                        // strError = "test 验证发生错误";
                        // return -1;
                        if (this.VerifyDatabaseDelete(channel,
                            strDbType,
                            dbname,
                            out strError) == -1)
                            return -1;
                    }
                    continue;
                }

                if (/*strDbType == "arrived"
                    || strDbType == "amerce"
                    || strDbType == "invoice"
                    || strDbType == "pinyin"
                    || strDbType == "gcat"
                    || strDbType == "word"
                    || strDbType == "message"*/
                    ServerDatabaseUtility.IsSingleDbType(strDbType))
                {
                    // 删除一个单独类型的数据库。
                    // 也会自动修改 library.xml 的相关元素
                    // parameters:
                    //      strLibraryCodeList  当前用户所管辖的分馆代码列表
                    //      bDbNameChanged  如果数据库发生了删除或者修改名字的情况，此参数会被设置为 true。否则其值不会发生改变
                    // return:
                    //      -1  出错
                    //      0   指定的数据库不存在
                    //      1   成功删除
                    nRet = DeleteSingleDatabase(channel,
                        "", // strLibraryCodeList,
                        dbname,
                        "", // strLogFileName,
                        ref bDbNameChanged,
                        out strError);
                    if (nRet == -1)
                    {
                        if (continueLoop)
                        {
                            skip_warnings.Add(strError);
                            continue;
                        }
                        return -1;
                    }
                    // 2024/3/25
                    if (nRet == 0 && StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;

                    if (StringUtil.IsInList("verify", strStyle))
                    {
                        // test
                        // strError = "test 验证发生错误";
                        // return -1;
                        if (this.VerifyDatabaseDelete(channel,
                            strDbType,
                            dbname,
                            out strError) == -1)
                            return -1;
                    }
                    continue;
                }

                if (ServerDatabaseUtility.IsUtilDbName(this.LibraryCfgDom, dbname, out _) == true)
                {
                    // 删除一个实用库。
                    // 也会自动修改 library.xml 的相关元素
                    // parameters:
                    //      strLibraryCodeList  当前用户所管辖的分馆代码列表
                    //      bDbNameChanged  如果数据库发生了删除或者修改名字的情况，此参数会被设置为 true。否则其值不会发生改变
                    // return:
                    //      -1  出错
                    //      0   指定的数据库不存在
                    //      1   成功删除
                    nRet = DeleteUtilDatabase(channel,
                        "", // strLibraryCodeList,
                        dbname,
                        "", // strLogFileName,
                        ref bDbNameChanged,
                        out strError);
                    if (nRet == -1)
                    {
                        if (continueLoop)
                        {
                            skip_warnings.Add(strError);
                            continue;
                        }
                        return -1;
                    }
                    // 2024/3/25
                    if (nRet == 0 && StringUtil.IsInList("notExistingAsError", strStyle))
                        return -1;

                    if (StringUtil.IsInList("verify", strStyle))
                    {
                        Debug.Assert(string.IsNullOrEmpty(strDbType) == false, "");
                        if (this.VerifyDatabaseDelete(channel,
                            strDbType,
                            dbname,
                            out strError) == -1)
                            return -1;
                    }
                    continue;
                }

                strError = "DeleteDatabases() 遭遇无法识别的数据库名 '" + dbname + "' (数据库类型 '" + strDbType + "')";
                if (continueLoop)
                {
                    if (StringUtil.IsInList("notExistingAsError", strStyle) == true)
                        skip_warnings.Add(strError);
                    continue;
                }

                return -1;
            }

            if (skip_warnings.Count > 0)
            {
                strError = StringUtil.MakePathList(skip_warnings, "; ");
                return -1;
            }
            return 0;
        }

        // 2024/2/22
        /*
    <root>
    <operation>setSystemParameter</operation> 
    <category>...</category> 
    <name>...</name> 
    <value>...</value> 
    <libraryCodeList>...</libraryCodeList>
    <operator>...</operator> 
    <operTime>Thu, 22 Feb 2024 15:38:40 +0800</operTime> 
    <clientAddress via="...">...</clientAddress> 
    <version>1.10</version> 
    </root>
        * */
        public int RecoverSetSystemParameter(
        RmsChannelCollection Channels,
        RecoverLevel level,
        XmlDocument domLog,
        Stream attachmentLog,
        string strStyle,
        out string strError)
        {
            strError = "";
            int nRet = 0;
            // long lRet = 0;

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 2024/2/22
            if (this.Changed)
                this.Flush();

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                string category = DomUtil.GetElementText(domLog.DocumentElement,
                    "category");
                string name = DomUtil.GetElementText(domLog.DocumentElement,
                    "name");
                string value = DomUtil.GetElementText(domLog.DocumentElement,
                    "value");
                string libraryCodeList = DomUtil.GetElementText(domLog.DocumentElement,
                    "libraryCodeList");
                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
    "operator");
                SessionInfo temp_sessioninfo = new SessionInfo(this);
                temp_sessioninfo.Account = new Account();
                temp_sessioninfo.Account.AccountLibraryCode = libraryCodeList;
                temp_sessioninfo.Account.UserID = strOperator;

                try
                {
                    nRet = this.SetSystemParameter(
        temp_sessioninfo,
        category,
        name,
        value,
        out bool succeed,
        out strError);
                    if (nRet == -1)
                    {
                        return -1;
                    }
                    if (succeed && this.Changed)
                        this.Flush();
                    return 0;
                }
                finally
                {
                    temp_sessioninfo.CloseSession();
                }
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // 和SnapShot方式相同
                bReuse = true;
                goto DO_SNAPSHOT;
            }
            return 0;
        ERROR1:
#if REMOVED
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetSystemParameter() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
#endif
            return -1;
        }

        /* version 1.10 才增加的日志操作类型
    <root>
    <operation>setCalendar</operation> 
    <action>...</action> change new delete overwirte
    <oldCalendar>...</oldCalendar> 
    <calendar>...</calendar> 
    <operator>...</operator> 
    <operTime>Thu, 22 Feb 2024 15:38:40 +0800</operTime> 
    <clientAddress via="...">...</clientAddress> 
    <version>1.10</version> 
    </root>
    * */
        public int RecoverSetCalendar(
    RmsChannelCollection Channels,
    RecoverLevel level,
    XmlDocument domLog,
    Stream attachmentLog,
    string strStyle,
    out string strError)
        {
            strError = "";
            int nRet = 0;
            // long lRet = 0;

            /*
            // 暂时把Robust当作Logic处理
            if (level == RecoverLevel.Robust)
                level = RecoverLevel.Logic;
            */

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 2024/2/22
            if (this.Changed)
                this.Flush();

            bool bReuse = false;    // 是否能够不顾RecoverLevel状态而重用部分代码

        DO_SNAPSHOT:

            // 快照恢复
            if (IsSnapshot(level, out bool robust)
                || bReuse == true)
            {
                string action = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");
                XmlElement calendar_element = null;
                if (action == "delete")
                {
                    calendar_element = domLog.DocumentElement.SelectSingleNode("oldCalendar") as XmlElement;
                    if (calendar_element == null)
                    {
                        strError = "日志记录中缺乏 oldCalendar 元素";
                        return -1;
                    }
                }
                else
                {
                    calendar_element = domLog.DocumentElement.SelectSingleNode("calendar") as XmlElement;
                    if (calendar_element == null)
                    {
                        strError = "日志记录中缺乏 calendar 元素";
                        return -1;
                    }
                }


                var info = GetElementValues(calendar_element);
                /*
                var info = new CalenderInfo();
                {
                    info.Name = DomUtil.GetElementText(domLog.DocumentElement,
                        "calendarName");
                    info.Range = DomUtil.GetElementText(domLog.DocumentElement,
                        "calendarRange");
                    info.Content = DomUtil.GetElementText(domLog.DocumentElement,
        "calendarContent");
                    info.Comment = DomUtil.GetElementText(domLog.DocumentElement,
        "calendarComment");
                }
                */
                string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
    "operator");

                SessionInfo temp_sessioninfo = new SessionInfo(this);
                temp_sessioninfo.Account = new Account();
                temp_sessioninfo.Account.AccountLibraryCode = "";
                temp_sessioninfo.Account.UserID = "~recover";

                try
                {
                    nRet = this.SetCalendar(
        temp_sessioninfo,
        action,
        info,
        "skipOperLog,recover",
        out ErrorCode error_code,
        out strError);
                    if (nRet == -1)
                        return -1;
                    if (this.Changed)
                        this.Flush();
                    return 0;
                }
                finally
                {
                    temp_sessioninfo.CloseSession();
                }
            }

            // 逻辑恢复或者混合恢复
            if (/*level == RecoverLevel.Logic
                || level == RecoverLevel.LogicAndSnapshot*/
                IsLogic(level, out robust))
            {
                // 和SnapShot方式相同
                bReuse = true;
                goto DO_SNAPSHOT;
            }
            return 0;
            /*
        ERROR1:
            if (level == RecoverLevel.LogicAndSnapshot)
            {
                WriteErrorLog($"RecoverSetCalendar() 用 LogicAndSnapShot 方式恢复遇到报错 {strError}，后面自动改用 SnapShot 方式尝试 ...");
                level = RecoverLevel.Snapshot;
                goto DO_SNAPSHOT;
            }
            return -1;
            */
        }

    }

    // 日志恢复级别
    // 注: 所谓快照就是主要用记录路径定位恢复; 所谓逻辑就是主要用参考ID或条码号定位恢复记录。
    public enum RecoverLevel
    {
        /*
        Logic = 0,  // 逻辑操作
        LogicAndSnapshot = 1,   // 逻辑操作，若失败则转用快照恢复
        Snapshot = 3,   // （完全的）快照
        Robust = 4, // 最强壮的容错恢复方式
        */
        Snapshot = 0x01,   // （纯粹的）快照
        Logic = 0x02,  // (纯粹的) 逻辑操作
        SnapshotRobust = 0x11,   // 容错的快照
        LogicRobust = 0x12, // 容错的逻辑
        RobustMask = 0x10,
    }
}
