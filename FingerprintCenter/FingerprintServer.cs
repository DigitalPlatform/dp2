using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using DigitalPlatform;
using DigitalPlatform.Interfaces;
using DigitalPlatform.CirculationClient;

namespace FingerprintCenter
{
    public class FingerprintServer : MarshalByRefObject, IFingerprint, IDisposable
    {
        public event MessageArrivedEvent MessageArrived;

        #region remoting server

#if HTTP_CHANNEL
        HttpChannel m_serverChannel = null;
#else
        static IpcServerChannel _serverChannel = null;
#endif
        // private ObjRef internalRef;

        public static bool StartRemotingServer()
        {
#if NO
            try
            {
#endif
            // EndRemoteChannel();

#if NO
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            Hashtable ht = new Hashtable();
            ht["portName"] = "FingerprintChannel";
            ht["name"] = "ipc";
            ht["authorizedGroup"] = "Administrators"; // "Everyone";
#endif

            //Instantiate our server channel.
#if HTTP_CHANNEL
            m_serverChannel = new HttpChannel();
#else
            // TODO: 重复启动 .exe 这里会抛出异常，要进行警告处理
            _serverChannel = new IpcServerChannel(
                 "FingerprintChannel");
            // _serverChannel = new IpcServerChannel(ht, provider);

#endif

            //Register the server channel.
            ChannelServices.RegisterChannel(_serverChannel, false);

            RemotingConfiguration.ApplicationName = "FingerprintServer";

            /*
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerFactory),
                "ServerFactory",
                WellKnownObjectMode.Singleton);
             * */


            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(FingerprintServer),
                "FingerprintServer",
                WellKnownObjectMode.Singleton);
#if NO
            internalRef = RemotingServices.Marshal(this,
                "FingerprintServer");
#endif
            Program.FingerPrint.Captured += FingerPrint_Captured;
            // Program.MainForm.OutputHistory("111");

            return true;
#if NO
            }
            catch (RemotingException ex)
            {
                this.ShowMessage(ex.Message);
                return false;
            }
#endif
        }

        private static void FingerPrint_Captured(object sender, DigitalPlatform.CirculationClient.CapturedEventArgs e)
        {
            lock (_syncRoot_messages)
            {
                // Program.MainForm.OutputHistory("captured");

                _messages.Add(e.Text);
                while (_messages.Count > 1000)
                    _messages.RemoveAt(0);
            }
        }

        public static void EndRemotingServer()
        {
            if (_serverChannel != null)
            {
                // RemotingServices.Unmarshal(internalRef);
                // Program.MainForm.OutputHistory("222");
                Program.FingerPrint.Captured -= FingerPrint_Captured;

                ChannelServices.UnregisterChannel(_serverChannel);
                _serverChannel = null;
            }
        }

        #endregion


        #region SendKey 有关功能

        // static private AtomicBoolean _sendKeyEnabled = new AtomicBoolean(false);

        public NormalResult EnableSendKey(bool enable)
        {
            return _enableSendKey(enable);
        }

        public static NormalResult _enableSendKey(bool enable)
        {
            bool old_enable = false;
            if (Program.MainForm != null)
            {
                old_enable = Program.MainForm.SendKeyEnabled;
                if (old_enable != enable)
                    Program.MainForm.SendKeyEnabled = enable;
                else
                    return new NormalResult();  // 优化，如果值没有变化则不显示操作历史
            }

            string message = "";
            if (enable)
                message = "指纹发送打开";
            else
                message = "指纹发送关闭";

            Task.Run(() =>
            {
                Program.MainForm?.OutputHistory(message, 0);
                Program.MainForm?.Speak(message);
            });

            return new NormalResult();
        }

        #endregion

