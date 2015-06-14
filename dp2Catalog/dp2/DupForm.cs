using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Catalog
{
    public partial class dp2DupForm : Form, ISearchForm
    {
        bool m_bInSearch = false;

        // 当前缺省的编码方式
        Encoding CurrentEncoding = Encoding.UTF8;

        public LibraryChannelCollection Channels = null;
        LibraryChannel Channel = null;

        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        string m_strXmlRecord = "";

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventFinish = new AutoResetEvent(false);

        public bool AutoBeginSearch = false;

        const int WM_INITIAL = API.WM_USER + 201;

        const int ITEMTYPE_NORMAL = 0;  // 普通事项
        const int ITEMTYPE_OVERTHRESHOLD = 1; // 权值超过阈值的事项

        #region 外部接口

        /// <summary>
        /// 查重方案名
        /// </summary>
        public string ProjectName
        {
            get
            {
                return this.comboBox_projectName.Text;
            }
            set
            {
                this.comboBox_projectName.Text = value;
            }
        }

        /// <summary>
        /// 发起查重的记录路径。id可以为?。主要用来模拟出keys
        /// </summary>
        public string RecordPath
        {
            get
            {
                return this.textBox_recordPath.Text;
            }
            set
            {
                this.textBox_recordPath.Text = value;
                this.Text = "查重: " + value;
            }
        }

        /// <summary>
        /// 发起查重的XML记录
        /// </summary>
        public string XmlRecord
        {
            get
            {
                return m_strXmlRecord;
            }
            set
            {
                m_strXmlRecord = value;
            }
        }

        /// <summary>
        /// 获得查重结果：所命中的权值超过阈值的记录路径的集合
        /// </summary>
        public string[] DupPaths
        {
            get
            {
                int i;
                List<string> aPath = new List<string>();
                for (i = 0; i < this.listView_browse.Items.Count; i++)
                {
                    ListViewItem item = this.listView_browse.Items[i];

                    if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                    {
                        aPath.Add(item.Text);
                    }
                    else
                        break;  // 假定超过阈值的事项都在前部，这里可以优化中断
                }

                if (aPath.Count == 0)
                    return new string[0];

                string[] result = new string[aPath.Count];
                aPath.CopyTo(result);

                return result;
            }
        }

        #endregion

        #region ISearchForm 接口函数

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

        public string CurrentProtocol
        {
            get
            {
                return "dp2library";
            }
        }

        public string CurrentResultsetPath
        {
            get
            {
                return this.LibraryServerName
                    + "/" + this.RecordPath
                    + "/" + "searchdup"
                    + "/default";
            }
        }

        // 刷新一条MARC记录
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError)
        {
            strError = "尚未实现";

            return -2;
        }


        // 删除一条MARC/XML记录
        // parameters:
        //      strSavePath 内容为"中文图书/1@本地服务器"。没有协议名部分。
        // return:
        //      -1  error
        //      0   suceed
        public int DeleteOneRecord(
            string strSavePath,
            byte[] baTimestamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "尚未实现";

            return -1;

        }

        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            return 0;
        }

        // 获得一条MARC/XML记录
        // return:
        //      -1  error 包括not found
        //      0   found
        //      1   为诊断记录
        public int GetOneRecord(
            string strStyle,
            int nTest,
            string strPathParam,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strRecord,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.Z3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo,
            out string strError)
        {
            strXmlFragment = "";
            strRecord = "";
            record = null;
            strError = "";
            currrentEncoding = this.CurrentEncoding;
            baTimestamp = null;
            strSavePath = "";
            strOutStyle = "marc";
            logininfo = new LoginInfo();
            lVersion = 0;

            // 防止重入
            if (m_bInSearch == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法获得记录。请稍后再试。";
                return -1;
            }

            if (strStyle != "marc" && strStyle != "xml")
            {
                strError = "DupForm只支持获取MARC格式记录和xml格式记录，不支持 '" + strStyle + "' 格式的记录";
                return -1;
            }
            int nRet = 0;

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            if (index == -1)
            {
                strError = "暂时不支持没有 index 的用法";
                return -1;
            }

            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);

            if (index >= this.listView_browse.Items.Count)
            {
                strError = "越过结果集尾部";
                return -1;
            }
            ListViewItem curItem = this.listView_browse.Items[index];

            if (bHilightBrowseLine == true)
            {
                // 修改listview中事项的选定状态
                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                {
                    this.listView_browse.SelectedItems[i].Selected = false;
                }

                curItem.Selected = true;
                curItem.EnsureVisible();
            }

            string strPurePath = curItem.Text;
            string strServerName = this.LibraryServerName;

            strPath = strPurePath + "@" + this.LibraryServerName;

            strSavePath = this.CurrentProtocol + ":" + strPath;

            // 拉上一个dp2检索窗，好办事
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                strError = "没有打开的dp2检索窗，无法GetOneRecordSyntax()";
                return -1;
            }

            // 获得server url
            string strServerUrl = dp2_searchform.GetServerUrl(strServerName);
            if (strServerUrl == null)
            {
                strError = "没有找到服务器名 '" + strServerName + "' 对应的URL";
                return -1;
            }

            this.Channel = this.Channels.GetChannel(strServerUrl);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                stop.SetMessage("正在装入书目记录 " + strPath + " ...");

                string[] formats = null;
                formats = new string[1];
                formats[0] = "xml";

                string[] results = null;

                long lRet = Channel.GetBiblioInfos(
                    stop,
                    strPurePath,
                    "",
                    formats,
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    strError = "路径为 '" + strPath + "' 的书目记录没有找到 ...";
                    goto ERROR1;   // not found
                }

                if (lRet == -1)
                    goto ERROR1;

                // this.BiblioTimestamp = baTimestamp;

                if (results == null)
                {
                    strError = "results == null";
                    goto ERROR1;
                }
                if (results.Length != formats.Length)
                {
                    strError = "result.Length != formats.Length";
                    goto ERROR1;
                }

                string strXml = results[0];

                if (strStyle == "marc")
                {
                    string strMarcSyntax = "";
                    string strOutMarcSyntax = "";
                    // 从数据记录中获得MARC格式
                    nRet = MarcUtil.Xml2Marc(strXml,
                        true,
                        strMarcSyntax,
                        out strOutMarcSyntax,
                        out strRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }


                    // 获得书目以外的其它XML片断
                    nRet = dp2SearchForm.GetXmlFragment(strXml,
            out strXmlFragment,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strRecord = strXml;
                    strOutStyle = strStyle;
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        public dp2DupForm()
        {
            InitializeComponent();
        }

        private void DupForm_Load(object sender, EventArgs e)
        {
            this.Channels = new LibraryChannelCollection();
            this.Channels.BeforeLogin += new BeforeLoginEventHandle(Channels_BeforeLogin);
            this.Channels.AfterLogin += new AfterLoginEventHandle(Channels_AfterLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            this.checkBox_includeLowCols.Checked = this.MainForm.AppInfo.GetBoolean(
    "dup_form",
    "include_low_cols",
    true);
            this.checkBox_returnAllRecords.Checked = this.MainForm.AppInfo.GetBoolean(
    "dup_form",
    "return_all_records",
    true);
            if (String.IsNullOrEmpty(this.comboBox_projectName.Text) == true)
            {
                this.comboBox_projectName.Text = this.MainForm.AppInfo.GetString(
                        "dup_form",
                        "projectname",
                        "");
            }

            string strWidths = this.MainForm.AppInfo.GetString(
"dup_form",
"browse_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                    strWidths,
                    true);
            }

            // 自动启动查重
            if (this.AutoBeginSearch == true)
            {
                API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
            }
        }

        void Channels_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            dp2Server server = this.MainForm.Servers[channel.Url];
            if (server == null)
            {
                // e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                return;
            }

#if SN
            if (server.Verified == false && StringUtil.IsInList("serverlicensed", channel.Rights) == false)
            {
                string strError = "";
                string strTitle = "查重窗需要先设置序列号才能访问服务器 " + server.Name + " " + server.Url;
                int nRet = this.MainForm.VerifySerialCode(strTitle,
                    "", 
                    true,
                    out strError);
                if (nRet == -1)
                {
                    channel.Close();
                    e.ErrorInfo = strTitle;
#if NO
                    MessageBox.Show(this.MainForm, "查重窗需要先设置序列号才能使用");
                    API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
#endif
                    return;
                }
            }
#endif

            server.Verified = true;
        }

        void Channels_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            dp2Server server = this.MainForm.Servers[channel.Url];
            if (server == null)
            {
                e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                e.Failed = true;
                e.Cancel = true;
                return;
            }

            if (e.FirstTry == true)
            {
                e.UserName = server.DefaultUserName;
                e.Password = server.DefaultPassword;
                e.Parameters = "location=dp2Catalog,type=worker";
                /*
                e.IsReader = false;
                e.Location = "dp2Catalog";
                 * */
                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = this.MainForm.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

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


            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = false;
            e.Parameters = "location=dp2Catalog,type=worker";
            /*
            e.IsReader = false;
            e.Location = "dp2Catalog";
             * */
            e.SavePasswordLong = true;
            e.LibraryServerUrl = dlg.ServerUrl;
        }

        ServerDlg SetDefaultAccount(
    string strServerUrl,
    string strTitle,
    string strComment,
    IWin32Window owner)
        {
            dp2Server server = this.MainForm.Servers[strServerUrl];

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

            this.MainForm.AppInfo.LinkFormState(dlg,
                "dp2_logindlg_state");

            dlg.ShowDialog(owner);

            this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            server.DefaultUserName = dlg.UserName;
            server.DefaultPassword =
                (dlg.SavePassword == true) ?
                dlg.Password : "";

            server.SavePassword = dlg.SavePassword;

            server.Url = dlg.ServerUrl;
            return dlg;
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        private void dp2DupForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void DupForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetBoolean(
    "dup_form",
    "include_low_cols",
    this.checkBox_includeLowCols.Checked);
                this.MainForm.AppInfo.SetBoolean(
        "dup_form",
        "return_all_records",
        this.checkBox_returnAllRecords.Checked);

                this.MainForm.AppInfo.SetString(
                    "dup_form",
                    "projectname",
                    this.comboBox_projectName.Text);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
                this.MainForm.AppInfo.SetString(
                    "dup_form",
                    "browse_list_column_width",
                    strWidths);
            }

            this.Channels.BeforeLogin -= new BeforeLoginEventHandle(Channels_BeforeLogin);

            EventFinish.Set();
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INITIAL:
                    {
                        this.DoSearchDup();
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        public void DoSearchDup()
        {
            string strError = "";
            string strUsedProjectName = "";

            this.EventFinish.Reset();

            this.m_bInSearch = true;
            try
            {

                int nRet = DoSearch(this.comboBox_projectName.Text,
                    this.textBox_recordPath.Text,
                    this.XmlRecord,
                    out strUsedProjectName,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                if (String.IsNullOrEmpty(strUsedProjectName) == false)
                    this.ProjectName = strUsedProjectName;
            }
            finally
            {
                this.EventFinish.Set();

                this.m_bInSearch = false;
            }
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            DoSearchDup();
        }

        void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            this.button_findServerName.Enabled = bEnable;

            this.button_viewXmlRecord.Enabled = bEnable;

            this.comboBox_projectName.Enabled = bEnable;
            this.textBox_recordPath.Enabled = bEnable;

            this.textBox_serverName.Enabled = bEnable;
        }

        // 图书馆服务器名
        public string LibraryServerName
        {
            get
            {
                return this.textBox_serverName.Text;
            }
            set
            {
                this.textBox_serverName.Text = value;
            }
        }

        // 检索
        // return:
        //      -1  error
        //      0   succeed
        public int DoSearch(string strProjectName,
            string strRecPath,
            string strXml,
            out string strUsedProjectName,
            out string strError)
        {
            strError = "";
            strUsedProjectName = "";

            if (strProjectName == "<默认>"
                || strProjectName == "<default>")
                strProjectName = "";

            EventFinish.Reset();

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行查重 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                this.ClearDupState(true);
                this.listView_browse.Items.Clear();

                // 获得server url
                if (String.IsNullOrEmpty(this.LibraryServerName) == true)
                {
                    strError = "尚未指定服务器名";
                    goto ERROR1;
                }
                dp2Server server = this.MainForm.Servers.GetServerByName(this.LibraryServerName);
                if (server == null)
                {
                    strError = "服务器名为 '" + this.LibraryServerName + "' 的服务器不存在...";
                    goto ERROR1;
                }

                this.SortColumns.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_browse.Columns);

                string strBrowseStyle = "cols";
                if (this.checkBox_includeLowCols.Checked == false)
                    strBrowseStyle += ",excludecolsoflowthreshold";

                string strServerUrl = server.Url;

                this.Channel = this.Channels.GetChannel(strServerUrl);

                long lRet = Channel.SearchDup(
                    stop,
                    strRecPath,
                    strXml,
                    strProjectName,
                    "includeoriginrecord", // includeoriginrecord
                    out strUsedProjectName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                if (lHitCount == 0)
                    goto END1;   // 查重发现没有命中


                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    DupSearchResult[] searchresults = null;

                    lRet = Channel.GetDupSearchResult(
                        stop,
                        lStart,
                        lPerCount,
                        strBrowseStyle, // "cols,excludecolsoflowthreshold",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                        break;

                    Debug.Assert(searchresults != null, "");

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DupSearchResult result = searchresults[i];

                        ListViewUtil.EnsureColumns(this.listView_browse,
    2 + (result.Cols == null ? 0 : result.Cols.Length),
    200);

                        if (this.checkBox_returnAllRecords.Checked == false)
                        {
                            // 遇到第一个权值较低的，就中断全部获取浏览过程
                            if (result.Weight < result.Threshold)
                                goto END1;
                        }

                        /*
                        if (result.Cols == null)
                        {
                            strError = "返回的结果行错误 result.Cols == null";
                            goto ERROR1;
                        }

                        ListViewUtil.EnsureColumns(this.listView_browse,
                            2 + result.Cols.Length,
                            200);
                         * */

                        ListViewItem item = new ListViewItem();
                        item.Text = result.Path;
                        item.SubItems.Add(result.Weight.ToString());
                        if (result.Cols != null)
                        {
                            for (int j = 0; j < result.Cols.Length; j++)
                            {
                                item.SubItems.Add(result.Cols[j]);
                            }
                        }
                        this.listView_browse.Items.Add(item);

                        if (item.Text == this.RecordPath)
                        {
                            // 如果就是发起记录自己  2008/2/29
                            item.ImageIndex = ITEMTYPE_OVERTHRESHOLD;
                            item.BackColor = Color.LightGoldenrodYellow;
                            item.ForeColor = SystemColors.GrayText; // 表示就是发起记录自己
                        }
                        else if (result.Weight >= result.Threshold)
                        {
                            item.ImageIndex = ITEMTYPE_OVERTHRESHOLD;
                            item.BackColor = Color.LightYellow;
                            item.Font = new Font(item.Font, FontStyle.Bold);
                        }
                        else
                        {
                            item.ImageIndex = ITEMTYPE_NORMAL;
                        }

                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;

                }

            END1:
                this.SetDupState();

                return (int)lHitCount;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EventFinish.Set();

                EnableControls(true);
            }


        ERROR1:
            return -1;
        }

        private void comboBox_projectName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_projectName.Items.Count > 0)
                return;

            string strError = "";
            int nRet = 0;

            string[] projectnames = null;
            // 列出可用的查重方案名
            nRet = ListProjectNames(this.RecordPath,
                out projectnames,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            for (int i = 0; i < projectnames.Length; i++)
            {
                this.comboBox_projectName.Items.Add(projectnames[i]);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 列出可用的查重方案名
        public int ListProjectNames(string strPureRecPath,
            out string[] projectnames,
            out string strError)
        {
            strError = "";
            projectnames = null;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取可用的查重方案名 ...");
            stop.BeginLoop();

            try
            {

                // 获得server url
                if (String.IsNullOrEmpty(this.LibraryServerName) == true)
                {
                    strError = "尚未指定服务器名";
                    goto ERROR1;
                }
                dp2Server server = this.MainForm.Servers.GetServerByName(this.LibraryServerName);
                if (server == null)
                {
                    strError = "服务器名为 '" + this.LibraryServerName + "' 的服务器不存在...";
                    goto ERROR1;
                }

                string strServerUrl = server.Url;

                this.Channel = this.Channels.GetChannel(strServerUrl);



                DupProjectInfo[] dpis = null;

                string strBiblioDbName = dp2SearchForm.GetDbName(strPureRecPath);

                long lRet = Channel.ListDupProjectInfos(
                    stop,
                    strBiblioDbName,
                    out dpis,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                projectnames = new string[dpis.Length];
                for (int i = 0; i < projectnames.Length; i++)
                {
                    projectnames[i] = dpis[i].Name;
                }

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }


        ERROR1:
            return -1;
        }

        private void textBox_recordPath_TextChanged(object sender, EventArgs e)
        {
            // 记录路径会影响到方案名列表
            // 修改记录路径的时候，迫使方案名下拉列表清空，这样当用到下拉列表的时候会自动去获取新内容
            this.comboBox_projectName.Items.Clear();

        }

        /// <summary>
        /// 等待检索结束
        /// </summary>
        public void WaitSearchFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }

        private void button_viewXmlRecord_Click(object sender, EventArgs e)
        {
            XmlViewerForm dlg = new XmlViewerForm();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "当前XML数据";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = this.XmlRecord;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog();
            return;
        }

        void ClearDupState(bool bSearching)
        {
            if (bSearching == true)
                this.label_dupMessage.Text = "正在查重...";
            else
                this.label_dupMessage.Text = "尚未查重";
        }

        // 设置查重状态
        void SetDupState()
        {
            int nCount = 0;
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];

                if (item.Text == this.RecordPath)
                    continue;   // 不包含发起记录自己 2008/2/29


                if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                    nCount++;
                else
                    break;  // 假定超过权值的事项都在前部，一旦发现一个不是的事项，循环就结束
            }

            if (nCount > 0)
                this.label_dupMessage.Text = "有 " + Convert.ToString(nCount) + " 条重复记录。";
            else
                this.label_dupMessage.Text = "没有重复记录。";

        }

        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            /*
            string strPurePath = this.listView_browse.SelectedItems[0].SubItems[0].Text;

            EntityForm form = new EntityForm();

            form.MdiParent = this.MainForm;

            form.MainForm = this.MainForm;
            form.Show();
            form.LoadRecord(strPath);
             * */
            int nIndex = -1;
            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                {
                    MessageBox.Show(this, "尚未选定要装入详细窗的事项");
                    return;
                }
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex);
        }

        void LoadDetail(int index)
        {
            // 取出记录路径，析出书目库名，然后看这个书目库的syntax
            // 可能装入MARC和DC两种不同的窗口
            string strError = "";

            // 防止重入
            if (m_bInSearch == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法装载记录。请稍后再试。";
                goto ERROR1;
            }

            string strSyntax = "";
            int nRet = GetOneRecordSyntax(index,
                out strSyntax,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strSyntax == "" // default = unimarc
                || strSyntax.ToLower() == "unimarc"
                || strSyntax.ToLower() == "usmarc")
            {

                MarcDetailForm form = new MarcDetailForm();

                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;

                // MARC Syntax OID
                // 需要建立数据库配置参数，从中得到MARC格式
                //// form.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.1";   // UNIMARC

                form.Show();

                form.LoadRecord(this, index);
            }
            else if (strSyntax.ToLower() == "dc")
            {

                DcForm form = new DcForm();

                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;

                form.Show();

                form.LoadRecord(this, index);
            }
            else
            {
                strError = "未知的syntax '" + strSyntax + "'";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = this.MainForm.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // 新开一个dp2检索窗
                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this.MainForm;
                dp2_searchform.MdiParent = this.MainForm;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();
            }


            return dp2_searchform;
        }

        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetOneRecordSyntax(int index,
            out string strSyntax,
            out string strError)
        {
            strError = "";
            strSyntax = "";

            if (index >= this.listView_browse.Items.Count)
            {
                strError = "越过结果集尾部";
                return -1;
            }

            ListViewItem curItem = this.listView_browse.Items[index];

            string strServerName = this.LibraryServerName;
            string strPurePath = curItem.Text;

            // 拉上一个dp2检索窗，好办事
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                strError = "没有打开的dp2检索窗，无法GetOneRecordSyntax()";
                return -1;
            }

            /*
            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            string strServerUrl = server.Url;
             * */
            string strBiblioDbName = dp2SearchForm.GetDbName(strPurePath);

            // 获得一个数据库的数据syntax
            // parameters:
            //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
            //              如果==null，表示会自动使用this.stop，并自动OnStop+=
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = dp2_searchform.GetDbSyntax(
                null,
                strServerName,
                strBiblioDbName,
                out strSyntax,
                out strError);

            if (nRet == -1)
                return -1;

            return nRet;
        }

        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_browse.Columns,
                true);

            // 排序
            this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_browse.ListViewItemSorter = null;
        }

        private void button_findServerName_Click(object sender, EventArgs e)
        {
            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请指定一个作为查重目标的 dp2library 服务器";
            dlg.dp2Channels = this.Channels;
            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_SERVER };
            dlg.Path = this.textBox_serverName.Text;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_serverName.Text = dlg.Path;
        }

        private void DupForm_Activated(object sender, EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            /*
            // 菜单
            MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
            MainForm.MenuItem_font.Enabled = true;

            // 工具条按钮
            MainForm.toolButton_search.Enabled = false;
            MainForm.toolButton_prev.Enabled = true;
            MainForm.toolButton_next.Enabled = true;
            MainForm.toolButton_nextBatch.Enabled = false;

            MainForm.toolButton_getAllRecords.Enabled = false;
            MainForm.toolButton_saveTo.Enabled = true;
            MainForm.toolButton_save.Enabled = true;
            MainForm.toolButton_delete.Enabled = true;

            MainForm.toolButton_loadTemplate.Enabled = true;
             * */

            MainForm.toolButton_dup.Enabled = false;
            MainForm.toolButton_verify.Enabled = false;
        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_browse.SelectedItems.Count == 1)
            {
                ListViewItem item = this.listView_browse.SelectedItems[0];
                int nLineNo = this.listView_browse.SelectedIndices[0] + 1;
                if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                {
                    if (item.Text == this.RecordPath)
                    {
                        this.label_message.Text = "序号 " + nLineNo.ToString() + ": 发起查重的记录(自己)";
                    }
                    else
                    {
                        this.label_message.Text = "序号 " + nLineNo.ToString() + ": 重复的记录";
                    }
                }
                else
                {
                    this.label_message.Text = "序号 " + nLineNo.ToString();
                }
            }
            else
            {
                this.label_message.Text = "";
            }

            // 装入(未装入的)浏览列
            if (this.listView_browse.SelectedItems.Count > 0)
            {
                List<string> pathlist = new List<string>();
                List<ListViewItem> itemlist = new List<ListViewItem>();
                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                {
                    ListViewItem item = this.listView_browse.SelectedItems[i];
                    string strFirstCol = ListViewUtil.GetItemText(item, 2);
                    if (string.IsNullOrEmpty(strFirstCol) == false)
                        continue;
                    pathlist.Add(item.Text);
                    itemlist.Add(item);
                }

                if (pathlist.Count > 0)
                {
                    string strError = "";
                    int nRet = GetBrowseCols(pathlist,
                        itemlist,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
            }

        }

        private void listView_browse_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("填充全部浏览列(&F)");
            menuItem.Click += new System.EventHandler(this.menu_fillBrowseCols_Click);
            /*
            if (this.listView_browse.SelectedItems.Count == 0)
                menuItem.Enabled = false;
             * */
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_browse, new Point(e.X, e.Y));
        }

        void menu_fillBrowseCols_Click(object sender, EventArgs e)
        {
            List<string> pathlist = new List<string>();
            List<ListViewItem> itemlist = new List<ListViewItem>();
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];
                string strFirstCol = ListViewUtil.GetItemText(item, 2);
                if (string.IsNullOrEmpty(strFirstCol) == false)
                    continue;
                pathlist.Add(item.Text);
                itemlist.Add(item);
            }

            if (pathlist.Count > 0)
            {
                string strError = "";
                int nRet = GetBrowseCols(pathlist,
                    itemlist,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }
        }

        int GetBrowseCols(List<string> pathlist,
            List<ListViewItem> itemlist,
            out string strError)
        {
            strError = "";

            // 获得server url
            if (String.IsNullOrEmpty(this.LibraryServerName) == true)
            {
                strError = "尚未指定服务器名";
                return -1;
            }
            dp2Server server = this.MainForm.Servers.GetServerByName(this.LibraryServerName);
            if (server == null)
            {
                strError = "服务器名为 '" + this.LibraryServerName + "' 的服务器不存在...";
                return -1;
            }

            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在填充浏览列 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                int nStart = 0;
                int nCount = 0;
                for (; ; )
                {
                    nCount = pathlist.Count - nStart;
                    if (nCount > 100)
                        nCount = 100;
                    if (nCount <= 0)
                        break;

                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (nStart + 1).ToString() + " - " + (nStart + nCount).ToString());

                    string[] paths = new string[nCount];
                    pathlist.CopyTo(nStart, paths, 0, nCount);

                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                    long lRet = this.Channel.GetBrowseRecords(
                        this.stop,
                        paths,
                        "id,cols",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (searchresults == null || searchresults.Length == 0)
                    {
                        strError = "searchresults == null || searchresults.Length == 0";
                        return -1;
                    }

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.CirculationClient.localhost.Record record = searchresults[i];

                        ListViewUtil.EnsureColumns(this.listView_browse,
                            2 + (record.Cols == null ? 0 : record.Cols.Length),
                            200);

                        ListViewItem item = itemlist[nStart + i];
                        item.Text = record.Path;
                        if (record.Cols != null)
                        {
                            for (int j = 0; j < record.Cols.Length; j++)
                            {
                                item.SubItems.Add(record.Cols[j]);
                            }
                        }
                    }


                    nStart += searchresults.Length;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 0;
        }


    }
}