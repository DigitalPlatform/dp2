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

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.IO;   // DateTimeUtil
using DigitalPlatform.CommonControl;

using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GcatClient.gcat_new_ws;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.MarcDom;
using System.Security.Permissions;

namespace dp2Circulation
{
    /// <summary>
    /// 框架窗口
    /// </summary>
    public partial class MainForm : Form
    {
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

        // int AssemblyVersion = 0;
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
        /*
        public int OperLogStatisAssemblyVersion = 0;
        public int ReaderStatisAssemblyVersion = 0;
        public int ItemStatisAssemblyVersion = 0;
        public int BiblioStatisAssemblyVersion = 0;
        public int XmlStatisAssemblyVersion = 0;
        public int Iso2709StatisAssemblyVersion = 0;
         * */
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

        // 保存界面信息
        /// <summary>
        /// 配置存储
        /// </summary>
        public ApplicationInfo AppInfo = new ApplicationInfo("dp2circulation.xml");

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

        /// <summary>
        /// 通讯通道。MainForm 自己使用
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

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
        /// 构造函数
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            try
            {
                this.qrRecognitionControl1 = new DigitalPlatform.Drawing.QrRecognitionControl();
            }
            catch
            {
            }
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

#if NO
        // defines how far we are extending the Glass margins
        private API.MARGINS margins;

        /// <summary>
        /// Override the OnPaintBackground method, to draw the desired
        /// Glass regions black and display as Glass
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (API.DwmIsCompositionEnabled())
            {
                e.Graphics.Clear(Color.Black);
                // put back the original form background for non-glass area
                Rectangle clientArea = new Rectangle(
                margins.Left,
                margins.Top,
                this.ClientRectangle.Width - margins.Left - margins.Right,
                this.ClientRectangle.Height - margins.Top - margins.Bottom);
                Brush b = new SolidBrush(this.BackColor);
                e.Graphics.FillRectangle(b, clientArea);
            }
        }


        /// <summary>
        /// Use the form padding values to define a Glass margin
        /// </summary>
        private void SetGlassRegion()
        {
            // Set up the glass effect using padding as the defining glass region
            if (API.DwmIsCompositionEnabled())
            {
                Padding padding = new System.Windows.Forms.Padding(50);
                margins = new API.MARGINS();
                margins.Top = padding.Top;
                margins.Left = padding.Left;
                margins.Bottom = padding.Bottom;
                margins.Right = padding.Right;
                API.DwmExtendFrameIntoClientArea(this.Handle, ref margins);
            }
        }
#endif

        private void MainForm_Load(object sender, EventArgs e)
        {
            /*
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.Transparent;
            this.toolStrip_main.BackColor = Color.Transparent;
             * */

            this.SetBevel(false);
#if NO
            if (!API.DwmIsCompositionEnabled())
            {
                //MessageBox.Show("This demo requires Vista, with Aero enabled.");
                //Application.Exit();
            }
            else
            {
                SetGlassRegion();
            }
#endif

            // 获得MdiClient窗口
            {
                Type t = typeof(Form);
                PropertyInfo pi = t.GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic);
                this.MdiClient = (MdiClient)pi.GetValue(this, null);
                this.MdiClient.SizeChanged += new EventHandler(MdiClient_SizeChanged);

                m_backgroundForm = new BackgroundForm();
                m_backgroundForm.MdiParent = this;
                m_backgroundForm.Show();
            }


            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                // MessageBox.Show(this, "no network");
                DataDir = Environment.CurrentDirectory;
            }

            string strError = "";
            int nRet = 0;

            {
                // 2013/6/16
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2Circulation_v2");
                PathUtil.CreateDirIfNeed(this.UserDir);

                this.UserTempDir = Path.Combine(this.UserDir, "temp");
                PathUtil.CreateDirIfNeed(this.UserTempDir);

                // 删除一些以前的目录
                string strDir = PathUtil.MergePath(this.DataDir, "operlogcache");
                if (Directory.Exists(strDir) == true)
                {
                    nRet = Global.DeleteDataDir(
                        this,
                        strDir,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, "删除以前遗留的文件目录时发生错误: " + strError);
                    }
                }
                strDir = PathUtil.MergePath(this.DataDir, "fingerprintcache");
                if (Directory.Exists(strDir) == true)
                {
                    nRet = Global.DeleteDataDir(
                    this,
                    strDir,
                    out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, "删除以前遗留的文件目录时发生错误: " + strError);
                    }
                }
            }

            {
                string strCssUrl = PathUtil.MergePath(this.DataDir, "/background.css");
                string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

                Global.WriteHtml(m_backgroundForm.WebBrowser,
                    "<html><head>" + strLink + "</head><body>");
            }

            // 设置窗口尺寸状态
            if (AppInfo != null)
            {
                // 首次运行，尽量利用“微软雅黑”字体
                if (this.IsFirstRun == true)
                {
                    SetFirstDefaultFont();
                }

                MainForm.SetControlFont(this, this.DefaultFont);

                AppInfo.LoadFormStates(this,
                    "mainformstate",
                    FormWindowState.Maximized);

                // 程序一启动就把这些参数设置为初始状态
                this.DisplayScriptErrorDialog = false;
            }

            InitialFixedPanel();

            // this.Update();   // 优化


            stopManager.Initial(this.toolButton_stop,
                (object)this.toolStripStatusLabel_main,
                (object)this.toolStripProgressBar_main);
            stopManager.OnDisplayMessage += new DisplayMessageEventHandler(stopManager_OnDisplayMessage);
            this.SetMenuItemState();


            // cfgcache
            nRet = cfgCache.Load(this.DataDir
                + "\\cfgcache.xml",
                out strError);
            if (nRet == -1)
            {
                if (IsFirstRun == false)
                    MessageBox.Show(strError);
            }


            cfgCache.TempDir = this.DataDir
                + "\\cfgcache";
            cfgCache.InstantSave = true;

            // 2013/4/12
            // 清除以前残余的文件
            cfgCache.Upgrade();

            // 消除上次程序意外终止时遗留的短期保存密码
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

            StartPrepareNames(true, true);

            this.MdiClient.ClientSizeChanged += new EventHandler(MdiClient_ClientSizeChanged);

            // GuiUtil.RegisterIE9DocMode();

            #region 脚本支持
            ScriptManager.applicationInfo = this.AppInfo;
            ScriptManager.CfgFilePath =
                this.DataDir + "\\mainform_statis_projects.xml";
            ScriptManager.DataDir = this.DataDir;

            ScriptManager.CreateDefaultContent -= new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);
            ScriptManager.CreateDefaultContent += new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);

            try
            {
                ScriptManager.Load();
            }
            catch (FileNotFoundException)
            {
                // 不必报错 2009/2/4 
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
            #endregion


            this.qrRecognitionControl1.Catched += new DigitalPlatform.Drawing.CatchedEventHandler(qrRecognitionControl1_Catched);
            this.qrRecognitionControl1.CurrentCamera = AppInfo.GetString(
                "mainform",
                "current_camera",
                "");
            this.qrRecognitionControl1.EndCatch();  // 一开始的时候并不打开摄像头 2013/5/25

            this.m_strPinyinGcatID = this.AppInfo.GetString("entity_form", "gcat_pinyin_api_id", "");
            this.m_bSavePinyinGcatID = this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", false);

#if NO
            // 2015/5/24
            MouseLButtonMessageFilter filter = new MouseLButtonMessageFilter();
            filter.MainForm = this;
            Application.AddMessageFilter(filter);
#endif

        }

        string m_strPrevMessageText = "";

        void stopManager_OnDisplayMessage(object sender, DisplayMessageEventArgs e)
        {
            if (m_backgroundForm != null)
            {
                if (e.Message != m_strPrevMessageText)
                {
                    m_backgroundForm.AppendHtml(HttpUtility.HtmlEncode(e.Message) + "<br/>");
                    m_strPrevMessageText = e.Message;
                }
            }
        }

        void MdiClient_SizeChanged(object sender, EventArgs e)
        {
            m_backgroundForm.Size = new System.Drawing.Size(this.MdiClient.ClientSize.Width, this.MdiClient.ClientSize.Height);
        }

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
            string strFontFilePath = PathUtil.MergePath(this.DataDir, "b3901.ttf");
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

#if NO
            // 记忆关闭时MDI窗口的Maximized状态
            if (this.ActiveMdiChild != null)
                this.MdiWindowState = this.ActiveMdiChild.WindowState;
#endif
        }

#if NO
        void DisableChildTopMost()
        {
            foreach (Control form in this.Controls)
            {
                if (form.TopMost == true)
                {
                    form.TopMost = false;
                    form.WindowState = FormWindowState.Minimized;
                }
            }
        }
#endif

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (this._acceptForm != null)
            {
                try
                {
                    this._acceptForm.Close();
                    this._acceptForm = null;
                }
                catch
                {
                }
            }
