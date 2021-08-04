using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Threading;

using System.Deployment.Application;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Net;   // for WebClient class

#if GCAT_SERVER
using DigitalPlatform.GcatClient.gcat_new_ws;
using DigitalPlatform.GcatClient;
#endif

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;   // QuickPinyin IsbnSplitter
using DigitalPlatform.CommonControl;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Core;
using System.Threading.Tasks;
using System.Web;

namespace dp2Catalog
{
    public partial class MainForm : Form, IChannelManager
    {
        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();

        public OperHistory OperHistory = null;

        const int WM_PREPARE = API.WM_USER + 200;
        /*
        public string UsedUcUserName = "";
        public string UserdUcPassword = "";
        public bool UserdUcSavePassword = false;
         * */
        public ObjectCache<Assembly> AssemblyCache = new ObjectCache<Assembly>();
        public ObjectCache<XmlDocument> DomCache = new ObjectCache<XmlDocument>();

        // 为C#脚本所准备
        public Hashtable ParamTable = new Hashtable();

        public QuickPinyin QuickPinyin = null;
        public IsbnSplitter IsbnSplitter = null;
        public QuickSjhm QuickSjhm = null;

        public CfgCache cfgCache = new CfgCache();

        // dp2library服务器信息(数据库syntax等)
        public dp2ServerInfoCollection ServerInfos = new dp2ServerInfoCollection();

        // dp2library服务器数组(缺省用户名/密码等)
        public dp2ServerCollectionNew Servers = null;

        public CharsetTable EaccCharsetTable = null;
        public Marc8Encoding Marc8Encoding = null;

        public string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public string UserDir = "";

        public string UserTempDir = "";

        public string UserLogDir = ""; // 2015/8/8


        //保存界面信息
        public ApplicationInfo AppInfo = null;  // new ApplicationInfo("dp2catalog.xml");

        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();
        public DigitalPlatform.Stop Stop = null;

        public FromCollection Froms = new FromCollection();

        // 使用过的连接MARC文件名
        public string LinkedMarcFileName = "";
        public string LinkedEncodingName = "";
        public string LinkedMarcSyntax = "";

        // 为了保存ISO2709文件服务的几个变量
        public string LastIso2709FileName = "";
        public bool LastCrLfIso2709 = false;
        public bool LastRemoveField998 = false;
        public string LastEncodingName = "";

        public string LastSavePath = "";

        // 最后使用过的工作单文件名
        public string LastWorksheetFileName = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channels_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channels_AfterLogin);

            int nRet = 0;
            string strError = "";

            this.SetTitle();

#if SN
#else
            this.MenuItem_resetSerialCode.Visible = false;
#endif

            // 初始化数据目录
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

            bool bDp2catalogXmlExist = false;

