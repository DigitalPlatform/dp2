// #define TEST

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
using System.ServiceProcess;
using System.ComponentModel;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using Ionic.Zip;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
// using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关初始化各种信息的功能
    /// </summary>
    public partial class MainForm
    {
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
                    this.WriteErrorLog(strError);
                }
            }
            catch (Exception ex)
            {
                strError = "CrashReport() (" + strTitle + ") 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.WriteErrorLog(strError);
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
                this.DisplayBackgroundText("开始自动更新(ClickOnce安装)\r\n");
                ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
                deployment.CheckForUpdateCompleted -= new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                deployment.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                deployment.CheckForUpdateProgressChanged -= new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);
                deployment.CheckForUpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);

                _updateState = UpdateState.CheckForUpdate;
                deployment.CheckForUpdateAsync();
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
            if (e.Error != null)
            {
                this.DisplayBackgroundText("ERROR: Could not retrieve new version of the application. Reason: \n" + e.Error.Message + "\nPlease report this error to the system administrator.");
                return;
            }
            else if (e.Cancelled == true)
            {
                this.DisplayBackgroundText("The update was cancelled.");
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
                this.DisplayBackgroundText("当前没有更新的版本\r\n");
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
            deployment.UpdateAsync();
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
            _updateState = UpdateState.Finish;

            if (e.Cancelled)
            {
                this.DisplayBackgroundText("The update of the application's latest version was cancelled.\r\n");
                return;
            }
            else if (e.Error != null)
            {
                this.DisplayBackgroundText("ERROR: Could not install the latest version of the application. Reason: \r\n" + e.Error.Message + "\r\nPlease report this error to the system administrator.\r\n");
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
            this.DisplayBackgroundText("dp2circulation 已经成功更新。重启可立即使用新版本。\r\n");
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

        static string _defaultDownloadBaseUrl = "http://dp2003.com/dp2circulation/v2/";

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

            this.DisplayBackgroundText("开始自动更新(绿色安装)\r\n这期间，您可继续进行其它操作\r\n");

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
                        this.DisplayBackgroundText("下载 " + strUrl + " 到 " + strLocalFileName + " ...\r\n");

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

                    this.DisplayBackgroundText("展开文件 " + strZipFileName + " 到 " + strTargetDir + " ...\r\n");
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
                    this.DisplayBackgroundText("dp2circulation 绿色安装包升级文件已经准备就绪。当退出 dp2circulation 时会自动进行安装。\r\n");
                else
                    this.DisplayBackgroundText("没有发现更新。\r\n");

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
            this.DisplayBackgroundText("绿色更新过程出错: " + strError + "\r\n");
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

                    this.DisplayBackgroundText("展开文件 " + strZipFileName + " 到 " + strTargetDir + " ...\r\n");
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

        // 程序启动时候需要执行的初始化操作
        // 这些操作只需要执行一次。也就是说，和登录和连接的服务器无关。如果有关，则要放在 InitialProperties() 中
        // FormLoad() 中的许多操作应当移动到这里来，以便尽早显示出框架窗口
        void FirstInitial()
        {
            string strError = "";
            int nRet = 0;

#if NO
            if (DetectIE() == false)
            {
                MessageBox.Show(this, "IE 浏览器故障，无法启动 dp2circulation。请联系数字平台");
                Application.Exit();
                return;
            }
#endif

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

            if (StringUtil.IsDevelopMode() == false)
            {
                this.MenuItem_separator_function2.Visible = false;
                this.MenuItem_chatForm.Visible = false;
                this.MenuItem_messageForm.Visible = false;
                this.MenuItem_openReservationListForm.Visible = false;
                // this.MenuItem_inventory.Visible = false;
                this.MenuItem_openMarc856SearchForm.Visible = false;
                this.MenuItem_createGreenApplication.Visible = false;
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

            this.MenuItem_upgradeFromDisk.Visible = !ApplicationDeployment.IsNetworkDeployed;

            OpenBackgroundForm();

            string strDriveName = "";
            if (GetUserDiskFreeSpace(out strDriveName) < 1024 * 1024 * 10)
            {
                Program.PromptAndExit(this, "用户目录所在硬盘 " + strDriveName + " 剩余空间太小(已小于10M)。请先腾出足够空间，再重新启动 dp2circulation");
                return;
            }

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

                // 启动时在日志中记载当前 dp2circulation 版本号
                this.WriteErrorLog(Assembly.GetAssembly(this.GetType()).FullName);

#if NO
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

                    this.WriteErrorLog("dp2circulation 启动时，发现本机尚未安装 .NET Framework 4 更新 KB2468871");

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

#endif

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
            // stopManager.OnDisplayMessage += new DisplayMessageEventHandler(stopManager_OnDisplayMessage);

            // 公用的 Stop 对象
            this.Stop = new DigitalPlatform.Stop();
            this.Stop.Register(stopManager, true);

            this.SetMenuItemState();

            LinkStopToBackgroundForm(true);

            // cfgcache
            Debug.Assert(string.IsNullOrEmpty(this.UserDir) == false, "");
            // 2015/10/3 改在 UserDir 下
            nRet = cfgCache.Load(Path.Combine(this.UserDir, "cfgcache.xml"),    // this.DataDir
                out strError);
            if (nRet == -1)
            {
                if (IsFirstRun == false)
                    MessageBox.Show(strError);
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
            Task.Factory.StartNew(() => CopyGreen());

            StartPrepareNames(true, true);

            if (this.MdiClient != null)
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

            this.m_strPinyinGcatID = this.AppInfo.GetString("entity_form", "gcat_pinyin_api_id", "");
            this.m_bSavePinyinGcatID = this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", false);

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

            this.PropertyTaskList.MainForm = this;
            this.PropertyTaskList.BeginThread();
        }

        bool _copyGreenError = false;   // 第一次 CopyGreen() 是否出错

        // 复制出一个绿色安装包
        // return:
        //      false   没有启动
        //      true    启动了。注意，有可能会启动了但后来出错了。
        bool CopyGreen(bool bForce = false)
        {
            int nRet = 0;
            string strError = "";

            // 本身如果是绿色安装包，没有必要再次复制出绿色安装包
            if (bForce == false && ApplicationDeployment.IsNetworkDeployed == false)
                return false;

            string strProgramDir = Environment.CurrentDirectory;
            string strTargetDir = "c:\\dp2circulation";

            if (PathUtil.IsEqual(strProgramDir, strTargetDir) == true)
                return false;

            this.DisplayBackgroundText("正在创建备用绿色安装包 ...\r\n");

            StringBuilder debugInfo = new StringBuilder();
            nRet = GreenProgram.CopyGreen(
                Program.ClientVersion,
                strProgramDir,
                this.DataDir,
                strTargetDir,
                debugInfo,
                out strError);
            if (nRet == -1)
            {
                ShowMessageBox("创建备用绿色安装包时出错: " + strError);
                this.DisplayBackgroundText(strError + "\r\n");
                // 发送给 dp2003.com
                ReportError("dp2circulation 创建备用绿色安装包时出错", strError + "\r\n\r\nDebug Info:\r\n" + debugInfo.ToString());
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
            else
            {
                GreenProgram.CreateShortcutToDesktop(
                   "内务绿色",
                   Path.Combine(strTargetDir, "dp2circulation.exe"),
                   false);
                this.DisplayBackgroundText("备用绿色安装包已经成功创建于 " + strTargetDir + "。\r\n");
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

        void OpenBackgroundForm()
        {
            if (this.m_backgroundForm != null)
                return;

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

            ClearBackground();
        }

        void ClearBackground()
        {
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

                    Global.WriteHtml(m_backgroundForm.WebBrowser,
                        "<html><head>" + strLink + "</head><body" + strBodyClass + ">");
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
            m_backgroundForm.Size = new System.Drawing.Size(this.MdiClient.ClientSize.Width, this.MdiClient.ClientSize.Height);
        }

        void CloseBackgroundForm()
        {
            if (this.m_backgroundForm != null)
            {
                // TODO: 最好有个淡出的功能
                this.MdiClient.SizeChanged -= new EventHandler(MdiClient_SizeChanged);
                this.m_backgroundForm.Close();
                this.m_backgroundForm = null;
            }
        }

        void ShowBackgroundProgress(string strID, string strText)
        {
            if (this.m_backgroundForm != null)
                this.m_backgroundForm.ShowProgressMessage(strID, strText);
        }

        void DisplayBackgroundText(string strText)
        {
            if (m_backgroundForm != null)
            {
                if (m_backgroundForm.InvokeRequired)
                {
                    m_backgroundForm.Invoke(new Action<string>(DisplayBackgroundText), strText);
                    return;
                }
                m_backgroundForm.AppendHtml(HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>"));
            }
        }

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

        #endregion
        /*
信息创建时间:*2015-9-15 15:48:56
当前操作系统信息:*Microsoft Windows NT 5.1.2600 Service Pack 3
当前操作系统版本号:*5.1.2600.196608
本机 MAC 地址:*00192145CC58
是否安装了 KB2468871:*True
系统进程:
--- Devices:
1) 360Safe Anti Hacker Service
2) 360Box mini-filter driver
3) 360Safe Camera Filter Service
4) 360netmon
5) 360qpesv driver
6) 360reskit driver
7) 360SelfProtection
8) Abiosdsk
9) Microsoft ACPI Driver
10) Microsoft ACPIEC Driver
11) Microsoft Kernel Acoustic Echo Canceller
12) AFD
13) Ambfilt
14) asc3350p
15) RAS Asynchronous Media Driver
16) 标准 IDE/ESDI 硬盘控制器
17) Atdisk
18) ATM ARP Client Protocol
19) 音频存根驱动程序
20) BAPIDRV
21) Beep
22) cbidf2k
23) cd20xrnt
24) Cdaudio
25) Cdfs
26) CD-ROM Driver
27) Changer
28) 磁盘驱动器
29) dmboot
30) Logical Disk Manager Driver
31) dmload
32) Microsoft Kernel DLS Syntheiszer
33) Microsoft Kernel DRM Audio Descrambler
34) DsArk
35) EfiSystemMon
36) exFat
37) Fastfat
38) Fdc
39) Fips
40) Flpydisk
41) FltMgr
42) FsVga
43) Volume Manager Driver
44) Generic Packet Classifier
45) Microsoft 用于 High Definition Audio 的 UAA 总线驱动程序
46) Microsoft HID Class Driver
47) HookPort
48) HTTP
49) HUAWEISERSP
50) i2omgmt
51) i8042 键盘和 PS/2 鼠标端口驱动程序
52) CD 烧制筛选驱动器
53) Service for Realtek HD Audio (WDM)
54) Intel Processor Driver
55) IPv6 Windows Firewall Driver
56) IP Traffic Filter Driver
57) IP in IP Tunnel Driver
58) IP Network Address Translator
59) IPSEC driver
60) IR Enumerator Service
61) PnP ISA/EISA Bus Driver
62) Keyboard Class Driver
63) Keyboard HID Driver
64) Microsoft Kernel Wave Audio Mixer
65) KSecDD
66) lbrtfdc
67) mnmdd
68) Modem
69) Monfilt
70) Mouse Class Driver
71) Mouse HID Driver
72) MountMgr
73) WebDav Client Redirector
74) MRxSmb
75) Msfs
76) Microsoft Streaming Service Proxy
77) Microsoft Streaming Clock Proxy
78) Microsoft Streaming Quality Manager Proxy
79) Microsoft System Management BIOS Driver
80) Mup
81) NDIS System Driver
82) Remote Access NDIS TAPI Driver
83) NDIS 用户模式 I/O 协议
84) Remote Access NDIS WAN Driver
85) NDIS Proxy
86) NetBIOS Interface
87) NetBios over Tcpip
88) Npfs
89) Ntfs
90) Null
91) IPX Traffic Filter Driver
92) IPX Traffic Forwarder Driver
93) Parallel port driver
94) PartMgr
95) ParVdm
96) PCI Bus Driver
97) PCIDump
98) PCIIde
99) Pcmcia
100) PDCOMP
101) PDFRAME
102) PDRELI
103) PDRFRAME
104) perc2hib
105) WAN Miniport (PPTP)
106) 处理器驱动程序
107) QoS Packet Scheduler
108) Direct Parallel Link Driver
109) QQFrmMgr
110) QQProtect
111) Quantum DeepScanner Servers
112) qutmipc
113) Remote Access Auto Connection Driver
114) WAN Miniport (L2TP)
115) 远程访问 PPPOE 驱动程序
116) Direct Parallel
117) Rdbss
118) RDPCDD
119) Terminal Server Device Redirector Driver
120) RDPWD
121) Digital CD Audio Playback Filter Driver
122) Realtek 10/100/1000 PCI NIC Family NDIS XP Driver
123) Realtek RTL8139(A/B/C)-based PCI Fast Ethernet Adapter NT Driver
124) Realtek 10/100/1000 PCI-E NIC Family NDIS XP Driver
125) Secdrv
126) Serenum Filter Driver
127) Serial port driver
128) Sfloppy
129) Simbad
130) SiS315
131) SiS AGP winXP Filter
132) SiSide
133) SiSkp
134) Microsoft Kernel Audio Splitter
135) System Restore Filter Driver
136) Srv
137) Software Bus Driver
138) Microsoft Kernel GS Wavetable Synthesizer
139) Microsoft Kernel System Audio Device
140) TCP/IP Protocol Driver
141) TDPIPE
142) TDTCP
143) Terminal Device Driver
144) Udfs
145) Microcode Update Driver
146) Microsoft USB Generic Parent Driver
147) Microsoft USB 2.0 Enhanced Host Controller Miniport Driver
148) USB2 Enabled Hub
149) Microsoft USB Open Host Controller Miniport Driver
150) Microsoft USB PRINTER Class
151) USB 大容量存储设备
152) Microsoft USB Universal Host Controller Miniport Driver
153) VgaSave
154) VolSnap
155) Remote Access IP ARP Driver
156) Kernel Mode Driver Frameworks service
157) WDICA
158) Microsoft WINMM WDM Audio Compatibility Driver
159) Android USB Driver
160) WpdUsb
161) Windows Driver Foundation - User-mode Driver Framework Platform Driver
162) WUDFRd
--- System process:
1) winlogon.exe
2) zstatus.exe
3) Explorer.EXE
4) dfsvc.exe
5) httpd.exe
6) smss.exe
7) httpd.exe
8) svchost.exe
9) DhMachineSvc.exe
10) DhPluginMgr.exe
11) csrss.exe
12) lsass.exe
13) alg.exe
14) services.exe
15) svchost.exe
16) zhudongfangyu.exe
17) svchost.exe
18) 360Tray.exe
19) svchost.exe
20) CAJSHost.exe
21) ctfmon.exe
22) svchost.exe
23) spoolsv.exe
24) svchost.exe
25) conime.exe
26) SoftMgrLite.exe
27) dp2Circulation.exe

         * */
        bool Detect360()
        {
            ServiceController[] devices = ServiceController.GetDevices();

            // 先检测驱动
            foreach (ServiceController controller in devices)
            {
                if (controller.DisplayName.StartsWith("360netmon", StringComparison.OrdinalIgnoreCase)
                    || controller.DisplayName.StartsWith("360SelfProtection", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // 再检测系统进程
            System.Diagnostics.Process[] process_list = System.Diagnostics.Process.GetProcesses();

            foreach (Process process in process_list)
            {
                string ModuleName = "";
                try
                {
                    ModuleName = process.MainModule.ModuleName;
                }
                catch (Exception)
                {
                    continue;
                }
                if (ModuleName.StartsWith("360Tray.exe", StringComparison.OrdinalIgnoreCase)
                    || ModuleName.StartsWith("zhudongfangyu.exe", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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
            filenames.Add("nonephoto.png");
#if NO
            filenames.Add("comment_change_actions.xml");
            filenames.Add("issue_change_actions.xml");
            filenames.Add("item_change_actions.xml");
            filenames.Add("order_change_actions.xml");
            filenames.Add("patron_change_actions.xml");
            filenames.Add("856_change_actions.xml");
#endif

            FileInfo[] fis = di.GetFiles("*.*");
            foreach (FileInfo fi in fis)
            {
                if (filenames.IndexOf(fi.Name.ToLower()) != -1)
                    filenames.Remove(fi.Name.ToLower());

                string strTargetFileName = Path.Combine(this.UserDir, fi.Name);
                if (File.Exists(strTargetFileName) == false)    // 偶尔会出现判断错误
                {
                    string strSourceFileName = fi.FullName;
#if DEBUG
                    if (File.Exists(strSourceFileName) == false)
                    {
                        strError = "配置文件 '" + strSourceFileName + "' 不存在，无法复制到用户目录。\r\n\r\n建议尽量直接从 dp2003.com 以 ClickOnce 方式安装 dp2circulation，以避免绿色安装时复制配置文件不全带来的麻烦";
                        goto ERROR1;
                    }
#endif
                    try
                    {
                        File.Copy(strSourceFileName, strTargetFileName, false);
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
                            MessageBox.Show(this, "dp2Circulation 不支持 Windows XP / Windows Server 2003 操作系统版本。请在 Windows Vista 及以上版本安装运行");
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

                        FirstRunDialog first_dialog = new FirstRunDialog();
                        MainForm.SetControlFont(first_dialog, this.DefaultFont);
                        first_dialog.MainForm = this;
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
                    }
                    else
                    {
                        // 以前已经安装的情况
                        if (Environment.OSVersion.Version.Major == 5)
                        {
                            MessageBox.Show(this, "尊敬的用户，dp2Circulation 在 2015 年 12 月 31 日以后将不再支持 Windows XP / Windows Server 2003 操作系统版本。请尽快升级您的 Windows 操作系统到 Vista 及以上版本。祝工作顺利。\r\n\r\n数字平台敬上");
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

#if NO
                nRet = PrepareSearch();
                if (nRet == 1)
                {
#endif
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
                        //      -2  出现严重错误，希望退出 Application
                        //      -1  一般错误
                        //      0   dp2Library的版本号过低。警告信息在strError中
                        //      1   dp2Library版本号符合要求
                        nRet = CheckVersion(false, out strError);
                        if (nRet == -2)
                        {
                            if (string.IsNullOrEmpty(strError) == false)
                                MessageBox.Show(this, strError);
                            // Application.Exit();
                            Program.PromptAndExit(null,
                                string.IsNullOrEmpty(strError) == false ? strError : "CheckVersion Fail...");
                            return false;
                        }
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
                    // EndSearch();
                }
#if NO
                }
#endif

                // 安装条码字体
                InstallBarcodeFont();
            END1:
#if NO
                Stop = new DigitalPlatform.Stop();
                Stop.Register(stopManager, true);	// 和容器关联
#endif
                // TODO: 这里有一定问题。最好临时申请一个 stop， 然后用后释放
                Stop.SetMessage("正在删除以前遗留的临时文件...");

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

                Stop.SetMessage("正在复制报表配置文件...");
                // 拷贝目录
                nRet = PathUtil.CopyDirectory(Path.Combine(this.DataDir, "report_def"),
                    Path.Combine(this.UserDir, "report_def"),
                    false,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                Stop.SetMessage("");
#if NO
                if (Stop != null) // 脱离关联
                {
                    Stop.Unregister();	// 和容器关联
                    Stop = null;
                }
#endif

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
                    {
                        this.ReportError("dp2circulation 创建 OperHistory 时出错", strError);
                        MessageBox.Show(this, "初始化 OperHistory 时出错: " + strError);
                    }
                    // this.timer_operHistory.Start();
                }

                if (Global.IsKbInstalled("KB2468871") == true)
                {
                    // 初始化 MessageHub
                    this.MessageHub = new MessageHub();
                    this.MessageHub.Initial(this, this.webBrowser_messageHub);
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

                ClearBackground();

                // 若第一次复制绿色版本失败，则需要再进行一次
                if (_copyGreenError == true)
                    Task.Factory.StartNew(() => CopyGreen());

                BeginUpdateClickOnceApplication();    // 自动探测更新 dp2circulation

                BeginUpdateGreenApplication(); // 自动进行绿色更新
            }
            finally
            {
                // 然后许可界面
                if (bFullInitial == true)
                {
                    this.MdiClient.Enabled = true;
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
                // CloseBackgroundForm();
                LinkStopToBackgroundForm(false);
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在连接服务器 " + channel.Url + " ...");
            Stop.BeginLoop();

            try
            {
                string strTime = "";
                channel.Timeout = new TimeSpan(0, 1, 0);
                long lRet = channel.GetClock(Stop,
                    out strTime,
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
        public string ServerVersion { get; set; }    // = 0

        /// <summary>
        /// 当前连接的 dp2library 的 uid
        /// </summary>
        public string ServerUID { get; set; }

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
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检查服务器 " + channel.Url + " 的版本号, 请稍候 ...");
            Stop.BeginLoop();

            try
            {
                string strVersion = "";
                string strUID = "";

#if NO
                strError = "测试退出";
                return -2;
#endif

                long lRet = channel.GetVersion(Stop,
    out strVersion,
    out strUID,
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

                string base_version = "2.60"; // 2.33
                if (StringUtil.CompareVersion(strVersion, base_version) < 0)   // 2.12
                {
                    // strError = "当前 dp2Circulation 版本必须和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + strVersion + " )。\r\n\r\n请立即升级 dp2Library 到最新版本。";
                    strError = "dp2 前端所连接的 dp2library 版本必须升级为 " + base_version + " 以上时才能使用 (当前 dp2library 版本为 " + strVersion + ")\r\n\r\n请立即升级 dp2Library 到最新版本。\r\n\r\n注：升级服务器的操作非常容易：\r\n1) 若是 dp2 标准版，请系统管理员在服务器机器上，运行 dp2installer(dp2服务器安装工具) 即可。这个模块的安装页面是 http://dp2003.com/dp2installer/v1/publish.htm 。\r\n2) 若是单机版或小型版，反复重启 dp2libraryxe 模块多次即可自动升级。\r\n\r\n亲，若有任何问题，请及时联系数字平台哟 ~";
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

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

                long lRet = channel.ListDbFroms(Stop,
                    "biblio",
                    this.Lang,
                    out infos,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出书目库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.BiblioDbFromInfos = infos;

                // 获得读者库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
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
                    this.ReaderDbFromInfos = null;
                }
                else
                {
                    this.ReaderDbFromInfos = infos;
                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.11") >= 0)
                {
                    // 获得实体库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "item",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出实体库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.ItemDbFromInfos = infos;

                    // 获得期库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "issue",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出期库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.IssueDbFromInfos = infos;

                    // 获得订购库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "order",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出订购库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.OrderDbFromInfos = infos;

                    // 获得评注库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "comment",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出评注库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }
                    this.CommentDbFromInfos = infos;
                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.17") >= 0)
                {
                    // 获得发票库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "invoice",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出发票库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.InvoiceDbFromInfos = infos;

                    // 获得违约金库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "amerce",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出违约金库检索途径过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    this.AmerceDbFromInfos = infos;

                }

                if (StringUtil.CompareVersion(this.ServerVersion, "2.47") >= 0)
                {
                    // 获得预约到书库的检索途径
                    infos = null;
                    lRet = channel.ListDbFroms(Stop,
        "arrived",
        this.Lang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 列出预约到书库检索途径过程发生错误：" + strError;
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

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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

        // TODO: 代码中 stream 的用法比较含混，建议修改为 using 包围的方式
        // 每个文件第一次请求只返回文件时间, 如果发现有修改后才全部获得文件内容
        // return:
        //      -1  出错
        //      0   本地已经有文件，并且最后修改时间和服务器的一致，因此不必重新获得了
        //      1   获得了文件内容
        int GetSystemFile(
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
                        long lRet = channel.GetFile(
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
                    long lRet = channel.GetFile(
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
            LibraryChannel channel = this.GetChannel();

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
                long lRet = channel.GetSystemParameter(Stop,
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得图书馆一般信息 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = channel.GetSystemParameter(Stop,
                    "library",
                    "name",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得图书馆一般信息library/name过程发生错误：" + strError;
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

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

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
                        if (StringUtil.CompareVersion(this.ServerVersion, "2.48") <= 0 && prop.Type == "amerce")
                            continue;
                        // 暂时不处理 accessLog 和 hitcount 类型
                        if (prop.Type == "accessLog" || prop.Type == "hitcount")
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
                    long lRet = channel.GetSystemParameter(Stop,
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
                                Stop,
                                normal.DbName,
                                "browse",
                                ByteArray.GetTimeStampByteArray(strTimestamp),
                                out strContent,
                                out baCfgOutputTimestamp,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            try
                            {
                                dom.LoadXml(strContent);
                            }
                            catch (Exception ex)
                            {
                                strError = "数据库 " + normal.DbName + " 的 browse 配置文件内容装入XMLDOM时出错: " + ex.Message;
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

                        string strContent = "";
                        byte[] baCfgOutputTimestamp = null;
                        nRet = GetCfgFile(
                            channel,
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得全部数据库定义 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = 0;

                this.AllDatabaseDom = null;

                lRet = channel.ManageDatabase(
    Stop,
    "getinfo",
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
                if (Stop != null)
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");
                }

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
            //REDO:
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

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

                this.ReturnChannel(channel);
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
            LibraryChannel channel = this.GetChannel();

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

                this.ReturnChannel(channel);
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
            LibraryChannel channel = this.GetChannel();

            string strError = "";
            //int nRet = 0;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在初始化预约到书库属性列表 ...");
            Stop.BeginLoop();

            try
            {
                this._arrivedDbName = "";

                if (StringUtil.CompareVersion(this.ServerVersion, "2.47") < 0)
                    return 0;

                string strValue = "";
                long lRet = channel.GetSystemParameter(Stop,
                    "arrived",
                    "dbname",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得预约到书库名过程发生错误：" + strError;
                    goto ERROR1;
                }

                this._arrivedDbName = strValue;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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
#if NO
            if (bPrepareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

            // this.Update();   // 优化

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得前端交费接口配置信息 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = channel.GetSystemParameter(Stop,
                    "circulation",
                    "clientFineInterface",
                    out strValue,
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
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

#if NO
            if (bPreareSearch == true)
            {
                if (PrepareSearch() == 0)
                    return -1;
            }
#endif
            LibraryChannel channel = this.GetChannel();

            // this.Update();   // 优化

            string strError = "";

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得索取号配置信息 ...");
            Stop.BeginLoop();

            try
            {
                string strValue = "";
                long lRet = channel.GetSystemParameter(Stop,
                    "circulation",
                    "callNumber",
                    out strValue,
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPreareSearch == true)
                    EndSearch();
#endif
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
            LibraryChannel channel = this.GetChannel();

            // this.Update();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得服务器当前时钟 ...");
            Stop.BeginLoop();

            try
            {
                string strTime = "";
                long lRet = channel.GetClock(
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
                strError = "InitialExtension CheckServerClock() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
#if NO
                if (bPrepareSearch == true)
                    EndSearch();
#endif
            }

            return 0;
        }

    }
}
