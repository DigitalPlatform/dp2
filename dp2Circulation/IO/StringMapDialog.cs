using DigitalPlatform.GUI;
using dp2Circulation.IO;
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
    /// 字符串对照表对话框
    /// </summary>
    public partial class StringMapDialog : Form
    {
        public StringMapDialog()
        {
            InitializeComponent();
        }

        private void StringMapDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("新增 (&N)");
            menuItem.Click += new System.EventHandler(this.menu_newItem_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改 (&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyItem_Click);
            if (this.listView1.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView1;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除 [" + this.listView1.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_Click);
            if (this.listView1.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView1, new Point(e.X, e.Y));
        }

        void menu_newItem_Click(object sender, EventArgs e)
        {
            int index = -1;

            if (this.listView1.SelectedIndices.Count > 0)
                index = this.listView1.SelectedIndices[0];

            TwoStringDialog dlg = new TwoStringDialog();
        REDO:
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem dup = ListViewUtil.FindItem(this.listView1, dlg.SourceString, 0);
            if (dup != null)
            {
                MessageBox.Show(this, "源字符串为 '" + dlg.SourceString + "' 的事项在列表中已经存在了，不允许重复。请修改");
                goto REDO;
            }

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, 0, dlg.SourceString);
            ListViewUtil.ChangeItemText(item, 1, dlg.TargetString);

            if (index == -1)
                this.listView1.Items.Add(item);
            else
                this.listView1.Items.Insert(index, item);
            ListViewUtil.ClearSelection(this.listView1);
            ListViewUtil.SelectLine(item, true);
        }

        void menu_modifyItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView1.SelectedItems[0];

            TwoStringDialog dlg = new TwoStringDialog();
            dlg.SourceString = ListViewUtil.GetItemText(item, 0);
            dlg.TargetString = ListViewUtil.GetItemText(item, 1);
        REDO:
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem dup = ListViewUtil.FindItem(this.listView1, dlg.SourceString, 0);
            if (dup != null && dup != item)
            {
                MessageBox.Show(this, "源字符串为 '" + dlg.SourceString + "' 的事项在列表中已经存在了，不允许重复。请修改");
                goto REDO;
            }
            ListViewUtil.ChangeItemText(item, 0, dlg.SourceString);
            ListViewUtil.ChangeItemText(item, 1, dlg.TargetString);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView1);
        }

        // 删除已经选定的行
        void menu_deleteSelected_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的事项。");
                return;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除选定的 " + this.listView1.SelectedItems.Count.ToString() + " 个事项?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            ListViewUtil.DeleteSelectedItems(this.listView1);
        }

        public List<TwoString> StringTable
        {
            get
            {
                List<TwoString> results = new List<TwoString>();
                foreach (ListViewItem item in this.listView1.Items)
                {
                    TwoString pair = new TwoString();
                    pair.Source = ListViewUtil.GetItemText(item, 0);
                    pair.Target = ListViewUtil.GetItemText(item, 1);
                    results.Add(pair);
                }

                return results;
            }
            set
            {
                List<int> indices = new List<int>();
                foreach (int i in this.listView1.SelectedIndices)
                {
                    indices.Add(i);
                }

                this.listView1.Items.Clear();
                if (value != null)
                {
                    foreach (TwoString pair in value)
                    {
                        ListViewItem item = new ListViewItem();
                        ListViewUtil.ChangeItemText(item, 0, pair.Source);
                        ListViewUtil.ChangeItemText(item, 1, pair.Target);
                        this.listView1.Items.Add(item);
                    }

                    foreach (int i in indices)
                    {
                        if (i < this.listView1.Items.Count)
                            this.listView1.Items[i].Selected = true;
                    }
                }
            }
        }

        public ListView ListView
        {
            get
            {
                return this.listView1;
            }
        }
    }

    public class TwoString
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }
}
