using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Xml;

namespace DigitalPlatform.DTLP
{
    /// <summary>
    /// 指定已知数据库 的 缺省查重方式的 对话框
    /// </summary>
    public partial class DefaultProjectDialog : Form
    {
        public DupCfgDialog DupCfgDialog = null;
        public XmlDocument dom = null;

        public DefaultProjectDialog()
        {
            InitializeComponent();
        }

        private void DefaultProjectDialog_Load(object sender, EventArgs e)
        {
            // 如果没有给出数据库名，这个域就应当可以编辑
            if (String.IsNullOrEmpty(this.DatabaseName) == true)
            {
                this.textBox_databaseName.ReadOnly = false;
                this.button_findDatabaseName.Enabled = true;
            }
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

        private void button_findProjectName_Click(object sender, EventArgs e)
        {
            GetProjectNameDialog dlg = new GetProjectNameDialog();

            dlg.dom = this.dom;
            dlg.ProjectName = this.textBox_defaultProjectName.Text;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_defaultProjectName.Text = dlg.ProjectName;
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

        public string DefaultProjectName
        {
            get
            {
                return this.textBox_defaultProjectName.Text;
            }
            set
            {
                this.textBox_defaultProjectName.Text = value;
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

            dlg.Text = "请选择数据库";
            dlg.Initial(this.DupCfgDialog.DtlpChannels,
                this.DupCfgDialog.DtlpChannel);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Path = this.textBox_databaseName.Text;
            dlg.EnabledIndices = new int[] { DtlpChannel.TypeStdbase };
            dlg.ShowDialog(this);

            this.textBox_databaseName.Text = dlg.Path;
        }


    }
}