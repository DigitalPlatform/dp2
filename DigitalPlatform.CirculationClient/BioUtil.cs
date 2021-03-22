using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.ResultSet;
using DigitalPlatform.Interfaces;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 和生物识别有关的实用功能
    /// 主要是从读者记录中析出 fingerprint 和 face 信息
    /// </summary>
    public class BioUtil : BioBase, IDisposable
    {
        // 2021/3/22
        // 配置参数表
        public IDictionary<string, string> ConfigTable = new Dictionary<string, string>();

        public event GetImageEventHandler GetImage = null;

        public virtual string BioTypeName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual string DriverName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // 算法版本号
        public virtual string AlgorithmVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Lock()
        {
            _lock.EnterWriteLock();
        }

        public void Unlock()
        {
            _lock.ExitWriteLock();
        }

        public void LockForRead()
        {
            _lock.EnterReadLock();
        }

        public void UnlockForRead()
        {
            _lock.ExitReadLock();
        }

        // 设备列表
        List<string> _dev_list = new List<string>();
        public List<string> DeviceList
        {
            get
            {
                return new List<string>(_dev_list);
            }
        }

        public class ReplicationResult : NormalResult
        {
            public string LastDate { get; set; }
            public long LastIndex { get; set; }

            // [out] 返回处理概述信息
            public ProcessInfo ProcessInfo { get; set; }
        }

        public virtual int AddItems(
            List<FingerprintItem> items,
            ProcessInfo info,
            out string strError)
        {
            strError = "尚未重载 AddItems() 函数";
            return -1;
        }

        public virtual int ItemCount
        {
            get
            {
                throw new Exception("尚未重载 ItemCount");
            }
        }

        public virtual NormalResult Init(int dev_index)
        {
            return new NormalResult { Value = -1, ErrorInfo = "尚未重载 Init() 函数" };
        }

        public virtual NormalResult Free()
        {
            return new NormalResult { Value = -1, ErrorInfo = "尚未重载 Free() 函数" };
        }

        // GetRegisterString() 过程中所使用的 CancellationTokenSource 对象
        public CancellationTokenSource _cancelOfRegister = new CancellationTokenSource();

        public virtual void CancelRegisterString()
        {
            _cancelOfRegister?.Cancel();
        }

        public void TriggerGetImage(GetImageEventArgs e)
        {
            this.GetImage?.Invoke(this, e);
        }

        public Image TryGetImage()
        {
            var e = new GetImageEventArgs();
            this.TriggerGetImage(e);
            return e.Image;
        }

        public virtual TextResult GetRegisterString(Image image,
            string strExcludeBarcodes)
        {
            return new TextResult
            {
                Value = -1,
                ErrorInfo = "尚未重载 GetRegisterString() 函数"
            };
        }

        public virtual void StartCapture(CancellationToken token)
        {

        }

        public virtual RecognitionFaceResult RecongnitionFace(Image image,
    CancellationToken token)
        {
            return new RecognitionFaceResult
            {
                Value = -1,
                ErrorInfo = "尚未重载 RecongnitionFaceResult() 函数"
            };
        }

        // 同步
        // 注：中途遇到异常(例如 Loader 抛出异常)，可能会丢失 INSERT_BATCH 条以内的日志记录写入 operlog 表
        // parameters:
        //      strLastDate   处理中断或者结束时返回最后处理过的日期
        //      last_index  处理或中断返回时最后处理过的位置。以后继续处理的时候可以从这个偏移开始
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        public ReplicationResult DoReplication(
            LibraryChannel channel,
            string strStartDate,
            string strEndDate,
            LogType logType,
            string serverVersion,
            CancellationToken token)
        {
            string strLastDate = "";
            long last_index = -1;    // -1 表示尚未处理

            // bool bUserChanged = false;
            ProcessInfo info = new ProcessInfo();

            // strStartDate 里面可能会包含 ":1-100" 这样的附加成分
            StringUtil.ParseTwoPart(strStartDate,
                ":",
                out string strLeft,
                out string strRight);
            strStartDate = strLeft;

            if (string.IsNullOrEmpty(strStartDate) == true)
            {
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = "DoReplication() 出错: strStartDate 参数值不应为空"
                };
            }

            try
            {
                int nRet = OperLogLoader.MakeLogFileNames(strStartDate,
                    strEndDate,
                    true,  // 是否包含扩展名 ".log"
                    out List<string> dates,
                    out string strWarning,
                    out string strError);
                if (nRet == -1)
                {
                    return new ReplicationResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }

                if (dates.Count > 0 && string.IsNullOrEmpty(strRight) == false)
                {
                    dates[0] = dates[0] + ":" + strRight;
                }

                // using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
                {
                    ProgressEstimate estimate = new ProgressEstimate();

                    OperLogLoader loader = new OperLogLoader
                    {
                        Channel = channel,
                        Stop = null, //  this.Progress;
                                     // loader.owner = this;
                        Estimate = estimate,
                        Dates = dates,
                        Level = 0,  // 2019/7/23 注：2 最简略。不知何故以前用了这个级别。缺点是 oldRecord 元素缺乏 InnerText
                        AutoCache = false,
                        CacheDir = "",
                        LogType = logType,
                        Filter = "setReaderInfo",
                        ServerVersion = serverVersion
                    };

                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = new TimeSpan(0, 2, 0);   // 二分钟

                    loader.Prompt += Loader_Prompt;
                    try
                    {
                        // int nRecCount = 0;

                        string strLastItemDate = "";
                        long lLastItemIndex = -1;
                        // TODO: 计算遍历耗费的时间。如果太短了，要想办法让调主知道这一点，放缓重新调用的节奏，以避免 CPU 和网络资源太高
                        foreach (OperLogItem item in loader)
                        {
                            token.ThrowIfCancellationRequested();

                            //if (stop != null)
                            //    stop.SetMessage("正在同步 " + item.Date + " " + item.Index.ToString() + " " + estimate.Text + "...");

                            if (string.IsNullOrEmpty(item.Xml) == true)
                                goto CONTINUE;

                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(item.Xml);
                            }
                            catch (Exception ex)
                            {

                                if (this.HasLoaderPrompt())
                                {
                                    strError = logType.ToString() + "日志记录 " + item.Date + " " + item.Index.ToString() + " XML 装入 DOM 的时候发生错误: " + ex.Message;
                                    MessagePromptEventArgs e = new MessagePromptEventArgs
                                    {
                                        MessageText = strError + "\r\n\r\n是否跳过此条继续处理?\r\n\r\n(确定: 跳过;  取消: 停止全部操作)",
                                        IncludeOperText = true,
                                        // + "\r\n\r\n是否跳过此条继续处理?",
                                        Actions = "yes,cancel"
                                    };
                                    Loader_Prompt(channel, e);
                                    if (e.ResultAction == "cancel")
                                        throw new ChannelException(channel.ErrorCode, strError);
                                    else if (e.ResultAction == "yes")
                                        continue;
                                    else
                                    {
                                        // no 也是抛出异常。因为继续下一批代价太大
                                        throw new ChannelException(channel.ErrorCode, strError);
                                    }
                                }
                                else
                                    throw new ChannelException(channel.ErrorCode, strError);

                            }

                            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                            if (strOperation == "setReaderInfo")
                            {
                                nRet = TraceSetReaderInfo(
                                    dom,
                                    info,
                                    out strError);
                            }
                            else
                                continue;

                            if (nRet == -1)
                            {
                                strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;

                                if (this.HasLoaderPrompt())
                                {
                                    MessagePromptEventArgs e = new MessagePromptEventArgs
                                    {
                                        MessageText = strError + "\r\n\r\n是否跳过此条继续处理?\r\n\r\n(确定: 跳过;  取消: 停止全部操作)",
                                        IncludeOperText = true,
                                        // + "\r\n\r\n是否跳过此条继续处理?",
                                        Actions = "yes,cancel"
                                    };
                                    Loader_Prompt(channel, e);
                                    if (e.ResultAction == "cancel")
                                        throw new Exception(strError);
                                    else if (e.ResultAction == "yes")
                                        continue;
                                    else
                                    {
                                        // no 也是抛出异常。因为继续下一批代价太大
                                        throw new Exception(strError);
                                    }
                                }
                                else
                                    throw new ChannelException(channel.ErrorCode, strError);
                            }

                        // lProcessCount++;
                        CONTINUE:
                            // 便于循环外获得这些值
                            strLastItemDate = item.Date;
                            lLastItemIndex = item.Index + 1;

                            // index = 0;  // 第一个日志文件后面的，都从头开始了
                        }
                        // 记忆
                        strLastDate = strLastItemDate;
                        last_index = lLastItemIndex;
                    }
                    finally
                    {
                        loader.Prompt -= Loader_Prompt;
                        channel.Timeout = old_timeout;
                    }
                }

                return new ReplicationResult
                {
                    Value = last_index == -1 ? 0 : 1,
                    LastDate = strLastDate,
                    LastIndex = last_index,
                    ProcessInfo = info
                };
            }
            catch (ChannelException ex)
            {
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ProcessInfo = info
                };
            }
            catch (InterruptException ex)
            {
                // 2019/7/4
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ProcessInfo = info
                };
            }
            catch (Exception ex)
            {
                string strError = "ReportForm DoReplication() exception: " + ExceptionUtil.GetDebugText(ex);
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    ProcessInfo = info
                };
            }
        }

        // TODO: 考虑有一种机制可以让 fingerprintcenter 或者 facecenter 的操作历史中能显示出变化情况，至少是缓存事项的数量变化
        // SetReaderInfo() API 恢复动作
        /*
<root>
	<operation>setReaderInfo</operation> 操作类型
	<action>...</action> 具体动作。有new change delete move 4种
	<record recPath='...'>...</record> 新记录
    <oldRecord recPath='...'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
    <changedEntityRecord itemBarcode='...' recPath='...' oldBorrower='...' newBorrower='...' /> 若干个元素。表示连带发生修改的册记录
	<operator>test</operator> 操作者
	<operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 操作时间
</root>

注: new 的时候只有<record>元素，delete的时候只有<oldRecord>元素，change的时候两者都有

         * */
        int TraceSetReaderInfo(
            XmlDocument domLog,
            ProcessInfo info,
            out string strError)
        {
            strError = "";

            string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

            if (strAction == "new"
                || strAction == "change"
                || strAction == "move")
            {
                string strNewRecPath = "";
                string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "record",
                    out XmlNode node);
                if (node == null)
                {
                    // 2019/11/5
                    // 注: move 操作，分馆账户获得日志记录时候可能会被 dp2library 滤除 record 元素。
                    // 此种情况可以理解为 delete 操作
                    if (strAction != "move")
                    {
                        strError = $"日志记录中缺<record>元素。日志记录内容如下：{domLog.OuterXml}";
                        return -1;
                    }
                }
                else
                {
                    strNewRecPath = DomUtil.GetAttr(node, "recPath");
                }

                string strOldRecord = "";
                string strOldRecPath = "";
                // if (strAction == "move")
                {
                    strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    /*
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }*/

                    if (node != null)
                    {
                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                        if (string.IsNullOrEmpty(strOldRecPath) == true)
                        {
                            strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                            return -1;
                        }
                    }

                    // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                    // 注意：如果 record 元素都不存在，则应该理解为 delete。如果 record 元素存在，即 recPath 属性存在但 InnerText 不存在，则当作移动过程记录没有变化，即采用 oldRecord 的 InnerText 作为新记录内容
                    if (string.IsNullOrEmpty(strRecord) == true
                        && string.IsNullOrEmpty(strNewRecPath) == false)
                        strRecord = strOldRecord;
                }

                // TODO: change 动作也可能删除 face 元素

                /*
                // 删除旧记录对应的指纹缓存
                if (strAction == "move"
                    && string.IsNullOrEmpty(strOldRecord) == false)
                {
                    if (DeleteFingerPrint(strOldRecord, info, out strError) == -1)
                        return -1;
                }
                */

                /*
                if (AddFingerPrint(strRecord, info, out strError) == -1)
                    return -1;
                    */
                if (ModifyFingerPrint(strOldRecord,
                    strRecord,
                    info,
                    out strError) == -1)
                    return -1;
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

                /*
                if (string.IsNullOrEmpty(strOldRecord) == false)
                {
                    if (DeleteFingerPrint(strOldRecord, info, out strError) == -1)
                        return -1;
                }
                */
                if (ModifyFingerPrint(strOldRecord,
    "<root />",
    info,
    out strError) == -1)
                    return -1;
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

        // 修改指纹缓存，或者删除指纹缓存
        int ModifyFingerPrint(
            string strOldRecord,
            string strNewRecord,
            ProcessInfo info,
            out string strError)
        {
            strError = "";

            XmlDocument old_dom = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(strOldRecord))
                    old_dom.LoadXml("<root />");
                else
                    old_dom.LoadXml(strOldRecord);
            }
            catch (Exception ex)
            {
                strError = $"strOldRecord 装载到 XmlDocument 时出现异常: {ex.Message}";
                return -1;
            }

            XmlDocument new_dom = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(strNewRecord))
                    new_dom.LoadXml("<root />");
                else
                    new_dom.LoadXml(strNewRecord);
            }
            catch (Exception ex)
            {
                strError = $"strNewRecord 装载到 XmlDocument 时出现异常: {ex.Message}";
                return -1;
            }

            string strOldReaderBarcode = GetReaderBarcode(old_dom);
            string strOldFingerPrintString = DomUtil.GetElementText(old_dom.DocumentElement,
                this.ElementName);

            string strNewReaderBarcode = GetReaderBarcode(new_dom);
            //if (string.IsNullOrEmpty(strNewReaderBarcode))
            //    return 0;
            string strNewFingerPrintString = DomUtil.GetElementText(new_dom.DocumentElement,
                this.ElementName);

            // *** 看新旧记录之间 fingerprint 之间的差异。有差异才需要覆盖进入高速缓存

            // 证条码号没有发生变化的情况
            if (strOldReaderBarcode == strNewReaderBarcode)
            {
                if (strOldFingerPrintString == strNewFingerPrintString)
                    return 0;   // 指纹特征没有发生变化

                if (string.IsNullOrEmpty(strOldReaderBarcode))
                    return 0;   // 空条码号忽视处理

                FingerprintItem item = new FingerprintItem
                {
                    FingerprintString = strNewFingerPrintString,
                    ReaderBarcode = strNewReaderBarcode
                };

                // return:
                //      0   成功
                //      其他  失败。错误码
                int nRet = AddItems(
                    new List<FingerprintItem> { item },
                    info,
                    out strError);
                if (nRet != 0)
                    return -1;

                return 1;
            }

            // 证条码号发生了变化的情况。两步处理

            // 1) 删除旧的
            ProcessInfo info1 = new ProcessInfo();

            if (string.IsNullOrEmpty(strOldReaderBarcode) == false)
            {
                FingerprintItem item = new FingerprintItem
                {
                    FingerprintString = "",
                    ReaderBarcode = strOldReaderBarcode
                };
                // return:
                //      0   成功
                //      其他  失败。错误码
                int nRet = AddItems(
                    new List<FingerprintItem> { item },
                    info1,
                    out strError);
                if (nRet != 0)
                    return -1;
            }

            // 2) 增加新的
            ProcessInfo info2 = new ProcessInfo();

            if (string.IsNullOrEmpty(strNewReaderBarcode) == false)
            {
                FingerprintItem item = new FingerprintItem
                {
                    FingerprintString = strNewFingerPrintString,
                    ReaderBarcode = strNewReaderBarcode
                };
                // return:
                //      0   成功
                //      其他  失败。错误码
                int nRet = AddItems(
                    new List<FingerprintItem> { item },
                    info2,
                    out strError);
                if (nRet != 0)
                    return -1;
            }

            if (info != null)
            {
                info.ChangeCount += info1.ChangeCount + info2.ChangeCount;
                info.DeleteCount += info1.DeleteCount + info2.DeleteCount;
                info.NewCount += info1.NewCount + info2.NewCount;
            }
            return 1;
        }

