using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;

namespace dp2Circulation
{
    internal partial class SimpleDatabaseDialog : Form
    {
        public string DatabaseType = "";

        int m_nInInitial = 0;
        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        public bool CreateMode = false; // 是否为创建模式？==true为创建模式；==false为修改模式
        public bool Recreate = false;   // 是否为重新创建模式？当CreateMode == true 时起作用

        XmlDocument dom = null;

        public SimpleDatabaseDialog()
        {
            InitializeComponent();
        }

        public int Initial(
            string strDatabaseType,
            string strXml,
out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strDatabaseType) == true)
            {
                strError = "strDatabaseType参数值不能为空";
                return -1;
            }

            this.DatabaseType = strDatabaseType;

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
            if (strType != this.DatabaseType)
            {
                strError = "<database>元素的type属性值('" + strType + "')应当为 '"+this.DatabaseType+"'";
                return -1;
            }

            this.m_nInInitial++;    // 避免xxxchanged响应

            try
            {

                this.textBox_dbName.Text = DomUtil.GetAttr(dom.DocumentElement,
                    "name");
            }
            finally
            {
                this.m_nInInitial--;
            }

            return 0;
        }

        private void SimpleDatabaseDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            if (this.CreateMode == true)
            {
                if (String.IsNullOrEmpty(this.DatabaseType) == true)
                {
                    strError = "尚未指定DatabaseType参数";
                    goto ERROR1;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
                    DomUtil.SetAttr(nodeDatabase, "type", this.DatabaseType);

                    // 库名
                    if (this.textBox_dbName.Text == "")
                    {
                        strError = "尚未指定库名";
                        goto ERROR1;
                    }
                    nRet = Global.CheckDbName(this.textBox_dbName.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    DomUtil.SetAttr(nodeDatabase, "name",
                        this.textBox_dbName.Text);

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
                    string strDatabaseInfo = "";
                    string strOutputInfo = "";

                    // 修改的数据库名
                    List<string> change_dbnames = new List<string>();

                    // 用于修改命令的DOM
                    XmlDocument change_dom = new XmlDocument();
                    change_dom.LoadXml("<root />");
                    XmlNode nodeChangeDatabase = change_dom.CreateElement("database");
                    change_dom.DocumentElement.AppendChild(nodeChangeDatabase);

                    // type
                    DomUtil.SetAttr(nodeChangeDatabase, "type", this.DatabaseType);


                    // 库名
                    string strOldReaderDbName = DomUtil.GetAttr(this.dom.DocumentElement,
                        "name");

                    if (String.IsNullOrEmpty(strOldReaderDbName) == false
                        && this.textBox_dbName.Text == "")
                    {
                        strError = "库名不能修改为空";
                        goto ERROR1;
                    }

                    bool bChanged = false;  // 是否有实质性修改命令

                    if (strOldReaderDbName != this.textBox_dbName.Text)
                    {
                        nRet = Global.CheckDbName(this.textBox_dbName.Text,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DomUtil.SetAttr(nodeChangeDatabase, "name", this.textBox_dbName.Text);
                        bChanged = true;
                    }

                    // 提示修改的数据库名，要删除的数据库，要创建的数据库
                    string strText = "要将数据库名 " + strOldReaderDbName + " 修改为 " + this.textBox_dbName.Text + ", 确实要继续?";

                    // 对话框警告
                    DialogResult result = MessageBox.Show(this,
                        strText,
                        "SimpleDatabaseDialog",
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
                            strOldReaderDbName,
                            strDatabaseInfo,
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
            this.textBox_dbName.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
        }

        public string DatabaseName
        {
            get
            {
                return this.textBox_dbName.Text;
            }
            set
            {
                this.textBox_dbName.Text = value;
            }
        }

        public bool DatabaseNameReadOnly
        {
            get
            {
                return this.textBox_dbName.ReadOnly;
            }
            set
            {
                this.textBox_dbName.ReadOnly = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
                if (String.IsNullOrEmpty(value) == false)
                    this.textBox_comment.Visible = true;
                else
                    this.textBox_comment.Visible = false;
            }
        }
    }
}