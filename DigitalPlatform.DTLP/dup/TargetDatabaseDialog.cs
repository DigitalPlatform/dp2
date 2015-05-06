using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace DigitalPlatform.DTLP
{
    /// <summary>
    /// 目标库事项 对话框
    /// </summary>
    public partial class TargetDatabaseDialog : Form
    {
        public DupCfgDialog DupCfgDialog = null;

        public TargetDatabaseDialog()
        {
            InitializeComponent();
        }

        private void TargetDatabaseDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_databaseName.Text == "")
            {
                MessageBox.Show(this, "尚未输入数据库名");
                return;
            }

            if (this.textBox_threshold.Text == "")
            {
                MessageBox.Show(this, "尚未输入阈值");
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

        public string DatabaseName
        {
            get
            {
                return this.textBox_databaseName.Text;
            }
            set
            {
                this.textBox_databaseName.Text = value;
            }
        }

        public string Threshold
        {
            get
            {
                return this.textBox_threshold.Text;
            }
            set
            {
                this.textBox_threshold.Text = value;
            }
        }

        private void button_findDatabaseName_Click(object sender, EventArgs e)
        {
            // 需要有DTLP资源对话框。需要有DtlpChannels的支持
            if (this.DupCfgDialog == null)
            {
                MessageBox.Show(this, "DupCfgDialog成员为空，无法打开选择目标数据库的对话框");
                return;
            }

            GetDtlpResDialog dlg = new GetDtlpResDialog();

            dlg.Text = "请选择目标数据库";
            dlg.Initial(this.DupCfgDialog.DtlpChannels,
                this.DupCfgDialog.DtlpChannel);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Path = this.textBox_databaseName.Text;
            dlg.EnabledIndices = new int[] { DtlpChannel.TypeStdbase };
            dlg.ShowDialog(this);

            this.textBox_databaseName.Text = dlg.Path;
        }

        private void textBox_threshold_Validating(object sender,
            CancelEventArgs e)
        {
            if (StringUtil.IsPureNumber(this.textBox_threshold.Text) == false)
            {
                MessageBox.Show(this, "阈值必须为纯数字");
                e.Cancel = true;
            }
        }
    }
}