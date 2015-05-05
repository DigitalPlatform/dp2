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

namespace dp2Catalog
{
    /// <summary>
    /// OAI服务器属性对话框
    /// </summary>
    public partial class OaiServerPropertyForm : Form
    {
        public XmlNode XmlNode = null;  // 服务器XML节点


        public OaiServerPropertyForm()
        {
            InitializeComponent();
        }

        private void OaiServerPropertyForm_Load(object sender, EventArgs e)
        {
            this.textBox_serverName.Text = DomUtil.GetAttr(this.XmlNode,
                "name");
            this.textBox_baseUrl.Text = DomUtil.GetAttr(this.XmlNode,
                "baseUrl");
            this.textBox_homepage.Text = DomUtil.GetAttr(this.XmlNode,
                "homepage");

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_serverName.Text == "")
            {
                strError = "尚未指定服务器名";
                goto ERROR1;
            }
            if (this.textBox_baseUrl.Text == "")
            {
                strError = "尚未指定服务器基地址";
                goto ERROR1;
            }


            DomUtil.SetAttr(this.XmlNode,
                "name", this.textBox_serverName.Text);
            DomUtil.SetAttr(this.XmlNode,
                "baseUrl", this.textBox_baseUrl.Text);
            DomUtil.SetAttr(this.XmlNode,
                "homepage", this.textBox_homepage.Text);

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

        public string ServerName
        {
            get
            {
                return this.textBox_serverName.Text;
            }
        }
    }
}