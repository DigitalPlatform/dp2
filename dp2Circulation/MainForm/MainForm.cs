using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;

using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Deployment.Application;

using System.Diagnostics;
using System.Net;   // for WebClient class
using System.IO;
using System.Web;
using System.Reflection;
using System.Drawing.Text;
using System.Speech.Synthesis;
using System.Security.Permissions;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.IO;   // DateTimeUtil
using DigitalPlatform.CommonControl;

using DigitalPlatform.GcatClient.gcat_new_ws;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.MarcDom;
using DigitalPlatform.MessageClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 框架窗口
    /// </summary>
    public partial class MainForm : Form, IChannelManager
    {
        internal ImageManager _imageManager = null;

        public PropertyTaskList PropertyTaskList = new PropertyTaskList();

        internal CommentViewerForm m_commentViewer = null;

        // 主要的通道池，用于当前服务器
        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        // 次要的通道池，用于著者号码和拼音
        public LibraryChannelPool _channelPoolExt = new LibraryChannelPool();


        // 2014/10/3
        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();

        SpeechSynthesizer m_speech = new SpeechSynthesizer();

        private DigitalPlatform.Drawing.QrRecognitionControl qrRecognitionControl1;

        internal event EventHandler FixedSelectedPageChanged = null;

        #region 脚本支持

        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = new ScriptManager();

        MainFormHost objStatis = null;
        Assembly AssemblyMain = null;

        string m_strInstanceDir = "";

        /// <summary>
        /// C# 脚本执行的实例目录
        /// </summary>
        public string InstanceDir
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strInstanceDir) == false)
                    return this.m_strInstanceDir;

                this.m_strInstanceDir = PathUtil.MergePath(this.DataDir, "~bin_" + Guid.NewGuid().ToString());
                PathUtil.CreateDirIfNeed(this.m_strInstanceDir);

                return this.m_strInstanceDir;
            }
        }

        #endregion


        CommentViewerForm m_propertyViewer = null;

        internal ObjectCache<Assembly> AssemblyCache = new ObjectCache<Assembly>();
        internal ObjectCache<XmlDocument> DomCache = new ObjectCache<XmlDocument>();

        // internal FormWindowState MdiWindowState = FormWindowState.Normal;

        // 服务器配置的前端交费信息接口参数
        /*
<clientFineInterface name="迪科远望"/>
         * * */
        /// <summary>
        /// 服务器配置的前端交费信息接口参数
        /// </summary>
        public string ClientFineInterfaceName = "";

        /// <summary>
        /// 从服务器端获取的 CallNumber 配置信息
        /// </summary>
        public XmlDocument CallNumberCfgDom = null;
        string CallNumberInfo = "";  // <callNumber>元素的InnerXml

        // public string LibraryServerDiretory = "";   // dp2libraryws的library.xml中配置的<libraryserver url='???'>内容

        /// <summary>
        /// MDI Client 区域
        /// </summary>
        public MdiClient MdiClient = null;

        BackgroundForm m_backgroundForm = null; // MDI Client 上面用于显示文字的窗口

        /// <summary>
        /// 书目摘要本地缓存
        /// </summary>
        public StringCache SummaryCache = new StringCache();

        /// <summary>
        /// 读者 XML 缓存
        /// 只用于获取读者证照片时。因为这时对内容变动不敏感
        /// </summary>
        public StringCache ReaderXmlCache = new StringCache();  // 只用于获取读者证照片时。因为这时对内容变动不敏感 2012/1/5

        // 为C#脚本所准备
        /// <summary>
        /// 参数存储
        /// 为 C# 脚本所准备
        /// </summary>
        public Hashtable ParamTable = new Hashtable();

        /// <summary>
        /// 快速加拼音对象
        /// </summary>
        public QuickPinyin QuickPinyin = null;

        /// <summary>
        /// ISBN 切割对象
        /// </summary>
        public IsbnSplitter IsbnSplitter = null;

        /// <summary>
        /// 四角号码对象
        /// </summary>
        public QuickSjhm QuickSjhm = null;

        /// <summary>
        /// 卡特表对象
        /// </summary>
        public QuickCutter QuickCutter = null;

        /// <summary>
        /// 服务器配置文件缓存
        /// </summary>
        public CfgCache cfgCache = new CfgCache();

        // 统计窗中assembly的版本计数
        internal int StatisAssemblyVersion = 0;

        bool m_bUrgent = false;
        string EncryptKey = "dp2circulation_client_password_key";

        /// <summary>
        /// 数据目录
        /// </summary>
        public string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public string UserDir = ""; // 2013/6/16

        public string UserTempDir = ""; // 2015/1/4

        public string UserLogDir = ""; // 2015/7/8

        // 保存界面信息
        /// <summary>
        /// 配置存储
        /// </summary>
        public ApplicationInfo AppInfo = null;  // new ApplicationInfo("dp2circulation.xml");

        /// <summary>
        /// Stop 管理器
        /// </summary>
        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

        #region 数据库信息集合

        /// <summary>
        /// 书目库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] BiblioDbFromInfos = null;   // 书目库检索路径信息

        /// <summary>
        /// 读者库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] ReaderDbFromInfos = null;   // 读者库检索路径信息 2012/2/8

        /// <summary>
        /// 实体库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] ItemDbFromInfos = null;   // 实体库检索路径信息 2012/5/5

        /// <summary>
        /// 订购库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] OrderDbFromInfos = null;   // 订购库检索路径信息 2012/5/5

        /// <summary>
        /// 期库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] IssueDbFromInfos = null;   // 期库检索路径信息 2012/5/5

        /// <summary>
        /// 评注库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] CommentDbFromInfos = null;   // 评注库检索路径信息 2012/5/5

        /// <summary>
        /// 发票库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] InvoiceDbFromInfos = null;   // 发票库检索路径信息 2012/11/8

        /// <summary>
        /// 违约金库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] AmerceDbFromInfos = null;   // 违约金库检索路径信息 2012/11/8

        /// <summary>
        /// 预约到书库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] ArrivedDbFromInfos = null;   // 预约到书库检索路径信息 2015/6/13

        /// <summary>
        /// 书目库属性集合
        /// </summary>
        public List<BiblioDbProperty> BiblioDbProperties = null;

        /// <summary>
        /// 普通库属性集合
        /// </summary>
        public List<NormalDbProperty> NormalDbProperties = null;

        // public string[] ReaderDbNames = null;
        /// <summary>
        /// 读者库属性集合
        /// </summary>
        public List<ReaderDbProperty> ReaderDbProperties = null;

        /// <summary>
        /// 实用库属性集合
        /// </summary>
        public List<UtilDbProperty> UtilDbProperties = null;

        #endregion

        /// <summary>
        /// 当前连接的服务器的图书馆名
        /// </summary>
        public string LibraryName = "";

        //
        internal ReaderWriterLock m_lockChannel = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

