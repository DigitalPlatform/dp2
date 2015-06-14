using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 指定已知数据库 的 缺省查重方式的 对话框
    /// </summary>
    internal partial class DefaultProjectDialog : Form
    {
        // public DupCfgDialog DupCfgDialog = null;
        public List<string> BiblioDbNames = null;

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
                this.comboBox_databaseName.Enabled = true;
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
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Dom = this.dom;
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
                return this.comboBox_databaseName.Text;
            }
            set
            {
                this.comboBox_databaseName.Text = value;
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

        /*
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
         * */

        private void comboBox_databaseName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_databaseName.Items.Count == 0
    && this.BiblioDbNames != null)
            {
                for (int i = 0; i < this.BiblioDbNames.Count; i++)
                {
                    string strDbName = this.BiblioDbNames[i];
                    this.comboBox_databaseName.Items.Add(strDbName);
                }
            }

        }


    }
}