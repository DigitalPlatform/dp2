using DigitalPlatform.GUI;
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
    /// 选择列对话框
    /// 从给定的列中，选定一些用于打印输出。可以调整列出现的先后顺序
    /// </summary>
    public partial class SelectColumnDialog : Form
    {
        /// <summary>
        /// 提供列信息的 ListView
        /// </summary>
        public ListView ListView
        {
            get;
            set;
        }

        public SelectColumnDialog()
        {
            InitializeComponent();

        }

        private void SelectColumnDialog_Load(object sender, EventArgs e)
        {
            // 延迟设置
            if (this._numberList != null)
            {
                this.NumberList = this._numberList;
                this._numberList = null;
            }

            listView_columns_SelectedIndexChanged(this, new EventArgs());
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_columns.CheckedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择任何列");
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void toolStripButton_move_up_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<int> indices = null;
            ListViewUtil.MoveItemUpDown(this.listView_columns, true, out indices, out strError);

            // 确保移动后的事项可见
            if (this.listView_columns.SelectedItems.Count > 0)
                this.listView_columns.SelectedItems[0].EnsureVisible();

            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
        }

        private void toolStripButton_move_down_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<int> indices = null;
            ListViewUtil.MoveItemUpDown(this.listView_columns, false, out indices, out strError);

            // 确保移动后的事项可见
            if (this.listView_columns.SelectedItems.Count > 0)
                this.listView_columns.SelectedItems[0].EnsureVisible();

            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
        }

        // 备用的数据
        List<string> _numberList = null;

        // 选择的列号数组
        public List<string> NumberList
        {
            get
            {
                List<string> results = new List<string>();
                foreach(ListViewItem item in this.listView_columns.CheckedItems)
                {
                    results.Add(item.Text);
                }
                return results;
            }
            set
            {
                // 要求 this.ListView 先设置好值。如果没有设置好，则只能存储以后备用
                if (this.ListView == null)
                {
                    this._numberList = value;
                    return;
                }
                if (this.listView_columns.Items.Count == 0)
                    FillList();

                if (value == null)
                    return;

                if (value.Count == 1 && value[0] == "<all>")
                {
                    foreach(ListViewItem item in this.listView_columns.Items)
                    {
                        item.Checked = true;
                    }
                    return;
                }

                // 按照要求的顺序把对象取出来
                List<ListViewItem> headers = new List<ListViewItem>();
                foreach(string s in value)
                {
                    ListViewItem item = FindItem(s);
                    if (item != null)
                    {
                        headers.Add(item);
                        this.listView_columns.Items.Remove(item);
                    }
                }

                // 将余下的事项 Checked 设置为 false
                foreach(ListViewItem item in this.listView_columns.Items)
                {
                    item.Checked = false;
                }

                // 然后插入到 listview 头部
                int i = 0;
                foreach(ListViewItem item in headers)
                {
                    this.listView_columns.Items.Insert(i, item);
                    item.Checked = true;
                    i++;
                }
            }
        }

        ListViewItem FindItem(string strNo)
        {
            foreach(ListViewItem item in this.listView_columns.Items)
            {
                if (item.Text == strNo)
                    return item;
            }
            return null;
        }

        void FillList()
        {
            this.listView_columns.Items.Clear();

            if (this.ListView == null)
                return;

            int i = 0;
            foreach(ColumnHeader header in this.ListView.Columns)
            {
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, (i + 1).ToString());
                ListViewUtil.ChangeItemText(item, 1, header.Text);
                item.Tag = header;
                this.listView_columns.Items.Add(item);
                i++;
            }

        }

        private void listView_columns_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked == true)
            {
                e.Item.Font = new Font(this.Font, FontStyle.Bold);
            }
            else
            {
                e.Item.Font = new Font(this.Font, FontStyle.Regular);
            }
        }

        private void listView_columns_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool bSelected = this.listView_columns.SelectedItems.Count > 0;
            this.toolStripButton_move_up.Enabled = bSelected;
            this.toolStripButton_move_down.Enabled = bSelected;
        }
    }
}
