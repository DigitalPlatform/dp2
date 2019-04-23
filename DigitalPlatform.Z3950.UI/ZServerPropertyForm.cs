using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Text;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;    // GetDp2ResDlg

namespace DigitalPlatform.Z3950.UI
{
    /// <summary>
    /// Z39.50服务器属性对话框
    /// </summary>
    public partial class ZServerPropertyForm : Form
    {
        public event FindDp2ServerEventHandler FindDp2Server;

        public XmlNode XmlNode = null;  // 服务器XML节点
        public string InitialResultInfo = "";

        // public MainForm MainForm = null; // 2007/12/16

        public ZServerPropertyForm()
        {
            InitializeComponent();
        }

        public bool UnionCatalogPageVisible
        {
            get
            {
                return this.tabControl_main.TabPages.Contains(this.tabPage_unionCatalog);
            }
            set
            {
                if (value == true)
                {
                    if (this.tabControl_main.TabPages.Contains(this.tabPage_unionCatalog) == false)
                        this.tabControl_main.TabPages.Add(this.tabPage_unionCatalog);
                }
                else
                    this.tabControl_main.TabPages.Remove(this.tabPage_unionCatalog);
            }
        }

        public static string GetPassword(string strEncrypted)
        {
            if (String.IsNullOrEmpty(strEncrypted) == true)
                return "";

            return Cryptography.Decrypt(
                    strEncrypted,
                    "dp2catalogpasswordkey");
        }

        public static string GetEncryptedPassword(string strPlainText)
        {
            if (String.IsNullOrEmpty(strPlainText) == true)
                return "";

            return Cryptography.Encrypt(strPlainText,
                "dp2catalogpasswordkey");
        }

        private void ZServerPropertyForm_Load(object sender, EventArgs e)
        {
            this.textBox_serverName.Text = DomUtil.GetAttr(this.XmlNode,
                "name");
            this.textBox_serverAddr.Text = DomUtil.GetAttr(this.XmlNode,
                "addr");
            this.textBox_serverPort.Text = DomUtil.GetAttr(this.XmlNode,
                "port");

            this.textBox_homepage.Text = DomUtil.GetAttr(this.XmlNode,
                "homepage");

            // authmethod 默认 "0"
            string strAuthMethod = DomUtil.GetAttr(this.XmlNode,
                "authmethod");
            int nAuthMethod = 0;
            try
            {
                nAuthMethod = Convert.ToInt32(strAuthMethod);
            }
            catch
            {
            }

            if (nAuthMethod == 0)
            {
                this.radioButton_authenStyeOpen.Checked = true;
            }
            else
            {
                this.radioButton_authenStyleIdpass.Checked = true;
            }

            this.textBox_groupID.Text = DomUtil.GetAttr(this.XmlNode,
                "groupid");
            this.textBox_userName.Text = DomUtil.GetAttr(this.XmlNode,
                "username");
            string strPassword = DomUtil.GetAttr(this.XmlNode,
            "password");
            if (string.IsNullOrEmpty(strPassword) == false)
                this.textBox_password.Text = GetPassword(strPassword);
            else
                this.textBox_password.Text = "";

            // converteacc 缺省值为 "0"
            this.checkBox_autoDetectEACC.Checked = GetBool(
                DomUtil.GetAttr(this.XmlNode,
                "converteacc"), false);

            // firstfull 缺省值为 "0"
            this.checkBox_alwaysUseFullElementSet.Checked =
                GetBool(DomUtil.GetAttr(this.XmlNode,
                "firstfull"), false);

            // detectmarcsyntax 缺省值为 "0"
            this.checkBox_autoDetectMarcSyntax.Checked =
                GetBool(DomUtil.GetAttr(this.XmlNode,
                "detectmarcsyntax"), false);

            // ignorereferenceid 缺省值为 "0"
            this.checkBox_ignoreRerenceID.Checked = GetBool(
DomUtil.GetAttr(this.XmlNode,
"ignorereferenceid"), false);

            // 对ISBN的预处理
            this.m_nDisableCheckIsbnParameters++;
            try
            {
                // isbn_force13 缺省值为 "0"
                this.checkBox_isbn_forceIsbn13.Checked = GetBool(
    DomUtil.GetAttr(this.XmlNode,
    "isbn_force13"), false);
                // isbn_force10 缺省值为 "0"
                this.checkBox_isbn_forceIsbn10.Checked = GetBool(
    DomUtil.GetAttr(this.XmlNode,
    "isbn_force10"), false);
                // isbn_addhyphen 缺省值为 "0"
                this.checkBox_isbn_addHyphen.Checked = GetBool(
    DomUtil.GetAttr(this.XmlNode,
    "isbn_addhyphen"), false);
                // isbn_removehyphen 缺省值为 "0"
                this.checkBox_isbn_removeHyphen.Checked = GetBool(
    DomUtil.GetAttr(this.XmlNode,
    "isbn_removehyphen"), false);
                // isbn_wild 缺省值为 "0"
                this.checkBox_isbn_wild.Checked = GetBool(
DomUtil.GetAttr(this.XmlNode,
"isbn_wild"), false);

            }
            finally
            {
                this.m_nDisableCheckIsbnParameters--;
            }

            // 2018/8/25
            // issn_force8 缺省值为 "1"
            string strValue = DomUtil.GetAttr(this.XmlNode, "issn_force8");
            //if (string.IsNullOrEmpty(strValue))
            //    strValue = "1"; // 缺省值为 1
            this.checkBox_forceIssn8.Checked = GetBool(strValue, true);

            // recsperbatch 缺省值为 "10"
            this.textBox_presentPerCount.Text = DomUtil.GetAttr(this.XmlNode,
                "recsperbatch");
            //if (string.IsNullOrEmpty(this.textBox_presentPerCount.Text))
            //    this.textBox_presentPerCount.Text = "10";

            // 数据库名
            XmlNodeList nodes = this.XmlNode.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strDatabaseName = DomUtil.GetAttr(nodes[i], "name");

                if (this.textBox_databaseNames.Text != "")
                    this.textBox_databaseNames.Text += "\r\n";

                this.textBox_databaseNames.Text += strDatabaseName;

                // 全选时候要排除的数据库名
                DomUtil.GetBooleanParam(nodes[i],
                    "notInAll",
                    false,
                    out bool bValue,
                    out string strError);
                if (bValue == true)
                {
                    if (this.textBox_notInAllDatabaseNames.Text != "")
                        this.textBox_notInAllDatabaseNames.Text += "\r\n";

                    this.textBox_notInAllDatabaseNames.Text += strDatabaseName;
                }
            }

