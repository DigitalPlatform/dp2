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
        public LibraryServerResult SetRecordInfo(
SessionInfo sessioninfo,
string strRecPath,
string strXml,
string strMetadata,
string strStyle,
byte[] baTimestamp,
out string strOutputRecPath,
out byte[] baOutputTimestamp)
        {
            strOutputRecPath = null;
            baOutputTimestamp = null;

            LibraryServerResult result = new LibraryServerResult();

            string strError = "";
            int nRet = 0;
            long lRet = 0;

            var strDbName = ResPath.GetDbName(strRecPath);
            var db_type = GetAllDbType(strDbName);
            if (string.IsNullOrEmpty(db_type))
            {
                strError = $"无法识别路径 '{strRecPath}' 中数据库 '{strDbName}' 的类型";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            bool bExist = true;    // strRecPath 所指的记录是否存在?
            bool append = ResPath.IsAppendRecPath(strRecPath);

            string strExistXml = "";
            byte[] exist_timestamp = null;
            int nRedoCount = 0;
        REDOLOAD:
            if (append == false)
            {
                lRet = channel.GetRes(strRecPath,
        out strExistXml,
        out _,  // strMetaData,
        out exist_timestamp,
        out strOutputRecPath,
        out strError);
                if (lRet == -1)
                {
                    if (channel.IsNotFoundOrDamaged())
                    {
                        // 如果记录不存在, 则构造一条空的记录
                        bExist = false;
                        strExistXml = "<root />";
                        exist_timestamp = null;
                        // strOutputPath = info.NewRecPath;
                    }
                    else
                    {
                        strError = "保存操作发生错误, 在读入原有记录阶段:" + strError;
                        goto ERROR1;
                    }
                }
            }
            else
            {
                bExist = false;
                strExistXml = "<root />";
                exist_timestamp = null;
            }

            // 把两个记录装入DOM
            XmlDocument domExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                if (string.IsNullOrEmpty(strExistXml))
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
                domNew.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "strXml 装载进入 DOM 时发生错误: " + ex.Message;
                goto ERROR1;
            }

            if (bExist)
            {
                // 观察时间戳是否发生变化
                nRet = ByteArray.Compare(baTimestamp, exist_timestamp);
                if (nRet != 0)
                {
                    // 时间戳不相等了
                    baOutputTimestamp = exist_timestamp;

                    if (bExist == false)
                        result.ErrorInfo = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 已被删除。";
                    else
                        result.ErrorInfo = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 已发生过修改";
                    result.ErrorCode = ErrorCode.TimestampMismatch;
                    return result;
                }
            }

            string part_type = "";
            string strWarning = "";

            bool bChangePartDeniedParam = false;
            // 合并新旧记录
            // 注: 本函数修改的是 domNew。另有一个函数 MergeNewOldRec()，和本函数效果对应，修改的是 domOld
            // parameters:
            //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
            //      domOld  旧记录。
            //      domNew  新记录。函数执行后其内容会被改变
            // return:
            //      -1  error
            //      0   new record not changed
            //      1   new record changed
            nRet = ItemDatabase.MergeOldNewRec(
                db_type,
                sessioninfo.RightsOrigin,
                domExist,
                domNew,
                ref bChangePartDeniedParam,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (bChangePartDeniedParam == true)
            {
                strWarning = strError;
            }

            /*
            // ??
            // 合并新旧记录
            // return:
            //      -1  出错
            //      0   正确
            //      1   有部分修改没有兑现。说明在strError中
            //      2   全部修改都没有兑现。说明在strError中 (2018/10/9)
            nRet = MergeTwoItemXml(
                sessioninfo,
                domExist,
                domNew,
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1 || nRet == 2)
            {
                if (nRet == 1)
                    part_type = "部分";
                else
                    part_type = "全部都没有";
                strWarning = strError;
            }

            // 为了后面的 CheckParent
            domNew.LoadXml(strNewXml);
            */

            string strNewXml = "";
            if (domNew.DocumentElement != null)
                strNewXml = domNew.DocumentElement.OuterXml;
            else
                strNewXml = domNew.OuterXml;

            // 保存新记录
            lRet = channel.DoSaveTextRes(strRecPath,
                strNewXml,
                false,   // include preamble?
                strMetadata,
                "content",
                exist_timestamp,
                out baOutputTimestamp,
                out strOutputRecPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        /*
                        strError = "反复保存均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                        */
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.TimestampMismatch;
                        return result;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                strError = "保存操作发生错误:" + strError;
                ConvertKernelErrorCode(channel.ErrorCode, ref result);
                result.Value = -1;
                result.ErrorInfo = strError;
                return result;
            }
            else // 成功
            {
                // TODO: 为 WriteRes 的操作日志增加一种新的形态，记忆 strXml 而不是 baContent

                /*
                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "change");

                // 新记录
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // 旧记录
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);
                */

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                // baOutputTimestamp = output_timestamp;
                // error.NewRecord = strNewXml;

                // TODO: 是否要返回实际保存的 strNewXml 给调主，以便调主可以写操作日志?

                result.ErrorInfo = "保存操作成功。NewTimeStamp 中返回了新的时间戳，NewRecord 中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                if (string.IsNullOrEmpty(strWarning) == false)
                {
                    result.ErrorInfo = "保存操作" + part_type + "兑现。" + strWarning;
                    result.ErrorCode = ErrorCode.PartialDenied;
                }
                else
                    result.ErrorCode = ErrorCode.NoError;
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 读出违约金库、盘点库等杂项数据库记录
        // 关键点是对读出的原始 XML 数据字段进行必要的权限和身份过滤
        public LibraryServerResult GetRecordInfo(
            SessionInfo sessioninfo,
            string strResPath,
            string strStyle,
            out string xml,
            out string strMetadata,
            out byte[] baOutputTimestamp,
            out string strOutputResPath)
        {
            xml = null;
            strMetadata = null;
            baOutputTimestamp = null;
            strOutputResPath = null;

            LibraryServerResult result = new LibraryServerResult();

            string strError = "";
            int nRet = 0;

            var strDbName = ResPath.GetDbName(strResPath);
            var db_type = GetAllDbType(strDbName);
            if (string.IsNullOrEmpty(db_type))
            {
                strError = $"无法识别路径 '{strResPath}' 中数据库 '{strDbName}' 的类型";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // TODO: 建议这里抽取出一个函数 GetAmerceInfo()，里面包含检查分馆权限的功能。FilterResultSet() 那里也可以用上这个抽取出来的函数
            long lRet = channel.GetRes(strResPath,
                strStyle + ",data,outputpath", // 确保可以获取到记录 XML 和 strOutputResPath
                out string item_xml,
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

            XmlDocument existing_dom = new XmlDocument();
            try
            {
                existing_dom.LoadXml(item_xml);
            }
            catch (Exception ex)
            {
                strError = "违约金记录 '" + strOutputResPath + "' 装入XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            if (db_type == "amerce")
            {
                // 检查当前账户是否有查看一条违约金记录的权限
                // return:
                //      -1  出错
                //      0   不具备权限
                //      1   具备权限
                nRet = HasAmerceReadRight(
                    sessioninfo,
                    strOutputResPath,
                    existing_dom,
                    out strError);
                if (nRet != 1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (db_type == "arrived")
            {
                nRet = HasArrivedReadRight(
                    sessioninfo,
                    strOutputResPath,
                    existing_dom,
                    out strError);
                if (nRet != 1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            bool changed = false;

            // 如果不具备 get...object 权限过滤掉 dprms:file
            if (StringUtil.IsInList($"get{db_type}object,getobject", sessioninfo.RightsOrigin) == false)
            {
                if (LibraryApplication.RemoveDprmsFile(existing_dom))
                    changed = true;
            }

            if (StringUtil.IsInList("data", strStyle))
                xml = item_xml;

            if (StringUtil.IsInList("outputpath", strStyle) == false)
                strOutputResPath = null;

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 2023/2/8
        // 检查当前账户是否有查看一条预约到书记录的权限
        // return:
        //      -1  出错
        //      0   不具备权限
        //      1   具备权限
        public int HasArrivedReadRight(
            SessionInfo sessioninfo,
            string strArrivedRecPath,
            XmlDocument arrived_dom,
            out string strError)
        {
            strError = "";

            // 注意这里要获得原始的 getreaderinfo:，因为并不在意 file 元素的权限
            var level = StringUtil.GetParameterByPrefix(sessioninfo.RightsOrigin, "getreaderinfo");
            if (level == null)
            {
                if (StringUtil.IsInList("borrow,return,reservation", sessioninfo.RightsOrigin) == false)
                {
                    strError = "当前账户不具备 getreaderinfo 或 borrow return reservation 权限";
                    return 0;
                }
            }

            // 读者只能看自己的预约到书记录
            if (sessioninfo.UserType == "reader")
            {
                // 证条码号
                string strReaderBarcode = DomUtil.GetElementText(arrived_dom.DocumentElement, "readerBarcode");
                if (sessioninfo.Account == null)
                {
                    strError = "sessioninfo.Account == null";
                    return -1;
                }
                if (sessioninfo.Account?.Barcode != strReaderBarcode)
                {
                    strError = "读者身份不能查看其他人的预约到书记录";
                    return 0;
                }
            }

            // 当前用户只能获取和管辖的馆代码关联的预约到书记录
            // 具体来说，就是预约到书记录涉及到的读者和册都要被当前账户管辖
            if (sessioninfo.GlobalUser == false)
            {
                // 读者所在馆代码
                string strLibraryCode = DomUtil.GetElementText(arrived_dom.DocumentElement, "libraryCode");
                // 册条码号
                string strItemBarcode = DomUtil.GetElementText(arrived_dom.DocumentElement, "itemBarcode");
                if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                {
                    // 进一步判断册记录是否在当前用户管辖范围内
                    if (string.IsNullOrEmpty(strItemBarcode) == false)
                    {
                        // return:
                        //      -1  errpr
                        //      0   不在控制范围
                        //      1   在控制范围
                        int nRet = this.IsItemInControl(
                            sessioninfo,
                            // channel,
                            strItemBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"预约到书记录 '{strArrivedRecPath}' 超出当前用户管辖范围，并且在尝试检索册记录 '{strItemBarcode}' 时遇到问题: {strError}";
                            return -1;  // AceessDenied and error
                        }
                        if (nRet == 1)
                        {
                            return 1;
                        }
                    }
                    strError = "预约到书记录 '" + strArrivedRecPath + "' 超出当前用户管辖范围，无法获取";
                    return 0;
                }
            }
            return 1;
        }

        // 2022/11/3
        // 检查当前账户是否有查看一条违约金记录的权限
        // return:
        //      -1  出错
        //      0   不具备权限
        //      1   具备权限
        public int HasAmerceReadRight(
            SessionInfo sessioninfo,
            string strAmerceRecPath,
            XmlDocument amerce_dom,
            out string strError)
        {
            strError = "";

            if (StringUtil.IsInList("getamerceinfo", sessioninfo.RightsOrigin) == false)
            {
                strError = "当前账户不具备 getamerceinfo 权限";
                return 0;
            }

            // 2023/2/9
            // 读者身份只能获得自己的违约金记录
            if (sessioninfo.UserType == "reader")
            {
                string strReaderBarcode = DomUtil.GetElementText(amerce_dom.DocumentElement, "readerBarcode");
                if (sessioninfo.Account == null)
                {
                    strError = "sessioninfo.Account == null";
                    return -1;
                }
                if (sessioninfo.Account.Barcode != strReaderBarcode)
                {
                    strError = "读者身份不允许查看其他人的违约金记录";
                    return 0;
                }
            }

            // 当前用户只能获取和管辖的馆代码关联的违约金记录
            if (sessioninfo.GlobalUser == false)
            {
                string strLibraryCode = DomUtil.GetElementText(amerce_dom.DocumentElement, "libraryCode");
                string strItemBarcode = DomUtil.GetElementText(amerce_dom.DocumentElement, "itemBarcode");
                if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                {
                    // 进一步判断册记录是否在当前用户管辖范围内
                    if (string.IsNullOrEmpty(strItemBarcode) == false)
                    {
                        // return:
                        //      -1  errpr
                        //      0   不在控制范围
                        //      1   在控制范围
                        int nRet = this.IsItemInControl(
                            sessioninfo,
                            // channel,
                            strItemBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"违约金记录 '{strAmerceRecPath}' 超出当前用户管辖范围，并且在尝试检索册记录 '{strItemBarcode}' 时遇到问题: {strError}";
                            return -1;  // AceessDenied and error
                        }
                        if (nRet == 1)
                        {
                            return 1;
                        }
                    }
                    strError = "违约金记录 '" + strAmerceRecPath + "' 超出当前用户管辖范围，无法获取";
                    return 0;
                }
            }
            return 1;
        }


        // 将 dp2kernel 的错误码翻译为 dp2library 错误码并放入 LibraryServerResult 对象的 ErrorCode 成员
        public static void ConvertKernelErrorCode(ChannelErrorCode origin,
            ref LibraryServerResult result)
        {
            if (origin == ChannelErrorCode.AlreadyExist)
            {
                result.ErrorCode = ErrorCode.AlreadyExist;
                return;
            }
            if (origin == ChannelErrorCode.AlreadyExistOtherType)
            {
                result.ErrorCode = ErrorCode.AlreadyExistOtherType;
                return;
            }
            if (origin == ChannelErrorCode.ApplicationStartError)
            {
                result.ErrorCode = ErrorCode.ApplicationStartError;
                return;
            }
            if (origin == ChannelErrorCode.EmptyRecord)
            {
                result.ErrorCode = ErrorCode.EmptyRecord;
                return;
            }
            if (origin == ChannelErrorCode.None)
            {
                result.ErrorCode = ErrorCode.NoError;
                return;
            }
            if (origin == ChannelErrorCode.NotFound)
            {
                result.ErrorCode = ErrorCode.NotFound;
                return;
            }
            if (origin == ChannelErrorCode.NotFoundSubRes)
            {
                result.ErrorCode = ErrorCode.NotFoundSubRes;
                return;
            }
            if (origin == ChannelErrorCode.NotHasEnoughRights)
            {
                result.ErrorCode = ErrorCode.NotHasEnoughRights;
                return;
            }

            if (origin == ChannelErrorCode.OtherError)
            {
                result.ErrorCode = ErrorCode.OtherError;
                return;
            }
            if (origin == ChannelErrorCode.PartNotFound)
            {
                result.ErrorCode = ErrorCode.PartNotFound;
                return;
            }
            if (origin == ChannelErrorCode.RequestCanceled)
            {
                result.ErrorCode = ErrorCode.RequestCanceled;
                return;
            }
            if (origin == ChannelErrorCode.RequestCanceledByEventClose)
            {
                result.ErrorCode = ErrorCode.RequestCanceledByEventClose;
                return;
            }
            if (origin == ChannelErrorCode.RequestError)
            {
                result.ErrorCode = ErrorCode.RequestError;
                return;
            }
            if (origin == ChannelErrorCode.RequestTimeOut)
            {
                result.ErrorCode = ErrorCode.RequestTimeOut;
                return;
            }
            if (origin == ChannelErrorCode.TimestampMismatch)
            {
                result.ErrorCode = ErrorCode.TimestampMismatch;
                return;
            }

            // TODO: 其实可以用 Parse() 来翻译值
            if (origin == ChannelErrorCode.Compressed)
            {
                result.ErrorCode = ErrorCode.Compressed;
                return;
            }

            if (origin == ChannelErrorCode.NotLogin)
            {
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = "内核登录失败: " + result.ErrorInfo;
                return;
            }

            result.ErrorCode = ErrorCode.SystemError;
        }

        /*
        public static ErrorCode ConvertKernelErrorCode(ChannelErrorCode origin)
        {
            if (origin == ChannelErrorCode.AlreadyExist)
                return ErrorCode.AlreadyExist;
            if (origin == ChannelErrorCode.AlreadyExistOtherType)
                return ErrorCode.AlreadyExistOtherType;
            if (origin == ChannelErrorCode.ApplicationStartError)
                return ErrorCode.ApplicationStartError;
            if (origin == ChannelErrorCode.EmptyRecord)
                return ErrorCode.EmptyRecord;
            if (origin == ChannelErrorCode.None)
                return ErrorCode.NoError;
            if (origin == ChannelErrorCode.NotFound)
                return ErrorCode.NotFound;
            if (origin == ChannelErrorCode.NotFoundSubRes)
                return ErrorCode.NotFoundSubRes;
            if (origin == ChannelErrorCode.NotHasEnoughRights)
                return ErrorCode.NotHasEnoughRights;

            if (origin == ChannelErrorCode.OtherError)
                return ErrorCode.OtherError;
            if (origin == ChannelErrorCode.PartNotFound)
                return ErrorCode.PartNotFound;
            if (origin == ChannelErrorCode.RequestCanceled)
                return ErrorCode.RequestCanceled;
            if (origin == ChannelErrorCode.RequestCanceledByEventClose)
                return ErrorCode.RequestCanceledByEventClose;
            if (origin == ChannelErrorCode.RequestError)
                return ErrorCode.RequestError;
            if (origin == ChannelErrorCode.RequestTimeOut)
                return ErrorCode.RequestTimeOut;
            if (origin == ChannelErrorCode.TimestampMismatch)
                return ErrorCode.TimestampMismatch;

            if (origin == ChannelErrorCode.NotLogin)
                return ErrorCode.SystemError;

            return ErrorCode.SystemError;
        }
         * */


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
                false,  // bInlucdePreamble
                "ignorechecktimestamp", // style
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
                result.ErrorInfo = "库名为 '" + strDbName + "' 途径为 '" + strFrom + "' 键值为 '" + strKey + "' 的记录没有找到";
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
                return 0;   // 没有找到
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