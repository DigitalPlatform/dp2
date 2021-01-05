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
    public partial class RegisterPalmprintDialog : Form
    {
        public RegisterPalmprintDialog()
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

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
