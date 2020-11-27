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
using DigitalPlatform.RFID;

namespace RfidTool
{
    public partial class ScanDialog : Form
    {
        public ScanDialog()
        {
            InitializeComponent();
        }

        private void textBox_barcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                MessageBox.Show(this, $"输入 '{this.textBox_barcode.Text}'");
                this.textBox_barcode.SelectAll();
                e.Handled = true;
            }
        }

        private void ScanDialog_Load(object sender, EventArgs e)
        {
            // 首次填充标签
            FillAllTags();
        }

        private void ScanDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void ScanDialog_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                DataModel.TagChanged += DataModel_TagChanged;
            else
                DataModel.TagChanged -= DataModel_TagChanged;
        }

        // 读卡器上的标签发生变化
        private void DataModel_TagChanged(object sender, NewTagChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        const int COLUMN_UID = 0;

        // TODO: 注意和 DataModel_TagChanged() 处理互斥
        void FillAllTags()
        {
            this.listView_tags.Items.Clear();
            foreach(var tag in DataModel.TagList.Tags)
            {
                if (tag.OneTag.Protocol == InventoryInfo.ISO14443A)
                    continue;

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_UID, tag.OneTag.UID);
            }
        }
    }
}
