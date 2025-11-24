using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.dp2.Statis
{
    /// <summary>
    /// 用于配置 排序列定义 的对话框
    /// </summary>
    public partial class SortColumnDialog : Form
    {
        // 排序列定义字符串。也就是用来创建SortColumnCollection的字符串
        public string DefString = "";

        List<string> ColumnNames = new List<string>();

        SortColumnCollection columns = null;

        public SortColumnDialog()
        {
            InitializeComponent();
        }

        private void SortColumnDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = FillList(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        private void SortColumnDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        string GetColumnName(int index)
        {
            index++;
            if (index < this.ColumnNames.Count)
                return this.ColumnNames[index];

            return "第" + (index + 1).ToString() + "列";
        }

        public int Initial(string strDefString,
            string strColumnNames)
        {
            this.DefString = strDefString;

            this.ColumnNames.Clear();

            string[] names = strColumnNames.Split(new char[] {','});
            for (int i = 0; i < names.Length; i++)
            {
                this.ColumnNames.Add(names[i]);
            }

            return 0;
        }

        int FillList(out string strError)
        {
            strError = "";

            this.listView_columns.Items.Clear();

            if (String.IsNullOrEmpty(this.DefString) == true)
                return 0;

            this.columns = new SortColumnCollection();

            this.columns.Build(this.DefString);

            for (int i = 0; i < this.columns.Count; i++)
            {
                SortColumn column = this.columns[i];

                string strColumnName = GetColumnName(column.nColumnNumber);

                ListViewItem item = new ListViewItem((i + 1).ToString(), 0);
                item.SubItems.Add(strColumnName);
                item.SubItems.Add(column.bAsc == true ? "升" : "降");
                item.SubItems.Add(column.dataType.ToString());
                item.SubItems.Add(column.bIgnorCase == true ? "是" : "否");
                this.listView_columns.Items.Add(item);
            }

            return 0;
        }
    }
}