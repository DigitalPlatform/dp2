using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace dp2Circulation
{
    /// <summary>
    /// 刷新数据库定义的方式和风格
    /// </summary>
    internal partial class RefreshStyleDialog : Form
    {
        public bool AutoRebuildKeysVisible = false;

        public string IncludeFilenames = "";
        public string ExcludeFilenames = "";

        public RefreshStyleDialog()
        {
            InitializeComponent();
        }

        private void RefreshStyleDialog_Load(object sender, EventArgs e)
        {
            if (AutoRebuildKeysVisible == true)
                this.checkBox_autoRebuildKeys.Visible = true;
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
            else
            {
                Debug.Assert(false, "");
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
    }
}