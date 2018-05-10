using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Deployment.Application;
using System.Xml;

using UpgradeDt1000ToDp2.Properties;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

// using DigitalPlatform.CirculationClient.localhost;

namespace UpgradeDt1000ToDp2
{
    public partial class MainForm : Form
    {
        public string DataDir = "";

        //保存界面信息
        public ApplicationInfo AppInfo = new ApplicationInfo("upgradedt1000todp2.xml");


        #region Icon排列顺序常量

        public const int OFFS_FOLDER = 0;
        public const int OFFS_STDBASE = 1;
        public const int OFFS_SMDBASE = 2;
        public const int OFFS_STDFILE = 3;
        public const int OFFS_CFGFILE = 4;
        public const int OFFS_TCPS = 5;
        public const int OFFS_MYCOMPUTER = 6;
        public const int OFFS_NORMAL = 7;
        public const int OFFS_KERNEL = 8;
        public const int OFFS_FROM = 9;
        public const int OFFS_CDROM = 10;
        public const int OFFS_MYDESKTOP = 11;

        #endregion

        string EncryptKey = "upgradedt1000todp2_encryptkey";

        // DTLP协议
        DtlpChannelArray DtlpChannelArray = new DtlpChannelArray();
        DtlpChannel DtlpChannel = null;	// 尽量使用一个通道

        // dp2library协议
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();
        public DigitalPlatform.Stop stop = null;

        // gis*.ini文件全路径。有可能是临时文件
        public string GisIniFilePath = "";

        // ltqx*.cfg文件全路径，有可能是临时文件
        public string LtqxCfgFilePath = "";

        const int WM_LOADSIZE = API.WM_USER + 201;
        const int WM_SCROLLHTMLTOEND = API.WM_USER + 202;


        public MainForm()
        {
            InitializeComponent();

            this.listView_dtlpDatabases.LargeImageList = imageList_resIcon16;
            this.listView_dtlpDatabases.SmallImageList = imageList_resIcon16;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 初始化数据目录
            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                // MessageBox.Show(this, "no network");
                DataDir = Environment.CurrentDirectory;
            }


            stopManager.Initial(this.toolButton_stop,
                (object)this.toolStripStatusLabel1,
                (object)this.toolStripProgressBar1);

            stop = new DigitalPlatform.Stop();
            stop.Register(this.stopManager, true);	// 和容器关联

            // DTLP
            this.textBox_dtlpAsAddress.Text = Settings.Default.DtlpAsAddress;
            this.textBox_dtlpUserName.Text = Settings.Default.DtlpUserName;

            this.checkBox_dtlpSavePassword.Checked = Settings.Default.DtlpSavePassword;

            if (this.checkBox_dtlpSavePassword.Checked == true)
            {
                string strPassword = Settings.Default.DtlpPassword;

                strPassword = this.DecryptPasssword(strPassword);

                this.textBox_dtlpPassword.Text = strPassword;
            }

            // DTLP协议
            this.DtlpChannelArray.AskAccountInfo += new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);
            // 准备唯一的通道
            if (this.DtlpChannel == null)
            {
                this.DtlpChannel = DtlpChannelArray.CreateChannel(0);
            }

            // dp2library协议
            this.textBox_dp2AsUrl.Text = Settings.Default.Dp2AsUrl;
            this.textBox_dp2UserName.Text = Settings.Default.Dp2UserName;

            this.checkBox_dp2SavePassword.Checked = Settings.Default.Dp2SavePassword;

            if (this.checkBox_dp2SavePassword.Checked == true)
            {
                string strPassword = Settings.Default.Dp2Password;

                strPassword = this.DecryptPasssword(strPassword);

                this.textBox_dp2Password.Text = strPassword;
            }

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
                case WM_SCROLLHTMLTOEND:
                    Global.ScrollToEnd(this.webBrowser_info);
                    return;
            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            if (AppInfo != null)
            {
                AppInfo.LoadFormStates(this,
                    "mainformstate",
                    FormWindowState.Maximized);
            }


