using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;

namespace dp2Circulation
{
    /// <summary>
    /// 入馆登记窗
    /// </summary>
    public partial class PassGateForm : MyForm
    {
        Commander commander = null;

        WebExternalHost m_webExternalHost = new WebExternalHost();

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
#endif

        ReaderWriterLock m_lock = new ReaderWriterLock();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        internal Thread threadWorker = null;
        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号
        internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        /// <summary>
        /// 轮询的间隔时间，单位是 1/1000 秒。缺省为 1 分钟
        /// </summary>
        public int PerTime = 1 * 60 * 1000;	// 1分钟?
        internal bool m_bClosed = true;

        int m_nTail = 0;

        // string HtmlString = "";

        // const int WM_SETHTML = API.WM_USER + 201;
        const int WM_RESTOREFOCUS = API.WM_USER + 202;

        bool m_bActive = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PassGateForm()
        {
            InitializeComponent();
        }

        private void PassGateForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // webbrowser
            this.m_webExternalHost.Initial(this.MainForm, this.webBrowser_readerInfo);
            this.webBrowser_readerInfo.ObjectForScripting = this.m_webExternalHost;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            this.AcceptButton = this.button_passGate;

            this.textBox_gateName.Text = this.MainForm.AppInfo.GetString(
                "passgate_form",
                "gate_name",
                "");
            this.checkBox_displayReaderDetailInfo.Checked = this.MainForm.AppInfo.GetBoolean(
                "passgate_form",
                "display_reader_detail_info",
                true);
            this.checkBox_hideBarcode.Checked = this.MainForm.AppInfo.GetBoolean(
                "passgate_form",
                "hide_barcode",
                false);
            this.checkBox_hideReaderName.Checked = this.MainForm.AppInfo.GetBoolean(
                "passgate_form",
                "hide_readername",
                false);

            this.StartWorkerThread();
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost.ChannelInUse;
        }

