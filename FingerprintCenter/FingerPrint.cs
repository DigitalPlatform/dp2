using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Drawing;

using libzkfpcsharp;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.ResultSet;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Interfaces;
using DigitalPlatform;

namespace FingerprintCenter
{
    /// <summary>
    /// 指纹功能类
    /// </summary>
    public static class FingerPrint
    {
        public static event SpeakEventHandler Speak = null;
        public static event CapturedEventHandler Captured = null;
        public static event ImageReadyEventHandler ImageReady = null;

        public static event DownloadProgressChangedEventHandler ProgressChanged = null;

        /// <summary>
        /// 提示框事件
        /// </summary>
        public static event MessagePromptEventHandler Prompt = null;

        static int _idSeed = 10;

        static Hashtable _id_barcode_table = new Hashtable();  // id --> barcode
        static Hashtable _barcode_id_table = new Hashtable();  // barcode --> id 

        static IntPtr _dBHandle = IntPtr.Zero;
        static IntPtr _devHandle = IntPtr.Zero;

        // 当前的运行模式
        // read: 读取模式。每次扫入一个指纹，识别出证条码号以后触发事件
        // register: 注册模式。要求读完三个指纹，API 才返回
        static string _mode = "read";
        // 存储注册用的指纹模板
        static List<byte[]> _register_template_list = new List<byte[]>();
        // 注册过程完成
        static AutoResetEvent _eventRegisterFinished = new AutoResetEvent(false);

        public static bool Init()
        {
            List<string> dev_list = new List<string>();
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                // TODO: 允许设置和选择多个指纹阅读器中的一个
                int nCount = zkfp2.GetDeviceCount();
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        dev_list.Add(i.ToString());
                    }
                }
                else
                {
                    zkfp2.Terminate();
                    // MessageBox.Show("No device connected!");
                    return false;
                }

                return true;
            }
            else
            {
                // MessageBox.Show("Initialize fail, ret=" + ret + " !");
                return false;
            }
        }

        public static void Free()
        {
            zkfp2.Terminate();
        }

        public static bool OpenZK()
        {
            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (_devHandle = zkfp2.OpenDevice(0
                //cmbIdx.SelectedIndex
                )))
            {
                // MessageBox.Show("OpenDevice fail");
                return false;
            }
            if (IntPtr.Zero == (_dBHandle = zkfp2.DBInit()))
            {
                // MessageBox.Show("Init DB fail");
                zkfp2.CloseDevice(_devHandle);
                _devHandle = IntPtr.Zero;
                return false;
            }

            _id_barcode_table.Clear();
            _barcode_id_table.Clear();

            return true;
#if NO
            bnInit.Enabled = false;
            bnFree.Enabled = true;
            bnOpen.Enabled = false;
            bnClose.Enabled = true;
            bnEnroll.Enabled = true;
            bnVerify.Enabled = true;
            bnIdentify.Enabled = true;
            RegisterCount = 0;
            cbRegTmp = 0;
            iFid = 1;
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

            FPBuffer = new byte[mfpWidth * mfpHeight];

            Thread captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;
            textRes.Text = "Open succ";