            FillEncodingList(this.comboBox_defaultEncoding,
                true);
            this.comboBox_defaultEncoding.Text = DomUtil.GetAttr(this.XmlNode,
                "defaultEncoding");
            /*
            // 补充MARC-8
            this.comboBox_defaultEncoding.Items.Add("MARC-8");
             * */

            FillEncodingList(this.comboBox_queryTermEncoding,
                false); // 检索词暂时不让用MARC-8编码方式
            this.comboBox_queryTermEncoding.Text = DomUtil.GetAttr(this.XmlNode,
                "queryTermEncoding");

            this.comboBox_defaultMarcSyntaxOID.Text = DomUtil.GetAttr(this.XmlNode,
                "defaultMarcSyntaxOID");

            this.comboBox_defaultElementSetName.Text = DomUtil.GetAttr(this.XmlNode,
                "defaultElementSetName");

            string strBindingDef = DomUtil.GetAttr(this.XmlNode,
                "recordSyntaxAndEncodingBinding");
            SetBinding(strBindingDef);


            // 字符集协商
            // charNegoUtf8 缺省值为 "0"
            this.checkBox_charNegoUTF8.Checked = GetBool(DomUtil.GetAttr(this.XmlNode,
                "charNegoUtf8"), false);
            // charNego_recordsInSeletedCharsets 缺省值为 "0"
            this.checkBox_charNegoRecordsInSelectedCharSets.Checked = GetBool(DomUtil.GetAttr(this.XmlNode,
                "charNego_recordsInSeletedCharsets"), false);

            checkBox_charNegoUTF8_CheckedChanged(null, null);

            // initial result info
            /*
            this.textBox_initializeInfomation.Text = DomUtil.GetAttr(this.XmlNode,
                "extraInfo");
             * */
            this.textBox_initializeInfomation.Text = InitialResultInfo;

            // set initial button state
            textBox_homepage_TextChanged(this, null);