#if NO
        // 写入新记录的指纹缓存，或者删除指纹缓存
        int AddFingerPrint(string strRecord,
            ProcessInfo info,
            out string strError)
        {
            strError = "";

            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(strRecord);

            string strReaderBarcode = GetReaderBarcode(new_dom);
            if (string.IsNullOrEmpty(strReaderBarcode))
                return 0;
            string strFingerPrintString = DomUtil.GetElementText(new_dom.DocumentElement,
                this.ElementName);

            // TODO: 看新旧记录之间 fingerprint 之间的差异。有差异才需要覆盖进入高速缓存
            FingerprintItem item = new FingerprintItem
            {
                FingerprintString = strFingerPrintString,
                ReaderBarcode = strReaderBarcode
            };
            // return:
            //      0   成功
            //      其他  失败。错误码
            int nRet = AddItems(
                new List<FingerprintItem> { item },
                info,
                out strError);
            if (nRet != 0)
                return -1;

            return 1;
        }

#endif

#if NO
        int DeleteFingerPrint(string strOldRecord,
            ProcessInfo info,
            out string strError)
        {
            strError = "";
            XmlDocument old_dom = new XmlDocument();
            old_dom.LoadXml(strOldRecord);

            string strReaderBarcode = GetReaderBarcode(old_dom);
            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                FingerprintItem item = new FingerprintItem
                {
                    FingerprintString = "",
                    ReaderBarcode = strReaderBarcode
                };
                // return:
                //      0   成功
                //      其他  失败。错误码
                int nRet = AddItems(
                    new List<FingerprintItem> { item },
                    info,
                    out strError);
                if (nRet != 0)
                    return -1;
            }
            return 0;
        }

