#define USE_LOCAL_CHANNEL
#define USE_THREAD   // 要使用独立的线程。似乎这样复杂化了简单的问题，没有必要

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Net;   // for WebClient class
using System.IO;
using System.Web;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 操作历史
    /// </summary>
    public class OperHistory : ThreadBase
    {
        int m_inOnTimer = 0;
#if USE_THREAD
        List<OneCall> m_calls = new List<OneCall>();

        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

#endif

        #region Thread

#if NO
#if USE_THREAD
        bool m_bStopThread = true;
        internal Thread _thread = null;

        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号
        internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        public int PerTime = 1000;   // 1 秒 5 * 60 * 1000;	// 5 分钟
#endif
#endif

        #endregion

        bool m_bNeedReload = false; // 是否需要重新装载project xml

        /// <summary>
        /// IE 浏览器控件，用于显示操作历史信息
        /// </summary>
        public WebBrowser WebBrowser = null;

        WebExternalHost m_webExternalHost = new WebExternalHost();

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

#if USE_LOCAL_CHANNEL
        // 2011/12/5
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();
#endif

        int m_nCount = 0;

        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = new ScriptManager();

        Assembly PrintAssembly = null;   // 打印代码的Assembly

        /// <summary>
        /// 脚本代码中 PrintHost 派生类对象实例
        /// </summary>
        public PrintHost PrintHostObj = null;   // 

        int m_nAssenblyVersion = 0;

        int AssemblyVersion
        {
            get
            {
                return this.m_nAssenblyVersion;
            }
            set
            {
                this.m_nAssenblyVersion = value;
            }
        }

        /// <summary>
        /// 获取配置参数：当前正在使用的出纳打印方案名
        /// </summary>
        public string CurrentProjectName
        {
            get
            {
                return this.MainForm.AppInfo.GetString(
                "charging_print",
                "projectName",
                "");
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {

        }

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            // string strCssUrl = this.MainForm.LibraryServerDir + "/history.css";
            string strCssUrl = PathUtil.MergePath(this.MainForm.DataDir, "/history.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            /*
            // 2009/2/11
            if (String.IsNullOrEmpty(this.MainForm.LibraryServerDir) == false)
                strJs = "<SCRIPT language='javaSCRIPT' src='" + this.MainForm.LibraryServerDir + "/getsummary.js" + "'></SCRIPT>";
            */
            // strJs = "<SCRIPT language='javaSCRIPT' src='" + PathUtil.MergePath(this.MainForm.DataDir, "getsummary.js") + "'></SCRIPT>";

            {
                HtmlDocument doc = WebBrowser.Document;

                if (doc == null)
                {
                    WebBrowser.Navigate("about:blank");
                    doc = WebBrowser.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.WebBrowser,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        /// <summary>
        /// 初始化 OperHistory 对象
        /// 初始化过程中，要编译出纳打印方案脚本代码，使它处于就绪状态
        /// </summary>
        /// <param name="main_form">框架窗口</param>
        /// <param name="webbrowser">用于显示操作历史信息的 IE 浏览器控件</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError中；0: 成功</returns>
        public int Initial(MainForm main_form,
            WebBrowser webbrowser,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            this.MainForm = main_form;

#if USE_LOCAL_CHANNEL
            this.Channel.Url = this.MainForm.LibraryServerUrl;
            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);
#endif


            /*
            string strLibraryServerUrl = this.MainForm.AppInfo.GetString(
"config",
"circulation_server_url",
"");
            int pos = strLibraryServerUrl.LastIndexOf("/");
            if (pos != -1)
                strLibraryServerUrl = strLibraryServerUrl.Substring(0, pos);
             * */


            this.WebBrowser = webbrowser;

            // webbrowser
            this.m_webExternalHost.Initial(this.MainForm, this.WebBrowser);
            this.WebBrowser.ObjectForScripting = this.m_webExternalHost;

            this.ClearHtml();

#if USE_THREAD
            this.BeginThread();
#endif

            /*
            Global.WriteHtml(this.WebBrowser,
    "<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>");
             * */

            /*

            // 准备script代码
            string strCsFileName = this.MainForm.DataDir + "\\charging_print.cs";
            string strRefFileName = this.MainForm.DataDir + "\\charging_print.cs.ref";

            if (File.Exists(strCsFileName) == true)
            {
                Encoding encoding = FileUtil.DetectTextFileEncoding(strCsFileName);

                StreamReader sr = null;

                try
                {
                    // TODO: 这里的自动探索文件编码方式功能不正确，
                    // 需要专门编写一个函数来探测文本文件的编码方式
                    // 目前只能用UTF-8编码方式
                    sr = new StreamReader(strCsFileName, encoding);
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }
                string strCode = sr.ReadToEnd();
                sr.Close();
                sr = null;

                // .ref文件可以缺省
                string strRef = "";
                if (File.Exists(strRefFileName) == true)
                {

                    try
                    {
                        sr = new StreamReader(strRefFileName, true);
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        return -1;
                    }
                    strRef = sr.ReadToEnd();
                    sr.Close();
                    sr = null;

                    // 提前检查
                    string[] saRef = null;
                    nRet = ScriptManager.GetRefsFromXml(strRef,
                        out saRef,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = strRefFileName + " 文件内容(应为XML格式)格式错误: " + strError;
                        return -1;
                    }
                }

                nRet = PrepareScript(strCode,
                   strRef,
                   out strError);
                if (nRet == -1)
                {
                    strError = "C#脚本文件 " + strCsFileName + " 准备过程发生错误(出纳单据打印功能因此暂时失效)：\r\n\r\n" + strError;
                    return -1;
                }
            }
             * */

            ScriptManager.applicationInfo = this.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                this.MainForm.DataDir + "\\charging_print_projects.xml";
            ScriptManager.DataDir = this.MainForm.DataDir;

            ScriptManager.CreateDefaultContent -= new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);
            ScriptManager.CreateDefaultContent += new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);

            try
            {
                ScriptManager.Load();
            }
            catch (FileNotFoundException ex)
            {
                strError = "file not found : " + ex.Message;
                return 0;   // 当作正常处理
            }
            catch (Exception ex)
            {
                strError = "load script manager error: " + ex.Message;
                return -1;
            }

            // 获得方案名
            string strProjectName = CurrentProjectName;

            if (String.IsNullOrEmpty(strProjectName) == false)
            {
                string strProjectLocate = "";
                // 获得方案参数
                // strProjectNamePath	方案名，或者路径
                // return:
                //		-1	error
                //		0	not found project
                //		1	found
                nRet = this.ScriptManager.GetProjectData(
                    strProjectName,
                    out strProjectLocate);
                if (nRet == 0)
                {
                    strError = "凭条打印方案 " + strProjectName + " 没有找到...";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "scriptManager.GetProjectData() error ...";
                    return -1;
                }

                // 
                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 2008/5/9
                this.Initial();
            }

            return 0;
        }

#if USE_LOCAL_CHANNEL
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            MainForm.Channel_BeforeLogin(this, e);
        }
#endif

        /// <summary>
        /// 关闭当前对象。包括关闭通讯通道
        /// </summary>
        public void Close()
        {
#if USE_THREAD
            this.StopThread(false);
#endif

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Destroy();

#if USE_LOCAL_CHANNEL
            if (this.Channel != null)
            {
                this.Channel.Close();
                this.Channel = null;
            }
#endif

        }

        private void scriptManager_CreateDefaultContent(object sender,
            CreateDefaultContentEventArgs e)
        {
            string strPureFileName = Path.GetFileName(e.FileName);

            if (String.Compare(strPureFileName, "main.cs", true) == 0)
            {
                CreateDefaultMainCsFile(e.FileName);
                e.Created = true;
            }
            else
            {
                e.Created = false;
            }

        }

        /// <summary>
        /// 为出纳打印方案创建起始的的 main.cs 文件
        /// </summary>
        /// <param name="strFileName">文件名</param>
        public static void CreateDefaultMainCsFile(string strFileName)
        {
            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Windows.Forms;");
                sw.WriteLine("using System.IO;");
                sw.WriteLine("using System.Text;");
                sw.WriteLine("using System.Xml;");
                sw.WriteLine("");
                sw.WriteLine("using DigitalPlatform.Xml;");
                sw.WriteLine("using DigitalPlatform.IO;");
                sw.WriteLine("");
                sw.WriteLine("using dp2Circulation;");
                sw.WriteLine("");
                sw.WriteLine("public class MyPrint : PrintHost");
                sw.WriteLine("{");
                sw.WriteLine("");
                sw.WriteLine("\tpublic override void OnTestPrint(object sender, PrintEventArgs e)");
                sw.WriteLine("\t{");
                sw.WriteLine("\t}");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("\tpublic override void OnPrint(object sender, PrintEventArgs e)");
                sw.WriteLine("\t{");
                sw.WriteLine("\t}");
                sw.WriteLine("");
                sw.WriteLine("}");
            }
        }

        /// <summary>
        /// 打开出纳打印方案管理窗口
        /// </summary>
        /// <param name="owner">宿主窗口</param>
        public void OnProjectManager(IWin32Window owner)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.MainForm.DefaultFont, false);
            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "OperHistory";
            dlg.scriptManager = this.ScriptManager;
            dlg.AppInfo = this.MainForm.AppInfo;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            this.m_bNeedReload = false;

            dlg.CreateProjectXmlFile -= new AutoCreateProjectXmlFileEventHandle(dlg_CreateProjectXmlFile);
            dlg.CreateProjectXmlFile += new AutoCreateProjectXmlFileEventHandle(dlg_CreateProjectXmlFile);

            dlg.ShowDialog(owner);

            // 如果需要重新装载project xml
            if (this.m_bNeedReload == true)
            {
                string strError = "";
                try
                {
                    ScriptManager.Load();
                }
                catch (Exception ex)
                {
                    strError = "load script manager error: " + ex.Message;
                    MessageBox.Show(owner, strError);
                }
            }
        }

        // 发生了自动创建project xml文件的事件
        void dlg_CreateProjectXmlFile(object sender, AutoCreateProjectXmlFileEventArgs e)
        {
            m_bNeedReload = true;
        }

        /// <summary>
        /// 调用出纳打印脚本中的 OnInitial() 函数，初始化对象状态
        /// </summary>
        public void Initial()
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                EventArgs e = new EventArgs();

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnInitial(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>OnInitial()时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        /// <summary>
        /// 当读者证条码号扫入时被触发
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        public void ReaderBarcodeScaned(string strReaderBarcode)
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                ReaderBarcodeScanedEventArgs e = new ReaderBarcodeScanedEventArgs();
                e.ReaderBarcode = strReaderBarcode;

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnReaderBarcodeScaned(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        // 创建打印内容并打印出来
        /// <summary>
        /// 打印时被触发
        /// </summary>
        public void Print()
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                PrintEventArgs e = new PrintEventArgs();
                e.PrintInfo = this.PrintHostObj.PrintInfo;
                e.Action = "print";

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnPrint(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        // 创建打印内容并打印出来
        /// <summary>
        /// 打印时被触发
        /// </summary>
        /// <param name="info">打印信息</param>
        public void Print(PrintInfo info)
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                PrintEventArgs e = new PrintEventArgs();
                e.PrintInfo = info;
                e.Action = "print";

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnPrint(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        // 创建打印内容
        /// <summary>
        /// 要创建打印内容时被触发
        /// </summary>
        /// <param name="info">打印信息</param>
        /// <param name="strResultString">结果字符串</param>
        /// <param name="strResultFormat">结果字符串的格式</param>
        public void GetPrintContent(PrintInfo info,
            out string strResultString,
            out string strResultFormat)
        {
            strResultString = "";
            strResultFormat = "";

            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                PrintEventArgs e = new PrintEventArgs();
                e.PrintInfo = info;
                e.Action = "create";

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnPrint(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }

                strResultString = e.ResultString;
                strResultFormat = e.ResultFormat;
            }

        }

        /// <summary>
        /// 要清除打印机配置时被触发
        /// </summary>
        public void ClearPrinterPreference()
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                PrintEventArgs e = new PrintEventArgs();
                e.PrintInfo = this.PrintHostObj.PrintInfo;

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnClearPrinterPreference(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        /// <summary>
        /// 要测试打印的时候被触发
        /// </summary>
        public void TestPrint()
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                PrintEventArgs e = new PrintEventArgs();
                e.PrintInfo = this.PrintHostObj.PrintInfo;

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnTestPrint(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        /*
        public void Action(string strActionName)
        {
            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                ActionEventArgs e = new ActionEventArgs();
                e.Operation = strActionName;
                e.OperName = strActionName;

                string strError = "";
                int nRet = this.TriggerScriptAction(e, out strError);
                if (nRet == -1)
                {
                    string strText = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(strError);
                    AppendHtml(strText);
                }
            }
        }*/

        #region Thread

#if USE_THREAD

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            List<OneCall> calls = new List<OneCall>();
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.m_inOnTimer++;
            try
            {
                for (int i = 0; i < this.m_calls.Count; i++)
                {
                    OneCall call = this.m_calls[i];

                    calls.Add(call);
                }

                this.m_calls.Clear();
            }
            finally
            {
                this.m_inOnTimer--;
                this.m_lock.ReleaseWriterLock();
            }

            foreach (OneCall call in calls)
            {
                if (call.name == "borrow")
                {
                    /*
                    Delegate_Borrow d = (Delegate_Borrow)call.func;
                    this.MainForm.Invoke(d, call.parameters);
                     * */
                    Borrow((IChargingForm)call.parameters[0],
                        (bool)call.parameters[1],
                        (string)call.parameters[2],
                        (string)call.parameters[3],
                        (string)call.parameters[4],
                        (string)call.parameters[5],
                        (string)call.parameters[6],
                        (BorrowInfo)call.parameters[7],
                        (DateTime)call.parameters[8],
                        (DateTime)call.parameters[9]);
                }
                else if (call.name == "return")
                {
                    /*
                    Delegate_Return d = (Delegate_Return)call.func;
                    this.MainForm.Invoke(d, call.parameters);
                     * */
                    Return((IChargingForm)call.parameters[0],
                        (bool)call.parameters[1],
                        (string)call.parameters[2],
                        (string)call.parameters[3],
                        (string)call.parameters[4],
                        (string)call.parameters[5],
                        (string)call.parameters[6],
                        (ReturnInfo)call.parameters[7],
                        (DateTime)call.parameters[8],
                        (DateTime)call.parameters[9]);
                }
                else if (call.name == "amerce")
                {
                    /*
                    Delegate_Amerce d = (Delegate_Amerce)call.func;
                    this.MainForm.Invoke(d, call.parameters);
                     * */
                    Amerce((string)call.parameters[0],
    (string)call.parameters[1],
    (List<OverdueItemInfo>)call.parameters[2],
    (string)call.parameters[3],
    (DateTime)call.parameters[4],
    (DateTime)call.parameters[5]);
                }
            }

#if NO
            if (calls.Count > 0)
            {
                this.m_lock.AcquireWriterLock(m_nLockTimeout);
                this.m_inOnTimer++;
                try
                {
                    for (int i = 0; i < calls.Count; i++)
                    {
                        this.m_calls.RemoveAt(0);
                    }
                }
                finally
                {
                    this.m_inOnTimer--;
                    this.m_lock.ReleaseWriterLock();
                }
            }
#endif
        }

#if NO
        void ThreadMain()
        {
            m_bStopThread = false;
            try
            {
                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventActive;

                while (m_bStopThread == false)
                {
                    int index = 0;
                    try
                    {
                        index = WaitHandle.WaitAny(events, PerTime, false);
                    }
                    catch (System.Threading.ThreadAbortException /*ex*/)
                    {
                        break;
                    }

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // 超时
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();

                    }
                    else if (index == 0)
                    {
                        break;
                    }
                    else
                    {
                        // 得到激活信号
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();
                    }
                }

                return;
            }
            finally
            {
                m_bStopThread = true;
            }
        }

        public bool Stopped
        {
            get
            {
                return m_bStopThread;
            }
        }

        void StopThread(bool bForce)
        {
            if (this._thread == null)
                return;

            // 如果以前在做，立即停止
            m_bStopThread = true;
            this.eventClose.Set();

            if (bForce == true)
            {
                if (this._thread != null)
                {
                    if (!this._thread.Join(2000))
                        this._thread.Abort();
                    this._thread = null;
                }
            }
        }

        public void BeginThread()
        {
            if (this._thread != null)
                return;

            // 如果以前在做，立即停止
            StopThread(true);



            this._thread = new Thread(new ThreadStart(this.ThreadMain));
            this._thread.Start();
        }

        public void Activate()
        {
            eventActive.Set();
        }
#endif

        void AddCall(OneCall call)
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.m_calls.Add(call);
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            Activate();
        }

#endif

        #endregion


#if NO
        internal void OnTimer()
        {
            if (this.m_inOnTimer > 0)
                return;
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.m_inOnTimer++;
            try
            {
                for (int i = 0; i < this.m_calls.Count; i++)
                {
                    OneCall call = this.m_calls[i];

                    if (call.name == "borrow")
                    {
                        Delegate_Borrow d = (Delegate_Borrow)call.func;
                        this.MainForm.Invoke(d, call.parameters);
                    }
                    else if (call.name == "return")
                    {
                        Delegate_Return d = (Delegate_Return)call.func;
                        this.MainForm.Invoke(d, call.parameters);
                    }
                    else if (call.name == "amerce")
                    {
                        Delegate_Amerce d = (Delegate_Amerce)call.func;
                        this.MainForm.Invoke(d, call.parameters);
                    }
                }

                this.m_calls.Clear();
            }
            finally
            {
                this.m_inOnTimer--;
                this.m_lock.ReleaseWriterLock();
            }
        }
#endif

        internal delegate void Delegate_Borrow(IChargingForm charging_form,
            bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strReaderSummary,
            string strItemXml, 
            BorrowInfo borrow_info,
            DateTime start_time,
            DateTime end_time);

        // 借阅动作异步事件
        internal void BorrowAsync(IChargingForm charging_form,
            bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strReaderSummary,
            string strItemXml,
            BorrowInfo borrow_info,
            DateTime start_time,
            DateTime end_time)
        {

#if !USE_THREAD
            Delegate_Borrow d = new Delegate_Borrow(Borrow);
            this.MainForm.BeginInvoke(d, new object[] { charging_form,
            bRenew,
            strReaderBarcode,
            strItemBarcode,
            strConfirmItemRecPath,
            strReaderSummary,
            strItemXml, 
            borrow_info,
            start_time,
            end_time});
#else
            OneCall call = new OneCall();
            call.name = "borrow";
            call.func = new Delegate_Borrow(Borrow);
            call.parameters = new object[] { charging_form,
            bRenew,
            strReaderBarcode,
            strItemBarcode,
            strConfirmItemRecPath,
            strReaderSummary,
            strItemXml, 
            borrow_info,
            start_time,
            end_time};

            AddCall(call);
#endif
        }

        static string DoubleToString(double v)
        {
            return v.ToString("0.00");
        }

        /// <summary>
        /// 获得书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认的册记录路径。可以为空</param>
        /// <param name="strSummary">书目摘要</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError中；0: 没有找到; 1: 找到了</returns>
        public int GetBiblioSummary(string strItemBarcode,
    string strConfirmItemRecPath,
    out string strSummary,
    out string strError)
        {
#if USE_LOCAL_CHANNEL
            string strBiblioRecPath = "";

            int nRet = this.MainForm.GetCachedBiblioSummary(strItemBarcode,
strConfirmItemRecPath,
out strSummary,
out strError);
            if (nRet == -1 || nRet == 1)
                return nRet;

            Debug.Assert(nRet == 0, "");

            long lRet = Channel.GetBiblioSummary(
                null,
                strItemBarcode,
                strConfirmItemRecPath,
                null,
                out strBiblioRecPath,
                out strSummary,
                out strError);
            if (lRet == -1)
            {
                return -1;
            }
            else
            {
                // 2013/12/13
                this.MainForm.SetBiblioSummaryCache(strItemBarcode,
                     strConfirmItemRecPath,
                     strSummary);
            }
            return (int)lRet;
#else
            return this.MainForm.GetBiblioSummary(strItemBarcode,
                        strConfirmItemRecPath,
                        out strSummary,
                        out strError);
#endif
        }

        internal void Borrow(
            IChargingForm charging_form,
            bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strReaderSummary,
            string strItemXml,  // 2008/5/9
            BorrowInfo borrow_info,
            DateTime start_time,
            DateTime end_time)
        {
            TimeSpan delta = end_time - start_time; // 未包括GetSummary()的时间

            string strText = "";
            int nRet = 0;

            string strOperName = "借";
            if (bRenew == true)
                strOperName = "续借";

            string strError = "";
            string strSummary = "";

            nRet = this.GetBiblioSummary(strItemBarcode,
                    strConfirmItemRecPath,
                    out strSummary,
                    out strError);
            if (nRet == -1)
                strSummary = strError;

            string strOperClass = "even";
            if ((this.m_nCount % 2) == 1)
                strOperClass = "odd";

            string strItemLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + HttpUtility.HtmlEncode(strItemBarcode) + "</a>";
            string strReaderLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + HttpUtility.HtmlEncode(strReaderBarcode) + "</a>";

            strText = "<div class='item " + strOperClass + " borrow'>"
                + "<div class='time_line'>"
                + " <div class='time'>" + DateTime.Now.ToLongTimeString() + "</div>"
                + " <div class='time_span'>耗时 " + DoubleToString(delta.TotalSeconds) + "秒</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + "<div class='reader_line'>"
                + " <div class='reader_prefix_text'>读者</div>"
                + " <div class='reader_barcode'>" + strReaderLink + "</div>"
                + " <div class='reader_summary'>" + HttpUtility.HtmlEncode(strReaderSummary) + "</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + "<div class='opername_line'>"
                + " <div class='opername'>" + HttpUtility.HtmlEncode(strOperName) + "</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + "<div class='item_line'>"
                + " <div class='item_prefix_text'>册</div>"
                + " <div class='item_barcode'>" + strItemLink + "</div> "
                + " <div class='item_summary'>" + HttpUtility.HtmlEncode(strSummary) + "</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + " <div class='clear'></div>"
                + "</div>";
            /*
            strText = "<div class='" + strOperClass + "'>"
    + "<div class='time_line'><span class='time'>" + DateTime.Now.ToLongTimeString() + "</span> <span class='time_span'>耗时 " + delta.TotalSeconds.ToString() + "秒</span></div>"
    + "<div class='reader_line'><span class='reader_prefix_text'>读者</span> <span class='reader_barcode'>[" + strReaderBarcode + "]</span>"
+ " <span class='reader_summary'>" + strReaderSummary + "<span></div>"
+ "<div class='opername_line'><span class='opername'>" + strOperName + "<span></div>"
+ "<div class='item_line'><span class='item_prefix_text'>册</span> <span class='item_barcode'>[" + strItemBarcode + "]</span> "
+ "<span class='item_summary' id='" + m_nCount.ToString() + "' onreadystatechange='GetOneSummary(\"" + m_nCount.ToString() + "\");'>" + strItemBarcode + "</span></div>"
+ "</div>";
             * */

            AppendHtml(strText);
            m_nCount++;


            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                BorrowedEventArgs e = new BorrowedEventArgs();
                e.OperName = strOperName;
                e.BiblioSummary = strSummary;
                e.ItemBarcode = strItemBarcode;
                e.ReaderBarcode = strReaderBarcode;
                e.TimeSpan = delta;
                e.ReaderSummary = strReaderSummary;
                e.ItemXml = strItemXml;
                e.ChargingForm = charging_form;

                if (borrow_info != null)
                {
                    if (String.IsNullOrEmpty(borrow_info.LatestReturnTime) == true)
                        e.LatestReturnDate = new DateTime(0);
                    else
                        e.LatestReturnDate = DateTimeUtil.FromRfc1123DateTimeString(borrow_info.LatestReturnTime).ToLocalTime();
                    e.Period = borrow_info.Period;
                    e.BorrowCount = borrow_info.BorrowCount;
                    e.BorrowOperator = borrow_info.BorrowOperator;
                }

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnBorrowed(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
                /*
                if (nRet == -1)
                {
                    strText = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(strError);
                    AppendHtml(strText);
                }*/
            }

            // 用tips飞出显示读者和册的摘要信息？或者明确显示在条码后面？
            // 读者证和册条码号本身就是锚点？
            // 读者摘要要么做在前端，通过XML发生，要么做在服务器，用固定规则发生。
        }

        internal delegate void Delegate_Return(IChargingForm charging_form,
            bool bLost,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strReaderSummary,
            string strItemXml,
            ReturnInfo return_info,
            DateTime start_time,
            DateTime end_time);

        internal void ReturnAsync(IChargingForm charging_form,
            bool bLost,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strReaderSummary,
            string strItemXml,
            ReturnInfo return_info,
            DateTime start_time,
            DateTime end_time)
        {
#if !USE_THREAD

            Delegate_Return d = new Delegate_Return(Return);
            this.MainForm.BeginInvoke(d, new object[] {charging_form,
            bLost,
            strReaderBarcode,
            strItemBarcode,
            strConfirmItemRecPath,
            strReaderSummary,
            strItemXml,
            return_info,
            start_time,
            end_time});
#else
            OneCall call = new OneCall();
            call.name = "return";
            call.func = new Delegate_Return(Return);
            call.parameters = new object[] { charging_form,
            bLost,
            strReaderBarcode,
            strItemBarcode,
            strConfirmItemRecPath,
            strReaderSummary,
            strItemXml,
            return_info,
            start_time,
            end_time};

            AddCall(call);
#endif
        }

        internal void Return(
            IChargingForm charging_form,
            bool bLost,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strReaderSummary,
            string strItemXml,  // 2008/5/9
            ReturnInfo return_info,
            DateTime start_time,
            DateTime end_time)
        {
            TimeSpan delta = end_time - start_time; // 未包括GetSummary()的时间

            string strText = "";
            int nRet = 0;

            string strOperName = "还";
            if (bLost == true)
                strOperName = "丢失";

            string strError = "";
            string strSummary = "";
            nRet = this.GetBiblioSummary(strItemBarcode,
                    strConfirmItemRecPath,
                    out strSummary,
                    out strError);
            if (nRet == -1)
                strSummary = strError;

            string strOperClass = "even";
            if ((this.m_nCount % 2) == 1)
                strOperClass = "odd";

            string strLocation = "";
            if (return_info != null)
                strLocation = return_info.Location;

            string strItemLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + HttpUtility.HtmlEncode(strItemBarcode) + "</a>";
            string strReaderLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + HttpUtility.HtmlEncode(strReaderBarcode) + "</a>";

            strText = "<div class='item " + strOperClass + " return'>"
                + "<div class='time_line'>"
                + " <div class='time'>" + DateTime.Now.ToLongTimeString() + "</div>"
                + " <div class='time_span'>耗时 " + DoubleToString(delta.TotalSeconds) + "秒</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + "<div class='reader_line'>"
                + " <div class='reader_prefix_text'>读者</div>"
                + " <div class='reader_barcode'>" + strReaderLink + "</div>"
                + " <div class='reader_summary'>" + HttpUtility.HtmlEncode(strReaderSummary) + "</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + "<div class='opername_line'>"
                + " <div class='opername'>" + HttpUtility.HtmlEncode(strOperName) + "</div>"
                + " <div class='clear'></div>"
                + "</div>"
                + "<div class='item_line'>"
                + " <div class='item_prefix_text'>册</div>"
                + " <div class='item_barcode'>" + strItemLink + "</div> "
                + " <div class='item_summary'>" + HttpUtility.HtmlEncode(strSummary) + "</div>"

                + (string.IsNullOrEmpty(strLocation) == false ? " <div class='item_location'>" + HttpUtility.HtmlEncode(strLocation) + "</div>" : "")
                + " <div class='clear'></div>"
                + "</div>"
                + " <div class='clear'></div>"
                + "</div>";

            /*
            strText = "<div class='" + strOperClass + "'>"
    + "<div class='time_line'><span class='time'>" + DateTime.Now.ToLongTimeString() + "</span> <span class='time_span'>耗时 " + delta.TotalSeconds.ToString() + "秒</span></div>"
    + "<div class='reader_line'><span class='reader_prefix_text'>读者</span> <span class='reader_barcode'>[" + strReaderBarcode + "]</span>"
+ " <span class='reader_summary'>" + strReaderSummary + "<span></div>"
+ "<div class='opername_line'><span class='opername'>" + strOperName + "<span></div>"
+ "<div class='item_line'><span class='item_prefix_text'>册</span> <span class='item_barcode'>[" + strItemBarcode + "]</span> "
+ "<span class='item_summary' id='" + m_nCount.ToString() + "' onreadystatechange='GetOneSummary(\"" + m_nCount.ToString() + "\");'>" + strItemBarcode + "</span></div>"
+ "</div>";
             * */
            AppendHtml(strText);
            m_nCount++;

            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                ReturnedEventArgs e = new ReturnedEventArgs();
                e.OperName = strOperName;
                e.BiblioSummary = strSummary;
                e.ItemBarcode = strItemBarcode;
                e.ReaderBarcode = strReaderBarcode;
                e.TimeSpan = delta;
                e.ReaderSummary = strReaderSummary;
                e.ItemXml = strItemXml;
                e.ChargingForm = charging_form;

                if (return_info != null)
                {
                    if (String.IsNullOrEmpty(return_info.BorrowTime) == true)
                        e.BorrowDate = new DateTime(0);
                    else
                        e.BorrowDate = DateTimeUtil.FromRfc1123DateTimeString(return_info.BorrowTime).ToLocalTime();

                    if (String.IsNullOrEmpty(return_info.LatestReturnTime) == true)
                        e.LatestReturnDate = new DateTime(0);
                    else
                        e.LatestReturnDate = DateTimeUtil.FromRfc1123DateTimeString(return_info.LatestReturnTime).ToLocalTime();
                    e.Period = return_info.Period;
                    e.BorrowCount = return_info.BorrowCount;
                    e.OverdueString = return_info.OverdueString;

                    e.BorrowOperator = return_info.BorrowOperator;
                    e.ReturnOperator = return_info.ReturnOperator;

                    // 2013/4/2
                    e.Location = return_info.Location;
                    e.BookType = return_info.BookType;
                }

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnReturned(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
            }
        }

        delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.MainForm.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.MainForm.BeginInvoke(d, new object[] { strText });
                return;
            }

            Global.WriteHtml(this.WebBrowser,
                strText);
            // Global.ScrollToEnd(this.WebBrowser);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.WebBrowser.Document.Window.ScrollTo(0,
    this.WebBrowser.Document.Body.ScrollRectangle.Height);
        }

        internal delegate void Delegate_Amerce(string strReaderBarcode,
            string strReaderSummary,
            List<OverdueItemInfo> overdue_infos,
            string strAmerceOperator,
            DateTime start_time,
            DateTime end_time);

        internal void AmerceAsync(string strReaderBarcode,
            string strReaderSummary,
            List<OverdueItemInfo> overdue_infos,
            string strAmerceOperator,
            DateTime start_time,
            DateTime end_time)
        {

#if !USE_THREAD
            Delegate_Amerce d = new Delegate_Amerce(Amerce);
            this.MainForm.BeginInvoke(d, new object[] {strReaderBarcode,
                strReaderSummary,
                overdue_infos,
                strAmerceOperator,
                start_time,
                end_time});
#else
            OneCall call = new OneCall();
            call.name = "amerce";
            call.func = new Delegate_Amerce(Amerce);
            call.parameters = new object[] { strReaderBarcode,
            strReaderSummary,
            overdue_infos,
            strAmerceOperator,
            start_time,
            end_time};

            AddCall(call);
#endif
        }

        internal void Amerce(
            string strReaderBarcode,
            string strReaderSummary,
            List<OverdueItemInfo> overdue_infos,
            string strAmerceOperator,
            DateTime start_time,
            DateTime end_time)
        {
            string strOperName = "交费";
            TimeSpan delta = end_time - start_time;

            string strText = "";
            int nRet = 0;


            foreach (OverdueItemInfo info in overdue_infos)
            {
                string strOperClass = "even";
                if ((this.m_nCount % 2) == 1)
                    strOperClass = "odd";

                string strSummary = "";

                string strItemLink = "";

                if (string.IsNullOrEmpty(info.ItemBarcode) == false)
                {
                    strItemLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + HttpUtility.HtmlEncode(info.ItemBarcode) + "</a>";
                    string strError = "";
                    nRet = this.GetBiblioSummary(info.ItemBarcode,
    info.RecPath,
    out strSummary,
    out strError);
                    if (nRet == -1)
                        strSummary = strError;

                    strItemLink += " <div class='item_summary'>" + HttpUtility.HtmlEncode(strSummary) + "</div>";
                }

                string strReaderLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + HttpUtility.HtmlEncode(strReaderBarcode) + "</a>";

                string strTimePrefix = "";
                if (overdue_infos.Count > 1)
                    strTimePrefix = overdue_infos.Count.ToString() + "笔交费共";

                strText = "<div class='item " + strOperClass + " amerce'>"
                    + "<div class='time_line'>"
                    + " <div class='time'>" + DateTime.Now.ToLongTimeString() + "</div>"
                    + " <div class='time_span'>" + strTimePrefix + "耗时 " + DoubleToString(delta.TotalSeconds) + "秒</div>"
                    + " <div class='clear'></div>"
                    + "</div>"
                    + "<div class='reader_line'>"
                    + " <div class='reader_prefix_text'>读者</div>"
                    + " <div class='reader_barcode'>" + strReaderLink + "</div>"
                    + " <div class='reader_summary'>" + HttpUtility.HtmlEncode(strReaderSummary) + "</div>"
                    + " <div class='clear'></div>"
                    + "</div>"
                    + "<div class='opername_line'>"
                    + " <div class='opername'>" + HttpUtility.HtmlEncode(strOperName) + "</div>"
                    + " <div class='clear'></div>"
                    + "</div>"
                    + "<div class='item_line'>"
                    + info.ToHtmlString(strItemLink)
                    + "</div>"
                    + " <div class='clear'></div>"
                    + "</div>";
                AppendHtml(strText);
                m_nCount++;

            }

            // 运行Script代码
            if (this.PrintAssembly != null)
            {
                AmercedEventArgs e = new AmercedEventArgs();
                e.OperName = strOperName;
                e.ReaderBarcode = strReaderBarcode;
                e.ReaderSummary = strReaderSummary;
                e.TimeSpan = delta;

                e.OverdueInfos = overdue_infos;

                e.AmerceOperator = strAmerceOperator;

                this.PrintHostObj.MainForm = this.MainForm;
                this.PrintHostObj.Assembly = this.PrintAssembly;
                try
                {
                    this.PrintHostObj.OnAmerced(this, e);
                }
                catch (Exception ex)
                {
                    string strErrorInfo = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(ExceptionUtil.GetDebugText(ex));
                    AppendHtml(strErrorInfo);
                }
                /*
                if (nRet == -1)
                {
                    strText = "<br/>单据打印脚本运行时出错: " + HttpUtility.HtmlEncode(strError);
                    AppendHtml(strText);
                }*/
            }

        }

