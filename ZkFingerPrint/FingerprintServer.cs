using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Speech.Synthesis;

using DigitalPlatform.Interfaces;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

using ZKFPEngXControl;
using DigitalPlatform;

// ZK4500

namespace ZkFingerprint
{
    public class FingerprintServer : MarshalByRefObject, IFingerprint, IDisposable
    {
        public event MessageArrivedEvent MessageArrived;
        public GetMessageResult GetMessage(string style)
        {
            return new GetMessageResult { Message = null };
        }

        public void Awake()
        {

        }

        TimeSpan m_interval = new TimeSpan(0, 0, 0, 1, 0);
        DateTime m_lastFinish = new DateTime(0);

        ZKFPEngX m_host = null;

        SpeechSynthesizer m_speech = null;

        int m_handle = -1;
        int m_idSeed = 10;

        Hashtable id_barcode_table = new Hashtable();  // id --> barcode
        Hashtable barcode_id_table = new Hashtable();  // barcode --> id 

        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventFinished = new AutoResetEvent(false);  // true : initial state is signaled 

        public int GetVersion(out string strVersion,
            out string strCfgInfo,
            out string strError)
        {
            strVersion = "1.0";
            strCfgInfo = "";
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

            return 1;
        }

        // 设置参数
        public bool SetParameter(string strName, object value)
        {
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
            return false;
        }

        public int Close()
        {
            eventClose.Set();

            if (this.m_host == null)
                return 0;

            this.m_host.EndEngine();
            if (this.m_handle != -1)
            {
                this.m_host.FreeFPCacheDB(this.m_handle);
                this.m_handle = -1;
            }

            this.m_host = null;
            return 1;
        }

        string m_strSpeakContent = ""; // 正在说的内容

        void m_speech_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            this.m_strSpeakContent = "";
        }

        void m_host_OnImageReceived(ref bool AImageValid)
        {
            if (this.DisplayFingerprintImage == false)
                return;

            int nWidth = 0;
            int nHeight = 0;
            IntPtr dc = GetImagePanelInfo(out nWidth, out nHeight);
            try
            {
                if (nWidth > 0)
                {
                    nHeight = (int)(((double)this.m_host.ImageHeight / (double)this.m_host.ImageWidth) * (double)nWidth);
                    this.m_host.PrintImageAt(dc.ToInt32(), 0, 0, nWidth, nHeight);
                }
            }
            finally
            {
                ReleaseImagePanelInfo(dc);
            }
        }

        void m_host_OnCapture(bool ActionResult, object ATemplate)
        {
            if (this.GameState == true)
                return;

            if (this.m_bInRegister == true)
                return;

            if (m_nInIdentity > 0)
                return; // 防止叠加和重入.因为 Identity 过程一般耗时较长

            if (DateTime.Now - m_lastFinish < m_interval)
            {
                // 两次操作间隔太密集了，忽略后面的
                Speak("请注意，按下手指后，请一直保持，等听到响声后再放开手指。谢谢");
                m_lastFinish = DateTime.Now;
                return;
            }

            if (ActionResult == false)
                goto ERROR1;

            if (this.m_handle == -1)
            {
                this.ActivateMainForm(true);
                this.DisplayInfo("指纹缓存尚未初始化。请先在 dp2circulation 中初始化指纹缓存...");
                if (this.SpeakOn == false)
                    SafeBeep(7);
                Speak("指纹缓存尚未初始化。请先在 dp-2 内务 中初始化 指纹缓存");
                return;
            }

            int nRet = this.DoIdentity();
            if (nRet == -1)
            {
                this.DisplayInfo("没有找到匹配的读者 ...");
                goto ERROR1;
            }

            string strBarcode = (string)id_barcode_table[nRet.ToString()];
            SafeBeep(1);
            SendKeys.SendWait(strBarcode + "\r");
            if (this.BeepOn == false)
                Speak("很好");

            // 闪绿灯
            SafeLight("green");
            m_lastFinish = DateTime.Now;
            return;

            ERROR1:
            SafeBeep(3);
            if (this.BeepOn == false)
                Speak("抱歉，无法识别您的指纹");
            // 失败，闪红灯
            SafeLight("red");
            m_lastFinish = DateTime.Now;
        }

        void Speak(string strText)
        {
            if (this.m_speech == null)
                return;

            if (this.SpeakOn == false)
                return;

            if (strText == this.m_strSpeakContent)
                return; // 正在说同样的句子，不必打断

            this.m_strSpeakContent = strText;
            this.m_speech.SpeakAsyncCancelAll();
            this.m_speech.SpeakAsync(strText);
        }

        void CancelSpeak()
        {
            if (this.m_speech == null)
                return;

            if (this.SpeakOn == false)
                return;

            this.m_strSpeakContent = "";
            this.m_speech.SpeakAsyncCancelAll();
        }

