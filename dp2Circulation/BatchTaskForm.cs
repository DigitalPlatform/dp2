using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /*
     * 2011/2/23
下列批处理任务被废止了：
跟踪DTLP数据库
创建缓存
     * */
    /// <summary>
    /// 批处理任务窗
    /// </summary>
    public partial class BatchTaskForm : MyForm
    {
        int m_nInRefresh = 0;

        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;
#endif

        string MonitorTaskName = "";    // 要监控的任务名

        Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder();
        long CurResultOffs = 0;

        long CurResultVersion = 0;

        const int WM_INITIAL = API.WM_USER + 201;

        MessageStyle m_messageStyle = MessageStyle.Progress | MessageStyle.Result;

        /// <summary>
        /// 消息风格
        /// </summary>
        public MessageStyle MessageStyle
        {
            get
            {
                return this.m_messageStyle;
            }
            set
            {
                this.m_messageStyle = value;

                this.ToolStripMenuItem_progress.Checked = false;
                this.ToolStripMenuItem_result.Checked = false;

                this.label_progress.Text = "";  // 每次变动，都清除进度显示行

                if ((this.m_messageStyle & MessageStyle.Progress) != 0)
                    this.ToolStripMenuItem_progress.Checked = true;
                if ((this.m_messageStyle & MessageStyle.Result) != 0)
                    this.ToolStripMenuItem_result.Checked = true;
               
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BatchTaskForm()
        {
            InitializeComponent();
        }

        private void BatchTaskForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.comboBox_taskName.Text = MainForm.AppInfo.GetString(
"BatchTaskForm",
"BatchTaskName",
    "");

            this.webBrowser_info.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_info_DocumentCompleted);


            // 使得菜单显示正确
            this.MessageStyle = this.MessageStyle;

            // 
            API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
        }

        void webBrowser_info_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Global.ScrollToEnd(this.webBrowser_info);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INITIAL:
                    {
                        stop.SetMessage("正在初始化浏览器控件...");
                        this.Update();
                        this.MainForm.Update();

                        ClearWebBrowser(this.webBrowser_info, true);

                        if (this.toolStripButton_monitoring.Checked == true)
                        {
                            StartMonitor(this.comboBox_taskName.Text,
                                this.toolStripButton_monitoring.Checked);
                        }
                        stop.SetMessage("");
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void BatchTaskForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.webBrowser_info.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(webBrowser_info_DocumentCompleted);

            this.timer_monitorTask.Stop();

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SetString(
    "BatchTaskForm",
    "BatchTaskName",
    this.comboBox_taskName.Text);
            }

        }

        private void button_start_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = StartBatchTask(this.comboBox_taskName.Text,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "任务 '" +this.comboBox_taskName.Text+ "' 已成功启动");

        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = StopBatchTask(this.comboBox_taskName.Text,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "任务 '" + this.comboBox_taskName.Text + "' 已停止");

        }

#if NO
        // *** 已经删除
        private void checkBox_monitoring_CheckedChanged(object sender, EventArgs e)
        {
            if (this.comboBox_taskName.Text == "")
                return;

                StartMonitor(this.comboBox_taskName.Text,
                    this.checkBox_monitoring.Checked);

        }