#if NO
        /// <summary>
        /// 通讯通道。MainForm 自己使用
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();
#endif

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 界面语言代码
        /// </summary>
        public string Lang = "zh";

        // const int WM_PREPARE = API.WM_USER + 200;

        Hashtable valueTableCache = new Hashtable();

        /// <summary>
        /// 操作历史对象
        /// </summary>
        public OperHistory OperHistory = null;

        /// <summary>
        /// 消息管理对象
        /// </summary>
        public MessageHub MessageHub = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            try
            {
                this.qrRecognitionControl1 = new DigitalPlatform.Drawing.QrRecognitionControl();

                // 
                // tabPage_camera
                // 
                this.tabPage_camera.Controls.Add(this.qrRecognitionControl1);
#if NO
            this.tabPage_camera.Location = new System.Drawing.Point(4, 25);
            this.tabPage_camera.Name = "tabPage_camera";
            this.tabPage_camera.Size = new System.Drawing.Size(98, 202);
            this.tabPage_camera.TabIndex = 4;
            this.tabPage_camera.Text = "QR 识别";
            this.tabPage_camera.UseVisualStyleBackColor = true;
#endif
                // 
                // qrRecognitionControl1
                // 
                this.qrRecognitionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                this.qrRecognitionControl1.Location = new System.Drawing.Point(0, 0);
                this.qrRecognitionControl1.Name = "qrRecognitionControl1";
                this.qrRecognitionControl1.Size = new System.Drawing.Size(98, 202);
                this.qrRecognitionControl1.MinimumSize = new Size(200, 200);
                this.qrRecognitionControl1.TabIndex = 0;
                this.qrRecognitionControl1.BackColor = Color.DarkGray;   //  System.Drawing.SystemColors.Window;
            }
            catch (FileLoadException ex)
            {
                if (Detect360() == true)
                {
                    MessageBox.Show("dp2Circulation (内务)受到 360 软件干扰而无法启动。请关闭或者卸载 360 软件然后再重新启动 dp2Circulation (内务)");
                    throw ex;
                }
                ReportError("dp2circulation 创建 QrRecognitionControl 过程出现异常", ExceptionUtil.GetDebugText(ex));
            }
            catch (Exception ex)
            {
                ReportError("dp2circulation 创建 QrRecognitionControl 过程出现异常", ExceptionUtil.GetDebugText(ex));
            }
        }

        /// <summary>
        /// 是否为安装后第一次运行
        /// </summary>
        public bool IsFirstRun
        {
            get
            {
                try
                {
                    if (ApplicationDeployment.CurrentDeployment.IsFirstRun == true)
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            this._channelPoolExt.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(ChannelExt_BeforeLogin);
            this._channelPoolExt.AfterLogin += new AfterLoginEventHandle(ChannelExt_AfterLogin);

            this.BeginInvoke(new Action(FirstInitial));
        }

        /*
发生未捕获的界面线程异常: 
Type: System.ObjectDisposedException
Message: 无法访问已释放的对象。
对象名:“Icon”。
Stack:
在 System.Drawing.Icon.get_Handle()
在 System.Drawing.Icon.get_Size()
在 System.Drawing.Icon.ToBitmap()
在 System.Windows.Forms.MdiControlStrip.GetTargetWindowIcon()
在 System.Windows.Forms.MdiControlStrip..ctor(IWin32Window target)
在 System.Windows.Forms.Form.UpdateMdiControlStrip(Boolean maximized)
在 System.Windows.Forms.Form.UpdateToolStrip()
在 System.Windows.Forms.Form.OnMdiChildActivate(EventArgs e)
在 System.Windows.Forms.Form.WmMdiActivate(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
         * */
        protected override void OnMdiChildActivate(EventArgs e)
        {
            try
            {
                base.OnMdiChildActivate(e);
            }
            catch (System.ObjectDisposedException)
            {

            }
        }

        string m_strPrevMessageText = "";

        void SetFirstDefaultFont()
        {
            if (this.DefaultFont != null)
                return;

            try
            {
                FontFamily family = new FontFamily("微软雅黑");
            }
            catch
            {
                return;
            }
            this.DefaultFontString = "微软雅黑, 9pt";
        }

        void RemoveBarcodeFont()
        {
            GlobalVars.PrivateFonts.Dispose();

            string strFontFilePath = Path.Combine(this.DataDir, "b3901.ttf");
            API.RemoveFontResourceA(strFontFilePath);
        }

        void InstallBarcodeFont()
        {
            bool bInstalled = true;
            try
            {
                FontFamily family = new FontFamily("C39HrP24DhTt");
            }
            catch
            {
                bInstalled = false;
            }

            if (bInstalled == true)
            {
                // 已经安装
                return;
            }

            // 
            string strFontFilePath = Path.Combine(this.DataDir, "b3901.ttf");
            int nRet = API.AddFontResourceA(strFontFilePath);
            if (nRet == 0)
            {
                // 失败
                MessageBox.Show(this, "安装字体文件 " + strFontFilePath + " 失败");
                return;
            }

            {
                // 成功

                // 为了解决 GDI+ 的一个 BUG
                // PrivateFontCollection m_pfc = new PrivateFontCollection();
                GlobalVars.PrivateFonts.AddFontFile(strFontFilePath);
#if NO
                API.SendMessage((IntPtr)0xffff,0x001d, IntPtr.Zero, IntPtr.Zero);
                API.SendMessage(this.Handle, 0x001d, IntPtr.Zero, IntPtr.Zero);
#endif
            }

#if NO
            /*
            try
            {
                FontFamily family = new FontFamily("C39HrP24DhTt");
            }
            catch (Exception ex)
            {
                bInstalled = false;
            }
             * */
            InstalledFontCollection enumFonts = new InstalledFontCollection();
            FontFamily[] fonts = enumFonts.Families;

            string strResult = "";
            foreach (FontFamily m in fonts)
            {
                strResult += m.Name + "\r\n";
            }

            int i = 0;
            i++;
#endif
        }

        void MdiClient_ClientSizeChanged(object sender, EventArgs e)
        {
            AcceptForm top = this.GetTopChildWindow<AcceptForm>();
            if (top != null)
            {
                top.OnMdiClientSizeChanged();
            }
        }

        /// <summary>
        /// 开始初始化，获得各种参数
        /// </summary>
        /// <param name="bFullInitial">是否执行全部初始化</param>
        /// <param name="bRestoreLastOpenedWindow">是否要恢复以前打开的窗口</param>
        public void StartPrepareNames(bool bFullInitial, bool bRestoreLastOpenedWindow)
        {
#if NO
            if (bFullInitial == true)
                API.PostMessage(this.Handle, WM_PREPARE, 1, 0);
            else
                API.PostMessage(this.Handle, WM_PREPARE, 0, 0);
#endif
            this.BeginInvoke(new Func<bool, bool, bool>(InitialProperties), bFullInitial, bRestoreLastOpenedWindow);
        }

        void InitialFixedPanel()
        {
            string strDock = this.AppInfo.GetString(
                "MainForm",
                "fixedpanel_dock",
                "right");
            int nFixedHeight = this.AppInfo.GetInt(
                "MainForm",
                "fixedpanel_height",
                100);
            int nFixedWidth = this.AppInfo.GetInt(
                "MainForm",
                "fixedpanel_width",
                -1);
            // 首次打开窗口
            if (nFixedWidth == -1)
                nFixedWidth = this.Width / 3;

            if (strDock == "bottom")
            {
                this.panel_fixed.Dock = DockStyle.Bottom;
                this.panel_fixed.Size = new Size(this.panel_fixed.Width,
                    nFixedHeight);
            }
            else if (strDock == "top")
            {
                this.panel_fixed.Dock = DockStyle.Top;
                this.panel_fixed.Size = new Size(this.panel_fixed.Width,
                    nFixedHeight);
            }
            else if (strDock == "left")
            {
                this.panel_fixed.Dock = DockStyle.Left;
                this.panel_fixed.Size = new Size(nFixedWidth,
                    this.panel_fixed.Size.Height);
            }
            else if (strDock == "right")
            {
                this.panel_fixed.Dock = DockStyle.Right;
                this.panel_fixed.Size = new Size(nFixedWidth,
                    this.panel_fixed.Size.Height);
            }

            this.splitter_fixed.Dock = this.panel_fixed.Dock;

            bool bHide = this.AppInfo.GetBoolean(
                "MainForm",
                "hide_fixed_panel",
                false);
            if (bHide == true)
            {
                /*
                this.panel_fixed.Visible = false;
                this.splitter_fixed.Visible = false;
                 * */
                this.PanelFixedVisible = false;
            }

            try
            {
                this.tabControl_panelFixed.SelectedIndex = this.AppInfo.GetInt(
                    "MainForm",
                    "active_fixed_panel_page",
                    0);
            }
            catch
            {
            }
        }

        void FinishFixedPanel()
        {
            string strDock = "right";
            if (this.panel_fixed.Dock == DockStyle.Bottom)
                strDock = "bottom";
            else if (this.panel_fixed.Dock == DockStyle.Left)
                strDock = "left";
            else if (this.panel_fixed.Dock == DockStyle.Right)
                strDock = "right";
            else if (this.panel_fixed.Dock == DockStyle.Top)
                strDock = "top";

            this.AppInfo.SetString(
                "MainForm",
                "fixedpanel_dock",
                strDock);
            this.AppInfo.SetInt(
                "MainForm",
                "fixedpanel_height",
                this.panel_fixed.Size.Height);
            this.AppInfo.SetInt(
                "MainForm",
                "fixedpanel_width",
                this.panel_fixed.Size.Width);

            this.AppInfo.SetInt(
    "MainForm",
    "active_fixed_panel_page",
    this.tabControl_panelFixed.SelectedIndex);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // DisableChildTopMost();

            // 在前面关闭MDI子窗口的时候已经遇到了终止关闭的情况，这里就不用再次询问了
            if (e.CloseReason == CloseReason.UserClosing && e.Cancel == true)
                return;

            // if (e.CloseReason != CloseReason.ApplicationExitCall)
            if (e.CloseReason == CloseReason.UserClosing)   // 2014/8/13
            {
                if (this.Stop != null)
                {
                    if (this.Stop.State == 0)    // 0 表示正在处理
                    {
                        MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                        e.Cancel = true;
                        return;
                    }
                }

                // 警告关闭
                DialogResult result = MessageBox.Show(this,
                    "确实要退出 dp2Circulation -- 内务/流通 ? ",
                    "dp2Circulation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            CancelUpdateClickOnceApplication();
            CancelUpdateGreenApplication();
        }

        void TryReportPromptLines()
        {
            string strText = Program.GetPromptStringLines();
            if (string.IsNullOrEmpty(strText) == true)
                return;

            strText += "\r\n\r\n===\r\n" + PackageEventLog.GetEnvironmentDescription().Replace("\t", "    ");

            int nRet = 0;
            string strError = "";
            try
            {
                // 发送报告
                nRet = LibraryChannel.CrashReport(
                    this.GetCurrentUserName() + "@" + this.ServerUID,
                    "dp2circulation 强制退出前的提示",
                    strText,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "CrashReport() (退出前的提示) 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.WriteErrorLog(strError);
            }

            Program.ClearPromptStringLines();   // 防止以后再次重复发送
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (StringUtil.IsDevelopMode() == false
                && StringUtil.IsNewInstance() == false)
            {
                // PackageEventLog.EnvironmentReport(this);
                // Debug.WriteLine("EnvironmentReport");
                TryReportPromptLines();
            }

            RemoveBarcodeFont();

            this.PropertyTaskList.Close();

            if (this.m_commentViewer != null)
                this.m_commentViewer.Close();

            if (this.MessageHub != null)
                this.MessageHub.Destroy();

            if (m_propertyViewer != null)
                m_propertyViewer.Close();

            if (this.qrRecognitionControl1 != null)
            {
                if (AppInfo != null)
                    AppInfo.SetString(
                       "mainform",
                       "current_camera",
                       this.qrRecognitionControl1.CurrentCamera);
                this.qrRecognitionControl1.Catched -= new DigitalPlatform.Drawing.CatchedEventHandler(qrRecognitionControl1_Catched);
            }

            if (this.MdiClient != null)
                this.MdiClient.ClientSizeChanged -= new EventHandler(MdiClient_ClientSizeChanged);

            // this.timer_operHistory.Stop();

            if (this.OperHistory != null)
                this.OperHistory.Close();

            // 保存窗口尺寸状态
            if (AppInfo != null)
            {
                string strOpenedMdiWindow = GuiUtil.GetOpenedMdiWindowString(this);
                this.AppInfo.SetString(
                    "main_form",
                    "last_opened_mdi_window",
                    strOpenedMdiWindow);

                FinishFixedPanel();

                AppInfo.SaveFormStates(this,
                    "mainformstate");
            }

            // cfgcache
            if (cfgCache != null)
            {
                string strError;
                int nRet = cfgCache.Save(null, out strError);
                if (nRet == -1)
                {
                    if (string.IsNullOrEmpty(this.UserLogDir) == false)
                        this.WriteErrorLog(strError);
                    // MessageBox.Show(this, strError);
                }
            }

            if (this.AppInfo != null)
            {
                // 消除短期保存的密码
                bool bSavePasswordLong =
        AppInfo.GetBoolean(
        "default_account",
        "savepassword_long",
        false);

                if (bSavePasswordLong == false)
                {
                    AppInfo.SetString(
                        "default_account",
                        "password",
                        "");
                }

#if GCAT_SERVER
                if (this.m_bSavePinyinGcatID == false)
                    this.m_strPinyinGcatID = "";
                this.AppInfo.SetString("entity_form", "gcat_pinyin_api_id", this.m_strPinyinGcatID);
                this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", this.m_bSavePinyinGcatID);
#endif

                //记住save,保存信息XML文件
                AppInfo.Save();
                AppInfo = null;	// 避免后面再用这个对象
            }

            if (Stop != null) // 脱离关联
            {
                Stop.Unregister(true);
                // Stop = null;
            }

            if (this._channelPool != null)
            {
                this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
                this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
                this._channelPool.Close();
            }

            if (this._channelPoolExt != null)
            {
                this._channelPoolExt.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(ChannelExt_BeforeLogin);
                this._channelPoolExt.AfterLogin -= new AfterLoginEventHandle(ChannelExt_AfterLogin);
                this._channelPoolExt.Close();
            }

#if NO
            if (this.Channel != null)
                this.Channel.Close();   // TODO: 最好限制一个时间，超过这个时间则Abort()
#endif

            if (this._updatedGreenZipFileNames != null && this._updatedGreenZipFileNames.Count > 0)
                StartGreenUtility();
        }

#if NO
        // 检查服务器端library.xml中<libraryserver url="???">配置是否正常
        // return:
        //      -1  error
        //      0   正常
        //      1   不正常
        int CheckServerUrl(out string strError)
        {
            strError = "";

            // 2009/2/11 
            if (String.IsNullOrEmpty(this.LibraryServerUrl) == true)
                return 0;

            int nRet = this.LibraryServerUrl.ToLower().IndexOf("/dp2library");
            if (nRet == -1)
            {
                strError = "前端所配置的图书馆应用服务器WebServiceUrl '" + this.LibraryServerUrl + "' 格式错误：结尾应当为/dp2library";
                return -1;
            }

            string strFirstDirectory = this.LibraryServerUrl.Substring(0, nRet).ToLower();

            if (String.IsNullOrEmpty(this.LibraryServerDiretory) == true)
            {
                // 服务器端是旧版本(没有传递<libraryserver url="???">的能力)，服务器端根本没有配<libraryserver url="???">事项，或者通过前端获取这一参数的时候没有成功
                return 0;
            }

            string strSecondDirectory = this.LibraryServerDiretory.ToLower();

            if (strFirstDirectory == strSecondDirectory)
                return 0;   // 相等

            string strFirstUrl = strFirstDirectory + "/install_stamp.txt";
            string strSecondUrl = strSecondDirectory + "/install_stamp.txt";

            byte[] first_data = null;
            byte[] second_data = null;
            WebClient webClient = new WebClient();
            try
            {
                first_data = webClient.DownloadData(strFirstUrl);
            }
            catch (Exception ex)
            {
                strError = "下载" + strFirstUrl + "文件发生错误 :" + ex.Message;
                return 0;   // 无法判断，权且当作正常
            }

            string strSuggestion = "";

            try
            {
                Uri uri = new Uri(strFirstUrl);
                string strFirstHost = uri.Host.ToLower();
                if (strFirstHost != "localhost"
                    && strFirstHost != "127.0.0.1")
                {
                    strSuggestion = "\r\n\r\n建议：修改应用服务器library.xml配置文件中<libraryserver url='???'>配置，???部分可采用值 '" + strFirstDirectory + "'";
                }
            }
            catch
            {
            }

            try
            {
                second_data = webClient.DownloadData(strSecondUrl);
            }
            catch (Exception ex)
            {
                strError = "警告：利用图书馆应用服务器数据目录内library.xml配置文件中，<libraryserver url='???'>所配置的URL '" + this.LibraryServerDiretory + "' ，对其下的文件install_stamp.txt进行检测性访问的时出错: " + ex.Message + "，这表明该参数配置不正常。" + strSuggestion;
                return 1;   // 不正常
            }

            if (ByteArray.Compare(first_data, second_data) != 0)
            {
                strError = "警告：利用图书馆应用服务器数据目录内library.xml配置文件中，<libraryserver url='???'>所配置的URL '" + this.LibraryServerDiretory + "' 进行检测性访问的时候，发现它和前端所配置的服务器 URL '' 所指向的不是同一个虚拟目录。" + strSuggestion;
                return 1;
            }

            return 0;
        }
#endif

        delegate void _RefreshCameraDevList();

        /// <summary>
        /// 刷新摄像头设备列表
        /// </summary>
        void RefreshCameraDevList()
        {
            if (this.qrRecognitionControl1 != null)
                this.qrRecognitionControl1.RefreshDevList();
        }

        /// <summary>
        /// 窗口缺省过程函数
        /// </summary>
        /// <param name="m">消息</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == API.WM_SHOWME)
                ShowMe();

            if (m.Msg == API.WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt32() == API.DBT_DEVNODES_CHANGED)
                {
                    _RefreshCameraDevList d = new _RefreshCameraDevList(RefreshCameraDevList);
                    this.BeginInvoke(d);
                }
            }
            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            // get our current "TopMost" value (ours will always be false though)
            bool top = TopMost;
            // make our form jump to the top of everything
            TopMost = true;
            // set it back to whatever it was
            TopMost = top;
        }
#if SN
        void WriteSerialNumberStatusFile()
        {
            try
            {
                string strFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dp2circulation_status");
                if (File.Exists(strFileName) == true)
                    return;
                using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
                {
                    sw.Write(DateTimeUtil.DateTimeToString8(DateTime.Now));
                }

                File.SetAttributes(strFileName, FileAttributes.Hidden);
            }
            catch
            {
            }
        }

        bool IsExistsSerialNumberStatusFile()
        {
            try
            {
                string strFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dp2circulation_status");
                return File.Exists(strFileName);
            }
            catch
            {
            }
            return true;    // 如果出现异常，则当作有此文件
        }

#endif

        void RestoreLastOpenedMdiWindow()
        {
            // 恢复上次遗留的窗口
            string strOpenedMdiWindow = this.AppInfo.GetString(
                "main_form",
                "last_opened_mdi_window",
                "");

            RestoreLastOpenedMdiWindow(strOpenedMdiWindow);
        }

        void RestoreLastOpenedMdiWindow(string strOpenedMdiWindow)
        {
            // 缺省开一个Z search form
            if (String.IsNullOrEmpty(strOpenedMdiWindow) == true)
                strOpenedMdiWindow = "dp2Circulation.QuickChargingForm"; // "dp2Circulation.ChargingForm";

            string[] types = strOpenedMdiWindow.Split(new char[] { ',' });
            for (int i = 0; i < types.Length; i++)
            {
                string strType = types[i];
                if (String.IsNullOrEmpty(strType) == true)
                    continue;

                if (strType == "dp2Circulation.ChargingForm")
                    this.MenuItem_openChargingForm_Click(this, null);
                else if (strType == "dp2Circulation.QuickChargingForm")
                    this.MenuItem_openQuickChargingForm_Click(this, null);
                else if (strType == "dp2Circulation.BiblioSearchForm")
                    this.MenuItem_openBiblioSearchForm_Click(this, null);
                else if (strType == "dp2Circulation.ReaderSearchForm")
                    this.MenuItem_openReaderSearchForm_Click(this, null);
                else if (strType == "dp2Circulation.ItemSearchForm")
                    this.MenuItem_openItemSearchForm_Click(this, null);
                else if (strType == "dp2Circulation.IssueSearchForm")
                    this.MenuItem_openIssueSearchForm_Click(this, null);
                else if (strType == "dp2Circulation.OrderSearchForm")
                    this.MenuItem_openOrderSearchForm_Click(this, null);
                else if (strType == "dp2Circulation.CommentSearchForm")
                    this.MenuItem_openCommentSearchForm_Click(this, null);
                else
                    continue;
            }

            // 装载MDI子窗口状态
            this.AppInfo.LoadFormMdiChildStates(this,
                "mainformstate");
        }

        #region 菜单命令

        // 新开日志统计窗
        private void MenuItem_openOperLogStatisForm_Click(object sender, EventArgs e)
        {
#if NO
            OperLogStatisForm form = new OperLogStatisForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<OperLogStatisForm>();
        }


        private void ToolStripMenuItem_openReportForm_Click(object sender, EventArgs e)
        {
#if NO
            ReportForm form = new ReportForm();
            form.MdiParent = this;
            form.Show();
#endif

#if NO
            string strError = "";
            int nRet = this.VerifySerialCode("report", out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "报表窗需要先设置序列号才能使用");
                return;
            }
#endif

            OpenWindow<ReportForm>();

        }

        // 新开读者统计窗
        private void MenuItem_openReaderStatisForm_Click(object sender, EventArgs e)
        {
#if NO
            ReaderStatisForm form = new ReaderStatisForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<ReaderStatisForm>();

        }

        // 新开册统计窗
        private void MenuItem_openItemStatisForm_Click(object sender, EventArgs e)
        {
#if NO
            ItemStatisForm form = new ItemStatisForm();

            // form.MainForm = this;
            // form.DbType = "item";
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<ItemStatisForm>();

        }

        // 新开书目统计窗
        private void MenuItem_openBiblioStatisForm_Click(object sender, EventArgs e)
        {
#if NO
            BiblioStatisForm form = new BiblioStatisForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<BiblioStatisForm>();

        }

        private void MenuItem_openXmlStatisForm_Click(object sender, EventArgs e)
        {
#if NO
            XmlStatisForm form = new XmlStatisForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<XmlStatisForm>();

        }

        private void MenuItem_openIso2709StatisForm_Click(object sender, EventArgs e)
        {
#if NO
            Iso2709StatisForm form = new Iso2709StatisForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<Iso2709StatisForm>();

        }

        // 新开入馆登记窗
        private void MenuItem_openPassGateForm_Click(object sender, EventArgs e)
        {
#if NO
            PassGateForm form = new PassGateForm();
            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<PassGateForm>();

        }

        // 新开结算窗
        private void MenuItem_openSettlementForm_Click(object sender, EventArgs e)
        {
#if NO
            SettlementForm form = new SettlementForm();

            form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<SettlementForm>();

        }

        // 新开读者窗
        private void MenuItem_openReaderInfoForm_Click(object sender, EventArgs e)
        {
#if NO
            ReaderInfoForm form = new ReaderInfoForm();

            form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<ReaderInfoForm>();
        }

        // 新开出纳打印管理窗
        private void MenuItem_openChargingPrintManageForm_Click(object sender, EventArgs e)
        {
#if NO
            ChargingPrintManageForm form = new ChargingPrintManageForm();

            form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<ChargingPrintManageForm>();
        }

        // 有关MDI子窗口排列的菜单命令
        private void MenuItem_mdi_arrange_Click(object sender, System.EventArgs e)
        {
            // 平铺 水平方式
            if (sender == MenuItem_tileHorizontal)
                this.LayoutMdi(MdiLayout.TileHorizontal);

            if (sender == MenuItem_tileVertical)
                this.LayoutMdi(MdiLayout.TileVertical);

            if (sender == MenuItem_cascade)
                this.LayoutMdi(MdiLayout.Cascade);

            if (sender == MenuItem_arrangeIcons)
                this.LayoutMdi(MdiLayout.ArrangeIcons);

        }

        // 新开出纳窗
        private void MenuItem_openChargingForm_Click(object sender, EventArgs e)
        {
#if NO
            ChargingForm form = new ChargingForm();

            form.MdiParent = this;

            form.MainForm = this;

            form.Show();
#endif
            OpenWindow<ChargingForm>();

        }

        private void MenuItem_openQuickChargingForm_Click(object sender, EventArgs e)
        {
#if NO
            QuickChargingForm form = new QuickChargingForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<QuickChargingForm>();
        }

        // 新开读者查询窗
        private void MenuItem_openReaderSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            ReaderSearchForm form = new ReaderSearchForm();

            form.MdiParent = this;

            // form.MainForm = this;

            form.Show();
#endif
            OpenWindow<ReaderSearchForm>();
        }

        void OpenWindow<T>()
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                T form = Activator.CreateInstance<T>();
                dynamic o = form;
                o.MdiParent = this;

                if (o.MainForm == null)
                {
                    try
                    {
                        o.MainForm = this;
                    }
                    catch
                    {
                        // 等将来所有窗口类型的 MainForm 都是只读的以后，再修改这里
                    }
                }
                o.Show();
            }
            else
                EnsureChildForm<T>(true);
        }

        // 新开实体查询窗
        private void MenuItem_openItemSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            ItemSearchForm form = new ItemSearchForm();
            form.DbType = "item";

            form.MdiParent = this;

            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ItemSearchForm>();
        }

        private void MenuItem_openOrderSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            OrderSearchForm form = new OrderSearchForm();
            // form.DbType = "order";

            form.MdiParent = this;

            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<OrderSearchForm>();
        }

        private void MenuItem_openIssueSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            IssueSearchForm form = new IssueSearchForm();
            // form.DbType = "issue";

            form.MdiParent = this;

            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<IssueSearchForm>();

        }

        private void MenuItem_openCommentSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            CommentSearchForm form = new CommentSearchForm();
            // form.DbType = "comment";

            form.MdiParent = this;

            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<CommentSearchForm>();
        }

        /// <summary>
        /// 打开一个实体/订购/期/评注查询窗
        /// </summary>
        /// <param name="strDbType">数据库类型。为 item/order/issue/comment 之一</param>
        /// <returns>新打开的窗口</returns>
        public ItemSearchForm OpenItemSearchForm(string strDbType)
        {
            ItemSearchForm form = null;

            if (strDbType == "item")
                form = new ItemSearchForm();
            else if (strDbType == "order")
                form = new OrderSearchForm();
            else if (strDbType == "issue")
                form = new IssueSearchForm();
            else if (strDbType == "comment")
                form = new CommentSearchForm();
            else
                form = new ItemSearchForm();

            form.DbType = strDbType;
            form.MdiParent = this;
            form.Show();

            return form;
        }

        private void MenuItem_openInvoiceSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            InvoiceSearchForm form = new InvoiceSearchForm();
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<InvoiceSearchForm>();

        }

        // 新开实体窗
        private void MenuItem_openItemInfoForm_Click(object sender, EventArgs e)
        {
#if NO
            ItemInfoForm form = new ItemInfoForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ItemInfoForm>();

        }

        // 新开时钟窗
        private void MenuItem_openClockForm_Click(object sender, EventArgs e)
        {
#if NO
            ClockForm form = new ClockForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ClockForm>();

        }

        public string MessageUserName
        {
            get
            {
                return this.AppInfo.GetString(
    "message",
    "username",
    "");
            }
        }

        public string MessagePassword
        {
            get
            {
                string strPassword = this.AppInfo.GetString(
        "message",
        "password",
        "");
                return this.DecryptPasssword(strPassword);
            }
        }

        // 系统参数配置
        private void MenuItem_configuration_Click(object sender, EventArgs e)
        {
            _expireVersionChecked = false;

            string strOldDefaultFontString = this.DefaultFontString;
            bool bOldShareBiblio = false;
            string strOldDp2MserverUrl = "";

            if (this.MessageHub != null)
            {
                bOldShareBiblio = this.MessageHub.ShareBiblio;
                strOldDp2MserverUrl = this.MessageHub.dp2MServerUrl;
            }

            string strOldMessageUserName = this.MessageUserName;
            string strOldMessagePassword = this.MessagePassword;

            CfgDlg dlg = new CfgDlg();

            dlg.ParamChanged += new ParamChangedEventHandler(CfgDlg_ParamChanged);
            dlg.ap = this.AppInfo;
            dlg.MainForm = this;

            dlg.UiState = this.AppInfo.GetString(
                    "main_form",
                    "cfgdlg_uiState",
                    "");
            this.AppInfo.LinkFormState(dlg,
                "cfgdlg_state");
            dlg.ShowDialog(this);
            // this.AppInfo.UnlinkFormState(dlg);
            this.AppInfo.SetString(
                    "main_form",
                    "cfgdlg_uiState",
                    dlg.UiState);

            dlg.ParamChanged -= new ParamChangedEventHandler(CfgDlg_ParamChanged);

            // 缺省字体发生了变化
            if (strOldDefaultFontString != this.DefaultFontString)
            {
                Size oldsize = this.Size;

                MainForm.SetControlFont(this, this.DefaultFont, true);

                /*
                if (this.WindowState == FormWindowState.Normal)
                    this.Size = oldsize;
                 * */

                foreach (Form child in this.MdiChildren)
                {
                    oldsize = child.Size;

                    MainForm.SetControlFont(child, this.DefaultFont, true);

                    // child.Size = oldsize;
                }
            }

            if (this.MessageHub != null
                && (bOldShareBiblio != this.MessageHub.ShareBiblio
                || strOldDp2MserverUrl != this.MessageHub.dp2MServerUrl
                || strOldMessageUserName != this.MessageUserName
                || strOldMessagePassword != this.MessagePassword))
            {
                // URL 变化，需要先关闭然后重新连接
                if (strOldDp2MserverUrl != this.MessageHub.dp2MServerUrl
                    || strOldMessageUserName != this.MessageUserName
                    || strOldMessagePassword != this.MessagePassword)
                    this.MessageHub.CloseConnection();

                // TODO: 如果没有 Connect，要先 Connect
                this.MessageHub.RefreshUserName();
                this.MessageHub.Connect();
#if NO
                if (bOldShareBiblio != this.MessageHub.ShareBiblio)
                    this.MessageHub.Login();    // 重新登录
#endif
            }
        }

        void CfgDlg_ParamChanged(object sender, ParamChangedEventArgs e)
        {
            if (e.Section == "charging_form"
                && e.Entry == "no_biblio_and_item_info")
            {
                // 遍历当前打开的所有chargingform
                List<Form> forms = GetChildWindows(typeof(ChargingForm));
                foreach (Form child in forms)
                {
                    ChargingForm chargingform = (ChargingForm)child;

                    chargingform.ClearItemAndBiblioControl();
                    chargingform.ChangeLayout((bool)e.Value);
                }
            }

        }

        // 退出
        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 新开修改密码窗
        private void MenuItem_openChangePasswordForm_Click(object sender, EventArgs e)
        {
#if NO
            ChangePasswordForm form = new ChangePasswordForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ChangePasswordForm>();
        }

        private void MenuItem_openAmerceForm_Click(object sender, EventArgs e)
        {
#if NO
            AmerceForm form = new AmerceForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<AmerceForm>();
        }

        private void MenuItem_openReaderManageForm_Click(object sender, EventArgs e)
        {
#if NO
            ReaderManageForm form = new ReaderManageForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ReaderManageForm>();
        }

        private void MenuItem_openBiblioSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            BiblioSearchForm form = new BiblioSearchForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<BiblioSearchForm>();
        }

        private void MenuItem_openEntityForm_Click(object sender, EventArgs e)
        {
#if NO
            EntityForm form = new EntityForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<EntityForm>();

        }

        // 批修改册
        private void MenuItem_openQuickChangeEntityForm_Click(object sender, EventArgs e)
        {
#if NO
            QuickChangeEntityForm form = new QuickChangeEntityForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<QuickChangeEntityForm>();

        }

        // 批修改书目
        private void MenuItem_openQuickChangeBiblioForm_Click(object sender, EventArgs e)
        {
#if NO
            QuickChangeBiblioForm form = new QuickChangeBiblioForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<QuickChangeBiblioForm>();

        }

        private void MenuItem_openOperLogForm_Click(object sender, EventArgs e)
        {
#if NO
            OperLogForm form = new OperLogForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<OperLogForm>();

        }

        private void MenuItem_openCalendarForm_Click(object sender, EventArgs e)
        {
#if NO
            CalendarForm form = new CalendarForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<CalendarForm>();
        }

        private void MenuItem_openBatchTaskForm_Click(object sender, EventArgs e)
        {
#if NO
            BatchTaskForm form = new BatchTaskForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<BatchTaskForm>();

        }

        private void MenuItem_openManagerForm_Click(object sender, EventArgs e)
        {
#if NO
            ManagerForm form = new ManagerForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ManagerForm>();

        }

        private void MenuItem_openUserForm_Click(object sender, EventArgs e)
        {
#if NO
            UserForm form = new UserForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<UserForm>();

        }

        private void MenuItem_channelForm_Click(object sender, EventArgs e)
        {
#if NO
            ChannelForm form = new ChannelForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ChannelForm>();

        }

        private void MenuItem_openActivateForm_Click(object sender, EventArgs e)
        {
#if NO
            ActivateForm form = new ActivateForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ActivateForm>();

        }

        private void MenuItem_openTestForm_Click(object sender, EventArgs e)
        {
#if NO
            TestForm form = new TestForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<TestForm>();

        }

        // 打开种次号窗
        private void MenuItem_openZhongcihaoForm_Click(object sender, EventArgs e)
        {
#if NO
            ZhongcihaoForm form = new ZhongcihaoForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ZhongcihaoForm>();

        }

        // 打开索取号窗
        private void MenuItem_openCallNumberForm_Click(object sender, EventArgs e)
        {
#if NO
            CallNumberForm form = new CallNumberForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<CallNumberForm>();

        }

        private void MenuItem_openUrgentChargingForm_Click(object sender, EventArgs e)
        {
#if NO
            UrgentChargingForm form = new UrgentChargingForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<UrgentChargingForm>();

        }

        // 应急恢复
        private void MenuItem_recoverUrgentLog_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is UrgentChargingForm)
            {
                ((UrgentChargingForm)this.ActiveMdiChild).Recover();
            }
        }

#if NO
        // 登出
        private void MenuItem_logout_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is EntityForm)
            {
                ((EntityForm)this.ActiveMdiChild).Logout();
            }
        }
#endif

        // 打开数据目录文件夹
        private void MenuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 打开程序目录文件夹
        private void MenuItem_openProgramFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_operCheckBorrowInfoForm_Click(object sender, EventArgs e)
        {
#if NO
            CheckBorrowInfoForm form = new CheckBorrowInfoForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<CheckBorrowInfoForm>();

        }

        // 典藏交接 hand over and take over
        private void MenuItem_handover_Click(object sender, EventArgs e)
        {
#if NO
            ItemHandoverForm form = new ItemHandoverForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ItemHandoverForm>();

        }

        // 采购 打印订单
        private void MenuItem_printOrder_Click(object sender, EventArgs e)
        {
#if NO
            PrintOrderForm form = new PrintOrderForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<PrintOrderForm>();

        }

        // 菜单 打印验收单
        private void MenuItem_printAccept_Click(object sender, EventArgs e)
        {
#if NO
            PrintAcceptForm form = new PrintAcceptForm();
            form.MdiParent = this;
            // form.MainForm = this;
            form.Show();
#endif
            OpenWindow<PrintAcceptForm>();


        }

        // 打印催询单
        private void MenuItem_printClaim_Click(object sender, EventArgs e)
        {
#if NO
            PrintClaimForm form = new PrintClaimForm();
            form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<PrintClaimForm>();

        }

        // 打印财产帐
        private void MenuItem_printAccountBook_Click(object sender, EventArgs e)
        {
#if NO
            AccountBookForm form = new AccountBookForm();
            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<AccountBookForm>();

        }

        // 打印装订单
        private void MenuItem_printBindingList_Click(object sender, EventArgs e)
        {
#if NO
            PrintBindingForm form = new PrintBindingForm();
            form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<PrintBindingForm>();

        }

#if NO
        MdiClient GetMdiClient()
        {
            Type t = typeof(Form);
            PropertyInfo pi = t.GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic);
            return (MdiClient)pi.GetValue(this, null);
        }
#endif

#if !ACCEPT_MODE
        AcceptForm _acceptForm = null;
#endif

        private void MenuItem_accept_Click(object sender, EventArgs e)
        {
#if ACCEPT_MODE

            AcceptForm top = this.GetTopChildWindow<AcceptForm>();
            if (top != null)
            {
                top.Activate();
                return;
            }

            AcceptForm form = new AcceptForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();

            // form.WaitLoadFinish();

            // API.PostMessage(this.Handle, WM_REFRESH_MDICLIENT, 0, 0);

            {
                this.MdiClient.Invalidate();
                this.MdiClient.Update();

                // TODO: Invalidate 全部打开的MDI子窗口
                for (int i = 0; i < this.MdiChildren.Length; i++)
                {
                    Global.InvalidateAllControls(this.MdiChildren[i]);
                }
            }
#else
            if (_acceptForm == null || _acceptForm.IsDisposed == true)
            {
                _acceptForm = new AcceptForm();
                _acceptForm.MainForm = this;
                _acceptForm.FormClosed -= new FormClosedEventHandler(accept_FormClosed);
                _acceptForm.FormClosed += new FormClosedEventHandler(accept_FormClosed);

                this.AppInfo.LinkFormState(_acceptForm, "acceptform_state");

                _acceptForm.Show(this);
            }
            else
            {
                _acceptForm.ActivateFirstPage();
            }

            if (Control.ModifierKeys == Keys.Control)
            {
                if (_acceptForm.Visible == false)
                {
                    _acceptForm.DoFloating();
                    // _acceptForm.Show(this);
                }
            }
            else
            {
                if (this.CurrentPropertyControl != _acceptForm.MainControl)
                    _acceptForm.DoDock(true); // 自动显示FixedPanel
            }

#endif
        }

        void accept_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_acceptForm != null)
            {
                if (this.AppInfo != null)
                    this.AppInfo.UnlinkFormState(_acceptForm);
                this._acceptForm = null;
            }
        }

        // 清除配置文件本地缓存
        private void MenuItem_clearCfgCache_Click(object sender, EventArgs e)
        {
            cfgCache.ClearCfgCache();

            this.AssemblyCache.Clear(); // 顺便也清除Assembly缓存
            this.DomCache.Clear();
        }

        // 清除书目摘要本地缓存
        private void MenuItem_clearSummaryCache_Click(object sender, EventArgs e)
        {
            this.SummaryCache.RemoveAll();
        }

        // 设置字体
        private void MenuItem_font_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is EntityForm)
            {
                ((EntityForm)this.ActiveMdiChild).SetFont();
            }
            else if (this.ActiveMdiChild is MyForm)
            {
                ((MyForm)this.ActiveMdiChild).SetBaseFont();
            }
        }

        // 恢复为缺省字体
        private void MenuItem_restoreDefaultFont_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is EntityForm)
            {
                ((EntityForm)this.ActiveMdiChild).RestoreDefaultFont();
            }
            else if (this.ActiveMdiChild is MyForm)
            {
                ((MyForm)this.ActiveMdiChild).RestoreDefaultFont();
            }
        }

        #endregion

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
                stopManager.DoStopAll(null);    // 2012/3/25
            else
                stopManager.DoStopActive();
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == null)
            {
                SetMenuItemState();
            }
        }

        internal void SetMenuItemState()
        {
            // 菜单

            // 工具条按钮
            this.ToolStripMenuItem_loadReaderInfo.Enabled = true;
            this.ToolStripMenuItem_loadItemInfo.Enabled = true;
            this.ToolStripMenuItem_autoLoadItemOrReader.Enabled = true;

            this.MenuItem_recoverUrgentLog.Enabled = false;
            this.MenuItem_font.Enabled = false;
            this.MenuItem_restoreDefaultFont.Enabled = false;
            this.MenuItem_logout.Enabled = false;
        }

