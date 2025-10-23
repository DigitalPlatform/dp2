using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 刷新数据库定义的方式和风格
    /// </summary>
    internal partial class RefreshStyleDialog : Form
    {
        public GetTempalteDirsFunc GetTempalteDirs { get; set; }

        public bool AutoRebuildKeysVisible = false;

        public string IncludeFilenames = "";
        public string ExcludeFilenames = "";

        public RefreshStyleDialog()
        {
            InitializeComponent();
        }

        static string[] _single_filenames = new string[] { 
        "dp2circulation_marc_autogen.cs",
        "dp2circulation_marc_verify.cs",
        "dp2circulation_marc_conver.cs",
        };

        private void RefreshStyleDialog_Load(object sender, EventArgs e)
        {
            if (AutoRebuildKeysVisible == true)
                this.checkBox_autoRebuildKeys.Visible = true;

            {
                foreach(var item in _single_filenames)
                {
                    this.checkedComboBox_singleCfgFileName.Items.Add(item);
                }
            }

            {
                var results = GetTempalteDirs?.Invoke();
                if (results != null)
                {
                    this.comboBox_templatesPath.Items.Clear();
                    foreach(var result in results)
                    {
                        this.comboBox_templatesPath.Items.Add(result);
                    }
                }
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.radioButton_all.Checked == true)
            {
                this.IncludeFilenames = "*";
                this.ExcludeFilenames = "";
            }
            else if (this.radioButton_allButTemplateFile.Checked == true)
            {
                this.IncludeFilenames = "*";
                this.ExcludeFilenames = "template";
            }
            else if (this.radioButton_structure.Checked == true)
            {
                this.IncludeFilenames = "keys,browse";
                this.ExcludeFilenames = "";
            }
            else if (this.radioButton_singleCfgFile.Checked == true)
            {
                // 2025/10/23
                var filename = this.checkedComboBox_singleCfgFileName.Text;
                if (string.IsNullOrEmpty(filename) == true)
                {
                    MessageBox.Show(this, "尚未输入要刷新的单个配置文件名");
                    return;
                }
                this.IncludeFilenames = filename;
                this.ExcludeFilenames = "";
            }
            else
            {
                Debug.Assert(false, "");
            }

            if (this.checkBox_recoverState.Checked == true)
            {
                DialogResult result = MessageBox.Show(this,
"只有在进行容错级别的日志恢复操作以前，才有必要把读者库的 keys 配置文件刷新为容错日志恢复所需的状态(这时读者库 keys 中配置了“所借册条码号”检索点)。如果平时这样做，会让流通借还操作的速度减慢。\r\n\r\n您确信要这样做么?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
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

        public bool AutoRebuildKeys
        {
            get
            {
                return this.checkBox_autoRebuildKeys.Checked;
            }
            set
            {
                this.checkBox_autoRebuildKeys.Checked = value;
            }
        }

        bool _recoverStateVisible = false;

        // “将 keys 按日志恢复要求刷新” checkbox 是否为可见状态?
        public bool RecoverStateVisible
        {
            get
            {
                return this._recoverStateVisible;
            }
            set
            {
                this._recoverStateVisible = value;
                this.checkBox_recoverState.Visible = value;
            }
        }

        public bool RecoverState
        {
            get
            {
                return this.checkBox_recoverState.Checked;
            }
            set
            {
                this.checkBox_recoverState.Checked = value;
            }
        }

        public string TemplatesPath
        {
            get
            {
                return this.comboBox_templatesPath.Text;
            }
            set
            {
                this.comboBox_templatesPath.Text = value;
            }
        }

        public bool TemplatesDirVisible
        {
            get
            {
                return this.comboBox_templatesPath.Visible;
            }
            set
            {
                if (value == false)
                    this.comboBox_templatesPath.Text = "";
                this.comboBox_templatesPath.Visible = value;
                this.label_templatePath.Visible = value;
            }
        }

        private void radioButton_singleCfgFile_CheckedChanged(object sender, EventArgs e)
        {
            this.checkedComboBox_singleCfgFileName.Visible = this.radioButton_singleCfgFile.Checked;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.radioButton_all);
                controls.Add(this.radioButton_structure);
                controls.Add(this.radioButton_allButTemplateFile);
                controls.Add(this.radioButton_singleCfgFile);
                controls.Add(this.checkedComboBox_singleCfgFileName);
                controls.Add(this.comboBox_templatesPath);
                controls.Add(this.checkBox_autoRebuildKeys);
                controls.Add(this.checkBox_recoverState);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.radioButton_all);
                controls.Add(this.radioButton_structure);
                controls.Add(this.radioButton_allButTemplateFile);
                controls.Add(this.radioButton_singleCfgFile);
                controls.Add(this.checkedComboBox_singleCfgFileName);
                controls.Add(this.comboBox_templatesPath);
                controls.Add(this.checkBox_autoRebuildKeys);
                controls.Add(this.checkBox_recoverState);
                GuiState.SetUiState(controls, value);
            }
        }

    }

    public delegate string[] GetTempalteDirsFunc();
}