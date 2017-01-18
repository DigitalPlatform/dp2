
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Deployment.Application;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Reflection;

using Ionic.Zip;

using Microsoft.Win32;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Install;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

using dp2LibraryXE.Properties;
using System.Threading.Tasks;

namespace dp2LibraryXE
{
    public partial class MainForm : Form
    {
        FloatingMessageForm _floatingMessage = null;

        const string default_opac_rights = "denychangemypassword,getsystemparameter,getres,search,getbiblioinfo,setbiblioinfo,getreaderinfo,writeobject,getbibliosummary,listdbfroms,simulatereader,simulateworker"
                                + ",getiteminfo,getorderinfo,getissueinfo,getcommentinfo";  // 2016/1/27

        const string localhost_opac_url = "http://localhost:8081/dp2OPAC";

        /// <summary>
        /// 数据目录
        /// </summary>
        public string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public string UserDir = "";

        public string TempDir = "";

        public string UserLogDir = ""; // 2015/8/8

        /// <summary>
        /// dp2Kernel 数据目录
        /// </summary>
        public string KernelDataDir = "";

        /// <summary>
        /// dp2Library 数据目录
        /// </summary>
        public string LibraryDataDir = "";

        /// <summary>
        /// dp2OPAC 数据目录
        /// </summary>
        public string OpacDataDir = "";

        /// <summary>
        /// dp2OPAC 应用程序目录 (虚拟目录)
        /// </summary>
        public string OpacAppDir = "";

        /// <summary>
        /// dp2 站点目录 (虚拟目录)
        /// </summary>
        public string dp2SiteDir = "";

        /// <summary>
        /// Stop 管理器
        /// </summary>
        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

        /// <summary>
        /// dp2library 服务器监听 URL 列表。一个或者多个URL。如果是多个 URL，用分号分隔
        /// </summary>
        public string LibraryServerUrlList = LibraryHost.default_single_url;    // "net.pipe://localhost/dp2library/xe"; // "net.tcp://localhost:8002/dp2library/xe";

        /// <summary>
        /// 配置存储
        /// </summary>
        public ApplicationInfo AppInfo = null;

        public MainForm()
        {
            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        private void MainForm_Load(object sender, EventArgs e)
        {
            GuiUtil.AutoSetDefaultFont(this);

            ClearForPureTextOutputing(this.webBrowser1);

#if NO
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }
#endif

            this.toolStripProgressBar_main.Visible = false;

            // 创建事件日志分支目录
            CreateEventSource();
#if LOG
            WriteLibraryEventLog("开始启动", EventLogEntryType.Information);
            WriteLibraryEventLog("当前操作系统信息：" + Environment.OSVersion.ToString(), EventLogEntryType.Information);
            WriteLibraryEventLog("当前操作系统版本号：" + Environment.OSVersion.Version.ToString(), EventLogEntryType.Information);
#endif

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length >= 2)
            {
#if LOG
                WriteLibraryEventLog("命令行参数=" + string.Join(",", args), EventLogEntryType.Information);
#endif
                // MessageBox.Show(string.Join(",", args));
                for (int i = 1; i < args.Length; i++)
                {
                    string strArg = args[i];
                    if (StringUtil.HasHead(strArg, "datadir=") == true)
                    {
                        this.DataDir = strArg.Substring("datadir=".Length);
#if LOG
                        WriteLibraryEventLog("从命令行参数得到, this.DataDir=" + this.DataDir, EventLogEntryType.Information);
#endif
                    }
                    else if (StringUtil.HasHead(strArg, "userdir=") == true)
                    {
                        this.UserDir = strArg.Substring("userdir=".Length);
#if LOG
                        WriteLibraryEventLog("从命令行参数得到, this.UserDir=" + this.UserDir, EventLogEntryType.Information);
#endif
                    }
                }
            }

            if (string.IsNullOrEmpty(this.DataDir) == true)
            {
                if (ApplicationDeployment.IsNetworkDeployed == true)
                {
#if LOG
                    WriteLibraryEventLog("从网络安装启动", EventLogEntryType.Information);
#endif
                    // MessageBox.Show(this, "network");
                    this.DataDir = Application.LocalUserAppDataPath;
                }
                else
                {
#if LOG
                    WriteLibraryEventLog("绿色安装方式启动", EventLogEntryType.Information);
#endif
                    // MessageBox.Show(this, "no network");
                    this.DataDir = Environment.CurrentDirectory;
                }
#if LOG
                WriteLibraryEventLog("普通方法得到, this.DataDir=" + this.DataDir, EventLogEntryType.Information);
#endif
            }

            if (string.IsNullOrEmpty(this.UserDir) == true)
            {
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2LibraryXE_v1");
#if LOG
                WriteLibraryEventLog("普通方法得到, this.UserDir=" + this.UserDir, EventLogEntryType.Information);
#endif
            }
            PathUtil.CreateDirIfNeed(this.UserDir);

            this.TempDir = Path.Combine(this.UserDir, "temp");
            PathUtil.CreateDirIfNeed(this.TempDir);

            // 2015/8/8
            this.UserLogDir = Path.Combine(this.UserDir, "log");
            PathUtil.CreateDirIfNeed(this.UserLogDir);

            this.AppInfo = new ApplicationInfo(Path.Combine(this.UserDir, "settings.xml"));

            this.KernelDataDir = Path.Combine(this.UserDir, "kernel_data");
            PathUtil.CreateDirIfNeed(this.KernelDataDir);

            this.LibraryDataDir = Path.Combine(this.UserDir, "library_data");
            PathUtil.CreateDirIfNeed(this.LibraryDataDir);

            this.OpacDataDir = Path.Combine(this.UserDir, "opac_data");
            PathUtil.CreateDirIfNeed(this.OpacDataDir);

            this.OpacAppDir = Path.Combine(this.UserDir, "opac_app");
            PathUtil.CreateDirIfNeed(this.OpacAppDir);

            this.dp2SiteDir = Path.Combine(this.UserDir, "dp2_site");
            PathUtil.CreateDirIfNeed(this.dp2SiteDir);

            stopManager.Initial(this.toolButton_stop,
    (object)this.toolStripStatusLabel_main,
    (object)this.toolStripProgressBar_main);

            this.AppInfo.LoadFormStates(this,
"mainformstate",
FormWindowState.Normal);
#if NO
            if (Settings.Default.WindowSize != null)
                this.Size = Settings.Default.WindowSize;
            if (Settings.Default.WindowLocation != null)
                this.Location = Settings.Default.WindowLocation;
#endif

            // cfgcache
            _versionManager.Load(Path.Combine(this.UserDir, "file_version.xml"));

            //Delegate_Initialize d = new Delegate_Initialize(Initialize);
            //this.BeginInvoke(d);
            this.BeginInvoke(new Action(Initialize));

            AutoStartDp2circulation = AutoStartDp2circulation;
        }

        // delegate void Delegate_Initialize();

        // 启动后要执行的初始化操作
        void Initialize()
        {
            string strError = "";
            int nRet = 0;

#if SN
            nRet = VerifySerialCode(out strError);
            if (nRet == -1)
            {
                Application.Exit();
                return;
            }

            GetMaxClients();
            GetLicenseType();
            GetFunction();
#else
            this.MenuItem_resetSerialCode.Visible = false;
#endif

            this._floatingMessage.Text = "正在启动 dp2Library XE，请等待 ...";

            Application.DoEvents();

            try
            {

                nRet = CopyUserBinDirectory(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "复制用户目录的 bin 子目录到程序目录时出错: " + strError);
                }

                // 首次运行自动安装数据目录
                {
                    nRet = SetupKernelDataDir(
                        true,
                        out strError);
                    if (nRet == -1)
                    {
                        WriteKernelEventLog("dp2Library XE 自动初始化数据目录出错: " + strError, EventLogEntryType.Error);
                        MessageBox.Show(this, strError);
                    }
                    else
                    {
                        WriteKernelEventLog("dp2Library XE 自动初始化数据目录成功", EventLogEntryType.Information);
                    }

                    nRet = SetupLibraryDataDir(
                        true,
                        out strError);
                    if (nRet == -1)
                    {
                        WriteLibraryEventLog("dp2Library XE 自动初始化数据目录出错: " + strError, EventLogEntryType.Error);
                        MessageBox.Show(this, strError);
                    }
                    else
                    {
                        WriteLibraryEventLog("dp2Library XE 自动初始化数据目录成功", EventLogEntryType.Information);
                    }
                }

                // 更新数据目录
                UpdateCfgs();

                // 启动两个后台服务
                nRet = dp2Kernel_start(true,
                    out strError);
                if (nRet == -1)
                {
                    WriteKernelEventLog("dp2Library XE 启动 dp2Kernel 时出错: " + strError, EventLogEntryType.Error);
                    MessageBox.Show(this, strError);
                }
                nRet = dp2Library_start(true,
                    out strError);
                if (nRet == -1)
                {
                    WriteLibraryEventLog("dp2Library XE 启动 dp2Library 时出错: " + strError, EventLogEntryType.Error);
                    MessageBox.Show(this, strError);
                }

                bool bInstalled = this.AppInfo.GetBoolean("OPAC", "installed", false);
                if (bInstalled == true)
                {
                    nRet = dp2OPAC_UpdateAppDir(false, out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "自动升级 dp2OPAC 程序目录过程出错: " + strError);

                    // 2015/7/17
                    // 自动升级 dp2OPAC 的数据目录中的 style 子目录
                    // 使用 opac_style.zip 来更新

                    // 更新 dp2OPAC 数据目录中的 style 子目录
                    // parameters:
                    //      bAuto   是否自动更新。true 表示(.zip 文件发生了变化)有必要才更新; false 表示无论如何均更新
                    nRet = UpdateOpacStyles(true,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "自动升级 dp2OPAC 数据目录的 style 子目录过程出错: " + strError);

                    // 检查当前超级用户帐户是否为空密码
                    // return:
                    //      -1  检查中出错
                    //      0   空密码
                    //      1   已经设置了密码
                    nRet = CheckNullPassword(out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "检查超级用户密码的过程出错: " + strError);

                    if (nRet == 0)
                    {
                        AutoCloseMessageBox.Show(this,
                            "当前超级用户 " + this.SupervisorUserName + " 的密码为空，如果启动 dp2OPAC，其他人将可能通过浏览器冒用此账户。\r\n\r\n请(使用 dp2circulation (内务前端))为此账户设置密码，然后重新启动 dp2libraryXE。\r\n\r\n为确保安全，本次未启动 dp2OPAC",
                            20 * 1000,
                            "dp2library XE 警告");
#if NO
                        MessageBox.Show(this,
                            "当前超级用户 " + this.SupervisorUserName + " 的密码为空，如果启动 dp2OPAC，其他人将可能通过浏览器冒用此账户。\r\n\r\n请(使用 dp2circulation (内务前端))为此账户设置密码，然后重新启动 dp2libraryXE。\r\n\r\n为确保安全，本次未启动 dp2OPAC",
                            "dp2library XE 警告",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
#endif
                    }
                    else
                    {
                        // return:
                        //      -1  出错
                        //      0   程序文件不存在
                        //      1   成功启动
                        nRet = StartIIsExpress("dp2Site", true, out strError);
                        if (nRet != 1)
                            MessageBox.Show(this, strError);
                    }
                }

                // 2014/11/16
                try
                {
                    EventWaitHandle.OpenExisting("dp2libraryXE V1 library host started").Set();
                }
                catch
                {
                }
            }
            finally
            {
#if NO
                _messageBar.Close();
                _messageBar = null;
#endif
                this._floatingMessage.Text = "";

                this.SetTitle();
            }

#if NO
            if (this.AutoStartDp2circulation == true)
            {
                try
                {
                    System.Diagnostics.Process.Start("http://dp2003.com/dp2circulation/v2/dp2circulation.application");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "启动 dp2Circulation 时出错: " + ex.Message);
                }
            }
#endif
        }

        // 将用户目录里面的 bin 子目录中的文件，拷贝覆盖到程序文件夹
        // 这个动作要在启动 dp2kernel 和 dp2library 以前做。因为这两个模块可能会挂接这些文件，如果挂接了就无法覆盖了
        int CopyUserBinDirectory(out string strError)
        {
            strError = "";

            string strSourceDir = Path.Combine(this.UserDir, "bin");
            if (Directory.Exists(strSourceDir))
            {
                string strTargetDir = Environment.CurrentDirectory;
                if (PathUtil.CopyDirectory(strSourceDir, strTargetDir, false, out strError) == -1)
                    return -1;
            }

            return 0;
        }

        public bool AutoStartDp2circulation
        {
            get
            {
                return this.AppInfo.GetBoolean("main_form", "auto_start_dp2circulation", true);
            }
            set
            {
                this.AppInfo.SetBoolean("main_form", "auto_start_dp2circulation", value);
                this.MenuItem_autoStartDp2Circulation.Checked = value;
            }
        }


        void SetListenUrl(string strMode)
        {
            // 设置监听 URL
            if (strMode == "miniServer" || strMode == "miniTest")    // "miniServer"
            {
                string strNewUrl = InputDlg.GetInput(
this,
"请指定服务器绑定的 URL",
"URL: ",
LibraryHost.default_miniserver_urls,
this.Font);
                if (strNewUrl == null)
                {
                    strNewUrl = LibraryHost.default_miniserver_urls;    //  "http://localhost:8001/dp2library/xe;net.pipe://localhost/dp2library/xe";
                    MessageBox.Show(this, "自动使用缺省的 URL " + strNewUrl);
                }

                this.AppInfo.SetString("main_form", "listening_url", strNewUrl);
                // TODO: 检查，
            }
            else
                this.AppInfo.SetString("main_form", "listening_url", LibraryHost.default_single_url);

        }

        #region 序列号机制

        // 是否为社区版
        bool _testMode = false;

        public bool TestMode
        {
            get
            {
                return this._testMode;
            }
            set
            {
                if (this._testMode != value)
                {
                    this._testMode = value;
                    SetTitle();
                }
            }
        }

        void SetTitle()
        {
            if (this.IsServer == true)
            {
                if (this.TestMode == true)
                    this.Text = "dp2Library XE 小型服务器 [社区版]";
                else
                    this.Text = "dp2Library XE 小型服务器 [专业版]";
            }
            else
            {
                if (this.TestMode == true)
                    this.Text = "dp2Library XE 单机 [社区版]";
                else
                    this.Text = "dp2Library XE 单机 [专业版]";
            }

            Assembly myAssembly = Assembly.GetAssembly(this.GetType());

            string strContent = @"
dp2Library XE
---
dp2 图书馆集成系统 图书馆应用服务器 "
                + (this.IsServer == false ? "单机" : "小型服务器")
                + (this.TestMode == false ? " [专业版]" : " [社区版]")
                + "\r\n版本: " + Program.ClientVersion
                +
@"
---
(C) 版权所有 2014-2015 数字平台(北京)软件有限责任公司
http://dp2003.com
2015 年以 Apache License Version 2.0 方式开源
http://github.com/digitalplatform/dp2"
+ (this.IsServer == false ? "" : @"
---
最大通道数： " + this.MaxClients.ToString())
     + @"
本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress()) + "\r\n---\r\n"
            + "环境:\r\n本机 .NET Framework 版本: " + myAssembly.ImageRuntimeVersion
            + "\r\n\r\n";

            AppendString(strContent);
        }