            {
                // 2015/5/8
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2Catalog_v2");
                PathUtil.TryCreateDir(this.UserDir);

                this.UserTempDir = Path.Combine(this.UserDir, "temp");
                PathUtil.TryCreateDir(this.UserTempDir);

                // 2015/8/8
                this.UserLogDir = Path.Combine(this.UserDir, "log");
                PathUtil.TryCreateDir(this.UserLogDir);

                // 将 dp2catalog.xml 文件从绿色安装目录或者 ClickOnce 安装的数据目录移动到用户目录
                nRet = MoveDataFile("dp2catalog.xml", out strError);
                if (nRet == -1)
                {
                    this.ReportError("dp2catalog 移动 dp2catalog.xml 时出现错误", "(安静报错)" + strError);
                    MessageBox.Show(this, strError);
                }

                string strDp2catalogXmlFileName = Path.Combine(this.UserDir, "dp2catalog.xml");
                bDp2catalogXmlExist = File.Exists(strDp2catalogXmlFileName);

                this.AppInfo = new ApplicationInfo(strDp2catalogXmlFileName);

#if NO
                string strOldFileName = Path.Combine(this.DataDir, "zserver.xml");
                string strNewFileName = Path.Combine(this.UserDir, "zserver.xml");
                if (File.Exists(strNewFileName) == false)
                {
                    try
                    {
                        if (File.Exists(strOldFileName) == true)
                        {
                            // 升级到 2.4 的情况。原来数据目录中的 zserver.xml 文件移动过来
                            File.Copy(strOldFileName, strNewFileName, true);
                            File.Delete(strOldFileName);    // 删除源文件，以免用户不清楚哪个文件起作用
                        }
                        else
                        {
                            // 刚安装好的时候，用户目录中还没有文件，于是从 default_zserver.xml 中复制过来
                            string strDefaultFileName = Path.Combine(this.DataDir, "default_zserver.xml");
                            Debug.Assert(File.Exists(strDefaultFileName) == true, "");
                            File.Copy(strDefaultFileName, strNewFileName, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "复制 zserver.xml 文件时出错: " + ex.Message);
                    }
                }
#endif
                MoveZServerXml();
            }

            // 设置窗口尺寸状态
            if (AppInfo != null)
            {
                // 首次运行，尽量利用“微软雅黑”字体
                if (this.IsFirstRun == true)
                {
                    SetFirstDefaultFont();
                }

                GuiUtil.SetControlFont(this, this.DefaultFont);

                AppInfo.LoadFormStates(this,
                    "mainformstate",
                    FormWindowState.Maximized);
            }

            InitialFixedPanel();

            // Stop初始化
            stopManager.Initial(this.toolButton_stop,
                (object)this.toolStripStatusLabel_main,
                (object)this.toolStripProgressBar_main);

            // stopManager.LinkReversButton(this.toolButton_search);
            List<object> reverse_buttons = new List<object>();
            reverse_buttons.Add(this.toolButton_search);
            reverse_buttons.Add(this.toolButton_nextBatch);
            reverse_buttons.Add(this.toolButton_getAllRecords);
            stopManager.LinkReverseButtons(reverse_buttons);

            // 菜单状态
            this.SetMenuItemState();

            // cfgcache
            Debug.Assert(string.IsNullOrEmpty(this.UserDir) == false, "");
            // 2018/6/25 改在 UserDir 下
            nRet = cfgCache.Load(Path.Combine(this.UserDir, "cfgcache.xml"),    // this.DataDir
                out strError);
            if (nRet == -1)
            {
                if (IsFirstRun == false)
                    MessageBox.Show(strError + "\r\n\r\n程序稍后会尝试自动创建这个文件");
            }

            cfgCache.TempDir = Path.Combine(this.UserDir, "cfgcache");  // this.DataDir
            cfgCache.InstantSave = true;

            // Z39.50 froms
            nRet = LoadFroms(Path.Combine(this.DataDir, "bib1use.xml"), out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            // MARC-8字符表
            this.EaccCharsetTable = new CharsetTable();
            try
            {
                this.EaccCharsetTable.Attach(Path.Combine(this.DataDir, "eacc_charsettable"),
                    Path.Combine(this.DataDir, "eacc_charsettable.index"));
                this.EaccCharsetTable.ReadOnly = true;  // 避免Close()的时候删除文件

                this.Marc8Encoding = new Marc8Encoding(this.EaccCharsetTable,
                    Path.Combine(this.DataDir, "asciicodetables.xml"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "装载 EACC 码表文件时发生错误: " + ex.Message);
            }

            // 将 servers.bin 文件从绿色安装目录或者 ClickOnce 安装的数据目录移动到用户目录
            nRet = MoveDataFile("servers.bin", out strError);
            if (nRet == -1)
            {
                this.ReportError("dp2catalog 移动 servers.bin 文件时出现错误", "(安静报错)" + strError);
                MessageBox.Show(this, strError);
            }

            nRet = ConvertServersBin(out strError);
            if (nRet == -1)
            {
                this.ReportError("dp2catalog 转换 servers.bin 文件到 servers.json 时出现错误", "(安静报错)" + strError);
                MessageBox.Show(this, strError);
            }

            // 从文件中装载创建一个dp2ServerCollection对象
            // parameters:
            //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
            //							如果==true，函数直接返回一个新的空ServerCollection对象
            // Exception:
            //			FileNotFoundException	文件没找到
            //			SerializationException	版本迁移时容易出现
            try
            {
                Servers = dp2ServerCollectionNew.Load(
                    Path.Combine(this.UserDir, "servers.json"),
                    true);
                // Servers.ownerForm = this;
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                Servers = new dp2ServerCollectionNew();
                // 设置文件名，以便本次运行结束时覆盖旧文件
                Servers.FileName = Path.Combine(this.DataDir, "servers.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "servers.json 装载出现异常: " + ex.Message);
            }

            this.Servers.ServerChanged += new dp2ServerChangedEventHandle(Servers_ServerChanged);

            if (IsFirstRun == true
                && bDp2catalogXmlExist == false
                // && this.Servers.Count == 0
                )
            {
#if NO
                MessageBox.Show(this, "欢迎您安装使用dp2Catalog -- 编目前端");

                // 提示新增dp2libraaryws服务器
                ManageServers(true);
                // ManagePreference();
#endif

                FirstRunDialog first_dialog = new FirstRunDialog();
                GuiUtil.SetControlFont(first_dialog, this.DefaultFont);
                first_dialog.MainForm = this;
                first_dialog.StartPosition = FormStartPosition.CenterScreen;
                if (first_dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }

                // 首次写入 运行模式 信息
                this.AppInfo.SetString("main_form", "last_mode", first_dialog.Mode);
                if (first_dialog.Mode == "test" || first_dialog.Mode == "community")
                {
                    this.AppInfo.SetString("sn", "sn", first_dialog.Mode);
                    this.AppInfo.Save();
                }

                if (first_dialog.ServerType == "[暂时不使用任何服务器]")
                {
                }
                else
                {
                    dp2ServerCollectionNew newServers = Servers.Dup();
                    dp2Server server = newServers.NewServer(-1);
                    server.Name = first_dialog.ServerName;  // first_dialog.ServerType;
                    if (string.IsNullOrEmpty(server.Name) == true)
                        server.Name = "服务器";
                    server.DefaultPassword = first_dialog.Password;
                    server.Url = first_dialog.ServerUrl;
                    server.DefaultUserName = first_dialog.UserName;
                    server.SavePassword = true;

                    Servers.Changed = true;
                    this.Servers.Import(newServers);
                }

                // 检测zserver.xml是否已经存在？
                // 一般情况下是用不到这个方式的
                string strServerXmlPath = Path.Combine(this.UserDir, "zserver.xml");
                if (FileUtil.FileExist(strServerXmlPath) == false)
                {
                    // 下载数据文件zserver.xml
                    nRet = DownloadUserFile("zserver.xml",
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
            }

            nRet = this.LoadIsbnSplitter(true, out strError);
            if (nRet == -1)
            {
                strError = "装载ISBN处理器时出现错误: " + strError;
                MessageBox.Show(this, strError);
            }

            this.LastSavePath = this.AppInfo.GetString(
                "main_form",
                "last_saved_path",
                "");

            StartPrepareNames(true);

#if GCAT_SERVER
            this.m_strPinyinGcatID = this.AppInfo.GetString("entity_form", "gcat_pinyin_api_id", "");
            this.m_bSavePinyinGcatID = this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", false);
#endif
        }

        void AppendString(string text)
        {
            this.Invoke((Action)(() =>
            {
                this.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(DateTime.Now.ToString() + " " + text) + "</div>");
            }));
        }

        // 将 zserver.xml 文件中绿色安装目录或者 ClickOnce 安装的数据目录移动到用户目录
        void MoveZServerXml()
        {
            string strOldFileName = Path.Combine(this.DataDir, "zserver.xml");
            string strNewFileName = Path.Combine(this.UserDir, "zserver.xml");
            if (File.Exists(strNewFileName) == false)
            {
                try
                {
                    if (File.Exists(strOldFileName) == true)
                    {
                        // 升级到 2.4 的情况。原来数据目录中的 zserver.xml 文件移动过来
                        File.Copy(strOldFileName, strNewFileName, true);
                        File.Delete(strOldFileName);    // 删除源文件，以免用户不清楚哪个文件起作用
                    }
                    else
                    {
                        // 刚安装好的时候，用户目录中还没有文件，于是从 default_zserver.xml 中复制过来
                        string strDefaultFileName = Path.Combine(this.DataDir, "default_zserver.xml");
                        Debug.Assert(File.Exists(strDefaultFileName) == true, "");
                        File.Copy(strDefaultFileName, strNewFileName, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "复制 zserver.xml 文件时出错: " + ex.Message);
                }
            }
        }

        // 把 servers.bin 文件转化为新的 servers.json 格式
        int ConvertServersBin(out string strError)
        {
            strError = "";

            try
            {
                string sourceFileName = Path.Combine(this.UserDir, "servers.bin");
                string targetFileName = Path.Combine(this.UserDir, "servers.json");
                if (File.Exists(sourceFileName) == false
                    || File.Exists(targetFileName) == true)
                    return 0;

                dp2ServerCollection servers = dp2ServerCollection.Load(
        sourceFileName,
        true);
                dp2ServerCollectionNew new_servers = new dp2ServerCollectionNew();
                foreach (dp2Server server in servers)
                {
                    new_servers.Add(server);
                }
                new_servers.Changed = true;
                new_servers.FileName = targetFileName;
                new_servers.Save(null);

                File.Delete(sourceFileName);
                return 1;
            }
            catch (Exception ex)
            {
                strError = "转换 servers.bin 文件到 servers.json 时出现异常: " + ex.Message;
                return -1;
            }
        }

        // 将指定文件文件从绿色安装目录或者 ClickOnce 安装的数据目录移动到用户目录
        int MoveDataFile(
            string strPureFileName,
            out string strError)
        {
            strError = "";

            string strTargetFileName = Path.Combine(this.UserDir, strPureFileName);
            if (File.Exists(strTargetFileName) == true)
                return 0;

            string strSourceDirectory = "";
            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                strSourceDirectory = Application.LocalUserAppDataPath;
            }
            else
            {
                strSourceDirectory = Environment.CurrentDirectory;
            }

            string strSourceFileName = Path.Combine(strSourceDirectory, strPureFileName);
            if (File.Exists(strSourceFileName) == false)
                return 0;   // 没有源文件，无法做什么

            try
            {
                File.Copy(strSourceFileName, strTargetFileName, false);

                // 2020/5/20 增加
                File.Delete(strSourceFileName);
            }
            catch (Exception ex)
            {
                strError = "复制文件 '" + strSourceFileName + "' 到 '" + strTargetFileName + "' 时出现异常：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            return 0;
        }

        public void ReportError(string strTitle,
string strError)
        {
            // 发送给 dp2003.com
            string strText = strError;
            if (string.IsNullOrEmpty(strText) == true)
                return;

            strText += "\r\n\r\n===\r\n";   // +PackageEventLog.GetEnvironmentDescription().Replace("\t", "    ");

            try
            {
                // 发送报告
                int nRet = LibraryChannel.CrashReport(
                    "@MAC:" + Program.GetMacAddressString(),
                    strTitle,
                    strText,
                    out strError);
                if (nRet == -1)
                {
                    strError = "CrashReport() (" + strTitle + ") 出错: " + strError;
                    this.WriteErrorLog(strError);
                }
            }
            catch (Exception ex)
            {
                strError = "CrashReport() (" + strTitle + ") 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.WriteErrorLog(strError);
            }
        }

        public void StartPrepareNames(bool bFullInitial)
        {
            if (bFullInitial == true)
                API.PostMessage(this.Handle, WM_PREPARE, 1, 0);
            else
                API.PostMessage(this.Handle, WM_PREPARE, 0, 0);
        }

        void RestoreLastOpenedMdiWindow(string strOpenedMdiWindow)
        {

            // 缺省开一个Z search form
            if (String.IsNullOrEmpty(strOpenedMdiWindow) == true)
            {
                if (this.Servers.Count == 0)
                    strOpenedMdiWindow = "dp2Catalog.ZSearchForm";  // ,dp2Catalog.dp2SearchForm
                else
                {
                    string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
                    if (string.IsNullOrEmpty(strSerialCode) == false)
                        strOpenedMdiWindow = "dp2Catalog.dp2SearchForm";
                }
            }

            List<string> types = StringUtil.SplitList(strOpenedMdiWindow);
            StringUtil.RemoveDup(ref types, true);
            // string[] types = strOpenedMdiWindow.Split(new char[] { ',' });
            foreach (string strType in types)
            {
                // string strType = types[i];
                if (String.IsNullOrEmpty(strType) == true)
                    continue;

                if (strType == "dp2Catalog.ZSearchForm")
                    this.MenuItem_openZSearchForm_Click(this, null);	// 打开一个Z39.50检索窗
                else if (strType == "dp2Catalog.dp2SearchForm")
                    this.MenuItem_openDp2SearchForm_Click(this, null);	// 打开一个dp2检索窗
                else if (strType == "dp2Catalog.DtlpSearchForm")
                    this.MenuItem_openDtlpSearchForm_Click(this, null);	// 打开一个dp2检索窗
                else if (strType == "dp2Catalog.ZBatchSearchForm")
                    this.MenuItem_openZBatchSearchForm_Click(this, null);	// 打开一个ZBatchSearchForm检索窗
                else if (strType == "dp2Catalog.AmazonSearchForm")
                    this.MenuItem_openAmazonSearchForm_Click(this, null);	// 打开一个AmaxonSearchForm检索窗
                else if (strType == "dp2Catalog.SruSearchForm")
                    this.MenuItem_openSruSearchForm_Click(this, null);	// 打开一个AmaxonSearchForm检索窗
                else
                    continue;

                // this.AppInfo.FirstMdiOpened = true; // 避免出现打开成Minimized状态的窗口
            }

            // 广告窗口
            MenuItem_openAdvertiseForm_Click(this, null);

            // 装载 MDI 子窗口状态
            this.AppInfo.LoadFormMdiChildStates(this,
                "mainformstate");
        }

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

        void Servers_ServerChanged(object sender, dp2ServerChangedEventArgs e)
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child is dp2SearchForm)
                {
                    dp2SearchForm searchform = (dp2SearchForm)child;
                    searchform.RefreshResTree();
                }

            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 在前面关闭MDI子窗口的时候已经遇到了终止关闭的情况，这里就不用再次询问了
            if (e.CloseReason == CloseReason.UserClosing && e.Cancel == true)
                return;

            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                // 警告关闭
                DialogResult result = MessageBox.Show(this,
                    "确实要退出 dp2Catalog -- dp2编目前端 ? ",
                    "dp2Catalog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this._channelPool != null)
            {
                this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channels_BeforeLogin);
                this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channels_AfterLogin);
                this._channelPool.Close();
            }

            this.Servers.ServerChanged -= new dp2ServerChangedEventHandle(Servers_ServerChanged);

            // 保存到文件
            // parameters:
            //		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
            Servers.Save(null);
            Servers = null;


            // 保存窗口尺寸状态
            if (AppInfo != null)
            {
                this.AppInfo.SetString(
                    "main_form",
                    "last_saved_path",
                    this.LastSavePath);

                string strOpenedMdiWindow = GuiUtil.GetOpenedMdiWindowString(this);
                this.AppInfo.SetString(
                    "main_form",
                    "last_opened_mdi_window",
                    strOpenedMdiWindow);

                /*
                // 存在Z3950 Search窗口
                this.AppInfo.SetInt(
                    "main_form",
                    "last_zsearch_window",
                    this.TopZSearchForm != null ? 1 : 0);

                // 存在dp2 Search窗口
                this.AppInfo.SetInt(
                    "main_form",
                    "last_dp2_search_window",
                    this.TopDp2SearchForm != null ? 1 : 0);

                // 存在DTLP Search窗口
                this.AppInfo.SetInt(
                    "main_form",
                    "last_dtlp_search_window",
                    this.TopDtlpSearchForm != null ? 1 : 0);
                 * */

                FinishFixedPanel();

                AppInfo.SaveFormStates(this,
                    "mainformstate");

#if GCAT_SERVER
                if (this.m_bSavePinyinGcatID == false)
                    this.m_strPinyinGcatID = "";
                this.AppInfo.SetString("entity_form", "gcat_pinyin_api_id", this.m_strPinyinGcatID);
                this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", this.m_bSavePinyinGcatID);
#endif
            }

            // cfgcache
            string strError;
            int nRet = cfgCache.Save(null, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            //记住save,保存信息XML文件
            AppInfo.Save();
            AppInfo = null;	// 避免后面再用这个对象		
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
                this.panel_fixed.Visible = false;
                this.splitter_fixed.Visible = false;
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

        /*
        // 看看一个窗口句柄是不是MDI子窗口的句柄？
        // 如果是，则返回该MDI子窗口的Form对象
        Form IsMdiChildren(IntPtr hwnd)
        {
            for (int i = 0; i < this.MdiChildren.Length; i++)
            {
                if (hwnd == this.MdiChildren[i].Handle)
                {
                    return this.MdiChildren[i];
                }
            }
            return null;    // not found
        }

        // 创建一个表示当前打开的MDI子窗口的字符串
        string GetOpenedMdiWindowString()
        {
            if (this.ActiveMdiChild == null)
                return null;

            // 得到顶层的MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return null;

            // 找到最底部的子窗口
            IntPtr hwndFirst = API.GetWindow(hwnd, API.GW_HWNDLAST);

            // 顺次得到字符串
            string strResult = "";
            hwnd = hwndFirst;
            for (; ; )
            {
                if (hwnd == IntPtr.Zero)
                    break;

                Form temp = IsMdiChildren(hwnd);
                if (temp != null)
                {
                    // 最小化的被忽略
                    if (temp.WindowState != FormWindowState.Minimized)
                        strResult += temp.GetType().ToString() + ",";
                }

                hwnd = API.GetWindow(hwnd, API.GW_HWNDPREV);
            }

            return strResult;
        }
         * */

        public void SetMenuItemState()
        {
            // 菜单
            this.MenuItem_saveOriginRecordToIso2709.Enabled = false;
            this.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            this.MenuItem_font.Enabled = false;
            this.MenuItem_saveToTemplate.Enabled = false;
            this.MenuItem_viewAccessPoint.Enabled = false;

            /*
            // 工具条按钮
            this.ToolStripMenuItem_loadReaderInfo.Enabled = true;
            this.ToolStripMenuItem_loadReaderInfo.Enabled = true;
             * */
            this.toolButton_nextBatch.Enabled = false;
            this.toolButton_prev.Enabled = false;
            this.toolButton_next.Enabled = false;

            this.toolButton_getAllRecords.Enabled = false;
            this.toolButton_saveTo.Enabled = false;
            this.toolButton_saveToDB.Enabled = false;
            this.toolButton_save.Enabled = false;
            this.toolButton_delete.Enabled = false;

            this.toolButton_refresh.Enabled = false;
            this.toolButton_loadTemplate.Enabled = false;

            this.toolButton_dup.Enabled = false;
            this.toolButton_verify.Enabled = false;
            this.toolButton_loadFullRecord.Enabled = false;
        }

        // 装载检索途径信息
        int LoadFroms(string strFileName,
            out string strError)
        {
            strError = "";
            this.Froms = new FromCollection();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载文件 " + strFileName + " 到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                Bib1Use from = new Bib1Use();
                from.Name = DomUtil.GetAttr(node, "name");
                from.UniName = DomUtil.GetAttr(node, "uni_name");
                from.Value = DomUtil.GetAttr(node, "value");
                from.Comment = DomUtil.GetAttr(node, "comment");

                this.Froms.Add(from);
            }

            return 0;
        }

        // 获得检索途径列表
        public string[] GetFromList()
        {
            string[] result = new string[this.Froms.Count];

            for (int i = 0; i < this.Froms.Count; i++)
            {
                Bib1Use from = this.Froms[i];
                result[i] = from.Name + " - " + from.Comment;
            }

            return result;
        }

        // 保存分割条位置
        public void SaveSplitterPos(SplitContainer container,
            string strSection,
            string strEntry)
        {
            if (this.AppInfo == null)
                return;

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
        public void LoadSplitterPos(SplitContainer container,
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

        #region 按钮事件

        // 检索
        private void toolButton_search_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm zsearchform = (ZSearchForm)this.ActiveMdiChild;
                    // zsearchform.DoSearchOneServer();
                    zsearchform.DoSearch();
                }
                else if (this.ActiveMdiChild is DtlpSearchForm)
                {
                    DtlpSearchForm dtlpsearchform = (DtlpSearchForm)this.ActiveMdiChild;
                    dtlpsearchform.DoSearch();
                }
                else if (this.ActiveMdiChild is dp2SearchForm)
                {
                    dp2SearchForm dp2searchform = (dp2SearchForm)this.ActiveMdiChild;
                    dp2searchform.DoSearch();
                }
                else if (this.ActiveMdiChild is dp2DupForm)
                {
                    dp2DupForm dupform = (dp2DupForm)this.ActiveMdiChild;
                    dupform.DoSearchDup();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    AmazonSearchForm form = (AmazonSearchForm)this.ActiveMdiChild;
                    form.DoSearch();
                }

            }
            finally
            {
                save.RestoreAll();
            }


        }

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            stopManager.DoStopActive();
        }

        private void toolButton_prev_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("prev");
                    else
                        detail.LoadRecord("prev");
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("prev");
                    else
                        detail.LoadRecord("prev");
                }
                else if (this.ActiveMdiChild is XmlDetailForm)
                {
                    XmlDetailForm detail = (XmlDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        // detail.LoadRecordByPath("next");
                        detail.LoadRecord("prev");
                    }
                    else
                        detail.LoadRecord("prev");
                }
            }
            finally
            {
                save.RestoreAll();
            }

        }

        private void toolButton_next_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("next");
                    else
                        detail.LoadRecord("next");
                }
                else if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("next");
                    else
                        detail.LoadRecord("next");
                }
                else if (this.ActiveMdiChild is XmlDetailForm)
                {
                    XmlDetailForm detail = (XmlDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        // detail.LoadRecordByPath("next");
                        detail.LoadRecord("next");
                    }
                    else
                        detail.LoadRecord("next");
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // 重新获得当前记录。似应叫reload更好
        private void toolButton_refresh_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.Reload();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.Reload();
                }
                if (this.ActiveMdiChild is dp2SearchForm)
                {
                    dp2SearchForm detail = (dp2SearchForm)this.ActiveMdiChild;
                    detail.Reload();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_loadFullRecord_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.LoadRecord("current", true);
                }
                else if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                    search.ReloadFullElementSet();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    var search = this.ActiveMdiChild as AmazonSearchForm;
                    search.ReloadFullElementSet();
                }
                else if (this.ActiveMdiChild is XmlDetailForm)
                {
                    var detail = this.ActiveMdiChild as XmlDetailForm;
                    detail.LoadRecord("current", true);
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_nextBatch_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;

                    search.NextBatch();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    AmazonSearchForm search = (AmazonSearchForm)this.ActiveMdiChild;

                    search.NextBatch();
                }
                else if (this.ActiveMdiChild is SruSearchForm)
                {
                    var search = (SruSearchForm)this.ActiveMdiChild;

                    search.NextBatch();
                }
            }
            finally
            {
                save.RestoreAll();
            }

        }

        private void toolButton_getAllRecords_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                    search.GetAllRecords();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    AmazonSearchForm search = (AmazonSearchForm)this.ActiveMdiChild;
                    search.GetAllRecords();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_saveTo_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                MenuItem_saveOriginRecordToIso2709_Click(this, null);
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_save_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.SaveRecord();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.SaveRecord();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // 另存到数据库
        private void toolButton_saveToDB_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.SaveRecord("saveas");
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.SaveRecord();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // 装载记录模板
        private void toolButton_loadTemplate_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.LoadTemplate();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.LoadTemplate();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // 保存到记录模板
        private void MenuItem_saveToTemplate_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.SaveToTemplate();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.SaveToTemplate();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void MenuItem_viewAccessPoint_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.GetAccessPoint();
                }

            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_delete_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.DeleteRecord();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.DeleteRecord();
                }
                if (this.ActiveMdiChild is dp2SearchForm)
                {
                    dp2SearchForm detail = (dp2SearchForm)this.ActiveMdiChild;
                    detail.DeleteSelectedRecords();
                }
            }
            finally
            {
                save.RestoreAll();
            }

        }

        #endregion


        // 当前顶层的SearchForm
        public ZSearchForm TopZSearchForm
        {
            get
            {
                return (ZSearchForm)GetTopChildWindow(typeof(ZSearchForm));
            }
        }

        // 当前顶层的DtlpSearchForm
        public DtlpSearchForm TopDtlpSearchForm
        {
            get
            {
                return (DtlpSearchForm)GetTopChildWindow(typeof(DtlpSearchForm));
            }

        }

        // 当前顶层的 dp2SearchForm
        public dp2SearchForm TopDp2SearchForm
        {
            get
            {
                return (dp2SearchForm)GetTopChildWindow(typeof(dp2SearchForm));
            }
        }

        // 当前顶层的 AmazonSearchForm
        public AmazonSearchForm TopAmazonSearchForm
        {
            get
            {
                return (AmazonSearchForm)GetTopChildWindow(typeof(AmazonSearchForm));
            }
        }

        // 当前顶层的MarcDetailForm
        public MarcDetailForm TopMarcDetailForm
        {
            get
            {
                return (MarcDetailForm)GetTopChildWindow(typeof(MarcDetailForm));
            }
        }

        public void SetMdiToNormal()
        {
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild.WindowState != FormWindowState.Normal)
                    this.ActiveMdiChild.WindowState = FormWindowState.Normal;
            }
        }

        MdiClient GetMdiClient()
        {
            Type t = typeof(Form);
            PropertyInfo pi = t.GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic);
            return (MdiClient)pi.GetValue(this, null);
        }

        // 设置窗口到左侧固定位置
        public void SetFixedPosition(Form form, string strStyle)
        {
            MdiClient client = GetMdiClient();
            if (strStyle == "left")
            {
                form.Location = new Point(0, 0);
                form.Size = new Size((client.ClientSize.Width / 2) - 1, client.ClientSize.Height - 1);
            }
            else
            {
                form.Location = new Point(client.ClientSize.Width / 2, 0);
                form.Size = new Size((client.ClientSize.Width / 2) - 1, client.ClientSize.Height - 1);
            }
        }

        public MarcDetailForm FixedMarcDetailForm
        {
            get
            {
                foreach (Form form in this.MdiChildren)
                {
                    if (form is MarcDetailForm)
                    {
                        MarcDetailForm detail = form as MarcDetailForm;
                        if (detail.Fixed)
                            return detail;
                    }
                }
                return null;
            }
        }

        // 当前顶层的DcForm
        public DcForm TopDcForm
        {
            get
            {
                return (DcForm)GetTopChildWindow(typeof(DcForm));
            }
        }

        // 得到特定类型的顶层MDI窗口。注，会忽略 fixed 类型的窗口
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
                foreach (Form form in this.MdiChildren)
                {
                    if (hwnd == form.Handle)
                    {
                        child = form;
                        goto FOUND;
                    }
                }

                goto CONTINUE;
            FOUND:

                if (child.GetType().Equals(type) == true)
                {
                    if (child is MyForm)
                    {
                        if (((MyForm)child).Fixed)
                            goto CONTINUE;
                    }
                    return child;
                }

            CONTINUE:
                hwnd = API.GetWindow(hwnd, API.GW_HWNDNEXT);
            }

            return null;
        }

        #region 菜单事件

        // 打开Z39.50检索窗
        private void MenuItem_openZSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            ZSearchForm form = new ZSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ZSearchForm>();
        }

        T OpenWindow<T>()
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                T form = Activator.CreateInstance<T>();
                dynamic o = form;
                o.MdiParent = this;

                try
                {
                    // 2018/6/24 MainForm 成员可能不存在，可能会抛出异常
                    if (o.MainForm == null)
                        o.MainForm = this;
                }
                catch
                {
                    // 等将来所有窗口类型的 MainForm 都是只读的以后，再修改这里
                }
                o.Show();
                return form;
            }
            else
                return EnsureChildForm<T>(true);
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

                try
                {
                    // 2013/3/26
                    // 2018/6/24 MainForm 成员可能不存在，可能会抛出异常
                    if (o.MainForm == null)
                        o.MainForm = this;
                }
                catch
                {
                    // 等将来所有窗口类型的 MainForm 都是只读的以后，再修改这里
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

        /// <summary>
        /// 得到特定类型的顶层 MDI 子窗口
        /// 注：不算 Fixed 窗口
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
                if (child != null && IsFixedMyForm(child) == false)  // 2016/12/16 跳过固定于左侧的 MyForm
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

        // 是否为固定于左侧的 MyForm?
        static bool IsFixedMyForm(Form child)
        {
            if (child is MyForm)
            {
                if (((MyForm)child).Fixed)
                    return true;
            }

            return false;
        }

        // 判断一个窗口句柄，是否为 MDI 子窗口？
        // 注：不处理 Visible == false 的窗口。因为取 Handle 会导致 Visible 变成 true
        // return:
        //      null    不是 MDI 子窗口o
        //      其他      返回这个句柄对应的 Form 对象
        Form IsChildHwnd(IntPtr hwnd)
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child.Visible == true && hwnd == child.Handle)
                    return child;
            }

            return null;
        }


        // 打开MARC记录窗
        private void MenuItem_openMarcDetailForm_Click(object sender, EventArgs e)
        {
#if NO
            MarcDetailForm form = new MarcDetailForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<MarcDetailForm>();
        }

        // 带有模板
        private void MenuItem_openMarcDetailFormEx_Click(object sender, EventArgs e)
        {
#if NO
            MarcDetailForm form = new MarcDetailForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
            form.LoadTemplate();
#endif
            MarcDetailForm form = OpenWindow<MarcDetailForm>();
            form.LoadTemplate();
        }

        // 打开XML记录窗
        private void MenuItem_loadXmlDetailForm_Click(object sender, EventArgs e)
        {
#if NO
            XmlDetailForm form = new XmlDetailForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<XmlDetailForm>();
        }

        private void MenuItem_openBerDebugForm_Click(object sender, EventArgs e)
        {
#if NO
            BerDebugForm form = new BerDebugForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<BerDebugForm>();
        }

        private void MenuItem_saveOriginRecordToWorksheet_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                detail.SaveRecordToWorksheet();
            }
            else if (this.ActiveMdiChild is ZSearchForm)
            {
                ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                search.menuItem_saveOriginRecordToWorksheet_Click(this, null);
            }
            else if (this.ActiveMdiChild is dp2SearchForm)
            {
                dp2SearchForm search = (dp2SearchForm)this.ActiveMdiChild;
                search.SaveOriginRecordToWorksheet();
            }
        }

        private void MenuItem_saveOriginRecordToIso2709_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                detail.SaveOriginRecordToIso2709();
            }
            else if (this.ActiveMdiChild is ZSearchForm)
            {
                ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                search.menuItem_saveOriginRecordToIso2709_Click(this, null);
            }
            else if (this.ActiveMdiChild is dp2SearchForm)
            {
                dp2SearchForm search = (dp2SearchForm)this.ActiveMdiChild;
                search.SaveOriginRecordToIso2709();
            }

        }

        private void MenuItem_openZhongcihaoForm_Click(object sender, EventArgs e)
        {
#if NO
            ZhongcihaoForm form = new ZhongcihaoForm();

            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ZhongcihaoForm>();
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
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

        // 版权
        private void MenuItem_copyright_Click(object sender, EventArgs e)
        {
            // throw new Exception("test throw exception");

            CopyrightDlg dlg = new CopyrightDlg();
            GuiUtil.SetControlFont(dlg, this.DefaultFont);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

        }

        // Dtlp协议检索窗
        private void MenuItem_openDtlpSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            DtlpSearchForm form = new DtlpSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<DtlpSearchForm>();
        }

        // dp2library协议检索窗
        private void MenuItem_openDp2SearchForm_Click(object sender, EventArgs e)
        {
#if NO
            if (this.TestMode == true)
            {
                MessageBox.Show(this, "dp2 检索窗需要先设置序列号(正式模式)才能使用");
                return;
            }
#endif


#if SN
            // 检查序列号
            // DateTime start_day = new DateTime(2014, 11, 15);    // 2014/11/15 以后强制启用序列号功能
            // if (DateTime.Now >= start_day || this.IsExistsSerialNumberStatusFile() == true)
            {
                // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                this.WriteSerialNumberStatusFile();

                string strError = "";
                int nRet = this.VerifySerialCode("dp2 检索窗需要先设置序列号才能使用",
                    "",
                    false,
                    out strError);
                if (nRet == -1)
                {
#if NO
                    MessageBox.Show(this, "dp2 检索窗需要先设置序列号才能使用");
                    return;
#endif
                }
            }

#endif

#if NO
            dp2SearchForm form = new dp2SearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<dp2SearchForm>();
        }

        // OAI-PMH协议检索窗
        private void MenuItem_openOaiSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            OaiSearchForm form = new OaiSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<OaiSearchForm>();
        }


        private void MenuItem_cfg_Click(object sender, EventArgs e)
        {
            string strOldDefaultFontString = this.DefaultFontString;

            SystemCfgForm dlg = new SystemCfgForm();
            GuiUtil.SetControlFont(dlg, this.DefaultFont);

            dlg.ParamChanged += new ParamChangedEventHandler(CfgDlg_ParamChanged);
            dlg.MainForm = this;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.AppInfo.LinkFormState(dlg,
                "systemcfg_dialog_state");

            dlg.ShowDialog(this);

            this.AppInfo.UnlinkFormState(dlg);
            dlg.ParamChanged -= new ParamChangedEventHandler(CfgDlg_ParamChanged);

            // 缺省字体发生了变化
            if (strOldDefaultFontString != this.DefaultFontString)
            {
                Size oldsize = this.Size;

                GuiUtil.SetControlFont(this, this.DefaultFont, true);

                foreach (Form child in this.MdiChildren)
                {
                    oldsize = child.Size;

                    GuiUtil.SetControlFont(child, this.DefaultFont, true);
                }
            }

            // 亚马逊 缺省服务器变化
            {
                // 遍历当前打开的所有AmazonSearchForm
                List<Form> forms = GetChildWindows(typeof(AmazonSearchForm));
                foreach (Form child in forms)
                {
                    AmazonSearchForm form = (AmazonSearchForm)child;
                    // 让按钮文字显示出来
                    form.RefreshUI();
                }
            }
        }

        void CfgDlg_ParamChanged(object sender, ParamChangedEventArgs e)
        {
            if (e.Section == "dp2searchform"
                && e.Entry == "layout")
            {
                // 遍历当前打开的所有dp2SearchForm
                List<Form> forms = GetChildWindows(typeof(dp2SearchForm));
                foreach (Form child in forms)
                {
                    dp2SearchForm form = (dp2SearchForm)child;

                    if (form.SetLayout((string)e.Value) == true)
                        form.AppInfo_LoadMdiLayout(form, null);
                }
            }

        }

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

        private void MenuItem_openDtlpLogForm_Click(object sender, EventArgs e)
        {
#if NO
            DtlpLogForm form = new DtlpLogForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<DtlpLogForm>();
        }

        private void MenuItem_openEaccForm_Click(object sender, EventArgs e)
        {
#if NO
            EaccForm form = new EaccForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<EaccForm>();
        }

        // 打开DC记录窗
        private void MenuItem_openDcForm_Click(object sender, EventArgs e)
        {
#if NO
            DcForm form = new DcForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<DcForm>();
        }

        // 打开DC记录窗 带模板
        private void MenuItem_openDcFormEx_Click(object sender, EventArgs e)
        {
#if NO
            DcForm form = new DcForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
            form.LoadTemplate();
#endif
            DcForm form = OpenWindow<DcForm>();
            form.LoadTemplate();
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
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_font_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;

                detail.SetFont();
            }
            if (this.ActiveMdiChild is DcForm)
            {
                DcForm detail = (DcForm)this.ActiveMdiChild;

                detail.SetFont();
            }

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

        #endregion


        public ToolStripProgressBar ToolStripProgressBar
        {
            get
            {
                return this.toolStripProgressBar_main;
            }
        }

        // 禁止除了停止按钮以外的其他按钮。
        // 如果要恢复，用EanbleStateCollection的RestoreAll()方法。
        public EnableStateCollection DisableToolButtons()
        {
            // 保存原来的状态
            EnableStateCollection results = new EnableStateCollection();

            results.Push(this.toolButton_getAllRecords);

            results.Push(this.toolButton_next);
            results.Push(this.toolButton_nextBatch);
            results.Push(this.toolButton_prev);
            results.Push(this.toolButton_save);
            results.Push(this.toolButton_saveTo);
            results.Push(this.toolButton_saveToDB);
            results.Push(this.toolButton_search);
            results.Push(this.toolButton_dup);
            results.Push(this.toolButton_verify);

            // this.toolButton_stop
            return results;
        }

        // 获得Encoding对象。本函数支持MARC-8编码名
        // return:
        //      -1  error
        //      0   succeed
        public int GetEncoding(string strName,
            out Encoding encoding,
            out string strError)
        {
            strError = "";
            encoding = null;

            try
            {

                if (StringUtil.IsNumber(strName) == true)
                {
                    try
                    {
                        Int32 nCodePage = Convert.ToInt32(strName);
                        encoding = Encoding.GetEncoding(nCodePage);
                    }
                    catch (Exception ex)
                    {
                        strError = "构造编码方式过程出错: " + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    if (strName.ToLower() == "eacc"
                        || strName.ToLower() == "marc-8")
                        encoding = this.Marc8Encoding;
                    else
                        encoding = Encoding.GetEncoding(strName);
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }


        public string MessageText
        {
            get
            {
                return this.toolStripStatusLabel_main.Text;
            }
            set
            {
                this.toolStripStatusLabel_main.Text = value;
            }
        }

        // 服务器名和缺省帐户管理
        public void ManageServers(bool bFirstRun)
        {
            ServersDlg dlg = new ServersDlg();
            GuiUtil.SetControlFont(dlg, this.DefaultFont);

            dp2ServerCollectionNew newServers = Servers.Dup();

            if (bFirstRun == true)
            {
                dlg.Text = "首次运行: 创建 dp2library 服务器目标";
                dlg.FirstRun = true;
            }
            dlg.Servers = newServers;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // this.Servers = newServers;
            this.Servers.Import(newServers);
        }

        // 下载数据文件
        public int DownloadDataFile(string strFileName,
            out string strError)
        {
            strError = "";

            string strUrl = "http://dp2003.com/dp2Catalog/" + strFileName;
            string strLocalFileName = Path.Combine(this.DataDir, strFileName);
            string strTempFileName = Path.Combine(this.DataDir, "~temp_download_webfile");

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

        // 2015/5/8
        // 下载用户文件
        public int DownloadUserFile(string strFileName,
            out string strError)
        {
            strError = "";

            string strUrl = "http://dp2003.com/dp2Catalog/" + strFileName;
            string strLocalFileName = Path.Combine(this.UserDir, strFileName);
            string strTempFileName = Path.Combine(this.UserDir, "~temp_download_webfile");

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
                this.QuickSjhm = new QuickSjhm(Path.Combine(this.DataDir, "sjhm.xml"));
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
                this.QuickPinyin = new QuickPinyin(Path.Combine(this.DataDir, "pinyin.xml"));
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

        public int LoadIsbnSplitter(bool bAutoDownload,
    out string strError)
        {
            strError = "";

            // 优化
            if (this.IsbnSplitter != null)
                return 0;

            string strFileName = Path.Combine(this.DataDir, "rangemessage.xml");

        REDO:

            try
            {
                this.IsbnSplitter = new IsbnSplitter(strFileName);  // "\\isbn.xml"
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地 isbn 规则文件 " + strFileName + " 发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile(Path.GetFileName(strFileName),    // "isbn.xml"
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
                strError = "装载本地 isbn 规则文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }

        // (C#脚本使用)
        // 从ISBN号中取得出版社号部分
        // 本函数可以自动适应有978前缀的新型ISBN号
        // ISBN号中无横杠时本函数会自动先加横杠然后再取得出版社号
        // parameters:
        //      strPublisherNumber  出版社号码。不包含978-部分
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

        // 查重
        private void toolButton_dup_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm marcform = (MarcDetailForm)this.ActiveMdiChild;
                    marcform.SearchDup("toolbar");
                }
                else if (this.ActiveMdiChild is DcForm)
                {
                    DcForm dcform = (DcForm)this.ActiveMdiChild;
                    dcform.SearchDup("toolbar");
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }


        private void toolButton_verify_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm marcform = (MarcDetailForm)this.ActiveMdiChild;
                marcform.VerifyData();
            }
            else if (this.ActiveMdiChild is DcForm)
            {
                DcForm dcform = (DcForm)this.ActiveMdiChild;
                // dcform.VerifyData();
            }
        }


        // 连接MARC文件
        private void MenuItem_linkMarcFile_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                // 如果当前窗口为MARC记录窗，则直接利用之
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                detail.LinkMarcFile();
            }
            else
            {
                // 如果当前窗口不是MARC记录窗，则新开一个
                MarcDetailForm form = new MarcDetailForm();

                form.MdiParent = this;

                form.MainForm = this;
                form.Show();

                form.LinkMarcFile();
            }
        }

        // 打开修改密码窗
        private void MenuItem_changePassword_Click(object sender, EventArgs e)
        {
#if NO
            ChangePasswordForm form = new ChangePasswordForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ChangePasswordForm>();
        }

        // 缺省的MDI子窗口宽度
        public static int DefaultMdiWindowWidth
        {
            get
            {
                return (int)((double)SystemInformation.WorkingArea.Width * 0.8);
            }
        }

        // 缺省的MDI子窗口高度
        public static int DefaultMdiWindowHeight
        {
            get
            {
                return (int)((double)SystemInformation.WorkingArea.Height * 0.8);
            }
        }

        // 清除配置文件本地缓存
        private void MenuItem_clearCfgCache_Click(object sender, EventArgs e)
        {
            cfgCache.ClearCfgCache();

            this.AssemblyCache.Clear(); // 顺便也清除Assembly缓存
            this.DomCache.Clear();
        }

        public AmazonSearchForm GetAmazonSearchForm()
        {
            AmazonSearchForm searchform = null;

            searchform = this.TopAmazonSearchForm;

            if (searchform == null)
            {
                // 新开一个 Amazon 检索窗
                FormWindowState old_state = this.WindowState;

                searchform = new AmazonSearchForm();
                searchform.MainForm = this;
                searchform.MdiParent = this;
                searchform.WindowState = FormWindowState.Minimized;
                searchform.Show();

                // 2008/3/17 
                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                // searchform.WaitLoadFinish();
            }

            return searchform;
        }

        public ZSearchForm GetZSearchForm()
        {
            ZSearchForm searchform = null;

            searchform = this.TopZSearchForm;

            if (searchform == null)
            {
                // 新开一个dp2检索窗
                // FormWindowState old_state = this.WindowState;

                searchform = new ZSearchForm();
                searchform.MainForm = this;
                searchform.MdiParent = this;
                // searchform.WindowState = FormWindowState.Minimized;
                searchform.Show();

                // this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                // searchform.WaitLoadFinish();
            }

            return searchform;
        }

        public dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            dp2_searchform = this.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // 新开一个dp2检索窗
                FormWindowState old_state = this.WindowState;

                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this;
                dp2_searchform.MdiParent = this;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();

                // 2008/3/17 
                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dp2_searchform.WaitLoadFinish();
            }

            return dp2_searchform;
        }

        public DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = this.TopDtlpSearchForm;

            dtlp_searchform = this.TopDtlpSearchForm;

            if (dtlp_searchform == null)
            {
                // 新开一个dtlp检索窗
                FormWindowState old_state = this.WindowState;

                dtlp_searchform = new DtlpSearchForm();
                dtlp_searchform.MainForm = this;
                dtlp_searchform.MdiParent = this;
                dtlp_searchform.WindowState = FormWindowState.Minimized;
                dtlp_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dtlp_searchform.WaitLoadFinish();

            }

            return dtlp_searchform;
        }

        public string DefaultSearchDupStartPath
        {
            get
            {
                // 备选的起点路径
                return this.AppInfo.GetString(
                    "searchdup",
                    "defaultStartPath",
                    "");
            }
            set
            {
                this.AppInfo.SetString(
                    "searchdup",
                    "defaultStartPath",
                    value);
            }
        }

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

        private void toolStripButton_testSearch_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm zsearchform = (ZSearchForm)this.ActiveMdiChild;
                    zsearchform.DoTestSearch();
                }
                else if (this.ActiveMdiChild is DtlpSearchForm)
                {

                }
                else if (this.ActiveMdiChild is dp2SearchForm)
                {
                }
                else if (this.ActiveMdiChild is dp2DupForm)
                {
                }

            }
            finally
            {
                save.RestoreAll();
            }
        }

        // 当前是否具备显示item property的条件
        public bool CanDisplayItemProperty()
        {
            if (this.PanelFixedVisible == false)
                return false;
            if (this.tabControl_panelFixed.SelectedTab != this.tabPage_property)
                return false;

            return true;
        }

        public void ActivatePropertyPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
        }

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
#if NO
                while (this.tabPage_property.Controls.Count > 0)
                {
                    this.tabPage_property.Controls.RemoveAt(0); 可能造成资源泄露!
                }
