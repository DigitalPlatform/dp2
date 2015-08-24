using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using System.Collections;
using System.Web;
using DigitalPlatform.IO;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Script;


using System.Reflection;
using Microsoft.Win32;
using DigitalPlatform.dp2.Statis;
// using DocumentFormat.OpenXml.Packaging;


namespace dp2Catalog
{
    public partial class dp2SearchForm : Form, ISearchForm
    {
        public string ExportRecPathFilename = "";
        // 最近使用过的记录路径文件名
        string m_strUsedRecPathFilename = "";

        Commander commander = null;
        BiblioViewerForm m_commentViewer = null;

        Hashtable m_biblioTable = new Hashtable(); // 书目记录路径 --> 书目信息
        int m_nChangedCount = 0;
        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();
        public string BinDir = "";

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        bool m_bInSearching = false;

        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

        public LibraryChannelCollection Channels = null;
        internal LibraryChannel Channel = null;

        public string Lang = "zh";

        const int WM_SELECT_INDEX_CHANGED = API.WM_USER + 200;
        const int WM_LOADSIZE = API.WM_USER + 201;
        const int WM_INITIAL_FOCUS = API.WM_USER + 202;

        // 当前缺省的编码方式
        Encoding CurrentEncoding = Encoding.UTF8;

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventLoadFinish = new AutoResetEvent(false);


        public dp2SearchForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_browse.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.ParsePath -= new ParsePathEventHandler(prop_ParsePath);
            prop.ParsePath += new ParsePathEventHandler(prop_ParsePath);


        }

        void prop_ParsePath(object sender, ParsePathEventArgs e)
        {
            string strServerName = "";
            string strPurePath = "";
            // 解析记录路径。
            // 记录路径为如下形态 "中文图书/1 @服务器"
            ParseRecPath(e.Path,
                out strServerName,
                out strPurePath);

            e.DbName = strServerName + "|" + GetDbName(strPurePath);
        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (this._linkMarcFile != null
                || StringUtil.HasHead(e.DbName, "mem|") == true
                || StringUtil.HasHead(e.DbName, "file|") == true)
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("题名");
                e.ColumnTitles.Add("责任者");
                e.ColumnTitles.Add("出版社");
                e.ColumnTitles.Add("出版日期");
                return;
            }

            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                return;
            }

            // e.ColumnTitles = this.MainForm.GetBrowseColumnNames(e.DbName);

            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection titles = this.GetBrowseColumnNames(e.DbName);
            if (titles == null) // 意外的数据库名
                return;
            e.ColumnTitles.AddRange(titles);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件

            /*
            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");
             * */

        }

        ColumnPropertyCollection GetBrowseColumnNames(string strPrefix)
        {
            string [] parts = strPrefix.Split(new char[] {'|'});
            if (parts.Length < 2)
            {
                // return new ColumnPropertyCollection();
                return null;
            }

            return this.dp2ResTree1.GetBrowseColumnNames(parts[0], parts[1]);
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_browse.Tag;
            prop.ClearCache();
        }

        public void RefreshResTree()
        {
            if (this.dp2ResTree1 != null)
                this.dp2ResTree1.Refresh(dp2ResTree.RefreshStyle.All);
        }

        private void dp2SearchForm_Load(object sender, EventArgs e)
        {
#if NO
            if (this.MainForm.TestMode == true)
            {
                MessageBox.Show(this.MainForm, "dp2 检索窗需要先设置序列号(正式模式)才能使用");
                API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
                return;
            }
#endif

#if SN

            // 检查序列号
            // DateTime start_day = new DateTime(2014, 11, 15);    // 2014/11/15 以后强制启用序列号功能
            // if (DateTime.Now >= start_day || this.MainForm.IsExistsSerialNumberStatusFile() == true)
            {
                // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                this.MainForm.WriteSerialNumberStatusFile();

                string strError = "";
                int nRet = this.MainForm.VerifySerialCode("dp2 检索窗需要先设置序列号才能使用",
                    "",
                    false,
                    out strError);
                if (nRet == -1)
                {
#if NO
                    MessageBox.Show(this.MainForm, "dp2 检索窗需要先设置序列号才能使用");
                    API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
                    return;
#endif
                }
                else
                {
                    // 为全部服务器设置 verified 标志
                    this.MainForm.Servers.SetAllVerified(true);
                }
            }
#else
            // 为全部服务器设置 verified 标志
            this.MainForm.Servers.SetAllVerified(true);
#endif

            //
            EventLoadFinish.Reset();
            this.BinDir = Environment.CurrentDirectory;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            SetLayout(this.LayoutName);

            this.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);

            LoadSize();


            this.Channels = new LibraryChannelCollection();
            this.Channels.BeforeLogin += new BeforeLoginEventHandle(Channels_BeforeLogin);
            this.Channels.AfterLogin += new AfterLoginEventHandle(Channels_AfterLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            //
            this.dp2ResTree1.TestMode = this.MainForm.TestMode;

            this.dp2ResTree1.stopManager = MainForm.stopManager;

            this.dp2ResTree1.Servers = MainForm.Servers;	// 引用

            this.dp2ResTree1.Channels = this.Channels;	// 引用

            this.dp2ResTree1.cfgCache = this.MainForm.cfgCache;

            string strSortTables = this.MainForm.AppInfo.GetString(
               "dp2_search",
               "sort_tables",
               ""); 
            this.dp2ResTree1.sort_tables = dp2ResTree.RestoreSortTables(strSortTables);

            this.dp2ResTree1.CheckBoxes = this.MainForm.AppInfo.GetBoolean(
               "dp2_search",
               "enable_checkboxes",
               false); 

            this.dp2ResTree1.Fill(null);

            this.textBox_simple_queryWord.Text = this.MainForm.AppInfo.GetString(
                "dp2_search_simple_query",
                "word",
                "");
            this.comboBox_simple_matchStyle.Text = this.MainForm.AppInfo.GetString(
    "dp2_search_simple_query",
    "matchstyle",
    "前方一致");

            this.textBox_mutiline_queryContent.Text = this.MainForm.AppInfo.GetString(
                "dp2_search_muline_query",
                "content",
                "");
            this.comboBox_multiline_matchStyle.Text = this.MainForm.AppInfo.GetString(
"dp2_search_muline_query",
"matchstyle",
"前方一致");

            string strWidths = this.MainForm.AppInfo.GetString(
    "dp2searchform",
    "record_list_column_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                    strWidths,
                    true);
            }

            string strSaveString = this.MainForm.AppInfo.GetString(
    "dp2searchform",
    "query_lines",
    "^^^");
            this.dp2QueryControl1.Restore(strSaveString);
            /*
            for (int i = 0; i < nQueryLineCount; i++)
            {
                this.dp2QueryControl1.AddLine();
            }
             * */


            // API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            // 按照上次保存的路径展开resdircontrol树
            string strResDirPath = this.MainForm.AppInfo.GetString(
                "dp2_search_simple_query",
                "resdirpath",
                "");
            if (strResDirPath != null)
            {
                this.Update();

                object[] pList = { strResDirPath };

                this.BeginInvoke(new Delegate_ExpandResDir(ExpandResDir),
                    pList);
            }
            else
            {
                this.EventLoadFinish.Set();
            }

            comboBox_matchStyle_TextChanged(null, null);
        }

        void Channels_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            dp2Server server = this.MainForm.Servers[channel.Url];
            if (server == null)
            {
                e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                return;
            }

#if SN
            if (server.Verified == false && StringUtil.IsInList("serverlicensed", channel.Rights) == false)
            {
                string strError = "";
                string strTitle = "dp2 检索窗需要先设置序列号才能访问服务器 " + server.Name + " " + server.Url;
                int nRet = this.MainForm.VerifySerialCode(strTitle, 
                    "",
                    true,
                    out strError);
                if (nRet == -1)
                {
                    channel.Close();
                    e.ErrorInfo = strTitle;
#if NO
                    MessageBox.Show(this.MainForm, "dp2 检索窗需要先设置序列号才能使用");
                    MainForm.AppInfo.SetString(
    "dp2_search_simple_query",
    "resdirpath",
    "");
                    API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
#endif
                    return;
                }
            }
#endif
            server.Verified = true;
        }


        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_nInViewing > 0;
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

                // 2014/11/10
                if (this.MainForm.TestMode == true)
                    e.Parameters += ",testmode=true";

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

            // 2014/11/10
            e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
            {
                // 从序列号中获得 expire= 参数值
                string strExpire = this.MainForm.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
            }
#endif

            // 2014/11/10
            if (this.MainForm.TestMode == true)
                e.Parameters += ",testmode=true";

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
            this.Activate();    // 让 MDI 子窗口翻出来到前面
            dlg.ShowDialog(owner);

            this.MainForm.AppInfo.UnlinkFormState(dlg);


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
                this.MainForm.Servers.Changed = true;

            return dlg;
        }


        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                    /*
                case WM_LOADSIZE:
                    LoadSize();
                    return;
                     * */
                case WM_INITIAL_FOCUS:
                    this.textBox_simple_queryWord.Focus();
                    return;
                case WM_SELECT_INDEX_CHANGED:
                    {
#if NO
                        if (this.listView_records.SelectedIndices.Count == 0)
                            this.label_message.Text = "";
                        else
                        {
                            if (this.listView_records.SelectedIndices.Count == 1)
                            {
                                this.label_message.Text = "第 " + (this.listView_records.SelectedIndices[0] + 1).ToString() + " 行";
                            }
                            else
                            {
                                this.label_message.Text = "从 " + (this.listView_records.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_records.SelectedIndices.Count.ToString() + " 个事项";
                            }
                        }
#endif
                        // 菜单动态变化
                        if (this.listView_browse.SelectedItems.Count == 0)
                        {
                            MainForm.toolButton_saveTo.Enabled = false;
                            MainForm.toolButton_delete.Enabled = false;

                            MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                            MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;

                            MainForm.StatusBarMessage = "";
                        }
                        else
                        {
                            MainForm.toolButton_saveTo.Enabled = true;
                            MainForm.toolButton_delete.Enabled = true;

                            MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                            MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;

                            if (this.listView_browse.SelectedItems.Count == 1)
                            {
                                MainForm.StatusBarMessage = "第 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行";
                            }
                            else
                            {
                                MainForm.StatusBarMessage = "从 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_browse.SelectedItems.Count.ToString() + " 个事项";
                            }
                        }

                            ListViewUtil.OnSeletedIndexChanged(this.listView_browse,
                                0,
                                null);

                        if (this.m_biblioTable != null)
                        {
                            if (CanCallNew(commander, m.Msg) == true)
                                DoViewComment(false);
                        }
                    }
                    return;

            }
            base.DefWndProc(ref m);
        }

        public bool CanCallNew(Commander commander, int msg)
        {
            if (this.m_nInViewing > 0)
            {
                // 缓兵之计
                // this.Stop();
                commander.AddMessage(msg);
                return false;   // 还不能启动
            }

            return true;    // 可以启动
        }

        public void AppInfo_LoadMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;



#if NO
            // 获得splitContainer_main的状态
            int nValue = MainForm.AppInfo.GetInt(
            "dp2searchform",
            "splitContainer_main",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_main.SplitterDistance = nValue;
                }
                catch
                {
                }
            }

            // 获得splitContainer_up的状态
            nValue = MainForm.AppInfo.GetInt(
            "dp2searchform",
            "splitContainer_up",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_up.SplitterDistance = nValue;
                }
                catch
                {
                }
            }

            /*
            // 获得splitContainer_queryAndResultInfo的状态
            nValue = MainForm.AppInfo.GetInt(
            "dp2searchform",
            "splitContainer_queryAndResultInfo",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_queryAndResultInfo.SplitterDistance = nValue;
                }
                catch
                {
                }
            }
            */
#endif

            try
            {
                // 获得splitContainer_main的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_main,
                    "dp2searchform",
                    "splitContainer_main");

                // 获得splitContainer_up的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_up,
                    "dp2searchform",
                    "splitContainer_up");

                // 获得splitContainer_queryAndResultInfo的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_queryAndResultInfo,
                    "dp2searchform",
                    "splitContainer_queryAndResultInfo");
            }
            catch
            {
            }
        }

        void AppInfo_SaveMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

#if NO
            // 保存splitContainer_main的状态
            MainForm.AppInfo.SetInt(
                "dp2searchform",
                "splitContainer_main",
                this.splitContainer_main.SplitterDistance);
            // 保存splitContainer_up的状态
            MainForm.AppInfo.SetInt(
                "dp2searchform",
                "splitContainer_up",
                this.splitContainer_up.SplitterDistance);
            /*
            // 保存splitContainer_queryAndResultInfo的状态
            MainForm.AppInfo.SetInt(
                "dp2searchform",
                "splitContainer_queryAndResultInfo",
                this.splitContainer_queryAndResultInfo.SplitterDistance);
             * */
#endif

            // 分割条位置
            // 保存splitContainer_main的状态
            this.MainForm.SaveSplitterPos(
                this.splitContainer_main,
                "dp2searchform",
                "splitContainer_main");
            // 保存splitContainer_up的状态
            this.MainForm.SaveSplitterPos(
                this.splitContainer_up,
                "dp2searchform",
                "splitContainer_up");
            // 保存splitContainer_queryAndResultInfo的状态
            this.MainForm.SaveSplitterPos(
                this.splitContainer_queryAndResultInfo,
                "dp2searchform",
                "splitContainer_queryAndResultInfo");

        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");

            /*
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "dp2_search_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);
            */
        }

        public void SaveSize()
        {
            /*
            MainForm.AppInfo.SaveMdiChildFormStates(this,
                "dp2_search_state");
            */
            if (this.WindowState != FormWindowState.Minimized)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
        "mdi_form_state");
            }
        }

        private void dp2SearchForm_FormClosing(object sender, FormClosingEventArgs e)
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

            if (this.m_nChangedCount > 0)
            {

                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 " + m_nChangedCount + " 项修改尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "dp2SearchForm",
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

        private void dp2SearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.commander != null)
                this.commander.Destroy();

            if (this.dp2ResTree1 != null)
                this.dp2ResTree1.Stop();

            if (stop != null) // 脱离关联
            {
                stop.Style = StopStyle.None;    // 需要强制中断
                stop.DoStop();

                stop.Unregister();	// 和容器关联
                stop = null;
            }

            // 保存前恢复简单检索面板，便于保存分割条的位置
            this.tabControl_query.SelectedTab = this.tabPage_simple;

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SetString(
                    "dp2_search_simple_query",
                    "word",
                    this.textBox_simple_queryWord.Text);
                this.MainForm.AppInfo.SetString(
    "dp2_search_simple_query",
    "matchstyle",
    this.comboBox_simple_matchStyle.Text);


                this.MainForm.AppInfo.SetString(
                    "dp2_search_muline_query",
                    "content",
                    StringUtil.GetSomeLines(this.textBox_mutiline_queryContent.Text, 100)
                    );

                this.MainForm.AppInfo.SetString(
    "dp2_search_muline_query",
    "matchstyle",
    this.comboBox_multiline_matchStyle.Text);


                // 保存resdircontrol最后的选择
                ResPath respath = new ResPath(this.dp2ResTree1.SelectedNode);
                MainForm.AppInfo.SetString(
                    "dp2_search_simple_query",
                    "resdirpath",
                    respath.FullPath);

                if (this.dp2ResTree1.SortTableChanged == true)
                {
                    string strSortTables = dp2ResTree.SaveSortTables(this.dp2ResTree1.sort_tables);

                    this.MainForm.AppInfo.SetString(
           "dp2_search",
           "sort_tables",
           strSortTables);
                }
                this.MainForm.AppInfo.SetBoolean(
       "dp2_search",
       "enable_checkboxes",
       this.dp2ResTree1.CheckBoxes);


                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
                this.MainForm.AppInfo.SetString(
                    "dp2searchform",
                    "record_list_column_width",
                    strWidths);

                this.MainForm.AppInfo.SetString(
    "dp2searchform",
    "query_lines",
    this.dp2QueryControl1.GetSaveString());


                SaveSize();

                this.MainForm.AppInfo.LoadMdiSize -= new EventHandler(AppInfo_LoadMdiSize);
                this.MainForm.AppInfo.SaveMdiSize -= new EventHandler(AppInfo_SaveMdiSize);

            }

            if (this.Channels != null)
                this.Channels.BeforeLogin -= new BeforeLoginEventHandle(Channels_BeforeLogin);

            if (this.m_commentViewer != null)
                this.m_commentViewer.Close();
        }

        public delegate void Delegate_ExpandResDir(string strResDirPath);

        void ExpandResDir(string strResDirPath)
        {
            try
            {
                this.Update();
                ResPath respath = new ResPath(strResDirPath);

                this.EnableControlsInSearching(false);

                // 展开到指定的节点
                this.dp2ResTree1.ExpandPath(respath);

                this.EnableControlsInSearching(true);

                this.EventLoadFinish.Set();

                API.PostMessage(this.Handle, WM_INITIAL_FOCUS, 0, 0);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 等待装载结束
        /// </summary>
        public void WaitLoadFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventLoadFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }

        // 允许或者禁止所有控件
        void EnableControls(bool bEnable)
        {
            this.listView_browse.Enabled = bEnable;
            EnableControlsInSearching(bEnable);
        }

        // 允许或者禁止大部分控件，除listview以外
        void EnableControlsInSearching(bool bEnable)
        {
            if (this.comboBox_simple_matchStyle.Text == "空值")
                this.textBox_simple_queryWord.Enabled = false;
            else
                this.textBox_simple_queryWord.Enabled = bEnable;

            this.comboBox_simple_matchStyle.Enabled = bEnable;

            this.comboBox_multiline_matchStyle.Enabled = bEnable;

            this.button_searchSimple.Enabled = bEnable;

            this.textBox_mutiline_queryContent.Enabled = bEnable;

            this.dp2ResTree1.Enabled = bEnable;

            this.dp2QueryControl1.Enabled = bEnable;

            if (bEnable == false)
            {
                this.timer1.Start();
            }
            else
            {
                this.timer1.Stop();
            }
        }

        private void dp2ResTree1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.textBox_resPath.Text = e.Node.FullPath;
        }

        public LibraryChannel GetChannel(string strServerUrl)
        {
            return this.Channels.GetChannel(strServerUrl);
        }

        public string GetServerUrl(string strServerName)
        {
            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);

            if (server == null)
                return null;    // not found

            return server.Url;
        }

        public int SearchMaxCount
        {
            get
            {
                return MainForm.AppInfo.GetInt(
                    "dp2library",
                    "search_max_count",
                    1000);

            }
        }

        // parameters:
        //      strAction   检索方式  auto / simple / multiline /logic。其中 auto 表示按照当前选定的面板进行检索
        public int DoSearch(string strAction = "auto")
        {
            bool bClear = true; // 是否清除浏览窗中已有的内容

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                bClear = false;

            ClearListViewPropertyCache();
            if (bClear == true)
            {
                if (this.m_nChangedCount > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前命中记录列表中有 " + this.m_nChangedCount.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                        "dp2SearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return 0;
                }
                this.ClearListViewItems();

                ListViewUtil.ClearSortColumns(this.listView_browse);
            }

            this._linkMarcFile = null;

            if (this.tabControl_query.SelectedTab == this.tabPage_simple
                || strAction == "simple")
            {
                if (this.dp2ResTree1.CheckBoxes == true)
                    return DoCheckedSimpleSearch();
                else
                    return DoSimpleSearch();
            }
            else if (this.tabControl_query.SelectedTab == this.tabPage_multiline
                || strAction == "multiline")
            {
                if (this.dp2ResTree1.CheckBoxes == true)
                    return DoCheckedMutilineSearch();
                else
                    return DoMutilineSearch();
            }
            else if (this.tabControl_query.SelectedTab == this.tabPage_logic
                || strAction == "login")
            {
                return DoLogicSearch();
            }
            return 0;
        }

        void ClearListViewItems()
        {
            this.listView_browse.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_browse);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_browse.Columns.Count; i++)
            {
                this.listView_browse.Columns[i].Text = i.ToString();
            }

            this.m_biblioTable = new Hashtable();
            this.m_nChangedCount = 0;

            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();
        }

        public static string GetMatchStyle(string strText)
        {
            // string strText = this.comboBox_matchStyle.Text;

            // 2009/8/6
            if (strText == "空值")
                return "null";

            if (String.IsNullOrEmpty(strText) == true)
                return "left"; // 缺省时认为是 前方一致

            if (strText == "前方一致")
                return "left";
            if (strText == "中间一致")
                return "middle";
            if (strText == "后方一致")
                return "right";
            if (strText == "精确一致")
                return "exact";

            return strText; // 直接返回原文
        }

        // 本函数不负责清除浏览列表中的已有内容
        int DoCheckedSimpleSearch()
        {
            string strError = "";
            int nRet = 0;

            bool bOutputKeyID = false;
            long lTotalCount = 0;	// 命中记录总数

            string strMatchStyle = GetMatchStyle(this.comboBox_multiline_matchStyle.Text);
            TargetItemCollection targets = null;

            // 第一阶段
            // return:
            //      -1  出错
            //      0   尚未选定检索目标
            //      1   成功
            nRet = this.dp2ResTree1.GetSearchTarget(out targets,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            Debug.Assert(targets != null, "GetSearchTarget() 异常");
            if (targets.Count == 0)
            {
                Debug.Assert(false, "");
                strError = "尚未选定检索目标";
                goto ERROR1;
            }


            // 第二阶段
            for (int i = 0; i < targets.Count; i++)
            {
                TargetItem item = (TargetItem)targets[i];
                item.Words = this.textBox_simple_queryWord.Text;
            }
            targets.MakeWordPhrases(
                strMatchStyle,
                false,   // Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_split_words", 1)),
                false,   // Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_detect_range", 0)),
                false    // Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_detect_relation", 0))
                );


            // 参数
            for (int i = 0; i < targets.Count; i++)
            {
                TargetItem item = (TargetItem)targets[i];
                item.MaxCount = this.SearchMaxCount;
            }

            // 第三阶段
            targets.MakeXml();

            // 正式检索

            // 修改窗口标题
            this.Text = "dp2检索窗 " + this.textBox_simple_queryWord.Text;
            ClearListViewPropertyCache();

#if NO
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // 按住Ctrl键的时候，不清除listview中的原有内容
            }
            else
            {
                ClearListViewItems();
            }
#endif

            bool bFillBrowseLine = true;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                bFillBrowseLine = false;
            }

            this.textBox_resultInfo.Clear();

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.m_bInSearching = true;

            long lFillCount = 0;    // 已经装入的部分

            this.listView_browse.BeginUpdate();
            try
            {
                for (int i = 0; i < targets.Count; i++)
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


                    TargetItem item = (TargetItem)targets[i];

                    this.Channel = this.Channels.GetChannel(item.Url);
                    Debug.Assert(this.Channel != null, "Channels.GetChannel 异常");

                    // textBox_simpleQuery_comment.Text += "检索式XML:\r\n" + item.Xml + "\r\n";

                    // 2010/5/18
                    string strBrowseStyle = "id,cols";
                    string strOutputStyle = "";
                    if (bOutputKeyID == true)
                    {
                        strOutputStyle = "keyid";
                        strBrowseStyle = "keyid,id,key,cols";
                    }

                    if (bFillBrowseLine == false)
                        StringUtil.SetInList(ref strBrowseStyle, "cols", false);

                    // MessageBox.Show(this, item.Xml);
                    long lRet = this.Channel.Search(
                        stop,
                        item.Xml,
                        "default",
                        strOutputStyle,
                        out strError);
                    if (lRet == -1)
                    {
                        // textBox_simpleQuery_comment.Text += "出错: " + strError + "\r\n";
                        MessageBox.Show(this, strError);
                        continue;
                    }
                    long lHitCount = lRet;
                    lTotalCount += lRet;

                    stop.SetProgressRange(0, lTotalCount);

                    // textBox_simpleQuery_comment.Text += "命中记录数: " + Convert.ToString(nRet) + "\r\n";
                    this.textBox_resultInfo.Text += "检索词 '" + this.textBox_simple_queryWord.Text + "' 命中 " + lTotalCount.ToString() + " 条记录\r\n";

                    if (lHitCount == 0)
                        continue;

                    long lStart = 0;
                    long lPerCount = Math.Min(50, lHitCount);
                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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

                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lPerCount,
                            strBrowseStyle,
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        // 处理浏览结果
                        for (int j = 0; j < searchresults.Length; j++)
                        {

                            NewLine(
                                this.listView_browse,
                                searchresults[j].Path + "@" + item.ServerName,
                                searchresults[j].Cols);
                        }

                        lStart += searchresults.Length;
                        lFillCount += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                        stop.SetProgressValue(lFillCount);
                    }
                }

                /*
            if (targets.Count > 1)
            {
                textBox_simpleQuery_comment.Text += "命中总条数: " + Convert.ToString(lTotalCount) + "\r\n";
            }
                 * */

            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();


                this.EnableControlsInSearching(true);

                this.m_bInSearching = false;
            }

            if (lTotalCount > 0)
                this.listView_browse.Focus();
            else
                this.textBox_simple_queryWord.Focus();

            return 0;

        ERROR1:
            MessageBox.Show(this, strError);
            this.textBox_simple_queryWord.Focus();
            return -1;
        }

        // 逻辑检索
        // 本函数不负责清除浏览列表中的已有内容
        int DoLogicSearch()
        {
            string strError = "";
            int nRet = 0;

            long lHitCount = 0;
            long lLoaded = 0;

            List<QueryItem> items = null;

            nRet = this.dp2QueryControl1.BuildQueryXml(
            this.SearchMaxCount,
            "zh",
            out items,
            out strError);
            if (nRet == -1)
                goto ERROR1;


            // 修改窗口标题
            this.Text = "dp2检索窗 逻辑检索";

#if NO
            ClearListViewPropertyCache();

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // 按住Ctrl键的时候，不清除listview中的原有内容
            }
            else
            {
                // this.listView_browse.Items.Clear();
                ClearListViewItems();
            }
