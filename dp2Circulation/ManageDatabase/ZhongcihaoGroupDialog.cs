using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    internal partial class ZhongcihaoGroupDialog : Form
    {
        public string AllZhongcihaoDatabaseInfoXml = "";// 定义了若干种次号库的XML片段
        public List<string> ExcludingDbNames = new List<string>();   // 要排除的、已经被使用了的种次号库名

        public ZhongcihaoGroupDialog()
        {
            InitializeComponent();
        }

        private void ZhongcihaoGroupDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_groupName.Text == "")
            {
                strError = "尚未输入组名";
                goto ERROR1;
            }

            if (this.textBox_zhongcihaoDbName.Text == "")
            {
                strError = "尚未输入种次号库名";
                goto ERROR1;
            }

            // 检查对话框中得到的种次号库，是不是被别处用过的种次号库？
            if (this.ExcludingDbNames != null)
            {
                if (this.ExcludingDbNames.IndexOf(this.textBox_zhongcihaoDbName.Text) != -1)
                {
                    strError = "您所指定的种次号库 '" + this.textBox_zhongcihaoDbName.Text + "' 已经被其他组使用过了";
                    goto ERROR1;
                }
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_getZhongcihaoDbName_Click(object sender, EventArgs e)
        {
            GetOpacMemberDatabaseNameDialog dlg = new GetOpacMemberDatabaseNameDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            // dlg.Text = "";
            dlg.AllDatabaseInfoXml = this.AllZhongcihaoDatabaseInfoXml;    // 定义了若干种次号库的XML片段
            dlg.ExcludingDbNames = this.ExcludingDbNames;   // 要排除的、已经被使用了的种次号库名
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_zhongcihaoDbName.Text = dlg.SelectedDatabaseName;
        }

        // 组名
        public string GroupName
        {
            get
            {
                return this.textBox_groupName.Text;
            }
            set
            {
                this.textBox_groupName.Text = value;
            }
        }

        // 种次号库名
        public string ZhongcihaoDbName
        {
            get
            {
                return this.textBox_zhongcihaoDbName.Text;
            }
            set
            {
                this.textBox_zhongcihaoDbName.Text = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }
    }
}