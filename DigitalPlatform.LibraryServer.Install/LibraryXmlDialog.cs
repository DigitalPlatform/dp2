using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 服务器同步参数对话框
    /// </summary>
    public partial class LibraryXmlDialog : Form
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

        public LibraryXmlDialog()
        {
            InitializeComponent();
        }

        private void LibraryXmlDialog_Load(object sender, EventArgs e)
        {
            this.Invoke(new Action(LoadLibraryXml));
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.Changed)
                SaveLibraryXml();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_editMasterServer_Click(object sender, EventArgs e)
        {
            ConfirmSupervisorDialog dlg = new ConfirmSupervisorDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.Text = "主服务器";
            dlg.Comment = "请设置主服务器的地址和用户名";
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

        string m_strUrl = "";
        string m_strUserName = "";
        string m_strPassword = "";

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

            XmlElement server = _dom.DocumentElement.SelectSingleNode("serverReplication") as XmlElement;
            if (server == null)
                return;

            this.m_strUrl = server.GetAttribute("url");
            this.m_strUserName = server.GetAttribute("username");
            try
            {
                this.m_strPassword = DecryptPassword(server.GetAttribute("password"));
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                strError = "library.xml 中 serverReplication 元素的 password 属性值不合法";
                goto ERROR1;
            }

            RefreshMasterServerSummary();
            return;
        ERROR1:
            this.MessageBoxShow(strError);
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

        void SaveLibraryXml()
        {
            XmlElement server = _dom.DocumentElement.SelectSingleNode("serverReplication") as XmlElement;
            if (server == null)
            {
                server = _dom.CreateElement("serverReplication");
                _dom.DocumentElement.AppendChild(server);
            }

            server.SetAttribute("url", this.m_strUrl);
            server.SetAttribute("username", this.m_strUserName);
            server.SetAttribute("password", EncryptPassword(this.m_strPassword));

            _dom.Save(this.LibraryXmlFileName);
            this.Changed = false;
        }

        void RefreshMasterServerSummary()
        {
            this.textBox_masterServer.Text = "dp2library URL = " + this.m_strUrl
                + "; UserName = " + this.m_strUserName
                + "; Password = " + new string('*', this.m_strPassword.Length);
        }

        private void LibraryXmlDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "LibraryXmlDialog",
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
    }
}
