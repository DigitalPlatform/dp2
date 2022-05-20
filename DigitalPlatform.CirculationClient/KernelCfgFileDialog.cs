using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform.Xml;
using Markdig;

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

        List<Control> _freeControls = new List<Control>();

        void DisposeFreeControls()
        {
            ControlExtention.DisposeFreeControls(_freeControls);
        }

        private void KernelCfgFileDialog_Load(object sender, EventArgs e)
        {
            this.Changed = false;

            if (IsMarkDownPath(this.Path) == false)
            {
                this.tabControl_main.TabPages.Remove(this.tabPage_preview);
                ControlExtention.AddFreeControl(_freeControls, this.tabPage_preview);
            }

            // 2022/5/20
            tabControl_main_SelectedIndexChanged(sender, e);

            this.BeginInvoke(new Action(FocusEdit));
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
                if (this.tabControl_main.SelectedTab == this.tabPage_preview)
                    return "preview";
                return "";
            }
            set
            {
                if (value == "content")
                    this.tabControl_main.SelectedTab = this.tabPage_content;
                else if (value == "property")
                    this.tabControl_main.SelectedTab = this.tabPage_property;
                else if (value == "preview")
                    this.tabControl_main.SelectedTab = this.tabPage_preview;
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

        long _textVersion = 0;
        long _previewVersion = 0;

        private void textBox_content_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;

            _textVersion++;
        }

        static string GetExtension(string path)
        {
            if (path == null)
                return "";
            int ret = path.LastIndexOf(".");
            if (ret != -1)
                return path.Substring(ret);
            return "";
        }

        public static bool IsMarkDownPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            var ext = GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                return false;
            if (ext.ToLower() == ".md")
                return true;
            return false;
        }

        void RefreshMarkDownPreview()
        {
            string text = this.textBox_content.Text;
            // Configure the pipeline with all advanced extensions active
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(text, pipeline);
            this.webBrowser1.DocumentText = $"<html><head></head><body>{html}</body></html>";

            _previewVersion = _textVersion;
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

        void FocusEdit()
        {
            this.textBox_content.Select(0, 0);
            this.textBox_content.Focus();
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_preview)
            {
                if (_previewVersion != _textVersion)
                {
                    if (IsMarkDownPath(this.Path))
                        RefreshMarkDownPreview();
                }
            }
        }
    }
}
