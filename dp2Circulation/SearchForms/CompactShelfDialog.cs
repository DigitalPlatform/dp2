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
    public partial class CompactShelfDialog : Form
    {
        public CompactShelfDialog()
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

        public string AccessNo
        {
            get
            {
                return this.textBox_accessNo.Text;
            }
            set
            {
                this.textBox_accessNo.Text = value;
            }
        }

        public string ShelfNo
        {
            get
            {
                return this.textBox_shelfNo.Text;
            }
            set
            {
                this.textBox_shelfNo.Text = value;
            }
        }

        public string NearCode
        {
            get
            {
                return this.textBox_nearCode.Text;
            }
            set
            {
                this.textBox_nearCode.Text = value;
            }
        }
    }
}