            try
            {

                // 获得splitContainer_main的状态
                int nValue = this.AppInfo.GetInt(
                "mainform_state",
                "splitContainer_main",
                -1);
                if (nValue != -1)
                    this.splitContainer_main.SplitterDistance = nValue;


                // 初始化浏览器控件
                this.EnableControls(false);
                stop.SetMessage("正在初始化浏览器控件，请稍候...");
                this.Update();

                Global.Clear(this.webBrowser_info);
                Global.WriteHtml(this.webBrowser_info,
                    "<html><head></head><body>");

                stop.SetMessage("");
                this.EnableControls(true);

            }
            catch
            {
            }

        }

        public void SaveSize()
        {
            // 保存窗口尺寸状态
            if (this.AppInfo != null)
            {
                // 保存splitContainer_main的状态
                this.AppInfo.SetInt(
                    "mainform_state",
                    "splitContainer_main",
                    this.splitContainer_main.SplitterDistance);

                this.AppInfo.SaveFormStates(this,
                    "mainformstate");
            }

        }

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            stopManager.DoStopActive();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        void channelArray_AskAccountInfo(object sender, AskDtlpAccountInfoEventArgs e)
        {
            e.Owner = null;
            e.UserName = "";
            e.Password = "";

            LoginDlg dlg = new LoginDlg();

            dlg.textBox_serverAddr.Text = e.Path;
            dlg.textBox_userName.Text = this.textBox_dtlpUserName.Text;
            dlg.textBox_password.Text = this.textBox_dtlpPassword.Text;

            // 先登录一次再说
            {
                byte[] baResult = null;
                int nRet = e.Channel.API_ChDir(this.textBox_dtlpUserName.Text,
                    this.textBox_dtlpPassword.Text,
                    e.Path,
                    out baResult);

                // 登录成功
                if (nRet > 0)
                {
                    e.Result = 2;
                    return;
                }
            }


            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.OK)
            {
                this.textBox_dtlpUserName.Text = dlg.textBox_userName.Text;
                this.textBox_dtlpPassword.Text = dlg.textBox_password.Text;

                e.UserName = dlg.textBox_userName.Text;
                e.Password = dlg.textBox_password.Text;
                e.Owner = this;
                e.Result = 1;
                return;
            }

            e.Result = 0;
            return;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 将Web控件中的内容全部保存到日志文件
            string strText = this.webBrowser_info.DocumentText;

            if (String.IsNullOrEmpty(strText) == false)
            {
                AppendHtml(
    "====================<br/>"
    + "升级程序退出时间: <br/>" + DateTime.Now.ToString() + "<br/>"
    + "====================<br/><br/>");

                strText = this.webBrowser_info.DocumentText;

                // 找到一个新文件名
                string strLogFilename = "";
                for (int i = 0; ; i++)
                {
                    strLogFilename = PathUtil.MergePath(this.DataDir, "log_" + DateTimeUtil.DateTimeToString8(DateTime.Now) + "_" + (i + 1).ToString() + ".html");
                    if (File.Exists(strLogFilename) == false)
                        break;
                }
                StreamUtil.WriteText(strLogFilename, strText);
            }

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null)
            {
                stop.Unregister(); // 脱离关联
                stop = null;
            }

            // DTLP
            Settings.Default.DtlpAsAddress = this.textBox_dtlpAsAddress.Text;
            Settings.Default.DtlpUserName = this.textBox_dtlpUserName.Text;
            Settings.Default.DtlpSavePassword = this.checkBox_dtlpSavePassword.Checked;

            if (this.checkBox_dtlpSavePassword.Checked == true)
            {
                string strPassword = this.EncryptPassword(this.textBox_dtlpPassword.Text);
                Settings.Default.DtlpPassword = strPassword;
            }
            else
            {
                Settings.Default.DtlpPassword = "";
            }

            // dp2library
            Settings.Default.Dp2AsUrl = this.textBox_dp2AsUrl.Text;
            Settings.Default.Dp2UserName = this.textBox_dp2UserName.Text;
            Settings.Default.Dp2SavePassword = this.checkBox_dp2SavePassword.Checked;

            if (this.checkBox_dp2SavePassword.Checked == true)
            {
                string strPassword = this.EncryptPassword(this.textBox_dp2Password.Text);
                Settings.Default.Dp2Password = strPassword;
            }
            else
            {
                Settings.Default.Dp2Password = "";
            }

            Settings.Default.Save();

            this.DtlpChannelArray.AskAccountInfo += new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);

            /*
            // 保存窗口尺寸状态
            if (AppInfo != null)
            {
                AppInfo.SaveFormStates(this,
                    "mainformstate");
            }*/
            this.SaveSize();

            //记住save,保存信息XML文件
            AppInfo.Save();
            AppInfo = null;	// 避免后面再用这个对象		
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

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.button_next.Text == "结束(&X)")
            {
                this.Close();
                return;
            }


            if (this.tabControl_main.SelectedTab == this.tabPage_inputDt1000ServerInfo)
            {
                // 检查输入是否都具备？
                if (this.textBox_dtlpAsAddress.Text == "")
                {
                    strError = "尚未输入dt1000应用服务器地址";
                    goto ERROR1;
                }

                if (this.textBox_dtlpUserName.Text == "")
                {
                    strError = "尚未输入dt1000 用户名";
                    goto ERROR1;
                }

                // 进行登录检验
                EnableControls(false);
                try
                {
                    AppendHtml(
                        "====================<br/>"
                        + "升级程序启动时间: <br/>" + DateTime.Now.ToString() + "<br/>"
                        + "====================<br/><br/>");

                    AppendHtml(
                        "====================<br/>"
                        + "登录到dt1000应用服务器<br/>"
                        + "====================<br/><br/>");


                    nRet = DetectLoginToDtlpServer(out strError);
                    if (nRet != 1)
                    {
                        strError = "登录到dt1000应用服务器 " + this.textBox_dtlpAsAddress.Text + " 不成功: " + strError;
                        goto ERROR1;
                    }

                    AppendHtml(
                        "登录成功<br/><br/>");

                }
                finally
                {
                    EnableControls(true);
                }

                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_locateGisIniFile;


            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_locateGisIniFile)
            {
                if (this.textBox_gisIniFileContent.Text == "")
                {
                    // 检查输入
                    if (this.textBox_gisIniFilePath.Text == "")
                    {
                        strError = "尚未指定gis.ini(或gis2000.ini)文件全路径";
                        goto ERROR1;
                    }

                    // 检查文件是否存在？
                    if (File.Exists(this.textBox_gisIniFilePath.Text) == false)
                    {
                        strError = "文件 " + this.textBox_gisIniFilePath.Text + "并不存在...";
                        goto ERROR1;
                    }

                    // 装入文件内容
                    nRet = LoadGisIniFileContent(this.textBox_gisIniFileContent.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_selectDt1000Database;

                nRet = FillDtlpDatabaseNames(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 根据gis*.ini文件中的联接的书目库和读者库，给出据库列表设上初步分析的类型
                SetDatabaseTypesByGisIniFileContent();

                // 根据数据库名对数据库类型做出一些预测
                GuessDatabaseTypes();

                // 2008/8/19 new add
                string strLtqxCfgFilename = "";
                // 获得ltqx*.cfg配置文件内容
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetLtqxCfgFilename(out strLtqxCfgFilename,
                    out strError);
                this.textBox_rights_ltxqCfgFilePath.Text = strLtqxCfgFilename;

            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_selectDt1000Database)
            {
                // 是否checkbox了要升级的数据库事项？是否有设置了类型但是没有checkbox的数据库?
                if (this.listView_dtlpDatabases.CheckedItems.Count == 0)
                {
                    strError = "尚未勾选任何一个要升级到dp2的数据库";
                    goto ERROR1;
                }

                // 警告那些设置了类型但是并未勾选的事项
                // return:
                //      0   没有警告
                //      1   警告了，用户选择继续
                //      2   警告了，用户选择返回
                nRet = WarningTypedButUncheckedDatabaseItem();
                if (nRet == 2)
                    return;

                // 警告那些空的和不支持的数据库类型
                // return:
                //      0   没有警告
                //      1   警告了，用户选择继续
                //      2   警告了，用户选择返回
                nRet = WarningDatabaseType();
                if (nRet == 2)
                    return;


                // 检查类型是否设置正确？是否至少有一个书目库和读者库参与流通？(未参与流通的书目库需要同时确认一下)

                // 抽查数据库内前10条记录，看看选择的类型是否正确?

                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_inputDp2ServerInfo;

            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_inputDp2ServerInfo)
            {
                // 检查输入是否都具备？
                if (this.textBox_dp2AsUrl.Text == "")
                {
                    strError = "尚未输入dp2应用服务器的WebService URL地址";
                    goto ERROR1;
                }

                if (this.textBox_dp2UserName.Text == "")
                {
                    strError = "尚未输入dp2用户名";
                    goto ERROR1;
                }

                this.Channel.Url = this.textBox_dp2AsUrl.Text;

                this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

                // 进行登录检验
                EnableControls(false);
                try
                {
                    AppendHtml(
    "====================<br/>"
    + "登录到dp2应用服务器<br/>"
    + "====================<br/><br/>");


                    nRet = DetectLoginToDp2Server(out strError);
                    if (nRet != 1)
                    {
                        strError = "登录到dp2应用服务器 " + this.textBox_dp2AsUrl.Text + " 不成功: " + strError;
                        goto ERROR1;
                    }

                    AppendHtml(
    "登录成功<br/><br/>");

                }
                finally
                {
                    EnableControls(true);
                }


                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_createTargetDatabase;

                // 获得dp2中所有已经存在的数据库
                nRet = this.ListAllExistingDp2Databases(out strError);
                if (nRet == -1)
                    goto ERROR1;
                // 列出要创建的dp2数据库。对其中已经存在的，要进行警告
                nRet = this.ListCreatingDp2Databases(out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_createDp2DatabaseSummary.Text = "";

            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_createTargetDatabase)
            {
                // 如果已经创建了数据库，再次按next按钮
                if (this.textBox_createDp2DatabaseSummary.Text != "")
                {
                    // 切换到下一属性页
                    this.tabControl_main.SelectedTab = this.tabPage_copyDatabase;
                    return;
                }

                // 创建新的数据库。创建之前，还要删除已经存在的同名数据库
                // return:
                //      -1  出错
                //      0   放弃删除和创建
                //      1   成功
                nRet = this.CreateNewDp2Databases(out strError);
                if (nRet == 0)
                    return;
                if (nRet == -1)
                    goto ERROR1;


                // 创建辅助库
                nRet = this.CreateDp2SimpleDatabases(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_copyDatabase;

            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_copyDatabase)
            {

                List<string> reader_dbnames = null;
                GetReaderDbNames(out reader_dbnames);
                if (reader_dbnames.Count > 0)
                {
                    bool bWarning = false;
                    string strText = "是否对即将要进行数据升级的类型为'读者库'的 " + Global.MakeListString(reader_dbnames, ",") + " 进行读者条码重复检查?\r\n\r\n(Yes: 检查; No: 不检查)\r\n\r\n注：在读者库数据升级过程中，如果发现有证条码重复的情况，将简单忽略后出现的读者记录，可能会导致数据问题被复杂化。最好的办法是选择检查证条码重复情况，并在升级以前解决这些问题。";
                    DialogResult result = MessageBox.Show(this,
            strText,
            "UpgradeDt1000ToDp2",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        // 检查看看读者库中的记录有没有使用重复的证条码
                        // return:
                        //      -1  error
                        //      0   没有问题
                        //      1   有重复证条码问题
                        nRet = VerifyDupReaderBarcode(reader_dbnames,
                            out bWarning,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1 || bWarning == true)
                        {
                            // 将next按钮修改为finish按钮，然后禁止使用后面的所有page
                            // 注意如果用户重新从前面的page走过来，没有发现重复问题，后面被禁止的page要放开允许使用
                            VisiblePages(false);
                            this.button_next.Text = "结束(&X)";
                            return;
                        }
                        else
                        {
                            VisiblePages(true);
                            this.button_next.Text = "继续(&C)";
                        }

                    }
                }


                List<string> issue_dbnames = null;
                GetIssueDbNames(out issue_dbnames);
                if (issue_dbnames.Count > 0)
                {
                    bool bWarning = false;
                    string strText = "是否对即将要进行数据升级的类型为'书目库,期刊'的 " + Global.MakeListString(issue_dbnames, ",") + " 进行数据检查?\r\n\r\n(Yes: 检查; No: 不检查)\r\n\r\n注：在期刊库数据升级过程中，如果发现同一日的期数据有不一致情况，将简单忽略差异，可能会导致数据问题被复杂化。最好的办法是选择检查期刊数据，并在升级以前解决这些问题。";
                    DialogResult result = MessageBox.Show(this,
            strText,
            "UpgradeDt1000ToDp2",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        // return:
                        //      -1  error
                        //      0   没有问题
                        //      1   有问题
                        nRet = VerifyIssueInfo(issue_dbnames,
                            out bWarning,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1 || bWarning == true)
                        {
                            // 将next按钮修改为finish按钮，然后禁止使用后面的所有page
                            // 注意如果用户重新从前面的page走过来，没有发现重复问题，后面被禁止的page要放开允许使用
                            VisiblePages(false);
                            this.button_next.Text = "结束(&X)";
                            return;
                        }
                        else
                        {
                            VisiblePages(true);
                            this.button_next.Text = "继续(&C)";
                        }

                    }
                }

                // 复制dtlp数据库内的全部数据到对应的dp2数据库中
                nRet = CopyDatabases(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_verifyLoan;
                return;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_verifyLoan)
            {
                // 整理流通信息
                // return:
                //      -1  error
                //      0   没有命中的读者记录
                //      1   正常处理完成
                nRet = VerifyLoan(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 切换到下一属性页
                this.tabControl_main.SelectedTab = this.tabPage_upgradeReaderRights;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_upgradeReaderRights)
            {
                if (this.textBox_rights_ltqxCfgContent.Text == "")
                {
                    // 检查输入
                    if (this.textBox_rights_ltxqCfgFilePath.Text == "")
                    {
                        strError = "尚未指定ltqx*.cfg配置文件全路径";
                        goto ERROR1;
                    }

                    // 检查文件是否存在？
                    if (File.Exists(this.textBox_rights_ltxqCfgFilePath.Text) == false)
                    {
                        strError = "文件 " + this.textBox_rights_ltxqCfgFilePath.Text + "并不存在...";
                        goto ERROR1;
                    }

                    // 装入文件内容
                    nRet = this.LoadLtqxCfgFileContent(this.textBox_rights_ltxqCfgFilePath.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 升级权限参数
                nRet = UpgradeReaderRightsParam(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 升级日历
                nRet = UpgradeCalendar(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 设置OPAC参数
                nRet = SetOpacDatabaseDef(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 升级种次号配置参数
                nRet = UpgradeZhongcihaoParam(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 切换到下一属性页
                // 结束
                AppendHtml(
    "====================<br/>"
    + "升级操作全部完成<br/>"
    + "====================<br/><br/>");
                this.button_next.Text = "结束(&X)";


                return;
            }


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 2008/8/26 new add
        List<TabPage> m_hiddenPages = new List<TabPage>();

        void VisiblePages(bool bVisible)
        {
            if (bVisible == false)
            {
                if (this.m_hiddenPages.Count > 0)
                    return; // 已经隐藏了
                this.m_hiddenPages.Add(this.tabPage_verifyLoan);
                this.m_hiddenPages.Add(this.tabPage_upgradeReaderRights);

                this.tabControl_main.TabPages.Remove(this.tabPage_verifyLoan);
                this.tabControl_main.TabPages.Remove(this.tabPage_upgradeReaderRights);
            }
            else
            {
                // 
                if (this.m_hiddenPages.Count == 0)
                    return; // 以前从来没有隐藏过，所以此时谈不上重新出现

                for (int i = 0; i < this.m_hiddenPages.Count; i++)
                {
                    this.tabControl_main.TabPages.Add(this.m_hiddenPages[i]);
                }

                this.m_hiddenPages.Clear();
            }
        }

        // 根据gis*.ini文件中的联接的书目库和读者库，给出据库列表设上初步分析的类型
        /*
[读者库]
//库1=192.168.1.111/读者库,证条码号,7,可用
库1=[srv]/读者库,证条码号,7,可用

[书目库]
//库1=192.168.1.111/图书总库,册条码,8,可用
库1=[srv]/图书总库,册条码,8,可用
         * * */
        void SetDatabaseTypesByGisIniFileContent()
        {
            string strError = "";
            int nRet = 0;

            this.GisIniFilePath = "";
            if (this.textBox_gisIniFilePath.Text == "")
            {
                Debug.Assert(this.textBox_gisIniFileContent.Text != "", "gis.ini文件内容是必须有的");
                // 如果没有gis*.ini文件名而有文件内容
                // 则将文件内容写入一个临时文件，以便GetPrivateProfileString() API能够执行
                this.GisIniFilePath = PathUtil.MergePath(this.DataDir, "temp_gis.ini");

                try
                {
                    StreamWriter sw = new StreamWriter(this.GisIniFilePath, false, Encoding.GetEncoding(936));
                    sw.Write(this.textBox_gisIniFileContent.Text);
                    sw.Close();
                }
                catch (Exception ex)
                {
                    strError = "文件 " + this.GisIniFilePath + " 创建过程发生错误: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
            {
                this.GisIniFilePath = this.textBox_gisIniFilePath.Text;
            }

            List<DatabaseProperty> reader_dbnames = new List<DatabaseProperty>();   // 参与流通的读者库
            for (int i = 0; ; i++)
            {
                string strEntry = "库" + (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("读者库",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.GisIniFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                string strDbPath = "";
                string strFrom = "";
                string strBarcodeLength = "";
                string strCanUse = "";

                // 解析出4个部分
                ParseFourPart(strLine,
                    out strDbPath,
                    out strFrom,
                    out strBarcodeLength,
                    out strCanUse);

                DatabaseProperty property = new DatabaseProperty();
                property.DbName = GetDbNameFromLinkString(strDbPath);
                property.CanUse = strCanUse;
                try
                {
                    property.BarcodeLength = Convert.ToInt32(strBarcodeLength);
                }
                catch
                {
                }

                reader_dbnames.Add(property);
            }

            List<DatabaseProperty> biblio_dbnames = new List<DatabaseProperty>();   // 参与流通的书目库(不一定是全部书目库)
            for (int i = 0; ; i++)
            {
                string strEntry = "库" + (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("书目库",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.GisIniFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                string strDbPath = "";
                string strFrom = "";
                string strBarcodeLength = "";
                string strCanUse = "";

                // 解析出4个部分
                ParseFourPart(strLine,
                    out strDbPath,
                    out strFrom,
                    out strBarcodeLength,
                    out strCanUse);

                DatabaseProperty property = new DatabaseProperty();
                property.DbName = GetDbNameFromLinkString(strDbPath);
                property.CanUse = strCanUse;
                try
                {
                    property.BarcodeLength = Convert.ToInt32(strBarcodeLength);
                }
                catch
                {
                }

                biblio_dbnames.Add(property);
            }

            // 设定数据库类型
            for (int i = 0; i < biblio_dbnames.Count; i++)
            {
                string strDbName = biblio_dbnames[i].DbName;

                ListViewItem item = ListViewUtil.FindItem(this.listView_dtlpDatabases,
                    strDbName, 0);
                if (item == null)
                    continue;   // 是否要警告呢?
                string strOldText = ListViewUtil.GetItemText(item, 1);
                if (String.IsNullOrEmpty(strOldText) == false)
                    strOldText += ",";
                strOldText += "书目库";
                ListViewUtil.ChangeItemText(item, 1, strOldText);

                if (biblio_dbnames[i].CanUse == "可用")
                    ListViewUtil.ChangeItemText(item, 2, "是");
            }

            for (int i = 0; i < reader_dbnames.Count; i++)
            {
                string strDbName = reader_dbnames[i].DbName;

                ListViewItem item = ListViewUtil.FindItem(this.listView_dtlpDatabases,
                    strDbName, 0);
                if (item == null)
                    continue;   // 是否要警告呢?
                string strOldText = ListViewUtil.GetItemText(item, 1);
                if (String.IsNullOrEmpty(strOldText) == false)
                    strOldText += ",";
                strOldText += "读者库";
                ListViewUtil.ChangeItemText(item, 1, strOldText);

                if (reader_dbnames[i].CanUse == "可用")
                    ListViewUtil.ChangeItemText(item, 2, "是");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据数据库名对数据库类型做出一些预测
        void GuessDatabaseTypes()
        {
            for (int i = 0; i < this.listView_dtlpDatabases.Items.Count; i++)
            {
                ListViewItem item = this.listView_dtlpDatabases.Items[i];

                string strOriginType = ListViewUtil.GetItemText(item, 1);

                /*
                // 跳过那些已经有类型的事项
                // 仅仅对完全没有类型值的数据库名进行猜测
                if (String.IsNullOrEmpty(strType) == false)
                {
                    continue;
                }*/

                string strDatabaseName = item.Text;
                string strType = "";

                if (strDatabaseName == "规范库")
                {
                    strType = "规范库,UNIMARC";
                }
                else if (strDatabaseName.Trim().ToLower() == "LTRizhi".ToLower())
                {
                    strType = "流通日志库";
                }

                else if (strDatabaseName == "辅助库"
                    || strDatabaseName == "ISBN"
                    || strDatabaseName == "种次号")
                {
                    strType = "辅助库";
                }
                else if (strDatabaseName == "源书目")
                    strType = "书目库,图书,UNIMARC";
                else if (strDatabaseName == "中文图书"
                    || strDatabaseName == "图书总库"
                    || strDatabaseName == "图书编目"
                    || strDatabaseName == "工作库")
                    strType = "书目库,图书,UNIMARC,实体";
                else if (strDatabaseName == "采购库"
                    || strDatabaseName == "图书采购")
                    strType = "书目库,图书,UNIMARC,采购";
                else if (strDatabaseName == "期刊源书目")
                    strType = "书目库,期刊,UNIMARC,采购";
                else if (strDatabaseName == "中文期刊"
               || strDatabaseName == "中文期刊库"
               || strDatabaseName == "期刊编目"
               || strDatabaseName == "期刊工作库")
                    strType = "书目库,期刊,UNIMARC,实体,采购"; // 期刊库一般都是用于采购的
                else if (strDatabaseName == "期刊篇名"
                    || strDatabaseName == "期刊篇名库"
                    || strDatabaseName == "中文期刊篇名")
                    strType = "书目库,UNIMARC";    // 篇名库没有采购、实体、期

                else if (strDatabaseName == "期刊采购库"
                    || strDatabaseName == "期刊采购"
                    || strDatabaseName == "中文期刊采购")
                    strType = "书目库,期刊,UNIMARC,采购";
                else if (strDatabaseName == "English Books"
                    || strDatabaseName == "英文图书")
                    strType = "书目库,图书,USMARC,实体";
                else if (strDatabaseName == "英文图书采购"
                    || strDatabaseName == "西文图书采购"
                    || strDatabaseName == "外文图书采购")
                    strType = "书目库,UNIMARC,采购";
                else if (strDatabaseName == "英文期刊"
                    || strDatabaseName == "英文期刊库"
                    || strDatabaseName == "English Series"
                    || strDatabaseName == "期刊编目"
                    || strDatabaseName == "西文期刊"
                    || strDatabaseName == "外文期刊"
                    || strDatabaseName == "期刊源书目")
                    strType = "书目库,期刊,USMARC,实体,采购";    // 期刊库一般都是用于采购的


                // 已经由原始类型的。所谓原始类型就是从gis.ini中获得的一些基本的类型
                if (String.IsNullOrEmpty(strOriginType) == false)
                {
                    if (StringUtil.IsInList("书目库", strOriginType) == true)
                    {
                        // 去掉和“书目库”矛盾的类型
                        StringUtil.SetInList(ref strType, "规范库", false);
                        StringUtil.SetInList(ref strType, "流通日志库", false);
                        StringUtil.SetInList(ref strType, "辅助库", false);
                        StringUtil.SetInList(ref strType, "读者库", false);

                        // 加上“书目库”类型
                        StringUtil.SetInList(ref strType, "书目库", true);

                    }
                    else if (StringUtil.IsInList("读者库", strOriginType) == true)
                    {
                        /*
                        // 去掉和“读者库”矛盾的类型
                        StringUtil.SetInList(ref strType, "规范库", false);
                        StringUtil.SetInList(ref strType, "流通日志库", false);
                        StringUtil.SetInList(ref strType, "辅助库", false);
                        StringUtil.SetInList(ref strType, "书目库", false);
                         * */

                        // 直接设为原始类型
                        strType = strOriginType;
                    }
                }


                if (strType != "")
                    ListViewUtil.ChangeItemText(item, 1, strType);

                /*
                // 根据角色设置正文颜色
                if (strType == ""
                    || StringUtil.IsInList("规范库", strType) == true
                    || StringUtil.IsInList("流通日志库", strType) == true
                    || StringUtil.IsInList("辅助库", strType) == true)
                {
                    item.ForeColor = SystemColors.GrayText;
                }*/
                SetDtlpDatabaseItemColor(item);
            }
        }

        static void SetDtlpDatabaseItemColor(ListViewItem item)
        {
            string strType = ListViewUtil.GetItemText(item, 1);

            string strSub = "";
            bool bRet = IsUnUpgradeType(strType,
                out strSub);

            // 根据角色设置正文颜色
            if (bRet == true)
            {
                item.ForeColor = SystemColors.GrayText;
            }
            else
            {
                item.ForeColor = SystemColors.WindowText;
            }

            if (item.Checked == true)
            {
                item.Font = new Font(item.Font, FontStyle.Bold);
                item.BackColor = Color.LightYellow;
            }
            else
            {
                item.Font = new Font(item.Font, FontStyle.Regular);
                item.BackColor = SystemColors.Window;
            }
        }

        // 是否为升级不支持的类型?
        // parameters:
        //      strSub  返回导致不支持升级的实际类型
        // return:
        //      false   支持升级
        //      true    不支持升级。strSub中返回了不支持升级的实际类型
        static bool IsUnUpgradeType(string strType,
            out string strSub)
        {
            strSub = "(空)";
            if (String.IsNullOrEmpty(strType) == true)
                return true;

            strSub = "流通日志库";
            if (StringUtil.IsInList(strSub, strType) == true)
                return true;

            strSub = "辅助库";
            if (StringUtil.IsInList(strSub, strType) == true)
                return true;

            strSub = "规范库";
            if (StringUtil.IsInList(strSub, strType) == true)
                return true;

            return false;
        }

        // 从字符串"[srv]/读者库"中解析出数据库名
        static string GetDbNameFromLinkString(string strLinkString)
        {
            int nRet = strLinkString.LastIndexOf("/");
            if (nRet == -1)
                return strLinkString.Trim();

            return strLinkString.Substring(nRet + 1).Trim();
        }

        // [srv]/读者库,证条码号,7,可用
        static void ParseFourPart(string strLine,
            out string strDbPath,
            out string strFrom,
            out string strBarcodeLength,
            out string strCanUse)
        {
            strDbPath = "";
            strFrom = "";
            strBarcodeLength = "";
            strCanUse = "";

            string[] parts = strLine.Split(new char[] { ',' });
            if (parts.Length > 0)
                strDbPath = parts[0];
            if (parts.Length > 1)
                strFrom = parts[1];
            if (parts.Length > 2)
                strBarcodeLength = parts[2];
            if (parts.Length > 3)
                strCanUse = parts[3];
        }

        // return:
        //      1   登录成功
        //      <=0 登录失败 strError中有原因
        int DetectLoginToDtlpServer(out string strError)
        {
            strError = "";

            byte[] baResult = null;
            int nRet = this.DtlpChannel.API_ChDir(this.textBox_dtlpUserName.Text,
                this.textBox_dtlpPassword.Text,
                this.textBox_dtlpAsAddress.Text,
                out baResult);
            // 登录成功
            if (nRet > 0)
            {
                return 1;
            }

            strError = this.DtlpChannel.GetErrorDescription();
            return nRet;
        }

        void EnableControls(bool bEnable)
        {
            this.tabControl_main.Enabled = bEnable;
            this.button_next.Enabled = bEnable;
            this.Update();
            Application.DoEvents();
        }

        #region 列出要升级的dt1000数据库

        // strStart, // 起始路径, ""表示根
        Package GetOneLevelDirPackage(string strStart,
            out string strError)
        {
            strError = "";
            int nRet;
            byte[] baPackage = null;

            // bool bSetDefault = false;	// 表示是否使用过缺省帐户

            //bool bFirstLogin = true;

            Package package = new Package();

            // CWaitCursor cursor;
            if (this.DtlpChannel == null)
            {
                this.DtlpChannel = this.DtlpChannelArray.CreateChannel(0);
            }

            Debug.Assert(this.DtlpChannel != null, "channel尚未初始化");

            Cursor.Current = Cursors.WaitCursor;

            nRet = this.DtlpChannel.Dir(strStart,
                out baPackage);

            Cursor.Current = Cursors.Default;

            if (nRet == -1)
            {
                strError = this.DtlpChannel.GetErrorDescription();
                goto ERROR1;
            }


            package.LoadPackage(baPackage, this.DtlpChannel.GetPathEncoding(strStart));
            package.Parse(PackageFormat.String);

            return package;
        ERROR1:
            return null;
        }

        int FillDtlpDatabaseNames(out string strError)
        {
            strError = "";
            Package package = null;

            this.listView_dtlpDatabases.Items.Clear();

            string strPath = this.textBox_dtlpAsAddress.Text;

            package = GetOneLevelDirPackage(strPath,
                out strError);
            if (package == null)
            {
                strError = "列dt1000书目库名发生错误: " + strError;
                return -1;
            }

            for (int i = 0; i < package.Count; i++)
            {
                Cell cell = (Cell)package[i];

                // TypeCdbase的不要
                if ((cell.Mask & DtlpChannel.TypeCdbase) == DtlpChannel.TypeCdbase)
                    continue;


                if ((cell.Mask & DtlpChannel.TypeStdbase) == DtlpChannel.TypeStdbase
                    || (cell.Mask & DtlpChannel.TypeSmdbase) == DtlpChannel.TypeSmdbase)
                {
                }
                else
                    continue;

                string strCurPath = cell.Path;
                if (strPath != "")
                {
                    // 前方应当完全一致
                    Debug.Assert(strCurPath.Length >= strPath.Length + 1);
                    strCurPath = strCurPath.Remove(0, strPath.Length + 1);
                }

                int nImage = GetImageIndex(cell.Mask);

                ListViewItem item = new ListViewItem();
                item.Text = strCurPath;
                item.ImageIndex = nImage;

                /*
                if ((cell.Mask & DtlpChannel.AttrExtend) != 0)
                {
                    SetLoading(nodeNew);
                }*/

                this.listView_dtlpDatabases.Items.Add(item);
            }

            return 0;
        }

        // 根据掩码计算出ICON图象下标
        static int GetImageIndex(Int32 lMask)
        {

            if ((lMask & DtlpChannel.TypeStdbase) != 0)
            {
                if ((lMask & DtlpChannel.AttrRdOnly) != 0)
                    return OFFS_CDROM;
            }

            if ((lMask & DtlpChannel.TypeStdbase) != 0)
            {
                return OFFS_STDBASE;
            }
            if ((lMask & DtlpChannel.TypeSmdbase) != 0)
                return OFFS_SMDBASE;
            if ((lMask & DtlpChannel.TypeStdfile) != 0)
                return OFFS_STDFILE;
            if ((lMask & DtlpChannel.TypeCfgfile) != 0)
                return OFFS_CFGFILE;
            if ((lMask & DtlpChannel.AttrTcps) != 0)
                return OFFS_TCPS;
            if ((lMask & DtlpChannel.TypeKernel) != 0)
                return OFFS_MYCOMPUTER;
            if ((lMask & DtlpChannel.TypeFrom) != 0)
                return OFFS_FROM;
            return 0;
        }

        private void button_selectAllDtlpDatabase_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_dtlpDatabases.Items.Count; i++)
            {
                ListViewItem item = this.listView_dtlpDatabases.Items[i];

                string strType = ListViewUtil.GetItemText(item, 1);

                string strSub = "";
                bool bRet = IsUnUpgradeType(strType,
                    out strSub);

                if (bRet == false && item.Checked == false)
                {
                    this.listView_dtlpDatabases.Items[i].Checked = true;
                }
            }
        }

        private void button_unSelectAllDtlpDatabase_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_dtlpDatabases.Items.Count; i++)
            {
                this.listView_dtlpDatabases.Items[i].Checked = false;
            }

        }

        private void listView_dtlpDatabases_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

            if (e.Item.Checked == true)
            {
                string strType = ListViewUtil.GetItemText(e.Item, 1);

                string strSub = "";
                bool bRet = IsUnUpgradeType(strType,
                    out strSub);

                if (bRet == true)
                {
                    MessageBox.Show(this, "数据库类型 '" + strSub + "' 不支持其升级，因而不能被勾选...");
                    e.Item.Checked = false;
                    return;
                }
            }

            SetDtlpDatabaseItemColor(e.Item);

            if (this.listView_dtlpDatabases.CheckedItems.Count == 0)
                this.label_selectedDatabasesCount.Text = "您尚未选定要升级的数据库";
            else
                this.label_selectedDatabasesCount.Text = "选定了 " + this.listView_dtlpDatabases.CheckedItems.Count.ToString() + " 个要升级的数据库";
        }

        // 设置dt1000数据库的属性(类型等)
        private void button_setDtlpDatabaseProperty_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_dtlpDatabases.SelectedItems.Count == 0)
            {
                strError = "尚未选定要设置类型的数据库事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_dtlpDatabases.SelectedItems[0];

            SourceDatabasePropertyDialog dlg = new SourceDatabasePropertyDialog();

            dlg.DatabaseName = item.Text;
            dlg.TypeString = ListViewUtil.GetItemText(item, 1);
            if (ListViewUtil.GetItemText(item, 2) == "是")
                dlg.InCirculation = true;
            else
                dlg.InCirculation = false;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            item.Text = dlg.DatabaseName;
            ListViewUtil.ChangeItemText(item, 1, dlg.TypeString);
            string strInCirculation = "";
            if (dlg.InCirculation == true)
                strInCirculation = "是";
            else
                strInCirculation = "";  // TODO: 需要设置为“否”么?

            ListViewUtil.ChangeItemText(item, 2, strInCirculation);

            SetDtlpDatabaseItemColor(item);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 在事项上双击
        private void listView_dtlpDatabases_DoubleClick(object sender, EventArgs e)
        {
            button_setDtlpDatabaseProperty_Click(sender, e);
        }

        // 改变按钮的状态
        private void listView_dtlpDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_dtlpDatabases.SelectedItems.Count == 0)
            {
                this.button_setDtlpDatabaseProperty.Text = "设置数据库的类型(&T)...";
                this.button_setDtlpDatabaseProperty.Enabled = false;
            }
            else
            {
                this.button_setDtlpDatabaseProperty.Text = "设置数据库 '"
                    + this.listView_dtlpDatabases.SelectedItems[0].Text
                    + "' 的类型(&T)...";
                this.button_setDtlpDatabaseProperty.Enabled = true;
            }
        }

        #endregion

        #region 定位gis.ini文件位置 -- tabPage_locateGisIniFile

        private void button_inputGisIniFlePath_Click(object sender, EventArgs e)
        {
            // 询问gis*.ini原始文件全路径
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要参考的gis.ini(或gis2000.ini)文件";
            dlg.FileName = this.textBox_gisIniFilePath.Text;
            dlg.Filter = "gis*.ini file (gis*.ini)|gis*.ini|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_gisIniFilePath.Text = dlg.FileName;

            /*
            // 装入文件内容
            int nRet = 0;
            string strError = "";
            nRet = LoadGisIniFileContent(this.textBox_gisIniFileContent.Text,
    out strError);
            if (nRet == -1)
                goto ERROR1;
             * */

            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        int LoadGisIniFileContent(string strGisIniFilePath,
            out string strError)
        {
            strError = "";

            try
            {

                StreamReader sr = new StreamReader(strGisIniFilePath,
                    Encoding.GetEncoding(936));

                this.textBox_gisIniFileContent.Text = sr.ReadToEnd();

                sr.Close();
            }
            catch (Exception ex)
            {
                strError = "打开或读入文件 " + strGisIniFilePath + " 时发生错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 自动查找gis.ini文件所在位置
        private void button_autoSearchGisIniFilePath_Click(object sender, EventArgs e)
        {
            string strError = "";
            this.EnableControls(false);
            try
            {

                // 1) 在windows目录下找到dt1000.ini，获得其
                // [Setup]
                // target=C:\DT1500\TechServ
                // 小节，从而可以推测出gis(2000).exe安装的目录，这个目录也就是gis(2000).ini所在的目录
                string strGisIniFilePath = "";
                string strComment = ""; // 文字描述查找的过程
                // return:
                //      0   没有找到
                //      1   找到
                int nRet = SearchGisIniFilePathByDt1000IniFile(out strGisIniFilePath,
                    out strComment);
                if (nRet == 1)
                {
                    this.textBox_gisIniFilePath.Text = strGisIniFilePath;

                    // 文件内容会自动装入
                    /*
                    nRet = LoadGisIniFileContent(this.textBox_gisIniFilePath.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                     * */
                }
                else
                {
                    strError = "无法找到gis*.ini文件。详细情况如下:\r\n\r\n" + strComment;
                    goto ERROR1;
                }


                // 2) 直接在每个驱动器上查找，如果发现多个，用其所在目录是否具备gis*.exe来帮助判断。如果实在还有多个，则列出供选择

            }
            finally
            {
                this.EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据windows目录下的dt1000.ini获得gis*.ini所在目录
        // return:
        //      0   没有找到
        //      1   找到
        int SearchGisIniFilePathByDt1000IniFile(out string strGisIniFilePath,
            out string strComment)
        {
            int nRet = 0;
            strGisIniFilePath = "";
            strComment = "";

            string strSystem32Directory = Environment.SystemDirectory;

            strComment += "系统给出的操作系统路径为 '" + strSystem32Directory + "'\r\n";

            if (String.IsNullOrEmpty(strSystem32Directory) == true)
            {
                strComment += "查找失败 -- 系统给出的操作系统路径为空\r\n";
                return 0;
            }

            string strWinntDirectory = PathUtil.PathPart(strSystem32Directory);
            if (String.IsNullOrEmpty(strWinntDirectory) == true)
            {
                strComment += "查找失败 -- 从系统给出的操作系统路径 '" + strSystem32Directory + "' 中居然没有析出目录部分\r\n";
                return 0;
            }

            string strDt1000IniFilePath = PathUtil.MergePath(strWinntDirectory, "dt1000.ini");

            strComment += "获得dt1000.ini文件的全路径 '" + strDt1000IniFilePath + "'\r\n";

            StringBuilder s = new StringBuilder(255, 255);
            nRet = API.GetPrivateProfileString("Setup",
                "target",
                "!!!null",
                s,
                255,
                strDt1000IniFilePath);
            string strLine = s.ToString();
            if (nRet <= 0
                || strLine == "!!!null")
            {
                strComment += "查找失败 -- 文件 '" + strDt1000IniFilePath + "'中[Setup]小节的target值不存在\r\n";
                return 0;
            }

            strComment += "获得文件 '" + strDt1000IniFilePath + "'中[Setup]小节的target值为'" + strLine + "'\r\n";


            string strDt1000BinDirectory = PathUtil.PathPart(strLine);
            if (String.IsNullOrEmpty(strDt1000BinDirectory) == true)
            {
                strComment += "查找失败 -- 从路径 '" + strLine + "'中析出目录部分失败\r\n";
                return 0;
            }

            strComment += "获得dt1000可执行文件安装目录 '" + strDt1000BinDirectory + "'\r\n";

            List<string> filepaths = null;
            string strTempComment = "";
            SearchFileName(strDt1000BinDirectory,
                "gis*.ini",
                out filepaths,
                out strTempComment);
            if (filepaths.Count == 0)
            {
                strComment += "查找失败 -- 在dt1000可执行文件安装目录 '" + strDt1000BinDirectory + "' 下没有找到匹配模式 gis*.ini 的文件。具体查找过程如下：\r\n";
                return 0;
            }

            if (filepaths.Count == 1)
            {
                strGisIniFilePath = filepaths[0];
                strComment += "在dt1000可执行文件安装目录 '" + strDt1000BinDirectory + "' 下找到匹配模式 gis*.ini 的唯一一个文件 '" + strGisIniFilePath + "'\r\n";
                return 1;
            }

            Debug.Assert(filepaths.Count > 1);
            strComment += "在dt1000可执行文件安装目录 '" + strDt1000BinDirectory + "' 下找到匹配模式 gis*.ini 的 " + filepaths.Count.ToString() + "个文件。后面将在这些文件的所在目录中进行进一步查找，那些同目录中包含 gis*.exe 文件的，将优先作为结果值...\r\n";

            // 如果找到多个，则同目录中有gis*.exe的优先返回
            for (int i = 0; i < filepaths.Count; i++)
            {
                string strFilePath = filepaths[i];
                string strPath = PathUtil.PathPart(strFilePath);
                if (String.IsNullOrEmpty(strPath) == true)
                {
                    strComment += "很奇怪，文件名 '" + strFilePath + "' 无法析出目录部分\r\n";
                    continue;
                }

                // 检测目录中是否存在gis*.exe?
                DirectoryInfo di = new DirectoryInfo(strPath);

                // 列出本级目录中符合要求的文件
                FileInfo[] fis = di.GetFiles("gis*.exe");
                if (fis.Length > 0)
                {
                    strGisIniFilePath = strFilePath;
                    strComment += "目录 '" + strPath + "' 中找到了至少一个匹配 gis*.exe 的文件名。因此将和这个目录关联的 '" + strGisIniFilePath + "' 作为结果返回。\r\n";
                    return 1;
                }

                strComment += "目录 '" + strPath + "' 中没有找到匹配 gis*.exe 的文件名。继续查找...\r\n";

            }

            // 实在不行，就返回第一个？TODO: 或者让人工选择?
            strGisIniFilePath = filepaths[0];
            // strComment += "所有候选目录中均未找到匹配 gis*.exe 的文件名。\r\n";
            strComment += "所有候选目录中均未找到匹配 gis*.exe 的文件名。那么只好返回候选文件名中的第一个 '" + strGisIniFilePath + "'。\r\n";

            return 1;   // 2008/10/10 changed
        }

        // 从一个目录和其下级目录中搜索文件名符合特定模式的文件
        static void SearchFileName(string strStartPath,
            string strFileNamePattern,
            out List<string> filepaths,
            out string strComment)
        {
            strComment = "";
            filepaths = new List<string>();

            DirectoryInfo di = new DirectoryInfo(strStartPath);

            // 列出本级目录中符合要求的文件
            FileInfo[] fis = di.GetFiles(strFileNamePattern);
            if (fis.Length == 0)
            {
                strComment += "在目录 " + di.FullName + " 中没有找到匹配模式 " + strFileNamePattern + " 的文件\r\n";
            }
            else
            {
                strComment += "在目录 " + di.FullName + " 中找到匹配模式 " + strFileNamePattern + " 的文件 " + fis.Length.ToString() + " 个，如下：\r\n";
            }
            for (int i = 0; i < fis.Length; i++)
            {
                filepaths.Add(fis[i].FullName);
                strComment += fis[i].FullName + "\r\n";
            }


            // 递归下级目录
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i].Name == "." || dis[i].Name == "..")
                    continue;

                List<string> temp = null;
                string strTempComment = "";
                SearchFileName(dis[i].FullName,
                    strFileNamePattern,
                    out temp,
                    out strTempComment);
                if (temp.Count > 0)
                    filepaths.AddRange(temp);

                if (String.IsNullOrEmpty(strTempComment) == false)
                    strComment += strTempComment;
            }
        }

        private void textBox_gisIniFilePath_TextChanged(object sender, EventArgs e)
        {
            // 试探装入文件内容
            string strError = "";
            int nRet = LoadGisIniFileContent(this.textBox_gisIniFilePath.Text,
                out strError);
            if (nRet != -1 && this.textBox_gisIniFileContent.Text != "")
            {
                this.label_gisIniFileContent.Text = this.textBox_gisIniFilePath.Text + " 文件内容:";
            }
            else
            {
                this.label_gisIniFileContent.Text = "gis.ini(或gis2000.ini)文件内容:";
            }
            /*
            if (nRet == -1)
                this.textBox_gisIniFileContent.Text = strError;
             * */
        }

        #endregion

        #region 在dp2中创建数据库

        // 警告那些设置了类型但是并未勾选的事项
        // return:
        //      0   没有警告
        //      1   警告了，用户选择继续
        //      2   警告了，用户选择返回
        int WarningTypedButUncheckedDatabaseItem()
        {
            string strText = "";
            for (int i = 0; i < this.listView_dtlpDatabases.Items.Count; i++)
            {
                ListViewItem item = this.listView_dtlpDatabases.Items[i];

                string strType = ListViewUtil.GetItemText(item, 1);

                string strSub = "";
                // 是否为升级不支持的类型?
                // parameters:
                //      strSub  返回导致不支持升级的实际类型
                // return:
                //      false   支持升级
                //      true    不支持升级。strSub中返回了不支持升级的实际类型
                bool bRet = IsUnUpgradeType(strType,
                    out strSub);
                if (bRet == true)
                    continue;   // 忽略那些本来就不支持其升级的数据库类型

                if (String.IsNullOrEmpty(strType) == false
                    && item.Checked == false)
                    strText += item.Text + "\r\n";
            }

            if (String.IsNullOrEmpty(strText) == false)
            {
                strText = "下列数据库设置了类型，但是未被勾选，如果继续，这些数据库不会被升级到dp2中:\r\n---\r\n" + strText + "\r\n\r\n确实要继续? (Yes: 继续; No: 返回，可重新勾选设置)";
                DialogResult result = MessageBox.Show(this,
        strText,
        "UpgradeDt1000ToDp2",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                    return 1;

                if (result == DialogResult.No)
                    return 2;
            }

            return 0;
        }



        // 警告那些空的和不支持的数据库类型
        // return:
        //      0   没有警告
        //      1   警告了，用户选择继续
        //      2   警告了，用户选择返回
        int WarningDatabaseType()
        {
            string strText = "";

            List<ListViewItem> items = new List<ListViewItem>();

            for (int i = 0; i < this.listView_dtlpDatabases.Items.Count; i++)
            {
                ListViewItem item = this.listView_dtlpDatabases.Items[i];

                string strType = ListViewUtil.GetItemText(item, 1);

                if (item.Checked == false)
                    continue;

                if (String.IsNullOrEmpty(strType) == true)
                {
                    strText += "数据库 " + item.Text + " 的类型未知\r\n";
                    goto CONTINUE;
                }

                string strSub = "";
                // 是否为升级不支持的类型?
                // parameters:
                //      strSub  返回导致不支持升级的实际类型
                // return:
                //      false   支持升级
                //      true    不支持升级。strSub中返回了不支持升级的实际类型
                bool bRet = IsUnUpgradeType(strType,
                    out strSub);
                if (bRet == true)
                {
                    strText += "数据库 " + item.Text + " 为类型 '" + strSub + "'，不支持其升级\r\n";
                    goto CONTINUE;
                }
                continue;

            CONTINUE:
                items.Add(item);

            }

            if (items.Count > 0)
            {
                strText = "下列数据库虽然被勾选了，但是其类型不被支持升级。如果继续，这些数据库不会被升级到dp2中:\r\n---\r\n" + strText + "\r\n\r\n确实要继续? (Yes: 继续; No: 返回，可重新勾选设置)";
                DialogResult result = MessageBox.Show(this,
        strText,
        "UpgradeDt1000ToDp2",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].Checked = false;
                    }
                    return 1;
                }

                if (result == DialogResult.No)
                    return 2;
            }

            return 0;
        }

        int WarningAtLeastOneDatabaseInCirculation()
        {
            return 0;
        }

        #endregion

        private void MainForm_Activated(object sender, EventArgs e)
        {
            this.stopManager.Active(this.stop);
        }

        #region 流通读者权限

        private void button_rights_findLtqxCfgFilename_Click(object sender, EventArgs e)
        {
            // 询问ltqx*.cfg原始文件全路径
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要参考的ltqx*.cfg文件";
            dlg.FileName = this.textBox_rights_ltxqCfgFilePath.Text;
            dlg.Filter = "ltqx*.cfg file (ltqx*.cfg)|ltqx*.cfg|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_rights_ltxqCfgFilePath.Text = dlg.FileName;
            return;
        }


        private void textBox_rights_ltxqCfgFilename_TextChanged(object sender, EventArgs e)
        {
            // 试探装入文件内容
            string strError = "";
            int nRet = LoadLtqxCfgFileContent(this.textBox_rights_ltxqCfgFilePath.Text,
                out strError);
            if (nRet != -1 && this.textBox_rights_ltqxCfgContent.Text != "")
            {
                this.label_rights_ltqxCfgFileContent.Text = this.textBox_gisIniFilePath.Text + " 文件内容:";
            }
            else
            {
                this.label_rights_ltqxCfgFileContent.Text = "ltqx*.cfg文件内容:";
            }
        }

        // 从dt1000服务器获得ltqx.cfg内容
        private void button_rights_getCfgFromDtlpServer_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.DtlpChannel == null)
            {
                strError = "DtlpChannel尚未初始化...";
                goto ERROR1;
            }

            Debug.Assert(DtlpChannel != null, "channel尚未初始化");

            string strPath = this.textBox_dtlpAsAddress.Text + "/cfgs/ltqx.cfg";
            string strContent = "";

            Cursor.Current = Cursors.WaitCursor;
            int nRet = this.DtlpChannel.GetCfgFile(strPath,
                out strContent,
                out strError);
            Cursor.Current = Cursors.Default;

            if (nRet == -1)
                goto ERROR1;

            this.textBox_rights_ltqxCfgContent.Text = strContent;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #endregion

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.button_next.Text = "继续(&C)";
            VisiblePages(true);
        }

        private void toolButton_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void listView_dtlpDatabases_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bSelected = this.listView_dtlpDatabases.SelectedItems.Count > 0;

            string strDatabaseName = "";
            if (bSelected == true)
                strDatabaseName = this.listView_dtlpDatabases.SelectedItems[0].Text;

            //
            menuItem = new MenuItem("设置数据库 '" + strDatabaseName + "' 的类型(&M)");
            menuItem.Click += new System.EventHandler(this.menu_setDatabaseType);
            if (bSelected == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            /*
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
             * */

            contextMenu.Show(this.listView_dtlpDatabases, new Point(e.X, e.Y));
        }

        // 设置数据库类型
        void menu_setDatabaseType(object sender, EventArgs e)
        {
            button_setDtlpDatabaseProperty_Click(sender, e);
        }

        // 从dt1000服务器某数据库的配置文件
        // return:
        //      -1  出错
        //      0   文件不存在
        //      1   成功
        int GetDtlpDbCfgFile(string strDbName,
            string strPureCfgFilename,
            out string strContent,
            out string strError)
        {
            strError = "";
            strContent = "";

            if (this.DtlpChannel == null)
            {
                strError = "DtlpChannel尚未初始化...";
                goto ERROR1;
            }

            Debug.Assert(DtlpChannel != null, "channel尚未初始化");

            string strPath = this.textBox_dtlpAsAddress.Text + "/" + strDbName + "/cfgs/" + strPureCfgFilename;

            Cursor.Current = Cursors.WaitCursor;
            // 获得(服务器端)配置文件内容
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   成功
            int nRet = this.DtlpChannel.GetCfgFile(strPath,
                out strContent,
                out strError);
            Cursor.Current = Cursors.Default;

            if (nRet == -1)
                goto ERROR1;

            return nRet;
        ERROR1:
            return -1;
        }

    }

    public class DatabaseProperty
    {
        public string DbName = "";
        public int BarcodeLength = 0;
        public string CanUse = "";
    }
}