#if NOOOOOOOOOOOOOO
        int PrepareScript(string strCode,
            string strRef,
            out string strError)
        {
            strError = "";
            string[] saRef = null;
            int nRet;

            nRet = ScriptManager.GetRefsFromXml(strRef,
                out saRef,
                out strError);
            if (nRet == -1)
            {
                strError = "strRef代码\r\n\r\n" + strRef + "\r\n\r\n格式错误: " + strError;
                return -1;
            }

            // 2007/12/4
            ScriptManager.RemoveRefsBinDirMacro(ref saRef);

            string[] saAddRef = {
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe"
								};

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            string strErrorInfo = "";
            string strWarningInfo = "";
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                null,   // strLibPaths,
                out this.PrintAssembly,
                out strErrorInfo,
                out strWarningInfo);
            if (nRet == -1)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strErrorInfo;
                return -1;
            }

            // 得到Assembly中PrintHost派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                PrintAssembly,
                "dp2Circulation.PrintHost");
            if (entryClassType == null)
            {
                strError = "dp2Circulation.PrintHost派生类没有找到";
                return -1;
            }

            // new一个PrintHost派生对象
            this.PrintHostObj = (PrintHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            if (PrintHostObj == null)
            {
                strError = "new PrintHost派生类对象失败";
                return -1;
            }



            return 0;
        }