#endif
                ControlExtention.ClearControls(this.tabPage_property);

                if (value != null)
                {
                    this.tabPage_property.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
                }
            }
        }

        public void ActivateVerifyResultPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
        }

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
#if NO
                while (this.tabPage_verifyResult.Controls.Count > 0)
                    this.tabPage_verifyResult.Controls.RemoveAt(0);
#endif
                ControlExtention.ClearControls(this.tabPage_verifyResult);

                if (value != null)
                {
                    this.tabPage_verifyResult.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
                }
            }
        }

        public void ActivateGenerateDataPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;
        }

        // 
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
#if NO
                while (this.tabPage_generateData.Controls.Count > 0)
                    this.tabPage_generateData.Controls.RemoveAt(0);
#endif
                ControlExtention.ClearControls(this.tabPage_generateData);

                if (value != null)
                {
                    this.tabPage_generateData.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;
                }
            }
        }

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
            }
        }

        private void toolStripButton_close_Click(object sender, EventArgs e)
        {
            this.PanelFixedVisible = false;
        }

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

#if NO
        // parameters:
        //      bForce  是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置
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
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    sub.Font = new Font(font, subfont.Style);
                }

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
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    item.Font = new Font(font, subfont.Style);
                }
            }
        }
#endif

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

        // GCAT通用汉语著者号码表 WebService URL
        public string GcatServerUrl
        {
            get
            {
                return this.AppInfo.GetString("config",
                    "gcat_server_url",
                    "http://dp2003.com/dp2library/");
            }
        }

        public string PinyinServerUrl
        {
            get
            {
                return this.AppInfo.GetString("config",
                    "pinyin_server_url",
                    "http://dp2003.com/dp2library/");
            }
        }

        // 是否要临时改用本地拼音。此状态退出时不会被记忆
        public bool ForceUseLocalPinyinFunc = false;

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

        private void MenuItem_openZBatchSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            ZBatchSearchForm form = new ZBatchSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ZBatchSearchForm>();
        }

        private void MenuItem_openAmazonSearchForm_Click(object sender, EventArgs e)
        {
#if NO
            AmazonSearchForm form = new AmazonSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<AmazonSearchForm>();
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // wparam == 1，表示进行全功能的初始化；否则仅仅是初始化名字部分
                case WM_PREPARE:
                    {
                        int nRet = 0;

                        // 先禁止界面
                        if (m.WParam.ToInt32() == 1)
                            EnableControls(false);

                        try
                        {
                            string strError = "";

#if NO
                            // 检查序列号。这里的暂时不要求各种产品功能
                            DateTime start_day = new DateTime(2014, 11, 15);    // 2014/11/15 以后强制启用序列号功能
                            if (DateTime.Now >= start_day || IsExistsSerialNumberStatusFile() == true)
                            {
                                // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                                WriteSerialNumberStatusFile();

                                nRet = this.VerifySerialCode("", out strError);
                                if (nRet == -1)
                                {
                                    MessageBox.Show(this, "dp2Catalog 需要先设置序列号才能使用");
                                    Application.Exit();
                                    return;
                                }
                            }
#endif

                            Stop = new DigitalPlatform.Stop();
                            Stop.Register(stopManager, true);	// 和容器关联
                            Stop.SetMessage("正在删除以前遗留的临时文件...");

                            DeleteAllTempFiles(this.DataDir);

                            Stop.SetMessage("");
                            if (Stop != null) // 脱离关联
                            {
                                Stop.Unregister();	// 和容器关联
                                Stop = null;
                            }

                            // 初始化历史对象，包括C#脚本
                            if (this.OperHistory == null)
                            {
                                this.OperHistory = new OperHistory();
                                nRet = this.OperHistory.Initial(this,
                                    this.webBrowser_history,
                                    out strError);
                                if (nRet == -1)
                                    MessageBox.Show(this, strError);
                            }

                            // 2020/8/21
                            // 后台自动检查更新
                            if (ApplicationDeployment.IsNetworkDeployed)
                            {
                                _ = Task.Run(() =>
                                {
                                    AppendString("开始后台 ClickOnce 升级");
                                    try
                                    {
                                        var result = ClientInfo.InstallUpdateSync();
                                        if (result.Value == -1)
                                            AppendString($"ClickOnce 后台自动更新出错: {result.ErrorInfo}\r\n");
                                        else if (result.Value == 1)
                                            AppendString($"ClickOnce 后台自动更新: {result.ErrorInfo}\r\n");
                                        else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                                            AppendString($"ClickOnce 后台自动更新: {result.ErrorInfo}\r\n");
                                    }
                                    catch(Exception ex)
                                    {
                                        AppendString($"后台自动升级出现异常: {ExceptionUtil.GetDebugText(ex)}");
                                    }
                                    finally
                                    {
                                        AppendString("结束后台 ClickOnce 升级");
                                    }
                                });
                            }
                        }
                        finally
                        {
                            // 然后许可界面
                            if (m.WParam.ToInt32() == 1)
                                EnableControls(true);

                        }

                        if (m.WParam.ToInt32() == 1)
                        {
                            // 恢复上次遗留的窗口
                            string strOpenedMdiWindow = this.AppInfo.GetString(
                                "main_form",
                                "last_opened_mdi_window",
                                "");

                            RestoreLastOpenedMdiWindow(strOpenedMdiWindow);
                        }
                        return;
                    }

                    // break;

            }
            base.DefWndProc(ref m);
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

        DELETE_FILES:
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

        void EnableControls(bool bEnable)
        {
            this.menuStrip_main.Enabled = bEnable;
        }

        private void MenuItem_openZBatchSearchForm1_Click(object sender, EventArgs e)
        {
#if NO
            ZBatchSearchForm form = new ZBatchSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<ZBatchSearchForm>();
        }

        // 把字符串中的汉字转换为四角号码
        // parameters:
        //      bLocal  是否从本地获取四角号码
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
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
                {
                    continue;
                }

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

