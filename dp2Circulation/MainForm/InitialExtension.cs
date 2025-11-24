// #define TEST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.ServiceProcess;
using System.ComponentModel;
using System.Web;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

// using log4net;
using Serilog;
using Serilog.Core;

using Ionic.Zip;
using AsyncPluggableProtocol;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Drawing;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Core;
using DigitalPlatform.RFID;
using Serilog.Events;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关初始化各种信息的功能
    /// </summary>
    public partial class MainForm
    {
        public CharsetTable EaccCharsetTable = null;
        public Marc8Encoding Marc8Encoding = null;

        public void ReportError(string strTitle,
    string strError)
        {
            // 发送给 dp2003.com
            string strText = strError;
            if (string.IsNullOrEmpty(strText) == true)
                return;

            strText += "\r\n\r\n===\r\n" + PackageEventLog.GetEnvironmentDescription().Replace("\t", "    ");

            try
            {
                // 发送报告
                int nRet = LibraryChannel.CrashReport(
                    this.GetCurrentUserName() + "@" + this.ServerUID,
                    strTitle,
                    strText,
                    out strError);
                if (nRet == -1)
                {
                    strError = "CrashReport() (" + strTitle + ") 出错: " + strError;
                    MainForm.WriteErrorLog(strError);
                }
            }
            catch (Exception ex)
            {
                strError = "CrashReport() (" + strTitle + ") 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                MainForm.TryWriteErrorLog(strError);
            }
        }

        #region ClickOnce 自动更新

        enum UpdateState
        {
            None = 0,
            CheckForUpdate = 1,
            Update = 2,
            Finish = 3,
        }

        UpdateState _updateState = UpdateState.None;

        void CancelUpdateClickOnceApplication()
        {
            if (ApplicationDeployment.IsNetworkDeployed
                && StringUtil.IsNewInstance() == false)
            {
                ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;

                // 2015/10/8
                deployment.CheckForUpdateCompleted -= new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                deployment.CheckForUpdateProgressChanged -= new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);

                deployment.UpdateCompleted -= new AsyncCompletedEventHandler(ad_UpdateCompleted);
                deployment.UpdateProgressChanged -= new DeploymentProgressChangedEventHandler(ad_UpdateProgressChanged);

                if (_updateState == UpdateState.CheckForUpdate)
                    deployment.CheckForUpdateAsyncCancel();
                else if (_updateState == UpdateState.Update)
                    deployment.UpdateAsyncCancel();
            }
        }


        private void BeginUpdateClickOnceApplication()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                OpenBackgroundForm();

                this.DisplayBackgroundTextLn("开始自动更新(ClickOnce安装)");
                // 2025/5/23
                this.DisplayBackgroundTextLn(ClientInfo.GetClickOnceInstallLocation());

                ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
                deployment.CheckForUpdateCompleted -= new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                deployment.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                deployment.CheckForUpdateProgressChanged -= new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);
                deployment.CheckForUpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);

                _updateState = UpdateState.CheckForUpdate;
                try
                {
                    deployment.CheckForUpdateAsync();
                }
                catch (Exception ex)
                {
                    this.DisplayBackgroundTextLn("更新操作 (CheckForUpdate) 出现异常: " + ExceptionUtil.GetAutoText(ex));
                }
            }
        }

        void ad_CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            // downloadStatus.Text = String.Format("Downloading: {0}. {1:D}K of {2:D}K downloaded.", GetProgressString(e.State), e.BytesCompleted / 1024, e.BytesTotal / 1024);
            this.ShowBackgroundProgress("progress_1",
            String.Format("Downloading: {0}. {1:D}K of {2:D}K downloaded.", GetProgressString(e.State), e.BytesCompleted / 1024, e.BytesTotal / 1024)
            );
        }

        string GetProgressString(DeploymentProgressState state)
        {
            if (state == DeploymentProgressState.DownloadingApplicationFiles)
            {
                return "application files";
            }
            else if (state == DeploymentProgressState.DownloadingApplicationInformation)
            {
                return "application manifest";
            }
            else
            {
                return "deployment manifest";
            }
        }

        void ad_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            // 2024/6/24
            // 延时关闭背景窗
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30), CancelToken);
                CloseBackgroundForm();
            });

            if (e.Error != null)
            {
                this.DisplayBackgroundTextLn("ERROR: Could not retrieve new version of the application. Reason: \r\n" + e.Error.Message + "\r\nPlease report this error to the system administrator.");
                return;
            }
            else if (e.Cancelled == true)
            {
                this.DisplayBackgroundTextLn("The update was cancelled.");
                return;
            }

            // Ask the user if they would like to update the application now.
            if (e.UpdateAvailable)
            {
#if NO
                sizeOfUpdate = e.UpdateSizeBytes;

                if (!e.IsUpdateRequired)
                {
                    DialogResult dr = MessageBox.Show("An update is available. Would you like to update the application now?\n\nEstimated Download Time: ", "Update Available", MessageBoxButtons.OKCancel);
                    if (DialogResult.OK == dr)
                    {
                        BeginUpdate();
                    }
                }
                else
                {
                    MessageBox.Show("A mandatory update is available for your application. We will install the update now, after which we will save all of your in-progress data and restart your application.");
                    BeginUpdate();
                }
#endif
                BeginUpdate();
            }
            else
            {
                this.DisplayBackgroundTextLn("当前没有发现新的版本");
            }
        }

        private void BeginUpdate()
        {
            ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
            deployment.UpdateCompleted -= new AsyncCompletedEventHandler(ad_UpdateCompleted);
            deployment.UpdateCompleted += new AsyncCompletedEventHandler(ad_UpdateCompleted);
            deployment.UpdateProgressChanged -= new DeploymentProgressChangedEventHandler(ad_UpdateProgressChanged);
            deployment.UpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_UpdateProgressChanged);

            _updateState = UpdateState.Update;

            OpenBackgroundForm();
            try
            {
                deployment.UpdateAsync();
            }
            catch (Exception ex)
            {
                this.DisplayBackgroundTextLn("更新操作 (Update) 出现异常: " + ExceptionUtil.GetAutoText(ex));
            }
        }

        void ad_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            String progressText = String.Format("{0:D}K out of {1:D}K downloaded - {2:D}% complete", e.BytesCompleted / 1024, e.BytesTotal / 1024, e.ProgressPercentage);
            // downloadStatus.Text = progressText;
            this.ShowBackgroundProgress("progress",
                progressText);
        }

        void ad_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // 2024/6/24
            // 延时关闭背景窗
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30), CancelToken);
                CloseBackgroundForm();
            });

            _updateState = UpdateState.Finish;

            if (e.Cancelled)
            {
                this.DisplayBackgroundTextLn("The update of the application's latest version was cancelled.");
                return;
            }
            else if (e.Error != null)
            {
                this.DisplayBackgroundTextLn("ERROR: Could not install the latest version of the application. Reason: \r\n" + e.Error.Message + "\r\nPlease report this error to the system administrator.");
                return;
            }

#if NO
            DialogResult dr = MessageBox.Show(this,
                "dp2Circulation 已经成功更新。是否重启 dp2Circulation? (If you do not restart now, the new version will not take effect until after you quit and launch the application again.)",
                "Restart Application", 
                MessageBoxButtons.OKCancel);
            if (DialogResult.OK == dr)
            {
                Application.Restart();
            }
#endif
            this.DisplayBackgroundTextLn("dp2circulation 已经成功更新。重启可立即使用新版本。");
        }

        #endregion

        #region 绿色安装包 自动更新

        // MyWebClient _webClient = null;
        System.Net.WebRequest _webRequest = null;

        /*
发生未捕获的界面线程异常: 
Type: System.Exception
Message: GetLastWin32Error [0], ex.NativeErrorCode = 14001, ex.ErrorCode=-2147467259
Stack:
在 dp2Circulation.MainForm.StartGreenUtility()
在 dp2Circulation.MainForm.MainForm_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.WmClose(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 dp2Circulation.MainForm.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

Type: System.ComponentModel.Win32Exception
Message: 应用程序无法启动，因为应用程序的并行配置不正确。有关详细信息，请参阅应用程序事件日志，或使用命令行 sxstrace.exe 工具。
Stack:
在 System.Diagnostics.Process.StartWithShellExecuteEx(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start()
在 System.Diagnostics.Process.Start(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start(String fileName, String arguments)
在 dp2Circulation.MainForm.StartGreenUtility()
         * */

        /*
发生未捕获的界面线程异常: 
Type: System.ComponentModel.Win32Exception
Message: 拒绝访问。
Stack:
在 System.Diagnostics.Process.StartWithShellExecuteEx(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start()
在 System.Diagnostics.Process.Start(ProcessStartInfo startInfo)
在 System.Diagnostics.Process.Start(String fileName, String arguments)
在 dp2Circulation.MainForm.StartGreenUtility()
在 dp2Circulation.MainForm.MainForm_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.WmClose(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 dp2Circulation.MainForm.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

         * */
        // 启动绿色安装小工具。因为 dp2circulation 正在运行时无法覆盖替换文件，所以需要另外启动一个小程序来完成这个任务
        void StartGreenUtility()
        {
#if NO
            string strError = "";

            // 将 greenutility.zip 展开到 c:\dp2circulation_temp
            string strZipFileName = "c:\\dp2circulation\\greenutility.zip";
            string strTargetDir = "c:\\~dp2circulation_greenutility";
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "展开文件 '"+strZipFileName+"' 到目录 '"+strTargetDir+"' 时出现异常" + ex.Message;
                ReportError("dp2circulation 展开 greenutility.zip 时出错", strError);
                return;
            }
