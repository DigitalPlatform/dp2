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
using System.Net;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC
{
    public partial class InstallOpacParamDlg : Form
    {
        public string LibraryReportDir = "";

        public bool NeedAppendRights = false;

        // 超级用户。用于创建代理帐户
        public string SupervisorUserName = "supervisor";
        public string SupervisorPassword = "";

        public string ManageAccountRights = "";

        // bool SavePassword = true;

        // public string RootDir = "";

        public InstallOpacParamDlg()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            this.Update();

            try
            {
                string strError = "";

                if (this.Dp2LibraryUrl == "")
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (this.ManageUserName == "")
                {
                    MessageBox.Show(this, "尚未指定代理帐户用户名。");
                    return;
                }

                if (this.ManageUserName == "reader"
    || this.ManageUserName == "public"
    || this.ManageUserName == "图书馆")
                {
                    strError = "代理帐户的用户名不能为 'reader' 'public' '图书馆' 之一，因为这些都是 dp2Library 系统内具有特定用途的保留帐户名";
                    MessageBox.Show(this, strError);
                    return;
                }

                if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
                {
                    strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                    MessageBox.Show(this, strError);
                    return;
                }

                // 验证代理帐户用户是否已经可用？
                // return:
                //       -1  出错
                //      0   不存在
                //      1   存在, 且密码一致
                //      2   存在, 但密码不一致
                int nRet = DetectManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "验证代理帐户时发现问题: " + strError + "\r\n\r\n请确保代理帐户已经正确创建");
                    return;
                }

                if (nRet == 1)
                {
                    // 增补权限
                    this.NeedAppendRights = true;
                }
                else if (nRet == 2)
                {
                    string strText = "代理帐户已经存在, 但是其密码和当前面板中拟设置的密码不一致。\r\n\r\n是否要重设其密码?\r\n\r\n(是(Yes): 重设密码并继续安装；否(No): 不重设密码并继续安装；取消(Cancel): 返回设置面板)";
                    DialogResult result = MessageBox.Show(this,
                        strText,
                        "setup_dp2opac",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        nRet = ResetManageUserPassword(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, "重设代理帐户密码时出错: " + strError + "\r\n\r\n请确保代理帐户已经正确创建");
                            return;
                        }
                    }

                    if (result == DialogResult.Cancel)
                        return; // 返回面板

                    // 增补权限
                    this.NeedAppendRights = true;
                }

                // 代理帐户不存在
                else if (nRet == 0)
                {
                    // 自动创立?
                    string strText = "代理帐户 '" + this.textBox_manageUserName.Text + "' 尚未创建, 是否创建之?\r\n\r\n(确定(OK): 创建； 取消(Cancel): 不创建，返回设置面板)";
                    DialogResult result = MessageBox.Show(this,
                        strText,
                        "setup_dp2opac",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;

                    nRet = CreateManageUser(out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }
                    this.NeedAppendRights = false;
                }

                // 获得报表目录路径
                if (IsLocalHostUrl(this.textBox_dp2LibraryUrl.Text) == true)
                {
                    string strDataDir = "";
                    nRet = GetLibraryDataDir(out strDataDir, out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, "获得 dp2Library 数据目录配置时出现问题: " + strError + "\r\n\r\n请在安装成功后手动配置 opac.xml 文件");
                    }

                    if (string.IsNullOrEmpty(strDataDir) == false)
                        this.LibraryReportDir = Path.Combine(strDataDir, "upload\\reports");
                    else
                        this.LibraryReportDir = "";
                }
                else
                    this.LibraryReportDir = "";
            }
            finally
            {
                EnableControls(true);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public static bool IsLocalHostUrl(string strUrl)
        {
            Uri uri = new Uri(strUrl);
            if (uri.Scheme.ToLower() == "net.pipe")
                return true;
            return IsLocalIpAddress(uri.Host);
        }

        // 检查一个地址是否等同 localhost
        // http://stackoverflow.com/questions/11834091/how-to-check-if-localhost
        public static bool IsLocalIpAddress(string host)
        {
            try
            { // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP))
                        return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP))
                            return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string Dp2LibraryUrl
        {
            get
            {
                return this.textBox_dp2LibraryUrl.Text;
            }
            set
            {
                this.textBox_dp2LibraryUrl.Text = value;
            }
        }

        // 代理用户名
        public string ManageUserName
        {
            get
            {
                return this.textBox_manageUserName.Text;
            }
            set
            {
                this.textBox_manageUserName.Text = value;
            }
        }

        // 代理用户密码
        public string ManagePassword
        {
            get
            {
                return this.textBox_managePassword.Text;
            }
            set
            {
                this.textBox_managePassword.Text = value;
                this.textBox_confirmManagePassword.Text = value;
            }
        }

        // 检测管理用户是否已经存在?
        // return:
        //       -1  出错
        //      0   不存在
        //      1   存在, 且密码一致
        //      2   存在, 但密码不一致
        int DetectManageUser(out string strError)
        {
            strError = "";
            if (this.textBox_dp2LibraryUrl.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.textBox_dp2LibraryUrl.Text;

                // Debug.Assert(false, "");
                string strParameters = "location=#setup,type=worker,client=dp2OPAC|";
                long nRet = channel.Login(this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    strParameters,
                    out strError);
                if (nRet == -1)
                {
                    strError = "以用户名 '" + this.textBox_manageUserName.Text + "' 和密码登录失败: " + strError;
                    return -1;
                }

                if (nRet == 1)
                    this.ManageAccountRights = channel.Rights;

                channel.Logout(out strError);

                if (nRet == 0)
                {
                    channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                    channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                    strError = "为确认代理帐户是否存在, 请用超级用户身份登录。";
                    nRet = channel.DoNotLogin(ref strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        strError = "以超级用户身份登录失败: " + strError + "\r\n\r\n因此无法确定代理帐户是否存在";
                        return -1;
                    }

                    UserInfo[] users = null;
                    nRet = channel.GetUser(
                        null,
                        "list",
                        this.textBox_manageUserName.Text,
                        0,
                        -1,
                        out users,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取用户 '" + this.textBox_manageUserName.Text + "' 信息时发生错误: " + strError + "\r\n\r\n因此无法确定代理帐户是否存在。";
                        return -1;
                    }
                    if (nRet == 1)
                    {
                        Debug.Assert(users != null, "");
                        strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 已经存在, 但其密码和当前面板拟设置的密码不一致。";
                        return 2;
                    }
                    if (nRet >= 1)
                    {
                        Debug.Assert(users != null, "");
                        strError = "以 '" + this.textBox_manageUserName.Text + "' 为用户名 的用户记录存在多条，这是一个严重错误，请系统管理员启用dp2circulation尽快修正此错误。";
                        return -1;
                    }

                    return 0;
                }

                return 1;
            }
        }

        void channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = this.SupervisorUserName;
                e.Password = this.SupervisorPassword;

                e.Parameters = "location=#setup,type=worker,client=dp2OPAC|";

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = null;

            if (sender is IWin32Window)
                owner = (IWin32Window)sender;
            else
                owner = this;

            CirculationLoginDlg dlg = new CirculationLoginDlg();
            GuiUtil.AutoSetDefaultFont(dlg);
            // dlg.Text = "";
            dlg.ServerUrl = this.textBox_dp2LibraryUrl.Text;
            dlg.Comment = e.ErrorInfo;
            dlg.UserName = e.UserName;
            dlg.SavePasswordShort = false;
            dlg.SavePasswordLong = false;
            dlg.Password = e.Password;
            dlg.IsReader = false;
            dlg.OperLocation = "#setup";
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(owner);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = dlg.SavePasswordShort;
            e.Parameters = "location=#setup,type=worker,client=dp2OPAC|";

            e.SavePasswordLong = dlg.SavePasswordLong;
            e.LibraryServerUrl = dlg.ServerUrl;

            this.SupervisorUserName = e.UserName;
            this.SupervisorPassword = e.Password;
        }

