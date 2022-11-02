using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// EntityRegisterControl 和 EntityRegisterWizard 的基类
    /// 提供基本的跨异构服务器检索功能
    /// </summary>
    public class EntityRegisterBase
    {
        // TODO: 要改为使用 MainForm 的公共 ChannelPool
        internal LibraryChannelPool _channelPool = new LibraryChannelPool();

        // public Stop Progress = null;

        public string Lang = "zh";

        AccountInfo _currentAccount = null;
        public AccountInfo CurrentAccount
        {
            get
            {
                return _currentAccount;
            }
            set
            {
                _currentAccount = value;
            }
        }

        XmlDocument _servers_dom = null;
        public XmlDocument ServersDom
        {
            get
            {
                return this._servers_dom;
            }
            set
            {
                this._servers_dom = value;
            }
        }

        public bool ServersDomChanged { get; set; }

        public EntityRegisterBase()
        {
            this._channelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }

        public static bool IsDot(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true
|| strText == ".")
                return true;
            return false;
        }

        void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            bool bIsReader = false;

            if (e.FirstTry == true)
            {
                Debug.Assert(_currentAccount != null, "");

                if (IsDot(_currentAccount.ServerUrl) == true)
                    e.LibraryServerUrl = Program.MainForm.LibraryServerUrl;
                else
                    e.LibraryServerUrl = _currentAccount.ServerUrl;

                if (IsDot(_currentAccount.UserName) == true)
                {
                    e.UserName = Program.MainForm.AppInfo.GetString(
                    "default_account",
                    "username",
                    "");

                    e.Password = Program.MainForm.AppInfo.GetString(
"default_account",
"password",
"");
                    e.Password = Program.MainForm.DecryptPasssword(e.Password);

                    bIsReader =
Program.MainForm.AppInfo.GetBoolean(
"default_account",
"isreader",
false);
                }
                else
                {
                    e.UserName = _currentAccount.UserName;

                    e.Password = _currentAccount.Password;

                    bIsReader = string.IsNullOrEmpty(_currentAccount.IsReader) == true ? false : DomUtil.IsBooleanTrue(_currentAccount.IsReader);
                }

#if NO
                if (IsDot(_currentAccount.IsReader) == true)
                    bIsReader =
        Program.MainForm.AppInfo.GetBoolean(
        "default_account",
        "isreader",
        false);
                else
                    bIsReader = DomUtil.IsBooleanTrue(_currentAccount.IsReader);
#endif
                Debug.Assert(Program.MainForm != null, "");

                string strLocation = Program.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");
                e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = Program.MainForm.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

                e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // TODO: 可以出现对话框，但要注意跨线程的问题

            DialogResult result = (DialogResult)Program.MainForm.Invoke((Func<DialogResult>)(() =>
            {
                using (LoginDlg dlg = new LoginDlg())
                {
                    Hashtable table = StringUtil.ParseParameters(e.Parameters, ',', '=');
                    string type = (string)table["type"];
                    bIsReader = type == "reader";
                    // TODO: Url 不让修改
                    dlg.UserName = bIsReader ? "~" + e.UserName : e.UserName;
                    dlg.Comment = e.ErrorInfo;
                    dlg.SavePassword = e.SavePasswordLong;
                    dlg.ServerUrl = e.LibraryServerUrl;
                    dlg.ShowDialog(Program.MainForm);
                    if (dlg.DialogResult == DialogResult.OK)
                    {
                        if (dlg.UserName.StartsWith("~"))
                        {
                            e.UserName = dlg.UserName.Substring(1);
                            bIsReader = true;
                        }
                        else
                        {
                            e.UserName = dlg.UserName;
                            bIsReader = false;
                        }
                        e.Password = dlg.Password;
                        e.SavePasswordLong = dlg.SavePassword;

                        // 同步到当前账户
                        if (IsDot(_currentAccount.UserName) == false)
                        {
                            _currentAccount.UserName = e.UserName;
                            _currentAccount.Password = e.Password;
                            _currentAccount.IsReader = bIsReader ? "yes" : "no";
                        }

                        // 长期保存密码，写回到 servers.xml 中
                        if (e.SavePasswordLong)
                        {
                            // 保存对账户的修改
                            var changed = SaveAccountInfo(this._servers_dom, _currentAccount);
                            if (changed == true)
                                this.ServersDomChanged = true;
                        }

                        table["type"] = bIsReader ? "reader" : "worker";
                        e.Parameters = StringUtil.BuildParameterString(table, ',', '=');
                    }
                    return dlg.DialogResult;
                }
            }));

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
        }


        public LibraryChannel MyGetChannel(string strServerUrl,
            string strUserName,
            GetChannelStyle style = GetChannelStyle.GUI)
        {
            if (EntityRegisterBase.IsDot(strServerUrl) == true)
                strServerUrl = Program.MainForm.LibraryServerUrl;
            if (EntityRegisterBase.IsDot(strUserName) == true)
                strUserName = Program.MainForm.DefaultUserName;

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);
            if ((style & GetChannelStyle.GUI) != 0)
                channel.Idle += channel_Idle;
            return channel;
        }

        void channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }

        public void MyReturnChannel(LibraryChannel channel)
        {
            channel.Idle -= channel_Idle;

            this._channelPool.ReturnChannel(channel);
        }

        string GetServerName(string strServerName)
        {
            if (IsDot(strServerName) == true)
            {
                if (this._servers_dom == null || this._servers_dom.DocumentElement == null)
                    return "";
                XmlElement server = (XmlElement)this._servers_dom.DocumentElement.SelectSingleNode("server[@url='.'] | server[@url='']");
                if (server == null)
                    return "";
                return server.GetAttribute("name");
            }
            return strServerName;
        }

        // parameters:
        //      strServerName   服务器名。可以为 .
        public AccountInfo GetAccountInfo(string strServerName, bool bSetCurrentAccount = true)
        {
            if (IsDot(strServerName) == true)
            {
                strServerName = GetServerName(strServerName);
                if (string.IsNullOrEmpty(strServerName) == true)
                    return null;
            }

            if (this._servers_dom == null || this._servers_dom.DocumentElement == null)
                return null;

            XmlNode server = this._servers_dom.DocumentElement.SelectSingleNode("server[@name='" + strServerName + "']");
            if (server == null)
                return null;

            if (bSetCurrentAccount == false)
                return GetAccountInfo(server as XmlElement);
            else
            {
                _currentAccount = GetAccountInfo(server as XmlElement);
                return _currentAccount;
            }
        }

        // 保存对账户的修改
        public static bool SaveAccountInfo(XmlDocument dom, AccountInfo account)
        {
            bool changed = false;
            XmlElement server = dom.DocumentElement.SelectSingleNode($"server[@name='{account.ServerName}']") as XmlElement;
            if (server == null)
                throw new Exception($"服务器 '{account.ServerName}' 在 servers.xml 文件中没有找到");

            if (account.UserName != server.GetAttribute("userName"))
            {
                server.SetAttribute("userName", account.UserName);
                changed = true;
            }

            var password = Program.MainForm.EncryptPassword(account.Password);
            if (password != server.GetAttribute("password"))
            {
                server.SetAttribute("password", password);
                changed = true;
            }

            if (account.IsReader != server.GetAttribute("isReader"))
            {
                server.SetAttribute("isReader", account.IsReader);
                changed = true;
            }
            return changed;
        }

        public static AccountInfo GetAccountInfo(XmlElement server)
        {
            AccountInfo account = new AccountInfo();
            account.ServerName = server.GetAttribute("name");
            account.ServerType = server.GetAttribute("type");
            account.ServerUrl = server.GetAttribute("url");
            account.UserName = server.GetAttribute("userName");
            var password = server.GetAttribute("password");
            account.Password = Program.MainForm.DecryptPasssword(password);

            account.IsReader = server.GetAttribute("isReader");
            account.ServerNode = server;
            return account;
        }

        Hashtable _serverInfoTable = new Hashtable();

        // 准备服务器信息
        public int GetServerInfo(
            // RegisterLine line,
            Stop stop,  // 2022/11/2
            LibraryChannel channel,
            AccountInfo account,
            out ServerInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            info = this._serverInfoTable[account.ServerUrl] as ServerInfo;
            if (info == null)
            {
                //if (line != null)
                //    line.BiblioSummary = "正在获取服务器 " + account.ServerName + " 的配置信息 ...";

                info = new ServerInfo();
                info.AccountInfo = account;
                int nRet = info.GetAllDatabaseInfo(channel,
                    stop,
                    out strError);
                if (nRet == -1)
                    return -1;
                nRet = info.InitialBiblioDbProperties(out strError);
                if (nRet == -1)
                    return -1;
                this._serverInfoTable[account.ServerUrl] = info;
            }
            return 0;
        }

        public string GetServerType(string strServerName)
        {
            AccountInfo account = this.GetAccountInfo(strServerName);
            if (account == null)
                return "";

            return account.ServerType;
        }
    }

    /// <summary>
    /// dp2Library 服务器信息
    /// </summary>
    public class ServerInfo
    {
        public AccountInfo AccountInfo = null;

        /// <summary>
        /// 表示当前全部数据库信息的 XmlDocument 对象
        /// </summary>
        public XmlDocument AllDatabaseDom = null;

        /// <summary>
        /// 书目库属性集合
        /// </summary>
        public List<BiblioDbProperty> BiblioDbProperties = null;

        public int GetAllDatabaseInfo(LibraryChannel channel,
            Stop stop,
            out string strError)
        {
            strError = "";
            string strValue = "";
            long lRet = 0;

            this.AllDatabaseDom = null;

            lRet = channel.ManageDatabase(
stop,
"getinfo",
"",
"",
"",
out strValue,
out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ErrorCode.AccessDenied)
                {
                }

                strError = "针对服务器 " + this.AccountInfo.ServerName + " 获得全部数据库定义过程发生错误：" + strError;
                return -1;
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
            return 0;
        }

        public int InitialBiblioDbProperties(
            out string strError)
        {
            strError = "";
            int nRet = 0;

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

            return 0;
        }

        // 获得全部书目库名列表
        // parameters:
        //      strServerName   服务器名。可以为 .
        public string GetBiblioDbNames()
        {
            List<string> results = new List<string>();

            // 2019/3/14
            if (this.AccountInfo != null && this.AccountInfo.ServerNode != null)
            {
                XmlNodeList databases = this.AccountInfo.ServerNode.SelectNodes("database");
                foreach (XmlElement database in databases)
                {
                    string dbName = database.GetAttribute("name");
                    results.Add(dbName);
                }
            }
            else if (this.BiblioDbProperties != null)
            {
                foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                {
                    // 2019/3/13 改为 不管有没有实体库，都参与检索
                    // 但不具备下属实体库的书目记录，在装入详细界面的时候，要当作外来数据处理
                    if (string.IsNullOrEmpty(prop.DbName) == false
                        // && string.IsNullOrEmpty(prop.ItemDbName) == false
                        )
                    {
                        results.Add(prop.DbName);
                    }
                }
            }

            return StringUtil.MakePathList(results);
        }
    }

    public class AccountInfo
    {
        public string ServerName = "";
        public string ServerType = "";
        public string ServerUrl = "";
        public string UserName = "";
        public string Password = "";
        public string IsReader = "";    // yes / no / 空 / . 。空和点表示依从当前帐户配置

        // 2019/3/14
        public XmlElement ServerNode { get; set; }

        public bool IsLocalServer
        {
            get
            {
                if (string.IsNullOrEmpty(this.ServerUrl) == true)
                    return true;
                if (this.ServerUrl == ".")
                    return true;
                return false;
            }
        }
    }
}

