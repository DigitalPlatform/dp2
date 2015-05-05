using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.DTLP
{
    /// <summary>
    /// 获得一个方案名
    /// </summary>
    public partial class GetProjectNameDialog : Form
    {
        public XmlDocument dom = null;


        public GetProjectNameDialog()
        {
            InitializeComponent();
        }


        private void GetProjectNameDialog_Load(object sender, EventArgs e)
        {
            FillProjectNameList(this.ProjectName);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_projectName.Text == "")
            {
                MessageBox.Show(this, "尚未指定查重方案名");
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


        void FillProjectNameList(string strSelectProjectName)
        {
            this.listView_projects.Items.Clear();

            if (this.dom == null)
                return;

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strComment = DomUtil.GetAttr(node, "comment");

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strComment);
                this.listView_projects.Items.Add(item);

                if (strSelectProjectName == strName)
                    item.Selected = true;
            }
        }

        private void listView_projects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_projects.SelectedItems.Count == 0)
                this.textBox_projectName.Text = "";
            else
                this.textBox_projectName.Text = this.listView_projects.SelectedItems[0].Text;
        }

        public string ProjectName
        {
            get
            {
                return this.textBox_projectName.Text;
            }
            set
            {
                this.textBox_projectName.Text = value;
            }
        }
    }
}