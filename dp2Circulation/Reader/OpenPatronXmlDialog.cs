using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 打开读者 XML 文件对话框
    /// </summary>
    public partial class OpenPatronXmlFileDialog : Form
    {
        bool _createMode = false;

        /// <summary>
        /// 是否为创建模式？false 表示打开模式
        /// </summary>
        public bool CreateMode
        {
            get
            {
                return _createMode;
            }
            set
            {
                _createMode = value;
                if (value == true)
                {
                    this.Text = "创建读者 XML 文件";
                }
                else
                {
                    this.Text = "打开读者 XML 文件";
                }
            }
        }

        public OpenPatronXmlFileDialog()
        {
            InitializeComponent();
        }

        private void OpenPatronXmlFileDialog_Load(object sender, EventArgs e)
        {
            checkBox_includeObjectFile_CheckedChanged(sender, e);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_xmlFileName.Text) == true)
            {
                strError = "尚未指定读者 XML 文件名";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text) == true
                && this.checkBox_includeObjectFile.Checked == true)
            {
                strError = "尚未指定对象文件目录";
                goto ERROR1;
            }

            // TODO: CreateMode 为 true 情况下，警告对象文件目录里面已经有了文件

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_getPatronXmlFileName_Click(object sender, EventArgs e)
        {
            if (this.CreateMode)
            {
                // 询问文件名
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要创建的读者 XML 文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = true;
                dlg.FileName = this.textBox_xmlFileName.Text;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "读者 XML 文件 (*.xml)|*.xml|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                this.textBox_xmlFileName.Text = dlg.FileName;
            }
            else
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要打开的读者 XML 文件名";
                dlg.FileName = this.textBox_xmlFileName.Text;
                dlg.Filter = "读者 XML 文件 (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                this.textBox_xmlFileName.Text = dlg.FileName;
            }

            AutoBuildObjectDirectoryName(false);
        }

        void AutoBuildObjectDirectoryName(bool bForce)
        {
            if (string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text)
                || bForce)
            {
                if (string.IsNullOrEmpty(this.textBox_xmlFileName.Text) == false)
                    this.textBox_objectDirectoryName.Text = this.textBox_xmlFileName.Text + ".object";
            }
        }

        // 2021/9/15
        // 根据文件名获得对象目录名。注：对象目录可能并没有输入，本函数是获得理论上的(和文件名配套的)对象目录路径
        public string GetRelatedObjectDirectoryName()
        {
            if (string.IsNullOrEmpty(this.textBox_xmlFileName.Text) == false)
                return this.textBox_xmlFileName.Text + ".object";
            return "";
        }

        private void button_getObjectDirectoryName_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定对象文件所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = this.CreateMode;
            dir_dlg.SelectedPath = this.textBox_objectDirectoryName.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_objectDirectoryName.Text = dir_dlg.SelectedPath;
        }

        private void checkBox_includeObjectFile_CheckedChanged(object sender, EventArgs e)
        {
            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            this.textBox_objectDirectoryName.ReadOnly = !control;

            if (this.checkBox_includeObjectFile.Checked)
            {
                this.textBox_objectDirectoryName.Enabled = true;
                this.button_getObjectDirectoryName.Enabled = true;

                this.checkBox_mimeFileExtension.Enabled = true;
                this.checkBox_usageFileExtension.Enabled = true;

                this.label_objectDirectoryName.Enabled = true;

                AutoBuildObjectDirectoryName(true);
            }
            else
            {
                this.textBox_objectDirectoryName.Enabled = false;
                this.button_getObjectDirectoryName.Enabled = false;

                this.checkBox_mimeFileExtension.Enabled = false;
                this.checkBox_usageFileExtension.Enabled = false;

                this.label_objectDirectoryName.Enabled = false;

                this.textBox_objectDirectoryName.Text = "";
            }
        }

        public string FileName
        {
            get
            {
                return this.textBox_xmlFileName.Text;
            }
            set
            {
                this.textBox_xmlFileName.Text = value;
            }
        }

        public string ObjectDirectoryName
        {
            get
            {
                return this.textBox_objectDirectoryName.Text;
            }
            set
            {
                this.textBox_xmlFileName.Text = value;
            }
        }

        public bool IncludeObjectFile
        {
            get
            {
                return this.checkBox_includeObjectFile.Checked;
            }
            set
            {
                this.checkBox_includeObjectFile.Checked = value;
            }
        }

        private void textBox_biblioDumpFileName_TextChanged(object sender, EventArgs e)
        {
            AutoBuildObjectDirectoryName(true);
        }

        public bool MimeFileExtension
        {
            get
            {
                return this.checkBox_mimeFileExtension.Checked;
            }
        }

        public bool UsageFileExtension
        {
            get
            {
                return this.checkBox_usageFileExtension.Checked;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_xmlFileName);
                controls.Add(this.checkBox_includeObjectFile);
                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.checkBox_mimeFileExtension);
                controls.Add(this.checkBox_usageFileExtension);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_xmlFileName);
                controls.Add(this.checkBox_includeObjectFile);
                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.checkBox_mimeFileExtension);
                controls.Add(this.checkBox_usageFileExtension);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