#if NO
        // 包装后版本
        public int GetPinyin(IWin32Window owner,
    string strHanzi,
    PinyinStyle style,
    bool bAutoSel,
    out string strPinyin,
    out string strError)
        {
            return GetPinyin(owner,
                strHanzi,
                style,
                (bAutoSel ? "auto" : ""),
                out strPinyin,
                out strError);
        }
#endif

        // 汉字字符串转换为拼音
        // 这个函数会按照当前配置，自动决定使用下层的加拼音函数
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        /// <summary>
        /// 汉字字符串转换为拼音
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strHanzi">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strDuoyinStyle">多音字处理风格。为 auto first 的组合，逗号分隔</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int GetPinyin(IWin32Window owner,
            string strHanzi,
            PinyinStyle style,
            string strDuoyinStyle,  // bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                return this.HanziTextToPinyin(
                    owner,
                    true,	// 本地，快速
                    strHanzi,
                    style,
                    strDuoyinStyle,
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
                return this.SmartHanziTextToPinyin(
                    owner,
                    strHanzi,
                    style,
                    strDuoyinStyle,
                    out strPinyin,
                    out strError);
            }
        }

        // 把字符串中的汉字和拼音分离
        // parameters:
        //      bLocal  是否从本地获取拼音
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        public int HanziTextToPinyin(
            IWin32Window owner,
            bool bLocal,
            string strText,
            PinyinStyle style,
            string strDuoyinStyle,  // 2014/10/20
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            bool bAuto = StringUtil.IsInList("auto", strDuoyinStyle);
            bool bFirst = StringUtil.IsInList("first", strDuoyinStyle);

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";

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
                {	// canceld
                    strPinyin += strHanzi;	// 只好将汉字放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// 如果是多个拼音
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    // GuiUtil.SetControlFont(dlg, this.Font);
                    float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    GuiUtil.SetControlFont(dlg, this.Font, false);
                    // 维持字体的原有大小比例关系
                    dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);

                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    if (bFirst == true
    && string.IsNullOrEmpty(dlg.Pinyins) == false)
                    {
                        dlg.ResultPinyin = SelPinyinDlg.GetFirstPinyin(dlg.Pinyins);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                        dlg.ShowDialog(owner);

                        this.AppInfo.UnlinkFormState(dlg);
                    }

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        strPinyin += strHanzi;
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

            return 1;   // 正常结束
        }