        private void PassGateForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.CloseThread();

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Destroy();

#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
                    "passgate_form",
                    "gate_name",
                    this.textBox_gateName.Text);

                this.MainForm.AppInfo.SetBoolean(
                    "passgate_form",
                    "display_reader_detail_info",
                    this.checkBox_displayReaderDetailInfo.Checked);
                this.MainForm.AppInfo.SetBoolean(
                    "passgate_form",
                    "hide_barcode",
                    this.checkBox_hideBarcode.Checked);
                this.MainForm.AppInfo.SetBoolean(
                    "passgate_form",
                    "hide_readername",
                    this.checkBox_hideReaderName.Checked);
            }
        }

        void ClearList()
        {
            this.listView_list.Items.Clear();
        }

        // 提交读者证条码号
        private void button_passGate_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.textBox_readerBarcode.Text) == true)
            {
                strError = "请输入读者证条码号";
                goto ERROR1;
            }

            int nMaxListItemsCount = this.MaxListItemsCount;

            ListViewItem item = new ListViewItem();

            if (this.checkBox_hideBarcode.Checked == true)
            {
                string strText = "";
                item.Text = strText.PadLeft(this.textBox_readerBarcode.Text.Length, '*');
            }
            else
                item.Text = this.textBox_readerBarcode.Text;

            ReaderInfo info = new ReaderInfo();
            info.ReaderBarcode = this.textBox_readerBarcode.Text;
            item.Tag = info;

            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add(DateTime.Now.ToString());

            item.ImageIndex = 0;    // 尚未填充数据

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (nMaxListItemsCount != -1
                    && this.listView_list.Items.Count > nMaxListItemsCount)
                    ClearList();

                this.listView_list.Items.Add(item);
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            item.EnsureVisible();

            this.eventActive.Set(); // 告诉工作线程

            this.textBox_readerBarcode.SelectAll();
            this.textBox_readerBarcode.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            this.textBox_readerBarcode.SelectAll();
            this.textBox_readerBarcode.Focus();
        }

        // 启动工作线程
        /*public*/ void StartWorkerThread()
        {
            this.m_bClosed = false;

            this.eventActive.Set();
            this.eventClose.Reset(); 

            this.threadWorker =
                new Thread(new ThreadStart(this.ThreadMain));
            this.threadWorker.Start();
        }

        /*public*/ void CloseThread()
        {
            this.eventClose.Set();
            this.m_bClosed = true;
        }

        /*public*/ void Stop()
        {
            this.eventClose.Set();
            this.m_bClosed = true;
        }

        // 工作线程
        /*public virtual*/ void ThreadMain()
        {
            try
            {
                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventActive;

                while (true)
                {
                    int index = WaitHandle.WaitAny(events, PerTime, false);

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

                    /*
                    // 是否循环?
                    if (this.Loop == false)
                        break;
                     * */
                }

                eventFinished.Set();
            }
            catch (Exception ex)
            {
                string strErrorText = "PassGateForm ThreadMain() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.MainForm.WriteErrorLog(strErrorText);
            }
        }

        ListViewItem GetItem(int i)
        {
            if (this.listView_list.InvokeRequired)
            {
                return (ListViewItem)this.listView_list.Invoke(new Func<int, ListViewItem>(GetItem), i);
            }

            return this.listView_list.Items[i];
        }

        void SetItemText(ListViewItem item, int column, string strText)
        {
            if (this.listView_list.InvokeRequired)
            {
                this.listView_list.BeginInvoke(new Action<ListViewItem, int, string>(SetItemText), item, column, strText);
                return;
            }

            ListViewUtil.ChangeItemText(item, column, strText);
        }

        void SetCounterText(long v)
        {
            if (this.textBox_counter.InvokeRequired)
            {
                this.textBox_counter.BeginInvoke(new Action<long>(SetCounterText), v);
                return;
            }
            this.textBox_counter.Text = v.ToString();
        }

        void SetItemImageIndex(ListViewItem item, int v)
        {
            if (this.listView_list.InvokeRequired)
            {
                this.listView_list.BeginInvoke(new Action<ListViewItem, int>(SetItemImageIndex), item, v);
                return;
            }

            item.ImageIndex = v;
        }

        void StartSetHtml(string strHtml)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(StartSetHtml), strHtml);
                return;
            }

            // API.PostMessage(this.Handle, WM_SETHTML, 0, 0);
            this.m_webExternalHost.SetHtmlString(strHtml,
    "passgateform_reader");
        }
        // listview imageindex 0:尚未初始化 1:已经初始化 2:出错

        // 工作线程每一轮循环的实质性工作
        void Worker()
        {
            try
            {
                string strError = "";

                for (int i = this.m_nTail; i < this.listView_list.Items.Count; i++)
                {
                    // ListViewItem item = this.listView_list.Items[i];
                    ListViewItem item = GetItem(i);

                    if (item.ImageIndex == 1)
                        continue;

                    // string strBarcode = item.Text;
                    ReaderInfo info = (ReaderInfo)item.Tag;
                    string strBarcode = info.ReaderBarcode;

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在初始化浏览器组件 ...");
                    stop.BeginLoop();

                    string strTypeList = "xml";
                    int nTypeCount = 1;

                    if (this.checkBox_displayReaderDetailInfo.Checked == true)
                    {
                        strTypeList += ",html";

                        if (this.MainForm.ServerVersion >= 2.25)
                            strTypeList += ":noborrowhistory";

                        nTypeCount = 2;
                    }

                    try
                    {
                        string[] results = null;
                        long lRet = Channel.PassGate(stop,
                            strBarcode,
                            this.textBox_gateName.Text, // strGateName
                            strTypeList,
                            out results,
                            out strError);
                        if (lRet == -1)
                        {
                            OnError(item, strError);
                            goto CONTINUE;
                        }

                        // this.textBox_counter.Text = lRet.ToString();
                        SetCounterText(lRet);

                        if (results.Length != nTypeCount)
                        {
                            strError = "results error...";
                            OnError(item, strError);
                            goto CONTINUE;
                        }

                        string strXml = results[0];

                        string strReaderName = "";
                        string strState = "";
                        int nRet = GetRecordInfo(strXml,
                            out strReaderName,
                            out strState,
                            out strError);
                        if (nRet == -1)
                        {
                            OnError(item, strError);
                            goto CONTINUE;
                        }

                        info.ReaderName = strReaderName;

                        if (this.checkBox_hideReaderName.Checked == true)
                        {
                            string strText = "";
                            // item.SubItems[1].Text = strText.PadLeft(strReaderName.Length, '*');
                            SetItemText(item, 1, strText.PadLeft(strReaderName.Length, '*'));
                        }
                        else
                        {
                            // item.SubItems[1].Text = strReaderName;
                            SetItemText(item, 1, strReaderName);
                        }

                        // item.SubItems[2].Text = strState;
                        SetItemText(item, 2, strState);

                        // item.ImageIndex = 1;    // error
                        SetItemImageIndex(item, 1);

                        if (this.checkBox_displayReaderDetailInfo.Checked == true
                            && results.Length == 2)
                        {
                            this.m_webExternalHost.StopPrevious();
                            this.webBrowser_readerInfo.Stop();

                            // this.HtmlString = results[1];

                            // API.PostMessage(this.Handle, WM_SETHTML, 0, 0);
                            StartSetHtml(results[1]);
                        }
                    }
                    finally
                    {
                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                    }

                CONTINUE:
                    this.m_nTail = i;
                }
            }
            catch(Exception ex)
            {
                string strErrorText = "PassGateForm Worker() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.MainForm.WriteErrorLog(strErrorText);
            }
        }

        void OnError(ListViewItem item,
            string strError)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<ListViewItem, string>(OnError), item, strError);
                return;
            }

            item.SubItems[1].Text = strError;
            item.ImageIndex = 2;    // error

            // this.HtmlString = strError;
            // API.PostMessage(this.Handle, WM_SETHTML, 0, 0);
            StartSetHtml(strError);

            // 发出警告性的响声
            Console.Beep();
        }

        static int GetRecordInfo(string strXml,
            out string strReaderName,
            out string strState,
            out string strError)
        {
            strError = "";
            strReaderName = "";
            strState = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            strReaderName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            return 0;
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
#if NO
                case WM_SETHTML:
                    if (this.m_webExternalHost.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {
                        this.m_webExternalHost.SetHtmlString(this.HtmlString,
                            "passgateform_reader");
                    }
                    return;
#endif
                case WM_RESTOREFOCUS:
                    this.textBox_readerBarcode.Focus();
                    this.textBox_readerBarcode.SelectAll();
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void checkBox_displayReaderDetailInfo_CheckedChanged(object sender, EventArgs e)
        {
#if NO
            Global.SetHtmlString(this.webBrowser_readerInfo,
                "(空白)");
#endif
            this.m_webExternalHost.ClearHtmlPage();
        }

        private void checkBox_hideBarcode_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_hideBarcode.Checked == true)
            {
                this.textBox_readerBarcode.PasswordChar = '*';
                this.textBox_readerBarcode.Text = "";

            }
            else
            {
                this.textBox_readerBarcode.PasswordChar = (char)0;
            }
            bool bChecked = this.checkBox_hideBarcode.Checked;
            // 修改listview内容
            for (int i = 0; i < this.listView_list.Items.Count; i++)
            {
                ListViewItem item = this.listView_list.Items[i];

                if (bChecked == true)
                {
                    int nLength = item.Text.Length;
                    string strText = "";
                    item.Text = strText.PadLeft(nLength, '*');
                }
                else
                {
                    ReaderInfo info = (ReaderInfo)item.Tag;
                    item.Text = info.ReaderBarcode;
                }
            }

        }

        private void checkBox_hideReaderName_CheckedChanged(object sender, EventArgs e)
        {
            bool bChecked = this.checkBox_hideReaderName.Checked;
            // 修改listview内容
            for (int i = 0; i < this.listView_list.Items.Count; i++)
            {
                ListViewItem item = this.listView_list.Items[i];

                if (item.ImageIndex == 2)
                    continue;

                if (bChecked == true)
                {
                    int nLength = item.SubItems[1].Text.Length;
                    string strText = "";
                    item.SubItems[1].Text = strText.PadLeft(nLength, '*');
                }
                else
                {
                    ReaderInfo info = (ReaderInfo)item.Tag;
                    item.SubItems[1].Text = info.ReaderName;
                }
            }

        }

        private void textBox_readerBarcode_Enter(object sender, EventArgs e)
        {
            this.MainForm.EnterPatronIdEdit(InputType.PQR);
        }

        private void textBox_readerBarcode_Leave(object sender, EventArgs e)
        {
            this.MainForm.LeavePatronIdEdit();

            if (m_bActive == false)
                return;

            if (Control.ModifierKeys == Keys.Control)
                return;

            API.PostMessage(this.Handle, WM_RESTOREFOCUS, 0, 0);
        }

        private void PassGateForm_Activated(object sender, EventArgs e)
        {
            // this.MainForm.stopManager.Active(this.stop);

            m_bActive = true;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        private void PassGateForm_Deactivate(object sender, EventArgs e)
        {
            m_bActive = false;
        }

        /// <summary>
        /// 列表中的最大行数。每当到达这个行数的时候，列表被自动清空一次。
        /// -1 表示不限制
        /// </summary>
        public int MaxListItemsCount
        {
            get
            {
                return MainForm.AppInfo.GetInt(
                    "passgate_form",
                    "max_list_items_count",
                    1000);
            }
        }

        private void webBrowser_readerInfo_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }
        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (this.MainForm.SuppressScriptErrors == true)
                e.Handled = true;
        }
    }

    class ReaderInfo
    {
        public string ReaderBarcode = "";
        public string ReaderName = "";
    }
}