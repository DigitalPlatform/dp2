using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Catalog
{
    public partial class DbSelectListDialog : Form
    {
        public List<string> DbNames = new List<string>();

        public DbSelectListDialog()
        {
            InitializeComponent();
        }

        private void DbSelectListDialog_Load(object sender, EventArgs e)
        {
            FillDbNames();

            checkBox_selectAll_CheckedChanged(null, null);
            listView1_SelectedIndexChanged(null, null);
        }

        void FillDbNames()
        {
            this.listView1.Items.Clear();
            foreach (string strText in this.DbNames)
            {
                ListViewItem item = new ListViewItem();
                item.Text = strText;
                this.listView1.Items.Add(item);
            }
        }

        List<string> BuildDbNames()
        {
            List<string> results = new List<string>();
            foreach (ListViewItem item in this.listView1.Items)
            {
                results.Add(item.Text);
            }

            return results;
        }

        private void button_up_Click(object sender, EventArgs e)
        {
            MoveUpDown(true);
        }

        private void button_down_Click(object sender, EventArgs e)
        {
            MoveUpDown(false);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DbNames = BuildDbNames();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void checkBox_selectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_selectAll.Checked == true)
            {
                this.listView1.Enabled = false;
                this.button_up.Enabled = false;
                this.button_down.Enabled = false;
            }
            else
            {
                this.listView1.Enabled = true;
                this.button_up.Enabled = true;
                this.button_down.Enabled = true;
            }
        }

        public bool SelectAllDb
        {
            get
            {
                return this.checkBox_selectAll.Checked;
            }
            set
            {
                this.checkBox_selectAll.Checked = value;
            }
        }

        void MoveUpDown(bool bUp)
        {
            // 当前已选择的node
            if (this.listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("尚未选定要移动的事项");
                return;
            }

            ListViewItem item = this.listView1.SelectedItems[0];
            int index = this.listView1.SelectedIndices[0];

            if (index == 0 && bUp == true)
                return;
            if (index >= this.listView1.Items.Count - 1
                && bUp == false)
                return;

            this.listView1.Items.Remove(item);

            if (bUp == true)
                index--;
            else
                index++;

            this.listView1.Items.Insert(index, item);
            listView1_SelectedIndexChanged(null, null);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0)
            {
                this.button_up.Enabled = false;
                this.button_down.Enabled = false;
                return;
            }

            this.button_up.Enabled = true;
            this.button_down.Enabled = true;

            int nIndex = this.listView1.SelectedIndices[0];
            if (nIndex == 0)
                this.button_up.Enabled = false;

            if (nIndex >= this.listView1.Items.Count - 1)
                this.button_down.Enabled = false;
        }
    }
}
