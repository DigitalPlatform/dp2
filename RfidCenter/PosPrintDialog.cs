using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RfidCenter
{
    public partial class PosPrintDialog : Form
    {
        public PosPrintDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string ExtendStyle
        {
            get
            {
                return this.textBox_style.Text;
            }
            set
            {
                this.textBox_style.Text = value;
            }
        }

        public string ActionString
        {
            get
            {
                return this.comboBox_action.Text;
            }
            set
            {
                this.comboBox_action.Text = value;
            }
        }

        public string PrintText
        {
            get
            {
                return this.textBox_text.Text;
            }
            set
            {
                this.textBox_text.Text = value;
            }
        }
    }
}