#if NO
        // 获得用户记录
        // return:
        //      -1  error
        //      0   not found
        //      >=1   检索命中的条数
        public static int GetUserRecord(
            RmsChannel channel,
            string strUserName,
            out string strRecPath,
            out string strXml,
            out byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            strXml = "";
            strRecPath = "";
            baTimeStamp = null;

            if (strUserName == "")
            {
                strError = "用户名为空";
                return -1;
            }

            string strQueryXml = "<target list='" + Defs.DefaultUserDb.Name
                + ":" + Defs.DefaultUserDb.SearchPath.UserName + "'><item><word>"
                + strUserName + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>chi</lang></target>";

            long nRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOutputStyle
                out strError);
            if (nRet == -1)
            {
                strError = "检索帐户库时出错: " + strError;
                return -1;
            }

            if (nRet == 0)
                return 0;	// not found

            long nSearchCount = nRet;

            List<string> aPath = null;
            nRet = channel.DoGetSearchResult(
                "default",
                1,
                "zh",
                null,	// stop,
                out aPath,
                out strError);
            if (nRet == -1)
            {
                strError = "检索注册用户库获取检索结果时出错: " + strError;
                return -1;
            }
            if (aPath.Count == 0)
            {
                strError = "检索注册用户库获取的检索结果为空";
                return -1;
            }

            // strRecID = ResPath.GetRecordId((string)aPath[0]);
            strRecPath = (string)aPath[0];

            string strStyle = "content,data,timestamp,withresmetadata";
            string strMetaData;
            string strOutputPath;

            nRet = channel.GetRes((string)aPath[0],
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获取注册用户库记录体时出错: " + strError;
                return -1;
            }


            return (int)nSearchCount;
        }

