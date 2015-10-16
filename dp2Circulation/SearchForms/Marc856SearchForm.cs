using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 856 字段检索窗
    /// </summary>
    public partial class Marc856SearchForm : MyForm
    {
        public Marc856SearchForm()
        {
            InitializeComponent();
        }

        private void Marc856SearchForm_Load(object sender, EventArgs e)
        {
            CreateColumns();
        }

        private void Marc856SearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Marc856SearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 新加入一行
        public ListViewItem AddLine(MarcField field)
        {
            string u = field.select("subfield[@name='u']").FirstContent;
            string x = field.select("subfield[@name='x']").FirstContent;

            string strRights = "";

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_FIELDINDEX, this.listView_records.Items.Count.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_URL, u);
            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, strRights);
            this.listView_records.Items.Add(item);
            return item;
        }

        const int COLUMN_SUMMARY = 1;
        const int COLUMN_FIELDINDEX = 2;
        const int COLUMN_URL = 3;
        const int COLUMN_RIGHTS = 4;

        void CreateColumns()
        {
            string[] titles = new string[] {
                "书目摘要",
                "字段序号",
                "$uURL",
                "权限",
            };
            foreach (string title in titles)
            {
                ColumnHeader header = new ColumnHeader();
                header.Width = 100;
                this.listView_records.Columns.Add(header);
            }
        }

    }
}
