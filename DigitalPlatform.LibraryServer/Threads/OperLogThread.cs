using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Messaging;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Message;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 操作日志的辅助线程。负责把积累的信息写入 mongodb 日志库
    /// </summary>
    public class OperLogThread : BatchTask
    {
        List<XmlDocument> _datas = new List<XmlDocument>();

        internal new ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        public OperLogThread(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
            this.PerTime = 5 * 60 * 1000;	// 5分钟

            if (string.IsNullOrEmpty(this.App.OutgoingQueue) == false)
            {
                try
                {
                    _queue = new MessageQueue(this.App.OutgoingQueue);
                    _queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                }
                catch (Exception ex)
                {
                    string strError = "OperLogThread 构造时，创建路径为 '" + this.App.OutgoingQueue + "' 的 MessageQueue 对象失败: " + ex.Message;
                    this.App.WriteErrorLog(strError);
                }
            }
            else
                this._queue = null;
        }

        public override string DefaultName
        {
            get
            {
                return "操作日志辅助线程";
            }
        }

        bool _errorWrited = false;  // WriteErrorLog() 是否进行过了。设立此变量是为了避免短时间内往 errorlog 写入大量内容

        /// <summary>
        /// 是否已经启用
        /// </summary>
        public bool Enabled
        {
            get
            {
#if NO
                if (this.App._mongoClient == null || this.App.ChargingOperDatabase == null)
                    return false;
                return true;
#endif
                if (string.IsNullOrEmpty(this.App.OutgoingQueue) == false
                    && StringUtil.IsInList("mq", this.App.CirculationNotifyTypes) == true)
                    return true;
                if (this.App._mongoClient != null && this.App.ChargingOperDatabase != null)
                    return true;
                return false;
            }
        }

        public bool MqNotifyEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(this.App.OutgoingQueue) == false
                    && StringUtil.IsInList("mq", this.App.CirculationNotifyTypes) == true)
                    return true;
                return false;
            }
        }

        // 流通操作日志(mongodb)是否已经启用
        public bool ChargingOperEnabled
        {
            get
            {
                if (this.App._mongoClient != null && this.App.ChargingOperDatabase != null)
                    return true;
                return false;
            }
        }

        // 检测一个 operation 是否需要处理
        public static bool NeedAdd(string strOperation)
        {
            if (strOperation == "borrow" || strOperation == "return")
                return true;
            if (strOperation == "amerce" || strOperation == "setReaderInfo" || strOperation == "setUser")
                return true;

            return false;
        }

        public void AddOperLog(XmlDocument dom)
        {
#if NO
            if (this.App._mongoClient == null || this.App.ChargingOperDatabase == null)
                return;
#endif
            if (this.Enabled == false)
                return;

            this.m_lock.EnterWriteLock();
            try
            {
                if (this._datas.Count > 10000)
                {
                    if (_errorWrited == false)
                    {
                        this.App.WriteErrorLog("OperLogThread 的缓冲空间爆满，已停止追加新事项 (10000 条)");
                        _errorWrited = true;
                    }
                    this.eventActive.Set(); // 提醒线程及时处理
                    return; // 不再允许加入新事项
                }
                this._datas.Add(dom);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            this.eventActive.Set(); // 提醒线程
        }

        // 一次操作循环
        public override void Worker()
        {
            List<XmlDocument> current = new List<XmlDocument>();

            this.m_lock.EnterWriteLock();
            try
            {
                if (this._datas.Count == 0)
                    return;
                current.AddRange(this._datas);
                this._datas.Clear();
                this._errorWrited = false;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            foreach (XmlDocument dom in current)
            {
                string strOperation = DomUtil.GetElementText(dom.DocumentElement,
    "operation");
                if (this.ChargingOperEnabled == true
                    && (strOperation == "borrow" || strOperation == "return"))
                {
                    string strError = "";
                    int nRet = BuildMongoOperDatabase.AppendOperationBorrowReturn(this.App,
                        dom,
                        strOperation,
                        out strError);
                    if (nRet == -1)
                        this.App.WriteErrorLog("OperLogThread 写入 mongodb 日志库时出错: " + strError);
                }

                if ((strOperation == "borrow"
                    || strOperation == "return"
                    || strOperation == "amerce"
                    || strOperation == "setReaderInfo"
                    || strOperation == "setUser")
#if NO
                    && string.IsNullOrEmpty(this.App.OutgoingQueue) == false
                    && StringUtil.IsInList("mq", this.App.CirculationNotifyTypes) == true
#endif
 && this.MqNotifyEnabled == true
                    )
                {
                    // 写入 MSMQ 队列
                    string strError = "";
                    int nRet = SendToQueue(
                        dom,
                        strOperation,
                        out strError);
                    if (nRet == -1)
                        this.App.WriteErrorLog("OperLogThread 写入 MSMQ 队列时出错: " + strError);
                }
            }
        }

        MessageQueue _queue = null;

        int SendToQueue(XmlDocument domOperLog,
            string strOperation,
            out string strError)
        {
            strError = "";

            string strBodyXml = "";
            string strReaderBarcode = "";
            string strReaderRefID = "";

            try
            {
                if (strOperation == "borrow" || strOperation == "return")
                    BuildBorrowReturnRecord(domOperLog,
                    out strBodyXml,
                    out strReaderBarcode,
                    out strReaderRefID);
                else if (strOperation == "amerce")
                    BuildAmerceRecord(domOperLog,
                    out strBodyXml,
                    out strReaderBarcode,
                    out strReaderRefID);
                else if (strOperation == "setReaderInfo")
                    BuildReaderChangedRecord(domOperLog,
                    out strBodyXml,
                    out strReaderBarcode,
                    out strReaderRefID);
                else if (strOperation == "setUser")
                    BuildUserChangedRecord(domOperLog,
                    out strBodyXml,
                    out strReaderBarcode,
                    out strReaderRefID);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            if (string.IsNullOrEmpty(strBodyXml))
                return 0;

            // 向 MSMQ 消息队列发送消息
            return ReadersMonitor.SendToQueue(this._queue,
                (string.IsNullOrEmpty(strReaderRefID) ? strReaderBarcode : "!refID:" + strReaderRefID)
                + "@LUID:" + this.App.UID,
                "xml",
                strBodyXml,
                out strError);
        }

        void BuildBorrowReturnRecord(XmlDocument domOperLog,
            out string strBodyXml,
            out string strReaderBarcode,
            out string strReaderRefID)
        {
            strBodyXml = "";
            strReaderBarcode = "";
            strReaderRefID = "";

            string strError = "";

            string strOperation = DomUtil.GetElementText(domOperLog.DocumentElement, "operation");

            string strLibraryCode = DomUtil.GetElementText(domOperLog.DocumentElement, "libraryCode");

            string strReaderRecord = DomUtil.GetElementText(domOperLog.DocumentElement, "readerRecord");
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(strReaderRecord);

            string strItemRecord = DomUtil.GetElementText(domOperLog.DocumentElement, "itemRecord");
            XmlDocument itemdom = new XmlDocument();
            itemdom.LoadXml(strItemRecord);

            string strItemRecPath = "";
            XmlElement item_record = domOperLog.DocumentElement.SelectSingleNode("itemRecord") as XmlElement;
            if (item_record != null)
                strItemRecPath = item_record.GetAttribute("recPath");

            strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
            strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");

            // 构造内容
            XmlDocument bodydom = new XmlDocument();
            bodydom.LoadXml("<root />");

            if (strOperation == "borrow")
                DomUtil.SetElementText(bodydom.DocumentElement, "type", "借书成功");
            else if (strOperation == "return")
                DomUtil.SetElementText(bodydom.DocumentElement, "type", "还书成功");
            else
            {
#if NO
                strError = "无法识别的 strOperation '" + strOperation + "'";
                throw new Exception(strError);
#endif
                DomUtil.SetElementText(bodydom.DocumentElement, "type", "修改交费注释");
                strBodyXml = "";
                return;
            }

            // 复制日志记录中的一级元素
            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("*");
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "readerRecord" || node.Name == "itemRecord")
                    continue;
                if (node.Name == "type")
                    DomUtil.SetElementText(bodydom.DocumentElement, "bookType", node.InnerText);
                else
                    DomUtil.SetElementText(bodydom.DocumentElement, node.Name, node.InnerText);
            }

            {
                XmlElement record = bodydom.CreateElement("patronRecord");
                bodydom.DocumentElement.AppendChild(record);
                record.InnerXml = readerdom.DocumentElement.InnerXml;

                DomUtil.DeleteElement(record, "borrowHistory");
                DomUtil.DeleteElement(record, "password");
                DomUtil.DeleteElement(record, "fingerprint");
                DomUtil.DeleteElement(record, "face");
                DomUtil.SetElementText(record, "libraryCode", strLibraryCode);
            }

            {
                XmlElement record = bodydom.CreateElement("itemRecord");
                bodydom.DocumentElement.AppendChild(record);
                record.InnerXml = itemdom.DocumentElement.InnerXml;

                string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

                DomUtil.DeleteElement(record, "borrowHistory");
                // 加入书目摘要
                string strSummary = "";
                string strBiblioRecPath = "";
                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "channel == null";
                    throw new Exception(strError);
                }
                LibraryServerResult result = this.App.GetBiblioSummary(
                    null,
                    channel,
                    strItemBarcode,
                    strItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1)
                {
                    // strSummary = result.ErrorInfo;
                }
                else
                {
                }

                DomUtil.SetElementText(record, "summary", strSummary);
            }

            strBodyXml = bodydom.DocumentElement.OuterXml;
        }

        void BuildAmerceRecord(XmlDocument domOperLog,
    out string strBodyXml,
    out string strReaderBarcode,
    out string strReaderRefID)
        {
            strBodyXml = "";
            strReaderBarcode = "";
            strReaderRefID = "";

            string strError = "";

            // string strOperation = DomUtil.GetElementText(domOperLog.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement, "action");
            string strLibraryCode = DomUtil.GetElementText(domOperLog.DocumentElement, "libraryCode");

            string strReaderRecord = DomUtil.GetElementText(domOperLog.DocumentElement, "readerRecord");
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(strReaderRecord);

            strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
            strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");

            // 构造内容
            XmlDocument bodydom = new XmlDocument();
            bodydom.LoadXml("<root />");

            if (strAction == "amerce")
                DomUtil.SetElementText(bodydom.DocumentElement, "type", "交费");
            else if (strAction == "undo")
                DomUtil.SetElementText(bodydom.DocumentElement, "type", "撤销交费");
            else if (strAction == "modifyprice")
            {
                // DomUtil.SetElementText(bodydom.DocumentElement, "type", "变更交费金额");
                strBodyXml = "";
                return;
            }
            else if (strAction == "expire")
                DomUtil.SetElementText(bodydom.DocumentElement, "type", "以停代金到期");
            else if (strAction == "modifycomment")
            {
                // DomUtil.SetElementText(bodydom.DocumentElement, "type", "修改交费注释");
                strBodyXml = "";
                return;
            }
            else
            {
#if NO
                strError = "无法识别的 strAction '" + strAction + "'";
                throw new Exception(strError);
#endif
                strBodyXml = "";
                return;
            }

            // 复制日志记录中的一级元素
            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("*");
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "readerRecord"
                    || node.Name == "oldReaderRecord"
                    || node.Name == "expireOverdues"
                    || node.Name == "amerceItems"
                    || node.Name == "amerceRecord")
                    continue;

                DomUtil.SetElementText(bodydom.DocumentElement, node.Name, node.InnerText);
            }

            {
                XmlElement record = bodydom.CreateElement("patronRecord");
                bodydom.DocumentElement.AppendChild(record);
                record.InnerXml = readerdom.DocumentElement.InnerXml;

                DomUtil.DeleteElement(record, "borrowHistory");
                DomUtil.DeleteElement(record, "password");
                DomUtil.DeleteElement(record, "fingerprint");
                DomUtil.DeleteElement(record, "face");
                DomUtil.SetElementText(record, "libraryCode", strLibraryCode);
            }

            // items
            XmlElement amerce_items = bodydom.DocumentElement.SelectSingleNode("items") as XmlElement;
            if (amerce_items == null)
            {
                amerce_items = bodydom.CreateElement("items");
                bodydom.DocumentElement.AppendChild(amerce_items);
            }

            XmlNodeList amerce_records = domOperLog.DocumentElement.SelectNodes("amerceRecord");
            foreach (XmlElement amerce_record in amerce_records)
            {
                string strAmercedXml = amerce_record.InnerText;
                if (string.IsNullOrEmpty(strAmercedXml))
                    continue;

                string strOverdueString = "";
                string strTemp = "";
                int nRet = LibraryApplication.ConvertAmerceRecordToOverdueString(strAmercedXml,
            out strTemp,
            out strOverdueString,
            out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                XmlDocumentFragment fragment = bodydom.CreateDocumentFragment();
                fragment.InnerXml = strOverdueString;

                amerce_items.AppendChild(fragment);
            }

            // expire 情况要把 expireOverdues/overdue 翻译为 items/overdue 元素
            if (strAction == "expire")
            {
                XmlElement expireOverdues = domOperLog.DocumentElement.SelectSingleNode("expiredOverdues") as XmlElement;
                if (expireOverdues != null)
                    amerce_items.InnerXml = expireOverdues.InnerXml;
            }
            else
            {
                // 留着 amerceItem 元素做测试对照

            }

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                throw new Exception(strError);
            }
            // 为 overdue 元素添加 summary 属性
            XmlNodeList overdues = bodydom.DocumentElement.SelectNodes("items/overdue");
            foreach (XmlElement overdue in overdues)
            {
                string strItemBarcode = overdue.GetAttribute("barcode");
                if (string.IsNullOrEmpty(strItemBarcode))
                    continue;

                // 加入书目摘要
                string strSummary = "";
                string strBiblioRecPath = "";
                LibraryServerResult result = this.App.GetBiblioSummary(
                    null,
                    channel,
                    strItemBarcode,
                    "", // strItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1)
                {
                    // strSummary = result.ErrorInfo;
                }
                else
                    overdue.SetAttribute("summary", strSummary);
            }

            strBodyXml = bodydom.DocumentElement.OuterXml;
        }

        // 注：move 时候可能涉及到读者所从属的 libraryCode 发生变化，需要研究一下如何表达
        // 一个方法是可以多发送一条 delete 动作的通知
        void BuildReaderChangedRecord(XmlDocument domOperLog,
    out string strBodyXml,
    out string strReaderBarcode,
    out string strReaderRefID)
        {
            strBodyXml = "";
            strReaderBarcode = "";
            strReaderRefID = "";

#if NO
            string strOperation = DomUtil.GetElementText(domOperLog.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement, "action");
#endif

            string strLibraryCode = DomUtil.GetElementText(domOperLog.DocumentElement, "libraryCode");

            string strReaderRecord = DomUtil.GetElementText(domOperLog.DocumentElement, "record");
            XmlDocument readerdom = new XmlDocument();
            if (string.IsNullOrEmpty(strReaderRecord) == false)
                readerdom.LoadXml(strReaderRecord);
            else
                readerdom = null;

#if NO
            string strOldReaderRecord = DomUtil.GetElementText(domOperLog.DocumentElement, "oldRecord");
            XmlDocument old_readerdom = new XmlDocument();
            if (string.IsNullOrEmpty(strOldReaderRecord) == false)
                old_readerdom.LoadXml(strOldReaderRecord);
            else
                old_readerdom = null;
#endif

#if NO
            string strReaderRecPath = "";
            XmlElement reader_record = domOperLog.DocumentElement.SelectSingleNode("record") as XmlElement;
            if (reader_record != null)
                strReaderRecPath = reader_record.GetAttribute("recPath");
#endif

            if (readerdom != null)
            {
                strReaderRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
                strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
            }

            // 构造内容
            XmlDocument bodydom = new XmlDocument();
            bodydom.LoadXml("<root />");

            DomUtil.SetElementText(bodydom.DocumentElement, "type", "读者记录变动");

            // 复制日志记录中的一级元素
            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("*");
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "record" || node.Name == "oldRecord"
                    || node.Name == "changedEntityRecord")
                    continue;

                DomUtil.SetElementText(bodydom.DocumentElement, node.Name, node.InnerText);
            }

            if (readerdom != null)
            {
                XmlElement record = bodydom.CreateElement("patronRecord");
                bodydom.DocumentElement.AppendChild(record);
                record.InnerXml = readerdom.DocumentElement.InnerXml;

                DomUtil.DeleteElement(record, "borrowHistory");
                DomUtil.DeleteElement(record, "password");
                DomUtil.DeleteElement(record, "fingerprint");
                DomUtil.DeleteElement(record, "face");

                XmlElement library_code = record.SelectSingleNode("libraryCode") as XmlElement;
                if (library_code == null)
                    DomUtil.SetElementText(record, "libraryCode", strLibraryCode);
            }

