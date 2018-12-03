using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    public partial class RunScriptDialog : Form
    {
        public RunScriptDialog()
        {
            InitializeComponent();
        }

        private void RunScriptDialog_Load(object sender, EventArgs e)
        {
            checkBox_autoSaveChanges_CheckedChanged(sender, e);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string ScriptFileName
        {
            get
            {
                return this.textBox_scriptFileName.Text;
            }
            set
            {
                this.textBox_scriptFileName.Text = value;
            }
        }

        public bool AutoSaveChanges
        {
            get
            {
                return this.checkBox_autoSaveChanges.Checked;
            }
            set
            {
                this.checkBox_autoSaveChanges.Checked = value;
            }
        }

        public bool ForceSave
        {
            get
            {
                return this.checkBox_forceSave.Checked;
            }
            set
            {
                this.checkBox_forceSave.Checked = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_scriptFileName);
                controls.Add(this.checkBox_autoSaveChanges);
                controls.Add(this.checkBox_forceSave);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_scriptFileName);
                controls.Add(this.checkBox_autoSaveChanges);
                controls.Add(this.checkBox_forceSave);
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_autoSaveChanges_CheckedChanged(object sender, EventArgs e)
        {
            this.checkBox_forceSave.Visible = this.checkBox_autoSaveChanges.Checked;
        }

        private void button_getScriptFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 C# 脚本文件";
            dlg.FileName = this.textBox_scriptFileName.Text;
            dlg.Filter = "C# 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_scriptFileName.Text = dlg.FileName;
        }
    }
}
