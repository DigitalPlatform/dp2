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

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Interfaces;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace FingerprintCenter
{
    /// <summary>
    /// 指纹功能类
    /// </summary>
    public class FingerPrint : BioUtil
    {
#if NO
        public static event SpeakEventHandler Speak = null;
        public static event CapturedEventHandler Captured = null;
        public static event ImageReadyEventHandler ImageReady = null;

        public static event DownloadProgressChangedEventHandler ProgressChanged = null;

        /// <summary>
        /// 提示框事件
        /// </summary>
        public static event MessagePromptEventHandler Prompt = null;
#endif
        int _idSeed = 10;

        Hashtable _id_barcode_table = new Hashtable();  // id --> barcode
        Hashtable _barcode_id_table = new Hashtable();  // barcode --> id 

        IntPtr _dBHandle = IntPtr.Zero;
        IntPtr _devHandle = IntPtr.Zero;

        // 当前的运行模式
        // read: 读取模式。每次扫入一个指纹，识别出证条码号以后触发事件
        // register: 注册模式。要求读完三个指纹，API 才返回
        string _mode = "read";
        // 存储注册用的指纹模板
        List<byte[]> _register_template_list = new List<byte[]>();
        // 注册过程中，查重时，需要排除条码号
        List<string> _exclude = new List<string>();
        // 注册过程完成
        AutoResetEvent _eventRegisterFinished = new AutoResetEvent(false);

        public const int DefaultThreshold = 10;

        int _shreshold = DefaultThreshold;
        public int Shreshold
        {
            get
            {
                return _shreshold;
            }
            set
            {
                _shreshold = value;
            }
        }

        public FingerPrint()
        {
            BrowseStyle = "fingerprint";
            SearchFrom = "指纹时间戳";
            ElementName = "fingerprint";
        }

        public override NormalResult Init(int dev_index)
        {
            try
            {
                NormalResult result = _init();
                if (result.Value == -1)
                    return result;
                return OpenZK(dev_index);
            }
            catch (Exception ex)
            {
                if (ex.Source == "libzkfpcsharp"
                    && ex.Message.IndexOf("libzkfp.dll") != -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorCode = "driver not install",
                        ErrorInfo = "尚未安装'中控'指纹仪厂家驱动程序"
                    };
                }
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public override NormalResult Free()
        {
            try
            {
                _free();
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
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

        NormalResult _init()
        {
            _free();

            _dev_list.Clear();
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                // TODO: 允许设置和选择多个指纹阅读器中的一个
                int nCount = zkfp2.GetDeviceCount();
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        _dev_list.Add(i.ToString());
                    }
                }
                else
                {
                    zkfp2.Terminate();
                    // MessageBox.Show("No device connected!");
                    return new NormalResult { Value = -1, ErrorInfo = "尚未连接指纹阅读器" };
                }

                return new NormalResult();
            }
            else
            {
                // MessageBox.Show("Initialize fail, ret=" + ret + " !");
                string message = $"初始化失败，错误码: {ret}";
                if (ret == -1)
                    message = "尚未连接指纹阅读器";

                return new NormalResult
                {
                    Value = -1,
                    ErrorCode = "fingerprint:" + ret.ToString(),
                    ErrorInfo = message
                };
            }
        }

        void _free()
        {
            zkfp2.Terminate();
        }

        NormalResult OpenZK(int dev_index)
        {
            CloseZK();

            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (_devHandle = zkfp2.OpenDevice(dev_index
                //cmbIdx.SelectedIndex
                )))
            {
                // MessageBox.Show("OpenDevice fail");
                return new NormalResult { Value = -1, ErrorInfo = "打开设备失败" };
            }
            if (IntPtr.Zero == (_dBHandle = zkfp2.DBInit()))
            {
                // MessageBox.Show("Init DB fail");
                //zkfp2.CloseDevice(_devHandle);
                //_devHandle = IntPtr.Zero;
                CloseZK();
                return new NormalResult { Value = -1, ErrorInfo = "初始化高速缓存失败" };
            }

            _id_barcode_table.Clear();
            _barcode_id_table.Clear();

#if NO
            int old_value = GetIntParameter(3);

            byte[] value = new byte[4];
            bool bRet = zkfp2.Int2ByteArray(500, value);
            ret = zkfp2.SetParameters(_devHandle, 3, value, 4);
#endif

            return new NormalResult();


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

        void CloseZK()
        {
            if (_dBHandle != IntPtr.Zero)
            {
                zkfp2.DBFree(_dBHandle);
                _dBHandle = IntPtr.Zero;
            }

            if (_devHandle != IntPtr.Zero)
            {
                zkfp2.CloseDevice(_devHandle);
                _devHandle = IntPtr.Zero;
            }
        }

        public void Light(string strColor, int duration = 500)
        {
            Task.Run(() =>
            {
                int code = 101;
                if (strColor == "white")
                    code = 101;
                else if (strColor == "green")
                    code = 102;
                else if (strColor == "red")
                    code = 103;

                byte[] value = new byte[4];
                bool bRet = zkfp2.Int2ByteArray(1, value);
                int ret = zkfp2.SetParameters(_devHandle, code, value, 4);

                Thread.Sleep(duration);

                bRet = zkfp2.Int2ByteArray(0, value);
                ret = zkfp2.SetParameters(_devHandle, code, value, 4);
            });
        }

        public void ClearDB()
        {
            if (_dBHandle != IntPtr.Zero)
            {
                int ret = zkfp2.DBClear(_dBHandle);
                _id_barcode_table.Clear();
                _barcode_id_table.Clear();
            }
        }

        int RemoveItem(string strReaderBarcode,
            out string strError)
        {
            return AddItems(new List<FingerprintItem> { new FingerprintItem { ReaderBarcode = strReaderBarcode } },
                out strError);
        }

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        // return:
        //      0   成功
        //      其他  失败。错误码
        public override int AddItems(
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
            if (_dBHandle == IntPtr.Zero)
            {
                strError = "指纹设备尚未初始化";
                return -1;
            }

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
            List<string> failed_barcodes = new List<string>();

            foreach (FingerprintItem item in items)
            {
                // TODO: 这里可以尝试检查拟加入的指纹模板是否在高速缓存中已经存在

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
                    int id = _idSeed++;

                    _id_barcode_table[id.ToString()] = item.ReaderBarcode;
                    _barcode_id_table[item.ReaderBarcode] = id;

                    try
                    {
                        int ret = zkfp2.DBAdd(_dBHandle, id, zkfp2.Base64ToBlob(item.FingerprintString));
                        if (ret != 0)
                        {
                            _id_barcode_table.Remove(id.ToString());
                            _barcode_id_table.Remove(item.ReaderBarcode);

                            // 可能是因为加入了重复或者相似的指纹模板导致
                            //strError = $"DBAdd() 失败，证条码号={item.ReaderBarcode}, 错误码={ret}";
                            //return ret;
                            failed_barcodes.Add(item.ReaderBarcode + "|" + ret);
                        }
                        // this.m_host.AddRegTemplateStrToFPCacheDB(this.m_handle, id, item.FingerprintString);
                    }
                    catch (Exception ex)
                    {
                        strError = "AddRegTemplateStrToFPCacheDB() error. id=" + id.ToString() + " ,item.FingerprintString='" + item.FingerprintString + "', message=" + ex.Message;
                        return -1;
                    }
                }
            }

            if (failed_barcodes.Count > 0)
            {
                strError = "下列证条码号对应的指纹模板加入高速缓存时失败: " + StringUtil.MakePathList(failed_barcodes);
                return -1;
            }

            return 0;
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

#if NO
        public static int InitFingerprintCache(
    LibraryChannel channel,
    string strDir,
    CancellationToken token,
    out string strError)
        {
            BioEnv env = new BioEnv {
                AddItems = AddItems,
                SetProgress = SetProgress,
                ShowMessage = ShowMessage,
                LoaderPrompt = Loader_Prompt,
            };
            return BioUtil.InitFingerprintCache(
            channel,
            strDir,
            token,
            env,
            out strError);
        }
#endif

        // Capture 过程中用到的变量
        class CaptureData
        {
            // StartCapture() 所使用的 token。记忆下来使用
            //  public CancellationToken _cancelToken = new CancellationToken();
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
        int GetIntParameter(int index)
        {
            int value = 0;
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(_devHandle, index, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref value);
            return value;
        }

        public override void StartCapture(CancellationToken token)
        {
            LibraryChannelManager.Log?.Debug($"StartCapture()");

            _captureData.mfpWidth = GetIntParameter(PARAMETER_PICTURE_WIDTH);
            _captureData.mfpHeight = GetIntParameter(PARAMETER_PICTURE_HEIGHT);

            // _captureData.FPBuffer = new byte[_captureData.mfpWidth * _captureData.mfpHeight];

            _register_template_list.Clear();
            _exclude.Clear();

            Thread captureThread = new Thread(new ParameterizedThreadStart(CaptureThreadMain));
            // captureThread.IsBackground = true;
            captureThread.Start(token);
            // _captureData._cancelToken = token;
        }

        void CaptureThreadMain(object obj)
        {
            CancellationToken token = (CancellationToken)obj;

            LibraryChannelManager.Log?.Debug($"Begin CaptureThreadMain()");
            try
            {
                // while (!_captureData._cancelToken.IsCancellationRequested)
                while (!token.IsCancellationRequested)
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
                    Task.Delay(200, token).Wait(token);
                    // Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                LibraryChannelManager.Log?.Error($"*** CaptureThreadMain() Exception: {ExceptionUtil.GetExceptionText(ex)}");
            }
            finally
            {
                LibraryChannelManager.Log?.Debug($"End CaptureThreadMain()");
            }
        }

        // parameters:
        //      template_buffer 指纹模板数据
        //      length  template_buffer 数组内有效数据长度
        void ProcessCaptureData(
            byte[] image_buffer,
            byte[] template_buffer,
            int length)
        {
            if (this.HasImageReady())
            {
                Task.Run(() =>
                {
                    // TODO: 注意检查这里是否会出现内存泄漏
                    MemoryStream ms = new MemoryStream();
                    BitmapFormat.GetBitmap(image_buffer,
                        _captureData.mfpWidth,
                        _captureData.mfpHeight,
                        ref ms);
                    TriggerImageReady(null, new ImageReadyEventArgs { Image = new Bitmap(ms) });
                });
            }

            if (_mode == "register")
            {
                // 查重
                {
                    int id = 0, score = 0;
                    int ret = zkfp2.DBIdentify(_dBHandle, template_buffer, ref id, ref score);
                    if (zkfp.ZKFP_ERR_OK == ret)
                    {
                        // 根据 id 取出 barcode 字符串
                        string strBarcode = (string)_id_barcode_table[id.ToString()];
                        if (_exclude.IndexOf(strBarcode) == -1)
                        {
                            Speaking($"您的指纹以前已经被 {strBarcode} 注册过了(id={id})，无法重复注册");
                            return;
                        }
                    }
                }

                // 和上一次的比对
                if (_register_template_list.Count > 0)
                {
                    int nRet = zkfp2.DBMatch(_dBHandle, template_buffer, _register_template_list[_register_template_list.Count - 1]);
                    if (nRet <= 0)
                    {
                        _register_template_list.Clear();    // 从头来
                        Light("red");
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
                    Light("green");
                    // 结束 register 轮回
                    _eventRegisterFinished.Set();
                    return;
                }
                Light("green");
                Speaking("很好。还需要扫入 " + (3 - _register_template_list.Count) + " 个指纹");
                return;
            }

            {
                int ret = zkfp.ZKFP_ERR_OK;
                int fid = 0, score = 0;
                ret = zkfp2.DBIdentify(_dBHandle, template_buffer, ref fid, ref score);
                Debug.WriteLine(string.Format("ret={0}, fid={1}, score={2}", ret, fid, score));
                if (score >= _shreshold
                    // zkfp.ZKFP_ERR_OK == ret
                    )
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
                    Light("green");
                    TriggerCaptured(null, e1);
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
                        Score = score,
                        ErrorInfo = $"无法识别, 错误码={ret}"
                    };
                    Light("red");
                    TriggerCaptured(null, e1);
                }
            }
        }

        // exception:
        //      可能会抛出异常。在 token 中断时
        public override TextResult GetRegisterString(
            Image image,
            string strExcludeBarcodes)
        {
            string save_mode = _mode;
            try
            {
                _mode = "register";
                _register_template_list.Clear();
                _exclude = StringUtil.SplitList(strExcludeBarcodes);
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

                // 尝试加入高速缓存
                {
                    string temp_id = Guid.NewGuid().ToString();
                    nRet = AddItems(new List<FingerprintItem> {
                        new FingerprintItem {
                            ReaderBarcode = temp_id,
                            FingerprintString = zkfp2.BlobToBase64(buffer, length)
                        }
                        },
                        out string strError);
                    if (nRet == -1)
                        return new TextResult { Value = -1, ErrorInfo = "尝试加入高速缓存时失败" };

                    RemoveItem(temp_id, out strError);
                }

                Speaking("获取指纹信息成功");
                return new TextResult { Value = 0, Text = zkfp2.BlobToBase64(buffer, length) };
            }
            finally
            {
                _mode = save_mode;
            }
        }

#if NO
        // 同步
        // 注：中途遇到异常(例如 Loader 抛出异常)，可能会丢失 INSERT_BATCH 条以内的日志记录写入 operlog 表
        // parameters:
        //      strLastDate   处理中断或者结束时返回最后处理过的日期
        //      last_index  处理或中断返回时最后处理过的位置。以后继续处理的时候可以从这个偏移开始
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        public static ReplicationResult DoReplication(
            LibraryChannel channel,
            string strStartDate,
            string strEndDate,
            LogType logType,
            CancellationToken token)
        {
            return BioUtil.DoReplication(channel,
                strStartDate,
                strEndDate,
                logType,
                token,
                Prompt,
                AddItems);
        }
#endif

#if NO
        public class ReplicationResult : NormalResult
        {
            public string LastDate { get; set; }
            public long LastIndex { get; set; }
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
        public static ReplicationResult DoReplication(
            LibraryChannel channel,
            string strStartDate,
            string strEndDate,
            LogType logType,
            CancellationToken token)
        {
            string strLastDate = "";
            long last_index = -1;    // -1 表示尚未处理

            // bool bUserChanged = false;

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
                List<string> dates = null;
                int nRet = OperLogLoader.MakeLogFileNames(strStartDate,
                    strEndDate,
                    true,  // 是否包含扩展名 ".log"
                    out dates,
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

                channel.Timeout = new TimeSpan(0, 1, 0);   // 一分钟


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
                        Level = 2,  // Program.MainForm.OperLogLevel;
                        AutoCache = false,
                        CacheDir = "",
                        LogType = logType,
                        Filter = "setReaderInfo"
                    };

                    loader.Prompt += Loader_Prompt;
                    try
                    {
                        // int nRecCount = 0;

                        string strLastItemDate = "";
                        long lLastItemIndex = -1;
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
#if NO
                            DialogResult result = System.Windows.Forms.DialogResult.No;
                            strError = logType.ToString() + "日志记录 " + item.Date + " " + item.Index.ToString() + " XML 装入 DOM 的时候发生错误: " + ex.Message;
                            string strText = strError;

                            this.Invoke((Action)(() =>
                            {
                                result = MessageBox.Show(this,
    strText + "\r\n\r\n是否跳过此条记录继续处理?",
    "ReportForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            }));

                            if (result == System.Windows.Forms.DialogResult.No)
                                return -1;

                            // 记入日志，继续处理
                            this.GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
#endif

                                if (Prompt != null)
                                {
                                    strError = logType.ToString() + "日志记录 " + item.Date + " " + item.Index.ToString() + " XML 装入 DOM 的时候发生错误: " + ex.Message;
                                    MessagePromptEventArgs e = new MessagePromptEventArgs
                                    {
                                        MessageText = strError + "\r\n\r\n是否跳过此条继续处理?\r\n\r\n(确定: 跳过;  取消: 停止全部操作)",
                                        IncludeOperText = true,
                                        // + "\r\n\r\n是否跳过此条继续处理?",
                                        Actions = "yes,cancel"
                                    };
                                    Prompt(channel, e);
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
                                    out strError);
                            }
                            else
                                continue;

                            if (nRet == -1)
                            {
                                strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;

                                if (Prompt != null)
                                {
                                    MessagePromptEventArgs e = new MessagePromptEventArgs
                                    {
                                        MessageText = strError + "\r\n\r\n是否跳过此条继续处理?\r\n\r\n(确定: 跳过;  取消: 停止全部操作)",
                                        IncludeOperText = true,
                                        // + "\r\n\r\n是否跳过此条继续处理?",
                                        Actions = "yes,cancel"
                                    };
                                    Prompt(channel, e);
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
                    }
                }

                return new ReplicationResult
                {
                    Value = last_index == -1 ? 0 : 1,
                    LastDate = strLastDate,
                    LastIndex = last_index
                };
            }
            catch (Exception ex)
            {
                string strError = "ReportForm DoReplication() exception: " + ExceptionUtil.GetDebugText(ex);
                return new ReplicationResult { Value = -1, ErrorInfo = strError };
            }
        }

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
        static int TraceSetReaderInfo(
XmlDocument domLog,
out string strError)
        {
            strError = "";

            string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

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
                    if (string.IsNullOrEmpty(strOldRecPath) == true)
                    {
                        strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                        return -1;
                    }

                    // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                    if (string.IsNullOrEmpty(strRecord) == true)
                        strRecord = strOldRecord;
                }

                // 删除旧记录对应的指纹缓存
                if (strAction == "move"
                    && string.IsNullOrEmpty(strOldRecord) == false)
                {
                    if (DeleteFingerPrint(strOldRecord, out strError) == -1)
                        return -1;
                }

                if (AddFingerPrint(strRecord, out strError) == -1)
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

                if (string.IsNullOrEmpty(strOldRecord) == false)
                {
                    if (DeleteFingerPrint(strOldRecord, out strError) == -1)
                        return -1;
                }
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

        // 写入新记录的指纹缓存
        static int AddFingerPrint(string strRecord, out string strError)
        {
            strError = "";

            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(strRecord);

            string strReaderBarcode = GetReaderBarcode(new_dom);
            if (string.IsNullOrEmpty(strReaderBarcode))
                return 0;
            string strFingerPrintString = DomUtil.GetElementText(new_dom.DocumentElement, "fingerprint");

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
                out strError);
            if (nRet != 0)
                return -1;

            return 1;
        }

        static int DeleteFingerPrint(string strOldRecord, out string strError)
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
                    out strError);
                if (nRet != 0)
                    return -1;
            }

            return 0;
        }

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

#endif
    }
}
