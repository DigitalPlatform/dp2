using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

using WUApiLib;

namespace DigitalPlatform.CommonControl
{
    // http://stackoverflow.com/questions/8432767/c-sharp-and-wuapi-begindownload-function
    /// <summary>
    /// Windows 更新对话框
    /// </summary>
    public partial class WindowsUpdateDialog : Form
    {
        public bool AutoRun = true;

        // TODO: 如果安装正常结束，则自动关闭对话框。如果最后有报错，最好弹出 MessageBox
        public bool AutoClose = false;

        public WindowsUpdateDialog()
        {
            InitializeComponent();
        }

        private void WindowsUpdateDialog_Load(object sender, EventArgs e)
        {
            ClearForPureTextOutputing(this.webBrowser1);
            Application.DoEvents();

            if (this.AutoRun)
                this.BeginInvoke(new Action(Run));
        }

        #region console

        // 线程安全
        public void ShowProgressMessage(string strID,
            string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<string, string>(ShowProgressMessage), strID, strText);
                return;
            }

            if (webBrowser1.Document == null)
                return;

            HtmlElement obj = this.webBrowser1.Document.GetElementById(strID);
            if (obj != null)
            {
                obj.InnerText = strText;
                return;
            }

            WriteHtml(this.webBrowser1, "</pre><span id='"+strID+"'>" + HttpUtility.HtmlEncode(strText) + "</span>"
                + "<pre style=\"font-family:Consolas; \">");
            ScrollToEnd();
        }

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
    + "<style media='screen' type='text/css'>"
    + "body { font-family:Microsoft YaHei; background-color:#555555; color:#eeeeee; }"
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
            this.webBrowser1.Document.Window.ScrollTo(
                0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        #endregion

        private void button_begin_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void Run()
        {
            string strError = "";

            this.Begin();

            Application.DoEvents();
            this.AppendSectionTitle("开始更新");
            Application.DoEvents();

            this.AppendString("开启 Windows Update 服务 ...\r\n");
            Application.DoEvents();

            int nRet = EnableServices(out strError);
            if (nRet == -1)
            {
                strError = "开启 Windows Update 服务失败:" + strError + "\r\n请使用 Windows 控制面板的“Windows 更新”功能，安装全部更新...";
                goto ERROR1;
            }

            BeginSearchUpdate();
            return;
        ERROR1:
            AppendString(strError);
            MessageBox.Show(this, strError);
            this.button_begin.Enabled = true;
        }

        int EnableServices(out string strError)
        {
            strError = "";
            ServiceController[] controllers = ServiceController.GetServices();

            foreach (ServiceController controller in controllers)
            {
                switch (controller.DisplayName)
                {
                    case "Windows Update":
                        RestartService(controller.DisplayName, 5000);
                        break;
                    case "Automatic Updates":
                        RestartService(controller.DisplayName, 5000);
                        break;
                    default:
                        break;
                }
            }

            // Check for iAutomaticUpdates.ServiceEnabled...
            IAutomaticUpdates iAutomaticUpdates = new AutomaticUpdates();
            if (!iAutomaticUpdates.ServiceEnabled)
            {
                try
                {
                    iAutomaticUpdates.EnableService();
                }
                catch(System.InvalidCastException ex)
                {
                    strError = ex.Message;
                    return -1;
                }
            }

            return 0;
        }

        public static void RestartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController serviceController = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                // 如果不是 Administrator 身份，会抛出异常
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch(Exception ex)
            {

            }
        }

        void Cancel()
        {
            if (_searchJob != null)
                _searchJob.RequestAbort();
            if (_downloadJob != null)
                _downloadJob.RequestAbort();

            if (_installationJob != null)
                _installationJob.RequestAbort();
        }

        UpdateSession _updateSession;
        IUpdateSearcher _updateSearcher;
        ISearchJob _searchJob = null;
        UpdateCollection _updateCollection;
        ISearchResult _updateSearchResult;

        void BeginSearchUpdate()
        {
            _updateSession = new UpdateSession();
            _updateSearcher = _updateSession.CreateUpdateSearcher();

            // Only Check Online..
            _updateSearcher.Online = true;

            this.AppendString("正在搜索更新，请耐心等候 ...\r\n(如果您这台电脑是安装 Windows 操作系统后第一次更新，可能会在这一步耗费较长时间，请一定耐心等待)\r\n");
            // Begin Asynchronous IUpdateSearcher...
            _searchJob = _updateSearcher.BeginSearch("IsInstalled=0 AND IsPresent=0", 
                new SearchCompleteFunc(this), 
                null // new UpdateSearcher_state(this)
                );
        }

        private void SearchUpdateComplete(WindowsUpdateDialog mainform)
        {
            WindowsUpdateDialog formRef = mainform;

            // Declare a new UpdateCollection and populate the result...
            _updateCollection = new UpdateCollection();
            _updateSearchResult = _updateSearcher.EndSearch(_searchJob);

            _searchJob = null;

            //Count = NewUpdatesSearchResult.Updates.Count;
            //formRef.Invoke(formRef.sendNotification);

            // Accept Eula code for each update
            for (int i = 0; i < _updateSearchResult.Updates.Count; i++)
            {
                IUpdate iUpdate = _updateSearchResult.Updates[i];

                if (iUpdate.EulaAccepted == false)
                {
                    iUpdate.AcceptEula();
                }

                _updateCollection.Add(iUpdate);
            }



            if (_updateSearchResult.Updates.Count > 0)
            {
                {
                    this.AppendString("\r\n发现 " + _updateSearchResult.Updates.Count + " 个更新:\r\n");

                    int i = 0;
                    foreach (IUpdate update in _updateSearchResult.Updates)
                    {
                        this.AppendString((i + 1).ToString() + ") " + update.Title + "\r\n");
                        // textBox1.AppendText(update.Title + Environment.NewLine);
                        i++;
                    }
                    this.AppendString("\r\n");
                }
#if NO
                DialogResult result = MessageBox.Show(this,
"要下载这 " + _updateSearchResult.Updates.Count + " 个更新么?",
"WindowsUpdateDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    OnAllComplete();
                    return;
                }
#endif

                BeginDownloadUpdate();
            }
            else
            {
                this.AppendString("当前没有发现任何更新");
                // 全部结束
                OnAllComplete();
            }
        }

        IUpdateDownloader _updateDownloader;
        IDownloadJob _downloadJob = null;
        IDownloadResult _downloadResult;

        void BeginDownloadUpdate()
        {
            this.AppendString("开始下载更新 ...\r\n");

            _updateSession = new UpdateSession();
            _updateDownloader = _updateSession.CreateUpdateDownloader();

            _updateDownloader.Updates = _updateCollection;
            _updateDownloader.Priority = DownloadPriority.dpHigh;
            _downloadJob = _updateDownloader.BeginDownload(new DownloadProgressChangedFunc(this),
                new DownloadCompleteFunc(this),
                null // new UpdateDownloader_state(this)
                );
        }

        void DownloadComplete()
        {
            _downloadResult = _updateDownloader.EndDownload(_downloadJob);

            _downloadJob = null;

            if (_downloadResult.ResultCode == OperationResultCode.orcSucceeded
                || _downloadResult.ResultCode == OperationResultCode.orcSucceededWithErrors)
            {
                this.ShowProgressMessage("progress_download", "");

                if (_downloadResult.ResultCode == OperationResultCode.orcSucceeded)
                    this.AppendString("下载完成。\r\n");
                else
                    this.AppendString("下载部分完成，部分出错。\r\n");

#if NO
                DialogResult result = MessageBox.Show(this,
"要安装这些更新么?",
"WindowsUpdateDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    OnAllComplete();
                    return;
                }
#endif

                BeginInstallation();
            }
            else
            {
                this.AppendString("下载更新失败。错误码: " + _downloadResult.ResultCode);
                // 全部结束
                OnAllComplete();
            }
        }

        //
        IUpdateInstaller _updateInstaller;
        IInstallationJob _installationJob = null;
        IInstallationResult _installationResult;

        void BeginInstallation()
        {
            UpdateCollection installCollection = new UpdateCollection();
            foreach(IUpdate update in this._updateCollection)
            {
                if (update.IsDownloaded)
                    installCollection.Add(update);
            }

            if (installCollection.Count == 0)
            {
                this.AppendString("下载完成，但没有可供安装的更新。操作结束。\r\n");
                OnAllComplete();
                return;
            }

            this.AppendString("开始安装更新 ...\r\n");

            _updateInstaller = _updateSession.CreateUpdateInstaller() as IUpdateInstaller;
            _updateInstaller.Updates = installCollection;   // this._updateCollection;

            // TODO: 不但要安装本次下载的，也要安装以前下载了但没有安装的

            _installationJob = _updateInstaller.BeginInstall(new InstallationProgressChangedFunc(this),
                new InstallCompletedFunc(this),
                null // new UpdateInstaller_state(this)
                );
        }

        internal void InstallationComplete()
        {
            /*
    public enum OperationResultCode
    {
        orcNotStarted = 0,
        orcInProgress = 1,
        orcSucceeded = 2,
        orcSucceededWithErrors = 3,
        orcFailed = 4,
        orcAborted = 5,
    }
             * */
            _installationResult = _updateInstaller.EndInstall(_installationJob);

            _installationJob = null;

            if (_installationResult.ResultCode == OperationResultCode.orcSucceeded)
            {
                this.ShowProgressMessage("progress_install", "");

                this.AppendString("安装完成。\r\n");
            }
            else
            {
                this.AppendString("安装更新失败。错误码: " + _installationResult.ResultCode);
                if (_installationResult.RebootRequired == true)
                {
                    OnAllComplete();
                    MessageBox.Show(this, "请重新启动电脑");
                    return;
                }

                // 全部结束
            }

            OnAllComplete();
        }

        bool _running = false;

        // 全部结束后设置好按钮状态
        // 线程安全
        void OnAllComplete()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(OnAllComplete));
                return;
            }
            this.button_begin.Enabled = true;
            this.button_close.Text = "关闭";

            this.AppendCrLn();
            this.AppendSectionTitle("结束更新");

            this._running = false;
        }

        void Begin()
        {
            this.button_begin.Enabled = false;
            this.button_close.Text = "中断";

            this._running = true;
        }

        #region searcher

        // onCompleted [in] 
        // An ISearchCompletedCallback interface that is called when an asynchronous search operation is complete.
        class SearchCompleteFunc : ISearchCompletedCallback
        {
            private WindowsUpdateDialog form1;

            public SearchCompleteFunc(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;
            }

            // Implementation of IDownloadCompletedCallback interface...
            public void Invoke(ISearchJob searchJob, ISearchCompletedCallbackArgs e)
            {
                form1.SearchUpdateComplete(this.form1);
            }
        }

