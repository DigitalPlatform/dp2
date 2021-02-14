using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Inventory
{
    public partial class FloatingErrorDialog : Form
    {
        public FloatingErrorDialog()
        {
            InitializeComponent();
        }

        public string Message
        {
            get
            {
                return this.label_text.Text;
            }
            set
            {
                this.label_text.Text = value;
            }
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