            // 联合编目
            this.textBox_unionCatalog_bindingDp2ServerName.Text = DomUtil.GetAttr(
                this.XmlNode, "unionCatalog_bindingDp2ServerName");

            this.textBox_unionCatalog_bindingUcServerUrl.Text = DomUtil.GetAttr(
    this.XmlNode, "unionCatalog_bindingUcServerUrl");
        }

        public static void FillEncodingList(ComboBox list,
    bool bHasMarc8)
        {
            list.Items.Clear();

            List<string> encodings = GetEncodingList(bHasMarc8);
            for (int i = 0; i < encodings.Count; i++)
            {
                list.Items.Add(encodings[i]);
            }
        }

        // 列出encoding名列表
        // 需要把gb2312 utf-8等常用的提前
        public static List<string> GetEncodingList(bool bHasMarc8)
        {
            List<string> result = new List<string>();

            EncodingInfo[] infos = Encoding.GetEncodings();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].GetEncoding().Equals(Encoding.GetEncoding(936)) == true)
                    result.Insert(0, infos[i].Name);
                else if (infos[i].GetEncoding().Equals(Encoding.UTF8) == true)
                    result.Insert(0, infos[i].Name);
                else
                    result.Add(infos[i].Name);
            }

            if (bHasMarc8 == true)
                result.Add("MARC-8");

            return result;
        }

        /*
        // 将绑定定义字符串设置到listview中
        void SetBinding(string strBindingString)
        {
            this.listView_recordSyntaxAndEncodingBinding.Items.Clear();

            string [] lines = strBindingString.Split(new string [] {"||"},
                StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i<lines.Length;i++)
            {
                string strSyntax = "";
                string strEncoding = "";
                string strLine = lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                int nRet = strLine.IndexOf('|');
                if (nRet != -1)
                {
                    strSyntax = strLine.Substring(0, nRet).Trim();
                    strEncoding = strLine.Substring(nRet + 1).Trim();
                }
                else
                {
                    strSyntax = strLine;
                    strEncoding = "";
                }

                ListViewItem item = new ListViewItem();
                item.Text = strSyntax;
                item.SubItems.Add(strEncoding);

                this.listView_recordSyntaxAndEncodingBinding.Items.Add(item);
            }
        }*/

        // 将绑定定义字符串设置到listview中
        void SetBinding(string strBindingString)
        {
            this.listView_recordSyntaxAndEncodingBinding.Items.Clear();

            RecordSyntaxAndEncodingBindingCollection bindings = new RecordSyntaxAndEncodingBindingCollection();
            bindings.Load(strBindingString);

            for (int i = 0; i < bindings.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem binding = bindings[i];

                ListViewItem item = new ListViewItem();
                item.Text = binding.RecordSyntax;
                item.SubItems.Add(binding.Encoding);

                this.listView_recordSyntaxAndEncodingBinding.Items.Add(item);
            }
        }


        // 获得绑定字符串
        string GetBindingString()
        {
            string strResult = "";
            for (int i = 0; i < this.listView_recordSyntaxAndEncodingBinding.Items.Count; i++)
            {
                ListViewItem item = this.listView_recordSyntaxAndEncodingBinding.Items[i];

                strResult += item.Text + "|" + item.SubItems[1].Text + "||";
            }

            return strResult;
        }

        public static bool GetBool(string strText, bool default_value)
        {
            if (string.IsNullOrEmpty(strText))
                return default_value;

            if (strText == "1")
                return true;
            if (strText.ToLower() == "y"
                || strText.ToLower() == "t")
                return true;

            return false;
        }

        private void ZServerPropertyForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ZServerPropertyForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 提醒切断Z-association
        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 服务器名应该必备
            if (string.IsNullOrEmpty(this.textBox_serverName.Text))
            {
                this.tabControl_main.SelectedTab = this.tabPage_general;
                strError = "尚未指定服务器名";
                goto ERROR1;
            }

            // 服务器地址应该必备
            // TODO: 检查是否输入了 http:// 这样的不合法地址
            if (string.IsNullOrEmpty(this.textBox_serverAddr.Text))
            {
                this.tabControl_main.SelectedTab = this.tabPage_general;
                strError = "尚未指定服务器地址";
                goto ERROR1;
            }

            // TODO: 端口号似乎应该允许缺省，缺省值为 210
