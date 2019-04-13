using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Z3950.UI
{
    public partial class RecordSyntaxAndEncodingBindingItemDlg : Form
    {
        public RecordSyntaxAndEncodingBindingItemDlg()
        {
            InitializeComponent();
        }

        public string Encoding
        {
            get
            {
                return this.comboBox_encoding.Text;
            }
            set
            {
                this.comboBox_encoding.Text = value;
            }
        }

        public string RecordSyntax
        {
            get
            {
                return this.comboBox_recordSyntax.Text;
            }
            set
            {
                this.comboBox_recordSyntax.Text = value;
            }
        }

        private void RecordSyntaxAndEncodingBindingItemDlg_Load(object sender, EventArgs e)
        {
            ZServerPropertyForm.FillEncodingList(this.comboBox_encoding, true);
            /*
            // 补充MARC-8编码方式
            this.comboBox_encoding.Items.Add("MARC-8");
             * */

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.comboBox_encoding.Text == "")
            {
                MessageBox.Show(this, "尚未选定编码方式");
                return;
            }
            if (this.comboBox_recordSyntax.Text == "")
            {
                MessageBox.Show(this, "尚未选定记录格式");
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
    }
}