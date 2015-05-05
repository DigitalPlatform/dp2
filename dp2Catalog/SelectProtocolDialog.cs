using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Catalog
{
    public partial class SelectProtocolDialog : Form
    {
        public List<String> Protocols = new List<string>();
        public string SelectedProtocol = "";

        public SelectProtocolDialog()
        {
            InitializeComponent();
        }

        private void SelectProtocolDialog_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < this.Protocols.Count; i++)
            {
                string strProtocol = this.Protocols[i];

                ListViewItem item = FindItem(strProtocol);
                if (item != null)
                    item.Tag = 1;
            }

            // 将没有设置标记的事项删除
            for (int i = 0; i < this.listView_protocol.Items.Count; i++)
            {
                ListViewItem item = this.listView_protocol.Items[i];

                if (item.Tag == null)
                {
                    this.listView_protocol.Items.RemoveAt(i);
                    i --;
                }
            }

            // 选择第一项
            if (listView_protocol.Items.Count != 0)
                listView_protocol.Items[0].Selected = true;

        }

        ListViewItem FindItem(string strProtocol)
        {
            for (int i = 0; i < this.listView_protocol.Items.Count; i++)
            {
                ListViewItem item = this.listView_protocol.Items[i];
                if (String.Compare(strProtocol,item.Text,true) == 0)
                    return item;
            }

            return null;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_protocol.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择协议");
                return;
            }

            this.SelectedProtocol = this.listView_protocol.SelectedItems[0].Text.ToLower();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void listView_protocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_protocol.SelectedItems.Count == 0)
                this.button_OK.Enabled = false;
            else
                this.button_OK.Enabled = true;
        }

        private void listView_protocol_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);
        }
    }
}