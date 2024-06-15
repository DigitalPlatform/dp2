using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 用于获得一个 MARC 格式转换目标书目库名的对话框
    /// </summary>
    internal partial class GetConvertTargetDbNameDlg : Form
    {
        /// <summary>
        /// 本对话框是否要自动结束?
        /// 当仅有一项合适的事项，并且名字等于this.DbName时，自动结束对话框
        /// </summary>
        public bool AutoFinish = false;

        /*
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;
         * */

        /// <summary>
        /// 是否为期刊模式? 
        /// 如果为true，只列出有下属期库的书目库名；否则只列出没有下属期库的书目库名 
        /// </summary>
        public bool SeriesMode = false; // 2008/12/29

        /// <summary>
        /// MARC 具体格式。"unimarc"和"usmarc"之一
        /// 如果为空，表示对格式无要求；如果不为空，则要求为该格式
        /// </summary>
        public string MarcSyntax = "";  // 

        /// <summary>
        /// 用户最后选定的数据库名
        /// </summary>
        public string DbName = "";

        // 筛选编目规则。
        // 如果为 null，表示不筛选。否则选择的书目库的编目规则必须符合 CatalogingRuleFilter，或者为空(为空表示允许多种编目规则)
        public string CatalogingRuleFilter { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public GetConvertTargetDbNameDlg()
        {
            InitializeComponent();
        }

        private void GetAcceptTargetDbNameDlg_Load(object sender, EventArgs e)
        {
            FillDbNameList();

            if (this.AutoFinish == true)
            {
                if (this.listView_dbnames.SelectedItems.Count == 1
                    && this.listView_dbnames.Items.Count == 1)
                {
                    button_OK_Click(this, null);
                }
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_dbnames.SelectedItems.Count == 0)
            {
                strError = "尚未选定目标数据库名";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.CatalogingRuleFilter) == false)
            {
                var selected_rule = ListViewUtil.GetItemText(this.listView_dbnames.SelectedItems[0], 1);
                if (string.IsNullOrEmpty(selected_rule)
                    || selected_rule == this.CatalogingRuleFilter)
                {

                }
                else
                {
                    strError = $"请选择一个和编目规则 '{this.CatalogingRuleFilter}' 相容的书目库";
                    goto ERROR1;
                }
            }

            this.DbName = this.listView_dbnames.SelectedItems[0].Text;

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

        void FillDbNameList()
        {
            this.listView_dbnames.Items.Clear();

            if (Program.MainForm.BiblioDbProperties != null)
            {
                foreach (var prop in Program.MainForm.BiblioDbProperties)
                {
                    /*
                    if (String.IsNullOrEmpty(prop.ItemDbName) == true)
                        continue;
                    */
                    if (String.IsNullOrEmpty(this.MarcSyntax) == false)
                    {
                        if (prop.Syntax.ToLower() != this.MarcSyntax.ToLower())
                            continue;
                    }

                    // 2008/12/29
                    if (this.SeriesMode == true)
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == true)
                            continue;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == false)
                            continue;
                    }

                    string strDbName = prop.DbName;

                    var cr = StringUtil.GetParameterByPrefix(prop.Role, "cr");

                    // 根据筛选编目规则名字，过滤数据库行
                    if (string.IsNullOrEmpty(this.CatalogingRuleFilter) == false)
                    {
                        if (string.IsNullOrEmpty(cr)
        || cr == this.CatalogingRuleFilter)
                        {

                        }
                        else
                            continue;
                    }



                        ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, 0, strDbName);
                    if (string.IsNullOrEmpty(cr) == false)
                        ListViewUtil.ChangeItemText(item, 1, cr);

                    this.listView_dbnames.Items.Add(item);

                    if (item.Text == this.DbName)
                        item.Selected = true;
                }
            }
        }
    }
}