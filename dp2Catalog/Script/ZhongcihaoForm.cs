using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Core;

namespace dp2Catalog
{
    public partial class ZhongcihaoForm : MyForm
    {
        // string EncryptKey = "dp2catalog_client_password_key";

        /*
        public string LibraryServerName = "";
        public string LibraryServerUrl = "";
         * */

        public LibraryChannelCollection Channels = null;
        LibraryChannel Channel = null;

#if NO
        public string Lang = "zh";
#endif

        //public MainForm MainForm = null;
        //DigitalPlatform.Stop stop = null;

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventFinish = new AutoResetEvent(false);

        string m_strMaxNumber = null;
        string m_strTailNumber = null;

        public bool AutoBeginSearch = false;

        const int WM_INITIAL = API.WM_USER + 201;

        public string MyselfBiblioRecPath = "";    // 发起取号的书目记录的路径。用来校正统计过程，排除自己。


        public ZhongcihaoForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_number.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);
#if NO
            prop.ParsePath -= new ParsePathEventHandler(prop_ParsePath);
            prop.ParsePath += new ParsePathEventHandler(prop_ParsePath);
#endif

        }

#if NO
        void prop_ParsePath(object sender, ParsePathEventArgs e)
        {
            string strServerName = "";
            string strPurePath = "";
            // 解析记录路径。
            // 记录路径为如下形态 "中文图书/1 @服务器"
            dp2SearchForm.ParseRecPath(e.Path,
                out strServerName,
                out strPurePath);

            e.DbName = strServerName + "|" + dp2SearchForm.GetDbName(strPurePath);
        }
#endif

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
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
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                // return new List<string>();
                return null;
            }

            return dp2_searchform.dp2ResTree1.GetBrowseColumnNames(this.textBox_serverName.Text, strPrefix);

            /*
            string[] parts = strPrefix.Split(new char[] { '|' });
            if (parts.Length < 2)
                return new List<string>();

            return dp2_searchform.dp2ResTree1.GetBrowseColumnNames(parts[0], parts[1]);
             * */
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            dp2_searchform = this.MainForm.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // 新开一个dp2检索窗
                FormWindowState old_state = this.WindowState;

                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this.MainForm;
                dp2_searchform.MdiParent = this.MainForm;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dp2_searchform.WaitLoadFinish();
            }

            return dp2_searchform;
        }

        private void ZhongcihaoForm_Load(object sender, EventArgs e)
        {
            LoadSize();

            this.Channels = new LibraryChannelCollection();
            this.Channels.BeforeLogin += new BeforeLoginEventHandle(Channels_BeforeLogin);

#if NO
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // 服务器名
            if (string.IsNullOrEmpty(this.textBox_serverName.Text) == true)
            {
                this.textBox_serverName.Text = this.MainForm.AppInfo.GetString(
    "zhongcihao_form",
    "servername",
    "");
            }

            // 类号
            if (String.IsNullOrEmpty(this.textBox_classNumber.Text) == true)
            {
                this.textBox_classNumber.Text = this.MainForm.AppInfo.GetString(
                    "zhongcihao_form",
                    "classnumber",
                    "");
            }

            // 线索书目库名
            if (String.IsNullOrEmpty(this.comboBox_biblioDbName.Text) == true)
            {
                this.comboBox_biblioDbName.Text = this.MainForm.AppInfo.GetString(
                    "zhongcihao_form",
                    "biblio_dbname",
                    "");
            }

            // 是否要返回浏览列
            this.checkBox_returnBrowseCols.Checked = this.MainForm.AppInfo.GetBoolean(
                    "zhongcihao_form",
                    "return_browse_cols",
                    true);

            string strWidths = this.MainForm.AppInfo.GetString(
"zhongcihao_form",
"record_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_number,
                    strWidths,
                    true);
            }

            if (this.AutoBeginSearch == true)
            {
                API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
            }
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

            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = false;
            e.Parameters = "location=dp2Catalog,type=worker";

            e.Parameters += ",client=dp2catalog|" + Program.ClientVersion;

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


