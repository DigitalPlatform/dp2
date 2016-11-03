using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2ZServer.Install
{
    public partial class InstallZServerDlg : Form
    {
        public InstallZServerDlg()
        {
            InitializeComponent();
        }

        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";

                if (this.LibraryWsUrl == "")
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (this.UserName == "")
                {
                    MessageBox.Show(this, "尚未指定 dp2Library 管理用户名。");
                    return;
                }

                /*
                if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
                {
                    strError = "dp2Library 管理用户 密码 和 再次输入密码 不一致。请重新输入。";
                    MessageBox.Show(this, strError);
                    return;
                }*/

                // 检测帐户登录是否成功?


                // 进行登录
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                int nRet = DoLogin(
                    this.comboBox_librarywsUrl.Text,
                    this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "检测 dp2library 帐户时发生错误: " + strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "您指定的 dp2library 帐户 不正确: " + strError);
                    return;
                }


                MessageBox.Show(this, "您指定的 dp2library 帐户 正确");
            }
            finally
            {
                EnableControls(true);
            }
        }

        int VerifyXml(out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.textBox_databaseDef.Text) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(this.textBox_databaseDef.Text);
                }
                catch (Exception ex)
                {
                    this.tabControl_main.SelectedTab = this.tabPage_database;
                    strError = "数据库定义 XML 存在格式错误: " + ex.Message;
                    return -1;
                }
            }

            return 0;
        }

        // 按住 Control 键可以越过检测 dp2library server 的部分
        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bControl = Control.ModifierKeys == Keys.Control;

            EnableControls(false);
            try
            {
                if (string.IsNullOrEmpty(this.LibraryWsUrl))
                {
                    strError = "尚未输入 dp2Library 服务器的 URL";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.UserName))
                {
                    strError = "尚未指定 dp2Library 管理用户名。";
                    goto ERROR1;
                }

                if (this.textBox_anonymousUserName.Text == ""
                    && this.textBox_anonymousPassword.Text != "")
                {
                    strError = "在未指定匿名登录用户名的情况下，不允许指定匿名登录密码。";
                    goto ERROR1;
                }

                if (VerifyXml(out strError) == -1)
                    goto ERROR1;

                /*
                if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
                {
                    strError = "dp2Library 管理用户 密码 和 再次输入密码 不一致。请重新输入。";
                    MessageBox.Show(this, strError);
                    return;
                }*/

                // 检测帐户登录是否成功?
                if (bControl == false)
                {
                    // 进行登录
                    // return:
                    //      -1  error
                    //      0   登录未成功
                    //      1   登录成功
                    int nRet = DoLogin(
                        this.comboBox_librarywsUrl.Text,
                        this.textBox_manageUserName.Text,
                        this.textBox_managePassword.Text,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "检测 dp2library 帐户时发生错误: " + strError;
                        goto ERROR1;
                    }
                    if (nRet == 0)
                    {
                        strError = "您指定的 dp2library 帐户 不正确: " + strError;
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                EnableControls(true);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 进行登录
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        static int DoLogin(
            string strLibraryWsUrl,
            string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            using (LibraryChannel Channel = new LibraryChannel())
            {

                Channel.Url = strLibraryWsUrl;

                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                long lRet = Channel.Login(strUserName,
                    strPassword,
                    "location=z39.50 server,type=worker,client=dp2ZServer|0.01",
                    /*
                    "z39.50 server",    // string strLocation,
                    false,  // bReader,
                     * */
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
        }

        void EnableControls(bool bEnable)
        {
            // this.textBox_confirmManagePassword.Enabled = bEnable;
            this.textBox_managePassword.Enabled = bEnable;
            this.textBox_manageUserName.Enabled = bEnable;
            this.comboBox_librarywsUrl.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;

            this.textBox_anonymousUserName.Enabled = bEnable;
            this.textBox_anonymousPassword.Enabled = bEnable;
            this.button_detectAnonymousUser.Enabled = bEnable;

            this.textBox_databaseDef.Enabled = bEnable;
            this.button_import_databaseDef.Enabled = bEnable;

            this.numericUpDown_z3950_port.Enabled = bEnable;
            this.textBox_z3950_maxResultCount.Enabled = bEnable;
            this.textBox_z3950_maxSessions.Enabled = bEnable;

            this.Update();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string LibraryWsUrl
        {
            get
            {
                return this.comboBox_librarywsUrl.Text;
            }
            set
            {
                this.comboBox_librarywsUrl.Text = value;
            }
        }

        public string UserName
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

        public string Password
        {
            get
            {
                return this.textBox_managePassword.Text;
            }
            set
            {
                this.textBox_managePassword.Text = value;
                // this.textBox_confirmManagePassword.Text = value;
            }
        }

        public string AnonymousUserName
        {
            get
            {
                return this.textBox_anonymousUserName.Text;
            }
            set
            {
                this.textBox_anonymousUserName.Text = value;
            }
        }

        public string AnonymousPassword
        {
            get
            {
                return this.textBox_anonymousPassword.Text;
            }
            set
            {
                this.textBox_anonymousPassword.Text = value;
            }
        }

        private void button_detectAnonymousUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";

                if (this.LibraryWsUrl == "")
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (this.AnonymousUserName == "")
                {
                    MessageBox.Show(this, "尚未指定 匿名登录用户名。");
                    return;
                }


                // 检测帐户登录是否成功?


                // 进行登录
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                int nRet = DoLogin(
                    this.comboBox_librarywsUrl.Text,
                    this.textBox_anonymousUserName.Text,
                    this.textBox_anonymousPassword.Text,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "检测 匿名登录 用户时发生错误: " + strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "您指定的 匿名登录 用户 不正确: " + strError);
                    return;
                }

                MessageBox.Show(this, "您指定的 匿名登录 用户 正确");
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_import_databaseDef_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";

            this.EnableControls(false);
            try
            {
                if (string.IsNullOrEmpty(this.comboBox_librarywsUrl.Text))
                {
                    strError = "尚未输入 dp2Library 服务器的 URL";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.textBox_manageUserName.Text))
                {
                    strError = "尚未指定 dp2Library 管理用户名";
                    goto ERROR1;
                }

                // this.textBox_databaseDef.Text = "";

                int nRet = GetDatabaseDef(
        this.comboBox_librarywsUrl.Text,
        this.textBox_manageUserName.Text,
        this.textBox_managePassword.Text,
        out strXml,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strOutputXml = "";
                nRet = BuildZDatabaseDef(strXml,
                    out strOutputXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    this.DatabasesXml = "";
                    MessageBox.Show(this, "dp2library 中尚未定义 OPAC 检索数据库，或没有为任何数据库定义别名。请先利用内务系统管理窗“OPAC”属性页进行配置，再使用本功能");
                }
                else
                    this.DatabasesXml = strOutputXml;
                return;
            }
            finally
            {
                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得 dp2library <virtualDatabases> 数据库定义
        // return:
        //      -1  error
        //      0   成功
        static int GetDatabaseDef(
            string strLibraryWsUrl,
            string strUserName,
            string strPassword,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            using (LibraryChannel Channel = new LibraryChannel())
            {

                Channel.Url = strLibraryWsUrl;

                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                long lRet = Channel.Login(strUserName,
                    strPassword,
                    "location=z39.50 server,type=worker,client=dp2ZServer|0.01",
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                {
                    strError = "登录未成功:" + strError;
                    return -1;
                }

                lRet = Channel.GetSystemParameter(
    null,
    "opac",
    "databases",
    out strXml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
        }

        /*
  <databases>
    <database name="中文图书" alias="cbook">
      <use value="4" from="题名" />
      <use value="7" from="ISBN" />
      <use value="8" from="ISSN" />
      <use value="21" from="主题词" />
      <use value="1003" from="责任者" />
    </database>
    <database name="英文图书" alias="ebook">
      <use value="4" from="题名" />
      <use value="7" from="ISBN" />
      <use value="8" from="ISSN" />
      <use value="21" from="主题词" />
      <use value="1003" from="责任者" />
    </database>
  </databases>
         * */
        // 根据 library.xml 中的 <virtualDatabases> 元素构造 dp2zserver.xml 中的 <databases>
        // return:
        //      -1  出错
        //      0   strXml 中没有发现有意义的信息
        //      1   构造成功
        int BuildZDatabaseDef(string strXml,
            out string strOutputXml,
            out string strError)
        {
            strError = "";
            strOutputXml = "";

            XmlDocument source_dom = new XmlDocument();
            source_dom.LoadXml("<root />");
            try
            {
                source_dom.DocumentElement.InnerXml = strXml;
            }
            catch (Exception ex)
            {
                strError = "输入的 XML 字符串格式错误: " + ex.Message;
                return -1;
            }

            XmlDocument target_dom = new XmlDocument();
            target_dom.LoadXml("<databases />");

            int createCount = 0;
            XmlNodeList databases = source_dom.DocumentElement.SelectNodes("database");
            foreach (XmlElement database in databases)
            {
                string name = database.GetAttribute("name");
                string alias = database.GetAttribute("alias");

                // 没有别名的数据库不会用在 Z39.50 检索中
                if (string.IsNullOrEmpty(alias))
                    continue;

                XmlElement new_database = target_dom.CreateElement("database");
                target_dom.DocumentElement.AppendChild(new_database);
                new_database.SetAttribute("name", name);
                new_database.SetAttribute("alias", alias);

                createCount++;

                // 翻译 use
                string from_name = FindFromByStyle(database, "title");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "4", from_name);

                from_name = FindFromByStyle(database, "isbn");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "7", from_name);

                from_name = FindFromByStyle(database, "issn");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "8", from_name);

                from_name = FindFromByStyle(database, "subject");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "21", from_name);

                from_name = FindFromByStyle(database, "contributor");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "1003", from_name);
            }

            if (createCount == 0)
                return 0;

            strOutputXml = target_dom.DocumentElement.OuterXml;
            return 1;
        }

        /*
      <use value="4" from="题名" />
         * */
        static void CreateUseElement(XmlElement database, string number, string from)
        {
            XmlElement element = database.OwnerDocument.CreateElement("use");
            database.AppendChild(element);
            element.SetAttribute("value", number);
            element.SetAttribute("from", from);
        }

        /*
        <database name="中文图书" alias="cbook">
            <caption lang="zh">中文图书</caption>
            <from name="ISBN" style="isbn">
                <caption lang="zh-CN">ISBN</caption>
                <caption lang="en">ISBN</caption>
            </from>
            <from name="ISSN" style="issn">
                <caption lang="zh-CN">ISSN</caption>
                <caption lang="en">ISSN</caption>
            </from>
            <from name="题名" style="title">
                <caption lang="zh-CN">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from name="题名拼音" style="pinyin_title">
                <caption lang="zh-CN">题名拼音</caption>
                <caption lang="en">Title pinyin</caption>
            </from>
            <from name="主题词" style="subject">
                <caption lang="zh-CN">主题词</caption>
                <caption lang="en">Thesaurus</caption>
            </from>
            <from name="中图法分类号" style="clc,__class">
                <caption lang="zh-CN">中图法分类号</caption>
                <caption lang="en">CLC Class number</caption>
            </from>
            <from name="责任者" style="contributor">
                <caption lang="zh-CN">责任者</caption>
                <caption lang="en">Contributor</caption>
            </from>
            <from name="责任者拼音" style="pinyin_contributor">
                <caption lang="zh-CN">责任者拼音</caption>
                <caption lang="en">Contributor pinyin</caption>
            </from>
            <from name="出版发行者" style="publisher">
                <caption lang="zh-CN">出版发行者</caption>
                <caption lang="en">Publisher</caption>
            </from>
            <from name="出版时间" style="publishtime,_time,_freetime">
                <caption lang="zh-CN">出版时间</caption>
                <caption lang="en">Publish Time</caption>
            </from>
            <from name="批次号" style="batchno">
                <caption lang="zh-CN">批次号</caption>
                <caption lang="en">Batch number</caption>
            </from>
            <from name="目标记录路径" style="targetrecpath">
                <caption lang="zh-CN">目标记录路径</caption>
                <caption lang="en">Target Record Path</caption>
            </from>
            <from name="状态" style="state">
                <caption lang="zh-CN">状态</caption>
                <caption lang="en">State</caption>
            </from>
            <from name="操作时间" style="opertime,_time,_utime">
                <caption lang="zh-CN">操作时间</caption>
                <caption lang="en">OperTime</caption>
            </from>
            <from name="其它标识号" style="identifier">
                <caption lang="zh-CN">其它标识号</caption>
                <caption lang="en">Identifier</caption>
            </from>
            <from name="__id" style="recid" />
        </database>
         * */
        static string FindFromByStyle(XmlElement database, string strStyle)
        {
            XmlNodeList froms = database.SelectNodes("from");
            foreach (XmlElement from in froms)
            {
                string name = from.GetAttribute("name");
                string style = from.GetAttribute("style");

                if (string.IsNullOrEmpty(style))
                    continue;

                if (StringUtil.IsInList(strStyle, style) == true)
                    return name;
            }

            return null;    // not found
        }

        // <databases> 元素 OuterXml
        public string DatabasesXml
        {
            get
            {
                return this.textBox_databaseDef.Text;
            }
            set
            {
                this.textBox_databaseDef.Text = DomUtil.GetIndentXml(value);
            }
        }

        public int Port
        {
            get
            {
                return Convert.ToInt32(this.numericUpDown_z3950_port.Value);
            }
            set
            {
                this.numericUpDown_z3950_port.Value = value;
            }
        }

        public string MaxSessions
        {
            get
            {
                return this.textBox_z3950_maxSessions.Text;
            }
            set
            {
                this.textBox_z3950_maxSessions.Text = value;
            }
        }

        public string MaxResultCount
        {
            get
            {
                return this.textBox_z3950_maxResultCount.Text;
            }
            set
            {
                this.textBox_z3950_maxResultCount.Text = value;
            }
        }
    }
}