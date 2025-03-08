using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
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
                return this.TryGet(() =>
                {
                    return this.textBox_scriptFileName.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_scriptFileName.Text = value;
                });
            }
        }

        public bool AutoSaveChanges
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_autoSaveChanges.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_autoSaveChanges.Checked = value;
                });
            }
        }

        public bool ForceSave
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_forceSave.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_forceSave.Checked = value;
                });
            }
        }

        // 2025/3/6
        public bool NoOperation
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_noOperation.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_noOperation.Checked = value;
                });
            }
        }

        // 2025/3/7
        public bool DontLogging
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_dontLogging.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_dontLogging.Checked = value;
                });
            }
        }

        // 2025/3/6
        public bool DontTriggerAutoGen
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_dontTriggerAutoGen.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_dontTriggerAutoGen.Checked = value;
                });
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
                controls.Add(this.checkBox_noOperation);
                controls.Add(this.checkBox_dontLogging);
                controls.Add(this.checkBox_dontTriggerAutoGen);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_scriptFileName);
                controls.Add(this.checkBox_autoSaveChanges);
                controls.Add(this.checkBox_forceSave);
                controls.Add(this.checkBox_noOperation);
                controls.Add(this.checkBox_dontLogging);
                controls.Add(this.checkBox_dontTriggerAutoGen);
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_autoSaveChanges_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox1.Visible = this.checkBox_autoSaveChanges.Checked;
            /*
            this.checkBox_forceSave.Visible = this.checkBox_autoSaveChanges.Checked;
            this.checkBox_noOperation.Visible = this.checkBox_autoSaveChanges.Checked;
            this.checkBox_dontTriggerAutoGen.Visible = this.checkBox_autoSaveChanges.Checked;
            */
            // 当 gourpbox 隐藏时，这些参数都应该是 false
            if (this.checkBox_autoSaveChanges.Checked == false)
            {
                this.checkBox_forceSave.Checked = false;
                this.checkBox_noOperation.Checked = false;
                this.checkBox_dontLogging.Checked = false;
                this.checkBox_dontTriggerAutoGen.Checked = false;
            }
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