#if NO
            if (string.IsNullOrEmpty(this.textBox_serverPort.Text))
            {
                this.tabControl_main.SelectedTab = this.tabPage_general;
                strError = "尚未指定服务器端口号";
                goto ERROR1;
            }
#endif

            if (string.IsNullOrEmpty(this.textBox_serverPort.Text) == false
                && StringUtil.IsNumber(this.textBox_serverPort.Text) == false)
            {
                this.tabControl_main.SelectedTab = this.tabPage_general;
                strError = "服务器端口号必须为纯数字";
                goto ERROR1;
            }

            // TODO: 每批记录条数应默认为 10
#if NO
            if (string.IsNullOrEmpty(this.textBox_presentPerCount.Text))
            {
                this.tabControl_main.SelectedTab = this.tabPage_search;
                strError = "尚未指定每批记录条数";
                goto ERROR1;
            }
#endif

            if (string.IsNullOrEmpty(this.textBox_presentPerCount.Text) == false
                && StringUtil.IsNumber(this.textBox_presentPerCount.Text) == false)
            {
                this.tabControl_main.SelectedTab = this.tabPage_search;
                strError = "每批记录条数必须为纯数字";
                goto ERROR1;
            }

            // 数据库名应该必备
            if (string.IsNullOrEmpty(this.textBox_databaseNames.Text))
            {
                this.tabControl_main.SelectedTab = this.tabPage_database;
                strError = "尚未指定数据库名";
                goto ERROR1;
            }

            if (this.checkBox_isbn_addHyphen.Checked == true
                && this.checkBox_isbn_removeHyphen.Checked == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_search;
                strError = "ISBN 加入横杠 和 去除横杠 不能同时为 true";
                goto ERROR1;
            }

            if (this.checkBox_isbn_forceIsbn10.Checked == true
    && this.checkBox_isbn_forceIsbn13.Checked == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_search;
                strError = "ISBN 规整为13位 和 规整为10位 不能同时为 true";
                goto ERROR1;
            }

            DomUtil.SetAttr(this.XmlNode,
                "name", this.textBox_serverName.Text);
            DomUtil.SetAttr(this.XmlNode,
                "addr", this.textBox_serverAddr.Text);
            if (string.IsNullOrEmpty(this.textBox_serverPort.Text) == false)
                DomUtil.SetAttr(this.XmlNode,
                    "port", this.textBox_serverPort.Text);
            if (string.IsNullOrEmpty(this.textBox_homepage.Text) == false)
                DomUtil.SetAttr(this.XmlNode,
                    "homepage", this.textBox_homepage.Text);

            // authmethod 默认 "0"
            if (this.radioButton_authenStyeOpen.Checked == true)
            {
                // 删除属性，减小 XML 尺寸
                DomUtil.SetAttr(this.XmlNode, "authmethod", null);

                // DomUtil.SetAttr(this.XmlNode,
                // "authmethod", "0");
            }
            else
            {
                DomUtil.SetAttr(this.XmlNode,
                "authmethod", "1");
            }

            SetTextAttr("groupid", this.textBox_groupID.Text);

            SetTextAttr("username", this.textBox_userName.Text);

            if (string.IsNullOrEmpty(this.textBox_password.Text) == false)
            {
                string strPassword = GetEncryptedPassword(this.textBox_password.Text);
                DomUtil.SetAttr(this.XmlNode,
                    "password", strPassword);
            }
            else
                DomUtil.SetAttr(this.XmlNode, "password", null);

            // converteacc 缺省值为 "0"
            if (this.checkBox_autoDetectEACC.Checked == true)
            {
                DomUtil.SetAttr(this.XmlNode,
                "converteacc", "1");
            }
            else
            {
                // 节省 XML 文件尺寸
                DomUtil.SetAttr(this.XmlNode, "converteacc", null);
                // DomUtil.SetAttr(this.XmlNode,
                // "converteacc", "0");
            }

            // firstfull 缺省值为 "0"
            if (this.checkBox_alwaysUseFullElementSet.Checked == true)
                DomUtil.SetAttr(this.XmlNode,
                "firstfull", "1");
            else
            {
                DomUtil.SetAttr(this.XmlNode,
"firstfull", null);
                // DomUtil.SetAttr(this.XmlNode,
                // "firstfull", "0");
            }

            // detectmarcsyntax 缺省值为 "0"
            if (this.checkBox_autoDetectMarcSyntax.Checked == true)
                DomUtil.SetAttr(this.XmlNode,
                "detectmarcsyntax", "1");
            else
            {
                DomUtil.SetAttr(this.XmlNode,
"detectmarcsyntax", null);

                //DomUtil.SetAttr(this.XmlNode,
                //"detectmarcsyntax", "0");
            }

            // ignorereferenceid 缺省值为 "0"
            if (this.checkBox_ignoreRerenceID.Checked == true)
                DomUtil.SetAttr(this.XmlNode,
                "ignorereferenceid", "1");
            else
            {
                DomUtil.SetAttr(this.XmlNode,
"ignorereferenceid", null);
                //DomUtil.SetAttr(this.XmlNode,
                //"ignorereferenceid", "0");
            }

            // 对ISBN的预处理

            // isbn_force13 缺省值为 "0"
            DomUtil.SetAttr(this.XmlNode,
            "isbn_force13",
            this.checkBox_isbn_forceIsbn13.Checked == true ? "1" : null);

            // isbn_force10 缺省值为 "0"
            DomUtil.SetAttr(this.XmlNode,
            "isbn_force10",
            this.checkBox_isbn_forceIsbn10.Checked == true ? "1" : null);

            // isbn_addhyphen 缺省值为 "0"
            DomUtil.SetAttr(this.XmlNode,
            "isbn_addhyphen",
            this.checkBox_isbn_addHyphen.Checked == true ? "1" : null);

            // isbn_removehyphen 缺省值为 "0"
            DomUtil.SetAttr(this.XmlNode,
            "isbn_removehyphen",
            this.checkBox_isbn_removeHyphen.Checked == true ? "1" : null);

            // isbn_wild 缺省值为 "0"
            DomUtil.SetAttr(this.XmlNode,
"isbn_wild",
this.checkBox_isbn_wild.Checked == true ? "1" : null);

            // 2018/8/25
            // issn_force8 缺省值为 "1"
            DomUtil.SetAttr(this.XmlNode,
"issn_force8",
this.checkBox_forceIssn8.Checked == true ? null : "0");

            // recsperbatch 缺省值为 "10"
            SetTextAttr("recsperbatch", this.textBox_presentPerCount.Text);

            SetTextAttr("defaultEncoding",
                this.comboBox_defaultEncoding.Text);

            SetTextAttr("queryTermEncoding",
    this.comboBox_queryTermEncoding.Text);

            SetTextAttr("defaultMarcSyntaxOID",
                this.comboBox_defaultMarcSyntaxOID.Text);

            SetTextAttr("defaultElementSetName",
    this.comboBox_defaultElementSetName.Text);

            // 先删除原有的下级数据库
            XmlNodeList nodes = this.XmlNode.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                this.XmlNode.RemoveChild(nodes[i]);
            }

            //
            List<string> exclude_dbnames = new List<string>();
            for (int i = 0; i < this.textBox_notInAllDatabaseNames.Lines.Length; i++)
            {
                string strName = this.textBox_notInAllDatabaseNames.Lines[i].Trim();
                if (String.IsNullOrEmpty(strName) == true)
                    continue;
                exclude_dbnames.Add(strName);
            }
            // 数据库名
            for (int i = 0; i < this.textBox_databaseNames.Lines.Length; i++)
            {
                string strName = this.textBox_databaseNames.Lines[i].Trim();
                if (String.IsNullOrEmpty(strName) == true)
                    continue;

                XmlNode newxmlnode = this.XmlNode.OwnerDocument.CreateElement("database");
                this.XmlNode.AppendChild(newxmlnode);
                DomUtil.SetAttr(newxmlnode, "name",
                    strName);

                if (exclude_dbnames.IndexOf(strName) != -1)
                    DomUtil.SetAttr(newxmlnode, "notInAll", "true");
            }

            string strBindingDef = GetBindingString();
            SetTextAttr("recordSyntaxAndEncodingBinding",
                    strBindingDef);

            // 字符集协商
            // charNegoUtf8 缺省值为 "0"
            if (this.checkBox_charNegoUTF8.Checked == true)
                DomUtil.SetAttr(this.XmlNode,
                    "charNegoUtf8", "1");
            else
            {
                DomUtil.SetAttr(this.XmlNode,
                    "charNegoUtf8", null);
                //DomUtil.SetAttr(this.XmlNode,
                //    "charNegoUtf8", "0");
            }

            // charNego_recordsInSeletedCharsets 缺省值为 "0"
            if (this.checkBox_charNegoRecordsInSelectedCharSets.Checked == true)
                DomUtil.SetAttr(this.XmlNode,
                    "charNego_recordsInSeletedCharsets", "1");
            else
            {
                DomUtil.SetAttr(this.XmlNode,
                    "charNego_recordsInSeletedCharsets", null);
                //DomUtil.SetAttr(this.XmlNode,
                //    "charNego_recordsInSeletedCharsets", "0");
            }

            // 联合编目
            SetTextAttr("unionCatalog_bindingDp2ServerName",
                this.textBox_unionCatalog_bindingDp2ServerName.Text);

            SetTextAttr("unionCatalog_bindingUcServerUrl",
    this.textBox_unionCatalog_bindingUcServerUrl.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void SetTextAttr(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
                DomUtil.SetAttr(this.XmlNode, name, null);
            else
                DomUtil.SetAttr(this.XmlNode,
                    name, value);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton_authenStyeOpen_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_authenStyeOpen.Checked == true)
                this.textBox_groupID.Enabled = false;
            else
                this.textBox_groupID.Enabled = true;

        }

        private void radioButton_authenStyleIdpass_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_authenStyeOpen.Checked == true)
                this.textBox_groupID.Enabled = false;
            else
                this.textBox_groupID.Enabled = true;

        }

        public string ServerName
        {
            get
            {
                return this.textBox_serverName.Text;
            }
        }

        private void button_modifyBindingItem_Click(object sender, EventArgs e)
        {
            if (this.listView_recordSyntaxAndEncodingBinding.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要修改的事项");
                return;
            }

            ListViewItem item = this.listView_recordSyntaxAndEncodingBinding.SelectedItems[0];

            RecordSyntaxAndEncodingBindingItemDlg dlg = new RecordSyntaxAndEncodingBindingItemDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.RecordSyntax = item.Text;
            dlg.Encoding = item.SubItems[1].Text;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            item.Text = dlg.RecordSyntax;
            item.SubItems[1].Text = dlg.Encoding;
        }

        private void button_newBindingItem_Click(object sender, EventArgs e)
        {

            RecordSyntaxAndEncodingBindingItemDlg dlg = new RecordSyntaxAndEncodingBindingItemDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Encoding = this.comboBox_defaultEncoding.Text;  // 引入缺省编码方式值
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 查重
            string strNewSyntax = GetLeftValue(dlg.RecordSyntax);
            for (int i = 0; i < this.listView_recordSyntaxAndEncodingBinding.Items.Count; i++)
            {
                string strExistSyntax = this.listView_recordSyntaxAndEncodingBinding.Items[i].Text;
                strExistSyntax = GetLeftValue(strExistSyntax);

                if (strNewSyntax == strExistSyntax)
                {
                    MessageBox.Show(this, "数据格式 '" + strNewSyntax + "' 已经存在(第 " + (i + 1).ToString() + " 行)，不能重复加入");
                    return;
                }
            }

            ListViewItem item = new ListViewItem();
            item.Text = dlg.RecordSyntax;
            item.SubItems.Add(dlg.Encoding);

            this.listView_recordSyntaxAndEncodingBinding.Items.Add(item);
        }

        private void button_deleteBindingItem_Click(object sender, EventArgs e)
        {
            if (this.listView_recordSyntaxAndEncodingBinding.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的事项");
                return;
            }

            this.listView_recordSyntaxAndEncodingBinding.Items.RemoveAt(
                this.listView_recordSyntaxAndEncodingBinding.SelectedIndices[0]);
        }

        private void listView_recordSyntaxAndEncodingBinding_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_recordSyntaxAndEncodingBinding.SelectedItems.Count > 0)
            {
                this.button_modifyBindingItem.Enabled = true;
                this.button_deleteBindingItem.Enabled = true;
            }
            else
            {
                this.button_modifyBindingItem.Enabled = false;
                this.button_deleteBindingItem.Enabled = false;
            }
        }

        private void textBox_homepage_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_homepage.Text != "")
                this.button_gotoHomepage.Enabled = true;
            else
                this.button_gotoHomepage.Enabled = false;
        }

        private void button_gotoHomepage_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(// "iexplore", 
                this.textBox_homepage.Text);
        }

        private void checkBox_charNegoUTF8_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_charNegoUTF8.Checked == true)
            {
                this.checkBox_charNegoRecordsInSelectedCharSets.Enabled = true;
            }
            else
            {
                this.checkBox_charNegoRecordsInSelectedCharSets.Enabled = false;
                this.checkBox_charNegoRecordsInSelectedCharSets.Checked = false;
            }
        }

        private void button_unionCatalog_findDp2Server_Click(object sender, EventArgs e)
        {
            if (this.FindDp2Server != null)
            {
                FindDp2ServerEventArgs e1 = new FindDp2ServerEventArgs();
                e1.Dp2ServerPath = this.textBox_unionCatalog_bindingDp2ServerName.Text;
                this.FindDp2Server(this, e1);
                if (e1.Canceled)
                    return;
                this.textBox_unionCatalog_bindingDp2ServerName.Text = e1.Dp2ServerPath;
            }
#if REMOVED
            dp2SearchForm dp2_searchform = this.MainForm.GetDp2SearchForm();

            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请指定要绑定的 dp2library 服务器名";
#if OLD_CHANNEL
            dlg.dp2Channels = dp2_searchform.Channels;
#endif
            dlg.ChannelManager = this.MainForm;
            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_SERVER };
            dlg.Path = this.textBox_unionCatalog_bindingDp2ServerName.Text;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_unionCatalog_bindingDp2ServerName.Text = dlg.Path;