#if NO
        // state [in] 
        // The caller-specific state that is returned by the AsyncState property of the ISearchJob interface.
        public class UpdateSearcher_state
        {
            private WindowsUpdateDialog form1;

            // Implementation of state interface...
            public UpdateSearcher_state(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;

                // form1.setTextBox2Notification("State: Search Started...");
            }
        }
#endif

        #endregion

        #region download

        // onProgressChanged [in] 
        // An IDownloadProgressChangedCallback interface that is called periodically for download progress changes before download is complete.
        class DownloadProgressChangedFunc : IDownloadProgressChangedCallback
        {
            private WindowsUpdateDialog form1;

            public DownloadProgressChangedFunc(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;
            }

            // Implementation of IDownloadProgressChangedCallback interface...
            public void Invoke(IDownloadJob downloadJob, 
                IDownloadProgressChangedCallbackArgs e)
            {
                decimal downloaded = ((e.Progress.TotalBytesDownloaded / 1024) / 1024);
                decimal toDownloaded = ((e.Progress.TotalBytesToDownload / 1024) / 1024);
                downloaded = decimal.Round(downloaded, 2);
                toDownloaded = decimal.Round(toDownloaded, 2);

                form1.ShowProgressMessage("progress_download",
                    "下载进度: "
                 + e.Progress.CurrentUpdateIndex
                 + "/"
                 + downloadJob.Updates.Count
                 + " - "
                 + downloaded + "Mb"
                 + " / "
                 + toDownloaded + "Mb");
            }
        }

        // onCompleted [in] 
        // An IDownloadCompletedCallback interface (C++/COM) that is called when an asynchronous download operation is complete.
        class DownloadCompleteFunc : IDownloadCompletedCallback
        {
            private WindowsUpdateDialog form1;

            public DownloadCompleteFunc(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;
            }

            // Implementation of IDownloadCompletedCallback interface...
            public void Invoke(IDownloadJob downloadJob, IDownloadCompletedCallbackArgs e)
            {
                form1.DownloadComplete();
            }
        }

