using System;
using System.Collections.Generic;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using Ionic.Zip;
using Microsoft.Win32;

using PalmCenter.Install;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.OPAC;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using dp2ZServer.Install;


namespace dp2Installer
{
    public partial class MainForm : Form
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        // 被锁定的实例名数组
        // 正在进行恢复操作的实例名，会进入本数组。以防中途被启动
        List<string> locking_instances = new List<string>();

        FloatingMessageForm _floatingMessage = null;

        /// <summary>
        /// Stop 管理器
        /// </summary>
        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop stop = null;

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

        public string BinDir
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        /// <summary>
        /// 配置存储
        /// </summary>
        // public ApplicationInfo AppInfo = null;

        public MainForm()
        {
            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }
        }

        static bool _networkDeployment = false;

        private void MainForm_Load(object sender, EventArgs e)
        {
            GuiUtil.AutoSetDefaultFont(this);

            var bRet = ClientInfo.Initial("dp2installer_v1");
            if (bRet == false)
            {
                Application.Exit();
                return;
            }

            ClientInfo.BeginUpdate(
TimeSpan.FromMinutes(2),
TimeSpan.FromMinutes(60),
_cancel.Token,
(text, level) =>
{
    AppendString($"{text}\r\n");
});

            stopManager.Initial(this.toolButton_stop,
(object)this.toolStripStatusLabel_main,
(object)this.toolStripProgressBar_main);

            stop = new DigitalPlatform.Stop();
            stop.Register(stopManager, true);   // 和容器关联

            /*
            // TODO: 这里 this.UserDir 尚未初始化
            this.AppInfo = new ApplicationInfo(Path.Combine(this.UserDir, "settings.xml"));
            this.AppInfo.LoadFormStates(this,
"mainformstate",
FormWindowState.Normal);
            */
            // 恢复 MainForm 的显示状态
            {
                var state = ClientInfo.Config.Get("mainForm", "state", "");
                if (string.IsNullOrEmpty(state) == false)
                {
                    bool force_minimize = ClientInfo.IsMinimizeMode();
                    FormProperty.SetProperty(state, this, force_minimize);
                }
            }

            ClearForPureTextOutputing(this.webBrowser1);

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
                        _networkDeployment = true;
                    }
                    else if (StringUtil.HasHead(strArg, "userdir=") == true)
                    {
                        this.UserDir = strArg.Substring("userdir=".Length);
#if LOG
                        WriteLibraryEventLog("从命令行参数得到, this.UserDir=" + this.UserDir, EventLogEntryType.Information);
#endif
                        _networkDeployment = true;
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
                    "dp2installer_v1");
#if LOG
                WriteLibraryEventLog("普通方法得到, this.UserDir=" + this.UserDir, EventLogEntryType.Information);
#endif
            }
            PathUtil.TryCreateDir(this.UserDir);

            this.TempDir = Path.Combine(this.UserDir, "temp");
            PathUtil.TryCreateDir(this.TempDir);

            // 2015/8/8
            this.UserLogDir = Path.Combine(this.UserDir, "log");
            PathUtil.TryCreateDir(this.UserLogDir);

            _versionManager.Load(Path.Combine(this.UserDir, "file_version.xml"));

            DisplayCopyRight();

            Refresh_dp2OPAC_MenuItems();

            Refresh_dp2kernel_MenuItems();
            Refresh_dp2library_MenuItems();
            Refresh_dp2ZServer_MenuItems();
            Refresh_palmCenter_MenuItems();

            this.BeginInvoke(new Action<object, EventArgs>(MenuItem_autoUpdate_Click), this, new EventArgs());

#if REMOVED
            // 2019/2/15
            // 后台自动检查更新
            Task.Run(() =>
            {
                var result = ClientInfo.InstallUpdateSync();
                if (result.Value == -1)
                    AppendString($"ClickOnce 后台自动更新出错: {result.ErrorInfo}\r\n");
                else if (result.Value == 1)
                    AppendString($"ClickOnce 后台自动更新: {result.ErrorInfo}\r\n");
                else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                    AppendString($"ClickOnce 后台自动更新: {result.ErrorInfo}\r\n");
            });
#endif
        }

        void DisplayCopyRight()
        {
            AppendString("dp2Installer - dp2 图书馆集成系统 安装实用工具\r\n");
            AppendString("版本: " + ClientInfo.ClientVersion + "\r\n");
            AppendString("(C)2015 版权所有 数字平台(北京)软件有限责任公司\r\n"
                + "2015 年以 Apache License Version 2.0 方式开源\r\n"
                + "https://github.com/digitalplatform/dp2\r\n");
            AppendString("\r\n");
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 2020/9/17
            _cancel?.Cancel();

            if (_versionManager != null)
            {
                _versionManager.AutoSave();
            }

            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            // 保存 MainForm 的显示状态
            {
                var state = FormProperty.GetProperty(this);
                ClientInfo.Config.Set("mainForm", "state", state);
            }

            /*
            if (this.AppInfo != null)
            {
                AppInfo.SaveFormStates(this,
        "mainformstate");

                AppInfo.Save();
                AppInfo = null;	// 避免后面再用这个对象
            }
            */

            if (_floatingMessage != null)
                _floatingMessage.Close();

            ClientInfo.Finish();
        }

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
    + "body { font-family:Microsoft YaHei; background-color:#555555; color:#eeeeee; } " // background-color:#555555
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
            if (this.webBrowser1.Document != null
                && this.webBrowser1.Document.Window != null
                && this.webBrowser1.Document.Body != null)
                this.webBrowser1.Document.Window.ScrollTo(
                    0,
                    this.webBrowser1.Document.Body.ScrollRectangle.Height);