#endif

            if (m_propertyViewer != null)
                m_propertyViewer.Close();

             AppInfo.SetString(
                "mainform",
                "current_camera",
                this.qrRecognitionControl1.CurrentCamera); 
            this.qrRecognitionControl1.Catched -= new DigitalPlatform.Drawing.CatchedEventHandler(qrRecognitionControl1_Catched);

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
            string strError;
            int nRet = cfgCache.Save(null, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);


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

            if (this.m_bSavePinyinGcatID == false)
                this.m_strPinyinGcatID = "";
            this.AppInfo.SetString("entity_form", "gcat_pinyin_api_id", this.m_strPinyinGcatID);
            this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", this.m_bSavePinyinGcatID);

            //记住save,保存信息XML文件
            AppInfo.Save();
            AppInfo = null;	// 避免后面再用这个对象

            if (this.Channel != null)
                this.Channel.Close();   // TODO: 最好限制一个时间，超过这个时间则Abort()

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
            this.qrRecognitionControl1.RefreshDevList();
        }

        /// <summary>
        /// 窗口缺省过程函数
        /// </summary>
        /// <param name="m">消息</param>
        protected override void WndProc(ref Message m)
        {
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

        // 初始化各种参数
        bool InitialProperties(bool bFullInitial, bool bRestoreLastOpenedWindow)
        {
            int nRet = 0;

            // 先禁止界面
            if (bFullInitial == true)
            {
                EnableControls(false);
                this.MdiClient.Enabled = false;
            }

            try
            {
                string strError = "";

                if (bFullInitial == true)
                {
                    // this.Logout(); 

#if NO
                                {
                                    FirstRunDialog first_dialog = new FirstRunDialog();
                                    MainForm.SetControlFont(first_dialog, this.DefaultFont);
                                    first_dialog.MainForm = this;
                                    first_dialog.StartPosition = FormStartPosition.CenterScreen;
                                    if (first_dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        Application.Exit();
                                        return;
                                    }
                                }
#endif

                    bool bFirstDialog = false;

                    // 如果必要，首先出现配置画面，便于配置dp2libraryws的URL
                    string strLibraryServerUrl = this.AppInfo.GetString(
                        "config",
                        "circulation_server_url",
                        "");
                    if (String.IsNullOrEmpty(strLibraryServerUrl) == true)
                    {
                        FirstRunDialog first_dialog = new FirstRunDialog();
                        MainForm.SetControlFont(first_dialog, this.DefaultFont);
                        first_dialog.MainForm = this;
                        first_dialog.StartPosition = FormStartPosition.CenterScreen;
                        if (first_dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                        {
                            Application.Exit();
                            return false;
                        }
                        bFirstDialog = true;

                        // 首次写入 运行模式 信息
                        this.AppInfo.SetString("main_form", "last_mode", first_dialog.Mode);
                        if (first_dialog.Mode == "test")
                        {
                            this.AppInfo.SetString("sn", "sn", "test");
                            this.AppInfo.Save();
                        }
                        else if (first_dialog.Mode == "community")
                        {
                            this.AppInfo.SetString("sn", "sn", "community");
                            this.AppInfo.Save();
                        }
                    }

#if NO
                    // 检查序列号。这里的暂时不要求各种产品功能
                    // DateTime start_day = new DateTime(2014, 10, 15);    // 2014/10/15 以后强制启用序列号功能
                    // if (DateTime.Now >= start_day || IsExistsSerialNumberStatusFile() == true)
                    {
                        // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                        // WriteSerialNumberStatusFile();

                        nRet = this.VerifySerialCode("", true, out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, "dp2Circulation 需要先设置序列号才能使用");
                            Application.Exit();
                            return false;
                        }
                    }
#endif

#if SN
                    {
                        _verified = false;
                        nRet = this.VerifySerialCode("", false, out strError);
                        if (nRet == 0)
                            _verified = true;

                    }
#else
                    this.MenuItem_resetSerialCode.Visible = false;
#endif

                    bool bLogin = this.AppInfo.GetBoolean(
                        "default_account",
                        "occur_per_start",
                        true);
                    if (bLogin == true
                        && bFirstDialog == false)   // 首次运行的对话框出现后，登录对话框就不必出现了
                    {
                        SetDefaultAccount(
                            null,
                            "登录", // "指定缺省帐户",
                            "首次登录", // "请指定后面操作中即将用到的缺省帐户信息。",
                            LoginFailCondition.None,
                            this,
                            false);
                    }
                    else
                    {
                        // 2015/5/15
                        string strServerUrl =
AppInfo.GetString("config",
"circulation_server_url",
"http://localhost:8001/dp2library");

                        if (string.Compare(strServerUrl, CirculationLoginDlg.dp2LibraryXEServerUrl, true) == 0)
                            AutoStartDp2libraryXE();
                    }
                }

                nRet = PrepareSearch();
                if (nRet == 1)
                {
                    try
                    {

                        // 2013/6/18
                        nRet = TouchServer(false);
                        if (nRet == -1)
                            goto END1;

                        // 只有在前一步没有错出的情况下才探测版本号
                        if (nRet == 0)
                        {
                            // 检查dp2Library版本号
                            // return:
                            //      -1  error
                            //      0   dp2Library的版本号过低。警告信息在strError中
                            //      1   dp2Library版本号符合要求
                            nRet = CheckVersion(false, out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                                goto END1;
                            }
                            if (nRet == 0)
                                MessageBox.Show(this, strError);
                        }

                        // 获得书目数据库From信息
                        nRet = GetDbFromInfos(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得全部数据库的定义
                        nRet = GetAllDatabaseInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得书目库属性列表
                        nRet = InitialBiblioDbProperties(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得读者库名列表
                        /*
                        nRet = GetReaderDbNames();
                        if (nRet == -1)
                            goto END1;
                         * */
                        nRet = InitialReaderDbProperties(false);
                        if (nRet == -1)
                            goto END1;

                        nRet = InitialArrivedDbProperties(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得实用库属性列表
                        nRet = GetUtilDbProperties(false);
                        if (nRet == -1)
                            goto END1;

                        // 2008/11/29 
                        nRet = InitialNormalDbProperties(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得图书馆一般信息
                        nRet = GetLibraryInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得索取号配置信息
                        // 2009/2/24 
                        nRet = GetCallNumberInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得前端交费接口配置信息
                        // 2009/7/20 
                        nRet = GetClientFineInterfaceInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得服务器映射到本地的配置文件
                        nRet = GetServerMappedFile(false);
                        if (nRet == -1)
                            goto END1;


                        /*
                        // 检查服务器端library.xml中<libraryserver url="???">配置是否正常
                        // return:
                        //      -1  error
                        //      0   正常
                        //      1   不正常
                        nRet = CheckServerUrl(out strError);
                        if (nRet != 0)
                            MessageBox.Show(this, strError);
                         * */


                        // 核对本地和服务器时钟
                        // return:
                        //      -1  error
                        //      0   没有问题
                        //      1   本地时钟和服务器时钟偏差过大，超过10分钟 strError中有报错信息
                        nRet = CheckServerClock(false, out strError);
                        if (nRet != 0)
                            MessageBox.Show(this, strError);
                    }
                    finally
                    {
                        EndSearch();
                    }
                }

                // 安装条码字体
                InstallBarcodeFont();

            END1:

                Stop = new DigitalPlatform.Stop();
                Stop.Register(stopManager, true);	// 和容器关联
                Stop.SetMessage("正在删除以前遗留的临时文件...");

                DeleteAllTempFiles(this.DataDir);
                DeleteAllTempFiles(this.UserTempDir);

                Stop.SetMessage("正在复制报表配置文件...");
                // 拷贝目录
                nRet = PathUtil.CopyDirectory(Path.Combine(this.DataDir, "report_def"),
                    Path.Combine(this.UserDir, "report_def"),
                    false,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                Stop.SetMessage("");
                if (Stop != null) // 脱离关联
                {
                    Stop.Unregister();	// 和容器关联
                    Stop = null;
                }

                // 2013/12/4
                if (InitialClientScript(out strError) == -1)
                    MessageBox.Show(this, strError);


                // 初始化历史对象，包括C#脚本
                if (this.OperHistory == null)
                {
                    this.OperHistory = new OperHistory();
                    nRet = this.OperHistory.Initial(this,
                        this.webBrowser_history,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);

                    // this.timer_operHistory.Start();

                }

            }
            finally
            {
                // 然后许可界面
                if (bFullInitial == true)
                {
                    this.MdiClient.Enabled = true;
                    EnableControls(true);
                }

                if (this.m_backgroundForm != null)
                {
                    // TODO: 最好有个淡出的功能
                    this.stopManager.OnDisplayMessage += new DisplayMessageEventHandler(stopManager_OnDisplayMessage);
                    this.MdiClient.SizeChanged -= new EventHandler(MdiClient_SizeChanged);
                    this.m_backgroundForm.Close();
                    this.m_backgroundForm = null;
                }
            }

            if (bRestoreLastOpenedWindow == true)
                RestoreLastOpenedMdiWindow();

            if (bFullInitial == true)
            {
#if NO
                // 恢复上次遗留的窗口
                string strOpenedMdiWindow = this.AppInfo.GetString(
                    "main_form",
                    "last_opened_mdi_window",
                    "");

                RestoreLastOpenedMdiWindow(strOpenedMdiWindow);
#endif

                // 初始化指纹高速缓存
                FirstInitialFingerprintCache();

            }
            return true;
        }

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
                strOpenedMdiWindow = "dp2Circulation.ChargingForm";

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

        // 系统参数配置
        private void MenuItem_configuration_Click(object sender, EventArgs e)
        {
            _expireVersionChecked = false;

            string strOldDefaultFontString = this.DefaultFontString;

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

        // 登出
        private void MenuItem_logout_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is EntityForm)
            {
                ((EntityForm)this.ActiveMdiChild).Logout();
            }
        }

        // 打开数据目录文件夹
        private void MenuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
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
                MessageBox.Show(this, ex.Message);
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

            foreach(Form child in this.MdiChildren)
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
            DigitalPlatform.CirculationClient.BeforeLoginEventArgs e)
        {
#if SN
            if (_expireVersionChecked == false)
            {
                double base_version = 2.36;
                string strExpire = GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false
                    && this.ServerVersion < base_version
                    && this.ServerVersion != 0)
                {
                    string strError = "具有失效序列号参数的 dp2Circulation 需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + this.ServerVersion.ToString() + " )。\r\n\r\n请升级 dp2Library 到最新版本，然后重新启动 dp2Circulation。\r\n\r\n点“确定”按钮退出";
                    MessageBox.Show(strError);
                    Application.Exit();
                    return;
                }
                _expireVersionChecked = true;
            }
#endif

            if (e.FirstTry == true)
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

            e.SavePasswordLong = dlg.SavePasswordLong;
            if (e.LibraryServerUrl != dlg.ServerUrl)
            {
                e.LibraryServerUrl = dlg.ServerUrl;
                _expireVersionChecked = false;
            }
        }


        internal void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
#if SN
            LibraryChannel channel = sender as LibraryChannel;
            if (_verified == false && StringUtil.IsInList("serverlicensed", channel.Rights) == false)
            {
                string strError = "";
                int nRet = this.VerifySerialCode("", true, out strError);
                if (nRet == -1)
                {
                    channel.Close();
                    MessageBox.Show(this, "dp2Circulation 需要先设置序列号才能使用");
                    Application.Exit();
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
                return 0;

            this.Channel.Url = this.LibraryServerUrl;

            this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            Stop = new DigitalPlatform.Stop();
            Stop.Register(stopManager, bActivateStop);	// 和容器关联

            return 1;
        }

        /// <summary>
        /// 结束检索
        /// </summary>
        /// <returns>返回 0</returns>
        public int EndSearch(bool bActivateStop = true)
        {
            if (Stop != null) // 脱离关联
            {
                Stop.Unregister(bActivateStop);	// 和容器关联
                Stop = null;
            }

            return 0;
        }

        // 登出
        /// <summary>
        /// MainForm 登出
        /// </summary>
        public void Logout()
        {
            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                return;  // 2009/2/11
            }

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在登出 ...");
            Stop.BeginLoop();

            try
            {
                // string strValue = "";
                long lRet = Channel.Logout(out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 登出时发生错误：" + strError;
                    goto ERROR1;
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                EndSearch();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 试探接触一下服务器
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int TouchServer(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在连接服务器 "+Channel.Url+" ...");
            Stop.BeginLoop();

            try
            {
                string strTime = "";
                long lRet = Channel.GetClock(Stop,
                    out strTime,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                    {
                        // 通讯安全性问题，时钟问题
                        strError = strError + "\r\n\r\n有可能是前端机器时钟和服务器时钟差异过大造成的";
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

        // 0 表示2.1以下。2.1和以上时才具有的获取版本号功能
        /// <summary>
        /// 当前连接的 dp2Library 版本号
        /// </summary>
        public double ServerVersion {get;set;}    // = 0

        /// <summary>
        /// 当前连接的 dp2library 的 uid
        /// </summary>
        public string ServerUID { get; set; }

        // return:
        //      -1  error
        //      0   dp2Library的版本号过低。警告信息在strError中
        //      1   dp2Library版本号符合要求
        /// <summary>
        /// 检查 dp2Library 版本号
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: dp2Library的版本号过低。警告信息在strError中; 1: dp2Library版本号符合要求</returns>
        public int CheckVersion(
            bool bPrepareSearch,
            out string strError)
        {
            strError = "";

            if (bPrepareSearch == true)
            {
                int nRet = PrepareSearch();
                if (nRet == 0)
                {
                    strError = "PrepareSearch() error";
                    return -1;
                }
            }

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检查版本号, 请稍候 ...");
            Stop.BeginLoop();

            try
            {
                string strVersion = "";
                string strUID = "";

                long lRet = Channel.GetVersion(Stop,
    out strVersion,
    out strUID,
    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                    {
                        // 原来的dp2Library不具备GetVersion() API，会走到这里
                        this.ServerVersion = 0;
                        this.ServerUID = "";
                        strError = "当前 dp2Circulation 版本需要和 dp2Library 2.1 或以上版本配套使用 (而当前 dp2Library 版本号为 '2.0或以下' )。请升级 dp2Library 到最新版本。";
                        return 0;
                    }

                    strError = "针对服务器 " + Channel.Url + " 获得版本号的过程发生错误：" + strError;
                    return -1;
                }

                this.ServerUID = strUID;

                double value = 0;

                if (string.IsNullOrEmpty(strVersion) == true)
                {
                    strVersion = "2.0以下";
                    value = 2.0;
                }
                else
                {
                    // 检查最低版本号
                    if (double.TryParse(strVersion, out value) == false)
                    {
                        strError = "dp2Library 版本号 '" + strVersion + "' 格式不正确";
                        return -1;
                    }
                }

                this.ServerVersion = value;

                double base_version = 2.33;
                if (value < base_version)   // 2.12
                {
                    // strError = "当前 dp2Circulation 版本需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + strVersion + " )。\r\n\r\n请尽快升级 dp2Library 到最新版本。";
                    // return 0;
                    strError = "当前 dp2Circulation 版本必须和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + strVersion + " )。\r\n\r\n请立即升级 dp2Library 到最新版本。";
                    this.AppInfo.Save();
                    MessageBox.Show(this, strError);
                    Application.Exit();
                    return -1;
                }

#if SN
                if (this.TestMode == true && this.ServerVersion < 2.34)
                {
                    strError = "dp2Circulation 的评估模式只能在所连接的 dp2library 版本为 2.34 以上时才能使用 (当前 dp2library 版本为 "+this.ServerVersion.ToString()+")";
                    this.AppInfo.Save();
                    MessageBox.Show(this, strError);
                    DialogResult result = MessageBox.Show(this,
    "重设序列号可以脱离评估模式。\r\n\r\n是否要在退出前重设序列号?",
    "dp2Circulation",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        Application.Exit();
                        return -1;
                    }
                    else
                    {
                        MenuItem_resetSerialCode_Click(this, new EventArgs());
                        Application.Exit();
                        return -1;
                    }
                }
#endif
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 1;
        }

        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得 书目库/读者库 的(公共)检索途径
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetDbFromInfos(bool bPrepareSearch = true)
        {
            REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // TODO: 在函数因为无法获得Channel而返回前，是否要清空相关的检索途径数据结构?
            // this.Update();
            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在列检索途径 ...");
            Stop.BeginLoop();

            try
            {
                // 获得书目库的检索途径
                BiblioDbFromInfo[] infos = null;

                long lRet = Channel.ListDbFroms(Stop,
                    "biblio",
                    this.Lang,
                    out infos,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 列出书目库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.BiblioDbFromInfos = infos;

                // 获得读者库的检索途径
                infos = null;
                lRet = Channel.ListDbFroms(Stop,
    "reader",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 列出读者库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                if (infos != null && this.BiblioDbFromInfos != null
                    && infos.Length > 0 && this.BiblioDbFromInfos.Length > 0
                    && infos[0].Caption == this.BiblioDbFromInfos[0].Caption)
                {
                    // 如果第一个元素的caption一样，则说明GetDbFroms API是旧版本的，不支持获取读者库的检索途径功能
                    this.ReaderDbFromInfos = null;
                }
                else
                {
                    this.ReaderDbFromInfos = infos;
                }

                if (this.ServerVersion >= 2.11)
                {
                    // 获得实体库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "item",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出实体库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.ItemDbFromInfos = infos;

                    // 获得期库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "issue",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出期库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.IssueDbFromInfos = infos;

                    // 获得订购库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "order",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出订购库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.OrderDbFromInfos = infos;

                    // 获得评注库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "comment",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出评注库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.CommentDbFromInfos = infos;
                }

                if (this.ServerVersion >= 2.17)
                {
                    // 获得发票库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "invoice",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出发票库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.InvoiceDbFromInfos = infos;

                    // 获得违约金库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "amerce",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出违约金库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.AmerceDbFromInfos = infos;

                }

                if (this.ServerVersion >= 2.47)
                {
                    // 获得预约到书库的检索途径
                    infos = null;
                    lRet = Channel.ListDbFroms(Stop,
        "arrived",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 列出预约到书库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.ArrivedDbFromInfos = infos;
                }

                // 需要检查一下Caption是否有重复(但是style不同)的，如果有，需要修改Caption名
                this.CanonicalizeBiblioFromValues();

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

        // 每个文件第一次请求只返回文件时间, 如果发现有修改后才全部获得文件内容
        // return:
        //      -1  出错
        //      0   本地已经有文件，并且最后修改时间和服务器的一致，因此不必重新获得了
        //      1   获得了文件内容
        int GetSystemFile(string strFileNameParam,
            out string strError)
        {
            strError = "";

            string strFileName = "";
            string strLastTime = "";
            StringUtil.ParseTwoPart(strFileNameParam,
                "|",
                out strFileName,
                out strLastTime);

            Stream stream = null;
            try
            {
                string strServerMappedPath = PathUtil.MergePath(this.DataDir, "servermapped");
                string strLocalFilePath = PathUtil.MergePath(strServerMappedPath, strFileName);
                PathUtil.CreateDirIfNeed(PathUtil.PathPart(strLocalFilePath));

                // 观察本地是否有这个文件，最后修改时间是否和服务器吻合
                if (File.Exists(strLocalFilePath) == true)
                {
                    FileInfo fi = new FileInfo(strLocalFilePath);
                    DateTime local_file_time = fi.LastWriteTimeUtc;

                    if (string.IsNullOrEmpty(strLastTime) == true)
                    {
                        Stop.SetMessage("正在获取系统文件 " + strFileName + " 的最后修改时间 ...");

                        byte[] baContent = null;
                        long lRet = Channel.GetFile(
        Stop,
        "cfgs",
        strFileName,
        -1, // lStart,
        0,  // lLength,
        out baContent,
        out strLastTime,
        out strError);
                        if (lRet == -1)
                            return -1;
                    }

                    if (string.IsNullOrEmpty(strLastTime) == true)
                    {
                        strError = "strLastTime 不应该为空";
                        return -1;
                    }
                    Debug.Assert(string.IsNullOrEmpty(strLastTime) == false, "");

                    DateTime remote_file_time = DateTimeUtil.FromRfc1123DateTimeString(strLastTime);
                    if (local_file_time == remote_file_time)
                        return 0;   // 不必再次获得内容了
                }

            REDO:
                Stop.SetMessage("正在下载系统文件 " + strFileName + " ...");

                string strPrevFileTime = "";
                long lStart = 0;
                long lLength = -1;
                for (; ; )
                {
                    byte[] baContent = null;
                    string strFileTime = "";
                    // 获得系统配置文件
                    // parameters:
                    //      strCategory 文件分类。目前只能使用 cfgs
                    //      lStart  需要获得文件内容的起点。如果为-1，表示(baContent中)不返回文件内容
                    //      lLength 需要获得的从lStart开始算起的byte数。如果为-1，表示希望尽可能多地取得(但是不能保证一定到尾)
                    // rights:
                    //      需要 getsystemparameter 权限
                    // return:
                    //      result.Value    -1 错误；其他 文件的总长度
                    long lRet = Channel.GetFile(
                        Stop,
                        "cfgs",
                        strFileName,
                        lStart,
                        lLength,
                        out baContent,
                        out strFileTime,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (stream == null)
                    {
                        stream = File.Open(
    strLocalFilePath,
    FileMode.Create,
    FileAccess.ReadWrite,
    FileShare.ReadWrite);
                    }

                    // 中途文件时间被修改了
                    if (string.IsNullOrEmpty(strPrevFileTime) == false
                        && strFileTime != strPrevFileTime)
                    {
                        goto REDO;  // 重新下载
                    }

                    if (lRet == 0)
                        return 0;   // 文件长度为0

                    stream.Write(baContent, 0, baContent.Length);
                    lStart += baContent.Length;

                    strPrevFileTime = strFileTime;

                    if (lStart >= lRet)
                        break;  // 整个文件已经下载完毕
                }

                stream.Close();
                stream = null;

                // 修改本地文件时间
                {
                    FileInfo fi = new FileInfo(strLocalFilePath);
                    fi.LastWriteTimeUtc = DateTimeUtil.FromRfc1123DateTimeString(strPrevFileTime);
                }
                return 1;   // 从服务器获得了内容
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        // 删除指定目录下，已知文件以外的其他文件
        /// <summary>
        /// 删除指定目录下，已知文件以外的其他文件
        /// </summary>
        /// <param name="strSourceDir">目录路径</param>
        /// <param name="exclude_filenames">要排除的文件名列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int RemoveFiles(string strSourceDir,
            List<string> exclude_filenames,
            out string strError)
        {
            strError = "";

            try
            {

                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                for (int i = 0; i < subs.Length; i++)
                {
                    if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = RemoveFiles(subs[i].FullName,
                            exclude_filenames,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        continue;
                    }

                    string strFileName = subs[i].FullName.ToLower();

                    if (exclude_filenames.IndexOf(strFileName) == -1)
                    {
                        try
                        {
                            File.Delete(strFileName);
                        }
                        catch
                        {
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return 0;
        }

        // 
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得图书馆一般信息
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetServerMappedFile(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();

            string strError = "";
            int nRet = 0;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得从服务器映射到本地的配置文件 ...");
            Stop.BeginLoop();

            try
            {
                string strServerMappedPath = PathUtil.MergePath(this.DataDir, "servermapped");
                List<string> fullnames = new List<string>();

                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "cfgs",
                    this.ServerVersion >= 2.23 ? "listFileNamesEx" : "listFileNames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得映射配置文件名过程发生错误：" + strError;
                    goto ERROR1;
                }
                if (lRet == 0)
                    goto DELETE_FILES;

                string[] filenames = null;
                
                if (this.ServerVersion >= 2.23)
                    filenames = strValue.Replace("||", "?").Split(new char[] { '?' });
                else
                    filenames = strValue.Split(new char[] { ',' });
                foreach (string filename in filenames)
                {
                    if (string.IsNullOrEmpty(filename) == true)
                        continue;

                    nRet = GetSystemFile(filename,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strFileName = "";
                    string strLastTime = "";
                    StringUtil.ParseTwoPart(filename,
                        "|",
                        out strFileName,
                        out strLastTime);
                    fullnames.Add(Path.Combine(strServerMappedPath, strFileName).ToLower());
                }

                DELETE_FILES:
                // 删除没有用到的文件
                nRet = RemoveFiles(strServerMappedPath,
                    fullnames,
                    out strError);
                if (nRet == -1)
                    return -1;

            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

        // 
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得图书馆一般信息
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetLibraryInfo(bool bPrepareSearch = true)
        {
            REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得图书馆一般信息 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "library",
                    "name",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得图书馆一般信息library/name过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.LibraryName = strValue;

                /*
                lRet = Channel.GetSystemParameter(Stop,
                    "library",
                    "serverDirectory",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得图书馆一般信息library/serverDirectory过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.LibraryServerDiretory = strValue;
                 * */
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
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
            byte [] remote_timestamp,
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

        // 
        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得普通库属性列表，主要是浏览窗栏目标题
        /// 必须在InitialBiblioDbProperties()和InitialReaderDbProperties()以后调用
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int InitialNormalDbProperties(bool bPrepareSearch)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();

            string strError = "";
            int nRet = 0;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得普通库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                this.NormalDbProperties = new List<NormalDbProperty>();

                // 创建NormalDbProperties数组
                if (this.BiblioDbProperties != null)
                {
                    for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                    {
                        BiblioDbProperty biblio = this.BiblioDbProperties[i];

                        NormalDbProperty normal = null;

                        if (String.IsNullOrEmpty(biblio.DbName) == false)
                        {
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.DbName;
                            this.NormalDbProperties.Add(normal);
                        }

                        if (String.IsNullOrEmpty(biblio.ItemDbName) == false)
                        {
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.ItemDbName;
                            this.NormalDbProperties.Add(normal);
                        }

                        // 为什么以前要注释掉?
                        if (String.IsNullOrEmpty(biblio.OrderDbName) == false)
                        {
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.OrderDbName;
                            this.NormalDbProperties.Add(normal);
                        }

                        if (String.IsNullOrEmpty(biblio.IssueDbName) == false)
                        {
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.IssueDbName;
                            this.NormalDbProperties.Add(normal);
                        }

                        if (String.IsNullOrEmpty(biblio.CommentDbName) == false)
                        {
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.CommentDbName;
                            this.NormalDbProperties.Add(normal);
                        }


                    }
                }

                if (this.ReaderDbNames != null)
                {
                    for (int i = 0; i < this.ReaderDbNames.Length; i++)
                    {
                        string strDbName = this.ReaderDbNames[i];

                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        NormalDbProperty normal = null;

                        normal = new NormalDbProperty();
                        normal.DbName = strDbName;
                        this.NormalDbProperties.Add(normal);
                    }
                }

                // 2015/6/13
                if (string.IsNullOrEmpty(this.ArrivedDbName) == false)
                {
                    NormalDbProperty normal = null;
                    normal = new NormalDbProperty();
                    normal.DbName = this.ArrivedDbName;
                    this.NormalDbProperties.Add(normal);
                }

                if (this.ServerVersion >= 2.23)
                {
                    // 构造文件名列表
                    List<string> filenames = new List<string>();
                    for (int i = 0; i < this.NormalDbProperties.Count; i++)
                    {
                        NormalDbProperty normal = this.NormalDbProperties[i];
                        filenames.Add(normal.DbName + "/cfgs/browse");
                    }

                    // 先获得时间戳
                    // TODO: 如果文件太多可以分批获取
                    string strValue = "";
                    long lRet = Channel.GetSystemParameter(Stop,
                        "cfgs/get_res_timestamps",
                        StringUtil.MakePathList(filenames),
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 获得 browse 配置文件时间戳的过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    // 构造时间戳列表
                    Hashtable table = new Hashtable();
                    List<string> results = StringUtil.SplitList(strValue, ',');
                    foreach (string s in results)
                    {
                        string strFileName = "";
                        string strTimestamp = "";

                        StringUtil.ParseTwoPart(s, "|", out strFileName, out strTimestamp);
                        if (string.IsNullOrEmpty(strTimestamp) == true)
                            continue;
                        table[strFileName] = strTimestamp;
                    }

                    // 获得配置文件并处理
                    for (int i = 0; i < this.NormalDbProperties.Count; i++)
                    {
                        NormalDbProperty normal = this.NormalDbProperties[i];

                        normal.ColumnProperties = new ColumnPropertyCollection();

                        string strFileName = normal.DbName + "/cfgs/browse";
                        string strTimestamp = (string)table[strFileName];

                        string strContent = "";
                        byte[] baCfgOutputTimestamp = null;
                        nRet = GetCfgFile(
                            Channel,
                            Stop,
                            normal.DbName,
                            "browse",
                            ByteArray.GetTimeStampByteArray(strTimestamp),
                            out strContent,
                            out baCfgOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strContent);
                        }
                        catch (Exception ex)
                        {
                            strError = "数据库 " + normal.DbName + " 的 browse 配置文件内容装入XMLDOM时出错: " + ex.Message;
                            goto ERROR1;
                        }

                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("//col");
                        foreach (XmlNode node in nodes)
                        {
                            string strColumnType = DomUtil.GetAttr(node, "type");

                            // 2013/10/23
                            string strColumnTitle = dp2ResTree.GetColumnTitle(node,
                                this.Lang);

                            normal.ColumnProperties.Add(strColumnTitle, strColumnType);
                        }
                    }

                }
                else
                {
                    // TODO: 是否缓存这些配置文件? 
                    // 获得 browse 配置文件
                    for (int i = 0; i < this.NormalDbProperties.Count; i++)
                    {
                        NormalDbProperty normal = this.NormalDbProperties[i];

                        normal.ColumnProperties = new ColumnPropertyCollection();

                        string strContent = "";
                        byte[] baCfgOutputTimestamp = null;
                        nRet = GetCfgFile(
                            Channel,
                            Stop,
                            normal.DbName,
                            "browse",
                            null,
                            out strContent,
                            out baCfgOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strContent);
                        }
                        catch (Exception ex)
                        {
                            strError = "数据库 " + normal.DbName + " 的 browse 配置文件内容装入XMLDOM时出错: " + ex.Message;
                            goto ERROR1;
                        }

                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("//col");
                        foreach (XmlNode node in nodes)
                        {
                            string strColumnType = DomUtil.GetAttr(node, "type");

                            // 2013/10/23
                            string strColumnTitle = dp2ResTree.GetColumnTitle(node,
                                this.Lang);

                            normal.ColumnProperties.Add(strColumnTitle, strColumnType);
                        }
                    }
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

#if NO
        // 获得 col 元素的 title 属性值，或者下属的语言相关的 title 元素值
        /*
<col>
	<title>
		<caption lang='zh-CN'>书名</caption>
		<caption lang='en'>Title</caption>
	</title>
         * */
        static string GetColumnTitle(XmlNode nodeCol, 
            string strLang = "zh")
        {
            string strColumnTitle = DomUtil.GetAttr(nodeCol, "title");
            if (string.IsNullOrEmpty(strColumnTitle) == false)
                return strColumnTitle;
            XmlNode nodeTitle = nodeCol.SelectSingleNode("title");
            if (nodeTitle == null)
                return "";
            return DomUtil.GetCaption(strLang, nodeTitle);
        }
#endif

        /// <summary>
        /// 重新获得全部数据库定义
        /// </summary>
        public void ReloadDatabasesInfo()
        {
            GetAllDatabaseInfo();
            InitialReaderDbProperties();
            GetUtilDbProperties();
        }

        /// <summary>
        /// 表示当前全部数据库信息的 XmlDocument 对象
        /// </summary>
        public XmlDocument AllDatabaseDom = null;

        /// <summary>
        /// 获取全部数据库定义
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetAllDatabaseInfo(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得全部数据库定义 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = 0;

                this.AllDatabaseDom = null;

                lRet = Channel.ManageDatabase(
    Stop,
    "getinfo",
    "",
    "",
    out strValue,
    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.ErrorCode == ErrorCode.AccessDenied)
                    {
                    }

                    strError = "针对服务器 " + Channel.Url + " 获得全部数据库定义过程发生错误：" + strError;
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strValue);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                this.AllDatabaseDom = dom;

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                if (Stop != null)
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");
                }

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得编目库属性列表
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int InitialBiblioDbProperties(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();

            string strError = "";
            int nRet = 0;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化书目库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                this.BiblioDbProperties = new List<BiblioDbProperty>();
                if (this.AllDatabaseDom == null)
                    return 0;

                XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database[@type='biblio']");
                foreach (XmlNode node in nodes)
                {

                    string strName = DomUtil.GetAttr(node, "name");
                    string strType = DomUtil.GetAttr(node, "type");
                    // string strRole = DomUtil.GetAttr(node, "role");
                    // string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");

                    BiblioDbProperty property = new BiblioDbProperty();
                    this.BiblioDbProperties.Add(property);
                    property.DbName = DomUtil.GetAttr(node, "name");
                    property.ItemDbName = DomUtil.GetAttr(node, "entityDbName");
                    property.Syntax = DomUtil.GetAttr(node, "syntax");
                    property.IssueDbName = DomUtil.GetAttr(node, "issueDbName");
                    property.OrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    property.CommentDbName = DomUtil.GetAttr(node, "commentDbName");
                    property.Role = DomUtil.GetAttr(node, "role");

                    bool bValue = true;
                    nRet = DomUtil.GetBooleanParam(node,
                        "inCirculation",
                        true,
                        out bValue,
                        out strError);
                    property.InCirculation = bValue;
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
#if NO
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
#endif
        }


#if NO
        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得编目库属性列表
        /// </summary>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int InitialBiblioDbProperties()
        {
        REDO:
            int nRet = PrepareSearch();
            if (nRet == 0)
                return -1;

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得书目库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = 0;

                this.BiblioDbProperties = new List<BiblioDbProperty>();


                // 新用法：一次性获得全部参数
                lRet = Channel.GetSystemParameter(Stop,
                    "system",
                    "biblioDbGroup",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得书目库信息过程发生错误：" + strError;
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strValue) == true)
                {
                    // 还是用旧方法

                    lRet = Channel.GetSystemParameter(Stop,
                        "biblio",
                        "dbnames",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 获得编目库名列表过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    string[] biblioDbNames = strValue.Split(new char[] { ',' });

                    for (int i = 0; i < biblioDbNames.Length; i++)
                    {
                        BiblioDbProperty property = new BiblioDbProperty();
                        property.DbName = biblioDbNames[i];
                        this.BiblioDbProperties.Add(property);
                    }


                    // 获得语法格式
                    lRet = Channel.GetSystemParameter(Stop,
                        "biblio",
                        "syntaxs",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 获得编目库数据格式列表过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    string[] syntaxs = strValue.Split(new char[] { ',' });

                    if (syntaxs.Length != this.BiblioDbProperties.Count)
                    {
                        strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而数据格式为 " + syntaxs.Length.ToString() + " 个，数量不一致";
                        goto ERROR1;
                    }

                    // 增补数据格式
                    for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                    {
                        this.BiblioDbProperties[i].Syntax = syntaxs[i];
                    }

                    {

                        // 获得对应的实体库名
                        lRet = Channel.GetSystemParameter(Stop,
                            "item",
                            "dbnames",
                            out strValue,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "针对服务器 " + Channel.Url + " 获得实体库名列表过程发生错误：" + strError;
                            goto ERROR1;
                        }

                        string[] itemdbnames = strValue.Split(new char[] { ',' });

                        if (itemdbnames.Length != this.BiblioDbProperties.Count)
                        {
                            strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而实体库名为 " + itemdbnames.Length.ToString() + " 个，数量不一致";
                            goto ERROR1;
                        }

                        // 增补数据格式
                        for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                        {
                            this.BiblioDbProperties[i].ItemDbName = itemdbnames[i];
                        }

                    }

                    {

                        // 获得对应的期库名
                        lRet = Channel.GetSystemParameter(Stop,
                            "issue",
                            "dbnames",
                            out strValue,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "针对服务器 " + Channel.Url + " 获得期库名列表过程发生错误：" + strError;
                            goto ERROR1;
                        }

                        string[] issuedbnames = strValue.Split(new char[] { ',' });

                        if (issuedbnames.Length != this.BiblioDbProperties.Count)
                        {
                            return 0; // TODO: 暂时不警告。等将来所有用户都更换了dp2libraryws 2007/10/19以后的版本后，这里再警告
                            /*
                            strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而期库名为 " + issuedbnames.Length.ToString() + " 个，数量不一致";
                            goto ERROR1;
                             * */
                        }

                        // 增补数据格式
                        for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                        {
                            this.BiblioDbProperties[i].IssueDbName = issuedbnames[i];
                        }
                    }

                    ///////

                    {

                        // 获得对应的订购库名
                        lRet = Channel.GetSystemParameter(Stop,
                            "order",
                            "dbnames",
                            out strValue,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "针对服务器 " + Channel.Url + " 获得订购库名列表过程发生错误：" + strError;
                            goto ERROR1;
                        }

                        string[] orderdbnames = strValue.Split(new char[] { ',' });

                        if (orderdbnames.Length != this.BiblioDbProperties.Count)
                        {
                            return 0; // TODO: 暂时不警告。等将来所有用户都更换了dp2libraryws 2007/11/30以后的版本后，这里再警告
                            /*
                            strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而订购库名为 " + orderdbnames.Length.ToString() + " 个，数量不一致";
                            goto ERROR1;
                             * */
                        }

                        // 增补数据格式
                        for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                        {
                            this.BiblioDbProperties[i].OrderDbName = orderdbnames[i];
                        }
                    }

                }
                else
                {
                    // 新方法
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<root />");

                    try
                    {
                        dom.DocumentElement.InnerXml = strValue;
                    }
                    catch (Exception ex)
                    {
                        strError = "category=system,name=biblioDbGroup所返回的XML片段在装入InnerXml时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];

                        BiblioDbProperty property = new BiblioDbProperty();
                        this.BiblioDbProperties.Add(property);
                        property.DbName = DomUtil.GetAttr(node, "biblioDbName");
                        property.ItemDbName = DomUtil.GetAttr(node, "itemDbName");
                        property.Syntax = DomUtil.GetAttr(node, "syntax");
                        property.IssueDbName = DomUtil.GetAttr(node, "issueDbName");
                        property.OrderDbName = DomUtil.GetAttr(node, "orderDbName");
                        property.CommentDbName = DomUtil.GetAttr(node, "commentDbName");
                        property.Role = DomUtil.GetAttr(node, "role");

                        bool bValue = true;
                        nRet = DomUtil.GetBooleanParam(node,
                            "inCirculation",
                            true,
                            out bValue,
                            out strError);
                        property.InCirculation = bValue;
                    }
                }

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
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

#endif

        string[] m_readerDbNames = null;

        /// <summary>
        /// 获得全部读者库名
        /// </summary>
        public string[] ReaderDbNames 
        {
            get
            {
                if (this.m_readerDbNames == null)
                {
                    this.m_readerDbNames = new string[this.ReaderDbProperties.Count];
                    int i = 0;
                    foreach (ReaderDbProperty prop in this.ReaderDbProperties)
                    {
                        this.m_readerDbNames[i++] = prop.DbName;
                    }
                }

                return this.m_readerDbNames;
            }
        }

        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得读者库属性列表
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int InitialReaderDbProperties(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            string strError = "";
            int nRet = 0;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化读者库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                this.ReaderDbProperties = new List<ReaderDbProperty>();
                this.m_readerDbNames = null;

                if (this.AllDatabaseDom == null)
                    return 0;

                XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database[@type='reader']");
                foreach (XmlNode node in nodes)
                {

                    ReaderDbProperty property = new ReaderDbProperty();
                    this.ReaderDbProperties.Add(property);
                    property.DbName = DomUtil.GetAttr(node, "name");
                    property.LibraryCode = DomUtil.GetAttr(node, "libraryCode");

                    bool bValue = true;
                    nRet = DomUtil.GetBooleanParam(node,
                        "inCirculation",
                        true,
                        out bValue,
                        out strError);
                    property.InCirculation = bValue;
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
#if NO
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
#endif
        }

        string _arrivedDbName = "";

        public string ArrivedDbName
        {
            get
            {
                return this._arrivedDbName;
            }
        }

        // 初始化预约到书库的相关属性
        public int InitialArrivedDbProperties(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            string strError = "";
            int nRet = 0;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化预约到书库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                this._arrivedDbName = "";

                if (this.ServerVersion < 2.47)
                    return 0;

                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "arrived",
                    "dbname",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得预约到书库名过程发生错误：" + strError;
                    goto ERROR1;
                }

                this._arrivedDbName = strValue;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }
            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

#if NO
        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得读者库属性列表
        /// </summary>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int InitialReaderDbProperties()
        {
        REDO:
            int nRet = PrepareSearch();
            if (nRet == 0)
                return -1;

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得读者库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = 0;

                this.ReaderDbProperties = new List<ReaderDbProperty>();
                this.m_readerDbNames = null;

                // 新用法：一次性获得全部参数
                lRet = Channel.GetSystemParameter(Stop,
                    "system",
                    "readerDbGroup",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得读者库信息过程发生错误：" + strError;
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strValue) == true)
                {
                    // 还是用旧方法

                    lRet = Channel.GetSystemParameter(Stop,
                        "reader",
                        "dbnames",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + Channel.Url + " 获得读者库名列表过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    string[] readerDbNames = strValue.Split(new char[] { ',' });

                    for (int i = 0; i < readerDbNames.Length; i++)
                    {
                        ReaderDbProperty property = new ReaderDbProperty();
                        property.DbName = readerDbNames[i];
                        this.ReaderDbProperties.Add(property);
                    }
                }
                else
                {
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
                        goto ERROR1;
                    }

                    XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];

                        ReaderDbProperty property = new ReaderDbProperty();
                        this.ReaderDbProperties.Add(property);
                        property.DbName = DomUtil.GetAttr(node, "name");
                        property.LibraryCode = DomUtil.GetAttr(node, "libraryCode");

                        bool bValue = true;
                        nRet = DomUtil.GetBooleanParam(node,
                            "inCirculation",
                            true,
                            out bValue,
                            out strError);
                        property.InCirculation = bValue;
                    }
                }

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
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }
#endif

        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得前端交费接口配置信息
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetClientFineInterfaceInfo(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();   // 优化

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得前端交费接口配置信息 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "circulation",
                    "clientFineInterface",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得前端交费接口配置信息过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.ClientFineInterfaceName = "";

                if (String.IsNullOrEmpty(strValue) == false)
                {
                    XmlDocument cfg_dom = new XmlDocument();
                    try
                    {
                        cfg_dom.LoadXml(strValue);
                    }
                    catch (Exception ex)
                    {
                        strError = "服务器配置的前端交费接口XML装入DOM时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    this.ClientFineInterfaceName = DomUtil.GetAttr(cfg_dom.DocumentElement,
                        "name");
                }

            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

        // 
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得索取号配置信息
        /// </summary>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetCallNumberInfo(bool bPreareSearch = true)
        {
            this.CallNumberInfo = "";
            this.CallNumberCfgDom = null;

            if (bPreareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();   // 优化

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得索取号配置信息 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "circulation",
                    "callNumber",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得索取号配置信息过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.CallNumberInfo = strValue;

                this.CallNumberCfgDom = new XmlDocument();
                this.CallNumberCfgDom.LoadXml("<callNumber/>");

                try
                {
                    this.CallNumberCfgDom.DocumentElement.InnerXml = this.CallNumberInfo;
                }
                catch (Exception ex)
                {
                    strError = "Set callnumber_cfg_dom InnerXml error: " + ex.Message;
                    goto ERROR1;
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPreareSearch == true)
                    EndSearch();
            }

            return 0;
        ERROR1:
            /*
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否要继续?",
                "dp2Circulation",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == DialogResult.OK)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
             * */
            return 1;
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

        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得实用库属性列表
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetUtilDbProperties(bool bPrepareSearch = true)
        {
        REDO:
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化实用库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                this.UtilDbProperties = new List<UtilDbProperty>();

                if (this.AllDatabaseDom == null)
                    return 0;
#if NO
                string strValue = "";
                long lRet = Channel.GetSystemParameter(Stop,
                    "utilDb",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得实用库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] utilDbNames = strValue.Split(new char[] { ',' });

                for (int i = 0; i < utilDbNames.Length; i++)
                {
                    UtilDbProperty property = new UtilDbProperty();
                    property.DbName = utilDbNames[i];
                    this.UtilDbProperties.Add(property);
                }

                // 获得类型
                lRet = Channel.GetSystemParameter(Stop,
                    "utilDb",
                    "types",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得实用库数据格式列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] types = strValue.Split(new char[] { ',' });

                if (types.Length != this.UtilDbProperties.Count)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得实用库名为 " + this.UtilDbProperties.Count.ToString() + " 个，而类型为 " + types.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                }

                // 增补数据格式
                for (int i = 0; i < this.UtilDbProperties.Count; i++)
                {
                    this.UtilDbProperties[i].Type = types[i];
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
#endif

                XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database");
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strType = DomUtil.GetAttr(node, "type");
                    // string strRole = DomUtil.GetAttr(node, "role");
                    // string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");

                    // 空的名字将被忽略
                    if (String.IsNullOrEmpty(strName) == true)
                        continue;

                    if (strType == "zhongcihao"
                        || strType == "publisher"
                        || strType == "dictionary")
                    {
                        UtilDbProperty property = new UtilDbProperty();
                        property.DbName = strName;
                        property.Type = strType;
                        this.UtilDbProperties.Add(property);
                    }

                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
#if NO
        ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
#endif
        }

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

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.menuStrip_main.Enabled = bEnable;

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
            string[] parts = strStyles.Split(new char[] {','});
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
            out string [] values,
            out string strError)
        {
            values = null;
            strError = "";

            // 先看看缓存里面是否已经有了
            string strName = strTableName + "~~~" + strDbName;

            values = (string [])this.valueTableCache[strName];

            if (values != null)
                return 0;

            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;  // 2009/2/11
            }

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获取值列表 ...");
            Stop.BeginLoop();

            try
            {
                long lRet = Channel.GetValueTable(
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

                EndSearch();
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
                EnsureChildForm<ChargingForm>().SmartFuncState = FuncState.VerifyRenew;
            }
            else
            {
                EnsureChildForm<QuickChargingForm>().Activate();
                EnsureChildForm<QuickChargingForm>().SmartFuncState = FuncState.VerifyRenew;
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

        // 装入册条码号相关记录
        // 尽量占用当前已经打开的种册窗
        void LoadItemBarcode()
        {
            if (this.toolStripTextBox_barcode.Text == "")
            {
                MessageBox.Show(this, "尚未输入条码");
                return;
            }

            /*
            ItemInfoForm form = this.TopItemInfoForm;

            if (form == null)
            {
                // 新开一个实体窗
                form = new ItemInfoForm();
                form.MdiParent = this;
                form.MainForm = this;
                form.Show();
            }

            form.LoadRecord(this.toolStripTextBox_barcode.Text);
             * */

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
                this.Channel.LibraryCodeList,
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
            {
                /*
                ReaderInfoForm form = this.TopReaderInfoForm;

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


                form.LoadRecord(this.toolStripTextBox_barcode.Text,
                    false);
                 * */
                LoadReaderBarcode();
            }

            if (nRet == 2)
            {
                LoadItemBarcode();
            }

            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 核对本地和服务器时钟
        // return:
        //      -1  error
        //      0   没有问题
        //      1   本地时钟和服务器时钟偏差过大，超过10分钟 strError中有报错信息
        int CheckServerClock(bool bPrepareSearch,
            out string strError)
        {
            strError = "";

            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return 0;
            }

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得服务器当前时钟 ...");
            Stop.BeginLoop();

            try
            {
                string strTime = "";
                long lRet = Channel.GetClock(
                    Stop,
                    out strTime,
                    out strError);
                if (lRet == -1)
                    return -1;

                DateTime server_time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
                server_time = server_time.ToLocalTime();

                DateTime now = DateTime.Now;

                TimeSpan delta = server_time - now;
                if (delta.TotalMinutes > 10 || delta.TotalMinutes < -10)
                {
                    strError = "本地时钟和服务器时钟差异过大，为 " 
                        + delta.ToString()
                        + "。\r\n\r\n"
                        + "测试时的服务器时间为: " + server_time.ToString() + "  本地时间为: " + now.ToString()
                        +"\r\n\r\n请用时钟窗仔细核对服务器时钟，如有必要重新设定服务器时钟为正确值。\r\n\r\n注：流通功能均采用服务器时钟，如果服务器时钟正确而本地时钟不正确，一般不会影响流通功能正常进行。";
                    return 1;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                if (bPrepareSearch == true)
                    EndSearch();
            }

            return 0;
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

            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;
            }

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索词条 '" + strKey + "' ...");
            Stop.BeginLoop();

            EnableControls(false);
            try
            {
                long lRet = Channel.Search(
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
                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;
                    lRet = Channel.GetSearchResult(
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

                    DigitalPlatform.CirculationClient.localhost.Record record = searchresults[0];

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
                lRet = Channel.WriteRes(
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

                EndSearch();
            }
        ERROR1:
            return -1;
        }

        // 检索词典库
        // parameters:
        //      results [in,out] 如果需要返回结果，需要在调用前 new List<string>()。如果不需要返回结果，传入 null 即可
        public int SearchDictionary(
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
+ "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>"+nMaxCount.ToString()+"</maxCount></item><lang>" + strLang + "</lang></target>";

            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;
            }

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
                long lRet = Channel.Search(
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
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                for (; ; )
                {
                    lRet = Channel.GetSearchResult(
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
                        DigitalPlatform.CirculationClient.localhost.Record record = searchresults[i];

                        results.Add(record.RecordBody.Xml);
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

                EndSearch();
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
                    if (Channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.NotFound)
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

            int nRet = PrepareSearch();
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                return -1;  // 2009/2/11
            }

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在验证条码号 "+strBarcode+"...");
            Stop.BeginLoop();

            // EnableControls(false);

            try
            {
                return VerifyBarcode(
                    Stop,
                    Channel,
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

                EndSearch();
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
                    if (Channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.NotFound)
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

            this.AppInfo.LinkFormState(dlg,
                "logindlg_state");

            if (fail_contidion == LoginFailCondition.PasswordError
                && dlg.SavePasswordShort == false
                && dlg.SavePasswordLong == false)
                dlg.AutoShowShortSavePasswordTip = true;

            dlg.ShowDialog(owner);

            this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

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
                nRet = PrepareSearch(bDisplayProgress);
                if (nRet == 0)
                    return "PrepareSearch() error";

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

                    long lRet = Channel.GetReaderInfo(Stop,
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

                    this.EndSearch();
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

            int nRet = PrepareSearch(bDisplayProgress);
            if (nRet == 0)
            {
                strError = "PrepareSearch() error";
                strSummary = strError;
                return -1;  // 2009/2/11
            }


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

                    long lRet = Channel.GetBiblioSummary(
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

                this.EndSearch(bDisplayProgress);   // BUG !!! 2012/3/28前少这一句
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

        private void MenuItem_copyright_Click(object sender, EventArgs e)
        {
            CopyrightDlg dlg = new CopyrightDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        /*
* 检查两条记录是否适合建立目标关系
* 1) 源库，应当是采购工作库
* 2) 源库和目标库，不是一个库
* 3) 源库的syntax和目标库的syntax，是一样的
* 4) 目标库不应该是外源角色
* */
        // parameters:
        //      strSourceRecPath    记录ID可以为问号
        //      strTargetRecPath    记录ID可以为问号，仅当bCheckTargetWenhao==false时
        // return:
        //      -1  出错
        //      0   不适合建立目标关系 (这种情况是没有什么错，但是不适合建立)
        //      1   适合建立目标关系
        internal int CheckBuildLinkCondition(string strSourceBiblioRecPath,
            string strTargetBiblioRecPath,
            bool bCheckTargetWenhao,
            out string strError)
        {
            strError = "";

            // TODO: 最好检查一下这个路径的格式。合法的书目库名可以在MainForm中找到

            // 检查是不是书目库名。MARC格式是否和当前数据库一致。不能是当前记录自己
            string strTargetDbName = Global.GetDbName(strTargetBiblioRecPath);
            string strTargetRecordID = Global.GetRecordID(strTargetBiblioRecPath);

            if (String.IsNullOrEmpty(strTargetDbName) == true
                || String.IsNullOrEmpty(strTargetRecordID) == true)
            {
                strError = "目标记录路径 '" + strTargetBiblioRecPath + "' 不是合法的记录路径";
                goto ERROR1;
            }

            // 2009/11/25 
            if (this.IsBiblioSourceDb(strTargetDbName) == true)
            {
                strError = "库 '" + strTargetDbName + "' 是 外源书目库 角色，不能作为目标库";
                return 0;
            }

            // 2011/11/29
            if (this.IsOrderWorkDb(strTargetDbName) == true)
            {
                strError = "库 '" + strTargetDbName + "' 是 采购工作库 角色，不能作为目标库";
                return 0;
            }

            string strSourceDbName = Global.GetDbName(strSourceBiblioRecPath);
            string strSourceRecordID = Global.GetRecordID(strSourceBiblioRecPath);

            if (String.IsNullOrEmpty(strSourceDbName) == true
                || String.IsNullOrEmpty(strSourceRecordID) == true)
            {
                strError = "源记录路径 '" + strSourceBiblioRecPath + "' 不是合法的记录路径";
                goto ERROR1;
            }

            /*
            if (this.IsOrderWorkDb(strSourceDbName) == false)
            {
                strError = "源库 '" + strSourceDbName + "' 不具备 采购工作库 角色";
                return 0;
            }*/

            // 根据书目库名获得MARC格式语法名
            // return:
            //      null    没有找到指定的书目库名
            string strSourceSyntax = this.GetBiblioSyntax(strSourceDbName);
            if (String.IsNullOrEmpty(strSourceSyntax) == true)
                strSourceSyntax = "unimarc";
            string strSourceIssueDbName = this.GetIssueDbName(strSourceDbName);


            bool bFound = false;
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.BiblioDbProperties[i];

                    if (prop.DbName == strTargetDbName)
                    {
                        bFound = true;

                        string strTargetSyntax = prop.Syntax;
                        if (String.IsNullOrEmpty(strTargetSyntax) == true)
                            strTargetSyntax = "unimarc";

                        if (strTargetSyntax != strSourceSyntax)
                        {
                            strError = "拟设置的目标记录因其书目数据格式为 '" + strTargetSyntax + "'，与源记录的书目数据格式 '" + strSourceSyntax + "' 不一致，因此操作被拒绝";
                            return 0;
                        }

                        if (String.IsNullOrEmpty(prop.IssueDbName)
                            != String.IsNullOrEmpty(strSourceIssueDbName))
                        {
                            strError = "拟设置的目标记录因其书目库 '" + strTargetDbName + "' 文献类型(期刊还是图书)和源记录的书目库 '" + strSourceDbName + "' 不一致，因此操作被拒绝";
                            return 0;
                        }
                    }
                }
            }

            if (bFound == false)
            {
                strError = "'" + strTargetDbName + "' 不是合法的书目库名";
                goto ERROR1;
            }

            // source

            if (this.IsBiblioDbName(strSourceDbName) == false)
            {
                strError = "'" + strSourceDbName + "' 不是合法的书目库名";
                goto ERROR1;
            }

            if (strSourceRecordID == "?")
            {
                /* 源记录ID可以为问号，因为这不妨碍建立连接关系
                strError = "源记录 '"+strSourceBiblioRecPath+"' 路径中ID不能为问号";
                return 0;
                 * */
            }
            else
            {
                // 如果不是问号，就要检查一下了，没坏处
                if (Global.IsPureNumber(strSourceRecordID) == false)
                {
                    strError = "源记录  '" + strSourceBiblioRecPath + "' 路径中ID部分必须为纯数字";
                    goto ERROR1;
                }
            }

            // target
            if (strTargetRecordID == "?")
            {
                if (bCheckTargetWenhao == true)
                {
                    strError = "目标记录 '"+strTargetBiblioRecPath+"' 路径中ID不能为问号";
                    return 0;
                }
            }
            else
            {
                if (Global.IsPureNumber(strTargetRecordID) == false)
                {
                    strError = "目标记录 '" + strTargetBiblioRecPath + "' 路径中ID部分必须为纯数字";
                    goto ERROR1;
                }
            }

            if (strTargetDbName == strSourceDbName)
            {
                strError = "目标记录和源记录不能属于同一个书目库 '"+strTargetBiblioRecPath+"'";
                return 0;
                // 注：这样就不用检查目标是否源记录了
            }

            return 1;
        ERROR1:
            return -1;
        }


        // 
        // return:
        //      null    没有找到指定的书目库名
        /// <summary>
        /// 根据书目库名获得 MARC 格式语法名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>语法名。如果为 null 表示没有找到</returns>
        public string GetBiblioSyntax(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                        return this.BiblioDbProperties[i].Syntax;
                }
            }

            return null;
        }

        // 
        // return:
        //      null    没有找到指定的书目库名
        /// <summary>
        /// 根据读者库名获得馆代码
        /// </summary>
        /// <param name="strReaderDbName">读者库名</param>
        /// <returns>管代码</returns>
        public string GetReaderDbLibraryCode(string strReaderDbName)
        {
            if (this.ReaderDbProperties != null)
            {
                foreach(ReaderDbProperty prop in this.ReaderDbProperties)
                {
                    if (prop.DbName == strReaderDbName)
                        return prop.LibraryCode;
                }
            }

            return null;
        }

        // 2013/6/15
        // 
        /// <summary>
        /// 获得全部可用的馆代码
        /// </summary>
        /// <returns>字符串集合</returns>
        public List<string> GetAllLibraryCode()
        {
            List<string> results = new List<string>();
            if (this.ReaderDbProperties != null)
            {
                foreach (ReaderDbProperty prop in this.ReaderDbProperties)
                {
                    results.Add(prop.LibraryCode);
                }

                results.Sort();
                StringUtil.RemoveDup(ref results);
            }

            return results;
        }

        // 
        /// <summary>
        /// 判断一个库名是否为合法的书目库名
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为合法的书目库名</returns>
        public bool IsValidBiblioDbName(string strDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        // 如果返回""，表示该书目库的下属库没有定义
        /// <summary>
        /// 根据书目库名获得对应的下属库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <param name="strDbType">下属库的类型</param>
        /// <returns>下属库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备此类型的下属库</returns>
        public string GetItemDbName(string strBiblioDbName,
            string strDbType)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                    {
                        if (strDbType == "item")
                            return this.BiblioDbProperties[i].ItemDbName;
                        else if (strDbType == "order")
                            return this.BiblioDbProperties[i].OrderDbName;
                        else if (strDbType == "issue")
                            return this.BiblioDbProperties[i].IssueDbName;
                        else if (strDbType == "comment")
                            return this.BiblioDbProperties[i].CommentDbName;
                        else
                            return "";
                    }
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的实体库没有定义
        /// <summary>
        /// 根据书目库名获得对应的实体库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>实体库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备实体库</returns>
        public string GetItemDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                        return this.BiblioDbProperties[i].ItemDbName;
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的期库没有定义
        /// <summary>
        /// 根据书目库名获得对应的期库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>期库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备期库</returns>
        public string GetIssueDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                        return this.BiblioDbProperties[i].IssueDbName;
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的订购库没有定义
        /// <summary>
        /// 根据书目库名获得对应的订购库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>订购库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备订购库</returns>
        public string GetOrderDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                        return this.BiblioDbProperties[i].OrderDbName;
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的评注库没有定义
        /// <summary>
        /// 根据书目库名获得对应的评注库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>评注库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备评注库</returns>
        public string GetCommentDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                        return this.BiblioDbProperties[i].CommentDbName;
                }
            }

            return null;
        }

        // 实体库名 --> 期刊/图书类型 对照表
        Hashtable itemdb_type_table = new Hashtable();

        // 
        // return:
        //      -1  不是实体库
        //      0   图书类型
        //      1   期刊类型
        /// <summary>
        /// 观察实体库是不是期刊库类型
        /// </summary>
        /// <param name="strItemDbName">实体库名</param>
        /// <returns>-1: 不是实体库; 0: 图书类型; 1: 期刊类型</returns>
        public int IsSeriesTypeFromItemDbName(string strItemDbName)
        {
            int nRet = 0;
            object o = itemdb_type_table[strItemDbName];
            if (o != null)
            {
                nRet = (int)o;
                return nRet;
            }

            string strBiblioDbName = GetBiblioDbNameFromItemDbName(strItemDbName);
            if (strBiblioDbName == null)
                return -1;
            string strIssueDbName = GetIssueDbName(strBiblioDbName);
            if (string.IsNullOrEmpty(strIssueDbName) == true)
                nRet = 0;
            else
                nRet = 1;

            itemdb_type_table[strItemDbName] = nRet;
            return nRet;
        }

        // 
        /// <summary>
        /// 根据实体库名获得对应的书目库名
        /// </summary>
        /// <param name="strItemDbName">实体库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromItemDbName(string strItemDbName)
        {
            // 2008/11/28 
            // 实体库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].ItemDbName == strItemDbName)
                        return this.BiblioDbProperties[i].DbName;
                }
            }

            return null;
        }

        // 根据 册/订购/期/评注 记录路径和 parentid 构造所从属的书目记录路径
        public string BuildBiblioRecPath(string strDbType,
            string strItemRecPath,
            string strParentID)
        {
            if (string.IsNullOrEmpty(strParentID) == true)
                return null;

            string strItemDbName = Global.GetDbName(strItemRecPath);
            if (string.IsNullOrEmpty(strItemDbName) == true)
                return null;

            string strBiblioDbName = this.GetBiblioDbNameFromItemDbName(strDbType, strItemDbName);
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                return null;

            return  strBiblioDbName + "/" + strParentID;
        }

        /// <summary>
        /// 根据实体(期/订购/评注)库名获得对应的书目库名
        /// </summary>
        /// <param name="strDbType">数据库类型</param>
        /// <param name="strItemDbName">数据库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromItemDbName(string strDbType,
            string strItemDbName)
        {
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                if (strDbType == "item")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.ItemDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "order")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.OrderDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "issue")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.IssueDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "comment")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.CommentDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else
                    throw new Exception("无法处理数据库类型 '"+strDbType+"'");
            }

            return null;
        }

        // 订购名 --> 期刊/图书类型 对照表
        Hashtable orderdb_type_table = new Hashtable();

        // 
        // return:
        //      -1  不是订购库
        //      0   图书类型
        //      1   期刊类型
        /// <summary>
        /// 观察订购库是不是期刊库类型
        /// </summary>
        /// <param name="strOrderDbName">订购库名</param>
        /// <returns>-1: 不是订购库; 0: 图书类型; 1: 期刊类型</returns>
        public int IsSeriesTypeFromOrderDbName(string strOrderDbName)
        {
            int nRet = 0;
            object o = orderdb_type_table[strOrderDbName];
            if (o != null)
            {
                nRet = (int)o;
                return nRet;
            }

            string strBiblioDbName = GetBiblioDbNameFromOrderDbName(strOrderDbName);
            if (strBiblioDbName == null)
                return -1;
            string strIssueDbName = GetIssueDbName(strBiblioDbName);
            if (string.IsNullOrEmpty(strIssueDbName) == true)
                nRet = 0;
            else
                nRet = 1;

            orderdb_type_table[strOrderDbName] = nRet;
            return nRet;
        }

        // 
        /// <summary>
        /// 根据期库名获得对应的书目库名
        /// </summary>
        /// <param name="strIssueDbName">期库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromIssueDbName(string strIssueDbName)
        {
            // 2008/11/28 
            // 期库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strIssueDbName) == true)
                return null;


            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].IssueDbName == strIssueDbName)
                        return this.BiblioDbProperties[i].DbName;
                }
            }

            return null;
        }

        // 
        /// <summary>
        /// 根据订购库名获得对应的书目库名
        /// </summary>
        /// <param name="strOrderDbName">订购库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromOrderDbName(string strOrderDbName)
        {
            // 2008/11/28 
            // 订购库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strOrderDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].OrderDbName == strOrderDbName)
                        return this.BiblioDbProperties[i].DbName;
                }
            }

            return null;
        }

        // 
        /// <summary>
        /// 根据评注库名获得对应的书目库名
        /// </summary>
        /// <param name="strCommentDbName">评注库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromCommentDbName(string strCommentDbName)
        {
            if (String.IsNullOrEmpty(strCommentDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].CommentDbName == strCommentDbName)
                        return this.BiblioDbProperties[i].DbName;
                }
            }

            return null;
        }


        // 2009/11/25
        // 
        /// <summary>
        /// 是否为外源书目库?
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>是否为外源书目库</returns>
        public bool IsBiblioSourceDb(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.BiblioDbProperties[i];

                    if (prop.DbName == strBiblioDbName)
                    {
                        if (StringUtil.IsInList("biblioSource", prop.Role) == true)
                            return true;
                    }
                }
            }

            return false;
        }

        // 2009/10/24
        // 
        /// <summary>
        /// 是否为采购工作库?
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>是否为采购工作裤</returns>
        public bool IsOrderWorkDb(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.BiblioDbProperties[i];

                    if (prop.DbName == strBiblioDbName)
                    {
                        if (StringUtil.IsInList("orderWork", prop.Role) == true)
                            return true;
                    }
                }
            }

            return false;
        }

        // 2008/12/15
        // 
        /// <summary>
        /// 是否为书目库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为书目库名</returns>
        public bool IsBiblioDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 2012/9/1
        /// <summary>
        /// 获得一个书目库的属性信息
        /// </summary>
        /// <param name="strDbName">书目库名</param>
        /// <returns>属性信息对象</returns>
        public BiblioDbProperty GetBiblioDbProperty(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                {
                    if (prop.DbName == strDbName)
                        return prop;
                }
            }

            return null;
        }

        // 2008/12/15
        // 
        /// <summary>
        /// 是否为实体库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为实体库名</returns>
        public bool IsItemDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].ItemDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 2008/12/15
        // 
        /// <summary>
        /// 是否为读者库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为读者库名</returns>
        public bool IsReaderDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.ReaderDbNames != null)    // 2009/3/29 
            {
                for (int i = 0; i < this.ReaderDbNames.Length; i++)
                {
                    if (this.ReaderDbNames[i] == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        /// <summary>
        /// 是否为订购库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为订购库名</returns>
        public bool IsOrderDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].OrderDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        /// <summary>
        /// 是否为期库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为期库名</returns>
        public bool IsIssueDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].IssueDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        /// <summary>
        /// 是否为评注库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为评注库名</returns>
        public bool IsCommentDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].CommentDbName == strDbName)
                        return true;
                }
            }

            return false;
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
                this.Channel.Close();   // 迫使通信通道重新登录

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

        /// <summary>
        /// 缺省的字体名
        /// </summary>
        public string DefaultFontString
        {
            get
            {
                return this.AppInfo.GetString(
                    "Global",
                    "default_font",
                    "");
            }
            set
            {
                this.AppInfo.SetString(
                    "Global",
                    "default_font",
                    value);
            }
        }

        /// <summary>
        /// 缺省字体
        /// </summary>
        new public Font DefaultFont
        {
            get
            {
                string strDefaultFontString = this.DefaultFontString;
                if (String.IsNullOrEmpty(strDefaultFontString) == true)
                {
                    return GuiUtil.GetDefaultFont();    // 2015/5/8
                    // return null;
                }

                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strDefaultFontString);
            }
        }

        // parameters:
        //      bForce  是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置
        /// <summary>
        /// 设置控件字体
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="font">字体</param>
        /// <param name="bForce">是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置</param>
        public static void SetControlFont(Control control,
            Font font,
            bool bForce = false)
        {
            if (font == null)
            {
                if (bForce == false)
                    return;
                font = Control.DefaultFont;
            }
            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            { }
            else
            control.Font = font;

            ChangeDifferentFaceFont(control, font);
        }

        static void ChangeDifferentFaceFont(Control parent,
            Font font)
        {
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                Font subfont = sub.Font;

#if NO
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    sub.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);


                    // sub.Font = new Font(font, subfont.Style);
                }
#endif
                ChangeFont(font, sub);

                if (sub is ToolStrip)
                {
                    ChangeDifferentFaceFont((ToolStrip)sub, font);
                }

                // 递归
                ChangeDifferentFaceFont(sub, font);
            }
        }

        static void ChangeDifferentFaceFont(ToolStrip tool,
    Font font)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            for(int i=0;i<tool.Items.Count;i++)
            {
                ToolStripItem item = tool.Items[i];

                Font subfont = item.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    // item.Font = new Font(font, subfont.Style);
                    item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
                }
            }
        }

        // 修改一个控件的字体
        static void ChangeFont(Font font,
            Control item)
        {
            Font subfont = item.Font;
            float ratio = subfont.SizeInPoints / font.SizeInPoints;
            if (subfont.Name != font.Name
                || subfont.SizeInPoints != font.SizeInPoints)
            {
                // item.Font = new Font(font, subfont.Style);
                item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
            }
        }

        static void ChangeDifferentFaceFont(SplitContainer tool,
Font font)
        {
            ChangeFont(font, tool.Panel1);
            // 递归
            ChangeDifferentFaceFont(tool.Panel1, font);

            ChangeFont(font, tool.Panel2);

            // 递归
            ChangeDifferentFaceFont(tool.Panel2, font);
        }

        /// <summary>
        /// 激活固定面板区域的“属性”属性页
        /// </summary>
        public void ActivatePropertyPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
        }

        /// <summary>
        /// 固定面板区域的属性控件
        /// </summary>
        public Control CurrentPropertyControl
        {
            get
            {
                if (this.tabPage_property.Controls.Count == 0)
                    return null;
                return this.tabPage_property.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_property.Controls.Count > 0)
                    this.tabPage_property.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_property.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
                }
            }
        }

        /// <summary>
        /// 激活固定面板区域的“验收”属性页
        /// </summary>
        public void ActivateAcceptPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_accept;
        }

        /// <summary>
        /// 固定面板区域的验收控件
        /// </summary>
        public Control CurrentAcceptControl
        {
            get
            {
                if (this.tabPage_accept.Controls.Count == 0)
                    return null;
                return this.tabPage_accept.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_accept.Controls.Count > 0)
                    this.tabPage_accept.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_accept.Controls.Add(value);
                }
            }
        }

        /// <summary>
        /// 激活固定面板区域的“校验结果”属性页
        /// </summary>
        public void ActivateVerifyResultPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
        }

        /// <summary>
        /// 固定面板区域的校验结果控件
        /// </summary>
        public Control CurrentVerifyResultControl
        {
            get
            {
                if (this.tabPage_verifyResult.Controls.Count == 0)
                    return null;
                return this.tabPage_verifyResult.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_verifyResult.Controls.Count > 0)
                    this.tabPage_verifyResult.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_verifyResult.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
                }

            }
        }

        /// <summary>
        /// 激活固定面板区域的“创建数据”属性页
        /// </summary>
        public void ActivateGenerateDataPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;
        }

        /// <summary>
        /// 固定面板区域的创建数据控件
        /// </summary>
        public Control CurrentGenerateDataControl
        {
            get
            {
                if (this.tabPage_generateData.Controls.Count == 0)
                    return null;
                return this.tabPage_generateData.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_generateData.Controls.Count > 0)
                    this.tabPage_generateData.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_generateData.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;

                    // 避免出现半截窗口图像的闪动
                    if (this.tabControl_panelFixed.Visible
                        && this.tabControl_panelFixed.SelectedTab == this.tabPage_generateData)
                        this.tabPage_generateData.Update();
                }
            }
        }

        /// <summary>
        /// 固定面板区域是否可见
        /// </summary>
        public bool PanelFixedVisible
        {
            get
            {
                return this.panel_fixed.Visible;
            }
            set
            {
                this.panel_fixed.Visible = value;
                this.splitter_fixed.Visible = value;

                this.MenuItem_displayFixPanel.Checked = value;
            }
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

        // 获得统计窗的英文类型名
        static string GetTypeName(object form)
        {
            string strTypeName = form.GetType().ToString();
            int nRet = strTypeName.LastIndexOf(".");
            if (nRet != -1)
                strTypeName = strTypeName.Substring(nRet + 1);

            return strTypeName;
        }

        // 获得统计窗的汉字类型名
        static string GetWindowName(object form)
        {
            return SelectInstallProjectsDialog.GetHanziHostName(GetTypeName(form));
        }

        // 从磁盘更新全部方案
        private void MenuItem_updateStatisProjectsFromDisk_Click(object sender, EventArgs e)
        {
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
        }

        // 从磁盘安装全部方案
        private void MenuItem_installStatisProjectsFromDisk_Click(object sender, EventArgs e)
        {
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
        }

        // 从 dp2003.com 安装全部方案
        private void MenuItem_installStatisProjects_Click(object sender, EventArgs e)
        {
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
        }

        // 为一个统计窗安装若干方案
        // parameters:
        //      projects    待安装的方案。注意有可能包含不适合安装到本窗口的方案
        // return:
        //      -1  出错
        //      >=0 安装的方案数
        int InstallProjects(
            object form,
            string strWindowName,
            List<ProjectItem> projects,
            string strSource,
            out string strError)
        {
            strError = "";
            int nInstallCount = 0;
            int nRet = 0;

            dynamic o = form;

            o.EnableControls(false);
            try
            {
                /*
                    string strTypeName = form.GetType().ToString();
                    nRet = strTypeName.LastIndexOf(".");
                    if (nRet != -1)
                        strTypeName = strTypeName.Substring(nRet + 1);
                */
                string strTypeName = GetTypeName(form);

                foreach (ProjectItem item in projects)
                {
                    if (strTypeName != item.Host)
                        continue;

                    string strLocalFileName = "";
                    string strLastModified = "";

                    if (strSource == "!url")
                    {
                        strLocalFileName = this.DataDir + "\\~install_project.projpack";
                        string strTempFileName = this.DataDir + "\\~temp_download_webfile";

                        nRet = WebFileDownloadDialog.DownloadWebFile(
                            this,
                            item.Url,
                            strLocalFileName,
                            strTempFileName,
                            "",
                            out strLastModified,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else
                    {
                        string strLocalDir = strSource;

                        // Uri uri = new Uri(item.Url);
                        /*
                        string strPath = item.Url;  // uri.LocalPath;
                        nRet = strPath.LastIndexOf("/");
                        if (nRet != -1)
                            strPath = strPath.Substring(nRet);
                         * */
                        string strPureFileName = ScriptManager.GetFileNameFromUrl(item.Url);

                        strLocalFileName = PathUtil.MergePath(strLocalDir, strPureFileName);

                        FileInfo fi = new FileInfo(strLocalFileName);
                        if (fi.Exists == false)
                        {
                            strError = "目录 '" + strLocalDir + "' 中没有找到文件 '" + strPureFileName + "'";
                            return -1;
                        }
                        strLastModified = DateTimeUtil.Rfc1123DateTimeString(fi.LastWriteTimeUtc);
                    }

                    // 安装Project
                    // return:
                    //      -1  出错
                    //      0   没有安装方案
                    //      >0  安装的方案数
                    nRet = o.ScriptManager.InstallProject(
                        o is Form ? o : this,
                        strWindowName,
                        strLocalFileName,
                        strLastModified,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    nInstallCount += nRet;
                }
            }
            finally
            {
                o.EnableControls(true);
            }

            return nInstallCount;
        }

        // 更新一个窗口拥有的全部方案
        // parameters:
        //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
        // return:
        //      -2  全部放弃
        //      -1  出错
        //      >=0 更新数
        int UpdateProjects(
            object form,
            string strSource,
            ref bool bHideMessageBox,
            ref bool bDontUpdate,
            out string strError)
        {
            strError = "";
            string strWarning = "";
            string strUpdateInfo = "";
            int nUpdateCount = 0;

            dynamic o = form;

            o.EnableControls(false);
            try
            {
                // 检查更新一个容器节点下的全部方案
                // parameters:
                //      dir_node    容器节点。如果 == null 检查更新全部方案
                //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      0   成功
                int nRet = o.ScriptManager.CheckUpdate(
                    o is Form ? o : this,
                    null,
                    strSource,
                    ref bHideMessageBox,
                    ref bDontUpdate,
                    ref nUpdateCount,
                    ref strUpdateInfo,
                    ref strWarning,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == -2)
                    return -2;
            }
            finally
            {
                o.EnableControls(true);
            }
            return nUpdateCount;
        }

        // 从 dp2003.com 检查更新全部方案
        private void MenuItem_updateStatisProjects_Click(object sender, EventArgs e)
        {
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
        }

        /// <summary>
        /// 是否隐藏浏览器控件的脚本错误提示
        /// </summary>
        public bool SuppressScriptErrors
        {
            get
            {
                return !DisplayScriptErrorDialog;
            }
        }

        // 浏览器控件允许脚本错误对话框(&S)
        /// <summary>
        /// 浏览器控件是否允许脚本错误对话框
        /// </summary>
        public bool DisplayScriptErrorDialog
        {
            get
            {

                return this.AppInfo.GetBoolean(
                    "global",
                    "display_webbrowsecontrol_scripterror_dialog",
                    false);
            }
            set
            {
                this.AppInfo.SetBoolean(
                    "global",
                    "display_webbrowsecontrol_scripterror_dialog",
                    value);
            }
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

        #region Client.cs 脚本支持

        ClientHost _clientHost = null;
        public ClientHost ClientHost
        {
            get
            {
                return _clientHost;
            }
        }

        int InitialClientScript(out string strError)
        {
            strError = "";

            Assembly assembly = null;

            string strServerMappedPath = Path.Combine(this.DataDir, "servermapped");
            string strFileName = Path.Combine(strServerMappedPath, "client/client.cs");

            if (File.Exists(strFileName) == false)
                return 0;   // 脚本文件没有找到

            int nRet = PrepareClientScript(strFileName,
                out assembly,
                out _clientHost,
                out strError);
            if (nRet == -1)
            {
                strError = "初始化前端脚本 '" + Path.GetFileName(strFileName) + "' 时出错: " + strError;
                return -1;
            }

            _clientHost.MainForm = this;

            return 0;
        }

        // 准备脚本环境
        int PrepareClientScript(string strCsFileName,
            out Assembly assembly,
            out ClientHost host,
            out string strError)
        {
            assembly = null;
            strError = "";
            host = null;

            string strContent = "";
            Encoding encoding;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strCsFileName,
                -1,
                out strContent,
                out encoding,
                out strError);
            if (nRet == -1)
                return -1;

            string strWarningInfo = "";
            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
                Environment.CurrentDirectory + "\\dp2circulation.exe",
            };

            // 2013/12/16
            nRet = ScriptManager.GetRef(strCsFileName,
    ref saAddRef,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            // 直接编译到内存
            // parameters:
            //		refs	附加的refs文件路径。路径中可能包含宏%installdir%
            nRet = ScriptManager.CreateAssembly_1(strContent,
                saAddRef,
                "",
                out assembly,
                out strError,
                out strWarningInfo);
            if (nRet == -1)
                goto ERROR1;

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Circulation.ClientHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Circulation.ClientHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (ClientHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        #region MainForm 统计方案

        private void scriptManager_CreateDefaultContent(object sender, CreateDefaultContentEventArgs e)
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

        // 
        /// <summary>
        /// 创建缺省的、宿主为 MainFormHost 的 main.cs 文件
        /// </summary>
        /// <param name="strFileName">文件全路径</param>
        /// <returns>0: 成功</returns>
        public static int CreateDefaultMainCsFile(string strFileName)
        {

            StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using System.Windows.Forms;");
            sw.WriteLine("using System.IO;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine("using System.Xml;");
            sw.WriteLine("");

            //sw.WriteLine("using DigitalPlatform.MarcDom;");
            //sw.WriteLine("using DigitalPlatform.Statis;");
            sw.WriteLine("using dp2Circulation;");
            sw.WriteLine("");
            sw.WriteLine("using DigitalPlatform.Xml;");
            sw.WriteLine("");

            sw.WriteLine("public class MyStatis : MainFormHost");

            sw.WriteLine("{");

            sw.WriteLine("	public override void Main(object sender, EventArgs e)");
            sw.WriteLine("	{");
            sw.WriteLine("	}");


            sw.WriteLine("}");
            sw.Close();

            return 0;
        }

        private void ToolStripMenuItem_projectManage_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "MainForm";
            dlg.scriptManager = this.ScriptManager;
            dlg.AppInfo = this.AppInfo;
            dlg.DataDir = this.DataDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        string m_strProjectName = "";

        // 执行统计方案
        private void toolStripMenuItem_runProject_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 出现对话框，询问Project名字
            GetProjectNameDlg dlg = new GetProjectNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.scriptManager = this.ScriptManager;
            dlg.ProjectName = this.m_strProjectName;
            dlg.NoneProject = false;

            this.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.m_strProjectName = dlg.ProjectName;

            //
            string strProjectLocate = "";
            // 获得方案参数
            // strProjectNamePath	方案名，或者路径
            // return:
            //		-1	error
            //		0	not found project
            //		1	found
            int nRet = this.ScriptManager.GetProjectData(
                dlg.ProjectName,
                out strProjectLocate);
            if (nRet == 0)
            {
                strError = "方案 " + dlg.ProjectName + " 没有找到...";
                goto ERROR1;
            }
            if (nRet == -1)
            {
                strError = "scriptManager.GetProjectData() error ...";
                goto ERROR1;
            }

            // 
            nRet = RunScript(dlg.ProjectName,
                strProjectLocate,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int RunScript(string strProjectName,
            string strProjectLocate,
            out string strError)
        {
            EnableControls(false);

            Stop = new DigitalPlatform.Stop();
            Stop.Register(stopManager, true);	// 和容器关联

            this.Stop.OnStop += new StopEventHandler(this.DoStop);
            this.Stop.Initial("正在执行脚本 ...");
            this.Stop.BeginLoop();

            try
            {

                int nRet = 0;
                strError = "";

                this.objStatis = null;
                this.AssemblyMain = null;

                // 2009/11/5 
                // 防止以前残留的打开的文件依然没有关闭
                Global.ForceGarbageCollection();

                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out objStatis,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                objStatis.ProjectDir = strProjectLocate;
                // objStatis.Console = this.Console;

                // 执行脚本的Main()

                if (objStatis != null)
                {
                    EventArgs args = new EventArgs();
                    objStatis.Main(this, args);
                }

                return 0;
            ERROR1:
                return -1;

            }
            catch (Exception ex)
            {
                strError = "脚本 '" + strProjectName + "' 执行过程抛出异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                this.Stop.EndLoop();
                this.Stop.OnStop -= new StopEventHandler(this.DoStop);
                this.Stop.Initial("");

                this.AssemblyMain = null;

                if (Stop != null) // 脱离关联
                {
                    Stop.Unregister();	// 和容器关联
                    Stop = null;
                }
                EnableControls(true);
            }
        }

        // 准备脚本环境
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            out MainFormHost objStatis,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~mainform_statis_main_" + Convert.ToString(this.StatisAssemblyVersion++) + ".dll");    // ++

            string strLibPaths = "\"" + this.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.Client.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\digitalplatform.statis.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.Script.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "MainForm",
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
                MessageBox.Show(this, strWarning);
            }


            this.AssemblyMain = Assembly.LoadFrom(strMainCsDllName);
            if (this.AssemblyMain == null)
            {
                strError = "LoadFrom " + strMainCsDllName + " fail";
                goto ERROR1;
            }

            // 得到Assembly中MainFormHost派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                this.AssemblyMain,
                "dp2Circulation.MainFormHost");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.MainFormHost 派生类。";
                goto ERROR1;
            }
            // new一个Statis派生对象
            objStatis = (MainFormHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Statis派生类设置参数
            objStatis.MainForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

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

        #region 为汉字加拼音相关功能

        // 把字符串中的汉字转换为四角号码
        // parameters:
        //      bLocal  是否从本地获取四角号码
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        /// <summary>
        /// 把字符串中的汉字转换为四角号码
        /// </summary>
        /// <param name="bLocal">是否从本地获取四角号码</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="sjhms">返回四角号码字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常</returns>
        public int HanziTextToSjhm(
            bool bLocal,
            string strText,
            out List<string> sjhms,
            out string strError)
        {
            strError = "";
            sjhms = new List<string>();

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                    continue;

                // 汉字
                string strHanzi = "";
                strHanzi += ch;


                string strResultSjhm = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.LoadQuickSjhm(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.QuickSjhm.GetSjhm(
                        strHanzi,
                        out strResultSjhm,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取四角号码");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceled
                    return 0;
                }

                Debug.Assert(strResultSjhm != "", "");

                strResultSjhm = strResultSjhm.Trim();
                sjhms.Add(strResultSjhm);
            }

            return 1;   // 正常结束
        }

        GcatServiceClient m_gcatClient = null;
        string m_strPinyinGcatID = "";
        bool m_bSavePinyinGcatID = false;

        // 汉字字符串转换为拼音。兼容以前版本
        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
        /// <summary>
        /// 汉字字符串转换为拼音，智能方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int SmartHanziTextToPinyin(
            IWin32Window owner,
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            return SmartHanziTextToPinyin(
                owner,
                strText,
                style,
                false,
                out strPinyin,
                out strError);
        }

        // 汉字字符串转换为拼音。新版本
        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
        /// <summary>
        /// 汉字字符串转换为拼音，智能方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int SmartHanziTextToPinyin(
            IWin32Window owner,
            string strText,
            PinyinStyle style,
            bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            bool bNotFoundPinyin = false;   // 是否出现过没有找到拼音、只能把汉字放入结果字符串的情况

            Stop new_stop = new DigitalPlatform.Stop();
            new_stop.Register(this.stopManager, true);	// 和容器关联
            new_stop.OnStop += new StopEventHandler(new_stop_OnStop);
            new_stop.Initial("正在获得 '" + strText + "' 的拼音信息 (从服务器 " + this.PinyinServerUrl + ")...");
            new_stop.BeginLoop();

            m_gcatClient = null;
            try
            {

                m_gcatClient = GcatNew.CreateChannel(this.PinyinServerUrl);

            REDO_GETPINYIN:
                int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字
                string strPinyinXml = "";
                // return:
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                int nRet = GcatNew.GetPinyin(
                    new_stop,
                    m_gcatClient,
                    m_strPinyinGcatID,
                    strText,
                    out strPinyinXml,
                    out strError);
                if (nRet == -1)
                {
                    if (new_stop != null && new_stop.State != 0)
                        return 0;

                    DialogResult result = MessageBox.Show(owner,
    "从服务器 '" + this.PinyinServerUrl + "' 获取拼音的过程出错:\r\n" + strError + "\r\n\r\n是否要临时改为使用本机加拼音功能? \r\n\r\n(注：临时改用本机拼音的状态在程序退出时不会保留。如果要永久改用本机拼音方式，请使用主菜单的“参数配置”命令，将“服务器”属性页的“拼音服务器URL”内容清空)",
    "EntityForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        this.ForceUseLocalPinyinFunc = true;
                        strError = "将改用本机拼音，请重新操作一次。(本次操作出错: " + strError + ")";
                        return -1;
                    }
                    strError = " " + strError;
                    return -1;
                }

                if (nRet == -2)
                {
                    IdLoginDialog login_dlg = new IdLoginDialog();
                    login_dlg.Text = "获得拼音 -- "
                        + ((string.IsNullOrEmpty(this.m_strPinyinGcatID) == true) ? "请输入ID" : strError);
                    login_dlg.ID = this.m_strPinyinGcatID;
                    login_dlg.SaveID = this.m_bSavePinyinGcatID;
                    login_dlg.StartPosition = FormStartPosition.CenterScreen;
                    if (login_dlg.ShowDialog(owner) == DialogResult.Cancel)
                    {
                        return 0;
                    }

                    this.m_strPinyinGcatID = login_dlg.ID;
                    this.m_bSavePinyinGcatID = login_dlg.SaveID;
                    goto REDO_GETPINYIN;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strPinyinXml);
                }
                catch (Exception ex)
                {
                    strError = "strPinyinXml装载到XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                foreach (XmlNode nodeWord in dom.DocumentElement.ChildNodes)
                {
                    if (nodeWord.NodeType == XmlNodeType.Text)
                    {
                        SelPinyinDlg.AppendText(ref strPinyin, nodeWord.InnerText);
                        nStatus = 0;
                        continue;
                    }

                    if (nodeWord.NodeType != XmlNodeType.Element)
                        continue;

                    string strWordPinyin = DomUtil.GetAttr(nodeWord, "p");
                    if (string.IsNullOrEmpty(strWordPinyin) == false)
                        strWordPinyin = strWordPinyin.Trim();

                    // 目前只取多套读音的第一套
                    nRet = strWordPinyin.IndexOf(";");
                    if (nRet != -1)
                        strWordPinyin = strWordPinyin.Substring(0, nRet).Trim();

                    string[] pinyin_parts = strWordPinyin.Split(new char[] { ' ' });
                    int index = 0;
                    // 让选择多音字
                    foreach (XmlNode nodeChar in nodeWord.ChildNodes)
                    {
                        if (nodeChar.NodeType == XmlNodeType.Text)
                        {
                            SelPinyinDlg.AppendText(ref strPinyin, nodeChar.InnerText);
                            nStatus = 0;
                            continue;
                        }

                        string strHanzi = nodeChar.InnerText;
                        string strCharPinyins = DomUtil.GetAttr(nodeChar, "p");

                        if (String.IsNullOrEmpty(strCharPinyins) == true)
                        {
                            strPinyin += strHanzi;
                            nStatus = 0;
                            index++;
                            continue;
                        }

                        if (strCharPinyins.IndexOf(";") == -1)
                        {
                            DomUtil.SetAttr(nodeChar, "sel", strCharPinyins);
                            SelPinyinDlg.AppendPinyin(ref strPinyin,
                                SelPinyinDlg.ConvertSinglePinyinByStyle(
                                    strCharPinyins,
                                    style)
                                    );
                            nStatus = 2;
                            index++;
                            continue;
                        }

#if _TEST_PINYIN
                        // 调试！
                        string[] parts = strCharPinyins.Split(new char[] {';'});
                        {
                            DomUtil.SetAttr(nodeChar, "sel", parts[0]);
                            AppendPinyin(ref strPinyin, parts[0]);
                            nStatus = 2;
                            index++;
                            continue;
                        }
#endif


                        string strSampleText = "";
                        int nOffs = -1;
                        SelPinyinDlg.GetOffs(dom.DocumentElement,
                            nodeChar,
                            out strSampleText,
                            out nOffs);

                        {	// 如果是多个拼音
                            SelPinyinDlg dlg = new SelPinyinDlg();
                            //float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            //float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            MainForm.SetControlFont(dlg, this.Font, false);
                            // 维持字体的原有大小比例关系
                            //dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            //dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            // 这个对话框比较特殊 MainForm.SetControlFont(dlg, this.Font, false);

                            dlg.Text = "请选择汉字 '" + strHanzi + "' 的拼音 (来自服务器 " + this.PinyinServerUrl + ")";
                            dlg.SampleText = strSampleText;
                            dlg.Offset = nOffs;
                            dlg.Pinyins = strCharPinyins;
                            if (index < pinyin_parts.Length)
                                dlg.ActivePinyin = pinyin_parts[index];
                            dlg.Hanzi = strHanzi;

                            if (bAutoSel == true
                                && string.IsNullOrEmpty(dlg.ActivePinyin) == false)
                            {
                                dlg.ResultPinyin = dlg.ActivePinyin;
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else
                            {
                                this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                                dlg.ShowDialog(owner);

                                this.AppInfo.UnlinkFormState(dlg);
                            }

                            Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                            if (dlg.DialogResult == DialogResult.Abort)
                            {
                                return 0;   // 用户希望整个中断
                            }

                            DomUtil.SetAttr(nodeChar, "sel", dlg.ResultPinyin);

                            if (dlg.DialogResult == DialogResult.Cancel)
                            {
                                SelPinyinDlg.AppendText(ref strPinyin, strHanzi);
                                nStatus = 2;
                                bNotFoundPinyin = true;
                            }
                            else if (dlg.DialogResult == DialogResult.OK)
                            {
                                SelPinyinDlg.AppendPinyin(ref strPinyin,
                                    SelPinyinDlg.ConvertSinglePinyinByStyle(
                                    dlg.ResultPinyin,
                                    style)
                                    );
                                nStatus = 2;
                            }
                            else
                            {
                                Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                            }

                            index++;
                        }
                    }
                }

#if _TEST_PINYIN
#else
                // 2014/10/22
                // 删除 word 下的 Text 节点
                XmlNodeList text_nodes = dom.DocumentElement.SelectNodes("word/text()");
                foreach (XmlNode node in text_nodes)
                {
                    Debug.Assert(node.NodeType == XmlNodeType.Text, "");
                    node.ParentNode.RemoveChild(node);
                }


                // 把没有p属性的<char>元素去掉，以便上传
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//char");
                foreach (XmlNode node in nodes)
                {
                    string strP = DomUtil.GetAttr(node, "p");
                    string strSelValue = DomUtil.GetAttr(node, "sel");  // 2013/9/13

                    if (string.IsNullOrEmpty(strP) == true
                        || string.IsNullOrEmpty(strSelValue) == true)
                    {
                        XmlNode parent = node.ParentNode;
                        parent.RemoveChild(node);

                        // 把空的<word>元素删除
                        if (parent.Name == "word"
                            && parent.ChildNodes.Count == 0
                            && parent.ParentNode != null)
                        {
                            parent.ParentNode.RemoveChild(parent);
                        }
                    }

                    // TODO: 一个拼音，没有其他选择的，是否就不上载了？
                    // 注意，前端负责新创建的拼音仍需上载；只是当初原样从服务器过来的，不用上载了
                }

                if (dom.DocumentElement.ChildNodes.Count > 0)
                {
                    // return:
                    //      -2  strID验证失败
                    //      -1  出错
                    //      0   成功
                    nRet = GcatNew.SetPinyin(
                        new_stop,
                        m_gcatClient,
                        "",
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (nRet == -1)
                    {
                        if (new_stop != null && new_stop.State != 0)
                            return 0;
                        return -1;
                    }
                }
#endif

                if (bNotFoundPinyin == false)
                    return 1;   // 正常结束

                return 2;   // 结果字符串中有没有找到拼音的汉字
            }
            finally
            {
                new_stop.EndLoop();
                new_stop.OnStop -= new StopEventHandler(new_stop_OnStop);
                new_stop.Initial("");
                new_stop.Unregister();
                if (m_gcatClient != null)
                {
                    m_gcatClient.Close();
                    m_gcatClient = null;
                }
            }
        }

        void new_stop_OnStop(object sender, StopEventArgs e)
        {
            if (this.m_gcatClient != null)
            {
                this.m_gcatClient.Abort();
            }
        }

        // 把字符串中的汉字和拼音分离
        // parameters:
        //      bLocal  是否从本机获取拼音
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        /// <summary>
        /// 汉字字符串转换为拼音，普通方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="bLocal">是否从本地获取拼音信息</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int HanziTextToPinyin(
            IWin32Window owner,
            bool bLocal,
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";
            bool bNotFoundPinyin = false;   // 是否出现过没有找到拼音、只能把汉字放入结果字符串的情况
            string strHanzi;
            int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                strHanzi = "";

                if (ch >= 0 && ch <= 128)
                {
                    if (nStatus == 2)
                        strPinyin += " ";

                    strPinyin += ch;

                    if (ch == ' ')
                        nStatus = 1;
                    else
                        nStatus = 0;

                    continue;
                }
                else
                {	// 汉字
                    strHanzi += ch;
                }

                // 汉字前面出现了英文或者汉字，中间间隔空格
                if (nStatus == 2 || nStatus == 0)
                    strPinyin += " ";

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(strHanzi) != -1)
                {
                    strPinyin += strHanzi;	// 放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                // 获得拼音
                string strResultPinyin = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.LoadQuickPinyin(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.QuickPinyin.GetPinyin(
                        strHanzi,
                        out strResultPinyin,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取拼音");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	
                    // canceled
                    strPinyin += strHanzi;	// 只好将汉字放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// 如果是多个拼音
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    //float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    //float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    MainForm.SetControlFont(dlg, this.Font, false);
                    // 维持字体的原有大小比例关系
                    //dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    //dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    // 这个对话框比较特殊 MainForm.SetControlFont(dlg, this.Font, false);

                    dlg.Text = "请选择汉字 '" + strHanzi + "' 的拼音 (来自本机)";
                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                    dlg.ShowDialog(owner);

                    this.AppInfo.UnlinkFormState(dlg);

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        strPinyin += strHanzi;
                        bNotFoundPinyin = true;
                    }
                    else if (dlg.DialogResult == DialogResult.OK)
                    {
                        strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                            dlg.ResultPinyin,
                            style);
                    }
                    else if (dlg.DialogResult == DialogResult.Abort)
                    {
                        return 0;   // 用户希望整个中断
                    }
                    else
                    {
                        Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                    }
                }
                else
                {
                    // 单个拼音

                    strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                        strResultPinyin,
                        style);
                }
                nStatus = 2;
            }

            if (bNotFoundPinyin == false)
                return 1;   // 正常结束

            return 2;   // 结果字符串中有没有找到拼音的汉字
        }

        // parameters:
        //      strIndicator    字段指示符。如果用null调用，则表示不对指示符进行筛选
        // return:
        //      0   没有找到匹配的配置事项
        //      >=1 找到。返回找到的配置事项个数
        /// <summary>
        /// 获得和一个字段相关的拼音配置事项集合
        /// </summary>
        /// <param name="cfg_dom">存储了配置信息的 XmlDocument 对象</param>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strIndicator">字段指示符</param>
        /// <param name="cfg_items">返回匹配的配置事项集合</param>
        /// <returns>0: 没有找到匹配的配置事项; >=1: 找到。值为配置事项个数</returns>
        public static int GetPinyinCfgLine(XmlDocument cfg_dom,
            string strFieldName,
            string strIndicator,
            out List<PinyinCfgItem> cfg_items)
        {
            cfg_items = new List<PinyinCfgItem>();

            XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                PinyinCfgItem item = new PinyinCfgItem(node);

                if (item.FieldName != strFieldName)
                    continue;

                if (string.IsNullOrEmpty(item.IndicatorMatchCase) == false
                    && string.IsNullOrEmpty(strIndicator) == false)
                {
                    if (MarcUtil.MatchIndicator(item.IndicatorMatchCase, strIndicator) == false)
                        continue;
                }

                cfg_items.Add(item);
            }

            return cfg_items.Count;
        }

        // 包装后的 汉字到拼音 函数
        // parameters:
        // return:
        //      -1  出错
        //      0   用户中断选择
        //      1   成功
        /// <summary>
        /// 汉字字符串转换为拼音
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strHanzi">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int GetPinyin(
            IWin32Window owner,
            string strHanzi,
            PinyinStyle style,  // PinyinStyle.None,
            bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";
            int nRet = 0;

            // 把字符串中的汉字和拼音分离
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                nRet = this.HanziTextToPinyin(
                    owner,
                    true,	// 本地，快速
                    strHanzi,
                    style,
                    out strPinyin,
                    out strError);
            }
            else
            {
                // 汉字字符串转换为拼音
                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                // return:
                //      -1  出错
                //      0   用户希望中断
                //      1   正常
                nRet = this.SmartHanziTextToPinyin(
                    owner,
                    strHanzi,
                    style,
                    bAutoSel,
                    out strPinyin,
                    out strError);
            }
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "用户中断。拼音子字段内容可能不完整。";
                return 0;
            }

            return 1;
        }