#endif
            if (this._updatedGreenZipFileNames.Count == 0)
                throw new ArgumentException("调用 StartGreenUtility() 前应该准备好 _updatedGreenZipFileNames 内容");

            string strBinDir = GetBinDir();
            string strUtilDir = GetUtilDir();

            string strExePath = Path.Combine(strUtilDir, "greenutility.exe");
            string strParameters = "-action:install -source:"
                + strBinDir  // source 是指存储了 .zip 文件的目录
                + " -target:" + strBinDir // target 是指最终要安装的目录 
                + " -wait:dp2circulation.exe"
                + " -files:" + StringUtil.MakePathList(this._updatedGreenZipFileNames);
            try
            {
                System.Diagnostics.Process.Start(strExePath, strParameters);
            }
            catch (Win32Exception ex)
            {
                // 改为抛出包含 Win32 错误码的异常
                // https://msdn.microsoft.com/en-us/library/ms681382(v=vs.85).aspx
                // ERROR_ACCESS_DENIED
                // 5 (0x5)
                // Access is denied.
                int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new Exception("GetLastWin32Error [" + error.ToString() + "], ex.NativeErrorCode = " + ex.NativeErrorCode + ", ex.ErrorCode=" + ex.ErrorCode, ex);
            }

            this._updatedGreenZipFileNames.Clear(); // 避免后面再次调用本函数
        }

        bool BeginUpdateGreenApplication()
        {
            if (ApplicationDeployment.IsNetworkDeployed == false
                && StringUtil.IsDevelopMode() == false
                && StringUtil.IsNewInstance() == false
                )
            {
                Task.Factory.StartNew(() => GreenUpdate());
                return true;
            }
            return false;
        }

        void CancelUpdateGreenApplication()
        {
            if (ApplicationDeployment.IsNetworkDeployed == false)
            {
#if NO
                WebClient temp_webclient = this._webClient;
                if (temp_webclient != null)
                    temp_webclient.CancelAsync();
#endif
                this.CancelWebClients();

                WebRequest temp_webrequest = this._webRequest;
                if (temp_webrequest != null)
                    temp_webrequest.Abort();
            }
        }

        string GetBinDir()
        {
            if (StringUtil.IsDevelopMode() == false)
                return Environment.CurrentDirectory;
            else
                return "c:\\dp2circulation";    // 开发用的版本，用这个恒定的目录进行测试，避免弄乱开发目录
        }

        string GetUtilDir()
        {
            string strBinDir = GetBinDir();
            return Path.Combine(Path.GetDirectoryName(strBinDir), "~" + Path.GetFileName(strBinDir) + "_greenutility");
        }

        // return:
        //      true    探测到
        //      false   没有探测到
        bool DetectDp2003Site()
        {
            IPAddress[] address_list1 = null;
            try
            {
                address_list1 = Dns.GetHostAddresses("dp2003.com");
                if (address_list1.Length > 0)
                    return true;
            }
            catch
            {
            }
            return false;
        }

        static string _defaultDownloadBaseUrl = "http://dp2003.com/dp2circulation/v3/";

        // 获得下载的基地址 URL。
        // 在参数配置里面配置这个地址的时候，如果希望强制发生作用，可以在第一字符使用 '~'。否则，程序会优先看 dp2003.com 域名是否可以解析，如果能解析则还是优先使用 dp2003.com 的地址
        string GetDownloadBaseUrl()
        {
            // 绿色安装包
            string strBaseUrl = this.AppInfo.GetString("config",
                "green_package_server_url",
                "").Trim();
            if (string.IsNullOrEmpty(strBaseUrl) == true)
                return _defaultDownloadBaseUrl;

            // 在 dp2003.com 域名有效的情况下，依然使用 dp2003.com 的发行目录
            if (DetectDp2003Site() == true && strBaseUrl[0] != '~')
                return _defaultDownloadBaseUrl;

            if (strBaseUrl[0] == '~')
                strBaseUrl.Substring(1);

            if (strBaseUrl[strBaseUrl.Length - 1] != '/')
                strBaseUrl += "/";

            return strBaseUrl;
        }

        void GreenUpdate()
        {
            int nRet = 0;
            string strError = "";

            string strBinDir = GetBinDir();
            string strUtilDir = GetUtilDir();

            this.DisplayBackgroundTextLn("开始自动更新(绿色安装)\r\n这期间，您可继续进行其它操作");

            // 希望下载的文件。纯文件名
            List<string> filenames = new List<string>() {
                "greenutility.zip", // 这是工具软件，不算在 dp2circulation 范围内
                "app.zip",
                "data.zip"};

            // 发现更新了并下载的文件。纯文件名
            List<string> updated_filenames = new List<string>();

            // 需要确保最后被展开的文件。如果下载了而未展开，则下次下载的时候会发现文件已经是最新了，从而不会下载，也不会展开。这就有漏洞了
            // 那么就要在下载和展开这个全过程中断的时候，记住删除已经下载的文件。这样可以迫使下次一定要下载和展开
            List<string> temp_filepaths = new List<string>();

            try
            {
                foreach (string filename in filenames)
                {
                    string strUrl = // "http://dp2003.com/dp2circulation/v2/"
                        GetDownloadBaseUrl()
                        + filename;
                    string strLocalFileName = Path.Combine(strBinDir, filename).ToLower();

                    if (File.Exists(strLocalFileName) == true)
                    {
                        // this.DisplayBackgroundText("检查文件版本 " + strUrl + " ...\r\n");

                        // 判断 http 服务器上一个文件是否已经更新
                        // return:
                        //      -1  出错
                        //      0   没有更新
                        //      1   已经更新
                        nRet = IsServerFileUpdated(strUrl,
                            File.GetLastWriteTimeUtc(strLocalFileName),
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                            updated_filenames.Add(filename);
#if NO
                        else
                            this.DisplayBackgroundText("没有更新。\r\n");
#endif
                    }
                    else
                        updated_filenames.Add(filename);

                    if (updated_filenames.IndexOf(filename) != -1)
                    {
                        this.DisplayBackgroundTextLn("下载 " + strUrl + " 到 " + strLocalFileName + " ...");

                        nRet = DownloadFile(strUrl,
                            strLocalFileName,
                            out strError);
                        if (nRet == -1)
                        {
                            goto ERROR1;
                        }

                        // 下载成功的本地文件，随时可能被删除，如果整个流程没有完成的话
                        temp_filepaths.Add(strLocalFileName);
                    }
                }

                string strGreenUtilityExe = Path.Combine(strUtilDir, "greenutility.exe");

                if (updated_filenames.IndexOf("greenutility.zip") != -1
                    || File.Exists(strGreenUtilityExe) == false)
                {
                    // 将 greenutility.zip 展开到 c:\dp2circulation_temp
                    string strZipFileName = Path.Combine(strBinDir, "greenutility.zip").ToLower();
                    string strTargetDir = strUtilDir;

                    this.DisplayBackgroundTextLn("展开文件 " + strZipFileName + " 到 " + strTargetDir + " ...");
                    try
                    {
                        using (ZipFile zip = ZipFile.Read(strZipFileName))
                        {
                            foreach (ZipEntry e in zip)
                            {
                                e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "展开文件 '" + strZipFileName + "' 到目录 '" + strTargetDir + "' 时出现异常" + ex.Message;
                        // 删除文件，以便下次能重新下载和展开
                        try
                        {
                            File.Delete(Path.Combine(strTargetDir, "greenutility.zip"));
                        }
                        catch
                        {

                        }
                        ReportError("dp2circulation 展开 greenutility.zip 时出错", strError);
                        return;
                    }

                    updated_filenames.Remove("greenutility.zip");
                    temp_filepaths.Remove(strZipFileName);
                }

#if TEST
            // 测试
            this._updatedGreenZipFileNames = new List<string>();
            this._updatedGreenZipFileNames.Add("app.zip");
#else
                // 给 MainForm 一个标记，当它退出的时候，会自动展开 .zip 文件完成升级安装
                this._updatedGreenZipFileNames = updated_filenames;
#endif
                if (this._updatedGreenZipFileNames.Count > 0)
                    this.DisplayBackgroundTextLn("dp2circulation 绿色安装包升级文件已经准备就绪。当退出 dp2circulation 时会自动进行安装。");
                else
                    this.DisplayBackgroundTextLn("没有发现更新。");

                temp_filepaths.Clear(); // 这样 finally 块就不会删除这些文件了
            }
            finally
            {
                foreach (string filepath in temp_filepaths)
                {
                    File.Delete(filepath);
                }
            }

            return;
        ERROR1:
            // ShowMessageBox(strError);
            this.DisplayBackgroundTextLn("绿色更新过程出错: " + strError);
            ReportError("dp2circulation GreenUpdate() 出错", strError);
        }

        // 从磁盘进行绿色安装包的升级。一般用于没有网络的环境
        // 本函数会要求操作者选择一个目录，这个目录里面应该存在一个 app.zip 文件，一个 data.zip 文件，可能还有 greenutility.zip 文件。第一次使用这个功能，必须要有 greenutility.zip 文件
        void UpgradeGreenFromDisk()
        {
            string strError = "";

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定升级文件所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            // dir_dlg.SelectedPath = this.textBox_outputFolder.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            string strBinDir = GetBinDir();
            string strUtilDir = GetUtilDir();

            // 需要确保最后被展开的文件。如果下载了而未展开，则下次下载的时候会发现文件已经是最新了，从而不会下载，也不会展开。这就有漏洞了
            // 那么就要在下载和展开这个全过程中断的时候，记住删除已经下载的文件。这样可以迫使下次一定要下载和展开
            List<string> temp_filepaths = new List<string>();

            try
            {

                string[] filenames = new string[] { "app.zip", "data.zip", "greenutility.zip" };
                // 发现更新了并下载的文件。纯文件名
                List<string> updated_filenames = new List<string>();

                // 检查所指定的目录中是否有特征性的文件
                List<string> found_filenames = new List<string>();
                foreach (string filename in filenames)
                {
                    string strSourcePath = Path.Combine(dir_dlg.SelectedPath, filename);
                    string strLocalFileName = Path.Combine(strBinDir, filename).ToLower();
                    if (File.Exists(strSourcePath) == false)
                        continue;

                    found_filenames.Add(filename);

                    if (File.Exists(strLocalFileName) == true &&
                        File.GetLastWriteTimeUtc(strSourcePath) <= File.GetLastWriteTimeUtc(strLocalFileName))
                        continue;

                    try
                    {
                        File.Copy(strSourcePath, strLocalFileName, true);
                    }
                    catch (Exception ex)
                    {
                        strError = "文件 '" + strSourcePath + "' 复制到 '" + strLocalFileName + "' 时出错: " + ex.Message;
                        goto ERROR1;
                    }
                    updated_filenames.Add(filename);
                    temp_filepaths.Add(strLocalFileName);
                }

                if (found_filenames.Count == 0)
                {
                    strError = "您所选择的目录 '" + dir_dlg.SelectedPath + "' 中没有包含任何名为 app.zip data.zip greenutility.zip 的文件";
                    goto ERROR1;
                }

                if (updated_filenames.Count == 0)
                {
                    strError = "您所选择的目录 '" + dir_dlg.SelectedPath + "' 中所包含的 " + StringUtil.MakePathList(found_filenames) + " 文件，相对于上次已经安装的文件，没有变化更新。升级操作因此被放弃";
                    goto ERROR1;
                }

                string strGreenUtilityExe = Path.Combine(strUtilDir, "greenutility.exe");

                if (updated_filenames.IndexOf("greenutility.zip") != -1
                    || File.Exists(strGreenUtilityExe) == false)
                {
                    // 将 greenutility.zip 展开到 c:\dp2circulation_temp
                    string strZipFileName = Path.Combine(strBinDir, "greenutility.zip").ToLower();
                    string strTargetDir = strUtilDir;

                    this.DisplayBackgroundTextLn("展开文件 " + strZipFileName + " 到 " + strTargetDir + " ...");
                    try
                    {
                        using (ZipFile zip = ZipFile.Read(strZipFileName))
                        {
                            foreach (ZipEntry e in zip)
                            {
                                e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "展开文件 '" + strZipFileName + "' 到目录 '" + strTargetDir + "' 时出现异常" + ex.Message;
                        // 删除文件，以便下次能重新下载和展开
                        try
                        {
                            File.Delete(Path.Combine(strTargetDir, "greenutility.zip"));
                        }
                        catch
                        {

                        }
                        ReportError("dp2circulation 展开 greenutility.zip 时出错", strError);
                        return;
                    }

                    updated_filenames.Remove("greenutility.zip");
                    temp_filepaths.Remove(strZipFileName);
                }

                string strExePath = Path.Combine(strUtilDir, "greenutility.exe");
                if (File.Exists(strExePath) == false)
                {
                    strError = "您指定的源目录 '" + dir_dlg.SelectedPath + "' 中必须包含 greenutility.zip 文件";
                    goto ERROR1;
                }

                // 给 MainForm 一个标记，当它退出的时候，会自动展开 .zip 文件完成升级安装
                this._updatedGreenZipFileNames = updated_filenames;

                if (this._updatedGreenZipFileNames.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
"为完成 dp2Circulation 升级安装，现在需要退出 dp2Circulation。请确认当前所有修改已经保存，可以重新启动了。\r\n\r\n是否立即重新启动?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        MessageBox.Show(this, "升级安装被放弃");
                        return;
                    }

                    temp_filepaths.Clear(); // 这样 finally 块就不会删除这些文件了

                    StartGreenUtility();
                    Application.Exit();
                }
                else
                {
                    strError = "没有发现更新";
                    goto ERROR1;
                }
            }
            finally
            {
                foreach (string filepath in temp_filepaths)
                {
                    File.Delete(filepath);
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 已经完成更新的纯 .zip 文件名
        List<string> _updatedGreenZipFileNames = null;

        // 判断 http 服务器上一个文件是否已经更新
        // return:
        //      -1  出错
        //      0   没有更新
        //      1   已经更新
        int IsServerFileUpdated(string strUrl,
            DateTime local_lastmodify,
            out string strError)
        {
            strError = "";
            _webRequest = System.Net.WebRequest.Create(strUrl);
            _webRequest.Method = "HEAD";
            _webRequest.Timeout = 5000;
            try
            {
                using (var response = _webRequest.GetResponse() as HttpWebResponse)
                {
                    string strLastModified = response.GetResponseHeader("Last-Modified");
                    if (string.IsNullOrEmpty(strLastModified) == true)
                    {
                        strError = "header 中无法获得 Last-Modified 值";
                        return -1;
                    }
                    DateTime time;
                    if (DateTimeUtil.TryParseRfc1123DateTimeString(strLastModified, out time) == false)
                    {
                        strError = "从响应中取出的 Last-Modified 字段值 '" + strLastModified + "' 格式不合法";
                        return -1;
                    }
                    if (time > local_lastmodify)
                        return 1;
                    return 0;
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        strError = ex.Message;
                        return -1;
                    }
                }
                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                _webRequest = null;
            }
            //return 0;
        }

        // 记忆 WebClient 对象，用于中断操作
        List<MyWebClient> _webClients = new List<MyWebClient>();

        void AddWebClient(MyWebClient webClient)
        {
            lock (_webClients)
            {
                _webClients.Add(webClient);
            }
        }

        void RemoveWebClient(MyWebClient webClient)
        {
            lock (_webClients)
            {
                _webClients.Remove(webClient);
            }
        }

        void CancelWebClients()
        {
            lock (_webClients)
            {
                foreach (MyWebClient webClient in _webClients)
                {
                    webClient.CancelAsync();    // 没有测试过 MyWebClient.Cancel()
                }
            }
        }

        // 从 http 服务器下载一个文件
        // 阻塞式
        int DownloadFile(string strUrl,
    string strLocalFileName,
    out string strError)
        {
            strError = "";

            using (MyWebClient webClient = new MyWebClient())
            {
                this.AddWebClient(webClient);
                try
                {
                    webClient.ReadWriteTimeout = 30 * 1000; // 30 秒，在读写之前 - 2015/12/3
                    webClient.Timeout = 30 * 60 * 1000; // 30 分钟，整个下载过程 - 2015/12/3
                    webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                    string strTempFileName = strLocalFileName + ".temp";
                    // TODO: 先下载到临时文件，然后复制到目标文件
                    try
                    {
                        webClient.DownloadFile(new Uri(strUrl, UriKind.Absolute), strTempFileName);

                        File.Delete(strLocalFileName);
                        File.Move(strTempFileName, strLocalFileName);
                        return 0;
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            var response = ex.Response as HttpWebResponse;
                            if (response != null)
                            {
                                if (response.StatusCode == HttpStatusCode.NotFound)
                                {
                                    strError = ex.Message;
                                    return -1;
                                }
                            }
                        }

                        strError = ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                }
                finally
                {
                    this.RemoveWebClient(webClient);
                }
            }
        }

        #endregion

        bool DetectIE()
        {
#if NO
            try
            {
                System.Diagnostics.Process.Start("iexplore", "https://support.microsoft.com/zh-cn/kb/2468871");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // 可能是 ie 没有安装?
                return false;
            }

            return true;
#endif
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = "iexplore";
            startinfo.Arguments = "https://support.microsoft.com/zh-cn/kb/2468871";
            {
                startinfo.UseShellExecute = false;
                // startinfo.CreateNoWindow = true;
            }

            Process process = new Process();
            process.StartInfo = startinfo;
            process.EnableRaisingEvents = true;

            try
            {
                process.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        static long GetUserDiskFreeSpace(out string strDriveName)
        {
            string strUserFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            strDriveName = Path.GetPathRoot(strUserFolder);
            return GetTotalFreeSpace(strDriveName);
        }

        static long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }

        void CopyJavascriptDirectories()
        {
            string strError = "";
            {
                string strSourceDirectory = Path.Combine(this.DataDir, "order");
                if (Directory.Exists(strSourceDirectory) == true)
                {
                    string strTargetDirectory = Path.Combine(this.UserDir, "order");
                    int nRet = PathUtil.CopyDirectory(strSourceDirectory, strTargetDirectory, false, out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "copy '" + strSourceDirectory + "' to '" + strTargetDirectory + "' error");
                }
                else
                    MessageBox.Show(this, "复制 Javascript 目录时发生错误。目录 '" + strSourceDirectory + "' 不存在");
            }

#if NO
            {
                string strSourceDirectory = Path.Combine(this.DataDir, "jquery");
                if (Directory.Exists(strSourceDirectory) == true)
                {
                    string strTargetDirectory = Path.Combine(this.UserDir, "jquery");
                    int nRet = PathUtil.CopyDirectory(strSourceDirectory, strTargetDirectory, false, out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "copy '" + strSourceDirectory + "' to '" + strTargetDirectory + "' error");
                }
                else
                    MessageBox.Show(this, "复制 Javascript 目录时发生错误。目录 '" + strSourceDirectory + "' 不存在");
            }
#endif

            {
                string strZipFileName = Path.Combine(this.DataDir, "jquery.zip");
                string strTargetDirectory = Path.Combine(this.UserDir, "jquery");
                bool bError = false;
                try
                {
                    using (ZipFile zip = ZipFile.Read(strZipFileName))
                    {
                        foreach (ZipEntry e in zip)
                        {
                            e.Extract(strTargetDirectory, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                    bError = true;
                }

                // 2017/5/18
                // 报错以后把目标目录删除，以便下次程序启动时候可以顺利运行
                if (bError)
                {
                    try
                    {
                        PathUtil.DeleteDirectory(strTargetDirectory);
                    }
                    catch (Exception ex)
                    {
                        // TODO: 是否顺便检测一下当前安装了 360?
                        MessageBox.Show(this, "自动删除文件夹 '" + strTargetDirectory + "' 时出错: " + ex.Message
                            + "\r\n\r\n请在退出内务以后，想办法手动删除文件夹 '" + strTargetDirectory + "'");
                    }
                }
            }

        }

        // 2018/3/26
        // 迁移以前版本的打印模板目录
        void MigratePrintTemplatesDirectory()
        {
            string strSourceDirectory = Path.Combine(this.DataDir, "print_templates");
            if (Directory.Exists(strSourceDirectory) == false)
                return;

            string strTargetDirectory = Path.Combine(this.UserDir, "print_templates");
            if (Directory.Exists(strTargetDirectory) == true)
                return; // 如果目标目录已经存在，就不进行迁移了

            int nRet = PathUtil.CopyDirectory(strSourceDirectory,
                strTargetDirectory,
                false,
                out string strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                PathUtil.DeleteDirectory(strSourceDirectory);
            }
            catch (Exception ex)
            {
                strError = "MigrateProjectDirectory() 删除源目录 '" + strSourceDirectory + "' 时发生异常: " + ex.Message;
                goto ERROR1;
            }

            return;
        ERROR1:
            this.ReportError("dp2circulation 迁移打印模板目录时出错", strError);
            MessageBox.Show(this, "迁移打印模板目录时出错: " + strError);
        }

        // 迁移旧版本的统计方案目录和各种配套文件
        void MigrateProjectDirectory()
        {
            string strError = "";

            string strSourceDirectory = Path.Combine(this.DataDir, "clientcfgs");
            if (Directory.Exists(strSourceDirectory) == false)
                return;

            string strTargetDirectory = Path.Combine(this.UserDir, "clientcfgs");
            if (Directory.Exists(strTargetDirectory) == true)
                return; // 如果目标目录已经存在，就不进行迁移了

            int nRet = PathUtil.CopyDirectory(strSourceDirectory,
                strTargetDirectory,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 移动几个 xml 文件
            DirectoryInfo di = new DirectoryInfo(this.DataDir);
            FileInfo[] fis = di.GetFiles("*projects.xml");

            foreach (FileInfo fi in fis)
            {
                string strSourcePath = Path.Combine(this.DataDir, fi.Name);
                if (File.Exists(strSourcePath) == false)
                    continue;
                string strTargetPath = Path.Combine(this.UserDir, fi.Name);
                File.Move(strSourcePath, strTargetPath);
            }

            try
            {
                PathUtil.DeleteDirectory(strSourceDirectory);
            }
            catch (Exception ex)
            {
                strError = "MigrateProjectDirectory() 删除源目录 '" + strSourceDirectory + "' 时发生异常: " + ex.Message;
                goto ERROR1;
            }

            return;
        ERROR1:
            this.ReportError("dp2circulation 迁移统计方案目录时出错", strError);
            MessageBox.Show(this, "迁移统计方案目录时出错: " + strError);
        }

        // 2016/4/26
        // 检查用户目录的权限是否足够
        bool CheckUserDirectory()
        {
            try
            {
                if (Directory.Exists(this.UserDir) == false)
                    goto ERROR1;

                // 创建和删除子目录试验
                string strTestDir = Path.Combine(this.UserDir, "_testdir_");
                PathUtil.TryCreateDir(strTestDir);

                if (Directory.Exists(strTestDir) == false)
                    goto ERROR1;

                Directory.Delete(strTestDir);

                // 创建文件试验
                string strTestFile1 = Path.Combine(this.UserDir, "_testfile1_");
                using (StreamWriter sw = new StreamWriter(strTestFile1, false))
                {
                    sw.WriteLine("first line");
                }

                string strTestFile2 = Path.Combine(this.UserDir, "_testfile2_");
                using (StreamWriter sw = new StreamWriter(strTestFile2, false))
                {
                    sw.WriteLine("first line");
                }

                // 复制文件试验
                File.Copy(strTestFile1, strTestFile2, true);

                File.Delete(strTestFile1);
                File.Delete(strTestFile2);

                // TODO: 检查下级有没有隐藏文件属性?
            }
            catch
            {
                goto ERROR1;
            }

            return true;
        ERROR1:
            Program.PromptAndExit(this, "用户目录 '" + this.UserDir + "' 创建失败或者权限不足。请确保当前 Windows 用户能访问和修改这个目录以及下级子目录、文件，并确保它或者上级目录不是隐藏的状态");
            return false;
        }

        public event StreamProgressChangedEventHandler StreamProgressChanged = null;

        // 程序启动时候需要执行的初始化操作
        // 这些操作只需要执行一次。也就是说，和登录和连接的服务器无关。如果有关，则要放在 InitialProperties() 中
        // FormLoad() 中的许多操作应当移动到这里来，以便尽早显示出框架窗口
        void FirstInitial()
        {
            string strError = "";
            int nRet = 0;

            this.SetBevel(false);

            if (StringUtil.IsDevelopMode() == false)
            {
                this.MenuItem_separator_function2.Visible = false;
                // this.MenuItem_chatForm.Visible = false;
                this.MenuItem_messageForm.Visible = false;
                this.MenuItem_openReservationListForm.Visible = false;
                // this.MenuItem_inventory.Visible = false;
                this.MenuItem_openMarc856SearchForm.Visible = false;
                this.MenuItem_createGreenApplication.Visible = false;
                this.MenuItem_refreshLibraryUID.Visible = false;
            }

#if NO
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
#endif



            this.MenuItem_upgradeFromDisk.Visible = !ApplicationDeployment.IsNetworkDeployed;

            // OpenBackgroundForm();

            if (GetUserDiskFreeSpace(out string strDriveName) < 1024 * 1024 * 10)
            {
                Program.PromptAndExit(this, "用户目录所在硬盘 " + strDriveName + " 剩余空间太小(已小于10M)。请先腾出足够空间，再重新启动 dp2circulation");
                return;
            }

            {
                // 2013/6/16
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2Circulation_v2");
                PathUtil.TryCreateDir(this.UserDir);

                // 2016/6/4
                // 用户目录可以重定向
                nRet = RedirectUserDir(out strError);
                if (nRet == -1)
                {
                    Program.PromptAndExit(this, "重定向用户目录时发生错误: " + strError);
                    return;
                }

                if (CheckUserDirectory() == false)
                    return;

                // 将 dp2circulation.xml 文件中绿色安装目录或者 ClickOnce 安装的数据目录移动到用户目录
                nRet = MoveDp2circulationXml(out strError);
                if (nRet == -1)
                {
                    this.ReportError("dp2circulation 移动 dp2circulation.xml 时出现错误", "(安静报错)" + strError);
                    MessageBox.Show(this, strError);
                }

                // this.AppInfo = new ApplicationInfo(Path.Combine(this.UserDir, "dp2circulation.xml"));
                // this.AppInfo = new NewApplicationInfo(ClientInfo.Config);   // 2022/1/24

                this.UserTempDir = Path.Combine(this.UserDir, "temp");
                PathUtil.TryCreateDir(this.UserTempDir);

                // 2015/7/8
                this.UserLogDir = Path.Combine(this.UserDir, "log");
                PathUtil.TryCreateDir(this.UserLogDir);

#if REMOVED
                var repository = log4net.LogManager.CreateRepository("main");
                log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(this.UserLogDir, "log_");
                log4net.Config.XmlConfigurator.Configure(repository);

                LibraryChannelManager.Log = LogManager.GetLogger("main", "channellib");
                _log = LogManager.GetLogger("main", "dp2circulation");


                // 启动时在日志中记载当前 dp2circulation 版本号
                // 此举也能尽早发现日志目录无法写入的问题，会抛出异常
                MainForm.WriteInfoLog(Assembly.GetAssembly(this.GetType()).FullName);
#endif

                // 检查 KB????
                /*
    操作类型 crashReport -- 异常报告 
    主题 dp2circulation 
    发送者 xxx
    媒体类型 text 
    内容 发生未捕获的异常: 
    Type: System.AggregateException
    Message: 未通过等待任务或访问任务的 Exception 属性观察到任务的异常。因此，终结器线程重新引发了未观察到的异常。
    Stack:
    在 System.Threading.Tasks.TaskExceptionHolder.Finalize()

    Type: System.IO.FileLoadException
    Message: 未能加载文件或程序集“System.Core, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e, Retargetable=Yes”或它的某一个依赖项。给定程序集名称或基本代码无效。 (异常来自 HRESULT:0x80131047)
    Stack:
    在 DigitalPlatform.MessageClient.MessageConnection.Login(String userName, String password, String libraryUID, String libraryName, String propertyList)
    在 dp2Circulation.MessageHub.Login()
    在 DigitalPlatform.MessageClient.MessageConnection.<>c__DisplayClass5.<ConnectAsync>b__3(Task antecendent)
    在 System.Threading.Tasks.Task.<>c__DisplayClassb.<ContinueWith>b__a(Object obj)
    在 System.Threading.Tasks.Task.InnerInvoke()
    在 System.Threading.Tasks.Task.Execute()


    dp2Circulation 版本: dp2Circulation, Version=2.4.5712.38964, Culture=neutral, PublicKeyToken=null
    操作系统：Microsoft Windows NT 6.0.6001 Service Pack 1 
    操作时间 2015/8/24 16:21:11 (Mon, 24 Aug 2015 16:21:11 +0800) 
    前端地址 xxx 经由 http://dp2003.com/dp2library 

                 * */
                // https://support.microsoft.com/zh-cn/kb/2468871
                string strKbName = "KB2468871";
                if (Global.IsKbInstalled(strKbName) == false)
                {
                    Application.DoEvents();

                    MainForm.WriteErrorLog("dp2circulation 启动时，发现本机尚未安装 .NET Framework 4 更新 KB2468871");

#if NO
                    DialogResult result = MessageBox.Show(this,
        "为顺利运行 dp2Circulation， 请先安装 .NET Framework 4 更新 (" + strKbName + ")"
        + "\r\n\r\n是否继续运行?",
        "dp2Circulation",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("iexplore", "https://support.microsoft.com/zh-cn/kb/2468871");
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            // 可能是 ie 没有安装?
                        }
                        Application.Exit();
                        return;
                    }
#endif
                    MessageBox.Show(this, "为顺利运行 dp2Circulation， 请先安装 .NET Framework 4 更新 (" + strKbName + ")");
                    if (Control.ModifierKeys != Keys.Control)
                    {
#if NO
                        try
                        {
                            System.Diagnostics.Process.Start("iexplore", "https://support.microsoft.com/zh-cn/kb/2468871");
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            // 可能是 ie 没有安装?
                        }
#endif
                        WindowsUpdateDialog dlg = new WindowsUpdateDialog();
                        MainForm.SetControlFont(dlg, this.DefaultFont);
                        dlg.ShowDialog(this);
                        // Application.Exit();
                        Program.PromptAndExit(this, "Windows 更新后退出");
                        return;
                    }
                }

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

                // 2015/10/3
                strDir = PathUtil.MergePath(this.DataDir, "cfgcache");
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

#if NO
            {
                string strCssUrl = PathUtil.MergePath(this.DataDir, "background.css");
                string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

                try
                {
                    Global.WriteHtml(m_backgroundForm.WebBrowser,
                        "<html><head>" + strLink + "</head><body>");
                }
                catch(Exception ex)
                {
                    //MessageBox.Show(this, "dp2circulation 所需的 IE 浏览器控件出现异常: " + ExceptionUtil.GetDebugText(ex));
                    //Application.Exit();
                    Program.PromptAndExit(this, "dp2circulation 所需的 IE 浏览器控件出现异常: " + ExceptionUtil.GetDebugText(ex));
                }
            }
#endif
            {
                this._imageManager = new ImageManager();
                this._imageManager.TempFileDir = this.UserTempDir;
                this._imageManager.ChannelPool = Program.MainForm._channelPool;
                // this._imageManager.GetObjectComplete += new GetObjectCompleteEventHandler(_imageManager_GetObjectComplete);
                this._imageManager.BeginThread();
            }

            // 用于在 WebControl 中展示 dp2 对象资源的协议
            ProtocolFactory.Register("dpres",
                () => new ResourceProtocol(this, (path, current, length) =>
                {
                    var func = this.StreamProgressChanged;
                    if (func != null)
                    {
                        DigitalPlatform.CirculationClient.StreamProgressChangedEventArgs e = new DigitalPlatform.CirculationClient.StreamProgressChangedEventArgs();
                        e.Path = path;
                        e.Current = current;
                        e.Length = length;
                        func(this, e);
                    }
                })
                );

            // 用于在 WebControl 中显示条码图像的协议
            ProtocolFactory.Register("barcode", () => new BarcodeProtocol());

            SetPrintLabelMode();

            {
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
            }

            // 2021/11/1
            if (this.AppInfo.GetBoolean("palmprint", "palmprintDialogVisible", false))
                ShowPalmprintDialog();

            stopManager.Initial(
                this,
                this.toolButton_stop,
                (object)this.toolStripStatusLabel_main,
                (object)this.toolStripProgressBar_main);
            // stopManager.OnDisplayMessage += new DisplayMessageEventHandler(stopManager_OnDisplayMessage);

#if REMOVED
            // 公用的 Stop 对象
            this.Stop = new DigitalPlatform.Stop();
            this.Stop.Register(stopManager, true);
#endif

            this.SetMenuItemState();

            OpenBackgroundForm();   // 2024/6/19
            try
            {
                // TODO: 是否每次重新启动 dp2circulation 都自动清除一次缓存？
                // cfgcache
                Debug.Assert(string.IsNullOrEmpty(this.UserDir) == false, "");
                // 2015/10/3 改在 UserDir 下
                nRet = cfgCache.Load(Path.Combine(this.UserDir, "cfgcache.xml"),    // this.DataDir
                    out strError);
                if (nRet == -1)
                {
                    if (IsFirstRun == false)
                        MessageBox.Show(strError + "\r\n\r\n程序稍后会尝试自动创建这个文件");
                }

                cfgCache.TempDir = Path.Combine(this.UserDir, "cfgcache");  // this.DataDir
                cfgCache.InstantSave = true;

                // 2013/4/12
                // 清除以前残余的文件
                cfgCache.Upgrade();

                if (this.AppInfo != null)
                {
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
                }

                // 第一次复制绿色版本
                _ = Task.Factory.StartNew(
                    () =>
                    {
                        CopyGreen();
                    },
        this._cancel.Token,
        TaskCreationOptions.LongRunning,
        TaskScheduler.Default);
                // _ = Task.Factory.StartNew(() => CopyGreen());

                StartPrepareNames(true, true);

                if (this.MdiClient != null)
                    this.MdiClient.ClientSizeChanged += new EventHandler(MdiClient_ClientSizeChanged);

                // GuiUtil.RegisterIE9DocMode();

                // 迁移统计方案文件
                MigrateProjectDirectory();

                CopyJavascriptDirectories();

                MigratePrintTemplatesDirectory();

                #region 脚本支持
                ScriptManager.applicationInfo = this.AppInfo;
                // ScriptManager.CfgFilePath = Path.Combine(this.DataDir, "mainform_statis_projects.xml");
                // ScriptManager.DataDir = this.DataDir;
                ScriptManager.CfgFilePath = Path.Combine(this.UserDir, "mainform_statis_projects.xml");
                ScriptManager.DataDir = this.UserDir;

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
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
                #endregion

                if (this.qrRecognitionControl1 != null)
                {
                    this.qrRecognitionControl1.Catched += new DigitalPlatform.Drawing.CatchedEventHandler(qrRecognitionControl1_Catched);
                    this.qrRecognitionControl1.CurrentCamera = AppInfo.GetString(
                        "mainform",
                        "current_camera",
                        "");
                    this.qrRecognitionControl1.EndCatch();  // 一开始的时候并不打开摄像头 2013/5/25
                }

#if GCAT_SERVER
            this.m_strPinyinGcatID = this.AppInfo.GetString("entity_form", "gcat_pinyin_api_id", "");
            this.m_bSavePinyinGcatID = this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", false);
#endif

#if NO
            // 2015/5/24
            MouseLButtonMessageFilter filter = new MouseLButtonMessageFilter();
            filter.MainForm = this;
            Application.AddMessageFilter(filter);
#endif

#if NO
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
#endif
                if (CopyDefaultCfgFiles(out strError) == false)
                {
                    // Application.Exit();
                    Program.PromptAndExit(this, strError);
                    return;
                }

#if PROPERTY_TASK_LIST
                this.PropertyTaskList.BeginThread();
#endif

                StartOrStopRfidManager();

#if NEWFINGER
                // 2022/6/7
                // 迁移以前的 fingerprintReaderUrl 到 palmprintReaderUrl
                if (string.IsNullOrEmpty(this.PalmprintReaderUrl))
                {
                    var old_url = this.AppInfo.GetString("fingerprint",
                        "fingerPrintReaderUrl",
                        "");
                    if (string.IsNullOrEmpty(old_url) == false)
                    {
                        this.AppInfo.SetString("palmprint",
                        "palmPrintReaderUrl",
                        old_url);
                        this.AppInfo.SetString("fingerprint",
                        "fingerPrintReaderUrl",
                        "");
                    }
                }
#endif

                // 2020/1/3
                _ = StartOrStopPalmManagerAsync();

                // 2019/9/13
                StartProcessManager();

                // 2021/3/13
                // 启动后台日志统计线程
                StartStatisLogWorker(_cancel.Token);

            }
            finally
            {
                CloseBackgroundForm();
            }
        }

        #region RFID

        CancellationTokenSource _cancelRfidManager = new CancellationTokenSource();

        public void StartOrStopRfidManager()
        {
            // 2021/3/19
            // 保护。防止重复 +=
            RfidManager.ListTags -= RfidManager_ListTags;

            if (string.IsNullOrEmpty(this.RfidCenterUrl) == false)
            {
                _cancelRfidManager?.Cancel();

                _cancelRfidManager = new CancellationTokenSource();
                RfidManager.Base.Name = "RFID 中心";
                RfidManager.Url = this.RfidCenterUrl;
                // RfidManager.Base.ShortWaitTime = TimeSpan.FromSeconds(1);
                RfidManager.SyncSetEAS = true;  // 协调 SetEAS() 时序
                RfidManager.InventoryIdleSeconds = this.RfidInventoryIdleSeconds;
                RfidManager.GetRSSI = this.UhfRSSI == 0 ? false : true;
                RfidTagList.OnlyReadEPC = this.UhfOnlyEpcCharging;
                SetRfidTagCachePolicy();    // RfidTagList.UseTagTable = false;
                // RfidManager.AntennaList = "1|2|3|4";    // testing
                // RfidManager.SetError += RfidManager_SetError;
                RfidManager.ListTags += RfidManager_ListTags;

                _ = Task.Run(() =>
                {
                    RfidManager.ClearChannels(); // 清理以前残留的通道 2022/6/10
                    var error = CheckRfidCenterVersion();
                    if (error != null)
                        this.BeginInvoke((Action)(() =>
                        {
                            MessageBox.Show(this, error);
                        }));
                });

                RfidManager.Start(_cancelRfidManager.Token);
            }
            else
            {
                _cancelRfidManager?.Cancel();
                RfidManager.Url = "";
                RfidManager.ListTags -= RfidManager_ListTags;
            }
        }

        void SetRfidTagCachePolicy()
        {
            if (this.RfidTagCachePolicy == "要缓存"
                || this.RfidTagCachePolicy == "部分缓存")
                RfidTagList.UseTagTable = true;
            else
                RfidTagList.UseTagTable = false;
        }


        /*
        private string _error = null;   // "test error line asdljasdkf; ;jasldfjasdjkf aasdfasdf";

        public string Error
        {
            get => _error;
            set
            {
                if (_error != value)
                {
                    _error = value;
                    // OnPropertyChanged("Error");
                }
            }
        }

            */


        public event TagChangedEventHandler TagChanged = null;
        // public event SetErrorEventHandler TagSetError = null;

        private void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 标签总数显示
            if (e.Result.Results != null)
            {
                List<OneTag> results = null;
                if (RfidManager.GetRSSI)
                    results = RfidTagList.FilterByRSSI(e.Result.Results, (byte)this.UhfRSSI);
                else
                    results = e.Result.Results;
                RfidTagList.Refresh(sender as BaseChannel<IRfid>,
                    e.ReaderNameList,
                    results,    // e.Result.Results,
                        (add_books, update_books, remove_books, add_patrons, update_patrons, remove_patrons) =>
                        {
                            RfidManager.Touch();
                            TagChanged?.Invoke(sender, new TagChangedEventArgs
                            {
                                AddBooks = add_books,
                                UpdateBooks = update_books,
                                RemoveBooks = remove_books,
                                AddPatrons = add_patrons,
                                UpdatePatrons = update_patrons,
                                RemovePatrons = remove_patrons
                            });
                            /* 
                             * 有两种级别处理 RFID 标签缓存的副作用：
                             * 1) 将 RfidManager.UseTagTable 设置为 false (注意默认为 true)，这样根本不会用到 _tagTable；
                             * 2) 保持 RfidManager.UseTagTable 为 true，但在每次 Refresh 的同时调用 RfidTagList>ClearTagTable() 函数清除已经拿走的标签的 _tagTable 缓存事项。
                             */

                            if (this.RfidTagCachePolicy == "部分缓存")
                            {
                                // 清除已经拿走的标签对应的 RfidTagList._tagTable 缓存信息
                                RfidTagList.ClearTagTableByDatas(remove_books);
                                RfidTagList.ClearTagTableByDatas(remove_patrons);
                            }
                        },
                        (type, text) =>
                        {
                            if (string.IsNullOrEmpty(text) == false)
                                RfidManager.TriggerSetError(this, new SetErrorEventArgs { Error = text });
                            else
                                RfidManager.TriggerSetError(this, new SetErrorEventArgs { Error = text });

                            // TagSetError?.Invoke(this, new SetErrorEventArgs { Error = text });
                        });

                // 2023/11/13
                // 单独触发一次 TagChanged
                if (RfidManager.GetRSSI && results.Count > 0)
                {
                    TagChanged?.Invoke(sender, new TagChangedEventArgs
                    {
                        UpdateRssiTags = results
                    });
                }

                // 标签总数显示 图书+读者卡
                // this.Number = $"{TagList.Books.Count}:{TagList.Patrons.Count}";
            }
        }

        #endregion


        #region 掌纹 FingerprintManager

        CancellationTokenSource _cancelPalmManager = new CancellationTokenSource();

        public async Task StartOrStopPalmManagerAsync()
        {
            // 2021/3/19
            // 保护。防止重复 +=
            FingerprintManager.Touched -= PalmprintManager_Touched;
            // SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;

            if (string.IsNullOrEmpty(this.PalmprintReaderUrl) == false)
            {
                string session_id = $"内务 {DateTime.Now.ToShortTimeString()}";
                // Program.MainForm.OperHistory.AppendHtml($"<div class='debug green'>{ HttpUtility.HtmlEncode($"掌纹监控任务启动") }</div>");
                WriteInfoLog($"{GetPalmName()}监控任务启动。session_id={session_id}");

                _cancelPalmManager?.Cancel();

                // 2022/6/11
                // 等待上一次的任务结束
                var task = FingerprintManager.Base.Task;
                if (task != null)
                    await task;

                _cancelPalmManager = new CancellationTokenSource();
                FingerprintManager.Url = this.PalmprintReaderUrl;
                FingerprintManager.Base.ShortWaitTime = TimeSpan.FromMilliseconds(1);   // 2021/11/1
                FingerprintManager.Base.Name = $"{GetPalmName()}中心";
                FingerprintManager.SessionID = session_id;
                FingerprintManager.Touched += PalmprintManager_Touched;
                // FingerprintManager.GetMessage($"clear,session:{session_id}");
                ClearPalmMessage();
                _ = Task.Run(() =>
                {
                    // await Task.Delay(TimeSpan.FromSeconds(1));
                    FingerprintManager.ClearChannels(); // 清理以前残留的通道 2022/6/10
                    var error = CheckPalmCenterVersion();
                    if (error != null)
                        this.BeginInvoke((Action)(() =>
                        {
                            MessageBox.Show(this, error);
                        }));
                });
                FingerprintManager.Start(_cancelPalmManager.Token);

                // SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            }
            else
            {
                // Program.MainForm.OperHistory.AppendHtml($"<div class='debug error'>{ HttpUtility.HtmlEncode($"掌纹监控任务停止") }</div>");
                WriteInfoLog($"{Program.MainForm.GetPalmName()}监控任务停止。session_id={FingerprintManager.SessionID}");

                _cancelPalmManager?.Cancel();
                FingerprintManager.Url = "";
                FingerprintManager.Touched -= PalmprintManager_Touched;
            }
        }

        public bool IsFingerprint()
        {
            return ProcessManager.IsFingerprintUrl(this.PalmprintReaderUrl);
        }

        public string GetPalmName()
        {
            if (ProcessManager.IsFingerprintUrl(this.PalmprintReaderUrl))
                return "指纹";
            return "掌纹";
        }

#if REMOVED
        // palmceneter 自己可以感知 power resume 了，这个功能就废止了
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    // 内务代为重启一次 palmcenter
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            if (string.IsNullOrEmpty(FingerprintManager.Url) == false)
                            {
                                Program.MainForm.Speak("掌纹中心被唤醒");
                                RestartPalmCenter();
                            }
                        }
                        catch (Exception ex)
                        {
                            ClientInfo.WriteErrorLog($"InitialExtension.SystemEvents_PowerModeChanged() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        }
                    });
                    break;
                case PowerModes.Suspend:
                    break;
            }
        }
#endif

        // 清除此前累积在 palmcenter 中未取的所有消息
        public void ClearPalmMessage()
        {
            if (string.IsNullOrEmpty(FingerprintManager.Url) == false)
            {
                FingerprintManager.GetMessage($"clear,session:{FingerprintManager.SessionID}");
                Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug green'>{HttpUtility.HtmlEncode($"清除此前未取的全部{Program.MainForm.GetPalmName()}消息")}</div>");
            }
        }

        // 重启掌纹中心
        public void RestartPalmCenter()
        {
            if (string.IsNullOrEmpty(FingerprintManager.Url) == false)
            {
                var result = FingerprintManager.GetState("restart");
                if (result.Value == -1)
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode($"重启{GetPalmName()}中心出错: {result.ErrorInfo}")}</div>");
                else
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug green'>{HttpUtility.HtmlEncode($"重启{GetPalmName()}中心成功。重启完成可能需要一定时间，请耐心等待")}</div>");
            }
        }

        const string rfidcenter_base_version = "1.14.17";

        public string CheckRfidCenterVersion()
        {
            if (string.IsNullOrEmpty(RfidManager.Url) == false)
            {
                List<string> errors = new List<string>();
                var result = RfidManager.GetState("getVersion");
                if (result.Value == -1)
                {
                    // _errors.Add("所连接的 RFID 中心版本太低。请升级到最新版本");
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode($"获得 RFID 中心版本号时出错: {result.ErrorInfo}")}</div>");
                    return null;
                }
                else
                {
                    try
                    {
                        if (StringUtil.CompareVersion(result.ErrorCode, rfidcenter_base_version) < 0)
                            errors.Add($"所连接的 RFID 中心版本太低(为 {result.ErrorCode} 版)。请升级到 {rfidcenter_base_version} 以上版本");
                    }
                    catch (Exception ex)
                    {
                        MainForm.WriteErrorLog($"CheckServerUID() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        errors.Add($"所连接的 RFID 中心版本太低。请升级到最新版本。({ex.Message})");
                    }
                }

                if (errors.Count > 0)
                {
                    string error = StringUtil.MakePathList(errors, "; ");
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode(error)}</div>");
                    return error;
                }
            }

            return null;
        }


        const string pamcenter_base_version = "1.1.3";   // "1.0.14";
        const string fingerprint_base_version = "2.3.1";

        public string CheckPalmCenterVersion()
        {
            if (string.IsNullOrEmpty(FingerprintManager.Url) == false)
            {
                var result = FingerprintManager.GetVersion();
                if (result.Value == -1)
                {
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode($"获得{GetPalmName()}中心版本号时出错: {result.ErrorInfo}")}</div>");
                    return null;
                }
                else if (this.IsFingerprint() == false)
                {
                    if (StringUtil.CompareVersion(result.Version, pamcenter_base_version) < 0)
                    {
                        string error = $"当前连接的掌纹中心版本号太低(为 {result.Version})，请升级到 {pamcenter_base_version} 或以上版本";
                        Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode(error)}</div>");
                        /*
                        _ = Task.Run(() =>
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                MessageBox.Show(this, error);
                            }));
                        });
                        */
                        return error;
                    }
                }
                else
                {
                    // 检查指纹中心版本号
                    if (StringUtil.CompareVersion(result.Version, fingerprint_base_version) < 0)
                    {
                        string error = $"当前连接的指纹中心版本号太低(为 {result.Version})，请升级到 {fingerprint_base_version} 或以上版本";
                        Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode(error)}</div>");
                        /*
                        _ = Task.Run(() =>
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                MessageBox.Show(this, error);
                            }));
                        });
                        */
                        return error;
                    }
                }
            }

            return null;
        }

#if OLD
        public void CheckPalmCenterVersion()
        {
            if (string.IsNullOrEmpty(FingerprintManager.Url) == false)
            {
                var result = FingerprintManager.GetVersion();
                if (result.Value == -1)
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{ HttpUtility.HtmlEncode($"获得{GetPalmName()}中心版本号时出错: {result.ErrorInfo}") }</div>");
                else if (this.IsFingerprint() == false)
                {
                    if (StringUtil.CompareVersion(result.Version, pamcenter_base_version) < 0)
                    {
                        string error = $"当前连接的掌纹中心版本号太低(为 {result.Version})，请升级到 {pamcenter_base_version} 或以上版本";
                        Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{ HttpUtility.HtmlEncode(error) }</div>");
                        _ = Task.Run(() =>
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                MessageBox.Show(this, error);
                            }));
                        });
                    }
                }
                else
                {
                    // 检查指纹中心版本号
                    if (StringUtil.CompareVersion(result.Version, fingerprint_base_version) < 0)
                    {
                        string error = $"当前连接的指纹中心版本号太低(为 {result.Version})，请升级到 {fingerprint_base_version} 或以上版本";
                        Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{ HttpUtility.HtmlEncode(error) }</div>");
                        _ = Task.Run(() =>
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                MessageBox.Show(this, error);
                            }));
                        });
                    }
                }
            }
        }