#if NO
        // state [in] 
        // The caller-specific state that the AsyncState property of the IDownloadJob interface returns. 
        // A caller may use this parameter to attach a value to the download job object. 
        // This allows the caller to retrieve custom information about that download job object at a later time.
        class UpdateDownloader_state
        {
            private WindowsUpdateDialog form1;

            // Implementation of state interface...
            public UpdateDownloader_state(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;

                // form1.setTextBox2Notification("State: Download Started...");
            }
        }
#endif

        #endregion

        #region installation

        // onProgressChanged [in] 
        // An IDownloadProgressChangedCallback interface that is called periodically for download progress changes before download is complete.
        class InstallationProgressChangedFunc : IInstallationProgressChangedCallback
        {
            private WindowsUpdateDialog form1;

            public InstallationProgressChangedFunc(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;
            }

            // Implementation of IDownloadProgressChangedCallback interface...
            public void Invoke(IInstallationJob iInstallationJob, IInstallationProgressChangedCallbackArgs e)
            {
                form1.ShowProgressMessage(
                    "progress_install",
                    "安装进度: "
                 + e.Progress.CurrentUpdateIndex
                 + " / "
                 + iInstallationJob.Updates.Count
                 + " - 已完成 "
                 + e.Progress.CurrentUpdatePercentComplete + "%");
            }
        }

        // onCompleted [in] 
        class InstallCompletedFunc : IInstallationCompletedCallback
        {
            private WindowsUpdateDialog form1;

            public InstallCompletedFunc(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;
            }

            // Implementation of IDownloadCompletedCallback interface...
            public void Invoke(IInstallationJob iInstallationJob, IInstallationCompletedCallbackArgs e)
            {
                form1.InstallationComplete();
            }
        }

