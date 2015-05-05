using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Collections;

namespace dp2Circulation
{
    /// <summary>
    /// 管理书目库(组)的对话框
    /// 书目库组由若干数据库组成，管理比较复杂
    /// </summary>
    internal partial class BiblioDatabaseDialog : Form
    {
        int m_nInInitial = 0;

        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        public bool CreateMode = false; // 是否为创建模式？==true为创建模式；==false为修改模式
        public bool Recreate = false;   // 是否为重新创建模式？当CreateMode == true 时起作用
        XmlDocument dom = null;

        public BiblioDatabaseDialog()
        {
            InitializeComponent();
        }

        // 拆分 复制参数，放入界面
        void SetReplicationParam(string strText)
        {
            Hashtable table = StringUtil.ParseParameters(strText);
            this.comboBox_replication_centerServer.Text = (string)table["server"];
            this.comboBox_replication_dbName.Text = (string)table["dbname"];
        }

        // 从界面搜集复制参数
        string GetReplicationParam()
        {
            if (string.IsNullOrEmpty(this.comboBox_replication_centerServer.Text) == true
                && string.IsNullOrEmpty(this.comboBox_replication_dbName.Text) == true)
                return "";

            Hashtable table = new Hashtable();
            table["server"] = this.comboBox_replication_centerServer.Text;
            table["dbname"] = this.comboBox_replication_dbName.Text;
            return StringUtil.BuildParameterString(table);
        }

        public int Initial(string strXml,
            out string strError)
        {
            strError = "";

            this.dom = new XmlDocument();
            try
            {
                this.dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            string strType = DomUtil.GetAttr(dom.DocumentElement,
                "type");
            if (strType != "biblio")
            {
                strError = "<database>元素的type属性值('" + strType + "')应当为biblio";
                return -1;
            }

            this.m_nInInitial++;    // 避免xxxchanged响应

            try
            {
                this.textBox_biblioDbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "name");
                this.comboBox_syntax.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "syntax");
                // 2009/10/23 new add
                this.checkedComboBox_role.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "role");

                SetReplicationParam(DomUtil.GetAttr(dom.DocumentElement, "replication"));