#if GCAT_SERVER
        GcatServiceClient m_gcatClient = null;
        string m_strPinyinGcatID = "";
        bool m_bSavePinyinGcatID = false;
#endif

        // 包装后的版本
        public int SmartHanziTextToPinyin(
    IWin32Window owner,
    string strText,
    PinyinStyle style,
    bool bAutoSel,
    out string strPinyin,
    out string strError)
        {
            return SmartHanziTextToPinyin(owner,
                strText,
                style,
               (bAutoSel ? "auto" : ""),
                out strPinyin,
                out strError);
        }

        // 汉字字符串转换为拼音
        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
        /// <summary>
        /// 汉字字符串转换为拼音，智能方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strDuoyinStyle">是否自动选择多音字。auto/first 的一个或者组合。如果为 auto,first 表示优先按照智能拼音选择，没有智能拼音的，选择第一个</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError"></param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int SmartHanziTextToPinyin(
            IWin32Window owner,
            string strText,
            PinyinStyle style,
            string strDuoyinStyle,  // bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            bool bAuto = StringUtil.IsInList("auto", strDuoyinStyle);
            bool bFirst = StringUtil.IsInList("first", strDuoyinStyle);

            bool bNotFoundPinyin = false;   // 是否出现过没有找到拼音、只能把汉字放入结果字符串的情况

