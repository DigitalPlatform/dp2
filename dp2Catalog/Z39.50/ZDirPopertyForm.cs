using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Xml;

using DigitalPlatform.Xml;

namespace dp2Catalog
{
    /// <summary>
    /// Z39.50检索目标树中的目录属性对话框
    /// </summary>
    public partial class ZDirPopertyForm : Form
    {
        public XmlNode XmlNode = null;

        public ZDirPopertyForm()
        {
            InitializeComponent();
        }

        private void ZDirPopertyForm_Load(object sender, EventArgs e)
        {
            this.textBox_dirName.Text = DomUtil.GetAttr(this.XmlNode,
                "name");
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_dirName.Text == "")
            {
                MessageBox.Show(this, "尚未指定目录名");
                return;
            }

            DomUtil.SetAttr(this.XmlNode,
                "name", this.textBox_dirName.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string DirName
        {
            get
            {
                return this.textBox_dirName.Text;
            }
        }

    }
}