#endif
        }

        private void comboBox_defaultMarcSyntaxOID_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_defaultMarcSyntaxOID.Invalidate();
        }

        private void comboBox_defaultElementSetName_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_defaultElementSetName.Invalidate();
        }

        private void comboBox_queryTermEncoding_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_queryTermEncoding.Invalidate();
        }

        private void comboBox_defaultEncoding_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_defaultEncoding.Invalidate();
        }

        int m_nDisableCheckIsbnParameters = 0;

        private void checkBox_isbn_forceIsbn13_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableCheckIsbnParameters > 0)
                return;

            if (this.checkBox_isbn_forceIsbn13.Checked == true)
            {
                if (this.checkBox_isbn_forceIsbn10.Checked == true)
                    this.checkBox_isbn_forceIsbn10.Checked = false;
            }
        }

        private void checkBox_isbn_forceIsbn10_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableCheckIsbnParameters > 0)
                return;

            if (this.checkBox_isbn_forceIsbn10.Checked == true)
            {
                if (this.checkBox_isbn_forceIsbn13.Checked == true)
                    this.checkBox_isbn_forceIsbn13.Checked = false;
            }
        }

        private void checkBox_isbn_addHyphen_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableCheckIsbnParameters > 0)
                return;

            if (this.checkBox_isbn_addHyphen.Checked == true)
            {
                if (this.checkBox_isbn_removeHyphen.Checked == true)
                    this.checkBox_isbn_removeHyphen.Checked = false;
            }
        }

        private void checkBox_isbn_removeHyphen_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableCheckIsbnParameters > 0)
                return;

            if (this.checkBox_isbn_removeHyphen.Checked == true)
            {
                if (this.checkBox_isbn_addHyphen.Checked == true)
                    this.checkBox_isbn_addHyphen.Checked = false;
            }
        }

        private void textBox_serverName_TextChanged(object sender, EventArgs e)
        {
            string strName = this.textBox_serverName.Text;
            this.Text = "Z39.50服务器属性"
                + (string.IsNullOrEmpty(strName) == false ? " -- " + strName : "");
        }

        /*
        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            {
                dp2_searchform = this.MainForm.TopDp2SearchForm;

                if (dp2_searchform == null)
                {
                    // 新开一个dp2检索窗
                    dp2_searchform = new dp2SearchForm();
                    dp2_searchform.MainForm = this.MainForm;
                    dp2_searchform.MdiParent = this.MainForm;
                    dp2_searchform.WindowState = FormWindowState.Minimized;
                    dp2_searchform.Show();
                }
            }

            return dp2_searchform;
        }*/

        // 解析出 '-' 左边的值
        public static string GetLeftValue(string strText)
        {
            int nRet = strText.IndexOf("-");
            if (nRet != -1)
                return strText.Substring(0, nRet).Trim();
            else
                return strText.Trim();
        }

        private void textBox_serverAddr_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.textBox_serverAddr.Text.IndexOfAny(new char[] { ':' }) != -1)
            {
                MessageBox.Show(this, $"所输入的值 '{this.textBox_serverAddr.Text}' 不合法。不允许包含冒号。请重新输入");
                e.Cancel = true;
            }
        }
    }

    /// <summary>
    /// 查找 dp2 服务器
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void FindDp2ServerEventHandler(object sender,
    FindDp2ServerEventArgs e);

    /// <summary>
    /// 查找 dp2 服务器事件的参数
    /// </summary>
    public class FindDp2ServerEventArgs : EventArgs
    {
        // [in][out]
        public string Dp2ServerPath { get; set; }

        // [out]
        public bool Canceled { get; set; }
    }

