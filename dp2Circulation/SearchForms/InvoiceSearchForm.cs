using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    public partial class InvoiceSearchForm : MyForm
    {
        // 数据库类型
        public string DbType = "invoice";  // invoice amerce


        public InvoiceSearchForm()
        {
            InitializeComponent();
        }

        private void InvoiceSearchForm_Load(object sender, EventArgs e)
        {
            this.FillFromList();

            string strDefaulFrom = "";
            if (this.DbType == "invoice")
                strDefaulFrom = "发票号";
            else if (this.DbType == "amerce")
                strDefaulFrom = "??";
            else
                throw new Exception("未知的DbType '" + this.DbType + "'");

            this.comboBox_from.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "from",
                strDefaulFrom);

            this.comboBox_matchStyle.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "match_style",
                "精确一致");

            string strWidths = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "record_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_records,
                    strWidths,
                    true);
            }

            comboBox_matchStyle_TextChanged(null, null);

            this.SetWindowTitle();
        }

        private void InvoiceSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InvoiceSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.MainForm.AppInfo.SetString(
    this.DbType + "_search_form",
    "from",
    this.comboBox_from.Text);

            this.MainForm.AppInfo.SetString(
                this.DbType + "_search_form",
                "match_style",
                this.comboBox_matchStyle.Text);

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
            this.MainForm.AppInfo.SetString(
                this.DbType + "_search_form",
                "record_list_column_width",
                strWidths);
        }

        void FillFromList()
        {
            BiblioDbFromInfo[] infos = null;
            if (this.DbType == "invoice")
                infos = this.MainForm.InvoiceDbFromInfos;
            else if (this.DbType == "amerce")
                infos = this.MainForm.AmerceDbFromInfos;
            else
                throw new Exception("未知的DbType '" + this.DbType + "'");

            if (infos != null && infos.Length > 0)
            {
                this.comboBox_from.Items.Clear();
                for (int i = 0; i < infos.Length; i++)
                {
                    string strCaption = infos[i].Caption;
                    this.comboBox_from.Items.Add(strCaption);
                }
            }
        }

        void SetWindowTitle()
        {
             this.Text = this.DbTypeCaption + "查询";
        }

        public string DbTypeCaption
        {
            get
            {
                if (this.DbType == "invoice")
                    return "发票";
                else if (this.DbType == "amerce")
                    return "违约金";
                else
                    throw new Exception("未知的DbType '" + this.DbType + "'");
            }
        }

        private void comboBox_matchStyle_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_matchStyle.Text == "空值")
            {
                this.textBox_queryWord.Text = "";
                this.textBox_queryWord.Enabled = false;
            }
            else
            {
                this.textBox_queryWord.Enabled = true;
            }
        }

    }
}
