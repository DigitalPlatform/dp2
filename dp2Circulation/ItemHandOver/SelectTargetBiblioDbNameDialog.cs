using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.GUI;

namespace dp2Circulation
{
    // 在对话框打开前，在this.TargetBiblioDbName中可以设置好优先的目标库名
    internal partial class SelectTargetBiblioDbNameDialog : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        public string SourceBiblioDbName = "";

        public SelectTargetBiblioDbNameDialog()
        {
            InitializeComponent();
        }

        private void SelectTargetBiblioDbNameDialog_Load(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = FillDbNames(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_dbName.Text) == true)
            {
                MessageBox.Show(this, "尚未指定目标书目库名");
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

        public string TargetBiblioDbName
        {
            get
            {
                return this.textBox_dbName.Text;
            }
            set
            {
                this.textBox_dbName.Text = value;
            }
        }

        public string MessageText
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        // 只填充和SourceBiblioDbName具有相同syntax的、相同出版物类型的书目库名
        // SourceBiblioDbName自己被排除在外
        int FillDbNames(out string strError)
        {
            strError = "";
            string strSourceSyntax = "";

            bool bSourceIsIssueDb = false;

            if (String.IsNullOrEmpty(this.SourceBiblioDbName) == false)
            {
                strSourceSyntax = Program.MainForm.GetBiblioSyntax(this.SourceBiblioDbName);
                if (strSourceSyntax == null)
                {
                    strError = "源书目库名 '" + this.SourceBiblioDbName + "' 居然不存在";
                    return -1;
                }

                if (String.IsNullOrEmpty(strSourceSyntax) == true)
                    strSourceSyntax = "unimarc";

                string strSourceIssueDbName = Program.MainForm.GetIssueDbName(this.SourceBiblioDbName);
                if (String.IsNullOrEmpty(strSourceIssueDbName) == false)
                    bSourceIsIssueDb = true;
                else
                    bSourceIsIssueDb = false;
            }

            if (Program.MainForm.BiblioDbProperties != null)
            {
                foreach (var prop in Program.MainForm.BiblioDbProperties)
                {
                    // BiblioDbProperty prop = Program.MainForm.BiblioDbProperties[i];

                    // 需要具备实体库
                    if (String.IsNullOrEmpty(prop.ItemDbName) == true)
                        continue;

                    // 排除this.SourcBibliDbName
                    if (prop.DbName == this.SourceBiblioDbName)
                        continue;

                    // 判断syntax
                    if (String.IsNullOrEmpty(strSourceSyntax) == false)
                    {
                        string strTempSyntax = prop.Syntax;
                        if (String.IsNullOrEmpty(strTempSyntax) == true)
                            strTempSyntax = "unimarc";

                        if (prop.Syntax != strSourceSyntax)
                            continue;
                    }

                    // 判断出版类型
                    if (String.IsNullOrEmpty(prop.IssueDbName) == false
                        && bSourceIsIssueDb == false)
                        continue;   // 出版物类型不一致

                    ListViewItem item = new ListViewItem();
                    item.Text = prop.DbName;
                    this.listView_dbNames.Items.Add(item);
                }
            }

            ListViewItem selected_item = ListViewUtil.FindItem(this.listView_dbNames, this.textBox_dbName.Text, 0);
            if (selected_item != null)
                selected_item.Selected = true;

            return 0;
        }

        private void listView_dbNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_dbNames.SelectedItems.Count == 0)
                this.textBox_dbName.Text = "";
            else
                this.textBox_dbName.Text = this.listView_dbNames.SelectedItems[0].Text;
        }

        private void listView_dbNames_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);
        }
    }
}