using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;


namespace DigitalPlatform.LibraryServer
{
    // 负责升级 mongodb 数据库的功能
    public partial class LibraryApplication
    {
        // 把册条码号用法升级为 @refID:xxx 形态
        // return:
        //      -1  出错
        //      其它  总共处理的条数
        public int UpgradeItemBarcodes(
            SessionInfo sessioninfo,
            delegate_appendResultText AppendResultText,
            delegate_setProgressText SetProgressText,
            CancellationToken token,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            bool need_change_mongodb = (this.ChargingOperDatabase != null
&& this.ChargingOperDatabase.Enabled);

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            /*
            foreach (ItemDbCfg cfg in this.ItemDbs)
            {
                if (String.IsNullOrEmpty(cfg.DbNames) == true)
                    continue;
            }
            */

            var db_paths = this.ItemDbs
                .Where(o => string.IsNullOrEmpty(o.DbName) == false)
                .Select(o => this.WsUrl + "?" + o.DbName)
                .ToList();
            RecordLoader loader = new RecordLoader(
                sessioninfo.Channels,
null,
db_paths,
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

                SetProgressText?.Invoke($"正在处理册记录 {record.RecPath}");

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
                nRet = ProcessItemRecord(
                    strOutputPath,
                    need_change_mongodb,
                    // strLibraryCode,
                    ref strXmlBody,
                    // nRedoCount,
                    // baOutputTimeStamp,
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
                        nRet = WriteSetEntityLog(sessioninfo,
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
                            AppendResultText?.Invoke($"SaveRecord({strOutputPath}) (遇到时间戳不匹配)重试十次以后依然出错，放弃重试。error : {strError}。", true);
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

        // 处理
        // parameters:
        //      changed    [out] xml 是否发生过改变
        // return:
        //      -1  出错
        //      0   正常
        int ProcessItemRecord(
            string recpath,
            bool need_change_mongodb,
            ref string xml,
            out bool changed,
            out string strError)
        {
            strError = "";

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

            string barcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            if (string.IsNullOrEmpty(barcode))
                return 0;
            string refID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");
            if (string.IsNullOrEmpty(refID))
            {
                refID = Guid.NewGuid().ToString();
                DomUtil.SetElementText(dom.DocumentElement,
                    "refID",
                    refID);
                changed = true;
            }

            if (need_change_mongodb)
                this.ChargingOperDatabase.ChangeItemBarcode(barcode, $"@refID:{refID}");

            if (changed == true)
            {
                // 册记录发生了变化
                xml = dom.DocumentElement.OuterXml;
            }

            return 0;
        }


        public delegate void delegate_appendResultText(string text, bool error = false);
        public delegate void delegate_setProgressText(string text);

        // 把证条码号用法升级为 @refID:xxx 形态
        // return:
        //      -1  出错
        //      其它  总共处理的条数
        public int UpgradePatronBarcodes(
            SessionInfo sessioninfo,
            delegate_appendResultText AppendResultText,
            delegate_setProgressText SetProgressText,
            CancellationToken token,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            bool need_change_mongodb = (this.ChargingOperDatabase != null
&& this.ChargingOperDatabase.Enabled);

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            /*
            List<string> db_paths = new List<string>();
            foreach (var cfg in this.ReaderDbs)
            {
                if (string.IsNullOrEmpty(cfg.DbNames) == false)
                    db_paths.Add(this.WsUrl + "?" + cfg.DbNames);
            }
            */
            var db_paths = this.ReaderDbs
    .Where(o => string.IsNullOrEmpty(o.DbName) == false)
    .Select(o => this.WsUrl + "?" + o.DbName)
    .ToList();

            RecordLoader loader = new RecordLoader(sessioninfo.Channels,
null,
db_paths,
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

                SetProgressText?.Invoke($"正在处理读者记录 {record.RecPath}");

                string strOutputPath = record.RecPath;
                string strXmlBody = record.Xml;
                byte[] baOutputTimeStamp = record.Timestamp;

                int nRedoCount = 0;
            REDO:
                string old_xml = strXmlBody;

                // 处理
                // parameters:
                //      changed    [out] strReaderXml 是否发生过改变
                // return:
                //      -1  出错
                //      0   正常
                nRet = ProcessPatronRecord(
                    strOutputPath,
                    need_change_mongodb,
                    // strLibraryCode,
                    ref strXmlBody,
                    // nRedoCount,
                    // baOutputTimeStamp,
                    out bool bChanged,
                    out strError);
                if (nRet == -1)
                {
                    AppendResultText?.Invoke("ProcessPatronRecord() error : " + strError + "。", true);
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
                        // 获得馆代码
                        nRet = this.GetLibraryCode(strOutputPath,
                            out string library_code,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"获得读者记录的馆代码时出错: {strError}";
                            return -1;
                        }
                        // 写入 setReaderInfo 操作日志
                        nRet = WriteSetReaderInfoLog(sessioninfo,
                            library_code,
                            strOutputPath,
                            old_xml,
                            strXmlBody,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"修改后的读者记录 {strOutputPath} 写 setReaderInfo 日志动作记录时出错: {strError}";
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
                            AppendResultText?.Invoke($"SaveRecord({strOutputPath}) (遇到时间戳不匹配)重试十次以后依然出错，放弃重试。error : {strError}。", true);
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

        /*
<root>
<operation>setReaderInfo</operation> 操作类型
<action>...</action> 具体动作。有new change delete move 4种
<record recPath='...'>...</record> 新记录
<oldRecord recPath='...'>...</oldRecord> 被覆盖或者删除的记录 动作为 change 和 delete 时具备此元素
<changedEntityRecord itemBarcode='...' recPath='...' oldBorrower='...' newBorrower='...' /> 若干个元素。表示连带发生修改的册记录
<operator>test</operator> 操作者
<operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 操作时间
</root>

注: new 的时候只有<record>元素，delete的时候只有<oldRecord>元素，change的时候两者都有

 * */
        int WriteSetReaderInfoLog(
            SessionInfo sessioninfo,
            string strLibraryCode,
            string recpath,
            string old_xml,
            string new_xml,
            out string strError)
        {
            strError = "";

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation", "setReaderInfo");
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "action", "change");

            string strOperTimeString = this.Clock.GetClock();   // RFC1123格式

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTimeString);

            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "record", new_xml);
            DomUtil.SetAttr(node, "recPath", recpath);

            node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "oldRecord", old_xml);
            DomUtil.SetAttr(node, "recPath", recpath);

            int nRet = this.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "WriteSetReaderInfoLog() 写入操作日志时发生错误: " + strError;
                return -1;
            }

            return 0;
        }


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
        int WriteSetEntityLog(
    SessionInfo sessioninfo,
    string recpath,
    string old_xml,
    string new_xml,
    out string strError)
        {
            strError = "";

            string strLibraryCode = GetItemLibraryCode(old_xml);

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation", "setEntity");
            DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCode",
strLibraryCode);    // 册所在的馆代码
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "action", "change");

            string strOperTimeString = this.Clock.GetClock();   // RFC1123格式

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTimeString);

            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "record", new_xml);
            DomUtil.SetAttr(node, "recPath", recpath);

