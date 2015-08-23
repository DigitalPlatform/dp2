using System;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关初始化各种信息的功能
    /// </summary>
    public partial class MainForm
    {
        // 程序启动时候需要执行的初始化操作
        // 这些操作只需要执行一次。也就是说，和登录和连接的服务器无关。如果有关，则要放在 InitialProperties() 中
        // FormLoad() 中的许多操作应当移动到这里来，以便尽早显示出框架窗口
        void FirstInitial()
        {
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

            if (Program.IsDevelopMode() == false)
            {
                this.MenuItem_separator_function2.Visible = false;
                this.MenuItem_chatForm.Visible = false;
                this.MenuItem_messageForm.Visible = false;
                this.MenuItem_openReservationListForm.Visible = false;
                this.MenuItem_inventory.Visible = false;
            }

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
                // DataDir = Environment.CurrentDirectory;

                // 2015/8/5
                this.DataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

                // 2015/7/8
                this.UserLogDir = Path.Combine(this.UserDir, "log");
                PathUtil.CreateDirIfNeed(this.UserLogDir);

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
                string strCssUrl = PathUtil.MergePath(this.DataDir, "background.css");
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

            stopManager.Initial(this.toolButton_stop,
                (object)this.toolStripStatusLabel_main,
                (object)this.toolStripProgressBar_main);
            stopManager.OnDisplayMessage += new DisplayMessageEventHandler(stopManager_OnDisplayMessage);
            this.SetMenuItemState();

            // cfgcache
            nRet = cfgCache.Load(Path.Combine(this.DataDir, "cfgcache.xml"),
                out strError);
            if (nRet == -1)
            {
                if (IsFirstRun == false)
                    MessageBox.Show(strError);
            }

            cfgCache.TempDir = Path.Combine(this.DataDir, "cfgcache");
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
            ScriptManager.CfgFilePath = Path.Combine(this.DataDir, "mainform_statis_projects.xml");
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

            // 2015/7/19
            // 复制 datadir default_objectrights.xml --> userdir objectrights.xml
            {
                string strTargetFileName = Path.Combine(this.UserDir, "objectrights.xml");
                if (File.Exists(strTargetFileName) == false)
                {
                    string strSourceFileName = Path.Combine(this.DataDir, "default_objectrights.xml");
                    if (File.Exists(strSourceFileName) == false)
                    {
                        MessageBox.Show(this, "配置文件 '" + strSourceFileName + "' 不存在，无法复制到用户目录。\r\n\r\n建议尽量直接从 dp2003.com 以 ClickOnce 方式安装 dp2circulation，以避免绿色安装时复制配置文件不全带来的麻烦");
                        Application.Exit();
                    }
                    else
                        File.Copy(strSourceFileName, strTargetFileName, false);
                }
            }
        }

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

                // if (Program.IsDevelopMode() == true)
                {
                    // 初始化 MessageHub
                    this.MessageHub = new MessageHub();
                    this.MessageHub.Initial(this, this.webBrowser_messageHub);
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
            Stop.Initial("正在连接服务器 " + Channel.Url + " ...");
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
        public double ServerVersion { get; set; }    // = 0

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
            Stop.Initial("正在检查服务器 " + this.Channel.Url + " 的版本号, 请稍候 ...");
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
                    strError = "dp2Circulation 的评估模式只能在所连接的 dp2library 版本为 2.34 以上时才能使用 (当前 dp2library 版本为 " + this.ServerVersion.ToString() + ")";
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

        // 
        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得普通库属性列表，主要是浏览窗栏目标题
        /// 必须在InitialBiblioDbProperties() GetUtilDbProperties() 和 InitialReaderDbProperties() 以后调用
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

                List<string> dbnames = new List<string>();
                // 创建NormalDbProperties数组
                if (this.BiblioDbProperties != null)
                {
                    for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                    {
                        BiblioDbProperty biblio = this.BiblioDbProperties[i];

                        // NormalDbProperty normal = null;

                        if (String.IsNullOrEmpty(biblio.DbName) == false)
                        {
#if NO
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.DbName;
                            this.NormalDbProperties.Add(normal);
#endif
                            dbnames.Add(biblio.DbName);
                        }

                        if (String.IsNullOrEmpty(biblio.ItemDbName) == false)
                        {
#if NO
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.ItemDbName;
                            this.NormalDbProperties.Add(normal);
#endif
                            dbnames.Add(biblio.ItemDbName);
                        }

                        // 为什么以前要注释掉?
                        if (String.IsNullOrEmpty(biblio.OrderDbName) == false)
                        {
#if NO
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.OrderDbName;
                            this.NormalDbProperties.Add(normal);
#endif
                            dbnames.Add(biblio.OrderDbName);
                        }

                        if (String.IsNullOrEmpty(biblio.IssueDbName) == false)
                        {
#if NO
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.IssueDbName;
                            this.NormalDbProperties.Add(normal);
#endif
                            dbnames.Add(biblio.IssueDbName);
                        }

                        if (String.IsNullOrEmpty(biblio.CommentDbName) == false)
                        {
#if NO
                            normal = new NormalDbProperty();
                            normal.DbName = biblio.CommentDbName;
                            this.NormalDbProperties.Add(normal);
#endif
                            dbnames.Add(biblio.CommentDbName);
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

#if NO
                        NormalDbProperty normal = null;

                        normal = new NormalDbProperty();
                        normal.DbName = strDbName;
                        this.NormalDbProperties.Add(normal);
#endif
                        dbnames.Add(strDbName);
                    }
                }

#if NO
                // 2015/6/13
                if (string.IsNullOrEmpty(this.ArrivedDbName) == false)
                {
                    NormalDbProperty normal = null;
                    normal = new NormalDbProperty();
                    normal.DbName = this.ArrivedDbName;
                    this.NormalDbProperties.Add(normal);
                }
#endif

                if (this.UtilDbProperties != null)
                {
                    foreach (UtilDbProperty prop in this.UtilDbProperties)
                    {
#if NO
                    NormalDbProperty normal = null;
                    normal = new NormalDbProperty();
                    normal.DbName = prop.DbName;
                    this.NormalDbProperties.Add(normal);
#endif
                        // 为了避免因 dp2library 2.48 及以前的版本的一个 bug 引起报错
                        if (this.ServerVersion <= 2.48 && prop.Type == "amerce")
                            continue;
                        dbnames.Add(prop.DbName);
                    }
                }

                foreach(string dbname in dbnames)
                {
                    NormalDbProperty normal = null;
                    normal = new NormalDbProperty();
                    normal.DbName = dbname;
                    this.NormalDbProperties.Add(normal);
                }

                if (this.ServerVersion >= 2.23)
                {
                    // 构造文件名列表
                    List<string> filenames = new List<string>();
                    foreach (NormalDbProperty normal in this.NormalDbProperties)
                    {
                        // NormalDbProperty normal = this.NormalDbProperties[i];
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
                    foreach (NormalDbProperty normal in this.NormalDbProperties)
                    {
                        // NormalDbProperty normal = this.NormalDbProperties[i];

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
                    foreach (NormalDbProperty normal in  this.NormalDbProperties)
                    {
                        // NormalDbProperty normal = this.NormalDbProperties[i];

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

                    if (strType == "biblio" || strType == "reader")
                        continue;

                    // biblio 和 reader 以外的所有类型都会被当作实用库
                    /*
  <database type="arrived" name="预约到书" /> 
  <database type="amerce" name="违约金" /> 
  <database type="publisher" name="出版者" /> 
  <database type="inventory" name="盘点" /> 
  <database type="message" name="消息" /> 
                     * */

#if NO
                    if (strType == "zhongcihao"
                        || strType == "publisher"
                        || strType == "dictionary")
#endif
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
                        + "\r\n\r\n请用时钟窗仔细核对服务器时钟，如有必要重新设定服务器时钟为正确值。\r\n\r\n注：流通功能均采用服务器时钟，如果服务器时钟正确而本地时钟不正确，一般不会影响流通功能正常进行。";
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

    }
}
