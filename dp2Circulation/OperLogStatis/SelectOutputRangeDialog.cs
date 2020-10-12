using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.GUI;

namespace dp2Circulation
{
    public partial class SelectOutputRangeDialog : Form
    {
        public SelectOutputRangeDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        List<TransferGroup> GetGroups(IEnumerable<ListViewItem> items)
        {
            List<TransferGroup> results = new List<TransferGroup>();
            foreach (ListViewItem item in items)
            {
                var result = new TransferGroup();
                result.BatchNo = ListViewUtil.GetItemText(item, 0);
                result.TargetLocation = ListViewUtil.GetItemText(item, 1);
                result.Items = item.Tag as List<TransferItem>;
                results.Add(result);
            }

            return results;
        }

        public List<TransferGroup> SelectedGroups
        {
            get
            {
                return GetGroups(this.listView1.CheckedItems.Cast<ListViewItem>());
            }
        }

        public List<TransferGroup> Groups
        {
            get
            {
                return GetGroups(this.listView1.Items.Cast<ListViewItem>());

                /*
                List<TransferGroup> results = new List<TransferGroup>();
                foreach(ListViewItem item in this.listView1.Items)
                {
                    var result = new TransferGroup();
                    result.BatchNo = ListViewUtil.GetItemText(item, 0);
                    result.TargetLocation = ListViewUtil.GetItemText(item, 1);
                    result.Items = item.Tag as List<TransferItem>;
                    results.Add(result);
                }

                return results;
                */
            }
            set
            {
                this.listView1.Items.Clear();
                foreach(var group in value)
                {
                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, 0, group.BatchNo);
                    ListViewUtil.ChangeItemText(item, 1, group.TargetLocation);
                    ListViewUtil.ChangeItemText(item, 2, group.Items.Count.ToString());
                    item.Tag = group.Items;

                    this.listView1.Items.Add(item);
                }
            }
        }
    }

    // 一组 TransferItem
    public class TransferGroup
    {
        public string BatchNo { get; set; }
        public string TargetLocation { get; set; }
        public List<TransferItem> Items { get; set; }
    }

    // 典藏移交动作
    public class TransferItem
    {
        // 册记录路径
        public string RecPath { get; set; }
        // 册条码号
        public string Barcode { get; set; }
        // 批次号
        public string BatchNo { get; set; }
        // 源 location
        public string SourceLocation { get; set; }
        // 目标 location
        public string TargetLocation { get; set; }
    }
}