#if REMOVED
    // 绑定信息数组
    public class RecordSyntaxAndEncodingBindingCollection : List<RecordSyntaxAndEncodingBindingItem>
    {
        // parameters:
        //      strBindingString    格式为"syntaxoid1 -- syntaxcomment1|encodingname1 -- encodingcomment1||syntaxoid2 -- syntaxcomment2|encodingname2 -- encodingcomment2"，末尾可能有多余的“||”
        public void Load(string strBindingString)
        {
            this.Clear();

            string[] lines = strBindingString.Split(new string[] { "||" },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string strSyntax = "";
                string strEncoding = "";
                string strLine = lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                int nRet = strLine.IndexOf('|');
                if (nRet != -1)
                {
                    strSyntax = strLine.Substring(0, nRet).Trim();
                    strEncoding = strLine.Substring(nRet + 1).Trim();
                }
                else
                {
                    strSyntax = strLine;
                    strEncoding = "";
                }

                RecordSyntaxAndEncodingBindingItem item = new RecordSyntaxAndEncodingBindingItem();
                item.RecordSyntax = strSyntax;
                item.Encoding = strEncoding;

                this.Add(item);
            }
        }

        // 返还为字符串形态
        public string GetString()
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem item = this[i];
                strResult += item.RecordSyntax + "|" + item.Encoding + "||";
            }

            return strResult;
        }

        public string GetEncodingName(string strRecordSyntaxOID)
        {
            for (int i = 0; i < this.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem item = this[i];

                if (item.RecordSyntaxOID == strRecordSyntaxOID)
                    return item.EncodingName;
            }

            return null;    // not found
        }

    }