        public void Dispose()
        {
            // this.Close();
            if (m_speech != null)
                m_speech.Dispose();
            eventClose.Dispose();
            eventFinished.Dispose();
        }

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        public int AddItems(List<FingerprintItem> items,
            out string strError)
        {
            strError = "";

            if (this.m_host == null)
            {
                if (Open(out strError) == -1)
                    return -1;
            }

            // 清除已有的全部缓存内容
            if (items == null || items.Count == 0)
            {
                if (this.m_handle != -1)
                {
                    this.m_host.FreeFPCacheDB(this.m_handle);
                    this.m_handle = -1;
                }
                return 0;
            }

            if (this.m_handle == -1)
            {
                this.m_handle = this.m_host.CreateFPCacheDB();
                this.id_barcode_table.Clear();
                this.barcode_id_table.Clear();
            }

            foreach (FingerprintItem item in items)
            {
                int id = m_idSeed++;

                // 看看条码号以前是否已经存在?
                if (barcode_id_table.Contains(item.ReaderBarcode) == true)
                {
                    int nOldID = (int)barcode_id_table[item.ReaderBarcode];
                    this.m_host.RemoveRegTemplateFromFPCacheDB(this.m_handle, nOldID);

                    id_barcode_table.Remove(nOldID.ToString());
                    if (string.IsNullOrEmpty(item.FingerprintString) == true)
                        barcode_id_table.Remove(item.ReaderBarcode);
                }

                if (string.IsNullOrEmpty(item.FingerprintString) == false)
                {
                    id_barcode_table[id.ToString()] = item.ReaderBarcode;
                    barcode_id_table[item.ReaderBarcode] = id;

                    try
                    {
                        this.m_host.AddRegTemplateStrToFPCacheDB(this.m_handle, id, item.FingerprintString);
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

#if NO
        public int ReadInput(out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            if (this.m_host == null)
            {
                if (Open(out strError) == -1)
                    return -1;
            }

            if (this.m_handle == -1)
            {
                strError = "尚未创建高速缓存";
                return -1;
            }

            if (this.m_host.IsRegister == true)
                this.m_host.CancelEnroll();

            this.m_host.OnCapture -= new IZKFPEngXEvents_OnCaptureEventHandler(m_host_OnCapture);
            this.m_host.OnCapture += new IZKFPEngXEvents_OnCaptureEventHandler(m_host_OnCapture);

            m_nValue = -1;
            m_template = null;
            m_bActionResult = false;
            eventFinished.Reset();
            this.m_host.BeginCapture();

            string strText = "请扫描指纹...";
            DisplayInfo(strText);

            WaitHandle[] events = new WaitHandle[2];

            events[0] = eventClose;
            events[1] = eventFinished;

            int index = WaitHandle.WaitAny(events, -1, false);

            if (index == WaitHandle.WaitTimeout)
            {
                strError = "超时";
                DisplayInfo(strError);
                return -1;
            }
            else if (index == 0)
            {
                strError = "接口被关闭";
                DisplayInfo(strError);
                return -1;
            }

            // 正常结束
            if (m_bActionResult == false)
            {
                strError = "获取指纹信息失败";
                DisplayInfo("非常抱歉，本轮获取指纹信息操作失败");
                return -1;
            }

            string strTemplate = this.m_host.GetTemplateAsString();
            int nScore = 0;
            int nCount = 0;
            int nRet = this.m_host.IdentificationFromStrInFPCacheDB(this.m_handle,
    strTemplate,
    ref nScore,
    ref nCount);
            strBarcode = nRet.ToString();
            /*
            int nRet = this.m_host.IdentificationInFPCacheDB(this.m_handle, 
                this.m_template,
                ref nScore,
                ref nCount);
            strBarcode = nRet.ToString();
             * */
            // strBarcode = this.m_nValue.ToString();

            Beep();

            return 0;
        }

        byte[] m_template = null;
        int m_nValue = -1;

        void m_host_OnCapture(bool ActionResult, object ATemplate)
        {
            m_bActionResult = ActionResult;
            // m_strTemplate = this.m_host.GetTemplateAsStringEx("10");
            this.m_template = (byte [])ATemplate;

            // this.m_nValue = this.Identity();

            eventFinished.Set();
        }
#endif

        public int CancelGetFingerprintString()
        {
            if (this.m_host == null)
                return -1;
            this.m_host.CancelEnroll();
            m_bCanceled = true;
            eventFinished.Set();
            return 0;
        }

        bool m_bInRegister = false;
        bool m_bCanceled = false;

        public int GetFingerprintString(
            string strExcludeBarcodes,
            out string strFingerprintString,
            out string strVersion,
            out string strError)
        {
            strFingerprintString = "[not support]";
            strVersion = "";
            strError = "1.0 暂不支持此函数";
            return -1;
        }

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
            strError = "";
            strFingerprintString = "";
            strVersion = "";

            if (this.m_host == null)
            {
                if (Open(out strError) == -1)
                    return -1;
            }

            // this.m_host.CancelCapture();

            m_bInRegister = true;
            ActivateMainForm(true);
            DisplayCancelButton(true);
            try
            {
                strVersion = "zk-" + this.m_host.FPEngineVersion;

                this.m_host.EnrollCount = 1;

                this.m_host.OnEnroll -= new IZKFPEngXEvents_OnEnrollEventHandler(m_host_OnEnroll);
                this.m_host.OnEnroll += new IZKFPEngXEvents_OnEnrollEventHandler(m_host_OnEnroll);

                eventFinished.Reset();
                m_bActionResult = false;
                m_bCanceled = false;

                this.m_host.BeginEnroll();

                string strText = "请扫描指纹。\r\n\r\n总共需要扫描 " + this.m_host.EnrollCount.ToString() + " 次";
                DisplayInfo(strText);

                Speak("请扫描指纹。一共需要按 " + this.m_host.EnrollCount.ToString() + " 次");

                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventFinished;

                int index = WaitHandle.WaitAny(events, -1, false);

                if (index == WaitHandle.WaitTimeout)
                {
                    strError = "超时";
                    DisplayInfo(strError);
                    return -1;
                }
                else if (index == 0)
                {
                    strError = "接口被关闭";
                    DisplayInfo(strError);
                    return -1;
                }

                // 取消
                if (m_bCanceled == true)
                {
                    strError = "获取指纹信息的操作被取消";
                    DisplayInfo(strError);
                    Speak(strError);
                    return 0;
                }

                // 正常结束
                if (m_bActionResult == false)
                {
                    strError = "获取指纹信息失败";
                    DisplayInfo("非常抱歉，本轮获取指纹信息操作失败");
                    if (this.SpeakOn == false)
                        SafeBeep(3);
                    Speak("非常抱歉，本轮获取指纹信息操作失败");
                    return -1;
                }

                // strFingerprintString = this.m_host.GetTemplateAsStringEx("10");
                strFingerprintString = this.m_host.GetTemplateAsString();
                if (this.SpeakOn == false)
                    SafeBeep(1);

                DisplayInfo("获取指纹信息成功");
                Speak("指纹扫描完成。谢谢");
                return 1;
            }
            finally
            {
                // this.m_host.BeginCapture();
                DisplayCancelButton(false);
                ActivateMainForm(false);
                m_bInRegister = false;
            }
        }

        void m_host_OnFeatureInfo(int AQuality)
        {
#if NO
            string strText = "";
            if (this.m_host.IsRegister == true)
            {
                strText = "总共需要 " + this.m_host.EnrollCount.ToString() + " 次 ";
                strText += "还需要 " + this.m_host.EnrollIndex.ToString() + " 次";
            }


            strText += " Fingerprint quality";
            if (AQuality != 0)
                strText += " not good, quality=" + this.m_host.LastQuality;
            else
                strText += " good, quality=" + this.m_host.LastQuality;

            /*
            if (this.DisplayMessage != null)
            {
                MessageEventArgs e = new MessageEventArgs();
                e.Message = strText;
                this.DisplayMessage(this, e);
            }
             * */
#endif
            DisplayFeatureInfo(AQuality);
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

        public void Beep(int nCount)
        {
            for (int i = 0; i < nCount; i++)
            {
                this.m_host.ControlSensor(13, 1);
            }
            this.m_host.ControlSensor(13, 0);
        }

        public void Light(string strColor)
        {
            if (strColor == "red")
            {
                this.m_host.ControlSensor(12, 1);
                this.m_host.ControlSensor(12, 0);
            }
            else
            {
                this.m_host.ControlSensor(11, 1);
                this.m_host.ControlSensor(11, 0);
            }
        }

        List<int> m_gameScores = new List<int>();

        public string GetFeatureInfo(int AQuality,
            bool bGameState = false)
        {
            string strText = "";

            if (AQuality != 0)
                strText += "抱歉，本次扫描质量不高 (" + this.m_host.LastQuality + ")";
            else
                strText += "恭喜您！本次扫描质量很好 (" + this.m_host.LastQuality + ")";

            if (this.m_host.IsRegister == true
                && (this.m_host.EnrollIndex - 1) > 0)
            {
                // strText = "总共需要 " + this.m_host.EnrollCount.ToString() + " 次 ";
                strText += "\r\n\r\n请继续扫描，后面还需要扫描 " + (this.m_host.EnrollIndex - 1).ToString() + " 次";

                string strQuality = "";
                if (AQuality != 0)
                    strQuality = "抱歉，这次扫描质量不高";
                else
                    strQuality = "很好";

                Speak(strQuality + "。请再按一次");
            }

            if (bGameState == true)
            {
                string strSpeak = GetScoreString(this.m_host.LastQuality);
                Speak(strSpeak);
                return strSpeak;
            }

            return strText;
        }

        int CountContinue(int v)
        {
            int nCount = 0;
            for (int i = this.m_gameScores.Count - 1; i >= 0; i--)
            {
                if (this.m_gameScores[i] != v)
                {
                    return nCount;
                }
                nCount++;
            }

            return nCount;
        }

        string GetScoreString(int v)
        {
            string strResult = "";
            if (v >= 100)
            {
                int nContine = CountContinue(100);
                if (nContine >= 1)
                {
                    strResult = "连续 " + (nContine + 1).ToString() + " 次 100 分！";
                    goto END1;
                }
                else
                    strResult = "极端完美！";
            }
            else if (v >= 90)
                strResult = "帅呆了！";
            else if (v >= 80)
                strResult = "非常好！";
            else if (v >= 70)
                strResult = "很好！";
            else if (v >= 60)
                strResult = "还行！";
            else if (v >= 50)
                strResult = "加油啊！";
            else
                strResult = "不好意思！";

            strResult += v.ToString() + " 分";

            END1:
            m_gameScores.Add(v);
            while (this.m_gameScores.Count > 100)
            {
                this.m_gameScores.RemoveAt(0);
            }
            return strResult;
        }

        bool m_bActionResult = false;
        // string m_strTemplate = "";

        void m_host_OnEnroll(bool ActionResult, object ATemplate)
        {
            m_bActionResult = ActionResult;
            // m_strTemplate = this.m_host.GetTemplateAsStringEx("10");

            eventFinished.Set();
        }

        void ActivateMainForm(bool bActive)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.ActivateWindow(bActive);
        }

        void DisplayCancelButton(bool bVisible)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.DisplayCancelButton(bVisible);
        }

        void SafeBeep(int nCount)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.Beep(this, nCount);
        }

        void SafeLight(string strColor)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.Light(this, strColor);
        }