#endif

        // 启动或停止监控一个任务
        void StartMonitor(string strTaskName,
            bool bStart)
        {
            if (String.IsNullOrEmpty(strTaskName) == true)
                return;

            this.MonitorTaskName = strTaskName;

            // 重新创建解码器（以便清除残留的信息）
            this.ResultTextDecoder = Encoding.UTF8.GetDecoder();
            this.CurResultOffs = 0;

            if (bStart == true)
            {
                this.timer_monitorTask.Start();
            }
            else
            {
                this.timer_monitorTask.Stop();
            }
        }

        // 启动批处理任务
        int StartBatchTask(string strTaskName,
            out string strError)
        {
            strError = "";

            BatchTaskStartInfo startinfo = new BatchTaskStartInfo();
            if (strTaskName == "日志恢复")
            {
                StartLogRecoverDlg dlg = new StartLogRecoverDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
            }
            else if (strTaskName == "dp2Library 同步")
            {
                StartReplicationDlg dlg = new StartReplicationDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
            }
            else if (strTaskName == "重建检索点")
            {
                StartRebuildKeysDlg dlg = new StartRebuildKeysDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
            }
            else if (strTaskName == "预约到书管理")
            {
                StartArriveMonitorDlg dlg = new StartArriveMonitorDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
            }
            /*
        else if (strTaskName == "跟踪DTLP数据库")
        {
            StartTraceDtlpDlg dlg = new StartTraceDtlpDlg();
        MainForm.SetControlFont(dlg, this.Font, false);
            dlg.StartInfo = startinfo;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
            {
                strError = "用户放弃启动";
                return -1;
            }
        }
             * */
            else if (strTaskName == "正元一卡通读者信息同步")
            {
                StartZhengyuanReplicationDlg dlg = new StartZhengyuanReplicationDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
            }
            else if (strTaskName == "迪科远望一卡通读者信息同步")
            {
                StartDkywReplicationDlg dlg = new StartDkywReplicationDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                startinfo.Start = "!breakpoint";    // 一开始就有适当的缺省值，避免从头开始跟踪
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
            }
            else if (strTaskName == "读者信息同步")
            {
#if NO
                StartPatronReplicationDlg dlg = new StartPatronReplicationDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                startinfo.Start = "!breakpoint";    // 一开始就有适当的缺省值，避免从头开始跟踪
                dlg.StartInfo = startinfo;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户放弃启动";
                    return -1;
                }
#endif
                startinfo.Start = "activate";   // 表示立即启动，忽略服务器原有定时启动参数
            }
            else if (strTaskName == "超期通知")
            {
                startinfo.Start = "activate";   // 表示立即启动，忽略服务器原有定时启动参数
            }

            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            try
            {
                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在"
                    + "启动"
                    + "任务 '" + strTaskName + "' ...");
                stop.BeginLoop();

                this.Update();
                this.MainForm.Update();

                try
                {
                    BatchTaskInfo param = new BatchTaskInfo();
                    param.StartInfo = startinfo;

                    BatchTaskInfo resultInfo = null;

                    // return:
                    //      -1  出错
                    //      0   启动成功
                    //      1   调用前任务已经处于执行状态，本次调用激活了这个任务
                    long lRet = Channel.BatchTask(
                        stop,
                        strTaskName,
                        "start",
                        param,
                        out resultInfo,
                        out strError);
                    if (lRet == -1 || lRet == 1)
                        goto ERROR1;

                    if (resultInfo != null)
                    {
                        Global.WriteHtml(this.webBrowser_info,
                            GetResultText(resultInfo.ResultText));
                        ScrollToEnd();
                    }

                    this.label_progress.Text = resultInfo.ProgressText;
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 停止批处理任务
        int StopBatchTask(string strTaskName,
            out string strError)
        {
            strError = "";

            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            try
            {

                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在"
                    + "停止"
                    + "任务 '" + strTaskName + "' ...");
                stop.BeginLoop();

                this.Update();
                this.MainForm.Update();

                try
                {
                    BatchTaskInfo param = new BatchTaskInfo();
                    BatchTaskInfo resultInfo = null;

                    long lRet = Channel.BatchTask(
                        stop,
                        strTaskName,
                        "stop",
                        param,
                        out resultInfo,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    Global.WriteHtml(this.webBrowser_info,
                        GetResultText(resultInfo.ResultText));
                    ScrollToEnd();

                    this.label_progress.Text = resultInfo.ProgressText;
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 启动批处理任务
        int ContinueAllBatchTask(out string strError)
        {
            strError = "";

            BatchTaskStartInfo startinfo = new BatchTaskStartInfo();

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在继续全部批处理任务 ...");
                stop.BeginLoop();

                this.Update();
                this.MainForm.Update();

                try
                {
                    BatchTaskInfo param = new BatchTaskInfo();
                    param.StartInfo = startinfo;

                    BatchTaskInfo resultInfo = null;

                    long lRet = Channel.BatchTask(
                        stop,
                        "",
                        "continue",
                        param,
                        out resultInfo,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (resultInfo != null)
                    {
                        Global.WriteHtml(this.webBrowser_info,
                            GetResultText(resultInfo.ResultText));
                        ScrollToEnd();

                        this.label_progress.Text = resultInfo.ProgressText;
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 暂停批处理任务
        int PauseAllBatchTask(out string strError)
        {
            strError = "";

            BatchTaskStartInfo startinfo = new BatchTaskStartInfo();

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在暂停全部批处理任务 ...");
                stop.BeginLoop();

                this.Update();
                this.MainForm.Update();

                try
                {
                    BatchTaskInfo param = new BatchTaskInfo();
                    param.StartInfo = startinfo;

                    BatchTaskInfo resultInfo = null;

                    long lRet = Channel.BatchTask(
                        stop,
                        "",
                        "pause",
                        param,
                        out resultInfo,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (resultInfo != null)
                    {
                        Global.WriteHtml(this.webBrowser_info,
                            GetResultText(resultInfo.ResultText));
                        ScrollToEnd();

                        this.label_progress.Text = resultInfo.ProgressText;
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }


        private void comboBox_taskName_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_taskName.Text == "")
                this.toolStripButton_monitoring.Enabled = false;
            else
                this.toolStripButton_monitoring.Enabled = true;

            if (this.MonitorTaskName != this.comboBox_taskName.Text)    // 2015/11/26
            {
                this.MonitorTaskName = this.comboBox_taskName.Text;
                this.CurResultOffs = 0;
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.comboBox_taskName.Enabled = bEnable;

            if (this.comboBox_taskName.Text != "")
                this.toolStripButton_monitoring.Enabled = bEnable;
            else
                this.toolStripButton_monitoring.Enabled = false;

            this.button_start.Enabled = bEnable;
            this.button_stop.Enabled = bEnable;

            /*
            this.button_refresh.Enabled = bEnable;
            this.button_rewind.Enabled = bEnable;
            this.button_clear.Enabled = bEnable;
             * */

            this.toolStripButton_refresh.Enabled = bEnable;
            this.toolStripButton_rewind.Enabled = bEnable;
            this.toolStripButton_clear.Enabled = bEnable;
            this.toolStripButton_continue.Enabled = bEnable;
            this.toolStripButton_pauseAll.Enabled = bEnable;
        }


        string GetResultText(byte[] baResult)
        {
            if (baResult == null)
                return "";
            if (baResult.Length == 0)
                return "";

            // Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder;
            char[] chars = new char[baResult.Length];

            int nCharCount = this.ResultTextDecoder.GetChars(
                baResult,
                    0,
                    baResult.Length,
                    chars,
                    0);
            Debug.Assert(nCharCount <= baResult.Length, "");

            return new string(chars, 0, nCharCount);
        }

        private void timer_monitorTask_Tick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(MonitorTaskName) == true)
                return;

            if (m_nInRefresh == 0)
            {
                // Global.ScrollToEnd(this.webBrowser_info);
                // ScrollToEnd();


                DoRefresh();
            }
        }

        void ScrollToEnd()
        {
            this.webBrowser_info.Document.Window.ScrollTo(0,
this.webBrowser_info.Document.Body.ScrollRectangle.Height);
        }

        // *** 已经删除
        private void button_refresh_Click(object sender, EventArgs e)
        {
            if (m_nInRefresh > 0)
            {
                MessageBox.Show(this, "正在刷新中");
                return;
            }

            DoRefresh();
        }

        void DoRefresh()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.m_nInRefresh++;
            this.toolStripButton_refresh.Enabled = false;
            // this.EnableControls(false);
            try
            {

                string strError = "";

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获取任务 '" + MonitorTaskName + "' 的最新信息 ...");
                stop.BeginLoop();

                try
                {

                    for (int i=0;i<10;i++)  // 最多循环获取10次
                    {
                        Application.DoEvents();
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        BatchTaskInfo param = new BatchTaskInfo();
                        BatchTaskInfo resultInfo = null;

                        if ((this.MessageStyle & MessageStyle.Result) == 0)
                        {
                            param.MaxResultBytes = 0;
                        }
                        else
                        {
                            param.MaxResultBytes = 4096;
                            if (i >= 5)  // 如果发现尚未来得及获取的内容太多，就及时扩大“窗口”尺寸
                                param.MaxResultBytes = 100 * 1024;
                        }

                        param.ResultOffset = this.CurResultOffs;

                        stop.SetMessage("正在获取任务 '" + MonitorTaskName + "' 的最新信息 (第 "+(i+1).ToString()+" 批 共10批)...");

                        long lRet = Channel.BatchTask(
                            stop,
                            MonitorTaskName,
                            "getinfo",
                            param,
                            out resultInfo,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        Global.WriteHtml(this.webBrowser_info,
                            GetResultText(resultInfo.ResultText));
                        ScrollToEnd();

                        // DateTime now = DateTime.Now;

                        if ((this.MessageStyle & MessageStyle.Progress) != 0)
                        {
                            this.label_progress.Text = // now.ToLongTimeString() + " -- " + 
                                resultInfo.ProgressText;
                        }

                        if ((this.MessageStyle & MessageStyle.Result) == 0)
                        {
                            // 没有必要显示累积
                            break;
                        }

                        if (this.CurResultOffs == 0)
                            this.CurResultVersion = resultInfo.ResultVersion;
                        else if (this.CurResultVersion != resultInfo.ResultVersion)
                        {
                            // 说明服务器端result文件其实已经更换
                            this.CurResultOffs = 0; // rewind
                            Global.WriteHtml(this.webBrowser_info,
                                "***新内容 version=" + resultInfo.ResultVersion.ToString() + " ***\r\n");
                            ScrollToEnd();
                            goto COINTINU1;
                        }

                        if (resultInfo.ResultTotalLength < param.ResultOffset)
                        {
                            // 说明服务器端result文件其实已经更换
                            this.CurResultOffs = 0; // rewind
                            Global.WriteHtml(this.webBrowser_info,
                                "***新内容***\r\n");
                            ScrollToEnd();
                            goto COINTINU1;
                        }
                        else
                        {
                            // 存储用以下次
                            this.CurResultOffs = resultInfo.ResultOffset;
                        }

                    COINTINU1:
                        // 如果本次并没有“触底”，需要立即循环获取新的信息。但是循环有一个最大次数，以应对服务器疯狂发生信息的情形。
                        if (resultInfo.ResultOffset >= resultInfo.ResultTotalLength)
                            break;
                    }
                }
                finally
                {
                    this.toolStripButton_refresh.Enabled = true;
                    // this.EnableControls(true);
                    this.m_nInRefresh--;
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }

                return;
            ERROR1:
                this.label_progress.Text = strError;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // *** 已经删除
        private void button_clear_Click(object sender, EventArgs e)
        {
            ClearWebBrowser(this.webBrowser_info, false);
        }

        // *** 已经删除
        private void button_rewind_Click(object sender, EventArgs e)
        {
            ClearWebBrowser(this.webBrowser_info, true);
        }

        // parameters:
        //      bRewind 是否顺便把指针拨向从头开始获取
        void ClearWebBrowser(WebBrowser webBrowser,
            bool bRewind)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
            doc.Write("<pre>");
            if (bRewind == true)
                this.CurResultOffs = 0; // 从头开始获取?
        }

        private void BatchTaskForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        private void ToolStripMenuItem_result_Click(object sender, EventArgs e)
        {
            if ((this.MessageStyle & MessageStyle.Result) != 0)
                this.MessageStyle -= MessageStyle.Result;
            else
                this.MessageStyle |= MessageStyle.Result;
        }

        private void ToolStripMenuItem_progress_Click(object sender, EventArgs e)
        {
            if ((this.MessageStyle & MessageStyle.Progress) != 0)
                this.MessageStyle -= MessageStyle.Progress;
            else
                this.MessageStyle |= MessageStyle.Progress;

        }

        // 刷新
        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            if (m_nInRefresh > 0)
            {
                MessageBox.Show(this, "正在刷新中");
                return;
            }

            DoRefresh();
        }

        // 一直显示进度
        private void toolStripButton_monitoring_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboBox_taskName.Text) == true)
            {
                this.ShowMessage("尚未选定任务名", "red", true);
                return;
            }

            StartMonitor(this.comboBox_taskName.Text,
                this.toolStripButton_monitoring.Checked);
        }

        // 从头重新获取
        private void toolStripButton_rewind_Click(object sender, EventArgs e)
        {
            ClearWebBrowser(this.webBrowser_info, true);
        }

        // 清除
        private void toolStripButton_clear_Click(object sender, EventArgs e)
        {
            ClearWebBrowser(this.webBrowser_info, false);
        }

        // 暂停
        private void toolStripButton_pauseAll_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PauseAllBatchTask(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "暂停全部批处理任务成功");

        }

        // 继续
        private void toolStripButton_continue_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = ContinueAllBatchTask(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "继续全部批处理任务成功");
        }


    }

    /// <summary>
    /// 消息风格
    /// </summary>
    [Flags]
    public enum MessageStyle
    {
        /// <summary>
        /// 累积内容
        /// </summary>
        Result = 0x01,  // 累积内容

        /// <summary>
        /// 进度
        /// </summary>
        Progress = 0x02,    // 进度
    }
}