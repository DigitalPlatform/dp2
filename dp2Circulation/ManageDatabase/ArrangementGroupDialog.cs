using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace dp2Circulation
{
    internal partial class ArrangementGroupDialog : Form
    {
        public string AllZhongcihaoDatabaseInfoXml = "";// 定义了若干种次号库的XML片段
        public List<string> ExcludingDbNames = new List<string>();   // 要排除的、已经被使用了的种次号库名

        public ArrangementGroupDialog()
        {
            InitializeComponent();
        }

        private void ArrangementGroupDialog_Load(object sender, EventArgs e)
        {

        }

        private void ArrangementGroupDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ArrangementGroupDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_groupName.Text == "")
            {
                strError = "尚未输入排架体系名";
                goto ERROR1;
            }

            if (this.comboBox_classType.Text == "")
            {
                strError = "尚未指定类号类型";
                goto ERROR1;
            }

            if (this.checkedComboBox_qufenhaoType.Text == "")
            {
                strError = "尚未指定区分号类型";
                goto ERROR1;
            }

#if NO
            if (this.comboBox_qufenhaoType.Text != "种次号"
                && this.comboBox_qufenhaoType.Text.ToLower() != "zhongcihao"
                && String.IsNullOrEmpty(this.textBox_zhongcihaoDbName.Text) == false)
            {
                strError = "当区分号类型不是“种次号”时，不能指定种次号库名";
                goto ERROR1;
            }
#endif
            if (StringUtil.IsInList("种次号", this.checkedComboBox_qufenhaoType.Text) == false
                && StringUtil.IsInList("zhongcihao", this.checkedComboBox_qufenhaoType.Text) == false 
                && String.IsNullOrEmpty(this.textBox_zhongcihaoDbName.Text) == false)
            {
                strError = "当区分号类型不是“种次号”时，不能指定种次号库名";
                goto ERROR1;
            }

            // 检查对话框中得到的种次号库，是不是被别处用过的种次号库？
            if (String.IsNullOrEmpty(this.textBox_zhongcihaoDbName.Text) == false
                && this.ExcludingDbNames != null)
            {
                if (this.ExcludingDbNames.IndexOf(this.textBox_zhongcihaoDbName.Text) != -1)
                {
                    strError = "您所指定的种次号库 '" + this.textBox_zhongcihaoDbName.Text + "' 已经被其他排架体系使用过了";
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

#if NO
        private void comboBox_qufenhaoType_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_qufenhaoType.Text == "种次号"
                || this.comboBox_qufenhaoType.Text.ToLower() == "zhongcihao")
            {
                this.textBox_zhongcihaoDbName.Enabled = true;
                this.button_getZhongcihaoDbName.Enabled = true;
            }
            else
            {
                this.textBox_zhongcihaoDbName.Enabled = false;
                this.textBox_zhongcihaoDbName.Text = "";
                this.button_getZhongcihaoDbName.Enabled = false;
            }
        }
#endif

        // 排架体系名
        public string ArrangementName
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

        public string ClassType
        {
            get
            {
                return this.comboBox_classType.Text;
            }
            set
            {
                this.comboBox_classType.Text = value;
            }
        }

        public string QufenhaoType
        {
            get
            {
                return this.checkedComboBox_qufenhaoType.Text;
            }
            set
            {
                this.checkedComboBox_qufenhaoType.Text = value;
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

        // 索取号形态
        public string CallNumberStyle
        {
            get
            {
                return this.comboBox_callNumberType.Text;
            }
            set
            {
                this.comboBox_callNumberType.Text = value;
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

        private void checkedComboBox_qufenhaoType_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_qufenhaoType.Items.Count > 0)
                return;

            string[] values = new string [] {
                "GCAT",
                "种次号",
                "四角号码",
                "石头汤著者号",
                "手动",
                "Cutter-Sanborn Three-Figure",
                "<无>",
            };
            foreach (string s in values)
            {
                this.checkedComboBox_qufenhaoType.Items.Add(s);
            }
        }

        private void checkedComboBox_qufenhaoType_TextChanged(object sender, EventArgs e)
        {
            if (StringUtil.IsInList("种次号", this.checkedComboBox_qufenhaoType.Text) == true
                || StringUtil.IsInList("zhongcihao", this.checkedComboBox_qufenhaoType.Text) == true)
            {
                this.textBox_zhongcihaoDbName.Enabled = true;
                this.button_getZhongcihaoDbName.Enabled = true;
            }
            else
            {
                this.textBox_zhongcihaoDbName.Enabled = false;
                this.textBox_zhongcihaoDbName.Text = "";
                this.button_getZhongcihaoDbName.Enabled = false;
            }
        }
    }
}