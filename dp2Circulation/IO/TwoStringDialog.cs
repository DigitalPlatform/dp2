using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation.IO
{
    public partial class TwoStringDialog : Form
    {
        public TwoStringDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string SourceString
        {
            get
            {
                return this.textBox_source.Text;
            }
            set
            {
                this.textBox_source.Text = value;
            }
        }

        public string TargetString
        {
            get
            {
                return this.comboBox_target.Text;
            }
            set
            {
                this.comboBox_target.Text = value;
            }
        }
    }
}