#if !GCAT_SERVER
            string strPinyinServerUrl = this.PinyinServerUrl;
            if (string.IsNullOrEmpty(strPinyinServerUrl) == false
                && strPinyinServerUrl.Contains("gcat"))
            {
                strError = "请重新配置拼音服务器 URL。当前的配置 '" + strPinyinServerUrl + "' 已过时。可配置为 http://dp2003.com/dp2library";
                return -1;
            }
            LibraryChannel channel = this.GetChannel(strPinyinServerUrl, "public");
#endif
#if GCAT_SERVER
            Stop new_stop = new DigitalPlatform.Stop();
            new_stop.Register(this.stopManager, true);	// 和容器关联
            new_stop.OnStop += new StopEventHandler(new_stop_OnStop);
            new_stop.Initial("正在获得 '" + strText + "' 的拼音信息 (从服务器 " + this.PinyinServerUrl + ")...");
            new_stop.BeginLoop();

            m_gcatClient = null;
#else
            Stop new_stop = new DigitalPlatform.Stop();
            new_stop.Register(this.stopManager, true);	// 和容器关联
            new_stop.OnStop += new StopEventHandler(this.DoStop);
            new_stop.Initial("正在获得 '" + strText + "' 的拼音信息 (从服务器 " + strPinyinServerUrl + ")...");
            new_stop.BeginLoop();