#endif

            bool bFillBrowseLine = true;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                bFillBrowseLine = false;
            }

            this.textBox_resultInfo.Clear();

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.m_bInSearching = true;

            long lTotalHitCount = 0;
            int nErrorCount = 0;

            this.listView_browse.BeginUpdate();
            try
            {
                for (int j = 0; j < items.Count; j++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }
                    QueryItem item = items[j];

                    string strServerName = item.ServerName;

                    dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
                    if (server == null)
                    {
                        strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                        goto ERROR1;
                    }

                    string strServerUrl = server.Url;
                    this.Channel = this.Channels.GetChannel(strServerUrl);

                    string strOutputStyle = "id";

                    long lRet = Channel.Search(stop,
                        item.QueryXml,
                        "default",
                        strOutputStyle,
                        out strError);
                    if (lRet == -1)
                    {
                        this.textBox_resultInfo.Text += "检索式 '" + item.QueryXml + "' 检索时发生错误：" + strError + "\r\n";
                        nErrorCount++;
                        continue;
                    }

                    lHitCount = lRet;

                    lTotalHitCount += lHitCount;

                    stop.SetProgressRange(0, lTotalHitCount);

                    this.textBox_resultInfo.Text += "已命中 " + lTotalHitCount.ToString() + " 条，检索尚未结束...\r\n";

                    if (lHitCount == 0)
                        continue;

                    long lStart = 0;
                    long lPerCount = Math.Min(50, lHitCount);
                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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

                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lPerCount,
                            bFillBrowseLine == true ? "id,cols" : "id",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            NewLine(
                                this.listView_browse,
                                searchresults[i].Path + "@" + strServerName,
                                searchresults[i].Cols);

                            lLoaded++;
                            stop.SetProgressValue(lLoaded);
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                    }
                } // end of items

                if (nErrorCount == 0)
                    this.textBox_resultInfo.Text = "共命中 " + lTotalHitCount.ToString() + " 条";
                else
                    this.textBox_resultInfo.Text += "检索完成。共命中 " + lTotalHitCount.ToString() + " 条";
            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);

                this.m_bInSearching = false;
            }

            if (lTotalHitCount > 0)
                this.listView_browse.Focus();
            else
                this.dp2QueryControl1.Focus();

            return 0;

        ERROR1:
            this.textBox_resultInfo.Text += strError;
            MessageBox.Show(this, strError);
            this.dp2QueryControl1.Focus();
            return -1;
        }

        // 单行检索。树为非CheckBox状态
        // 本函数不负责清除浏览列表中的已有内容
        int DoSimpleSearch()
        {
            string strError = "";
            int nRet = 0;

            long lHitCount = 0;

            string strServerName = "";
            string strServerUrl = "";
            string strDbName = "";
            string strFrom = "";
            string strFromStyle = "";

            nRet = dp2ResTree.GetNodeInfo(this.dp2ResTree1.SelectedNode,
                out strServerName,
                out strServerUrl,
                out strDbName,
                out strFrom,
                out strFromStyle,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strFromStyle = dp2ResTree.GetDisplayFromStyle(strFromStyle, true, false);   // 注意，去掉 __ 开头的那些，应该还剩下至少一个 style。_ 开头的不要滤出

            this.Channel = this.Channels.GetChannel(strServerUrl);

            // 修改窗口标题
            this.Text = "dp2检索窗 " + this.textBox_simple_queryWord.Text;


#if NO
            ClearListViewPropertyCache();

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // 按住Ctrl键的时候，不清除listview中的原有内容
            }
            else
            {
                // this.listView_browse.Items.Clear();
                ClearListViewItems();
            }
#endif

            bool bFillBrowseLine = true;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                bFillBrowseLine = false;
            }

            this.textBox_resultInfo.Clear();

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.m_bInSearching = true;

            this.listView_browse.BeginUpdate();
            try
            {
                if (String.IsNullOrEmpty(strDbName) == true)
                    strDbName = "<all>";

                if (String.IsNullOrEmpty(strFrom) == true)
                {
                    strFrom = "<all>";
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strFromStyle = "<all>";
                }

                // 注："null"只能在前端短暂存在，而内核是不认这个所谓的matchstyle的
                string strMatchStyle = GetMatchStyle(this.comboBox_simple_matchStyle.Text);

                if (this.textBox_simple_queryWord.Text == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.textBox_simple_queryWord.Text = "";

                        // 专门检索空值
                        strMatchStyle = "exact";
                    }
                    else
                    {
                        // 为了在检索词为空的时候，检索出全部的记录
                        strMatchStyle = "left";
                    }
                }
                else
                {
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        goto ERROR1;
                    }
                }

                string strQueryXml = "";
                long lRet = Channel.SearchBiblio(stop,
                    strDbName,
                    this.textBox_simple_queryWord.Text,
                    this.SearchMaxCount,    // 1000,
                    strFromStyle,
                    strMatchStyle,
                    this.Lang,
                    null,   // strResultSetName
                    "", // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                {
                    this.textBox_resultInfo.Text += "检索词 '" + this.textBox_simple_queryWord.Text + "' 检索时发生错误：" + strError + "\r\n";
                    goto ERROR1;
                }

                lHitCount = lRet;

                this.textBox_resultInfo.Text += "检索词 '" + this.textBox_simple_queryWord.Text + "' 命中 " + lHitCount.ToString() + " 条记录\r\n";

                stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                this.listView_browse.Focus();

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

                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lPerCount,
                        bFillBrowseLine == true ? "id,cols" : "id",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        goto ERROR1;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {

                        NewLine(
                            this.listView_browse,
                            searchresults[i].Path + "@" + strServerName,
                            searchresults[i].Cols);
                    }

                    lStart += searchresults.Length;
                    // lCount -= searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;

                    stop.SetProgressValue(lStart);

                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);

                this.m_bInSearching = false;
            }

            if (lHitCount > 0)
                this.listView_browse.Focus();
            else
                this.textBox_simple_queryWord.Focus();

            return 0;

        ERROR1:
            MessageBox.Show(this, strError);
            this.textBox_simple_queryWord.Focus();
            return -1;
        }

        // 多行检索
        // 本函数不负责清除浏览列表中的已有内容
        int DoCheckedMutilineSearch()
        {
            string strError = "";
            int nRet = 0;

            long lTotalCount = 0;	// 命中记录总数
            long lFillCount = 0;
            long lHitCount = 0;
            int nLineCount = 0;

            List<string> hited_lines = new List<string>(4096);
            List<string> nothited_lines = new List<string>(4096);

            string strMatchStyle = GetMatchStyle(this.comboBox_multiline_matchStyle.Text);

            TargetItemCollection targets = null;

            // 第一阶段
            // return:
            //      -1  出错
            //      0   尚未选定检索目标
            //      1   成功
            nRet = this.dp2ResTree1.GetSearchTarget(out targets,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            Debug.Assert(targets != null, "GetSearchTarget() 异常");
            if (targets.Count == 0)
            {
                Debug.Assert(false, "");
                strError = "尚未选定检索目标";
                goto ERROR1;
            }

            bool bDontAsk = this.MainForm.AppInfo.GetBoolean(
                "dp2_search_muline_query",
                "matchstyle_middle_dontask",
                false);
            if (strMatchStyle == "middle" && bDontAsk == false)
            {
                MessageDialog.Show(this,
                    "您选择了 中间一致 匹配方式进行检索，这种匹配方式检索速度稍慢。如果可能，最好采用其他匹配方式，以便提高检索速度。",
                    "下次不再出现此对话框",
                    ref bDontAsk);
                if (bDontAsk == true)
                {
                    this.MainForm.AppInfo.SetBoolean(
                        "dp2_search_muline_query",
                        "matchstyle_middle_dontask",
                        bDontAsk);
                }
            }

            List<string> dbnames = new List<string>();
            nRet = targets.GetDbNameList(out dbnames, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (dbnames.Count > 1)
            {
                DbSelectListDialog select_dlg = new DbSelectListDialog();
                GuiUtil.SetControlFont(select_dlg, this.Font);
                select_dlg.DbNames = dbnames;
                select_dlg.SelectAllDb = this.SelectAllDb;
                this.MainForm.AppInfo.LinkFormState(select_dlg, "dp2searchform_DbSelectListDialog_state");
                select_dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(select_dlg);
                if (select_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return 0;

                this.SelectAllDb = select_dlg.SelectAllDb;

                if (select_dlg.SelectAllDb == true)
                    dbnames = null; // 全部命中都要
                else
                    dbnames = select_dlg.DbNames;   // 顺序选择
            }
            else
                dbnames = null;

            // 修改窗口标题
            this.Text = "dp2检索窗 " + this.textBox_simple_queryWord.Text;

#if NO
            ClearListViewPropertyCache();

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // 按住Ctrl键的时候，不清除listview中的原有内容
            }
            else
            {
                // this.listView_browse.Items.Clear();
                ClearListViewItems();
            }
#endif

            bool bFillBrowseLine = true;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                bFillBrowseLine = false;
            }

            this.textBox_resultInfo.Clear();

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.m_bInSearching = true;

            // long lTotalHitCount = 0;

            this.listView_browse.BeginUpdate();
            try
            {
                DateTime start_time = DateTime.Now;
                stop.SetProgressRange(0, this.textBox_mutiline_queryContent.Lines.Length);

                for (int j = 0; j < this.textBox_mutiline_queryContent.Lines.Length; j++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    string strLine = this.textBox_mutiline_queryContent.Lines[j].Trim();

                    stop.SetProgressValue(j);

                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;

                    nLineCount++;

                    // 第二阶段
                    for (int i = 0; i < targets.Count; i++)
                    {
                        TargetItem item = (TargetItem)targets[i];
                        item.Words = strLine;
                    }
                    targets.MakeWordPhrases(
                        strMatchStyle,
                        false,   // Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_split_words", 1)),
                        false,   // Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_detect_range", 0)),
                        false    // Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_detect_relation", 0))
                        );

                    // 参数
                    for (int i = 0; i < targets.Count; i++)
                    {
                        TargetItem item = (TargetItem)targets[i];
                        item.MaxCount = this.SearchMaxCount;
                    }

                    // 第三阶段
                    targets.MakeXml();

                    long lPerLineHitCount = 0;

                    List<ListViewItem> new_items = new List<ListViewItem>();

                    for (int i = 0; i < targets.Count; i++)
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


                        TargetItem item = (TargetItem)targets[i];

                        this.Channel = this.Channels.GetChannel(item.Url);
                        Debug.Assert(this.Channel != null, "Channels.GetChannel 异常");

                        long lRet = this.Channel.Search(
            stop,
            item.Xml,
            "default",
            "", // strOutputStyle,
            out strError);
                        if (lRet == -1)
                        {
                            this.textBox_resultInfo.Text += "检索词 '" + strLine + "' 检索时发生错误：" + strError + "\r\n";
                            continue;
                        }

                        lHitCount = lRet;
                        lTotalCount += lHitCount;
                        lPerLineHitCount += lHitCount;

                        if (lHitCount == 0)
                            continue;

                        // stop.SetProgressRange(0, lTotalCount);

                        long lStart = 0;
                        long lPerCount = Math.Min(50, lHitCount);
                        DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                        // this.listView_browse.Focus();

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

                            stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " ('" + strLine + "' 命中 " + lHitCount.ToString() + " 条记录) ...");

                            lRet = Channel.GetSearchResult(
                                stop,
                                "default",   // strResultSetName
                                lStart,
                                lPerCount,
                                bFillBrowseLine == true ? "id,cols" : "id",
                                this.Lang,
                                out searchresults,
                                out strError);
                            if (lRet == -1)
                                goto ERROR1;

                            if (lRet == 0)
                            {
                                strError = "未命中";
                                goto ERROR1;
                            }

                            // 处理浏览结果
                            for (int k = 0; k < searchresults.Length; k++)
                            {
                                ListViewItem new_item = NewLine(
                                    this.listView_browse,
                                    searchresults[k].Path + "@" + item.ServerName,
                                    searchresults[k].Cols);
                                new_items.Add(new_item);
                            }

                            lStart += searchresults.Length;
                            lFillCount += searchresults.Length;
                            // stop.SetProgressValue(lFillCount);
                            // lCount -= searchresults.Length;
                            if (lStart >= lHitCount || lPerCount <= 0)
                                break;

                        }
                    }

                    int nEndLine = this.listView_browse.Items.Count;

                    // 需要筛选
                    if (dbnames != null && new_items.Count > 1)
                    {
                        int nRemoved = RemoveMultipleItems(dbnames, new_items);
                        lPerLineHitCount -= nRemoved;
                        lTotalCount -= nRemoved;
                    }


                    // this.textBox_resultInfo.Text += "检索词 '" + strLine + "' 命中 " + lPerLineHitCount.ToString() + " 条记录\r\n";
                    if (lPerLineHitCount == 0)
                        nothited_lines.Add(strLine);
                    else
                        hited_lines.Add(strLine + "\t" + lPerLineHitCount.ToString());

                } // end of lines

                TimeSpan delta = DateTime.Now - start_time;

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                // 最后显示整理过的注释
                string strComment = "检索 " + nLineCount.ToString() + " 行用时 " + delta.ToString() + "\r\n";
                if (hited_lines.Count > 0)
                {
                    strComment += "*** 以下检索词共命中 " + lTotalCount.ToString() + " 条:\r\n";
                    foreach (string strLine in hited_lines)
                    {
                        strComment += strLine + "\r\n";
                    }
                }
                if (nothited_lines.Count > 0)
                {
                    strComment += "*** 以下检索词没有命中:\r\n";
                    foreach (string strLine in nothited_lines)
                    {
                        strComment += strLine + "\r\n";
                    }
                }
                this.textBox_resultInfo.Text = strComment;
            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);

                this.m_bInSearching = false;
            }

            if (lTotalCount > 0)
                this.listView_browse.Focus();
            else
                this.textBox_mutiline_queryContent.Focus();

            return 0;

        ERROR1:
            MessageBox.Show(this, strError);
            this.textBox_simple_queryWord.Focus();
            return -1;
        }

        // 去除多余的命中记录行
        int RemoveMultipleItems(List<string> dbnames,
            List<ListViewItem> items)
        {
            List<string> hit_dbnames = new List<string>();
            // 列出已经命中的数据库名
            foreach (ListViewItem item in items)
            {
                string strNameString = GetDbNameString(item.Text);
                hit_dbnames.Add(strNameString);
            }

            string strFoundDbName = "";
            foreach (string strDbName in dbnames)
            {
                if (hit_dbnames.IndexOf(strDbName) != -1)
                {
                    strFoundDbName = strDbName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(strFoundDbName) == true)
            {
                Debug.Assert(false, "");
                return 0;
            }

            // 除了这个数据库名以外的都删除
            int nDeleteCount = 0;
            foreach (ListViewItem item in items)
            {
                string strNameString = GetDbNameString(item.Text);

                if (strNameString != strFoundDbName)
                {
                    this.listView_browse.Items.Remove(item);
                    nDeleteCount++;
                }
            }

            return nDeleteCount;
        }

        // 从路径字符串中获得表示数据库的字符串
        // '数据库名@服务器名'
        static string GetDbNameString(string strRecPath)
        {
            string strServerName = "";
            string strPath = "";
            ParseRecPath(strRecPath,
                out strServerName,
                out strPath);
            string strDbName = GetDbName(strPath);

            return strDbName + "@" + strServerName;
        }

        // 多行检索。树为非CheckBox状态
        // 本函数不负责清除浏览列表中的已有内容
        int DoMutilineSearch()
        {
            string strError = "";
            int nRet = 0;

            long lHitCount = 0;

            List<string> hited_lines = new List<string>(4096);
            List<string> nothited_lines = new List<string>(4096);

            string strServerName = "";
            string strServerUrl = "";
            string strDbName = "";
            string strFrom = "";
            string strFromStyle = "";

            nRet = dp2ResTree.GetNodeInfo(this.dp2ResTree1.SelectedNode,
                out strServerName,
                out strServerUrl,
                out strDbName,
                out strFrom,
                out strFromStyle,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.Channel = this.Channels.GetChannel(strServerUrl);

            // 修改窗口标题
            this.Text = "dp2检索窗 " + this.textBox_simple_queryWord.Text;

#if NO
            ClearListViewPropertyCache();

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // 按住Ctrl键的时候，不清除listview中的原有内容
            }
            else
            {
                // this.listView_browse.Items.Clear();
                ClearListViewItems();
            }
#endif

            bool bFillBrowseLine = true;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                bFillBrowseLine = false;
            }
            
            this.textBox_resultInfo.Clear();

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.m_bInSearching = true;

            long lTotalHitCount = 0;
            int nLineCount = 0;

            this.listView_browse.BeginUpdate();
            try
            {
                if (String.IsNullOrEmpty(strDbName) == true)
                    strDbName = "<all>";

                if (String.IsNullOrEmpty(strFrom) == true)
                {
                    strFrom = "<all>";
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strFromStyle = "<all>";
                }

                string strMatchStyle = GetMatchStyle(this.comboBox_multiline_matchStyle.Text);

                bool bDontAsk = this.MainForm.AppInfo.GetBoolean(
"dp2_search_muline_query",
"matchstyle_middle_dontask",
false);
                if (strMatchStyle == "middle" && bDontAsk == false)
                {
                    MessageDialog.Show(this,
                        "您选择了 中间一致 匹配方式进行检索，这种匹配方式检索速度稍慢。如果可能，最好采用其他匹配方式，以便提高检索速度。",
                        "下次不再出现此对话框",
                        ref bDontAsk);
                    if (bDontAsk == true)
                    {
                        this.MainForm.AppInfo.SetBoolean(
                            "dp2_search_muline_query",
                            "matchstyle_middle_dontask",
                            bDontAsk);
                    }
                }

                DateTime start_time = DateTime.Now;

                stop.SetProgressRange(0, this.textBox_mutiline_queryContent.Lines.Length);

                for (int j = 0; j < this.textBox_mutiline_queryContent.Lines.Length; j++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    string strLine = this.textBox_mutiline_queryContent.Lines[j].Trim();

                    stop.SetProgressValue(j);

                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;

                    string strQueryXml = "";
                    long lRet = Channel.SearchBiblio(stop,
                        strDbName,
                        strLine,
                        this.SearchMaxCount,    // 1000,
                        strFromStyle,
                        strMatchStyle,
                        this.Lang,
                        null,   // strResultSetName
                        "",    // strSearchStyle
                        "", // strOutputStyle
                        out strQueryXml,
                        out strError);
                    if (lRet == -1)
                    {
                        this.textBox_resultInfo.Text += "检索词 '" + strLine + "' 检索时发生错误：" + strError + "\r\n";
                        continue;
                    }

                    lHitCount = lRet;

                    nLineCount++;

                    lTotalHitCount += lHitCount;
                    // this.textBox_resultInfo.Text += "检索词 '" + strLine + "' 命中 " + lHitCount.ToString() + " 条记录\r\n";
                    if (lHitCount == 0)
                        nothited_lines.Add(strLine);
                    else
                        hited_lines.Add(strLine + "\t" + lHitCount.ToString());

                    if (lHitCount == 0)
                        continue;

                    long lStart = 0;
                    long lPerCount = Math.Min(50, lHitCount);
                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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

                        stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " ('" + strLine + "' 命中 " + lHitCount.ToString() + " 条记录) ...");

                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lPerCount,
                            bFillBrowseLine == true ? "id,cols" : "id",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            NewLine(
                                this.listView_browse,
                                searchresults[i].Path + "@" + strServerName,
                                searchresults[i].Cols);
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                    }
                } // end of lines

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);

                TimeSpan delta = DateTime.Now - start_time;

                // 最后显示整理过的注释
                string strComment = "检索 "+nLineCount.ToString()+" 行用时 " + delta.ToString() + "\r\n";
                if (hited_lines.Count > 0)
                {
                    strComment += "*** 以下检索词共命中 " + lTotalHitCount.ToString() + " 条:\r\n";
                    foreach (string strLine in hited_lines)
                    {
                        strComment += strLine + "\r\n";
                    }
                }
                if (nothited_lines.Count > 0)
                {
                    strComment += "*** 以下检索词没有命中:\r\n";
                    foreach (string strLine in nothited_lines)
                    {
                        strComment += strLine + "\r\n";
                    }
                }
                this.textBox_resultInfo.Text = strComment;
            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);

                this.m_bInSearching = false;
            }

            if (lTotalHitCount > 0)
                this.listView_browse.Focus();
            else
                this.textBox_mutiline_queryContent.Focus();

            return 0;

        ERROR1:
            MessageBox.Show(this, strError);
            this.textBox_simple_queryWord.Focus();
            return -1;
        }

        // 解析记录路径。
        // 记录路径为如下形态 "中文图书/1 @服务器"
        public static void ParseRecPath(string strRecPath,
            out string strServerName,
            out string strPath)
        {
            int nRet = strRecPath.IndexOf("@");
            if (nRet == -1)
            {
                strServerName = "";
                strPath = strRecPath;
                return;
            }
            strServerName = strRecPath.Substring(nRet + 1).Trim();
            strPath = strRecPath.Substring(0, nRet).Trim();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        // 在listview最后追加一行
        public static ListViewItem NewLine(
            ListView list,
            string strID,
            string[] others)
        {
            if (others == null)
                ListViewUtil.EnsureColumns(list, 1);
            else
                ListViewUtil.EnsureColumns(list, others.Length + 1);

            ListViewItem item = new ListViewItem(strID, 0);

            list.Items.Add(item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            return item;
        }

        public static void ChangeCols(ListViewItem item,
            string strRecPath,
            string[] cols)
        {
            ListViewUtil.ChangeItemText(item, 0, strRecPath);

            int nCol = 1;
            foreach (string s in cols)
            {
                ListViewUtil.ChangeItemText(item, nCol, s);
                nCol++;
            }
            // TODO: 清空多余的列
        }

        /*
        // 确保列标题数量足够
        public static void EnsureColumns(ListView list,
            int nCount)
        {
            if (list.Columns.Count >= nCount)
                return;

            for (int i = list.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                if (i == 0)
                {
                    strText = "记录路径";
                }
                else
                {
                    strText = Convert.ToString(i);
                }

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = 200;
                list.Columns.Add(col);
            }

        }
         * */

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
                string strServerName = "";
                string strServerUrl = "";
                string strDbName = "";
                string strFrom = "";
                string strFromStyle = "";

                string strError = "";

                int nRet = dp2ResTree.GetNodeInfo(this.dp2ResTree1.SelectedNode,
                    out strServerName,
                    out strServerUrl,
                    out strDbName,
                    out strFrom,
                    out strFromStyle,
                    out strError);
                if (nRet == -1)
                    return "";

                return strServerName
                    + "/" + strDbName
                    + "/" + strFrom
                    + "/" + this.textBox_simple_queryWord.Text
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
            strError = "";
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

            List<ListViewItem> items = new List<ListViewItem>();

            if (index == -1)
            {
                ListViewItem item = ListViewUtil.FindItem(this.listView_browse, strPath, 0);
                if (item == null)
                {
                    strError = "路径为 '" + strPath + "' 的事项在列表中没有找到";
                    return 2;
                }
                items.Add(item);
            }
            else
            {
                if (index >= this.listView_browse.Items.Count)
                {
                    strError = "index ["+index.ToString()+"] 越过结果集尾部";
                    return -1;
                }
                items.Add(this.listView_browse.Items[index]);
            }

            if (strAction == "refresh")
            {
                nRet = RefreshListViewLines(items,
        out strError);
                if (nRet == -1)
                    return -1;

                DoViewComment(false);
                return 1;
            }

            return 0;
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
            strError = "";

            // 解析记录路径
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strSavePath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除记录 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                stop.SetMessage("正在删除书目记录 " + strPurePath + " ...");


                string[] formats = null;
                formats = new string[1];
                formats[0] = "xml";

                // string[] results = null;
                //                 byte[] baTimestamp = null;

                string strOutputBibilioRecPath = "";

                long lRet = Channel.SetBiblioInfo(
                    stop,
                    "delete",
                    strPurePath,
                    "xml",
                    "",
                    baTimestamp,
                    "",
                    out strOutputBibilioRecPath,
                    out baOutputTimestamp,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
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

        // 同步一条 MARC/XML 记录
        // 如果 Lversion 比检索窗中的记录新，则用 strMARC 内容更新检索窗内的记录
        // 如果 lVersion 比检索窗中的记录旧(也就是说 Lverion 的值偏小)，那么从 strMARC 中取出记录更新到记录窗
        // parameters:
        //      lVersion    [in]记录窗的 Version [out] 检索窗的记录 Version
        // return:
        //      -1  出错
        //      0   没有必要更新
        //      1   已经更新到 检索窗
        //      2   需要从 strMARC 中取出内容更新到记录窗
        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            BiblioInfo info = null;

            // 存储所获得书目记录 XML
            info = (BiblioInfo)this.m_biblioTable[strPath];
            if (info == null)
            {
                // 检索窗中内存尚未存储的情况，相当于 version = 0
                if (lVersion > 0)
                {
                    // 预先准备好 info 
                    // 找到 Item 行
                    ListViewItem item = ListViewUtil.FindItem(this.listView_browse, strPath, 0);
                    if (item == null)
                    {
                        strError = "路径为 '"+strPath+"' 的事项在列表中没有找到";
                        return -1;
                    }

                    nRet = GetBiblioInfo(
                        true,
                        item,
                        out info,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 继续向后执行
                }
                else
                    return 0;
            }

            if (info != null)
            {
                if (lVersion == info.NewVersion)
                    return 0;

                string strXml = "";
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    strXml = info.OldXml;
                else
                    strXml = info.NewXml;

                if (lVersion > info.NewVersion)
                {
                    // 来自 strMARC 的更新一点
                    info.NewVersion = lVersion;

                    if (strSyntax == "xml")
                        strXml = strMARC;
                    else
                    {
                        XmlDocument domMarc = new XmlDocument();
                        domMarc.LoadXml(strXml);

                        nRet = MarcUtil.Marc2Xml(strMARC,
                            strSyntax,
                            out domMarc,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        strXml = domMarc.OuterXml;
                    }
                }
                else
                {
                    // 来自 info 的更新一点
                    lVersion = info.NewVersion;

                    if (strSyntax == "xml")
                        strMARC = strXml;
                    else
                    {
                        // 将XML格式转换为MARC格式
                        // 自动从数据记录中获得MARC语法
                        nRet = MarcUtil.Xml2Marc(strXml,
                            true,
                            null,
                            out strSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    return 2;
                }

                /*
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    info.OldXml = strXml;
                else
                    info.NewXml = strXml;
                 * */
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    this.m_nChangedCount++;
                info.NewXml = strXml;

                DoViewComment(false);
                return 1;
            }

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

#if NO
            // 防止重入
            if (m_bInSearching == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法获得记录。请稍后再试。";
                return -1;
            }
#endif

            if (strStyle != "marc" && strStyle != "xml")
            {
                strError = "dp2SearchForm只支持获取MARC格式记录和xml格式记录，不支持 '" + strStyle + "' 格式的记录";
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
                string strOutputPath = "";
                nRet = InternalGetOneRecord(
                    true,
                    strStyle,
                    strPath,
                    strDirection,
                    strParameters,  // 2013/9/22
                    out strRecord,
                    out strXmlFragment,
                    out strOutputPath,
                    out strOutStyle,
                    out baTimestamp,
                    out record,
                    out currrentEncoding,
                    out strError);
                if (string.IsNullOrEmpty(strOutputPath) == false)
                    strSavePath = this.CurrentProtocol + ":" + strOutputPath;
                return nRet;
            }

            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);

            if (index >= this.listView_browse.Items.Count)
            {
                // 如果检索曾经中断过，这里可以触发继续检索
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

#if NO
            if (this.linkMarcFile != null)
            {
                BiblioInfo info = null;
                int nRet = GetBiblioInfo(
                    true,
                    curItem,
                    out info,
                    out strError);
                if (info == null)
                {
                    strError = "not found";
                    return -1;
                }

                if (strStyle == "marc")
                {
                    string strMarcSyntax = "";
                    string strOutMarcSyntax = "";
                    // 从数据记录中获得MARC格式
                    nRet = MarcUtil.Xml2Marc(info.OldXml,
                        true,
                        strMarcSyntax,
                        out strOutMarcSyntax,
                        out strRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        return -1;
                    }

                    record = new DigitalPlatform.Z3950.Record();
                    if (strOutMarcSyntax == "unimarc" || strOutMarcSyntax == "")
                        record.m_strSyntaxOID = "1.2.840.10003.5.1";
                    else if (strOutMarcSyntax == "usmarc")
                        record.m_strSyntaxOID = "1.2.840.10003.5.10";
                    else if (strOutMarcSyntax == "dc")
                        record.m_strSyntaxOID = "?";
                    else
                    {
                        strError = "未知的MARC syntax '" + strOutMarcSyntax + "'";
                        return -1;
                    }

                    // 获得书目以外的其它XML片断
                    nRet = GetXmlFragment(info.OldXml,
            out strXmlFragment,
            out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    strRecord = info.OldXml;
                    strOutStyle = strStyle;

                    record = new DigitalPlatform.Z3950.Record();
                    record.m_strSyntaxOID = "1.2.840.10003.5.109.10";
                }

                return 0;
            }
#endif

            strPath = curItem.Text;

            strSavePath = this.CurrentProtocol + ":" + strPath;

            {
                string strOutputPath = "";

                nRet = InternalGetOneRecord(
                    true,
                    strStyle,
                    strPath,
                    "",
                    strParameters,  // 2013/9/22
                    out strRecord,
                    out strXmlFragment,
                    out strOutputPath,
                    out strOutStyle,
                    out baTimestamp,
                    out record,
                    out currrentEncoding,
                    out strError);
                if (string.IsNullOrEmpty(strOutputPath) == false)
                    strSavePath = this.CurrentProtocol + ":" + strOutputPath;
                return nRet;
            }

        }

        #endregion

        // 是否为尚未初始化的 BiblioInfo
        static bool IsNullBiblioInfo(BiblioInfo info)
        {
            if (string.IsNullOrEmpty(info.OldXml) == true
                && string.IsNullOrEmpty(info.NewXml) == true)
                return true;
            return false;
        }

        // 获得一条MARC/XML记录
        // parameters:
        //      strPath 记录路径。格式为"中文图书/1 @服务器名"
        //      strDirection    方向。为 prev/next/current之一。current可以缺省。
        //      strOutputPath   [out]返回的实际路径。格式和strPath相同。不包含协议名称部分。
        //      strXmlFragment  书目以外的XML其它片断。当strStyle不是"marc"的时候，不返回这个
        // return:
        //      -1  error 包括not found
        //      0   found
        //      1   为诊断记录
        int InternalGetOneRecord(
            bool bUseLoop,
            string strStyle,
            string strPath,
            string strDirection,
            string strParameters,   // "reload" 从数据库重新获取
            out string strRecord,
            out string strXmlFragment,
            out string strOutputPath,
            out string strOutStyle,
            out byte[] baTimestamp,
            out DigitalPlatform.Z3950.Record record,
            out Encoding currrentEncoding,
            out string strError)
        {
            strXmlFragment = "";
            strRecord = "";
            strOutputPath = "";
            record = null;
            strError = "";
            currrentEncoding = this.CurrentEncoding;
            baTimestamp = null;
            strOutStyle = "marc";

            if (strStyle != "marc" && strStyle != "xml")
            {
                strError = "dp2SearchForm只支持获取MARC格式记录和xml格式记录，不支持 '" + strStyle + "' 格式的记录";
                return -1;
            }

            bool bReload = StringUtil.IsInList("reload", strParameters);

            string strXml = "";

            // if (this.linkMarcFile != null)
            if (bReload == false)
            {
                BiblioInfo info = null;

                // 存储所获得书目记录 XML
                info = (BiblioInfo)this.m_biblioTable[strPath];
                if (info != null)
                {
                    if (string.IsNullOrEmpty(info.NewXml) == true)
                        strXml = info.OldXml;
                    else
                        strXml = info.NewXml;

                    Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");

                    strOutputPath = info.RecPath;
                    baTimestamp = info.Timestamp;
                    goto SKIP0;
                }
            }

            // 解析记录路径
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            Stop temp_stop = this.stop;
            LibraryChannel channel = null;

            bool bUseNewChannel = false;
            if (m_bInSearching == true)
            {
                channel = this.Channels.NewChannel(strServerUrl);
                bUseNewChannel = true;

                temp_stop = new Stop();
                temp_stop.Register(MainForm.stopManager, true);	// 和容器关联
            }
            else
            {
                this.Channel = this.Channels.GetChannel(strServerUrl);
                channel = this.Channel;
            }

            if (bUseLoop == true)
            {
                temp_stop.OnStop += new StopEventHandler(this.DoStop);
                temp_stop.Initial("正在初始化浏览器组件 ...");
                temp_stop.BeginLoop();

                this.Update();
                this.MainForm.Update();
            }

            try
            {
                temp_stop.SetMessage("正在装入书目记录 " + strPath + " ...");

                string[] formats = null;
                formats = new string[2];
                formats[0] = "xml";
                formats[1] = "outputpath";  // 获得实际路径

                string[] results = null;
                //                 byte[] baTimestamp = null;

                Debug.Assert(string.IsNullOrEmpty(strPurePath) == false, "");

                string strCmd = strPurePath;
                if (String.IsNullOrEmpty(strDirection) == false)
                    strCmd += "$" + strDirection;

                long lRet = channel.GetBiblioInfos(
                    temp_stop,
                    strCmd,
                    "",
                    formats,
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    if (String.IsNullOrEmpty(strDirection) == true
                        || strDirection == "current")
                        strError = "路径为 '" + strPath + "' 的书目记录没有找到 ...";
                    else
                    {
                        string strText = strDirection;
                        if (strDirection == "prev")
                            strText = "前一条";
                        else if (strDirection == "next")
                            strText = "后一条";
                        strError = "路径为 '" + strPath + "' 的"+strText+"书目记录没有找到 ...";
                    }

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

                strXml = results[0];
                strOutputPath = results[1] + "@" + strServerName;
                Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");
            }
            finally
            {
                if (bUseLoop == true)
                {
                    temp_stop.EndLoop();
                    temp_stop.OnStop -= new StopEventHandler(this.DoStop);
                    temp_stop.Initial("");
                }

                if (bUseNewChannel == true)
                {
                    this.Channels.RemoveChannel(channel);
                    channel = null;

                    temp_stop.Unregister();	// 和容器关联
                    temp_stop = null;
                }
            }

        SKIP0:
            if (strStyle == "marc")
            {

                string strMarcSyntax = "";
                string strOutMarcSyntax = "";
                // 从数据记录中获得MARC格式
                int nRet = MarcUtil.Xml2Marc(strXml,
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

                Debug.Assert(string.IsNullOrEmpty(strRecord) == false, "");

                record = new DigitalPlatform.Z3950.Record();
                if (strOutMarcSyntax == "unimarc" || strOutMarcSyntax == "")
                    record.m_strSyntaxOID = "1.2.840.10003.5.1";
                else if (strOutMarcSyntax == "usmarc")
                    record.m_strSyntaxOID = "1.2.840.10003.5.10";
                else if (strOutMarcSyntax == "dc")
                    record.m_strSyntaxOID = "?";
                else
                {
                    strError = "未知的MARC syntax '" + strOutMarcSyntax + "'";
                    goto ERROR1;
                }

                // 获得书目以外的其它XML片断
                nRet = GetXmlFragment(strXml,
        out strXmlFragment,
        out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                strRecord = strXml;
                strOutStyle = strStyle;

                record = new DigitalPlatform.Z3950.Record();
                record.m_strSyntaxOID = "1.2.840.10003.5.109.10";
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 获得书目以外的其它XML片断
        public static int GetXmlFragment(string strXml,
            out string strXmlFragment,
            out string strError)
        {
            strXmlFragment = "";
            strError = "";

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

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            nsmgr.AddNamespace("unimarc", DpNs.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//unimarc:leader | //unimarc:controlfield | //unimarc:datafield | //usmarc:leader | //usmarc:controlfield | //usmarc:datafield", nsmgr);
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            strXmlFragment = dom.DocumentElement.InnerXml;

            return 0;
        }


        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            int nIndex = -1;
            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex,
                LoadToExistDetailWindow == true? false : true);
        }

        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetOneRecordSyntax(int index,
            bool bUseNewChannel,
            out string strSyntax,
            out string strError)
        {
            strError = "";
            strSyntax = "";
            int nRet = 0;

            if (index >= this.listView_browse.Items.Count)
            {
                // 如果检索曾经中断过，这里可以触发继续检索
                strError = "越过结果集尾部";
                return -1;
            }

            if (this._linkMarcFile != null)
            {
                if (_linkMarcFile.MarcSyntax == "<自动>"
                    || _linkMarcFile.MarcSyntax.ToLower() == "<auto>")
                {
                    // 
                }

                strSyntax = this._linkMarcFile.MarcSyntax;
                if (string.IsNullOrEmpty(strSyntax) == false)
                    return 1;
                else
                    return 0;
            }

            ListViewItem curItem = this.listView_browse.Items[index];

            string strPath = curItem.Text;
            // 解析记录路径
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            if (string.Compare(strServerName, "mem", true) == 0 
                || string.Compare(strServerName, "file", true) == 0)
            {
                // 从 hashtable 中探得 MARC 格式
                BiblioInfo info = null;

                // 存储所获得书目记录 XML
                info = (BiblioInfo)this.m_biblioTable[strPath];
                if (info == null)
                {
                    strError = "路径在 '"+strPath+"' 的记录信息在 m_biblioTable 中没有找到";
                    return -1;
                }

                string strXml = "";
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    strXml = info.OldXml;
                else
                    strXml = info.NewXml;

                string strMARC = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml,
                    true,
                    null,
                    out strSyntax,
                    out strMARC,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (string.IsNullOrEmpty(strSyntax) == false)
                    return 1;

                return 0;
            }

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            }

            string strServerUrl = server.Url;

            string strBiblioDbName = GetDbName(strPurePath);

            // 获得一个数据库的数据syntax
            // parameters:
            //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
            //              如果==null，表示会自动使用this.stop，并自动OnStop+=
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetDbSyntax(
                null,
                bUseNewChannel,
                strServerName,
                strServerUrl,
                strBiblioDbName,
                out strSyntax,
                out strError);

            if (nRet == -1)
                return -1;

            return nRet;
        }

        // 顶层正好有所需的详细窗
        bool HasTopDetailForm(int index)
        {
            // 取出记录路径，析出书目库名，然后看这个书目库的syntax
            // 可能装入MARC和DC两种不同的窗口
            string strError = "";

#if NO
            // 防止重入
            if (m_bInSearching == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法装载记录。请稍后再试。";
                goto ERROR1;
            }
#endif

            string strSyntax = "";
            int nRet = GetOneRecordSyntax(index,
                this.m_bInSearching,
                out strSyntax,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strSyntax == "" // default = unimarc
                || strSyntax.ToLower() == "unimarc"
                || strSyntax.ToLower() == "usmarc")
            {
                if (this.MainForm.TopMarcDetailForm != null)
                    return true;
            }
            else if (strSyntax.ToLower() == "dc")
            {
                if (this.MainForm.TopDcForm != null)
                    return true;
            }
            else
            {
                strError = "未知的syntax '" + strSyntax + "'";
                goto ERROR1;
            }

            return false;
        ERROR1:
            // MessageBox.Show(this, strError);
        return false;
        }

        // parameters:
        //      bOpendNew   是否打开新的详细窗
        void LoadDetail(int index,
            bool bOpenNew = true)
        {
            // 取出记录路径，析出书目库名，然后看这个书目库的syntax
            // 可能装入MARC和DC两种不同的窗口
            string strError = "";

#if NO
            // 防止重入
            if (m_bInSearching == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法装载记录。请稍后再试。";
                goto ERROR1;
            }
#endif

            string strSyntax = "";
            int nRet = GetOneRecordSyntax(index,
                this.m_bInSearching,
                out strSyntax,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strSyntax == "" // default = unimarc
                || strSyntax.ToLower() == "unimarc"
                || strSyntax.ToLower() == "usmarc")
            {

                MarcDetailForm form = null;

                if (bOpenNew == false)
                    form = this.MainForm.TopMarcDetailForm;

                if (form == null)
                {
                    form = new MarcDetailForm();

                    form.MdiParent = this.MainForm;
                    form.MainForm = this.MainForm;

                    form.Show();
                }
                else
                    form.Activate();


                // MARC Syntax OID
                // 需要建立数据库配置参数，从中得到MARC格式
                ////form.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.1";   // UNIMARC

                form.LoadRecord(this, index);
            }
            else if (strSyntax.ToLower() == "dc")
            {

                DcForm form = null;

                if (bOpenNew == false)
                    form = this.MainForm.TopDcForm;

                if (form == null)
                {
                    form = new DcForm();

                    form.MdiParent = this.MainForm;
                    form.MainForm = this.MainForm;

                    form.Show();
                }
                else
                    form.Activate();

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


        /*
        // 获得书目记录的XML格式
        // parameters:
        //      strMarcSyntax 要创建的XML记录的marcsyntax。
        public static int GetBiblioXml(
            string strMarcSyntax,
            string strMARC,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            MemoryStream s = new MemoryStream();

            MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8);

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            if (strMarcSyntax == "unimarc")
            {
                writer.MarcNameSpaceUri = DpNs.unimarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else if (strMarcSyntax == "usmarc")
            {
                writer.MarcNameSpaceUri = Ns.usmarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else // 其他
            {
                writer.MarcNameSpaceUri = DpNs.unimarcxml;
                writer.MarcPrefix = "unimarc";
            }

            // string strMARC = this.MarcEditor.Marc;
            string strDebug = strMARC.Replace((char)Record.FLDEND, '#');
            int nRet = writer.WriteRecord(strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            writer.Flush();
            s.Flush();

            s.Seek(0, SeekOrigin.Begin);

            XmlDocument domMarc = new XmlDocument();
            try
            {
                domMarc.Load(s);
            }
            catch (Exception ex)
            {
                strError = "XML数据装入DOM时出错: " + ex.Message;
                return -1;
            }
            finally
            {
                //File.Delete(strTempFileName);
                s.Close();
            }

            strXml = domMarc.OuterXml;
            return 0;
        }
         * */

        // 获得一个数据库的数据syntax
        // parameters:
        //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
        //              如果==null，表示会自动使用this.stop，并自动OnStop+=
        //      bUseNewChannel  是否使用新的Channel对象。如果==false，表示尽量使用以前的
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetDbSyntax(
            Stop stop,
            bool bUseNewChannel,
            string strServerName,
            string strServerUrl,
            string strDbName,
            out string strSyntax,
            out string strError)
        {
            strSyntax = "";
            strError = "";

            bool bInitialStop = false;
            if (stop == null)
            {
                if (bUseNewChannel == true)
                {
                    stop = new Stop();
                    stop.Register(MainForm.stopManager, true);	// 和容器关联
                }
                else
                    stop = this.stop;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获得服务器 "+strServerUrl+" 的信息 ...");
                stop.BeginLoop();

                bInitialStop = true;
            }
            
            dp2ServerInfo info = null;

            try
            {
                info = this.MainForm.ServerInfos.GetServerInfo(stop,
                    bUseNewChannel,
                    this.Channels,
                    strServerName,
                    strServerUrl,
                    this.MainForm.TestMode,
                    out strError);
                if (info == null)
                    return -1;
            }
            finally
            {
                if (bInitialStop == true)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    if (bUseNewChannel == true)
                    {
                        stop.Unregister();	// 和容器关联
                        stop = null;
                    }
                }
            }

            for (int i = 0; i < info.BiblioDbProperties.Count; i++)
            {
                BiblioDbProperty prop = info.BiblioDbProperties[i];
                if (prop.DbName == strDbName)
                {
                    strSyntax = prop.Syntax;
                    return 1;
                }
            }

            return 0;   // not found dbname
        }


        // 获得publisher等实用库的库名
        public int GetUtilDbName(
            Stop stop,
            string strServerName,
            string strServerUrl,
            string strFuncName, // "publisher"
            out string strUtilDbName,
            out string strError)
        {
            strUtilDbName = "";
            strError = "";

            bool bInitialStop = false;
            if (stop == null)
            {
                stop = this.stop;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获得服务器 " + strServerUrl + " 的信息 ...");
                stop.BeginLoop();

                bInitialStop = true;
            }

            dp2ServerInfo info = null;

            try
            {
                info = this.MainForm.ServerInfos.GetServerInfo(stop,
                    this.m_bInSearching,
                    this.Channels,
                    strServerName,
                    strServerUrl,
                    this.MainForm.TestMode,
                    out strError);
                if (info == null)
                    return -1;
            }
            finally
            {
                if (bInitialStop == true)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }

            for (int i = 0; i < info.UtilDbProperties.Count; i++)
            {
                UtilDbProperty prop = info.UtilDbProperties[i];
                if (prop.Type == "publisher")
                {
                    strUtilDbName = prop.DbName;
                    return 1;
                }

            }

            return 0;    // not found
        }


        // 获得出版社相关信息
        public int GetPublisherInfo(
            string strServerName,
            string strPublisherNumber,
            out string str210,
            out string strError)
        {
            strError = "";
            str210 = "";

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得出版社信息 ...");
            stop.BeginLoop();

            try
            {
                string strDbName = "";

                // 获得publisher等实用库的库名
                int nRet = GetUtilDbName(
                    stop,
                    strServerName,
                    strServerUrl,
                    "publisher",
                    out strDbName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "尚未定义publisher类型的实用库名";
                    return -1;
                }

                this.Channel = this.Channels.GetChannel(strServerUrl);

                string strAction = "";

                long lRet = Channel.GetUtilInfo(
                    stop,
                    strAction,
                    strDbName,
                    "ISBN",
                    strPublisherNumber,
                    "v210",
                    out str210,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }


            return 1;
        }

        // 设置出版社相关信息
        public int SetPublisherInfo(
            string strServerName,
            string strPublisherNumber,
            string str210,
            out string strError)
        {
            strError = "";

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置出版社信息 ...");
            stop.BeginLoop();

            try
            {

                string strDbName = "";

                // 获得publisher等实用库的库名
                int nRet = GetUtilDbName(
                    stop,
                    strServerName,
                    strServerUrl,
                    "publisher",
                    out strDbName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "尚未定义publisher类型的实用库名";
                    return -1;
                }

                this.Channel = this.Channels.GetChannel(strServerUrl);

                string strAction = "";

                long lRet = Channel.SetUtilInfo(
                    stop,
                    strAction,
                    strDbName,
                    "ISBN",
                    "r",
                    "i",
                    "v210",
                    strPublisherNumber,
                    str210,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

        }

        // 获得102相关信息
        public int Get102Info(
            string strServerName,
            string strPublisherNumber,
            out string str102,
            out string strError)
        {
            strError = "";
            str102 = "";

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得102信息 ...");
            stop.BeginLoop();

            try
            {
                string strDbName = "";

                // 获得publisher等实用库的库名
                int nRet = GetUtilDbName(
                    stop,
                    strServerName,
                    strServerUrl,
                    "publisher",
                    out strDbName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "尚未定义publisher类型的实用库名";
                    return -1;
                }

                this.Channel = this.Channels.GetChannel(strServerUrl);


                string strAction = "";

                long lRet = Channel.GetUtilInfo(
                    stop,
                    strAction,
                    strDbName,
                    "ISBN",
                    strPublisherNumber,
                    "v102",
                    out str102,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }


            return 1;
        }

        // 设置102相关信息
        public int Set102Info(
            string strServerName,
            string strPublisherNumber,
            string str102,
            out string strError)
        {
            strError = "";

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置102信息 ...");
            stop.BeginLoop();

            try
            {
                string strDbName = "";

                // 获得publisher等实用库的库名
                int nRet = GetUtilDbName(
                    stop,
                    strServerName,
                    strServerUrl,
                    "publisher",
                    out strDbName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "尚未定义publisher类型的实用库名";
                    return -1;
                }

                this.Channel = this.Channels.GetChannel(strServerUrl);

                string strAction = "";

                long lRet = Channel.SetUtilInfo(
                    stop,
                    strAction,
                    strDbName,
                    "ISBN",
                    "r",
                    "i",
                    "v102",
                    strPublisherNumber,
                    str102,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

        }

        /*
        static string GetDbName(string strPurePath)
        {
            int nRet = 0;

            nRet = strPurePath.IndexOf("/");
            if (nRet != -1)
                return strPurePath.Substring(0, nRet).Trim();

            return strPurePath;
        }*/
        // 从路径中取出库名部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetDbName(string strPurePath)
        {
            int nRet = strPurePath.LastIndexOf("/");
            if (nRet == -1)
                return strPurePath;

            return strPurePath.Substring(0, nRet).Trim();
        }

        // 从路径中取出记录号部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

        // 记录路径是否为追加型？
        // 所谓追加型，就是记录ID部分为'?'，或者没有记录ID部分
        public static bool IsAppendRecPath(string strPath)
        {
            if (String.IsNullOrEmpty(strPath) == true)
                return true;

            string strRecordID = GetRecordID(strPath);
            if (String.IsNullOrEmpty(strRecordID) == true
                || strRecordID == "?")
                return true;

            return false;
        }

        public int GetChannelRights(
            string strServerName,
            out string strRights,               
            out string strError)
        {
            strError = "";
            strRights = "";

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            }
            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);
            if (string.IsNullOrEmpty(this.Channel.Rights) == true)
            {
                string strValue = "";
                long lRet = this.Channel.GetSystemParameter(stop,
                    "biblio",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                    return -1;
            }

            strRights = this.Channel.Rights;

            return 0;
        }


        public int ForceLogin(
    Stop stop,
    string strServerName,
    out string strError)
        {

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            }
            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);
            string strValue = "";
            long lRet = this.Channel.GetSystemParameter(stop,
                "biblio",
                "dbnames",
                out strValue,
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // 获得一个数据库的数据syntax
        // parameters:
        //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
        //              如果==null，表示会自动使用this.stop，并自动OnStop+=
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetDbSyntax(
            Stop stop,
            string strServerName,
            string strDbName,
            out string strSyntax,
            out string strError)
        {
            strSyntax = "";

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            }
            string strServerUrl = server.Url;

            // 获得一个数据库的数据syntax
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetDbSyntax(stop,
                this.m_bInSearching,
                strServerName,
                strServerUrl,
                strDbName,
                out strSyntax,
                out strError);
            if (nRet == -1)
                return -1;

            if (String.IsNullOrEmpty(strSyntax) == true)
                strSyntax = "unimarc";

            return 1;
        }

        // 保存记录
        // parameters:
        //      strPath 格式为"中文图书/7@本地服务器"
        //      strOriginSyntax 要保存 MARC 记录的原始 MARC 格式。如果为空，表示在函数中不核对记录和书目库的 MARC 格式是否一致
        //      strOutputPath   格式为"中文图书/7@本地服务器"
        //      strXmlFragment  书目以外的其余XML片断。注意，没有根元素
        // return:
        //      -2  timestamp mismatch
        //      -1  error
        //      0   succeed
        public int SaveMarcRecord(
            bool bUseLoop,
            string strPath,
            string strMARC,
            string strOriginSyntax,
            byte[] baTimestamp,
            string strXmlFragment,  // 书目以外的其余XML片断
            string strComment,
            out string strOutputPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;
            strOutputPath = "";

            int nRet = 0;

#if NO
            // 防止重入
            if (m_bInSearching == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法获得记录。请稍后再试。";
                return -1;
            }
#endif

            // 将strPath解析为server url和local path两个部分
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            }

            string strServerUrl = server.Url;

            Stop temp_stop = this.stop;
            LibraryChannel channel = null;

            bool bUseNewChannel = false;
            if (m_bInSearching == true)
            {
                channel = this.Channels.NewChannel(strServerUrl);
                bUseNewChannel = true;
                temp_stop = new Stop();
                temp_stop.Register(MainForm.stopManager, true);	// 和容器关联
            }
            else
            {
                this.Channel = this.Channels.GetChannel(strServerUrl);
                channel = this.Channel;
            }

            if (bUseLoop == true)
            {
                temp_stop.OnStop += new StopEventHandler(this.DoStop);
                temp_stop.Initial("正在初始化浏览器组件 ...");
                temp_stop.BeginLoop();

                this.Update();
                this.MainForm.Update();
            }


            try
            {
                string strDbName = GetDbName(strPurePath);
                string strSyntax = "";

                // 获得一个数据库的数据syntax
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetDbSyntax(this.stop,
                    bUseNewChannel,
                    strServerName,
                    strServerUrl,
                    strDbName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (String.IsNullOrEmpty(strSyntax) == true)
                    strSyntax = "unimarc";

                // 核对 MARC 格式
                if (string.IsNullOrEmpty(strOriginSyntax) == false)
                {
                    if (strOriginSyntax != strSyntax)
                    {
                        strError = "拟保存的记录的 MARC 格式为 '"+strOriginSyntax+"'，和目标书目库 '"+strDbName+"' 的 MARC 格式 '"+strSyntax+"' 不符合，无法保存 ";
                        goto ERROR1;
                    }
                }
/*
                nRet = MarcUtil.Marc2Xml(
    strMARC,
    strSyntax,
    out strXml,
    out strError);

                if (nRet == -1)
                    goto ERROR1;
 * */
                XmlDocument domMarc = null;
                nRet = MarcUtil.Marc2Xml(strMARC,
                    strSyntax,
                    out domMarc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                Debug.Assert(domMarc != null, "");

                // 合成其它XML片断
                if (string.IsNullOrEmpty(strXmlFragment) == false)
                {
                    XmlDocumentFragment fragment = domMarc.CreateDocumentFragment();
                    try
                    {
                        fragment.InnerXml = strXmlFragment;
                    }
                    catch (Exception ex)
                    {
                        strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                        return -1;
                    }

                    domMarc.DocumentElement.AppendChild(fragment);
                }

                string strXml = domMarc.OuterXml;

                string strAction = "change";

                if (IsAppendRecPath(strPurePath) == true)
                    strAction = "new";

                temp_stop.SetMessage("正在保存书目记录 " + strPath + " ...");

                string strOutputBiblioRecPath = "";

                long lRet = channel.SetBiblioInfo(
                    temp_stop,
                    strAction,
                    strPurePath,
                    "xml",
                    strXml,
                    baTimestamp,
                    strComment,
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp,
                    out strError);

                // 即便出错也有可能已经返回了路径 2013/6/17
                if (string.IsNullOrEmpty(strOutputBiblioRecPath) == false)
                    strOutputPath = strOutputBiblioRecPath + "@" + strServerName;

                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.TimestampMismatch)
                        return -2;   // timestamp mismatch
                    goto ERROR1;
                }

                // this.BiblioTimestamp = baTimestamp;


            }
            finally
            {
                temp_stop.Initial("");

                if (bUseLoop == true)
                {
                    temp_stop.EndLoop();
                    temp_stop.OnStop -= new StopEventHandler(this.DoStop);
                }

                if (bUseNewChannel == true)
                {
                    this.Channels.RemoveChannel(channel);
                    channel = null;

                    temp_stop.Unregister();	// 和容器关联
                    temp_stop = null;
                }
            }
            return 0;
        ERROR1:
            return -1;
        }


        // 保存记录
        // parameters:
        //      strPath 格式为"中文图书/7@本地服务器"
        //      strOutputPath   格式为"中文图书/7@本地服务器"
        public int SaveXmlRecord(
            string strPath,
            string strXml,
            byte[] baTimestamp,
            out string strOutputPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;
            strOutputPath = "";

            // int nRet = 0;

            // 将strPath解析为server url和local path两个部分
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                string strAction = "change";

                if (IsAppendRecPath(strPath) == true)
                    strAction = "new";


                stop.SetMessage("正在保存书目记录 " + strPath + " ...");

                string strOutputBiblioRecPath = "";

                long lRet = Channel.SetBiblioInfo(
                    stop,
                    strAction,
                    strPurePath,
                    "xml",
                    strXml,
                    baTimestamp,
                    "",
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                strOutputPath = strOutputBiblioRecPath + "@" + strServerName;
                // this.BiblioTimestamp = baTimestamp;


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

        // 包装后的版本
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetCfgFile(
            string strPath,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            return GetCfgFile(this.m_bInSearching,  // false,
                strPath,
                out strContent,
                out baOutputTimestamp,
                out strError);
        }


        // 获得配置文件
        // 是用到了CfgCache的
        // parameters:
        //      bNewChannel 是否要使用新创建的Channel?
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetCfgFile(
            bool bNewChannel,
            string strPath,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;
            strContent = "";

            // 将strPath解析为server url和local path两个部分
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            Stop temp_stop = this.stop;
            LibraryChannel channel = null;

            if (bNewChannel == false)
            {
                this.Channel = this.Channels.GetChannel(strServerUrl);
                channel = this.Channel;
            }
            else
            {
                channel = this.Channels.NewChannel(strServerUrl);
                temp_stop = new Stop();
                temp_stop.Register(MainForm.stopManager, true);	// 和容器关联
            }


            temp_stop.OnStop += new StopEventHandler(this.DoStop);
            temp_stop.Initial("正在下载配置文件 ...");
            temp_stop.BeginLoop();

            try
            {
                // string strPath = strBiblioDbName + "/cfgs/" + strCfgFileName;

                temp_stop.SetMessage("正在下载配置文件 " + strPurePath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = channel.GetRes(temp_stop,
                    MainForm.cfgCache,
                    strPurePath,
                    strStyle,
                    null,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    // 2011/6/21
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;
                    goto ERROR1;
                }

            }
            finally
            {
                temp_stop.EndLoop();
                temp_stop.OnStop -= new StopEventHandler(this.DoStop);
                temp_stop.Initial("");

                if (bNewChannel == true)
                {
                    this.Channels.RemoveChannel(channel);
                    channel = null;

                    temp_stop.Unregister();	// 和容器关联
                    temp_stop = null;
                }
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 保存配置文件
        public int SaveCfgFile(
            string strPath,
            /*
             * string strBiblioDbName,
            string strCfgFileName,
             * */
            string strContent,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";

            // 将strPath解析为server url和local path两个部分
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                return -1;
            } 
            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);



            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存配置文件 ...");
            stop.BeginLoop();

            try
            {
                // string strPath = strBiblioDbName + "/cfgs/" + strCfgFileName;

                stop.SetMessage("正在保存配置文件 " + strPurePath + " ...");

                byte[] output_timestamp = null;
                string strOutputPath = "";

                long lRet = Channel.WriteRes(
                    stop,
                    strPurePath,
                    strContent,
                    true,
                    "",	// style
                    baTimestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.LongRecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_browse.Columns,
                true);

            // 排序
            this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_browse.ListViewItemSorter = null;
        }

        // 捕获键盘的输入
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 回车
            if (keyData == Keys.Enter)
            {
                bool bClear = true; // 是否清除浏览窗中已有的内容

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    bClear = false;

                ClearListViewPropertyCache();
                if (bClear == true)
                {
                    if (this.m_nChangedCount > 0)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "当前命中记录列表中有 " + this.m_nChangedCount.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                            "dp2SearchForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.No)
                            return true;
                    }
                    this.ClearListViewItems();

                    ListViewUtil.ClearSortColumns(this.listView_browse);
                }

                this._linkMarcFile = null;


                if (this.textBox_simple_queryWord.Focused == true
                    || this.textBox_mutiline_queryContent.Focused == true)
                {
                    // 检索词那里回车
                    this.DoSearch();
                }
                else if (this.tabControl_query.SelectedTab == this.tabPage_logic
                    && this.dp2QueryControl1.Focused == true)
                {
                    this.DoLogicSearch();
                }
                else if (this.listView_browse.Focused == true)
                {
                    // 浏览框中回车
                    listView_browse_DoubleClick(this, null);
                }

                return true;
            }

            return false;
        }

        private void dp2SearchForm_Activated(object sender, EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            // 菜单
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            }
            else
            {
                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;
            }

            MainForm.MenuItem_font.Enabled = false;

            // 工具条按钮
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MainForm.toolButton_saveTo.Enabled = false;
                MainForm.toolButton_delete.Enabled = false;
            }
            else
            {
                MainForm.toolButton_saveTo.Enabled = true;
                MainForm.toolButton_delete.Enabled = true;
            }

            MainForm.toolButton_refresh.Enabled = true;
            MainForm.toolButton_loadFullRecord.Enabled = false;
        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
#if NO
            // 菜单动态变化
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MainForm.toolButton_saveTo.Enabled = false;
                MainForm.toolButton_delete.Enabled = false;

                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;

                MainForm.StatusBarMessage = "";
            }
            else
            {
                MainForm.toolButton_saveTo.Enabled = true;
                MainForm.toolButton_delete.Enabled = true;

                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;

                if (this.listView_browse.SelectedItems.Count == 1)
                {
                    MainForm.StatusBarMessage = "第 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行";
                }
                else
                {
                    MainForm.StatusBarMessage = "从 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_browse.SelectedItems.Count.ToString() + " 个事项";
                }
            }

            ListViewUtil.OnSeletedIndexChanged(this.listView_browse,
    0,
    null);
#endif

            this.commander.AddMessage(WM_SELECT_INDEX_CHANGED);

        }

        public void SaveOriginRecordToWorksheet()
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_browse.SelectedItems.Count == 0)
            {
                strError = "尚未选定要保存的记录";
                goto ERROR1;
            }

            // Encoding preferredEncoding = this.CurrentEncoding;

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的工作单文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = MainForm.LastWorksheetFileName;
            dlg.Filter = "工作单文件 (*.wor)|*.wor|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            /*
            Encoding targetEncoding = null;
            nRet = this.MainForm.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
             * */

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "dp2SearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            MainForm.LastWorksheetFileName = dlg.FileName;

            StreamWriter sw = null;

            try
            {
                // 创建文件
                sw = new StreamWriter(MainForm.LastWorksheetFileName,
                    bAppend,	// append
                    System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + MainForm.LastWorksheetFileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存到工作单格式 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, this.listView_browse.SelectedItems.Count);

                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
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

                    string strPath = this.listView_browse.SelectedItems[i].Text;

                    // byte[] baTarget = null;

                    string strRecord = "";
                    string strOutputPath = "";
                    string strOutStyle = "";
                    byte[] baTimestamp = null;
                    DigitalPlatform.Z3950.Record record = null;
                    Encoding currrentEncoding;
                    string strXmlFragment = "";

                    // 获得一条MARC/XML记录
                    // parameters:
                    //      strPath 记录路径。格式为"中文图书/1 @服务器名"
                    //      strDirection    方向。为 prev/next/current之一。current可以缺省。
                    //      strOutputPath   [out]返回的实际路径。格式和strPath相同。
                    // return:
                    //      -1  error 包括not found
                    //      0   found
                    //      1   为诊断记录
                    nRet = InternalGetOneRecord(
                        false,
                        "marc",
                        strPath,
                        "current",
                        "",
                        out strRecord,
                        out strXmlFragment,
                        out strOutputPath,
                        out strOutStyle,
                        out baTimestamp,
                        out record,
                        out currrentEncoding,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strMarcSyntax = "";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                        strMarcSyntax = "unimarc";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                        strMarcSyntax = "usmarc";

                    Debug.Assert(strMarcSyntax != "", "");

                    List<string> lines = null;
                    // 将机内格式变换为工作单格式 
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcUtil.CvtJineiToWorksheet(
                        strRecord,
                        -1,
                        out lines,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    foreach(string line in lines)
                    {
                        sw.WriteLine(line);
                    }

                    stop.SetProgressValue(i + 1);
                }


            }
            catch (Exception ex)
            {
                strError = "写入文件 " + MainForm.LastWorksheetFileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                sw.Close();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

            // 
            if (bAppend == true)
                MainForm.MessageText = this.listView_browse.SelectedItems.Count.ToString()
                    + "条记录成功追加到文件 " + MainForm.LastWorksheetFileName + " 尾部";
            else
                MainForm.MessageText = this.listView_browse.SelectedItems.Count.ToString()
                    + "条记录成功保存到新文件 " + MainForm.LastWorksheetFileName + " 尾部";

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public void SaveOriginRecordToIso2709()
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;

            if (this.listView_browse.SelectedItems.Count == 0)
            {
                strError = "尚未选定要保存的记录";
                goto ERROR1;
            }

            Encoding preferredEncoding = this.CurrentEncoding;

            string strPreferedMarcSyntax = "";

            if (this._linkMarcFile != null)
                strPreferedMarcSyntax = this._linkMarcFile.MarcSyntax;
            else
            {
                // 观察要保存的第一条记录的marc syntax
                nRet = GetOneRecordSyntax(0,
                    this.m_bInSearching,
                    out strPreferedMarcSyntax,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = true;
            dlg.FileName = MainForm.LastIso2709FileName;
            dlg.CrLf = MainForm.LastCrLfIso2709;
            dlg.RemoveField998 = MainForm.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(true);
            dlg.EncodingName =
                (String.IsNullOrEmpty(MainForm.LastEncodingName) == true ? GetEncodingForm.GetEncodingName(preferredEncoding) : MainForm.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
            
            if (string.IsNullOrEmpty(strPreferedMarcSyntax) == false)
                dlg.MarcSyntax = strPreferedMarcSyntax;
            else
                dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            
            if (bControl == false)
                dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            Encoding targetEncoding = null;

            if (dlg.EncodingName == "MARC-8"
                && preferredEncoding.Equals(this.MainForm.Marc8Encoding) == false)
            {
                strError = "保存操作无法进行。只有在记录的原始编码方式为 MARC-8 时，才能使用这个编码方式保存记录。";
                goto ERROR1;
            }

            nRet = this.MainForm.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            string strLastFileName = MainForm.LastIso2709FileName;
            string strLastEncodingName = MainForm.LastEncodingName;

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "dp2SearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性
            if (strLastFileName == dlg.FileName
                && bAppend == true)
            {
                if (strLastEncodingName != ""
                    && strLastEncodingName != dlg.EncodingName)
                {
                    DialogResult result = MessageBox.Show(this,
                        "文件 '" + dlg.FileName + "' 已在先前已经用 " + strLastEncodingName + " 编码方式存储了记录，现在又以不同的编码方式 " + dlg.EncodingName + " 追加记录，这样会造成同一文件中存在不同编码方式的记录，可能会令它无法被正确读取。\r\n\r\n是否继续? (是)追加  (否)放弃操作",
                        "dp2SearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃处理...";
                        goto ERROR1;
                    }

                }
            }

            MainForm.LastIso2709FileName = dlg.FileName;
            MainForm.LastCrLfIso2709 = dlg.CrLf;
            MainForm.LastEncodingName = dlg.EncodingName;
            MainForm.LastRemoveField998 = dlg.RemoveField998;


            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存到MARC文件 ...");
            stop.BeginLoop();


            Stream s = null;

            try
            {
                s = File.Open(MainForm.LastIso2709FileName,
                     FileMode.OpenOrCreate);
                if (bAppend == false)
                    s.SetLength(0);
                else
                    s.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + MainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            int nCount = 0;

            try
            {
                stop.SetProgressRange(0, this.listView_browse.SelectedItems.Count);
                bool bAsked = false;
                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
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

                    string strPath = this.listView_browse.SelectedItems[i].Text;

                    byte[] baTarget = null;

                    string strRecord = "";
                    string strOutputPath = "";
                    string strOutStyle = "";
                    byte[] baTimestamp = null;
                    DigitalPlatform.Z3950.Record record = null;
                    Encoding currrentEncoding;
                    string strXmlFragment = "";

                    // 获得一条MARC/XML记录
                    // parameters:
                    //      strPath 记录路径。格式为"中文图书/1 @服务器名"
                    //      strDirection    方向。为 prev/next/current之一。current可以缺省。
                    //      strOutputPath   [out]返回的实际路径。格式和strPath相同。
                    // return:
                    //      -1  error 包括not found
                    //      0   found
                    //      1   为诊断记录
                    nRet = InternalGetOneRecord(
                        false,
                        "marc",
                        strPath,
                        "current",
                        "",
                        out strRecord,
                        out strXmlFragment,
                        out strOutputPath,
                        out strOutStyle,
                        out baTimestamp,
                        out record,
                        out currrentEncoding,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strMarcSyntax = "";

                    if (dlg.MarcSyntax == "<自动>")
                    {
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        if (strMarcSyntax == "unimarc" && dlg.Mode880 == true
                            && bAsked == false)
                        {
                            DialogResult result = MessageBox.Show(this,
"书目记录 " + strPath + " 的 MARC 格式为 UNIMARC，在保存对话框选择“<自动>”的情况下，在保存前将不会被处理为 880 模式。如果确需在保存前处理为 880 模式，请终止当前操作，重新进行一次保存，注意在保存对话框中明确选择 “USMARC” 格式。\r\n\r\n请问是否继续处理? \r\n\r\n(Yes 继续处理，UNIMARC 格式记录不会处理为 880 模式；\r\nNo 中断整批保存操作)",
"BiblioSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto END1;
                            bAsked = true;
                        }
                    }
                    else
                    {
                        strMarcSyntax = dlg.MarcSyntax;
                        // TODO: 检查常用字段名和所选定的 MARC 格式是否矛盾。如果矛盾给出警告
                    }

                    Debug.Assert(strMarcSyntax != "", "");

                    if (dlg.RemoveField998 == true)
                    {
                        MarcRecord temp = new MarcRecord(strRecord);
                        temp.select("field[@name='998']").detach();
                        strRecord = temp.Text;
                    }

                    if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                    {
                        MarcRecord temp = new MarcRecord(strRecord);
                        MarcQuery.To880(temp);
                        strRecord = temp.Text;
                    }

                    // 将MARC机内格式转换为ISO2709格式
                    // parameters:
                    //      strSourceMARC   [in]机内格式MARC记录。
                    //      strMarcSyntax   [in]为"unimarc"或"usmarc"
                    //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
                    //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcUtil.CvtJineiToISO2709(
                        strRecord,
                        strMarcSyntax,
                        targetEncoding,
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    /*
                    Encoding sourceEncoding = connection.GetRecordsEncoding(
                        this.MainForm,
                        record.m_strSyntaxOID);


                    if (sourceEncoding.Equals(targetEncoding) == true)
                    {
                        // source和target编码方式相同，不用转换
                        baTarget = record.m_baRecord;
                    }
                    else
                    {
                        nRet = ChangeIso2709Encoding(
                            sourceEncoding,
                            record.m_baRecord,
                            targetEncoding,
                            strMarcSyntax,
                            out baTarget,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }*/

                    s.Write(baTarget, 0,
                        baTarget.Length);

                    if (dlg.CrLf == true)
                    {
                        byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                        s.Write(baCrLf, 0,
                            baCrLf.Length);
                    }

                    nCount++;

                    stop.SetProgressValue(i + 1);
                }
            }
            catch (Exception ex)
            {
                strError = "写入文件 " + MainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                s.Close();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);

            }

        END1:
            // 
            if (bAppend == true)
                MainForm.MessageText = nCount.ToString()
                    + "条记录成功追加到文件 " + MainForm.LastIso2709FileName + " 尾部";
            else
                MainForm.MessageText = nCount.ToString()
                    + "条记录成功保存到新文件 " + MainForm.LastIso2709FileName + " 尾部";

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 是否优先装入已经打开的详细窗?
        public bool LoadToExistDetailWindow
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "all_search_form",
                    "load_to_exist_detailwindow",
                    true);
            }
        }

        private void listView_browse_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;

            int nSelectedCount = 0;
            nSelectedCount = this.listView_browse.SelectedItems.Count;

            bool bHasTopDetailForm = false;
            if (nSelectedCount > 0)
                bHasTopDetailForm = HasTopDetailForm(this.listView_browse.SelectedIndices[0]);

            menuItem = new ToolStripMenuItem("装入已打开的记录窗(&L)");
            if (this.LoadToExistDetailWindow == true
                && bHasTopDetailForm == true)
                menuItem.Font = new Font(menuItem.Font, FontStyle.Bold);
            menuItem.Click += new System.EventHandler(this.menu_loadToOpenedDetailForm_Click);
            if (nSelectedCount == 0
                || bHasTopDetailForm == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("装入新开的记录窗(&L)");
            if (this.LoadToExistDetailWindow == false
                || bHasTopDetailForm == false)
                menuItem.Font = new Font(menuItem.Font, FontStyle.Bold);
            menuItem.Click += new System.EventHandler(this.menu_loadToNewDetailForm_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            ToolStripSeparator sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutToClipboard_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            menuItem = new ToolStripMenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("复制单列(&S)");
            if (this.listView_browse.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            for (int i = 0; i < this.listView_browse.Columns.Count; i++)
            {
                ToolStripMenuItem subMenuItem = new ToolStripMenuItem("复制列 '" + this.listView_browse.Columns[i].Text + "'");
                subMenuItem.Tag = i;
                subMenuItem.Click += new System.EventHandler(this.menu_copySingleColumnToClipboard_Click);
                menuItem.DropDownItems.Add(subMenuItem);
            }

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(string)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            menuItem = new ToolStripMenuItem("粘贴[前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertBefore_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("粘贴[后插](&V)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertAfter_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 全选
            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(menuItem_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("刷新所选择的 " + nSelectedCount.ToString() + " 个浏览行(&B)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItems_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);


            menuItem = new ToolStripMenuItem("移除所选择的 " + nSelectedCount.ToString() + " 个事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // bool bLooping = (stop != null && stop.State == 0);    // 0 表示正在处理

            // 批处理
            // 正在检索的时候，不允许进行批处理操作。因为stop.BeginLoop()嵌套后的Min Max Value之间的保存恢复问题还没有解决
            {
                menuItem = new ToolStripMenuItem("批处理(&B)");
                contextMenu.Items.Add(menuItem);

#if NO
                ToolStripMenuItem subMenuItem = new MenuItem("快速修改书目记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&Q)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickChangeRecords_Click);
                if (this.listView_browse.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);

                
                // ---
                sep = new ToolStripSeparator();
                menuItem.DropDownItems.Add(sep);
#endif


                ToolStripMenuItem subMenuItem = new ToolStripMenuItem("执行 MarcQuery 脚本 [" + this.listView_browse.SelectedItems.Count.ToString() + "] (&M)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickMarcQueryRecords_Click);
                if (this.listView_browse.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);

                subMenuItem = new ToolStripMenuItem("丢弃修改 [" + this.listView_browse.SelectedItems.Count.ToString() + "] (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_clearSelectedChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.listView_browse.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);

                subMenuItem = new ToolStripMenuItem("丢弃全部修改 [" + this.m_nChangedCount.ToString() + "] (&L)");
                subMenuItem.Click += new System.EventHandler(this.menu_clearAllChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);

                subMenuItem = new ToolStripMenuItem("保存选定的修改 [" + this.listView_browse.SelectedItems.Count.ToString() + "] (&S)");
                subMenuItem.Click += new System.EventHandler(this.menu_saveSelectedChangedRecords_Click);
                if (this._linkMarcFile != null || this.m_nChangedCount == 0 || this.listView_browse.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);

                subMenuItem = new ToolStripMenuItem("保存全部修改 [" + this.m_nChangedCount.ToString() + "] (&A)");
                subMenuItem.Click += new System.EventHandler(this.menu_saveAllChangedRecords_Click);
                if (this._linkMarcFile != null || this.m_nChangedCount == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);

                subMenuItem = new ToolStripMenuItem("创建新的 MarcQuery 脚本文件 (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_createMarcQueryCsFile_Click);
                menuItem.DropDownItems.Add(subMenuItem);

                // ---
                sep = new ToolStripSeparator();
                menuItem.DropDownItems.Add(sep);

                subMenuItem = new ToolStripMenuItem("删除所选择的 " + nSelectedCount.ToString() + " 个书目记录(&D)");
                subMenuItem.Click += new System.EventHandler(this.menu_deleteSelectedRecords_Click);
                if (this._linkMarcFile != null
                    || nSelectedCount == 0
                    || this.m_bInSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);


                // ---
                sep = new ToolStripSeparator();
                menuItem.DropDownItems.Add(sep);

                // 追加保存到数据库
                subMenuItem = new ToolStripMenuItem("将选定的 " + nSelectedCount.ToString() + " 条记录以追加方式保存到数据库(&A)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToDatabase_Click);
                if (nSelectedCount == 0)
                    subMenuItem.Enabled = false;
                menuItem.DropDownItems.Add(subMenuItem);
            }

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("导出所选择的 " + nSelectedCount.ToString() + " 个事项到记录路径文件(&S)...");
            menuItem.Click += new System.EventHandler(this.menu_saveToRecordPathFile_Click);
            if (this._linkMarcFile != null || nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("从记录路径文件导入(&I)...");
            menuItem.Click += new System.EventHandler(this.menu_importFromRecPathFile_Click);
            if (this.m_bInSearching == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("从 MARC 文件导入(&M)...");
            menuItem.Click += new System.EventHandler(this.menu_importFromMarcFile_Click);
            if (this.m_bInSearching == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 保存原始记录到工作单文件
            menuItem = new ToolStripMenuItem("保存选定的 "
                + nSelectedCount.ToString()
                + " 条记录到工作单文件(&W)");
            if (nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToWorksheet_Click);
            contextMenu.Items.Add(menuItem);


            // 保存原始记录到ISO2709文件
            menuItem = new ToolStripMenuItem("保存选定的 "
                + nSelectedCount.ToString()
                + " 条记录到 MARC 文件(&S)");
            if (nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToIso2709_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("清除浏览过滤器缓存(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearBrowseFltxCache_Click);
            if (this.m_bInSearching == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.listView_browse, e.Location);
        }

        void menu_clearBrowseFltxCache_Click(object sender, EventArgs e)
        {
            this.Filters.Clear();
        }

        void menu_copySingleColumnToClipboard_Click(object sender, EventArgs e)
        {
            int nColumn = (int)((ToolStripMenuItem)sender).Tag;

            Global.CopyLinesToClipboard(this, nColumn, this.listView_browse, false);
        }

        #region MarcQuery


#if NO
        // 从 .ref 获取附加的库文件路径
        int GetRef(string strCsFileName,
            ref string[] refs,
            out string strError)
        {
            strError = "";

            string strRefFileName = strCsFileName + ".ref";

            // .ref文件可以缺省
            if (File.Exists(strRefFileName) == false)
                return 0;   // .ref 文件不存在

            string strRef = "";
            try
            {
                using (StreamReader sr = new StreamReader(strRefFileName, true))
                {
                    strRef = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            // 提前检查
            string[] add_refs = null;
            int nRet = ScriptManager.GetRefsFromXml(strRef,
                out add_refs,
                out strError);
            if (nRet == -1)
            {
                strError = strRefFileName + " 文件内容(应为XML格式)格式错误: " + strError;
                return -1;
            }

            // 兑现宏
            if (add_refs != null)
            {
                for (int i = 0; i < add_refs.Length; i++)
                {
                    add_refs[i] = add_refs[i].Replace("%bindir%", Environment.CurrentDirectory);
                }
            }

            refs = Append(refs, add_refs);
            return 1;
        }
#endif

        // 准备脚本环境
        // TODO: 检测同名的 .ref 文件
        int PrepareMarcQuery(string strCsFileName,
            out Assembly assembly,
            out MarcQueryHost host,
            out string strError)
        {
            assembly = null;
            strError = "";
            host = null;


            string strContent = "";
            Encoding encoding;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strCsFileName,
                -1,
                out strContent,
                out encoding,
                out strError);
            if (nRet == -1)
                return -1;

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",
                                    // "D:\\Program Files\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.0\\WindowsBase.dll",
                                    ExcelUtil.GetWindowsBaseDllPath(),

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									Environment.CurrentDirectory + "\\documentformat.openxml.dll",
                                    Environment.CurrentDirectory + "\\dp2catalog.exe",
            };

            nRet = ScriptManager.GetRef(strCsFileName,
                ref saAddRef,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strTemp = ExcelUtil.GetWindowsBaseDllPath();

            string strWarningInfo = "";
            // 直接编译到内存
            // parameters:
            //		refs	附加的refs文件路径。路径中可能包含宏%installdir%
            nRet = ScriptManager.CreateAssembly_1(strContent,
                saAddRef,
                "", // strLibPath,
                out assembly,
                out strError,
                out strWarningInfo);
            if (nRet == -1)
                goto ERROR1;

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Catalog.MarcQueryHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Catalog.MarcQueryHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (MarcQueryHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        void menu_quickMarcQueryRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_browse.SelectedItems.Count == 0)
            {
                strError = "尚未选择要执行 MarcQuery 脚本的事项";
                goto ERROR1;
            }

            // 书目信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 MarcQuery 脚本文件";
            dlg.FileName = this.m_strUsedMarcQueryFilename;
            dlg.Filter = "MarcQuery 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedMarcQueryFilename = dlg.FileName;

            MarcQueryHost host = null;
            Assembly assembly = null;

            nRet = PrepareMarcQuery(this.m_strUsedMarcQueryFilename,
                out assembly,
                out host,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            host.MainForm = this.MainForm;
            host.UiForm = this;
            host.CodeFileName = this.m_strUsedMarcQueryFilename;    // 2013/10/8

            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行脚本 " + dlg.FileName + "</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对书目记录执行 MarcQuery 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_browse.Enabled = false;
            try
            {

                // Initial
            {
                host.RecordPath = "";
                host.MarcRecord = null;
                host.MarcSyntax = "";
                host.Changed = false;
                host.UiItem = null;

                StatisEventArgs args = new StatisEventArgs();
                host.OnInitial(this, args);
                if (args.Continue == ContinueType.SkipAll)
                    return;
                if (args.Continue == ContinueType.Error)
                {
                    strError = args.ParamString;
                    goto ERROR1;
                }
            }


                if (stop != null)
                    stop.SetProgressRange(0, this.listView_browse.SelectedItems.Count);

                {
                    host.RecordPath = "";
                    host.MarcRecord = null;
                    host.MarcSyntax = "";
                    host.Changed = false;
                    host.UiItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnBegin(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        return;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_browse.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(this.Channels,
                    this.dp2ResTree1.Servers,
                    stop,
                    items,
                    this.m_biblioTable);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    string strMARC = "";
                    string strMarcSyntax = "";
                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(info.OldXml,
                        true,
                        null,
                        out strMarcSyntax,
                        out strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }

                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    string strOuterFieldDef = "";
                    if (strMarcSyntax == "unimarc")
                        strOuterFieldDef = "4**";

                    host.RecordPath = info.RecPath;
                    host.MarcRecord = new MarcRecord(strMARC, strOuterFieldDef);
                    host.MarcSyntax = strMarcSyntax;
                    host.Changed = false;
                    host.UiItem = item.ListViewItem;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnRecord(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        break;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }

                    if (host.Changed == true)
                    {
                        string strXml = info.OldXml;
                        nRet = MarcUtil.Marc2XmlEx(host.MarcRecord.Text,
                            strMarcSyntax,
                            ref strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                            info.NewVersion = DateTime.Now.Ticks;
                        }

                        item.ListViewItem.BackColor = SystemColors.Info;
                        item.ListViewItem.ForeColor = SystemColors.InfoText;
                    }

                    // 显示为工作单形式
                    i++;
                }

                {
                    host.RecordPath = "";
                    host.MarcRecord = null;
                    host.MarcSyntax = "";
                    host.Changed = false;
                    host.UiItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnEnd(this, args);
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "执行 MarcQuery 脚本的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                if (host != null)
                    host.FreeResources();

                this.listView_browse.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行脚本 " + dlg.FileName + "</div>");
            }

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 丢弃选定的修改
        void menu_clearSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可丢弃");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.listView_browse.SelectedItems)
                {
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        continue;

                    if (String.IsNullOrEmpty(info.NewXml) == false)
                    {
                        info.NewXml = "";

                        item.BackColor = SystemColors.Window;
                        item.ForeColor = SystemColors.WindowText;

                        this.m_nChangedCount--;
                        Debug.Assert(this.m_nChangedCount >= 0, "");
                    }

                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
        }

        // 丢弃全部修改
        void menu_clearAllChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可丢弃");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.listView_browse.Items)
                {
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        continue;

                    if (String.IsNullOrEmpty(info.NewXml) == false)
                    {
                        info.NewXml = "";

                        item.BackColor = SystemColors.Window;
                        item.ForeColor = SystemColors.WindowText;

                        this.m_nChangedCount--;
                        Debug.Assert(this.m_nChangedCount >= 0, "");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
        }

        // 保存选定事项的修改
        // 注:不能保存回到(原先装入来自的) MARC 文件
        void menu_saveSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?
            string strError = "";

            if (this._linkMarcFile != null)
            {
                strError = "暂不允许保存回 MARC 文件";
                goto ERROR1;
            }
            if (this.m_nChangedCount == 0)
            {
                strError = "当前没有任何修改过的事项需要保存";
                goto ERROR1;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_browse.SelectedItems)
            {
                items.Add(item);
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 保存全部修改事项
        // 注:不能保存回到(原先装入来自的) MARC 文件
        void menu_saveAllChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?
            string strError = "";

            if (this._linkMarcFile != null)
            {
                strError = "暂不允许保存回 MARC 文件";
                goto ERROR1;
            }
            if (this.m_nChangedCount == 0)
            {
                strError = "当前没有任何修改过的事项需要保存";
                goto ERROR1;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_browse.Items)
            {
                items.Add(item);
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        int SaveChangedRecords(List<ListViewItem> items,
    out string strError)
        {
            strError = "";

            int nReloadCount = 0;
            int nSavedCount = 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存书目记录 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            this.listView_browse.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "已中断";
                        return -1;
                    }

                    ListViewItem item = items[i];
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                    {
                        stop.SetProgressValue(i);
                        goto CONTINUE;
                    }

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        goto CONTINUE;

                    if (string.IsNullOrEmpty(info.NewXml) == true)
                        goto CONTINUE;

                    stop.SetMessage("正在保存书目记录 " + strRecPath);

                    // 解析记录路径
                    string strServerName = "";
                    string strPurePath = "";
                    ParseRecPath(strRecPath,
                        out strServerName,
                        out strPurePath);

                    // 获得server url
                    dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
                    if (server == null)
                    {
                        strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                        return -1;
                    }
                    string strServerUrl = server.Url;

                    this.Channel = this.Channels.GetChannel(strServerUrl);

                    string strOutputPath = "";

                    byte[] baNewTimestamp = null;

                    long lRet = Channel.SetBiblioInfo(
                        stop,
                        "change",
                        strPurePath,
                        "xml",
                        info.NewXml,
                        info.Timestamp,
                        "",
                        out strOutputPath,
                        out baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (Channel.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            DialogResult result = MessageBox.Show(this,
    "保存书目记录 " + strRecPath + " 时遭遇时间戳不匹配: " + strError + "。\r\n\r\n此记录已无法被保存。\r\n\r\n请问现在是否要顺便重新装载此记录? \r\n\r\n(Yes 重新装载；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
    "BiblioSearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto CONTINUE;

                            // 重新装载书目记录到 OldXml
                            string[] results = null;
                            // byte[] baTimestamp = null;
                            lRet = Channel.GetBiblioInfos(
                                stop,
                                strPurePath,
                                "",
                                new string[] { "xml" },   // formats
                                out results,
                                out baNewTimestamp,
                                out strError);
                            if (lRet == 0)
                            {
                                // TODO: 警告后，把 item 行移除？
                                return -1;
                            }
                            if (lRet == -1)
                                return -1;
                            if (results == null || results.Length == 0)
                            {
                                strError = "results error";
                                return -1;
                            }
                            info.OldXml = results[0];
                            info.Timestamp = baNewTimestamp;
                            nReloadCount++;
                            goto CONTINUE;
                        }

                        return -1;
                    }

                    // 检查是否有部分字段被拒绝
                    if (Channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        DialogResult result = MessageBox.Show(this,
"保存书目记录 " + strRecPath + " 时部分字段被拒绝。\r\n\r\n此记录已部分保存成功。\r\n\r\n请问现在是否要顺便重新装载此记录以便观察? \r\n\r\n(Yes 重新装载(到旧记录部分)；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
"BiblioSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;
                        // 重新装载书目记录到 OldXml
                        string[] results = null;
                        // byte[] baTimestamp = null;
                        lRet = Channel.GetBiblioInfos(
                            stop,
                            strPurePath,
                            "",
                            new string[] { "xml" },   // formats
                            out results,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == 0)
                        {
                            // TODO: 警告后，把 item 行移除？
                            return -1;
                        }
                        if (lRet == -1)
                            return -1;
                        if (results == null || results.Length == 0)
                        {
                            strError = "results error";
                            return -1;
                        }
                        info.OldXml = results[0];
                        info.Timestamp = baNewTimestamp;
                        nReloadCount++;
                        goto CONTINUE;
                    }

                    info.Timestamp = baNewTimestamp;
                    info.OldXml = info.NewXml;
                    info.NewXml = "";

                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;

                    nSavedCount++;

                    this.m_nChangedCount--;
                    Debug.Assert(this.m_nChangedCount >= 0, "");

                CONTINUE:
                    stop.SetProgressValue(i);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
                this.listView_browse.Enabled = true;
            }

            //2013/10/22
            int nRet = RefreshListViewLines(items,
    out strError);
            if (nRet == -1)
                return -1;

            DoViewComment(false);

            strError = "";
            if (nSavedCount > 0)
                strError += "共保存书目记录 " + nSavedCount + " 条";
            if (nReloadCount > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += " ; ";
                strError += "有 " + nReloadCount + " 条书目记录因为时间戳不匹配或部分字段被拒绝而重新装载旧记录部分(请观察后重新保存)";
            }

            return 0;
        }

        string m_strUsedMarcQueryFilename = "";

        // 创建新的 MarcQuery 脚本文件
        void menu_createMarcQueryCsFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的脚本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "C#脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                MarcQueryHost.CreateStartCsFile(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }

            System.Diagnostics.Process.Start("notepad.exe", dlg.FileName);

            this.m_strUsedMarcQueryFilename = dlg.FileName;

        }

        // 删除所选择的书目记录
        public void DeleteSelectedRecords()
        {
            string strError = "";

            if (this._linkMarcFile != null)
            {
                strError = "暂不支持从 MARC 文件中直接删除记录";
                goto ERROR1;
            }

            if (this.listView_browse.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的书目记录";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要从数据库中删除所选定的 " + this.listView_browse.SelectedItems.Count.ToString() + " 个书目记录?\r\n\r\n(警告：书目记录被删除后，无法恢复。如果删除书目记录，则其下属的册、期、订购、评注记录和对象资源会一并删除)\r\n\r\n(OK 删除；Cancel 取消)",
"dp2SearchForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_browse.SelectedItems)
            {
                items.Add(item);
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除书目记录 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);

            // 暂时禁止因为 listview 选择发生改变而频繁刷新状态行
            this.listView_browse.SelectedIndexChanged -= new System.EventHandler(this.listView_browse_SelectedIndexChanged);

            try
            {
                stop.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "已中断";
                            goto ERROR1;
                        }
                    }

                    ListViewItem item = items[i];
                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    // 解析记录路径
                    string strServerName = "";
                    string strPurePath = "";
                    ParseRecPath(strRecPath,
                        out strServerName,
                        out strPurePath);

                    // 获得server url
                    dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
                    if (server == null)
                    {
                        strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                        goto ERROR1;
                    }
                    string strServerUrl = server.Url;

                    this.Channel = this.Channels.GetChannel(strServerUrl);

                    string[] results = null;
                    byte[] baTimestamp = null;
                    string strOutputPath = "";

                    stop.SetMessage("正在删除书目记录 " + strPurePath);

                    long lRet = Channel.GetBiblioInfos(
                        stop,
                        strPurePath,
                        "",
                        null,   // formats
                        out results,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        result = MessageBox.Show(this,
    "在获得记录 '" + strRecPath + "' 的时间戳的过程中出现错误: " + strError + "。\r\n\r\n是否继续强行删除此记录? (Yes 强行删除；No 不删除；Cancel 放弃当前未完成的全部删除操作)",
    "dp2SearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            goto ERROR1;
                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
                    } 
                    if (lRet == -1 || lRet == 0)
                        goto ERROR1;

                    byte[] baNewTimestamp = null;

                    lRet = Channel.SetBiblioInfo(
                        stop,
                        "delete",
                        strPurePath,
                        "xml",
                        "", // strXml,
                        baTimestamp,
                        "",
                        out strOutputPath,
                        out baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    stop.SetProgressValue(i);

                    this.listView_browse.Items.Remove(item);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
                this.listView_browse.SelectedIndexChanged += new System.EventHandler(this.listView_browse_SelectedIndexChanged);
            }

            MessageBox.Show(this, "成功删除书目记录 " + items.Count + " 条");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_deleteSelectedRecords_Click(object sender, EventArgs e)
        {
            DeleteSelectedRecords();
        }

        LinkMarcFile _linkMarcFile = null;

        public LinkMarcFile LinkMarcFile
        {
            get
            {
                return this._linkMarcFile;
            }
        }

        // 从 MARC 文件中导入
        void menu_importFromMarcFile_Click(object sender, EventArgs e)
        {
            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = false;
            dlg.GetEncoding -= new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.GetEncoding += new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.FileName = this.MainForm.LinkedMarcFileName;
            // dlg.CrLf = MainForm.LastCrLfIso2709;
            dlg.EncodingListItems = Global.GetEncodingList(true);
            // dlg.EncodingName = ""; GetEncodingForm.GetEncodingName(preferredEncoding);
            // dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
            // dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = true;

            if (String.IsNullOrEmpty(this.MainForm.LinkedEncodingName) == false)
                dlg.EncodingName = this.MainForm.LinkedEncodingName;
            if (String.IsNullOrEmpty(this.MainForm.LinkedMarcSyntax) == false)
                dlg.MarcSyntax = this.MainForm.LinkedMarcSyntax;

            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 储存用过的文件名
            // 2009/9/21
            this.MainForm.LinkedMarcFileName = dlg.FileName;
            this.MainForm.LinkedEncodingName = dlg.EncodingName;
            this.MainForm.LinkedMarcSyntax = dlg.MarcSyntax;

            string strError = "";


            _linkMarcFile = new LinkMarcFile();
            int nRet = _linkMarcFile.Open(dlg.FileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;



            _linkMarcFile.Encoding = dlg.Encoding;
            _linkMarcFile.MarcSyntax = dlg.MarcSyntax;

            ClearListViewItems();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在从 MARC 文件导入 ...");
            stop.BeginLoop();


            this.listView_browse.BeginUpdate();
            try
            {

                ListViewUtil.ClearSortColumns(this.listView_browse);
                stop.SetProgressRange(0, _linkMarcFile.Stream.Length);


                bool bEOF = false;
                for (int i = 0; bEOF == false; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strMARC = "";
                    byte[] baRecord = null;
                    // 获得下一条记录
                    // return:
                    //      -1  error
                    //      0   succeed
                    //      1   reach end(当前返回的记录有效)
                    //	    2	结束(当前返回的记录无效)
                    nRet = _linkMarcFile.NextRecord(out strMARC,
                        out baRecord,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        bEOF = true;
                    if (nRet == 2)
                        break;

                    if (_linkMarcFile.MarcSyntax == "<自动>"
    || _linkMarcFile.MarcSyntax.ToLower() == "<auto>")
                    {
                        // 自动识别MARC格式
                        string strOutMarcSyntax = "";
                        // 探测记录的MARC格式 unimarc / usmarc / reader
                        // return:
                        //      0   没有探测出来。strMarcSyntax为空
                        //      1   探测出来了
                        nRet = MarcUtil.DetectMarcSyntax(strMARC,
                            out strOutMarcSyntax);
                        _linkMarcFile.MarcSyntax = strOutMarcSyntax;    // 有可能为空，表示探测不出来
                        if (String.IsNullOrEmpty(_linkMarcFile.MarcSyntax) == true)
                        {
                            MessageBox.Show(this, "软件无法确定此 MARC 文件的 MARC 格式");
                        }
                    }

                    if (dlg.Mode880 == true && _linkMarcFile.MarcSyntax == "usmarc")
                    {
                        MarcRecord temp = new MarcRecord(strMARC);
                        MarcQuery.ToParallel(temp);
                        strMARC = temp.Text;
                    }

                    string strXml = "";
                    nRet = MarcUtil.Marc2XmlEx(strMARC,
        _linkMarcFile.MarcSyntax,
        ref strXml,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strRecPath = i.ToString() + " @file";
                    BiblioInfo info = new BiblioInfo();
                    info.RecPath = strRecPath;
                    this.m_biblioTable[strRecPath] = info;

                    info.OldXml = strXml;
                    info.Timestamp = null;
                    info.RecPath = strRecPath;

                    string strSytaxOID = "";

                    if (_linkMarcFile.MarcSyntax == "unimarc")
                        strSytaxOID = "1.2.840.10003.5.1";                // unimarc
                    else if (_linkMarcFile.MarcSyntax == "usmarc")
                        strSytaxOID = "1.2.840.10003.5.10";               // usmarc

                    string strBrowseText = "";
                    if (strSytaxOID == "1.2.840.10003.5.1"    // unimarc
        || strSytaxOID == "1.2.840.10003.5.10")  // usmarc
                    {
                        nRet = BuildMarcBrowseText(
                            strSytaxOID,
                            strMARC,
                            out strBrowseText,
                            out strError);
                        if (nRet == -1)
                            strBrowseText = strError;
                    }

                    string[] cols = strBrowseText.Split(new char[] { '\t' });

                    // 创建浏览行
                    NewLine(
        this.listView_browse,
        strRecPath,
        cols);
                    stop.SetMessage(i.ToString());
                    stop.SetProgressValue(_linkMarcFile.Stream.Position);
                }
            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                _linkMarcFile.Close();
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            _linkMarcFile = null;
        }

        void dlg_GetEncoding(object sender, GetEncodingEventArgs e)
        {
            string strError = "";
            Encoding encoding = null;
            int nRet = this.MainForm.GetEncoding(e.EncodingName,
                out encoding,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }
            e.Encoding = encoding;
        }

        // 创建MARC格式记录的浏览格式
        // paramters:
        //      strMARC MARC机内格式
        public int BuildMarcBrowseText(
            string strSytaxOID,
            string strMARC,
            out string strBrowseText,
            out string strError)
        {
            strBrowseText = "";
            strError = "";

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this.MainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = this.MainForm.DataDir + "\\" + strSytaxOID.Replace(".", "_") + "\\marc_browse.fltx";

            int nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBrowseText = host.ResultString;

            }
            finally
            {
                // 归还对象
                this.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            // 不让缓存，因为可能出现了编译错误
            // TODO: 精确区分编译错误
            this.Filters.ClearFilter(strFilterFileName);
            return -1;
        }

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
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string[] saAddRef1 = {
										 this.BinDir + "\\digitalplatform.marcdom.dll",
										 // this.BinDir + "\\digitalplatform.marckernel.dll",
										 // this.BinDir + "\\digitalplatform.libraryserver.dll",
										 this.BinDir + "\\digitalplatform.dll",
										 this.BinDir + "\\digitalplatform.Text.dll",
										 this.BinDir + "\\digitalplatform.IO.dll",
										 this.BinDir + "\\digitalplatform.Xml.dll",
										 this.BinDir + "\\dp2catalog.exe" };

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

            return 0;
        ERROR1:
            return -1;
        }


        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的书目记录路径文件名";
            dlg.FileName = this.m_strUsedRecPathFilename;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedRecPathFilename = dlg.FileName;

            StreamReader sr = null;
            string strError = "";
            bool bSkipBrowse = false;    // 测试

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + dlg.FileName + " 失败: " + ex.Message;
                goto ERROR1;
            }

            int nDupCount = 0;  // 发现的全部重复的记录数。两条记录发生重复，第一条不算在内
            int nSkipDupCount = 0;  // 已经跳过的重复记录数

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入记录路径 ...");
            stop.BeginLoop();

            this.m_bInSearching = true; // 防止中间点某行导致 GetBiblioInfo() 和这里的循环冲突

            this.EnableControlsInSearching(false);
            this.listView_browse.BeginUpdate();
            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_browse);
                stop.SetProgressRange(0, sr.BaseStream.Length);

                if (this.listView_browse.Items.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this.listView_browse.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        "dp2SearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                bool bHideMessageBox = false;
                DialogResult dup_result = System.Windows.Forms.DialogResult.OK;
                Hashtable dup_table = new Hashtable();  // 检查记录路径是否有重复的 Hashtable

                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }


                    string strRecPath = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);

                    if (strRecPath == null)
                        break;

                    // TODO: 检查路径的正确性，检查数据库是否为书目库之一
                    // 解析记录路径
                    string strServerUrl = "";
                    string strPurePath = "";
                    ParseRecPath(strRecPath,
                        out strServerUrl,
                        out strPurePath);
                    dp2Server server = this.dp2ResTree1.Servers.GetServer(strServerUrl);
                    if (server == null)
                    {
                        strError = "URL 为 '" + strServerUrl + "' 的服务器在检索窗中尚未定义...";
                        goto ERROR1;
                    }

                    strRecPath = strPurePath + "@" + server.Name;

                    // 2014/4/4
                    if (dup_table[strRecPath] != null)
                    {
                        nDupCount++;
                        if (bHideMessageBox == false)
                        {
                            // this.listView_browse.ForceUpdate();
                            Application.DoEvents();

                            dup_result = MessageDialog.Show(this,
    "书目记录 " + strRecPath + " 和已经装入的记录重复了。请问是否装入重复的记录?",
    MessageBoxButtons.YesNoCancel,
    MessageBoxDefaultButton.Button1,
    "以后不再提示，按本次的选择处理",
    ref bHideMessageBox,
    new string[] { "装入", "跳过", "中断" });
                        }

                        if (dup_result == System.Windows.Forms.DialogResult.No)
                        {
                            nSkipDupCount++;
                            continue;
                        }
                        if (dup_result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                    }
                    else
                        dup_table[strRecPath] = true;

                    if (nSkipDupCount > 0)
                        stop.SetMessage("正在导入路径 " + strRecPath + " (已跳过重复记录 " + nSkipDupCount.ToString() + " 条)" );
                    else
                        stop.SetMessage("正在导入路径 " + strRecPath);

                    ListViewItem item = new ListViewItem();
                    item.Text = strRecPath;

                    this.listView_browse.Items.Add(item);

                    if (bSkipBrowse == false
                        && !(Control.ModifierKeys == Keys.Control))
                    {
                        int nRet = RefreshOneLine(item,
                out strError);
                        if (nRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
        "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 获取；No 不获取；Cancel 放弃导入)",
        "dp2SearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                bSkipBrowse = true;
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                            {
                                strError = "已中断";
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                this.listView_browse.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.m_bInSearching = false;

                this.EnableControlsInSearching(true);

                if (sr != null)
                    sr.Close();
            }

            if (nSkipDupCount > 0)
                MessageBox.Show(this, "装入成功。跳过重复记录 "+nSkipDupCount+" 条");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 调用前，记录路径列已经有值
        int RefreshOneLine(ListViewItem item,
            out string strError)
        {
            strError = "";

            string strPath = ListViewUtil.GetItemText(item, 0);

            // 解析记录路径
            string strServerName = "";
            string strPurePath = "";
            ParseRecPath(strPath,
                out strServerName,
                out strPurePath);

            // 获得server url
            dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
            if (server == null)
            {
                strError = "名为 '"+strServerName+"' 的服务器在检索窗中尚未定义...";
                return -1;
            }
            string strServerUrl = server.Url;

            this.Channel = this.Channels.GetChannel(strServerUrl);


            string[] paths = new string[1];
            paths[0] = strPurePath;
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

            for (int i = 0; i < searchresults[0].Cols.Length; i++)
            {
                ListViewUtil.ChangeItemText(item,
                    i + 1,
                    searchresults[0].Cols[i]);
            }

            return 0;
        }

        DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = null;

            dtlp_searchform = this.MainForm.TopDtlpSearchForm;

            if (dtlp_searchform == null)
            {
                // 新开一个dtlp检索窗
                FormWindowState old_state = this.WindowState;

                dtlp_searchform = new DtlpSearchForm();
                dtlp_searchform.MainForm = this.MainForm;
                dtlp_searchform.MdiParent = this.MainForm;
                dtlp_searchform.WindowState = FormWindowState.Minimized;
                dtlp_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dtlp_searchform.WaitLoadFinish();
            }

            return dtlp_searchform;
        }

        void dlg_GetDp2SearchParam(object sender, GetDp2SearchParamEventArgs e)
        {
            e.dp2Channels = this.Channels;
            e.MainForm = this.MainForm;
        }

        void dlg_GetDtlpSearchParam(object sender, GetDtlpSearchParamEventArgs e)
        {
            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            e.DtlpChannels = dtlp_searchform.DtlpChannels;
            e.DtlpChannel = dtlp_searchform.DtlpChannel;
        }

        // 追加保存到数据库
        void menu_saveToDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strPreferedMarcSyntax = "";
            if (this._linkMarcFile != null)
                strPreferedMarcSyntax = this._linkMarcFile.MarcSyntax;
            else
            {
                // 观察要保存的第一条记录的marc syntax
                nRet = GetOneRecordSyntax(0,
                    this.m_bInSearching,
                    out strPreferedMarcSyntax,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            string strLastSavePath = MainForm.LastSavePath;
            if (String.IsNullOrEmpty(strLastSavePath) == false)
            {
                string strOutputPath = "";
                nRet = MarcDetailForm.ChangePathToAppendStyle(strLastSavePath,
                    out strOutputPath,
                    out strError);
                if (nRet == -1)
                {
                    MainForm.LastSavePath = ""; // 避免下次继续出错
                    goto ERROR1;
                }
                strLastSavePath = strOutputPath;
            }


            SaveRecordDlg dlg = new SaveRecordDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.SaveToDbMode = true;    // 不允许在textbox中修改路径

            dlg.MainForm = this.MainForm;
            dlg.GetDtlpSearchParam += new GetDtlpSearchParamEventHandle(dlg_GetDtlpSearchParam);
            dlg.GetDp2SearchParam += new GetDp2SearchParamEventHandle(dlg_GetDp2SearchParam);
            {
                dlg.RecPath = strLastSavePath;
                dlg.Text = "请选择目标数据库";
            }
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            MainForm.LastSavePath = dlg.RecPath;

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(dlg.RecPath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#if NO
            string strDp2ServerName = "";
            string strPurePath = "";
            // 解析记录路径。
            // 记录路径为如下形态 "中文图书/1 @服务器"
            dp2SearchForm.ParseRecPath(strPath,
                out strDp2ServerName,
                out strPurePath);

            string strDbName = GetDbName(strPurePath);
            string strSyntax = "";

            // 获得一个数据库的数据syntax
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetDbSyntax(this.stop,
                bUseNewChannel,
                strServerName,
                strServerUrl,
                strDbName,
                out strSyntax,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 获得一个数据库的数据syntax
            // parameters:
            //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
            //              如果==null，表示会自动使用this.stop，并自动OnStop+=
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = this.GetDbSyntax(
                null,
                strServerName,
                strBiblioDbName,
                out strSyntax,
                out strError);
            if (nRet == -1)
            {
                strError = "获取书目库 '" + strBiblioDbName + "的数据格式时发生错误: " + strError;
                goto ERROR1;
            }
#endif

            // TODO: 禁止问号以外的其它ID

            this.stop.BeginLoop();

            this.EnableControlsInSearching(false);
            try
            {

                // dtlp协议的记录保存
                if (strProtocol.ToLower() == "dtlp")
                {
                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法保存记录";
                        goto ERROR1;
                    }
                    if (stop != null)
                        stop.SetProgressRange(0, this.listView_browse.SelectedItems.Count);

                    for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                            stop.SetProgressValue(i);
                        }

                        ListViewItem item = this.listView_browse.SelectedItems[i];
                        string strSourcePath = item.Text;

                        string strRecord = "";
                        string strOutputPath = "";
                        string strOutStyle = "";
                        byte[] baTimestamp = null;
                        DigitalPlatform.Z3950.Record record = null;
                        Encoding currrentEncoding;
                        string strXmlFragment = "";
                        nRet = InternalGetOneRecord(
                            false,
    "marc",
    strSourcePath,
    "current",
                    "",
    out strRecord,
    out strXmlFragment,
    out strOutputPath,
    out strOutStyle,
    out baTimestamp,
    out record,
    out currrentEncoding,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";
                        if (string.IsNullOrEmpty(strMarcSyntax) == true)
                        {
                            strError = "记录 '"+strSourcePath+"' 不是MARC格式，无法保存到DTLP服务器";
                            goto ERROR1;
                        }

                        byte[] baOutputTimestamp = null;
                        nRet = dtlp_searchform.SaveMarcRecord(
                            strPath,
                            strRecord,
                            baTimestamp,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    MessageBox.Show(this, "保存成功");
                    return;
                }
                else if (strProtocol.ToLower() == "dp2library")
                {
                    if (stop != null)
                        stop.SetProgressRange(0, this.listView_browse.SelectedItems.Count);

                    for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }

                            stop.SetProgressValue(i);
                        }

                        ListViewItem item = this.listView_browse.SelectedItems[i];
                        string strSourcePath = item.Text;

                        string strRecord = "";
                        string strOutputPath = "";
                        string strOutStyle = "";
                        byte[] baTimestamp = null;
                        DigitalPlatform.Z3950.Record record = null;
                        Encoding currrentEncoding;
                        string strXmlFragment = "";
                        nRet = InternalGetOneRecord(
                            false,
    "marc",
    strSourcePath,
    "current",
                    "",
    out strRecord,
    out strXmlFragment,
    out strOutputPath,
    out strOutStyle,
    out baTimestamp,
    out record,
    out currrentEncoding,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        string strMarcSyntax = MarcDetailForm.GetMarcSyntax(record.m_strSyntaxOID);
                        if (string.IsNullOrEmpty(strMarcSyntax) == true)
                            strMarcSyntax = "unimarc";

                        byte[] baOutputTimestamp = null;
                        string strComment = "copy from " + strSourcePath;
                        // return:
                        //      -2  timestamp mismatch
                        //      -1  error
                        //      0   succeed
                        nRet = this.SaveMarcRecord(
                            false,
                            strPath,
                            strRecord,
                            strMarcSyntax,
                            baTimestamp,
                            strXmlFragment,
                            strComment,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                    }
                    MessageBox.Show(this, "保存成功");
                    return;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持Z39.50协议的保存操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }

            }
            finally
            {
                this.stop.EndLoop();
                this.stop.HideProgress();

                this.EnableControlsInSearching(true);
            }

            // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存到记录路径文件
        void menu_saveToRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的记录路径文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportRecPathFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportRecPathFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportRecPathFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "记录路径文件 '" + this.ExportRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    "dp2SearchForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                {
                    ListViewItem item = this.listView_browse.SelectedItems[i];

                    // 解析记录路径
                    string strServerName = "";
                    string strPurePath = "";
                    ParseRecPath(item.Text,
                        out strServerName,
                        out strPurePath);
                    // 获得server url
                    dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
                    if (server == null)
                    {
                        strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                        goto ERROR1;
                    }

                    string strPath = strPurePath + "@" + server.Url;

                    sw.WriteLine(strPath);
                }

                this.Cursor = oldCursor;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "书目记录路径 " + this.listView_browse.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 刷新所选择的浏览行
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
            string strError = "";

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新浏览行 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);

            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                stop.SetProgressRange(0, this.listView_browse.SelectedItems.Count);

                for (int i=0; i<this.listView_browse.SelectedItems.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }

                    ListViewItem item = this.listView_browse.SelectedItems[i];

                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    // TODO: 检查路径的正确性，检查数据库是否为书目库之一

                    int nRet = RefreshOneLine(item,
            out strError);
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
    "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 继续获取；No 放弃刷新)",
    "dp2SearchForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto ERROR1;
                    }

                    stop.SetProgressValue(i + 1);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif
        // 刷新所选择的浏览行。也就是重新从数据库中装载浏览列
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_browse.SelectedItems.Count == 0)
            {
                strError = "尚未选择要刷新的浏览行";
                goto ERROR1;
            }

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_browse.SelectedItems)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                if (IsItemChanged(item) == true)
                    nChangedCount++;
            }
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
    "要刷新的 " + this.listView_browse.SelectedItems.Count.ToString() + " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
    "dp2SearchForm",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            } 
            
            nRet = RefreshListViewLines(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 观察一个事项是否在内存中修改过
        bool IsItemChanged(ListViewItem item)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return false;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return false;

            if (string.IsNullOrEmpty(info.NewXml) == false)
                return true;

            return false;
        }

        // 清除一个事项的修改信息
        // parameters:
        //      bClearBiblioInfo    是否顺便清除事项的 BiblioInfo 信息
        void ClearOneChange(ListViewItem item,
            bool bClearBiblioInfo = false)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return;

            if (String.IsNullOrEmpty(info.NewXml) == false)
            {
                info.NewXml = "";

                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
            }

            if (bClearBiblioInfo == true)
                this.m_biblioTable.Remove(strRecPath);
        }

        public int RefreshListViewLines(List<ListViewItem> items_param,
    out string strError)
        {
            strError = "";

            if (items_param.Count == 0)
                return 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新浏览行 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                List<ListViewItem> items = new List<ListViewItem>();
                List<string> recpaths = new List<string>();
                foreach (ListViewItem item in items_param)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;
                    items.Add(item);
                    recpaths.Add(item.Text);

                    ClearOneChange(item, true);
                }

                if (stop != null)
                    stop.SetProgressRange(0, items.Count);

                BrowseLoader loader = new BrowseLoader();
                loader.Channels = this.Channels;
                loader.Servers = this.dp2ResTree1.Servers;
                loader.Stop = stop;
                loader.RecPaths = recpaths;

                int i = 0;
                foreach (DigitalPlatform.CirculationClient.localhost.Record record in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    Debug.Assert(record.Path == recpaths[i], "");

                    if (stop != null)
                    {
                        stop.SetMessage("正在刷新浏览行 " + record.Path + " ...");
                        stop.SetProgressValue(i);
                    }

                    ListViewItem item = items[i];
                    if (record.Cols == null)
                    {
                        int c = 0;
                        foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                        {
                            if (c != 0)
                                subitem.Text = "";
                            c++;
                        }
                    }
                    else
                    {
                        for (int c = 0; c < record.Cols.Length; c++)
                        {
                            ListViewUtil.ChangeItemText(item,
                            c + 1,
                            record.Cols[c]);
                        }

                        // TODO: 是否清除余下的列内容?
                    }


                    i++;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);
            }
        }



        // 从窗口中移走所选择的事项
        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            for (int i = this.listView_browse.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.listView_browse.Items.RemoveAt(this.listView_browse.SelectedIndices[i]);
            }

            this.Cursor = oldCursor;
        }

        void menu_copyToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this, 
                "dp2SearchForm",
                this.listView_browse,
                false);
        }

        void menu_cutToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this,
                "dp2SearchForm",
                this.listView_browse, 
                true);
        }

        void menu_pasteFromClipboard_insertBefore_Click(object sender, EventArgs e)
        {
            Global.PasteLinesFromClipboard(this,
                "dp2SearchForm,AmazonSearchForm",
                this.listView_browse,
                true);

            ConvertPastedLines();
        }

        void menu_pasteFromClipboard_insertAfter_Click(object sender, EventArgs e)
        {
            Global.PasteLinesFromClipboard(this,
                "dp2SearchForm,AmazonSearchForm",
                this.listView_browse,
                false);

            ConvertPastedLines();
        }


        // 将刚刚 paste 进入的新行，进行处理，以便达到功能完满的水平
        // TODO: 其实当前浏览列表中是应该允许 UNIMARC 和 USMARC 两类记录并存的
        void ConvertPastedLines()
        {
            string strError = "";
            int nRet = 0;

            AmazonSearchForm amazon_searchform = this.MainForm.GetAmazonSearchForm();

            foreach (ListViewItem item in this.listView_browse.SelectedItems)
            {
                if (item.Tag is AmazonSearchForm.ItemInfo)
                {
                    AmazonSearchForm.ItemInfo origin = (AmazonSearchForm.ItemInfo)item.Tag;
                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    string strMARC = "";
                    nRet = amazon_searchform.GetItemInfo(origin,
            out strMARC,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strXml = "";
                    nRet = MarcUtil.Marc2Xml(strMARC,
    "unimarc",
    out strXml,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;


                    strRecPath = strRecPath + " @mem";
                    BiblioInfo info = new BiblioInfo();
                    info.RecPath = strRecPath;
                    this.m_biblioTable[strRecPath] = info;

                    info.OldXml = strXml;
                    info.Timestamp = null;
                    info.RecPath = strRecPath;

                    string strSytaxOID = "1.2.840.10003.5.1";

                    string strBrowseText = "";
                    nRet = BuildMarcBrowseText(
                        strSytaxOID,
                        strMARC,
                        out strBrowseText,
                        out strError);

                    string[] cols = strBrowseText.Split(new char[] { '\t' });

                    // 修改浏览行
                    ChangeCols(
                        item,
                        strRecPath,
                        cols);

                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_loadToOpenedDetailForm_Click(object sender, EventArgs e)
        {
            int nIndex = -1;
            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }
            LoadDetail(nIndex,
        false);
        }


        void menu_loadToNewDetailForm_Click(object sender, EventArgs e)
        {
            int nIndex = -1;
            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }
            LoadDetail(nIndex,
        true);
        }

        void menuItem_selectAll_Click(object sender,
            EventArgs e)
        {
            this.listView_browse.BeginUpdate();

            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                this.listView_browse.Items[i].Selected = true;
            }

            this.listView_browse.EndUpdate();

        }

        void menuItem_saveOriginRecordToWorksheet_Click(object sender, EventArgs e)
        {
            this.SaveOriginRecordToWorksheet();
        }

        void menuItem_saveOriginRecordToIso2709_Click(object sender, EventArgs e)
        {
            this.SaveOriginRecordToIso2709();
        }

        private void button_searchSimple_Click(object sender, EventArgs e)
        {
#if NO
            if (this.dp2ResTree1.CheckBoxes == true)
                DoCheckedSimpleSearch();
            else
                DoSimpleSearch();
#endif

            DoSearch("simple");
        }

        private void comboBox_matchStyle_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_simple_matchStyle.Invalidate();
        }

        private void comboBox_matchStyle_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_simple_matchStyle.Text == "空值")
            {
                this.textBox_simple_queryWord.Text = "";
                this.textBox_simple_queryWord.Enabled = false;
            }
            else
            {
                this.textBox_simple_queryWord.Enabled = true;
            }
        }

        public bool SetLayout(string strLayoutName)
        {
            if (strLayoutName != "资源树最大"
                && strLayoutName != "浏览框最大(横向)"
                && strLayoutName != "浏览框最大(竖向)")
                return false;

            // 保存前恢复简单检索面板或者多行
            if (this.tabControl_query.SelectedTab == this.tabPage_logic)
                this.tabControl_query.SelectedTab = this.tabPage_simple;

            this.splitContainer_main.Panel1.Controls.RemoveAt(0);
            this.splitContainer_main.Panel2.Controls.RemoveAt(0);

            this.splitContainer_up.Panel1.Controls.RemoveAt(0);
            this.splitContainer_up.Panel2.Controls.RemoveAt(0);


            if (strLayoutName == "资源树最大")
            {
                this.splitContainer_main.Orientation = Orientation.Vertical;
                this.splitContainer_main.Panel1.Controls.Add(this.panel_resTree);

                this.splitContainer_up.Orientation = Orientation.Horizontal;
                this.splitContainer_up.Panel1.Controls.Add(this.splitContainer_queryAndResultInfo);
                this.splitContainer_up.Panel2.Controls.Add(this.listView_browse);

                this.splitContainer_main.Panel2.Controls.Add(this.splitContainer_up);
            }
            else if (strLayoutName == "浏览框最大(横向)"
                || strLayoutName == "浏览框最大(竖向)")
            {

                this.splitContainer_up.Panel1.Controls.Add(this.panel_resTree);
                this.splitContainer_up.Panel2.Controls.Add(this.splitContainer_queryAndResultInfo);

                this.splitContainer_main.Panel1.Controls.Add(this.splitContainer_up);

                this.splitContainer_main.Panel2.Controls.Add(this.listView_browse);
            }
            else 
            {
                Debug.Assert(false, "");
            }

            if (strLayoutName == "浏览框最大(横向)")
            {
                this.splitContainer_main.Orientation = Orientation.Horizontal;
                this.splitContainer_up.Orientation = Orientation.Vertical;
                //this.splitContainer_queryAndResultInfo.Orientation = Orientation.Horizontal;
            }
            else if (strLayoutName == "浏览框最大(竖向)")
            {
                this.splitContainer_main.Orientation = Orientation.Vertical;
                this.splitContainer_up.Orientation = Orientation.Horizontal;
                //this.splitContainer_queryAndResultInfo.Orientation = Orientation.Horizontal;
            }


            return true;
        }

        public void Reload()
        {
            if (this.dp2ResTree1.Focused == true)
            {
                this.dp2ResTree1.Refresh(dp2ResTree.RefreshStyle.All);
                // this.dp2ResTree1.Focus();
            }
            else if (this.listView_browse.Focused == true)
            {
                // TODO: 重新装载全部
            }
        }

        // 获得一个数据库的特性
        // 可能会抛出异常
        public NormalDbProperty GetDbProperty(string strServerName,
            string strDbName)
        {
            return this.dp2ResTree1.GetDbProperty(strServerName,
                strDbName);
        }

        private void dp2QueryControl1_GetList(object sender, GetListEventArgs e)
        {
            // 列出服务器
            if (string.IsNullOrEmpty(e.Path) == true)
            {
                e.Values = this.dp2ResTree1.GetServerNames();
                return;
            }

            try
            {

                string[] parts = e.Path.Split(new char[] { '/' });
                if (parts.Length == 1)
                {
                    e.Values = this.dp2ResTree1.GetDbNames(parts[0]);
                }
                else if (parts.Length == 2)
                {
                    e.Values = this.dp2ResTree1.GetFromNames(parts[0], parts[1]);
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ex.Message) == true)
                    e.ErrorInfo = "error";
                else
                    e.ErrorInfo = ex.Message;
                return;
            }
        }

        private void tabControl_query_Selected(object sender, TabControlEventArgs e)
        {
            string strLayoutName = this.LayoutName;

            if (this.tabControl_query.SelectedTab == this.tabPage_logic)
            {
                this.dp2ResTree1.Visible = false;

                if (strLayoutName == "资源树最大")
                    this.splitContainer_main.Panel1Collapsed = true;
                else if (strLayoutName == "浏览框最大(横向)")
                    this.splitContainer_up.Panel1Collapsed = true;
                else if (strLayoutName == "浏览框最大(竖向)")
                    this.splitContainer_up.Panel1Collapsed = true;
            }
            else
            {
                this.dp2ResTree1.Visible = true;
                if (strLayoutName == "资源树最大")
                    this.splitContainer_main.Panel1Collapsed = false;
                else if (strLayoutName == "浏览框最大(横向)")
                    this.splitContainer_up.Panel1Collapsed = false;
                else if (strLayoutName == "浏览框最大(竖向)")
                    this.splitContainer_up.Panel1Collapsed = false;
            }
        }

        private void dp2QueryControl1_ViewXml(object sender, EventArgs e)
        {
            string strError = "";
            List<QueryItem> items = null;

            int nRet = this.dp2QueryControl1.BuildQueryXml(
            this.SearchMaxCount,
            "zh",
            out items,
            out strError);
            if (nRet == -1)
            {
                strError = "在创建XML检索式的过程中出错: " + strError;
                goto ERROR1;
            }

            string strFileName = this.MainForm.DataDir + "\\~logic_queries.txt";
            using (StreamWriter sw = new StreamWriter(strFileName,
                false,
                Encoding.UTF8))
            {
                for (int j = 0; j < items.Count; j++)
                {

                    QueryItem item = items[j];
                    sw.WriteLine("---\r\n针对服务器 " + item.ServerName + ":");
                    string strXml = "";
                    nRet = DomUtil.GetIndentXml(item.QueryXml, out strXml, out strError);
                    if (nRet == -1)
                    {
                        strError = "XML检索式 '"+item.QueryXml+"' XML格式有错: " + strError;
                        goto ERROR1;
                    }
                    sw.WriteLine(strXml);
                }
            }

            System.Diagnostics.Process.Start("notepad.exe", strFileName);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public string LayoutName
        {
            get
            {
                return MainForm.AppInfo.GetString(
    "dp2searchform",
    "layout",
    "资源树最大");
            }
        }

        // 从多个数据库中命中时，是否都要
        public bool SelectAllDb
        {
            get
            {
                return MainForm.AppInfo.GetBoolean(
    "dp2searchform",
    "sekect_all_db",
    true);
            }
            set
            {
                MainForm.AppInfo.SetBoolean(
    "dp2searchform",
    "sekect_all_db",
    value);
            }
        }

        private void label_simple_queryWord_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;

            menuItem = new ToolStripMenuItem("RFC1123时间值...");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Single_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("u时间值...");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_uSingle_Click);
            contextMenu.Items.Add(menuItem);


            // ---
            ToolStripSeparator sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("RFC1123时间值范围...");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Range_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("u时间值范围...");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_uRange_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.label_simple_queryWord, e.Location);
        }

        private void ToolStripMenuItem_rfc1123Single_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.RangeMode = false;
            try
            {
                dlg.Rfc1123String = this.textBox_simple_queryWord.Text;
            }
            catch
            {
                this.textBox_simple_queryWord.Text = "";
            }
            this.MainForm.AppInfo.LinkFormState(dlg, "dp2searchform_gettimedialog_single");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_simple_queryWord.Text = dlg.Rfc1123String;
        }

        private void ToolStripMenuItem_uSingle_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.RangeMode = false;
            try
            {
                dlg.uString = this.textBox_simple_queryWord.Text;
            }
            catch
            {
                this.textBox_simple_queryWord.Text = "";
            }

            this.MainForm.AppInfo.LinkFormState(dlg, "dp2searchform_gettimedialog_single");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_simple_queryWord.Text = dlg.uString;

        }

        private void ToolStripMenuItem_rfc1123Range_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.RangeMode = true;
            // 分割为两个字符串
            try
            {
                dlg.Rfc1123String = this.textBox_simple_queryWord.Text;
            }
            catch
            {
                this.textBox_simple_queryWord.Text = "";
            }
            this.MainForm.AppInfo.LinkFormState(dlg, "dp2searchform_gettimedialog_range");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_simple_queryWord.Text = dlg.Rfc1123String;

        }

        private void ToolStripMenuItem_uRange_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.RangeMode = true;
            try
            {
                dlg.uString = this.textBox_simple_queryWord.Text;
            }
            catch
            {
                this.textBox_simple_queryWord.Text = "";
            }

            this.MainForm.AppInfo.LinkFormState(dlg, "dp2searchform_gettimedialog_range");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_simple_queryWord.Text = dlg.uString;
        }

        bool InSearching
        {
            get
            {
                return m_bInSearching;
            }
        }

        int GetBiblioInfo(
    bool bCheckSearching,
    ListViewItem item,
    out BiblioInfo info,
    out string strError)
        {
            strError = "";
            info = null;

            if (this.m_biblioTable == null)
                return 0;

            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return 0;


            // 存储所获得书目记录 XML
            info = (BiblioInfo)this.m_biblioTable[strRecPath];


            if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
            {
                if (bCheckSearching == true && this._linkMarcFile == null)
                {
                    if (this.InSearching == true)
                        return 0;
                }

                // 解析记录路径
                string strServerName = "";
                string strPurePath = "";
                ParseRecPath(strRecPath,
                    out strServerName,
                    out strPurePath);
                // 获得server url
                dp2Server server = this.dp2ResTree1.Servers.GetServerByName(strServerName);
                if (server == null)
                {
                    strError = "名为 '" + strServerName + "' 的服务器在检索窗中尚未定义...";
                    return -1;
                }
                string strServerUrl = server.Url;

                this.Channel = this.Channels.GetChannel(strServerUrl);

                string[] results = null;
                byte[] baTimestamp = null;
                // 获得书目记录
                long lRet = Channel.GetBiblioInfos(
                    stop,
                    strPurePath,
                    "",
                    new string[] { "xml" },   // formats
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                    return -1;  // 是否设定为特殊状态?
                if (lRet == -1)
                    return -1;

                if (results == null || results.Length == 0)
                {
                    strError = "results error";
                    return -1;
                }

                string strXml = results[0];

                // 滞后创建新对象，避免在 hashtable 中存在一个尚未初始化的对象，而被其他线程抢先使用了
                if (info == null)
                {
                    info = new BiblioInfo();
                    info.RecPath = strRecPath;
                    this.m_biblioTable[strRecPath] = info;
                }

                info.OldXml = strXml;
                info.Timestamp = baTimestamp;
                info.RecPath = strRecPath;
            }

            return 1;
        }

        int m_nInViewing = 0;

        void DoViewComment(bool bOpenWindow)
        {
            m_nInViewing++;
            try
            {
                _doViewComment(bOpenWindow);
            }
            finally
            {
                m_nInViewing--;
            }
        }


        void _doViewComment(bool bOpenWindow)
        {
            string strError = "";
            string strMarcHtml = "";
            // string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (this.MainForm.CanDisplayItemProperty() == false)
                    return;
            }

            if (this.m_biblioTable == null
                || this.listView_browse.SelectedItems.Count != 1)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            ListViewItem item = this.listView_browse.SelectedItems[0];
#if NO
            string strRecPath = this.listView_records.SelectedItems[0].Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }
#endif

            // BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            BiblioInfo info = null;
            int nRet = GetBiblioInfo(
                true,
                item,
                out info,
                out strError);
            if (info == null)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            string strXml1 = "";
            string strHtml2 = "";
            string strXml2 = "";
            string strBiblioHtml = "";

            if (nRet == -1)
            {
                strHtml2 = HttpUtility.HtmlEncode(strError);
            }
            else
            {
                nRet = GetXmlHtml(info,
                    out strXml1,
                    out strXml2,
                    out strHtml2,
                    out strBiblioHtml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            strMarcHtml = "<html>" +
    GetHeadString() +
    "<body>" +
    strHtml2 +
    "</body></html>";




            bool bNew = false;
            if (this.m_commentViewer == null
                || (bOpenWindow == true && this.m_commentViewer.Visible == false))
            {
                m_commentViewer = new BiblioViewerForm();
                GuiUtil.SetControlFont(m_commentViewer, this.Font, false);
                bNew = true;
            }

            m_commentViewer.MainForm = this.MainForm;  // 必须是第一句

            if (bNew == true)
            {
                // m_commentViewer.InitialWebBrowser();
            }

            m_commentViewer.Text = "MARC内容 '" + info.RecPath + "'";
            m_commentViewer.HtmlString = strBiblioHtml;
            m_commentViewer.MarcString = strMarcHtml;
            m_commentViewer.XmlString = MergeXml(strXml1, strXml2);
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // this.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    this.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_commentViewer.WindowState == FormWindowState.Minimized)
                        m_commentViewer.WindowState = FormWindowState.Normal;
                    m_commentViewer.Activate();
                }
            }
            else
            {
                if (m_commentViewer.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewComment() 出错: " + strError);
        }
        static string MergeXml(string strXml1,
    string strXml2)
        {
            if (string.IsNullOrEmpty(strXml1) == true)
                return strXml2;
            if (string.IsNullOrEmpty(strXml2) == true)
                return strXml1;

            return strXml1; // 临时这样
        }

        int GetXmlHtml(BiblioInfo info,
            out string strXml1,
            out string strXml2,
            out string strMarcHtml,
            out string strBiblioHtml,
            out string strError)
        {
            strError = "";
            strXml1 = "";
            strXml2 = "";
            strMarcHtml = "";
            strBiblioHtml = "";
            int nRet = 0;

            strXml1 = info.OldXml;
            strXml2 = info.NewXml;

            string strMarcSyntax = "";

            string strOldMARC = "";
            string strOldFragmentXml = "";
            if (string.IsNullOrEmpty(strXml1) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml1,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strOldMARC,
                    out strOldFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    return -1;
                }
                strMarcSyntax = strOutMarcSyntax;
            }

            string strNewMARC = "";
            string strNewFragmentXml = "";
            if (string.IsNullOrEmpty(strXml2) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml2,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strNewMARC,
                    out strNewFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    return -1;
                }
                strMarcSyntax = strOutMarcSyntax;

            }

            string strMARC = "";
            if (string.IsNullOrEmpty(strOldMARC) == false
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                nRet = MarcDiff.DiffHtml(
                    strOldMARC,
                    strOldFragmentXml,
                    "",
                    strNewMARC,
                    strNewFragmentXml,
                    "",
                    out strMarcHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
                strMARC = strNewMARC;
            }
            else if (string.IsNullOrEmpty(strOldMARC) == false
    && string.IsNullOrEmpty(strNewMARC) == true)
            {
                strMarcHtml = MarcUtil.GetHtmlOfMarc(strOldMARC,
                    strOldFragmentXml,
                    "",
                    false);
                strMARC = strOldMARC;
            }
            else if (string.IsNullOrEmpty(strOldMARC) == true
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                strMarcHtml = MarcUtil.GetHtmlOfMarc(strNewMARC,
                    strNewFragmentXml,
                    "",
                    false);
                strMARC = strNewMARC;
            }

            // return:
            //      -1  出错
            //      0   .fltx 文件没有找到
            //      1   成功
            nRet = this.MainForm.BuildMarcHtmlText(
                MarcDetailForm.GetSyntaxOID(strMarcSyntax),
                strMARC,
                out strBiblioHtml,
                out strError);
            if (nRet == -1)
                strBiblioHtml = strError.Replace("\r\n", "<br/>");

            return 0;
        }

        void marc_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                    this.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "operloghtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    //"<script type='text/javascript' src='%datadir%/jquery.js'></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.listView_browse.ForceUpdate();
        }


    }
    // 为一行存储的书目信息
    public class BiblioInfo
    {
        public string RecPath = "";
        public string OldXml = "";
        public byte[] Timestamp = null; // 来自数据库的时间戳
        public string NewXml = "";
        public long NewVersion = 0; // NewXml修改后的版本号 datetime ticks
    }

    public class LoaderItem
    {
        public BiblioInfo BiblioInfo = null;
        public ListViewItem ListViewItem = null;

        public LoaderItem(BiblioInfo info, ListViewItem item)
        {
            this.BiblioInfo = info;
            this.ListViewItem = item;
        }
    }

    /// <summary>
    /// 根据 ListViewItem 数组获得书目记录信息的枚举器
    /// 可以利用缓存机制
    /// </summary>
    public class ListViewBiblioLoader : IEnumerable
    {
        public List<ListViewItem> Items
        {
            get;
            set;
        }

        public Hashtable CacheTable
        {
            get;
            set;
        }

        BiblioLoader m_loader = null;

        public ListViewBiblioLoader(LibraryChannelCollection channels,
            dp2ServerCollection servers,
            Stop stop,
            List<ListViewItem> items,
            Hashtable cacheTable)
        {
            m_loader = new BiblioLoader();
            m_loader.Channels = channels;
            m_loader.Servers = servers;
            m_loader.Stop = stop;
            m_loader.Format = "xml";
            m_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp; // 附加信息只取得 timestamp

            this.Items = items;
            this.CacheTable = cacheTable;
        }

        public IEnumerator GetEnumerator()
        {
            Debug.Assert(m_loader != null, "");

            Hashtable dup_table = new Hashtable();  // 确保 recpaths 中不会出现重复的路径

            List<string> recpaths = new List<string>(); // 缓存中没有包含的那些记录
            foreach (ListViewItem item in this.Items)
            {
                string strRecPath = item.Text;
                Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

#if NO
                BiblioInfo info = (BiblioInfo)this.CacheTable[strRecPath];
                if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                    recpaths.Add(strRecPath);
#endif

                if (dup_table.ContainsKey(strRecPath) == true)
                    continue;
                BiblioInfo info = (BiblioInfo)this.CacheTable[strRecPath];
                if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                {
                    recpaths.Add(strRecPath);
                    dup_table[strRecPath] = true;
                }
            }

            // 注： Hashtable 在这一段时间内不应该被修改。否则会破坏 m_loader 和 items 之间的锁定对应关系

            m_loader.RecPaths = recpaths;

            var enumerator = m_loader.GetEnumerator();

            // 开始循环
            foreach (ListViewItem item in this.Items)
            {
                string strRecPath = item.Text;
                Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                BiblioInfo info = (BiblioInfo)this.CacheTable[strRecPath];
                if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                {
                    if (m_loader.Stop != null)
                    {
                        m_loader.Stop.SetMessage("正在获取书目记录 " + strRecPath);
                    }
                    bool bRet = enumerator.MoveNext();
                    if (bRet == false)
                    {
                        Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                        // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                        yield break;
                    }

                    BiblioItem biblio = (BiblioItem)enumerator.Current;
                    Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                    // 需要放入缓存
                    if (info == null)
                    {
                        info = new BiblioInfo();
                        info.RecPath = biblio.RecPath;
                    }
                    info.OldXml = biblio.Content;
                    info.Timestamp = biblio.Timestamp;
                    this.CacheTable[strRecPath] = info;
                    yield return new LoaderItem(info, item);
                }
                else
                    yield return new LoaderItem(info, item);
            }
        }
    }

}