#if NO

        // 当前顶层的ReaderInfoForm
        public ReaderInfoForm TopReaderInfoForm
        {
            get
            {
                return (ReaderInfoForm)GetTopChildWindow(typeof(ReaderInfoForm));
            }

        }

        // 当前顶层的AcceptForm
        public AcceptForm TopAcceptForm
        {
            get
            {
                return GetTopChildWindow<AcceptForm>();
            }
        }


        public ActivateForm TopActivateForm
        {
            get
            {
                return (ActivateForm)GetTopChildWindow(typeof(ActivateForm));
            }
        }

        public EntityForm TopEntityForm
        {
            get
            {
                return (EntityForm)GetTopChildWindow(typeof(EntityForm));
            }
        }

        public DupForm TopDupForm
        {
            get
            {
                return (DupForm)GetTopChildWindow(typeof(DupForm));
            }
        }

        // 当前顶层的ItemInfoForm
        public ItemInfoForm TopItemInfoForm
        {
            get
            {
                return (ItemInfoForm)GetTopChildWindow(typeof(ItemInfoForm));
            }
        }

        public UtilityForm TopUtilityForm
        {
            get
            {
                return (UtilityForm)GetTopChildWindow(typeof(UtilityForm));
            }
        }

        // 当前顶层的ChargingForm
        public ChargingForm TopChargingForm
        {
            get
            {
                return (ChargingForm)GetTopChildWindow(typeof(ChargingForm));
            }
        }

        // 当前顶层的ChargingForm
        public UrgentChargingForm TopUrgentChargingForm
        {
            get
            {
                return (UrgentChargingForm)GetTopChildWindow(typeof(UrgentChargingForm));
            }
        }

        // 当前顶层的HtmlPrintForm
        public HtmlPrintForm TopHtmlPrintForm
        {
            get
            {
                return (HtmlPrintForm)GetTopChildWindow(typeof(HtmlPrintForm));
            }
        }


        // 当前顶层的ChargingPrintManageForm
        public ChargingPrintManageForm TopChargingPrintManageForm
        {
            get
            {
                return (ChargingPrintManageForm)GetTopChildWindow(typeof(ChargingPrintManageForm));
            }
        }

        // 当前顶层的AmerceForm
        public AmerceForm TopAmerceForm
        {
            get
            {
                return (AmerceForm)GetTopChildWindow(typeof(AmerceForm));
            }
        }

        // 当前顶层的ReaderManageForm
        public ReaderManageForm TopReaderManageForm
        {
            get
            {
                return (ReaderManageForm)GetTopChildWindow(typeof(ReaderManageForm));
            }
        }

        // 当前顶层的PrintAcceptForm
        public PrintAcceptForm TopPrintAcceptForm
        {
            get
            {
                return (PrintAcceptForm)GetTopChildWindow(typeof(PrintAcceptForm));
            }
        }

        // 当前顶层的LabelPrintForm
        public LabelPrintForm TopLabelPrintForm
        {
            get
            {
                return (LabelPrintForm)GetTopChildWindow(typeof(LabelPrintForm));
            }
        }

        // 当前顶层的CardPrintForm
        public CardPrintForm TopCardPrintForm
        {
            get
            {
                return (CardPrintForm)GetTopChildWindow(typeof(CardPrintForm));
            }
        }

        // 当前顶层的BiblioStatisForm
        public BiblioStatisForm TopBiblioStatisForm
        {
            get
            {
                return (BiblioStatisForm)GetTopChildWindow(typeof(BiblioStatisForm));
            }
        }

#endif

        // 得到特定类型的MDI窗口
        List<Form> GetChildWindows(Type type)
        {
            List<Form> results = new List<Form>();

            foreach (Form child in this.MdiChildren)
            {
                if (child.GetType().Equals(type) == true)
                    results.Add(child);
            }

            return results;
        }

        /// <summary>
        /// 获得一个已经打开的 MDI 子窗口，如果没有，则新打开一个
        /// </summary>
        /// <typeparam name="T">子窗口类型</typeparam>
        /// <returns>子窗口对象</returns>
        public T EnsureChildForm<T>(bool bActivate = false)
        {
            T form = GetTopChildWindow<T>();
            if (form == null)
            {
                form = Activator.CreateInstance<T>();
                dynamic o = form;
                o.MdiParent = this;

                // 2013/3/26
                if (o.MainForm == null)
                {
                    try
                    {
                        o.MainForm = this;
                    }
                    catch
                    {
                        // 等将来所有窗口类型的 MainForm 都是只读的以后，再修改这里
                    }
                }
                o.Show();
            }
            else
            {
                if (bActivate == true)
                {
                    try
                    {
                        dynamic o = form;
                        o.Activate();

                        if (o.WindowState == FormWindowState.Minimized)
                            o.WindowState = FormWindowState.Normal;
                    }
                    catch
                    {
                    }
                }
            }
            return form;
        }

        // 
        /// <summary>
        /// 得到特定类型的顶层 MDI 子窗口
        /// </summary>
        /// <typeparam name="T">子窗口类型</typeparam>
        /// <returns>子窗口对象</returns>
        public T GetTopChildWindow<T>()
        {
            if (ActiveMdiChild == null)
                return default(T);

            // 得到顶层的MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return default(T);

            while (hwnd != IntPtr.Zero)
            {
                Form child = null;
                // 判断一个窗口句柄，是否为 MDI 子窗口？
                // return:
                //      null    不是 MDI 子窗口o
                //      其他      返回这个句柄对应的 Form 对象
                child = IsChildHwnd(hwnd);
                if (child != null)
                {
                    // if (child is T)
                    if (child.GetType().Equals(typeof(T)) == true)
                    {
                        try
                        {
                            return (T)Convert.ChangeType(child, typeof(T));
                        }
                        catch (InvalidCastException ex)
                        {
                            throw new InvalidCastException("在将类型 '" + child.GetType().ToString() + "' 转换为类型 '" + typeof(T).ToString() + "' 的过程中出现异常: " + ex.Message, ex);
                        }
                    }
                }

                hwnd = API.GetWindow(hwnd, API.GW_HWNDNEXT);
            }

            return default(T);
        }

        // 判断一个窗口句柄，是否为 MDI 子窗口？
        // return:
        //      null    不是 MDI 子窗口o
        //      其他      返回这个句柄对应的 Form 对象
        Form IsChildHwnd(IntPtr hwnd)
        {
            foreach (Form child in this.MdiChildren)
            {
                if (hwnd == child.Handle)
                    return child;
            }

            return null;
        }

        // 得到特定类型的顶层MDI窗口
        Form GetTopChildWindow(Type type)
        {
            if (ActiveMdiChild == null)
                return null;

            // 得到顶层的MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return null;

            for (; ; )
            {
                if (hwnd == IntPtr.Zero)
                    break;

                Form child = null;
                for (int j = 0; j < this.MdiChildren.Length; j++)
                {
                    if (hwnd == this.MdiChildren[j].Handle)
                    {
                        child = this.MdiChildren[j];
                        goto FOUND;
                    }
                }

                goto CONTINUE;
            FOUND:

                if (child.GetType().Equals(type) == true)
                    return child;

            CONTINUE:
                hwnd = API.GetWindow(hwnd, API.GW_HWNDNEXT);
            }

            return null;
        }

        // 得到顶层MDI窗口
        // 注： this.ActiveMdiChild 不一定在最顶层
        Form GetTopChildWindow()
        {
            if (this.MdiChildren.Length == 0)
                return null;
            List<IntPtr> handles = new List<IntPtr>();
            foreach (Form mdi in this.MdiChildren)
            {
                handles.Add(mdi.Handle);
            }

            IntPtr hwnd = this.ActiveMdiChild.Handle;
            IntPtr top = hwnd;
            while (hwnd != IntPtr.Zero)
            {
                if (handles.IndexOf(hwnd) != -1)
                    top = hwnd;

                hwnd = API.GetWindow(hwnd, API.GW_HWNDPREV);
            }

            foreach (Form mdi in this.MdiChildren)
            {
                if (mdi.Handle == top)
                    return mdi;
            }

            return null;
        }
#if NO
        // 得到顶层MDI窗口
        // 注： this.ActiveMdiChild 不一定在最顶层
        Form GetTopChildWindow()
        {
            if (ActiveMdiChild == null)
                return null;

            // 得到顶层的MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;
            if (hwnd == IntPtr.Zero)
                return null;

            for (; ; )
            {
                if (hwnd == IntPtr.Zero)
                    break;

                Form child = null;
                for (int j = 0; j < this.MdiChildren.Length; j++)
                {
                    if (hwnd == this.MdiChildren[j].Handle)
                    {
                        child = this.MdiChildren[j];
                        return child;
                    }
                }

                hwnd = API.GetWindow(hwnd, API.GW_HWNDPREV);
            }

            return this.ActiveMdiChild;
        }

#endif

        // 是否为MDI子窗口中最顶层的2个之一?
        // 2008/9/8
        internal bool IsTopTwoChildWindow(Form form)
        {
            if (ActiveMdiChild == null)
                return false;

            // 得到顶层的MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return false;

            for (int i = 0; i < 2; i++)
            {
                if (hwnd == IntPtr.Zero)
                    break;

                if (hwnd == form.Handle)
                {
                    return true;
                }

                hwnd = API.GetWindow(hwnd, API.GW_HWNDNEXT);
            }

            return false;
        }

        // 根据条码装载读者记录
        private void ToolStripMenuItem_loadReaderInfo_Click(object sender, EventArgs e)
        {
            this.ToolStripMenuItem_loadReaderInfo.Checked = true;
            this.ToolStripMenuItem_loadItemInfo.Checked = false;
            this.ToolStripMenuItem_autoLoadItemOrReader.Checked = false;
            this.toolStripButton_loadBarcode.Text = "读者";
        }

        // 根据条码装载实体记录
        private void ToolStripMenuItem_loadItemInfo_Click(object sender, EventArgs e)
        {
            this.ToolStripMenuItem_loadReaderInfo.Checked = false;
            this.ToolStripMenuItem_loadItemInfo.Checked = true;
            this.ToolStripMenuItem_autoLoadItemOrReader.Checked = false;
            this.toolStripButton_loadBarcode.Text = "册";
        }

        private void ToolStripMenuItem_autoLoadItemOrReader_Click(object sender, EventArgs e)
        {
            this.ToolStripMenuItem_loadReaderInfo.Checked = false;
            this.ToolStripMenuItem_loadItemInfo.Checked = false;
            this.ToolStripMenuItem_autoLoadItemOrReader.Checked = true;
            this.toolStripButton_loadBarcode.Text = "自动";
        }

        bool _expireVersionChecked = false;

        internal void Channel_BeforeLogin(object sender,
            DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
#if SN
            if (_expireVersionChecked == false)
            {
                string base_version = "2.36";
                string strExpire = GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false
                    && StringUtil.CompareVersion(this.ServerVersion, base_version) < 0
                    && this.ServerVersion != "0.0")
                {
                    string strError = "具有失效序列号参数的 dp2Circulation 需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + this.ServerVersion.ToString() + " )。\r\n\r\n请升级 dp2Library 到最新版本，然后重新启动 dp2Circulation。\r\n\r\n点“确定”按钮退出";
                    Program.PromptAndExit(this, strError);
                    e.Cancel = true;
                    return;
                }
                _expireVersionChecked = true;
            }
#endif

#if NO
            {
                double base_version = 2.60;

                if (this.ServerVersion < base_version
                    && this.ServerVersion != 0)
                {
                    string strError = "dp2 前端所连接的 dp2library 版本必须升级为 " + base_version + " 以上时才能使用 (当前 dp2library 版本为 " + this.ServerVersion.ToString() + ")\r\n\r\n注：升级服务器的操作非常容易：\r\n1) 若是 dp2 标准版，请系统管理员在服务器机器上，运行 dp2installer(dp2服务器安装工具) 即可。这个模块的安装页面是 http://dp2003.com/dp2installer/v1/publish.htm 。\r\n2) 若是单机版或小型版，反复重启 dp2libraryxe 模块多次即可自动升级。\r\n\r\n亲，若有任何问题，请及时联系数字平台哟 ~";
                    Program.PromptAndExit(this, strError);
                    e.Cancel = true;
                    return;
                }
            }
#endif
            if (e.FirstTry == true)
            {
                string strPhoneNumber = "";

                {
                    e.UserName = AppInfo.GetString(
                        "default_account",
                        "username",
                        "");
                    e.Password = AppInfo.GetString(
                        "default_account",
                        "password",
                        "");
                    e.Password = this.DecryptPasssword(e.Password);

                    strPhoneNumber = AppInfo.GetString(
        "default_account",
        "phoneNumber",
        "");

                    bool bIsReader =
                        AppInfo.GetBoolean(
                        "default_account",
                        "isreader",
                        false);

                    string strLocation = AppInfo.GetString(
                    "default_account",
                    "location",
                    "");
                    e.Parameters = "location=" + strLocation;
                    if (bIsReader == true)
                        e.Parameters += ",type=reader";
                }

                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

                // 2014/10/23
                if (this.TestMode == true)
                    e.Parameters += ",testmode=true";

                e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;

                // 以手机短信验证方式登录
                if (string.IsNullOrEmpty(strPhoneNumber) == false)
                    e.Parameters += ",phoneNumber=" + strPhoneNumber;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = null;

            if (sender is IWin32Window)
                owner = (IWin32Window)sender;
            else
                owner = this;

            CirculationLoginDlg dlg = SetDefaultAccount(
                e.LibraryServerUrl,
                null,
                e.ErrorInfo,
                e.LoginFailCondition,
                owner);
            if (dlg == null)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = dlg.SavePasswordShort;
            e.Parameters = "location=" + dlg.OperLocation;
            if (dlg.IsReader == true)
                e.Parameters += ",type=reader";

            // 2014/9/13
            e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
            // 从序列号中获得 expire= 参数值
            {
                string strExpire = GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
            }
#endif

            // 2014/10/23
            if (this.TestMode == true)
                e.Parameters += ",testmode=true";

            e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;

            if (string.IsNullOrEmpty(dlg.TempCode) == false)
                e.Parameters += ",tempCode=" + dlg.TempCode;

            // 以手机短信验证方式登录
            if (string.IsNullOrEmpty(dlg.PhoneNumber) == false)
                e.Parameters += ",phoneNumber=" + dlg.PhoneNumber;

            e.SavePasswordLong = dlg.SavePasswordLong;
            if (e.LibraryServerUrl != dlg.ServerUrl)
            {
                e.LibraryServerUrl = dlg.ServerUrl;
                _expireVersionChecked = false;
            }
        }

        internal string _currentUserName = "";
        internal string _currentUserRights = "";
        internal string _currentLibraryCodeList = "";

        internal void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
#if SN
            LibraryChannel channel = sender as LibraryChannel;
            _currentUserName = channel.UserName;
            _currentUserRights = channel.Rights;
            _currentLibraryCodeList = channel.LibraryCodeList;

            if (_verified == false && StringUtil.IsInList("serverlicensed", channel.Rights) == false)
            {
                string strError = "";
                int nRet = this.VerifySerialCode("", true, out strError);
                if (nRet == -1)
                {
                    channel.Close();
                    Program.PromptAndExit(this, "dp2Circulation 专业版需要先设置序列号才能使用。\r\n\r\n注：可以切换为社区版，不需要设置序列号即可使用。方法是：在设置序列号对话框中，按左下角的“切换为社区版”按钮。");
                    return;
                }
            }
            _verified = true;
#endif
        }

#if SN
        bool _verified = false;

        // 从序列号中获得 expire= 参数值
        // 参数值为 MAC 地址的列表，中间分隔以 '|'
        internal string GetExpireParam()
        {
            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
            if (string.IsNullOrEmpty(strSerialCode) == false)
            {
                string strExtParam = GetExtParams(strSerialCode);
                if (string.IsNullOrEmpty(strExtParam) == false)
                {
                    Hashtable ext_table = StringUtil.ParseParameters(strExtParam);
                    return (string)ext_table["expire"];
                }
            }

            return "";
        }

#endif

        /// <summary>
        /// 获得缺省用户名
        /// </summary>
        public string DefaultUserName
        {
            get
            {
                return AppInfo.GetString(
                "default_account",
                "username",
                "");
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.GUI)
        {
            if (EntityRegisterBase.IsDot(strServerUrl) == true)
                strServerUrl = this.LibraryServerUrl;
            if (EntityRegisterBase.IsDot(strUserName) == true)
                strUserName = this.DefaultUserName;

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);
            if ((style & GetChannelStyle.GUI) != 0)
                channel.Idle += channel_Idle;
            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        void channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            channel.Idle -= channel_Idle;

            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
        }

#if NO
        int _inPrepareSearch = 0;   // 进入的次数，用以防止嵌套调用

        // return:
        //      0   没有准备成功
        //      1   准备成功
        /// <summary>
        /// 准备进行检索
        /// </summary>
        /// <param name="bActivateStop">是否激活 Stop</param>
        /// <returns>0: 没有成功; 1: 成功</returns>
        public int PrepareSearch(bool bActivateStop = true)
        {
            if (String.IsNullOrEmpty(this.LibraryServerUrl) == true)
                return 0;   // 调用者在看到返回 0 以后，不应再调用 EndSearch() 了

            this._inPrepareSearch++;

            if (this._inPrepareSearch == 1)
            {
                this.Channel.Url = this.LibraryServerUrl;

                this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);

                this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

                Stop = new DigitalPlatform.Stop();
                Stop.Register(stopManager, bActivateStop);	// 和容器关联
            }

            return 1;
        }

        /// <summary>
        /// 结束检索
        /// </summary>
        /// <returns>返回 0</returns>
        public int EndSearch(bool bActivateStop = true)
        {
            if (this._inPrepareSearch == 1)
            {
                if (Stop != null) // 脱离关联
                {
                    Stop.Unregister(bActivateStop);	// 和容器关联
                    Stop = null;
                }

                this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
                this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            }

            this._inPrepareSearch--;

            return 0;
        }
#endif

        // 登出
        /// <summary>
        /// MainForm 登出
        /// </summary>
        public void Logout()
        {
#if NO
            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                return;  // 2009/2/11
            }