#endif

            try
            {
#if GCAT_SERVER
                m_gcatClient = GcatNew.CreateChannel(this.PinyinServerUrl);
            REDO_GETPINYIN:
#endif

                int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字
                string strPinyinXml = "";
#if GCAT_SERVER
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
#else
                // return:
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                long lRet = channel.GetPinyin(
                    "pinyin",
                    strText,
                    out strPinyinXml,
                    out strError);
#endif
                if (lRet == -1)
                {
#if GCAT_SERVER
                    if (new_stop != null && new_stop.State != 0)
                        return 0;
#endif

                    DialogResult result = MessageBox.Show(this,
    "从服务器 '" + this.PinyinServerUrl + "' 获取拼音的过程出错:\r\n" + strError + "\r\n\r\n是否要临时改为使用本机加拼音功能? \r\n\r\n(注：临时改用本机拼音的状态在程序退出时不会保留。如果要永久改用本机拼音方式，请使用主菜单的“参数配置”命令，将“服务器”属性页的“拼音服务器URL”内容清空)",
    "MainForm",
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

#if GCAT_SERVER
                if (lRet == -2)
                {
                    IdLoginDialog login_dlg = new IdLoginDialog();
                    GuiUtil.SetControlFont(login_dlg, this.Font);
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
#endif

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
                    int nRet = strWordPinyin.IndexOf(";");
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
                            float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            GuiUtil.SetControlFont(dlg, this.Font, false);
                            // 维持字体的原有大小比例关系
                            dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            // 这个对话框比较特殊 GuiUtil.SetControlFont(dlg, this.Font, false);

                            dlg.Text = "请选择汉字 '" + strHanzi + "' 的拼音 (来自服务器 " + this.PinyinServerUrl + ")";
                            dlg.SampleText = strSampleText;
                            dlg.Offset = nOffs;
                            dlg.Pinyins = strCharPinyins;
                            if (index < pinyin_parts.Length)
                                dlg.ActivePinyin = pinyin_parts[index];
                            dlg.Hanzi = strHanzi;

                            if (bAuto == true
                                && string.IsNullOrEmpty(dlg.ActivePinyin) == false)
                            {
                                dlg.ResultPinyin = dlg.ActivePinyin;
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else if (bFirst == true
                                && string.IsNullOrEmpty(dlg.Pinyins) == false)
                            {
                                dlg.ResultPinyin = SelPinyinDlg.GetFirstPinyin(dlg.Pinyins);
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

                // 2013/9/17
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
#if GCAT_SERVER
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
#else
                    // return:
                    //      -1  出错
                    //      0   成功
                    lRet = channel.SetPinyin(
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (lRet == -1)
                        return -1;
#endif
                }
#endif

                if (bNotFoundPinyin == false)
                    return 1;   // 正常结束

                return 2;   // 结果字符串中有没有找到拼音的汉字
            }
            finally
            {
#if !GCAT_SERVER
                this.ReturnChannel(channel);

                new_stop.EndLoop();
                new_stop.OnStop += new StopEventHandler(this.DoStop);
                new_stop.Initial("");
                new_stop.Unregister();
#endif

#if GCAT_SERVER
                new_stop.EndLoop();
                new_stop.OnStop -= new StopEventHandler(new_stop_OnStop);
                new_stop.Initial("");
                new_stop.Unregister();
                if (m_gcatClient != null)
                {
                    m_gcatClient.Close();
                    m_gcatClient = null;
                }
#endif
            }
        }

#if GCAT_SERVER
        void new_stop_OnStop(object sender, StopEventArgs e)
        {
            if (this.m_gcatClient != null)
            {
                this.m_gcatClient.Abort();
            }
        }
#endif

        // parameters:
        //      strIndicator    字段指示符。如果用null调用，则表示不对指示符进行筛选
        // return:
        //      0   没有找到匹配的配置事项
        //      >=1 找到。返回找到的配置事项个数
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

        // 为一条 MARC 记录加拼音
        // return:
        //      -1  出错。包括中断的情况
        //      0   正常
        /// <summary>
        /// 为 MarcRecord 对象内的记录加拼音
        /// </summary>
        /// <param name="record">MARC 记录对象</param>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="style">风格</param>
        /// <param name="strPrefix">前缀字符串。缺省为空 [本参数暂时未启用]</param>
        /// <param name="strDuoyinStyle">是否自动选择多音字。auto/first 之一或者组合</param>
        /// <returns>-1: 出错。包括中断的情况; 0: 正常</returns>
        public int AddPinyin(
            MarcRecord record,
            string strCfgXml,
            PinyinStyle style = PinyinStyle.None,
            string strPrefix = "",
            string strDuoyinStyle = ""
            /*bool bAutoSel = false*/)
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

            // PinyinStyle style = PinyinStyle.None;	// 在这里修改拼音大小写风格
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

                            if (MarcDetailHost.ContainHanzi(strHanzi) == false)
                                continue;

                            string strPinyin = "";
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
                            nRet = this.GetPinyin(this,
                                strHanzi,
                                style,
                                strDuoyinStyle,   // bAutoSel,
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

        public void RemovePinyin(
            MarcRecord record,
            string strCfgXml)
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

                foreach (PinyinCfgItem item in cfg_items)
                {
                    foreach (char ch in item.To)
                    {
                        string to = new string(ch, 1);

                        // 删除已经存在的目标子字段
                        field.select("subfield[@name='" + to + "']").detach();
                    }
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 创建 MARC 格式记录的 HTML 格式
        // paramters:
        //      strMARC MARC机内格式
        // return:
        //      -1  出错
        //      0   .fltx 文件没有找到
        //      1   成功
        public int BuildMarcHtmlText(
            string strSytaxOID,
            string strMARC,
            out string strHtmlString,
            out string strError)
        {
            strHtmlString = "";
            strError = "";
            int nRet = 0;

#if NO
            nRet = BuildMarc21Html(strMARC,
            out strHtmlString,
            out strError);
            if (nRet == -1)
                return -1;
            return 1;
#endif

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this;

            BrowseFilterDocument filter = null;

            string strFilterFileName = Path.Combine(this.DataDir, strSytaxOID.Replace(".", "_") + "\\marc_html.fltx");

            nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
                return 0;

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strHtmlString = host.ResultString;
            }
            finally
            {
                // 归还对象
                filter.FilterHost = null;   // 2016/1/23
                this.Filters.SetFilter(strFilterFileName, filter);
            }

            return 1;
        ERROR1:
            // 不让缓存，因为可能出现了编译错误
            // TODO: 精确区分编译错误
            this.Filters.ClearFilter(strFilterFileName);
            return -1;
        }

        // return:
        //      -1  出错
        //      0   .fltx 文件没有找到
        //      1   成功
        public int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)this.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new BrowseFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " BrowseFilterDocument doc = (BrowseFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            // filter.Load(strFilterFileName);

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (FileNotFoundException ex)
            {
                strError = ex.Message;
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strBinDir = Environment.CurrentDirectory;

            string[] saAddRef1 = {
                    Path.Combine(strBinDir , "digitalplatform.core.dll"),
                    Path.Combine(strBinDir , "digitalplatform.marcdom.dll"),
                    Path.Combine(strBinDir , "digitalplatform.marckernel.dll"),
                    Path.Combine(strBinDir , "digitalplatform.marcquery.dll"),
                    Path.Combine(strBinDir , "digitalplatform.dll"),
                    Path.Combine(strBinDir , "digitalplatform.Text.dll"),
                    Path.Combine(strBinDir , "digitalplatform.IO.dll"),
                    Path.Combine(strBinDir , "digitalplatform.Xml.dll"),
                    Path.Combine(strBinDir , "digitalplatform.LibraryClient.dll"),  // 2017/1/14
					Path.Combine(strBinDir , "dp2catalog.exe") };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
                out assembly,
                out strError,
                out strWarning);

            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;
            return 1;
        ERROR1:
            return -1;
        }

        #region 序列号机制

#if SN
        internal void WriteSerialNumberStatusFile()
        {
            try
            {
                string strFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dp2catalog_status");
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

        internal bool IsExistsSerialNumberStatusFile()
        {
            try
            {
                string strFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dp2catalog_status");
                return File.Exists(strFileName);
            }
            catch
            {
            }
            return true;    // 如果出现异常，则当作有此文件
        }

#endif



        bool _testMode = false;  // true: 序列号为空的时候也当作 评估模式，这样评估模式的前端就无法和标准版的 dp2library 搭配使用了，只能和 dp2libraryXE 单机版评估模式搭配使用

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
            if (this.TestMode == true)
                this.Text = "dp2Catalog V3 -- 编目 [评估模式]";
            else if (this.CommunityMode == true)
                this.Text = "dp2Catalog V3 -- 编目 [社区版]";
            else
                this.Text = "dp2Catalog V3 -- 编目 [专业版]";
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
        internal int VerifySerialCode(
            string strTitle,
            string strRequirFuncList,
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
                    strTitle,
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
        //      bServer     是否为小型服务器版本。如果是小型服务器版本，用 net.tcp 协议绑定 dp2library host；如果不是单机版本，用 net.pipe 绑定 dp2library host
        string GetEnvironmentString(string strMAC,
            bool bNextYear = false)
        {
            Hashtable table = new Hashtable();
            table["mac"] = strMAC;  //  SerialCodeForm.GetMacAddress();
            if (bNextYear == false)
                table["time"] = SerialCodeForm.GetTimeRange();
            else
                table["time"] = SerialCodeForm.GetNextYearTimeRange();

            table["product"] = "dp2catalog";

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

        string CopyrightKey = "dp2catalog_sn_key";

        // return:
        //      0   Cancel
        //      1   OK
        int ResetSerialCode(
            string strTitle,
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
            dlg.Text = strTitle;
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
        "确实要将序列号设置为空?\r\n\r\n(一旦将序列号设置为空，dp2Catalog 将自动退出，下次启动需要重新设置序列号)",
        "dp2Catalog",
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
                    "重新设置序列号",
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

        private void MenuItem_openAdvertiseForm_Click(object sender, EventArgs e)
        {
#if NO
            AdvertiseForm form = new AdvertiseForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
#endif
            OpenWindow<AdvertiseForm>();
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

        private static readonly Object _syncRoot_errorLog = new Object(); // 2018/6/26

        // 写入日志文件。每天创建一个单独的日志文件
        public void WriteErrorLog(string strText)
        {
            FileUtil.WriteErrorLog(
                _syncRoot_errorLog,
                this.UserLogDir,
                strText,
                "log_",
                ".txt");
        }

        // 用 Z39.50 检索当前记录
        private void toolStripButton_searchZ_Click(object sender, EventArgs e)
        {
            // 从当前记录窗得到检索词
            Form active = this.ActiveMdiChild;
            if (active == null)
                return;

            string strUse = "";
            string strQueryWord = "";
            if (active is MarcDetailForm)
            {
                MarcDetailForm detailform = active as MarcDetailForm;
                bool bRet = detailform.GetQueryContent(out strUse,
                    out strQueryWord);
                if (bRet == false)
                {
                    MessageBox.Show(this, "无法从当前记录窗 MARC 记录中获得检索词");
                    return;
                }
            }

            ZSearchForm searchform = GetZSearchForm();
            searchform.SetQueryContent(strUse, strQueryWord);
            searchform.Activate();
            searchform.DoSearch();
        }

        // 用亚马逊检索当前记录
        private void toolStripButton_searchA_Click(object sender, EventArgs e)
        {
            // 从当前记录窗得到检索词
            Form active = this.ActiveMdiChild;
            if (active == null)
                return;

            string strUse = "";
            string strQueryWord = "";
            if (active is MarcDetailForm)
            {
                MarcDetailForm detailform = active as MarcDetailForm;
                bool bRet = detailform.GetQueryContent(out strUse,
                    out strQueryWord);
                if (bRet == false)
                {
                    MessageBox.Show(this, "无法从当前记录窗 MARC 记录中获得检索词");
                    return;
                }
            }

            AmazonSearchForm searchform = GetAmazonSearchForm();
            if (searchform.WindowState == FormWindowState.Minimized)
                searchform.WindowState = FormWindowState.Normal;
            searchform.SetQueryContent(strUse, strQueryWord);
            searchform.Activate();
            searchform.DoSearch();
        }

        // 从当前窗口复制到固定窗口
        private void toolStripButton_copyToFixed_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detailform = (MarcDetailForm)this.ActiveMdiChild;
                detailform.CopyMarcToFixed();
            }
        }

        List<LibraryChannel> _channelList = new List<LibraryChannel>();
        public void DoStop(object sender, StopEventArgs e)
        {
            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        #region dp2library 通道

        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel(string strServerUrl,
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.GUI)
        {
            if (strUserName == ".")
            {
                dp2Server server = this.Servers[strServerUrl];
                if (server == null)
                    throw new Exception("没有找到 URL 为 " + strServerUrl + " 的服务器对象(为寻找默认用户名 . 阶段)");

                if (strUserName == ".")
                    strUserName = server.DefaultUserName;
            }

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

        void Channels_BeforeLogin(object sender, DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            if (e.FirstTry == true)
            {
                dp2Server server = this.Servers[channel.Url];
#if NO
            if (server == null)
            {
                e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                e.Failed = true;
                e.Cancel = true;
                return;
            }
#endif

                if (server != null)
                {
                    e.UserName = server.DefaultUserName;
                    e.Password = server.DefaultPassword;
                }
                else
                {
                    if (channel != null)
                    {
                        e.UserName = channel.UserName;
                        e.Password = channel.Password;
                    }
                    else
                    {
                        e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                        e.Failed = true;
                        e.Cancel = true;
                        return;
                    }
                }

                string type = "worker";
                if (e.UserName.StartsWith("~"))
                {
                    e.UserName = e.UserName.Substring(1);
                    type = "reader";
                }


                e.Parameters = $"location=dp2Catalog,type={type}";

                /*
                e.IsReader = false;
                e.Location = "dp2Catalog";
                 * */
                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = this.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

                // 2014/11/10
                if (this.TestMode == true)
                    e.Parameters += ",testmode=true";

                e.Parameters += ",client=dp2catalog|" + Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = this;

            ServerDlg dlg = SetDefaultAccount(
                e.LibraryServerUrl,
                null,
                e.ErrorInfo,
                owner);
            if (dlg == null)
            {
                e.Cancel = true;
                return;
            }

            if (Servers.Changed == true)
            {
                SaveServers();
            }

            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = false;

            {
                string type = "worker";
                if (e.UserName.StartsWith("~"))
                {
                    e.UserName = e.UserName.Substring(1);
                    type = "reader";
                }

                e.Parameters = $"location=dp2Catalog,type={type}";
            }

            // 2014/11/10
            e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
            {
                // 从序列号中获得 expire= 参数值
                string strExpire = this.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
            }
#endif

            // 2014/11/10
            if (this.TestMode == true)
                e.Parameters += ",testmode=true";

            e.Parameters += ",client=dp2catalog|" + Program.ClientVersion;

            e.SavePasswordLong = true;
            e.LibraryServerUrl = dlg.ServerUrl;
        }

        void SaveServers()
        {
            if (string.IsNullOrEmpty(this.Servers.FileName) == false)
                this.Servers.Save(null);
        }

        ServerDlg SetDefaultAccount(
            string strServerUrl,
            string strTitle,
            string strComment,
            IWin32Window owner)
        {
            dp2Server server = this.Servers[strServerUrl];

            ServerDlg dlg = new ServerDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
            }
            else
            {
                dlg.ServerUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;


            if (String.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;

            dlg.Comment = strComment;
            dlg.UserName = server.DefaultUserName;

            this.AppInfo.LinkFormState(dlg,
                "dp2_logindlg_state");
            this.Activate();    // 让 MDI 子窗口翻出来到前面
            dlg.ShowDialog(owner);

            this.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            bool bChanged = false;

            if (server.DefaultUserName != dlg.UserName)
            {
                server.DefaultUserName = dlg.UserName;
                bChanged = true;
            }

            string strNewPassword = (dlg.SavePassword == true) ?
            dlg.Password : "";
            if (server.DefaultPassword != strNewPassword)
            {
                server.DefaultPassword = strNewPassword;
                bChanged = true;
            }

            if (server.SavePassword != dlg.SavePassword)
            {
                server.SavePassword = dlg.SavePassword;
                bChanged = true;
            }

            if (server.Url != dlg.ServerUrl)
            {
                server.Url = dlg.ServerUrl;
                bChanged = true;
            }

            if (bChanged == true)
                this.Servers.Changed = true;

            return dlg;
        }

        public string CurrentUserName { get; set; }

        void Channels_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            this.CurrentUserName = channel.UserName;

            dp2Server server = this.Servers[channel.Url];
#if NO
            if (server == null)
            {
                e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                return;
            }
#endif

            if (server != null)
            {
#if SN
                if (server.Verified == false && StringUtil.IsInList("serverlicensed", channel.Rights) == false)
                {
                    string strTitle = "dp2 检索窗需要先设置序列号才能访问服务器 " + server.Name + " " + server.Url;
                    int nRet = this.VerifySerialCode(strTitle,
                        "",
                        true,
                        out string strError);
                    if (nRet == -1)
                    {
                        channel.Close();
                        e.ErrorInfo = strTitle;
                        return;
                    }
                }
#endif
                server.Verified = true;
            }

            if (_virusScanned == false)
            {
                if (StringUtil.IsInList("clientscanvirus", channel.Rights) == true)
                {
                    if (DetectVirus.DetectXXX() == true || DetectVirus.DetectGuanjia() == true)
                    {
                        {
                            channel.Close();
                            e.ErrorInfo = "dp2Catalog 被木马软件干扰，无法运行";
                            return;
                        }
                        /*
                        channel.Close();
                        // Program.PromptAndExit(this, );
                        throw new InterruptException("dp2Catalog 被木马软件干扰，无法运行");
                        */
                    }
                }
                _virusScanned = true;
            }
        }

        static bool _virusScanned = false;

        #endregion

        private void MenuItem_editMarcoTable_Click(object sender, EventArgs e)
        {
            MacroTableDialog dlg = new MacroTableDialog();
            // MainForm.SetControlFont(dlg, this.Font, false);
            GuiUtil.SetControlFont(dlg, this.DefaultFont);
            dlg.XmlFileName = Path.Combine(Program.MainForm.UserDir, "marceditor_macrotable.xml");

            this.AppInfo.LinkFormState(dlg, "MacroTableDialog_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
        }

        private void MenuItem_openSruSearchForm_Click(object sender, EventArgs e)
        {
            OpenWindow<SruSearchForm>();
        }
    }

    public class EnableState
    {
        public object Control = null;
        public bool Enabled = false;
    }

    public class EnableStateCollection : List<EnableState>
    {
        // 推入，并Disable控件
        public void Push(object obj)
        {
            EnableState state = new EnableState();
            state.Control = obj;

            ToolStripItem item = null;
            Control control = null;

            if (obj is Control)
            {
                control = (Control)obj;
                state.Enabled = control.Enabled;

                control.Enabled = false;
            }
            else if (obj is ToolStripItem)
            {
                item = (ToolStripItem)obj;
                state.Enabled = item.Enabled;

                item.Enabled = false;
            }
            else
            {
                throw new Exception("obj类型必须为Control ToolStripItem之一");
            }

            this.Add(state);
        }



        public void RestoreAll()
        {
            for (int i = 0; i < this.Count; i++)
            {
                EnableState state = this[i];

                ToolStripItem item = null;
                Control control = null;

                if (state.Control is Control)
                {
                    control = (Control)state.Control;
                    control.Enabled = state.Enabled;
                }
                else if (state.Control is ToolStripItem)
                {
                    item = (ToolStripItem)state.Control;
                    item.Enabled = state.Enabled;
                }
                else
                {
                    throw new Exception("state.Control类型必须为Control ToolStripItem之一");
                }

            }

            this.Clear(); // 用后清除
        }
    }

    //
    /*
<item name = "ISSN" value=8 uni_name = "Identifier-ISSN" />
     * */
    public class Bib1Use
    {
        public string Name = "";
        public string Value = "";
        public string UniName = "";
        public string Comment = "";
    }

    public class FromCollection : List<Bib1Use>
    {
        public string GetValue(string strName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name.Trim() == strName.Trim())
                    return this[i].Value;
            }

            return null;    // not found
        }

    }

}