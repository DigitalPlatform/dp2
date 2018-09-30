using System;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 标签设计窗口
    /// 定义标签的尺寸、格式
    /// </summary>
    public partial class LabelDesignForm : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public LabelDesignForm()
        {
            InitializeComponent();

            this.labelDefControl1.DecimalPlaces = 4;
        }

        Encoding _fileEncoding = Encoding.UTF8;

        private void LabelDesignForm_Load(object sender, EventArgs e)
        {
            LoadLabelDefFile();

            // 装载缺省设置
            if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
            {
                this.labelDefControl1.SetDefaultValue(string.IsNullOrEmpty(this.SampleLabelText) == true);
                this.labelDefControl1.Synchronize();
                this.labelDefControl1.Changed = false;
            }
        }

        void LoadLabelDefFile()
        {
            if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == false)
            {
                string strError = "";
                string strContent = "";

                Encoding encoding = null;
                // 能自动识别文件内容的编码方式的读入文本文件内容模块
                // return:
                //      -1  出错
                //      0   文件不存在
                //      1   文件存在
                int nRet = FileUtil.ReadTextFileContent(this.textBox_labelDefFilename.Text,
                    -1,
                    out strContent,
                    out encoding,
                    out strError);
                if (nRet == 1)
                {
                    this._fileEncoding = encoding;
                    this.labelDefControl1.Xml = strContent;
                }
                else
                    MessageBox.Show(this, strError);
            }

            if (string.IsNullOrEmpty(this.SampleLabelText) == true)
            {
                this.SampleLabelText = LabelDefControl.BuildSampleLabelText();
            }

            this.labelDefControl1.Synchronize();
            this.labelDefControl1.Changed = false;
        }

        private void LabelDesignForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.labelDefControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "LabelDesignForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void LabelDesignForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.labelDefControl1.Synchronize();

            // 保存回原来的文件
            if (this.labelDefControl1.Changed == true)
            {
                if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
                {
                    // 询问文件名
                    SaveFileDialog dlg = new SaveFileDialog();

                    dlg.Title = "请指定要保存的标签定义文件名";
                    dlg.CreatePrompt = false;
                    dlg.OverwritePrompt = true;
                    dlg.FileName = this.textBox_labelDefFilename.Text;
                    // dlg.InitialDirectory = Environment.CurrentDirectory;
                    dlg.Filter = "标签定义文件 (*.xml)|*.xml|All files (*.*)|*.*";

                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    this.textBox_labelDefFilename.Text = dlg.FileName;
                }

                if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == false)
                    SaveFile();
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        // 保存文件
        void SaveFile()
        {
            if (this.labelDefControl1.Changed == false)
                return;

            if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
                return;

            string strError = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.labelDefControl1.Xml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 时出错: " + ex.Message;
                goto ERROR1;
            }

            using (XmlTextWriter w = new XmlTextWriter(this.textBox_labelDefFilename.Text, Encoding.UTF8))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                dom.WriteTo(w);
            }

            this.labelDefControl1.Changed = false;
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 获取或设置标签定义文件名
        /// </summary>
        public string DefFileName
        {
            get
            {
                return this.textBox_labelDefFilename.Text;
            }
            set
            {
                this.textBox_labelDefFilename.Text = value;
            }
        }

        /// <summary>
        /// 获取或设置样例标签内容
        /// </summary>
        public string SampleLabelText
        {
            get
            {
                return this.labelDefControl1.SampleLabelText;
            }
            set
            {
                this.labelDefControl1.SampleLabelText = value;
            }
        }

        private void button_findLabelDefFilename_Click(object sender, EventArgs e)
        {
            if (this.labelDefControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时不保存就装载新的标签定义文件，现有修改信息将丢失。\r\n\r\n要保存当前修改么? \r\n\r\n(Yes: 保存，然后继续装载; No: 不保存，但继续装载; Cancel: 放弃装载)",
    "LabelDesignForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == DialogResult.Yes)
                {
                    SaveFile();
                }
            }

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的标签定义文件名";
            dlg.FileName = this.textBox_labelDefFilename.Text;
            dlg.Filter = "标签定义文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_labelDefFilename.Text = dlg.FileName;

            LoadLabelDefFile();
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                return this.labelDefControl1.UiState;
            }
            set
            {
                this.labelDefControl1.UiState = value;
            }
        }
    }
}
