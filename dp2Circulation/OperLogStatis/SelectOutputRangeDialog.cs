using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;

namespace dp2Circulation
{
    public partial class SelectOutputRangeDialog : Form
    {
        public SelectOutputRangeDialog()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView1.Tag = prop;
            // 批次号
            prop.SetSortStyle(0, ColumnSortStyle.LeftAlign);
            // 目标馆藏地
            prop.SetSortStyle(1, ColumnSortStyle.LeftAlign);
            // 册数
            prop.SetSortStyle(2, ColumnSortStyle.RightAlign);

            prop.CompareColumn += new CompareEventHandler(prop_CompareColumn);
        }

        internal static void prop_CompareColumn(object sender, CompareEventArgs e)
        {
            e.Result = string.Compare(e.String1, e.String2);
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView1.Tag;
            prop.ClearCache();
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
                foreach (var group in value)
                {
                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, 0, group.BatchNo);
                    ListViewUtil.ChangeItemText(item, 1, group.TargetLocation);
                    ListViewUtil.ChangeItemText(item, 2, group.Items.Count.ToString());
                    item.Tag = group.Items;

                    item.Checked = true;

                    this.listView1.Items.Add(item);
                }
            }
        }

        private void toolStripButton_selectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                item.Checked = true;
            }
        }

        private void toolStripButton_clearAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                item.Checked = false;
            }
        }

        public bool OutputOneSheet
        {
            get
            {
                return this.toolStripButton_outputOneSheet.Checked;
            }
            set
            {
                this.toolStripButton_outputOneSheet.Checked = value;
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var item = e.Item;
            if (item.Checked)
            {
                item.ForeColor = Color.White;
                item.BackColor = Color.DarkGreen;
            }
            else
            {
                item.ForeColor = SystemColors.ControlText;
                item.BackColor = SystemColors.Control;
            }

            EnableOKButton();
        }

        void EnableOKButton()
        {
            this.button_OK.Enabled = this.listView1.CheckedItems.Count != 0;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView1);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView1);
                GuiState.SetUiState(controls, value);
            }
        }

        private void SelectOutputRangeDialog_Load(object sender, EventArgs e)
        {
            EnableOKButton();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView1, e);
        }
    }

    // 一组 TransferItem
    public class TransferGroup
    {
        public string BatchNo { get; set; }
        public string TargetLocation { get; set; }
        public List<TransferItem> Items { get; set; }
    }

    // 典藏移交动作。全部是从 setEntity - transfer 日志记录中获取信息
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

        public string SourceCurrentLocation { get; set; }
        public string TargetCurrentLocation { get; set; }

        public string SourceShelfNo { get; set; }
        public string TargetShelfNo { get; set; }

        public DateTime OperTime { get; set; }
        public string Operator { get; set; }

        // 更新后的册记录 XML
        public string NewXml { get; set; }

        // 日志记录中 style 元素
        public string Style { get; set; }
    }
}
