using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 编辑内核配置文件的对话框
    /// </summary>
    public partial class KernelCfgFileDialog : Form
    {
        public bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;

                this.button_OK.Enabled = value;
            }
        }

        bool _changed = false;

        public KernelCfgFileDialog()
        {
            InitializeComponent();
        }

        private void KernelCfgFileDialog_Load(object sender, EventArgs e)
        {
            this.Changed = false;

        }

        private void KernelCfgFileDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前对话框有信息被修改。若关闭窗口或者取消修改，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "KernelCfgFileDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void KernelCfgFileDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.Changed = false;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string Content
        {
            get
            {
                return this.textBox_content.Text;
            }
            set
            {
                this.textBox_content.Text = value;
            }
        }

        public string ActivePage
        {
            get
            {
                if (this.tabControl_main.SelectedTab == this.tabPage_content)
                    return "content";
                if (this.tabControl_main.SelectedTab == this.tabPage_property)
                    return "property";
                return "";
            }
            set
            {
                if (value == "content")
                    this.tabControl_main.SelectedTab = this.tabPage_content;
                else if (value == "property")
                    this.tabControl_main.SelectedTab = this.tabPage_property;
                else
                    throw new Exception("Invalid page name '" + value + "'");
            }
        }

        public string Path
        {
            get
            {
                return this.textBox_path.Text;
            }
            set
            {
                this.textBox_path.Text = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.textBox_serverUrl.Text;
            }
            set
            {
                this.textBox_serverUrl.Text = value;
            }
        }

        public string MIME
        {
            get
            {
                return this.textBox_mime.Text;
            }
            set
            {
                this.textBox_mime.Text = value;
            }
        }

        private void textBox_content_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }

        private void toolStripButton_formatXml_Click(object sender, EventArgs e)
        {
            string strOutXml = "";
            string strError = "";
            int nRet = DomUtil.GetIndentXml(this.textBox_content.Text, out strOutXml, out strError);
            if (nRet == -1)
                MessageBox.Show(strError);
            else
                this.textBox_content.Text = strOutXml;
        }
    }
}
