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
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    public partial class BindCardNumberDialog : Form
    {
        public BindCardNumberDialog()
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

        public string Numbers
        {
            get
            {
                List<string> list = new List<string>();
                foreach (string s in this.listBox1.Items)
                {
                    list.Add(s);
                }
                return StringUtil.MakePathList(list);
            }
            set
            {
                this.listBox1.Items.Clear();

                if (string.IsNullOrEmpty(value))
                    return;

                List<string> list = new List<string>();
                list = StringUtil.SplitList(value);
                foreach (string s in list)
                {
                    this.listBox1.Items.Add(s);
                }
            }
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            string result = InputDlg.GetInput(this,
                "添加普通号码",
                "号码:",
                "",
                this.Font);
            if (result == null)
                return;
            // 查重
            if (this.listBox1.Items.IndexOf(result) == -1)
            {
                this.listBox1.Items.Add(result);
                this.listBox1.SelectedItem = result;
            }
            else
                MessageBox.Show(this, $"号码 {result} 已经存在了");
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            List<string> delete_items = new List<string>();
            foreach (string s in this.listBox1.SelectedItems)
            {
                delete_items.Add(s);
            }

            foreach (string s in delete_items)
            {
                this.listBox1.Items.Remove(s);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndices.Count == 0)
                this.button_delete.Enabled = false;
            else
                this.button_delete.Enabled = true;
        }

        private void button_add14443_Click(object sender, EventArgs e)
        {
            RfidToolForm dialog = new RfidToolForm();
            dialog.Text = "选择 ISO14443A 读者卡";
            dialog.OkCancelVisible = true;
            dialog.LayoutVertical = false;
            dialog.AutoCloseDialog = false;
            dialog.ProtocolFilter = InventoryInfo.ISO14443A;

            // dialog.SelectedPII = auto_select_pii;
            // dialog.AutoSelectCondition = "auto_or_blankPII";
            Program.MainForm.AppInfo.LinkFormState(dialog, "select14443TagDialog_formstate");
            dialog.ShowDialog(this);

            if (dialog.DialogResult == DialogResult.Cancel)
                return;

            string result = dialog.SelectedID;
            if (result.StartsWith("uid:"))
                result = result.Substring("uid:".Length);

            // 查重
            if (this.listBox1.Items.IndexOf(result) == -1)
            {
                this.listBox1.Items.Add(result);
                this.listBox1.SelectedItem = result;
            }
            else
                MessageBox.Show(this, $"号码 {result} 已经存在了");
        }
    }
}