#endif

        // 创建代理帐户
        int CreateManageUser(out string strError)
        {
            strError = "";
            if (this.textBox_dp2LibraryUrl.Text == "")
            {
                strError = "尚未指定dp2Library服务器URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "reader"
                || this.textBox_manageUserName.Text == "public"
                || this.textBox_manageUserName.Text == "图书馆")
            {
                strError = "代理帐户的用户名不能为 'reader' 'public' '图书馆' 之一，因为这些都是 dp2Library 系统内具有特定用途的保留帐户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.textBox_dp2LibraryUrl.Text;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                strError = "请用超级用户身份登录，以便创建代理帐户。";
                int nRet = channel.DoNotLogin(ref strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以超级用户身份登录失败: " + strError;
                    return -1;
                }
                UserInfo user = new UserInfo();
                user.UserName = this.textBox_manageUserName.Text;
                user.Password = this.textBox_managePassword.Text;
                user.SetPassword = true;
                user.Rights = "getsystemparameter,getres,search,getbiblioinfo,setbiblioinfo,getreaderinfo,writeobject,getbibliosummary,listdbfroms,simulatereader,simulateworker";

                /*
    代理帐户:
    getsystemparameter
    getres
    search
    getbiblioinfo
    getreaderinfo
    writeobject * */

                long lRet = channel.SetUser(
        null,
        "new",
        user,
        out strError);
                if (lRet == -1)
                {
                    strError = "创建代理帐户时发生错误: " + strError;
                    return -1;
                }

                channel.Logout(out strError);
                return 0;
            }
        }

        // 重设置代理帐户密码
        int ResetManageUserPassword(out string strError)
        {
            strError = "";
            if (this.textBox_dp2LibraryUrl.Text == "")
            {
                strError = "尚未指定dp2Library服务器URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.textBox_dp2LibraryUrl.Text;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                strError = "请用超级用户身份登录，以便重设代理帐户密码。";
                int nRet = channel.DoNotLogin(ref strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以超级用户身份登录失败: " + strError;
                    return -1;
                }

                if (StringUtil.IsInList("changeuserpassword", channel.Rights) == false)
                {
                    strError = "您所使用的超级用户 '" + this.SupervisorUserName + "' 不具备 changeuserpassword 权限，无法进行(为代理帐户 '" + this.textBox_manageUserName.Text + "' )重设密码的操作";
                    return -1;
                }

                UserInfo user = new UserInfo();
                user.UserName = this.textBox_manageUserName.Text;
                user.Password = this.textBox_managePassword.Text;

                long lRet = channel.SetUser(
                    null,
                    "resetpassword",
                    user,
                    out strError);
                if (lRet == -1)
                {
                    strError = "重设密码时发生错误: " + strError;
                    return -1;
                }

                channel.Logout(out strError);
                return 0;
            }
        }

        // 获得 dp2Library 数据目录
        int GetLibraryDataDir(out string strDataDir, out string strError)
        {
            strError = "";
            strDataDir = "";

            if (this.textBox_dp2LibraryUrl.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.textBox_dp2LibraryUrl.Text;

                // Debug.Assert(false, "");
                string strParameters = "location=#setup,type=worker,client=dp2OPAC|";
                long nRet = channel.Login(this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    strParameters,
                    out strError);
                if (nRet == -1)
                {
                    strError = "以用户名 '" + this.textBox_manageUserName.Text + "' 和密码登录失败: " + strError;
                    return -1;
                }

                try
                {
                    if (nRet == 0 || StringUtil.IsInList("getsystemparameter", channel.Rights) == false)
                    {
                        channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                        channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                        strError = "为获取 dp2Library 数据目录配置信息, 请用超级用户身份登录。";
                        nRet = channel.DoNotLogin(ref strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            strError = "以超级用户身份登录失败: " + strError + "\r\n\r\n因此无法获取 dp2Library 数据目录配置信息";
                            return -1;
                        }
                    }

                    nRet = channel.GetSystemParameter(
            null,
            "cfgs",
            "getDataDir",
            out strDataDir,
            out strError);
                    if (nRet == -1)
                        return -1;
                }
                finally
                {
                    channel.Logout(out strError);
                }

                return 0;
            }
        }

