using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    public partial class OpenBiblioDumpFileDialog : Form
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

                this.TryInvoke(() =>
                {
                    if (value == true)
                    {
                        this.Text = "创建书目转储文件";
                    }
                    else
                    {
                        this.Text = "打开书目转储文件";
                    }
                });
            }
        }

        public OpenBiblioDumpFileDialog()
        {
            InitializeComponent();
        }

        private void OpenBiblioDumpFileDialog_Load(object sender, EventArgs e)
        {
            checkBox_includeObjectFile_CheckedChanged(sender, e);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_biblioDumpFileName.Text) == true)
            {
                strError = "尚未指定书目转储文件名";
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

        private void button_getBiblioDumpFileName_Click(object sender, EventArgs e)
        {
            if (this.CreateMode)
            {
                // 询问文件名
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要创建的书目转储文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = true;
                dlg.FileName = this.textBox_biblioDumpFileName.Text;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "书目转储文件 (*.bdf)|*.bdf|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                this.textBox_biblioDumpFileName.Text = dlg.FileName;
            }
            else
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要打开的书目转储文件名";
                dlg.FileName = this.textBox_biblioDumpFileName.Text;
                dlg.Filter = "书目转储文件 (*.bdf)|*.bdf|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                this.textBox_biblioDumpFileName.Text = dlg.FileName;
            }

            AutoBuildObjectDirectoryName(false);
        }

        void AutoBuildObjectDirectoryName(bool bForce)
        {
            if (string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text)
                || bForce)
            {
                if (string.IsNullOrEmpty(this.textBox_biblioDumpFileName.Text) == false)
                    this.textBox_objectDirectoryName.Text = this.textBox_biblioDumpFileName.Text + ".object";
            }
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

                this.label_objectDirectoryName.Enabled = true;

                AutoBuildObjectDirectoryName(true);
            }
            else
            {
                this.textBox_objectDirectoryName.Enabled = false;
                this.button_getObjectDirectoryName.Enabled = false;

                this.label_objectDirectoryName.Enabled = false;

                this.textBox_objectDirectoryName.Text = "";
            }
        }

        public string FileName
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.textBox_biblioDumpFileName.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_biblioDumpFileName.Text = value;
                });
            }
        }

        public string ObjectDirectoryName
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.textBox_objectDirectoryName.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_biblioDumpFileName.Text = value;
                });
            }
        }

        public bool IncludeEntities
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_includeEntities.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_includeEntities.Checked = value;
                });
            }
        }

        public bool IncludeIssues
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_includeIssues.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_includeIssues.Checked = value;
                });
            }
        }

        public bool IncludeOrders
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_includeOrders.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_includeOrders.Checked = value;
                });
            }
        }

        public bool IncludeComments
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_includeComments.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_includeComments.Checked = value;
                });
            }
        }

        public bool IncludeObjectFile
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_includeObjectFile.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_includeObjectFile.Checked = value;
                });
            }
        }

        // 2024/5/28
        // 备份模式。意思是“是否要从服务器获得没有被过滤的原始记录 XML”
        public bool BackupMode
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_backup.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_backup.Checked = value;
                });
            }
        }

        private void textBox_biblioDumpFileName_TextChanged(object sender, EventArgs e)
        {
            AutoBuildObjectDirectoryName(true);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_biblioDumpFileName);
                controls.Add(this.checkBox_includeEntities);
                controls.Add(this.checkBox_includeOrders);
                controls.Add(this.checkBox_includeIssues);
                controls.Add(this.checkBox_includeComments);
                controls.Add(this.checkBox_includeObjectFile);
                controls.Add(this.textBox_objectDirectoryName);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_biblioDumpFileName);
                controls.Add(this.checkBox_includeEntities);
                controls.Add(this.checkBox_includeOrders);
                controls.Add(this.checkBox_includeIssues);
                controls.Add(this.checkBox_includeComments);
                controls.Add(this.checkBox_includeObjectFile);
                controls.Add(this.textBox_objectDirectoryName);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
