using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZkFingerprint
{
    public partial class OptionDialog : Form
    {
        public OptionDialog()
        {
            InitializeComponent();
        }

        public bool DisplayFingerprintImage
        {
            get
            {
                return this.checkBox_displayFingerprintImage.Checked;
            }
            set
            {
                this.checkBox_displayFingerprintImage.Checked = value;
            }
        }

        public int Threshold
        {
            get
            {
                return (int)this.numericUpDown_threshold.Value;
            }
            set
            {
                this.numericUpDown_threshold.Value = value;
            }
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
    }
}
