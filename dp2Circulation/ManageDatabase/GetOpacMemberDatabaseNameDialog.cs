using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    internal partial class GetOpacMemberDatabaseNameDialog : Form
    {
        // 表示当前全部数据库信息的XML字符串
        public string AllDatabaseInfoXml = "";

        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        // 需要排除的若干数据库名
        public List<string> ExcludingDbNames = null;

        public GetOpacMemberDatabaseNameDialog()
        {
            InitializeComponent();

            this.listView_databases.LargeImageList = this.imageList_databaseType;
            this.listView_databases.SmallImageList = this.imageList_databaseType;
        }

        private void GetOpacMemberDatabaseNameDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = ListAllDatabases(this.AllDatabaseInfoXml,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            // 选中第一个
            if (this.listView_databases.Items.Count > 0)
            {
                if (this.SelectedDatabaseName != "")
                {
                    ListViewItem item = ListViewUtil.FindItem(this.listView_databases,
                        this.SelectedDatabaseName,
                        0);
                    if (item != null)
                        item.Selected = true;
                }
                else
                {
                    // 选中第一个不是灰色的item
                    for (int i = 0; i < this.listView_databases.Items.Count; i++)
                    {
                        ListViewItem item = this.listView_databases.Items[i];
                        if (item.ImageIndex == 0)
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                }
            }
        }

        // 在listview中列出所有数据库
        int ListAllDatabases(string strAllDatbaseInfo,
            out string strError)
        {
            strError = "";

            this.listView_databases.Items.Clear();

            if (String.IsNullOrEmpty(strAllDatbaseInfo) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strAllDatbaseInfo);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");



                string strTypeName = "";
                if (this.ManagerForm != null)
                {
                    strTypeName = this.ManagerForm.GetTypeName(strType);
                    if (strTypeName == null)
                        strTypeName = strType;
                }
                else
                    strTypeName = strType;

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strTypeName);
                item.Tag = node.OuterXml;   // 记载XML定义片断

                this.listView_databases.Items.Add(item);

                if (this.ExcludingDbNames != null
    && this.ExcludingDbNames.Count > 0)
                {
                    // 如果为要排除的数据库名，则颜色发灰
                    if (this.ExcludingDbNames.IndexOf(strName) != -1)
                    {
                        item.ForeColor = SystemColors.GrayText; // 灰色
                        item.ImageIndex = 1;    // 表示要排除
                    }
                }
            }

            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_databaseName.Text == "")
            {
                MessageBox.Show(this, "尚未指定数据库名");
                return;
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listView_databases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_databases.SelectedItems.Count > 0)
            {
                List<string> dbnames = new List<string>();
                foreach (ListViewItem item in this.listView_databases.SelectedItems)
                {
                    if (item.ImageIndex == 0)
                        dbnames.Add(item.Text);
                }
                this.textBox_databaseName.Text = StringUtil.MakePathList(dbnames);
            }
            else
                this.textBox_databaseName.Text = "";
        }

        public string SelectedDatabaseName
        {
            get
            {
                return this.textBox_databaseName.Text;
            }
            set
            {
                this.textBox_databaseName.Text = value;
            }
        }

        private void listView_databases_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(this, null);
        }
    }
}