#if NO
        void channels_AskAccountInfo(object sender, AskAccountInfoEventArgs e)
        {
            e.Owner = this;

            LoginDlg dlg = new LoginDlg();

            dlg.textBox_serverAddr.Text = this.textBox_dp2LibraryUrl.Text;
            dlg.textBox_serverAddr.ReadOnly = true;
            dlg.textBox_comment.Text = e.Comment;
            dlg.textBox_userName.Text = this.ManagerUserName;
            dlg.textBox_password.Text = this.ManagerPassword;
            dlg.checkBox_savePassword.Checked = this.SavePassword;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
            {
                e.Result = 0;
                return;
            }

            this.ManagerPassword = dlg.textBox_userName.Text;

            if (dlg.checkBox_savePassword.Checked == true)
                this.ManagerPassword = dlg.textBox_password.Text;
            else
                this.ManagerPassword = "";

            e.UserName = dlg.textBox_userName.Text;
            e.Password = dlg.textBox_password.Text;

            e.Result = 1;
        }
#endif

#if NO
        int BuildUserRecord(out string strXml,
    out string strError)
        {
            strXml = "";
            strError = "";

            XmlDocument UserRecDom = new XmlDocument();
            UserRecDom.LoadXml("<record><name /><password /><server /></record>");


            // 设置用户名
            DomUtil.SetElementText(UserRecDom.DocumentElement,
                "name",
                this.textBox_manageUserName.Text);


            // 密码
            DomUtil.SetElementText(UserRecDom.DocumentElement,
               "password",
                Cryptography.GetSHA1(this.textBox_managePassword.Text));

            XmlNode nodeServer = UserRecDom.DocumentElement.SelectSingleNode("server");
            if (nodeServer == null)
            {
                Debug.Assert(false, "不可能的情况");
                return -1;
            }

            DomUtil.SetAttr(nodeServer, "rights", "children_database:create,list");

            strXml = UserRecDom.OuterXml;

            return 0;
        }

#endif

        void EnableControls(bool bEnable)
        {
            this.textBox_confirmManagePassword.Enabled = bEnable;
            this.textBox_managePassword.Enabled = bEnable;
            this.textBox_manageUserName.Enabled = bEnable;
            this.textBox_dp2LibraryUrl.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_createManageUser.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;
            this.button_resetManageUserPassword.Enabled = bEnable;

        }

        // 检测管理帐户是否存在，登录是否正确
        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                if (string.IsNullOrEmpty(this.textBox_manageUserName.Text) == true)
                {
                    MessageBox.Show(this, "请先在面板上指定要检测的代理帐户名");
                    return;
                }
                string strError = "";
                // return:
                //       -1  出错
                //      0   不存在
                //      1   存在, 且密码一致
                //      2   存在, 但密码不一致
                int nRet = DetectManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else if (nRet == 0)
                {
                    MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 目前尚不存在。");
                }
                else if (nRet == 2)
                {
                    MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 经检验存在，但其密码和当前面板上输入的密码不一致。");
                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 经检验存在。");
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 创建一个新的管理帐户。需要用 root 权限登录才能创建
        private void button_createManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";
                int nRet = CreateManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, "代理帐户创建成功。");
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_resetManageUserPassword_Click(object sender, EventArgs e)
        {
            // 重设置代理帐户密码
            EnableControls(false);
            try
            {
                string strError = "";
                int nRet = ResetManageUserPassword(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, "重设代理帐户密码成功。");
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void textBox_manageUserName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_manageUserName.Text) == true)
            {
                this.button_detectManageUser.Enabled = false;
                this.button_createManageUser.Enabled = false;
                this.button_resetManageUserPassword.Enabled = false;
            }
            else
            {
                this.button_detectManageUser.Enabled = true;
                this.button_createManageUser.Enabled = true;
                this.button_resetManageUserPassword.Enabled = true;
            }
        }

    }
}