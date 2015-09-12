using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和期刊业务(期)相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
#if NOOOOOOOOOOOO
        // 获得期刊记录
        // 本函数可获得超过1条以上的路径
        // parameters:
        //      timestamp   返回命中的第一条的timestamp
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetIssueRecXml(
            RmsChannelCollection channels,
            string strIssueDbName,
            string strParentID,
            string strPublishTime,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = null;

            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            // 构造检索式
            // 构造检索式
            string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "出版时间")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strPublishTime)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml += "<operator value='AND'/>";


            strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "父记录")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strParentID)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml = "<group>" + strQueryXml + "</group>";

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "册条码号 '" + strPublishTime + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            // List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                Math.Min(nMax, lHitCount),
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            Debug.Assert(aPath != null, "");

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            string strOutputPath = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 删除期记录的操作
        int DoIssueOperDelete(
            SessionInfo sessioninfo,
            RmsChannel channel,
            IssueInfo info,
            string strIssueDbName,
            string strParentID,
            string strOldPublishTime,
            string strNewPublishTime,   // TODO: 本参数是否可以废除?
            XmlDocument domOldRec,
            ref XmlDocument domOperLog,
            ref List<IssueInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            IssueInfo error = null;
            int nRet = 0;
            long lRet = 0;
            string strError = "";

            // 如果newrecpath为空但是oldrecpath有值，就用oldrecpath的值
            // 2007/10/23
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                if (String.IsNullOrEmpty(info.OldRecPath) == false)
                    info.NewRecPath = info.OldRecPath;
            }


            // 如果记录路径为空, 则先获得记录路径
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                List<string> aPath = null;

                if (String.IsNullOrEmpty(strOldPublishTime) == true)
                {
                    strError = "info.OldRecord中的<publishTime>元素中的出版时间，和info.RecPath参数值，不能同时为空。";
                    goto ERROR1;
                }

                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                nRet = this.SearchIssueRecDup(
                    sessioninfo.Channels,
                    strIssueDbName,
                    strParentID,
                    strOldPublishTime,
                    100,
                    out aPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "删除操作中出版时间查重阶段发生错误:" + strError;
                    goto ERROR1;
                }


                if (nRet == 0)
                {
                    error = new IssueInfo(info);
                    error.ErrorInfo = "父记录ID为 '"+strParentID+"', + 出版时间为 '" + strOldPublishTime + "' 的期记录已不存在";
                    error.ErrorCode = ErrorCodeValue.NotFound;
                    ErrorInfos.Add(error);
                    return -1;
                }


                if (nRet > 1)
                {
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);

                    // 在删除操作中，遇到重复的是很平常的事情。只要
                    // info.OldRecPath能够清晰地指出要删除的那一条，就可以执行删除
                    if (String.IsNullOrEmpty(info.OldRecPath) == false)
                    {
                        if (aPath.IndexOf(info.OldRecPath) == -1)
                        {
                            strError = "出版时间 '" + strOldPublishTime + "' 已经被下列多条期记录使用了: " + String.Join(",", pathlist) + "'，但并不包括info.OldRecPath所指的路径 '" + info.OldRecPath + "'。";
                            goto ERROR1;
                        }
                        info.NewRecPath = info.OldRecPath;
                    }
                    else
                    {

                        strError = "出版时间 '" + strOldPublishTime + "' 已经被下列多条期记录使用了: " + String.Join(",", pathlist) + "'，这是一个严重的系统故障，请尽快通知系统管理员处理。";
                        goto ERROR1;
                    }
                }
                else
                {
                    Debug.Assert(nRet == 1, "");

                    info.NewRecPath = aPath[0];
                }

                ///

                /*

                if (nRet > 1)
                {
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);

                    strError = "出版时间 '" + strOldPublishTime + "' 已经被下列多条期记录使用了: " + String.Join(",", pathlist) + "'，这是一个严重的系统故障，请尽快通知系统管理员处理。";
                    goto ERROR1;
                }

                info.NewRecPath = aPath[0];
                 * */
            }

            Debug.Assert(String.IsNullOrEmpty(info.NewRecPath) == false, "");
            // Debug.Assert(strEntityDbName != "", "");

            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";
            string strExistingXml = "";

        REDOLOAD:

            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(info.NewRecPath,
                out strExistingXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    error = new IssueInfo(info);
                    error.ErrorInfo = "出版时间为 '" + strOldPublishTime + "' 的期记录 '" + info.NewRecPath + "' 已不存在";
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
                else
                {
                    error = new IssueInfo(info);
                    error.ErrorInfo = "删除操作发生错误, 在读入原有记录 '" + info.NewRecPath + "' 阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // 把记录装入DOM
            XmlDocument domExist = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistingXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            // 观察已经存在的记录中，出版时间是否和strOldPublishTime一致
            if (String.IsNullOrEmpty(strOldPublishTime) == false)
            {
                string strExistingPublishTime = DomUtil.GetElementText(domExist.DocumentElement,
                    "publishTime");
                if (strExistingPublishTime != strOldPublishTime)
                {
                    strError = "路径为 '" + info.NewRecPath + "' 的期记录中<publishTime>元素中的出版时间 '" + strExistingPublishTime + "' 和strOldXml中<publishTime>元素中的出版时间 '" + strOldPublishTime + "' 不一致。拒绝删除(如果允许删除，则会造成不经意删除了别的期记录的危险)。";
                    goto ERROR1;
                }
            }

            /*
            // 观察已经存在的记录是否有流通信息
            if (IsIssueHasCirculationInfo(domExist) == true)
            {
                strError = "拟删除的期记录 '" + info.NewRecPath + "' 中包含有流通信息，不能删除。";
                goto ERROR1;
            }*/

            // 比较时间戳
            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 如果前端给出了旧记录，就有和库中记录进行比较的基础
                if (String.IsNullOrEmpty(info.OldRecord) == false)
                {
                    // 比较两个记录, 看看和期要害信息有关的字段是否发生了变化
                    // return:
                    //      0   没有变化
                    //      1   有变化
                    nRet = IsIssueInfoChanged(domExist,
                        domOldRec);
                    if (nRet == 1)
                    {

                        error = new IssueInfo(info);
                        error.NewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                        error.ErrorInfo = "数据库中即将删除的期记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                        error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }

                info.OldTimestamp = exist_timestamp;
                info.NewTimestamp = exist_timestamp;
            }

            byte[] output_timestamp = null;

            lRet = channel.DoDeleteRes(info.NewRecPath,
                info.OldTimestamp,
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复删除均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new IssueInfo(info);
                error.NewTimestamp = output_timestamp;
                error.ErrorInfo = "删除操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else
            {
                // 成功
                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "delete");

                // 不创建<record>元素

                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistingXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);


                // 如果删除成功，则不必要在数组中返回表示成功的信息元素了
            }

            return 0;
        ERROR1:
            error = new IssueInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // 执行SetIssues API中的"move"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // return:
        //      -1  出错
        //      0   成功
        int DoIssueOperMove(
            RmsChannel channel,
            IssueInfo info,
            ref XmlDocument domOperLog,
            ref List<IssueInfo> ErrorInfos)
        {
            // int nRedoCount = 0;
            IssueInfo error = null;
            bool bExist = true;    // info.RecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // 检查路径
            if (info.OldRecPath == info.NewRecPath)
            {
                strError = "当action为\"move\"时，info.NewRecordPath路径 '" + info.NewRecPath + "' 和info.OldRecPath '" + info.OldRecPath + "' 必须不相同";
                goto ERROR1;
            }

            // 检查即将覆盖的目标位置是不是有记录，如果有，则不允许进行move操作。
            // 如果要进行带覆盖目标位置记录功能的move操作，前端可以先执行一个delete操作，然后再执行move操作。
            // 这样规定，是为了避免过于复杂的判断逻辑，也便于前端操作者清楚操作的后果。
            // 因为如果允许move带有覆盖目标记录功能，则被覆盖的记录的预删除操作，等于进行了一次注销，但这个效用不明显，对前端操作人员准确判断事态并对后果负责(而且可能这种注销需要额外的操作权限)，不利
            bool bAppendStyle = false;  // 目标路径是否为追加形态？
            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                bAppendStyle = true;

            string strOutputPath = "";
            string strMetaData = "";


            if (bAppendStyle == false)
            {
                string strExistTargetXml = "";
                byte[] exist_target_timestamp = null;

                // 获取覆盖目标位置的现有记录
                lRet = channel.GetRes(info.NewRecPath,
                    out strExistTargetXml,
                    out strMetaData,
                    out exist_target_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
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
                        error = new IssueInfo(info);
                        error.ErrorInfo = "move操作发生错误, 在读入即将覆盖的目标位置 '" + info.NewRecPath + "' 原有记录阶段:" + strError;
                        error.ErrorCode = channel.OriginErrorCode;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }
                else
                {
                    // 如果记录存在，则目前不允许这样的操作
                    strError = "移动(move)操作被拒绝。因为在即将覆盖的目标位置 '" + info.NewRecPath + "' 已经存在期记录。请先删除(delete)这条记录，再进行移动(move)操作";
                    goto ERROR1;
                }
            }


            string strExistSourceXml = "";
            byte[] exist_source_timestamp = null;

            // 先读出数据库中源位置的已有记录
            // REDOLOAD:

            lRet = channel.GetRes(info.OldRecPath,
                out strExistSourceXml,
                out strMetaData,
                out exist_source_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    /*
                    // 如果记录不存在, 则构造一条空的记录
                    bExist = false;
                    strExistSourceXml = "<root />";
                    exist_source_timestamp = null;
                    strOutputPath = info.NewRecPath;
                     * */
                    // 这种情况如果放宽，会有严重的副作用，所以不让放宽
                    strError = "move操作的源记录 '" + info.OldRecPath + "' 在数据库中不存在，所以无法进行移动操作。";
                    goto ERROR1;
                }
                else
                {
                    error = new IssueInfo(info);
                    error.ErrorInfo = "保存操作发生错误, 在读入库中原有源记录(路径在info.OldRecPath) '" + info.OldRecPath + "' 阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // 把两个记录装入DOM

            XmlDocument domSourceExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domSourceExist.LoadXml(strExistSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecord装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }


            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_source_timestamp);
            if (nRet != 0)
            {
                // 时间戳不相等了
                // 需要把info.OldRecord和strExistXml进行比较，看看和期记到有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                // 比较两个记录, 看看和期记到有关的字段是否发生了变化
                // return:
                //      0   没有变化
                //      1   有变化
                nRet = IsIssueInfoChanged(domOld,
                    domSourceExist);
                if (nRet == 1)
                {
                    error = new IssueInfo(info);
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    error.OldRecord = strExistSourceXml;
                    error.OldTimestamp = exist_source_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录已被删除。";
                    else
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录已发生过修改";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_source_timestamp此时已经反映了库中被修改后的记录的时间戳
            }


            // 合并新旧记录
            string strNewXml = "";
            nRet = MergeTwoIssueXml(domSourceExist,
                domNew,
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // 移动记录
            byte[] output_timestamp = null;

            // TODO: Copy后还要写一次？因为Copy并不写入新记录。
            // 其实Copy的意义在于带走资源。否则还不如用Save+Delete
            lRet = channel.DoCopyRecord(info.OldRecPath,
                info.NewRecPath,
                true,   // bDeleteSourceRecord
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "DoCopyRecord() error :" + strError;
                goto ERROR1;
            }

            // Debug.Assert(strOutputPath == info.NewRecPath);
            string strTargetPath = strOutputPath;

            lRet = channel.DoSaveTextRes(strTargetPath,
                strNewXml,
                false,   // include preamble?
                "content",
                output_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "WriteIssues()API move操作中，期记录 '" + info.OldRecPath + "' 已经被成功移动到 '" + strTargetPath + "' ，但在写入新内容时发生错误: " + strError;

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    // 不进行反复处理。
                    // 因为源已经移动，情况很复杂
                }

                // 仅仅写入错误日志即可。没有Undo
                this.WriteErrorLog(strError);

                /*
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复保存(DoCopyRecord())均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }*/


                error = new IssueInfo(info);
                error.ErrorInfo = "保存操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // 成功
            {
                info.NewRecPath = strOutputPath;    // 兑现保存的位置，因为可能有追加形式的路径

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "move");

                // 新记录
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // 旧记录
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistSourceXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                error = new IssueInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "保存操作成功。NewRecPath中返回了实际保存的路径, NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;

        ERROR1:
            error = new IssueInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // 执行SetIssues API中的"change"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // return:
        //      -1  出错
        //      0   成功
        static int DoIssueOperChange(
            RmsChannel channel,
            IssueInfo info,
            ref XmlDocument domOperLog,
            ref List<IssueInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            IssueInfo error = null;
            bool bExist = true;    // info.RecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // 检查一下路径
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                strError = "info.NewRecPath中的路径不能为空";
                goto ERROR1;
            }

            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?")
            {
                strError = "info.NewRecPath路径 '" + strTargetRecId + "' 中记录ID部分不能为'?'";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strTargetRecId) == true)
            {
                strError = "info.NewRecPath路径 '" + strTargetRecId + "' 中记录ID部分不能为空";
                goto ERROR1;
            }

            if (info.OldRecPath != info.NewRecPath)
            {
                strError = "当action为\"change\"时，info.NewRecordPath路径 '" + info.NewRecPath + "' 和info.OldRecPath '" + info.OldRecPath + "' 必须相同";
                goto ERROR1;
            }

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";


            // 先读出数据库中即将覆盖位置的已有记录

        REDOLOAD:

            lRet = channel.GetRes(info.NewRecPath,
                out strExistXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    // 如果记录不存在, 则构造一条空的记录
                    bExist = false;
                    strExistXml = "<root />";
                    exist_timestamp = null;
                    strOutputPath = info.NewRecPath;
                }
                else
                {
                    error = new IssueInfo(info);
                    error.ErrorInfo = "保存操作发生错误, 在读入原有记录阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }


            // 把两个记录装入DOM

            XmlDocument domExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecord装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }


            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 时间戳不相等了
                // 需要把info.OldRecord和strExistXml进行比较，看看和期记到有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                // 比较两个记录, 看看和期记到有关的字段是否发生了变化
                // return:
                //      0   没有变化
                //      1   有变化
                nRet = IsIssueInfoChanged(domOld,
                    domExist);
                if (nRet == 1)
                {
                    error = new IssueInfo(info);
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    error.OldRecord = strExistXml;
                    error.OldTimestamp = exist_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录已被删除。";
                    else
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录已发生过修改";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_timestamp此时已经反映了库中被修改后的记录的时间戳
            }


            // 合并新旧记录
            string strNewXml = "";
            nRet = MergeTwoIssueXml(domExist,
                domNew,
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // 保存新记录
            byte[] output_timestamp = null;
            lRet = channel.DoSaveTextRes(info.NewRecPath,
                strNewXml,
                false,   // include preamble?
                "content",
                exist_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复保存均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new IssueInfo(info);
                error.ErrorInfo = "保存操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // 成功
            {
                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "change");

                // 新记录
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // 旧记录
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                error = new IssueInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "保存操作成功。NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;

        ERROR1:
            error = new IssueInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // <DoIssueOperChange()的下级函数>
        // 比较两个记录, 看看和记到有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        static int IsIssueInfoChanged(XmlDocument dom1,
            XmlDocument dom2)
        {
            // 要害元素名列表
            string[] element_names = new string[] {
                "parent",
                "publishTime",
                "no",   // 总期号
                "volume",
                "price",
                "comment",
                "batchNo"
            };

            for (int i = 0; i < element_names.Length; i++)
            {
                string strText1 = DomUtil.GetElementText(dom1.DocumentElement,
                    element_names[i]);
                string strText2 = DomUtil.GetElementText(dom2.DocumentElement,
                    element_names[i]);

                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }

        // <DoIssueOperChange()的下级函数>
        // 合并新旧记录
        static int MergeTwoIssueXml(XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中

            // 要害元素名列表
            string[] element_names = new string[] {
                "parent",
                "publishTime",
                "no",   // 总期号
                "volume",
                "price",
                "comment",
                "batchNo"
            };

            for (int i = 0; i < element_names.Length; i++)
            {
                string strTextNew = DomUtil.GetElementText(domNew.DocumentElement,
                    element_names[i]);

                DomUtil.SetElementText(domExist.DocumentElement,
                    element_names[i], strTextNew);
            }

            strMergedXml = domExist.OuterXml;

            return 0;
        }


        // 构造出适合保存的新期记录
        static int BuildNewIssueRecord(string strOriginXml,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strOriginXml);
            }
            catch (Exception ex)
            {
                strError = "装载strOriginXml到DOM时出错: " + ex.Message;
                return -1;
            }

            /*
            // 流通元素名列表
            string[] element_names = new string[] {
                "borrower",
                "borrowDate",
                "borrowPeriod",
                "borrowHistory",
            };

            for (int i = 0; i < element_names.Length; i++)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    element_names[i], "");
            }
             * */

            strXml = dom.OuterXml;

            return 0;
        }

        // 根据 父记录ID/出版时间 对期库进行查重
        // 本函数只负责查重, 并不获得记录体
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchIssueRecDup(
            RmsChannelCollection channels,
            string strIssueDbName,
            string strParentID,
            string strPublishTime,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = null;

            LibraryApplication app = this;

            // 构造检索式
            string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "出版时间")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strPublishTime)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml += "<operator value='AND'/>";


            strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "父记录")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strParentID)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml = "<group>" + strQueryXml + "</group>";

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "出版时间为 '" + strPublishTime + "' 并且 父记录为 '"+strParentID+"' 的记录没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }


        // 对新旧期记录中包含的出版时间进行比较, 看看是否发生了变化(进而就需要查重)
        // 出版时间包含在<publishTime>元素中
        // parameters:
        //      strOldPublishTime   顺便返回旧记录中的出版时间
        //      strNewPublishTime   顺便返回新记录中的出版时间
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        static int CompareTwoIssueNo(XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldPublishTime,
            out string strOldParentID,
            out string strNewPublishTime,
            out string strNewParentID,
            out string strError)
        {
            strError = "";

            strOldPublishTime = "";
            strNewPublishTime = "";

            strOldParentID = "";
            strNewParentID = "";

            strOldPublishTime = DomUtil.GetElementText(domOldRec.DocumentElement,
                "publishTime");

            strNewPublishTime = DomUtil.GetElementText(domNewRec.DocumentElement,
                "publishTime");

            strOldParentID = DomUtil.GetElementText(domOldRec.DocumentElement,
                "parent");

            strNewParentID = DomUtil.GetElementText(domNewRec.DocumentElement,
                "parent");

            if (strOldPublishTime != strNewPublishTime)
                return 1;   // 不相等

            return 0;   // 相等
        }

        // TODO: 需要有限定期范围的能力
        // 获得期信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      issues 返回的期信息数组
        // 权限：需要有getissues权限
        // return:
        //      Result.Value    -1出错 0没有找到 其他 实体记录的个数
        public Result GetIssues(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            out IssueInfo[] issues)
        {

            issues = null;

            Result result = new Result();

            // 权限字符串
            if (StringUtil.IsInList("getissues", sessioninfo.RightsOrigin) == false
        && StringUtil.IsInList("getissueinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "获得期信息 操作被拒绝。不具备getissueinfo或getissues权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }


            int nRet = 0;
            string strError = "";

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的期库名
            string strIssueDbName = "";
            nRet = this.GetIssueDbName(strBiblioDbName,
                 out strIssueDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strIssueDbName) == true)
            {
                strError = "书目库名 '" +strBiblioDbName+ "' 对应的期库名没有定义";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 检索期库中全部从属于特定id的记录

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "父记录")       // 2007/9/14
                + "'><item><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
            long lRet = channel.DoSearch(strQueryXml,
                "issues",
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "没有找到";
                return result;
            }

            int nResultCount = (int)lRet;

            if (nResultCount > 10000)
            {
                strError = "命中期记录数 " + nResultCount.ToString() + " 超过 10000, 暂时不支持";
                goto ERROR1;
            }

            List<IssueInfo> issueinfos = new List<IssueInfo>();

            int nStart = 0;
            int nPerCount = 100;
            for (; ; )
            {
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "issues",
                    nStart,
                    nPerCount,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }

                // 获得每条记录
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(aPath[i],
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    IssueInfo issueinfo = new IssueInfo();

                    if (lRet == -1)
                    {
                        issueinfo.OldRecPath = aPath[i];
                        issueinfo.ErrorCode = channel.OriginErrorCode;
                        issueinfo.ErrorInfo = channel.ErrorInfo;

                        issueinfo.OldRecord = "";
                        issueinfo.OldTimestamp = null;

                        issueinfo.NewRecPath = "";
                        issueinfo.NewRecord = "";
                        issueinfo.NewTimestamp = null;
                        issueinfo.Action = "";


                        goto CONTINUE;
                    }

                    issueinfo.OldRecPath = strOutputPath;
                    issueinfo.OldRecord = strXml;
                    issueinfo.OldTimestamp = timestamp;

                    issueinfo.NewRecPath = "";
                    issueinfo.NewRecord = "";
                    issueinfo.NewTimestamp = null;
                    issueinfo.Action = "";

                CONTINUE:
                    issueinfos.Add(issueinfo);
                }

                nStart += aPath.Count;
                if (nStart >= nResultCount)
                    break;
            }

            // 挂接到结果中
            issues = new IssueInfo[issueinfos.Count];
            for (int i = 0; i < issueinfos.Count; i++)
            {
                issues[i] = issueinfos[i];
            }

            result.Value = issues.Length;
            return result;

        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }


        // 设置/保存期信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分。库名可以用来确定书目库，id可以被实体记录用来设置<parent>元素内容。另外书目库名和IssueInfo中的NewRecPath形成映照关系，需要检查它们是否正确对应
        //      issueinfos 要提交的的期信息数组
        // 权限：需要有setissues权限
        // 修改意见: 写入期库中的记录, 还缺乏<operator>和<operTime>字段
        public Result SetIssues(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            IssueInfo[] issueinfos,
            out IssueInfo[] errorinfos)
        {
            errorinfos = null;

            Result result = new Result();

            // 权限字符串
            if (StringUtil.IsInList("setissueinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "保存期信息 操作被拒绝。不具备setissueinfo权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }


            int nRet = 0;
            long lRet = 0;
            string strError = "";

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的期库名
            string strIssueDbName = "";
            nRet = this.GetIssueDbName(strBiblioDbName,
                 out strIssueDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strIssueDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的期库名没有定义";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            byte[] output_timestamp = null;
            string strOutputPath = "";

            List<IssueInfo> ErrorInfos = new List<IssueInfo>();

            for (int i = 0; i < issueinfos.Length; i++)
            {
                IssueInfo info = issueinfos[i];

                // TODO: 当操作为"delete"时，是否可以允许只设置OldRecPath，而不必设置NewRecPath
                // 如果两个都设置，则要求设置为一致的。

                // 检查路径中的库名部分
                if (String.IsNullOrEmpty(info.NewRecPath) == false)
                {
                    strError = "";


                    string strDbName = ResPath.GetDbName(info.NewRecPath);

                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "NewRecPath中数据库名不应为空";
                    }

                    if (strDbName != strIssueDbName)
                    {
                        strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为 '" + strIssueDbName + "'。(因为书目库名为 '" + strBiblioDbName + "'，其对应的期库名应为 '" + strIssueDbName + "' )";
                    }

                    if (strError != "")
                    {
                        IssueInfo error = new IssueInfo(info);
                        error.ErrorInfo = strError;
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                        continue;
                    }
                }

                // 把(前端发过来的)旧记录装载到DOM
                XmlDocument domOldRec = new XmlDocument();
                try
                {
                    // 用strOldRecord的目的是不想改变info.OldRecord内容, 因为后者可能被复制到输出信息中
                    string strOldRecord = info.OldRecord;
                    if (String.IsNullOrEmpty(strOldRecord) == true)
                        strOldRecord = "<root />";

                    domOldRec.LoadXml(strOldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord XML记录装载到DOM时出错: " + ex.Message;

                    IssueInfo error = new IssueInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                // 把要保存的新记录装载到DOM
                XmlDocument domNewRec = new XmlDocument();
                try
                {
                    // 用strNewRecord的目的是不想改变info.NewRecord内容, 因为后者可能被复制到输出信息中
                    string strNewRecord = info.NewRecord;

                    if (String.IsNullOrEmpty(strNewRecord) == true)
                        strNewRecord = "<root />";

                    domNewRec.LoadXml(strNewRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.NewRecord XML记录装载到DOM时出错: " + ex.Message;

                    IssueInfo error = new IssueInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                string strOldPublishTime = "";
                string strNewPublishTime = "";

                string strOldParentID = "";
                string strNewParentID = "";

                // 对出版时间加锁?
                string strLockPublishTime = "";

                try
                {


                    // 命令new和change的共有部分 -- 出版时间查重, 也需要加锁
                    // delete则需要加锁
                    if (info.Action == "new"
                        || info.Action == "change"
                        || info.Action == "delete"
                        || info.Action == "move")
                    {

                        // 仅仅用来获取一下新出版时间
                        // 看看新旧出版时间是否有差异
                        // 对IssueInfo中的OldRecord和NewRecord中包含的条码号进行比较, 看看是否发生了变化(进而就需要查重)
                        // return:
                        //      -1  出错
                        //      0   相等
                        //      1   不相等
                        nRet = CompareTwoIssueNo(domOldRec,
                            domNewRec,
                            out strOldPublishTime,
                            out strOldParentID,
                            out strNewPublishTime,
                            out strNewParentID,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "CompareTwoIssueNo() error : " + strError;
                            goto ERROR1;
                        }

                        if (info.Action == "new"
                            || info.Action == "change"
                            || info.Action == "move")
                            strLockPublishTime = strNewPublishTime;
                        else if (info.Action == "delete")
                        {
                            // 顺便进行一些检查
                            if (String.IsNullOrEmpty(strNewPublishTime) == false)
                            {
                                strError = "没有必要在delete操作的IssueInfo中, 包含NewRecord内容...。相反，注意一定要在OldRecord中包含即将删除的原记录";
                                goto ERROR1;
                            }
                            strLockPublishTime = strOldPublishTime;
                        }


                        // 加锁
                        if (String.IsNullOrEmpty(strLockPublishTime) == false)
                            this.EntityLocks.LockForWrite(strLockPublishTime);

                        // 进行出版时间查重
                        // TODO: 查重的时候要注意，如果操作类型为“move”，则可以允许查出和info.OldRecPath重的，因为它即将被删除
                        if (nRet == 1   // 新旧出版时间不等，才查重。这样可以提高运行效率。
                            && (info.Action == "new"
                                || info.Action == "change"
                                || info.Action == "move")       // delete操作不查重
                            && String.IsNullOrEmpty(strNewPublishTime) == false
                            )
                        {
                            string strParentID = strNewParentID;

                            if (String.IsNullOrEmpty(strParentID) == true)
                                strParentID = strOldParentID;

                            List<string> aPath = null;
                            // 根据 父记录ID+出版时间 对期库进行查重
                            // 本函数只负责查重, 并不获得记录体
                            // return:
                            //      -1  error
                            //      其他    命中记录条数(不超过nMax规定的极限)
                            nRet = SearchIssueRecDup(
                                sessioninfo.Channels,
                                strIssueDbName,
                                strParentID,
                                strNewPublishTime,
                                100,
                                out aPath,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            bool bDup = false;
                            if (nRet == 0)
                            {
                                bDup = false;
                            }
                            else if (nRet == 1) // 命中一条
                            {
                                Debug.Assert(aPath.Count == 1, "");

                                if (info.Action == "new")
                                {
                                    if (aPath[0] == info.NewRecPath) // 正好是自己
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号

                                }
                                else if (info.Action == "change")
                                {
                                    Debug.Assert(info.NewRecPath == info.OldRecPath, "当操作类型为change时，info.NewRecPath应当和info.OldRecPath相同");
                                    if (aPath[0] == info.OldRecPath) // 正好是自己
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号
                                }
                                else if (info.Action == "move")
                                {
                                    if (aPath[0] == info.OldRecPath) // 正好是源记录
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号
                                }
                                else
                                {
                                    Debug.Assert(false, "这里不可能出现的info.Action值 '" + info.Action + "'");
                                }


                            } // end of if (nRet == 1)
                            else
                            {
                                Debug.Assert(nRet > 1, "");
                                bDup = true;

                                // 因为move操作不允许目标位置存在记录，所以这里就不再费力考虑了
                                // 如果将来move操作允许目标位置存在记录，则这里需要判断：无论源还是目标位置发现条码号重，都不算重。
                            }

                            // 报错
                            if (bDup == true)
                            {
                                string[] pathlist = new string[aPath.Count];
                                aPath.CopyTo(pathlist);

                                IssueInfo error = new IssueInfo(info);
                                error.ErrorInfo = "出版时间 '" + strNewPublishTime + "' 已经被下列期记录使用了: " + String.Join(",", pathlist);
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }
                    }

                    // 准备日志DOM
                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "setIssue");

                    // 兑现一个命令
                    if (info.Action == "new")
                    {
                        // 检查新记录的路径中的id部分是否正确
                        // 库名部分，前面已经统一检查过了
                        strError = "";

                        if (String.IsNullOrEmpty(info.NewRecPath) == true)
                        {
                            info.NewRecPath = strIssueDbName + "/?";
                        }
                        else
                        {

                            string strID = ResPath.GetRecordId(info.NewRecPath);
                            if (String.IsNullOrEmpty(strID) == true)
                            {
                                strError = "RecPath中id部分应当为'?'";
                            }

                            if (strError != "")
                            {
                                IssueInfo error = new IssueInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }

                        // 构造出适合保存的新期记录
                        string strNewXml = "";
                        nRet = BuildNewIssueRecord(info.NewRecord,
                            out strNewXml,
                            out strError);
                        if (nRet == -1)
                        {
                            IssueInfo error = new IssueInfo(info);
                            error.ErrorInfo = strError;
                            error.ErrorCode = ErrorCodeValue.CommonError;
                            ErrorInfos.Add(error);
                            continue;
                        }

                        lRet = channel.DoSaveTextRes(info.NewRecPath,
                            strNewXml,
                            false,   // include preamble?
                            "content",
                            info.OldTimestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            IssueInfo error = new IssueInfo(info);
                            error.NewTimestamp = output_timestamp;
                            error.ErrorInfo = "保存新记录的操作发生错误:" + strError;
                            error.ErrorCode = channel.OriginErrorCode;
                            ErrorInfos.Add(error);

                            domOperLog = null;  // 表示不必写入日志
                        }
                        else // 成功
                        {

                            DomUtil.SetElementText(domOperLog.DocumentElement, "action", "new");

                            // 不创建<oldRecord>元素

                            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                                "record", strNewXml);
                            DomUtil.SetAttr(node, "recPath", strOutputPath);

                            // 新记录保存成功，需要返回信息元素。因为需要返回新的时间戳和实际保存的记录路径

                            IssueInfo error = new IssueInfo(info);
                            error.NewRecPath = strOutputPath;

                            error.NewRecord = strNewXml;    // 所真正保存的记录，可能稍有变化, 因此需要返回给前端
                            error.NewTimestamp = output_timestamp;

                            error.ErrorInfo = "保存新记录的操作成功。NewTimeStamp中返回了新的时间戳, RecPath中返回了实际存入的记录路径。";
                            error.ErrorCode = ErrorCodeValue.NoError;
                            ErrorInfos.Add(error);
                        }
                    }
                    else if (info.Action == "change")
                    {
                        // 执行SetIssues API中的"change"操作
                        nRet = DoIssueOperChange(
                            channel,
                            info,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // 失败
                            domOperLog = null;  // 表示不必写入日志
                        }
                    }
                    else if (info.Action == "move")
                    {
                        // 执行SetIssues API中的"move"操作
                        nRet = DoIssueOperMove(
                            channel,
                            info,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // 失败
                            domOperLog = null;  // 表示不必写入日志
                        }
                    }
                    else if (info.Action == "delete")
                    {
                        string strParentID = strNewParentID;

                        if (String.IsNullOrEmpty(strParentID) == true)
                            strParentID = strOldParentID;


                        // 删除期记录的操作
                        nRet = DoIssueOperDelete(
                            sessioninfo,
                            channel,
                            info,
                            strIssueDbName,
                            strParentID,
                            strOldPublishTime,
                            strNewPublishTime,
                            domOldRec,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // 失败
                            domOperLog = null;  // 表示不必写入日志
                        }
                    }
                    else
                    {
                        // 不支持的命令
                        IssueInfo error = new IssueInfo(info);
                        error.ErrorInfo = "不支持的操作命令 '" + info.Action + "'";
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                    }


                    // 写入日志
                    if (domOperLog != null)
                    {
                        string strOperTime = this.Clock.GetClock();
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                            sessioninfo.UserID);   // 操作者
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                            strOperTime);   // 操作时间

                        nRet = this.OperLog.WriteOperLog(domOperLog,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "SetIssues() API 写入日志时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }
                }
                finally
                {
                    if (String.IsNullOrEmpty(strLockPublishTime) == false)
                        this.EntityLocks.UnlockForWrite(strLockPublishTime);
                }

            }

            // 复制到结果中
            errorinfos = new IssueInfo[ErrorInfos.Count];
            for (int i = 0; i < ErrorInfos.Count; i++)
            {
                errorinfos[i] = ErrorInfos[i];
            }

            result.Value = ErrorInfos.Count;  // 返回信息的数量

            return result;
        ERROR1:
            // 这里的报错，是比较严重的错误。如果是数组中部分的请求发生的错误，则不在这里报错，而是通过返回错误信息数组的方式来表现
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }
#endif
    }

#if NOOOOOOOOOOOOOOOO
    // 期信息
    public class IssueInfo
    {

        public string OldRecPath = "";  // 原记录路径
        public string OldRecord = "";   // 旧记录
        public byte[] OldTimestamp = null;  // 旧记录对应的时间戳

        public string NewRecPath = ""; // 新记录路径
        public string NewRecord = "";   // 新记录
        public byte[] NewTimestamp = null;  // 新记录对应的时间戳

        public string Action = "";   // 要执行的操作(get时此项无用) 值为new change delete move 4种之一。change要求OldRecPath和NewRecPath一样。move不要求两者一样。把move操作单列出来，主要是为了日志统计的便利。
        public string ErrorInfo = "";   // 出错信息
        public ErrorCodeValue ErrorCode = ErrorCodeValue.NoError;   // 出错码（表示属于何种类型的错误）

        public IssueInfo(IssueInfo info)
        {
            this.OldRecPath = info.OldRecPath;
            this.OldRecord = info.OldRecord;
            this.OldTimestamp = info.OldTimestamp;
            this.NewRecPath = info.NewRecPath;
            this.NewRecord = info.NewRecord;
            this.NewTimestamp = info.NewTimestamp;
            this.Action = info.Action;
            this.ErrorInfo = info.ErrorInfo;
            this.ErrorCode = info.ErrorCode;
        }

        public IssueInfo()
        {

        }
    }
#endif
}
