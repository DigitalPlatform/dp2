using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2rms
{
    public partial class OverwriteOrAppendBackupFileDlg : Form
    {
        public OverwriteOrAppendBackupFileDlg()
        {
            InitializeComponent();
        }

        private void OverwriteOrAppendBackupFileDlg_Load(object sender, EventArgs e)
        {

        }

        private void button_append_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button_overwrite_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();

        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public bool KeepAppend
        {
            get
            {
                return this.checkBox_keepAppend.Checked;
            }
            set
            {
                this.checkBox_keepAppend.Checked = value;
            }
        }

        public string Message
        {
            set
            {
                this.label_message.Text = value;
            }
        }
    }
}