#endif

#if REMOVED
    // 绑定信息元素
    public class RecordSyntaxAndEncodingBindingItem
    {
        public string RecordSyntaxOID = "";
        public string RecordSyntaxComment = "";
        public string EncodingName = "";
        public string EncodingNameComment = "";

        // 将 "value -- comment" 形态的字符串拆分为"value"和"comment"两个部分
        public static void ParseValueAndComment(string strText,
            out string strValue,
            out string strComment)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
            {
                strValue = strText.Trim();
                strComment = "";
                return;
            }

            strValue = strText.Substring(0, nRet).Trim();
            strComment = strText.Substring(nRet + 2).Trim();
        }

        public string RecordSyntax
        {
            get
            {
                if (String.IsNullOrEmpty(this.RecordSyntaxComment) == true)
                    return this.RecordSyntaxOID;

                return this.RecordSyntaxOID + " -- " + this.RecordSyntaxComment;
            }
            set
            {
                string strValue = "";
                string strComment = "";

                ParseValueAndComment(value, out strValue, out strComment);
                this.RecordSyntaxOID = strValue;
                this.RecordSyntaxComment = strComment;
            }
        }

        public string Encoding
        {
            get
            {
                if (String.IsNullOrEmpty(this.EncodingNameComment) == true)
                    return this.EncodingName;
                return this.EncodingName + " -- " + this.EncodingNameComment;
            }
            set
            {
                string strValue = "";
                string strComment = "";

                ParseValueAndComment(value, out strValue, out strComment);
                this.EncodingName = strValue;
                this.EncodingNameComment = strComment;
            }
        }
    }

#endif
}