#endif

        static string GetReaderBarcode(XmlDocument dom)
        {
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "barcode");
            if (string.IsNullOrEmpty(strReaderBarcode) == false)
                return strReaderBarcode;

            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID))
                return "";
            return "@refID:" + strRefID;
        }

        public class ReplicationPlan : NormalResult
        {
            public string StartDate { get; set; }
        }

        // 整体获得读者指纹信息以前，预备获得同步计划信息
        // 也就是第一次同步开始的位置信息
        public static ReplicationPlan GetReplicationPlan(LibraryChannel channel)
        {
            // 开始处理时的日期
            string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

            // 获得日志文件中记录的总数
            // parameters:
            //      strDate 日志文件的日期，8 字符
            // return:
            //      -2  此类型的日志在 dp2library 端尚未启用
            //      -1  出错
            //      0   日志文件不存在，或者记录数为 0
            //      >0  记录数
            long lCount = OperLogLoader.GetOperLogCount(
                null,
                channel,
                strEndDate,
                LogType.OperLog,
                out string strError);
            if (lCount < 0)
            {
                // errorCode: "RequestError" 服务器没有响应
                return new ReplicationPlan { Value = -1, ErrorInfo = strError, ErrorCode = channel.ErrorCode.ToString() };
            }
            return new ReplicationPlan { StartDate = strEndDate + ":" + lCount + "-" };
        }

        static int GetDbNamesByCacheDir(string strDir,
            out List<string> dbnames,
            out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            DirectoryInfo di = new DirectoryInfo(strDir);
            FileInfo[] fis = di.GetFiles("*.");
            foreach (var fi in fis)
            {
                dbnames.Add(fi.Name);
            }

            return 1;
        }

        // parameters:
        //      channel 通讯通道。如果为 null，表示希望根据以前的缓存文件初始化生物特征高速缓存，而不是从 dp2library 获取和更新信息
        // return:
        //      -1  出错
        //      >=0   成功。返回实际初始化的事项
        public NormalResult InitFingerprintCache(
            LibraryChannel channel,
            string strDir,
            CancellationToken token)
        {
            string strError = "";

            try
            {
                // 清空以前的全部缓存内容，以便重新建立
                // return:
                //      -1  出错
                //      >=0 实际发送给接口程序的事项数目
                int nRet = CreateFingerprintCache(null, null,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    return new NormalResult { Value = nRet, ErrorInfo = strError };

                // this.Prompt("正在初始化指纹缓存 ...\r\n请不要关闭本窗口\r\n\r\n(在此过程中，与指纹识别无关的窗口和功能不受影响，可前往使用)\r\n");

                List<string> readerdbnames = null;
                if (channel == null)
                {
                    // 根据已存在的缓存文件名列出读者库名
                    nRet = GetDbNamesByCacheDir(strDir,
    out readerdbnames,
    out strError);
                    if (nRet == -1)
                    {
                        return new NormalResult { Value = -1, ErrorInfo = strError, ErrorCode = channel.ErrorCode.ToString() };
                    }
                }
                else
                {
                    nRet = GetCurrentOwnerReaderNameList(
                        channel,
                        out readerdbnames,
                        out strError);
                    if (nRet == -1)
                    {
                        return new NormalResult { Value = -1, ErrorInfo = strError, ErrorCode = channel.ErrorCode.ToString() };
                    }
                }
                if (readerdbnames.Count == 0)
                {
                    strError = $"因当前用户没有管辖任何读者库，初始化{this.BioTypeName}缓存的操作无法完成";
                    return new NormalResult { Value = -1, ErrorInfo = strError };
                }

                this.SetProgress(0, readerdbnames.Count);

                int nCount = 0;
                // 对这些读者库逐个进行高速缓存的初始化
                // 使用 特殊的 browse 格式，以便获得读者记录中的 fingerprint timestamp字符串，或者兼获得 fingerprint string
                // <fingerprint timestamp='XXXX'></fingerprint>
                int i = 0;
                foreach (string strReaderDbName in readerdbnames)
                {
                    // 初始化一个读者库的指纹缓存
                    // return:
                    //      -1  出错
                    //      >=0 实际发送给接口程序的事项数目
                    nRet = BuildOneDbCache(
                        channel,
                        strDir,
                        strReaderDbName,
                        token,
                        out strError);
                    if (nRet == -1)
                        return new NormalResult { Value = -1, ErrorInfo = strError };
                    nCount += nRet;
                    i++;

                    this.SetProgress(i, readerdbnames.Count);
                }

#if NO
                if (nCount == 0)
                {
                    strError = "因当前用户管辖的读者库 " + StringUtil.MakePathList(readerdbnames) + " 中没有任何具有指纹信息的读者记录，初始化指纹缓存的操作没有完成";
                    return -1;
                }
#endif
                if (nCount == 0)
                {
                    strError = $"当前用户管辖的读者库 { StringUtil.MakePathList(readerdbnames) } 中没有任何具有{this.BioTypeName}信息的读者记录，{this.BioTypeName}缓存为空";
                    return new NormalResult();
                }

                this.ShowMessage($"{this.BioTypeName}缓存初始化成功");
                return new NormalResult { Value = nCount };
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return new NormalResult { Value = -1, ErrorInfo = strError };
            }
        }

        // 根据结果集文件初始化指纹高速缓存
        // parameters:
        //      resultset   用于初始化的结果集对象。如果为 null，表示希望清空指纹高速缓存
        //                  一般可用 null 调用一次，然后用多个 resultset 对象逐个调用一次
        // return:
        //      -1  出错
        //      >=0 实际发送给接口程序的事项数目
        int CreateFingerprintCache(DpResultSet resultset,
            ProcessInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.ShowMessage("加入高速缓存");

            try
            {
                if (resultset == null)
                {
                    // 清空以前的全部缓存内容，以便重新建立
                    // return:
                    //      0   成功
                    //      其他  失败。错误码
                    nRet = this.AddItems(
                        null,
                        null,
                        out strError);
                    if (nRet != 0)
                        return -1;

                    return 0;
                }

                int nSendCount = 0;
                long nCount = resultset.Count;
                List<FingerprintItem> items = new List<FingerprintItem>();
                for (long i = 0; i < nCount; i++)
                {
                    DpRecord record = resultset[i];

                    //string strTimestamp = "";
                    //string strBarcode = "";
                    //string strFingerprint = "";
                    ParseResultItemString(record.BrowseText,
out string strTimestamp,
out string strBarcode,
out string strFingerprint);

                    // 2021/1/5
                    // 注意读者证条码号为空的，不要发送出去
                    if (string.IsNullOrEmpty(strBarcode))
                        continue;

                    FingerprintItem item = new FingerprintItem
                    {
                        ReaderBarcode = strBarcode,
                        FingerprintString = strFingerprint
                    };

                    items.Add(item);
                    if (items.Count >= 100)
                    {
                        // return:
                        //      0   成功
                        //      其他  失败。错误码
                        nRet = this.AddItems(
                            items,
                            info,
                            out strError);
                        if (nRet != 0)
                            return -1;

                        nSendCount += items.Count;
                        items.Clear();
                    }
                }

                if (items.Count > 0)
                {
                    // return:
                    //      0   成功
                    //      其他  失败。错误码
                    nRet = this.AddItems(
                        items,
                        info,
                        out strError);
                    if (nRet != 0)
                        return -1;

                    nSendCount += items.Count;
                }

                // Console.Beep(); // 表示读取成功
                return nSendCount;
            }
            finally
            {
            }
        }


        // 获得当前帐户所管辖的读者库名字
        static int GetCurrentOwnerReaderNameList(
            LibraryChannel channel,
            out List<string> readerdbnames,
            out string strError)
        {
            strError = "";
            readerdbnames = new List<string>();
            // int nRet = 0;

            long lRet = channel.GetSystemParameter(null,
    "system",
    "readerDbGroup",
    out string strValue,
    out strError);
            if (lRet == -1)
                return -1;

            // 新方法
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            try
            {
                dom.DocumentElement.InnerXml = strValue;
            }
            catch (Exception ex)
            {
                strError = "category=system,name=readerDbGroup所返回的XML片段在装入InnerXml时出错: " + ex.Message;
                return -1;
            }

            string strLibraryCodeList = channel.LibraryCodeList;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            foreach (XmlElement node in nodes)
            {
                string strLibraryCode = node.GetAttribute("libraryCode");

                if (IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                        continue;
                }

                string strDbName = node.GetAttribute("name");
                readerdbnames.Add(strDbName);
            }

            return 0;
        }

        public static bool IsGlobalUser(string strLibraryCodeList)
        {
            if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                return true;
            return false;
        }

        // 浏览列风格关键字。决定 browse 文件名的一部分
        public string BrowseStyle = "fingerprint";

        // 检索时间戳的途径名
        public string SearchFrom = "指纹时间戳";

        // 读者记录中表示信息的元素名
        public string ElementName = "fingerprint";

        // 初始化一个读者库的指纹缓存
        // parameters:
        //      channel 通讯通道。如果为 null，表示希望根据以前的缓存文件初始化生物特征高速缓存，而不是从 dp2library 获取和更新信息
        // return:
        //      -1  出错
        //      >=0 实际发送给接口程序的事项数目
        int BuildOneDbCache(
            LibraryChannel channel,
            string strDir,
            string strReaderDbName,
            CancellationToken token,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            DpResultSet resultset = null;
            bool bCreate = false;

            Hashtable timestamp_table = new Hashtable();    // recpath --> fingerprint timestamp

            this.ShowMessage(strReaderDbName);

            // 结果集文件名
            string strResultsetFilename = Path.Combine(strDir, strReaderDbName);

            if (File.Exists(strResultsetFilename) == false)
            {
                if (channel == null)
                {
                    strError = $"缓存文件 '{strResultsetFilename}' 不存在，无法进行脱机初始化高速缓存操作";
                    return -1;
                }
                resultset = new DpResultSet(false, false);
                resultset.Create(strResultsetFilename,
                    strResultsetFilename + ".index");
                bCreate = true;
            }
            else
                bCreate = false;

            // 2020/9/25
            // 利用以前的缓存文件建立高速缓存
            if (channel == null)
            {
                resultset = new DpResultSet(false, false);
                resultset.Attach(strResultsetFilename,
        strResultsetFilename + ".index");

                // return:
                //      -2  remoting服务器连接失败。驱动程序尚未启动
                //      -1  出错
                //      >=0 实际发送给接口程序的事项数目
                nRet = CreateFingerprintCache(resultset, null,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    return -1;

                return nRet;
            }

            // *** 第一阶段， 创建新的结果集文件；或者获取全部读者记录中的指纹时间戳

            bool bDone = false;    // 创建情形下 是否完成了写入操作
            try
            {
                long lRet = channel.SearchReader(null,  // stop,
strReaderDbName,
"",
-1,
this.SearchFrom,
"left",
"zh",
null,   // strResultSetName
"", // strOutputStyle
out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.AccessDenied)
                        strError = "用户 " + channel.UserName + " 权限不足: " + strError;
                    return -1;
                }

                if (lRet == 0)
                {
                    // TODO: 这时候如果以前有结果集文件还会残留，但不会影响功能正确性，可以改进为把残留的结果集文件删除
                    return 0;
                }

                long lHitCount = lRet;

                ResultSetLoader loader = new ResultSetLoader(channel,
                    null,
                    null,
                    bCreate == true ? $"id,cols,format:cfgs/browse_{this.BrowseStyle}" : $"id,cols,format:cfgs/browse_{this.BrowseStyle}timestamp",
                    "zh");
                loader.Prompt += this.Loader_Prompt;

                foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                {
                    token.ThrowIfCancellationRequested();

                    this.ShowMessage("正在处理读者记录 " + record.Path);

                    if (bCreate == true)
                    {
                        if (record.Cols == null || record.Cols.Length < 3)
                        {
                            continue;
                            /*
                            strError = $"record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_{this.BrowseStyle}";
                            return -1;
                            */
                        }
                        if (string.IsNullOrEmpty(record.Cols[0]) == true)
                            continue;   // 读者记录中没有指纹信息
                        DpRecord item = new DpRecord(record.Path);
                        // timestamp | barcode | fingerprint
                        item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                        resultset.Add(item);
                    }
                    else
                    {
                        if (record.Cols == null || record.Cols.Length < 1)
                        {
                            continue;
                            /*
                            strError = $"record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_{this.BrowseStyle}timestamp";
                            return -1;
                            */
                        }
                        if (record.Cols.Length < 2)
                        {
                            continue;
                            /*
                            strError = $"record.Cols error ... 需要刷新配置文件 cfgs/browse_{this.BrowseStyle}timestamp 到最新版本";
                            return -1;
                            */
                        }
                        if (string.IsNullOrEmpty(record.Cols[0]) == true)
                            continue;   // 读者记录中没有指纹信息

                        // 记载时间戳
                        // timestamp | barcode 
                        timestamp_table[record.Path] = record.Cols[0] + "|" + record.Cols[1];
                    }
                }

                if (bCreate == true)
                    bDone = true;

                if (bCreate == true)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      >=0 实际发送给接口程序的事项数目
                    nRet = CreateFingerprintCache(resultset, null,
    out strError);
                    if (nRet == -1 || nRet == -2)
                        return -1;

                    return nRet;
                }
            }
            finally
            {
                if (bCreate == true)
                {
                    Debug.Assert(resultset != null, "");
                    if (bDone == true)
                    {
                        resultset.Detach(out string strTemp1,
                            out string strTemp2);
                    }
                    else
                    {
                        // 否则文件会被删除
                        resultset.Close();
                    }
                }
            }

            // 比对时间戳，更新结果集文件
            Hashtable update_table = new Hashtable();   // 需要更新的事项。recpath --> 1
            resultset = new DpResultSet(false, false);
            resultset.Attach(strResultsetFilename,
    strResultsetFilename + ".index");
            try
            {
                long nCount = resultset.Count;
                for (long i = 0; i < nCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    DpRecord record = resultset[i];

                    string strRecPath = record.ID;

                    this.ShowMessage("比对 " + strRecPath);

                    // timestamp | barcode 
                    string strNewTimestamp = (string)timestamp_table[strRecPath];
                    if (strNewTimestamp == null)
                    {
                        // 最新状态下，读者记录已经不存在，需要从结果集中删除
                        resultset.RemoveAt((int)i);
                        i--;
                        nCount--;
                        continue;
                    }

                    // 拆分出证条码号 2013/1/28
                    string strNewBarcode = "";
                    nRet = strNewTimestamp.IndexOf("|");
                    if (nRet != -1)
                    {
                        strNewBarcode = strNewTimestamp.Substring(nRet + 1);
                        strNewTimestamp = strNewTimestamp.Substring(0, nRet);
                    }

                    // 最新读者记录中已经没有指纹信息。例如读者记录中的指纹元素被删除了
                    if (string.IsNullOrEmpty(strNewTimestamp) == true)
                    {
                        // 删除现有事项
                        resultset.RemoveAt((int)i);
                        i--;
                        nCount--;

                        timestamp_table.Remove(strRecPath);
                        continue;
                    }

                    // 取得结果集文件中的原有时间戳字符串
                    string strText = record.BrowseText; // timestamp | barcode | fingerprint
                    nRet = strText.IndexOf("|");
                    if (nRet == -1)
                    {
                        strError = "browsetext 错误，没有 '|' 字符";
                        return -1;
                    }
                    string strOldTimestamp = strText.Substring(0, nRet);
                    // timestamp | barcode | fingerprint
                    string strOldBarcode = strText.Substring(nRet + 1);
                    nRet = strOldBarcode.IndexOf("|");
                    if (nRet != -1)
                    {
                        strOldBarcode = strOldBarcode.Substring(0, nRet);
                    }

                    // 时间戳发生变化，需要更新事项
                    if (strNewTimestamp != strOldTimestamp
                        || strNewBarcode != strOldBarcode)
                    {
                        // 如果证条码号为空，无法建立对照关系，要跳过
                        if (string.IsNullOrEmpty(strNewBarcode) == false)
                            update_table[strRecPath] = 1;

                        // 删除现有事项
                        resultset.RemoveAt((int)i);
                        i--;
                        nCount--;
                    }
                    timestamp_table.Remove(strRecPath);
                }

                // 循环结束后，timestamp_table中剩余的是当前结果集文件中没有包含的那些读者记录路径

                if (update_table.Count > 0)
                {
                    // 获取指纹信息，追加到结果集文件的尾部
                    // parameters:
                    //      update_table   key为读者记录路径
                    AppendFingerprintInfo(
                        channel,
                        resultset,
                        update_table,
                        token);
                }

                // 如果服务器端新增了指纹信息,需要获取后追加到结果集文件尾部
                if (timestamp_table.Count > 0)
                {
                    // 获取指纹信息，追加到结果集文件的尾部
                    // parameters:
                    //      update_table   key为读者记录路径
                    AppendFingerprintInfo(
                        channel,
                        resultset,
                        timestamp_table,
                        token);
                }

                // return:
                //      -2  remoting服务器连接失败。驱动程序尚未启动
                //      -1  出错
                //      >=0 实际发送给接口程序的事项数目
                nRet = CreateFingerprintCache(resultset, null,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    return -1;

                return nRet;
            }
            finally
            {
                resultset.Detach(out string strTemp1, out string strTemp2);
            }
        }

        // 获取指纹信息，追加到结果集文件的尾部
        // parameters:
        //      update_table   key为读者记录路径
        void AppendFingerprintInfo(
            LibraryChannel channel,
            DpResultSet resultset,
            Hashtable update_table,
            CancellationToken token)
        {
            List<string> lines = new List<string>();
            foreach (string recpath in update_table.Keys)
            {
                lines.Add(recpath);
            }

            BrowseLoader loader = new BrowseLoader();
            loader.RecPaths = lines;
            loader.Channel = channel;
            loader.Format = $"id,cols,format:cfgs/browse_{this.BrowseStyle}";
            loader.Prompt += this.Loader_Prompt;

            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
            {
                token.ThrowIfCancellationRequested();

                this.ShowMessage("追加 " + record.Path);

                if (record.Cols == null || record.Cols.Length < 3)
                {
                    continue;
                    /*
                    string strError = $"record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_{this.BrowseStyle}";
                    // TODO: 并发操作的情况下，会在中途出现读者记录被别的前端修改的情况，这里似乎可以continue
                    throw new Exception(strError);
                    */
                }

                // 如果证条码号为空，无法建立对照关系，要跳过
                if (string.IsNullOrEmpty(record.Cols[1]) == true)
                    continue;

                DpRecord item = new DpRecord(record.Path);
                // timestamp | barcode | fingerprint
                item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                resultset.Add(item);
            }
        }

        static void ParseResultItemString(string strText,
out string strTimestamp,
out string strBarcode,
out string strFingerprint)
        {
            strTimestamp = "";
            strBarcode = "";
            strFingerprint = "";

            string[] parts = strText.Split(new char[] { '|' });
            if (parts.Length > 0)
                strTimestamp = parts[0];
            if (parts.Length > 1)
                strBarcode = parts[1];
            if (parts.Length > 2)
                strFingerprint = parts[2];
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~BioUtil() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

#if NO
        public delegate int Delegate_addItems(
List<FingerprintItem> items,
out string strError);

        public delegate void Delegate_setProgress(long current, long total);

        public delegate void Delegate_showMessage(string text);


        public class BioEnv
        {
            public Delegate_addItems AddItems { get; set; }
            public Delegate_setProgress SetProgress { get; set; }
            public Delegate_showMessage ShowMessage { get; set; }
            public MessagePromptEventHandler LoaderPrompt { get; set; }

        }

#endif
    }


    /// <summary>
    /// 获取图象事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetImageEventHandler(object sender,
    GetImageEventArgs e);

    /// <summary>
    /// 获取图象事件的参数
    /// </summary>
    public class GetImageEventArgs : EventArgs
    {
        // [out] 返回图象
        public Image Image { get; set; }
    }

    public class ProcessInfo
    {
        // 新创建数量
        public int NewCount { get; set; }
        // 删除数量
        public int DeleteCount { get; set; }
        // 修改数量
        public int ChangeCount { get; set; }

        /*
        public static ProcessInfo operator+(ProcessInfo info1, ProcessInfo info2)
        {
            ProcessInfo result = new ProcessInfo();
            result.NewCount = info1.NewCount + info2.NewCount;
            result.DeleteCount = info1.DeleteCount + info2.DeleteCount;
            result.ChangeCount = info1.ChangeCount + info2.ChangeCount;
            return result;
        }
        */

        public static void AddTo(ProcessInfo info1, ProcessInfo info2)
        {
            info2.NewCount += info1.NewCount;
            info2.DeleteCount += info1.DeleteCount;
            info2.ChangeCount += info1.ChangeCount;
        }
    }
}