#endif
            this.webBrowser1.ScrollToEnd();
        }


        #endregion

        private void button_uninstall_dp2library_Click(object sender, EventArgs e)
        {
            System.ServiceProcess.ServiceInstaller installer = new System.ServiceProcess.ServiceInstaller();
            System.Configuration.Install.InstallContext context = new System.Configuration.Install.InstallContext();
            installer.Context = context;
            installer.ServiceName = "dp2LibraryService";
            try
            {
                installer.Uninstall(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

#if NO
        private void button_start_Click(object sender, EventArgs e)
        {
            ServiceController service = new ServiceController("dp2LibraryService");
            try
            {
                TimeSpan timeout = TimeSpan.FromMinutes(5);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                if (GetNativeErrorCode(ex) == 1060)
                    MessageBox.Show(this, "服务不存在");
                else if (GetNativeErrorCode(ex) == 1056)
                    MessageBox.Show(this, "调用前已经启动了");
                else
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            MessageBox.Show(this, "OK");
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            ServiceController service = new ServiceController("dp2LibraryService");
            try
            {
                TimeSpan timeout = TimeSpan.FromMinutes(5);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception ex)
            {
                if (GetNativeErrorCode(ex) == 1060)
                    MessageBox.Show(this, "服务不存在");
                else if (GetNativeErrorCode(ex) == 1062)
                    MessageBox.Show(this, "调用前已经停止了");
                else
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            MessageBox.Show(this, "OK");
        }
#endif

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

#if NO
        static string Unquote(string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";
            if (strValue[0] == '\"')
                strValue = strValue.Substring(1);
            if (string.IsNullOrEmpty(strValue) == true)
                return "";
            if (strValue[strValue.Length - 1] == '\"')
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }
#endif

        private async void MenuItem_dp2library_update_Click(object sender, EventArgs e)
        {
            await update_dp2library();
        }

        private async Task update_dp2library()
        {
            string strError = "";
            int nRet = 0;

            if (this.locking_instances.Count > 0)
            {
                strError = "目前有下列 dp2library 实例(" + StringUtil.MakePathList(this.locking_instances) + ")处于锁定状态，不允许此时更新 dp2library";
                goto ERROR1;
            }

            this._floatingMessage.Text = "正在更新 dp2library - 图书馆应用服务器 ...";

            try
            {
                AppendSectionTitle("更新 dp2library 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2library 未曾安装过";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                AppendString("正在停止 dp2library 服务 ...\r\n");
                NormalResult result = await InstallHelper.StopService(this, "dp2LibraryService");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
#if NO
                nRet = InstallHelper.StopService("dp2LibraryService",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif
                AppendString("dp2library 服务已经停止\r\n");

                string strZipFileName = Path.Combine(this.DataDir, "library_app.zip");

                AppendString("更新可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    Path.GetDirectoryName(strExePath),
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 更新 dp2library 数据目录中的 cfgs 子目录 和 templates 子目录
                nRet = UpdateLibraryCfgs(true, out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("检查序列号 ...\r\n");

                // 检查和设置各个实例的序列号
                nRet = VerifyDp2librarySerialNumbers(false, out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("正在重新启动 dp2library 服务 ...\r\n");
                nRet = InstallHelper.StartService("dp2LibraryService",
        out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2library 服务启动成功\r\n");

                AppendSectionTitle("更新 dp2library 结束");
            }
            finally
            {
                this._floatingMessage.Text = "";
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_listServices_Click(object sender, EventArgs e)
        {
#if NO
            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController service in services)
            {
                if (service.ServiceName == "dp2LibraryService")
                {

                }
            }
#endif

            MessageBox.Show(this, InstallHelper.GetPathOfService("dp2LibraryService"));
        }

#if NO
        // 速度慢
        public static string GetPathOfService(string serviceName)
        {
            WqlObjectQuery wqlObjectQuery = new WqlObjectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName));
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                return managementObject.GetPropertyValue("PathName").ToString();
            }

            return null;
        }
#endif

        bool DetectChange(List<OpacAppInfo> infos,
            string strStyleZipFileName)
        {
            foreach (OpacAppInfo info in infos)
            {
                if (string.IsNullOrEmpty(info.PhysicalPath) == true)
                    continue;

                string strTargetDir = Path.Combine(info.DataDir, "style");
                if (Directory.Exists(strTargetDir) == true)
                {
                    if (DetectChange(strStyleZipFileName, strTargetDir) == true)
                        return true;
                }

                strTargetDir = Path.Combine(info.PhysicalPath, "style");
                if (Directory.Exists(strTargetDir) == true)
                {
                    if (DetectChange(strStyleZipFileName, strTargetDir) == true)
                        return true;
                }
            }

            return false;
        }

        // 探测一个 zip 文件上次用于升级以后是否发生了新变化
        // return:
        //      false   没有发生变化
        //      true    发生了变化
        bool DetectChange(string strZipFileName,
            string strTargetDir = null)
        {
            string strOldTimestamp = "";

            string strEntry = Path.GetFileName(strZipFileName);
            if (strTargetDir != null)
                strEntry += "|" + strTargetDir;

            // 由于数据目录经常变化，所以要使用纯文件名
            int nRet = _versionManager.GetFileVersion(strEntry, out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (strOldTimestamp == strNewTimestamp)
                return false;
            return true;
        }

        // 更新可执行目录
        // parameters:
        //      excludes    要排除的文件名。纯文件名。必须为小写形态
        // return:
        //      -1  出错
        //      0   没有必要刷新
        //      1   已经刷新
        int RefreshBinFiles(
            bool bAuto,
            string strZipFileName,
            string strTargetDir,
            List<string> excludes,
            out string strError)
        {
            strError = "";

            if (excludes != null)
            {
                foreach (string filename in excludes)
                {
                    if (filename.ToLower() != filename)
                    {
                        strError = "excludes 中的字符串必须为小写形态";
                        return -1;
                    }
                }
            }

            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(Path.GetFileName(strZipFileName), out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (bAuto == true && strOldTimestamp == strNewTimestamp)
            {
                strError = "没有更新";
                return 0;
            }

            // 要求在 xxx_app.zip 内准备要安装的可执行程序文件
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    for (int i = 0; i < zip.Count; i++)
                    {
                        ZipEntry e = zip[i];

                        if (excludes != null && (e.Attributes & FileAttributes.Directory) == 0)
                        {
                            if (excludes.IndexOf(Path.GetFileName(e.FileName).ToLower()) != -1)
                                continue;
                        }

                        string strPart = GetSubPath(e.FileName);
                        string strFullPath = Path.Combine(strTargetDir, strPart);

                        e.FileName = strPart;

                        if ((e.Attributes & FileAttributes.Directory) == 0)
                        {
                            ExtractFile(e, strTargetDir);
                            AppendString("更新文件 " + strFullPath + "\r\n");
                        }
                        else
                            e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
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
            _versionManager.SetFileVersion(Path.GetFileName(strZipFileName), strNewTimestamp);
            _versionManager.AutoSave();
            return 1;
        }

        // 2022/1/27
        // 从 .zip 文件中解析出若干指定的文件
        int ExtractFiles(
    string strZipFileName,
    string strTargetDir,
    List<string> filenames,
    out string strError)
        {
            strError = "";

            if (filenames != null)
            {
                foreach (string filename in filenames)
                {
                    if (filename.ToLower() != filename)
                    {
                        strError = "filenames 中的字符串必须为小写形态";
                        return -1;
                    }
                }
            }

            int count = 0;

            // 要求在 xxx_app.zip 内准备要安装的可执行程序文件
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    for (int i = 0; i < zip.Count; i++)
                    {
                        ZipEntry e = zip[i];

                        if (filenames != null && (e.Attributes & FileAttributes.Directory) == 0)
                        {
                            if (filenames.IndexOf(Path.GetFileName(e.FileName).ToLower()) == -1)
                                continue;
                        }

                        string strPart = GetSubPath(e.FileName);
                        string strFullPath = Path.Combine(strTargetDir, strPart);

                        e.FileName = strPart;

                        if ((e.Attributes & FileAttributes.Directory) == 0)
                        {
                            ExtractFile(e, strTargetDir);
                            // AppendString("提取文件到 " + strFullPath + "\r\n");
                            AppendString($"从 {strZipFileName} 提取文件 {e.FileName}\r\n");
                            count++;
                        }
                        else
                        {
                            // e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            return 1;
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

            // string strOldValue = e.FileName;

            // e.FileName = Path.GetFileName(e.FileName);

            // e.Extract(strTempDir, ExtractExistingFileAction.OverwriteSilently);
            // e.FileName = strOldValue;

            // 2015/5/9
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
                    PathUtil.TryCreateDir(Path.GetDirectoryName(strTargetPath));

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

        private void MenuItem_dp2library_instanceManagement_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;
            bool bInstalled = true;

            this._floatingMessage.Text = "正在配置 dp2library 实例 ...";

            try
            {
                AppendSectionTitle("配置实例开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    if (bControl == false)
                    {
                        strError = "dp2library 未曾安装过";
                        goto ERROR1;
                    }
                    bInstalled = false;
                }

                //strExePath = Unquote(strExePath);
                //string strRootDir = Path.GetDirectoryName(strExePath);

                // MessageBox.Show(this, "为进行配置，将首先停止 dp2library 服务。配置完成后 dp2library 会重新启动");

#if NO
                if (bInstalled == true)
                {
                    AppendString("正在停止 dp2library 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2LibraryService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2library 服务已经停止\r\n");
                }
#endif

                List<string> new_instance_names = null;

                try
                {
                    DigitalPlatform.LibraryServer.InstanceDialog dlg = new DigitalPlatform.LibraryServer.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.LockingInstances = this.locking_instances;
                    dlg.TempDir = this.TempDir;
                    // dlg.SourceDir = strRootDir;
                    dlg.CopyFiles += dlg_dp2Library_CopyFiles;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    //if (dlg.DialogResult == DialogResult.Cancel)
                    //    return;

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("创建实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }

                    new_instance_names = dlg.NewInstanceNames;
                }
                finally
                {
#if NO
                    if (bInstalled == true)
                    {
                        string strError1 = "";
                        AppendString("正在重新启动 dp2library 服务 ...\r\n");
                        nRet = InstallHelper.StartService("dp2LibraryService",
        out strError1);
                        if (nRet == -1)
                        {
                            AppendString("dp2library 服务启动失败: " + strError1 + "\r\n");
                            MessageBox.Show(this, strError1);
                        }
                        else
                        {
                            AppendString("dp2library 服务启动成功\r\n");
                        }
                    }
#endif

                    AppendSectionTitle("配置实例结束");
                    this.Refresh_dp2library_MenuItems();
                }

                CreateDefaultDatabases(new_instance_names);
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        FileVersionManager _versionManager = new FileVersionManager();

        // 更新 dp2OPAC 数据目录或者虚拟目录中的 style 子目录
        // parameters:
        //      bAuto   是否自动更新。true 表示(.zip 文件发生了变化)有必要才更新; false 表示无论如何均更新
        int UpdateOpacStyles(
            bool bAuto,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strZipFileName = Path.Combine(this.DataDir, "opac_style.zip");
#if NO
            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(Path.GetFileName(strZipFileName), out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (bAuto == false || strOldTimestamp != strNewTimestamp)
            {
#endif
            List<OpacAppInfo> infos = null;
            // 查找 dp2OPAC 路径
            // return:
            //      -1  出错
            //      其他  返回找到的路径个数
            nRet = OpacAppInfo.GetOpacInfo(out infos,
                out strError);
            if (nRet == -1)
                return -1;

            if (infos.Count == 0)
                return 0;

            foreach (OpacAppInfo info in infos)
            {
                if (string.IsNullOrEmpty(info.PhysicalPath) == true)
                    continue;

                AppendString("更新实例 '" + info.IisPath + "' 的 style 目录 ...\r\n");

                // 从 opac_style.zip 中展开部分目录内容
                string strTargetPath = Path.Combine(info.PhysicalPath, "style");
                if (Directory.Exists(strTargetPath) == true)
                {
                    nRet = dp2OPAC_extractDir(
                        bAuto,
                        strZipFileName,
                        strTargetPath,
                        false,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                strTargetPath = Path.Combine(info.DataDir, "style");
                if (Directory.Exists(strTargetPath) == true)
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
            }
#if NO
                _versionManager.SetFileVersion(Path.GetFileName(strZipFileName), strNewTimestamp);
                _versionManager.AutoSave();
            }
#endif

            return 0;
        }

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

        // 更新 dp2library 数据目录中的 cfgs 子目录 和 templates 子目录
        // parameters:
        //      bAuto   是否自动更新。true 表示(.zip 文件发生了变化)有必要才更新; false 表示无论如何均更新
        int UpdateLibraryCfgs(
            bool bAuto,
            out string strError)
        {
            strError = "";

            string strZipFileName = Path.Combine(this.DataDir, "library_data.zip");

            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(Path.GetFileName(strZipFileName), out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (bAuto == false || strOldTimestamp != strNewTimestamp)
            {
                for (int i = 0; ; i++)
                {
                    string strInstanceName = "";
                    string strDataDir = "";
                    string strCertificatSN = "";

                    string[] existing_urls = null;
                    bool bRet = InstallHelper.GetInstanceInfo("dp2Library",
                        i,
                        out strInstanceName,
                        out strDataDir,
                        out existing_urls,
                        out strCertificatSN);
                    if (bRet == false)
                        break;

                    AppendString("更新实例 '" + strInstanceName + "' 的数据目录 " + strDataDir + " ...\r\n");

                    // 从 library_data.zip 中展开部分目录内容
                    nRet = dp2Library_extractPartDir(strDataDir,
                        "",
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                _versionManager.SetFileVersion(Path.GetFileName(strZipFileName), strNewTimestamp);
                _versionManager.AutoSave();
            }

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

        // 去掉第一级路径
        static string GetSubPath(string strPath)
        {
            int nRet = strPath.IndexOfAny(new char[] { '/', '\\' }, 0);
            if (nRet == -1)
                return "";
            return strPath.Substring(nRet + 1);
        }


        // 从 library_data.zip 中展开部分目录内容
        // parameters:
        //      strLibraryDataDir   dp2library的数据目录。这是一个恒定的目录，不会(像 ClickOnce 数据目录一样)变化
        //      strPartList 展开哪些部分? cfgs/templates/other
        int dp2Library_extractPartDir(
            string strLibraryDataDir,
            string strPartList,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strPartList) == true)
                strPartList = "cfgs,templates";

            string strCfgsDir = Path.Combine(strLibraryDataDir, "cfgs");
            string strTemplatesDir = Path.Combine(strLibraryDataDir, "templates");

            string strZipFileName = Path.Combine(this.DataDir, "library_data.zip");

            // 要求在 library_data.zip 内准备要安装的数据文件(初次安装而不是升级安装)
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    // foreach (ZipEntry e in zip)
                    for (int i = 0; i < zip.Count; i++)
                    {
                        ZipEntry e = zip[i];
                        // string strFullPath = Path.Combine(this.UserDir, e.FileName);
                        string strPart = GetSubPath(e.FileName);
                        string strFullPath = Path.Combine(strLibraryDataDir, strPart);

                        e.FileName = strPart;

                        // 测试strPath1是否为strPath2的下级目录或文件
                        //	strPath1正好等于strPath2的情况也返回true
                        if (PathUtil.IsChildOrEqual(strFullPath, strTemplatesDir) == true
                            && StringUtil.IsInList("templates", strPartList) == true)
                        {
                            if ((e.Attributes & FileAttributes.Directory) == 0)
                                AppendString("更新模板文件 " + strFullPath + "\r\n");

                            e.Extract(strLibraryDataDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                        else if (PathUtil.IsChildOrEqual(strFullPath, strCfgsDir) == true
                            && StringUtil.IsInList("cfgs", strPartList) == true)
                        {
                            // 观察文件版本
                            if (File.Exists(strFullPath) == true)
                            {

                                // .zip 中的对应文件的时间戳
                                string strZipTimestamp = e.LastModified.ToString();

                                // 版本管理器记载的时间戳
                                string strTimestamp = "";
                                int nRet = _versionManager.GetFileVersion(strFullPath, out strTimestamp);
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

                            e.Extract(strLibraryDataDir, ExtractExistingFileAction.OverwriteSilently);
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
                        else
                        {
                            if (StringUtil.IsInList("other", strPartList) == true)
                            {
                                if ((e.Attributes & FileAttributes.Directory) == 0)
                                    AppendString("释放文件 " + strFullPath + "\r\n");

                                e.Extract(strLibraryDataDir, ExtractExistingFileAction.OverwriteSilently);
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

        // 更新 dp2library 全部数据目录中的配置文件
        private void MenuItem_dp2library_updateCfgs_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            nRet = UpdateLibraryCfgs(
                false,  // 强制更新
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("更新结束");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

        private void MenuItem_copyright_Click(object sender, EventArgs e)
        {
            // throw new Exception("test throw exception");
        }

        // 检查和设置各个实例的序列号
        // return:
        //      -1  出错
        //      其他  返回重设过的序列号个数
        int VerifyDp2librarySerialNumbers(bool bDisplaySectionTitle,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            int nCount = 0;

            if (bDisplaySectionTitle)
                AppendSectionTitle("检查序列号开始");

            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertificateSN = "";
                string strSerialCode = "";

                string[] existing_urls = null;
                bool bRet = InstallHelper.GetInstanceInfo("dp2Library",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertificateSN,
                    out strSerialCode,
                    out string style);
                if (bRet == false)
                    break;

                AppendString("*** 检查实例 " + (i + 1) + " '" + strInstanceName + "' 的序列号 '" + strSerialCode + "' ...\r\n");

                if (string.IsNullOrEmpty(strSerialCode) == true
                    || strSerialCode == "community"
                    || strSerialCode == "*")
                    continue;

                if ( // String.IsNullOrEmpty(strSerialCode) == true || 
                    DigitalPlatform.LibraryServer.OneInstanceDialog.MatchLocalString(strSerialCode, strInstanceName) == false)
                {
                    // MessageBox.Show(this, "序列号无效。请重新输入");

                    AppendString("实例 '" + strInstanceName + "' 的序列号 " + strSerialCode + " 失效了，正在请求重新设置 ...\r\n");

                    string strOldSerialCode = strSerialCode;
                REDO_INPUT:
                    // 出现对话框重新设置序列号
                    // return:
                    //      0   Cancel
                    //      1   OK
                    nRet = DigitalPlatform.LibraryServer.OneInstanceDialog.ResetSerialCode(
    this,
    "需要为实例 '" + strInstanceName + "' 重新设置序列号",
    true,
    ref strSerialCode,
    DigitalPlatform.LibraryServer.OneInstanceDialog.GetEnvironmentString(strFirstMac, strSerialCode, strInstanceName));
                    if (nRet == 0)
                    {
                        AppendString("放弃设置实例 '" + strInstanceName + "' 的序列号\r\n");
                        continue;
                    }
                    if (DigitalPlatform.LibraryServer.OneInstanceDialog.MatchLocalString(strSerialCode, strInstanceName) == false
                        && strSerialCode != "community")
                    {
                        MessageBox.Show(this, "实例 '" + strInstanceName + "' 的序列号 '" + strSerialCode + "' 经验证无效。请重新输入");
                        goto REDO_INPUT;
                    }

                    if (strOldSerialCode != strSerialCode)
                    {
                        InstallHelper.SetInstanceInfo(
                            "dp2Library",
                            i,
                            strInstanceName,
                            strDataDir,
                            existing_urls,
                            strCertificateSN,
                            strSerialCode,
                            style);
                        AppendString("实例 '" + strInstanceName + "' 的序列号被重新设定为 " + strSerialCode + " \r\n");
                        nCount++;
                    }
                }
            }

            if (bDisplaySectionTitle)
                AppendSectionTitle("检查序列号结束");

            return nCount;
        }

        private void MenuItem_dp2library_verifySerialNumbers_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 检查和设置各个实例的序列号
            // return:
            //      -1  出错
            //      其他  返回重设过的序列号个数
            nRet = VerifyDp2librarySerialNumbers(
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet > 0)
            {
                AppendString("因重设了 " + nRet + " 个序列号， 正在自动重启 dp2library 服务 ...\r\n");
                nRet = InstallHelper.StopService("dp2LibraryService",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                nRet = InstallHelper.StartService("dp2LibraryService",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2library 服务已经成功重启\r\n");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private async void MenuItem_dp2kernel_update_Click(object sender, EventArgs e)
        {
            await update_dp2kernel();
        }

        // 升级 dp2kernel
        private async Task update_dp2kernel()
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在更新 dp2kernel - 数据库内核 ...";

            try
            {
                AppendSectionTitle("更新 dp2kernel 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2kernel 未曾安装过";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                AppendString("正在停止 dp2kernel 服务 ...\r\n");
                NormalResult result = await InstallHelper.StopService(this, "dp2KernelService");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
#if NO
                nRet = InstallHelper.StopService("dp2KernelService",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif
                AppendString("dp2kernel 服务已经停止\r\n");

                string strZipFileName = Path.Combine(this.DataDir, "kernel_app.zip");

                AppendString("更新可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    Path.GetDirectoryName(strExePath),
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("正在重新启动 dp2kernel 服务 ...\r\n");
                nRet = InstallHelper.StartService("dp2KernelService",
        out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2kernel 服务启动成功\r\n");

                AppendSectionTitle("更新 dp2kernel 结束");
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void MenuItem_dp2opac_update_Click(object sender, EventArgs e)
        {
            string strError = "";

            this._floatingMessage.Text = "正在更新 dp2OPAC - 读者公共查询 ...";

            try
            {
                string strZipFileName = Path.Combine(this.DataDir, "opac_app.zip");

                List<OpacAppInfo> infos = null;
                // 查找 dp2OPAC 路径
                // return:
                //      -1  出错
                //      其他  返回找到的路径个数
                int nRet = OpacAppInfo.GetOpacInfo(out infos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (infos.Count == 0)
                {
                    strError = "在本机的 IIS 中没有找到任何名为 dp2OPAC 的虚拟目录";
                    goto ERROR1;
                }

                AppendSectionTitle("更新 dp2OPAC 开始");

                foreach (OpacAppInfo info in infos)
                {
                    if (string.IsNullOrEmpty(info.PhysicalPath) == true)
                        continue;

                    AppendString("*** 更新 IIS 虚拟目录 " + info.IisPath + " 对应的物理目录 " + info.PhysicalPath + " 中的可执行文件 ...\r\n");

                    // 2015/7/21
                    // 先删除 目标目录下的 app_code 目录内的所有文件
                    // 这是因为以前版本的 dp2OPAC 可能在这里遗留了 global.asax.cs 文件，而新版本移动到其上级子目录存储了，并且 app_code 子目录内没有任何文件了
                    string strAppCodeDir = Path.Combine(info.PhysicalPath, "app_code");
                    if (Directory.Exists(strAppCodeDir) == true)
                    {
                        PathUtil.DeleteDirectory(strAppCodeDir);
                    }

                    List<string> excludes = new List<string>() { "web.config" };
                    // 更新可执行目录
                    // return:
                    //      -1  出错
                    //      0   没有必要刷新
                    //      1   已经刷新
                    nRet = RefreshBinFiles(
                        false,
        strZipFileName,
        info.PhysicalPath,
        excludes,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 2022/1/27
                    // 单独合并更新 web.config 文件
                    if (nRet == 1)
                    {
                        string strTempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        //string strTempFileName = "";
                        try
                        {
                            nRet = ExtractFiles(
        strZipFileName,
        strTempDir,
        new List<string>() { "web.config" },
        out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            string sourceFileName = Path.Combine(strTempDir, "web.config");
                            string targetFileName = Path.Combine(info.PhysicalPath, "web.config");

                            //strTempFileName = sourceFileName;

                            if (File.Exists(sourceFileName) && File.Exists(targetFileName))
                            {
                                nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
            targetFileName,
            out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                if (nRet == 0)
                                    AppendString("web.config 文件没有变化\r\n");
                                else if (nRet == 1)
                                    AppendString("web.config 文件发生了局部更新\r\n");
                            }
                        }
                        finally
                        {
                            /*
                            if (File.Exists(strTempFileName))
                                File.Delete(strTempFileName);
                            */
                            if (Directory.Exists(strTempDir))
                                Directory.Delete(strTempDir, true);
                        }
                    }
                }



                nRet = UpdateOpacStyles(
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendSectionTitle("更新 dp2OPAC 结束");
            }
            finally
            {
                this._floatingMessage.Text = "";
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 自动更新
        private async void MenuItem_autoUpdate_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> names = new List<string>();
            string strZipFileName = "";
            string strExePath = "";

            // ---
            strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strZipFileName = Path.Combine(this.DataDir, "kernel_app.zip");
                if (DetectChange(strZipFileName) == true)
                    names.Add("dp2Kernel");
            }

            // ---
            strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strZipFileName = Path.Combine(this.DataDir, "library_app.zip");

                string strDataZipFileName = Path.Combine(this.DataDir, "library_data.zip");
                if (DetectChange(strZipFileName) == true
                    || DetectChange(strDataZipFileName) == true)
                    names.Add("dp2Library");
            }

            // ---
            List<OpacAppInfo> infos = null;
            // 查找 dp2OPAC 路径
            // return:
            //      -1  出错
            //      其他  返回找到的路径个数
            int nRet = OpacAppInfo.GetOpacInfo(out infos,
                out strError);
            if (nRet > 0)
            {
                strZipFileName = Path.Combine(this.DataDir, "opac_app.zip");

                string strStyleZipFileName = Path.Combine(this.DataDir, "opac_style.zip");

                if (DetectChange(strZipFileName) == true
                    || DetectChange(infos, strStyleZipFileName) == true)
                    names.Add("dp2OPAC");
            }

            // ---
            strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strZipFileName = Path.Combine(this.DataDir, "zserver_app.zip");

                // string strDataZipFileName = Path.Combine(this.DataDir, "zserver_data.zip");
                if (DetectChange(strZipFileName) == true
                    /*|| DetectChange(strDataZipFileName) == true*/)
                    names.Add("dp2ZServer");
            }

            // 2021/1/4
            // ---
            {
                string strProgramDir = GetProductDirectory("palmcenter");
                strExePath = Path.Combine(strProgramDir, "palmcenter.exe");

                if (File.Exists(strExePath) == true)
                {
                    // (ClickOnce 安装时)确保文件已经下载到本地
                    var ret = await PrepareDataFile("palm_app.zip");
                    if (ret.Value == -1)
                    {
                        AppendString($"*** 出错: 准备 palm_app.zip 文件失败: {ret.ErrorInfo}\r\n");
                    }
                    else
                    {
                        strZipFileName = Path.Combine(this.DataDir, "palm_app.zip");

                        if (DetectChange(strZipFileName) == true)
                            names.Add("palmCenter");
                    }
                }
            }


            // 2021/12/22
            // ---
            {
                string strProgramDir = GetProductDirectory("dp2gate");
                strExePath = Path.Combine(strProgramDir, "dp2gate.exe");

                if (File.Exists(strExePath) == true)
                {
                    // (ClickOnce 安装时)确保文件已经下载到本地
                    var ret = await PrepareDataFile("gate_app.zip");
                    if (ret.Value == -1)
                    {
                        AppendString($"*** 出错: 准备 gate_app.zip 文件失败: {ret.ErrorInfo}\r\n");
                    }
                    else
                    {
                        strZipFileName = Path.Combine(this.DataDir, "gate_app.zip");

                        if (DetectChange(strZipFileName) == true)
                            names.Add("dp2Gate");
                    }
                }
            }

            if (names.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
"下列模块有新版本：\r\n" + StringUtil.MakePathList(names, "\r\n") + "\r\n\r\n是否更新？",
"dp2Installer",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
                foreach (string name in names)
                {
                    if (name == "dp2Kernel")
                        await update_dp2kernel();
                    else if (name == "dp2Library")
                        await update_dp2library();
                    else if (name == "dp2OPAC")
                        MenuItem_dp2opac_update_Click(this, new EventArgs());
                    else if (name == "dp2ZServer")
                        MenuItem_dp2ZServer_update_Click(this, new EventArgs());
                    else if (name == "palmCenter")
                        MenuItem_palmCenter_update_Click(this, new EventArgs());
                    else if (name == "dp2Gate")
                        MenuItem_dp2Gate_update_Click(this, new EventArgs());
                }
            }
            else
            {
                AppendSectionTitle("目前没有任何新版本需要更新");
            }
        }

#if NO
        private void MenuItem_dp2library_openDataDir_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            if (menu.HasDropDownItems == false)
            {
                AddMenuItem(menu, "dp2Library");
            }
        }
#endif

#if NO
        private void MenuItem_dp2kernel_openDataDir_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            if (menu.HasDropDownItems == false)
            {
                AddMenuItem(menu, "dp2Kernel");
            }
        }
#endif

        // 更新和 dp2OPAC 有关的菜单状态
        void Refresh_dp2OPAC_MenuItems()
        {
            // TODO: 检测操作系统版本和 IIS 版本，IIS 7 以上时才 Enable 安装 dp2OPAC 的菜单项

            this.MenuItem_dp2opac_openDataDir.DropDownItems.Clear();
            AddMenuItem(this.MenuItem_dp2opac_openDataDir, "dp2OPAC data");

            this.MenuItem_dp2opac_openVirtualDir.DropDownItems.Clear();
            AddMenuItem(this.MenuItem_dp2opac_openVirtualDir, "dp2OPAC virtual");
        }

#if NO
        private void MenuItem_dp2opac_openDataDir_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            if (menu.HasDropDownItems == false)
            {
                AddMenuItem(menu, "dp2OPAC data");
            }
        }

        private void MenuItem_dp2opac_openVirtualDir_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            if (menu.HasDropDownItems == false)
            {
                AddMenuItem(menu, "dp2OPAC virtual");
            }
        }
#endif

        void AddMenuItem(ToolStripMenuItem menuItem, string strProductName)
        {
            if (strProductName == "dp2OPAC data"
                || strProductName == "dp2OPAC virtual")
            {
                string strError = "";
                List<OpacAppInfo> infos = null;
                // 查找 dp2OPAC 路径
                // return:
                //      -1  出错
                //      其他  返回找到的路径个数
                int nRet = OpacAppInfo.GetOpacInfo(out infos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                foreach (OpacAppInfo info in infos)
                {
                    if (string.IsNullOrEmpty(info.PhysicalPath) == true)
                        continue;

                    if (strProductName == "dp2OPAC data")
                    {
                        ToolStripMenuItem subItem = new ToolStripMenuItem("'" + info.IisPath + "' - " + info.DataDir);
                        subItem.Tag = strProductName + "|" + info.DataDir;
                        subItem.Click += new EventHandler(subItem_Click);
                        menuItem.DropDownItems.Add(subItem);
                    }
                    else
                    {
                        ToolStripMenuItem subItem = new ToolStripMenuItem("'" + info.IisPath + "' - " + info.PhysicalPath);
                        subItem.Tag = strProductName + "|" + info.PhysicalPath;
                        subItem.Click += new EventHandler(subItem_Click);
                        menuItem.DropDownItems.Add(subItem);
                    }
                }

                if (menuItem.DropDownItems.Count > 0)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;
                return;
            ERROR1:
                menuItem.DropDownItems.Add(new ToolStripMenuItem(strError));
                if (menuItem.DropDownItems.Count > 0)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;

                return;
            }

            if (strProductName == "dp2ZServer")
            {
                string strDataDir = "";
                string strExePath = InstallHelper.GetPathOfService("dp2ZService");
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strExePath = StringUtil.Unquote(strExePath, "\"\"");
                    strDataDir = Path.GetDirectoryName(strExePath);
                    ToolStripMenuItem subItem = new ToolStripMenuItem(strDataDir);
                    subItem.Tag = strProductName + "|" + strDataDir;
                    subItem.Click += new EventHandler(subItem_Click);
                    menuItem.DropDownItems.Add(subItem);
                }
                return;
            }

            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertificatSN = "";

                string[] existing_urls = null;
                bool bRet = InstallHelper.GetInstanceInfo(strProductName,
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertificatSN);
                if (bRet == false)
                    break;
                ToolStripMenuItem subItem = new ToolStripMenuItem("'" + strInstanceName + "' - " + strDataDir);
                subItem.Tag = strProductName + "|" + strDataDir;
                subItem.Click += new EventHandler(subItem_Click);
                menuItem.DropDownItems.Add(subItem);
            }

            if (menuItem.DropDownItems.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
        }

        void subItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            string strText = menuItem.Tag as string;

            string strProductName = "";
            string strDataDir = "";
            StringUtil.ParseTwoPart(strText, "|", out strProductName, out strDataDir);

            if (string.IsNullOrEmpty(strDataDir) == false)
            {
                try
                {
                    System.Diagnostics.Process.Start(strDataDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
        }

        private void MenuItem_dp2library_openAppDir_Click(object sender, EventArgs e)
        {
            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");
                try
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
            else
            {
                MessageBox.Show(this, "dp2Library 未曾安装过");
            }
        }

        private void MenuItem_dp2kernel_openAppDir_Click(object sender, EventArgs e)
        {
            string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");
                try
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
            else
            {
                MessageBox.Show(this, "dp2Kernel 未曾安装过");
            }
        }

        private void MenuItem_zipDebugInfos_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;

            this._floatingMessage.Text = "正在打包事件日志信息 ...";
            try
            {
                string strZipFileName = Path.Combine(this.TempDir, "dp2installer_eventlog.zip");

                List<EventLog> logs = new List<EventLog>();

                logs.Add(new EventLog("DigitalPlatform", ".", "*"));
                logs.Add(new EventLog("Application"));

                // "最近31天" "最近十年" "最近七天"

                nRet = PackageEventLog(logs,
                    strZipFileName,
                    bControl ? "最近十年" : "最近31天",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                try
                {
                    System.Diagnostics.Process.Start(this.TempDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void BeginLoop(string strText)
        {

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strText);
            stop.BeginLoop();
        }

        void EndLoop()
        {
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");
            stop.HideProgress();


        }

        internal void DoStop(object sender, StopEventArgs e)
        {

        }

        void DisplayEventLog(EventLog Log)
        {
            try
            {
                foreach (EventLogEntry entry in Log.Entries)
                {
                    string strText = "*\r\n"
                        //+ "Machine Name:\t" + entry.MachineName + "\r\n"
                        + entry.Source + " \t"
                        //+ "Category:\t" + entry.Category + "\r\n"
                        + entry.EntryType.ToString() + " \t"
                        //+ "Event ID:\t" + entry.InstanceId.ToString() + "\r\n"
                        //+ "User Name:\t" + entry.UserName + "\r\n"
                        + entry.TimeGenerated.ToString() + "\r\n"
                        + entry.Message + "\r\n\r\n";

                    AppendString(strText);
                }
            }
            catch (Exception ex)
            {
                AppendString("显示日志信息时出现异常: " + ex.Message + "\r\n");
            }
        }

        int MakeWindowsLogFile(EventLog log,
            string strEventLogFilename,
            out string strError)
        {
            strError = "";
            int nLines = 0;
            try
            {
                if (stop != null)
                    stop.SetMessage("正在准备 Windows 事件日志 " + log.LogDisplayName + "...");

                using (StreamWriter sw = new StreamWriter(strEventLogFilename, false, Encoding.UTF8))
                {
                    foreach (EventLogEntry entry in log.Entries)
                    {
                        Application.DoEvents();
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strText = "*\r\n"
                            + entry.Source + " \t"
                            + entry.EntryType.ToString() + " \t"
                            + entry.TimeGenerated.ToString() + "\r\n"
                            + entry.Message + "\r\n\r\n";

                        sw.Write(strText);
                        nLines++;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "输出 Windows 日志 " + log.LogDisplayName + "的信息时出现异常: " + ex.Message;
                return -1;
            }

            return nLines;
        }

        int PackageEventLog(List<EventLog> logs,
            string strZipFileName,
            string strRangeName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.BeginLoop("正在打包事件日志 ...");
            Application.DoEvents();

            this.toolStripProgressBar_main.Style = ProgressBarStyle.Marquee;
            this.toolStripProgressBar_main.Visible = true;

            try
            {
#if NO
                string strTempDir = Path.Combine(this.UserDir, "temp");
                PathUtil.CreateDirIfNeed(strTempDir);
#endif
                string strTempDir = this.TempDir;

                PathUtil.TryClearDir(strTempDir);

                List<string> filenames = new List<string>();

                foreach (EventLog log in logs)
                {
                    // 创建 eventlog_digitalplatform.txt 文件
                    string strEventLogFilename = Path.Combine(strTempDir, "eventlog_" + log.LogDisplayName + ".txt");

                    //
                    //
                    nRet = MakeWindowsLogFile(log,
                        strEventLogFilename,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (nRet > 0)
                        filenames.Add(strEventLogFilename);
                    else
                        File.Delete(strEventLogFilename);
                }

                // 创建一个描述了安装的各个实例和环境情况的文件
                string strDescriptionFilename = Path.Combine(strTempDir, "description.txt");
                try
                {
                    if (stop != null)
                        stop.SetMessage("正在准备 description.txt 文件 ...");

                    using (StreamWriter sw = new StreamWriter(strDescriptionFilename, false, Encoding.UTF8))
                    {
                        sw.Write(GetEnvironmentDescription());
                    }
                }
                catch (Exception ex)
                {
                    strError = "输出 description.txt 时出现异常: " + ex.Message;
                    return -1;
                }

                filenames.Add(strDescriptionFilename);

                // TODO: 是否复制整个数据目录？ 需要避免复制日志文件和其他尺寸很大的文件

                // 复制错误日志文件和其他重要文件
                List<string> dates = MakeDates(strRangeName); // "最近31天""最近十年""最近七天"

                // *** dp2library 各个 instance
                string strLibraryTempDir = Path.Combine(strTempDir, "dp2library");
                PathUtil.TryCreateDir(strLibraryTempDir);

                for (int i = 0; ; i++)
                {
                    string strInstanceName = "";
                    string strDataDir = "";
                    string strCertificatSN = "";

                    string[] existing_urls = null;
                    bool bRet = InstallHelper.GetInstanceInfo("dp2Library",
                        i,
                        out strInstanceName,
                        out strDataDir,
                        out existing_urls,
                        out strCertificatSN);
                    if (bRet == false)
                        break;

                    string strInstanceDir = strLibraryTempDir;
                    if (string.IsNullOrEmpty(strInstanceName) == false)
                    {
                        strInstanceDir = Path.Combine(strLibraryTempDir, "instance_" + strInstanceName);
                        PathUtil.TryCreateDir(strInstanceDir);
                    }

                    // 复制 library.xml
                    {
                        string strFilePath = Path.Combine(strDataDir, "library.xml");
                        string strTargetFilePath = Path.Combine(strInstanceDir, "library.xml");
                        if (File.Exists(strFilePath) == true)
                        {
                            File.Copy(strFilePath,
                                strTargetFilePath);
                            filenames.Add(strTargetFilePath);
                        }
                    }

                    foreach (string date in dates)
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strFilePath = Path.Combine(strDataDir, "log/log_" + date + ".txt");
                        if (File.Exists(strFilePath) == false)
                            continue;
                        string strTargetFilePath = Path.Combine(strInstanceDir, "log_" + date + ".txt");

                        if (stop != null)
                            stop.SetMessage("正在复制文件 " + strFilePath);

                        File.Copy(strFilePath, strTargetFilePath);
                        filenames.Add(strTargetFilePath);
                    }
                }

                // dp2kernel 各个 instance
                string strKernelTempDir = Path.Combine(strTempDir, "dp2kernel");
                PathUtil.TryCreateDir(strKernelTempDir);

                for (int i = 0; ; i++)
                {
                    string strInstanceName = "";
                    string strDataDir = "";
                    string strCertificatSN = "";

                    string[] existing_urls = null;
                    bool bRet = InstallHelper.GetInstanceInfo("dp2Kernel",
                        i,
                        out strInstanceName,
                        out strDataDir,
                        out existing_urls,
                        out strCertificatSN);
                    if (bRet == false)
                        break;

                    string strInstanceDir = strKernelTempDir;
                    if (string.IsNullOrEmpty(strInstanceName) == false)
                    {
                        strInstanceDir = Path.Combine(strKernelTempDir, "instance_" + strInstanceName);
                        PathUtil.TryCreateDir(strInstanceDir);
                    }

                    // 复制 databases.xml
                    {
                        string strFilePath = Path.Combine(strDataDir, "databases.xml");
                        string strTargetFilePath = Path.Combine(strInstanceDir, "databases.xml");
                        if (File.Exists(strFilePath) == true)
                        {
                            File.Copy(strFilePath,
                                strTargetFilePath);
                            filenames.Add(strTargetFilePath);
                        }
                    }

                    foreach (string date in dates)
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strFilePath = Path.Combine(strDataDir, "log/log_" + date + ".txt");
                        if (File.Exists(strFilePath) == false)
                            continue;

                        string strTargetFilePath = Path.Combine(strInstanceDir, "log_" + date + ".txt");
                        if (stop != null)
                            stop.SetMessage("正在复制文件 " + strFilePath);

                        File.Copy(strFilePath, strTargetFilePath);
                        filenames.Add(strTargetFilePath);
                    }
                }

                // dp2opac 各个 instance
                string strOpacTempDir = Path.Combine(strTempDir, "dp2opac");
                PathUtil.TryCreateDir(strOpacTempDir);

                List<OpacAppInfo> infos = null;
                // 查找 dp2OPAC 路径
                // return:
                //      -1  出错
                //      其他  返回找到的路径个数
                nRet = OpacAppInfo.GetOpacInfo(out infos,
                    out strError);
                if (nRet == -1)
                {
                    // 可能是 IIS 没有安装
                }
                else
                {
                    List<OpacAppInfo> infos1 = new List<OpacAppInfo>();
                    foreach (OpacAppInfo info in infos)
                    {
                        if (string.IsNullOrEmpty(info.PhysicalPath) == true)
                            continue;
                        if (string.IsNullOrEmpty(info.DataDir) == true)
                            continue;
                        infos1.Add(info);
                    }

                    int index = 0;
                    foreach (OpacAppInfo info in infos1)
                    {
                        string strInstanceName = (index + 1).ToString();
                        string strDataDir = info.DataDir;

                        if (infos1.Count == 1)
                            strInstanceName = "";
                        else
                        {
                            if (string.IsNullOrEmpty(strInstanceName) == false)
                                strInstanceName += "_";
                        }

                        string strInstanceDir = strOpacTempDir;
                        if (string.IsNullOrEmpty(strInstanceName) == false
                            && infos1.Count != 1)
                        {
                            strInstanceDir = Path.Combine(strOpacTempDir, "instance_" + strInstanceName);
                            PathUtil.TryCreateDir(strInstanceDir);
                        }

                        // 复制 opac.xml
                        {
                            string strFilePath = Path.Combine(strDataDir, "opac.xml");
                            string strTargetFilePath = Path.Combine(strInstanceDir, "opac.xml");
                            if (File.Exists(strFilePath) == true)
                            {
                                File.Copy(strFilePath, strTargetFilePath);
                                filenames.Add(strTargetFilePath);
                            }
                        }

                        // 复制 webui.xml
                        {
                            string strFilePath = Path.Combine(strDataDir, "webui.xml");
                            string strTargetFilePath = Path.Combine(strInstanceDir, "webui.xml");
                            if (File.Exists(strFilePath) == true)
                            {
                                File.Copy(strFilePath,
                                    strTargetFilePath);
                                filenames.Add(strTargetFilePath);
                            }
                        }

                        foreach (string date in dates)
                        {
                            Application.DoEvents();

                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            string strFilePath = Path.Combine(strDataDir, "log/log_" + date + ".txt");
                            if (File.Exists(strFilePath) == false)
                                continue;

                            string strTargetFilePath = Path.Combine(strOpacTempDir, strInstanceName + "log_" + date + ".txt");
                            if (stop != null)
                                stop.SetMessage("正在复制文件 " + strFilePath);

                            File.Copy(strFilePath, strTargetFilePath);
                            filenames.Add(strTargetFilePath);
                        }

                        index++;
                    }
                }

                // *** palmcenter
                string strPalmDataDir = Utility.GetServiceUserDirectory("palmCenter");
                if (Directory.Exists(strPalmDataDir))
                {
                    string strPalmTempDir = Path.Combine(strTempDir, "palmcenter");
                    PathUtil.TryCreateDir(strPalmTempDir);

                    // settings.xml 文件
                    {
                        string strFilePath = Path.Combine(strPalmDataDir, "settings.xml");
                        string strTargetFilePath = Path.Combine(strPalmTempDir, "settings.xml");
                        if (File.Exists(strFilePath) == true)
                        {
                            File.Copy(strFilePath,
                                strTargetFilePath);
                            filenames.Add(strTargetFilePath);
                        }
                    }

                    foreach (string date in dates)
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strFilePath = Path.Combine(strPalmDataDir, "log/log_" + date + ".txt");
                        if (File.Exists(strFilePath) == false)
                            continue;
                        string strTargetFilePath = Path.Combine(strPalmTempDir, "log_" + date + ".txt");

                        if (stop != null)
                            stop.SetMessage("正在复制文件 " + strFilePath);

                        File.Copy(strFilePath, strTargetFilePath);
                        filenames.Add(strTargetFilePath);
                    }
                }

                if (filenames.Count == 0)
                    return 0;

                if (filenames.Count > 0)
                {
                    this.toolStripProgressBar_main.Style = ProgressBarStyle.Continuous;

                    bool bRangeSetted = false;
                    using (ZipFile zip = new ZipFile(Encoding.UTF8))
                    {
                        // http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                        // https://dotnetzip.codeplex.com/workitem/14087
                        // uncommenting the following line can be used as a work-around
                        zip.ParallelDeflateThreshold = -1;

                        foreach (string filename in filenames)
                        {
                            Application.DoEvents();

                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            string strShortFileName = filename.Substring(strTempDir.Length + 1);
                            if (stop != null)
                                stop.SetMessage("正在压缩 " + strShortFileName);
                            string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                            zip.AddFile(filename, directoryPathInArchive);
                        }

                        if (stop != null)
                            stop.SetMessage("正在写入压缩文件 ...");

                        Application.DoEvents();

                        zip.SaveProgress += (s, e) =>
                        {
                            Application.DoEvents();
                            if (stop != null && stop.State != 0)
                            {
                                e.Cancel = true;
                                return;
                            }

                            if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                            {
                                if (bRangeSetted == false)
                                {
                                    stop.SetProgressRange(0, e.EntriesTotal);
                                    bRangeSetted = true;
                                }

                                stop.SetProgressValue(e.EntriesSaved);
                            }
                        };

                        zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                        zip.Save(strZipFileName);

                        stop.HideProgress();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }
                    }

                    if (stop != null)
                        stop.SetMessage("正在删除中间文件 ...");

                    // 删除原始文件
                    foreach (string filename in filenames)
                    {
                        File.Delete(filename);
                    }

                    // 删除四个子目录
                    PathUtil.DeleteDirectory(Path.Combine(strTempDir, "dp2library"));
                    PathUtil.DeleteDirectory(Path.Combine(strTempDir, "dp2kernel"));
                    PathUtil.DeleteDirectory(Path.Combine(strTempDir, "dp2opac"));
                    PathUtil.DeleteDirectory(Path.Combine(strTempDir, "palmcenter"));
                }
            }
            finally
            {
                this.EndLoop();
                this.toolStripProgressBar_main.Style = ProgressBarStyle.Continuous;
            }

            return 0;
        }

        List<string> MakeDates(string strName)
        {
            List<string> filenames = new List<string>();

            string strStartDate = "";
            string strEndDate = "";

            if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7 - 1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                throw new Exception("无法识别的周期 '" + strName + "'");
            }

            string strWarning = "";
            string strError = "";
            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            int nRet = MakeDates(strStartDate,
                strEndDate,
        out filenames,
        out strWarning,
        out strError);
            if (nRet == -1)
                goto ERROR1;
#if NO
            if (string.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);
#endif

            return filenames;
        ERROR1:
            throw new Exception(strError);
        }

        static int MakeDates(string strStartDate,
    string strEndDate,
    out List<string> dates,
    out string strWarning,
    out string strError)
        {
            dates = new List<string>();
            strError = "";
            strWarning = "";
            int nRet = 0;

            if (String.Compare(strStartDate, strEndDate) > 0)
            {
                strError = "起始日期 '" + strStartDate + "' 不应大于结束日期 '" + strEndDate + "'。";
                return -1;
            }

            string strDate = strStartDate;

            for (; ; )
            {
                dates.Add(strDate);

                string strNextDate = "";
                // 获得（理论上）下一个日志文件名
                // return:
                //      -1  error
                //      0   正确
                //      1   正确，并且strLogFileName已经是今天的日子了
                nRet = GetNextDate(strDate,
                    out strNextDate,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    if (String.Compare(strDate, strEndDate) < 0)
                    {
                        strWarning = "因日期范围的尾部 " + strEndDate + " 超过今天(" + DateTime.Now.ToLongDateString() + ")，部分日期被略去...";
                        break;
                    }
                }

                Debug.Assert(strDate != strNextDate, "");

                strDate = strNextDate;
                if (String.Compare(strDate, strEndDate) > 0)
                    break;
            }

            return 0;
        }

        // 获得（理论上）下一个日志文件名
        // return:
        //      -1  error
        //      0   正确
        //      1   正确，并且 strNextDate 已经是今天的日子了
        static int GetNextDate(string strDate,
            out string strNextDate,
            out string strError)
        {
            strError = "";
            strNextDate = "";
            int nRet = 0;

#if NO
            string strYear = strDate.Substring(0, 4);
            string strMonth = strDate.Substring(4, 2);
            string strDay = strDate.Substring(6, 2);

            int nYear = 0;
            int nMonth = 0;
            int nDay = 0;

            try
            {
                nYear = Convert.ToInt32(strYear);
            }
            catch
            {
                strError = "日志文件名 '" + strDate + "' 中的 '"
                    + strYear + "' 部分格式错误";
                return -1;
            }

            try
            {
                nMonth = Convert.ToInt32(strMonth);
            }
            catch
            {
                strError = "日志文件名 '" + strDate + "' 中的 '"
                    + strMonth + "' 部分格式错误";
                return -1;
            }

            try
            {
                nDay = Convert.ToInt32(strDay);
            }
            catch
            {
                strError = "日志文件名 '" + strDate + "' 中的 '"
                    + strDay + "' 部分格式错误";
                return -1;
            }

            DateTime time = DateTime.Now;
            try
            {
                time = new DateTime(nYear, nMonth, nDay);
            }
            catch (Exception ex)
            {
                strError = "日期 " + strDate + " 格式错误: " + ex.Message;
                return -1;
            }
#endif
            DateTime time = DateTimeUtil.Long8ToDateTime(strDate);

            DateTime now = DateTime.Now;

            // 正规化时间
            nRet = RoundTime("day",
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = RoundTime("day",
                ref time,
                out strError);
            if (nRet == -1)
                return -1;

            bool bNow = false;
            if (time >= now)
                bNow = true;

            time = time + new TimeSpan(1, 0, 0, 0); // 后面一天

            strNextDate = time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0');

            if (bNow == true)
                return 1;

            return 0;
        }
        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            time = time.ToLocalTime();
            if (strUnit == "day")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            time = time.ToUniversalTime();

            return 0;
        }

        private void MenuItem_displayDigitalPlatformEventLog_Click(object sender, EventArgs e)
        {
            EventLog Log = new EventLog("DigitalPlatform", ".", "*");
            DisplayEventLog(Log);
        }

        public Version GetIisVersion()
        {
            using (RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
            {
                if (componentsKey != null)
                {
                    int majorVersion = (int)componentsKey.GetValue("MajorVersion", -1);
                    int minorVersion = (int)componentsKey.GetValue("MinorVersion", -1);

                    if (majorVersion != -1 && minorVersion != -1)
                    {
                        return new Version(majorVersion, minorVersion);
                    }
                }

                return new Version(0, 0);
            }
        }

        // 获得环境描述字符串
        string GetEnvironmentDescription()
        {
            string strError = "";

            StringBuilder text = new StringBuilder();
            text.Append("信息创建时间:\t" + DateTime.Now.ToString() + "\r\n");
            text.Append("当前操作系统信息:\t" + Environment.OSVersion.ToString() + "\r\n");
            text.Append("当前操作系统版本号:\t" + Environment.OSVersion.Version.ToString() + "\r\n");
            text.Append("本机 MAC 地址:\t" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress()) + "\r\n");
            text.Append("IIS 版本号:\t" + GetIisVersion().ToString() + "\r\n");

            // *** dp2library
            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                text.Append("\r\n*** dp2library\r\n");
                text.Append("可执行文件目录:\t" + Path.GetDirectoryName(strExePath) + "\r\n");

                for (int i = 0; ; i++)
                {
                    string strInstanceName = "";
                    string strDataDir = "";
                    string strCertificatSN = "";
                    string strSerialCode = "";

                    string[] existing_urls = null;
                    bool bRet = InstallHelper.GetInstanceInfo("dp2Library",
                        i,
                        out strInstanceName,
                        out strDataDir,
                        out existing_urls,
                        out strCertificatSN,
                        out strSerialCode,
                        out string style);
                    if (bRet == false)
                        break;

                    text.Append("\r\n实例 " + (i + 1) + "\r\n");
                    text.Append("实例名:\t" + strInstanceName + "\r\n");
                    text.Append("数据目录:\t" + strDataDir + "\r\n");
                    text.Append("协议绑定:\t" + StringUtil.MakePathList(existing_urls) + "\r\n");
                    text.Append("数字签名SN:\t" + strCertificatSN + "\r\n");
                    text.Append("序列号:\t" + strSerialCode + "\r\n");
                    text.Append("风格:\t" + style + "\r\n");
                }
            }

            // *** dp2kernel
            strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                text.Append("\r\n*** dp2kernel\r\n");
                text.Append("可执行文件目录:\t" + Path.GetDirectoryName(strExePath) + "\r\n");

                for (int i = 0; ; i++)
                {
                    string strInstanceName = "";
                    string strDataDir = "";
                    string strCertificatSN = "";
                    string strSerialCode = "";

                    string[] existing_urls = null;
                    bool bRet = InstallHelper.GetInstanceInfo("dp2Kernel",
                        i,
                        out strInstanceName,
                        out strDataDir,
                        out existing_urls,
                        out strCertificatSN,
                        out strSerialCode,
                        out string style);
                    if (bRet == false)
                        break;

                    text.Append("\r\n实例 " + (i + 1) + "\r\n");
                    text.Append("实例名:\t" + strInstanceName + "\r\n");
                    text.Append("数据目录:\t" + strDataDir + "\r\n");
                    text.Append("协议绑定:\t" + StringUtil.MakePathList(existing_urls) + "\r\n");
                    text.Append("数字签名SN:\t" + strCertificatSN + "\r\n");
                    text.Append("序列号:\t" + strSerialCode + "\r\n");
                    text.Append("风格:\t" + style + "\r\n");
                }
            }

            List<OpacAppInfo> infos = null;
            // 查找 dp2OPAC 路径
            // return:
            //      -1  出错
            //      其他  返回找到的路径个数
            int nRet = OpacAppInfo.GetOpacInfo(out infos,
                out strError);
            if (nRet == -1)
                text.Append("FindOpacPath() error :" + strError);
            else
            {
                List<OpacAppInfo> infos1 = new List<OpacAppInfo>();
                foreach (OpacAppInfo info in infos)
                {
                    if (string.IsNullOrEmpty(info.PhysicalPath) == true)
                        continue;
                    if (string.IsNullOrEmpty(info.DataDir) == true)
                        continue;
                    infos1.Add(info);
                }

                if (infos.Count > 0)
                {
                    text.Append("\r\n*** dp2OPAC\r\n");

                    int index = 0;
                    foreach (OpacAppInfo info in infos1)
                    {
                        string strDataDir = info.DataDir;

                        text.Append("\r\n实例 " + (index + 1) + "\r\n");
                        text.Append("实例名:\t" + info.IisPath + "\r\n");
                        text.Append("可执行文件目录:\t" + info.PhysicalPath + "\r\n");
                        text.Append("数据目录:\t" + info.DataDir + "\r\n");

                        index++;
                    }
                }
            }
            return text.ToString();
        }

        private void MenuItem_dp2kernel_instanceManagement_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;
            bool bInstalled = true;

            this._floatingMessage.Text = "正在配置 dp2kernel 实例 ...";

            try
            {
                AppendSectionTitle("配置实例开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    if (bControl == false)
                    {
                        strError = "dp2kernel 未曾安装过";
                        goto ERROR1;
                    }
                    bInstalled = false;
                }

                //strExePath = Unquote(strExePath);
                //string strRootDir = Path.GetDirectoryName(strExePath);

                // MessageBox.Show(this, "为进行配置，将首先停止 dp2library 服务。配置完成后 dp2library 会重新启动");

#if NO
                if (bInstalled == true)
                {
                    AppendString("正在停止 dp2kernel 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2KernelService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2kernel 服务已经停止\r\n");
                }
#endif

                try
                {
                    DigitalPlatform.rms.InstanceDialog dlg = new DigitalPlatform.rms.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);

                    // dlg.SourceDir = strRootDir;
                    dlg.DataZipFileName = Path.Combine(this.DataDir, "kernel_data.zip");
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    //if (dlg.DialogResult == DialogResult.Cancel)
                    //    return;

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("管理实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }
                }
                finally
                {
#if NO
                    if (bInstalled == true)
                    {
                        string strError1 = "";
                        AppendString("正在重新启动 dp2kernel 服务 ...\r\n");
                        nRet = InstallHelper.StartService("dp2KernelService",
        out strError1);
                        if (nRet == -1)
                        {
                            AppendString("dp2kernel 服务启动失败: " + strError1 + "\r\n");
                            MessageBox.Show(this, strError1);
                        }
                        else
                        {
                            AppendString("dp2kernel 服务启动成功\r\n");
                        }
                    }
#endif

                    AppendSectionTitle("配置实例结束");
                    Refresh_dp2kernel_MenuItems();
                }

            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        // 刷新菜单状态
        void Refresh_dp2kernel_MenuItems()
        {
            string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                this.MenuItem_dp2kernel_install.Enabled = true;
                this.MenuItem_dp2kernel_update.Enabled = false;
            }
            else
            {
                this.MenuItem_dp2kernel_install.Enabled = false;
                this.MenuItem_dp2kernel_update.Enabled = true;
            }

            this.MenuItem_dp2kernel_openDataDir.DropDownItems.Clear();
            AddMenuItem(MenuItem_dp2kernel_openDataDir, "dp2Kernel");
        }

        // 刷新菜单状态
        void Refresh_dp2library_MenuItems()
        {
            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                this.MenuItem_dp2library_install.Enabled = true;
                this.MenuItem_dp2library_update.Enabled = false;
            }
            else
            {
                this.MenuItem_dp2library_install.Enabled = false;
                this.MenuItem_dp2library_update.Enabled = true;
            }

            this.MenuItem_dp2library_openDataDir.DropDownItems.Clear();
            AddMenuItem(MenuItem_dp2library_openDataDir, "dp2Library");
        }

        // 刷新菜单状态
        void Refresh_dp2ZServer_MenuItems()
        {
            string strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                this.MenuItem_dp2ZServer_install.Enabled = true;
                this.MenuItem_dp2ZServer_update.Enabled = false;
            }
            else
            {
                this.MenuItem_dp2ZServer_install.Enabled = false;
                this.MenuItem_dp2ZServer_update.Enabled = true;
            }

            this.MenuItem_dp2ZServer_openDataDir.DropDownItems.Clear();
            AddMenuItem(MenuItem_dp2ZServer_openDataDir, "dp2ZServer");
        }

        // 获得 dpkernel 或 dp2library 的程序存储目录
        // 在 64 位操作系统下，获得 Program files (x86)
        // 在 32 位操作系统下，获得 Program Files
        // 目前 dp2kernel 和 dp2library 在 64 位操作系统下还都是 32 位的模块
        public static string GetProductDirectory(
            string strProduct,
            string strCompany = "digitalplatform")
        {
            string strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(strProgramDir) == true)
                strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            Debug.Assert(string.IsNullOrEmpty(strProgramDir) == false, "");

            return Path.Combine(strProgramDir, strCompany + "\\" + strProduct);
        }

        // 首次安装 dp2kernel
        private void MenuItem_dp2kernel_install_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在安装 dp2kernel - 数据库内核 ...";

            try
            {
                AppendSectionTitle("安装 dp2kernel 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strError = "dp2kernel 已经安装过了，不能重复安装";
                    goto ERROR1;
                }
                // strExePath = Unquote(strExePath);

                // program files (x86)/digitalplatform/dp2kernel
                string strProgramDir = GetProductDirectory("dp2kernel");

                PathUtil.TryCreateDir(strProgramDir);

                string strZipFileName = Path.Combine(this.DataDir, "kernel_app.zip");

                AppendString("安装可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 创建实例
                AppendString("创建实例 ...\r\n");

                try
                {
                    DigitalPlatform.rms.InstanceDialog dlg = new DigitalPlatform.rms.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);

                    // dlg.SourceDir = strRootDir;
                    dlg.DataZipFileName = Path.Combine(this.DataDir, "kernel_data.zip");
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

#if NO
                    // TODO: 是否必须要创建至少一个实例?
                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        AppendSectionTitle("放弃创建实例 ...");
                        return;
                    }
#endif

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("创建实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }

                }
                finally
                {
                    AppendString("创建实例结束 ...\r\n");
                }


                // 注册为 Windows Service
                strExePath = Path.Combine(strProgramDir, "dp2kernel.exe");

#if NO
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { strExePath });
            }
            catch (Exception ex)
            {
                strError = "注册 Windows Service 的过程发生错误: " + ex.Message;
                goto ERROR1;
            }
#endif
                AppendString("注册 Windows Service ...\r\n");

                nRet = InstallService(strExePath,
        true,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
            AppendString("启动 dp2kernel 服务 ...\r\n");
            nRet = StartService("dp2KernelService",
    out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2kernel 服务启动成功\r\n");
#endif

                AppendSectionTitle("安装 dp2kernel 结束");
                Refresh_dp2kernel_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2kernel_installService_Click(object sender, EventArgs e)
        {
            InstallOrUninstallService("dp2Kernel", true);

#if NO
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注册 Windows Service 开始");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strError = "dp2kernel 已经注册为 Windows Service，无法重复进行注册";
                goto ERROR1;
            }

            string strProgramDir = GetProductDirectory("dp2kernel");

            strExePath = Path.Combine(strProgramDir, "dp2kernel.exe");

            if (File.Exists(strExePath) == false)
            {
                strError = "dp2kernel.exe 尚未复制到目标位置，无法进行注册";
                goto ERROR1;
            }

            // 注册 Windows Service
#if NO
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { strExePath });
            }
            catch (Exception ex)
            {
                strError = "注册 Windows Service 的过程发生错误: " + ex.Message;
                goto ERROR1;
            }
#endif
            nRet = InstallService(strExePath,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注册 Windows Service 结束");

            this.Refresh_dp2kernel_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_dp2kernel_uninstallService_Click(object sender, EventArgs e)
        {
            InstallOrUninstallService("dp2Kernel", false);

#if NO
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注销 Windows Service 开始");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2kernel 尚未安装和注册为 Windows Service，无法进行注销";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            // 注销 Windows Service

#if NO
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { "/u", strExePath });
            }
            catch (Exception ex)
            {
                strError = "注销 Windows Service 的过程发生错误: " + ex.Message;
                goto ERROR1;
            }
#endif
            nRet = InstallService(strExePath,
    false,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注销 Windows Service 结束");

            this.Refresh_dp2kernel_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_dp2kernel_startService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2kernel 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在启动 dp2kernel 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StartService("dp2KernelService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2kernel 服务成功启动\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2kernel_stopService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2kernel 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在停止 dp2kernel 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StopService("dp2KernelService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2kernel 服务已经停止\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public static int InstallService(string fullFileName,
bool bInstall,
out string strError)
        {
            // http://stackoverflow.com/questions/20938531/managedinstallerclass-installhelper-is-locking-winservice-exe-file
            // ManagedInstallerClass.InstallHelper is locking WinService exe file
            Isolated<InstallServiceWork> isolated = new Isolated<InstallServiceWork>();
            try
            {
                strError = isolated.Value.InstallService(new Parameters { ExePath = fullFileName, Install = bInstall });
                if (string.IsNullOrEmpty(strError) == true)
                    return 0;
                return -1;
            }
            finally
            {
                isolated.Dispose();
            }
        }

        // 首次安装 dp2library
        private void MenuItem_dp2library_install_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在安装 dp2library - 图书馆应用服务器 ...";
            try
            {

                AppendSectionTitle("安装 dp2library 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strError = "dp2library 已经安装过了，不能重复安装";
                    goto ERROR1;
                }
                // strExePath = Unquote(strExePath);

                // program files (x86)/digitalplatform/dp2library
                string strProgramDir = GetProductDirectory("dp2library");

                PathUtil.TryCreateDir(strProgramDir);

                string strZipFileName = Path.Combine(this.DataDir, "library_app.zip");

                AppendString("安装可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 还需要把 library_data.zip 的内容都展开到程序目录的 temp 子目录中，以兼容以前的 Installer 功能
                string strTempDir = Path.Combine(strProgramDir, "temp");
                PathUtil.TryCreateDir(strTempDir);
                nRet = dp2Library_extractPartDir(strTempDir,
        "cfgs,templates,other",
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                List<string> new_instance_names = null;

                // 创建实例
                AppendString("创建实例 ...\r\n");

                try
                {
                    DigitalPlatform.LibraryServer.InstanceDialog dlg = new DigitalPlatform.LibraryServer.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.LockingInstances = this.locking_instances;
                    dlg.TempDir = this.TempDir;
                    // dlg.DataZipFileName = Path.Combine(this.DataDir, "library_data.zip");
                    dlg.CopyFiles += dlg_dp2Library_CopyFiles;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);   // ForegroundWindow.Instance

#if NO
                    // TODO: 是否必须要创建至少一个实例?
                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        AppendSectionTitle("放弃创建实例 ...");
                        return;
                    }
#endif

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("创建实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }

                    new_instance_names = dlg.NewInstanceNames;
                }
                finally
                {
                    AppendString("创建实例结束 ...\r\n");
                }


                // 注册为 Windows Service
                strExePath = Path.Combine(strProgramDir, "dp2library.exe");

                AppendString("注册 Windows Service ...\r\n");

                nRet = InstallService(strExePath,
        true,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendSectionTitle("安装 dp2library 结束");
                Refresh_dp2library_MenuItems();

                CreateDefaultDatabases(new_instance_names);
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void CreateDefaultDatabases(List<string> new_instance_names)
        {
            string strError = "";
            // 创建最初的书目库
            // 每个实例的 library.xml 中都记载了是否首次创建过书目库
            if (new_instance_names != null
                && new_instance_names.Count > 0)
            {
                AppendSectionTitle("创建初始书目库开始");

                foreach (string strInstanceName in new_instance_names)
                {
                    AppendString("为实例 '" + strInstanceName + "' 创建初始书目库\r\n");
                    // return:
                    //      -1  出错
                    //      0   没有必要创建
                    //      1   成功创建
                    int nRet = CreateDefaultDatabases(strInstanceName, out strError);
                    if (nRet == -1)
                    {
                        AppendString(strError + "\r\n");
                        MessageBox.Show(this, strError);
                        continue;
                    }
                    if (nRet == 0)
                        AppendString(strError + "\r\n");
                }

                AppendSectionTitle("创建初始书目库结束");
            }
        }

        void dlg_dp2Library_CopyFiles(object sender, CopyFilesEventArgs e)
        {
            string strError = "";
            int nRet = dp2Library_extractPartDir(
                e.DataDir,
                e.Action,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }
        }

        private void MenuItem_dp2library_startService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.locking_instances.Count > 0)
            {
                strError = "目前有下列 dp2library 实例(" + StringUtil.MakePathList(this.locking_instances) + ")处于锁定状态，不允许此时启动或者停止 dp2library";
                goto ERROR1;
            }

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2library 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在启动 dp2library 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StartService("dp2LibraryService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2library 服务成功启动\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2library_stopService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.locking_instances.Count > 0)
            {
                strError = "目前有下列 dp2library 实例(" + StringUtil.MakePathList(this.locking_instances) + ")处于锁定状态，不允许此时启动或者停止 dp2library";
                goto ERROR1;
            }

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2library 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在停止 dp2library 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StopService("dp2LibraryService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2library 服务已经停止\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2library_installService_Click(object sender, EventArgs e)
        {
            InstallOrUninstallService("dp2Library", true);
#if NO
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注册 Windows Service 开始");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strError = "dp2library 已经注册为 Windows Service，无法重复进行注册";
                goto ERROR1;
            }

            string strProgramDir = GetProductDirectory("dp2library");
            strExePath = Path.Combine(strProgramDir, "dp2library.exe");

            if (File.Exists(strExePath) == false)
            {
                strError = "dp2library.exe 尚未复制到目标位置，无法进行注册";
                goto ERROR1;
            }

            // 注册 Windows Service
            nRet = InstallService(strExePath,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注册 Windows Service 结束");

            this.Refresh_dp2library_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        // 2017/8/26
        void InstallOrUninstallService(string strName, bool bInstall)
        {
            string strError = "";
            int nRet = 0;

            if (strName == "dp2Library")
            {
                if (this.locking_instances.Count > 0)
                {
                    strError = "目前有下列 dp2library 实例(" + StringUtil.MakePathList(this.locking_instances) + ")处于锁定状态，不允许此时启动或者停止 dp2library";
                    goto ERROR1;
                }
            }

            if (bInstall)
                AppendSectionTitle("注册 Windows Service 开始");
            else
                AppendSectionTitle("注销 Windows Service 开始");

            Application.DoEvents();

            string strServiceName = strName + "Service";
            if (strName == "dp2ZServer")
                strServiceName = "dp2ZService";

            string strExePath = InstallHelper.GetPathOfService(strServiceName);
            if (bInstall == true)
            {
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strError = strName + " 已经注册为 Windows Service，无法重复进行注册";
                    goto ERROR1;
                }
                // program files (x86)/digitalplatform/dp2capo
                string strProgramDir = GetProductDirectory(strName);

                strExePath = Path.Combine(strProgramDir, strName + ".exe");
                if (File.Exists(strExePath) == false)
                {
                    strError = strName + ".exe 尚未复制到目标位置 '" + strExePath + "'，无法进行注册";
                    goto ERROR1;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = strName + " 尚未安装和注册为 Windows Service，无法进行注销";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                // 先停止 Service
                nRet = InstallHelper.StopService(strServiceName,
        out strError);
                if (nRet == -1)
                    MessageBox.Show(this, "停止 Service 时出错" + strError);
            }

            nRet = InstallService(strExePath,
                bInstall,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (bInstall)
                AppendSectionTitle("注册 Windows Service 结束");
            else
                AppendSectionTitle("注销 Windows Service 结束");

            if (strName == "dp2Library")
                this.Refresh_dp2library_MenuItems();
            if (strName == "dp2Kernel")
                this.Refresh_dp2kernel_MenuItems();
            if (strName == "dp2ZServer")
                this.Refresh_dp2ZServer_MenuItems();

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        private void MenuItem_dp2library_uninstallService_Click(object sender, EventArgs e)
        {
            InstallOrUninstallService("dp2Library", false);

#if NO
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注销 Windows Service 开始");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2library 尚未安装和注册为 Windows Service，无法进行注销";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            // testing
            nRet = InstallHelper.StopService("dp2LibraryService",
    out strError);
            if (nRet == -1)
                MessageBox.Show(this, "停止 Service 时出错" + strError);

            // 注销 Windows Service
            nRet = InstallService(strExePath,
    false,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注销 Windows Service 结束");

            this.Refresh_dp2library_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        // 卸载 dp2kernel
        // TODO： 可以仿照卸载 dp2library 过程重新编写。到底是先注销 Service，还是先删除数据实例 ?
        private void MenuItem_dp2kernel_uninstall_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在卸载 dp2kernel - 数据库内核 ...";

            try
            {
                AppendSectionTitle("卸载 dp2Kernel 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2KernelService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2kernel 尚未安装和注册为 Windows Service，无法进行注销";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

#if NO
                {
                    AppendString("正在停止 dp2kernel 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2KernelService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2kernel 服务已经停止\r\n");
                }
#endif

#if NO
            DialogResult result = MessageBox.Show(this,
"确实要卸载 dp2Kernel? ",
"dp2Installer",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;   // cancelled
#endif


                {
                    DigitalPlatform.rms.InstanceDialog dlg = new DigitalPlatform.rms.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.Text = "dp2Kernel - 彻底卸载所有实例和数据目录";
                    dlg.Comment = "下列实例将被全部卸载。请仔细确认。一旦卸载，全部数据目录、数据库和实例信息将被删除，并且无法恢复。";
                    dlg.UninstallMode = true;
                    dlg.SourceDir = ""; // Path.GetDirectoryName(strExePath);
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
#if NO
                        MessageBox.Show(this,
                            "已放弃卸载全部实例和数据目录。仅仅卸载了执行程序。");

#endif
                        AppendString("放弃卸载\r\n");
                        return;
                        //strError = "放弃卸载";
                        //goto ERROR1;
                    }
                    else
                    {
                        AppendString("已删除全部数据目录\r\n");
                    }
                }

                // 探测 .exe 是否为新版本。新版本中 Installer.Uninstall 动作不会删除数据目录

                {
                    AppendString("正在停止 dp2kernel 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2KernelService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2kernel 服务已经停止\r\n");
                }

                AppendString("注销 Windows Service\r\n");

                // 注销 Windows Service
                nRet = InstallService(strExePath,
        false,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("删除程序目录\r\n");

            // 删除程序目录
            REDO_DELETE_PROGRAMDIR:
                try
                {
                    PathUtil.DeleteDirectory(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    "删除程序目录 '" + Path.GetDirectoryName(strExePath) + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
    "卸载 dp2Kernel",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_DELETE_PROGRAMDIR;
                }

                AppendSectionTitle("卸载 dp2Kernel 结束");
                this.Refresh_dp2kernel_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*
         * 制作一个 InstanceDialog，可以允许管理多个实例。
         * 和 dp2kernel dp2library 不同，每个实例都有单独的 App 目录。
         * 
         * 
         * 
         * 
         * 
         * */
        private void MenuItem_dp2opac_install_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (Environment.OSVersion.Version.Major < 6)
            {
                strError = "dp2Installer 仅支持在 Windows Server 2008/2012 以及 Windows Vista/7/8 和以上版本的操作系统中安装 dp2OPAC";
                goto ERROR1;
            }

            this._floatingMessage.Text = "正在安装 dp2OPAC - 读者公共查询...";
            try
            {
                // 检查 IIS 是否已经启用
                nRet = OpacAppInfo.GetOpacInfo(out List<OpacAppInfo> infos, out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendSectionTitle("安装 dp2OPAC 开始");

                AppendString("停止 AppPool 'dp2OPAC'\r\n");

                Application.DoEvents();

                // 启动或者停止 AppPool
                nRet = OpacAppInfo.StartAppPool("dp2OPAC",
                    false,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                try
                {
                    DigitalPlatform.OPAC.InstanceDialog dlg = new DigitalPlatform.OPAC.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);

                    dlg.CopyFiles += dlg_dp2OPAC_CopyFiles;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult == DialogResult.Cancel)
                        return;

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("创建实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }
                }
                finally
                {
                    AppendString("重新启动 AppPool 'dp2OPAC'\r\n");
                    // 启动或者停止 AppPool
                    nRet = OpacAppInfo.StartAppPool("dp2OPAC",
                        true,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);

                    Refresh_dp2OPAC_MenuItems();
                    AppendSectionTitle("安装 dp2OPAC 结束");
                }
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        void dlg_dp2OPAC_CopyFiles(object sender, CopyFilesEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(e.Action == "app" || e.Action == "data", "");

            if (e.Action == "app")
            {
                string strZipFileName = Path.Combine(this.DataDir, "opac_app.zip");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    e.DataDir,
                    null,   // 包括了 web.config
                    out strError);
                if (nRet == -1)
                {
                    e.ErrorInfo = strError;
                    return;
                }

#if NO
                // 从 opac_style.zip 中展开部分目录内容
                string strTargetPath = Path.Combine(e.DataDir, "style");
                if (Directory.Exists(strTargetPath) == true)
                {
                    strZipFileName = Path.Combine(this.DataDir, "opac_style.zip");

                    nRet = dp2OPAC_extractDir(
                        false,
                        strZipFileName,
                        strTargetPath,
                        out strError);
                    if (nRet == -1)
                    {
                        e.ErrorInfo = strError;
                        return;
                    }
                }
#endif
            }

            if (e.Action == "data")
            {
                string strZipFileName = Path.Combine(this.DataDir, "opac_data.zip");
                try
                {
                    using (ZipFile zip = ZipFile.Read(strZipFileName))
                    {
                        for (int i = 0; i < zip.Count; i++)
                        {
                            ZipEntry e1 = zip[i];

                            string strPart = GetSubPath(e1.FileName);
                            string strFullPath = Path.Combine(e.DataDir, strPart);

                            e1.FileName = strPart;

                            e1.Extract(e.DataDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    e.ErrorInfo = strError;
                    return;
                }

                // 从 opac_style.zip 中展开部分目录内容
                string strTargetPath = Path.Combine(e.DataDir, "style");
                if (Directory.Exists(strTargetPath) == true)
                {
                    strZipFileName = Path.Combine(this.DataDir, "opac_style.zip");

                    nRet = dp2OPAC_extractDir(
                        false,
                        strZipFileName,
                        strTargetPath,
                        true,   // 需要避开 .css.macro 文件的 .css 文件
                        out strError);
                    if (nRet == -1)
                    {
                        e.ErrorInfo = strError;
                        return;
                    }
                }
            }
        }

        // 安装/启用 IIS
        private void MenuItem_tools_enableIIS_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = 0;

            // http://stackoverflow.com/questions/5936719/calling-dism-exe-from-system-diagnostics-process-fails
            string strFileName = "%WINDIR%\\SysNative\\dism.exe";
            strFileName = Environment.ExpandEnvironmentVariables(strFileName);

            var featureNames = new[] 
    {
        "IIS-ApplicationDevelopment",
        "IIS-CommonHttpFeatures",
        "IIS-DefaultDocument",
        "IIS-ISAPIExtensions",
        "IIS-ISAPIFilter",
        "IIS-ManagementConsole",
        //"IIS-NetFxExtensibility",
        "IIS-RequestFiltering",
        "IIS-Security",
        "IIS-StaticContent",
        "IIS-WebServer",
        "IIS-WebServerRole",
        "IIS-NetFxExtensibility45",
        "IIS-ASPNET45",
    };

            string strLine = string.Format(
            "/NoRestart /Online /Enable-Feature {0}",
            string.Join(
                " ",
                featureNames.Select(name => string.Format("/FeatureName:{0}", name))));

            AppendSectionTitle("开始启用 IIS");
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
                    goto ERROR1;
                AppendString(strError);
            }
            finally
            {
                AppendSectionTitle("结束启用 IIS");

                this.Cursor = oldCursor;
                this.Enabled = true;
            }

            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
#endif
            string strError = "";
            int nRet = 0;

            var featureNames = new[]
    {
        "IIS-ApplicationDevelopment",
        "IIS-CommonHttpFeatures",
        "IIS-DefaultDocument",
        "IIS-ISAPIExtensions",
        "IIS-ISAPIFilter",
        "IIS-ManagementConsole",
        //"IIS-NetFxExtensibility",
        "IIS-RequestFiltering",
        "IIS-Security",
        "IIS-StaticContent",
        "IIS-WebServer",
        "IIS-WebServerRole",
        "IIS-NetFxExtensibility45",
        "IIS-ASPNET45",
    };
            nRet = EnableServerFeature("IIS",
                featureNames,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        #region 安装 dp2library 以后创建书目库的相关函数

        // 出现提示
        // return:
        //      true    继续
        //      false   放弃
        bool PromptCreating(string strText)
        {
            DialogResult result = MessageBox.Show(this,
"安装程序可自动为实例 '" + _info.InstanceName + "' " + strText + "\r\n\r\n请问要创建么? ",
"安装 dp2Library",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                return true;
            return false;
        }

        // 创建缺省的几个数据库
        // TODO: 把过程显示在控制台
        // TODO: 在准备好执行以前，出现一个 MessageBox 汇总提醒将要进行的操作，此时应可以放弃操作。这样就起到了让安装者可以选择是否创建缺省书目库的作用。如果是安装后需要从 dt1000 利用工具软件直接升级已有数据，操作者可以跳过创建缺省书目库这一步
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        int CreateDefaultDatabases(string strInstanceName,
            out string strError)
        {
            strError = "";

            int nRet = PrepareSearch(strInstanceName, out strError);
            if (nRet == -1)
                return -1;

            if (_info.InitialDatabase == true)
            {
                strError = "以前已经创建过了，本次没有再创建";
                return 0;
            }

            EnableControls(false);

            Stop.OnStop += new StopEventHandler(this.DoLibraryStop);
            Stop.Initial("正在创建数据库 ...");
            Stop.BeginLoop();

            try
            {
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                nRet = ManageHelper.CreateDefaultDatabases(Channel,
                    Stop,
                    PromptCreating,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建初始书目库的过程出错: " + strError;
                    return -1;
                }
                if (nRet == 0)
                    return 0;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoLibraryStop);
                Stop.Initial("");

                EnableControls(true);

                EndSearch();
            }

            AppendString("为标记 library.xml 文件，正在停止实例 '" + strInstanceName + "' ...\r\n");
            Application.DoEvents();

#if NO
            nRet = InstallHelper.StopService("dp2LibraryService",
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                AppendString("dp2library 服务已经停止\r\n");
#endif
            nRet = StartOrStopOneDp2libraryInstance(strInstanceName,
            "stop",
            out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                AppendString("dp2library 实例 '" + strInstanceName + "' 已经停止\r\n");

            // 在 library.xml 中标记，已经创建过初始书目库了
            // return:
            //      -1  出错
            //      0   成功
            nRet = LibraryInstallHelper.MaskDefaultDatabaseCreated(_info.DataDir, out strError);
            if (nRet == -1)
            {
                strError = "标记初始书目库已经创建的过程出错: " + strError;
                return -1;
            }

            AppendString("正在启动实例 '" + strInstanceName + "' ...\r\n");
            Application.DoEvents();

#if NO
            nRet = InstallHelper.StartService("dp2LibraryService",
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                AppendString("dp2library 服务成功启动\r\n");
#endif
            nRet = StartOrStopOneDp2libraryInstance(strInstanceName,
"start",
out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                AppendString("dp2library 实例 '" + strInstanceName + "' 成功启动\r\n");


            return 1;
        }

        int StartOrStopOneDp2libraryInstance(string strInstanceName,
            string strAction,
            out string strError)
        {
            strError = "";

            int nRet = DigitalPlatform.LibraryServer.InstanceDialog.dp2library_serviceControl(
strAction,
strInstanceName,
out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        void DoLibraryStop(object sender, StopEventArgs e)
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
            this.menuStrip1.Enabled = bEnable;
        }

        string EncryptKey = "dp2installer_client_password_key";

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
        /// 通讯通道。MainForm 自己使用
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        // 当前正在使用的 dp2library instance 信息
        LibraryInstanceInfo _info = null;

        static string GetFirstUrl(string[] urls)
        {
            if (urls == null || urls.Length == 0)
                return "";

            foreach (string url in urls)
            {
                Uri uri = new Uri(url);
                // 跳过 basic.http
                if (uri.Scheme.ToLower() == "basic.http")
                    continue;
                return url;
            }

            return "";
        }

        // 准备进行检索
        // return:
        //      -1  出错
        //      0   成功
        public int PrepareSearch(string strInstanceName,
            out string strError)
        {
            strError = "";
            // 从注册表和 library.xml 文件中获得实例信息
            // parameters:
            //      
            // return:
            //      -1  出错
            //      0   实例没有找到
            //      1   成功
            int nRet = LibraryInstallHelper.GetLibraryInstanceInfo(
                strInstanceName,
                out _info,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return -1;

            if (this.Channel == null)
                this.Channel = new LibraryChannel();

            this.Channel.Url = GetFirstUrl(_info.Urls);

            this.Channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);

            Stop = new DigitalPlatform.Stop();
            Stop.Register(stopManager, true);	// 和容器关联

            return 0;
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
                e.UserName = _info.SupervisorUserName;   //  "supervisor";

                // e.Password = _info.SupervisorPassword;
                e.Password = "";    // 一般来说第一次登录会失败了

                string strLocation = "manager";
                e.Parameters = "location=" + strLocation;
                e.Parameters += ",client=dp2installer|" + ClientInfo.ClientVersion;

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
            e.Parameters += ",client=dp2installer|" + ClientInfo.ClientVersion;

            e.SavePasswordLong = dlg.SavePasswordLong;
            if (e.LibraryServerUrl != dlg.ServerUrl)
            {
                e.LibraryServerUrl = dlg.ServerUrl;
                // _expireVersionChecked = false;
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
            GuiUtil.AutoSetDefaultFont(dlg);

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
                dlg.ServerUrl = GetFirstUrl(_info.Urls);
            }
            else
            {
                dlg.ServerUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;

            if (String.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;

            dlg.SupervisorMode = true;

            dlg.Comment = strComment;
            dlg.UserName = ClientInfo.Config.Get(
                "default_account",
                "username",
                "supervisor");

            dlg.SavePasswordShort =
                ClientInfo.Config.GetBoolean(
                "default_account",
                "savepassword_short",
                false);

            dlg.SavePasswordLong =
                ClientInfo.Config.GetBoolean(
                "default_account",
                "savepassword_long",
                false);

            if (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true)
            {
                dlg.Password = ClientInfo.Config.Get(
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
            dlg.OperLocation = ClientInfo.Config.Get(
                "default_account",
                "location",
                "");

            ClientInfo.MemoryState(dlg, "memory_state", "logindlg_state");
            //this.AppInfo.LinkFormState(dlg,
            //    "logindlg_state");

            if (fail_contidion == LoginFailCondition.PasswordError
                && dlg.SavePasswordShort == false
                && dlg.SavePasswordLong == false)
                dlg.AutoShowShortSavePasswordTip = true;

            dlg.ShowDialog(owner);

            // this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            ClientInfo.Config.Set(
                "default_account",
                "username",
                dlg.UserName);
            ClientInfo.Config.Set(
                "default_account",
                "password",
                (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true) ?
                this.EncryptPassword(dlg.Password) : "");

            ClientInfo.Config.SetBoolean(
    "default_account",
    "savepassword_short",
    dlg.SavePasswordShort);

            ClientInfo.Config.SetBoolean(
                "default_account",
                "savepassword_long",
                dlg.SavePasswordLong);

            ClientInfo.Config.Set(
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

        #endregion

        // 卸载 dp2library
        private void MenuItem_dp2library_uninstall_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.locking_instances.Count > 0)
            {
                strError = "目前有下列 dp2library 实例(" + StringUtil.MakePathList(this.locking_instances) + ")处于锁定状态，无法卸载 dp2library";
                goto ERROR1;
            }

            this._floatingMessage.Text = "正在卸载 dp2library - 图书馆应用服务器 ...";
            try
            {
                AppendSectionTitle("卸载 dp2library 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2LibraryService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2library 未曾安装过";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

#if NO
                {
                    AppendString("正在停止 dp2library 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2LibraryService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2library 服务已经停止\r\n");
                }
#endif

                try
                {
                    DigitalPlatform.LibraryServer.InstanceDialog dlg = new DigitalPlatform.LibraryServer.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.LockingInstances = this.locking_instances;
                    dlg.TempDir = this.TempDir;
                    dlg.Text = "dp2Library - 彻底卸载所有实例和数据目录";
                    dlg.Comment = "下列实例将被全部卸载。请仔细确认。一旦卸载，全部数据目录、数据库和实例信息将被删除，并且无法恢复。";
                    dlg.UninstallMode = true;
                    dlg.CopyFiles += dlg_dp2Library_CopyFiles;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        AppendString("放弃卸载\r\n");
                        return;
                    }

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("卸载实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }

                    {
                        AppendString("正在停止 dp2library 服务 ...\r\n");

                        nRet = InstallHelper.StopService("dp2LibraryService",
                out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        AppendString("dp2library 服务已经停止\r\n");
                    }

                    AppendString("注销 Windows Service 开始\r\n");

                    Application.DoEvents();

                    // 注销 Windows Service
                    nRet = InstallService(strExePath,
            false,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("注销 Windows Service 结束\r\n");

                // 删除程序目录
                REDO_DELETE_PROGRAMDIR:
                    try
                    {
                        PathUtil.DeleteDirectory(Path.GetDirectoryName(strExePath));
                    }
                    catch (Exception ex)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        "删除程序目录 '" + Path.GetDirectoryName(strExePath) + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
        "卸载 dp2Library",
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_DELETE_PROGRAMDIR;
                    }

                    this.Refresh_dp2library_MenuItems();
                }
                finally
                {
                    AppendSectionTitle("卸载 dp2library 结束");
                }

            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        private static readonly Object _syncRoot_errorLog = new Object(); // 2018/6/26

        // 写入日志文件。每天创建一个单独的日志文件
        public void WriteErrorLog(string strText)
        {
            /*
            FileUtil.WriteErrorLog(
                _syncRoot_errorLog,
                this.UserLogDir,
                strText,
                "log_",
                ".txt");
            */
            ClientInfo.WriteErrorLog(strText);
        }

        // 启用 MSMQ
        private void MenuItem_dp2library_enableMsmq_Click(object sender, EventArgs e)
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

        // 安装 MongoDB
        private void MenuItem_dp2library_setupMongoDB_Click(object sender, EventArgs e)
        {
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

            PathUtil.TryCreateDir(Path.Combine(strDataDir, "db"));
            PathUtil.TryCreateDir(Path.Combine(strDataDir, "log"));

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

        // 首次安装 dp2ZServer
        private void MenuItem_dp2ZServer_install_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在安装 dp2ZServer - Z39.50 服务器 ...";
            try
            {

                AppendSectionTitle("安装 dp2ZServer 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2ZService");
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strError = "dp2ZServer 已经安装过了，不能重复安装";
                    goto ERROR1;
                }
                // strExePath = Unquote(strExePath);

                // program files (x86)/digitalplatform/dp2library
                string strProgramDir = GetProductDirectory("dp2zserver");

                PathUtil.TryCreateDir(strProgramDir);

                string strZipFileName = Path.Combine(this.DataDir, "zserver_app.zip");

                AppendString("安装可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // 还需要把 zserver_data.zip 的内容都展开到程序目录的 temp 子目录中，以兼容以前的 Installer 功能
                string strTempDir = Path.Combine(strProgramDir, "temp");
                PathUtil.CreateDirIfNeed(strTempDir);
                nRet = dp2Library_extractPartDir(strTempDir,
        "cfgs,templates,other",
        out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif
                // 展开 zserver_data.zip 到程序目录
                string strDataZipFileName = Path.Combine(this.DataDir, "zserver_data.zip");
                nRet = ExtractDataFile(strDataZipFileName, strProgramDir, out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 创建 dp2zserver.xml
                {
                    string strXmlFileName = Path.Combine(strProgramDir, "dp2zserver.xml");

                    InstallZServerDlg param_dlg = new InstallZServerDlg();
                    GuiUtil.AutoSetDefaultFont(param_dlg);
                    // 从XML文件中装载已有的信息到对话框
                    // return:
                    //      -1  error
                    //      0   not load
                    //      1   loaded
                    nRet = InstallZServerUtil.LoadInfoFromDp2zserverXmlFile(
                        strXmlFileName,
                        param_dlg,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    param_dlg.ShowDialog(this);
                    if (param_dlg.DialogResult == DialogResult.Cancel)
                    {
                        strError = "安装被放弃 ...";
                        goto ERROR1;
                    }

                    // 写入dp2zserver.xml文件
                    // return:
                    //      -1  error, install faild
                    //      0   succeed
                    nRet = InstallZServerUtil.WriteDp2zserverXmlFile(
            param_dlg,
            strXmlFileName,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 注册为 Windows Service
                strExePath = Path.Combine(strProgramDir, "dp2zserver.exe");

                AppendString("注册 Windows Service ...\r\n");

                nRet = InstallService(strExePath,
        true,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendSectionTitle("安装 dp2ZServer 结束");
                Refresh_dp2ZServer_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int ExtractDataFile(string strZipFileName,
            string strDataDir,
            out string strError)
        {
            strError = "";

            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    for (int i = 0; i < zip.Count; i++)
                    {
                        ZipEntry e1 = zip[i];

                        string strPart = GetSubPath(e1.FileName);
                        string strFullPath = Path.Combine(strDataDir, strPart);

                        e1.FileName = strPart;

                        e1.Extract(strDataDir, ExtractExistingFileAction.OverwriteSilently);
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

        private void MenuItem_dp2ZServer_uninstall_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            DialogResult result = MessageBox.Show(this,
"确实要卸载 dp2ZServer?\r\n\r\n注意: 卸载后程序文件和配置文件将被删除，且不可恢复",
"dp2Installer",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;   // cancelled

            this._floatingMessage.Text = "正在卸载 dp2ZServer - Z39.50 服务器 ...";
            try
            {
                AppendSectionTitle("卸载 dp2ZServer 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2ZService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2ZServer 未曾安装过";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                {
                    AppendString("正在停止 dp2ZServer 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2ZService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2ZServer 服务已经停止\r\n");
                }

                AppendString("注销 Windows Service 开始\r\n");

                Application.DoEvents();

                // 注销 Windows Service
                nRet = InstallService(strExePath,
        false,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("注销 Windows Service 结束\r\n");

            // 删除程序目录
            REDO_DELETE_PROGRAMDIR:
                try
                {
                    PathUtil.DeleteDirectory(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    "删除程序目录 '" + Path.GetDirectoryName(strExePath) + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
    "卸载 dp2ZServer",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_DELETE_PROGRAMDIR;
                }

                AppendSectionTitle("卸载 dp2ZServer 结束");
                Refresh_dp2ZServer_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2ZServer_update_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在更新 dp2ZServer - Z39.50 服务器 ...";

            try
            {
                AppendSectionTitle("更新 dp2ZServer 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2ZService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2ZServer 未曾安装过";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                AppendString("正在停止 dp2ZServer 服务 ...\r\n");
                nRet = InstallHelper.StopService("dp2ZService",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2ZServer 服务已经停止\r\n");

                string strZipFileName = Path.Combine(this.DataDir, "zserver_app.zip");

                AppendString("更新可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    Path.GetDirectoryName(strExePath),
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("正在重新启动 dp2ZServer 服务 ...\r\n");
                nRet = InstallHelper.StartService("dp2ZService",
        out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2ZServer 服务启动成功\r\n");

                AppendSectionTitle("更新 dp2ZServer 结束");
            }
            finally
            {
                this._floatingMessage.Text = "";
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2ZServer_openAppDir_Click(object sender, EventArgs e)
        {
            string strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");
                try
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
            else
            {
                MessageBox.Show(this, "dp2ZServer 未曾安装过");
            }

        }

        private void MenuItem_dp2ZServer_startService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2ZServer 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在启动 dp2ZServer 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StartService("dp2ZService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2ZServer 服务成功启动\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void MenuItem_dp2ZServer_stopService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2ZServer 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在停止 dp2ZServer 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StopService("dp2ZService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2ZServer 服务已经停止\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2ZServer_installService_Click(object sender, EventArgs e)
        {
            InstallOrUninstallService("dp2ZServer", true);
#if NO
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注册 Windows Service 开始");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strError = "dp2ZServer 已经注册为 Windows Service，无法重复进行注册";
                goto ERROR1;
            }

            string strProgramDir = GetProductDirectory("dp2zserver");
            strExePath = Path.Combine(strProgramDir, "dp2zserver.exe");

            if (File.Exists(strExePath) == false)
            {
                strError = "dp2zserver.exe 尚未复制到目标位置，无法进行注册";
                goto ERROR1;
            }

            // 注册 Windows Service
            nRet = InstallService(strExePath,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注册 Windows Service 结束");

            this.Refresh_dp2ZServer_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_dp2ZServer_uninstallService_Click(object sender, EventArgs e)
        {
            InstallOrUninstallService("dp2ZServer", false);

#if NO
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注销 Windows Service 开始");

            Application.DoEvents();

            string strExePath = InstallHelper.GetPathOfService("dp2ZService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2ZServer 尚未安装和注册为 Windows Service，无法进行注销";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            // 注销 Windows Service
            nRet = InstallService(strExePath,
    false,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注销 Windows Service 结束");

            this.Refresh_dp2ZServer_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_dp2ZServer_instanceManagement_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;
            bool bInstalled = true;

            this._floatingMessage.Text = "正在配置 dp2ZServer 实例 ...";

            try
            {
                AppendSectionTitle("配置实例开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2ZService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    if (bControl == false)
                    {
                        strError = "dp2ZServer 未曾安装过";
                        goto ERROR1;
                    }
                    bInstalled = false;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                if (bInstalled == true)
                {
                    AppendString("正在停止 dp2ZServer 服务 ...\r\n");

                    nRet = InstallHelper.StopService("dp2ZService",
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2ZServer 服务已经停止\r\n");
                }

                try
                {
                    // 修改 dp2zserver.xml
                    {
                        string strXmlFileName = Path.Combine(Path.GetDirectoryName(strExePath), "dp2zserver.xml");

                        InstallZServerDlg param_dlg = new InstallZServerDlg();
                        GuiUtil.AutoSetDefaultFont(param_dlg);
                        // 从XML文件中装载已有的信息到对话框
                        // return:
                        //      -1  error
                        //      0   not load
                        //      1   loaded
                        nRet = InstallZServerUtil.LoadInfoFromDp2zserverXmlFile(
                            strXmlFileName,
                            param_dlg,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        param_dlg.ShowDialog(this);
                        if (param_dlg.DialogResult == DialogResult.Cancel)
                        {
                            strError = "配置实例被放弃 ...";
                            goto ERROR1;
                        }

                        // 写入dp2zserver.xml文件
                        // return:
                        //      -1  error, install faild
                        //      0   succeed
                        nRet = InstallZServerUtil.WriteDp2zserverXmlFile(
                param_dlg,
                strXmlFileName,
                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                }
                finally
                {
                    if (bInstalled == true)
                    {
                        string strError1 = "";
                        AppendString("正在重新启动 dp2ZServer 服务 ...\r\n");
                        nRet = InstallHelper.StartService("dp2ZService",
        out strError1);
                        if (nRet == -1)
                        {
                            AppendString("dp2ZServer 服务启动失败: " + strError1 + "\r\n");
                            MessageBox.Show(this, strError1);
                        }
                        else
                        {
                            AppendString("dp2ZServer 服务启动成功\r\n");
                        }
                    }

                    AppendSectionTitle("配置实例结束");
                }
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }


        #region Service Control 功能

#if NO
        IpcClientChannel m_dp2libraryScChannel = new IpcClientChannel();
        IServiceControl m_dp2libraryScObj = null;

        // "ipc://dp2library_ServiceControlChannel/dp2library_ServiceControlServer"
        int StartDp2libraryScChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            //Register the channel with ChannelServices.
            if (this.m_dp2libraryScChannel == null)
                this.m_dp2libraryScChannel = new IpcClientChannel();

            ChannelServices.RegisterChannel(m_dp2libraryScChannel, true);

            try
            {
                IServiceControl m_dp2libraryScObj = (IServiceControl)Activator.GetObject(typeof(IServiceControl),
                    strUrl);
                if (m_dp2libraryScObj == null)
                {
                    strError = "无法连接到 remoting 服务器 " + strUrl;
                    return -1;
                }
            }
            finally
            {
            }

            return 0;
        }

        void EndDp2libraryScChannel()
        {
            if (this.m_dp2libraryScChannel != null)
            {
                ChannelServices.UnregisterChannel(m_dp2libraryScChannel);
                this.m_dp2libraryScChannel = null;
            }
        }
#endif

        #endregion

        private void ToolStripMenuItem_uninstallDp2zserver_Click(object sender, EventArgs e)
        {
            MenuItem_dp2ZServer_uninstall_Click(sender, e);
        }

        private async void ToolStripMenuItem_getMD5ofFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要获取 MD5 的文件名(可以复选)";
            dlg.Filter = "All files (*.*)|*.*";
            dlg.Multiselect = true;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string result = "";
            this._floatingMessage.Text = "正在运算 MD5, 请等待 ...";
            try
            {
                result = await Task<string>.Run(() =>
                {
                    int i = 0;
                    StringBuilder text = new StringBuilder();
                    foreach (var filename in dlg.FileNames)
                    {
                        this._floatingMessage.Text = $"正在运算 {i + 1}) {filename} 的 MD5, 请等待 ...";
                        // using (FileStream stream = File.OpenRead(filename))
                        // 注意要用共享方式打开。因为操作日志文件有可能此时被其他进程打开使用
                        using (FileStream stream = new FileStream(filename,
    FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var bytes = DynamicDownloader.GetFileMd5(stream);
                            text.Append($"{i + 1}) {filename} \tLength: {stream.Length} \tMD5: {Convert.ToBase64String(bytes)}\r\n");
                        }
                        i++;
                    }

                    return text.ToString();
                });
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            MessageDialog.Show(this, result);
        }

        private void MenuItem_dp2Commander_install_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在安装 dp2Commander - 远程控制器 ...";
            try
            {

                AppendSectionTitle("安装 dp2Commander 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = InstallHelper.GetPathOfService("dp2CommanderService");
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strError = "dp2library 已经安装过了，不能重复安装";
                    goto ERROR1;
                }
                // strExePath = Unquote(strExePath);

                // program files (x86)/digitalplatform/dp2library
                string strProgramDir = GetProductDirectory("dp2Commander");

                PathUtil.TryCreateDir(strProgramDir);

                string strZipFileName = Path.Combine(this.DataDir, "commander_app.zip");

                AppendString("安装可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                /*
                // 还需要把 library_data.zip 的内容都展开到程序目录的 temp 子目录中，以兼容以前的 Installer 功能
                string strTempDir = Path.Combine(strProgramDir, "temp");
                PathUtil.TryCreateDir(strTempDir);
                nRet = dp2Library_extractPartDir(strTempDir,
        "cfgs,templates,other",
        out strError);
                if (nRet == -1)
                    goto ERROR1;
                */

                List<string> new_instance_names = null;

                // 创建实例
                AppendString("创建实例 ...\r\n");

                try
                {
                    DigitalPlatform.LibraryServer.InstanceDialog dlg = new DigitalPlatform.LibraryServer.InstanceDialog();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.LockingInstances = this.locking_instances;
                    dlg.TempDir = this.TempDir;
                    // dlg.DataZipFileName = Path.Combine(this.DataDir, "library_data.zip");
                    dlg.CopyFiles += dlg_dp2Library_CopyFiles;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);   // ForegroundWindow.Instance

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("创建实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }

                    new_instance_names = dlg.NewInstanceNames;
                }
                finally
                {
                    AppendString("创建实例结束 ...\r\n");
                }


                // 注册为 Windows Service
                strExePath = Path.Combine(strProgramDir, "dp2Commander.exe");

                AppendString("注册 Windows Service ...\r\n");

                nRet = InstallService(strExePath,
        true,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendSectionTitle("安装 dp2Commander 结束");
                Refresh_dp2library_MenuItems();

                // CreateDefaultDatabases(new_instance_names);
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 首次安装 palmCenter
        private async void MenuItem_palmCenter_install_Click(object sender, EventArgs e)
        {
            await installPalmCenter();
        }

        // 更新 palmCenter
        private async void MenuItem_palmCenter_update_Click(object sender, EventArgs e)
        {
            await installPalmCenter("update");
        }

        private async Task installPalmCenter(string style = "")
        {
            string strError = "";
            int nRet = 0;

            bool update = StringUtil.IsInList("update", style);
            string actionName = "安装";
            if (update)
                actionName = "更新";

            string strProgramDir = "";

            this._floatingMessage.Text = $"正在{actionName} palmCenter - 掌纹中心 ...";

            try
            {
                AppendSectionTitle($"{actionName} palmCenter 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                strProgramDir = GetProductDirectory("palmcenter");
                string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");
                if (update == false && File.Exists(strExePath) == true)
                {
                    strError = "palmCenter 已经安装过了，不能重复安装";
                    goto ERROR1;
                }

                PathUtil.TryCreateDir(strProgramDir);

                if (update)
                {
                    // 停止 service
                    AppendString("正在停止 palmcenter 服务 ...\r\n");
                    Application.DoEvents();

                    nRet = InstallHelper.StopService("palmCenterService",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    AppendString("palmcenter 服务已经停止\r\n");
                }

                // (ClickOnce 安装时)确保文件已经下载到本地
                var ret = await PrepareDataFile("palm_app.zip");
                if (ret.Value == -1)
                {
                    strError = $"准备 palm_app.zip 文件失败: {ret.ErrorInfo}";
                    goto ERROR1;
                }

                string strZipFileName = Path.Combine(this.DataDir, "palm_app.zip");

                AppendString($"{actionName}可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (update == false)
                {
                    // 配置参数
                    AppendString("配置参数 ...\r\n");

                    try
                    {
                        using (var dlg = new PalmCenter.Install.SettingDialog())
                        {
                            GuiUtil.AutoSetDefaultFont(dlg);
                            var dlg_result = dlg.ShowDialog(this);
                            if (dlg_result == DialogResult.Cancel)
                            {
                                // TODO: 注意清理干净可执行文件，以便后面可以重新安装
                                strError = "放弃配置";
                                goto ERROR1;
                            }
                        }
                    }
                    finally
                    {
                        AppendString("配置参数结束 ...\r\n");
                    }

                    // 注册为 Windows Service
                    // strExePath = Path.Combine(strProgramDir, "palmcenter.exe");

                    AppendString("注册 Windows Service ...\r\n");

                    nRet = installPalmService("install start",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    // 启动 service
                    AppendString("正在启动 palmcenter 服务 ...\r\n");
                    Application.DoEvents();

                    nRet = InstallHelper.StartService("palmCenterService",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("palmcenter 服务成功启动\r\n");
                }
                // 此后不再删除程序目录
                strProgramDir = "";

                AppendSectionTitle($"{actionName} palmCenter 结束");
                Refresh_palmCenter_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            if (string.IsNullOrEmpty(strProgramDir) == false)
                DeleteProgramDir(strProgramDir);

            MessageBox.Show(this, strError);
        }

        // 打开数据文件夹
        private void MenuItem_palmCenter_openDataDir_Click(object sender, EventArgs e)
        {
            string dir = Utility.GetServiceUserDirectory("palmCenter");
            if (Directory.Exists(dir) == false)
            {
                MessageBox.Show(this, $"文件夹 {dir} 不存在");
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 打开程序文件夹
        private void MenuItem_palmCenter_openProgramFolder_Click(object sender, EventArgs e)
        {
            string strProgramDir = GetProductDirectory("palmcenter");
            if (Directory.Exists(strProgramDir) == false)
            {
                MessageBox.Show(this, $"文件夹 {strProgramDir} 不存在");
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(strProgramDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 配置 掌纹中心
        private void MenuItem_palmCenter_config_Click(object sender, EventArgs e)
        {
            string strError = "";

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strProgramDir = GetProductDirectory("palmcenter");
            string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "palmcenter 未曾安装过";
                goto ERROR1;
            }

            /*
            // 停止服务
            AppendString("正在停止 palmcenter 服务 ...\r\n");
            Application.DoEvents();

            int nRet = InstallHelper.StopService("palmCenterService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("palmcenter 服务已经停止\r\n");
            */

            AppendString("正在配置 palmcenter 参数 ...\r\n");

            using (var dlg = new PalmCenter.Install.SettingDialog())
            {
                dlg.ShowDialog(this);
            }

            AppendString("配置 palmcenter 参数完成\r\n");

            /*
            // 启动服务
            AppendString("正在启动 palmcenter 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StartService("palmCenterService",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendString("palmcenter 服务成功启动\r\n");
            */
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_palmCenter_startService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strProgramDir = GetProductDirectory("palmcenter");
            string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "palmcenter 未曾安装过";
                goto ERROR1;
            }
            // strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在启动 palmcenter 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StartService("palmCenterService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("palmcenter 服务成功启动\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_palmCenter_stopService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strProgramDir = GetProductDirectory("palmcenter");
            string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "palmcenter 未曾安装过";
                goto ERROR1;
            }
            // strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在停止 palmcenter 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StopService("palmCenterService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("palmcenter 服务已经停止\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_palmCenter_installService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = installPalmService("install", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_palmCenter_uninstallService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = installPalmService("uninstall", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int installPalmService(string action,
            out string strError)
        {
            strError = "";

            string strProgramDir = GetProductDirectory("palmcenter");
            string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "palmcenter 未曾安装过";
                return 0;
            }

            try
            {
                string arguments = action;
                var process = System.Diagnostics.Process.Start(strExePath, arguments);
                process.WaitForExit();
                var code = process.ExitCode;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            AppendString($"palmcenter 服务{(action.StartsWith("install") ? "注册" : "注销")}成功\r\n");
            return 1;
        }

        // 卸载 palmCenter
        private void MenuItem_palmCenter_uninstall_Click(object sender, EventArgs e)
        {
            string strError = "";

            DialogResult result = MessageBox.Show(this,
"确实要卸载 palmCenter?\r\n\r\n注意: 卸载后程序文件将被删除",
"dp2Installer",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;   // cancelled

            string strProgramDir = GetProductDirectory("palmcenter");
            string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");

            // 注销
            int nRet = installPalmService("uninstall", out strError);
            if (nRet == -1)
                goto ERROR1;

            // 等待结束

            // 删除程序目录
            /*
            REDO_DELETE_PROGRAMDIR:
            try
            {
                PathUtil.DeleteDirectory(strProgramDir);
            }
            catch (Exception ex)
            {
                DialogResult temp_result = MessageBox.Show(this,
"删除程序目录 '" + strProgramDir + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
"卸载 palmCenter",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Retry)
                    goto REDO_DELETE_PROGRAMDIR;
            }
            */
            DeleteProgramDir(strProgramDir);

            AppendSectionTitle("卸载 palmCenter 结束");
            this.Refresh_palmCenter_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        bool DeleteProgramDir(string strProgramDir)
        {
        // 删除程序目录
        REDO_DELETE_PROGRAMDIR:
            try
            {
                PathUtil.DeleteDirectory(strProgramDir);
                return true;
            }
            catch (Exception ex)
            {
                DialogResult temp_result = MessageBox.Show(this,
"删除程序目录 '" + strProgramDir + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
"卸载 palmCenter",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Retry)
                    goto REDO_DELETE_PROGRAMDIR;
                return false;
            }
        }

        void Refresh_palmCenter_MenuItems()
        {
            string strProgramDir = GetProductDirectory("palmcenter");
            string strExePath = Path.Combine(strProgramDir, "palmcenter.exe");

            if (File.Exists(strExePath) == false)
            {
                this.MenuItem_palmCenter_install.Enabled = true;
                this.MenuItem_palmCenter_update.Enabled = false;
                this.MenuItem_palmCenter_uninstall.Enabled = false;
            }
            else
            {
                this.MenuItem_palmCenter_install.Enabled = false;
                this.MenuItem_palmCenter_update.Enabled = true;
                this.MenuItem_palmCenter_uninstall.Enabled = true;
            }

            // this.MenuItem_palmCenter_openDataDir.DropDownItems.Clear();
            // AddMenuItem(MenuItem_palmCenter_openDataDir, "palmCenter");
        }

        class PrepareResult : NormalResult
        {
            public string LocalFileName { get; set; }
        }

        async Task<PrepareResult> PrepareDataFile(string filename,
            string style = "")
        {
            string localFileName = Path.Combine(this.DataDir, filename);
            if (_networkDeployment == false
                && File.Exists(localFileName)
                && StringUtil.IsInList("network", style) == false)
                return new PrepareResult { LocalFileName = localFileName };

            string url = "http://dp2003.com/dp2installer/v3/" + filename;
            if (File.Exists(localFileName))
            {
                var fi = new FileInfo(localFileName);
                // 探测差异
                var get_result = await DownloadUtility.GetServerFileInfo(
                    url,
                    fi.LastWriteTimeUtc,
                    fi.Length);
                if (get_result.Value == -1)
                    return new PrepareResult
                    {
                        Value = -1,
                        ErrorInfo = get_result.ErrorInfo,
                        ErrorCode = get_result.ErrorCode,
                        LocalFileName = localFileName
                    };
                if (get_result.Value == 0)
                    return new PrepareResult
                    {
                        LocalFileName = localFileName
                    };
            }

            // TODO: 最好出现一个下载进度对话框，并且允许中断
            AppendString($"正在下载文件 {url} ...\r\n");

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(_cancel.Token))
            using (FileDownloadDialog dlg = new FileDownloadDialog())
            {
                dlg.FormClosed += (s, e) =>
                {
                    cancel.Cancel();
                };
                this.Invoke((Action)(() =>
                {
                    dlg.Show(this);
                }));

                // 下载文件
                var download_result = await DownloadUtility.DownloadFileAsync(
                    url,
                    localFileName,
                    cancel.Token,
                    (sender, e1) =>
                    {
                        dlg.SetProgress($"正在下载文件 {url}",
                            e1.BytesReceived,
                            e1.TotalBytesToReceive);
                    },
                    () =>
                    {
                        /*
                        this.Invoke((Action)(() =>
                        {
                            dlg.Close();
                        }));
                        */
                    });
                if (download_result.Value == -1)
                {
                    AppendString($"下载文件 {url} 出错: {download_result.ErrorInfo}\r\n");

                    return new PrepareResult
                    {
                        Value = -1,
                        ErrorInfo = download_result.ErrorInfo,
                        ErrorCode = download_result.ErrorCode,
                        LocalFileName = localFileName
                    };
                }
            }

            AppendString($"下载文件 {url} 完成\r\n");
            return new PrepareResult { LocalFileName = localFileName };
        }

        private async void MenuItem_palmCenter_installDriver_Click(object sender, EventArgs e)
        {
            string strError = "";
            var download_result = await PrepareDataFile("palm_driver_setup.exe", "network");
            if (download_result.Value == -1)
            {
                strError = download_result.ErrorInfo;
                goto ERROR1;
            }
            Process.Start(download_result.LocalFileName);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 首次安装 dp2Gate
        private async void MenuItem_dp2Gate_install_Click(object sender, EventArgs e)
        {
            await installdp2Gate();
        }

        // 更新 dp2Gate
        private async void MenuItem_dp2Gate_update_Click(object sender, EventArgs e)
        {
            await installdp2Gate("update");
        }

        // 打开数据文件夹
        private void MenuItem_dp2Gate_openDataDir_Click(object sender, EventArgs e)
        {
            string dir = Utility.GetServiceUserDirectory("dp2gate");
            if (Directory.Exists(dir) == false)
            {
                MessageBox.Show(this, $"文件夹 {dir} 不存在");
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 打开程序文件夹
        private void MenuItem_dp2Gate_openProgramFolder_Click(object sender, EventArgs e)
        {
            string strProgramDir = GetProductDirectory("dp2gate");
            if (Directory.Exists(strProgramDir) == false)
            {
                MessageBox.Show(this, $"文件夹 {strProgramDir} 不存在");
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(strProgramDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 配置 dp2Gate
        private void MenuItem_dp2Gate_config_Click(object sender, EventArgs e)
        {
            string strError = "";

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strProgramDir = GetProductDirectory("dp2gate");
            string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "dp2gate 未曾安装过";
                goto ERROR1;
            }

            AppendString("正在配置 dp2gate 参数 ...\r\n");

            using (var dlg = new PalmCenter.Install.SettingDialog())
            {
                dlg.ShowDialog(this);
            }

            AppendString("配置 dp2gate 参数完成\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2Gate_startService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strProgramDir = GetProductDirectory("dp2gate");
            string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "dp2gate 未曾安装过";
                goto ERROR1;
            }
            // strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在启动 dp2gate 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StartService("dp2GateService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2Gate 服务成功启动\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2Gate_stopService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strProgramDir = GetProductDirectory("dp2gate");
            string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "dp2gate 未曾安装过";
                goto ERROR1;
            }

            AppendString("正在停止 dp2gate 服务 ...\r\n");
            Application.DoEvents();

            nRet = InstallHelper.StopService("dp2GateService",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2Gate 服务已经停止\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2Gate_installService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = installGateService("install", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2Gate_uninstallService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = installGateService("uninstall", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 卸载 dp2Gate
        private void MenuItem_dp2Gate_uninstall_Click(object sender, EventArgs e)
        {
            string strError = "";

            DialogResult result = MessageBox.Show(this,
"确实要卸载 dp2Gate?\r\n\r\n注意: 卸载后程序文件将被删除",
"dp2Installer",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;   // cancelled

            string strProgramDir = GetProductDirectory("dp2gate");
            string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");

            // 注销
            int nRet = installGateService("uninstall", out strError);
            if (nRet == -1)
                goto ERROR1;

            // 等待结束

            // 删除程序目录
            DeleteProgramDir(strProgramDir);

            AppendSectionTitle("卸载 dp2Gate 结束");
            this.Refresh_palmCenter_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private async Task installdp2Gate(string style = "")
        {
            string strError = "";
            int nRet = 0;

            bool update = StringUtil.IsInList("update", style);
            string actionName = "安装";
            if (update)
                actionName = "更新";

            string strProgramDir = "";

            this._floatingMessage.Text = $"正在{actionName} dp2Gate - 门禁中心 ...";

            try
            {
                AppendSectionTitle($"{actionName} dp2Gate 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                strProgramDir = GetProductDirectory("dp2gate");
                string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");
                if (update == false && File.Exists(strExePath) == true)
                {
                    strError = "dp2Gate 已经安装过了，不能重复安装";
                    goto ERROR1;
                }

                PathUtil.TryCreateDir(strProgramDir);

                if (update)
                {
                    // 停止 service
                    AppendString("正在停止 dp2Gate 服务 ...\r\n");
                    Application.DoEvents();

                    nRet = InstallHelper.StopService("dp2GateService",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    AppendString("dp2Gate 服务已经停止\r\n");
                }

                // (ClickOnce 安装时)确保文件已经下载到本地
                var ret = await PrepareDataFile("gate_app.zip");
                if (ret.Value == -1)
                {
                    strError = $"准备 gate_app.zip 文件失败: {ret.ErrorInfo}";
                    goto ERROR1;
                }

                string strZipFileName = Path.Combine(this.DataDir, "gate_app.zip");

                AppendString($"{actionName}可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (update == false)
                {
                    // 配置参数
                    AppendString("配置参数 ...\r\n");

                    try
                    {
                        using (var dlg = new dp2Gate.Install.SettingDialog())
                        {
                            GuiUtil.AutoSetDefaultFont(dlg);
                            var dlg_result = dlg.ShowDialog(this);
                            if (dlg_result == DialogResult.Cancel)
                            {
                                // TODO: 注意清理干净可执行文件，以便后面可以重新安装
                                strError = "放弃配置";
                                goto ERROR1;
                            }
                        }
                    }
                    finally
                    {
                        AppendString("配置参数结束 ...\r\n");
                    }

                    // 注册为 Windows Service
                    strExePath = Path.Combine(strProgramDir, "dp2gate.exe");

                    AppendString("注册 Windows Service ...\r\n");

                    nRet = installGateService("install start",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    // 启动 service
                    AppendString("正在启动 dp2gate 服务 ...\r\n");
                    Application.DoEvents();

                    nRet = InstallHelper.StartService("dp2GateService",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2Gate 服务成功启动\r\n");
                }
                // 此后不再删除程序目录
                strProgramDir = "";

                AppendSectionTitle($"{actionName} dp2Gate 结束");
                Refresh_dp2Gate_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            if (string.IsNullOrEmpty(strProgramDir) == false)
                DeleteProgramDir(strProgramDir);

            MessageBox.Show(this, strError);
        }

        void Refresh_dp2Gate_MenuItems()
        {
            string strProgramDir = GetProductDirectory("dp2gate");
            string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");

            if (File.Exists(strExePath) == false)
            {
                this.MenuItem_dp2Gate_install.Enabled = true;
                this.MenuItem_dp2Gate_update.Enabled = false;
                this.MenuItem_dp2Gate_uninstall.Enabled = false;
            }
            else
            {
                this.MenuItem_dp2Gate_install.Enabled = false;
                this.MenuItem_dp2Gate_update.Enabled = true;
                this.MenuItem_dp2Gate_uninstall.Enabled = true;
            }
        }


        int installGateService(string action,
    out string strError)
        {
            strError = "";

            string strProgramDir = GetProductDirectory("dp2gate");
            string strExePath = Path.Combine(strProgramDir, "dp2gate.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "dp2gate 未曾安装过";
                return 0;
            }

            try
            {
                string arguments = action;
                var process = System.Diagnostics.Process.Start(strExePath, arguments);
                process.WaitForExit();
                var code = process.ExitCode;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            AppendString($"dp2gate 服务{(action.StartsWith("install") ? "注册" : "注销")}成功\r\n");
            return 1;
        }

    }
}