#if NO
        // 包装后的 汉字到拼音 函数
        // parameters:
        // return:
        //      -1  出错
        //      0   用户中断选择
        //      1   成功
        public int HanziTextToPinyin(string strHanzi,
            bool bAutoSel,
            PinyinStyle style,  // PinyinStyle.None,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";
            int nRet = 0;

            // 把字符串中的汉字和拼音分离
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                nRet = this.HanziTextToPinyin(
                    this,
                    true,	// 本地，快速
                    strHanzi,
                    style,
                    out strPinyin,
                    out strError);
            }
            else
            {
                // 汉字字符串转换为拼音
                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                // return:
                //      -1  出错
                //      0   用户希望中断
                //      1   正常
                nRet = this.SmartHanziTextToPinyin(
                    this,
                    strHanzi,
                    style,
                    bAutoSel,
                    out strPinyin,
                    out strError);
            }
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "用户中断。拼音子字段内容可能不完整。";
                return 0;
            }

            return 1;
        }
#endif

        // parameters:
        //      strPrefix   要加入拼音子字段内容前部的前缀字符串。例如 {cr:NLC} 或 {cr:CALIS}
        // return:
        //      -1  出错。包括中断的情况
        //      0   正常
        /// <summary>
        /// 为 MarcRecord 对象内的记录加拼音
        /// </summary>
        /// <param name="record">MARC 记录对象</param>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="style">风格</param>
        /// <param name="strPrefix">前缀字符串。缺省为空</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <returns>-1: 出错。包括中断的情况; 0: 正常</returns>
        public int AddPinyin(
            MarcRecord record,
            string strCfgXml,
            PinyinStyle style = PinyinStyle.None,
            string strPrefix = "",
            bool bAutoSel = false)
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml装载到XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            MarcNodeList fields = record.select("field");

            foreach (MarcField field in fields)
            {

                List<PinyinCfgItem> cfg_items = null;
                int nRet = GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                string strHanzi = "";

                string strFieldPrefix = "";

                // 2012/11/5
                // 观察字段内容前面的 {} 部分
                {
                    string strCmd = StringUtil.GetLeadingCommand(field.Content);
                    if (string.IsNullOrEmpty(strRuleParam) == false
                        && string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strCurRule = strCmd.Substring(3);
                        if (strCurRule != strRuleParam)
                            continue;
                    }
                    else if (string.IsNullOrEmpty(strCmd) == false)
                    {
                        strFieldPrefix = "{" + strCmd + "}";
                    }
                }

                // 2012/11/5
                // 观察 $* 子字段
                {
                    MarcNodeList subfields = field.select("subfield[@name='*']");
                    //

                    if (subfields.count > 0)
                    {
                        string strCurStyle = subfields[0].Content;
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && strCurStyle != strRuleParam)
                            continue;
                        else if (string.IsNullOrEmpty(strCurStyle) == false)
                        {
                            strFieldPrefix = "{cr:" + strCurStyle + "}";
                        }
                    }
                }

                foreach (PinyinCfgItem item in cfg_items)
                {
                    for (int k = 0; k < item.From.Length; k++)
                    {
                        if (item.From.Length != item.To.Length)
                        {
                            strError = "配置事项 fieldname='" + item.FieldName + "' from='" + item.From + "' to='" + item.To + "' 其中from和to参数值的字符数不等";
                            goto ERROR1;
                        }

                        string from = new string(item.From[k], 1);
                        string to = new string(item.To[k], 1);

                        // 删除已经存在的目标子字段
                        field.select("subfield[@name='" + to + "']").detach();

                        MarcNodeList subfields = field.select("subfield[@name='" + from + "']");

                        foreach (MarcSubfield subfield in subfields)
                        {
                            strHanzi = subfield.Content;

                            if (DetailHost.ContainHanzi(strHanzi) == false)
                                continue;

                            string strSubfieldPrefix = "";  // 当前子字段内容本来具有的前缀

                            // 检查内容前部可能出现的 {} 符号
                            string strCmd = StringUtil.GetLeadingCommand(strHanzi);
                            if (string.IsNullOrEmpty(strRuleParam) == false
                                && string.IsNullOrEmpty(strCmd) == false
                                && StringUtil.HasHead(strCmd, "cr:") == true)
                            {
                                string strCurRule = strCmd.Substring(3);
                                if (strCurRule != strRuleParam)
                                    continue;   // 当前子字段属于和strPrefix表示的不同的编目规则，需要跳过，不给加拼音
                                strHanzi = strHanzi.Substring(strPrefix.Length); // 去掉 {} 部分
                            }
                            else if (string.IsNullOrEmpty(strCmd) == false)
                            {
                                strHanzi = strHanzi.Substring(strCmd.Length + 2); // 去掉 {} 部分
                                strSubfieldPrefix = "{" + strCmd + "}";
                            }

                            string strPinyin;

#if NO
                            // 把字符串中的汉字和拼音分离
                            // return:
                            //      -1  出错
                            //      0   用户希望中断
                            //      1   正常
                            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
                               || this.ForceUseLocalPinyinFunc == true)
                            {
                                nRet = this.HanziTextToPinyin(
                                    this,
                                    true,	// 本地，快速
                                    strHanzi,
                                    style,
                                    out strPinyin,
                                    out strError);
                            }
                            else
                            {
                                // 汉字字符串转换为拼音
                                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                                // return:
                                //      -1  出错
                                //      0   用户希望中断
                                //      1   正常
                                nRet = this.SmartHanziTextToPinyin(
                                    this,
                                    strHanzi,
                                    style,
                                    bAutoSel,
                                    out strPinyin,
                                    out strError);
                            }
#endif
                            nRet = this.GetPinyin(
                                this,
                                strHanzi,
                                style,
                                bAutoSel,
                                out strPinyin,
                                out strError);
                            if (nRet == -1)
                            {
                                goto ERROR1;
                            }
                            if (nRet == 0)
                            {
                                strError = "用户中断。拼音子字段内容可能不完整。";
                                goto ERROR1;
                            }

                            string strContent = strPinyin;

                            if (string.IsNullOrEmpty(strPrefix) == false)
                                strContent = strPrefix + strPinyin;
                            else if (string.IsNullOrEmpty(strSubfieldPrefix) == false)
                                strContent = strSubfieldPrefix + strPinyin;

                            subfield.after(MarcQuery.SUBFLD + to + strPinyin);
                        }
                    }
                }
            }

            return 0;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
            {
                if (strError[0] != ' ')
                    MessageBox.Show(this, strError);
            }
            return -1;
        }

        /// <summary>
        /// 为 MarcRecord 对象内的记录删除拼音
        /// </summary>
        /// <param name="record">MARC 记录对象</param>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="strPrefix">前缀字符串。缺省为空</param>
        public void RemovePinyin(
            MarcRecord record,
            string strCfgXml,
            string strPrefix = "")
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml装载到XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            MarcNodeList fields = record.select("field");

            foreach (MarcField field in fields)
            {

                List<PinyinCfgItem> cfg_items = null;
                int nRet = GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,    // TODO: 可以不考虑指示符的情况，扩大删除的搜寻范围
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                string strField = field.Text;

                // 观察字段内容前面的 {} 部分
                if (string.IsNullOrEmpty(strRuleParam) == false)
                {
                    string strCmd = StringUtil.GetLeadingCommand(field.Content);
                    if (string.IsNullOrEmpty(strRuleParam) == false
                        && string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strCurRule = strCmd.Substring(3);
                        if (strCurRule != strRuleParam)
                            continue;
                    }
                }

                // 2012/11/6
                // 观察 $* 子字段
                if (string.IsNullOrEmpty(strRuleParam) == false)
                {
                    MarcNodeList subfields = field.select("subfield[@name='*']");
                    if (subfields.count > 0)
                    {
                        string strCurStyle = subfields[0].Content;
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && strCurStyle != strRuleParam)
                            continue;
                    }
                }

                foreach (PinyinCfgItem item in cfg_items)
                {
                    for (int k = 0; k < item.To.Length; k++)
                    {
                        string to = new string(item.To[k], 1);
                        if (string.IsNullOrEmpty(strPrefix) == true)
                        {
                            // 删除已经存在的目标子字段
                            field.select("subfield[@name='" + to + "']").detach();
                        }
                        else
                        {
                            MarcNodeList subfields = field.select("subfield[@name='" + to + "']");

                            // 只删除具有特定前缀的内容的子字段
                            foreach (MarcSubfield subfield in subfields)
                            {
                                string strContent = subfield.Content;
                                if (subfield.Content.Length == 0)
                                    subfields.detach(); // 空内容的子字段要删除
                                else
                                {
                                    if (StringUtil.HasHead(subfield.Content, strPrefix) == true)
                                        subfields.detach();
                                }
                            }
                        }
                    }
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #endregion

        #region QR 识别

        // 当前所允许的输入类型
        InputType m_inputType = InputType.None;

        // 当输入焦点进入 读者标识 编辑区 的时候触发
        internal void EnterPatronIdEdit(InputType inputtype)
        {
            m_inputType = inputtype;
            this.qrRecognitionControl1.StartCatch();
        }

        // 当输入焦点离开 读者标识 编辑区 的时候触发
        internal void LeavePatronIdEdit()
        {
            this.qrRecognitionControl1.EndCatch();
            // m_bDisableCamera = false;
        }

        // 清除防止重复的缓存条码号
        public void ClearQrLastText()
        {
            this.qrRecognitionControl1.LastText = "";
        }

        bool m_bDisableCamera = false;
        // string _cameraName = "";

        /// <summary>
        /// 摄像头禁止捕获
        /// </summary>
        public void DisableCamera()
        {
            //    _cameraName = this.qrRecognitionControl1.CurrentCamera;
            if (this.qrRecognitionControl1.InCatch == true)
            {
                this.qrRecognitionControl1.EndCatch();
                this.m_bDisableCamera = true;

                // this.qrRecognitionControl1.CurrentCamera = "";
            }
        }

        /// <summary>
        /// 摄像头恢复捕获
        /// </summary>
        public void EnableCamera()
        {
            //    this.qrRecognitionControl1.CurrentCamera = _cameraName;
            if (m_bDisableCamera == true)
            {
                this.qrRecognitionControl1.StartCatch();
                this.m_bDisableCamera = false;
            }
        }

        void qrRecognitionControl1_Catched(object sender, DigitalPlatform.Drawing.CatchedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text) == true)
                return;

            int nHitCount = 0;  // 匹配的次数
            if ((this.m_inputType & InputType.QR) == InputType.QR)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.QR_CODE) != 0)
                    nHitCount++;
            }
            // 检查是否属于 PQR 二维码
            if ((this.m_inputType & InputType.PQR) == InputType.PQR)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.QR_CODE) != 0
                    && StringUtil.HasHead(e.Text, "PQR:") == true)
                    nHitCount++;
            }
            // 检查是否属于 ISBN 一维码
            if ((this.m_inputType & InputType.EAN_BARCODE) == InputType.EAN_BARCODE)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.EAN_13) != 0
                    /* && IsbnSplitter.IsIsbn13(e.Text) == true */)
                    nHitCount++;
            }
            // 检查是否属于普通一维码
            if ((this.m_inputType & InputType.NORMAL_BARCODE) == InputType.NORMAL_BARCODE)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.All_1D) > 0)
                    nHitCount++;
            }

            if (nHitCount > 0)
            {
                // SendKeys.Send(e.Text + "\r");
                Invoke(new Action<string>(SendKey), e.Text + "\r");
            }
            else
            {
                // TODO: 警告
            }
        }

        private void SendKey(string strText)
        {
            SendKeys.Send(strText);
        }

        #endregion

        private void MenuItem_openUserFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
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
                this.OperHistory.ClearHtml();
            }
            else if (this.tabControl_panelFixed.SelectedTab == this.tabPage_camera)
            {
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
#if NO
            NewInventoryForm form = new NewInventoryForm();
            form.MdiParent = this;
            form.Show();
#endif
            // OpenWindow<NewInventoryForm>();
            OpenWindow<InventoryForm>();

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

        void SetTitle()
        {
            if (this.TestMode == true)
                this.Text = "dp2Circulation V2 -- 内务 [评估模式]";
            else
                this.Text = "dp2Circulation V2 -- 内务";

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
                    this.AppInfo.SetString("main_form", "last_mode", "community");
                    return 0;
                }
            }
            else
            {
                this.TestMode = false;
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
            string[] parts = strFuncList.Split(new char[] {','});
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
                // 覆盖写入 运行模式 信息，防止用户作弊
                // 小型版没有对应的评估模式
                this.AppInfo.SetString("main_form", "last_mode", "test");
                return;
            }
            else if (strSerialCode == "community")
            {
                this.TestMode = false;
                this.AppInfo.SetString("main_form", "last_mode", "community");
                return;
            }
            else
            {
                this.TestMode = false;
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
                    Application.Exit();
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

        private void MenuItem_openEntityRegisterForm_Click(object sender, EventArgs e)
        {
            OpenWindow<EntityRegisterForm>();
        }

        private void MenuItem_openEntityRegisterWizard_Click(object sender, EventArgs e)
        {
            if (this.ServerVersion < 2.48)
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

                            this.PrepareSearch(true);
                            try
                            {
                                nRet = EntityRegisterWizard.DetectAccess(this.Channel,
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
                                this.EndSearch();
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
                strError = ex.Message;
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
                strError = "获得服务器 "+strUrl1+" 的 UID 时出错: " + strError;
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
            catch (Exception ex)
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
            if (this.Channel != null && string.IsNullOrEmpty(this.Channel.UserName) == false)
                return this.Channel.UserName;
            // TODO: 或者迫使登录一次
            return AppInfo.GetString(
                    "default_account",
                    "username",
                    "");
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
                string strServerUrl = ReportForm.GetValidPathString(this.LibraryServerUrl.Replace("/","_"));
                string strDirectory = Path.Combine(this.UserDir, "servers\\" + strServerUrl);
                PathUtil.CreateDirIfNeed(strDirectory);
                return strDirectory;
            }
        }

        private void MenuItem_openArrivedSearchForm_Click(object sender, EventArgs e)
        {
            if (this.ServerVersion < 2.47)
            {
                MessageBox.Show(this, "dp2library 版本 2.47 和以上才能使用 预约到书查询窗");
                return;
            }
            OpenWindow<ArrivedSearchForm>();
        }

        private void MenuItem_openReservationListForm_Click(object sender, EventArgs e)
        {
            if (this.ServerVersion < 2.47)
            {
                MessageBox.Show(this, "dp2library 版本 2.47 和以上才能使用 预约响应窗");
                return;
            }
            OpenWindow<ReservationListForm>();
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