#if NOOOOOOOOOOOOOOOOOOOOOOOO
        public void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {

            if (e.FirstTry == true)
            {
                e.UserName = this.MainForm.AppInfo.GetString(
                    "default_account",
                    "username",
                    "");
                e.Password = this.MainForm.AppInfo.GetString(
                    "default_account",
                    "password",
                    "");
                e.Password = this.DecryptPasssword(e.Password);

                e.IsReader =
                    this.MainForm.AppInfo.GetBoolean(
                    "default_account",
                    "isreader",
                    false);
                e.Location = this.MainForm.AppInfo.GetString(
                    "default_account",
                    "location",
                    "");
                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = null;

            if (sender is Form)
                owner = (Form)sender;
            else
                owner = this;

            CirculationLoginDlg dlg = SetDefaultAccount(
                e.CirculationServerUrl,
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
            e.SavePasswordShort = dlg.SavePasswordShort;
            e.IsReader = dlg.IsReader;
            e.Location = dlg.OperLocation;
            e.SavePasswordLong = dlg.SavePasswordLong;
            e.CirculationServerUrl = dlg.ServerUrl;
        }

        CirculationLoginDlg SetDefaultAccount(
    string strServerUrl,
    string strTitle,
    string strComment,
    IWin32Window owner)
        {
            CirculationLoginDlg dlg = new CirculationLoginDlg();

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
                dlg.ServerUrl =
        this.MainForm.AppInfo.GetString("config",
        "circulation_server_url",
        "http://localhost/dp2libraryws/library.asmx");
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
            dlg.UserName = this.MainForm.AppInfo.GetString(
                "default_account",
                "username",
                "");

            dlg.SavePasswordShort =
    this.MainForm.AppInfo.GetBoolean(
    "default_account",
    "savepassword_short",
    false);

            dlg.SavePasswordLong =
                this.MainForm.AppInfo.GetBoolean(
                "default_account",
                "savepassword_long",
                false);

            if (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true)
            {
                dlg.Password = this.MainForm.AppInfo.GetString(
        "default_account",
        "password",
        "");
                dlg.Password = this.DecryptPasssword(dlg.Password);
            }
            else
            {
                dlg.Password = "";
            }


            dlg.IsReader =
                this.MainForm.AppInfo.GetBoolean(
                "default_account",
                "isreader",
                false);
            dlg.OperLocation = this.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");

            this.MainForm.AppInfo.LinkFormState(dlg,
                "logindlg_state");

            dlg.ShowDialog(owner);

            this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            this.MainForm.AppInfo.SetString(
                "default_account",
                "username",
                dlg.UserName);
            this.MainForm.AppInfo.SetString(
                "default_account",
                "password",
                (dlg.SavePasswordShort == true || dlg.SavePasswordLong == true) ?
                this.EncryptPassword(dlg.Password) : "");

            this.MainForm.AppInfo.SetBoolean(
    "default_account",
    "savepassword_short",
    dlg.SavePasswordShort);

            this.MainForm.AppInfo.SetBoolean(
                "default_account",
                "savepassword_long",
                dlg.SavePasswordLong);

            this.MainForm.AppInfo.SetBoolean(
                "default_account",
                "isreader",
                dlg.IsReader);
            this.MainForm.AppInfo.SetString(
                "default_account",
                "location",
                dlg.OperLocation);


            // 2006/12/30
            this.MainForm.AppInfo.SetString(
                "config",
                "circulation_server_url",
                dlg.ServerUrl);


            return dlg;
        }

        public string DecryptPasssword(string strEncryptedText)
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

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, this.EncryptKey);
        }