#endif

        // 紧凑日志
        static CompactLog _compactLog = new CompactLog();
        static DateTime _lastCompactTime = DateTime.MinValue;
        static TimeSpan _compactLength = TimeSpan.FromMinutes(10);
        static int _compactAppendCount = 0;

        private void PalmprintManager_Touched(object sender, TouchedEventArgs e)
        {
            // 2021/3/27
            // 注意当 palmcenter 出于不正常状态时，此处会被每秒一次反复到达，处理不当会造成操作历史窗内容爆满。所以这里用了压缩日志
            if (e.Result?.Value == -1
                && Program.MainForm.OperHistory != null)
            {
                _ = _compactLog.Add("从" + Program.MainForm.GetPalmName() + "中心获取消息时出错: {0}", new object[] { e.Result?.ErrorInfo });
                _compactAppendCount++;

                // 每隔一段时间集中显示一次
                if (_lastCompactTime == DateTime.MinValue
                    || DateTime.Now - _lastCompactTime > _compactLength)
                {
                    _compactLog?.WriteToLog((text) =>
                    {
                        WriteErrorLog(text);
                        Program.MainForm.OperHistory?.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(text).Replace("\r\n", "<br/>") + "</div>");
                    });
                    // 移走已经输出的全部条目
                    _compactLog?.RemoveEntry();
                    _lastCompactTime = DateTime.Now;
                }
                return;
            }

            if (e.Quality == -1)
            {
                this.Invoke((Action)(() =>
                {
                    var id = e.Result?.MessageID;
                    this.SetPalmprintMessage($"{e.Message}", id);
                }));
                /*
                {
                    string error = $"palmTouched 提示文字 (e.Quality == -1) {e.Message}";
                    Program.MainForm.OperHistory?.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode(error) + "</div>");
                }
                */
                return;
            }
            else
                Program.MainForm.OperHistory?.AppendHtml($"<div class='debug recpath'>{HttpUtility.HtmlEncode($"{Program.MainForm.GetPalmName()}消息 {e.ToString()}")}</div>");

            // dp2circulation 自己不是在最前面的时候，不进行掌纹 SendKey。这样避免和同时运行的 dp2SSL 冲突(dp2ssl 自己可以轮询掌纹 message)
            if (_isActivated == false)
            {
                // Debug.WriteLine($"_palmprintForm.Focused={_palmprintForm.Focused}");
                if (_palmprintForm != null && _palmprintForm.IsActivated
    && this.ActiveMdiChild is QuickChargingForm)
                {
                    this.Invoke((Action)(() =>
                    {
                        this.Activate();
                    }));
                    ((QuickChargingForm)this.ActiveMdiChild).FocusInput();
                }
                else
                {
                    string error = $"注意此时内务窗口不在最前面，textbox 无法捕获{Program.MainForm.GetPalmName()}输入";
                    Program.MainForm.OperHistory?.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div>");

                    /*
                    // 2021/3/18
                    Program.MainForm.Speak(error);
                    */
                    return;
                }
            }

            // 2021/1/5
            if (string.IsNullOrEmpty(e.Message))
            {
                string error = "palmTouched e.Message 为空";
                Program.MainForm.OperHistory?.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div>");
                return;
            }

            // 2021/10/28
            // 忽略图像消息
            if (e.Message.StartsWith("!image"))
                return;

            if (_disablePalmSendKey == false)
            {
                this.Invoke((Action)(() =>
                {
                    // SendKeys.Send(e.Message + "\r");
                    SendKeys.Send($"pii:{e.Message},tou:80\r");
                    this.SetPalmprintMessage("识别成功 " + e.Message, e.Result?.MessageID);
                }));
            }
            else
            {
                string error = $"{Program.MainForm.GetPalmName()}输入 '{e.Message}' 被丢弃";
                Program.MainForm.OperHistory?.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div>");
            }
        }

        bool _disablePalmSendKey = false;

        public void EnablePalmSendKey()
        {
            _disablePalmSendKey = false;

            if (_palmprintForm != null)
                _palmprintForm.DisableSendKey = false;

            string text = $"重新启用{GetPalmName()}输入";
            Program.MainForm.OperHistory?.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode(text) + "</div>");
        }

        public void DisablePalmSendKey()
        {
            _disablePalmSendKey = true;

            if (_palmprintForm != null)
                _palmprintForm.DisableSendKey = true;

            string text = $"临时禁用{GetPalmName()}输入";
            Program.MainForm.OperHistory?.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode(text) + "</div>");
        }

        #endregion

        #region ProcessManager

        CancellationTokenSource _cancelProcessMonitor = new CancellationTokenSource();

        public void StartProcessManager()
        {
            // 停止前一次的 monitor
            if (_cancelProcessMonitor != null)
            {
                _cancelProcessMonitor.Cancel();
                _cancelProcessMonitor.Dispose();

                _cancelProcessMonitor = new CancellationTokenSource();
            }

            // if (ProcessMonitor == true)
            {
                List<DigitalPlatform.IO.ProcessInfo> infos = new List<DigitalPlatform.IO.ProcessInfo>();
                if (string.IsNullOrEmpty(this.FaceReaderUrl) == false
                    && ProcessManager.IsIpcUrl(this.FaceReaderUrl))
                    infos.Add(new DigitalPlatform.IO.ProcessInfo
                    {
                        Name = "人脸中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/dp2-人脸中心",
                        MutexName = "{E343F372-13A0-482F-9784-9865B112C042}"
                    });
                if (string.IsNullOrEmpty(this.RfidCenterUrl) == false
                    && ProcessManager.IsIpcUrl(this.RfidCenterUrl)
                    && this.AutoStartRfidCenter)
                    infos.Add(new DigitalPlatform.IO.ProcessInfo
                    {
                        Name = "RFID中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/RFID中心",
                        MutexName = "{CF1B7B4A-C7ED-4DB8-B5CC-59A067880F92}"
                    });

                if (
#if NEWFINGER
                    (string.IsNullOrEmpty(this.PalmprintReaderUrl) == false
                    && ProcessManager.IsFingerprintUrl(this.PalmprintReaderUrl))
#else
                    ||
                    (string.IsNullOrEmpty(this.FingerprintReaderUrl) == false
                    && ProcessManager.IsIpcUrl(this.FingerprintReaderUrl))
#endif
                    )
                    infos.Add(new DigitalPlatform.IO.ProcessInfo
                    {
                        Name = "指纹中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/dp2-指纹中心",
                        MutexName = "{75FB942B-5E25-4228-9093-D220FFEDB33C}"
                    });

                ProcessManager.Start(infos,
                    (info, text) =>
                    {
                        WriteInfoLog($"{info.Name} {text}");
                    },
                    _cancelProcessMonitor.Token);
            }
        }

        /*
        // 启动指纹监控任务。这个任务会不断尝试 GetMessage()
        public void StartFingerprintTask()
        {

        }
        */

        #endregion

        // 将 dp2circulation.xml 文件中绿色安装目录或者 ClickOnce 安装的数据目录移动到用户目录
        int MoveDp2circulationXml(out string strError)
        {
            strError = "";

            string strTargetFileName = Path.Combine(this.UserDir, "dp2circulation.xml");
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

            string strSourceFileName = Path.Combine(strSourceDirectory, "dp2circulation.xml");
            if (File.Exists(strSourceFileName) == false)
                return 0;   // 没有源文件，无法做什么

            try
            {
                File.Copy(strSourceFileName, strTargetFileName, false);
            }
            catch (Exception ex)
            {
                strError = "复制文件 '" + strSourceFileName + "' 到 '" + strTargetFileName + "' 时出现异常：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            return 0;
        }

        int RedirectUserDir(out string strError)
        {
            strError = "";

            string strRedirectFileName = Path.Combine(this.UserDir, "redirect.txt");
            if (File.Exists(strRedirectFileName) == false)
                return 0;

            try
            {
                using (StreamReader sr = new StreamReader(strRedirectFileName))
                {
                    string strLine = sr.ReadLine();
                    if (Directory.Exists(strLine) == false)
                    {
                        strError = "目录 '" + strLine + "' 尚未创建";
                        return -1;
                    }

                    this.UserDir = strLine;
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "读取文件 '" + strRedirectFileName + "' 时出现异常：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        bool _copyGreenError = false;   // 第一次 CopyGreen() 是否出错

        // 复制出一个绿色安装包
        // parameters:
        //      bForce  是否要在无论任何状态(包括本来就是绿色运行状态)下都强制创建？
        // return:
        //      false   没有启动
        //      true    启动了。注意，有可能会启动了但后来出错了。
        bool CopyGreen(bool bForce = false)
        {
            int nRet = 0;
            string strError = "";

            // 本身如果是绿色安装包，没有必要再次复制出绿色安装包
            if (bForce == false && ApplicationDeployment.IsNetworkDeployed == false)
            {
                // this.DisplayBackgroundTextLn("当前已经处在绿色运行状态，无法创建应急绿色安装包");
                return false;
            }

            string strProgramDir = Environment.CurrentDirectory;
            string strTargetDir = "c:\\dp2circulation";

            if (PathUtil.IsEqual(strProgramDir, strTargetDir) == true)
                return false;

            this.DisplayBackgroundTextLn("正在创建应急绿色安装包 ...");

            StringBuilder debugInfo = new StringBuilder();
            // return:
            //      -2  权限不够
            //      -1  出错
            //      0   没有必要创建。(也许是因为当前程序正是从备用位置启动的、版本没有发生更新)
            //      1   已经创建
            nRet = GreenProgram.CopyGreen(
                Program.ClientVersion,
                strProgramDir,
                this.DataDir,
                strTargetDir,
                debugInfo,
                out strError);
            if (nRet == -1)
            {
                ShowMessageBox("创建应急绿色安装包时出错: " + strError);
                this.DisplayBackgroundTextLn(strError);
                // 发送给 dp2003.com
                ReportError("dp2circulation 创建应急绿色安装包时出错", strError + "\r\n\r\nDebug Info:\r\n" + debugInfo.ToString());
                _copyGreenError = true;
#if NO
                string strText = strError;
                if (string.IsNullOrEmpty(strText) == true)
                    return;

                strText += "\r\n\r\n===\r\n" + PackageEventLog.GetEnvironmentDescription().Replace("\t", "    ");

                try
                {
                    // 发送报告
                    nRet = LibraryChannel.CrashReport(
                        this.GetCurrentUserName() + "@" + this.ServerUID,
                        "dp2circulation 创建备用绿色安装包时出错",
                        strText,
                        out strError);
                }
                catch (Exception ex)
                {
                    strError = "CrashReport() (创建备用绿色安装包时出错) 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                    this.WriteErrorLog(strError);
                }
#endif
            }
            else if (nRet == -2)
            {
                this.DisplayBackgroundTextLn("创建应急绿色安装包时出错: " + strError);
                return false;
            }
            else if (nRet == 0)
            {
                this.DisplayBackgroundTextLn("没有必要创建: " + strError);
                return false;
            }
            else
            {
                string path = Path.Combine(strTargetDir, "dp2circulation.exe");
                try
                {
                    GreenProgram.CreateShortcutToDesktop(
                       "内务",  // "内务绿色"
                       path,
                       false);
                }
                catch (Exception ex)
                {
                    strError = $"dp2circulation 创建应急绿色安装包快捷方式时出现异常(path='{path}'): {ExceptionUtil.GetDebugText(ex)}";
                    this.ReportError("dp2circulation 创建应急绿色安装包快捷方式时出现异常", $"(安静报错){strError} (path='{path}')");
                    this.DisplayBackgroundTextLn(strError);
                }

                this.DisplayBackgroundTextLn("应急绿色安装包已经成功创建于 " + strTargetDir + "。");
            }

            return true;
        }

        void ShowMessageBox(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(ShowMessageBox), strText);
                return;
            }
            MessageBox.Show(this, strText);
        }

        #region Background Form

        int _backgroundOpenCount = 0;

        void OpenBackgroundForm()
        {
            _backgroundOpenCount++;
            if (_backgroundOpenCount == 1)
            {
                if (this.m_backgroundForm != null)
                    return;

                // 获得MdiClient窗口
                {
                    Type t = typeof(Form);
                    PropertyInfo pi = t.GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic);
                    this.MdiClient = (MdiClient)pi.GetValue(this, null);
                    this.MdiClient.SizeChanged -= new EventHandler(MdiClient_SizeChanged);
                    this.MdiClient.SizeChanged += new EventHandler(MdiClient_SizeChanged);

                    this.TryInvoke(() =>
                    {
                        m_backgroundForm = new BackgroundForm();
                        m_backgroundForm.MdiParent = this;
                        m_backgroundForm.Dock = DockStyle.Fill;
                        m_backgroundForm.Show();
                    });
                }

                this.TryInvoke(() =>
                {
                    ClearBackground();
                });

                LinkStopToBackgroundForm(true);
            }
        }

        void ClearBackground()
        {
            if (m_backgroundForm == null)
                return;

            Debug.Assert(string.IsNullOrEmpty(this.DataDir) == false, "");
            {
                string strCssUrl = PathUtil.MergePath(this.DataDir, "background.css");
                string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

                try
                {
                    HtmlDocument doc = m_backgroundForm.WebBrowser.Document;

                    if (doc == null)
                    {
                        // webBrowser.Navigate("about:blank");
                        Global.Navigate(m_backgroundForm.WebBrowser, "about:blank");
                        doc = m_backgroundForm.WebBrowser.Document;
                    }

                    doc = doc.OpenNew(true);

                    string strBodyClass = " class='clickonce' ";
                    if (ApplicationDeployment.IsNetworkDeployed == false)
                        strBodyClass = " class='green' ";

                    string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>";

                    Global.WriteHtml(m_backgroundForm.WebBrowser,
                        // "<html><head>" 
                        strHead + strLink + "</head><body" + strBodyClass + ">");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(this, "dp2circulation 所需的 IE 浏览器控件出现异常: " + ExceptionUtil.GetDebugText(ex));
                    //Application.Exit();
                    Program.PromptAndExit(this, "dp2circulation 所需的 IE 浏览器控件出现异常: " + ExceptionUtil.GetDebugText(ex));
                }
            }
        }

        void LinkStopToBackgroundForm(bool bLink)
        {
            if (this.stopManager == null)
                return;

            Debug.Assert(stopManager != null, "");

            if (bLink)
                this.stopManager.OnDisplayMessage += new DisplayMessageEventHandler(stopManager_OnDisplayMessage);
            else
                this.stopManager.OnDisplayMessage -= new DisplayMessageEventHandler(stopManager_OnDisplayMessage);
        }

        void MdiClient_SizeChanged(object sender, EventArgs e)
        {
            if (m_backgroundForm != null)
                m_backgroundForm.Size = new System.Drawing.Size(this.MdiClient.ClientSize.Width, this.MdiClient.ClientSize.Height);
        }

        void CloseBackgroundForm()
        {
            _backgroundOpenCount--;
            if (_backgroundOpenCount == 0)
            {
                var backgroundForm = this.m_backgroundForm;
                if (backgroundForm != null)
                {
                    LinkStopToBackgroundForm(false);

                    // TODO: 最好有个淡出的功能
                    this.MdiClient.SizeChanged -= new EventHandler(MdiClient_SizeChanged);
                    this.m_backgroundForm = null;   // 先摘离 this，然后再 Close()。避免并发使用的其它地方遇到 backgroundForm.Created 为 false 情况
                    this.TryInvoke(() =>
                    {
                        /*
                        this.m_backgroundForm.MdiParent = null;
                        this.m_backgroundForm.Dock = DockStyle.None;
                        */
                        backgroundForm.Close();
                    });
                }
            }
        }

        void ShowBackgroundProgress(string strID, string strText)
        {
            if (this.m_backgroundForm != null)
                this.m_backgroundForm.ShowProgressMessage(strID, strText);
        }

        // 自动带一个回车
        void DisplayBackgroundTextLn(string strText)
        {
            DisplayBackgroundText(strText + "\r\n");
        }

        void DisplayBackgroundText(string strText)
        {
            if (m_backgroundForm != null)
            {
                /*
                if (m_backgroundForm.InvokeRequired)
                {
                    m_backgroundForm.Invoke(new Action<string>(DisplayBackgroundText), strText);
                    return;
                }
                */
                // m_backgroundForm.AppendHtml(HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>"));
                var lines = strText.Replace("\r\n", "\n").Split('\n');
                foreach (var line in lines)
                {
                    m_backgroundForm.AppendHtml("<div>" + HttpUtility.HtmlEncode(line) + "</div>");
                }
            }
        }

        void stopManager_OnDisplayMessage(object sender, DisplayMessageEventArgs e)
        {
            if (m_backgroundForm != null)
            {
                this.TryInvoke(() =>
                {
                    if (e.Message != m_strPrevMessageText)
                    {
                        m_backgroundForm.AppendHtml(HttpUtility.HtmlEncode(e.Message) + "<br/>");
                        m_strPrevMessageText = e.Message;
                    }
                });
            }
        }

        #endregion

        // 判断两个文件的版本号是否一致
        static bool VersionChanged(string filename1, string filename2)
        {
            if (File.Exists(filename1) == false)
                return false;
            if (File.Exists(filename2) == false)
                return false;
            string strVersion1 = GetVersion(filename1);
            string strVersion2 = GetVersion(filename2);
            if (strVersion1 == strVersion2)
                return false;
            return true;
        }

        // 获得一个文件的版本号
        static string GetVersion(string filename)
        {
            string strExt = Path.GetExtension(filename).ToLower();
            if (strExt == ".xml")
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(filename);
                }
                catch
                {
                    return null;
                }

                if (dom.DocumentElement == null)
                    return null;
                return dom.DocumentElement.GetAttribute("version");
            }

            return null;
        }

        // 从数据目录 default 子目录复制配置文件到用户目录
        // 如果用户目录中已经有同名文件存在，则不复制了
        // 这种做法，是为了让用户可以修改实际使用的配置文件，并且在升级安装的时候，用户目录内的文件不会被安装修改原始的配置文件过程所覆盖
        // return:
        //      true    成功
        //      false   失败。需要立即返回
        bool CopyDefaultCfgFiles(out string strError)
        {
            strError = "";

            // 从 数据目录 default 子目录中列举所有文件名
            string strDefaultDir = Path.Combine(this.DataDir, "default");
            DirectoryInfo di = new DirectoryInfo(strDefaultDir);
            if (di.Exists == false)
            {
                strError = "配置文件目录 '" + strDefaultDir + "' 不存在，无法进行配置文件初始化操作。\r\n\r\n建议尽量直接从 dp2003.com 以 ClickOnce 方式安装 dp2circulation，以避免绿色安装时复制配置文件不全带来的麻烦";
                goto ERROR1;
            }

            // 必须具备的一些配置文件名
            List<string> filenames = new List<string>();
            filenames.Add("objectrights.xml");
            filenames.Add("inventory_item_browse.xml");
            filenames.Add("inventory.css");
            // filenames.Add("charginghistory.css");
            filenames.Add("nonephoto.png");
#if NO
            filenames.Add("comment_change_actions.xml");
            filenames.Add("issue_change_actions.xml");
            filenames.Add("item_change_actions.xml");
            filenames.Add("order_change_actions.xml");
            filenames.Add("patron_change_actions.xml");
            filenames.Add("856_change_actions.xml");
#endif
            filenames.Add("patronrights.xml");

            FileInfo[] fis = di.GetFiles("*.*");
            foreach (FileInfo fi in fis)
            {
                if (filenames.IndexOf(fi.Name.ToLower()) != -1)
                    filenames.Remove(fi.Name.ToLower());

                string strSourceFileName = fi.FullName;
                string strTargetFileName = Path.Combine(this.UserDir, fi.Name);
                if (File.Exists(strTargetFileName) == false    // 偶尔会出现判断错误
                    || VersionChanged(strSourceFileName, strTargetFileName) == true)
                {
#if DEBUG
                    if (File.Exists(strSourceFileName) == false)
                    {
                        strError = "配置文件 '" + strSourceFileName + "' 不存在，无法复制到用户目录。\r\n\r\n建议尽量直接从 dp2003.com 以 ClickOnce 方式安装 dp2circulation，以避免绿色安装时复制配置文件不全带来的麻烦";
                        goto ERROR1;
                    }
#endif
                    try
                    {
                        File.Copy(strSourceFileName, strTargetFileName, true);
                    }
                    catch (IOException)
                    {
                        // https://msdn.microsoft.com/en-us/library/ms681382(v=vs.85).aspx
                        // ERROR_FILE_EXISTS
                        // 80 (0x50)
                        // The file exists.
                        int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                        if (error == 80)
                            continue;
                        throw;
                    }
                }
            }

            if (filenames.Count > 0)
            {
                strError = "配置文件目录 '" + strDefaultDir + "' 中缺乏以下必备的配置文件: " + StringUtil.MakePathList(filenames) + "。\r\n\r\n建议尽量直接从 dp2003.com 以 ClickOnce 方式安装 dp2circulation，以避免绿色安装时复制配置文件不全带来的麻烦";
                goto ERROR1;
            }
            return true;
        ERROR1:
            // MessageBox.Show(this, strError);
            return false;
        }

        // 复制选定的部分配置文件到用户目录。会覆盖用户目录中已经存在的文件。此功能就是恢复出厂设置的功能
        public int CopyDefaultCfgFiles(List<string> filenames,
            out string strError)
        {
            strError = "";

            string strDefaultDir = Path.Combine(this.DataDir, "default");
            DirectoryInfo di = new DirectoryInfo(strDefaultDir);
            if (di.Exists == false)
            {
                strError = "配置文件目录 '" + strDefaultDir + "' 不存在";
                return -1;
            }

            foreach (string filename in filenames)
            {
                string strSourceFileName = Path.Combine(strDefaultDir, filename);
                string strTargetFileName = Path.Combine(this.UserDir, filename);
                if (File.Exists(strSourceFileName) == false)
                {
                    strError = "源文件 '" + strSourceFileName + "' 不存在";
                    return -1;
                }

                File.Copy(strSourceFileName, strTargetFileName, true);
            }

            return 0;
        }

        // InitialProperties() 是否至少执行成功过一次
        static bool _initialPropertiesComplete = false;

        // 2020/11/8
        // 确保 MainForm 连接成功过 dp2library 服务器
        // return:
        //      false   没有成功
        //      true    成功
        public async Task<bool> EnsureConnectLibraryServerAsync()
        {
            if (_initialPropertiesComplete)
                return true;
            await Task.Factory.StartNew(
                () =>
                {
                    InitialProperties(false, false);
                },
this._cancel.Token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
            /*
            await Task.Run(() =>
            {
                InitialProperties(false, false);
            });
            */
            if (_initialPropertiesComplete)
                return true;
            return false;
        }

        // 2021/9/4
        // 重新从 dp2library 服务器装载各种属性参数
        public void BeginRefreshProperties()
        {
            _ = Task.Factory.StartNew(
                () =>
                {
                    InitialProperties(false, false);
                },
this._cancel.Token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
            /*
            _ = Task.Run(() =>
            {
                InitialProperties(false, false);
            });
            */
        }

        // 初始化各种参数
        bool InitialProperties(bool bFullInitial, bool bRestoreLastOpenedWindow)
        {
            int nRet = 0;

            // 先禁止界面
            if (bFullInitial == true)
            {
                EnableControls(false);
                this.TryInvoke(() =>
                {
                    this.MdiClient.Enabled = false;
                });
            }

            var looping = Looping(null);
            OpenBackgroundForm();   // 2024/6/19
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

                    // 如果必要，首先出现配置画面，便于配置 dp2library 的 URL
                    string strLibraryServerUrl = this.AppInfo.GetString(
                        "config",
                        "circulation_server_url",
                        "");
                    if (String.IsNullOrEmpty(strLibraryServerUrl) == true)
                    {
                        // http://stackoverflow.com/questions/860459/determine-os-using-environment-osversion
                        // 判断当前操作系统版本
                        if (Environment.OSVersion.Version.Major == 5)
                        {
#if NO
                            if (Environment.OSVersion.Version.Minor == 1)
                            {
                                // XP
                            }
                            else if (Environment.OSVersion.Version.Minor == 2)
                            {
                                // Server 2003.  XP 64-bit will also fall in here.
                            }
#endif
                            this.MessageBoxShow("dp2Circulation 不支持 Windows XP / Windows Server 2003 操作系统版本。请在 Windows Vista 及以上版本安装运行");
                            if (Control.ModifierKeys != Keys.Control)
                            {
                                // Application.Exit();
                                Program.PromptAndExit((IWin32Window)null, "dp2Circulation 不支持 Windows XP / Windows Server 2003 操作系统版本。请在 Windows Vista 及以上版本安装运行");
                                return false;
                            }
                        }
                        else if (Environment.OSVersion.Version.Major >= 6)
                        {
                            // Vista on up
                        }

                        var ret = (bool)this.Invoke((Func<bool>)(() =>
                        {
                            using (FirstRunDialog first_dialog = new FirstRunDialog())
                            {
                                MainForm.SetControlFont(first_dialog, this.DefaultFont);
                                // first_dialog.MainForm = this;
                                first_dialog.StartPosition = FormStartPosition.CenterScreen;
                                if (first_dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                                {
                                    // Application.Exit();
                                    Program.PromptAndExit(null, "取消首次设置");
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

                                return true;
                            }
                        }));
                        if (ret == false)
                            return false;
                    }
                    else
                    {
                        // 以前已经安装的情况
                        if (Environment.OSVersion.Version.Major == 5)
                        {
                            this.MessageBoxShow("尊敬的用户，dp2Circulation 在 2015 年 12 月 31 日以后将不再支持 Windows XP / Windows Server 2003 操作系统版本。请尽快升级您的 Windows 操作系统到 Vista 及以上版本。祝工作顺利。\r\n\r\n数字平台敬上");
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
                    this.TryInvoke(() =>
                    {
                        _verified = false;
                        // return:
                        //      -1  出错
                        //      0   放弃
                        //      1   成功
                        nRet = this.VerifySerialCode("", false, out strError);
                        if (nRet == 1)
                            _verified = true;
                    });
#else
                    this.MenuItem_resetSerialCode.Visible = false;
#endif

                    bool bLogin = this.AppInfo.GetBoolean(
                        "default_account",
                        "occur_per_start",
                        true);
                    if (bLogin == true
                        && bFirstDialog == false   // 首次运行的对话框出现后，登录对话框就不必出现了
                        && PrintLabelMode == false)
                    {
                        this.TryInvoke(() =>
                        {
                            var text = "首次登录";
                            if (string.IsNullOrEmpty(this.WelcomeText) == false)
                                text = this.WelcomeText.Replace("\\r\\n", "\r\n");
                            SetDefaultAccount(
                                null,
                                "登录", // "指定缺省帐户",
                                text, // "请指定后面操作中即将用到的缺省帐户信息。",
                                LoginFailCondition.None,
                                this,
                                false);
                        });
                    }
                    else if (PrintLabelMode == false)
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

                if (this.PrintLabelMode == false && this.LibraryServerUrl != "[None]")
                {
                    try
                    {
                        // 2021/6/19
                        // 设置全局加密协议
                        {
                            var securityProtocol = this.AppInfo.GetString(
                                    "global",
                                    "securityProtocol",
                                    "SystemDefault");
                            Enum.TryParse<SecurityProtocolType>(securityProtocol, out SecurityProtocolType protocol);
                            System.Net.ServicePointManager.SecurityProtocol = protocol;
                        }

                        // 2013/6/18
                        nRet = TouchServer(false);
                        if (nRet == -1)
                            goto END1;

                        // 只有在前一步没有出错的情况下才探测版本号
                        if (nRet == 0)
                        {
                            // 检查dp2Library版本号
                            // return:
                            //      -2  出现严重错误，希望退出 Application
                            //      -1  一般错误
                            //      0   dp2Library的版本号过低。警告信息在strError中
                            //      1   dp2Library版本号符合要求
                            nRet = CheckVersion(false, out strError);
                            if (nRet == -2)
                            {
                                if (string.IsNullOrEmpty(strError) == false)
                                    this.MessageBoxShow(strError);
                                // Application.Exit();
                                Program.PromptAndExit(null,
                                    string.IsNullOrEmpty(strError) == false ? strError : "CheckVersion Fail...");
                                return false;
                            }
                            if (nRet == -1)
                            {
                                this.MessageBoxShow(strError);
                                goto END1;
                            }
                            if (nRet == 0)
                                this.MessageBoxShow(strError);
                        }

                        // 获得图书馆一般信息
                        nRet = GetLibraryInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得各种类型的数据库的检索途径
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

                        // 2020/4/24
                        // TODO: 如果前面的语句跳过去了，则 this.NormalDbProperties 为 null，可能会引起后面 Assertion 报错
                        nRet = InitialNormalDbProperties(false);
                        if (nRet == -1)
                            goto END1;

                        //

                        // 获得索取号配置信息
                        // 2009/2/24 
                        nRet = GetCallNumberInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得 RFID 配置信息
                        // 2019/1/11
                        nRet = GetRfidInfo(false);
                        if (nRet == -1)
                            goto END1;

                        // 获得条码校验规则
                        // 2019/6/1
                        nRet = GetBarcodeValidationInfo();
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
                            this.MessageBoxShow(strError);

                        // 2022/3/10
                        ClearValueTableCache();

                        _ = Task.Run(async () =>
                        {
                            string location = AppInfo.GetString("global", "currentLocation", "");

                            // 填充列表
                            await FillLibraryCodeListMenuAsync();

                            this.Invoke((Action)(() =>
                            {
                                // 复原以前的选择
                                SetCurrentLocation(location);
                            }));
                        });

                        _initialPropertiesComplete = true;

                        // this.BeginInvoke(new Action(FillLibraryCodeListMenu));
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog($"InitialProperties() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        return false;
                    }
                    finally
                    {
                        // EndSearch();
                    }
                }

            END1:

#if NO
                // 安装条码字体
                InstallExternalFont("C39HrP24DhTt", Path.Combine(this.DataDir, "b3901.ttf"));
                // 安装 OCR-B 10 BT 字体
                InstallExternalFont("OCR-B 10 BT", Path.Combine(this.DataDir, "ocr-b.ttf"));
#endif


                // 安装条码字体
                InstallExternalFont(Path.Combine(this.DataDir, "b3901.ttf"));
                // 安装 OCR-B 10 BT 字体
                InstallExternalFont(Path.Combine(this.DataDir, "ocr-b.ttf"));


                // TODO: 这里有一定问题。最好临时申请一个 stop， 然后用后释放
                looping.Progress.SetMessage("正在删除以前遗留的临时文件...");

                /*
Type: System.UnauthorizedAccessException
Message: Access to the path 'D:\System Volume Information\' is denied.
Stack:
   at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
   at System.IO.FileSystemEnumerableIterator`1.CommonInit()
   at System.IO.FileSystemEnumerableIterator`1..ctor(String path, String
originalUserPath, String searchPattern, SearchOption searchOption,
SearchResultHandler`1 resultHandler)
   at System.IO.DirectoryInfo.InternalGetFiles(String searchPattern,
SearchOption searchOption)
   at System.IO.DirectoryInfo.GetFiles()
   at dp2Circulation.MainForm.DeleteAllTempFiles(String strDataDir)
   at dp2Circulation.MainForm.DeleteAllTempFiles(String strDataDir)
   at dp2Circulation.MainForm.InitialProperties(Boolean bFullInitial,
Boolean bRestoreLastOpenedWindow)
 
 
dp2Circulation 版本: dp2Circulation, Version=2.4.5715.19592,
Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3
                 * * */
                try
                {
                    DeleteAllTempFiles(this.DataDir);
                }
                catch (System.UnauthorizedAccessException ex)
                {
#if NO
                    MessageBox.Show(this, "在试图删除数据目录 '"+this.DataDir+"' 内临时文件时出错: " + ex.Message
                        + "\r\n\r\n既然您把软件安装到这个目录或者试图从这里运行软件，就该给当前 Windows 用户赋予针对这个目录的列目录和删除文件的权限");
                    Application.Exit();
#endif
                    Program.PromptAndExit(this, "在试图删除数据目录 '" + this.DataDir + "' 内临时文件时出错: " + ex.Message
                        + "\r\n\r\n既然您把软件安装到这个目录或者试图从这里运行软件，就该给当前 Windows 用户赋予针对这个目录的列目录和删除文件的权限");
                    return false;
                }

                try
                {
                    DeleteAllTempFiles(this.UserTempDir);
                }
                catch (System.UnauthorizedAccessException ex)
                {
#if NO
                    MessageBox.Show(this, "在试图删除用户临时目录 '" + this.UserTempDir + "' 内临时文件时出错: " + ex.Message
                        + "\r\n\r\n应给当前 Windows 用户赋予针对这个目录的列目录和删除文件的权限");
                    Application.Exit();
#endif
                    Program.PromptAndExit(this, "在试图删除用户临时目录 '" + this.UserTempDir + "' 内临时文件时出错: " + ex.Message
                        + "\r\n\r\n应给当前 Windows 用户赋予针对这个目录的列目录和删除文件的权限");
                    return false;
                }

                looping.Progress.SetMessage("正在复制报表配置文件...");
                // 拷贝目录
                nRet = PathUtil.CopyDirectory(Path.Combine(this.DataDir, "report_def"),
                    Path.Combine(this.UserDir, "report_def"),
                    false,
                    out strError);
                if (nRet == -1)
                    this.MessageBoxShow(strError);

                looping.Progress.SetMessage("");
#if NO
                if (Stop != null) // 脱离关联
                {
                    Stop.Unregister();	// 和容器关联
                    Stop = null;
                }
#endif

                // 2013/12/4
                if (InitialClientScript(out strError) == -1)
                    this.MessageBoxShow(strError);

                // 初始化历史对象，包括C#脚本
                if (this.OperHistory == null)
                {
                    looping.Progress.SetMessage("正在初始化操作历史面板 ...");

                    this.OperHistory = new OperHistory();
                    nRet = this.OperHistory.Initial(// this,
                        this.webBrowser_history,
                        out strError);
                    if (nRet == -1)
                    {
                        this.ReportError("dp2circulation 创建 OperHistory 时出错", strError);
                        this.MessageBoxShow("初始化 OperHistory 时出错: " + strError);
                    }
                    // this.timer_operHistory.Start();
                }

                if (Global.IsKbInstalled("KB2468871") == true)
                {
                    // 初始化 MessageHub
                    this.MessageHub = new MessageHub();
                    this.Invoke((Action)(() =>
                    {
                        this.MessageHub.Initial(this.webBrowser_messageHub);
                    }));
                }
                else
                {
                    // 在 webBrowser 中显示警告信息
                    string strCssUrl = Path.Combine(this.DataDir, "history.css");
                    string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
                    Global.SetHtmlString(this.webBrowser_messageHub,
                        "<html><head>" + strLink + "</head><body>" + "<p>需要安装 Windows 更新 KB2468871 以后才能使用消息相关功能</p>",
                        this.DataDir,
                        "error");
                }

                // 启动自动更新。m_backgroundForm 延迟关闭。但取消和 stop 的关联
                this.Invoke((Action)(() =>
                {
                    ClearBackground();
                }));

                // 若第一次复制绿色版本失败，则需要再进行一次
                if (_copyGreenError == true)
                {
                    _ = Task.Factory.StartNew(
    () =>
    {
        CopyGreen();
    },
this._cancel.Token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
                    // _ = Task.Factory.StartNew(() => CopyGreen());
                }

                // 注意此函数中会用到背景窗口
                BeginUpdateClickOnceApplication();    // 自动探测更新 dp2circulation

                BeginUpdateGreenApplication(); // 自动进行绿色更新
            }
            finally
            {
                looping.Dispose();

                // 然后许可界面
                if (bFullInitial == true)
                {
                    this.TryInvoke(() =>
                    {
                        this.MdiClient.Enabled = true;
                    });
                    EnableControls(true);
                }

#if NO
                if (this.m_backgroundForm != null)
                {
                    // TODO: 最好有个淡出的功能
                    this.stopManager.OnDisplayMessage -= new DisplayMessageEventHandler(stopManager_OnDisplayMessage);
                    //this.MdiClient.SizeChanged -= new EventHandler(MdiClient_SizeChanged);
                    //this.m_backgroundForm.Close();
                    //this.m_backgroundForm = null;
                }
#endif
                CloseBackgroundForm();  // 2024/6/19
            }


            if (bRestoreLastOpenedWindow == true)
            {
                this.TryInvoke(() =>
                {
                    if (PrintLabelMode)
                        OpenWindow<LabelPrintForm>();
                    else
                        RestoreLastOpenedMdiWindow();
                });
            }

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

                // 这是 zkfingerprint 要求的动作
                // 初始化指纹高速缓存
                // FirstInitialFingerprintCache();
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
            string strError = "";
        REDO:
            /*
            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在连接服务器 " + channel.Url + " ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "settimeout:0:1:0");    // 这里通常是第一次连接服务器，超时时间不可太长。settimeout:0:1:0 的意思是强制设置为 1 分钟。而 timeout:0:1:0 则是如果短于 1 分钟则设置为 1 分钟，这个 channel 以前遗留的 .Timeout 值可能很大(比如 40 分钟)显然是不符合这里的要求的
            looping.Progress.SetMessage("正在连接服务器 " + channel.Url + " ...");
            try
            {
                long lRet = channel.GetClock(
                    looping.Progress,
                    out string strTime,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                    {
                        // 通讯安全性问题，时钟问题
                        strError = strError + "\r\n\r\n有可能是前端机器时钟和服务器时钟差异过大造成的";
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
            }

            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
        }

        string _serverVersion = "0.0";

        // 0 表示2.1以下。2.1和以上时才具有的获取版本号功能
        /// <summary>
        /// 当前连接的 dp2Library 版本号
        /// </summary>
        public string ServerVersion
        {
            get
            {
                return this._serverVersion;
            }
            set
            {
                this._serverVersion = value;
            }
        }

        /// <summary>
        /// 当前连接的 dp2library 的 uid
        /// </summary>
        public string ServerUID { get; set; }   // 注意初始值为 null

        /// <summary>
        /// 当前连接的 dp2library 的失效期
        /// </summary>
        public string ExpireDate { get; set; }

        // return:
        //      -2  出现严重错误，希望退出 Application
        //      -1  一般错误
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

#if NO
            if (bPrepareSearch == true)
            {
                int nRet = PrepareSearch();
                if (nRet == 0)
                {
                    strError = "PrepareSearch() error";
                    return -1;
                }
            }
#endif
            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检查服务器 " + channel.Url + " 的版本号, 请稍候 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "settimeout:0:1:0");
            looping.Progress.SetMessage("正在检查服务器 " + channel.Url + " 的版本号, 请稍候 ...");
            try
            {

#if NO
                strError = "测试退出";
                return -2;
#endif

                long lRet = channel.GetVersion(looping.Progress,
    out string strVersion,
    out string strUID,
    out strError);
                if (lRet == -1)
                {
                    if (channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                    {
                        // 原来的dp2Library不具备GetVersion() API，会走到这里
                        this.ServerVersion = "0.0";
                        this.ServerUID = "";
                        strError = "当前 dp2Circulation 版本需要和 dp2Library 2.1 或以上版本配套使用 (而当前 dp2Library 版本号为 '2.0或以下' )。请升级 dp2Library 到最新版本。";
                        return 0;
                    }

                    strError = "针对服务器 " + channel.Url + " 获得版本号的过程发生错误：" + strError;
                    return -1;
                }

                this.ServerUID = strUID;

#if NO
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
#endif
                if (string.IsNullOrEmpty(strVersion) == true)
                    strVersion = "2.0";

                this.ServerVersion = strVersion;

                string dp2library_base_version = "2.60"; // 2.33
                if (StringUtil.CompareVersion(strVersion, dp2library_base_version) < 0)   // 2.12
                {
                    // strError = "当前 dp2Circulation 版本必须和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + strVersion + " )。\r\n\r\n请立即升级 dp2Library 到最新版本。";
                    strError = "dp2 前端所连接的 dp2library 版本必须升级为 " + dp2library_base_version + " 以上时才能使用 (当前 dp2library 版本为 " + strVersion + ")\r\n\r\n请立即升级 dp2Library 到最新版本。\r\n\r\n注：升级服务器的操作非常容易：\r\n1) 若是 dp2 标准版，请系统管理员在服务器机器上，运行 dp2installer(dp2服务器安装工具) 即可。这个模块的安装页面是 http://dp2003.com/dp2installer/v1/publish.htm 。\r\n2) 若是单机版或小型版，反复重启 dp2libraryxe 模块多次即可自动升级。\r\n\r\n亲，若有任何问题，请及时联系数字平台哟 ~";
                    if (this.AppInfo != null)
                        this.AppInfo.Save();
                    return -2;
                }

#if SN
                if (this.TestMode == true && StringUtil.CompareVersion(this.ServerVersion, "2.34") < 0)
                {
                    strError = "dp2Circulation 的评估模式只能在所连接的 dp2library 版本为 2.34 以上时才能使用 (当前 dp2library 版本为 " + this.ServerVersion.ToString() + ")";
                    if (this.AppInfo != null)
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
                        strError = "";
                        return -2;
                    }
                    else
                    {
                        MenuItem_resetSerialCode_Click(this, new EventArgs());
                        strError = "重设序列号后退出";
                        return -2;
                    }
                }
#endif
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 1;
        }

        // 获得各种类型的数据库的检索途径
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            // TODO: 在函数因为无法获得Channel而返回前，是否要清空相关的检索途径数据结构?
            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在列检索途径 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在列检索途径 ...",
                "settimeout:0:1:0");
            try
            {
                // 获得书目库的检索途径
                long lRet = channel.ListDbFroms(
                    looping.Progress,
                    "biblio",
                    this.Lang,
                    out BiblioDbFromInfo[] infos,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出书目库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.BiblioDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());

                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.6") >= 0)
                {
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
    "authority",
    this.Lang,
    out infos,
    out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出规范库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.AuthorityDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());
                }

                // 获得读者库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(
                    looping.Progress,
    "reader",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出读者库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                if (infos != null && this.BiblioDbFromInfos != null
                    && infos.Length > 0 && this.BiblioDbFromInfos.Length > 0
                    && infos[0].Caption == this.BiblioDbFromInfos[0].Caption)
                {
                    // 如果第一个元素的caption一样，则说明GetDbFroms API是旧版本的，不支持获取读者库的检索途径功能
                    this.ReaderDbFromInfos = new BiblioDbFromInfo[] { };
                }
                else
                {
                    this.ReaderDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());    // infos;
                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.11") >= 0)
                {
                    // 获得实体库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "item",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出实体库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.ItemDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());

                    // 获得期库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "issue",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出期库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.IssueDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());

                    // 获得订购库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "order",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出订购库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.OrderDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());

                    // 获得评注库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "comment",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出评注库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.CommentDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());
                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.17") >= 0)
                {
                    // 获得发票库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "invoice",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出发票库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.InvoiceDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());

                    // 获得违约金库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "amerce",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出违约金库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.AmerceDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());

                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.47") >= 0)
                {
                    // 获得预约到书库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(
                        looping.Progress,
        "arrived",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出预约到书库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.ArrivedDbFromInfos = (infos ?? Array.Empty<BiblioDbFromInfo>());
                }

                // 需要检查一下Caption是否有重复(但是style不同)的，如果有，需要修改Caption名
                this.CanonicalizeBiblioFromValues();

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            if (this.Visible == false || this.IsDisposed)
                return -1;
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
        }

        // TODO: 代码中 stream 的用法比较含混，建议修改为 using 包围的方式
        // 每个文件第一次请求只返回文件时间, 如果发现有修改后才全部获得文件内容
        // return:
        //      -1  出错
        //      0   本地已经有文件，并且最后修改时间和服务器的一致，因此不必重新获得了
        //      1   获得了文件内容
        int GetSystemFile(
            Stop stop,
            LibraryChannel channel,
            string strFileNameParam,
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
                PathUtil.TryCreateDir(PathUtil.PathPart(strLocalFilePath));

                // 观察本地是否有这个文件，最后修改时间是否和服务器吻合
                if (File.Exists(strLocalFilePath) == true)
                {
                    FileInfo fi = new FileInfo(strLocalFilePath);
                    DateTime local_file_time = fi.LastWriteTimeUtc;

                    if (string.IsNullOrEmpty(strLastTime) == true)
                    {
                        stop?.SetMessage("正在获取系统文件 " + strFileName + " 的最后修改时间 ...");

                        long lRet = channel.GetFile(
        stop,
        "cfgs",
        strFileName,
        -1, // lStart,
        0,  // lLength,
        "gzip",
        out byte[] baContent,
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
                stop?.SetMessage("正在下载系统文件 " + strFileName + " ...");

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
                    long lRet = channel.GetFile(
                        stop,
                        "cfgs",
                        strFileName,
                        lStart,
                        lLength,
                        "gzip",
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
                strError = "InitialExtension GetSystemFile() exception: " + ExceptionUtil.GetAutoText(ex);
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
                strError = "InitialExtension RemoveFiles() exception: " + ExceptionUtil.GetAutoText(ex);
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";
            int nRet = 0;

            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得从服务器映射到本地的配置文件 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得从服务器映射到本地的配置文件 ...",
                "settimeout:0:2:0");
            try
            {
                string strServerMappedPath = PathUtil.MergePath(this.DataDir, "servermapped");
                List<string> fullnames = new List<string>();

                string strValue = "";
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "cfgs",
                    StringUtil.CompareVersion(this.ServerVersion, "2.23") >= 0 ? "listFileNamesEx" : "listFileNames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得映射配置文件名过程发生错误：" + strError;
                    goto ERROR1;
                }
                if (lRet == 0)
                    goto DELETE_FILES;

                string[] filenames = null;

                if (StringUtil.CompareVersion(this.ServerVersion, "2.23") >= 0)
                    filenames = strValue.Replace("||", "?").Split(new char[] { '?' });
                else
                    filenames = strValue.Split(new char[] { ',' });
                foreach (string filename in filenames)
                {
                    if (string.IsNullOrEmpty(filename) == true)
                        continue;

                    nRet = GetSystemFile(
                        looping.Progress,
                        channel,
                        filename,
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
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";
            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得图书馆一般信息 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得图书馆一般信息 ...",
                "settimeout:0:2:0");
            try
            {
                this.LibraryName = "";
                this.ExpireDate = "";
                this.OpacServerUrl = "";

                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "library",
                    "name",
                    out string strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得图书馆一般信息library/name过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.LibraryName = strValue;

                this.SetTitle();

                this.SetServerName(channel.Url, this.LibraryName);

                lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "system",
                    "expire",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " system/expire 过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.ExpireDate = strValue;

                // OPAC URL
                lRet = channel.GetSystemParameter(
                    looping.Progress,
    "opac",
    "serverDirectory",
    out strValue,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " opac/serverDirectory 过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.OpacServerUrl = strValue;
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";
            int nRet = 0;

            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得普通库属性列表 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得普通库属性列表 ...",
                "settimeout:0:2:0");
            try
            {
                this.NormalDbProperties = new List<NormalDbProperty>();

                List<string> dbnames = new List<string>();
                // 创建NormalDbProperties数组
                if (this.BiblioDbProperties != null)
                {
                    foreach (var biblio in this.BiblioDbProperties)
                    {
                        // BiblioDbProperty biblio = this.BiblioDbProperties[i];

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
                        if (StringUtil.CompareVersion(this.ServerVersion, "2.48") <= 0 && prop.Type == "amerce")
                            continue;
                        // 暂时不处理 accessLog / hitcount / chargingOper 类型
                        if (prop.Type == "accessLog"
                            || prop.Type == "hitcount"
                            || prop.Type == "chargingOper"
                            || (prop.Type.Length > 0 && prop.Type[0] == '_'))
                            continue;
                        dbnames.Add(prop.DbName);
                    }
                }

                dbnames.Add("[inventory_item]");

                foreach (string dbname in dbnames)
                {
                    NormalDbProperty normal = null;
                    normal = new NormalDbProperty();
                    normal.DbName = dbname;
                    this.NormalDbProperties.Add(normal);
                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.23") >= 0)
                {
                    // 构造文件名列表
                    List<string> filenames = new List<string>();
                    foreach (NormalDbProperty normal in this.NormalDbProperties)
                    {
                        if (string.IsNullOrEmpty(normal.DbName) == false
                            && normal.DbName[0] == '[')
                            continue;
                        // NormalDbProperty normal = this.NormalDbProperties[i];
                        filenames.Add(normal.DbName + "/cfgs/browse");
                    }

                    // 先获得时间戳
                    // TODO: 如果文件太多可以分批获取
                    string strValue = "";
                    long lRet = channel.GetSystemParameter(
                        looping.Progress,
                        "cfgs/get_res_timestamps",
                        StringUtil.MakePathList(filenames),
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 获得 browse 配置文件时间戳的过程发生错误：" + strError;
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

                        XmlDocument dom = new XmlDocument();
                        if (normal.DbName == "[inventory_item]")
                        {
                            string strFileName = Path.Combine(this.UserDir, "inventory_item_browse.xml");
                            try
                            {
                                dom.Load(strFileName);
                            }
                            catch (Exception ex)
                            {
                                strError = "配置文件 " + strFileName + " 装入 XMLDOM 时出错: " + ex.Message;
                                goto ERROR1;
                            }
                        }
                        else
                        {
                            string strFileName = normal.DbName + "/cfgs/browse";
                            string strTimestamp = (string)table[strFileName];

                            string strContent = "";
                            byte[] baCfgOutputTimestamp = null;
                            nRet = GetCfgFile(
                                channel,
                                looping.Progress,
                                normal.DbName,
                                "browse",
                                ByteArray.GetTimeStampByteArray(strTimestamp),
                                out strContent,
                                out baCfgOutputTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                if (channel.ErrorCode == ErrorCode.AccessDenied)
                                    continue;
                                goto ERROR1;
                            }

                            try
                            {
                                dom.LoadXml(strContent);
                            }
                            catch (Exception ex)
                            {
                                strError = "数据库 " + normal.DbName + " 的 browse 配置文件内容装入XMLDOM时出错: " + ex.Message;
                                // 2020/8/20
                                this.cfgCache.ClearCfgCache();
                                goto ERROR1;
                            }
                        }

                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("//col");
                        foreach (XmlElement node in nodes)
                        {
                            string strColumnType = DomUtil.GetAttr(node, "type");

                            // 2013/10/23
                            string strColumnTitle = dp2ResTree.GetColumnTitle(node,
                                this.Lang);

                            string strXPath = DomUtil.GetElementText(node, "xpath");
                            string strConvert = node.GetAttribute("convert");

                            normal.ColumnProperties.Add(strColumnTitle, strColumnType, strXPath, strConvert);
                        }
                    }
                }
                else
                {
                    // TODO: 是否缓存这些配置文件? 
                    // 获得 browse 配置文件
                    foreach (NormalDbProperty normal in this.NormalDbProperties)
                    {
                        // NormalDbProperty normal = this.NormalDbProperties[i];
                        if (string.IsNullOrEmpty(normal.DbName) == false
    && normal.DbName[0] == '[')
                            continue;

                        normal.ColumnProperties = new ColumnPropertyCollection();

                        nRet = GetCfgFile(
                            channel,
                            looping.Progress,
                            normal.DbName,
                            "browse",
                            null,
                            out string strContent,
                            out byte[] baCfgOutputTimestamp,
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
                        foreach (XmlElement node in nodes)
                        {
                            string strColumnType = DomUtil.GetAttr(node, "type");

                            // 2013/10/23
                            string strColumnTitle = dp2ResTree.GetColumnTitle(node,
                                this.Lang);

                            string strXPath = DomUtil.GetElementText(node, "xpath");
                            string strConvert = node.GetAttribute("convert");

                            normal.ColumnProperties.Add(strColumnTitle, strColumnType, strXPath, strConvert);
                        }
                    }
                }
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得全部数据库定义 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得全部数据库定义 ...",
                "settimeout:0:2:0");
            try
            {
                string strValue = "";
                long lRet = 0;

                this.AllDatabaseDom = null;

                lRet = channel.ManageDatabase(
    looping.Progress,
    "getinfo",
    "",
    "",
    "",
    out strValue,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.AccessDenied)
                    {
                    }

                    strError = "针对服务器 " + channel.Url + " 获得全部数据库定义过程发生错误：" + strError;
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
                looping.Dispose();
                /*
                if (Stop != null)
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");
                }

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
        }

        // return:
        //      0   希望 retry
        //      1   希望返回后继续
        //      -1  希望返回后中断处理
        int AskRetry(string strError)
        {
            DialogResult result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
            {
                return MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            }));
            if (result == System.Windows.Forms.DialogResult.Yes)
                return 0;
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
        /// 获得书目和规范库属性列表
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int InitialBiblioDbProperties(bool bPrepareSearch = true)
        {
            //REDO:
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";
            int nRet = 0;

            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化书目库属性列表 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(/*out LibraryChannel channel,*/
                "正在初始化书目库属性列表 ...",
                "settimeout:0:2:0");
            try
            {
                this.BiblioDbProperties = new List<BiblioDbProperty>();
                this.AuthorityDbProperties = new List<BiblioDbProperty>();

                if (this.AllDatabaseDom == null)
                    return 0;

                {
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
                }

                {
                    XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database[@type='authority']");
                    foreach (XmlNode node in nodes)
                    {
                        string strName = DomUtil.GetAttr(node, "name");
                        string strType = DomUtil.GetAttr(node, "type");

                        BiblioDbProperty property = new BiblioDbProperty();
                        this.AuthorityDbProperties.Add(property);
                        property.DbName = DomUtil.GetAttr(node, "name");
                        property.Syntax = DomUtil.GetAttr(node, "syntax");
                        property.Usage = DomUtil.GetAttr(node, "usage");
                    }
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
                    if (this.ReaderDbProperties == null)
                        return null;

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
            //REDO:
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";
            int nRet = 0;

            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化读者库属性列表 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(/*out LibraryChannel channel,*/
                "正在初始化读者库属性列表 ...",
                "settimeout:0:2:0");
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
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";
            //int nRet = 0;

            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化预约到书库属性列表 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在初始化预约到书库属性列表 ...",
                "settimeout:0:2:0");
            try
            {
                this._arrivedDbName = "";

                if (StringUtil.CompareVersion(this.ServerVersion, "2.47") < 0)
                    return 0;

                int nRedoCount = 0;
            REDO_GET:
                string strValue = "";
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "arrived",
                    "dbname",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    // 2021/8/3
                    if (channel.ErrorCode == ErrorCode.ServerTimeout
                        && nRedoCount < 3)
                    {
                        nRedoCount++;
                        goto REDO_GET;
                    }
                    strError = "针对服务器 " + channel.Url + " 获得预约到书库名过程发生错误：" + strError;
                    goto ERROR1;
                }

                this._arrivedDbName = strValue;
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }
            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
        }

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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();   // 优化

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得前端交费接口配置信息 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得前端交费接口配置信息 ...",
                "settimeout:0:2:0");
            try
            {
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "circulation",
                    "clientFineInterface",
                    out string strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得前端交费接口配置信息过程发生错误：" + strError;
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
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
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
            */
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

#if NO
            if (bPreareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();   // 优化

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得索取号配置信息 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得索取号配置信息 ...",
                "settimeout:0:2:0");
            try
            {
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "circulation",
                    "callNumber",
                    out string strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得索取号配置信息过程发生错误：" + strError;
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
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPreareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        ERROR1:
            return 1;
        }

        /// <summary>
        /// 获得 RFID 配置信息
        /// </summary>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetRfidInfo(bool bPreareSearch = true)
        {
            this.RfidInfo = "";
            this.RfidCfgDom = new XmlDocument();
            this.RfidCfgDom.LoadXml("<rfid />");

            if (StringUtil.CompareVersion(this.ServerVersion, "3.11") < 0)
                return 0;

            string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得 RFID 配置信息 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得 RFID 配置信息 ...",
                "settimeout:0:2:0");
            try
            {
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "system",
                    "rfid",
                    out string strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得 RFID 配置信息过程发生错误：" + strError;
                    return -1;
                }

                this.RfidInfo = strValue;

                try
                {
                    // this.RfidCfgDom.DocumentElement.InnerXml = this.RfidInfo;
                    if (string.IsNullOrEmpty(this.RfidInfo) == false)
                        this.RfidCfgDom.LoadXml(this.RfidInfo);
                }
                catch (Exception ex)
                {
                    strError = "load RfidCfgDom OuterXml error: " + ex.Message;
                    return -1;
                }

                return 0;
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
            }
        }

        public int GetBarcodeValidationInfo()
        {
        REDO:
            string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得条码校验规则 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得条码校验规则 ...",
                "settimeout:0:2:0");
            try
            {
                this.BarcodeValidation = "";

                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "circulation",
                    "barcodeValidation",
                    out string strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得条码校验规则过程发生错误：" + strError;
                    goto ERROR1;
                }

                int nRet = ManagerForm.AddRoot(strValue,
"barcodeValidation",
out string strXml,
out strError);
                if (nRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得条码校验规则过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.BarcodeValidation = strXml;

                // TODO: 是否验证一下 XML 的正确性、合法性
#if NO
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
                }
#endif
                return 0;
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
            }
        ERROR1:
            var ret = AskRetry(strError);
            if (ret == 0)
                goto REDO;
            return ret;
            /*
            DialogResult result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
            {
                return MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            }));
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
            */
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
            //REDO:
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif

            // this.Update();

            //string strError = "";

            /*
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化实用库属性列表 ...");
            Stop.BeginLoop();
            */
            var looping = Looping("正在初始化实用库属性列表 ...",
                "settimeout:0:2:0");
            try
            {
                this.UtilDbProperties = new List<UtilDbProperty>();

                if (this.AllDatabaseDom == null)
                    return 0;

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
                return 0;
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }
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

#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return 0;
            }
#endif
            /*
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得服务器当前时钟 ...");
            Stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得服务器当前时钟 ...",
                "settimeout:0:1:0");
            try
            {
                string strTime = "";
                long lRet = channel.GetClock(
                    looping.Progress,
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

                return 0;
            }
            catch (Exception ex)
            {
                strError = "InitialExtension CheckServerClock() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                looping.Dispose();
                /*
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
                */
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }
        }

        string _focusLibraryCode = "";

        // 当前操作所针对的分馆 代码
        // 注: 全局用户可以操作任何分馆，和总馆，通过此成员，可以明确它当前正在操作哪个分馆，这样可以明确 VerifyBarcode() 的 strLibraryCodeList 参数值
        public string FocusLibraryCode
        {
            get
            {
                return _focusLibraryCode;
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this._focusLibraryCode = value;
                    string strName = string.IsNullOrEmpty(value) == true ? "[总馆]" : value;
                    this.toolStripDropDownButton_selectLibraryCode.Text = "选择分馆 " + strName;
                });
            }
        }

        async Task<NormalResult> FillLibraryCodeListMenuAsync()
        {
            var ret = await GetAllLibraryCodesAsync();
            /*
            int nRet = this.GetAllLibraryCodes(out List<string> all_library_codes,
                out string strError);
            */
            if (ret.Value == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "获得全部馆代码时出错: " + ret.ErrorInfo
                };
            var all_library_codes = ret.LibraryCodes;

            List<string> library_codes = new List<string>();
            if (Global.IsGlobalUser(_currentLibraryCodeList) == true)
            {
                library_codes = all_library_codes;
                library_codes.Insert(0, "");
            }
            else
                library_codes = StringUtil.SplitList(_currentLibraryCodeList);

            this.TryInvoke(() =>
            {
                // 保存以前的选择
                string old_location = GetCurrentLocation();

                this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Clear();
                foreach (string library_code in library_codes)
                {
                    string strName = library_code;
                    if (string.IsNullOrEmpty(strName) == true)
                        strName = "[总馆]";
                    ToolStripItem item = new ToolStripMenuItem(strName);
                    item.Tag = library_code;
                    item.Click += item_Click;
                    this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Add(item);
                }

                // 添加附加的馆藏地
                string value = this.AppInfo.GetString(
        "global",
        "additionalLocations",
        "");
                List<string> list = StringUtil.SplitList(value);
                foreach (string s in list)
                {
                    ToolStripItem item = new ToolStripMenuItem(s);
                    item.Tag = s;
                    item.Click += item_Click;
                    this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Add(item);
                }

                // 恢复以前的选择
                if (string.IsNullOrEmpty(old_location) == false)
                    SetCurrentLocation(old_location);
                else
                {
                    // 默认选定第一项
                    if (this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Count > 0)
                        item_Click(this.toolStripDropDownButton_selectLibraryCode.DropDownItems[0], new EventArgs());
                }
            });
            return new NormalResult();
        }

        void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem current in this.toolStripDropDownButton_selectLibraryCode.DropDownItems)
            {
                if (current != item && current.Checked == true)
                    current.Checked = false;
            }
            item.Checked = true;
            FocusLibraryCode = item.Tag as string;
        }

        string GetCurrentLocation()
        {
            foreach (ToolStripMenuItem current in this.toolStripDropDownButton_selectLibraryCode.DropDownItems)
            {
                if (current.Checked == true)
                    return current.Tag as string;
            }

            return null;
        }

        void SetCurrentLocation(string location)
        {
            foreach (ToolStripMenuItem current in this.toolStripDropDownButton_selectLibraryCode.DropDownItems)
            {
                string current_text = current.Tag as string;
                if (current_text == location)
                {
                    item_Click(current, new EventArgs());
                    return;
                }
            }

            // 选择第一项
            if (this.toolStripDropDownButton_selectLibraryCode.DropDownItems.Count > 0)
                item_Click(this.toolStripDropDownButton_selectLibraryCode.DropDownItems[0], new EventArgs());
        }

        public class GetCodesResult : NormalResult
        {
            public List<string> LibraryCodes { get; set; }
        }

        public async Task<GetCodesResult> GetAllLibraryCodesAsync()
        {
            return await Task.Run(() =>
            {
                var ret = this._getAllLibraryCodes(
    out List<string> library_codes,
    out string strError);
                return new GetCodesResult
                {
                    Value = ret,
                    LibraryCodes = library_codes,
                    ErrorInfo = strError
                };
            });
        }

        // 获得全部可用的图书馆代码。注意，并不包含 "" (全局)
        int _getAllLibraryCodes(out List<string> library_codes,
            out string strError)
        {
            strError = "";
            library_codes = new List<string>();

            /*
            LibraryChannel channel = this.GetChannel();
            if (Stop != null)
            {
                Stop.OnStop += new StopEventHandler(this.DoStop);
                Stop.Initial("正在获得全部馆代码 ...");
                Stop.BeginLoop();
            }
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得全部馆代码 ...",
                "settimeout:0:2:0");
            try
            {
                string strValue = "";
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "system",
                    "libraryCodes",
                    out strValue,
                    out strError);
                if (lRet == -1)
                    return -1;
                library_codes = StringUtil.SplitList(strValue);
                return 0;
            }
            finally
            {
                looping.Dispose();
                /*
                if (Stop != null)
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");
                }

                this.ReturnChannel(channel);
                */
            }
        }

        #region UCS 上载接口的几个配置参数

        public string UcsApiUrl
        {
            get
            {
                return AppInfo.GetString(
    "ucsUpload",
    "apiURL",
    "http://202.96.31.28/X");
            }
        }

        public string UcsDatabaseName
        {
            get
            {
                return AppInfo.GetString(
    "ucsUpload",
    "databaseName",
    "UCS01");
            }
        }

        public string UcsUserName
        {
            get
            {
                return AppInfo.GetString(
    "ucsUpload",
    "userName",
    "");
            }
        }


        public string UcsPassword
        {
            get
            {
                string password = AppInfo.GetString(
"ucsUpload",
"password",
"");
                return Program.MainForm.DecryptPasssword(password);
            }
        }

        public string UcsFilterScriptCode
        {
            get
            {
                return AppInfo.GetString(
    "ucsUpload",
    "filterScriptCode",
    "");
            }
        }

        #endregion

        // 同类书区分号窗 自动取种次号时自动忽略的状态值
        public string CallNumberIgnoreItemState
        {
            get
            {
                return this.AppInfo?.GetString(
"callNumber",
"ignore_item_state",
"");
            }
        }

        public bool CallNumberUseEmptyNumber
        {
            get
            {
                return this.AppInfo?.GetBoolean(
"callNumber",
"use_empty_number",
false) ?? false;
            }
        }
    }

    public delegate void TagChangedEventHandler(object sender,
    TagChangedEventArgs e);

    /// <summary>
    /// 设置标签变化事件的参数
    /// </summary>
    public class TagChangedEventArgs : EventArgs
    {
        public List<TagAndData> AddBooks { get; set; }
        public List<TagAndData> UpdateBooks { get; set; }
        public List<TagAndData> RemoveBooks { get; set; }

        public List<TagAndData> AddPatrons { get; set; }
        public List<TagAndData> UpdatePatrons { get; set; }
        public List<TagAndData> RemovePatrons { get; set; }

        // 2023/11/13
        // RSSI 发生改变的标签
        public List<OneTag> UpdateRssiTags { get; set; }

    }

}