#if NO
        // state [in] 
        // The caller-specific state that the AsyncState property of the IDownloadJob interface returns. 
        // A caller may use this parameter to attach a value to the download job object. 
        // This allows the caller to retrieve custom information about that download job object at a later time.
        public class UpdateInstaller_state
        {
            private WindowsUpdateDialog form1;

            // Implementation of state interface...
            public UpdateInstaller_state(WindowsUpdateDialog mainForm)
            {
                this.form1 = mainForm;

                // form1.setTextBox2Notification("State: Installation Started...");
            }
        }
#endif

        #endregion

        private void button_testprogress_Click(object sender, EventArgs e)
        {
            this.AppendSectionTitle("begin test progress");
            Application.DoEvents();
            Thread.Sleep(1000);
            this.AppendString("some text\r\n");
            Application.DoEvents();
            Thread.Sleep(1000);

            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);

                this.ShowProgressMessage("test_progress",
                    "count " + i);
                Application.DoEvents();
            }

            this.AppendString("complete\r\n");
            Application.DoEvents();
            Thread.Sleep(1000);

        }

        private void WindowsUpdateDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._running == true)
            {
                DialogResult result = MessageBox.Show(this,
"Windows 更新尚未完成，确实要关闭它?",
"WindowsUpdateDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            this.Cancel();
        }
    }
}

/*

                {
                    UpdateSession uSession = new UpdateSession();
                    IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();
                    uSearcher.Online = false;
                    try
                    {
                        this.AppendString("列出已经下载但没有安装的更新\r\n");
                        ISearchResult sResult = uSearcher.Search("IsInstalled=0 And IsHidden=0");
                        this.AppendString("Found " + sResult.Updates.Count + " updates" + Environment.NewLine);
                        foreach (IUpdate update in sResult.Updates)
                        {
                            this.AppendString(update.Title + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Something went wrong: " + ex.Message);
                    }

                    return;
                }

*/