#endif

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INITIAL:
                    {
                        this.button_searchDouble_Click(null, null);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
        "mdi_form_state");
            }
        }

        private void ZhongcihaoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 服务器名
                this.MainForm.AppInfo.SetString(
    "zhongcihao_form",
    "servername",
    this.textBox_serverName.Text);

                // 类号
                this.MainForm.AppInfo.SetString(
                    "zhongcihao_form",
                    "classnumber",
                    this.textBox_classNumber.Text);

                // 线索书目库名
                this.MainForm.AppInfo.SetString(
                    "zhongcihao_form",
                    "biblio_dbname",
                    this.comboBox_biblioDbName.Text);

                // 是否要返回浏览列
                this.MainForm.AppInfo.SetBoolean(
                        "zhongcihao_form",
                        "return_browse_cols",
                        this.checkBox_returnBrowseCols.Checked);

                // 服务器名
                this.MainForm.AppInfo.GetString(
                        "zhongcihao_form",
                        "servername",
                        this.textBox_serverName.Text);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_number);
                this.MainForm.AppInfo.SetString(
                    "zhongcihao_form",
                    "record_list_column_width",
                    strWidths);
            }

            this.Channels.BeforeLogin -= new BeforeLoginEventHandle(Channels_BeforeLogin);

            EventFinish.Set();

            SaveSize();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        /// <summary>
        /// 书目库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return this.comboBox_biblioDbName.Text;
            }
            set
            {
                this.comboBox_biblioDbName.Text = value;
            }
        }

        /// <summary>
        /// 类号
        /// </summary>
        public string ClassNumber
        {
            get
            {
                return this.textBox_classNumber.Text;
            }
            set
            {
                this.textBox_classNumber.Text = value;
            }
        }

        /// <summary>
        /// 最大号
        /// </summary>
        public string MaxNumber
        {
            get
            {
                if (String.IsNullOrEmpty(m_strMaxNumber) == true)
                {
                    string strError = "";

                    int nRet = FillList(true, out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return m_strMaxNumber;
                ERROR1:
                    throw (new Exception(strError));
                }
                return m_strMaxNumber;
            }
            set
            {
                this.textBox_maxNumber.Text = value;
                m_strMaxNumber = value;
            }
        }

        /// <summary>
        /// 尾号
        /// </summary>
        public string TailNumber
        {
            get
            {
                if (String.IsNullOrEmpty(m_strTailNumber) == true)
                {
                    string strError = "";

                    string strTailNumber = "";
                    int nRet = SearchTailNumber(out strTailNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    m_strTailNumber = strTailNumber;
                    return m_strTailNumber;
                ERROR1:
                    throw (new Exception(strError));

                }
                return m_strTailNumber;

            }
            set
            {
                string strError = "";
                string strOutputNumber = "";
                int nRet = SaveTailNumber(value,
                    out strOutputNumber,
                    out strError);
                if (nRet == -1)
                    throw (new Exception(strError));
                else
                    m_strTailNumber = strOutputNumber;	// 刷新记忆
            }
        }

        // 检索
        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                int nRet = FillList(true, out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 不获得本类尾号
                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        void EnableControls(bool bEnable)
        {
            this.comboBox_biblioDbName.Enabled = bEnable;
            this.textBox_classNumber.Enabled = bEnable;
            this.textBox_maxNumber.Enabled = bEnable;
            this.textBox_tailNumber.Enabled = bEnable;

            this.button_copyMaxNumber.Enabled = bEnable;
            this.button_getTailNumber.Enabled = bEnable;
            this.button_pushTailNumber.Enabled = bEnable;
            this.button_saveTailNumber.Enabled = bEnable;
            this.button_searchClass.Enabled = bEnable;
            this.button_searchDouble.Enabled = bEnable;
        }

        int FillList(bool bSort,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            this.listView_number.Items.Clear();
            this.MaxNumber = "";

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

            /*
            if (dom == null)
            {
                strError = "请先调用GetGlobalCfgFile()函数";
                return -1;
            }
             * */

            if (this.ClassNumber == "")
            {
                strError = "尚未指定分类号";
                return -1;
            }

            if (this.BiblioDbName == "")
            {
                strError = "尚未指定书目库名";
                return -1;
            }

            EnableControls(false);

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索同类书记录 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();
            */
            var looping = BeginLoop(this.DoStop, "正在检索同类书记录 ...");

            try
            {
                string strQueryXml = "";

                long lRet = Channel.SearchUsedZhongcihao(
                    looping.Progress,
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    "zhongcihao",
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0)
                {
                    strError = "没有命中的记录。";
                    return 0;   // not found
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                ZhongcihaoSearchResult[] searchresults = null;

                if (looping.Progress != null)
                    looping.Progress.SetProgressRange(0, lHitCount);

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    long lCurrentPerCount = lPerCount;

                    bool bShift = Control.ModifierKeys == Keys.Shift;
                    string strBrowseStyle = "cols";
                    if (bShift == true || this.checkBox_returnBrowseCols.Checked == false)
                    {
                        strBrowseStyle = "";
                        lCurrentPerCount = lPerCount * 10;
                    }

                    looping.Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = Channel.GetZhongcihaoSearchResult(
                        looping.Progress,
                        GetZhongcihaoDbGroupName(this.BiblioDbName),
                        // "!" + this.BiblioDbName,
                        "zhongcihao",   // strResultSetName
                        lStart,
                        lPerCount,
                        strBrowseStyle, // style
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
                        ZhongcihaoSearchResult result_item = searchresults[i];
                        ListViewItem item = new ListViewItem();
                        item.Text = result_item.Path;
                        item.SubItems.Add(result_item.Zhongcihao);

                        if (result_item.Cols != null)
                        {
                            ListViewUtil.EnsureColumns(this.listView_number, result_item.Cols.Length + 1);
                            for (int j = 0; j < result_item.Cols.Length; j++)
                            {
                                ListViewUtil.ChangeItemText(item, j + 2, result_item.Cols[j]);
                            }
                        }

                        this.listView_number.Items.Add(item);
                        if (looping.Progress != null)
                            looping.Progress.SetProgressValue(lStart + i + 1);
                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }
            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                */
                EndLoop(looping);

                EnableControls(true);
            }

            if (bSort == true)
            {
                // 排序
                this.listView_number.ListViewItemSorter = new ZhongcihaoListViewItemComparer();
                this.listView_number.ListViewItemSorter = null;

                // 把重复种次号的事项用特殊颜色标出来
                ColorDup();

                this.MaxNumber = GetTopNumber(this.listView_number);    // this.listView_number.Items[0].SubItems[1].Text;
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 从已经排序的事项中，取出位置最高事项的种次号。
        // 本函数会自动排除MyselfBiblioRecPath这条记录
        string GetTopNumber(ListView list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                string strRecPath = item.Text;
                if (strRecPath != this.MyselfBiblioRecPath)
                    return item.SubItems[1].Text;
            }

            // TODO: 如果除了自己以外，并没有其他包含有效种次号的事项了，那也只好用自己的种次号-1来充当？

            return "";  // 没有找到
        }

        // 使相邻重复行变色
        void ColorDup()
        {
            string strPrevNumber = "";
            Color color1 = Color.FromArgb(220, 220, 220);
            Color color2 = Color.FromArgb(230, 230, 230);
            Color color = color1;
            int nDupCount = 0;
            for (int i = 0; i < this.listView_number.Items.Count; i++)
            {
                string strNumber = this.listView_number.Items[i].SubItems[1].Text;

                if (strNumber == strPrevNumber)
                {
                    if (i >= 1 && nDupCount == 0)
                        this.listView_number.Items[i - 1].BackColor = color;

                    this.listView_number.Items[i].BackColor = color;
                    nDupCount++;
                }
                else
                {
                    if (nDupCount >= 1)
                    {
                        // 换一下颜色
                        if (color == color1)
                            color = color2;
                        else
                            color = color1;
                    }

                    nDupCount = 0;

                    this.listView_number.Items[i].BackColor = SystemColors.Window;

                }


                strPrevNumber = strNumber;
            }

        }


        // 检索尾号，放入面板中界面元素
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int PanelGetTailNumber(out string strError)
        {
            strError = "";
            this.textBox_tailNumber.Text = "";

            string strTailNumber = "";
            int nRet = SearchTailNumber(out strTailNumber,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            this.textBox_tailNumber.Text = strTailNumber;
            // this.label_tailNumberTitle.Text = "库'" + this.ZhongcihaoDbName + "'中的尾号(&T):";
            return 1;
        }


        /// <summary>
        ///  检索获得种次号库中对应类目的尾号。此功能比较单纯，所获得的结果并不放入面板界面元素
        /// </summary>
        /// <param name="strTailNumber">返回尾号</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1出错;0没有找到;1找到</returns>
        public int SearchTailNumber(
            out string strTailNumber,
            out string strError)
        {
            strTailNumber = "";


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

            EnableControls(false);

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得尾号 ...");
            stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在获得尾号 ...");

            try
            {
                long lRet = Channel.GetZhongcihaoTailNumber(
                    looping.Progress,
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    out strTailNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                */
                EndLoop(looping);

                EnableControls(true);
            }

        // return 0;
        ERROR1:
            return -1;
        }

        // 推动尾号。如果已经存在的尾号比strTestNumber还要大，则不推动
        public int PushTailNumber(string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            this._processing++;
            try
            {
                // 获得server url
                if (String.IsNullOrEmpty(this.LibraryServerName) == true)
                {
                    strError = "尚未指定服务器名";
                    goto ERROR1;
                }
                if (this.MainForm == null
                    || this.MainForm.Servers == null)
                {
                    strError = "this.MainForm == null || this.MainForm.Servers == null";
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

                EnableControls(false);

                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在推动尾号 ...");
                stop.BeginLoop();
                */
                var looping = BeginLoop(this.DoStop, "正在推动尾号 ...");

                try
                {
                    long lRet = Channel.SetZhongcihaoTailNumber(
                        looping.Progress,
                        "conditionalpush",
                        GetZhongcihaoDbGroupName(this.BiblioDbName),
                        // "!" + this.BiblioDbName,
                        this.ClassNumber,
                        strTestNumber,
                        out strOutputNumber,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    return (int)lRet;
                }
                finally
                {
                    /*
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    */
                    EndLoop(looping);

                    EnableControls(true);
                }
            }
            finally
            {
                this._processing--;
            }

        // return 0;
        ERROR1:
            return -1;
        }

        public int SaveTailNumber(
            string strTailNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

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


            EnableControls(false);

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存尾号 ...");
            stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在保存尾号 ...");

            try
            {
                long lRet = Channel.SetZhongcihaoTailNumber(
                    looping.Progress,
                    "save",
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strTailNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                */
                EndLoop(looping);

                EnableControls(true);
            }

        // return 0;
        ERROR1:
            return -1;
        }

        // 获得尾号
        private void button_getTailNumber_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                this.textBox_tailNumber.Text = "";   // 预先清空，以防误会

                // 获得本类尾号
                int nRet = PanelGetTailNumber(out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "类 '" + this.ClassNumber + "' 的尾号尚不存在";
                    goto ERROR1;
                }

                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存尾号
        private void button_saveTailNumber_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_tailNumber.Text == "")
            {
                strError = "尚未输入要保存的尾号";
                goto ERROR1;
            }

            EventFinish.Reset();
            try
            {
                string strOutputNumber = "";

                // 保存本类尾号
                int nRet = SaveTailNumber(this.textBox_tailNumber.Text,
                    out strOutputNumber,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 用检索得到的同类书中实际用到的最大号，试探性推动种次号库中的尾号
        private void button_pushTailNumber_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strOutputNumber = "";
            // 推动尾号
            int nRet = PushTailNumber(this.textBox_maxNumber.Text,
                out strOutputNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_tailNumber.Text = strOutputNumber;
            // MessageBox.Show(this, "推动尾号成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 得到当前书目中统计出来的最大号的加1以后的号
        // return:
        //      -1  error
        //      0   not found
        //      1   succeed
        public int GetMaxNumberPlusOne(out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";
            string strMaxNumber = "";

            try
            {
                strMaxNumber = this.MaxNumber;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strMaxNumber) == true)
                return 0;

            int nRet = StringUtil.IncreaseLeadNumber(strMaxNumber,
                1,
                out strResult,
                out strError);
            if (nRet == -1)
            {
                strError = "为数字 '" + strMaxNumber + "' 增量时发生错误: " + strError;
                goto ERROR1;

            }
            return 1;
        ERROR1:
            return -1;
        }

        // 复制比当前书目中统计出来的最大号还大1的号
        private void button_copyMaxNumber_Click(object sender, EventArgs e)
        {
            string strResult = "";
            string strError = "";

            // 得到当前书目中统计出来的最大号的加1以后的号
            // return:
            //      -1  error
            //      1   succeed
            int nRet = GetMaxNumberPlusOne(out strResult,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            if (nRet == 0)
                strResult = "1";    // 如果当前从书目中无法统计出最大号，则视为得到"0"，而加1以后正好为"1"

            // Clipboard.SetDataObject(strResult);
            StringUtil.RunClipboard(() =>
            {
                Clipboard.SetDataObject(strResult);
            });
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 检索两种：同类书、尾号
        private void button_searchDouble_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                this.textBox_tailNumber.Text = "";   // 预防filllist 提前退出, 忘记处理

                int nRet = FillList(true, out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 一并获得本类尾号
                nRet = PanelGetTailNumber(out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
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

        private void comboBox_biblioDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_biblioDbName.Items.Count > 0)
                return;

            // this.comboBox_biblioDbName.Items.Add("<全部>");

            /*
            for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
            {
                BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];
                this.comboBox_biblioDbName.Items.Add(property.DbName);
            }
             * */
            string strError = "";

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

            // 获得server url
            string strServerUrl = server.Url;

            List<string> dbnames = null;
            int nRet = GetBiblioDbNames(
                null,   // this.stop,
                this.LibraryServerName,
                strServerUrl,
                out dbnames,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            for (int i = 0; i < dbnames.Count; i++)
            {
                this.comboBox_biblioDbName.Items.Add(dbnames[i]);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得一个书目库名列表
        // parameters:
        //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
        //              如果==null，表示会自动使用this.stop，并自动OnStop+=
        // return:
        //      -1  error
        //      0   OK
        int GetBiblioDbNames(
            Stop stop,
            string strServerName,
            string strServerUrl,
            out List<string> dbnames,
            out string strError)
        {
            dbnames = new List<string>();
            strError = "";

            /*
            bool bInitialStop = false;
            if (stop == null)
            {
                stop = this.stop;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获得服务器 " + strServerUrl + " 的信息 ...");
                stop.BeginLoop();

                bInitialStop = true;
            }
            */
            var looping = BeginLoop(this.DoStop, "正在获得服务器 " + strServerUrl + " 的信息 ...");

            dp2ServerInfo info = null;

            try
            {
                info = this.MainForm.ServerInfos.GetServerInfo(
                    looping.Progress,
                    false,
#if OLD_CHANNEL
                    this.Channels,
#endif
                    strServerName,
                    strServerUrl,
                    this.MainForm.TestMode,
                    out strError);
                if (info == null)
                    return -1;
            }
            finally
            {
                /*
                if (bInitialStop == true)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
                */
                EndLoop(looping);
            }

            for (int i = 0; i < info.BiblioDbProperties.Count; i++)
            {
                BiblioDbProperty prop = info.BiblioDbProperties[i];

                dbnames.Add(prop.DbName);
            }

            return 0;
        }

        // 将面板上输入的线索数据库名或者种次号方案名变换为API使用的形态
        static string GetZhongcihaoDbGroupName(string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return "";

            // 如果第一个字符有!符号，表明是方案名
            if (strText[0] == '!')
                return strText.Substring(1);

            // 没有！符号，表明是线索数据库名
            return "!" + strText;
        }

        // 增量尾号
        public int IncreaseTailNumber(string strDefaultNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

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


            EnableControls(false);

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在增量尾号 ...");
            stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在增量尾号 ...");

            try
            {
                long lRet = Channel.SetZhongcihaoTailNumber(
                    looping.Progress,
                    "increase",
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strDefaultNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                */
                EndLoop(looping);

                EnableControls(true);
            }

        // return 0;
        ERROR1:
            return -1;
        }

        #region 协调外部调用的函数

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

        #endregion

        // 按照一定的策略，获得种次号
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        public int GetNumber(
            ZhongcihaoStyle style,
            string strClass,
            string strBiblioDbName,
            out string strNumber,
            out string strError)
        {
            strNumber = "";
            strError = "";
            int nRet = 0;

            this.ClassNumber = strClass;
            this.BiblioDbName = strBiblioDbName;

            // 仅利用书目统计最大号
            if (style == ZhongcihaoStyle.Biblio)
            {
                // 得到当前书目中统计出来的最大号的加1以后的号
                // return:
                //      -1  error
                //      1   succeed
                nRet = GetMaxNumberPlusOne(out strNumber,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                return 1;
            }



            // 每次都利用书目统计最大号来检验、校正尾号
            if (style == ZhongcihaoStyle.BiblioAndSeed
                || style == ZhongcihaoStyle.SeedAndBiblio)
            {

                string strTailNumber = this.TailNumber;

                // 如果本类尚未创建种次号条目
                if (String.IsNullOrEmpty(strTailNumber) == true)
                {
                    // 毕竟初始值还是利用了统计结果
                    string strTestNumber = "";
                    // 得到当前书目中统计出来的最大号的加1以后的号
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   succeed
                    nRet = GetMaxNumberPlusOne(out strTestNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                        strTestNumber = "1";

                    // 此类从来没有过记录，当前是第一条
                    strNumber = InputDlg.GetInput(
                        this,
                        null,
                        "请输入类 '" + strClass + "' 的当前种次号最大号:",
                        strTestNumber);
                    if (strNumber == null)
                        return 0;	// 放弃整个操作

                    // dlg.TailNumber = strNumber;	

                    nRet = PushTailNumber(strNumber,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return 1;
                }
                else // 本类已经有种次号条目
                {
                    // 检查和统计值的关系
                    string strTestNumber = "";
                    // 得到当前书目中统计出来的最大号的加1以后的号
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   succeed
                    nRet = GetMaxNumberPlusOne(out strTestNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                    {
                        // 依靠现有尾号增量即可
                        nRet = this.IncreaseTailNumber("1",
                            out strNumber,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        return 1;
                    }

                    // 用统计出来的号推动当前尾号，就起到了检验的作用
                    nRet = PushTailNumber(strTestNumber,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 如果到这里就返回，效果为保守型增量，即如果当前记录反复取号而不保存，则尾号不盲目增量。当然缺点也是很明显的 -- 有可能多个窗口取出重号来
                    if (style == ZhongcihaoStyle.BiblioAndSeed)
                        return 1;

                    if (strTailNumber != strNumber)  // 如果实际发生了推动，就要这个号，不必增量了
                        return 1;

                    // 依靠现有尾号增量
                    nRet = this.IncreaseTailNumber("1",
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return 1;
                }

                // return 1;
            }

            // 仅利用(种次号库)尾号
            if (style == ZhongcihaoStyle.Seed)
            {
                string strTailNumber = this.TailNumber;

                // 如果本类尚未创建种次号条目
                if (String.IsNullOrEmpty(strTailNumber) == true)
                {
                    // 毕竟初始值还是利用了统计结果
                    string strTestNumber = "";
                    // 得到当前书目中统计出来的最大号的加1以后的号
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   succeed
                    nRet = GetMaxNumberPlusOne(out strTestNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                        strTestNumber = "1";
                    // 此类从来没有过记录，当前是第一条
                    strNumber = InputDlg.GetInput(
                        this,
                        null,
                        "请输入类 '" + strClass + "' 的当前种次号最大号:",
                        strTestNumber);
                    if (strNumber == null)
                        return 0;	// 放弃整个操作

                    // dlg.TailNumber = strNumber;	

                    nRet = PushTailNumber(strNumber,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return 1;
                }
                else // 本类已经有种次号项目，增量即可
                {
                    nRet = this.IncreaseTailNumber("1",
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                return 1;
            }





            return 1;
        ERROR1:
            return -1;
        }

        // 双击：将书目记录装入详细窗
        private void listView_number_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_number.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入详细窗的事项");
                return;
            }
            string strPath = this.listView_number.SelectedItems[0].SubItems[0].Text;

            MessageBox.Show(this, "尚未实现");
            /*
            EntityForm form = new EntityForm();

            form.MdiParent = this.MainForm;

            form.MainForm = this.MainForm;
            form.Show();
            form.LoadRecord(strPath);
             * */

        }

        private void button_findServerName_Click(object sender, EventArgs e)
        {
            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

#if OLD_CHANNEL
            dlg.dp2Channels = this.Channels;
#endif
            dlg.ChannelManager = Program.MainForm;

            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_SERVER };
            dlg.Path = this.textBox_serverName.Text;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_serverName.Text = dlg.Path;
        }

        private void listView_number_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSelectedIndexChanged(this.listView_number,
    0,
    new List<int> { 1 });
        }
    }

    // 排序
    // Implements the manual sorting of items by columns.
    class ZhongcihaoListViewItemComparer : IComparer
    {
        public ZhongcihaoListViewItemComparer()
        {
        }

        public int Compare(object x, object y)
        {
            // 种次号字符串需要右对齐 2007/10/12
            string s1 = ((ListViewItem)x).SubItems[1].Text;
            string s2 = ((ListViewItem)y).SubItems[1].Text;

            int nMaxLength = Math.Max(s1.Length, s2.Length);
            s2 = s2.PadLeft(nMaxLength, '0');
            s1 = s1.PadLeft(nMaxLength, '0');

            return -1 * String.Compare(s1, s2);
        }
    }

    // 种次号取号的风格
    public enum ZhongcihaoStyle
    {
        Biblio = 1, // 仅利用书目统计最大号
        BiblioAndSeed = 2,  // 每次都利用书目统计最大号来检验、校正尾号。偏重书目统计值，不盲目增量尾号。
        SeedAndBiblio = 3, // 每次都利用书目统计最大号来检验、校正尾号。偏重尾号，每次都增量尾号
        Seed = 4, // 仅利用(种次号库)尾号
    }

}