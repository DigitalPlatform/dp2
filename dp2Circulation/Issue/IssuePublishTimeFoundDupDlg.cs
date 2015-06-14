using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    // 在编辑期记录、期记到的过程中，发现有重复出版日期的记录，
    // 本对话框用于显示这些期记录
    internal partial class IssuePublishTimeFoundDupDlg : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;
        public string IssueText = "";   // 期的HTML信息
        public string BiblioText = "";  // 种的HTML信息


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

        public IssuePublishTimeFoundDupDlg()
        {
            InitializeComponent();
        }

        private void IssuePublishTimeFoundDupDlg_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.IssueText) == false)
                Global.SetHtmlString(this.webBrowser_issue,
                    this.IssueText,
                    this.MainForm.DataDir,
                    "ossuepublishtimedup_item");

            if (String.IsNullOrEmpty(this.BiblioText) == false)
                Global.SetHtmlString(this.webBrowser_biblio,
                    this.BiblioText,
                    this.MainForm.DataDir,
                    "ossuepublishtimedup_item");

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}