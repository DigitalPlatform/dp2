using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Batch
{
    public partial class RebuildKeysDialog : Form
    {
        public RebuildKeysDialog()
        {
            InitializeComponent();
        }

        public bool WholeMode
        {
            get
            {
                if (this.radioButton_whole.Checked == true)
                    return true;
                return false;
            }
            set
            {
                this.radioButton_whole.Checked = true;
            }
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
    }
}