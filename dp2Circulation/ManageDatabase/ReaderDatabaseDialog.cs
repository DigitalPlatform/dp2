using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    internal partial class ReaderDatabaseDialog : Form
    {
        int m_nInInitial = 0;

        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        public bool CreateMode = false; // 是否为创建模式？==true为创建模式；==false为修改模式
        public bool Recreate = false;   // 是否为重新创建模式？当CreateMode == true 时起作用
        XmlDocument dom = null;

        /// <summary>
        /// 图书馆代码列表字符串。提供给 combobox 使用
        /// </summary>
        public string LibraryCodeList
        {
            get
            {
                StringBuilder text = new StringBuilder();
                foreach (string s in this.comboBox_libraryCode.Items)
                {
                    if (text.Length > 0)
                        text.Append(",");
                    text.Append(s);
                }
                return text.ToString();
            }
            set
            {
                List<string> values = StringUtil.SplitList(value);
                this.comboBox_libraryCode.Items.Clear();
                foreach (string s in values)
                {
                    this.comboBox_libraryCode.Items.Add(s);
                }
            }
        }


        public ReaderDatabaseDialog()
        {
            InitializeComponent();
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
            if (strType != "reader")
            {
                strError = "<database>元素的type属性值('" + strType + "')应当为reader";
                return -1;
            }

            this.m_nInInitial++;    // 避免xxxchanged响应

            try
            {

                this.textBox_readerDbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "name");

                string strInCirculation = DomUtil.GetAttr(dom.DocumentElement,
                    "inCirculation");
                if (String.IsNullOrEmpty(strInCirculation) == true)
                    strInCirculation = "true";

                this.comboBox_libraryCode.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "libraryCode");

                this.checkBox_inCirculation.Checked = DomUtil.IsBooleanTrue(strInCirculation);

            }
            finally
            {
                this.m_nInInitial--;
            }

            return 0;
        }


        private void ReaderDatabaseDialog_Load(object sender, EventArgs e)
        {
            // 如果只有一项列表事项，而当前为空白，则自动设置好这一项
            if (this.CreateMode == true
                && string.IsNullOrEmpty(this.comboBox_libraryCode.Text) == true
                && this.comboBox_libraryCode.Items.Count > 0)
                this.comboBox_libraryCode.Text = (string)this.comboBox_libraryCode.Items[0];
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

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
                    DomUtil.SetAttr(nodeDatabase, "type", "reader");

                    // 是否参与流通？
                    string strInCirculation = "true";
                    if (this.checkBox_inCirculation.Checked == true)
                        strInCirculation = "true";
                    else
                        strInCirculation = "false";

                    DomUtil.SetAttr(nodeDatabase, "inCirculation", strInCirculation);

                    // 书目库名
                    if (this.textBox_readerDbName.Text == "")
                    {
                        strError = "尚未指定读者库名";
                        goto ERROR1;
                    }
                    nRet = Global.CheckDbName(this.textBox_readerDbName.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    DomUtil.SetAttr(nodeDatabase, "name",
                        this.textBox_readerDbName.Text);

                    // 2012/9/7
                    DomUtil.SetAttr(nodeDatabase, "libraryCode",
    this.comboBox_libraryCode.Text);


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

                    // 修改的数据库名
                    List<string> change_dbnames = new List<string>();

                    // 用于修改命令的DOM
                    XmlDocument change_dom = new XmlDocument();
                    change_dom.LoadXml("<root />");
                    XmlNode nodeChangeDatabase = change_dom.CreateElement("database");
                    change_dom.DocumentElement.AppendChild(nodeChangeDatabase);

                    // type
                    DomUtil.SetAttr(nodeChangeDatabase, "type", "reader");


                    // 书目库名
                    string strOldReaderDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "name");

                    if (String.IsNullOrEmpty(strOldReaderDbName) == false
                        && this.textBox_readerDbName.Text == "")
                    {
                        strError = "读者库名不能修改为空";
                        goto ERROR1;
                    }

                    bool bChanged = false;  // 是否有实质性修改命令

                    if (strOldReaderDbName != this.textBox_readerDbName.Text)
                    {
                        nRet = Global.CheckDbName(this.textBox_readerDbName.Text,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_readerDbName.Text);
                        bChanged = true;
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
                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_readerDbName.Text);
                        DomUtil.SetAttr(nodeChangeDatabase, "inCirculation",
                            this.checkBox_inCirculation.Checked == true ? "true" : "false");
                        bInCirculationChanged = true;
                    }

                    bool bLibraryCodeChanged = false;

                    // 是否参与流通
                    string strOldLibraryCode = DomUtil.GetAttr(this.dom.DocumentElement,
                        "libraryCode");
                    if (strOldLibraryCode != this.comboBox_libraryCode.Text)
                    {
                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_readerDbName.Text);
                        DomUtil.SetAttr(nodeChangeDatabase, "libraryCode",
                            this.comboBox_libraryCode.Text);
                        bLibraryCodeChanged = true;
                    }

                    if (bChanged == false && bInCirculationChanged == false && bLibraryCodeChanged == false)
                        goto END1;

                    // 提示修改的数据库名，要删除的数据库，要创建的数据库
                    string strText = "";

                    if (bChanged == true)
                        strText += "要将数据库名 '" + strOldReaderDbName + "' 修改为 '" + this.textBox_readerDbName.Text + "'";

                    if (bInCirculationChanged == true)
                    {
                        if (strText != "")
                            strText += "；并";
                        else
                            strText += "要";
                        strText += "将 '是否参与流通' 状态 修改为"
                            + (this.checkBox_inCirculation.Checked == true ? "'要参与'" : "'不参与'");
                    }

                    if (bLibraryCodeChanged == true)
                    {
                        if (strText != "")
                            strText += "；并";
                        else
                            strText += "要";
                        strText += "将 图书馆代码 修改为 '"
                            + this.comboBox_libraryCode.Text + "'";
                    }
                        
                    strText += "。\r\n\r\n确实要继续?";

                    // 对话框警告
                    DialogResult result = MessageBox.Show(this,
                        strText,
                        "ReaderDatabaseDialog",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                        return;

                    string strDatabaseInfo = "";
                    string strOutputInfo = "";

                    strDatabaseInfo = change_dom.OuterXml;

                    // 修改数据库
                    nRet = this.ManagerForm.ChangeDatabase(
                        strOldReaderDbName,
                        strDatabaseInfo,
                        out strOutputInfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

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

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.textBox_readerDbName.Enabled = bEnable;
            this.comboBox_libraryCode.Enabled = bEnable;
            this.checkBox_inCirculation.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
        }

        public string ReaderDatabaseName
        {
            get
            {
                return this.textBox_readerDbName.Text;
            }
            set
            {
                this.textBox_readerDbName.Text = value;
            }
        }

        public string LibraryCode
        {
            get
            {
                return this.comboBox_libraryCode.Text;
            }
            set
            {
                this.comboBox_libraryCode.Text = value;
            }
        }
    }
}