#if SN
        int _maxClients = 5;
#else
        int _maxClients = 200;
#endif
        public int MaxClients
        {
            get
            {
                return _maxClients;
            }
            set
            {
                if (_maxClients != value)
                {
                    _maxClients = value;
                    this.SetTitle();
                }
            }
        }

#if SN
        void GetMaxClients()
        {
            string strLocalString = GetEnvironmentString(this.IsServer, "");
            Hashtable table = StringUtil.ParseParameters(strLocalString);
            string strProduct = (string)table["product"];

            string strMaxClients = (string)table["clients"];
            if (string.IsNullOrEmpty(strMaxClients) == false)
            {
                int v = this.MaxClients;
                if (int.TryParse(strMaxClients, out v) == true)
                {
                    this.MaxClients = v;
                }
                else
                    throw new Exception("clients 参数值 '" + strMaxClients + "' 格式错误");
            }

            // this.SetTitle();
        }

#endif

        // 许可方式
        // "server" 表示服务器验证服务器自己的序列号，就不要求前端验证前端自己的序列号了
        string _licenseType = "";
        public string LicenseType
        {
            get
            {
                return _licenseType;
            }
            set
            {
                if (_licenseType != value)
                {
                    _licenseType = value;
                    // this.SetTitle();
                }
            }
        }

        // 许可的功能列表
        string _function = "";
        public string Function
        {
            get
            {
                return _function;
            }
            set
            {
                if (_function != value)
                {
                    _function = value;
                    // this.SetTitle();
                }
            }
        }

#if SN
        void GetLicenseType()
        {
            string strLocalString = GetEnvironmentString(this.IsServer, "");
            Hashtable table = StringUtil.ParseParameters(strLocalString);
            // string strProduct = (string)table["product"];

            this.LicenseType = (string)table["licensetype"];

            // this.SetTitle();
        }

        void GetFunction()
        {
            string strLocalString = GetEnvironmentString(this.IsServer, "");
            Hashtable table = StringUtil.ParseParameters(strLocalString);
            this.Function = (string)table["function"];
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

        // 将本地字符串匹配序列号
        bool MatchLocalString(bool bIsServer, string strSerialNumber)
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            foreach (string mac in macs)
            {
                string strLocalString = GetEnvironmentString(bIsServer, mac);
                string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                    return true;
            }

            return false;
        }

#endif

#if SN
        int VerifySerialCode(out string strError)
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
                // 如果当前窗口没有在最前面
                {
                    if (this.WindowState == FormWindowState.Minimized)
                        this.WindowState = FormWindowState.Normal;
                    this.Activate();
                    API.SetForegroundWindow(this.Handle);
                }

                FirstRunDialog first_dialog = new FirstRunDialog();
                MainForm.SetControlFont(first_dialog, this.Font);
                first_dialog.MainForm = this;
                first_dialog.Mode = this.AppInfo.GetString("main_form", "last_mode", "test");   // "standard"
                first_dialog.StartPosition = FormStartPosition.CenterScreen;
                if (first_dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                {
                    strError = "放弃";
                    return -1;
                }

                // 首次写入 运行模式 信息
                this.AppInfo.SetString("main_form", "last_mode", first_dialog.Mode);
                if (first_dialog.Mode == "test" || first_dialog.Mode == "miniTest")
                {
                    this.AppInfo.SetString("sn", "sn", first_dialog.Mode);
                    this.AppInfo.Save();
                }

                ////
                SetListenUrl(first_dialog.Mode);
            }

            // 修改前的模式
            string strOldMode = this.AppInfo.GetString("main_form", "last_mode", "test");   // "standard"

        REDO_VERIFY:
            strSerialCode = this.AppInfo.GetString("sn", "sn", "");
            if (strSerialCode == "test")
            {
                this.TestMode = true;
                // 覆盖写入 运行模式 信息，防止用户作弊
                // 小型版没有对应的评估模式
                this.AppInfo.SetString("main_form", "last_mode", strSerialCode);
                return 0;
            }
            else if (strSerialCode == "miniTest")
            {
                this.TestMode = true;
                this.AppInfo.SetString("main_form", "last_mode", strSerialCode);
                return 0;
            }
            else
                this.TestMode = false;

            // string strLocalString = GetEnvironmentString(this.IsServer);

            // string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");

            if (// strSha1 != GetCheckCode(strSerialCode)
                (MatchLocalString(this.IsServer, strSerialCode) == false
                && MatchLocalString(!this.IsServer, strSerialCode) == false)
                || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (String.IsNullOrEmpty(strSerialCode) == false)
                {
                    MessageBox.Show(this, "序列号无效。请重新输入");
                }

                // 出现设置序列号对话框
                nRet = ResetSerialCode(
                    false,
                    strSerialCode,
                    GetEnvironmentString(this.IsServer, strFirstMac));
                if (nRet == 0)
                {
                    strError = "放弃";
                    return -1;
                }
                string strMode = CannonicalizeInputMode(strOldMode, strSerialCode);
                if (strMode != strSerialCode)
                    this.AppInfo.SetString("sn", "sn", strMode);

                goto REDO_VERIFY;
            }
            return 0;
        }

#endif


#if SN
        // parameters:
        //      bServer     是否为小型服务器版本。如果是小型服务器版本，用 net.tcp 协议绑定 dp2library host；如果不是单机版本，用 net.pipe 绑定 dp2library host
        string GetEnvironmentString(bool bServer, string strMAC)
        {
            Hashtable table = new Hashtable();
            table["mac"] = strMAC;  //  SerialCodeForm.GetMacAddress();
            table["time"] = GetTimeRange();

            if (bServer == true)
                table["product"] = "dp2libraryXE server";
            else
                table["product"] = "dp2libraryXE";

#if NO
            string strMaxClients = this.AppInfo.GetString("main_form", "clients", "");
            if (string.IsNullOrEmpty(strMaxClients) == false)
                table["clients"] = strMaxClients;
#endif
            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
            if (string.IsNullOrEmpty(strSerialCode) == false)
            {
                string strExtParam = GetExtParams(strSerialCode);
                if (string.IsNullOrEmpty(strExtParam) == false)
                {
                    Hashtable ext_table = StringUtil.ParseParameters(strExtParam);
                    string strMaxClients = (string)ext_table["clients"];
                    if (string.IsNullOrEmpty(strMaxClients) == false)
                        table["clients"] = strMaxClients;

                    string strLicenseType = (string)ext_table["licensetype"];
                    if (string.IsNullOrEmpty(strLicenseType) == false)
                        table["licensetype"] = strLicenseType;

                    // 2015/11/17
                    string strFunction = (string)ext_table["function"];
                    if (string.IsNullOrEmpty(strFunction) == false)
                        table["function"] = strFunction;
                }
            }

            return StringUtil.BuildParameterString(table);
        }

        static string GetTimeRange()
        {
#if NO
            DateTime now = DateTime.Now;
            return now.Year.ToString().PadLeft(4, '0')
            + now.Month.ToString().PadLeft(2, '0');
#endif
            DateTime now = DateTime.Now;
            return now.Year.ToString().PadLeft(4, '0');
        }

        string CopyrightKey = "dp2libraryXE_sn_key";

        // return:
        //      0   Cancel
        //      1   OK
        int ResetSerialCode(
            bool bAllowSetBlank,
            string strOldSerialCode,
            string strOriginCode)
        {
            Hashtable ext_table = StringUtil.ParseParameters(strOriginCode);
            string strMAC = (string)ext_table["mac"];
            if (string.IsNullOrEmpty(strMAC) == true)
                strOriginCode = "!error";
            else
                strOriginCode = Cryptography.Encrypt(strOriginCode,
                this.CopyrightKey);
            SerialCodeForm dlg = new SerialCodeForm();
            dlg.Font = this.Font;
            dlg.DefaultCodes = new List<string>(new string[] {
                "community|社区版",
                "singleCommunity|单机版的社区版",
                "miniCommunity|小型版的社区版",
            });
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
        "确实要将序列号设置为空?\r\n\r\n(一旦将序列号设置为空，dp2Library XE 将自动退出，下次启动需要重新设置运行模式和序列号。此时可重新选择评估模式运行，但数据库数量和可修改的记录数量都会受到一定限制)",
        "dp2Library XE",
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

        [DllImport("user32.dll")]
        public extern static bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);

        [DllImport("user32.dll")]
        public extern static bool ShutdownBlockReasonDestroy(IntPtr hWnd);

        bool _skipFinalize = false; // 是否要忽略普通的结束的过程

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 警告关闭
                DialogResult result = MessageBox.Show(this,
                    "确实要退出 dp2Library XE?\r\n\r\n(本程序提供了 “dp2Library 应用服务器单机版/小型版” 的后台服务功能，一旦退出，图书馆业务前端将无法运行。平时应保持运行状态，将窗口最小化即可)",
                    "dp2Library XE",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                // Abort Shutdown
                // Process.Start("shutdown.exe", "-s -t 4");

                // http://stackoverflow.com/questions/11089259/shutdownblockreasoncreate-create-multiple-reasons-to-display-during-logoff-shu

                try
                {
                    ShutdownBlockReasonCreate(this.Handle, "正在退出 dp2Library XE，请稍候 ...");
                }
                catch (System.EntryPointNotFoundException)
                {
                    // Windows Server 2003 下面会抛出此异常
                }


                {
                    this._isBlocked = true;
                    e.Cancel = true;
                }

                this._skipFinalize = true;
                this.BeginInvoke(new Action<bool>(Finalize), true);
            }
        }

        private bool _isBlocked = false;

        protected override void WndProc(ref Message aMessage)
        {
            const int WM_QUERYENDSESSION = 0x0011;
            const int WM_ENDSESSION = 0x0016;

            if (_isBlocked && (aMessage.Msg == WM_QUERYENDSESSION || aMessage.Msg == WM_ENDSESSION))
                return;

            base.WndProc(ref aMessage);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel.Cancel();

            if (this._skipFinalize == false)
                Finalize(false);
#if NO
            this.toolStripStatusLabel_main.Text = "正在退出 dp2Library XE，请稍候 ...";
            Application.DoEvents();

            AppInfo.Save();
            AppInfo = null;	// 避免后面再用这个对象

            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = this.Size;
                Settings.Default.WindowLocation = this.Location;
            }
            else
            {
                Settings.Default.WindowSize = this.RestoreBounds.Size;
                Settings.Default.WindowLocation = this.RestoreBounds.Location;
            }

            Settings.Default.Save();

            dp2Library_stop();
            dp2Kernel_stop();
