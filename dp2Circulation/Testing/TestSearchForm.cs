using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    internal partial class TestSearchForm : MyForm
    {
        int m_nBeforeAbort = -1;

        // bool m_bStopWatching = true;
        internal Thread threadWorker = null;
        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号
        internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 


        // public string QueryFilename = "";

        public TestSearchForm()
        {
            this.UseLooping = true; // 2022/11/4

            InitializeComponent();
        }

        private async void TestSearchForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
            Program.MainForm.AppInfo.LoadMdiLayout += new EventHandler(AppInfo_LoadMdiLayout);
            Program.MainForm.AppInfo.SaveMdiLayout += new EventHandler(AppInfo_SaveMdiLayout);

            this.textBox_biblioSearch_queryFilename.Text = Program.MainForm.AppInfo.GetString(
                "testsearchform",
                "queryfilename",
                "");


            this.textBox_searchBiblio_beforeAbort.Text = Program.MainForm.AppInfo.GetString(
                "testsearchform",
                "beforeabort",
                "-1");


            this.textBox_searchBiblio_loopTimes.Text = Program.MainForm.AppInfo.GetString(
                "testsearchform",
                "looptimes",
                "1");

            var ret = await Program.MainForm.EnsureConnectLibraryServerAsync();
            if (ret == false)
            {
                this.ShowMessage("连接到 dp2library 失败，本窗口部分功能无法使用", "red");
            }

            BeginWorkerThread();
        }

        private void TestSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.eventClose.Set();

        }

        private void TestSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                    "testsearchform",
                    "queryfilename",
                    this.textBox_biblioSearch_queryFilename.Text);

                Program.MainForm.AppInfo.SetString(
        "testsearchform",
        "beforeabort",
        this.textBox_searchBiblio_beforeAbort.Text);

                Program.MainForm.AppInfo.SetString(
        "testsearchform",
        "looptimes",
        this.textBox_searchBiblio_loopTimes.Text);

                Program.MainForm.AppInfo.LoadMdiLayout -= new EventHandler(AppInfo_LoadMdiLayout);
                Program.MainForm.AppInfo.SaveMdiLayout -= new EventHandler(AppInfo_SaveMdiLayout);
            }
        }

        void AppInfo_SaveMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;
        }

        void AppInfo_LoadMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;
        }

        private void button_searchBiblio_findFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定检索式文件名";
            dlg.FileName = this.textBox_biblioSearch_queryFilename.Text;
            dlg.Filter = "检索式文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_biblioSearch_queryFilename.Text = dlg.FileName;
        }

        // 2022/11/4
        public override void EnableControls(bool bEnable)
        {
            EnableControlsInSearching(bEnable);
        }

        // TODO: 重构
        void EnableControlsInSearching(bool bEnable)
        {
            this.TryInvoke(() =>
            {
                this.button_beginSearch.Enabled = bEnable;
                this.textBox_biblioSearch_queryFilename.Enabled = bEnable;
                this.button_searchBiblio_findFilename.Enabled = bEnable;
                this.textBox_searchBiblio_beforeAbort.Enabled = bEnable;
                this.textBox_searchBiblio_loopTimes.Enabled = bEnable;
            });
        }

        // parameters:
        //      bRewind 是否顺便把指针拨向从头开始获取
        void ClearWebBrowser(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
            doc.Write("<pre>");
        }

        void BeginWorkerThread()
        {
            // 如果以前在做，立即停止
            // m_bStopWatching = true;

            this.threadWorker =
        new Thread(new ThreadStart(this.ThreadMain));
            this.threadWorker.Start();
        }

        // 工作线程每一轮循环的实质性工作
        public virtual void Worker()
        {
            // 等待一定时间，切断通道
            WaitHandle[] events = new WaitHandle[2];
            events[0] = eventClose;
            events[1] = eventActive;

            int index = 0;
            try
            {
                index = WaitHandle.WaitAny(events, this.m_nBeforeAbort, false);
            }
            catch (System.Threading.ThreadAbortException /*ex*/)
            {
                return;
            }

            if (index == WaitHandle.WaitTimeout)
            {
                // 超时

                // TODO: 显示?
                // 进行中断
                try
                {
                    // this.Channel.Abort();
                    this.DoStop(null, null);
                }
                catch
                {
                    // 这是为了让线程不会因为通讯故障而退出
                }
                return;
            }
            else if (index == 0)
            {
                eventClose.Set();
                return;
            }
            else
            {
                // 得到下一轮激活信号
                eventActive.Set();
                return;
            }

        }

        public void ThreadMain()
        {
            try
            {
                WaitHandle[] events = new WaitHandle[2];
                events[0] = eventClose;
                events[1] = eventActive;

                while (true)
                {
                    int index = 0;
                    try
                    {
                        index = WaitHandle.WaitAny(events, -1, false);
                    }
                    catch (System.Threading.ThreadAbortException /*ex*/)
                    {
                        break;
                    }

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // 超时
                        eventActive.Reset();
                        Worker();
                    }
                    else if (index == 0)
                    {
                        break;
                    }
                    else
                    {
                        // 得到激活信号
                        eventActive.Reset();
                        Worker();
                    }
                }
            }
            catch (Exception /*ex*/)
            {
            }
            finally
            {
                eventFinished.Set();
                // this.m_bStopWatching = true;
            }

        }

        private void button_beginSearch_Click(object sender, EventArgs e)
        {
            string strError = "";
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.Load(this.textBox_biblioSearch_queryFilename.Text);
            }
            catch (Exception ex)
            {
                strError = "检索式文件装入XMLDOM发生错误: " + ex.Message;
                goto ERROR1;
            }

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在检索...");
            _stop.BeginLoop();

            this.EnableControlsInSearching(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在检索...",
                "disableControl");
            try
            {
                ClearWebBrowser(this.webBrowser1);

                int nStart = -1;
                int nEnd = -1;
                Random randObj = new Random();
                int nRet = this.textBox_searchBiblio_beforeAbort.Text.IndexOf("-");
                if (nRet == -1 || this.textBox_searchBiblio_beforeAbort.Text == "-1")
                {
                    this.m_nBeforeAbort = Convert.ToInt32(this.textBox_searchBiblio_beforeAbort.Text);
                }
                else
                {
                    string strStart = this.textBox_searchBiblio_beforeAbort.Text.Substring(0, nRet).Trim();
                    string strEnd = this.textBox_searchBiblio_beforeAbort.Text.Substring(nRet + 1).Trim();
                    nStart = Convert.ToInt32(strStart);
                    nEnd = Convert.ToInt32(strEnd);
                }

                int nLoopTimes = Convert.ToInt32(this.textBox_searchBiblio_loopTimes.Text);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("query");
                for (int j = 0; j < nLoopTimes; j++)
                {
                    looping.Progress.SetMessage("循环 " + (j + 1).ToString() + "...");
                    Global.WriteHtml(this.webBrowser1,
        "\r\n循环 " + (j + 1).ToString() + "...\r\n");

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (looping.Stopped)
                        {
                            strError = "中断";
                            goto ERROR1;
                        }

                        XmlNode node = nodes[i];

                        string strWord = DomUtil.GetAttr(node, "word");
                        string strDbName = DomUtil.GetAttr(node, "dbname");
                        string strFromStyle = DomUtil.GetAttr(node, "fromstyle");
                        string strMatchStyle = DomUtil.GetAttr(node, "matchstyle");
                        string strComment = DomUtil.GetAttr(node, "comment");

                        int nTimeLimit = -2;
                        if (node.Attributes["timelimit"] != null)
                            nTimeLimit = Convert.ToInt32(DomUtil.GetAttr(node, "timelimit"));

                        // timelimit属性优先起作用
                        if (nTimeLimit != -2)
                            this.m_nBeforeAbort = nTimeLimit;
                        else
                        {
                            if (nStart != -1 && nEnd != -1)
                            {
                                this.m_nBeforeAbort = randObj.Next(nStart, nEnd);
                            }
                            else
                                this.m_nBeforeAbort = Convert.ToInt32(this.textBox_searchBiblio_beforeAbort.Text);
                        }

                        looping.Progress.SetMessage("正在检索 '" + strWord + "' ...");
                        Global.WriteHtml(this.webBrowser1,
            "正在检索 '" + strWord + "' 中断前毫秒数=" + this.m_nBeforeAbort.ToString() + "; " + strComment + "...\r\n");

                        this.eventActive.Set();

                        DateTime timeStart = DateTime.Now;

                        string strQueryXml = "";
                        long lRet = channel.SearchBiblio(looping.Progress,
                            strDbName,
                            strWord,
                            -1,
                            strFromStyle,
                            strMatchStyle,
                            this.Lang,
                            null,   // strResultSetName
                            "",    // strSearchStyle
                            "", // strOutputStyle,
                            "",
                            out strQueryXml,
                            out _,
                            out strError);

                        TimeSpan delta = DateTime.Now - timeStart;

                        Global.WriteHtml(this.webBrowser1,
    "    返回 lRet='" + lRet.ToString() + "' strError='" + strError + "' 用时= " + delta.TotalSeconds.ToString() + " 秒\r\n");

                        if (lRet == -1)
                        {
                            if (channel.ErrorCode != ErrorCode.RequestCanceled)
                                goto ERROR1;
                        }

                        long lHitCount = lRet;
                    }
                }
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControlsInSearching(true);
                */
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