#if NO
            if (old_readerdom != null)
            {
                XmlElement record = bodydom.CreateElement("oldPatronRecord");
                bodydom.DocumentElement.AppendChild(record);
                record.InnerXml = old_readerdom.DocumentElement.InnerXml;

                DomUtil.DeleteElement(record, "borrowHistory");
                DomUtil.DeleteElement(record, "password");
                DomUtil.DeleteElement(record, "fingerprint");
                DomUtil.DeleteElement(record, "face");
            }
#endif

            strBodyXml = bodydom.DocumentElement.OuterXml;
        }

        void BuildUserChangedRecord(XmlDocument domOperLog,
out string strBodyXml,
out string strReaderBarcode,
out string strReaderRefID)
        {
            strBodyXml = "";
            strReaderBarcode = "";
            strReaderRefID = "";

#if NO
            string strOperation = DomUtil.GetElementText(domOperLog.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement, "action");

            string strLibraryCode = DomUtil.GetElementText(domOperLog.DocumentElement, "libraryCode");
#endif

            // 构造内容
            XmlDocument bodydom = new XmlDocument();
            bodydom.LoadXml("<root />");

            DomUtil.SetElementText(bodydom.DocumentElement, "type", "工作人员账户变动");

            // 复制日志记录中的一级元素
            {
                XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("*");
                foreach (XmlNode node in nodes)
                {
                    DomUtil.SetElementOuterXml(bodydom.DocumentElement, node.Name, node.OuterXml);
                }
            }

            // 将 oldAccount 和 account 元素中的 password 属性删除
            {
                XmlNodeList nodes = bodydom.DocumentElement.SelectNodes("oldAccount | account");
                foreach (XmlElement node in nodes)
                {
                    if (node.HasAttribute("password") == true)
                        node.RemoveAttribute("password");
                }
            }

            strBodyXml = bodydom.DocumentElement.OuterXml;
        }

        // 从 MSMQ 队列获得若干消息
        // parameters:
        public List<MessageData> GetMessage(int nMaxCount, TimeSpan timeout)
        {
            List<MessageData> results = new List<MessageData>();
            if (nMaxCount == 0)
                return results;
            try
            {
                MessageEnumerator iterator = _queue.GetMessageEnumerator2();
                int i = 0;
                while (iterator.MoveNext(timeout))
                {
                    System.Messaging.Message message = iterator.Current;

                    MessageData record = new MessageData();
                    record.strBody = (string)message.Body;
                    record.strMime = "xml";

                    results.Add(record);
                    // iterator.RemoveCurrent();
                    i++;
                    if (i >= nMaxCount)
                        break;
                }

                return results;
            }
            catch (MessageQueueException ex)
            {
                this.App.WriteErrorLog("GetMessage() 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
            catch (Exception ex)
            {
                this.App.WriteErrorLog("GetMessage() 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }

            return results;
        }

        // 从 MSMQ 队列中移走若干消息
        // parameters:
        public void RemoveMessage(int nCount)
        {
            if (nCount == 0)
                return;

            List<MessageData> results = new List<MessageData>();
            TimeSpan timeout = new TimeSpan(0, 0, 1);

            try
            {
                MessageEnumerator iterator = _queue.GetMessageEnumerator2();
                int i = 0;
                while (iterator.MoveNext(timeout))
                {
                    if (i >= nCount)
                        break;

                    iterator.RemoveCurrent();
                    i++;
                }
                return;
            }
            catch (MessageQueueException ex)
            {
                this.App.WriteErrorLog("RemoveMessage(" + nCount + ") 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
            catch (Exception ex)
            {
                this.App.WriteErrorLog("RemoveMessage(" + nCount + ") 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
        }

    }
}