#endif

        string m_strInstanceDir = "";
        // 创建唯一的实例目录。dp2Circulation退出后这个目录不会被保留
        string InstanceDir
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strInstanceDir) == false)
                    return this.m_strInstanceDir;

                this.m_strInstanceDir = PathUtil.MergePath(this.MainForm.DataDir, "~bin_" + Guid.NewGuid().ToString());
                PathUtil.CreateDirIfNeed(this.m_strInstanceDir);

                return this.m_strInstanceDir;
            }
        }

        // 准备脚本环境
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            out string strError)
        {
            strError = "";
            this.PrintAssembly = null;

            PrintHostObj = null;

            string strWarning = "";


            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~charging_print_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

            string strLibPaths = "\"" + this.MainForm.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe"
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "OperHistory",
                strProjectName,
                "main.cs",
                saAddRef,
                strLibPaths,
                strMainCsDllName,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                    goto ERROR1;
                MessageBox.Show(this.MainForm, strWarning);
            }


            this.PrintAssembly = Assembly.LoadFrom(strMainCsDllName);
            if (this.PrintAssembly == null)
            {
                strError = "LoadFrom " + strMainCsDllName + " fail";
                goto ERROR1;
            }

            // 得到Assembly中PrintHost派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                PrintAssembly,
                "dp2Circulation.PrintHost");
            if (entryClassType == null)
            {
                strError = "dp2Circulation.PrintHost派生类没有找到";
                return -1;
            }

            // new一个PrintHost派生对象
            this.PrintHostObj = (PrintHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            if (this.PrintHostObj == null)
            {
                strError = "new PrintHost派生类对象失败";
                return -1;
            }

            this.PrintHostObj.ProjectDir = strProjectLocate;
            this.PrintHostObj.InstanceDir = this.InstanceDir;
            return 0;
        ERROR1:
            return -1;
        }
    }

    class OneCall
    {
        public string name = "";
        public object func = null;
        public object [] parameters = null;
    }
}