            node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "oldRecord", old_xml);
            DomUtil.SetAttr(node, "recPath", recpath);

            int nRet = this.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "WriteSetEntityLog() 写入操作日志时发生错误: " + strError;
                return -1;
            }

            return 0;
        }

        // Exception: 可能会抛出 XML 异常
        static string GetItemLibraryCode(string old_xml)
        {
            XmlDocument itemdom = new XmlDocument();
            itemdom.LoadXml(old_xml);
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
    "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 将馆藏地点字符串分解为 馆代码+地点名 两个部分
            ParseCalendarName(strLocation,
        out string strLibraryCode,
        out _);

            return strLibraryCode;
        }


        // 处理
        // parameters:
        //      changed    [out] xml 是否发生过改变
        // return:
        //      -1  出错
        //      0   正常
        int ProcessPatronRecord(
            string recpath,
            bool need_change_mongodb,
            ref string xml,
            out bool changed,
            out string strError)
        {
            strError = "";

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

            string barcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            if (string.IsNullOrEmpty(barcode))
                return 0;
            string refID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");
            if (string.IsNullOrEmpty(refID))
            {
                refID = Guid.NewGuid().ToString();
                DomUtil.SetElementText(dom.DocumentElement,
                    "refID",
                    refID);
                changed = true;
            }

            if (need_change_mongodb)
                this.ChargingOperDatabase.ChangePatronBarcode(barcode, $"@refID:{refID}");

            if (changed == true)
            {
                // 读者记录发生了变化
                xml = dom.DocumentElement.OuterXml;
            }

            return 0;
        }

        // TODO: 是否应该产生 SetReaderInfo() 或者 SetItemInfo() 操作日志
        // 保存(修改后的)记录
        // return:
        //      -2  保存时遇到时间戳冲突，已经重新装载读者记录返回于 strReaderXml 中，时间戳于 output_timestamp 中
        //      -1  出错
        //      0   保存成功
        int SaveRecord(RmsChannel channel,
            string strPath,
            ref string strReaderXml,
            byte[] baTimeStamp,
            out byte[] output_timestamp,
            out string strError)
        {
            long lRet = channel.DoSaveTextRes(strPath,
                strReaderXml,
                false,
                "content",  // ,ignorechecktimestamp
                baTimeStamp,
                out output_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // 时间戳冲突
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    string strStyle = "data,content,timestamp,outputpath";

                    lRet = channel.GetRes(strPath,
                        strStyle,
                        out strReaderXml,
                        out string strMetaData,
                        out baTimeStamp,
                        out strOutputPath,
                        out strError);
                    output_timestamp = baTimeStamp;
                    if (lRet == -1)
                    {
                        strError = "写回记录 '" + strPath + "' 时发生时间戳冲突，重装记录时又发生错误: " + strError;
                        return -1;
                    }

                    //nRedoCount++;
                    //goto REDO;
                    return -2;
                }

                strError = "写回记录 '" + strPath + "' 时发生错误: " + strError;
                return -1;
            }

            return 0;
        }

    }
}
