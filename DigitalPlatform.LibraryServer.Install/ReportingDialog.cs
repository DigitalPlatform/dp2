using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using MySqlConnector;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    public partial class ReportingDialog : Form
    {
        bool _changed = false;

        public bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }

        public string LibraryXmlFileName { get; set; }

        XmlDocument _dom = new XmlDocument();

        public ReportingDialog()
        {
            InitializeComponent();
        }

        string m_strUrl = "";
        string m_strUserName = "";
        string m_strPassword = "";

        private void button_editMasterServer_Click(object sender, EventArgs e)
        {
            ConfirmSupervisorDialog dlg = new ConfirmSupervisorDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.Text = "dp2library 服务器";
            dlg.Comment = "请设置 dp2library 服务器的地址和用户名";
            dlg.ServerUrl = this.m_strUrl;
            dlg.UserName = this.m_strUserName;
            dlg.Password = this.m_strPassword;
            dlg.PhoneNumberVisible = false;
            dlg.ServerUrlReadOnly = false;

            dlg.StartPosition = FormStartPosition.CenterScreen;
        REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            if (string.IsNullOrEmpty(dlg.ServerUrl))
            {
                MessageBox.Show(this, "尚未输入服务器 URL");
                goto REDO_INPUT;
            }

            if (string.IsNullOrEmpty(dlg.UserName))
            {
                MessageBox.Show(this, "尚未输入用户名");
                goto REDO_INPUT;
            }

            if (dlg.ServerUrl != this.m_strUrl
                || dlg.UserName != this.m_strUserName
                || dlg.Password != this.m_strPassword)
            {
                this.m_strUrl = dlg.ServerUrl;
                this.m_strUserName = dlg.UserName;
                this.m_strPassword = dlg.Password;

                RefreshMasterServerSummary();
                this.Changed = true;
            }
        }

        void RefreshMasterServerSummary()
        {
            this.textBox_masterServer.Text = "dp2library URL = " + this.m_strUrl
                + "; UserName = " + this.m_strUserName
                + "; Password = " + new string('*', this.m_strPassword.Length);
        }

        private void ReportingDialog_Load(object sender, EventArgs e)
        {
            this.Invoke(new Action(LoadLibraryXml));
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_confirmLoginPassword.Text != this.textBox_loginPassword.Text)
            {
                strError = "“密码”和“再次输入密码”不一致。请重新输入";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_sqlServerName.Text) == false)
            {
                this.button_OK.Enabled = false;
                try
                {
                    int nRet = VerifySqlServer(
                        this.textBox_sqlServerName.Text,
                        this.textBox_instanceName.Text,
                        this.textBox_loginName.Text,
                        this.textBox_loginPassword.Text,
                        out strError);
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
        "在检查服务器参数的过程中发生错误: \r\n\r\n"
        + strError
        + "\r\n\r\n是否依然采用这些参数继续完成安装?",
        "MySqlDataSourceDlg",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                        if (result == System.Windows.Forms.DialogResult.No)
                        {
                            MessageBox.Show(this, "请修改服务器参数");
                            return;
                        }
                    }
                }
                finally
                {
                    this.button_OK.Enabled = true;
                }
            }

            // if (this.Changed)
            SaveLibraryXml();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void ReportingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ReportingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        void LoadLibraryXml()
        {
            if (string.IsNullOrEmpty(this.LibraryXmlFileName))
                return;

            string strError = "";
            _dom = new XmlDocument();
            try
            {
                _dom.Load(this.LibraryXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "装载配置文件 '" + this.LibraryXmlFileName + "' 进入 XMLDOM 时出错: " + ex.Message;
                goto ERROR1;
            }

            {
                XmlElement server = _dom.DocumentElement.SelectSingleNode("reportReplication") as XmlElement;
                if (server != null)
                {
                    this.m_strUrl = server.GetAttribute("serverUrl");
                    this.m_strUserName = server.GetAttribute("userName");
                    try
                    {
                        this.m_strPassword = DecryptPassword(server.GetAttribute("password"));
                    }
                    catch (System.Security.Cryptography.CryptographicException)
                    {
                        strError = "library.xml 中 reportReplication 元素的 password 属性值不合法";
                        goto ERROR1;
                    }
                }
                else
                {
                    this.m_strUrl = "";
                    this.m_strUserName = "";
                    this.m_strPassword = "";
                }
                RefreshMasterServerSummary();
            }

            {
                XmlElement reportStorage = _dom.DocumentElement.SelectSingleNode("reportStorage") as XmlElement;
                if (reportStorage != null)
                {
                    this.textBox_sqlServerName.Text = reportStorage.GetAttribute("serverName");
                    this.textBox_loginName.Text = reportStorage.GetAttribute("userId");
                    this.textBox_loginPassword.Text = DecryptReportPassword(reportStorage.GetAttribute("password"));
                    this.textBox_confirmLoginPassword.Text = this.textBox_loginPassword.Text;
                    this.textBox_instanceName.Text = reportStorage.GetAttribute("databaseName");
                }
                else
                {
                    this.textBox_sqlServerName.Text = "";
                    this.textBox_loginName.Text = "";
                    this.textBox_loginPassword.Text = "";
                    this.textBox_confirmLoginPassword.Text = "";
                    this.textBox_instanceName.Text = "";
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void SaveLibraryXml()
        {
            {
                XmlElement server = _dom.DocumentElement.SelectSingleNode("reportReplication") as XmlElement;
                if (server == null)
                {
                    server = _dom.CreateElement("reportReplication");
                    _dom.DocumentElement.AppendChild(server);
                }

                server.SetAttribute("serverUrl", this.m_strUrl);
                server.SetAttribute("userName", this.m_strUserName);
                server.SetAttribute("password", EncryptPassword(this.m_strPassword));
            }

            {
                XmlElement reportStorage = _dom.DocumentElement.SelectSingleNode("reportStorage") as XmlElement;
                if (reportStorage == null)
                {
                    reportStorage = _dom.CreateElement("reportStorage");
                    _dom.DocumentElement.AppendChild(reportStorage);
                }

                reportStorage.SetAttribute("serverName", this.textBox_sqlServerName.Text);
                reportStorage.SetAttribute("userId", this.textBox_loginName.Text);
                reportStorage.SetAttribute("password", EncryptReportPassword(this.textBox_loginPassword.Text));
                reportStorage.SetAttribute("databaseName", this.textBox_instanceName.Text);
            }

            _dom.Save(this.LibraryXmlFileName);
            this.Changed = false;
        }

        internal string DecryptReportPassword(string password)
        {
            // password可能为空
            try
            {
                return Cryptography.Decrypt(password,
                        "dp2003");
            }
            catch
            {
                /*
                strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                return -1;
                */
                return "errorpassword";
            }
        }

        internal string EncryptReportPassword(string text)
        {
            return Cryptography.Encrypt(text, "dp2003");
        }

        const string EncryptKey = "dp2circulationpassword";

        // 加密明文
        public static string EncryptPassword(string PlainText)
        {
            return Cryptography.Encrypt(PlainText, EncryptKey);
        }

        // 解密加密过的文字
        public static string DecryptPassword(string EncryptText)
        {
            return Cryptography.Decrypt(EncryptText, EncryptKey);
        }

        public int VerifySqlServer(
string strSqlServerName,
string strDatabaseName,
string strSqlUserName,
string strSqlUserPassword,
out string strError)
        {
            strError = "";

            string strConnection = $"Server={strSqlServerName};User Id={strSqlUserName};Password={strSqlUserPassword};";   // Database={strDatabaseName};

            /*
            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strSqlUserPassword + ";"
                + strSqlServerName + ";"
                + "Connect Timeout=30;";
            */

            try
            {
                using (MySqlConnection connection = new MySqlConnection(strConnection))
                {
                    try
                    {
                        connection.Open();
                        MySqlConnection.ClearPool(connection);
                    }
                    catch (MySqlException sqlEx)
                    {
                        strError = "连接 SQL 数据库出错： " + sqlEx.Message + "。";
                        int nError = (int)sqlEx.ErrorCode;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "连接 SQL 数据库出错： " + ex.Message + " 类型:" + ex.GetType().ToString();
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }
            return 0;
        }
    }
}