#endif
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在登出 ...");
            Stop.BeginLoop();

            try
            {
                // string strValue = "";
                long lRet = channel.Logout(out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 登出时发生错误：" + strError;
                    goto ERROR1;
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                // EndSearch();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 获得配置文件
        // 会用到 cfgCache
        // return:
        //      -1  出错或者没有找到
        //      1   找到
        /// <summary>
        /// 获得配置文件。用到了配置文件缓存
        /// </summary>
        /// <param name="Channel">通讯通道</param>
        /// <param name="stop">停止对象</param>
        /// <param name="strDbName">数据库名</param>
        /// <param name="strCfgFileName">配置文件名</param>
        /// <param name="remote_timestamp">远端时间戳。如果为 null，表示要从服务器实际获取时间戳</param>
        /// <param name="strContent">返回配置文件内容</param>
        /// <param name="baOutputTimestamp">返回配置文件时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错或没有找到; 1: 找到</returns>
        public int GetCfgFile(
            LibraryChannel Channel,
            Stop stop,
            string strDbName,
            string strCfgFileName,
            byte[] remote_timestamp,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strContent = "";

            /*
            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }*/

            /*
            if (stop != null)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载配置文件 ...");
                stop.BeginLoop();
            }*/

            // m_nInGetCfgFile++;

            try
            {
                string strPath = strDbName + "/cfgs/" + strCfgFileName;

                stop.SetMessage("正在下载配置文件 " + strPath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = Channel.GetRes(stop,
                    this.cfgCache,
                    strPath,
                    strStyle,
                    remote_timestamp,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                /*
                if (stop != null)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }*/

                // m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }

#if NO
        // 此函数希望逐渐废止
        // 获得关于一个特定馆藏地点的索取号配置信息
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 获得关于一个特定馆藏地点的索取号配置信息。
        /// 本函数将废止
        /// </summary>
        /// <param name="strLocation">馆藏地点</param>
        /// <param name="strArrangeGroupName">返回排架体系名</param>
        /// <param name="strZhongcihaoDbname">返回种次号库名</param>
        /// <param name="strClassType">返回分来号类型</param>
        /// <param name="strQufenhaoType">返回区分号类型</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:  出错</para>
        /// <para>0:   没有找到</para>
        /// <para>1:   找到</para>
        /// </returns>
        public int GetCallNumberInfo(string strLocation,
            out string strArrangeGroupName,
            out string strZhongcihaoDbname,
            out string strClassType,
            out string strQufenhaoType,
            out string strError)
        {
            strError = "";
            strArrangeGroupName = "";
            strZhongcihaoDbname = "";
            strClassType = "";
            strQufenhaoType = "";

            if (this.CallNumberCfgDom == null)
                return 0;

            if (this.CallNumberCfgDom.DocumentElement == null)
                return 0;

            XmlNode node = this.CallNumberCfgDom.DocumentElement.SelectSingleNode("group/location[@name='" + strLocation + "']");
            if (node == null)
            {
                // 2014/2/13
                XmlNodeList nodes = this.CallNumberCfgDom.DocumentElement.SelectNodes("group/location");
                if (nodes.Count == 0)
                    return 0;
                foreach (XmlNode current in nodes)
                {
                    string strPattern = DomUtil.GetAttr(current, "name");
                    if (LibraryServerUtil.MatchLocationName(strLocation, strPattern) == true)
                    {
                        node = current;
                        goto END1;
                    }
                }
                return 0;
            }

            END1:
            XmlNode nodeGroup = node.ParentNode;
            strArrangeGroupName = DomUtil.GetAttr(nodeGroup, "name");
            strZhongcihaoDbname = DomUtil.GetAttr(nodeGroup, "zhongcihaodb");
            strClassType = DomUtil.GetAttr(nodeGroup, "classType");
            strQufenhaoType = DomUtil.GetAttr(nodeGroup, "qufenhaoType");

            return 1;
        }

#endif

        // 此函数希望逐渐废止
        // 获得关于一个特定馆藏地点的索取号配置信息
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 获得关于一个特定馆藏地点的索取号配置信息。
        /// 本函数将废止
        /// </summary>
        /// <param name="strLocation">馆藏地点</param>
        /// <param name="strArrangeGroupName">返回排架体系名</param>
        /// <param name="strZhongcihaoDbname">返回种次号库名</param>
        /// <param name="strClassType">返回分来号类型</param>
        /// <param name="strQufenhaoType">返回区分号类型</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:  出错</para>
        /// <para>0:   没有找到</para>
        /// <para>1:   找到</para>
        /// </returns>
        public int GetCallNumberInfo(string strLocation,
            out string strArrangeGroupName,
            out string strZhongcihaoDbname,
            out string strClassType,
            out string strQufenhaoType,
            out string strError)
        {
            strError = "";
            strArrangeGroupName = "";
            strZhongcihaoDbname = "";
            strClassType = "";
            strQufenhaoType = "";

            ArrangementInfo info = null;
            int nRet = GetArrangementInfo(strLocation,
                out info,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            strArrangeGroupName = info.ArrangeGroupName;
            strZhongcihaoDbname = info.ZhongcihaoDbname;
            strClassType = info.ClassType;
            strQufenhaoType = info.QufenhaoType;
            return nRet;
        }

        // 注意排架体系定义中的 location 元素 name 值，可能含有通配符
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 获得关于一个特定馆藏地点的索取号配置信息
        /// </summary>
        /// <param name="strLocation">馆藏地点字符串</param>
        /// <param name="info">返回索取号配置信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetArrangementInfo(string strLocation,
            out ArrangementInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            if (this.CallNumberCfgDom == null)
                return 0;

            if (this.CallNumberCfgDom.DocumentElement == null)
                return 0;

            XmlNode node = this.CallNumberCfgDom.DocumentElement.SelectSingleNode("group/location[@name='" + strLocation + "']");
            if (node == null)
            {
                XmlNodeList nodes = this.CallNumberCfgDom.DocumentElement.SelectNodes("group/location");
                if (nodes.Count == 0)
                    return 0;
                foreach (XmlNode current in nodes)
                {
                    string strPattern = DomUtil.GetAttr(current, "name");
                    if (LibraryServerUtil.MatchLocationName(strLocation, strPattern) == true)
                    {
                        info = new ArrangementInfo();
                        info.Fill(current.ParentNode);
                        return 1;
                    }
                }

                return 0;
            }

            info = new ArrangementInfo();
            XmlNode nodeGroup = node.ParentNode;
            info.Fill(nodeGroup);
            return 1;
        }

#if NO
        // 获得读者库名列表
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        public int GetReaderDbNames()
        {
            int nRet = PrepareSearch();
            if (nRet == 0)
                return -1;

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得读者库名列表 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "reader",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得读者库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.ReaderDbNames = strValue.Split(new char[] { ',' });
                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否要继续?",
                "dp2Circulation",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
        if (result == DialogResult.OK)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }
#endif


        /// <summary>
        /// 获得一个数据库的栏目属性集合
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>属性集合</returns>
        public ColumnPropertyCollection GetBrowseColumnProperties(string strDbName)
        {
            // ColumnPropertyCollection results = new ColumnPropertyCollection();
            Debug.Assert(this.NormalDbProperties != null, "this.NormalDbProperties == null");
            if (this.NormalDbProperties == null)
                return null;    // 2014/12/22
            for (int i = 0; i < this.NormalDbProperties.Count; i++)
            {
                NormalDbProperty prop = this.NormalDbProperties[i];
                Debug.Assert(prop != null, "prop == null");
                if (prop == null)
                    continue;    // 2014/12/22
                if (prop.DbName == strDbName)
                    return prop.ColumnProperties;
            }

            return null;    // not found
        }

#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif
        List<LibraryChannel> _channelList = new List<LibraryChannel>();
        public void DoStop(object sender, StopEventArgs e)
        {
            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.menuStrip_main.Enabled = bEnable;
            this.panel_fixed.Enabled = bEnable;

            this.toolStripDropDownButton_barcodeLoadStyle.Enabled = bEnable;
            this.toolStripTextBox_barcode.Enabled = bEnable;

            this.toolButton_amerce.Enabled = bEnable;
            this.toolButton_borrow.Enabled = bEnable;
            this.toolButton_lost.Enabled = bEnable;
            this.toolButton_readerManage.Enabled = bEnable;
            this.toolButton_renew.Enabled = bEnable;
            this.toolButton_return.Enabled = bEnable;
            this.toolButton_verifyReturn.Enabled = bEnable;
            this.toolButton_print.Enabled = bEnable;
            this.toolStripButton_loadBarcode.Enabled = bEnable;

            this.toolStripDropDownButton_selectLibraryCode.Enabled = bEnable;
        }

        // 把caption名正规化。
        // 也就是判断是否有重复的caption名，如果有，给区别开来
        void CanonicalizeBiblioFromValues()
        {
            if (this.BiblioDbFromInfos == null)
                return;

            for (int i = 0; i < this.BiblioDbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.BiblioDbFromInfos[i];

                int nSeed = 1;
                for (int j = i + 1; j < this.BiblioDbFromInfos.Length; j++)
                {
                    BiblioDbFromInfo info1 = this.BiblioDbFromInfos[j];

                    // 如果caption名重，则给一个附加编号
                    if (info.Caption == info1.Caption
                        && info.Style != info1.Style)
                    {
                        info1.Caption += nSeed.ToString();
                        nSeed++;
                    }
                }
            }
        }

        // 2009/11/8 
        // 
        // Exception:
        //     可能会抛出Exception异常
        /// <summary>
        /// 根据from style字符串得到style caption
        /// </summary>
        /// <param name="strStyle">from style字符串</param>
        /// <returns>style caption字符串</returns>
        public string GetBiblioFromCaption(string strStyle)
        {
            if (this.BiblioDbFromInfos == null)
            {
                throw new Exception("this.DbFromInfos尚未初始化");
            }

            Debug.Assert(this.BiblioDbFromInfos != null, "this.DbFromInfos尚未初始化");

            for (int i = 0; i < this.BiblioDbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.BiblioDbFromInfos[i];

                if (strStyle == info.Style)
                    return info.Caption;
            }

            return null;
        }

        // 
        // Exception:
        //     可能会抛出Exception异常
        /// <summary>
        /// 根据from名列表字符串得到from style列表字符串
        /// </summary>
        /// <param name="strCaptions">检索途径名</param>
        /// <returns>style列表字符串</returns>
        public string GetBiblioFromStyle(string strCaptions)
        {
            if (this.BiblioDbFromInfos == null)
            {
                throw new Exception("this.DbFromInfos尚未初始化");
                // return null;    // 2009/3/29 
            }

            Debug.Assert(this.BiblioDbFromInfos != null, "this.DbFromInfos尚未初始化");

            string strResult = "";

            string[] parts = strCaptions.Split(new char[] { ',' });
            for (int k = 0; k < parts.Length; k++)
            {
                string strCaption = parts[k].Trim();

                // 2009/9/23 
                // TODO: 是否可以直接使用\t后面的部分呢？
                // 规整一下caption字符串，切除后面可能有的\t部分
                int nRet = strCaption.IndexOf("\t");
                if (nRet != -1)
                    strCaption = strCaption.Substring(0, nRet).Trim();

                if (strCaption.ToLower() == "<all>"
                    || strCaption == "<全部>"
                    || String.IsNullOrEmpty(strCaption) == true)
                    return "<all>";

                for (int i = 0; i < this.BiblioDbFromInfos.Length; i++)
                {
                    BiblioDbFromInfo info = this.BiblioDbFromInfos[i];

                    if (strCaption == info.Caption)
                    {
                        if (string.IsNullOrEmpty(strResult) == false)
                            strResult += ",";
                        // strResult += GetDisplayStyle(info.Style, true);   // 注意，去掉 _ 和 __ 开头的那些，应该还剩下至少一个 style
                        strResult += GetDisplayStyle(info.Style, true, false);   // 注意，去掉 __ 开头的那些，应该还剩下至少一个 style。_ 开头的不要滤出
                    }
                }
            }

            return strResult;

            // return null;
        }

        // ComboBox版本
        /// <summary>
        /// 填充书目库检索途径 ComboBox 列表
        /// </summary>
        /// <param name="comboBox_from">ComboBox 对象</param>
        public void FillBiblioFromList(ComboBox comboBox_from)
        {
            // 保存当前的 Text 值
            string strOldText = comboBox_from.Text;

            comboBox_from.Items.Clear();

            comboBox_from.Items.Add("<全部>");

            if (this.BiblioDbFromInfos == null)
                return;

            Debug.Assert(this.BiblioDbFromInfos != null);

            string strFirstItem = "";
            // 装入检索途径
            for (int i = 0; i < this.BiblioDbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.BiblioDbFromInfos[i];

                comboBox_from.Items.Add(info.Caption/* + "(" + infos[i].Style+ ")"*/);

                if (i == 0)
                    strFirstItem = info.Caption;
            }

            comboBox_from.Text = strFirstItem;

            // 2014/5/20
            if (string.IsNullOrEmpty(strOldText) == false)
                comboBox_from.Text = strOldText;
        }

        // TabComboBox版本
        // 右边列出style名
        /// <summary>
        /// 填充书目库检索途径 TabComboBox 列表
        /// 每一行左边是检索途径名，右边是 style 名
        /// </summary>
        /// <param name="comboBox_from">TabComboBox对象</param>
        public void FillBiblioFromList(DigitalPlatform.CommonControl.TabComboBox comboBox_from)
        {
            comboBox_from.Items.Clear();

            comboBox_from.Items.Add("<全部>");

            if (this.BiblioDbFromInfos == null)
                return;

            Debug.Assert(this.BiblioDbFromInfos != null);

            string strFirstItem = "";
            // 装入检索途径
            for (int i = 0; i < this.BiblioDbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.BiblioDbFromInfos[i];

                comboBox_from.Items.Add(info.Caption + "\t" + GetDisplayStyle(info.Style));

                if (i == 0)
                    strFirstItem = info.Caption;
            }

            comboBox_from.Text = strFirstItem;
        }

        // 过滤掉 _ 开头的那些style子串
        // parameters:
        //      bRemove2    是否也要滤除 __ 前缀的
        //                  当出现在检索途径列表里面的时候，为了避免误会，要出现 __ 前缀的；而发送检索请求到 dp2library 的时候，为了避免连带也引起匹配其他检索途径，要把 __ 前缀的 style 滤除
        static string GetDisplayStyle(string strStyles,
            bool bRemove2 = false)
        {
            string[] parts = strStyles.Split(new char[] { ',' });
            List<string> results = new List<string>();
            foreach (string part in parts)
            {
                string strText = part.Trim();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;

                if (bRemove2 == false)
                {
                    // 只滤除 _ 开头的
                    if (StringUtil.HasHead(strText, "_") == true
                        && StringUtil.HasHead(strText, "__") == false)
                        continue;
                }
                else
                {
                    // 2013/12/30 _ 和 __ 开头的都被滤除
                    if (StringUtil.HasHead(strText, "_") == true)
                        continue;
                }

                results.Add(strText);
            }

            return StringUtil.MakePathList(results, ",");
        }

        // 过滤掉 _ 开头的那些style子串
        // parameters:
        //      bRemove2    是否滤除 __ 前缀的
        //      bRemove1    是否滤除 _ 前缀的
        static string GetDisplayStyle(string strStyles,
            bool bRemove2,
            bool bRemove1)
        {
            string[] parts = strStyles.Split(new char[] { ',' });
            List<string> results = new List<string>();
            foreach (string part in parts)
            {
                string strText = part.Trim();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;

                if (strText[0] == '_')
                {
                    if (bRemove1 == true)
                    {
                        if (strText.Length >= 2 && /*strText[0] == '_' &&*/ strText[1] != '_')
                            continue;
#if NO
                        if (strText[0] == '_')
                            continue;
#endif
                        if (strText.Length == 1)
                            continue;
                    }

                    if (bRemove2 == true && strText.Length >= 2)
                    {
                        if (/*strText[0] == '_' && */ strText[1] == '_')
                            continue;
                    }
                }


                results.Add(strText);
            }

            return StringUtil.MakePathList(results, ",");
        }

        // 
        /// <summary>
        /// dp2Library 服务器 URL
        /// </summary>
        public string LibraryServerUrl
        {
            get
            {
                if (this.AppInfo == null)
                    return "";

                return this.AppInfo.GetString(
                    "config",
                    "circulation_server_url",
                    "http://localhost:8001/dp2library");
            }
            set
            {
                if (this.AppInfo != null)
                {
                    this.AppInfo.SetString(
                        "config",
                        "circulation_server_url",
                        value);
                }
            }
        }

        /// <summary>
        /// 清除值列表缓存
        /// </summary>
        public void ClearValueTableCache()
        {
            this.valueTableCache = new Hashtable();

            // 通知所有 MDI 子窗口
            ParamChangedEventArgs e = new ParamChangedEventArgs();
            e.Section = "valueTableCacheCleared";
            NotifyAllMdiChildren(e);
        }

        void NotifyAllMdiChildren(ParamChangedEventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form is MyForm)
                {
                    MyForm myForm = form as MyForm;
                    myForm.OnNotify(e);
                }
            }
        }

        /*
发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 dp2Circulation.MainForm.GetValueTable(String strTableName, String strDbName, String[]& values, String& strError)
在 dp2Circulation.ChangeItemActionDialog.dlg_GetValueTable(Object sender, GetValueTableEventArgs e)
在 dp2Circulation.OneActionDialog.FillDropDown(ComboBox combobox)
在 dp2Circulation.OneActionDialog.comboBox_fieldValue_DropDown(Object sender, EventArgs e)
在 System.Windows.Forms.ComboBox.WmReflectCommand(Message& m)
在 System.Windows.Forms.ComboBox.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

         * */
        // 获得值列表
        // 带有cache功能
        /// <summary>
        /// 获得值列表
        /// </summary>
        /// <param name="strTableName">表格名</param>
        /// <param name="strDbName">数据库名</param>
        /// <param name="values">返回值字符串数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetValueTable(string strTableName,
            string strDbName,
            out string[] values,
            out string strError)
        {
            values = null;
            strError = "";

            // 先看看缓存里面是否已经有了
            string strName = strTableName + "~~~" + strDbName;

            values = (string[])this.valueTableCache[strName];

            if (values != null)
                return 0;

#if NO
            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;  // 2009/2/11
            }
#endif
            LibraryChannel channel = this.GetChannel();

            if (Stop == null)
            {
                ReportError("dp2circulation 调试信息", "MainForm.GetValueTable() 中发现 Stop 为空");
            }

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获取值列表 ...");
            Stop.BeginLoop();

            try
            {
                channel.Timeout = new TimeSpan(0, 0, 10);
                long lRet = channel.GetValueTable(
                    Stop,
                    strTableName,
                    strDbName,
                    out values,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (values == null)
                    values = new string[0];

                this.valueTableCache[strName] = values;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                // EndSearch();
            }

            return 0;
        ERROR1:
            return -1;
        }

        #region EnsureXXXForm ...

        /// <summary>
        /// 获得最顶层的 UtilityForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public UtilityForm EnsureUtilityForm()
        {
#if NO
            UtilityForm form = TopUtilityForm;
            if (form == null)
            {
                form = new UtilityForm();
                form.MdiParent = this;
                // form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<UtilityForm>();
        }

        /// <summary>
        /// 获得最顶层的 HtmlPrintForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <param name="bMinimized">是否最小化窗口</param>
        /// <returns>窗口</returns>
        public HtmlPrintForm EnsureHtmlPrintForm(bool bMinimized)
        {
            HtmlPrintForm form = GetTopChildWindow<HtmlPrintForm>();
            if (form == null)
            {
                Form top = this.ActiveMdiChild;
                FormWindowState top_state = FormWindowState.Normal;
                if (top != null)
                {
                    top_state = top.WindowState;
                }

                form = new HtmlPrintForm();
                form.MdiParent = this;
                form.MainForm = this;
                if (bMinimized == true)
                    form.WindowState = FormWindowState.Minimized;
                form.Show();

                if (top != null && bMinimized == true)
                {
                    if (top.WindowState != top_state)
                        top.WindowState = top_state;
                }
            }

            return form;
        }

        /// <summary>
        /// 获得最顶层的 AmerceForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public AmerceForm EnsureAmerceForm()
        {
#if NO
            AmerceForm form = TopAmerceForm;
            if (form == null)
            {
                form = new AmerceForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<AmerceForm>();
        }

        /// <summary>
        /// 获得最顶层的 ReaderManageForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public ReaderManageForm EnsureReaderManageForm()
        {
#if NO
            ReaderManageForm form = TopReaderManageForm;
            if (form == null)
            {
                form = new ReaderManageForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<ReaderManageForm>();
        }

        /// <summary>
        /// 获得最顶层的 ReaderInfoForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public ReaderInfoForm EnsureReaderInfoForm()
        {
#if NO
            ReaderInfoForm form = TopReaderInfoForm;
            if (form == null)
            {
                form = new ReaderInfoForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<ReaderInfoForm>();
        }

        /// <summary>
        /// 获得最顶层的 ActivateForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public ActivateForm EnsureActivateForm()
        {
#if NO
            ActivateForm form = TopActivateForm;
            if (form == null)
            {
                form = new ActivateForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<ActivateForm>();
        }

        /// <summary>
        /// 获得最顶层的 EntityForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public EntityForm EnsureEntityForm()
        {
#if NO
            EntityForm form = TopEntityForm;
            if (form == null)
            {
                form = new EntityForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<EntityForm>();
        }

        /// <summary>
        /// 获得最顶层的 DupForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public DupForm EnsureDupForm()
        {
#if NO
            DupForm form = TopDupForm;
            if (form == null)
            {
                form = new DupForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<DupForm>();
        }

        /// <summary>
        /// 获得最顶层的 ItemInfoForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public ItemInfoForm EnsureItemInfoForm()
        {
#if NO
            ItemInfoForm form = TopItemInfoForm;
            if (form == null)
            {
                form = new ItemInfoForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<ItemInfoForm>();
        }

        /// <summary>
        /// 获得最顶层的 PrintAcceptForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public PrintAcceptForm EnsurePrintAcceptForm()
        {
#if NO
            PrintAcceptForm form = TopPrintAcceptForm;
            if (form == null)
            {
                form = new PrintAcceptForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<PrintAcceptForm>();
        }

        /// <summary>
        /// 获得最顶层的 LabelPrintForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public LabelPrintForm EnsureLabelPrintForm()
        {
#if NO
            LabelPrintForm form = TopLabelPrintForm;
            if (form == null)
            {
                form = new LabelPrintForm();
                form.MdiParent = this;
                // form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<LabelPrintForm>();
        }

        /// <summary>
        /// 获得最顶层的 CardPrintForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public CardPrintForm EnsureCardPrintForm()
        {
#if NO
            CardPrintForm form = TopCardPrintForm;
            if (form == null)
            {
                form = new CardPrintForm();
                form.MdiParent = this;
                // form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<CardPrintForm>();
        }

        /// <summary>
        /// 获得最顶层的 BiblioStatisForm 窗口，如果没有，则新创建一个
        /// </summary>
        /// <returns>窗口</returns>
        public BiblioStatisForm EnsureBiblioStatisForm()
        {
#if NO
            BiblioStatisForm form = TopBiblioStatisForm;
            if (form == null)
            {
                form = new BiblioStatisForm();
                form.MdiParent = this;
                // form.MainForm = this;
                form.Show();
            }

            return form;
#endif
            return EnsureChildForm<BiblioStatisForm>();
        }

        #endregion

        private void toolButton_borrow_Click(object sender, EventArgs e)
        {
            if (this.Urgent == false)
            {
                if (this.ActiveMdiChild != null
                    && this.ActiveMdiChild is ChargingForm)
                {
                    EnsureChildForm<ChargingForm>().Activate();
                    EnsureChildForm<ChargingForm>().SmartFuncState = FuncState.Borrow;
                }
                else
                {
                    EnsureChildForm<QuickChargingForm>().Activate();
                    EnsureChildForm<QuickChargingForm>().SmartFuncState = FuncState.Borrow;
                }
            }
            else
            {
                EnsureChildForm<UrgentChargingForm>().Activate();
                EnsureChildForm<UrgentChargingForm>().SmartFuncState = FuncState.Borrow;
            }
        }

        private void toolButton_return_Click(object sender, EventArgs e)
        {
            if (this.Urgent == false)
            {
                if (this.ActiveMdiChild != null
                    && this.ActiveMdiChild is ChargingForm)
                {
                    EnsureChildForm<ChargingForm>().Activate();
                    EnsureChildForm<ChargingForm>().SmartFuncState = FuncState.Return;
                }
                else
                {
                    EnsureChildForm<QuickChargingForm>().Activate();
                    EnsureChildForm<QuickChargingForm>().SmartFuncState = FuncState.Return;
                }
            }
            else
            {
                EnsureChildForm<UrgentChargingForm>().Activate();
                EnsureChildForm<UrgentChargingForm>().SmartFuncState = FuncState.Return;
            }
        }

        private void toolButton_verifyReturn_Click(object sender, EventArgs e)
        {
            if (this.Urgent == false)
            {
                if (this.ActiveMdiChild != null
                    && this.ActiveMdiChild is ChargingForm)
                {
                    EnsureChildForm<ChargingForm>().Activate();
                    EnsureChildForm<ChargingForm>().SmartFuncState = FuncState.VerifyReturn;
                }
                else
                {
                    EnsureChildForm<QuickChargingForm>().Activate();
                    EnsureChildForm<QuickChargingForm>().SmartFuncState = FuncState.VerifyReturn;
                }
            }
            else
            {
                EnsureChildForm<UrgentChargingForm>().Activate();
                EnsureChildForm<UrgentChargingForm>().SmartFuncState = FuncState.VerifyReturn;
            }
        }

        private void toolButton_renew_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null
                && this.ActiveMdiChild is ChargingForm)
            {
                EnsureChildForm<ChargingForm>().Activate();
                EnsureChildForm<ChargingForm>().SmartFuncState = FuncState.Renew;   // FuncState.VerifyRenew;
            }
            else
            {
                EnsureChildForm<QuickChargingForm>().Activate();
                EnsureChildForm<QuickChargingForm>().SmartFuncState = FuncState.Renew;   // FuncState.VerifyRenew;
            }
        }

        private void toolButton_lost_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null
                && this.ActiveMdiChild is ChargingForm)
            {
                EnsureChildForm<ChargingForm>().Activate();
                EnsureChildForm<ChargingForm>().SmartFuncState = FuncState.Lost;
            }
            else
            {
                EnsureChildForm<QuickChargingForm>().Activate();
                EnsureChildForm<QuickChargingForm>().SmartFuncState = FuncState.Lost;
            }
        }

        private void toolButton_amerce_Click(object sender, EventArgs e)
        {
            EnsureAmerceForm().Activate();
        }

        private void toolButton_readerManage_Click(object sender, EventArgs e)
        {
            EnsureReaderManageForm().Activate();
        }

        // 触发打印按钮
        private void toolButton_print_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                EnsureChildForm<ChargingPrintManageForm>().Activate();
            }
            else
            {
                Form active = this.ActiveMdiChild;

                if (active is ChargingForm)
                {
                    ChargingForm form = (ChargingForm)active;
                    form.Print();
                }
                else if (active is QuickChargingForm)
                {
                    QuickChargingForm form = (QuickChargingForm)active;
                    form.Print();
                }
                else if (active is AmerceForm)
                {
                    AmerceForm form = (AmerceForm)active;
                    form.Print();
                }
            }
        }

        // 装载条码
        private void toolStripButton_loadBarcode_Click(object sender, EventArgs e)
        {
            if (this.ToolStripMenuItem_loadReaderInfo.Checked == true)
                LoadReaderBarcode();
            else if (this.ToolStripMenuItem_loadItemInfo.Checked == true)
                LoadItemBarcode();
            else
            {
                Debug.Assert(this.ToolStripMenuItem_autoLoadItemOrReader.Checked == true, "");
                LoadItemOrReaderBarcode();
            }
            this.toolStripTextBox_barcode.SelectAll();
        }

        private void toolStripTextBox_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // 回车
                case Keys.Enter:
                    toolStripButton_loadBarcode_Click(sender, e);
                    break;
            }

        }

        // return:
        //      true    已经装载
        //      false   没有装载
        bool TryLoadItemBarcodeToItemSearchForm()
        {
            Form top = this.GetTopChildWindow();
            if (top != null && top is ItemSearchForm)
            {
                ItemSearchForm form = top as ItemSearchForm;
                form.Activate();
                List<string> barcodes = new List<string>();
                barcodes.Add(this.toolStripTextBox_barcode.Text);

                form.ClearMessage();
                string strError = "";
                // 往列表中追加若干册条码号
                // return:
                //      -1  出错
                //      0   成功
                //      1   成功，但有警告，警告在 strError 中返回
                int nRet = form.AppendBarcodes(barcodes, out strError);
                if (nRet == -1 || nRet == 1)
                {
                    // MessageBox.Show(this, strError);
                    form.ShowMessage(strError, "red", true);
                }

                this.toolStripTextBox_barcode.SelectAll();
                this.toolStripTextBox_barcode.Focus();
                return true;
            }

            return false;
        }

        // 装入册条码号相关记录
        // 尽量占用当前已经打开的种册窗
        void LoadItemBarcode()
        {
            if (this.toolStripTextBox_barcode.Text == "")
            {
                MessageBox.Show(this, "尚未输入条码");
                return;
            }

            if (TryLoadItemBarcodeToItemSearchForm() == true)
                return;

            EntityForm form = this.GetTopChildWindow<EntityForm>();

            if (form == null)
            {
                // 新开一个种册窗
                form = new EntityForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }
            else
                Global.Activate(form);

            // 装载一个册，连带装入种
            // parameters:
            //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            form.LoadItemByBarcode(this.toolStripTextBox_barcode.Text, false);
        }

        // 装入读者证条码号相关的记录
        // 尽量占用当前已经打开的读者窗
        void LoadReaderBarcode()
        {
            if (this.toolStripTextBox_barcode.Text == "")
            {
                MessageBox.Show(this, "尚未输入条码号");
                return;
            }

            ReaderInfoForm form = this.GetTopChildWindow<ReaderInfoForm>();

            if (form == null)
            {
                // 新开一个读者窗
                form = new ReaderInfoForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }
            else
                Global.Activate(form);


            // 根据读者证条码号，装入读者记录
            // parameters:
            //      bForceLoad  在发生重条码的情况下是否强行装入第一条
            form.LoadRecord(this.toolStripTextBox_barcode.Text,
                false);
        }

        // 自动判断条码类型并装入相应的记录
        void LoadItemOrReaderBarcode()
        {
            string strError = "";

            if (this.toolStripTextBox_barcode.Text == "")
            {
                strError = "尚未输入条码";
                goto ERROR1;
            }

            // 形式校验条码号
            // return:
            //      -2  服务器没有配置校验方法，无法校验
            //      -1  error
            //      0   不是合法的条码号
            //      1   是合法的读者证条码号
            //      2   是合法的册条码号
            int nRet = VerifyBarcode(
                this.FocusLibraryCode,  // this._currentLibraryCodeList,    // this.Channel.LibraryCodeList,
                this.toolStripTextBox_barcode.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == -2)
            {
                strError = "服务器端尚未提供条码号校验方法，因此无法分辨读者证条码号和册条码号";
                goto ERROR1;
            }

            if (nRet == 0)
            {
                if (String.IsNullOrEmpty(strError) == true)
                    strError = "条码不合法";
                goto ERROR1;
            }

            if (nRet == 1)
                LoadReaderBarcode();

            if (nRet == 2)
                LoadItemBarcode();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 写入词典库记录
        public int WriteDictionary(
            string strDbName,
            string strKey,
            string strValue,
            out string strError)
        {
            strError = "";

            string strLang = "zh";
            string strQueryXml = "<target list='" + strDbName + ":" + "键" + "'><item><word>"
+ StringUtil.GetXmlStringSimple(strKey)
+ "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + strLang + "</lang></target>";

#if NO
            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索词条 '" + strKey + "' ...");
            Stop.BeginLoop();

            EnableControls(false);
            try
            {
                long lRet = channel.Search(
                    Stop,
                    strQueryXml,
                    "default",
                    "",
                    out strError);

                if (lRet == -1)
                    goto ERROR1;

                string strXml = "";
                string strRecPath = "";
                byte[] baTimestamp = null;
                if (lRet >= 1)
                {
                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;
                    lRet = channel.GetSearchResult(
                        Stop,
                        "default",
                        0,
                        1,
                        "id,xml,timestamp",   // "id,cols"
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DigitalPlatform.LibraryClient.localhost.Record record = searchresults[0];

                    strXml = (record.RecordBody.Xml);
                    strRecPath = record.Path;
                    baTimestamp = record.RecordBody.Timestamp;
                }

                XmlDocument dom = new XmlDocument();
                if (string.IsNullOrEmpty(strXml) == false)
                {
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "XML 装入 DOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }
                }
                else
                {
                    dom.LoadXml("<root />");
                }

                // 创建 key 元素
                XmlNode nodeKey = dom.DocumentElement.SelectSingleNode("key");
                if (nodeKey == null)
                {
                    nodeKey = dom.CreateElement("key");
                    dom.DocumentElement.AppendChild(nodeKey);
                }

                DomUtil.SetAttr(nodeKey, "name", strKey);

                // 寻找匹配的 rel 元素
                XmlNode nodeRel = dom.DocumentElement.SelectSingleNode("rel[@name=" + StringUtil.XPathLiteral(strValue) + "]");
                if (nodeRel == null)
                {
                    nodeRel = dom.CreateElement("rel");
                    dom.DocumentElement.AppendChild(nodeRel);

                    DomUtil.SetAttr(nodeRel, "name", strValue);
                }

                // weight 加 1
                string strWeight = DomUtil.GetAttr(nodeRel, "weight");
                if (string.IsNullOrEmpty(strWeight) == true)
                    strWeight = "1";
                else
                {
                    long v = 0;
                    long.TryParse(strWeight, out v);
                    v++;
                    strWeight = v.ToString();
                }

                DomUtil.SetAttr(nodeRel, "weight", strWeight);

                // 写回记录
                if (string.IsNullOrEmpty(strRecPath) == true)
                    strRecPath = strDbName + "/?";

                byte[] output_timestamp = null;
                string strOutputPath = "";
                // 保存Xml记录。包装版本。用于保存文本类型的资源。
                lRet = channel.WriteRes(
                    Stop,
                    strRecPath,
                    dom.DocumentElement.OuterXml,
                    true,
                    "",
                    baTimestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return 0;
            }
            finally
            {
                EnableControls(true);

                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                // EndSearch();
            }
        ERROR1:
            return -1;
        }

        // 检索词典库
        // parameters:
        //      results [in,out] 如果需要返回结果，需要在调用前 new List<string>()。如果不需要返回结果，传入 null 即可
        public int SearchDictionary(
            LibraryChannel channel,
            Stop stop,
            string strDbName,
            string strKey,
            string strMatchStyle,
            int nMaxCount,
            ref List<string> results,
            out string strError)
        {
            strError = "";

            string strLang = "zh";
            string strQueryXml = "<target list='" + strDbName + ":" + "键" + "'><item><word>"
+ StringUtil.GetXmlStringSimple(strKey)
+ "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMaxCount.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

#if NO
            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;
            }
#endif
            // LibraryChannel channel = this.GetChannel();

            if (stop == null)
            {
                stop = Stop;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索词条 '" + strKey + "' ...");
                stop.BeginLoop();

                EnableControls(false);
            }

            try
            {
                long lRet = channel.Search(
                    stop,
                    strQueryXml,
                    "default",
                    "",
                    out strError);
                if (lRet == 0)
                    return 0;
                if (lRet == -1)
                    goto ERROR1;
                if (results == null)
                    return (int)lRet;

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                for (; ; )
                {
                    lRet = channel.GetSearchResult(
                        Stop,
                        "default",
                        lStart,
                        lPerCount,
                        "id,xml",   // "id,cols"
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record record = searchresults[i];

                        results.Add(record.Path + "|" + record.RecordBody.Xml);
                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }

                return (int)lHitCount;
            }
            finally
            {
                if (stop == Stop)
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }

                // this.ReturnChannel(channel);
                // EndSearch();
            }
        ERROR1:
            return -1;
        }

#if NO
        // 形式校验条码号
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        int VerifyBarcode(
            string strBarcode,
            out string strError)
        {
            strError = "";

            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;  // 2009/2/11
            }

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在校验条码 ...");
            Stop.BeginLoop();

            /*
            this.Update();
            this.MainForm.Update();
             * */
            EnableControls(false);

            try
            {
                long lRet = Channel.VerifyBarcode(
                    Stop,
                    strBarcode,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        return -2;
                    goto ERROR1;
                }
                return (int)lRet;
            }
            finally
            {
                EnableControls(true);

                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                EndSearch();
            }
        ERROR1:
            return -1;
        }
#endif

        // 包装后的版本
        // 形式校验条码号
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        int VerifyBarcode(
            string strLibraryCodeList,
            string strBarcode,
            out string strError)
        {
            strError = "";

#if NO
            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;  // 2009/2/11
            }
#endif
            LibraryChannel channel = this.GetChannel();

            if (this.Stop == null)
            {
                strError = "MainForm.Stop 尚未初始化";
                return -1;
            }

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在验证条码号 " + strBarcode + "...");
            Stop.BeginLoop();

            // EnableControls(false);

            try
            {
                return VerifyBarcode(
                    Stop,
                    channel,
                    strLibraryCodeList,
                    strBarcode,
                    EnableControls,
                    out strError);
            }
            finally
            {
                // EnableControls(true);

                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                // EndSearch();
            }
        }

        public delegate void Delegate_enableControls(bool bEnable);

        // 形式校验条码号
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="stop">停止对象</param>
        /// <param name="Channel">通讯通道</param>
        /// <param name="strLibraryCode">馆代码</param>
        /// <param name="strBarcode">要校验的条码号</param>
        /// <param name="procEnableControls">EnableControl()函数地址</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-2  服务器没有配置校验方法，无法校验</para>
        /// <para>-1  出错</para>
        /// <para>0   不是合法的条码号</para>
        /// <para>1   是合法的读者证条码号</para>
        /// <para>2   是合法的册条码号</para>
        /// </returns>
        public int VerifyBarcode(
            Stop stop,
            LibraryChannel Channel,
            string strLibraryCode,
            string strBarcode,
            Delegate_enableControls procEnableControls,
            out string strError)
        {
            strError = "";

            // 2014/5/4
            if (StringUtil.HasHead(strBarcode, "PQR:") == true)
            {
                strError = "这是读者证号二维码";
                return 1;
            }

            // 优先进行前端校验
            if (this.ClientHost != null)
            {
                bool bOldStyle = false;
                dynamic o = this.ClientHost;
                try
                {
                    return o.VerifyBarcode(
                        strLibraryCode, // 2014/9/27 新增
                        strBarcode,
                        out strError);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
                {
                    // 继续向后进行服务器端条码校验
                    bOldStyle = true;
                }
                catch (Exception ex)
                {
                    strError = "前端执行校验脚本抛出异常: " + ExceptionUtil.GetDebugText(ex);
                    return -1;
                }

                if (bOldStyle == true)
                {
                    // 尝试以前的参数方式
                    try
                    {
                        return o.VerifyBarcode(
                            strBarcode,
                            out strError);
                    }
                    catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
                    {
                        // 继续向后进行服务器端条码校验
                    }
                    catch (Exception ex)
                    {
                        strError = "前端执行校验脚本抛出异常: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                }
            }

            if (procEnableControls != null)
                procEnableControls(false);
            // EnableControls(false);

#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在校验条码 ...");
            stop.BeginLoop();
#endif
            try
            {
                long lRet = Channel.VerifyBarcode(
                    stop,
                    strLibraryCode,
                    strBarcode,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        return -2;
                    return -1;
                }
                return (int)lRet;
            }
            finally
            {
#if NO
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                // EnableControls(true);
#endif
                if (procEnableControls != null)
                    procEnableControls(true);
            }
        }

        public void SetServerName(string strUrl, string strServerName)
        {
            string value = AppInfo.GetString("login",
        "used_list",
        "");

            if (CirculationLoginDlg.SetServerName(ref value,
                strUrl,
                strServerName,
                true) == true)
            {
                AppInfo.SetString("login",
                    "used_list",
                        value);
            }
        }

        // TODO: 对于外部 URL 如何做到不记载到 dp2circulation.xml 中?
        // parameters:
        //      bLogin  是否在对话框后立即登录？如果为false，表示只是设置缺省帐户，并不直接登录
        CirculationLoginDlg SetDefaultAccount(
            string strServerUrl,
            string strTitle,
            string strComment,
            LoginFailCondition fail_contidion,
            IWin32Window owner,
            bool bLogin = true)
        {
            CirculationLoginDlg dlg = new CirculationLoginDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.UsedList = AppInfo.GetString("login",
        "used_list",
        "");

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
                dlg.ServerUrl =
        AppInfo.GetString("config",
        "circulation_server_url",
        "http://localhost:8001/dp2library");
            }
            else
            {
                dlg.ServerUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;

            if (String.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;

            if (bLogin == false)
                dlg.SetDefaultMode = true;
            dlg.Comment = strComment;
            dlg.UserName = AppInfo.GetString(
                "default_account",
                "username",
                "");

            dlg.SavePasswordShort =
    AppInfo.GetBoolean(
    "default_account",
    "savepassword_short",
    false);

            dlg.SavePasswordLong =
                AppInfo.GetBoolean(
                "default_account",
                "savepassword_long",
                false);

            if (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true)
            {
                dlg.Password = AppInfo.GetString(
        "default_account",
        "password",
        "");
                dlg.Password = this.DecryptPasssword(dlg.Password);
            }
            else
            {
                dlg.Password = "";
            }

            dlg.IsReader =
                AppInfo.GetBoolean(
                "default_account",
                "isreader",
                false);
            dlg.OperLocation = AppInfo.GetString(
                "default_account",
                "location",
                "");

            // 2016/11/11
            if (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true)
                dlg.PhoneNumber = AppInfo.GetString(
"default_account",
"phoneNumber",
"");

            this.AppInfo.LinkFormState(dlg,
                "logindlg_state");

            if (fail_contidion == LoginFailCondition.PasswordError
                && dlg.SavePasswordShort == false
                && dlg.SavePasswordLong == false)
                dlg.AutoShowShortSavePasswordTip = true;

            if (fail_contidion == LoginFailCondition.RetryLogin)
            {
                dlg.ActivateTempCode();
                dlg.RetryLogin = true;
            }
            if (fail_contidion == LoginFailCondition.TempCodeMismatch)
            {
                dlg.ActivateTempCode();
                dlg.RetryLogin = true;  // 尝试再次登录
            }
            if (fail_contidion == LoginFailCondition.NeedSmsLogin)
                dlg.ActivatePhoneNumber();

            dlg.ShowDialog(owner);

            this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            AppInfo.SetString("login",
"used_list",
dlg.UsedList);

            // 如果是即将访问 dp2libraryXE 单机版，这里要启动它
            if (string.Compare(dlg.ServerUrl, CirculationLoginDlg.dp2LibraryXEServerUrl, true) == 0)
                AutoStartDp2libraryXE();

            AppInfo.SetString(
                "default_account",
                "username",
                dlg.UserName);
            AppInfo.SetString(
                "default_account",
                "password",
                (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true) ?
                this.EncryptPassword(dlg.Password) : "");

            AppInfo.SetBoolean(
    "default_account",
    "savepassword_short",
    dlg.SavePasswordShort);

            AppInfo.SetBoolean(
                "default_account",
                "savepassword_long",
                dlg.SavePasswordLong);

            AppInfo.SetBoolean(
                "default_account",
                "isreader",
                dlg.IsReader);
            AppInfo.SetString(
                "default_account",
                "location",
                dlg.OperLocation);

            // 2006/12/30 
            AppInfo.SetString(
                "config",
                "circulation_server_url",
                dlg.ServerUrl);

            // 2016/11/11
            AppInfo.SetString(
    "default_account",
    "phoneNumber",
    dlg.PhoneNumber);

            return dlg;
        }

        void AutoStartDp2libraryXE()
        {
            string strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V2/dp2Library XE");
            if (File.Exists(strShortcutFilePath) == false)
            {
                // 安装和启动
                DialogResult result = MessageBox.Show(this,
"dp2libraryXE 在本机尚未安装。\r\ndp2Circulation (内务)即将访问 dp2LibraryXE 单机版服务器，需要安装它才能正常使用。\r\n\r\n是否立即从 dp2003.com 下载安装?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    FirstRunDialog.StartDp2libraryXe(
                        this,
                        "dp2Circulation",
                        this.Font,
                        false);
            }
            else
            {
                if (FirstRunDialog.HasDp2libraryXeStarted() == false)
                {
                    FirstRunDialog.StartDp2libraryXe(
                        this,
                        "dp2Circulation",
                        this.Font,
                        true);
                }
            }

            // 如果当前窗口没有在最前面
            {
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.Activate();
                API.SetForegroundWindow(this.Handle);
            }
        }

        internal string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        internal string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, this.EncryptKey);
        }

        /// <summary>
        /// 当前是否为应急借还状态
        /// </summary>
        public bool Urgent
        {
            get
            {
                return this.m_bUrgent;
            }
            set
            {
                this.m_bUrgent = value;
            }
        }

        // 获得缓存中的读者记录XML
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        internal int GetCachedReaderXml(string strReaderBarcode,
            string strConfirmReaderRecPath,
            out string strXml,
            out string strOutputPath,
            out string strError)
        {
            strXml = "";
            strOutputPath = "";
            strError = "";

            string strReaderBarcodeUnionPath = strReaderBarcode + "|" + strConfirmReaderRecPath;
            // 看看cache中是否已经有了
            StringCacheItem item = this.ReaderXmlCache.SearchItem(strReaderBarcodeUnionPath);
            if (item != null)
            {
                int nRet = item.Content.IndexOf("|");
                if (nRet == -1)
                    strXml = item.Content;
                else
                {
                    strOutputPath = item.Content.Substring(0, nRet);
                    strXml = item.Content.Substring(nRet + 1);
                }
                return 1;
            }

            return 0;
        }

        // 加入读者记录XML缓存
        internal void SetReaderXmlCache(string strReaderBarcode,
    string strConfirmReaderRecPath,
    string strXml,
            string strPath)
        {
            string strReaderBarcodeUnionPath = strReaderBarcode + "|" + strConfirmReaderRecPath;
            StringCacheItem item = this.SummaryCache.EnsureItem(strReaderBarcodeUnionPath);
            item.Content = strPath + "|" + strXml;
        }

        // 2014/9/20
        /// <summary>
        /// 获得读者摘要
        /// </summary>
        /// <param name="strPatronBarcode">读者证条码号</param>
        /// <param name="bDisplayProgress">是否在进度条上显示</param>
        /// <returns></returns>
        public string GetReaderSummary(string strPatronBarcode,
            bool bDisplayProgress)
        {
            if (string.IsNullOrEmpty(strPatronBarcode) == true)
                return "";

            string strError = "";
            string strXml = "";
            string strOutputPath = "";

            // 获得缓存中的读者记录XML
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = this.GetCachedReaderXml(strPatronBarcode,
                "",
out strXml,
out strOutputPath,
out strError);
            if (nRet == -1)
                return strError;

            if (nRet == 0)
            {
#if NO
                nRet = PrepareSearch(bDisplayProgress);
                if (nRet == 0)
                    return "PrepareSearch() error";
#endif
                LibraryChannel channel = this.GetChannel();

                Stop.OnStop += new StopEventHandler(this.DoStop);
                if (bDisplayProgress == true)
                {
                    Stop.Initial("正在获得读者信息 '" + strPatronBarcode + "'...");
                    Stop.BeginLoop();
                }

                try
                {
                    string[] results = null;
                    byte[] baTimestamp = null;

                    long lRet = channel.GetReaderInfo(Stop,
                        strPatronBarcode,
                        "xml",
                        out results,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        return "!" + strError;
                    }
                    else if (lRet > 1)
                    {
                        strError = "!读者证条码号 " + strPatronBarcode + " 有重复记录 " + lRet.ToString() + "条";
                        return strError;
                    }
                    if (lRet == 0)
                    {
                        strError = "!证条码号为 " + strPatronBarcode + " 的读者记录没有找到";
                        return strError;
                    }

                    Debug.Assert(results.Length > 0, "");
                    strXml = results[0];

                    // 加入到缓存
                    this.SetReaderXmlCache(strPatronBarcode,
                        "",
                        strXml,
                        strOutputPath);
                }
                finally
                {
                    if (bDisplayProgress == true)
                    {
                        Stop.EndLoop();
                        Stop.Initial("");
                    }
                    Stop.OnStop -= new StopEventHandler(this.DoStop);

                    this.ReturnChannel(channel);
                    // this.EndSearch();
                }
            }

            return Global.GetReaderSummary(strXml);
        }


        // 获得缓存中的bibliosummary
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        internal int GetCachedBiblioSummary(string strItemBarcode,
    string strConfirmItemRecPath,
    out string strSummary,
    out string strError)
        {
            strSummary = "";
            strError = "";

            string strItemBarcodeUnionPath = strItemBarcode + "|" + strConfirmItemRecPath;
            // 看看cache中是否已经有了
            StringCacheItem item = this.SummaryCache.SearchItem(strItemBarcodeUnionPath);
            if (item != null)
            {
                strSummary = item.Content;
                return 1;
            }

            return 0;
        }

        internal void SetBiblioSummaryCache(string strItemBarcode,
    string strConfirmItemRecPath,
    string strSummary)
        {
            string strItemBarcodeUnionPath = strItemBarcode + "|" + strConfirmItemRecPath;
            // 如果cache中没有，则加入cache
            StringCacheItem item = this.SummaryCache.EnsureItem(strItemBarcodeUnionPath);
            item.Content = strSummary;
        }

        /// <summary>
        /// 获得书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认的册记录路径</param>
        /// <param name="bDisplayProgress">是否在进度条上显示</param>
        /// <param name="strSummary">返回书目摘要</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetBiblioSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            bool bDisplayProgress,
            out string strSummary,
            out string strError)
        {
            /*
            // 调试
            strSummary = "...";
            strError = "";
            return 0;
             * */

            string strItemBarcodeUnionPath = strItemBarcode + "|" + strConfirmItemRecPath;
            // 看看cache中是否已经有了
            StringCacheItem item = this.SummaryCache.SearchItem(strItemBarcodeUnionPath);
            if (item != null)
            {
                strError = "";
                strSummary = item.Content;
                return 0;
            }

#if NO
            int nRet = PrepareSearch(bDisplayProgress);
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                strSummary = strError;
                return -1;  // 2009/2/11
            }
#endif
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            if (bDisplayProgress == true)
            {
                Stop.Initial("MainForm正在获得书目摘要 '" + strItemBarcodeUnionPath + "'...");
                Stop.BeginLoop();
            }

            try
            {
                string strBiblioRecPath = "";

                // 因为本对象只有一个Channel通道，所以要锁定使用
                this.m_lockChannel.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    long lRet = channel.GetBiblioSummary(
                        Stop,
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

                }
                catch
                {
                    strSummary = "对Channel对象锁定失败...";
                    strError = "对Channel对象锁定失败...";
                    return -1;
                }
                finally
                {
                    this.m_lockChannel.ReleaseWriterLock();
                }

                // 如果cache中没有，则加入cache
                item = this.SummaryCache.EnsureItem(strItemBarcodeUnionPath);
                item.Content = strSummary;

            }
            finally
            {
                if (bDisplayProgress == true)
                {
                    Stop.EndLoop();
                    Stop.Initial("");
                }
                Stop.OnStop -= new StopEventHandler(this.DoStop);

                this.ReturnChannel(channel);
                // this.EndSearch(bDisplayProgress);   // BUG !!! 2012/3/28前少这一句
            }

            return 0;
        }

#if NOOOOOOOOOOOOOOO
        // 已经被stop弄成有显示不出来的毛病
        public void ShowProgress(bool bVisible)
        {
            this.toolStripProgressBar_main.Visible = bVisible;
        }

        public void SetProgressValue(int nValue)
        {
            this.toolStripProgressBar_main.Value = nValue;
        }

        public void SetProgressRange(int nMax)
        {
            this.toolStripProgressBar_main.Minimum = 0;
            this.toolStripProgressBar_main.Maximum = nMax;
        }
#endif

        private void MenuItem_about_Click(object sender, EventArgs e)
        {
            AboutDlg dlg = new AboutDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            // dlg.MainForm = this;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }


        /// <summary>
        /// 获取或设置框架窗口状态行的文字信息
        /// </summary>
        public string StatusBarMessage
        {
            get
            {
                return toolStripStatusLabel_main.Text;
            }
            set
            {
                toolStripStatusLabel_main.Text = value;
            }
        }


#if NO
        // 下载数据文件
        public int DownloadDataFile(string strFileName,
            out string strError)
        {
            strError = "";

            WebClient webClient = new WebClient();

            // TODO: 是否启用dp2003.cn域名?
            string strUrl = "http://dp2003.com/dp2Circulation/" + strFileName;
            string strLocalFileName = this.DataDir + "\\" + strFileName;
            try
            {
                webClient.DownloadFile(strUrl,
                    strLocalFileName);
            }
            catch (Exception ex)
            {
                strError = "下载" + strFileName + "文件发生错误 :" + ex.Message;
                return -1;
            }

            strError = "下载" + strFileName + "文件成功 :\r\n" + strUrl + " --> " + strLocalFileName;
            return 0;
        }
#endif

        // 
        /// <summary>
        /// 下载数据文件
        /// 从 http://dp2003.com/dp2Circulation/ 位置下载到数据目录，文件名保持不变
        /// </summary>
        /// <param name="strFileName">纯文件名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int DownloadDataFile(string strFileName,
            out string strError)
        {
            strError = "";

            string strUrl = "http://dp2003.com/dp2Circulation/" + strFileName;
            string strLocalFileName = this.DataDir + "\\" + strFileName;
            string strTempFileName = this.DataDir + "\\~temp_download_webfile";

            int nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
                strUrl,
                strLocalFileName,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;
            strError = "下载" + strFileName + "文件成功 :\r\n" + strUrl + " --> " + strLocalFileName;
            return 0;
        }

        /// <summary>
        /// 装载快速加拼音需要的辅助信息
        /// </summary>
        /// <param name="bAutoDownload">是否自动从 dp2003.com 下载相关数据</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 调用前已经装载; 1：从文件装载</returns>
        public int LoadQuickPinyin(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            // 优化
            if (this.QuickPinyin != null)
                return 0;

        REDO:

            try
            {
                this.QuickPinyin = new QuickPinyin(this.DataDir + "\\pinyin.xml");
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地拼音文件发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("pinyin.xml",
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "装载本地拼音文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// 装载卡特表信息
        /// </summary>
        /// <param name="bAutoDownload">是否自动从 dp2003.com 下载相关数据</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 调用前已经装载; 1：从文件装载</returns>
        public int LoadQuickCutter(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            // 优化
            if (this.QuickCutter != null)
                return 0;

        REDO:

            try
            {
                this.QuickCutter = new QuickCutter(this.DataDir + "\\cutter.xml");
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地卡特表文件发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("cutter.xml",
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "装载本地卡特表文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// 装载四角号码信息
        /// </summary>
        /// <param name="bAutoDownload">是否自动从 dp2003.com 下载相关数据</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 调用前已经装载; 1：从文件装载</returns>
        public int LoadQuickSjhm(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            // 优化
            if (this.QuickSjhm != null)
                return 0;

        REDO:

            try
            {
                this.QuickSjhm = new QuickSjhm(this.DataDir + "\\sjhm.xml");
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地四角号码文件发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("sjhm.xml",
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "装载本地四角号码文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// 装载 ISBN 切割信息
        /// </summary>
        /// <param name="bAutoDownload">是否自动从 dp2003.com 下载相关数据</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 调用前已经装载; 1：从文件装载</returns>
        public int LoadIsbnSplitter(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            // 优化
            if (this.IsbnSplitter != null)
                return 0;

        REDO:

            try
            {
                this.IsbnSplitter = new IsbnSplitter(this.DataDir + "\\rangemessage.xml");  // "\\isbn.xml"
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地 isbn 规则文件 rangemessage.xml 发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("rangemessage.xml",    // "isbn.xml"
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "装载本地isbn规则文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }

#if NO
        // 
        /// <summary>
        /// 获得ISBN实用库的库名
        /// </summary>
        /// <returns>ISBN实用库的库名</returns>
        public string GetPublisherUtilDbName()
        {
            if (this.UtilDbProperties == null)
                return null;    // not found

            for (int i = 0; i < this.UtilDbProperties.Count; i++)
            {
                UtilDbProperty property = this.UtilDbProperties[i];

                if (property.Type == "publisher")
                    return property.DbName;
            }

            return null;    // not found
        }
#endif
        /// <summary>
        /// 获得一个特定类型的实用库的库名
        /// </summary>
        /// <param name="strType">类型</param>
        /// <returns>实用库的库名</returns>
        public string GetUtilDbName(string strType)
        {
            if (this.UtilDbProperties == null)
                return null;    // not found

            for (int i = 0; i < this.UtilDbProperties.Count; i++)
            {
                UtilDbProperty property = this.UtilDbProperties[i];

                if (property.Type == strType)
                    return property.DbName;
            }

            return null;    // not found
        }


        // 从ISBN号中取得出版社号部分
        // 本函数可以自动适应有978前缀的新型ISBN号
        // ISBN号中无横杠时本函数会自动先加横杠然后再取得出版社号
        // parameters:
        //      strPublisherNumber  出版社号码。不包含978-部分
        /// <summary>
        /// 从 ISBN 号中取得出版社号部分
        /// 本函数可以自动适应有 978 前缀的新型 ISBN 号
        /// ISBN 号中无横杠时本函数会自动先加横杠然后再取得出版社号
        /// </summary>
        /// <param name="strISBN">ISBN 号字符串</param>
        /// <param name="strPublisherNumber">返回出版社号码部分。不包含 978- 部分</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int GetPublisherNumber(string strISBN,
            out string strPublisherNumber,
            out string strError)
        {
            strPublisherNumber = "";
            strError = "";

            int nRet = strISBN.IndexOf("-");
            if (nRet == -1)
            {

                nRet = this.LoadIsbnSplitter(true, out strError);
                if (nRet == -1)
                {
                    strError = "在取出版社号前，发现ISBN号中没有横杠，在加入横杠的过程中，出现错误: " + strError;
                    return -1;
                }

                string strResult = "";

                nRet = this.IsbnSplitter.IsbnInsertHyphen(strISBN,
                    "force10",  // 用于出版社号码的ISBN，不关心978前缀
                    out strResult,
                    out strError);
                if (nRet == -1)
                {
                    strError = "在取出版社号前，发现ISBN号中没有横杠，在加入横杠的过程中，出现错误: " + strError;
                    return -1;
                }

                strISBN = strResult;
            }

            return Global.GetPublisherNumber(strISBN,
                out strPublisherNumber,
                out strError);
        }

        // 保存分割条位置
        internal void SaveSplitterPos(SplitContainer container,
            string strSection,
            string strEntry)
        {
            if (this.AppInfo == null)
                return;

            if (container.ParentForm != null
                && container.ParentForm.WindowState == FormWindowState.Minimized)
            {
                container.ParentForm.WindowState = FormWindowState.Normal;  // 2012/3/16
                // TODO: 直接返回，不保存？
                // Debug.Assert(false, "SaveSplitterPos()应当在窗口为非Minimized状态下调用");
            }

            float fValue = (float)container.SplitterDistance /
                (
                container.Orientation == Orientation.Horizontal ?
                (float)container.Height
                :
                (float)container.Width
                )
                ;
            this.AppInfo.SetFloat(
                strSection,
                strEntry,
                fValue);

        }

        // 获得并设置分割条位置
        internal void LoadSplitterPos(SplitContainer container,
            string strSection,
            string strEntry)
        {
            float fValue = this.AppInfo.GetFloat(
                strSection,
                strEntry,
                (float)0);
            if (fValue == 0)
                return;

            try
            {
                container.SplitterDistance = (int)Math.Ceiling(
                (
                container.Orientation == Orientation.Horizontal ?
                (float)container.Height
                :
                (float)container.Width
                )
                * fValue);
            }
            catch
            {
            }
        }

        // 出纳借还操作的API是否需要返回item xml数据？
        // 这是为了PrintHost中根据item xml实现多种功能的需要，而打开。否则出于效率考虑，关闭
        // 目前还没有必要出现在配置面板上
        /// <summary>
        /// 出纳借还操作的API是否需要返回item xml数据？
        /// 这是为了PrintHost中根据item xml实现多种功能的需要，而打开。否则出于效率考虑，关闭
        /// </summary>
        public bool ChargingNeedReturnItemXml
        {
            get
            {
                // return true;
                return this.AppInfo.GetBoolean("charging",
                    "need_return_item_xml",
                    false);
            }
            set
            {
                this.AppInfo.SetBoolean("charging",
                    "need_return_item_xml",
                    value);
            }
        }

#if NO
        private void timer_operHistory_Tick(object sender, EventArgs e)
        {
            this.OperHistory.OnTimer();
        }
#endif

        // 
        // return:
        //      null    装载失败
        //      其他    装载(或发现)成功
        /// <summary>
        /// 观察当前是否有符合指定路径的 EntityForm 已经打开，如果没有则新打开一个
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <returns>EntityForm 对象。如果为 null 则表示打开失败</returns>
        public EntityForm GetEntityForm(string strBiblioRecPath)
        {
            for (int i = 0; i < this.MdiChildren.Length; i++)
            {
                Form child = this.MdiChildren[i];

                if (child is EntityForm)
                {
                    EntityForm entity_form = (EntityForm)child;
                    if (entity_form.BiblioRecPath == strBiblioRecPath)
                        return entity_form;
                }
            }

            EntityForm form = new EntityForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();

            // return:
            //      -1  出错。已经用MessageBox报错
            //      0   没有装载
            //      1   成功装载
            //      2   通道被占用
            int nRet = form.LoadRecordOld(strBiblioRecPath,
                "",
                true);
            if (nRet != 1)
            {
                form.Close();
                return null;
            }

            return form;
        }
#if NO
        // 返回图书馆应用服务器虚拟目录URL
        // 注: 这是根据前端本地配置推算出来的
        // 例如“http://test111/dp2libraryws”
        public string LibraryServerDir
        {
            get
            {
                string strLibraryServerUrl = this.AppInfo.GetString(
                    "config",
                    "circulation_server_url",
                    "");
                int pos = strLibraryServerUrl.LastIndexOf("/");
                if (pos != -1)
                    return strLibraryServerUrl.Substring(0, pos);

                return strLibraryServerUrl;
            }
        }
#endif
        // 返回图书馆应用服务器虚拟目录URL
        // 注: 这是根据前端本地配置推算出来的
        // 例如“http://test111/dp2library”
        internal string LibraryServerDir1
        {
            get
            {
                string strLibraryServerUrl = this.AppInfo.GetString(
                    "config",
                    "circulation_server_url",
                    "");

                return strLibraryServerUrl;
            }
        }

        // 
        /// <summary>
        /// GCAT通用汉语著者号码表 WebService URL
        /// 缺省为 http://dp2003.com/gcatserver/
        /// </summary>
        public string GcatServerUrl
        {
            get
            {
                return this.AppInfo.GetString("config",
                    "gcat_server_url",
                    "http://dp2003.com/gcatserver/");
            }
        }

        /// <summary>
        /// 拼音服务器 URL。
        /// 缺省为 http://dp2003.com/gcatserver/
        /// </summary>
        public string PinyinServerUrl
        {
            get
            {
                return this.AppInfo.GetString("config",
                    "pinyin_server_url",
                    "http://dp2003.com/gcatserver/");
            }
        }

        /// <summary>
        /// 是否要临时改用本地拼音。此状态退出时不会被记忆
        /// </summary>
        public bool ForceUseLocalPinyinFunc = false;    // 是否要临时改用本地拼音。此状态退出时不会被记忆

        private void MenuItem_clearDatabaseInfoCatch_Click(object sender, EventArgs e)
        {
            bool bEnabled = this.MenuItem_clearDatabaseInfoCatch.Enabled;
            this.MenuItem_clearDatabaseInfoCatch.Enabled = false;
            try
            {
                //// this.Channel.Close();   // 迫使通信通道重新登录

                // 重新获得各种库名、列表
                this.StartPrepareNames(false, false);
            }
            finally
            {
                this.MenuItem_clearDatabaseInfoCatch.Enabled = bEnabled;
            }
        }

        // 标签打印窗
        private void MenuItem_openLabelPrintForm_Click(object sender, EventArgs e)
        {
#if NO
            LabelPrintForm form = new LabelPrintForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<LabelPrintForm>();

        }

        // 卡片打印窗
        private void MenuItem_openCardPrintForm_Click(object sender, EventArgs e)
        {
#if NO
            CardPrintForm form = new CardPrintForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<CardPrintForm>();

        }

        // 
        /// <summary>
        /// 在数据目录下获得临时文件名
        /// </summary>
        /// <param name="strDirName">临时文件目录名</param>
        /// <param name="strFilenamePrefix">临时文件名前缀字符串</param>
        /// <returns>临时文件名</returns>
        public string NewTempFilename(string strDirName,
            string strFilenamePrefix)
        {
            string strFilePath = "";
            int nRedoCount = 0;
            string strDir = PathUtil.MergePath(this.DataDir, strDirName);
            PathUtil.CreateDirIfNeed(strDir);
            for (int i = 0; ; i++)
            {
                strFilePath = PathUtil.MergePath(strDir, strFilenamePrefix + (i + 1).ToString());
                if (File.Exists(strFilePath) == false)
                {
                    // 创建一个0字节的文件
                    try
                    {
                        File.Create(strFilePath).Close();
                    }
                    catch (Exception/* ex*/)
                    {
                        if (nRedoCount > 10)
                        {
                            string strError = "创建文件 '" + strFilePath + "' 失败...";
                            throw new Exception(strError);
                        }
                        nRedoCount++;
                        continue;
                    }
                    break;
                }
            }

            return strFilePath;
        }

        private void toolStrip_main_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void toolStrip_main_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strWhole = (String)e.Data.GetData("Text");

            string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1)
            {
                strError = "连一行也不存在";
                goto ERROR1;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string strFirstLine = lines[i].Trim();

                if (String.IsNullOrEmpty(strFirstLine) == true)
                    continue;

                // 取得recpath
                string strRecPath = "";
                int nRet = strFirstLine.IndexOf("\t");
                if (nRet == -1)
                    strRecPath = strFirstLine;
                else
                    strRecPath = strFirstLine.Substring(0, nRet).Trim();

                // 判断它是书目记录路径，还是实体记录路径？
                string strDbName = Global.GetDbName(strRecPath);

                if (this.IsBiblioDbName(strDbName) == true)
                {
                    EntityForm form = new EntityForm();
                    form.MdiParent = this;
                    form.MainForm = this;
                    form.Show();

                    form.LoadRecordOld(strRecPath,
                        "",
                        true);
                }
                else if (this.IsItemDbName(strDbName) == true)
                {
                    // TODO: 需要改进为，如果在当前已经打开的EntityForm中找到册记录路径，就不另行打开窗口了。仅仅选中相应的listview行即可

                    EntityForm form = new EntityForm();
                    form.MdiParent = this;
                    form.MainForm = this;
                    form.Show();

                    form.LoadItemByRecPath(strRecPath,
                        false);
                }
                else if (this.IsReaderDbName(strDbName) == true)
                {
                    ReaderInfoForm form = new ReaderInfoForm();
                    form.MdiParent = this;
                    form.MainForm = this;
                    form.Show();

                    form.LoadRecordByRecPath(strRecPath,
                        "");
                }
                else
                {
                    strError = "记录路径 '" + strRecPath + "' 中的数据库名既不是书目库名，也不是实体库名...";
                    goto ERROR1;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除数据目录下全部临时文件
        // 在软件启动的时候调用
        void DeleteAllTempFiles(string strDataDir)
        {
            // 出让控制权
            Application.DoEvents();

            DirectoryInfo di = new DirectoryInfo(strDataDir);

            if (string.IsNullOrEmpty(di.Name) == false
                && di.Name[0] == '~')
            {
                try
                {
                    di.Delete(true);
                }
                catch
                {
                    // goto DELETE_FILES;
                }

                return;
            }

            // DELETE_FILES:
            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                string strFileName = fis[i].Name;
                if (strFileName.Length > 0
                    && strFileName[0] == '~')
                {
                    Stop.SetMessage("正在删除 " + fis[i].FullName);
                    try
                    {
                        File.Delete(fis[i].FullName);
                    }
                    catch
                    {
                    }
                }
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                DeleteAllTempFiles(dis[i].FullName);
            }
        }

        private void MenuItem_openTestSearch_Click(object sender, EventArgs e)
        {
#if NO
            TestSearchForm form = new TestSearchForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<TestSearchForm>();

        }


        private void toolStripButton_close_Click(object sender, EventArgs e)
        {
            this.PanelFixedVisible = false;
        }

        private void toolButton_refresh_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is EntityForm)
            {
                ((EntityForm)this.ActiveMdiChild).Reload();
            }
            if (this.ActiveMdiChild is ChargingForm)
            {
                ((ChargingForm)this.ActiveMdiChild).Reload();
            }
            if (this.ActiveMdiChild is ItemInfoForm)
            {
                ((ItemInfoForm)this.ActiveMdiChild).Reload();
            }
        }

        private void MenuItem_utility_Click(object sender, EventArgs e)
        {
#if NO
            UtilityForm form = new UtilityForm();

            // form.MainForm = this;
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<UtilityForm>();

        }

        // 
        /// <summary>
        /// 当前固定面板是否具备显示“属性”的条件
        /// </summary>
        /// <returns>是否</returns>
        public bool CanDisplayItemProperty()
        {
            if (this.PanelFixedVisible == false)
                return false;
            if (this.tabControl_panelFixed.SelectedTab != this.tabPage_property)
                return false;

            return true;
        }

        /// <summary>
        /// 获得固定面板区域属性的标题
        /// </summary>
        /// <returns>标题文字</returns>
        public string GetItemPropertyTitle()
        {
            if (this.m_propertyViewer == null)
                return null;

            return this.m_propertyViewer.Text;
        }

        /// <summary>
        /// 在固定面板区域的“属性”属性页显示信息
        /// </summary>
        /// <param name="strTitle">标题文字</param>
        /// <param name="strHtml">HTML 字符串</param>
        /// <param name="strXml">XML 字符串</param>
        public void DisplayItemProperty(string strTitle,
    string strHtml,
    string strXml)
        {
            if (this.CanDisplayItemProperty() == false)
                return;

            bool bNew = false;
            if (this.m_propertyViewer == null)
            {
                m_propertyViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_propertyViewer, this.Font, false);
                bNew = true;
            }

            // 优化
            if (m_propertyViewer.Text == strTitle)
                return;

            m_propertyViewer.MainForm = this;  // 必须是第一句

            if (string.IsNullOrEmpty(strTitle) == true
                && string.IsNullOrEmpty(strHtml) == true
                && string.IsNullOrEmpty(strXml) == true)
            {
                this.m_propertyViewer.Clear();
                this.m_propertyViewer.Text = "";
            }
            else
            {
                if (bNew == true)
                    m_propertyViewer.InitialWebBrowser();

                m_propertyViewer.SuppressScriptErrors = true;   // 调试的时候设置为 false

                m_propertyViewer.Text = strTitle;
                m_propertyViewer.HtmlString = strHtml;
                m_propertyViewer.XmlString = strXml;
            }


            // 
            if (this.CurrentPropertyControl != m_propertyViewer.MainControl)
                m_propertyViewer.DoDock(false); // 不会自动显示FixedPanel
        }


        // 从磁盘更新全部方案
        private void MenuItem_updateStatisProjectsFromDisk_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nUpdateCount = 0;
            int nRet = 0;

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定方案所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            // dir_dlg.SelectedPath = this.textBox_outputFolder.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            bool bHideMessageBox = false;
            bool bDontUpdate = false;

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    bNewOpened = true;
                    Application.DoEvents();
                }

                try
                {
                    // return:
                    //      -2  全部放弃
                    //      -1  出错
                    //      >=0 更新数
                    nRet = UpdateProjects(form,
                        dir_dlg.SelectedPath,
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;
                    nUpdateCount += nRet;
                }
                finally
                {
                    if (bNewOpened == true)
                        form.Close();
                }
            }

            // 凭条打印
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this.OperHistory,
                    dir_dlg.SelectedPath,
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            // MainForm
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this,
                    dir_dlg.SelectedPath,
                    ref bHideMessageBox,
                    ref bDontUpdate,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            if (nUpdateCount > 0)
                MessageBox.Show(this, "共更新 " + nUpdateCount.ToString() + " 个方案");
            else
                MessageBox.Show(this, "没有发现更新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            UpdateStatisProjectsFromDisk();
        }

        // 从磁盘安装全部方案
        private void MenuItem_installStatisProjectsFromDisk_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = -1;
            int nInstallCount = 0;

            bool bDebugger = false;
            if (Control.ModifierKeys == Keys.Control)
                bDebugger = true;

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定方案所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            // dir_dlg.SelectedPath = this.textBox_outputFolder.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            // this.textBox_outputFolder.Text = dir_dlg.SelectedPath;


            // 寻找 projects.xml 文件
            string strLocalFileName = PathUtil.MergePath(dir_dlg.SelectedPath, "projects.xml");
            if (File.Exists(strLocalFileName) == false)
            {
                // strError = "您所指定的目录 '" + dir_dlg.SelectedPath + "' 中并没有包含 projects.xml 文件，无法进行安装";
                // goto ERROR1;

                // 如果没有 projects.xml 文件，则搜索全部 *.projpack 文件，并创建好一个临时的 ~projects.xml文件
                strLocalFileName = PathUtil.MergePath(this.DataDir, "~projects.xml");
                nRet = ScriptManager.BuildProjectsFile(dir_dlg.SelectedPath,
                    strLocalFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 列出已经安装的方案的URL
            List<string> installed_urls = new List<string>();
            List<Form> newly_opened_forms = new List<Form>();
            List<Form> forms = new List<Form>();

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                // bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    newly_opened_forms.Add(form);
                    Application.DoEvents();
                }

                forms.Add(form);

                dynamic o = form;
                List<string> urls = new List<string>();
                nRet = o.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 凭条打印
            {
                List<string> urls = new List<string>();
                nRet = this.OperHistory.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 框架窗口
            {
                List<string> urls = new List<string>();
                nRet = this.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            try
            {
                SelectInstallProjectsDialog dlg = new SelectInstallProjectsDialog();
                MainForm.SetControlFont(dlg, this.DefaultFont);
                dlg.XmlFilename = strLocalFileName;
                dlg.InstalledUrls = installed_urls;
                if (bDebugger == true)
                    dlg.Category = "debugger";
                dlg.StartPosition = FormStartPosition.CenterScreen;

                this.AppInfo.LinkFormState(dlg,
                    "SelectInstallProjectsDialog_state");
                dlg.ShowDialog(this);
                this.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // 分宿主进行安装
                foreach (Form form in forms)
                {
                    // 为一个统计窗安装若干方案
                    // parameters:
                    //      projects    待安装的方案。注意有可能包含不适合安装到本窗口的方案
                    // return:
                    //      -1  出错
                    //      >=0 安装的方案数
                    nRet = InstallProjects(
                        form,
                        GetWindowName(form),
                        dlg.SelectedProjects,
                        dir_dlg.SelectedPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // 凭条打印
                {
                    nRet = InstallProjects(
    this.OperHistory,
    "凭条打印",
    dlg.SelectedProjects,
    dir_dlg.SelectedPath,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // MainForm
                {
                    nRet = InstallProjects(
    this,
    "框架窗口",
    dlg.SelectedProjects,
    dir_dlg.SelectedPath,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }
            }
            finally
            {
                // 关闭本次新打开的窗口
                foreach (Form form in newly_opened_forms)
                {
                    form.Close();
                }
            }

            MessageBox.Show(this, "共安装方案 " + nInstallCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            InstallStatisProjectsFromDisk();
        }

        // 从 dp2003.com 安装全部方案
        private void MenuItem_installStatisProjects_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = -1;
            int nInstallCount = 0;

            bool bDebugger = false;
            if (Control.ModifierKeys == Keys.Control)
                bDebugger = true;

            // 下载projects.xml文件
            string strLocalFileName = PathUtil.MergePath(this.DataDir, "~temp_projects.xml");
            string strTempFileName = PathUtil.MergePath(this.DataDir, "~temp_download_projects.xml");

            try
            {
                File.Delete(strLocalFileName);
            }
            catch
            {
            }
            try
            {
                File.Delete(strTempFileName);
            }
            catch
            {
            }

            nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
                "http://dp2003.com/dp2circulation/projects/projects.xml",
                strLocalFileName,
                strTempFileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 列出已经安装的方案的URL
            List<string> installed_urls = new List<string>();
            List<Form> newly_opened_forms = new List<Form>();
            List<Form> forms = new List<Form>();

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                // bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    newly_opened_forms.Add(form);
                    Application.DoEvents();
                }

                forms.Add(form);

                dynamic o = form;
                List<string> urls = new List<string>();
                nRet = o.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 凭条打印
            {
                List<string> urls = new List<string>();
                nRet = this.OperHistory.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 框架窗口
            {
                List<string> urls = new List<string>();
                nRet = this.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            try
            {
                SelectInstallProjectsDialog dlg = new SelectInstallProjectsDialog();
                MainForm.SetControlFont(dlg, this.DefaultFont);
                dlg.XmlFilename = strLocalFileName;
                dlg.InstalledUrls = installed_urls;
                if (bDebugger == true)
                    dlg.Category = "debugger";
                dlg.StartPosition = FormStartPosition.CenterScreen;

                this.AppInfo.LinkFormState(dlg,
                    "SelectInstallProjectsDialog_state");
                dlg.ShowDialog(this);
                this.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // 分宿主进行安装
                foreach (Form form in forms)
                {
                    // 为一个统计窗安装若干方案
                    // parameters:
                    //      projects    待安装的方案。注意有可能包含不适合安装到本窗口的方案
                    // return:
                    //      -1  出错
                    //      >=0 安装的方案数
                    nRet = InstallProjects(
                        form,
                        GetWindowName(form),
                        dlg.SelectedProjects,
                        "!url",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // 凭条打印
                {
                    nRet = InstallProjects(
    this.OperHistory,
    "凭条打印",
    dlg.SelectedProjects,
                        "!url",
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // MainForm
                {
                    nRet = InstallProjects(
    this,
    "框架窗口",
    dlg.SelectedProjects,
                        "!url",
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }
            }
            finally
            {
                // 关闭本次新打开的窗口
                foreach (Form form in newly_opened_forms)
                {
                    form.Close();
                }
            }

            MessageBox.Show(this, "共安装方案 " + nInstallCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            InstallStatisProjects();
        }

        // 从 dp2003.com 检查更新全部方案
        private void MenuItem_updateStatisProjects_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nUpdateCount = 0;
            int nRet = 0;

            bool bHideMessageBox = false;
            bool bDontUpdate = false;

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    bNewOpened = true;
                    Application.DoEvents();
                }

                try
                {
                    // return:
                    //      -2  全部放弃
                    //      -1  出错
                    //      >=0 更新数
                    nRet = UpdateProjects(form,
                        "!url",
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;
                    nUpdateCount += nRet;
                }
                finally
                {
                    if (bNewOpened == true)
                        form.Close();
                }
            }

            // 凭条打印
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this.OperHistory,
                        "!url",
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            // MainForm
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this,
                        "!url",
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            if (nUpdateCount > 0)
                MessageBox.Show(this, "共更新 " + nUpdateCount.ToString() + " 个方案");
            else
                MessageBox.Show(this, "没有发现更新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            UpdateStatisProjects();
        }

#if NO
        private void button_test_Click(object sender, EventArgs e)
        {
            ScrollToEnd(this.OperHistory.WebBrowser);
        }

        public static void ScrollToEnd(WebBrowser webBrowser)
        {
            /*
            HtmlDocument doc = webBrowser.Document;
            doc.Body.ScrollIntoView(false);
             * */
            /*
            webBrowser.Focus();
            System.Windows.Forms.SendKeys.Send("{PGDN}");
             * */
            webBrowser.Document.Window.ScrollTo(0,
                webBrowser.Document.Body.ScrollRectangle.Height);
        }
#endif


        // 2012/3/25
        private void ToolStripMenuItem_stopAll_Click(object sender, EventArgs e)
        {
            stopManager.DoStopAll(null);
        }

        // 
        /// <summary>
        /// 准备打印机配置对象。如果在系统配置中没有找到信息，则新创建一个PrinterInfo对象
        /// </summary>
        /// <param name="strType">类型</param>
        /// <returns>PrinterInfo对象，打印机配置信息</returns>
        public PrinterInfo PreparePrinterInfo(string strType)
        {
            PrinterInfo info = this.GetPrinterInfo(strType);
            if (info != null)
                return info;
            info = new PrinterInfo();
            info.Type = strType;
            return info;
        }

        // 
        /// <summary>
        /// 获得一种特定类型的打印机配置
        /// </summary>
        /// <param name="strType">类型</param>
        /// <returns>PrinterInfo对象</returns>
        internal PrinterInfo GetPrinterInfo(string strType)
        {
            string strText = this.AppInfo.GetString("printerInfo",
                strType,
                "");
            if (string.IsNullOrEmpty(strText) == true)
                return null;    // not found
            return new PrinterInfo(strType, strText);
        }

        // 
        /// <summary>
        /// 保存一种特定类型的打印机配置
        /// </summary>
        /// <param name="strType">类型</param>
        /// <param name="info">打印机配置信息</param>
        public void SavePrinterInfo(string strType,
            PrinterInfo info)
        {
            if (info == null)
            {
                this.AppInfo.SetString("printerInfo",
                    strType,
                    null);
                return;
            }

            this.AppInfo.SetString("printerInfo",
                strType,
                info.GetText());
        }

        private void MenuItem_displayFixPanel_Click(object sender, EventArgs e)
        {
            this.PanelFixedVisible = !this.PanelFixedVisible;
        }

        // 打开一个订购统计窗
        private void MenuItem_openOrderStatisForm_Click(object sender, EventArgs e)
        {
#if NO
            OrderStatisForm form = new OrderStatisForm();

            // form.MainForm = this;
            // form.DbType = "order";
            form.MdiParent = this;
            form.Show();
#endif
            OpenWindow<OrderStatisForm>();

        }

        /// <summary>
        /// 指纹本地缓存目录
        /// </summary>
        public string FingerPrintCacheDir
        {
            get
            {
                // string strDir = PathUtil.MergePath(this.MainForm.DataDir, "fingerprintcache");
                return PathUtil.MergePath(this.UserDir, "fingerprintcache");   // 2013/6/16
            }
        }

        /// <summary>
        /// 操作日志本地缓存目录
        /// </summary>
        public string OperLogCacheDir
        {
            get
            {
                // return PathUtil.MergePath(this.DataDir, "operlogcache");
                // return PathUtil.MergePath(this.UserDir, "operlogcache");    // 2013/6/16
                return Path.Combine(this.ServerCfgDir, "operlogcache"); // 2015/6/20
            }
        }

        /// <summary>
        /// 是否自动缓存操作日志
        /// </summary>
        public bool AutoCacheOperlogFile
        {
            get
            {
                // 自动缓存日志文件
                return
                    this.AppInfo.GetBoolean(
                    "global",
                    "auto_cache_operlogfile",
                    true);
            }
        }

        /// <summary>
        /// 加拼音时自动选择多音字
        /// </summary>
        public bool AutoSelPinyin
        {
            get
            {
                return this.AppInfo.GetBoolean(
                    "global",
                    "auto_select_pinyin",
                    false);
            }
        }

        // 保存封面扫描的原始图像
        public bool SaveOriginCoverImage
        {
            get
            {
                return this.AppInfo.GetBoolean(
                    "global",
                    "save_orign_cover_image",
                    false);
            }
            set
            {
                this.AppInfo.SetBoolean(
                    "global",
                    "save_orign_cover_image",
                    value);
            }
        }


        // 初始化指纹缓存
        private void MenuItem_initFingerprintCache_Click(object sender, EventArgs e)
        {
            ReaderSearchForm form = new ReaderSearchForm();
            form.FingerPrintMode = true;
            form.MdiParent = this;
            form.Show();

            string strError = "";
            // return:
            //      -2  remoting服务器连接失败。驱动程序尚未启动
            //      -1  出错
            //      0   成功
            int nRet = form.InitFingerprintCache(false, out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;
            form.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            form.Close();
        }

        // 首次初始化指纹缓存。如果接口程序尚未启动，则不报错。这是因为缺省就有remoting server的URL配置，很可能用户根本不是要配置接口程序的意思
        void FirstInitialFingerprintCache()
        {
            string strError = "";

            // 没有配置 指纹阅读器接口URL 参数，就没有必要进行初始化
            if (string.IsNullOrEmpty(this.FingerprintReaderUrl) == true)
                return;

            ReaderSearchForm form = new ReaderSearchForm();
            form.FingerPrintMode = true;
            form.MdiParent = this;
            form.Opacity = 0;
            form.Show();
            form.Update();

            // TODO: 显示正在初始化，不要关闭窗口
            // return:
            //      -2  remoting服务器连接失败。驱动程序尚未启动
            //      -1  出错
            //      0   成功
            int nRet = form.InitFingerprintCache(true, out strError);
            if (nRet == -1 || nRet == -2)
            {
                strError = "初始化指纹缓存失败: " + strError;
                goto ERROR1;
            }
            form.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            form.Close();
        }

        // 
        /// <summary>
        /// 身份证读卡器 URL
        /// </summary>
        public string IdcardReaderUrl
        {
            get
            {
                return this.AppInfo.GetString("cardreader",
                    "idcardReaderUrl",
                    "");  // 常用值 "ipc://IdcardChannel/IdcardServer"
            }
        }

        // 
        /// <summary>
        /// 指纹阅读器 URL
        /// </summary>
        public string FingerprintReaderUrl
        {
            get
            {
                return this.AppInfo.GetString("fingerprint",
                    "fingerPrintReaderUrl",
                    "");  // 常用值 "ipc://FingerprintChannel/FingerprintServer"
            }
        }

        // 
        /// <summary>
        /// 指纹代理帐户 用户名
        /// </summary>
        public string FingerprintUserName
        {
            get
            {
                return this.AppInfo.GetString("fingerprint",
                    "userName",
                    "");
            }
            set
            {
                this.AppInfo.SetString("fingerprint",
                    "userName",
                    value);
            }
        }

        // 
        /// <summary>
        /// 指纹代理帐户 密码
        /// </summary>
        internal string FingerprintPassword
        {
            get
            {
                string strPassword = this.AppInfo.GetString("fingerprint",
                    "password",
                    "");
                return this.DecryptPasssword(strPassword);
            }
            set
            {
                string strPassword = this.EncryptPassword(value);
                this.AppInfo.SetString(
                    "fingerprint",
                    "password",
                    strPassword);
            }
        }

        // 固定面板中 Page 选定页
        private void tabControl_panelFixed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.FixedSelectedPageChanged != null)
                this.FixedSelectedPageChanged(this, e);
        }


        private void MenuItem_openUserFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 
        /// <summary>
        /// 日志获取详细级别。0 最详细 1 简略 2 最简略
        /// </summary>
        public int OperLogLevel
        {
            get
            {
                string strText = this.AppInfo.GetString(
                    "operlog_form",
                    "level",
                    "1 -- 简略");
                string strNumber = StringUtil.GetLeft(strText);
                int v = 0;
                Int32.TryParse(strNumber, out v);
                return v;
            }
        }

        private void MenuItem_closeAllMdiWindows_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                form.Close();
            }
        }

        /// <summary>
        /// 朗读
        /// </summary>
        /// <param name="strText">要朗读的文本</param>
        public void Speak(string strText)
        {
            this.m_speech.SpeakAsyncCancelAll();
            this.m_speech.SpeakAsync(strText);
            // MessageBox.Show(this, strText);
        }

        private void toolStripMenuItem_fixedPanel_clear_Click(object sender, EventArgs e)
        {
            if (this.tabControl_panelFixed.SelectedTab == this.tabPage_history)
            {
                if (this.OperHistory != null)
                    this.OperHistory.ClearHtml();
            }
            else if (this.tabControl_panelFixed.SelectedTab == this.tabPage_camera)
            {
                if (this.qrRecognitionControl1 != null)
                    this.qrRecognitionControl1.CurrentCamera = "";
            }
        }

        delegate object Delegate_InvokeScript(WebBrowser webBrowser,
            string strFuncName, object[] args);

        public object InvokeScript(
            WebBrowser webBrowser,
            string strFuncName,
            object[] args)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    Delegate_InvokeScript d = new Delegate_InvokeScript(InvokeScript);
                    return this.Invoke(d, new object[] { webBrowser, strFuncName, args });
                }

                return webBrowser.Document.InvokeScript(strFuncName, args);
            }
            catch
            {
                return null;
            }
        }

        public void BeginInvokeScript(
    WebBrowser webBrowser,
    string strFuncName,
    object[] args)
        {

            try
            {
                Delegate_InvokeScript d = new Delegate_InvokeScript(InvokeScript);
                this.BeginInvoke(d, new object[] { webBrowser, strFuncName, args });
            }
            catch
            {
            }
        }

        private void MenuItem_inventory_Click(object sender, EventArgs e)
        {
            if (StringUtil.CompareVersion(this.ServerVersion, "2.50") < 0)
            {
                MessageBox.Show(this, "dp2library 版本 2.50 和以上才能使用 盘点窗");
                return;
            }
            OpenWindow<InventoryForm>();
        }

        private void MenuItem_importExport_Click(object sender, EventArgs e)
        {
            if (StringUtil.CompareVersion(this.ServerVersion, "2.93") < 0)
            {
                MessageBox.Show(this, "dp2library 2.93 及以上版本才能使用 从书目转储文件导入窗");
                return;
            }

            OpenWindow<ImportExportForm>();
        }

        private void tabControl_panelFixed_SizeChanged(object sender, EventArgs e)
        {
            if (this.qrRecognitionControl1 != null)
            {
                this.qrRecognitionControl1.PerformAutoScale();
                this.qrRecognitionControl1.PerformLayout();
            }
        }

        private void contextMenuStrip_fixedPanel_Opening(object sender, CancelEventArgs e)
        {
            // 固定面板 验收属性页 上下文菜单不出现。因为内嵌的功能自己处理了
            if (this.tabControl_panelFixed.SelectedTab == this.tabPage_accept)
                e.Cancel = true;
        }

        private void tabPage_accept_Enter(object sender, EventArgs e)
        {
            if (this._acceptForm != null)
                this._acceptForm.EnableProgress();
        }

        private void tabPage_accept_Leave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
#if NO
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
#endif

            if (keyData == Keys.Enter)
            {
                if (this.tabControl_panelFixed.SelectedTab == this.tabPage_accept
                    && this._acceptForm != null)
                    this._acceptForm.DoEnterKey();

                // return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void MenuItem_messageForm_Click(object sender, EventArgs e)
        {
            OpenWindow<MessageForm>();
        }

        #region 序列号机制

        bool _testMode = false;

        public bool TestMode
        {
            get
            {
                return this._testMode;
            }
            set
            {
                this._testMode = value;
                SetTitle();
            }
        }

        bool _communityMode = false;

        public bool CommunityMode
        {
            get
            {
                return this._communityMode;
            }
            set
            {
                this._communityMode = value;
                SetTitle();
            }
        }

        void SetTitle()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(SetTitle));
                return;
            }

            if (this.TestMode == true)
                this.Text = "dp2Circulation V2 -- 内务 [评估模式]";
            else if (this.CommunityMode == true)
                this.Text = "dp2Circulation V2 -- 内务 [社区版]";
            else
                this.Text = "dp2Circulation V2 -- 内务 [专业版]";
        }

#if SN
        // 将本地字符串匹配序列号
        bool MatchLocalString(string strSerialNumber)
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            foreach (string mac in macs)
            {
                string strLocalString = GetEnvironmentString(mac);
                string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                    return true;
            }

            // 2014/12/19
            if (DateTime.Now.Month == 12)
            {
                foreach (string mac in macs)
                {
                    string strLocalString = GetEnvironmentString(mac, true);
                    string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                    if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                        return true;
                }
            }

            return false;
        }

        // parameters:
        //      strRequirFuncList   要求必须具备的功能列表。逗号间隔的字符串
        //      bReinput    如果序列号不满足要求，是否直接出现对话框让用户重新输入序列号
        // return:
        //      -1  出错
        //      0   正确
        internal int VerifySerialCode(string strRequirFuncList,
            bool bReinput,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 2014/11/15
            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");

            // 首次运行
            if (string.IsNullOrEmpty(strSerialCode) == true)
            {
            }

        REDO_VERIFY:

            if (strSerialCode == "test")
            {
                if (string.IsNullOrEmpty(strRequirFuncList) == true)
                {
                    this.TestMode = true;
                    this.CommunityMode = false;
                    // 覆盖写入 运行模式 信息，防止用户作弊
                    // 小型版没有对应的评估模式
                    this.AppInfo.SetString("main_form", "last_mode", "test");
                    return 0;
                }
            }
            else if (strSerialCode == "community")
            {
                if (string.IsNullOrEmpty(strRequirFuncList) == true)
                {
                    this.TestMode = false;
                    this.CommunityMode = true;
                    this.AppInfo.SetString("main_form", "last_mode", "community");
                    return 0;
                }
            }
            else
            {
                this.TestMode = false;
                this.CommunityMode = false;
                this.AppInfo.SetString("main_form", "last_mode", "standard");
            }

            //string strLocalString = GetEnvironmentString();

            //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");

            if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false ||
                // strSha1 != GetCheckCode(strSerialCode)
                    MatchLocalString(strSerialCode) == false
                    || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (bReinput == false)
                {
                    strError = "序列号无效";
                    return -1;
                }

                if (String.IsNullOrEmpty(strSerialCode) == false)
                    MessageBox.Show(this, "序列号无效。请重新输入");
                else if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false)
                    MessageBox.Show(this, "序列号中 function 参数无效。请重新输入");

                // 出现设置序列号对话框
                nRet = ResetSerialCode(
                    false,
                    strSerialCode,
                    GetEnvironmentString(strFirstMac));
                if (nRet == 0)
                {
                    strError = "放弃";
                    return -1;
                }
                strSerialCode = this.AppInfo.GetString("sn", "sn", "");
                goto REDO_VERIFY;
            }
            return 0;
        }

        // return:
        //      false   不满足
        //      true    满足
        bool CheckFunction(string strEnvString,
            string strFuncList)
        {
            Hashtable table = StringUtil.ParseParameters(strEnvString);
            string strFuncValue = (string)table["function"];
            string[] parts = strFuncList.Split(new char[] { ',' });
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part) == true)
                    continue;
                if (StringUtil.IsInList(part, strFuncValue) == false)
                    return false;
            }

            return true;
        }

        // parameters:
        string GetEnvironmentString(string strMAC,
            bool bNextYear = false)
        {
            Hashtable table = new Hashtable();
            table["mac"] = strMAC;  // SerialCodeForm.GetMacAddress();
            // table["time"] = GetTimeRange();
            if (bNextYear == false)
                table["time"] = SerialCodeForm.GetTimeRange();
            else
                table["time"] = SerialCodeForm.GetNextYearTimeRange();

            table["product"] = "dp2circulation";

            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
            // 将 strSerialCode 中的扩展参数设定到 table 中
            SerialCodeForm.SetExtParams(ref table, strSerialCode);
#if NO
            if (string.IsNullOrEmpty(strSerialCode) == false)
            {
                string strExtParam = GetExtParams(strSerialCode);
                if (string.IsNullOrEmpty(strExtParam) == false)
                {
                    Hashtable ext_table = StringUtil.ParseParameters(strExtParam);
                    string function = (string)ext_table["function"];
                    if (string.IsNullOrEmpty(function) == false)
                        table["function"] = function;
                }
            }
#endif

            return StringUtil.BuildParameterString(table);
        }

        // 获得 xxx|||xxxx 的左边部分
        static string GetCheckCode(string strSerialCode)
        {
            string strSN = "";
            string strExtParam = "";
            StringUtil.ParseTwoPart(strSerialCode,
                "|||",
                out strSN,
                out strExtParam);

            return strSN;
        }

        // 获得 xxx|||xxxx 的右边部分
        static string GetExtParams(string strSerialCode)
        {
            string strSN = "";
            string strExtParam = "";
            StringUtil.ParseTwoPart(strSerialCode,
                "|||",
                out strSN,
                out strExtParam);

            return strExtParam;
        }

#if NO
        static string GetTimeRange()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString().PadLeft(4, '0');
        }
#endif

        string CopyrightKey = "dp2circulation_sn_key";

        // return:
        //      0   Cancel
        //      1   OK
        int ResetSerialCode(
            bool bAllowSetBlank,
            string strOldSerialCode,
            string strOriginCode)
        {
            _expireVersionChecked = false;

            Hashtable ext_table = StringUtil.ParseParameters(strOriginCode);
            string strMAC = (string)ext_table["mac"];
            if (string.IsNullOrEmpty(strMAC) == true)
                strOriginCode = "!error";
            else
                strOriginCode = Cryptography.Encrypt(strOriginCode,
                    this.CopyrightKey);
            SerialCodeForm dlg = new SerialCodeForm();
            dlg.Font = this.Font;
            dlg.DefaultCodes = new List<string>(new string[] { "community|社区版" });
            dlg.SerialCode = strOldSerialCode;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.OriginCode = strOriginCode;

        REDO:
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            if (string.IsNullOrEmpty(dlg.SerialCode) == true)
            {
                if (bAllowSetBlank == true)
                {
                    DialogResult result = MessageBox.Show(this,
        "确实要将序列号设置为空?\r\n\r\n(一旦将序列号设置为空，dp2Circulation 将自动退出，下次启动需要重新设置序列号)",
        "dp2Circulation",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        return 0;
                    }
                }
                else
                {
                    MessageBox.Show(this, "序列号不允许为空。请重新设置");
                    goto REDO;
                }
            }

            this.AppInfo.SetString("sn", "sn", dlg.SerialCode);
            this.AppInfo.Save();
            return 1;
        }

#endif

        #endregion

        private void MenuItem_resetSerialCode_Click(object sender, EventArgs e)
        {
#if SN
            string strError = "";
            int nRet = 0;

            // 2014/11/15
            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            string strRequirFuncList = "";  // 因为这里是设置通用的序列号，不具体针对哪个功能，所以对设置后，序列号的功能不做检查。只有等到用到具体功能的时候，才能发现序列号是否包含具体功能的 function = ... 参数

            string strSerialCode = "";
        REDO_VERIFY:

            if (strSerialCode == "test")
            {
                this.TestMode = true;
                this.CommunityMode = false;
                // 覆盖写入 运行模式 信息，防止用户作弊
                // 小型版没有对应的评估模式
                this.AppInfo.SetString("main_form", "last_mode", "test");
                return;
            }
            else if (strSerialCode == "community")
            {
                this.TestMode = false;
                this.CommunityMode = true;
                this.AppInfo.SetString("main_form", "last_mode", "community");
                return;
            }
            else
            {
                this.TestMode = false;
                this.CommunityMode = false;
                this.AppInfo.SetString("main_form", "last_mode", "standard");
            }

            //string strLocalString = GetEnvironmentString();

            //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");

            if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false ||
                // strSha1 != GetCheckCode(strSerialCode) 
                    MatchLocalString(strSerialCode) == false
                    || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (String.IsNullOrEmpty(strSerialCode) == false)
                    MessageBox.Show(this, "序列号无效。请重新输入");
                else if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false)
                    MessageBox.Show(this, "序列号中 function 参数无效。请重新输入");


                // 出现设置序列号对话框
                nRet = ResetSerialCode(
                    true,
                    strSerialCode,
                    GetEnvironmentString(strFirstMac));
                if (nRet == 0)
                {
                    strError = "放弃";
                    goto ERROR1;
                }
                strSerialCode = this.AppInfo.GetString("sn", "sn", "");
                if (string.IsNullOrEmpty(strSerialCode) == true)
                {
                    // Application.Exit();
                    Program.PromptAndExit(null, "放弃设置序列号");
                    return;
                }

                this.AppInfo.Save();
                goto REDO_VERIFY;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_openEntityRegisterWizard_Click(object sender, EventArgs e)
        {
            if (StringUtil.CompareVersion(this.ServerVersion, "2.48") < 0)
            {
                MessageBox.Show(this, "dp2library 版本 2.48 和以上才能使用 册登记窗");
                return;
            }
            OpenWindow<EntityRegisterWizard>();
        }

        private void MenuItem_reLogin_Click(object sender, EventArgs e)
        {
            StartPrepareNames(true, false);
        }

        // 获得一个临时文件名。文件并未创建
        public string GetTempFileName(string strPrefix)
        {
            return Path.Combine(this.UserTempDir, "~" + strPrefix + Guid.NewGuid().ToString());
        }

        #region servers.xml

        // HnbUrl.HnbUrl

#if NO
        static string _baseCfg = @"
<root>
  <server name='红泥巴.数字平台中心' type='dp2library' url='http://123.103.13.236/dp2library' userName='public'/>
  <server name='亚马逊中国' type='amazon' url='webservices.amazon.cn'/>
</root>";
#endif

        static string _baseCfg = @"
<root>
  <server name='红泥巴.数字平台中心' type='dp2library' url='http://hnbclub.cn/dp2library' userName='public'/>
  <server name='亚马逊中国' type='amazon' url='webservices.amazon.cn'/>
</root>";

        // servers.xml 版本号
        // 0.01 2014/12/10
        // 0.02 2015/6/15 access 属性中增加 delete 值类型
        public const double SERVERSXML_VERSION = 0.02;

        // 创建 servers.xml 配置文件
        public int BuildServersCfgFile(string strCfgFileName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(_baseCfg);

                dom.DocumentElement.SetAttribute("version", SERVERSXML_VERSION.ToString());

                // 添加当前服务器
                {
                    XmlElement server = dom.CreateElement("server");
                    if (dom.DocumentElement.FirstChild != null)
                        dom.DocumentElement.InsertBefore(server, dom.DocumentElement.FirstChild);   // 插入到最前面
                    else
                        dom.DocumentElement.AppendChild(server);

                    server.SetAttribute("name", "当前服务器");
                    server.SetAttribute("type", "dp2library");
                    server.SetAttribute("url", ".");
                    server.SetAttribute("userName", ".");

                    int nCount = 0;
                    // 添加 database 元素
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        // USMARC 格式的，和期刊库，都跳过
                        if (prop.Syntax != "unimarc"
                            || string.IsNullOrEmpty(prop.IssueDbName) == false)
                            continue;

                        // TODO: 临时库和中央库要有一个属性表示区别。将来在确定目标的算法中可能有用

                        // 临时库或中央库
                        if (StringUtil.IsInList("catalogWork", prop.Role) == true
                            || StringUtil.IsInList("catalogTarget", prop.Role) == true)
                        {
                            XmlElement database = dom.CreateElement("database");
                            server.AppendChild(database);

                            string strBiblioAccess = "";
                            string strEntityAccess = "";

#if NO
                            nRet = this.PrepareSearch(true);
                            if (nRet == 0)
                            {
                                strError = "PrepareSearch() error";
                                return -1;
                            }
#endif
                            LibraryChannel channel = this.GetChannel();

                            try
                            {
                                nRet = EntityRegisterWizard.DetectAccess(channel,
                    this.Stop,
                    prop.DbName,
                    prop.Syntax,
                    out strBiblioAccess,
                    out strEntityAccess,
                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "在探测书目库 " + prop.DbName + " 读写权限的过程中出错: " + strError;
                                    return -1;
                                }
                            }
                            finally
                            {
                                this.ReturnChannel(channel);
                                // this.EndSearch();
                            }

                            database.SetAttribute("name", prop.DbName);
                            database.SetAttribute("isTarget", "yes");
                            database.SetAttribute("access", strBiblioAccess);  // "append,overwrite"

                            database.SetAttribute("entityAccess", strEntityAccess);    // "append,overwrite"
                            nCount++;
                        }

#if NO
                    // 中央库
                    if ()
                    {
                        XmlElement database = dom.CreateElement("database");
                        server.AppendChild(database);

                        database.SetAttribute("name", prop.DbName);
                        database.SetAttribute("isTarget", "yes");
                        database.SetAttribute("access", "append,overwrite");

                        database.SetAttribute("entityAccess", "append,overwrite");
                        nCount++;
                    }
#endif
                    }

                    if (nCount == 0)
                    {
                        strError = "当前服务器 (" + this.LibraryServerUrl + ") 尚未定义角色为 catalogWork 或 catalogTarget 的图书书目库。创建服务器配置文件失败";
                        return -1;
                    }
                }

                string strHnbUrl = "";
                {
                    XmlElement server = dom.DocumentElement.SelectSingleNode("server[@name='红泥巴.数字平台中心']") as XmlElement;
                    if (server != null)
                        strHnbUrl = server.GetAttribute("url");
#if NO
                    // 当前服务器是红泥巴服务器，要删除多余的事项
                    // TODO: 判断两个 URL 是否相等的时候，需要用 DNS 得到 IP 进行比较
                    if (ServerDlg.IsSameUrl(this.LibraryServerUrl, strHnbUrl) == true)
                    // if (string.Compare(this.LibraryServerUrl, strHnbUrl, true) == 0)
                    {
                        server.ParentNode.RemoveChild(server);
                    }

#endif

                    // return:
                    //      -1  出错
                    //      0   不是同一个服务器
                    //      1   是同一个服务器
                    nRet = IsSameDp2libraryServer(this.LibraryServerUrl, strHnbUrl, out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        // TODO: 为当前服务器节点增加注释，表示实际上它就是红泥巴服务器
                        server.ParentNode.RemoveChild(server);
                    }
                }

                dom.Save(strCfgFileName);
                return 0;
            }
            catch (Exception ex)
            {
                strError = "MainForm BuildServersCfgFile() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
        }

        // 判断两个 URL 是否指向的是同一个 dp2library 服务器
        // 算法是，从 dp2library 服务器获得它的 uid，然后比较
        // return:
        //      -1  出错
        //      0   不是同一个服务器
        //      1   是同一个服务器
        public int IsSameDp2libraryServer(string strUrl1,
            string strUrl2,
            out string strError)
        {
            strError = "";

            string strUID1 = "";
            string strUID2 = "";
            int nRet = GetDp2libraryServerUID(null, strUrl1, out strUID1, out strError);
            if (nRet == -1)
            {
                strError = "获得服务器 " + strUrl1 + " 的 UID 时出错: " + strError;
                return -1;
            }
            if (string.IsNullOrEmpty(strUID1) == true)
            {
                strError = "获得服务器 " + strUrl1 + " 的 UID 时出错: UID 为空";
                return -1;
            }
            nRet = GetDp2libraryServerUID(null, strUrl2, out strUID2, out strError);
            if (nRet == -1)
            {
                strError = "获得服务器 " + strUrl2 + " 的 UID 时出错: " + strError;
                return -1;
            }
            if (string.IsNullOrEmpty(strUID2) == true)
            {
                strError = "获得服务器 " + strUrl2 + " 的 UID 时出错: UID 为空";
                return -1;
            }

            if (strUID1 != strUID2)
                return 0;
            return 1;
        }

        public static int GetDp2libraryServerUID(Stop stop,
            string strURL,
            out string strUID,
            out string strError)
        {
            strError = "";
            strUID = "";

            LibraryChannel channel = new LibraryChannel();
            try
            {
                channel.Url = strURL;
                string strVersion = "";
                long lRet = channel.GetVersion(stop, out strVersion, out strUID, out strError);
                if (lRet == -1)
                    return -1;
                return 0;
            }
            finally
            {
                channel.Close();
            }
        }

        // 获得配置文件的版本号
        public static double GetServersCfgFileVersion(string strCfgFileName)
        {
            if (File.Exists(strCfgFileName) == false)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFileName);
            }
            catch (Exception)
            {
                return 0;
            }

            if (dom.DocumentElement == null)
                return 0;

            double version = 0;
            string strVersion = dom.DocumentElement.GetAttribute("version");
            if (double.TryParse(strVersion, out version) == false)
                return 0;

            return version;
        }

        // 获得当前用户的用户名
        public string GetCurrentUserName()
        {
#if NO
            if (this.Channel != null && string.IsNullOrEmpty(this.Channel.UserName) == false)
                return this.Channel.UserName;
#endif
            if (string.IsNullOrEmpty(this._currentUserName) == false)
                return this._currentUserName;

            // TODO: 或者迫使登录一次
            if (this.AppInfo != null)
                return AppInfo.GetString(
                    "default_account",
                    "username",
                    "");

            return "";
        }

        // 最近登录过的用户的权限
        public string GetCurrentUserRights()
        {
            return this._currentUserRights;
        }

        #endregion // servers.xml

        private void MainForm_Resize(object sender, EventArgs e)
        {

        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
#if NO
            foreach (Form form in this.OwnedForms)
            {
                if (form is LineLayerForm)
                {
                    if (form.TopMost == false)
                    form.TopMost = true;
                }
            }
#endif
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
#if NO
            foreach (Form form in this.OwnedForms)
            {
                if (form is LineLayerForm)
                {
                    if (form.TopMost == true)
                        form.TopMost = false;
                }
            }
#endif
        }

        #region 消息过滤

#if NO
        public event MessageFilterEventHandler MessageFilter = null;

        // Creates a  message filter.
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public class MouseLButtonMessageFilter : IMessageFilter
        {
            public MainForm MainForm = null;
            public bool PreFilterMessage(ref Message m)
            {
                // Blocks all the messages relating to the left mouse button. 
                if (m.Msg >= 513 && m.Msg <= 515)
                {
                    if (this.MainForm.MessageFilter != null)
                    {
                        MessageFilterEventArgs e = new MessageFilterEventArgs();
                        e.Message = m;
                        this.MainForm.MessageFilter(this, e);
                        m = e.Message;
                        return e.ReturnValue;
                    }
                }
                return false;
            }
        }

#endif

        #endregion

        /// <summary>
        /// 获得当前 dp2library 服务器相关的本地配置目录路径。这是在用户目录中用 URL 映射出来的子目录名
        /// </summary>
        public string ServerCfgDir
        {
            get
            {
                string strServerUrl = ReportForm.GetValidPathString(this.LibraryServerUrl.Replace("/", "_"));
                string strDirectory = Path.Combine(this.UserDir, "servers\\" + strServerUrl);
                PathUtil.CreateDirIfNeed(strDirectory);
                return strDirectory;
            }
        }

        private void MenuItem_openArrivedSearchForm_Click(object sender, EventArgs e)
        {
            if (StringUtil.CompareVersion(this.ServerVersion, "2.47") < 0)
            {
                MessageBox.Show(this, "dp2library 版本 2.47 和以上才能使用 预约到书查询窗");
                return;
            }
            OpenWindow<ArrivedSearchForm>();
        }

        private void MenuItem_openReservationListForm_Click(object sender, EventArgs e)
        {
            if (StringUtil.CompareVersion(this.ServerVersion, "2.47") < 0)
            {
                MessageBox.Show(this, "dp2library 版本 2.47 和以上才能使用 预约响应窗");
                return;
            }
            OpenWindow<ReservationListForm>();
        }

        // 写入日志文件。每天创建一个单独的日志文件
        public void WriteErrorLog(string strText)
        {
            if (string.IsNullOrEmpty(this.UserLogDir) == true)
                throw new ArgumentException("this.UserLogDir 不应为空");

            FileUtil.WriteErrorLog(
                this.UserLogDir,
                this.UserLogDir,
                strText,
                "log_",
                ".txt");
        }

        // 打包错误日志
        private void menuItem_packageErrorLog_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;

            Stop temp_stop = new DigitalPlatform.Stop();
            temp_stop.Register(stopManager, true);	// 和容器关联

            temp_stop.OnStop += new StopEventHandler(this.DoStop);
            temp_stop.Initial("正在打包事件日志信息 ...");
            temp_stop.BeginLoop();
            this.EnableControls(false);

            try
            {
                string strTempDir = Path.Combine(this.UserTempDir, "~zip_events");
                PathUtil.CreateDirIfNeed(strTempDir);

                string strZipFileName = Path.Combine(strTempDir, "dp2circulation_eventlog.zip");

                List<EventLog> logs = new List<EventLog>();

                // logs.Add(new EventLog("DigitalPlatform", ".", "*"));
                logs.Add(new EventLog("Application"));

                // "最近31天" "最近十年" "最近七天"

                nRet = PackageEventLog.Package(logs,
                    strZipFileName,
                    bControl ? "最近十年" : "最近31天",
                    this.UserDir,
                    strTempDir,
                    (strText) =>
                    {
                        Application.DoEvents();

                        if (strText != null)
                            temp_stop.SetMessage(strText);

                        if (temp_stop != null && temp_stop.State != 0)
                            return false;
                        return true;
                    },
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                try
                {
                    System.Diagnostics.Process.Start(strTempDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
            finally
            {
                this.EnableControls(true);
                temp_stop.EndLoop();
                temp_stop.OnStop -= new StopEventHandler(this.DoStop);
                temp_stop.Initial("");

                if (temp_stop != null) // 脱离关联
                {
                    temp_stop.Unregister(true);
                    temp_stop = null;
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_chatForm_Click(object sender, EventArgs e)
        {
            OpenWindow<ChatForm>();
        }

        private void menuItem_updateDp2circulation_Click(object sender, EventArgs e)
        {
            BeginUpdateClickOnceApplication();
        }

        // 从磁盘升级
        private void MenuItem_upgradeFromDisk_Click(object sender, EventArgs e)
        {
            UpgradeGreenFromDisk();
        }

        private void MenuItem_openMarc856SearchForm_Click(object sender, EventArgs e)
        {
            OpenWindow<Marc856SearchForm>();
        }

        private void MenuItem_startAnotherDp2circulation_Click(object sender, EventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Program.ReleaseMutex();
                StartClickOnceDp2circulation();
                return;
            }

            string strFileName = Assembly.GetExecutingAssembly().CodeBase;
            var processInfo = new ProcessStartInfo(strFileName);

            List<string> args = StringUtil.GetCommandLineArgs();
            args.Add("newinstance");
            args.Add("green");  // 不顾 Ctrl 键状态强制用绿色方式运行

            // 调试用 Program.ReleaseMutex();

            processInfo.UseShellExecute = true;
            // processInfo.Verb = "runas";
            processInfo.Arguments = StringUtil.MakePathList(args, " ");
            processInfo.WorkingDirectory = Path.GetDirectoryName(strFileName);

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "dp2circulation 新实例启动失败: " + ex.Message);
            }
        }

        // 启动 ClickOnce 方式安装的 dp2circulation
        int StartClickOnceDp2circulation()
        {
            try
            {
#if NO
                string strUrl = "http://dp2003.com/dp2circulation/v2/dp2circulation.application";
                Process.Start(strUrl);
#endif

                string strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V2/dp2内务 V2");
                if (File.Exists(strShortcutFilePath) == false)
                {
                    return 0;
                }
                else
                {
                    Process.Start(strShortcutFilePath);
                    return 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "dp2circulation 启动失败" + ex.Message);
                return -1;
            }
        }

        private void MenuItem_createGreenApplication_Click(object sender, EventArgs e)
        {
            // TODO: 需要加入判断，如果当前已经是绿色位置启动的，就隐藏此菜单
            Task.Factory.StartNew(() => CopyGreen(true));
        }

        // 获得 MARC HTML 对照显示的头部字符串
        public string GetMarcHtmlHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(this.DataDir, "operloghtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";
        }

        // 打开 CommentViewer 窗口
        public void OpenCommentViewer(bool bOpenWindow)
        {
            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (this.CanDisplayItemProperty() == false)
                    return;
            }

            bool bNew = false;
            if (this.m_commentViewer == null
                || (bOpenWindow == true && this.m_commentViewer.Visible == false))
            {
                bNew = true;
                m_commentViewer = new CommentViewerForm();
                m_commentViewer.MainForm = this;  // 必须是第一句

                m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
                m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);

                MainForm.SetControlFont(m_commentViewer, this.Font, false);
                m_commentViewer.InitialWebBrowser();
            }

            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    this.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_commentViewer.WindowState == FormWindowState.Minimized)
                        m_commentViewer.WindowState = FormWindowState.Normal;
                    m_commentViewer.Activate();
                }
            }
            else
            {
                if (m_commentViewer.Visible == true)
                {

                }
                else
                {
                    if (this.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
        }

        void marc_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                // this.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        // 点对点通讯的用户管理功能
        private void toolStripButton_messageHub_userManage_Click(object sender, EventArgs e)
        {
            UserManageDialog dlg = new UserManageDialog();
            MainForm.SetControlFont(dlg, this.DefaultFont);
            dlg.Connection = this.MessageHub;
            dlg.ShowDialog(this);

            if (dlg.Changed == true)
            {
                this.MessageHub.CloseConnection();

                this.MessageHub.Connect();
                // this.MessageHub.Login();
            }
        }

        private void toolStripButton_messageHub_relogin_Click(object sender, EventArgs e)
        {
            this.MessageHub.CloseConnection();

            this.MessageHub.RefreshUserName();
            this.MessageHub.Connect();
            // this.MessageHub.Login();
        }

        private void MenuItem_refreshLibraryUID_Click(object sender, EventArgs e)
        {
            this.ServerUID = Guid.NewGuid().ToString();
        }

    }

    /// <summary>
    /// 消息过滤事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void MessageFilterEventHandler(object sender,
    MessageFilterEventArgs e);

    /// <summary>
    /// 消息过滤事件的参数
    /// </summary>
    public class MessageFilterEventArgs : EventArgs
    {
        public Message Message;  // [in][out]
        public bool ReturnValue = false;    // true 表示要吞掉这个消息， false 表示不干扰这个消息
    }

    [Flags]
    internal enum InputType
    {
        None = 0,
        QR = 0x01,             // QR 码
        PQR = 0x02,             // PRQ: 引导的 QR 码
        EAN_BARCODE = 0x04,    // EAN (包括 ISBN) 条码。注意，不是 QR 码
        NORMAL_BARCODE = 0x08,    // 普通 1D 条码
        ALL = (QR | PQR | EAN_BARCODE | NORMAL_BARCODE)  // 所有类型的 Mask
    }

    // 
    /// <summary>
    /// 实用库属性
    /// </summary>
    public class UtilDbProperty
    {
        /// <summary>
        /// 数据库名
        /// </summary>
        public string DbName = "";  // 库名

        /// <summary>
        /// 类型
        /// </summary>
        public string Type = "";  // 类型，用途
    }

    // 
    /// <summary>
    /// 书目库属性
    /// </summary>
    public class BiblioDbProperty
    {
        /// <summary>
        /// 书目库名
        /// </summary>
        public string DbName = "";  // 书目库名
        /// <summary>
        /// 格式语法
        /// </summary>
        public string Syntax = "";  // 格式语法

        /// <summary>
        /// 实体库名
        /// </summary>
        public string ItemDbName = "";  // 对应的实体库名

        /// <summary>
        /// 期库名
        /// </summary>
        public string IssueDbName = ""; // 对应的期库名 2007/10/19 

        /// <summary>
        /// 订购库名
        /// </summary>
        public string OrderDbName = ""; // 对应的订购库名 2007/11/30 

        /// <summary>
        /// 评注库名
        /// </summary>
        public string CommentDbName = "";   // 对应的评注库名 2009/10/23 

        /// <summary>
        /// 角色
        /// </summary>
        public string Role = "";    // 角色 2009/10/23 

        /// <summary>
        /// 是否参与流通
        /// </summary>
        public bool InCirculation = true;  // 是否参与流通 2009/10/23 
    }

    // 
    /// <summary>
    /// 读者库属性
    /// </summary>
    public class ReaderDbProperty
    {
        /// <summary>
        /// 读者库名
        /// </summary>
        public string DbName = "";  // 读者库名
        /// <summary>
        /// 是否参与流通
        /// </summary>
        public bool InCirculation = true;  // 是否参与流通
        /// <summary>
        /// 馆代码
        /// </summary>
        public string LibraryCode = ""; // 馆代码
    }

    // 
    /// <summary>
    /// 普通库的属性
    /// </summary>
    public class NormalDbProperty
    {
        /// <summary>
        /// 数据库名
        /// </summary>
        public string DbName = "";

        /// <summary>
        /// 浏览栏目属性集合
        /// </summary>
        public ColumnPropertyCollection ColumnProperties = new ColumnPropertyCollection();
    }

    /// <summary>
    /// 排架体系信息
    /// </summary>
    public class ArrangementInfo
    {
        /// <summary>
        /// 排架体系名
        /// </summary>
        public string ArrangeGroupName = "";
        /// <summary>
        /// 种次号数据库名
        /// </summary>
        public string ZhongcihaoDbname = "";
        /// <summary>
        /// 类号类型
        /// </summary>
        public string ClassType = "";
        /// <summary>
        /// 区分号类型
        /// </summary>
        public string QufenhaoType = "";
        /// <summary>
        /// 索取号类型
        /// </summary>
        public string CallNumberStyle = "";

        /// <summary>
        /// 根据 XmlNode 构造本对象
        /// </summary>
        /// <param name="nodeArrangementGroup">定义节点对象</param>
        public void Fill(XmlNode nodeArrangementGroup)
        {
            this.ArrangeGroupName = DomUtil.GetAttr(nodeArrangementGroup, "name");
            this.ZhongcihaoDbname = DomUtil.GetAttr(nodeArrangementGroup, "zhongcihaodb");
            this.ClassType = DomUtil.GetAttr(nodeArrangementGroup, "classType");
            this.QufenhaoType = DomUtil.GetAttr(nodeArrangementGroup, "qufenhaoType");
            this.CallNumberStyle = DomUtil.GetAttr(nodeArrangementGroup, "callNumberStyle");
        }
    }

    // 
    /// <summary>
    /// 打印机首选配置信息
    /// </summary>
    public class PrinterInfo
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Type = "";
        /// <summary>
        /// 预设缺省打印机名字，记忆选择过的打印机名字
        /// </summary>
        public string PrinterName = "";  // 预设缺省打印机名字，记忆选择过的打印机名字
        /// <summary>
        /// 预设缺省的纸张尺寸名字
        /// </summary>
        public string PaperName = "";   // 预设缺省的纸张尺寸名字

        /// <summary>
        /// 打印纸方向
        /// </summary>
        public bool Landscape = false;  // 打印纸方向

        // 
        /// <summary>
        /// 根据文本表现形式构造
        /// </summary>
        /// <param name="strType">类型</param>
        /// <param name="strText">正文。格式为 printerName=/??;paperName=???</param>
        public PrinterInfo(string strType,
            string strText)
        {
            this.Type = strType;

            Hashtable table = StringUtil.ParseParameters(strText,
                ';',
                '=');
            this.PrinterName = (string)table["printerName"];
            this.PaperName = (string)table["paperName"];
            string strLandscape = (string)table["landscape"];
            if (string.IsNullOrEmpty(strLandscape) == true)
                this.Landscape = false;
            else
                this.Landscape = DomUtil.IsBooleanTrue(strLandscape);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrinterInfo()
        {
        }

        // 
        /// <summary>
        /// 获得文本表现形式
        /// </summary>
        /// <returns>返回文本表现形式。格式为 printerName=/??;paperName=???</returns>
        public string GetText()
        {
            return "printerName=" + this.PrinterName
                + ";paperName=" + this.PaperName
                + (this.Landscape == true ? ";landscape=yes" : "");
        }
    }

    /// <summary>
    /// 全局变量容器
    /// </summary>
    public static class GlobalVars
    {
        /// <summary>
        /// 私有字体集合
        /// </summary>
        public static PrivateFontCollection PrivateFonts = new PrivateFontCollection();
    }

    /// <summary>
    /// 流通 / 内务前端程序 dp2circulation.exe
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}