        void DisplayFeatureInfo(int nQuality)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.DisplayFeatureInfo(this, nQuality);
        }

        void DisplayInfo(string strText)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.DisplayInfo(strText);
        }

        int m_nInIdentity = 0;

        // 进行识别
        int DoIdentity()
        {
            m_nInIdentity++;
            try
            {
                // Thread.Sleep(2000);  // 为了模拟匹配算法较为缓慢的情况
                MainForm form = (MainForm)Application.OpenForms[0];
                return form.Identity(this);
            }
            finally
            {
                m_nInIdentity--;
            }
        }

        public int Match()
        {
            // string strTemplate = this.m_host.GetTemplateAsStringEx("10");
            string strTemplate = this.m_host.GetTemplateAsString();
            int nScore = 8;
            int nCount = 0;
            int nRet = this.m_host.IdentificationFromStrInFPCacheDB(this.m_handle,
                strTemplate,
                ref nScore,
                ref nCount);

            return nRet;
        }

        bool BeepOn
        {
            get
            {
                MainForm form = (MainForm)Application.OpenForms[0];
                return form.BeepOn;
            }
        }

        bool SpeakOn
        {
            get
            {
                MainForm form = (MainForm)Application.OpenForms[0];
                return form.SpeakOn;
            }
        }

        bool DisplayFingerprintImage
        {
            get
            {
                MainForm form = (MainForm)Application.OpenForms[0];
                return form.DisplayFingerprintImage;
            }
        }

        bool GameState
        {
            get
            {
                MainForm form = (MainForm)Application.OpenForms[0];
                return form.GameState;
            }
        }

        IntPtr GetImagePanelInfo(out int nWidth, out int nHeight)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            return form.GetImagePanelInfo(out nWidth, out nHeight);
        }

        void ReleaseImagePanelInfo(IntPtr hDC)
        {
            MainForm form = (MainForm)Application.OpenForms[0];
            form.ReleaseImagePanelInfo(hDC);
        }

        #region SendKey 有关功能

        static private AtomicBoolean _sendKeyEnabled = new AtomicBoolean(false);

        public NormalResult EnableSendKey(bool enable)
        {
            if (enable == true)
                _sendKeyEnabled.FalseToTrue();
            else
                _sendKeyEnabled.TrueToFalse();

            /*
            string message = "";
            if (enable)
                message = "SendKey 打开";
            else
                message = "SendKey 关闭";
            Program.MainForm.OutputHistory(message, 0);
            Program.MainForm?.Speak(message);
            */

            return new NormalResult();
        }

        #endregion
    }
}