        private void SafeInvokeMessageArrived(string Message)
        {
            //if (!serverActive)
            //    return;

            if (MessageArrived == null)
                return;         //No Listeners

            MessageArrivedEvent listener = null;
            Delegate[] dels = MessageArrived.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (MessageArrivedEvent)del;
                    listener.Invoke(Message);
                }
                catch (Exception ex)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    MessageArrived -= listener;
                }
            }
        }

        private static readonly Object _syncRoot_messages = new Object(); // 2017/5/18
        static List<string> _messages = new List<string>();

        // 取走一条消息
        public GetMessageResult GetMessage(string style)
        {
            lock (_syncRoot_messages)
            {
                // Program.MainForm.OutputHistory($"messages.Count={_messages.Count}");
                if (ClientInfo.ErrorState != "normal")
                {
                    return new GetMessageResult
                    {
                        Value = -1,
                        ErrorInfo = $"{ClientInfo.ErrorStateInfo}",
                        ErrorCode = $"state:{ClientInfo.ErrorState}"
                    };
                }
                if (_messages.Count == 0)
                    return new GetMessageResult { Message = null };
                if (style == "clear")
                {
                    _messages.Clear();
                    return new GetMessageResult { Message = "" };
                }
                string message = _messages[0];
                _messages.RemoveAt(0);
                // Program.MainForm?.Speak($"拿走 {message}");
                return new GetMessageResult { Message = message };
            }
        }

        public int GetVersion(out string strVersion,
            out string strCfgInfo,
            out string strError)
        {
            // strVersion = "2.0";
            strVersion = ClientInfo.ClientVersion;
            strCfgInfo = "selfInitCache";
            strError = "";
            return 0;
        }

        // return:
        //      -1  出错
        //      0   调用前端口已经打开
        //      1   成功
        public int Open(out string strError)
        {
            strError = "";

#if NO
            eventClose.Reset();

            if (this.m_host != null)
            {
                strError = "FingerprintServer 已经打开";
                return 0;
            }

            /*
System.PlatformNotSupportedException: 系统上未安装语音，或没有当前安全设置可用的语音。

Server stack trace: 
   在 System.Speech.Internal.Synthesis.VoiceSynthesis..ctor(WeakReference speechSynthesizer)
   在 System.Speech.Synthesis.SpeechSynthesizer.get_VoiceSynthesizer()
   在 System.Speech.Synthesis.SpeechSynthesizer.remove_SpeakCompleted(EventHandler`1 value)
   在 ZkFingerprint.FingerprintServer.Open(String& strError)
   在 System.Runtime.Remoting.Messaging.StackBuilderSink._PrivateProcessMessage(IntPtr md, Object[] args, Object server, Int32 methodPtr, Boolean fExecuteInContext, Object[]& outArgs)
   在 System.Runtime.Remoting.Messaging.StackBuilderSink.SyncProcessMessage(IMessage msg, Int32 methodPtr, Boolean fExecuteInContext)

Exception rethrown at [0]: 
   在 System.Runtime.Remoting.Proxies.RealProxy.HandleReturnMessage(IMessage reqMsg, IMessage retMsg)
   在 System.Runtime.Remoting.Proxies.RealProxy.PrivateInvoke(MessageData& msgData, Int32 type)
   在 DigitalPlatform.Interfaces.IFingerprint.Open(String& strError)
   在 ZkFingerprint.MainForm.OpenServer(Boolean bDisplayErrorMessage)
   在 ZkFingerprint.MainForm.MainForm_Load(Object sender, EventArgs e)
   在 System.Windows.Forms.Form.OnLoad(EventArgs e)
   在 System.Windows.Forms.Form.OnCreateControl()
   在 System.Windows.Forms.Control.CreateControl(Boolean fIgnoreVisible)
   在 System.Windows.Forms.Control.CreateControl()
   在 System.Windows.Forms.Control.WmShowWindow(Message& m)
   在 System.Windows.Forms.Control.WndProc(Message& m)
   在 System.Windows.Forms.ScrollableControl.WndProc(Message& m)
   在 System.Windows.Forms.Form.WmShowWindow(Message& m)
   在 System.Windows.Forms.Form.WndProc(Message& m)
   在 ZkFingerprint.MainForm.WndProc(Message& m)
   在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
   在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
   在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

             * */
            try
            {
                this.m_speech = new SpeechSynthesizer();
                this.m_speech.SpeakCompleted -= new EventHandler<SpeakCompletedEventArgs>(m_speech_SpeakCompleted);
                this.m_speech.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(m_speech_SpeakCompleted);
            }
            catch (System.PlatformNotSupportedException ex)
            {
                strError = ex.Message;
                return -1;
            }

            try
            {
                m_host = new ZKFPEngX();
            }
            catch (Exception ex)
            {
                strError = "open error : " + ex.Message;
                return -1;
            }

            //  0 初始化成功
            //  1 指纹识别驱动程序加载失败
            //  2 没有连接指纹识别仪
            //  3 属性 SensorIndex 指定的指纹仪不存在
            int nRet = this.m_host.InitEngine();
            if (nRet != 0)
            {
                if (nRet == 1)
                    strError = "指纹识别驱动程序加载失败";
                else if (nRet == 2)
                    strError = "尚未连接指纹阅读器";
                else if (nRet == 3)
                    strError = "属性 SensorIndex (" + this.m_host.SensorIndex.ToString() + ") 指定的指纹仪不存在";
                else
                    strError = "初始化失败，错误码 " + nRet.ToString();

                Speak(strError);
                this.m_host = null;
                return -1;
            }

            this.m_host.FPEngineVersion = "10";

            this.m_host.OnFeatureInfo -= new IZKFPEngXEvents_OnFeatureInfoEventHandler(m_host_OnFeatureInfo);
            this.m_host.OnFeatureInfo += new IZKFPEngXEvents_OnFeatureInfoEventHandler(m_host_OnFeatureInfo);

            this.m_host.OnImageReceived -= new IZKFPEngXEvents_OnImageReceivedEventHandler(m_host_OnImageReceived);
            this.m_host.OnImageReceived += new IZKFPEngXEvents_OnImageReceivedEventHandler(m_host_OnImageReceived);

            this.m_host.OnCapture -= new IZKFPEngXEvents_OnCaptureEventHandler(m_host_OnCapture);
            this.m_host.OnCapture += new IZKFPEngXEvents_OnCaptureEventHandler(m_host_OnCapture);
            this.m_host.BeginCapture();

            Speak("指纹阅读器接口程序成功启动");
#endif
            return 1;
        }

        // 设置参数
        public bool SetParameter(string strName, object value)
        {
#if NO
            if (strName == "Threshold")
            {
                // 指纹识别系统比对识别阈值
                // 1-100 默认 10
                this.m_host.Threshold = (int)value;
                return true;
            }
            if (strName == "OneToOneThreshold")
            {
                // 低速指纹 1:1 比对的识别阈值分数
                // 1-100 默认 10
                this.m_host.Threshold = (int)value;
                return true;
            }
#endif
            return false;
        }

        public int Close()
        {
            return 1;
        }

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        // return:
        //      0   成功
        //      其他  失败。错误码
        public int AddItems(List<FingerprintItem> items,
            out string strError)
        {
            return Program.FingerPrint.AddItems(items,
                null,
                out strError);
        }

        public int CancelGetFingerprintString()
        {
            Program.FingerPrint.CancelRegisterString();
            return 0;
        }

        // 1.0 版函数
        // TODO: 防止函数过程重入
        // 获得一个指纹特征字符串
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        public int GetFingerprintString(out string strFingerprintString,
            out string strVersion,
            out string strError)
        {
#if NO
            strError = "";
            strFingerprintString = "";
            strVersion = "";

            try
            {
                TextResult result = FingerPrint.GetRegisterString("");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                strFingerprintString = result.Text;
                strVersion = "zk-10";
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return 0;
            }
#endif
            return GetFingerprintString("",
                out strFingerprintString,
                out strVersion,
                out strError);
        }

        // 2.0 增加的函数
        // TODO: 防止函数过程重入
        // 获得一个指纹特征字符串
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        public int GetFingerprintString(
            string strExcludeBarcodes,
            out string strFingerprintString,
            out string strVersion,
            out string strError)
        {
            strError = "";
            strFingerprintString = "";
            strVersion = "";

            Program.MainForm?.ActivateWindow(true);
            Program.MainForm?.DisplayCancelButton(true);
            try
            {
                TextResult result = Program.FingerPrint.GetRegisterString(null, strExcludeBarcodes);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                strFingerprintString = result.Text;
                strVersion = "zk-10";
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return 0;
            }
            finally
            {
                Program.MainForm?.ActivateWindow(false);
                Program.MainForm?.DisplayCancelButton(false);
            }
        }

        // 2.0 拟增加的函数
        int ServerFingerPrintChanged(
            string strStyle,
            out string strError)
        {
            strError = "";

            return 0;
        }

        // 验证读者指纹. 1:1比对
        // parameters:
        //      item    读者信息。ReaderBarcode成员提供了读者证条码号，FingerprintString提供了指纹特征码
        //              如果 FingerprintString 不为空，则用它和当前采集的指纹进行比对；
        //              否则用 ReaderBarcode，对高速缓存中的指纹进行比对
        // return:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        public int VerifyFingerprint(FingerprintItem item,
            out string strError)
        {
            strError = "";

            return 0;

#if NO
            // 等到扫描一次指纹
            // 这次的扫描不要进行自动比对，也不要键盘仿真

            string strTemplate = item.FingerprintString;

            bool bRet = this.m_host.VerFingerFromStr(ref strTemplate,
                 strThisString,
                 false,
                 ref bChanged);
#endif
        }

        public NormalResult GetState(string style)
        {
            if (style == "restart")
            {
                Program.MainForm.Restart();
                return new NormalResult();
            }

            // 获得当前 fingerprintcenter 所连接的 dp2library 服务器的 UID
            if (style == "getLibraryServerUID")
                return new NormalResult
                {
                    Value = 0,
                    ErrorCode = Program.MainForm.ServerUID,
                };

            if (ClientInfo.ErrorState == "normal")
                return new NormalResult
                {
                    Value = 0,
                    ErrorCode = ClientInfo.ErrorState,
                    ErrorInfo = ClientInfo.ErrorStateInfo
                };
            return new NormalResult
            {
                Value = -1,
                ErrorCode = ClientInfo.ErrorState,
                ErrorInfo = ClientInfo.ErrorStateInfo
            };
        }

        public NormalResult ActivateWindow()
        {
            Program.MainForm.ActivateWindow();
            return new NormalResult();
        }

        public void Dispose()
        {
            Program.FingerPrint?.CancelRegisterString();
        }
    }
}