#endif
            if (_floatingMessage != null)
                _floatingMessage.Close();
        }

        void Finalize(bool bExitAndShutdown)
        {
            if (this.AppInfo != null)
            {
                AppInfo.SaveFormStates(this,
        "mainformstate");

                AppInfo.Save();
                AppInfo = null;	// 避免后面再用这个对象
            }

            if (_versionManager != null)
            {
                _versionManager.AutoSave();
            }

            try
            {
#if NO
                if (this.WindowState == FormWindowState.Normal)
                {
                    Settings.Default.WindowSize = this.Size;
                    Settings.Default.WindowLocation = this.Location;
                }
                else
                {
                    Settings.Default.WindowSize = this.RestoreBounds.Size;
                    Settings.Default.WindowLocation = this.RestoreBounds.Location;
                }

                Settings.Default.Save();
#endif
            }
            catch
            {
                // 可能在第一个进程退出的时候会遇到异常 2014/11/14
            }

            CloseIIsExpress(false);

            this.toolStripStatusLabel_main.Text = "正在退出 dp2Library XE，请稍候 ...";
            Application.DoEvents();

            dp2Library_stop();
            dp2Kernel_stop();

            // 测试用 Thread.Sleep(10000);
            if (bExitAndShutdown == true)
            {
                // MessageBox.Show(this, "end");
                try
                {
                    ShutdownBlockReasonDestroy(this.Handle);
                }
                catch (System.EntryPointNotFoundException)
                {
                }
                _isBlocked = false;

                if (
    Environment.OSVersion.Version.Major == 5 &&
    (
    Environment.OSVersion.Version.Minor == 1 || // Windows XP
    Environment.OSVersion.Version.Minor == 2)   // Windows Server 2003
    )
                {
                    // Windows XP 和 Server 2003 时候补充 shutdown 命令
                    Process.Start("shutdown.exe", "-s -t 4");
                }

                Application.Exit();
            }
        }

        private void button_setupKernelDataDir_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 安装 dp2kernel 的数据目录
            // parameters:
            //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
            int nRet = SetupKernelDataDir(
                false,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            MessageBox.Show(this, "dp2Kernel 数据目录安装成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #region dp2Kernel

        KernelHost kernel_host = null;

        int dp2Kernel_start(
            bool bAutoStart,
            out string strError)
        {
            strError = "";

            Debug.Assert(string.IsNullOrEmpty(this.KernelDataDir) == false, "");

            string strFilename = Path.Combine(this.KernelDataDir, "databases.xml");
            if (File.Exists(strFilename) == false)
            {
                strError = "dp2Kernel XE 尚未初始化";
                return 0;
            }

            if (bAutoStart == true && kernel_host != null)
            {
                strError = "dp2Kernel 先前已经启动了";
                return 0;
            }

            dp2Kernel_stop();

            kernel_host = new KernelHost();
            kernel_host.DataDir = this.KernelDataDir;
            int nRet = kernel_host.Start(out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        void dp2Kernel_stop()
        {
            if (kernel_host != null)
            {
                kernel_host.Stop();
                kernel_host = null;
            }
        }

        // 删除以前的目录
        int DeleteDataDirectory(
            string strDirectory,
            out string strError)
        {
            strError = "";

            // 检查目录是否符合规则
            // 不能使用根目录
            string strRoot = Directory.GetDirectoryRoot(strDirectory);
            if (PathUtil.IsEqual(strRoot, strDirectory) == true)
            {
                strError = "数据目录 '" + strDirectory + "' 不合法。不能是根目录";
                return -1;
            }

            // 删除本实例创建过的全部 SQL 数据库
            // parameters:
            // return:
            //      -1  出错
            //      0   databases.xml 文件不存在; 或 databases.xml 中没有任何 SQL 数据库信息
            //      1   成功删除
            int nRet = DigitalPlatform.rms.InstanceDialog.DeleteAllSqlDatabase(strDirectory,
                out strError);
            if (nRet == -1)
            {
                strError = "删除数据目录 '" + strDirectory + "' 中的全部 SQL 数据库过程出错: " + strError;
                return -1;
            }

            try
            {
                PathUtil.RemoveReadOnlyAttr(strDirectory);
            }
            catch
            {
                string strCurrentDir = Directory.GetCurrentDirectory();
                int i = 0;
                i++;
            }

            try
            {
                PathUtil.DeleteDirectory(strDirectory);
            }
            catch (Exception ex)
            {
                string strCurrentDir = Directory.GetCurrentDirectory();

                strError = "删除目录 " + strDirectory + " 的过程中出现错误: " + ex.Message + "。\r\n请手动删除此目录后，重新进行操作";
                return -1;
            }
            return 0;
        }

        // 安装 dp2kernel 的数据目录
        // parameters:
        //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
        int SetupKernelDataDir(
            bool bAutoSetup,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFilename = Path.Combine(this.KernelDataDir, "databases.xml");
            if (File.Exists(strFilename) == true)
            {
                if (bAutoSetup == true)
                {
                    strError = "dp2Kernel 数据目录先前已经安装过，本次没有重新安装";
                    return 0;
                }

                DialogResult result = MessageBox.Show(this,
    "警告：dp2Kernel 数据目录先前已经安装过了，本次重新安装，将摧毁以前的全部数据。\r\n\r\n确实要重新安装？",
    "dp2Library XE",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strError = "放弃重新安装 dp2Kernel 数据目录";
                    return 0;
                }

            }


            SelectSqlServerDialog dlg = new SelectSqlServerDialog();
            MainForm.SetControlFont(dlg, this.Font);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                strError = "放弃重新安装 dp2Kernel 数据目录";
                return 0;
            }

            dp2Kernel_stop();

            // 删除以前的目录
            nRet = DeleteDataDirectory(
                Path.GetDirectoryName(strFilename),
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            if (_messageBar != null)
                _messageBar.MessageText = "正在初始化 dp2Kernel 数据目录 ...";
#endif
            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "正在初始化 dp2Kernel 数据目录 ...";

            nRet = dp2Kernel_CreateNewDataDir(out strError);
            if (nRet == -1)
                return -1;

            // 创建/修改 databases.xml 文件
            // return:
            //      -1  error
            //      0   succeed
            nRet = dp2Kernel_createXml(
                dlg.SelectedType, // "localdb",
                this.KernelDataDir,
                out strError);
            if (nRet == -1)
                return -1;

            // 修改root用户记录文件
            // parameters:
            //      strUserName 如果为null，表示不修改用户名
            //      strPassword 如果为null，表示不修改密码
            //      strRights   如果为null，表示不修改权限
            nRet = dp2Kernel_ModifyRootUser(this.KernelDataDir,
                "root",
                "",
                null,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = dp2Kernel_start(true,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }


        // 创建数据目录，并复制进基本内容
        int dp2Kernel_CreateNewDataDir(
            out string strError)
        {
            strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "kernel_data.zip");

            // 要求在 KernelData.zip 内准备要安装的数据文件(初次安装而不是升级安装)
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

#if NO
            int nRet = PathUtil.CopyDirectory(strTempDataDir,
    this.KernelDataDir,
    true,
    out strError);
            if (nRet == -1)
            {
                strError = "拷贝临时目录 '" + strTempDataDir + "' 到数据目录 '" + this.KernelDataDir + "' 时发生错误：" + strError;
                return -1;
            }
#endif

            return 0;
        }

        int CreateLocalDBInstance(string strInstanceName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strExePath = FindExePath("sqllocaldb.exe");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "无法找到 sqllocaldb.exe 的全路径";
                return -1;
            }

            List<string> lines = new List<string>();
            lines.Add("create " + strInstanceName);
            // parameters:
            //      lines   若干行参数。每行执行一次
            //      bOutputCmdLine  在输出中是否包含命令行? 如果为 false，表示不包含命令行，只有命令结果文字
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            nRet = InstallHelper.RunCmd(
                strExePath,
                lines,
                false,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        private static int CompareByValue(string x, string y)
        {
            double v1 = 0;
            double v2 = 0;
            double.TryParse(x, out v1);
            double.TryParse(y, out v2);
            if (v1 == v2)
                return 0;
            if (v1 > v2)
                return 1;
            return -1;
        }

        // http://csharptest.net/526/how-to-search-the-environments-path-for-an-exe-or-dll/
        /// <summary>
        /// Expands environment variables and, if unqualified, locates the exe in the working directory
        /// or the evironment's path.
        /// </summary>
        /// <param name="exe">The name of the executable file</param>
        /// <returns>The fully-qualified path to the file</returns>
        /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == String.Empty)
                {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
                // throw new FileNotFoundException(new FileNotFoundException().Message, exe);
                return null;
            }
            return Path.GetFullPath(exe);
        }



        // 创建/修改 databases.xml 文件
        // parameters:
        //      strSqlServerType    sqlite/localdb，两者之一
        // return:
        //      -1  error
        //      0   succeed
        public int dp2Kernel_createXml(
            string strSqlServerType,
            string strDataDir,
            out string strError)
        {
            strError = "";

            string strFilename = Path.Combine(strDataDir, "databases.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + " 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode nodeDatasource = dom.DocumentElement.SelectSingleNode("datasource");
            if (nodeDatasource == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<datasource>元素不存在。";
                return -1;
            }

            if (strSqlServerType == "sqlite")
            {
                DomUtil.SetAttr(nodeDatasource, "mode", null);

                /*
                 * 
        <datasource userid="" password="7E/u3+nbJxg=" servername="~sqlite" servertype="SQLite" />             * 
                 * */

                DomUtil.SetAttr(nodeDatasource,
                    "servertype",
                    "SQLite");
                DomUtil.SetAttr(nodeDatasource,
                    "servername",
                    "~sqlite");
                DomUtil.SetAttr(nodeDatasource,
                     "userid",
                     "");
            }
            else if (strSqlServerType == "localdb")
            {
                int nRet = CreateLocalDBInstance("dp2libraryxe",
            out strError);
                if (nRet == -1)
                {
                    strError = "创建 SQL LocalDB 的实例 'dp2libraryxe' 时出错: " + strError + "\r\n\r\n因此无法创建 databases.xml 配置文件";
                    return -1;
                }

                DomUtil.SetAttr(nodeDatasource, "mode", "SSPI");

                /*
                 * 
        <datasource userid="" password="7E/u3+nbJxg=" servername="~sqlite" servertype="SQLite" />             * 
                 * */

                DomUtil.SetAttr(nodeDatasource,
                    "servertype",
                    "MS SQL Server");
                DomUtil.SetAttr(nodeDatasource,
                    "servername",
                    "(LocalDB)\\dp2libraryxe"); // 缺省为 MSSQLLocalDB 或 v11.0
                DomUtil.SetAttr(nodeDatasource,
                     "userid",
                     null);
                DomUtil.SetAttr(nodeDatasource,
                     "password",
                     null);
            }
            else
            {
                strError = "未知的 SQL 服务器类型 '" + strSqlServerType + "'";
                return -1;
            }
#if NO
            string strPassword = Cryptography.Encrypt(this.DatabaseLoginPassword, "dp2003");
            DomUtil.SetAttr(nodeDatasource,
                "password",
                strPassword);
#endif

            XmlNode nodeDbs = dom.DocumentElement.SelectSingleNode("dbs");
            if (nodeDbs == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<dbs>元素不存在。";
                return -1;
            }
            DomUtil.SetAttr(nodeDbs,
                 "instancename",
                 "");

            dom.Save(strFilename);
            return 0;
        }

        // 修改root用户记录文件
        // parameters:
        //      strUserName 如果为null，表示不修改用户名
        //      strPassword 如果为null，表示不修改密码
        //      strRights   如果为null，表示不修改权限
        static int dp2Kernel_ModifyRootUser(string strDataDir,
            string strUserName,
            string strPassword,
            string strRights,
            out string strError)
        {
            strError = "";

            if (strUserName == null
                && strPassword == null
                && strRights == null)
                return 0;

            string strFileName = PathUtil.MergePath(strDataDir, "userdb\\0000000001.xml");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载root用户记录文件 " + strFileName + " 到DOM时发生错误: " + ex.Message;
                return -1;
            }

            string strOldUserName = "";
            if (strUserName != null)
            {
                strOldUserName = DomUtil.GetElementText(dom.DocumentElement,
                    "name");
                DomUtil.SetElementText(dom.DocumentElement,
                    "name",
                    strUserName);
            }

            if (strPassword != null)
            {
                DomUtil.SetElementText(dom.DocumentElement, "password",
                    Cryptography.GetSHA1(strPassword));
            }

            if (strRights != null)
            {
                XmlNode nodeServer = dom.DocumentElement.SelectSingleNode("server");
                if (nodeServer == null)
                {
                    Debug.Assert(false, "不可能的情况");
                    strError = "root用户记录文件 " + strFileName + " 格式错误: 根元素下没有<server>元素";
                    return -1;
                }

                DomUtil.SetAttr(nodeServer, "rights", strRights);
            }

            dom.Save(strFileName);

            // 修改keys_name.xml文件
            if (strUserName != null
                && strUserName != strOldUserName)
            {
                strFileName = PathUtil.MergePath(strDataDir, "userdb\\keys_name.xml");

                dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (Exception ex)
                {
                    strError = "装载用户keys文件 " + strFileName + " 到DOM时发生错误: " + ex.Message;
                    return -1;
                }

                XmlNode node = dom.DocumentElement.SelectSingleNode("key/keystring[text()='" + strOldUserName + "']");
                if (node == null)
                {
                    strError = "更新用户keys文件时出错：" + "根下 key/keystring 文本值为 '" + strOldUserName + "' 的元素没有找到";
                    return -1;
                }
                node.InnerText = strUserName;
                dom.Save(strFileName);
            }

            return 0;
        }

        #endregion

        #region dp2Library


        LibraryHost library_host = null;

        // parameters:
        //      bAutoStart  是否自动启动。如果为 false，表示会重新启动，假如以前启动过的话
        int dp2Library_start(
            bool bAutoStart,
            out string strError)
        {
            strError = "";

            Debug.Assert(string.IsNullOrEmpty(this.LibraryDataDir) == false, "");

            string strFilename = Path.Combine(this.LibraryDataDir, "library.xml");
            if (File.Exists(strFilename) == false)
            {
                strError = "dp2Library XE 尚未初始化";
                return 0;
            }

            if (bAutoStart == true && library_host != null)
            {
                strError = "dp2Library 先前已经启动了";
                return 0;
            }

            dp2Library_stop();

            // 获得 监听 URL
#if NO
            if (this.IsServer == true)
            {
                string strUrl = this.AppInfo.GetString("main_form", "listening_url", "");
                if (string.IsNullOrEmpty(strUrl) == true)
                {
                    string strNewUrl = InputDlg.GetInput(
    this,
    "请指定服务器绑定的 URL",
    "URL: ",
    "http://localhost:8001/dp2library/xe",
    this.Font);
                    if (strNewUrl == null)
                    {
                        strNewUrl = "http://localhost:8001/dp2library/xe";
                        MessageBox.Show(this, "自动使用缺省的 URL " + strNewUrl);
                    }

                    this.AppInfo.SetString("main_form", "listening_url", strNewUrl);

                    this.LibraryServerUrl = strNewUrl;
                }
                else
                    this.LibraryServerUrl = strUrl;
            }
            else
            {
                this.LibraryServerUrl = "net.pipe://localhost/dp2library/xe";
            }
#endif

            // 检查监听 URL
            if (this.IsServer == true)
            {
                this.LibraryServerUrlList = this.AppInfo.GetString("main_form", "listening_url", "");
                if (string.IsNullOrEmpty(this.LibraryServerUrlList) == true)
                {
                    strError = "尚未正确配置监听URL， dp2library server 无法启动";
                    return -1;
                }

                // TODO: 必须是 http net.tcp 协议之一
            }
            else
            {
                // 强制设置为固定值
                this.LibraryServerUrlList = LibraryHost.default_single_url; //  "net.pipe://localhost/dp2library/xe";
            }

#if NO
            if (this.IsServer == true)
                this.LibraryServerUrl = "http://localhost/dp2library/xe";
            else
                this.LibraryServerUrl = "net.pipe://localhost/dp2library/xe";
#endif

            library_host = new LibraryHost();
            library_host.DataDir = this.LibraryDataDir;
            library_host.HostUrl = this.LibraryServerUrlList;
            int nRet = library_host.Start(out strError);
            if (nRet == -1)
                return -1;

            if (this.library_host != null)
            {
                // this.library_host.SetTestMode(this.TestMode);
                if (this.TestMode == true)
                    this.library_host.SetMaxClients(5); // 社区版限定 5 个前端
                else
                    this.library_host.SetMaxClients(this.MaxClients);
                this.library_host.SetLicenseType(this.LicenseType);
                this.library_host.SetFunction(this.Function);
            }

            return 1;
        }

        void dp2Library_stop()
        {
            if (library_host != null)
            {
                library_host.Stop();
                library_host = null;
            }
        }

        // 安装 dp2Library 的数据目录
        // parameters:
        //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
        int SetupLibraryDataDir(
            bool bAutoSetup,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // TODO: 是否改为探测目录是否存在?

            string strFilename = PathUtil.MergePath(this.LibraryDataDir, "library.xml");
            if (File.Exists(strFilename) == true)
            {
                if (bAutoSetup == true)
                {
                    strError = "dp2Library 数据目录先前已经安装过，本次没有重新安装";
                    return 0;
                }

                DialogResult result = MessageBox.Show(this,
    "警告：dp2Library 数据目录先前已经安装过了，本次重新安装，将摧毁以前的全部数据。\r\n\r\n确实要重新安装？",
    "dp2Library XE",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strError = "放弃重新安装 dp2Library 数据目录";
                    return 0;
                }
            }

            // 删除 dp2kernel 中的全部数据库

            dp2Library_stop();
            dp2Kernel_stop();

            // 删除以前的目录
            nRet = DeleteDataDirectory(
                Path.GetDirectoryName(strFilename),
                out strError);
            if (nRet == -1)
                return -1;

            // 清空残留的前端密码，避免后面登录时候的困惑
            AppInfo.SetString(
    "default_account",
    "password",
    "");

            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "正在初始化 dp2Library 数据目录 ...";

            nRet = dp2Library_CreateNewDataDir(out strError);
            if (nRet == -1)
                return -1;

            // 创建/修改 library.xml 文件
            // return:
            //      -1  error
            //      0   succeed
            nRet = dp2Library_createXml(this.LibraryDataDir,
                "supervisor",
                "",
                null,
                "本地图书馆",
                out strError);
            if (nRet == -1)
                return -1;

            // TODO: 每次升级安装后，需要覆盖 templates 目录和 cfgs 目录

            nRet = dp2Kernel_start(true,
                out strError);
            if (nRet == -1)
                return -1;
            nRet = dp2Library_start(true,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            if (_messageBar != null)
                _messageBar.MessageText = "正在创建基本数据库，可能需要几分钟时间 ...";
#endif
            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "正在创建基本数据库，可能需要几分钟时间 ...";

            {
                string strErrorText = "";
                Task task = Task.Factory.StartNew(() =>
                {
                    string strError1 = "";
                    // 创建默认的几个数据库
                    nRet = CreateDefaultDatabases(out strError1);
                    if (nRet == -1)
                    {
                        strErrorText = "创建数据库时出错: " + strError1;
                    }
                },
CancellationToken.None,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
                while (task.Wait(100, _cancel.Token) == false)
                {
                    Application.DoEvents();
                    if (_cancel.Token.IsCancellationRequested)
                        break;
                }
                if (nRet == -1)
                {
                    strError = strErrorText;
                    return -1;
                }
            }


            return 1;
        }

        // 创建缺省的几个数据库
        // TODO: 把过程显示在控制台
        // return:
        //      -1  出错
        //      0   成功
        int CreateDefaultDatabases(out string strError)
        {
            strError = "";

            int nRet = PrepareSearch();

            EnableControls(false);

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在创建数据库 ...");
            Stop.BeginLoop();

            try
            {
                return ManageHelper.CreateDefaultDatabases(Channel, Stop, null, out strError);
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                EnableControls(true);

                EndSearch();
            }
        }

#if NO
        // 创建缺省的几个数据库
        // TODO: 把过程显示在控制台
        // return:
        //      -1  出错
        //      0   成功
        int CreateDefaultDatabases(out string strError)
        {
            strError = "";

            // 创建书目库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            List<string> biblio_dbnames = new List<string>();

            // 创建书目库
            {
                // parameters:
                //      strUsage    book/series
                //      strSyntax   unimarc/usmarc
                CreateBiblioDatabaseNode(database_dom,
                    "中文图书",
                    "book",
                    "orderRecommendStore",
                    "unimarc",
                    true);
                biblio_dbnames.Add("中文图书");

                CreateBiblioDatabaseNode(database_dom,
    "中文期刊",
    "series",
    "",
    "unimarc",
    true);
                biblio_dbnames.Add("中文期刊");

                CreateBiblioDatabaseNode(database_dom,
    "西文图书",
    "book",
    "",
    "usmarc",
    true);
                biblio_dbnames.Add("西文图书");

                CreateBiblioDatabaseNode(database_dom,
    "西文期刊",
    "series",
    "",
    "usmarc",
    true);
                biblio_dbnames.Add("西文期刊");

            }

            // 创建读者库
            CreateReaderDatabaseNode(database_dom,
                "读者",
                "",
                true);

            // 预约到书
            CreateSimpleDatabaseNode(database_dom,
    "预约到书",
    "arrived");

            // 违约金
            CreateSimpleDatabaseNode(database_dom,
                "违约金",
                "amerce");

            // 出版者
            CreateSimpleDatabaseNode(database_dom,
    "出版者",
    "publisher");

            // 消息
            CreateSimpleDatabaseNode(database_dom,
    "消息",
    "message");


            // 创建 OPAC 数据库的定义
            XmlDocument opac_dom = new XmlDocument();
            opac_dom.LoadXml("<virtualDatabases />");

            foreach (string dbname in biblio_dbnames)
            {
                XmlElement node = opac_dom.CreateElement("database");
                opac_dom.DocumentElement.AppendChild(node);
                node.SetAttribute("name", dbname);
            }

            // 浏览格式
            // 插入格式节点
            XmlDocument browse_dom = new XmlDocument();
            browse_dom.LoadXml("<browseformats />");

            foreach (string dbname in biblio_dbnames)
            {
                XmlElement database = browse_dom.CreateElement("database");
                browse_dom.DocumentElement.AppendChild(database);
                database.SetAttribute("name", dbname);

                XmlElement format = browse_dom.CreateElement("format");
                database.AppendChild(format);
                format.SetAttribute("name", "详细");
                format.SetAttribute("type", "biblio");
                format.InnerXml = "<caption lang=\"zh-CN\">详细</caption><caption lang=\"en\">Detail</caption>";
            }

            int nRet = PrepareSearch();
            try
            {
                EnableControls(false);

                Stop.OnStop += new StopEventHandler(this.DoStop);
                Stop.Initial("正在创建数据库 ...");
                Stop.BeginLoop();

                try
                {
                    string strOutputInfo = "";
                    long lRet = Channel.ManageDatabase(
                        Stop,
                        "create",
                        "",
                        database_dom.OuterXml,
                        out strOutputInfo,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    lRet = Channel.SetSystemParameter(
    Stop,
    "opac",
    "databases",
    opac_dom.DocumentElement.InnerXml,
    out strError);
                    if (lRet == -1)
                        return -1;

                    lRet = Channel.SetSystemParameter(
    Stop,
    "opac",
    "browseformats",
    browse_dom.DocumentElement.InnerXml,
    out strError);
                    if (lRet == -1)
                        return -1;

                    return 0;
                }
                finally
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");

                    EnableControls(true);
                }
            }
            finally
            {
                EndSearch();
            }
        }

#endif

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        /// <summary>
        /// 通讯通道。MainForm 自己使用
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 准备进行检索
        /// </summary>
        /// <returns>0: 没有成功; 1: 成功</returns>
        public int PrepareSearch()
        {
            if (String.IsNullOrEmpty(this.LibraryServerUrlList) == true)
                return 0;

            if (this.Channel == null)
                this.Channel = new LibraryChannel();

            this.Channel.Url = GetFirstUrl(this.LibraryServerUrlList);

            this.Channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);

            Stop = new DigitalPlatform.Stop();
            Stop.Register(stopManager, true);	// 和容器关联

            return 1;
        }

        static string GetFirstUrl(string strUrlList)
        {
            List<string> urls = StringUtil.SplitList(strUrlList, ';');
            if (urls.Count == 0)
                return "";

            return urls[0];
        }

        /// <summary>
        /// 结束检索
        /// </summary>
        /// <returns>返回 0</returns>
        public int EndSearch()
        {
            if (Stop != null) // 脱离关联
            {
                Stop.Unregister();	// 和容器关联
                Stop = null;
            }

            this.Channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.Close();
            this.Channel = null;

            return 0;
        }

        internal void Channel_BeforeLogin(object sender,
    DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = this.SupervisorUserName;   //  "supervisor";

                e.Password = this.DecryptPasssword(AppInfo.GetString(
"default_account",
"password",
""));

                string strLocation = "manager";
                e.Parameters = "location=" + strLocation;

                e.Parameters += ",client=dp2libraryxe|" + Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

#if NO
            e.Cancel = true;
            e.ErrorInfo = "管理帐户无效"; 
#endif
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

#if NO
            if (dlg.IsReader == true)
                e.Parameters += ",type=reader";

            // 2014/9/13
            e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

            // 从序列号中获得 expire= 参数值
            {
                string strExpire = GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
            }

            // 2014/10/23
            if (this.TestMode == true)
                e.Parameters += ",testmode=true";
#endif
            e.Parameters += ",client=dp2libraryxe|" + Program.ClientVersion;

            e.SavePasswordLong = dlg.SavePasswordLong;
            if (e.LibraryServerUrl != dlg.ServerUrl)
            {
                e.LibraryServerUrl = dlg.ServerUrl;
                // _expireVersionChecked = false;
            }
        }

        public string SupervisorUserName
        {
            get
            {
                if (this.AppInfo == null)
                    return "supervisor";
                return AppInfo.GetString(
    "default_account",
    "username",
    "supervisor");
            }
            set
            {
                AppInfo.SetString(
    "default_account",
    "username",
    value);
            }
        }

        // parameters:
        //      bLogin  是否在对话框后立即登录？如果为false，表示只是设置缺省帐户，并不直接登录
        CirculationLoginDlg SetDefaultAccount(
            string strServerUrl,
            string strTitle,
            string strComment,
            LoginFailCondition fail_contidion,
            IWin32Window owner)
        {
            CirculationLoginDlg dlg = new CirculationLoginDlg();
            MainForm.SetControlFont(dlg, this.Font);

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
                dlg.ServerUrl = GetFirstUrl(this.LibraryServerUrlList);
            }
            else
            {
                dlg.ServerUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;

            if (String.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;
#if NO
            if (bLogin == false)
                dlg.SetDefaultMode = true;
#endif

            dlg.SupervisorMode = true;

            dlg.Comment = strComment;
            dlg.UserName = AppInfo.GetString(
                "default_account",
                "username",
                "supervisor");

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

            dlg.IsReader = false;
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

            AppInfo.SetString(
                "default_account",
                "location",
                dlg.OperLocation);

#if NO
            AppInfo.SetString(
                "config",
                "circulation_server_url",
                dlg.ServerUrl);
#endif
            return dlg;
        }

        string EncryptKey = "dp2libraryxe_client_password_key";

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
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.BeginInvoke(new Action(() =>
            {
                this.menuStrip1.Enabled = bEnable;
            }));
        }



        // 创建数据目录，并复制进基本内容
        int dp2Library_CreateNewDataDir(
            out string strError)
        {
            strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "library_data.zip");

            // 要求在 library_data.zip 内准备要安装的数据文件(初次安装而不是升级安装)
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        try
                        {
                            e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                        catch (Exception ex)
                        {
                            strError = ExceptionUtil.GetAutoText(ex);
                            return -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
#if NO
            int nRet = PathUtil.CopyDirectory(strTempDataDir,
    this.LibraryDataDir,
    true,
    out strError);
            if (nRet == -1)
            {
                strError = "拷贝临时目录 '" + strTempDataDir + "' 到数据目录 '" + this.LibraryDataDir + "' 时发生错误：" + strError;
                return -1;
            }
#endif

            return 0;
        }

        // 备份一个文件
        // 顺次备份为 ._1 ._2 ...
        static void BackupFile(string strFullPath)
        {
            for (int i = 0; ; i++)
            {
                string strBackupFilePath = strFullPath + "._" + (i + 1).ToString();
                if (File.Exists(strBackupFilePath) == false)
                {
                    File.Copy(strFullPath, strBackupFilePath);
                    return;
                }
            }
        }

        // 从 library_data.zip 中展开部分目录内容
        int dp2Library_extractPartDir(
            out string strError)
        {
            strError = "";

            string strCfgsDir = Path.Combine(this.UserDir, "library_data/cfgs");
            string strTemplatesDir = Path.Combine(this.UserDir, "library_data/templates");

            string strZipFileName = Path.Combine(this.DataDir, "library_data.zip");

            // 要求在 library_data.zip 内准备要安装的数据文件(初次安装而不是升级安装)
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        string strFullPath = Path.Combine(this.UserDir, e.FileName);


                        // 测试strPath1是否为strPath2的下级目录或文件
                        //	strPath1正好等于strPath2的情况也返回true
                        if (PathUtil.IsChildOrEqual(strFullPath, strTemplatesDir) == true)
                        {
                            e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                        else if (PathUtil.IsChildOrEqual(strFullPath, strCfgsDir) == true)
                        {
                            // 观察文件版本
                            if (File.Exists(strFullPath) == true)
                            {
                                string strTimestamp = "";
                                int nRet = _versionManager.GetFileVersion(strFullPath, out strTimestamp);
                                if (nRet == 1)
                                {
                                    // .zip 中的对应文件的时间戳
                                    string strZipTimesamp = e.LastModified.ToString();
                                    if (strZipTimesamp == strTimestamp)
                                        continue;

                                    // 看看当前物理文件是否已经是修改过
                                    string strPhysicalTimestamp = File.GetLastWriteTime(strFullPath).ToString();
                                    if (strPhysicalTimestamp != strTimestamp)
                                    {
                                        // 需要备份
                                        BackupFile(strFullPath);
                                    }


                                }
                            }


                            e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                            if ((e.Attributes & FileAttributes.Directory) == 0)
                            {
                                if (e.LastModified != File.GetLastWriteTime(strFullPath))
                                {
                                    /*
#if LOG
                                    string strText = "文件 " + strFullPath + " 的最后修改时间为 '" + File.GetLastWriteTime(strFullPath).ToString() + "'，不是期望的 '" + e.LastModified.ToString() + "' ";
                                    WriteLibraryEventLog(strText, EventLogEntryType.Information);
#endif
                                     * */
                                    // 时间有可能不一致，可能是夏令时之类的问题
                                    File.SetLastWriteTime(strFullPath, e.LastModified);
                                }
                                Debug.Assert(e.LastModified == File.GetLastWriteTime(strFullPath));
                                _versionManager.SetFileVersion(strFullPath, e.LastModified.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            _versionManager.AutoSave();
            return 0;
        }

        public string GetLibraryXmlUid()
        {
            if (string.IsNullOrEmpty(this.LibraryDataDir) == true)
                return null;

            string strFilename = PathUtil.MergePath(this.LibraryDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch
            {
                return null;
            }

            return LibraryServerUtil.GetLibraryXmlUid(dom);
        }

        // 创建/修改 library.xml 文件
        // return:
        //      -1  error
        //      0   succeed
        public int dp2Library_createXml(string strDataDir,
            string strSupervisorUserName,
            string strSupervisorPassword,
            string strSupervisorRights,
            string strLibraryName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + " 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode nodeRmsServer = dom.DocumentElement.SelectSingleNode("rmsserver");
            if (nodeRmsServer == null)
            {
                nodeRmsServer = dom.CreateElement("rmsserver");
                dom.DocumentElement.AppendChild(nodeRmsServer);
            }

            DomUtil.SetAttr(nodeRmsServer,
                "url",
                "net.pipe://localhost/dp2kernel/XE"
                );
            DomUtil.SetAttr(nodeRmsServer,
                 "username",
                 "root");

            string strPassword = Cryptography.Encrypt("", "dp2circulationpassword");
            DomUtil.SetAttr(nodeRmsServer,
                "password",
                strPassword);

            // 
            XmlNode nodeAccounts = dom.DocumentElement.SelectSingleNode("accounts");
            if (nodeAccounts == null)
            {
                nodeAccounts = dom.CreateElement("accounts");
                dom.DocumentElement.AppendChild(nodeAccounts);
            }
            XmlElement nodeSupervisor = nodeAccounts.SelectSingleNode("account[@type='']") as XmlElement;
            if (nodeSupervisor == null)
            {
                nodeSupervisor = dom.CreateElement("account");
                nodeAccounts.AppendChild(nodeSupervisor);
            }

            if (strSupervisorUserName != null)
                DomUtil.SetAttr(nodeSupervisor, "name", strSupervisorUserName);
            if (strSupervisorPassword != null)
            {
#if NO
                DomUtil.SetAttr(nodeSupervisor, "password",
                    Cryptography.Encrypt(strSupervisorPassword, "dp2circulationpassword")
                    );
#endif

                double version = LibraryServerUtil.GetLibraryXmlVersion(dom);

                if (version <= 2.0)
                {
                    nodeSupervisor.SetAttribute("password",
                        Cryptography.Encrypt(strSupervisorPassword, "dp2circulationpassword")
                        );
                }
                else
                {
                    // 新的密码存储策略
                    string strHashed = "";
                    nRet = LibraryServerUtil.SetUserPassword(strSupervisorPassword, out strHashed, out strError);
                    if (nRet == -1)
                    {
                        strError = "SetUserPassword() error: " + strError;
                        return -1;
                    }
                    nodeSupervisor.SetAttribute("password", strHashed);
                }
            }
            if (strSupervisorRights != null)
                DomUtil.SetAttr(nodeSupervisor, "rights", strSupervisorRights);

            if (strLibraryName != null)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                        "libraryInfo/libraryName",
                        strLibraryName);
            }

            dom.Save(strFilename);
            return 0;
        }

        // 修改 library.xml 文件，创建或修改用户帐户
        // parameters:
        //      strStyle    写入风格。 merge 表示需要和以前的权限合并
        // return:
        //      -1  error
        //      0   succeed
        public int dp2Library_changeXml_addAccount(string strDataDir,
            string strUserName,
            string strType,
            string strRights,
            string strStyle,
            out string strError)
        {
            strError = "";

            string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + " 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlElement accounts_root = dom.DocumentElement.SelectSingleNode("accounts") as XmlElement;
            if (accounts_root == null)
            {
                accounts_root = dom.CreateElement("accounts");
                dom.DocumentElement.AppendChild(accounts_root);
            }

            XmlElement account = accounts_root.SelectSingleNode("account[@name='" + strUserName + "']") as XmlElement;
            if (account == null)
            {
                account = dom.CreateElement("account");
                accounts_root.AppendChild(account);
            }

            account.SetAttribute("name", strUserName);
            if (string.IsNullOrEmpty(strType) == false)
                account.SetAttribute("type", strType);
            else
                account.RemoveAttribute("type");

            string strOldRights = account.GetAttribute("rights");
            if (StringUtil.IsInList("merge", strStyle) == true)
            {
                List<string> old_rights = StringUtil.SplitList(strOldRights, ',');
                List<string> new_rights = StringUtil.SplitList(strRights, ',');
                new_rights.AddRange(old_rights);
                StringUtil.RemoveDupNoSort(ref new_rights);
                strRights = StringUtil.MakePathList(new_rights);
            }

            account.SetAttribute("rights", strRights);

            dom.Save(strFilename);
            return 0;
        }

        #endregion

        private void button_setupLibraryDataDir_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 安装 dp2Library 的数据目录
            // parameters:
            //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
            int nRet = SetupLibraryDataDir(
                false,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            MessageBox.Show(this, "dp2Library 数据目录安装成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuItem_setupKernelDataDir_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 安装 dp2kernel 的数据目录
            // parameters:
            //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
            int nRet = SetupKernelDataDir(
                false,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            MessageBox.Show(this, "dp2Kernel 数据目录安装成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_setupLibraryDataDir_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 安装 dp2Library 的数据目录
            // parameters:
            //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
            int nRet = SetupLibraryDataDir(
                false,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            MessageBox.Show(this, "dp2Library 数据目录安装成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_openLibraryWsdl_Click(object sender, EventArgs e)
        {
            if (library_host == null)
            {
                MessageBox.Show(this, "library_host 尚未安装或启动");
                return;
            }
            Process.Start("IExplore.exe", library_host.MetadataUrl);
        }

        private void MenuItem_openKernelWsdl_Click(object sender, EventArgs e)
        {
            if (kernel_host == null)
            {
                MessageBox.Show(this, "kernel_host 尚未安装或启动");
                return;
            }
            Process.Start("IExplore.exe", kernel_host.MetadataUrl);
        }

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
                stopManager.DoStopAll(null);    // 2012/3/25
            else
                stopManager.DoStopActive();

        }

        private void ToolStripMenuItem_stopAll_Click(object sender, EventArgs e)
        {
            stopManager.DoStopAll(null);
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

        void CreateEventSource()
        {
            // 创建事件日志目录
            if (!EventLog.SourceExists("dp2Library"))
            {
                EventLog.CreateEventSource("dp2Library", "DigitalPlatform");
            }
            if (!EventLog.SourceExists("dp2Kernel"))
            {
                EventLog.CreateEventSource("dp2Kernel", "DigitalPlatform");
            }
        }

        void WriteLibraryEventLog(
            string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog();
            Log.Source = "dp2Library";
            Log.WriteEntry(strText, type);
        }

        void WriteKernelEventLog(
    string strText,
    EventLogEntryType type)
        {
            EventLog Log = new EventLog();
            Log.Source = "dp2Kernel";
            Log.WriteEntry(strText, type);
        }

        // 是否为小型服务器版本
        bool IsServer
        {
            get
            {
                string strMode = this.AppInfo.GetString("main_form", "last_mode", "test");  // "standard"
                if (strMode == "miniServer" || strMode == "miniTest")    // "miniServer"
                    return true;
                return false;
                // return this.AppInfo.GetBoolean("product", "isServer", false);
            }
            /*
            set
            {
                this.AppInfo.SetBoolean("product", "isServer", value);
            }
             * */
        }

        /* 和以前兼容的 Mode 含义
test	-- community single
miniTest	-- community mini 这是新增的
standard	-- enterprise single
miniServer	-- enterprise mini
 * */
        // 正规化输入的模式字符串
        // community 模式是模糊的，可能指单机，也可能指小型服务器
        // 根据以前的模式定义，复原 community 类型的详细模式
        static string CannonicalizeInputMode(string strOldMode, string strNewMode)
        {
            if (strNewMode == "singleCommunity")
                return "test";

            if (strNewMode == "miniCommunity")
                return "miniTest";

            // 将模糊的变清晰
            if (strNewMode == "community")
            {
                if (strOldMode == "test" || strOldMode == "standard")
                    return "test";
                if (strOldMode == "miniTest" || strOldMode == "miniServer")
                    return "miniTest";
            }

            return strNewMode;
        }

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

            // 修改前的模式
            string strOldMode = this.AppInfo.GetString("main_form", "last_mode", "test");   // "standard"

            string strSerialCode = "";
        REDO_VERIFY:

            //string strLocalString = GetEnvironmentString(this.IsServer);

            //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");

            if (
                // MatchLocalString(this.IsServer, strSerialCode) == false

                (MatchLocalString(this.IsServer, strSerialCode) == false
                && MatchLocalString(!this.IsServer, strSerialCode) == false)

                || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (String.IsNullOrEmpty(strSerialCode) == false)
                {
                    MessageBox.Show(this, "序列号无效。请重新输入");
                }

                // 出现设置序列号对话框
                nRet = ResetSerialCode(
                    true,
                    strSerialCode,
                    GetEnvironmentString(this.IsServer, strFirstMac));
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

                string strMode = CannonicalizeInputMode(strOldMode, strSerialCode);

                if (strMode != strSerialCode)
                    this.AppInfo.SetString("sn", "sn", strMode);

#if NO
                Debug.Assert(strMode == "test"
                    || strMode == "miniTest"
                    || strMode == "standard"
                    || strMode == "miniServer", "");
#endif

                if (strMode == "test" || strMode == "miniTest")
                {
                    this.TestMode = true;
                    this.AppInfo.SetString("main_form", "last_mode", strMode);
                    this.AppInfo.Save();
                    // 
                    SetListenUrl(strMode);
                    SetTitle();

                    goto RESTART;
                }
                else
                    this.TestMode = false;

                // 如果小型服务器/单机方式发生了改变
                if (MatchLocalString(!this.IsServer, strSerialCode) == true)
                {
                    if (this.IsServer == true)
                    {
                        // 反转为 单机版
                        if (this.TestMode == true)
                            this.AppInfo.SetString("main_form", "last_mode", "test");
                        else
                            this.AppInfo.SetString("main_form", "last_mode", "standard");   // "standard"
                    }
                    else
                    {
                        // 反转为小型服务器
                        if (this.TestMode == true)
                            this.AppInfo.SetString("main_form", "last_mode", "miniTest"); // "miniServer"
                        else
                            this.AppInfo.SetString("main_form", "last_mode", "miniServer"); // "miniServer"
                        this.TestMode = false;
                    }

                    // 设置监听 URL
                    SetListenUrl(this.AppInfo.GetString("main_form", "last_mode", "test")); // "standard"

                    SetTitle();
                }

                // 解析 product 参数，重新设置授权模式
                {
                    Hashtable table = StringUtil.ParseParameters(GetEnvironmentString(this.IsServer, ""));
                    string strProduct = (string)table["product"];

                    if (strProduct == "dp2libraryXE server")
                    {
                        if (this.TestMode == true)
                            this.AppInfo.SetString("main_form", "last_mode", "miniTest"); // "miniServer"
                        else
                            this.AppInfo.SetString("main_form", "last_mode", "miniServer"); // "miniServer"
                    }
                    else
                    {
                        if (this.TestMode == true)
                            this.AppInfo.SetString("main_form", "last_mode", "test");
                        else
                            this.AppInfo.SetString("main_form", "last_mode", "standard");   // "standard"
                    }
                }
                this.AppInfo.Save();

                goto REDO_VERIFY;
            }


        RESTART:
            // 修改后的模式
            //string strNewMode = this.AppInfo.GetString("main_form", "last_mode", "standard");
            //if (strOldMode != strNewMode)
            {
                GetMaxClients();
                GetLicenseType();
                GetFunction();
                // 重新启动
                RestartDp2libraryIfNeed();
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
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
            for (int i = 0; i < tool.Items.Count; i++)
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

        private void MenuItem_autoStartDp2Circulation_Click(object sender, EventArgs e)
        {
            if (this.AutoStartDp2circulation == false)
            {
                this.AutoStartDp2circulation = true;
            }
            else
            {
                this.AutoStartDp2circulation = false;
            }
        }

        private void MenuItem_copyright_Click(object sender, EventArgs e)
        {
            throw new Exception("test exception");

            MessageBox.Show(this, "dp2Library XE\r\ndp2 图书馆集成系统 图书馆应用服务器 单机版/小型版\r\n\r\n(C)2006-2015 版权所有 数字平台(北京)软件有限责任公司\r\n"
                + "2015 年以 Apache License Version 2.0 方式开源\r\n"
                + "http://github.com/digitalplatform/dp2\r\n");
        }

        private void MenuItem_setListeningUrl_Click(object sender, EventArgs e)
        {
            if (this.IsServer == true)
            {
                string strUrl = this.AppInfo.GetString("main_form", "listening_url", "");
                if (string.IsNullOrEmpty(strUrl) == true)
                    strUrl = LibraryHost.default_miniserver_urls;   //  "http://localhost:8001/dp2library/xe;net.pipe://localhost/dp2library/xe";

                string strNewUrl = InputDlg.GetInput(
this,
"请指定服务器绑定的 URL",
"URL: (多个 URL 之间可以用分号间隔)",
strUrl,
this.Font);
                if (strNewUrl == null)
                {
                    MessageBox.Show(this, "放弃修改");
                    return;
                }

                this.AppInfo.SetString("main_form", "listening_url", strNewUrl);
                this.LibraryServerUrlList = strNewUrl;

                // 重新启动
                RestartDp2libraryIfNeed();
            }
            else
            {
                MessageBox.Show(this, "单机版监听 URL 为 " + this.LibraryServerUrlList + "， 不可修改");
            }

        }

        // 如果必要，重新启动 dp2library
        void RestartDp2libraryIfNeed()
        {
            dp2Library_stop();

            // 重新启动
            if (library_host == null)
            {
                string strError = "";
                int nRet = dp2Library_start(
                    false,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "重新启动 dp2Library 时出错：" + strError);
                    return;
                }
            }
        }

        // 如果必要，重新启动 dp2kernel
        void RestartDp2kernelIfNeed()
        {
            dp2Kernel_stop();

            // 重新启动
            if (kernel_host == null)
            {
                string strError = "";
                int nRet = dp2Kernel_start(
                    false,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "重新启动 dp2Kernel 时出错：" + strError);
                    return;
                }
            }
        }

        // 从安装包更新数据目录中的配置文件
        private void MenuItem_updateDataDir_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 从 library_data.zip 中展开部分目录内容
            int nRet = dp2Library_extractPartDir(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        FileVersionManager _versionManager = new FileVersionManager();

        // 更新 library_data 中的 cfgs 子目录 和 templates 子目录
        void UpdateCfgs()
        {
            string strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "library_data.zip");

            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(Path.GetFileName(strZipFileName), out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (strOldTimestamp != strNewTimestamp)
            {
                // 从 library_data.zip 中展开部分目录内容
                nRet = dp2Library_extractPartDir(out strError);
                if (nRet == -1)
                    goto ERROR1;
                _versionManager.SetFileVersion(Path.GetFileName(strZipFileName), strNewTimestamp);
                _versionManager.AutoSave();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


#if NO
        // 记忆配置文件的版本
        void SaveFileInfo()
        {
            string strCfgsDir = Path.Combine(this.UserDir, "library_data/cfgs");

            List<string> filenames = GetFileNames(strCfgsDir);

            foreach (string filename in filenames)
            {
                _versionManager.SetFileVersion(filename, File.GetLastWriteTime(filename).ToString());
            }

            _versionManager.AutoSave();
        }
#endif

        #region dp2OPAC

        // 安装 dp2OPAC 的数据目录
        // parameters:
        //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
        // return:
        //      -1  出错
        //      0   放弃安装
        //      1   安装成功
        int SetupOpacDataAndAppDir(
            bool bAutoSetup,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            CloseIIsExpress(true);

            // TODO: 是否改为探测目录是否存在?

            string strFilename = PathUtil.MergePath(this.OpacDataDir, "opac.xml");
            if (File.Exists(strFilename) == true)
            {
                if (bAutoSetup == true)
                {
                    strError = "dp2OPAC 数据目录先前已经安装过，本次没有重新安装";
                    return 0;
                }

                DialogResult result = MessageBox.Show(this,
    "警告：dp2OPAC 数据目录先前已经安装过了，本次重新安装，将摧毁以前的全部数据。\r\n\r\n确实要重新安装？",
    "dp2Library XE",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strError = "放弃重新安装 dp2OPAC 数据目录";
                    return 0;
                }
            }

            // TODO: 停止 IIS Express

            // 删除以前的目录
            nRet = DeleteDataDirectory(
                Path.GetDirectoryName(strFilename),
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            if (_messageBar != null)
                _messageBar.MessageText = "正在初始化 dp2OPAC 数据目录和应用程序目录 ...";
#endif
            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "正在初始化 dp2OPAC 数据目录和应用程序目录 ...";


            nRet = dp2OPAC_CreateNewDataDir(out strError);
            if (nRet == -1)
                return -1;

            // 2015/7/17 opac_style.zip 中的内容在首次安装后就要覆盖到数据目录。这样可以不在 opac_data.zip 文件中包含 style 子目录了
            // 更新 dp2OPAC 数据目录中的 style 子目录
            // parameters:
            //      bAuto   是否自动更新。true 表示(.zip 文件发生了变化)有必要才更新; false 表示无论如何均更新
            nRet = UpdateOpacStyles(false,
            out strError);
            if (nRet == -1)
                return -1;

            // 删除以前的目录
            nRet = DeleteDataDirectory(
                this.OpacAppDir,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = dp2OPAC_CreateNewAppDir(this.OpacDataDir, out strError);
            if (nRet == -1)
                return -1;

            // 修改 library.xml 文件，创建或修改用户帐户
            // return:
            //      -1  error
            //      0   succeed
            nRet = dp2Library_changeXml_addAccount(this.LibraryDataDir,
                "opac",
                null,
                default_opac_rights,
                "merge",
                out strError);
            if (nRet == -1)
                return -1;

            // 创建/修改 library.xml 文件
            // return:
            //      -1  error
            //      0   succeed
            nRet = dp2OPAC_createXml(this.OpacDataDir,
                "opac",
                "",
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // 比较两个时间戳的先后
        static int CompareTimestamp(string strTimestamp1, string strTimestamp2)
        {
            DateTime time1 = new DateTime(0);
            DateTime time2 = new DateTime(0);
            if (string.IsNullOrEmpty(strTimestamp1) == false)
            {
                if (DateTime.TryParse(strTimestamp1, out time1) == false)
                    goto BAD_TIMESTRING;
            }
            if (string.IsNullOrEmpty(strTimestamp2) == false)
            {
                if (DateTime.TryParse(strTimestamp2, out time2) == false)
                    goto BAD_TIMESTRING;
            }

            if (time1 == time2)
                return 0;
            if (time1 > time2)
                return 1;
            return -1;
        BAD_TIMESTRING:
            return string.Compare(strTimestamp1, strTimestamp2);
        }

        // 更新 dp2OPAC 数据目录中的 style 子目录
        // parameters:
        //      bAuto   是否自动更新。true 表示(.zip 文件发生了变化)有必要才更新; false 表示无论如何均更新
        int UpdateOpacStyles(
            bool bAuto,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strZipFileName = Path.Combine(this.DataDir, "opac_style.zip");

            string strTargetPath = Path.Combine(this.OpacDataDir, "style");
            if (Directory.Exists(strTargetPath) == true || bAuto == false)
            {
                nRet = dp2OPAC_extractDir(
                    bAuto,
                    strZipFileName,
                    strTargetPath,
                    true,   // 需要避开 .css.macro 文件的 .css 文件
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 2015/7/17
        // 从 opac_style.zip 中展开目录内容
        // parameters:
        //      bAuto   是否自动更新。如果为 true，表示根据以前的时间戳判断是否有必要更新。如果为 false，表示强制更新
        //      bDetectMacroFile    是否检测 .css.macro 文件。如果为 true，表示检测到同名的 .css.macro 文件后，.css 文件就不拷贝了
        int dp2OPAC_extractDir(
            bool bAuto,
            string strZipFileName,
            string strTargetDir,
            bool bDetectMacroFile,
            out string strError)
        {
            strError = "";

            // 记忆的时间戳，其入口事项和 zip 文件名以及目标目录路径均有关，这样当目标目录变化的时候，也能促使重新刷新
            string strEntry = Path.GetFileName(strZipFileName) + "|" + strTargetDir;

            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(strEntry, out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (bAuto == false || strOldTimestamp != strNewTimestamp)
            {
                try
                {
                    using (ZipFile zip = ZipFile.Read(strZipFileName))
                    {
                        for (int i = 0; i < zip.Count; i++)
                        {
                            ZipEntry e = zip[i];
                            // string strFullPath = Path.Combine(strTargetDir, e.FileName);
                            string strPart = GetSubPath(e.FileName);
                            string strFullPath = Path.Combine(strTargetDir, strPart);

                            e.FileName = strPart;

                            // 观察 .css 文件是否有同名的 .css.macro 文件，如果有则不复制了
                            if (bDetectMacroFile == true
                                && Path.GetExtension(strFullPath).ToLower() == ".css")
                            {
                                string strTempPath = strFullPath + ".macro";
                                if (File.Exists(strTempPath) == true)
                                    continue;
                            }

                            {
                                // 观察文件版本
                                if (File.Exists(strFullPath) == true)
                                {
                                    // .zip 中的对应文件的时间戳
                                    string strZipTimestamp = e.LastModified.ToString();

                                    // 版本管理器记载的时间戳
                                    string strTimestamp = "";
                                    nRet = _versionManager.GetFileVersion(strFullPath, out strTimestamp);
                                    if (nRet == 1)
                                    {
                                        // *** 记载过上次的版本

                                        if (strZipTimestamp == strTimestamp)
                                            continue;

                                        if ((e.Attributes & FileAttributes.Directory) == 0)
                                            AppendString("更新配置文件 " + strFullPath + "\r\n");

                                        // 覆盖前看看当前物理文件是否已经是修改过
                                        string strPhysicalTimestamp = File.GetLastWriteTime(strFullPath).ToString();
                                        if (strPhysicalTimestamp != strTimestamp)
                                        {
                                            // 需要备份
                                            BackupFile(strFullPath);
                                        }
                                    }
                                    else
                                    {
                                        // *** 没有记载过版本

                                        if ((e.Attributes & FileAttributes.Directory) == 0)
                                            AppendString("更新配置文件 " + strFullPath + "\r\n");

                                        // 覆盖前看看当前物理文件是否已经是修改过
                                        string strPhysicalTimestamp = File.GetLastWriteTime(strFullPath).ToString();
                                        if (strPhysicalTimestamp != strZipTimestamp)
                                        {
                                            // 需要备份
                                            BackupFile(strFullPath);
                                        }
                                    }
                                }
                                else
                                {
                                    if ((e.Attributes & FileAttributes.Directory) == 0)
                                        AppendString("创建配置文件 " + strFullPath + "\r\n");
                                }

                                if ((e.Attributes & FileAttributes.Directory) == 0)
                                {
                                    ExtractFile(e, strTargetDir);
                                }
                                else
                                    e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);

                                if ((e.Attributes & FileAttributes.Directory) == 0)
                                {
                                    if (e.LastModified != File.GetLastWriteTime(strFullPath))
                                    {
                                        // 时间有可能不一致，可能是夏令时之类的问题
                                        File.SetLastWriteTime(strFullPath, e.LastModified);
                                    }
                                    Debug.Assert(e.LastModified == File.GetLastWriteTime(strFullPath));
                                    _versionManager.SetFileVersion(strFullPath, e.LastModified.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                _versionManager.SetFileVersion(strEntry, strNewTimestamp);
                _versionManager.AutoSave();
            }

            _versionManager.AutoSave();
            return 0;
        }

        // 去掉第一级路径
        static string GetSubPath(string strPath)
        {
            int nRet = strPath.IndexOfAny(new char[] { '/', '\\' }, 0);
            if (nRet == -1)
                return "";
            return strPath.Substring(nRet + 1);
        }

        void ExtractFile(ZipEntry e, string strTargetDir)
        {
#if NO
            string strTempDir = Path.Combine(this.UserDir, "temp");
            PathUtil.CreateDirIfNeed(strTempDir);
#endif
            string strTempDir = this.TempDir;

            string strTempPath = Path.Combine(strTempDir, Path.GetFileName(e.FileName));
            string strTargetPath = Path.Combine(strTargetDir, e.FileName);

            using (FileStream stream = new FileStream(strTempPath, FileMode.Create))
            {
                e.Extract(stream);
            }

            int nErrorCount = 0;
            for (; ; )
            {
                try
                {
                    // 确保目标目录已经创建
                    PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strTargetPath));

                    File.Copy(strTempPath, strTargetPath, true);
                }
                catch (Exception ex)
                {
                    if (nErrorCount > 10)
                    {
                        DialogResult result = MessageBox.Show(this,
"复制文件 " + strTempPath + " 到 " + strTargetPath + " 的过程中出现错误: " + ex.Message + "。\r\n\r\n是否要重试？",
"dp2Installer",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            throw new Exception("复制文件 " + strTargetPath + " 到 " + strTargetPath + " 的过程中出现错误: " + ex.Message);
                        }
                        nErrorCount = 0;
                    }

                    nErrorCount++;
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            File.Delete(strTempPath);
        }

        // 刷新应用程序目录
        // parameters:
        //      bForce  true 强制升级  false 自动升级，如果 .zip 文件时间戳没有变化就不升级
        int dp2OPAC_UpdateAppDir(
            bool bForce,
            out string strError)
        {
            strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "opac_app.zip");

            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(Path.GetFileName(strZipFileName), out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();

            if (bForce == true || CompareTimestamp(strOldTimestamp, strNewTimestamp) != 0)  // 2016/9/28 原来是 <
            {
                if (bForce == false)
                    AppendSectionTitle("自动升级 dp2OPAC");

                if (string.IsNullOrEmpty(strOldTimestamp) == false)
                    AppendString("已安装时间戳 " + strOldTimestamp + "\r\n");
                AppendString("将安装时间戳 " + strNewTimestamp + "\r\n");

                // 2015/7/21
                // 先删除 目标目录下的 app_code 目录内的所有文件
                // 这是因为以前版本的 dp2OPAC 可能在这里遗留了 global.asax.cs 文件，而新版本移动到其上级子目录存储了
                string strAppCodeDir = Path.Combine(this.OpacAppDir, "app_code");
                if (Directory.Exists(strAppCodeDir) == true)
                {
                    PathUtil.DeleteDirectory(strAppCodeDir);
                }

                try
                {
                    using (ZipFile zip = ZipFile.Read(strZipFileName))
                    {
                        foreach (ZipEntry e in zip)
                        {
                            if (e.FileName.ToLower() == "opac_app/web.config")
                                continue;

                            AppendString(e.FileName + "\r\n");

                            e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                _versionManager.SetFileVersion(Path.GetFileName(strZipFileName), strNewTimestamp);
                _versionManager.AutoSave();

                if (bForce == false)
                    AppendSectionTitle("结束升级 dp2OPAC");
            }

            return 0;
        }

        // 创建应用程序目录，并复制进基本内容
        int dp2OPAC_CreateNewAppDir(
            string strDataDir,
            out string strError)
        {
            strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "opac_app.zip");

            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            // 创建start.xml文件
            string strStartXmlFileName = Path.Combine(this.OpacAppDir, "start.xml");
            int nRet = this.CreateStartXml(strStartXmlFileName,
                strDataDir,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 创建start.xml文件
        // parameters:
        //      strFileName start.xml文件名
        int CreateStartXml(string strFileName,
            string strDataDir,
            out string strError)
        {
            strError = "";

            try
            {
                string strXml = "<root datadir=''/>";

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);

                DomUtil.SetAttr(dom.DocumentElement, "datadir", strDataDir);

                dom.Save(strFileName);

                return 0;
            }
            catch (Exception ex)
            {
                strError = "创建start.xml文件出错：" + ex.Message;
                return -1;
            }
        }


        // 创建数据目录，并复制进基本内容
        int dp2OPAC_CreateNewDataDir(
            out string strError)
        {
            strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "opac_data.zip");

            // 要求在 opac_data.zip 内准备要安装的数据文件(初次安装而不是升级安装)
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            return 0;
        }

        // 创建/修改 opac.xml 文件
        // return:
        //      -1  error
        //      0   succeed
        public int dp2OPAC_createXml(string strDataDir,
            string strOpacUserName,
            string strOpacPassword,
            // string strSupervisorRights,
            // string strLibraryName,
            out string strError)
        {
            strError = "";

            string strFilename = PathUtil.MergePath(strDataDir, "opac.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + " 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode nodeRmsServer = dom.DocumentElement.SelectSingleNode("libraryServer");
            if (nodeRmsServer == null)
            {
                nodeRmsServer = dom.CreateElement("libraryServer");
                dom.DocumentElement.AppendChild(nodeRmsServer);
            }

            DomUtil.SetAttr(nodeRmsServer,
                "url",
                LibraryHost.default_single_url);
            DomUtil.SetAttr(nodeRmsServer,
                 "username",
                 strOpacUserName);

            string strPassword = Cryptography.Encrypt(strOpacPassword, "dp2circulationpassword");
            DomUtil.SetAttr(nodeRmsServer,
                "password",
                strPassword);

            // 报表目录
            DomUtil.SetAttr(nodeRmsServer,
     "reportDir",
     Path.Combine(this.LibraryDataDir, "upload/reports"));

            dom.Save(strFilename);
            return 0;
        }

        #endregion

        private void MenuItem_setupOpacDataAppDir_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 安装 dp2OPAC 的数据目录
            // parameters:
            //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
            int nRet = SetupOpacDataAndAppDir(
                false,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            MessageBox.Show(this, "dp2OPAC 数据目录和应用程序目录安装成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        Process _processIIsExpress = null;

        void CloseIIsExpress(bool bWait = true)
        {
            if (_processIIsExpress != null)
            {
                try
                {
                    _processIIsExpress.Kill();
                    if (bWait == true)
                        _processIIsExpress.WaitForExit();
                    _processIIsExpress.Dispose();
                }
                catch
                {
                }

                _processIIsExpress = null;
            }
        }

        // return:
        //      -1  出错
        //      0   程序文件不存在
        //      1   成功启动
        int StartIIsExpress(string strSite,
            bool bHide,
            out string strError)
        {
            strError = "";

            CloseIIsExpress();

            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
"iis express\\iisexpress.exe");

            if (File.Exists(fileName) == false)
            {
                strError = "文件 " + fileName + " 不存在， IIS Express 启动失败";
                return 0;
            }

            string arguments = "";


            if (string.IsNullOrEmpty(strSite) == false)
            {
                arguments = "/site:" + strSite + " /systray:true";
            }

            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = fileName;
            startinfo.Arguments = arguments;
            if (bHide == true)
            {
                // 此二行会导致 statis.aspx 停顿
                //startinfo.RedirectStandardOutput = true;
                //startinfo.RedirectStandardError = true;

                startinfo.UseShellExecute = false;
                startinfo.CreateNoWindow = true;
            }

            Process process = new Process();
            process.StartInfo = startinfo;
            process.EnableRaisingEvents = true;

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                strError = "运行 IIS Express 异常: " + ex.Message;
                return -1;
            }
            _processIIsExpress = process;
            return 1;
        }

        private void MenuItem_startIISExpress_Click(object sender, EventArgs e)
        {
#if NO
            if (_processIIsExpress != null)
            {
                _processIIsExpress.Kill();
                _processIIsExpress.WaitForExit();
                _processIIsExpress.Dispose();
                _processIIsExpress = null;
            }

            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
"iis express\\iisexpress");

            // string fileName = "\"%programfiles%/iis express\\iisexpress\"";
            string arguments = "/site:dp2Site";
            // string arguments = "/path:" + this.OpacAppDir + " /port:8081";
            _processIIsExpress = Process.Start(fileName, arguments);
#endif
            string strError = "";

            bool bHide = true;
            if (Control.ModifierKeys == Keys.Control)
                bHide = false;
            // return:
            //      -1  出错
            //      0   程序文件不存在
            //      1   成功启动
            int nRet = StartIIsExpress("dp2Site", bHide, out strError);
            if (nRet == 1)
                AppendSectionTitle("IIS Express 启动成功");
            else
            {
                AppendSectionTitle("IIS Express 启动失败: " + strError);
                MessageBox.Show(this, strError);
            }
        }

        private void MenuItem_registerWebApp_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = RegisterWebApp(out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "注册成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int RegisterWebApp(out string strError)
        {
            strError = "";

            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
"iis express/appcmd");

            List<string> lines = new List<string>();

            // 创建新的 Site
            lines.Add("add site /name:dp2Site /bindings:\"http/:8081:\" /physicalPath:\"" + this.dp2SiteDir + "\"");

            // 允许任何 IP 域名访问本站点
            // lines.Add("set site \"WebSite1\" /bindings:http/:8080:");

            // 创建应用程序
            lines.Add("delete app \"dp2Site/dp2OPAC\"");
            lines.Add("add app /site.name:dp2Site /path:/dp2OPAC /physicalPath:" + this.OpacAppDir);

            // 创建 AppPool
            lines.Add("delete apppool \"dp2OPAC\"");
            lines.Add("add apppool /name:dp2OPAC");
            // 修改 AppPool 特性： .NET 4.0
            lines.Add("set apppool \"dp2OPAC\" /managedRuntimeVersion:v4.0");
            // 修改 AppPool 特性： Integrated
            lines.Add("set apppool \"dp2OPAC\" /managedPipelineMode:Integrated");

            // 修改 AppPool 特性： disallowOverlappingRotation
            lines.Add("set apppool \"dp2OPAC\" /recycling.disallowOverlappingRotation:true");

            // 使用这个 AppPool
            lines.Add("set app \"dp2Site/dp2OPAC\" /applicationPool:dp2OPAC");

            // 确保 MyDocuments 里面的 IISExpress 和 My WebSites 目录创建


            // return:
            //      -1  出错
            //      0   程序文件不存在
            //      1   成功启动
            int nRet = StartIIsExpress("", true, out strError);
            if (nRet != 1)
                return -1;

            Thread.Sleep(3000);
            CloseIIsExpress();

            AppendSectionTitle("开始注册");
            try
            {
                int i = 0;
                foreach (string arguments in lines)
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    AppendString("\r\n" + (i + 1).ToString() + ")\r\n" + fileName + " " + arguments + "\r\n");

                    // Process.Start(fileName, arguments).WaitForExit();
                    using (Process process = Process.Start(info))
                    {

                        process.OutputDataReceived += new DataReceivedEventHandler(
            (s, e1) =>
            {
                AppendString(e1.Data + "\r\n");
            }
        );
                        process.ErrorDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            AppendString("error:" + e1.Data + "\r\n");
                        }
                        );
                        process.BeginOutputReadLine();
                        while (true)
                        {
                            Application.DoEvents();
                            if (process.WaitForExit(500) == true)
                                break;
                        }
                        // 显示残余的文字
#if NO
                        while (!process.StandardOutput.EndOfStream)
                        {
                            Application.DoEvents();
                            Thread.Sleep(1);
                        }
#endif
                        // process.CancelOutputRead();
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }

                    i++;
                }
            }
            finally
            {
                AppendSectionTitle("结束注册");
            }

            // TODO：需要重新启动 IIS Express
            return 0;
        }

        private void MenuItem_iisExpressVersion_Click(object sender, EventArgs e)
        {
            string strError = "";

            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
"iis express\\iisexpress.exe");
            if (File.Exists(fileName) == false)
            {
                strError = "文件 " + fileName + " 不存在";
                goto ERROR1;
            }

            try
            {
                FileVersionInfo version = FileVersionInfo.GetVersionInfo(fileName);
                MessageBox.Show(this, version.FileMajorPart.ToString());
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*
发生未捕获的界面线程异常: 
Type: System.ComponentModel.Win32Exception
Message: 找不到应用程序
Stack:
在 System.Diagnostics.Process.StartWithShellExecuteEx(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start()
在 System.Diagnostics.Process.Start(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start(String fileName)
在 dp2LibraryXE.MainForm.MenuItem_installDp2Opac_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)
在 System.Windows.Forms.ToolStripMenuItem.OnClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStripItem.FireEventInteractive(EventArgs e, ToolStripItemEventType met)
在 System.Windows.Forms.ToolStripItem.FireEvent(EventArgs e, ToolStripItemEventType met)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.ToolStripDropDown.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ScrollableControl.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.ToolStripDropDown.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


         * */
        // 安装 dp2OPAC
        private void MenuItem_installDp2Opac_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("开始安装 dp2OPAC");

            bool bForce = false;
            if (Control.ModifierKeys == Keys.Control)
            {
                AppendString("强制安装\r\n");
                bForce = true;
            }

            if (bForce == false)
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                }
                else
                {
                    strError = "当前 Windows 操作系统版本太低，无法安装使用 IIS Express 8.0";
                    goto ERROR1;
                }
            }

            // 首先安装 IIS Express 8
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
"iis express\\iisexpress.exe");

            if (bForce == false)
            {
                if (File.Exists(fileName) == true
                    && FileVersionInfo.GetVersionInfo(fileName).FileMajorPart >= 8)
                {
                }
                else
                {

                    // 安装 IIS Express 8
                    strError = "需要先安装 IIS Express 8.0。\r\n\r\n安装完 IIS Express 后，请重新执行本命令";
                    AppendString(strError + "\r\n");

                    this.AppInfo.SetBoolean("OPAC", "installed", false);
                    this.AppInfo.Save();

                    MessageBox.Show(this, strError);
                    string install_url = "http://www.microsoft.com/zh-cn/download/details.aspx?id=34679";
                    try
                    {
                        Process.Start(install_url);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "打开 URL '" + install_url + "' 失败: " + ex.Message);
                    }
                    return;
                }
            }

#if NO
            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            _messageBar.Font = this.Font;
            _messageBar.BackColor = SystemColors.Info;
            _messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2Library XE";
            _messageBar.MessageText = "正在安装 dp2OPAC，请等待 ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            // _messageBar.TopMost = true;
            _messageBar.Show(this);
            _messageBar.Update();
#endif
            this._floatingMessage.Text = "正在安装 dp2OPAC，请等待 ...";

            Application.DoEvents();

            try
            {
                // 安装 dp2OPAC 的数据目录
                // parameters:
                //      bAutoSetup  是否自动安装。自动安装时，如果已经存在数据文件，则不会再次安装。否则会强行重新安装，但安装前会出现对话框警告
                nRet = SetupOpacDataAndAppDir(
                    false,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                nRet = RegisterWebApp(out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.AppInfo.SetBoolean("OPAC", "installed", true);
                this.AppInfo.Save();
            }
            finally
            {
#if NO
                _messageBar.Close();
                _messageBar = null;
#endif
                this._floatingMessage.Text = "";
            }

            string strInformation = "dp2OPAC 安装完成。\r\n\r\n在本机可以使用 " + localhost_opac_url + " 访问";
            AppendString(strInformation + "\r\n");

            // 检查当前超级用户帐户是否为空密码
            // return:
            //      -1  检查中出错
            //      0   空密码
            //      1   已经设置了密码
            nRet = CheckNullPassword(out strError);
            if (nRet == -1)
                MessageBox.Show(this, "检查超级用户密码的过程出错: " + strError);

            if (nRet == 0)
            {
                MessageBox.Show(this, strInformation);

                MessageBox.Show(this, "当前超级用户 " + this.SupervisorUserName + " 的密码为空，如果启动 dp2OPAC，其他人将可能通过浏览器冒用此账户。\r\n\r\n请(使用 dp2circulation (内务前端))为此账户设置密码，然后重新启动 dp2libraryXE。\r\n\r\n为确保安全，本次未启动 dp2OPAC",
                    "dp2library XE 警告",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                // MessageBox.Show(this, "即将启动 IIS Express。不要关闭这个窗口");
                // return:
                //      -1  出错
                //      0   程序文件不存在
                //      1   成功启动
                nRet = StartIIsExpress("dp2Site", true, out strError);
                if (nRet != 1)
                    goto ERROR1;
                MessageBox.Show(this, strInformation);
                try
                {
                    Process.Start(localhost_opac_url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "打开 URL '" + localhost_opac_url + "' 失败: " + ex.Message);
                }
            }

            AppendSectionTitle("结束安装 dp2OPAC");
            return;
        ERROR1:
            AppendString(strError + "\r\n");

            this.AppInfo.SetBoolean("OPAC", "installed", false);
            this.AppInfo.Save();

            MessageBox.Show(this, strError);
        }


        // download IIS Express 8.0
        // http://www.microsoft.com/en-us/download/details.aspx?id=34679


        #region console
        /// <summary>
        /// 将浏览器控件中已有的内容清除，并为后面输出的纯文本显示做好准备
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        public static void ClearForPureTextOutputing(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                // + "<link rel='stylesheet' href='"+strCssFileName+"' type='text/css'>"
    + "<style media='screen' type='text/css'>"
    + "body { font-family:Microsoft YaHei; background-color:#555555; color:#eeeeee; } "
    + "</style>"
    + "</head><body>";

            doc = doc.OpenNew(true);
            doc.Write(strHead + "<pre style=\"font-family:Consolas; \">");  // Calibri
        }

        /// <summary>
        /// 将 HTML 信息输出到控制台，显示出来。
        /// </summary>
        /// <param name="strText">要输出的 HTML 字符串</param>
        public void WriteToConsole(string strText)
        {
            WriteHtml(this.webBrowser1, strText);
        }

        /// <summary>
        /// 将文本信息输出到控制台，显示出来
        /// </summary>
        /// <param name="strText">要输出的文本字符串</param>
        public void WriteTextToConsole(string strText)
        {
            WriteHtml(this.webBrowser1, HttpUtility.HtmlEncode(strText));
        }

        /// <summary>
        /// 向一个浏览器控件中追加写入 HTML 字符串
        /// 不支持异步调用
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
#if NO
                webBrowser.DocumentText = "<h1>hello</h1>";
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "");
#endif
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);
        }


        void AppendSectionTitle(string strText)
        {
            AppendCrLn();
            AppendString("*** " + strText + " ***\r\n");
            AppendCurrentTime();
            AppendCrLn();
        }

        void AppendCurrentTime()
        {
            AppendString("*** " + DateTime.Now.ToString() + " ***\r\n");
        }

        void AppendCrLn()
        {
            AppendString("\r\n");
        }

        // 线程安全
        void AppendString(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<string>(AppendString), strText);
                return;
            }
            this.WriteTextToConsole(strText);
            ScrollToEnd();
        }

        void ScrollToEnd()
        {
#if NO
            this.webBrowser1.Document.Window.ScrollTo(
                0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
#endif
            this.webBrowser1.ScrollToEnd();
        }


        #endregion

        // 检查当前超级用户帐户是否为空密码
        // return:
        //      -1  检查中出错
        //      0   空密码
        //      1   已经设置了密码
        int CheckNullPassword(out string strError)
        {
            strError = "";

            int nRet = PrepareSearch();
            try
            {
                EnableControls(false);

                Stop.OnStop += new StopEventHandler(this.DoStop);
                Stop.Initial("正在检查密码 ...");
                Stop.BeginLoop();

                try
                {
                    int nRedoCount = 0;
                REDO:
                    // return:
                    //      -1  error
                    //      0   登录未成功
                    //      1   登录成功
                    long lRet = Channel.Login(this.SupervisorUserName,
                        "",
                        "type=worker,client=dp2LibraryXE|" + Program.ClientVersion,
                        out strError);
                    if (lRet == -1)
                    {
                        if (Channel.WcfException is System.TimeoutException
                            && nRedoCount < 3)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        strError = "已经设置了密码";
                        return 1;
                    }

                    strError = "密码为空，危险";
                    return 0;
                }
                finally
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");

                    EnableControls(true);
                }
            }
            finally
            {
                EndSearch();
            }
        }

        private void MenuItem_test_Click(object sender, EventArgs e)
        {
            int nRet = PrepareSearch();
            try
            {
                EnableControls(false);

                Stop.OnStop += new StopEventHandler(this.DoStop);
                Stop.Initial("testing ...");
                Stop.BeginLoop();

                try
                {
                    DigitalPlatform.LibraryClient.localhost.BiblioDbFromInfo[] infos = null;
                    string strError = "";
                    long lRet = Channel.ListDbFroms(Stop,
                        "biblio",
                        "zh",
                        out infos,
                        out strError);
#if NO
                    if (lRet == -1)
                        return -1; ;
                    return (int)lRet;
#endif
                }
                finally
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");

                    EnableControls(true);
                }
            }
            finally
            {
                EndSearch();
            }
        }

        /*
发生未捕获的界面线程异常: 
Type: System.ComponentModel.Win32Exception
Message: 找不到应用程序
Stack:
在 System.Diagnostics.Process.StartWithShellExecuteEx(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start()
在 System.Diagnostics.Process.Start(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start(String fileName)
在 dp2LibraryXE.MainForm.MenuItem_openDp2OPACHomePage_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)
在 System.Windows.Forms.ToolStripMenuItem.OnClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStripItem.FireEventInteractive(EventArgs e, ToolStripItemEventType met)
在 System.Windows.Forms.ToolStripItem.FireEvent(EventArgs e, ToolStripItemEventType met)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.ToolStripDropDown.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ScrollableControl.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.ToolStripDropDown.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


         * */
        private void MenuItem_openDp2OPACHomePage_Click(object sender, EventArgs e)
        {
            bool bInstalled = this.AppInfo.GetBoolean("OPAC", "installed", false);
            if (bInstalled == false)
                MessageBox.Show(this, "dp2OPAC 尚未安装");
            else
            {
                try
                {
                    Process.Start(localhost_opac_url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "打开 URL '" + localhost_opac_url + "' 失败: " + ex.Message);
                }
            }
        }

        private void MenuItem_updateDp2Opac_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("开始升级 dp2OPAC");

            bool bForce = false;
            if (Control.ModifierKeys == Keys.Control)
            {
                AppendString("强制安装\r\n");
                bForce = true;
            }

            if (bForce == false)
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                }
                else
                {
                    strError = "当前 Windows 操作系统版本太低，无法安装使用 IIS Express 8.0";
                    goto ERROR1;
                }
            }

            string fileName = Path.Combine(
                this.OpacAppDir, "book.aspx");

            if (bForce == false)
            {
                if (File.Exists(fileName) == true)
                {
                }
                else
                {
                    strError = "尚未安装 dp2OPAC";
                    goto ERROR1;
                }
            }

#if NO
            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            _messageBar.Font = this.Font;
            _messageBar.BackColor = SystemColors.Info;
            _messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2Library XE";
            _messageBar.MessageText = "正在升级 dp2OPAC，请等待 ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            // _messageBar.TopMost = true;
            _messageBar.Show(this);
            _messageBar.Update();
#endif
            this._floatingMessage.Text = "正在升级 dp2OPAC，请等待 ...";

            Application.DoEvents();

            try
            {
                nRet = dp2OPAC_UpdateAppDir(true, out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
#if NO
                _messageBar.Close();
                _messageBar = null;
#endif
                this._floatingMessage.Text = "";
            }

            AppendSectionTitle("结束升级 dp2OPAC");
            return;
        ERROR1:
            AppendString(strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        private void MenuItem_getSqllocaldbexePath_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strPath = "";

#if NO
            // 获得 sqllocaldb.exe 的全路径
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = GetSqlLocalDBExePath(out strPath,
                out strError);
            if (nRet == 1)
                MessageBox.Show(this, strPath);
            else
                MessageBox.Show(this, strError);
#endif
            MessageBox.Show(this, FindExePath("sqllocaldb.exe"));
        }

        // 写入日志文件。每天创建一个单独的日志文件
        public void WriteErrorLog(string strText)
        {
            FileUtil.WriteErrorLog(
                this.UserLogDir,
                this.UserLogDir,
                strText,
                "log_",
                ".txt");
        }

        private void MenuItem_restartDp2library_Click(object sender, EventArgs e)
        {
            RestartDp2libraryIfNeed();
        }

        private void MenuItem_restartDp2Kernel_Click(object sender, EventArgs e)
        {
            RestartDp2kernelIfNeed();
        }

        private void MenuItem_enableWindowsMsmq_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            var featureNames = new[] 
    {
        "MSMQ-Container",
        "MSMQ-Server",
    };
            // Windows Server 2008, Windows Server 2012 的用法
            var server_featureNames = new[] 
    {
        "MSMQ-Services",
        "MSMQ-Server",
    };

            nRet = EnableServerFeature("MSMQ",
                InstallHelper.isWindowsServer ? server_featureNames : featureNames,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            /*

1)
C:\WINDOWS\SysNative\dism.exe /NoRestart /Online /Enable-Feature /FeatureName:MSMQ-Container /FeatureName:MSMQ-Server

部署映像服务和管理工具
版本: 10.0.10586.0

映像版本: 10.0.10586.0

启用一个或多个功能

[                           0.1%                           ] 

[==========================100.0%==========================] 
操作成功完成。
             * */

            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        int EnableServerFeature(string strName,
    string[] featureNames,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            // http://stackoverflow.com/questions/5936719/calling-dism-exe-from-system-diagnostics-process-fails
            string strFileName = "%WINDIR%\\SysNative\\dism.exe";
            strFileName = Environment.ExpandEnvironmentVariables(strFileName);

            string strLine = string.Format(
            "/NoRestart /Online /Enable-Feature {0}",
            string.Join(
                " ",
                featureNames.Select(name => string.Format("/FeatureName:{0}", name))));

            AppendSectionTitle("开始启用 " + strName);
            AppendString("整个过程耗费的时间可能较长，请耐心等待 ...\r\n");
            Application.DoEvents();

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;
            try
            {
                // parameters:
                //      lines   若干行参数。每行执行一次
                // return:
                //      -1  出错
                //      0   成功。strError 里面有运行输出的信息
                nRet = InstallHelper.RunCmd(
                    strFileName,
                    new List<string> { strLine },
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;
                AppendString(RemoveProgressText(strError));
            }
            finally
            {
                AppendSectionTitle("结束启用 " + strName);

                this.Cursor = oldCursor;
                this.Enabled = true;
            }

            return 0;
        }

        static string RemoveProgressText(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return "";

            List<string> results = new List<string>();

            string[] lines = strText.Replace("\r\n", "\r").Split(new char[] { '\r' });
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                string strLine = line.Trim();
                if (string.IsNullOrEmpty(strLine))
                    continue;

                if (strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                    continue;
                results.Add(strLine);
            }

            return string.Join("\r\n", results.ToArray());
        }


        private void MenuItem_configLibraryXmlMq_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strLibraryXmlFileName = PathUtil.MergePath(this.LibraryDataDir, "library.xml");
            if (File.Exists(strLibraryXmlFileName) == false)
            {
                strError = "单机版 dp2Library 模块 尚未安装。请先安装 dp2Library 模块";
                goto ERROR1;
            }
            // 为 dp2library 的 library.xml 文件增配 MSMQ 相关参数。这之前要确保在 Windows 上启用了 Message Queue Service。
            // return:
            //      -1  出错
            //      0   没有修改
            //      1   发生了修改
            int nRet = InstallHelper.SetupMessageQueue(
                strLibraryXmlFileName,
                "xe",
                Control.ModifierKeys == Keys.Control ? false : true,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
                MessageBox.Show(this, "添加参数成功");
            else
                MessageBox.Show(this, "配置文件本次操作后没有发生变化 (先前已经配置过 MQ 参数了)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_configLibraryXmlMongoDB_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strLibraryXmlFileName = PathUtil.MergePath(this.LibraryDataDir, "library.xml");
            if (File.Exists(strLibraryXmlFileName) == false)
            {
                strError = "单机版 dp2Library 模块 尚未安装。请先安装 dp2Library 模块";
                goto ERROR1;
            }
            // 为 dp2library 的 library.xml 文件增配 MSMQ 相关参数。这之前要确保在 Windows 上启用了 Message Queue Service。
            // return:
            //      -1  出错
            //      0   没有修改
            //      1   发生了修改
            int nRet = InstallHelper.SetupMongoDB(
                strLibraryXmlFileName,
                "xe",
                Control.ModifierKeys == Keys.Control ? false : true,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
                MessageBox.Show(this, "添加参数成功");
            else
                MessageBox.Show(this, "配置文件本次操作后没有发生变化 (先前已经配置过 MongoDB 参数了)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static string GetExePath(string command_line)
        {
            int nRet = command_line.IndexOf(".exe");
            if (nRet == -1)
                return command_line;
            if (command_line[nRet + ".exe".Length] == '\"')
                return command_line.Substring(0, nRet + ".exe".Length + 1);
            return command_line.Substring(0, nRet + ".exe".Length);
        }

        void RemoveMongoDBService()
        {
            string strError = "";
            int nRet = 0;

            string strCommandLine = InstallHelper.GetPathOfService("MongoDB");
            if (string.IsNullOrEmpty(strCommandLine) == true)
            {
                strError = "MongoDB 先前并未注册为 Windows Service。所以无法 Remove";
                goto ERROR1;
            }

            string strFileName = GetExePath(strCommandLine);    // Path.Combine(dlg.BinDir, "mongod.exe");
            // strFileName = StringUtil.Unquote(strFileName, "\"\"");
            string strLine = " --remove";

            AppendSectionTitle("开始移走 MongoDB");
            Application.DoEvents();

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;
            try
            {
                // parameters:
                //      lines   若干行参数。每行执行一次
                // return:
                //      -1  出错
                //      0   成功。strError 里面有运行输出的信息
                nRet = InstallHelper.RunCmd(
                    strFileName,
                    new List<string> { strLine },
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString(RemoveProgressText(strError));
            }
            finally
            {
                AppendSectionTitle("结束移走 MongoDB");

                this.Cursor = oldCursor;
                this.Enabled = true;
            }

            AppendString("MongoDB 移走成功\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2library_setupMongoDB_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                RemoveMongoDBService();
                return;
            }

            string strError = "";
            int nRet = 0;

            string strExePath = InstallHelper.GetPathOfService("MongoDB");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strError = "MongoDB 已经安装过了。(位于 " + strExePath + ")";
                goto ERROR1;
            }

            SetupMongoDbDialog dlg = new SetupMongoDbDialog();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.DataDir = "c:\\mongo_data";

            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 创建目录和 mongod.cfg 文件
            string strDataDir = dlg.DataDir;
            string strConfigFileName = Path.Combine(strDataDir, "mongod.cfg");

            PathUtil.CreateDirIfNeed(Path.Combine(strDataDir, "db"));
            PathUtil.CreateDirIfNeed(Path.Combine(strDataDir, "log"));

            using (StreamWriter sw = new StreamWriter(strConfigFileName, false))
            {
                sw.WriteLine("systemLog:");
                sw.WriteLine("    destination: file");
                sw.WriteLine("    path: " + strDataDir + "\\log\\mongod.log");
                sw.WriteLine("storage:");
                sw.WriteLine("    dbPath: " + strDataDir + "\\db");
                sw.WriteLine("net:");
                sw.WriteLine("   bindIp: 127.0.0.1");
                sw.WriteLine("   port: 27017");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
            }

            // 
            // 在 mongod.exe 所在目录执行：
            // "C:\mongodb\bin\mongod.exe" --config "C:\mongodb\mongod.cfg" –install

            string strFileName = Path.Combine(dlg.BinDir, "mongod.exe");
            string strLine = " --config " + strConfigFileName + " --install";

            AppendSectionTitle("开始启用 MongoDB");
            Application.DoEvents();

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;
            try
            {
                // parameters:
                //      lines   若干行参数。每行执行一次
                // return:
                //      -1  出错
                //      0   成功。strError 里面有运行输出的信息
                nRet = InstallHelper.RunCmd(
                    strFileName,
                    new List<string> { strLine },
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString(RemoveProgressText(strError));
            }
            finally
            {
                AppendSectionTitle("结束启用 MongoDB");

                this.Cursor = oldCursor;
                this.Enabled = true;
            }

            AppendString("MongoDB 安装配置成功\r\n");

            Thread.Sleep(1000);

            {
                AppendString("正在启动 MongoDB 服务 ...\r\n");
                nRet = InstallHelper.StartService("MongoDB",
    out strError);
                if (nRet == -1)
                {
                    AppendString("MongoDB 服务启动失败: " + strError + "\r\n");
                    goto ERROR1;
                }
                else
                {
                    AppendString("MongoDB 服务启动成功\r\n");
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

    }

    /*
     * 删除一个 App
     * D:\Program Files\IIS Express>appcmd delete app /app.name:WebSite1/dp2OPAC
APP 对象“WebSite1/dp2OPAC”已删除

     * 添加一个 AppPool
     * D:\Program Files\IIS Express>appcmd add apppool /name:dp2OPAC
APPPOOL 对象“dp2OPAC”已添加
     * 
     * 察看 AppPool 特性
     * D:\Program Files\IIS Express>appcmd list apppool "dp2OPAC" /text:*
APPPOOL
  APPPOOL.NAME:"dp2OPAC"
  PipelineMode:"Integrated"
  RuntimeVersion:""
  state:"Unknown"
  [add]
    name:"dp2OPAC"
    queueLength:"1000"
    autoStart:"true"
    enable32BitAppOnWin64:"false"
    managedRuntimeVersion:""
    managedRuntimeLoader:"v4.0"
    enableConfigurationOverride:"true"
    managedPipelineMode:"Integrated"
    CLRConfigFile:""
    passAnonymousToken:"true"
    startMode:"OnDemand"
    [processModel]
      identityType:"ApplicationPoolIdentity"
      userName:""
      password:""
      loadUserProfile:"false"
      setProfileEnvironment:"true"
      logonType:"LogonBatch"
      manualGroupMembership:"false"
      idleTimeout:"00:20:00"
      maxProcesses:"1"
      shutdownTimeLimit:"00:01:30"
      startupTimeLimit:"00:01:30"
      pingingEnabled:"true"
      pingInterval:"00:00:30"
      pingResponseTime:"00:01:30"
      logEventOnProcessModel:"IdleTimeout"
    [recycling]
      disallowOverlappingRotation:"false"
      disallowRotationOnConfigChange:"false"
      logEventOnRecycle:"Time, Memory, PrivateMemory"
      [periodicRestart]
        memory:"0"
        privateMemory:"0"
        requests:"0"
        time:"1.05:00:00"
        [schedule]
    [failure]
      loadBalancerCapabilities:"HttpLevel"
      orphanWorkerProcess:"false"
      orphanActionExe:""
      orphanActionParams:""
      rapidFailProtection:"true"
      rapidFailProtectionInterval:"00:05:00"
      rapidFailProtectionMaxCrashes:"5"
      autoShutdownExe:""
      autoShutdownParams:""
    [cpu]
      limit:"0"
      action:"NoAction"
      resetInterval:"00:05:00"
      smpAffinitized:"false"
      smpProcessorAffinityMask:"4294967295"
      smpProcessorAffinityMask2:"4294967295"
      processorGroup:"0"
      numaNodeAssignment:"MostAvailableMemory"
      numaNodeAffinityMode:"Soft"

     * 
     * 
     * D:\Program Files\IIS Express>appcmd set apppool "dp2OPAC" /managedRuntimeVersion:v4.0
APPPOOL 对象“dp2OPAC”已更改

     * D:\Program Files\IIS Express>appcmd set apppool "dp2OPAC" /managedPipelineMode:Integrated
APPPOOL 对象“dp2OPAC”已更改
     * 
     * D:\Program Files\IIS Express>appcmd set apppool "dp2OPAC" /recycling.disallowOverlappingRotation:true
APPPOOL 对象“dp2OPAC”已更改
     * 
     * D:\Program Files\IIS Express>appcmd set app "WebSite1/dp2OPAC" /applicationPool:dp2OPAC
APP 对象“WebSite1/dp2OPAC”已更改
     * 
     * 
     * D:\Program Files\IIS Express>appcmd list site
SITE "WebSite1" (id:1,bindings:http/:8080:localhost,state:Unknown)

D:\Program Files\IIS Express>appcmd set site "WebSite1" /bindings:http/*:8080:

D:\Program Files\IIS Express>appcmd list site
SITE "WebSite1" (id:1,bindings:http/*:8080:,state:Unknown)

D:\Program Files\IIS Express>
     * 
     * 
     * 
     * */


}
