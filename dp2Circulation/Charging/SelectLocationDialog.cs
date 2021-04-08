using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class SelectLocationDialog : MyForm
    {
        public string SelectedLocation
        {
            get
            {
                return this.textBox_selectedLocation.Text;
            }
            set
            {
                this.textBox_selectedLocation.Text = value;
            }
        }

        public SelectLocationDialog()
        {
            InitializeComponent();

            this.SuppressSizeSetting = true;  // 不需要基类 MyForm 的尺寸设定功能
        }

        private void SelectLocationDialog_Load(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                this.Invoke((Action)(() =>
                {
                    FillList();
                }));
            });
        }

        void FillList()
        {
            this.listBox1.Items.Clear();

            int nRet = GetLocationList(
    out List<string> list,
    out string strError);
            if (nRet == -1)
            {
                ShowMessage(strError, "red", true);
                return;
            }

            int i = 0;
            foreach (string s in list)
            {
                this.listBox1.Items.Add(s);

                if (this.SelectedLocation == s)
                    this.listBox1.SelectedIndex = i;

                i++;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            /*
            if (this.listBox1.SelectedItem == null)
            {
                MessageBox.Show(this, "请在列表中选择一个馆藏地");
                return;
            }

            this.SelectedLocation = this.listBox1.SelectedItem as string;
            */

            if (string.IsNullOrEmpty(this.SelectedLocation))
            {
                MessageBox.Show(this, "请在列表中选择一个馆藏地");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.textBox_selectedLocation.Text = this.listBox1.SelectedItem as string;
        }

        public string BatchNo
        {
            get
            {
                return this.textBox_batchNo.Text;
            }
            set
            {
                this.textBox_batchNo.Text = value;
            }
        }
    }
}