                this.textBox_entityDbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "entityDbName");
                this.textBox_issueDbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "issueDbName");
                this.textBox_orderDbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "orderDbName");
                this.textBox_commentDbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "commentDbName");

                string strInCirculation = DomUtil.GetAttr(dom.DocumentElement,
                    "inCirculation");
                if (String.IsNullOrEmpty(strInCirculation) == true)
                    strInCirculation = "true";
                this.checkBox_inCirculation.Checked = DomUtil.IsBooleanTrue(strInCirculation);


                // usage属性一般在从服务器传来的XML片段中是没有的。仅仅在创建的时候，client发给server的xml片段中才有
                string strUsage = DomUtil.GetAttr(dom.DocumentElement,
                    "usage");
                if (String.IsNullOrEmpty(strUsage) == true)
                {
                    // 如果usage参数为空，则需要综合判断
                    if (this.textBox_issueDbName.Text == "")
                    {
                        strUsage = "book -- 图书";
                    }
                    else
                    {
                        strUsage = "series -- 期刊";
                    }
                }

                this.comboBox_documentType.Text = strUsage;
            }
            finally
            {
                this.m_nInInitial--;
            }


            return 0;
        }

        private void BiblioDatabaseDialog_Load(object sender, EventArgs e)
        {
            if (this.CreateMode == false)
            {
                this.comboBox_syntax.Enabled = false;
                this.comboBox_documentType.Enabled = false;
            }

        }

        private void textBox_biblioDbName_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nInInitial > 0)
                return;

            if (this.CreateMode == true)
            {
                /*
                string strSyntax = GetPureValue(this.comboBox_syntax.Text);
                if (String.IsNullOrEmpty(strSyntax) == true)
                    strSyntax = "unimarc";
                 * */

                string strUsage = GetPureValue(this.comboBox_documentType.Text);
                if (String.IsNullOrEmpty(strUsage) == true)
                    strUsage = "book";

                if (strUsage == "book")
                {
                    if (this.textBox_biblioDbName.Text == "")
                    {
                        this.textBox_entityDbName.Text = "";
                        this.textBox_orderDbName.Text = "";
                        this.textBox_issueDbName.Text = "";
                        this.textBox_commentDbName.Text = "";
                    }
                    else
                    {
                        this.textBox_entityDbName.Text = this.textBox_biblioDbName.Text + "实体";
                        this.textBox_orderDbName.Text = this.textBox_biblioDbName.Text + "订购";
                        this.textBox_issueDbName.Text = "";
                        this.textBox_commentDbName.Text = this.textBox_biblioDbName.Text + "评注";
                    }
                }
                else if (strUsage == "series")
                {
                    if (this.textBox_biblioDbName.Text == "")
                    {
                        this.textBox_entityDbName.Text = "";
                        this.textBox_orderDbName.Text = "";
                        this.textBox_issueDbName.Text = "";
                        this.textBox_commentDbName.Text = "";
                    }
                    else
                    {
                        this.textBox_entityDbName.Text = this.textBox_biblioDbName.Text + "实体";
                        this.textBox_orderDbName.Text = this.textBox_biblioDbName.Text + "订购";
                        this.textBox_issueDbName.Text = this.textBox_biblioDbName.Text + "期";
                        this.textBox_commentDbName.Text = this.textBox_biblioDbName.Text + "评注";
                    }
                }
            }
        }

        static string GetPureValue(string strText)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
                return strText;

            return strText.Substring(0, nRet).Trim();
        }

        private void comboBox_usage_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nInInitial > 0)
                return;

            string strUsage = GetPureValue(this.comboBox_documentType.Text);
            if (String.IsNullOrEmpty(strUsage) == true)
                strUsage = "book";

            if (strUsage == "book")
            {
                this.textBox_issueDbName.Text = "";
            }
            else if (strUsage == "series")
            {
                if (this.textBox_biblioDbName.Text == "")
                    this.textBox_issueDbName.Text = "";
                else
                    this.textBox_issueDbName.Text = this.textBox_biblioDbName.Text + "期";
            }
        }

        static string MakeListString(List<string> names)
        {
            string strResult = "";
            for (int i = 0; i < names.Count; i++)
            {
                strResult += names[i] + "\r\n";
            }

            return strResult;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 检查

            // syntax
            if (this.comboBox_syntax.Text == "")
            {
                strError = "尚未指定数据格式";
                goto ERROR1;
            }

            if (this.comboBox_documentType.Text == "")
            {
                strError = "尚未指定文献类型";
                goto ERROR1;
            }

            if (this.checkBox_inCirculation.Checked == true)
            {
                if (String.IsNullOrEmpty(this.textBox_entityDbName.Text) == true)
                {
                    strError = "要参与流通，就必须指定实体库名";
                    goto ERROR1;
                }
            }

            if ((string.IsNullOrEmpty(this.comboBox_replication_dbName.Text) == false
                && string.IsNullOrEmpty(this.comboBox_replication_centerServer.Text) == true)
                ||
            (string.IsNullOrEmpty(this.comboBox_replication_dbName.Text) == true
                && string.IsNullOrEmpty(this.comboBox_replication_centerServer.Text) == false))
            {
                strError = "“同步”属性页 的 中心服务器 和 书目库名，必须同时具备";
                goto ERROR1;
            }

            string strUsage = GetPureValue(this.comboBox_documentType.Text);
            string strRole = this.checkedComboBox_role.Text;
            string strReplication = GetReplicationParam();

            {
            REDO:
                if (strUsage == "book")
                {
                    if (String.IsNullOrEmpty(this.textBox_issueDbName.Text) == false)
                    {
                        // 2009/2/6 new add
                        if (this.CreateMode == false)
                        {
                            // 对话框警告
                            DialogResult result = MessageBox.Show(this,
                                "确实要将书目库 '"
                                + this.textBox_biblioDbName.Text
                                + "' 的文献类型修改为 期刊?",
                                "BiblioDatabaseDialog",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result != DialogResult.Yes)
                            {
                                strError = "当文献类型为 图书 时，不能指定期库名";
                                goto ERROR1;
                            }

                            strUsage = "series";  // 故意不改变combobox的内容，以便Cancel后能够恢复原状
                            goto REDO;
                        }
                        else
                        {

                            strError = "当文献类型为 图书 时，不能指定期库名";
                            goto ERROR1;
                        }
                    }
                }
                else if (strUsage == "series")
                {
                    if (StringUtil.IsInList("orderWork", strRole) == true)
                    {
                        strError = "当文献类型为 期刊 时，角色不能为 orderWork (采购工作库)";
                        goto ERROR1;
                    }

                    if (String.IsNullOrEmpty(this.textBox_issueDbName.Text) == true)
                    {
                        // 2009/2/6 new add
                        if (this.CreateMode == false)
                        {
                            // 对话框警告
                            DialogResult result = MessageBox.Show(this,
                                "确实要将书目库 '"
                                + this.textBox_biblioDbName.Text
                                + "' 的文献类型修改为 图书?",
                                "BiblioDatabaseDialog",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result != DialogResult.Yes)
                            {
                                strError = "当文献类型为 期刊 时，必须指定期库名";
                                goto ERROR1;
                            }

                            strUsage = "book";  // 故意不改变combobox的内容，以便Cancel后能够恢复原状
                            goto REDO;
                        }
                        else
                        {
                            strError = "当文献类型为 期刊 时，必须指定期库名";
                            goto ERROR1;
                        }
                    }
                }
            }

            // 针对 采购工作库 的检查
            if (StringUtil.IsInList("orderWork", strRole) == true)
            {
                if (String.IsNullOrEmpty(this.textBox_orderDbName.Text) == true)
                {
                    strError = "当角色为 orderWork (采购工作库)时，必须包含订购库";
                    goto ERROR1;
                }

                // 2009/11/5 new add
                if (String.IsNullOrEmpty(this.textBox_entityDbName.Text) == true)
                {
                    strError = "当角色为 orderWork (采购工作库)时，必须包含实体库";
                    goto ERROR1;
                }

                if (this.checkBox_inCirculation.Checked == true)
                {
                    strError = "当角色为 orderWork (采购工作库)时，不能参与流通";
                    goto ERROR1;
                }
            }

            // 针对 外源书目库 的检查
            if (StringUtil.IsInList("biblioSource", strRole) == true)
            {
                if (String.IsNullOrEmpty(this.textBox_biblioDbName.Text) == true)
                {
                    strError = "当角色为 biblioSource (外源书目库)时，必须包含书目库";
                    goto ERROR1;
                }

                if (this.checkBox_inCirculation.Checked == true)
                {
                    strError = "当角色为 biblioSource (外源书目库)时，不能参与流通";
                    goto ERROR1;
                }
            }

            if (this.CreateMode == true)
            {
                // 创建模式
                EnableControls(false);

                try
                {
                    string strDatabaseInfo = "";
                    string strOutputInfo = "";

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<root />");
                    XmlNode nodeDatabase = dom.CreateElement("database");
                    dom.DocumentElement.AppendChild(nodeDatabase);

                    // type
                    DomUtil.SetAttr(nodeDatabase, "type", "biblio");

                    // syntax
                    if (this.comboBox_syntax.Text == "")
                    {
                        strError = "尚未指定数据格式";
                        goto ERROR1;
                    }
                    string strSyntax = GetPureValue(this.comboBox_syntax.Text);
                    DomUtil.SetAttr(nodeDatabase, "syntax", strSyntax);

                    // usage
                    /*
                    if (this.comboBox_documentType.Text == "")
                    {
                        strError = "尚未指定文献类型";
                        goto ERROR1;
                    }
                    string strUsage = GetPureValue(this.comboBox_documentType.Text);
                     * */

                    DomUtil.SetAttr(nodeDatabase, "usage", strUsage);

                    // role
                    DomUtil.SetAttr(nodeDatabase, "role", strRole);

                    if (string.IsNullOrEmpty(strReplication) == false)
                        DomUtil.SetAttr(nodeDatabase, "replication", strReplication);

                    // inCirculation
                    string strInCirculation = "true";
                    if (this.checkBox_inCirculation.Checked == true)
                        strInCirculation = "true";
                    else
                        strInCirculation = "false";

                    DomUtil.SetAttr(nodeDatabase, "inCirculation", strInCirculation);


                    // 书目库名
                    if (this.textBox_biblioDbName.Text == "")
                    {
                        strError = "尚未指定书目库名";
                        goto ERROR1;
                    }
                    nRet = Global.CheckDbName(this.textBox_biblioDbName.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    DomUtil.SetAttr(nodeDatabase, "name", this.textBox_biblioDbName.Text);

                    // 实体库名
                    if (this.textBox_entityDbName.Text != "")
                    {
                        /*
                        if (this.textBox_entityDbName.Text == "")
                        {
                            strError = "尚未指定实体库名";
                            goto ERROR1;
                        }
                         * */

                        nRet = Global.CheckDbName(this.textBox_entityDbName.Text,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeDatabase, "entityDbName", this.textBox_entityDbName.Text);
                    }

                    // 订购库名
                    if (this.textBox_orderDbName.Text != "")
                    {
                        nRet = Global.CheckDbName(this.textBox_orderDbName.Text,
        out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeDatabase, "orderDbName", this.textBox_orderDbName.Text);
                    }

                    // 检查期库名的具备和usage是否矛盾
                    if (String.IsNullOrEmpty(strUsage) == true)
                        strUsage = "book";

                    if (strUsage == "book")
                    {
                        if (this.textBox_issueDbName.Text != "")
                        {
                            strError = "用途为book时，期库名必须为空";
                            goto ERROR1;
                        }
                    }
                    else if (strUsage == "series")
                    {
                        if (this.textBox_issueDbName.Text == "")
                        {
                            strError = "用途为series时，期库名必须具备";
                            goto ERROR1;
                        }
                    }

                    // 期库名
                    if (this.textBox_issueDbName.Text != "")
                    {
                        nRet = Global.CheckDbName(this.textBox_issueDbName.Text,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeDatabase, "issueDbName", this.textBox_issueDbName.Text);
                    }

                    // 评注库名
                    if (this.textBox_commentDbName.Text != "")
                    {
                        nRet = Global.CheckDbName(this.textBox_commentDbName.Text,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeDatabase, "commentDbName", this.textBox_commentDbName.Text);
                    }


                    // 为确认身份而登录
                    // return:
                    //      -1  出错
                    //      0   放弃登录
                    //      1   登录成功
                    nRet = this.ManagerForm.ConfirmLogin(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "创建数据库的操作被放弃";
                        MessageBox.Show(this, strError);

                    }
                    else
                    {
                        strDatabaseInfo = dom.OuterXml;

                        // 创建数据库
                        nRet = this.ManagerForm.CreateDatabase(
                            strDatabaseInfo,
                            this.Recreate,
                            out strOutputInfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }
                finally
                {
                    EnableControls(true);
                }
            }
            else
            {
                // 修改模式
                EnableControls(false);

                try
                {
                    string strDatabaseInfo = "";
                    string strOutputInfo = "";

                    // 修改的数据库名
                    List<string> change_dbnames = new List<string>();
                    // 删除的数据库名
                    List<string> delete_dbnames = new List<string>();
                    // 创建的数据库名
                    List<string> create_dbnames = new List<string>();

                    // 用于修改命令的DOM
                    XmlDocument change_dom = new XmlDocument();
                    change_dom.LoadXml("<root />");
                    XmlNode nodeChangeDatabase = change_dom.CreateElement("database");
                    change_dom.DocumentElement.AppendChild(nodeChangeDatabase);

                    /*
                    // 用于删除命令的DOM
                    XmlDocument delete_dom = new XmlDocument();
                    delete_dom.LoadXml("<root />");
                     * */

                    // 用于创建命令的DOM
                    XmlDocument create_dom = new XmlDocument();
                    create_dom.LoadXml("<root />");

                    // type
                    DomUtil.SetAttr(nodeChangeDatabase, "type", "biblio");

                    // syntax
                    if (this.comboBox_syntax.Text == "")
                    {
                        strError = "尚未指定数据格式";
                        goto ERROR1;
                    }
                    string strSyntax = GetPureValue(this.comboBox_syntax.Text);
                    DomUtil.SetAttr(nodeChangeDatabase, "syntax", strSyntax);

                    // usage

                    /*
                    if (this.comboBox_documentType.Text == "")
                    {
                        strError = "尚未指定文献类型";
                        goto ERROR1;
                    }
                    string strUsage = GetPureValue(this.comboBox_documentType.Text);
                     * */
                    DomUtil.SetAttr(nodeChangeDatabase, "usage", strUsage);


                    // 检查期库名的具备和usage是否矛盾
                    if (String.IsNullOrEmpty(strUsage) == true)
                        strUsage = "book";

                    if (strUsage == "book")
                    {
                        if (this.textBox_issueDbName.Text != "")
                        {
                            strError = "用途为book时，期库名必须为空";
                            goto ERROR1;
                        }
                    }
                    else if (strUsage == "series")
                    {
                        if (this.textBox_issueDbName.Text == "")
                        {
                            strError = "用途为series时，期库名必须具备";
                            goto ERROR1;
                        }
                    }

                    // 书目库名
                    string strOldBiblioDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "name");

                    if (String.IsNullOrEmpty(strOldBiblioDbName) == false
                        && this.textBox_biblioDbName.Text == "")
                    {
                        strError = "书目库名不能修改为空";
                        goto ERROR1;
                    }

                    bool bChanged = false;  // 是否有实质性修改命令。但不用于表示是否需要创建和删除数据库

                    if (strOldBiblioDbName != this.textBox_biblioDbName.Text)
                    {
                        nRet = Global.CheckDbName(this.textBox_biblioDbName.Text,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_biblioDbName.Text);
                        bChanged = true;
                        change_dbnames.Add(strOldBiblioDbName + " --> " + this.textBox_biblioDbName.Text);
                    }

                    bool bInCirculationChanged = false;

                    // 是否参与流通
                    string strOldInCirculation = DomUtil.GetAttr(this.dom.DocumentElement,
                        "inCirculation");
                    if (String.IsNullOrEmpty(strOldInCirculation) == true)
                        strOldInCirculation = "true";

                    bool bOldInCirculation = DomUtil.IsBooleanTrue(strOldInCirculation);
                    if (bOldInCirculation != this.checkBox_inCirculation.Checked)
                    {
                        // 当XML中具有inCirculation属性的时候，才表示要修改这个因素
                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_biblioDbName.Text);
                        DomUtil.SetAttr(nodeChangeDatabase, "inCirculation",
                            this.checkBox_inCirculation.Checked == true ? "true" : "false");
                        bChanged = true;
                        bInCirculationChanged = true;
                    }

                    bool bRoleChanged = false;

                    // 角色
                    string strOldRole = DomUtil.GetAttr(this.dom.DocumentElement,
                        "role");
                    if (strOldRole != strRole)
                    {
                        // 当XML中具有role属性的时候，才表示要修改这个因素
                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_biblioDbName.Text);
                        DomUtil.SetAttr(nodeChangeDatabase, "role",
                            strRole);
                        bChanged = true;
                        bRoleChanged = true;
                    }

                    bool bReplicationChanged = false;

                    // 角色
                    string strOldReplication = DomUtil.GetAttr(this.dom.DocumentElement,
                        "replication");
                    if (strOldReplication != strReplication)
                    {
                        // 当 XML 中具有 replication 属性的时候，才表示要修改这个因素
                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_biblioDbName.Text);
                        DomUtil.SetAttr(nodeChangeDatabase, "replication",
                            strReplication);
                        bChanged = true;
                        bReplicationChanged = true;
                    }

                    // 实体库名
                    string strOldEntityDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "entityDbName");
                    if (this.textBox_entityDbName.Text == "")
                    {
                        if (String.IsNullOrEmpty(strOldEntityDbName) == false)
                        {
                            // 实体库名从有内容修改为空，表示要删除实体库
                            /*
                            XmlNode nodeDeleteDatabase = delete_dom.CreateElement("database");
                            delete_dom.DocumentElement.AppendChild(nodeDeleteDatabase);

                            DomUtil.SetAttr(nodeDeleteDatabase, "name", strOldEntityDbName);
                             * */
                            delete_dbnames.Add(strOldEntityDbName);
                        }
                    }
                    else if (strOldEntityDbName != this.textBox_entityDbName.Text)
                    {

                        if (String.IsNullOrEmpty(strOldEntityDbName) == true)
                        {
                            // 实体库名从空变为有值，表示要创建实体库
                            XmlNode nodeCreateDatabase = create_dom.CreateElement("database");
                            create_dom.DocumentElement.AppendChild(nodeCreateDatabase);

                            DomUtil.SetAttr(nodeCreateDatabase, "name", this.textBox_entityDbName.Text);
                            DomUtil.SetAttr(nodeCreateDatabase, "type", "entity");
                            DomUtil.SetAttr(nodeCreateDatabase, "biblioDbName", this.textBox_biblioDbName.Text);
                            create_dbnames.Add(this.textBox_entityDbName.Text);
                        }
                        else
                        {
                            nRet = Global.CheckDbName(this.textBox_entityDbName.Text,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            DomUtil.SetAttr(nodeChangeDatabase, "entityDbName", this.textBox_entityDbName.Text);
                            bChanged = true;
                            change_dbnames.Add(strOldEntityDbName + " --> " + this.textBox_entityDbName.Text);
                        }
                    }

                    // 订购库名
                    string strOldOrderDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "orderDbName");
                    if (this.textBox_orderDbName.Text == "")
                    {
                        if (String.IsNullOrEmpty(strOldOrderDbName) == false)
                        {
                            // 订购名从有内容修改为空，表示要删除订购库
                            /*
                            XmlNode nodeDeleteDatabase = delete_dom.CreateElement("database");
                            delete_dom.DocumentElement.AppendChild(nodeDeleteDatabase);

                            DomUtil.SetAttr(nodeDeleteDatabase, "name", strOldOrderDbName);
                             * */
                            delete_dbnames.Add(strOldOrderDbName);
                        }
                    }
                    else if (strOldOrderDbName != this.textBox_orderDbName.Text)
                    {
                        if (String.IsNullOrEmpty(strOldOrderDbName) == true)
                        {
                            // 订购库名从空变为有值，表示要创建订购库
                            XmlNode nodeCreateDatabase = create_dom.CreateElement("database");
                            create_dom.DocumentElement.AppendChild(nodeCreateDatabase);

                            DomUtil.SetAttr(nodeCreateDatabase, "name", this.textBox_orderDbName.Text);
                            DomUtil.SetAttr(nodeCreateDatabase, "type", "order");
                            DomUtil.SetAttr(nodeCreateDatabase, "biblioDbName", this.textBox_biblioDbName.Text);
                            create_dbnames.Add(this.textBox_orderDbName.Text);
                        }
                        else
                        {
                            nRet = Global.CheckDbName(this.textBox_orderDbName.Text,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            DomUtil.SetAttr(nodeChangeDatabase, "orderDbName", this.textBox_orderDbName.Text);
                            bChanged = true;
                            change_dbnames.Add(strOldOrderDbName + " --> " + this.textBox_orderDbName.Text);
                        }
                    }

                    // 期库名
                    string strOldIssueDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "issueDbName");
                    if (this.textBox_issueDbName.Text == "")
                    {
                        if (String.IsNullOrEmpty(strOldIssueDbName) == false)
                        {
                            // 期库名从有内容修改为空，表示要删除期库
                            /*
                            XmlNode nodeDeleteDatabase = delete_dom.CreateElement("database");
                            delete_dom.DocumentElement.AppendChild(nodeDeleteDatabase);

                            DomUtil.SetAttr(nodeDeleteDatabase, "name", strOldIssueDbName);
                             * */
                            delete_dbnames.Add(strOldIssueDbName);
                        }
                    }
                    else if (strOldIssueDbName != this.textBox_issueDbName.Text)
                    {
                        if (String.IsNullOrEmpty(strOldIssueDbName) == true)
                        {
                            // 期库名从空变为有值，表示要创建期库
                            XmlNode nodeCreateDatabase = create_dom.CreateElement("database");
                            create_dom.DocumentElement.AppendChild(nodeCreateDatabase);

                            DomUtil.SetAttr(nodeCreateDatabase, "name", this.textBox_issueDbName.Text);
                            DomUtil.SetAttr(nodeCreateDatabase, "type", "issue");
                            DomUtil.SetAttr(nodeCreateDatabase, "biblioDbName", this.textBox_biblioDbName.Text);
                            create_dbnames.Add(this.textBox_issueDbName.Text);
                        }
                        else
                        {
                            nRet = Global.CheckDbName(this.textBox_issueDbName.Text,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            DomUtil.SetAttr(nodeChangeDatabase, "issueDbName", this.textBox_issueDbName.Text);
                            bChanged = true;
                            change_dbnames.Add(strOldIssueDbName + " --> " + this.textBox_issueDbName.Text);
                        }
                    }


                    // 评注库名
                    string strOldCommentDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "commentDbName");
                    if (this.textBox_commentDbName.Text == "")
                    {
                        if (String.IsNullOrEmpty(strOldCommentDbName) == false)
                        {
                            delete_dbnames.Add(strOldCommentDbName);
                        }
                    }
                    else if (strOldCommentDbName != this.textBox_commentDbName.Text)
                    {
                        if (String.IsNullOrEmpty(strOldCommentDbName) == true)
                        {
                            // 评注库名从空变为有值，表示要创建期库
                            XmlNode nodeCreateDatabase = create_dom.CreateElement("database");
                            create_dom.DocumentElement.AppendChild(nodeCreateDatabase);

                            DomUtil.SetAttr(nodeCreateDatabase, "name", this.textBox_commentDbName.Text);
                            DomUtil.SetAttr(nodeCreateDatabase, "type", "comment");
                            DomUtil.SetAttr(nodeCreateDatabase, "biblioDbName", this.textBox_biblioDbName.Text);
                            create_dbnames.Add(this.textBox_commentDbName.Text);
                        }
                        else
                        {
                            nRet = Global.CheckDbName(this.textBox_commentDbName.Text,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            DomUtil.SetAttr(nodeChangeDatabase, "commentDbName", this.textBox_commentDbName.Text);
                            bChanged = true;
                            change_dbnames.Add(strOldCommentDbName + " --> " + this.textBox_commentDbName.Text);
                        }
                    }

                    // 提示修改的数据库名，要删除的数据库，要创建的数据库
                    string strText = "";
                    if (change_dbnames.Count > 0)
                    {
                        strText += "要进行下列数据库名修改:\r\n---\r\n";
                        strText += MakeListString(change_dbnames);
                        strText += "\r\n";
                    }

                    if (delete_dbnames.Count > 0)
                    {
                        strText += "要删除下列数据库:\r\n---\r\n";
                        strText += MakeListString(delete_dbnames);
                        strText += "警告: 数据库被删除后，其中的数据再也无法复原！\r\n";
                        strText += "\r\n";
                    }

                    if (create_dbnames.Count > 0)
                    {
                        strText += "要创建下列数据库:\r\n---\r\n";
                        strText += MakeListString(create_dbnames);
                        strText += "\r\n";
                    }

                    if (bInCirculationChanged == true)
                    {
                        strText += "\r\n书目库 '是否参与流通' 状态发生了修改，变为:\r\n---\r\n";
                        strText += this.checkBox_inCirculation.Checked == true ? "要参与流通" : "不参与流通";
                        strText += "\r\n";
                    }

                    if (bRoleChanged == true)
                    {
                        strText += "\r\n书目库 '角色' 发生了修改，变为:\r\n---\r\n";
                        strText += strRole;
                        strText += "\r\n";
                    }

                    if (bReplicationChanged == true)
                    {
                        strText += "\r\n书目库 '复制参数' 发生了修改，变为:\r\n---\r\n";
                        strText += strReplication;
                        strText += "\r\n";
                    }

                    if (bChanged == false && string.IsNullOrEmpty(strText) == true)
                    {
                        Debug.Assert(string.IsNullOrEmpty(strText) == true, "");

#if DEBUG
                        XmlNodeList nodes = create_dom.DocumentElement.SelectNodes("database");
                        Debug.Assert(nodes.Count == 0, "");
#endif

                        // 2013/1/27
                        // 要测试，发生修改的情况下(还有新创建数据库的情况下)，OK按钮按下后，不应静悄悄退出 
                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                        return;
                    }

                    // 对话框警告
                    DialogResult result = MessageBox.Show(this,
                        strText + "\r\n确实要继续?",
                        "BiblioDatabaseDialog",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                        return;

                    if (bChanged == true)
                    {
                        strDatabaseInfo = change_dom.OuterXml;

                        // 修改数据库
                        nRet = this.ManagerForm.ChangeDatabase(
                            strOldBiblioDbName,
                            strDatabaseInfo,
                            out strOutputInfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    // 删除数据库
                    /*
                    XmlNodeList nodes = delete_dom.DocumentElement.SelectNodes("database");
                    if (nodes.Count > 0)
                    {
                        strDatabaseInfo = delete_dom.OuterXml;
                     * */
                    bool bConfirmed = false;
                    if (delete_dbnames.Count > 0)
                    {
                        // 为确认身份而登录
                        // return:
                        //      -1  出错
                        //      0   放弃登录
                        //      1   登录成功
                        nRet = this.ManagerForm.ConfirmLogin(out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            strError = "刷新数据库的操作被放弃";
                            MessageBox.Show(this, strError);
                        }
                        else
                        {
                            bConfirmed = true;
                            // 删除数据库
                            nRet = this.ManagerForm.DeleteDatabase(
                                MakeListString(delete_dbnames),
                                out strOutputInfo,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                    }

                    // 创建数据库
                    {
                        XmlNodeList nodes = create_dom.DocumentElement.SelectNodes("database");
                        if (nodes.Count > 0)
                        {
                            if (bConfirmed == false)
                            {
                                // 为确认身份而登录
                                // return:
                                //      -1  出错
                                //      0   放弃登录
                                //      1   登录成功
                                nRet = this.ManagerForm.ConfirmLogin(out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                if (nRet == 0)
                                {
                                    strError = "创建数据库的操作被放弃";
                                    MessageBox.Show(this, strError);
                                    goto END1;
                                }
                            }

                            strDatabaseInfo = create_dom.OuterXml;

                            // 创建数据库
                            nRet = this.ManagerForm.CreateDatabase(
                                strDatabaseInfo,
                                false,
                                out strOutputInfo,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                        }
                    }
                }
                finally
                {
                    EnableControls(true);
                }

            }

        END1:
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 书目库名
        public string BiblioDatabaseName
        {
            get
            {
                return this.textBox_biblioDbName.Text;
            }
            set
            {
                this.textBox_biblioDbName.Text = value;
            }
        }

        public bool InCirculation
        {
            get
            {
                return this.checkBox_inCirculation.Checked;
            }
            set
            {
                this.checkBox_inCirculation.Checked = value;
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.textBox_biblioDbName.Enabled = bEnable;
            this.textBox_entityDbName.Enabled = bEnable;
            this.textBox_issueDbName.Enabled = bEnable;
            this.textBox_orderDbName.Enabled = bEnable;
            this.textBox_commentDbName.Enabled = bEnable;

            this.checkBox_inCirculation.Enabled = bEnable;

            if (this.CreateMode == true)
            {
                this.comboBox_syntax.Enabled = bEnable;
                this.comboBox_documentType.Enabled = bEnable;
            }
            else
            {
                this.comboBox_syntax.Enabled = false;
                this.comboBox_documentType.Enabled = false;
            }

            this.checkedComboBox_role.Enabled = bEnable;

            this.comboBox_replication_centerServer.Enabled = bEnable;
            this.comboBox_replication_dbName.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
        }

        private void checkedComboBox_role_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_role.Items.Count != 0)
                return;

            this.checkedComboBox_role.Items.Add("orderWork\t采购工作库");
            this.checkedComboBox_role.Items.Add("orderRecommendStore\t荐购存储库");
            this.checkedComboBox_role.Items.Add("biblioSource\t外源书目库");
            this.checkedComboBox_role.Items.Add("catalogWork\t编目工作库");
            this.checkedComboBox_role.Items.Add("catalogTarget\t编目中央库");
        }

        private void comboBox_replication_centerServer_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_replication_centerServer.Items.Count > 0)
                return;

            List<string> server_names = this.ManagerForm.GetCenterServerNames();
            foreach (string s in server_names)
            {
                this.comboBox_replication_centerServer.Items.Add(s);
            }

        }

        int m_nInDropDown = 0;

        private void comboBox_replication_dbName_DropDown(object sender, EventArgs e)
        {
            if (m_nInDropDown > 0 || this.comboBox_replication_dbName.Items.Count > 0)
                return;

            string strError = "";
            m_nInDropDown++;
            try
            {
                if (string.IsNullOrEmpty(this.comboBox_replication_centerServer.Text) == true)
                {
                    strError = "请先选定中心服务器";
                    goto ERROR1;
                }

                List<string> dbnames = null;
                int nRet = this.ManagerForm.GetRemoteBiblioDbNames(
                    this.comboBox_replication_centerServer.Text,
                    out dbnames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                foreach (string s in dbnames)
                {
                    this.comboBox_replication_dbName.Items.Add(s);
                }
            }
            finally
            {
                m_nInDropDown--;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}