#endif
        }

        public static void CloseZK()
        {
            if (_dBHandle != IntPtr.Zero)
            {
                zkfp2.DBFree(_dBHandle);
                _dBHandle = IntPtr.Zero;
            }
        }

        public static void ClearDB()
        {
            if (_dBHandle != IntPtr.Zero)
            {
                zkfp2.DBClear(_dBHandle);
                _id_barcode_table.Clear();
                _barcode_id_table.Clear();
            }
        }

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        // return:
        //      0   成功
        //      其他  失败。错误码
        public static int AddItems(
            List<FingerprintItem> items,
            out string strError)
        {
            strError = "";

#if NO
            if (this.m_host == null)
            {
                if (Open(out strError) == -1)
                    return -1;
            }
#endif

            // 清除已有的全部缓存内容
            if (items == null || items.Count == 0)
            {
#if NO
                if (this.m_handle != -1)
                {

                    this.m_host.FreeFPCacheDB(this.m_handle);
                    this.m_handle = -1;
                }
#endif
                ClearDB();
                return 0;
            }

#if NO
            if (this.m_handle == -1)
            {
                this.m_handle = this.m_host.CreateFPCacheDB();
                this.id_barcode_table.Clear();
                this.barcode_id_table.Clear();
            }
#endif

            foreach (FingerprintItem item in items)
            {
                int id = _idSeed++;

                // 看看条码号以前是否已经存在?
                if (_barcode_id_table.Contains(item.ReaderBarcode) == true)
                {
                    int nOldID = (int)_barcode_id_table[item.ReaderBarcode];
                    // this.m_host.RemoveRegTemplateFromFPCacheDB(this.m_handle, nOldID);
                    zkfp2.DBDel(_dBHandle, nOldID);

                    _id_barcode_table.Remove(nOldID.ToString());
                    if (string.IsNullOrEmpty(item.FingerprintString) == true)
                        _barcode_id_table.Remove(item.ReaderBarcode);
                }

                if (string.IsNullOrEmpty(item.FingerprintString) == false)
                {
                    _id_barcode_table[id.ToString()] = item.ReaderBarcode;
                    _barcode_id_table[item.ReaderBarcode] = id;

                    try
                    {
                        int ret = zkfp2.DBAdd(_dBHandle, id, zkfp2.Base64ToBlob(item.FingerprintString));
                        if (ret != 0)
                            return ret;
                        // this.m_host.AddRegTemplateStrToFPCacheDB(this.m_handle, id, item.FingerprintString);
                    }
                    catch (Exception ex)
                    {
                        strError = "AddRegTemplateStrToFPCacheDB() error. id=" + id.ToString() + " ,item.FingerprintString='" + item.FingerprintString + "', message=" + ex.Message;
                        return -1;
                    }
                }
            }

            return 0;
        }

        public static void SetProgress(long current, long total)
        {
            if (ProgressChanged != null)
            {
                DownloadProgressChangedEventArgs e = new DownloadProgressChangedEventArgs(current, total);
                ProgressChanged(null, e);
            }
        }

        public static void Speaking(string text)
        {
            if (Speak != null)
            {
                Speak(null, new SpeakEventArgs { Text = text });
            }
        }

        public static void ShowMessage(string text)
        {
            if (ProgressChanged != null)
            {
                DownloadProgressChangedEventArgs e = new DownloadProgressChangedEventArgs(text)
                {
                    BytesReceived = -1,
                    TotalBytesToReceive = -1
                };
                ProgressChanged(null, e);
            }
        }

        // parameters:
        //      bDelayShow  延迟显示当前窗口。如果为 false，表示不操心显示的事儿
        // return:
        //      -2  remoting服务器连接失败。指纹接口程序尚未启动
        //      -1  出错
        //      0   成功
        public static int InitFingerprintCache(
            LibraryChannel channel,
            string strDir,
            CancellationToken token,
            out string strError)
        {
            strError = "";

            // 清空以前的全部缓存内容，以便重新建立
            // return:
            //      -2  remoting服务器连接失败。驱动程序尚未启动
            //      -1  出错
            //      >=0 实际发送给接口程序的事项数目
            int nRet = CreateFingerprintCache(null,
                out strError);
            if (nRet == -1 || nRet == -2)
                return nRet;

            // this.Prompt("正在初始化指纹缓存 ...\r\n请不要关闭本窗口\r\n\r\n(在此过程中，与指纹识别无关的窗口和功能不受影响，可前往使用)\r\n");

            List<string> readerdbnames = null;
            nRet = GetCurrentOwnerReaderNameList(
                channel,
                out readerdbnames,
                out strError);
            if (nRet == -1)
                return -1;
            if (readerdbnames.Count == 0)
            {
                strError = "因当前用户没有管辖任何读者库，初始化指纹缓存的操作无法完成";
                return -1;
            }

            SetProgress(0, readerdbnames.Count);

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
                    return -1;
                nCount += nRet;
                i++;
                SetProgress(i, readerdbnames.Count);
            }

            if (nCount == 0)
            {
                strError = "因当前用户管辖的读者库 " + StringUtil.MakePathList(readerdbnames) + " 中没有任何具有指纹信息的读者记录，初始化指纹缓存的操作没有完成";
                return -1;
            }

            return 0;
        }


        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      >=0 实际发送给接口程序的事项数目
        static int CreateFingerprintCache(DpResultSet resultset,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ShowMessage("加入高速缓存");

            try
            {
                if (resultset == null)
                {
                    // 清空以前的全部缓存内容，以便重新建立
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      0   成功
                    nRet = AddItems(
                        //channel,
                        null,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == -2)
                        return -2;

                    return 0;
                }

                int nSendCount = 0;
                long nCount = resultset.Count;
                List<FingerprintItem> items = new List<FingerprintItem>();
                for (long i = 0; i < nCount; i++)
                {
                    DpRecord record = resultset[i];

                    string strTimestamp = "";
                    string strBarcode = "";
                    string strFingerprint = "";
                    ParseResultItemString(record.BrowseText,
out strTimestamp,
out strBarcode,
out strFingerprint);
                    // TODO: 注意读者证条码号为空的，不要发送出去


                    FingerprintItem item = new FingerprintItem();
                    item.ReaderBarcode = strBarcode;
                    item.FingerprintString = strFingerprint;

                    items.Add(item);
                    if (items.Count >= 100)
                    {
                        // return:
                        //      -2  remoting服务器连接失败。驱动程序尚未启动
                        //      -1  出错
                        //      0   成功
                        nRet = AddItems(
                            //channel,
                            items,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == -2)
                            return -2;
                        nSendCount += items.Count;
                        items.Clear();
                    }
                }

                if (items.Count > 0)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      0   成功
                    nRet = AddItems(
                        //channel,
                        items,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == -2)
                        return -2;
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

        // 初始化一个读者库的指纹缓存
        // return:
        //      -1  出错
        //      >=0 实际发送给接口程序的事项数目
        static int BuildOneDbCache(
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

            ShowMessage(strReaderDbName);

            // 结果集文件名
            string strResultsetFilename = Path.Combine(strDir, strReaderDbName);

            if (File.Exists(strResultsetFilename) == false)
            {
                resultset = new DpResultSet(false, false);
                resultset.Create(strResultsetFilename,
                    strResultsetFilename + ".index");
                bCreate = true;
            }
            else
                bCreate = false;

            // *** 第一阶段， 创建新的结果集文件；或者获取全部读者记录中的指纹时间戳

            bool bDone = false;    // 创建情形下 是否完成了写入操作
            try
            {
                /*
                long lRet = Channel.SearchReader(stop,
        strReaderDbName,
        "1-9999999999",
        -1,
        "__id",
        "left",
        this.Lang,
        null,   // strResultSetName
        "", // strOutputStyle
        out strError);
                */
                long lRet = channel.SearchReader(null,  // stop,
strReaderDbName,
"",
-1,
"指纹时间戳",
"left",
ClientInfo.Lang,
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
                    bCreate == true ? "id,cols,format:cfgs/browse_fingerprint" : "id,cols,format:cfgs/browse_fingerprinttimestamp",
                    ClientInfo.Lang);
                loader.Prompt += Loader_Prompt;

                foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                {
                    token.ThrowIfCancellationRequested();

                    ShowMessage("正在处理读者记录 " + record.Path);

                    if (bCreate == true)
                    {
                        if (record.Cols == null || record.Cols.Length < 3)
                        {
                            strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                            return -1;
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
                            strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprinttimestamp";
                            return -1;
                        }
                        if (record.Cols.Length < 2)
                        {
                            strError = "record.Cols error ... 需要刷新配置文件 cfgs/browse_fingerprinttimestamp 到最新版本";
                            return -1;
                        }
                        if (string.IsNullOrEmpty(record.Cols[0]) == true)
                            continue;   // 读者记录中没有指纹信息

                        // 记载时间戳
                        // timestamp | barcode 
                        timestamp_table[record.Path] = record.Cols[0] + "|" + record.Cols[1];
                    }
                }

#if NO
                // stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    token.ThrowIfCancellationRequested();


                    lRet = channel.GetSearchResult(
                        null,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        bCreate == true ? "id,cols,format:cfgs/browse_fingerprint" : "id,cols,format:cfgs/browse_fingerprinttimestamp",
                        ClientInfo.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                    {
                        strError = "GetSearchResult() return 0";
                        return -1;
                    }

                    Debug.Assert(searchresults != null, "");
                    Debug.Assert(searchresults.Length > 0, "");

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record record = searchresults[i];
                        if (bCreate == true)
                        {
                            if (record.Cols == null || record.Cols.Length < 3)
                            {
                                strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                                return -1;
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
                                strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprinttimestamp";
                                return -1;
                            }
                            if (record.Cols.Length < 2)
                            {
                                strError = "record.Cols error ... 需要刷新配置文件 cfgs/browse_fingerprinttimestamp 到最新版本";
                                return -1;
                            }
                            if (string.IsNullOrEmpty(record.Cols[0]) == true)
                                continue;   // 读者记录中没有指纹信息

                            // 记载时间戳
                            // timestamp | barcode 
                            timestamp_table[record.Path] = record.Cols[0] + "|" + record.Cols[1];
                        }
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    //stop.SetMessage(strReaderDbName + " 包含记录 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                    //stop.SetProgressValue(lStart);
                }

#endif

                if (bCreate == true)
                    bDone = true;

                if (bCreate == true)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      >=0 实际发送给接口程序的事项数目
                    nRet = CreateFingerprintCache(resultset,
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

                    ShowMessage("比对 " + strRecPath);

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
                nRet = CreateFingerprintCache(resultset,
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

        private static void Loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (Prompt != null)
                Prompt(sender, e);
        }

        // 获取指纹信息，追加到结果集文件的尾部
        // parameters:
        //      update_table   key为读者记录路径
        static void AppendFingerprintInfo(
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
            loader.Format = "id,cols,format:cfgs/browse_fingerprint";
            loader.Prompt += Loader_Prompt;

            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
            {
                token.ThrowIfCancellationRequested();

                ShowMessage("追加 " + record.Path);

                if (record.Cols == null || record.Cols.Length < 3)
                {
                    string strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                    // TODO: 并发操作的情况下，会在中途出现读者记录被别的前端修改的情况，这里似乎可以continue
                    throw new Exception(strError);
                }

                // 如果证条码号为空，无法建立对照关系，要跳过
                if (string.IsNullOrEmpty(record.Cols[1]) == true)
                    continue;

                DpRecord item = new DpRecord(record.Path);
                // timestamp | barcode | fingerprint
                item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                resultset.Add(item);
            }
#if NO
            // 需要获得更新的事项，然后追加到结果集文件的尾部
            // 注意，需要定期彻底重建结果集文件，以便回收多余空间
            List<string> lines = new List<string>();
            foreach (string recpath in update_table.Keys)
            {
                lines.Add(recpath);
                if (lines.Count >= 100)
                {
                    List<DigitalPlatform.LibraryClient.localhost.Record> records = null;
                    GetSomeFingerprintData(
                        channel,
                        lines,
                        token,
    out records);

                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in records)
                    {
                        if (record.Cols == null || record.Cols.Length < 3)
                        {
                            strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                            // TODO: 并发操作的情况下，会在中途出现读者记录被别的前端修改的情况，这里似乎可以continue
                            return -1;
                        }

                        // 如果证条码号为空，无法建立对照关系，要跳过
                        if (string.IsNullOrEmpty(record.Cols[1]) == true)
                            continue;

                        DpRecord item = new DpRecord(record.Path);
                        // timestamp | barcode | fingerprint
                        item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                        resultset.Add(item);
                    }
                    lines.Clear();
                }
            }

            if (lines.Count > 0)
            {
                List<DigitalPlatform.LibraryClient.localhost.Record> records = null;
                GetSomeFingerprintData(
                    channel,
                    lines,
                    token,
out records);

                foreach (DigitalPlatform.LibraryClient.localhost.Record record in records)
                {
                    if (record.Cols == null || record.Cols.Length < 3)
                    {
                        strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                        // TODO: 并发操作的情况下，会在中途出现读者记录被别的前端修改的情况，这里似乎可以continue
                        return -1;
                    }
                    DpRecord item = new DpRecord(record.Path);
                    // timestamp | barcode | fingerprint
                    item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                    resultset.Add(item);
                }
            }

            return 0;
#endif
        }

#if NO
        // 处理一小批指纹数据的装入
        // parameters:
        static void GetSomeFingerprintData(
            LibraryChannel channel,
            List<string> lines,
            CancellationToken token,
            out List<DigitalPlatform.LibraryClient.localhost.Record> records)
        {
            // strError = "";

            records = new List<DigitalPlatform.LibraryClient.localhost.Record>();

            for (; ; )
            {
                token.ThrowIfCancellationRequested();

                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                string[] paths = new string[lines.Count];
                lines.CopyTo(paths);
                REDO_GETRECORDS:
                long lRet = channel.GetBrowseRecords(
                    null,
                    paths,
                    "id,cols,format:cfgs/browse_fingerprint",
                    out searchresults,
                    out string strError);
                if (lRet == -1)
                {
#if NO
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    "ReaderSearchForm",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETRECORDS;
                    return -1;
#endif

                    if (Prompt != null)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs
                        {
                            // e.MessageText = "获得书目记录 '"+strCommand+"' ("+StringUtil.MakePathList(format_list)+") 时发生错误： " + strError;
                            MessageText = strError + "\r\n\r\n是否重试?",
                            Actions = "yes,no,cancel"
                        };
                        Prompt(channel, e);
                        if (e.ResultAction == "cancel")
                            throw new ChannelException(channel.ErrorCode, strError);
                        else if (e.ResultAction == "yes")
                            goto REDO_GETRECORDS;
                        else
                        {
                            // no 也是抛出异常。因为继续下一批代价太大
                            throw new ChannelException(channel.ErrorCode, strError);
                        }
                    }
                    else
                        throw new ChannelException(channel.ErrorCode, strError);

                }

                records.AddRange(searchresults);

                // 去掉已经做过的一部分
                lines.RemoveRange(0, searchresults.Length);

                if (lines.Count == 0)
                    break;
            }
        }
#endif
        public static bool IsGlobalUser(string strLibraryCodeList)
        {
            if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                return true;
            return false;
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

        // Capture 过程中用到的变量
        class CaptureData
        {
            // StartCapture() 所使用的 token。记忆下来使用
            public CancellationToken _cancelToken = new CancellationToken();
            public byte[] CapTmp = new byte[2048];
            public int cbCapTmp = 2048;
            // public byte[] FPBuffer;

            public int mfpWidth = 0;
            public int mfpHeight = 0;
        }

        static CaptureData _captureData = new CaptureData();

        const int PARAMETER_PICTURE_WIDTH = 1;  // 图象宽
        const int PARAMETER_PICTURE_HEIGHT = 2; // 图象高

        // index:
        //      1   图像宽
        //      2   图像高
        static int GetIntParameter(int index)
        {
            int value = 0;
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(_devHandle, index, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref value);
            return value;
        }

        public static void StartCapture(CancellationToken token)
        {
            _captureData.mfpWidth = GetIntParameter(PARAMETER_PICTURE_WIDTH);
            _captureData.mfpHeight = GetIntParameter(PARAMETER_PICTURE_HEIGHT);

            // _captureData.FPBuffer = new byte[_captureData.mfpWidth * _captureData.mfpHeight];

            _register_template_list.Clear();

            Thread captureThread = new Thread(new ThreadStart(CaptureThreadMain));
            captureThread.IsBackground = true;
            captureThread.Start();
            _captureData._cancelToken = token;
        }

        static void CaptureThreadMain()
        {
            while (!_captureData._cancelToken.IsCancellationRequested)
            {
                byte[] image_buffer = new byte[_captureData.mfpWidth * _captureData.mfpHeight];

                byte[] template_buffer = new byte[2048];
                int template_buffer_length = 2048;
                int ret = zkfp2.AcquireFingerprint(_devHandle,
                    image_buffer,
                    template_buffer,
                    ref template_buffer_length);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    // SendMessage(FormHandle, MESSAGE_CAPTURED_OK, IntPtr.Zero, IntPtr.Zero);
                    ProcessCaptureData(image_buffer,
                        template_buffer, 
                        template_buffer_length);
                }
                Thread.Sleep(200);
            }
        }

        // parameters:
        //      template_buffer 指纹模板数据
        //      length  template_buffer 数组内有效数据长度
        static void ProcessCaptureData(
            byte[] image_buffer,
            byte[] template_buffer,
            int length)
        {
            if (ImageReady != null)
            {
                Task.Run(() =>
                {
                    MemoryStream ms = new MemoryStream();
                    BitmapFormat.GetBitmap(image_buffer,
                        _captureData.mfpWidth,
                        _captureData.mfpHeight,
                        ref ms);
                    ImageReady(null, new ImageReadyEventArgs { Image = new Bitmap(ms) });
                });
            }

            if (_mode == "register")
            {
#if NO
                // 查重
                {
                    int id = 0, score = 0;
                    int ret = zkfp2.DBIdentify(mDBHandle, template_buffer, ref id, ref score);
                    if (zkfp.ZKFP_ERR_OK == ret)
                    {
                        // TODO: 需要把 id 翻译为号码字符串显示
                        Speaking("您的指纹以前已经注册过了(id=" + id + ")，无法重复注册");
                        return;
                    }
                }
#endif

                // 和上一次的比对
                if (_register_template_list.Count > 0)
                {
                    int nRet = zkfp2.DBMatch(_dBHandle, template_buffer, _register_template_list[_register_template_list.Count - 1]);
                    if (nRet <= 0)
                    {
                        Speaking("刚扫入的指纹质量不佳，请继续重新扫入");
                        return;
                    }
                }

                {
                    byte[] buffer = new byte[length];
                    Array.Copy(template_buffer, buffer, length);

                    _register_template_list.Add(buffer);
                }

                if (_register_template_list.Count >= 3)
                {
                    // 结束 register 轮回
                    _eventRegisterFinished.Set();
                    return;
                }
                Speaking("很好。还需要扫入 " + (3 - _register_template_list.Count) + " 个指纹");
                return;
            }

            {
                int ret = zkfp.ZKFP_ERR_OK;
                int fid = 0, score = 0;
                ret = zkfp2.DBIdentify(_dBHandle, template_buffer, ref fid, ref score);
                if (zkfp.ZKFP_ERR_OK == ret)
                {
                    //textRes.Text = "Identify succ, fid= " + fid + ",score=" + score + "!";
                    //return;

                    // 根据 id 取出 barcode 字符串，然后发送给当前焦点 textbox
                    string strBarcode = (string)_id_barcode_table[fid.ToString()];
                    //SafeBeep(1);
                    CapturedEventArgs e1 = new CapturedEventArgs
                    {
                        Text = strBarcode,
                        Score = score
                    };
                    Captured(null, e1);
                    // SendKeys.SendWait(strBarcode + "\r");
                    //if (this.BeepOn == false)
                    //    Speak("很好");

                    // 闪绿灯
                    //SafeLight("green");
                }
                else
                {
                    //textRes.Text = "Identify fail, ret= " + ret;
                    //return;
                    CapturedEventArgs e1 = new CapturedEventArgs
                    {
                        ErrorInfo = "Identify fail, ret= " + ret
                    };
                    Captured(null, e1);
                }
            }
        }

        // GetRegisterString() 过程中所使用的 CancellationTokenSource 对象
        static CancellationTokenSource _cancelOfRegister = new CancellationTokenSource();

        public static void CancelRegisterString()
        {
            _cancelOfRegister.Cancel();
        }

        // exception:
        //      可能会抛出异常。在 token 中断时
        public static TextResult GetRegisterString()
        {
            string save_mode = _mode;
            ClientInfo.MainForm?.ActivateWindow(true);
            ClientInfo.MainForm?.DisplayCancelButton(true);
            try
            {
                _mode = "register";
                _register_template_list.Clear();
                _cancelOfRegister = new CancellationTokenSource();
                _eventRegisterFinished.Reset();
                Speaking("请扫入指纹。一共需要扫三遍");

                while (_eventRegisterFinished.WaitOne(TimeSpan.FromMilliseconds(500)) == false)
                {
                    _cancelOfRegister.Token.ThrowIfCancellationRequested();
                }

                byte[] buffer = new byte[2048];
                int length = 0; // buffer.Length;
                int nRet = zkfp2.DBMerge(_dBHandle, _register_template_list[0], _register_template_list[1], _register_template_list[2], buffer, ref length);
                if (nRet != zkfp.ZKFP_ERR_OK)
                {
                    Speaking("非常抱歉，合成指纹时发生错误");
                    return new TextResult { Value = -1, ErrorInfo = "合成模板时发生错误，错误码=" + nRet };
                }

                Speaking("获取指纹信息成功");
                return new TextResult { Value = 0, Text = zkfp2.BlobToBase64(buffer, length) };
            }
            finally
            {
                ClientInfo.MainForm?.ActivateWindow(false);
                ClientInfo.MainForm?.DisplayCancelButton(false);
                _mode = save_mode;
            }
        }



#if NO
        public static bool Identify()
        {

            int ret = zkfp.ZKFP_ERR_OK;
            int fid = 0, score = 0;
            ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
            if (zkfp.ZKFP_ERR_OK == ret)
            {
                textRes.Text = "Identify succ, fid= " + fid + ",score=" + score + "!";
                return;
            }
            else
            {
                textRes.Text = "Identify fail, ret= " + ret;
                return;
            }
        }
#endif
    }

#if NO
    public class FingerprintItem
    {
        // 指纹特征字符串
        public string FingerprintString = "";
        // 读者证条码号
        public string ReaderBarcode = "";
    }
#endif

    /// <summary>
    /// 捕获完成提示事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void CapturedEventHandler(object sender,
        CapturedEventArgs e);

    /// <summary>
    /// 捕获完成事件的参数
    /// </summary>
    public class CapturedEventArgs : EventArgs
    {
        // 识别出的号码文字
        public string Text { get; set; }
        public int Score { get; set; }
        public string ErrorInfo { get; set; }
    }

    public delegate void ImageReadyEventHandler(object sender,
    ImageReadyEventArgs e);

    /// <summary>
    /// 捕获完成事件的参数
    /// </summary>
    public class ImageReadyEventArgs : EventArgs
    {
        public Image Image { get; set; }    // 注意，使用者要负责 Dispose() 这个 Image 对象
    }

    public delegate void SpeakEventHandler(object sender,
    SpeakEventArgs e);

    public class SpeakEventArgs : EventArgs
    {
        public string Text { get; set; }
    }

}
