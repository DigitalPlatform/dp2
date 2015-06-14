using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// OPAC普通数据库(相对于虚拟库)
    /// </summary>
    internal partial class OpacNormalDatabaseDialog : Form
    {
        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        public List<string> ExcludingDbNames = null;

        public OpacNormalDatabaseDialog()
        {
            InitializeComponent();
        }

        private void OpacNormalDatabaseDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_findDatabaseName_Click(object sender, EventArgs e)
        {
            GetOpacMemberDatabaseNameDialog dlg = new GetOpacMemberDatabaseNameDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ExcludingDbNames = this.ExcludingDbNames;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.SelectedDatabaseName = this.textBox_databaseName.Text;
            dlg.ManagerForm = this.ManagerForm;
            dlg.AllDatabaseInfoXml = this.ManagerForm.GetAllBiblioDbInfoXml();  //  this.ManagerForm.AllDatabaseInfoXml;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;
            this.textBox_databaseName.Text = dlg.SelectedDatabaseName;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_databaseName.Text == "")
            {
                MessageBox.Show(this, "尚未指定数据库名");
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

        /// <summary>
        /// 数据库名。允许输入多个数据库名，逗号间隔
        /// </summary>
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
    }
}