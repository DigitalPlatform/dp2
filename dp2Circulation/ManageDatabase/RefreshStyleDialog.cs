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
    }
}